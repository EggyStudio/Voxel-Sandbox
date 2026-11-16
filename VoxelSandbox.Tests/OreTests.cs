using Xunit;

namespace VoxelSandbox.Tests;

public class OreTests
{
    [Fact]
    public void A_Vein_Goes_On_Across_A_Columns_Edge()
    {
        // Columns of stone alone side by side, their ores placed, and the ores standing on an edge
        // between two of them, of which a vein going on across the edge finds its ore across it
        // far oftener than the ores of the column beside would by their share of its blocks.
        var columns = new Dictionary<(int X, int Z), ChunkColumn>();
        for (int cz = 0; cz < 4; cz++)
            for (int cx = 0; cx < 4; cx++)
            {
                var column = new ChunkColumn(cx, cz);
                for (int y = 0; y < 128; y++)
                    for (int z = 0; z < Section.Size; z++)
                        for (int x = 0; x < Section.Size; x++) column.Set(x, y, z, BlockId.Stone);
                Ores.Place(column, 1, (_, _) => Biomes.Plains);
                columns[(cx, cz)] = column;
            }
        var (onEdge, across, ores, stone) = (0, 0, 0, 0);
        foreach (var ((cx, cz), column) in columns)
            for (int y = 0; y < 128; y++)
                for (int z = 0; z < Section.Size; z++)
                    for (int x = 0; x < Section.Size; x++)
                    {
                        var block = column.Get(x, y, z);
                        stone++;
                        if (block == BlockId.Stone) continue;
                        ores++;
                        if (x != Section.Mask || !columns.TryGetValue((cx + 1, cz), out var east)) continue;
                        onEdge++;
                        if (east.Get(0, y, z) == block) across++;
                    }

        Assert.True(onEdge > 100, $"{onEdge} ores on an edge");
        Assert.True(across > onEdge / 8, $"{across} of {onEdge} ores on an edge find their ore across it, where {ores} of {stone} blocks are ore");
    }
}
