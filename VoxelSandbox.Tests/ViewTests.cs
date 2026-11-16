using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The view's turn as the mouse and the commands set it.</summary>
public sealed class ViewTests
{
    [Fact]
    public void A_View_Turned_One_Way_A_Long_Time_Keeps_A_Turns_Fine_Steps()
    {
        var player = new FirstPersonController();
        // Some eleven thousand whole turns one way, as the mouse sums them.
        for (int i = 0; i < 100_000; i++) player.Yaw -= 0.7f;
        Assert.InRange(player.Yaw, -MathF.PI, MathF.PI);

        // A pixel's turn of the mouse at the sensitivity it starts at still turns the view by it.
        var before = player.Yaw;
        player.Yaw -= player.Sensitivity;
        Assert.Equal(before - player.Sensitivity, player.Yaw, 1e-5f);
    }
}
