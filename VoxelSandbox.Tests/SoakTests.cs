using Engine;
using Xunit;
using static Engine.E3D;

namespace VoxelSandbox.Tests;

/// <summary>A fact run where <c>VOXEL_SOAK=1</c> asks for it, as the long soak is, and skipped with its reason otherwise.</summary>
public sealed class SoakFactAttribute : FactAttribute
{
    public SoakFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("VOXEL_SOAK") != "1") Skip = "the long soak runs where VOXEL_SOAK=1 asks for it";
    }
}

/// <summary>
/// A session flying over the overworld and back, saving, reading the packs again and turning the
/// render distance and the Corner Shade along it, then staying, its counts level after their first
/// rise or following the land loaded.
/// </summary>
[Collection(nameof(HeadlessGames))]
public sealed class SoakTests : IDisposable
{
    private readonly string _folder = Directory.CreateTempSubdirectory("voxel-soak-").FullName;

    public void Dispose() => Directory.Delete(_folder, true);

    [Fact]
    public void A_Short_Session_Holds_Level() => Soak(steps: 40, every: 10, stay: 600);

    [SoakFact]
    public void A_Long_Session_Holds_Level() => Soak(steps: 500, every: 50, stay: 3000);

    // Flies so many steps of four columns east and back, saving every so many steps, reading the
    // packs again every two of those and turning the settings every three, then stays so many
    // frames.
    private void Soak(int steps, int every, int stay)
    {
        // A pack of a font and a texture, read again along the flight.
        var pack = Path.Combine(_folder, "pack");
        var textures = Path.Combine(pack, "assets", "minecraft", "textures");
        Directory.CreateDirectory(Path.Combine(textures, "font"));
        Directory.CreateDirectory(Path.Combine(textures, "block"));
        var sheet = new byte[128 * 128 * 4];
        for (int i = 0; i < 128 * 128; i++)
            if (i % 128 % 8 < 5 && i / 128 % 8 < 7) sheet[i * 4 + 3] = 255;
        File.WriteAllBytes(Path.Combine(textures, "font", "ascii.png"), ExportImageToMemory(new Image(sheet, 128, 128), ".png"));
        File.WriteAllBytes(Path.Combine(textures, "block", "stone.png"), ExportImageToMemory(new Image([.. Enumerable.Range(0, 256).SelectMany(_ => new byte[] { 90, 120, 60, 255 })], 16, 16), ".png"));
        using var headless = new HeadlessGame(save: Path.Combine(_folder, "world"), packs: [pack]);
        var game = headless.Game;
        game.SwitchWorld("overworld", 1, Path.Combine(_folder, "soak"));
        game.RenderDistance = 2;
        var shade = game.Menus.Screens(game).All().SelectMany(s => s.Options).First(o => o.Name == "Corner Shade");
        Go(headless, 0.5);
        Gui.TextWidth("Options");
        var (files, textured) = (OpenFiles(), HeadlessGame.Textures());
        var chestsSeen = 0;
        void Leg(int from, int to, int step)
        {
            for (int i = from; i != to; i += step)
            {
                Go(headless, i * 64 + 0.5);
                chestsSeen = Math.Max(chestsSeen, game.Chests.All.Count);
                if (i % every == 0)
                {
                    game.SaveWorld();
                    // What the mobs and the chests hold is the loaded columns', and the save holds
                    // the regions about the player.
                    Assert.All(game.Chests.All.Keys, at => Assert.True(game.World.HasColumn(VoxelWorld.ColumnOf(at.X), VoxelWorld.ColumnOf(at.Z)), $"a chest at {at} of a column not loaded"));
                    Assert.Equal(game.World.ColumnCount, game.Mobs.Columns().Count());
                    Assert.InRange(game.Save!.MobRegions, 1, 4);
                    Assert.InRange(game.Save!.ChestRegions, 1, 4);
                }
                if (i % (2 * every) == 0)
                {
                    game.ReloadPacks();
                    Gui.TextWidth("Options");
                }
                if (i % (3 * every) == 0)
                {
                    game.RenderDistance = game.RenderDistance == 2 ? 3 : 2;
                    shade.Change(shade.Value == 50 ? 75 : 50);
                    shade.Settled?.Invoke();
                }
            }
        }
        Leg(1, steps + 1, 1);
        // The heap's first rise is done by the far end, its meshes and its land as many as they
        // come to, and the way back and the stay add none past that.
        var heap = Heap();
        Leg(steps - 1, -1, -1);
        game.RenderDistance = 2;
        Go(headless, 0.5);
        for (int f = 0; f < stay; f++) headless.Step(1);
        game.SaveWorld();

        // The flight met chests to keep, and the stay back where it began holds what it began with.
        Assert.True(chestsSeen > 0, "the flight met no chest");
        Assert.All(game.Chests.All.Keys, at => Assert.True(game.World.HasColumn(VoxelWorld.ColumnOf(at.X), VoxelWorld.ColumnOf(at.Z))));
        Assert.InRange(HeadlessGame.Textures(), textured - 2, textured + 2);
        Assert.InRange(OpenFiles(), 0, files + 4);
        Assert.InRange(Heap(), 0, heap + 32);
    }

    // Puts the player at a place on the land's east-west line as voxel.tp does and steps the game
    // until it stands.
    private static void Go(HeadlessGame headless, double x)
    {
        var game = headless.Game;
        var to = new LandPlace(x, 120, 0.5);
        game.Leave(to);
        game.Recenter(to);
        game.Player.Teleport(LandOrigin.PlaceOf(to));
        headless.Step(2);
        var clock = System.Diagnostics.Stopwatch.StartNew();
        while ((game.Loading || game.Streamer.Pending > 0 || game.Renderer.Meshing > 0) && clock.Elapsed.TotalSeconds < 30)
        {
            headless.Step(1);
            Thread.Sleep(1);
        }
    }

    private static long Heap()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        return GC.GetTotalMemory(true) >> 20;
    }

    // The files the process holds open, where the system lists them.
    private static int OpenFiles() => Directory.Exists("/proc/self/fd") ? Directory.GetFiles("/proc/self/fd").Length : 0;
}
