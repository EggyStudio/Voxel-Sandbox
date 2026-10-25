using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// Walks, sprints, sneaks, jumps and flies the player's body from the keys, turns the view with
/// the mouse, and gives the camera at the player's eyes, with the speeds of Minecraft's creative mode.
/// </summary>
public sealed class FirstPersonController
{
    private const float Gravity = 32;
    private const float FallLimit = 78;
    // A jump of a block and a quarter under that gravity, which clears one block as Minecraft's does.
    private const float JumpSpeed = 9;
    private const float Walk = 4.3f, Sprint = 5.6f, Sneak = 1.3f, Fly = 10.9f, FlySprint = 21.6f, FlyVertical = 7.5f;
    private const float StandingEyes = 1.62f, SneakingEyes = 1.27f;
    // In water the body moves at a little over half speed.
    private const float SwimSpeed = 0.6f;
    // Minecraft's water, by its tick of a twentieth of a second, where Space lifts by 0.04 a tick
    // before the tick's move, the speed keeps 0.8 of itself after it and falls by 0.005, out of the
    // water it falls by 0.08 and keeps 0.98, a jump from the bottom leaves at 0.42, and a body pushing
    // into a block with room for it raised 0.6 out of the water rises at 0.3 a tick to step out.
    private const float WaterTick = 0.05f, SwimLift = 0.04f, WaterKeep = 0.8f, WaterFall = 0.005f, AirFall = 0.08f, AirKeep = 0.98f;
    private const float TickJump = 0.42f, StepOut = 0.3f, StepRoom = 0.6f;
    // Standing on the bottom in water no deeper than this over its feet, Space jumps rather than
    // lifts, at most every half second, as Minecraft's threshold and its ten ticks between jumps have it.
    private const float Wade = 0.4f, JumpDelay = 0.5f;
    private const float DoubleTap = 0.3f;

    private double _lastSpace = double.NegativeInfinity;
    private float _jumpWait;
    // The body in water or bobbing out of it moves by Minecraft's ticks, which hold its speed in
    // blocks a tick carried from one tick to the next, the speed it moves at over the tick under
    // way, and the time left of that tick.
    private bool _bobbing;
    private float _tickVelocity, _tickSpeed, _tickLeft;
    // Whether the body climbs out of water, from its first push against a block's side in the water
    // until it stands or stops pushing.
    private bool _climbing;
    private float _eyes = StandingEyes;
    private float _fov;

    public FirstPersonController() => _fov = FieldOfView;

    public Body Body { get; } = new();

    /// <summary>The turn about the vertical in radians, 0 looking north (toward -z) and growing to the left.</summary>
    public float Yaw { get; set; }

    /// <summary>The tilt in radians, positive looking up.</summary>
    public float Pitch { get; set; }

    public bool Flying { get; set; }

    /// <summary>Whether two presses of Space start flying, as in creative mode and not in survival.</summary>
    public bool MayFly { get; set; } = true;

    /// <summary>Whether the body flies through the blocks, stopped by none, as in spectator mode, where it always flies.</summary>
    public bool Spectating { get; set; }

    /// <summary>Whether Left Control sprints, which in survival asks for more than six food.</summary>
    public bool MaySprint { get; set; } = true;

    /// <summary>How many times the player has been put somewhere, which the vitals read so a teleport is no fall.</summary>
    public int Teleports { get; private set; }

    public bool Sprinting { get; private set; }

    public bool Sneaking { get; private set; }

    /// <summary>Whether any of the body is in water, as of the last update.</summary>
    public bool Swimming { get; private set; }

    /// <summary>Whether the eye is in water, which tints the view.</summary>
    public bool EyeInWater { get; private set; }

    /// <summary>The vertical field of view in degrees while walking, widened while sprinting.</summary>
    public float FieldOfView { get; set; } = 70;

    /// <summary>Radians turned for each pixel the mouse moves.</summary>
    public float Sensitivity { get; set; } = 0.0025f;

    /// <summary>The zoom its key gives, which narrows the view and the mouse's turn alike.</summary>
    public Zoom Zoom { get; } = new();

    /// <summary>Radians turned for each pixel the mouse moves as the view stands now, narrowed with the zoom so the aim holds.</summary>
    public float Turn => Sensitivity * Zoom.Share;

    public Vector3 Eye => Body.Position + new Vector3(0, _eyes, 0);

    public Vector3 Look => new(-MathF.Sin(Yaw) * MathF.Cos(Pitch), MathF.Sin(Pitch), -MathF.Cos(Yaw) * MathF.Cos(Pitch));

    /// <summary>
    /// The camera of the view, at the eye looking along the look, or from without, behind the player
    /// or in front of it, as far as the blocks let it stand, looking at the eye.
    /// </summary>
    public Camera3D Camera => View switch
    {
        View.First => new(Eye, Eye + Look, Vector3.UnitY, _fov * Zoom.Share),
        View.Behind => new(Eye - Look * _away, Eye, Vector3.UnitY, _fov * Zoom.Share),
        _ => new(Eye + Look * _away, Eye, Vector3.UnitY, _fov * Zoom.Share),
    };

    // How far a view from without stands from the eye, as last found.
    private float _away = ThirdPerson.Away;

    /// <summary>The way the player faces in degrees from north, clockwise, as a compass reads it.</summary>
    public float Heading
    {
        get => ((-Yaw * 180 / MathF.PI) % 360 + 360) % 360;
        set => Yaw = -value * MathF.PI / 180;
    }

    /// <summary>
    /// Turns, steers and moves the body for a frame of so many seconds, the mouse turning the view
    /// where <paramref name="look"/> is set, as while the cursor is held, and the keys moving the
    /// player where <paramref name="move"/> is, as unless a text field has them, each read from the
    /// frame's input.
    /// </summary>
    public void Update(VoxelWorld world, float seconds, Input input, bool look, bool move)
    {
        // The zoom's key and the wheel while it is held act only while the mouse turns the view.
        Zoom.Update(seconds, look && input.KeyDown(Zoom.Key), look ? GetMouseWheelMove() : 0);
        if (look)
        {
            var mouse = input.MouseDelta;
            Yaw -= mouse.X * Turn;
            Pitch = Math.Clamp(Pitch - mouse.Y * Turn, -1.55f, 1.55f);
        }

        var controls = new Controls();
        if (move)
        {
            controls = new Controls((input.KeyDown(Key.W) ? 1 : 0) - (input.KeyDown(Key.S) ? 1 : 0), (input.KeyDown(Key.D) ? 1 : 0) - (input.KeyDown(Key.A) ? 1 : 0),
                input.KeyDown(Key.Space), input.KeyDown(Key.LeftShift), input.KeyDown(Key.LeftControl));
            if (input.KeyPressed(Key.Space))
            {
                // Two presses of Space in quick succession start or stop flying, as in creative mode.
                if (MayFly && GetTime() - _lastSpace < DoubleTap)
                {
                    Flying = !Flying;
                    Body.Velocity.Y = 0;
                    _lastSpace = double.NegativeInfinity;
                }
                else _lastSpace = GetTime();
            }
        }
        Step(world, seconds, controls);
    }

    /// <summary>Where the camera stands, at the eye, behind the player or in front of it, as F5 steps it.</summary>
    public View View { get; set; }

    /// <summary>How far the player's legs swing and where in their swing they are, as a mob's are, for the model a view from without shows.</summary>
    public float Stride { get; private set; }

    public float StridePhase { get; private set; }

    /// <summary>What the keys ask of the body for a frame, ahead and to the right from -1 to 1, and whether Space, Left Shift and Left Control are held.</summary>
    public readonly record struct Controls(float Forward = 0, float Side = 0, bool Up = false, bool Down = false, bool Sprint = false);

    /// <summary>Moves the body for a frame of so many seconds as the controls ask, as <see cref="Update"/> does from the keys.</summary>
    public void Step(VoxelWorld world, float seconds, Controls controls)
    {
        var before = Body.Position;
        var (forward, side, up, down, sprintKey) = (controls.Forward, controls.Side, controls.Up, controls.Down, controls.Sprint);
        if (forward <= 0) Sprinting = false;
        else if (sprintKey && MaySprint) Sprinting = true;
        if (!MaySprint) Sprinting = false;
        var depth = WaterDepth(world, Body.Position);
        Swimming = !Flying && depth > 0;
        Sneaking = down && !Flying && !Swimming;

        var ahead = new Vector3(-MathF.Sin(Yaw), 0, -MathF.Cos(Yaw));
        var right = new Vector3(MathF.Cos(Yaw), 0, -MathF.Sin(Yaw));
        var wish = ahead * forward + right * side;
        if (wish.LengthSquared() > 1) wish = Vector3.Normalize(wish);
        var speed = (Flying ? (Sprinting ? FlySprint : Fly) : Sneaking ? Sneak : Sprinting ? Sprint : Walk) * (Swimming ? SwimSpeed : 1);

        // The velocity eases toward the one wished for, quickly on the ground and slowly in the air,
        // so a jump keeps most of its run.
        var grip = 1 - MathF.Exp(-(Flying ? 8 : Body.OnGround ? 14 : 2.5f) * seconds);
        Body.Velocity.X += (wish.X * speed - Body.Velocity.X) * grip;
        Body.Velocity.Z += (wish.Z * speed - Body.Velocity.Z) * grip;
        if (Flying)
            Body.Velocity.Y += (((up ? 1 : 0) - (down ? 1 : 0)) * FlyVertical - Body.Velocity.Y) * grip;
        else if (Swimming || _bobbing && !Body.OnGround)
        {
            // The body rises and falls by Minecraft's ticks in the water and above it until it stands
            // again, moving over each tick at the speed the tick's start gives, so its bob at the top
            // reaches the source's height whatever a frame's length, a swimmer holding Space coming up
            // until its feet leave the water, going on a little and falling back, sinking under the
            // surface and rising again.
            if (!_bobbing) (_tickVelocity, _tickLeft) = (Body.Velocity.Y * WaterTick, 0);
            _bobbing = true;
            _jumpWait -= seconds;
            var rise = 0f;
            for (var left = seconds; left > 0;)
            {
                if (_tickLeft <= 0) SwimTick(world, Body.Position + new Vector3(0, rise, 0), wish, up);
                var part = MathF.Min(left, _tickLeft);
                rise += _tickSpeed * part;
                (_tickLeft, left) = (_tickLeft - part, left - part);
            }
            Body.Velocity.Y = seconds > 0 ? rise / seconds : _tickSpeed;
        }
        else
        {
            Body.Velocity.Y = MathF.Max(Body.Velocity.Y - Gravity * seconds, -FallLimit);
            if (up && Body.OnGround) Body.Velocity.Y = JumpSpeed;
        }

        if (Flying || Body.OnGround || wish == Vector3.Zero) _climbing = false;
        var stopped = Body.Velocity.Y == 0;
        if (Spectating) Body.Position += Body.Velocity * seconds;
        else Body.Move(world, Body.Velocity * seconds, keepToEdges: Sneaking && Body.OnGround, climbing: _climbing);
        if (Flying && Body.OnGround && down && !Spectating) Flying = false;
        // A tick's rise or fall that a block stopped carries no speed on, and a body that stands or
        // flies again leaves the ticks for the frame's own time.
        if (_bobbing && !stopped && Body.Velocity.Y == 0) (_tickVelocity, _tickSpeed) = (0, 0);
        if (Flying || Body.OnGround && !Swimming) _bobbing = false;

        _eyes += ((Sneaking ? SneakingEyes : StandingEyes) - _eyes) * (1 - MathF.Exp(-20 * seconds));
        var eye = Eye;
        EyeInWater = world.GetBlock((int)MathF.Floor(eye.X), (int)MathF.Floor(eye.Y), (int)MathF.Floor(eye.Z)) is var around && Blocks.IsWater(around);
        var widened = Sprinting && wish != Vector3.Zero ? FieldOfView * 1.12f : FieldOfView;
        _fov += (widened - _fov) * (1 - MathF.Exp(-10 * seconds));

        // The legs swing by how far the body went across, as a mob's do, and a view from without
        // stands as far from the eye as the blocks let it.
        var across = new Vector2(Body.Position.X - before.X, Body.Position.Z - before.Z);
        var (stride, phase) = (Stride, StridePhase);
        Gait.Step(ref stride, ref phase, across.Length(), seconds);
        (Stride, StridePhase) = (stride, phase);
        // The body turns toward the way it walks, front to back where it walks backward, and after
        // the head where the head turns more than 75 degrees from it, as the source's does.
        if (across.LengthSquared() > 1e-6f)
        {
            var toward = MathF.Atan2(-across.X, -across.Y);
            if (MathF.Abs(MathF.IEEERemainder(toward - Yaw, MathF.Tau)) > MathF.PI / 2) toward += MathF.PI;
            BodyYaw += Math.Clamp(MathF.IEEERemainder(toward - BodyYaw, MathF.Tau), -10 * seconds, 10 * seconds);
        }
        var off = MathF.IEEERemainder(Yaw - BodyYaw, MathF.Tau);
        if (MathF.Abs(off) > 1.3f) BodyYaw = Yaw - MathF.Sign(off) * 1.3f;
        if (View != View.First) _away = ThirdPerson.Distance(world, Eye, View == View.Behind ? -Look : Look);
    }

    /// <summary>The body's turn, which follows the way it walks and the head's, the head leading it by at most 75 degrees.</summary>
    public float BodyYaw { get; private set; }

    /// <summary>Whether it climbs out of water onto a bank, as a swimmer pushing into one does.</summary>
    public bool Climbing => _climbing;

    // One of Minecraft's ticks of a body in water or bobbing above it, from where the tick finds the
    // feet. In water it takes a jump from the bottom where the water stands no deeper than Wade over
    // them, or else Space's lift before the move and the drag and fall after it, and pushing into a
    // block with room for the body raised out of the water, the step out. Above the water it takes
    // the fall and the drag of the air.
    private void SwimTick(VoxelWorld world, Vector3 feet, Vector3 wish, bool up)
    {
        var depth = WaterDepth(world, feet);
        float moving;
        if (depth > 0)
        {
            var wading = Body.OnGround && depth <= Wade;
            if (up && wading && _jumpWait <= 0) (_tickVelocity, _jumpWait) = (TickJump, JumpDelay);
            moving = _tickVelocity + (up && !wading ? SwimLift : 0);
            _tickVelocity = moving * WaterKeep - WaterFall;
            var raised = feet + wish * 0.05f + new Vector3(0, StepRoom + MathF.Max(0, moving), 0);
            if (wish != Vector3.Zero && !Body.Fits(world, feet + wish * 0.05f) && Body.Fits(world, raised) && WaterDepth(world, raised) <= 0)
            {
                _tickVelocity = StepOut;
                _climbing = true;
            }
        }
        else
        {
            moving = _tickVelocity;
            _tickVelocity = (_tickVelocity - AirFall) * AirKeep;
        }
        (_tickSpeed, _tickLeft) = (moving / WaterTick, WaterTick);
    }

    /// <summary>Puts the player's feet at a place, still.</summary>
    public void Teleport(Vector3 feet)
    {
        Body.Position = feet;
        Body.Velocity = Vector3.Zero;
        (_bobbing, _tickVelocity, _tickSpeed, _tickLeft) = (false, 0, 0, 0);
        Teleports++;
    }

    // How far the water's top stands above the feet, up through the water over them, the top of
    // still water an eighth under its cell's and flowing water's lower, and nothing where the feet
    // are above it or their cell holds none. Lava is swum through as water is.
    private static float WaterDepth(VoxelWorld world, Vector3 feet)
    {
        int x = (int)MathF.Floor(feet.X), y = (int)MathF.Floor(feet.Y), z = (int)MathF.Floor(feet.Z);
        var at = world.GetBlock(x, y, z);
        if (!Blocks.IsFluid(at)) return 0;
        for (var above = world.GetBlock(x, y + 1, z); Blocks.SameFluid(at, above); above = world.GetBlock(x, y + 1, z)) (at, y) = (above, y + 1);
        return MathF.Max(0, y + Blocks.WaterHeight(at, waterAbove: false) - feet.Y);
    }
}
