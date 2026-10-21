using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

public class PlantAndOreTests
{
    private static readonly Item StonePickaxe = Items.Find("stone_pickaxe")!, IronPickaxe = Items.Find("iron_pickaxe")!;

    [Fact]
    public void Every_Surface_Of_The_New_Blocks_Names_A_Pack_Texture()
    {
        var added = Blocks.All.Where(b => b.Id >= BlockId.JungleLog);
        var surfaces = added.SelectMany(b => b.Shape is { } shape ? shape.Pieces.Select(p => p.Surface) : [b.Top, b.Side, b.Bottom]).Distinct();
        Assert.All(surfaces, s => Assert.NotNull(PackTextures.Of(s)));
    }

    [Fact]
    public void A_Flower_Is_Two_Flat_Walls_Crossed_Corner_To_Corner_Each_Drawn_From_Both_Sides_And_Cut_Out()
    {
        var world = Laid.World(c => c.Set(0, 0, 0, BlockId.Bedrock));
        world.SetBlock(8, 20, 8, BlockId.Grass);
        world.SetBlock(8, 21, 8, BlockId.Poppy);
        var mesher = new SectionMesher();
        Assert.True(mesher.Mesh(world, new SectionKey(0, 1, 0)));

        // The stem's two walls and the petals' two, a quad on each side of each, and none of the
        // poppy's faces among the grass block's.
        var faces = mesher.Cutout;
        Assert.Equal(8 * 4, faces.Count);
        Assert.Equal(24, mesher.Solid.Count);
        for (int quad = 0; quad < 8; quad++)
        {
            var corners = Enumerable.Range(quad * 4, 4).Select(i => faces.Positions[i]).ToArray();
            // Each wall runs from one corner of the cell to the other, flat, with no thickness.
            Assert.Equal(1f, MathF.Abs(corners.Max(c => c.X) - corners.Min(c => c.X)), 3);
            Assert.Equal(1f, MathF.Abs(corners.Max(c => c.Z) - corners.Min(c => c.Z)), 3);
            var normal = System.Numerics.Vector3.Cross(corners[1] - corners[0], corners[2] - corners[0]);
            Assert.Equal(0, System.Numerics.Vector3.Dot(normal, corners[3] - corners[0]), 3);
            Assert.Contains(faces.Unpack(quad * 4).Face, new[] { 6, 7 });
        }
    }

    [Fact]
    public void A_Plant_No_Pack_Has_Takes_The_Games_Own_Mask_Tinted_By_Its_Color()
    {
        var atlas = TerrainAtlas.None;
        Assert.NotEqual(0, atlas.Tile(Surfaces.ShortGrass));
        var green = new Color(90, 180, 60);
        var (_, tint, _) = atlas.Paint(Surfaces.ShortGrass, green, green);
        Assert.Equal(Surfaces.All[Surfaces.ShortGrass].Under(green, green), tint);
        // Clear between its blades and its color on them.
        var texels = Enumerable.Range(0, 16 * 16).Select(i => atlas.Texel(Surfaces.ShortGrass, (i % 16 + 0.5f) / 16, (i / 16 + 0.5f) / 16, green, green)!.Value).ToList();
        Assert.Contains(texels, t => t.A == 0);
        Assert.Contains(texels, t => t.A == 255 && t.G == tint.G);
        Assert.Equal(0, atlas.Tile(Surfaces.Stone));
    }

    [Fact]
    public void Leaves_Drop_Their_Sapling_One_Time_In_Twenty()
    {
        var random = new Random(3);
        var saplings = Enumerable.Range(0, 4000).Count(_ => Loot.Of(BlockId.JungleLeaves, random).Any(d => d.Item == Items.Of(BlockId.JungleSapling)));
        Assert.InRange(saplings, 150, 250);
    }

    [Fact]
    public void A_Diamond_Asks_An_Iron_Pickaxe_And_Each_Wood_Makes_Its_Planks()
    {
        Assert.Empty(Loot.Of(BlockId.DiamondOre, new Random(1), StonePickaxe));
        Assert.Equal((Items.Diamond, 1), Loot.Of(BlockId.DiamondOre, new Random(1), IronPickaxe).Single());
        Assert.Equal(Items.Of(BlockId.CopperOre), Loot.Of(BlockId.CopperOre, new Random(1), StonePickaxe).Single().Item);
        Assert.Equal(0, Loot.Hardness(BlockId.Poppy));

        foreach (var (log, planks) in new[] { (BlockId.JungleLog, BlockId.JunglePlanks), (BlockId.AcaciaLog, BlockId.AcaciaPlanks), (BlockId.DarkOakLog, BlockId.DarkOakPlanks), (BlockId.CherryLog, BlockId.CherryPlanks) })
        {
            ItemStack?[] grid = [new ItemStack(Items.Of(log), 1), null, null, null];
            Assert.Equal(Items.Of(planks), Recipes.Match(grid, 2)!.Result);
        }
        Assert.Equal(Items.GoldIngot, Smelting.Result(Items.Of(BlockId.GoldOre)));
    }

    [Fact]
    public void A_Meadow_Grows_Grass_And_Flowers_And_Diamonds_Lie_Deep()
    {
        var land = new Overworld(1);
        int grass = 0, flowers = 0, diamonds = 0, highestDiamond = 0;
        BlockId[] meadow = [BlockId.Dandelion, BlockId.Poppy, BlockId.OxeyeDaisy, BlockId.AzureBluet, BlockId.Cornflower, BlockId.RedTulip, BlockId.OrangeTulip, BlockId.WhiteTulip, BlockId.PinkTulip];
        for (int cx = -6; cx < 6; cx++)
            for (int cz = -6; cz < 6; cz++)
            {
                var column = land.Generate(cx, cz);
                for (int y = 1; y < ChunkColumn.Height; y++)
                    for (int z = 0; z < 16; z++)
                        for (int x = 0; x < 16; x++)
                        {
                            var block = column.Get(x, y, z);
                            if (block == BlockId.ShortGrass) grass++;
                            else if (Array.IndexOf(meadow, block) >= 0) flowers++;
                            else if (block == BlockId.DiamondOre) (diamonds, highestDiamond) = (diamonds + 1, Math.Max(highestDiamond, y));
                        }
            }
        Assert.True(grass > 500, $"{grass} short grass");
        Assert.True(flowers > 10, $"{flowers} flowers");
        Assert.True(diamonds > 0, "no diamond ore");
        Assert.InRange(highestDiamond, 5, 20);
    }
}
