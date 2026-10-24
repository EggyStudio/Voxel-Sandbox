using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>An arrow's strike on the player, its damage and where it came from, an event the game deals.</summary>
public readonly record struct ArrowStrike(int Damage, Vector3 From);

/// <summary>An arrow in flight or stuck in a block, an entity of the engine's ECS.</summary>
/// <remarks>
/// Its behavior flies it in the engine's frame, reading the voxel world, the player and the
/// seconds from the game's <see cref="GameStep"/> on the main thread, which alone may read the
/// world, and sends each strike on the player as an <see cref="ArrowStrike"/>, which the game deals
/// as its next frame answers the mobs. It also takes its world and its inputs as arguments, so a
/// test flies the arrows of an ECS world of its own as the frame flies the game's.
/// </remarks>
[Behavior]
public partial struct Arrow
{
    public Vector3 Position;

    public Vector3 Velocity;

    /// <summary>Whether it has struck a block and stays where it struck.</summary>
    public bool Stuck;

    /// <summary>Seconds since it was loosed.</summary>
    public float Age;

    private const float Life = 60, Step = 0.1f;

    [OnUpdate]
    public static void Fly(BehaviorContext ctx)
    {
        if (!ctx.World.TryGetResource<GameStep>(out var step) || step.Seconds <= 0) return;
        foreach (var (damage, from) in Fly(ctx.Ecs, step.World, step.Player, step.Seconds, step.Random)) ctx.World.SendEvent(new ArrowStrike(damage, from));
    }

    /// <summary>Moves every arrow in flight, a step of a tenth of a block at a time, and gives the damage of each that struck the player and where it came from.</summary>
    public static List<(int Damage, Vector3 From)> Fly(EcsWorld ecs, VoxelWorld world, Body player, float seconds, Random random)
    {
        var hits = new List<(int, Vector3)>();
        var gone = new List<int>();
        foreach (var (id, _) in ecs.Query<Arrow>())
        {
            ref var arrow = ref ecs.GetRef<Arrow>(id);
            arrow.Age += seconds;
            if (arrow.Age > Life) gone.Add(id);
            else if (!arrow.Stuck && Move(ref arrow, world, player, seconds, random) is { } hit)
            {
                hits.Add(hit);
                gone.Add(id);
            }
        }
        foreach (var id in gone) ecs.Despawn(id);
        return hits;
    }

    // Falls and slows the arrow along its flight, sticking it in the first block it meets, and gives
    // its strike where it meets the player's box first.
    private static (int, Vector3)? Move(ref Arrow arrow, VoxelWorld world, Body player, float seconds, Random random)
    {
        arrow.Velocity.Y -= Arrows.Gravity * seconds;
        arrow.Velocity *= MathF.Exp(-0.2f * seconds);
        var move = arrow.Velocity * seconds;
        var steps = Math.Max(1, (int)MathF.Ceiling(move.Length() / Step));
        for (int i = 0; i < steps; i++)
        {
            arrow.Position += move / steps;
            if (Inside(player.Min - new Vector3(0.05f), player.Max + new Vector3(0.05f), arrow.Position))
            {
                // As Minecraft's, two points of damage for each block a tick it flies, rounded up, with a chance of one more.
                var damage = (int)MathF.Ceiling(arrow.Velocity.Length() / 20 * 2) + (random.Next(4) == 0 ? 1 : 0);
                return (damage, arrow.Position - arrow.Velocity * 0.05f);
            }
            int x = (int)MathF.Floor(arrow.Position.X), y = (int)MathF.Floor(arrow.Position.Y), z = (int)MathF.Floor(arrow.Position.Z);
            foreach (var box in world.BoxesAt(x, y, z))
                if (Inside(new Vector3(x, y, z) + box.Min, new Vector3(x, y, z) + box.Max, arrow.Position))
                {
                    arrow.Stuck = true;
                    return null;
                }
        }
        return null;
    }

    private static bool Inside(Vector3 min, Vector3 max, Vector3 p) =>
        p.X >= min.X && p.X <= max.X && p.Y >= min.Y && p.Y <= max.Y && p.Z >= min.Z && p.Z <= max.Z;
}

/// <summary>
/// The arrows skeletons loose, falling under Minecraft's arrow gravity and slowed a little by the
/// air, which strike the player's box for damage by their speed or stick in the first block they
/// meet, and are gone a minute after they were loosed, each an <see cref="Arrow"/> entity of an
/// ECS world.
/// </summary>
public sealed class Arrows(EcsWorld ecs)
{
    /// <summary>The pull on an arrow in blocks a second each second, Minecraft's 0.05 a tick.</summary>
    public const float Gravity = 20;

    private readonly Random _random = new();

    /// <summary>Every arrow, where it is, its flight, whether it is stuck and its age.</summary>
    public IEnumerable<Arrow> All
    {
        get
        {
            foreach (var (_, arrow) in ecs.Query<Arrow>()) yield return arrow;
        }
    }

    /// <summary>How many arrows fly or stand stuck.</summary>
    public int Count => ecs.Count<Arrow>();

    public void Shoot(Vector3 from, Vector3 velocity) => ecs.Add(ecs.Spawn(), new Arrow { Position = from, Velocity = velocity });

    /// <summary>Flies the arrows by so many seconds as a frame flies them, and gives the damage of each that struck the player and where it came from.</summary>
    public List<(int Damage, Vector3 From)> Update(VoxelWorld world, Body player, float seconds) => Arrow.Fly(ecs, world, player, seconds, _random);

    /// <summary>Despawns every arrow, as a world closes.</summary>
    public void Clear()
    {
        var all = new List<int>();
        foreach (var (id, _) in ecs.Query<Arrow>()) all.Add(id);
        foreach (var id in all) ecs.Despawn(id);
    }
}
