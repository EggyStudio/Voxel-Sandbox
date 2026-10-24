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
        for (int i = 0; i < 600 && falling.Count > 0; i++) falling.Update(world, 1 / 60f);
    }

    [Fact]
    public void A_Column_Of_Sand_In_The_Air_Falls_Block_By_Block_Onto_The_Floor()
    {
        var world = Floor();
        var falling = new FallingBlocks(new Engine.EcsWorld());
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
        var falling = new FallingBlocks(new Engine.EcsWorld());
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
        var falling = new FallingBlocks(new Engine.EcsWorld());
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

    // Runs a felling three seconds a frame at a time, counting the logs it drops as items and those
    // it lets fall as blocks.
    private static (int Items, int Falling) Felled(TreeFelling felling, VoxelWorld world)
    {
        var (items, falling) = (0, 0);
        for (int i = 0; i < 3 * 60; i++)
        {
            felling.Update(world, 1 / 60f);
            items += felling.Broken.Count(b => BlockGroups.Logs.Contains(b.Block));
            falling += felling.Dropped.Count;
            felling.Broken.Clear();
            felling.Dropped.Clear();
        }
        return (items, falling);
    }

    [Fact]
    public void Felling_With_An_Axe_Takes_A_Use_For_Each_Log_Dropped_As_An_Item_And_One_Run_Out_Leaves_The_Rest_Standing()
    {
        // The oak's four logs over the cut at its foot, felled with uses to spare, of which the seed's
        // share falls as blocks, one, and the three dropped as items take one use each.
        var world = Floor();
        Oak(world, 4, 4);
        var felling = new TreeFelling();
        world.SetBlock(4, 4, 4, BlockId.Air);
        felling.Fell(world, 4, 4, 4, uses: 100);
        var (items, falling) = Felled(felling, world);
        Assert.Equal((3, 1), (items, falling));
        Assert.Equal(3, felling.Worn);

        // With one use left the first log dropped as an item takes it, and the logs over it stand,
        // with the leaves about them.
        world = Floor();
        Oak(world, 4, 4);
        felling = new TreeFelling();
        world.SetBlock(4, 4, 4, BlockId.Air);
        felling.Fell(world, 4, 4, 4, uses: 1);
        (items, _) = Felled(felling, world);
        Assert.Equal(1, items);
        Assert.Equal(1, felling.Worn);
        Assert.False(felling.Busy);
        Assert.Equal(BlockId.OakLog, world.GetBlock(4, 8, 4));
        Assert.Equal(BlockId.OakLeaves, world.GetBlock(5, 8, 4));
        Assert.Equal(BlockId.OakLeaves, world.GetBlock(4, 9, 4));

        // An axe whose last use was the cut fells nothing.
        world = Floor();
        Oak(world, 4, 4);
        felling = new TreeFelling();
        world.SetBlock(4, 4, 4, BlockId.Air);
        felling.Fell(world, 4, 4, 4, uses: 0);
        Assert.False(felling.Busy);
        Assert.All(Enumerable.Range(5, 4), y => Assert.Equal(BlockId.OakLog, world.GetBlock(4, y, 4)));
    }

    [Fact]
    public void Felling_Takes_A_Lower_Tree_Whose_Branch_Touches_Whole_Its_Crown_Going_From_The_Tick_Its_First_Log_Does()
    {
        // An oak, and a tree beside it in a pit a block lower, its trunk from 3 to 6 and its branch
        // reaching across through the oak's leaves to touch the oak's trunk, a leaf over its top.
        var world = Floor();
        Oak(world, 4, 4);
        for (int y = 3; y <= 6; y++) world.SetBlock(8, y, 4, BlockId.OakLog);
        foreach (var x in (ReadOnlySpan<int>)[7, 6, 5]) world.SetBlock(x, 7, 4, BlockId.OakLog);
        world.SetBlock(8, 7, 4, BlockId.OakLeaves);
        var felling = new TreeFelling();
        world.SetBlock(4, 4, 4, BlockId.Air);
        felling.Fell(world, 4, 4, 4);
        float time = 0, firstLog = -1, firstLeaf = -1, lastLeaf = 0;
        for (int i = 0; i < 4 * 60; i++)
        {
            felling.Update(world, 1 / 60f);
            time += 1 / 60f;
            foreach (var (_, _, _, block) in felling.Broken.Concat(felling.Dropped))
                if (BlockGroups.Logs.Contains(block))
                {
                    if (firstLog < 0) firstLog = time;
                }
                else
                {
                    if (firstLeaf < 0) firstLeaf = time;
                    lastLeaf = time;
                }
            felling.Broken.Clear();
            felling.Dropped.Clear();
        }
        Assert.False(felling.Busy);
        // Neither tree keeps a log, the lower one's under the cut among them, nor a leaf.
        for (int y = 3; y <= 12; y++)
            for (int z = 0; z <= 8; z++)
                for (int x = 0; x <= 12; x++)
                {
                    Assert.False(BlockGroups.Logs.Contains(world.GetBlock(x, y, z)), $"a log stays at {x}, {y}, {z}");
                    Assert.False(Blocks.IsLeaves(world.GetBlock(x, y, z)), $"a leaf stays at {x}, {y}, {z}");
                }
        // The first leaf goes in the frame the first log does, and the crown within the second after.
        Assert.True(firstLog > 0);
        Assert.Equal(firstLog, firstLeaf);
        Assert.InRange(lastLeaf - firstLog, 0.5f, 1);
    }
}
