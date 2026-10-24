using Engine;

namespace VoxelSandbox;

/// <summary>A TNT a fire burned, where it stood, an event the game lights.</summary>
public readonly record struct TntCaught(int X, int Y, int Z);

/// <summary>A fire burning in a cell, an entity of the engine's ECS, its age and the seconds to its next tick.</summary>
/// <remarks>
/// Its behavior burns the fires in the engine's frame, reading the voxel world, the seconds and the
/// chances from the game's <see cref="GameStep"/> on the main thread, which alone may read and change
/// the world, and sends each TNT a fire burned as a <see cref="TntCaught"/>, which the game lights.
/// It also takes its world and its inputs as arguments, so a test burns the fires of an ECS world
/// of its own. <see cref="Fires"/> has the source's numbers.
/// </remarks>
[Behavior]
public partial struct Fire
{
    public int X, Y, Z;

    /// <summary>Its age, from 0 to 15.</summary>
    public int Age;

    /// <summary>Seconds to its next tick.</summary>
    public float Wait;

    [OnUpdate]
    public static void Spread(BehaviorContext ctx)
    {
        if (!ctx.World.TryGetResource<GameStep>(out var step) || step.Seconds <= 0) return;
        var primed = new List<(int, int, int)>();
        Spread(ctx.Ecs, step.World, step.Seconds, step.Random, primed);
        foreach (var (x, y, z) in primed) ctx.World.SendEvent(new TntCaught(x, y, z));
    }

    /// <summary>Goes on by so many seconds, each fire whose tick comes burning as the source's does, and gives where it burned TNT.</summary>
    public static void Spread(EcsWorld ecs, VoxelWorld world, float seconds, Random random, List<(int X, int Y, int Z)> primed)
    {
        var due = new List<int>();
        foreach (var (id, _) in ecs.Query<Fire>())
        {
            ref var fire = ref ecs.GetRef<Fire>(id);
            if (fire.Wait <= seconds) due.Add(id);
            else fire.Wait -= seconds;
        }
        foreach (var id in due) Fires.Burn(ecs, world, id, random, primed);
    }
}

/// <summary>
/// The fires burning, as the source's fire burns, each ticking now and then, growing older, burning
/// the blocks beside it away or into more fire, catching the air beside what burns near it, and
/// going out where nothing feeds it, so a house of planks burns down and a fire on stone dies.
/// </summary>
/// <remarks>
/// <para>
/// The numbers are Minecraft's. A fire ticks every 30 to 39 ticks, at a twentieth of a second each,
/// and at each its age, from 0 to 15, grows by one a third of the time. One with nothing that burns
/// beside it goes out where no whole block holds it or once its age passes 3, and one of age 15 over
/// a block that does not burn goes out a quarter of the time. Each block beside it burns at a
/// chance of its burning odds in 300, or 250 for the one over and the one under it, becoming fire
/// with a chance of 5 in its age and 10 and otherwise going, and TNT it burns is lit. Each cell of
/// air from one under it to four over it and one to each side catches at a chance of its odds,
/// the best of its neighbors' catching odds and 40 and 14 for the difficulty of normal over its age
/// and 30, in 100, or 100 more for each cell past the first over it.
/// </para>
/// <para>
/// A fire over magma burns on, as the source's does over its endless burning blocks. Fire placed by
/// the world, as a column holding some arrives, starts at age 0. Each fire is a <see cref="Fire"/>
/// entity of an ECS world.
/// </para>
/// </remarks>
public sealed class Fires(EcsWorld ecs, Random? random = null)
{
    private const float Tick = 0.05f;
    private const int Difficulty = 2;

    // How readily each block catches fire into the air beside it and how readily it burns away, the
    // source's odds, both none for a block that does not burn.
    private static readonly (int Catches, int Burns)[] _odds = Odds();

    private readonly Random _random = random ?? new Random();

    /// <summary>Where each fire burns and its age.</summary>
    public IEnumerable<((int X, int Y, int Z) At, int Age)> All
    {
        get
        {
            foreach (var (_, fire) in ecs.Query<Fire>()) yield return ((fire.X, fire.Y, fire.Z), fire.Age);
        }
    }

    /// <summary>Whether a block burns, as the source's planks, logs, leaves, wool, hay, TNT and plants of the ground do.</summary>
    public static bool Burns(BlockId block) => _odds[(int)block].Burns > 0;

    /// <summary>Whether a fire can stand in a cell, over a whole block or beside one that burns.</summary>
    public static bool CanStand(VoxelWorld world, int x, int y, int z) => Blocks.IsSturdy(world.GetBlock(x, y - 1, z)) || BesideFuel(world, x, y, z);

    /// <summary>Lights a fire in a cell of air where it can stand, as flint and steel does.</summary>
    /// <returns>Whether it was lit.</returns>
    public bool Light(VoxelWorld world, int x, int y, int z)
    {
        if (world.GetBlock(x, y, z) != BlockId.Air || !CanStand(world, x, y, z)) return false;
        return Set(ecs, world, x, y, z, 0, _random);
    }

    /// <summary>Takes the fires the world set, by an edit or as a column holding some arrived, to keep them burning.</summary>
    /// <remarks>
    /// The world names every fire set, those the fires spread themselves among them, so a cell that
    /// already has its fire is passed over, found among the fires as they stand, since the ECS has no
    /// lookup of an entity by its cell.
    /// </remarks>
    public void Take(VoxelWorld world)
    {
        if (world.Kindled.Count == 0) return;
        var burning = new HashSet<(int, int, int)>();
        foreach (var (_, fire) in ecs.Query<Fire>()) burning.Add((fire.X, fire.Y, fire.Z));
        foreach (var at in world.Kindled)
        {
            // Each cell named draws its delay, its new fire's or one let go, so the chances drawn after
            // it are the same whichever of the cells burned already.
            var wait = Delay(_random);
            if (burning.Add(at)) ecs.Add(ecs.Spawn(), new Fire { X = at.X, Y = at.Y, Z = at.Z, Wait = wait });
        }
        world.Kindled.Clear();
    }

    /// <summary>Goes on by so many seconds as a frame goes on, each fire whose tick comes burning as the source's does, and gives where it burned TNT.</summary>
    public List<(int X, int Y, int Z)> Update(VoxelWorld world, float seconds)
    {
        var primed = new List<(int, int, int)>();
        Fire.Spread(ecs, world, seconds, _random, primed);
        return primed;
    }

    /// <summary>Lets go of every fire, as a world closes.</summary>
    public void Clear()
    {
        var all = new List<int>();
        foreach (var (id, _) in ecs.Query<Fire>()) all.Add(id);
        foreach (var id in all) ecs.Despawn(id);
    }

    /// <summary>Burns the fire of an entity at its tick.</summary>
    internal static void Burn(EcsWorld ecs, VoxelWorld world, int id, Random random, List<(int X, int Y, int Z)> primed)
    {
        var (x, y, z, age) = (ecs.GetRef<Fire>(id).X, ecs.GetRef<Fire>(id).Y, ecs.GetRef<Fire>(id).Z, ecs.GetRef<Fire>(id).Age);
        // A fire put out by a hand, a blast or water is let go at its tick.
        if (world.GetBlock(x, y, z) != BlockId.Fire)
        {
            ecs.Despawn(id);
            return;
        }
        if (!CanStand(world, x, y, z))
        {
            Out(ecs, world, id);
            return;
        }
        var below = world.GetBlock(x, y - 1, z);
        ref var fire = ref ecs.GetRef<Fire>(id);
        (fire.Age, fire.Wait) = (Math.Min(15, age + random.Next(3) / 2), Delay(random));
        if (below != BlockId.Magma)
        {
            if (!BesideFuel(world, x, y, z))
            {
                if (!Blocks.IsSturdy(below) || age > 3) Out(ecs, world, id);
                return;
            }
            if (age == 15 && random.Next(4) == 0 && !Burns(below))
            {
                Out(ecs, world, id);
                return;
            }
        }
        BurnAway(ecs, world, x + 1, y, z, 300, age, random, primed);
        BurnAway(ecs, world, x - 1, y, z, 300, age, random, primed);
        BurnAway(ecs, world, x, y - 1, z, 250, age, random, primed);
        BurnAway(ecs, world, x, y + 1, z, 250, age, random, primed);
        BurnAway(ecs, world, x, y, z - 1, 300, age, random, primed);
        BurnAway(ecs, world, x, y, z + 1, 300, age, random, primed);
        for (int dx = -1; dx <= 1; dx++)
            for (int dz = -1; dz <= 1; dz++)
                for (int dy = -1; dy <= 4; dy++)
                {
                    if (dx == 0 && dy == 0 && dz == 0) continue;
                    var catches = Catches(world, x + dx, y + dy, z + dz);
                    if (catches <= 0) continue;
                    var odds = (catches + 40 + Difficulty * 7) / (age + 30);
                    if (odds > 0 && random.Next(100 + Math.Max(0, dy - 1) * 100) <= odds)
                        Set(ecs, world, x + dx, y + dy, z + dz, Math.Min(15, age + random.Next(5) / 4), random);
                }
    }

    // Burns a block beside a fire at a chance of its burning odds in so many, into fire or away.
    private static void BurnAway(EcsWorld ecs, VoxelWorld world, int x, int y, int z, int chance, int age, Random random, List<(int X, int Y, int Z)> primed)
    {
        var block = world.GetBlock(x, y, z);
        if (random.Next(chance) >= _odds[(int)block].Burns) return;
        if (random.Next(age + 10) < 5) Set(ecs, world, x, y, z, Math.Min(15, age + random.Next(5) / 4), random);
        else world.SetBlock(x, y, z, BlockId.Air);
        if (block == BlockId.Tnt) primed.Add((x, y, z));
    }

    // Sets a fire into a cell that has none, which a fire spreading or flint and steel only reach.
    private static bool Set(EcsWorld ecs, VoxelWorld world, int x, int y, int z, int age, Random random)
    {
        if (!world.SetBlock(x, y, z, BlockId.Fire)) return false;
        ecs.Add(ecs.Spawn(), new Fire { X = x, Y = y, Z = z, Age = age, Wait = Delay(random) });
        return true;
    }

    private static void Out(EcsWorld ecs, VoxelWorld world, int id)
    {
        ref var fire = ref ecs.GetRef<Fire>(id);
        world.SetBlock(fire.X, fire.Y, fire.Z, BlockId.Air);
        ecs.Despawn(id);
    }

    private static float Delay(Random random) => (30 + random.Next(10)) * Tick;

    // How readily a cell catches, none but for air, where it is the best of its neighbors'.
    private static int Catches(VoxelWorld world, int x, int y, int z)
    {
        if (world.GetBlock(x, y, z) != BlockId.Air) return 0;
        var best = 0;
        foreach (var (dx, dy, dz) in Sides) best = Math.Max(best, _odds[(int)world.GetBlock(x + dx, y + dy, z + dz)].Catches);
        return best;
    }

    private static bool BesideFuel(VoxelWorld world, int x, int y, int z)
    {
        foreach (var (dx, dy, dz) in Sides)
            if (Burns(world.GetBlock(x + dx, y + dy, z + dz))) return true;
        return false;
    }

    private static readonly (int X, int Y, int Z)[] Sides = [(1, 0, 0), (-1, 0, 0), (0, 1, 0), (0, -1, 0), (0, 0, 1), (0, 0, -1)];

    // The source's odds, how readily each catches and burns, planks and what is made of them 5 and
    // 20, logs 5 and 5, leaves and wool 30 and 60, hay 60 and 20, TNT 15 and 100, and the flowers
    // and grasses 60 and 100, its saplings and mushrooms not burning.
    private static (int Catches, int Burns)[] Odds()
    {
        var odds = new (int, int)[Blocks.All.Count];
        void Set(IEnumerable<BlockId> blocks, int catches, int burns)
        {
            foreach (var block in blocks) odds[(int)block] = (catches, burns);
        }
        Set(BlockGroups.Planks, 5, 20);
        Set(Blocks.All.Where(b => b.Item is BlockId.OakSlab or BlockId.SpruceSlab or BlockId.OakStairs or BlockId.SpruceStairs or BlockId.OakFence).Select(b => b.Id), 5, 20);
        Set(BlockGroups.Logs, 5, 5);
        Set(BlockGroups.Leaves.Keys, 30, 60);
        Set(BlockGroups.Wool, 30, 60);
        Set([BlockId.HayBale], 60, 20);
        Set([BlockId.Tnt], 15, 100);
        Set(BlockGroups.Plants.Where(p => !Blocks.Get(p).Key.EndsWith("_sapling", StringComparison.Ordinal) && p is not (BlockId.BrownMushroom or BlockId.RedMushroom)), 60, 100);
        return odds;
    }
}
