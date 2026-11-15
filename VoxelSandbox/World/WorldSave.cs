using System.Collections.Concurrent;
using System.IO.Compression;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// Where the player stood and looked, the heading and pitch in degrees, the hour, the world's kind
/// and seed, the key of each block by its number in the regions of version 1, the mode played in,
/// each slot of the inventory as an item's key and a count, the hotbar's chosen slot, the player's
/// health, food and saturation, the count of days, and whether the weather is held turned or clear,
/// as a save keeps them. A save that names no mode opens in creative mode, one without health and
/// food with them full, one without a day on the first, and one without a held weather following
/// its days.
/// </summary>
public sealed record WorldInfo(string Kind, int Seed, double X, double Y, double Z, float Heading, float Pitch, bool Flying, float Hour, string[] Blocks,
    string Mode = "creative", string[]? Inventory = null, int Selected = 0, int Health = Vitals.Most, int Food = Vitals.Most, float Saturation = 5, int Day = 0, bool? Weather = null)
{
    /// <summary>
    /// Whether the player lands over the spawn as in a new world, since the save held no place the
    /// land holds, which a save never writes.
    /// </summary>
    [JsonIgnore]
    public bool Landing { get; init; }
}

/// <summary>The types a save writes as JSON, read and written by code System.Text.Json's generator makes.</summary>
/// <remarks>
/// A native build turns serializing by reflection off, and there a save serialized by reflection
/// throws, which ends the program the first time it saves or opens a world. A float that is no
/// number or infinite is written by its name, which the writer otherwise refuses, so no value of the
/// game's ends a save, and the reader bounds it.
/// </remarks>
[JsonSourceGenerationOptions(WriteIndented = true, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals)]
[JsonSerializable(typeof(WorldInfo))]
[JsonSerializable(typeof(WorldSave.FurnaceRecord[]))]
[JsonSerializable(typeof(WorldSave.ChestRecord[]))]
internal sealed partial class SaveJson : JsonSerializerContext;

/// <summary>
/// A world's folder on disk, which holds the columns changed from what the generator makes, in
/// region files of 32 by 32 columns, the mobs of every column that has had its mobs placed, the
/// furnaces, and the world's <see cref="WorldInfo"/>.
/// </summary>
/// <remarks>
/// <para>
/// A column the player never changed is not kept, since its seed makes it again the same. A changed
/// column is kept whole, its blocks compressed, and its light worked out again when it loads, as a
/// generated one's is. A region file holds every kept column of its 32 by 32 and is written whole,
/// as every file of the save is, to a file beside it whose bytes are on the disk before it takes
/// the file's name, so a crash of the game or of the machine in the middle of a save leaves the old
/// file or the new one, each whole.
/// </para>
/// <para>
/// A region of version 1 is numbered by the keys the world's info lists. Where those number the
/// blocks as this build does, a region is written in version 1, which needs no list of its own.
/// Where they number them otherwise, the info keeps them as they were read and a region is written
/// in version 2, the key of each block by its number before its columns. A region numbered
/// otherwise than this build has its columns given this build's numbers as it is read, so every
/// column kept and written is in this build's numbers alone.
/// </para>
/// <para>
/// A file of the save that cannot be read is moved aside, under its name and <c>.unread</c>, so
/// the next save writes a new one and the bytes stay, and one that cannot be moved is written by no
/// save of the run.
/// </para>
/// <para>
/// The main thread reads a region the first time a column in it is asked for and stores columns as
/// they unload. The workers that load columns only read the compressed bytes they are handed, so
/// the dictionary of kept columns is the one thing both touch.
/// </para>
/// </remarks>
public sealed class WorldSave
{
    public const int RegionSize = 32;
    private const int Version = 2;
    // The mobs file's places in doubles from its version 2, in floats in its version 1, which is read.
    private const int MobsVersion = 2;
    private static readonly byte[] Magic = "E3DR"u8.ToArray();
    private static readonly byte[] MobsMagic = "E3DM"u8.ToArray();

    private readonly ConcurrentDictionary<(int X, int Z), byte[]> _kept = new();
    private readonly HashSet<(int X, int Z)> _readRegions = [];
    private readonly HashSet<(int X, int Z)> _changedRegions = [];
    // The files that could not be read nor moved aside, which no save of the run writes.
    private readonly HashSet<string> _held = [];
    // The keys the world's info lists, and the numbers they give the regions of version 1 in this
    // build, null where they are the same or the info lists none.
    private string[]? _keys;
    private ushort[]? _renumber;

    public WorldSave(string folder)
    {
        Folder = folder;
        Directory.CreateDirectory(Path.Combine(folder, "regions"));
    }

    /// <summary>The folder every world is saved under, one folder a world.</summary>
    /// <remarks>
    /// The local application data folder is made where it does not exist yet, as on a new account,
    /// since the path of a missing one is empty, which puts saves under the working folder, where
    /// the program's own file, named VoxelSandbox on Linux and macOS, stands in their way.
    /// </remarks>
    public static string Root => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create), "VoxelSandbox", "saves");

    public string Folder { get; }

    public string Name => Path.GetFileName(Folder);

    // A shift floors toward negative columns as a division would not.
    private static (int X, int Z) RegionOf(int x, int z) => (x >> 5, z >> 5);

    private string RegionPath((int X, int Z) region) => Path.Combine(Folder, "regions", $"r.{region.X}.{region.Z}.bin");

    /// <summary>Reads the region holding a column the first time it is asked for, so <see cref="Kept"/> answers for every column in it.</summary>
    public void ReadRegion(int x, int z)
    {
        var region = RegionOf(x, z);
        if (!_readRegions.Add(region)) return;
        var path = RegionPath(region);
        if (!File.Exists(path)) return;
        try
        {
            using var reader = new BinaryReader(File.OpenRead(path));
            var version = reader.ReadBytes(4).AsSpan().SequenceEqual(Magic) ? reader.ReadInt32() : 0;
            if (version is not (1 or Version))
                throw new InvalidDataException("it is not a region of a version this game reads");
            var renumber = version == 1 ? _renumber : Renumbering(ReadKeys(reader));
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int lx = reader.ReadInt16(), lz = reader.ReadInt16(), length = reader.ReadInt32();
                if (length < 0) throw new InvalidDataException("a column of it has no length");
                // A length past what the file has left is taken as the early end it is before an array
                // of it is made.
                if (length > reader.BaseStream.Length - reader.BaseStream.Position) throw new EndOfStreamException("a column of it ends early");
                var bytes = reader.ReadBytes(length);
                _kept[(region.X * RegionSize + lx, region.Z * RegionSize + lz)] = renumber is null ? bytes : Renumbered(bytes, renumber);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or EndOfStreamException)
        {
            // The region's columns are made again by the generator rather than the world not loading,
            // and those read before the fault are written again at the next save.
            TraceLog(LogLevel.Warning, $"The region {path} could not be read, so its columns are generated again and {SetAside(path)}. {ex.Message}");
            _changedRegions.Add(region);
        }
    }

    /// <summary>A kept column's compressed blocks in this build's numbers, or null for one the generator makes. Safe to call from a worker.</summary>
    public byte[]? Kept(int x, int z) => _kept.GetValueOrDefault((x, z));

    /// <summary>
    /// The key of each block by its number in the regions of version 1, which list none of their
    /// own, for a save to write into the world's info: those the info listed as it was read, where
    /// they number the blocks otherwise than this build, and this build's otherwise.
    /// </summary>
    public string[] BlockKeys => _renumber is null ? [.. Blocks.All.Select(b => b.Key)] : _keys!;

    // The number each saved block takes in this build, by its number when saved, from the keys
    // listed with it, so blocks added or reordered since keep what was built. A key this build does
    // not know becomes air. Null where the numbers are the same.
    private static ushort[]? Renumbering(string[] keys)
    {
        var renumber = new ushort[keys.Length];
        var same = keys.Length <= Blocks.All.Count;
        for (int i = 0; i < keys.Length; i++)
        {
            var known = Blocks.All.FirstOrDefault(b => b.Key == keys[i]);
            renumber[i] = (ushort)(known?.Id ?? BlockId.Air);
            same &= renumber[i] == i;
        }
        return same ? null : renumber;
    }

    // The key of each block by its number, as a region lists them.
    private static string[] ReadKeys(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        if (count is < 0 or > ushort.MaxValue + 1) throw new InvalidDataException("its list of blocks has no length");
        var keys = new string[count];
        for (int i = 0; i < count; i++) keys[i] = reader.ReadString();
        return keys;
    }

    // A kept column's compressed blocks given this build's numbers from those of another.
    private static byte[] Renumbered(byte[] bytes, ushort[] renumber)
    {
        using var output = new MemoryStream();
        using (var input = new ZLibStream(new MemoryStream(bytes), CompressionMode.Decompress))
        using (var zip = new ZLibStream(output, CompressionLevel.Fastest))
        {
            var raw = new byte[Section.Volume * sizeof(ushort)];
            while (input.ReadAtLeast(raw, raw.Length, throwOnEndOfStream: false) == raw.Length)
            {
                var blocks = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, ushort>(raw.AsSpan());
                for (int i = 0; i < blocks.Length; i++) blocks[i] = blocks[i] < renumber.Length ? renumber[blocks[i]] : (ushort)0;
                zip.Write(raw);
            }
        }
        return output.ToArray();
    }

    // Moves a file that could not be read aside, under its name and .unread, or .unread and a number
    // where an earlier one lies, or holds it where it cannot be moved, as while another program has
    // it open, and gives what became of it for the log.
    private string SetAside(string path)
    {
        var aside = path + ".unread";
        for (int n = 2; File.Exists(aside); n++) aside = $"{path}.unread{n}";
        try
        {
            File.Move(path, aside);
            return $"the file is kept as {Path.GetFileName(aside)}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _held.Add(path);
            return "the file, which could not be moved aside, is written by no save of this run";
        }
    }

    // Writes a file whole beside its name and moves it over the old one once its bytes are on the
    // disk, but for a file held since it could not be read.
    private void Replace(string path, Action<Stream> write)
    {
        if (_held.Contains(path)) return;
        var written = path + ".new";
        using (var file = new FileStream(written, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            write(file);
            file.Flush(flushToDisk: true);
        }
        File.Move(written, path, overwrite: true);
    }

    /// <summary>
    /// Leaves out a kept column whose blocks could not be read, so it is generated again, and moves
    /// its region's file aside, the region's other columns written again at the next
    /// <see cref="Flush"/>.
    /// </summary>
    public void Unreadable(int x, int z)
    {
        _kept.TryRemove((x, z), out _);
        var region = RegionOf(x, z);
        var path = RegionPath(region);
        var aside = File.Exists(path) ? $" and {SetAside(path)}" : "";
        TraceLog(LogLevel.Warning, $"The column {x}, {z} of the region {path} could not be read, so it is generated again{aside}.");
        _changedRegions.Add(region);
    }

    /// <summary>Keeps a changed column, to be written with its region at the next <see cref="Flush"/>.</summary>
    public void Keep(ChunkColumn column)
    {
        ReadRegion(column.X, column.Z);
        _kept[(column.X, column.Z)] = Encode(column);
        _changedRegions.Add(RegionOf(column.X, column.Z));
        column.Changed = false;
    }

    /// <summary>Writes every region a column was kept in since the last flush, with the key of each block by its number where the world's info numbers them otherwise.</summary>
    public void Flush()
    {
        foreach (var region in _changedRegions)
        {
            var columns = _kept.Where(c => RegionOf(c.Key.X, c.Key.Z) == region).ToList();
            Replace(RegionPath(region), file =>
            {
                using var writer = new BinaryWriter(file, System.Text.Encoding.UTF8, leaveOpen: true);
                writer.Write(Magic);
                writer.Write(_renumber is null ? 1 : Version);
                if (_renumber is not null)
                {
                    writer.Write(Blocks.All.Count);
                    foreach (var block in Blocks.All) writer.Write(block.Key);
                }
                writer.Write(columns.Count);
                foreach (var ((x, z), bytes) in columns)
                {
                    writer.Write((short)(x - region.X * RegionSize));
                    writer.Write((short)(z - region.Z * RegionSize));
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                }
            });
        }
        _changedRegions.Clear();
    }

    /// <summary>
    /// The worlds saved under <see cref="Root"/>, or under the folder given, the one played last first,
    /// each by its folder's name with its kind and seed, a folder whose info cannot be read left out.
    /// </summary>
    public static IReadOnlyList<(string Name, string Kind, int Seed)> Saved(string? root = null)
    {
        root ??= Root;
        if (!Directory.Exists(root)) return [];
        var found = new List<(string Name, string Kind, int Seed, DateTime Played)>();
        foreach (var folder in Directory.GetDirectories(root))
        {
            var path = Path.Combine(folder, "world.json");
            if (!File.Exists(path)) continue;
            try
            {
                if (JsonSerializer.Deserialize(File.ReadAllText(path), SaveJson.Default.WorldInfo) is { } info)
                    found.Add((Path.GetFileName(folder), Bounded(info).Kind, info.Seed, File.GetLastWriteTimeUtc(path)));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                // A world whose info cannot be read is left out, as opening it would begin it anew.
            }
        }
        return [.. found.OrderByDescending(w => w.Played).Select(w => (w.Name, w.Kind, w.Seed))];
    }

    /// <summary>The world's info, or null for a folder never saved into.</summary>
    public WorldInfo? ReadInfo()
    {
        var path = Path.Combine(Folder, "world.json");
        if (!File.Exists(path)) return null;
        try
        {
            var read = JsonSerializer.Deserialize(File.ReadAllText(path), SaveJson.Default.WorldInfo);
            if (read is null) return null;
            var info = Bounded(read);
            if (info != read)
                TraceLog(LogLevel.Warning, $"{path} holds a kind, a place, a heading, a pitch or an hour the game cannot hold, so those open as a new world's and the rest as saved.");
            if (info.Blocks is { } keys) (_keys, _renumber) = (keys, Renumbering(keys));
            return info;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            TraceLog(LogLevel.Warning, $"{path} could not be read, so the world begins as a new one of its name and {SetAside(path)}. {ex.Message}");
            return null;
        }
    }

    // A world's info with each value the game cannot hold taken as a new world's, so a file changed by
    // hand opens with the rest of what it keeps. A kind named by none is one this build makes none
    // of, which opens as the kind asked for. A place that is no number or lies past the land's edge
    // lands the player over the spawn. A heading, a pitch or an hour that is no number is a new
    // world's.
    private static WorldInfo Bounded(WorldInfo info)
    {
        if (info.Kind is null) info = info with { Kind = "" };
        if (!new LandPlace(info.X, info.Y, info.Z).InLand) info = info with { X = 0, Y = 0, Z = 0, Landing = true };
        if (!float.IsFinite(info.Heading)) info = info with { Heading = 0 };
        if (!float.IsFinite(info.Pitch)) info = info with { Pitch = 0 };
        if (!float.IsFinite(info.Hour)) info = info with { Hour = Sky.FirstHour };
        return info;
    }

    public void WriteInfo(WorldInfo info) =>
        Replace(Path.Combine(Folder, "world.json"), file => JsonSerializer.Serialize(file, info, SaveJson.Default.WorldInfo));

    /// <summary>
    /// The mobs kept by each column that has had its mobs placed, read from the folder's mobs file,
    /// with the columns that have, or none of either for a folder never saved into.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A mob is saved by its kind's name, which the file lists once, so kinds added or reordered
    /// since keep what was saved, and a mob of a kind this build does not know is left out. Its place and its
    /// home are the land's own, in doubles, where a file of version 1 holds them in floats, which a
    /// million blocks out keep them only to an eighth of a block.
    /// </para>
    /// <para>
    /// A mob saved with a place that is no number is set down at its home where it has one that is a
    /// number, a villager's, in its home's column, and otherwise left out, and the log says how many
    /// of each. Its place cannot be had again, since it is no number and the mob is kept by the
    /// column x 0 that no number turns into, not by its own.
    /// </para>
    /// </remarks>
    public (Dictionary<(int X, int Z), List<MobRecord>> Kept, HashSet<(int X, int Z)> Placed) ReadMobs()
    {
        var kept = new Dictionary<(int X, int Z), List<MobRecord>>();
        var placed = new HashSet<(int X, int Z)>();
        var (homed, lost) = (new List<MobRecord>(), 0);
        var path = Path.Combine(Folder, "mobs.bin");
        if (!File.Exists(path)) return (kept, placed);
        try
        {
            using var reader = new BinaryReader(new ZLibStream(File.OpenRead(path), CompressionMode.Decompress));
            var version = reader.ReadBytes(4).AsSpan().SequenceEqual(MobsMagic) ? reader.ReadInt32() : 0;
            if (version is not (1 or MobsVersion))
                throw new InvalidDataException("it is not a mobs file of a version this game reads");
            LandPlace Place() => version == 1 ? new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()) : new(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
            var listed = reader.ReadInt32();
            // A record names its kind by a byte, so no file the game writes lists more kinds than a
            // byte tells apart.
            if (listed is < 0 or > 256) throw new InvalidDataException("its list of kinds has no length");
            var kinds = new MobKind?[listed];
            for (int i = 0; i < kinds.Length; i++) kinds[i] = MobKinds.Find(reader.ReadString());
            var columns = reader.ReadInt32();
            for (int c = 0; c < columns; c++)
            {
                var key = (reader.ReadInt32(), reader.ReadInt32());
                placed.Add(key);
                var count = reader.ReadUInt16();
                var list = new List<MobRecord>(count);
                for (int m = 0; m < count; m++)
                {
                    var number = reader.ReadByte();
                    var kind = number < kinds.Length ? kinds[number] : null;
                    var variant = reader.ReadByte();
                    var at = Place();
                    var yaw = reader.ReadSingle();
                    var health = reader.ReadByte();
                    LandPlace? home = reader.ReadBoolean() ? Place() : null;
                    if (kind is null) continue;
                    var record = new MobRecord(kind, (byte)(variant % kind.Variants.Length), at, yaw, Math.Clamp((int)health, 1, kind.Health), home);
                    if (at.Finite) list.Add(record);
                    else if (home is { Finite: true } stand) homed.Add(record with { Position = stand });
                    else lost++;
                }
                if (list.Count > 0) kept[key] = list;
            }
            foreach (var record in homed)
            {
                var column = (VoxelWorld.ColumnOf((int)Math.Floor(record.Position.X)), VoxelWorld.ColumnOf((int)Math.Floor(record.Position.Z)));
                if (kept.TryGetValue(column, out var list)) list.Add(record);
                else kept[column] = [record];
            }
            if (homed.Count + lost > 0)
                TraceLog(LogLevel.Warning, $"{path} kept {homed.Count + lost} mobs whose place was no number, {homed.Count} set down at their homes and {lost} left out.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or EndOfStreamException)
        {
            // The columns' mobs are placed again by their generator rather than the world not loading.
            TraceLog(LogLevel.Warning, $"{path} could not be read, so the columns' mobs are placed again and {SetAside(path)}. {ex.Message}");
            kept.Clear();
            placed.Clear();
        }
        return (kept, placed);
    }

    /// <summary>Writes the mobs of every column that has had its mobs placed, those with none among them, to a file beside the old one that then takes its name.</summary>
    public void WriteMobs(IEnumerable<((int X, int Z) Column, IReadOnlyList<MobRecord> Mobs)> columns)
    {
        var list = columns.ToList();
        Replace(Path.Combine(Folder, "mobs.bin"), file =>
        {
            using var writer = new BinaryWriter(new ZLibStream(file, CompressionLevel.Fastest, leaveOpen: true));
            writer.Write(MobsMagic);
            writer.Write(MobsVersion);
            writer.Write(MobKinds.All.Length);
            foreach (var kind in MobKinds.All) writer.Write(kind.Name);
            writer.Write(list.Count);
            foreach (var ((x, z), mobs) in list)
            {
                writer.Write(x);
                writer.Write(z);
                writer.Write((ushort)mobs.Count);
                foreach (var mob in mobs)
                {
                    writer.Write(mob.Kind.Id);
                    writer.Write(mob.Variant);
                    writer.Write(mob.Position.X);
                    writer.Write(mob.Position.Y);
                    writer.Write(mob.Position.Z);
                    writer.Write(mob.Yaw);
                    writer.Write((byte)Math.Clamp(mob.Health, 0, 255));
                    writer.Write(mob.Home is not null);
                    if (mob.Home is { } home)
                    {
                        writer.Write(home.X);
                        writer.Write(home.Y);
                        writer.Write(home.Z);
                    }
                }
            }
        });
    }

    /// <summary>A furnace as a save keeps it, with its place, its three slots as <see cref="Inventory.Encode"/> gives them, and its fire and smelting.</summary>
    public sealed record FurnaceRecord(int X, int Y, int Z, string[] Slots, float Burning, float BurnTotal, float Cooked);

    /// <summary>The world's furnaces, read from its <c>furnaces.json</c>, none for a folder never saved into.</summary>
    public Furnaces ReadFurnaces()
    {
        var furnaces = new Furnaces();
        var path = Path.Combine(Folder, "furnaces.json");
        if (!File.Exists(path)) return furnaces;
        // A burn or a smelt that is no number or below none, as a file changed by hand may hold, is
        // none, and a burn past its fuel's whole takes itself as the whole, so the furnace's flame and
        // arrow stay within their pictures.
        static float Held(float value) => float.IsFinite(value) && value > 0 ? value : 0;
        try
        {
            foreach (var r in JsonSerializer.Deserialize(File.ReadAllText(path), SaveJson.Default.FurnaceRecordArray) ?? [])
            {
                var (burning, cooked) = (Held(r.Burning), Held(r.Cooked));
                var furnace = new Furnace { Burning = burning, BurnTotal = MathF.Max(Held(r.BurnTotal), burning), Cooked = cooked };
                // A furnace listed with no slots, as a file changed by hand may list one, stands empty.
                var slots = r.Slots ?? [];
                for (int i = 0; i < Math.Min(3, slots.Length); i++) furnace.Slots[i] = Inventory.Decode(slots[i]);
                furnaces.Add((r.X, r.Y, r.Z), furnace);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            TraceLog(LogLevel.Warning, $"{path} could not be read, so the world's furnaces begin empty and {SetAside(path)}. {ex.Message}");
        }
        return furnaces;
    }

    public void WriteFurnaces(Furnaces furnaces)
    {
        var records = furnaces.All.Select(f => new FurnaceRecord(f.Key.X, f.Key.Y, f.Key.Z, [.. f.Value.Slots.Select(Inventory.Encode)], f.Value.Burning, f.Value.BurnTotal, f.Value.Cooked)).ToArray();
        Replace(Path.Combine(Folder, "furnaces.json"), file => JsonSerializer.Serialize(file, records, SaveJson.Default.FurnaceRecordArray));
    }

    /// <summary>A chest as a save keeps it, with its place, its slots as <see cref="Inventory.Encode"/> gives them, and the loot it holds not yet rolled.</summary>
    public sealed record ChestRecord(int X, int Y, int Z, string[] Slots, string? Loot, uint Seed);

    /// <summary>The world's chests, read from its <c>chests.json</c>, none for a folder never saved into.</summary>
    public Chests ReadChests()
    {
        var chests = new Chests();
        var path = Path.Combine(Folder, "chests.json");
        if (!File.Exists(path)) return chests;
        try
        {
            foreach (var r in JsonSerializer.Deserialize(File.ReadAllText(path), SaveJson.Default.ChestRecordArray) ?? [])
            {
                var chest = new Chest { Loot = r.Loot, Seed = r.Seed };
                var slots = r.Slots ?? [];
                for (int i = 0; i < Math.Min(Chest.Size, slots.Length); i++) chest.Slots[i] = Inventory.Decode(slots[i]);
                chests.Add((r.X, r.Y, r.Z), chest);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            TraceLog(LogLevel.Warning, $"{path} could not be read, so the world's chests begin empty and {SetAside(path)}. {ex.Message}");
        }
        return chests;
    }

    public void WriteChests(Chests chests)
    {
        var records = chests.All.Select(c => new ChestRecord(c.Key.X, c.Key.Y, c.Key.Z, [.. c.Value.Slots.Select(Inventory.Encode)], c.Value.Loot, c.Value.Seed)).ToArray();
        Replace(Path.Combine(Folder, "chests.json"), file => JsonSerializer.Serialize(file, records, SaveJson.Default.ChestRecordArray));
    }

    // A column's blocks, every section's in turn, compressed. Its light is left out, since it is
    // worked out again when the column loads.
    private static byte[] Encode(ChunkColumn column)
    {
        using var output = new MemoryStream();
        using (var zip = new ZLibStream(output, CompressionLevel.Fastest))
            foreach (var section in column.Sections)
                zip.Write(System.Runtime.InteropServices.MemoryMarshal.AsBytes(section.Blocks.AsSpan()));
        return output.ToArray();
    }

    /// <summary>Makes a kept column again from its compressed blocks, in this build's numbers. Safe to call from a worker.</summary>
    /// <remarks>
    /// A column kept while the world stood from 0 to 128, eight sections, is laid at its heights over
    /// the deep stone of <paramref name="generated"/>, the column as the generator makes it, its floor
    /// of bedrock at 0 taken out for the generated blocks, and is kept again whole when it next
    /// unloads. Where no generated column is given, such a column's deep stone is air.
    /// </remarks>
    public static ChunkColumn Decode(int x, int z, byte[] bytes, ChunkColumn? generated = null)
    {
        var column = new ChunkColumn(x, z);
        using var zip = new ZLibStream(new MemoryStream(bytes), CompressionMode.Decompress);
        var raw = new byte[Section.Volume * sizeof(ushort)];
        var sections = new List<ushort[]>();
        while (zip.ReadAtLeast(raw, raw.Length, throwOnEndOfStream: false) == raw.Length)
            sections.Add(System.Runtime.InteropServices.MemoryMarshal.Cast<byte, ushort>(raw).ToArray());
        var shallow = sections.Count == OldSections;
        var first = shallow ? -ChunkColumn.BottomSection : 0;
        for (int s = 0; s < sections.Count && first + s < ChunkColumn.SectionCount; s++)
        {
            var (section, blocks) = (column.Sections[first + s], sections[s]);
            for (int i = 0; i < Section.Volume; i++)
            {
                var block = blocks[i];
                if (block != 0) section.Set(i & Section.Mask, i >> 8, (i >> 4) & Section.Mask, (BlockId)block);
            }
        }
        column.Changed = false;
        if (!shallow || generated is null) return column;
        for (int s = 0; s < first; s++)
            for (int i = 0; i < Section.Volume; i++)
                column.Sections[s].Set(i & Section.Mask, i >> 8, (i >> 4) & Section.Mask, (BlockId)generated.Sections[s].Blocks[i]);
        for (int dz = 0; dz < Section.Size; dz++)
            for (int dx = 0; dx < Section.Size; dx++)
                if (column.Get(dx, 0, dz) == BlockId.Bedrock) column.Set(dx, 0, dz, generated.Get(dx, 0, dz));
        column.Changed = true;
        return column;
    }

    // How many sections a column was kept with while the world stood from 0 to 128.
    private const int OldSections = 8;

    /// <summary>Whether a kept column's bytes hold the eight sections of a world that stood from 0 to 128, which its deep stone is generated under.</summary>
    public static bool Shallow(byte[] bytes)
    {
        using var zip = new ZLibStream(new MemoryStream(bytes), CompressionMode.Decompress);
        var raw = new byte[Section.Volume * sizeof(ushort)];
        var sections = 0;
        while (zip.ReadAtLeast(raw, raw.Length, throwOnEndOfStream: false) == raw.Length) sections++;
        return sections == OldSections;
    }
}
