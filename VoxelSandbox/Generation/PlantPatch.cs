namespace VoxelSandbox;

/// <summary>
/// A patch of plants as one of the source's placed features lays it, its numbers read from
/// Minecraft's worldgen data, so many patches a chunk, each at a place of the chunk on its ground,
/// and around each so many tries, a plant where a try finds air over grass.
/// </summary>
/// <param name="Count">How many patches a chunk holds, or where the noise is below -0.8 at the chunk's corner <paramref name="BelowNoise"/> when that is not negative.</param>
/// <param name="BelowNoise">How many patches a chunk holds where the source's noise of the biomes' details stands under -0.8, or -1 for a count the noise does not change.</param>
/// <param name="Rarity">How many patches there are for each one kept, 1 for every one.</param>
/// <param name="Tries">How many tries a patch makes.</param>
/// <param name="Spread">How far across and along a try goes at most, by the sum of two whole numbers from 0 to it less it, so more land near the patch's middle than at its edge.</param>
/// <param name="Rise">How far up or down a try goes at most, the same way, so on level land most miss the ground.</param>
/// <param name="Pick">The plant a try places, from a random and its place.</param>
public sealed record PlantPatch(int Count, int BelowNoise, int Rarity, int Tries, int Spread, int Rise, Func<Random, int, int, BlockId> Pick)
{
    // The source's noise of the biomes' details, read at a two-hundredth of a block, which sets how
    // many patches some chunks hold, and the one its plains' flowers read at a two-hundredth too,
    // under -0.8 of which they are tulips.
    private static readonly Noise Details = new(2345), Tulips = new(2346);

    /// <summary>How many patches the chunk whose corner is at a place holds.</summary>
    public int PatchesAt(int x, int z) => BelowNoise >= 0 && Details.At(x / 200f, z / 200f) < -0.8f ? BelowNoise : Count;

    // A plant of a few kinds by weight.
    private static Func<Random, int, int, BlockId> Of(params (BlockId Plant, int Weight)[] kinds)
    {
        var total = kinds.Sum(k => k.Weight);
        return (random, _, _) =>
        {
            var pick = random.Next(total);
            foreach (var (plant, weight) in kinds)
                if ((pick -= weight) < 0) return plant;
            return kinds[^1].Plant;
        };
    }

    private static readonly BlockId[] PlainsTulips = [BlockId.OrangeTulip, BlockId.RedTulip, BlockId.PinkTulip, BlockId.WhiteTulip];
    private static readonly BlockId[] PlainsFlowers = [BlockId.Poppy, BlockId.AzureBluet, BlockId.OxeyeDaisy, BlockId.Cornflower];

    // The source's flowers of the plains, tulips where its noise is under -0.8, and elsewhere a
    // dandelion, or one time in three a poppy, an azure bluet, an oxeye daisy or a cornflower.
    private static BlockId PlainsFlower(Random random, int x, int z) =>
        Tulips.At(x * 0.005f, z * 0.005f) < -0.8f ? PlainsTulips[random.Next(PlainsTulips.Length)]
            : random.NextSingle() < 1 / 3f ? PlainsFlowers[random.Next(PlainsFlowers.Length)] : BlockId.Dandelion;

    // The source's patches the game's biomes take, in the order its biomes place them, each named as
    // its placed feature is.
    public static readonly PlantPatch TallGrass2 = new(7, 0, 32, 96, 7, 3, Of((BlockId.TallGrass, 1)));
    public static readonly PlantPatch TallGrass = new(1, -1, 5, 96, 7, 3, Of((BlockId.TallGrass, 1)));
    public static readonly PlantPatch FlowerPlains = new(4, 15, 32, 64, 6, 2, PlainsFlower);
    public static readonly PlantPatch FlowerDefault = new(1, -1, 32, 64, 7, 3, Of((BlockId.Poppy, 2), (BlockId.Dandelion, 1)));
    public static readonly PlantPatch FlowerWarm = new(1, -1, 16, 64, 7, 3, Of((BlockId.Poppy, 2), (BlockId.Dandelion, 1)));
    public static readonly PlantPatch FlowerSwamp = new(1, -1, 32, 64, 6, 2, Of((BlockId.BlueOrchid, 1)));
    public static readonly PlantPatch FlowerCherry = new(10, 5, 1, 96, 6, 2, Of((BlockId.PinkPetals, 1)));
    public static readonly PlantPatch GrassPlain = new(10, 5, 1, 32, 7, 3, Of((BlockId.ShortGrass, 1)));
    public static readonly PlantPatch GrassForest = new(2, -1, 1, 32, 7, 3, Of((BlockId.ShortGrass, 1)));
    public static readonly PlantPatch GrassNormal = new(5, -1, 1, 32, 7, 3, Of((BlockId.ShortGrass, 1)));
    public static readonly PlantPatch GrassSavanna = new(20, -1, 1, 32, 7, 3, Of((BlockId.ShortGrass, 1)));
    public static readonly PlantPatch GrassTaiga2 = new(1, -1, 1, 32, 7, 3, Of((BlockId.ShortGrass, 1), (BlockId.Fern, 4)));
    public static readonly PlantPatch GrassJungle = new(25, -1, 1, 32, 7, 3, Of((BlockId.ShortGrass, 3), (BlockId.Fern, 1)));
    public static readonly PlantPatch GrassBadlands = new(1, -1, 1, 32, 7, 3, Of((BlockId.ShortGrass, 1)));

    /// <summary>Every patch, in the order the overworld lays them out, the tall grass and the flowers before the grass, as the source's biomes list them.</summary>
    public static readonly PlantPatch[] All = [TallGrass2, TallGrass, FlowerPlains, FlowerDefault, FlowerWarm, FlowerSwamp, FlowerCherry, GrassPlain, GrassForest, GrassNormal, GrassSavanna, GrassTaiga2, GrassJungle, GrassBadlands];
}
