namespace VoxelSandbox;

/// <summary>
/// The resource packs the player keeps for the game, in a folder beside the saves as Minecraft keeps
/// its own beside its worlds, and which of them are on, in a file beside it naming one pack a line,
/// the first over the rest, as Minecraft lists its packs in its options.
/// </summary>
/// <remarks>
/// A line names a pack in the folder by its file's or its folder's name, or anywhere else by its
/// whole path, so a pack kept elsewhere is used where it lies. A blank line and a line beginning
/// with <c>#</c> are passed over. The game reads the file as it starts and writes it as the packs
/// are chosen, so the packs chosen show again at the next start.
/// </remarks>
public sealed class PackList(string root)
{
    /// <summary>The game's own, beside the saves under the local application data folder.</summary>
    public static PackList Default => new(Path.GetDirectoryName(WorldSave.Root)!);

    /// <summary>The folder the packs are kept in.</summary>
    public string Folder => Path.Combine(root, "resourcepacks");

    /// <summary>The file naming the packs that are on.</summary>
    public string File => Path.Combine(root, "resourcepacks.txt");

    /// <summary>The packs in the folder by name, each a zip or a folder, made where it is not there yet so the player finds it.</summary>
    public IReadOnlyList<string> Available()
    {
        Directory.CreateDirectory(Folder);
        return [.. Directory.EnumerateFileSystemEntries(Folder)
            .Where(p => Directory.Exists(p) || p.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFileName).OfType<string>().Order(StringComparer.OrdinalIgnoreCase)];
    }

    /// <summary>The lines naming the packs that are on, the first over the rest, or none where the file is not there.</summary>
    public IReadOnlyList<string> Read()
    {
        if (!System.IO.File.Exists(File)) return [];
        return [.. System.IO.File.ReadAllLines(File).Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith('#'))];
    }

    /// <summary>Names the packs that are on, the first over the rest.</summary>
    public void Write(IEnumerable<string> entries)
    {
        Directory.CreateDirectory(root);
        System.IO.File.WriteAllLines(File, entries);
    }

    /// <summary>The path a line names, a pack in the folder by its name or one elsewhere by its whole path.</summary>
    public string PathOf(string entry) => Path.IsPathRooted(entry) ? entry : Path.Combine(Folder, entry);

    /// <summary>
    /// A pack's name as a menu shows it, without <c>.zip</c> and the folder it lies in, and without
    /// the codes of Minecraft's colors, a section sign and the letter after it, that some packs' names hold.
    /// </summary>
    public static string Shown(string entry)
    {
        var name = Path.GetFileName(Path.TrimEndingDirectorySeparator(entry));
        if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) name = name[..^4];
        var shown = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (name[i] == '§') { i++; continue; }
            shown.Append(name[i]);
        }
        return shown.ToString();
    }
}
