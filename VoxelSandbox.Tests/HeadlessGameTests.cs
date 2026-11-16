using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
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

        // Every loaded column emptied of mobs but one holding a pig.
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

            Assert.Contains(HeadlessGame.SavedMobs(folder), mob => mob.Kind == MobKinds.Pig && mob.Position == pig);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Theory]
    [InlineData("Kind", "null")]
    [InlineData("X", "1e300")]
    [InlineData("Heading", "1e300")]
    [InlineData("Hour", "1e300")]
    [InlineData("Food", "-1")]
    public void A_World_Whose_Info_Holds_A_Value_The_Game_Cannot_Hold_Opens_With_The_Rest_And_Saves(string field, string value)
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            // A world saved with its third slot chosen, its info given the value by hand.
            using (var first = new HeadlessGame(save: folder)) first.Game.Hotbar.Selected = 2;
            var path = Path.Combine(folder, "world.json");
            var info = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            info[field] = JsonNode.Parse(value);
            File.WriteAllText(path, info.ToJsonString());

            using (var opened = new HeadlessGame(save: folder))
            {
                opened.Step(5);
                var (game, at) = (opened.Game, LandOrigin.LandOf(opened.Game.Player.Body.Position));
                Assert.Equal("flat", game.World.Generator.Name);
                Assert.Equal(2, game.Hotbar.Selected);
                Assert.True(Math.Abs(at.X) < 16 && Math.Abs(at.Z) < 16 && at.Y is > 0 and < 100, at.ToString());
                Assert.True(float.IsFinite(game.Player.Heading) && float.IsFinite(game.Sky.Hour));
                Assert.InRange(game.Vitals.Food, 0, Vitals.Most);
            }

            var saved = new WorldSave(folder).ReadInfo()!;
            Assert.Equal("flat", saved.Kind);
            Assert.True(Math.Abs(saved.X) < 16 && Math.Abs(saved.Z) < 16, $"{saved.X}, {saved.Z}");
            Assert.Equal(2, saved.Selected);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Kept_Column_Whose_Blocks_Do_Not_Inflate_Is_Generated_Again_And_Its_Regions_Others_Kept()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            // A glowstone 70 high in the player's column and in the one east of it, the region's two
            // columns, the first it lists given a zlib head no stream begins with.
            using (var first = new HeadlessGame(save: folder))
            {
                first.Game.World.SetBlock(2, 70, 2, BlockId.Glowstone);
                first.Game.World.SetBlock(18, 70, 2, BlockId.Glowstone);
            }
            var path = Path.Combine(folder, "regions", "r.0.0.bin");
            var bytes = File.ReadAllBytes(path);
            Assert.Equal(2, BitConverter.ToInt32(bytes, 8));
            var (cut, whole) = BitConverter.ToInt16(bytes, 12) == 0 ? (2, 18) : (18, 2);
            bytes[20] = 0;
            File.WriteAllBytes(path, bytes);

            using (var opened = new HeadlessGame(save: folder))
            {
                Assert.Equal(BlockId.Air, opened.Game.World.GetBlock(cut, 70, 2));
                Assert.Equal(BlockId.Glowstone, opened.Game.World.GetBlock(whole, 70, 2));
                Assert.Equal(bytes, File.ReadAllBytes(path + ".unread"));
            }

            using var again = new HeadlessGame(save: folder);
            Assert.Equal(BlockId.Glowstone, again.Game.World.GetBlock(whole, 70, 2));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_World_Whose_Blocks_Were_Numbered_Otherwise_Keeps_Them_Through_A_Save()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            // A world saved by a build that numbered stone as this one numbers grass, and grass as
            // stone, with a stone 70 high in the column east of the player's, in the player's region,
            // and in the column west of it, in the region beside.
            using (new HeadlessGame(save: folder)) { }
            var keys = Blocks.All.Select(b => b.Key).ToArray();
            (keys[(int)BlockId.Grass], keys[(int)BlockId.Stone]) = (keys[(int)BlockId.Stone], keys[(int)BlockId.Grass]);
            var info = new WorldSave(folder).ReadInfo()!;
            new WorldSave(folder).WriteInfo(info with { Blocks = keys });
            var (east, west) = (new ChunkColumn(1, 0), new ChunkColumn(-1, 0));
            east.Set(0, 70, 0, BlockId.Grass);
            west.Set(0, 70, 0, BlockId.Grass);
            SaveTests.WriteVersion1Region(folder, 0, 0, east);
            SaveTests.WriteVersion1Region(folder, -1, 0, west);

            // Played and saved by this build, a block set in the player's column so that its region
            // is written again.
            using (var played = new HeadlessGame(save: folder))
            {
                Assert.Equal(BlockId.Stone, played.Game.World.GetBlock(16, 70, 0));
                played.Game.World.SetBlock(2, 70, 2, BlockId.Glowstone);
            }

            using var again = new HeadlessGame(save: folder);
            Assert.Equal(BlockId.Glowstone, again.Game.World.GetBlock(2, 70, 2));
            Assert.Equal(BlockId.Stone, again.Game.World.GetBlock(16, 70, 0));
            Assert.Equal(BlockId.Stone, again.Game.World.GetBlock(-16, 70, 0));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Column_Saves_The_Mobs_It_Keeps_Out_Of_The_World_With_One_Placed_Among_Them()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        var feet = LandOrigin.LandOf(game.Player.Body.Position);
        var column = (VoxelWorld.ColumnOf((int)Math.Floor(feet.X)), VoxelWorld.ColumnOf((int)Math.Floor(feet.Z)));
        var kept = new LandPlace(Math.Floor(feet.X) + 0.25, feet.Y, Math.Floor(feet.Z) + 0.25);
        var placed = new LandPlace(Math.Floor(feet.X) + 0.75, feet.Y, Math.Floor(feet.Z) + 0.75);
        game.Mobs.Add(new MobRecord(MobKinds.Pig, 0, kept, 0, MobKinds.Pig.Health, null));
        // Spawning turned off keeps the pig out of the world in its column, and one is placed there
        // as voxel.spawn places it.
        game.Mobs.SetSpawning(game.World, false);
        game.Mobs.Add(new MobRecord(MobKinds.Pig, 0, placed, 0, MobKinds.Pig.Health, null));

        var saved = Assert.Single(game.Mobs.Columns(), c => c.Column == column).Mobs;
        Assert.Contains(saved, mob => mob.Position == kept);
        Assert.Contains(saved, mob => mob.Position == placed);
    }

    [Fact]
    public void The_Stacks_Of_The_Inventory_Window_Are_Saved_While_It_Is_Open_And_As_The_Game_Closes()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using var headless = new HeadlessGame(save: folder);
            var game = headless.Game;
            game.InventoryWindow.Small.Slots[0] = new ItemStack(Items.Stick, 3);

            game.SaveWorld();
            Assert.Contains("stick 3", new WorldSave(folder).ReadInfo()!.Inventory!);
            Assert.Equal(new ItemStack(Items.Stick, 3), game.InventoryWindow.Small.Slots[0]);

            game.Dispose();
            Assert.Contains("stick 3", new WorldSave(folder).ReadInfo()!.Inventory!);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_World_Switched_To_Takes_No_Stack_Of_The_Inventory_Window_From_The_One_Left()
    {
        var (folder, other) = (Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N")), Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N")));
        try
        {
            using var headless = new HeadlessGame(save: folder);
            var game = headless.Game;
            game.InventoryWindow.Small.Slots[0] = new ItemStack(Items.Stick, 3);

            game.SwitchWorld("flat", 2, other);

            Assert.Contains("stick 3", new WorldSave(folder).ReadInfo()!.Inventory!);
            Assert.All(game.InventoryWindow.Small.Slots, slot => Assert.Null(slot));
            Assert.DoesNotContain(game.Inventory.Slots, slot => slot?.Item == Items.Stick);
        }
        finally
        {
            foreach (var path in new[] { folder, other })
                if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
        }
    }

    [Fact]
    public void A_Save_That_Cannot_Be_Written_Lets_The_Game_Close_And_Switch_And_Waits_Longer()
    {
        var (folder, other) = (Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N")), Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N")));
        try
        {
            // A world saved once, then a folder in the place of each file a save writes before it
            // takes the file's name, so none can be written, on every system alike.
            using (var first = new HeadlessGame(save: folder)) first.Game.World.SetBlock(2, 70, 2, BlockId.Glowstone);
            foreach (var name in new[] { "world.json", Path.Combine("regions", "m.0.0.bin"), "furnaces.json", Path.Combine("regions", "c.0.0.bin"), Path.Combine("regions", "r.0.0.bin") })
                Directory.CreateDirectory(Path.Combine(folder, name + ".new"));

            // Closed with a block changed, which it cannot write.
            using (var closed = new HeadlessGame(save: folder)) closed.Game.World.SetBlock(3, 70, 2, BlockId.Glowstone);

            using var headless = new HeadlessGame(save: folder);
            var game = headless.Game;
            game.World.SetBlock(4, 70, 2, BlockId.Glowstone);
            game.SaveWorld();
            var told = game.Said;
            headless.Step(3);
            game.SaveWorld();

            Assert.StartsWith("The world could not be saved", told?.Text);
            Assert.True(game.Said?.Age > 0, "the second save that failed told the player again");
            Assert.Equal(120, game.SaveWait);
            var read = new WorldSave(folder);
            read.ReadRegion(0, 0);
            Assert.Equal(BlockId.Glowstone, WorldSave.Decode(0, 0, read.Kept(0, 0)!).Get(2, 70, 2));

            game.SwitchWorld("flat", 2, other);

            Assert.Equal(other, game.Save?.Folder);
            Assert.Equal(30, game.SaveWait);
        }
        finally
        {
            foreach (var path in new[] { folder, other })
                if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
        }
    }

    [Fact]
    public void Leaving_A_Menu_Whose_Settings_Cannot_Be_Kept_Draws_On_And_Tells_The_Player_Once()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            // A folder in the place of the copy the options file is written to before it takes its name.
            Directory.CreateDirectory(Path.Combine(folder, "options.txt.new"));
            using var headless = new HeadlessGame();
            var game = headless.Game;
            game.SettingsFile = new OptionsFile(folder);

            game.Menus.Open = Menu.Options;
            headless.Step(2);
            game.Menus.Open = Menu.None;
            headless.Step(3);
            var told = game.Said;
            game.Menus.Open = Menu.Options;
            headless.Step(2);
            game.Menus.Open = Menu.None;
            headless.Step(3);

            Assert.StartsWith("The settings could not be kept", told?.Text);
            Assert.True(game.Said?.Age > told?.Age, "leaving the menu again told the player again");
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void An_Unreadable_Options_File_Keeps_The_Defaults_And_Is_Written_Once_A_Setting_Changes()
    {
        // A file's mode the account cannot read is set on Linux and macOS alone.
        if (OperatingSystem.IsWindows()) return;
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(folder, "options.txt");
        try
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(path, "render.Render_Distance:7\n");
            File.SetUnixFileMode(path, UnixFileMode.None);
            using var headless = new HeadlessGame();
            var game = headless.Game;
            var (file, screens) = (new OptionsFile(folder), game.Menus.Screens(game).All().ToList());
            var distance = game.RenderDistance;

            Assert.Null(file.Find("render", screens.First(s => s.Name == "render").Options.First(o => o.Kept)));
            Assert.ThrowsAny<UnauthorizedAccessException>(() => file.Read(screens));
            Assert.Equal(distance, game.RenderDistance);
            Assert.False(file.Write(screens));
            game.RenderDistance = distance + 1;
            Assert.True(file.Write(screens));
        }
        finally
        {
            if (File.Exists(path)) File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_World_Whose_Folder_Cannot_Be_Made_Is_Played_Unsaved_And_Said()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            // A file where the world's folder would stand above it.
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "a file"), "");
            using var headless = new HeadlessGame(save: Path.Combine(folder, "a file", "world"));

            Assert.Null(headless.Game.Save);
            Assert.Contains("The world's folder could not be made, so it is played unsaved", headless.Game.FileFaults);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Screenshot_Whose_Folder_Cannot_Be_Made_Says_So_And_Not_That_It_Was_Saved()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using var headless = new HeadlessGame(save: Path.Combine(folder, "world"));
            File.WriteAllText(Path.Combine(folder, "a file"), "");
            var game = headless.Game;
            game.Shots = new Screenshots(Path.Combine(folder, "a file", "screenshots"));

            game.Screenshot();

            Assert.StartsWith("Screenshots could not be kept", game.Said?.Text);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Switch_To_A_World_Whose_Folder_Cannot_Be_Made_Keeps_The_World_Played_And_Says_So()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using var headless = new HeadlessGame(save: Path.Combine(folder, "world"));
            File.WriteAllText(Path.Combine(folder, "a file"), "");
            var game = headless.Game;

            game.SwitchWorld("flat", 2, Path.Combine(folder, "a file", "other"));

            Assert.Equal(Path.Combine(folder, "world"), game.Save?.Folder);
            Assert.Contains("The world's folder could not be made, so the world played stays", game.FileFaults);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Recording_Whose_File_Cannot_Be_Written_Ends_And_Says_So()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "a file"), "");
            using var headless = new HeadlessGame();
            var game = headless.Game;

            game.Flicker = new FlickerRun(13, 10, 1, 0, 0, Path.Combine(folder, "a file", "flicker.txt"));
            headless.Step(20);

            Assert.Null(game.Flicker);
            Assert.Contains("The recording could not be written", game.FileFaults);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Start_Whose_Options_File_Cannot_Be_Read_Keeps_The_Defaults_And_Leaves_The_File()
    {
        // A file's mode the account cannot read is set on Linux and macOS alone.
        if (OperatingSystem.IsWindows()) return;
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        var options = Path.Combine(folder, "settings", "options.txt");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(options)!);
            File.WriteAllText(options, "render.Render_Distance:7\n");
            File.SetUnixFileMode(options, UnixFileMode.None);
            var written = File.GetLastWriteTimeUtc(options);

            using (var headless = new HeadlessGame(save: Path.Combine(folder, "world"), settings: Path.Combine(folder, "settings")))
                Assert.Contains("The settings could not be read", headless.Game.FileFaults);

            Assert.Equal(written, File.GetLastWriteTimeUtc(options));
        }
        finally
        {
            if (File.Exists(options)) File.SetUnixFileMode(options, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Start_Whose_Packs_List_Cannot_Be_Read_Draws_No_Pack_And_Says_So()
    {
        if (OperatingSystem.IsWindows()) return;
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        var list = Path.Combine(folder, "settings", "resourcepacks.txt");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(list)!);
            File.WriteAllText(list, "a pack.zip\n");
            File.SetUnixFileMode(list, UnixFileMode.None);

            using var headless = new HeadlessGame(save: Path.Combine(folder, "world"), settings: Path.Combine(folder, "settings"));

            Assert.Contains("The packs' list could not be read", headless.Game.FileFaults);
            Assert.Empty(headless.Game.PackEntries);
        }
        finally
        {
            if (File.Exists(list)) File.SetUnixFileMode(list, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void The_Packs_Screen_Over_A_Packs_Folder_That_Cannot_Be_Listed_Draws_On_And_Says_So()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            // A file in the place of the packs' folder, on every system alike.
            Directory.CreateDirectory(Path.Combine(folder, "settings"));
            File.WriteAllText(Path.Combine(folder, "settings", "resourcepacks"), "");
            using var headless = new HeadlessGame(save: Path.Combine(folder, "world"), settings: Path.Combine(folder, "settings"));
            var game = headless.Game;

            game.Menus.Open = Menu.Packs;
            headless.Step(3);
            game.Menus.Open = Menu.None;
            headless.Step(3);

            Assert.Contains("The packs' folder could not be read", game.FileFaults);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Pack_That_Cannot_Be_Read_Is_Left_Out_And_Said()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(folder);
            var cut = Path.Combine(folder, "cut.zip");
            File.WriteAllText(cut, "no zip");
            using var headless = new HeadlessGame();

            headless.Game.ChoosePacks([cut]);
            headless.Step(2);

            Assert.StartsWith("The pack cut.zip could not be read", headless.Game.Said?.Text);
            Assert.False(headless.Game.Packs.Any);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Game_That_Saves_Leaves_The_Players_Settings_Alone()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using var headless = new HeadlessGame(save: folder);
            // Neither the player's packs nor the default ones, which a missing pack list fetches.
            Assert.Empty(headless.Game.PackEntries);
            Assert.Equal(FetchState.Idle, headless.Game.Defaults.State);
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

            // A place 2,000 blocks past the reach east moves the origin to the step nearest it, every
            // place kept.
            Assert.True(game.Recenter(new LandPlace(feet.X + LandOrigin.Reach + 2000, feet.Y, feet.Z)));
            Assert.Equal(((int)Math.Round((feet.X + LandOrigin.Reach + 2000) / LandOrigin.Step) * LandOrigin.Step, 0), (LandOrigin.X, LandOrigin.Z));
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
    public void A_Walk_Across_Where_The_Origin_Moves_And_Back_Goes_On_Without_A_Jump_Or_A_Meal()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        try
        {
            // Put 7.5 blocks short of the reach east, as voxel.tp puts the player, and stood once the
            // land there is in.
            var start = new LandPlace(LandOrigin.Reach - 7.5, LandOrigin.LandOf(game.Player.Body.Position).Y, 0.5);
            game.Leave(start);
            game.Recenter(start);
            game.Player.Teleport(LandOrigin.PlaceOf(start));
            game.Mode = GameMode.Survival;
            var clock = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                headless.Step(1);
                Thread.Sleep(2);
            }
            while (game.Streamer.Pending > 0 && clock.Elapsed.TotalSeconds < 30);
            headless.Step(10);
            Assert.Equal(0, LandOrigin.X);
            var (exhaustion, saturation) = (game.Vitals.Exhaustion, game.Vitals.Saturation);

            // Sixteen blocks east a tenth of a block a frame, across the reach, where the origin moves,
            // and back west over it, where the origin stays, the player within its reach.
            var walked = new List<double>();
            void Walk(float step)
            {
                for (int frame = 0; frame < 160; frame++)
                {
                    game.Player.Body.Move(game.World, new Vector3(step, 0, 0), keepToEdges: false);
                    headless.Step(1);
                    walked.Add(LandOrigin.LandOf(game.Player.Body.Position).X);
                }
            }
            Walk(0.1f);
            Assert.Equal((LandOrigin.Reach, 1), (LandOrigin.X, game.Recentered));
            Assert.All(walked.Zip(walked.Skip(1)), pair => Assert.Equal(0.1, pair.Second - pair.First, 3));
            walked.Clear();
            Walk(-0.1f);
            Assert.Equal((LandOrigin.Reach, 1), (LandOrigin.X, game.Recentered));
            Assert.All(walked.Zip(walked.Skip(1)), pair => Assert.Equal(-0.1, pair.Second - pair.First, 3));

            // The walk of 32 blocks tires the player by a hundredth a block, the move none, where
            // taken for a walk of 1,024 blocks it took two of the saturation's points.
            Assert.Equal(saturation, game.Vitals.Saturation);
            Assert.Equal(0.32, game.Vitals.Exhaustion - exhaustion, 2);
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
            // A file of no version holds every setting, Soft Shadows at the 0 such a file holds as its
            // default, so that line is read as a default and the default of 1.5 stands.
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

    [Fact]
    public void No_Section_Of_A_Column_That_Never_Loaded_Is_Kept_Parked()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        headless.Step(10);

        // The columns of the ring past the streamed ones are marked as their neighbors arrive, and never load.
        Assert.NotEmpty(game.Renderer.Parked);
        Assert.All(game.Renderer.Parked, key => Assert.True(game.World.HasColumn(key.X, key.Z), $"{key} is parked with its column not loaded"));
    }

    [Fact]
    public void A_Pack_Of_The_Same_Tiles_In_Other_Colors_Gives_The_Light_Field_Its_Colors()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using var headless = new HeadlessGame();
            var game = headless.Game;
            var feet = LandOrigin.LandOf(game.Player.Body.Position);
            var key = new SectionKey(VoxelWorld.ColumnOf((int)Math.Floor(feet.X)), (int)Math.Floor(feet.Y - 1) >> Section.Shift, VoxelWorld.ColumnOf((int)Math.Floor(feet.Z)));
            // A pack whose ground is of one color, its textures named as the game's own.
            void Paint(byte r, byte g, byte b)
            {
                var plain = new Image([.. Enumerable.Range(0, 16 * 16).SelectMany(_ => new[] { r, g, b, (byte)255 })], 16, 16);
                foreach (var texture in new[] { "grass_block_top", "grass_block_side", "dirt", "stone", "bedrock" })
                {
                    var path = Path.Combine(folder, "assets", "minecraft", "textures", "block", texture + ".png");
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    File.WriteAllBytes(path, ExportImageToMemory(plain, ".png"));
                }
            }
            Paint(200, 30, 30);
            game.ChoosePacks([folder]);
            headless.Step(30);
            var red = game.Renderer.SolidColors(key).ToArray();
            Assert.NotEmpty(red);

            // The pack's textures painted green and read again, as F3 with T does, every tile
            // keeping its number.
            Paint(30, 200, 30);
            game.ReloadPacks();
            headless.Step(30);

            Assert.NotEqual(red, game.Renderer.SolidColors(key).ToArray());
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Block_Falling_And_A_Lit_Tnt_As_The_World_Closes_Are_In_It_When_It_Opens_Again()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using (var headless = new HeadlessGame(save: folder))
            {
                var game = headless.Game;
                // A sand let fall from twelve, its cell taken out as a fall takes it, one over a torch,
                // which it would break on, and a TNT lit on the ground.
                game.World.SetBlock(4, 4, 2, BlockId.Torch);
                game.Falling.Drop(BlockId.Sand, 2, 12, 2);
                game.Falling.Drop(BlockId.Sand, 4, 12, 2);
                game.Tnt.Light(6, 4, 2);
                headless.Step(1);
            }

            using var again = new HeadlessGame(save: folder);
            var world = again.Game.World;
            Assert.Equal(BlockId.Sand, world.GetBlock(2, 4, 2));
            Assert.Equal((BlockId.Torch, BlockId.Sand), (world.GetBlock(4, 4, 2), world.GetBlock(4, 5, 2)));
            Assert.Equal(BlockId.Tnt, world.GetBlock(6, 4, 2));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void The_Corner_Shade_Let_Go_After_A_World_Switch_Shades_The_World_Played_Again()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        // The screens as the game builds them reading its settings at its start, before any switch.
        var shade = game.Menus.Screens(game).All().SelectMany(s => s.Options).First(o => o.Name == "Corner Shade");
        var left = game.World;
        Switch(headless, 2);
        left.Reshaded.Clear();
        game.World.Reshaded.Clear();

        shade.Change(50);
        shade.Settled?.Invoke();

        Assert.NotEmpty(game.World.Reshaded);
        Assert.Empty(left.Reshaded);
    }

    [Fact]
    public void A_World_Switched_From_Is_Let_Go()
    {
        using var headless = new HeadlessGame();
        headless.Game.Menus.Screens(headless.Game);

        var left = Leave(headless);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.False(left.IsAlive);
    }

    // Switches the game to another world, giving the world left by a reference that does not hold
    // it, in a frame of its own so no local of the test's holds it either.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference Leave(HeadlessGame headless)
    {
        var left = new WeakReference(headless.Game.World);
        Switch(headless, 2);
        return left;
    }

    // Switches the game to the flat world of a seed and steps it until the world stands.
    private static void Switch(HeadlessGame headless, int seed)
    {
        headless.Game.SwitchWorld("flat", seed);
        var clock = System.Diagnostics.Stopwatch.StartNew();
        do
        {
            headless.Step(1);
            Thread.Sleep(2);
        }
        while ((headless.Game.Loading || headless.Game.Streamer.Pending > 0) && clock.Elapsed.TotalSeconds < 30);
    }

    [Fact]
    public void A_Column_Made_At_Once_While_A_Worker_Makes_It_Keeps_What_Changed_In_It()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        // A streamer of the game's world far from the game's own, which asks its workers for the
        // columns around a place, while the one under it is made at once, as a save lands the player.
        var streamer = new ChunkStreamer(game.World, game.Renderer, game.Mobs, game.Drops, game.Chests, null);
        var at = LandOrigin.PlaceOf(new LandPlace(16_000.5, 40, 0.5));
        var (x, z) = LandOrigin.ColumnOf(at);
        streamer.Update(at, 1);
        streamer.LoadNow(at, 0);
        Assert.True(game.World.TryGetColumn(x, z, out var made));
        game.World.SetBlock(x * Section.Size + 1, 70, z * Section.Size + 1, BlockId.Glowstone);

        var clock = System.Diagnostics.Stopwatch.StartNew();
        while (streamer.Pending > 0 && clock.Elapsed.TotalSeconds < 30)
        {
            streamer.Update(at, 1);
            Thread.Sleep(2);
        }

        Assert.True(game.World.TryGetColumn(x, z, out var kept));
        Assert.Same(made, kept);
        Assert.Equal(BlockId.Glowstone, game.World.GetBlock(x * Section.Size + 1, 70, z * Section.Size + 1));
    }

    [Fact]
    public void A_Block_Falling_Over_A_Column_That_Unloads_Is_Set_Down_In_It()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using var headless = new HeadlessGame(save: folder);
            var game = headless.Game;
            game.Falling.Drop(BlockId.Sand, 2, 12, 2);
            headless.Step(1);

            Go(headless, 5000.5, 0.5);
            Go(headless, 0.5, 0.5);

            Assert.Equal(BlockId.Sand, game.World.GetBlock(2, 4, 2));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void The_Mobs_Of_A_Column_Left_Are_Kept_By_The_Save_And_Come_Back_With_It()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            var pig = new LandPlace(2.5, 4, 2.5);
            bool Back(VoxelGame game) => game.Mobs.All.Any(mob => mob.Kind == MobKinds.Pig && LandOrigin.LandOf(mob.Body.Position) == pig);
            using (var headless = new HeadlessGame(save: folder))
            {
                var game = headless.Game;
                game.Mobs.Still = true;
                game.Mobs.Add(new MobRecord(MobKinds.Pig, 0, pig, 0, MobKinds.Pig.Health, null));

                // Its column unloads, the pig held by the save alone, and loads again.
                Go(headless, 5000.5, 0.5);
                Assert.Equal(0, game.Mobs.Kept);
                Assert.DoesNotContain(game.Mobs.Columns(), column => column.Column == (0, 0));
                Go(headless, 0.5, 0.5);
                Assert.True(Back(game));

                // And unloads again, the world closed far from it.
                Go(headless, 5000.5, 0.5);
            }

            using var again = new HeadlessGame(save: folder);
            again.Game.Mobs.Still = true;
            Go(again, 0.5, 0.5);

            Assert.True(Back(again.Game));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void The_Mobs_Hold_The_Loaded_Columns_Alone_However_Far_The_Player_Goes()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using var headless = new HeadlessGame(save: folder);
            var game = headless.Game;
            var held = new List<(int Columns, int Loaded, int Regions)>();
            for (int step = 1; step <= 12; step++)
            {
                Go(headless, step * 2000.5, 0.5);
                game.SaveWorld();
                held.Add((game.Mobs.Columns().Count(), game.World.ColumnCount, game.Save!.MobRegions));
            }

            // The columns kept loaded at a render distance of 2 lie in four regions at most.
            Assert.All(held, h => Assert.Equal(h.Loaded, h.Columns));
            Assert.All(held, h => Assert.InRange(h.Regions, 1, 4));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    // Puts the player at a place of the land as voxel.tp does and steps the game until it stands.
    private static void Go(HeadlessGame headless, double x, double z)
    {
        var game = headless.Game;
        var to = new LandPlace(x, 5, z);
        game.Leave(to);
        game.Recenter(to);
        game.Player.Teleport(LandOrigin.PlaceOf(to));
        headless.Step(2);
        var clock = System.Diagnostics.Stopwatch.StartNew();
        while ((game.Loading || game.Streamer.Pending > 0) && clock.Elapsed.TotalSeconds < 30)
        {
            headless.Step(1);
            Thread.Sleep(2);
        }
    }

    [Fact]
    public void A_World_Whose_Hour_Is_Past_A_Days_Count_Opens_Within_A_Day()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using (new HeadlessGame(save: folder)) { }
            var path = Path.Combine(folder, "world.json");
            var json = JsonNode.Parse(File.ReadAllText(path))!;
            (json["Hour"], json["Day"]) = (1e30, 0);
            File.WriteAllText(path, json.ToJsonString());

            var (day, hour) = (-1, -1f);
            // Opened on a thread of its own, so a loop that never ends fails the test rather than
            // holding it.
            var opening = new Thread(() =>
            {
                using var again = new HeadlessGame(save: folder);
                again.Game.Sky.Cycle = true;
                again.Step(3);
                (day, hour) = (again.Game.Sky.Day, again.Game.Sky.Hour);
            }) { IsBackground = true };
            opening.Start();

            Assert.True(opening.Join(TimeSpan.FromSeconds(60)));
            Assert.InRange(day, 0, 1);
            Assert.InRange(hour, 0, 24);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void The_Save_Holds_The_Blocks_Of_The_Regions_About_The_Player_However_Far_It_Goes()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using var headless = new HeadlessGame(save: folder);
            var game = headless.Game;
            var held = new List<int>();
            // Twelve steps of 2,000 blocks, a block changed where the player stands at each.
            for (int step = 1; step <= 12; step++)
            {
                Go(headless, step * 2000.5, 0.5);
                game.World.SetBlock(step * 2000, 70, 0, BlockId.Glowstone);
                game.SaveWorld();
                held.Add(game.Save!.ColumnsHeld);
            }

            Assert.All(held, columns => Assert.InRange(columns, 1, 4));
            // A block of a region let go is read again as its column loads.
            Go(headless, 2000.5, 0.5);
            Assert.Equal(BlockId.Glowstone, game.World.GetBlock(2000, 70, 0));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void Reading_A_Pack_Of_A_Font_Again_Holds_As_Many_Textures()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            // A font sheet of 128 pixels, its glyphs lit squares of five by seven.
            var sheet = new byte[128 * 128 * 4];
            for (int i = 0; i < 128 * 128; i++)
                if (i % 128 % 8 < 5 && i / 128 % 8 < 7) sheet[i * 4 + 3] = 255;
            var at = Path.Combine(folder, "assets", "minecraft", "textures", "font");
            Directory.CreateDirectory(at);
            File.WriteAllBytes(Path.Combine(at, "ascii.png"), ExportImageToMemory(new Image(sheet, 128, 128), ".png"));
            using var headless = new HeadlessGame(packs: [folder]);
            var counts = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                headless.Game.ReloadPacks();
                // The text measured, as a menu does, so the font is made.
                Gui.TextWidth("Options");
                headless.Step(2);
                counts.Add(HeadlessGame.Textures());
            }

            Assert.Equal(counts[0], counts[^1]);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

}
