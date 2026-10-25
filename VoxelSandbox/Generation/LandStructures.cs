namespace VoxelSandbox;

/// <summary>
/// A temple of cobblestone overgrown with moss in the jungle, three floors stepping in to its top
/// under a roof, a stair up from its hall and one down to a vault under it holding two chests of the
/// source's jungle temple loot.
/// </summary>
/// <remarks>
/// The source's jungle temples stand one to each square of 32 chunks, at a chunk among its first 24,
/// where the jungle holds them, twelve blocks by fifteen, as this one is. Two blocks in five of its
/// walls are mossy, picked by their place.
/// </remarks>
public sealed class JungleTemple : Structure
{
    public override string Name => "temple";
    public override int Size => 15;
    public override int CellSize => 32 * Section.Size;
    protected override int Chance => 100;
    protected override int Salt => 0x7e3b;

    public override bool ClearsTrees => true;

    protected override (int X, int Z) Within(uint hash) => Spread(hash, 24);

    protected override bool Suits(Biome biome, int ground, uint hash, out int y)
    {
        y = ground;
        return ground >= Overworld.SeaLevel && biome == Biomes.Jungle;
    }

    public override void Build(ColumnClip clip, StructureStart start, Overworld land)
    {
        // Its front faces north or south, so its fifteen blocks of depth lie along z.
        var turn = (start.Hash >> 8 & 1) == 0 ? 0 : 2;
        var f = new StructureFrame(clip, turn == 0 ? start.X + 1 : start.X + 12, start.Y, turn == 0 ? start.Z : start.Z + 14, turn);
        BlockId Stone(int x, int y, int z) => PlaceHash.Of(x * 17 + y, z, (int)start.Hash) % 5 < 2 ? BlockId.MossyCobblestone : BlockId.Cobblestone;
        void Ring(int inset, int y0, int y1)
        {
            for (int y = y0; y <= y1; y++)
                for (int z = inset; z <= 14 - inset; z++)
                    for (int x = inset; x <= 11 - inset; x++)
                        if (x == inset || x == 11 - inset || z == inset || z == 14 - inset) f.Set(x, y, z, Stone(x, y, z));
        }
        void Floor(int inset, int y)
        {
            for (int z = inset; z <= 14 - inset; z++)
                for (int x = inset; x <= 11 - inset; x++)
                    f.Set(x, y, z, Stone(x, y, z));
        }

        f.Ground(0, 11, 15, BlockId.Cobblestone, 13);
        // The vault under the hall, its floor five under the ground's.
        for (int y = -6; y <= -1; y++)
            for (int z = 0; z <= 14; z++)
                for (int x = 0; x <= 11; x++)
                    f.Set(x, y, z, Stone(x, y, z));
        f.Fill(2, -5, 3, 9, -2, 12, BlockId.Air);
        Floor(0, 0);
        Ring(0, 1, 3);
        f.Fill(1, 1, 1, 10, 3, 13, BlockId.Air);
        Floor(1, 4);
        Ring(1, 5, 7);
        f.Fill(2, 5, 2, 9, 7, 12, BlockId.Air);
        Floor(2, 8);
        Ring(3, 9, 10);
        f.Fill(4, 9, 4, 7, 10, 10, BlockId.Air);
        Floor(3, 11);
        // The door, three wide, and the windows of the floors over it.
        f.Fill(5, 1, 0, 6, 2, 0, BlockId.Air);
        foreach (var z in (ReadOnlySpan<int>)[4, 7, 10])
        {
            f.Fill(1, 6, z, 1, 6, z, BlockId.Air);
            f.Fill(10, 6, z, 10, 6, z, BlockId.Air);
        }
        f.Fill(5, 6, 1, 6, 6, 1, BlockId.Air);
        // A stair up the hall's left side to the floor over it, through a gap in that floor.
        for (int step = 0; step < 4; step++)
        {
            f.Set(2, 1 + step, 10 - step, BlockId.CobblestoneStairs);
            f.Fill(2, 2 + step, 10 - step, 2, 4, 10 - step, BlockId.Air);
        }
        // A stair down the hall's right side into the vault.
        for (int step = 0; step < 5; step++)
        {
            f.Set(9, -1 - step, 3 + step, BlockId.CobblestoneStairs);
            f.Fill(9, -step, 3 + step, 9, 0, 3 + step, BlockId.Air);
        }
        f.Chest(3, -5, 11, Facing.East, ChestLoot.JungleTemple.Name, land.Seed);
        f.Chest(3, -5, 4, Facing.East, ChestLoot.JungleTemple.Name, land.Seed);
    }
}

/// <summary>
/// A witch's hut in the swamp, a room of spruce planks on posts of oak over the ground or the
/// water, with a porch at its door, windows of fences, a roof of spruce stairs, a crafting table and,
/// as the modpack's huts have, a chest of what a witch keeps.
/// </summary>
/// <remarks>
/// The source's swamp huts stand one to each square of 32 chunks, at a chunk among its first 24,
/// where the swamp holds them, seven blocks by nine with the floor three over the ground, as this one
/// is, over the water where it stands in a pool.
/// </remarks>
public sealed class WitchHut : Structure
{
    public override string Name => "hut";
    public override int Size => 9;
    public override int CellSize => 32 * Section.Size;
    protected override int Chance => 100;
    protected override int Salt => 0x5c4d;

    public override bool ClearsTrees => true;

    protected override (int X, int Z) Within(uint hash) => Spread(hash, 24);

    protected override bool Suits(Biome biome, int ground, uint hash, out int y)
    {
        y = Math.Max(ground, Overworld.SeaLevel) + 3;
        return biome == Biomes.Swamp;
    }

    public override void Build(ColumnClip clip, StructureStart start, Overworld land)
    {
        var turn = (int)(start.Hash >> 8 & 3);
        // The frame at the hut's middle, its door's side from three in front of it to five behind.
        var f = new StructureFrame(clip, start.X + 4, start.Y, start.Z + 4, turn);
        f.Fill(-3, 1, -3, 3, 6, 4, BlockId.Air);
        f.Fill(-2, 0, -3, 2, 0, 3, BlockId.SprucePlanks);
        // Posts at the room's corners down to the ground through any water.
        foreach (var (x, z) in (ReadOnlySpan<(int, int)>)[(-2, -2), (2, -2), (-2, 3), (2, 3)])
            for (int y = 3; y >= -8; y--)
            {
                if (y < 0 && f.Get(x, y, z) is not (BlockId.Air or BlockId.Water) && !BlockGroups.Plants.Contains(f.Get(x, y, z))) break;
                f.Set(x, y, z, BlockId.OakLog);
            }
        // The room's walls, its door in front and a window of fences in each side.
        for (int y = 1; y <= 3; y++)
            for (int z = -2; z <= 3; z++)
                for (int x = -2; x <= 2; x++)
                    if ((Math.Abs(x) == 2 || z == -2 || z == 3) && !(x is -2 or 2 && z is -2 or 3))
                        f.Set(x, y, z, BlockId.SprucePlanks);
        f.Fill(0, 1, -2, 0, 2, -2, BlockId.Air);
        f.Set(-2, 2, 0, BlockId.OakFence);
        f.Set(2, 2, 0, BlockId.OakFence);
        // The porch's rail at its front corners.
        f.Set(-2, 1, -3, BlockId.OakFence);
        f.Set(2, 1, -3, BlockId.OakFence);
        // The roof, stairs climbing from each side to a ridge of planks along the middle.
        f.Fill(-3, 4, -3, -3, 4, 4, BlockId.SpruceStairsEast);
        f.Fill(3, 4, -3, 3, 4, 4, BlockId.SpruceStairsWest);
        f.Fill(-2, 4, -3, 2, 4, 4, BlockId.SprucePlanks);
        f.Fill(-1, 5, -3, -1, 5, 4, BlockId.SpruceStairsEast);
        f.Fill(1, 5, -3, 1, 5, 4, BlockId.SpruceStairsWest);
        f.Fill(0, 5, -3, 0, 5, 4, BlockId.SprucePlanks);
        f.Set(-1, 1, 2, BlockId.CraftingTable);
        f.Chest(1, 1, 2, Facing.North, ChestLoot.WitchHut.Name, land.Seed);
    }
}

/// <summary>
/// A pillagers' outpost on open land, a tower of dark oak, four floors of birch planks up to a
/// platform with a rail of fences under a roof, stairs of cobblestone up each floor, windows of
/// fences, and a chest of the source's outpost loot on the platform.
/// </summary>
/// <remarks>
/// The source's outposts stand one to each square of 32 chunks, at a chunk among its first 24, in one
/// square in five, on the plains, the desert, the savanna, the taiga and the snowy plains, and never
/// within ten chunks of a village. The tower is the modpack's watchtower's look, logs of dark oak at
/// its corners, walls of its planks and floors of birch over a base of cobblestone.
/// </remarks>
public sealed class PillagerOutpost : Structure
{
    public override string Name => "outpost";
    public override int Size => 13;
    public override int CellSize => 32 * Section.Size;
    protected override int Chance => 20;
    protected override int Salt => 0x0b7f;

    public override bool ClearsTrees => true;

    protected override (int X, int Z) Within(uint hash) => Spread(hash, 24);

    protected override bool Suits(Biome biome, int ground, uint hash, out int y)
    {
        y = ground;
        return ground > Overworld.SeaLevel && (biome == Biomes.Plains || biome == Biomes.Desert || biome == Biomes.Savanna || biome == Biomes.Taiga || biome == Biomes.SnowyPlains);
    }

    protected override bool Fits(Overworld land, int x, int z) =>
        !Structures.Village.StartsIn(x - 10 * Section.Size, z - 10 * Section.Size, x + Size + 10 * Section.Size, z + Size + 10 * Section.Size, land).Any();

    public override void Build(ColumnClip clip, StructureStart start, Overworld land)
    {
        var turn = (int)(start.Hash >> 8 & 3);
        // The tower's nine blocks a side about the square's middle, its door in front.
        var f = new StructureFrame(clip, start.X + 6, start.Y, start.Z + 6, turn);
        for (int z = -4; z <= 4; z++)
            for (int x = -4; x <= 4; x++)
                for (int d = 1; d <= 6 && f.Get(x, -d, z) is BlockId.Air or BlockId.Water || d == 1; d++)
                    f.Set(x, -d, z, BlockId.Cobblestone);
        f.Fill(-6, 1, -6, 6, 22, 6, BlockId.Air);
        f.Fill(-4, 0, -4, 4, 0, 4, BlockId.Cobblestone);
        for (int storey = 0; storey < 3; storey++)
        {
            var floor = storey * 5;
            for (int y = floor + 1; y <= floor + 4; y++)
                for (int z = -4; z <= 4; z++)
                    for (int x = -4; x <= 4; x++)
                    {
                        if (Math.Abs(x) != 4 && Math.Abs(z) != 4) continue;
                        var corner = Math.Abs(x) == 4 && Math.Abs(z) == 4;
                        var window = !corner && (x == 0 || z == 0) && y - floor is 2 or 3;
                        f.Set(x, y, z, corner ? BlockId.DarkOakLog : window ? BlockId.OakFence : BlockId.DarkOakPlanks);
                    }
            // The floor over it of birch, a gap over the stair up the left wall.
            f.Fill(-3, floor + 5, -3, 3, floor + 5, 3, BlockId.BirchPlanks);
            for (int step = 0; step < 4; step++)
            {
                f.Set(-3, floor + 1 + step, 2 - step, BlockId.CobblestoneStairs);
                f.Fill(-3, floor + 2 + step, 2 - step, -3, floor + 5, 2 - step, BlockId.Air);
            }
        }
        f.Fill(0, 1, -4, 0, 2, -4, BlockId.Air);
        // The platform over the tower, wider by a block each way, with a rail of fences, and the roof
        // on posts at its corners.
        f.Fill(-5, 15, -5, 5, 15, 5, BlockId.DarkOakPlanks);
        f.Fill(-3, 15, -1, -3, 15, 2, BlockId.Air);
        for (int z = -5; z <= 5; z++)
            for (int x = -5; x <= 5; x++)
                if (Math.Abs(x) == 5 || Math.Abs(z) == 5) f.Set(x, 16, z, BlockId.OakFence);
        foreach (var (x, z) in (ReadOnlySpan<(int, int)>)[(-4, -4), (4, -4), (-4, 4), (4, 4)])
            f.Fill(x, 16, z, x, 18, z, BlockId.DarkOakLog);
        f.Fill(-5, 19, -5, 5, 19, 5, BlockId.DarkOakPlanks);
        f.Chest(2, 16, 2, Facing.North, ChestLoot.Outpost.Name, land.Seed);
    }
}
