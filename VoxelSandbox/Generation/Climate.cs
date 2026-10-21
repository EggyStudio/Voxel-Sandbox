using Engine;

namespace VoxelSandbox;

/// <summary>A kind of land, which the climate chooses, with the blocks on its ground and what grows on it.</summary>
/// <param name="Name">The name the settings window and <c>voxel.state</c> show.</param>
/// <param name="Top">The block on the ground.</param>
/// <param name="Under">The three blocks under it.</param>
/// <param name="Deep">The blocks under those down to the stone, none for stone at once.</param>
/// <param name="Trees">What grows on its ground and how often, in thousandths of the columns where it can.</param>
public sealed record Biome(string Name, BlockId Top, BlockId Under, BlockId Deep, (IFeature Feature, int PerThousand)[] Trees)
{
    /// <summary>The biome's place in <see cref="Biomes.All"/>, which a column keeps for each of its places.</summary>
    public byte Id { get; init; }

    /// <summary>The animals that graze on it, one kind of which a column's herd is.</summary>
    public MobKind[] Animals { get; init; } = [];

    /// <summary>The color of its grass where it has one of its own rather than the climate's, as the badlands' is.</summary>
    public Color? GrassColor { get; init; }

    /// <summary>The color of its leaves where it has one of its own rather than the climate's, as a swamp's and the badlands' are.</summary>
    public Color? FoliageColor { get; init; }

    /// <summary>How it changes the climate's color of its grass, as a dark forest darkens it and a swamp mottles it.</summary>
    public GrassShade Shade { get; init; }
}

/// <summary>How a biome changes the climate's color of its grass, as Minecraft's dark forest and swamp do.</summary>
public enum GrassShade
{
    None,

    /// <summary>Halfway to a dark olive, as Minecraft's dark forest darkens its grass.</summary>
    DarkForest,

    /// <summary>In patches of two greens a noise lays out, as Minecraft's swamp mottles its grass.</summary>
    Swamp,
}

/// <summary>Every biome, by its id.</summary>
public static class Biomes
{
    private static readonly IFeature Oak = new OakTree(), Birch = new BirchTree(), Spruce = new SpruceTree(), Cactus = new CactusFeature();

    // The plants of the ground, each a few kinds by weight, as the source's patches of grass and its
    // flowers of each biome are.
    private static IFeature On(BlockId ground, params (BlockId, int)[] kinds) => new PlantFeature(kinds, ground);
    private static readonly IFeature Grasses = On(BlockId.Grass, (BlockId.ShortGrass, 6), (BlockId.TallGrass, 1));
    private static readonly IFeature Ferns = On(BlockId.Grass, (BlockId.Fern, 3), (BlockId.ShortGrass, 1));
    private static readonly IFeature Meadow = On(BlockId.Grass, (BlockId.Dandelion, 4), (BlockId.Poppy, 4), (BlockId.OxeyeDaisy, 2), (BlockId.AzureBluet, 2),
        (BlockId.Cornflower, 2), (BlockId.RedTulip, 1), (BlockId.OrangeTulip, 1), (BlockId.WhiteTulip, 1), (BlockId.PinkTulip, 1));
    private static readonly IFeature Woodland = On(BlockId.Grass, (BlockId.LilyOfTheValley, 2), (BlockId.Dandelion, 3), (BlockId.Poppy, 3), (BlockId.Allium, 1));
    private static readonly IFeature Warm = On(BlockId.Grass, (BlockId.Dandelion, 2), (BlockId.Poppy, 2), (BlockId.OrangeTulip, 1));
    private static readonly IFeature Mushrooms = On(BlockId.Grass, (BlockId.BrownMushroom, 2), (BlockId.RedMushroom, 1));
    private static readonly IFeature Orchids = On(BlockId.Grass, (BlockId.BlueOrchid, 1));
    private static readonly IFeature Petals = On(BlockId.Grass, (BlockId.PinkPetals, 1));
    private static readonly IFeature Blossoms = On(BlockId.Grass, (BlockId.Allium, 1), (BlockId.PinkTulip, 2));
    private static readonly IFeature DeadBushes = new PlantFeature([(BlockId.DeadBush, 1)], BlockId.Sand, BlockId.RedSand, BlockId.Terracotta,
        BlockId.WhiteTerracotta, BlockId.OrangeTerracotta, BlockId.YellowTerracotta, BlockId.RedTerracotta, BlockId.BrownTerracotta, BlockId.LightGrayTerracotta);
    private static readonly MobKind[] Farm = [MobKinds.Pig, MobKinds.Cow, MobKinds.Sheep, MobKinds.Chicken];

    public static readonly Biome Plains = new("plains", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Oak, 3), (Grasses, 140), (Meadow, 14)]) { Id = 0, Animals = Farm };
    public static readonly Biome Forest = new("forest", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Oak, 30), (Birch, 8), (Grasses, 50), (Woodland, 8), (Mushrooms, 2)]) { Id = 1, Animals = Farm };
    public static readonly Biome BirchForest = new("birch forest", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Birch, 34), (Grasses, 60), (Woodland, 8)]) { Id = 2, Animals = Farm };
    public static readonly Biome Taiga = new("taiga", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Spruce, 28), (Ferns, 40), (Mushrooms, 3)]) { Id = 3, Animals = Farm };
    public static readonly Biome SnowyTaiga = new("snowy taiga", BlockId.Snow, BlockId.Dirt, BlockId.Air, [(Spruce, 22)]) { Id = 4 };
    public static readonly Biome SnowyPlains = new("snowy plains", BlockId.Snow, BlockId.Dirt, BlockId.Air, [(Spruce, 2)]) { Id = 5 };
    public static readonly Biome Desert = new("desert", BlockId.Sand, BlockId.Sand, BlockId.Sandstone, [(Cactus, 6), (DeadBushes, 4)]) { Id = 6 };
    public static readonly Biome Mountains = new("mountains", BlockId.Stone, BlockId.Stone, BlockId.Air, [(Spruce, 3), (Grasses, 10)]) { Id = 7, Animals = [MobKinds.Sheep] };

    // The seven of the source's biomes after them, their colors and how often their trees stand read
    // from Minecraft's numbers and the biome overhaul's, a tree a column in a chunk being four in a
    // thousand. Their own trees take the oak's and the birch's place until they are grown.
    public static readonly Biome Swamp = new("swamp", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Oak, 14), (Grasses, 40), (Orchids, 6), (Mushrooms, 4)])
    {
        Id = 8, FoliageColor = new Color(106, 112, 57), Shade = GrassShade.Swamp,
    };
    public static readonly Biome Savanna = new("savanna", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Oak, 4), (Grasses, 180), (Warm, 3)]) { Id = 9, Animals = [MobKinds.Cow, MobKinds.Sheep] };
    public static readonly Biome Jungle = new("jungle", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Oak, 90), (Grasses, 80), (Ferns, 40), (Warm, 2)]) { Id = 10, Animals = [MobKinds.Chicken, MobKinds.Pig] };
    public static readonly Biome DarkForest = new("dark forest", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Oak, 56), (Birch, 8), (Mushrooms, 12), (Grasses, 20), (Woodland, 2)]) { Id = 11, Shade = GrassShade.DarkForest, Animals = Farm };
    public static readonly Biome Badlands = new("badlands", BlockId.RedSand, BlockId.RedSand, BlockId.Terracotta, [(Cactus, 3), (DeadBushes, 10)])
    {
        Id = 12, GrassColor = new Color(144, 129, 77), FoliageColor = new Color(158, 129, 77),
    };
    public static readonly Biome Beach = new("beach", BlockId.Sand, BlockId.Sand, BlockId.Sandstone, []) { Id = 13 };
    public static readonly Biome River = new("river", BlockId.Sand, BlockId.Sand, BlockId.Air, []) { Id = 14 };
    // The cherry grove on raised ground where it is cool and wet, its grass and leaves a light green
    // of their own, as the gallery of the owner's modpack shows it.
    public static readonly Biome CherryGrove = new("cherry grove", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Oak, 40), (Petals, 150), (Grasses, 40), (Blossoms, 3)])
    {
        Id = 15, GrassColor = new Color(182, 219, 97), FoliageColor = new Color(182, 219, 97), Animals = [MobKinds.Pig, MobKinds.Sheep],
    };

    public static readonly Biome[] All = [Plains, Forest, BirchForest, Taiga, SnowyTaiga, SnowyPlains, Desert, Mountains, Swamp, Savanna, Jungle, DarkForest, Badlands, Beach, River, CherryGrove];
}

/// <summary>
/// The temperature and humidity of every place, from 0 to 1, which choose its biome and tint its
/// grass and leaves.
/// </summary>
/// <remarks>
/// Both vary over hundreds of blocks, so the tint of grass and leaves, which is a color looked up
/// from the two as Minecraft's colormaps are, changes smoothly across a biome's edge where the
/// biome itself changes at once. Read only once made, so the workers share one.
/// </remarks>
public sealed class Climate(int seed)
{
    /// <summary>The plains' temperature and humidity, which a tinted block's hotbar color is shown at.</summary>
    public const float PlainsTemperature = 0.65f, PlainsHumidity = 0.45f;

    private readonly Noise _temperature = new(seed + 10), _humidity = new(seed + 11);

    public float Temperature(int x, int z) => Math.Clamp(0.5f + 0.9f * _temperature.Fractal(x / 700f, z / 700f, 3), 0, 1);

    public float Humidity(int x, int z) => Math.Clamp(0.5f + 0.9f * _humidity.Fractal(x / 600f, z / 600f, 3), 0, 1);

    /// <summary>How flat the land lies from 0 to 1, high where it is hot and dry, which lowers the hills of a desert as it nears one.</summary>
    public static float Dryness(float temperature, float humidity) => SmoothStep(0.6f, 0.85f, temperature) * SmoothStep(0.45f, 0.2f, humidity);

    /// <summary>How far a place is the badlands from 0 to 1, hottest and driest, inside the desert, whose mesas rise where it is.</summary>
    public static float Badlands(float temperature, float humidity) => SmoothStep(0.7f, 0.76f, temperature) * SmoothStep(0.3f, 0.22f, humidity);

    /// <summary>How far a place is a swamp from 0 to 1, warm and wettest, whose land sinks toward the sea where it is.</summary>
    public static float Swampness(float temperature, float humidity) =>
        SmoothStep(0.7f, 0.78f, humidity) * SmoothStep(0.48f, 0.54f, temperature) * SmoothStep(0.74f, 0.68f, temperature);

    /// <summary>How far a place's climate is a cherry grove's from 0 to 1, cool to temperate and wet, which raised ground there is.</summary>
    public static float Cherry(float temperature, float humidity) =>
        SmoothStep(0.33f, 0.38f, temperature) * SmoothStep(0.54f, 0.49f, temperature) * SmoothStep(0.5f, 0.56f, humidity) * SmoothStep(0.74f, 0.68f, humidity);

    /// <summary>How far ground is raised for a cherry grove from 0 to 1, from 20 under the mountains to 14 under them.</summary>
    public static float Raised(int height, int mountainsFrom) => SmoothStep(mountainsFrom - 20, mountainsFrom - 14, height);

    /// <summary>
    /// The biome of a place by its climate and the height of its ground, as Minecraft's table of
    /// temperatures and humidities chooses its biomes: the cold ones, then where it is hot the
    /// badlands within the desert, the swamp where it is warm and wettest, the jungle where it is hot
    /// and wet and the savanna where it is hot and dry, and where it is temperate the cherry grove
    /// on raised ground, then the dark forest, the forest, the birch forest and the plains, from wet
    /// to dry.
    /// </summary>
    public static Biome Choose(float temperature, float humidity, int height, int mountainsFrom)
    {
        if (height >= mountainsFrom) return Biomes.Mountains;
        if (temperature < 0.3f)
            return humidity > 0.4f ? (temperature < 0.18f ? Biomes.SnowyTaiga : Biomes.Taiga) : Biomes.SnowyPlains;
        if (Badlands(temperature, humidity) > 0.5f) return Biomes.Badlands;
        if (temperature > 0.7f && humidity < 0.4f) return Biomes.Desert;
        if (Swampness(temperature, humidity) > 0.5f) return Biomes.Swamp;
        if (temperature > 0.62f)
        {
            if (humidity > 0.66f) return Biomes.Jungle;
            if (humidity < 0.5f) return Biomes.Savanna;
        }
        if (Cherry(temperature, humidity) * Raised(height, mountainsFrom) > 0.5f) return Biomes.CherryGrove;
        if (humidity > 0.76f) return Biomes.DarkForest;
        if (humidity > 0.62f) return Biomes.Forest;
        if (humidity > 0.48f && temperature < 0.6f) return Biomes.BirchForest;
        return Biomes.Plains;
    }

    // The colors at the corners of Minecraft's grass and foliage colormaps, cold and dry, cold and
    // wet, hot and dry, and hot and wet, between which a place's color is blended.
    private static readonly Color[] GrassCorners = [new(128, 180, 151), new(134, 183, 131), new(191, 183, 85), new(89, 201, 60)];
    private static readonly Color[] FoliageCorners = [new(96, 161, 123), new(104, 164, 100), new(174, 164, 42), new(48, 187, 43)];

    public static Color Grass(float temperature, float humidity) => Blend(GrassCorners, temperature, humidity);

    public static Color Foliage(float temperature, float humidity) => Blend(FoliageCorners, temperature, humidity);

    private static Color Blend(Color[] corners, float t, float h)
    {
        static float Lerp(float a, float b, float s) => a + (b - a) * s;
        byte Channel(Func<Color, byte> of) =>
            (byte)Lerp(Lerp(of(corners[0]), of(corners[1]), h), Lerp(of(corners[2]), of(corners[3]), h), t);
        return new Color(Channel(c => c.R), Channel(c => c.G), Channel(c => c.B));
    }

    private static float SmoothStep(float from, float to, float x)
    {
        var t = Math.Clamp((x - from) / (to - from), 0, 1);
        return t * t * (3 - 2 * t);
    }
}
