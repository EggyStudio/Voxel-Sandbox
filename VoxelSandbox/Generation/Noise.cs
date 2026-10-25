namespace VoxelSandbox;

/// <summary>Ken Perlin's improved gradient noise on a plane, from a seed, read at integer places over a scale.</summary>
/// <remarks>
/// <para>
/// Its values are worked out in fixed point with integers alone, as <see cref="SourceNoise"/>'s
/// are, so a shader on the GPU reckons each one to the bit as the CPU does. A place's coordinate is
/// the place times its step, a block's share of the noise's cycle over the scale, an integer of 24
/// fractional bits whose 8 whole bits wrap at the noise's period of 256 as it overflows, and each
/// octave doubles the step. The noise, its fades, its lerps and its octaves' weights are 16
/// fractional bits, each product taken down to the floor.
/// </para>
/// <para>
/// Read only once made, so the workers that generate columns share one.
/// </para>
/// </remarks>
public sealed class Noise
{
    private readonly byte[] _permutation = new byte[512];

    // A coordinate of one half in 24 fractional bits, where the plane lies through the noise's cells.
    private const uint Half = 1u << 23;

    public Noise(int seed)
    {
        var order = new byte[256];
        for (int i = 0; i < 256; i++) order[i] = (byte)i;
        new Random(seed).Shuffle(order);
        for (int i = 0; i < 512; i++) _permutation[i] = order[i & 255];
    }

    /// <summary>The noise's permutation, twice over, which a shader reads as the CPU does.</summary>
    public ReadOnlySpan<byte> Permutation => _permutation;

    /// <summary>The step of a block along an axis over a scale in blocks, in 24 fractional bits.</summary>
    public static uint Step(float scale) => (uint)Math.Round((1 << 24) / (double)scale);

    /// <summary>The noise at a place over a scale in blocks, from about -1 to 1.</summary>
    public float At(int x, int z, float scale) => FixedAt(x, z, Step(scale)) / (float)FixedNoise.One;

    /// <summary>
    /// Octaves of the noise at a place over a scale in blocks, each twice as fine and half as strong,
    /// from about -1 to 1.
    /// </summary>
    public float Fractal(int x, int z, float scale, int octaves) => FixedFractal(x, z, Step(scale), octaves) / (float)FixedNoise.One;

    /// <summary>The noise at a place at a step a block, in 16 fractional bits, as a shader reckons it.</summary>
    public int FixedAt(int x, int z, uint step) => Improved(unchecked((uint)x * step), Half, unchecked((uint)z * step));

    /// <summary>The octaves of the noise at a place at a step a block, in 16 fractional bits, as a shader reckons them.</summary>
    public int FixedFractal(int x, int z, uint step, int octaves)
    {
        int sum = 0;
        for (int i = 0; i < octaves; i++)
            sum += FixedNoise.Mul(FixedAt(x, z, unchecked(step << i)), Weight(i, octaves));
        return sum;
    }

    /// <summary>An octave's share of the octaves' sum, its amplitude, a half for each octave before it, over their total, in 16 fractional bits.</summary>
    public static int Weight(int octave, int octaves) => (int)Math.Round(Math.Pow(0.5, octave) / (2 - Math.Pow(0.5, octaves - 1)) * FixedNoise.One);

    private int Improved(uint x, uint y, uint z)
    {
        int xi = (int)(x >> 24), yi = (int)(y >> 24), zi = (int)(z >> 24);
        int fx = (int)(x >> 8 & 0xFFFF), fy = (int)(y >> 8 & 0xFFFF), fz = (int)(z >> 8 & 0xFFFF);
        int u = FixedNoise.Smooth(fx), v = FixedNoise.Smooth(fy), w = FixedNoise.Smooth(fz);
        var p = _permutation;
        int a = p[xi] + yi, aa = p[a] + zi, ab = p[a + 1] + zi;
        int b = p[xi + 1] + yi, ba = p[b] + zi, bb = p[b + 1] + zi;
        const int One = FixedNoise.One;
        return Lerp(w,
            Lerp(v, Lerp(u, Grad(p[aa], fx, fy, fz), Grad(p[ba], fx - One, fy, fz)),
                Lerp(u, Grad(p[ab], fx, fy - One, fz), Grad(p[bb], fx - One, fy - One, fz))),
            Lerp(v, Lerp(u, Grad(p[aa + 1], fx, fy, fz - One), Grad(p[ba + 1], fx - One, fy, fz - One)),
                Lerp(u, Grad(p[ab + 1], fx, fy - One, fz - One), Grad(p[bb + 1], fx - One, fy - One, fz - One))));
    }

    private static int Lerp(int t, int a, int b) => a + FixedNoise.Mul(t, b - a);

    private static int Grad(int hash, int x, int y, int z)
    {
        int h = hash & 15;
        int u = h < 8 ? x : y;
        int v = h < 4 ? y : h is 12 or 14 ? x : z;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}
