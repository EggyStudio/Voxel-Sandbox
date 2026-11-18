using System.Numerics;
using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>
/// The inventory's windows and the menus as the game's state shows them, the keys a test presses
/// given through the game's toggles, since the engine gives a test game none.
/// </summary>
[Collection(nameof(HeadlessGames))]
public sealed class ScreenTests : IDisposable
{
    public void Dispose() => Serve(null);

    // The game the commands answer of, as the program sets it, which a test reaches through the
    // setter the program uses.
    private static void Serve(VoxelGame? game) => typeof(GameCommands).GetProperty(nameof(GameCommands.Game))!.SetValue(null, game);

    private static string Run(string line) => ConsoleCommands.Run(line) ?? "";

    [Fact]
    public void The_Light_And_View_Keys_Act_On_No_Screen_And_The_Inventorys_Key_On_No_Death()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        var pressed = new HashSet<Key>();
        game.Pressed = pressed.Contains;
        void Press(params Key[] keys)
        {
            pressed.UnionWith(keys);
            headless.Step(1);
            pressed.Clear();
        }
        var (quality, view) = (game.Light.Quality, game.Player.View);

        Press(Key.E);
        Assert.True(game.InventoryOpen);
        Press(Key.G, Key.F5);
        Assert.Equal((quality, view), (game.Light.Quality, game.Player.View));
        Press(Key.E);
        game.Menus.Open = Menu.Pause;
        Press(Key.G, Key.F5);
        Assert.Equal((quality, view), (game.Light.Quality, game.Player.View));
        game.Menus.Open = Menu.None;
        Press(Key.G, Key.F5);
        Assert.NotEqual(quality, game.Light.Quality);
        Assert.NotEqual(view, game.Player.View);

        game.Mode = GameMode.Survival;
        game.Vitals.Damage(1000, "fell");
        headless.Step(2);
        Press(Key.E);
        Assert.False(game.InventoryOpen);
    }

    [Fact]
    public void A_Chests_Window_Open_As_The_Player_Leaves_For_Far_Land_Keeps_The_Chest_It_Shows()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            using var headless = new HeadlessGame(save: folder, still: true);
            var game = headless.Game;
            Serve(game);
            game.World.SetBlock(0, 4, -2, BlockId.Chest);
            game.Chests.At(0, 4, -2).Slots[0] = new ItemStack(Items.Coal, 8);
            Run("voxel.look 0 -30");
            headless.Step(2);
            Assert.Equal("the chest at 0, 4, -2 is opened", Run("voxel.place"));

            // Far off with the window open, the chest's column let go, and back.
            Run("voxel.tp 100000 10 0.5");
            Stand(headless);
            Run("voxel.tp 0.5 10 0.5");
            Stand(headless);

            Assert.Equal(new ItemStack(Items.Coal, 8), game.Chests.All[(0, 4, -2)].Slots[0]);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    // Steps the game until the columns about the player are in.
    private static void Stand(HeadlessGame headless)
    {
        var clock = System.Diagnostics.Stopwatch.StartNew();
        do
        {
            headless.Step(1);
            Thread.Sleep(2);
        }
        while ((headless.Game.Loading || headless.Game.Streamer.Pending > 0) && clock.Elapsed.TotalSeconds < 30);
    }

    // Every item the player holds, in the inventory and the window, and the world holds lying on
    // the ground, counted by its kind.
    private static SortedDictionary<string, int> Counted(VoxelGame game)
    {
        var counted = new SortedDictionary<string, int>();
        foreach (var (item, count) in game.Inventory.Slots.OfType<ItemStack>().Concat(game.InventoryWindow.Holding).Select(stack => (stack.Item, stack.Count))
            .Concat(game.Drops.All.Select(lying => (lying.Drop.Item, lying.Drop.Count))))
            counted[item.Key] = counted.GetValueOrDefault(item.Key) + count;
        return counted;
    }

    // A count with more of a kind.
    private static SortedDictionary<string, int> With(SortedDictionary<string, int> counted, Item item, int count) =>
        new(counted) { [item.Key] = counted.GetValueOrDefault(item.Key) + count };

    [Fact]
    public void Stacks_The_Window_Holds_Past_A_Full_Inventory_Are_Kept_Once_By_A_Save_With_It_Open_One_With_It_Shut_And_A_Quit()
    {
        var root = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        var (world, open, shut) = (Path.Combine(root, "world"), Path.Combine(root, "open"), Path.Combine(root, "shut"));
        var (planks, stone) = (Items.Of(BlockId.OakPlanks), Items.Of(BlockId.Stone));
        SortedDictionary<string, int> four = [], six = [];
        try
        {
            using (var headless = new HeadlessGame(save: world, still: true))
            {
                var game = headless.Game;
                var pressed = new HashSet<Key>();
                game.Pressed = pressed.Contains;
                void Press(Key key)
                {
                    pressed.Add(key);
                    headless.Step(1);
                    pressed.Clear();
                }
                // Every slot full of stone, the window open with four planks in its grid that the
                // inventory has no room for, and the world saved, the save copied as a run ended
                // then would leave it.
                game.Mode = GameMode.Survival;
                for (int i = 0; i < game.Inventory.Slots.Length; i++) game.Inventory.Slots[i] = new ItemStack(stone, 64);
                Press(Key.E);
                Assert.True(game.InventoryOpen);
                game.InventoryWindow.Small.Slots[0] = new ItemStack(planks, 4);
                four = Counted(game);
                Assert.Equal(4, four[planks.Key]);
                game.SaveWorld();
                HeadlessGameTests.CopyFolder(world, open);
                headless.Step(5);
                Assert.Equal(four, Counted(game));

                // The window shut, the planks thrown, and the world saved again.
                Press(Key.E);
                Assert.False(game.InventoryOpen);
                game.SaveWorld();
                HeadlessGameTests.CopyFolder(world, shut);
                Assert.Equal(four, Counted(game));

                // The window open again with two planks more in its grid as the game quits.
                Press(Key.E);
                game.InventoryWindow.Small.Slots[1] = new ItemStack(planks, 2);
                six = Counted(game);
            }

            foreach (var (folder, counted) in new[] { (open, four), (shut, four), (world, six) })
            {
                using var again = new HeadlessGame(save: folder, still: true);
                Assert.Equal(counted, Counted(again.Game));
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void A_Stack_Dropped_From_The_Window_Is_Thrown_In_Its_World_Before_A_Switch_Or_A_Quit_Saves_It()
    {
        var root = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        var (world, other) = (Path.Combine(root, "world"), Path.Combine(root, "other"));
        SortedDictionary<string, int> left = [], quit = [];
        try
        {
            using (var headless = new HeadlessGame(save: world, still: true))
            {
                // Sticks dropped with Q in the frame before the world is switched, and coal in the
                // frame the game quits once the other world stands.
                var game = headless.Game;
                left = With(Counted(game), Items.Stick, 7);
                game.InventoryWindow.Dropped.Add(new ItemStack(Items.Stick, 7));
                game.SwitchWorld("flat", 2, other);
                Assert.False(Counted(game).ContainsKey(Items.Stick.Key));
                Stand(headless);
                quit = With(Counted(game), Items.Coal, 3);
                game.InventoryWindow.Dropped.Add(new ItemStack(Items.Coal, 3));
            }

            using (var first = new HeadlessGame(save: world, still: true)) Assert.Equal(left, Counted(first.Game));
            using var second = new HeadlessGame(save: other, still: true);
            Assert.Equal(quit, Counted(second.Game));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void A_Felling_Wears_Its_Own_Axe_And_Ends_Where_The_Axe_Leaves_Its_Slot()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        Serve(game);
        game.Mode = GameMode.Survival;
        // A trunk of eight logs before the player under a crown, an axe in the first slot and a
        // pickaxe with two uses left in the second.
        for (int y = 4; y <= 11; y++) game.World.SetBlock(0, y, -2, BlockId.OakLog);
        for (int dz = -3; dz <= -1; dz++)
            for (int dx = -1; dx <= 1; dx++)
                game.World.SetBlock(dx, 12, dz, BlockId.OakLeaves);
        var (axe, pickaxe) = (Items.Find("stone_axe")!, Items.Find("wooden_pickaxe")!);
        game.Inventory.Slots[0] = new ItemStack(axe, 1);
        game.Inventory.Slots[1] = new ItemStack(pickaxe, 1, pickaxe.Durability - 2);
        game.Hotbar.Selected = 0;
        // The items with the trunk's logs, standing or dropped, counted as logs.
        var log = Items.Of(BlockId.OakLog);
        SortedDictionary<string, int> Logged() => With(Counted(game), log, Enumerable.Range(4, 8).Count(y => game.World.GetBlock(0, y, -2) == BlockId.OakLog));
        var before = Logged();
        Run("voxel.look 0 -30");
        headless.Step(2);
        Run("voxel.break");
        Assert.True(game.Felling.Busy);
        var cut = game.Inventory.Slots[0];

        // The axe swapped for the worn pickaxe at once, as a number key over the slot swaps them.
        (game.Inventory.Slots[0], game.Inventory.Slots[1]) = (game.Inventory.Slots[1], game.Inventory.Slots[0]);
        headless.Step(90);

        Assert.False(game.Felling.Busy);
        Assert.Equal(new ItemStack(pickaxe, 1, pickaxe.Durability - 2), game.Inventory.Slots[0]);
        Assert.Equal(cut, game.Inventory.Slots[1]);
        Assert.Equal(BlockId.OakLog, game.World.GetBlock(0, 5, -2));
        Assert.Equal(before, Logged());
    }

    [Fact]
    public void A_Felling_Begun_In_Survival_Wears_Its_Axe_No_Further_In_Creative()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        Serve(game);
        game.Mode = GameMode.Survival;
        // A trunk of eight logs before the player under a crown and an axe with two uses left, cut
        // in survival.
        for (int y = 4; y <= 11; y++) game.World.SetBlock(0, y, -2, BlockId.OakLog);
        for (int dz = -3; dz <= -1; dz++)
            for (int dx = -1; dx <= 1; dx++)
                game.World.SetBlock(dx, 12, dz, BlockId.OakLeaves);
        var axe = Items.Find("stone_axe")!;
        game.Inventory.Slots[0] = new ItemStack(axe, 1, axe.Durability - 2);
        game.Hotbar.Selected = 0;
        Run("voxel.look 0 -30");
        headless.Step(2);
        Run("voxel.break");
        Assert.True(game.Felling.Busy);
        var (cut, before) = (game.Inventory.Slots[0], Counted(game));

        // The game switched to creative as the trunk falls, where its logs drop nothing.
        game.Mode = GameMode.Creative;
        headless.Step(90);

        Assert.Equal(cut, game.Inventory.Slots[0]);
        Assert.Equal(before, Counted(game));
    }

    [Fact]
    public void The_Commands_That_Put_Items_Keep_The_Stacks_They_Take_The_Place_Of_And_Make_Only_Those_Asked()
    {
        using var headless = new HeadlessGame(still: true);
        var game = headless.Game;
        Serve(game);
        game.Mode = GameMode.Survival;
        var (stone, dirt, glass, sand) = (Items.Of(BlockId.Stone), Items.Of(BlockId.Dirt), Items.Of(BlockId.Glass), Items.Of(BlockId.Sand));
        // The counts with the furnace's slots among them.
        game.World.SetBlock(0, 4, -2, BlockId.Furnace);
        var furnace = game.Furnaces.At(0, 4, -2);
        SortedDictionary<string, int> All() => furnace.Slots.OfType<ItemStack>().Aggregate(Counted(game), (counted, stack) => With(counted, stack.Item, stack.Count));

        // A stack chosen in survival over a slot holding five coal.
        Array.Fill(game.Inventory.Slots, null);
        game.Inventory.Slots[game.Hotbar.Selected] = new ItemStack(Items.Coal, 5);
        var before = All();
        Run($"voxel.select {stone.Key}");
        Assert.Equal(With(before, stone, stone.MostInStack), All());

        // Nothing given for a count of none.
        before = All();
        Run("voxel.give stick 0");
        Assert.Equal(before, All());

        // Five sticks given to a full inventory.
        Array.Fill(game.Inventory.Slots, new ItemStack(stone, 64));
        before = All();
        Run("voxel.give stick 5");
        Assert.Equal(With(before, Items.Stick, 5), All());

        // A furnace given dirt to smelt and glass to burn, then sand and glass in their places.
        Array.Fill(game.Inventory.Slots, null);
        before = All();
        Run($"voxel.furnace 0 4 -2 {dirt.Key} 3 {glass.Key} 2");
        Run($"voxel.furnace 0 4 -2 {sand.Key} 4 {glass.Key} 1");
        Assert.Equal(With(With(With(With(before, dirt, 3), glass, 2), sand, 4), glass, 1), All());
    }

    [Fact]
    public void Escape_And_E_In_One_Frame_Shut_The_Window()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        var pressed = new HashSet<Key>();
        game.Pressed = pressed.Contains;
        void Press(params Key[] keys)
        {
            pressed.UnionWith(keys);
            headless.Step(1);
            pressed.Clear();
        }

        Press(Key.E);
        Assert.True(game.InventoryOpen);
        Press(Key.Escape, Key.E);
        Assert.False(game.InventoryOpen);
    }

    [Theory]
    [InlineData("quit")]
    [InlineData("switch")]
    [InlineData("spawning")]
    public void A_Mob_Falling_Over_As_Its_World_Is_Closed_Or_Left_Or_Its_Mobs_Taken_Leaves_Its_Loot_Once(string end)
    {
        var root = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        var world = Path.Combine(root, "world");
        var (wool, mutton, log) = (Items.Of(BlockId.WhiteWool), Items.RawMutton, Items.Of(BlockId.OakLog));
        // The logs of a trunk before the player, standing, fallen as blocks or lying as items.
        int Logs(VoxelGame game)
        {
            var blocks = 0;
            for (int x = -8; x <= 8; x++)
                for (int y = 1; y <= 24; y++)
                    for (int z = -12; z <= 6; z++)
                        if (game.World.GetBlock(x, y, z) == BlockId.OakLog) blocks++;
            return blocks + Counted(game).GetValueOrDefault(log.Key);
        }
        SortedDictionary<string, int> before = [];
        var logs = 0;
        try
        {
            using (var headless = new HeadlessGame(save: world, still: true))
            {
                var game = headless.Game;
                Serve(game);
                game.Mode = GameMode.Survival;
                // A white sheep a few blocks off struck down, which the mobs held still keep
                // falling over.
                var sheep = game.Mobs.Add(new MobRecord(MobKinds.Sheep, 0, LandOrigin.LandOf(game.Player.Body.Position + new Vector3(4, 0, 0)), 0, MobKinds.Sheep.Health, null));
                before = Counted(game);
                Assert.True(sheep.Hit(sheep.Body.Position - Vector3.UnitX, 100));
                Assert.True(sheep.Dying >= 0);
                if (end == "quit")
                {
                    // A trunk of eight logs before the player under a crown, felled with an axe as
                    // the game quits.
                    for (int y = 4; y <= 11; y++) game.World.SetBlock(0, y, -2, BlockId.OakLog);
                    for (int dz = -3; dz <= -1; dz++)
                        for (int dx = -1; dx <= 1; dx++)
                            game.World.SetBlock(dx, 12, dz, BlockId.OakLeaves);
                    game.Inventory.Slots[0] = new ItemStack(Items.Find("stone_axe")!, 1);
                    game.Hotbar.Selected = 0;
                    logs = Logs(game);
                    Run("voxel.look 0 -30");
                    headless.Step(2);
                    Run("voxel.break");
                    Assert.True(game.Felling.Busy);
                }
                else if (end == "switch") game.SwitchWorld("flat", 2, Path.Combine(root, "other"));
                else Run("voxel.spawning off");
            }

            using var again = new HeadlessGame(save: world, still: true);
            var counted = Counted(again.Game);
            Assert.Equal(before.GetValueOrDefault(wool.Key) + 1, counted.GetValueOrDefault(wool.Key));
            Assert.InRange(counted.GetValueOrDefault(mutton.Key) - before.GetValueOrDefault(mutton.Key), 1, 2);
            if (end == "quit") Assert.Equal(logs, Logs(again.Game));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void A_Save_In_The_Frame_A_Mob_Is_Taken_Away_Keeps_Its_Loot()
    {
        var root = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        var (world, copy) = (Path.Combine(root, "world"), Path.Combine(root, "copy"));
        var wool = Items.Of(BlockId.WhiteWool);
        SortedDictionary<string, int> before = [];
        try
        {
            using (var headless = new HeadlessGame(save: world, still: true))
            {
                // A white sheep taken away as its fall ends, its death waiting for the next frame's
                // play as a save comes, and the save copied as a run ended then would leave it.
                var game = headless.Game;
                var sheep = game.Mobs.Add(new MobRecord(MobKinds.Sheep, 0, LandOrigin.LandOf(game.Player.Body.Position + new Vector3(4, 0, 0)), 0, MobKinds.Sheep.Health, null));
                before = Counted(game);
                game.Mobs.Clear(MobKinds.Sheep);
                E3D.GetApp().World.SendEvent(new MobDied(sheep, LandOrigin.LandOf(sheep.Body.Position)));
                game.SaveWorld();
                HeadlessGameTests.CopyFolder(world, copy);
            }

            using var again = new HeadlessGame(save: copy, still: true);
            Assert.Equal(before.GetValueOrDefault(wool.Key) + 1, Counted(again.Game).GetValueOrDefault(wool.Key));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Stacks_Through_A_Save_That_Fails_On_A_Region_File_Are_Kept_Once_And_Held_Until_A_Save_Stands()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        (int X, int Z) near, far;
        try
        {
            using (var headless = new HeadlessGame(save: folder, still: true))
            {
                var game = headless.Game;
                Serve(game);
                // Five sticks lying in a loaded column a few blocks off, and three coal thrown over
                // a column far off that is not loaded, in a region of its own.
                var (sticks, coal) = (game.Player.Body.Position + new Vector3(6, 0.5f, 0), LandOrigin.PlaceOf(new LandPlace(2000.5, 80, 0.5)));
                game.Drops.Spawn(Items.Stick, 5, sticks, Vector3.Zero);
                game.Drops.Spawn(Items.Coal, 3, coal, Vector3.Zero);
                (near, far) = (LandOrigin.ColumnOf(sticks), LandOrigin.ColumnOf(coal));
                Assert.True(game.World.HasColumn(near.X, near.Z));
                Assert.False(game.World.HasColumn(far.X, far.Z));
                headless.Step(2);

                // A folder in the place of the far region's stacks file before it takes its name,
                // so the save fails.
                var blocked = Path.Combine(folder, "regions", $"s.{far.X >> 5}.{far.Z >> 5}.bin.new");
                Directory.CreateDirectory(blocked);
                game.SaveWorld();
                Assert.NotNull(game.SaveFault);
                Directory.Delete(blocked);

                // The player gone far off, so the sticks' column unloads, and a save that stands.
                Run("voxel.tp -3000.5 80 0.5");
                Stand(headless);
                Assert.False(game.World.HasColumn(near.X, near.Z));
                game.SaveWorld();
                Assert.Null(game.SaveFault);
            }

            var read = new WorldSave(folder);
            int Lying(Item item, (int X, int Z) column) =>
                read.TakeDrops(column.X, column.Z)?.Select(record => Inventory.Decode(record.Stack)).Where(stack => stack?.Item == item).Sum(stack => stack!.Value.Count) ?? 0;
            Assert.Equal(5, Lying(Items.Stick, near));
            Assert.Equal(3, Lying(Items.Coal, far));
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Crafting_Tables_Window_Shuts_As_The_Table_Goes()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        Serve(game);
        game.World.SetBlock(0, 4, -2, BlockId.CraftingTable);
        Run("voxel.look 0 -30");
        headless.Step(2);

        Assert.Equal("the crafting table is opened", Run("voxel.place"));
        Assert.True(game.InventoryOpen && game.AtTable);
        // As a blast takes it, the player's window open.
        game.World.SetBlock(0, 4, -2, BlockId.Air);
        headless.Step(2);

        Assert.False(game.InventoryOpen);
        Assert.False(game.AtTable);
    }

    [Fact]
    public void The_Menus_Save_In_A_Run_That_Keeps_None_Says_What_The_Consoles_Save_Answers()
    {
        using var headless = new HeadlessGame();
        Serve(headless.Game);

        headless.Game.SaveAsked();

        Assert.Equal("The world is transient and is not saved", headless.Game.Said?.Text);
        Assert.Equal("the world is transient and is not saved", Run("voxel.save"));
    }

    [Fact]
    public void A_Lists_Wheel_Moves_It_A_Row_For_A_Move_However_Small()
    {
        Assert.Equal(1, Menus.Scrolled(0, -0.25f, 10, 6));
        Assert.Equal(0, Menus.Scrolled(1, 0.4f, 10, 6));
        Assert.Equal(4, Menus.Scrolled(4, -1, 10, 6));
        Assert.Equal(0, Menus.Scrolled(0, -3, 3, 6));
    }
}
