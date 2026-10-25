namespace VoxelSandbox;

/// <summary>
/// The source's normal noise, two sums of octaves of its improved Perlin noise, the second read at
/// a hair over the first's coordinates, their sum scaled by a sixth over the deviation the source
/// expects of it, from a base octave, a frequency of two to its power a block, and an amplitude for
/// each octave, as its worldgen data gives a noise.
/// </summary>
/// <remarks>
/// <para>
/// The deviation expected is a tenth of one and one over the span of octaves with an amplitude
/// and one, as the source's normal noise reckons it, so a noise of many octaves spreads about as
/// widely as one of one and the caves' thresholds read the same of each. The source's newer data
/// gives each noise a base amplitude in its place, which encodes the same scale for its own sums.
/// </para>
/// <para>
/// Each octave twice the frequency of the one before weighs half as much, the lowest weighing
/// two to the power of the octaves less one over two to their power less one, so the octaves sum
/// to about the lowest's range. Each octave is an improved Perlin noise of its own permutation and
/// offset, seeded from the world's seed and the noise's name, so two noises of one world differ
/// and a world gives each the same every time.
/// </para>
/// <para>
/// Its values are worked out in fixed point with integers alone, so a shader on the GPU, which
/// rounds floats as it will, reckons each one to the bit as the CPU does. A level reads a point as
/// its offset plus the point's integer coordinates times its frequency, each a 32-bit integer of
/// 24 fractional bits, so the 8 whole bits wrap at the improved noise's period of 256 as the sum
/// overflows, and the noise, its fades, its lerps and the weights of its levels are 16 fractional
/// bits, each product taken down to the floor as <see cref="FixedNoise.Mul"/> takes it. A frequency
/// a hair from its own value, rounded to 24 bits, moves a level's reading by under a ten
/// thousandth of a cycle a thousand blocks out.
/// </para>
/// <para>
/// It holds nothing it changes once made, so the workers that generate columns read it at once.
/// </para>
/// </remarks>
public sealed class SourceNoise
{
    private const double Second = 1.0181268882175227;

    // Each level of the two sums, its improved noise, its frequency over the first sum's lowest,
    // and its weight in the noise's value.
    private readonly (Improved Noise, double Frequency, int Weight)[] _levels;
    private readonly Sampler _plain;

    public SourceNoise(int seed, string name, int baseOctave, params double[] amplitudes)
    {
        if (amplitudes.Length == 0) amplitudes = [1];
        var random = new Random(seed ^ StableHash(name));
        // The first sum's levels draw their permutations and offsets before the second's, as the
        // source seeds them.
        var first = amplitudes.Select(a => a != 0 ? new Improved(random) : null).ToArray();
        var second = amplitudes.Select(a => a != 0 ? new Improved(random) : null).ToArray();
        var span = Array.FindLastIndex(amplitudes, a => a != 0) - Array.FindIndex(amplitudes, a => a != 0);
        var amplitude = 1 / 6.0 / (0.1 * (1 + 1.0 / (span + 1)));
        var levels = new List<(Improved, double, int)>();
        foreach (var (sum, scale) in new[] { (first, 1.0), (second, Second) })
        {
            double input = Math.Pow(2, baseOctave), weight = Math.Pow(2, amplitudes.Length - 1) / (Math.Pow(2, amplitudes.Length) - 1);
            for (int i = 0; i < amplitudes.Length; i++)
            {
                if (sum[i] is { } level) levels.Add((level, input * scale, (int)Math.Round(amplitudes[i] * weight * amplitude * FixedNoise.One)));
                input *= 2;
                weight /= 2;
            }
        }
        _levels = [.. levels];
        _plain = Scaled(1, 1, 1);
    }

    /// <summary>The noise at a point of integer coordinates, from about -1 to 1.</summary>
    public double At(int x, int y, int z) => _plain.At(x, y, z);

    /// <summary>A reading of the noise at points whose coordinates are taken times a scale along each axis.</summary>
    public Sampler Scaled(double x, double y, double z) => new(this, new Vector3d(x, y, z));

    /// <summary>A scale along each axis.</summary>
    public readonly record struct Vector3d(double X, double Y, double Z);

    /// <summary>
    /// The noise read at points taken times a scale, each level's frequency along each axis and its
    /// offset as integers of 24 fractional bits, which a shader reads as the CPU does.
    /// </summary>
    public sealed class Sampler
    {
        /// <summary>Each level's permutation, its offset and frequency along x, y and z, and its weight.</summary>
        public readonly (byte[] Permutation, uint Ox, uint Oy, uint Oz, uint Kx, uint Ky, uint Kz, int Weight)[] Levels;

        internal Sampler(SourceNoise noise, Vector3d scale)
        {
            static uint Fixed(double v) => unchecked((uint)(long)Math.Round(v * (1 << 24)));
            Levels = [.. noise._levels.Select(l => (l.Noise.Permutation, Fixed(l.Noise.Offset.X), Fixed(l.Noise.Offset.Y), Fixed(l.Noise.Offset.Z),
                Fixed(l.Frequency * scale.X), Fixed(l.Frequency * scale.Y), Fixed(l.Frequency * scale.Z), l.Weight))];
        }

        /// <summary>The noise at a point of integer coordinates, from about -1 to 1.</summary>
        public double At(int x, int y, int z) => FixedAt(x, y, z) / (double)FixedNoise.One;

        /// <summary>The noise at a point as an integer of 16 fractional bits, as a shader reckons it.</summary>
        public int FixedAt(int x, int y, int z)
        {
            int sum = 0;
            foreach (var (p, ox, oy, oz, kx, ky, kz, weight) in Levels)
            {
                uint cx = unchecked(ox + (uint)x * kx), cy = unchecked(oy + (uint)y * ky), cz = unchecked(oz + (uint)z * kz);
                sum += FixedNoise.Mul(FixedNoise.Improved(p, cx, cy, cz), weight);
            }
            return sum;
        }
    }

    // A string's hash that is the same in every run, where string.GetHashCode is not.
    private static int StableHash(string text)
    {
        unchecked
        {
            var hash = (int)2166136261;
            foreach (var c in text) hash = (hash ^ c) * 16777619;
            return hash;
        }
    }

    // Ken Perlin's improved noise's permutation of 256 and its offset of its own, drawn as the source draws them.
    private sealed class Improved
    {
        public readonly byte[] Permutation = new byte[256];
        public readonly Vector3d Offset;

        public Improved(Random random)
        {
            Offset = new Vector3d(random.NextDouble() * 256, random.NextDouble() * 256, random.NextDouble() * 256);
            for (int i = 0; i < 256; i++) Permutation[i] = (byte)i;
            for (int i = 0; i < 256; i++)
            {
                var j = random.Next(256 - i);
                (Permutation[i], Permutation[i + j]) = (Permutation[i + j], Permutation[i]);
            }
        }
    }
}

/// <summary>
/// Ken Perlin's improved noise in fixed point, the source's twelve edges of a cube and four of them
/// again for its gradients, read from integers alone so a shader reckons it to the bit as the CPU does.
/// </summary>
public static class FixedNoise
{
    /// <summary>One in 16 fractional bits.</summary>
    public const int One = 1 << 16;

    private static readonly int[] Gradients =
    [
        1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 0, 1, 0, 1, -1, 0, 1, 1, 0, -1, -1, 0, -1,
        0, 1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 1, 1, 0, 0, -1, 1, -1, 1, 0, 0, -1, -1,
    ];

    /// <summary>
    /// The product of two numbers of 16 fractional bits, taken down to the floor, which a shader with
    /// no 64-bit integers takes in four products of halves to the same bits.
    /// </summary>
    public static int Mul(int a, int b) => (int)(((long)a * b) >> 16);

    /// <summary>The fade of a fraction of 16 bits, six to the fifth less fifteen to the fourth plus ten to the third.</summary>
    public static int Smooth(int t) => Mul(Mul(Mul(t, t), t), Mul(t, 6 * t - 15 * One) + 10 * One);

    private static int Lerp(int t, int a, int b) => a + Mul(t, b - a);

    private static int Dot(int hash, int x, int y, int z)
    {
        var g = (hash & 15) * 3;
        return Gradients[g] * x + Gradients[g + 1] * y + Gradients[g + 2] * z;
    }

    /// <summary>
    /// The noise at a point given as integers of 24 fractional bits whose 8 whole bits wrap at the
    /// period of 256, from about -1 to 1 in 16 fractional bits.
    /// </summary>
    public static int Improved(byte[] permutation, uint x, uint y, uint z)
    {
        int P(int i) => permutation[i & 255];
        int i = (int)(x >> 24), j = (int)(y >> 24), k = (int)(z >> 24);
        int fx = (int)(x >> 8 & 0xFFFF), fy = (int)(y >> 8 & 0xFFFF), fz = (int)(z >> 8 & 0xFFFF);
        int a = P(i), b = P(i + 1);
        int aa = P(a + j), ab = P(a + j + 1), ba = P(b + j), bb = P(b + j + 1);
        int u = Smooth(fx), v = Smooth(fy), w = Smooth(fz);
        return Lerp(w,
            Lerp(v, Lerp(u, Dot(P(aa + k), fx, fy, fz), Dot(P(ba + k), fx - One, fy, fz)),
                    Lerp(u, Dot(P(ab + k), fx, fy - One, fz), Dot(P(bb + k), fx - One, fy - One, fz))),
            Lerp(v, Lerp(u, Dot(P(aa + k + 1), fx, fy, fz - One), Dot(P(ba + k + 1), fx - One, fy, fz - One)),
                    Lerp(u, Dot(P(ab + k + 1), fx, fy - One, fz - One), Dot(P(bb + k + 1), fx - One, fy - One, fz - One))));
    }
}
