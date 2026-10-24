using Engine;
using Xunit;
using static Engine.E3D;

namespace VoxelSandbox.Tests;

/// <summary>A pack's clear pixels cut out of a block's faces, and a lamp's faces each its own.</summary>
public sealed class CutoutTests : IDisposable
{
    private readonly string _folder = Directory.CreateTempSubdirectory("voxel-cutout-").FullName;

    public void Dispose() => Directory.Delete(_folder, true);

    // A picture sixteen square, clear in its middle eight by eight where a frame holds it, as a pack's glass is.
    private static Image Framed() => new([.. Enumerable.Range(0, 256).SelectMany(i =>
    {
        int x = i % 16, y = i / 16;
        return x is >= 4 and < 12 && y is >= 4 and < 12 ? new byte[] { 0, 0, 0, 0 } : new byte[] { 200, 200, 200, 255 };
    })], 16, 16);

    private static Image Plain(byte alpha) => new([.. Enumerable.Range(0, 256).SelectMany(_ => new byte[] { 120, 160, 200, alpha })], 16, 16);

    // The atlas of a pack of the textures given, as a folder.
    private TerrainAtlas Atlas(params (string Texture, Image Image)[] textures)
    {
        var root = Path.Combine(_folder, Guid.NewGuid().ToString("N"));
        foreach (var (texture, image) in textures)
        {
            var path = Path.Combine(root, "assets", "minecraft", "textures", texture + ".png");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, ExportImageToMemory(image, ".png"));
        }
        using var packs = new ResourcePacks([root]);
        return TerrainAtlas.Build(packs);
    }

    // A floor of bedrock at 0, the rest open sky.
    private static VoxelWorld Floor() => Laid.World(c => c.Set(0, 0, 0, BlockId.Bedrock));

    [Fact]
    public void A_Texture_Clear_In_Some_Pixels_Cuts_Out_And_One_Partly_Clear_Blends()
    {
        var atlas = Atlas(("block/glass", Framed()), ("block/ice", Plain(128)), ("block/stone", Plain(255)), ("block/cactus_side", Framed()));
        Assert.True(atlas.Cuts(Surfaces.Glass));
        Assert.False(atlas.Cuts(Surfaces.Ice));
        Assert.False(atlas.Cuts(Surfaces.Stone));
        // The cactus's texture is clear at its edges for the source's model, which its own shape
        // draws, so no whole block of it is cut out.
        Assert.True(atlas.Cuts(Surfaces.CactusSide));
        Assert.False(atlas.CutOut(BlockId.Cactus));
        // The game's own masks of its plants are cut out by their own meshes.
        Assert.False(TerrainAtlas.None.Cuts(Surfaces.ShortGrass));
    }

    [Fact]
    public void A_Plant_A_Pack_Gives_A_Picture_Wears_It_And_One_It_Does_Not_The_Games_Own_Mask()
    {
        var atlas = Atlas(("block/short_grass", Framed()));
        // The pack's picture, its average its own, and the fern, which the pack leaves out, the game's mask.
        Assert.NotNull(atlas.Average(Surfaces.ShortGrass));
        Assert.True(atlas.Cuts(Surfaces.ShortGrass));
        Assert.Null(atlas.Average(Surfaces.Fern));
        Assert.NotEqual(0, atlas.Tile(Surfaces.Fern));
    }

    [Fact]
    public void Leaves_A_Pack_Cuts_Out_Keep_Their_Faces_Against_Each_Other_And_Hide_Nothing()
    {
        var world = Floor();
        world.SetBlock(5, 20, 5, BlockId.OakLeaves);
        world.SetBlock(6, 20, 5, BlockId.OakLeaves);
        world.SetBlock(5, 19, 5, BlockId.Stone);
        var mesher = new SectionMesher { Atlas = Atlas(("block/oak_leaves", Framed())) };
        Assert.True(mesher.Atlas.CutOut(BlockId.OakLeaves));
        Assert.False(mesher.Atlas.Hides(BlockId.OakLeaves));
        Assert.True(mesher.Mesh(world, new SectionKey(0, 1, 0)));
        // Six faces a leaf, the two between them among them, but the first's bottom, which the stone
        // under it hides, cut out and casting a shadow, and the stone's top under the leaf among its
        // solid faces.
        Assert.Equal(11 * 4, mesher.SolidCutout.Count);
        Assert.Contains(Enumerable.Range(0, mesher.Solid.Count), i => mesher.Solid.Unpack(i).Face == 2 && mesher.Solid.Positions[i].Y == 4);

        // With no pack the leaves are whole and hide the faces beside them.
        mesher.Atlas = TerrainAtlas.None;
        Assert.True(mesher.Mesh(world, new SectionKey(0, 1, 0)));
        Assert.Equal(0, mesher.SolidCutout.Count);
        Assert.DoesNotContain(Enumerable.Range(0, mesher.Solid.Count), i => mesher.Solid.Unpack(i).Face == 2 && mesher.Solid.Positions[i].Y == 4);
    }

    [Fact]
    public void Glass_Keeps_Its_Frame_And_Loses_The_Streaks_Across_Its_Pane()
    {
        // A frame a pixel wide about a clear pane with a streak across it, as Bare Bones draws its glass.
        var streaked = new Image([.. Enumerable.Range(0, 256).SelectMany(i =>
        {
            int x = i % 16, y = i / 16;
            return x is 0 or 15 || y is 0 or 15 || (x == y && x is >= 2 and <= 4) ? new byte[] { 220, 230, 235, 255 } : new byte[] { 0, 0, 0, 0 };
        })], 16, 16);
        var picture = Atlas(("block/glass", streaked)).Picture(Surfaces.Glass)!.Value;
        byte Alpha(int x, int y) => picture.Data[(y * picture.Width + x) * 4 + 3];
        Assert.Equal(255, Alpha(7, 0));
        Assert.Equal(255, Alpha(15, 9));
        Assert.Equal(0, Alpha(3, 3));
        Assert.Equal(0, Alpha(8, 8));
        // A frame four pixels wide about a clear pane, streaked nowhere, is kept as it is.
        var framed = Atlas(("block/glass", Framed())).Picture(Surfaces.Glass)!.Value;
        Assert.Equal(Framed().Data, framed.Data);
    }

    [Fact]
    public void Two_Panes_Side_By_Side_Join_So_One_Frame_Goes_About_Both()
    {
        var world = Floor();
        world.SetBlock(5, 20, 5, BlockId.Glass);
        world.SetBlock(6, 20, 5, BlockId.Glass);
        // Glass of another kind above the left pane joins neither.
        world.SetBlock(5, 21, 5, BlockId.WhiteStainedGlass);
        var mesher = new SectionMesher { Atlas = Atlas(("block/glass", Framed()), ("block/white_stained_glass", Framed())) };
        Assert.True(mesher.Mesh(world, new SectionKey(0, 1, 0)));
        var faces = mesher.CutoutGlass;
        // A pane's face toward +z, its texture's right toward +x, by its lowest corner in the section.
        uint? FrontJoins(int x, int y) => Enumerable.Range(0, faces.Count / 4)
            .Where(q => faces.Unpack(4 * q).Face == 4
                && Enumerable.Range(4 * q, 4).Min(i => faces.Positions[i].X) == x && Enumerable.Range(4 * q, 4).Min(i => faces.Positions[i].Y) == y)
            .Select(q => SectionMesher.Joins(faces.Packed[8 * q]))
            .Single();
        Assert.Equal(1u, FrontJoins(5, 20 - 16));
        Assert.Equal(2u, FrontJoins(6, 20 - 16));
        Assert.Equal(0u, FrontJoins(5, 21 - 16));
    }

    [Fact]
    public void Glass_A_Pack_Cuts_Out_Casts_No_Shadow_And_Meets_Its_Kind_With_No_Face()
    {
        var world = Floor();
        world.SetBlock(5, 20, 5, BlockId.Glass);
        world.SetBlock(6, 20, 5, BlockId.Glass);
        var mesher = new SectionMesher { Atlas = Atlas(("block/glass", Framed())) };
        Assert.True(mesher.Mesh(world, new SectionKey(0, 1, 0)));
        Assert.Equal(10 * 4, mesher.CutoutGlass.Count);
        Assert.Equal(0, mesher.Cutout.Count);
        Assert.Equal(0, mesher.SeeThrough.Count);
    }

    [Fact]
    public void A_Doors_Window_A_Pack_Cuts_Out_Still_Casts_A_Shadow()
    {
        var world = Floor();
        world.SetBlock(5, 21, 5, BlockId.OakDoorTop);
        var mesher = new SectionMesher { Atlas = Atlas(("block/oak_door_top", Framed())) };
        Assert.True(mesher.Mesh(world, new SectionKey(0, 1, 0)));
        Assert.Equal(6 * 4, mesher.SolidCutout.Count);
        Assert.Equal(0, mesher.Solid.Count);
    }

    [Fact]
    public void The_Eye_Sees_Through_A_Section_Of_Leaves_A_Pack_Cuts_Out()
    {
        var section = new Section();
        for (int y = 0; y < Section.Size; y++)
            for (int z = 0; z < Section.Size; z++)
                for (int x = 0; x < Section.Size; x++) section.Set(x, y, z, BlockId.OakLeaves);
        Assert.Equal(0UL, Visibility.Joins(section));
        Assert.Equal((1UL << 36) - 1, Visibility.Joins(section, Atlas(("block/oak_leaves", Framed()))));
        Assert.Equal(0UL, Visibility.Joins(section, TerrainAtlas.None));
    }

    [Fact]
    public void A_Lit_Furnace_Shows_The_Furnaces_Top_Above_And_Glows_At_Its_Front_Alone()
    {
        var furnace = Blocks.Get(BlockId.FurnaceLit);
        Assert.Equal((Surfaces.FurnaceTop, Surfaces.FurnaceLit, Surfaces.FurnaceTop), (furnace.Top, furnace.Side, furnace.Bottom));
        Assert.Equal(Surfaces.FurnaceLit, Assert.Single(furnace.Glows).Surface);

        // With no pack, its faces' own colors side by side, and its glow its front's alone.
        var atlas = TerrainAtlas.None;
        var lit = Surfaces.All[Surfaces.FurnaceLit];
        var faces = (furnace.Top, furnace.Side, furnace.Bottom);
        var strip = ChunkRenderer.Strip(atlas, faces, lit, glow: false);
        var glow = ChunkRenderer.Strip(atlas, faces, lit, glow: true);
        var size = atlas.Pixels;
        Assert.Equal((3 * size, size), (strip.Width, strip.Height));
        Color At(Image image, int tile) => new(image.Data[tile * size * 4], image.Data[tile * size * 4 + 1], image.Data[tile * size * 4 + 2], image.Data[tile * size * 4 + 3]);
        Assert.Equal(Surfaces.All[Surfaces.FurnaceTop].Color, At(strip, 0));
        Assert.Equal(lit.Color with { A = 255 }, At(strip, 1));
        Assert.Equal(new Color(0, 0, 0, 0), At(glow, 0));
        Assert.Equal(Color.White, At(glow, 1));
    }
}
