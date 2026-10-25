namespace VoxelSandbox;

/// <summary>
/// A chest's 27 slots, and the loot a structure left in it, rolled the first time the chest is
/// opened or broken, as the source rolls a chest's loot table, from the seed its place gave it.
/// </summary>
public sealed class Chest
{
    public const int Size = 27;

    public ItemStack?[] Slots { get; } = new ItemStack?[Size];

    /// <summary>The name of the <see cref="ChestLoot"/> table its loot is still to be rolled from, or null.</summary>
    public string? Loot { get; set; }

    /// <summary>The seed its loot is rolled from, which its place and the world's seed gave it.</summary>
    public uint Seed { get; set; }

    /// <summary>Rolls the loot it holds into its slots, each stack into an empty slot picked at random, once.</summary>
    public void Roll()
    {
        if (Loot is not { } name) return;
        Loot = null;
        if (ChestLoot.Find(name) is not { } table) return;
        var random = new Random((int)Seed);
        foreach (var stack in table.Roll(random))
        {
            var empty = Enumerable.Range(0, Size).Where(i => Slots[i] is null).ToList();
            if (empty.Count == 0) break;
            Slots[empty[random.Next(empty.Count)]] = stack;
        }
    }
}

/// <summary>A chest a structure placed as a column was generated, its place and the loot it holds.</summary>
public readonly record struct ChestPlaced(int X, int Y, int Z, string Loot, uint Seed);

/// <summary>
/// The chests of a world by the place of their block, each given up what it holds once its block
/// is gone, broken or blown up, as a furnace is.
/// </summary>
/// <remarks>
/// A generated column's chests join as the column does, their loot not yet rolled, and a save keeps
/// every chest, those of columns not loaded among them, in the world's <c>chests.json</c>. A column
/// generated again, one no edit changed, finds its chests already there and leaves them as they are.
/// </remarks>
public sealed class Chests
{
    private readonly Dictionary<(int X, int Y, int Z), Chest> _all = [];
    private readonly List<(int X, int Y, int Z)> _gone = [];

    public IReadOnlyDictionary<(int X, int Y, int Z), Chest> All => _all;

    /// <summary>The chest of the block at a place, begun empty the first time it is asked for, its loot rolled.</summary>
    public Chest At(int x, int y, int z)
    {
        if (!_all.TryGetValue((x, y, z), out var chest)) _all[(x, y, z)] = chest = new Chest();
        chest.Roll();
        return chest;
    }

    public void Add((int X, int Y, int Z) at, Chest chest) => _all[at] = chest;

    /// <summary>
    /// Takes the chests a generated column's structures placed, those not already known whose block
    /// still stands, since a piece built after one may have cut it away, and lets the column's list go.
    /// </summary>
    public void Joined(ChunkColumn column)
    {
        foreach (var placed in column.Chests)
            if (IsChest(column.Get(placed.X - column.X * Section.Size, placed.Y, placed.Z - column.Z * Section.Size)))
                _all.TryAdd((placed.X, placed.Y, placed.Z), new Chest { Loot = placed.Loot, Seed = placed.Seed });
        column.Chests.Clear();
    }

    public static bool IsChest(BlockId block) => block is BlockId.Chest or BlockId.ChestEast or BlockId.ChestSouth or BlockId.ChestWest;

    /// <summary>Takes away every chest of a loaded column whose block is gone, its loot rolled first.</summary>
    /// <returns>What those chests held, and where, for the caller to drop.</returns>
    public List<((int X, int Y, int Z) At, ItemStack Stack)> Update(VoxelWorld world)
    {
        var spilled = new List<((int, int, int), ItemStack)>();
        foreach (var (at, chest) in _all)
            if (world.HasColumn(VoxelWorld.ColumnOf(at.X), VoxelWorld.ColumnOf(at.Z)) && !IsChest(world.GetBlock(at.X, at.Y, at.Z)))
            {
                chest.Roll();
                foreach (var stack in chest.Slots)
                    if (stack is { } held) spilled.Add((at, held));
                _gone.Add(at);
            }
        foreach (var at in _gone) _all.Remove(at);
        _gone.Clear();
        return spilled;
    }
}

/// <summary>
/// A table a chest's loot is rolled from, pools of entries each rolled a number of times, as the
/// source's loot tables are, each entry an item, how many of it and its weight against the pool's
/// others, or nothing where its item is null.
/// </summary>
public sealed record ChestLoot(string Name, ChestLoot.Pool[] Pools)
{
    public readonly record struct Entry(Item? Item, int Weight, int Least = 1, int Most = 1);

    public readonly record struct Pool(int LeastRolls, int MostRolls, Entry[] Entries);

    /// <summary>The stacks a roll gives, a stack for each entry rolled, none for an empty one.</summary>
    public IEnumerable<ItemStack> Roll(Random random)
    {
        foreach (var pool in Pools)
        {
            var total = pool.Entries.Sum(e => e.Weight);
            var rolls = random.Next(pool.LeastRolls, pool.MostRolls + 1);
            for (int roll = 0; roll < rolls; roll++)
            {
                var pick = random.Next(total);
                foreach (var entry in pool.Entries)
                {
                    if ((pick -= entry.Weight) >= 0) continue;
                    if (entry.Item is { } item)
                        yield return new ItemStack(item, Math.Min(item.MostInStack, random.Next(entry.Least, entry.Most + 1)));
                    break;
                }
            }
        }
    }

    /// <summary>
    /// An abandoned mineshaft's, the source's table's weights and counts with the items the game has,
    /// an apple for the golden one, the game's wheat seeds for the source's three kinds of seeds and
    /// plain rails for its powered, detector and activator ones, and nothing for its name tag, its book
    /// and its enchanted apple.
    /// </summary>
    public static readonly ChestLoot Mineshaft = new("mineshaft",
    [
        new(1, 1, [new(Items.Apple, 20), new(Items.Find("iron_pickaxe"), 5), new(null, 46)]),
        new(2, 4,
        [
            new(Items.IronIngot, 10, 1, 5), new(Items.GoldIngot, 5, 1, 3), new(Items.Redstone, 5, 4, 9), new(Items.LapisLazuli, 5, 4, 9),
            new(Items.Diamond, 3, 1, 2), new(Items.Coal, 10, 3, 8), new(Items.Bread, 15, 1, 3), new(Items.Of(BlockId.CaveVines), 15, 3, 6),
            new(Items.Of(BlockId.Wheat), 30, 2, 4),
        ]),
        new(3, 3, [new(Items.Of(BlockId.Rail), 20, 4, 8), new(Items.Of(BlockId.Rail), 15, 1, 4), new(Items.Of(BlockId.Torch), 15, 1, 16)]),
    ]);

    /// <summary>Every table, by its name.</summary>
    public static readonly ChestLoot[] All = [Mineshaft];

    public static ChestLoot? Find(string name) => All.FirstOrDefault(t => t.Name == name);
}
