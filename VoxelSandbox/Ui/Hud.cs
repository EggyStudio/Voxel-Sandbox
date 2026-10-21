using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>The crosshair, the hotbar and the outline of the block looked at, drawn over the world.</summary>
public static class Hud
{
    /// <summary>
    /// Outlines a block a hair larger than it, so its edges are not lost in its faces' depth, and a
    /// block smaller than its cell by the edges of its pieces together, as a stair's step and slab
    /// are outlined as one shape. Called inside <c>BeginMode3D</c>.
    /// </summary>
    public static void DrawOutline(BlockHit hit, BlockShape? shape)
    {
        var cell = new Vector3(hit.X, hit.Y, hit.Z);
        var color = new Color(0, 0, 0, 220);
        if (shape is null)
        {
            DrawCubeWires(cell + new Vector3(0.5f), 1.004f, 1.004f, 1.004f, color);
            return;
        }
        // Each edge drawn a little out from the shape's middle, as the whole block's is drawn larger.
        var middle = (shape.Min + shape.Max) / 2;
        foreach (var (from, to) in shape.Edges)
            DrawLine3D(cell + middle + (from - middle) * 1.004f, cell + middle + (to - middle) * 1.004f, color);
    }

    /// <summary>
    /// Darkens a block being broken in survival over its pieces, more the further it is broken, as
    /// Minecraft's cracks spread. Called inside <c>BeginMode3D</c>.
    /// </summary>
    public static void DrawBreaking(BlockHit hit, BlockShape? shape, float progress)
    {
        var color = new Color(20, 16, 12, (byte)(15 + 95 * Math.Clamp(progress, 0, 1)));
        foreach (var box in shape?.Boxes ?? [Box.Cell])
        {
            var size = box.Max - box.Min + new Vector3(0.006f);
            DrawCube(new Vector3(hit.X, hit.Y, hit.Z) + (box.Min + box.Max) / 2, size.X, size.Y, size.Z, color);
        }
    }

    public static void DrawCrosshair()
    {
        int cx = GetScreenWidth() / 2, cy = GetScreenHeight() / 2;
        // A pack's crosshair is fifteen of the interface's pixels across, centered as Minecraft's is.
        if (PackGui.Get("sprites/hud/crosshair") is { } crosshair)
        {
            var s = Gui.Scale;
            PackGui.Draw(crosshair, new Rectangle(cx - 7 * s, cy - 7 * s, 15 * s, 15 * s), s);
            return;
        }
        var shadow = new Color(0, 0, 0, 120);
        DrawRectangle(cx - 10, cy - 2, 20, 4, shadow);
        DrawRectangle(cx - 2, cy - 10, 4, 20, shadow);
        DrawRectangle(cx - 9, cy - 1, 18, 2, Color.White);
        DrawRectangle(cx - 1, cy - 9, 2, 18, Color.White);
    }

    /// <summary>
    /// The hotbar at the bottom of the window as Minecraft lays it out, the chosen slot framed, and
    /// over it the name of what the chosen slot holds, above the hearts and food where they show.
    /// </summary>
    public static void DrawHotbar(Hotbar hotbar, bool vitals = false)
    {
        var s = Gui.Scale;
        int x0 = (GetScreenWidth() - GuiArt.Hotbar.Width * s) / 2, y = GetScreenHeight() - GuiArt.Hotbar.Height * s;
        Gui.Sprite(GuiArt.Hotbar, x0, y);
        for (int i = 0; i < Hotbar.Size; i++)
            if (hotbar.Inventory.Slots[i] is { } stack) Gui.Item(stack, x0 + (3 + 20 * i) * s, y + 3 * s);
        Gui.Sprite(GuiArt.Selection, x0 + (20 * hotbar.Selected - 1) * s, y - s);

        if (hotbar.Current is not { } held) return;
        var name = held.Item.Name;
        Gui.Text(name, (GetScreenWidth() - Gui.TextWidth(name)) / 2, GetScreenHeight() - (vitals ? 59 : 45) * s, Color.White);
    }

    /// <summary>
    /// Ten hearts of health over the hotbar's left half and ten drumsticks of food over its right,
    /// each worth two points and half drawn for one over the empty place, as Minecraft's are, and
    /// while breath runs short, bubbles of it over the food, each row as much as
    /// <paramref name="fade"/> shows of it.
    /// </summary>
    public static void DrawVitals(Vitals vitals, VitalsFade fade)
    {
        var s = Gui.Scale;
        int x0 = (GetScreenWidth() - GuiArt.Hotbar.Width * s) / 2, y = GetScreenHeight() - 39 * s;
        var right = x0 + GuiArt.Hotbar.Width * s - GuiArt.HeartFull.Width * s;
        Color Faded(float shown) => new(255, 255, 255, (byte)(255 * shown));
        for (int i = 0; i < 10; i++)
        {
            if (fade.Hearts > 0)
            {
                var health = Math.Clamp(vitals.Health - i * 2, 0, 2);
                Gui.Sprite(GuiArt.HeartEmpty, x0 + i * 8 * s, y, Faded(fade.Hearts));
                if (health > 0) Gui.Sprite(health == 2 ? GuiArt.HeartFull : GuiArt.HeartHalf, x0 + i * 8 * s, y, Faded(fade.Hearts));
            }
            // Food from the right, as Minecraft's is drawn.
            if (fade.Food > 0)
            {
                var food = Math.Clamp(vitals.Food - i * 2, 0, 2);
                Gui.Sprite(GuiArt.FoodEmpty, right - i * 8 * s, y, Faded(fade.Food));
                if (food > 0) Gui.Sprite(food == 2 ? GuiArt.FoodFull : GuiArt.FoodHalf, right - i * 8 * s, y, Faded(fade.Food));
            }
            if (fade.Air > 0 && i < (int)MathF.Ceiling(vitals.Air / Vitals.Breath * 10))
                Gui.Sprite(GuiArt.Air, right - i * 8 * s, y - 10 * s, Faded(fade.Air));
        }
    }

    /// <summary>A bar under the crosshair, filling as the food held is eaten.</summary>
    public static void DrawEating(float progress)
    {
        int cx = GetScreenWidth() / 2, cy = GetScreenHeight() / 2 + 16;
        DrawRectangle(cx - 20, cy, 40, 4, new Color(0, 0, 0, 140));
        DrawRectangle(cx - 20, cy, (int)(40 * Math.Clamp(progress, 0, 1)), 4, new Color(196, 120, 58));
    }

    /// <summary>A line of help at the top left, while the settings window is closed, with no inventory or throw in spectator, where nothing is held.</summary>
    public static void DrawHelp(GlobalIllumination light, GameMode mode)
    {
        var holding = !mode.Touches() ? "" : $"E {(mode.Lives() ? "inventory" : "items")}   Q throw   ";
        var text = $"F3 settings   F3+F4 mode   {holding}G light: {light}   F1 hide   {mode}";
        DrawText(text, 11, 11, 10, new Color(0, 0, 0, 160));
        DrawText(text, 10, 10, 10, Color.White);
    }
}
