using System.Numerics;
using System.Text.Json;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The interface's textures from the player's resource packs, read where Minecraft reads them, the
/// HUD's and the widgets' under <c>gui/sprites</c>, the windows' under <c>gui/container</c> and the
/// font from <c>font/ascii</c>, each in place of the game's own where a pack has it.
/// </summary>
/// <remarks>
/// <para>
/// A sprite is drawn over the box Minecraft draws it in, whatever its pixels, so a pack drawn at twice
/// the source's size shows at the same size, and one its metadata calls a nine-slice keeps its
/// edges, as wide as the metadata gives them in the interface's pixels, between which its middle is
/// stretched. A window's texture is 256 of the interface's pixels across whatever its pixels, the
/// window in its upper left corner, as Minecraft lays its containers out.
/// </para>
/// <para>
/// A texture is loaded the first time it is asked for and kept until the packs change.
/// </para>
/// </remarks>
public static class PackGui
{
    /// <summary>
    /// A texture of a pack and how it is drawn, its size in the interface's pixels that its metadata
    /// gives, or none for one drawn stretched over the box it is given, and the edges of a nine-slice.
    /// </summary>
    public readonly record struct Picture(Texture2D Texture, int Width, int Height, int Left = 0, int Top = 0, int Right = 0, int Bottom = 0)
    {
        /// <summary>Whether its edges keep their size, as a nine-slice's do.</summary>
        public bool Sliced => Left + Top + Right + Bottom > 0;
    }

    // Each of the game's pictures that a pack's sprite takes the place of, by its path under
    // gui/sprites and the size Minecraft draws it at, the hotbar's frame a pixel shorter than the game's.
    private static readonly Dictionary<GuiSprite, (string Path, int Width, int Height)> _sprites = new()
    {
        [GuiArt.Hotbar] = ("hud/hotbar", 182, 22),
        [GuiArt.Selection] = ("hud/hotbar_selection", 24, 23),
        [GuiArt.HeartFull] = ("hud/heart/full", 9, 9),
        [GuiArt.HeartHalf] = ("hud/heart/half", 9, 9),
        [GuiArt.HeartEmpty] = ("hud/heart/container", 9, 9),
        [GuiArt.FoodFull] = ("hud/food_full", 9, 9),
        [GuiArt.FoodHalf] = ("hud/food_half", 9, 9),
        [GuiArt.FoodEmpty] = ("hud/food_empty", 9, 9),
        [GuiArt.Air] = ("hud/air", 9, 9),
        [GuiArt.Button] = ("widget/button", 200, 20),
        [GuiArt.ButtonLit] = ("widget/button_highlighted", 200, 20),
    };

    private static ResourcePacks _packs = ResourcePacks.None;
    private static readonly Dictionary<string, Picture?> _loaded = [];
    private static PackFont? _font;
    private static bool _fontRead;

    /// <summary>Reads the interface's textures from these packs from now on, letting go of those of the packs before.</summary>
    public static void Use(ResourcePacks packs)
    {
        foreach (var picture in _loaded.Values)
            if (picture is { } loaded) UnloadTexture(loaded.Texture);
        if (_font is { } font) UnloadTexture(font.Texture);
        _loaded.Clear();
        (_packs, _font, _fontRead) = (packs, null, false);
    }

    /// <summary>A texture of the packs by its path under <c>textures/gui</c> without <c>.png</c>, as <c>sprites/hud/hotbar</c>, or null where none has it.</summary>
    public static Picture? Get(string path)
    {
        if (_loaded.TryGetValue(path, out var known)) return known;
        Picture? picture = null;
        if (_packs.Picture($"gui/{path}") is { } found)
        {
            var (image, meta) = found;
            var texture = LoadTextureFromImage(image);
            SetTextureFilter(texture, TextureFilter.Point);
            var (width, height, left, top, right, bottom) = Scaling(meta);
            picture = new Picture(texture, width, height, left, top, right, bottom);
        }
        return _loaded[path] = picture;
    }

    /// <summary>The pack's sprite in place of one of the game's pictures, with the size Minecraft draws it at, or null where none has it.</summary>
    public static (Picture Picture, int Width, int Height)? For(GuiSprite sprite) =>
        _sprites.TryGetValue(sprite, out var named) && Get($"sprites/{named.Path}") is { } picture ? (picture, named.Width, named.Height) : null;

    /// <summary>A window's texture by its name under <c>gui/container</c>, as <c>inventory</c>, or null where no pack has it.</summary>
    public static Picture? Container(string name) => Get($"container/{name}");

    /// <summary>The packs' font, or null where none has one.</summary>
    public static PackFont? Font
    {
        get
        {
            if (_fontRead) return _font;
            _fontRead = true;
            if (_packs.Texture("font/ascii") is { } image && PackFont.From(image) is { } font) _font = font;
            return _font;
        }
    }

    /// <summary>
    /// A sprite's size in the interface's pixels and its edges, from the <c>gui.scaling</c> of its
    /// metadata, none for a sprite drawn stretched, as Minecraft's are unless their metadata says.
    /// </summary>
    public static (int Width, int Height, int Left, int Top, int Right, int Bottom) Scaling(JsonElement? meta)
    {
        if (meta is not { ValueKind: JsonValueKind.Object } root || !root.TryGetProperty("gui", out var gui) || gui.ValueKind != JsonValueKind.Object
            || !gui.TryGetProperty("scaling", out var scaling) || scaling.ValueKind != JsonValueKind.Object
            || !scaling.TryGetProperty("type", out var type) || type.GetString() != "nine_slice")
            return (0, 0, 0, 0, 0, 0);
        int Whole(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.TryGetInt32(out var number) ? Math.Max(0, number) : 0;
        var (width, height) = (Whole(scaling, "width"), Whole(scaling, "height"));
        if (!scaling.TryGetProperty("border", out var border)) return (width, height, 0, 0, 0, 0);
        if (border.ValueKind == JsonValueKind.Number)
        {
            var all = border.TryGetInt32(out var edge) ? Math.Max(0, edge) : 0;
            return (width, height, all, all, all, all);
        }
        return border.ValueKind == JsonValueKind.Object
            ? (width, height, Whole(border, "left"), Whole(border, "top"), Whole(border, "right"), Whole(border, "bottom"))
            : (width, height, 0, 0, 0, 0);
    }

    /// <summary>
    /// Draws a picture over a box of the screen at the interface's scale, its middle stretched between
    /// edges that keep their size where it is a nine-slice, and the whole of it stretched otherwise.
    /// </summary>
    public static void Draw(Picture picture, Rectangle box, int scale, Color? tint = null)
    {
        var texture = picture.Texture;
        if (!picture.Sliced || picture.Width <= 0)
        {
            DrawTexturePro(texture, new Rectangle(0, 0, texture.Width, texture.Height), box, Vector2.Zero, 0, tint ?? Color.White);
            return;
        }
        // The nine-slice drawn in the texture's own pixels under a 2D camera, which keeps its edges at
        // their size in the pixels it draws in, scaled to the interface, so a pack at twice the
        // source's size keeps edges as wide in the interface's pixels.
        var texels = (float)texture.Width / picture.Width;
        var zoom = scale / texels;
        var info = new NPatchInfo(new Rectangle(0, 0, texture.Width, texture.Height), (int)(picture.Left * texels), (int)(picture.Top * texels),
            (int)(picture.Right * texels), (int)(picture.Bottom * texels));
        BeginMode2D(new Camera2D(new Vector2(box.X, box.Y), Vector2.Zero, 0, zoom));
        DrawTextureNPatch(texture, info, new Rectangle(0, 0, box.Width / zoom, box.Height / zoom), Vector2.Zero, 0, tint ?? Color.White);
        EndMode2D();
    }

    /// <summary>A part of a picture from a place in it, as a furnace's flame and arrow fill, at the interface's scale, both in the interface's pixels.</summary>
    public static void DrawPart(Picture picture, int width, int height, int fromX, int fromY, int partWidth, int partHeight, int x, int y, int scale)
    {
        if (partWidth <= 0 || partHeight <= 0) return;
        var texture = picture.Texture;
        float across = (float)texture.Width / width, down = (float)texture.Height / height;
        DrawTexturePro(texture, new Rectangle(fromX * across, fromY * down, partWidth * across, partHeight * down),
            new Rectangle(x, y, partWidth * scale, partHeight * scale), Vector2.Zero, 0, Color.White);
    }
}

/// <summary>
/// A pack's font, Minecraft's sheet of sixteen by sixteen glyphs of <c>font/ascii</c>, each eight of
/// the interface's pixels high, of which the printable ASCII characters are read, each as wide as
/// its rightmost lit column and a pixel's gap, and a space four, as Minecraft measures them.
/// </summary>
public sealed class PackFont
{
    private readonly int[] _advance;

    private PackFont(Texture2D texture, int cell, int[] advance) => (Texture, Cell, _advance) = (texture, cell, advance);

    /// <summary>The sheet.</summary>
    public Texture2D Texture { get; }

    /// <summary>A glyph's cell in the sheet's pixels.</summary>
    public int Cell { get; }

    /// <summary>The font of a sheet, or null for one whose sides are not sixteen cells.</summary>
    public static PackFont? From(Image sheet)
    {
        if (sheet.Width < 16 || sheet.Width != sheet.Height || sheet.Width % 16 != 0) return null;
        var texture = LoadTextureFromImage(sheet);
        SetTextureFilter(texture, TextureFilter.Point);
        return new PackFont(texture, sheet.Width / 16, Advances(sheet));
    }

    /// <summary>
    /// How far each of the sheet's 256 glyphs moves the pen in the interface's pixels, its lit columns
    /// to the rightmost one, scaled to the eight pixels of Minecraft's glyph, and a pixel's gap, four
    /// for a space and a glyph with nothing lit.
    /// </summary>
    public static int[] Advances(Image sheet)
    {
        var cell = sheet.Width / 16;
        var advances = new int[256];
        for (int glyph = 0; glyph < 256; glyph++)
        {
            int left = glyph % 16 * cell, top = glyph / 16 * cell, lit = -1;
            for (int x = cell - 1; x >= 0 && lit < 0; x--)
                for (int y = 0; y < cell && lit < 0; y++)
                    if (sheet.Data[((top + y) * sheet.Width + left + x) * 4 + 3] > 0) lit = x;
            advances[glyph] = glyph == ' ' || lit < 0 ? 4 : (int)(0.5f + (lit + 1) * 8f / cell) + 1;
        }
        return advances;
    }

    // A character's glyph, the printable ASCII ones where Minecraft's sheet keeps them and a question
    // mark for the rest.
    private static int Glyph(char c) => c is >= ' ' and <= '~' ? c : '?';

    /// <summary>How wide a line of text is in the interface's pixels.</summary>
    public int Width(string text)
    {
        var width = 0;
        foreach (var c in text) width += _advance[Glyph(c)];
        return width;
    }

    /// <summary>Draws a line of text with its top left at a place on the screen, at the interface's scale.</summary>
    public void Draw(string text, int x, int y, int scale, Color color)
    {
        foreach (var c in text)
        {
            var glyph = Glyph(c);
            if (glyph != ' ')
                DrawTexturePro(Texture, new Rectangle(glyph % 16 * Cell, glyph / 16 * Cell, Cell, Cell), new Rectangle(x, y, 8 * scale, 8 * scale), Vector2.Zero, 0, color);
            x += _advance[glyph] * scale;
        }
    }
}
