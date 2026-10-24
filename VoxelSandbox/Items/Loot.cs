namespace VoxelSandbox;

/// <summary>
/// What breaking a block in survival and killing a mob give, and how long a block takes to break,
/// as Minecraft's do.
/// </summary>
/// <remarks>
/// A block takes its hardness times one and a half seconds to break, divided by the speed of the
/// tool held where it is of the kind the block is for, as a pickaxe is for stone and an axe for
/// wood. A block that asks for a pickaxe, as stone, coal and concrete do, takes its hardness times
/// five seconds without one and drops nothing, as in Minecraft.
/// </remarks>
public static class Loot
{
    /// <summary>
    /// The items a block broken in survival drops with an item held. Grass, farmland and a path
    /// drop their dirt, stone its cobblestone, coal ore its coal, diamond and emerald ore their gem,
    /// lapis ore four to nine lapis and redstone ore four or five redstone, a giant mushroom's cap up to
    /// two of its mushroom, leaves their sapling one
    /// time in twenty and an oak's or a dark oak's an apple one in two hundred, the grasses a seed of
    /// wheat one time in eight, a dead bush up to two sticks, glass and ice nothing, wheat its wheat
    /// and a few seeds, any other block the item that places it, and a block that asks for a pickaxe
    /// broken without one nothing, as Minecraft's do.
    /// </summary>
    public static IEnumerable<(Item Item, int Count)> Of(BlockId block, Random random, Item? held = null)
    {
        if (!Harvests(block, held)) yield break;
        var item = Blocks.Get(block).Item;
        if (BlockGroups.Leaves.TryGetValue(item, out var sapling))
        {
            if (random.Next(20) == 0) yield return (Items.Of(sapling), 1);
            if (item is BlockId.OakLeaves or BlockId.DarkOakLeaves && random.Next(200) == 0) yield return (Items.Apple, 1);
            yield break;
        }
        if (BlockGroups.Glass.Contains(item)) yield break;
        // An ore in deepslate drops what its ore in stone does, or itself where that drops itself.
        switch (Ores.Plain(item))
        {
            case BlockId.Air or BlockId.Water or BlockId.Lava or BlockId.Bedrock or BlockId.Ice:
                yield break;
            case BlockId.ShortGrass or BlockId.TallGrass or BlockId.Fern:
                if (random.Next(8) == 0) yield return (Items.Of(BlockId.Wheat), 1);
                yield break;
            case BlockId.Gravel:
                // Gravel gives flint one time in ten, as the source's does.
                yield return random.Next(10) == 0 ? (Items.Flint, 1) : (Items.Of(BlockId.Gravel), 1);
                yield break;
            case BlockId.DeadBush:
                if (random.Next(3) is > 0 and var sticks) yield return (Items.Stick, sticks);
                yield break;
            // A giant mushroom's cap up to two of its mushroom, as Minecraft's.
            case BlockId.BrownMushroomBlock or BlockId.RedMushroomBlock:
                if (random.Next(3) is > 0 and var caps) yield return (Items.Of(item == BlockId.BrownMushroomBlock ? BlockId.BrownMushroom : BlockId.RedMushroom), caps);
                yield break;
            case BlockId.DiamondOre:
                yield return (Items.Diamond, 1);
                yield break;
            case BlockId.EmeraldOre:
                yield return (Items.Emerald, 1);
                yield break;
            case BlockId.LapisOre:
                yield return (Items.LapisLazuli, 4 + random.Next(6));
                yield break;
            case BlockId.RedstoneOre:
                yield return (Items.Redstone, 4 + random.Next(2));
                yield break;
            case BlockId.Grass or BlockId.Farmland or BlockId.DirtPath:
                yield return (Items.Of(BlockId.Dirt), 1);
                yield break;
            case BlockId.Stone:
                yield return (Items.Of(BlockId.Cobblestone), 1);
                yield break;
            case BlockId.Deepslate:
                yield return (Items.Of(BlockId.CobbledDeepslate), 1);
                yield break;
            case BlockId.CoalOre:
                yield return (Items.Coal, 1);
                yield break;
            case BlockId.FurnaceLit:
                yield return (Items.Of(BlockId.Furnace), 1);
                yield break;
            case BlockId.Wheat:
                yield return (Items.Wheat, 1);
                yield return (Items.Of(BlockId.Wheat), 1 + random.Next(3));
                yield break;
            default:
                yield return (Items.Of(item), 1);
                yield break;
        }
    }

    // The wool a sheep drops by its variant, the colors of its coat in order.
    private static readonly BlockId[] Wool = [BlockId.WhiteWool, BlockId.LightGrayWool, BlockId.GrayWool, BlockId.BlackWool, BlockId.BrownWool, BlockId.PinkWool];

    /// <summary>
    /// The items a mob drops as it dies. A pig drops one to three porkchops, a cow up to two
    /// leather and one to three beef, a sheep a block of its wool and one or two mutton, a chicken
    /// up to two feathers and its meat, a zombie up to two rotten flesh, a skeleton up to two bones
    /// and two arrows, a creeper up to two gunpowder unless it blew itself up, a spider up to two
    /// string and now and then an eye, and a villager nothing.
    /// </summary>
    public static IEnumerable<(Item Item, int Count)> Of(Mob mob, Random random)
    {
        (Item, int)[] drops = mob.Kind.Name switch
        {
            "pig" => [(Items.RawPorkchop, 1 + random.Next(3))],
            "cow" => [(Items.Leather, random.Next(3)), (Items.RawBeef, 1 + random.Next(3))],
            "sheep" => [(Items.Of(Wool[mob.Variant % Wool.Length]), 1), (Items.RawMutton, 1 + random.Next(2))],
            "chicken" => [(Items.Feather, random.Next(3)), (Items.RawChicken, 1)],
            "zombie" => [(Items.RottenFlesh, random.Next(3))],
            "skeleton" => [(Items.Bone, random.Next(3)), (Items.Arrow, random.Next(3))],
            "creeper" => mob.Exploded ? [] : [(Items.Gunpowder, random.Next(3))],
            "spider" => [(Items.String, random.Next(3)), (Items.SpiderEye, random.Next(3) == 0 ? 1 : 0)],
            _ => [],
        };
        return drops.Where(d => d.Item2 > 0);
    }

    /// <summary>Seconds a block takes to break in survival with an item held, or by hand, 0 for one that breaks at once.</summary>
    public static float BreakSeconds(BlockId block, Item? held = null)
    {
        var (tool, _) = ToolFor(block);
        var speed = tool != ToolKind.None && held?.Tool == tool ? held.Speed : 1;
        return Hardness(block) * (Harvests(block, held) ? 1.5f : 5) / speed;
    }

    /// <summary>
    /// Whether a block broken with an item held, or by hand, drops anything, which a block that asks
    /// for a pickaxe does only to one, and iron ore only to one of stone or better.
    /// </summary>
    public static bool Harvests(BlockId block, Item? held) =>
        ToolFor(block) is not (ToolKind.Pickaxe, true) || held?.Tool == ToolKind.Pickaxe && held.Tier >= TierFor(block);

    /// <summary>The tier of pickaxe a block's ore asks, 2 for iron, copper and lapis ore, 3 for gold, redstone, diamond and emerald ore and 1 for the rest.</summary>
    public static int TierFor(BlockId block) => Ores.Plain(block) switch
    {
        BlockId.IronOre or BlockId.CopperOre or BlockId.LapisOre => 2,
        BlockId.GoldOre or BlockId.RedstoneOre or BlockId.DiamondOre or BlockId.EmeraldOre => 3,
        _ => 1,
    };

    /// <summary>The kind of tool a block breaks fastest to, and whether it drops anything broken without one, as Minecraft's stone does not.</summary>
    public static (ToolKind Tool, bool Needed) ToolFor(BlockId block) => Ores.Plain(Blocks.Get(block).Item) switch
    {
        var wood when BlockGroups.Logs.Contains(wood) || BlockGroups.Planks.Contains(wood) => (ToolKind.Axe, false),
        BlockId.BrownMushroomBlock or BlockId.RedMushroomBlock or BlockId.MushroomStem => (ToolKind.Axe, false),
        var clay when BlockGroups.Terracotta.Contains(clay) => (ToolKind.Pickaxe, true),
        BlockId.CopperOre or BlockId.GoldOre or BlockId.RedstoneOre or BlockId.LapisOre or BlockId.DiamondOre or BlockId.EmeraldOre => (ToolKind.Pickaxe, true),
        BlockId.RedSand or BlockId.Clay => (ToolKind.Shovel, false),
        BlockId.Stone or BlockId.Cobblestone or BlockId.CoalOre or BlockId.IronOre or BlockId.Furnace or BlockId.Sandstone or BlockId.StoneBricks or BlockId.Magma
            or BlockId.Deepslate or BlockId.CobbledDeepslate or BlockId.WhiteConcrete or BlockId.RedConcrete or BlockId.GreenConcrete or BlockId.BlueConcrete
            or BlockId.Lantern or BlockId.SoulLantern or BlockId.StoneSlab or BlockId.CobblestoneSlab or BlockId.SandstoneSlab
            or BlockId.CobblestoneStairs or BlockId.SandstoneStairs or BlockId.StoneBrickStairs => (ToolKind.Pickaxe, true),
        BlockId.Ice => (ToolKind.Pickaxe, false),
        BlockId.OakLog or BlockId.BirchLog or BlockId.SpruceLog or BlockId.OakPlanks or BlockId.SprucePlanks or BlockId.BirchPlanks
            or BlockId.CraftingTable or BlockId.OakFence or BlockId.OakDoor or BlockId.OakSlab or BlockId.SpruceSlab
            or BlockId.OakStairs or BlockId.SpruceStairs => (ToolKind.Axe, false),
        BlockId.Dirt or BlockId.Grass or BlockId.Sand or BlockId.Gravel or BlockId.Snow or BlockId.Farmland or BlockId.DirtPath => (ToolKind.Shovel, false),
        _ => (ToolKind.None, false),
    };

    /// <summary>
    /// How much a block stands against an explosion, Minecraft's blast resistance, which is its
    /// hardness for most, 6 for stone and what is made of it, 3 for wood and coal ore, 100 for
    /// water, and bedrock past any explosion.
    /// </summary>
    public static float Resistance(BlockId block) => Ores.Plain(Blocks.Get(block).Item) switch
    {
        BlockId.Bedrock => 3_600_000,
        var wood when BlockGroups.Logs.Contains(wood) || BlockGroups.Planks.Contains(wood) => 3,
        var clay when BlockGroups.Terracotta.Contains(clay) => 4.2f,
        BlockId.CopperOre or BlockId.GoldOre or BlockId.RedstoneOre or BlockId.LapisOre or BlockId.DiamondOre or BlockId.EmeraldOre => 3,
        BlockId.Water or BlockId.Lava => 100,
        BlockId.Stone or BlockId.Cobblestone or BlockId.StoneBricks or BlockId.StoneSlab or BlockId.CobblestoneSlab
            or BlockId.CobblestoneStairs or BlockId.StoneBrickStairs or BlockId.Deepslate or BlockId.CobbledDeepslate => 6,
        BlockId.OakLog or BlockId.BirchLog or BlockId.SpruceLog or BlockId.OakPlanks or BlockId.SprucePlanks or BlockId.BirchPlanks
            or BlockId.OakSlab or BlockId.SpruceSlab or BlockId.OakStairs or BlockId.SpruceStairs or BlockId.OakFence or BlockId.CoalOre or BlockId.IronOre => 3,
        var item => Hardness(item),
    };

    /// <summary>A block's hardness, Minecraft's, by the item that places it.</summary>
    public static float Hardness(BlockId block) => Blocks.Get(block).Item switch
    {
        // Fire, which places as no item, breaks at once and stops no blast, as the source's does.
        BlockId.Air => 0,
        var deep when Ores.InStone.ContainsKey(deep) => 4.5f,
        BlockId.Torch or BlockId.SoulTorch or BlockId.EndRod or BlockId.Wheat or BlockId.Tnt => 0,
        var plant when BlockGroups.Plants.Contains(plant) => 0,
        var leaves when BlockGroups.Leaves.ContainsKey(leaves) => 0.2f,
        BlockId.BrownMushroomBlock or BlockId.RedMushroomBlock or BlockId.MushroomStem => 0.2f,
        var glass when BlockGroups.Glass.Contains(glass) => 0.3f,
        var wool when BlockGroups.Wool.Contains(wool) => 0.8f,
        var clay when BlockGroups.Terracotta.Contains(clay) => 1.25f,
        BlockId.RedSand => 0.5f,
        BlockId.Clay => 0.6f,
        BlockId.CopperOre or BlockId.GoldOre or BlockId.RedstoneOre or BlockId.LapisOre or BlockId.DiamondOre or BlockId.EmeraldOre => 3,
        BlockId.OakLeaves or BlockId.BirchLeaves or BlockId.SpruceLeaves or BlockId.Snow => 0.2f,
        BlockId.Glass or BlockId.Glowstone or BlockId.SeaLantern => 0.3f,
        BlockId.Cactus => 0.4f,
        BlockId.Dirt or BlockId.Sand or BlockId.Ice or BlockId.Magma or BlockId.HayBale => 0.5f,
        BlockId.Grass or BlockId.Gravel or BlockId.Farmland => 0.6f,
        BlockId.DirtPath => 0.65f,
        BlockId.Sandstone or BlockId.SandstoneStairs or BlockId.WhiteWool or BlockId.LightGrayWool or BlockId.GrayWool
            or BlockId.BlackWool or BlockId.BrownWool or BlockId.PinkWool => 0.8f,
        BlockId.Shroomlight => 1,
        BlockId.Stone or BlockId.StoneBricks or BlockId.StoneBrickStairs => 1.5f,
        BlockId.CraftingTable => 2.5f,
        BlockId.Deepslate => 3,
        BlockId.CobbledDeepslate => 3.5f,
        BlockId.CoalOre or BlockId.IronOre => 3,
        BlockId.Furnace => 3.5f,
        BlockId.WhiteConcrete or BlockId.RedConcrete or BlockId.GreenConcrete or BlockId.BlueConcrete => 1.8f,
        BlockId.OakDoor => 3,
        BlockId.Lantern or BlockId.SoulLantern => 3.5f,
        // Cobblestone, logs, planks, fences, and slabs and stairs of wood and stone.
        _ => 2,
    };
}
