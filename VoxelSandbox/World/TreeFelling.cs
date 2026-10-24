namespace VoxelSandbox;

/// <summary>
/// A tree felled where the log its trunk stands on is broken, its logs breaking from the cut upward
/// a block a tick, a share of them falling as blocks, and then its leaves decaying over a few
/// seconds, as the source's leaves do once no log stands within six leaves of them.
/// </summary>
/// <remarks>
/// The logs of a tree are those joined to the cut, edge or corner, at its height or above, within
/// twelve blocks of it across, as a big oak's branches join its trunk, so a tree touching another
/// fells only where their logs touch, and they fell only where leaves touch them, so a wall of logs
/// a house or a village stands on keeps its logs. Once the last of them has gone, every leaf within
/// seven blocks of where they stood is read for its way to a log through leaves, and those with
/// none within six steps decay, each at a time of its own from half a second to four, so the leaves
/// of a tree beside it stay.
/// </remarks>
public sealed class TreeFelling(int seed = 1)
{
    private const float Tick = 0.05f;
    private const int Reach = 12, MostLogs = 256, LeafReach = 6;

    private enum Step { Log, Leaves, Leaf }

    private readonly List<(float At, Step Step, int X, int Y, int Z, int X1, int Y1, int Z1)> _steps = [];
    private readonly Random _random = new(seed);
    private float _time;

    /// <summary>The logs and leaves broken, where and which, for the game to drop and to throw bits of, and clear.</summary>
    public List<(int X, int Y, int Z, BlockId Block)> Broken { get; } = [];

    /// <summary>The logs that fall as blocks, where they stood and which, for the game to let fall and clear.</summary>
    public List<(int X, int Y, int Z, BlockId Block)> Dropped { get; } = [];

    /// <summary>Whether a felling is still under way.</summary>
    public bool Busy => _steps.Count > 0;

    /// <summary>Fells the tree whose trunk stood on the log broken at a place, where the block under it was no log.</summary>
    public void Fell(VoxelWorld world, int x, int y, int z)
    {
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
                        if (ny < y || Math.Abs(nx - x) > Reach || Math.Abs(nz - z) > Reach || !seen.Add((nx, ny, nz))) continue;
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
        if (logs.Count == 0)
        {
            _steps.Add((_time, Step.Leaves, x - LeafReach - 1, y, z - LeafReach - 1, x + LeafReach + 1, y + LeafReach + 1, z + LeafReach + 1));
            return;
        }
        int x0 = x, y1 = y, z0 = z, x1 = x, z1 = z;
        foreach (var (lx, ly, lz) in logs)
        {
            _steps.Add((_time + (ly - y + 1) * Tick, Step.Log, lx, ly, lz, 0, 0, 0));
            (x0, x1, z0, z1, y1) = (Math.Min(x0, lx), Math.Max(x1, lx), Math.Min(z0, lz), Math.Max(z1, lz), Math.Max(y1, ly));
        }
        _steps.Add((_time + (y1 - y + 2) * Tick, Step.Leaves, x0 - LeafReach - 1, y, z0 - LeafReach - 1, x1 + LeafReach + 1, y1 + LeafReach + 1, z1 + LeafReach + 1));
    }

    /// <summary>Goes on by so many seconds, taking each log and leaf whose time has come.</summary>
    public void Update(VoxelWorld world, float seconds)
    {
        _time += seconds;
        for (int i = 0; i < _steps.Count; i++)
        {
            var step = _steps[i];
            if (step.At > _time) continue;
            _steps.RemoveAt(i--);
            var block = world.GetBlock(step.X, step.Y, step.Z);
            switch (step.Step)
            {
                case Step.Log when BlockGroups.Logs.Contains(block):
                    world.SetBlock(step.X, step.Y, step.Z, BlockId.Air);
                    (_random.Next(4) == 0 ? Dropped : Broken).Add((step.X, step.Y, step.Z, block));
                    break;
                case Step.Leaf when Blocks.IsLeaves(block):
                    world.SetBlock(step.X, step.Y, step.Z, BlockId.Air);
                    Broken.Add((step.X, step.Y, step.Z, block));
                    break;
                case Step.Leaves:
                    foreach (var (lx, ly, lz) in Loose(world, step.X, step.Y, step.Z, step.X1, step.Y1, step.Z1))
                        _steps.Add((_time + 0.5f + 3.5f * _random.NextSingle(), Step.Leaf, lx, ly, lz, 0, 0, 0));
                    break;
            }
        }
    }

    // The leaves within a box with no log six leaves or fewer away through leaves, read from every
    // log in the box and a band around it outward, as the source keeps each leaf's distance to a log.
    private static List<(int X, int Y, int Z)> Loose(VoxelWorld world, int x0, int y0, int z0, int x1, int y1, int z1)
    {
        var near = new HashSet<(int, int, int)>();
        var front = new Queue<((int X, int Y, int Z) At, int Steps)>();
        for (int y = y0 - LeafReach; y <= y1 + LeafReach; y++)
            for (int z = z0 - LeafReach; z <= z1 + LeafReach; z++)
                for (int x = x0 - LeafReach; x <= x1 + LeafReach; x++)
                    if (BlockGroups.Logs.Contains(world.GetBlock(x, y, z))) front.Enqueue(((x, y, z), 0));
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
        var loose = new List<(int, int, int)>();
        for (int y = y0; y <= y1; y++)
            for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++)
                    if (Blocks.IsLeaves(world.GetBlock(x, y, z)) && !near.Contains((x, y, z))) loose.Add((x, y, z));
        return loose;
    }
}
