namespace VoxelSandbox;

/// <summary>
/// The pixel art the sky's quads are drawn with, the sun, the moon in each of its phases, the glow
/// around each and a star, as RGBA bytes a row at a time from the top, a clear pixel's alpha 0.
/// </summary>
/// <remarks>
/// The sun and the moon are squares with their corners cut, 12 pixels across, as Minecraft's are
/// squares. The moon's phases are one sheet four across and two down, phase 0 the full moon at the
/// top left and 4 the new moon at the bottom left, in Minecraft's order, waning through the first
/// row and waxing through the second. Each phase has a clear pixel around it in the sheet, since a
/// sample at the edge of a phase's square, taken without filtering, may land on the pixel past it.
/// </remarks>
public static class SkyArt
{
    /// <summary>How many pixels the sun and the moon are across.</summary>
    public const int Size = 12;

    /// <summary>How many phases the moon goes through, a day each, as Minecraft's does.</summary>
    public const int Phases = 8;

    /// <summary>How many pixels apart the moon's phases are in their sheet, each with its clear edge.</summary>
    public const int Cell = Size + 2;

    /// <summary>
    /// How many pixels the glow around the sun or the moon is across, on its body's grid, the
    /// body's own pixels at its middle, enough for the glow to clear before its edge at the sun's
    /// pixels of <see cref="Sky.SunPixelDegrees"/>.
    /// </summary>
    public const int HaloSize = 104;

    /// <summary>How many steps of brightness the glow takes from its edge to the sun.</summary>
    public const int HaloSteps = 6;

    // The sun's edge, middle and core, the core the palest.
    private static readonly string[] Sun =
    [
        "..oooooooo..",
        ".ommmmmmmmo.",
        "ommmccccmmmo",
        "ommccccccmmo",
        "omccccccccmo",
        "omccccccccmo",
        "omccccccccmo",
        "omccccccccmo",
        "ommccccccmmo",
        "ommmccccmmmo",
        ".ommmmmmmmo.",
        "..oooooooo..",
    ];

    // The moon's face and its darker craters.
    private static readonly string[] Moon =
    [
        "..wwwwwwww..",
        ".wwwwwwwgww.",
        "wwggwwwwwwww",
        "wwggwwwwwgww",
        "wwwwwwwwwwww",
        "wwwwwwwggwww",
        "wgwwwwwggwww",
        "wwwwwwwwwwww",
        "wwwwgwwwwwgw",
        "wwwwwwwwwwww",
        ".wwwgwwwwww.",
        "..wwwwwwww..",
    ];

    // How far round the light on the moon has turned in each phase, in half turns, the crescents a
    // little short of the eighth past the quarter, so the two pixels they keep run unbroken from
    // tip to tip rather than one breaking off.
    private static readonly float[] Turns = [0, 0.25f, 0.5f, 0.7f, 1, 1.3f, 1.5f, 1.75f];

    // A star's middle and its arms, which a dim star leaves out.
    private static readonly string[] Star = [".a.", "asa", ".a."];

    private static (byte R, byte G, byte B) Shade(char pixel) => pixel switch
    {
        'o' => (255, 196, 80),
        'm' => (255, 230, 130),
        'c' => (255, 252, 222),
        'w' => (226, 230, 240),
        'g' => (176, 182, 198),
        'a' => (190, 198, 230),
        's' => (255, 255, 255),
        _ => throw new ArgumentOutOfRangeException(nameof(pixel)),
    };

    /// <summary>The sun, <see cref="Size"/> pixels square.</summary>
    public static byte[] SunPixels() => Paint(Sun, (_, _) => true);

    /// <summary>
    /// The glow around the sun, <see cref="HaloSize"/> pixels square and each pixel
    /// <paramref name="degrees"/> across, white with its brightness as its alpha. The brightness
    /// falls off from the sun as the twelfth power of the cosine of a pixel's angle from it and is
    /// held to <see cref="HaloSteps"/> steps, so the glow shows as rings whose edges step along the
    /// pixels. The moon's glow is the same pixels on the moon's smaller grid.
    /// </summary>
    public static byte[] HaloPixels(float degrees)
    {
        var pixels = new byte[HaloSize * HaloSize * 4];
        for (int y = 0; y < HaloSize; y++)
            for (int x = 0; x < HaloSize; x++)
            {
                float across = x + 0.5f - HaloSize / 2f, down = y + 0.5f - HaloSize / 2f;
                var angle = float.DegreesToRadians(MathF.Sqrt(across * across + down * down) * degrees);
                var glow = MathF.Pow(MathF.Max(0, MathF.Cos(angle)), 12);
                var at = (y * HaloSize + x) * 4;
                (pixels[at], pixels[at + 1], pixels[at + 2]) = (255, 255, 255);
                pixels[at + 3] = (byte)(255 * MathF.Round(glow * HaloSteps) / HaloSteps);
            }
        return pixels;
    }

    /// <summary>A star, three pixels square, its middle the pixel at 1, 1.</summary>
    public static byte[] StarPixels() => Paint(Star, (_, _) => true);

    /// <summary>
    /// The sheet of the moon's phases, <see cref="Cell"/> times four pixels across and twice
    /// <see cref="Cell"/> down, each phase's dark side clear.
    /// </summary>
    public static byte[] MoonPixels()
    {
        const int across = Cell * 4;
        var sheet = new byte[across * Cell * 2 * 4];
        for (int phase = 0; phase < Phases; phase++)
        {
            var cell = Paint(Moon, (x, y) => Lit(phase, x, y));
            int x0 = phase % 4 * Cell + 1, y0 = phase / 4 * Cell + 1;
            for (int y = 0; y < Size; y++)
                Array.Copy(cell, y * Size * 4, sheet, ((y0 + y) * across + x0) * 4, Size * 4);
        }
        return sheet;
    }

    /// <summary>
    /// Whether a pixel of the moon is lit in a phase, the moon taken as a ball lit from a way that
    /// turns from behind the eye at the full moon to behind the moon at the new one, from the left
    /// while it wanes and from the right while it waxes.
    /// </summary>
    public static bool Lit(int phase, int x, int y)
    {
        float u = (x + 0.5f) / Size * 2 - 1, v = (y + 0.5f) / Size * 2 - 1;
        // The square's pixel is taken to the place on the round face it stands for, drawn in along
        // its way from the middle until its distance is the larger of its two offsets, so the
        // square's edge is the ball's edge, a crescent thins toward its tips and the full moon's
        // corners are lit.
        var round = MathF.Sqrt(u * u + v * v);
        if (round > 0) (u, v) = (u * MathF.Max(MathF.Abs(u), MathF.Abs(v)) / round, v * MathF.Max(MathF.Abs(u), MathF.Abs(v)) / round);
        var depth = MathF.Sqrt(MathF.Max(0, 1 - u * u - v * v));
        var angle = Turns[phase] * MathF.PI;
        return -u * MathF.Sin(angle) + depth * MathF.Cos(angle) > 1e-4f;
    }

    private static byte[] Paint(string[] pattern, Func<int, int, bool> shown)
    {
        int width = pattern[0].Length, height = pattern.Length;
        var pixels = new byte[width * height * 4];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                if (pattern[y][x] == '.' || !shown(x, y)) continue;
                var (r, g, b) = Shade(pattern[y][x]);
                var at = (y * width + x) * 4;
                (pixels[at], pixels[at + 1], pixels[at + 2], pixels[at + 3]) = (r, g, b, 255);
            }
        return pixels;
    }
}
