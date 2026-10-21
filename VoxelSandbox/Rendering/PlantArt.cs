namespace VoxelSandbox;

/// <summary>
/// The game's own pixels for its plants where no pack gives them a texture, sixteen square, white
/// where the plant is and clear around it, tinted by the plant's own color as its faces are, so a
/// blade of grass or a flower is cut out of its crossed walls rather than drawn as two panels.
/// </summary>
/// <remarks>
/// A plant's walls take the rows of its tile from the cell's foot up, as a pack's texture lies on
/// them, so a mask draws its plant at the bottom: a flower's stem in the rows its stem's walls
/// cover and its petals in the rows above, a mushroom's stem and cap the same, and grass in blades
/// of heights a hash of their column gives. A lantern's iron takes one too, for its handle's and
/// chain's crossed walls, which are cut out as a plant's are.
/// </remarks>
public static class PlantArt
{
    private const int Size = 16;

    private static readonly Dictionary<int, byte[]> _masks = Build();

    /// <summary>A surface's mask, or null for one that is not a plant's.</summary>
    public static byte[]? Mask(int surface) => _masks.GetValueOrDefault(surface);

    private static Dictionary<int, byte[]> Build()
    {
        var masks = new Dictionary<int, byte[]>();
        byte[] Paint(Func<int, int, bool> at)
        {
            var pixels = new byte[Size * Size * 4];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                    if (at(x, y)) pixels.AsSpan((y * Size + x) * 4, 4).Fill(255);
            return pixels;
        }
        int Hash(int x, int salt) => (int)(PlaceHash.Of(x, salt, 0x9a17) % 1000);

        // Blades of grass in every other column, each as high as a hash of its column gives, the
        // tall grass's taller and the fern's with a leaflet out to each side halfway up.
        byte[] Blades(int lowest, int range, int salt, bool leaflets = false) => Paint((x, y) =>
        {
            var high = lowest + Hash(x / 2, salt) % range;
            if ((x + Hash(x / 4, salt + 1)) % 2 == 0 && y >= Size - high) return true;
            return leaflets && y == Size - high / 2 && x % 4 == 1;
        });
        masks[Surfaces.ShortGrass] = Blades(4, 8, 1);
        masks[Surfaces.TallGrass] = Blades(9, 7, 2);
        masks[Surfaces.Fern] = Blades(5, 8, 3, leaflets: true);

        // A flower's stem up its middle with a leaf to one side, below the rows of its petals, a
        // round head of five across.
        var stem = Paint((x, y) => y >= 10 && (x is 7 or 8 || y == 12 && x is 9 or 10));
        var petals = Paint((x, y) => y is >= 5 and <= 9 && (x - 7.5f) * (x - 7.5f) + (y - 7f) * (y - 7f) <= 7);
        foreach (var (stemSurface, petalSurface) in (ReadOnlySpan<(int, int)>)
            [(Surfaces.DandelionStem, Surfaces.Dandelion), (Surfaces.PoppyStem, Surfaces.Poppy), (Surfaces.BlueOrchidStem, Surfaces.BlueOrchid),
             (Surfaces.AlliumStem, Surfaces.Allium), (Surfaces.AzureBluetStem, Surfaces.AzureBluet), (Surfaces.RedTulipStem, Surfaces.RedTulip),
             (Surfaces.OrangeTulipStem, Surfaces.OrangeTulip), (Surfaces.WhiteTulipStem, Surfaces.WhiteTulip), (Surfaces.PinkTulipStem, Surfaces.PinkTulip),
             (Surfaces.OxeyeDaisyStem, Surfaces.OxeyeDaisy), (Surfaces.CornflowerStem, Surfaces.Cornflower), (Surfaces.LilyOfTheValleyStem, Surfaces.LilyOfTheValley)])
            (masks[stemSurface], masks[petalSurface]) = (stem, petals);

        // A sapling's thin trunk under a round crown.
        var sapling = Paint((x, y) => y >= 11 && x is 7 or 8 || y is >= 3 and <= 11 && (x - 7.5f) * (x - 7.5f) + (y - 7f) * (y - 7f) <= 16);
        foreach (var surface in (ReadOnlySpan<int>)[Surfaces.OakSapling, Surfaces.BirchSapling, Surfaces.SpruceSapling, Surfaces.JungleSapling,
                     Surfaces.AcaciaSapling, Surfaces.DarkOakSapling, Surfaces.CherrySapling])
            masks[surface] = sapling;

        // A mushroom's short stem in the rows its stem's walls cover and its cap above them.
        var mushroomStem = Paint((x, y) => y >= 13 && x is >= 7 and <= 8);
        var cap = Paint((x, y) => y is >= 9 and <= 12 && Math.Abs(x - 7.5f) <= 4 - (12 - y) * 0.5f);
        (masks[Surfaces.BrownMushroomStem], masks[Surfaces.RedMushroomStem], masks[Surfaces.BrownMushroom], masks[Surfaces.RedMushroom]) = (mushroomStem, mushroomStem, cap, cap);

        // A dead bush's bare twigs, forking up and out from its foot.
        masks[Surfaces.DeadBush] = Paint((x, y) => y >= 4 && (Math.Abs(x - 7.5f) - (15 - y) * 0.45f) is > -0.8f and < 0.8f || x is 7 or 8 && y >= 11);

        // Petals scattered over the ground in clusters of two by two, laid on the carpet's top.
        masks[Surfaces.PinkPetals] = Paint((x, y) => Hash(x / 2 + y / 2 * 8, 4) % 3 == 0);

        // Wheat in stalks, every other column, with ears at their tops.
        masks[Surfaces.Wheat] = Paint((x, y) => x % 3 != 2 && y >= 2 + Hash(x, 5) % 3);

        // A lantern's iron, whole but in columns 11 to 13 of rows 1 to 11, where the source's model
        // lays its handle's and chain's crossed walls, which hold an arch over the cap or a chain's
        // links, each wall's rows its own, rather than two panels of iron.
        string[] links = ["###", "#.#", "#.#", "###", "...", "#.#", "###", "...", "...", "###", "#.#"];
        var iron = Paint((x, y) => x is < 11 or > 13 || y is < 1 or > 11 || links[y - 1][x - 11] == '#');
        (masks[Surfaces.LanternIron], masks[Surfaces.SoulLanternIron]) = (iron, iron);
        return masks;
    }
}
