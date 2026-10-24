using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The surfaces seen kept for far land, its rings, and its cells' boxes.</summary>
public sealed class FarLandTests
{
    [Fact]
    public void A_Seen_Columns_Surface_Is_Kept_And_Read_Back_From_Its_Saves_Folder()
    {
        var column = new Overworld(1).Generate(-3, 70);
        var folder = Path.Combine(Directory.CreateTempSubdirectory().FullName, "far");
        try
        {
            var seen = new FarCache(folder);
            Assert.Null(seen.Get(-3, 70));
            seen.Record(column);
            Assert.True(seen.Recorded.TryDequeue(out var recorded) && recorded == (-3, 70));
            seen.Flush();
            var again = new FarCache(folder).Get(-3, 70);
            Assert.NotNull(again);
            for (int z = 0; z < Section.Size; z++)
                for (int x = 0; x < Section.Size; x++)
                {
                    var at = z * Section.Size + x;
                    var top = column.Top(x, z);
                    Assert.Equal(top, again.Heights[at]);
                    Assert.Equal(column.Get(x, top, z), (BlockId)again.Tops[at]);
                    Assert.Equal(column.Biome[at], again.Biomes[at]);
                }
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(folder)!, true);
        }
    }

    [Fact]
    public void The_Rings_Double_Out_From_The_Render_Distance_To_The_Far_One()
    {
        Assert.Equal([(12, 24), (24, 48), (48, 64)], Enumerable.Range(1, FarLand.Levels).Select(l => FarLand.Bounds(l, 12, 64)));
        Assert.Equal([(12, 24), (24, 32), (48, 32)], Enumerable.Range(1, FarLand.Levels).Select(l => FarLand.Bounds(l, 12, 32)));
        // A column on the near land's edge is the near land's, and the next the first ring's.
        Assert.False(FarLand.Within(12, 0, 12, 24));
        Assert.True(FarLand.Within(13, 0, 12, 24));
        Assert.True(FarLand.Within(24, 0, 12, 24));
        Assert.False(FarLand.Within(25, 0, 12, 24));
    }

    [Fact]
    public void Level_Land_Is_One_Top_And_A_Step_Stands_On_Walls_Rounded_To_The_Cells_Width()
    {
        var (heights, colors) = (new short[FarLand.Side * FarLand.Side], new Color[FarLand.Side * FarLand.Side]);
        Array.Fill(heights, (short)63);
        Array.Fill(colors, new Color(90, 140, 60));
        // A tile of the first ring wholly within it, 20 columns from the eye's.
        var level = FarLand.Mesh(1, 5, 0, heights, colors, 0, 0, 0, 1000);
        Assert.Equal(6, level.Indices.Count);
        Assert.All(level.Positions, p => Assert.Equal(64, p.Y));

        // One cell 4 higher, its top at 68, is a top of its own, its three neighbors' walls and the
        // land's top around it in more rectangles.
        heights[10 * FarLand.Side + 10] = 67;
        var step = FarLand.Mesh(1, 5, 0, heights, colors, 0, 0, 0, 1000);
        Assert.Contains(step.Positions, p => p.Y == 68);
        Assert.True(step.Indices.Count / 6 >= 1 + 4 + 4);

        // A cell at 64 in cells two wide rounds its top of 65 down to 64.
        heights[10 * FarLand.Side + 10] = 64;
        Assert.DoesNotContain(FarLand.Mesh(1, 5, 0, heights, colors, 0, 0, 0, 1000).Positions, p => p.Y == 65);

        // A tile the ring does not reach draws nothing.
        Assert.Equal(0, FarLand.Mesh(1, 5, 0, heights, colors, 0, 0, 100, 200).Count);
    }
}
