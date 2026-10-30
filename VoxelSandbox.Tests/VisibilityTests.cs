using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

public class VisibilityTests
{
    [Fact]
    public void A_Section_Of_Stone_Joins_No_Faces_And_One_Of_Air_Joins_Them_All()
    {
        var stone = new Section();
        for (int i = 0; i < Section.Volume; i++) stone.Set(i & 15, i >> 8, (i >> 4) & 15, BlockId.Stone);
        Assert.Equal(0UL, Visibility.Joins(stone));
        Assert.Equal((1UL << 36) - 1, Visibility.Joins(new Section()));

        // A tunnel along x through the stone joins its two ends and nothing else.
        for (int x = 0; x < 16; x++) stone.Set(x, 8, 8, BlockId.Air);
        var joins = Visibility.Joins(stone);
        Assert.NotEqual(0UL, joins & 1UL << (0 * 6 + 1));
        Assert.Equal(0UL, joins & 1UL << (0 * 6 + 2));
    }

    [Fact]
    public void A_Flood_As_The_Eye_Steps_Into_The_Next_Section_Goes_On_Over_Frames_Keeping_The_Last_Ones_Sections()
    {
        // Stone from the bottom to 60 and the sky above it, the eye in the open over the stone.
        var world = Laid.World(c => Laid.Fill(c, 0, 60, BlockId.Stone));
        var visibility = new Visibility();
        Assert.True(visibility.Flood(world, new Vector3(8.5f, 70, 8.5f), 2));
        var first = visibility.Seen.ToHashSet();

        // A step into the section beside, with no time to flood, keeps the last flood's sections
        // and finishes in a later call, which leaves out the column three behind that the first held.
        Assert.False(visibility.Flood(world, new Vector3(24.5f, 70, 8.5f), 2, budgetMs: 0));
        Assert.Equal(first, visibility.Seen.ToHashSet());
        var calls = 1;
        while (!visibility.Flood(world, new Vector3(24.5f, 70, 8.5f), 2, budgetMs: 0)) calls++;
        Assert.True(calls > 1, "the flood finished in one call");
        Assert.Contains(new SectionKey(-3, 4, 0), first);
        Assert.DoesNotContain(new SectionKey(-3, 4, 0), visibility.Seen);

        // A jump of more than a section floods at once.
        Assert.True(visibility.Flood(world, new Vector3(-40.5f, 70, 8.5f), 2, budgetMs: 0));
    }

    [Fact]
    public void From_A_Closed_Room_Under_The_Ground_The_Flood_Reaches_The_Room_Alone()
    {
        // Stone from the bottom to 60, the sky above it.
        var world = Laid.World(c => Laid.Fill(c, 0, 60, BlockId.Stone));
        for (int y = 20; y <= 26; y++)
            for (int z = 4; z <= 10; z++)
                for (int x = 4; x <= 10; x++) world.SetBlock(x, y, z, BlockId.Air);
        var visibility = new Visibility();

        visibility.Flood(world, new Vector3(7.5f, 22, 7.5f), 2);

        Assert.Equal(new HashSet<SectionKey> { new(0, 1, 0) }, visibility.Seen.Where(k => world.HasColumn(k.X, k.Z)).ToHashSet());
        Assert.False(visibility.Sees(new SectionKey(0, 4, 0)));

        // From the sky the flood reaches every column's top sections.
        visibility.Flood(world, new Vector3(7.5f, 100, 7.5f), 2);
        Assert.True(visibility.Sees(new SectionKey(1, 6, 1)) && visibility.Sees(new SectionKey(-1, 3, 0)));
        Assert.False(visibility.Sees(new SectionKey(0, 1, 0)));
    }

    [Fact]
    public void A_Ray_Toward_The_Light_Marks_The_Sections_Left_Out_It_Passes_To_The_Top_Or_Its_Length()
    {
        // From the top of the one section seen straight up, every section over it to the world's top.
        var seen = new SectionKey(0, 0, 0);
        var marked = new HashSet<SectionKey>();
        ChunkRenderer.March(new Vector3(8, 16, 8), Vector3.UnitY, float.MaxValue, key => key == seen, marked);
        Assert.Equal(Enumerable.Range(1, ChunkColumn.CeilingSection - 1).Select(y => new SectionKey(0, y, 0)), marked.OrderBy(k => k.Y));
        // Leaning toward +x, up a stair of sections that never turns back, as far as its length.
        marked.Clear();
        ChunkRenderer.March(new Vector3(16, 16, 8), Vector3.Normalize(new Vector3(1, 1, 0)), 40, key => key == seen, marked);
        Assert.Contains(new SectionKey(1, 1, 0), marked);
        Assert.DoesNotContain(seen, marked);
        Assert.All(marked, key => Assert.True(key.X >= 0 && key.Z == 0 && key.X <= 3 && key.Y <= 3, key.ToString()));
        // A section it passes that is seen is left out of the mark.
        marked.Clear();
        ChunkRenderer.March(new Vector3(8, 16, 8), Vector3.UnitY, float.MaxValue, key => key.Y <= 2, marked);
        Assert.DoesNotContain(new SectionKey(0, 2, 0), marked);
        Assert.Contains(new SectionKey(0, 3, 0), marked);
    }
}
