using Engine;
using Xunit;
using static Engine.E3D;

namespace VoxelSandbox.Tests;

/// <summary>A pack's connected textures of OptiFine's form, read, chosen by a face's neighbors or its place, and meshed.</summary>
public sealed class ConnectedTextureTests : IDisposable
{
    private readonly string _folder = Directory.CreateTempSubdirectory("voxel-ctm-").FullName;

    public void Dispose() => Directory.Delete(_folder, true);

    [Fact]
    public void Properties_Read_To_Their_Method_Their_Tiles_And_What_They_Take()
    {
        var full = ConnectedTextures.Parse("optifine/ctm/glass/glass.properties", "# glass\nmethod=ctm\ntiles=0-46\nmatchBlocks=minecraft:glass\n")!;
        Assert.Equal(ConnectMethod.Full, full.Method);
        Assert.Equal(47, full.Tiles.Length);
        Assert.Equal("optifine/ctm/glass/0.png", full.Tiles[0]);
        Assert.Equal("optifine/ctm/glass/46.png", full.Tiles[46]);
        Assert.Equal(["glass"], full.Blocks);
        Assert.False(full.ByTile);
        Assert.Equal(63, full.Faces);

        var shelf = ConnectedTextures.Parse("optifine/ctm/bookshelf/shelf.properties", "method=horizontal\ntiles=a b ./c <default>\nmatchTiles=bookshelf\nfaces=sides\n")!;
        Assert.Equal(ConnectMethod.Horizontal, shelf.Method);
        Assert.Equal(["optifine/ctm/bookshelf/a.png", "optifine/ctm/bookshelf/b.png", "optifine/ctm/bookshelf/c.png"], shelf.Tiles.Take(3).OfType<string>());
        Assert.Null(shelf.Tiles[3]);
        Assert.Equal(["block/bookshelf"], shelf.Textures);
        Assert.True(shelf.ByTile);
        Assert.Equal(1 | 2 | 16 | 32, shelf.Faces);

        // A rule naming neither blocks nor textures takes the texture its file is named for.
        var sandstone = ConnectedTextures.Parse("optifine/ctm/sandstone/sandstone.properties", "method=repeat\ntiles=0-3\nwidth=2\nheight=2\n")!;
        Assert.Equal(["block/sandstone"], sandstone.Textures);
        Assert.Equal((2, 2), (sandstone.Width, sandstone.Height));
        var random = ConnectedTextures.Parse("optifine/ctm/stone/stone.properties", "method=random\ntiles=0-2\nweights=1 0 3\nmatchBlocks=stone\n")!;
        Assert.Equal([1, 0, 3], random.Weights);
        // The methods not read, and a rule with no tiles, are left out.
        Assert.Null(ConnectedTextures.Parse("optifine/ctm/a/a.properties", "method=overlay\ntiles=0-16\n"));
        Assert.Null(ConnectedTextures.Parse("optifine/ctm/a/a.properties", "method=ctm\n"));
    }

    [Fact]
    public void The_Full_Methods_Tile_Follows_The_Template_For_Each_Set_Of_Neighbors()
    {
        int Tile(bool l = false, bool d = false, bool r = false, bool u = false, bool dl = false, bool dr = false, bool ur = false, bool ul = false) =>
            ConnectedTextures.FullTile(l, d, r, u, dl, dr, ur, ul);
        Assert.Equal(0, Tile());
        Assert.Equal(3, Tile(l: true));
        Assert.Equal(1, Tile(r: true));
        Assert.Equal(2, Tile(l: true, r: true));
        Assert.Equal(12, Tile(d: true));
        Assert.Equal(36, Tile(u: true));
        Assert.Equal(24, Tile(u: true, d: true));
        // Every side and every corner the middle of a wall, every side and no corner its four inner corners.
        Assert.Equal(26, Tile(true, true, true, true, true, true, true, true));
        Assert.Equal(46, Tile(true, true, true, true));
        // A wall's lower left corner, joined on its right, above and between them.
        Assert.Equal(37, Tile(r: true, u: true, ur: true));
        // A corner beside a side that does not join counts for nothing.
        Assert.Equal(3, Tile(l: true, dl: true, ul: true));

        Assert.Equal(3, ConnectedTextures.HorizontalTile(false, false));
        Assert.Equal(2, ConnectedTextures.HorizontalTile(true, false));
        Assert.Equal(0, ConnectedTextures.HorizontalTile(false, true));
        Assert.Equal(1, ConnectedTextures.HorizontalTile(true, true));
        Assert.Equal(2, ConnectedTextures.VerticalTile(true, false));
    }

    [Fact]
    public void A_Random_Tile_Stays_By_Its_Place_And_Its_Weights_And_A_Repeat_Lays_Its_Sheet_Across()
    {
        int[] weights = [1, 0, 3];
        var picked = Enumerable.Range(0, 400).Select(i => ConnectedTextures.RandomTile(4, i, 5, i * 7, 3, weights)).ToList();
        // The same place the same tile, and a tile weighed nothing never.
        Assert.Equal(picked, Enumerable.Range(0, 400).Select(i => ConnectedTextures.RandomTile(4, i, 5, i * 7, 3, weights)));
        Assert.DoesNotContain(1, picked);
        Assert.InRange(picked.Count(t => t == 2), 230, 370);
        Assert.Contains(0, picked);
        Assert.Equal(3, Enumerable.Range(0, 300).Select(i => ConnectedTextures.RandomTile(2, i, 0, 0, 3, [])).Distinct().Count());

        // A sheet of two by two on the south faces, across with x and down with the height falling.
        Assert.Equal([0, 1, 0, 1], Enumerable.Range(0, 4).Select(x => ConnectedTextures.RepeatTile(4, x, 0, 0, 2, 2)));
        Assert.Equal(2, ConnectedTextures.RepeatTile(4, 0, -1, 0, 2, 2));
        Assert.Equal(0, ConnectedTextures.RepeatTile(4, 0, -2, 0, 2, 2));
    }

    [Fact]
    public void A_Wall_Of_Glass_Takes_The_Packs_Tiles_By_Its_Neighbors_And_A_Block_Without_A_Pack_Keeps_Its_Own()
    {
        // A pack of the glass's picture and its 47 tiles, each a colour of its own.
        var root = Path.Combine(_folder, "pack");
        void Write(string path, Image image)
        {
            var file = Path.Combine(root, "assets", "minecraft", path);
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            File.WriteAllBytes(file, ExportImageToMemory(image, ".png"));
        }
        Image Solid(int n) => new([.. Enumerable.Range(0, 256).SelectMany(_ => new byte[] { (byte)(n * 5), 100, 200, 255 })], 16, 16);
        // The glass's own picture clear in its middle, as a pack's glass is, so its faces are cut out.
        Write("textures/block/glass.png", new([.. Enumerable.Range(0, 256).SelectMany(i => i % 16 is >= 4 and < 12 && i / 16 is >= 4 and < 12 ? new byte[4] : [200, 200, 200, 255])], 16, 16));
        for (int n = 0; n < 47; n++) Write($"optifine/ctm/glass/{n}.png", Solid(n));
        File.WriteAllText(Path.Combine(root, "assets", "minecraft", "optifine", "ctm", "glass", "glass.properties"), "method=ctm\ntiles=0-46\nmatchBlocks=glass\n");
        using var packs = new ResourcePacks([root]);
        var atlas = TerrainAtlas.Build(packs);
        var (rule, numbers) = atlas.Connected(Surfaces.Glass)!.Value;
        Assert.Equal(ConnectMethod.Full, rule.Method);
        Assert.Null(TerrainAtlas.None.Connected(Surfaces.Glass));

        // A wall of glass three by three across x and y, its faces toward +z read by their lowest corner.
        var world = Laid.World(c => c.Set(0, 0, 0, BlockId.Bedrock));
        for (int y = 20; y <= 22; y++)
            for (int x = 5; x <= 7; x++)
                world.SetBlock(x, y, 5, BlockId.Glass);
        var mesher = new SectionMesher { Atlas = atlas };
        Assert.True(mesher.Mesh(world, new SectionKey(0, 1, 0)));
        var faces = mesher.CutoutGlass;
        int FrontTile(int x, int y) => Enumerable.Range(0, faces.Count / 4)
            .Where(q => faces.Unpack(4 * q).Face == 4
                && Enumerable.Range(4 * q, 4).Min(i => faces.Positions[i].X) == x && Enumerable.Range(4 * q, 4).Min(i => faces.Positions[i].Y) == y - 16)
            .Select(q => (int)(SectionMesher.TilesOf(faces.Packed[8 * q]) & 511))
            .Single();
        Assert.Equal(numbers[26], FrontTile(6, 21));
        Assert.Equal(numbers[37], FrontTile(5, 20));
        Assert.Equal(numbers[ConnectedTextures.FullTile(true, true, false, false, true, false, false, false)], FrontTile(7, 22));
    }
}
