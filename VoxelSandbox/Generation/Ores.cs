namespace VoxelSandbox;

/// <summary>
/// Veins of ore in the stone by depth, as Minecraft's are, each so many blocks along a short
/// wandering line: coal 18 a column at heights from 5 to 100, eight long, iron 10 from 5 to 64, six
/// long, copper 10 from 20 to 80, eight long, gold 4 from 5 to 32 and redstone 6 from 5 to 16, seven
/// long, lapis 2 from 5 to 32, six long, a diamond vein from 5 to 16, five long, and in the
/// mountains three single emeralds from 48 to 98, taking the place of stone alone, so a cave's
/// walls and a ravine's show them. Minecraft's depths are raised, so the rarest ores lie in the stone
/// over the deepslate under the floor of 0, which holds none.
/// </summary>
/// <remarks>Each vein stays within its column and depends on the seed and the column alone, so a column is generated the same every time.</remarks>
public static class Ores
{
    private const int Salt = 0x0c0a;

    public static void Place(ChunkColumn column, int seed)
    {
        var random = new Random((int)PlaceHash.Of(column.X, column.Z, seed ^ Salt));
        Veins(column, random, BlockId.CoalOre, 18, 8, 5, 96);
        Veins(column, random, BlockId.IronOre, 10, 6, 5, 60);
        Veins(column, random, BlockId.CopperOre, 10, 8, 20, 60);
        Veins(column, random, BlockId.GoldOre, 4, 7, 5, 28);
        Veins(column, random, BlockId.RedstoneOre, 6, 7, 5, 12);
        Veins(column, random, BlockId.LapisOre, 2, 6, 5, 28);
        Veins(column, random, BlockId.DiamondOre, 1, 5, 5, 12);
        if (column.Biome[8 * Section.Size + 8] == Biomes.Mountains.Id) Veins(column, random, BlockId.EmeraldOre, 3, 1, 48, 50);
    }

    // So many veins of an ore, each so many steps long, at heights from a floor up through a span.
    private static void Veins(ChunkColumn column, Random random, BlockId ore, int veins, int length, int floor, int span)
    {
        for (int vein = 0; vein < veins; vein++)
        {
            int x = random.Next(Section.Size), y = floor + random.Next(span), z = random.Next(Section.Size);
            for (int step = 0; step < length; step++)
            {
                if ((uint)x < Section.Size && (uint)z < Section.Size && column.Get(x, y, z) == BlockId.Stone) column.Set(x, y, z, ore);
                var way = random.Next(6);
                x += way == 0 ? 1 : way == 1 ? -1 : 0;
                y += way == 2 ? 1 : way == 3 ? -1 : 0;
                z += way == 4 ? 1 : way == 5 ? -1 : 0;
            }
        }
    }
}
