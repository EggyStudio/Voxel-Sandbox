using Xunit;

namespace VoxelSandbox.Tests;

public class ModeTests
{
    [Fact]
    public void Each_Mode_Lets_The_Player_Do_What_Minecrafts_Does()
    {
        Assert.True(GameMode.Creative.Builds() && !GameMode.Creative.Lives() && GameMode.Creative.Touches());
        Assert.True(GameMode.Survival.Builds() && GameMode.Survival.Lives() && GameMode.Survival.Touches());
        // Adventure keeps survival's health and hunger and its hands, and places and breaks nothing.
        Assert.True(!GameMode.Adventure.Builds() && GameMode.Adventure.Lives() && GameMode.Adventure.Touches());
        Assert.True(!GameMode.Spectator.Builds() && !GameMode.Spectator.Lives() && !GameMode.Spectator.Touches());
    }

    [Fact]
    public void The_Switcher_Opens_On_The_Mode_Played_Before_And_Steps_Through_All_Four()
    {
        var switcher = new ModeSwitcher();
        switcher.Step(GameMode.Survival, GameMode.Spectator);
        Assert.True(switcher.Open);
        Assert.Equal(GameMode.Spectator, switcher.Chosen);
        switcher.Step(GameMode.Survival, GameMode.Spectator);
        Assert.Equal(GameMode.Creative, switcher.Chosen);
        switcher.Step(GameMode.Survival, GameMode.Spectator);
        Assert.Equal(GameMode.Survival, switcher.Chosen);
        switcher.Step(GameMode.Survival, GameMode.Spectator);
        Assert.Equal(GameMode.Adventure, switcher.Chosen);

        Assert.Equal(GameMode.Adventure, switcher.Release());
        Assert.False(switcher.Open);
        // F3 let go with the switcher shut plays nothing.
        Assert.Null(switcher.Release());
    }

    [Fact]
    public void With_No_Mode_Played_Before_The_Switcher_Opens_On_Survival_From_Creative_And_On_Creative_From_The_Others()
    {
        foreach (var (now, first) in new[] { (GameMode.Creative, GameMode.Survival), (GameMode.Survival, GameMode.Creative), (GameMode.Spectator, GameMode.Creative) })
        {
            var switcher = new ModeSwitcher();
            switcher.Step(now, null);
            Assert.Equal(first, switcher.Chosen);
        }
    }
}
