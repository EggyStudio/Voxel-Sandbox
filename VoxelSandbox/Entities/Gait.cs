using System.Numerics;

namespace VoxelSandbox;

/// <summary>What a mob's model is turned by in a frame, its stride, its head's look, its wings and whether it hunts.</summary>
/// <param name="Phase">Where its legs are in their swing, as Minecraft's walk position counts it.</param>
/// <param name="Stride">How far its legs swing, from 0 standing to 1, as Minecraft's walk speed.</param>
/// <param name="HeadYaw">The head's turn about the vertical from the body's, in radians.</param>
/// <param name="HeadPitch">The head's tilt, in radians.</param>
/// <param name="Wings">How far its wings are spread, in radians.</param>
/// <param name="Hunting">Whether it hunts the player, which raises a zombie's arms and a skeleton's bow.</param>
public readonly record struct Stance(float Phase, float Stride, float HeadYaw, float HeadPitch, float Wings, bool Hunting);

/// <summary>
/// How a mob's legs, arms and head turn as it walks and looks, by the numbers Minecraft 26.3's
/// models walk by, and how its stride follows its speed.
/// </summary>
/// <remarks>
/// Minecraft's walk turns a leg about its hip by cos(position × 0.6662) × 1.4 × speed, where its
/// speed is four times the blocks moved a tick, up to 1, eased four tenths of the way each tick, and
/// its position runs on by the speed each tick, so a walk's legs cycle once every 2.36 blocks.
/// A turn about x or y in the game's model space runs the other way from Minecraft's, whose y runs
/// down and whose x is mirrored, and a turn about z the same way.
/// </remarks>
public static class Gait
{
    // A tick of Minecraft's, which its walk is counted in.
    private const float Ticks = 20;
    private const float Cycle = 0.6662f;

    // A zombie's arms at rest and while it hunts, Minecraft's turns of π over 2.25 and over 1.5
    // from hanging, 10 degrees under level and 30 over it.
    private const float Reaching = MathF.PI / 2.25f, Raised = MathF.PI / 1.5f;

    // Where each of a spider's four pairs of legs, from hind to front, is in its walk.
    private static readonly float[] SpiderOffsets = [0, MathF.PI, MathF.PI / 2, 3 * MathF.PI / 2];

    /// <summary>Moves a stride and its phase on by a frame in which the mob moved so far over the ground.</summary>
    /// <remarks>
    /// A frame that took no time, as the game's first does before the engine has timed one, leaves
    /// the stride as it was, where its speed would be 0 over 0 and the stride, and with it every
    /// leg's swing, not a number from then on, so the legs were drawn nowhere.
    /// </remarks>
    public static void Step(ref float stride, ref float phase, float along, float seconds)
    {
        if (seconds <= 0) return;
        var speed = MathF.Min(1, 4 * along / (seconds * Ticks));
        stride += (speed - stride) * (1 - MathF.Pow(0.6f, seconds * Ticks));
        phase += stride * seconds * Ticks;
    }

    /// <summary>The turn of a piece of a kind's model about its pivot in a stance, its pose among it.</summary>
    public static Matrix4x4 Turn(MobKind kind, ModelPart part, Stance stance)
    {
        var at = stance.Phase * Cycle;
        var legs = MathF.Cos(at) * kind.StrideSwing * stance.Stride;
        return part.Swing switch
        {
            Swing.Head => Matrix4x4.CreateRotationX(stance.HeadPitch) * Matrix4x4.CreateRotationY(stance.HeadYaw),
            Swing.LegA => Matrix4x4.CreateRotationX(legs),
            Swing.LegB => Matrix4x4.CreateRotationX(-legs),
            Swing.LeftWing => Matrix4x4.CreateRotationZ(-stance.Wings),
            Swing.RightWing => Matrix4x4.CreateRotationZ(stance.Wings),
            Swing.ArmA or Swing.ArmB => Arm(kind, part.Swing == Swing.ArmB, at, stance),
            Swing.SpiderA or Swing.SpiderB => Spider(part, part.Swing == Swing.SpiderA, at, stance.Stride),
            _ => ModelPart.Turned(part.Pose),
        };
    }

    // A zombie reaches its arms out ahead, higher while it hunts. A skeleton's hang and swing
    // against its legs until it hunts, and then hold its bow out where it looks, the left hand
    // turned in to the string, as Minecraft's bow is drawn.
    private static Matrix4x4 Arm(MobKind kind, bool right, float at, Stance stance)
    {
        if (!kind.Shoots) return Matrix4x4.CreateRotationX(stance.Hunting ? Raised : Reaching);
        if (!stance.Hunting) return Matrix4x4.CreateRotationX((right ? 1 : -1) * MathF.Cos(at) * stance.Stride);
        return Matrix4x4.CreateRotationX(MathF.PI / 2 + stance.HeadPitch) * Matrix4x4.CreateRotationY((right ? 0.1f : -0.5f) + stance.HeadYaw);
    }

    // A spider's leg turns about the vertical with cos(2 × position) and lifts about z with
    // |sin(position)|, each by 0.4 of the stride, its pair a quarter of the cycle from the next, the
    // left side mirrored, added to the angles the leg reaches out at.
    private static Matrix4x4 Spider(ModelPart part, bool right, float at, float stride)
    {
        var offset = SpiderOffsets[part.Pair];
        var turn = -MathF.Cos(2 * at + offset) * 0.4f * stride;
        var lift = MathF.Abs(MathF.Sin(at + offset) * 0.4f) * stride;
        return ModelPart.Turned(part.Pose + (right ? new Vector3(0, -turn, lift) : new Vector3(0, turn, -lift)));
    }
}
