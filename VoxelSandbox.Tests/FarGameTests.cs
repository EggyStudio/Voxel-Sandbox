using System.Globalization;
using System.Text.RegularExpressions;
using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>
/// The whole game played far from the land's origin, a million blocks out and at a negative place,
/// through the commands a player's tools send, its places kept against a <see cref="LandOrigin"/>
/// moved near the player.
/// </summary>
[Collection(nameof(HeadlessGames))]
public sealed partial class FarGameTests : IDisposable
{
    // A folder of this test's own for the world's save, removed with it.
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        Serve(null);
        LandOrigin.Near(0, 0);
        if (Directory.Exists(_folder)) Directory.Delete(_folder, recursive: true);
    }

    // The game the commands answer of, as the program sets it, which a test reaches through the
    // setter the program uses.
    private static void Serve(VoxelGame? game) => typeof(GameCommands).GetProperty(nameof(GameCommands.Game))!.SetValue(null, game);

    private static string Run(string line) => ConsoleCommands.Run(string.Create(CultureInfo.InvariantCulture, $"{line}")) ?? "";

    [GeneratedRegex(@"^feet at (-?[0-9.]+), (-?[0-9.]+), (-?[0-9.]+) ")]
    private static partial Regex Feet();

    [Theory]
    [InlineData(1_139_992.5, 596.5)]
    [InlineData(-284_680.5, -33.5)]
    public void A_Game_Far_Out_Keeps_The_Players_Place_A_Broken_Block_And_A_Villager_Across_A_Save(double x, double z)
    {
        double y;
        LandPlace villager, home, stood;
        using (var headless = new HeadlessGame(save: _folder))
        {
            var game = headless.Game;
            Serve(game);
            y = LandOrigin.LandOf(game.Player.Body.Position).Y;

            Assert.StartsWith("the player stands at", Run(FormattableString.Invariant($"voxel.tp {x} {y} {z}")));
            headless.Step(1);
            var state = Feet().Match(Run("voxel.state"));
            Assert.True(state.Success);
            Assert.Equal((x, y, z), (double.Parse(state.Groups[1].Value, CultureInfo.InvariantCulture), double.Parse(state.Groups[2].Value, CultureInfo.InvariantCulture),
                double.Parse(state.Groups[3].Value, CultureInfo.InvariantCulture)));
            Assert.True(Math.Abs(x - LandOrigin.X) <= LandOrigin.Reach && Math.Abs(z - LandOrigin.Z) <= LandOrigin.Reach);
            // The land there in, as the streamer brings it.
            var clock = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                headless.Step(1);
                Thread.Sleep(2);
            }
            while (game.Streamer.Pending > 0 && clock.Elapsed.TotalSeconds < 30);

            // The ground's block east of the feet, looked at half way down, broken, where the eye 1.62
            // over the feet meets the ground 1.62 ahead.
            int bx = (int)Math.Floor(x + 1.62), by = (int)Math.Floor(y) - 1, bz = (int)Math.Floor(z);
            Assert.NotEqual(BlockId.Air, game.World.GetBlock(bx, by, bz));
            Run("voxel.look 90 -45");
            headless.Step(1);
            Assert.Equal((bx, by, bz), (game.Target?.X, game.Target?.Y, game.Target?.Z));
            Assert.Equal("the block is broken", Run("voxel.break"));
            Assert.Equal(BlockId.Air, game.World.GetBlock(bx, by, bz));

            // A villager set down three blocks ahead of the player, at home where it stands.
            Run("voxel.look 270 0");
            Run("voxel.still on");
            headless.Step(1);
            Assert.StartsWith("villager", Run("voxel.spawn villager"));
            var mob = Assert.Single(game.Mobs.All, m => m.Kind == MobKinds.Villager);
            (villager, home) = (LandOrigin.LandOf(mob.Body.Position), LandOrigin.LandOf(mob.Home!.Value));
            Assert.Equal(x - 3, villager.X, 3);
            Assert.StartsWith("saved to", Run("voxel.save"));
            stood = LandOrigin.LandOf(game.Player.Body.Position);
            Assert.Equal((x, z), (stood.X, stood.Z));
        }

        // The save holds the villager and its home where they were, a place of the land in doubles.
        var (kept, _) = new WorldSave(_folder).ReadMobs();
        var record = Assert.Single(kept.Values.SelectMany(list => list), m => m.Kind == MobKinds.Villager);
        Assert.Equal((villager, home), (record.Position, record.Home));

        // Opened again, the player stands where it stood, the origin near it, the block stays broken
        // and the villager stands where it was saved, held still from the first frame, since it
        // walked half a block in the frames a slower or faster machine takes to stand the world.
        LandOrigin.Near(0, 0);
        using (var again = new HeadlessGame(save: _folder, still: true))
        {
            var game = again.Game;
            Serve(game);
            Assert.Equal(stood, LandOrigin.LandOf(game.Player.Body.Position));
            Assert.True(Math.Abs(x - LandOrigin.X) <= LandOrigin.Reach);
            Assert.Equal(BlockId.Air, game.World.GetBlock((int)Math.Floor(x + 1.62), (int)Math.Floor(y) - 1, (int)Math.Floor(z)));
            // However many frames pass.
            again.Step(120);
            var mob = Assert.Single(game.Mobs.All, m => m.Kind == MobKinds.Villager);
            Assert.Equal((record.Position, record.Home), (LandOrigin.LandOf(mob.Body.Position), LandOrigin.LandOf(mob.Home!.Value)));
        }
    }

    [Fact]
    public void A_Player_Who_Dies_Far_Out_Comes_Back_At_The_Spawn_Exactly()
    {
        using var headless = new HeadlessGame(at: new LandPlace(-284_680.5, 0, 596.5));
        var game = headless.Game;
        Serve(game);
        Run("voxel.mode survival");
        Assert.True(Math.Abs(-284_680.5 - LandOrigin.X) <= LandOrigin.Reach);

        Assert.True(game.Vitals.Damage(40, "fell from a high place"));
        Assert.StartsWith("the player stands at 0.5, ", Run("voxel.respawn"));

        var feet = LandOrigin.LandOf(game.Player.Body.Position);
        Assert.Equal((0.5, 0.5), (feet.X, feet.Z));
        Assert.Equal((0, 0), (LandOrigin.X, LandOrigin.Z));
    }
}
