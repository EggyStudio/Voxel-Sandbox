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
        // The data folder the test's own, given to the game, since the folder a system keeps an
        // account's data in is the account's own on macOS whatever the home, and its list of packs
        // written empty, so the game fetches no pack.
        var data = Path.Combine(_folder, "data");
        Directory.CreateDirectory(data);
        File.WriteAllText(Path.Combine(data, "resourcepacks.txt"), "");
        var start = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var argument in new[] { typeof(VoxelGame).Assembly.Location, "--flat", "--world", "ended", "--data", data }) start.ArgumentList.Add(argument);
        start.Environment["E3D_HEADLESS"] = "1";
        using var stood = new ManualResetEventSlim();
        using var game = new Process { StartInfo = start };
        // The game's last lines, of its output and its errors, kept for a failure to give.
        var said = new System.Collections.Concurrent.ConcurrentQueue<string>();
        void Said(string? line)
        {
            if (line is null) return;
            said.Enqueue(line);
            while (said.Count > 40) said.TryDequeue(out _);
        }
        // The folder the game's log says it saved the world in, the last it said.
        string? saved = null;
        game.OutputDataReceived += (_, line) =>
        {
            Said(line.Data);
            if (line.Data?.Contains("WorldState: Loading -> Playing") == true) stood.Set();
            if (line.Data?.IndexOf(" is saved in ", StringComparison.Ordinal) is >= 0 and var at) saved = line.Data[(at + 13)..].TrimEnd('.');
        };
        game.ErrorDataReceived += (_, line) => Said(line.Data);
        string Told() => $"{(game.HasExited ? $"it ended with code {game.ExitCode}" : "it runs on")}, its last lines\n{string.Join("\n", said)}";
        game.Start();
        game.BeginOutputReadLine();
        game.BeginErrorReadLine();
        try
        {
            var clock = Stopwatch.StartNew();
            while (!stood.Wait(100) && !game.HasExited && clock.Elapsed < TimeSpan.FromMinutes(2)) { }
            Assert.True(stood.IsSet, $"the world did not stand, {Told()}");
            using (var kill = Process.Start("kill", ["-TERM", game.Id.ToString()])) kill.WaitForExit();

            Assert.True(game.WaitForExit(TimeSpan.FromMinutes(1)), $"the game did not end within a minute of the request, {Told()}");
            // The output read to its end, which a wait with a limit does not wait for, so its last
            // lines are in.
            game.WaitForExit();
            Assert.True(game.ExitCode == 0, $"the game ended with code {game.ExitCode}, {Told()}");
            // The world is read where the game's log says it saved it, in the data folder given.
            Assert.True(saved == Path.Combine(data, "saves", "ended"), $"the world was said saved in {saved ?? "no folder"}, {Told()}");
            Assert.True(File.Exists(Path.Combine(data, "saves", "ended", "world.json")), $"no world's info was written, {Told()}");
        }
        finally
        {
            if (!game.HasExited) game.Kill(entireProcessTree: true);
        }
    }
}
