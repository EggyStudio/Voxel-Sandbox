using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

public class GenerationTests
{
    private static ushort[] Blocks(ChunkColumn column) => [.. column.Sections.SelectMany(s => s.Blocks)];

    [Fact]
    public void A_Column_Is_Generated_The_Same_Every_Time()
    {
        var land = new Overworld(1);
        var first = land.Generate(5, -7);
        land.Generate(4, -7);
        var again = new Overworld(1).Generate(5, -7);

        Assert.Equal(Blocks(first), Blocks(again));
        Assert.Equal(first.Biome, again.Biome);
    }

    [Fact]
    public void A_Tree_On_A_Columns_Edge_Grows_Its_Crown_Into_The_Column_Beside_It()
    {
        // The first trunks found standing on the +x edge of a column, and the blocks beside each in
        // the next column, generated alone, about its top.
        var land = new Overworld(1);
        var checkedTrunks = 0;
        for (int cx = -20; cx < 20 && checkedTrunks < 5; cx++)
        {
            var column = land.Generate(cx, 2);
            var next = land.Generate(cx + 1, 2);
            for (int z = 2; z < 14; z++)
            {
                var top = -1;
                for (int y = 0; y < ChunkColumn.Height; y++)
                    if (column.Get(15, y, z) is BlockId.OakLog or BlockId.BirchLog or BlockId.SpruceLog) top = y;
                if (top < 0 || column.Get(14, top, z) is BlockId.OakLog or BlockId.BirchLog or BlockId.SpruceLog) continue;
                var crown = 0;
                for (int y = top - 2; y <= top + 1; y++)
                    for (int dz = -1; dz <= 1; dz++)
                        if (next.Get(0, y, z + dz) != BlockId.Air) crown++;
                Assert.True(crown > 0, $"the trunk at column {cx}, x 15, z {z} up to {top} has no crown across its edge");
                checkedTrunks++;
            }
        }
        Assert.True(checkedTrunks > 0, "no trunk stood on a column's edge in the columns read");
    }

    [Fact]
    public void Water_Stands_To_Sea_Level_Over_Ground_Under_It()
    {
        var land = new Overworld(1);
        // The ground near the origin that lies lowest, which the survey of seed 1 puts under water.
        var (x, z) = Enumerable.Range(-20, 41).SelectMany(i => Enumerable.Range(-20, 41).Select(j => (X: i * 8, Z: j * 8)))
            .MinBy(p => land.At(p.X, p.Z).Height);
        var height = land.At(x, z).Height;
        Assert.True(height < Overworld.SeaLevel, $"the lowest ground found stands at {height}");

        var column = land.Generate(x >> 4, z >> 4);
        var top = column.Get(x & 15, Overworld.SeaLevel, z & 15);
        Assert.True(top is BlockId.Water or BlockId.Ice);
        Assert.Equal(BlockId.Air, column.Get(x & 15, Overworld.SeaLevel + 1, z & 15));
    }

    [Fact]
    public void Every_Torch_And_Lantern_Of_A_Village_Holds_To_A_Block_And_No_Tree_Stands_In_It()
    {
        var land = new Overworld(1);
        var village = Structures.Find("village")!;
        var start = Enumerable.Range(-6, 13).SelectMany(cz => Enumerable.Range(-6, 13).Select(cx => village.StartIn(cx, cz, land)))
            .First(s => s is not null)!.Value;

        // Every column the village reaches, each generated alone, and the blocks read across them.
        var columns = new Dictionary<(int, int), ChunkColumn>();
        for (int cz = (start.Z - 4) >> Section.Shift; cz <= (start.Z + village.Size + 4) >> Section.Shift; cz++)
            for (int cx = (start.X - 4) >> Section.Shift; cx <= (start.X + village.Size + 4) >> Section.Shift; cx++)
                columns[(cx, cz)] = land.Generate(cx, cz);
        BlockId At(int x, int y, int z) =>
            columns.TryGetValue((x >> Section.Shift, z >> Section.Shift), out var c) && (uint)y < ChunkColumn.Height ? c.Get(x & Section.Mask, y, z & Section.Mask) : BlockId.Air;

        int lights = 0, paths = 0;
        for (int z = start.Z; z < start.Z + village.Size; z++)
            for (int x = start.X; x < start.X + village.Size; x++)
                for (int y = 1; y < ChunkColumn.Height; y++)
                {
                    var block = At(x, y, z);
                    if (block == BlockId.DirtPath) paths++;
                    Assert.False(block is BlockId.OakLeaves or BlockId.SpruceLeaves or BlockId.BirchLeaves, $"leaves stand at {x}, {y}, {z} in the village");
                    if (VoxelSandbox.Blocks.Get(block).Shape is not { Falls: true } shape || VoxelSandbox.Blocks.Get(block).Light == 0) continue;
                    lights++;
                    var (sx, sy, sz) = shape.Support;
                    Assert.True(VoxelSandbox.Blocks.Holds(At(x + sx, y + sy, z + sz), shape.Facing), $"{VoxelSandbox.Blocks.Get(block).Key} at {x}, {y}, {z} holds to {VoxelSandbox.Blocks.Get(At(x + sx, y + sy, z + sz)).Key}");
                }
        Assert.True(paths > 100, $"the village has {paths} blocks of road");
        Assert.True(lights >= 9, $"the village has {lights} torches and lanterns");
    }

    [Fact]
    public void No_Tunnel_Opens_And_No_Ravine_Is_Cut_Under_Seed_1s_First_Villages()
    {
        var land = new Overworld(1);
        var village = Structures.Find("village")!;
        var ravines = Structures.Find("ravine")!;
        var starts = Enumerable.Range(-12, 25).SelectMany(cz => Enumerable.Range(-12, 25).Select(cx => village.StartIn(cx, cz, land)))
            .OfType<StructureStart>().OrderBy(s => Math.Abs(s.X) + Math.Abs(s.Z)).Take(8).ToList();
        Assert.Equal(8, starts.Count);

        int roads = 0;
        foreach (var start in starts)
        {
            int x0 = start.X - 6, z0 = start.Z - 6, x1 = start.X + village.Size + 5, z1 = start.Z + village.Size + 5;
            Assert.Empty(ravines.StartsIn(x0, z0, x1, z1, land));
            var columns = new Dictionary<(int, int), ChunkColumn>();
            for (int cz = z0 >> Section.Shift; cz <= z1 >> Section.Shift; cz++)
                for (int cx = x0 >> Section.Shift; cx <= x1 >> Section.Shift; cx++)
                    columns[(cx, cz)] = land.Generate(cx, cz);
            BlockId At(int x, int y, int z) => columns[(x >> Section.Shift, z >> Section.Shift)].Get(x & Section.Mask, y, z & Section.Mask);

            // Open air from over the ground reaches no deeper than three blocks under its top
            // anywhere on the village's ground, as far as a house set into a slope clears, so no
            // pit opens beside a road or a footing, and a road lies on three whole blocks.
            for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++)
                {
                    var ground = land.At(x, z).Height;
                    var floor = ground + 1;
                    while (floor > 0 && At(x, floor, z) == BlockId.Air) floor--;
                    Assert.True(floor >= ground - 3, $"open air down to {floor + 1} at {x}, {z}, where the ground is {ground}, by the village from {start.X}, {start.Z}");
                    if (At(x, ground, z) != BlockId.DirtPath) continue;
                    roads++;
                    for (int y = ground - 3; y < ground; y++) Assert.NotEqual(BlockId.Air, At(x, y, z));
                }
        }
        Assert.True(roads > 8 * 100, $"{roads} blocks of road");
    }

    [Fact]
    public void A_Ravine_Cuts_The_Ground_Open_To_The_Sky_At_Its_Middle()
    {
        var land = new Overworld(1);
        var ravine = Structures.Find("ravine")!;
        var start = Enumerable.Range(-6, 13).SelectMany(cz => Enumerable.Range(-6, 13).Select(cx => ravine.StartIn(cx, cz, land)))
            .First(s => s is not null)!.Value;
        int x = start.X + ravine.Size / 2, z = start.Z + ravine.Size / 2;
        var ground = land.At(x, z).Height;

        var column = land.Generate(x >> Section.Shift, z >> Section.Shift);
        Lighting.Compute(column);
        for (int y = ground - 6; y <= ground + 2; y++)
            Assert.Equal(BlockId.Air, column.Get(x & Section.Mask, y, z & Section.Mask));
        var index = Section.Index(x & Section.Mask, (ground - 6) & Section.Mask, z & Section.Mask);
        Assert.Equal(Lighting.Max, column.Sections[(ground - 6) >> Section.Shift].SkyLight(index));
    }

    [Fact]
    public void Somewhere_Near_The_Origin_A_Tunnel_Breaks_Out_Under_The_Open_Sky()
    {
        // A cave's mouth is air under the ground's height that the open sky shines straight into,
        // which no ravine near the origin makes.
        var land = new Overworld(1);
        var ravines = Structures.Find("ravine")!;
        var mouths = 0;
        for (int cz = -12; cz < 12 && mouths == 0; cz++)
            for (int cx = -12; cx < 12 && mouths == 0; cx++)
            {
                if (ravines.StartsIn(cx * Section.Size, cz * Section.Size, cx * Section.Size + Section.Mask, cz * Section.Size + Section.Mask, land).Any()) continue;
                var column = land.Generate(cx, cz);
                Lighting.Compute(column);
                for (int z = 0; z < Section.Size; z++)
                    for (int x = 0; x < Section.Size; x++)
                    {
                        var ground = land.At(cx * Section.Size + x, cz * Section.Size + z).Height;
                        var y = ground - 5;
                        if (y <= Overworld.SeaLevel || column.Get(x, y, z) != BlockId.Air) continue;
                        if (column.Sections[y >> Section.Shift].SkyLight(Section.Index(x, y & Section.Mask, z)) == Lighting.Max) mouths++;
                    }
            }
        Assert.True(mouths > 0, "no tunnel near the origin opens to the sky");
    }

    [Fact]
    public void No_Plant_Stands_Over_Ground_A_Cave_Or_A_Ravine_Carved_Away()
    {
        // The land of seed 1 for a hundred and sixty blocks each way about a ravine's start, its plants
        // each standing on a block a body stands on.
        var land = new Overworld(1);
        var start = Structures.Find("ravine")!.StartsIn(-512, -512, 512, 512, land).First();
        int cx = start.X >> Section.Shift, cz = start.Z >> Section.Shift;
        var floating = new List<string>();
        for (int dz = -5; dz <= 5; dz++)
            for (int dx = -5; dx <= 5; dx++)
            {
                var column = land.Generate(cx + dx, cz + dz);
                for (int z = 0; z < Section.Size; z++)
                    for (int x = 0; x < Section.Size; x++)
                        for (int y = 1; y < ChunkColumn.Height; y++)
                            if (BlockGroups.Plants.Contains(column.Get(x, y, z)) && !VoxelSandbox.Blocks.Collides(column.Get(x, y - 1, z)))
                                floating.Add($"{column.Get(x, y, z)} at {(cx + dx) * Section.Size + x}, {y}, {(cz + dz) * Section.Size + z}");
            }
        Assert.Empty(floating);
    }
}
