using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The screenshots F2 takes, as Minecraft's, each the frame written as a PNG into a
/// <c>screenshots</c> folder beside the saves, made the first time one is taken, and named by the
/// time it was taken as Minecraft names its own, <c>2026-10-21_03.45.12.png</c>, with a count after
/// it where two fall in one second.
/// </summary>
/// <remarks>
/// The engine writes the frame once it is presented, so a name is kept as taken from the moment it
/// is asked for, and a second screenshot in the same second takes the next count before the first
/// file is there.
/// </remarks>
public sealed class Screenshots(string folder)
{
    private readonly HashSet<string> _taken = new(StringComparer.Ordinal);

    /// <summary>The game's own, beside the saves under the local application data folder.</summary>
    public static Screenshots Default => new(Path.Combine(Path.GetDirectoryName(WorldSave.Root)!, "screenshots"));

    /// <summary>The folder the screenshots are written into.</summary>
    public string Folder => folder;

    /// <summary>A screenshot's name for its time and its count within that second, the first with none, as Minecraft's are.</summary>
    public static string Name(DateTime time, int count) => $"{time:yyyy-MM-dd_HH.mm.ss}{(count > 1 ? $"_{count}" : "")}.png";

    /// <summary>The path the next screenshot at a time is written to, the first count of that second neither on disk nor taken, kept as taken.</summary>
    public string Reserve(DateTime time)
    {
        for (int count = 1; ; count++)
        {
            var path = Path.Combine(folder, Name(time, count));
            if (!File.Exists(path) && _taken.Add(path)) return path;
        }
    }

    /// <summary>Writes the frame being drawn into the folder, made where it is not there yet, under the name of the time now.</summary>
    /// <returns>The file's name.</returns>
    public string Take()
    {
        Directory.CreateDirectory(folder);
        var path = Reserve(DateTime.Now);
        TakeScreenshot(path);
        return Path.GetFileName(path);
    }
}
