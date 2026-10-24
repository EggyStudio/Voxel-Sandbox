using Engine;

namespace VoxelSandbox;

/// <summary>The world's state, loading behind its screen or standing to be played, an engine state the game's frame is gated on.</summary>
public enum WorldState
{
    Loading,
    Playing,
}

/// <summary>Whether the player lives and dies, as in survival and adventure, a state the engine computes from the game mode, present only while the player does.</summary>
public enum Living
{
    Yes,
}

/// <summary>
/// The game's frame in the engine's stages, begun in the first after the frame's commands and played
/// in the update once the player has moved, so what the game does each frame runs in the order the
/// stages give it.
/// </summary>
/// <remarks>
/// The program's loop draws alone, and the engine runs the stages from its BeginDrawing, the first
/// through the update before the game sets its camera, so the player moved in the stage before the
/// update is drawn where it stands in the same frame.
/// </remarks>
[Behavior]
public partial struct GameFrame
{
    [OnFirst]
    public static void Begin(BehaviorContext ctx)
    {
        if (ctx.World.TryGetResource<VoxelGame>(out var game)) game.Begin();
    }

    [OnUpdate]
    [InState(WorldState.Playing)]
    public static void Play(BehaviorContext ctx)
    {
        if (ctx.World.TryGetResource<VoxelGame>(out var game)) game.Play();
    }

    /// <summary>The player's health, food and breath taken while the player lives in a world that stands, and the HUD faded by them.</summary>
    [OnUpdate]
    [InState(Living.Yes)]
    [RunIf(nameof(Playing))]
    public static void Live(BehaviorContext ctx)
    {
        if (ctx.World.TryGetResource<VoxelGame>(out var game)) game.TakeVitals();
    }

    /// <summary>The HUD shown whole once the player no longer lives, as in creative.</summary>
    [OnExit(Living.Yes)]
    public static void ShowHud(BehaviorContext ctx)
    {
        if (ctx.World.TryGetResource<VoxelGame>(out var game)) game.Fade.Reset();
    }

    /// <summary>The cursor freed as a menu opens over the game.</summary>
    [OnExit(Menu.None)]
    public static void FreeCursor(BehaviorContext ctx)
    {
        if (ctx.World.TryGetResource<VoxelGame>(out var game)) game.Capture(false);
    }

    /// <summary>The cursor taken back as the last menu shuts, unless a window of the game's or the settings are open.</summary>
    [OnEnter(Menu.None)]
    public static void TakeCursor(BehaviorContext ctx)
    {
        if (ctx.World.TryGetResource<VoxelGame>(out var game)) game.Capture(!game.Settings.Open && !game.InventoryOpen);
    }

    /// <summary>Whether the world stands to be played.</summary>
    public static bool Playing(World world) => world.TryGetResource<State<WorldState>>(out var state) && state.Current == WorldState.Playing;
}

/// <summary>
/// The first person controller of the player's entity, which turns and moves the player from the
/// frame's input as its context gives it, and swings the hand by how far the player walked.
/// </summary>
[Behavior]
public partial struct PlayerControl
{
    [OnPreUpdate]
    [InState(WorldState.Playing)]
    public static void Steer(BehaviorContext ctx)
    {
        if (!ctx.World.TryGetResource<GameStep>(out var step)) return;
        foreach (var (_, player, hand) in ctx.Ecs.Query<FirstPersonController, HandRenderer>())
        {
            var before = player.Body.Position;
            player.Update(step.World, step.Seconds, ctx.Input, step.Look, step.Move);
            var walked = new System.Numerics.Vector2(player.Body.Position.X - before.X, player.Body.Position.Z - before.Z).Length();
            hand.Update(step.Seconds, player.Flying ? 0 : walked, player.Body.OnGround);
            // The mobs watch and hunt toward the eye where it stands now.
            step.Eye = player.Eye;
        }
    }
}
