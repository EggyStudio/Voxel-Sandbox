using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The rain and the snow the weather lets fall, drawn as the source draws them, in each column
/// within eight of the eye's whose biome they fall on, from over the eye down to the highest block
/// in it, so none falls under a roof, a picture of streaks or flakes scrolling down it.
/// </summary>
/// <remarks>
/// <para>
/// Each column holds two walls of the picture crossed at its middle, seen from both sides. The rain
/// falls ten blocks a second and the snow a block and a half, drifting across as it goes, each
/// column's picture begun at a place of its own so the columns do not fall in step. The pictures
/// are the game's own, a few thin pale streaks and scattered flakes, since none of the default
/// packs holds the source's.
/// </para>
/// <para>
/// The walls are built anew each frame from the columns about the eye, a few thousand corners, and
/// fade in and out with the weather's turn.
/// </para>
/// </remarks>
public sealed class PrecipitationRenderer : IDisposable
{
    private const int Reach = 8, Above = 14, Below = 10, Tall = 4;
    private const float RainFall = 10, SnowFall = 1.5f;

    private readonly Texture2D _rain = Load(RainPixels()), _snow = Load(SnowPixels());
    private ModelMesh _rainMesh, _snowMesh;

    /// <summary>The columns drawn this frame, rain and snow together, for the profile.</summary>
    public int Columns { get; private set; }

    /// <summary>Draws the rain and the snow about the eye as far as the weather has turned, at a time in seconds that scrolls them.</summary>
    public void Draw(VoxelWorld world, Vector3 eye, float share, double time)
    {
        Forget();
        Columns = 0;
        if (share <= 0) return;
        List<ModelVertex> rain = [], snow = [];
        List<uint> rainIndices = [], snowIndices = [];
        int ex = (int)MathF.Floor(eye.X), ez = (int)MathF.Floor(eye.Z);
        for (int z = ez - Reach; z <= ez + Reach; z++)
            for (int x = ex - Reach; x <= ex + Reach; x++)
            {
                // The eye's own column is left out, whose walls would stand across the view.
                var apart = (x - ex) * (x - ex) + (z - ez) * (z - ez);
                if (apart > Reach * Reach || apart == 0) continue;
                var kind = world.BiomeAt(x, z)?.Precipitation ?? Precipitation.None;
                if (kind == Precipitation.None) continue;
                float bottom = MathF.Max(world.Top(x, z) + 1, eye.Y - Below), top = eye.Y + Above;
                if (top <= bottom) continue;
                Columns++;
                var snowing = kind == Precipitation.Snow;
                var start = (PlaceHash.Of(x, z, 7) & 255) / 255f;
                var fallen = (float)(time * (snowing ? SnowFall : RainFall) / Tall) + start;
                var drift = snowing ? (float)(time * 0.15) + start : 0;
                Column(snowing ? snow : rain, snowing ? snowIndices : rainIndices, x, z, bottom, top, fallen, drift);
            }
        var alpha = (byte)Math.Clamp(255 * share, 0, 255);
        if (rain.Count > 0)
        {
            _rainMesh = UploadMesh([.. rain], [.. rainIndices]);
            DrawMesh(_rainMesh, Material(_rain, alpha), Matrix4x4.Identity);
        }
        if (snow.Count > 0)
        {
            _snowMesh = UploadMesh([.. snow], [.. snowIndices]);
            DrawMesh(_snowMesh, Material(_snow, alpha), Matrix4x4.Identity);
        }
    }

    // Two walls crossed in a column's middle, along x and along z, the picture's rows down them by
    // the height in blocks over four and its columns across with the drift.
    private static void Column(List<ModelVertex> vertices, List<uint> indices, int x, int z, float bottom, float top, float fallen, float drift)
    {
        float vTop = top / Tall + fallen, vBottom = bottom / Tall + fallen;
        foreach (var (near, far, normal) in new[]
        {
            (new Vector3(x, 0, z + 0.5f), new Vector3(x + 1, 0, z + 0.5f), Vector3.UnitZ),
            (new Vector3(x + 0.5f, 0, z), new Vector3(x + 0.5f, 0, z + 1), Vector3.UnitX),
        })
        {
            var first = (uint)vertices.Count;
            vertices.Add(new ModelVertex(near with { Y = bottom }, normal, new Vector2(drift, vBottom)));
            vertices.Add(new ModelVertex(far with { Y = bottom }, normal, new Vector2(drift + 1, vBottom)));
            vertices.Add(new ModelVertex(far with { Y = top }, normal, new Vector2(drift + 1, vTop)));
            vertices.Add(new ModelVertex(near with { Y = top }, normal, new Vector2(drift, vTop)));
            indices.AddRange([first, first + 1, first + 2, first, first + 2, first + 3]);
        }
    }

    private static ModelMaterial Material(Texture2D texture, byte alpha) =>
        new(new Color(255, 255, 255, alpha), texture) { AlphaMode = MaterialAlphaMode.Blend, DoubleSided = true, CastsShadows = false, BouncesLight = false, Roughness = 1 };

    // Lets go of the last frame's walls, which that frame has drawn.
    private void Forget()
    {
        if (_rainMesh.IsValid) UnloadMesh(_rainMesh);
        if (_snowMesh.IsValid) UnloadMesh(_snowMesh);
        (_rainMesh, _snowMesh) = (default, default);
    }

    private static Texture2D Load(byte[] pixels)
    {
        var texture = LoadTextureFromImage(new Image(pixels, Side, Side * Tall));
        SetTextureFilter(texture, TextureFilter.Point);
        return texture;
    }

    // The pictures' width in pixels, a block's, and their height, four blocks'.
    private const int Side = 16;

    /// <summary>The rain's picture, a block wide and four tall, nine thin pale blue streaks four to nine pixels long on nothing, faint at their tops.</summary>
    public static byte[] RainPixels()
    {
        var pixels = new byte[Side * Side * Tall * 4];
        var random = new Random(31);
        for (int streak = 0; streak < 9; streak++)
        {
            int x = random.Next(Side), y = random.Next(Side * Tall), length = 4 + random.Next(6);
            for (int i = 0; i < length; i++)
                Paint(pixels, x, (y + i) % (Side * Tall), 170, 190, 230, (byte)(50 + 80 * i / length));
        }
        return pixels;
    }

    /// <summary>The snow's picture, a block wide and four tall, a dozen white flakes of one and two pixels on nothing.</summary>
    public static byte[] SnowPixels()
    {
        var pixels = new byte[Side * Side * Tall * 4];
        var random = new Random(37);
        for (int flake = 0; flake < 12; flake++)
        {
            int x = random.Next(Side), y = random.Next(Side * Tall);
            Paint(pixels, x, y, 250, 250, 255, 190);
            if (random.Next(4) == 0) Paint(pixels, (x + 1) % Side, y, 240, 240, 250, 150);
        }
        return pixels;
    }

    private static void Paint(byte[] pixels, int x, int y, byte r, byte g, byte b, byte a)
    {
        var at = (y * Side + x) * 4;
        (pixels[at], pixels[at + 1], pixels[at + 2], pixels[at + 3]) = (r, g, b, Math.Max(pixels[at + 3], a));
    }

    public void Dispose()
    {
        Forget();
        UnloadTexture(_rain);
        UnloadTexture(_snow);
    }
}
