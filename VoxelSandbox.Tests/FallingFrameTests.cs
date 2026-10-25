using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>Sand and gravel falling where nothing holds them up, as the source's gravity blocks do, and a felled tree's logs landing, as the game's frame lets them fall.</summary>
[Collection(nameof(HeadlessGames))]
public sealed class FallingFrameTests
{
    // A game of frames of a sixtieth of a second, its animals taken away, and a place six blocks
    // from the player along x at the height of its feet on the flat world's ground, where the
    // ground's top stands at 4 in the small worlds the other tests lay.
    private static (HeadlessGame Headless, int X, int Feet, int Z) Game()
    {
        var headless = new HeadlessGame(frameSeconds: 1 / 60f);
        var game = headless.Game;
        game.Mobs.Clear(null);
        var at = game.Player.Body.Position;
        return (headless, (int)MathF.Floor(at.X) + 6, (int)MathF.Floor(at.Y), (int)MathF.Floor(at.Z));
    }

    // Steps the game until every falling block has landed, and a frame more, in which the game drops
    // those that broke.
    private static void Settle(HeadlessGame headless)
    {
        headless.Step(1);
        for (int frame = 0; frame < 600 && headless.Game.Falling.Count > 0; frame++) headless.Step(1);
        Assert.Equal(0, headless.Game.Falling.Count);
        headless.Step(1);
    }

    [Fact]
    public void A_Column_Of_Sand_In_The_Air_Falls_Block_By_Block_Onto_The_Floor()
    {
        var (headless, x, feet, z) = Game();
        using var _ = headless;
        var world = headless.Game.World;
        for (int y = feet + 6; y <= feet + 8; y++) world.SetBlock(x, y, z, BlockId.Sand);
        Assert.Equal(3, world.Loosened.Count);
        Assert.All(Enumerable.Range(feet + 6, 3), y => Assert.Equal(BlockId.Air, world.GetBlock(x, y, z)));
        Settle(headless);
        Assert.All(Enumerable.Range(feet, 3), y => Assert.Equal(BlockId.Sand, world.GetBlock(x, y, z)));
        Assert.Equal(BlockId.Air, world.GetBlock(x, feet + 3, z));

        // Breaking the block under a standing column lets the whole of it fall on.
        world.SetBlock(x + 3, feet, z + 3, BlockId.Stone);
        world.SetBlock(x + 3, feet + 1, z + 3, BlockId.Gravel);
        world.SetBlock(x + 3, feet + 2, z + 3, BlockId.Gravel);
        Assert.Empty(world.Loosened);
        world.SetBlock(x + 3, feet, z + 3, BlockId.Air);
        Settle(headless);
        Assert.Equal(BlockId.Gravel, world.GetBlock(x + 3, feet, z + 3));
        Assert.Equal(BlockId.Gravel, world.GetBlock(x + 3, feet + 1, z + 3));
        Assert.Equal(BlockId.Air, world.GetBlock(x + 3, feet + 2, z + 3));
    }

    [Fact]
    public void Sand_Landing_On_A_Torch_Breaks_Into_Its_Item_And_Gravel_Sinks_Through_Water()
    {
        var (headless, x, feet, z) = Game();
        using var _ = headless;
        var game = headless.Game;
        var world = game.World;
        world.SetBlock(x, feet, z, BlockId.Torch);
        world.SetBlock(x, feet + 4, z, BlockId.Sand);
        Settle(headless);
        Assert.Equal(BlockId.Torch, world.GetBlock(x, feet, z));
        // The sand that broke lies as an item where it landed, as the game drops what the frame sends.
        Assert.Contains(game.Drops.All, drop => drop.Drop.Item == Items.Of(BlockId.Sand));

        for (int y = feet; y <= feet + 2; y++) world.SetBlock(x + 4, y, z + 4, BlockId.Water);
        world.SetBlock(x + 4, feet + 5, z + 4, BlockId.Gravel);
        Settle(headless);
        Assert.Equal(BlockId.Gravel, world.GetBlock(x + 4, feet, z + 4));
    }

    [Fact]
    public void Felling_A_Tree_Takes_Its_Logs_And_Leaves_Some_Logs_Landing_As_Blocks_And_Leaves_Its_Neighbor()
    {
        var (headless, x, feet, z) = Game();
        using var _ = headless;
        var game = headless.Game;
        var world = game.World;
        var leaves = FallingTests.Oak(world, x, z, feet);
        FallingTests.Oak(world, x + 10, z, feet);
        var felling = new TreeFelling();
        world.SetBlock(x, feet, z, BlockId.Air);
        felling.Fell(world, x, feet, z);
        var (broken, dropped) = (0, 0);
        for (int i = 0; i < 6 * 60; i++)
        {
            felling.Update(world, 1 / 60f);
            broken += felling.Broken.Count;
            dropped += felling.Dropped.Count;
            foreach (var (bx, by, bz, block) in felling.Dropped) game.Falling.Drop(block, bx, by, bz);
            felling.Broken.Clear();
            felling.Dropped.Clear();
            headless.Step(1);
        }
        Assert.False(felling.Busy);
        Assert.Equal(4 + leaves, broken + dropped);
        Assert.InRange(dropped, 1, 3);
        // The logs that fell stand on the ground where the trunk stood, one on another.
        Assert.Empty(game.Falling.All);
        Assert.Equal(dropped, Enumerable.Range(feet, 5).Count(y => world.GetBlock(x, y, z) == BlockId.OakLog));
        Assert.Equal(0, Enumerable.Range(z - 2, 5).Sum(lz => Enumerable.Range(x - 2, 5).Sum(lx => Enumerable.Range(feet + 3, 3).Count(y => world.GetBlock(lx, y, lz) == BlockId.OakLeaves))));
        // The oak ten blocks off keeps its logs and its leaves.
        Assert.All(Enumerable.Range(feet, 5), y => Assert.Equal(BlockId.OakLog, world.GetBlock(x + 10, y, z)));
        Assert.Equal(BlockId.OakLeaves, world.GetBlock(x + 12, feet + 3, z + 2));

        // A wall of logs that no leaf touches is no tree, and breaking its foot fells nothing.
        for (int y = feet; y <= feet + 3; y++) world.SetBlock(x + 20, y, z, BlockId.OakLog);
        world.SetBlock(x + 20, feet, z, BlockId.Air);
        felling.Fell(world, x + 20, feet, z);
        for (int i = 0; i < 60; i++) felling.Update(world, 1 / 60f);
        Assert.All(Enumerable.Range(feet + 1, 3), y => Assert.Equal(BlockId.OakLog, world.GetBlock(x + 20, y, z)));
    }
}
