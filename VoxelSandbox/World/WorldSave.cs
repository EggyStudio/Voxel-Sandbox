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
/// generated one's is. A region file holds every kept column of its 32 by 32.
/// </para>
/// <para>
/// A save is written whole (<see cref="WriteWhole"/>). Each of its files is written under its name
/// and <c>.new</c>, its bytes on the disk before the save goes on, then a list naming them all
/// (<c>save.list</c>), written the same way and moved into its name, and then each file is moved
/// into its own. A world opened with a list standing finishes its moves, so a crash of the game or
/// of the machine opens the world as it was before the save or as the save left it, never a part
/// of each. .NET has no call to flush a folder, so the moves rest on a filesystem that keeps them
/// in their order after the machine stops, as a journaling one does.
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
    // The bytes past which a world's info, and its chests' or furnaces' file, is one that cannot be
    // read rather than read whole, where the game writes an info of some 6 KB, its list of blocks
    // the most of it, and some 700 bytes a chest.
    private const long MostInfoBytes = 16 << 20, MostRecordsBytes = 64 << 20;
    // The version of the regions' chests files, the chests a column of one may hold, past which the
    // file is one that cannot be read, where a column's floor holds 256 a layer and generation
    // places a few, and the longest text a slot or a loot's name is kept as, an item's key with its
    // count and wear.
    private const int ChestsVersion = 1, MostChestsInColumn = 4096, MostChestText = 256;
    private static readonly byte[] ChestsMagic = "E3DC"u8.ToArray();
    // The version of the regions' mobs files, after the 1 and 2 of mobs.bin.
    private const int MobsVersion = 3;
    // The mobs a column of a mobs file may hold, past which the file is one that cannot be read,
    // where generation gives a column a herd of two to four or a villager for each of its houses
    // and the game breeds none.
    private const int MostMobsInColumn = 1024;
    // The columns and the mobs mobs.bin may hold, past which it is one that cannot be read, where a
    // world explored to a million columns holds some three hundred thousand mobs, a herd in a
    // tenth of its columns.
    private const int MostOneFileColumns = 1 << 20, MostOneFileMobs = 1 << 20;
    // The names a save's list may hold and the bytes it may take, past which it is one that cannot
    // be read, where a save names three files for each region about the player and one for each
    // region whose blocks changed since the last, some tens in a flight between saves.
    private const int MostListed = 4096, MostListBytes = 1 << 20;
    private const string ListName = "save.list";
    private static readonly System.Text.RegularExpressions.Regex Listed = new(@"^(world\.json|furnaces\.json|mobs\.bin\.read|chests\.json\.read|regions/[rmc]\.-?[0-9]{1,10}\.-?[0-9]{1,10}\.bin)$");
    private static readonly byte[] Magic = "E3DR"u8.ToArray();
    private static readonly byte[] MobsMagic = "E3DM"u8.ToArray();

    private readonly ConcurrentDictionary<(int X, int Z), byte[]> _kept = new();
    private readonly HashSet<(int X, int Z)> _readRegions = [];
    private readonly HashSet<(int X, int Z)> _changedRegions = [];
    // The blocks of numbers past the game's read as air, by region, and how many of them were said.
    private readonly Dictionary<(int X, int Z), (int Read, int Said)> _unknown = [];
    // The mobs and the chests the save keeps by region, beside the blocks.
    private readonly Regional<MobRecord> _mobs;
    private readonly Regional<ChestRecord> _chests;
    // The files that could not be read nor moved aside, which no save of the run writes.
    private readonly HashSet<string> _held = [];
    // The files a save written whole has written under their scratch names, by the names they are
    // moved into, and what it does once they are, none outside such a save.
    private List<string>? _staged;
    private List<Action>? _afterward;
    // The keys the world's info lists, and the numbers they give the regions of version 1 in this
    // build, null where they are the same or the info lists none.
    private string[]? _keys;
    private ushort[]? _renumber;

    public WorldSave(string folder) : this(folder, null)
    {
    }

    /// <summary>A world's folder whose writes and moves are each told to a test before they are made, which may end the save there.</summary>
    internal WorldSave(string folder, Action<string>? writing)
    {
        Folder = folder;
        Writing = writing;
        Directory.CreateDirectory(Path.Combine(folder, "regions"));
        _mobs = new(this, ReadMobRegion, WriteMobRegion, OneFile, ReadOneFile);
        _chests = new(this, ReadChestRegion, WriteChestRegion);
        if (Finish()) ClearScratch();
    }

    /// <summary>Told the path of each file before it is written under its scratch name or moved into its own, for a test to end a save there.</summary>
    internal Action<string>? Writing { get; set; }

    /// <summary>The folder every world is saved under, one folder a world.</summary>
    /// <remarks>
    /// The local application data folder is made where it does not exist yet, as on a new account,
    /// since the path of a missing one is empty, which puts saves under the working folder, where
    /// the program's own file, named VoxelSandbox on Linux and macOS, stands in their way. The
    /// game's tests set it to a folder of their run's own, so no world a test opens lies among the
    /// saves of the account it runs under.
    /// </remarks>
    public static string Root
    {
        get => _root ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create), "VoxelSandbox", "saves");
        internal set => _root = value;
    }

    private static string? _root;

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
            if (count is < 0 or > RegionSize * RegionSize) throw new InvalidDataException("it counts more columns than a region holds");
            var unread = 0;
            for (int i = 0; i < count; i++)
            {
                int lx = reader.ReadInt16(), lz = reader.ReadInt16(), length = reader.ReadInt32();
                if (lx is < 0 or >= RegionSize || lz is < 0 or >= RegionSize) throw new InvalidDataException("a column of it lies outside it");
                if (length < 0) throw new InvalidDataException("a column of it has no length");
                // A length past what the file has left is taken as the early end it is before an array
                // of it is made.
                if (length > reader.BaseStream.Length - reader.BaseStream.Position) throw new EndOfStreamException("a column of it ends early");
                var bytes = reader.ReadBytes(length);
                var at = (region.X * RegionSize + lx, region.Z * RegionSize + lz);
                if (renumber is null) _kept[at] = bytes;
                // A column whose blocks do not inflate is generated again alone, and the columns
                // after it are read.
                else if (Renumbered(bytes, renumber) is { } renumbered) _kept[at] = renumbered;
                else unread++;
            }
            if (unread > 0)
            {
                TraceLog(LogLevel.Warning, $"{unread} columns of the region {path} could not be read, so they are generated again and {CopyAside(path)}.");
                _changedRegions.Add(region);
            }
        }
        catch (Exception ex) when (SetsAside(ex))
        {
            // The region's columns are made again by the generator rather than the world not loading,
            // and those read before the fault are written again at the next save.
            TraceLog(LogLevel.Warning, $"The region {path} could not be read, so its columns are generated again and {SetAside(path)}. {ex.Message}");
            _changedRegions.Add(region);
        }
        catch (Exception ex) when (Holds(ex))
        {
            TraceLog(LogLevel.Warning, $"The region {path} could not be read, so its columns are generated for this run and {Hold(path)}. {ex.Message}");
        }
    }

    // Whether a read failed on a file read wrong, ended early, holding what no save writes or
    // refused to the account, which is set aside so the save writes the file again.
    private static bool SetsAside(Exception ex) => ex is EndOfStreamException or InvalidDataException or FormatException or UnauthorizedAccessException;

    // Whether a read failed on a fault of the disk or on a file another program holds, which may
    // pass, so the file is left as it stands rather than taken for one read wrong, and a passing
    // fault never puts what the generator makes in place of what was saved.
    private static bool Holds(Exception ex) => ex is IOException and not EndOfStreamException;

    // Leaves a file that could not be read as it stands, written by no save of this run, and gives
    // what became of it for the log, one gone since it was found held by none.
    private string Hold(string path)
    {
        if (!File.Exists(path)) return "the file is gone";
        _held.Add(path);
        return "the file, which no save of this run writes, is left as it stands";
    }

    // Copies a file that could not be read in part aside, under its name and .unread or .unread and
    // a number, the file standing until a save writes it whole, and gives what became of it for the
    // log.
    private static string CopyAside(string path)
    {
        var aside = path + ".unread";
        for (int n = 2; File.Exists(aside); n++) aside = $"{path}.unread{n}";
        try
        {
            File.Copy(path, aside);
            return $"a copy of the file is kept as {Path.GetFileName(aside)}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return $"no copy of the file could be kept, {ex.Message}";
        }
    }

    /// <summary>How many columns' compressed blocks the save holds.</summary>
    public int ColumnsHeld => _kept.Count;

    /// <summary>
    /// Lets go of the blocks held of each region no loaded column lies in and none of whose changes
    /// waits to be written, so the save holds those of the regions about the player however far the
    /// player goes, and reads a region again as one of its columns loads.
    /// </summary>
    public void LetGo(IEnumerable<(int X, int Z)> loaded)
    {
        var standing = loaded.Select(column => RegionOf(column.X, column.Z)).ToHashSet();
        var gone = _readRegions.Where(region => !standing.Contains(region) && !_changedRegions.Contains(region)).ToHashSet();
        foreach (var column in _kept.Keys)
            if (gone.Contains(RegionOf(column.X, column.Z))) _kept.TryRemove(column, out _);
        _readRegions.ExceptWith(gone);
    }

    /// <summary>A kept column's compressed blocks in this build's numbers, or null for one the generator makes. Safe to call from a worker.</summary>
    public byte[]? Kept(int x, int z) => _kept.GetValueOrDefault((x, z));

    /// <summary>
    /// The key of each block by its number in the regions of version 1, which list none of their
    /// own, for a save to write into the world's info, those the info listed as it was read where
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
        var known = new Dictionary<string, BlockId>();
        foreach (var block in Blocks.All) known.TryAdd(block.Key, block.Id);
        for (int i = 0; i < keys.Length; i++)
        {
            renumber[i] = (ushort)(keys[i] is { } key && known.TryGetValue(key, out var id) ? id : BlockId.Air);
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

    // A kept column's compressed blocks given this build's numbers from those of another, or null
    // for blocks that do not inflate.
    private static byte[]? Renumbered(byte[] bytes, ushort[] renumber)
    {
        using var output = new MemoryStream();
        try
        {
            using (var input = new ZLibStream(new MemoryStream(bytes), CompressionMode.Decompress))
            using (var zip = new ZLibStream(output, CompressionLevel.Fastest))
            {
                var raw = new byte[Section.Volume * sizeof(ushort)];
                // A column holds so many sections, so no more is read however far its bytes
                // inflate.
                for (int s = 0; s < ChunkColumn.SectionCount && input.ReadAtLeast(raw, raw.Length, throwOnEndOfStream: false) == raw.Length; s++)
                {
                    var blocks = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, ushort>(raw.AsSpan());
                    for (int i = 0; i < blocks.Length; i++) blocks[i] = blocks[i] < renumber.Length ? renumber[blocks[i]] : (ushort)0;
                    zip.Write(raw);
                }
            }
        }
        catch (InvalidDataException)
        {
            return null;
        }
        return output.ToArray();
    }

    // A file of the save's read whole, one past so many bytes refused as one that cannot be read.
    private static string ReadText(string path, long most)
    {
        var length = new FileInfo(path).Length;
        return length > most ? throw new IOException($"it holds {length >> 20} MB, past the {most >> 20} MB a file of its kind holds") : File.ReadAllText(path);
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
    // disk, or once a save written whole has its list, but for a file held since it could not be
    // read.
    private void Replace(string path, Action<Stream> write)
    {
        if (_held.Contains(path)) return;
        WriteBeside(path, write);
        if (_staged is null) Move(path);
        else if (!_staged.Contains(path)) _staged.Add(path);
    }

    // Writes a file under its name and .new, its bytes on the disk before it is closed.
    private void WriteBeside(string path, Action<Stream> write)
    {
        var written = path + ".new";
        Writing?.Invoke(written);
        using var file = new FileStream(written, FileMode.Create, FileAccess.Write, FileShare.None);
        write(file);
        file.Flush(flushToDisk: true);
    }

    // Moves a file written beside its name into it, or an old one-file save to its name as read,
    // where the file to move still stands, as it does not where a save ended among its moves moved
    // it.
    private void Move(string path)
    {
        var from = path.EndsWith(".read", StringComparison.Ordinal) ? path[..^".read".Length] : path + ".new";
        if (!File.Exists(from)) return;
        Writing?.Invoke(path);
        File.Move(from, path, overwrite: true);
    }

    // Does what a save does once its files are in their places, at once outside a save written
    // whole.
    private void Afterward(Action then)
    {
        if (_afterward is null) then();
        else _afterward.Add(then);
    }

    // Moves an old one-file save to its name as read, once the regions it was read into are
    // written.
    private void Retire(string oneFile)
    {
        if (_staged is null) Move(oneFile + ".read");
        else _staged.Add(oneFile + ".read");
    }

    /// <summary>
    /// Writes a save whole, every file <paramref name="write"/> writes kept beside its name until a
    /// list of them stands, then moved into its name, so the world opens as before the save or as
    /// after it wherever the save ends, the save's regions let go of only once the list stands.
    /// </summary>
    /// <remarks>
    /// A save that throws before its list deletes what it wrote beside the names, and what it had
    /// to write waits for the next. One that throws among its moves leaves its list, which the next
    /// save or the next opening of the world finishes.
    /// </remarks>
    public void WriteWhole(Action write)
    {
        Finish();
        (_staged, _afterward) = ([], []);
        var list = Path.Combine(Folder, ListName);
        var listed = false;
        try
        {
            write();
            WriteBeside(list, file =>
            {
                using var writer = new StreamWriter(file, leaveOpen: true);
                foreach (var path in _staged) writer.Write(Path.GetRelativePath(Folder, path).Replace(Path.DirectorySeparatorChar, '/') + "\n");
            });
            Writing?.Invoke(list);
            File.Move(list + ".new", list, overwrite: true);
            listed = true;
            foreach (var path in _staged) Move(path);
            foreach (var then in _afterward) then();
            File.Delete(list);
        }
        catch when (!listed)
        {
            foreach (var path in _staged.Append(list))
                try
                {
                    File.Delete(path + ".new");
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    // A scratch file that cannot be deleted is written over by the next save, or
                    // deleted as the world opens next.
                }
            throw;
        }
        finally
        {
            (_staged, _afterward) = (null, null);
        }
    }

    // A save written on a worker, while which nothing but it reads or changes the save.
    private Task? _aside;

    /// <summary>
    /// Whether a save <see cref="WriteAside"/> began is under way, while which nothing but it reads
    /// or changes the save.
    /// </summary>
    public bool Busy => _aside is { IsCompleted: false };

    /// <summary>
    /// Whether a save <see cref="WriteAside"/> began has ended, for <see cref="Settle"/> to take.
    /// </summary>
    public bool Ended => _aside is { IsCompleted: true };

    /// <summary>
    /// Writes a save whole as <see cref="WriteWhole"/> does, then lets go of the regions as
    /// <see cref="LetGo"/> does, on a worker, so the frame that begins it does not wait for its
    /// files to reach the disk.
    /// </summary>
    /// <remarks>
    /// What <paramref name="write"/> writes is gathered before, on the thread the world changes on,
    /// and that thread touches the save in no other way until <see cref="Settle"/> takes the save's
    /// end, as the streamer holds its columns while the save is <see cref="Busy"/>. A save begun
    /// while another is under way waits for it first.
    /// </remarks>
    public void WriteAside(Action write, IReadOnlyList<(int X, int Z)> loaded)
    {
        Settle();
        _aside = Task.Run(() =>
        {
            WriteWhole(write);
            LetGo(loaded);
        });
    }

    /// <summary>
    /// Waits for a save <see cref="WriteAside"/> began, where one was, and throws what it threw.
    /// </summary>
    /// <returns>Whether a save was taken.</returns>
    public bool Settle()
    {
        if (_aside is not { } aside) return false;
        _aside = null;
        aside.GetAwaiter().GetResult();
        return true;
    }

    // Finishes the moves of a save whose list stands, ended among them, and gives whether the
    // scratch files left may go, as they may not where a list that cannot be read was set aside,
    // whose files are left with it.
    private bool Finish()
    {
        var list = Path.Combine(Folder, ListName);
        if (File.Exists(list) && !_held.Contains(list))
        {
            string[] names;
            try
            {
                names = ReadList(list);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
            {
                TraceLog(LogLevel.Warning, $"The list of a save of {Name} cannot be read, so the world opens as before that save, {SetAside(list)}. {ex.Message}");
                return false;
            }
            foreach (var name in names) Move(Path.Combine(Folder, name));
            File.Delete(list);
        }
        return !Directory.EnumerateFiles(Folder, ListName + ".unread*").Any();
    }

    // Deletes the scratch files of a save that ended before its list, as the world opens.
    private void ClearScratch()
    {
        foreach (var scratch in Directory.EnumerateFiles(Folder, "*.new").Concat(Directory.EnumerateFiles(Path.Combine(Folder, "regions"), "*.new")).ToList())
            try
            {
                File.Delete(scratch);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // A scratch file that cannot be deleted is read by nothing and written over by the
                // save of its file.
            }
    }

    // The names a save's list holds, each a file of the save's own kinds in its folder, a list past
    // its bounds or naming another file refused as one that cannot be read.
    private static string[] ReadList(string path)
    {
        var names = ReadText(path, MostListBytes).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (names.Length > MostListed) throw new InvalidDataException($"it names {names.Length} files, past the {MostListed} a save writes");
        foreach (var name in names)
            if (!Listed.IsMatch(name)) throw new InvalidDataException("it names a file that is not one of the save's");
        return names;
    }

    /// <summary>
    /// Leaves out a kept column whose blocks could not be read, so it is generated again, and keeps
    /// a copy of its region's file aside, the file standing until the next <see cref="Flush"/>
    /// writes the region's other columns.
    /// </summary>
    /// <remarks>
    /// The region is read first, as a change to it is, so one let go since the column was asked for
    /// is written again with its other columns.
    /// </remarks>
    public void Unreadable(int x, int z)
    {
        ReadRegion(x, z);
        _kept.TryRemove((x, z), out _);
        var region = RegionOf(x, z);
        var path = RegionPath(region);
        var aside = File.Exists(path) ? $" and {CopyAside(path)}" : "";
        TraceLog(LogLevel.Warning, $"The column {x}, {z} of the region {path} could not be read, so it is generated again{aside}.");
        _changedRegions.Add(region);
    }

    /// <summary>
    /// Counts the blocks of a column read as air since their numbers lie past the game's, for the
    /// next <see cref="Flush"/> to say once a region with how many its columns held, so a world
    /// losing blocks this way is seen.
    /// </summary>
    public void Unknown(int x, int z, int count)
    {
        var region = RegionOf(x, z);
        var (read, said) = _unknown.GetValueOrDefault(region);
        _unknown[region] = (read + count, said);
    }

    /// <summary>Keeps a changed column, to be written with its region at the next <see cref="Flush"/>.</summary>
    public void Keep(ChunkColumn column)
    {
        ReadRegion(column.X, column.Z);
        _kept[(column.X, column.Z)] = Encode(column);
        _changedRegions.Add(RegionOf(column.X, column.Z));
        column.Changed = false;
    }

    /// <summary>Writes every region a column was kept in since the last flush, with the key of each block by its number where the world's info numbers them otherwise, and says how many blocks each region gave read as air since it last said.</summary>
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
        Afterward(_changedRegions.Clear);
        foreach (var (region, (read, _)) in _unknown.Where(counted => counted.Value.Read > counted.Value.Said).ToList())
        {
            TraceLog(LogLevel.Warning, $"The region {RegionPath(region)} holds {read} blocks of numbers this game does not know, which are read as air.");
            _unknown[region] = (read, read);
        }
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
                if (JsonSerializer.Deserialize(ReadText(path, MostInfoBytes), SaveJson.Default.WorldInfo) is { } info)
                    found.Add((Path.GetFileName(folder), Bounded(info).Kind, info.Seed, File.GetLastWriteTimeUtc(path)));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                // A world whose info cannot be read is left out, as opening it would begin it anew,
                // and the log names it.
                TraceLog(LogLevel.Warning, $"The world {Path.GetFileName(folder)} is left out of the worlds listed, since {path} could not be read. {ex.Message}");
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
            var read = JsonSerializer.Deserialize(ReadText(path, MostInfoBytes), SaveJson.Default.WorldInfo);
            if (read is null) return null;
            var info = Bounded(read);
            if (info != read)
                TraceLog(LogLevel.Warning, $"{path} holds a kind, a place, a heading, a pitch, an hour, a day, a saturation or a list of blocks the game cannot hold, so those open as a new world's and the rest as saved.");
            if (info.Blocks is { } keys) (_keys, _renumber) = (keys, Renumbering(keys));
            return info;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            InfoFault = SetAside(path);
            TraceLog(LogLevel.Warning, $"{path} could not be read, so the world begins as a new one of its name and {InfoFault}. {ex.Message}");
            return null;
        }
    }

    /// <summary>What became of the world's info where <see cref="ReadInfo"/> could not read it, for the player to be told, or null.</summary>
    public string? InfoFault { get; private set; }

    // A world's info with each value the game cannot hold taken as a new world's, so a file changed by
    // hand opens with the rest of what it keeps. A kind named by none is one this build makes none
    // of, which opens as the kind asked for. A place that is no number or lies past the land's edge
    // lands the player over the spawn. A heading, a pitch, an hour or a saturation that is no number
    // is a new world's, and a list of blocks past the 65,536 a region's numbers tell apart is held
    // to them.
    private static WorldInfo Bounded(WorldInfo info)
    {
        if (info.Kind is null) info = info with { Kind = "" };
        if (!new LandPlace(info.X, info.Y, info.Z).InLand) info = info with { X = 0, Y = 0, Z = 0, Landing = true };
        if (!float.IsFinite(info.Heading)) info = info with { Heading = 0 };
        if (!float.IsFinite(info.Pitch)) info = info with { Pitch = 0 };
        if (!float.IsFinite(info.Hour)) info = info with { Hour = Sky.FirstHour };
        // An hour past a day is held within one by its remainder, and a day before the first is the
        // first.
        else if (info.Hour is < 0 or >= 24) info = info with { Hour = (info.Hour % 24 + 24) % 24 };
        if (info.Day < 0) info = info with { Day = 0 };
        if (!float.IsFinite(info.Saturation)) info = info with { Saturation = 5 };
        if (info.Blocks is { Length: > ushort.MaxValue + 1 } keys) info = info with { Blocks = keys[..(ushort.MaxValue + 1)] };
        return info;
    }

    public void WriteInfo(WorldInfo info) =>
        Replace(Path.Combine(Folder, "world.json"), file => JsonSerializer.Serialize(file, info, SaveJson.Default.WorldInfo));

    /// <summary>
    /// The mobs a column kept as it unloaded, or as the world was last saved while it was loaded,
    /// taken out of the save to stand in the world, or none where the column has not had its mobs
    /// placed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A region's mobs are kept in a file of their own beside its blocks, read as one of its columns
    /// first loads, and held while a column of it is loaded or its mobs wait to be written, so the
    /// mobs held stay as many however far the player goes. A mob is saved by its kind's name, which
    /// the file lists once, so kinds added or reordered since keep what was saved, and a mob of a
    /// kind this build does not know, or one whose place is no number, is left out.
    /// </para>
    /// <para>
    /// A folder's <c>mobs.bin</c>, which holds every column's mobs in one file, is read once into the
    /// regions and written into their files at the next <see cref="WriteMobs"/>, then kept beside as
    /// read.
    /// </para>
    /// </remarks>
    public List<MobRecord>? TakeMobs(int x, int z) => _mobs.Take(x, z);

    /// <summary>Keeps the mobs of a column that unloads, none among them for one whose mobs were placed and are gone, to be written at the next <see cref="WriteMobs"/>.</summary>
    public void KeepMobs(int x, int z, List<MobRecord> mobs) => _mobs.Keep(x, z, mobs);

    /// <summary>How many regions' mobs the save holds.</summary>
    public int MobRegions => _mobs.Regions;

    /// <summary>
    /// Writes the mobs of the loaded columns that have had their mobs placed, as they stand, with
    /// those kept by the columns unloaded since, into their regions' files, and lets go of each region
    /// no loaded column lies in.
    /// </summary>
    public void WriteMobs(IEnumerable<((int X, int Z) Column, IReadOnlyList<MobRecord> Mobs)> loaded) => _mobs.Write(loaded);

    private string OneFile => Path.Combine(Folder, "mobs.bin");

    private string MobRegionPath((int X, int Z) region) => Path.Combine(Folder, "regions", $"m.{region.X}.{region.Z}.bin");

    // The records of a kind the save keeps by region in files of their own beside the blocks, a
    // region's read from its file the first time one of its columns asks and held while a column of
    // it is loaded or its records wait to be written, so the records held stay as many however far
    // the player goes. The one file a folder kept every column's records of the kind in, where the
    // kind is given one, is read into the regions the first time they are asked for and kept beside
    // as read once they are written.
    private sealed class Regional<T>(WorldSave save, Func<(int X, int Z), Dictionary<(int X, int Z), List<T>>> read,
        Action<(int X, int Z), Dictionary<(int X, int Z), List<T>>> write, string? oneFile = null, Func<Dictionary<(int X, int Z), List<T>>>? readOneFile = null)
    {
        private readonly Dictionary<(int X, int Z), Dictionary<(int X, int Z), List<T>>> _regions = [];
        private readonly HashSet<(int X, int Z)> _changed = [];
        private bool _oneFileRead;

        public int Regions => _regions.Count;

        public List<T>? Take(int x, int z) => Region(RegionOf(x, z)).Remove((x, z), out var records) ? records : null;

        public void Keep(int x, int z, List<T> records)
        {
            var region = RegionOf(x, z);
            Region(region)[(x, z)] = records;
            _changed.Add(region);
        }

        public void Write(IEnumerable<((int X, int Z) Column, IReadOnlyList<T> Records)> loaded)
        {
            ReadOneFileOnce();
            var columns = loaded.ToList();
            var standing = new HashSet<(int X, int Z)>();
            foreach (var (column, records) in columns)
            {
                var region = RegionOf(column.X, column.Z);
                Region(region)[column] = [.. records];
                _changed.Add(region);
                standing.Add(region);
            }
            foreach (var region in _changed) write(region, _regions[region]);
            if (oneFile is not null && File.Exists(oneFile) && !save._held.Contains(oneFile)) save.Retire(oneFile);
            // A loaded column's records stand in the world, so what was written of them is let go
            // from its region too, which takes them again as the column unloads.
            save.Afterward(() =>
            {
                _changed.Clear();
                foreach (var region in _regions.Keys.Where(r => !standing.Contains(r)).ToList()) _regions.Remove(region);
                foreach (var (column, _) in columns) _regions[RegionOf(column.X, column.Z)].Remove(column);
            });
        }

        // The records a region's columns keep, read from its file the first time.
        private Dictionary<(int X, int Z), List<T>> Region((int X, int Z) region)
        {
            ReadOneFileOnce();
            if (!_regions.TryGetValue(region, out var columns)) _regions[region] = columns = read(region);
            return columns;
        }

        // Takes the records of the one file into the regions they lie in, the first time the save's
        // records of the kind are asked for.
        private void ReadOneFileOnce()
        {
            if (_oneFileRead) return;
            _oneFileRead = true;
            if (oneFile is null || readOneFile is null || !File.Exists(oneFile)) return;
            foreach (var (column, records) in readOneFile())
            {
                var region = RegionOf(column.X, column.Z);
                if (!_regions.TryGetValue(region, out var columns)) _regions[region] = columns = read(region);
                columns[column] = records;
                _changed.Add(region);
            }
        }
    }

    private Dictionary<(int X, int Z), List<MobRecord>> ReadMobRegion((int X, int Z) region)
    {
        var columns = new Dictionary<(int X, int Z), List<MobRecord>>();
        var path = MobRegionPath(region);
        if (!File.Exists(path)) return columns;
        var lost = 0;
        try
        {
            using var reader = new BinaryReader(new ZLibStream(File.OpenRead(path), CompressionMode.Decompress));
            if (!reader.ReadBytes(4).AsSpan().SequenceEqual(MobsMagic) || reader.ReadInt32() != MobsVersion)
                throw new InvalidDataException("it is not a mobs file of a version this game reads");
            var kinds = ReadKinds(reader);
            var count = reader.ReadInt32();
            if (count is < 0 or > RegionSize * RegionSize) throw new InvalidDataException("it counts more columns than a region holds");
            for (int c = 0; c < count; c++)
            {
                int lx = reader.ReadByte(), lz = reader.ReadByte();
                if (lx >= RegionSize || lz >= RegionSize) throw new InvalidDataException("a column of it lies outside it");
                var mobs = columns[(region.X * RegionSize + lx, region.Z * RegionSize + lz)] = [];
                var held = reader.ReadInt32();
                if (held is < 0 or > MostMobsInColumn) throw new InvalidDataException("a column of it holds more mobs than a column of the game's");
                for (int m = 0; m < held; m++)
                    if (ReadMob(reader, kinds, floats: false) is { Position.Finite: true } mob) mobs.Add(mob);
                    else lost++;
            }
            if (lost > 0) TraceLog(LogLevel.Warning, $"{path} kept {lost} mobs of a kind this game does not know or whose place was no number, which are left out.");
        }
        catch (Exception ex) when (SetsAside(ex) || Holds(ex))
        {
            // The region's columns place their mobs again by their generator rather than the world
            // not loading.
            TraceLog(LogLevel.Warning, $"{path} could not be read, so the region's mobs are placed again and {(SetsAside(ex) ? SetAside(path) : Hold(path))}. {ex.Message}");
            columns.Clear();
        }
        return columns;
    }

    // The kinds a mobs file lists, each by its name, none for a name this build does not know.
    private static MobKind?[] ReadKinds(BinaryReader reader)
    {
        var listed = reader.ReadInt32();
        // A record names its kind by a byte, so no file the game writes lists more kinds than a byte
        // tells apart.
        if (listed is < 0 or > 256) throw new InvalidDataException("its list of kinds has no length");
        var kinds = new MobKind?[listed];
        for (int i = 0; i < kinds.Length; i++) kinds[i] = MobKinds.Find(reader.ReadString());
        return kinds;
    }

    // A mob as a mobs file keeps it, its places in doubles or in floats, or none for a kind this
    // build does not know.
    private static MobRecord? ReadMob(BinaryReader reader, MobKind?[] kinds, bool floats)
    {
        LandPlace Place() => floats ? new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()) : new(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
        var number = reader.ReadByte();
        var kind = number < kinds.Length ? kinds[number] : null;
        var variant = reader.ReadByte();
        var at = Place();
        var yaw = reader.ReadSingle();
        // A turn that is no number is none, so no pose is turned by it.
        if (!float.IsFinite(yaw)) yaw = 0;
        var health = reader.ReadByte();
        LandPlace? home = reader.ReadBoolean() ? Place() : null;
        return kind is null ? null : new MobRecord(kind, (byte)(variant % kind.Variants.Length), at, yaw, Math.Clamp((int)health, 1, kind.Health), home);
    }

    private void WriteMobRegion((int X, int Z) region, Dictionary<(int X, int Z), List<MobRecord>> columns) =>
        Replace(MobRegionPath(region), file =>
        {
            using var writer = new BinaryWriter(new ZLibStream(file, CompressionLevel.Fastest, leaveOpen: true));
            writer.Write(MobsMagic);
            writer.Write(MobsVersion);
            writer.Write(MobKinds.All.Length);
            foreach (var kind in MobKinds.All) writer.Write(kind.Name);
            writer.Write(columns.Count);
            foreach (var ((x, z), mobs) in columns)
            {
                writer.Write((byte)(x - region.X * RegionSize));
                writer.Write((byte)(z - region.Z * RegionSize));
                writer.Write(mobs.Count);
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

    // The mobs of mobs.bin, a file of every column's mobs, of version 1 with its places in floats or
    // 2 in doubles, by the columns that have had their mobs placed. A mob whose place is no number is
    // set down at its home where it has one that is a number, a villager's, in its home's column, and
    // otherwise left out, and the log says how many of each.
    private Dictionary<(int X, int Z), List<MobRecord>> ReadOneFile()
    {
        var columns = new Dictionary<(int X, int Z), List<MobRecord>>();
        var (strays, lost) = (new List<MobRecord>(), 0);
        try
        {
            using var reader = new BinaryReader(new ZLibStream(File.OpenRead(OneFile), CompressionMode.Decompress));
            var version = reader.ReadBytes(4).AsSpan().SequenceEqual(MobsMagic) ? reader.ReadInt32() : 0;
            if (version is not (1 or 2)) throw new InvalidDataException("it is not a mobs file of a version this game reads");
            var kinds = ReadKinds(reader);
            var count = reader.ReadInt32();
            if (count is < 0 or > MostOneFileColumns) throw new InvalidDataException("it counts more columns than a world of the game's");
            var total = 0;
            for (int c = 0; c < count; c++)
            {
                var mobs = columns[(reader.ReadInt32(), reader.ReadInt32())] = [];
                var held = reader.ReadUInt16();
                if (held > MostMobsInColumn || (total += held) > MostOneFileMobs) throw new InvalidDataException("it holds more mobs than a world of the game's");
                for (int m = held; m > 0; m--)
                {
                    if (ReadMob(reader, kinds, floats: version == 1) is not { } mob) continue;
                    if (mob.Position.Finite) mobs.Add(mob);
                    else if (mob.Home is { Finite: true } home) strays.Add(mob with { Position = home });
                    else lost++;
                }
            }
            foreach (var mob in strays)
            {
                var column = (VoxelWorld.ColumnOf((int)Math.Floor(mob.Position.X)), VoxelWorld.ColumnOf((int)Math.Floor(mob.Position.Z)));
                if (columns.TryGetValue(column, out var mobs)) mobs.Add(mob);
                else columns[column] = [mob];
            }
            if (strays.Count + lost > 0)
                TraceLog(LogLevel.Warning, $"{OneFile} kept {strays.Count + lost} mobs whose place was no number, {strays.Count} set down at their homes and {lost} left out.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or FormatException)
        {
            // The columns' mobs are placed again by their generator rather than the world not loading.
            TraceLog(LogLevel.Warning, $"{OneFile} could not be read, so the columns' mobs are placed again and {SetAside(OneFile)}. {ex.Message}");
            columns.Clear();
        }
        return columns;
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
            // A record that is null, as a file changed by hand may hold, is no furnace.
            foreach (var r in (JsonSerializer.Deserialize(ReadText(path, MostRecordsBytes), SaveJson.Default.FurnaceRecordArray) ?? []).OfType<FurnaceRecord>())
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

    public void WriteFurnaces(Furnaces furnaces) => WriteFurnaces(FurnaceRecords(furnaces));

    /// <summary>
    /// The furnaces as a save keeps them, gathered apart from their writing for a save written
    /// aside.
    /// </summary>
    public static FurnaceRecord[] FurnaceRecords(Furnaces furnaces) =>
        [.. furnaces.All.Select(f => new FurnaceRecord(f.Key.X, f.Key.Y, f.Key.Z, [.. f.Value.Slots.Select(Inventory.Encode)], f.Value.Burning, f.Value.BurnTotal, f.Value.Cooked))];

    /// <summary>Writes the furnaces <see cref="FurnaceRecords"/> gathered.</summary>
    public void WriteFurnaces(FurnaceRecord[] records) =>
        Replace(Path.Combine(Folder, "furnaces.json"), file => JsonSerializer.Serialize(file, records, SaveJson.Default.FurnaceRecordArray));

    /// <summary>A chest as a save keeps it, with its place, its slots as <see cref="Inventory.Encode"/> gives them, and the loot it holds not yet rolled.</summary>
    public sealed record ChestRecord(int X, int Y, int Z, string[] Slots, string? Loot, uint Seed);

    /// <summary>
    /// The chests a column kept as it unloaded, or as the world was last saved while it was loaded,
    /// taken out of the save to stand in the world, or none where the save keeps none of the
    /// column.
    /// </summary>
    /// <remarks>
    /// A region's chests are kept in a file of their own beside its blocks and its mobs, so the
    /// chests held stay as many however far the player goes.
    /// </remarks>
    public List<ChestRecord>? TakeChests(int x, int z) => _chests.Take(x, z);

    /// <summary>Keeps the chests of a column that unloads, none for one whose chests are gone, to be written at the next <see cref="WriteChests"/>.</summary>
    public void KeepChests(int x, int z, List<ChestRecord> chests) => _chests.Keep(x, z, chests);

    /// <summary>How many regions' chests the save holds.</summary>
    public int ChestRegions => _chests.Regions;

    /// <summary>
    /// Writes the chests of the loaded columns as they stand, with those kept by the columns
    /// unloaded since, into their regions' files, and lets go of each region no loaded column lies
    /// in.
    /// </summary>
    public void WriteChests(IEnumerable<((int X, int Z) Column, IReadOnlyList<ChestRecord> Chests)> loaded) => _chests.Write(loaded);

    private string ChestRegionPath((int X, int Z) region) => Path.Combine(Folder, "regions", $"c.{region.X}.{region.Z}.bin");

    private Dictionary<(int X, int Z), List<ChestRecord>> ReadChestRegion((int X, int Z) region)
    {
        var columns = new Dictionary<(int X, int Z), List<ChestRecord>>();
        var path = ChestRegionPath(region);
        if (!File.Exists(path)) return columns;
        try
        {
            using var reader = new BinaryReader(new ZLibStream(File.OpenRead(path), CompressionMode.Decompress));
            if (!reader.ReadBytes(4).AsSpan().SequenceEqual(ChestsMagic) || reader.ReadInt32() != ChestsVersion)
                throw new InvalidDataException("it is not a chests file of a version this game reads");
            var count = reader.ReadInt32();
            if (count is < 0 or > RegionSize * RegionSize) throw new InvalidDataException("it counts more columns than a region holds");
            for (int c = 0; c < count; c++)
            {
                int lx = reader.ReadByte(), lz = reader.ReadByte();
                if (lx >= RegionSize || lz >= RegionSize) throw new InvalidDataException("a column of it lies outside it");
                var (cx, cz) = (region.X * RegionSize + lx, region.Z * RegionSize + lz);
                var chests = columns[(cx, cz)] = [];
                var held = reader.ReadInt32();
                if (held is < 0 or > MostChestsInColumn) throw new InvalidDataException("a column of it holds more chests than a column of the game's");
                for (int i = 0; i < held; i++)
                {
                    int x = cx * Section.Size + reader.ReadByte(), y = reader.ReadInt16(), z = cz * Section.Size + reader.ReadByte();
                    if (x >= (cx + 1) * Section.Size || z >= (cz + 1) * Section.Size || y is < ChunkColumn.Bottom or >= ChunkColumn.Ceiling)
                        throw new InvalidDataException("a chest of it lies outside its column");
                    var slots = new string[reader.ReadByte()];
                    if (slots.Length > Chest.Size) throw new InvalidDataException("a chest of it holds more slots than a chest");
                    for (int s = 0; s < slots.Length; s++) slots[s] = ReadChestText(reader);
                    var loot = reader.ReadBoolean() ? ReadChestText(reader) : null;
                    chests.Add(new ChestRecord(x, y, z, slots, loot, reader.ReadUInt32()));
                }
            }
        }
        catch (Exception ex) when (SetsAside(ex) || Holds(ex))
        {
            // The region's columns take the chests their generation gives them again rather than
            // the world not loading.
            TraceLog(LogLevel.Warning, $"{path} could not be read, so the region's chests begin as its columns give them and {(SetsAside(ex) ? SetAside(path) : Hold(path))}. {ex.Message}");
            columns.Clear();
        }
        return columns;
    }

    // A slot's or a loot's text of a chests file, refused past the longest either is kept as.
    private static string ReadChestText(BinaryReader reader)
    {
        var length = reader.Read7BitEncodedInt();
        if (length is < 0 or > MostChestText) throw new InvalidDataException("a text of it is longer than a slot's");
        var bytes = reader.ReadBytes(length);
        return bytes.Length < length ? throw new EndOfStreamException("a text of it ends early") : System.Text.Encoding.UTF8.GetString(bytes);
    }

    private void WriteChestRegion((int X, int Z) region, Dictionary<(int X, int Z), List<ChestRecord>> columns) =>
        Replace(ChestRegionPath(region), file =>
        {
            using var writer = new BinaryWriter(new ZLibStream(file, CompressionLevel.Fastest, leaveOpen: true));
            writer.Write(ChestsMagic);
            writer.Write(ChestsVersion);
            writer.Write(columns.Count);
            foreach (var ((x, z), chests) in columns)
            {
                writer.Write((byte)(x - region.X * RegionSize));
                writer.Write((byte)(z - region.Z * RegionSize));
                writer.Write(chests.Count);
                foreach (var chest in chests)
                {
                    writer.Write((byte)(chest.X - x * Section.Size));
                    writer.Write((short)chest.Y);
                    writer.Write((byte)(chest.Z - z * Section.Size));
                    writer.Write((byte)chest.Slots.Length);
                    foreach (var slot in chest.Slots) writer.Write(slot ?? "");
                    writer.Write(chest.Loot is not null);
                    if (chest.Loot is { } loot) writer.Write(loot);
                    writer.Write(chest.Seed);
                }
            }
        });

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
    public static ChunkColumn Decode(int x, int z, byte[] bytes, ChunkColumn? generated = null) => Decode(x, z, bytes, out _, generated);

    /// <summary>A kept column's blocks, as <see cref="Decode(int, int, byte[], ChunkColumn?)"/> gives them, with how many were of numbers past the game's, read as air.</summary>
    public static ChunkColumn Decode(int x, int z, byte[] bytes, out int unknown, ChunkColumn? generated = null)
    {
        unknown = 0;
        var column = new ChunkColumn(x, z);
        using var zip = new ZLibStream(new MemoryStream(bytes), CompressionMode.Decompress);
        var raw = new byte[Section.Volume * sizeof(ushort)];
        var sections = new List<ushort[]>();
        // A column holds so many sections, so no more is read however far its bytes inflate.
        while (sections.Count < ChunkColumn.SectionCount && zip.ReadAtLeast(raw, raw.Length, throwOnEndOfStream: false) == raw.Length)
            sections.Add(System.Runtime.InteropServices.MemoryMarshal.Cast<byte, ushort>(raw).ToArray());
        var shallow = sections.Count == OldSections;
        var first = shallow ? -ChunkColumn.BottomSection : 0;
        for (int s = 0; s < sections.Count && first + s < ChunkColumn.SectionCount; s++)
        {
            var (section, blocks) = (column.Sections[first + s], sections[s]);
            for (int i = 0; i < Section.Volume; i++)
            {
                // A block of a number past the game's is air, as a key the build does not know is.
                var block = blocks[i];
                if (block >= Blocks.All.Count) unknown++;
                else if (block != 0) section.Set(i & Section.Mask, i >> 8, (i >> 4) & Section.Mask, (BlockId)block);
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
        // One past the eight is enough to tell, however far the bytes inflate.
        while (sections <= OldSections && zip.ReadAtLeast(raw, raw.Length, throwOnEndOfStream: false) == raw.Length) sections++;
        return sections == OldSections;
    }
}
