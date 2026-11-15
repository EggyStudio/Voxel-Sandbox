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
}
