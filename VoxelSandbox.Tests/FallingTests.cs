using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>Sand and gravel falling where nothing holds them up, as the source's gravity blocks do.</summary>
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
}
