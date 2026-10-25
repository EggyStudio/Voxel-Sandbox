namespace VoxelSandbox;

/// <summary>
/// The source's newer caves, as its density functions for the overworld give them, with their
/// numbers, cheese caves where a large noise is low, spaghetti tunnels where two noises are near
/// zero, thinned by a third, noodles thinner still, entrances that open them to the ground's top,
/// and pillars left standing in the widest. The ground is solid where the density is over 0 and a
/// cave where it is 0 or under.
/// </summary>
/// <remarks>
/// <para>
/// The density is worked out at the corners of the source's cells, 4 blocks across and 8 high, and
/// between them interpolated, as the source does, which shapes its caves smoothly and asks a
/// tenth of the noises a block. Its terrain's own density, which the cave terms read for how deep a
/// place lies, is here the depth under the ground's top at 0.156 a block, about what the source's
/// gradient with height gives, so the top ten blocks open only where an entrance does, and cheese
/// caves keep a little further down.
/// </para>
/// <para>
/// It holds nothing it changes once made, so the workers that generate columns read it at once.
/// </para>
/// </remarks>
public sealed class Caves
{
    /// <summary>How much the source's terrain density grows for each block under the ground's top.</summary>
    public const double SlopedPerBlock = 0.156;

    private const int CellWidth = 4, CellHeight = 8;
    private const int Across = Section.Size / CellWidth + 1, Levels = ChunkColumn.Height / CellHeight + 1;

    private readonly SourceNoise _layer, _cheese, _entrance, _spaghetti1, _spaghetti2, _rarity, _thickness3d, _spaghetti2d,
        _modulator, _elevation, _thickness2d, _roughness, _roughnessModulator, _noodle, _noodleThickness, _ridgeA, _ridgeB,
        _pillar, _pillarRareness, _pillarThickness;

    public Caves(int seed)
    {
        // Each noise's base octave, and its octaves' amplitudes where it has more than one, the source's.
        _layer = new(seed, "cave_layer", -8);
        _cheese = new(seed, "cave_cheese", -8, 0.5, 1, 2, 1, 2, 1, 0, 2, 0);
        _entrance = new(seed, "cave_entrance", -7, 0.4, 0.5, 1);
        _spaghetti1 = new(seed, "spaghetti_3d_1", -7);
        _spaghetti2 = new(seed, "spaghetti_3d_2", -7);
        _rarity = new(seed, "spaghetti_3d_rarity", -11);
        _thickness3d = new(seed, "spaghetti_3d_thickness", -8);
        _spaghetti2d = new(seed, "spaghetti_2d", -7);
        _modulator = new(seed, "spaghetti_2d_modulator", -11);
        _elevation = new(seed, "spaghetti_2d_elevation", -8);
        _thickness2d = new(seed, "spaghetti_2d_thickness", -11);
        _roughness = new(seed, "spaghetti_roughness", -5);
        _roughnessModulator = new(seed, "spaghetti_roughness_modulator", -8);
        _noodle = new(seed, "noodle", -8);
        _noodleThickness = new(seed, "noodle_thickness", -8);
        _ridgeA = new(seed, "noodle_ridge_a", -7);
        _ridgeB = new(seed, "noodle_ridge_b", -7);
        _pillar = new(seed, "pillar", -7, 1, 1);
        _pillarRareness = new(seed, "pillar_rareness", -8);
        _pillarThickness = new(seed, "pillar_thickness", -8);
        // The readings at a scale of their own, each of the tunnels' at each width a slow noise picks.
        (_layerAt, _cheeseAt, _entranceAt, _rarityAt) = (_layer.Scaled(1, 8, 1), _cheese.Scaled(1, 0.6666666666666666, 1), _entrance.Scaled(0.75, 0.5, 0.75), _rarity.Scaled(2, 1, 2));
        (_thickness2dAt, _modulatorAt, _pillarAt) = (_thickness2d.Scaled(2, 1, 2), _modulator.Scaled(2, 1, 2), _pillar.Scaled(25, 0.3, 25));
        (_ridgeAAt, _ridgeBAt) = (_ridgeA.Scaled(NoodleScale, NoodleScale, NoodleScale), _ridgeB.Scaled(NoodleScale, NoodleScale, NoodleScale));
        _spaghetti1At = [.. SpaghettiWidths.Select(w => _spaghetti1.Scaled(w.Scale, w.Scale, w.Scale))];
        _spaghetti2At = [.. SpaghettiWidths.Select(w => _spaghetti2.Scaled(w.Scale, w.Scale, w.Scale))];
        _spaghetti2dAt = [.. Spaghetti2dWidths.Select(w => _spaghetti2d.Scaled(w.Scale, w.Scale, w.Scale))];
    }

    private readonly SourceNoise.Sampler _layerAt, _cheeseAt, _entranceAt, _rarityAt, _thickness2dAt, _modulatorAt, _pillarAt, _ridgeAAt, _ridgeBAt;
    private readonly SourceNoise.Sampler[] _spaghetti1At, _spaghetti2At, _spaghetti2dAt;

    // The tunnels' scales and weights a slow noise picks among, a quarter wider to twice as wide.
    private static readonly (double Scale, double Weight)[] SpaghettiWidths = [(4 / 3.0, 0.75), (1.0, 1.0), (2 / 3.0, 1.5), (0.5, 2.0)];
    private static readonly (double Scale, double Weight)[] Spaghetti2dWidths = [(2.0, 0.5), (4 / 3.0, 0.75), (1.0, 1.0), (0.5, 2.0), (1 / 3.0, 3.0)];
    private const double NoodleScale = 2.6666666666666665;

    /// <summary>The density at a place so many blocks under the ground's top, solid over 0 and a cave at 0 or under.</summary>
    public double Density(int x, int y, int z, double depth)
    {
        var sloped = depth * SlopedPerBlock;
        var roughness = Roughness(x, y, z);
        double density;
        // Near the top the ground opens only at an entrance, and deeper the cheese caves, the
        // tunnels and the noodles take it, the pillars standing where their noise holds them.
        if (sloped < 1.5625) density = Math.Min(sloped, 5 * Entrances(x, y, z, roughness));
        else
        {
            var cheese = 4 * Square(_layerAt.At(x, y, z)) + Math.Clamp(_cheeseAt.At(x, y, z) + 0.27, -1, 1)
                + Math.Clamp(-0.64 * sloped + 1.5, 0, 0.5);
            var underground = Math.Min(Math.Min(cheese, Entrances(x, y, z, roughness)), Spaghetti2d(x, y, z) + roughness);
            var pillars = Pillars(x, y, z);
            density = Math.Max(underground, pillars < 0.03 ? -1e6 : pillars);
        }
        // Toward the world's floor the ground closes, wholly at -64 and not at all from -40.
        density = 0.1171875 + Math.Clamp((y + 64) / 24.0, 0, 1) * (density - 0.1171875);
        return Math.Min(density, Noodle(x, y, z));
    }

    /// <summary>
    /// The density at the corners of a column's cells, under the ground's top a function gives at
    /// each corner, from the world's floor up through the cell over the highest top, solid above it.
    /// </summary>
    public Grid Carve(int columnX, int columnZ, Func<int, int, int> groundAt)
    {
        var grid = new Grid(new double[Across * Across * Levels]);
        int x0 = columnX * Section.Size, z0 = columnZ * Section.Size;
        for (int cz = 0; cz < Across; cz++)
            for (int cx = 0; cx < Across; cx++)
            {
                int x = x0 + cx * CellWidth, z = z0 + cz * CellWidth;
                var ground = groundAt(x, z);
                for (int level = 0; level < Levels; level++)
                    grid.Values[(level * Across + cz) * Across + cx] = Corner(x, ChunkColumn.Bottom + level * CellHeight, z, ground);
            }
        return grid;
    }

    // The density at a corner of the cells under the ground's top there. The top block counts one
    // deep, as the source's terrain is solid through its surface, and a corner over the ground as
    // deep as the top, so a cell across the top opens only where an entrance does.
    private double Corner(int x, int y, int z, int ground) => y > ground + CellHeight ? 1 : Density(x, y, z, Math.Max(ground - y, 0) + 1);

    /// <summary>
    /// The density at single places as a column's <see cref="Grid"/> has it, each corner of the
    /// cells asked worked out once, for a feature that reaches from one column into another and
    /// asks whether the ground it stands on is carved where no grid of that column is at hand.
    /// </summary>
    public sealed class Places(Caves caves, Func<int, int, int> groundAt)
    {
        private readonly Dictionary<(int, int, int), double> _corners = [];

        /// <summary>The density at a place, interpolated between its cell's corners as the column's grid interpolates it.</summary>
        public double DensityAt(int x, int y, int z)
        {
            int x0 = (int)Math.Floor(x / (double)CellWidth) * CellWidth, z0 = (int)Math.Floor(z / (double)CellWidth) * CellWidth;
            var y0 = ChunkColumn.Bottom + (y - ChunkColumn.Bottom) / CellHeight * CellHeight;
            double fx = (x - x0) / (double)CellWidth, fz = (z - z0) / (double)CellWidth, fy = (y - y0) / (double)CellHeight;
            double At(int dx, int dy, int dz) => Corner(x0 + dx * CellWidth, y0 + dy * CellHeight, z0 + dz * CellWidth);
            static double Lerp(double t, double a, double b) => a + t * (b - a);
            return Lerp(fy,
                Lerp(fz, Lerp(fx, At(0, 0, 0), At(1, 0, 0)), Lerp(fx, At(0, 0, 1), At(1, 0, 1))),
                Lerp(fz, Lerp(fx, At(0, 1, 0), At(1, 1, 0)), Lerp(fx, At(0, 1, 1), At(1, 1, 1))));
        }

        private double Corner(int x, int y, int z)
        {
            if (_corners.TryGetValue((x, y, z), out var value)) return value;
            return _corners[(x, y, z)] = caves.Corner(x, y, z, groundAt(x, z));
        }
    }

    /// <summary>The density at the corners of a column's cells, rows of corners along x, then along z, then up.</summary>
    public readonly record struct Grid(double[] Values)
    {
        /// <summary>The density at a place of the column, x and z from 0 to 15 and y within the world, interpolated between its cell's corners.</summary>
        public double DensityAt(int x, int y, int z)
        {
            int cx = x / CellWidth, cz = z / CellWidth, level = (y - ChunkColumn.Bottom) / CellHeight;
            double fx = x % CellWidth / (double)CellWidth, fz = z % CellWidth / (double)CellWidth, fy = (y - ChunkColumn.Bottom) % CellHeight / (double)CellHeight;
            var values = Values;
            double At(int dx, int dy, int dz) => values[((level + dy) * Across + cz + dz) * Across + cx + dx];
            double Lerp(double t, double a, double b) => a + t * (b - a);
            return Lerp(fy,
                Lerp(fz, Lerp(fx, At(0, 0, 0), At(1, 0, 0)), Lerp(fx, At(0, 0, 1), At(1, 0, 1))),
                Lerp(fz, Lerp(fx, At(0, 1, 0), At(1, 1, 0)), Lerp(fx, At(0, 1, 1), At(1, 1, 1))));
        }
    }

    private double Entrances(int x, int y, int z, double roughness)
    {
        var opening = _entranceAt.At(x, y, z) + 0.37 + Gradient(y, -10, 0.3, 30, 0);
        // The tunnels' width by a slow noise, a quarter wider to twice as wide, as the source picks
        // among four of its readings.
        var rarity = _rarityAt.At(x, y, z);
        var width = rarity < -0.5 ? 0 : rarity < 0 ? 1 : rarity < 0.5 ? 2 : 3;
        var weight = SpaghettiWidths[width].Weight;
        var first = Math.Abs(_spaghetti1At[width].At(x, y, z) * weight);
        var second = Math.Abs(_spaghetti2At[width].At(x, y, z) * weight);
        var thickness = _thickness3d.At(x, y, z) * -0.011500001 - 0.0765;
        return Math.Min(opening, roughness + Math.Clamp(Math.Max(first, second) + thickness, -1, 1));
    }

    private double Spaghetti2d(int x, int y, int z)
    {
        var thickness = _thickness2dAt.At(x, y, z) * -0.34999996 - 0.95;
        var modulator = _modulatorAt.At(x, y, z);
        var width = modulator < -0.75 ? 0 : modulator < -0.5 ? 1 : modulator < 0.5 ? 2 : modulator < 0.75 ? 3 : 4;
        var tunnel = Math.Abs(_spaghetti2dAt[width].At(x, y, z) * Spaghetti2dWidths[width].Weight) + thickness * 0.083;
        var level = Cube(Math.Abs(_elevation.At(x, 0, z) * 8 + Gradient(y, -64, 8, 320, -40)) + thickness);
        return Math.Clamp(Math.Max(tunnel, level), -1, 1);
    }

    private double Roughness(int x, int y, int z) =>
        (_roughnessModulator.At(x, y, z) * -0.05 - 0.05) * (Math.Abs(_roughness.At(x, y, z)) - 0.4);

    private double Pillars(int x, int y, int z) =>
        (_pillarAt.At(x, y, z) * 2 + (_pillarRareness.At(x, y, z) * -1 - 1)) * Cube(_pillarThickness.At(x, y, z) * 0.55 + 0.55);

    private double Noodle(int x, int y, int z)
    {
        if (y < -60 || y >= 321 || _noodle.At(x, y, z) < 0) return 64;
        var ridges = Math.Max(Math.Abs(_ridgeAAt.At(x, y, z)), Math.Abs(_ridgeBAt.At(x, y, z)));
        return _noodleThickness.At(x, y, z) * -0.025 - 0.075 + ridges * 1.5;
    }

    // A value from one at one height to another at another, held at each beyond them.
    private static double Gradient(double y, double from, double fromValue, double to, double toValue) =>
        fromValue + Math.Clamp((y - from) / (to - from), 0, 1) * (toValue - fromValue);

    private static double Square(double v) => v * v;

    private static double Cube(double v) => v * v * v;
}
