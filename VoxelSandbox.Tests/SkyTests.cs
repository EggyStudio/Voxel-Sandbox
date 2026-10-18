using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

public class SkyTests
{
    [Fact]
    public void The_Day_Goes_On_At_Midnight_And_The_Hour_Starts_Again()
    {
        Assert.Equal((0.5f, 4), Rounded(Sky.Advance(23.5f, 3, 1)));
        Assert.Equal((12f, 3), Rounded(Sky.Advance(10, 3, 2)));
        // A long step, as a slow frame with a short day makes, passes more than one midnight.
        Assert.Equal((1f, 5), Rounded(Sky.Advance(23, 3, 26)));
    }

    [Fact]
    public void The_Moon_Is_Full_On_The_First_Night_Goes_Through_Eight_Phases_And_Changes_While_It_Is_Down()
    {
        Assert.Equal(0, Sky.MoonPhase(0, 20));
        // The hours before noon belong to the night that began the evening before.
        Assert.Equal(0, Sky.MoonPhase(1, 3));
        Assert.Equal(1, Sky.MoonPhase(1, 20));
        Assert.Equal(4, Sky.MoonPhase(4, 23));
        Assert.Equal(7, Sky.MoonPhase(8, 5));
        Assert.Equal(0, Sky.MoonPhase(8, 20));
    }

    [Fact]
    public void The_Moon_Is_Opposite_The_Sun_And_The_Stars_Keep_Their_Places_Against_Them()
    {
        var star = Vector3.Normalize(new Vector3(0.2f, 0.7f, -0.6f));
        var atSix = Vector3.Dot(Sky.SunToward(6), star);
        for (float hour = 0; hour < 24; hour += 1.5f)
        {
            Assert.Equal(atSix, Vector3.Dot(Sky.SunToward(hour), Sky.Turned(hour, star)), 4);
            Assert.Equal(1, Sky.Turned(hour, star).Length(), 4);
        }
        // The sun stands in the east at 6, over the south at noon, and in the west at 18.
        Assert.True(Sky.SunToward(6).X > 0.9f && MathF.Abs(Sky.SunToward(6).Y) < 1e-4f);
        Assert.True(Sky.SunToward(12).Y > 0.9f && Sky.SunToward(12).Z > 0);
        Assert.True(Sky.SunToward(18).X < -0.9f);
    }

    [Fact]
    public void The_Moon_Is_Lit_On_The_Left_As_It_Wanes_And_On_The_Right_As_It_Waxes_And_Dark_When_New()
    {
        var lit = new int[SkyArt.Phases];
        int left = 0, right = 0;
        for (int phase = 0; phase < SkyArt.Phases; phase++)
            for (int y = 0; y < SkyArt.Size; y++)
                for (int x = 0; x < SkyArt.Size; x++)
                    if (SkyArt.Lit(phase, x, y))
                    {
                        lit[phase]++;
                        if (phase == 2) left += x < SkyArt.Size / 2 ? 1 : -1;
                        if (phase == 6) right += x >= SkyArt.Size / 2 ? 1 : -1;
                    }

        Assert.Equal(SkyArt.Size * SkyArt.Size, lit[0]);
        Assert.Equal(0, lit[4]);
        // Waning and waxing mirror each other, each phase less lit on the way to the new moon.
        for (int phase = 1; phase < 4; phase++)
        {
            Assert.Equal(lit[phase], lit[SkyArt.Phases - phase]);
            Assert.True(lit[phase] < lit[phase - 1]);
        }
        Assert.Equal(SkyArt.Size * SkyArt.Size / 2, lit[2]);
        // At the last quarter the left half alone is lit, and at the first the right half.
        Assert.Equal(lit[2], left);
        Assert.Equal(lit[6], right);
    }

    [Fact]
    public void Each_Phase_Of_The_Moon_Sits_In_Its_Cell_With_A_Clear_Edge()
    {
        var sheet = SkyArt.MoonPixels();
        const int across = SkyArt.Cell * 4;
        for (int y = 0; y < SkyArt.Cell * 2; y++)
            for (int x = 0; x < across; x++)
            {
                var edge = x % SkyArt.Cell is 0 or SkyArt.Cell - 1 || y % SkyArt.Cell is 0 or SkyArt.Cell - 1;
                if (edge) Assert.Equal(0, sheet[(y * across + x) * 4 + 3]);
            }
        // The full moon's middle is lit and the new moon's is clear.
        Assert.Equal(255, sheet[((1 + 6) * across + 1 + 6) * 4 + 3]);
        Assert.Equal(0, sheet[((SkyArt.Cell + 1 + 6) * across + 1 + 6) * 4 + 3]);
    }

    [Fact]
    public void The_Glow_Around_The_Sun_Falls_In_Steps_On_The_Suns_Grid_And_Clears_Before_Its_Edge()
    {
        var glow = SkyArt.HaloPixels(0.75f);
        const int middle = SkyArt.HaloSize / 2;
        int Alpha(int x, int y) => glow[(y * SkyArt.HaloSize + x) * 4 + 3];

        // The sun's square and the glow's share their middle and their grid of pixels.
        Assert.Equal(0, (SkyArt.HaloSize - SkyArt.Size) % 2);
        Assert.Equal(255, Alpha(middle, middle));
        var levels = new HashSet<int>();
        for (int x = middle; x < SkyArt.HaloSize; x++)
        {
            levels.Add(Alpha(x, middle));
            if (x + 1 < SkyArt.HaloSize) Assert.True(Alpha(x + 1, middle) <= Alpha(x, middle));
        }
        // A few rings, each a step of brightness, rather than a smooth fall.
        Assert.InRange(levels.Count, 4, SkyArt.HaloSteps + 1);
        for (int i = 0; i < SkyArt.HaloSize; i++)
        {
            Assert.Equal(0, Alpha(i, 0));
            Assert.Equal(0, Alpha(0, i));
            Assert.Equal(0, Alpha(i, SkyArt.HaloSize - 1));
            Assert.Equal(0, Alpha(SkyArt.HaloSize - 1, i));
        }
    }

    private static (float, int) Rounded((float Hour, int Day) at) => (MathF.Round(at.Hour, 3), at.Day);
}
