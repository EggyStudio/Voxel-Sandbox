using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

public class BiomeTests
{
    // The places of seed 1 within so many blocks of the origin, a step apart, nearest first.
    private static IEnumerable<(int X, int Z)> Around(int reach, int step)
    {
        for (int ring = 0; ring * step <= reach; ring++)
            for (int z = -ring; z <= ring; z++)
                for (int x = -ring; x <= ring; x++)
                    if (Math.Max(Math.Abs(x), Math.Abs(z)) == ring) yield return (x * step, z * step);
    }

    [Theory]
    [InlineData(0.8f, 0.1f, "badlands")]
    [InlineData(0.8f, 0.35f, "desert")]
    [InlineData(0.66f, 0.3f, "savanna")]
    [InlineData(0.8f, 0.8f, "jungle")]
    [InlineData(0.6f, 0.85f, "swamp")]
    [InlineData(0.45f, 0.85f, "dark forest")]
    [InlineData(0.5f, 0.66f, "forest")]
    [InlineData(0.5f, 0.55f, "birch forest")]
    [InlineData(0.5f, 0.3f, "plains")]
    [InlineData(0.2f, 0.5f, "taiga")]
    public void The_Climate_Chooses_Each_Biome_Where_The_Source_Puts_It(float temperature, float humidity, string biome) =>
        Assert.Equal(biome, Climate.Choose(temperature, humidity, 60, 82).Name);

    [Fact]
    public void A_Cherry_Grove_Stands_On_Raised_Ground_Where_Low_Ground_Of_Its_Climate_Is_Forest() =>
        Assert.Equal(("cherry grove", "birch forest"), (Climate.Choose(0.44f, 0.6f, 72, 82).Name, Climate.Choose(0.44f, 0.6f, 58, 82).Name));

    [Fact]
    public void Each_Of_The_Eight_Lies_Within_Three_Thousand_Blocks_Of_The_Origin()
    {
        var land = new Overworld(1);
        var missing = new HashSet<Biome>([Biomes.Swamp, Biomes.Savanna, Biomes.Jungle, Biomes.DarkForest, Biomes.Badlands, Biomes.Beach, Biomes.River, Biomes.CherryGrove]);
        foreach (var (x, z) in Around(3000, 32))
        {
            missing.Remove(land.At(x, z).Biome);
            if (missing.Count == 0) break;
        }
        Assert.Empty(missing.Select(b => b.Name));
    }

    [Fact]
    public void A_River_Runs_Under_The_Sea_Level_With_Water_Over_Its_Bed()
    {
        var land = new Overworld(1);
        var (x, z) = Around(2000, 8).First(p => land.At(p.X, p.Z).Biome == Biomes.River);
        var height = land.At(x, z).Height;
        Assert.InRange(height, Overworld.SeaLevel - 4, Overworld.SeaLevel - 1);

        var column = land.Generate(x >> 4, z >> 4);
        Assert.Contains(column.Get(x & 15, Overworld.SeaLevel, z & 15), new[] { BlockId.Water, BlockId.Ice });
        Assert.Contains(column.Get(x & 15, height, z & 15), new[] { BlockId.Sand, BlockId.Gravel, BlockId.Clay });
    }

    [Fact]
    public void A_Mesa_Shows_The_Bands_Of_Its_Terracotta_Down_Its_Side()
    {
        var land = new Overworld(1);
        var (x, z) = Around(3000, 16).First(p => land.At(p.X, p.Z) is { Biome: var b, Height: > Overworld.SeaLevel + 20 } && b == Biomes.Badlands);
        var height = land.At(x, z).Height;
        var column = land.Generate(x >> 4, z >> 4);

        BlockId[] terracotta = [BlockId.Terracotta, BlockId.WhiteTerracotta, BlockId.OrangeTerracotta, BlockId.YellowTerracotta, BlockId.RedTerracotta, BlockId.BrownTerracotta, BlockId.LightGrayTerracotta];
        var side = Enumerable.Range(Overworld.SeaLevel, height - Overworld.SeaLevel + 1).Select(y => column.Get(x & 15, y, z & 15)).Where(b => b != BlockId.Air).ToList();
        Assert.All(side, b => Assert.Contains(b, terracotta));
        Assert.True(side.Distinct().Count() >= 3, $"the mesa's side shows {string.Join(", ", side.Distinct())}");
    }

    [Fact]
    public void A_Swamp_Lies_About_The_Sea_Level_In_Its_Own_Colors()
    {
        var land = new Overworld(1);
        // A place well inside a swamp, where the swamps of every place around it lie too.
        var (x, z) = Around(3000, 32).First(p => Enumerable.Range(-2, 5).All(d => land.At(p.X + d * 12, p.Z).Biome == Biomes.Swamp && land.At(p.X, p.Z + d * 12).Biome == Biomes.Swamp));
        var heights = Enumerable.Range(-8, 17).Select(d => land.At(x + d, z).Height).ToList();
        Assert.True(heights.Count(h => Math.Abs(h - Overworld.SeaLevel) <= 3) >= 14, $"the swamp's ground stands at {string.Join(", ", heights)}");

        var column = land.Generate(x >> 4, z >> 4);
        Assert.Equal(Biomes.Swamp.Id, column.Biome[(z & 15) * 16 + (x & 15)]);
        Assert.Contains(column.GrassTint[(z & 15) * 16 + (x & 15)], new[] { new Color(76, 118, 60), new Color(106, 112, 57) });
    }
}
