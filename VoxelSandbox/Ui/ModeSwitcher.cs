using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The switcher F3 held with F4 opens, a row of the four modes' icons in boxes under the chosen
/// mode's name, as Minecraft's is. It opens on the mode played before the one now, or where there
/// is none on survival from creative and on creative from the others; each press of F4 while F3 is
/// held steps it to the next mode, and letting F3 go plays the one chosen.
/// </summary>
public sealed class ModeSwitcher
{
    // Minecraft's switcher in its pixels, a panel of 125 by 75, boxes of 26 a side 31 apart and its
    // icons of 16, drawn two screen pixels to one.
    private const int Scale = 2, Box = 26 * Scale, Pitch = 31 * Scale, PanelWidth = 125 * Scale, PanelHeight = 75 * Scale;

    private static readonly GameMode[] Order = [GameMode.Creative, GameMode.Survival, GameMode.Adventure, GameMode.Spectator];

    // Adventure's map and spectator's eye, which no item of the game's is, painted here sixteen
    // pixels a side as its icons are, and a pixel of each in its color or none.
    private static readonly Color?[] Map = Paint((x, y) =>
        x is 0 or 15 || y is 0 or 15 ? null
        : x is 1 or 14 || y is 1 or 14 ? new Color(122, 90, 56)
        : (x - 5) * (x - 5) + (y - 5) * (y - 5) < 8 || x > 9 && y > 8 && y < 13 ? new Color(96, 146, 64)
        : x > 7 && x < 12 && y > 2 && y < 7 || x > 2 && x < 7 && y > 9 && y < 13 ? new Color(70, 112, 196)
        : new Color(226, 210, 164));

    private static readonly Color?[] Eye = Paint((x, y) =>
    {
        var d = MathF.Sqrt((x + 0.5f - 8) * (x + 0.5f - 8) + (y + 0.5f - 8) * (y + 0.5f - 8));
        if (d > 6.5f) return null;
        if (d > 5.5f) return new Color(18, 62, 50);
        if (x is 5 or 6 && y is 4 or 5) return new Color(176, 236, 204);
        return d < 1.6f ? new Color(12, 22, 18) : d < 3.2f ? new Color(40, 122, 92) : new Color(72, 170, 132);
    });

    /// <summary>Whether the switcher is open, as while F3 is held after F4 was pressed.</summary>
    public bool Open { get; private set; }

    /// <summary>The mode the switcher has chosen, which letting F3 go plays.</summary>
    public GameMode Chosen { get; private set; }

    /// <summary>A press of F4 while F3 is held, which opens the switcher on its first choice or steps it to the next mode.</summary>
    public void Step(GameMode current, GameMode? previous)
    {
        if (Open)
        {
            Chosen = Chosen.Next();
            return;
        }
        Open = true;
        Chosen = previous is { } before && before != current ? before : current == GameMode.Creative ? GameMode.Survival : GameMode.Creative;
    }

    /// <summary>F3 let go, which closes the switcher and gives the mode chosen, or null where it was not open.</summary>
    public GameMode? Release()
    {
        if (!Open) return null;
        Open = false;
        return Chosen;
    }

    /// <summary>Draws the switcher over the middle of the window where it is open.</summary>
    public void Draw()
    {
        if (!Open) return;
        int left = (GetScreenWidth() - PanelWidth) / 2, top = GetScreenHeight() / 2 - PanelHeight / 2 - 27 * Scale;
        DrawRectangle(left, top, PanelWidth, PanelHeight, new Color(16, 16, 20, 200));
        DrawRectangleLines(left, top, PanelWidth, PanelHeight, new Color(255, 255, 255, 60));
        Shadowed(Chosen.Title(), top + 6 * Scale, Color.White);

        var first = left + (PanelWidth - (Order.Length - 1) * Pitch - Box) / 2;
        var boxTop = top + 20 * Scale;
        for (int i = 0; i < Order.Length; i++)
        {
            var x = first + i * Pitch;
            DrawRectangle(x, boxTop, Box, Box, new Color(60, 60, 64, 220));
            DrawRectangleLines(x, boxTop, Box, Box, new Color(0, 0, 0, 200));
            Icon(Order[i], x + (Box - 16 * Scale) / 2, boxTop + (Box - 16 * Scale) / 2);
            if (Order[i] == Chosen)
                for (int ring = 0; ring < Scale; ring++)
                    DrawRectangleLines(x - Scale + ring, boxTop - Scale + ring, Box + 2 * (Scale - ring), Box + 2 * (Scale - ring), Color.White);
        }

        // "[ F4 ]" in Minecraft's aqua, then "Next" in white, under the boxes.
        const string key = "[ F4 ]", next = " Next";
        var width = MeasureText(key + next, 20);
        var textX = (GetScreenWidth() - width) / 2;
        var textY = boxTop + Box + 6 * Scale;
        DrawText(key, textX + 2, textY + 2, 20, new Color(0, 0, 0, 160));
        DrawText(key, textX, textY, 20, new Color(85, 255, 255));
        DrawText(next, textX + MeasureText(key, 20) + 2, textY + 2, 20, new Color(0, 0, 0, 160));
        DrawText(next, textX + MeasureText(key, 20), textY, 20, Color.White);
    }

    // A mode's icon, creative's grass block and survival's iron sword from the items' atlas, and
    // adventure's map and spectator's eye painted here.
    private static void Icon(GameMode mode, int x, int y)
    {
        var item = mode switch { GameMode.Creative => Items.Find("grass"), GameMode.Survival => Items.Find("iron_sword"), _ => null };
        if (item is not null)
        {
            DrawTexturePro(Icons.Texture(), Icons.Source(item), new Rectangle(x, y, 16 * Scale, 16 * Scale), Vector2.Zero, 0, Color.White);
            return;
        }
        var pixels = mode == GameMode.Adventure ? Map : Eye;
        for (int i = 0; i < pixels.Length; i++)
            if (pixels[i] is { } color)
                DrawRectangle(x + i % 16 * Scale, y + i / 16 * Scale, Scale, Scale, color);
    }

    private static void Shadowed(string text, int y, Color color)
    {
        var x = (GetScreenWidth() - MeasureText(text, 20)) / 2;
        DrawText(text, x + 2, y + 2, 20, new Color(0, 0, 0, 160));
        DrawText(text, x, y, 20, color);
    }

    private static Color?[] Paint(Func<int, int, Color?> pixel)
    {
        var pixels = new Color?[256];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = pixel(i % 16, i / 16);
        return pixels;
    }
}
