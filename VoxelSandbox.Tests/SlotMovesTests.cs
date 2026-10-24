using Xunit;

namespace VoxelSandbox.Tests;

public class SlotMovesTests
{
    private static readonly Item Stone = Items.Of(BlockId.Stone), Dirt = Items.Of(BlockId.Dirt), Sword = Items.Find("iron_sword")!;

    [Fact]
    public void A_Shift_Click_Moves_A_Hotbar_Stack_Onto_Its_Kind_Above_And_Then_Into_The_First_Empty_Slot()
    {
        var inventory = new Inventory();
        inventory.Slots[0] = new ItemStack(Stone, 40);
        inventory.Slots[12] = new ItemStack(Stone, 50);

        Assert.True(SlotMoves.QuickMove(inventory, 0, null));

        Assert.Null(inventory.Slots[0]);
        Assert.Equal(64, inventory.Slots[12]!.Value.Count);
        Assert.Equal(new ItemStack(Stone, 26), inventory.Slots[Hotbar.Size]);
    }

    [Fact]
    public void A_Shift_Click_Moves_A_Stack_Above_Into_The_Hotbar_And_Into_A_Furnace_By_What_It_Smelts_Or_Burns()
    {
        var inventory = new Inventory();
        inventory.Slots[20] = new ItemStack(Dirt, 5);
        inventory.Slots[21] = new ItemStack(Items.RawBeef, 3);
        inventory.Slots[22] = new ItemStack(Items.Coal, 8);
        var furnace = new Furnace();

        SlotMoves.QuickMove(inventory, 20, furnace);
        SlotMoves.QuickMove(inventory, 21, furnace);
        SlotMoves.QuickMove(inventory, 22, furnace);

        // Dirt neither smelts nor burns, so it goes into the hotbar's first empty slot.
        Assert.Equal(new ItemStack(Dirt, 5), inventory.Slots[0]);
        Assert.Equal(new ItemStack(Items.RawBeef, 3), furnace.Slots[Furnace.Input]);
        Assert.Equal(new ItemStack(Items.Coal, 8), furnace.Slots[Furnace.Fuel]);
    }

    [Fact]
    public void A_Stack_Out_Of_A_Container_Fills_The_Inventory_From_The_Hotbars_Last_Slot()
    {
        var inventory = new Inventory();
        var grid = new ItemStack?[] { new ItemStack(Dirt, 7) };

        SlotMoves.MoveInto(new SlotAt(grid, 0), SlotMoves.FromContainer(inventory));

        Assert.Null(grid[0]);
        Assert.Equal(new ItemStack(Dirt, 7), inventory.Slots[Hotbar.Size - 1]);
    }

    [Fact]
    public void A_Double_Click_Gathers_The_Stacks_Not_Full_First_Up_To_A_Whole_Stack()
    {
        var inventory = new Inventory();
        inventory.Slots[3] = new ItemStack(Stone, 64);
        inventory.Slots[5] = new ItemStack(Stone, 20);
        inventory.Slots[6] = new ItemStack(Dirt, 30);

        var held = SlotMoves.Gather(new ItemStack(Stone, 10), [.. SlotMoves.Range(inventory.Slots, 0, Inventory.Size)]);

        Assert.Equal(new ItemStack(Stone, 64), held);
        Assert.Null(inventory.Slots[5]);
        Assert.Equal(new ItemStack(Stone, 30), inventory.Slots[3]);
        Assert.Equal(new ItemStack(Dirt, 30), inventory.Slots[6]);
    }

    [Fact]
    public void A_Drag_Spreads_The_Held_Stack_Evenly_Or_One_A_Slot_Over_The_Slots_It_Passed()
    {
        var slots = new ItemStack?[4];
        slots[3] = new ItemStack(Dirt, 1);
        SlotAt[] over = [new(slots, 0), new(slots, 1), new(slots, 2), new(slots, 3)];

        // The slot holding dirt is passed over, so ten go three a slot into the other three.
        var left = SlotMoves.Spread(new ItemStack(Stone, 10), over, one: false);
        Assert.Equal(new ItemStack(Stone, 1), left);
        Assert.All(slots[..3], s => Assert.Equal(new ItemStack(Stone, 3), s));

        var ones = new ItemStack?[3];
        left = SlotMoves.Spread(new ItemStack(Stone, 10), [new(ones, 0), new(ones, 1), new(ones, 2)], one: true);
        Assert.Equal(new ItemStack(Stone, 7), left);
        Assert.All(ones, s => Assert.Equal(new ItemStack(Stone, 1), s));
    }

    [Fact]
    public void A_Drag_Shows_Each_Slots_Share_And_What_Is_Left_Before_It_Is_Let_Go_And_Changes_No_Slot()
    {
        var slots = new ItemStack?[4];
        slots[3] = new ItemStack(Dirt, 1);
        SlotAt[] over = [new(slots, 0), new(slots, 1), new(slots, 2), new(slots, 3)];

        var (put, left) = SlotMoves.Shares(new ItemStack(Stone, 10), over, one: false);
        Assert.Equal([over[0], over[1], over[2]], put.Select(p => p.At));
        Assert.All(put, p => Assert.Equal(new ItemStack(Stone, 3), p.Stack));
        Assert.Equal(new ItemStack(Stone, 1), left);
        // The slots stay as they are until the drag is let go and Spread puts the shares in them.
        Assert.All(slots[..3], s => Assert.Null(s));
    }

    [Fact]
    public void A_Sort_Puts_Each_Kind_Together_In_Filled_Stacks_And_Keeps_Worn_Tools_Apart()
    {
        var slots = new ItemStack?[8];
        slots[0] = new ItemStack(Dirt, 10);
        slots[2] = new ItemStack(Stone, 50);
        slots[3] = new ItemStack(Sword, 1, 7);
        slots[4] = new ItemStack(Dirt, 60);
        slots[6] = new ItemStack(Stone, 30);
        slots[7] = new ItemStack(Sword, 1);

        SlotMoves.Sort(slots, 0, slots.Length);

        var (first, second) = Stone.Id < Dirt.Id ? (Stone, Dirt) : (Dirt, Stone);
        int Total(Item item) => item == Stone ? 80 : 70;
        Assert.Equal(new ItemStack(first, 64), slots[0]);
        Assert.Equal(new ItemStack(first, Total(first) - 64), slots[1]);
        Assert.Equal(new ItemStack(second, 64), slots[2]);
        Assert.Equal(new ItemStack(second, Total(second) - 64), slots[3]);
        Assert.Equal(new ItemStack(Sword, 1, 0), slots[4]);
        Assert.Equal(new ItemStack(Sword, 1, 7), slots[5]);
        Assert.Null(slots[6]);
        Assert.Null(slots[7]);
    }
}
