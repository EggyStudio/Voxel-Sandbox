using System.Globalization;
using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// A setting of an options screen, a slider between two ends or a button that steps through its
/// choices, as Minecraft's options are, which reads a value of the game's and sets it.
/// </summary>
public sealed class Option(string name, Func<float> get, Action<float> set)
{
    public string Name => name;

    /// <summary>The slider's ends.</summary>
    public float Low { get; init; }

    public float High { get; init; } = 1;

    /// <summary>The step a slider's value is rounded to, or 0 to keep it where it falls.</summary>
    public float Step { get; init; }

    /// <summary>The names of a button's choices, its value the index of the one chosen, or none for a slider.</summary>
    public string[]? Choices { get; init; }

    /// <summary>How a slider's value reads in its label, with two places at most where none is given.</summary>
    public Func<float, string>? Format { get; init; }

    /// <summary>What is done once a slider is let go or a button pressed, work too slow for every frame a slider moves.</summary>
    public Action? Settled { get; init; }

    /// <summary>Whether the options file keeps the setting across runs, as it does all but the world's own, which its save keeps.</summary>
    public bool Kept { get; init; } = true;

    public float Value => get();

    /// <summary>The value as the label shows it, a button's choice by its name.</summary>
    public string Shown => Choices is { } choices
        ? choices[Math.Clamp((int)MathF.Round(Value), 0, choices.Length - 1)]
        : Format?.Invoke(Value) ?? Value.ToString("0.##", CultureInfo.InvariantCulture);

    public string Label => $"{Name}: {Shown}";

    /// <summary>Sets the value, held within the ends and rounded to the step, or a button's to the nearest of its choices.</summary>
    public void Change(float value)
    {
        if (Choices is { } choices) value = Math.Clamp(MathF.Round(value), 0, choices.Length - 1);
        else
        {
            value = Math.Clamp(value, Low, High);
            if (Step > 0) value = Math.Clamp(Low + MathF.Round((value - Low) / Step) * Step, Low, High);
        }
        if (value != Value) set(value);
    }

    /// <summary>Steps a button to its next choice, from the last to the first, and settles it.</summary>
    public void Next()
    {
        if (Choices is not { } choices) return;
        Change(((int)MathF.Round(Value) + 1) % choices.Length);
        Settled?.Invoke();
    }

    /// <summary>A button that turns a setting on and off.</summary>
    public static Option Toggle(string name, Func<bool> get, Action<bool> set) =>
        new(name, () => get() ? 1 : 0, value => set(value >= 0.5f)) { Choices = ["Off", "On"] };

    /// <summary>A button that steps through an enumeration's values, each shown by its name.</summary>
    public static Option Choice<T>(string name, Func<T> get, Action<T> set) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        return new(name, () => Array.IndexOf(values, get()), value => set(values[(int)value])) { Choices = [.. values.Select(v => v.ToString())] };
    }

    /// <summary>A slider of a share in hundredths, shown as a percentage.</summary>
    public static string Percent(float value) => $"{value:0}%";
}

/// <summary>An options screen, its title, its settings and the buttons under them that open screens of their own.</summary>
public sealed record OptionScreen(string Title, IReadOnlyList<Option> Options, IReadOnlyList<(string Label, Menu Opens)> Links);

/// <summary>
/// A color of the sky's, its sRGB levels set by three sliders from 0 to 255. A light or a tint in
/// linear light is shown as the sRGB color of its share of <paramref name="Scale"/>, a sky's color
/// as it is where the scale is 0.
/// </summary>
public sealed record SkyColor(string Name, Func<Vector3> Get, Action<Vector3> Set, float Scale)
{
    /// <summary>The color as the swatch shows it, in sRGB levels from 0 to 255.</summary>
    public Vector3 Levels => Scale > 0 ? SkyColors.Encode(Vector3.Clamp(Get() / Scale, Vector3.Zero, Vector3.One)) * 255 : Get();

    /// <summary>The sliders of its red, green and blue, each counting a change to the sky's colors.</summary>
    public IReadOnlyList<Option> Channels(SkyColors colors) =>
    [
        Channel("Red", 0, colors),
        Channel("Green", 1, colors),
        Channel("Blue", 2, colors),
    ];

    private Option Channel(string name, int channel, SkyColors colors) => new(name, () => Levels[channel], level =>
    {
        var value = Get();
        value[channel] = Scale > 0 ? SkyColors.Decode(level / 255) * Scale : level;
        Set(value);
        colors.Changed();
    })
    { High = 255, Step = 1, Format = level => $"{level:0}" };
}

/// <summary>
/// The screens the options open, Video, Render, Post Processing and Sky and Visuals, each holding
/// the settings of its area, and the sky's colors, a screen to each color.
/// </summary>
/// <remarks>
/// Each setting reads its value from where the game keeps it, the engine's own where the engine
/// says it, so a screen shows what a command or the engine's windows set too.
/// </remarks>
public sealed class OptionScreens
{
    // The keys the zoom may take, none of them the game's for anything else.
    private static readonly Key[] ZoomKeys = [Key.C, Key.V, Key.Z, Key.X, Key.R, Key.LeftAlt];

    // The names of the moon's phases in the order the sky counts them.
    private static readonly string[] Phases =
        ["Full", "Waning Gibbous", "Last Quarter", "Waning Crescent", "New", "Waxing Crescent", "First Quarter", "Waxing Gibbous"];

    public OptionScreens(VoxelGame game)
    {
        var light = game.Light;
        var renderer = game.Renderer;
        var sky = game.Sky;
        var player = game.Player;
        General = new("Options",
        [
            new("FOV", () => player.FieldOfView, v => player.FieldOfView = v) { Low = 30, High = 110, Step = 1, Format = v => $"{v:0}" },
            // The sensitivity in hundredths of the game's own, 100 the 0.0025 radians a pixel it starts at.
            new("Sensitivity", () => player.Sensitivity / 0.0025f * 100, v => player.Sensitivity = v / 100 * 0.0025f) { Low = 10, High = 200, Step = 1, Format = Option.Percent },
            new("GUI Scale", () => Gui.Chosen, v => Gui.Chosen = (int)v) { Choices = ["Auto", "1", "2", "3", "4"] },
            Option.Toggle("Fading HUD", () => game.Fade.Enabled, on => game.Fade.Enabled = on),
            new("Zoom Key", () => Array.IndexOf(ZoomKeys, player.Zoom.Key), v => player.Zoom.Key = ZoomKeys[(int)v]) { Choices = [.. ZoomKeys.Select(k => k.ToString())] },
        ], []);

        var video = VideoSettings = new VideoSettings();
        Video = new("Video Settings",
        [
            new("Window Mode", () => VideoSettings.WindowMode, v => video.SetWindowMode((int)v)) { Choices = VideoSettings.WindowModes },
            // The window or the monitor changes once the slider is let go, rather than through every
            // size it passes.
            new("Resolution", () => video.Resolution, v => video.Resolution = (int)v)
                { High = Math.Max(1, video.Modes.Length), Step = 1, Format = v => video.ResolutionName((int)v), Settled = video.ApplyResolution },
            new("Frame Limit", () => video.FrameLimit, v => video.SetFrameLimit((int)v))
                { Low = 10, High = VideoSettings.Unlimited, Step = 10, Format = v => v >= VideoSettings.Unlimited ? "Unlimited" : $"{v:0} fps" },
            Option.Toggle("VSync", () => VideoSettings.Vsync, VideoSettings.SetVsync),
        ], []);

        Render = new("Render Settings",
        [
            new("Render Distance", () => game.RenderDistance, v => game.RenderDistance = (int)v) { Low = 2, High = 16, Step = 1, Format = v => $"{v:0} chunks" },
            Option.Choice("Global Illumination", () => light.Quality, quality =>
            {
                light.Quality = quality;
                SetGlobalIllumination(quality);
            }),
            Option.Toggle("Screen Probes", () => light.ScreenProbes, on =>
            {
                light.ScreenProbes = on;
                SetGlobalIlluminationScreenProbes(on);
            }),
            new("Shadow Distance", () => light.ShadowDistance, v =>
            {
                light.ShadowDistance = v;
                SetShadowDistance(v);
            }) { Low = 16, High = 192, Step = 8, Format = v => $"{v:0} blocks" },
            // The sections are shaded again once the slider is let go, rather than each frame it moves.
            new("Corner Shade", () => renderer.CornerStrength * 100, v => renderer.CornerStrength = v / 100)
                { High = 100, Step = 5, Format = Option.Percent, Settled = game.World.ReshadeAll },
            new("Emission", () => renderer.GlowScale, v => renderer.GlowScale = v) { High = 4, Step = 0.1f, Format = v => $"{v:0.0}" },
            Option.Toggle("Colored Lamps", () => renderer.ColoredLamps, on => renderer.ColoredLamps = on),
            new("Rim Light", () => renderer.Rim * 100, v => renderer.Rim = v / 100) { High = 100, Step = 5, Format = Option.Percent },
            Option.Toggle("Unshaded", () => renderer.Unshaded, on => renderer.Unshaded = on),
            new("Field Cascades", () => light.Cascades, v =>
            {
                light.Cascades = (int)v;
                light.ApplyField();
            }) { Low = 1, High = 8, Step = 1 },
            new("Field Cell", () => light.CellSize, v =>
            {
                light.CellSize = v;
                light.ApplyField();
            }) { Low = 0.125f, High = 1, Step = 0.125f, Format = v => $"{v:0.###} blocks" },
            new("Cascades a Frame", () => light.Budget, v =>
            {
                light.Budget = (int)v;
                light.ApplyField();
            }) { Low = 1, High = 8, Step = 1 },
        ], []);

        PostProcessing = new("Post Processing",
        [
            new("Bloom", () => GetBloom().Intensity, v =>
            {
                light.Bloom = v;
                SetBloom(v, light.BloomThreshold);
            }) { High = 2, Step = 0.05f },
            new("Bloom Threshold", () => GetBloom().Threshold, v =>
            {
                light.BloomThreshold = v;
                SetBloom(light.Bloom, v);
            }) { Low = 0.5f, High = 8, Step = 0.25f, Format = v => $"{v:0.00}" },
            new("Ambient Occlusion", () => light.Occlusion, v =>
            {
                light.Occlusion = v;
                SetAmbientOcclusion(v);
            }) { High = 1, Step = 0.05f },
            Option.Toggle("Auto Exposure", () => GetAutoExposure().Enabled, on =>
            {
                light.AutoExposure = on;
                light.ApplyExposure();
            }),
            new("Exposure", () => light.Exposure, v =>
            {
                light.Exposure = v;
                light.ApplyExposure();
            }) { Low = 0.1f, High = 4, Step = 0.05f, Format = v => GetAutoExposure().Enabled ? "Auto" : $"{v:0.00}" },
            new("Contrast", () => GetColorGrading().Contrast, v =>
            {
                light.Contrast = v;
                SetColorGrading(v, light.Saturation, Color.White);
            }) { Low = 0.5f, High = 2, Step = 0.02f },
            new("Saturation", () => GetColorGrading().Saturation, v =>
            {
                light.Saturation = v;
                SetColorGrading(light.Contrast, v, Color.White);
            }) { High = 2.5f, Step = 0.05f },
            Option.Choice("Tonemap", GetTonemap, curve =>
            {
                if (curve == Tonemap.Curve) light.ApplyCurve();
                else SetTonemap(curve);
            }),
            // The curve's four numbers, each of which chooses the curve as it is set.
            Curve("Curve Exposure", () => light.CurveExposure, v => light.CurveExposure = v, 0.1f, 3, light),
            Curve("Curve White", () => light.CurveWhite, v => light.CurveWhite = v, 0.5f, 3, light),
            Curve("Curve Lower", () => light.CurveLower, v => light.CurveLower = v, 0.5f, 2, light),
            Curve("Curve Upper", () => light.CurveUpper, v => light.CurveUpper = v, 0.5f, 2, light),
            new("Vignette", () => GetVignette().Intensity, v => SetVignette(v, GetVignette().Radius)) { High = 1, Step = 0.05f },
            Option.Toggle("FXAA", IsFxaaEnabled, SetFxaa),
            new("Motion Blur", () => GetMotionBlur().Amount, v => SetMotionBlur(v)) { High = 1, Step = 0.05f },
        ], []);

        var colors = sky.Colors;
        SkyAndVisuals = new("Sky and Visuals",
        [
            new("Time", () => sky.Hour, v => sky.Hour = v % 24) { High = 24, Step = 0.25f, Format = v => $"{(int)v:00}:{(int)(v % 1 * 60):00}", Kept = false },
            new("Daylight Cycle", () => sky.Cycle ? 1 : 0, v => sky.Cycle = v >= 0.5f) { Choices = ["Off", "On"], Kept = false },
            new("Day Length", () => sky.DayMinutes, v => sky.DayMinutes = v) { Low = 1, High = 60, Step = 1, Format = v => $"{v:0} min" },
            // The phase is set by moving the count of days on to the next day of that phase, so the
            // days counted never go back.
            new("Moon", () => Sky.MoonPhase(sky.Day, 18), v => sky.Day += ((int)v - Sky.MoonPhase(sky.Day, 18) + Phases.Length) % Phases.Length)
                { Choices = Phases, Kept = false },
            new("Sun Strength", () => colors.SunStrength / SkyColors.DefaultSunStrength * 100, v =>
            {
                colors.SunStrength = SkyColors.DefaultSunStrength * v / 100;
                colors.Changed();
            }) { High = 200, Step = 5, Format = Option.Percent },
            new("Moon Strength", () => colors.MoonStrength / SkyColors.DefaultMoonStrength * 100, v =>
            {
                colors.MoonStrength = SkyColors.DefaultMoonStrength * v / 100;
                colors.Changed();
            }) { High = 400, Step = 10, Format = Option.Percent },
            Option.Toggle("Weather", () => game.Weather.Enabled, on => game.Weather.Enabled = on),
            Option.Toggle("Clouds", () => game.Clouds.Shown, on => game.Clouds.Shown = on),
            Option.Toggle("Fog", () => game.Fog, on => game.Fog = on),
            Option.Toggle("Haze by Height", () => game.HazeByHeight, on => game.HazeByHeight = on),
            Option.Toggle("Morning Haze", () => game.MorningHaze, on => game.MorningHaze = on),
        ], [("Sky Colors...", Menu.SkyColors), ("Particles...", Menu.Particles)]);

        var particles = game.Particles;
        Particles = new("Particles",
        [
            Density("Rain Splashes", () => particles.SplashDensity, v => particles.SplashDensity = v),
            Density("Falling Leaves", () => particles.LeafDensity, v => particles.LeafDensity = v),
            Density("Cave Dust", () => particles.DustDensity, v => particles.DustDensity = v),
            Density("Wakes", () => particles.WakeDensity, v => particles.WakeDensity = v),
        ], []);

        var sounds = game.Sounds;
        Sounds = new("Music and Sounds",
        [
            Loudness("Master Volume", () => sounds.Master, v => sounds.Master = v),
            Loudness("Footsteps", () => sounds.Steps, v => sounds.Steps = v),
            Loudness("Ambience", () => sounds.Ambience, v => sounds.Ambience = v),
        ], []);

        Colors = SkyColorsOf(colors);
        SkyColors = colors;
    }

    /// <summary>The options' own settings, over the buttons to the other screens.</summary>
    public OptionScreen General { get; }

    public OptionScreen Video { get; }

    public VideoSettings VideoSettings { get; }

    public OptionScreen Render { get; }

    public OptionScreen PostProcessing { get; }

    public OptionScreen SkyAndVisuals { get; }

    /// <summary>How thick each kind of the world's small moving things is, from none to twice its own.</summary>
    public OptionScreen Particles { get; }

    /// <summary>How loud the sounds play, all of them, the steps and the places' sounds.</summary>
    public OptionScreen Sounds { get; }

    /// <summary>The sky's colors, each set on a screen of its own.</summary>
    public IReadOnlyList<SkyColor> Colors { get; }

    public SkyColors SkyColors { get; }

    /// <summary>The screens of settings by the name a command and the options file give each, general, video, render, post, sky, particles, sounds and colors, the last every channel of every color.</summary>
    public IEnumerable<(string Name, IReadOnlyList<Option> Options)> All()
    {
        yield return ("general", General.Options);
        yield return ("video", Video.Options);
        yield return ("render", Render.Options);
        yield return ("post", PostProcessing.Options);
        yield return ("sky", SkyAndVisuals.Options);
        yield return ("particles", Particles.Options);
        yield return ("sounds", Sounds.Options);
        yield return ("colors", ChannelsOf(Colors, SkyColors));
    }

    /// <summary>Every channel of every color, each named by its color and its channel.</summary>
    // A loudness from none to full, shown as a share.
    private static Option Loudness(string name, Func<float> get, Action<float> set) =>
        new(name, () => get() * 100, v => set(v / 100)) { High = 100, Step = 5, Format = Option.Percent };

    // A density from none to twice its own, shown as a share.
    private static Option Density(string name, Func<float> get, Action<float> set) =>
        new(name, () => get() * 100, v => set(v / 100)) { High = 200, Step = 10, Format = Option.Percent };

    public static IReadOnlyList<Option> ChannelsOf(IReadOnlyList<SkyColor> colors, SkyColors sky) =>
    [
        .. colors.SelectMany(color => color.Channels(sky).Select(channel =>
            new Option($"{color.Name} {channel.Name}", () => channel.Value, channel.Change) { High = 255, Step = 1, Format = channel.Format })),
    ];

    /// <summary>
    /// The sky's colors as its screens set them, the sun's at noon and low, the moon's, the night's
    /// tint, and the sky's at the top and the horizon by day, at sunset and by night.
    /// </summary>
    /// <remarks>
    /// The lights and the tint are each shown as a share of the largest part of the color they
    /// start at, so each starts with one channel at 255 and can be set brighter in none, the
    /// strength of the sun's and the moon's set on the screen above.
    /// </remarks>
    public static IReadOnlyList<SkyColor> SkyColorsOf(SkyColors colors) =>
    [
        new("Noon Sunlight", () => colors.NoonLight, v => colors.NoonLight = v, Largest(SkyColors.DefaultNoonLight)),
        new("Low Sunlight", () => colors.LowLight, v => colors.LowLight = v, Largest(SkyColors.DefaultLowLight)),
        new("Moonlight", () => colors.Moonlight, v => colors.Moonlight = v, Largest(SkyColors.DefaultMoonlight)),
        new("Night Tint", () => colors.NightTint, v => colors.NightTint = v, Largest(SkyColors.DefaultNightTint)),
        new("Day Sky", () => colors.DayZenith, v => colors.DayZenith = v, 0),
        new("Day Horizon", () => colors.DayHorizon, v => colors.DayHorizon = v, 0),
        new("Sunset Horizon", () => colors.DuskHorizon, v => colors.DuskHorizon = v, 0),
        new("Night Sky", () => colors.NightZenith, v => colors.NightZenith = v, 0),
        new("Night Horizon", () => colors.NightHorizon, v => colors.NightHorizon = v, 0),
    ];

    private static float Largest(Vector3 v) => MathF.Max(v.X, MathF.Max(v.Y, v.Z));

    private static Option Curve(string name, Func<float> get, Action<float> set, float low, float high, LightSettings light) => new(name, get, v =>
    {
        set(v);
        light.ApplyCurve();
    }) { Low = low, High = high, Step = 0.05f, Format = v => $"{v:0.00}" };
}
