using System.Numerics;
using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

public class OptionTests
{
    [Fact]
    public void A_Slider_Holds_Within_Its_Ends_And_Rounds_To_Its_Step()
    {
        float bloom = 0.1f;
        var sets = 0;
        var option = new Option("Bloom", () => bloom, v => (bloom, sets) = (v, sets + 1)) { High = 2, Step = 0.05f };
        Assert.Equal("Bloom: 0.1", option.Label);
        option.Change(3);
        Assert.Equal(2, bloom);
        option.Change(-1);
        Assert.Equal(0, bloom);
        option.Change(0.123f);
        Assert.Equal(0.1f, bloom, 4);
        // A value that rounds to the one it holds sets nothing.
        option.Change(0.11f);
        Assert.Equal(3, sets);
    }

    [Fact]
    public void A_Button_Steps_Through_Its_Choices_From_The_Last_To_The_First_And_Settles_Each()
    {
        var quality = GlobalIllumination.Medium;
        var settled = 0;
        var gi = Option.Choice("Global Illumination", () => quality, q => quality = q);
        Assert.Equal("Global Illumination: Medium", gi.Label);
        gi.Next();
        Assert.Equal(GlobalIllumination.High, quality);
        gi.Next();
        Assert.Equal(GlobalIllumination.Off, quality);

        var on = false;
        var probes = new Option("Screen Probes", () => on ? 1 : 0, v => on = v >= 0.5f) { Choices = ["Off", "On"], Settled = () => settled++ };
        probes.Next();
        Assert.True(on);
        Assert.Equal("Screen Probes: On", probes.Label);
        probes.Next();
        Assert.False(on);
        Assert.Equal(2, settled);
    }

    [Fact]
    public void Each_Of_The_Sky_Colors_Screens_Sets_Its_Color_And_The_Sky_Takes_It()
    {
        var colors = new SkyColors();
        var screens = OptionScreens.SkyColorsOf(colors);
        Assert.Equal(9, screens.Count);
        foreach (var color in screens)
        {
            var channels = color.Channels(colors);
            for (int i = 0; i < 3; i++)
            {
                var version = colors.Version;
                channels[i].Change(0);
                Assert.Equal(0, color.Get()[i], 4);
                Assert.Equal(0, channels[i].Value, 4);
                channels[i].Change(255);
                // A light reaches the largest part of the color it starts at, a sky's color 255.
                Assert.Equal(color.Scale > 0 ? color.Scale : 255, color.Get()[i], 3);
                Assert.Equal(255, channels[i].Value, 3);
                Assert.True(colors.Version > version, $"{color.Name} {channels[i].Name} counts no change");
            }
        }

        // The noon light set without red lights the land with none.
        colors.Reset();
        Assert.Equal(1.33f, colors.SunLight(1).X, 3);
        screens[0].Channels(colors)[0].Change(0);
        Assert.Equal(0, colors.SunLight(1).X, 4);
        Assert.Equal(1.045f, colors.SunLight(1).Y, 3);
        colors.Reset();
        Assert.Equal(SkyColors.DefaultDayZenith, colors.DayZenith);
    }

    [Fact]
    public void A_Light_Shown_At_The_Levels_It_Starts_At_Keeps_Its_Color()
    {
        var colors = new SkyColors();
        foreach (var color in OptionScreens.SkyColorsOf(colors))
        {
            var start = color.Get();
            var channels = color.Channels(colors);
            // Each channel set to the level it shows, as a slider let go where it was pressed sets it.
            for (int i = 0; i < 3; i++) channels[i].Change(MathF.Round(channels[i].Value));
            var now = color.Get();
            Assert.True(Vector3.Distance(start, now) < 0.01f * MathF.Max(1, start.Length()), $"{color.Name} went from {start} to {now}");
        }
    }
}
