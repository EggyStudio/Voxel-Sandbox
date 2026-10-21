using Engine;

namespace VoxelSandbox;

/// <summary>
/// The game's own looks for the grass's top, in its flat style of few lines and plain colors, where
/// no pack's block state lists any: the plain grass, patches of clover a shade darker than the grass
/// under them, a few pebbles, and little blossoms of a few colors, each drawn sixteen pixels square
/// and weighed by how often a block shows it.
/// </summary>
/// <remarks>
/// A clover's patch darkens the grass's own tile where it lies, so it is tinted by the climate as
/// the grass is, and it is laid over whatever tile the grass has, a pack's or the game's white one,
/// which keeps the way it lies, since a pack draws its grass's streaks one way, the patch and the
/// decor turning over it instead.
/// A pebble's and a blossom's pixels keep their own colors, laid as a layer of decor over the grass,
/// clear elsewhere. The weights follow those of the grass's looks in the owner's modpack, about four
/// blocks in five plain, one in twelve clover and a few in a hundred stones or flowers.
/// </remarks>
public static class GroundArt
{
    /// <summary>The pixels across a look.</summary>
    public const int Size = 16;

    /// <summary>
    /// A look of the grass's top, the pixels the grass's own tile darkens under it by a share where
    /// it is clover, or else a layer of decor laid over it, clear where the grass shows, and its weight.
    /// </summary>
    public readonly record struct Look(bool[]? Darkened, byte[]? Decor, int Weight);

    /// <summary>How much darker a clover's leaves are than the grass under them.</summary>
    public const float CloverShade = 0.72f;

    /// <summary>The looks, the plain grass first.</summary>
    public static IReadOnlyList<Look> Looks { get; } = Build();

    private static List<Look> Build()
    {
        int Hash(int a, int b) => (int)(PlaceHash.Of(a, b, 0x5eed) % 1000);
        List<Look> looks = [new(null, null, 480)];

        // Clover in clusters of three leaves about a pixel's middle, the larger patch more of them.
        foreach (var (clusters, weight, salt) in (ReadOnlySpan<(int, int, int)>)[(5, 32, 1), (2, 48, 2)])
        {
            var dark = new bool[Size * Size];
            for (int c = 0; c < clusters; c++)
            {
                int x = 2 + Hash(c, salt) % 12, y = 2 + Hash(c, salt + 9) % 12;
                foreach (var (dx, dy) in (ReadOnlySpan<(int, int)>)[(0, 0), (-1, 0), (0, -1), (1, 1), (1, 0)])
                    dark[(y + dy) * Size + x + dx] = true;
            }
            looks.Add(new(dark, null, weight));
        }

        // Pebbles, two or three small stones of a gray with a darker pixel under each.
        var pebbles = new byte[Size * Size * 4];
        foreach (var (x, y, wide) in (ReadOnlySpan<(int, int, bool)>)[(4, 5, true), (10, 9, false), (7, 12, true)])
        {
            Put(pebbles, x, y, new Color(142, 142, 136));
            if (wide) Put(pebbles, x + 1, y, new Color(160, 160, 154));
            Put(pebbles, x, y + 1, new Color(96, 96, 92));
            if (wide) Put(pebbles, x + 1, y + 1, new Color(110, 110, 104));
        }
        looks.Add(new(null, pebbles, 4));

        // Blossoms of four petals about a center, one to a small look and three to a large one, each
        // look of one kind of flower's colors.
        (Color Petal, Color Middle)[] kinds =
        [
            (new Color(240, 240, 232), new Color(236, 196, 56)), (new Color(244, 204, 52), new Color(214, 120, 30)),
            (new Color(96, 132, 232), new Color(236, 236, 236)), (new Color(232, 132, 176), new Color(250, 222, 120)),
        ];
        for (int k = 0; k < kinds.Length; k++)
            foreach (var (count, weight) in (ReadOnlySpan<(int, int)>)[(1, 4), (3, 2)])
            {
                var flowers = new byte[Size * Size * 4];
                for (int f = 0; f < count; f++)
                {
                    int x = 3 + Hash(f + k * 7, count) % 10, y = 3 + Hash(f + k * 7, count + 5) % 10;
                    Put(flowers, x, y, kinds[k].Middle);
                    foreach (var (dx, dy) in (ReadOnlySpan<(int, int)>)[(1, 0), (-1, 0), (0, 1), (0, -1)]) Put(flowers, x + dx, y + dy, kinds[k].Petal);
                }
                looks.Add(new(null, flowers, weight));
            }
        return looks;
    }

    private static void Put(byte[] pixels, int x, int y, Color color)
    {
        if ((uint)x >= Size || (uint)y >= Size) return;
        var at = (y * Size + x) * 4;
        (pixels[at], pixels[at + 1], pixels[at + 2], pixels[at + 3]) = (color.R, color.G, color.B, 255);
    }

    /// <summary>A patch turned by so many quarter turns clockwise, so it lies another way over a grass tile that keeps its own.</summary>
    public static bool[] Turned(bool[] patch, int turns)
    {
        for (int turn = 0; turn < turns; turn++)
        {
            var turned = new bool[Size * Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    turned[x * Size + Size - 1 - y] = patch[y * Size + x];
            patch = turned;
        }
        return patch;
    }

    /// <summary>A tile of the grass's top under a clover's patch, its pixels darkened where the patch lies, at the look's size.</summary>
    public static Image Darken(Image tile, bool[] darkened)
    {
        var pixels = new byte[Size * Size * 4];
        for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                var from = (y * tile.Height / Size * tile.Width + x * tile.Width / Size) * 4;
                var at = (y * Size + x) * 4;
                var shade = darkened[y * Size + x] ? CloverShade : 1;
                for (int c = 0; c < 3; c++) pixels[at + c] = (byte)(tile.Data[from + c] * shade);
                pixels[at + 3] = tile.Data[from + 3];
            }
        return new Image(pixels, Size, Size);
    }
}
