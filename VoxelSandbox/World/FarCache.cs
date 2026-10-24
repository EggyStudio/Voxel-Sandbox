using System.Collections.Concurrent;
using System.IO.Compression;

namespace VoxelSandbox;

/// <summary>
/// The surface of every column the player has seen, the height of the highest block at each of its
/// places, that block and the biome, kept as the column unloads, so the far land is drawn from what
/// stood there when it was last seen, as the modpack's cache of far chunks keeps it.
/// </summary>
/// <remarks>
/// <para>
/// A save keeps the surfaces in the folder <c>far</c> beside its regions, in files of 32 by 32
/// columns each written whole when the save flushes, and a world kept nowhere keeps them while it is
/// open. A surface is 1.25 KB as it is held and about a fifth of that written.
/// </para>
/// <para>
/// The main thread records surfaces and flushes them, and the far land's workers read them, a
/// region's file the first time one of its columns is asked for.
/// </para>
/// </remarks>
public sealed class FarCache(string? folder)
{
    /// <summary>A column's surface, each array a value for each of its places, x fastest.</summary>
    public sealed record Surface(short[] Heights, ushort[] Tops, byte[] Biomes);

    private const int RegionShift = 5;
    private readonly ConcurrentDictionary<(int X, int Z), Surface> _surfaces = new();
    private readonly ConcurrentDictionary<(int X, int Z), object> _regions = new();
    private readonly HashSet<(int X, int Z)> _changed = [];
    private readonly ConcurrentQueue<(int X, int Z)> _recorded = new();

    /// <summary>The columns recorded since they were last taken, for the far land to draw again.</summary>
    public ConcurrentQueue<(int X, int Z)> Recorded => _recorded;

    /// <summary>Keeps a column's surface as it stands now.</summary>
    public void Record(ChunkColumn column)
    {
        var place = Section.Size * Section.Size;
        var surface = new Surface(new short[place], new ushort[place], new byte[place]);
        for (int z = 0; z < Section.Size; z++)
            for (int x = 0; x < Section.Size; x++)
            {
                var at = z * Section.Size + x;
                var top = column.Top(x, z);
                surface.Heights[at] = (short)top;
                surface.Tops[at] = (ushort)(top < ChunkColumn.Bottom ? BlockId.Air : column.Get(x, top, z));
                surface.Biomes[at] = column.Biome[at];
            }
        Read(column.X, column.Z);
        _surfaces[(column.X, column.Z)] = surface;
        _changed.Add((column.X >> RegionShift, column.Z >> RegionShift));
        _recorded.Enqueue((column.X, column.Z));
    }

    /// <summary>A column's surface where it has been seen, reading its region's file the first time. Safe to call from a worker.</summary>
    public Surface? Get(int x, int z)
    {
        Read(x, z);
        return _surfaces.GetValueOrDefault((x, z));
    }

    /// <summary>Writes the files of every region a surface was recorded in since the last flush.</summary>
    public void Flush()
    {
        if (folder is null) return;
        Directory.CreateDirectory(folder);
        foreach (var (rx, rz) in _changed)
        {
            var path = PathOf(rx, rz);
            using (var writer = new BinaryWriter(new ZLibStream(File.Create(path + ".new"), CompressionLevel.Fastest)))
            {
                var inRegion = _surfaces.Where(s => s.Key.X >> RegionShift == rx && s.Key.Z >> RegionShift == rz).ToList();
                writer.Write(inRegion.Count);
                foreach (var ((x, z), surface) in inRegion)
                {
                    writer.Write(x);
                    writer.Write(z);
                    foreach (var height in surface.Heights) writer.Write(height);
                    foreach (var top in surface.Tops) writer.Write(top);
                    writer.Write(surface.Biomes);
                }
            }
            File.Move(path + ".new", path, overwrite: true);
        }
        _changed.Clear();
    }

    // Reads the file of a column's region the first time a column of it is asked for, one worker
    // reading a region while others wait on it.
    private void Read(int x, int z)
    {
        if (folder is null) return;
        var region = (x >> RegionShift, z >> RegionShift);
        if (_regions.TryGetValue(region, out var read) && read is bool) return;
        lock (_regions.GetOrAdd(region, _ => new object()))
        {
            if (_regions[region] is bool) return;
            var path = PathOf(region.Item1, region.Item2);
            if (File.Exists(path))
            {
                try
                {
                    using var reader = new BinaryReader(new ZLibStream(File.OpenRead(path), CompressionMode.Decompress));
                    var place = Section.Size * Section.Size;
                    for (int count = reader.ReadInt32(); count > 0; count--)
                    {
                        var (cx, cz) = (reader.ReadInt32(), reader.ReadInt32());
                        var surface = new Surface(new short[place], new ushort[place], new byte[place]);
                        for (int i = 0; i < place; i++) surface.Heights[i] = reader.ReadInt16();
                        for (int i = 0; i < place; i++) surface.Tops[i] = reader.ReadUInt16();
                        reader.ReadExactly(surface.Biomes);
                        _surfaces.TryAdd((cx, cz), surface);
                    }
                }
                catch (Exception ex) when (ex is IOException or InvalidDataException)
                {
                    Console.Error.WriteLine($"{path} could not be read, so its far land is not drawn from what was seen there. {ex.Message}");
                }
            }
            _regions[region] = true;
        }
    }

    private string PathOf(int rx, int rz) => Path.Combine(folder!, $"r.{rx}.{rz}.bin");
}
