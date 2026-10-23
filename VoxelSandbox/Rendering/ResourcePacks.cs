using System.IO.Compression;
using System.Text.Json;
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
/// <c>assets/minecraft</c>, and read when asked for. A texture is decoded from its PNG to RGBA with
/// the <c>.mcmeta</c> beside it in the same pack, and an animated one, which its metadata says is a
/// strip of frames one above another as Minecraft draws water and lanterns, gives its first frame as
/// its picture and its frames in the order they play through <see cref="Animation"/>.
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
    public byte[]? Read(string path) => Find(path) is { } pack ? Read(pack, path) : null;

    // The first pack holding a file, or null.
    private int? Find(string path)
    {
        for (int i = 0; i < _packs.Count; i++)
            if (_packs[i].Files.ContainsKey(path)) return i;
        return null;
    }

    private byte[]? Read(int pack, string path)
    {
        var (_, zip, files) = _packs[pack];
        if (!files.TryGetValue(path, out var at)) return null;
        if (zip is null) return File.ReadAllBytes(at);
        using var stream = zip.GetEntry(at)!.Open();
        using var copy = new MemoryStream();
        stream.CopyTo(copy);
        return copy.ToArray();
    }

    /// <summary>
    /// A texture under <c>assets/minecraft/textures</c> by its path there without <c>.png</c>, as
    /// <c>block/stone</c>, decoded to RGBA, the first frame of an animated one, or null where no pack has it.
    /// </summary>
    public Image? Texture(string name) => Picture(name)?.Image;

    /// <summary>
    /// A texture as <see cref="Texture"/> reads it with the root of the <c>.mcmeta</c> the same pack
    /// holds beside it, or null for the metadata where it has none or it is not JSON.
    /// </summary>
    public (Image Image, JsonElement? Meta)? Picture(string name)
    {
        var path = $"textures/{name}.png";
        if (Find(path) is not { } pack || Read(pack, path) is not { } png) return null;
        var image = LoadImageFromMemory(".png", png);
        if (image.Data is null || image.Width <= 0) return null;
        var meta = Meta(Read(pack, path + ".mcmeta"));
        if (meta is not { } root || !root.TryGetProperty("animation", out var animation) || animation.ValueKind != JsonValueKind.Object) return (image, meta);
        // A strip of frames one above another, each as tall as it is wide unless the metadata says
        // otherwise, of which the first is kept.
        int width = Number(animation, "width") ?? image.Width, height = Number(animation, "height") ?? width;
        if (width > image.Width || height > image.Height) return (image, meta);
        var frame = new byte[width * height * 4];
        for (int y = 0; y < height; y++) Array.Copy(image.Data, y * image.Width * 4, frame, y * width * 4, width * 4);
        return (new Image(frame, width, height), meta);
    }

    /// <summary>
    /// An animated texture's frames in the order its metadata plays them, each with the game ticks,
    /// a twentieth of a second each, it is held for, its own time or the metadata's frame time, or 1
    /// where neither is given, or null for a texture no pack has or that has no two frames to play.
    /// A frame's index past the strip's frames is left out, and frames that the metadata blends one
    /// into the next are stepped.
    /// </summary>
    public (Image[] Frames, int[] Ticks)? Animation(string name)
    {
        var path = $"textures/{name}.png";
        if (Find(path) is not { } pack || Read(pack, path) is not { } png) return null;
        if (Meta(Read(pack, path + ".mcmeta")) is not { } root || !root.TryGetProperty("animation", out var animation) || animation.ValueKind != JsonValueKind.Object) return null;
        var image = LoadImageFromMemory(".png", png);
        if (image.Data is null || image.Width <= 0) return null;
        int width = Number(animation, "width") ?? image.Width, height = Number(animation, "height") ?? width;
        if (width > image.Width || height > image.Height) return null;
        var count = image.Height / height;
        var time = Number(animation, "frametime") ?? 1;
        var order = new List<(int Index, int Ticks)>();
        if (animation.TryGetProperty("frames", out var frames) && frames.ValueKind == JsonValueKind.Array)
            foreach (var frame in frames.EnumerateArray())
            {
                if (frame.ValueKind == JsonValueKind.Number && frame.TryGetInt32(out var index)) order.Add((index, time));
                else if (frame.ValueKind == JsonValueKind.Object && frame.TryGetProperty("index", out var at) && at.TryGetInt32(out index))
                    order.Add((index, Number(frame, "time") ?? time));
            }
        else
            order.AddRange(Enumerable.Range(0, count).Select(i => (i, time)));
        order.RemoveAll(f => f.Index < 0 || f.Index >= count);
        if (order.Select(f => f.Index).Distinct().Count() < 2) return null;
        Image Frame(int index)
        {
            var pixels = new byte[width * height * 4];
            for (int y = 0; y < height; y++) Array.Copy(image.Data, ((index * height + y) * image.Width) * 4, pixels, y * width * 4, width * 4);
            return new Image(pixels, width, height);
        }
        return ([.. order.Select(f => Frame(f.Index))], [.. order.Select(f => f.Ticks)]);
    }

    private static int? Number(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.TryGetInt32(out var number) && number > 0 ? number : null;

    private static JsonElement? Meta(byte[]? bytes)
    {
        if (bytes is null) return null;
        try
        {
            using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public void Dispose()
    {
        foreach (var (_, zip, _) in _packs) zip?.Dispose();
        _packs.Clear();
    }
}
