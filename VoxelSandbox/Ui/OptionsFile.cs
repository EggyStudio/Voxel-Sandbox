using System.Globalization;

namespace VoxelSandbox;

/// <summary>
/// The settings of the options' screens kept across runs, in a file beside the saves as Minecraft
/// keeps its options.txt, one line a setting, its screen and its name joined by a point, its words
/// by underscores, then a colon and its value, a choice by its name.
/// </summary>
/// <remarks>
/// The game reads the file as it starts and writes it as a screen of the menu is left, so a setting
/// changed shows again at the next start, and the file is written again only where a setting
/// differs from what it holds. A line the game cannot read, of a setting it does not have, a value
/// that is no number or none of the setting's choices, or a number out of its reach, is passed over
/// and its setting keeps its default. The hour, the day's cycle and the moon are the world's, which
/// its save keeps, and are left out.
/// </remarks>
public sealed class OptionsFile(string root)
{
    // The text last read or written, so leaving a screen with nothing changed writes nothing.
    private string? _held;

    /// <summary>The game's own, beside the saves under the local application data folder.</summary>
    public static OptionsFile Default => new(Path.GetDirectoryName(WorldSave.Root)!);

    public string File => Path.Combine(root, "options.txt");

    /// <summary>A setting's key in the file, its screen and its name joined by a point.</summary>
    public static string Key(string screen, Option option) => $"{screen}.{option.Name.Replace(' ', '_')}";

    /// <summary>The file's text for the settings as they stand, one line to each one kept.</summary>
    public static string Text(IEnumerable<(string Name, IReadOnlyList<Option> Options)> screens)
    {
        var text = new System.Text.StringBuilder();
        foreach (var (screen, options) in screens)
            foreach (var option in options.Where(o => o.Kept))
                text.Append(Key(screen, option)).Append(':').Append(ValueOf(option)).Append('\n');
        return text.ToString();
    }

    /// <summary>Writes the settings as they stand where they differ from the file's.</summary>
    /// <returns>Whether the file was written.</returns>
    public bool Write(IEnumerable<(string Name, IReadOnlyList<Option> Options)> screens)
    {
        var text = Text(screens);
        if (text == _held) return false;
        Directory.CreateDirectory(root);
        // Written beside and moved over the file, so a run stopped as it writes leaves the last whole one.
        System.IO.File.WriteAllText(File + ".new", text);
        System.IO.File.Move(File + ".new", File, overwrite: true);
        _held = text;
        return true;
    }

    /// <summary>Sets each setting the file holds a line of, the rest keeping their defaults.</summary>
    /// <returns>How many lines set a setting.</returns>
    public int Read(IEnumerable<(string Name, IReadOnlyList<Option> Options)> screens)
    {
        if (!System.IO.File.Exists(File)) return 0;
        var text = System.IO.File.ReadAllText(File);
        _held = text;
        var kept = new Dictionary<string, Option>(StringComparer.OrdinalIgnoreCase);
        foreach (var (screen, options) in screens)
            foreach (var option in options.Where(o => o.Kept))
                kept[Key(screen, option)] = option;
        var set = 0;
        foreach (var line in text.Split('\n'))
        {
            var colon = line.IndexOf(':');
            if (colon < 0 || !kept.TryGetValue(line[..colon].Trim(), out var option)) continue;
            if (Parse(option, line[(colon + 1)..].Trim()) is not { } value) continue;
            option.Change(value);
            option.Settled?.Invoke();
            set++;
        }
        return set;
    }

    // A choice by its name with its words joined by underscores, a number with a point.
    private static string ValueOf(Option option) => option.Choices is not null
        ? option.Shown.Replace(' ', '_')
        : option.Value.ToString("0.####", CultureInfo.InvariantCulture);

    private static float? Parse(Option option, string value)
    {
        if (option.Choices is { } choices)
        {
            var index = Array.FindIndex(choices, c => c.Replace(' ', '_').Equals(value, StringComparison.OrdinalIgnoreCase));
            return index >= 0 ? index : null;
        }
        if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) || !float.IsFinite(number)) return null;
        return number >= option.Low && number <= option.High ? number : null;
    }
}
