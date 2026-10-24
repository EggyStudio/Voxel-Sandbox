using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>Swimming by Minecraft's numbers, the bob at the top with Space held and the step out onto a bank.</summary>
public sealed class SwimTests
{
    // The water's top, an eighth under its top block's.
    private const float Surface = 8.875f;

    // Stone to 3 under water from 4 to 8, x 0 to 7 of each column, and a bank from x 8 to 15 whose
    // top stands at a height.
    private static VoxelWorld Pool(int bankTop) => Laid.World(c =>
    {
        Laid.Fill(c, 0, 3, BlockId.Stone);
        for (int y = 4; y <= 8; y++)
            for (int z = 0; z < 16; z++)
                for (int x = 0; x < 8; x++)
                    c.Set(x, y, z, BlockId.Water);
        for (int y = 4; y < bankTop; y++)
            for (int z = 0; z < 16; z++)
                for (int x = 8; x < 16; x++)
                    c.Set(x, y, z, BlockId.Stone);
    });

    private static FirstPersonController Swimmer(float x) => new() { MayFly = false, Heading = 90, Body = { Position = new Vector3(x, 4, 4.5f) } };

    [Theory]
    [InlineData(1 / 60f)]
    [InlineData(1 / 20f)]
    public void Space_Held_In_Deep_Water_Bobs_At_The_Top_Within_The_Sources_Band(float frame)
    {
        var world = Pool(9);
        var player = Swimmer(4.5f);
        var heights = new List<float>();
        for (float time = 0; time < 10; time += frame)
        {
            player.Step(world, frame, new FirstPersonController.Controls(Up: true));
            if (time > 3) heights.Add(player.Body.Position.Y);
        }
        // Minecraft's ticks bob the feet from 0.74 under the surface to 0.28 over it, coming out of
        // the water and falling back in, and never launch the body out.
        Assert.InRange(heights.Min(), Surface - 0.8f, Surface - 0.5f);
        Assert.InRange(heights.Max(), Surface + 0.15f, Surface + 0.35f);
    }

    [Fact]
    public void Without_Space_A_Swimmer_Sinks_Slowly()
    {
        var world = Pool(9);
        var player = Swimmer(4.5f);
        player.Body.Position = new Vector3(4.5f, 8, 4.5f);
        for (int i = 0; i < 60; i++) player.Step(world, 1 / 60f, new FirstPersonController.Controls());
        var at = player.Body.Position.Y;
        for (int i = 0; i < 60; i++) player.Step(world, 1 / 60f, new FirstPersonController.Controls());
        // Toward half a block a second, as 0.005 a tick against a drag of 0.8 holds it.
        Assert.InRange(at - player.Body.Position.Y, 0.45f, 0.55f);
    }

    [Theory]
    [InlineData(1 / 60f)]
    [InlineData(1 / 20f)]
    public void Swimming_Up_Into_A_Bank_Level_With_The_Water_Steps_Out_And_One_A_Block_Higher_Is_Out_Of_Reach(float frame)
    {
        // A bank whose top is the water's top block's, a step over the surface.
        var world = Pool(9);
        var player = Swimmer(6.5f);
        for (float time = 0; time < 5 && !(player.Body.OnGround && player.Body.Position.X > 8); time += frame)
            player.Step(world, frame, new FirstPersonController.Controls(Forward: 1, Up: true));
        Assert.True(player.Body.OnGround && player.Body.Position.X > 8 && player.Body.Position.Y is >= 9 and < 9.01f, $"stands at {player.Body.Position}");

        // A bank a block over the water's top block is more than the step out reaches from the top
        // of the bob, and the body is not thrown up it.
        world = Pool(10);
        player = Swimmer(6.5f);
        var highest = 0f;
        for (float time = 0; time < 5; time += frame)
        {
            player.Step(world, frame, new FirstPersonController.Controls(Forward: 1, Up: true));
            highest = MathF.Max(highest, player.Body.Position.Y);
        }
        Assert.True(player.Body.Position.X < 8, $"climbed to {player.Body.Position}");
        Assert.True(highest < Surface + 0.6f, $"rose to {highest}");
    }
}
