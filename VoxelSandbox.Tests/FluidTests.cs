using Xunit;

namespace VoxelSandbox.Tests;

public class FluidTests
{
    // A floor of stone with its top at 11.
    private static VoxelWorld Floor() => Laid.World(c => Laid.Fill(c, 0, 10, BlockId.Stone));

    [Fact]
    public void A_Source_On_A_Floor_Spreads_Seven_Blocks_Along_It_And_No_Further()
    {
        var world = Floor();
        world.SetBlock(8, 11, 8, BlockId.Water);

        world.Fluids.Update(5);

        for (int d = 1; d <= 7; d++) Assert.Equal(Blocks.FlowingWater(d), world.GetBlock(8 + d, 11, 8));
        Assert.Equal(BlockId.Air, world.GetBlock(16, 11, 8));
        Assert.Equal(BlockId.Air, world.GetBlock(8, 12, 8));
    }

    [Fact]
    public void Water_Off_A_Ledge_Falls_To_The_Floor_And_Spreads_There()
    {
        var world = Floor();
        for (int y = 11; y <= 14; y++) world.SetBlock(8, y, 8, BlockId.Stone);
        world.SetBlock(8, 15, 8, BlockId.Water);

        world.Fluids.Update(5);

        Assert.Equal(BlockId.Water1, world.GetBlock(9, 15, 8));
        for (int y = 11; y <= 14; y++) Assert.Equal(BlockId.FallingWater, world.GetBlock(9, y, 8));
        Assert.Equal(BlockId.Water1, world.GetBlock(10, 11, 8));
    }

    [Fact]
    public void Flowing_Water_Dries_When_Its_Source_Is_Taken_Away()
    {
        var world = Floor();
        world.SetBlock(8, 11, 8, BlockId.Water);
        world.Fluids.Update(5);

        world.SetBlock(8, 11, 8, BlockId.Air);
        world.Fluids.Update(20);

        for (int d = 0; d <= 7; d++) Assert.Equal(BlockId.Air, world.GetBlock(8 + d, 11, 8));
    }

    [Fact]
    public void Two_Sources_With_A_Gap_Between_Them_Make_A_Third_In_It()
    {
        var world = Floor();
        world.SetBlock(8, 11, 8, BlockId.Water);
        world.SetBlock(10, 11, 8, BlockId.Water);

        world.Fluids.Update(5);

        Assert.Equal(BlockId.Water, world.GetBlock(9, 11, 8));
    }

    [Fact]
    public void Flowing_Water_Stands_Lower_The_Further_It_Has_Flowed()
    {
        var world = Floor();
        world.SetBlock(8, 11, 8, BlockId.Water);
        world.Fluids.Update(5);

        var mesher = new SectionMesher();
        Assert.True(mesher.Mesh(world, new SectionKey(0, 0, 0)));
        // The tops of the source and of the water one and two blocks from it along x.
        var faces = mesher.Water;
        var tops = Enumerable.Range(0, faces.Count / 4).Where(q => faces.Unpack(q * 4).Face == 2)
            .Select(q => (Middle: (faces.Positions[q * 4] + faces.Positions[q * 4 + 2]) / 2, Height: faces.Positions[q * 4].Y)).ToArray();
        float TopAt(int x) => tops.Where(t => (int)MathF.Floor(t.Middle.X) == x && (int)MathF.Floor(t.Middle.Z) == 8).Select(t => t.Height).DefaultIfEmpty(-1).Max();
        Assert.True(TopAt(8) > TopAt(9) && TopAt(9) > TopAt(10), $"the tops stand at {TopAt(8)}, {TopAt(9)} and {TopAt(10)}");
    }

    [Fact]
    public void Water_Stopped_At_A_Column_Not_Loaded_Flows_On_As_It_Loads()
    {
        var world = Floor();
        world.Remove(1, 0);
        world.SetBlock(14, 11, 8, BlockId.Water);
        world.Fluids.Update(5);
        Assert.Equal(Blocks.FlowingWater(1), world.GetBlock(15, 11, 8));

        var column = world.Generator.Generate(1, 0);
        Lighting.Compute(column);
        world.Add(column);
        world.Fluids.Update(5);

        Assert.Equal(Blocks.FlowingWater(2), world.GetBlock(16, 11, 8));
        Assert.Equal(Blocks.FlowingWater(4), world.GetBlock(18, 11, 8));
    }

    [Fact]
    public void A_Flood_Half_Spread_As_Its_Column_Unloads_Goes_On_As_The_Column_Arrives_Again()
    {
        // A source spreading two blocks along the floor, its column let go with what waits in it,
        // the rest of the world going on, and the column back.
        var world = Laid.World(c => Laid.Fill(c, 0, 10, BlockId.Stone), 1);
        world.SetBlock(8, 11, 8, BlockId.Water);
        world.Fluids.Update(0.6f);
        Assert.Equal(BlockId.Air, world.GetBlock(15, 11, 8));
        Assert.True(world.TryGetColumn(0, 0, out var column));
        world.Remove(0, 0);
        world.Fluids.Update(1);
        world.Add(column);

        world.Fluids.Update(5);

        Assert.Equal(Blocks.FlowingWater(7), world.GetBlock(15, 11, 8));
    }

    [Fact]
    public void Water_Fed_Across_The_Edge_Of_A_Column_Not_Loaded_Keeps_Its_Level_Until_It_Arrives()
    {
        // A source on the west edge of the column at x 1, its water flowing west into the column at
        // the origin, then the source's column let go and the water beside the edge asked again.
        var world = Laid.World(c => Laid.Fill(c, 0, 10, BlockId.Stone), 1);
        world.SetBlock(16, 11, 8, BlockId.Water);
        world.Fluids.Update(5);
        Assert.Equal(Blocks.FlowingWater(1), world.GetBlock(15, 11, 8));
        Assert.True(world.TryGetColumn(1, 0, out var column));
        world.Remove(1, 0);
        world.Fluids.Around(15, 11, 8);
        world.Fluids.Update(5);

        Assert.Equal(Blocks.FlowingWater(1), world.GetBlock(15, 11, 8));
        Assert.Equal(Blocks.FlowingWater(2), world.GetBlock(14, 11, 8));
        world.Add(column);
        world.Fluids.Update(5);
        Assert.Equal(Blocks.FlowingWater(1), world.GetBlock(15, 11, 8));
    }

    [Fact]
    public void A_Column_Arriving_With_Flowing_Water_On_Its_Edge_Lets_It_Flow_On_And_Its_Source_Stay()
    {
        // The column at x 1 comes with a source and a level of water flowing from it at its west
        // edge, and a source alone at its edge further along than the flow reaches, as a save
        // keeps them, beside air.
        var world = Laid.World(c =>
        {
            Laid.Fill(c, 0, 10, BlockId.Stone);
            if (c.X != 1 || c.Z != 0) return;
            c.Set(1, 11, 13, BlockId.Water);
            c.Set(0, 11, 13, Blocks.FlowingWater(1));
            c.Set(0, 11, 2, BlockId.Water);
        });

        world.Fluids.Update(5);

        Assert.Equal(Blocks.FlowingWater(2), world.GetBlock(15, 11, 13));
        Assert.Equal(BlockId.Air, world.GetBlock(15, 11, 2));
    }
}
