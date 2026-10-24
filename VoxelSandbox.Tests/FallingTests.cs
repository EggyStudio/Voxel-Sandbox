using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>Sand and gravel falling where nothing holds them up, as the source's gravity blocks do, and trees felled.</summary>
public sealed class FallingTests
{
    // Stone to 3, the ground's top at 4, the rest open sky.
    private static VoxelWorld Floor() => Laid.World(c => Laid.Fill(c, 0, 3, BlockId.Stone));

    private static void Settle(VoxelWorld world, FallingBlocks falling)
    {
        falling.Take(world);
        for (int i = 0; i < 600 && falling.All.Count > 0; i++) falling.Update(world, 1 / 60f);
    }

    [Fact]
    public void A_Column_Of_Sand_In_The_Air_Falls_Block_By_Block_Onto_The_Floor()
    {
        var world = Floor();
        var falling = new FallingBlocks();
        for (int y = 10; y <= 12; y++) world.SetBlock(2, y, 2, BlockId.Sand);
        Assert.Equal(3, world.Loosened.Count);
        Assert.All(Enumerable.Range(10, 3), y => Assert.Equal(BlockId.Air, world.GetBlock(2, y, 2)));
        Settle(world, falling);
        Assert.Empty(falling.All);
        Assert.Empty(falling.Broken);
        Assert.All(Enumerable.Range(4, 3), y => Assert.Equal(BlockId.Sand, world.GetBlock(2, y, 2)));
        Assert.Equal(BlockId.Air, world.GetBlock(2, 7, 2));

        // Breaking the block under a standing column lets the whole of it fall on.
        world.SetBlock(5, 4, 5, BlockId.Stone);
        world.SetBlock(5, 5, 5, BlockId.Gravel);
        world.SetBlock(5, 6, 5, BlockId.Gravel);
        Assert.Empty(world.Loosened);
        world.SetBlock(5, 4, 5, BlockId.Air);
        Settle(world, falling);
        Assert.Equal(BlockId.Gravel, world.GetBlock(5, 4, 5));
        Assert.Equal(BlockId.Gravel, world.GetBlock(5, 5, 5));
        Assert.Equal(BlockId.Air, world.GetBlock(5, 6, 5));
    }

    [Fact]
    public void Sand_Landing_On_A_Torch_Breaks_Into_Its_Item_And_Gravel_Sinks_Through_Water()
    {
        var world = Floor();
        var falling = new FallingBlocks();
        world.SetBlock(2, 4, 2, BlockId.Torch);
        world.SetBlock(2, 8, 2, BlockId.Sand);
        Settle(world, falling);
        Assert.Equal(BlockId.Torch, world.GetBlock(2, 4, 2));
        Assert.Equal((2, 4, 2, BlockId.Sand), Assert.Single(falling.Broken));

        for (int y = 4; y <= 6; y++) world.SetBlock(6, y, 6, BlockId.Water);
        world.SetBlock(6, 9, 6, BlockId.Gravel);
        Settle(world, falling);
        Assert.Equal(BlockId.Gravel, world.GetBlock(6, 4, 6));
    }

    // An oak at a place, five logs up from the ground's top at 4 under a crown of leaves two layers
    // of five by five and a top of three by three, and how many leaves it holds.
    private static int Oak(VoxelWorld world, int x, int z)
    {
        var leaves = 0;
        for (int y = 7; y <= 9; y++)
            for (int dz = -2; dz <= 2; dz++)
                for (int dx = -2; dx <= 2; dx++)
                {
                    if (y == 9 && (Math.Abs(dx) == 2 || Math.Abs(dz) == 2) || y < 9 && dx == 0 && dz == 0) continue;
                    world.SetBlock(x + dx, y, z + dz, BlockId.OakLeaves);
                    leaves++;
                }
        for (int y = 4; y <= 8; y++) world.SetBlock(x, y, z, BlockId.OakLog);
        return leaves;
    }

    [Fact]
    public void Felling_A_Tree_Takes_Its_Logs_And_Leaves_Some_Logs_Landing_As_Blocks_And_Leaves_Its_Neighbor()
    {
        var world = Floor();
        var leaves = Oak(world, 4, 4);
        Oak(world, 14, 4);
        var felling = new TreeFelling();
        world.SetBlock(4, 4, 4, BlockId.Air);
        felling.Fell(world, 4, 4, 4);
        var falling = new FallingBlocks();
        var (broken, dropped) = (0, 0);
        for (int i = 0; i < 6 * 60; i++)
        {
            felling.Update(world, 1 / 60f);
            broken += felling.Broken.Count;
            dropped += felling.Dropped.Count;
            foreach (var (x, y, z, block) in felling.Dropped) falling.Drop(block, x, y, z);
            felling.Broken.Clear();
            felling.Dropped.Clear();
            falling.Update(world, 1 / 60f);
        }
        Assert.False(felling.Busy);
        Assert.Equal(4 + leaves, broken + dropped);
        Assert.InRange(dropped, 1, 3);
        // The logs that fell stand on the ground where the trunk stood, one on another.
        Assert.Empty(falling.All);
        Assert.Equal(dropped, Enumerable.Range(4, 5).Count(y => world.GetBlock(4, y, 4) == BlockId.OakLog));
        Assert.Equal(0, Enumerable.Range(2, 5).Sum(z => Enumerable.Range(2, 5).Sum(x => Enumerable.Range(7, 3).Count(y => world.GetBlock(x, y, z) == BlockId.OakLeaves))));
        // The oak ten blocks off keeps its logs and its leaves.
        Assert.All(Enumerable.Range(4, 5), y => Assert.Equal(BlockId.OakLog, world.GetBlock(14, y, 4)));
        Assert.Equal(BlockId.OakLeaves, world.GetBlock(16, 7, 6));

        // A wall of logs that no leaf touches is no tree, and breaking its foot fells nothing.
        for (int y = 4; y <= 7; y++) world.SetBlock(24, y, 4, BlockId.OakLog);
        world.SetBlock(24, 4, 4, BlockId.Air);
        felling.Fell(world, 24, 4, 4);
        for (int i = 0; i < 60; i++) felling.Update(world, 1 / 60f);
        Assert.All(Enumerable.Range(5, 3), y => Assert.Equal(BlockId.OakLog, world.GetBlock(24, y, 4)));
    }
}
