using System.Numerics;

namespace VoxelSandbox;

/// <summary>
/// The colors the sky is painted in through the day and the light the sun and the moon cast, which
/// the Sky and Visuals screen sets, each starting at the modpack's shader's.
/// </summary>
/// <remarks>
/// The sky's colors are sRGB levels from 0 to 255. The lights and the night's tint are in linear
/// light, the screen showing each as the sRGB color of its own share of a fixed scale, so a light
/// stronger than 1 is set by its hue. A change counts up <see cref="Version"/>, which the sky reads
/// to paint and light itself again.
/// </remarks>
public sealed class SkyColors
{
    public static readonly Vector3 DefaultNightZenith = new(6, 9, 22), DefaultDayZenith = new(78, 134, 222);
    public static readonly Vector3 DefaultNightHorizon = new(18, 24, 44), DefaultDayHorizon = new(178, 208, 240), DefaultDuskHorizon = new(238, 150, 96);

    // The sunlight of the modpack's shader, Aurora's Shaders after Complementary, read as numbers, in
    // linear light (0.7, 0.55, 0.4) times 1.9 at noon, a warm white, and toward the horizon
    // (0.64, 0.45, 0.3) to the power of 1.5 and the share short of noon, times 5, an orange, mixed by
    // the square root of the sun's height, the shader's noon factor.
    public static readonly Vector3 DefaultNoonLight = new Vector3(0.7f, 0.55f, 0.4f) * 1.9f, DefaultLowLight = new(0.64f, 0.45f, 0.3f);

    // The moonlight in the hue of the modpack's shader's night light, (0.15, 0.14, 0.20) in linear
    // light, as strong as a moonlight of the luminance the game's had, a quarter of a white's 0.41.
    public static readonly Vector3 DefaultMoonlight = new Vector3(0.15f, 0.14f, 0.20f) / 0.20f;
    public const float DefaultMoonStrength = 0.25f * 0.409f / 0.732f;

    // How strong the sun is for each unit of the shader's light, so its noon is as bright as the
    // game's sun was before it took the shader's color, a white of 2.2 whose luminance was 0.915.
    public const float DefaultSunStrength = 2.2f * 0.915f / 1.085f;

    // The night's tint of the modpack's shader with the modpack's own settings, which multiplies the
    // sky's colors and so the haze's in linear light, a violet blue, by as much as the night is the
    // night.
    public static readonly Vector3 DefaultNightTint = new(1.10f, 0.90f, 1.30f);

    public Vector3 NightZenith = DefaultNightZenith, DayZenith = DefaultDayZenith;
    public Vector3 NightHorizon = DefaultNightHorizon, DayHorizon = DefaultDayHorizon, DuskHorizon = DefaultDuskHorizon;
    public Vector3 NoonLight = DefaultNoonLight, LowLight = DefaultLowLight, Moonlight = DefaultMoonlight;
    public Vector3 NightTint = DefaultNightTint;
    public float SunStrength = DefaultSunStrength, MoonStrength = DefaultMoonStrength;

    /// <summary>A count that rises with each change, so the sky paints itself again once it differs from the one it painted with.</summary>
    public int Version { get; private set; }

    /// <summary>Counts a change made to the colors, which the sky shows within its next frame.</summary>
    public void Changed() => Version++;

    /// <summary>Puts every color and strength back to the modpack's shader's.</summary>
    public void Reset()
    {
        (NightZenith, DayZenith) = (DefaultNightZenith, DefaultDayZenith);
        (NightHorizon, DayHorizon, DuskHorizon) = (DefaultNightHorizon, DefaultDayHorizon, DefaultDuskHorizon);
        (NoonLight, LowLight, Moonlight, NightTint) = (DefaultNoonLight, DefaultLowLight, DefaultMoonlight, DefaultNightTint);
        (SunStrength, MoonStrength) = (DefaultSunStrength, DefaultMoonStrength);
        Changed();
    }

    /// <summary>The sun's light in linear light at a height, the sine of its angle over the horizon, as the modpack's shader colors it.</summary>
    public Vector3 SunLight(float height)
    {
        var noon = MathF.Sqrt(MathF.Max(height, 0));
        var power = 1.5f + (1 - noon);
        var low = new Vector3(MathF.Pow(LowLight.X, power), MathF.Pow(LowLight.Y, power), MathF.Pow(LowLight.Z, power)) * 5;
        return Vector3.Lerp(low, NoonLight, noon);
    }

    /// <summary>A sky's color in sRGB levels from 0 to 255 tinted by the night's tint as far as the day's light is gone, <paramref name="day"/> from 0 at night to 1 by day.</summary>
    public Vector3 AtNight(Vector3 color, float day) => Tinted(color, Vector3.Lerp(NightTint, Vector3.One, day));

    /// <summary>An sRGB level from 0 to 1 in linear light.</summary>
    public static float Decode(float c) => c <= 0.04045f ? c / 12.92f : MathF.Pow((c + 0.055f) / 1.055f, 2.4f);

    /// <summary>Linear light to sRGB levels from 0 to 1.</summary>
    public static Vector3 Encode(Vector3 linear)
    {
        static float One(float c) => c <= 0.0031308f ? 12.92f * c : 1.055f * MathF.Pow(c, 1 / 2.4f) - 0.055f;
        return new Vector3(One(linear.X), One(linear.Y), One(linear.Z));
    }

    // A color in sRGB levels from 0 to 255 multiplied by a tint in linear light.
    private static Vector3 Tinted(Vector3 color, Vector3 tint)
    {
        var linear = new Vector3(Decode(color.X / 255), Decode(color.Y / 255), Decode(color.Z / 255)) * tint;
        return Encode(Vector3.Min(linear, Vector3.One)) * 255;
    }
}
