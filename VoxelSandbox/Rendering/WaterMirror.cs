using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The plane calm water mirrors the shore and the sky in, set each frame at the top of the water most
/// of the view looks down on, and none where no water is in view, the eye is under water or the
/// setting is off, so the engine draws its mirrored view of the scene only while water shows it.
/// </summary>
/// <remarks>
/// The engine mirrors one plane, so water at another height, a pool above a river, takes the sky's
/// light alone, as it does with the setting off.
/// </remarks>
public sealed class WaterMirror
{
    // How far a ray looks for water, past where the haze begins at the default twelve columns, 125 blocks off.
    private const float Reach = 128;

    /// <summary>Whether the water mirrors the shore, the setting.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>The height of the plane set this frame, or null for none.</summary>
    public float? Height { get; private set; }

    /// <summary>Sets the plane at the water in view through a camera, or none.</summary>
    public void Update(VoxelWorld world, Camera3D camera, bool underWater)
    {
        Height = Enabled && !underWater ? Find(world, camera) : null;
        SetReflectionPlane(Height ?? 0, Height is null ? Vector3.Zero : Vector3.UnitY);
    }

    // The water's top most of fifteen rays meet, five across the window in three rows from its middle
    // down, where water seen from the land lies.
    private static float? Find(VoxelWorld world, Camera3D camera)
    {
        var seen = new Dictionary<float, int>();
        int width = GetScreenWidth(), height = GetScreenHeight();
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < 5; column++)
            {
                var ray = GetScreenToWorldRay(new Vector2(width * (column + 0.5f) / 5, height * (0.5f + 0.2f * row)), camera);
                if (WaterTop(world, ray.Position, ray.Direction) is { } top) seen[top] = seen.GetValueOrDefault(top) + 1;
            }
        return seen.Count == 0 ? null : seen.MaxBy(pair => pair.Value).Key;
    }

    /// <summary>
    /// The top of the first water a ray meets in half-block steps, as the water's own faces stand, or
    /// none where a block that hides what is behind it comes first or none lies within reach.
    /// </summary>
    public static float? WaterTop(VoxelWorld world, Vector3 from, Vector3 way)
    {
        for (float along = 0; along < Reach; along += 0.5f)
        {
            var at = from + way * along;
            int x = (int)MathF.Floor(at.X), y = (int)MathF.Floor(at.Y), z = (int)MathF.Floor(at.Z);
            var block = world.GetBlock(x, y, z);
            if (Blocks.IsWater(block)) return y + Blocks.WaterHeight(block, Blocks.IsWater(world.GetBlock(x, y + 1, z)));
            if (Blocks.IsOpaque(block)) return null;
        }
        return null;
    }
}
