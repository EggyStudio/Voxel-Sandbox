using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>
/// The console's commands given the ends of what they take, each answered in its frame with what it
/// did or why it did nothing.
/// </summary>
[Collection(nameof(HeadlessGames))]
public sealed class CommandTests : IDisposable
{
    public void Dispose() => Serve(null);

    // The game the commands answer of, as the program sets it, which a test reaches through the
    // setter the program uses.
    private static void Serve(VoxelGame? game) => typeof(GameCommands).GetProperty(nameof(GameCommands.Game))!.SetValue(null, game);

    private static string Run(string line) => ConsoleCommands.Run(line) ?? "";

    [Fact]
    public void A_Fill_Or_A_Room_Reaching_The_Ints_End_Answers_At_Once_And_Changes_No_Block()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        Serve(game);
        var before = game.World.GetBlock(0, 4, 0);
        (string Line, string Said)[] boxes =
        [
            ("voxel.fill 0 4 0 2147483647 4 0 stone", "corners lie within"),
            ("voxel.room 2147483647 4 0 2147483647 4 0 stone", "corners lie within"),
            ("voxel.fill -2147483648 4 0 0 4 0 stone", "corners lie within"),
            ("voxel.fill -1073741824 4 0 1073741824 4 0 stone", "the box holds 2147483649 blocks"),
        ];

        foreach (var (line, said) in boxes)
        {
            // Each runs on a thread of its own, which the process does not wait for, so a loop that
            // does not end fails the test rather than holds it.
            var answer = "";
            var worker = new Thread(() => answer = Run(line)) { IsBackground = true };
            worker.Start();
            Assert.True(worker.Join(TimeSpan.FromSeconds(10)), $"{line} gave no answer within 10 s");
            Assert.True(answer.Contains(said), $"{line} said {answer}");
        }
        Assert.Equal(before, game.World.GetBlock(0, 4, 0));
    }

    [Fact]
    public void The_Save_Command_Says_So_Where_The_Worlds_Files_Cannot_Be_Written()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            // A world saved once, then a folder in the place of each file a save writes before it
            // takes the file's name, so none can be written, on every system alike.
            using (var first = new HeadlessGame(save: folder)) first.Game.World.SetBlock(2, 70, 2, BlockId.Glowstone);
            foreach (var name in new[] { "world.json", Path.Combine("regions", "m.0.0.bin"), "furnaces.json", "chests.json", Path.Combine("regions", "r.0.0.bin") })
                Directory.CreateDirectory(Path.Combine(folder, name + ".new"));
            using var headless = new HeadlessGame(save: folder);
            Serve(headless.Game);
            headless.Game.World.SetBlock(4, 70, 2, BlockId.Glowstone);

            var said = Run("voxel.save");

            Assert.StartsWith("the world could not be saved", said);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_World_Played_Unsaved_Is_Said_To_Be_Unsaved_And_Not_Transient()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            // A file where the world's folder would stand above it.
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, "a file"), "");
            using var headless = new HeadlessGame(save: Path.Combine(folder, "a file", "world"));
            Serve(headless.Game);

            var state = Run("voxel.state");
            Assert.Contains(" times, unsaved, ", state);
            Assert.DoesNotContain("transient", state);
            var said = Run("voxel.save");
            Assert.Contains("unsaved", said);
            Assert.DoesNotContain("transient", said);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Quality_Of_The_Light_Naming_No_Value_Is_Refused()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        Serve(game);
        var quality = game.Light.Quality;

        foreach (var number in new[] { "7", "-1" })
            Assert.Contains("no quality is called", Run($"voxel.gi {number}"));
        Assert.Equal(quality, game.Light.Quality);
    }

    [Fact]
    public void A_Mode_The_Worlds_Info_Gives_By_A_Number_Naming_None_Is_Read_As_A_New_Worlds()
    {
        var folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            new WorldSave(folder).WriteInfo(new WorldInfo("flat", 1, 0, 4, 0, 0, 0, false, 10, [.. Blocks.All.Select(b => b.Key)], "7"));
            using var headless = new HeadlessGame(save: folder);

            Assert.Equal(GameMode.Creative, headless.Game.Mode);
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void A_Recording_Past_36000_Frames_Is_Refused_With_Its_Most()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        Serve(game);

        string[] lines =
        [
            "voxel.flicker 36001 0 0 5 flicker.txt",
            "voxel.flicker 2147483647 0 0 5 flicker.txt",
            "voxel.flicker 20 0 0 2147483647 flicker.txt",
        ];
        foreach (var line in lines)
            Assert.Contains("at most 36000 frames", Run(line));
        Assert.Null(game.Flicker);
    }
}
