using System.IO.Compression;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// Minecraft resource packs the player has, each a zip or a folder holding <c>assets/minecraft</c>,
/// layered as Minecraft layers them, the first of the list over the rest, so a file the first lacks
/// is read from the next that has it. The game ships no pack and reads only the player's own.
/// </summary>
/// <remarks>
/// A pack's files are listed once as it loads, a zip's entries and a folder's files under
/// <c>assets/minecraft</c>, and read when asked for. A texture is decoded from its PNG to RGBA, and an
/// animated one, a strip of square frames one above another as Minecraft draws water and lanterns,
/// gives its first frame.
/// </remarks>
public sealed class ResourcePacks : IDisposable
{
    private const string Root = "assets/minecraft/";

    // Each pack's files by their path under assets/minecraft, as a zip's entry or a folder's file.
    private readonly List<(string Name, ZipArchive? Zip, Dictionary<string, string> Files)> _packs = [];

    /// <summary>No pack, so every texture is the game's own color.</summary>
    public static readonly ResourcePacks None = new([]);

    /// <summary>Opens the packs at the paths given, the first over the rest, leaving out a path that is neither a zip nor a folder.</summary>
    public ResourcePacks(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            if (Directory.Exists(path))
            {
                var files = new Dictionary<string, string>(StringComparer.Ordinal);
                var root = Path.Combine(path, "assets", "minecraft");
                if (Directory.Exists(root))
                    foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                        files[Path.GetRelativePath(root, file).Replace('\\', '/')] = file;
                _packs.Add((Path.GetFileName(Path.TrimEndingDirectorySeparator(path)), null, files));
            }
            else if (File.Exists(path) && path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                var zip = ZipFile.OpenRead(path);
                var files = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var entry in zip.Entries)
                    if (entry.FullName.StartsWith(Root, StringComparison.Ordinal) && entry.Length > 0)
                        files[entry.FullName[Root.Length..]] = entry.FullName;
                _packs.Add((Path.GetFileNameWithoutExtension(path), zip, files));
            }
        }
    }

    /// <summary>The packs' names, the first over the rest.</summary>
    public IReadOnlyList<string> Names => [.. _packs.Select(p => p.Name)];

    /// <summary>Whether any pack is open.</summary>
    public bool Any => _packs.Count > 0;

    /// <summary>The bytes of a file under <c>assets/minecraft</c> from the first pack that has it, or null where none does.</summary>
    public byte[]? Read(string path)
    {
        foreach (var (_, zip, files) in _packs)
        {
            if (!files.TryGetValue(path, out var at)) continue;
            if (zip is null) return File.ReadAllBytes(at);
            using var stream = zip.GetEntry(at)!.Open();
            using var copy = new MemoryStream();
            stream.CopyTo(copy);
            return copy.ToArray();
        }
        return null;
    }

    /// <summary>
    /// A texture under <c>assets/minecraft/textures</c> by its path there without <c>.png</c>, as
    /// <c>block/stone</c>, decoded to RGBA, the first frame of an animated one, or null where no pack has it.
    /// </summary>
    public Image? Texture(string name)
    {
        if (Read($"textures/{name}.png") is not { } png) return null;
        var image = LoadImageFromMemory(".png", png);
        if (image.Data is null || image.Width <= 0) return null;
        if (image.Height <= image.Width) return image;
        // A strip of frames as tall as it is wide, one above another, of which the first is kept.
        var frame = new byte[image.Width * image.Width * 4];
        Array.Copy(image.Data, frame, frame.Length);
        return new Image(frame, image.Width, image.Width);
    }

    public void Dispose()
    {
        foreach (var (_, zip, _) in _packs) zip?.Dispose();
        _packs.Clear();
    }
}
