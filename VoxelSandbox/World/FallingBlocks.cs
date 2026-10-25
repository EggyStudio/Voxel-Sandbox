using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>A falling block that landed where it could not stand, where and which, an event the game drops as an item.</summary>
public readonly record struct FallingBroke(int X, int Y, int Z, BlockId Block);

/// <summary>A block falling, an entity of the engine's ECS beside its <see cref="Body"/>.</summary>
/// <remarks>
/// Its behavior lets it fall in the engine's frame, reading the voxel world and the seconds from the
/// game's <see cref="GameStep"/> on the main thread, which alone may read and change the world, and
/// sends each that lands where it cannot stand as a <see cref="FallingBroke"/>, which the game drops
/// as an item.
/// </remarks>
[Behavior]
public partial struct FallingBlock
{
    public BlockId Block;

    /// <summary>Seconds it has fallen.</summary>
    public float Age;

    private const float Tick = 0.05f, Fall = 0.04f, Keep = 0.98f, MostSeconds = 60;

    /// <summary>Moves the falling blocks on by the step's seconds, sets down or breaks those that land, and sends each that broke.</summary>
    [OnUpdate]
    public static void Drop(BehaviorContext ctx)
    {
        if (!ctx.World.TryGetResource<GameStep>(out var step) || step.Seconds <= 0) return;
        var ecs = ctx.Ecs;
        var broken = new List<(int X, int Y, int Z, BlockId Block)>();
        var keep = MathF.Pow(Keep, step.Seconds / Tick);
        // The lowest moves first, so one falling over another finds it standing once it has landed,
        // as a column of sand stacks again where it lands.
        var order = new List<(int Id, float Y)>();
        foreach (var (id, _, body) in ecs.Query<FallingBlock, Body>()) order.Add((id, body.Position.Y));
        order.Sort((a, b) => a.Y.CompareTo(b.Y));
        var landed = new List<int>();
        foreach (var (id, _) in order)
        {
            var body = ecs.GetRef<Body>(id);
            ref var falling = ref ecs.GetRef<FallingBlock>(id);
            falling.Age += step.Seconds;
            body.Velocity.Y -= Fall / (Tick * Tick) * step.Seconds;
            body.Velocity *= keep;
            body.Move(step.World, body.Velocity * step.Seconds, keepToEdges: false);
            if (body.OnGround)
            {
                int x = (int)MathF.Floor(body.Position.X), y = (int)MathF.Floor(body.Position.Y + 0.01f), z = (int)MathF.Floor(body.Position.Z);
                if (!BlockGroups.GivesWay(step.World.GetBlock(x, y, z)) || !step.World.SetBlock(x, y, z, falling.Block)) broken.Add((x, y, z, falling.Block));
                landed.Add(id);
            }
            else if (falling.Age > MostSeconds || body.Position.Y < ChunkColumn.Bottom - 16) landed.Add(id);
        }
        foreach (var id in landed) ecs.Despawn(id);
        foreach (var (x, y, z, block) in broken) ctx.World.SendEvent(new FallingBroke(x, y, z, block));
    }
}

/// <summary>
/// The blocks falling as the source's falling blocks do, each a block's body falling until it lands,
/// where it stands as its block again, or over a block that does not give way, as a torch or a slab
/// in its cell, breaks into its item.
/// </summary>
/// <remarks>
/// A falling block falls faster by 0.04 of a block a tick each tick and keeps 0.98 of its speed a
/// tick, Minecraft's numbers, so it falls about 40 blocks a second at most. One that falls out of the
/// world or for a minute is let go. Each is a <see cref="FallingBlock"/> entity of an ECS world beside
/// its body.
/// </remarks>
public sealed class FallingBlocks(EcsWorld ecs)
{
    /// <summary>The blocks falling, which block each is and its body.</summary>
    public IEnumerable<(FallingBlock Falling, Body Body)> All
    {
        get
        {
            foreach (var (_, falling, body) in ecs.Query<FallingBlock, Body>()) yield return (falling, body);
        }
    }

    /// <summary>How many blocks fall.</summary>
    public int Count => ecs.Count<FallingBlock>();

    /// <summary>Lets fall the blocks the world loosened, each from its cell.</summary>
    public void Take(VoxelWorld world)
    {
        foreach (var (x, y, z, block) in world.Loosened) Drop(block, x, y, z);
        world.Loosened.Clear();
    }

    /// <summary>Lets a block fall from its cell, as a felled tree's log does.</summary>
    public void Drop(BlockId block, int x, int y, int z)
    {
        var id = ecs.Spawn();
        ecs.Add(id, new FallingBlock { Block = block });
        ecs.Add(id, new Body(0.49f, 0.98f) { Position = new Vector3(x + 0.5f, y, z + 0.5f) });
    }

    /// <summary>Lets go of every falling block, as a world closes.</summary>
    public void Clear()
    {
        var all = new List<int>();
        foreach (var (id, _) in ecs.Query<FallingBlock>()) all.Add(id);
        foreach (var id in all) ecs.Despawn(id);
    }
}
