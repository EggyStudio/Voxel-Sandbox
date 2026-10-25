using System.Numerics;
using Engine;
using ImGuiNET;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// An ImGui window of what the frame holds and costs, the toggles of debugging and the world's kind,
/// seed and mode, with the engine's own windows of global illumination, the frame's profile and the
/// effects over the frame beside it.
/// </summary>
/// <remarks>The settings a player changes are on the options' screens, which <see cref="OptionScreens"/> holds.</remarks>
public sealed class SettingsWindow
{
    private static readonly string[] Worlds = Generators.Kinds;
    private static readonly string[] Modes = Enum.GetNames<GameMode>();

    private int _worldType;
    private int _seed = 1;

    public bool Open { get; set; }

    public bool BounceWindow { get; set; }

    public bool ProfileWindow { get; set; }

    /// <summary>Whether the engine's window of the effects over the frame is drawn, the tonemap, exposure, grading, bloom and the rest.</summary>
    public bool PostProcessWindow { get; set; }

    public void Draw(VoxelGame game)
    {
        if (BounceWindow) DrawBounceWindow();
        if (ProfileWindow) DrawProfileWindow();
        if (PostProcessWindow)
        {
            // At the window's right edge the first time, clear of this window at its left.
            ImGui.SetNextWindowPos(new Vector2(GetScreenWidth() - 10, 10), ImGuiCond.FirstUseEver, new Vector2(1, 0));
            DrawPostProcessWindow();
        }
        if (!Open) return;

        ImGui.SetNextWindowPos(new Vector2(10, 10), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new Vector2(460, 0), ImGuiCond.FirstUseEver);
        var open = Open;
        if (ImGui.Begin("Voxel", ref open))
        {
            Stats(game);
            if (ImGui.CollapsingHeader("Debugging", ImGuiTreeNodeFlags.DefaultOpen)) Debugging(game);
            if (ImGui.CollapsingHeader("World", ImGuiTreeNodeFlags.DefaultOpen)) World(game);
        }
        ImGui.End();
        Open = open;
    }

    private static void Stats(VoxelGame game)
    {
        var player = game.Player;
        var at = player.Body.Position;
        ImGui.TextUnformatted($"{GetFPS()} fps, {GetFrameTime() * 1000:0.0} ms a frame");
        ImGui.TextUnformatted($"Feet at {at.X:0.0}, {at.Y:0.0}, {at.Z:0.0}, facing {player.Heading:0} degrees{(player.Flying ? ", flying" : "")}, in {game.World.BiomeAt((int)MathF.Floor(at.X), (int)MathF.Floor(at.Z))?.Name ?? "nowhere loaded"}");
        ImGui.TextUnformatted(game.TargetMob is { } mob ? $"Looking at a {mob.Kind.Name}, {mob.Doing}, its health {mob.Health} of {mob.Kind.Health}"
            : game.Target is { } hit ? $"Looking at {Blocks.Get(game.World.GetBlock(hit.X, hit.Y, hit.Z)).Name} at {hit.X}, {hit.Y}, {hit.Z}, its face's light {LightBeside(game, hit)}"
            : "Looking at nothing within reach");
        var renderer = game.Renderer;
        ImGui.TextUnformatted($"{game.World.ColumnCount} columns, {game.Streamer.Pending} generating, {game.World.Loaded.Count + game.Renderer.Meshing} sections to mesh");
        ImGui.TextUnformatted($"{renderer.Sections} sections drawn as a mesh each, {renderer.Lamps} lamps");
        ImGui.TextUnformatted($"{renderer.Draws} draws of {renderer.Triangles:N0} triangles, {game.Clouds.Faces} faces of clouds");
        ImGui.TextUnformatted($"Hour {game.Sky.Hour:0.00} of day {game.Sky.Day}{(game.Sky.Cycle ? ", going on" : ", held")}");
        if (game.Mode.Lives())
            ImGui.TextUnformatted($"Health {game.Vitals.Health}, food {game.Vitals.Food}, saturation {game.Vitals.Saturation:0.0}, exhaustion {game.Vitals.Exhaustion:0.00}");
        ImGui.TextUnformatted($"{game.Mobs.All.Count} mobs loaded, {game.MobRenderer.Drawn} drawn in {game.MobRenderer.Draws} draws, {game.Mobs.Kept} kept by columns not loaded");
    }

    private static string LightBeside(VoxelGame game, BlockHit hit)
    {
        var (x, y, z) = hit.Beside;
        var (sky, block) = game.World.GetLight(x, y, z);
        return $"sky {sky}, blocks {block}";
    }

    private void Debugging(VoxelGame game)
    {
        // Each changes every section's colors, which are written again a few sections a frame.
        var levels = game.Renderer.LightLevels;
        if (ImGui.Checkbox("Light levels", ref levels))
        {
            game.Renderer.LightLevels = levels;
            game.World.ReshadeAll();
        }
        ImGui.SameLine();
        var corners = game.Renderer.CornerShade;
        if (ImGui.Checkbox("Corners shaded", ref corners))
        {
            game.Renderer.CornerShade = corners;
            game.World.ReshadeAll();
        }
        var cull = game.Renderer.Cull;
        if (ImGui.Checkbox($"Leave unseen sections undrawn, {game.Renderer.Culled} now###cull", ref cull)) game.Renderer.Cull = cull;
        var still = game.Mobs.Still;
        if (ImGui.Checkbox("Mobs stand still", ref still)) game.Mobs.Still = still;

        var bounce = BounceWindow;
        if (ImGui.Checkbox("Global illumination window", ref bounce)) BounceWindow = bounce;
        ImGui.SameLine();
        var profile = ProfileWindow;
        if (ImGui.Checkbox("The frame profile", ref profile)) ProfileWindow = profile;
        var effects = PostProcessWindow;
        if (ImGui.Checkbox("The effects over the frame", ref effects)) PostProcessWindow = effects;
    }

    private void World(VoxelGame game)
    {
        var mode = (int)game.Mode;
        if (ImGui.Combo("Mode", ref mode, Modes, Modes.Length)) game.Mode = (GameMode)mode;
        ImGui.TextUnformatted(game.Save is { } save ? $"Saved in {save.Folder} every 30 seconds" : "Transient, not saved");
        if (game.Save is not null && ImGui.Button("Save now")) game.SaveWorld();
        ImGui.Combo("Kind", ref _worldType, Worlds, Worlds.Length);
        ImGui.InputInt("Seed", ref _seed);
        if (ImGui.Button("Open or begin")) game.NextWorld = (Worlds[_worldType], _seed);
    }
}
