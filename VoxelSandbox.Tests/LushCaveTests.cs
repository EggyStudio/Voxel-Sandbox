using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The source's lush caves, under seed 1's wettest ground near x 960 and z -990, and their cave vines and glow berries.</summary>
public sealed class LushCaveTests
{
    [Fact]
    public void Under_The_Wettest_Ground_The_Caves_Hold_Moss_Vines_With_Berries_And_Pools_And_Under_Dry_Ground_None()
    {
        var land = new Overworld(1);
        int moss = 0, vines = 0, lit = 0, clay = 0, plants = 0, loose = 0, open = 0;
        foreach (var (cx, cz) in (ReadOnlySpan<(int, int)>)[(60, -62), (62, -62), (56, -64)])
        {
            var column = land.Generate(cx, cz);
            for (int z = 0; z < Section.Size; z++)
                for (int x = 0; x < Section.Size; x++)
                    for (int y = ChunkColumn.Bottom + 1; y < 40; y++)
                    {
                        var block = column.Get(x, y, z);
                        if (block == BlockId.MossBlock) moss++;
                        if (block is BlockId.ShortGrass or BlockId.MossCarpet or BlockId.TallGrass) plants++;
                        if (block == BlockId.Clay) clay++;
                        if (block is BlockId.CaveVines or BlockId.CaveVinesLit)
                        {
                            if (block == BlockId.CaveVines) vines++;
                            else lit++;
                            if (!Blocks.Holds(column.Get(x, y + 1, z), Facing.Down)) loose++;
                        }
                        // No water stands beside a cave's air within the column, a pool's held in by its rim.
                        if (Blocks.IsWater(block) && (x is > 0 and < Section.Size - 1 && z is > 0 and < Section.Size - 1))
                            foreach (var (dx, dy, dz) in (ReadOnlySpan<(int, int, int)>)[(1, 0, 0), (-1, 0, 0), (0, 0, 1), (0, 0, -1), (0, -1, 0)])
                                if (column.Get(x + dx, y + dy, z + dz) == BlockId.Air) open++;
                    }
        }
        Assert.True(moss > 500, $"{moss} moss blocks");
        Assert.True(plants > 200, $"{plants} plants on the moss");
        Assert.True(clay > 40, $"{clay} blocks of clay about the pools");
        Assert.Equal(0, loose);
        Assert.Equal(0, open);
        // A fifth of the vines' blocks bear glow berries.
        Assert.InRange(100.0 * lit / (vines + lit), 10, 30);

        var dry = land.Generate(6, -4);
        for (int s = 0; s < ChunkColumn.SectionCount; s++) Assert.DoesNotContain((ushort)BlockId.MossBlock, dry.Sections[s].Blocks);
    }

    [Fact]
    public void Glow_Berries_Hang_Vines_Under_A_Block_That_Fall_With_It_And_Only_Vines_With_Berries_Give_Them()
    {
        var world = Laid.World(c => c.Set(8, 20, 8, BlockId.Stone));
        var berries = Items.Find("glow_berries") ?? Items.Find("Glow Berries")!;
        Assert.Equal((2, 0.4f), (berries.Food, berries.Saturation));
        Assert.Equal(BlockId.CaveVines, world.Fit(berries.Block!.Value, 8, 19, 8, 0, -1, 0));
        Assert.Equal(BlockId.Air, world.Fit(berries.Block!.Value, 8, 21, 8, 0, 1, 0));

        world.SetBlock(8, 19, 8, BlockId.CaveVines);
        world.SetBlock(8, 18, 8, BlockId.CaveVinesLit);
        world.SetBlock(8, 17, 8, BlockId.CaveVines);
        world.SetBlock(8, 20, 8, BlockId.Air);
        Assert.Equal(3, world.Fallen.Count);
        Assert.All([19, 18, 17], y => Assert.Equal(BlockId.Air, world.GetBlock(8, y, 8)));

        Assert.Empty(Loot.Of(BlockId.CaveVines, new Random(1)));
        Assert.Equal(berries, Loot.Of(BlockId.CaveVinesLit, new Random(1)).Single().Item);
        Assert.Equal(14, Blocks.Get(BlockId.CaveVinesLit).Light);
    }
}
