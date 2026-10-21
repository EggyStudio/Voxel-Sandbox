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
    public void A_Quad_On_The_Sky_Keeps_Its_Sides_There_As_The_Sky_Turns_It()
    {
        var ways = new[] { Sky.SunToward(6), -Sky.SunToward(6), Vector3.Normalize(new Vector3(0.2f, 0.7f, -0.6f)) };
        foreach (var way in ways)
        {
            var (right, up) = Sky.Across(way);
            for (float hour = 0; hour < 24; hour += 1.5f)
            {
                var turned = Sky.Turned(hour, way);
                var (turnedRight, turnedUp) = Sky.Across(turned);
                // The sides at the turned place are the sides turned, so the sky carries a quad
                // whole and the camera has no part in which way its sides run.
                Near(Sky.Turned(hour, right), turnedRight);
                Near(Sky.Turned(hour, up), turnedUp);
                // It lies across the way to it, square, its right side along the way the sky turns
                // there, and its front toward the middle of the sky.
                Assert.Equal(0, Vector3.Dot(turnedRight, turned), 4);
                Assert.Equal(0, Vector3.Dot(turnedUp, turned), 4);
                Assert.Equal(1, turnedUp.Length(), 4);
                Assert.Equal(0, turnedRight.Z, 4);
                Near(Vector3.Cross(turned, turnedUp), turnedRight);
            }
        }
        // The moon at midnight, high in the north, stands upright to a player facing it, east on
        // the right.
        var (moonRight, moonUp) = Sky.Across(-Sky.SunToward(0));
        Assert.True(moonRight.X > 0.99f && moonUp.Y > 0.2f, $"right {moonRight}, up {moonUp}");
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
        // The full moon's middle is lit and the new moon's is clear, its glow too.
        const int middle = 1 + SkyArt.Texels / 2;
        Assert.Equal(255, sheet[(middle * across + middle) * 4 + 3]);
        Assert.Equal(0, sheet[((SkyArt.Cell + middle) * across + middle) * 4 + 3]);
        Assert.Equal(0, sheet[((SkyArt.Cell + middle) * across + middle - SkyArt.Size * SkyArt.Fine) * 4 + 3]);
    }

    [Fact]
    public void The_Sun_Is_A_Square_Of_Eight_Pixels_At_The_Middle_Of_Its_Picture_With_A_Smooth_Glow_Clear_Before_Its_Edge()
    {
        var sun = SkyArt.SunPixels();
        int Alpha(int x, int y) => sun[(y * SkyArt.Texels + x) * 4 + 3];
        const int from = (SkyArt.Texels - SkyArt.Size * SkyArt.Fine) / 2, middle = SkyArt.Texels / 2;

        // The body is whole, square and opaque, its corners too, each of its pixels a block of
        // texels of one color, and the glow beside it far dimmer.
        for (int y = from; y < from + SkyArt.Size * SkyArt.Fine; y++)
            for (int x = from; x < from + SkyArt.Size * SkyArt.Fine; x++)
            {
                Assert.Equal(255, Alpha(x, y));
                int px = from + (x - from) / SkyArt.Fine * SkyArt.Fine, py = from + (y - from) / SkyArt.Fine * SkyArt.Fine;
                Assert.Equal(sun[(py * SkyArt.Texels + px) * 4], sun[(y * SkyArt.Texels + x) * 4]);
            }
        Assert.InRange(Alpha(from - 1, middle), 1, (int)(255 * SkyArt.GlowPeak) + 1);

        // Outward from the body the glow never brightens and falls a level or two a texel at most,
        // with no steps a dithering would leave, and it is gone at the picture's edge.
        for (int x = from - 1; x > 0; x--)
        {
            Assert.True(Alpha(x - 1, middle) <= Alpha(x, middle), $"the glow rises at {x - 1}");
            Assert.True(Alpha(x, middle) - Alpha(x - 1, middle) <= 2, $"the glow steps at {x - 1}");
        }
        Assert.True(Alpha(from - 4 * SkyArt.Fine, middle) < Alpha(from - 1, middle));
        for (int i = 0; i < SkyArt.Texels; i++)
        {
            Assert.Equal(0, Alpha(i, 0));
            Assert.Equal(0, Alpha(0, i));
            Assert.Equal(0, Alpha(i, SkyArt.Texels - 1));
            Assert.Equal(0, Alpha(SkyArt.Texels - 1, i));
        }
    }

    [Fact]
    public void A_Low_Suns_Glow_Dims_Through_The_Haze_Domes_Band_And_Is_Not_Cut_At_Its_Top()
    {
        // The hour the sun's middle stands three degrees up, halfway through the dome's band.
        var hour = 6f;
        while (float.RadiansToDegrees(MathF.Asin(Sky.SunToward(hour).Y)) < 3) hour += 0.001f;
        var turn = Matrix4x4.CreateRotationZ(float.DegreesToRadians((hour - 6) * 15));
        var corners = Sky.Grid(Sky.SunToward(6), Sky.Distance * 0.15f / SkyArt.Size);
        const int side = SkyArt.Across + 1;

        int partly = 0;
        var heights = new List<(float Degrees, float Clear)>();
        for (int column = 0; column < side; column++)
            for (int row = 0; row < side; row++)
            {
                var way = Vector3.Transform(corners[row * side + column], turn);
                var degrees = float.RadiansToDegrees(MathF.Asin(way.Y / way.Length()));
                var clear = Sky.Clear(way);
                // Whole above the band, gone under the horizon and dimming between, so no line
                // crosses the glow where the band begins.
                if (degrees >= 6) Assert.Equal(1, clear, 3);
                else if (degrees <= 0) Assert.Equal(0, clear, 3);
                else if (degrees > 0.1f && degrees < 5.9f)
                {
                    Assert.True(clear > 0 && clear < 1, $"{clear} of the glow shows at {degrees} degrees");
                    partly++;
                }
                heights.Add((degrees, clear));
            }
        Assert.True(partly > side, $"{partly} corners lie in the band");
        // Higher on the sky, never less of it shows.
        var ordered = heights.OrderBy(h => h.Degrees).ToList();
        for (int i = 1; i < ordered.Count; i++) Assert.True(ordered[i].Clear >= ordered[i - 1].Clear - 1e-4f);
    }

    [Fact]
    public void The_Haze_Dome_Lies_Beyond_The_Sun_The_Moon_And_The_Stars()
    {
        Assert.True(HazeDome.Radius > Sky.StarDistance && Sky.StarDistance > Sky.Distance);
        // The sun's picture's farthest corner lies short of the dome too.
        var corners = Sky.Grid(Sky.SunToward(6), Sky.Distance * 0.15f / SkyArt.Size);
        Assert.True(corners.Max(c => c.Length()) < HazeDome.Radius);
    }

    private static (float, int) Rounded((float Hour, int Day) at) => (MathF.Round(at.Hour, 3), at.Day);

    private static void Near(Vector3 expected, Vector3 actual) =>
        Assert.True(Vector3.Distance(expected, actual) < 1e-4f, $"expected {expected}, got {actual}");
}
