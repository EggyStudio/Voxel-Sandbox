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
    public void A_World_A_Test_Opens_By_Its_Name_Alone_Lies_Among_The_Runs_Own_Saves()
    {
        var folder = Directory.CreateTempSubdirectory("voxel-named-").FullName;
        try
        {
            using var headless = new HeadlessGame(save: Path.Combine(folder, "world"));
            headless.Game.SwitchWorld("flat", 2, "named");

            var account = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VoxelSandbox", "saves");
            Assert.False(headless.Game.Save!.Folder.StartsWith(account, StringComparison.Ordinal), $"a test opened {headless.Game.Save.Folder} among the account's saves");
            Assert.Equal(Path.Combine(Path.GetTempPath(), Path.GetFileName(WorldSave.Root), "named"), headless.Game.Save.Folder);
        }
        finally
        {
            Directory.Delete(folder, true);
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
    public void A_Timed_Save_Is_Written_On_A_Worker_While_The_Columns_Wait_And_Reads_Back_As_Gathered()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            // Frames of a twentieth of a second, so the timed save comes due within 600.
            using var headless = new HeadlessGame(frameSeconds: 0.05f, save: folder, still: true);
            var game = headless.Game;
            (int X, int Y, int Z)[] changed = [(2, 70, 2), (-30, 60, 17)];
            game.World.SetBlock(changed[0].X, changed[0].Y, changed[0].Z, BlockId.Glowstone);
            game.World.SetBlock(changed[1].X, changed[1].Y, changed[1].Z, BlockId.Stone);

            // The save held at its first file, ten seconds at most, so the frames stepped meanwhile
            // run while it is under way.
            using var held = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            game.Save!.Writing = _ =>
            {
                if (held.IsSet) return;
                held.Set();
                release.Wait(TimeSpan.FromSeconds(10));
            };
            var clock = System.Diagnostics.Stopwatch.StartNew();
            while (!held.IsSet && clock.Elapsed.TotalSeconds < 60) headless.Step(1);
            Assert.True(game.Save.Busy);
            var gathered = LandOrigin.LandOf(game.Player.Body.Position);

            // The changed columns as the save gathered them, then one changed again while the
            // worker writes, which the save holds as gathered and the next writes.
            var columns = changed.Select(c => (X: VoxelWorld.ColumnOf(c.X), Z: VoxelWorld.ColumnOf(c.Z))).ToArray();
            var before = columns.Select(c => game.World.TryGetColumn(c.X, c.Z, out var column) ? Blocks(column) : []).ToArray();
            game.World.SetBlock(changed[0].X, changed[0].Y, changed[0].Z, BlockId.Air);

            // A step of four columns east asks for columns past the last load's reach, which wait
            // while the save is under way.
            var (cx, cz) = LandOrigin.ColumnOf(game.Player.Body.Position);
            var past = (X: cx + 4 + game.RenderDistance + 2, Z: cz);
            Assert.False(game.World.HasColumn(past.X, past.Z));
            game.Player.Teleport(game.Player.Body.Position + new Vector3(64, 0, 0));
            headless.Step(20);
            Assert.True(game.Save.Busy);
            Assert.False(game.World.HasColumn(past.X, past.Z));

            release.Set();
            while ((game.Save.Busy || game.Save.Ended || game.Streamer.Pending > 0) && clock.Elapsed.TotalSeconds < 120)
            {
                headless.Step(1);
                Thread.Sleep(2);
            }
            Assert.Null(game.SaveFault);
            Assert.True(game.World.HasColumn(past.X, past.Z));

            // Read back by a save of its own, the changed columns block for block as gathered, and
            // the player where the save gathered it.
            var read = new WorldSave(folder);
            for (int i = 0; i < columns.Length; i++)
            {
                read.ReadRegion(columns[i].X, columns[i].Z);
                Assert.Equal(before[i], Blocks(WorldSave.Decode(columns[i].X, columns[i].Z, read.Kept(columns[i].X, columns[i].Z)!)));
            }
            var info = read.ReadInfo()!;
            Assert.Equal((gathered.X, gathered.Z), (info.X, info.Z));
            Assert.True(game.World.TryGetColumn(columns[0].X, columns[0].Z, out var again) && again.Changed);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }

        // A column's blocks, every one in order.
        static BlockId[] Blocks(ChunkColumn column)
        {
            var blocks = new List<BlockId>();
            for (int y = ChunkColumn.Bottom; y < ChunkColumn.Ceiling; y++)
                for (int z = 0; z < Section.Size; z++)
                    for (int x = 0; x < Section.Size; x++)
                        blocks.Add(column.Get(x, y, z));
            return [.. blocks];
        }
    }

    [Fact]
    public void A_Timed_Save_That_Fails_On_Its_Worker_Says_So_And_Waits_Longer()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using var headless = new HeadlessGame(frameSeconds: 0.05f, save: folder, still: true);
            var game = headless.Game;
            game.World.SetBlock(2, 70, 2, BlockId.Glowstone);
            // A folder in the place of the world's file before it takes its name, as a full disk
            // fails a save.
            Directory.CreateDirectory(Path.Combine(folder, "world.json.new"));
            var clock = System.Diagnostics.Stopwatch.StartNew();
            while (game.SaveFault is null && clock.Elapsed.TotalSeconds < 60) headless.Step(1);

            Assert.StartsWith("The world could not be saved", game.Said?.Text);
            Assert.Equal(60, game.SaveWait);
            Directory.Delete(Path.Combine(folder, "world.json.new"));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Save_That_Cannot_Be_Written_Lets_The_Game_Close_Keeps_The_World_At_A_Switch_And_Waits_Longer()
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

            Assert.Equal(folder, game.Save?.Folder);
            Assert.StartsWith("The world could not be saved, so it stays open", game.Said?.Text);
            Assert.Equal(240, game.SaveWait);
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
    public void A_Switch_To_Another_World_After_A_Save_That_Failed_Keeps_The_World_Played_Until_It_Is_Saved()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            var world = Path.Combine(folder, "world");
            using var headless = new HeadlessGame(save: world, still: true);
            var game = headless.Game;
            game.World.SetBlock(2, 70, 2, BlockId.Glowstone);
            // Every write of the save refused, as a full disk refuses it.
            game.Save!.Writing = _ => throw new IOException("a test's full disk");

            game.SwitchWorld("flat", 2, Path.Combine(folder, "other"));

            Assert.Equal(world, game.Save?.Folder);
            Assert.StartsWith("The world could not be saved, so it stays open", game.Said?.Text);
            Assert.Equal(BlockId.Glowstone, game.World.GetBlock(2, 70, 2));
            // The disk freed and the world saved, the block among what it keeps.
            game.Save!.Writing = null;
            game.SaveWorld();
            Assert.Null(game.SaveFault);
            var save = new WorldSave(world);
            save.ReadRegion(0, 0);
            Assert.Equal(BlockId.Glowstone, WorldSave.Decode(0, 0, save.Kept(0, 0)!).Get(2, 70, 2));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void Save_And_Quit_After_A_Save_That_Failed_Keeps_The_Game_Open()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using var headless = new HeadlessGame(save: Path.Combine(folder, "world"), still: true);
            var game = headless.Game;
            game.Save!.Writing = _ => throw new IOException("a test's full disk");

            Assert.False(game.SaveToQuit());
            Assert.StartsWith("The world could not be saved, so the game stays open", game.Said?.Text);
            game.Save!.Writing = null;
            Assert.True(game.SaveToQuit());
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_World_Whose_Info_Cannot_Be_Read_Is_Named_As_It_Is_Left_Out_Of_The_List_And_Opens_Saying_So()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        var world = Path.Combine(folder, "world");
        Directory.CreateDirectory(world);
        File.WriteAllText(Path.Combine(world, "world.json"), "no info");
        var said = new List<string>();
        SetTraceLogCallback((_, line) =>
        {
            if (line.Contains("is left out of the worlds listed")) lock (said) said.Add(line);
        });
        try
        {
            Assert.Empty(WorldSave.Saved(folder));
            Assert.Contains("The world world is left out of the worlds listed", Assert.Single(said));

            using var headless = new HeadlessGame(save: world, still: true);
            Assert.Contains("The world's info could not be read, so its player begins anew", headless.Game.FileFaults);
            Assert.True(File.Exists(Path.Combine(world, "world.json.unread")));
        }
        finally
        {
            SetTraceLogCallback(null);
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Run_Opened_On_A_Data_Folder_Another_Run_Holds_Says_So_And_Writes_Nothing_There()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var world = Path.Combine(folder, "saves", "world");
        try
        {
            // The data folder held by another run.
            using (new FileStream(Path.Combine(folder, "running.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            using (var headless = new HeadlessGame(save: world, settings: folder, still: true))
            {
                var game = headless.Game;
                Assert.Contains("Another run of the game holds its data folder, so this run keeps nothing there", game.FileFaults);
                game.World.SetBlock(2, 70, 2, BlockId.Glowstone);
                game.SaveWorld();
                game.KeepOptions();
            }

            Assert.False(File.Exists(Path.Combine(world, "world.json")));
            Assert.False(File.Exists(Path.Combine(folder, "options.txt")));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Screenshot_Is_Said_Saved_Once_Its_File_Stands_Whole_And_Said_Not_Written_Where_It_Does_Not()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using var headless = new HeadlessGame(save: Path.Combine(folder, "world"), still: true);
            var game = headless.Game;
            game.Shots = new Screenshots(Path.Combine(folder, "screenshots"));

            // A run with no window draws nothing for the engine to write, so the picture is written
            // here as the engine writes it in its place, cut short and then whole.
            game.Screenshot();
            headless.Step(1);
            Assert.DoesNotContain("Saved screenshot", game.Said?.Text ?? "");
            var path = Assert.Single(game.ShotsWaited);
            var png = ExportImageToMemory(new Image(new byte[16 * 16 * 4], 16, 16), ".png");
            File.WriteAllBytes(path, png[..^12]);
            headless.Step(1);
            Assert.DoesNotContain("Saved screenshot", game.Said?.Text ?? "");
            File.WriteAllBytes(path, png);
            headless.Step(1);
            Assert.Equal($"Saved screenshot as {Path.GetFileName(path)}", game.Said?.Text);

            // One never written, said so once its time is past.
            game.Screenshot();
            var clock = System.Diagnostics.Stopwatch.StartNew();
            while (game.ShotsWaited.Count > 0 && clock.Elapsed.TotalSeconds < VoxelGame.ShotSeconds + 10)
            {
                headless.Step(1);
                Thread.Sleep(20);
            }
            Assert.Matches("^The screenshot .* could not be written$", game.Said?.Text);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Felling_Under_Way_As_The_World_Is_Left_Is_Taken_Whole_Before_Its_Save_And_Goes_No_Further()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            var world = Path.Combine(folder, "world");
            using var headless = new HeadlessGame(save: world, still: true);
            var game = headless.Game;
            // An oak felled at its foot and the world switched in the same frame.
            FallingTests.Oak(game.World, 2, 2);
            game.World.SetBlock(2, 4, 2, BlockId.Air);
            game.Felling.Fell(game.World, 2, 4, 2);
            Assert.True(game.Felling.Busy);

            game.SwitchWorld("flat", 2, Path.Combine(folder, "other"));

            Assert.False(game.Felling.Busy);
            var save = new WorldSave(world);
            save.ReadRegion(0, 0);
            var column = WorldSave.Decode(0, 0, save.Kept(0, 0)!);
            for (int y = 5; y <= 8; y++) Assert.NotEqual(BlockId.OakLog, column.Get(2, y, 2));
            Assert.NotEqual(BlockId.OakLeaves, column.Get(2, 9, 2));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void Sand_Loosened_As_The_World_Is_Left_Or_A_Jump_Leaves_Its_Column_Is_Set_Down_As_A_Block()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            var (world, other) = (Path.Combine(folder, "world"), Path.Combine(folder, "other"));
            using var headless = new HeadlessGame(save: world, still: true);
            var game = headless.Game;
            // Sand on stone, the stone broken and the world switched in the same frame.
            game.World.SetBlock(3, 4, 3, BlockId.Stone);
            game.World.SetBlock(3, 5, 3, BlockId.Sand);
            headless.Step(1);
            game.World.SetBlock(3, 4, 3, BlockId.Air);
            game.SwitchWorld("flat", 2, other);

            var save = new WorldSave(world);
            save.ReadRegion(0, 0);
            Assert.Equal(BlockId.Sand, WorldSave.Decode(0, 0, save.Kept(0, 0)!).Get(3, 4, 3));

            // The same in the world switched to, the player jumping far off as the stone breaks,
            // and back.
            Settle(headless);
            game.World.SetBlock(3, 4, 3, BlockId.Stone);
            game.World.SetBlock(3, 5, 3, BlockId.Sand);
            headless.Step(1);
            game.World.SetBlock(3, 4, 3, BlockId.Air);
            game.Leave(new LandPlace(100_000.5, 10, 0.5));
            game.Recenter(new LandPlace(100_000.5, 10, 0.5));
            game.Player.Teleport(LandOrigin.PlaceOf(new LandPlace(100_000.5, 10, 0.5)));
            Settle(headless);
            game.Leave(new LandPlace(0.5, 10, 0.5));
            game.Recenter(new LandPlace(0.5, 10, 0.5));
            game.Player.Teleport(LandOrigin.PlaceOf(new LandPlace(0.5, 10, 0.5)));
            Settle(headless);

            Assert.Equal(BlockId.Sand, game.World.GetBlock(3, 4, 3));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    // Steps the game until the columns about the player are in.
    private static void Settle(HeadlessGame headless)
    {
        var clock = System.Diagnostics.Stopwatch.StartNew();
        do
        {
            headless.Step(1);
            Thread.Sleep(2);
        }
        while ((headless.Game.Loading || headless.Game.Streamer.Pending > 0) && clock.Elapsed.TotalSeconds < 30);
    }

    [Fact]
    public void A_Column_Changed_In_A_World_That_Keeps_No_Save_Holds_Its_Change_Through_A_Jump_Away_And_Back()
    {
        // A transient world, a block set beside the spawn and the player put far off and back.
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        void Jump(LandPlace to)
        {
            game.Leave(to);
            game.Recenter(to);
            game.Player.Teleport(LandOrigin.PlaceOf(to));
            Settle(headless);
        }
        game.World.SetBlock(2, 70, 2, BlockId.Glowstone);
        Jump(new LandPlace(100_000.5, 10, 0.5));
        Jump(new LandPlace(0.5, 10, 0.5));
        Assert.Equal(BlockId.Glowstone, game.World.GetBlock(2, 70, 2));
        Assert.True(game.Streamer.UnsavedBytes > 0);

        // Past the bytes held, the change let go longest ago is given up and the log says so once.
        var said = new List<string>();
        SetTraceLogCallback((_, line) =>
        {
            if (line.Contains("keeps no save, so the changes")) lock (said) said.Add(line);
        });
        try
        {
            game.Streamer.MostUnsavedBytes = 1;
            game.World.SetBlock(3, 70, 3, BlockId.Glowstone);
            Jump(new LandPlace(100_000.5, 10, 0.5));
            Jump(new LandPlace(0.5, 10, 0.5));
            Assert.NotEqual(BlockId.Glowstone, game.World.GetBlock(3, 70, 3));
            Assert.Single(said);
        }
        finally
        {
            SetTraceLogCallback(null);
        }
    }

    [Fact]
    public void Columns_Made_For_A_Place_The_Player_Left_Do_Not_Join_There()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        // A streamer of the game's world asking for the columns about a place far east, then put
        // far west before they join.
        var streamer = new ChunkStreamer(game.World, game.Renderer, game.Mobs, game.Drops, game.Chests, null) { Workers = 8 };
        var (east, west) = (new LandPlace(16_000.5, 40, 0.5), new LandPlace(-16_000.5, 40, 0.5));
        var (ex, ez) = LandOrigin.ColumnOf(LandOrigin.PlaceOf(east));
        streamer.Update(LandOrigin.PlaceOf(east), 1);
        streamer.Leave(west, 1);

        var joinedEast = false;
        var clock = System.Diagnostics.Stopwatch.StartNew();
        do
        {
            streamer.Update(LandOrigin.PlaceOf(west), 1);
            joinedEast |= game.World.HasColumn(ex, ez);
            Thread.Sleep(2);
        }
        while (streamer.Pending > 0 && clock.Elapsed.TotalSeconds < 30);

        Assert.False(joinedEast);
        Assert.True(game.World.HasColumn(VoxelWorld.ColumnOf(-16_001), 0));
    }

    [Fact]
    public void Stacks_Lying_As_The_World_Closes_Lie_There_As_It_Opens_Again_With_Their_Counts_Whole()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            // A spill of three stacks, a worn pickaxe among them, lying a second before the world
            // closes.
            var pickaxe = Items.Find("wooden_pickaxe")!;
            using (var first = new HeadlessGame(save: folder, still: true))
            {
                var game = first.Game;
                game.Drops.Spawn(Items.Stick, 5, LandOrigin.PlaceOf(new LandPlace(2.5, 6, 2.5)), Vector3.Zero);
                game.Drops.Spawn(Items.Coal, 64, LandOrigin.PlaceOf(new LandPlace(4.5, 6, 2.5)), Vector3.Zero);
                game.Drops.Spawn(pickaxe, 1, LandOrigin.PlaceOf(new LandPlace(6.5, 6, 2.5)), Vector3.Zero, wear: 3);
                first.Step(30);
            }

            using var again = new HeadlessGame(save: folder, still: true);
            var lying = again.Game.Drops.All.Select(d => (d.Drop.Item.Key, d.Drop.Count, d.Drop.Wear)).Order().ToList();
            Assert.Equal([("coal", 64, 0), ("stick", 5, 0), ("wooden_pickaxe", 1, 3)], lying);
            Assert.All(again.Game.Drops.All, d => Assert.InRange(d.Drop.Age, 0.5f, 30));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Stack_In_A_Column_Let_Go_And_Saved_Away_Lies_There_Once_As_The_Player_Comes_Back_After_A_Quit()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            var (away, home) = (new LandPlace(100_000.5, 10, 0.5), new LandPlace(0.5, 10, 0.5));
            void Jump(HeadlessGame headless, LandPlace to)
            {
                headless.Game.Leave(to);
                headless.Game.Recenter(to);
                headless.Game.Player.Teleport(LandOrigin.PlaceOf(to));
                Settle(headless);
            }
            // A stack beside the spawn, its column let go by a jump far off, the world saved and
            // closed there, then opened again and the player back.
            using (var first = new HeadlessGame(save: folder, still: true))
            {
                first.Game.Drops.Spawn(Items.Stick, 5, LandOrigin.PlaceOf(new LandPlace(2.5, 6, 2.5)), Vector3.Zero);
                first.Step(5);
                Jump(first, away);
                first.Game.SaveWorld();
            }
            using var again = new HeadlessGame(save: folder, at: away, still: true);
            Jump(again, home);
            again.Game.SaveWorld();
            Settle(again);

            var stick = Assert.Single(again.Game.Drops.All, d => d.Drop.Item == Items.Stick);
            Assert.Equal(5, stick.Drop.Count);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Saved_Stack_Whose_Time_Ran_Out_Is_Not_Laid_Down_And_One_With_Time_Left_Keeps_Its_Age()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            // Two stacks a save keeps beside the spawn, one with no time left and one with thirty
            // seconds.
            var save = new WorldSave(folder);
            save.KeepDrops(0, 0, [new("stick 2", new LandPlace(2.5, 6, 2.5), 0), new("coal 3", new LandPlace(4.5, 6, 2.5), 30)]);
            save.WriteDrops([]);

            using var headless = new HeadlessGame(save: folder, still: true);

            var coal = Assert.Single(headless.Game.Drops.All);
            Assert.Equal((Items.Coal, 3), (coal.Drop.Item, coal.Drop.Count));
            Assert.InRange(coal.Drop.Age, ItemDrop.Lifetime - 30, ItemDrop.Lifetime - 20);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Stack_Thrown_Into_A_Column_Not_Loaded_Is_Kept_By_A_Save_Before_The_Column_Arrives()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            // A stack lying far off where no column is loaded, the world closed, then opened a few
            // blocks from it, out of the player's reach.
            var far = new LandPlace(5_000.5, 6, 0.5);
            using (var first = new HeadlessGame(save: folder, still: true))
            {
                first.Game.Drops.Spawn(Items.Stick, 5, LandOrigin.PlaceOf(far), Vector3.Zero);
                first.Step(2);
            }
            using var again = new HeadlessGame(save: folder, at: new LandPlace(5_006.5, 10, 0.5), still: true);

            var stick = Assert.Single(again.Game.Drops.All, d => d.Drop.Item == Items.Stick);
            Assert.Equal(5, stick.Drop.Count);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void An_Arrow_Flying_Into_A_Column_Not_Loaded_Waits_At_Its_Border_And_Flies_On_As_It_Arrives()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        // The column four east of the spawn's loaded and the fifth not, an arrow loosed east toward
        // the fifth.
        Assert.True(game.World.HasColumn(4, 0));
        Assert.False(game.World.HasColumn(5, 0));
        game.Arrows.Shoot(LandOrigin.PlaceOf(new LandPlace(60.5, 20, 0.5)), new Vector3(40, 10, 0));
        headless.Step(60);

        var waiting = Assert.Single(game.Arrows.All);
        Assert.False(waiting.Stuck);
        Assert.InRange(LandOrigin.LandOf(waiting.Position).X, 78, 80);

        // The player put east, so the fifth column arrives and the arrow flies on.
        game.Player.Teleport(LandOrigin.PlaceOf(new LandPlace(70.5, 10, 0.5)));
        Settle(headless);
        headless.Step(10);
        Assert.True(LandOrigin.LandOf(Assert.Single(game.Arrows.All).Position).X > 80);
    }

    // Copies a save's folder as a run ended at that moment would leave it.
    internal static void CopyFolder(string from, string to)
    {
        foreach (var path in Directory.GetFiles(from, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(to, Path.GetRelativePath(from, path));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(path, target);
        }
    }

    [Fact]
    public void A_Block_Falling_At_A_Save_Is_Kept_Where_It_Falls_And_Lands_Once_Whether_The_Game_Goes_On_Or_Ends_There()
    {
        var root = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        var (folder, ended) = (Path.Combine(root, "world"), Path.Combine(root, "ended"));
        static int Sand(VoxelGame game) => Enumerable.Range(ChunkColumn.Bottom, ChunkColumn.Ceiling - ChunkColumn.Bottom).Count(y => game.World.GetBlock(2, y, 2) == BlockId.Sand);
        static void Landed(HeadlessGame headless)
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            while (headless.Game.Falling.Count > 0 && clock.Elapsed.TotalSeconds < 10) headless.Step(1);
        }
        try
        {
            using (var headless = new HeadlessGame(save: folder, still: true))
            {
                // Sand let fall from high over the spawn, saved as it falls, and the save copied as
                // a run ended then would leave it.
                var game = headless.Game;
                game.Falling.Drop(BlockId.Sand, 2, 100, 2);
                headless.Step(2);
                Assert.Equal(1, game.Falling.Count);
                game.SaveWorld();
                CopyFolder(folder, ended);

                // The game going on, the sand lands and a save keeps it as a block alone.
                Landed(headless);
                game.SaveWorld();
                Assert.Equal(1, Sand(game));
            }
            using (var again = new HeadlessGame(save: folder, still: true))
            {
                Assert.Equal(0, again.Game.Falling.Count);
                Assert.Equal(1, Sand(again.Game));
            }
            using var after = new HeadlessGame(save: ended, still: true);
            Landed(after);
            Assert.Equal(1, Sand(after.Game));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void A_Lit_TNT_At_A_Save_Is_Kept_With_Its_Fuse_And_Burns_On_After_A_Load()
    {
        var root = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        var (folder, ended) = (Path.Combine(root, "world"), Path.Combine(root, "ended"));
        try
        {
            using (var headless = new HeadlessGame(save: folder, still: true))
            {
                // A TNT lit high over the spawn with a minute's fuse, saved as it burns, and the
                // save copied as a run ended then would leave it.
                headless.Game.Tnt.Light(2, 100, 2, fuse: 60);
                headless.Step(2);
                headless.Game.SaveWorld();
                CopyFolder(folder, ended);
            }

            using var after = new HeadlessGame(save: ended, still: true);
            var lit = Assert.Single(after.Game.Tnt.All);
            Assert.InRange(lit.Lit.Left, 50, 60);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void A_Fire_Keeps_Its_Age_Across_A_Load()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using (var headless = new HeadlessGame(save: folder, still: true))
            {
                // A fire on magma, which never puts it out, at the ninth of its ages as the world
                // closes.
                var game = headless.Game;
                game.World.SetBlock(2, 4, 2, BlockId.Magma);
                Assert.True(game.Fires.Light(game.World, 2, 5, 2, new Random(1)));
                headless.Step(1);
                foreach (var (id, _) in headless.Ecs.Query<Fire>()) headless.Ecs.GetRef<Fire>(id).Age = 9;
            }

            using var again = new HeadlessGame(save: folder, still: true);
            var fire = Assert.Single(again.Game.Fires.All);
            Assert.Equal((2, 5, 2), fire.At);
            Assert.True(fire.Age >= 9, $"the fire's age is {fire.Age}");
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
            // A run with no window holds no monitor's modes nor a borderless state, so it shows the
            // resolution read, the mode being the engine's window's alone.
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
    public void A_Section_The_Flood_Leaves_Out_Stands_In_The_Shadow_Maps_And_The_Field()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        game.Mode = GameMode.Spectator;
        // The sun's shadows held to a block, so no section is marked to cast one and the stone's
        // standing is the culling's alone.
        game.Light.ShadowDistance = 1;
        // Stone in the air over the flat world's floor, its section seen and meshed from above.
        for (int y = 20; y < 24; y++)
            for (int z = 6; z < 10; z++)
                for (int x = 6; x < 10; x++) game.World.SetBlock(x, y, z, BlockId.Stone);
        var stone = new SectionKey(0, 1, 0);
        Stand(new LandPlace(8.5, 40, 2.5));
        Assert.True(game.Renderer.Sees(stone));
        Assert.All(Standing(), part => Assert.Equal((true, false), part));

        // Under the floor, from where the flood reaches nothing over it, the stone stands in the
        // shadow maps and the field alone, which the light reads as it reads a drawn one.
        Stand(new LandPlace(8.5, -20, 2.5));
        Assert.False(game.Renderer.Sees(stone));
        Assert.All(Standing(), part => Assert.Equal((true, true), part));

        // With no section culled it is drawn.
        game.Renderer.Cull = false;
        Stand(new LandPlace(8.5, -20, 2.5));
        Assert.All(Standing(), part => Assert.Equal((true, false), part));

        List<(bool Shown, bool ShadowOnly)> Standing()
        {
            var parts = game.Renderer.Standing(stone).ToList();
            Assert.NotEmpty(parts);
            return parts;
        }
        void Stand(LandPlace at)
        {
            game.Player.Teleport(LandOrigin.PlaceOf(at));
            var clock = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                headless.Step(1);
                Thread.Sleep(2);
            }
            while ((game.Streamer.Pending > 0 || game.World.Loaded.Count > 0 || game.World.Reshaded.Count > 0 || game.Renderer.Meshing > 0) && clock.Elapsed.TotalSeconds < 30);
            headless.Step(5);
        }
    }

    [Fact]
    public void A_Corner_Of_The_Drawn_Reach_A_Step_Brings_Is_Drawn_Soon_After_It_With_No_Send_Failing()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        game.Mode = GameMode.Spectator;
        game.RenderDistance = 4;
        var renderer = game.Renderer;
        Go(4.5);
        Stand();
        // Column 5, two rows aside, lies within the drawn reach of column 1, and column 6 three
        // rows aside, the corner's diagonal neighbor, outside the load circle of column 0.
        var corner = new SectionKey(5, 0, 2);
        var failed = renderer.Ungathered;

        // East at a fifth of a block a frame over column 1's face.
        var (frame, stepped, drawn) = (0, -1, -1);
        for (var tenth = 47; tenth <= 260 && drawn < 0; tenth += 2, frame++)
        {
            Go(tenth / 10.0);
            if (stepped < 0 && tenth >= 160) stepped = frame;
            if (stepped >= 0 && Drawn()) drawn = frame;
        }

        Assert.True(stepped >= 0 && drawn >= 0, $"stepped at {stepped}, drawn at {drawn}");
        Assert.True(drawn - stepped <= Frames, $"drawn {drawn - stepped} frames after the step");
        Assert.Equal(failed, renderer.Ungathered);

        bool Drawn() => renderer.Sees(corner) && renderer.Standing(corner).ToList() is { Count: > 0 } parts && parts.All(p => p.Shown && !p.ShadowOnly);
        void Go(double x)
        {
            game.Player.Teleport(LandOrigin.PlaceOf(new LandPlace(x, 10, 8.5)));
            headless.Step(1);
            Thread.Sleep(2);
        }
        void Stand()
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                headless.Step(1);
                Thread.Sleep(2);
            }
            while ((game.Streamer.Pending > 0 || game.World.Loaded.Count > 0 || renderer.Meshing > 0) && clock.Elapsed.TotalSeconds < 30);
        }
    }

    // The frames after the step a corner section is drawn by, the step's own or the one after.
    private const int Frames = 1;

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void The_Sections_The_Flood_Made_Ahead_Sees_Are_Taken_In_No_More_Than_The_Release_A_Frame(bool cutShort, bool markedAgain)
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        game.Mode = GameMode.Spectator;
        game.RenderDistance = 4;
        // No section cast toward the light, so the ridge waits on the floods alone, the flood ahead
        // finishing in the frame it begins.
        game.Light.ShadowDistance = 1;
        var renderer = game.Renderer;
        renderer.FloodBudgetMs = double.PositiveInfinity;
        // A step east asks for column 1's circle too, every column of which joins before the ridge
        // is raised, so no join marks the ridge's sections again on the way.
        Go(4.5);
        Go(4.7);
        Stand();
        // A ridge of stone in the air over columns 5 and 6, a block in each of its sections, past
        // the flood's reach from column 0 and within or beside it from column 1, so its sections
        // are held back as unseen until the flood made ahead of the step into column 1 sees them.
        var ridge = new List<SectionKey>();
        for (int cx = 5; cx <= 6; cx++)
            for (int cz = -3; cz <= 3; cz++)
                for (int sy = 1; sy <= 8; sy++)
                {
                    game.World.SetBlock(cx * Section.Size + 8, sy * Section.Size + 8, cz * Section.Size + 8, BlockId.Stone);
                    ridge.Add(new SectionKey(cx, sy, cz));
                }
        Stand();
        // East to a block short of where the flood ahead of the step begins, half a column from the
        // face, and still again, so the floods from the eye have reached what they reach.
        for (var tenth = 49; tenth <= 70; tenth += 2) Go(tenth / 10.0);
        Stand();
        var held = ridge.Count(key => !Taken(key));

        // On toward column 1's face at a fifth of a block a frame, each frame's meshes finished
        // before the next, up to the step.
        var (most, taken) = (0, 0);
        // Three frames' meshing in four cut to nothing where asked, as a slow machine's frames run
        // out of their time, so the sections released in them wait for the fourth.
        for (var tenth = 72; tenth < 160; tenth += 2)
        {
            var before = ridge.Count(Taken);
            renderer.MeshMs = cutShort ? (tenth % 8 != 6 ? 0 : null) : markedAgain ? double.PositiveInfinity : null;
            // The ridge's sections marked again where asked over the frames the flood ahead is made
            // in, as columns joining late mark their neighbors' sections, each frame's meshing
            // given all the time it asks.
            if (markedAgain && tenth is >= 84 and <= 90) game.World.Loaded.UnionWith(ridge);
            Go(tenth / 10.0);
            Settle();
            most = Math.Max(most, ridge.Count(Taken) - before);
        }
        taken = ridge.Count(Taken) - (ridge.Count - held);

        // Frames given their time take in more than a frame's release before the step, and frames
        // cut short no more than a release in any one.
        if (!cutShort) Assert.True(taken > ChunkRenderer.ReleasedAFrame, $"{taken} of the {held} held taken in before the step");
        Assert.True(most <= ChunkRenderer.ReleasedAFrame, $"{most} taken in a frame");

        bool Taken(SectionKey key) => renderer.Standing(key).Any();
        void Go(double x)
        {
            game.Player.Teleport(LandOrigin.PlaceOf(new LandPlace(x, 10, 8.5)));
            headless.Step(1);
        }
        void Settle()
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            while (renderer.Meshed < renderer.Meshing && clock.Elapsed.TotalSeconds < 10) Thread.Sleep(1);
        }
        void Stand()
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                headless.Step(1);
                Thread.Sleep(2);
            }
            while ((game.Streamer.Pending > 0 || game.World.Loaded.Count > 0 || game.World.Reshaded.Count > 0 || renderer.Meshing > 0) && clock.Elapsed.TotalSeconds < 30);
        }
    }

    [Fact]
    public void The_Columns_Past_The_Players_Load_Circle_Join_One_A_Frame()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        game.Mode = GameMode.Spectator;
        game.RenderDistance = 4;
        Go(4.5);
        Stand();
        // The load circle reaches the render distance and two more.
        var reach = (game.RenderDistance + 2) * (game.RenderDistance + 2) + game.RenderDistance + 2;

        // East at a fifth of a block a frame over the faces of columns 1 and 2, each step asking
        // for the next column's circle.
        var (most, past) = (0, 0);
        for (var tenth = 47; tenth <= 400; tenth += 2)
        {
            var before = Loaded();
            Go(tenth / 10.0);
            var cx = (int)Math.Floor(tenth / 10.0 / Section.Size);
            var joined = Loaded().Except(before).Count(c => (c.X - cx) * (c.X - cx) + c.Z * c.Z > reach);
            (most, past) = (Math.Max(most, joined), past + joined);
        }

        Assert.True(past > ChunkStreamer.AheadAFrame, $"{past} columns past the load circle joined");
        Assert.True(most <= ChunkStreamer.AheadAFrame, $"{most} columns past the load circle joined in a frame");

        HashSet<(int X, int Z)> Loaded() => [.. game.World.Columns.Select(c => (c.X, c.Z))];
        void Go(double x)
        {
            game.Player.Teleport(LandOrigin.PlaceOf(new LandPlace(x, 10, 8.5)));
            headless.Step(1);
            Thread.Sleep(2);
        }
        void Stand()
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                headless.Step(1);
                Thread.Sleep(2);
            }
            while ((game.Streamer.Pending > 0 || game.World.Loaded.Count > 0) && clock.Elapsed.TotalSeconds < 30);
        }
    }

    [Fact]
    public void A_New_World_Is_Drawn_Behind_The_Loading_Screen_For_Its_Count_Of_Frames_With_The_Player_Landed()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        game.NextWorld = ("flat", 2, null);
        headless.Step(1);
        Assert.True(game.Loading);

        // Each frame of the loading screen, whether the world is drawn behind it, and in those the
        // player's place and the mirrored view's plane, until the world shows.
        var (behind, mirrored, clock) = (0, 0, System.Diagnostics.Stopwatch.StartNew());
        while (game.Loading && clock.Elapsed.TotalSeconds < 30)
        {
            headless.Step(1);
            Thread.Sleep(2);
            if (!game.DrawnBehind) continue;
            behind++;
            if (game.Mirror.Height is not null) mirrored++;
            Assert.Equal(new LandPlace(0.5, game.World.Top(0, 0) + 1, 0.5), LandOrigin.LandOf(game.Player.Body.Position));
        }

        Assert.False(game.Loading);
        Assert.Equal(VoxelGame.BehindFrames, behind);
        Assert.True(mirrored > 0, "the mirrored view was not made ahead behind the loading screen");
        headless.Step(1);
        Assert.False(game.DrawnBehind);
    }

    [Fact]
    public void A_Respawn_In_A_World_Shown_Draws_Nothing_Behind_Its_Loading_Screen()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;

        // The world stands and has been shown, its first frames made, so the respawn's loading
        // screen goes as the spawn stands, the player landed on it.
        game.Respawn();
        var (behind, clock) = (0, System.Diagnostics.Stopwatch.StartNew());
        while (game.Loading && clock.Elapsed.TotalSeconds < 30)
        {
            headless.Step(1);
            Thread.Sleep(2);
            if (game.DrawnBehind) behind++;
        }

        Assert.False(game.Loading);
        Assert.Equal(0, behind);
        Assert.Equal(new LandPlace(0.5, game.World.Top(0, 0) + 1, 0.5), LandOrigin.LandOf(game.Player.Body.Position));
    }

    [Fact]
    public void A_Respawn_In_A_World_Kept_Where_The_Next_Ones_Folder_Cannot_Be_Made_Draws_Nothing_Behind_Its_Loading_Screen()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using var headless = new HeadlessGame(save: Path.Combine(folder, "world"), still: true);
            File.WriteAllText(Path.Combine(folder, "a file"), "");
            var game = headless.Game;
            game.SwitchWorld("flat", 2, Path.Combine(folder, "a file", "other"));
            Assert.Equal(Path.Combine(folder, "world"), game.Save?.Folder);

            // The world kept has been shown since it opened, so its respawn's loading screen goes
            // as the spawn stands.
            game.Respawn();
            var (behind, clock) = (0, System.Diagnostics.Stopwatch.StartNew());
            while (game.Loading && clock.Elapsed.TotalSeconds < 30)
            {
                headless.Step(1);
                Thread.Sleep(2);
                if (game.DrawnBehind) behind++;
            }

            Assert.False(game.Loading);
            Assert.Equal(0, behind);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Column_Whose_Making_Fails_Is_Asked_For_Again_While_The_Player_Stands_Its_Tries_At_Most()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        // A world of its own whose column east of the player's fails to be made twice and is made
        // on the third try, and whose column west fails on every try.
        var tries = new System.Collections.Concurrent.ConcurrentDictionary<(int X, int Z), int>();
        var world = new VoxelWorld(new Laid(column =>
        {
            var tried = tries.AddOrUpdate((column.X, column.Z), 1, (_, count) => count + 1);
            if (column.X == 1 && column.Z == 0 && tried <= 2 || column.X == -1 && column.Z == 0) throw new InvalidOperationException("a test's column");
            Laid.Fill(column, 0, 3, BlockId.Stone);
        }));
        var streamer = new ChunkStreamer(world, game.Renderer, game.Mobs, game.Drops, game.Chests, null);
        var said = new List<string>();
        SetTraceLogCallback((_, line) =>
        {
            if (line.Contains("Column -1, 0 could not be generated in")) lock (said) said.Add(line);
        });
        try
        {
            // The player stands still in the column at the origin, so the plan is made once.
            var at = new Vector3(8.5f, 10, 8.5f);
            var clock = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                streamer.Update(at, 1);
                Thread.Sleep(2);
            }
            while (streamer.Pending > 0 && clock.Elapsed.TotalSeconds < 30);

            Assert.Equal(0, streamer.Pending);
            Assert.True(world.HasColumn(1, 0));
            Assert.Equal(3, tries[(1, 0)]);
            Assert.False(world.HasColumn(-1, 0));
            Assert.Equal(ChunkStreamer.Tries, tries[(-1, 0)]);

            // A plan made again, as the player steps into the next column and back, leaves out the
            // column whose tries ended.
            for (int step = 0; step < 20; step++) streamer.Update(at + new Vector3(Section.Size, 0, 0), 1);
            for (int step = 0; step < 20; step++)
            {
                streamer.Update(at, 1);
                Thread.Sleep(2);
            }
            Assert.Equal(ChunkStreamer.Tries, tries[(-1, 0)]);
            Assert.EndsWith($"Column -1, 0 could not be generated in {ChunkStreamer.Tries} tries, so it is left out of the world.", Assert.Single(said));
        }
        finally
        {
            SetTraceLogCallback(null);
        }
    }

    [Fact]
    public void A_World_Whose_Column_Beside_The_Player_Is_Left_Out_Opens_With_The_Gap_And_Says_So()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        // A flat world whose column east of the spawn's fails on every try.
        game.Generating = generator => new Failing(generator, 1, 0);
        var said = new List<string>();
        SetTraceLogCallback((_, line) =>
        {
            if (line.Contains("Column 1, 0 could not be generated in")) lock (said) said.Add(line);
        });
        try
        {
            game.NextWorld = ("flat", 6, null);
            headless.Step(1);
            var clock = System.Diagnostics.Stopwatch.StartNew();
            while (game.Loading && clock.Elapsed.TotalSeconds < 30)
            {
                headless.Step(1);
                Thread.Sleep(2);
            }

            Assert.False(game.Loading);
            Assert.False(game.World.HasColumn(1, 0));
            Assert.True(game.Streamer.LeftOut(1, 0));
            headless.Step(30);
            Assert.EndsWith($"Column 1, 0 could not be generated in {ChunkStreamer.Tries} tries, so it is left out of the world.", Assert.Single(said));
        }
        finally
        {
            SetTraceLogCallback(null);
        }
    }

    // A world's generator whose one column fails to be made.
    private sealed class Failing(IWorldGenerator generator, int failingX, int failingZ) : IWorldGenerator
    {
        public string Name => generator.Name;

        public int Seed => generator.Seed;

        public ChunkColumn Generate(int columnX, int columnZ) =>
            columnX == failingX && columnZ == failingZ ? throw new InvalidOperationException("a test's column") : generator.Generate(columnX, columnZ);

        public void Paint(ChunkColumn column) => generator.Paint(column);
    }

    [Fact]
    public void Every_Texture_The_Game_Loads_Is_Let_Go_As_It_Closes()
    {
        // The window's close says how many textures the program left loaded, past the engine's own
        // fonts, which it lets go first.
        var said = new List<string>();
        SetTraceLogCallback((_, line) =>
        {
            if (line.Contains("were still loaded")) lock (said) said.Add(line);
        });
        try
        {
            var headless = new HeadlessGame(still: true);
            try
            {
                // The interface's art, which a frame of the HUD draws, among the textures the game
                // holds.
                headless.Step(2);
                var art = GuiArt.Texture();
                Assert.True(IsTextureValid(art));

                headless.Game.Dispose();

                Assert.False(IsTextureValid(art));
            }
            finally
            {
                CloseAudioDevice();
                CloseWindow();
            }
            Assert.Empty(said);
        }
        finally
        {
            SetTraceLogCallback(null);
        }
    }

    [Fact]
    public void A_Mesh_Made_With_The_Atlas_Before_The_Packs_Were_Read_Again_Is_Not_Taken_And_Its_Section_Is_Meshed_Again()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        var renderer = game.Renderer;
        // The workers held as they begin the sections marked to shade again, which they mesh with
        // the atlas set when they were sent.
        using var held = new ManualResetEventSlim();
        var sent = new System.Collections.Concurrent.ConcurrentDictionary<SectionKey, bool>();
        renderer.Building = key =>
        {
            sent[key] = true;
            held.Wait();
        };
        var before = renderer.Atlas;
        game.World.ReshadeAll();
        var clock = System.Diagnostics.Stopwatch.StartNew();
        while (sent.IsEmpty && clock.Elapsed.TotalSeconds < 30)
        {
            headless.Step(1);
            Thread.Sleep(2);
        }
        Assert.False(sent.IsEmpty);

        // The packs read again while the workers mesh, then let go, and every meshing finished
        // before the next frame, which takes them in before it sends any section again.
        var taken = new List<(SectionKey Key, TerrainAtlas Atlas)>();
        renderer.Taking = (key, atlas) => taken.Add((key, atlas));
        game.ReloadPacks();
        Assert.NotSame(before, renderer.Atlas);
        held.Set();
        while (renderer.Meshed < renderer.Meshing && clock.Elapsed.TotalSeconds < 30) Thread.Sleep(1);
        do
        {
            headless.Step(1);
            Thread.Sleep(2);
        }
        while ((renderer.Meshing > 0 || game.World.Reshaded.Count > 0) && clock.Elapsed.TotalSeconds < 60);

        Assert.Equal(0, renderer.Meshing);
        Assert.DoesNotContain(taken, t => t.Atlas == before);
        Assert.Contains(taken, t => sent.ContainsKey(t.Key));
    }

    [Fact]
    public void The_Textures_Of_The_Packs_Interface_And_Of_The_View_Within_A_Wall_Close_With_The_Game()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            // A pack of the hotbar's art and of stone, which the interface and the view within a
            // wall load as they first draw them.
            foreach (var (texture, width, height) in new[] { ("gui/sprites/hud/hotbar", 182, 22), ("block/stone", 16, 16) })
            {
                var path = Path.Combine(folder, "assets", "minecraft", "textures", texture + ".png");
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, ExportImageToMemory(new Image(new byte[width * height * 4], width, height), ".png"));
            }
            var headless = new HeadlessGame(packs: [folder]);
            try
            {
                var hotbar = PackGui.Get("sprites/hud/hotbar")?.Texture;
                var stone = InWall.Texture("block/stone");
                Assert.True(hotbar is { } h && IsTextureValid(h));
                Assert.True(stone is { } s && IsTextureValid(s));

                headless.Game.Dispose();

                Assert.False(IsTextureValid(hotbar!.Value));
                Assert.False(IsTextureValid(stone!.Value));
                Assert.Null(PackGui.Get("sprites/hud/hotbar"));
                Assert.Null(InWall.Texture("block/stone"));
            }
            finally
            {
                CloseAudioDevice();
                CloseWindow();
            }
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Respawn_From_A_Closed_Room_Forgets_The_Rooms_Flood_As_Its_Loading_Begins()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        game.Mode = GameMode.Spectator;
        game.RenderDistance = 4;
        var renderer = game.Renderer;
        renderer.FloodBudgetMs = double.PositiveInfinity;

        // A room of stone closed on every side in the air far east of the spawn, built once the
        // land there stands, and the player then put in it, so the flood from its inside sees its
        // own section alone.
        renderer.Cull = true;
        game.Player.Teleport(LandOrigin.PlaceOf(new LandPlace(200.5, 80, 8.5)));
        Stand();
        for (int x = 198; x <= 202; x++)
            for (int y = 38; y <= 42; y++)
                for (int z = 6; z <= 10; z++)
                    if (x is 198 or 202 || y is 38 or 42 || z is 6 or 10) game.World.SetBlock(x, y, z, BlockId.Stone);
        Stand();
        game.Player.Teleport(LandOrigin.PlaceOf(new LandPlace(200.5, 39, 8.5)));
        Stand();
        headless.Step(5);
        var spawn = new SectionKey(0, 0, 0);
        Assert.False(renderer.Sees(spawn), "the room's flood sees the spawn's ground");

        // The respawn's loading meshes the spawn's sections by a flood that sees them all, none
        // held back by the room's.
        game.Respawn();
        Assert.True(game.Loading);
        Assert.True(renderer.Sees(spawn), "the room's flood holds the spawn's ground unseen as its loading begins");

        void Stand()
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                headless.Step(1);
                Thread.Sleep(2);
            }
            while ((game.Streamer.Pending > 0 || game.World.Loaded.Count > 0 || renderer.Meshing > 0) && clock.Elapsed.TotalSeconds < 30);
        }
    }

    [Fact]
    public void A_Painting_Of_The_Icons_That_Fails_Leaves_Those_Drawn_Before_And_The_Game_Drawing()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        headless.Step(2);
        Assert.True(Icons.Current);
        var drawn = Icons.Texture();

        Icons.PaintStep = () => throw new InvalidOperationException("a test's painting");
        try
        {
            game.ReloadPacks();
            headless.Step(30);
            Assert.False(Icons.Current);
            Assert.Equal(drawn, Icons.Texture());
        }
        finally
        {
            Icons.PaintStep = null;
        }

        // The packs read again paint the icons, which the frame after takes.
        game.ReloadPacks();
        var clock = System.Diagnostics.Stopwatch.StartNew();
        while (!Icons.Current && clock.Elapsed.TotalSeconds < 10)
        {
            headless.Step(1);
            Thread.Sleep(2);
        }
        Assert.True(Icons.Current);
    }

    [Fact]
    public void The_Frames_Behind_The_Loading_Screen_Are_Counted_Again_Where_The_Packs_Are_Read_Again_Among_Them()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        game.NextWorld = ("flat", 4, null);
        headless.Step(1);
        var clock = System.Diagnostics.Stopwatch.StartNew();
        while (game.Loading && !game.DrawnBehind && clock.Elapsed.TotalSeconds < 30)
        {
            headless.Step(1);
            Thread.Sleep(2);
        }
        Assert.True(game.DrawnBehind);
        headless.Step(2);

        // The packs read again mark every section to shade again, so the world stops standing and
        // its loading screen shows alone until it does, then the frames behind it from the first.
        // The frame after is given all the meshing time it asks and comes once the icons painted
        // again on a worker are done, as on a machine whose worker is quick beside its frame, so
        // every marked section is sent in it and nothing else holds the screen alone.
        game.Renderer.MeshMs = double.PositiveInfinity;
        game.ReloadPacks();
        while (!Icons.Painted && clock.Elapsed.TotalSeconds < 60) Thread.Sleep(1);
        var (plain, behind) = (0, 0);
        while (game.Loading && clock.Elapsed.TotalSeconds < 60)
        {
            headless.Step(1);
            Thread.Sleep(2);
            if (!game.Loading) break;
            if (game.DrawnBehind) behind++;
            else (plain, behind) = (plain + 1, 0);
        }

        Assert.False(game.Loading);
        Assert.True(plain > 0, "the world was drawn behind the screen while it did not stand");
        Assert.Equal(VoxelGame.BehindFrames, behind);
    }

    [Fact]
    public void Twenty_Worlds_Opened_In_Turn_With_The_Packs_Read_Again_As_Each_Loads_Stand_With_The_Icons_Of_Their_Packs()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        for (int world = 0; world < 20; world++)
        {
            // The packs read again in the world's first loading frame, the icons painted again
            // while it loads and the sections of the world before still meshing.
            game.NextWorld = ("flat", 100 + world, null);
            headless.Step(1);
            Assert.True(game.Loading);
            game.ReloadPacks();
            var clock = System.Diagnostics.Stopwatch.StartNew();
            while (game.Loading && clock.Elapsed.TotalSeconds < 30)
            {
                headless.Step(1);
                Thread.Sleep(2);
            }
            Assert.False(game.Loading, $"world {world} did not stand");
            headless.Step(1);
            Assert.True(Icons.Current, $"the icons drawn in world {world} are not those of the packs read last");
            Assert.Equal(100 + world, game.World.Generator.Seed);
        }
        Assert.Empty(game.FileFaults);
    }

    [Fact]
    public void The_Water_Mirror_Is_Made_Ahead_Under_The_Eye_For_Its_Count_Of_Frames_As_A_World_Opens()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        headless.Step(WaterMirror.AheadFrames);

        // The flat world holds no water, so a plane stands only where it is made ahead, each frame
        // from the one the next world's loading screen goes in, the opening's own frames passed.
        game.NextWorld = ("flat", 3, null);
        var (set, clock) = (0, System.Diagnostics.Stopwatch.StartNew());
        headless.Step(1);
        Assert.True(game.Loading);
        Count();
        while (game.Loading && clock.Elapsed.TotalSeconds < 30)
        {
            headless.Step(1);
            Thread.Sleep(2);
            Count();
        }
        for (int frame = 0; frame < WaterMirror.AheadFrames * 2; frame++)
        {
            headless.Step(1);
            Count();
        }
        Assert.False(game.Loading);
        Assert.Equal(WaterMirror.AheadFrames, set);

        // With the setting off none is made.
        (game.Mirror.Enabled, set) = (false, 0);
        game.Mirror.MakeAhead();
        for (int frame = 0; frame < WaterMirror.AheadFrames * 2; frame++)
        {
            headless.Step(1);
            Count();
        }
        Assert.Equal(0, set);

        void Count()
        {
            if (game.Mirror.Height is not { } height) return;
            set++;
            Assert.True(height < game.Player.Camera.Position.Y, $"the plane at {height} stands over the eye");
        }
    }

    [Fact]
    public void The_Groups_The_Columns_Unloaded_At_A_Step_Leave_Are_Made_Again_The_Count_A_Frame_After_It()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        game.Mode = GameMode.Spectator;
        game.RenderDistance = 4;
        Go(8.5);
        Stand();

        // Each step east into columns 1 to 4, settled at the column's face first so the step brings
        // no work of the walk, and each frame from the step on, the groups made again wholly behind
        // the column stepped from, two columns across from an even one, where the circle the step
        // asks for joins no column and only the step's unload past the load circle marks a group.
        var (made, column, counting) = (new List<int>(), 0, false);
        game.Renderer.Remade = corner =>
        {
            if (counting && corner.X + 1 < column - 1) made[^1]++;
        };
        var steps = new List<int>();
        for (column = 1; column <= 4; column++)
        {
            for (var x = Section.Size * column - 7.5; x < Section.Size * column - 0.1; x += 0.2) Go(x);
            Stand();
            counting = true;
            steps.Add(made.Count);
            made.Add(0);
            Go(Section.Size * column + 0.1);
            for (var frame = 0; frame < 30; frame++)
            {
                made.Add(0);
                headless.Step(1);
                Thread.Sleep(2);
            }
            counting = false;
        }

        Assert.True(made.Sum() > ChunkRenderer.LeftAFrame, $"{made.Sum()} groups behind the player were made again");
        Assert.True(steps.All(step => made[step] == 0), $"groups behind the player were made again in the steps' frames, {string.Join(" ", steps.Select(step => made[step]))}");
        Assert.True(made.Max() <= ChunkRenderer.LeftAFrame, $"{made.Max()} groups behind the player were made again in a frame, {string.Join(" ", made)}");

        void Go(double x)
        {
            game.Player.Teleport(LandOrigin.PlaceOf(new LandPlace(x, 10, 8.5)));
            headless.Step(1);
            Thread.Sleep(2);
        }
        void Stand()
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                headless.Step(1);
                Thread.Sleep(2);
            }
            while ((game.Streamer.Pending > 0 || game.World.Loaded.Count > 0 || game.Renderer.Meshing > 0) && clock.Elapsed.TotalSeconds < 30);
        }
    }

    [Fact]
    public void The_Circle_Of_The_Next_Column_Is_Asked_For_As_The_Player_Steps_Into_A_Column()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        game.Mode = GameMode.Spectator;
        game.RenderDistance = 4;
        Go(14.5);
        Stand();
        // Column 8 lies within the load circle of column 2 and outside those of columns 0 and 1.
        var (x, far) = (15.5, (X: 8, Z: 0));
        Assert.False(game.World.HasColumn(far.X, far.Z), "the far column is loaded before the step");

        // East over column 1's face at a hundredth of a block a frame, until the far column joins
        // or the player is a quarter column into column 1.
        while (!game.World.HasColumn(far.X, far.Z) && x < 16 + Section.Size / 4) Go(x += 0.01);

        Assert.True(game.World.HasColumn(far.X, far.Z), $"the far column has not joined by {x:0.00}");

        void Go(double at)
        {
            game.Player.Teleport(LandOrigin.PlaceOf(new LandPlace(at, 10, 8.5)));
            headless.Step(1);
            Thread.Sleep(2);
        }
        void Stand()
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                headless.Step(1);
                Thread.Sleep(2);
            }
            while ((game.Streamer.Pending > 0 || game.World.Loaded.Count > 0) && clock.Elapsed.TotalSeconds < 30);
        }
    }

    [Fact]
    public void The_Flood_Made_Ahead_Is_Finished_In_The_Frame_The_Eye_Steps_Into_Its_Section()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        game.Mode = GameMode.Spectator;
        // Floods held to sixteen sections a call, which takes a flood of the flat land some thirty
        // frames, so one begun as the eye steps is not finished in that frame.
        game.Renderer.FloodBudgetMs = 0;
        Go(4.5);
        // Column 4 lies on the ring past the flood's reach that a flood from column 1 marks seen,
        // and past that ring from column 0.
        var brought = new SectionKey(4, 1, 0);
        Assert.False(game.Renderer.Sees(brought));

        // A tenth of a block a frame east up to the face of column 1, the last half section of it
        // flooded ahead, then the frame the eye steps over it.
        for (int tenth = 1; tenth <= 113; tenth++) Go(4.5 + tenth / 10.0);
        Assert.False(game.Renderer.Sees(brought));
        Go(16.05);
        Assert.True(game.Renderer.Sees(brought));

        void Go(double x)
        {
            game.Player.Teleport(LandOrigin.PlaceOf(new LandPlace(x, 10, 8.5)));
            headless.Step(1);
        }
    }

    [Fact]
    public void A_Flood_Made_Ahead_Is_Taken_Only_Where_It_Is_The_Flood_From_The_Eye()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        game.Mode = GameMode.Spectator;
        var renderer = game.Renderer;
        var (taken, stale) = (0, new List<string>());
        renderer.TookAhead = (eye, seen) =>
        {
            taken++;
            var whole = new Visibility();
            whole.Flood(game.World, eye, game.RenderDistance, renderer.Atlas);
            if (!whole.Seen.SetEquals(seen)) stale.Add($"at {eye}, {seen.Count} seen against {whole.Seen.Count}");
        };

        // East over three faces, a turn north short of the next, back east over it, and south, a
        // block set within the reach every 23 frames, some of them between a flood ahead's start
        // and the step it is made for.
        var (x, z) = (4.5, 8.5);
        var frame = 0;
        Fly(0.4, 0, 110);
        Fly(0, -0.4, 50);
        Fly(0.4, 0, 60);
        Fly(0, 0.4, 90);

        Assert.True(taken >= 4, $"{taken} floods made ahead were taken");
        Assert.Empty(stale);

        void Fly(double dx, double dz, int frames)
        {
            for (int i = 0; i < frames; i++, frame++)
            {
                (x, z) = (x + dx, z + dz);
                if (frame % 23 == 11) game.World.SetBlock((int)Math.Floor(x) + 20, 6 + frame % 5, (int)Math.Floor(z), BlockId.Stone);
                game.Player.Teleport(LandOrigin.PlaceOf(new LandPlace(x, 10, z)));
                headless.Step(1);
            }
        }
    }

    [Fact]
    public void Made_Columns_Are_Taken_In_Two_A_Frame_The_Nearest_First()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        // A streamer of the game's world far from the game's own, whose eight workers make the
        // eight columns nearest a place together, as the columns of a batch of the caves' readings
        // are.
        var streamer = new ChunkStreamer(game.World, game.Renderer, game.Mobs, game.Drops, game.Chests, null) { Workers = 8 };
        var at = LandOrigin.PlaceOf(new LandPlace(16_000.5, 40, 0.5));
        var (cx, cz) = LandOrigin.ColumnOf(at);
        streamer.Update(at, 1);
        var clock = System.Diagnostics.Stopwatch.StartNew();
        while (streamer.Made < 8 && clock.Elapsed.TotalSeconds < 30) Thread.Sleep(2);
        Assert.Equal(8, streamer.Made);

        // Of the eight made, the place's column, the four beside it and three on its corners, an
        // update takes the place's column and one beside it, and the next two more beside it.
        Assert.Equal([0L, 1L], Taken());
        Assert.Equal([1L, 1L], Taken());
        while (streamer.Pending > 0 && clock.Elapsed.TotalSeconds < 60)
        {
            Assert.InRange(Taken().Count, 0, ChunkStreamer.JoinedAFrame);
            Thread.Sleep(2);
        }
        Assert.Equal(0, streamer.Pending);

        // The squared distances from the place's column of the columns an update takes in.
        List<long> Taken()
        {
            var before = Loaded();
            streamer.Update(at, 1);
            return [.. Loaded().Except(before).Select(c => (long)(c.X - cx) * (c.X - cx) + (long)(c.Z - cz) * (c.Z - cz)).Order()];
        }
        HashSet<(int X, int Z)> Loaded()
        {
            var loaded = new HashSet<(int X, int Z)>();
            for (int z = cz - 3; z <= cz + 3; z++)
                for (int x = cx - 3; x <= cx + 3; x++)
                    if (game.World.HasColumn(x, z)) loaded.Add((x, z));
            return loaded;
        }
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

    // A pack folder of textures of one size each with the metadata given beside those that have
    // one.
    private static void WritePack(string folder, params (string Texture, int Width, int Height, string? Meta)[] textures)
    {
        foreach (var (texture, width, height, meta) in textures)
        {
            var path = Path.Combine(folder, "assets", "minecraft", "textures", texture + ".png");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, ExportImageToMemory(new Image(new byte[width * height * 4], width, height), ".png"));
            if (meta is not null) File.WriteAllText(path + ".mcmeta", meta);
        }
    }

    [Fact]
    public void A_Game_Whose_Interface_Pack_Holds_Metadata_Of_Other_Kinds_Draws_Its_Sprites_Without_It()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        var said = new List<string>();
        SetTraceLogCallback((_, line) =>
        {
            if (line.Contains("hotbar.png.mcmeta") || line.Contains("button.png.mcmeta")) lock (said) said.Add(line);
        });
        try
        {
            // The hotbar's metadata an array at its root, and the button's a scaling whose type is
            // a number.
            WritePack(folder, ("gui/sprites/hud/hotbar", 182, 22, "[]"),
                ("gui/sprites/widget/button", 200, 20, """{ "gui": { "scaling": { "type": 5 } } }"""));
            using var headless = new HeadlessGame(packs: [folder]);
            headless.Step(3);

            Assert.NotNull(PackGui.Get("sprites/hud/hotbar"));
            Assert.Equal((0, 0), (PackGui.Get("sprites/widget/button")!.Value.Width, PackGui.Get("sprites/widget/button")!.Value.Height));
            Assert.Equal(2, said.Count);
        }
        finally
        {
            SetTraceLogCallback(null);
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Blocks_Metadata_Of_Other_Kinds_Than_It_Takes_Is_Read_As_None_And_Said_Once()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        var said = new List<string>();
        SetTraceLogCallback((_, line) =>
        {
            if (line.Contains(".png.mcmeta, so its texture is read as having none")) lock (said) said.Add(line);
        });
        try
        {
            // A strip of two frames whose frame time is given as text, and one whose frames' height
            // is.
            WritePack(folder, ("block/stone", 16, 32, """{ "animation": { "frametime": "2" } }"""),
                ("block/dirt", 16, 32, """{ "animation": { "height": "16" } }"""));
            using var packs = new ResourcePacks([folder]);

            Assert.Null(packs.Animation("block/stone"));
            Assert.Null(packs.Animation("block/stone"));
            Assert.Equal((16, 32), (packs.Texture("block/dirt")!.Value.Width, packs.Texture("block/dirt")!.Value.Height));
            Assert.Null(packs.Picture("block/dirt")!.Value.Meta);
            Assert.Equal(2, said.Count);
        }
        finally
        {
            SetTraceLogCallback(null);
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Default_Pack_That_Cannot_Be_Read_Counts_As_Missing_And_Is_Said_Once()
    {
        // A file's mode the account cannot read is set on Linux and macOS alone.
        if (OperatingSystem.IsWindows()) return;
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        var list = new PackList(folder);
        byte[] bytes = [1, 2, 3];
        var defaults = new DefaultPacks([new DefaultPack("pack.zip", "pack", "project", "version", "https://cdn.example/pack.zip", bytes.Length,
            Convert.ToHexStringLower(System.Security.Cryptography.SHA512.HashData(bytes)))]);
        Directory.CreateDirectory(list.Folder);
        var path = list.PathOf("pack.zip");
        File.WriteAllBytes(path, bytes);
        var said = new List<string>();
        SetTraceLogCallback((_, line) =>
        {
            if (line.Contains("could not be read, so it counts as missing")) lock (said) said.Add(line);
        });
        File.SetUnixFileMode(path, UnixFileMode.None);
        try
        {
            Assert.True(defaults.Missing(list));
            Assert.True(defaults.Missing(list));
            Assert.Equal(["pack.zip"], defaults.Unreadable);
            Assert.Single(said);
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            Assert.False(defaults.Missing(list));
            Assert.Empty(defaults.Unreadable);
        }
        finally
        {
            SetTraceLogCallback(null);
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
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
