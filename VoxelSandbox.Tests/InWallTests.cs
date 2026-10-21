using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

public class InWallTests
{
    [Fact]
    public void The_Inside_Of_A_Whole_Opaque_Block_Shows_Where_The_Eye_Is_In_It_But_In_Spectator()
    {
        var world = Laid.World(c => c.Set(0, 0, 0, BlockId.Bedrock));
        BlockId[] blocks = [BlockId.Stone, BlockId.Glowstone, BlockId.OakLeaves, BlockId.Glass, BlockId.Ice, BlockId.Water, BlockId.Torch];
        for (int i = 0; i < blocks.Length; i++) world.SetBlock(i * 2, 20, 0, blocks[i]);
        BlockId? Seen(int i, GameMode mode = GameMode.Survival) => InWall.Block(world, new Vector3(i * 2 + 0.5f, 20.6f, 0.5f), mode);

        Assert.Equal(BlockId.Stone, Seen(0));
        Assert.Equal(BlockId.Stone, Seen(0, GameMode.Creative));
        Assert.Equal(BlockId.Glowstone, Seen(1));
        // Leaves, glass, ice, water and a torch leave the view open, as they do in Minecraft.
        for (int i = 2; i < blocks.Length; i++) Assert.Null(Seen(i));
        // Spectator flies through the blocks and sees past them, and the open air shows nothing.
        Assert.Null(Seen(0, GameMode.Spectator));
        Assert.Null(InWall.Block(world, new Vector3(1.5f, 20.6f, 0.5f), GameMode.Survival));
    }
}
