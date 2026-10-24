using System.Numerics;

namespace VoxelSandbox;

/// <summary>
/// The snow the weather lays on the land while it snows, a layer an eighth of a block deep on the
/// highest block of a column whose top takes it, as the source's snow settles, and melts again once
/// the weather has cleared.
/// </summary>
/// <remarks>
/// The columns within 48 blocks of the eye are tried at random, sixty a second, as the source ticks
/// its blocks at random, so the snow settles over minutes rather than at once and melts as slowly. A
/// column takes snow where its biome snows and its top is a whole block's or leaves, the cell over
/// it open, and gives its layer back while the weather is clear.
/// </remarks>
public sealed class SnowCover
{
    /// <summary>How far about the eye the columns are tried, and how many a second.</summary>
    public const int Reach = 48;

    public const float TriesASecond = 60;

    private float _due;

    /// <summary>Lays snow while <paramref name="snowing"/>, melts it while <paramref name="clear"/>, so many columns about the eye tried as so many seconds pass.</summary>
    public void Update(VoxelWorld world, Vector3 eye, bool snowing, bool clear, float seconds, Random random)
    {
        if (!snowing && !clear) return;
        _due += seconds * TriesASecond;
        for (; _due >= 1; _due--)
        {
            var angle = random.NextDouble() * Math.Tau;
            var distance = Reach * Math.Sqrt(random.NextDouble());
            int x = (int)MathF.Floor(eye.X + (float)(Math.Cos(angle) * distance)), z = (int)MathF.Floor(eye.Z + (float)(Math.Sin(angle) * distance));
            Try(world, x, z, snowing);
        }
    }

    /// <summary>Lays a layer on a column's top while snowing where it takes one, or melts the layer it holds while clear.</summary>
    /// <returns>Whether the column changed.</returns>
    public static bool Try(VoxelWorld world, int x, int z, bool snowing)
    {
        var top = world.Top(x, z);
        if (top < ChunkColumn.Bottom) return false;
        var block = world.GetBlock(x, top, z);
        if (!snowing) return block == BlockId.SnowLayer && world.SetBlock(x, top, z, BlockId.Air);
        if (world.BiomeAt(x, z)?.Precipitation != Precipitation.Snow || !Takes(block) || world.GetBlock(x, top + 1, z) != BlockId.Air) return false;
        return world.SetBlock(x, top + 1, z, BlockId.SnowLayer);
    }

    /// <summary>Whether snow settles on a block, a whole block's top or leaves, and not water, ice or a layer of snow.</summary>
    public static bool Takes(BlockId block) =>
        block != BlockId.SnowLayer && !Blocks.IsWater(block) && block != BlockId.Ice && (Blocks.IsSturdy(block) || BlockGroups.Leaves.ContainsKey(block));
}
