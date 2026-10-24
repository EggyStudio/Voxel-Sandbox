using Xunit;

namespace VoxelSandbox.Tests;

public class LightingTests
{
    // Solid stone from the bottom to 20, the open sky above it.
    private static VoxelWorld Stone() => Laid.World(c => Laid.Fill(c, 0, 20, BlockId.Stone));

    [Fact]
    public void The_Open_Sky_Falls_Down_A_Shaft_Without_Losing_A_Level()
    {
        var world = Stone();
        for (int y = 10; y <= 20; y++) world.SetBlock(8, y, 8, BlockId.Air);

        Assert.Equal(15, world.GetLight(8, 10, 8).Sky);
    }

    [Fact]
    public void A_Roofed_Shaft_With_No_Opening_Goes_Dark()
    {
        var world = Stone();
        for (int y = 10; y <= 20; y++) world.SetBlock(8, y, 8, BlockId.Air);
        world.SetBlock(8, 20, 8, BlockId.Stone);

        for (int y = 10; y < 20; y++) Assert.Equal(0, world.GetLight(8, y, 8).Sky);
    }

    [Fact]
    public void A_Lamp_Lights_A_Level_Less_A_Block_Away_And_Takes_It_Back_When_Broken()
    {
        var world = Stone();
        for (int y = 10; y <= 19; y++) world.SetBlock(8, y, 8, BlockId.Air);
        world.SetBlock(8, 10, 8, BlockId.Glowstone);

        Assert.Equal(15, world.GetLight(8, 10, 8).Block);
        Assert.Equal(14, world.GetLight(8, 11, 8).Block);
        Assert.Equal(10, world.GetLight(8, 15, 8).Block);

        world.SetBlock(8, 10, 8, BlockId.Air);
        for (int y = 10; y <= 19; y++) Assert.Equal(0, world.GetLight(8, y, 8).Block);
    }

    [Fact]
    public void The_Sky_Fades_Two_Levels_For_Each_Block_Of_Water()
    {
        var world = Laid.World(c =>
        {
            Laid.Fill(c, 0, 10, BlockId.Stone);
            Laid.Fill(c, 11, 14, BlockId.Water);
        });

        Assert.Equal(15, world.GetLight(3, 15, 3).Sky);
        Assert.Equal(13, world.GetLight(3, 14, 3).Sky);
        Assert.Equal(11, world.GetLight(3, 13, 3).Sky);
        Assert.Equal(7, world.GetLight(3, 11, 3).Sky);
    }

    [Fact]
    public void A_Canopy_Takes_A_Level_On_The_Way_In_And_One_More_Past_Its_First_Layer_And_Its_Floor_Keeps_The_Rest()
    {
        // A canopy five deep from 20 to 24 over every place, and one a single leaf deep at 24.
        var deep = Laid.World(c =>
        {
            Laid.Fill(c, 0, 3, BlockId.Stone);
            Laid.Fill(c, 20, 24, BlockId.OakLeaves);
        });
        Assert.Equal(14, deep.GetLight(8, 24, 8).Sky);
        Assert.Equal(13, deep.GetLight(8, 23, 8).Sky);
        Assert.Equal(13, deep.GetLight(8, 20, 8).Sky);
        Assert.Equal(13, deep.GetLight(8, 19, 8).Sky);
        Assert.Equal(13, deep.GetLight(8, 4, 8).Sky);

        var thin = Laid.World(c =>
        {
            Laid.Fill(c, 0, 3, BlockId.Stone);
            Laid.Fill(c, 24, 24, BlockId.OakLeaves);
        });
        Assert.Equal(14, thin.GetLight(8, 4, 8).Sky);

        // A hole cut down through the five opens the floor under it to the full sky, and the leaves
        // set back shade it as before.
        for (int y = 20; y <= 24; y++) deep.SetBlock(8, y, 8, BlockId.Air);
        Assert.Equal(15, deep.GetLight(8, 4, 8).Sky);
        for (int y = 20; y <= 24; y++) deep.SetBlock(8, y, 8, BlockId.OakLeaves);
        Assert.Equal(13, deep.GetLight(8, 4, 8).Sky);
        Assert.Equal(13, deep.GetLight(8, 12, 8).Sky);
    }

    [Fact]
    public void Glass_Lets_The_Sky_Through_As_Air_Does()
    {
        var world = Stone();
        for (int y = 10; y <= 20; y++) world.SetBlock(8, y, 8, BlockId.Air);
        world.SetBlock(8, 20, 8, BlockId.Glass);

        Assert.Equal(15, world.GetLight(8, 10, 8).Sky);
    }

    [Fact]
    public void A_Lamp_On_A_Columns_Edge_Lights_The_Column_Beside_It()
    {
        // A lamp at x 15 of every column, so the column at x 1 is lit across its edge at x 16.
        var world = Laid.World(c =>
        {
            Laid.Fill(c, 0, 10, BlockId.Stone);
            c.Set(15, 11, 8, BlockId.Glowstone);
        });

        Assert.Equal(14, world.GetLight(16, 11, 8).Block);
    }
}
