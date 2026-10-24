using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>
/// A stack of an item lying in the world, an entity of the engine's ECS beside its <see cref="Body"/>,
/// which falls, slides to a stop and is picked up, as Minecraft's are.
/// </summary>
/// <remarks>
/// <para>
/// Its behaviors run in the engine's frame, each a concern of its own in the stage that orders it:
/// the stacks fall and age first, the player picks up those within reach next, and stacks of one
/// item lying together join and the empty and the old vanish last. They read the voxel world, the
/// player and the inventory from the game's <see cref="GameStep"/>, and run on the main thread,
/// which alone may read the world. Each also takes its world and its inputs as arguments, so a
/// test steps the stacks of an ECS world of its own as the frame steps the game's.
/// </para>
/// <para>
/// A stack can be picked up half a second after it is dropped, two seconds after the player throws
/// it, so a stack thrown is not taken back at once, and two stacks of one item within half a block
/// of each other join. A stack in a column not loaded waits there, its five minutes held, until the
/// column loads again, and no stack is saved with the world.
/// </para>
/// </remarks>
[Behavior]
public partial struct ItemDrop
{
    /// <summary>The item's id, which the struct keeps in place of the item, a class every copy would share.</summary>
    public int ItemId;

    public readonly Item Item => Items.All[ItemId];

    public int Count;

    /// <summary>How worn a tool dropped is, which it keeps when picked up again.</summary>
    public int Wear;

    /// <summary>Seconds from its drop before it can be picked up.</summary>
    public float Delay;

    /// <summary>Seconds since it was dropped.</summary>
    public float Age;

    private const float Lifetime = 300, Gravity = 16, Join = 0.5f;

    [OnPreUpdate]
    public static void Fall(BehaviorContext ctx)
    {
        if (ctx.World.TryGetResource<GameStep>(out var step)) Fall(ctx.Ecs, step.World, step.Seconds);
    }

    [OnUpdate]
    public static void PickUp(BehaviorContext ctx)
    {
        if (ctx.World.TryGetResource<GameStep>(out var step) && step.PickUp && step.Seconds > 0) PickUp(ctx.Ecs, step.Player, step.Inventory);
    }

    [OnPostUpdate]
    public static void Gather(BehaviorContext ctx) => Gather(ctx.Ecs);

    /// <summary>Moves and ages by so many seconds every stack whose column is loaded.</summary>
    public static void Fall(EcsWorld ecs, VoxelWorld world, float seconds)
    {
        if (seconds <= 0) return;
        foreach (var (id, _, body) in ecs.Query<ItemDrop, Body>())
        {
            if (!world.HasColumn(VoxelWorld.ColumnOf((int)MathF.Floor(body.Position.X)), VoxelWorld.ColumnOf((int)MathF.Floor(body.Position.Z)))) continue;
            ecs.GetRef<ItemDrop>(id).Age += seconds;
            Move(world, body, seconds);
        }
    }

    /// <summary>
    /// Puts into the inventory the stacks past their delay that the player's box reaches within a
    /// block across and half a block up and down.
    /// </summary>
    /// <returns>How many items the player picked up.</returns>
    public static int PickUp(EcsWorld ecs, Body player, Inventory inventory)
    {
        var picked = 0;
        Vector3 reachMin = player.Min - new Vector3(1, 0.5f, 1), reachMax = player.Max + new Vector3(1, 0.5f, 1);
        foreach (var (id, drop, body) in ecs.Query<ItemDrop, Body>())
        {
            if (drop.Age < drop.Delay || drop.Count == 0) continue;
            Vector3 min = body.Min, max = body.Max;
            if (min.X > reachMax.X || max.X < reachMin.X || min.Y > reachMax.Y || max.Y < reachMin.Y || min.Z > reachMax.Z || max.Z < reachMin.Z) continue;
            var left = inventory.Add(drop.Item, drop.Count, drop.Wear);
            picked += drop.Count - left;
            ecs.GetRef<ItemDrop>(id).Count = left;
        }
        return picked;
    }

    /// <summary>
    /// Joins stacks of one item lying within half a block of each other, each taking as much of the
    /// others as it holds, and despawns the empty stacks and those five minutes old.
    /// </summary>
    public static void Gather(EcsWorld ecs)
    {
        var stacks = new List<(int Id, Vector3 At)>();
        foreach (var (id, _, body) in ecs.Query<ItemDrop, Body>()) stacks.Add((id, body.Position));
        for (int i = 0; i < stacks.Count; i++)
        {
            ref var a = ref ecs.GetRef<ItemDrop>(stacks[i].Id);
            if (a.Count == 0 || a.Count >= a.Item.MostInStack) continue;
            for (int j = i + 1; j < stacks.Count; j++)
            {
                ref var b = ref ecs.GetRef<ItemDrop>(stacks[j].Id);
                if (b.Count == 0 || b.ItemId != a.ItemId || Vector3.DistanceSquared(stacks[i].At, stacks[j].At) > Join * Join) continue;
                var moved = Math.Min(b.Count, a.Item.MostInStack - a.Count);
                (a.Count, b.Count) = (a.Count + moved, b.Count - moved);
                if (a.Count >= a.Item.MostInStack) break;
            }
        }
        foreach (var (id, _) in stacks)
            if (ecs.GetRef<ItemDrop>(id) is { Count: 0 } or { Age: > Lifetime }) ecs.Despawn(id);
    }

    private static void Move(VoxelWorld world, Body body, float seconds)
    {
        var middle = body.Position + new Vector3(0, 0.125f, 0);
        if (Blocks.IsWater(world.GetBlock((int)MathF.Floor(middle.X), (int)MathF.Floor(middle.Y), (int)MathF.Floor(middle.Z))))
        {
            // A stack floats up through water, slowed by it.
            body.Velocity.Y = MathF.Min(body.Velocity.Y + 10 * seconds, 1);
            body.Velocity.X *= MathF.Exp(-3 * seconds);
            body.Velocity.Z *= MathF.Exp(-3 * seconds);
        }
        else
        {
            body.Velocity.Y = MathF.Max(body.Velocity.Y - Gravity * seconds, -40);
            // It slides to a stop on the ground and drifts a little in the air.
            var drag = MathF.Exp(-(body.OnGround ? 10 : 0.5f) * seconds);
            body.Velocity.X *= drag;
            body.Velocity.Z *= drag;
        }
        body.Move(world, body.Velocity * seconds, keepToEdges: false);
    }
}

/// <summary>
/// The stacks of items lying in the loaded columns, dropped by a block broken in survival, by a mob
/// as it dies and by the player, picked up when the player comes near, and gone after five minutes,
/// each an entity of an ECS world with an <see cref="ItemDrop"/> and a <see cref="Body"/>.
/// </summary>
public sealed class ItemDrops(EcsWorld ecs)
{
    /// <summary>Seconds before a stack a block or a mob dropped can be picked up, and one the player threw.</summary>
    public const float Delay = 0.5f, ThrownDelay = 2;

    private readonly Random _random = new();

    /// <summary>Every stack, its item, count and age, and its body.</summary>
    public IEnumerable<(ItemDrop Drop, Body Body)> All
    {
        get
        {
            foreach (var (_, drop, body) in ecs.Query<ItemDrop, Body>()) yield return (drop, body);
        }
    }

    /// <summary>How many stacks lie in the world.</summary>
    public int Count => ecs.Count<ItemDrop>();

    /// <summary>Drops a stack at a place with a velocity, which can be picked up after a delay.</summary>
    public Entity Spawn(Item item, int count, Vector3 at, Vector3 velocity, float delay = Delay, int wear = 0)
    {
        var id = ecs.Spawn();
        ecs.Add(id, new ItemDrop { ItemId = item.Id, Count = count, Wear = wear, Delay = delay });
        ecs.Add(id, new Body(0.125f, 0.25f) { Position = at, Velocity = velocity });
        return ecs.Handle(id);
    }

    /// <summary>Drops a stack from a block's middle, hopping a little in a direction of its own, as a broken block's does.</summary>
    public void Pop(Item item, int count, int x, int y, int z)
    {
        var angle = (float)_random.NextDouble() * MathF.Tau;
        var velocity = new Vector3(MathF.Cos(angle) * 1.2f, 3, MathF.Sin(angle) * 1.2f);
        Spawn(item, count, new Vector3(x + 0.5f, y + 0.375f, z + 0.5f), velocity);
    }

    /// <summary>
    /// Steps the stacks by so many seconds as a frame steps them, falling, picked up unless
    /// <paramref name="pickUp"/> is clear, as while the player lies dead, joined and gone when old.
    /// </summary>
    /// <returns>How many items the player picked up.</returns>
    public int Update(VoxelWorld world, Body player, Inventory inventory, float seconds, bool pickUp = true)
    {
        ItemDrop.Fall(ecs, world, seconds);
        var picked = pickUp ? ItemDrop.PickUp(ecs, player, inventory) : 0;
        ItemDrop.Gather(ecs);
        return picked;
    }

    /// <summary>Despawns every stack, as a world closes.</summary>
    public void Clear()
    {
        var all = new List<int>();
        foreach (var (id, _) in ecs.Query<ItemDrop>()) all.Add(id);
        foreach (var id in all) ecs.Despawn(id);
    }
}
