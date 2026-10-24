namespace VoxelSandbox;

/// <summary>
/// The atmosphere's haze over the land, growing with the distance from the eye, thinning with the
/// height above the sea and thickest about sunrise, as the terrain shader draws it, which this
/// reckons in the same numbers so the tests read it.
/// </summary>
/// <remarks>
/// <para>
/// The haze's share at a point is the most of it times one less e to the minus the cube of the
/// distance times its growth, as the modpack's shader's, with the growth taken by the density along
/// the ray from the eye. The density is whole at the sea's level and under it and falls by e over
/// each <see cref="Scale"/> blocks above it, so a valley holds its haze and a peak eighty blocks over
/// the sea about two fifths of it, its mean along the ray reckoned whole rather than sampled.
/// </para>
/// <para>
/// The morning thickens it, as the land's mist at dawn does, the growth two and a half times its
/// noon's at a quarter past six and back to it by nine, and the evening leaves it as at noon.
/// </para>
/// </remarks>
public static class Haze
{
    /// <summary>The height the haze is whole at and under, the sea's level.</summary>
    public const float Base = Overworld.SeaLevel;

    /// <summary>The blocks above <see cref="Base"/> over which the haze thins by e.</summary>
    public const float Scale = 40;

    /// <summary>The mean of the haze's density along the ray from an eye at a height to a point at another, 1 at and under the sea's level.</summary>
    public static float HeightShare(float eyeY, float pointY)
    {
        float h0 = MathF.Max(eyeY - Base, 0) / Scale, h1 = MathF.Max(pointY - Base, 0) / Scale;
        var across = h1 - h0;
        return MathF.Abs(across) < 1e-3f ? MathF.Exp(-h0) : (MathF.Exp(-h0) - MathF.Exp(-h1)) / across;
    }

    /// <summary>The atmosphere's share of a point's color, by its distance and the heights of the eye and the point, thinned by height where <paramref name="byHeight"/> is set.</summary>
    public static float Aerial(float distance, float eyeY, float pointY, float most, float growth, bool byHeight = true)
    {
        var density = byHeight ? HeightShare(eyeY, pointY) : 1;
        return most * (1 - MathF.Exp(-distance * distance * distance * 1e-6f * growth * density));
    }

    /// <summary>How many times its noon's the haze grows at an hour, two and a half at a quarter past six, falling to one by nine and before four.</summary>
    public static float Dawn(float hour)
    {
        var from = (hour - 6.25f) / 1.25f;
        return 1 + 1.5f * MathF.Exp(-from * from);
    }
}
