using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The whole game the frame's tests run, built with no window.</summary>
[Collection(nameof(HeadlessGames))]
public sealed class HeadlessGameTests
{
    [Fact]
    public void A_Headless_Game_Stands_Its_Flat_World_With_Every_Column_In_Reach_And_The_Player_On_Its_Ground()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        Assert.False(game.Loading);
        Assert.Equal(0, game.Streamer.Pending);
        var feet = game.Player.Body.Position;
        int cx = VoxelWorld.ColumnOf((int)MathF.Floor(feet.X)), cz = VoxelWorld.ColumnOf((int)MathF.Floor(feet.Z));
        for (int dz = -2; dz <= 2; dz++)
            for (int dx = -2; dx <= 2; dx++)
                Assert.True(game.World.HasColumn(cx + dx, cz + dz), $"the column {cx + dx}, {cz + dz} is not in");
        headless.Step(10);
        Assert.True(game.Player.Body.OnGround, $"the player stands at {game.Player.Body.Position}");
        // With no device to compute on, every column reads its caves' noises itself.
        Assert.False(game.CaveFields.Running);
        Assert.Equal(0, game.CaveFields.Read);
    }
}
