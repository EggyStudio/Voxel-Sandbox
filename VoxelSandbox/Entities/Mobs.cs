using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>A mob that died and was taken away, an event the game drops its loot for.</summary>
public readonly record struct MobDied(Mob Mob);

/// <summary>
/// The behaviors of the mobs, each an entity of the engine's ECS with its <see cref="Mob"/>, the
/// mob's <see cref="Body"/>, its <see cref="MobLife"/> and its <see cref="MobPose"/>, which think
/// and move the mobs that are not hostile, push every mob apart and take away the dead in the
/// engine's frame, the hostile ones thinking in <see cref="MobHostile"/>'s.
/// </summary>
/// <remarks>
/// They read the voxel world, the player, the hour and the seconds from the game's
/// <see cref="GameStep"/> on the main thread, which alone may read the world, and send what the
/// hostile mobs did to the player as <see cref="MobEvent"/>s and each mob taken away as a
/// <see cref="MobDied"/>, which the game carries out. Nothing steps while the mobs stand still. Each
/// also takes its world and its inputs as arguments, so a test steps the mobs of an ECS world of its
/// own as the frame steps the game's.
/// </remarks>
[Behavior]
public partial struct MobMind
{
    [OnUpdate]
    public static void Think(BehaviorContext ctx)
    {
        if (!ctx.World.TryGetResource<GameStep>(out var step) || step.Seconds <= 0 || step.MobsStill) return;
        var events = new List<MobEvent>();
        Think(ctx.Ecs, step.World, step.Hunted ? new MobTarget(step.Player, step.Eye) : null, step.Eye, step.Hour, step.Seconds, events);
        foreach (var e in events) ctx.World.SendEvent(e);
    }

    /// <summary>The hostile mobs spawned in the dark about the player and taken away far from it, while the player lives in a world that stands.</summary>
    [OnUpdate]
    [InState(Living.Yes)]
    [RunIf(nameof(Playing))]
    public static void Spawn(BehaviorContext ctx)
    {
        if (ctx.World.TryGetResource<VoxelGame>(out var game) && ctx.World.TryGetResource<GameStep>(out var step))
            game.Mobs.Spawn(step.World, step.Player.Position, step.Hour, step.Seconds);
    }

    /// <summary>Whether the world stands to be played.</summary>
    public static bool Playing(World world) => world.TryGetResource<State<WorldState>>(out var state) && state.Current == WorldState.Playing;

    [OnPostUpdate]
    public static void Crowd(BehaviorContext ctx)
    {
        if (ctx.World.TryGetResource<GameStep>(out var step) && step.Seconds > 0 && !step.MobsStill) Crowd(ctx.Ecs, step.Player, step.Seconds);
    }

    [OnPostUpdate]
    public static void Vanish(BehaviorContext ctx)
    {
        if (!ctx.World.TryGetResource<GameStep>(out var step) || step.Seconds <= 0 || step.MobsStill) return;
        foreach (var mob in Vanish(ctx.Ecs)) ctx.World.SendEvent(new MobDied(mob));
    }

    /// <summary>
    /// Thinks and moves every mob that is not hostile whose columns are loaded around it, which
    /// <paramref name="target"/> and <paramref name="events"/> are given to as to the hostile ones'.
    /// </summary>
    public static void Think(EcsWorld ecs, VoxelWorld world, MobTarget? target, Vector3 playerEye, float hour, float seconds, List<MobEvent> events)
    {
        foreach (var (_, mob, body, _, _) in ecs.Query<Mob, Body, MobLife, MobPose>().Without<MobHostile>())
        {
            var (cx, cz) = Mobs.ColumnOf(body.Position);
            if (Mobs.Around(world, cx, cz)) mob.Update(world, target, playerEye, hour, seconds, events);
        }
    }

    /// <summary>Pushes each pair of overlapping mobs apart along x and z, and a mob the player overlaps away from the player.</summary>
    public static void Crowd(EcsWorld ecs, Body player, float seconds)
    {
        var bodies = new List<Body>();
        foreach (var (_, _, body) in ecs.Query<Mob, Body>()) bodies.Add(body);
        var strength = MathF.Min(1, 8 * seconds);
        for (int i = 0; i < bodies.Count; i++)
        {
            var a = bodies[i];
            for (int j = i + 1; j < bodies.Count; j++)
            {
                var b = bodies[j];
                if (Apart(a, b) is not { } push) continue;
                a.Velocity -= push * strength * 4;
                b.Velocity += push * strength * 4;
            }
            if (Apart(player, a) is { } away) a.Velocity += away * strength * 6;
        }
    }

    /// <summary>Takes away the mobs that have fallen over for long enough, and gives them.</summary>
    public static List<Mob> Vanish(EcsWorld ecs)
    {
        var gone = new List<(int Id, Mob Mob)>();
        foreach (var (id, mob, life) in ecs.Query<Mob, MobLife>())
            if (life.Gone) gone.Add((id, mob));
        foreach (var (id, _) in gone) ecs.Despawn(id);
        return [.. gone.Select(g => g.Mob)];
    }

    // The way from a box to another along x and z, as long as they overlap, or null where they do not.
    private static Vector3? Apart(Body a, Body b)
    {
        Vector3 aMin = a.Min, aMax = a.Max, bMin = b.Min, bMax = b.Max;
        if (aMin.X >= bMax.X || aMax.X <= bMin.X || aMin.Z >= bMax.Z || aMax.Z <= bMin.Z || aMin.Y >= bMax.Y || aMax.Y <= bMin.Y) return null;
        var way = b.Position - a.Position;
        way.Y = 0;
        var overlap = MathF.Min(MathF.Min(aMax.X - bMin.X, bMax.X - aMin.X), MathF.Min(aMax.Z - bMin.Z, bMax.Z - aMin.Z));
        return way.LengthSquared() > 1e-6f ? Vector3.Normalize(way) * overlap : new Vector3(overlap, 0, 0);
    }
}

/// <summary>
/// The mobs of the loaded columns, those of the columns unloaded kept by their column, and which
/// columns have had their mobs placed, so a column's animals and villagers are placed once, when it
/// is first generated, and come back where they were left each time it loads after.
/// </summary>
/// <remarks>
/// <para>
/// A mob thinks and moves only while the columns around its own are loaded, so it never walks into
/// one that is not and never stands on the air an unloaded column reads as. When a column unloads,
/// the mobs standing in it go with it, and come back with it.
/// </para>
/// <para>
/// Mobs push each other apart and are pushed away by the player, as Minecraft's are, a little each
/// frame by how far their boxes overlap.
/// </para>
/// <para>
/// Each mob is an entity of an ECS world, its <see cref="Mob"/> and its <see cref="Body"/>, which
/// <see cref="MobMind"/> steps in the engine's frame.
/// </para>
/// <para>
/// Hostile mobs spawn as Minecraft's do, in packs of one to four on a solid block in the dark,
/// where no light from blocks reaches and the sky's light, less its darkening at night, is no
/// brighter than a roll from 0 to 7. Four tries a second look for a place between 24 and 64 blocks
/// from the player while fewer than 30 are about, and a hostile mob more than 128 blocks off is
/// gone at once, and one more than 32 off now and then. They are not kept by a column that unloads,
/// nor saved.
/// </para>
/// </remarks>
public sealed class Mobs
{
    // How far the crosshair reaches a mob, as Minecraft's does.
    public const float Reach = 3.5f;

    // At most so many hostile mobs about, a try for a pack each quarter second.
    private const int HostileCap = 30;
    private const float SpawnEvery = 0.25f;

    private float _spawnWait;

    private readonly EcsWorld _ecs;
    private readonly Dictionary<(int X, int Z), List<MobRecord>> _kept;
    private readonly HashSet<(int X, int Z)> _placed;
    private readonly Random _random = new();
    private int _nextId = 1;

    /// <summary>Begins with the mobs a save kept, or none, as entities of an ECS world.</summary>
    public Mobs(EcsWorld ecs, Dictionary<(int X, int Z), List<MobRecord>>? kept = null, HashSet<(int X, int Z)>? placed = null)
    {
        _ecs = ecs;
        _kept = kept ?? [];
        _placed = placed ?? [];
    }

    /// <summary>The mobs of the loaded columns, gathered as they stand.</summary>
    public IReadOnlyList<Mob> All
    {
        get
        {
            var all = new List<Mob>();
            foreach (var (_, mob) in _ecs.Query<Mob>()) all.Add(mob);
            return all;
        }
    }

    /// <summary>How many mobs the loaded columns hold.</summary>
    public int Count => _ecs.Count<Mob>();

    /// <summary>Whether the mobs stand still and think nothing, as for a capture.</summary>
    public bool Still { get; set; }

    /// <summary>The hostile mobs alive.</summary>
    public int Hostile => All.Count(m => m.Kind.Hostile && m.Dying < 0);

    /// <summary>Mobs kept by columns not loaded.</summary>
    public int Kept => _kept.Values.Sum(l => l.Count);

    /// <summary>Puts a mob into the world, a number of its own given.</summary>
    public Mob Add(MobRecord record)
    {
        return new Mob(_nextId++, record, _random.Next(), _ecs);
    }

    /// <summary>Brings back a column's kept mobs as it loads, or places those its generator gave it the first time.</summary>
    public void Joined(ChunkColumn column)
    {
        var key = (column.X, column.Z);
        if (_placed.Add(key))
        {
            foreach (var spawn in column.Spawns) Add(spawn);
        }
        else if (_kept.Remove(key, out var kept))
            foreach (var record in kept) Add(record);
        column.Spawns.Clear();
    }

    /// <summary>Keeps the mobs standing in a column as it unloads.</summary>
    public void Leaving(int x, int z)
    {
        List<MobRecord>? kept = null;
        // The mobs are walked for their column, since the ECS finds an entity by its id alone.
        Despawn(mob =>
        {
            if (mob.Dying >= 0 || ColumnOf(mob) != (x, z)) return false;
            if (!mob.Kind.Hostile) (kept ??= []).Add(mob.Record);
            return true;
        });
        if (kept is not null) _kept[(x, z)] = kept;
    }

    // Takes away the mobs a rule picks, and gives how many.
    private int Despawn(Func<Mob, bool> picked)
    {
        var gone = new List<int>();
        foreach (var (id, mob) in _ecs.Query<Mob>())
            if (picked(mob)) gone.Add(id);
        foreach (var id in gone) _ecs.Despawn(id);
        return gone.Count;
    }

    /// <summary>The column a mob stands in.</summary>
    public static (int X, int Z) ColumnOf(Mob mob) => ColumnOf(mob.Body.Position);

    /// <summary>The column a place lies in.</summary>
    public static (int X, int Z) ColumnOf(Vector3 at) => (VoxelWorld.ColumnOf((int)MathF.Floor(at.X)), VoxelWorld.ColumnOf((int)MathF.Floor(at.Z)));

    /// <summary>Every column that has had its mobs placed, with the mobs it holds, the loaded ones' by where they stand.</summary>
    public IEnumerable<((int X, int Z) Column, IReadOnlyList<MobRecord> Mobs)> Columns()
    {
        var loaded = new Dictionary<(int, int), List<MobRecord>>();
        foreach (var mob in All)
            if (mob.Dying < 0 && !mob.Kind.Hostile)
            {
                var key = ColumnOf(mob);
                if (!loaded.TryGetValue(key, out var list)) loaded[key] = list = [];
                list.Add(mob.Record);
            }
        foreach (var key in _placed)
        {
            if (loaded.TryGetValue(key, out var here)) yield return (key, here);
            else if (_kept.TryGetValue(key, out var kept)) yield return (key, kept);
            else yield return (key, []);
        }
    }

    /// <summary>Whether the columns around a column are loaded, which a mob's own must be for it to think and move.</summary>
    public static bool Around(VoxelWorld world, int cx, int cz)
    {
        for (int dz = -1; dz <= 1; dz++)
            for (int dx = -1; dx <= 1; dx++)
                if (!world.HasColumn(cx + dx, cz + dz)) return false;
        return true;
    }

    /// <summary>The nearest living mob a ray from a point meets within a reach, and how far along the ray.</summary>
    public (Mob Mob, float Distance)? Cast(Vector3 from, Vector3 direction, float reach)
    {
        direction = Vector3.Normalize(direction);
        (Mob, float)? nearest = null;
        foreach (var mob in All)
        {
            if (mob.Dying >= 0) continue;
            if (Meets(from, direction, mob.Body.Min, mob.Body.Max) is { } distance && distance <= reach && (nearest is null || distance < nearest.Value.Item2))
                nearest = (mob, distance);
        }
        return nearest;
    }

    // How far along a ray it enters a box, or null where it misses, by the slabs between the box's faces.
    private static float? Meets(Vector3 from, Vector3 direction, Vector3 min, Vector3 max)
    {
        float near = 0, far = float.MaxValue;
        for (int axis = 0; axis < 3; axis++)
        {
            float o = from[axis], d = direction[axis], lo = min[axis], hi = max[axis];
            if (MathF.Abs(d) < 1e-8f)
            {
                if (o < lo || o > hi) return null;
                continue;
            }
            float t0 = (lo - o) / d, t1 = (hi - o) / d;
            if (t0 > t1) (t0, t1) = (t1, t0);
            near = MathF.Max(near, t0);
            far = MathF.Min(far, t1);
            if (near > far) return null;
        }
        return near;
    }

    /// <summary>Whether a living mob's box takes up any of the boxes a block would stand in at a place, which the block is not placed into.</summary>
    public bool Overlaps(int x, int y, int z, IReadOnlyList<Box> boxes)
    {
        foreach (var mob in All)
            if (mob.Dying < 0 && mob.Body.Overlaps(x, y, z, boxes)) return true;
        return false;
    }

    /// <summary>
    /// How much the sky's light is darkened at an hour, Minecraft's 0 by day and 11 at night, rising
    /// from half past five in the evening to seven and falling from five in the morning to half past six.
    /// </summary>
    public static int Darkening(float hour) =>
        hour is >= 6.5f and <= 17.5f ? 0
        : hour > 17.5f && hour < 19 ? (int)((hour - 17.5f) / 1.5f * 11)
        : hour > 5 && hour < 6.5f ? (int)((6.5f - hour) / 1.5f * 11)
        : 11;

    /// <summary>Whether a hostile mob of a kind can spawn with its feet in a cell, which needs a solid block under it, the cells it takes free, and the cell dark enough by a roll.</summary>
    public static bool Spawnable(VoxelWorld world, MobKind kind, int x, int y, int z, float hour, int roll)
    {
        if (!Blocks.IsOpaque(world.GetBlock(x, y - 1, z))) return false;
        for (int i = 0; i < (int)MathF.Ceiling(kind.Height); i++)
        {
            var block = world.GetBlock(x, y + i, z);
            if (world.BoxesAt(x, y + i, z).Count > 0 || Blocks.IsWater(block)) return false;
        }
        var (sky, light) = world.GetLight(x, y, z);
        return light == 0 && Math.Max(0, sky - Darkening(hour)) <= roll;
    }

    /// <summary>Tries for a pack of hostile mobs in the dark around the player, four tries a second, and takes away those far from it.</summary>
    public void Spawn(VoxelWorld world, Vector3 player, float hour, float seconds)
    {
        Despawn(m => m.Kind.Hostile && m.Dying < 0 && Despawns(Vector3.Distance(m.Body.Position, player), seconds));
        _spawnWait -= seconds;
        if (_spawnWait > 0 || Hostile >= HostileCap) return;
        _spawnWait = SpawnEvery;
        var angle = _random.NextDouble() * Math.Tau;
        var distance = 24 + _random.NextDouble() * 40;
        int x = (int)MathF.Floor(player.X + (float)(Math.Cos(angle) * distance)), z = (int)MathF.Floor(player.Z + (float)(Math.Sin(angle) * distance));
        if (!Around(world, VoxelWorld.ColumnOf(x), VoxelWorld.ColumnOf(z))) return;
        var top = world.Top(x, z);
        if (top < ChunkColumn.Bottom) return;
        // A height at random from the bottom to over the ground, and the first open cell on a block under it.
        var y = ChunkColumn.Bottom + 1 + _random.Next(top - ChunkColumn.Bottom + 1);
        while (y > ChunkColumn.Bottom + 1 && (world.BoxesAt(x, y - 1, z).Count == 0 || world.BoxesAt(x, y, z).Count > 0)) y--;
        var kind = Pick(_random.Next(100));
        var pack = 1 + _random.Next(4);
        for (int tries = 0; tries < pack * 3 && pack > 0 && Hostile < HostileCap; tries++)
        {
            int px = x + _random.Next(-3, 4), pz = z + _random.Next(-3, 4);
            for (int py = y + 1; py >= y - 1; py--)
                if (Spawnable(world, kind, px, py, pz, hour, _random.Next(8)))
                {
                    Add(new MobRecord(kind, 0, new Vector3(px + 0.5f, py, pz + 0.5f), (float)(_random.NextDouble() * Math.Tau), kind.Health, null));
                    pack--;
                    break;
                }
        }
    }

    // Gone at once beyond 128 blocks and beyond 32 about once in forty seconds, as Minecraft's despawning.
    private bool Despawns(float distance, float seconds) => distance > 128 || distance > 32 && _random.NextDouble() < seconds / 40;

    private static MobKind Pick(int roll)
    {
        foreach (var (kind, weight) in MobKinds.Night)
        {
            if (roll < weight) return kind;
            roll -= weight;
        }
        return MobKinds.Zombie;
    }

    /// <summary>Takes away every mob of a kind, or every mob where none is given, from the loaded columns.</summary>
    public int Clear(MobKind? kind) => Despawn(m => kind is null || m.Kind == kind);
}
