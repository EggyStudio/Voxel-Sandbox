namespace VoxelSandbox;

/// <summary>A slot of an array of slots, the inventory's, a crafting grid's or a furnace's.</summary>
public readonly record struct SlotAt(ItemStack?[] Slots, int Index)
{
    public ItemStack? Stack
    {
        get => Slots[Index];
        set => Slots[Index] = value;
    }
}

/// <summary>
/// What the inventory window's quick moves do to its slots, as Minecraft's do: a shift-click's move
/// of a stack into the slots it goes to, a double-click's gathering of a kind onto the held stack, a
/// drag's spreading of the held stack over slots, and the sort of a run of slots by kind, as the
/// sorting mods of Minecraft do it.
/// </summary>
public static class SlotMoves
{
    /// <summary>
    /// Moves a slot's stack into other slots, onto the stacks of its item with room first and then into
    /// the first empty one, each in the order given, as a shift-click does.
    /// </summary>
    /// <returns>Whether any of it moved.</returns>
    public static bool MoveInto(SlotAt from, IReadOnlyList<SlotAt> into)
    {
        if (from.Stack is not { } stack) return false;
        var (item, left) = (stack.Item, stack.Count);
        if (item.MostInStack > 1)
            foreach (var target in into)
                if (left > 0 && target.Stack is { } there && there.Item == item && there.Count < item.MostInStack)
                {
                    var moved = Math.Min(left, item.MostInStack - there.Count);
                    target.Slots[target.Index] = there with { Count = there.Count + moved };
                    left -= moved;
                }
        if (left > 0)
            foreach (var target in into)
                if (target.Stack is null)
                {
                    target.Slots[target.Index] = stack with { Count = left };
                    left = 0;
                    break;
                }
        from.Slots[from.Index] = left > 0 ? stack with { Count = left } : null;
        return left < stack.Count;
    }

    /// <summary>
    /// Where a shift-click on a slot of the inventory moves its stack: into the furnace open, its
    /// input for what smelts and its fuel for what burns, and otherwise from the hotbar into the rest
    /// of the inventory and from the rest into the hotbar, as Minecraft's inventory moves it.
    /// </summary>
    /// <returns>Whether any of it moved.</returns>
    public static bool QuickMove(Inventory inventory, int slot, Furnace? furnace)
    {
        var from = new SlotAt(inventory.Slots, slot);
        if (from.Stack is not { } stack) return false;
        if (furnace is not null)
        {
            if (Smelting.Result(stack.Item) is not null && MoveInto(from, [new SlotAt(furnace.Slots, Furnace.Input)])) return true;
            if (Smelting.Burns(stack.Item) > 0 && MoveInto(from, [new SlotAt(furnace.Slots, Furnace.Fuel)])) return true;
        }
        return MoveInto(from, slot < Hotbar.Size ? [.. Range(inventory.Slots, Hotbar.Size, Inventory.Size)] : [.. Range(inventory.Slots, 0, Hotbar.Size)]);
    }

    /// <summary>
    /// The inventory's slots in the order a stack from a grid or a furnace goes into them, the
    /// hotbar's from its last and then the rest from its last, as Minecraft fills them from the end.
    /// </summary>
    public static IReadOnlyList<SlotAt> FromContainer(Inventory inventory) =>
        [.. Range(inventory.Slots, 0, Hotbar.Size).Reverse(), .. Range(inventory.Slots, Hotbar.Size, Inventory.Size).Reverse()];

    /// <summary>How many of an item the slots have room for, on stacks of it and in empty slots.</summary>
    public static int Room(IReadOnlyList<SlotAt> slots, Item item) =>
        slots.Sum(at => at.Stack is not { } there ? item.MostInStack : there.Item == item ? item.MostInStack - there.Count : 0);

    /// <summary>
    /// Fills the held stack with every stack of its item among the slots, those not full first and
    /// then the full ones, up to a whole stack, as a double-click gathers a kind.
    /// </summary>
    public static ItemStack Gather(ItemStack held, IReadOnlyList<SlotAt> slots)
    {
        var most = held.Item.MostInStack;
        for (int pass = 0; pass < 2; pass++)
            foreach (var at in slots)
            {
                if (held.Count >= most) return held;
                if (at.Stack is not { } stack || stack.Item != held.Item || stack.Wear != held.Wear || pass == 0 && stack.Count >= most) continue;
                var moved = Math.Min(stack.Count, most - held.Count);
                held = held with { Count = held.Count + moved };
                at.Slots[at.Index] = stack.Count > moved ? stack with { Count = stack.Count - moved } : null;
            }
        return held;
    }

    /// <summary>
    /// Spreads the held stack over the slots a drag passed, an even share into each, or one into each
    /// for a drag with the right button, each slot empty or holding the item with room, as Minecraft's
    /// drag spreads a stack.
    /// </summary>
    /// <returns>What is left held, or null where none is.</returns>
    public static ItemStack? Spread(ItemStack held, IReadOnlyList<SlotAt> over, bool one)
    {
        var most = held.Item.MostInStack;
        var targets = over.Where(at => at.Stack is null || at.Stack is { } there && there.Item == held.Item && there.Wear == held.Wear && there.Count < most).ToList();
        if (targets.Count == 0) return held;
        var each = one ? 1 : Math.Max(1, held.Count / targets.Count);
        var left = held.Count;
        foreach (var at in targets)
        {
            if (left == 0) break;
            var there = at.Stack?.Count ?? 0;
            var put = Math.Min(Math.Min(each, left), most - there);
            at.Slots[at.Index] = held with { Count = there + put };
            left -= put;
        }
        return left > 0 ? held with { Count = left } : null;
    }

    /// <summary>
    /// Sorts a run of slots, each kind's stacks together and filled, in the order of the items, a
    /// worn tool apart by its wear, the empty slots after them.
    /// </summary>
    public static void Sort(ItemStack?[] slots, int start, int end)
    {
        var totals = new Dictionary<(Item Item, int Wear), int>();
        for (int i = start; i < end; i++)
            if (slots[i] is { } stack)
            {
                totals[(stack.Item, stack.Wear)] = totals.GetValueOrDefault((stack.Item, stack.Wear)) + stack.Count;
                slots[i] = null;
            }
        var at = start;
        foreach (var ((item, wear), count) in totals.OrderBy(t => t.Key.Item.Id).ThenBy(t => t.Key.Wear))
            for (var left = count; left > 0; left -= item.MostInStack)
                slots[at++] = new ItemStack(item, Math.Min(left, item.MostInStack), wear);
    }

    /// <summary>The slots of an array from one index up to another.</summary>
    public static IEnumerable<SlotAt> Range(ItemStack?[] slots, int start, int end)
    {
        for (int i = start; i < end; i++) yield return new SlotAt(slots, i);
    }
}
