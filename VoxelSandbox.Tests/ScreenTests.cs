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
