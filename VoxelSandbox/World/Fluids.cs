namespace VoxelSandbox;

/// <summary>
/// Water that flows down into the air under it as falling water, and where it cannot fall, sideways
/// one level further from its source for each block, up to seven, drying back as its source goes.
/// Two sources beside each other over ground make a third between them.
/// </summary>
/// <remarks>
/// <para>
/// Water moves in ticks of 20 a second, a block every five. A block that changes asks the water at
/// and around it to look again at what feeds it five ticks on, and each cell of water so asked
/// takes the level its neighbors give it and spreads from there, so a still lake stays still until
/// something beside it changes. Each tick moves at most so many cells, so a flood is spread over
/// frames rather than taken in one.
/// </para>
/// <para>
/// What is waiting is not saved, so water half flowed when the world is left stands as it was when
/// the world is opened again, until a block beside it changes.
/// </para>
/// </remarks>
public sealed class Fluids(VoxelWorld world)
{
    private const float Tick = 1 / 20f;
    private const int Delay = 5;
    private const int MostPerTick = 4096;

    private readonly Queue<(int X, int Y, int Z, long Due)> _due = new();
    private readonly HashSet<(int X, int Y, int Z)> _waiting = [];
    private long _tick;
    private float _time;

    /// <summary>Cells of water waiting to look again at what feeds them.</summary>
    public int Waiting => _waiting.Count;

    /// <summary>Asks a cell and the six around it to look again, five ticks on.</summary>
    public void Around(int x, int y, int z)
    {
        Ask(x, y, z);
        for (int d = 0; d < 6; d++) Ask(x + Lighting.Dx[d], y + Lighting.Dy[d], z + Lighting.Dz[d]);
    }

    /// <summary>
    /// Asks the flowing water along the edges between a column that has arrived and the loaded
    /// columns beside it to look again where it faces air across the edge, on either side, so water
    /// that stopped at the edge of a column not loaded flows on into it. A source is left alone, so a
    /// sea or a lake the land made beside a cave across the edge stays as it was made. A packed
    /// section is read only where it holds flowing water or flowing water faces it, so the land's
    /// deep sections stay packed.
    /// </summary>
    public void Join(ChunkColumn column)
    {
        ReadOnlySpan<(int Dx, int Dz)> sides = [(1, 0), (-1, 0), (0, 1), (0, -1)];
        foreach (var (dx, dz) in sides)
        {
            if (!world.TryGetColumn(column.X + dx, column.Z + dz, out var other)) continue;
            for (int y = ChunkColumn.Bottom; y < ChunkColumn.Ceiling; y++)
            {
                var (near, far) = (column.At(y), other.At(y));
                bool nearFlows = !near.Open && (!near.Packed || near.Flows), farFlows = !far.Open && (!far.Packed || far.Flows);
                if (!nearFlows && !farFlows)
                {
                    y += Section.Mask;
                    continue;
                }
                for (int t = 0; t < Section.Size; t++)
                {
                    // The cell on this column's edge and the one across it in the neighbor.
                    int ax = dx == 0 ? t : dx > 0 ? Section.Mask : 0, az = dz == 0 ? t : dz > 0 ? Section.Mask : 0;
                    int bx = dx == 0 ? t : dx > 0 ? 0 : Section.Mask, bz = dz == 0 ? t : dz > 0 ? 0 : Section.Mask;
                    if (nearFlows && Flowing(column.Get(ax, y, az)) && other.Get(bx, y, bz) == BlockId.Air) Ask(column.X * Section.Size + ax, y, column.Z * Section.Size + az);
                    else if (farFlows && Flowing(other.Get(bx, y, bz)) && column.Get(ax, y, az) == BlockId.Air) Ask(other.X * Section.Size + bx, y, other.Z * Section.Size + bz);
                }
            }
        }
    }

    private static bool Flowing(BlockId block) => Blocks.IsWater(block) && block != BlockId.Water;

    private void Ask(int x, int y, int z)
    {
        if (Blocks.IsWater(world.GetBlock(x, y, z)) && _waiting.Add((x, y, z))) _due.Enqueue((x, y, z, _tick + Delay));
    }

    /// <summary>Runs the ticks that <paramref name="seconds"/> brings.</summary>
    public void Update(float seconds)
    {
        _time += seconds;
        while (_time >= Tick)
        {
            _time -= Tick;
            _tick++;
            for (int moved = 0; moved < MostPerTick && _due.TryPeek(out var next) && next.Due <= _tick; moved++)
            {
                _due.Dequeue();
                _waiting.Remove((next.X, next.Y, next.Z));
                Flow(next.X, next.Y, next.Z);
            }
        }
    }

    // A cell of water takes what feeds it, a source what it is, flowing water one level further than
    // the nearest beside it, or falling water under water, and dries where nothing does; then it
    // falls into the air under it, or where it stands on something spreads sideways a level further.
    private void Flow(int x, int y, int z)
    {
        var block = world.GetBlock(x, y, z);
        if (!Blocks.IsWater(block)) return;

        if (block != BlockId.Water)
        {
            var fed = Fed(x, y, z);
            if (fed != block)
            {
                world.SetBlock(x, y, z, fed);
                return;
            }
        }

        var below = world.GetBlock(x, y - 1, z);
        if (below == BlockId.Air)
        {
            world.SetBlock(x, y - 1, z, BlockId.FallingWater);
            return;
        }
        // Water spreads sideways from ground under it or from a source, and not from the top of
        // flowing water, which carries it on itself.
        if (!Blocks.Collides(below) && below != BlockId.Water) return;
        var next = Blocks.WaterLevel(block) + 1;
        if (next > 7) return;
        for (int d = 0; d < 6; d++)
        {
            if (Lighting.Dy[d] != 0) continue;
            int nx = x + Lighting.Dx[d], nz = z + Lighting.Dz[d];
            var beside = world.GetBlock(nx, y, nz);
            if (beside == BlockId.Air || beside is >= BlockId.Water1 and <= BlockId.Water7 && Blocks.WaterLevel(beside) > next)
                world.SetBlock(nx, y, nz, Blocks.FlowingWater(next));
        }
    }

    // What a cell that is not a source should be by what feeds it.
    private BlockId Fed(int x, int y, int z)
    {
        if (Blocks.IsWater(world.GetBlock(x, y + 1, z))) return BlockId.FallingWater;
        int nearest = 8, sources = 0;
        for (int d = 0; d < 6; d++)
        {
            if (Lighting.Dy[d] != 0) continue;
            var beside = world.GetBlock(x + Lighting.Dx[d], y, z + Lighting.Dz[d]);
            if (!Blocks.IsWater(beside)) continue;
            if (beside == BlockId.Water) sources++;
            // Water beside that stands on falling water or on air carries none sideways.
            var under = world.GetBlock(x + Lighting.Dx[d], y - 1, z + Lighting.Dz[d]);
            if (beside != BlockId.Water && !Blocks.Collides(under) && under != BlockId.Water) continue;
            nearest = Math.Min(nearest, Blocks.WaterLevel(beside));
        }
        var ground = world.GetBlock(x, y - 1, z);
        if (sources >= 2 && (Blocks.Collides(ground) || ground == BlockId.Water)) return BlockId.Water;
        return nearest + 1 <= 7 ? Blocks.FlowingWater(nearest + 1) : BlockId.Air;
    }
}
