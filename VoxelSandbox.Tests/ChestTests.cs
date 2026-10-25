using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>A chest's loot, rolled once from its place, given up when its block goes, kept by a save, and its faces cut from a pack's chest.</summary>
public sealed class ChestTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, recursive: true);
    }

    // A world whose column at the origin holds a mineshaft's chest at 3, 10, 4, as a structure leaves one.
    private static (VoxelWorld World, Chests Chests) Placed()
    {
        var world = Laid.World(c => Laid.Fill(c, 0, 9, BlockId.Stone));
        var column = new ChunkColumn(0, 0);
        new ColumnClip(column).Chest(3, 10, 4, Facing.South, ChestLoot.Mineshaft.Name, 1);
        var chests = new Chests();
        var placed = Assert.Single(column.Chests);
        chests.Joined(column);
        Assert.Empty(column.Chests);
        Assert.Equal(BlockId.ChestSouth, column.Get(3, 10, 4));
        world.SetBlock(3, 10, 4, BlockId.ChestSouth);
        Assert.Equal((3, 10, 4), (placed.X, placed.Y, placed.Z));
        return (world, chests);
    }

    [Fact]
    public void A_Placed_Chests_Loot_Is_Rolled_Once_When_Opened_The_Same_From_Its_Place_And_Of_Its_Table()
    {
        var (_, chests) = Placed();
        Assert.Equal(ChestLoot.Mineshaft.Name, chests.All[(3, 10, 4)].Loot);
        var opened = chests.At(3, 10, 4);
        Assert.Null(opened.Loot);
        var held = opened.Slots.OfType<ItemStack>().ToList();
        // The third pool's three rolls give rails or torches every time.
        Assert.True(held.Count(s => s.Item == Items.Of(BlockId.Rail) || s.Item == Items.Of(BlockId.Torch)) >= 1, string.Join(", ", held));
        Assert.Equal(opened.Slots, Placed().Chests.At(3, 10, 4).Slots);
        // Taken from, it stays as left, its loot not rolled again.
        opened.Slots[Array.FindIndex(opened.Slots, s => s is not null)] = null;
        Assert.Equal(held.Count - 1, chests.At(3, 10, 4).Slots.OfType<ItemStack>().Count());
        // A column generated again leaves a chest it finds there as it is.
        var again = new ChunkColumn(0, 0);
        new ColumnClip(again).Chest(3, 10, 4, Facing.South, ChestLoot.Mineshaft.Name, 1);
        chests.Joined(again);
        Assert.Equal(held.Count - 1, chests.At(3, 10, 4).Slots.OfType<ItemStack>().Count());
    }

    [Fact]
    public void A_Broken_Chest_Gives_Up_Its_Loot_Rolled_And_A_Save_Keeps_The_Rest_Unrolled()
    {
        var (world, chests) = Placed();
        Assert.Empty(chests.Update(world));
        var unrolled = new Chest { Loot = ChestLoot.Mineshaft.Name, Seed = chests.All[(3, 10, 4)].Seed };
        unrolled.Roll();
        world.SetBlock(3, 10, 4, BlockId.Air);
        var spilled = chests.Update(world);
        Assert.Equal(unrolled.Slots.OfType<ItemStack>().ToList(), spilled.Select(s => s.Stack).ToList());
        Assert.All(spilled, s => Assert.Equal((3, 10, 4), s.At));
        Assert.Empty(chests.All);

        // A chest taken from and one not yet opened come back from a save as they were.
        var kept = new Chests();
        kept.At(-7, 30, 2).Slots[5] = new ItemStack(Items.Coal, 12);
        kept.Add((40, -20, 9), new Chest { Loot = ChestLoot.Mineshaft.Name, Seed = 77 });
        new WorldSave(_folder).WriteChests(kept);
        var back = new WorldSave(_folder).ReadChests();
        Assert.Equal(new ItemStack(Items.Coal, 12), back.All[(-7, 30, 2)].Slots[5]);
        Assert.Equal((ChestLoot.Mineshaft.Name, 77u), (back.All[(40, -20, 9)].Loot, back.All[(40, -20, 9)].Seed));
    }

    [Fact]
    public void A_Chests_Faces_Are_Cut_From_The_Model_Picture_Its_Lid_Over_Its_Base_And_Its_Latch_On_The_Front()
    {
        // A picture of the model whose every pixel tells its place, its red the column and its green the row.
        var model = new Image([.. Enumerable.Range(0, 64 * 64).SelectMany(i => new[] { (byte)(i % 64), (byte)(i / 64), (byte)100, (byte)255 })], 64, 64);
        Pixel(ChestArt.Cut(model, ChestArt.Top), 1, 1, 28, 0);
        Pixel(ChestArt.Cut(model, ChestArt.Bottom), 14, 14, 14 + 13, 19 + 13);
        var front = ChestArt.Cut(model, ChestArt.Front);
        // The lid's side upside down from row 2, the base's under it from row 7, and the latch at 7, 5.
        Pixel(front, 1, 2, 14, 14 + 4);
        Pixel(front, 1, 7, 14, 33 + 8);
        Pixel(front, 7, 5, 1, 1 + 3);
        Pixel(ChestArt.Cut(model, ChestArt.Side), 7, 5, 20, 14 + 1);
        // Outside the box the tile is clear.
        Assert.Equal(0, ChestArt.Cut(model, ChestArt.Front).Data[(0 * 16 + 0) * 4 + 3]);

        static void Pixel(Image tile, int x, int y, int fromX, int fromY)
        {
            var at = (y * tile.Width + x) * 4;
            Assert.Equal((fromX, fromY), (tile.Data[at], tile.Data[at + 1]));
        }
    }
}
