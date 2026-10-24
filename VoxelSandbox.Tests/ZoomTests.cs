using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The zoom its key gives, narrowing the view and the mouse's turn together.</summary>
public class ZoomTests
{
    private const float Frame = 1 / 60f;

    private static void Frames(Zoom zoom, int frames, bool held, float wheel = 0)
    {
        for (int i = 0; i < frames; i++) zoom.Update(Frame, held, i == 0 ? wheel : 0);
    }

    [Fact]
    public void The_Zoom_Narrows_To_A_Quarter_Over_A_Tenth_Of_A_Second_And_Widens_Back_On_Release()
    {
        var zoom = new Zoom();
        Assert.Equal(1, zoom.Share);
        Frames(zoom, 3, held: true);
        Assert.InRange(zoom.Share, 0.6f, 0.65f);
        Frames(zoom, 3, held: true);
        Assert.Equal(Zoom.First, zoom.Share, 4);
        Frames(zoom, 30, held: true);
        Assert.Equal(Zoom.First, zoom.Share, 4);
        Frames(zoom, 3, held: false);
        Assert.InRange(zoom.Share, 0.6f, 0.65f);
        Frames(zoom, 3, held: false);
        Assert.Equal(1, zoom.Share, 4);
    }

    [Fact]
    public void The_Wheel_Narrows_The_Zoom_Further_To_A_Tenth_And_The_Next_Press_Starts_At_A_Quarter()
    {
        var zoom = new Zoom();
        Frames(zoom, 6, held: true);
        Frames(zoom, 1, held: true, wheel: 1);
        Assert.Equal(0.2f, zoom.Share, 4);
        for (int i = 0; i < 10; i++) Frames(zoom, 1, held: true, wheel: 1);
        Assert.Equal(Zoom.Least, zoom.Share, 4);
        Frames(zoom, 1, held: true, wheel: -20);
        Assert.Equal(Zoom.First, zoom.Share, 4);
        Frames(zoom, 1, held: true, wheel: 2);
        Frames(zoom, 6, held: false);
        Frames(zoom, 6, held: true);
        Assert.Equal(Zoom.First, zoom.Share, 4);
    }

    [Fact]
    public void The_Zoomed_View_And_The_Mouse_Turn_Narrow_Together()
    {
        var player = new FirstPersonController();
        var (fov, turn) = (player.Camera.FovY, player.Turn);
        Frames(player.Zoom, 6, held: true);
        Assert.Equal(fov * Zoom.First, player.Camera.FovY, 3);
        Assert.Equal(turn * Zoom.First, player.Turn, 6);
    }
}
