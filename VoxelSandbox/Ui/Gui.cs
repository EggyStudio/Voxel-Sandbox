using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The interface's drawing, in its own pixels at a whole number of the screen's, as Minecraft draws
/// its interface: the pictures of <see cref="GuiArt"/>, panels stretched between their edges, text
/// with its shadow, an item's icon with its count and its wear, and a tooltip.
/// </summary>
/// <remarks>
/// The scale is the largest whole number at which 320 by 240 of the interface's pixels fit the
/// window, as Minecraft's automatic scale is, 3 at 1280 by 720, unless <see cref="Chosen"/> sets
/// one. Text is the engine's own font at ten of its pixels for each of the interface's eight.
/// </remarks>
public static class Gui
{
    /// <summary>The scale chosen in the settings, or 0 for the largest that fits.</summary>
    public static int Chosen { get; set; }

    /// <summary>Screen pixels to one of the interface's.</summary>
    public static int Scale
    {
        get
        {
            var fits = Math.Max(1, Math.Min(GetScreenWidth() / 320, GetScreenHeight() / 240));
            return Chosen > 0 ? Math.Min(Chosen, fits) : fits;
        }
    }

    /// <summary>The height of a line of text in screen pixels, nine of the interface's.</summary>
    public static int LineHeight => 9 * Scale;

    /// <summary>A picture at a place on the screen, at the interface's scale, tinted.</summary>
    public static void Sprite(GuiSprite sprite, int x, int y, Color? tint = null) =>
        DrawTexturePro(GuiArt.Texture(), sprite.Source, new Rectangle(x, y, sprite.Width * Scale, sprite.Height * Scale), Vector2.Zero, 0, tint ?? Color.White);

    /// <summary>A part of a picture from its left, as an arrow or a flame fills, at the interface's scale.</summary>
    public static void SpritePart(GuiSprite sprite, int x, int y, int width, int height, int fromY = 0)
    {
        if (width <= 0 || height <= 0) return;
        DrawTexturePro(GuiArt.Texture(), new Rectangle(sprite.X, sprite.Y + fromY, width, height), new Rectangle(x, y + fromY * Scale, width * Scale, height * Scale), Vector2.Zero, 0, Color.White);
    }

    /// <summary>A picture stretched over a box of the screen between its edges, which keep their size at the interface's scale.</summary>
    public static void Stretched(GuiSprite sprite, Rectangle box, Color? tint = null)
    {
        // The engine's nine-patch keeps the edges at their size in the pixels it draws in, so the box
        // is drawn in the interface's own pixels under a 2D camera scaled to the interface.
        var info = new NPatchInfo(sprite.Source, sprite.Border, sprite.Border, sprite.Border, sprite.Border);
        var scale = Scale;
        BeginBox(box, scale, out var inner);
        DrawTextureNPatch(GuiArt.Texture(), info, inner, Vector2.Zero, 0, tint ?? Color.White);
        EndBox();
    }

    /// <summary>Text at a place on the screen with its shadow a pixel of the interface's below and to the right, as Minecraft's is.</summary>
    public static void Text(string text, int x, int y, Color color, bool shadow = true)
    {
        var size = 10 * Scale;
        if (shadow) DrawText(text, x + Scale, y + Scale, size, new Color((byte)(color.R / 4), (byte)(color.G / 4), (byte)(color.B / 4), color.A));
        DrawText(text, x, y, size, color);
    }

    /// <summary>How wide a line of text is on the screen.</summary>
    public static int TextWidth(string text) => MeasureText(text, 10 * Scale);

    /// <summary>
    /// An item's icon sixteen of the interface's pixels across at a place, a worn tool's bar of uses
    /// left under it and its count at its lower right where it is more than one.
    /// </summary>
    public static void Item(ItemStack stack, int x, int y, Color? tint = null)
    {
        var s = Scale;
        DrawTexturePro(Icons.Texture(), Icons.Source(stack.Item), new Rectangle(x, y, 16 * s, 16 * s), Vector2.Zero, 0, tint ?? Color.White);
        if (stack.Wear > 0 && stack.Item.Durability > 0)
        {
            var left = 1 - (float)stack.Wear / stack.Item.Durability;
            DrawRectangle(x + 2 * s, y + 13 * s, 13 * s, 2 * s, Color.Black);
            DrawRectangle(x + 2 * s, y + 13 * s, (int)(13 * s * left), s, new Color((byte)(255 * (1 - left)), (byte)(255 * left), 30));
        }
        if (stack.Count <= 1) return;
        var text = stack.Count.ToString();
        Text(text, x + 17 * s - TextWidth(text), y + 9 * s, Color.White);
    }

    /// <summary>A tooltip of lines beside the pointer, its first line white and the rest gray, kept inside the window.</summary>
    public static void Tooltip(IReadOnlyList<string> lines)
    {
        if (lines.Count == 0) return;
        var s = Scale;
        var mouse = GetMousePosition();
        int width = lines.Max(TextWidth) + 8 * s, height = lines.Count * LineHeight + 6 * s;
        int x = (int)mouse.X + 12 * s, y = (int)mouse.Y - 12 * s;
        if (x + width > GetScreenWidth()) x = (int)mouse.X - 12 * s - width;
        y = Math.Clamp(y, 0, Math.Max(0, GetScreenHeight() - height));
        Stretched(GuiArt.Tooltip, new Rectangle(x, y, width, height));
        for (int i = 0; i < lines.Count; i++)
            Text(lines[i], x + 4 * s, y + 4 * s + i * LineHeight, i == 0 ? Color.White : new Color(170, 170, 170));
    }

    /// <summary>Whether the pointer is within a box of the screen.</summary>
    public static bool Over(Rectangle box)
    {
        var mouse = GetMousePosition();
        return mouse.X >= box.X && mouse.Y >= box.Y && mouse.X < box.X + box.Width && mouse.Y < box.Y + box.Height;
    }

    // A 2D camera putting the interface's pixel at the box's corner and scaling them to the screen's.
    private static void BeginBox(Rectangle box, int scale, out Rectangle inner)
    {
        BeginMode2D(new Camera2D(new Vector2(box.X, box.Y), Vector2.Zero, 0, scale));
        inner = new Rectangle(0, 0, box.Width / scale, box.Height / scale);
    }

    private static void EndBox() => EndMode2D();
}
