using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>Sections of open air sharing their arrays, deep sections out of the eye's reach packed, and the reach itself.</summary>
public sealed class ReachTests
{
    [Fact]
    public void A_Section_Packs_And_Unpacks_To_The_Same_Blocks_And_Light_And_Open_Air_Shares_Until_Written()
    {
        var section = new Section();
        section.Set(3, 4, 5, BlockId.Stone);
        section.Set(9, 0, 1, BlockId.Glowstone);
        section.SetLevel(Section.Index(9, 0, 1), false, 15);
        section.SetLevel(Section.Index(2, 2, 2), true, 7);
        var (blocks, light) = (section.Blocks.ToArray(), section.Light.ToArray());
        section.Pack();
        Assert.True(section.Packed);
        Assert.False(section.Dark);
        Assert.InRange(section.Bytes, 1, 2000);
        Assert.Equal(blocks, section.Blocks);
        Assert.Equal(light, section.Light);
        Assert.False(section.Packed);

        // Two sections of open air share their arrays, and a block set in one leaves the other air.
        var (a, b) = (OpenAir(), OpenAir());
        Assert.True(a.Share() && b.Share());
        Assert.Same(a.Blocks, b.Blocks);
        Assert.Equal(0, a.Bytes);
        a.Set(1, 1, 1, BlockId.Dirt);
        Assert.Equal(BlockId.Dirt, a.Get(1, 1, 1));
        Assert.Equal(BlockId.Air, b.Get(1, 1, 1));
        b.SetLevel(Section.Index(1, 1, 1), true, 3);
        Assert.Equal(3, b.SkyLight(Section.Index(1, 1, 1)));
        Assert.False(new Section().Share());
    }

    [Fact]
    public void A_Columns_Deep_Sections_Out_Of_Reach_Pack_And_Read_Back_As_They_Were_Its_Surface_Kept_Whole()
    {
        var land = new Overworld(1);
        var column = land.Generate(20, 0);
        Lighting.Compute(column);
        var whole = column.Sections.Select(s => (s.Blocks.ToArray(), s.Light.ToArray())).ToArray();
        column.Settle();
        Assert.InRange(column.Floor, 30, 120);

        // An eye high over a column 20 away keeps none of its deep sections and every one from under its floor up.
        var packed = column.Pack(new SectionKey(0, 15, 0), 14);
        Assert.True(packed > 0);
        for (int s = 0; s < ChunkColumn.SectionCount; s++)
        {
            var number = ChunkColumn.BottomSection + s;
            var section = column.Sections[s];
            if (number >= column.FloorSection) Assert.False(section.Packed);
        }
        for (int s = 0; s < ChunkColumn.SectionCount; s++)
        {
            Assert.Equal(whole[s].Item1, column.Sections[s].Blocks);
            Assert.Equal(whole[s].Item2, column.Sections[s].Light);
        }
    }

    [Fact]
    public void A_Deep_Section_Is_In_Reach_Within_The_Distance_In_Three_Dimensions_And_A_Surface_One_Everywhere()
    {
        var eye = new SectionKey(0, 4, 0);
        Assert.True(Reach.Within(new SectionKey(40, 6, 0), 3, eye, 12));
        Assert.True(Reach.Within(new SectionKey(8, -4, 0), 3, eye, 12));
        Assert.False(Reach.Within(new SectionKey(11, -4, 0), 3, eye, 12));
        Assert.False(Reach.Within(new SectionKey(0, -4, 12), 3, new SectionKey(0, 12, 0), 12));
        Assert.Equal(new SectionKey(-1, -4, 2), Reach.Of(new System.Numerics.Vector3(-0.5f, -50, 40)));
    }

    [Fact]
    public void Joining_A_Column_Leaves_Its_Neighbors_Dark_Packed_Sections_Packed()
    {
        var land = new Overworld(1);
        var world = new VoxelWorld(land);
        var eye = new SectionKey(0, 15, -40);
        ChunkColumn Made(int x, int z)
        {
            var column = land.Generate(x, z);
            Lighting.Compute(column);
            column.Settle();
            column.Pack(eye, 2);
            return column;
        }
        var first = Made(0, 0);
        world.Add(first);
        var packed = first.Sections.Count(s => s.Packed);
        Assert.True(packed > 0);
        world.Add(Made(1, 0));
        Assert.Equal(packed, first.Sections.Count(s => s.Packed));
    }

    private static Section OpenAir()
    {
        var section = new Section();
        for (int i = 0; i < Section.Volume; i++) section.SetLevel(i, true, Lighting.Max);
        return section;
    }
}
