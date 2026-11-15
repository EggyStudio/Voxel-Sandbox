using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The player's vitals as the game's frame takes them, in a whole game with no window.</summary>
[Collection(nameof(HeadlessGames))]
public sealed class VitalsFrameTests
{
    [Fact]
    public void A_Flight_In_Creative_Or_Spectator_Tires_The_Player_Nothing_Back_In_Survival()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        game.Mode = GameMode.Survival;
        headless.Step(3);
        var before = (game.Vitals.Exhaustion, game.Vitals.Saturation, game.Vitals.Food);

        // Two flights of 200 blocks east, out of survival, where the vitals are not taken.
        foreach (var away in new[] { GameMode.Creative, GameMode.Spectator })
        {
            game.Mode = away;
            for (int i = 0; i < 40; i++)
            {
                game.Player.Body.Position += new Vector3(5, 0, 0);
                headless.Step(1);
            }
            game.Mode = GameMode.Survival;
            headless.Step(2);
        }

        // Four of exhaustion take a point of saturation, so the saturation and the food are held too.
        Assert.Equal((before.Saturation, before.Food), (game.Vitals.Saturation, game.Vitals.Food));
        Assert.True(game.Vitals.Exhaustion - before.Exhaustion < 0.5f, $"the flights tired the player by {game.Vitals.Exhaustion - before.Exhaustion:0.00}");
    }

    [Fact]
    public void A_Fall_Begun_In_Survival_And_Ended_In_Creative_Harms_Nothing_Back_In_Survival()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        game.Mode = GameMode.Survival;
        var top = game.Player.Body.Position.Y + 30;
        game.Player.Teleport(game.Player.Body.Position + new Vector3(0, 30, 0));
        headless.Step(2);
        // Twelve blocks fallen in survival, the rest in creative.
        for (int i = 0; i < 300 && top - game.Player.Body.Position.Y < 12; i++) headless.Step(1);
        Assert.True(game.Vitals.Fallen > 10, $"the fall counted {game.Vitals.Fallen:0.0}");
        game.Mode = GameMode.Creative;
        for (int i = 0; i < 300 && !game.Player.Body.OnGround; i++) headless.Step(1);
        headless.Step(10);

        game.Mode = GameMode.Survival;
        headless.Step(3);

        Assert.Equal(Vitals.Most, game.Vitals.Health);
    }
}
