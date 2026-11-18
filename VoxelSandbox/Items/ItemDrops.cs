using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>
/// A stack of an item lying in the world, an entity of the engine's ECS beside its <see
/// cref="Body"/>, which falls, slides to a stop and is picked up.
/// </summary>
/// <remarks>
/// <para>
/// Its behaviors run in the engine's frame, each a concern of its own in the stage that orders it:
/// the stacks fall and age first, the player picks up those within reach next, and stacks of one
/// item lying together join and the empty and the old vanish last. They read the voxel world and
/// the player from the game's <see cref="GameStep"/> and the inventory as a resource, and run on
/// the main thread, which alone may read the world.
/// </para>
/// <para>
/// A stack can be picked up half a second after it is dropped, two seconds after the player throws
/// it, so a stack thrown is not taken back at once, and two stacks of one item within half a block
/// of each other join. A stack whose column unloads is kept out of the ECS by that column, its
/// place in the land's own doubles, and set down where it lay as the column loads again, its five
/// minutes held, so the things a player spilled in a death far from the spawn lie there after the
/// respawn, as a stack left a teleport's length away does. One dropped into a column not loaded
/// waits there likewise. A saved world keeps each stack with its column's region, as it keeps the
/// mobs, with the seconds it has left, so a stack lies across a quit and still goes in its time.
/// </para>
/// </remarks>
[Behavior]
public partial struct ItemDrop
{
    /// <summary>
    /// The item's id, which the struct keeps in place of the item, a class every copy would share.
    /// </summary>
    public int ItemId;

    public readonly Item Item => Items.All[ItemId];

    public int Count;

    /// <summary>How worn a tool dropped is, which it keeps when picked up again.</summary>
    public int Wear;

    /// <summary>Seconds from its drop before it can be picked up.</summary>
    public float Delay;

    /// <summary>Seconds since it was dropped.</summary>
    public float Age;

    /// <summary>Seconds a stack lies before it goes.</summary>
    public const float Lifetime = 300;

    private const float Gravity = 16, Join = 0.5f;

    /// <summary>
    /// Moves and ages by the step's seconds every stack whose column is loaded, burning away those
    /// in lava.
    /// </summary>
    [OnPreUpdate]
    public static void Fall(BehaviorContext ctx)
    {
        if (!ctx.World.TryGetResource<GameStep>(out var step) || step.Seconds <= 0) return;
        foreach (var (id, _, body) in ctx.Ecs.Query<ItemDrop, Body>())
        {
            var (cx, cz) = LandOrigin.ColumnOf(body.Position);
            if (!step.World.HasColumn(cx, cz)) continue;
            ctx.Ecs.GetRef<ItemDrop>(id).Age += step.Seconds;
            Move(step.World, body, step.Seconds);
            // A stack in lava or in a fire burns away.
            if (Vitals.Touches(step.World, body, BlockId.Lava, -0.001f) || Vitals.Touches(step.World, body, BlockId.Fire, -0.001f)) ctx.Ecs.GetRef<ItemDrop>(id).Count = 0;
        }
    }

    /// <summary>
    /// Puts into the inventory the stacks past their delay that the player's box reaches within a
    /// block across and half a block up and down, while the player picks up.
    /// </summary>
    [OnUpdate]
    public static void PickUp(BehaviorContext ctx)
    {
        if (!ctx.World.TryGetResource<GameStep>(out var step) || !step.PickUp || step.Seconds <= 0 || !ctx.World.TryGetResource<Inventory>(out var inventory)) return;
        var player = step.Player;
        Vector3 reachMin = player.Min - new Vector3(1, 0.5f, 1), reachMax = player.Max + new Vector3(1, 0.5f, 1);
        foreach (var (id, drop, body) in ctx.Ecs.Query<ItemDrop, Body>())
        {
            if (drop.Age < drop.Delay || drop.Count == 0) continue;
            Vector3 min = body.Min, max = body.Max;
            if (min.X > reachMax.X || max.X < reachMin.X || min.Y > reachMax.Y || max.Y < reachMin.Y || min.Z > reachMax.Z || max.Z < reachMin.Z) continue;
            ctx.Ecs.GetRef<ItemDrop>(id).Count = inventory.Add(drop.Item, drop.Count, drop.Wear);
        }
    }

    /// <summary>
    /// Joins stacks of one item lying within half a block of each other, each taking as much of the
    /// others as it holds, and despawns the empty stacks and those five minutes old, while the game
    /// takes time.
    /// </summary>
    [OnPostUpdate]
    public static void Gather(BehaviorContext ctx)
    {
        if (ctx.World.TryGetResource<GameStep>(out var step) && step.Seconds <= 0) return;
        var ecs = ctx.Ecs;
        // Each stack is weighed against those of its item in the cells of the joining's reach about
        // its own alone, the only ones near enough to join it, and takes from them in the order the
        // stacks were gathered, so of stacks that join the one gathered first holds what they join.
        Lying.Clear();
        Firsts.Clear();
        foreach (var (id, drop, body) in ecs.Query<ItemDrop, Body>())
        {
            var cell = CellOf(body.Position, drop.ItemId);
            Lying.Add((id, body.Position, Firsts.GetValueOrDefault(cell, -1)));
            Firsts[cell] = Lying.Count - 1;
        }
        for (int i = 0; i < Lying.Count; i++)
        {
            ref var a = ref ecs.GetRef<ItemDrop>(Lying[i].Id);
            if (a.Count == 0 || a.Count >= a.Item.MostInStack) continue;
            Near.Clear();
            var (x, y, z, item) = CellOf(Lying[i].At, a.ItemId);
            for (int dz = -1; dz <= 1; dz++)
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                        for (int j = Firsts.GetValueOrDefault((x + dx, y + dy, z + dz, item), -1); j >= 0; j = Lying[j].Next)
                            if (j > i && Vector3.DistanceSquared(Lying[i].At, Lying[j].At) <= Join * Join) Near.Add(j);
            Near.Sort();
            foreach (var j in Near)
            {
                ref var b = ref ecs.GetRef<ItemDrop>(Lying[j].Id);
                if (b.Count == 0) continue;
                var moved = Math.Min(b.Count, a.Item.MostInStack - a.Count);
                (a.Count, b.Count) = (a.Count + moved, b.Count - moved);
                if (a.Count >= a.Item.MostInStack) break;
            }
        }
        foreach (var (id, _, _) in Lying)
            if (ecs.GetRef<ItemDrop>(id) is { Count: 0 } or { Age: > Lifetime }) ecs.Despawn(id);
    }

    // The stacks of the frame, each with the one gathered before it of its item in its cell, the
    // last of each cell, and the stacks near one, kept from frame to frame so gathering allocates
    // nothing.
    private static readonly List<(int Id, Vector3 At, int Next)> Lying = [];
    private static readonly Dictionary<(int X, int Y, int Z, int Item), int> Firsts = [];
    private static readonly List<int> Near = [];

    // The cell of the joining's reach a place lies in, with the item of the stack there.
    private static (int X, int Y, int Z, int Item) CellOf(Vector3 at, int item) =>
        ((int)MathF.Floor(at.X / Join), (int)MathF.Floor(at.Y / Join), (int)MathF.Floor(at.Z / Join), item);

    private static void Move(VoxelWorld world, Body body, float seconds)
    {
        var middle = body.Position + new Vector3(0, 0.125f, 0);
        var (mx, my, mz) = LandOrigin.BlockOf(middle);
        if (Blocks.IsWater(world.GetBlock(mx, my, mz)))
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
/// each an entity of an ECS world with an <see cref="ItemDrop"/> and a <see cref="Body"/>, and
/// those the unloaded columns keep.
/// </summary>
public sealed class ItemDrops(EcsWorld ecs)
{
    /// <summary>
    /// Seconds before a stack a block or a mob dropped can be picked up, and one the player threw.
    /// </summary>
    public const float Delay = 0.5f, ThrownDelay = 2;

    private readonly Random _random = new();
    // The stacks of the columns that unloaded, by column, each with its place in the land's own
    // doubles, so one left a jump's length away comes back where it lay.
    private readonly Dictionary<(int X, int Z), List<(ItemDrop Drop, LandPlace At, Vector3 Velocity)>> _kept = [];

    /// <summary>Every stack, its item, count and age, and its body.</summary>
    public IEnumerable<(ItemDrop Drop, Body Body)> All
    {
        get
        {
            foreach (var (_, drop, body) in ecs.Query<ItemDrop, Body>()) yield return (drop, body);
        }
    }

    /// <summary>How many stacks lie in the loaded columns.</summary>
    public int Count => ecs.Count<ItemDrop>();

    /// <summary>How many stacks the unloaded columns keep where the world keeps no save.</summary>
    public int Kept => _kept.Values.Sum(stacks => stacks.Count);

    /// <summary>
    /// The save of the world the stacks lie in, which keeps those of the columns that unload, or
    /// null for a world that keeps none.
    /// </summary>
    public WorldSave? Save { get; set; }

    /// <summary>
    /// Drops a stack at a place with a velocity, which can be picked up after a delay.
    /// </summary>
    public Entity Spawn(Item item, int count, Vector3 at, Vector3 velocity, float delay = Delay, int wear = 0)
    {
        var id = ecs.Spawn();
        ecs.Add(id, new ItemDrop { ItemId = item.Id, Count = count, Wear = wear, Delay = delay });
        ecs.Add(id, new Body(0.125f, 0.25f) { Position = at, Velocity = velocity });
        return ecs.Handle(id);
    }

    /// <summary>
    /// Drops a stack from a block's middle, hopping a little in a direction of its own, as a broken
    /// block's does.
    /// </summary>
    public void Pop(Item item, int count, int x, int y, int z)
    {
        var angle = (float)_random.NextDouble() * MathF.Tau;
        var velocity = new Vector3(MathF.Cos(angle) * 1.2f, 3, MathF.Sin(angle) * 1.2f);
        Spawn(item, count, LandOrigin.PlaceOf(x, y, z) + new Vector3(0.5f, 0.375f, 0.5f), velocity);
    }

    /// <summary>
    /// Keeps the stacks lying in a column as it unloads, each by its place in the land, out of the
    /// ECS until the column loads again.
    /// </summary>
    public void Leaving(int x, int z)
    {
        var gone = new List<int>();
        foreach (var (id, drop, body) in ecs.Query<ItemDrop, Body>())
        {
            if (LandOrigin.ColumnOf(body.Position) != (x, z)) continue;
            if (drop.Count > 0)
            {
                if (!_kept.TryGetValue((x, z), out var kept)) _kept[(x, z)] = kept = [];
                kept.Add((drop, LandOrigin.LandOf(body.Position), body.Velocity));
            }
            gone.Add(id);
        }
        foreach (var id in gone) ecs.Despawn(id);
        // A saved world's save keeps them with those it keeps of the column, as it keeps the stacks
        // of the loaded columns at a save.
        if (Save is { } save && _kept.Remove((x, z), out var left))
            save.KeepDrops(x, z, [.. save.TakeDrops(x, z) ?? [], .. left.Select(s => Record(s.Drop, s.At))]);
    }

    /// <summary>
    /// Keeps the stacks lying over columns not loaded, thrown or dropped there, as the column they
    /// wait for keeps its own as it unloads, so a save before that column arrives keeps them.
    /// </summary>
    public void KeepStrays(VoxelWorld world)
    {
        var columns = new HashSet<(int X, int Z)>();
        foreach (var (_, _, body) in ecs.Query<ItemDrop, Body>())
            if (LandOrigin.ColumnOf(body.Position) is var (x, z) && !world.HasColumn(x, z)) columns.Add((x, z));
        foreach (var (x, z) in columns) Leaving(x, z);
    }

    /// <summary>
    /// The stacks of each loaded column as a save keeps them, a column with none among them, so a
    /// save clears what it held of one.
    /// </summary>
    public IEnumerable<((int X, int Z) Column, IReadOnlyList<WorldSave.DropRecord> Drops)> Columns(VoxelWorld world)
    {
        var lying = new Dictionary<(int X, int Z), List<WorldSave.DropRecord>>();
        foreach (var column in world.Columns) lying[(column.X, column.Z)] = [];
        foreach (var (_, drop, body) in ecs.Query<ItemDrop, Body>())
            if (drop.Count > 0 && lying.TryGetValue(LandOrigin.ColumnOf(body.Position), out var stacks)) stacks.Add(Record(drop, LandOrigin.LandOf(body.Position)));
        foreach (var (column, stacks) in lying) yield return (column, stacks);
    }

    // A stack as a save keeps it, its time left what it has not lain of its lifetime.
    private static WorldSave.DropRecord Record(ItemDrop drop, LandPlace at) =>
        new(Inventory.Encode(new ItemStack(drop.Item, drop.Count, drop.Wear)), at, ItemDrop.Lifetime - drop.Age);

    /// <summary>
    /// Sets down the stacks a column kept as it loads, each where it lay, as old and as it moved.
    /// </summary>
    public void Joined(int x, int z)
    {
        if (_kept.Remove((x, z), out var kept))
            foreach (var (drop, at, velocity) in kept) Lay(drop, at, velocity);
        // A saved stack is laid as old as it was, one whose time ran out or whose item this build
        // does not know left out.
        foreach (var record in Save?.TakeDrops(x, z) ?? [])
            if (record.Left > 0 && Inventory.Decode(record.Stack) is { } stack)
                Lay(new ItemDrop { ItemId = stack.Item.Id, Count = stack.Count, Wear = stack.Wear, Age = ItemDrop.Lifetime - record.Left }, record.At, Vector3.Zero);
    }

    private void Lay(ItemDrop drop, LandPlace at, Vector3 velocity)
    {
        var id = ecs.Spawn();
        ecs.Add(id, drop);
        ecs.Add(id, new Body(0.125f, 0.25f) { Position = LandOrigin.PlaceOf(at), Velocity = velocity });
    }

    /// <summary>
    /// Despawns every stack and lets go of those the unloaded columns keep, as a world closes.
    /// </summary>
    public void Clear()
    {
        var all = new List<int>();
        foreach (var (id, _) in ecs.Query<ItemDrop>()) all.Add(id);
        foreach (var id in all) ecs.Despawn(id);
        _kept.Clear();
    }
}
