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
    public void No_Two_Biomes_Meet_That_The_Source_Never_Puts_Side_By_Side()
    {
        // The badlands lie in the source's hot band where it is dry, its high ground badlands too, so
        // they meet the desert, the savanna, a river or a beach and nothing green, and nothing of the
        // hot band meets the snow, a band of temperature or two lying between them.
        var land = new Overworld(1);
        Biome[] hot = [Biomes.Desert, Biomes.Badlands, Biomes.Savanna, Biomes.Jungle], snowy = [Biomes.SnowyPlains, Biomes.SnowyTaiga];
        Biome[] besideBadlands = [Biomes.Badlands, Biomes.Desert, Biomes.Savanna, Biomes.River, Biomes.Beach];
        var seen = new HashSet<(Biome, Biome)>();
        const int step = 16;
        for (int z = -4096; z < 4096; z += step)
            for (int x = -4096; x < 4096; x += step)
            {
                var here = land.Ground(x, z).Biome;
                foreach (var there in new[] { land.Ground(x - step, z).Biome, land.Ground(x, z - step).Biome })
                    if (there != here) seen.Add((here, there));
            }
        Assert.Contains((Biomes.Badlands, Biomes.Desert), seen.Select(p => p.Item1 == Biomes.Badlands ? p : (p.Item2, p.Item1)));
        Assert.All(seen.Where(p => p.Item1 == Biomes.Badlands || p.Item2 == Biomes.Badlands),
            p => Assert.Contains(p.Item1 == Biomes.Badlands ? p.Item2 : p.Item1, besideBadlands));
        Assert.DoesNotContain(seen, p => hot.Contains(p.Item1) && snowy.Contains(p.Item2) || snowy.Contains(p.Item1) && hot.Contains(p.Item2));
    }

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

    [Fact]
    public void Each_Biomes_Middle_Takes_Its_Own_Grass_And_Leaves_As_The_Sources_Colormaps_Give_Them()
    {
        var land = new Overworld(1);
        // Places well inside each biome of seed 1, read from a map of its biomes 32 blocks apart.
        (int X, int Z, Biome Biome)[] middles =
        [
            (-288, 64, Biomes.Plains), (-1120, 320, Biomes.Forest), (96, -1120, Biomes.BirchForest), (-384, -96, Biomes.Taiga),
            (256, 1312, Biomes.SnowyTaiga), (416, 256, Biomes.SnowyPlains), (704, -704, Biomes.Desert), (352, -448, Biomes.Savanna),
            (-928, -1376, Biomes.Jungle), (-1024, 1248, Biomes.DarkForest), (1440, 608, Biomes.Badlands), (-736, -960, Biomes.CherryGrove),
        ];
        foreach (var (x, z, biome) in middles)
        {
            var column = land.Generate(x >> 4, z >> 4);
            var at = (z & 15) * 16 + (x & 15);
            Assert.Equal(biome.Id, column.Biome[at]);
            Near(biome.GrassColor, column.GrassTint[at], biome.Name);
            Near(biome.FoliageColor, column.FoliageTint[at], biome.Name);
        }
        // The plains' grass the source's own green, no blue in it to read as teal.
        Assert.Equal(new Color(145, 189, 89), Biomes.Plains.GrassColor);
    }

    private static void Near(Color expected, Color actual, string where) =>
        Assert.True(Math.Abs(expected.R - actual.R) <= 2 && Math.Abs(expected.G - actual.G) <= 2 && Math.Abs(expected.B - actual.B) <= 2,
            $"{where} reads {actual}, where {expected} is asked");
}
