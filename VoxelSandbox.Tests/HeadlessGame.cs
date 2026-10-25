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
/// The game saves nothing and loads no pack, and draws two columns about the player, which the
/// flat world loads within a second or two.
/// </para>
/// </remarks>
public sealed class HeadlessGame : IDisposable
{
    /// <summary>The seconds each frame steps the game by unless a test gives another.</summary>
    public const float FrameSeconds = 1 / 30f;

    public HeadlessGame(int seed = 1, float frameSeconds = FrameSeconds)
    {
        Environment.SetEnvironmentVariable("E3D_HEADLESS", "1");
        Environment.SetEnvironmentVariable("E3D_FRAME_TIME", frameSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
        InitWindow(1280, 720, "Voxel Sandbox");
        // The frames are counted, not waited on, so none is held back to a rate.
        SetTargetFPS(0);
        try
        {
            Game = new VoxelGame("flat", seed, null, transient: true, []) { RenderDistance = 2 };
            for (int frame = 0; frame < 600 && Game.Loading; frame++) Game.Draw();
            Assert.False(Game.Loading, "the flat world did not load within 600 frames");
        }
        catch
        {
            CloseWindow();
            throw;
        }
    }

    public VoxelGame Game { get; }

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
        CloseWindow();
    }
}

/// <summary>The tests that build a <see cref="HeadlessGame"/>, which run one at a time.</summary>
[CollectionDefinition(nameof(HeadlessGames), DisableParallelization = true)]
public sealed class HeadlessGames;
