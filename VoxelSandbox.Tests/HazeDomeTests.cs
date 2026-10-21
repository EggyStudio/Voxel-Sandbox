using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

public class HazeDomeTests
{
    [Fact]
    public void The_Dome_Lies_About_The_Eye_Below_The_Horizon_Facing_It_And_Fades_Into_The_Sky_Above()
    {
        var dome = HazeDome.Build();
        Assert.True(dome.Count > 0);
        for (int i = 0; i < dome.Count; i++)
        {
            Assert.Equal(HazeDome.Radius, dome.Positions[i].Length(), 1);
            var (_, _, _, tint) = dome.Unpack(i);
            var above = MathF.Asin(dome.Positions[i].Y / HazeDome.Radius) * 180 / MathF.PI;
            // The whole of the haze from the horizon down, none at the top of its band six degrees up.
            if (above <= 0.01f) Assert.Equal(255, tint.A);
            if (above >= 5.99f) Assert.Equal(0, tint.A);
        }
        // Every triangle's front is toward the eye at the middle.
        for (int t = 0; t < dome.Indices.Count; t += 3)
        {
            Vector3 a = dome.Positions[(int)dome.Indices[t]], b = dome.Positions[(int)dome.Indices[t + 1]], c = dome.Positions[(int)dome.Indices[t + 2]];
            Assert.True(Vector3.Dot(Vector3.Cross(b - a, c - a), -(a + b + c)) > 0, $"triangle {t / 3} faces away from the eye");
        }
        Assert.True(HazeDome.Radius > 16 * 16 * 1.5f, "the dome lies within the farthest render distance");
    }
}
