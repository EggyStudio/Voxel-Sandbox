namespace VoxelSandbox;

/// <summary>
/// Water that flows down into the air under it as falling water, and where it cannot fall, sideways
/// one level further from its source for each block, up to seven, drying back as its source goes.
/// Two sources beside each other over ground make a third between them. It washes away a torch or
/// a crop it flows into, which drops as a block falling with its support does, and makes obsidian
/// of the lava beside it or under it.
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
/// What is waiting is not saved, and a column's flowing water is asked again as it arrives, so
/// water half flowed when its column was let go or the world was left goes on as it arrives again.
/// Water beside a column not loaded, whose feeder may stand there, keeps its level until the column
/// arrives.
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
    /// Asks every flowing cell of a column that has arrived to look again, since what waited in it
    /// was let go as it unloaded, so a flood or a stream drying there goes on from where it stood,
    /// and the flowing water across its edges in the loaded columns beside, whose feeder may lie in
    /// it. A source is left alone, so a sea or a lake the land made beside a cave across the edge
    /// stays as it was made. A packed section is read only where it holds flowing water, so the
    /// land's deep sections stay packed.
    /// </summary>
    public void Join(ChunkColumn column)
    {
        for (int y = ChunkColumn.Bottom; y < ChunkColumn.Ceiling; y += Section.Size)
        {
            var section = column.At(y);
            if (section.Open || (section.Packed ? !section.Flows : !section.Blocks.AsSpan().ContainsAnyInRange((ushort)BlockId.Water1, (ushort)BlockId.FallingWater)))
                continue;
            for (int i = 0; i < Section.Volume; i++)
            {
                int lx = i & Section.Mask, ly = y + (i >> 8), lz = (i >> 4) & Section.Mask;
                if (Flowing(column.Get(lx, ly, lz))) Ask(column.X * Section.Size + lx, ly, column.Z * Section.Size + lz);
            }
        }
        ReadOnlySpan<(int Dx, int Dz)> sides = [(1, 0), (-1, 0), (0, 1), (0, -1)];
        foreach (var (dx, dz) in sides)
        {
            if (!world.TryGetColumn(column.X + dx, column.Z + dz, out var other)) continue;
            for (int y = ChunkColumn.Bottom; y < ChunkColumn.Ceiling; y++)
            {
                var far = other.At(y);
                if (far.Open || far.Packed && !far.Flows)
                {
                    y += Section.Mask;
                    continue;
                }
                for (int t = 0; t < Section.Size; t++)
                {
                    // The cell across this column's edge in the neighbor.
                    int bx = dx == 0 ? t : dx > 0 ? 0 : Section.Mask, bz = dz == 0 ? t : dz > 0 ? 0 : Section.Mask;
                    if (Flowing(other.Get(bx, y, bz))) Ask(other.X * Section.Size + bx, y, other.Z * Section.Size + bz);
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
        for (int d = 0; d < 6; d++)
            if (Lighting.Dy[d] <= 0 && world.GetBlock(x + Lighting.Dx[d], y + Lighting.Dy[d], z + Lighting.Dz[d]) == BlockId.Lava)
                world.SetBlock(x + Lighting.Dx[d], y + Lighting.Dy[d], z + Lighting.Dz[d], BlockId.Obsidian);

        if (block != BlockId.Water)
        {
            var fed = Fed(x, y, z);
            if (fed != block)
            {
                // Water beside a column not loaded, whose feeder may stand there, keeps its level,
                // and is asked again as the column arrives.
                if (BesideAbsent(x, z)) return;
                world.SetBlock(x, y, z, fed);
                return;
            }
        }

        var below = world.GetBlock(x, y - 1, z);
        if (below == BlockId.Air || Washes(below))
        {
            Wash(x, y - 1, z, below);
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
            if (beside == BlockId.Air || Washes(beside) || beside is >= BlockId.Water1 and <= BlockId.Water7 && Blocks.WaterLevel(beside) > next)
            {
                Wash(nx, y, nz, beside);
                world.SetBlock(nx, y, nz, Blocks.FlowingWater(next));
            }
        }
    }

    // Whether water flowing into a cell washes its block away, a torch or a crop.
    private static bool Washes(BlockId block) =>
        block is >= BlockId.Torch and <= BlockId.SoulWallTorchEast or BlockId.Wheat or BlockId.YoungWheat;

    // A block water washes away drops as one falling with its support does.
    private void Wash(int x, int y, int z, BlockId block)
    {
        if (Washes(block)) world.Fallen.Add((x, y, z, block));
    }

    // Whether a column beside a cell is not loaded.
    private bool BesideAbsent(int x, int z) =>
        !world.HasColumn(VoxelWorld.ColumnOf(x + 1), VoxelWorld.ColumnOf(z)) || !world.HasColumn(VoxelWorld.ColumnOf(x - 1), VoxelWorld.ColumnOf(z))
        || !world.HasColumn(VoxelWorld.ColumnOf(x), VoxelWorld.ColumnOf(z + 1)) || !world.HasColumn(VoxelWorld.ColumnOf(x), VoxelWorld.ColumnOf(z - 1));

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
