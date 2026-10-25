using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// What the player's right hand holds, drawn in the view as the source draws it, the held block's
/// cube or item's picture, swinging on a hit, a break or a use and bobbing with the walk, and the
/// player's own right arm of its model when the hand is empty.
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

    /// <summary>How far through its swing the hand is, from 0 to 1, or 0 at rest, which the model's arm swings by too.</summary>
    public float Swung => _swing < 0 ? 0 : _swing / SwingSeconds;

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

    /// <summary>Draws the held item before the eye, looking the way given, or for an empty hand the model's right arm. Called inside <c>BeginMode3D</c>.</summary>
    public void Draw(Vector3 eye, Vector3 look, Item? held, DropRenderer drops, MobRenderer models)
    {
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

        var swing = Swung;
        var root = MathF.Sqrt(swing);
        if (held is not { } item)
        {
            models.DrawArm(Arm(swing) * pose * toWorld);
            return;
        }
        // The source's arm with an item, its swing's way and its turns, then the item's model in the
        // right hand.
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
        DrawMesh(mesh, material, Turn(flat, item.Tool != ToolKind.None) * pose * toWorld);
    }

    /// <summary>
    /// Where the source's empty right hand stands in the view as it swings, its arm's boxes taken
    /// from the model's space, y up and x mirrored, into the source's, and placed by the source's
    /// turns of a player's arm in the first person.
    /// </summary>
    public static Matrix4x4 Arm(float swing)
    {
        var root = MathF.Sqrt(swing);
        float across = -0.3f * MathF.Sin(root * MathF.PI), up = 0.4f * MathF.Sin(root * MathF.Tau), ahead = -0.4f * MathF.Sin(swing * MathF.PI);
        var model = Matrix4x4.CreateScale(-1, -1, 1) * Matrix4x4.CreateTranslation(0, 1.5f, 0);
        return model * Matrix4x4.CreateTranslation(5.6f, 0, 0) * Matrix4x4.CreateRotationY(Degrees(-135)) * Matrix4x4.CreateRotationX(Degrees(200))
            * Matrix4x4.CreateRotationZ(Degrees(120)) * Matrix4x4.CreateTranslation(-1, 3.6f, 3.5f)
            * Matrix4x4.CreateRotationZ(Degrees(MathF.Sin(swing * swing * MathF.PI) * -20)) * Matrix4x4.CreateRotationY(Degrees(MathF.Sin(root * MathF.PI) * 70))
            * Matrix4x4.CreateRotationY(Degrees(45)) * Matrix4x4.CreateTranslation(across + 0.64f, up - 0.6f, ahead - 0.72f);
    }

    /// <summary>
    /// How a held item's model stands in the right hand, as the source's display for the first
    /// person's right hand places it, a block's cube turned by 45 degrees at four tenths of its size,
    /// or an item's picture, a pixel deep and centered on the origin facing +z, at 0.68 of its size,
    /// tilted by 25 degrees and turned by a quarter about the upright axis, its handle's end ahead of
    /// the hand and leaning toward the view's middle.
    /// </summary>
    /// <remarks>
    /// A tool's picture is first turned by a half about its handle, the diagonal from the picture's
    /// lower left to its upper right, which keeps the handle where the quarter turn puts it and
    /// brings an axe's blade, left of the handle's end in the picture, ahead of the handle rather than
    /// toward the eye. A pick's head, a shovel's and a sword's blade lie alike on both sides of the
    /// handle and look the same either way.
    /// </remarks>
    public static Matrix4x4 Turn(bool flat, bool tool = false) => flat
        ? (tool ? AboutHandle : Matrix4x4.Identity) * Matrix4x4.CreateScale(0.68f / 0.3f) * Matrix4x4.CreateRotationZ(Degrees(25))
            * Matrix4x4.CreateRotationY(Degrees(90)) * Matrix4x4.CreateTranslation(1.13f / 16, 3.2f / 16, 1.13f / 16)
        : Matrix4x4.CreateScale(0.4f / 0.25f) * Matrix4x4.CreateRotationY(Degrees(45));

    // A half turn about the line where x and y are equal, which swaps them and turns the picture's
    // front to its back.
    private static readonly Matrix4x4 AboutHandle = new(
        0, 1, 0, 0,
        1, 0, 0, 0,
        0, 0, -1, 0,
        0, 0, 0, 1);

    private static float Degrees(float degrees) => float.DegreesToRadians(degrees);
}
