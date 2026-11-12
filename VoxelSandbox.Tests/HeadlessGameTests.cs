using System.Numerics;
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
        var (cx, cz) = LandOrigin.ColumnOf(game.Player.Body.Position);
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
    public void A_Teleport_Of_A_Million_Blocks_Leaves_No_Column_Behind()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        var feet = game.Player.Body.Position;
        var (cx, cz) = LandOrigin.ColumnOf(feet);
        Assert.True(game.World.HasColumn(cx, cz));

        // 1,000,000 blocks east is 62,500 columns, whose square passes an int.
        game.Player.Teleport(feet + new Vector3(1_000_000, 0, 0));
        var clock = System.Diagnostics.Stopwatch.StartNew();
        do headless.Step(1);
        while (game.Streamer.Pending > 0 && clock.Elapsed.TotalSeconds < 30);

        Assert.False(game.World.HasColumn(cx, cz), $"the column {cx}, {cz} the player left is still in, of {game.World.ColumnCount}");
        Assert.All(game.World.Columns, c => Assert.True(Math.Abs(c.X - cx) > 60_000, $"the column {c.X}, {c.Z} is still in"));
    }

    [Fact]
    public void Restocking_Places_Again_What_The_Land_Placed_Where_A_Save_Lost_It()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        game.Mobs.Still = true;
        var generated = game.World.Columns.ToDictionary(c => (c.X, c.Z), c => game.World.Generator.Generate(c.X, c.Z).Spawns.ToList());
        var spawns = generated.Values.SelectMany(list => list).ToList();
        Assert.NotEmpty(spawns);

        // Emptied as a save that lost the loaded columns' mobs left them, but one column holding a pig.
        game.Mobs.Clear(null);
        var pig = new LandPlace(spawns[0].Position.X, spawns[0].Position.Y, spawns[0].Position.Z);
        var held = generated.First(pair => pair.Value.Contains(spawns[0])).Key;
        game.Mobs.Add(new MobRecord(MobKinds.Pig, 0, pig, 0, MobKinds.Pig.Health, null));
        // And one place of another column filled with stone where its spawn's feet stand.
        var blocked = spawns.First(m => LandOrigin.ColumnOf(LandOrigin.PlaceOf(m.Position)) != held);
        game.World.SetBlock((int)Math.Floor(blocked.Position.X), (int)Math.Floor(blocked.Position.Y), (int)Math.Floor(blocked.Position.Z), BlockId.Stone);

        var (columns, filled, animals, villagers) = game.Mobs.Restock(game.World);

        var expected = spawns.Where(m => LandOrigin.ColumnOf(LandOrigin.PlaceOf(m.Position)) != held && m != blocked).ToList();
        Assert.Equal(game.World.ColumnCount - 1, columns);
        Assert.Equal((expected.Count, 1), (animals.Placed + villagers.Placed, animals.LeftOut + villagers.LeftOut));
        Assert.Equal(expected.Select(m => m.Position).Append(pig).OrderBy(p => (p.X, p.Y, p.Z)), game.Mobs.All.Select(m => LandOrigin.LandOf(m.Body.Position)).OrderBy(p => (p.X, p.Y, p.Z)));
        Assert.Equal(generated.Count(pair => pair.Key != held && pair.Value.Any(m => m != blocked)), filled);

        // A second restock finds every column holding a mob or one with none to give.
        Assert.Equal(0, game.Mobs.Restock(game.World).Animals.Placed);
    }

    [Fact]
    public void Mobs_Kept_Out_Stay_Out_Of_A_World_Opened_After()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        void Open(int seed)
        {
            game.SwitchWorld("flat", seed);
            var clock = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                headless.Step(1);
                Thread.Sleep(2);
            }
            while ((game.Loading || game.Streamer.Pending > 0) && clock.Elapsed.TotalSeconds < 30);
        }

        game.Mobs.SetSpawning(game.World, false);
        Open(2);
        Assert.False(game.Mobs.Spawning);
        Assert.Empty(game.Mobs.All);

        // The same world with the mobs let in again holds its herds.
        game.Mobs.SetSpawning(game.World, true);
        Open(2);
        Assert.NotEmpty(game.Mobs.All);
    }

    [Fact]
    public void Closing_The_Game_Again_Keeps_The_Mobs_The_First_Close_Saved()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using var headless = new HeadlessGame(save: folder);
            var game = headless.Game;
            var feet = LandOrigin.LandOf(game.Player.Body.Position);
            var pig = new LandPlace(feet.X + 3.5, feet.Y, feet.Z + 0.5);
            game.Mobs.Add(new MobRecord(MobKinds.Pig, 0, pig, 0, MobKinds.Pig.Health, null));

            game.Dispose();
            // As the app disposes the game again as the window closes, after its ECS let the mobs go.
            game.Mobs.Clear(null);
            game.Dispose();

            var (kept, _) = new WorldSave(folder).ReadMobs();
            Assert.Contains(kept.Values.SelectMany(list => list), mob => mob.Kind == MobKinds.Pig && mob.Position == pig);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void Moving_The_Origin_Keeps_Every_Place_In_The_Land()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        try
        {
            game.Mobs.Still = true;
            var feet = LandOrigin.LandOf(game.Player.Body.Position);
            var pig = game.Mobs.Add(new MobRecord(MobKinds.Pig, 0, new LandPlace(feet.X + 3.25, feet.Y, feet.Z + 0.5), 0, MobKinds.Pig.Health, null));
            var stack = game.Drops.Spawn(Items.Find("stone")!, 1, LandOrigin.PlaceOf(new LandPlace(feet.X - 2.5, feet.Y + 2, feet.Z)), Vector3.Zero);
            var drop = headless.Ecs.GetReadOnly<Body>(stack);
            headless.Step(2);
            var (pigAt, dropAt) = (LandOrigin.LandOf(pig.Body.Position), LandOrigin.LandOf(drop.Position));

            // A place 3,000 blocks east moves the origin to 3,072, every place kept.
            Assert.True(game.Recenter(new LandPlace(feet.X + 3000, feet.Y, feet.Z)));
            Assert.Equal((3072, 0), (LandOrigin.X, LandOrigin.Z));
            Assert.Equal(feet, LandOrigin.LandOf(game.Player.Body.Position));
            Assert.Equal(pigAt, LandOrigin.LandOf(pig.Body.Position));
            Assert.Equal(dropAt, LandOrigin.LandOf(drop.Position));

            // The player, still where it stood, takes the origin back near it in the next frame.
            headless.Step(1);
            Assert.Equal((0, 0, 2), (LandOrigin.X, LandOrigin.Z, game.Recentered));
            Assert.Equal(feet, LandOrigin.LandOf(game.Player.Body.Position));
            Assert.Equal(pigAt, LandOrigin.LandOf(pig.Body.Position));
        }
        finally
        {
            LandOrigin.Near(0, 0);
        }
    }

    [Fact]
    public void A_Walk_Across_Where_The_Origin_Moves_Goes_On_Without_A_Jump()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        try
        {
            // Put 1,016 blocks east, as voxel.tp puts the player, and stood once the land there is in.
            var start = new LandPlace(1016.5, LandOrigin.LandOf(game.Player.Body.Position).Y, 0.5);
            game.Recenter(start);
            game.Player.Teleport(LandOrigin.PlaceOf(start));
            var clock = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                headless.Step(1);
                Thread.Sleep(2);
            }
            while (game.Streamer.Pending > 0 && clock.Elapsed.TotalSeconds < 30);
            headless.Step(10);
            Assert.Equal(0, LandOrigin.X);

            // Sixteen blocks east a tenth of a block a frame, across 1,024, where the origin moves.
            var walked = new List<double>();
            for (int frame = 0; frame < 160; frame++)
            {
                game.Player.Body.Move(game.World, new Vector3(0.1f, 0, 0), keepToEdges: false);
                headless.Step(1);
                walked.Add(LandOrigin.LandOf(game.Player.Body.Position).X);
            }

            Assert.Equal(1024, LandOrigin.X);
            Assert.Equal(1, game.Recentered);
            Assert.All(walked.Zip(walked.Skip(1)), pair => Assert.Equal(0.1, pair.Second - pair.First, 3));
        }
        finally
        {
            LandOrigin.Near(0, 0);
        }
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
    public void A_Kept_Value_Equal_To_A_Default_Since_Changed_Gives_Way_To_The_New_Default_In_An_Older_File()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            var file = new OptionsFile(folder);
            Directory.CreateDirectory(folder);
            var screens = game.Menus.Screens(game).All();
            // A file of no version holds every setting as it stood, Soft Shadows at the 0 it held
            // before its default was 1.5, so that line is read as the old default.
            File.WriteAllText(file.File, "render.Soft_Shadows:0\nrender.Far_Cascade:Off\nrender.Shadow_Distance:64\nrender.Render_Distance:6\n");
            Assert.Equal(1, file.Read(screens));
            Assert.Equal((1.5f, true, 128f, 6), (game.Light.ShadowSoftness, game.Light.FarCascade, game.Light.ShadowDistance, game.RenderDistance));
            // Written again, the file says its version, and a 0 chosen since stands.
            Assert.StartsWith($"version:{OptionsFile.Version}\n", OptionsFile.Text(screens));
            File.WriteAllText(file.File, $"version:{OptionsFile.Version}\nrender.Soft_Shadows:0\n");
            Assert.Equal(1, file.Read(screens));
            Assert.Equal(0f, game.Light.ShadowSoftness);
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
