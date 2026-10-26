using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The interface's drawing, in its own pixels at a whole number of the screen's, as Minecraft draws
/// its interface, of the pictures of <see cref="GuiArt"/>, panels stretched between their edges,
/// text with its shadow, an item's icon with its count and its wear, and a tooltip.
/// </summary>
/// <remarks>
/// The scale is the largest whole number at which 320 by 240 of the interface's pixels fit the
/// window, as Minecraft's automatic scale is, 3 at 1280 by 720, unless <see cref="Chosen"/> sets
/// one. Text is the font of the player's resource packs where one has it, and the engine's own font
/// at ten of its pixels for each of the interface's eight otherwise. A picture a pack has, as
/// <see cref="PackGui"/> reads it, is drawn in place of the game's own.
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
    public static void Sprite(GuiSprite sprite, int x, int y, Color? tint = null)
    {
        if (PackGui.For(sprite) is { } pack)
            PackGui.Draw(pack.Picture, new Rectangle(x, y, pack.Width * Scale, pack.Height * Scale), Scale, tint);
        else
            DrawTexturePro(GuiArt.Texture(), sprite.Source, new Rectangle(x, y, sprite.Width * Scale, sprite.Height * Scale), Vector2.Zero, 0, tint ?? Color.White);
    }

    /// <summary>A part of a picture from its left, as an arrow or a flame fills, at the interface's scale.</summary>
    public static void SpritePart(GuiSprite sprite, int x, int y, int width, int height, int fromY = 0)
    {
        if (width <= 0 || height <= 0) return;
        DrawTexturePro(GuiArt.Texture(), new Rectangle(sprite.X, sprite.Y + fromY, width, height), new Rectangle(x, y + fromY * Scale, width * Scale, height * Scale), Vector2.Zero, 0, Color.White);
    }

    /// <summary>A picture stretched over a box of the screen between its edges, which keep their size at the interface's scale.</summary>
    public static void Stretched(GuiSprite sprite, Rectangle box, Color? tint = null)
    {
        if (PackGui.For(sprite) is { } pack)
        {
            PackGui.Draw(pack.Picture, box, Scale, tint);
            return;
        }
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
        var s = Scale;
        var dark = new Color((byte)(color.R / 4), (byte)(color.G / 4), (byte)(color.B / 4), color.A);
        if (PackGui.Font is { } font)
        {
            if (shadow) font.Draw(text, x + s, y + s, s, dark);
            font.Draw(text, x, y, s, color);
            return;
        }
        if (shadow) DrawText(text, x + s, y + s, 10 * s, dark);
        DrawText(text, x, y, 10 * s, color);
    }

    /// <summary>How wide a line of text is on the screen.</summary>
    public static int TextWidth(string text) => PackGui.Font is { } font ? font.Width(text) * Scale : MeasureText(text, 10 * Scale);

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
        // A pack's font at Minecraft's place, nine below the icon's top, and the engine's, which sits
        // lower in its line, two higher.
        Text(text, x + 17 * s - TextWidth(text), y + (PackGui.Font is null ? 7 : 9) * s, Color.White);
    }

    /// <summary>The world darkened behind a window, as Minecraft darkens it behind its screens.</summary>
    public static void Backdrop() => DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), new Color(16, 16, 16, 150));

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
        // A pack's tooltip is its background and its frame over the lines and twelve pixels around
        // them, as Minecraft draws its tooltips, the frame's glow reaching past the box.
        if (PackGui.Get("sprites/tooltip/background") is { } background)
        {
            var around = new Rectangle(x - 8 * s, y - 8 * s, width + 16 * s, height + 16 * s);
            PackGui.Draw(background, around, s);
            if (PackGui.Get("sprites/tooltip/frame") is { } frame) PackGui.Draw(frame, around, s);
        }
        else Stretched(GuiArt.Tooltip, new Rectangle(x, y, width, height));
        for (int i = 0; i < lines.Count; i++)
            Text(lines[i], x + 4 * s, y + 4 * s + i * LineHeight, i == 0 ? Color.White : new Color(170, 170, 170));
    }

    // The box of the slider a press began on, which follows the pointer until the button is let go,
    // known by its box since its label shows its value and changes as it moves.
    private static Rectangle? _sliding;

    /// <summary>A button of the interface's, lit under the pointer with its label yellow, as Minecraft's is.</summary>
    /// <returns>Whether it was clicked this frame.</returns>
    /// <summary>
    /// A field of typed text as Minecraft's edit boxes are, a black box in a gray border with its text
    /// and a blinking caret, taking the characters typed this frame and Backspace, which repeats while
    /// held, to so many characters, and placing the input method's window beside it.
    /// </summary>
    /// <returns>The text with what was typed this frame.</returns>
    public static string TextField(string text, Rectangle box, int most)
    {
        SetTextInputArea(box, text.Length);
        for (var typed = GetCharPressed(); typed != 0; typed = GetCharPressed())
            if (typed >= ' ' && typed != 127 && text.Length < most) text += char.ConvertFromUtf32(typed);
        if (text.Length > 0 && (IsKeyPressed(Key.Backspace) || IsKeyPressedRepeat(Key.Backspace))) text = text[..^1];
        var s = Scale;
        DrawRectangleRec(box, new Color(160, 160, 160, 255));
        DrawRectangleRec(new Rectangle(box.X + s, box.Y + s, box.Width - 2 * s, box.Height - 2 * s), Color.Black);
        var y = (int)(box.Y + (box.Height - 8 * s) / 2);
        var ink = new Color(224, 224, 224, 255);
        Text(text, (int)box.X + 4 * s, y, ink);
        // The caret blinks a third of a second on and off, as Minecraft's does.
        if ((int)(GetTime() * 3) % 2 == 0) Text("_", (int)box.X + 4 * s + TextWidth(text), y, ink);
        return text;
    }

    public static bool Button(string label, Rectangle box)
    {
        var over = Over(box);
        Stretched(over ? GuiArt.ButtonLit : GuiArt.Button, box);
        Centered(label, box, over ? new Color(255, 255, 160) : Color.White);
        return over && IsMouseButtonPressed(MouseButton.Left);
    }

    /// <summary>
    /// A slider of the interface's, its knob along a darker button at its value between two ends and
    /// its label over it, which a press and a drag along it move, as Minecraft's are.
    /// </summary>
    /// <returns>The value, moved where the slider is held, and through <paramref name="letGo"/> whether it was let go this frame.</returns>
    public static float Slider(string label, Rectangle box, float value, float low, float high, out bool letGo)
    {
        var s = Scale;
        letGo = false;
        if (Over(box) && IsMouseButtonPressed(MouseButton.Left)) _sliding = box;
        if (_sliding == box)
        {
            var along = Math.Clamp((GetMousePosition().X - box.X - 4 * s) / (box.Width - 8 * s), 0, 1);
            value = low + along * (high - low);
            if (!IsMouseButtonDown(MouseButton.Left))
            {
                _sliding = null;
                letGo = true;
            }
        }
        var lit = Over(box) || _sliding == box;
        var at = (value - low) / (high - low);
        var knob = new Rectangle(box.X + (box.Width - 8 * s) * Math.Clamp(at, 0, 1), box.Y, 8 * s, box.Height);
        // A pack's slider is its track, lit under the pointer, and its handle, as Minecraft's are.
        if (PackGui.Get(lit ? "sprites/widget/slider_highlighted" : "sprites/widget/slider") is { } track
            && PackGui.Get(lit ? "sprites/widget/slider_handle_highlighted" : "sprites/widget/slider_handle") is { } handle)
        {
            PackGui.Draw(track, box, s);
            PackGui.Draw(handle, knob, s);
        }
        else
        {
            Stretched(GuiArt.Button, box, new Color(150, 150, 150));
            Stretched(lit ? GuiArt.ButtonLit : GuiArt.Button, knob);
        }
        Centered(label, box, Over(box) ? new Color(255, 255, 160) : Color.White);
        return value;
    }

    /// <summary>A line of text in the middle of a box, with its shadow.</summary>
    public static void Centered(string text, Rectangle box, Color color) =>
        Text(text, (int)(box.X + (box.Width - TextWidth(text)) / 2), (int)(box.Y + (box.Height - 8 * Scale) / 2), color);

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
