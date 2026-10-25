using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The noises in fixed point, which a shader reckons to the bit as the CPU does, held to the float formulas they stand for.</summary>
public sealed class FixedNoiseTests
{
    [Fact]
    public void A_Product_Of_Sixteen_Fractional_Bits_Is_Taken_Down_To_The_Floor_And_The_Fade_Keeps_Its_Ends_And_Middle()
    {
        const int One = FixedNoise.One;
        Assert.Equal(One / 4, FixedNoise.Mul(One / 2, One / 2));
        Assert.Equal(-1, FixedNoise.Mul(-1, 1));
        Assert.Equal(-3 * One, FixedNoise.Mul(-2 * One, One + One / 2));
        Assert.Equal(0, FixedNoise.Smooth(0));
        Assert.Equal(One / 2, FixedNoise.Smooth(One / 2));
        Assert.Equal(One, FixedNoise.Smooth(One));
        // The noise is nothing at the corners of its cells, its whole bits wrapping at 256.
        var permutation = Enumerable.Range(0, 256).Select(i => (byte)(255 - i)).ToArray();
        Assert.Equal(0, FixedNoise.Improved(permutation, 5u << 24, 200u << 24, 255u << 24));
    }

    [Fact]
    public void The_Source_Noise_Reads_As_Its_Formula_In_Doubles_But_For_Rounding_And_Repeats_Its_Period()
    {
        var noise = new SourceNoise(7, "cave_cheese", -8, 0.5, 1, 2, 1, 2, 1, 0, 2, 0);
        var sampler = noise.Scaled(1, 0.6666666666666666, 1);
        var random = new Random(3);
        var worst = 0.0;
        for (int i = 0; i < 2000; i++)
        {
            int x = random.Next(-3000, 3000), y = random.Next(-64, 320), z = random.Next(-3000, 3000);
            worst = Math.Max(worst, Math.Abs(sampler.At(x, y, z) - Reference(sampler, x, y, z)));
        }
        Assert.True(worst < 2e-3, $"the fixed point strays {worst} from the doubles");
        // The first sum's level, read at a step of a power of two, repeats where its coordinate's 24
        // fractional bits and 8 whole ones overflow, a period of the noise's 256 cells.
        var level = new SourceNoise(7, "spaghetti_3d_rarity", -11).Scaled(2, 1, 2).Levels[0];
        Assert.Equal(1u << 14, level.Kx);
        uint At(int x) => unchecked(level.Ox + (uint)x * level.Kx);
        Assert.Equal(FixedNoise.Improved(level.Permutation, At(37), level.Oy, level.Oz), FixedNoise.Improved(level.Permutation, At(37 + (1 << 18)), level.Oy, level.Oz));
    }

    [Fact]
    public void The_Plane_Noise_Reads_As_Its_Formula_In_Floats_But_For_Rounding()
    {
        var noise = new Noise(12);
        var random = new Random(4);
        var worst = 0.0;
        for (int i = 0; i < 2000; i++)
        {
            int x = random.Next(-5000, 5000), z = random.Next(-5000, 5000);
            worst = Math.Max(worst, Math.Abs(noise.Fractal(x, z, 160, 5) - Reference(noise, x / 160.0, z / 160.0, 5)));
        }
        Assert.True(worst < 2e-3, $"the fixed point strays {worst} from the doubles");
    }

    // The source noise's sum in doubles over the sampler's own permutations, offsets and steps, as
    // its levels were read before they were fixed.
    private static double Reference(SourceNoise.Sampler sampler, int x, int y, int z)
    {
        var sum = 0.0;
        foreach (var (p, ox, oy, oz, kx, ky, kz, weight) in sampler.Levels)
        {
            double Coordinate(uint offset, uint step, int at) => (offset + (double)step * at) / (1 << 24) % 256;
            sum += Improved(i => p[i & 255], Coordinate(ox, kx, x), Coordinate(oy, ky, y), Coordinate(oz, kz, z), Source) * weight / FixedNoise.One;
        }
        return sum;
    }

    // The plane noise's octaves in doubles over its own permutation.
    private static double Reference(Noise noise, double x, double z, int octaves)
    {
        var p = noise.Permutation.ToArray();
        double sum = 0, amplitude = 1, total = 0;
        for (int i = 0; i < octaves; i++)
        {
            double Wrap(double v) => (v % 256 + 256) % 256;
            sum += Improved(k => p[k & 511], Wrap(x), 0.5, Wrap(z), Plane) * amplitude;
            total += amplitude;
            amplitude *= 0.5;
            x *= 2;
            z *= 2;
        }
        return sum / total;
    }

    // Ken Perlin's improved noise in doubles, its gradients by a rule given.
    private static double Improved(Func<int, int> p, double x, double y, double z, Func<int, double, double, double, double> grad)
    {
        int i = (int)Math.Floor(x), j = (int)Math.Floor(y), k = (int)Math.Floor(z);
        double fx = x - i, fy = y - j, fz = z - k;
        static double Smooth(double t) => t * t * t * (t * (t * 6 - 15) + 10);
        static double Lerp(double t, double a, double b) => a + t * (b - a);
        int a = p(i) + j, aa = p(a) + k, ab = p(a + 1) + k, b = p(i + 1) + j, ba = p(b) + k, bb = p(b + 1) + k;
        if (grad == Source)
        {
            // The source's noise hashes each corner through the permutation once more.
            (a, b) = (p(i), p(i + 1));
            (aa, ab, ba, bb) = (p(a + j), p(a + j + 1), p(b + j), p(b + j + 1));
            (aa, ab, ba, bb) = (aa + k, ab + k, ba + k, bb + k);
        }
        double u = Smooth(fx), v = Smooth(fy), w = Smooth(fz);
        return Lerp(w,
            Lerp(v, Lerp(u, grad(p(aa), fx, fy, fz), grad(p(ba), fx - 1, fy, fz)), Lerp(u, grad(p(ab), fx, fy - 1, fz), grad(p(bb), fx - 1, fy - 1, fz))),
            Lerp(v, Lerp(u, grad(p(aa + 1), fx, fy, fz - 1), grad(p(ba + 1), fx - 1, fy, fz - 1)), Lerp(u, grad(p(ab + 1), fx, fy - 1, fz - 1), grad(p(bb + 1), fx - 1, fy - 1, fz - 1))));
    }

    private static readonly int[] Gradients =
    [
        1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 0, 1, 0, 1, -1, 0, 1, 1, 0, -1, -1, 0, -1,
        0, 1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 1, 1, 0, 0, -1, 1, -1, 1, 0, 0, -1, -1,
    ];

    private static double Source(int hash, double x, double y, double z)
    {
        var g = (hash & 15) * 3;
        return Gradients[g] * x + Gradients[g + 1] * y + Gradients[g + 2] * z;
    }

    private static double Plane(int hash, double x, double y, double z)
    {
        int h = hash & 15;
        double u = h < 8 ? x : y, v = h < 4 ? y : h is 12 or 14 ? x : z;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}
