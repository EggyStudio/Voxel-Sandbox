using Engine;
using Xunit;
using static Engine.E3D;

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
    public void The_Start_Reads_The_Windows_Mode_And_Resolution_From_The_Options_File()
    {
        using var headless = new HeadlessGame();
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            var video = new VideoSettings();
            var (width, height) = (GetScreenWidth(), GetScreenHeight());
            // With no file the window keeps the size it opened at, in a window.
            video.Restore(new OptionsFile(folder));
            Assert.Equal((0, width, height), (VideoSettings.WindowMode, GetScreenWidth(), GetScreenHeight()));
            var file = new OptionsFile(folder);
            Directory.CreateDirectory(folder);
            File.WriteAllText(file.File, "video.Window_Mode:Borderless\nvideo.Resolution:1\nrender.Render_Distance:6\n");
            Assert.Equal(2f, file.Find("video", OptionScreens.WindowModeOf(video)));
            Assert.Equal(1f, file.Find("video", OptionScreens.ResolutionOf(video)));
            // A run with no window holds no monitor's modes nor a borderless state, so the
            // resolution read is what it shows, the mode being the engine's window's alone.
            video.Restore(file);
            Assert.Equal(1, video.Resolution);
            // A mode the file cannot read leaves the window in a window.
            File.WriteAllText(file.File, "video.Window_Mode:Sideways\n");
            Assert.Null(file.Find("video", OptionScreens.WindowModeOf(video)));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
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

    [Fact]
    public void The_New_World_Screen_Begins_A_World_Of_The_Kind_And_The_Seed_Typed()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        game.Menus.Begin(game, "overworld", "42");
        headless.Step(2);
        Assert.Equal(("overworld", 42), (game.World.Generator.Name, game.World.Generator.Seed));
        Assert.Equal(Menu.None, game.Menus.Open);
        // Words are hashed into a seed as Minecraft hashes them, and a number is taken as it reads.
        game.Menus.Begin(game, "flat", "hello");
        headless.Step(2);
        Assert.Equal(("flat", 99162322), (game.World.Generator.Name, game.World.Generator.Seed));
        Assert.Equal(-5, Menus.SeedOf(" -5 "));
    }

    [Fact]
    public void The_New_World_Screen_Opens_With_Its_Seeds_Field_Holding_The_Keyboard()
    {
        // The field takes the typing from the screen's first frame, so the game's own keys stand
        // aside. Typing and pasting into it go through e3d, which build/play.sh does.
        using var headless = new HeadlessGame();
        var game = headless.Game;
        headless.Step(2);
        Assert.False(IsTextInputActive());
        game.Menus.OpenNewWorld();
        headless.Step(2);
        Assert.True(IsTextInputActive());
        Assert.Equal(Menu.NewWorld, game.Menus.Open);
    }

    [Fact]
    public void A_Pack_Fonts_Engine_Font_Measures_Each_Glyph_As_Wide_As_Its_Advance()
    {
        // A's lit to its fifth column on a sheet of sixteen pixels a glyph, so the engine's font
        // measures text at eight pixels as the sheet draws it, at twice that size twice as wide. The
        // font's atlas is a texture, so a game stands for the window.
        using var headless = new HeadlessGame();
        var sheet = GenImageColor(256, 256, Color.Blank);
        int left = 'A' % 16 * 16, top = 'A' / 16 * 16;
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 10; x++)
                sheet.Data[((top + y) * sheet.Width + left + x) * 4 + 3] = 255;
        var advances = PackFont.Advances(sheet);
        var font = PackFont.MeasureOf(sheet, advances);
        Assert.Equal(8, font.BaseSize);
        Assert.Equal(6 + 4 + 4 + 6, MeasureTextEx(font, "A BA", 8, 0).X);
        Assert.Equal(2 * (6 + 4), MeasureTextEx(font, "A ", 16, 0).X);
    }
}
