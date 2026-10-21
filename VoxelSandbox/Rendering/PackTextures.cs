using Engine;

namespace VoxelSandbox;

/// <summary>
/// The texture of a resource pack each surface and item is drawn with, by the name Minecraft gives
/// it under <c>assets/minecraft/textures</c>, a table the game keeps, since its blocks are its own
/// and only some of them have a likeness in the source.
/// </summary>
/// <remarks>
/// A surface the table leaves out keeps the game's own color with a pack loaded, as do the small
/// pieces whose texture Minecraft lays out by a model of its own, a lantern's cap and an end rod's
/// foot, and the lit parts of a torch, a lantern and an end rod, drawn as emissive cubes. A furnace
/// and a crafting table have no front, so each of their sides takes the front's texture, which
/// tells them apart from stone and planks.
/// </remarks>
public static class PackTextures
{
    /// <summary>
    /// A surface's texture, an overlay over it that the climate's grass color tints, as the grass
    /// block's side is drawn, leaving the texture under it untinted, and a color the texture is
    /// tinted by where Minecraft tints it by one of its own, as its water and its birch and spruce
    /// leaves are.
    /// </summary>
    public readonly record struct Look(string Texture, string? Overlay = null, Color? Tint = null);

    private static readonly Dictionary<int, Look> _surfaces = new()
    {
        [Surfaces.Grass] = new("block/grass_block_top"),
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
        // The torch's texture holds its stick where the stick stands in its cell, so the stick's
        // faces show the stick's pixels.
        [Surfaces.TorchStick] = new("block/torch"),
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
    };

    /// <summary>A surface's texture, or null where the table gives it none.</summary>
    public static Look? Of(int surface) => _surfaces.TryGetValue(surface, out var look) ? look : null;

    /// <summary>
    /// The texture an item's icon is drawn from, its key's under <c>item</c> for an item that places
    /// no block, or null for a block drawn as a small cube of its faces.
    /// </summary>
    public static string? OfItem(Item item) =>
        _items.TryGetValue(item.Key, out var name) ? name : item.Block is null ? $"item/{item.Key}" : null;
}
