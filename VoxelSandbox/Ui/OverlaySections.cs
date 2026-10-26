using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The game's sections of the engine's debug overlay, which F3 shows, as Minecraft's debug screen
/// shows what its world holds, under the engine's own lines of the system, the camera, the frame and
/// the memory.
/// </summary>
public static class OverlaySections
{
    /// <summary>The sections' names, in the order the overlay shows them.</summary>
    public static readonly string[] Names = ["Player", "Generation", "Light", "Sky", "Sections", "Mobs"];

    /// <summary>Adds the game's sections to the overlay, each read from the game as the overlay draws it.</summary>
    public static void Add(VoxelGame game)
    {
        AddDebugOverlaySection("Player", () => Player(game));
        AddDebugOverlaySection("Generation", () => Generation(game));
        AddDebugOverlaySection("Light", () => Light(game));
        AddDebugOverlaySection("Sky", () => Sky(game));
        AddDebugOverlaySection("Sections", () => Sections(game));
        AddDebugOverlaySection("Mobs", () => Mobs(game));
    }

    /// <summary>The player's place, heading and aim, and its health and food where it lives by them.</summary>
    public static IEnumerable<string> Player(VoxelGame game)
    {
        var player = game.Player;
        var at = player.Body.Position;
        yield return $"Feet at {at.X:0.0}, {at.Y:0.0}, {at.Z:0.0}, facing {player.Heading:0} degrees{(player.Flying ? ", flying" : "")}, in {game.Mode.ToString().ToLowerInvariant()}";
        yield return game.TargetMob is { } mob ? $"Looking at a {mob.Kind.Name}, {mob.Doing}, its health {mob.Health} of {mob.Kind.Health}"
            : game.Target is { } hit ? $"Looking at {Blocks.Get(game.World.GetBlock(hit.X, hit.Y, hit.Z)).Name} at {hit.X}, {hit.Y}, {hit.Z}"
            : "Looking at nothing within reach";
        if (game.Mode.Lives())
            yield return $"Health {game.Vitals.Health}, food {game.Vitals.Food}, saturation {game.Vitals.Saturation:0.0}, exhaustion {game.Vitals.Exhaustion:0.00}";
    }

    /// <summary>The world's kind and seed, the biome and climate at the feet and the caves' readings on the GPU.</summary>
    public static IEnumerable<string> Generation(VoxelGame game)
    {
        var generator = game.World.Generator;
        yield return $"{char.ToUpperInvariant(generator.Name[0])}{generator.Name[1..]}, seed {generator.Seed}";
        var feet = game.Player.Body.Position;
        int x = (int)MathF.Floor(feet.X), z = (int)MathF.Floor(feet.Z);
        if (generator is Overworld land)
        {
            var (height, temperature, humidity, biome) = land.At(x, z);
            yield return $"In {biome.Name}, temperature {temperature:0.00}, humidity {humidity:0.00}, the ground at {height}";
        }
        else yield return $"In {game.World.BiomeAt(x, z)?.Name ?? "nowhere loaded"}";
        var fields = game.CaveFields;
        yield return fields.Running ? $"The caves' noises read on the GPU, {fields.Read} columns read, {fields.Backlog} waiting"
            : fields.Enabled ? "The caves' noises read on the processor, the GPU unable" : "The caves' noises read on the processor";
    }

    /// <summary>The hour and the day.</summary>
    public static IEnumerable<string> Sky(VoxelGame game)
    {
        yield return $"Hour {game.Sky.Hour:0.00} of day {game.Sky.Day}{(game.Sky.Cycle ? ", going on" : ", held")}";
    }

    /// <summary>The sky's and the blocks' light at the eye and on the face looked at.</summary>
    public static IEnumerable<string> Light(VoxelGame game)
    {
        var eye = game.Player.Eye;
        var (sky, block) = game.World.GetLight((int)MathF.Floor(eye.X), (int)MathF.Floor(eye.Y), (int)MathF.Floor(eye.Z));
        yield return $"At the eye sky {sky}, blocks {block}";
        if (game.Target is { } hit)
        {
            var (fx, fy, fz) = hit.Beside;
            var (faceSky, faceBlock) = game.World.GetLight(fx, fy, fz);
            yield return $"On the face looked at sky {faceSky}, blocks {faceBlock}";
        }
    }

    /// <summary>The columns and sections loaded, waiting and drawn, and what the frame draws of them.</summary>
    public static IEnumerable<string> Sections(VoxelGame game)
    {
        var renderer = game.Renderer;
        yield return $"{game.World.ColumnCount} columns, {game.Streamer.Pending} generating, {game.World.Loaded.Count + renderer.Meshing} sections to mesh";
        yield return $"{renderer.Sections} sections meshed, {renderer.Culled} left undrawn by the flood, {renderer.Deep.Sections} deep drawn";
        yield return $"{renderer.Draws} draws of {renderer.Triangles:N0} triangles, {renderer.Lamps} lamps, {game.Clouds.Faces} faces of clouds";
    }

    /// <summary>The mobs loaded, drawn and kept by columns not loaded.</summary>
    public static IEnumerable<string> Mobs(VoxelGame game)
    {
        yield return $"{game.Mobs.All.Count} loaded, {game.MobRenderer.Drawn} drawn in {game.MobRenderer.Draws} draws, {game.Mobs.Kept} kept by columns not loaded";
    }
}
