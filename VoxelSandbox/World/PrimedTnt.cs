using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>A TNT lit and burning down its fuse, an entity of the engine's ECS beside its <see cref="Body"/>.</summary>
/// <remarks>
/// Its behavior burns it in the engine's frame, reading the voxel world and the seconds from the
/// game's <see cref="GameStep"/> on the main thread, which alone may read the world, and leaves
/// where each went off in <see cref="PrimedTnt.Exploded"/>, which the game sets off. It also takes
/// its world and its inputs as arguments, so a test burns the TNT of an ECS world of its own.
/// </remarks>
[Behavior]
public partial struct LitTnt
{
    /// <summary>Seconds its fuse has left.</summary>
    public float Left;

    /// <summary>Whether it shows white now, every other five ticks of its fuse, as the source's flashes.</summary>
    public readonly bool Flashing => (int)(Left / PrimedTnt.Tick) / 5 % 2 == 0;

    /// <summary>How large it stands, swelling by up to three tenths over its last ten ticks, as the source's does.</summary>
    public readonly float Size
    {
        get
        {
            var f = Math.Clamp(1 - Left / (10 * PrimedTnt.Tick), 0, 1);
            f *= f;
            return 1 + f * f * 0.3f;
        }
    }

    [OnUpdate]
    public static void Burn(BehaviorContext ctx)
    {
        if (ctx.World.TryGetResource<GameStep>(out var step) && step.Seconds > 0 && ctx.World.TryGetResource<PrimedTnt>(out var tnt))
            Burn(ctx.Ecs, step.World, step.Seconds, tnt.Exploded);
    }

    /// <summary>Moves the TNT on by so many seconds and burns their fuses, setting off those whose fuses have run out.</summary>
    public static void Burn(EcsWorld ecs, VoxelWorld world, float seconds, List<Vector3> exploded)
    {
        var keep = MathF.Pow(PrimedTnt.Keep, seconds / PrimedTnt.Tick);
        var slide = MathF.Pow(PrimedTnt.Slide, seconds / PrimedTnt.Tick);
        var gone = new List<int>();
        foreach (var (id, _, body) in ecs.Query<LitTnt, Body>())
        {
            body.Velocity.Y -= PrimedTnt.Fall / (PrimedTnt.Tick * PrimedTnt.Tick) * seconds;
            body.Velocity *= keep;
            if (body.OnGround) (body.Velocity.X, body.Velocity.Z) = (body.Velocity.X * slide, body.Velocity.Z * slide);
            body.Move(world, body.Velocity * seconds, keepToEdges: false);
            if ((ecs.GetRef<LitTnt>(id).Left -= seconds) > 0) continue;
            exploded.Add(body.Position + new Vector3(0, 1 / 16f, 0));
            gone.Add(id);
        }
        foreach (var id in gone) ecs.Despawn(id);
    }
}

/// <summary>
/// The TNT lit and burning down its fuse as the source's primed TNT does, each popping up from its
/// cell, falling and sliding to a stop, flashing white every five ticks and swelling over its last
/// ten, then exploding at a power of 4.
/// </summary>
/// <remarks>
/// Lit by flint and steel its fuse is four seconds, 80 ticks, and lit by another blast it is 10 to
/// 29 ticks, so a row of TNT goes off one after another. It falls faster by 0.04 of a block a tick
/// each tick and keeps 0.98 of its speed, and on the ground its speed across falls to 0.7 a tick, the
/// source's numbers. Each is a <see cref="LitTnt"/> entity of an ECS world beside its body.
/// </remarks>
public sealed class PrimedTnt(EcsWorld ecs)
{
    /// <summary>The power of TNT's blast and the length of its fuse lit by flint and steel, in seconds.</summary>
    public const float Power = 4, Fuse = 4;

    internal const float Tick = 0.05f, Fall = 0.04f, Keep = 0.98f, Slide = 0.7f;

    private readonly Random _random = new(1);

    /// <summary>The TNT burning, each its fuse and its body.</summary>
    public IEnumerable<(LitTnt Lit, Body Body)> All
    {
        get
        {
            foreach (var (_, lit, body) in ecs.Query<LitTnt, Body>()) yield return (lit, body);
        }
    }

    /// <summary>How many TNT burn.</summary>
    public int Count => ecs.Count<LitTnt>();

    /// <summary>Where TNT went off, its blasts' middles a sixteenth over its feet, for the game to set off and clear.</summary>
    public List<Vector3> Exploded { get; } = [];

    /// <summary>Lights the TNT of a cell, the cell's block taken out by the caller, with a fuse of so many seconds, popping it up as the source's does.</summary>
    public void Light(int x, int y, int z, float fuse = Fuse)
    {
        var turn = _random.NextDouble() * Math.Tau;
        var id = ecs.Spawn();
        ecs.Add(id, new LitTnt { Left = fuse });
        ecs.Add(id, new Body(0.49f, 0.98f)
        {
            Position = new Vector3(x + 0.5f, y, z + 0.5f),
            Velocity = new Vector3(-(float)Math.Sin(turn) * 0.02f, 0.2f, -(float)Math.Cos(turn) * 0.02f) / Tick,
        });
    }

    /// <summary>Lights the TNT of a cell that another blast reached, its fuse 10 to 29 ticks.</summary>
    public void Chain(int x, int y, int z) => Light(x, y, z, (10 + _random.Next(20)) * Tick);

    /// <summary>Burns the TNT by so many seconds as a frame burns them, setting off those whose fuses have run out.</summary>
    public void Update(VoxelWorld world, float seconds) => LitTnt.Burn(ecs, world, seconds, Exploded);

    /// <summary>Puts out every TNT and forgets those set off, as a world closes.</summary>
    public void Clear()
    {
        var all = new List<int>();
        foreach (var (id, _) in ecs.Query<LitTnt>()) all.Add(id);
        foreach (var id in all) ecs.Despawn(id);
        Exploded.Clear();
    }
}
