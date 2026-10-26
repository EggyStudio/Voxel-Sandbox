using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The whole game the frame's tests run, built with no window.</summary>
[Collection(nameof(HeadlessGames))]
public sealed class HeadlessGameTests
{
    [Fact]
    public void A_Headless_Game_Stands_Its_Flat_World_With_Every_Column_In_Reach_And_The_Player_On_Its_Ground()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        Assert.False(game.Loading);
        Assert.Equal(0, game.Streamer.Pending);
        var feet = game.Player.Body.Position;
        int cx = VoxelWorld.ColumnOf((int)MathF.Floor(feet.X)), cz = VoxelWorld.ColumnOf((int)MathF.Floor(feet.Z));
        for (int dz = -2; dz <= 2; dz++)
            for (int dx = -2; dx <= 2; dx++)
                Assert.True(game.World.HasColumn(cx + dx, cz + dz), $"the column {cx + dx}, {cz + dz} is not in");
        headless.Step(10);
        Assert.True(game.Player.Body.OnGround, $"the player stands at {game.Player.Body.Position}");
        // With no device to compute on, every column reads its caves' noises itself.
        Assert.False(game.CaveFields.Running);
        Assert.Equal(0, game.CaveFields.Read);
    }

    [Fact]
    public void An_Options_File_That_Turned_The_Screen_Probes_Off_Leaves_The_Quality_That_Traces_Them()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        Assert.Equal(GlobalIllumination.High, game.Light.Quality);
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            var file = new OptionsFile(folder);
            Directory.CreateDirectory(folder);
            File.WriteAllText(file.File, "render.Screen_Probes:Off\nrender.Render_Distance:6\n");
            // The screen's probes have no setting, the engine tracing them at Medium and High, so a
            // file's line of them is passed over and the one beside it read.
            Assert.Equal(1, file.Read(game.Menus.Screens(game).All()));
            Assert.Equal(GlobalIllumination.High, game.Light.Quality);
            Assert.Equal(6, game.RenderDistance);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void The_Debug_Overlay_Holds_The_Games_Sections_Of_The_Player_Generation_Light_Sky_Sections_And_Mobs()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        OverlaySections.Add(game);
        Assert.StartsWith("Feet at ", OverlaySections.Player(game).First());
        Assert.Equal("Flat, seed 1", OverlaySections.Generation(game).First());
        Assert.StartsWith("At the eye sky ", OverlaySections.Light(game).First());
        Assert.StartsWith("Hour ", OverlaySections.Sky(game).First());
        Assert.Contains(" columns, ", OverlaySections.Sections(game).First());
        Assert.Contains(" loaded, ", OverlaySections.Mobs(game).First());
        // Each is the engine's overlay's, which takes it away again by its name.
        foreach (var name in OverlaySections.Names) Assert.True(E3D.RemoveDebugOverlaySection(name), name);
    }
}
