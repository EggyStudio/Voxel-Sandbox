using System.Diagnostics;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The game run as a program of its own with no window, on a data folder of the test's own.</summary>
public sealed class ProcessTests : IDisposable
{
    private readonly string _folder = Directory.CreateTempSubdirectory("voxel-process-").FullName;

    public void Dispose() => Directory.Delete(_folder, true);

    [Fact]
    public void A_Request_To_End_The_Process_Saves_The_World_As_The_Windows_Close_Does()
    {
        // A signal is sent to a process on Linux and macOS alone.
        if (OperatingSystem.IsWindows()) return;
        // The data folder's list of packs written empty, so the game fetches no pack.
        var data = Path.Combine(_folder, "VoxelSandbox");
        Directory.CreateDirectory(data);
        File.WriteAllText(Path.Combine(data, "resourcepacks.txt"), "");
        var start = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var argument in new[] { typeof(VoxelGame).Assembly.Location, "--flat", "--world", "ended" }) start.ArgumentList.Add(argument);
        start.Environment["E3D_HEADLESS"] = "1";
        start.Environment["XDG_DATA_HOME"] = _folder;
        using var stood = new ManualResetEventSlim();
        using var game = new Process { StartInfo = start };
        game.OutputDataReceived += (_, line) =>
        {
            if (line.Data?.Contains("WorldState: Loading -> Playing") == true) stood.Set();
        };
        game.ErrorDataReceived += (_, _) => { };
        game.Start();
        game.BeginOutputReadLine();
        game.BeginErrorReadLine();
        try
        {
            Assert.True(stood.Wait(TimeSpan.FromMinutes(2)), "the world did not stand within two minutes");
            using (var kill = Process.Start("kill", ["-TERM", game.Id.ToString()])) kill.WaitForExit();

            Assert.True(game.WaitForExit(TimeSpan.FromMinutes(1)), "the game did not end within a minute of the request");
            Assert.Equal(0, game.ExitCode);
            Assert.True(File.Exists(Path.Combine(data, "saves", "ended", "world.json")));
        }
        finally
        {
            if (!game.HasExited) game.Kill(entireProcessTree: true);
        }
    }
}
