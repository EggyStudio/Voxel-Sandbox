using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The player's right arm and what it holds, drawn in the view as the source draws them, the arm
/// when the hand is empty and the held block's cube or item's picture when it is not, swinging on a
/// hit, a break or a use and bobbing with the walk.
/// </summary>
/// <remarks>
/// <para>
/// The source poses them in the view's own space, about a block before the eye, and they are posed
/// so here and drawn a quarter of that size toward the eye, which shows them the same, so they stand
/// inside the player's own box, no nearer a wall than the player's body is, and no block the player
/// can stand beside cuts into them. A held item takes the source's swing and its models' places in
/// the right hand, a block's cube turned by 45 degrees at four tenths of its size and an item's
/// picture turned to the side and tilted at 0.68 of it.
/// </para>
/// <para>
/// The arm wears the right arm of the packs' player skin, Steve's, or the game's own sleeve and hand
/// where no pack has one. They are lit by the world's light where the eye is and cast no shadow.
/// </para>
/// </remarks>
public sealed class HandRenderer : IDisposable
{
    // How much nearer the eye the view's pose is drawn, which keeps its look.
    private const float Near = 0.25f;
    // The source's swing takes six ticks, its walk's bob reaches a tenth of a block, and its walk is
    // counted at 0.6 of the distance walked.
    private const float SwingSeconds = 0.3f, MostBob = 0.1f, WalkShare = 0.6f;
    // The arm, four pixels by twelve by four.
    private static readonly Vector3 ArmSize = new(0.25f, 0.75f, 0.25f);

    private ModelMesh? _arm;
    private ModelMaterial? _armMaterial;
    private Texture2D _skin;
    private ResourcePacks _packs = ResourcePacks.None;
    private float _swing = -1, _walked, _bob;

    /// <summary>Draws the arm with the packs' player skin from the next frame, as the packs change.</summary>
    public void Use(ResourcePacks packs)
    {
        Forget();
        _packs = packs;
    }

    /// <summary>Swings the arm, as a hit, a break or a use does, unless a swing is under way and short of its half.</summary>
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

    /// <summary>Draws the arm or the held item before the eye, looking the way given. Called inside <c>BeginMode3D</c>.</summary>
    public void Draw(Vector3 eye, Vector3 look, Item? held, DropRenderer drops)
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

        var swing = _swing < 0 ? 0 : _swing / SwingSeconds;
        var root = MathF.Sqrt(swing);
        if (held is { } item)
        {
            // The source's arm with an item, its swing's way and its turns, then the item's model in
            // the right hand.
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
            return;
        }

        // The empty arm reaching up from the lower right toward the middle, its hand's end before
        // the eye, swung forward and in by the source's swing.
        var reach = MathF.Sin(root * MathF.PI);
        pose = Matrix4x4.CreateRotationZ(Degrees(20))
            * Matrix4x4.CreateRotationX(Degrees(-70 - 30 * reach))
            * Matrix4x4.CreateRotationY(Degrees(20 * reach))
            * Matrix4x4.CreateTranslation(0.8f - 0.3f * reach, -0.55f + 0.4f * MathF.Sin(root * MathF.Tau), -0.45f - 0.4f * MathF.Sin(swing * MathF.PI))
            * pose;
        if (_arm is null || _armMaterial is null) (_arm, _armMaterial) = Arm();
        DrawMesh(_arm.Value, _armMaterial.Value, pose * toWorld);
    }

    private static float Degrees(float degrees) => float.DegreesToRadians(degrees);

    // The arm, from its shoulder at 0 up to its hand's end at its length, wearing the right arm of
    // the packs' skin, each face its part of it with the sleeve at the shoulder, or the game's own
    // sleeve and hand.
    private (ModelMesh, ModelMaterial) Arm()
    {
        var mesh = new BoxMesh();
        Vector3 min = new(-ArmSize.X / 2, 0, -ArmSize.Z / 2), max = new(ArmSize.X / 2, ArmSize.Y, ArmSize.Z / 2);
        if (_packs.Texture("entity/player/wide/steve") is { Width: 64, Height: 64 } skin)
        {
            // The sides' pictures run from the shoulder's row at the picture's top, which the faces
            // lay at their bottom, so each part is read upside down.
            static (Vector2, Vector2) Side(int u, int v) => (new(u / 64f, (v + 12) / 64f), new((u + 4) / 64f, v / 64f));
            static (Vector2, Vector2) End(int u, int v) => (new(u / 64f, v / 64f), new((u + 4) / 64f, (v + 4) / 64f));
            mesh.Add(min, max, Color.White, [Side(40, 20), Side(48, 20), End(48, 16), End(44, 16), Side(44, 20), Side(52, 20)]);
            _skin = LoadTextureFromImage(skin);
            SetTextureFilter(_skin, TextureFilter.Point);
            return (mesh.Upload(), new ModelMaterial(Color.White, _skin) { AlphaMode = MaterialAlphaMode.Opaque, Roughness = 0.9f, CastsShadows = false, BouncesLight = false });
        }
        mesh.Add(min, new Vector3(max.X, ArmSize.Y / 3, max.Z), new Color(0, 167, 167));
        mesh.Add(min with { Y = ArmSize.Y / 3 }, max, new Color(199, 144, 116));
        return (mesh.Upload(), new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Opaque, Roughness = 0.9f, CastsShadows = false, BouncesLight = false });
    }

    private void Forget()
    {
        if (_arm is { } arm) UnloadMesh(arm);
        if (_skin.IsValid) UnloadTexture(_skin);
        (_arm, _armMaterial, _skin) = (null, null, default);
    }

    public void Dispose() => Forget();
}
