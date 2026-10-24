namespace VoxelSandbox;

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
/// the world, as a column holding some arrives, starts at age 0.
/// </para>
/// </remarks>
public sealed class Fires
{
    private const float Tick = 0.05f;
    private const int Difficulty = 2;

    // How readily each block catches fire into the air beside it and how readily it burns away, the
    // source's odds, both none for a block that does not burn.
    private static readonly (int Catches, int Burns)[] _odds = Odds();

    private readonly Dictionary<(int X, int Y, int Z), (int Age, float Wait)> _burning = [];
    private readonly List<KeyValuePair<(int X, int Y, int Z), (int Age, float Wait)>> _due = [];
    private readonly Random _random;

    public Fires(Random? random = null) => _random = random ?? new Random();

    /// <summary>Where each fire burns and its age.</summary>
    public IEnumerable<((int X, int Y, int Z) At, int Age)> All => _burning.Select(f => (f.Key, f.Value.Age));

    /// <summary>The TNT the fires burned, where it stood, for the game to light and clear.</summary>
    public List<(int X, int Y, int Z)> Primed { get; } = [];

    /// <summary>Whether a block burns, as the source's planks, logs, leaves, wool, hay, TNT and plants of the ground do.</summary>
    public static bool Burns(BlockId block) => _odds[(int)block].Burns > 0;

    /// <summary>Whether a fire can stand in a cell, over a whole block or beside one that burns.</summary>
    public static bool CanStand(VoxelWorld world, int x, int y, int z) => Blocks.IsSturdy(world.GetBlock(x, y - 1, z)) || BesideFuel(world, x, y, z);

    /// <summary>Lights a fire in a cell of air where it can stand, as flint and steel does.</summary>
    /// <returns>Whether it was lit.</returns>
    public bool Light(VoxelWorld world, int x, int y, int z)
    {
        if (world.GetBlock(x, y, z) != BlockId.Air || !CanStand(world, x, y, z)) return false;
        return Set(world, x, y, z, 0);
    }

    /// <summary>Takes the fires the world set, by an edit or as a column holding some arrived, to keep them burning.</summary>
    public void Take(VoxelWorld world)
    {
        foreach (var at in world.Kindled) _burning.TryAdd(at, (0, Delay()));
        world.Kindled.Clear();
    }

    /// <summary>Goes on by so many seconds, each fire whose tick comes burning as the source's does.</summary>
    public void Update(VoxelWorld world, float seconds)
    {
        _due.Clear();
        foreach (var fire in _burning)
            if (fire.Value.Wait <= seconds) _due.Add(fire);
            else _burning[fire.Key] = (fire.Value.Age, fire.Value.Wait - seconds);
        foreach (var (at, (age, _)) in _due) Burn(world, at.X, at.Y, at.Z, age);
    }

    /// <summary>Lets go of every fire, as a world closes.</summary>
    public void Clear()
    {
        _burning.Clear();
        Primed.Clear();
    }

    private void Burn(VoxelWorld world, int x, int y, int z, int age)
    {
        // A fire put out by a hand, a blast or water is let go at its tick.
        if (world.GetBlock(x, y, z) != BlockId.Fire)
        {
            _burning.Remove((x, y, z));
            return;
        }
        if (!CanStand(world, x, y, z))
        {
            Out(world, x, y, z);
            return;
        }
        var below = world.GetBlock(x, y - 1, z);
        _burning[(x, y, z)] = (Math.Min(15, age + _random.Next(3) / 2), Delay());
        if (below != BlockId.Magma)
        {
            if (!BesideFuel(world, x, y, z))
            {
                if (!Blocks.IsSturdy(below) || age > 3) Out(world, x, y, z);
                return;
            }
            if (age == 15 && _random.Next(4) == 0 && !Burns(below))
            {
                Out(world, x, y, z);
                return;
            }
        }
        BurnAway(world, x + 1, y, z, 300, age);
        BurnAway(world, x - 1, y, z, 300, age);
        BurnAway(world, x, y - 1, z, 250, age);
        BurnAway(world, x, y + 1, z, 250, age);
        BurnAway(world, x, y, z - 1, 300, age);
        BurnAway(world, x, y, z + 1, 300, age);
        for (int dx = -1; dx <= 1; dx++)
            for (int dz = -1; dz <= 1; dz++)
                for (int dy = -1; dy <= 4; dy++)
                {
                    if (dx == 0 && dy == 0 && dz == 0) continue;
                    var catches = Catches(world, x + dx, y + dy, z + dz);
                    if (catches <= 0) continue;
                    var odds = (catches + 40 + Difficulty * 7) / (age + 30);
                    if (odds > 0 && _random.Next(100 + Math.Max(0, dy - 1) * 100) <= odds)
                        Set(world, x + dx, y + dy, z + dz, Math.Min(15, age + _random.Next(5) / 4));
                }
    }

    // Burns a block beside a fire at a chance of its burning odds in so many, into fire or away.
    private void BurnAway(VoxelWorld world, int x, int y, int z, int chance, int age)
    {
        var block = world.GetBlock(x, y, z);
        if (_random.Next(chance) >= _odds[(int)block].Burns) return;
        if (_random.Next(age + 10) < 5) Set(world, x, y, z, Math.Min(15, age + _random.Next(5) / 4));
        else world.SetBlock(x, y, z, BlockId.Air);
        if (block == BlockId.Tnt) Primed.Add((x, y, z));
    }

    private bool Set(VoxelWorld world, int x, int y, int z, int age)
    {
        if (!world.SetBlock(x, y, z, BlockId.Fire)) return false;
        _burning[(x, y, z)] = (age, Delay());
        return true;
    }

    private void Out(VoxelWorld world, int x, int y, int z)
    {
        world.SetBlock(x, y, z, BlockId.Air);
        _burning.Remove((x, y, z));
    }

    private float Delay() => (30 + _random.Next(10)) * Tick;

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
