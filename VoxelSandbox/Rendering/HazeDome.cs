using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>
/// The dome about the eye below the horizon, drawn in the haze's color, so the land's fading edge
/// meets a sky of the color it fades into, as Minecraft draws what lies below its horizon in its
/// fog's color, and a band over the horizon fading into the sky above it.
/// </summary>
/// <remarks>
/// The dome is drawn through the terrain shader beyond the haze's end, which gives every pixel of it
/// the haze's color itself, after the same light and the same steps to the screen as the land's
/// edge, so the two meet with no step, under water as above it and whatever color the haze takes.
/// The sky's image keeps its darker lower half for the light it gives the world. The dome lies
/// beyond the farthest render distance's haze, and the land and the water, nearer, hide it where
/// they stand. It lies beyond the sun, the moon and the stars too, inside the far plane, since it
/// writes depth over its whole band, its clear top too, and the sky's bodies drawn behind it would
/// end in a line at the band's top. They are drawn over it instead, each taking the share of the
/// haze <see cref="Shown"/> leaves clear at its height.
/// </remarks>
public static class HazeDome
{
    /// <summary>How far the dome lies from the eye, past the haze's end at the farthest render distance and the stars, and inside the engine's far plane of 4000.</summary>
    public const float Radius = 3600;

    // Rings of the dome by their height above the horizon in degrees, and how much of the haze each
    // shows, nothing at its top, the whole from the horizon down.
    private static readonly (float Degrees, float Shown)[] Rings =
        [(6, 0), (3, 0.35f), (1.5f, 0.75f), (0, 1), (-3, 1), (-15, 1), (-40, 1), (-70, 1), (-90, 1)];

    private const int Around = 48;

    /// <summary>How much of the haze the dome shows at a height above the horizon in degrees, between its rings as its faces blend it.</summary>
    public static float Shown(float degrees)
    {
        if (degrees >= Rings[0].Degrees) return Rings[0].Shown;
        for (int ring = 1; ring < Rings.Length; ring++)
        {
            var (low, high) = (Rings[ring], Rings[ring - 1]);
            if (degrees >= low.Degrees) return low.Shown + (high.Shown - low.Shown) * (degrees - low.Degrees) / (high.Degrees - low.Degrees);
        }
        return Rings[^1].Shown;
    }

    /// <summary>The dome's faces about the origin, each vertex white with the alpha of its ring, its corners counterclockwise seen from the middle.</summary>
    public static SectionMesher.Faces Build()
    {
        var faces = new SectionMesher.Faces();
        for (int ring = 0; ring + 1 < Rings.Length; ring++)
            for (int step = 0; step < Around; step++)
            {
                Vector3 Point(int r, int s)
                {
                    var (elevation, turn) = (float.DegreesToRadians(Rings[r].Degrees), s * MathF.Tau / Around);
                    return Radius * new Vector3(MathF.Cos(elevation) * MathF.Cos(turn), MathF.Sin(elevation), MathF.Cos(elevation) * MathF.Sin(turn));
                }
                Span<(Vector3 At, float Shown)> quad =
                [
                    (Point(ring, step), Rings[ring].Shown), (Point(ring, step + 1), Rings[ring].Shown),
                    (Point(ring + 1, step + 1), Rings[ring + 1].Shown), (Point(ring + 1, step), Rings[ring + 1].Shown),
                ];
                var first = (uint)faces.Count;
                // Each corner carries the open sky's level, so the terrain shader gives the dome its
                // haze whole wherever the eye stands.
                for (int i = 0; i < 4; i++) faces.Add(quad[i].At, 2, i, 1, new Color(255, 255, 255, (byte)(255 * quad[i].Shown)), sky: Lighting.Max);
                // Each triangle wound so the side toward the middle is the front the model pass draws,
                // and the one of no area where the lowest ring meets at a point left out.
                foreach (var (i, j, k) in (ReadOnlySpan<(int, int, int)>)[(0, 1, 2), (0, 2, 3)])
                {
                    var normal = Vector3.Cross(quad[j].At - quad[i].At, quad[k].At - quad[i].At);
                    if (normal.LengthSquared() < 1e-3f) continue;
                    var inward = Vector3.Dot(normal, -(quad[i].At + quad[j].At + quad[k].At)) > 0;
                    faces.Indices.AddRange(inward ? [first + (uint)i, first + (uint)j, first + (uint)k] : [first + (uint)i, first + (uint)k, first + (uint)j]);
                }
            }
        return faces;
    }
}
