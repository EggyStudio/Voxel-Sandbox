namespace VoxelSandbox;

/// <summary>
/// Minecraft's two light levels, the sky's and the emissive blocks'. The sky's is 15 under the open
/// sky, falls straight down without losing any and loses one level for each block it goes sideways
/// or up, and an emissive block's loses one level for each block from it. Opaque blocks stop both,
/// glass lets them through, water takes a level more for each block of it, so the sky's light
/// fades with depth, and leaves take one level for each block of them, the way down too, so a
/// forest's floor is shaded by its canopy rather than darkened as a cave is, as Minecraft's leaves
/// let the light through.
/// </summary>
/// <remarks>
/// Each block's levels darken the faces beside it where neither the sky nor a lamp reaches, so a
/// cave dug into is dark until it is lit, whatever indirect light comes in. They are worked out by
/// spreading from the light outward one block at a time, and taken back the same way when a block
/// is placed, the levels that depended on the light taken away and the edge of what remains spread
/// again.
/// </remarks>
public static class Lighting
{
    public const int Max = 15;

    // The six neighbors in the order +x, -x, +y, -y, +z, -z, the fourth being straight down.
    internal static readonly int[] Dx = [1, -1, 0, 0, 0, 0];
    internal static readonly int[] Dy = [0, 0, 1, -1, 0, 0];
    internal static readonly int[] Dz = [0, 0, 0, 0, 1, -1];
    internal const int Down = 3;

    /// <summary>The level a neighbor of a kind of block takes from a block at <paramref name="level"/> the way <paramref name="direction"/> goes, or 0 for one light does not enter.</summary>
    internal static int Into(int level, int direction, bool sky, BlockId neighbor)
    {
        if (Blocks.IsLeaves(neighbor)) return level - 1;
        if (Blocks.IsOpaque(neighbor)) return 0;
        if (Blocks.IsWater(neighbor)) return level - 2;
        return sky && direction == Down && level == Max ? Max : level - 1;
    }

    // Whether the sky shines straight down through a block without losing a level, as through air and glass.
    private static bool Clear(BlockId block) => !Blocks.IsOpaque(block) && !Blocks.IsWater(block);

    /// <summary>
    /// Works out a column's light as though its sides were walls, on the worker that generated it,
    /// so only the light that crosses its edges is left for <see cref="LightEngine.Join"/>.
    /// </summary>
    public static void Compute(ChunkColumn column)
    {
        var tops = new int[Section.Size * Section.Size];
        for (int z = 0; z < Section.Size; z++)
            for (int x = 0; x < Section.Size; x++)
            {
                var y = ChunkColumn.Ceiling - 1;
                for (; y >= ChunkColumn.Bottom && Clear(column.Get(x, y, z)); y--)
                    column.At(y).SetLevel(Section.Index(x, y & Section.Mask, z), true, Max);
                tops[z * Section.Size + x] = y;
            }

        // The open sky beside a column whose top is higher spreads sideways under its overhang and
        // into any cave opening from it, and the lowest open cell of each place spreads down into
        // the water under it, so those cells are where the spreading starts.
        var queue = new Queue<(int X, int Y, int Z)>();
        for (int z = 0; z < Section.Size; z++)
            for (int x = 0; x < Section.Size; x++)
            {
                var highest = tops[z * Section.Size + x];
                for (int d = 0; d < 6; d++)
                {
                    int nx = x + Dx[d], nz = z + Dz[d];
                    if (Dy[d] != 0 || (uint)nx >= Section.Size || (uint)nz >= Section.Size) continue;
                    highest = Math.Max(highest, tops[nz * Section.Size + nx]);
                }
                var lowest = tops[z * Section.Size + x] + 1;
                for (int y = lowest; y <= Math.Max(highest, lowest) && y < ChunkColumn.Ceiling; y++) queue.Enqueue((x, y, z));
            }
        Spread(column, queue, sky: true);

        for (int y = ChunkColumn.Bottom; y < ChunkColumn.Ceiling; y++)
            for (int z = 0; z < Section.Size; z++)
                for (int x = 0; x < Section.Size; x++)
                {
                    var light = Blocks.Get(column.Get(x, y, z)).Light;
                    if (light == 0) continue;
                    column.At(y).SetLevel(Section.Index(x, y & Section.Mask, z), false, light);
                    queue.Enqueue((x, y, z));
                }
        Spread(column, queue, sky: false);
    }

    private static void Spread(ChunkColumn column, Queue<(int X, int Y, int Z)> queue, bool sky)
    {
        while (queue.TryDequeue(out var cell))
        {
            var level = column.At(cell.Y).Level(Section.Index(cell.X, cell.Y & Section.Mask, cell.Z), sky);
            if (level <= 1) continue;
            for (int d = 0; d < 6; d++)
            {
                int x = cell.X + Dx[d], y = cell.Y + Dy[d], z = cell.Z + Dz[d];
                if ((uint)x >= Section.Size || (uint)z >= Section.Size || !ChunkColumn.Holds(y)) continue;
                var section = column.At(y);
                var index = Section.Index(x, y & Section.Mask, z);
                var next = Into(level, d, sky, (BlockId)section.Blocks[index]);
                if (section.Level(index, sky) >= next) continue;
                section.SetLevel(index, sky, next);
                queue.Enqueue((x, y, z));
            }
        }
    }
}

/// <summary>
/// Keeps the loaded world's light levels as columns arrive and blocks change, on the main thread,
/// and marks each section whose faces a changed level shades.
/// </summary>
public sealed class LightEngine(VoxelWorld world)
{
    private readonly Queue<(int X, int Y, int Z)> _sky = new(), _block = new();
    private readonly Queue<(int X, int Y, int Z, int Level)> _taken = new();

    private bool TryCell(int x, int y, int z, out Section section, out int index)
    {
        if (!ChunkColumn.Holds(y) || !world.TryGetColumn(x >> Section.Shift, z >> Section.Shift, out var column))
        {
            (section, index) = (null!, 0);
            return false;
        }
        section = column.At(y);
        index = Section.Index(x & Section.Mask, y & Section.Mask, z & Section.Mask);
        return true;
    }

    private void Set(int x, int y, int z, Section section, int index, bool sky, int level)
    {
        section.SetLevel(index, sky, level);
        world.Reshade(x, y, z);
    }

    /// <summary>
    /// Spreads the light across the edges between a column that has arrived and the loaded columns
    /// beside it, from whichever side of each edge is brighter by two levels or more.
    /// </summary>
    public void Join(ChunkColumn column)
    {
        ReadOnlySpan<(int Dx, int Dz)> sides = [(1, 0), (-1, 0), (0, 1), (0, -1)];
        foreach (var (dx, dz) in sides)
        {
            if (!world.TryGetColumn(column.X + dx, column.Z + dz, out var other)) continue;
            for (int y = ChunkColumn.Bottom; y < ChunkColumn.Ceiling; y++)
            {
                var (mine, theirs) = (column.At(y), other.At(y));
                // Two sections of open air, or two packed with no light in them, have no light to
                // join, and a packed one is left packed.
                if (mine.Open && theirs.Open || mine is { Packed: true, Dark: true } && theirs is { Packed: true, Dark: true })
                {
                    y += Section.Mask;
                    continue;
                }
                for (int t = 0; t < Section.Size; t++)
                {
                    // The cell on this column's edge and the one across it in the neighbor.
                    int ax = dx == 0 ? t : dx > 0 ? Section.Mask : 0, az = dz == 0 ? t : dz > 0 ? Section.Mask : 0;
                    int bx = dx == 0 ? t : dx > 0 ? 0 : Section.Mask, bz = dz == 0 ? t : dz > 0 ? 0 : Section.Mask;
                    int a = Section.Index(ax, y & Section.Mask, az), b = Section.Index(bx, y & Section.Mask, bz);
                    int wax = column.X * Section.Size + ax, waz = column.Z * Section.Size + az;
                    int wbx = other.X * Section.Size + bx, wbz = other.Z * Section.Size + bz;
                    foreach (var sky in (ReadOnlySpan<bool>)[true, false])
                    {
                        int la = mine.Level(a, sky), lb = theirs.Level(b, sky);
                        var queue = sky ? _sky : _block;
                        if (la - lb >= 2) queue.Enqueue((wax, y, waz));
                        else if (lb - la >= 2) queue.Enqueue((wbx, y, wbz));
                    }
                }
            }
        }
        Spread(_sky, sky: true);
        Spread(_block, sky: false);
    }

    /// <summary>
    /// Takes back and spreads again the light around a block that has changed to <paramref name="now"/>:
    /// the levels at the block and every level that came from them are taken away, and the light
    /// around spreads back in as the new block lets it, which covers every change between air, glass,
    /// water and an opaque block alike.
    /// </summary>
    public void Changed(int x, int y, int z, BlockId now)
    {
        if (!TryCell(x, y, z, out var section, out var index)) return;
        var opaque = Blocks.IsOpaque(now) && !Blocks.IsLeaves(now);

        var held = section.BlockLight(index);
        if (held > 0)
        {
            Set(x, y, z, section, index, false, 0);
            _taken.Enqueue((x, y, z, held));
            Take(_block, sky: false);
        }
        if (Blocks.Get(now).Light is var gives and > 0)
        {
            Set(x, y, z, section, index, false, gives);
            _block.Enqueue((x, y, z));
        }
        if (!opaque) Around(x, y, z, _block);
        Spread(_block, sky: false);

        var sky = section.SkyLight(index);
        if (sky > 0)
        {
            Set(x, y, z, section, index, true, 0);
            _taken.Enqueue((x, y, z, sky));
            Take(_sky, sky: true);
        }
        if (!opaque)
        {
            // The open sky above the world's top shines straight in.
            if (y == ChunkColumn.Ceiling - 1) Set(x, y, z, section, index, true, Lighting.Into(Lighting.Max, Lighting.Down, true, now));
            Around(x, y, z, _sky);
        }
        Spread(_sky, sky: true);
    }

    // The neighbors of a cell, whose light spreads into it from wherever they hold some.
    private void Around(int x, int y, int z, Queue<(int X, int Y, int Z)> queue)
    {
        queue.Enqueue((x, y, z));
        for (int d = 0; d < 6; d++) queue.Enqueue((x + Lighting.Dx[d], y + Lighting.Dy[d], z + Lighting.Dz[d]));
    }

    // Takes away every level that came from the light in the taken queue, and leaves the brighter
    // levels at the edge of what was taken, which came from elsewhere, to spread back in.
    private void Take(Queue<(int X, int Y, int Z)> spread, bool sky)
    {
        while (_taken.TryDequeue(out var cell))
        {
            for (int d = 0; d < 6; d++)
            {
                int x = cell.X + Lighting.Dx[d], y = cell.Y + Lighting.Dy[d], z = cell.Z + Lighting.Dz[d];
                if (!TryCell(x, y, z, out var section, out var index)) continue;
                var level = section.Level(index, sky);
                if (level == 0) continue;
                // An emissive block keeps its own level. One that lets light through, as a torch
                // does, may hold more from a brighter light beside it, which is taken back to its own.
                var own = sky ? 0 : Blocks.Get((BlockId)section.Blocks[index]).Light;
                var came = level < cell.Level || sky && d == Lighting.Down && cell.Level == Lighting.Max && level == Lighting.Max;
                if (came && level > own)
                {
                    Set(x, y, z, section, index, sky, own);
                    _taken.Enqueue((x, y, z, level));
                    if (own > 0) spread.Enqueue((x, y, z));
                }
                else spread.Enqueue((x, y, z));
            }
        }
    }

    private void Spread(Queue<(int X, int Y, int Z)> queue, bool sky)
    {
        while (queue.TryDequeue(out var cell))
        {
            if (!TryCell(cell.X, cell.Y, cell.Z, out var from, out var at)) continue;
            var level = from.Level(at, sky);
            if (level <= 1) continue;
            for (int d = 0; d < 6; d++)
            {
                int x = cell.X + Lighting.Dx[d], y = cell.Y + Lighting.Dy[d], z = cell.Z + Lighting.Dz[d];
                if (!TryCell(x, y, z, out var section, out var index)) continue;
                var next = Lighting.Into(level, d, sky, (BlockId)section.Blocks[index]);
                if (section.Level(index, sky) >= next) continue;
                Set(x, y, z, section, index, sky, next);
                queue.Enqueue((x, y, z));
            }
        }
    }
}
