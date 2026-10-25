using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>
/// A mob as a save keeps it and a generator places it, with its kind, variant, place, turn and
/// health, and its home, where a villager goes at night, inside its house, and wanders near by day,
/// or null for a mob that wanders anywhere.
/// </summary>
public readonly record struct MobRecord(MobKind Kind, byte Variant, Vector3 Position, float Yaw, int Health, Vector3? Home);

/// <summary>
/// A mob's life, its health, the red a hit leaves on it and how long it has lain dying, a component
/// of the mob's entity whose behavior ages it in the engine's frame.
/// </summary>
/// <remarks>
/// A hit while the mob is still red does nothing, as in Minecraft, and a mob is taken away once it
/// has lain dying for a second. Its behavior ages each mob's life by <see cref="Pass"/>, which a test
/// of a mob alone calls as the frame would.
/// </remarks>
[Behavior]
public partial struct MobLife
{
    /// <summary>Seconds a hit reddens a mob for, and how long it has lain dying when it is taken away.</summary>
    public const float Unhurt = 0.5f, DeathTime = 1;

    public int Health;

    /// <summary>Seconds left of the red a hit leaves on it, during which another hit does nothing.</summary>
    public float Hurt;

    /// <summary>Seconds since it died, as it falls over, or less than zero while it lives.</summary>
    public float Dying;

    /// <summary>Whether it has fallen over for long enough to be taken away.</summary>
    public readonly bool Gone => Dying >= DeathTime;

    [OnUpdate]
    public static void Age(BehaviorContext ctx)
    {
        if (!ctx.World.TryGetResource<GameStep>(out var step) || step.Seconds <= 0 || step.MobsStill) return;
        foreach (var (id, _) in ctx.Ecs.Query<MobLife>()) ctx.Ecs.GetRef<MobLife>(id).Pass(step.Seconds);
    }

    /// <summary>Fades the red and counts the time dying by so many seconds.</summary>
    public void Pass(float seconds)
    {
        Hurt = MathF.Max(0, Hurt - seconds);
        if (Dying >= 0) Dying += seconds;
    }

    /// <summary>Takes a hit's damage and reddens, falling over where the health is gone, unless still red or dying.</summary>
    /// <returns>Whether it took the hit.</returns>
    public bool Hit(int damage)
    {
        if (Hurt > 0 || Dying >= 0) return false;
        Health -= damage;
        Hurt = Unhurt;
        if (Health <= 0) Dying = 0;
        return true;
    }

    /// <summary>Takes some health with no wait, as burning does, reddening for a moment.</summary>
    public void Harm(int damage)
    {
        if (Dying >= 0) return;
        Health -= damage;
        Hurt = MathF.Max(Hurt, 0.25f);
        if (Health <= 0) Dying = 0;
    }
}

/// <summary>
/// A mob's pose, the turns of its body and its head, its legs' swing and its wings' spread, a
/// component of the mob's entity that its movement writes and the renderer reads by a query.
/// </summary>
/// <remarks>
/// The yaw is a turn about the vertical as the player's is, 0 facing north and growing to the left,
/// so a model built facing north is turned by it alone.
/// </remarks>
public struct MobPose
{
    /// <summary>The body's turn, toward where it walks.</summary>
    public float Yaw;

    /// <summary>The head's turn, which leads the body's and watches the player.</summary>
    public float HeadYaw;

    /// <summary>The head's tilt, positive looking up.</summary>
    public float HeadPitch;

    /// <summary>How far its legs swing, from 0 standing to 1 at five blocks a second, and where in their swing they are, as <see cref="Gait.Step"/> moves them.</summary>
    public float Stride, StridePhase;

    /// <summary>How far its wings are spread, from 0 to 1, as a chicken's flap while it falls.</summary>
    public float Flap;

    /// <summary>Seconds it has moved for, which a wing's flap beats by.</summary>
    public float Swing;

    /// <summary>A wing's angle in radians as it flaps, from <see cref="Flap"/>.</summary>
    public readonly float WingAngle => Flap * (0.6f + 0.6f * MathF.Sin(Swing * 30));
}

/// <summary>
/// A mob in the world, with its body, which falls, swims and is stopped by the blocks as the
/// player's is, and its mind, which wanders along the ways <see cref="Pathfinder"/> finds, runs
/// when hit, looks at the player nearby, for a villager goes home at night through its door, and
/// for a hostile mob hunts the player it sees, striking, shooting or exploding beside it.
/// </summary>
/// <remarks>
/// <para>
/// Each choice follows Minecraft's goals for its passive mobs. It stands a few seconds, then walks
/// to a place up to ten blocks away; a hit makes it run from place to place for a few seconds; and
/// now and then it turns its head to watch a player within six blocks. A villager wanders within 20
/// blocks of its house by day and goes inside it from six in the evening to six in the morning,
/// opening the doors in its way and shutting them behind it.
/// </para>
/// <para>
/// A mob is an entity of an ECS world, the game's or one of its own where none is given, as in a
/// test, with itself, its body, its <see cref="MobLife"/> and its <see cref="MobPose"/> as
/// components, and a hostile one its <see cref="MobHostile"/>, and it reads and writes them there.
/// The class keeps what a struct cannot hold safely, its way and its Random.
/// </para>
/// </remarks>
public sealed partial class Mob
{
    private const float Gravity = 32, FallLimit = 78, JumpSpeed = 9;
    private const float Wander = 10, HomeReach = 20, Panicking = 5;
    private const float Watch = 6;

    private readonly Random _random;
    private List<Step>? _path;
    private int _next;
    private float _wait, _panic, _stuck, _walked, _watching;
    private Vector3 _lastPlace;
    private bool _goingHome, _swimming;
    private Step? _openedDoor;

    public Mob(int id, MobRecord record, int seed, EcsWorld? ecs = null)
    {
        Id = id;
        Kind = record.Kind;
        Variant = record.Variant;
        Body = new Body(record.Kind.HalfWidth, record.Kind.Height) { Position = record.Position };
        Home = record.Home;
        Ecs = ecs ?? new EcsWorld();
        Entity = Ecs.Spawn();
        Ecs.Add(Entity, this);
        Ecs.Add(Entity, Body);
        Ecs.Add(Entity, new MobLife { Health = record.Health, Dying = -1 });
        Ecs.Add(Entity, new MobPose { Yaw = record.Yaw, HeadYaw = record.Yaw });
        if (Kind.Hostile) Ecs.Add(Entity, new MobHostile { ShotWait = 1, Fuse = -1 });
        _random = new Random(seed);
        _wait = 1 + (float)_random.NextDouble() * 6;
        _lastPlace = record.Position;
    }

    /// <summary>A number of its own while the world is open, which <c>voxel.mobs</c> lists it by.</summary>
    public int Id { get; }

    public MobKind Kind { get; }

    public byte Variant { get; }

    public Body Body { get; }

    /// <summary>The ECS world it is an entity of, and its entity there.</summary>
    public EcsWorld Ecs { get; }

    public int Entity { get; }

    // Its life and its pose as its entity holds them, read and written in place.
    private ref MobLife Life => ref Ecs.GetRef<MobLife>(Entity);

    private ref MobPose Pose => ref Ecs.GetRef<MobPose>(Entity);

    /// <inheritdoc cref="MobPose.Yaw"/>
    public float Yaw => Pose.Yaw;

    public int Health => Life.Health;

    public Vector3? Home { get; }

    /// <inheritdoc cref="MobLife.Hurt"/>
    public float Hurt => Life.Hurt;

    /// <inheritdoc cref="MobLife.Dying"/>
    public float Dying => Life.Dying;

    /// <inheritdoc cref="MobLife.Gone"/>
    public bool Gone => Life.Gone;

    /// <inheritdoc cref="MobPose.Stride"/>
    public float Stride => Pose.Stride;

    public float StridePhase => Pose.StridePhase;

    /// <summary>What it is doing, for <c>voxel.mobs</c>.</summary>
    public string Doing => Dying >= 0 ? "dying" : Fuse >= 0 ? "hissing" : Hunting ? "hunting" : _panic > 0 ? "running" : _goingHome ? "going home" : _path is not null ? "walking" : "standing";

    /// <summary>The cells it is walking through, the next first, or none.</summary>
    public IEnumerable<Step> Path => _path is null ? [] : _path.Skip(_next);

    public MobRecord Record => new(Kind, Variant, Body.Position, Yaw, Health, Home);

    /// <summary>
    /// Takes a hit from a place, losing some health, reddening, and knocked away from it, then runs
    /// for a few seconds, or falls over where its health is gone. A hit while it is still red does
    /// nothing, as in Minecraft.
    /// </summary>
    public bool Hit(Vector3 from, int damage)
    {
        ref var life = ref Life;
        if (!life.Hit(damage)) return false;
        var away = Body.Position - from;
        away.Y = 0;
        away = away.LengthSquared() > 1e-6f ? Vector3.Normalize(away) : new Vector3(MathF.Sin(Yaw), 0, MathF.Cos(Yaw));
        Body.Velocity = new Vector3(away.X * 7, 6, away.Z * 7);
        if (life.Dying >= 0) return true;
        // A hostile mob hit turns on the player wherever it is, where a passive one runs.
        if (Kind.Hostile) Hostile.Angry = true;
        else
        {
            _panic = Panicking;
            (_path, _wait, _goingHome) = (null, 0, false);
        }
        return true;
    }

    /// <summary>Takes some health with neither a knock nor a run, as burning does, reddening for a moment.</summary>
    public void Harm(int damage) => Life.Harm(damage);

    /// <summary>Thinks and moves for a frame of so many seconds, with the player's eye and the hour of the day, hunting no one.</summary>
    public void Update(VoxelWorld world, Vector3 playerEye, float hour, float seconds) => Update(world, null, playerEye, hour, seconds, []);

    /// <summary>
    /// Thinks and moves for a frame of so many seconds, a hostile mob hunting the target where one is
    /// given and adding what it does to the player, its strikes, its shots and its blast, to <paramref name="events"/>.
    /// </summary>
    public void Update(VoxelWorld world, MobTarget? target, Vector3 playerEye, float hour, float seconds, List<MobEvent> events)
    {
        var middle = Body.Position + new Vector3(0, Kind.Height * 0.5f, 0);
        _swimming = Blocks.IsWater(world.GetBlock((int)MathF.Floor(middle.X), (int)MathF.Floor(middle.Y), (int)MathF.Floor(middle.Z)));
        var wish = Vector3.Zero;
        var jump = false;
        // A mob lying dying thinks no more, and its life's behavior counts its time.
        if (Dying < 0)
        {
            if (Kind.Burns) Burn(world, hour, seconds);
            (wish, jump) = Kind.Hostile && Hunts(world, target) is { } prey ? Hunt(world, prey, seconds, events) : Think(world, playerEye, hour, seconds);
        }
        Move(world, wish, jump, seconds);
        ShutDoorBehind(world);
    }

    // Chooses what to do and gives the velocity it wishes for along the ground and whether to jump.
    private (Vector3 Wish, bool Jump) Think(VoxelWorld world, Vector3 playerEye, float hour, float seconds)
    {
        var night = hour >= 18 || hour < 6;
        _panic = MathF.Max(0, _panic - seconds);

        if (_path is null)
        {
            _wait -= seconds;
            if (_panic > 0)
            {
                if (_wait <= 0)
                {
                    Choose(world, 5, 10);
                    _wait = _path is null ? 0.5f : 0;
                }
            }
            else if (Kind.OpensDoors && night && Home is { } home && Vector3.DistanceSquared(Body.Position, home) > 2.25f)
            {
                // Home at night, tried again every few seconds while no way is found.
                if (_wait <= 0)
                {
                    _goingHome = WalkTo(world, Cell(home));
                    _wait = _goingHome ? 0 : 3;
                }
            }
            else if (_wait <= 0)
            {
                if (Home is null) Choose(world, 3, Wander);
                else Choose(world, 0, night ? 2 : HomeReach);
                _wait = _path is null ? 1 : 2 + (float)_random.NextDouble() * 6;
            }
        }

        Look(playerEye, seconds);
        if (_path is null) return (Vector3.Zero, false);
        return Follow(world, seconds, _panic > 0 ? Kind.Speed * 2.2f : _goingHome ? Kind.Speed * 1.2f : Kind.Speed);
    }

    // A way to a place between two distances off, around the mob, or around its home unless it is
    // running, tried a few times.
    private void Choose(VoxelWorld world, float least, float most)
    {
        var around = _panic <= 0 && Home is { } home ? home : Body.Position;
        for (int tries = 0; tries < 6; tries++)
        {
            var angle = (float)_random.NextDouble() * MathF.Tau;
            var distance = least + (float)_random.NextDouble() * (most - least);
            int x = (int)MathF.Floor(around.X + MathF.Cos(angle) * distance), z = (int)MathF.Floor(around.Z + MathF.Sin(angle) * distance);
            // The ground there, from a little above down, so the way climbs or drops a few blocks at
            // most, and not water, which a mob crosses where it must and does not wander into.
            for (int y = (int)MathF.Floor(around.Y) + 3; y >= (int)MathF.Floor(around.Y) - 4; y--)
                if (Pathfinder.Stands(world, Kind, new Step(x, y, z)))
                {
                    if (!Blocks.IsWater(world.GetBlock(x, y, z)) && WalkTo(world, new Step(x, y, z))) return;
                    break;
                }
        }
    }

    private bool WalkTo(VoxelWorld world, Step goal)
    {
        // From water the way starts at its top, where the mob floats, so it climbs out onto a bank
        // a block above the water, as out of a well.
        var start = Cell(Body.Position);
        if (Blocks.IsWater(world.GetBlock(start.X, start.Y, start.Z)))
            while (Blocks.IsWater(world.GetBlock(start.X, start.Y + 1, start.Z))) start = start with { Y = start.Y + 1 };
        var path = Pathfinder.Find(world, Kind, start, goal, reach: 32);
        if (path is null || path.Count == 0) return false;
        (_path, _next, _stuck, _walked) = (path, 0, 0, 0);
        return true;
    }

    private static Step Cell(Vector3 at) => new((int)MathF.Floor(at.X), (int)MathF.Floor(at.Y + 0.2f), (int)MathF.Floor(at.Z));

    // Walks toward the next cell of the way, opening a shut door in it, jumping where it is higher
    // and the body is stopped, and drops the way where the end is reached or the body is stuck.
    private (Vector3, bool) Follow(VoxelWorld world, float seconds, float speed)
    {
        var path = _path!;
        var cell = path[_next];
        var target = new Vector3(cell.X + 0.5f, cell.Y, cell.Z + 0.5f);
        var flat = target - Body.Position;
        flat.Y = 0;
        // A cell is reached once the body's middle is near its middle and its feet near its floor.
        if (flat.LengthSquared() < 0.09f && MathF.Abs(Body.Position.Y - cell.Y) < 0.8f)
        {
            if (++_next >= path.Count)
            {
                _path = null;
                _goingHome = false;
                return (Vector3.Zero, false);
            }
            return Follow(world, seconds, speed);
        }

        if (Kind.OpensDoors) OpenDoorAt(world, cell);

        // Stuck where it has moved under a tenth of a block in a second and a half, or walking one
        // way for 20 seconds.
        _walked += seconds;
        _stuck += seconds;
        if (Vector3.DistanceSquared(Body.Position, _lastPlace) > 0.01f) (_lastPlace, _stuck) = (Body.Position, 0);
        if (_stuck > 1.5f || _walked > 20)
        {
            _path = null;
            _goingHome = false;
            _wait = 1;
            return (Vector3.Zero, false);
        }

        var way = Vector3.Normalize(flat);
        // A jump from the ground or from water onto a cell higher than a step, as out of a well.
        var jump = (Body.OnGround || _swimming) && cell.Y > Body.Position.Y + 0.55f && flat.LengthSquared() < 2.25f;
        return (way * speed, jump);
    }

    // Opens a shut door in a cell of the way, either half, remembering it to shut behind.
    private void OpenDoorAt(VoxelWorld world, Step cell)
    {
        for (int y = cell.Y; y <= cell.Y + 1; y++)
            if (Pathfinder.IsShutDoor(world.GetBlock(cell.X, y, cell.Z)) && world.Toggle(cell.X, y, cell.Z))
            {
                _openedDoor = cell with { Y = y };
                return;
            }
    }

    // Shuts a door it opened once its body is clear of the door's cell by a block.
    private void ShutDoorBehind(VoxelWorld world)
    {
        if (_openedDoor is not { } door) return;
        var middle = new Vector3(door.X + 0.5f, Body.Position.Y, door.Z + 0.5f);
        if (Vector3.DistanceSquared(middle, Body.Position) < 1.7f * 1.7f && Dying < 0) return;
        if (!Pathfinder.IsShutDoor(world.GetBlock(door.X, door.Y, door.Z)) && Blocks.Get(world.GetBlock(door.X, door.Y, door.Z)).Toggled is not null)
            world.Toggle(door.X, door.Y, door.Z);
        _openedDoor = null;
    }

    // Turns the head toward the player within six blocks now and then for a few seconds, and
    // otherwise along the body.
    private void Look(Vector3 playerEye, float seconds)
    {
        ref var pose = ref Pose;
        var eye = Body.Position + new Vector3(0, Kind.Eyes, 0);
        var toPlayer = playerEye - eye;
        _watching -= seconds;
        if (_watching <= -1 && toPlayer.LengthSquared() < Watch * Watch && _random.NextDouble() < seconds * 0.4)
            _watching = 2 + (float)_random.NextDouble() * 2;
        if (_watching > 0 && _panic <= 0 && toPlayer.LengthSquared() < Watch * Watch * 1.5f)
        {
            var yaw = MathF.Atan2(-toPlayer.X, -toPlayer.Z);
            // The head turns no further than 75 degrees from the body, which turns after it beyond that.
            var off = Math.Clamp(Angle(yaw - pose.Yaw), -1.3f, 1.3f);
            pose.HeadYaw = Turn(pose.HeadYaw, pose.Yaw + off, 8 * seconds);
            var flat = MathF.Sqrt(toPlayer.X * toPlayer.X + toPlayer.Z * toPlayer.Z);
            pose.HeadPitch += (Math.Clamp(MathF.Atan2(toPlayer.Y, flat), -0.8f, 0.8f) - pose.HeadPitch) * MathF.Min(1, 8 * seconds);
            if (MathF.Abs(Angle(yaw - pose.Yaw)) > 1.3f && _path is null) pose.Yaw = Turn(pose.Yaw, yaw, 2 * seconds);
        }
        else
        {
            pose.HeadYaw = Turn(pose.HeadYaw, pose.Yaw, 6 * seconds);
            pose.HeadPitch += (0 - pose.HeadPitch) * MathF.Min(1, 6 * seconds);
        }
    }

    private void Move(VoxelWorld world, Vector3 wish, bool jump, float seconds)
    {
        var swimming = _swimming;
        var grip = 1 - MathF.Exp(-(Body.OnGround && Hurt < MobLife.Unhurt - 0.15f ? 12 : swimming ? 4 : 1.5f) * seconds);
        Body.Velocity.X += (wish.X - Body.Velocity.X) * grip;
        Body.Velocity.Z += (wish.Z - Body.Velocity.Z) * grip;
        if (swimming)
        {
            // A mob floats in water and bobs at its top, as Minecraft's do.
            Body.Velocity.Y = jump ? JumpSpeed * 0.8f : MathF.Min(Body.Velocity.Y + 24 * seconds, 2.5f);
        }
        else
        {
            Body.Velocity.Y = MathF.Max(Body.Velocity.Y - Gravity * seconds, Kind.FallsSlowly ? -2.5f : -FallLimit);
            if (jump) Body.Velocity.Y = JumpSpeed;
        }

        var before = Body.Position;
        Body.Move(world, Body.Velocity * seconds, keepToEdges: false);
        var moved = Body.Position - before;
        var along = MathF.Sqrt(moved.X * moved.X + moved.Z * moved.Z);
        ref var pose = ref Pose;
        // A hunter faces its prey as it moves, a skeleton backing away from the player among them.
        if (along > 1e-4f && wish != Vector3.Zero && !Hunting) pose.Yaw = Turn(pose.Yaw, MathF.Atan2(-moved.X, -moved.Z), 10 * seconds);
        Gait.Step(ref pose.Stride, ref pose.StridePhase, along, seconds);
        pose.Swing += seconds;
        pose.Flap += ((Kind.FallsSlowly && !Body.OnGround && !swimming ? 1 : 0) - pose.Flap) * MathF.Min(1, 12 * seconds);
    }

    private static float Angle(float a) => MathF.IEEERemainder(a, MathF.Tau);

    // A turn toward another by at most so much, the short way round.
    private static float Turn(float from, float to, float most) => from + Math.Clamp(Angle(to - from), -most, most);
}
