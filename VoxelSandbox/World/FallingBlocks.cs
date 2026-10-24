using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>A block falling, an entity of the engine's ECS beside its <see cref="Body"/>.</summary>
/// <remarks>
/// Its behavior lets it fall in the engine's frame, reading the voxel world and the seconds from the
/// game's <see cref="GameStep"/> on the main thread, which alone may read and change the world, and
/// leaves each that lands where it cannot stand in <see cref="FallingBlocks.Broken"/>, which the game
/// drops as items. It also takes its world and its inputs as arguments, so a test lets fall the
/// blocks of an ECS world of its own.
/// </remarks>
[Behavior]
public partial struct FallingBlock
{
    public BlockId Block;

    /// <summary>Seconds it has fallen.</summary>
    public float Age;

    private const float Tick = 0.05f, Fall = 0.04f, Keep = 0.98f, MostSeconds = 60;

    [OnUpdate]
    public static void Drop(BehaviorContext ctx)
    {
        if (ctx.World.TryGetResource<GameStep>(out var step) && step.Seconds > 0 && ctx.World.TryGetResource<FallingBlocks>(out var falling))
            Drop(ctx.Ecs, step.World, step.Seconds, falling.Broken);
    }

    /// <summary>Moves the falling blocks on by so many seconds, and sets down or breaks those that land.</summary>
    public static void Drop(EcsWorld ecs, VoxelWorld world, float seconds, List<(int X, int Y, int Z, BlockId Block)> broken)
    {
        var keep = MathF.Pow(Keep, seconds / Tick);
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
            falling.Age += seconds;
            body.Velocity.Y -= Fall / (Tick * Tick) * seconds;
            body.Velocity *= keep;
            body.Move(world, body.Velocity * seconds, keepToEdges: false);
            if (body.OnGround)
            {
                int x = (int)MathF.Floor(body.Position.X), y = (int)MathF.Floor(body.Position.Y + 0.01f), z = (int)MathF.Floor(body.Position.Z);
                if (!BlockGroups.GivesWay(world.GetBlock(x, y, z)) || !world.SetBlock(x, y, z, falling.Block)) broken.Add((x, y, z, falling.Block));
                landed.Add(id);
            }
            else if (falling.Age > MostSeconds || body.Position.Y < ChunkColumn.Bottom - 16) landed.Add(id);
        }
        foreach (var id in landed) ecs.Despawn(id);
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

    /// <summary>The blocks that landed where they could not stand, where and which, for the game to drop as items and clear.</summary>
    public List<(int X, int Y, int Z, BlockId Block)> Broken { get; } = [];

    /// <summary>Lets fall the blocks the world loosened, each from its cell.</summary>
    public void Take(VoxelWorld world)
    {
        foreach (var (x, y, z, block) in world.Loosened) Drop(block, x, y, z);
        world.Loosened.Clear();
    }

    /// <summary>Moves the falling blocks on by so many seconds as a frame moves them, and sets down or breaks those that land.</summary>
    public void Update(VoxelWorld world, float seconds) => FallingBlock.Drop(ecs, world, seconds, Broken);

    /// <summary>Lets a block fall from its cell, as a felled tree's log does.</summary>
    public void Drop(BlockId block, int x, int y, int z)
    {
        var id = ecs.Spawn();
        ecs.Add(id, new FallingBlock { Block = block });
        ecs.Add(id, new Body(0.49f, 0.98f) { Position = new Vector3(x + 0.5f, y, z + 0.5f) });
    }

    /// <summary>Lets go of every falling block and forgets those broken, as a world closes.</summary>
    public void Clear()
    {
        var all = new List<int>();
        foreach (var (id, _) in ecs.Query<FallingBlock>()) all.Add(id);
        foreach (var id in all) ecs.Despawn(id);
        Broken.Clear();
    }
}
