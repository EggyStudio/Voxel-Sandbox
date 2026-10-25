using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>Fire burning, spreading and going out, as the source's does, as the game's frame burns it.</summary>
[Collection(nameof(HeadlessGames))]
public sealed class FireFrameTests
{
    // A square of a block, 16 along x and z, laid afresh in the flat world at the top of its ground
    // eight blocks from the player within a ring of stone, with air over both, and a fire lit over
    // its middle with the step's chances seeded, burned for two minutes of the game's frames of a
    // twentieth of a second. Gives how many fires burned at once at most and how many of the
    // square's blocks went.
    private static (int Most, int Burned) Burn(HeadlessGame headless, int seed, BlockId block)
    {
        var game = headless.Game;
        var ground = (int)MathF.Floor(game.Player.Body.Position.Y) - 1;
        int x0 = (int)MathF.Floor(game.Player.Body.Position.X) + 8, z0 = (int)MathF.Floor(game.Player.Body.Position.Z) - 8;
        for (int z = z0 - 4; z < z0 + 20; z++)
            for (int x = x0 - 4; x < x0 + 20; x++)
            {
                game.World.SetBlock(x, ground, z, x >= x0 && x < x0 + 16 && z >= z0 && z < z0 + 16 ? block : BlockId.Stone);
                for (int y = ground + 1; y <= ground + 6; y++) game.World.SetBlock(x, y, z, BlockId.Air);
            }
        game.Fires.Clear();
        game.Step.Random = new Random(seed);
        Assert.True(game.Fires.Light(game.World, x0 + 8, ground + 1, z0 + 8, game.Step.Random));
        var most = 0;
        for (int frame = 0; frame < 120 * 20; frame++)
        {
            headless.Step(1);
            most = Math.Max(most, game.Fires.All.Count());
        }
        var burned = 0;
        for (int z = z0; z < z0 + 16; z++)
            for (int x = x0; x < x0 + 16; x++)
                if (game.World.GetBlock(x, ground, z) is BlockId.Air or BlockId.Fire) burned++;
        return (most, burned);
    }

    [Fact]
    public void A_Fire_On_Planks_Spreads_And_Burns_Them_Away()
    {
        using var headless = new HeadlessGame(frameSeconds: 0.05f);
        headless.Game.Mobs.Clear(null);
        for (int seed = 1; seed <= 3; seed++)
        {
            var (most, burned) = Burn(headless, seed, BlockId.OakPlanks);
            Assert.True(most >= 5, $"seed {seed} burned at most {most} fires");
            Assert.True(burned >= 3, $"seed {seed} burned {burned} planks away");
        }
    }

    [Fact]
    public void A_Fire_On_Stone_Dies_Where_It_Was_Lit()
    {
        using var headless = new HeadlessGame(frameSeconds: 0.05f);
        var game = headless.Game;
        game.Mobs.Clear(null);
        for (int seed = 1; seed <= 3; seed++)
        {
            var (most, burned) = Burn(headless, seed, BlockId.Stone);
            Assert.Equal(1, most);
            Assert.Equal(0, burned);
            Assert.Empty(game.Fires.All);
        }
    }
}
