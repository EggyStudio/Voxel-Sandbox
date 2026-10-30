using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The window's mode, its resolution, the frame limit and vsync, which the Video Settings screen
/// sets, each through the engine's window as the source's video settings set its own.
/// </summary>
/// <remarks>
/// The resolution is the window's size in a window and the monitor's mode in fullscreen, and a
/// borderless window covers the monitor at the desktop's own mode. Its first step, Current, is the
/// size the window had as the screen was first made, and the desktop's mode in fullscreen. The frame
/// limit's last step, 260 as the source's, leaves the frames unlimited, as the game starts.
/// </remarks>
public sealed class VideoSettings
{
    public const int Unlimited = 260;

    /// <summary>The window's modes by the index <see cref="WindowMode"/> gives them.</summary>
    public static readonly string[] WindowModes = ["Windowed", "Fullscreen", "Borderless"];

    private readonly int _width, _height;

    public VideoSettings()
    {
        // One mode to each size, at its highest refresh rate, largest first as the engine lists them.
        Modes = [.. GetMonitorModes(GetCurrentMonitor()).GroupBy(m => (m.Width, m.Height)).Select(sizes => sizes.MaxBy(m => m.RefreshRate))];
        (_width, _height) = (GetScreenWidth(), GetScreenHeight());
    }

    /// <summary>The monitor's modes, read as the settings are made.</summary>
    public MonitorMode[] Modes { get; }

    /// <summary>The resolution chosen, 0 for Current and from 1 the mode of <see cref="Modes"/> one before it.</summary>
    public int Resolution { get; set; }

    public int FrameLimit { get; private set; } = Unlimited;

    /// <summary>The window's mode as the engine says it, 0 a window, 1 fullscreen and 2 borderless.</summary>
    public static int WindowMode => IsWindowState(ConfigFlags.BorderlessWindowedMode) ? 2 : IsWindowFullscreen() ? 1 : 0;

    /// <summary>The resolution's name, a mode's size or Current.</summary>
    public string ResolutionName(int resolution) =>
        resolution >= 1 && resolution <= Modes.Length ? $"{Modes[resolution - 1].Width}x{Modes[resolution - 1].Height}" : "Current";

    /// <summary>Puts the window into a mode, leaving the mode it is in for a window first, fullscreen at the resolution chosen.</summary>
    public void SetWindowMode(int mode)
    {
        var now = WindowMode;
        if (mode == now) return;
        if (now == 1) ToggleFullscreen();
        else if (now == 2) ToggleBorderlessWindowed();
        if (mode == 1) SetWindowFullscreenMode(Chosen ?? default);
        else if (mode == 2) ToggleBorderlessWindowed();
        else ApplyResolution();
    }

    /// <summary>Sizes the window to the resolution chosen, or sets the monitor's mode to it in fullscreen.</summary>
    public void ApplyResolution()
    {
        switch (WindowMode)
        {
            case 0:
                var (width, height) = Chosen is { } mode ? (mode.Width, mode.Height) : (_width, _height);
                if (width != GetScreenWidth() || height != GetScreenHeight()) SetWindowSize(width, height);
                break;
            case 1:
                SetWindowFullscreenMode(Chosen ?? default);
                break;
        }
    }

    /// <summary>
    /// Puts the window into the resolution and the mode an options file keeps, a window of the size
    /// it opened at where the file keeps neither.
    /// </summary>
    /// <remarks>
    /// The start calls it before the window's first frame and before the world opens, so the window
    /// shows as it was left rather than changing once the game is up. The resolution is set before
    /// the mode, so a fullscreen window changes the monitor's mode once.
    /// </remarks>
    public void Restore(OptionsFile file)
    {
        var resolution = OptionScreens.ResolutionOf(this);
        if (file.Find("video", resolution) is { } chosen) resolution.Change(chosen);
        var mode = file.Find("video", OptionScreens.WindowModeOf(this)) is { } kept ? (int)kept : 0;
        if (mode != 0) SetWindowMode(mode);
        else ApplyResolution();
    }

    /// <summary>Holds the frames to a rate, or leaves them unlimited at <see cref="Unlimited"/>.</summary>
    public void SetFrameLimit(int fps)
    {
        FrameLimit = Math.Clamp(fps, 10, Unlimited);
        SetTargetFPS(FrameLimit >= Unlimited ? 0 : FrameLimit);
    }

    /// <summary>Whether the frames wait for the monitor's refresh, as the engine says it.</summary>
    public static bool Vsync => IsWindowState(ConfigFlags.VsyncHint);

    public static void SetVsync(bool on)
    {
        if (on) SetWindowState(ConfigFlags.VsyncHint);
        else ClearWindowState(ConfigFlags.VsyncHint);
    }

    private MonitorMode? Chosen => Resolution >= 1 && Resolution <= Modes.Length ? Modes[Resolution - 1] : null;
}
