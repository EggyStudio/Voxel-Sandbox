namespace VoxelSandbox;

/// <summary>
/// The source's aquifers, which fill its caves with water or lava to levels of their own, so a
/// cave holds a lake at one height and the next a dry floor or one at another, with walls left
/// standing between two levels, and lava lies under -54 wherever the caves reach.
/// </summary>
/// <remarks>
/// <para>
/// The ground is cut into cells 16 blocks across and 12 high, each with a place within it by its
/// hash, and a cave's block takes the fluid of the nearest place's aquifer, to its level, and air
/// over it. Where the ground's top over the place lies under the sea, the aquifer is the sea's, to
/// the sea level; elsewhere a noise of floodedness, readier near the top under the sea, leaves it
/// dry, floods it to the sea level, or sets it a level of its own, the middle of its band of 40
/// blocks moved by a noise of spread up or down by steps of 3, never over the ground's lowest top
/// around. A level of -10 or under is lava where a slow noise of lava is far from 0.
/// </para>
/// <para>
/// Near the middle between two places whose aquifers differ, a wall is kept where the pressure of
/// their difference, eased by a noise of barriers, outweighs how open the cave is, so the two
/// levels stand apart, as the source's. The ground's top is read at the middles of the chunks
/// around a place's, each once a column, where the source reads it at the place's own offsets.
/// </para>
/// </remarks>
public sealed class Aquifers
{
    /// <summary>The height under which every cave holds lava, the source's.</summary>
    public const int LavaLevel = -54;

    // The level of an aquifer with no fluid, as far under the world as the source's, since the
    // pressure between it and another reads their middle, which must lie under every place.
    private const int Dry = -2032;

    private readonly SourceNoise _barrier, _floodedness, _spread, _lava;
    private readonly int _seed, _seaLevel;

    public Aquifers(int seed, int seaLevel)
    {
        (_seed, _seaLevel) = (seed, seaLevel);
        _barrier = new(seed, "aquifer_barrier", -3);
        _floodedness = new(seed, "aquifer_fluid_level_floodedness", -7);
        _spread = new(seed, "aquifer_fluid_level_spread", -5);
        _lava = new(seed, "aquifer_lava", -1);
    }

    /// <summary>An aquifer's level and fluid, its fluid under the level and air at it and over it.</summary>
    public readonly record struct Status(int Level, BlockId Fluid)
    {
        public BlockId At(int y) => y < Level ? Fluid : BlockId.Air;
    }

    /// <summary>The aquifers of one column as it is generated, its ground's top around read once and each aquifer worked out once.</summary>
    public sealed class Column(Aquifers aquifers, Func<int, int, int> groundAt)
    {
        private readonly Dictionary<(int, int, int), Status> _statuses = [];
        private readonly Dictionary<(int, int), int> _surfaces = [];

        /// <summary>
        /// What a cave's block at a place becomes, the fluid or the air of its aquifer, or null for
        /// solid ground kept as a wall between two aquifers, by the cave's density there, 0 or under.
        /// </summary>
        public BlockId? At(int x, int y, int z, double density)
        {
            if (y < LavaLevel) return BlockId.Lava;
            // The three nearest places of the cells around, by their squared distances.
            int gx = (int)Math.Floor((x - 5) / 16.0), gy = (int)Math.Floor((y + 1) / 12.0), gz = (int)Math.Floor((z - 5) / 16.0);
            (int D, (int, int, int) At) first = (int.MaxValue, default), second = first, third = first;
            for (int dx = 0; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    for (int dz = 0; dz <= 1; dz++)
                    {
                        var place = aquifers.Place(gx + dx, gy + dy, gz + dz);
                        int ex = place.X - x, ey = place.Y - y, ez = place.Z - z;
                        var d = ex * ex + ey * ey + ez * ez;
                        if (d < first.D) (third, second, first) = (second, first, (d, place));
                        else if (d < second.D) (third, second) = (second, (d, place));
                        else if (d < third.D) third = (d, place);
                    }
            var one = StatusAt(first.At);
            var fluid = one.At(y);
            var near12 = Similarity(first.D, second.D);
            if (near12 <= 0) return fluid;
            if (fluid == BlockId.Water && aquifers.Global(y - 1).At(y - 1) == BlockId.Lava) return fluid;
            var two = StatusAt(second.At);
            if (density + near12 * aquifers.Pressure(x, y, z, one, two) > 0) return null;
            var three = StatusAt(third.At);
            var near13 = Similarity(first.D, third.D);
            if (near13 > 0 && density + near12 * near13 * aquifers.Pressure(x, y, z, one, three) > 0) return null;
            var near23 = Similarity(second.D, third.D);
            if (near23 > 0 && density + near12 * near23 * aquifers.Pressure(x, y, z, two, three) > 0) return null;
            return fluid;
        }

        private static double Similarity(int a, int b) => 1 - Math.Abs(b - a) / 25.0;

        private Status StatusAt((int X, int Y, int Z) place)
        {
            if (_statuses.TryGetValue(place, out var status)) return status;
            return _statuses[place] = aquifers.Work(place.X, place.Y, place.Z, Surface);
        }

        // The ground's top at the middle of a chunk, read once a column.
        private int Surface(int chunkX, int chunkZ)
        {
            if (_surfaces.TryGetValue((chunkX, chunkZ), out var top)) return top;
            return _surfaces[(chunkX, chunkZ)] = groundAt(chunkX * Section.Size + 8, chunkZ * Section.Size + 8);
        }
    }

    /// <summary>The aquifers of a column about to be generated, under the ground's top a function gives at a place.</summary>
    public Column For(Func<int, int, int> groundAt) => new(this, groundAt);

    // The place of a cell's aquifer, within 10 blocks across and 9 up of its corner, by its hash.
    private (int X, int Y, int Z) Place(int gx, int gy, int gz)
    {
        var hash = PlaceHash.Of(gx * 31 + gy, gz, _seed ^ 0x0aaf);
        return (gx * 16 + (int)(hash % 10), gy * 12 + (int)(hash / 10 % 9), gz * 16 + (int)(hash / 90 % 10));
    }

    // The fluid everywhere but in an aquifer of its own, lava under -54 and the sea to its level over it.
    private Status Global(int y) => y < LavaLevel ? new Status(LavaLevel, BlockId.Lava) : new Status(_seaLevel, BlockId.Water);

    // The chunks around a place whose ground's top the source reads, as offsets in chunks.
    private static readonly (int X, int Z)[] Around = [(0, 0), (-2, -1), (-1, -1), (0, -1), (1, -1), (-3, 0), (-2, 0), (-1, 0), (1, 0), (-2, 1), (-1, 1), (0, 1), (1, 1)];

    // An aquifer at a place, the sea's where the ground's top near it lies under the sea, and
    // otherwise its own, dry, flooded to the sea's level or at a level of its own.
    private Status Work(int x, int y, int z, Func<int, int, int> surface)
    {
        var global = Global(y);
        int lowest = int.MaxValue, top = y + 12, bottom = y - 12;
        var underSea = false;
        int cx = (int)Math.Floor(x / 16.0), cz = (int)Math.Floor(z / 16.0);
        foreach (var (ox, oz) in Around)
        {
            var ground = surface(cx + ox, cz + oz);
            var raised = ground + 8;
            var middle = ox == 0 && oz == 0;
            if (middle && bottom > raised) return global;
            var higher = top > raised;
            if ((higher || middle) && Global(raised).At(raised) != BlockId.Air)
            {
                if (middle) underSea = true;
                if (higher) return Global(raised);
            }
            lowest = Math.Min(lowest, ground);
        }
        var level = Level(x, y, z, global, lowest, underSea);
        return new Status(level, FluidOf(x, y, z, global, level));
    }

    private int Level(int x, int y, int z, Status global, int lowest, bool underSea)
    {
        var depth = lowest + 8 - y;
        var near = underSea ? Math.Clamp(1 - depth / 64.0, 0, 1) : 0;
        var floodedness = Math.Clamp(_floodedness.At(x, y * 0.67, z), -1, 1);
        var spreading = floodedness - (0.8 - 1.1 * near);
        var flooded = floodedness - (0.4 - 1.2 * near);
        if (spreading > 0) return global.Level;
        if (flooded <= 0) return Dry;
        int gx = (int)Math.Floor(x / 16.0), gy = (int)Math.Floor(y / 40.0), gz = (int)Math.Floor(z / 16.0);
        var spread = (int)Math.Floor(_spread.At(gx, gy * 0.7142857142857143, gz) * 10 / 3) * 3;
        return Math.Min(lowest, gy * 40 + 20 + spread);
    }

    private BlockId FluidOf(int x, int y, int z, Status global, int level)
    {
        if (level > -10 || level == Dry || global.Fluid == BlockId.Lava) return global.Fluid;
        int gx = (int)Math.Floor(x / 64.0), gy = (int)Math.Floor(y / 40.0), gz = (int)Math.Floor(z / 64.0);
        return Math.Abs(_lava.At(gx, gy, gz)) > 0.3 ? BlockId.Lava : BlockId.Water;
    }

    // How strongly two aquifers keep a wall between them at a height, as the source reckons it, most
    // where water meets lava and where the height lies between their two levels.
    private double Pressure(int x, int y, int z, Status one, Status two)
    {
        var (a, b) = (one.At(y), two.At(y));
        if (a == BlockId.Lava && b == BlockId.Water || a == BlockId.Water && b == BlockId.Lava) return 2;
        var difference = Math.Abs(one.Level - two.Level);
        if (difference == 0) return 0;
        var offset = y + 0.5 - 0.5 * (one.Level + two.Level);
        var within = difference / 2.0 - Math.Abs(offset);
        double pressure;
        if (offset > 0)
        {
            var t = within;
            pressure = t > 0 ? t / 1.5 : t / 2.5;
        }
        else
        {
            var t = 3 + within;
            pressure = t > 0 ? t / 3 : t / 10;
        }
        var barrier = pressure is >= -2 and <= 2 ? _barrier.At(x, y * 0.5, z) : 0;
        return 2 * (barrier + pressure);
    }
}
