using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The stacks on the ground as the game's frame moves, picks up and joins them.</summary>
[Collection(nameof(HeadlessGames))]
public sealed class ItemFrameTests
{
    [Fact]
    public void A_Stack_On_The_Ground_Is_Picked_Up_After_Its_Delay_As_Far_As_The_Inventory_Has_Room()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        game.Mobs.Clear(null);
        var inventory = game.Inventory;
        for (int i = 1; i < Inventory.Size; i++) inventory.Slots[i] = new ItemStack(Items.Feather, 64);
        inventory.Slots[0] = new ItemStack(Items.Of(BlockId.Sand), 60);
        var feet = game.Player.Body.Position;
        game.Drops.Spawn(Items.Of(BlockId.Sand), 10, feet + new Vector3(1, 0.5f, 0), Vector3.Zero);

        // Within its half second's delay nothing is taken, and past it as much as the slot holds.
        headless.Step(9);
        Assert.Equal(10, Assert.Single(game.Drops.All).Drop.Count);
        headless.Step(9);
        Assert.Equal(64, inventory.Slots[0]!.Value.Count);
        var left = Assert.Single(game.Drops.All);
        Assert.Equal(6, left.Drop.Count);
        Assert.Equal(MathF.Floor(feet.Y), left.Body.Position.Y, 2);
    }

    [Fact]
    public void Two_Stacks_Of_One_Item_Lying_Together_Join()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        game.Mobs.Clear(null);
        // Ten blocks from the player, out of its reach.
        var at = game.Player.Body.Position + new Vector3(10, 0, 0);
        game.Drops.Spawn(Items.Leather, 2, at, Vector3.Zero);
        game.Drops.Spawn(Items.Leather, 3, at + new Vector3(0.2f, 0, 0), Vector3.Zero);
        game.Drops.Spawn(Items.Feather, 1, at + new Vector3(0.1f, 0, 0), Vector3.Zero);

        headless.Step(3);

        Assert.Equal(2, game.Drops.Count);
        Assert.Equal(5, game.Drops.All.Single(d => d.Drop.Item == Items.Leather).Drop.Count);
    }
}
