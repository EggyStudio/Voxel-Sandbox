using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>
/// The files the game reads from beside the program, in the tests' folder, which takes the game's
/// resources as the program's does, so a build that copies them flat fails here.
/// </summary>
public sealed class OwnFilesTests
{
    [Fact]
    public void The_Built_Folder_Holds_Each_File_The_Game_Reads_Beside_It_In_Its_Place()
    {
        foreach (var path in OwnFiles.Paths) Assert.True(File.Exists(path), $"{path} is not in the built folder");
    }

    [Fact]
    public void The_Shaders_The_Game_Checks_As_It_Opens_Are_Every_Shader_It_Ships()
    {
        var shipped = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "resources", "shaders"), "*.slang").Select(Path.GetFileName).Order();
        Assert.Equal(shipped, OwnFiles.Shaders.Select(shader => shader.Name).Order());
    }
}
