using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The files the game reads from beside the program, its shaders under <c>resources/shaders</c> and
/// the default packs' <c>resources/packs.json</c>, checked as it opens.
/// </summary>
/// <remarks>
/// The engine's LoadShader gives an invalid shader where a file is missing or does not compile, with
/// the reason in the log, and the renderers draw on with the engine's own shader, so a build that left
/// the game's out would draw the land lit wrong and say nothing of it.
/// </remarks>
public static class OwnFiles
{
    /// <summary>The game's shaders, each with whether it is a compute shader.</summary>
    public static readonly (string Name, bool Compute)[] Shaders =
        [("terrain.slang", false), ("bits.slang", false), ("mob.slang", false), ("cavefields.slang", true)];

    /// <summary>The path beside the program of a shader of the game's.</summary>
    public static string ShaderPath(string name) => Path.Combine(AppContext.BaseDirectory, "resources", "shaders", name);

    /// <summary>The path beside the program of each file the game reads from there.</summary>
    public static IEnumerable<string> Paths =>
        Shaders.Select(shader => ShaderPath(shader.Name)).Prepend(Path.Combine(AppContext.BaseDirectory, "resources", "packs.json"));

    /// <summary>
    /// Why the game cannot open, naming the first of its files missing beside the program or not
    /// compiling, or null where every one is there and each shader compiles.
    /// </summary>
    public static string? Fault()
    {
        if (Paths.FirstOrDefault(path => !File.Exists(path)) is { } missing)
            return $"The game cannot open, since {missing} is missing.";
        foreach (var (name, compute) in Shaders)
        {
            var path = ShaderPath(name);
            var shader = compute ? LoadComputeShader(path) : LoadShader(path);
            if (!IsShaderValid(shader)) return $"The game cannot open, since {path} did not compile, with the reason above in the log.";
            UnloadShader(shader);
        }
        return null;
    }
}
