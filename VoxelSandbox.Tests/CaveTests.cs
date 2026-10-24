using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The source's newer caves, their aquifers and its ores by depth, over seed 1's spawn.</summary>
public sealed class CaveTests
{
    [Fact]
    public void The_Caves_Open_The_Deep_Ground_And_Its_Top_At_Entrances_But_Never_Under_Water()
    {
        var land = new Overworld(1);
        long air = 0, under = 0;
        int opened = 0, drained = 0;
        for (int cz = -3; cz <= 3; cz++)
            for (int cx = -3; cx <= 3; cx++)
            {
                var column = land.Generate(cx, cz);
                for (int z = 0; z < Section.Size; z++)
                    for (int x = 0; x < Section.Size; x++)
                    {
                        var ground = land.Ground(cx * 16 + x, cz * 16 + z).Height;
                        for (int y = -40; y < ground - 12; y++)
                        {
                            under++;
                            if (column.Get(x, y, z) == BlockId.Air) air++;
                        }
                        var open = column.Top(x, z) < ground - 2;
                        if (open && ground > Overworld.SeaLevel + 1) opened++;
                        if (open && ground <= Overworld.SeaLevel + 1) drained++;
                    }
            }
        Assert.InRange(100.0 * air / under, 1, 20);
        Assert.InRange(opened, 50, 3000);
        Assert.Equal(0, drained);
    }

    [Fact]
    public void The_Caves_Hold_Lava_Under_Minus_54_And_Water_In_Some_Aquifers_None_Of_It_Over_Air()
    {
        var land = new Overworld(1);
        long lava = 0, deepAir = 0, water = 0, floating = 0;
        for (int cz = -40; cz <= 40; cz += 8)
            for (int cx = -40; cx <= 40; cx += 8)
            {
                var column = land.Generate(cx, cz);
                for (int z = 0; z < Section.Size; z++)
                    for (int x = 0; x < Section.Size; x++)
                    {
                        var ground = land.Ground(cx * 16 + x, cz * 16 + z).Height;
                        for (int y = ChunkColumn.Bottom + 1; y < ground - 3; y++)
                        {
                            var block = column.Get(x, y, z);
                            if (y < Aquifers.LavaLevel && block == BlockId.Air) deepAir++;
                            if (block == BlockId.Lava) lava++;
                            if (!Blocks.IsWater(block)) continue;
                            water++;
                            if (column.Get(x, y - 1, z) == BlockId.Air) floating++;
                        }
                    }
            }
        Assert.Equal(0, deepAir);
        Assert.True(lava > 100, $"{lava} blocks of lava");
        Assert.True(water > 1000, $"{water} blocks of water");
        // Two aquifers' levels meet behind a wall the pressure between them keeps, so none stands over a cave's air.
        Assert.Equal(0, floating);
    }

    [Fact]
    public void Lava_Lights_Its_Cave_Gives_Way_And_Is_No_Lamp_Item_Or_Target()
    {
        var lava = Blocks.Get(BlockId.Lava);
        Assert.Equal(15, lava.Light);
        Assert.True(lava.Molten);
        Assert.Empty(lava.Glows);
        Assert.False(Blocks.IsLamp(BlockId.Lava));
        Assert.False(Blocks.Collides(BlockId.Lava));
        Assert.False(Blocks.IsTarget(BlockId.Lava));
        Assert.True(BlockGroups.GivesWay(BlockId.Lava));
        Assert.DoesNotContain(Items.All, item => item.Block == BlockId.Lava);
    }

    [Fact]
    public void A_Column_Is_Carved_The_Same_Each_Time_It_Is_Made()
    {
        var (first, second) = (new Overworld(1).Generate(-2, 3), new Overworld(1).Generate(-2, 3));
        for (int s = 0; s < ChunkColumn.SectionCount; s++) Assert.Equal(first.Sections[s].Blocks, second.Sections[s].Blocks);
    }

    [Fact]
    public void Diamonds_Lie_In_The_Deepslate_And_The_Stone_Just_Over_It()
    {
        var land = new Overworld(1);
        int deep = 0, stone = 0, highest = int.MinValue;
        for (int cz = -3; cz <= 3; cz++)
            for (int cx = -3; cx <= 3; cx++)
            {
                var column = land.Generate(cx, cz);
                for (int y = ChunkColumn.Bottom; y < 64; y++)
                    for (int z = 0; z < Section.Size; z++)
                        for (int x = 0; x < Section.Size; x++)
                        {
                            var block = column.Get(x, y, z);
                            if (block == BlockId.DeepslateDiamondOre) deep++;
                            if (block == BlockId.DiamondOre) stone++;
                            if (block is BlockId.DiamondOre or BlockId.DeepslateDiamondOre) highest = Math.Max(highest, y);
                        }
            }
        // The source's diamonds peak far under the world's floor, so most of those placed lie in the deepslate.
        Assert.True(deep > 3 * stone, $"{deep} deepslate diamond ore against {stone} in stone");
        Assert.InRange(highest, 0, 28);
        Assert.Equal(BlockId.DiamondOre, Ores.Plain(BlockId.DeepslateDiamondOre));
        Assert.Equal(Items.Diamond, Loot.Of(BlockId.DeepslateDiamondOre, new Random(1), Items.Find("iron_pickaxe")).Single().Item);
        Assert.Equal(4.5f, Loot.Hardness(BlockId.DeepslateDiamondOre));
    }
}
