namespace VoxelSandbox;

/// <summary>
/// An abandoned mineshaft deep under the ground, a room with a floor of dirt from whose walls
/// corridors run out, three wide and three high, turning, crossing and stepping down, as the
/// source's do, held up every five blocks by posts of the biome's logs under a beam of its planks,
/// with rails along some, cobwebs under the beams, a lantern hanging from some beams and a chest of
/// the source's mineshaft loot in a niche of some corridors.
/// </summary>
/// <remarks>
/// <para>
/// Its pieces are laid out from its start's hash alone, each corridor two to four lengths of five
/// blocks and the next piece at its end a corridor four times in five, a crossing or a stair down
/// otherwise, eight pieces deep at most from the room and within 80 blocks of the room's middle, as
/// the source's are, and a piece that would cross one laid before it is left out. So every column
/// it crosses lays out the same pieces and builds its own part of them.
/// </para>
/// <para>
/// The source holds its corridors up with fences under a plank, and the modpack's mineshafts with
/// posts of logs, its wood the biome's, acacia in the savanna, spruce in the taiga, birch in the birch
/// forest, jungle wood in the jungle, cherry in the cherry grove and dark oak in the dark forest and
/// the badlands, where the source's own is dark oak, and hang lanterns from some of their beams. A
/// corridor's floor over a cave is laid of planks, and nothing is cut within four blocks of the
/// ground, so no corridor opens to the sky.
/// </para>
/// </remarks>
public sealed class Mineshaft : Structure
{
    public override string Name => "mineshaft";

    // How far from its room's middle its pieces reach.
    private const int Reach = 80;

    public override int Size => 2 * Reach;

    // One to each square of sixteen chunks, about the source's one chunk in 250, anywhere in it.
    public override int CellSize => 16 * Section.Size;

    protected override int Chance => 100;

    protected override int Salt => 0x3ab5;

    // How many pieces deep from the room it goes, and how many pieces it has at most.
    private const int Deepest = 8, Most = 160;

    protected override bool Suits(Biome biome, int ground, uint hash, out int y)
    {
        y = -40 + (int)((hash >> 22) % 70);
        return y < ground - 20;
    }

    private enum Kind
    {
        Room,
        Corridor,
        Crossing,
        Stairs,
    }

    /// <summary>
    /// A piece, the box it fills, its entrance's floor and the way it runs from it, and for a corridor
    /// whether it has rails and where along it a chest stands, or -1, and a hash for its touches.
    /// </summary>
    private readonly record struct Piece(Kind Kind, int X0, int Y0, int Z0, int X1, int Y1, int Z1,
        int X = 0, int Y = 0, int Z = 0, int Dx = 0, int Dz = 0, int Length = 0, bool Rails = false, int Chest = -1, int Hash = 0)
    {
        public bool Crosses(Piece other) =>
            X0 <= other.X1 && X1 >= other.X0 && Y0 <= other.Y1 && Y1 >= other.Y0 && Z0 <= other.Z1 && Z1 >= other.Z0;
    }

    // A run of numbers from a start's hash, each the hash of the one before.
    private struct Rolls(uint state)
    {
        private uint _state = state;

        public int Next(int below) => (int)((_state = PlaceHash.Of((int)_state, 0x6d1, 0x5bd1)) % (uint)below);
    }

    // An entrance still to be built on, its floor, its way and how deep it lies from the room.
    private readonly record struct Opening(int X, int Y, int Z, int Dx, int Dz, int Deep);

    // The pieces of the mineshaft at a start, laid out from its hash.
    private List<Piece> Layout(StructureStart start)
    {
        var rolls = new Rolls(start.Hash);
        int cx = start.X + Reach, cz = start.Z + Reach, y = start.Y;
        int width = 7 + rolls.Next(6), depth = 7 + rolls.Next(6), height = 4 + rolls.Next(6);
        int x0 = cx - width / 2, z0 = cz - depth / 2;
        var room = new Piece(Kind.Room, x0, y, z0, x0 + width - 1, y + height, z0 + depth - 1);
        var pieces = new List<Piece> { room };
        var open = new Queue<Opening>();
        // Ways out of each wall, at its floor or a little above, every four to seven blocks along it.
        foreach (var (dx, dz) in Ways)
        {
            var along = dx != 0 ? depth : width;
            for (int t = 1 + rolls.Next(3); t < along - 1; t += 4 + rolls.Next(4))
            {
                int ex = dx > 0 ? room.X1 + 1 : dx < 0 ? room.X0 - 1 : room.X0 + t;
                int ez = dz > 0 ? room.Z1 + 1 : dz < 0 ? room.Z0 - 1 : room.Z0 + t;
                open.Enqueue(new Opening(ex, y + 1 + rolls.Next(Math.Max(1, height - 3)), ez, dx, dz, 1));
            }
        }
        while (open.Count > 0 && pieces.Count < Most)
        {
            var at = open.Dequeue();
            if (at.Deep > Deepest) continue;
            var roll = rolls.Next(100);
            if (roll < 80) Corridor(pieces, open, ref rolls, start, at);
            else if (roll < 90) Crossing(pieces, open, ref rolls, start, at);
            else Stairs(pieces, open, start, at);
        }
        return pieces;
    }

    private static readonly (int X, int Z)[] Ways = [(0, -1), (1, 0), (0, 1), (-1, 0)];

    // A piece's box running from an entrance along its way for a length, a half width to each side
    // and a height over its floor, from a depth under it.
    private static Piece Along(Kind kind, Opening at, int length, int height, int half = 1, int under = 0)
    {
        int ex = at.X + at.Dx * (length - 1), ez = at.Z + at.Dz * (length - 1);
        int ax = at.Dz != 0 ? half : 0, az = at.Dx != 0 ? half : 0;
        return new Piece(kind, Math.Min(at.X, ex) - ax, at.Y - under, Math.Min(at.Z, ez) - az, Math.Max(at.X, ex) + ax, at.Y + height, Math.Max(at.Z, ez) + az,
            at.X, at.Y, at.Z, at.Dx, at.Dz, length);
    }

    // Whether a piece lies within the mineshaft's square and over the world's floor, and crosses
    // none laid before it.
    private bool Fits(List<Piece> pieces, Piece piece, StructureStart start) =>
        piece.X0 >= start.X && piece.X1 < start.X + Size && piece.Z0 >= start.Z && piece.Z1 < start.Z + Size
        && piece.Y0 > ChunkColumn.Bottom + 4 && !pieces.Any(p => p.Crosses(piece));

    // The entrance a way along a piece and across it, turned to run across it toward that side.
    private static Opening Side(Opening at, int along, int side, int deep) =>
        new(at.X + at.Dx * along - at.Dz * side, at.Y, at.Z + at.Dz * along + at.Dx * side, -at.Dz * Math.Sign(side), at.Dx * Math.Sign(side), deep);

    private void Corridor(List<Piece> pieces, Queue<Opening> open, ref Rolls rolls, StructureStart start, Opening at)
    {
        for (int lengths = 2 + rolls.Next(3); lengths >= 1; lengths--)
        {
            var length = lengths * 5;
            var piece = Along(Kind.Corridor, at, length, 2);
            if (!Fits(pieces, piece, start)) continue;
            // A chest stands in one corridor in eight, at the first or last block of one of its
            // lengths, clear of its supports, about as many to a mineshaft as the source's carts of
            // chests, one place in a hundred.
            var chest = rolls.Next(8) == 0 ? rolls.Next(lengths) * 5 + (rolls.Next(2) == 0 ? 0 : 4) : -1;
            pieces.Add(piece with { Rails = rolls.Next(3) == 0, Chest = chest, Hash = rolls.Next(int.MaxValue) });
            // On straight from its end, or turned to its right or its left at its last block but one.
            var next = rolls.Next(4) switch
            {
                0 or 1 => at with { X = at.X + at.Dx * length, Z = at.Z + at.Dz * length, Deep = at.Deep + 1 },
                2 => Side(at, length - 2, 2, at.Deep + 1),
                _ => Side(at, length - 2, -2, at.Deep + 1),
            };
            open.Enqueue(next);
            // A branch off a long corridor's side now and then.
            if (lengths >= 3 && rolls.Next(3) == 0)
                open.Enqueue(Side(at, 5 + rolls.Next(length - 8), rolls.Next(2) == 0 ? 2 : -2, at.Deep + 1));
            return;
        }
    }

    // A crossing five blocks a side, three high or seven, the taller held up by logs at its corners,
    // with ways on from its other three sides.
    private void Crossing(List<Piece> pieces, Queue<Opening> open, ref Rolls rolls, StructureStart start, Opening at)
    {
        var piece = Along(Kind.Crossing, at, 5, rolls.Next(4) == 0 ? 6 : 2, half: 2);
        if (!Fits(pieces, piece, start)) return;
        pieces.Add(piece);
        open.Enqueue(at with { X = at.X + at.Dx * 5, Z = at.Z + at.Dz * 5, Deep = at.Deep + 1 });
        open.Enqueue(Side(at, 2, 3, at.Deep + 1));
        open.Enqueue(Side(at, 2, -3, at.Deep + 1));
    }

    // A stair down five blocks over nine, two flat at each end, and the way on at its foot.
    private void Stairs(List<Piece> pieces, Queue<Opening> open, StructureStart start, Opening at)
    {
        var piece = Along(Kind.Stairs, at, 9, 2, under: 5);
        if (!Fits(pieces, piece, start)) return;
        pieces.Add(piece);
        open.Enqueue(at with { X = at.X + at.Dx * 9, Y = at.Y - 5, Z = at.Z + at.Dz * 9, Deep = at.Deep + 1 });
    }

    // The logs and planks of a mineshaft by the biome over its room.
    private static (BlockId Log, BlockId Planks) Wood(Biome biome) =>
        biome == Biomes.Savanna ? (BlockId.AcaciaLog, BlockId.AcaciaPlanks)
        : biome == Biomes.Taiga || biome == Biomes.SnowyTaiga ? (BlockId.SpruceLog, BlockId.SprucePlanks)
        : biome == Biomes.BirchForest ? (BlockId.BirchLog, BlockId.BirchPlanks)
        : biome == Biomes.Jungle ? (BlockId.JungleLog, BlockId.JunglePlanks)
        : biome == Biomes.CherryGrove ? (BlockId.CherryLog, BlockId.CherryPlanks)
        : biome == Biomes.DarkForest || biome == Biomes.Badlands ? (BlockId.DarkOakLog, BlockId.DarkOakPlanks)
        : (BlockId.OakLog, BlockId.OakPlanks);

    public override void Build(ColumnClip clip, StructureStart start, Overworld land)
    {
        var pieces = Layout(start);
        if (!pieces.Any(p => clip.Touches(p.X0 - 2, p.Z0 - 2, p.X1 + 2, p.Z1 + 2))) return;
        var (log, planks) = Wood(land.At(start.X + Reach, start.Z + Reach).Biome);
        // The ground over each place of the column, read once it is asked for.
        var grounds = new int[Section.Size * Section.Size];
        Array.Fill(grounds, int.MinValue);
        int Ground(int x, int z)
        {
            ref var ground = ref grounds[(z - clip.MinZ) * Section.Size + x - clip.MinX];
            if (ground == int.MinValue) ground = land.At(x, z).Height;
            return ground;
        }
        // Whether a place lies in the column and four blocks or more under its ground.
        bool Deep(int x, int y, int z) => clip.Inside(x, y, z) && y < Ground(x, z) - 4;
        void Put(int x, int y, int z, BlockId block)
        {
            if (Deep(x, y, z)) clip.Set(x, y, z, block);
        }

        foreach (var piece in pieces)
        {
            if (!clip.Touches(piece.X0 - 2, piece.Z0 - 2, piece.X1 + 2, piece.Z1 + 2)) continue;
            switch (piece.Kind)
            {
                case Kind.Room:
                    for (int z = piece.Z0; z <= piece.Z1; z++)
                        for (int x = piece.X0; x <= piece.X1; x++)
                        {
                            if (!Deep(x, piece.Y1, z)) continue;
                            clip.Set(x, piece.Y0, z, BlockId.Dirt);
                            clip.Fill(x, piece.Y0 + 1, z, x, piece.Y1, z, BlockId.Air);
                        }
                    break;
                case Kind.Crossing:
                    for (int z = piece.Z0; z <= piece.Z1; z++)
                        for (int x = piece.X0; x <= piece.X1; x++)
                        {
                            if (!Deep(x, piece.Y1, z)) continue;
                            Floor(clip, x, piece.Y0 - 1, z, planks);
                            clip.Fill(x, piece.Y0, z, x, piece.Y1, z, BlockId.Air);
                            var corner = (x == piece.X0 || x == piece.X1) && (z == piece.Z0 || z == piece.Z1);
                            if (corner && piece.Y1 - piece.Y0 > 2) clip.Fill(x, piece.Y0, z, x, piece.Y1, z, log);
                        }
                    break;
                case Kind.Stairs:
                    for (int t = 0; t < piece.Length; t++)
                        for (int w = -1; w <= 1; w++)
                        {
                            int x = piece.X + piece.Dx * t - piece.Dz * w, z = piece.Z + piece.Dz * t + piece.Dx * w;
                            var floor = piece.Y - Math.Clamp(t - 1, 0, 5);
                            if (!Deep(x, floor + 2, z)) continue;
                            Floor(clip, x, floor - 1, z, planks);
                            clip.Fill(x, floor, z, x, floor + 2, z, BlockId.Air);
                        }
                    break;
                default:
                    Corridor(clip, piece, land.Seed, log, planks, Deep, Put);
                    break;
            }
        }
    }

    // A block of floor of planks where a cave or water left none under a corridor.
    private static void Floor(ColumnClip clip, int x, int y, int z, BlockId planks)
    {
        if (clip.Get(x, y, z) is BlockId.Air or BlockId.Water or BlockId.Lava) clip.Set(x, y, z, planks);
    }

    // A corridor cut along its way, a support every five blocks from its third, its rails, its
    // cobwebs, its lanterns and its chest.
    private static void Corridor(ColumnClip clip, Piece piece, int seed, BlockId log, BlockId planks, Func<int, int, int, bool> deep, Action<int, int, int, BlockId> put)
    {
        int y = piece.Y;
        var rail = Blocks.Variant(BlockId.Rail, piece.Dx != 0 ? Facing.East : Facing.North);
        for (int t = 0; t < piece.Length; t++)
        {
            var support = t % 5 == 2;
            for (int w = -1; w <= 1; w++)
            {
                int x = piece.X + piece.Dx * t - piece.Dz * w, z = piece.Z + piece.Dz * t + piece.Dx * w;
                if (!deep(x, y + 2, z)) continue;
                var hash = PlaceHash.Of(x * 7 + y, z, piece.Hash);
                Floor(clip, x, y - 1, z, planks);
                clip.Fill(x, y, z, x, y + 2, z, BlockId.Air);
                if (support)
                {
                    if (w != 0) clip.Fill(x, y, z, x, y + 1, z, log);
                    clip.Set(x, y + 2, z, planks);
                    // A lantern hangs under the middle of one beam in ten.
                    if (w == 0 && hash % 10 == 0) clip.Set(x, y + 1, z, BlockId.HangingLantern);
                }
                else if (w != 0 && hash % 14 == 0) clip.Set(x, y + 2, z, BlockId.Cobweb);
                if (piece.Rails && w == 0 && !support && hash / 14 % 7 != 0) clip.Set(x, y, z, rail);
            }
        }
        if (piece.Chest < 0) return;
        // The chest stands in a niche cut a block into one wall, its latch toward the corridor.
        var side = (piece.Hash & 1) == 0 ? 2 : -2;
        int cx = piece.X + piece.Dx * piece.Chest - piece.Dz * side, cz = piece.Z + piece.Dz * piece.Chest + piece.Dx * side;
        if (!deep(cx, y + 1, cz)) return;
        var (fx, fz) = (piece.Dz * Math.Sign(side), -piece.Dx * Math.Sign(side));
        var facing = (fx, fz) switch { (1, 0) => Facing.East, (-1, 0) => Facing.West, (0, 1) => Facing.South, _ => Facing.North };
        put(cx, y + 1, cz, BlockId.Air);
        clip.Chest(cx, y, cz, facing, ChestLoot.Mineshaft.Name, seed);
    }
}
