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
/// It holds nothing it changes once made, so the workers that generate columns read it at once.
/// </para>
/// </remarks>
public sealed class SourceNoise
{
    private const double Second = 1.0181268882175227;
    private readonly Octaves _first, _second;
    private readonly double _amplitude;

    /// <summary>A noise of a world's seed by its name, from its base octave, and the amplitude of each octave, one of 1 where none are given.</summary>
    public SourceNoise(int seed, string name, int baseOctave, params double[] amplitudes)
    {
        if (amplitudes.Length == 0) amplitudes = [1];
        var random = new Random(seed ^ StableHash(name));
        _first = new Octaves(random, baseOctave, amplitudes);
        _second = new Octaves(random, baseOctave, amplitudes);
        var span = Array.FindLastIndex(amplitudes, a => a != 0) - Array.FindIndex(amplitudes, a => a != 0);
        _amplitude = 1 / 6.0 / (0.1 * (1 + 1.0 / (span + 1)));
    }

    /// <summary>The noise at a place.</summary>
    public double At(double x, double y, double z) =>
        (_first.At(x, y, z) + _second.At(x * Second, y * Second, z * Second)) * _amplitude;

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

    // The octaves of one sum, each an improved noise at twice the frequency of the one before.
    private sealed class Octaves
    {
        private readonly Improved?[] _levels;
        private readonly double[] _amplitudes;
        private readonly double _lowestInput, _lowestValue;

        public Octaves(Random random, int baseOctave, double[] amplitudes)
        {
            _amplitudes = amplitudes;
            _levels = [.. amplitudes.Select(a => a != 0 ? new Improved(random) : null)];
            _lowestInput = Math.Pow(2, baseOctave);
            _lowestValue = Math.Pow(2, amplitudes.Length - 1) / (Math.Pow(2, amplitudes.Length) - 1);
        }

        public double At(double x, double y, double z)
        {
            double value = 0, input = _lowestInput, weight = _lowestValue;
            for (int i = 0; i < _levels.Length; i++)
            {
                if (_levels[i] is { } level) value += _amplitudes[i] * level.At(Wrap(x * input), Wrap(y * input), Wrap(z * input)) * weight;
                input *= 2;
                weight /= 2;
            }
            return value;
        }

        // A coordinate kept within the range a double keeps its fractions in, as the source wraps it.
        private static double Wrap(double v) => v - Math.Floor(v / 3.3554432E7 + 0.5) * 3.3554432E7;
    }

    // Ken Perlin's improved noise, a permutation of 256 and an offset of its own, its gradients the
    // twelve edges of a cube and four of them again, as the source's.
    private sealed class Improved
    {
        private static readonly int[] Gradients =
        [
            1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 0, 1, 0, 1, -1, 0, 1, 1, 0, -1, -1, 0, -1,
            0, 1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 1, 1, 0, 0, -1, 1, -1, 1, 0, 0, -1, -1,
        ];

        private readonly byte[] _p = new byte[256];
        private readonly double _xo, _yo, _zo;

        public Improved(Random random)
        {
            (_xo, _yo, _zo) = (random.NextDouble() * 256, random.NextDouble() * 256, random.NextDouble() * 256);
            for (int i = 0; i < 256; i++) _p[i] = (byte)i;
            for (int i = 0; i < 256; i++)
            {
                var j = random.Next(256 - i);
                (_p[i], _p[i + j]) = (_p[i + j], _p[i]);
            }
        }

        private int P(int i) => _p[i & 255];

        private static double Dot(int hash, double x, double y, double z)
        {
            var g = (hash & 15) * 3;
            return Gradients[g] * x + Gradients[g + 1] * y + Gradients[g + 2] * z;
        }

        private static double Smooth(double t) => t * t * t * (t * (t * 6 - 15) + 10);

        private static double Lerp(double t, double a, double b) => a + t * (b - a);

        public double At(double x, double y, double z)
        {
            double dx = x + _xo, dy = y + _yo, dz = z + _zo;
            int i = (int)Math.Floor(dx), j = (int)Math.Floor(dy), k = (int)Math.Floor(dz);
            double fx = dx - i, fy = dy - j, fz = dz - k;
            int a = P(i), b = P(i + 1);
            int aa = P(a + j), ab = P(a + j + 1), ba = P(b + j), bb = P(b + j + 1);
            double u = Smooth(fx), v = Smooth(fy), w = Smooth(fz);
            return Lerp(w,
                Lerp(v, Lerp(u, Dot(P(aa + k), fx, fy, fz), Dot(P(ba + k), fx - 1, fy, fz)),
                        Lerp(u, Dot(P(ab + k), fx, fy - 1, fz), Dot(P(bb + k), fx - 1, fy - 1, fz))),
                Lerp(v, Lerp(u, Dot(P(aa + k + 1), fx, fy, fz - 1), Dot(P(ba + k + 1), fx - 1, fy, fz - 1)),
                        Lerp(u, Dot(P(ab + k + 1), fx, fy - 1, fz - 1), Dot(P(bb + k + 1), fx - 1, fy - 1, fz - 1))));
        }
    }
}
