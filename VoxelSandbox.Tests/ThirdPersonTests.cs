using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The camera of a view from without, pulled in by the blocks, and the views F5 steps through.</summary>
public sealed class ThirdPersonTests
{
    [Fact]
    public void The_Camera_Stands_Four_Blocks_Off_Or_A_Tenth_Short_Of_A_Block_In_The_Way()
    {
        var world = Laid.World(c => Laid.Fill(c, 0, 10, BlockId.Stone));
        var eye = new Vector3(8.5f, 12.62f, 8.5f);
        Assert.Equal(ThirdPerson.Away, ThirdPerson.Distance(world, eye, Vector3.UnitX));
        // A wall a block and a half along x, and the floor a block and five eighths down.
        world.SetBlock(10, 12, 8, BlockId.Stone);
        Assert.Equal(1.4f, ThirdPerson.Distance(world, eye, Vector3.UnitX), 3);
        Assert.Equal(1.52f, ThirdPerson.Distance(world, eye, -Vector3.UnitY), 3);
        // A block whose side the middle ray passes five hundredths off still pulls the camera in,
        // met by the ray a tenth beside it.
        world.SetBlock(10, 12, 8, BlockId.Air);
        world.SetBlock(11, 12, 9, BlockId.Stone);
        var beside = eye with { Z = 8.95f };
        Assert.Null(VoxelRay.Cast(world, beside, Vector3.UnitX, ThirdPerson.Away));
        Assert.Equal(2.4f, ThirdPerson.Distance(world, beside, Vector3.UnitX), 3);
    }

    [Fact]
    public void F5_Steps_From_The_Eye_To_Behind_To_In_Front_And_Back_And_The_Camera_Looks_At_The_Eye()
    {
        Assert.Equal(View.Behind, ThirdPerson.Next(View.First));
        Assert.Equal(View.Front, ThirdPerson.Next(View.Behind));
        Assert.Equal(View.First, ThirdPerson.Next(View.Front));

        var world = Laid.World(c => Laid.Fill(c, 0, 10, BlockId.Stone));
        var player = new FirstPersonController { View = View.Behind };
        player.Body.Position = new Vector3(8.5f, 11, 8.5f);
        player.Step(world, 1 / 30f, new FirstPersonController.Controls());
        var camera = player.Camera;
        Assert.Equal(player.Eye, camera.Target);
        Assert.Equal(ThirdPerson.Away, Vector3.Distance(camera.Position, player.Eye), 3);
        Assert.True(Vector3.Dot(camera.Position - player.Eye, player.Look) < 0, "the camera stands behind the eye");
        player.View = View.Front;
        player.Step(world, 1 / 30f, new FirstPersonController.Controls());
        Assert.True(Vector3.Dot(player.Camera.Position - player.Eye, player.Look) > 0, "the camera stands in front of the eye");
    }

    [Fact]
    public void The_Players_Model_Stands_Its_Height_With_Its_Arms_Hanging_And_Swinging_Against_Its_Legs()
    {
        var kind = MobKinds.Player;
        // Its boxes reach from its feet to the top of its hair, fifteen sixteenths of 32 pixels.
        var top = kind.Parts.SelectMany(p => p.Boxes).Max(b => b.Max.Y) * kind.Scale / 16;
        Assert.InRange(top, 1.87f, 1.9f);
        ModelPart Part(Swing swing) => kind.Parts.Single(p => p.Swing == swing);
        // At rest the arms hang, and walking each swings against the leg on its side.
        Assert.Equal(Matrix4x4.Identity, Gait.Turn(kind, Part(Swing.ArmA), new Stance(0, 0, 0, 0, 0, false)));
        var walking = new Stance(0, 1, 0, 0, 0, false);
        float Swung(Swing swing) => MathF.Atan2(Gait.Turn(kind, Part(swing), walking).M23, Gait.Turn(kind, Part(swing), walking).M22);
        Assert.True(Swung(Swing.ArmA) * Swung(Swing.LegA) < 0, "the left arm swings against the left leg");
        Assert.True(Swung(Swing.ArmB) * Swung(Swing.LegB) < 0, "the right arm swings against the right leg");
    }
}
