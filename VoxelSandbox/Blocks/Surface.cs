using Engine;

namespace VoxelSandbox;

/// <summary>
/// The look of a block's face, its color, which a section's mesh carries in the face's vertices, and
/// for an emissive block its emission, which its cube is drawn with as a material.
/// </summary>
/// <remarks>
/// A material's emission is one for its whole draw, so an emissive surface is drawn as cubes of its
/// own rather than as faces of a section, whose one material is not emissive.
/// </remarks>
public sealed record Surface(int Id, string Name, Color Color, float Roughness, Color Emissive, float Glow, SurfaceTint Tint, bool Unseen = false)
{
    /// <summary>Whether the surface is emissive.</summary>
    public bool Emits => Glow > 0;

    /// <summary>
    /// Whether its lamp's cubes are drawn wholly clear, so they show nowhere and cast no shadow and
    /// the scene's distance field alone takes their light, as a fire's light is given apart from its
    /// flames, which change with each step of their texture.
    /// </summary>
    /// <remarks>
    /// The field reads a draw's color and emission whatever its alpha, and the model pass and the
    /// shadows cut out a masked material's clear pixels, so such a cube lights the land around as
    /// a lamp does and nothing of it is seen.
    /// </remarks>

    /// <summary>The surface's color under a tint, which a tinted surface multiplies its gray by and any other ignores.</summary>
    public Color Under(Color grass, Color foliage) => Tint switch
    {
        SurfaceTint.Grass => Multiply(Color, grass),
        SurfaceTint.Foliage => Multiply(Color, foliage),
        _ => Color,
    };

    /// <summary>The surface's color as it looks in the plains, which the hotbar shows.</summary>
    public Color Plain => Under(Biomes.Plains.GrassColor, Biomes.Plains.FoliageColor);

    private static Color Multiply(Color a, Color b) => new((byte)(a.R * b.R / 255), (byte)(a.G * b.G / 255), (byte)(a.B * b.B / 255));

    /// <summary>The material a lamp's cube of this surface is drawn with, its emissive intensity scaled by <paramref name="glowScale"/>.</summary>
    public ModelMaterial Material(float glowScale) => new(Color)
    {
        AlphaMode = MaterialAlphaMode.Opaque,
        Roughness = Roughness,
        Emissive = Emits ? Emissive : Color.Black,
        EmissiveIntensity = Emits ? Glow * glowScale : 1,
    };
}

/// <summary>Which of the climate's colors a surface is tinted by, as Minecraft tints its gray grass and leaves.</summary>
public enum SurfaceTint
{
    None,
    Grass,
    Foliage,
}

/// <summary>Every surface a block shows, by its id.</summary>
public static class Surfaces
{
    private static readonly List<Surface> _all = [];

    private static int Add(string name, Color color, float roughness = 0.9f, Color emissive = default, float glow = 0, SurfaceTint tint = SurfaceTint.None, bool unseen = false)
    {
        _all.Add(new Surface(_all.Count, name, color, roughness, emissive, glow, tint, unseen));
        return _all.Count - 1;
    }

    // Each full block's color is the average of Bare Bones' texture of it, decoded and weighed by
    // alpha, read from the owner's copy as numbers by voxel.palette, so the game drawn with no pack
    // looks as it does with the pack it fetches, as the owner asked, a see-through block's alpha its
    // texture's cover, and a surface the pack tints by a color of its own, as water and birch and
    // spruce leaves, under that color. The plants keep their own, since a flower's average mixes
    // its stem and its petals, and so do the ores, since without a texture an ore shows by its color
    // alone and an average reads as the stone around it.
    // The gray a tinted surface multiplies its climate's color by, Bare Bones' gray grass and leaves.
    public static readonly int Grass = Add("grass", new Color(174, 174, 174), tint: SurfaceTint.Grass);
    public static readonly int Dirt = Add("dirt", new Color(155, 101, 49));
    public static readonly int Stone = Add("stone", new Color(130, 130, 130));
    public static readonly int Cobblestone = Add("cobblestone", new Color(116, 116, 116));
    public static readonly int Bedrock = Add("bedrock", new Color(97, 97, 97));
    public static readonly int Sand = Add("sand", new Color(218, 207, 163));
    public static readonly int Bark = Add("bark", new Color(107, 84, 50));
    public static readonly int LogTop = Add("log top", new Color(162, 129, 80));
    public static readonly int Leaves = Add("leaves", new Color(120, 128, 120), tint: SurfaceTint.Foliage);
    public static readonly int Planks = Add("planks", new Color(166, 132, 83));
    public static readonly int Snow = Add("snow", new Color(252, 255, 255));
    // Concrete, whose saturated colors make the color a wall gives its neighbors plain to see.
    public static readonly int WhiteConcrete = Add("white concrete", new Color(206, 212, 213), 0.8f);
    public static readonly int RedConcrete = Add("red concrete", new Color(141, 32, 32), 0.8f);
    public static readonly int GreenConcrete = Add("green concrete", new Color(73, 91, 36), 0.8f);
    public static readonly int BlueConcrete = Add("blue concrete", new Color(46, 48, 144), 0.8f);
    public static readonly int Glowstone = Add("glowstone", new Color(181, 154, 87), emissive: new Color(255, 200, 120), glow: 4);
    public static readonly int SeaLantern = Add("sea lantern", new Color(219, 232, 222), emissive: new Color(190, 230, 255), glow: 4);
    public static readonly int Shroomlight = Add("shroomlight", new Color(254, 196, 133), emissive: new Color(255, 150, 60), glow: 4);
    public static readonly int Magma = Add("magma", new Color(180, 98, 40), emissive: new Color(255, 90, 30), glow: 1.5f);
    public static readonly int BirchBark = Add("birch bark", new Color(239, 239, 239));
    public static readonly int BirchTop = Add("birch top", new Color(209, 194, 156));
    // Birch and spruce leaves keep a color of their own whatever the climate, as Minecraft's do.
    public static readonly int BirchLeaves = Add("birch leaves", new Color(98, 129, 29));
    public static readonly int SpruceBark = Add("spruce bark", new Color(56, 36, 17));
    public static readonly int SpruceTop = Add("spruce top", new Color(113, 83, 48));
    public static readonly int SpruceLeaves = Add("spruce leaves", new Color(45, 71, 45));
    public static readonly int Sandstone = Add("sandstone", new Color(211, 196, 148));
    public static readonly int CactusSide = Add("cactus side", new Color(104, 140, 55));
    public static readonly int CactusTop = Add("cactus top", new Color(109, 142, 63));
    public static readonly int Gravel = Add("gravel", new Color(127, 124, 123));
    // A see-through surface's alpha is how much of it covers what is behind.
    public static readonly int Glass = Add("glass", new Color(239, 247, 248, 65), roughness: 0.05f);
    public static readonly int Water = Add("water", new Color(44, 84, 162, 191), roughness: 0.05f);
    public static readonly int Ice = Add("ice", new Color(111, 180, 255, 158), roughness: 0.1f);
    public static readonly int DirtPath = Add("dirt path", new Color(195, 169, 96));
    public static readonly int StoneBricks = Add("stone bricks", new Color(123, 123, 123));
    public static readonly int DoorWood = Add("door wood", new Color(136, 106, 62));
    public static readonly int Farmland = Add("farmland", new Color(151, 86, 52));
    public static readonly int SprucePlanks = Add("spruce planks", new Color(119, 88, 52));
    public static readonly int Hay = Add("hay", new Color(176, 150, 42));
    public static readonly int HayTop = Add("hay top", new Color(183, 161, 45));
    public static readonly int Wheat = Add("wheat", new Color(196, 178, 82));
    // A soul torch's stick and a soul lantern's iron look as the others do, apart so a pack's soul
    // torch and soul lantern textures lie on them.
    public static readonly int TorchStick = Add("torch stick", new Color(162, 132, 88));
    public static readonly int SoulTorchStick = Add("soul torch stick", new Color(162, 132, 88));
    public static readonly int LanternIron = Add("lantern iron", new Color(52, 54, 62), 0.5f);
    public static readonly int SoulLanternIron = Add("soul lantern iron", new Color(52, 54, 62), 0.5f);
    public static readonly int EndRodBase = Add("end rod base", new Color(150, 132, 160));
    // Wool of the colors a sheep is born with, the same as its coat.
    public static readonly int WhiteWool = Add("white wool", new Color(254, 254, 254), 1);
    public static readonly int LightGrayWool = Add("light gray wool", new Color(157, 157, 151), 1);
    public static readonly int GrayWool = Add("gray wool", new Color(71, 79, 82), 1);
    public static readonly int BlackWool = Add("black wool", new Color(54, 53, 60), 1);
    public static readonly int BrownWool = Add("brown wool", new Color(120, 75, 43), 1);
    public static readonly int PinkWool = Add("pink wool", new Color(244, 147, 176), 1);
    public static readonly int BirchPlanks = Add("birch planks", new Color(200, 179, 124));
    public static readonly int CraftingTableTop = Add("crafting table top", new Color(131, 82, 48));
    public static readonly int CraftingTableSide = Add("crafting table side", new Color(154, 128, 94));
    // Stone darkened by its coal, since without textures an ore shows by its color alone.
    public static readonly int CoalOre = Add("coal ore", new Color(72, 72, 74));
    // Stone warmed by its iron, pale tan, as the ore's flecks are.
    public static readonly int IronOre = Add("iron ore", new Color(170, 146, 128));
    public static readonly int FurnaceSide = Add("furnace side", new Color(110, 110, 110));
    public static readonly int FurnaceTop = Add("furnace top", new Color(107, 107, 107));
    public static readonly int FurnaceLit = Add("lit furnace", new Color(147, 134, 113), emissive: new Color(255, 140, 50), glow: 1.2f);
    // A small light's emissive intensity is its light over its surface, so a torch's flame, a 48th
    // of a block's surface, is far brighter than glowstone to light a room nearly as well, as a
    // torch's level of 14 nearly matches glowstone's 15. Each is set so a closed room's floor takes
    // about three quarters of glowstone's light from a torch or an end rod, nine tenths from a
    // lantern, and two fifths from the soul fire's blue, whose level is 10.
    public static readonly int Flame = Add("flame", new Color(255, 214, 130), emissive: new Color(255, 180, 90), glow: 140);
    public static readonly int SoulFlame = Add("soul flame", new Color(140, 236, 245), emissive: new Color(80, 220, 255), glow: 70);
    public static readonly int LanternGlass = Add("lantern glass", new Color(250, 205, 125), emissive: new Color(255, 175, 85), glow: 25);
    public static readonly int SoulLanternGlass = Add("soul lantern glass", new Color(130, 225, 235), emissive: new Color(70, 210, 255), glow: 12);
    public static readonly int EndRod = Add("end rod", new Color(245, 242, 235), emissive: new Color(255, 248, 240), glow: 17);
    // Faces in the color of the surfaces they are split from, told apart where a resource pack
    // gives each a texture of its own, the grass block's side its strip of grass, the upper half of
    // a door its window and sandstone its top.
    public static readonly int GrassSide = Add("grass side", new Color(150, 117, 49));
    public static readonly int DoorTop = Add("door top", new Color(144, 114, 70));
    public static readonly int SandstoneTop = Add("sandstone top", new Color(221, 211, 168));
    // The badlands' red sand and the colors of their banded terracotta, and the clay of river beds.
    public static readonly int RedSand = Add("red sand", new Color(191, 103, 33));
    public static readonly int Terracotta = Add("terracotta", new Color(148, 89, 63));
    public static readonly int WhiteTerracotta = Add("white terracotta", new Color(209, 184, 173));
    public static readonly int OrangeTerracotta = Add("orange terracotta", new Color(160, 81, 45));
    public static readonly int YellowTerracotta = Add("yellow terracotta", new Color(201, 149, 8));
    public static readonly int RedTerracotta = Add("red terracotta", new Color(145, 64, 43));
    public static readonly int BrownTerracotta = Add("brown terracotta", new Color(83, 57, 42));
    public static readonly int LightGrayTerracotta = Add("light gray terracotta", new Color(158, 127, 114));
    public static readonly int Clay = Add("clay", new Color(161, 167, 178));
    // The woods of the jungle, the savanna, the dark forest and the cherry grove, their bark, the rings
    // of a log's end and their planks, and leaves the climate tints but the cherry's pink.
    public static readonly int JungleBark = Add("jungle bark", new Color(85, 69, 25));
    public static readonly int JungleTop = Add("jungle top", new Color(159, 116, 81));
    public static readonly int JunglePlanks = Add("jungle planks", new Color(166, 120, 88));
    public static readonly int AcaciaBark = Add("acacia bark", new Color(99, 93, 83));
    public static readonly int AcaciaTop = Add("acacia top", new Color(164, 95, 62));
    public static readonly int AcaciaPlanks = Add("acacia planks", new Color(173, 92, 51));
    public static readonly int DarkOakBark = Add("dark oak bark", new Color(60, 47, 27));
    public static readonly int DarkOakTop = Add("dark oak top", new Color(67, 45, 23));
    public static readonly int DarkOakPlanks = Add("dark oak planks", new Color(65, 42, 20));
    public static readonly int CherryBark = Add("cherry bark", new Color(72, 44, 56));
    public static readonly int CherryTop = Add("cherry top", new Color(204, 159, 155));
    public static readonly int CherryPlanks = Add("cherry planks", new Color(224, 173, 167));
    public static readonly int JungleLeaves = Add("jungle leaves", new Color(108, 117, 97), tint: SurfaceTint.Foliage);
    public static readonly int AcaciaLeaves = Add("acacia leaves", new Color(142, 135, 113), tint: SurfaceTint.Foliage);
    public static readonly int DarkOakLeaves = Add("dark oak leaves", new Color(95, 105, 89), tint: SurfaceTint.Foliage);
    public static readonly int CherryLeaves = Add("cherry leaves", new Color(247, 170, 215));
    // Saplings and the plants of the ground, each flower its stem and its petals, which a pack's one
    // texture of the flower covers both of, the grasses gray where the climate tints them.
    public static readonly int OakSapling = Add("oak sapling", new Color(70, 120, 40));
    public static readonly int BirchSapling = Add("birch sapling", new Color(110, 140, 70));
    public static readonly int SpruceSapling = Add("spruce sapling", new Color(40, 72, 42));
    public static readonly int JungleSapling = Add("jungle sapling", new Color(52, 112, 32));
    public static readonly int AcaciaSapling = Add("acacia sapling", new Color(112, 122, 40));
    public static readonly int DarkOakSapling = Add("dark oak sapling", new Color(40, 82, 30));
    public static readonly int CherrySapling = Add("cherry sapling", new Color(220, 150, 180));
    public static readonly int DandelionStem = Add("dandelion stem", new Color(62, 122, 42));
    public static readonly int Dandelion = Add("dandelion", new Color(250, 220, 40));
    public static readonly int PoppyStem = Add("poppy stem", new Color(62, 122, 42));
    public static readonly int Poppy = Add("poppy", new Color(220, 30, 30));
    public static readonly int BlueOrchidStem = Add("blue orchid stem", new Color(62, 122, 42));
    public static readonly int BlueOrchid = Add("blue orchid", new Color(40, 170, 220));
    public static readonly int AlliumStem = Add("allium stem", new Color(62, 122, 42));
    public static readonly int Allium = Add("allium", new Color(180, 110, 220));
    public static readonly int AzureBluetStem = Add("azure bluet stem", new Color(62, 122, 42));
    public static readonly int AzureBluet = Add("azure bluet", new Color(230, 230, 240));
    public static readonly int RedTulipStem = Add("red tulip stem", new Color(62, 122, 42));
    public static readonly int RedTulip = Add("red tulip", new Color(220, 50, 40));
    public static readonly int OrangeTulipStem = Add("orange tulip stem", new Color(62, 122, 42));
    public static readonly int OrangeTulip = Add("orange tulip", new Color(240, 140, 40));
    public static readonly int WhiteTulipStem = Add("white tulip stem", new Color(62, 122, 42));
    public static readonly int WhiteTulip = Add("white tulip", new Color(240, 240, 240));
    public static readonly int PinkTulipStem = Add("pink tulip stem", new Color(62, 122, 42));
    public static readonly int PinkTulip = Add("pink tulip", new Color(240, 170, 200));
    public static readonly int OxeyeDaisyStem = Add("oxeye daisy stem", new Color(62, 122, 42));
    public static readonly int OxeyeDaisy = Add("oxeye daisy", new Color(240, 240, 230));
    public static readonly int CornflowerStem = Add("cornflower stem", new Color(62, 122, 42));
    public static readonly int Cornflower = Add("cornflower", new Color(70, 100, 230));
    public static readonly int LilyOfTheValleyStem = Add("lily of the valley stem", new Color(62, 122, 42));
    public static readonly int LilyOfTheValley = Add("lily of the valley", new Color(245, 245, 245));
    public static readonly int PinkPetals = Add("pink petals", new Color(240, 170, 200));
    public static readonly int ShortGrass = Add("short grass", new Color(150, 150, 150), tint: SurfaceTint.Grass);
    public static readonly int TallGrass = Add("tall grass", new Color(150, 150, 150), tint: SurfaceTint.Grass);
    public static readonly int TallGrassBottom = Add("tall grass's lower half", new Color(150, 150, 150), tint: SurfaceTint.Grass);
    public static readonly int Fern = Add("fern", new Color(140, 140, 140), tint: SurfaceTint.Grass);
    public static readonly int DeadBush = Add("dead bush", new Color(140, 100, 50));
    public static readonly int BrownMushroomStem = Add("brown mushroom stem", new Color(220, 210, 190));
    public static readonly int RedMushroomStem = Add("red mushroom stem", new Color(220, 210, 190));
    public static readonly int BrownMushroom = Add("brown mushroom", new Color(150, 110, 80));
    public static readonly int RedMushroom = Add("red mushroom", new Color(200, 40, 40));
    // Ores by depth, stone warmed or cooled by the color of what is in it.
    public static readonly int CopperOre = Add("copper ore", new Color(150, 120, 100));
    public static readonly int GoldOre = Add("gold ore", new Color(176, 164, 110));
    public static readonly int RedstoneOre = Add("redstone ore", new Color(146, 96, 96));
    public static readonly int LapisOre = Add("lapis ore", new Color(100, 112, 146));
    public static readonly int DiamondOre = Add("diamond ore", new Color(120, 164, 164));
    public static readonly int EmeraldOre = Add("emerald ore", new Color(108, 146, 116));
    // Glass and wool in the sixteen colors of Minecraft's dyes, the wool's six of a sheep's coat before.
    public static readonly int WhiteStainedGlass = Add("white stained glass", new Color(255, 255, 255, 191), roughness: 0.05f);
    public static readonly int OrangeStainedGlass = Add("orange stained glass", new Color(222, 117, 27, 191), roughness: 0.05f);
    public static readonly int MagentaStainedGlass = Add("magenta stained glass", new Color(181, 58, 226, 191), roughness: 0.05f);
    public static readonly int LightBlueStainedGlass = Add("light blue stained glass", new Color(107, 163, 232, 191), roughness: 0.05f);
    public static readonly int YellowStainedGlass = Add("yellow stained glass", new Color(255, 255, 2, 191), roughness: 0.05f);
    public static readonly int LimeStainedGlass = Add("lime stained glass", new Color(130, 221, 10, 191), roughness: 0.05f);
    public static readonly int PinkStainedGlass = Add("pink stained glass", new Color(242, 127, 165, 191), roughness: 0.05f);
    public static readonly int GrayStainedGlass = Add("gray stained glass", new Color(76, 76, 76, 191), roughness: 0.05f);
    public static readonly int LightGrayStainedGlass = Add("light gray stained glass", new Color(153, 153, 153, 191), roughness: 0.05f);
    public static readonly int CyanStainedGlass = Add("cyan stained glass", new Color(63, 128, 161, 191), roughness: 0.05f);
    public static readonly int PurpleStainedGlass = Add("purple stained glass", new Color(127, 63, 178, 191), roughness: 0.05f);
    public static readonly int BlueStainedGlass = Add("blue stained glass", new Color(30, 60, 180, 191), roughness: 0.05f);
    public static readonly int BrownStainedGlass = Add("brown stained glass", new Color(100, 69, 40, 191), roughness: 0.05f);
    public static readonly int GreenStainedGlass = Add("green stained glass", new Color(105, 134, 45, 191), roughness: 0.05f);
    public static readonly int RedStainedGlass = Add("red stained glass", new Color(200, 19, 19, 191), roughness: 0.05f);
    public static readonly int BlackStainedGlass = Add("black stained glass", new Color(2, 2, 2, 191), roughness: 0.05f);
    public static readonly int OrangeWool = Add("orange wool", new Color(247, 121, 22), 1);
    public static readonly int MagentaWool = Add("magenta wool", new Color(192, 70, 182), 1);
    public static readonly int LightBlueWool = Add("light blue wool", new Color(64, 187, 223), 1);
    public static readonly int YellowWool = Add("yellow wool", new Color(252, 199, 37), 1);
    public static readonly int LimeWool = Add("lime wool", new Color(111, 187, 24), 1);
    public static readonly int CyanWool = Add("cyan wool", new Color(22, 141, 148), 1);
    public static readonly int PurpleWool = Add("purple wool", new Color(135, 48, 182), 1);
    public static readonly int BlueWool = Add("blue wool", new Color(55, 58, 160), 1);
    public static readonly int GreenWool = Add("green wool", new Color(88, 114, 25), 1);
    public static readonly int RedWool = Add("red wool", new Color(170, 42, 36), 1);
    // A giant mushroom's caps and its stem.
    public static readonly int BrownMushroomCap = Add("brown mushroom cap", new Color(150, 113, 81));
    public static readonly int RedMushroomCap = Add("red mushroom cap", new Color(225, 56, 56));
    public static readonly int MushroomStem = Add("mushroom stem", new Color(206, 198, 182));
    public static readonly int TntSide = Add("tnt side", new Color(206, 72, 52));
    public static readonly int TntTop = Add("tnt top", new Color(170, 150, 140));
    public static readonly int TntBottom = Add("tnt bottom", new Color(150, 130, 120));
    // A fire's flames, cut out of its walls and lit by its own cell's light, and the light it gives,
    // a box inside them that only the scene's distance field sees, as bright as glowstone's over a
    // room for its smaller box.
    public static readonly int Fire = Add("fire", new Color(232, 140, 40));
    public static readonly int FireLight = Add("fire light", new Color(255, 170, 70), emissive: new Color(255, 150, 50), glow: 6, unseen: true);
    // The deepslate under the floor of 0, its sides and its top, and the cobbles it breaks into.
    public static readonly int Deepslate = Add("deepslate", new Color(85, 85, 87));
    public static readonly int DeepslateTop = Add("deepslate top", new Color(84, 84, 86));
    public static readonly int CobbledDeepslate = Add("cobbled deepslate", new Color(84, 84, 87));
    // The ores in deepslate, each its ore's own color darkened as deepslate is from stone.
    public static readonly int DeepslateCoalOre = Add("deepslate coal ore", new Color(47, 47, 48));
    public static readonly int DeepslateIronOre = Add("deepslate iron ore", new Color(111, 95, 84));
    public static readonly int DeepslateCopperOre = Add("deepslate copper ore", new Color(98, 78, 65));
    public static readonly int DeepslateGoldOre = Add("deepslate gold ore", new Color(115, 107, 72));
    public static readonly int DeepslateRedstoneOre = Add("deepslate redstone ore", new Color(95, 63, 63));
    public static readonly int DeepslateLapisOre = Add("deepslate lapis ore", new Color(65, 73, 95));
    public static readonly int DeepslateDiamondOre = Add("deepslate diamond ore", new Color(78, 107, 107));
    public static readonly int DeepslateEmeraldOre = Add("deepslate emerald ore", new Color(71, 95, 76));
    // Lava, emissive through its own faces, which its section's mesh holds, where a whole emissive
    // block is a lamp's cube apart, as orange as the source's still lava is and as bright over a
    // cave as glowstone.
    public static readonly int Lava = Add("lava", new Color(207, 92, 20), emissive: new Color(255, 120, 30), glow: 4);

    /// <summary>Every surface, its index its id.</summary>
    public static IReadOnlyList<Surface> All => _all;
}
