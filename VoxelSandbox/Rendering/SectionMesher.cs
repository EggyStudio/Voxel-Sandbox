using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>
/// Turns a section into the faces of its blocks that touch air, each corner colored by its surface
/// and shaded by the light and the blocks around it, and the places of the emissive blocks, which
/// are drawn apart.
/// </summary>
/// <remarks>
/// <para>
/// Each face is two triangles of its own, so a flat floor of 256 blocks is 512 triangles, where
/// greedy meshing would join it into a few. The faces are kept apart so a texture can be laid on
/// each later, and so each corner keeps a color of its own. A section's opaque faces are one mesh,
/// their surfaces told apart by their vertices' colors, which the scene's distance field reads as
/// each block's color as the model pass does, and its see-through faces of glass, ice and water a
/// second, each vertex's alpha its surface's.
/// </para>
/// <para>
/// A corner is shaded as Minecraft's smooth lighting shades it, from the four cells in front of the
/// face that meet at the corner, which are the one the face looks into, the two beside it and the
/// one across the corner. Its light is the average of those that are open, and its occlusion counts
/// those that are solid, so a corner where two blocks meet is darker, and both come out of the same
/// four reads. The shade multiplies the surface's color into the vertex's, so a face where neither
/// the sky nor a lamp reaches is drawn nearly black and reflects nearly no indirect light, as an
/// unlit cave in Minecraft shows nothing.
/// </para>
/// <para>
/// The mesher reads the section and the blocks and light of the 26 sections around it into one
/// padded copy, so the loop over its blocks never asks the world where a block is. It is used on
/// the main thread, and holds its lists between calls so meshing allocates nothing but the arrays
/// that are uploaded.
/// </para>
/// </remarks>
public sealed class SectionMesher
{
    private const int P = Section.Size + 2;

    private readonly ushort[] _blocks = new ushort[P * P * P];
    private readonly byte[] _light = new byte[P * P * P];
    private readonly Section?[] _around = new Section?[27];
    private ChunkColumn _column = null!;
    // The section's lowest corner in the world, so a block's place picks its look from a pack's block state.
    private (int X, int Y, int Z) _origin;

    /// <summary>The textures of the packs loaded, or none, whose tiles each face is drawn with.</summary>
    public TerrainAtlas Atlas { get; set; } = TerrainAtlas.None;

    /// <summary>Whether a face's corners are darkened where neither the sky nor a block's light reaches them.</summary>
    public bool LightLevels { get; set; } = true;

    /// <summary>Whether a face's corners are darkened where blocks meet around them.</summary>
    public bool CornerShade { get; set; } = true;

    /// <summary>
    /// How far the corners darken where blocks meet around them, from 0 for not at all to 1 for
    /// Minecraft's four levels, each level taken that far from 1.
    /// </summary>
    public float CornerStrength { get; set; } = 1;

    /// <summary>
    /// Faces as a mesh, with the position of each corner, eight packed bytes a corner
    /// (<see cref="Pack"/>) that the terrain shader unpacks into its normal, its tiles and its
    /// color, the color of each corner as the scene's light field reads it, its surface's
    /// darkened by its shade, and their triangles.
    /// </summary>
    public sealed class Faces
    {
        public readonly List<Vector3> Positions = [];
        public readonly List<uint> Packed = [];
        public readonly List<Color> Colors = [];
        public readonly List<uint> Indices = [];

        /// <summary>The corners held.</summary>
        public int Count => Positions.Count;

        public void Clear()
        {
            Positions.Clear();
            Packed.Clear();
            Colors.Clear();
            Indices.Clear();
        }

        /// <summary>
        /// A corner of a face, the color it carries under the shade from 0 to 1, its tiles as
        /// <see cref="TerrainAtlas.Paint"/> packs them, and the face's color as the scene's light
        /// field reads it, the carried color where none is given.
        /// </summary>
        public void Add(Vector3 position, int face, int corner, float shade, Color tint, uint tiles = 0, Color? albedo = null)
        {
            var (first, second) = Pack(face, corner, shade, tint, tiles);
            Positions.Add(position);
            Packed.Add(first);
            Packed.Add(second);
            var seen = albedo ?? tint;
            Colors.Add(new Color((byte)MathF.Round(seen.R * shade), (byte)MathF.Round(seen.G * shade), (byte)MathF.Round(seen.B * shade), tint.A));
        }

        /// <summary>The face, corner, shade and tint packed into a corner's eight bytes.</summary>
        public (int Face, int Corner, float Shade, Color Tint) Unpack(int corner) => SectionMesher.Unpack(Packed[2 * corner], Packed[2 * corner + 1]);
    }

    /// <summary>
    /// A corner's eight bytes as two uints. The first holds the face it lies on, 0 to 7 in the
    /// order of <c>Normals</c>, at its low 3 bits, which of the face's four corners it is at the
    /// next 2, its shade from 0 to 127 at the 7 above, whether its overlay is left untinted at bit
    /// 12, and its tiles in the 19 bits from bit 13, the surface's tile, its overlay's and whether
    /// the tile under the overlay is left untinted, and the second the color it carries, red, green
    /// and blue a byte each from the lowest, and its alpha in the highest byte. The color is sRGB,
    /// as a vertex color is, and the shade multiplies it in that space. A shade of 128 levels holds
    /// the light's sixteen levels and the corners' four darkenings with room to spare. A face with no
    /// overlay and neither flag may carry its corner's place on its texture in the overlay's bits
    /// instead, as <see cref="PlaceOf"/> reads it.
    /// </summary>
    public static (uint First, uint Second) Pack(int face, int corner, float shade, Color tint, uint tiles = 0)
    {
        var level = (uint)Math.Clamp(MathF.Round(shade * 127), 0, 127);
        return ((uint)face | (uint)corner << 3 | level << 5 | (tiles >> 19 & 1) << 12 | (tiles & 0x7FFFF) << 13,
            tint.R | (uint)tint.G << 8 | (uint)tint.B << 16 | (uint)tint.A << 24);
    }

    /// <summary>A corner's tiles read back from the first of its uints, as <see cref="TerrainAtlas.Paint"/> packs them.</summary>
    public static uint TilesOf(uint first) => first >> 13 | (first >> 12 & 1) << 19;

    /// <summary>A corner's eight bytes read back, as the terrain shader reads them.</summary>
    public static (int Face, int Corner, float Shade, Color Tint) Unpack(uint first, uint second) =>
        ((int)(first & 7), (int)(first >> 3 & 3), (first >> 5 & 127) / 127f,
            new Color((byte)second, (byte)(second >> 8), (byte)(second >> 16), (byte)(second >> 24)));

    /// <summary>The outward normal of a face by its number, as the terrain shader's table holds it.</summary>
    public static Vector3 Normal(int face) => Normals[face];

    /// <summary>A corner of a box's face, 0 or 1 along each axis, counterclockwise seen from outside, as the faces are meshed.</summary>
    public static Vector3 Corner(int face, int corner) => Corners[face][corner];

    /// <summary>
    /// The place on its texture in sixteenths a corner carries in the first of its uints, across and
    /// down, or null for a corner whose texture is laid by where it lies in its block.
    /// </summary>
    public static Vector2? PlaceOf(uint first)
    {
        var placed = first >> 22 & 511;
        if (placed == 0 || first >> 31 != 0 || (first >> 12 & 1) != 0) return null;
        return new Vector2((placed - 1) % 17, (placed - 1) / 17);
    }

    /// <summary>
    /// Where a corner of a box's face lies on the part of its texture the face shows, in sixteenths,
    /// across from the part's left and down from its top as seen from outside, as Minecraft lays a
    /// model's face on its texture, from where the corner lies on its box, 0 or 1 along each axis.
    /// </summary>
    public static Vector2 Place(Parts parts, int face, Vector3 corner)
    {
        var part = face == 2 ? parts.Top : face == 3 ? parts.Bottom : parts.Sides;
        var (across, down) = face switch
        {
            0 => (1 - corner.Z, 1 - corner.Y),
            1 => (corner.Z, 1 - corner.Y),
            2 => (corner.X, corner.Z),
            3 => (corner.X, 1 - corner.Z),
            4 => (corner.X, 1 - corner.Y),
            _ => (1 - corner.X, 1 - corner.Y),
        };
        return new Vector2(part.X + (part.Z - part.X) * across, part.Y + (part.W - part.Y) * down);
    }

    /// <summary>The last section's faces of opaque blocks.</summary>
    public Faces Solid { get; } = new();

    /// <summary>The last section's faces of see-through blocks, glass, ice and water, each vertex's alpha its surface's.</summary>
    public Faces SeeThrough { get; } = new();

    /// <summary>
    /// The last section's faces cut out where a pack's texture is clear whose shadow is cut by the
    /// texture too, a plant's and every flat wall's, as a lantern's handle is.
    /// </summary>
    public Faces Cutout { get; } = new();

    /// <summary>The last section's faces of glass cut out where a pack's texture is clear, which cast no shadow, as glass the pack leaves whole casts none.</summary>
    public Faces CutoutGlass { get; } = new();

    /// <summary>
    /// The last section's faces cut out where a pack's texture is clear that cast a shadow, a door's
    /// window and the leaves of a pack whose leaves are clear between them.
    /// </summary>
    public Faces SolidCutout { get; } = new();

    /// <summary>The emissive blocks of the last section, by their index in it.</summary>
    public List<(int Index, BlockId Block)> Emitters { get; } = [];

    // The six directions in the order +x, -x, +y, -y, +z, -z, each face's corners counterclockwise
    // seen from outside the block, which is the side the model pass draws. The terrain shader holds
    // the same six normals, by the face a vertex's packed bytes give.
    private static readonly Vector3[] Normals = [Vector3.UnitX, -Vector3.UnitX, Vector3.UnitY, -Vector3.UnitY, Vector3.UnitZ, -Vector3.UnitZ, Vector3.UnitY, Vector3.UnitY];

    // A plant's wall along x and along z, the two faces after the six, lit as if it faced up and its
    // texture laid along it, as the terrain shader's table holds them.
    private const int WallAlongX = 6, WallAlongZ = 7;
    private static readonly int[] Steps = [1, -1, P * P, -P * P, P, -P];
    private static readonly Vector3[][] Corners =
    [
        [new(1, 0, 0), new(1, 1, 0), new(1, 1, 1), new(1, 0, 1)],
        [new(0, 0, 0), new(0, 0, 1), new(0, 1, 1), new(0, 1, 0)],
        [new(0, 1, 0), new(0, 1, 1), new(1, 1, 1), new(1, 1, 0)],
        [new(0, 0, 0), new(1, 0, 0), new(1, 0, 1), new(0, 0, 1)],
        [new(0, 0, 1), new(1, 0, 1), new(1, 1, 1), new(0, 1, 1)],
        [new(0, 0, 0), new(0, 1, 0), new(1, 1, 0), new(1, 0, 0)],
    ];
    // For each face and corner, the steps in the padded copy from the cell the face looks into to the
    // two cells beside it toward the corner and to the cell across the corner.
    private static readonly int[,] Side1 = new int[6, 4], Side2 = new int[6, 4], Across = new int[6, 4];

    // How much of its color a corner keeps on the screen for each count of the three cells around it
    // left open, as Minecraft's corners darken.
    private static readonly float[] Occlusion = [0.45f, 0.62f, 0.8f, 1];

    static SectionMesher()
    {
        int[] unit = [1, P * P, P];
        for (int face = 0; face < 6; face++)
        {
            var normal = face / 2;
            int first = normal == 0 ? 1 : 0, second = normal == 2 ? 1 : 2;
            for (int corner = 0; corner < 4; corner++)
            {
                var at = Corners[face][corner];
                int s1 = Component(at, first) > 0.5f ? unit[first] : -unit[first];
                int s2 = Component(at, second) > 0.5f ? unit[second] : -unit[second];
                (Side1[face, corner], Side2[face, corner], Across[face, corner]) = (s1, s2, s1 + s2);
            }
        }

    }

    private static int Pad(int x, int y, int z) => ((y + 1) * P + z + 1) * P + x + 1;

    /// <summary>
    /// The share of the light reaching it that a face keeps at a light level from 0 to 15, in linear
    /// light, nearly all of it down to a few levels from dark and next to none at 0.
    /// </summary>
    /// <remarks>
    /// Global illumination brings a lamp's falloff itself, so the level marks where no light
    /// reaches rather than dimming the light by distance a second time, as Minecraft's own curve
    /// would, which keeps a quarter at level 9 on the screen and a twentieth of the light.
    /// </remarks>
    public static float Lit(float level)
    {
        var dark = 1 - level / Lighting.Max;
        return 1 - 0.98f * dark * dark * dark * dark;
    }

    /// <summary>Meshes a section into <see cref="Solid"/>, <see cref="SeeThrough"/>, <see cref="Cutout"/>, <see cref="CutoutGlass"/>, <see cref="SolidCutout"/> and <see cref="Emitters"/>.</summary>
    /// <returns>False when one of the eight columns around it is not loaded, so its edges cannot be told yet, and nothing is meshed.</returns>
    public bool Mesh(VoxelWorld world, SectionKey key)
    {
        Solid.Clear();
        SeeThrough.Clear();
        Cutout.Clear();
        CutoutGlass.Clear();
        SolidCutout.Clear();
        Emitters.Clear();
        _origin = (key.X * Section.Size, key.Y * Section.Size, key.Z * Section.Size);

        for (int dz = -1; dz <= 1; dz++)
            for (int dx = -1; dx <= 1; dx++)
            {
                if (!world.TryGetColumn(key.X + dx, key.Z + dz, out var column)) return false;
                if (dx == 0 && dz == 0) _column = column;
                for (int dy = -1; dy <= 1; dy++)
                    _around[(dy + 1) * 9 + (dz + 1) * 3 + dx + 1] = (uint)(key.Y + dy) < ChunkColumn.SectionCount ? column.Sections[key.Y + dy] : null;
            }
        if (_around[13] is not { Filled: > 0 }) return true;

        Fill();
        for (int y = 0; y < Section.Size; y++)
            for (int z = 0; z < Section.Size; z++)
                for (int x = 0; x < Section.Size; x++)
                {
                    var at = Pad(x, y, z);
                    var block = (BlockId)_blocks[at];
                    if (block == BlockId.Air) continue;
                    var info = Blocks.Get(block);
                    if (info.Glows.Length > 0) Emitters.Add((Section.Index(x, y, z), block));
                    if (info.Shape is { } shape)
                    {
                        // A fence joins the fences and whole blocks on its four sides, and stairs
                        // turn into a corner by the stairs before and behind them.
                        if (Blocks.Joins(block))
                            shape = Blocks.ShapeAmong(block, (BlockId)_blocks[at + Steps[5]], (BlockId)_blocks[at + Steps[0]], (BlockId)_blocks[at + Steps[4]], (BlockId)_blocks[at + Steps[1]])!;
                        AddShape(shape, at, new Vector3(x, y, z));
                        continue;
                    }
                    if (info.Emits) continue;
                    // Water stands lower the further it has flowed, a source's top an eighth low, and
                    // fills its cell under more water. Beside lower water its side shows above the
                    // other's top, and beside water as high it shows nothing.
                    var water = Blocks.IsWater(block);
                    var top = water ? Blocks.WaterHeight(block, Blocks.IsWater((BlockId)_blocks[at + Steps[2]])) : 1;
                    for (int face = 0; face < 6; face++)
                    {
                        var neighbor = (BlockId)_blocks[at + Steps[face]];
                        var bottom = 0f;
                        if (water && Blocks.IsWater(neighbor))
                        {
                            if (face is 2 or 3) continue;
                            bottom = Blocks.WaterHeight(neighbor, Blocks.IsWater((BlockId)_blocks[at + Steps[face] + Steps[2]]));
                            if (bottom >= top) continue;
                        }
                        else if (!Shows(block, info.Kind, neighbor)) continue;
                        Add(face == 2 ? info.Top : face == 3 ? info.Bottom : info.Side, info.Kind == BlockKind.Glass || Atlas.CutOut(block), info.Kind, face, at, new Vector3(x, y, z), bottom, top);
                    }
                }
        return true;
    }

    // Whether a block's face shows beside a neighbor. An opaque block shows a face to anything that
    // does not hide it, and a see-through block to anything that neither hides it nor is of its own
    // kind, so two panes of glass or two blocks of water meet with no face between them. A block cut
    // out where a pack's texture is clear hides nothing, so its neighbors and its own kind beside it
    // keep their faces, as Minecraft draws its leaves. An opaque block keeps its face against a lamp
    // too, hidden behind the lamp's cube, so the mesh stays closed around the lamp, since the scene's
    // distance field reads a cell whose nearest point lies on an edge no other triangle of its mesh
    // shares as open, and would read the stone beside a lamp as the air in front of the lamp's faces.
    private bool Shows(BlockId block, BlockKind kind, BlockId neighbor)
    {
        if (Atlas.Hides(neighbor)) return kind == BlockKind.Opaque && Blocks.IsLamp(neighbor);
        return kind == BlockKind.Opaque || neighbor != block;
    }

    // The faces a face of a surface goes with, cut out where a pack's texture is clear and the block
    // may be cut out, casting a shadow for an opaque block's and a smaller block's and none for
    // glass's, or else see-through where the surface's own color is, and solid.
    private Faces FacesOf(int surface, bool cuttable, BlockKind kind) =>
        cuttable && Atlas.Cuts(surface) ? kind == BlockKind.Glass ? CutoutGlass : SolidCutout
            : Surfaces.All[surface].Color.A < 255 ? SeeThrough : Solid;

    // A face of a block, its corners on the cell's floor raised to bottom and those on its ceiling
    // lowered to top, for water and for a side shown only above the water beside it.
    private void Add(int surface, bool cuttable, BlockKind kind, int face, int at, Vector3 block, float bottom, float top)
    {
        // A tinted surface takes its column's grass or foliage color at the block's place.
        var place = (int)block.Z * Section.Size + (int)block.X;
        var spot = PlaceHash.Of(_origin.X + (int)block.X, _origin.Z + (int)block.Z, _origin.Y + (int)block.Y);
        var (tiles, color, albedo) = Atlas.Paint(surface, _column.GrassTint[place], _column.FoliageTint[place], spot);
        var faces = FacesOf(surface, cuttable, kind);
        var first = (uint)faces.Count;
        var front = at + Steps[face];
        Span<int> occlusion = stackalloc int[4];

        for (int i = 0; i < 4; i++)
        {
            int side1 = front + Side1[face, i], side2 = front + Side2[face, i], across = front + Across[face, i];
            bool closed1 = Blocks.IsOpaque((BlockId)_blocks[side1]), closed2 = Blocks.IsOpaque((BlockId)_blocks[side2]), closed3 = Blocks.IsOpaque((BlockId)_blocks[across]);
            // Two blocks on either side close the corner whatever is across it, as Minecraft has it.
            occlusion[i] = closed1 && closed2 ? 0 : 3 - (closed1 ? 1 : 0) - (closed2 ? 1 : 0) - (closed3 ? 1 : 0);

            int sky = _light[front] >> 4, lit = _light[front] & 15, open = 1;
            if (!closed1) Gather(side1, ref sky, ref lit, ref open);
            if (!closed2) Gather(side2, ref sky, ref lit, ref open);
            if (!closed3 && !(closed1 && closed2)) Gather(across, ref sky, ref lit, ref open);

            // The light kept is linear and the vertex's color sRGB, so it is encoded as the color is.
            var kept = LightLevels ? MathF.Pow(Lit((float)Math.Max(sky, lit) / open), 1 / 2.2f) : 1;
            var shade = kept * (CornerShade ? 1 - CornerStrength * (1 - Occlusion[occlusion[i]]) : 1);
            var corner = Corners[face][i];
            corner.Y = corner.Y == 1 ? top : bottom;
            faces.Add(block + corner, face, i, shade, color, tiles, albedo);
        }

        // The quad is split along the diagonal whose corners are less closed, so a closed corner
        // darkens one triangle softly rather than both along a line. The choice follows the blocks
        // alone, so a change of light keeps the triangles and changes only the colors.
        var indices = faces.Indices;
        if (occlusion[0] + occlusion[2] < occlusion[1] + occlusion[3])
        {
            indices.Add(first + 1);
            indices.Add(first + 2);
            indices.Add(first + 3);
            indices.Add(first + 1);
            indices.Add(first + 3);
            indices.Add(first);
        }
        else
        {
            indices.Add(first);
            indices.Add(first + 1);
            indices.Add(first + 2);
            indices.Add(first);
            indices.Add(first + 2);
            indices.Add(first + 3);
        }
    }

    // The pieces of a block smaller than its cell that are not emissive, each a box of six faces,
    // its top's corners moved by its lean, or a flat wall. A face on the cell's side against an
    // opaque block is left out, as a torch's foot on the ground is. Every corner takes the light of
    // the block's own cell, since the pieces stand inside it, and none is shaded by the blocks
    // around. A plant's pieces and every flat wall, as a lantern's handle is, go with the faces cut
    // out where a pack's texture is clear, and a piece with parts of its texture shows them.
    private void AddShape(BlockShape shape, int at, Vector3 block)
    {
        int sky = _light[at] >> 4, lit = _light[at] & 15;
        var kept = LightLevels ? MathF.Pow(Lit(Math.Max(sky, lit)), 1 / 2.2f) : 1;
        var place = (int)block.Z * Section.Size + (int)block.X;
        var offset = Offset(shape.Offset, _origin.X + (int)block.X, _origin.Z + (int)block.Z);
        foreach (var piece in shape.Pieces)
        {
            if (piece.Glows) continue;
            var faces = shape.Cutout || piece.Plane ? Cutout : FacesOf(piece.Surface, true, BlockKind.Shaped);
            var (tiles, color, albedo) = Atlas.Paint(piece.Surface, _column.GrassTint[place], _column.FoliageTint[place]);
            if (piece.Plane)
            {
                AddWall(faces, piece, block + offset, kept, color, tiles, albedo);
                continue;
            }
            var size = piece.To - piece.From;
            for (int face = 0; face < 6; face++)
            {
                var axis = face / 2;
                var onSide = face % 2 == 0 ? Component(piece.To, axis) >= 1 : Component(piece.From, axis) <= 0;
                if (onSide && (axis == 1 || piece.Lean == Vector3.Zero) && Atlas.Hides((BlockId)_blocks[at + Steps[face]])) continue;

                // A leaning face keeps the normal of the face it leans from, the terrain shader holding
                // six, which a wall torch's stick, leaning by 23 degrees, shows little of.
                var first = (uint)faces.Count;
                for (int i = 0; i < 4; i++)
                {
                    var corner = Corners[face][i];
                    var shown = piece.Parts is { } parts ? WithPlace(tiles, Place(parts, face, corner)) : tiles;
                    faces.Add(block + piece.From + corner * size + corner.Y * piece.Lean, face, i, kept, color, shown, albedo);
                }
                faces.Indices.Add(first);
                faces.Indices.Add(first + 1);
                faces.Indices.Add(first + 2);
                faces.Indices.Add(first);
                faces.Indices.Add(first + 2);
                faces.Indices.Add(first + 3);
            }
        }
    }

    /// <summary>
    /// How far a plant of an offset kind stands off its cell's middle at a place, as Minecraft offsets
    /// it: a nibble of the place's hash each for across x and z, from a quarter of a block one way to
    /// a quarter the other in sixteen steps, and for the kind that sinks a third nibble, down to a fifth.
    /// </summary>
    public static Vector3 Offset(PlantOffset kind, int x, int z)
    {
        if (kind == PlantOffset.None) return Vector3.Zero;
        var hash = PlaceHash.Of(x, z, 0x0ff5);
        float across = ((hash & 15) / 15f - 0.5f) * 0.5f, along = ((hash >> 8 & 15) / 15f - 0.5f) * 0.5f;
        var down = kind == PlantOffset.AcrossAndDown ? ((hash >> 4 & 15) / 15f - 1) * 0.2f : 0;
        return new Vector3(across, down, along);
    }

    // A corner carries its place on its texture in sixteenths where a face carries its overlay's
    // tile, one more than across and seventeen times down, with neither flag, since a face showing a
    // part of its texture has no overlay, so the texture lies on it wherever the face stands.
    private static uint WithPlace(uint tiles, Vector2 place)
    {
        uint u = (uint)Math.Clamp(MathF.Round(place.X), 0, 16), v = (uint)Math.Clamp(MathF.Round(place.Y), 0, 16);
        return tiles & 511 | (1 + u + 17 * v) << 9;
    }

    // A flat wall, from its piece's first corner across to its second, drawn from both sides with no
    // edges, as Minecraft's crossed planes are. Both sides are lit as if they faced up, as Minecraft
    // lights its plants with no shade by the way they face, and show the wall's part of its texture,
    // or for a plant the texture laid along the axis the wall runs most along from the cell's foot,
    // and each side's corners run counterclockwise seen from it.
    private static void AddWall(Faces faces, Piece piece, Vector3 block, float kept, Color color, uint tiles, Color albedo)
    {
        var (a, b) = (piece.From, piece.To);
        var face = MathF.Abs(b.X - a.X) >= MathF.Abs(b.Z - a.Z) ? WallAlongX : WallAlongZ;
        var part = piece.Parts?.Sides;
        // The wall runs from its lower end along the axis its texture is laid across, so its corners
        // 0 to 3, the lower end's foot, the upper end's foot, the upper end's top and the lower end's
        // top, lie at the texture's lower left, lower right, upper right and upper left, a part's ends
        // swapped with the wall's.
        if (face == WallAlongX ? a.X > b.X : a.Z > b.Z)
        {
            (a, b) = (new Vector3(b.X, a.Y, b.Z), new Vector3(a.X, b.Y, a.Z));
            if (part is { } ends) part = new Vector4(ends.Z, ends.Y, ends.X, ends.W);
        }
        Span<Vector3> quad = [new(a.X, a.Y, a.Z), new(b.X, a.Y, b.Z), new(b.X, b.Y, b.Z), new(a.X, b.Y, a.Z)];
        var across = new Vector3(b.Z - a.Z, 0, a.X - b.X);
        foreach (var side in (ReadOnlySpan<float>)[1, -1])
        {
            var normal = across * side;
            var turned = Vector3.Dot(Vector3.Cross(quad[1] - quad[0], quad[2] - quad[0]), normal) < 0;
            var first = (uint)faces.Count;
            for (int i = 0; i < 4; i++)
            {
                var corner = turned ? 3 - i : i;
                var at = quad[corner];
                var place = part is { } shown ? new Vector2(corner is 0 or 3 ? shown.X : shown.Z, corner < 2 ? shown.W : shown.Y)
                    : new Vector2(face == WallAlongX ? at.X : at.Z, 1 - at.Y) * 16;
                faces.Add(block + at, face, corner, kept, color, WithPlace(tiles, place), albedo);
            }
            faces.Indices.Add(first);
            faces.Indices.Add(first + 1);
            faces.Indices.Add(first + 2);
            faces.Indices.Add(first);
            faces.Indices.Add(first + 2);
            faces.Indices.Add(first + 3);
        }
    }

    private static float Component(Vector3 v, int axis) => axis == 0 ? v.X : axis == 1 ? v.Y : v.Z;

    private void Gather(int cell, ref int sky, ref int lit, ref int open)
    {
        sky += _light[cell] >> 4;
        lit += _light[cell] & 15;
        open++;
    }

    // The section and the cells of the 26 around it that touch it into the padded copy. Below the
    // world counts as solid and dark, since its faces are never seen, and above it as open sky.
    private void Fill()
    {
        for (int py = -1; py <= Section.Size; py++)
        {
            var sy = py < 0 ? 0 : py == Section.Size ? 2 : 1;
            for (int pz = -1; pz <= Section.Size; pz++)
            {
                var sz = pz < 0 ? 0 : pz == Section.Size ? 2 : 1;
                for (int px = -1; px <= Section.Size; px++)
                {
                    var sx = px < 0 ? 0 : px == Section.Size ? 2 : 1;
                    var at = Pad(px, py, pz);
                    if (_around[sy * 9 + sz * 3 + sx] is { } section)
                    {
                        var index = Section.Index(px & Section.Mask, py & Section.Mask, pz & Section.Mask);
                        _blocks[at] = section.Blocks[index];
                        _light[at] = section.Light[index];
                    }
                    else if (sy == 0)
                    {
                        _blocks[at] = (ushort)BlockId.Bedrock;
                        _light[at] = 0;
                    }
                    else
                    {
                        _blocks[at] = 0;
                        _light[at] = Lighting.Max << 4;
                    }
                }
            }
        }
    }
}
