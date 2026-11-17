namespace VoxelSandbox;

/// <summary>
/// A tree felled where the log its trunk stands on is broken, its logs breaking from the lowest
/// upward a block a tick, a share of them falling as blocks, and its leaves decaying over a second
/// from the tick its first log goes, as the source's leaves do once no log stands within six leaves
/// of them.
/// </summary>
/// <remarks>
/// The logs of a tree are those joined to the cut through any of the 26 blocks about each, edge or
/// corner, above the cut or under it, within twelve blocks of it across, as a big oak's branches
/// join its trunk, so a tree whose branches touch another's fells with it whole, the other's trunk
/// too where it stands lower than the cut. They fell only where leaves touch them, so a wall of
/// logs a house or a village stands on keeps its logs. As the tree is felled, every leaf within
/// seven blocks of its logs is read for its way through leaves to a log that is not the tree's, and
/// those with none within six steps decay one after another, in an order of their own, over the
/// second from the tick the first log goes, so the crown and the falling logs are in the air
/// together and the leaves of a tree beside it stay. An axe fells a tree by its uses, one for each
/// log that drops as an item and none for one that falls as a block, and one that runs out leaves
/// the logs still to go standing, and of the crown only the leaves loose of them decay.
/// </remarks>
public sealed class TreeFelling(int seed = 1)
{
    private const float Tick = 0.05f;
    private const int Reach = 12, MostLogs = 256, LeafReach = 6;
    // The seconds the crown's leaves take to go, one after another.
    private const float CrownSeconds = 1;

    private enum Step { Log, Leaf }

    // A tree being felled, the uses left of the axe that fells it, null where none are counted, the
    // box its crown is read in, and once the axe has run out the leaves loose of the logs left.
    private sealed class Tree(int? uses, (int X0, int Y0, int Z0, int X1, int Y1, int Z1) box, int hand)
    {
        public int? Uses = uses;
        public readonly int Hand = hand;
        public readonly (int X0, int Y0, int Z0, int X1, int Y1, int Z1) Box = box;
        public HashSet<(int X, int Y, int Z)>? Loose;
    }

    private readonly List<(float At, Step Step, int X, int Y, int Z, Tree Tree)> _steps = [];
    // The leaves a felling left standing since a column within their reach of a log was not
    // loaded, each group with its tree, asked again once those columns are.
    private readonly List<(Tree Tree, List<(int X, int Y, int Z)> Leaves)> _spared = [];
    private readonly Random _random = new(seed);
    private float _time;

    /// <summary>The logs and leaves broken, where and which, for the game to drop and to throw bits of, and clear.</summary>
    public List<(int X, int Y, int Z, BlockId Block)> Broken { get; } = [];

    /// <summary>The logs that fall as blocks, where they stood and which, for the game to let fall and clear.</summary>
    public List<(int X, int Y, int Z, BlockId Block)> Dropped { get; } = [];

    /// <summary>Whether a felling is still under way.</summary>
    public bool Busy => _steps.Count > 0;

    /// <summary>The uses the logs dropped as items have taken of the axes felling them, by the hand each tree was felled with, the hotbar slot of its axe, for the game to wear each axe by and clear.</summary>
    public Dictionary<int, int> WornBy { get; } = [];

    /// <summary>
    /// Fells the tree whose trunk stood on the log broken at a place, where the block under it was no
    /// log, with an axe of so many uses left in a hand, the hotbar slot it is in, or with no uses
    /// counted where none is given.
    /// </summary>
    public void Fell(VoxelWorld world, int x, int y, int z, int? uses = null, int hand = -1)
    {
        if (uses <= 0) return;
        var logs = new List<(int X, int Y, int Z)>();
        var seen = new HashSet<(int, int, int)> { (x, y, z) };
        var queue = new Queue<(int X, int Y, int Z)>([(x, y, z)]);
        while (queue.Count > 0 && logs.Count < MostLogs)
        {
            var (cx, cy, cz) = queue.Dequeue();
            for (int dy = -1; dy <= 1; dy++)
                for (int dz = -1; dz <= 1; dz++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = cx + dx, ny = cy + dy, nz = cz + dz;
                        if (Math.Abs(nx - x) > Reach || Math.Abs(nz - z) > Reach || !seen.Add((nx, ny, nz))) continue;
                        if (!BlockGroups.Logs.Contains(world.GetBlock(nx, ny, nz))) continue;
                        logs.Add((nx, ny, nz));
                        queue.Enqueue((nx, ny, nz));
                    }
        }
        bool Leafy((int X, int Y, int Z) at) =>
            Blocks.IsLeaves(world.GetBlock(at.X + 1, at.Y, at.Z)) || Blocks.IsLeaves(world.GetBlock(at.X - 1, at.Y, at.Z))
            || Blocks.IsLeaves(world.GetBlock(at.X, at.Y + 1, at.Z)) || Blocks.IsLeaves(world.GetBlock(at.X, at.Y - 1, at.Z))
            || Blocks.IsLeaves(world.GetBlock(at.X, at.Y, at.Z + 1)) || Blocks.IsLeaves(world.GetBlock(at.X, at.Y, at.Z - 1));
        if (!Leafy((x, y, z)) && !logs.Exists(Leafy)) return;
        int x0 = x, y0 = logs.Count == 0 ? y : logs.Min(l => Math.Min(l.Y, y)), y1 = y, z0 = z, x1 = x, z1 = z;
        foreach (var (lx, ly, lz) in logs)
            (x0, x1, z0, z1, y1) = (Math.Min(x0, lx), Math.Max(x1, lx), Math.Min(z0, lz), Math.Max(z1, lz), Math.Max(y1, ly));
        var tree = new Tree(uses, (x0 - LeafReach - 1, y0, z0 - LeafReach - 1, x1 + LeafReach + 1, y1 + LeafReach + 1, z1 + LeafReach + 1), hand);
        foreach (var (lx, ly, lz) in logs) _steps.Add((_time + (ly - y0 + 1) * Tick, Step.Log, lx, ly, lz, tree));
        // The crown is read now, the tree's logs counted as gone, and its leaves go from the tick the
        // first log does, after it, since the steps of a tick are taken in the order they were added.
        var (bx0, by0, bz0, bx1, by1, bz1) = tree.Box;
        var crown = Loose(world, bx0, by0, bz0, bx1, by1, bz1, [.. logs], tree);
        _random.Shuffle(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(crown));
        var start = _time + (logs.Count == 0 ? 0 : Tick);
        for (int i = 0; i < crown.Count; i++)
            _steps.Add((start + i * CrownSeconds / crown.Count, Step.Leaf, crown[i].X, crown[i].Y, crown[i].Z, tree));
    }

    /// <summary>Goes on by so many seconds, taking each log and leaf whose time has come.</summary>
    public void Update(VoxelWorld world, float seconds)
    {
        _time += seconds;
        AskSpared(world);
        Take(world, all: false);
    }

    /// <summary>Takes every log and leaf still to go at once, as the world is left or closed.</summary>
    public void Finish(VoxelWorld world) => Take(world, all: true);

    /// <summary>Lets go of every felling, as another world opens.</summary>
    public void Clear()
    {
        _steps.Clear();
        _spared.Clear();
        Broken.Clear();
        Dropped.Clear();
        WornBy.Clear();
    }

    /// <summary>
    /// Ends the fellings of the axe of a hand that left it, as its uses running out ends them, the
    /// logs still to go standing and of the crown only the leaves loose of them decaying.
    /// </summary>
    public void Disarm(VoxelWorld world, int hand)
    {
        foreach (var tree in _steps.Select(step => step.Tree).Where(tree => tree.Hand == hand && tree.Uses is not 0).Distinct().ToList())
        {
            tree.Uses = 0;
            var (x0, y0, z0, x1, y1, z1) = tree.Box;
            tree.Loose = [.. Loose(world, x0, y0, z0, x1, y1, z1, [], tree)];
        }
    }

    // Asks again the leaves spared beside a column not loaded once every column within their reach
    // is, those loose of every log decaying as a felled tree's crown does.
    private void AskSpared(VoxelWorld world)
    {
        for (int i = _spared.Count - 1; i >= 0; i--)
        {
            var (tree, leaves) = _spared[i];
            if (leaves.Exists(leaf => NearAbsent(world, leaf.X, leaf.Z))) continue;
            _spared.RemoveAt(i);
            var (x0, y0, z0, x1, y1, z1) = tree.Box;
            var loose = Loose(world, x0, y0, z0, x1, y1, z1, [], null).ToHashSet();
            var asked = new Tree(null, tree.Box, tree.Hand);
            foreach (var leaf in leaves)
                if (loose.Contains(leaf)) _steps.Add((_time, Step.Leaf, leaf.X, leaf.Y, leaf.Z, asked));
        }
    }

    // Whether a column within a leaf's reach of a log is not loaded, where a log holding the leaf
    // may stand.
    private static bool NearAbsent(VoxelWorld world, int x, int z)
    {
        for (int cz = VoxelWorld.ColumnOf(z - LeafReach); cz <= VoxelWorld.ColumnOf(z + LeafReach); cz++)
            for (int cx = VoxelWorld.ColumnOf(x - LeafReach); cx <= VoxelWorld.ColumnOf(x + LeafReach); cx++)
                if (!world.HasColumn(cx, cz)) return true;
        return false;
    }

    // Takes the logs and leaves whose time has come, or all of them.
    private void Take(VoxelWorld world, bool all)
    {
        for (int i = 0; i < _steps.Count; i++)
        {
            var step = _steps[i];
            if (!all && step.At > _time) continue;
            _steps.RemoveAt(i--);
            var block = world.GetBlock(step.X, step.Y, step.Z);
            switch (step.Step)
            {
                case Step.Log when BlockGroups.Logs.Contains(block) && step.Tree.Uses is not 0:
                    world.SetBlock(step.X, step.Y, step.Z, BlockId.Air);
                    if (_random.Next(4) == 0)
                    {
                        Dropped.Add((step.X, step.Y, step.Z, block));
                        break;
                    }
                    Broken.Add((step.X, step.Y, step.Z, block));
                    if (step.Tree.Uses is not { } left) break;
                    WornBy[step.Tree.Hand] = WornBy.GetValueOrDefault(step.Tree.Hand) + 1;
                    // The axe's last use leaves the logs still to go standing, and of the crown the
                    // leaves loose of them alone decay.
                    if ((step.Tree.Uses = left - 1) == 0)
                    {
                        var (x0, y0, z0, x1, y1, z1) = step.Tree.Box;
                        step.Tree.Loose = [.. Loose(world, x0, y0, z0, x1, y1, z1, [], step.Tree)];
                    }
                    break;
                case Step.Leaf when Blocks.IsLeaves(block) && step.Tree.Loose?.Contains((step.X, step.Y, step.Z)) != false:
                    world.SetBlock(step.X, step.Y, step.Z, BlockId.Air);
                    Broken.Add((step.X, step.Y, step.Z, block));
                    break;
            }
        }
    }

    // The leaves within a box with no log six leaves or fewer away through leaves, read from every
    // log in the box and a band around it outward but those felled, as the source keeps each leaf's
    // distance to a log. A leaf within reach of a column not loaded, where a log holding it may
    // stand, is spared for its tree where one is given, and asked again once the column is loaded.
    private List<(int X, int Y, int Z)> Loose(VoxelWorld world, int x0, int y0, int z0, int x1, int y1, int z1, HashSet<(int X, int Y, int Z)> felled, Tree? tree)
    {
        var near = new HashSet<(int, int, int)>();
        var front = new Queue<((int X, int Y, int Z) At, int Steps)>();
        for (int y = y0 - LeafReach; y <= y1 + LeafReach; y++)
            for (int z = z0 - LeafReach; z <= z1 + LeafReach; z++)
                for (int x = x0 - LeafReach; x <= x1 + LeafReach; x++)
                    if (BlockGroups.Logs.Contains(world.GetBlock(x, y, z)) && !felled.Contains((x, y, z))) front.Enqueue(((x, y, z), 0));
        ReadOnlySpan<(int, int, int)> ways = [(1, 0, 0), (-1, 0, 0), (0, 1, 0), (0, -1, 0), (0, 0, 1), (0, 0, -1)];
        while (front.Count > 0)
        {
            var ((x, y, z), steps) = front.Dequeue();
            if (steps == LeafReach) continue;
            foreach (var (dx, dy, dz) in ways)
            {
                var next = (x + dx, y + dy, z + dz);
                if (Blocks.IsLeaves(world.GetBlock(x + dx, y + dy, z + dz)) && near.Add(next)) front.Enqueue((next, steps + 1));
            }
        }
        var (loose, spared) = (new List<(int X, int Y, int Z)>(), new List<(int X, int Y, int Z)>());
        for (int y = y0; y <= y1; y++)
            for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++)
                    if (Blocks.IsLeaves(world.GetBlock(x, y, z)) && !near.Contains((x, y, z)))
                        (tree is not null && NearAbsent(world, x, z) ? spared : loose).Add((x, y, z));
        if (spared.Count > 0) _spared.Add((tree!, spared));
        return loose;
    }
}
