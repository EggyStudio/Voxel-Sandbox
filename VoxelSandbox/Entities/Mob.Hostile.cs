using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>The player as a hostile mob hunts it, by its box and its eye.</summary>
public readonly record struct MobTarget(Body Body, Vector3 Eye);

/// <summary>Something a mob does to the player, which the game carries out.</summary>
public abstract record MobEvent(Mob Mob);

/// <summary>A strike of so much damage from where the mob stands.</summary>
public sealed record MobStrike(Mob Mob, int Damage, Vector3 From) : MobEvent(Mob);

/// <summary>An arrow loosed from a place with a velocity.</summary>
public sealed record MobShot(Mob Mob, Vector3 From, Vector3 Velocity) : MobEvent(Mob);

/// <summary>An explosion of a power at a place, as a creeper's of three.</summary>
public sealed record MobBlast(Mob Mob, Vector3 At, float Power) : MobEvent(Mob);

/// <summary>
/// A hostile mob's hunt, whether it hunts the player and has been angered, its waits between its
/// strikes, its shots and its ways found again, its creeper's fuse and its burning by day, a
/// component of the mob's entity whose behavior is the hostile mobs' mind in the engine's frame.
/// </summary>
/// <remarks>
/// The hostile mobs think and move in this behavior's system and the others in
/// <see cref="MobMind"/>'s, each reading its mob's body, life, pose and hunt by one query.
/// </remarks>
[Behavior]
public partial struct MobHostile
{
    /// <summary>Seconds until it finds its way to the player again, strikes again and shoots again.</summary>
    public float Repath, StrikeWait, ShotWait;

    /// <summary>Seconds it has burned under the sun since its last point of health lost to it.</summary>
    public float Burn;

    /// <summary>Whether a hit has angered it, so it hunts the player wherever it is.</summary>
    public bool Angry;

    /// <summary>Whether it hunts the player, which raises a zombie's arms and a skeleton's bow.</summary>
    public bool Hunting;

    /// <summary>Seconds a creeper's fuse has burned, or less than zero while it is not lit.</summary>
    public float Fuse;

    /// <summary>
    /// Thinks and moves every hostile mob whose columns are loaded around it, hunting the player as
    /// in survival, and sends what they do to the player as <see cref="MobEvent"/>s.
    /// </summary>
    [OnUpdate]
    public static void Hunt(BehaviorContext ctx)
    {
        if (!ctx.World.TryGetResource<GameStep>(out var step) || step.Seconds <= 0 || step.MobsStill) return;
        var events = new List<MobEvent>();
        var target = step.Hunted ? new MobTarget(step.Player, step.Eye) : (MobTarget?)null;
        foreach (var (_, mob, body, _, _, _) in ctx.Ecs.Query<Mob, Body, MobLife, MobPose, MobHostile>())
        {
            var (cx, cz) = Mobs.ColumnOf(body.Position);
            if (Mobs.Around(step.World, cx, cz)) mob.Update(step.World, target, step.Eye, step.Hour, step.Seconds, events);
        }
        foreach (var e in events) ctx.World.SendEvent(e);
    }
}

public sealed partial class Mob
{
    // A strike or a shot every second or two, and a creeper's fuse of a second and a half, which it
    // gives up where the player gets seven blocks away, as Minecraft's do.
    private const float StrikeEvery = 1, ShotEvery = 2, FuseTime = 1.5f, FuseReach = 3, FuseLeave = 7, ArrowSpeed = 30;

    // Its hunt as its entity holds it, which a hostile mob alone has.
    private ref MobHostile Hostile => ref Ecs.GetRef<MobHostile>(Entity);

    /// <inheritdoc cref="MobHostile.Fuse"/>
    public float Fuse => Kind.Hostile ? Hostile.Fuse : -1;

    /// <inheritdoc cref="MobHostile.Hunting"/>
    public bool Hunting => Kind.Hostile && Hostile.Hunting;

    /// <summary>Whether it blew itself up, which leaves nothing to drop, kept here since the loot is reckoned once its entity is gone.</summary>
    public bool Exploded { get; private set; }

    // The target where it is the hunter's to hunt, within its range and seen, or anywhere within
    // twice that once the hunt has begun or a hit has angered it.
    private MobTarget? Hunts(VoxelWorld world, MobTarget? target)
    {
        ref var hostile = ref Hostile;
        if (target is not { } prey)
        {
            hostile.Hunting = false;
            return null;
        }
        var eye = Body.Position + new Vector3(0, Kind.Eyes, 0);
        var distance = Vector3.Distance(eye, prey.Eye);
        var range = hostile.Hunting || hostile.Angry ? Kind.FollowRange * 2 : Kind.FollowRange;
        hostile.Hunting = distance <= range && (hostile.Hunting || hostile.Angry || Sees(world, eye, prey.Eye));
        if (!hostile.Hunting) hostile.Fuse = -1;
        return hostile.Hunting ? prey : null;
    }

    /// <summary>Whether no block stands between two points.</summary>
    public static bool Sees(VoxelWorld world, Vector3 from, Vector3 to)
    {
        var way = to - from;
        var length = way.Length();
        return length < 1e-3f || VoxelRay.Cast(world, from, way / length, length) is not { } hit || hit.Distance >= length - 0.05f;
    }

    private (Vector3 Wish, bool Jump) Hunt(VoxelWorld world, MobTarget prey, float seconds, List<MobEvent> events)
    {
        ref var hostile = ref Hostile;
        hostile.StrikeWait -= seconds;
        hostile.ShotWait -= seconds;
        hostile.Repath -= seconds;
        var eye = Body.Position + new Vector3(0, Kind.Eyes, 0);
        var to = prey.Body.Position - Body.Position;
        var flat = new Vector2(to.X, to.Z);
        var distance = to.Length();
        Face(prey.Eye, seconds);

        if (Kind.Explodes)
        {
            if (hostile.Fuse >= 0)
            {
                hostile.Fuse = distance > FuseLeave ? -1 : hostile.Fuse + seconds;
                if (hostile.Fuse < FuseTime) return (Vector3.Zero, false);
                events.Add(new MobBlast(this, Body.Position + new Vector3(0, Kind.Height / 2, 0), 3));
                Exploded = true;
                ref var life = ref Life;
                (life.Health, life.Dying) = (0, MobLife.DeathTime);
                return (Vector3.Zero, false);
            }
            if (distance < FuseReach && Sees(world, eye, prey.Eye))
            {
                hostile.Fuse = 0;
                return (Vector3.Zero, false);
            }
        }

        if (Kind.Shoots && distance < 15 && Sees(world, eye, prey.Eye))
        {
            if (hostile.ShotWait <= 0)
            {
                hostile.ShotWait = ShotEvery;
                events.Add(new MobShot(this, eye + Vector3.Normalize(prey.Eye - eye) * 0.6f, Aim(eye, prey)));
            }
            // It keeps between five and ten blocks off, backing away from a player who closes in.
            if (distance < 5 && flat.LengthSquared() > 1e-4f)
            {
                _path = null;
                var away = -Vector2.Normalize(flat) * Kind.ChaseSpeed * 0.7f;
                return (new Vector3(away.X, 0, away.Y), false);
            }
            if (distance < 10)
            {
                _path = null;
                return (Vector3.Zero, false);
            }
        }

        // Within reach of the player, a strike each second, still pressing in.
        if (!Kind.Shoots && !Kind.Explodes && flat.Length() < Kind.HalfWidth + prey.Body.HalfWidth + 0.7f && MathF.Abs(to.Y) < 1.5f)
        {
            if (hostile.StrikeWait <= 0)
            {
                hostile.StrikeWait = StrikeEvery;
                events.Add(new MobStrike(this, Kind.AttackDamage, Body.Position));
            }
            return (Ahead(flat) * Kind.ChaseSpeed * 0.3f, false);
        }

        // Near and on its level it runs straight at the player, and otherwise along a way found to
        // where the player stands, found again every half second.
        if (flat.Length() < 3 && MathF.Abs(to.Y) < 0.6f)
        {
            _path = null;
            return (Ahead(flat) * Kind.ChaseSpeed, Body.OnGround && Blocked(world, flat));
        }
        if (hostile.Repath <= 0 || _path is null)
        {
            hostile.Repath = 0.5f;
            if (Ground(world, prey.Body.Position) is not { } goal || !WalkTo(world, goal)) _path = null;
        }
        if (_path is null) return (Ahead(flat) * Kind.ChaseSpeed, Body.OnGround && Blocked(world, flat));
        return Follow(world, seconds, Kind.ChaseSpeed);
    }

    private static Vector3 Ahead(Vector2 flat) => flat.LengthSquared() > 1e-6f ? new Vector3(Vector2.Normalize(flat).X, 0, Vector2.Normalize(flat).Y) : Vector3.Zero;

    // Whether a block stands a step ahead at the feet with room above it, which a jump climbs.
    private bool Blocked(VoxelWorld world, Vector2 flat)
    {
        if (flat.LengthSquared() < 1e-6f) return false;
        var ahead = Body.Position + new Vector3(Vector2.Normalize(flat).X, 0, Vector2.Normalize(flat).Y) * (Kind.HalfWidth + 0.4f);
        int x = (int)MathF.Floor(ahead.X), y = (int)MathF.Floor(Body.Position.Y + 0.1f), z = (int)MathF.Floor(ahead.Z);
        return world.BoxesAt(x, y, z).Count > 0 && world.BoxesAt(x, y + 1, z).Count == 0 && world.BoxesAt(x, y + 2, z).Count == 0;
    }

    // The cell the mob can stand in at or under a place, up to four blocks down, as under a player in the air.
    private Step? Ground(VoxelWorld world, Vector3 at)
    {
        int x = (int)MathF.Floor(at.X), y = (int)MathF.Floor(at.Y + 0.2f), z = (int)MathF.Floor(at.Z);
        for (int down = 0; down <= 4; down++)
            if (Pathfinder.Stands(world, Kind, new Step(x, y - down, z))) return new Step(x, y - down, z);
        return null;
    }

    // An arrow's velocity at the player's chest from the bow, raised for the drop of its flight under
    // the arrow's gravity of 20, with a little scatter, as a skeleton's on normal difficulty.
    private Vector3 Aim(Vector3 from, MobTarget prey)
    {
        var chest = prey.Body.Position + new Vector3(0, prey.Body.Height * 0.6f, 0);
        var to = chest - from;
        var flat = new Vector2(to.X, to.Z).Length();
        var time = flat / ArrowSpeed;
        to.Y += 0.5f * Arrows.Gravity * time * time;
        var scatter = new Vector3((float)_random.NextDouble() - 0.5f, (float)_random.NextDouble() - 0.5f, (float)_random.NextDouble() - 0.5f) * 0.06f;
        return Vector3.Normalize(Vector3.Normalize(to) + scatter) * ArrowSpeed;
    }

    // Turns the head to look at a point and the body after it.
    private void Face(Vector3 point, float seconds)
    {
        ref var pose = ref Pose;
        var eye = Body.Position + new Vector3(0, Kind.Eyes, 0);
        var to = point - eye;
        var yaw = MathF.Atan2(-to.X, -to.Z);
        pose.HeadYaw = Turn(pose.HeadYaw, yaw, 10 * seconds);
        pose.Yaw = Turn(pose.Yaw, yaw, 6 * seconds);
        var flat = MathF.Sqrt(to.X * to.X + to.Z * to.Z);
        pose.HeadPitch += (Math.Clamp(MathF.Atan2(to.Y, flat), -0.8f, 0.8f) - pose.HeadPitch) * MathF.Min(1, 8 * seconds);
    }

    // Under the open sky by day, out of water, a point of health a second, as Minecraft's undead burn.
    private void Burn(VoxelWorld world, float hour, float seconds)
    {
        var head = Body.Position + new Vector3(0, Kind.Height - 0.1f, 0);
        int x = (int)MathF.Floor(head.X), y = (int)MathF.Floor(head.Y), z = (int)MathF.Floor(head.Z);
        ref var hostile = ref Hostile;
        if (hour < 6.5f || hour > 17.5f || _swimming || world.GetLight(x, y, z).Sky < Lighting.Max)
        {
            hostile.Burn = 0;
            return;
        }
        hostile.Burn += seconds;
        if (hostile.Burn < 1) return;
        hostile.Burn = 0;
        Harm(1);
    }
}
