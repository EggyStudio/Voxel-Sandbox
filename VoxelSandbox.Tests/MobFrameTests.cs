using System.Numerics;
using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The mobs as the game's frame steps them, in a whole game with no window.</summary>
[Collection(nameof(HeadlessGames))]
public sealed class MobFrameTests
{
    // A mob of a kind beside the player, so many blocks along x and up, the animals the flat world
    // placed taken away first.
    private static Mob Beside(VoxelGame game, MobKind kind, float x, float up = 0)
    {
        var at = game.Player.Body.Position + new Vector3(x, up, 0);
        return game.Mobs.Add(new MobRecord(kind, 0, at, 0, kind.Health, null));
    }

    [Fact]
    public void Each_Mob_Is_Read_By_One_Query_Of_Its_Kind_And_Moved_By_Its_Mind_In_The_Frame()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        game.Mobs.Clear(null);
        var pig = Beside(game, MobKinds.Pig, 3, up: 6);
        var zombie = Beside(game, MobKinds.Zombie, -3, up: 6);
        var ecs = headless.Ecs;
        // The passive mobs are those with no hunt, the hostile those with one, each mob in one of the two.
        Assert.Equal([pig], ecs.Query<Mob, Body, MobLife, MobPose>().Without<MobHostile>().Select(row => row.C1));
        Assert.Equal([zombie], ecs.Query<Mob, Body, MobLife, MobPose, MobHostile>().Select(row => row.C1));
        var (pigFrom, zombieFrom) = (pig.Body.Position.Y, zombie.Body.Position.Y);
        headless.Step(3);
        // In the air, each has fallen, moved by its own mind, and the pose its movement wrote is the
        // one its entity holds.
        Assert.True(pig.Body.Position.Y < pigFrom, $"the pig stands at {pig.Body.Position}");
        Assert.True(zombie.Body.Position.Y < zombieFrom, $"the zombie stands at {zombie.Body.Position}");
        Assert.Equal(pig.Stride, ecs.GetRef<MobPose>(pig.Entity).Stride);
    }

    [Fact]
    public void A_Zombie_Beside_A_Player_In_Survival_Strikes_It_Through_The_Frame()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        game.Mobs.Clear(null);
        game.Mode = GameMode.Survival;
        headless.Step(1);
        Beside(game, MobKinds.Zombie, 1);
        // Two seconds bring at least one strike of three, sent as an event the game carries out.
        headless.Step(60);
        Assert.True(game.Vitals.Health <= Vitals.Most - 3, $"the player's health is {game.Vitals.Health}");
    }

    [Fact]
    public void A_Mob_Killed_Is_Taken_Away_After_It_Has_Lain_Dying_And_Leaves_Its_Loot()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        game.Mobs.Clear(null);
        var pig = Beside(game, MobKinds.Pig, 2);
        Assert.Equal(0, game.Drops.Count);
        Assert.True(pig.Hit(game.Player.Body.Position, 100));
        headless.Step(15);
        Assert.Contains(pig, game.Mobs.All);
        // A second of lying dying, then the frame takes it away and the game drops its pork.
        headless.Step(30);
        Assert.DoesNotContain(pig, game.Mobs.All);
        Assert.Contains(game.Drops.All, drop => drop.Drop.Item == Items.RawPorkchop);
    }

    [Fact]
    public void A_Skeleton_At_Night_Shoots_A_Player_In_Survival_And_Its_Arrows_Strike_It_Through_The_Frame()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        game.Mobs.Clear(null);
        game.Mode = GameMode.Survival;
        game.Sky.Hour = 0;
        headless.Step(1);
        Beside(game, MobKinds.Skeleton, 8);
        // In five seconds it shoots two or three arrows, each of three or four points, which the
        // frame flies home and the game deals.
        headless.Step(150);
        Assert.InRange(game.Vitals.Health, Vitals.Most - 12, Vitals.Most - 3);
    }
}
