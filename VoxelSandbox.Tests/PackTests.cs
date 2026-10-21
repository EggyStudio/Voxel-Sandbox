using System.IO.Compression;
using Engine;
using Xunit;
using static Engine.E3D;

namespace VoxelSandbox.Tests;

// With the icon tests, since a pack's icons are painted into the atlas they read.
[Collection(nameof(Icons))]
public sealed class PackTests : IDisposable
{
    private readonly string _folder = Directory.CreateTempSubdirectory("voxel-packs-").FullName;

    public void Dispose() => Directory.Delete(_folder, true);

    // An image of one color, so many pixels across and down.
    private static Image Plain(Color color, int width = 2, int height = 2) =>
        new([.. Enumerable.Range(0, width * height).SelectMany(_ => new[] { color.R, color.G, color.B, color.A })], width, height);

    // A pack as a folder holding the textures by their names under textures, as Minecraft's packs hold them.
    private string Folder(string name, params (string Texture, Image Image)[] textures)
    {
        var root = Path.Combine(_folder, name);
        foreach (var (texture, image) in textures)
        {
            var path = Path.Combine(root, "assets", "minecraft", "textures", texture + ".png");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, ExportImageToMemory(image, ".png"));
        }
        return root;
    }

    // A texture's metadata beside it in a folder pack.
    private static void Meta(string pack, string texture, string json) =>
        File.WriteAllText(Path.Combine(pack, "assets", "minecraft", "textures", texture + ".png.mcmeta"), json);

    // The same pack as a zip, as a pack is shared.
    private string Zip(string name, params (string Texture, Image Image)[] textures)
    {
        var path = Path.Combine(_folder, name + ".zip");
        using var zip = ZipFile.Open(path, ZipArchiveMode.Create);
        File.WriteAllText(Path.Combine(_folder, "pack.mcmeta"), "{}");
        zip.CreateEntryFromFile(Path.Combine(_folder, "pack.mcmeta"), "pack.mcmeta");
        foreach (var (texture, image) in textures)
        {
            using var stream = zip.CreateEntry($"assets/minecraft/textures/{texture}.png").Open();
            stream.Write(ExportImageToMemory(image, ".png"));
        }
        return path;
    }

    private static Color At(Image image, int x, int y)
    {
        var at = (y * image.Width + x) * 4;
        return new Color(image.Data[at], image.Data[at + 1], image.Data[at + 2], image.Data[at + 3]);
    }

    [Fact]
    public void The_First_Pack_Is_Read_Over_The_Rest_And_A_Texture_It_Lacks_From_The_Next_That_Has_It()
    {
        var first = Folder("first", ("block/stone", Plain(new Color(200, 0, 0))));
        var second = Zip("second", ("block/stone", Plain(new Color(0, 0, 200))), ("block/dirt", Plain(new Color(0, 200, 0))));
        using var packs = new ResourcePacks([first, Path.Combine(_folder, "missing.zip"), second]);

        Assert.Equal(new[] { "first", "second" }, packs.Names);
        Assert.Equal(new Color(200, 0, 0), At(packs.Texture("block/stone")!.Value, 0, 0));
        Assert.Equal(new Color(0, 200, 0), At(packs.Texture("block/dirt")!.Value, 1, 1));
        Assert.Null(packs.Texture("block/sand"));
    }

    [Fact]
    public void An_Animated_Texture_Gives_Its_First_Frame_And_A_Tall_One_Its_Metadata_Does_Not_Animate_Stays_Whole()
    {
        // Two frames of two pixels square, red over blue, as Minecraft strips a texture's frames, and
        // the same strip as a slider's handle, taller than it is wide.
        var strip = new Image([.. Plain(new Color(200, 0, 0)).Data, .. Plain(new Color(0, 0, 200)).Data], 2, 4);
        var pack = Folder("animated", ("block/water_still", strip), ("gui/sprites/widget/slider_handle", strip));
        Meta(pack, "block/water_still", """{ "animation": { "frametime": 2 } }""");
        using var packs = new ResourcePacks([pack]);

        var frame = packs.Texture("block/water_still")!.Value;
        Assert.Equal((2, 2), (frame.Width, frame.Height));
        Assert.Equal(new Color(200, 0, 0), At(frame, 1, 1));
        Assert.Equal((2, 4), (packs.Texture("gui/sprites/widget/slider_handle")!.Value.Width, packs.Texture("gui/sprites/widget/slider_handle")!.Value.Height));
    }

    [Fact]
    public void A_Sprites_Nine_Slice_Is_Read_From_Its_Metadata_With_One_Border_Or_Four()
    {
        var pack = Folder("sliced", ("gui/sprites/widget/button", Plain(Color.White, 200, 20)), ("gui/sprites/widget/slider_handle", Plain(Color.White, 8, 20)),
            ("gui/sprites/hud/hotbar", Plain(Color.White, 182, 22)));
        Meta(pack, "gui/sprites/widget/button", """{ "gui": { "scaling": { "type": "nine_slice", "width": 200, "height": 20, "border": 3 } } }""");
        Meta(pack, "gui/sprites/widget/slider_handle",
            """{ "gui": { "scaling": { "type": "nine_slice", "width": 8, "height": 20, "border": { "left": 2, "top": 2, "right": 2, "bottom": 3 } } } }""");
        using var packs = new ResourcePacks([pack]);

        Assert.Equal((200, 20, 3, 3, 3, 3), PackGui.Scaling(packs.Picture("gui/sprites/widget/button")!.Value.Meta));
        Assert.Equal((8, 20, 2, 2, 2, 3), PackGui.Scaling(packs.Picture("gui/sprites/widget/slider_handle")!.Value.Meta));
        Assert.Equal((0, 0, 0, 0, 0, 0), PackGui.Scaling(packs.Picture("gui/sprites/hud/hotbar")!.Value.Meta));
    }

    [Fact]
    public void A_Pack_Fonts_Glyph_Is_As_Wide_As_Its_Lit_Columns_And_A_Pixels_Gap_At_Any_Size()
    {
        // A sheet of eight pixels a glyph with A lit to its fifth column, and the same at twice the
        // size lit to its tenth, both five of the interface's pixels and a gap, and a space four.
        foreach (var cell in new[] { 8, 16 })
        {
            var sheet = Plain(new Color(0, 0, 0, 0), 16 * cell, 16 * cell);
            int left = 'A' % 16 * cell, top = 'A' / 16 * cell;
            for (int y = 0; y < cell; y++)
                for (int x = 0; x < 5 * cell / 8; x++)
                    sheet.Data[((top + y) * sheet.Width + left + x) * 4 + 3] = 255;
            var advances = PackFont.Advances(sheet);
            Assert.Equal(6, advances['A']);
            Assert.Equal(4, advances[' ']);
            Assert.Equal(4, advances['B']);
        }
    }

    [Fact]
    public void With_No_Pack_A_Surface_Keeps_Its_Own_Color_And_The_White_Tile()
    {
        var grass = new Color(90, 180, 60);
        Assert.False(TerrainAtlas.None.Any);
        Assert.Equal((0u, Surfaces.All[Surfaces.Stone].Color, Surfaces.All[Surfaces.Stone].Color), TerrainAtlas.None.Paint(Surfaces.Stone, grass, grass));
        var tinted = Surfaces.All[Surfaces.Grass].Under(grass, grass);
        Assert.Equal((0u, tinted, tinted), TerrainAtlas.None.Paint(Surfaces.Grass, grass, grass));
    }

    [Fact]
    public void A_Textured_Surface_Carries_White_Or_The_Climates_Color_And_Gives_The_Field_Its_Average()
    {
        using var packs = new ResourcePacks([Folder("plain", ("block/stone", Plain(new Color(100, 100, 100))), ("block/grass_block_top", Plain(new Color(200, 200, 200))))]);
        var atlas = TerrainAtlas.Build(packs);
        var grass = new Color(120, 200, 80);

        var (tiles, tint, albedo) = atlas.Paint(Surfaces.Stone, grass, grass);
        Assert.Equal((uint)atlas.Tile(Surfaces.Stone), tiles);
        Assert.NotEqual(0u, tiles);
        Assert.Equal(Color.White, tint);
        Assert.Equal(new Color(100, 100, 100), albedo);

        // The grass's gray texture under the climate's color, as Minecraft tints it by its colormap.
        (_, tint, albedo) = atlas.Paint(Surfaces.Grass, grass, grass);
        Assert.Equal(grass, tint);
        Assert.InRange(albedo.G, 150, 165);
        Assert.True(albedo.G > albedo.R && albedo.R > albedo.B);
    }

    [Fact]
    public void The_Grass_Blocks_Side_Lays_Its_Overlay_Tinted_Over_Its_Untinted_Texture()
    {
        var brown = new Color(120, 80, 50);
        // An overlay of white across its top row, clear below, as the grass block's side overlay is.
        var overlay = Plain(new Color(0, 0, 0, 0), 2, 2);
        for (int x = 0; x < 2; x++) (overlay.Data[x * 4], overlay.Data[x * 4 + 1], overlay.Data[x * 4 + 2], overlay.Data[x * 4 + 3]) = (255, 255, 255, 255);
        using var packs = new ResourcePacks([Folder("grass", ("block/grass_block_side", Plain(brown)), ("block/grass_block_side_overlay", overlay))]);
        var atlas = TerrainAtlas.Build(packs);
        var green = new Color(90, 180, 60);

        var (tiles, tint, albedo) = atlas.Paint(Surfaces.GrassSide, green, green);
        Assert.Equal((uint)atlas.Tile(Surfaces.GrassSide) | (uint)atlas.Overlay(Surfaces.GrassSide) << 9 | 1u << 18, tiles);
        Assert.Equal(green, tint);
        Assert.Equal(green, atlas.Texel(Surfaces.GrassSide, 0.1f, 0.1f, green, green));
        Assert.Equal(brown, atlas.Texel(Surfaces.GrassSide, 0.1f, 0.9f, green, green));
        // Half of the face is the grass's and half the dirt's, which the field reads as between them.
        Assert.InRange(albedo.G, brown.G + 1, green.G - 1);
    }

    [Fact]
    public void Every_Tile_Is_Drawn_At_The_Largest_Size_Two_By_Two_In_Its_Cell()
    {
        // A tile of four pixels of four colors, and one of 32, larger than the game's own masks of 16.
        var quarters = new Image([200, 0, 0, 255, 0, 200, 0, 255, 0, 0, 200, 255, 200, 200, 0, 255], 2, 2);
        using var packs = new ResourcePacks([Folder("sizes", ("block/stone", quarters), ("block/dirt", Plain(new Color(9, 9, 9), 32, 32)))]);
        var atlas = TerrainAtlas.Build(packs);
        var image = new Image(atlas.Image(), atlas.Width, atlas.Height);
        const int p = 32;
        Assert.Equal(p, atlas.Pixels);
        Assert.Equal(TerrainAtlas.Columns * 2 * p, atlas.Width);

        int cell = atlas.Tile(Surfaces.Stone) % TerrainAtlas.Columns * 2 * p, row = atlas.Tile(Surfaces.Stone) / TerrainAtlas.Columns * 2 * p;
        // Each pixel of the small tile is a half tile square, and the tile is repeated across and
        // down its cell from a half tile in, its upper left corner at the middle's corner, where the
        // shader reads from, and its lower right at the cell's corner.
        Assert.Equal(new Color(200, 0, 0), At(image, cell + p / 2, row + p / 2));
        Assert.Equal(new Color(200, 0, 0), At(image, cell + p - 1, row + p - 1));
        Assert.Equal(new Color(0, 200, 0), At(image, cell + p, row + p / 2));
        Assert.Equal(new Color(0, 0, 200), At(image, cell + p / 2, row + p));
        Assert.Equal(new Color(200, 200, 0), At(image, cell, row));
        Assert.Equal(At(image, cell + 1, row + p / 2), At(image, cell + p + 1, row + 3 * p / 2));
        Assert.Equal(Color.White, At(image, 3, 3));
    }

    [Fact]
    public void A_Corners_Tiles_Ride_Above_Its_Shade_And_Leave_The_Rest_As_It_Was()
    {
        var (first, second) = SectionMesher.Pack(5, 2, 1, new Color(1, 2, 3, 4), (1u << 19) - 1);
        Assert.Equal((1u << 19) - 1, SectionMesher.TilesOf(first));
        Assert.Equal((5, 2, 1f, new Color(1, 2, 3, 4)), SectionMesher.Unpack(first, second));
    }

    [Fact]
    public void With_A_Pack_An_Item_Is_Its_Texture_And_A_Blocks_Faces_Take_Theirs()
    {
        var stick = Plain(new Color(150, 100, 50), 16, 16);
        using var packs = new ResourcePacks([Folder("icons", ("item/stick", stick), ("block/stone", Plain(new Color(100, 100, 100))))]);
        try
        {
            Icons.Use(TerrainAtlas.Build(packs), packs);
            Assert.Equal(stick.Data, Icons.Pixels(Items.Stick));
            // The stone's top in the middle of its icon is the texture's gray, unshaded as the top is.
            var stone = Icons.Pixels(Items.Of(BlockId.Stone));
            var top = (3 * Icons.Size + 8) * 4;
            Assert.Equal(new byte[] { 100, 100, 100, 255 }, stone[top..(top + 4)]);
        }
        finally
        {
            Icons.Use(TerrainAtlas.None, ResourcePacks.None);
        }
    }

    [Fact]
    public void The_List_Keeps_The_Packs_On_In_Order_Names_In_The_Folder_Or_Whole_Paths()
    {
        var list = new PackList(Path.Combine(_folder, "game"));
        Assert.Empty(list.Read());
        Folder(Path.Combine("game", "resourcepacks", "Zeta"), ("block/stone", Plain(Color.White)));
        File.WriteAllText(Path.Combine(list.Folder, "alpha.zip"), "");
        File.WriteAllText(Path.Combine(list.Folder, "notes.txt"), "");
        Assert.Equal(new[] { "alpha.zip", "Zeta" }, list.Available());

        var elsewhere = Path.Combine(_folder, "elsewhere.zip");
        list.Write(["Zeta", elsewhere]);
        File.AppendAllLines(list.File, ["", "# a note", "  alpha.zip  "]);
        Assert.Equal(new[] { "Zeta", elsewhere, "alpha.zip" }, list.Read());
        Assert.Equal(Path.Combine(list.Folder, "Zeta"), list.PathOf("Zeta"));
        Assert.Equal(elsewhere, list.PathOf(elsewhere));
    }

    [Fact]
    public void A_Packs_Name_Is_Shown_Without_Its_Zip_Its_Folder_And_Its_Color_Codes()
    {
        Assert.Equal("Bare Bones 1.21", PackList.Shown("Bare Bones 1.21.zip"));
        Assert.Equal("Bare Bones x 1.21.2 Redstone Torch", PackList.Shown("/packs/§cBare Bones x 1.21.2 Redstone Torch§r.zip"));
        Assert.Equal("Zeta", PackList.Shown("Zeta/"));
    }

    // A stand-in for the network, answering each request with the bytes a rule gives and counting
    // the requests and the user agents they named.
    private sealed class Served(Func<string, byte[]> bytes) : HttpMessageHandler
    {
        public readonly List<string> Asked = [], Agents = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Asked.Add(request.RequestUri!.ToString());
            Agents.Add(request.Headers.UserAgent.ToString());
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new ByteArrayContent(bytes(request.RequestUri.ToString())) });
        }
    }

    private static DefaultPack Pinned(string file, byte[] bytes) =>
        new(file, file, "project", "version", $"https://cdn.example/{file}", bytes.Length, Convert.ToHexStringLower(System.Security.Cryptography.SHA512.HashData(bytes)));

    [Fact]
    public void The_Default_Packs_Are_Bare_Bones_Recolourful_And_The_Font_Pinned_On_Modrinth_By_Their_Bytes()
    {
        var packs = DefaultPacks.Load().Packs;
        Assert.Equal(new[] { "Bare Bones 1.21.zip", "Recolourful Containers 1.1.2 (1.19.4-1.21.x).zip", "qrafty's-capitalized-font.zip" }, packs.Select(p => p.File));
        Assert.All(packs, p =>
        {
            Assert.StartsWith($"https://cdn.modrinth.com/data/{p.Project}/versions/{p.Version}/", p.Url);
            Assert.Matches("^[0-9a-f]{128}$", p.Sha512);
            Assert.True(p.Size > 0);
        });
        Assert.True(DefaultPacks.Accepts("pack"u8, Pinned("a.zip", "pack"u8.ToArray()).Sha512));
        Assert.False(DefaultPacks.Accepts("park"u8, Pinned("a.zip", "pack"u8.ToArray()).Sha512));
    }

    [Fact]
    public void A_Fetch_Keeps_A_Pack_In_The_Folder_Takes_The_Right_Bytes_And_Refuses_Others()
    {
        var list = new PackList(Path.Combine(_folder, "game"));
        byte[] kept = [1, 2, 3], fetched = [4, 5, 6], refused = [7, 8, 9];
        Directory.CreateDirectory(list.Folder);
        File.WriteAllBytes(list.PathOf("kept.zip"), kept);
        var served = new Served(url => url.EndsWith("refused.zip") ? [0, 0, 0] : fetched);

        var defaults = new DefaultPacks([Pinned("kept.zip", kept), Pinned("fetched.zip", fetched), Pinned("refused.zip", refused)]);
        defaults.Start(list, served);
        defaults.Wait();

        Assert.Equal(FetchState.Failed, defaults.State);
        Assert.Contains("refused.zip", defaults.Problem);
        Assert.Equal(new[] { "https://cdn.example/fetched.zip", "https://cdn.example/refused.zip" }, served.Asked);
        Assert.All(served.Agents, agent => Assert.StartsWith("VoxelSandbox/", agent));
        Assert.Equal(fetched, File.ReadAllBytes(list.PathOf("fetched.zip")));
        Assert.False(File.Exists(list.PathOf("refused.zip")));
        Assert.Empty(Directory.GetFiles(list.Folder, "*.part"));
        Assert.True(defaults.Missing(list));

        // With the right bytes served, the next fetch brings the last one in.
        served = new Served(_ => refused);
        defaults.Start(list, served);
        defaults.Wait();
        Assert.Equal((FetchState.Done, 3), (defaults.State, defaults.Fetched));
        Assert.False(defaults.Missing(list));
    }

    [Fact]
    public void The_Mesher_Writes_Each_Faces_Tile_And_The_Field_Reads_Its_Textures_Average()
    {
        using var packs = new ResourcePacks([Folder("stone", ("block/stone", Plain(new Color(100, 100, 100))))]);
        var world = Laid.World(c => c.Set(0, 0, 0, BlockId.Bedrock));
        world.SetBlock(8, 20, 8, BlockId.Stone);
        var mesher = new SectionMesher { Atlas = TerrainAtlas.Build(packs), LightLevels = false, CornerShade = false };
        Assert.True(mesher.Mesh(world, new SectionKey(0, 1, 0)));

        var faces = mesher.Solid;
        Assert.Equal(24, faces.Count);
        for (int i = 0; i < faces.Count; i++)
        {
            Assert.Equal((uint)mesher.Atlas.Tile(Surfaces.Stone), SectionMesher.TilesOf(faces.Packed[2 * i]));
            Assert.Equal(Color.White, faces.Unpack(i).Tint);
            Assert.Equal(new Color(100, 100, 100), faces.Colors[i]);
        }
    }
}
