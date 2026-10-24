using Engine;

namespace VoxelSandbox;

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
    public static void Play(BehaviorContext ctx)
    {
        if (ctx.World.TryGetResource<VoxelGame>(out var game)) game.Play();
    }
}

/// <summary>
/// The first person controller of the player's entity, which turns and moves the player from the
/// frame's input as its context gives it, and swings the hand by how far the player walked.
/// </summary>
[Behavior]
public partial struct PlayerControl
{
    [OnPreUpdate]
    public static void Steer(BehaviorContext ctx)
    {
        if (!ctx.World.TryGetResource<GameStep>(out var step) || !step.Playing) return;
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
