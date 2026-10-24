using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>A kind of land, which the climate chooses, with the blocks on its ground and what grows on it.</summary>
/// <param name="Name">The name the settings window and <c>voxel.state</c> show.</param>
/// <param name="Top">The block on the ground.</param>
/// <param name="Under">The three blocks under it.</param>
/// <param name="Deep">The blocks under those down to the stone, none for stone at once.</param>
/// <param name="Trees">What grows on its ground and how often, in thousandths of the places where it can, the features that reach past their place, the trees, on one roll and those that keep to it, the plants, on another.</param>
public sealed record Biome(string Name, BlockId Top, BlockId Under, BlockId Deep, (IFeature Feature, int PerThousand)[] Trees)
{
    /// <summary>The biome's place in <see cref="Biomes.All"/>, which a column keeps for each of its places.</summary>
    public byte Id { get; init; }

    /// <summary>The animals that graze on it, one kind of which a column's herd is.</summary>
    public MobKind[] Animals { get; init; } = [];

    /// <summary>The color of its grass, as Minecraft's grass colormap gives it at the source biome's own temperature and downfall.</summary>
    public Color GrassColor { get; init; }

    /// <summary>The color of its oak leaves, as Minecraft's foliage colormap gives it there.</summary>
    public Color FoliageColor { get; init; }

    /// <summary>Whether its grass lies in patches of two greens a noise lays out, as Minecraft's swamp mottles its grass.</summary>
    public bool Mottled { get; init; }

    /// <summary>The source's patches of grass and flowers that grow in it, which the overworld lays out after the trees.</summary>
    public PlantPatch[] Patches { get; init; } = [];
}

/// <summary>Every biome, by its id.</summary>
public static class Biomes
{
    private static readonly IFeature Oak = new OakTree(), Birch = new BirchTree(), Spruce = new SpruceTree(), Cactus = new CactusFeature();
    private static readonly IFeature BigOak = new BigOakTree(), SmallJungle = new JungleTree(), MegaJungle = new MegaJungleTree(), Bush = new JungleBush(),
        Acacia = new AcaciaTree(), DarkOak = new DarkOakTree(), BrownMushroom = new GiantMushroom(false), RedMushroom = new GiantMushroom(true), Cherry = new CherryTree();

    // The grass and the flowers grow in the source's own patches (PlantPatch), and the mushrooms of
    // the ground and the dead bushes on a roll of their own at each place, a few kinds by weight.
    private static IFeature On(BlockId ground, params (BlockId, int)[] kinds) => new PlantFeature(kinds, ground);
    private static readonly IFeature Mushrooms = On(BlockId.Grass, (BlockId.BrownMushroom, 2), (BlockId.RedMushroom, 1));
    private static readonly IFeature DeadBushes = new PlantFeature([(BlockId.DeadBush, 1)], BlockId.Sand, BlockId.RedSand, BlockId.Terracotta,
        BlockId.WhiteTerracotta, BlockId.OrangeTerracotta, BlockId.YellowTerracotta, BlockId.RedTerracotta, BlockId.BrownTerracotta, BlockId.LightGrayTerracotta);
    private static readonly MobKind[] Farm = [MobKinds.Pig, MobKinds.Cow, MobKinds.Sheep, MobKinds.Chicken];

    public static readonly Biome Plains = new("plains", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Oak, 2), (BigOak, 1)]) { Patches = [PlantPatch.TallGrass2, PlantPatch.FlowerPlains, PlantPatch.GrassPlain], GrassColor = new Color(145, 189, 89), FoliageColor = new Color(119, 171, 47), Id = 0, Animals = Farm };
    public static readonly Biome Forest = new("forest", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Oak, 26), (BigOak, 4), (Birch, 8), (Mushrooms, 4)]) { Patches = [PlantPatch.FlowerDefault, PlantPatch.GrassForest], GrassColor = new Color(121, 192, 90), FoliageColor = new Color(89, 174, 48), Id = 1, Animals = Farm };
    public static readonly Biome BirchForest = new("birch forest", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Birch, 34)]) { Patches = [PlantPatch.FlowerDefault, PlantPatch.GrassForest], GrassColor = new Color(136, 187, 103), FoliageColor = new Color(107, 169, 65), Id = 2, Animals = Farm };
    public static readonly Biome Taiga = new("taiga", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Spruce, 28), (Mushrooms, 4)]) { Patches = [PlantPatch.FlowerDefault, PlantPatch.GrassTaiga2], GrassColor = new Color(134, 183, 131), FoliageColor = new Color(104, 164, 100), Id = 3, Animals = Farm };
    public static readonly Biome SnowyTaiga = new("snowy taiga", BlockId.Snow, BlockId.Dirt, BlockId.Air, [(Spruce, 22)]) { Patches = [PlantPatch.FlowerDefault, PlantPatch.GrassTaiga2], GrassColor = new Color(128, 180, 151), FoliageColor = new Color(96, 161, 123), Id = 4 };
    public static readonly Biome SnowyPlains = new("snowy plains", BlockId.Snow, BlockId.Dirt, BlockId.Air, [(Spruce, 2)]) { Patches = [PlantPatch.FlowerDefault, PlantPatch.GrassBadlands], GrassColor = new Color(128, 180, 151), FoliageColor = new Color(96, 161, 123), Id = 5 };
    public static readonly Biome Desert = new("desert", BlockId.Sand, BlockId.Sand, BlockId.Sandstone, [(Cactus, 6), (DeadBushes, 4)]) { Patches = [PlantPatch.FlowerDefault, PlantPatch.GrassBadlands], GrassColor = new Color(191, 183, 85), FoliageColor = new Color(174, 164, 42), Id = 6 };
    public static readonly Biome Mountains = new("mountains", BlockId.Stone, BlockId.Stone, BlockId.Air, [(Spruce, 3)]) { Patches = [PlantPatch.FlowerDefault, PlantPatch.GrassBadlands], GrassColor = new Color(138, 182, 137), FoliageColor = new Color(109, 163, 107), Id = 7, Animals = [MobKinds.Sheep] };

    // The seven of the source's biomes after them, their colors and how often their trees stand read
    // from Minecraft's numbers and the biome overhaul's, a tree a column in a chunk being four in a
    // thousand, fewer where a tree's crown is wide, since the game's trees grow into each other
    // where the source's leave out one that has no room.
    public static readonly Biome Swamp = new("swamp", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Oak, 14), (Mushrooms, 4)])
    {
        Patches = [PlantPatch.FlowerSwamp, PlantPatch.GrassNormal],
        Id = 8, GrassColor = new Color(106, 112, 57), FoliageColor = new Color(106, 112, 57), Mottled = true,
    };
    public static readonly Biome Savanna = new("savanna", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Acacia, 4)]) { Patches = [PlantPatch.TallGrass, PlantPatch.FlowerWarm, PlantPatch.GrassSavanna], GrassColor = new Color(191, 183, 85), FoliageColor = new Color(174, 164, 42), Id = 9, Animals = [MobKinds.Cow, MobKinds.Sheep] };
    public static readonly Biome Jungle = new("jungle", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(MegaJungle, 8), (SmallJungle, 30), (Bush, 30)]) { Patches = [PlantPatch.FlowerWarm, PlantPatch.GrassJungle], GrassColor = new Color(89, 201, 60), FoliageColor = new Color(48, 187, 11), Id = 10, Animals = [MobKinds.Chicken, MobKinds.Pig] };
    public static readonly Biome DarkForest = new("dark forest", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(DarkOak, 40), (BrownMushroom, 3), (RedMushroom, 3), (Oak, 6), (Birch, 4), (Mushrooms, 12)]) { Patches = [PlantPatch.FlowerDefault, PlantPatch.GrassForest], GrassColor = new Color(80, 122, 50), FoliageColor = new Color(89, 174, 48), Id = 11, Animals = Farm };
    public static readonly Biome Badlands = new("badlands", BlockId.RedSand, BlockId.RedSand, BlockId.Terracotta, [(Cactus, 3), (DeadBushes, 10)])
    {
        Patches = [PlantPatch.GrassBadlands],
        Id = 12, GrassColor = new Color(144, 129, 77), FoliageColor = new Color(158, 129, 77),
    };
    public static readonly Biome Beach = new("beach", BlockId.Sand, BlockId.Sand, BlockId.Sandstone, []) { Patches = [PlantPatch.FlowerDefault, PlantPatch.GrassBadlands], GrassColor = new Color(145, 189, 89), FoliageColor = new Color(119, 171, 47), Id = 13 };
    public static readonly Biome River = new("river", BlockId.Sand, BlockId.Sand, BlockId.Air, []) { Patches = [PlantPatch.FlowerDefault, PlantPatch.GrassBadlands], GrassColor = new Color(142, 185, 113), FoliageColor = new Color(113, 167, 77), Id = 14 };
    // The cherry grove on raised ground where it is cool and wet, its grass and leaves a light green
    // of their own, as the gallery of the owner's modpack shows it.
    public static readonly Biome CherryGrove = new("cherry grove", BlockId.Grass, BlockId.Dirt, BlockId.Air, [(Cherry, 14)])
    {
        Patches = [PlantPatch.TallGrass2, PlantPatch.GrassPlain, PlantPatch.FlowerCherry],
        Id = 15, GrassColor = new Color(182, 219, 97), FoliageColor = new Color(182, 219, 97), Animals = [MobKinds.Pig, MobKinds.Sheep],
    };

    public static readonly Biome[] All = [Plains, Forest, BirchForest, Taiga, SnowyTaiga, SnowyPlains, Desert, Mountains, Swamp, Savanna, Jungle, DarkForest, Badlands, Beach, River, CherryGrove];
}

/// <summary>
/// The temperature and humidity of every place, from 0 to 1, which choose its biome and tint its
/// grass and leaves.
/// </summary>
/// <remarks>
/// Both vary over hundreds of blocks, so the tint of grass and leaves, each biome's own blended by
/// how far a place is each biome, changes smoothly across a biome's edge where the biome itself
/// changes at once. Read only once made, so the workers share one.
/// </remarks>
public sealed class Climate(int seed)
{
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
        // High ground where it is hot and dry is the badlands, as the source's hot peaks are, never
        // mountains of stone and spruces on a mesa's edge.
        if (height >= mountainsFrom) return temperature > 0.7f && humidity < 0.4f ? Biomes.Badlands : Biomes.Mountains;
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

    // How far about each of Choose's bounds its choice is made soft for the colors, in the climate's
    // units, which spreads a biome's edge over about twenty blocks, as Minecraft blends its colors
    // over the biomes around a place.
    private const float Soft = 0.02f;

    /// <summary>
    /// The grass's and the leaves' colors of a place, each biome's own blended by how far the place is
    /// that biome, its share from <see cref="Choose"/> made soft about each of its bounds, the swamp's
    /// grass the green its mottle lays there.
    /// </summary>
    public static (Color Grass, Color Foliage) Colors(float t, float h, int height, int mountainsFrom, Color swampGrass)
    {
        Vector3 grass = Vector3.Zero, foliage = Vector3.Zero;
        void Add(Biome biome, float share, Color? grassColor = null)
        {
            if (share <= 0) return;
            var g = grassColor ?? biome.GrassColor;
            grass += share * new Vector3(g.R, g.G, g.B);
            foliage += share * new Vector3(biome.FoliageColor.R, biome.FoliageColor.G, biome.FoliageColor.B);
        }
        float Above(float x, float at) => SmoothStep(at - Soft, at + Soft, x);

        var mountains = SmoothStep(mountainsFrom - 3, mountainsFrom + 3, height);
        Add(Biomes.Mountains, mountains);
        var rest = 1 - mountains;
        var cold = 1 - Above(t, 0.3f);
        var wet = Above(h, 0.4f);
        Add(Biomes.SnowyTaiga, rest * cold * wet * (1 - Above(t, 0.18f)));
        Add(Biomes.Taiga, rest * cold * wet * Above(t, 0.18f));
        Add(Biomes.SnowyPlains, rest * cold * (1 - wet));
        rest *= 1 - cold;
        var badlands = Badlands(t, h);
        Add(Biomes.Badlands, rest * badlands);
        rest *= 1 - badlands;
        var desert = Above(t, 0.7f) * (1 - Above(h, 0.4f));
        Add(Biomes.Desert, rest * desert);
        rest *= 1 - desert;
        var swamp = Swampness(t, h);
        Add(Biomes.Swamp, rest * swamp, swampGrass);
        rest *= 1 - swamp;
        float jungle = Above(t, 0.62f) * Above(h, 0.66f), savanna = Above(t, 0.62f) * (1 - Above(h, 0.5f));
        Add(Biomes.Jungle, rest * jungle);
        Add(Biomes.Savanna, rest * savanna);
        rest *= 1 - jungle - savanna;
        var cherry = Cherry(t, h) * Raised(height, mountainsFrom);
        Add(Biomes.CherryGrove, rest * cherry);
        rest *= 1 - cherry;
        float dark = Above(h, 0.76f), forest = Above(h, 0.62f) * (1 - dark), birch = Above(h, 0.48f) * (1 - Above(h, 0.62f)) * (1 - Above(t, 0.6f));
        Add(Biomes.DarkForest, rest * dark);
        Add(Biomes.Forest, rest * forest);
        Add(Biomes.BirchForest, rest * birch);
        Add(Biomes.Plains, rest * (1 - dark - forest - birch));
        static Color Of(Vector3 c) => new((byte)MathF.Round(c.X), (byte)MathF.Round(c.Y), (byte)MathF.Round(c.Z));
        return (Of(grass), Of(foliage));
    }

    private static float SmoothStep(float from, float to, float x)
    {
        var t = Math.Clamp((x - from) / (to - from), 0, 1);
        return t * t * (3 - 2 * t);
    }
}
