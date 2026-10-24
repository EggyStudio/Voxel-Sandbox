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
