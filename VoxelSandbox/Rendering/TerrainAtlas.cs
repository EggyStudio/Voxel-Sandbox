using Engine;

namespace VoxelSandbox;

/// <summary>
/// The textures resource packs give the surfaces, laid out in one image the terrain shader samples,
/// each surface's tile, the tile of an overlay over it, and the average color of each as the scene's
/// distance field reads a face.
/// </summary>
/// <remarks>
/// <para>
/// Every tile is drawn at the largest of the packs' sizes, up to <see cref="MostPixels"/> and a power
/// of two, a smaller one repeated pixel by pixel to it, in a cell twice its size holding it two by
/// two from a half tile in, so a sample of a smaller mip level near a tile's edge reads the same tile
/// across it rather than its neighbor. The shader reads the middle of the cell, a half tile in from
/// each side, where the tile lies from its corner, and the image's mip levels go down to the level
/// where a cell is one pixel, past which it holds the level there.
/// </para>
/// <para>
/// Tile 0 is white, which a surface no pack gives a texture keeps, drawn in the game's own color, so
/// with no pack the terrain is drawn as it was before packs were read.
/// </para>
/// </remarks>
public sealed class TerrainAtlas
{
    /// <summary>The most pixels across a tile is kept at, a larger pack's tiles sampled down to it.</summary>
    public const int MostPixels = 128;

    /// <summary>The cells across the image.</summary>
    public const int Columns = 16;

    /// <summary>The most tiles the shader's nine bits of a tile's number reach, tile 0 among them.</summary>
    public const int MostTiles = 512;

    /// <summary>The atlas of no pack, its one white tile for every surface.</summary>
    public static readonly TerrainAtlas None = Build(ResourcePacks.None);

    private readonly int[] _tile, _overlay;
    private readonly Color?[] _tint;
    private readonly List<byte[]> _pictures;
    // Each tile's average color decoded from sRGB, weighed by alpha, and how much of it is covered.
    private readonly List<(float R, float G, float B, float Covered)> _averages;

    private TerrainAtlas(int pixels, int[] tile, int[] overlay, Color?[] tint, List<byte[]> pictures)
    {
        (Pixels, _tile, _overlay, _tint, _pictures) = (pixels, tile, overlay, tint, pictures);
        _averages = [.. pictures.Select(Average)];
        Rows = (pictures.Count + Columns - 1) / Columns;
    }

    /// <summary>Pixels across a tile.</summary>
    public int Pixels { get; }

    /// <summary>The cells down the image.</summary>
    public int Rows { get; }

    /// <summary>The tiles held, the white one among them.</summary>
    public int Tiles => _pictures.Count;

    /// <summary>Whether any surface has a texture of a pack.</summary>
    public bool Any => _pictures.Count > 1;

    /// <summary>The image's width in pixels.</summary>
    public int Width => Columns * 2 * Pixels;

    /// <summary>The image's height in pixels.</summary>
    public int Height => Rows * 2 * Pixels;

    /// <summary>Builds the atlas of the textures the packs give the surfaces of <see cref="PackTextures"/>, a pack's first frame of an animated one.</summary>
    public static TerrainAtlas Build(ResourcePacks packs)
    {
        var count = Surfaces.All.Count;
        var (tile, overlay, tint) = (new int[count], new int[count], new Color?[count]);
        var found = new List<Image> { new(Enumerable.Repeat((byte)255, 4).ToArray(), 1, 1) };
        var numbers = new Dictionary<string, int>(StringComparer.Ordinal);

        int Number(string name)
        {
            if (numbers.TryGetValue(name, out var number)) return number;
            if (found.Count >= MostTiles || packs.Texture(name) is not { } image) return numbers[name] = 0;
            found.Add(image);
            return numbers[name] = found.Count - 1;
        }

        for (int surface = 0; surface < count; surface++)
        {
            if (PackTextures.Of(surface) is not { } look || (tile[surface] = Number(look.Texture)) == 0) continue;
            tint[surface] = look.Tint;
            if (look.Overlay is { } over) overlay[surface] = Number(over);
        }

        var pixels = (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)Math.Clamp(found.Skip(1).Select(i => i.Width).DefaultIfEmpty(1).Max(), 1, MostPixels));
        return new TerrainAtlas(pixels, tile, overlay, tint, [.. found.Select(i => Resized(i, pixels))]);
    }

    // An image sampled to so many pixels square by its nearest pixel, so a smaller tile's pixels
    // stay square at the size of the largest.
    private static byte[] Resized(Image image, int pixels)
    {
        var resized = new byte[pixels * pixels * 4];
        for (int y = 0; y < pixels; y++)
            for (int x = 0; x < pixels; x++)
            {
                int sx = x * image.Width / pixels, sy = y * image.Height / pixels;
                Array.Copy(image.Data, (sy * image.Width + sx) * 4, resized, (y * pixels + x) * 4, 4);
            }
        return resized;
    }

    private static (float, float, float, float) Average(byte[] rgba)
    {
        float r = 0, g = 0, b = 0, weight = 0;
        for (int i = 0; i < rgba.Length; i += 4)
        {
            var alpha = rgba[i + 3] / 255f;
            (r, g, b, weight) = (r + Linear(rgba[i]) * alpha, g + Linear(rgba[i + 1]) * alpha, b + Linear(rgba[i + 2]) * alpha, weight + alpha);
        }
        return weight > 0 ? (r / weight, g / weight, b / weight, weight / (rgba.Length / 4)) : (0, 0, 0, 0);
    }

    /// <summary>The surface's tile, or 0 where no pack gives it a texture.</summary>
    public int Tile(int surface) => _tile[surface];

    /// <summary>The tile of the overlay over the surface's, or 0 for none.</summary>
    public int Overlay(int surface) => _overlay[surface];

    /// <summary>
    /// How a face of a surface is drawn under the climate's colors at its place, the tiles packed as
    /// the terrain shader reads them, the tile in the low 9 bits, the overlay's in the next 9 and
    /// whether the tile under the overlay is left untinted in the bit above, the color the vertex
    /// carries, and the face's average color as the scene's distance field reads it, all sRGB.
    /// </summary>
    /// <remarks>
    /// A surface with no texture carries its own color under the climate's, as it did before packs
    /// were read. A textured one carries the color its texture is tinted by, its own tint, the
    /// climate's for a surface the climate tints, or white, opaque, since a see-through texture's
    /// alpha is its own, and under an overlay the grass color the overlay takes.
    /// </remarks>
    public (uint Tiles, Color Tint, Color Albedo) Paint(int surface, Color grass, Color foliage)
    {
        var look = Surfaces.All[surface];
        var tile = _tile[surface];
        if (tile == 0)
        {
            var own = look.Under(grass, foliage);
            return (0, own, own);
        }
        var tint = _tint[surface] ?? look.Tint switch { SurfaceTint.Grass => grass, SurfaceTint.Foliage => foliage, _ => Color.White };
        var (r, g, b, _) = _averages[tile];
        var overlay = _overlay[surface];
        if (overlay == 0)
            return ((uint)tile, tint with { A = 255 }, Encode(r * Linear(tint.R), g * Linear(tint.G), b * Linear(tint.B)));
        var (or, og, ob, covered) = _averages[overlay];
        return ((uint)tile | (uint)overlay << 9 | 1u << 18, grass with { A = 255 },
            Encode(Mix(r, or * Linear(grass.R), covered), Mix(g, og * Linear(grass.G), covered), Mix(b, ob * Linear(grass.B), covered)));
    }

    /// <summary>
    /// The color of a surface's texture at a place across its tile from 0 to 1, tinted and with its
    /// overlay over it as a face of it is drawn, or null where no pack gives it one.
    /// </summary>
    public Color? Texel(int surface, float u, float v, Color grass, Color foliage)
    {
        var tile = _tile[surface];
        if (tile == 0) return null;
        var (_, tint, _) = Paint(surface, grass, foliage);
        int x = Math.Clamp((int)(u * Pixels), 0, Pixels - 1), y = Math.Clamp((int)(v * Pixels), 0, Pixels - 1);
        var at = (y * Pixels + x) * 4;
        var base_ = _pictures[tile];
        var overlay = _overlay[surface];
        if (overlay == 0) return Multiply(base_, at, tint);
        var over = _pictures[overlay];
        var top = Multiply(over, at, tint);
        var alpha = over[at + 3] / 255f;
        return new Color((byte)(base_[at] + (top.R - base_[at]) * alpha), (byte)(base_[at + 1] + (top.G - base_[at + 1]) * alpha),
            (byte)(base_[at + 2] + (top.B - base_[at + 2]) * alpha), base_[at + 3]);
    }

    /// <summary>A surface's tile alone, for an emissive block's cube drawn with a texture of its own, or null where it has none.</summary>
    public Image? Picture(int surface) => _tile[surface] == 0 ? null : new Image(_pictures[_tile[surface]], Pixels, Pixels);

    /// <summary>The atlas's pixels, a row at a time from the top, four bytes a pixel, each tile two by two in its cell from a half tile in.</summary>
    public byte[] Image()
    {
        var image = new byte[Width * Height * 4];
        var half = Pixels / 2;
        for (int tile = 0; tile < _pictures.Count; tile++)
        {
            int cx = tile % Columns * 2 * Pixels, cy = tile / Columns * 2 * Pixels;
            for (int y = 0; y < 2 * Pixels; y++)
                for (int x = 0; x < 2 * Pixels; x++)
                    Array.Copy(_pictures[tile], ((y + Pixels - half) % Pixels * Pixels + (x + Pixels - half) % Pixels) * 4, image, ((cy + y) * Width + cx + x) * 4, 4);
        }
        return image;
    }

    private static Color Multiply(byte[] picture, int at, Color tint) =>
        new((byte)(picture[at] * tint.R / 255), (byte)(picture[at + 1] * tint.G / 255), (byte)(picture[at + 2] * tint.B / 255), picture[at + 3]);

    private static float Mix(float under, float over, float covered) => under + (over - under) * covered;

    private static float Linear(byte value)
    {
        var c = value / 255f;
        return c <= 0.04045f ? c / 12.92f : MathF.Pow((c + 0.055f) / 1.055f, 2.4f);
    }

    private static Color Encode(float r, float g, float b)
    {
        static byte Channel(float c) => (byte)Math.Clamp(MathF.Round((c <= 0.0031308f ? c * 12.92f : 1.055f * MathF.Pow(c, 1 / 2.4f) - 0.055f) * 255), 0, 255);
        return new Color(Channel(r), Channel(g), Channel(b));
    }
}
