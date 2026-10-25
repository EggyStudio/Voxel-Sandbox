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

    // The nearest start of a structure to the origin on seed 1, and the blocks of the columns its
    // square and margin reach, each generated alone.
    private static (StructureStart Start, Region Region, Dictionary<BlockId, int> Counts) Nearest(string name)
    {
        var land = new Overworld(1);
        var structure = Structures.Find(name)!;
        var start = structure.StartsIn(-6144, -6144, 6143, 6143, land).OrderBy(s => Math.Abs(s.X) + Math.Abs(s.Z)).First();
        var middle = (X: start.X + structure.Size / 2, Z: start.Z + structure.Size / 2);
        var region = new Region(land, middle.X, middle.Z, 4);
        var counts = new Dictionary<BlockId, int>();
        for (int z = start.Z - 2; z < start.Z + structure.Size + 2; z++)
            for (int x = start.X - 2; x < start.X + structure.Size + 2; x++)
                for (int y = start.Y - 8; y <= start.Y + 24; y++)
                {
                    var block = region.At(x, y, z);
                    counts[block] = counts.GetValueOrDefault(block) + 1;
                }
        return (start, region, counts);
    }

    private static int Of(Dictionary<BlockId, int> counts, params BlockId[] kinds) => kinds.Sum(k => counts.GetValueOrDefault(k));

    private static readonly BlockId[] AnyChest = [BlockId.Chest, BlockId.ChestEast, BlockId.ChestSouth, BlockId.ChestWest];

    [Fact]
    public void The_Seas_Floor_Holds_Ruins_Of_Broken_Walls_And_Wrecked_Ships_With_Their_Chests()
    {
        var (ruin, _, stones) = Nearest("ruin");
        Assert.True(ruin.Y <= Overworld.SeaLevel - 6, $"a ruin stands at {ruin}");
        var walls = Of(stones, BlockId.StoneBricks, BlockId.MossyStoneBricks, BlockId.CrackedStoneBricks, BlockId.Sandstone);
        Assert.True(walls >= 40, $"the ruin at {ruin} has {walls} blocks of its walls and floor");
        Assert.True(Of(stones, AnyChest) >= 1, $"the ruin at {ruin} holds no chest");

        var (ship, _, hull) = Nearest("shipwreck");
        Assert.True(ship.Y <= Overworld.SeaLevel - 8, $"a wreck lies at {ship}");
        var planks = Of(hull, BlockId.OakPlanks, BlockId.SprucePlanks, BlockId.DarkOakPlanks);
        var mast = Of(hull, BlockId.OakLog, BlockId.SpruceLog, BlockId.DarkOakLog);
        Assert.True(planks >= 100 && mast >= 12, $"the wreck at {ship} has {planks} planks and {mast} logs");
        Assert.True(Of(hull, AnyChest) >= 1, $"the wreck at {ship} holds no chest");
    }

    [Fact]
    public void A_Jungle_Holds_A_Mossy_Temple_With_Two_Chests_In_Its_Vault()
    {
        var (temple, region, stones) = Nearest("temple");
        Assert.Equal(Biomes.Jungle, new Overworld(1).At(temple.X + 7, temple.Z + 7).Biome);
        var (cobble, mossy) = (Of(stones, BlockId.Cobblestone), Of(stones, BlockId.MossyCobblestone));
        Assert.True(cobble + mossy >= 400 && mossy > (cobble + mossy) / 4, $"the temple at {temple} has {cobble} cobblestone and {mossy} mossy");
        Assert.Equal(2, Of(stones, AnyChest));
        Assert.True(Of(stones, BlockId.CobblestoneStairs, BlockId.CobblestoneStairsEast, BlockId.CobblestoneStairsSouth, BlockId.CobblestoneStairsWest) >= 9);
    }

    [Fact]
    public void A_Swamp_Holds_A_Witchs_Hut_On_Posts_And_Open_Land_A_Pillagers_Tower_Far_From_Villages()
    {
        var (hut, _, wood) = Nearest("hut");
        Assert.True(Of(wood, BlockId.SprucePlanks) >= 40 && Of(wood, BlockId.OakLog) >= 12, $"the hut at {hut} has {Of(wood, BlockId.SprucePlanks)} planks and {Of(wood, BlockId.OakLog)} logs");
        Assert.Equal((1, 1), (Of(wood, BlockId.CraftingTable), Of(wood, AnyChest)));

        var (outpost, _, tower) = Nearest("outpost");
        Assert.True(Of(tower, BlockId.DarkOakPlanks) >= 150 && Of(tower, BlockId.BirchPlanks) >= 80 && Of(tower, BlockId.DarkOakLog) >= 40,
            $"the outpost at {outpost} has {Of(tower, BlockId.DarkOakPlanks)} dark oak planks, {Of(tower, BlockId.BirchPlanks)} birch and {Of(tower, BlockId.DarkOakLog)} logs");
        Assert.Equal(1, Of(tower, AnyChest));
        var land = new Overworld(1);
        Assert.Empty(Structures.Village.StartsIn(outpost.X - 160, outpost.Z - 160, outpost.X + 172, outpost.Z + 172, land));
    }
}
