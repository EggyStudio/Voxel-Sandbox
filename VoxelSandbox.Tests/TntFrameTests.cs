using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>TNT lit and going off, as the source's does, as the game's frame burns it.</summary>
[Collection(nameof(HeadlessGames))]
public sealed class TntFrameTests
{
    [Fact]
    public void A_Lit_Tnt_Pops_Up_Lands_Flashes_And_Goes_Off_At_Four_Seconds()
    {
        using var headless = new HeadlessGame(frameSeconds: 1 / 60f);
        var game = headless.Game;
        game.Mobs.Clear(null);
        // Lit on the flat world's ground six blocks from the player, at its feet's height.
        int x = (int)MathF.Floor(game.Player.Body.Position.X) + 6, feet = (int)MathF.Floor(game.Player.Body.Position.Y), z = (int)MathF.Floor(game.Player.Body.Position.Z);
        game.Tnt.Light(x, feet, z);
        Assert.True(Assert.Single(game.Tnt.All).Lit.Flashing);
        var highest = 0f;
        for (int frame = 0; frame < 4 * 60 - 1; frame++)
        {
            headless.Step(1);
            highest = MathF.Max(highest, Assert.Single(game.Tnt.All).Body.Position.Y);
            Assert.Empty(game.Blasts.All);
        }
        Assert.InRange(highest - feet, 0.3f, 1.5f);
        var (lit, body) = Assert.Single(game.Tnt.All);
        Assert.True(body.OnGround);
        Assert.True(lit.Size > 1.25f);
        // The last tick of the fuse sets it off a sixteenth over its feet, as an event the game carries
        // out in the frame after.
        for (int frame = 0; frame < 3 && game.Tnt.Count > 0; frame++) headless.Step(1);
        headless.Step(1);
        Assert.Empty(game.Tnt.All);
        var blast = Assert.Single(game.Blasts.All).At;
        Assert.InRange(blast.Y - feet, 0.06f, 0.07f);
        Assert.InRange(blast.X - x, 0.1f, 0.9f);

        // TNT another blast reaches burns 10 to 29 ticks.
        for (int i = 0; i < 20; i++) game.Tnt.Chain(x, feet + 8, z);
        Assert.All(game.Tnt.All, t => Assert.InRange(t.Lit.Left, 0.5f, 1.45f));
    }
}
