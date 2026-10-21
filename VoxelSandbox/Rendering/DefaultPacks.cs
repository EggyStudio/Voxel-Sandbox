using System.Security.Cryptography;
using System.Text.Json;

namespace VoxelSandbox;

/// <summary>A pack the game turns on by default, its file's name, its title, its Modrinth project and version, where its file is fetched from, and its size and SHA-512.</summary>
public sealed record DefaultPack(string File, string Title, string Project, string Version, string Url, long Size, string Sha512);

/// <summary>How the fetch of the default packs stands.</summary>
public enum FetchState
{
    Idle,
    Fetching,
    Done,
    Failed,
    Stopped,
}

/// <summary>
/// The resource packs the game is drawn with by default, which the repository does not hold, since
/// their terms let no one but their makers hand them out, and which the game fetches from the
/// makers' own pages on Modrinth into its folder of packs the first time it starts, as a Modrinth
/// modpack names its files rather than holding them.
/// </summary>
/// <remarks>
/// <para>
/// <c>resources/packs.json</c> names each pack's file, its Modrinth version and the SHA-512 of the
/// one file the game takes, so a file whose bytes differ is refused and nothing else is ever turned
/// on. The fetch runs on a task of its own, so the game starts at once with its own art and draws
/// itself with the packs once all of them are in and checked. A file already in the folder with the
/// right bytes is kept rather than fetched again.
/// </para>
/// <para>
/// Each request names the game in its user agent, as Modrinth's terms ask of a program reading it.
/// </para>
/// </remarks>
public sealed class DefaultPacks
{
    private readonly IReadOnlyList<DefaultPack> _packs;
    private CancellationTokenSource? _stop;
    private Task? _fetch;

    public DefaultPacks(IReadOnlyList<DefaultPack> packs) => _packs = packs;

    /// <summary>The packs of <c>resources/packs.json</c> beside the program, or none where it is not there.</summary>
    public static DefaultPacks Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "resources", "packs.json");
        return new DefaultPacks(System.IO.File.Exists(path) ? Read(System.IO.File.ReadAllText(path)) : []);
    }

    /// <summary>The packs a manifest names, in its order, the first over the rest.</summary>
    public static IReadOnlyList<DefaultPack> Read(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("packs", out var packs) || packs.ValueKind != JsonValueKind.Array) return [];
        string Text(JsonElement pack, string name) => pack.TryGetProperty(name, out var value) ? value.GetString() ?? "" : "";
        return [.. packs.EnumerateArray().Select(p => new DefaultPack(Text(p, "file"), Text(p, "title"), Text(p, "project"), Text(p, "version"),
            Text(p, "url"), p.TryGetProperty("size", out var size) && size.TryGetInt64(out var bytes) ? bytes : 0, Text(p, "sha512")))];
    }

    /// <summary>Whether bytes are the file a SHA-512 names.</summary>
    public static bool Accepts(ReadOnlySpan<byte> bytes, string sha512) =>
        Convert.ToHexStringLower(SHA512.HashData(bytes)).Equals(sha512, StringComparison.OrdinalIgnoreCase);

    /// <summary>The packs, the first over the rest.</summary>
    public IReadOnlyList<DefaultPack> Packs => _packs;

    /// <summary>How the fetch stands.</summary>
    public FetchState State { get; private set; }

    /// <summary>How many of the packs are in and checked.</summary>
    public int Fetched { get; private set; }

    /// <summary>What stopped a failed fetch.</summary>
    public string? Problem { get; private set; }

    /// <summary>Whether a pack is not in the folder with the right bytes.</summary>
    public bool Missing(PackList list) => _packs.Any(p => !Present(list, p));

    private static bool Present(PackList list, DefaultPack pack)
    {
        var path = list.PathOf(pack.File);
        return System.IO.File.Exists(path) && new FileInfo(path).Length == pack.Size && Accepts(System.IO.File.ReadAllBytes(path), pack.Sha512);
    }

    /// <summary>Begins fetching the packs that are missing into the list's folder on a task of its own, through a handler of the tests' where one is given.</summary>
    public void Start(PackList list, HttpMessageHandler? handler = null)
    {
        if (State == FetchState.Fetching || _packs.Count == 0) return;
        (State, Fetched, Problem) = (FetchState.Fetching, 0, null);
        _stop = new CancellationTokenSource();
        var stop = _stop.Token;
        _fetch = Task.Run(async () =>
        {
            try
            {
                Problem = await Fetch(list, handler, stop);
                State = Problem is null ? FetchState.Done : FetchState.Failed;
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested)
            {
                State = FetchState.Stopped;
            }
            catch (Exception problem) when (problem is HttpRequestException or IOException or TaskCanceledException or UnauthorizedAccessException)
            {
                (Problem, State) = (problem.Message, FetchState.Failed);
            }
        }, stop);
    }

    /// <summary>Stops a fetch under way, leaving the packs already in.</summary>
    public void Stop() => _stop?.Cancel();

    /// <summary>Waits for a fetch under way to end, as a test does.</summary>
    public void Wait() => _fetch?.Wait();

    private async Task<string?> Fetch(PackList list, HttpMessageHandler? handler, CancellationToken stop)
    {
        Directory.CreateDirectory(list.Folder);
        using var client = new HttpClient(handler ?? new HttpClientHandler(), disposeHandler: handler is null) { Timeout = TimeSpan.FromMinutes(2) };
        var version = typeof(DefaultPacks).Assembly.GetName().Version;
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"VoxelSandbox/{version?.ToString(3) ?? "0"} (+https://github.com/EggyStudio/Voxel-Sandbox)");
        foreach (var pack in _packs)
        {
            if (!Present(list, pack))
            {
                var bytes = await client.GetByteArrayAsync(pack.Url, stop);
                if (!Accepts(bytes, pack.Sha512)) return $"{pack.Title} came with other bytes than the game takes, and is left out";
                // Written beside its place and moved into it, so a fetch cut short leaves no half file.
                var path = list.PathOf(pack.File);
                await System.IO.File.WriteAllBytesAsync(path + ".part", bytes, stop);
                System.IO.File.Move(path + ".part", path, overwrite: true);
            }
            Fetched++;
        }
        return null;
    }
}
