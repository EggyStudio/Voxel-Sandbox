using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>Which of the game's menus is open over the world.</summary>
public enum Menu
{
    None,
    Pause,
    Options,
    Packs,
}

/// <summary>
/// The menu Escape opens, which holds the world still while it is open, as Minecraft's does when
/// played alone, and its options of the player's own, drawn as the game's own buttons and sliders.
/// </summary>
/// <remarks>
/// The options are the ones a player sets, the render distance, the field of view, the mouse's
/// sensitivity, the interface's scale, whether full rows of the HUD fade, how far corners darken, how
/// much light bounces and the resource packs. What the developer reads and sets stays in the settings
/// window F3 opens.
/// <para>
/// The resource packs' screen lists the packs that are on, the first over the rest, numbered, then
/// the others in the game's folder. A click turns a pack on over the rest or off, the arrow beside a
/// pack that is on moves it up a place, and Done saves the list and draws the game with it, as
/// Minecraft's screen of packs does. Under the list it says how the fetch of the default packs
/// stands, with a switch that stops it while it runs and begins it where a default pack is missing.
/// </para>
/// </remarks>
public sealed class Menus
{
    private const int ButtonWidth = 200, ButtonHeight = 20, PackRows = 6;

    // The packs' screen's list while it is open, the packs on first, the packs in the folder, and
    // how many rows the list is scrolled past.
    private List<string>? _chosen;
    private IReadOnlyList<string> _available = [];
    private int _packScroll;
    // The packs on as the screen last read them, so a fetch that turns the default packs on while it
    // is open shows them, and whether a default pack is missing as the fetch last stood, worked out
    // as it changes rather than each frame, since it reads every default pack's bytes.
    private IReadOnlyList<string>? _seen;
    private FetchState? _fetchSeen;
    private bool _missing;

    /// <summary>The menu open, or none.</summary>
    public Menu Open { get; set; }

    /// <summary>Whether the player asked to quit, which ends the game's loop once the frame is drawn.</summary>
    public bool QuitAsked { get; private set; }

    /// <summary>Goes back a menu, from the options to the menu and from the menu to the game.</summary>
    public void Back()
    {
        _chosen = null;
        Open = Open switch { Menu.Packs => Menu.Options, Menu.Options => Menu.Pause, _ => Menu.None };
    }

    /// <summary>Draws the menu open over the darkened world and answers its buttons and sliders.</summary>
    public void Draw(VoxelGame game)
    {
        if (Open == Menu.None) return;
        Gui.Backdrop();
        var s = Gui.Scale;
        int width = ButtonWidth * s, height = ButtonHeight * s, x = (GetScreenWidth() - width) / 2;
        var top = GetScreenHeight() / 4;
        Rectangle Row(int row) => new(x, top + (row * 24 + 16) * s, width, height);

        if (Open == Menu.Pause)
        {
            Title("Game Menu", top);
            if (Gui.Button("Back to Game", Row(0))) Open = Menu.None;
            if (Gui.Button("Options...", Row(1))) Open = Menu.Options;
            if (Gui.Button("Save the World", Row(2))) game.SaveWorld();
            if (Gui.Button("Save and Quit", Row(3)))
            {
                game.SaveWorld();
                QuitAsked = true;
            }
            return;
        }
        if (Open == Menu.Packs)
        {
            DrawPacks(game, s, height);
            return;
        }

        // The options in two columns of 150, as Minecraft lays its options out, and Done under them.
        Rectangle Cell(int column, int row) => new(GetScreenWidth() / 2 - 155 * s + column * 160 * s, top + (row * 24 + 16) * s, 150 * s, height);
        Title("Options", top);
        game.RenderDistance = (int)MathF.Round(Gui.Slider($"Render Distance: {game.RenderDistance}", Cell(0, 0), game.RenderDistance, 2, 16, out _));
        game.Player.FieldOfView = MathF.Round(Gui.Slider($"FOV: {game.Player.FieldOfView:0}", Cell(1, 0), game.Player.FieldOfView, 30, 110, out _));
        // The sensitivity in hundredths of the game's own, 100 the 0.0025 radians a pixel it starts at.
        var sensitivity = Gui.Slider($"Sensitivity: {game.Player.Sensitivity / 0.0025f * 100:0}%", Cell(0, 1), game.Player.Sensitivity / 0.0025f * 100, 10, 200, out _);
        game.Player.Sensitivity = MathF.Round(sensitivity) / 100 * 0.0025f;
        if (Gui.Button($"GUI Scale: {(Gui.Chosen == 0 ? "Auto" : Gui.Chosen.ToString())}", Cell(1, 1))) Gui.Chosen = (Gui.Chosen + 1) % 5;
        if (Gui.Button($"Fading HUD: {(game.Fade.Enabled ? "On" : "Off")}", Cell(0, 2))) game.Fade.Enabled = !game.Fade.Enabled;
        var corners = Gui.Slider($"Corner Shade: {game.Renderer.CornerStrength * 100:0}%", Cell(1, 2), game.Renderer.CornerStrength * 100, 0, 100, out var letGo) / 100;
        game.Renderer.CornerStrength = MathF.Round(corners * 20) / 20;
        if (letGo) game.World.ReshadeAll();
        if (Gui.Button($"Light: {game.Light.Quality}", Cell(0, 3))) game.Light.NextQuality();
        if (Gui.Button("Resource Packs...", Cell(1, 3)))
        {
            (_chosen, _fetchSeen, _packScroll) = (null, null, 0);
            Open = Menu.Packs;
        }
        if (Gui.Button("Done", Row(4))) Open = Menu.Pause;
    }

    // The packs on and the packs in the folder in rows the wheel scrolls, the folder's place under
    // them, and Done, higher on the screen than the other menus to hold them.
    private void DrawPacks(VoxelGame game, int s, int height)
    {
        var defaults = game.Defaults;
        if (_chosen is null || !ReferenceEquals(_seen, game.PackEntries))
            (_chosen, _seen, _available) = ([.. game.PackEntries], game.PackEntries, PackList.Default.Available());
        if (_fetchSeen != defaults.State)
        {
            _fetchSeen = defaults.State;
            _missing = defaults.State != FetchState.Fetching && defaults.Packs.Count > 0 && defaults.Missing(PackList.Default);
            _available = PackList.Default.Available();
        }
        var top = GetScreenHeight() / 8;
        var width = Math.Min(320 * s, GetScreenWidth() - 40 * s);
        var x = (GetScreenWidth() - width) / 2;
        Rectangle Row(int row, int from = 0, int across = -1) => new(x + from, top + (row * 24 + 16) * s, across < 0 ? width - from : across, height);
        Title("Resource Packs", top);

        List<string> rows = [.. _chosen, .. _available.Where(a => !_chosen.Contains(a))];
        _packScroll = Math.Clamp(_packScroll - Math.Sign(GetMouseWheelMove()), 0, Math.Max(0, rows.Count - PackRows));
        for (int row = 0; row < PackRows && row + _packScroll < rows.Count; row++)
        {
            var entry = rows[row + _packScroll];
            var place = _chosen.IndexOf(entry);
            var label = Fitted(place >= 0 ? $"{place + 1}. {PackList.Shown(entry)}" : PackList.Shown(entry), width - 36 * s);
            if (Gui.Button(label, Row(row, 0, width - 24 * s)))
            {
                if (place >= 0) _chosen.RemoveAt(place);
                else _chosen.Insert(0, entry);
            }
            if (place > 0 && Gui.Button("^", Row(row, width - 20 * s, 20 * s))) (_chosen[place - 1], _chosen[place]) = (_chosen[place], _chosen[place - 1]);
        }
        if (rows.Count == 0) Gui.Centered("No packs yet", Row(0), new Color(170, 170, 170));
        var fetching = defaults.State == FetchState.Fetching;
        var said = fetching ? $"Fetching the default packs from Modrinth, {defaults.Fetched} of {defaults.Packs.Count} in"
            : defaults.State == FetchState.Failed ? $"The default packs were not fetched: {defaults.Problem}"
            : $"Packs go in {PackList.Default.Folder}";
        Gui.Centered(Fitted(said, width), Row(PackRows), new Color(170, 170, 170));
        // The switch beside Done while the fetch runs or a default pack is missing.
        var half = (width - 10 * s) / 2;
        var missing = !fetching && _missing;
        if (fetching || missing)
        {
            if (Gui.Button(fetching ? "Stop Fetching" : "Fetch Default Packs", Row(PackRows + 1, 0, half)))
            {
                if (fetching) game.StopDefaults();
                else game.FetchDefaults();
            }
        }
        if (Gui.Button("Done", fetching || missing ? Row(PackRows + 1, width - half, half) : Row(PackRows + 1)))
        {
            game.ChoosePacks(_chosen);
            Back();
        }
    }

    // A line cut short with dots where it is wider than a width of the screen.
    private static string Fitted(string text, int width)
    {
        if (Gui.TextWidth(text) <= width) return text;
        while (text.Length > 1 && Gui.TextWidth(text + "...") > width) text = text[..^1];
        return text + "...";
    }

    private static void Title(string text, int top) => Gui.Text(text, (GetScreenWidth() - Gui.TextWidth(text)) / 2, top - 4 * Gui.Scale, Color.White);
}
