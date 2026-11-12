using Engine;
using Xunit;
using static Engine.E3D;

namespace VoxelSandbox.Tests;

/// <summary>
/// A whole game in an app with no window, its flat world of a seed loaded, its frames stepped by
/// the engine's stages as the game's own loop steps them, each a thirtieth of a second unless the
/// test gives another, so a test runs the game's behaviors as the game runs them.
/// </summary>
/// <remarks>
/// <para>
/// The flat API holds one app for the process, which <c>InitWindow</c> builds with no window while
/// <c>E3D_HEADLESS</c> is set, and the engine's default plugins hold Dear ImGui's one context, so
/// the tests that build a game run one at a time in <see cref="HeadlessGames"/>, each closing its
/// window before the next opens one.
/// </para>
/// <para>
/// The game saves nothing unless given a folder of its own to save in, loads no pack, and draws two
/// columns about the player, who stands at the flat world's middle unless given a place of the land
/// to be put at, as <c>voxel.tp</c> puts the player, the origin moved near it. The world's
/// columns are made on workers by the clock, not by the frames, which pass in a fraction of that
/// with nothing held to a rate, so the game is stepped until it stands and every column in reach is
/// in, yielding a moment between frames while it loads, or until 30 seconds have passed, which a
/// slow machine's flat world takes a second or two of.
/// </para>
/// </remarks>
public sealed class HeadlessGame : IDisposable
{
    /// <summary>The seconds each frame steps the game by unless a test gives another.</summary>
    public const float FrameSeconds = 1 / 30f;

    public HeadlessGame(int seed = 1, float frameSeconds = FrameSeconds, LandPlace? at = null, string? save = null)
    {
        Environment.SetEnvironmentVariable("E3D_HEADLESS", "1");
        Environment.SetEnvironmentVariable("E3D_FRAME_TIME", frameSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
        InitWindow(1280, 720, "Voxel Sandbox");
        // The frames are counted, not waited on, so none is held back to a rate.
        SetTargetFPS(0);
        // Its sounds go to a device that plays none, as a run with no window shown does.
        InitAudioDevice();
        try
        {
            // A save's folder given whole stands in for its name, which the saves' folder is joined to.
            Game = new VoxelGame("flat", seed, save, transient: save is null, []) { RenderDistance = 2 };
            Stand();
            if (at is { } place)
            {
                Game.Recenter(place);
                Game.Player.Teleport(LandOrigin.PlaceOf(place));
                Stand();
            }
        }
        catch
        {
            CloseAudioDevice();
            CloseWindow();
            throw;
        }
    }

    public VoxelGame Game { get; }

    // Steps the game until the world stands and every column in reach is in, a frame at least, so
    // the streamer has seen where the player stands before nothing is pending.
    private void Stand()
    {
        var (clock, frames) = (System.Diagnostics.Stopwatch.StartNew(), 0);
        do
        {
            Game.Draw();
            frames++;
            if (Game.Loading || Game.Streamer.Pending > 0) Thread.Sleep(2);
        }
        while ((Game.Loading || Game.Streamer.Pending > 0) && clock.Elapsed.TotalSeconds < 30);
        Assert.False(Game.Loading, $"the flat world did not stand within 30 seconds, {frames} frames");
        Assert.Equal(0, Game.Streamer.Pending);
    }

    /// <summary>The ECS world the game's entities are in.</summary>
    public EcsWorld Ecs => GetApp().World.Resource<EcsWorld>();

    /// <summary>Steps the game by so many frames, each through the engine's stages.</summary>
    public void Step(int frames)
    {
        for (int frame = 0; frame < frames; frame++) Game.Draw();
    }

    public void Dispose()
    {
        Game.Dispose();
        CloseAudioDevice();
        CloseWindow();
    }
}

/// <summary>The tests that build a <see cref="HeadlessGame"/>, which run one at a time.</summary>
[CollectionDefinition(nameof(HeadlessGames), DisableParallelization = true)]
public sealed class HeadlessGames;
