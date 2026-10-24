using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The places the world's small moving things are due at a rate, and the pictures they are drawn with.</summary>
public class ParticleTests
{
    [Fact]
    public void A_Rate_Spread_Over_Short_Frames_Comes_Whole_And_A_Long_Frame_Bursts_No_More_Than_The_Emitters_Show()
    {
        var carry = 0f;
        var due = 0;
        for (int frame = 0; frame < 60; frame++) due += AmbientParticles.Due(ref carry, AmbientParticles.Splashes, 1 / 60f, 3);
        Assert.InRange(due, 59, 60);
        // At sixty a second a frame of a second has sixty due, of which the three emitters show three,
        // and what piles up past them is let go.
        carry = 0;
        Assert.Equal(3, AmbientParticles.Due(ref carry, AmbientParticles.Splashes, 1, 3));
        Assert.InRange(carry, 0, 3);
        carry = 0;
        Assert.Equal(0, AmbientParticles.Due(ref carry, 0, 1, 3));
    }

    [Fact]
    public void A_Ring_Spreads_And_Fades_Over_Its_Frames()
    {
        var (pixels, width, height) = AmbientParticles.Rings();
        Assert.Equal((16 * AmbientParticles.RingFrames, 16), (width, height));
        // The frames' widths across the middle row, and their strongest alpha.
        var spans = new List<int>();
        var alphas = new List<int>();
        for (int frame = 0; frame < AmbientParticles.RingFrames; frame++)
        {
            var lit = Enumerable.Range(0, 16).Where(x => pixels[(7 * width + frame * 16 + x) * 4 + 3] > 0).ToList();
            Assert.NotEmpty(lit);
            spans.Add(lit.Max() - lit.Min());
            alphas.Add(Enumerable.Range(0, 16 * 16).Max(i => pixels[((i / 16) * width + frame * 16 + i % 16) * 4 + 3]));
        }
        Assert.Equal(spans.Order(), spans);
        Assert.True(spans[^1] > spans[0] + 8, string.Join(", ", spans));
        Assert.Equal(alphas.OrderDescending(), alphas);
    }

    [Fact]
    public void A_Falling_Leaf_Turns_Through_Its_Frames_The_Same_Each_Half_Turn()
    {
        var (pixels, width, height) = AmbientParticles.LeafFrames();
        Assert.Equal(8, height);
        int frames = width / 8;
        Assert.Equal(16, frames);
        // Each frame's shape, its alpha, the shade of its middle left out, which a hair's turn moves.
        byte[] Frame(int frame) => [.. Enumerable.Range(0, 64).Select(i => pixels[((i / 8) * width + frame * 8 + i % 8) * 4 + 3])];
        Assert.All(Enumerable.Range(0, frames), f => Assert.Contains(Frame(f), a => a == 255));
        // Four frames are half a turn, which shows the leaf as it was, and two a quarter, which does not.
        Assert.Equal(Frame(0), Frame(4));
        Assert.NotEqual(Frame(0), Frame(2));
    }
}
