using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The window E opens, which shows in survival the inventory's 36 slots under a crafting grid of
/// two by two, in creative every item to put in the hotbar's chosen slot, at a crafting table the
/// inventory under a grid of three by three in either mode, and at a furnace the inventory under
/// the furnace's slots, each drawn as the game's own panel at Minecraft's places, or as the window's
/// texture of the player's resource packs where one has it.
/// </summary>
/// <remarks>
/// The slots are laid out as Minecraft's are, three rows of nine and the hotbar's below them. A left
/// click takes up a slot's stack, puts the stack held down, joins it to a stack of its item or swaps
/// it with another, and a right click takes up half a stack or puts down one of the held stack, as
/// Minecraft's clicks do, in the inventory and in the grid alike. A click on what the grid makes
/// takes it up, using one item of each of the grid's stacks, onto the held stack where it is of the
/// same item and has room. What is held and what the grid holds go back into the inventory when the
/// window closes. The slot under the pointer is lit and its item named in a tooltip, and the stack
/// held follows the pointer.
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
    // Minecraft's panel of a container, 176 by 166 of the interface's pixels, and the creative one.
    private const int Width = 176, Height = 166, CreativeWidth = 195, CreativeHeight = 136, CreativeRows = 5;
    // A double-click is a second press on the same slot within this many seconds, as Minecraft's.
    private const double DoubleClick = 0.25;

    // A slot of the window: where it is in the interface's pixels from the panel's corner, and whether
    // it is one a click only takes from, as a furnace's output, or what a grid makes.
    private readonly record struct SlotView(SlotAt At, int X, int Y, bool TakeOnly = false, CraftingGrid? Makes = null);

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
    private Chest? _chest;

    // The slots a drag with a stack held has passed, from the one it was pressed on, and its button.
    private readonly List<SlotAt> _dragged = [];
    private MouseButton _dragButton;
    private bool _dragging;
    private (double Time, SlotAt? At) _lastPress = (double.NegativeInfinity, null);
    // How many rows of the creative window's items are scrolled past.
    private int _scroll;

    /// <summary>Draws the inventory's slots under a crafting grid.</summary>
    /// <returns>Whether the window stays open, which it does until E or Escape shuts it.</returns>
    public bool DrawInventory(Inventory inventory, CraftingGrid grid)
    {
        (_inventory, _grid, _furnace, _chest) = (inventory, grid, null, null);
        var table = grid.Size == 3;
        List<SlotView> slots = [];
        // Minecraft's inventory puts its grid of two at the upper right with what it makes beside it,
        // and a crafting table its grid of three at the left with what it makes at the right.
        int gridX = table ? 30 : 98, gridY = table ? 17 : 18, madeX = table ? 124 : 154, madeY = table ? 35 : 28;
        for (int i = 0; i < grid.Slots.Length; i++)
            slots.Add(new SlotView(new SlotAt(grid.Slots, i), gridX + i % grid.Size * 18, gridY + i / grid.Size * 18));
        slots.Add(new SlotView(default, madeX, madeY, Makes: grid));
        AddInventory(slots, inventory);
        // A pack's window holds its arrow.
        Window(slots, inventory, table ? "crafting_table" : "inventory", (left, top, s, packed) =>
        {
            if (!packed) Gui.Sprite(GuiArt.Arrow, left + (table ? 90 : 130) * s, top + (table ? 35 : 28) * s);
        });
        return true;
    }

    /// <summary>Draws a chest's three rows of nine slots over the inventory's, as the source's chest lays them out.</summary>
    /// <returns>Whether the window stays open, which it does until E or Escape shuts it.</returns>
    public bool DrawChest(Inventory inventory, Chest chest)
    {
        (_inventory, _grid, _furnace, _chest) = (inventory, null, null, chest);
        List<SlotView> slots = [];
        for (int i = 0; i < Chest.Size; i++)
            slots.Add(new SlotView(new SlotAt(chest.Slots, i), 8 + i % Hotbar.Size * 18, 18 + i / Hotbar.Size * 18));
        // The source's chest of three rows puts the inventory a pixel lower than the furnace's window.
        AddInventory(slots, inventory, 85);
        Window(slots, inventory, "generic_54", (_, _, _, _) => { }, ChestRows);
        return true;
    }

    // The rows of a chest, whose window the source cuts from its picture of six rows, the top and
    // the rows' part of it and then its inventory's part, so the window is 17 and 18 a row high over
    // the 96 of the inventory.
    private const int ChestRows = 3;

    /// <summary>Draws a furnace's slots, what it smelts over its fuel with the fire's flame and the smelting's arrow between them and what it made, over the inventory's.</summary>
    /// <returns>Whether the window stays open, which it does until E or Escape shuts it.</returns>
    public bool DrawFurnace(Inventory inventory, Furnace furnace)
    {
        (_inventory, _grid, _furnace, _chest) = (inventory, null, furnace, null);
        List<SlotView> slots =
        [
            new(new SlotAt(furnace.Slots, Furnace.Input), 56, 17),
            new(new SlotAt(furnace.Slots, Furnace.Fuel), 56, 53),
            new(new SlotAt(furnace.Slots, Furnace.Output), 116, 35, TakeOnly: true),
        ];
        AddInventory(slots, inventory);
        Window(slots, inventory, "furnace", (left, top, s, packed) =>
        {
            // The flame burns down from its top as the fuel burns out, and the arrow fills from its
            // left as the smelting goes on, as Minecraft's furnace shows them.
            var burnt = furnace.BurnTotal > 0 ? furnace.Burning / furnace.BurnTotal : 0;
            var lit = PackGui.Get("sprites/container/furnace/lit_progress");
            var burn = PackGui.Get("sprites/container/furnace/burn_progress");
            if (packed && lit is { } flameSprite && burn is { } arrowSprite)
            {
                // A pack's furnace holds the empty flame and arrow, over which its sprites are drawn
                // where and as far as Minecraft draws them, the flame a pixel high at least while it burns.
                var high = furnace.Burning > 0 ? (int)MathF.Ceiling(burnt * 13) + 1 : 0;
                PackGui.DrawPart(flameSprite, 14, 14, 0, 14 - high, 14, high, left + 56 * s, top + (50 - high) * s, s);
                PackGui.DrawPart(arrowSprite, 24, 16, 0, 0, (int)MathF.Ceiling(furnace.Cooked / Smelting.Seconds * 24), 16, left + 79 * s, top + 34 * s, s);
                return;
            }
            Gui.Sprite(GuiArt.Flame, left + 57 * s, top + 37 * s);
            var flame = (int)MathF.Ceiling(burnt * GuiArt.FlameFull.Height);
            Gui.SpritePart(GuiArt.FlameFull, left + 57 * s, top + 37 * s, GuiArt.FlameFull.Width, flame, GuiArt.FlameFull.Height - flame);
            Gui.Sprite(GuiArt.Arrow, left + 79 * s, top + 35 * s);
            Gui.SpritePart(GuiArt.ArrowFull, left + 79 * s, top + 35 * s, (int)(furnace.Cooked / Smelting.Seconds * GuiArt.ArrowFull.Width), GuiArt.ArrowFull.Height);
        });
        return true;
    }

    /// <summary>
    /// Draws every item in rows of nine, which the wheel scrolls, one of which a click puts into the
    /// hotbar's chosen slot, over the hotbar, whose slot a click chooses.
    /// </summary>
    /// <returns>Whether the window stays open, which it does until E or Escape shuts it.</returns>
    public bool DrawCreative(Hotbar hotbar)
    {
        (_inventory, _grid, _furnace, _chest) = (null, null, null, null);
        var s = Gui.Scale;
        int left = (GetScreenWidth() - CreativeWidth * s) / 2, top = (GetScreenHeight() - CreativeHeight * s) / 2;
        var rows = (Items.All.Count + Hotbar.Size - 1) / Hotbar.Size;
        Gui.Backdrop();
        if (Gui.Over(new Rectangle(left, top, CreativeWidth * s, CreativeHeight * s)))
            _scroll = Math.Clamp(_scroll - Math.Sign(GetMouseWheelMove()), 0, Math.Max(0, rows - CreativeRows));

        // A pack's creative window holds its slots and its scroll bar's track.
        var pack = PackGui.Container("creative_inventory/tab_items");
        if (pack is { } window) PackGui.DrawPart(window, 256, 256, 0, 0, CreativeWidth, CreativeHeight, left, top, s);
        else Gui.Stretched(GuiArt.Panel, new Rectangle(left, top, CreativeWidth * s, CreativeHeight * s));
        Item? hovered = null;
        for (int row = 0; row < CreativeRows; row++)
            for (int column = 0; column < Hotbar.Size; column++)
            {
                var index = (row + _scroll) * Hotbar.Size + column;
                int x = left + (9 + column * 18) * s, y = top + (18 + row * 18) * s;
                if (pack is null) Gui.Sprite(GuiArt.Slot, x - s, y - s);
                if (index >= Items.All.Count) continue;
                var item = Items.All[index];
                Gui.Item(new ItemStack(item, 1), x, y);
                if (!Gui.Over(new Rectangle(x, y, 16 * s, 16 * s))) continue;
                Lit(x, y);
                hovered = item;
                if (IsMouseButtonPressed(MouseButton.Left)) hotbar.Inventory.Slots[hotbar.Selected] = new ItemStack(item, 1);
            }
        // The scroll bar beside the rows, its thumb as far down as the rows are scrolled.
        int barX = left + 175 * s, barY = top + 18 * s, barHeight = CreativeRows * 18 * s - 2 * s;
        if (pack is null) DrawRectangle(barX, barY, 12 * s, barHeight, new Color(139, 139, 139));
        var thumb = rows > CreativeRows ? (int)((float)_scroll / (rows - CreativeRows) * (barHeight - 15 * s)) : 0;
        if (PackGui.Get("sprites/container/creative_inventory/scroller") is { } scroller) PackGui.Draw(scroller, new Rectangle(barX, barY + thumb, 12 * s, 15 * s), s);
        else Gui.Stretched(GuiArt.Button, new Rectangle(barX, barY + thumb, 12 * s, 15 * s));

        for (int i = 0; i < Hotbar.Size; i++)
        {
            int x = left + (9 + i * 18) * s, y = top + 112 * s;
            if (pack is null) Gui.Sprite(GuiArt.Slot, x - s, y - s);
            if (hotbar.Inventory.Slots[i] is { } stack) Gui.Item(stack, x, y);
            if (i == hotbar.Selected) DrawRectangleLinesEx(new Rectangle(x - s, y - s, 18 * s, 18 * s), s, Color.White);
            if (!Gui.Over(new Rectangle(x, y, 16 * s, 16 * s))) continue;
            Lit(x, y);
            if (hotbar.Inventory.Slots[i] is { } named) hovered = named.Item;
            if (IsMouseButtonPressed(MouseButton.Left)) hotbar.Selected = i;
        }
        if (hovered is { } shown) Gui.Tooltip([shown.Name]);
        return true;
    }

    /// <summary>Puts the stack held and the grids' stacks back into the inventory as the window closes.</summary>
    /// <returns>What did not fit, which the caller drops.</returns>
    public List<ItemStack> Close(Inventory inventory)
    {
        var back = Small.Empty();
        back.AddRange(Table.Empty());
        if (Held is { } held) back.Add(held);
        Held = null;
        (_dragging, _scroll) = (false, 0);
        _dragged.Clear();
        var left = new List<ItemStack>();
        foreach (var stack in back)
            if (inventory.Add(stack.Item, stack.Count, stack.Wear) is > 0 and var over) left.Add(stack with { Count = over });
        return left;
    }

    // The inventory's three rows of nine and the hotbar's below them, at Minecraft's places.
    private static void AddInventory(List<SlotView> slots, Inventory inventory, int top = 84)
    {
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < Hotbar.Size; column++)
                slots.Add(new SlotView(new SlotAt(inventory.Slots, Hotbar.Size + row * Hotbar.Size + column), 8 + column * 18, top + row * 18));
        for (int column = 0; column < Hotbar.Size; column++)
            slots.Add(new SlotView(new SlotAt(inventory.Slots, column), 8 + column * 18, top + 58));
    }

    // The panel, or the window's texture of a pack under the name Minecraft gives it, with no title
    // or label over it, since its slots say what it is, the window's own pictures, told whether the
    // window is the pack's, its slots and their stacks, then the pointer's clicks and keys on them,
    // the tooltip and the stack held. A pack's window holds its slots.
    private void Window(List<SlotView> slots, Inventory inventory, string container, Action<int, int, int, bool> pictures, int rows = 0)
    {
        var s = Gui.Scale;
        int left = (GetScreenWidth() - Width * s) / 2, top = (GetScreenHeight() - Height * s) / 2;
        Gui.Backdrop();
        var pack = PackGui.Container(container);
        if (pack is { } window && rows > 0)
        {
            PackGui.DrawPart(window, 256, 256, 0, 0, Width, 17 + rows * 18, left, top, s);
            PackGui.DrawPart(window, 256, 256, 0, 126, Width, 96, left, top + (17 + rows * 18) * s, s);
        }
        else if (pack is { } whole) PackGui.DrawPart(whole, 256, 256, 0, 0, Width, Height, left, top, s);
        else Gui.Stretched(GuiArt.Panel, new Rectangle(left, top, Width * s, Height * s));
        pictures(left, top, s, pack is not null);

        // A drag over more than one slot with a stack held shows each slot's share in it, the slot
        // lit, and what is left under the pointer, before the button is let go.
        var (shares, kept) = _dragging && _dragged.Count > 1 && Held is { } spread
            ? SlotMoves.Shares(spread, _dragged, one: _dragButton == MouseButton.Right) : ([], Held);
        SlotView? under = null;
        foreach (var slot in slots)
        {
            int x = left + slot.X * s, y = top + slot.Y * s;
            if (pack is null) Gui.Sprite(GuiArt.Slot, x - s, y - s);
            var stack = slot.Makes is { } grid ? grid.Recipe is { } recipe ? new ItemStack(recipe.Result, recipe.Count) : null : slot.At.Stack;
            var share = slot.Makes is null ? shares.FindIndex(p => p.At == slot.At) : -1;
            if (share >= 0) stack = shares[share].Stack;
            if (stack is { } shown) Gui.Item(shown, x, y);
            if (share >= 0) Lit(x, y);
            if (!Gui.Over(new Rectangle(x, y, 16 * s, 16 * s))) continue;
            Lit(x, y);
            under = slot;
        }

        if (under is { } at) Pointer(at, inventory);
        Release();
        if (Gui.Over(new Rectangle(left, top, Width * s, Height * s)) && IsMouseButtonPressed(MouseButton.Middle) || IsKeyPressed(Key.R))
            SlotMoves.Sort(inventory.Slots, Hotbar.Size, Inventory.Size);

        if (Held is not null)
        {
            var mouse = GetMousePosition();
            if (kept is { } held) Gui.Item(held, (int)mouse.X - 8 * s, (int)mouse.Y - 8 * s);
        }
        else if (under is { } named && (named.Makes is { } makes ? makes.Recipe?.Result : named.At.Stack?.Item) is { } item)
        {
            var wear = named.Makes is null ? named.At.Stack?.Wear ?? 0 : 0;
            Gui.Tooltip(item.Durability > 0 ? [item.Name, $"{item.Durability - wear} uses left"] : [item.Name]);
        }
    }

    // The pointer's presses and keys on the slot under it.
    private void Pointer(SlotView slot, Inventory inventory)
    {
        if (slot.Makes is { } grid)
        {
            if (IsMouseButtonPressed(MouseButton.Left)) TakeMade(grid, inventory);
            return;
        }
        if (slot.TakeOnly)
        {
            if (IsMouseButtonPressed(MouseButton.Left)) TakeOnly(slot.At, inventory);
            return;
        }
        if (IsMouseButtonPressed(MouseButton.Left)) Press(slot.At, MouseButton.Left);
        else if (IsMouseButtonPressed(MouseButton.Right)) Press(slot.At, MouseButton.Right);
        else if (_dragging && IsMouseButtonDown(_dragButton) && !_dragged.Contains(slot.At)) _dragged.Add(slot.At);
        Keys(slot.At);
    }

    // A click on what the grid makes takes it up, onto the held stack where it is of the same item
    // and has room, and with Shift crafts as many as the inventory has room for into it.
    private void TakeMade(CraftingGrid grid, Inventory inventory)
    {
        if (grid.Recipe is not { } recipe) return;
        if (Shift())
        {
            var into = SlotMoves.FromContainer(inventory);
            while (grid.Recipe is { } next && SlotMoves.Room(into, next.Result) >= next.Count && grid.Craft() is { } crafted)
                SlotMoves.MoveInto(new SlotAt([crafted], 0), into);
            return;
        }
        if (Held is { } held && (held.Item != recipe.Result || held.Count + recipe.Count > held.Item.MostInStack)) return;
        grid.Craft();
        Held = new ItemStack(recipe.Result, (Held?.Count ?? 0) + recipe.Count);
    }

    // A slot whose stack a click takes up, onto the held stack where it is of the same item and has
    // room, as a furnace's output is taken, and with Shift into the inventory.
    private void TakeOnly(SlotAt at, Inventory inventory)
    {
        if (at.Stack is not { } taken) return;
        if (Shift())
        {
            SlotMoves.MoveInto(at, SlotMoves.FromContainer(inventory));
            return;
        }
        if (Held is not { } held) (Held, at.Slots[at.Index]) = (taken, null);
        else if (held.Item == taken.Item && held.Count < held.Item.MostInStack)
        {
            var moved = Math.Min(taken.Count, held.Item.MostInStack - held.Count);
            Held = held with { Count = held.Count + moved };
            at.Slots[at.Index] = taken.Count > moved ? taken with { Count = taken.Count - moved } : null;
        }
    }

    // A press on a slot: a shift-click's move, a double-click's gathering with a stack held, a click
    // or a right click with none held, or the start of a drag with one held, which its release ends.
    private void Press(SlotAt at, MouseButton button)
    {
        var twice = button == MouseButton.Left && _lastPress.At == at && GetTime() - _lastPress.Time < DoubleClick;
        if (button == MouseButton.Left) _lastPress = (GetTime(), at);
        if (button == MouseButton.Left && Shift())
        {
            QuickMove(at);
            return;
        }
        if (twice && Held is { } held)
        {
            Held = SlotMoves.Gather(held, Gatherable());
            return;
        }
        if (Held is null)
        {
            if (button == MouseButton.Left) Click(at.Slots, at.Index);
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
        if (!_dragging || !IsMouseButtonReleased(_dragButton)) return;
        _dragging = false;
        if (Held is { } held)
        {
            var right = _dragButton == MouseButton.Right;
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
            if (IsKeyPressed(Key.One + n) && !(at.Slots == inventory.Slots && at.Index == n))
                (at.Slots[at.Index], inventory.Slots[n]) = (inventory.Slots[n], at.Slots[at.Index]);
        if (!IsKeyPressed(Key.Q) || at.Stack is not { } stack) return;
        var count = IsKeyDown(Key.LeftControl) || IsKeyDown(Key.RightControl) ? stack.Count : 1;
        Dropped.Add(stack with { Count = count });
        at.Slots[at.Index] = stack.Count > count ? stack with { Count = stack.Count - count } : null;
    }

    private static bool Shift() => IsKeyDown(Key.LeftShift) || IsKeyDown(Key.RightShift);

    private void QuickMove(SlotAt at)
    {
        if (_inventory is not { } inventory) return;
        if (at.Slots == inventory.Slots && _chest is { } chest) SlotMoves.MoveInto(at, SlotMoves.Range(chest.Slots, 0, Chest.Size).ToList());
        else if (at.Slots == inventory.Slots) SlotMoves.QuickMove(inventory, at.Index, _furnace);
        else SlotMoves.MoveInto(at, SlotMoves.FromContainer(inventory));
    }

    // The slots a double-click gathers from, every one the window shows but what a grid or a furnace makes.
    private List<SlotAt> Gatherable()
    {
        var slots = new List<SlotAt>();
        if (_grid is { } grid) slots.AddRange(SlotMoves.Range(grid.Slots, 0, grid.Slots.Length));
        if (_furnace is { } furnace) slots.AddRange([new SlotAt(furnace.Slots, Furnace.Input), new SlotAt(furnace.Slots, Furnace.Fuel)]);
        if (_chest is { } chest) slots.AddRange(SlotMoves.Range(chest.Slots, 0, Chest.Size));
        if (_inventory is { } inventory) slots.AddRange(SlotMoves.Range(inventory.Slots, 0, Inventory.Size));
        return slots;
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

    // The slot under the pointer lit white, as Minecraft lights it.
    private static void Lit(int x, int y)
    {
        var s = Gui.Scale;
        DrawRectangle(x, y, 16 * s, 16 * s, new Color(255, 255, 255, 110));
    }
}
