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
    public void A_Plant_Stands_Off_Its_Cells_Middle_By_Its_Place_As_The_Source_Offsets_It()
    {
        Assert.Equal(System.Numerics.Vector3.Zero, SectionMesher.Offset(PlantOffset.None, 3, 7));
        var seen = new HashSet<System.Numerics.Vector3>();
        for (int z = 0; z < 16; z++)
            for (int x = 0; x < 16; x++)
            {
                var across = SectionMesher.Offset(PlantOffset.Across, x, z);
                var down = SectionMesher.Offset(PlantOffset.AcrossAndDown, x, z);
                // Up to a quarter of a block either way across, the same at a place every time, a
                // flower never sunk and grass sunk up to a fifth.
                Assert.InRange(across.X, -0.25f, 0.25f);
                Assert.InRange(across.Z, -0.25f, 0.25f);
                Assert.Equal(0, across.Y);
                Assert.InRange(down.Y, -0.2f, 0);
                Assert.Equal(across.X, down.X);
                Assert.Equal(across, SectionMesher.Offset(PlantOffset.Across, x, z));
                seen.Add(across);
            }
        Assert.True(seen.Count > 100, $"{seen.Count} offsets among 256 places");
        Assert.Equal(PlantOffset.AcrossAndDown, Blocks.Get(BlockId.ShortGrass).Shape!.Offset);
        Assert.Equal(PlantOffset.Across, Blocks.Get(BlockId.Poppy).Shape!.Offset);
        Assert.Equal(PlantOffset.None, Blocks.Get(BlockId.OakSapling).Shape!.Offset);
    }

    [Fact]
    public void An_Offset_Grass_Keeps_Its_Walls_A_Block_Wide_And_Its_Texture_From_Its_Own_Corners()
    {
        var world = Laid.World(c => c.Set(0, 0, 0, BlockId.Bedrock));
        world.SetBlock(5, 20, 9, BlockId.Grass);
        world.SetBlock(5, 21, 9, BlockId.ShortGrass);
        var mesher = new SectionMesher();
        Assert.True(mesher.Mesh(world, new SectionKey(0, 1, 0)));
        var offset = SectionMesher.Offset(PlantOffset.AcrossAndDown, 5, 9);
        var faces = mesher.Cutout;
        Assert.Equal(16, faces.Count);
        Assert.Equal(5 + offset.X, faces.Positions.Min(p => p.X), 3);
        Assert.Equal(6 + offset.X, faces.Positions.Max(p => p.X), 3);
        Assert.Equal(21 - 16 + offset.Y, faces.Positions.Min(p => p.Y), 3);
        for (int i = 0; i < faces.Count; i++)
        {
            // The corner's place on the texture, across from the lower end and down from the top of
            // the block, read back as the shader reads it.
            var place = SectionMesher.PlaceOf(faces.Packed[2 * i])!.Value / 16;
            var (_, corner, _, _) = faces.Unpack(i);
            Assert.Equal(corner is 1 or 2 ? 1 : 0, place.X);
            Assert.Equal(corner < 2 ? 1 : 1 - 12 / 16f, place.Y, 3);
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
    public void A_Plains_Field_Has_The_Sources_Grass_Its_Tall_Grass_Two_Blocks_High_And_A_Few_Flowers()
    {
        // Seed 1's plains about -288, 64, twelve columns by twelve, the grass tops of the plains alone.
        var land = new Overworld(1);
        BlockId[] flowers = [BlockId.Dandelion, BlockId.Poppy, BlockId.OxeyeDaisy, BlockId.AzureBluet, BlockId.Cornflower, BlockId.RedTulip, BlockId.OrangeTulip, BlockId.WhiteTulip, BlockId.PinkTulip];
        int tops = 0, grass = 0, tall = 0, flowered = 0, halves = 0;
        for (int cz = -2; cz < 10; cz++)
            for (int cx = -24; cx < -12; cx++)
            {
                var column = land.Generate(cx, cz);
                for (int z = 0; z < 16; z++)
                    for (int x = 0; x < 16; x++)
                    {
                        var (height, biome, _, _, _, _) = land.Ground(cx * 16 + x, cz * 16 + z);
                        if (biome != Biomes.Plains || column.Get(x, height, z) != BlockId.Grass) continue;
                        tops++;
                        var above = column.Get(x, height + 1, z);
                        if (above == BlockId.ShortGrass) grass++;
                        else if (above == BlockId.TallGrass)
                        {
                            tall++;
                            if (column.Get(x, height + 2, z) != BlockId.TallGrassTop) halves++;
                        }
                        else if (Array.IndexOf(flowers, above) >= 0) flowered++;
                    }
            }
        Assert.True(tops > 10000, $"{tops} grass tops of the plains");
        // The source's plains hold ten patches of short grass a chunk, five where its noise is low,
        // of 32 tries each, a quarter of which find the ground on level land, so about 26 grass tops
        // in a hundred bear short grass there, 2 tall grass and 1 a flower, and fewer on these
        // plains, where hills turn tries away and the noise is low in places.
        Assert.InRange(grass / (double)tops, 0.16, 0.30);
        Assert.InRange(tall / (double)tops, 0.002, 0.03);
        Assert.InRange(flowered / (double)tops, 0.0005, 0.015);
        // Every tall grass stands on two blocks, its top over its lower half.
        Assert.Equal(0, halves);
    }

    [Fact]
    public void Breaking_Either_Half_Of_Tall_Grass_Takes_Both_And_Its_Top_Is_No_Item_Of_Its_Own()
    {
        var world = Laid.World(c => Laid.Fill(c, 0, 3, BlockId.Dirt));
        foreach (var broken in new[] { 4, 5 })
        {
            world.SetBlock(2, 4, 2, BlockId.TallGrass);
            world.SetBlock(2, 5, 2, BlockId.TallGrassTop);
            world.SetBlock(2, broken, 2, BlockId.Air);
            Assert.Equal(BlockId.Air, world.GetBlock(2, 4, 2));
            Assert.Equal(BlockId.Air, world.GetBlock(2, 5, 2));
        }
        Assert.Equal(BlockId.TallGrass, Blocks.Get(BlockId.TallGrassTop).Item);
        Assert.DoesNotContain(Items.All, i => i.Block == BlockId.TallGrassTop);
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
