using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The player's model posed by what it does, read by where its hands, its hips and its head go in each stance.</summary>
public sealed class PlayerPoseTests
{
    private const float Px = 1 / 16f;

    private static ModelPart Part(Swing swing) => MobKinds.Player.Parts.Single(p => p.Swing == swing);

    // Where a point of a piece goes, given in the model's pixels, in a stance, in the model's blocks.
    private static Vector3 At(Swing swing, Vector3 pixels, PlayerStance stance) =>
        Vector3.Transform(pixels * Px, Part(swing).PlacedBy(PlayerPoses.Turn(Part(swing), stance)));

    // The right hand's end and the left's, ten pixels down from each shoulder's pivot.
    private static Vector3 RightHand(PlayerStance stance) => At(Swing.ArmB, Part(Swing.ArmB).Pivot + new Vector3(0, -10, 0), stance);

    private static Vector3 LeftHand(PlayerStance stance) => At(Swing.ArmA, Part(Swing.ArmA).Pivot + new Vector3(0, -10, 0), stance);

    private static readonly PlayerStance Standing = new(0, 0, 0, 0);

    [Fact]
    public void Standing_Each_Piece_Hangs_As_Built_And_Walking_The_Arms_Swing_Against_The_Legs()
    {
        foreach (var part in MobKinds.Player.Parts) Assert.True(PlayerPoses.Turn(part, Standing).IsIdentity, $"{part.Swing} turned standing");
        Assert.True(PlayerPoses.Whole(Standing).IsIdentity);
        // At the walk's first phase the left leg is ahead, -z, and the left arm behind it.
        var walking = Standing with { Stride = 1 };
        var leftFoot = At(Swing.LegA, Part(Swing.LegA).Pivot + new Vector3(0, -12, 0), walking);
        Assert.True(leftFoot.Z * LeftHand(walking).Z < 0, $"the left foot at {leftFoot} and the left hand at {LeftHand(walking)}");
    }

    [Fact]
    public void A_Sneak_Leans_The_Body_Over_Its_Hips_Back_And_Lowers_The_Head()
    {
        var sneaking = Standing with { Sneaking = true };
        var hips = At(Swing.None, new Vector3(0, 12, 0), sneaking);
        Assert.True(hips.Z > 3 * Px, $"the hips at {hips}");
        var head = At(Swing.Head, new Vector3(0, 28, 0), sneaking);
        Assert.Equal((28 - 4.2f) * Px, head.Y, 3);
        // Its arms hang along the leaning body, their hands back.
        Assert.True(RightHand(sneaking).Z > 2 * Px);
        Assert.Equal(-2 * Px, Vector3.Transform(Vector3.Zero, PlayerPoses.Whole(sneaking)).Y, 3);
    }

    [Fact]
    public void A_Swimmer_Lies_Along_Its_Look_Head_Ahead_And_Strokes_In_Turn()
    {
        var swimming = Standing with { Swimming = true, Stride = 1 };
        var whole = PlayerPoses.Whole(swimming);
        var head = Vector3.Transform(new Vector3(0, 30, 0) * Px, whole);
        var feet = Vector3.Transform(Vector3.Zero, whole);
        // Laid flat, the head is ahead, -z, of the feet, both about the body's middle's height.
        Assert.True(head.Z < feet.Z - 1, $"the head at {head} and the feet at {feet}");
        Assert.InRange(head.Y, 0.7f, 1.1f);
        Assert.InRange(feet.Y, 0.7f, 1.1f);
        // At the stroke's first phase the left arm lies along the side and the right reaches over the head.
        Assert.True(LeftHand(swimming).Y < Part(Swing.ArmA).Pivot.Y * Px, $"the left hand at {LeftHand(swimming)}");
        Assert.True(RightHand(swimming).Y > Part(Swing.ArmB).Pivot.Y * Px, $"the right hand at {RightHand(swimming)}");
    }

    [Fact]
    public void A_Climber_Reaches_Up_A_Jumper_Spreads_Its_Arms_And_A_Holder_Holds_Ahead()
    {
        var shoulder = Part(Swing.ArmB).Pivot.Y * Px;
        Assert.True(RightHand(Standing with { Climbing = true }).Y > shoulder);
        Assert.True(LeftHand(Standing with { Climbing = true }).Y > Part(Swing.ArmA).Pivot.Y * Px);
        // Off the ground each hand goes out from the body's side.
        Assert.True(MathF.Abs(RightHand(Standing with { InAir = true }).X) > MathF.Abs(RightHand(Standing).X));
        Assert.True(MathF.Abs(LeftHand(Standing with { InAir = true }).X) > MathF.Abs(LeftHand(Standing).X));
        // An item held brings the right hand ahead, and the left stays.
        Assert.True(RightHand(Standing with { Holding = true }).Z < RightHand(Standing).Z - Px);
        Assert.Equal(LeftHand(Standing), LeftHand(Standing with { Holding = true }));
    }

    [Fact]
    public void A_Hit_Swings_The_Right_Arm_Up_Ahead_And_Back_Down_Turning_The_Body()
    {
        float Ahead(float swing) => -RightHand(Standing with { Swing = swing }).Z;
        Assert.True(Ahead(0.5f) > Ahead(0) + 4 * Px, $"half through the hit the hand is {Ahead(0.5f)} ahead");
        Assert.True(Ahead(0.99f) < Ahead(0.5f), "the hand comes back down by the hit's end");
        Assert.False(PlayerPoses.Turn(Part(Swing.None), Standing with { Swing = 0.25f }).IsIdentity);
        Assert.Equal(LeftHand(Standing), LeftHand(Standing with { Swing = 0.5f }));
    }
}
