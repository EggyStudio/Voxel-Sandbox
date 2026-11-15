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
}
