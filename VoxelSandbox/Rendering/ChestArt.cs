using Engine;

namespace VoxelSandbox;

/// <summary>
/// A chest's faces as tiles of the terrain's atlas, cut from a pack's picture of the source's chest
/// model, <c>entity/chest/normal</c>, which lays the model's boxes out on one picture of 64 pixels a
/// side rather than giving a block's six faces.
/// </summary>
/// <remarks>
/// <para>
/// The source's chest is a base fourteen pixels across and ten high under a lid five high, the lid
/// lying a pixel down over the base, with a latch two across and four high on its front. Its picture
/// holds the lid's top at 28, 0, its base's bottom at 14, 19, the lid's four sides in a row from 0,
/// 14 five high and the base's in a row from 0, 33 ten high, each fourteen across, the front second,
/// and the latch's front at 1, 1, the sides drawn upside down since the model is turned over. A tile
/// lays its face as the block's box of fourteen by fourteen shows it, a pixel in from each side and
/// from the cell's top two pixels down, so the box's faces show it where they cross their cell.
/// </para>
/// <para>
/// The names <see cref="Top"/>, <see cref="Side"/>, <see cref="Front"/> and <see cref="Bottom"/>
/// stand in <see cref="PackTextures"/> for a texture's name, and <see cref="Cut(ResourcePacks, string)"/> makes the tile.
/// </para>
/// </remarks>
public static class ChestArt
{
    private const string Picture = "entity/chest/normal";

    public const string Top = Picture + "#top", Side = Picture + "#side", Front = Picture + "#front", Bottom = Picture + "#bottom";

    /// <summary>Whether a texture's name is one of a chest's faces.</summary>
    public static bool Owns(string name) => name.StartsWith(Picture + "#", StringComparison.Ordinal);

    /// <summary>A chest's face cut from the packs' picture of its model at the picture's own scale, or null where no pack has the picture.</summary>
    public static Image? Cut(ResourcePacks packs, string name) => packs.Texture(Picture) is { } model ? Cut(model, name) : null;

    /// <summary>A chest's face cut from a picture of its model, a tile a quarter of the picture's width a side.</summary>
    public static Image Cut(Image model, string name)
    {
        var scale = Math.Max(1, model.Width / 64);
        var tile = new Image(new byte[16 * scale * 16 * scale * 4], 16 * scale, 16 * scale);
        switch (name)
        {
            case Top:
                Copy(model, tile, scale, 28, 0, 14, 14, 1, 1, flip: false);
                break;
            case Bottom:
                Copy(model, tile, scale, 14, 19, 14, 14, 1, 1, flip: false);
                break;
            default:
                // The lid's side from row 2 and the base's under it, the base's top row behind the lid.
                Copy(model, tile, scale, 14, 14, 14, 5, 1, 2, flip: true);
                Copy(model, tile, scale, 14, 33, 14, 9, 1, 7, flip: true);
                if (name == Front) Copy(model, tile, scale, 1, 1, 2, 4, 7, 5, flip: true);
                break;
        }
        return tile;
    }

    // Copies a part of the model's picture, in the source's pixels, onto a place of the tile, turned
    // upside down where the model draws it so.
    private static void Copy(Image model, Image tile, int scale, int fromX, int fromY, int width, int height, int toX, int toY, bool flip)
    {
        for (int y = 0; y < height * scale; y++)
            for (int x = 0; x < width * scale; x++)
            {
                var row = flip ? height * scale - 1 - y : y;
                int from = ((fromY * scale + row) * model.Width + fromX * scale + x) * 4, to = ((toY * scale + y) * tile.Width + toX * scale + x) * 4;
                if (from + 4 <= model.Data.Length && to + 4 <= tile.Data.Length) Array.Copy(model.Data, from, tile.Data, to, 4);
            }
    }
}
