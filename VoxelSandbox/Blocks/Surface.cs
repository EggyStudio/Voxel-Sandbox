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
public sealed record Surface(int Id, string Name, Color Color, float Roughness, Color Emissive, float Glow, SurfaceTint Tint)
{
    /// <summary>Whether the surface is emissive.</summary>
    public bool Emits => Glow > 0;

    /// <summary>The surface's color under a tint, which a tinted surface multiplies its gray by and any other ignores.</summary>
    public Color Under(Color grass, Color foliage) => Tint switch
    {
        SurfaceTint.Grass => Multiply(Color, grass),
        SurfaceTint.Foliage => Multiply(Color, foliage),
        _ => Color,
    };

    /// <summary>The surface's color as it looks in the plains, which the hotbar shows.</summary>
    public Color Plain => Under(Climate.Grass(Climate.PlainsTemperature, Climate.PlainsHumidity), Climate.Foliage(Climate.PlainsTemperature, Climate.PlainsHumidity));

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

    private static int Add(string name, Color color, float roughness = 0.9f, Color emissive = default, float glow = 0, SurfaceTint tint = SurfaceTint.None)
    {
        _all.Add(new Surface(_all.Count, name, color, roughness, emissive, glow, tint));
        return _all.Count - 1;
    }

    // The gray a tinted surface multiplies its climate's color by, about the brightness of Minecraft's
    // gray grass and leaf textures, which its colormap tints.
    public static readonly int Grass = Add("grass", new Color(184, 184, 184), tint: SurfaceTint.Grass);
    public static readonly int Dirt = Add("dirt", new Color(134, 96, 67));
    public static readonly int Stone = Add("stone", new Color(125, 125, 125));
    public static readonly int Cobblestone = Add("cobblestone", new Color(98, 98, 98));
    public static readonly int Bedrock = Add("bedrock", new Color(52, 52, 52));
    public static readonly int Sand = Add("sand", new Color(219, 207, 163));
    public static readonly int Bark = Add("bark", new Color(102, 81, 51));
    public static readonly int LogTop = Add("log top", new Color(168, 136, 86));
    public static readonly int Leaves = Add("leaves", new Color(150, 150, 150), tint: SurfaceTint.Foliage);
    public static readonly int Planks = Add("planks", new Color(162, 130, 78));
    public static readonly int Snow = Add("snow", new Color(240, 247, 250));
    // Concrete in saturated colors, so the color a wall gives its neighbors is plain to see.
    public static readonly int WhiteConcrete = Add("white concrete", new Color(212, 216, 217), 0.8f);
    public static readonly int RedConcrete = Add("red concrete", new Color(170, 35, 35), 0.8f);
    public static readonly int GreenConcrete = Add("green concrete", new Color(75, 140, 40), 0.8f);
    public static readonly int BlueConcrete = Add("blue concrete", new Color(45, 60, 160), 0.8f);
    public static readonly int Glowstone = Add("glowstone", new Color(230, 190, 110), emissive: new Color(255, 200, 120), glow: 4);
    public static readonly int SeaLantern = Add("sea lantern", new Color(200, 225, 220), emissive: new Color(190, 230, 255), glow: 4);
    public static readonly int Shroomlight = Add("shroomlight", new Color(240, 150, 70), emissive: new Color(255, 150, 60), glow: 4);
    public static readonly int Magma = Add("magma", new Color(120, 40, 20), emissive: new Color(255, 90, 30), glow: 1.5f);
    public static readonly int BirchBark = Add("birch bark", new Color(216, 212, 198));
    public static readonly int BirchTop = Add("birch top", new Color(194, 174, 120));
    // Birch and spruce leaves keep a color of their own whatever the climate, as Minecraft's do.
    public static readonly int BirchLeaves = Add("birch leaves", new Color(102, 134, 68));
    public static readonly int SpruceBark = Add("spruce bark", new Color(60, 44, 28));
    public static readonly int SpruceTop = Add("spruce top", new Color(116, 86, 52));
    public static readonly int SpruceLeaves = Add("spruce leaves", new Color(49, 79, 49));
    public static readonly int Sandstone = Add("sandstone", new Color(216, 202, 155));
    public static readonly int CactusSide = Add("cactus side", new Color(78, 128, 42));
    public static readonly int CactusTop = Add("cactus top", new Color(108, 150, 64));
    public static readonly int Gravel = Add("gravel", new Color(132, 126, 124));
    // A see-through surface's alpha is how much of it covers what is behind.
    public static readonly int Glass = Add("glass", new Color(214, 232, 236, 64), roughness: 0.05f);
    public static readonly int Water = Add("water", new Color(44, 92, 196, 168), roughness: 0.05f);
    public static readonly int Ice = Add("ice", new Color(164, 200, 242, 190), roughness: 0.1f);
    public static readonly int DirtPath = Add("dirt path", new Color(148, 122, 65));
    public static readonly int StoneBricks = Add("stone bricks", new Color(122, 121, 122));
    public static readonly int DoorWood = Add("door wood", new Color(140, 108, 62));
    public static readonly int Farmland = Add("farmland", new Color(96, 62, 36));
    public static readonly int SprucePlanks = Add("spruce planks", new Color(115, 85, 49));
    public static readonly int Hay = Add("hay", new Color(186, 152, 32));
    public static readonly int HayTop = Add("hay top", new Color(166, 136, 40));
    public static readonly int Wheat = Add("wheat", new Color(196, 178, 82));
    public static readonly int TorchStick = Add("torch stick", new Color(104, 78, 46));
    public static readonly int Iron = Add("dark iron", new Color(52, 54, 62), 0.5f);
    public static readonly int EndRodBase = Add("end rod base", new Color(150, 132, 160));
    // Wool of the colors a sheep is born with, the same as its coat.
    public static readonly int WhiteWool = Add("white wool", new Color(233, 236, 236), 1);
    public static readonly int LightGrayWool = Add("light gray wool", new Color(142, 142, 135), 1);
    public static readonly int GrayWool = Add("gray wool", new Color(63, 68, 72), 1);
    public static readonly int BlackWool = Add("black wool", new Color(24, 24, 30), 1);
    public static readonly int BrownWool = Add("brown wool", new Color(114, 71, 40), 1);
    public static readonly int PinkWool = Add("pink wool", new Color(237, 141, 172), 1);
    public static readonly int BirchPlanks = Add("birch planks", new Color(196, 178, 122));
    public static readonly int CraftingTableTop = Add("crafting table top", new Color(176, 132, 82));
    public static readonly int CraftingTableSide = Add("crafting table side", new Color(128, 94, 58));
    // Stone darkened by its coal, since without textures an ore shows by its color alone.
    public static readonly int CoalOre = Add("coal ore", new Color(72, 72, 74));
    // Stone warmed by its iron, pale tan, as the ore's flecks are.
    public static readonly int IronOre = Add("iron ore", new Color(170, 146, 128));
    public static readonly int FurnaceSide = Add("furnace side", new Color(92, 92, 92));
    public static readonly int FurnaceTop = Add("furnace top", new Color(116, 116, 116));
    public static readonly int FurnaceLit = Add("lit furnace", new Color(110, 92, 80), emissive: new Color(255, 140, 50), glow: 1.2f);
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
    public static readonly int GrassSide = Add("grass side", new Color(134, 96, 67));
    public static readonly int DoorTop = Add("door top", new Color(140, 108, 62));
    public static readonly int SandstoneTop = Add("sandstone top", new Color(216, 202, 155));
    // The badlands' red sand and the colors of their banded terracotta, and the clay of river beds.
    public static readonly int RedSand = Add("red sand", new Color(190, 102, 33));
    public static readonly int Terracotta = Add("terracotta", new Color(152, 94, 67));
    public static readonly int WhiteTerracotta = Add("white terracotta", new Color(209, 178, 161));
    public static readonly int OrangeTerracotta = Add("orange terracotta", new Color(161, 83, 37));
    public static readonly int YellowTerracotta = Add("yellow terracotta", new Color(186, 133, 35));
    public static readonly int RedTerracotta = Add("red terracotta", new Color(143, 61, 46));
    public static readonly int BrownTerracotta = Add("brown terracotta", new Color(77, 51, 35));
    public static readonly int LightGrayTerracotta = Add("light gray terracotta", new Color(135, 106, 97));
    public static readonly int Clay = Add("clay", new Color(160, 166, 179));
    // The woods of the jungle, the savanna, the dark forest and the cherry grove, their bark, the rings
    // of a log's end and their planks, and leaves the climate tints but the cherry's pink.
    public static readonly int JungleBark = Add("jungle bark", new Color(85, 68, 25));
    public static readonly int JungleTop = Add("jungle top", new Color(150, 110, 72));
    public static readonly int JunglePlanks = Add("jungle planks", new Color(160, 115, 80));
    public static readonly int AcaciaBark = Add("acacia bark", new Color(103, 96, 86));
    public static readonly int AcaciaTop = Add("acacia top", new Color(171, 92, 51));
    public static readonly int AcaciaPlanks = Add("acacia planks", new Color(168, 90, 50));
    public static readonly int DarkOakBark = Add("dark oak bark", new Color(60, 46, 26));
    public static readonly int DarkOakTop = Add("dark oak top", new Color(80, 58, 34));
    public static readonly int DarkOakPlanks = Add("dark oak planks", new Color(66, 43, 20));
    public static readonly int CherryBark = Add("cherry bark", new Color(54, 33, 44));
    public static readonly int CherryTop = Add("cherry top", new Color(178, 130, 128));
    public static readonly int CherryPlanks = Add("cherry planks", new Color(226, 178, 172));
    public static readonly int JungleLeaves = Add("jungle leaves", new Color(150, 150, 150), tint: SurfaceTint.Foliage);
    public static readonly int AcaciaLeaves = Add("acacia leaves", new Color(150, 150, 150), tint: SurfaceTint.Foliage);
    public static readonly int DarkOakLeaves = Add("dark oak leaves", new Color(150, 150, 150), tint: SurfaceTint.Foliage);
    public static readonly int CherryLeaves = Add("cherry leaves", new Color(229, 173, 194));
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
    public static readonly int WhiteStainedGlass = Add("white stained glass", new Color(255, 255, 255, 120), roughness: 0.05f);
    public static readonly int OrangeStainedGlass = Add("orange stained glass", new Color(216, 127, 51, 120), roughness: 0.05f);
    public static readonly int MagentaStainedGlass = Add("magenta stained glass", new Color(178, 76, 216, 120), roughness: 0.05f);
    public static readonly int LightBlueStainedGlass = Add("light blue stained glass", new Color(102, 153, 216, 120), roughness: 0.05f);
    public static readonly int YellowStainedGlass = Add("yellow stained glass", new Color(229, 229, 51, 120), roughness: 0.05f);
    public static readonly int LimeStainedGlass = Add("lime stained glass", new Color(127, 204, 25, 120), roughness: 0.05f);
    public static readonly int PinkStainedGlass = Add("pink stained glass", new Color(242, 127, 165, 120), roughness: 0.05f);
    public static readonly int GrayStainedGlass = Add("gray stained glass", new Color(76, 76, 76, 120), roughness: 0.05f);
    public static readonly int LightGrayStainedGlass = Add("light gray stained glass", new Color(153, 153, 153, 120), roughness: 0.05f);
    public static readonly int CyanStainedGlass = Add("cyan stained glass", new Color(76, 127, 153, 120), roughness: 0.05f);
    public static readonly int PurpleStainedGlass = Add("purple stained glass", new Color(127, 63, 178, 120), roughness: 0.05f);
    public static readonly int BlueStainedGlass = Add("blue stained glass", new Color(51, 76, 178, 120), roughness: 0.05f);
    public static readonly int BrownStainedGlass = Add("brown stained glass", new Color(102, 76, 51, 120), roughness: 0.05f);
    public static readonly int GreenStainedGlass = Add("green stained glass", new Color(102, 127, 51, 120), roughness: 0.05f);
    public static readonly int RedStainedGlass = Add("red stained glass", new Color(153, 51, 51, 120), roughness: 0.05f);
    public static readonly int BlackStainedGlass = Add("black stained glass", new Color(25, 25, 25, 120), roughness: 0.05f);
    public static readonly int OrangeWool = Add("orange wool", new Color(240, 118, 19), 1);
    public static readonly int MagentaWool = Add("magenta wool", new Color(189, 68, 179), 1);
    public static readonly int LightBlueWool = Add("light blue wool", new Color(58, 175, 217), 1);
    public static readonly int YellowWool = Add("yellow wool", new Color(248, 197, 39), 1);
    public static readonly int LimeWool = Add("lime wool", new Color(112, 185, 25), 1);
    public static readonly int CyanWool = Add("cyan wool", new Color(21, 137, 145), 1);
    public static readonly int PurpleWool = Add("purple wool", new Color(121, 42, 172), 1);
    public static readonly int BlueWool = Add("blue wool", new Color(53, 57, 157), 1);
    public static readonly int GreenWool = Add("green wool", new Color(84, 109, 27), 1);
    public static readonly int RedWool = Add("red wool", new Color(160, 39, 34), 1);

    /// <summary>Every surface, its index its id.</summary>
    public static IReadOnlyList<Surface> All => _all;
}
