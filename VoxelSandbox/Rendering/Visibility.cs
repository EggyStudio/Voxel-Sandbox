namespace VoxelSandbox;

/// <summary>
/// The sections the eye can see through open cells, after Minecraft's visibility graph, in which
/// each section knows which of its six faces its open cells join, and a flood from the eye's
/// section passes from one section into the next only across faces so joined, never turning back
/// the way it came, so the land behind a closed room's walls or under the ground is left out. From
/// the eye's own section it leaves through the faces the open cells around the eye reach, where
/// Minecraft's leaves through all six, so a room closed within one section is drawn alone.
/// </summary>
/// <remarks>
/// <para>
/// A cell is open where its block hides nothing behind it, as air, water, glass, torches, slabs and
/// leaves cut out where a pack's texture is clear do. A section's joins are worked out again the
/// first time they are asked after a block in it changes or under another atlas, whose packs may
/// cut out other blocks. The faces are numbered +x, -x, +y, -y, +z and -z, each the one before
/// its opposite.
/// </para>
/// <para>
/// The flood reaches the sections of the loaded columns within the render distance and the world's
/// heights. An eye above or below the world sees every section, since the sky over the world's top
/// is not a section the flood could pass through.
/// </para>
/// </remarks>
public sealed class Visibility
{
    private static readonly (int X, int Y, int Z)[] Steps = [(1, 0, 0), (-1, 0, 0), (0, 1, 0), (0, -1, 0), (0, 0, 1), (0, 0, -1)];

    private HashSet<SectionKey> _seen = [], _next = [];
    private readonly Queue<(SectionKey Key, int From, int Ways)> _queue = new();

    /// <summary>The sections the last <see cref="Flood"/> reached.</summary>
    public IReadOnlySet<SectionKey> Seen => _seen;

    /// <summary>Whether the last flood left anything out, false where the eye stood outside the world's heights.</summary>
    public bool Culled { get; private set; }

    /// <summary>
    /// Which pairs of a section's faces its open cells join, a bit for each ordered pair, the bit of
    /// faces a and b at a times 6 plus b, under the atlas whose packs cut some blocks out, or none.
    /// </summary>
    public static ulong Joins(Section section, TerrainAtlas? atlas = null)
    {
        atlas ??= TerrainAtlas.None;
        if (section.Visibility.Version == section.Version && section.Visibility.Marks == atlas.Serial) return section.Visibility.Joins;
        var joins = Work(section, atlas);
        section.Visibility = (joins, section.Version, atlas.Serial);
        return joins;
    }

    // The faces of a section that the run of open cells holding a cell reaches, every face where
    // the cell is not open, as for an eye inside a block.
    private static int FacesFrom(Section section, int cell, TerrainAtlas atlas)
    {
        if (atlas.Hides((BlockId)section.Blocks[cell])) return 63;
        var seen = new bool[Section.Volume];
        var stack = new int[Section.Volume];
        int faces = 0, top = 0;
        stack[top++] = cell;
        seen[cell] = true;
        while (top > 0)
        {
            var i = stack[--top];
            int x = i & Section.Mask, y = i >> 8, z = (i >> 4) & Section.Mask;
            faces |= (x == Section.Mask ? 1 : 0) | (x == 0 ? 2 : 0) | (y == Section.Mask ? 4 : 0) | (y == 0 ? 8 : 0) | (z == Section.Mask ? 16 : 0) | (z == 0 ? 32 : 0);
            foreach (var (step, edge) in (ReadOnlySpan<(int, bool)>)[(1, x < Section.Mask), (-1, x > 0), (256, y < Section.Mask), (-256, y > 0), (16, z < Section.Mask), (-16, z > 0)])
            {
                var n = i + step;
                if (!edge || seen[n] || atlas.Hides((BlockId)section.Blocks[n])) continue;
                seen[n] = true;
                stack[top++] = n;
            }
        }
        return faces;
    }

    private static ulong Work(Section section, TerrainAtlas atlas)
    {
        const ulong all = (1UL << 36) - 1;
        if (section.Filled == 0) return all;
        var open = new bool[Section.Volume];
        var any = false;
        for (int i = 0; i < Section.Volume; i++) any |= open[i] = !atlas.Hides((BlockId)section.Blocks[i]);
        if (!any) return 0;

        ulong joins = 0;
        var stack = new int[Section.Volume];
        for (int start = 0; start < Section.Volume; start++)
        {
            if (!open[start]) continue;
            // A flood through this run of open cells, noting the faces it touches, its cells closed as it goes.
            int faces = 0, top = 0;
            stack[top++] = start;
            open[start] = false;
            while (top > 0)
            {
                var i = stack[--top];
                int x = i & Section.Mask, y = i >> 8, z = (i >> 4) & Section.Mask;
                if (x == Section.Mask) faces |= 1;
                if (x == 0) faces |= 2;
                if (y == Section.Mask) faces |= 4;
                if (y == 0) faces |= 8;
                if (z == Section.Mask) faces |= 16;
                if (z == 0) faces |= 32;
                if (x < Section.Mask && open[i + 1]) { open[i + 1] = false; stack[top++] = i + 1; }
                if (x > 0 && open[i - 1]) { open[i - 1] = false; stack[top++] = i - 1; }
                if (y < Section.Mask && open[i + 256]) { open[i + 256] = false; stack[top++] = i + 256; }
                if (y > 0 && open[i - 256]) { open[i - 256] = false; stack[top++] = i - 256; }
                if (z < Section.Mask && open[i + 16]) { open[i + 16] = false; stack[top++] = i + 16; }
                if (z > 0 && open[i - 16]) { open[i - 16] = false; stack[top++] = i - 16; }
            }
            for (int a = 0; a < 6; a++)
                for (int b = 0; b < 6; b++)
                    if ((faces >> a & 1) != 0 && (faces >> b & 1) != 0) joins |= 1UL << (a * 6 + b);
        }
        return joins;
    }

    // What the last flood was worked out for, the eye's section and the faces it leaves by, the
    // world's openings, the render distance and the atlas.
    private (SectionKey Start, int Exits, int Openings, int Distance, int Atlas)? _floodedFor;

    // The faces the eye's section is left by from the eye's cell, and the cell, the section's
    // version and the atlas they were worked out for, the section of a column not loaded having none.
    private ((int X, int Y, int Z) Cell, Section? Home, int Version, int Atlas) _exitsFor;
    private int _exits;

    /// <summary>
    /// The faces of the eye's section the open cells around the eye reach, a bit for each, every
    /// face where no column holds the eye, and none, as -1, where the eye stands above or below the world.
    /// </summary>
    public static int Exits(VoxelWorld world, System.Numerics.Vector3 eye, TerrainAtlas atlas)
    {
        var start = Reach.Of(eye);
        if (start.Y is < ChunkColumn.BottomSection or >= ChunkColumn.CeilingSection) return -1;
        return world.TryGetColumn(start.X, start.Z, out var home)
            ? FacesFrom(home.Numbered(start.Y)!, Section.Index((int)MathF.Floor(eye.X) & Section.Mask, (int)MathF.Floor(eye.Y) & Section.Mask, (int)MathF.Floor(eye.Z) & Section.Mask), atlas)
            : 63;
    }

    // The eye's exits, worked out again only as the eye enters another cell or a block of its
    // section changes, since they ask a flood of the section's cells.
    private int ExitsKept(VoxelWorld world, System.Numerics.Vector3 eye, TerrainAtlas atlas)
    {
        var cell = ((int)MathF.Floor(eye.X), (int)MathF.Floor(eye.Y), (int)MathF.Floor(eye.Z));
        var start = Reach.Of(eye);
        var home = world.TryGetColumn(start.X, start.Z, out var column) ? column.Numbered(start.Y) : null;
        var key = (cell, home, home?.Version ?? 0, atlas.Serial);
        if (key == _exitsFor) return _exits;
        (_exitsFor, _exits) = (key, Exits(world, eye, atlas));
        return _exits;
    }

    // A flood under way, what it was started for, its start, the faces it leaves its start by and
    // the reach it is held to.
    private (SectionKey Start, int Exits, int Openings, int Distance, int Atlas)? _floodingFor;
    private int _reach;

    /// <summary>
    /// Floods from the section holding the eye through the sections of the loaded columns within a
    /// render distance of it, as <see cref="Seen"/> then holds, unless nothing it depends on
    /// changed since the last flood, the eye's section, the faces it leaves that section by, the
    /// world's <see cref="VoxelWorld.Openings"/>, the render distance and the atlas, which keeps
    /// the last flood's sections.
    /// </summary>
    /// <remarks>
    /// Where the eye has stepped into a section beside the last flood's, the flood takes at most
    /// <paramref name="budgetMs"/> a frame and goes on in the frames after, <see cref="Seen"/>
    /// keeping the last whole flood's sections meanwhile, since a flood over the sections of 12
    /// columns took 4 ms with each section's joins kept and up to 32 where it worked them out, all
    /// in the frame the eye crossed. A flood under way runs to its end before another begins, so
    /// the world's openings changing every frame as columns arrive still sees each flood finished.
    /// A jump of more than a section floods at once, as a teleport's first frame does.
    /// </remarks>
    /// <returns>Whether a flood finished in this call.</returns>
    public bool Flood(VoxelWorld world, System.Numerics.Vector3 eye, int renderDistance, TerrainAtlas? atlas = null, double budgetMs = double.PositiveInfinity)
    {
        atlas ??= TerrainAtlas.None;
        if (_floodingFor is null)
        {
            var start = Reach.Of(eye);
            var exits = ExitsKept(world, eye, atlas);
            var wanted = (start, exits, world.Openings, renderDistance, atlas.Serial);
            if (_floodedFor == wanted) return false;
            // A step into a section beside the last flood's start spreads, and any other change floods at once.
            if (_floodedFor is not { } last || Math.Abs(last.Start.X - start.X) > 1 || Math.Abs(last.Start.Y - start.Y) > 1 || Math.Abs(last.Start.Z - start.Z) > 1)
                budgetMs = double.PositiveInfinity;
            _floodingFor = wanted;
            _next.Clear();
            _queue.Clear();
            if (exits < 0)
            {
                Finish(culled: false);
                return true;
            }
            _reach = renderDistance * renderDistance + renderDistance;
            _next.Add(start);
            _queue.Enqueue((start, -1, 0));
        }
        var (from, ways) = (_floodingFor.Value.Start, _floodingFor.Value.Exits);
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var steps = 0;
        while (_queue.TryDequeue(out var at))
        {
            if (!world.TryGetColumn(at.Key.X, at.Key.Z, out var column)) continue;
            var joins = Joins(column.Numbered(at.Key.Y)!, atlas);
            for (int d = 0; d < 6; d++)
            {
                // Never back the way any step of the flood went, and out only through a face the cells
                // joined to the one it came in by.
                if ((at.Ways & 1 << (d ^ 1)) != 0) continue;
                if (at.From >= 0 ? (joins >> (at.From * 6 + d) & 1) == 0 : (ways >> d & 1) == 0) continue;
                var next = new SectionKey(at.Key.X + Steps[d].X, at.Key.Y + Steps[d].Y, at.Key.Z + Steps[d].Z);
                if (next.Y is < ChunkColumn.BottomSection or >= ChunkColumn.CeilingSection || !_next.Add(next)) continue;
                int dx = next.X - from.X, dz = next.Z - from.Z;
                if (dx * dx + dz * dz > _reach || !world.HasColumn(next.X, next.Z)) continue;
                _queue.Enqueue((next, d ^ 1, at.Ways | 1 << d));
            }
            if ((++steps & 15) == 0 && watch.Elapsed.TotalMilliseconds > budgetMs) return false;
        }
        Finish(culled: true);
        return true;
    }

    // Makes the flood under way the last whole one.
    private void Finish(bool culled)
    {
        (_seen, _next) = (_next, _seen);
        _next.Clear();
        Culled = culled;
        _floodedFor = _floodingFor;
        _floodingFor = null;
    }

    /// <summary>Whether the last flood reached a section, every one where it left nothing out.</summary>
    public bool Sees(SectionKey key) => !Culled || _seen.Contains(key);
}
