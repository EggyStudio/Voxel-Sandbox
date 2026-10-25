using System.Numerics;

namespace VoxelSandbox;

/// <summary>Where the camera stands, at the player's eye, behind the player looking at it, or in front of it looking back.</summary>
public enum View
{
    First,
    Behind,
    Front,
}

/// <summary>
/// The camera of a view from without, four blocks from the eye along the look, as the source's
/// third-person camera stands, pulled in where a block would stand between it and the eye.
/// </summary>
/// <remarks>
/// As the source does, rays go from the eye toward where the camera would stand from five points,
/// the eye and four a tenth of a block from it across the way, so a corner the middle ray passes
/// does not show its inside, and the camera stands a tenth of a block short of the nearest block any
/// of them meets.
/// </remarks>
public static class ThirdPerson
{
    /// <summary>How far the camera stands from the eye where nothing is in the way, and how far short of a block it stops.</summary>
    public const float Away = 4, Short = 0.1f;

    /// <summary>How far from the eye the camera stands along a way, the full <see cref="Away"/> or short of the nearest block.</summary>
    public static float Distance(VoxelWorld world, Vector3 eye, Vector3 way)
    {
        way = Vector3.Normalize(way);
        var across = Vector3.Normalize(Vector3.Cross(way, MathF.Abs(way.Y) > 0.99f ? Vector3.UnitX : Vector3.UnitY));
        var up = Vector3.Cross(across, way);
        var nearest = Away;
        foreach (var offset in (ReadOnlySpan<Vector3>)[Vector3.Zero, across * Short, -across * Short, up * Short, -up * Short])
            if (VoxelRay.Cast(world, eye + offset, way, Away) is { } hit) nearest = MathF.Min(nearest, hit.Distance);
        return nearest < Away ? MathF.Max(0, nearest - Short) : Away;
    }

    /// <summary>The next view in the order F5 steps them, the eye's, behind and in front.</summary>
    public static View Next(View view) => (View)(((int)view + 1) % 3);
}
