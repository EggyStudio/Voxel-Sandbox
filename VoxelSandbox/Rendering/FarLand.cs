using System.Collections.Concurrent;
using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The land past the render distance out to <see cref="Distance"/> columns, drawn coarser by
/// distance in rings, each twice as far out as the one before and of cells twice as wide, 2 blocks
/// across in the first, out to twice the render distance, 4 in the second, out to four times, and 8
/// past that, each cell a box of its size at its own place, so far land keeps its size and steps in
/// cubes of the merged size.
/// </summary>
/// <remarks>
/// <para>
/// A cell's height is the highest of the heights its places' surfaces hold, rounded down to its
/// width, and its color the average of its highest block's texture under its biome's tint, from
/// the surface the player saw there, which <see cref="FarCache"/> keeps, so far land shows what
/// stood there when it was last seen. Its top is a face, merged with its neighbors' of the same
/// height and color, and it stands on walls down to each lower neighbor, or a skirt of two cells
/// where the neighbor is drawn by another ring or the near land.
/// </para>
/// <para>
/// The land is tiled in squares of 64 cells, and a ring's tiles draw only the columns within the
/// ring, so the rings and the near land meet with no gap and no overlap. A tile is sampled and
/// meshed on a worker and uploaded on the main thread, and meshed again as the eye's column moves
/// a ring's edge over it, and sampled again as a column in it is seen anew. Far land is lit flat
/// by the terrain shader with no ambient occlusion and casts no shadow, so neither the shadows nor
/// the scene's distance field hold it, and the haze covers its edge.
/// </para>
/// </remarks>
public sealed class FarLand : IDisposable
{
    /// <summary>How many rings of coarser cells there are, the last reaching to <see cref="Distance"/>.</summary>
    public const int Levels = 3;

    /// <summary>A tile's cells along each side.</summary>
    public const int Cells = 64;

    /// <summary>The cells along each side of a tile's sampled heights and colors, its own and a border of one.</summary>
    public const int Side = Cells + 2;
    // At most so many tiles are sampled and meshed at once.
    private const int Workers = 2;
    /// <summary>The height of a cell whose surface no column gives.</summary>
    public const short Unknown = short.MinValue;

    private sealed class Tile(int level, int x, int z)
    {
        public readonly int Level = level, X = x, Z = z;
        public bool[] Drawn = [];
        public short[]? Heights;
        public Color[]? Colors;
        public ModelMesh Mesh;
        public bool Sample = true, Remesh = true, Busy;

        public int Width => 1 << Level;
        public int Columns => Cells * Width / Section.Size;
        public Vector3 Origin => new(X * Cells * Width, 0, Z * Cells * Width);
    }

    private readonly Dictionary<(int Level, int X, int Z), Tile> _tiles = [];
    private readonly ConcurrentQueue<(Tile Tile, short[] Heights, Color[] Colors, bool[] Drawn, SectionMesher.Faces Faces)> _done = new();
    private (int X, int Z, int Near, int Far)? _ringsFor;
    private int _running;

    /// <summary>
    /// How many columns from the eye far land reaches, none where it is no more than the render
    /// distance, 32 by default, which over seed 1's spawn at 12 columns adds 0.4 ms of the GPU's
    /// work.
    /// </summary>
    public int Distance { get; set; } = 32;

    /// <summary>The tiles drawn last frame and their triangles, for the settings window.</summary>
    public (int Tiles, int Triangles) Drawn { get; private set; }

    /// <summary>
    /// Takes in the tiles the workers finished, works out which tiles each ring needs as the eye's
    /// column moves, and starts sampling and meshing those that need it, nearest first.
    /// </summary>
    public void Update(Vector3 eye, int renderDistance, FarCache seen, TerrainAtlas atlas, IWorldGenerator generator)
    {
        while (_done.TryDequeue(out var result))
        {
            var (tile, heights, colors, drawn, faces) = result;
            Interlocked.Decrement(ref _running);
            tile.Busy = false;
            if (!_tiles.ContainsKey((tile.Level, tile.X, tile.Z))) continue;
            (tile.Heights, tile.Colors) = (heights, colors);
            // A tile whose rings moved again while it was meshed is meshed again for them.
            if (!drawn.AsSpan().SequenceEqual(tile.Drawn)) tile.Remesh = true;
            if (tile.Mesh.IsValid) UnloadMesh(tile.Mesh);
            tile.Mesh = faces.Count > 0
                ? UploadMesh([.. faces.Positions], [.. faces.Indices], MeshStream.From(MeshStreamFormat.UInt2, faces.Packed.ToArray().AsSpan()), [.. faces.Colors])
                : default;
        }

        if (Distance <= renderDistance)
        {
            Clear();
            return;
        }
        while (seen.Recorded.TryDequeue(out var column))
            foreach (var tile in _tiles.Values)
            {
                var span = tile.Columns;
                if (column.X >= tile.X * span && column.X < (tile.X + 1) * span && column.Z >= tile.Z * span && column.Z < (tile.Z + 1) * span)
                    tile.Sample = true;
            }

        int ex = (int)MathF.Floor(eye.X) >> Section.Shift, ez = (int)MathF.Floor(eye.Z) >> Section.Shift;
        if (_ringsFor != (ex, ez, renderDistance, Distance))
        {
            _ringsFor = (ex, ez, renderDistance, Distance);
            Ring(ex, ez, renderDistance);
        }

        foreach (var tile in _tiles.Values.Where(t => (t.Sample || t.Remesh) && !t.Busy)
                     .OrderBy(t => Vector2.DistanceSquared(new Vector2(t.Origin.X, t.Origin.Z) + new Vector2(t.Columns * Section.Size / 2f), new Vector2(eye.X, eye.Z))))
        {
            if (_running >= Workers) break;
            (tile.Busy, tile.Remesh) = (true, false);
            var (sample, drawn, heights, colors) = (tile.Sample, tile.Drawn, tile.Heights, tile.Colors);
            tile.Sample = false;
            var (inner, outer) = Bounds(tile.Level, renderDistance, Distance);
            Interlocked.Increment(ref _running);
            Task.Run(() =>
            {
                try
                {
                    if (sample || heights is null || colors is null) (heights, colors) = Sample(tile.Level, tile.X, tile.Z, seen, atlas, generator);
                    _done.Enqueue((tile, heights, colors, drawn, Mesh(tile.Level, tile.X, tile.Z, heights, colors, ex, ez, inner, outer)));
                }
                catch (Exception error)
                {
                    Console.Error.WriteLine($"Far land at level {tile.Level}, tile {tile.X}, {tile.Z} could not be made. {error}");
                    _done.Enqueue((tile, heights ?? [], colors ?? [], drawn, new SectionMesher.Faces()));
                }
            });
        }
    }

    /// <summary>Draws every tile with a mesh through the far land's material.</summary>
    public void Draw(ModelMaterial material)
    {
        int tiles = 0, triangles = 0;
        foreach (var tile in _tiles.Values)
        {
            if (!tile.Mesh.IsValid) continue;
            DrawMesh(tile.Mesh, material, Matrix4x4.CreateTranslation(tile.Origin));
            tiles++;
            triangles += tile.Mesh.TriangleCount;
        }
        Drawn = (tiles, triangles);
    }

    /// <summary>Lets go of every tile, as a world closes or far land is turned off.</summary>
    public void Clear()
    {
        foreach (var tile in _tiles.Values)
            if (tile.Mesh.IsValid) UnloadMesh(tile.Mesh);
        _tiles.Clear();
        _ringsFor = null;
        Drawn = default;
    }

    public void Dispose() => Clear();

    /// <summary>
    /// The columns from the eye's the ring of a level lies between, past the first and within the
    /// second, the last ring reaching to the far distance, and none past it.
    /// </summary>
    public static (int Inner, int Outer) Bounds(int level, int renderDistance, int distance) =>
        (renderDistance << (level - 1), level == Levels ? distance : Math.Min(distance, renderDistance << level));

    /// <summary>Whether a column so far from the eye's lies past one distance and within another, by the circle the streamer loads columns in.</summary>
    public static bool Within(int dx, int dz, int inner, int outer)
    {
        var d = dx * dx + dz * dz;
        return d > inner * inner + inner && d <= outer * outer + outer;
    }

    // The tiles each ring needs around the eye's column and which of their columns each draws.
    private void Ring(int ex, int ez, int renderDistance)
    {
        var wanted = new HashSet<(int, int, int)>();
        for (int level = 1; level <= Levels; level++)
        {
            var (inner, outer) = Bounds(level, renderDistance, Distance);
            if (inner >= outer) continue;
            var span = Cells * (1 << level) / Section.Size;
            for (int tz = (int)Math.Floor((ez - outer) / (double)span); tz <= (int)Math.Floor((ez + outer) / (double)span); tz++)
                for (int tx = (int)Math.Floor((ex - outer) / (double)span); tx <= (int)Math.Floor((ex + outer) / (double)span); tx++)
                {
                    var drawn = new bool[span * span];
                    var any = false;
                    for (int z = 0; z < span; z++)
                        for (int x = 0; x < span; x++)
                            any |= drawn[z * span + x] = Within(tx * span + x - ex, tz * span + z - ez, inner, outer);
                    if (!any) continue;
                    wanted.Add((level, tx, tz));
                    if (!_tiles.TryGetValue((level, tx, tz), out var tile)) _tiles[(level, tx, tz)] = tile = new Tile(level, tx, tz);
                    if (drawn.AsSpan().SequenceEqual(tile.Drawn)) continue;
                    (tile.Drawn, tile.Remesh) = (drawn, true);
                }
        }
        foreach (var key in _tiles.Keys.Where(k => !wanted.Contains(k)).ToList())
        {
            if (_tiles[key].Mesh.IsValid) UnloadMesh(_tiles[key].Mesh);
            _tiles.Remove(key);
        }
    }

    /// <summary>
    /// The height and color of each cell of a level's tile, with a border of one cell around it,
    /// rows of <see cref="Side"/>, the highest of the heights the seen surfaces hold over the cell's
    /// places, or where no column there was seen the generator's surface at the cell's middle, and
    /// its block's average color under its biome's tint, or <see cref="Unknown"/> where neither
    /// gives one. Safe to call from a worker.
    /// </summary>
    public static (short[] Heights, Color[] Colors) Sample(int level, int tileX, int tileZ, FarCache seen, TerrainAtlas atlas, IWorldGenerator generator)
    {
        var (heights, colors) = (new short[Side * Side], new Color[Side * Side]);
        var width = 1 << level;
        for (int j = 0; j < Side; j++)
            for (int i = 0; i < Side; i++)
            {
                int bx = (tileX * Cells + i - 1) * width, bz = (tileZ * Cells + j - 1) * width;
                var surface = seen.Get(bx >> Section.Shift, bz >> Section.Shift);
                heights[j * Side + i] = Unknown;
                if (surface is null)
                {
                    var (height, block, id) = generator.Surface(bx + width / 2, bz + width / 2);
                    if (height < ChunkColumn.Bottom) continue;
                    var land = Biomes.All[id];
                    heights[j * Side + i] = (short)height;
                    colors[j * Side + i] = atlas.Paint(Blocks.Get(block).Top, land.GrassColor, land.FoliageColor).Albedo with { A = 255 };
                    continue;
                }
                int best = -1, highest = int.MinValue;
                for (int z = 0; z < width; z++)
                    for (int x = 0; x < width; x++)
                    {
                        var at = ((bz + z) & Section.Mask) * Section.Size + ((bx + x) & Section.Mask);
                        if (surface.Heights[at] > highest) (highest, best) = (surface.Heights[at], at);
                    }
                if (highest < ChunkColumn.Bottom) continue;
                heights[j * Side + i] = (short)highest;
                var biome = Biomes.All[surface.Biomes[best]];
                colors[j * Side + i] = atlas.Paint(Blocks.Get((BlockId)surface.Tops[best]).Top, biome.GrassColor, biome.FoliageColor).Albedo with { A = 255 };
            }
        return (heights, colors);
    }

    /// <summary>
    /// A level's tile's cells as boxes in the tile's own space, those of the columns its ring draws
    /// around the eye's column, each top merged with its neighbors' of the same height and color
    /// along its row and across the rows, and walls down to each lower neighbor, or a skirt of two
    /// cells beside one the ring does not draw. Safe to call from a worker.
    /// </summary>
    public static SectionMesher.Faces Mesh(int level, int tileX, int tileZ, short[] heights, Color[] colors, int ex, int ez, int inner, int outer)
    {
        var faces = new SectionMesher.Faces();
        if (heights.Length == 0) return faces;
        var width = 1 << level;
        // The top of each cell this tile draws, rounded down to its width, or none.
        var tops = new int[Side * Side];
        for (int j = 0; j < Side; j++)
            for (int i = 0; i < Side; i++)
            {
                var h = heights[j * Side + i];
                int cx = (tileX * Cells + i - 1) * width >> Section.Shift, cz = (tileZ * Cells + j - 1) * width >> Section.Shift;
                var ours = Within(cx - ex, cz - ez, inner, outer);
                tops[j * Side + i] = h == Unknown || !ours ? int.MinValue : (int)Math.Floor((h + 1) / (double)width) * width;
            }

        // Tops, merged into rectangles of one height and color.
        var used = new bool[Cells * Cells];
        for (int j = 0; j < Cells; j++)
            for (int i = 0; i < Cells; i++)
            {
                var at = (j + 1) * Side + i + 1;
                if (used[j * Cells + i] || tops[at] == int.MinValue) continue;
                var (top, color) = (tops[at], colors[at]);
                bool Same(int ci, int cj) => !used[cj * Cells + ci] && tops[(cj + 1) * Side + ci + 1] == top && colors[(cj + 1) * Side + ci + 1] == color;
                var right = i;
                while (right + 1 < Cells && Same(right + 1, j)) right++;
                var down = j;
                while (down + 1 < Cells && Enumerable.Range(i, right - i + 1).All(ci => Same(ci, down + 1))) down++;
                for (int cj = j; cj <= down; cj++)
                    for (int ci = i; ci <= right; ci++) used[cj * Cells + ci] = true;
                Box(faces, 2, new Vector3(i * width, top - 1, j * width), new Vector3((right - i + 1) * width, 1, (down - j + 1) * width), color);
            }

        // Walls down to each lower neighbor, or a skirt beside a cell this ring does not draw.
        ReadOnlySpan<(int Dx, int Dz, int Face)> sides = [(1, 0, 0), (-1, 0, 1), (0, 1, 4), (0, -1, 5)];
        for (int j = 0; j < Cells; j++)
            for (int i = 0; i < Cells; i++)
            {
                var at = (j + 1) * Side + i + 1;
                var top = tops[at];
                if (top == int.MinValue) continue;
                foreach (var (dx, dz, face) in sides)
                {
                    var beside = tops[at + dz * Side + dx];
                    var foot = beside == int.MinValue ? top - 2 * width : beside;
                    if (foot >= top) continue;
                    Box(faces, face, new Vector3(i * width, foot, j * width), new Vector3(width, top - foot, width), colors[at]);
                }
            }
        return faces;
    }

    // One face of a box from its lowest corner across a size, in the terrain shader's packing, its
    // color carried whole and laid over the white tile.
    private static void Box(SectionMesher.Faces faces, int face, Vector3 from, Vector3 size, Color color)
    {
        var first = (uint)faces.Count;
        for (int corner = 0; corner < 4; corner++)
            faces.Add(from + SectionMesher.Corner(face, corner) * size, face, corner, 1, color);
        faces.Indices.Add(first);
        faces.Indices.Add(first + 1);
        faces.Indices.Add(first + 2);
        faces.Indices.Add(first);
        faces.Indices.Add(first + 2);
        faces.Indices.Add(first + 3);
    }
}
