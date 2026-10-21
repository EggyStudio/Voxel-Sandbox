using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>Which of the game's menus is open over the world.</summary>
public enum Menu
{
    None,
    Pause,
    Options,
}

/// <summary>
/// The menu Escape opens, which holds the world still while it is open, as Minecraft's does when
/// played alone, and its options of the player's own, drawn as the game's own buttons and sliders.
/// </summary>
/// <remarks>
/// The options are the ones a player sets, the render distance, the field of view, the mouse's
/// sensitivity, the interface's scale, whether full rows of the HUD fade, how far corners darken and
/// how much light bounces. What the developer reads and sets stays in the settings window F3 opens.
/// </remarks>
public sealed class Menus
{
    private const int ButtonWidth = 200, ButtonHeight = 20;

    /// <summary>The menu open, or none.</summary>
    public Menu Open { get; set; }

    /// <summary>Whether the player asked to quit, which ends the game's loop once the frame is drawn.</summary>
    public bool QuitAsked { get; private set; }

    /// <summary>Goes back a menu, from the options to the menu and from the menu to the game.</summary>
    public void Back() => Open = Open == Menu.Options ? Menu.Pause : Menu.None;

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
        if (Gui.Button("Done", Row(4))) Open = Menu.Pause;
    }

    private static void Title(string text, int top) => Gui.Text(text, (GetScreenWidth() - Gui.TextWidth(text)) / 2, top - 4 * Gui.Scale, Color.White);
}
