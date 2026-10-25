using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>Where a fire stands and the cells it is found by, in small worlds of their own.</summary>
public sealed class FireTests
{
    [Fact]
    public void A_Cell_The_World_Names_Again_Keeps_Its_One_Fire_Found_By_Its_Cell()
    {
        var world = Laid.World(c => Laid.Fill(c, 0, 10, BlockId.Stone));
        var ecs = new Engine.EcsWorld();
        var fires = new Fires(ecs);
        Assert.True(fires.Light(world, 8, 11, 8, new Random(1)));
        // The cell lit is named, then named twice more beside a new one.
        world.Kindled.AddRange([(8, 11, 8), (8, 11, 8), (9, 11, 8)]);
        fires.Take(world, new Random(1));
        Assert.Equal(2, ecs.Count<Fire>());
        var index = ecs.Index<Fire, (int X, int Y, int Z)>();
        Assert.True(index.TryGet((9, 11, 8), out var entity));
        Assert.Equal((9, 11, 8), ecs.GetRef<Fire>(entity).Key);
        Assert.False(index.Contains((10, 11, 8)));
    }

    [Fact]
    public void A_Fire_Stands_Over_A_Whole_Block_Or_Beside_One_That_Burns_And_Takes_The_Sides_It_Burns_Toward()
    {
        var world = Laid.World(c => Laid.Fill(c, 0, 10, BlockId.Stone));
        var fires = new Fires(new Engine.EcsWorld());
        Assert.False(fires.Light(world, 8, 13, 8, new Random(1)));
        world.SetBlock(8, 13, 7, BlockId.OakPlanks);
        Assert.True(fires.Light(world, 8, 13, 8, new Random(1)));
        Assert.Same(Blocks.FireAmong(BlockId.Air, BlockId.OakPlanks, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air), world.ShapeAt(8, 13, 8));
        Assert.Same(Blocks.FireAmong(BlockId.Stone, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air), Blocks.FireAmong(BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air));
        Assert.Equal(9, Blocks.FireAmong(BlockId.Stone, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air).Pieces.Count);
        Assert.Equal(2, Blocks.FireAmong(BlockId.Air, BlockId.OakPlanks, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air).Pieces.Count);
        Assert.False(Fires.Burns(BlockId.Stone));
        Assert.False(Fires.Burns(BlockId.OakSapling));
        Assert.True(Fires.Burns(BlockId.Poppy));
    }
}
