using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>The inventory's first nine slots, one of them chosen, picked by 1 to 9 and the wheel.</summary>
public sealed class Hotbar(Inventory inventory)
{
    public const int Size = 9;

    // Stone, planks and glass to build with, a torch, a lantern and two emissive blocks, and the
    // concrete walls that show color bleeding, which a world in creative mode begins with.
    private static readonly BlockId[] Creative =
    [
        BlockId.Stone, BlockId.OakPlanks, BlockId.Glass, BlockId.Torch, BlockId.Lantern, BlockId.Glowstone, BlockId.SeaLantern,
        BlockId.WhiteConcrete, BlockId.RedConcrete,
    ];

    /// <summary>Seconds the chosen item's name stays whole over the hotbar after the choice changes, and seconds it takes to fade, as Minecraft's 30 ticks and 10.</summary>
    public const float NameHeld = 1.5f, NameFades = 0.5f;

    private (int Slot, Item? Item) _named = (-1, null);
    private float _namedFor;

    public Inventory Inventory => inventory;

    public int Selected { get; set; }

    /// <summary>How much of the chosen item's name shows over the hotbar, whole as the chosen slot or what it holds changes and gone two seconds after.</summary>
    public float NameShown { get; private set; }

    /// <summary>The chosen slot's stack, or null where it is empty.</summary>
    public ItemStack? Current => inventory.Slots[Selected];

    /// <summary>The block the chosen slot's item places, or air where it holds none.</summary>
    public BlockId CurrentBlock => Current?.Item.Block ?? BlockId.Air;

    /// <summary>Fills the hotbar with the blocks a world in creative mode begins with.</summary>
    public void FillCreative()
    {
        for (int i = 0; i < Size; i++) inventory.Slots[i] = new ItemStack(Items.Of(Creative[i]), 1);
    }

    /// <summary>Follows the choice over a frame of so many seconds, showing the chosen item's name again where the slot or what it holds changed.</summary>
    public void Follow(float seconds)
    {
        var now = (Selected, Current?.Item);
        if (now != _named) (_named, _namedFor) = (now, 0);
        else _namedFor += seconds;
        NameShown = now.Item is null ? 0 : Math.Clamp(1 - (_namedFor - NameHeld) / NameFades, 0, 1);
    }

    public void Update(bool keys, bool wheel)
    {
        if (keys)
            for (int i = 0; i < Size; i++)
                if (IsKeyPressed(Key.One + i)) Selected = i;
        if (!wheel) return;
        // The wheel turned away from the player moves left, as Minecraft's does.
        var turned = GetMouseWheelMove();
        if (turned > 0) Selected = (Selected + Size - 1) % Size;
        else if (turned < 0) Selected = (Selected + 1) % Size;
    }

    /// <summary>
    /// Selects the hotbar's slot holding an item, as picking a block does, or where none does, puts
    /// the item into the chosen slot in creative mode and in survival moves a stack of it from the
    /// rest of the inventory there, swapping what the slot held.
    /// </summary>
    public void Pick(Item item, bool creative)
    {
        for (int i = 0; i < Size; i++)
            if (inventory.Slots[i]?.Item == item)
            {
                Selected = i;
                return;
            }
        if (creative)
        {
            inventory.Slots[Selected] = new ItemStack(item, 1);
            return;
        }
        var found = inventory.Find(item);
        if (found >= Size) (inventory.Slots[Selected], inventory.Slots[found]) = (inventory.Slots[found], inventory.Slots[Selected]);
    }
}
