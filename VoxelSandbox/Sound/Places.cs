using System.Numerics;

namespace VoxelSandbox;

/// <summary>Which step a block sounds, and which sounds of a place play where the listener stands.</summary>
public static class Places
{
    /// <summary>How far a body walks on the ground between two steps heard, in blocks, about the source's.</summary>
    public const float StepLength = 1.6f;

    /// <summary>The height over the sea where the wind begins and where it blows at its loudest.</summary>
    public const float WindFrom = 32, WindFull = 80;

    /// <summary>
    /// The step a body standing at a place sounds, the water's where its feet are in water, the
    /// snow's on a layer of snow, and otherwise the block's under its feet.
    /// </summary>
    public static StepSound StepAt(VoxelWorld world, Vector3 feet)
    {
        int x = (int)MathF.Floor(feet.X), y = (int)MathF.Floor(feet.Y), z = (int)MathF.Floor(feet.Z);
        var at = world.GetBlock(x, y, z);
        if (Blocks.IsWater(at)) return StepSound.Water;
        if (at == BlockId.SnowLayer) return StepSound.Snow;
        return StepOf(world.GetBlock(x, (int)MathF.Floor(feet.Y - 0.2f), z));
    }

    /// <summary>The step a block sounds walked on, the source's sound groups made seven.</summary>
    public static StepSound StepOf(BlockId block)
    {
        if (Blocks.IsWater(block)) return StepSound.Water;
        if (block is BlockId.Snow or BlockId.SnowLayer || BlockGroups.Wool.Contains(block)) return StepSound.Snow;
        if (block is BlockId.Sand or BlockId.RedSand) return StepSound.Sand;
        if (block == BlockId.Gravel) return StepSound.Gravel;
        if (block is BlockId.Grass or BlockId.Dirt or BlockId.MossBlock || Blocks.IsLeaves(block) || BlockGroups.Plants.Contains(block)
            || Blocks.Get(block).Key is "farmland" or "podzol" or "mycelium" or "coarse_dirt" or "rooted_dirt" or "mud" or "hay_block") return StepSound.Grass;
        var key = Blocks.Get(block).Key;
        if (BlockGroups.Logs.Contains(block) || BlockGroups.Planks.Contains(block) || key.Contains("planks") || key.Contains("_log") || key.Contains("_wood")
            || key is "crafting_table" or "bookshelf" or "chest" or "barrel" || key.Contains("door") || key.Contains("fence")) return StepSound.Wood;
        return StepSound.Stone;
    }

    /// <summary>What of a place the sounds of the place are chosen by.</summary>
    /// <param name="Biome">The biome of the listener's column, or none where it is not loaded.</param>
    /// <param name="Hour">The hour of the day.</param>
    /// <param name="InCave">Whether the listener stands in a cave, under the sea's level with little of the sky's light.</param>
    /// <param name="Height">How far over the sea the listener stands.</param>
    /// <param name="Water">The share of the blocks about the listener that are water, from 0 to 1.</param>
    /// <param name="Weather">How far the weather has turned, from 0 clear to 1.</param>
    public readonly record struct Place(Biome? Biome, float Hour, bool InCave, float Height, float Water, float Weather);

    /// <summary>
    /// How loud each sound of a place plays at one, from 0 to 1, by <see cref="AmbientSound"/>: the
    /// birds in the woods by day, the crickets by night where it is neither cold nor dry, the wind
    /// rising with the height over the sea, a cave's drips and low tone in a cave, and the water by
    /// how much of it lies about, the birds and the crickets hushed by the weather.
    /// </summary>
    public static float[] Levels(Place place)
    {
        var levels = new float[Enum.GetValues<AmbientSound>().Length];
        var day = place.Hour is >= 5.5f and < 19.5f;
        var calm = 1 - 0.8f * place.Weather;
        if (place.InCave)
        {
            levels[(int)AmbientSound.Cave] = 1;
            levels[(int)AmbientSound.Water] = place.Water;
            return levels;
        }
        if (place.Biome is { } biome)
        {
            if (day && Wooded(biome)) levels[(int)AmbientSound.Birds] = calm;
            if (!day && biome.Precipitation == Precipitation.Rain && biome != Biomes.Mountains) levels[(int)AmbientSound.Crickets] = 0.8f * calm;
        }
        levels[(int)AmbientSound.Wind] = Math.Clamp((place.Height - WindFrom) / (WindFull - WindFrom), 0, 1);
        levels[(int)AmbientSound.Water] = place.Water;
        return levels;
    }

    private static bool Wooded(Biome biome) =>
        biome == Biomes.Forest || biome == Biomes.BirchForest || biome == Biomes.DarkForest || biome == Biomes.Taiga || biome == Biomes.SnowyTaiga
        || biome == Biomes.Jungle || biome == Biomes.Swamp || biome == Biomes.CherryGrove || biome == Biomes.Plains;
}
