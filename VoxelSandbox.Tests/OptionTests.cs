using System.Numerics;
using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

public sealed class OptionTests : IDisposable
{
    // A folder of this test's own, removed with it, so no run reads another's options.
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, recursive: true);
    }

    // Settings over fields of their own, a slider, a button and one the file leaves out, as a screen holds them.
    private sealed class Held
    {
        public float Distance = 12, Hour = 10;
        public GlobalIllumination Quality = GlobalIllumination.High;

        public List<(string Name, IReadOnlyList<Option> Options)> Screens() =>
        [
            ("render", [
                new Option("Render Distance", () => Distance, v => Distance = v) { Low = 2, High = 16, Step = 1 },
                Option.Choice("Global Illumination", () => Quality, q => Quality = q),
            ]),
            ("sky", [new Option("Time", () => Hour, v => Hour = v) { High = 24, Kept = false }]),
        ];
    }

    [Fact]
    public void The_Options_File_Keeps_Each_Setting_A_Line_And_A_New_Run_Reads_Them()
    {
        var file = new OptionsFile(_folder);
        var before = new Held { Distance = 6, Quality = GlobalIllumination.Medium, Hour = 3 };
        Assert.True(file.Write(before.Screens()));
        Assert.Equal([$"version:{OptionsFile.Version}", "render.Render_Distance:6", "render.Global_Illumination:Medium"], File.ReadAllLines(file.File));
        // Nothing changed, nothing written.
        Assert.False(file.Write(before.Screens()));

        var after = new Held();
        Assert.Equal(2, new OptionsFile(_folder).Read(after.Screens()));
        Assert.Equal(6, after.Distance);
        Assert.Equal(GlobalIllumination.Medium, after.Quality);
        // The hour is the world's, which the file leaves alone.
        Assert.Equal(10, after.Hour);
    }

    [Fact]
    public void A_Line_The_Options_File_Cannot_Read_Leaves_Its_Setting_At_Its_Default()
    {
        Directory.CreateDirectory(_folder);
        var file = new OptionsFile(_folder);
        foreach (var lines in new[]
        {
            "render.Render_Distance:far\nrender.Global_Illumination:Low",
            "render.Render_Distance:40\nrender.Global_Illumination:Low",
            "render.Render_Distance:NaN\nrender.Global_Illumination:Low",
            "render.Render_Distanc:6\nrender.Global_Illumination:Low",
            "render.Render_Distance 6\nrender.Global_Illumination:Low",
            "render.Render_Distance:8\nrender.Global_Illumination:Brightest",
            "sky.Time:5\nrender.Global_Illumination:Low",
        })
        {
            File.WriteAllText(file.File, lines);
            var held = new Held();
            new OptionsFile(_folder).Read(held.Screens());
            Assert.Equal(lines.Contains("Distance:8") ? 8 : 12, held.Distance);
            Assert.Equal(lines.Contains("Brightest") ? GlobalIllumination.High : GlobalIllumination.Low, held.Quality);
            Assert.Equal(10, held.Hour);
        }
    }

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
        var rim = new Option("Rim Light", () => on ? 1 : 0, v => on = v >= 0.5f) { Choices = ["Off", "On"], Settled = () => settled++ };
        rim.Next();
        Assert.True(on);
        Assert.Equal("Rim Light: On", rim.Label);
        rim.Next();
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
