using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The weather's clock over the days of a seed, its turns coming and going, and what falls on each biome.</summary>
public class WeatherTests
{
    [Fact]
    public void The_Weather_Turns_On_About_Two_Days_In_Five_The_Same_Each_Time_A_Seeds_Days_Come_Round()
    {
        var weather = new Weather { Seed = 1 };
        var turns = Enumerable.Range(0, 200).Select(weather.TurnOn).ToArray();
        var turned = turns.Count(t => t is not null);
        Assert.InRange(turned, 60, 100);
        Assert.All(turns.OfType<(float Start, float Hours)>(), t =>
        {
            Assert.InRange(t.Start, 0, 24);
            Assert.InRange(t.Hours, Weather.LeastHours, Weather.MostHours);
        });
        // The same seed gives the same days, another seed others.
        Assert.Equal(turns, Enumerable.Range(0, 200).Select(new Weather { Seed = 1 }.TurnOn));
        Assert.NotEqual(turns, Enumerable.Range(0, 200).Select(new Weather { Seed = 2 }.TurnOn));
    }

    [Fact]
    public void A_Turn_Comes_And_Goes_Over_Half_An_Hour_And_Runs_On_Past_Midnight()
    {
        var weather = new Weather { Seed = 1 };
        var day = Enumerable.Range(0, 400).First(d => weather.TurnOn(d) is { } t && t.Start + t.Hours > 24.5f && t.Start < 23);
        var (start, hours) = weather.TurnOn(day)!.Value;
        Assert.Equal(0, weather.Share(day, start - 0.1f), 3);
        Assert.Equal(0.5f, weather.Share(day, start + Weather.FadeHours / 2), 2);
        Assert.Equal(1, weather.Share(day, start + 1), 3);
        // Past midnight the next day still holds it, and it has gone once its hours are out.
        Assert.Equal(1, weather.Share(day + 1, start + hours - 24 - 1), 3);
        Assert.Equal(0, weather.Share(day + 1, start + hours - 24 + 0.1f), 3);
    }

    [Fact]
    public void Held_Weather_Stays_And_Weather_Turned_Off_Never_Turns()
    {
        var weather = new Weather { Seed = 1, Held = true };
        Assert.All(Enumerable.Range(0, 24), h => Assert.Equal(1, weather.Share(3, h)));
        weather.Held = false;
        Assert.All(Enumerable.Range(0, 24), h => Assert.Equal(0, weather.Share(3, h)));
        weather.Held = true;
        weather.Enabled = false;
        Assert.Equal(0, weather.Share(3, 12));
    }

    [Fact]
    public void Snow_Falls_On_The_Cold_Biomes_Rain_On_The_Wet_And_Nothing_On_The_Dry()
    {
        Assert.Equal(Precipitation.Snow, Biomes.SnowyPlains.Precipitation);
        Assert.Equal(Precipitation.Snow, Biomes.SnowyTaiga.Precipitation);
        Assert.Equal(Precipitation.None, Biomes.Desert.Precipitation);
        Assert.Equal(Precipitation.None, Biomes.Savanna.Precipitation);
        Assert.Equal(Precipitation.None, Biomes.Badlands.Precipitation);
        Assert.All(new[] { Biomes.Plains, Biomes.Forest, Biomes.Jungle, Biomes.Swamp, Biomes.Taiga }, b => Assert.Equal(Precipitation.Rain, b.Precipitation));
    }
}

/// <summary>The snow the weather lays on the tops and melts again.</summary>
public class SnowCoverTests
{
    [Fact]
    public void Snow_Settles_A_Layer_On_The_Tops_Of_A_Snowy_Biome_And_Melts_Once_The_Weather_Clears()
    {
        // Stone to 10 under snowy plains, but for the column at 6 under the plains, with a pool at 4 and leaves at 8.
        var world = Laid.World(c =>
        {
            Laid.Fill(c, 0, 10, BlockId.Stone);
            Array.Fill(c.Biome, Biomes.SnowyPlains.Id);
            c.Biome[6 * Section.Size + 6] = Biomes.Plains.Id;
            c.Set(4, 10, 4, BlockId.Water);
            c.Set(8, 11, 8, BlockId.OakLeaves);
        });
        Assert.True(SnowCover.Try(world, 2, 2, snowing: true));
        Assert.Equal(BlockId.SnowLayer, world.GetBlock(2, 11, 2));
        // A layer takes no second, nor does water, and a column of the plains none.
        Assert.False(SnowCover.Try(world, 2, 2, snowing: true));
        Assert.False(SnowCover.Try(world, 4, 4, snowing: true));
        Assert.False(SnowCover.Try(world, 6, 6, snowing: true));
        Assert.True(SnowCover.Try(world, 8, 8, snowing: true));
        Assert.Equal(BlockId.SnowLayer, world.GetBlock(8, 12, 8));
        // The layer is walked on, a block an eighth deep.
        Assert.Equal(0.125f, Assert.Single(Blocks.Get(BlockId.SnowLayer).Boxes).Max.Y, 4);

        // Once clear the layers melt, and a column with none is left as it is.
        Assert.True(SnowCover.Try(world, 2, 2, snowing: false));
        Assert.Equal(BlockId.Air, world.GetBlock(2, 11, 2));
        Assert.False(SnowCover.Try(world, 3, 3, snowing: false));

        // Tried at random about the eye for two minutes, the snow covers most of the tops near it, as
        // a column within its reach is tried about once in two minutes.
        var cover = new SnowCover();
        var random = new Random(1);
        for (int i = 0; i < 1200; i++) cover.Update(world, new System.Numerics.Vector3(8, 12, 8), snowing: true, clear: false, 1 / 10f, random);
        var covered = Enumerable.Range(0, 16).Sum(z => Enumerable.Range(0, 16).Count(x => world.GetBlock(x, 11, z) == BlockId.SnowLayer));
        Assert.True(covered > 128, $"{covered} of the 256 tops near the eye are covered");
        for (int i = 0; i < 2400; i++) cover.Update(world, new System.Numerics.Vector3(8, 12, 8), snowing: false, clear: true, 1 / 10f, random);
        Assert.True(Enumerable.Range(0, 16).Sum(z => Enumerable.Range(0, 16).Count(x => world.GetBlock(x, 11, z) == BlockId.SnowLayer)) < covered / 4);
    }
}
