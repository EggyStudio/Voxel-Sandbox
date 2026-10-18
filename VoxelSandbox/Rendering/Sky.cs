using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The time of day and the count of days, the sun or the moon as the light that casts the shadows,
/// a painted sky that lights the world from all around and is drawn behind it, and the sun, the
/// moon and the stars drawn over the sky as billboards.
/// </summary>
/// <remarks>
/// <para>
/// The sky is an equirectangular image painted here, a smooth gradient from the horizon to the
/// zenith colored by the hour, given to <c>SetEnvironmentMap</c>, which filters it on the GPU.
/// Global illumination brings the sky's light back where a ray of its last cascade meets nothing,
/// and weighs the sky's reflection by how much of the sky a surface's probes found, so a cave lit by
/// no lamp is dark. The image is painted again only when the time has moved by
/// <see cref="RepaintHours"/>, since each painting is a new map to filter.
/// </para>
/// <para>
/// The sun, the glow around it, the moon and the stars are pixel art (<see cref="SkyArt"/>) on
/// billboards facing the camera, sampled without filtering, so their pixels stay square however
/// large they are drawn. The glow's pixels are the sun's, its billboard's middle the sun's, so its
/// rings step along the sun's grid, and it is added to the sky behind it.
/// The sky turns about the axis from north to south, the sun on one side of it and the moon on the
/// other, and the stars keep their places on it, so they turn with the sun and the moon. They are
/// drawn far out, inside the far plane and in front of the sky's backdrop, so the land and the
/// clouds hide them.
/// </para>
/// </remarks>
public sealed class Sky : IDisposable
{
    private const int Width = 256;
    private const int Height = 128;
    private const float RepaintHours = 0.05f;

    // How far out the sun and the moon are drawn, and the stars behind them, and the degrees each
    // pixel of their art spans, the sun's 12 pixels about nine degrees across.
    private const float Distance = 3000, StarDistance = 3200;
    private const float SunPixel = 0.75f, MoonPixel = 0.55f, StarPixel = 0.16f;
    private const int StarCount = 500;

    private static readonly Vector3 NightZenith = new(6, 9, 22), DayZenith = new(78, 134, 222);
    private static readonly Vector3 NightHorizon = new(18, 24, 44), DayHorizon = new(178, 208, 240), DuskHorizon = new(238, 150, 96);
    private static readonly Vector3 Noon = new(255, 244, 228), LowSun = new(255, 160, 90), Moonlight = new(150, 170, 230);

    // Each star's way on the sky as it stands at 6, when the sun rises, its brightness, and whether
    // it is one of the bright ones drawn as a cross of five pixels rather than a single one.
    private static readonly (Vector3 Way, float Brightness, bool Bright)[] Stars = PlaceStars();

    private readonly LightHandle _light;
    private readonly byte[] _pixels = new byte[Width * Height * 4];
    private readonly Texture2D _sun, _halo, _moon, _star;
    private float _litHour = float.NaN, _paintedHour = float.NaN;

    public Sky(float hour)
    {
        Hour = hour;
        _light = CreateDirectionalLight(-Vector3.UnitY, Color.White, 1, castsShadows: true);
        _sun = Load(SkyArt.SunPixels(), SkyArt.Size, SkyArt.Size);
        _halo = Load(SkyArt.HaloPixels(SunPixel), SkyArt.HaloSize, SkyArt.HaloSize);
        _moon = Load(SkyArt.MoonPixels(), SkyArt.Cell * 4, SkyArt.Cell * 2);
        _star = Load(SkyArt.StarPixels(), 3, 3);
        Apply();
    }

    /// <summary>The hour of the day, from 0 to 24, the sun rising at 6 in the east and setting at 18 in the west.</summary>
    public float Hour { get; set; }

    /// <summary>How many days have passed, one more at each midnight while <see cref="Cycle"/> is set, which the moon's phase follows.</summary>
    public int Day { get; set; }

    /// <summary>Whether the time moves on by itself.</summary>
    public bool Cycle { get; set; }

    /// <summary>How many minutes a whole day takes while <see cref="Cycle"/> is set.</summary>
    public float DayMinutes { get; set; } = 20;

    /// <summary>The color of the sky at the horizon, which the frame is cleared to behind the sky.</summary>
    public Color Horizon { get; private set; }

    public void Update(float seconds)
    {
        if (Cycle) (Hour, Day) = Advance(Hour, Day, seconds * 24 / (DayMinutes * 60));
        Apply();
    }

    /// <summary>The hour and the day so many hours on from an hour of a day.</summary>
    public static (float Hour, int Day) Advance(float hour, int day, float hours)
    {
        var next = hour + hours;
        var days = (int)MathF.Floor(next / 24);
        return (next - days * 24, day + days);
    }

    /// <summary>
    /// The phase of the moon, from 0 for the full moon to 4 for the new one and on to 7, of the
    /// night an hour of a day falls in, the full moon on the night the first day ends with, as
    /// Minecraft's first night has.
    /// </summary>
    /// <remarks>
    /// A night belongs to the day whose evening it begins on, so the hours before noon show the
    /// phase of the day before, and the moon changes while it is down rather than while it is seen.
    /// </remarks>
    public static int MoonPhase(int day, float hour) => ((hour < 12 ? day - 1 : day) % SkyArt.Phases + SkyArt.Phases) % SkyArt.Phases;

    /// <summary>The way toward the sun at an hour, of length one, tilted toward the south a little so the shadows of the blocks do not run along their edges at noon.</summary>
    public static Vector3 SunToward(float hour) => Vector3.Normalize(Turned(hour, new Vector3(1, 0, 0.3f)));

    /// <summary>
    /// A way on the sky as it stands at 6, turned as the sky turns by an hour, about the axis from
    /// north to south, so the sun goes from the east at 6 up over the south to the west at 18.
    /// </summary>
    public static Vector3 Turned(float hour, Vector3 way) => Vector3.Transform(way, Turn(hour));

    // The sky's turn at an hour since 6.
    private static Matrix4x4 Turn(float hour) => Matrix4x4.CreateRotationZ((hour - 6) / 12 * MathF.PI);

    /// <summary>
    /// Draws the stars, the moon and the sun around the camera's eye, the stars fading in as the
    /// day fades and near the horizon, and the sun and the moon fading as they set. Called inside
    /// <c>BeginMode3D</c>.
    /// </summary>
    public void Draw(Camera3D camera)
    {
        var eye = camera.Position;
        var view = camera.View;
        var up = new Vector3(view.M12, view.M22, view.M32);
        var toSun = SunToward(Hour);

        var night = 1 - DayShare(toSun);
        var turn = Turn(Hour);
        if (night > 0.01f)
            foreach (var (way, brightness, bright) in Stars)
            {
                var toStar = Vector3.Transform(way, turn);
                var alpha = night * brightness * SmoothStep(0.0f, 0.15f, toStar.Y);
                if (alpha < 0.01f) continue;
                var source = bright ? new Rectangle(0, 0, 3, 3) : new Rectangle(1, 1, 1, 1);
                Billboard(camera, _star, source, eye + toStar * StarDistance, up, Pixel(StarDistance, StarPixel), alpha);
            }

        var moon = MoonPhase(Day, Hour);
        Billboard(camera, _moon, new Rectangle(moon % 4 * SkyArt.Cell + 1, moon / 4 * SkyArt.Cell + 1, SkyArt.Size, SkyArt.Size),
            eye - toSun * Distance, up, Pixel(Distance, MoonPixel), Setting(-toSun));

        // The glow, warmer and stronger at dawn and dusk, gone a little after the sun has set. It
        // writes no depth, since the sun is drawn over it at the same distance and would fail
        // against it where the two quads' depths part by a rounding.
        var dusk = DuskShare(toSun);
        var glow = (0.25f + 0.5f * dusk) * SmoothStep(-0.2f, 0, toSun.Y);
        if (glow > 0.01f)
        {
            BeginBlendMode(BlendMode.Additive);
            rlDisableDepthMask();
            Billboard(camera, _halo, new Rectangle(0, 0, SkyArt.HaloSize, SkyArt.HaloSize), eye + toSun * Distance, up, Pixel(Distance, SunPixel), glow,
                ColorOf(Vector3.Lerp(Noon, DuskHorizon, dusk)));
            rlEnableDepthMask();
            EndBlendMode();
        }
        Billboard(camera, _sun, new Rectangle(0, 0, SkyArt.Size, SkyArt.Size), eye + toSun * Distance, up, Pixel(Distance, SunPixel), Setting(toSun));
    }

    public void Dispose()
    {
        UnloadTexture(_sun);
        UnloadTexture(_halo);
        UnloadTexture(_moon);
        UnloadTexture(_star);
    }

    // A billboard of part of a texture, each of its pixels so wide, at an alpha and in a color,
    // white unless given.
    private static void Billboard(Camera3D camera, Texture2D texture, Rectangle source, Vector3 at, Vector3 up, float pixel, float alpha, Color? color = null)
    {
        if (alpha <= 0) return;
        var size = new Vector2(source.Width, source.Height) * pixel;
        var (r, g, b) = color is { } c ? (c.R, c.G, c.B) : ((byte)255, (byte)255, (byte)255);
        DrawBillboardPro(camera, texture, source, at, up, size, size / 2, 0, new Color(r, g, b, (byte)(255 * Math.Clamp(alpha, 0, 1))));
    }

    // How wide a pixel so many degrees across is at a distance, the same for every pixel of a
    // billboard, so two billboards of one pixel's size share a grid where their middles meet.
    private static float Pixel(float distance, float degrees) => distance * MathF.Tan(float.DegreesToRadians(degrees));

    // How much of the sun or the moon shows as it sets, gone once its middle is a little below the
    // horizon, where the sky's backdrop beyond the world's edge would show it.
    private static float Setting(Vector3 toward) => SmoothStep(-0.12f, 0.02f, toward.Y);

    // How much of the day's light the sky has, 0 at night and 1 once the sun is well up.
    private static float DayShare(Vector3 toSun) => SmoothStep(-0.15f, 0.2f, toSun.Y);

    // How much of dawn's or dusk's color the sky takes, most as the sun crosses the horizon.
    private static float DuskShare(Vector3 toSun) => SmoothStep(-0.2f, 0.0f, toSun.Y) * (1 - SmoothStep(0.0f, 0.3f, toSun.Y));

    private static Texture2D Load(byte[] pixels, int width, int height)
    {
        var texture = LoadTextureFromImage(new Image(pixels, width, height));
        SetTextureFilter(texture, TextureFilter.Point);
        SetTextureWrap(texture, TextureWrap.Clamp);
        return texture;
    }

    // The stars, spread evenly over the sky from a fixed hash, so each run has the same ones.
    private static (Vector3, float, bool)[] PlaceStars()
    {
        var stars = new (Vector3, float, bool)[StarCount];
        for (int i = 0; i < StarCount; i++)
        {
            float a = PlaceHash.Of(i, 0, 7) / (float)uint.MaxValue, b = PlaceHash.Of(i, 1, 7) / (float)uint.MaxValue;
            var shine = PlaceHash.Of(i, 2, 7);
            var z = a * 2 - 1;
            var ring = MathF.Sqrt(1 - z * z);
            var turn = b * 2 * MathF.PI;
            stars[i] = (new Vector3(ring * MathF.Cos(turn), ring * MathF.Sin(turn), z), 0.45f + 0.55f * (shine % 1000) / 999f, shine % 8 == 0);
        }
        return stars;
    }

    private void Apply()
    {
        if (Hour == _litHour) return;
        _litHour = Hour;
        var toSun = SunToward(Hour);

        // The sun by day and the moon opposite it by night, each fading out where the light turns
        // from one to the other, so the shadows do not jump with it.
        if (toSun.Y > -0.05f)
        {
            var low = 1 - SmoothStep(0, 0.35f, toSun.Y);
            SetLightDirection(_light, -toSun);
            SetLightColor(_light, ColorOf(Vector3.Lerp(Noon, LowSun, low)), 2.2f * SmoothStep(-0.05f, 0.1f, toSun.Y));
        }
        else
        {
            SetLightDirection(_light, toSun);
            SetLightColor(_light, ColorOf(Moonlight), 0.25f * SmoothStep(0.05f, 0.2f, -toSun.Y));
        }

        if (!float.IsNaN(_paintedHour) && HoursApart(Hour, _paintedHour) < RepaintHours) return;
        _paintedHour = Hour;
        Paint(toSun);
        SetEnvironmentMap(new Image(_pixels, Width, Height));
    }

    private void Paint(Vector3 toSun)
    {
        var day = DayShare(toSun);
        var dusk = DuskShare(toSun);
        var zenith = Vector3.Lerp(NightZenith, DayZenith, day);
        var horizon = Vector3.Lerp(Vector3.Lerp(NightHorizon, DayHorizon, day), DuskHorizon, dusk * 0.8f);
        var ground = horizon * 0.5f;
        Horizon = ColorOf(horizon);

        for (int py = 0; py < Height; py++)
        {
            // Down from the top of the image, as the engine reads an equirectangular map.
            var theta = (py + 0.5f) / Height * MathF.PI;
            for (int px = 0; px < Width; px++)
            {
                var longitude = ((px + 0.5f) / Width - 0.5f) * 2 * MathF.PI;
                var d = new Vector3(MathF.Sin(longitude) * MathF.Sin(theta), MathF.Cos(theta), -MathF.Cos(longitude) * MathF.Sin(theta));

                var color = d.Y >= 0
                    ? Vector3.Lerp(zenith, horizon, MathF.Pow(1 - d.Y, 4))
                    // Below the horizon the sky keeps the horizon's color a while before darkening,
                    // so the haze at the world's edge meets a sky of its own color.
                    : Vector3.Lerp(horizon, ground, SmoothStep(0.1f, 0.6f, -d.Y));

                var at = (py * Width + px) * 4;
                _pixels[at] = (byte)Math.Clamp(color.X, 0, 255);
                _pixels[at + 1] = (byte)Math.Clamp(color.Y, 0, 255);
                _pixels[at + 2] = (byte)Math.Clamp(color.Z, 0, 255);
                _pixels[at + 3] = 255;
            }
        }
    }

    private static float HoursApart(float a, float b)
    {
        var apart = MathF.Abs(a - b) % 24;
        return MathF.Min(apart, 24 - apart);
    }

    private static float SmoothStep(float from, float to, float x)
    {
        var t = Math.Clamp((x - from) / (to - from), 0, 1);
        return t * t * (3 - 2 * t);
    }

    private static Color ColorOf(Vector3 rgb) =>
        new((byte)Math.Clamp(rgb.X, 0, 255), (byte)Math.Clamp(rgb.Y, 0, 255), (byte)Math.Clamp(rgb.Z, 0, 255));
}
