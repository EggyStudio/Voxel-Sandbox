using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The structures of the source's kinds, each placed at a known seed and its blocks counted across the columns it crosses.</summary>
public sealed class StructureTests
{
    // Every column of a square about a place, each generated alone, and a block read across them.
    private sealed class Region(Overworld land, int middleX, int middleZ, int columns)
    {
        public readonly Dictionary<(int X, int Z), ChunkColumn> Columns = Enumerable.Range(0, columns * columns)
            .Select(i => ((middleX >> Section.Shift) - columns / 2 + i % columns, (middleZ >> Section.Shift) - columns / 2 + i / columns))
            .ToDictionary(c => c, c => land.Generate(c.Item1, c.Item2));

        public BlockId At(int x, int y, int z) =>
            Columns.TryGetValue((x >> Section.Shift, z >> Section.Shift), out var c) && ChunkColumn.Holds(y) ? c.Get(x & Section.Mask, y, z & Section.Mask) : BlockId.Air;

        public IEnumerable<(int X, int Y, int Z, BlockId Block)> Blocks(int y0, int y1) =>
            Columns.SelectMany(c => Enumerable.Range(0, Section.Size * Section.Size * (y1 - y0 + 1)).Select(i =>
            {
                int x = i % Section.Size, z = i / Section.Size % Section.Size, y = y0 + i / (Section.Size * Section.Size);
                return (c.Key.X * Section.Size + x, y, c.Key.Z * Section.Size + z, c.Value.Get(x, y, z));
            }));
    }

    [Fact]
    public void A_Mineshaft_Lays_Its_Corridors_Rails_Supports_Cobwebs_And_Chests_Whole_Across_The_Columns_It_Crosses()
    {
        var land = new Overworld(1);
        var mineshaft = Structures.Find("mineshaft")!;
        var start = mineshaft.StartsIn(-256, -256, 255, 255, land).OrderBy(s => Math.Abs(s.X + 80) + Math.Abs(s.Z + 80)).First();
        var region = new Region(land, start.X + 80, start.Z + 80, 8);
        var blocks = region.Blocks(start.Y - 45, start.Y + 30).ToList();
        int Count(params BlockId[] kinds) => blocks.Count(b => kinds.Contains(b.Block));

        // Seed 1's nearest, built from 71, 12, 36 under a forest, its supports of oak.
        Assert.Equal((71, 12, 36), (start.X, start.Y, start.Z));
        var (rails, cobwebs, chests, lanterns) = (Count(BlockId.Rail, BlockId.RailEast), Count(BlockId.Cobweb), Count(BlockId.Chest, BlockId.ChestEast, BlockId.ChestSouth, BlockId.ChestWest), Count(BlockId.HangingLantern));
        Assert.True(rails >= 20 && cobwebs >= 5 && chests >= 1 && lanterns >= 1, $"{rails} rails, {cobwebs} cobwebs, {chests} chests and {lanterns} lanterns");
        Assert.True(Count(BlockId.OakLog) >= 20, $"{Count(BlockId.OakLog)} posts of oak");

        // Each chest's loot is held by the column the chest stands in, at its place, and none joins
        // where a piece built after it cut its block away.
        Assert.All(region.Columns, c => Assert.All(c.Value.Chests, p => Assert.Equal(c.Key, (p.X >> Section.Shift, p.Z >> Section.Shift))));
        var joined = new Chests();
        foreach (var column in region.Columns.Values) joined.Joined(column);
        Assert.Equal(chests, joined.All.Count);
        Assert.All(joined.All.Keys, p => Assert.True(Chests.IsChest(region.At(p.X, p.Y, p.Z)), $"no chest at {p}"));

        // A corridor crossing a column's edge runs on whole in the column past it, its rails side by side.
        var across = blocks.Count(b => b.Block == BlockId.RailEast && (b.X & Section.Mask) == Section.Mask && region.At(b.X + 1, b.Y, b.Z) == BlockId.RailEast)
            + blocks.Count(b => b.Block == BlockId.Rail && (b.Z & Section.Mask) == Section.Mask && region.At(b.X, b.Y, b.Z + 1) == BlockId.Rail);
        Assert.True(across >= 1, "no rail runs on across a column's edge");
    }
}
