using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using Engine;
using Xunit;
using static Engine.E3D;

namespace VoxelSandbox.Tests;

/// <summary>
/// Resource packs made to break the game, as a pack from the internet may be, each opened by the
/// test game or read by the packs, which leave out what they cannot read and draw on.
/// </summary>
[Collection(nameof(HeadlessGames))]
public sealed class HostilePackTests : IDisposable
{
    private readonly string _folder = Directory.CreateTempSubdirectory("voxel-hostile-packs-").FullName;

    public void Dispose() => Directory.Delete(_folder, true);

    // A picture of one color, sixteen pixels square.
    private static byte[] Png(Color color) =>
        ExportImageToMemory(new Image([.. Enumerable.Range(0, 256).SelectMany(_ => new[] { color.R, color.G, color.B, color.A })], 16, 16), ".png");

    // A zip pack of files by their paths under assets/minecraft.
    private string Zip(string name, params (string Path, byte[] Bytes)[] files)
    {
        var path = Path.Combine(_folder, name + ".zip");
        using var zip = ZipFile.Open(path, ZipArchiveMode.Create);
        foreach (var (file, bytes) in files)
        {
            using var stream = zip.CreateEntry("assets/minecraft/" + file, CompressionLevel.SmallestSize).Open();
            stream.Write(bytes);
        }
        return path;
    }

    // A folder pack of files by their paths under assets/minecraft.
    private string Folder(string name, params (string Path, byte[] Bytes)[] files)
    {
        var root = Path.Combine(_folder, name);
        foreach (var (file, bytes) in files)
        {
            var path = Path.Combine(root, "assets", "minecraft", file);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
        }
        return root;
    }

    private static readonly Color Red = new(200, 30, 30, 255), Blue = new(30, 30, 200, 255);

    [Fact]
    public void A_Pack_Entry_That_Does_Not_Inflate_Is_Left_Out_And_The_Game_Opens_Saying_So()
    {
        var pack = Zip("cut", ("textures/block/stone.png", [.. Enumerable.Range(0, 4096).Select(i => (byte)(i * 7 % 251))]), ("textures/block/dirt.png", Png(Red)));
        // The stone's compressed bytes turned over past their first few, so they do not inflate.
        var bytes = File.ReadAllBytes(pack);
        var name = Encoding.ASCII.GetBytes("assets/minecraft/textures/block/stone.png");
        var at = bytes.AsSpan().IndexOf(name);
        var data = at + name.Length + BitConverter.ToUInt16(bytes, at - 2);
        for (int i = data + 4; i < data + 64; i++) bytes[i] ^= 0x5a;
        File.WriteAllBytes(pack, bytes);

        using var headless = new HeadlessGame(packs: [pack]);
        var game = headless.Game;

        Assert.Equal(["cut"], game.Packs.Names);
        Assert.Contains("A file of the pack cut could not be read", game.FileFaults);
        Assert.Equal(Red.R, game.Renderer.Atlas.Picture(Surfaces.Dirt)!.Value.Data[0]);
    }

    [Fact]
    public void A_Pack_Entry_Inflating_Past_64_MB_Is_Left_Out_Before_It_Is_Read()
    {
        // 2.1 GB of zeros as the stone's picture, a few megabytes zipped.
        var pack = Path.Combine(_folder, "vast.zip");
        using (var zip = ZipFile.Open(pack, ZipArchiveMode.Create))
        {
            using var stream = zip.CreateEntry("assets/minecraft/textures/block/stone.png", CompressionLevel.Fastest).Open();
            var zeros = new byte[1 << 24];
            for (int i = 0; i < 128; i++) stream.Write(zeros);
        }

        using var headless = new HeadlessGame(packs: [pack]);

        Assert.Equal(["vast"], headless.Game.Packs.Names);
        Assert.Contains("A file of the pack vast could not be read", headless.Game.FileFaults);
    }

    [Fact]
    public void A_Folder_Packs_Picture_That_Is_A_Link_To_A_Device_Is_Left_Out_Before_It_Is_Opened()
    {
        if (OperatingSystem.IsWindows()) return;
        var pack = Folder("device", ("textures/block/dirt.png", Png(Red)));
        File.CreateSymbolicLink(Path.Combine(pack, "assets", "minecraft", "textures", "block", "stone.png"), "/dev/zero");

        using var headless = new HeadlessGame(packs: [pack]);

        Assert.Contains("A file of the pack device could not be read", headless.Game.FileFaults);
        Assert.Equal(Red.R, headless.Game.Renderer.Atlas.Picture(Surfaces.Dirt)!.Value.Data[0]);
    }

    [Fact]
    public void A_Folder_Packs_Picture_That_Is_A_Pipe_Is_Left_Out_Before_It_Is_Opened()
    {
        if (OperatingSystem.IsWindows()) return;
        var pack = Folder("pipe", ("textures/block/dirt.png", Png(Red)));
        using (var made = Process.Start("mkfifo", Path.Combine(pack, "assets", "minecraft", "textures", "block", "stone.png"))) made.WaitForExit();
        using var packs = new ResourcePacks([pack]);

        // Read on a thread of its own, which the process does not wait for, so a read that waits on
        // the pipe fails the test rather than holds it.
        Image? stone = null;
        var reader = new Thread(() => stone = packs.Texture("block/stone")) { IsBackground = true };
        reader.Start();

        Assert.True(reader.Join(TimeSpan.FromSeconds(10)), "the pipe's read gave no answer within 10 s");
        Assert.Null(stone);
        Assert.Contains(packs.Faults, f => f.Name == "pipe");
    }

    [Fact]
    public void A_Connected_Rule_Naming_Tiles_To_The_Ints_Top_Or_Past_The_Atlas_Is_Read_At_Once()
    {
        string[] texts =
        [
            "method=random\ntiles=2147483600-2147483647\nmatchBlocks=stone\n",
            "method=random\nmatchBlocks=stone\ntiles=" + string.Concat(Enumerable.Repeat("0-255 ", 400_000)) + "\n",
        ];
        foreach (var text in texts)
        {
            // Read on a thread of its own, so a list read without end fails the test rather than holds it.
            ConnectedRule? rule = null;
            var reader = new Thread(() => rule = ConnectedTextures.Parse("optifine/ctm/stone/stone.properties", text)) { IsBackground = true };
            reader.Start();

            Assert.True(reader.Join(TimeSpan.FromSeconds(10)), $"the rule of {text.Length} characters gave no answer within 10 s");
            Assert.InRange(rule!.Tiles.Length, 1, TerrainAtlas.MostTiles + 256);
        }
    }

    [Fact]
    public void Random_Tiles_Whose_Weights_Pass_The_Ints_Top_Together_Choose_One_Of_Them()
    {
        int[] weights = [int.MaxValue, int.MaxValue];
        for (int x = 0; x < 64; x++)
            Assert.InRange(ConnectedTextures.RandomTile(2, x, 3, x * 7, 2, weights), 0, 1);
    }

    [Fact]
    public void A_Packs_Entries_Named_Out_Of_The_Packs_Folder_Write_Nothing_Outside_It()
    {
        var packs = Path.Combine(_folder, "packs");
        Directory.CreateDirectory(packs);
        var pack = Path.Combine(packs, "climb.zip");
        using (var zip = ZipFile.Open(pack, ZipArchiveMode.Create))
        {
            string[] names =
            [
                "assets/minecraft/textures/block/stone.png",
                "assets/minecraft/../../../climbed.png",
                "assets/minecraft/textures/block/../../../../../../climbed-far.png",
                "/assets/minecraft/textures/block/dirt.png",
                "assets/minecraft/textures\\block\\..\\..\\..\\..\\climbed-back.png",
                "../assets/minecraft/textures/block/sand.png",
            ];
            foreach (var name in names)
            {
                using var stream = zip.CreateEntry(name).Open();
                stream.Write(Png(Blue));
            }
        }

        using (var headless = new HeadlessGame(packs: [pack]))
        {
            headless.Step(5);
            headless.Game.ReloadPacks();
            headless.Step(5);
            Assert.Equal(["climb"], headless.Game.Packs.Names);
        }

        Assert.Equal([pack], Directory.EnumerateFileSystemEntries(_folder, "*", SearchOption.AllDirectories).Where(File.Exists));
        Assert.False(File.Exists(Path.GetFullPath(Path.Combine(packs, "..", "..", "climbed.png"))));
    }

    [Fact]
    public void A_Reload_Failing_At_A_Piece_Draws_On_With_The_Packs_Before_And_Holds_None_Of_The_New()
    {
        var before = Zip("before", ("textures/block/stone.png", Png(Red)));
        var next = Zip("next", ("textures/block/stone.png", Png(Blue)));
        using var headless = new HeadlessGame(packs: [before]);
        var game = headless.Game;
        // A fault no bound of the reads holds, at a piece between the terrain and the mobs, thrown
        // so many times.
        var faults = 0;
        game.PackStep = piece =>
        {
            if (piece == "sky" && faults-- > 0) throw new InvalidOperationException("a fault of the test's");
        };

        faults = 1;
        game.ChoosePacks([next]);
        headless.Step(5);

        Assert.Equal(["before"], game.Packs.Names);
        Assert.Equal(Red.R, game.Renderer.Atlas.Picture(Surfaces.Stone)!.Value.Data[0]);
        Assert.Single(game.FileFaults, f => f.Contains("next"));
        // Opened to be written alone, which a handle the packs kept would refuse.
        using (new FileStream(next, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }

        // A fault the packs before meet too leaves the game's own art, and neither pack open.
        faults = 2;
        game.ChoosePacks([next]);
        headless.Step(5);

        Assert.Empty(game.Packs.Names);
        Assert.Null(game.Renderer.Atlas.Picture(Surfaces.Stone));
        using (new FileStream(next, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
        using (new FileStream(before, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }

        faults = 0;
        game.ChoosePacks([next]);
        Assert.Equal(["next"], game.Packs.Names);
    }

    // A PNG of one gray, so many pixels across and down, its rows compressed as they are made, so a
    // vast one costs its compression alone.
    private static byte[] WidePng(int width, int height)
    {
        static void Chunk(Stream to, string kind, ReadOnlySpan<byte> data)
        {
            Span<byte> length = stackalloc byte[4];
            System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
            to.Write(length);
            var named = new byte[4 + data.Length];
            Encoding.ASCII.GetBytes(kind).CopyTo(named, 0);
            data.CopyTo(named.AsSpan(4));
            to.Write(named);
            // The decoder reads past a chunk's check without it, so it is left at zero.
            to.Write(new byte[4]);
        }
        using var png = new MemoryStream();
        png.Write([0x89, (byte)'P', (byte)'N', (byte)'G', 13, 10, 26, 10]);
        var header = new byte[13];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(header, width);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height);
        (header[8], header[9]) = (8, 6);
        Chunk(png, "IHDR", header);
        using var packed = new MemoryStream();
        using (var z = new ZLibStream(packed, CompressionLevel.Fastest, leaveOpen: true))
        {
            var row = new byte[1 + width * 4];
            for (int i = 1; i < row.Length; i++) row[i] = 128;
            for (int y = 0; y < height; y++) z.Write(row);
        }
        Chunk(png, "IDAT", packed.ToArray());
        Chunk(png, "IEND", []);
        return png.ToArray();
    }

    [Fact]
    public void A_Picture_Past_4096_Wide_Or_4096_By_16384_Pixels_Is_Left_Out_Before_It_Is_Decoded()
    {
        var pack = Zip("vast", ("textures/block/stone.png", WidePng(4097, 1)), ("textures/block/dirt.png", WidePng(4096, 16385)),
            ("textures/block/sand.png", WidePng(512, 16384)));
        using var packs = new ResourcePacks([pack]);

        Assert.Null(packs.Texture("block/stone"));
        Assert.Null(packs.Texture("block/dirt"));
        Assert.Equal(512, packs.Texture("block/sand")?.Width);
        Assert.Equal(2, packs.Faults.Count);
    }

    [Fact]
    public void An_Animations_Frames_Are_Cut_Once_Each_However_Long_Its_List_Plays_Them()
    {
        var strip = WidePng(16, 32);
        var pack = Zip("long", ("textures/block/stone.png", strip),
            ("textures/block/stone.png.mcmeta", Encoding.UTF8.GetBytes($"{{\"animation\":{{\"frames\":[{string.Join(",", Enumerable.Repeat("0,1", 100_000))}]}}}}")));
        using var packs = new ResourcePacks([pack]);

        var (frames, ticks) = packs.Animation("block/stone")!.Value;

        Assert.Equal(2, frames.Select(f => f.Data).Distinct(ReferenceEqualityComparer.Instance).Count());
        Assert.InRange(frames.Length, 2, TerrainAtlas.MostTiles + 1);
        Assert.Equal(frames.Length, ticks.Length);
    }

    [Fact]
    public void A_Model_Past_4096_Boxes_Or_With_A_Number_Past_A_Float_Or_No_Picture_Size_Is_Left_Out()
    {
        const string box = "{\"coordinates\":[-5,7,-8,10,16,8],\"textureOffset\":[28,8]}";
        static byte[] Model(string size, string translate, int boxes) => Encoding.UTF8.GetBytes(
            $"{{\"textureSize\":{size},\"models\":[{{\"part\":\"body\",\"translate\":{translate},\"boxes\":[{string.Join(",", Enumerable.Repeat(box, boxes))}]}}]}}");
        var pig = MobKinds.All.First(k => k.Name == "pig");
        (string Name, byte[] Model)[] refused =
        [
            ("many", Model("[64,32]", "[0,-13,-2]", 4097)),
            ("huge", Model("[64,32]", "[1e39,-13,-2]", 1)),
            ("unsized", Model("[0,0]", "[0,-13,-2]", 1)),
        ];
        foreach (var (name, model) in refused)
        {
            using var packs = new ResourcePacks([Zip(name, ("optifine/cem/pig.jem", model))]);
            Assert.Null(PackModels.Read(packs, pig));
        }

        using var read = new ResourcePacks([Zip("fine", ("optifine/cem/pig.jem", Model("[64,32]", "[0,-13,-2]", 2)))]);
        Assert.NotNull(PackModels.Read(read, pig));
    }

    [Fact]
    public void A_Pack_On_The_List_That_Is_Not_There_Is_Named()
    {
        using var headless = new HeadlessGame(packs: [Path.Combine(_folder, "gone.zip")]);

        Assert.Empty(headless.Game.Packs.Names);
        Assert.Contains("The pack gone.zip could not be read", headless.Game.FileFaults);
    }

    [Fact]
    public void A_Picture_Refused_By_Its_Size_Gives_Way_To_The_Next_Packs_In_The_Same_Call()
    {
        var first = Zip("vast", ("textures/block/stone.png", WidePng(4097, 1)), ("textures/block/dirt.png", WidePng(4097, 1)));
        var second = Zip("fine", ("textures/block/stone.png", WidePng(16, 16)), ("textures/block/dirt.png", WidePng(16, 16)));
        using var packs = new ResourcePacks([first, second]);

        Assert.Equal(16, packs.Texture("block/stone")?.Width);
        Assert.Equal(16, packs.ImageAt("textures/block/dirt.png")?.Width);
        Assert.Equal(2, packs.Faults.Count(f => f.Name == "vast"));
    }

    [Fact]
    public void An_Animation_Of_A_Frame_Held_Long_Builds_The_Atlas_At_Its_Cost()
    {
        // Two frames, one held a tick and one a hundred million.
        var pixels = Enumerable.Range(0, 16 * 32).SelectMany(i => i < 256 ? new[] { Red.R, Red.G, Red.B, Red.A } : new[] { Blue.R, Blue.G, Blue.B, Blue.A }).ToArray();
        var pack = Folder("held", ("textures/block/stone.png", ExportImageToMemory(new Image(pixels, 16, 32), ".png")),
            ("textures/block/stone.png.mcmeta", """{"animation":{"frames":[{"index":0,"time":1},{"index":1,"time":100000000}]}}"""u8.ToArray()));
        using var packs = new ResourcePacks([pack]);

        var before = GC.GetAllocatedBytesForCurrentThread();
        var atlas = TerrainAtlas.Build(packs);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.InRange(allocated, 0, 256L << 20);
        Assert.Empty(atlas.Animations);
        Assert.NotEqual(0, atlas.Tile(Surfaces.Stone));
    }
}
