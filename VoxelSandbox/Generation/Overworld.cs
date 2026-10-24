using Engine;

namespace VoxelSandbox;

/// <summary>
/// Land of the biomes a climate chooses, with rolling plains and forests of oak and birch, a dark
/// forest where it is wettest, taiga and snowy plains of spruce where it is cold, flat desert of
/// sand and cactus where it is hot and dry, the badlands' mesas of banded terracotta within it, the
/// savanna beside it and the jungle where it is hot and wet, swamps sunk to the sea level with pools
/// among them, mountains of stone with snowy tops, lakes and seas up to sea level over sand and
/// gravel, frozen where it is cold, beaches along them, rivers winding through the land to them,
/// veins of ore in the stone and the deepslate, the source's caves under the ground opening to the
/// sky at its entrances, and ravines.
/// </summary>
public sealed class Overworld : IWorldGenerator
{
    /// <summary>The height water stands to over lower ground.</summary>
    public const int SeaLevel = 48;

    private const int SnowLine = 92;
    // Ground this high is mountains, bare stone, whatever the climate.
    private const int MountainsFrom = 82;
    private const int SandBelow = 50;
    // Badlands ground this high is a mesa, its terracotta bare.
    private const int Mesas = SeaLevel + 14;

    private readonly Noise _hills, _mountains, _sand, _gravel, _basins, _rivers, _mesas, _swampFloor, _mottle, _clay;
    private readonly Climate _climate;
    private readonly Caves _caves;
    private readonly Aquifers _aquifers;
    // The badlands' terracotta's color at each height, bands one to three high, the same for a seed.
    private readonly BlockId[] _bands;

    public Overworld(int seed)
    {
        Seed = seed;
        _hills = new Noise(seed);
        _mountains = new Noise(seed + 1);
        _sand = new Noise(seed + 2);
        _gravel = new Noise(seed + 3);
        _basins = new Noise(seed + 7);
        _rivers = new Noise(seed + 12);
        _mesas = new Noise(seed + 13);
        _swampFloor = new Noise(seed + 14);
        _mottle = new Noise(seed + 15);
        _clay = new Noise(seed + 16);
        _climate = new Climate(seed);
        _caves = new Caves(seed);
        _aquifers = new Aquifers(seed, SeaLevel);
        _bands = Bands(seed);
    }

    // Terracotta's colors by how often a band takes each, mostly the plain and the orange as the
    // source's badlands are, a band one to three blocks high, the whole cycle as high as the world.
    private static BlockId[] Bands(int seed)
    {
        (BlockId Block, int Weight)[] colors =
        [
            (BlockId.Terracotta, 7), (BlockId.OrangeTerracotta, 5), (BlockId.YellowTerracotta, 2), (BlockId.RedTerracotta, 2),
            (BlockId.BrownTerracotta, 1), (BlockId.WhiteTerracotta, 2), (BlockId.LightGrayTerracotta, 1),
        ];
        var total = colors.Sum(c => c.Weight);
        var bands = new BlockId[ChunkColumn.Ceiling];
        for (int y = 0; y < bands.Length;)
        {
            var hash = PlaceHash.Of(y, 0, seed ^ 0x7e2a);
            var pick = (int)(hash % (uint)total);
            var block = colors.First(c => (pick -= c.Weight) < 0).Block;
            for (int high = 1 + (int)(hash / 7 % 3); high > 0 && y < bands.Length; high--) bands[y++] = block;
        }
        return bands;
    }

    public string Name => "overworld";

    public int Seed { get; }

    /// <summary>
    /// The height of the ground's top block at a world column, its climate and its biome. A dry,
    /// hot climate lowers the hills and keeps the mountains away, so a desert lies flat and the land
    /// flattens smoothly as it nears one rather than at the biome's edge.
    /// </summary>
    public (int Height, float Temperature, float Humidity, Biome Biome) At(int x, int z)
    {
        var land = Shape(x, z);
        return (land.Height, land.Temperature, land.Humidity, land.Biome);
    }

    // A column's ground and climate, how far it is a swamp, the badlands and a river from 0 to 1,
    // and its biome.
    private readonly record struct Land(int Height, float Temperature, float Humidity, float Swamp, float Badlands, float River, Biome Biome);

    private Land Shape(int x, int z)
    {
        float temperature = _climate.Temperature(x, z), humidity = _climate.Humidity(x, z);
        float badlands = Climate.Badlands(temperature, humidity), swamp = Climate.Swampness(temperature, humidity);
        // The badlands keep their hills, which the desert around them loses.
        var dry = Climate.Dryness(temperature, humidity) * (1 - badlands);
        var hills = _hills.Fractal(x / 160f, z / 160f, 5) * 18 * (1 - 0.6f * dry);
        var mountains = Math.Max(0, _mountains.Fractal(x / 420f, z / 420f, 3)) * (1 - Climate.Dryness(temperature, humidity));
        // Wide basins sink the land under the sea level, the lakes and seas of the world, as
        // Minecraft's continentalness lowers its oceans.
        var basin = Math.Clamp((-_basins.Fractal(x / 520f, z / 520f, 3) - 0.1f) / 0.3f, 0, 1);
        var sunk = basin * basin * (3 - 2 * basin);
        var height = 56 + hills + mountains * mountains * 110 - sunk * 22;

        // The badlands rise in mesas, flat tops in steps of five up to thirty or so blocks, above the
        // ground where a noise is high.
        if (badlands > 0)
        {
            var mesa = Math.Max(0, _mesas.Fractal(x / 90f, z / 90f, 3) + 0.08f) * 95;
            var stepped = MathF.Floor(mesa / 5) * 5 + Math.Clamp((mesa % 5 - 4) * 5, 0, 5);
            height += badlands * stepped;
        }
        // A swamp sinks toward the sea level, a block or two over and under it, so pools stand
        // among its ground, the land around it sinking in with it.
        if (swamp > 0 && height > SeaLevel - 4)
            height += (SeaLevel + 0.5f + _swampFloor.At(x / 14f, z / 14f) * 2.4f - height) * swamp;

        // A river winds where a noise is near zero, in a valley that sinks the land around it most
        // of the way to the water, and cut down to three blocks under the sea level where it
        // runs, both fading out on high ground, as the source's rivers do not climb its mountains.
        var winding = MathF.Abs(_rivers.Fractal(x / 420f, z / 420f, 2));
        var below = SmoothStep(MountainsFrom, MountainsFrom - 16, height);
        var valley = SmoothStep(0.11f, 0.03f, winding) * below;
        if (height > SeaLevel + 1) height -= (height - (SeaLevel + 1)) * valley * 0.85f;
        var river = SmoothStep(0.032f, 0.01f, winding) * below;
        if (height > SeaLevel - 3) height -= (height - (SeaLevel - 3)) * river;

        var ground = Math.Clamp((int)height, 4, ChunkColumn.Ceiling - 12);
        var biome = river > 0.5f && ground < SeaLevel ? Biomes.River
            // A beach along the sea where the basin sinks the land, above the water and a little over it, but where it is cold.
            : sunk > 0.02f && ground >= SeaLevel - 1 && ground <= SeaLevel + 2 && temperature >= 0.3f && swamp < 0.5f && badlands < 0.5f ? Biomes.Beach
            : Climate.Choose(temperature, humidity, ground, MountainsFrom);
        return new Land(ground, temperature, humidity, swamp, badlands, river, biome);
    }

    private static float SmoothStep(float from, float to, float x)
    {
        var t = Math.Clamp((x - from) / (to - from), 0, 1);
        return t * t * (3 - 2 * t);
    }

    public void Paint(ChunkColumn column)
    {
        for (int z = 0; z < Section.Size; z++)
            for (int x = 0; x < Section.Size; x++)
            {
                int wx = column.X * Section.Size + x, wz = column.Z * Section.Size + z;
                Paint(column, x, z, wx, wz, Shape(wx, wz));
            }
    }

    // A place's biome and its grass's and leaves' colors, each biome's own blended by how far the
    // place is that biome, so the colors change smoothly across an edge where the biome changes at
    // once, the swamp's grass in the source's two greens by a noise.
    private void Paint(ChunkColumn column, int x, int z, int wx, int wz, Land land)
    {
        var at = z * Section.Size + x;
        column.Biome[at] = land.Biome.Id;
        var mottled = _mottle.At(wx / 44f, wz / 44f) < -0.1f ? new Color(76, 118, 60) : new Color(106, 112, 57);
        (column.GrassTint[at], column.FoliageTint[at]) = Climate.Colors(land.Temperature, land.Humidity, land.Height, MountainsFrom, mottled);
    }

    /// <summary>
    /// The ground at a world column, with its height, biome and climate, and its top block and the
    /// three under it. Low ground in a temperate biome lies in patches of sand, and mountains in
    /// patches of gravel. A feature or a structure starting in a column not generated reads its
    /// ground here.
    /// </summary>
    public (int Height, Biome Biome, BlockId Top, BlockId Under, float Temperature, float Humidity) Ground(int x, int z)
    {
        var (height, temperature, humidity, biome) = At(x, z);
        var temperate = biome == Biomes.Plains || biome == Biomes.Forest || biome == Biomes.BirchForest;
        var sandy = temperate && height < SandBelow && _sand.At(x / 48f, z / 48f) > 0.05f;
        // Ground under the water is sand near the shore and gravel deeper, as a sea's floor is, and
        // in patches clay under a river's or a swamp's shallow water, as the source lays its disks of clay.
        if (height < SeaLevel)
        {
            var floor = (biome == Biomes.River || biome == Biomes.Swamp) && _clay.At(x / 9f, z / 9f) > 0.25f ? BlockId.Clay
                : biome == Biomes.Swamp ? BlockId.Dirt
                : height >= SeaLevel - 4 ? BlockId.Sand : BlockId.Gravel;
            return (height, biome, floor, floor == BlockId.Clay ? BlockId.Clay : floor, temperature, humidity);
        }
        // A mesa's terracotta is bare, its top and the block under it the bands at their heights.
        if (biome == Biomes.Badlands && height > Mesas) return (height, biome, _bands[height], _bands[height - 1], temperature, humidity);
        var top = height >= SnowLine ? BlockId.Snow
            : sandy ? BlockId.Sand
            : biome == Biomes.Mountains && _gravel.At(x / 24f, z / 24f) > 0.3f ? BlockId.Gravel
            : biome.Top;
        return (height, biome, top, sandy ? BlockId.Sand : biome.Under, temperature, humidity);
    }

    /// <summary>The ground's top, or over land under the sea level the sea's, frozen where the climate is cold, as a column lays them.</summary>
    public (int Height, BlockId Top, byte Biome) Surface(int x, int z)
    {
        var (height, biome, top, _, temperature, _) = Ground(x, z);
        return height < SeaLevel ? (SeaLevel, temperature < 0.3f ? BlockId.Ice : BlockId.Water, biome.Id) : (height, top, biome.Id);
    }

    public ChunkColumn Generate(int columnX, int columnZ)
    {
        var column = new ChunkColumn(columnX, columnZ);
        int minX = columnX * Section.Size, minZ = columnZ * Section.Size;
        var villages = Structures.Village.GroundIn(minX, minZ, minX + Section.Mask, minZ + Section.Mask, this);
        var grounds = new (int Height, Biome Biome, BlockId Top)[Section.Size * Section.Size];
        var caves = _caves.Carve(columnX, columnZ, (x, z) => At(x, z).Height);
        var aquifers = _aquifers.For((x, z) => At(x, z).Height);
        for (int z = 0; z < Section.Size; z++)
            for (int x = 0; x < Section.Size; x++)
            {
                int wx = columnX * Section.Size + x, wz = columnZ * Section.Size + z;
                var (height, biome, top, under, temperature, _) = Ground(wx, wz);
                grounds[z * Section.Size + x] = (height, biome, top);
                // The caves open to the top where an entrance reaches it but under water, which they
                // would drain, and on a village's ground, which its roads and footings lie on.
                var open = height > SeaLevel + 1 && !Structures.Village.OnGround(villages, wx, wz);
                Paint(column, x, z, wx, wz, Shape(wx, wz));
                var banded = biome == Biomes.Badlands;
                column.Set(x, ChunkColumn.Bottom, z, BlockId.Bedrock);
                for (int y = ChunkColumn.Bottom + 1; y <= height; y++)
                {
                    // The badlands' terracotta lies in its bands down to the sea level, so a mesa's
                    // sides and top show them, red sand lying on the low ground alone, and stone below,
                    // which the source's deepslate takes the place of under 0, thinning out up to 8.
                    var block = y == height ? top
                        : banded && y > height - 2 ? under
                        : banded && y > SeaLevel - 8 ? _bands[y]
                        : y > height - 4 ? under
                        : biome.Deep != BlockId.Air && y > height - 8 ? biome.Deep
                        : Thins(wx, y, wz, 0, 8, 0x0de5) ? BlockId.Deepslate : BlockId.Stone;
                    // The source's bedrock thins out over the five blocks above the world's floor,
                    // and lies under the caves' carving, so no cave opens onto the void.
                    if (Thins(wx, y, wz, ChunkColumn.Bottom, ChunkColumn.Bottom + 5, 0x0bed))
                    {
                        column.Set(x, y, z, BlockId.Bedrock);
                        continue;
                    }
                    // The caves stay three blocks under the ground where they may not open, and
                    // elsewhere open where the source's entrances reach the top, a mouth the sun
                    // shines into. A cave holds its aquifer's water or lava under its level and air
                    // over it, or keeps the ground as a wall between two aquifers.
                    if ((y < height - 3 || open) && caves.DensityAt(x, y, z) is var density and <= 0 && aquifers.At(wx, y, wz, density) is { } held)
                    {
                        if (held != BlockId.Air) column.Set(x, y, z, held);
                        continue;
                    }
                    column.Set(x, y, z, block);
                }
                // Water over lower ground, its top frozen where the climate is cold.
                for (int y = height + 1; y <= SeaLevel; y++)
                    column.Set(x, y, z, y == SeaLevel && temperature < 0.3f ? BlockId.Ice : BlockId.Water);
            }

        Ores.Place(column, Seed);
        var clip = new ColumnClip(column);
        PlaceFeatures(clip, columnX, columnZ);
        PlacePatches(clip, grounds, columnX, columnZ);
        PlacePlants(clip, grounds);
        foreach (var structure in Structures.All) structure.BuildIn(clip, this);
        // A plant whose ground a tunnel's mouth carved away, or a ravine built after the plants, is
        // taken out, so none stands in the air.
        for (int z = 0; z < Section.Size; z++)
            for (int x = 0; x < Section.Size; x++)
            {
                var y = grounds[z * Section.Size + x].Height + 1;
                if (y < ChunkColumn.Ceiling && BlockGroups.Plants.Contains(column.Get(x, y, z)) && !Blocks.Collides(column.Get(x, y - 1, z)))
                    column.Set(x, y, z, BlockId.Air);
                // Tall grass stands whole or not at all, where a structure took one of its halves.
                if (y + 1 >= ChunkColumn.Ceiling) continue;
                var (lower, upper) = (column.Get(x, y, z) == BlockId.TallGrass, column.Get(x, y + 1, z) == BlockId.TallGrassTop);
                if (lower && !upper) column.Set(x, y, z, BlockId.Air);
                if (upper && !lower) column.Set(x, y + 1, z, BlockId.Air);
            }
        Herds.Place(column, Seed);
        return column;
    }

    // The features that reach past their place, the trees, that start in this column and in the eight
    // around it within reach of it, in one order every column keeps, so two that overlap overlap the
    // same way wherever they are placed.
    private void PlaceFeatures(ColumnClip clip, int columnX, int columnZ)
    {
        // The areas where structures keep trees out, which a tree reaching into them is kept out of too.
        var clearings = new List<(int X0, int Z0, int X1, int Z1)>();
        foreach (var structure in Structures.All)
            if (structure.ClearsTrees)
                foreach (var start in structure.StartsIn(clip.MinX - MostReach, clip.MinZ - MostReach, clip.MinX + Section.Mask + MostReach, clip.MinZ + Section.Mask + MostReach, this))
                    clearings.Add((start.X - MostReach, start.Z - MostReach, start.X + structure.Size - 1 + MostReach, start.Z + structure.Size - 1 + MostReach));

        for (int cz = columnZ - 1; cz <= columnZ + 1; cz++)
            for (int cx = columnX - 1; cx <= columnX + 1; cx++)
                for (int z = 0; z < Section.Size; z++)
                    for (int x = 0; x < Section.Size; x++)
                    {
                        int wx = cx * Section.Size + x, wz = cz * Section.Size + z;
                        var hash = PlaceHash.Of(wx, wz, Seed);
                        var roll = hash % 1000;
                        if (roll >= MostFeatures) continue;
                        if (wx < clip.MinX - MostReach || wx > clip.MinX + Section.Mask + MostReach
                            || wz < clip.MinZ - MostReach || wz > clip.MinZ + Section.Mask + MostReach) continue;
                        if (clearings.Exists(c => wx >= c.X0 && wx <= c.X1 && wz >= c.Z0 && wz <= c.Z1)) continue;
                        var (height, biome, top, _, _, _) = Ground(wx, wz);
                        foreach (var (feature, perThousand) in biome.Trees)
                        {
                            if (feature.Reach == 0) continue;
                            if (roll >= perThousand)
                            {
                                roll -= (uint)perThousand;
                                continue;
                            }
                            if (feature.GrowsOn(top)) feature.Place(clip, wx, height, wz, hash / 1000);
                            break;
                        }
                    }
    }

    // The source's patches of grass and flowers that start in this column's chunk and in the eight
    // around it, each patch kind in turn as the source's biomes list them, so a patch spreads across
    // a chunk's edge as the source's do while this column keeps the tries that land in it. A chunk's
    // patches of a kind are laid out by a random of their own and each patch's tries by another, so
    // every column lays out a patch the same, and one that cannot reach this column or stands in a
    // biome that does not grow it is passed over. A kind no biome near the column grows, those of its
    // own places and of eight places four blocks past its edges and corners, is passed over without
    // reading the ground under each patch, which the patches of the chunks around would cost the
    // column's generation nearly half again. A try places its plant where it finds air over grass at the
    // ground's own height, so on level land a try that rises or falls misses, as the source's do, and
    // tall grass where the air over that is free too.
    private void PlacePatches(ColumnClip clip, (int Height, Biome Biome, BlockId Top)[] grounds, int columnX, int columnZ)
    {
        var near = new HashSet<Biome>(grounds.Select(g => g.Biome));
        for (int dz = -1; dz <= 1; dz++)
            for (int dx = -1; dx <= 1; dx++)
                if (dx != 0 || dz != 0) near.Add(Ground(clip.MinX + 8 + dx * 12, clip.MinZ + 8 + dz * 12).Biome);
        for (int kind = 0; kind < PlantPatch.All.Length; kind++)
        {
            var patch = PlantPatch.All[kind];
            if (!near.Any(b => Array.Exists(b.Patches, p => ReferenceEquals(p, patch)))) continue;
            for (int cz = columnZ - 1; cz <= columnZ + 1; cz++)
                for (int cx = columnX - 1; cx <= columnX + 1; cx++)
                {
                    var random = new Random((int)PlaceHash.Of(cx, cz, Seed ^ (kind + 1) * 0x3C6EF372));
                    var count = patch.PatchesAt(cx * Section.Size, cz * Section.Size);
                    for (int i = 0; i < count; i++)
                    {
                        int ox = cx * Section.Size + random.Next(Section.Size), oz = cz * Section.Size + random.Next(Section.Size);
                        if (patch.Rarity > 1 && random.Next(patch.Rarity) != 0) continue;
                        if (ox + patch.Spread < clip.MinX || ox - patch.Spread > clip.MinX + Section.Mask
                            || oz + patch.Spread < clip.MinZ || oz - patch.Spread > clip.MinZ + Section.Mask) continue;
                        var (height, biome) = cx == columnX && cz == columnZ
                            ? (grounds[(oz - clip.MinZ) * Section.Size + ox - clip.MinX].Height, grounds[(oz - clip.MinZ) * Section.Size + ox - clip.MinX].Biome)
                            : Ground(ox, oz) is var (h, b, _, _, _, _) ? (h, b) : default;
                        if (!Array.Exists(biome.Patches, p => ReferenceEquals(p, patch))) continue;
                        var tries = new Random((int)PlaceHash.Of(ox, oz, Seed ^ (kind + 1) * 0x2545F491 ^ i));
                        for (int t = 0; t < patch.Tries; t++)
                        {
                            int x = ox + Spread(tries, patch.Spread), y = height + 1 + Spread(tries, patch.Rise), z = oz + Spread(tries, patch.Spread);
                            var plant = patch.Pick(tries, x, z);
                            if (x < clip.MinX || x > clip.MinX + Section.Mask || z < clip.MinZ || z > clip.MinZ + Section.Mask) continue;
                            var ground = grounds[(z - clip.MinZ) * Section.Size + x - clip.MinX];
                            if (y != ground.Height + 1 || y + 1 >= ChunkColumn.Ceiling || ground.Top != BlockId.Grass || clip.Get(x, y, z) != BlockId.Air) continue;
                            if (plant == BlockId.TallGrass)
                            {
                                if (clip.Get(x, y + 1, z) != BlockId.Air) continue;
                                clip.Set(x, y + 1, z, BlockId.TallGrassTop);
                            }
                            clip.Set(x, y, z, plant);
                        }
                    }
                }
        }
    }

    // A try's way across, along or up from its patch's middle, the sum of two whole numbers from 0 to
    // the spread less the spread, as the source's trapezoid of no plateau gives it.
    private static int Spread(Random random, int spread) => random.Next(spread + 1) + random.Next(spread + 1) - spread;

    // The plants and the other features that keep to their place, on a roll of their own at each place
    // of this column alone, on the ground it read for its blocks, after the trees, so a plant takes the
    // air beside a trunk and a meadow can be nearly covered with no tree the fewer for it.
    private void PlacePlants(ColumnClip clip, (int Height, Biome Biome, BlockId Top)[] grounds)
    {
        for (int z = 0; z < Section.Size; z++)
            for (int x = 0; x < Section.Size; x++)
            {
                int wx = clip.MinX + x, wz = clip.MinZ + z;
                var (height, biome, top) = grounds[z * Section.Size + x];
                var hash = PlaceHash.Of(wx, wz, Seed ^ 0x5a17);
                var roll = hash % 1000;
                foreach (var (feature, perThousand) in biome.Trees)
                {
                    if (feature.Reach > 0) continue;
                    if (roll >= perThousand)
                    {
                        roll -= (uint)perThousand;
                        continue;
                    }
                    if (feature.GrowsOn(top)) feature.Place(clip, wx, height, wz, hash / 1000);
                    break;
                }
            }
    }

    // The most trees any biome places in a thousand places, and the farthest any reaches, so a place
    // that cannot hold one or cannot reach the column is passed over without reading its ground.
    private static readonly int MostFeatures = Biomes.All.Max(b => b.Trees.Where(t => t.Feature.Reach > 0).Sum(t => t.PerThousand));
    private static readonly int MostReach = Biomes.All.SelectMany(b => b.Trees).Max(t => t.Feature.Reach);

    // Whether a block takes a layer that is whole up to one height and thins out to none at another,
    // as the source's vertical gradients do, its share falling evenly between them, by a hash of the
    // block's place, so deepslate meets the stone and bedrock the deepslate in a ragged edge.
    private bool Thins(int x, int y, int z, int whole, int none, int salt) =>
        y <= whole || y < none && PlaceHash.Of(x * 389 + y, z, Seed ^ salt) % 1000 < 1000 * (none - y) / (none - whole);

}
