using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The haze over the land by distance, height and hour, and the sun's color through the day.</summary>
public class HazeTests
{
    // The haze's most and growth as the game sets them at the render distance of twelve columns.
    private const float Most = (0.95f - 0.25f) * 0.325f, Growth = 0.5f;

    [Fact]
    public void The_Haze_Grows_With_Distance_And_Thins_Over_Land_High_Above_The_Sea()
    {
        const float sea = Haze.Base, eye = Haze.Base + 20;
        // Nearer is clearer, at the sea's level and over a peak alike.
        Assert.True(Haze.Aerial(40, eye, sea, Most, Growth) < Haze.Aerial(120, eye, sea, Most, Growth));
        Assert.True(Haze.Aerial(40, eye, sea + 80, Most, Growth) < Haze.Aerial(120, eye, sea + 80, Most, Growth));
        // At and under the sea's level the density is whole, as the haze by distance alone.
        Assert.Equal(1, Haze.HeightShare(sea, sea), 4);
        Assert.Equal(1, Haze.HeightShare(sea - 30, sea - 10), 4);
        Assert.Equal(Haze.Aerial(150, sea, sea, Most, Growth, byHeight: false), Haze.Aerial(150, sea, sea, Most, Growth), 4);
        // A peak eighty blocks over the sea, seen from the sea's level, holds about two fifths of it.
        Assert.InRange(Haze.HeightShare(sea, sea + 80), 0.38f, 0.48f);
        // The same distance over higher land is clearer, and seen from higher up clearer still.
        float valley = Haze.Aerial(150, eye, sea, Most, Growth), peak = Haze.Aerial(150, eye, sea + 80, Most, Growth);
        Assert.True(peak < valley, $"{peak} over the peak against {valley} over the valley");
        Assert.True(Haze.HeightShare(sea + 60, sea + 80) < Haze.HeightShare(sea, sea + 80));
        // It never passes its most.
        Assert.True(Haze.Aerial(10_000, sea, sea, Most, Growth) <= Most + 1e-6f);
    }

    [Fact]
    public void The_Haze_Thickens_About_Sunrise_And_Is_As_At_Noon_By_Evening()
    {
        Assert.Equal(2.5f, Haze.Dawn(6.25f), 3);
        Assert.InRange(Haze.Dawn(9), 1, 1.05f);
        Assert.InRange(Haze.Dawn(12), 1, 1.0001f);
        Assert.InRange(Haze.Dawn(17.5f), 1, 1.0001f);
        Assert.InRange(Haze.Dawn(3.5f), 1, 1.05f);
        Assert.True(Haze.Aerial(100, Haze.Base, Haze.Base, Most, Growth * Haze.Dawn(6.25f)) > Haze.Aerial(100, Haze.Base, Haze.Base, Most, Growth));
    }

    [Fact]
    public void The_Sun_Is_Warm_White_At_Noon_And_Deepens_To_Orange_As_It_Sinks()
    {
        var colors = new SkyColors();
        // The sun's light at four hours, its blue's share of its red falling as the sun sinks.
        float[] hours = [12, 9, 7, 6.2f];
        var blue = hours.Select(h => colors.SunLight(Sky.SunToward(h).Y)).Select(l => l.Z / l.X).ToArray();
        for (int i = 1; i < blue.Length; i++)
            Assert.True(blue[i] < blue[i - 1], $"the sun at {hours[i]} has {blue[i]} blue to red, against {blue[i - 1]} at {hours[i - 1]}");
        // Noon's a warm white, its blue more than half its red, and near sunrise an orange.
        Assert.InRange(blue[0], 0.5f, 0.7f);
        Assert.True(blue[^1] < 0.4f, $"the sun at 6.2 has {blue[^1]} blue to red");
        // The evening mirrors the morning.
        var evening = colors.SunLight(Sky.SunToward(17).Y);
        var morning = colors.SunLight(Sky.SunToward(7).Y);
        Assert.Equal(morning.Z / morning.X, evening.Z / evening.X, 3);
    }
}
