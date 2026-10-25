namespace VoxelSandbox;

/// <summary>
/// The ruin of a building on the sea's floor, of stone bricks, mossy and cracked, where the water is
/// cold and of sandstone where it is warm, its walls broken more the higher they stood, a chest of
/// the source's small ocean ruin's loot inside, and one in three a big ruin of three rooms.
/// </summary>
/// <remarks>
/// The source's ocean ruins stand one to each square of 20 chunks, at a chunk among its first 12,
/// where the ocean holds them, here where the sea's floor lies six blocks or more under its top. Its
/// walls keep each block by a share that falls with the height, nine in ten at the floor and three in
/// ten at the top, as the source's ruins keep a share of their template's blocks, and the water
/// stays in its rooms.
/// </remarks>
public sealed class OceanRuin : Structure
{
    public override string Name => "ruin";
    public override int Size => 16;
    public override int CellSize => 20 * Section.Size;
    protected override int Chance => 100;
    protected override int Salt => 0x0c3a;

    protected override (int X, int Z) Within(uint hash) => Spread(hash, 12);

    protected override bool Suits(Biome biome, int ground, uint hash, out int y)
    {
        y = ground;
        return ground <= Overworld.SeaLevel - 6;
    }

    public override void Build(ColumnClip clip, StructureStart start, Overworld land)
    {
        var warm = land.At(start.X + Size / 2, start.Z + Size / 2).Temperature > 0.62f;
        var big = start.Hash / 7 % 3 == 0;
        // The frame stands at the square's middle, so its rooms turned any way lie within it.
        var frame = new StructureFrame(clip, start.X + Size / 2, start.Y, start.Z + Size / 2, (int)(start.Hash >> 8 & 3));
        Room(frame, -6, -6, warm, start.Hash, land.Seed, chest: true);
        if (!big) return;
        Room(frame with { Y = start.Y - 1 }, 0, -6, warm, start.Hash ^ 0x51, land.Seed, chest: false);
        Room(frame with { Y = start.Y + 1 }, -6, 0, warm, start.Hash ^ 0xa3, land.Seed, chest: true);
    }

    // A room seven blocks a side at a corner of the frame, its floor and its broken walls, a doorway
    // in its front, and a chest in its back corner where it holds one.
    private static void Room(StructureFrame f, int ox, int oz, bool warm, uint hash, int seed, bool chest)
    {
        for (int z = 0; z < 7; z++)
            for (int x = 0; x < 7; x++)
            {
                var wall = x == 0 || x == 6 || z == 0 || z == 6;
                // The floor down to the sea's floor, so a room on a slope does not stand on the water.
                for (int d = 0; d <= 3 && (d == 0 || f.Get(ox + x, -d, oz + z) is BlockId.Water or BlockId.Air); d++)
                    f.Set(ox + x, -d, oz + z, Stone(warm, PlaceHash.Of(ox + x, oz + z, (int)hash + d)));
                if (!wall || z == 0 && x == 3) continue;
                for (int y = 1; y <= 4; y++)
                {
                    var roll = PlaceHash.Of(ox + x + 31 * y, oz + z, (int)hash) % 100;
                    if (roll < 100 - 20 * y + 10) f.Set(ox + x, y, oz + z, Stone(warm, roll));
                }
            }
        if (chest) f.Chest(ox + 5, 1, oz + 5, Facing.North, ChestLoot.OceanRuin.Name, seed);
    }

    // A block of a ruin's wall, a cold ruin's stone bricks one in two, mossy one in four and cracked
    // one in four, a warm one's sandstone.
    private static BlockId Stone(bool warm, uint roll) => warm ? BlockId.Sandstone
        : ((roll >> 4) % 4) switch { 0 => BlockId.MossyStoneBricks, 1 => BlockId.CrackedStoneBricks, _ => BlockId.StoneBricks };
}

/// <summary>
/// A ship sunk to the sea's floor, its hull of planks narrowing to its bow, its deck, its mast of
/// logs and its cabin at its stern, a chest of supplies in its hold and one of treasure in its cabin,
/// one in three broken off behind its middle, as the source's shipwrecks lie.
/// </summary>
/// <remarks>
/// The source's shipwrecks stand one to each square of 24 chunks, at a chunk among its first 20,
/// where the ocean holds them, here where the sea's floor lies eight blocks or more under its top. The
/// hull is twenty-two blocks long and seven wide, its keel on the floor, and the water fills it, as it
/// does the air over the sea's top where its mast stands out.
/// </remarks>
public sealed class Shipwreck : Structure
{
    public override string Name => "shipwreck";
    public override int Size => 24;
    public override int CellSize => 24 * Section.Size;
    protected override int Chance => 100;
    protected override int Salt => 0x5a1f;

    private const int Length = 22;

    protected override (int X, int Z) Within(uint hash) => Spread(hash, 20);

    protected override bool Suits(Biome biome, int ground, uint hash, out int y)
    {
        y = ground;
        return ground <= Overworld.SeaLevel - 8;
    }

    // How far the hull reaches to each side at a place along it, narrowing to its bow.
    private static int Half(int z) => z switch { 0 => 1, 1 or 2 => 2, _ => 3 };

    public override void Build(ColumnClip clip, StructureStart start, Overworld land)
    {
        var turn = (int)(start.Hash >> 8 & 3);
        // The frame stands at the middle of the square, so the hull turned any way lies within it.
        var f = new StructureFrame(clip, start.X + Size / 2, start.Y, start.Z + Size / 2, turn);
        (BlockId Planks, BlockId Log) wood = ((start.Hash >> 12) % 3) switch
        {
            0 => (BlockId.OakPlanks, BlockId.OakLog),
            1 => (BlockId.SprucePlanks, BlockId.SpruceLog),
            _ => (BlockId.DarkOakPlanks, BlockId.DarkOakLog),
        };
        var broken = start.Hash / 5 % 3 == 0;
        var stern = broken ? 12 + (int)(start.Hash >> 16) % 5 : Length;
        BlockId Wet(int y) => start.Y + y <= Overworld.SeaLevel ? BlockId.Water : BlockId.Air;
        for (int z = 0; z < stern; z++)
        {
            var half = Half(z);
            var lz = z - Length / 2;
            for (int x = -half; x <= half; x++)
            {
                // The hull's bottom rises toward its sides, the keel at the floor.
                var bottom = Math.Max(0, Math.Abs(x) - 1);
                for (int y = bottom; y <= 4; y++)
                {
                    var hull = y == bottom || Math.Abs(x) == half || z == 0 || z == Length - 1;
                    f.Set(x, y, lz, hull ? wood.Planks : y == 4 && z is > 1 and < Length - 2 && !(x == 0 && z is 6 or 7) ? wood.Planks : Wet(y));
                }
                for (int y = 5; y <= 9; y++) f.Set(x, y, lz, Wet(y));
            }
        }
        // The mast, which a broken stern's ship keeps, standing from the keel well over the deck.
        for (int y = 1; y <= 12; y++) f.Set(0, y, 9 - Length / 2, wood.Log);
        f.Chest(0, 1, 4 - Length / 2, Facing.South, ChestLoot.ShipwreckSupply.Name, land.Seed);
        if (broken) return;
        // The cabin at the stern, its door toward the bow, the chest of treasure inside.
        for (int z = 16; z <= 20; z++)
            for (int x = -2; x <= 2; x++)
                for (int y = 5; y <= 8; y++)
                {
                    var edge = Math.Abs(x) == 2 || z == 16 || z == 20;
                    f.Set(x, y, z - Length / 2, y == 8 || edge && !(z == 16 && x == 0 && y <= 6) ? wood.Planks : Wet(y));
                }
        f.Chest(1, 5, 19 - Length / 2, Facing.North, ChestLoot.ShipwreckTreasure.Name, land.Seed);
    }
}
