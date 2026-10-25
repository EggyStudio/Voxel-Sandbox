using System.Numerics;

namespace VoxelSandbox;

/// <summary>What the player's model is posed by, its walk, its head's turn and tilt, its arm's swing and what it is doing.</summary>
/// <param name="Phase">Where in their swing its legs are, as <see cref="Gait.Step"/> moves them.</param>
/// <param name="Stride">How far its legs swing, from 0 standing to 1.</param>
/// <param name="HeadYaw">The head's turn from the body's, which leads it toward the aim.</param>
/// <param name="HeadPitch">The head's tilt, positive looking up.</param>
/// <param name="Swing">How far through a hit, a break or a use its right arm has swung, from 0 to 1, or 0 at rest.</param>
/// <param name="Sneaking">Whether it sneaks, leaning over.</param>
/// <param name="Swimming">Whether it swims, laid along the way it looks.</param>
/// <param name="Climbing">Whether it climbs, its arms reaching up in turn.</param>
/// <param name="InAir">Whether it is off the ground, as in a jump or a fall, its arms spread a little.</param>
/// <param name="Holding">Whether its right hand holds an item, which it holds a little ahead.</param>
public readonly record struct PlayerStance(float Phase, float Stride, float HeadYaw, float HeadPitch, float Swing = 0, bool Sneaking = false,
    bool Swimming = false, bool Climbing = false, bool InAir = false, bool Holding = false);

/// <summary>
/// The player's model posed as the source poses its player, each piece turned about its pivot, and
/// the whole laid along the way it looks while it swims.
/// </summary>
/// <remarks>
/// <para>
/// The numbers are the source's humanoid model's. The legs swing by 1.4 of the stride and the arms
/// against them by 1, a sneak leans the body by half a radian and the arms by 0.4 along it, drops
/// the head by 4.2 pixels and the body and arms by 3.2, draws the legs 4 pixels back and the whole
/// two pixels down, and an item held turns the right arm a tenth of a half turn ahead and halves its
/// swing. A hit swings the right arm up and ahead by 1.2 radians at its height and more for an aim
/// held low, across by 0.4, and turns the body by up to 0.2. The source turns its pieces in its own
/// space, y down and x mirrored, so a turn ahead there is one back here about x.
/// </para>
/// <para>
/// A swimmer is laid flat along its look, raised by the look's tilt, about the middle of its body,
/// its head turned up to look ahead and down, its arms sweeping from over its head back along its
/// sides in turn and its legs fluttering. A climber's arms reach up over its head in turn with its
/// legs' swing, and one in the air holds its arms a little out from its sides.
/// </para>
/// </remarks>
public static class PlayerPoses
{
    private const float Cycle = 0.6662f, Px = 1 / 16f;

    // The neck, where the body and the head turn, and the middle of the body, where a swimmer turns.
    private static readonly Vector3 Neck = new(0, 24 * Px, 0), Middle = new(0, 14 * Px, 0);

    /// <summary>A piece's turn about its pivot in a stance, with any move of it, in the model's blocks.</summary>
    public static Matrix4x4 Turn(ModelPart part, PlayerStance stance)
    {
        var at = stance.Phase * Cycle;
        var legs = MathF.Cos(at) * 1.4f * stance.Stride;
        var sneak = stance.Sneaking && !stance.Swimming;
        switch (part.Swing)
        {
            case Swing.Head:
            {
                var tilt = stance.Swimming ? MathF.PI / 4 : stance.HeadPitch;
                var turn = Matrix4x4.CreateRotationX(tilt) * Matrix4x4.CreateRotationY(stance.HeadYaw);
                return sneak ? turn * Matrix4x4.CreateTranslation(0, -4.2f * Px, 0) : turn;
            }
            case Swing.LegA or Swing.LegB:
            {
                var sign = part.Swing == Swing.LegA ? 1 : -1;
                var swing = stance.Swimming ? sign * MathF.Cos(at * 2) * 0.3f : sign * legs;
                var turn = Matrix4x4.CreateRotationX(swing);
                return sneak ? turn * Matrix4x4.CreateTranslation(0, -0.2f * Px, 4 * Px) : turn;
            }
            case Swing.ArmA or Swing.ArmB:
                return Arm(part.Swing == Swing.ArmB, at, stance, sneak);
            default:
            {
                // The body, which a hit turns and a sneak leans over at the neck.
                var turn = Matrix4x4.CreateRotationY(-HitTurn(stance.Swing));
                if (!sneak) return turn;
                return turn * Matrix4x4.CreateTranslation(-Neck) * Matrix4x4.CreateRotationX(-0.5f) * Matrix4x4.CreateTranslation(Neck) * Matrix4x4.CreateTranslation(0, -3.2f * Px, 0);
            }
        }
    }

    /// <summary>The whole model's place in its own space, laid along the look while it swims, two pixels down while it sneaks, upright otherwise.</summary>
    public static Matrix4x4 Whole(PlayerStance stance)
    {
        if (stance.Swimming)
            return Matrix4x4.CreateTranslation(-Middle) * Matrix4x4.CreateRotationX(-MathF.PI / 2 + Math.Clamp(stance.HeadPitch, -1.2f, 1.2f)) * Matrix4x4.CreateTranslation(Middle);
        return stance.Sneaking ? Matrix4x4.CreateTranslation(0, -2 * Px, 0) : Matrix4x4.Identity;
    }

    // An arm's turn, swinging against the legs, reaching up in turn while climbing, sweeping while
    // swimming, the right one swung by a hit and held ahead with an item.
    private static Matrix4x4 Arm(bool right, float at, PlayerStance stance, bool sneak)
    {
        var sign = right ? 1 : -1;
        if (stance.Swimming)
        {
            // From along the side, 0, over the head and back, π, the right a half stroke behind.
            var stroke = MathF.PI * (0.5f - 0.5f * MathF.Cos(at + (right ? MathF.PI : 0)));
            return Matrix4x4.CreateRotationX(stroke);
        }
        var x = sign * MathF.Cos(at) * stance.Stride;
        if (stance.Climbing) x = MathF.PI * 0.85f + sign * MathF.Cos(at * 2) * 0.3f;
        if (right && stance.Holding) x = x * 0.5f + MathF.PI / 10;
        // The right arm stands at +x, so a turn about z by +0.15 takes its hand out, and the left's by -0.15.
        float y = 0, z = stance.InAir && !stance.Climbing ? sign * 0.15f : 0;
        if (right && stance.Swing > 0)
        {
            var swing = stance.Swing;
            var eased = 1 - MathF.Pow(1 - swing, 4);
            // Higher for an aim held low, as the source's hit follows the head's tilt.
            x += MathF.Sin(eased * MathF.PI) * 1.2f + MathF.Sin(swing * MathF.PI) * (stance.HeadPitch + 0.7f) * 0.75f;
            y -= HitTurn(swing) * 2;
            z += MathF.Sin(swing * MathF.PI) * 0.4f;
        }
        if (sneak) x -= 0.4f;
        var turn = Matrix4x4.CreateRotationZ(z) * Matrix4x4.CreateRotationY(y) * Matrix4x4.CreateRotationX(x);
        return sneak ? turn * Matrix4x4.CreateTranslation(0, -3.2f * Px, 0) : turn;
    }

    // How far a hit turns the body, the source's sine of the root of its swing over a whole turn.
    private static float HitTurn(float swing) => swing > 0 ? MathF.Sin(MathF.Sqrt(swing) * MathF.Tau) * 0.2f : 0;

    /// <summary>
    /// Where the right hand holds an item in the model's own space as the arm is turned, the source's
    /// place of an item in its third-person hand, a pixel in and ten down the arm from its shoulder
    /// and two ahead, turned to point ahead.
    /// </summary>
    public static Matrix4x4 Hand(ModelPart arm, PlayerStance stance) =>
        Matrix4x4.CreateRotationX(-MathF.PI / 2) * Matrix4x4.CreateTranslation(arm.Pivot * Px + new Vector3(-1 * Px, -10 * Px, -2 * Px)) * arm.PlacedBy(Turn(arm, stance));
}
