using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// A picture of the interface's, its place in <see cref="GuiArt"/>'s atlas and, for one drawn
/// stretched, the width of its edge that keeps its size, in the interface's own pixels.
/// </summary>
public readonly record struct GuiSprite(int X, int Y, int Width, int Height, int Border = 0)
{
    public Rectangle Source => new(X, Y, Width, Height);
}

/// <summary>
/// The interface's pictures, painted once into an atlas of RGBA bytes in the game's own flat look,
/// a dark outline, a pixel of light above and of shade below and plain colors, as Bare Bones draws
/// Minecraft's, the panels and the buttons drawn stretched between edges that keep their size.
/// </summary>
/// <remarks>
/// Each picture is in the interface's own pixels, which <see cref="Gui"/> draws at a whole number
/// of the screen's, so its edges stay square. A resource pack's interface textures take their place
/// where one is loaded.
/// </remarks>
public static class GuiArt
{
    private const int Width = 256, Height = 64;

    // The panel's gray and its edges, a slot's and the hotbar's, as Minecraft's interface has them.
    private static readonly Color Outline = new(0, 0, 0), Light = new(255, 255, 255), Shade = new(85, 85, 85), Face = new(198, 198, 198);
    private static readonly Color SlotFill = new(139, 139, 139), SlotDark = new(55, 55, 55);
    private static readonly Color BarEdge = new(150, 150, 150), BarInner = new(96, 96, 96), BarFill = new(20, 20, 20, 150);
    private static readonly Color Chosen = new(96, 214, 72), ChosenDark = new(36, 120, 30);

    /// <summary>A window's panel, stretched between edges of four.</summary>
    public static readonly GuiSprite Panel = new(0, 0, 16, 16, 4);

    /// <summary>A slot of 18 with its sunken edge, an item's 16 within it.</summary>
    public static readonly GuiSprite Slot = new(16, 0, 18, 18);

    /// <summary>The hotbar's nine slots, 182 across, each 20 apart.</summary>
    public static readonly GuiSprite Hotbar = new(0, 24, 182, 22);

    /// <summary>The frame around the hotbar's chosen slot.</summary>
    public static readonly GuiSprite Selection = new(184, 24, 24, 24);

    /// <summary>A button and the one under the pointer, stretched between edges of three.</summary>
    public static readonly GuiSprite Button = new(40, 0, 20, 20, 3), ButtonLit = new(60, 0, 20, 20, 3);

    /// <summary>A tooltip's dark box, stretched between edges of three.</summary>
    public static readonly GuiSprite Tooltip = new(80, 0, 12, 12, 3);

    /// <summary>A heart whole and half, its empty place, and food whole, half and empty, each nine across.</summary>
    public static readonly GuiSprite HeartFull = new(96, 0, 9, 9), HeartHalf = new(106, 0, 9, 9), HeartEmpty = new(116, 0, 9, 9);

    public static readonly GuiSprite FoodFull = new(126, 0, 9, 9), FoodHalf = new(136, 0, 9, 9), FoodEmpty = new(146, 0, 9, 9);

    /// <summary>A bubble of breath.</summary>
    public static readonly GuiSprite Air = new(156, 0, 9, 9);

    /// <summary>The arrow of a grid's making and a furnace's smelting, and the furnace's flame, empty and full.</summary>
    public static readonly GuiSprite Arrow = new(166, 0, 22, 15), ArrowFull = new(190, 0, 22, 15), Flame = new(214, 0, 14, 14), FlameFull = new(230, 0, 14, 14);

    // A heart, a drumstick and a bubble, a row a string: o the outline, r and m their color, l its
    // light, d the empty place's dark and . nothing.
    private static readonly string[] HeartPattern = [".oo...oo.", "orro.orro", "orlrorrro", "orrrrrrro", ".orrrrro.", "..orrro..", "...oro...", "....o....", "........."];
    private static readonly string[] FoodPattern = ["....oooo.", "...ommmmo", "..ommlmmo", "..ommmmmo", ".oommmmo.", "o.oooo...", ".wo......", "ow.......", ".o......."];
    private static readonly string[] AirPattern = ["..ooooo..", ".obbbbbo.", "obwbbbbbo", "obwbbbbbo", "obbbbbbbo", "obbbbbbbo", ".obbbbbo.", "..ooooo..", "........."];

    private static byte[]? _atlas;
    private static Texture2D _texture;

    /// <summary>The atlas, a row of pixels at a time from the top, four bytes a pixel.</summary>
    public static byte[] Atlas => _atlas ??= Paint();

    /// <summary>The atlas as a texture, loaded the first time it is drawn and sampled without filtering.</summary>
    public static Texture2D Texture()
    {
        if (_texture.IsValid) return _texture;
        _texture = LoadTextureFromImage(new Image(Atlas, Width, Height));
        SetTextureFilter(_texture, TextureFilter.Point);
        return _texture;
    }

    private static byte[] Paint()
    {
        var atlas = new byte[Width * Height * 4];
        void Put(int x, int y, Color c)
        {
            var at = (y * Width + x) * 4;
            (atlas[at], atlas[at + 1], atlas[at + 2], atlas[at + 3]) = (c.R, c.G, c.B, c.A);
        }

        // The panel: its corners cut, a dark outline, light above and to the left, shade below and
        // to the right, and the face between.
        Fill(Panel, (x, y) =>
        {
            int r = Panel.Width - 1, b = Panel.Height - 1;
            if ((x is 0 || x == r) && (y is 0 || y == b)) return null;
            if (x == 0 || y == 0 || x == r || y == b) return Outline;
            if (x == 1 || y == 1) return x == r - 1 || y == b - 1 ? Face : Light;
            if (x == r - 1 || y == b - 1) return Shade;
            return Face;
        });
        // A slot sunk into the panel, dark above and to the left and light below and to the right.
        Fill(Slot, (x, y) => x == 0 || y == 0 ? x == Slot.Width - 1 || y == Slot.Height - 1 ? SlotFill : SlotDark
            : x == Slot.Width - 1 || y == Slot.Height - 1 ? Light : SlotFill);
        // A button raised from the panel, its top lit and its bottom shaded, and the lit one outlined white.
        foreach (var (button, rim) in (ReadOnlySpan<(GuiSprite, Color)>)[(Button, Outline), (ButtonLit, Light)])
            Fill(button, (x, y) =>
            {
                int r = button.Width - 1, b = button.Height - 1;
                if (x == 0 || y == 0 || x == r || y == b) return rim;
                if (y == 1) return new Color(206, 206, 206);
                if (y >= b - 2) return new Color(72, 72, 72);
                return new Color(132, 132, 132);
            });
        // A tooltip, dark with a thin lighter edge inside its outline.
        Fill(Tooltip, (x, y) =>
        {
            int r = Tooltip.Width - 1, b = Tooltip.Height - 1;
            if (x == 0 || y == 0 || x == r || y == b) return new Color(16, 16, 20, 240);
            if (x == 1 || y == 1 || x == r - 1 || y == b - 1) return new Color(96, 96, 112, 240);
            return new Color(24, 24, 30, 235);
        });
        // The hotbar: an outline around nine cells, each a lighter edge and a darker one around a
        // see-through inside, 20 across with the outline's pixel shared.
        Fill(Hotbar, (x, y) =>
        {
            int r = Hotbar.Width - 1, b = Hotbar.Height - 1;
            if (x == 0 || y == 0 || x == r || y == b) return Outline;
            var cell = (x - 1) % 20;
            if (y == 1 || y == b - 1 || cell == 0 || cell == 19 || x == r - 1) return BarEdge;
            if (y == 2 || y == b - 2 || cell == 1 || cell == 18 || x == r - 2) return BarInner;
            return BarFill;
        });
        Fill(Selection, (x, y) =>
        {
            int r = Selection.Width - 1, b = Selection.Height - 1;
            if (x == 0 || y == 0 || x == r || y == b) return ChosenDark;
            if (x == 1 || y == 1 || x == r - 1 || y == b - 1) return Chosen;
            if (x == 2 || y == 2 || x == r - 2 || y == b - 2) return ChosenDark;
            return null;
        });

        Pattern(HeartFull, HeartPattern, c => c, 9);
        Pattern(HeartHalf, HeartPattern, c => c, 5);
        Pattern(HeartEmpty, HeartPattern, c => c, 0);
        Pattern(FoodFull, FoodPattern, c => c, 9);
        Pattern(FoodHalf, FoodPattern, c => c, 4, fromRight: true);
        Pattern(FoodEmpty, FoodPattern, c => c, 0);
        Pattern(Air, AirPattern, c => c, 9);

        // The arrow, a shaft five high and a head narrowing from the middle row's thirteen to a
        // point, and the flame, narrow at its tip and widest two thirds down, each empty and full.
        foreach (var (sprite, color) in (ReadOnlySpan<(GuiSprite, Color)>)[(Arrow, SlotFill), (ArrowFull, Light)])
            Fill(sprite, (x, y) => (x < 15 ? Math.Abs(y - 7) <= 2 : Math.Abs(y - 7) <= 21 - x) ? color : null);
        foreach (var (sprite, color) in (ReadOnlySpan<(GuiSprite, Color)>)[(Flame, SlotFill), (FlameFull, new Color(255, 160, 40))])
            Fill(sprite, (x, y) => MathF.Abs(x - 6.5f) <= (y < 9 ? 0.5f + y * 0.6f : 0.5f + (13 - y) * 1.3f) ? color : null);
        return atlas;

        void Fill(GuiSprite sprite, Func<int, int, Color?> color)
        {
            for (int y = 0; y < sprite.Height; y++)
                for (int x = 0; x < sprite.Width; x++)
                    if (color(x, y) is { } c) Put(sprite.X + x, sprite.Y + y, c);
        }

        // A pattern's pixels in their colors, its color only up to a column, the rest of it the
        // empty place's dark, so a half heart is red on its left and a half drumstick on its right.
        void Pattern(GuiSprite sprite, string[] rows, Func<Color, Color> tint, int filled, bool fromRight = false)
        {
            for (int y = 0; y < rows.Length; y++)
                for (int x = 0; x < rows[y].Length; x++)
                {
                    var full = fromRight ? x >= 9 - filled : x < filled;
                    Color? c = rows[y][x] switch
                    {
                        'o' => new Color(16, 10, 8),
                        'r' => full ? new Color(214, 28, 36) : new Color(52, 28, 28),
                        'l' => full ? new Color(255, 150, 150) : new Color(52, 28, 28),
                        'm' => full ? new Color(176, 104, 48) : new Color(46, 34, 24),
                        'w' => full ? new Color(232, 226, 210) : new Color(70, 66, 60),
                        'b' => new Color(70, 150, 230),
                        _ => null,
                    };
                    if (c is { } color) Put(sprite.X + x, sprite.Y + y, tint(color));
                }
        }
    }
}
