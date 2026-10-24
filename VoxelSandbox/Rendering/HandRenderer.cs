using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// What the player's right hand holds, drawn in the view as the source draws it, the held block's
/// cube or item's picture, swinging on a hit, a break or a use and bobbing with the walk, and nothing
/// when the hand is empty.
/// </summary>
/// <remarks>
/// <para>
/// The source poses a held item in the view's own space, about a block before the eye, and it is
/// posed so here and drawn a quarter of that size toward the eye, which shows it the same, so it
/// stands inside the player's own box, no nearer a wall than the player's body is, and no block the
/// player can stand beside cuts into it. It takes the source's swing and its models' places in the
/// right hand, a block's cube turned by 45 degrees at four tenths of its size and an item's picture
/// turned to the side and tilted at 0.68 of it.
/// </para>
/// <para>
/// It is lit by the world's light where the eye is and casts no shadow.
/// </para>
/// </remarks>
public sealed class HandRenderer
{
    // How much nearer the eye the view's pose is drawn, which keeps its look.
    private const float Near = 0.25f;
    // The source's swing takes six ticks, its walk's bob reaches a tenth of a block, and its walk is
    // counted at 0.6 of the distance walked.
    private const float SwingSeconds = 0.3f, MostBob = 0.1f, WalkShare = 0.6f;

    private float _swing = -1, _walked, _bob;

    /// <summary>Swings the held item, as a hit, a break or a use does, unless a swing is under way and short of its half.</summary>
    public void Swing()
    {
        if (_swing < 0 || _swing >= SwingSeconds / 2) _swing = 0;
    }

    /// <summary>Moves the swing on by so many seconds, and the bob by how far the player walked over them.</summary>
    public void Update(float seconds, float walked, bool onGround)
    {
        if (_swing >= 0 && (_swing += seconds) >= SwingSeconds) _swing = -1;
        _walked += walked * WalkShare;
        var bob = onGround && seconds > 0 ? MathF.Min(MostBob, walked / seconds * 0.05f) : 0;
        _bob += (bob - _bob) * (1 - MathF.Exp(-12 * seconds));
    }

    /// <summary>Draws the held item before the eye, looking the way given, and nothing for an empty hand. Called inside <c>BeginMode3D</c>.</summary>
    public void Draw(Vector3 eye, Vector3 look, Item? held, DropRenderer drops)
    {
        if (held is not { } item) return;
        var right = Vector3.Normalize(Vector3.Cross(look, Vector3.UnitY));
        var up = Vector3.Cross(right, look);
        var toWorld = Matrix4x4.CreateScale(Near) * new Matrix4x4(
            right.X, right.Y, right.Z, 0,
            up.X, up.Y, up.Z, 0,
            -look.X, -look.Y, -look.Z, 0,
            eye.X, eye.Y, eye.Z, 1);

        // The walk's bob as the source bobs the view, side to side and down at each step.
        var step = _walked * MathF.PI;
        var pose = Matrix4x4.CreateRotationX(Degrees(MathF.Abs(MathF.Cos(step - 0.2f) * _bob) * 5))
            * Matrix4x4.CreateRotationZ(Degrees(MathF.Sin(step) * _bob * 3))
            * Matrix4x4.CreateTranslation(MathF.Sin(step) * _bob * 0.5f, -MathF.Abs(MathF.Cos(step) * _bob), 0);

        // The source's arm with an item, its swing's way and its turns, then the item's model in the
        // right hand.
        var swing = _swing < 0 ? 0 : _swing / SwingSeconds;
        var root = MathF.Sqrt(swing);
        var g = MathF.Sin(swing * swing * MathF.PI);
        var h = MathF.Sin(root * MathF.PI);
        pose = Matrix4x4.CreateRotationY(Degrees(-45))
            * Matrix4x4.CreateRotationX(Degrees(h * -80))
            * Matrix4x4.CreateRotationZ(Degrees(h * -20))
            * Matrix4x4.CreateRotationY(Degrees(45 + g * -20))
            * Matrix4x4.CreateTranslation(0.56f, -0.52f, -0.72f)
            * Matrix4x4.CreateTranslation(-0.4f * h, 0.2f * MathF.Sin(root * MathF.Tau), -0.2f * MathF.Sin(swing * MathF.PI))
            * pose;
        var (mesh, material, flat) = drops.Model(item);
        var model = flat
            ? Matrix4x4.CreateScale(0.68f / 0.3f) * Matrix4x4.CreateRotationZ(Degrees(25)) * Matrix4x4.CreateRotationY(Degrees(90))
                * Matrix4x4.CreateTranslation(1.13f / 16, 3.2f / 16, 1.13f / 16)
            : Matrix4x4.CreateScale(0.4f / 0.25f) * Matrix4x4.CreateRotationY(Degrees(45));
        DrawMesh(mesh, material, model * pose * toWorld);
    }

    private static float Degrees(float degrees) => float.DegreesToRadians(degrees);
}
