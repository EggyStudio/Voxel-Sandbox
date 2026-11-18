using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>
/// Where a fire stands and the cells it is found by, in small worlds of their own.
/// </summary>
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

    [Fact]
    public void Rain_Puts_Out_A_Fire_Open_To_The_Sky_Where_It_Rains_But_Not_Under_A_Roof_Over_Magma_Or_In_The_Desert()
    {
        // Five fires on stone: one open to the sky, one under a roof of stone over it and its
        // sides, one over magma, one in a desert's columns and one open to the sky that the rain
        // misses.
        var world = Laid.World(c =>
        {
            Laid.Fill(c, 0, 10, BlockId.Stone);
            for (int x = 6; x <= 8; x++)
                for (int z = 1; z <= 3; z++)
                    c.Set(x, 14, z, BlockId.Stone);
            c.Set(12, 10, 2, BlockId.Magma);
            for (int x = 1; x <= 3; x++)
                for (int z = 9; z <= 11; z++)
                    c.Biome[z * Section.Size + x] = (byte)Array.IndexOf(Biomes.All, Biomes.Desert);
        });
        var ecs = new Engine.EcsWorld();
        var fires = new Fires(ecs);
        (int X, int Y, int Z)[] cells = [(2, 11, 2), (7, 11, 2), (12, 11, 2), (2, 11, 10), (12, 11, 10)];
        foreach (var (x, y, z) in cells) Assert.True(fires.Light(world, x, y, z, new Random(1)));

        // Each fire's tick, every chance drawn at its lowest, the rain falling on all but the last.
        var index = ecs.Index<Fire, (int X, int Y, int Z)>();
        foreach (var at in cells)
        {
            Assert.True(index.TryGet(at, out var id));
            Fires.Burn(ecs, world, id, new Lowest(), [], raining: at != (12, 11, 10));
        }
        Assert.Equal(BlockId.Air, world.GetBlock(2, 11, 2));
        Assert.Equal(BlockId.Fire, world.GetBlock(7, 11, 2));
        Assert.Equal(BlockId.Fire, world.GetBlock(12, 11, 2));
        Assert.Equal(BlockId.Fire, world.GetBlock(2, 11, 10));
        Assert.Equal(BlockId.Fire, world.GetBlock(12, 11, 10));
    }

    // Draws every chance at its lowest, so a fire goes out wherever the rain may put it out.
    private sealed class Lowest : Random
    {
        protected override double Sample() => 0;
    }

    [Fact]
    public void A_Rail_And_A_Cobweb_Do_Not_Burn_Where_The_Plants_Of_Their_Shapes_Do()
    {
        Assert.All(Blocks.All.Where(block => block.Item is BlockId.Rail or BlockId.Cobweb), block => Assert.False(Fires.Burns(block.Id), block.Key));
        Assert.True(Fires.Burns(BlockId.CaveVines));
        Assert.True(Fires.Burns(BlockId.MossCarpet));
    }
}
