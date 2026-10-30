using System.Numerics;

namespace VoxelSandbox;

/// <summary>
/// Which way a block smaller than its cell faces, up where it stands on the block below, down where
/// it hangs from the block above, and toward a side where it is fixed to the block behind it, as
/// Minecraft's torch on a wall faces away from the wall. North is toward -z and east toward +x, as
/// Minecraft has them.
/// </summary>
public enum Facing
{
    Up,
    Down,
    North,
    South,
    West,
    East,
}

/// <summary>
/// The form stairs take from the stairs beside them, as Minecraft's do, straight, or turned into
/// an inner corner, the step and a quarter before it, or an outer one, a quarter of the step, toward
/// the left or the right of the way they climb.
/// </summary>
public enum StairForm
{
    Straight,
    InnerLeft,
    InnerRight,
    OuterLeft,
    OuterRight,
}

/// <summary>
/// The parts of a texture a piece's faces show, as Minecraft's models give a face's coordinates,
/// each from its left and top corner to its right and bottom one in sixteenths of the texture as
/// seen from outside, one the four sides share, one for the top and one for the bottom. A flat wall
/// shows the sides' part from its first corner's end to its second's.
/// </summary>
public readonly record struct Parts(Vector4 Sides, Vector4 Top, Vector4 Bottom)
{
    /// <summary>A flat wall's part, from the end at its first corner to the end at its second.</summary>
    public static Parts Wall(float u0, float v0, float u1, float v1) => new(new Vector4(u0, v0, u1, v1), default, default);
}

/// <summary>
/// A box of a block smaller than its cell, between two corners in the cell's own space from 0 to 1,
/// of one surface, its top face moved by <see cref="Lean"/>, as a torch on a wall leans out from it,
/// or where it is a <see cref="Plane"/> a flat wall standing from its first corner across to its
/// second, as a plant's are. Its faces show the <see cref="Parts"/> of its texture where it has them,
/// and otherwise the part its place in the cell covers. A box shows the <see cref="Faces"/> its bits
/// name, a bit for each in the order +x, -x, +y, -y, +z and -z, all six unless it is given fewer, as
/// an element of the source's models lists the faces it has.
/// </summary>
public readonly record struct Piece(Vector3 From, Vector3 To, int Surface, Vector3 Lean = default, bool Plane = false, Parts? Parts = null, int Faces = Piece.All)
{
    /// <summary>The bits of all six faces.</summary>
    public const int All = 63;

    /// <summary>Whether it shows a face by its number in the order +x, -x, +y, -y, +z and -z.</summary>
    public bool Shows(int face) => (Faces >> face & 1) != 0;

    /// <summary>Whether its surface is emissive, so it is drawn as a lamp's cube rather than as faces of its section.</summary>
    public bool Glows => Surfaces.All[Surface].Emits;
}

/// <summary>A box in a cell's own space from 0 to 1, between its lowest and highest corners.</summary>
public readonly record struct Box(Vector3 Min, Vector3 Max)
{
    /// <summary>The whole cell.</summary>
    public static readonly Box Cell = new(Vector3.Zero, Vector3.One);
}

/// <summary>
/// The boxes a block smaller than its cell is made of, as a torch, a lantern, a fence, a slab or
/// stairs are, the way it faces and the box around them all, which the crosshair meets and the
/// outline is drawn on.
/// </summary>
/// <remarks>
/// <para>
/// An emissive box is drawn as a lamp's cube of its own scaled to it, square to the axes, so global
/// illumination carries it as a small light by its box, and the other boxes are faces of its
/// section's mesh. Light passes through such a block and it hides none of the faces beside
/// it. A torch, a lantern or an end rod falls when the block it holds to is broken; a fence, a slab
/// or stairs stand where they are put.
/// </para>
/// <para>
/// A solid one stops the player at its boxes, a fence at a box half a block taller than its post,
/// as Minecraft's is, so it cannot be jumped over.
/// </para>
/// </remarks>
public sealed class BlockShape
{
    public BlockShape(Facing facing, Piece[] pieces, bool falls = true, bool solid = false, bool upper = false, bool open = false, Box[]? collision = null, StairForm? form = null,
        bool cutout = false, PlantOffset offset = PlantOffset.None, Box[]? outline = null, bool whole = false, Piece[]? plain = null)
    {
        Facing = facing;
        Plain = plain;
        Whole = whole;
        Form = form;
        Cutout = cutout;
        Offset = offset;
        Falls = falls;
        Upper = upper;
        Open = open;
        Pieces = pieces;
        (Min, Max) = (Vector3.One, Vector3.Zero);
        var boxes = new List<Box>();
        foreach (var piece in pieces)
        {
            if (piece.Glows && piece.Lean != Vector3.Zero) throw new ArgumentException("An emissive box is drawn as a cube square to the axes, so it cannot lean.");
            var (low, high) = (Vector3.One, Vector3.Zero);
            foreach (var corner in Corners(piece))
            {
                low = Vector3.Min(low, corner);
                high = Vector3.Max(high, corner);
            }
            (Min, Max) = (Vector3.Min(Min, low), Vector3.Max(Max, high));
            boxes.Add(new Box(low, high));
        }
        Boxes = outline ?? [.. boxes];
        Collision = !solid ? [] : collision ?? [.. boxes];
    }

    private (Vector3 From, Vector3 To)[]? _edges;

    public Facing Facing { get; }

    /// <summary>Whether it falls when the block it holds to is broken, as a torch does and a fence does not.</summary>
    public bool Falls { get; }

    /// <summary>Whether it is the upper of the two ways a slab or stairs are placed, as one placed against a ceiling is.</summary>
    public bool Upper { get; }

    /// <summary>Whether it is a door swung open, which a player places shut.</summary>
    public bool Open { get; }

    /// <summary>The form of stairs, or null for a shape that is not stairs.</summary>
    public StairForm? Form { get; }

    /// <summary>
    /// Whether its faces are cut out where a pack's texture is clear, as a plant's are, drawn apart
    /// from the solid faces so those keep the depth test that runs before their pixels are lit.
    /// </summary>
    public bool Cutout { get; }

    /// <summary>How a plant stands off its cell's middle, by its place's hash, as Minecraft offsets its flowers and grass.</summary>
    public PlantOffset Offset { get; }

    /// <summary>
    /// Whether its item, on the ground, in the hand and as an icon, is the whole cube of its block's
    /// pictures, as a cactus's is, its model filling its cell but for its sides standing a pixel in.
    /// </summary>
    public bool Whole { get; }

    public IReadOnlyList<Piece> Pieces { get; }

    /// <summary>
    /// The pieces drawn in place of <see cref="Pieces"/> where the pictures cut nothing out, or none
    /// for a shape drawn alike under every picture.
    /// </summary>
    public IReadOnlyList<Piece>? Plain { get; }

    /// <summary>The boxes that stop the player, none for one the player walks through.</summary>
    public IReadOnlyList<Box> Collision { get; }

    /// <summary>The box around each of its pieces, or the boxes it is given, which the crosshair meets and the outline is drawn on.</summary>
    public IReadOnlyList<Box> Boxes { get; }

    /// <summary>
    /// The edges of the outline of its boxes together, in the cell's own space, each where their
    /// faces meet at an angle and not where two boxes' faces lie in one plane, as Minecraft outlines
    /// a stair's step and its slab as one shape.
    /// </summary>
    public IReadOnlyList<(Vector3 From, Vector3 To)> Edges => _edges ??= Outline(Boxes);

    /// <summary>The corner of the box around every piece nearest the cell's origin.</summary>
    public Vector3 Min { get; }

    /// <summary>The corner of the box around every piece farthest from the cell's origin.</summary>
    public Vector3 Max { get; }

    /// <summary>Whether any of its pieces is emissive.</summary>
    public bool Glows => Pieces.Any(p => p.Glows);

    /// <summary>The step from its cell to the block it holds to, the one behind the way it faces.</summary>
    public (int X, int Y, int Z) Support => Facing switch
    {
        Facing.Up => (0, -1, 0),
        Facing.Down => (0, 1, 0),
        Facing.North => (0, 0, 1),
        Facing.South => (0, 0, -1),
        Facing.West => (1, 0, 0),
        _ => (-1, 0, 0),
    };

    /// <summary>The way a block placed against a face faces, away from the block whose face it is, from that face's outward normal.</summary>
    public static Facing Away(int x, int y, int z) => (x, y, z) switch
    {
        (0, 1, 0) => Facing.Up,
        (0, -1, 0) => Facing.Down,
        (0, 0, -1) => Facing.North,
        (0, 0, 1) => Facing.South,
        (-1, 0, 0) => Facing.West,
        _ => Facing.East,
    };

    // The edges of boxes together. Along each axis, each box's four edges that way are cut where
    // any box begins or ends along it, and a piece of an edge is kept where the four places around
    // it, a hair off it across the other two axes, are one or three inside the boxes, or two that
    // touch only at the edge, which is where the faces about it meet at an angle.
    private static (Vector3 From, Vector3 To)[] Outline(IReadOnlyList<Box> boxes)
    {
        const float Hair = 1e-3f;
        var edges = new HashSet<(Vector3 From, Vector3 To)>();
        bool Inside(Vector3 p) => boxes.Any(b => p.X > b.Min.X && p.X < b.Max.X && p.Y > b.Min.Y && p.Y < b.Max.Y && p.Z > b.Min.Z && p.Z < b.Max.Z);
        for (int along = 0; along < 3; along++)
        {
            int first = (along + 1) % 3, second = (along + 2) % 3;
            var cuts = boxes.SelectMany(b => new[] { b.Min[along], b.Max[along] }).Distinct().Order().ToArray();
            foreach (var box in boxes)
                foreach (var u in (ReadOnlySpan<float>)[box.Min[first], box.Max[first]])
                    foreach (var v in (ReadOnlySpan<float>)[box.Min[second], box.Max[second]])
                        for (int i = 0; i + 1 < cuts.Length; i++)
                        {
                            float from = cuts[i], to = cuts[i + 1];
                            if (from < box.Min[along] || to > box.Max[along]) continue;
                            var middle = Point(along, (from + to) / 2, first, u, second, v);
                            bool a = Inside(middle + Point(first, Hair, second, Hair)), b = Inside(middle + Point(first, Hair, second, -Hair));
                            bool c = Inside(middle + Point(first, -Hair, second, -Hair)), d = Inside(middle + Point(first, -Hair, second, Hair));
                            var count = (a ? 1 : 0) + (b ? 1 : 0) + (c ? 1 : 0) + (d ? 1 : 0);
                            if (count is 1 or 3 || count == 2 && a == c)
                                edges.Add((Point(along, from, first, u, second, v), Point(along, to, first, u, second, v)));
                        }
        }
        return [.. edges];
    }

    // A point given by its value on one, two or three axes, zero on the others.
    private static Vector3 Point(int axis, float value, int other, float otherValue, int third = -1, float thirdValue = 0)
    {
        var p = Vector3.Zero;
        p[axis] = value;
        p[other] = otherValue;
        if (third >= 0) p[third] = thirdValue;
        return p;
    }

    /// <summary>A piece's eight corners, its top four moved by its lean.</summary>
    public static IEnumerable<Vector3> Corners(Piece piece)
    {
        for (int i = 0; i < 8; i++)
        {
            var corner = new Vector3(i & 1, (i >> 1) & 1, (i >> 2) & 1);
            yield return piece.From + corner * (piece.To - piece.From) + corner.Y * piece.Lean;
        }
    }

    // A piece given in sixteenths of a block, as Minecraft's models give theirs.
    private static Piece Px(float x0, float y0, float z0, float x1, float y1, float z1, int surface, Vector3 lean = default, Parts? parts = null) =>
        new(new Vector3(x0, y0, z0) / 16, new Vector3(x1, y1, z1) / 16, surface, lean / 16, Parts: parts);

    // The parts of a texture a box shows, its sides', its top's and its bottom's, in sixteenths.
    private static Parts Shows(float u0, float v0, float u1, float v1, float tu0, float tv0, float tu1, float tv1, float bu0, float bv0, float bu1, float bv1) =>
        new(new Vector4(u0, v0, u1, v1), new Vector4(tu0, tv0, tu1, tv1), new Vector4(bu0, bv0, bu1, bv1));

    // A flat wall as the source's model gives it in sixteenths, turned by 45 degrees about the cell's
    // upright middle line as the model turns it, so its first corner's end, the lower along x or z,
    // turns to the lower along x.
    private static Piece Turned45(float x0, float y0, float z0, float x1, float y1, float z1, int surface, Parts parts)
    {
        var half = MathF.Sqrt(0.5f);
        Vector3 Turn(float x, float y, float z) => new(8 + (x - 8 + z - 8) * half, y, 8 + (z - 8 - (x - 8)) * half);
        return new Piece(Turn(x0, y0, z0) / 16, Turn(x1, y1, z1) / 16, surface, Plane: true, Parts: parts);
    }

    // The source's torch is one box from the foot up 10 sixteenths, its sides showing the torch's
    // texture from row 6 down to 16, its top rows 6 and 7 and its bottom rows 13 and 14, here cut into
    // the stick's 8 rows of wood and the head's 2 lit rows above them, each showing its share, so the
    // head that glows and stands upright holds nothing of the wood.
    private static readonly Parts StickParts = Shows(7, 8, 9, 16, 7, 6, 9, 8, 7, 13, 9, 15);
    private static readonly Parts FlameParts = Shows(7, 6, 9, 8, 7, 6, 9, 8, 7, 13, 9, 15);

    /// <summary>
    /// A cactus as the source's model draws it, its top and bottom across the whole cell, each side
    /// standing a pixel in from its side of the cell and spanning the cell's whole width, so where a
    /// pack's pictures are clear at their edges, as the source's are, they are cut out and the thorns
    /// on the edges stick out past the sides beside them. Each face shows its whole picture as a part,
    /// laid as one laid by its place would be, so its corners carry their places and its shadow is cut
    /// where the picture is clear, and the top's clear border, a pixel past the sides, casts no band
    /// of shadow down them. Where the side's picture cuts nothing out, as the game's own does not,
    /// the cactus is a plain box instead, its sides a pixel in from the cell's showing the picture's
    /// middle fourteen pixels across, so no side shows its edge a pixel past the sides beside it.
    /// The source's box stops the player, a pixel in at each side and a pixel short of the top, and
    /// its outline is a pixel in at each side.
    /// </summary>
    public static BlockShape Cactus(int top, int side, int bottom) => new(Facing.Up,
    [
        new(Vector3.Zero, Vector3.One, top, Parts: WholePicture, Faces: 1 << 2),
        new(Vector3.Zero, Vector3.One, bottom, Parts: WholePicture, Faces: 1 << 3),
        new(new Vector3(0, 0, 1) / 16, new Vector3(16, 16, 15) / 16, side, Parts: WholePicture, Faces: 1 << 4 | 1 << 5),
        new(new Vector3(1, 0, 0) / 16, new Vector3(15, 16, 16) / 16, side, Parts: WholePicture, Faces: 1 << 0 | 1 << 1),
    ], falls: false, solid: true, collision: [new Box(new Vector3(1, 0, 1) / 16, new Vector3(15, 15, 15) / 16)],
        outline: [new Box(new Vector3(1, 0, 1) / 16, new Vector3(15, 16, 15) / 16)], whole: true,
        plain:
        [
            new(Vector3.Zero, Vector3.One, top, Parts: WholePicture, Faces: 1 << 2),
            new(Vector3.Zero, Vector3.One, bottom, Parts: WholePicture, Faces: 1 << 3),
            new(new Vector3(1, 0, 1) / 16, new Vector3(15, 16, 15) / 16, side, Parts: Shows(1, 0, 15, 16, 0, 0, 16, 16, 0, 0, 16, 16), Faces: 1 << 0 | 1 << 1 | 1 << 4 | 1 << 5),
        ]);

    private static readonly Parts WholePicture = Shows(0, 0, 16, 16, 0, 0, 16, 16, 0, 0, 16, 16);

    /// <summary>A torch standing on the block below, a stick with a flame at its top, showing the parts of the torch's texture the source's model gives.</summary>
    public static BlockShape Torch(int flame, int stick) => new(Facing.Up,
    [
        Px(7, 0, 7, 9, 8, 9, stick, parts: StickParts),
        Px(7, 8, 7, 9, 10, 9, flame, parts: FlameParts),
    ]);

    /// <summary>
    /// A torch fixed to the wall behind it, its stick of wood leaning out from the wall's face by
    /// three pixels over its eight, about the 22.5 degrees Minecraft's leans, up to the foot of its lit
    /// head, which stands upright on the stick's top.
    /// </summary>
    public static BlockShape WallTorch(Facing facing, int flame, int stick) => new(facing, Yawed(facing,
        Px(7, 3, 14, 9, 11, 16, stick, new Vector3(0, 0, -3), StickParts),
        Px(7, 11, 11, 9, 13, 13, flame, parts: FlameParts)));

    // The source's lantern shows its texture's rows 2 to 8 on its glass's sides and rows 9 to 14 on
    // its top and bottom, its cap's sides rows 0 and 1 and its top and bottom rows 10 to 13, and its
    // handle's and chain's crossed walls columns 11 to 13 of the rows each covers.
    private static readonly Parts GlassParts = Shows(0, 2, 6, 9, 0, 9, 6, 15, 0, 9, 6, 15);
    private static readonly Parts CapParts = Shows(1, 0, 5, 2, 1, 10, 5, 14, 1, 10, 5, 14);

    /// <summary>
    /// A lantern standing on the block below, its glass lit, and its cap and handle, a pair of
    /// crossed walls, of iron, each showing the parts of the lantern's texture the source's model gives.
    /// </summary>
    public static BlockShape Lantern(int glass, int iron) => new(Facing.Up,
    [
        Px(5, 0, 5, 11, 7, 11, glass, parts: GlassParts),
        Px(6, 7, 6, 10, 9, 10, iron, parts: CapParts),
        Turned45(6.5f, 9, 8, 9.5f, 11, 8, iron, Parts.Wall(11, 1, 14, 3)),
        Turned45(8, 9, 6.5f, 8, 11, 9.5f, iron, Parts.Wall(14, 10, 11, 12)),
    ], solid: true);

    /// <summary>A lantern hanging from the block above by a short chain, a pair of crossed walls.</summary>
    public static BlockShape HangingLantern(int glass, int iron) => new(Facing.Down,
    [
        Px(5, 1, 5, 11, 8, 11, glass, parts: GlassParts),
        Px(6, 8, 6, 10, 10, 10, iron, parts: CapParts),
        Turned45(6.5f, 11, 8, 9.5f, 15, 8, iron, Parts.Wall(11, 1, 14, 5)),
        Turned45(8, 10, 6.5f, 8, 16, 9.5f, iron, Parts.Wall(14, 6, 11, 12)),
    ], solid: true);

    /// <summary>An end rod, an emissive white rod on a small base, its base on the block it is fixed to.</summary>
    public static BlockShape EndRod(Facing facing) => new(facing, Turned(facing,
        Px(6, 0, 6, 10, 1, 10, Surfaces.EndRodBase),
        Px(7, 1, 7, 9, 16, 9, Surfaces.EndRod)), solid: true);

    /// <summary>
    /// A fence's post and two rails toward each side whose bit is set in <paramref name="arms"/>,
    /// north 1, east 2, south 4 and west 8, as the fences and whole blocks beside it join it.
    /// </summary>
    public static BlockShape Fence(int wood, int arms)
    {
        List<Piece> pieces = [Px(6, 0, 6, 10, 16, 10, wood)];
        List<Box> collision = [Tall(6, 6, 10, 10)];
        Facing[] sides = [Facing.North, Facing.East, Facing.South, Facing.West];
        for (int side = 0; side < 4; side++)
        {
            if ((arms & 1 << side) == 0) continue;
            pieces.AddRange(Yawed(sides[side], Px(7, 12, 0, 9, 15, 6, wood), Px(7, 6, 0, 9, 9, 6, wood)));
            var arm = Yawed(sides[side], Px(6, 0, 0, 10, 24, 6, wood))[0];
            collision.Add(new Box(arm.From, arm.To));
        }
        return new BlockShape(Facing.Up, [.. pieces], falls: false, solid: true, collision: [.. collision]);

        static Box Tall(float x0, float z0, float x1, float z1) => new(new Vector3(x0, 0, z0) / 16, new Vector3(x1, 24, z1) / 16);
    }

    /// <summary>
    /// A plant of two flat walls crossed from corner to corner of the cell, as Minecraft draws its grasses,
    /// saplings and flowers, so many sixteenths high, its lower part of one surface and from
    /// <paramref name="split"/> up of another, as a flower's stem and its petals are. A pack's texture
    /// lies across the walls from the cell's foot, so its plant drawn at the bottom of the texture
    /// shows whole, and its clear pixels are cut out. It falls when the block under it is broken.
    /// </summary>
    public static BlockShape Plant(int lower, float height, int upper = -1, float split = 0, PlantOffset offset = PlantOffset.None)
    {
        List<Piece> pieces = [];
        (float From, float To, int Surface)[] parts = split > 0 && upper >= 0 ? [(0, split, lower), (split, height, upper)] : [(0, height, lower)];
        foreach (var (from, to, surface) in parts)
        {
            pieces.Add(Wall(0, from, 0, 16, to, 16, surface));
            pieces.Add(Wall(0, from, 16, 16, to, 0, surface));
        }
        return new BlockShape(Facing.Up, [.. pieces], cutout: true, offset: offset);
    }

    /// <summary>
    /// A fire as the source's models draw it, toward the sides whose bits are set in
    /// <paramref name="sides"/>, north 1, east 2, south 4, west 8 and up 16, a wall of flame a block
    /// and two fifths high against each side, and under the block over it two walls sloping down
    /// from it; or with none, on the ground, four walls leaning across the cell, crossed in pairs,
    /// and the four against its sides. The walls show the fire's texture whole, cut out where it
    /// is clear, and its light is a box inside them that the scene's distance field alone sees. The
    /// crosshair meets it at a sixteenth over its foot, as the source's, and it stands where it is
    /// when the block under it goes, burning out at its next tick where nothing holds it.
    /// </summary>
    public static BlockShape Fire(int sides)
    {
        List<Piece> pieces = [Px(2, 0, 2, 14, 14, 14, Surfaces.FireLight)];
        // The source's walls in sixteenths, each from its foot's first end to its foot's second and
        // up to its top, which leans by the last three, the leaning ones its turns by 22.5 degrees
        // about the cell's middle as its models give them, stretched back across the cell.
        void Add(float x0, float y0, float z0, float x1, float y1, float z1, float leanX = 0, float leanZ = 0) =>
            pieces.Add(new Piece(new Vector3(x0, y0, z0) / 16, new Vector3(x1, y1, z1) / 16, Surfaces.Fire, new Vector3(leanX, 0, leanZ) / 16, Plane: true,
                Parts: Parts.Wall(0, 0, 16, 16)));
        if (sides == 0)
        {
            Add(0, 0.331f, 12.114f, 16, 22.731f, 12.114f, leanZ: -9.279f);
            Add(0, 0.331f, 3.886f, 16, 22.731f, 3.886f, leanZ: 9.279f);
            Add(5.486f, -0.331f, 0, 5.486f, 22.069f, 16, leanX: 9.279f);
            Add(10.514f, -0.331f, 0, 10.514f, 22.069f, 16, leanX: -9.279f);
        }
        if (sides == 0 || (sides & 1) != 0) Add(0, 0, 0.01f, 16, 22.4f, 0.01f);
        if (sides == 0 || (sides & 2) != 0) Add(15.99f, 0, 0, 15.99f, 22.4f, 16);
        if (sides == 0 || (sides & 4) != 0) Add(0, 0, 15.99f, 16, 22.4f, 15.99f);
        if (sides == 0 || (sides & 8) != 0) Add(0.01f, 0, 0, 0.01f, 22.4f, 16);
        if ((sides & 16) != 0)
        {
            Add(0, 9.373f, 0, 0, 16, 16, leanX: 16);
            Add(16, 9.373f, 0, 16, 16, 16, leanX: -16);
        }
        return new BlockShape(Facing.Up, [.. pieces], falls: false, outline: [new Box(Vector3.Zero, new Vector3(1, 1 / 16f, 1))]);
    }

    // A flat wall from one corner across to another in sixteenths, standing from the lower height to the upper.
    private static Piece Wall(float x0, float y0, float z0, float x1, float y1, float z1, int surface) =>
        new(new Vector3(x0, y0, z0) / 16, new Vector3(x1, y1, z1) / 16, surface, Plane: true);

    /// <summary>
    /// Cave vines, two flat walls crossed from corner to corner of the cell as a plant's are, which
    /// hang from the block over them and fall when it is broken, and with glow berries their light, a
    /// box inside them that the scene's distance field alone sees, as a fire's is.
    /// </summary>
    public static BlockShape Vines(int surface, int light = -1)
    {
        List<Piece> pieces = [Wall(0, 0, 0, 16, 16, 16, surface), Wall(0, 0, 16, 16, 16, 0, surface)];
        if (light >= 0) pieces.Add(Px(5, 3, 5, 11, 13, 11, light));
        return new BlockShape(Facing.Down, [.. pieces], cutout: true);
    }

    /// <summary>A carpet of petals a sixteenth high over the ground, as the cherry grove's pink petals lie, cut out where its texture is clear.</summary>
    public static BlockShape Carpet(int surface) => new(Facing.Up, [Px(0, 0, 0, 16, 1, 16, surface)], cutout: true);

    /// <summary>Ripe wheat, four thin stalks' walls crossed as Minecraft's crops are, which falls when its farmland is broken.</summary>
    public static BlockShape Wheat(int stalks) => new(Facing.Up,
    [
        Wall(0, 0, 4, 16, 14, 4, stalks),
        Wall(0, 0, 12, 16, 14, 12, stalks),
        Wall(4, 0, 0, 4, 14, 16, stalks),
        Wall(12, 0, 0, 12, 14, 16, stalks),
    ], cutout: true);

    /// <summary>
    /// A half of a door, a panel three sixteenths thick across the side of its cell away from
    /// <paramref name="facing"/>, the way the player looked placing it, or swung open on its hinge
    /// to lie along the side to the left of that way.
    /// </summary>
    public static BlockShape Door(int wood, Facing facing, bool upper, bool open) => new(facing, Yawed(facing,
        open ? Px(0, 0, 0, 3, 16, 16, wood) : Px(0, 0, 13, 16, 16, 16, wood)), falls: false, solid: true, upper: upper, open: open);

    /// <summary>
    /// A chest, a box fourteen pixels across and fourteen high in the middle of its cell, its latch's
    /// face toward the way it faces, which stops the player at its box as the source's does.
    /// </summary>
    public static BlockShape Chest(Facing facing)
    {
        var front = facing switch { Facing.East => 0, Facing.West => 1, Facing.South => 4, _ => 5 };
        var sides = (1 << 0 | 1 << 1 | 1 << 4 | 1 << 5) & ~(1 << front);
        Piece Box(int surface, int faces) => Px(1, 0, 1, 15, 14, 15, surface) with { Faces = faces };
        var box = new Box(new Vector3(1, 0, 1) / 16, new Vector3(15, 14, 15) / 16);
        return new(facing, [Box(Surfaces.ChestTop, 1 << 2), Box(Surfaces.ChestSide, sides), Box(Surfaces.ChestFront, 1 << front), Box(Surfaces.ChestBottom, 1 << 3)],
            falls: false, solid: true, collision: [box], outline: [box]);
    }

    /// <summary>A cobweb, two walls crossed as a plant's are, which holds to nothing and stays where it is put, as the source's does.</summary>
    public static BlockShape Cobweb(int surface) => new(Facing.Up, [Wall(0, 0, 0, 16, 16, 16, surface), Wall(0, 0, 16, 16, 16, 0, surface)], falls: false, cutout: true);

    /// <summary>A straight rail lying on the block under it, a pixel deep, along the way it faces, which falls when that block is broken.</summary>
    public static BlockShape Rail(int surface, Facing facing) => new(facing, [Px(0, 0, 0, 16, 1, 16, surface)], cutout: true);

    /// <summary>A layer of snow an eighth of a block deep on the block under it, walked on, and gone when that block is broken.</summary>
    public static BlockShape SnowLayer(int surface) => new(Facing.Up, [Px(0, 0, 0, 16, 2, 16, surface)], solid: true);

    /// <summary>A slab, half a block high, in the lower half of its cell or the <paramref name="upper"/> one.</summary>
    public static BlockShape Slab(int surface, bool upper) => new(Facing.Up,
        [upper ? Px(0, 8, 0, 16, 16, 16, surface) : Px(0, 0, 0, 16, 8, 16, surface)], falls: false, solid: true, upper: upper);

    /// <summary>
    /// Stairs climbing toward <paramref name="facing"/>, a slab with a step on its half that way, or
    /// turned over against a ceiling where <paramref name="upper"/>, in a form, the step whole, an
    /// outer corner's quarter of it on the left or the right of that half, or an inner corner's step
    /// with a quarter before it on the left or the right, as Minecraft's stairs models are.
    /// </summary>
    public static BlockShape Stairs(int surface, Facing facing, bool upper, StairForm form = StairForm.Straight)
    {
        // Facing north the step is the north half, z 0 to 8, and its left the west, x 0 to 8.
        float low = upper ? 0 : 8, high = upper ? 8 : 16;
        Piece[] step = form switch
        {
            StairForm.OuterLeft => [Px(0, low, 0, 8, high, 8, surface)],
            StairForm.OuterRight => [Px(8, low, 0, 16, high, 8, surface)],
            StairForm.InnerLeft => [Px(0, low, 0, 16, high, 8, surface), Px(0, low, 8, 8, high, 16, surface)],
            StairForm.InnerRight => [Px(0, low, 0, 16, high, 8, surface), Px(8, low, 8, 16, high, 16, surface)],
            _ => [Px(0, low, 0, 16, high, 8, surface)],
        };
        return new(facing, Yawed(facing, [upper ? Px(0, 8, 0, 16, 16, 16, surface) : Px(0, 0, 0, 16, 8, 16, surface), .. step]),
            falls: false, solid: true, upper: upper, form: form);
    }

    // Pieces given facing north, a wall behind them at +z or a step on their north half, turned about
    // the cell's upright middle line to face another side.
    private static Piece[] Yawed(Facing facing, params Piece[] pieces) => [.. pieces.Select(p => Moved(p, c => facing switch
    {
        Facing.South => new Vector3(-c.X, c.Y, -c.Z),
        Facing.East => new Vector3(-c.Z, c.Y, c.X),
        Facing.West => new Vector3(c.Z, c.Y, -c.X),
        _ => c,
    }))];

    // Pieces given facing up, standing on the block below, turned about the cell's middle to face
    // another way. A piece that leans cannot be turned off its upright, and the parts of a texture
    // a piece shows on its sides would turn onto its ends, so none of these leans or has parts.
    private static Piece[] Turned(Facing facing, params Piece[] pieces) => [.. pieces.Select(p => Moved(p, c => facing switch
    {
        Facing.Down => new Vector3(c.X, -c.Y, -c.Z),
        Facing.North => new Vector3(c.X, c.Z, -c.Y),
        Facing.South => new Vector3(c.X, -c.Z, c.Y),
        Facing.East => new Vector3(c.Y, -c.X, c.Z),
        Facing.West => new Vector3(-c.Y, c.X, c.Z),
        _ => c,
    }))];

    // A piece's corners and lean taken through a turn about the cell's middle, its corners sorted
    // again so its From is the lower.
    private static Piece Moved(Piece piece, Func<Vector3, Vector3> turn)
    {
        var half = new Vector3(0.5f);
        Vector3 a = turn(piece.From - half) + half, b = turn(piece.To - half) + half;
        return piece with { From = Vector3.Min(a, b), To = Vector3.Max(a, b), Lean = turn(piece.Lean) };
    }
}

/// <summary>
/// How a plant stands off its cell's middle, as Minecraft's offset types place it, not at all, as a
/// sapling, a mushroom and a dead bush stand, up to a quarter of a block either way across, as a
/// flower and tall grass do, or across and sunk up to a fifth of a block, as short grass and a fern
/// are.
/// </summary>
public enum PlantOffset
{
    None,
    Across,
    AcrossAndDown,
}
