using System.Numerics;
using Engine;
using ImGuiNET;

namespace VoxelSandbox;

/// <summary>
/// The window E opens, which shows in survival the inventory's 36 slots under a crafting grid of
/// two by two, in creative every item to put in the hotbar's chosen slot, at a crafting table the
/// inventory under a grid of three by three in either mode, and at a furnace the inventory under
/// the furnace's slots.
/// </summary>
/// <remarks>
/// The slots are laid out as Minecraft's are, three rows of nine and the hotbar's below them. A left
/// click takes up a slot's stack, puts the stack held down, joins it to a stack of its item or swaps
/// it with another, and a right click takes up half a stack or puts down one of the held stack, as
/// Minecraft's clicks do, in the inventory and in the grid alike. A click on what the grid makes
/// takes it up, using one item of each of the grid's stacks, onto the held stack where it is of the
/// same item and has room. What is held and what the grid holds go back into the inventory when the
/// window closes.
/// <para>
/// The quick moves are Minecraft's too, as <see cref="SlotMoves"/> makes them. Shift with a click
/// moves a slot's stack between the hotbar and the rest of the inventory, into the furnace open, or
/// out of a grid or a furnace into the inventory, and on what the grid makes crafts as many as the
/// inventory has room for. A number key over a slot swaps it with that slot of the hotbar, a
/// double-click with a stack held gathers its kind onto it, a drag over slots with a stack held
/// spreads it over them, evenly with the left button and one each with the right, and Q over a slot
/// drops one of its items, with Control its stack. A middle click on the window or R sorts the
/// inventory's 27 slots above the hotbar, each kind together and its stacks filled, as the sorting
/// mods of Minecraft do.
/// </para>
/// </remarks>
public sealed class InventoryWindow
{
    private const float Slot = 40;

    /// <summary>The inventory's own crafting grid, two by two.</summary>
    public CraftingGrid Small { get; } = new(2);

    /// <summary>A crafting table's grid, three by three.</summary>
    public CraftingGrid Table { get; } = new(3);

    /// <summary>The stack taken up and following the mouse, or null.</summary>
    public ItemStack? Held { get; private set; }

    /// <summary>The stacks Q and Control with Q dropped from the slot under the mouse, which the game throws and clears.</summary>
    public List<ItemStack> Dropped { get; } = [];

    // What the window shows this frame, for a shift-click to know where a stack goes.
    private Inventory? _inventory;
    private CraftingGrid? _grid;
    private Furnace? _furnace;

    // The slots a drag with a stack held has passed, from the one it was pressed on, and its button.
    private readonly List<SlotAt> _dragged = [];
    private ImGuiMouseButton _dragButton;
    private bool _dragging;

    /// <summary>Draws the inventory's slots under a crafting grid, the chosen hotbar slot outlined.</summary>
    /// <returns>Whether the window stays open.</returns>
    public bool DrawInventory(Inventory inventory, int selected, CraftingGrid grid)
    {
        (_inventory, _grid, _furnace) = (inventory, grid, null);
        Center();
        var open = true;
        if (ImGui.Begin(grid.Size == 3 ? "Crafting Table" : "Inventory", ref open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse))
        {
            DrawGrid(grid);
            ImGui.Separator();
            InventorySlots(inventory, selected);
        }
        ImGui.End();
        return open;
    }

    // The grid's slots, an arrow, and the slot of what they make.
    private void DrawGrid(CraftingGrid grid)
    {
        ImGui.BeginGroup();
        for (int row = 0; row < grid.Size; row++)
            for (int column = 0; column < grid.Size; column++)
            {
                if (column > 0) ImGui.SameLine();
                ImGui.PushID(1000 + grid.Size * 100);
                SlotButton(grid.Slots, row * grid.Size + column, false);
                ImGui.PopID();
            }
        ImGui.EndGroup();
        ImGui.SameLine();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (grid.Size * (Slot + ImGui.GetStyle().ItemSpacing.Y) - Slot) / 2);
        ImGui.TextUnformatted("  ->  ");
        ImGui.SameLine();
        var made = grid.Recipe is { } recipe ? new ItemStack(recipe.Result, recipe.Count) : (ItemStack?)null;
        ImGui.PushID("made");
        ImGui.ColorButton("##made", Empty, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoDragDrop, new Vector2(Slot));
        var clicked = ImGui.IsItemClicked(ImGuiMouseButton.Left);
        if (made is { } shown)
        {
            DrawStack(ImGui.GetWindowDrawList(), ImGui.GetItemRectMin(), shown);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(shown.Item.Name);
        }
        ImGui.PopID();
        if (!clicked || made is not { } result) return;
        if (Shift() && _inventory is { } inventory)
        {
            // As many as the inventory has room for, each into it as a stack out of a container goes.
            var into = SlotMoves.FromContainer(inventory);
            while (grid.Recipe is { } next && SlotMoves.Room(into, next.Result) >= next.Count && grid.Craft() is { } crafted)
                SlotMoves.MoveInto(new SlotAt([crafted], 0), into);
            return;
        }
        if (Held is { } held && (held.Item != result.Item || held.Count + result.Count > held.Item.MostInStack)) return;
        grid.Craft();
        Held = new ItemStack(result.Item, (Held?.Count ?? 0) + result.Count);
    }

    /// <summary>
    /// Draws a furnace's slots, what it smelts over its fuel with the fire's and the smelting's bars
    /// beside them and what it made, over the inventory's slots, the chosen hotbar slot outlined. What
    /// it made is taken up by a click and never put into.
    /// </summary>
    /// <returns>Whether the window stays open.</returns>
    public bool DrawFurnace(Inventory inventory, int selected, Furnace furnace)
    {
        (_inventory, _grid, _furnace) = (inventory, null, furnace);
        Center();
        var open = true;
        if (ImGui.Begin("Furnace", ref open, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse))
        {
            ImGui.PushID("furnace");
            ImGui.BeginGroup();
            SlotButton(furnace.Slots, Furnace.Input, false);
            ImGui.ProgressBar(furnace.BurnTotal > 0 ? furnace.Burning / furnace.BurnTotal : 0, new Vector2(Slot, 6), "");
            SlotButton(furnace.Slots, Furnace.Fuel, false);
            ImGui.EndGroup();
            ImGui.SameLine();
            ImGui.BeginGroup();
            ImGui.Dummy(new Vector2(1, Slot / 2));
            ImGui.ProgressBar(furnace.Cooked / Smelting.Seconds, new Vector2(Slot * 1.5f, 8), "");
            ImGui.EndGroup();
            ImGui.SameLine();
            ImGui.BeginGroup();
            ImGui.Dummy(new Vector2(1, Slot / 2 - 4));
            TakeOnly(furnace.Slots, Furnace.Output);
            ImGui.EndGroup();
            ImGui.PopID();
            ImGui.Separator();
            InventorySlots(inventory, selected);
        }
        ImGui.End();
        return open;
    }

    // A slot whose stack a click takes up, onto the held stack where it is of the same item and has
    // room, as a furnace's output is taken.
    private void TakeOnly(ItemStack?[] slots, int slot)
    {
        ImGui.PushID(slot);
        var stack = slots[slot];
        ImGui.ColorButton("##slot", Empty, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoDragDrop, new Vector2(Slot));
        var clicked = ImGui.IsItemClicked(ImGuiMouseButton.Left);
        if (stack is { } shown)
        {
            DrawStack(ImGui.GetWindowDrawList(), ImGui.GetItemRectMin(), shown);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(shown.Item.Name);
        }
        ImGui.PopID();
        if (!clicked || stack is not { } taken) return;
        if (Shift() && _inventory is { } inventory)
        {
            SlotMoves.MoveInto(new SlotAt(slots, slot), SlotMoves.FromContainer(inventory));
            return;
        }
        if (Held is not { } held) (Held, slots[slot]) = (taken, null);
        else if (held.Item == taken.Item && held.Count < held.Item.MostInStack)
        {
            var moved = Math.Min(taken.Count, held.Item.MostInStack - held.Count);
            Held = held with { Count = held.Count + moved };
            slots[slot] = taken.Count > moved ? taken with { Count = taken.Count - moved } : null;
        }
    }

    // The inventory's three rows of nine and the hotbar's below them, as every window but the creative one shows them.
    private void InventorySlots(Inventory inventory, int selected)
    {
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < Hotbar.Size; column++)
            {
                if (column > 0) ImGui.SameLine();
                SlotButton(inventory.Slots, Hotbar.Size + row * Hotbar.Size + column, false);
            }
        ImGui.Separator();
        for (int column = 0; column < Hotbar.Size; column++)
        {
            if (column > 0) ImGui.SameLine();
            SlotButton(inventory.Slots, column, column == selected);
        }
        if (Held is { } held) DrawStack(ImGui.GetForegroundDrawList(), ImGui.GetIO().MousePos - new Vector2(Slot / 2), held);
        Release();
        if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Middle) || E3D.IsKeyPressed(Key.R))
            SlotMoves.Sort(inventory.Slots, Hotbar.Size, Inventory.Size);
    }

    // A press on a slot: a shift-click's move, a double-click's gathering with a stack held, a click
    // or a right click with none held, or the start of a drag with one held, which its release ends.
    private void Press(SlotAt at, ImGuiMouseButton button)
    {
        if (button == ImGuiMouseButton.Left && Shift())
        {
            QuickMove(at);
            return;
        }
        if (button == ImGuiMouseButton.Left && Held is { } held && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            Held = SlotMoves.Gather(held, Gatherable());
            return;
        }
        if (Held is null)
        {
            if (button == ImGuiMouseButton.Left) Click(at.Slots, at.Index);
            else RightClick(at.Slots, at.Index);
            return;
        }
        (_dragging, _dragButton) = (true, button);
        _dragged.Clear();
        _dragged.Add(at);
    }

    // A drag let go, which over one slot is a click or a right click there, and over more spreads the
    // held stack over them.
    private void Release()
    {
        if (!_dragging || !ImGui.IsMouseReleased(_dragButton)) return;
        _dragging = false;
        if (Held is { } held)
        {
            var right = _dragButton == ImGuiMouseButton.Right;
            if (_dragged.Count > 1) Held = SlotMoves.Spread(held, _dragged, one: right);
            else if (right) RightClick(_dragged[0].Slots, _dragged[0].Index);
            else Click(_dragged[0].Slots, _dragged[0].Index);
        }
        _dragged.Clear();
    }

    // The keys over a slot: a number swaps it with that slot of the hotbar, and Q drops one of its
    // items, with Control its stack.
    private void Keys(SlotAt at)
    {
        if (_inventory is not { } inventory) return;
        for (int n = 0; n < Hotbar.Size; n++)
            if (E3D.IsKeyPressed(Key.One + n) && !(at.Slots == inventory.Slots && at.Index == n))
                (at.Slots[at.Index], inventory.Slots[n]) = (inventory.Slots[n], at.Slots[at.Index]);
        if (!E3D.IsKeyPressed(Key.Q) || at.Stack is not { } stack) return;
        var count = E3D.IsKeyDown(Key.LeftControl) || E3D.IsKeyDown(Key.RightControl) ? stack.Count : 1;
        Dropped.Add(stack with { Count = count });
        at.Slots[at.Index] = stack.Count > count ? stack with { Count = stack.Count - count } : null;
    }

    // The keys are read through the engine, as the rest of the game reads them, which a key sent
    // through ./e3d reaches as one pressed at the keyboard does, where ImGui's own key state saw no
    // Shift, number, Q or R sent that way.
    private static bool Shift() => E3D.IsKeyDown(Key.LeftShift) || E3D.IsKeyDown(Key.RightShift);

    private void QuickMove(SlotAt at)
    {
        if (_inventory is not { } inventory) return;
        if (at.Slots == inventory.Slots) SlotMoves.QuickMove(inventory, at.Index, _furnace);
        else SlotMoves.MoveInto(at, SlotMoves.FromContainer(inventory));
    }

    // The slots a double-click gathers from, every one the window shows but what a grid or a furnace makes.
    private List<SlotAt> Gatherable()
    {
        var slots = new List<SlotAt>();
        if (_grid is { } grid) slots.AddRange(SlotMoves.Range(grid.Slots, 0, grid.Slots.Length));
        if (_furnace is { } furnace) slots.AddRange([new SlotAt(furnace.Slots, Furnace.Input), new SlotAt(furnace.Slots, Furnace.Fuel)]);
        if (_inventory is { } inventory) slots.AddRange(SlotMoves.Range(inventory.Slots, 0, Inventory.Size));
        return slots;
    }

    /// <summary>Draws every item as a swatch, which puts one of it into the hotbar's chosen slot when clicked.</summary>
    /// <returns>Whether the window stays open.</returns>
    public bool DrawCreative(Hotbar hotbar)
    {
        Center();
        var open = true;
        if (ImGui.Begin("Items", ref open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            var column = 0;
            foreach (var item in Items.All)
            {
                if (ImGui.ColorButton(item.Key, Empty, ImGuiColorEditFlags.NoTooltip, new Vector2(Slot)))
                    hotbar.Inventory.Slots[hotbar.Selected] = new ItemStack(item, 1);
                Icon(ImGui.GetWindowDrawList(), ImGui.GetItemRectMin(), item);
                if (ImGui.IsItemHovered()) ImGui.SetTooltip(item.Name);
                if (++column % 10 != 0) ImGui.SameLine();
            }
            ImGui.NewLine();
            ImGui.TextUnformatted($"Chosen slot: {hotbar.Selected + 1}, {hotbar.Current?.Item.Name ?? "empty"}");
        }
        ImGui.End();
        return open;
    }

    /// <summary>Puts the stack held and the grids' stacks back into the inventory as the window closes.</summary>
    /// <returns>What did not fit, which the caller drops.</returns>
    public List<ItemStack> Close(Inventory inventory)
    {
        var back = Small.Empty();
        back.AddRange(Table.Empty());
        if (Held is { } held) back.Add(held);
        Held = null;
        var left = new List<ItemStack>();
        foreach (var stack in back)
            if (inventory.Add(stack.Item, stack.Count, stack.Wear) is > 0 and var over) left.Add(stack with { Count = over });
        return left;
    }

    private static readonly Vector4 Empty = new(0.16f, 0.16f, 0.18f, 1);

    private static void Center() =>
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

    private void SlotButton(ItemStack?[] slots, int slot, bool chosen)
    {
        ImGui.PushID(slot);
        var stack = slots[slot];
        ImGui.ColorButton("##slot", Empty, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoDragDrop, new Vector2(Slot));
        var left = ImGui.IsItemClicked(ImGuiMouseButton.Left);
        var right = ImGui.IsItemClicked(ImGuiMouseButton.Right);
        var list = ImGui.GetWindowDrawList();
        var min = ImGui.GetItemRectMin();
        if (stack is { } shown)
        {
            DrawStack(list, min, shown);
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(shown.Item.Durability > 0 ? $"{shown.Item.Name}, {shown.Item.Durability - shown.Wear} uses left" : shown.Item.Name);
        }
        var max = ImGui.GetItemRectMax();
        if (chosen) list.AddRect(min - Vector2.One, max + Vector2.One, 0xFFFFFFFF, 0, ImDrawFlags.None, 2);
        var at = new SlotAt(slots, slot);
        // While a drag holds its button, the slot it was pressed on stays ImGui's active item, so the
        // slots it passes are found by the mouse over their squares.
        var under = ImGui.IsMouseHoveringRect(min, max);
        if (left) Press(at, ImGuiMouseButton.Left);
        else if (right) Press(at, ImGuiMouseButton.Right);
        else if (_dragging && under && ImGui.IsMouseDown(_dragButton) && !_dragged.Contains(at)) _dragged.Add(at);
        if (under) Keys(at);
        ImGui.PopID();
    }

    // Takes up a stack, puts the held one down, joins it to a stack of its item, or swaps the two.
    private void Click(ItemStack?[] slots, int slot)
    {
        var there = slots[slot];
        if (Held is { } held && there is { } stack && stack.Item == held.Item && stack.Count < stack.Item.MostInStack)
        {
            var moved = Math.Min(held.Count, held.Item.MostInStack - stack.Count);
            slots[slot] = stack with { Count = stack.Count + moved };
            Held = held.Count > moved ? held with { Count = held.Count - moved } : null;
            return;
        }
        (Held, slots[slot]) = (there, Held);
    }

    // Takes up half a stack, the larger half, or puts down one of the held stack.
    private void RightClick(ItemStack?[] slots, int slot)
    {
        var there = slots[slot];
        if (Held is not { } held)
        {
            if (there is not { } stack) return;
            var half = (stack.Count + 1) / 2;
            Held = stack with { Count = half };
            slots[slot] = stack.Count > half ? stack with { Count = stack.Count - half } : null;
            return;
        }
        if (there is { } other && (other.Item != held.Item || other.Count >= other.Item.MostInStack)) return;
        slots[slot] = held with { Count = (there?.Count ?? 0) + 1 };
        Held = held.Count > 1 ? held with { Count = held.Count - 1 } : null;
    }

    // A stack drawn over its slot, its icon, a worn tool's bar of uses left, and the count where it
    // is more than one.
    private static void DrawStack(ImDrawListPtr list, Vector2 min, ItemStack stack)
    {
        Icon(list, min, stack.Item);
        if (stack.Wear > 0 && stack.Item.Durability > 0)
        {
            var left = 1 - (float)stack.Wear / stack.Item.Durability;
            list.AddRectFilled(min + new Vector2(4, Slot - 6), min + new Vector2(Slot - 4, Slot - 3), 0xFF000000);
            list.AddRectFilled(min + new Vector2(4, Slot - 6), min + new Vector2(4 + (Slot - 8) * left, Slot - 3), ImGui.GetColorU32(new Vector4(1 - left, left, 0.1f, 1)));
        }
        if (stack.Count <= 1) return;
        var text = stack.Count.ToString();
        var at = min + new Vector2(Slot - 4, Slot - 2) - ImGui.CalcTextSize(text);
        list.AddText(at + Vector2.One, 0xC0000000, text);
        list.AddText(at, 0xFFFFFFFF, text);
    }

    // An item's icon at twice its size in the middle of its slot, its part of the icons' atlas.
    private static void Icon(ImDrawListPtr list, Vector2 min, Item item)
    {
        var corner = min + new Vector2((Slot - Icons.Size * 2) / 2);
        var source = Icons.Source(item);
        var atlas = new Vector2(Icons.Width, Icons.Height);
        list.AddImage(E3D.GetImGuiTextureId(Icons.Texture()), corner, corner + new Vector2(Icons.Size * 2),
            new Vector2(source.X, source.Y) / atlas, new Vector2(source.X + source.Width, source.Y + source.Height) / atlas);
    }
}
