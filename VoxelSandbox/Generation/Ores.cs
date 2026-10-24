namespace VoxelSandbox;

/// <summary>
/// The ores in the stone and the deepslate, placed as the source's ore features are, each so many
/// blobs a chunk at heights of a uniform or a trapezoid spread, of so many blocks along a short
/// wandering line, an ore of the stone in stone and its deepslate ore in deepslate, taking no
/// other block's place, so a cave's walls and a ravine's show them.
/// </summary>
/// <remarks>
/// <para>
/// The source's placements, by count a chunk, blocks a blob and heights, are coal 30 of 17 from 136
/// to the world's top and 20 of 17 from 0 to 192 peaking between; iron 90 of 9 from 80 to 384, 10
/// of 9 from -24 to 56 and 10 of 4 from the floor to 72; copper 16 of 10 from -16 to 112; gold 4 of
/// 9 from -64 to 32, one of 9 half the time from -64 to -48 and in the badlands 50 more from 32 to
/// 256; redstone 4 of 8 from the floor to 15 and 8 of 8 from -96 to -32; lapis 2 of 7 from -32 to
/// 32 and 4 of 7 buried from the floor to 64; diamond 7 of 4, 4 of 8 buried and one of 12 in nine
/// chunks from -144 to 16, and 2 of 8 from -64 to -4; and in the mountains 100 emeralds of 3 from
/// -16 to 480. A trapezoid spread peaks halfway, as the sum of two even draws does, and a height
/// past the world's places nothing.
/// </para>
/// <para>
/// A blob may leave out the blocks of it open to a cave's air, by a share the source gives each,
/// half for the lower coal, gold and the smaller diamonds and all for the buried lapis and
/// diamonds, so those lie only behind the walls. Each blob stays within its column and depends on
/// the seed and the column alone, so a column is generated the same every time.
/// </para>
/// </remarks>
public static class Ores
{
    private const int Salt = 0x0c0a;

    /// <summary>Each ore in deepslate and its ore in stone.</summary>
    public static readonly Dictionary<BlockId, BlockId> InStone = new()
    {
        [BlockId.DeepslateCoalOre] = BlockId.CoalOre, [BlockId.DeepslateIronOre] = BlockId.IronOre,
        [BlockId.DeepslateCopperOre] = BlockId.CopperOre, [BlockId.DeepslateGoldOre] = BlockId.GoldOre,
        [BlockId.DeepslateRedstoneOre] = BlockId.RedstoneOre, [BlockId.DeepslateLapisOre] = BlockId.LapisOre,
        [BlockId.DeepslateDiamondOre] = BlockId.DiamondOre, [BlockId.DeepslateEmeraldOre] = BlockId.EmeraldOre,
    };

    private static readonly Dictionary<BlockId, BlockId> InDeepslate = InStone.ToDictionary(p => p.Value, p => p.Key);

    /// <summary>The ore of stone a block is the deepslate kind of, or the block itself.</summary>
    public static BlockId Plain(BlockId block) => InStone.GetValueOrDefault(block, block);

    // One of the source's ore placements, its blobs a chunk, one in so many chunks where it is rare,
    // its blocks a blob, its heights and how they spread, the share of a blob's blocks open to air
    // left out, and the biome it alone is placed in.
    private readonly record struct Placement(BlockId Ore, int Count, int Size, int Low, int High, bool Trapezoid, float Discard, int Rarity = 1, string? Only = null);

    private static readonly Placement[] All =
    [
        new(BlockId.CoalOre, 30, 17, 136, ChunkColumn.Ceiling - 1, false, 0),
        new(BlockId.CoalOre, 20, 17, 0, 192, true, 0.5f),
        new(BlockId.IronOre, 90, 9, 80, 384, true, 0),
        new(BlockId.IronOre, 10, 9, -24, 56, true, 0),
        new(BlockId.IronOre, 10, 4, ChunkColumn.Bottom, 72, false, 0),
        new(BlockId.CopperOre, 16, 10, -16, 112, true, 0),
        new(BlockId.GoldOre, 4, 9, -64, 32, true, 0.5f),
        new(BlockId.GoldOre, 1, 9, -64, -48, false, 0.5f, Rarity: 2),
        new(BlockId.GoldOre, 50, 9, 32, 256, false, 0, Only: "badlands"),
        new(BlockId.RedstoneOre, 4, 8, ChunkColumn.Bottom, 15, false, 0),
        new(BlockId.RedstoneOre, 8, 8, -96, -32, true, 0),
        new(BlockId.LapisOre, 2, 7, -32, 32, true, 0),
        new(BlockId.LapisOre, 4, 7, ChunkColumn.Bottom, 64, false, 1),
        new(BlockId.DiamondOre, 7, 4, -144, 16, true, 0.5f),
        new(BlockId.DiamondOre, 2, 8, -64, -4, false, 0.5f),
        new(BlockId.DiamondOre, 1, 12, -144, 16, true, 0.7f, Rarity: 9),
        new(BlockId.DiamondOre, 4, 8, -144, 16, true, 1),
        new(BlockId.EmeraldOre, 100, 3, -16, 480, true, 0, Only: "mountains"),
    ];

    public static void Place(ChunkColumn column, int seed)
    {
        var random = new Random((int)PlaceHash.Of(column.X, column.Z, seed ^ Salt));
        var biome = Biomes.All[column.Biome[8 * Section.Size + 8]].Name;
        foreach (var placement in All)
        {
            if (placement.Only is { } only && only != biome) continue;
            if (placement.Rarity > 1 && random.Next(placement.Rarity) != 0) continue;
            for (int blob = 0; blob < placement.Count; blob++)
            {
                var y = placement.Trapezoid ? Trapezoid(random, placement.Low, placement.High) : placement.Low + random.Next(placement.High - placement.Low + 1);
                Blob(column, random, placement, random.Next(Section.Size), y, random.Next(Section.Size));
            }
        }
    }

    // A height of a trapezoid spread with no plateau, the sum of two even draws over its halves, as the source's.
    private static int Trapezoid(Random random, int low, int high)
    {
        var span = high - low;
        var half = span / 2;
        return low + random.Next(span - half + 1) + random.Next(half + 1);
    }

    // So many blocks of an ore along a wandering line from a place, in stone or deepslate alone, each
    // open to a cave's air left out at the placement's share.
    private static void Blob(ChunkColumn column, Random random, Placement placement, int x, int y, int z)
    {
        for (int step = 0; step < placement.Size; step++)
        {
            if ((uint)x < Section.Size && (uint)z < Section.Size && ChunkColumn.Holds(y))
            {
                var there = column.Get(x, y, z);
                var ore = there == BlockId.Stone ? placement.Ore : there == BlockId.Deepslate ? InDeepslate[placement.Ore] : BlockId.Air;
                if (ore != BlockId.Air && !(placement.Discard > 0 && OpenToAir(column, x, y, z) && random.NextSingle() < placement.Discard))
                    column.Set(x, y, z, ore);
            }
            var way = random.Next(6);
            x += way == 0 ? 1 : way == 1 ? -1 : 0;
            y += way == 2 ? 1 : way == 3 ? -1 : 0;
            z += way == 4 ? 1 : way == 5 ? -1 : 0;
        }
    }

    // Whether a place of the column has air beside it within the column.
    private static bool OpenToAir(ChunkColumn column, int x, int y, int z)
    {
        ReadOnlySpan<(int, int, int)> sides = [(1, 0, 0), (-1, 0, 0), (0, 1, 0), (0, -1, 0), (0, 0, 1), (0, 0, -1)];
        foreach (var (dx, dy, dz) in sides)
        {
            int nx = x + dx, ny = y + dy, nz = z + dz;
            if ((uint)nx < Section.Size && (uint)nz < Section.Size && ChunkColumn.Holds(ny) && column.Get(nx, ny, nz) == BlockId.Air) return true;
        }
        return false;
    }
}
