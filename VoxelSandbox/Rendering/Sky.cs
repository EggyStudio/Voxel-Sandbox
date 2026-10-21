using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The time of day and the count of days, the sun or the moon as the light that casts the shadows,
/// a painted sky that lights the world from all around and is drawn behind it, and the sun, the
/// moon and the stars fixed on the sky.
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
/// The sun and the moon, each a square body with its glow around it, are pixel art
/// (<see cref="SkyArt"/>) on quads fixed on the sky, sampled without filtering, so their pixels
/// stay square however large they are drawn, and each star is a plain square. A quad lies across the way to its middle, its right
/// side along the way the sky turns there and its up across that, so where it stands and which way
/// its sides run depend on the hour alone and never on which way the camera looks, and the sky
/// turns them all as one. The sun and the moon are added to the sky behind them, as Minecraft adds
/// its own, and where a resource pack has its own pictures of them, those are drawn in their place.
/// The sky turns about the axis from north to south, the sun on one side of it and the moon on the
/// other, and the stars keep their places on it, so they turn with the sun and the moon. They are
/// drawn far out, inside the far plane and in front of the sky's backdrop, so the land and the
/// clouds hide them.
/// </para>
/// <para>
/// They are drawn in front of the haze dome too (<see cref="HazeDome"/>), which lies beyond them,
/// and each takes the share of the dome's haze left clear at its height instead, the sun and the
/// moon as a grid of a quad to each pixel of their pictures with that share at each corner. So a
/// sun sinking into the haze dims under it as the dome blended over it would show it, where behind
/// the dome, which writes depth over its whole band, it would end in a line at the band's clear top.
/// </para>
/// </remarks>
public sealed class Sky : IDisposable
{
    private const int Width = 256;
    private const int Height = 128;
    private const float RepaintHours = 0.05f;

    /// <summary>How far out the sun and the moon are drawn.</summary>
    public const float Distance = 3000;

    /// <summary>How far out the stars are drawn, behind the sun and the moon.</summary>
    public const float StarDistance = 3200;

    // The sun's and the moon's bodies are as wide as Minecraft's against their distance, its sun the
    // middle 8 pixels of a texture of 32 on a quad 60 across at a distance of 100, so 15 at 100,
    // about 8.6 degrees, and its moon the same 8 of 32 on a quad 40 across, so 10 at 100, about 5.7
    // degrees. A star's pixel spans 0.16 degrees.
    private const float SunAcross = 0.15f, MoonAcross = 0.10f;

    /// <summary>How many stars the sky holds.</summary>
    public const int StarCount = 3000;

    /// <summary>How many degrees each star's square is across.</summary>
    public const float StarDegrees = 0.28f;

    // The way toward the sun as the sky stands at 6, tilted toward the south a little so the
    // shadows of the blocks do not run along their edges at noon, the moon opposite it.
    private static readonly Vector3 SunWay = Vector3.Normalize(new Vector3(1, 0, 0.3f));

    private static readonly Vector3 NightZenith = new(6, 9, 22), DayZenith = new(78, 134, 222);
    private static readonly Vector3 NightHorizon = new(18, 24, 44), DayHorizon = new(178, 208, 240), DuskHorizon = new(238, 150, 96);
    private static readonly Vector3 Moonlight = new(150, 170, 230);

    // The sunlight of the modpack's shader, Aurora's Shaders after Complementary, read as numbers, in
    // linear light (0.7, 0.55, 0.4) times 1.9 at noon, a warm white, and toward the horizon
    // (0.64, 0.45, 0.3) to the power of 1.5 and the share short of noon, times 5, an orange, mixed by
    // the square root of the sun's height, the shader's noon factor.
    private static readonly Vector3 NoonLight = new Vector3(0.7f, 0.55f, 0.4f) * 1.9f, LowLight = new(0.64f, 0.45f, 0.3f);

    // How strong the sun is for each unit of the shader's light, so its noon is as bright as the
    // game's sun was before it took the shader's color, a white of 2.2 whose luminance was 0.915.
    private const float SunStrength = 2.2f * 0.915f / 1.085f;

    /// <summary>
    /// Each star's way on the sky as it stands at 6, when the sun rises, the half of its square's
    /// right side and of its up there, and its color in sRGB levels from 0 to 1.
    /// </summary>
    public static readonly (Vector3 Way, Vector3 Right, Vector3 Up, Vector3 Color)[] Stars = PlaceStars();

    private readonly LightHandle _light;
    private readonly byte[] _pixels = new byte[Width * Height * 4];
    private readonly Texture2D _sun, _moon;
    private float _litHour = float.NaN, _paintedHour = float.NaN;

    // A pack's sun, and its moon as one sheet of four phases by two or as a picture to each phase,
    // or none where no pack has them.
    private Texture2D? _packSun, _packMoonSheet;
    private Texture2D?[] _packMoons = new Texture2D?[SkyArt.Phases];

    // The names Minecraft gives the moon's phases' own pictures, in the order of the phases.
    private static readonly string[] PhaseNames =
        ["full_moon", "waning_gibbous", "third_quarter", "waning_crescent", "new_moon", "waxing_crescent", "first_quarter", "waxing_gibbous"];

    public Sky(float hour)
    {
        Hour = hour;
        _light = CreateDirectionalLight(-Vector3.UnitY, Color.White, 1, castsShadows: true);
        _sun = Load(SkyArt.SunPixels(), SkyArt.Texels, SkyArt.Texels);
        _moon = Load(SkyArt.MoonPixels(), SkyArt.Cell * 4, SkyArt.Cell * 2);
        Apply();
    }

    /// <summary>
    /// Draws the sun and the moon with the resource packs' own pictures of them where a pack has
    /// them, under <c>environment/celestial</c> as Minecraft reads them now or as the sun and the
    /// sheet of the moon's phases under <c>environment</c> as it read them before, and with the
    /// game's own where none has.
    /// </summary>
    public void Use(ResourcePacks packs)
    {
        UnloadPacks();
        _packSun = Loaded(packs.Texture("environment/celestial/sun") ?? packs.Texture("environment/sun"));
        for (int phase = 0; phase < SkyArt.Phases; phase++) _packMoons[phase] = Loaded(packs.Texture($"environment/celestial/moon/{PhaseNames[phase]}"));
        if (_packMoons.Any(m => m is null))
        {
            foreach (var moon in _packMoons) if (moon is { } texture) UnloadTexture(texture);
            _packMoons = new Texture2D?[SkyArt.Phases];
            _packMoonSheet = Loaded(packs.Texture("environment/moon_phases"));
        }
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
    public static Vector3 SunToward(float hour) => Turned(hour, SunWay);

    /// <summary>
    /// A way on the sky as it stands at 6, turned as the sky turns by an hour, about the axis from
    /// north to south, so the sun goes from the east at 6 up over the south to the west at 18.
    /// </summary>
    public static Vector3 Turned(float hour, Vector3 way) => Vector3.Transform(way, Turn(hour));

    // The sky's turn at an hour since 6, in degrees and as a matrix, about the axis from north to
    // south.
    private static float TurnDegrees(float hour) => (hour - 6) * 15;

    private static Matrix4x4 Turn(float hour) => Matrix4x4.CreateRotationZ(float.DegreesToRadians(TurnDegrees(hour)));

    /// <summary>
    /// The right side and the up of a quad fixed on the sky with its middle on a way, each of
    /// length one, the right side along the way the sky turns there and the up across it, so that
    /// a quad seen from the middle of the sky shows its front.
    /// </summary>
    /// <remarks>
    /// The sky turns about its axis, which the turn leaves where it is, so the sides of a way
    /// turned by an hour are the sides of the way turned by the same hour, and a quad keeps them as
    /// the sky turns it. At the axis itself, where the sky does not move, the right side is east.
    /// </remarks>
    public static (Vector3 Right, Vector3 Up) Across(Vector3 way)
    {
        var right = Vector3.Cross(way, Vector3.UnitZ);
        right = right.LengthSquared() > 1e-6f ? Vector3.Normalize(right) : Vector3.UnitX;
        return (right, Vector3.Cross(right, way));
    }

    /// <summary>
    /// Draws the stars, the moon and the sun on the sky about the eye, the stars fading in as the
    /// day fades and near the horizon, and the sun and the moon fading as they set, each taking the
    /// share of the haze dome left clear at its height where the dome is drawn. Called inside
    /// <c>BeginMode3D</c>.
    /// </summary>
    public void Draw(Vector3 eye, bool hazed)
    {
        var toSun = SunToward(Hour);
        var night = 1 - DayShare(toSun);
        var turn = Turn(Hour);

        // Every quad is given in the sky's space as it stands at 6, which turns it about the eye.
        rlPushMatrix();
        rlTranslatef(eye.X, eye.Y, eye.Z);
        rlRotatef(TurnDegrees(Hour), 0, 0, 1);
        if (night > 0.01f)
        {
            rlBegin(RlDrawMode.Quads);
            foreach (var (way, right, up, color) in Stars)
            {
                var turned = Vector3.Transform(way, turn);
                // Thinning over the lowest 20 degrees and gone beside the sun and the moon.
                var beside = MathF.Pow(MathF.Min(1, MathF.Abs(Vector3.Dot(turned, toSun)) * 1.002f), 100);
                var alpha = night * Math.Clamp(turned.Y * 3, 0, 1) * (1 - beside) * (hazed ? Clear(turned) : 1);
                if (alpha < 0.01f) continue;
                rlColor4ub((byte)(255 * Math.Min(1, color.X)), (byte)(255 * Math.Min(1, color.Y)), (byte)(255 * Math.Min(1, color.Z)), (byte)(255 * alpha));
                Corners(way * StarDistance, right, up, Vector4.Zero);
            }
            rlEnd();
        }

        // The moon and then the sun, each added to the sky and writing no depth, as Minecraft adds
        // its own, so the glow brightens the sky behind it and a body drawn over another adds to it.
        var phase = MoonPhase(Day, Hour);
        var moon = _packMoons[phase] is { } own ? (own, new Rectangle(0, 0, own.Width, own.Height))
            : _packMoonSheet is { } sheet ? (sheet, new Rectangle(phase % 4 * sheet.Width / 4f, phase / 4 * sheet.Height / 2f, sheet.Width / 4f, sheet.Height / 2f))
            : (_moon, new Rectangle(phase % 4 * SkyArt.Cell + 1, phase / 4 * SkyArt.Cell + 1, SkyArt.Texels, SkyArt.Texels));
        var sun = _packSun is { } packed ? packed : _sun;
        BeginBlendMode(BlendMode.Additive);
        rlDisableDepthMask();
        Body(moon.Item1, moon.Item2, -SunWay, Distance * MoonAcross / SkyArt.Size, Setting(-toSun), turn, hazed);
        Body(sun, new Rectangle(0, 0, sun.Width, sun.Height), SunWay, Distance * SunAcross / SkyArt.Size, Setting(toSun), turn, hazed);
        rlEnableDepthMask();
        EndBlendMode();
        rlPopMatrix();
    }

    public void Dispose()
    {
        UnloadPacks();
        UnloadTexture(_sun);
        UnloadTexture(_moon);
    }

    /// <summary>
    /// The corners of a grid of <see cref="SkyArt.Across"/> squares a side on the sky about a way,
    /// at the sun's and the moon's distance in the sky's space as it stands at 6, each square a pixel
    /// of their pictures so wide, a row at a time from the top left, so a quad of the grid is a pixel
    /// of the picture.
    /// </summary>
    public static Vector3[] Grid(Vector3 way, float pixel)
    {
        const int side = SkyArt.Across + 1;
        var (right, up) = Across(way);
        var corners = new Vector3[side * side];
        for (int row = 0; row < side; row++)
            for (int column = 0; column < side; column++)
                corners[row * side + column] = way * Distance + right * ((column - SkyArt.Across / 2f) * pixel) + up * ((SkyArt.Across / 2f - row) * pixel);
        return corners;
    }

    /// <summary>
    /// The share of what lies behind the haze dome that shows through it along a way about the eye,
    /// all of it above the dome's band, none from the horizon down, as <see cref="HazeDome.Shown"/>
    /// leaves it.
    /// </summary>
    public static float Clear(Vector3 way) => 1 - HazeDome.Shown(float.RadiansToDegrees(MathF.Asin(Math.Clamp(way.Y / way.Length(), -1, 1))));

    // A picture or part of one on the sky at the sun's and the moon's distance, its middle on a way
    // in the sky's space and each pixel of the game's own pictures so wide, as a grid of a quad to
    // each of those pixels, each corner at an alpha and at the share of the haze left clear at its
    // height on the sky turned by the hour, where the dome is drawn.
    private static void Body(Texture2D texture, Rectangle source, Vector3 way, float pixel, float alpha, Matrix4x4 turn, bool hazed)
    {
        if (alpha <= 0) return;
        const int side = SkyArt.Across + 1;
        var corners = Grid(way, pixel);
        var shown = new byte[corners.Length];
        for (int i = 0; i < corners.Length; i++)
            shown[i] = (byte)(255 * Math.Clamp(alpha * (hazed ? Clear(Vector3.Transform(corners[i], turn)) : 1), 0, 1));
        if (shown.All(s => s == 0)) return;
        float step = source.Width / SkyArt.Across / texture.Width, rise = source.Height / SkyArt.Across / texture.Height;
        float left = source.X / texture.Width, top = source.Y / texture.Height;
        rlSetTexture(texture.Id);
        rlBegin(RlDrawMode.Quads);
        for (int row = 0; row < SkyArt.Across; row++)
            for (int column = 0; column < SkyArt.Across; column++)
                // From the top left down and round against the clock as the middle of the sky sees
                // it, as Corners lays a quad.
                foreach (var (down, across) in (ReadOnlySpan<(int, int)>)[(0, 0), (1, 0), (1, 1), (0, 1)])
                {
                    var at = (row + down) * side + column + across;
                    rlColor4ub(255, 255, 255, shown[at]);
                    Vertex(corners[at], left + (column + across) * step, top + (row + down) * rise);
                }
        rlEnd();
        rlSetTexture(0);
    }

    private void UnloadPacks()
    {
        foreach (var texture in (Texture2D?[])[_packSun, _packMoonSheet, .. _packMoons])
            if (texture is { } loaded) UnloadTexture(loaded);
        (_packSun, _packMoonSheet) = (null, null);
        Array.Clear(_packMoons);
    }

    private static Texture2D? Loaded(Image? image)
    {
        if (image is not { } picture) return null;
        var texture = LoadTextureFromImage(picture);
        SetTextureFilter(texture, TextureFilter.Point);
        SetTextureWrap(texture, TextureWrap.Clamp);
        return texture;
    }

    // A quad's corners about its middle, half its right side and half its up from it, from the top
    // left down and round against the clock as the middle of the sky sees it, which is the face a
    // shape drawn in 3D shows, with the texture's part from its top left to its bottom right.
    private static void Corners(Vector3 middle, Vector3 right, Vector3 up, Vector4 uv)
    {
        Vertex(middle - right + up, uv.X, uv.Y);
        Vertex(middle - right - up, uv.X, uv.W);
        Vertex(middle + right - up, uv.Z, uv.W);
        Vertex(middle + right + up, uv.Z, uv.Y);
    }

    private static void Vertex(Vector3 at, float u, float v)
    {
        rlTexCoord2f(u, v);
        rlVertex3f(at.X, at.Y, at.Z);
    }

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

    // The stars, spread evenly over the sky from a fixed hash, so each run has the same ones, as the
    // modpack's shader, Aurora's Shaders after Complementary, draws its own: squares of StarDegrees,
    // each as bright as 40 times the square of how far the product of three noises passes 0.7, the
    // product drawn until it passes, so most are faint and a few bright, in a cool white, the light
    // taken to sRGB levels.
    private static (Vector3, Vector3, Vector3, Vector3)[] PlaceStars()
    {
        var half = StarDistance * MathF.Tan(float.DegreesToRadians(StarDegrees)) / 2;
        var stars = new (Vector3, Vector3, Vector3, Vector3)[StarCount];
        float Next(int i, int k) => PlaceHash.Of(i, k, 13) / (float)uint.MaxValue;
        for (int i = 0; i < StarCount; i++)
        {
            var z = Next(i, 0) * 2 - 1;
            var ring = MathF.Sqrt(1 - z * z);
            var turn = Next(i, 1) * MathF.Tau;
            var way = new Vector3(ring * MathF.Cos(turn), ring * MathF.Sin(turn), z);
            var product = 0f;
            for (int k = 2; product <= 0.7f; k += 3) product = Next(i, k) * Next(i, k + 1) * Next(i, k + 2);
            var light = 40 * (product - 0.7f) * (product - 0.7f) * new Vector3(0.38f, 0.4f, 0.5f);
            var (right, up) = Across(way);
            stars[i] = (way, right * half, up * half, new Vector3(MathF.Pow(light.X, 1 / 2.2f), MathF.Pow(light.Y, 1 / 2.2f), MathF.Pow(light.Z, 1 / 2.2f)));
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
            // The shader's light by the sun's height, its hue as the color and its largest part as
            // the strength, so the hue never clips.
            var light = SunLight(toSun.Y);
            var most = MathF.Max(light.X, MathF.Max(light.Y, light.Z));
            SetLightDirection(_light, -toSun);
            SetLightColor(_light, ColorOf(Encode(light / most) * 255), SunStrength * most * SmoothStep(-0.05f, 0.1f, toSun.Y));
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

    /// <summary>The sun's light in linear light at a height, the sine of its angle over the horizon, as the modpack's shader colors it.</summary>
    public static Vector3 SunLight(float height)
    {
        var noon = MathF.Sqrt(MathF.Max(height, 0));
        var power = 1.5f + (1 - noon);
        var low = new Vector3(MathF.Pow(LowLight.X, power), MathF.Pow(LowLight.Y, power), MathF.Pow(LowLight.Z, power)) * 5;
        return Vector3.Lerp(low, NoonLight, noon);
    }

    // Linear light to sRGB levels from 0 to 1.
    private static Vector3 Encode(Vector3 linear)
    {
        static float One(float c) => c <= 0.0031308f ? 12.92f * c : 1.055f * MathF.Pow(c, 1 / 2.4f) - 0.055f;
        return new Vector3(One(linear.X), One(linear.Y), One(linear.Z));
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
