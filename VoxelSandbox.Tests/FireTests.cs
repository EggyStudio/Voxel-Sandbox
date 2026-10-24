using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>Fire burning, spreading and going out, as the source's does.</summary>
public sealed class FireTests
{
    [Fact]
    public void A_Fire_On_Planks_Spreads_And_Burns_Them_Away()
    {
        for (int seed = 1; seed <= 3; seed++)
        {
            var world = Laid.World(c => Laid.Fill(c, 0, 10, BlockId.OakPlanks));
            var fires = new Fires(new Engine.EcsWorld(), new Random(seed));
            Assert.True(fires.Light(world, 8, 11, 8));
            var (most, burned) = Burn(world, fires, 120);
            Assert.True(most >= 5, $"seed {seed} burned at most {most} fires");
            Assert.True(burned >= 3, $"seed {seed} burned {burned} planks away");
        }
    }

    [Fact]
    public void A_Fire_On_Stone_Dies_Where_It_Was_Lit()
    {
        for (int seed = 1; seed <= 3; seed++)
        {
            var world = Laid.World(c => Laid.Fill(c, 0, 10, BlockId.Stone));
            var fires = new Fires(new Engine.EcsWorld(), new Random(seed));
            Assert.True(fires.Light(world, 8, 11, 8));
            var (most, burned) = Burn(world, fires, 120);
            Assert.Equal(1, most);
            Assert.Equal(0, burned);
            Assert.Empty(fires.All);
            Assert.Equal(BlockId.Air, world.GetBlock(8, 11, 8));
        }
    }

    [Fact]
    public void A_Fire_Stands_Over_A_Whole_Block_Or_Beside_One_That_Burns_And_Takes_The_Sides_It_Burns_Toward()
    {
        var world = Laid.World(c => Laid.Fill(c, 0, 10, BlockId.Stone));
        var fires = new Fires(new Engine.EcsWorld(), new Random(1));
        Assert.False(fires.Light(world, 8, 13, 8));
        world.SetBlock(8, 13, 7, BlockId.OakPlanks);
        Assert.True(fires.Light(world, 8, 13, 8));
        Assert.Same(Blocks.FireAmong(BlockId.Air, BlockId.OakPlanks, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air), world.ShapeAt(8, 13, 8));
        Assert.Same(Blocks.FireAmong(BlockId.Stone, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air), Blocks.FireAmong(BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air));
        Assert.Equal(9, Blocks.FireAmong(BlockId.Stone, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air).Pieces.Count);
        Assert.Equal(2, Blocks.FireAmong(BlockId.Air, BlockId.OakPlanks, BlockId.Air, BlockId.Air, BlockId.Air, BlockId.Air).Pieces.Count);
        Assert.False(Fires.Burns(BlockId.Stone));
        Assert.False(Fires.Burns(BlockId.OakSapling));
        Assert.True(Fires.Burns(BlockId.Poppy));
    }

    // Burns for so many seconds a tick at a time, and how many fires burned at once at most and how
    // many planks went.
    private static (int Most, int Burned) Burn(VoxelWorld world, Fires fires, float seconds)
    {
        var most = 0;
        for (int tick = 0; tick < seconds * 20; tick++)
        {
            fires.Take(world);
            fires.Update(world, 0.05f);
            most = Math.Max(most, fires.All.Count());
        }
        var burned = 0;
        for (int y = 0; y <= 10; y++)
            for (int z = -32; z < 48; z++)
                for (int x = -32; x < 48; x++)
                    if (world.GetBlock(x, y, z) is BlockId.Air or BlockId.Fire) burned++;
        return (most, burned);
    }
}
