using System.IO.Compression;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The world from -64 to 320, its floor and its deepslate as the source lays them, and a save kept before it stood so.</summary>
public sealed class WorldLayoutTests
{
    [Fact]
    public void The_World_Stands_From_Minus_64_To_320_On_Bedrock_With_Deepslate_Under_0()
    {
        Assert.Equal((-64, 320, -4, 20), (ChunkColumn.Bottom, ChunkColumn.Ceiling, ChunkColumn.BottomSection, ChunkColumn.CeilingSection));
        var land = new Overworld(1);
        var bedrock = new int[6];
        var (deepslate, stone) = (new int[10], new int[10]);
        for (int c = 0; c < 4; c++)
        {
            var column = land.Generate(c, -c);
            for (int z = 0; z < Section.Size; z++)
                for (int x = 0; x < Section.Size; x++)
                {
                    for (int y = -64; y <= -59; y++)
                        if (column.Get(x, y, z) == BlockId.Bedrock) bedrock[y + 64]++;
                    for (int y = -60; y < 0; y++) Assert.NotEqual(BlockId.Stone, column.Get(x, y, z));
                    for (int y = 0; y < 10; y++)
                    {
                        if (column.Get(x, y, z) == BlockId.Deepslate) deepslate[y]++;
                        if (column.Get(x, y, z) == BlockId.Stone) stone[y]++;
                    }
                }
        }
        // Whole bedrock at the floor, thinning out by a fifth a block over it, none at -59.
        Assert.Equal(1024, bedrock[0]);
        Assert.True(bedrock[1] > bedrock[2] && bedrock[2] > bedrock[3] && bedrock[3] > bedrock[4] && bedrock[4] > 0, string.Join(", ", bedrock));
        Assert.Equal(0, bedrock[5]);
        // Deepslate whole at 0, thinning out to none at 8, stone taking its place.
        Assert.Equal(0, stone[0]);
        Assert.True(deepslate[2] > deepslate[6] && stone[6] > stone[2], $"{string.Join(", ", deepslate)} against {string.Join(", ", stone)}");
        Assert.Equal(0, deepslate[8]);
        Assert.Equal(0, deepslate[9]);
    }

    [Fact]
    public void A_Column_Kept_While_The_World_Stood_From_0_Lies_Over_Generated_Deep_Stone()
    {
        // Eight sections as a save held them, bedrock along the floor of 0 and planks at a height of 5.
        var old = new ushort[8][];
        for (int s = 0; s < 8; s++) old[s] = new ushort[Section.Volume];
        for (int z = 0; z < Section.Size; z++)
            for (int x = 0; x < Section.Size; x++)
                (old[0][Section.Index(x, 0, z)], old[0][Section.Index(x, 5, z)]) = ((ushort)BlockId.Bedrock, (ushort)BlockId.OakPlanks);
        using var bytes = new MemoryStream();
        using (var zip = new ZLibStream(bytes, CompressionLevel.Fastest))
            foreach (var section in old) zip.Write(System.Runtime.InteropServices.MemoryMarshal.AsBytes(section.AsSpan()));
        var kept = bytes.ToArray();
        Assert.True(WorldSave.Shallow(kept));

        var generated = new Overworld(1).Generate(5, 7);
        var column = WorldSave.Decode(5, 7, kept, null, generated);
        Assert.True(column.Changed);
        for (int z = 0; z < Section.Size; z++)
            for (int x = 0; x < Section.Size; x++)
            {
                Assert.Equal(BlockId.OakPlanks, column.Get(x, 5, z));
                Assert.Equal(generated.Get(x, 0, z), column.Get(x, 0, z));
                Assert.Equal(BlockId.Bedrock, column.Get(x, -64, z));
                Assert.Equal(generated.Get(x, -30, z), column.Get(x, -30, z));
                Assert.Equal(BlockId.Air, column.Get(x, 200, z));
            }

        // Kept again, it holds every section and opens as it stands.
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var save = new WorldSave(folder);
            save.Keep(column);
            var whole = save.Kept(5, 7)!;
            Assert.False(WorldSave.Shallow(whole));
            var back = WorldSave.Decode(5, 7, whole, null);
            Assert.Equal(BlockId.OakPlanks, back.Get(3, 5, 3));
            Assert.Equal(column.Get(3, -30, 3), back.Get(3, -30, 3));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
