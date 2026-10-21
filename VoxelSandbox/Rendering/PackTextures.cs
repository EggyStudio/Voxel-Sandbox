using Engine;

namespace VoxelSandbox;

/// <summary>
/// The texture of a resource pack each surface and item is drawn with, by the name Minecraft gives
/// it under <c>assets/minecraft/textures</c>, a table the game keeps, since its blocks are its own
/// and only some of them have a likeness in the source.
/// </summary>
/// <remarks>
/// A surface the table leaves out keeps the game's own color with a pack loaded, as does an end
/// rod, whose texture Minecraft lays out by a model of its own. A torch's and a lantern's pieces
/// take their textures by the parts the source's models give their faces, the lit flame and glass
/// drawn as emissive cubes showing theirs. A furnace and a crafting table have no front, so each of
/// their sides takes the front's texture, which tells them apart from stone and planks.
/// </remarks>
public static class PackTextures
{
    /// <summary>
    /// A surface's texture, an overlay over it that the climate's grass color tints, as the grass
    /// block's side is drawn, leaving the texture under it untinted, and a color the texture is
    /// tinted by where Minecraft tints it by one of its own, as its water and its birch and spruce
    /// leaves are.
    /// </summary>
    /// <remarks>
    /// Where <paramref name="Looks"/> is set, the game's own looks of <see cref="GroundArt"/> lie over
    /// the texture, each picked by the block's place. A pack gives pictures alone, and the game
    /// reads nothing of a pack's block states or models, which hold how a block is drawn and
    /// chosen, since how the game draws and chooses is its own.
    /// </remarks>
    public readonly record struct Look(string Texture, string? Overlay = null, Color? Tint = null, bool Looks = false);

    private static readonly Dictionary<int, Look> _surfaces = new()
    {
        [Surfaces.Grass] = new("block/grass_block_top", Looks: true),
        [Surfaces.GrassSide] = new("block/grass_block_side", "block/grass_block_side_overlay"),
        [Surfaces.Dirt] = new("block/dirt"),
        [Surfaces.Stone] = new("block/stone"),
        [Surfaces.Cobblestone] = new("block/cobblestone"),
        [Surfaces.Bedrock] = new("block/bedrock"),
        [Surfaces.Sand] = new("block/sand"),
        [Surfaces.Bark] = new("block/oak_log"),
        [Surfaces.LogTop] = new("block/oak_log_top"),
        [Surfaces.Leaves] = new("block/oak_leaves"),
        [Surfaces.Planks] = new("block/oak_planks"),
        [Surfaces.Snow] = new("block/snow"),
        [Surfaces.WhiteConcrete] = new("block/white_concrete"),
        [Surfaces.RedConcrete] = new("block/red_concrete"),
        [Surfaces.GreenConcrete] = new("block/green_concrete"),
        [Surfaces.BlueConcrete] = new("block/blue_concrete"),
        [Surfaces.Glowstone] = new("block/glowstone"),
        [Surfaces.SeaLantern] = new("block/sea_lantern"),
        [Surfaces.Shroomlight] = new("block/shroomlight"),
        [Surfaces.Magma] = new("block/magma"),
        [Surfaces.BirchBark] = new("block/birch_log"),
        [Surfaces.BirchTop] = new("block/birch_log_top"),
        // Minecraft's own colors for the leaves it does not tint by the climate.
        [Surfaces.BirchLeaves] = new("block/birch_leaves", Tint: new Color(128, 167, 85)),
        [Surfaces.SpruceBark] = new("block/spruce_log"),
        [Surfaces.SpruceTop] = new("block/spruce_log_top"),
        [Surfaces.SpruceLeaves] = new("block/spruce_leaves", Tint: new Color(97, 153, 97)),
        [Surfaces.Sandstone] = new("block/sandstone"),
        [Surfaces.SandstoneTop] = new("block/sandstone_top"),
        [Surfaces.CactusSide] = new("block/cactus_side"),
        [Surfaces.CactusTop] = new("block/cactus_top"),
        [Surfaces.Gravel] = new("block/gravel"),
        [Surfaces.Glass] = new("block/glass"),
        // The water's gray texture under the color of Minecraft's water in most of its biomes.
        [Surfaces.Water] = new("block/water_still", Tint: new Color(63, 118, 228)),
        [Surfaces.Ice] = new("block/ice"),
        [Surfaces.DirtPath] = new("block/dirt_path_top"),
        [Surfaces.StoneBricks] = new("block/stone_bricks"),
        [Surfaces.DoorWood] = new("block/oak_door_bottom"),
        [Surfaces.DoorTop] = new("block/oak_door_top"),
        [Surfaces.Farmland] = new("block/farmland"),
        [Surfaces.SprucePlanks] = new("block/spruce_planks"),
        [Surfaces.Hay] = new("block/hay_block_side"),
        [Surfaces.HayTop] = new("block/hay_block_top"),
        [Surfaces.Wheat] = new("block/wheat_stage7"),
        [Surfaces.TorchStick] = new("block/torch"),
        [Surfaces.Flame] = new("block/torch"),
        [Surfaces.SoulTorchStick] = new("block/soul_torch"),
        [Surfaces.SoulFlame] = new("block/soul_torch"),
        [Surfaces.LanternIron] = new("block/lantern"),
        [Surfaces.LanternGlass] = new("block/lantern"),
        [Surfaces.SoulLanternIron] = new("block/soul_lantern"),
        [Surfaces.SoulLanternGlass] = new("block/soul_lantern"),
        [Surfaces.WhiteWool] = new("block/white_wool"),
        [Surfaces.LightGrayWool] = new("block/light_gray_wool"),
        [Surfaces.GrayWool] = new("block/gray_wool"),
        [Surfaces.BlackWool] = new("block/black_wool"),
        [Surfaces.BrownWool] = new("block/brown_wool"),
        [Surfaces.PinkWool] = new("block/pink_wool"),
        [Surfaces.BirchPlanks] = new("block/birch_planks"),
        [Surfaces.CraftingTableTop] = new("block/crafting_table_top"),
        [Surfaces.CraftingTableSide] = new("block/crafting_table_front"),
        [Surfaces.CoalOre] = new("block/coal_ore"),
        [Surfaces.IronOre] = new("block/iron_ore"),
        [Surfaces.FurnaceSide] = new("block/furnace_front"),
        [Surfaces.FurnaceTop] = new("block/furnace_top"),
        [Surfaces.FurnaceLit] = new("block/furnace_front_on"),
        [Surfaces.RedSand] = new("block/red_sand"),
        [Surfaces.Terracotta] = new("block/terracotta"),
        [Surfaces.WhiteTerracotta] = new("block/white_terracotta"),
        [Surfaces.OrangeTerracotta] = new("block/orange_terracotta"),
        [Surfaces.YellowTerracotta] = new("block/yellow_terracotta"),
        [Surfaces.RedTerracotta] = new("block/red_terracotta"),
        [Surfaces.BrownTerracotta] = new("block/brown_terracotta"),
        [Surfaces.LightGrayTerracotta] = new("block/light_gray_terracotta"),
        [Surfaces.Clay] = new("block/clay"),
        [Surfaces.JungleBark] = new("block/jungle_log"),
        [Surfaces.JungleTop] = new("block/jungle_log_top"),
        [Surfaces.JunglePlanks] = new("block/jungle_planks"),
        [Surfaces.JungleLeaves] = new("block/jungle_leaves"),
        [Surfaces.AcaciaBark] = new("block/acacia_log"),
        [Surfaces.AcaciaTop] = new("block/acacia_log_top"),
        [Surfaces.AcaciaPlanks] = new("block/acacia_planks"),
        [Surfaces.AcaciaLeaves] = new("block/acacia_leaves"),
        [Surfaces.DarkOakBark] = new("block/dark_oak_log"),
        [Surfaces.DarkOakTop] = new("block/dark_oak_log_top"),
        [Surfaces.DarkOakPlanks] = new("block/dark_oak_planks"),
        [Surfaces.DarkOakLeaves] = new("block/dark_oak_leaves"),
        [Surfaces.CherryBark] = new("block/cherry_log"),
        [Surfaces.CherryTop] = new("block/cherry_log_top"),
        [Surfaces.CherryPlanks] = new("block/cherry_planks"),
        [Surfaces.CherryLeaves] = new("block/cherry_leaves"),
        [Surfaces.OakSapling] = new("block/oak_sapling"),
        [Surfaces.BirchSapling] = new("block/birch_sapling"),
        [Surfaces.SpruceSapling] = new("block/spruce_sapling"),
        [Surfaces.JungleSapling] = new("block/jungle_sapling"),
        [Surfaces.AcaciaSapling] = new("block/acacia_sapling"),
        [Surfaces.DarkOakSapling] = new("block/dark_oak_sapling"),
        [Surfaces.CherrySapling] = new("block/cherry_sapling"),
        // A flower's stem and its petals both lie under its one texture, laid across its walls from the foot.
        [Surfaces.DandelionStem] = new("block/dandelion"),
        [Surfaces.Dandelion] = new("block/dandelion"),
        [Surfaces.PoppyStem] = new("block/poppy"),
        [Surfaces.Poppy] = new("block/poppy"),
        [Surfaces.BlueOrchidStem] = new("block/blue_orchid"),
        [Surfaces.BlueOrchid] = new("block/blue_orchid"),
        [Surfaces.AlliumStem] = new("block/allium"),
        [Surfaces.Allium] = new("block/allium"),
        [Surfaces.AzureBluetStem] = new("block/azure_bluet"),
        [Surfaces.AzureBluet] = new("block/azure_bluet"),
        [Surfaces.RedTulipStem] = new("block/red_tulip"),
        [Surfaces.RedTulip] = new("block/red_tulip"),
        [Surfaces.OrangeTulipStem] = new("block/orange_tulip"),
        [Surfaces.OrangeTulip] = new("block/orange_tulip"),
        [Surfaces.WhiteTulipStem] = new("block/white_tulip"),
        [Surfaces.WhiteTulip] = new("block/white_tulip"),
        [Surfaces.PinkTulipStem] = new("block/pink_tulip"),
        [Surfaces.PinkTulip] = new("block/pink_tulip"),
        [Surfaces.OxeyeDaisyStem] = new("block/oxeye_daisy"),
        [Surfaces.OxeyeDaisy] = new("block/oxeye_daisy"),
        [Surfaces.CornflowerStem] = new("block/cornflower"),
        [Surfaces.Cornflower] = new("block/cornflower"),
        [Surfaces.LilyOfTheValleyStem] = new("block/lily_of_the_valley"),
        [Surfaces.LilyOfTheValley] = new("block/lily_of_the_valley"),
        [Surfaces.PinkPetals] = new("block/pink_petals"),
        [Surfaces.ShortGrass] = new("block/short_grass"),
        // The game's tall grass is one block high, so it takes the top of the source's, which ends in tips.
        [Surfaces.TallGrass] = new("block/tall_grass_top"),
        [Surfaces.Fern] = new("block/fern"),
        [Surfaces.DeadBush] = new("block/dead_bush"),
        [Surfaces.BrownMushroomStem] = new("block/brown_mushroom"),
        [Surfaces.BrownMushroom] = new("block/brown_mushroom"),
        [Surfaces.RedMushroomStem] = new("block/red_mushroom"),
        [Surfaces.RedMushroom] = new("block/red_mushroom"),
        [Surfaces.CopperOre] = new("block/copper_ore"),
        [Surfaces.GoldOre] = new("block/gold_ore"),
        [Surfaces.RedstoneOre] = new("block/redstone_ore"),
        [Surfaces.LapisOre] = new("block/lapis_ore"),
        [Surfaces.DiamondOre] = new("block/diamond_ore"),
        [Surfaces.EmeraldOre] = new("block/emerald_ore"),
        [Surfaces.WhiteStainedGlass] = new("block/white_stained_glass"),
        [Surfaces.OrangeStainedGlass] = new("block/orange_stained_glass"),
        [Surfaces.MagentaStainedGlass] = new("block/magenta_stained_glass"),
        [Surfaces.LightBlueStainedGlass] = new("block/light_blue_stained_glass"),
        [Surfaces.YellowStainedGlass] = new("block/yellow_stained_glass"),
        [Surfaces.LimeStainedGlass] = new("block/lime_stained_glass"),
        [Surfaces.PinkStainedGlass] = new("block/pink_stained_glass"),
        [Surfaces.GrayStainedGlass] = new("block/gray_stained_glass"),
        [Surfaces.LightGrayStainedGlass] = new("block/light_gray_stained_glass"),
        [Surfaces.CyanStainedGlass] = new("block/cyan_stained_glass"),
        [Surfaces.PurpleStainedGlass] = new("block/purple_stained_glass"),
        [Surfaces.BlueStainedGlass] = new("block/blue_stained_glass"),
        [Surfaces.BrownStainedGlass] = new("block/brown_stained_glass"),
        [Surfaces.GreenStainedGlass] = new("block/green_stained_glass"),
        [Surfaces.RedStainedGlass] = new("block/red_stained_glass"),
        [Surfaces.BlackStainedGlass] = new("block/black_stained_glass"),
        [Surfaces.OrangeWool] = new("block/orange_wool"),
        [Surfaces.MagentaWool] = new("block/magenta_wool"),
        [Surfaces.LightBlueWool] = new("block/light_blue_wool"),
        [Surfaces.YellowWool] = new("block/yellow_wool"),
        [Surfaces.LimeWool] = new("block/lime_wool"),
        [Surfaces.CyanWool] = new("block/cyan_wool"),
        [Surfaces.PurpleWool] = new("block/purple_wool"),
        [Surfaces.BlueWool] = new("block/blue_wool"),
        [Surfaces.GreenWool] = new("block/green_wool"),
        [Surfaces.RedWool] = new("block/red_wool"),
        [Surfaces.BrownMushroomCap] = new("block/brown_mushroom_block"),
        [Surfaces.RedMushroomCap] = new("block/red_mushroom_block"),
        [Surfaces.MushroomStem] = new("block/mushroom_stem"),
    };

    // The items whose texture is named apart from their key, and the blocks whose item Minecraft
    // draws flat from a texture rather than as a small cube.
    private static readonly Dictionary<string, string> _items = new()
    {
        ["raw_porkchop"] = "item/porkchop",
        ["raw_beef"] = "item/beef",
        ["raw_mutton"] = "item/mutton",
        ["raw_chicken"] = "item/chicken",
        ["steak"] = "item/cooked_beef",
        ["wheat_sheaf"] = "item/wheat",
        ["wheat"] = "item/wheat_seeds",
        ["torch"] = "block/torch",
        ["soul_torch"] = "block/soul_torch",
        ["lantern"] = "item/lantern",
        ["soul_lantern"] = "item/soul_lantern",
        ["oak_door"] = "item/oak_door",
        ["oak_sapling"] = "block/oak_sapling",
        ["birch_sapling"] = "block/birch_sapling",
        ["spruce_sapling"] = "block/spruce_sapling",
        ["jungle_sapling"] = "block/jungle_sapling",
        ["acacia_sapling"] = "block/acacia_sapling",
        ["dark_oak_sapling"] = "block/dark_oak_sapling",
        ["cherry_sapling"] = "block/cherry_sapling",
        ["dandelion"] = "block/dandelion",
        ["poppy"] = "block/poppy",
        ["blue_orchid"] = "block/blue_orchid",
        ["allium"] = "block/allium",
        ["azure_bluet"] = "block/azure_bluet",
        ["red_tulip"] = "block/red_tulip",
        ["orange_tulip"] = "block/orange_tulip",
        ["white_tulip"] = "block/white_tulip",
        ["pink_tulip"] = "block/pink_tulip",
        ["oxeye_daisy"] = "block/oxeye_daisy",
        ["cornflower"] = "block/cornflower",
        ["lily_of_the_valley"] = "block/lily_of_the_valley",
        ["pink_petals"] = "item/pink_petals",
        ["short_grass"] = "block/short_grass",
        ["tall_grass"] = "block/tall_grass_top",
        ["fern"] = "block/fern",
        ["dead_bush"] = "block/dead_bush",
        ["brown_mushroom"] = "block/brown_mushroom",
        ["red_mushroom"] = "block/red_mushroom",
    };

    /// <summary>A surface's texture, or null where the table gives it none.</summary>
    public static Look? Of(int surface) => _surfaces.TryGetValue(surface, out var look) ? look : null;

    // Each kind's picture under entity, the newer name first where Minecraft renamed it as it gave
    // the kind variants by climate, a pack made for an older version holding the older. The sheep
    // and the villager are drawn in their own colors, since the sheep's wool and the villager's
    // clothes are pictures of their own laid over theirs.
    private static readonly Dictionary<string, string[]> _mobs = new()
    {
        ["pig"] = ["entity/pig/temperate_pig", "entity/pig/pig"],
        ["cow"] = ["entity/cow/temperate_cow", "entity/cow/cow"],
        ["chicken"] = ["entity/chicken/temperate_chicken", "entity/chicken"],
        ["zombie"] = ["entity/zombie/zombie"],
        ["skeleton"] = ["entity/skeleton/skeleton"],
        ["creeper"] = ["entity/creeper/creeper"],
        ["spider"] = ["entity/spider/spider"],
    };

    /// <summary>The names a kind of mob's picture may have in a pack, the first a pack has taken, or none for a kind drawn in the game's own colors.</summary>
    public static IReadOnlyList<string> OfMob(string kind) => _mobs.GetValueOrDefault(kind) ?? [];

    /// <summary>
    /// The texture an item's icon is drawn from, its key's under <c>item</c> for an item that places
    /// no block, or null for a block drawn as a small cube of its faces.
    /// </summary>
    public static string? OfItem(Item item) =>
        _items.TryGetValue(item.Key, out var name) ? name : item.Block is null ? $"item/{item.Key}" : null;
}
