using System.Globalization;
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
    Video,
    Render,
    PostProcessing,
    SkyAndVisuals,
    SkyColors,
    Particles,
    Sounds,
    SkyColor,
    Debug,
    World,
    NewWorld,
}

/// <summary>
/// The menu Escape opens, which holds the world still while it is open, as Minecraft's does when
/// played alone, and its options of the player's own, drawn as the game's own buttons and sliders.
/// </summary>
/// <remarks>
/// The options are the field of view, the mouse's sensitivity, the interface's scale and whether full
/// rows of the HUD fade, with buttons to the screens of the window's settings, the render's, the
/// effects over the frame, the sky and its colors, and the resource packs, each of
/// <see cref="OptionScreens"/> drawn as two columns of sliders and buttons with Done under them.
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
    // The saves listed as the World screen opened and how far its list is scrolled, and the New World
    // screen's kind and the seed typed.
    private IReadOnlyList<(string Name, string Kind, int Seed)> _saves = [];
    private int _worldScroll, _kind;
    private TextEdit _seed = new("");
    // The packs on as the screen last read them, so a fetch that turns the default packs on while it
    // is open shows them, and whether a default pack is missing as the fetch last stood, worked out
    // as it changes rather than each frame, since it reads every default pack's bytes.
    private IReadOnlyList<string>? _seen;
    private FetchState? _fetchSeen;
    private bool _missing;

    private OptionScreens? _screens;
    // The sky's color the color's screen sets.
    private int _color;

    private Menu _open;
    private bool _stated;

    /// <summary>The menu open, or none, the engine's state of it following, so the behaviors it gates follow it.</summary>
    public Menu Open
    {
        get => _open;
        set
        {
            _open = value;
            if (_stated) SetState(value);
        }
    }

    /// <summary>Makes the menu open a state of the engine's, which it follows from then on.</summary>
    public void MakeState()
    {
        E3D.AddState(_open);
        _stated = true;
    }

    /// <summary>The options' screens of the game's settings, made the first time they are asked for.</summary>
    public OptionScreens Screens(VoxelGame game) => _screens ??= new OptionScreens(game);

    /// <summary>Whether the player asked to quit, which ends the game's loop once the frame is drawn.</summary>
    public bool QuitAsked { get; private set; }

    /// <summary>Goes back a menu, from the options to the menu and from the menu to the game.</summary>
    public void Back()
    {
        _chosen = null;
        Open = Open switch
        {
            Menu.Packs or Menu.Video or Menu.Render or Menu.PostProcessing or Menu.SkyAndVisuals or Menu.Sounds or Menu.Debug => Menu.Options,
            Menu.SkyColor => Menu.SkyColors,
            Menu.SkyColors or Menu.Particles => Menu.SkyAndVisuals,
            Menu.Options or Menu.World => Menu.Pause,
            Menu.NewWorld => Menu.World,
            _ => Menu.None,
        };
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
            if (Gui.Button("World...", Row(2)))
            {
                (_saves, _worldScroll) = (game.Transient ? [] : WorldSave.Saved(), 0);
                Open = Menu.World;
            }
            if (Gui.Button("Save the World", Row(3))) game.SaveWorld();
            if (Gui.Button("Save and Quit", Row(4)))
            {
                game.SaveWorld();
                QuitAsked = true;
            }
            return;
        }
        if (Open == Menu.World)
        {
            DrawWorlds(game, s, height);
            return;
        }
        if (Open == Menu.NewWorld)
        {
            DrawNewWorld(game, s, height);
            return;
        }
        if (Open == Menu.Packs)
        {
            DrawPacks(game, s, height);
            return;
        }
        var screens = Screens(game);
        switch (Open)
        {
            case Menu.Video:
                DrawScreen(screens.Video, s, height);
                return;
            case Menu.Render:
                DrawScreen(screens.Render, s, height);
                return;
            case Menu.PostProcessing:
                DrawScreen(screens.PostProcessing, s, height);
                return;
            case Menu.SkyAndVisuals:
                DrawScreen(screens.SkyAndVisuals, s, height);
                return;
            case Menu.SkyColors:
                DrawSkyColors(screens, s, height);
                return;
            case Menu.Particles:
                DrawScreen(screens.Particles, s, height);
                return;
            case Menu.Sounds:
                DrawScreen(screens.Sounds, s, height);
                return;
            case Menu.SkyColor:
                DrawSkyColor(screens, s, height);
                return;
            case Menu.Debug:
                DrawScreen(screens.Debug, s, height);
                return;
        }

        var general = screens.General.Options;
        // The settings' rows and the four rows of buttons under them, placed as the other screens are,
        // so Done stays in a short window.
        top = TopFor((general.Count + 1) / 2 + 4);
        Title("Options", top);
        for (int i = 0; i < general.Count; i++) Draw(general[i], Cell(i % 2, i / 2, top));
        // The buttons to the other screens in rows of two under the settings.
        var below = (general.Count + 1) / 2;
        if (Gui.Button("Video Settings...", Cell(0, below, top))) Open = Menu.Video;
        if (Gui.Button("Render Settings...", Cell(1, below, top))) Open = Menu.Render;
        if (Gui.Button("Post Processing...", Cell(0, below + 1, top))) Open = Menu.PostProcessing;
        if (Gui.Button("Sky and Visuals...", Cell(1, below + 1, top))) Open = Menu.SkyAndVisuals;
        if (Gui.Button("Music and Sounds...", Cell(0, below + 2, top))) Open = Menu.Sounds;
        if (Gui.Button("Resource Packs...", Cell(1, below + 2, top)))
        {
            (_chosen, _fetchSeen, _packScroll) = (null, null, 0);
            Open = Menu.Packs;
        }
        if (Gui.Button("Debug Settings...", Cell(0, below + 3, top))) Open = Menu.Debug;
        if (Gui.Button("Done", Row(below + 4))) Open = Menu.Pause;
    }

    // A cell of two columns of 150, as Minecraft lays its options out.
    private static Rectangle Cell(int column, int row, int top)
    {
        var s = Gui.Scale;
        return new(GetScreenWidth() / 2 - 155 * s + column * 160 * s, top + (row * 24 + 16) * s, 150 * s, ButtonHeight * s);
    }

    // The top of a screen of so many rows and Done under them, a quarter down the window where they
    // fit under it and higher where they would not.
    private static int TopFor(int rows) => Math.Max(20 * Gui.Scale, Math.Min(GetScreenHeight() / 4, GetScreenHeight() - ((rows + 1) * 24 + 24) * Gui.Scale));

    // A screen's settings in two columns, the buttons to the screens under it after them, and Done
    // under all, which goes back a screen.
    private void DrawScreen(OptionScreen screen, int s, int height)
    {
        var cells = screen.Options.Count + screen.Links.Count;
        var rows = (cells + 1) / 2;
        var top = TopFor(rows);
        Title(screen.Title, top);
        for (int i = 0; i < screen.Options.Count; i++) Draw(screen.Options[i], Cell(i % 2, i / 2, top));
        for (int i = 0; i < screen.Links.Count; i++)
        {
            var at = screen.Options.Count + i;
            if (Gui.Button(screen.Links[i].Label, Cell(at % 2, at / 2, top))) Open = screen.Links[i].Opens;
        }
        var width = ButtonWidth * s;
        if (Gui.Button("Done", new Rectangle((GetScreenWidth() - width) / 2, top + (rows * 24 + 16) * s, width, height))) Back();
    }

    // A setting as a slider or a button. A slider sets its value only as it is moved, so a value
    // between its steps, as the hour the day has reached, stays as it is while the screen is shown.
    private static void Draw(Option option, Rectangle box)
    {
        if (option.Choices is not null)
        {
            if (Gui.Button(option.Label, box)) option.Next();
            return;
        }
        var value = Gui.Slider(option.Label, box, option.Value, option.Low, option.High, out var letGo);
        if (value != option.Value) option.Change(value);
        if (letGo) option.Settled?.Invoke();
    }

    // A button to each of the sky's colors in two columns, each opening the color's screen, then one
    // that puts every color back as it started, and Done.
    private void DrawSkyColors(OptionScreens screens, int s, int height)
    {
        var colors = screens.Colors;
        var rows = (colors.Count + 2) / 2;
        var top = TopFor(rows);
        Title("Sky Colors", top);
        for (int i = 0; i < colors.Count; i++)
        {
            if (Gui.Button(colors[i].Name + "...", Cell(i % 2, i / 2, top)))
            {
                _color = i;
                Open = Menu.SkyColor;
            }
        }
        if (Gui.Button("Reset All", Cell(colors.Count % 2, colors.Count / 2, top))) screens.SkyColors.Reset();
        var width = ButtonWidth * s;
        if (Gui.Button("Done", new Rectangle((GetScreenWidth() - width) / 2, top + (rows * 24 + 16) * s, width, height))) Back();
    }

    // A color's red, green and blue as sliders across the screen, the color itself under them, and
    // Reset, which puts the color back as it started, beside Done.
    private void DrawSkyColor(OptionScreens screens, int s, int height)
    {
        var color = screens.Colors[_color];
        var top = TopFor(5);
        int width = 310 * s, x = (GetScreenWidth() - width) / 2;
        Rectangle Line(int row) => new(x, top + (row * 24 + 16) * s, width, height);
        Title(color.Name, top);
        var channels = color.Channels(screens.SkyColors);
        for (int i = 0; i < channels.Count; i++) Draw(channels[i], Line(i));
        var levels = color.Levels;
        var swatch = Line(3);
        DrawRectangle((int)swatch.X, (int)swatch.Y, (int)swatch.Width, (int)swatch.Height, Color.White);
        DrawRectangle((int)swatch.X + s, (int)swatch.Y + s, (int)swatch.Width - 2 * s, (int)swatch.Height - 2 * s,
            new Color((byte)Math.Clamp(levels.X, 0, 255), (byte)Math.Clamp(levels.Y, 0, 255), (byte)Math.Clamp(levels.Z, 0, 255)));
        if (Gui.Button("Reset", Cell(0, 4, top)))
        {
            var fresh = OptionScreens.SkyColorsOf(new SkyColors())[_color];
            color.Set(fresh.Get());
            screens.SkyColors.Changed();
        }
        if (Gui.Button("Done", Cell(1, 4, top))) Back();
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

    // The saves to open, the one played last first, six at a time under the wheel, the open one
    // marked, and the button to the New World screen under them.
    private void DrawWorlds(VoxelGame game, int s, int height)
    {
        const int Shown = 6;
        var top = TopFor(Shown + 1);
        Title("World", top);
        Rectangle Wide(int row) => new(GetScreenWidth() / 2 - 155 * s, top + (row * 24 + 16) * s, 310 * s, height);
        var gray = new Color(160, 160, 160, 255);
        if (game.Transient) Gui.Centered("A transient run keeps no saves", Wide(0), gray);
        else if (_saves.Count == 0) Gui.Centered("No world is saved yet", Wide(0), gray);
        _worldScroll = Math.Clamp(_worldScroll - (int)GetMouseWheelMove(), 0, Math.Max(0, _saves.Count - Shown));
        for (int i = 0; i < Math.Min(Shown, _saves.Count - _worldScroll); i++)
        {
            var (name, kind, seed) = _saves[_worldScroll + i];
            var open = name == game.WorldName;
            if (Gui.Button($"{name}, {kind} of seed {seed}{(open ? ", open" : "")}", Wide(i)) && !open)
            {
                game.NextWorld = (kind, seed, name);
                Open = Menu.None;
            }
        }
        if (Gui.Button("New World...", Wide(Shown))) OpenNewWorld();
        if (Gui.Button("Done", Wide(Shown + 1))) Open = Menu.Pause;
    }

    /// <summary>Opens the screen of a new world, its seed's field empty and holding the keyboard, so a seed is typed at once.</summary>
    public void OpenNewWorld()
    {
        (_kind, _seed) = (0, new TextEdit("") { Focused = true });
        Open = Menu.NewWorld;
    }

    // A world begun of a kind and a seed, typed or offered at random, as Minecraft's screen of a new
    // world begins one.
    private void DrawNewWorld(VoxelGame game, int s, int height)
    {
        var top = TopFor(6);
        Title("Create New World", top);
        Rectangle Wide(int row) => new(GetScreenWidth() / 2 - 155 * s, top + (row * 24 + 16) * s, 310 * s, height);
        var kinds = Generators.Kinds;
        if (Gui.Button($"World Type: {char.ToUpperInvariant(kinds[_kind][0])}{kinds[_kind][1..]}", Wide(0))) _kind = (_kind + 1) % kinds.Length;
        var gray = new Color(160, 160, 160, 255);
        Gui.Text("Seed for the world, a random one where blank", (int)Wide(1).X, (int)Wide(1).Y + 6 * s, gray);
        // Enter in the field begins the world, as the button below does.
        var entered = Gui.TextField(ref _seed, Wide(2), 32);
        if (Gui.Button("Random Seed", Wide(3))) _seed = new TextEdit(Random.Shared.Next().ToString(CultureInfo.InvariantCulture));
        if (Gui.Button("Create New World", Wide(4)) || entered) Begin(game, kinds[_kind], _seed.Text);
        if (Gui.Button("Cancel", Wide(5))) Open = Menu.World;
    }

    /// <summary>
    /// Begins a world of a kind with the seed a text gives, the world open saved and left at the
    /// start of the next frame, or opens the one saved of that kind and seed, as the Create New World
    /// button does.
    /// </summary>
    public void Begin(VoxelGame game, string kind, string seedText)
    {
        game.NextWorld = (kind, SeedOf(seedText), null);
        Open = Menu.None;
    }

    /// <summary>
    /// The seed a text gives, the whole number it reads as, a longer number folded into one, or other
    /// text hashed as Minecraft hashes a seed's words, and a random one for none.
    /// </summary>
    public static int SeedOf(string text)
    {
        text = text.Trim();
        if (text.Length == 0) return Random.Shared.Next();
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var whole)) return whole;
        if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)) return (int)(number ^ (number >>> 32));
        var hash = 0;
        foreach (var c in text) hash = unchecked(31 * hash + c);
        return hash;
    }

    private static void Title(string text, int top) => Gui.Text(text, (GetScreenWidth() - Gui.TextWidth(text)) / 2, top - 4 * Gui.Scale, Color.White);
}
