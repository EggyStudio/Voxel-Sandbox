using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>
/// The icon of every item, 16 pixels square, drawn once into an atlas of RGBA bytes that the
/// hotbar draws as a texture and the inventory window draws pixel by pixel.
/// </summary>
/// <remarks>
/// <para>
/// A block's icon is the block as a small cube seen from above and from two sides, cast pixel by
/// pixel against the block's own boxes, so a slab, stairs, a torch or a fence keep their shapes.
/// Each face takes its surface's color as it looks in the plains, with a fixed speckle standing for
/// a texture, since the blocks have colors and not textures, and is shaded by the way it faces,
/// the top brightest and the right darkest, as Minecraft's inventory shades its blocks. An emissive
/// face is left unshaded. An item that places no block is drawn from a few strokes and dots, its
/// edge outlined in a dark shade of its color, its upper left edge lit and its lower right shaded,
/// and each kind of tool keeps one shape, its head in the color of the tool's material.
/// </para>
/// <para>
/// The atlas is <see cref="Columns"/> icons across, each in a cell with a clear pixel around it,
/// since a sample at an icon's edge, taken without filtering, may land on the pixel past it.
/// </para>
/// </remarks>
public static class Icons
{
    /// <summary>How many pixels an icon is across.</summary>
    public const int Size = 16;

    /// <summary>How many pixels apart the icons are in the atlas, each with its clear edge.</summary>
    public const int Cell = Size + 2;

    /// <summary>How many icons the atlas holds across.</summary>
    public const int Columns = 10;

    /// <summary>How many rows of icons the atlas holds.</summary>
    public static int Rows => (Items.All.Count + Columns - 1) / Columns;

    /// <summary>The atlas's width in pixels.</summary>
    public static int Width => Columns * Cell;

    /// <summary>The atlas's height in pixels.</summary>
    public static int Height => Rows * Cell;

    /// <summary>The atlas, a row of pixels at a time from the top, four bytes a pixel.</summary>
    public static byte[] Atlas => _atlas ??= Paint();

    /// <summary>A stretch of one row of an icon in one color, from one pixel up to another, which the inventory window draws as a rectangle.</summary>
    public readonly record struct Run(int Y, int From, int To, Color Color);

    private static byte[]? _atlas;
    private static Run[][]? _runs;

    // The way from a block toward the eye, from the south and east and 30 degrees above, and the
    // icon's right and up across it.
    private static readonly Vector3 ToEye = Vector3.Normalize(new Vector3(MathF.Cos(MathF.PI / 6) / MathF.Sqrt(2), MathF.Sin(MathF.PI / 6), MathF.Cos(MathF.PI / 6) / MathF.Sqrt(2)));
    private static readonly Vector3 Right = Vector3.Normalize(Vector3.Cross(-ToEye, Vector3.UnitY));
    private static readonly Vector3 Up = Vector3.Cross(Right, -ToEye);

    // Pixels for each unit of the block across the icon, the cube's outline 16 pixels high.
    private static readonly float Scale = Size / (2 * Vector3.Dot(new Vector3(-0.5f, 0.5f, -0.5f), Up));

    // The most a block smaller than its cell is drawn larger by, so a torch or a lantern shows.
    private const float MostZoom = 2.5f;

    private static readonly Color Handle = new(104, 76, 44);
    private static readonly Color BoneWhite = new(226, 222, 206);

    /// <summary>Where an item's icon lies in the atlas.</summary>
    public static Rectangle Source(Item item) => new(item.Id % Columns * Cell + 1, item.Id / Columns * Cell + 1, Size, Size);

    /// <summary>An item's icon alone, <see cref="Size"/> pixels square.</summary>
    public static byte[] Pixels(Item item)
    {
        var icon = new byte[Size * Size * 4];
        var source = Source(item);
        for (int y = 0; y < Size; y++)
            Array.Copy(Atlas, (((int)source.Y + y) * Width + (int)source.X) * 4, icon, y * Size * 4, Size * 4);
        return icon;
    }

    /// <summary>An item's icon as stretches of its rows, each of one color, the clear pixels left out.</summary>
    public static Run[] RunsOf(Item item) => (_runs ??= [.. Items.All.Select(Runs)])[item.Id];

    private static Run[] Runs(Item item)
    {
        var icon = Pixels(item);
        var runs = new List<Run>();
        for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size;)
            {
                var at = (y * Size + x) * 4;
                int to = x + 1;
                while (to < Size && icon.AsSpan(at, 4).SequenceEqual(icon.AsSpan((y * Size + to) * 4, 4))) to++;
                if (icon[at + 3] > 0) runs.Add(new Run(y, x, to, new Color(icon[at], icon[at + 1], icon[at + 2], icon[at + 3])));
                x = to;
            }
        return [.. runs];
    }

    private static byte[] Paint()
    {
        var atlas = new byte[Width * Height * 4];
        foreach (var item in Items.All)
        {
            var icon = item.Block is { } block ? Cube(Blocks.Get(block)) : Drawn(item);
            var source = Source(item);
            for (int y = 0; y < Size; y++)
                Array.Copy(icon, y * Size * 4, atlas, (((int)source.Y + y) * Width + (int)source.X) * 4, Size * 4);
        }
        return atlas;
    }

    // A block seen from above and from the south and east sides, each pixel the face of its boxes
    // the way through the pixel meets first.
    private static byte[] Cube(BlockInfo block)
    {
        (Vector3 Min, Vector3 Max, int Top, int Side)[] boxes = block.Shape is { } shape
            ? [.. shape.Pieces.Select(p => (Vector3.Min(p.From, p.To), Vector3.Max(p.From, p.To), p.Surface, p.Surface))]
            : [(Vector3.Zero, Vector3.One, block.Top, block.Side)];
        var middle = new Vector3(0.5f);

        // The outline of the boxes across the icon, which a block smaller than its cell both ways
        // is drawn larger to fill, up to MostZoom, and which is moved to the icon's middle, so a
        // slab stays as wide as a cube and a torch stands as high as one.
        float left = float.MaxValue, right = float.MinValue, low = float.MaxValue, high = float.MinValue;
        foreach (var (min, max, _, _) in boxes)
            for (int corner = 0; corner < 8; corner++)
            {
                var at = new Vector3((corner & 1) == 0 ? min.X : max.X, (corner & 2) == 0 ? min.Y : max.Y, (corner & 4) == 0 ? min.Z : max.Z) - middle;
                (left, right) = (MathF.Min(left, Vector3.Dot(at, Right)), MathF.Max(right, Vector3.Dot(at, Right)));
                (low, high) = (MathF.Min(low, Vector3.Dot(at, Up)), MathF.Max(high, Vector3.Dot(at, Up)));
            }
        var zoom = Math.Clamp(MathF.Min(Size / Scale / (high - low), 2 * Vector3.Dot(new Vector3(0.5f, 0, -0.5f), Right) / (right - left)), 1, MostZoom);
        var scale = Scale * zoom;
        var shift = Right * ((left + right) / 2) + Up * ((low + high) / 2);

        var pixels = new byte[Size * Size * 4];
        for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                var from = middle + shift + Right * ((x + 0.5f - Size / 2f) / scale) + Up * ((Size / 2f - y - 0.5f) / scale) + ToEye * 4;
                float nearest = float.MaxValue;
                int face = -1, surface = 0;
                foreach (var (min, max, top, side) in boxes)
                    if (Enter(from, -ToEye, min, max, out var t, out var axis) && t < nearest)
                        (nearest, face, surface) = (t, axis, axis == 1 ? top : side);
                if (face < 0) continue;

                var look = Surfaces.All[surface];
                // The top brightest, the south side a fifth darker and the east side darker still.
                var shade = look.Emits ? 1 : face == 1 ? 1 : face == 2 ? 0.8f : 0.62f;
                var speckle = 1 + ((int)(PlaceHash.Of(x, y, surface) % 9) - 4) * 0.012f;
                var color = look.Plain;
                var at = (y * Size + x) * 4;
                pixels[at] = Channel(color.R * shade * speckle);
                pixels[at + 1] = Channel(color.G * shade * speckle);
                pixels[at + 2] = Channel(color.B * shade * speckle);
                // Opaque, so glass, ice and water show their color rather than the slot through it.
                pixels[at + 3] = 255;
            }
        return pixels;
    }

    // Where a way from a point first enters a box, and across which axis's face, 0 for x, 1 for y
    // and 2 for z, by the slabs between each pair of the box's faces.
    private static bool Enter(Vector3 from, Vector3 way, Vector3 min, Vector3 max, out float t, out int axis)
    {
        (t, axis) = (float.MinValue, -1);
        var leave = float.MaxValue;
        for (int i = 0; i < 3; i++)
        {
            float a = (min[i] - from[i]) / way[i], b = (max[i] - from[i]) / way[i];
            if (a > b) (a, b) = (b, a);
            if (a > t) (t, axis) = (a, i);
            leave = MathF.Min(leave, b);
        }
        return t <= leave && leave > 0;
    }

    // A stroke of an icon from one point to another in a square from 0 to 1 with y down, so wide as
    // a share of the icon, and a dot where its ends meet.
    private readonly record struct Mark(Vector2 From, Vector2 To, float Width, Color Color);

    private static Mark M(float x0, float y0, float x1, float y1, float width, Color color) => new(new Vector2(x0, y0), new Vector2(x1, y1), width, color);

    private static Mark Dot(float x, float y, float radius, Color color) => new(new Vector2(x, y), new Vector2(x, y), radius * 2, color);

    // The marks of an item that places no block, a later mark over an earlier one.
    private static Mark[] Marks(Item item)
    {
        var c = item.Color;
        return item.Tool switch
        {
            ToolKind.Pickaxe => [M(0.22f, 0.82f, 0.66f, 0.38f, 0.12f, Handle), M(0.28f, 0.2f, 0.62f, 0.2f, 0.13f, c), M(0.62f, 0.2f, 0.8f, 0.38f, 0.13f, c), M(0.8f, 0.38f, 0.8f, 0.72f, 0.13f, c)],
            ToolKind.Axe => [M(0.22f, 0.82f, 0.7f, 0.32f, 0.12f, Handle), M(0.46f, 0.2f, 0.74f, 0.48f, 0.24f, c)],
            ToolKind.Shovel => [M(0.22f, 0.82f, 0.6f, 0.42f, 0.12f, Handle), M(0.6f, 0.4f, 0.76f, 0.24f, 0.26f, c)],
            ToolKind.Sword => [M(0.42f, 0.58f, 0.82f, 0.18f, 0.13f, c), M(0.26f, 0.5f, 0.5f, 0.74f, 0.1f, Handle), M(0.18f, 0.82f, 0.36f, 0.64f, 0.12f, Handle)],
            _ => item.Key switch
            {
                "stick" => [M(0.25f, 0.8f, 0.75f, 0.2f, 0.13f, c)],
                "coal" or "charcoal" => [Dot(0.45f, 0.56f, 0.28f, c), Dot(0.6f, 0.42f, 0.22f, c), Dot(0.36f, 0.4f, 0.17f, c)],
                "iron_ingot" => [M(0.2f, 0.62f, 0.8f, 0.46f, 0.26f, c)],
                "apple" => [Dot(0.5f, 0.58f, 0.3f, c), M(0.5f, 0.3f, 0.56f, 0.14f, 0.08f, Handle), Dot(0.66f, 0.22f, 0.08f, new Color(70, 150, 50))],
                "bread" => [M(0.2f, 0.62f, 0.8f, 0.42f, 0.3f, c)],
                "leather" => [M(0.32f, 0.5f, 0.68f, 0.5f, 0.52f, c)],
                "feather" => [M(0.34f, 0.68f, 0.72f, 0.22f, 0.26f, c), M(0.22f, 0.86f, 0.76f, 0.16f, 0.07f, new Color(170, 170, 170))],
                "wheat_sheaf" => [M(0.5f, 0.86f, 0.3f, 0.26f, 0.08f, c), M(0.5f, 0.86f, 0.5f, 0.2f, 0.08f, c), M(0.5f, 0.86f, 0.7f, 0.26f, 0.08f, c),
                    Dot(0.3f, 0.22f, 0.1f, c), Dot(0.5f, 0.16f, 0.1f, c), Dot(0.7f, 0.22f, 0.1f, c)],
                "rotten_flesh" => [M(0.22f, 0.6f, 0.78f, 0.4f, 0.32f, c), Dot(0.6f, 0.46f, 0.08f, new Color(110, 130, 60)), Dot(0.36f, 0.58f, 0.06f, new Color(110, 130, 60))],
                "bone" => [M(0.26f, 0.74f, 0.74f, 0.26f, 0.12f, c), Dot(0.2f, 0.72f, 0.09f, c), Dot(0.28f, 0.8f, 0.09f, c), Dot(0.72f, 0.2f, 0.09f, c), Dot(0.8f, 0.28f, 0.09f, c)],
                "arrow" => [M(0.2f, 0.8f, 0.76f, 0.24f, 0.07f, Handle), M(0.68f, 0.32f, 0.84f, 0.16f, 0.16f, new Color(170, 170, 180)),
                    M(0.14f, 0.74f, 0.26f, 0.86f, 0.14f, new Color(236, 236, 236))],
                "gunpowder" => [Dot(0.34f, 0.66f, 0.15f, c), Dot(0.54f, 0.6f, 0.18f, c), Dot(0.7f, 0.7f, 0.12f, c), Dot(0.48f, 0.44f, 0.14f, c)],
                "string" => [M(0.18f, 0.72f, 0.38f, 0.4f, 0.07f, c), M(0.38f, 0.4f, 0.6f, 0.62f, 0.07f, c), M(0.6f, 0.62f, 0.82f, 0.3f, 0.07f, c)],
                "spider_eye" => [Dot(0.5f, 0.55f, 0.3f, c), Dot(0.56f, 0.5f, 0.11f, new Color(40, 10, 20)), Dot(0.38f, 0.42f, 0.05f, new Color(250, 250, 250))],
                "raw_porkchop" or "cooked_porkchop" => [Dot(0.44f, 0.56f, 0.28f, c), M(0.5f, 0.5f, 0.74f, 0.3f, 0.2f, c), Dot(0.8f, 0.24f, 0.08f, BoneWhite)],
                "raw_beef" or "steak" => [M(0.22f, 0.6f, 0.78f, 0.42f, 0.36f, c), M(0.3f, 0.62f, 0.7f, 0.48f, 0.05f, new Color(240, 222, 204))],
                "raw_mutton" or "cooked_mutton" => [M(0.3f, 0.7f, 0.62f, 0.38f, 0.32f, c), M(0.62f, 0.38f, 0.8f, 0.2f, 0.09f, BoneWhite), Dot(0.82f, 0.16f, 0.07f, BoneWhite)],
                "raw_chicken" or "cooked_chicken" => [Dot(0.42f, 0.6f, 0.26f, c), M(0.56f, 0.46f, 0.78f, 0.24f, 0.09f, BoneWhite), Dot(0.8f, 0.2f, 0.07f, BoneWhite), Dot(0.84f, 0.26f, 0.06f, BoneWhite)],
                _ => [Dot(0.5f, 0.5f, 0.32f, c)],
            },
        };
    }

    // An item drawn from its marks, then given its edge, an outline in a dark shade of the color
    // beside it, its upper left edge lit and its lower right shaded.
    private static byte[] Drawn(Item item)
    {
        var marks = Marks(item);
        var colors = new Color?[Size, Size];
        for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                var at = new Vector2(x + 0.5f, y + 0.5f) / Size;
                foreach (var mark in marks)
                    if (Distance(at, mark.From, mark.To) <= mark.Width / 2) colors[x, y] = mark.Color;
            }

        bool Filled(int x, int y) => x is >= 0 and < Size && y is >= 0 and < Size && colors[x, y] is not null;
        var pixels = new byte[Size * Size * 4];
        for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                Color color;
                float shade;
                if (colors[x, y] is { } own)
                {
                    color = own;
                    shade = !Filled(x - 1, y) || !Filled(x, y - 1) ? 1.22f : !Filled(x + 1, y) || !Filled(x, y + 1) ? 0.76f : 1;
                }
                else if (new[] { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) }.FirstOrDefault(p => Filled(p.Item1, p.Item2), (-1, -1)) is var (bx, by) && bx >= 0)
                    (color, shade) = (colors[bx, by]!.Value, 0.32f);
                else continue;
                var at = (y * Size + x) * 4;
                (pixels[at], pixels[at + 1], pixels[at + 2], pixels[at + 3]) = (Channel(color.R * shade), Channel(color.G * shade), Channel(color.B * shade), 255);
            }
        return pixels;
    }

    private static float Distance(Vector2 point, Vector2 from, Vector2 to)
    {
        var along = to - from;
        var t = along.LengthSquared() > 0 ? Math.Clamp(Vector2.Dot(point - from, along) / along.LengthSquared(), 0, 1) : 0;
        return Vector2.Distance(point, from + along * t);
    }

    private static byte Channel(float value) => (byte)Math.Clamp(value, 0, 255);
}
