using System.Globalization;
using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The game's console commands, which <c>./e3d command</c> runs, so a test of the light can be built,
/// walked to and looked at from the terminal without a mouse.
/// </summary>
/// <remarks>Commands run on the main thread at the start of a frame, so they change the game directly.</remarks>
public static class GameCommands
{
    // The most blocks one fill changes, a cube of 64, so a mistyped corner does not stall the frame.
    private const long FillLimit = 64 * 64 * 64;

    /// <summary>The game being played, which <c>./e3d eval</c> reaches too.</summary>
    public static VoxelGame? Game { get; internal set; }

    [Command("voxel.frames", "The median, the 99th part and the slowest of the last 600 frames' times, and the most a frame's placing of the sections took since asked, in ms")]
    internal static string Frames()
    {
        if (Game is not { } game) return "no world is loaded";
        var (median, p99, slowest, count) = game.FrameTimes();
        var peak = game.Renderer.PeakMs;
        game.Renderer.PeakMs = 0;
        return string.Create(CultureInfo.InvariantCulture, $"over {count} frames, median {median:0.00} ms, 99th part {p99:0.00} ms, slowest {slowest:0.00} ms, the sections' placing at most {peak:0.00} ms");
    }

    [Command("voxel.state", "Where the player stands and looks, the block looked at, the columns and sections loaded, the frame's draws, the hour and the light")]
    internal static string State()
    {
        if (Game is not { } game) return "no world is loaded";
        var player = game.Player;
        var at = player.Body.Position;
        var renderer = game.Renderer;
        var target = game.TargetMob is { } mob ? $"looking at {mob.Kind.Name} {mob.Id}"
            : game.Target is { } hit ? $"looking at {Blocks.Get(game.World.GetBlock(hit.X, hit.Y, hit.Z)).Key} at {hit.X}, {hit.Y}, {hit.Z}"
            : "looking at nothing within reach";
        return (game.Loading ? $"loading {game.WorldName}, {game.LoadingShare():0%} done; " : "") + $"feet at {at.X:0.00}, {at.Y:0.00}, {at.Z:0.00} in {game.World.BiomeAt((int)MathF.Floor(at.X), (int)MathF.Floor(at.Z))?.Name ?? "nowhere loaded"}, facing {player.Heading:0} and pitched {player.Pitch * 180 / MathF.PI:0}, "
            + $"{(player.Flying ? "flying" : player.Body.OnGround ? "on the ground" : "in the air")}{EyeInside(game)}, {target}; "
            + $"{game.World.ColumnCount} columns, {game.Streamer.Pending} generating, {game.World.Loaded.Count + game.Renderer.Meshing} sections to mesh; "
            + $"{renderer.Sections} sections, {renderer.Draws} draws, {renderer.Triangles} triangles, {renderer.Lamps} lamps, {renderer.Culled} culled, {renderer.Deep.Sections} deep of {renderer.Deep.Triangles} triangles; "
            + $"{game.Mobs.All.Count} mobs loaded, {game.MobRenderer.Drawn} drawn in {game.MobRenderer.Draws} draws; "
            + $"hour {game.Sky.Hour:0.00} of day {game.Sky.Day}, light {game.Light.Quality}, render distance {game.RenderDistance}, world {game.World.Generator.Name} of seed {game.World.Generator.Seed}, "
            + $"{(game.Save is { } save ? $"saved in {save.Name}" : "transient")}, {game.Mode.ToString().ToLowerInvariant()}, "
            + $"holding {(game.Hotbar.Current is { } held ? $"{held.Count} {held.Item.Key}" : "nothing")}, {game.Drops.Count} stacks on the ground; "
            + Vitals(game.Vitals);
    }

    // Which block the eye's cell holds where it holds anything but air, so a capture taken from inside
    // one is caught before it is taken.
    private static string EyeInside(VoxelGame game)
    {
        var eye = game.Player.Eye;
        var block = game.World.GetBlock((int)MathF.Floor(eye.X), (int)MathF.Floor(eye.Y), (int)MathF.Floor(eye.Z));
        return block == BlockId.Air ? "" : $", the eye inside {Blocks.Get(block).Key}";
    }

    private static string Vitals(Vitals v) =>
        $"health {v.Health}, food {v.Food}, saturation {v.Saturation:0.0}, exhaustion {v.Exhaustion:0.00}, breath {v.Air:0.0} s{(v.Dead ? $", dead, you {v.Cause}" : "")}";

    [Command("voxel.vitals", "Sets the player's health and food from 0 to 20 where given, and says them with the saturation, exhaustion and breath: voxel.vitals [health] [food]")]
    internal static string SetVitals(int health = -1, int food = -1)
    {
        if (Game is not { } game) return "no world is loaded";
        var v = game.Vitals;
        if (health >= 0 || food >= 0) v.Set(health >= 0 ? health : v.Health, food >= 0 ? food : v.Food, v.Saturation);
        return Vitals(v);
    }

    [Command("voxel.hurt", "Takes some health from the player in survival or adventure, as a hit does: voxel.hurt <amount>")]
    internal static string Hurt(int amount)
    {
        if (Game is not { } game) return "no world is loaded";
        if (!game.Mode.Lives()) return "the player is harmed in survival and adventure alone";
        game.Vitals.Damage(amount, "were hurt by a command");
        return Vitals(game.Vitals);
    }

    [Command("voxel.respawn", "Gives the player a new life at the world's spawn with full health and food, as the button after a death does")]
    internal static string Respawn()
    {
        if (Game is not { } game) return "no world is loaded";
        game.Respawn();
        var at = game.Player.Body.Position;
        return $"the player stands at {at.X:0.0}, {at.Y:0.0}, {at.Z:0.0}";
    }

    [Command("voxel.tp", "Puts the player's feet at a place: voxel.tp <x> <y> <z>")]
    internal static string Teleport(float x, float y, float z)
    {
        if (Game is not { } game) return "no world is loaded";
        game.Player.Teleport(new Vector3(x, y, z));
        return $"the player stands at {x}, {y}, {z}";
    }

    [Command("voxel.look", "Turns the player to a heading in degrees clockwise from north (-z) and a pitch in degrees up: voxel.look <heading> <pitch>")]
    internal static string Look(float heading, float pitch)
    {
        if (Game is not { } game) return "no world is loaded";
        game.Player.Heading = heading;
        game.Player.Pitch = Math.Clamp(pitch, -89, 89) * MathF.PI / 180;
        return $"the player faces {heading} and is pitched {pitch}";
    }

    [Command("voxel.fly", "Starts or stops flying: voxel.fly <on>")]
    internal static string Fly(bool on)
    {
        if (Game is not { } game) return "no world is loaded";
        game.Player.Flying = on;
        game.Player.Body.Velocity = Vector3.Zero;
        return on ? "the player flies" : "the player walks";
    }

    [Command("voxel.set", "Places a block by its name or number, air to clear one: voxel.set <x> <y> <z> <block>")]
    internal static string Set(int x, int y, int z, string block)
    {
        if (Game is not { } game) return "no world is loaded";
        if (!Blocks.TryFind(block, out var id)) return $"no block is called {block}, and voxel.blocks lists them";
        return game.World.SetBlock(x, y, z, id) ? $"{Blocks.Get(id).Key} is at {x}, {y}, {z}" : $"nothing changed at {x}, {y}, {z}, which holds that block already or is not loaded";
    }

    [Command("voxel.fill", "Fills a box between two corners with a block, air to clear it: voxel.fill <x1> <y1> <z1> <x2> <y2> <z2> <block>")]
    internal static string Fill(int x1, int y1, int z1, int x2, int y2, int z2, string block) => Box(x1, y1, z1, x2, y2, z2, block, hollow: false);

    [Command("voxel.room", "Builds the walls, floor and ceiling of a box between two corners from a block and clears its inside, a closed room to light: voxel.room <x1> <y1> <z1> <x2> <y2> <z2> <block>")]
    internal static string Room(int x1, int y1, int z1, int x2, int y2, int z2, string block) => Box(x1, y1, z1, x2, y2, z2, block, hollow: true);

    private static string Box(int x1, int y1, int z1, int x2, int y2, int z2, string block, bool hollow)
    {
        if (Game is not { } game) return "no world is loaded";
        if (!Blocks.TryFind(block, out var id)) return $"no block is called {block}, and voxel.blocks lists them";
        (x1, x2) = (Math.Min(x1, x2), Math.Max(x1, x2));
        (y1, y2) = (Math.Min(y1, y2), Math.Max(y1, y2));
        (z1, z2) = (Math.Min(z1, z2), Math.Max(z1, z2));
        var volume = (long)(x2 - x1 + 1) * (y2 - y1 + 1) * (z2 - z1 + 1);
        if (volume > FillLimit) return $"the box holds {volume} blocks, more than the {FillLimit} one fill changes";

        var changed = 0;
        for (int y = y1; y <= y2; y++)
            for (int z = z1; z <= z2; z++)
                for (int x = x1; x <= x2; x++)
                {
                    var shell = x == x1 || x == x2 || y == y1 || y == y2 || z == z1 || z == z2;
                    if (game.World.SetBlock(x, y, z, hollow && !shell ? BlockId.Air : id)) changed++;
                }
        return $"{changed} of {volume} blocks changed";
    }

    [Command("voxel.light", "The sky's and the emissive blocks' light levels from 0 to 15 at a place: voxel.light <x> <y> <z>")]
    internal static string Light(int x, int y, int z)
    {
        if (Game is not { } game) return "no world is loaded";
        var (sky, block) = game.World.GetLight(x, y, z);
        return $"{Blocks.Get(game.World.GetBlock(x, y, z)).Key} at {x}, {y}, {z}, sky {sky}, blocks {block}";
    }

    [Command("voxel.shade", "Whether faces are darkened by light levels and their corners where blocks meet: voxel.shade <levels> <corners>")]
    internal static string Shade(bool levels, bool corners)
    {
        if (Game is not { } game) return "no world is loaded";
        (game.Renderer.LightLevels, game.Renderer.CornerShade) = (levels, corners);
        game.World.ReshadeAll();
        return $"light levels {(levels ? "on" : "off")}, corners shaded {(corners ? "on" : "off")}, every section shaded again over the next frames";
    }

    [Command("voxel.corners", "Sets how far the corners darken where blocks meet, from 0 for not at all to 1 for Minecraft's levels: voxel.corners <strength>")]
    internal static string Corners(float strength)
    {
        if (Game is not { } game) return "no world is loaded";
        game.Renderer.CornerStrength = strength;
        game.World.ReshadeAll();
        return $"corners darken at {game.Renderer.CornerStrength:0.##} of Minecraft's levels, every section shaded again over the next frames";
    }

    [Command("voxel.time", "Sets the hour of the day from 0 to 24, the sun up from 6 to 18: voxel.time <hour>")]
    internal static string Time(float hour)
    {
        if (Game is not { } game) return "no world is loaded";
        game.Sky.Hour = (hour % 24 + 24) % 24;
        return $"the hour is {game.Sky.Hour:0.00}";
    }

    [Command("voxel.day", "Sets the count of days, which the moon's phase follows, its full moon on the night day 0 ends with and every eighth after: voxel.day <day>")]
    internal static string Day(int day)
    {
        if (Game is not { } game) return "no world is loaded";
        game.Sky.Day = Math.Max(0, day);
        return $"the day is {game.Sky.Day}, and tonight's moon is {PhaseNames[Sky.MoonPhase(game.Sky.Day, 18)]}";
    }

    private static readonly string[] PhaseNames =
        ["full", "waning gibbous", "at its last quarter", "a waning crescent", "new", "a waxing crescent", "at its first quarter", "waxing gibbous"];

    [Command("voxel.weather", "Says how far the weather has turned and what falls where the player stands, or holds it turned or clear, or lets it turn by its days again: voxel.weather [turned|clear|days]")]
    internal static string SetWeather(string hold = "")
    {
        if (Game is not { } game) return "no world is loaded";
        var weather = game.Weather;
        switch (hold.ToLowerInvariant())
        {
            case "": break;
            case "turned": weather.Held = true; break;
            case "clear": weather.Held = false; break;
            case "days": weather.Held = null; break;
            default: return $"no weather is called {hold}, which is turned, clear or days";
        }
        var at = game.Player.Body.Position;
        var falls = game.World.BiomeAt((int)MathF.Floor(at.X), (int)MathF.Floor(at.Z))?.Precipitation ?? Precipitation.None;
        var held = weather.Held switch { true => "held turned", false => "held clear", null => "turning by its days" };
        return string.Create(CultureInfo.InvariantCulture, $"the weather is {held}, turned {weather.Share(game.Sky.Day, game.Sky.Hour):0.00} and shown {game.Sky.Overcast:0.00}, and {(falls == Precipitation.None ? "nothing falls" : falls.ToString().ToLowerInvariant() + " falls")} where the player stands, on {game.Precipitation.Columns} columns about the eye");
    }

    [Command("voxel.cycle", "Lets the day go on by itself or holds the hour: voxel.cycle <on>")]
    internal static string Cycle(bool on)
    {
        if (Game is not { } game) return "no world is loaded";
        game.Sky.Cycle = on;
        return on ? "the day goes on" : "the hour is held";
    }

    [Command("voxel.world", "Saves the world and opens the one of a kind, overworld or flat, and seed, beginning it where none is saved: voxel.world <kind> [seed]")]
    internal static string NewWorld(string kind, int seed = 1)
    {
        if (Game is not { } game) return "no world is loaded";
        if (Generators.Create(kind, seed) is null) return $"no world is called {kind}, which is {string.Join(" or ", Generators.Kinds)}";
        game.NextWorld = (kind.ToLowerInvariant(), seed, null);
        return $"the {kind.ToLowerInvariant()} world of seed {seed} opens next frame";
    }

    [Command("voxel.save", "Saves the changed columns and the player's place now, as is done every 30 seconds and on leaving a world")]
    internal static string SaveNow()
    {
        if (Game is not { } game) return "no world is loaded";
        if (game.Save is null) return "the world is transient and is not saved";
        game.SaveWorld();
        return $"saved to {game.Save.Folder}";
    }

    [Command("voxel.gpu", "Says whether the GPU reads the caves' noises for the columns to generate and how many columns it has read, turns it on or off for the session, or compares its readings with the CPU's over a square of columns about the player: voxel.gpu [on|off|verify] [columns across]")]
    internal static string Gpu(string action = "", int across = 2)
    {
        if (Game is not { } game) return "no world is loaded";
        var fields = game.CaveFields;
        switch (action)
        {
            case "on" or "off":
                fields.Enabled = action == "on";
                break;
            case "verify":
            {
                var at = game.Player.Body.Position;
                int cx = VoxelWorld.ColumnOf((int)MathF.Floor(at.X)), cz = VoxelWorld.ColumnOf((int)MathF.Floor(at.Z));
                across = Math.Clamp(across, 1, 8);
                var columns = Enumerable.Range(0, across * across).Select(i => (cx - across / 2 + i % across, cz - across / 2 + i / across)).ToList();
                return fields.Verify(columns) is var (compared, differ)
                    ? $"{differ} of {compared} readings over {columns.Count} columns differ between the GPU and the CPU"
                    : "the GPU reads no caves' noises here, with no compute or no overworld";
            }
            case "":
                break;
            default:
                return $"{action} is neither on, off nor verify";
        }
        var state = fields.Running ? "on" : fields.Enabled ? "on but unable, with no compute or no overworld" : "off";
        return $"the GPU's reading of the caves' noises is {state}, {fields.Read} columns read since the world opened";
    }

    [Command("voxel.distance", "Sets how many columns around the player are drawn, from 2 to 16: voxel.distance <columns>")]
    internal static string Distance(int columns)
    {
        if (Game is not { } game) return "no world is loaded";
        game.RenderDistance = Math.Clamp(columns, 2, 16);
        return $"{game.RenderDistance} columns are drawn around the player";
    }

    [Command("voxel.packs", "Says the resource packs on, the first over the rest, and those in the game's folder, or turns on the packs named, parted by semicolons, a name in the folder or a whole path, and saves them as the setting, or turns every one off with none: voxel.packs [pack; pack...|none]")]
    internal static string Packs(string packs = "")
    {
        if (Game is not { } game) return "no world is loaded";
        var list = PackList.Default;
        if (packs.Length > 0)
        {
            var entries = packs.Trim() == "none" ? [] : packs.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
            if (entries.FirstOrDefault(e => !File.Exists(list.PathOf(e)) && !Directory.Exists(list.PathOf(e))) is { } missing)
                return $"no pack is at {list.PathOf(missing)}";
            game.ChoosePacks(entries);
        }
        var on = game.PackEntries.Count == 0 ? "none" : string.Join("; ", game.PackEntries);
        var kept = list.Available() is { Count: > 0 } available ? string.Join("; ", available) : "none";
        return $"on, the first over the rest: {on}; read {game.Packs.Names.Count} of them; in {list.Folder}: {kept}";
    }

    [Command("voxel.palette", "Each surface's color as the packs loaded give it, its texture's average before any tint, in sRGB, and how much of it is covered, which the game's own colors are read from")]
    internal static string Palette()
    {
        if (Game is not { } game) return "no world is loaded";
        var atlas = game.Renderer.Atlas;
        var lines = Surfaces.All.Select(s => atlas.Average(s.Id) is { } c ? $"{s.Name}: {c.R} {c.G} {c.B} {c.A}" : null).OfType<string>().ToList();
        return lines.Count == 0 ? "no pack gives a surface a texture" : string.Join("; ", lines);
    }

    [Command("voxel.reload", "Reads the resource packs on again from their files, as F3 with T does")]
    internal static string Reload()
    {
        if (Game is not { } game) return "no world is loaded";
        game.ReloadPacks();
        return $"read {game.Packs.Names.Count} packs again";
    }

    [Command("voxel.select", "Puts an item, a block or a thing of its own, in the hotbar's chosen slot, one in creative mode and a full stack in survival: voxel.select <item>")]
    internal static string Select(string item)
    {
        if (Game is not { } game) return "no world is loaded";
        if (Items.Find(item) is not { } found) return $"no item is called {item}, and voxel.items lists them";
        var count = game.Mode == GameMode.Creative ? 1 : found.MostInStack;
        game.Inventory.Slots[game.Hotbar.Selected] = new ItemStack(found, count);
        return $"slot {game.Hotbar.Selected + 1} holds {count} {found.Key}";
    }

    [Command("voxel.give", "Puts so many of an item into the inventory, as a stack picked up goes: voxel.give <item> [count]")]
    internal static string Give(string item, int count = 1)
    {
        if (Game is not { } game) return "no world is loaded";
        if (Items.Find(item) is not { } found) return $"no item is called {item}, and voxel.items lists them";
        var left = game.Inventory.Add(found, Math.Max(1, count));
        return $"{Math.Max(1, count) - left} {found.Key} put in{(left > 0 ? $", {left} did not fit" : "")}";
    }

    [Command("voxel.drop", "Lays one of an item on the ground at a place, as a block broken there drops it: voxel.drop <item> <x> <y> <z>")]
    internal static string Drop(string item, float x, float y, float z)
    {
        if (Game is not { } game) return "no world is loaded";
        if (Items.Find(item) is not { } found) return $"no item is called {item}, and voxel.items lists them";
        game.Drops.Spawn(found, 1, new Vector3(x, y, z), Vector3.Zero);
        return $"one {found.Key} lies at {x:0.00}, {y:0.00}, {z:0.00}";
    }

    [Command("voxel.inventory", "Each slot of the inventory that holds something, the hotbar's 1 to 9 and the rest's 10 to 36")]
    internal static string ListInventory()
    {
        if (Game is not { } game) return "no world is loaded";
        var held = game.Inventory.Slots.Select((s, i) => s is { } stack ? $"{i + 1} {stack.Count} {stack.Item.Key}" : null).Where(s => s is not null).ToList();
        return held.Count == 0 ? "the inventory is empty" : string.Join(", ", held);
    }

    [Command("voxel.items", "Every item's name, the blocks' and the things' of their own, which voxel.select and voxel.give take")]
    internal static string ListItems() => string.Join(", ", Items.All.Select(i => i.Key));

    [Command("voxel.craft", "Makes an item from what the inventory holds as a recipe book does, as at a crafting table, so many times: voxel.craft <item> [times]")]
    internal static string Craft(string item, int times = 1)
    {
        if (Game is not { } game) return "no world is loaded";
        if (Items.Find(item) is not { } found) return $"no item is called {item}, and voxel.items lists them";
        if (!Recipes.All.Any(r => r.Result == found)) return $"no recipe makes {found.Key}";
        var made = 0;
        for (int i = 0; i < Math.Max(1, times); i++)
        {
            if (Recipes.CraftFrom(game.Inventory, found) is not { } recipe) break;
            var left = game.Inventory.Add(recipe.Result, recipe.Count);
            if (left > 0) game.Drops.Spawn(recipe.Result, left, game.Player.Eye, Vector3.Zero, ItemDrops.ThrownDelay);
            made += recipe.Count;
        }
        return made > 0 ? $"{made} {found.Key} made" : $"the inventory lacks what {found.Key} is made of";
    }

    [Command("voxel.chest", "Says what the chest at a place holds, rolling the loot it holds first as opening it does: voxel.chest <x> <y> <z>")]
    internal static string Chest(int x, int y, int z)
    {
        if (Game is not { } game) return "no world is loaded";
        if (!Chests.IsChest(game.World.GetBlock(x, y, z))) return $"no chest stands at {x}, {y}, {z}";
        var held = game.Chests.At(x, y, z).Slots.OfType<ItemStack>().ToList();
        return held.Count == 0 ? "the chest is empty" : string.Join(", ", held.Select(s => $"{s.Count} {s.Item.Key}"));
    }

    [Command("voxel.furnace", "Says what the furnace at a place holds, its fire and its smelting, putting so many of an item to smelt and of a fuel in it where given: voxel.furnace <x> <y> <z> [item] [count] [fuel] [count]")]
    internal static string Furnace(int x, int y, int z, string item = "", int count = 1, string fuel = "", int fuelCount = 1)
    {
        if (Game is not { } game) return "no world is loaded";
        if (!VoxelSandbox.Furnaces.IsFurnace(game.World.GetBlock(x, y, z))) return $"no furnace stands at {x}, {y}, {z}";
        var furnace = game.Furnaces.At(x, y, z);
        if (item.Length > 0)
        {
            if (Items.Find(item) is not { } input) return $"no item is called {item}";
            furnace.Slots[VoxelSandbox.Furnace.Input] = new ItemStack(input, Math.Clamp(count, 1, input.MostInStack));
        }
        if (fuel.Length > 0)
        {
            if (Items.Find(fuel) is not { } burns) return $"no item is called {fuel}";
            furnace.Slots[VoxelSandbox.Furnace.Fuel] = new ItemStack(burns, Math.Clamp(fuelCount, 1, burns.MostInStack));
        }
        string Slot(ItemStack? s) => s is { } stack ? $"{stack.Count} {stack.Item.Key}" : "nothing";
        return $"smelting {Slot(furnace.Slots[0])}, fuel {Slot(furnace.Slots[1])}, made {Slot(furnace.Slots[2])}; "
            + $"{(furnace.Lit ? $"burning, {furnace.Burning:0.0} of {furnace.BurnTotal:0} s left" : "out")}, smelted {furnace.Cooked:0.0} of {Smelting.Seconds:0} s";
    }

    [Command("voxel.mode", "Plays in creative, survival, adventure or spectator mode, as the switcher F3 with F4 opens does: voxel.mode <creative|survival|adventure|spectator>")]
    internal static string SetMode(string mode)
    {
        if (Game is not { } game) return "no world is loaded";
        if (!Enum.TryParse<GameMode>(mode, ignoreCase: true, out var chosen) || !Enum.IsDefined(chosen))
            return $"no mode is called {mode}, which is creative, survival, adventure or spectator";
        game.Mode = chosen;
        return $"the player plays in {chosen.ToString().ToLowerInvariant()} mode";
    }

    [Command("voxel.throw", "Throws one of the chosen slot's items ahead, or its whole stack, as Q and Control with Q do: voxel.throw [all]")]
    internal static string Throw(bool all = false)
    {
        if (Game is not { } game) return "no world is loaded";
        return game.Throw(all) ? $"thrown, {game.Drops.Count} stacks on the ground" : "the chosen slot is empty";
    }

    [Command("voxel.clouds", "Shows or hides the clouds, and where a drift is given places them that many blocks east, as a capture taken again places them: voxel.clouds <on|off> [drift]")]
    internal static string ShowClouds(bool on, float drift = -1)
    {
        if (Game is not { } game) return "no world is loaded";
        game.Clouds.Shown = on;
        if (drift >= 0) game.Clouds.Drift = drift;
        return on ? $"the clouds are drawn, {game.Clouds.Faces} faces, drifted {game.Clouds.Drift:0.0} blocks east" : "the clouds are hidden";
    }

    [Command("voxel.view", "Sets where the camera stands, at the eye, behind the player or in front of it, as F5 steps it: voxel.view <first|behind|front>")]
    internal static string SetView(string view)
    {
        if (Game is not { } game) return "no world is loaded";
        if (!Enum.TryParse<View>(view, ignoreCase: true, out var chosen) || !Enum.IsDefined(chosen)) return $"no view is called {view}, which is first, behind or front";
        game.Player.View = chosen;
        return chosen switch { View.First => "the camera stands at the eye", View.Behind => "the camera stands behind the player", _ => "the camera stands in front of the player" };
    }

    [Command("voxel.sounds", "How loud each sound of the place plays, before the settings, the room about the player as a reverb would read it, the steps played since the world opened and those of the last frame with the share the blocks between let through, and the voices the engine's audio holds")]
    internal static string ShowSounds()
    {
        if (Game is not { } game) return "no world is loaded";
        var sounds = game.Sounds;
        var levels = string.Join(", ", Enum.GetValues<AmbientSound>().Select(a => $"{a.ToString().ToLowerInvariant()} {sounds.Levels[(int)a]:0.00}"));
        var last = sounds.Heard.Count == 0 ? "none" : string.Join(", ", sounds.Heard.Select(h => $"{h.Sound.ToString().ToLowerInvariant()} {(h.Own ? "of the player" : $"at {h.At.X:0.0}, {h.At.Y:0.0}, {h.At.Z:0.0} through {h.Through:0.00}")}"));
        var voices = Engine.E3D.GetApp().World.TryGetResource<Engine.AudioServer>(out var audio) ? audio.LiveSourceCount : 0;
        var (enclosed, size) = sounds.Room;
        return $"{levels}; the room {enclosed:0.00} closed, its walls {size:0.0} blocks off; {sounds.StepsHeard} steps played, in the last frame {last}; {voices} voices";
    }

    [Command("voxel.break", "Breaks the block the crosshair rests on at once, dropping what it leaves in survival, as a left click does")]
    internal static string Break() =>
        Game is not { } game ? "no world is loaded" : game.Break() ? "the block is broken" : "no block within reach can be broken";

    [Command("voxel.place", "Places the hotbar's chosen block against the face the crosshair rests on, using one up in survival, or opens a crafting table or opens or shuts a door, as a right click does")]
    internal static string Place()
    {
        if (Game is not { } game) return "no world is loaded";
        var (atTable, atFurnace, atChest) = (game.AtTable, game.AtFurnace, game.AtChest);
        var used = game.Use();
        if (!atTable && game.AtTable) return "the crafting table is opened";
        if (atFurnace is null && game.AtFurnace is { } at) return $"the furnace at {at.X}, {at.Y}, {at.Z} is opened";
        if (atChest is null && game.AtChest is { } box) return $"the chest at {box.X}, {box.Y}, {box.Z} is opened";
        return used is not BlockId.Air ? $"{Blocks.Get(used).Key} is placed" : "nothing can be placed there";
    }

    [Command("voxel.fires", "How many fires burn and the nearest few, each where it burns and its age from 0 to 15: voxel.fires [count]")]
    internal static string ListFires(int count = 5)
    {
        if (Game is not { } game) return "no world is loaded";
        // A fire put out by a hand, a blast or water is let go at its next tick, and is not counted.
        var all = game.Fires.All.Where(f => game.World.GetBlock(f.At.X, f.At.Y, f.At.Z) == BlockId.Fire).ToList();
        if (all.Count == 0) return "no fire burns";
        var eye = game.Player.Eye;
        var nearest = all.OrderBy(f => Vector3.DistanceSquared(new Vector3(f.At.X + 0.5f, f.At.Y, f.At.Z + 0.5f), eye)).Take(count)
            .Select(f => $"{f.At.X}, {f.At.Y}, {f.At.Z} at age {f.Age}");
        return $"{all.Count} {(all.Count == 1 ? "fire burns" : "fires burn")}, the nearest at {string.Join("; ", nearest)}";
    }

    [Command("voxel.mobs", "How many mobs of each kind are loaded and kept, and the nearest few, each with its number, where it stands, what it does and its health: voxel.mobs [count]")]
    internal static string ListMobs(int count = 5)
    {
        if (Game is not { } game) return "no world is loaded";
        var mobs = game.Mobs;
        var kinds = string.Join(", ", MobKinds.All.Select(k => $"{mobs.All.Count(m => m.Kind == k)} {k.Name}"));
        var at = game.Player.Body.Position;
        var nearest = mobs.All.OrderBy(m => Vector3.DistanceSquared(m.Body.Position, at)).Take(Math.Max(0, count))
            .Select(m => $"{m.Kind.Name} {m.Id} at {m.Body.Position.X:0.0}, {m.Body.Position.Y:0.0}, {m.Body.Position.Z:0.0}, "
                + $"{Vector3.Distance(m.Body.Position, at):0} blocks away, {m.Doing}, health {m.Health}{(m.Home is { } home ? $", at home at {home.X:0}, {home.Y:0}, {home.Z:0}" : "")}");
        return $"{mobs.All.Count} loaded ({kinds}), {mobs.Kept} kept by columns not loaded{(mobs.Still ? ", standing still" : "")}"
            + (count > 0 && mobs.All.Count > 0 ? "; " + string.Join("; ", nearest) : "");
    }

    [Command("voxel.spawn", "Places a mob of a kind where the crosshair rests, or three blocks ahead, its variant by number where given, a villager at home there: voxel.spawn <pig|cow|sheep|chicken|villager> [variant]")]
    internal static string Spawn(string kind, int variant = -1)
    {
        if (Game is not { } game) return "no world is loaded";
        if (MobKinds.Find(kind) is not { } found) return $"no mob is called {kind}, which is {string.Join(", ", MobKinds.All.Select(k => k.Name))}";
        var player = game.Player;
        var at = game.Target is { } hit ? new Vector3(hit.Beside.X + 0.5f, hit.Beside.Y, hit.Beside.Z + 0.5f)
            : player.Body.Position + Vector3.Normalize(new Vector3(player.Look.X, 0, player.Look.Z)) * 3;
        var chosen = variant >= 0 ? (byte)(variant % found.Variants.Length) : found.Variant(Random.Shared.Next(1000));
        var mob = game.Mobs.Add(new MobRecord(found, chosen, at, player.Yaw + MathF.PI, found.Health, found.OpensDoors ? at : null));
        return $"{found.Name} {mob.Id} stands at {at.X:0.0}, {at.Y:0.0}, {at.Z:0.0}";
    }

    [Command("voxel.attack", "Hits the mob the crosshair rests on, as a left click does")]
    internal static string Attack()
    {
        if (Game is not { } game) return "no world is loaded";
        if (game.TargetMob is not { } mob) return $"no mob within {Mobs.Reach} blocks under the crosshair";
        if (!game.Attack()) return $"{mob.Kind.Name} {mob.Id} is still red from the last hit";
        return mob.Dying >= 0 ? $"{mob.Kind.Name} {mob.Id} dies" : $"{mob.Kind.Name} {mob.Id} is hit, its health {mob.Health}";
    }

    [Command("voxel.still", "Stops the mobs where they stand, the clouds where they have drifted and the bits of broken blocks where they fly, or lets them move again, for a capture: voxel.still <on>")]
    internal static string Still(bool on)
    {
        if (Game is not { } game) return "no world is loaded";
        (game.Mobs.Still, game.Clouds.Held, game.Bits.Held) = (on, on, on);
        return on ? "the mobs stand still and the clouds and the bits hold" : "the mobs move and the clouds and the bits drift";
    }

    [Command("voxel.bits", "How many bits of broken blocks fly and the middle of the one nearest a place: voxel.bits <x> <y> <z>")]
    internal static string BitsAround(float x, float y, float z)
    {
        if (Game is not { } game) return "no world is loaded";
        var place = new Vector3(x, y, z);
        var nearest = game.Bits.All.Select(b => b.Body.Position + new Vector3(0, b.Size / 2, 0)).OrderBy(p => Vector3.DistanceSquared(p, place)).FirstOrDefault();
        return game.Bits.All.Count == 0 ? "no bits fly" : $"{game.Bits.All.Count} bits, the nearest at {nearest.X:F2}, {nearest.Y:F2}, {nearest.Z:F2}";
    }

    [Command("voxel.loading", "Keeps the loading screen up over a world that has loaded, so its pixels are read, or lets it go: voxel.loading <hold>")]
    internal static string HoldLoading(bool hold)
    {
        if (Game is not { } game) return "no world is loaded";
        game.HoldLoading = hold;
        return hold ? "the loading screen is held" : game.Loading ? "the loading screen goes once the world stands" : "the world is shown";
    }

    [Command("voxel.kill", "Takes away the loaded mobs of a kind, or all of them: voxel.kill <kind|all>")]
    internal static string Kill(string kind)
    {
        if (Game is not { } game) return "no world is loaded";
        MobKind? found = null;
        if (!kind.Equals("all", StringComparison.OrdinalIgnoreCase) && (found = MobKinds.Find(kind)) is null)
            return $"no mob is called {kind}, which is {string.Join(", ", MobKinds.All.Select(k => k.Name))} or all";
        return $"{game.Mobs.Clear(found)} mobs taken away";
    }

    [Command("voxel.cull", "Leaves the sections the eye cannot see through open cells undrawn, or draws every one: voxel.cull <on>")]
    internal static string Cull(bool on)
    {
        if (Game is not { } game) return "no world is loaded";
        game.Renderer.Cull = on;
        return on ? "the sections the eye cannot see are left undrawn" : "every section within the render distance is drawn";
    }

    [Command("voxel.ticks", "Holds the animated textures and lamps at a tick, a twentieth of a second each, or lets them run with the time: voxel.ticks <tick|off>")]
    internal static string Ticks(string tick)
    {
        if (Game is not { } game) return "no world is loaded";
        if (tick.Equals("off", StringComparison.OrdinalIgnoreCase))
        {
            game.Renderer.HeldTicks = null;
            return "the animated textures run with the time";
        }
        if (!int.TryParse(tick, out var held) || held < 0) return $"{tick} is neither a tick from 0 up nor off";
        game.Renderer.HeldTicks = held;
        return $"the animated textures are held at tick {held}";
    }

    [Command("voxel.gi", "Sets global illumination to a quality, off, low, medium or high: voxel.gi <quality>")]
    internal static string Gi(string quality)
    {
        if (Game is not { } game) return "no world is loaded";
        if (!Enum.TryParse<GlobalIllumination>(quality, ignoreCase: true, out var chosen)) return $"no quality is called {quality}, which is off, low, medium or high";
        game.Light.Quality = chosen;
        SetGlobalIllumination(chosen);
        return $"global illumination is at {chosen}";
    }

    [Command("voxel.field", "Sets the scene's distance field's cascades from 1 to 8, the finest's cell in blocks and the cascades built again a frame: voxel.field <cascades> <cell> <budget>")]
    internal static string Field(int cascades, float cell, int budget)
    {
        if (Game is not { } game) return "no world is loaded";
        var light = game.Light;
        (light.Cascades, light.CellSize, light.Budget) = (Math.Clamp(cascades, 1, 8), Math.Max(0.05f, cell), Math.Clamp(budget, 1, 8));
        light.ApplyField();
        return $"{light.Cascades} cascades, the finest's cell {light.CellSize} blocks, {light.Budget} built a frame";
    }

    [Command("voxel.options", "Each setting of an options screen with its value and reach, of general, video, render, post, sky, particles, sounds or colors, or sets one by its name, its words joined by underscores, to a value or a choice: voxel.options <screen> [setting] [value]")]
    internal static string Options(string screen, string setting = "", string value = "")
    {
        if (Game is not { } game) return "no world is loaded";
        var screens = game.Menus.Screens(game).All().ToList();
        var options = screens.FirstOrDefault(s => s.Name.Equals(screen, StringComparison.OrdinalIgnoreCase)).Options;
        if (options is null) return $"no screen is called {screen}, which is {string.Join(", ", screens.Select(s => s.Name))}";
        if (setting.Length == 0) return string.Join("\n", options.Select(Reach));
        var option = options.FirstOrDefault(o => o.Name.Replace(' ', '_').Equals(setting, StringComparison.OrdinalIgnoreCase));
        if (option is null) return $"no setting of {screen} is called {setting}";
        if (value.Length > 0)
        {
            var choice = option.Choices is { } choices ? Array.FindIndex(choices, c => c.Replace(' ', '_').Equals(value, StringComparison.OrdinalIgnoreCase)) : -1;
            if (choice >= 0) option.Change(choice);
            else if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)) option.Change(number);
            else return $"{value} is neither a number nor a choice of {option.Name}";
            option.Settled?.Invoke();
            game.KeepOptions();
        }
        return Reach(option);

        static string Reach(Option o) => o.Choices is { } choices
            ? $"{o.Label}, of {string.Join(", ", choices)}"
            : string.Create(CultureInfo.InvariantCulture, $"{o.Label}, {o.Value:0.###} from {o.Low} to {o.High}{(o.Step > 0 ? " by " + o.Step.ToString(CultureInfo.InvariantCulture) : "")}");
    }

    [Command("voxel.shadows", "Sets how far from the eye the sun's shadows reach, in blocks: voxel.shadows <distance>")]
    internal static string Shadows(float distance)
    {
        if (Game is not { } game) return "no world is loaded";
        game.Light.ShadowDistance = Math.Max(1, distance);
        SetShadowDistance(game.Light.ShadowDistance);
        return $"the sun's shadows reach {game.Light.ShadowDistance} blocks";
    }

    [Command("voxel.hud", "Shows or hides the crosshair, the hotbar, the outline and the line of help, as F1 does, for a capture: voxel.hud <on>")]
    internal static string Hud(bool on)
    {
        if (Game is not { } game) return "no world is loaded";
        game.HudHidden = !on;
        return on ? "the hud is shown" : "the hud is hidden";
    }

    [Command("voxel.scale", "Sets the interface's scale, 1 to 4 of the screen's pixels to each of its own or 0 for the largest that fits, as the options' GUI Scale does: voxel.scale <scale>")]
    internal static string SetScale(int scale)
    {
        Gui.Chosen = Math.Clamp(scale, 0, 4);
        return $"the interface is drawn at {Gui.Scale} of the screen's pixels to each of its own";
    }

    [Command("voxel.fov", "Sets the vertical field of view in degrees, 30 to 110 as the options' FOV, as a zoom narrows it for a screenshot: voxel.fov <degrees>")]
    internal static string SetFieldOfView(float degrees)
    {
        if (Game is not { } game) return "no world is loaded";
        game.Player.FieldOfView = Math.Clamp(degrees, 30, 110);
        return $"the field of view is {game.Player.FieldOfView:0.#} degrees";
    }

    [Command("voxel.flicker", "Records each frame's change in the picture from the one before for some frames, the player turning some degrees and stepping some blocks ahead in equal parts over some frames from the tenth, into a file: voxel.flicker <frames> <turn> <step> <over> <path>")]
    internal static string Flicker(int frames, float turn, float step, int over, string path)
    {
        if (Game is not { } game) return "no world is loaded";
        over = Math.Max(1, over);
        frames = Math.Max(frames, 12 + over);
        game.Flicker = new FlickerRun(frames, 10, over, turn, step, Path.GetFullPath(path));
        return $"recording {frames} frames into {Path.GetFullPath(path)}, the motion over frames 10 to {10 + over}";
    }

    [Command("voxel.ring", "The last frame's luminance around a circle on a level plane, by angle, its mean, its swing and its peaks: voxel.ring <x> <y> <z> <radius> <samples>")]
    internal static string Ring(float x, float y, float z, float radius, int samples)
    {
        if (Game is not { } game) return "no world is loaded";
        if (!Measurements.Keeping()) return "the frames are kept from now on, so ask again after a frame";
        return Measurements.Ring(LoadImageFromScreen(), game.Player.Camera, new System.Numerics.Vector3(x, y, z), radius, Math.Clamp(samples, 8, 720));
    }

    [Command("voxel.spots", "How many pixels of a rectangle of the last frame stand darker, and how many lighter, than all four neighbors three pixels away by more than so many levels: voxel.spots <x> <y> <width> <height> <levels>")]
    internal static string Spots(int x, int y, int width, int height, float levels)
    {
        if (Game is null) return "no world is loaded";
        if (!Measurements.Keeping()) return "the frames are kept from now on, so ask again after a frame";
        var (darker, lighter, read) = Measurements.Spots(LoadImageFromScreen(), x, y, width, height, levels);
        return $"{darker} darker, {lighter} lighter of {read} at {x}, {y}";
    }

    [Command("voxel.pixel", "The last frame's color in sRGB levels at the pixel a point in the world lies on: voxel.pixel <x> <y> <z>")]
    internal static string Pixel(float x, float y, float z)
    {
        if (Game is not { } game) return "no world is loaded";
        if (!Measurements.Keeping()) return "the frames are kept from now on, so ask again after a frame";
        return Measurements.Pixel(LoadImageFromScreen(), game.Player.Camera, new System.Numerics.Vector3(x, y, z));
    }

    [Command("voxel.line", "The last frame's luminance at points evenly along a line in the world, and the largest change between neighbors as a share of the whole: voxel.line <x1> <y1> <z1> <x2> <y2> <z2> <samples>")]
    internal static string Line(float x1, float y1, float z1, float x2, float y2, float z2, int samples)
    {
        if (Game is not { } game) return "no world is loaded";
        if (!Measurements.Keeping()) return "the frames are kept from now on, so ask again after a frame";
        return Measurements.Line(LoadImageFromScreen(), game.Player.Camera, new System.Numerics.Vector3(x1, y1, z1), new System.Numerics.Vector3(x2, y2, z2), Math.Clamp(samples, 2, 720));
    }

    [Command("voxel.color", "The last frame's mean red, green and blue in sRGB levels over a rectangle of the window: voxel.color <x> <y> <width> <height>")]
    internal static string MeanColor(int x, int y, int width, int height)
    {
        if (Game is null) return "no world is loaded";
        if (!Measurements.Keeping()) return "the frames are kept from now on, so ask again after a frame";
        return Measurements.MeanColor(LoadImageFromScreen(), x, y, width, height) is var (r, g, b)
            ? $"red {r:0.0}, green {g:0.0}, blue {b:0.0} over {width} by {height} at {x}, {y}"
            : $"no pixel of {width} by {height} at {x}, {y} is on the window";
    }

    [Command("voxel.gaps", "Pixels of the last frame in a rectangle of the window nearer the background's color than both neighbors along a row or a column, as a crack between faces lets the sky through, the background taken at each row from the column left of the rectangle: voxel.gaps <x> <y> <width> <height>")]
    internal static string Gaps(int x, int y, int width, int height)
    {
        if (Game is null) return "no world is loaded";
        if (!Measurements.Keeping()) return "the frames are kept from now on, so ask again after a frame";
        return Measurements.Gaps(LoadImageFromScreen(), x, y, width, height) is { } gaps
            ? $"{gaps.Count} gaps in {width} by {height} at {x}, {y}" + (gaps.Count > 0 ? $", the first at {gaps[0].X}, {gaps[0].Y}" : "")
            : $"{width} by {height} at {x}, {y} with the column left of it is not all on the window";
    }

    [Command("voxel.edge", "Where the drawn land ends up a column of the last frame's pixels, the land's last pixel's color and the one above's, and the largest step between neighboring pixels within 16 of it, in sRGB levels: voxel.edge <x>")]
    internal static string Edge(int x)
    {
        if (Game is not { } game) return "no world is loaded";
        if (!Measurements.Keeping()) return "the frames are kept from now on, so ask again after a frame";
        var image = LoadImageFromScreen();
        if ((uint)x >= image.Width) return $"no column {x} is on the window";
        // The highest pixel whose ray meets a block of a column the renderer draws, from the bottom up.
        int eyeX = (int)MathF.Floor(game.Player.Eye.X) >> Section.Shift, eyeZ = (int)MathF.Floor(game.Player.Eye.Z) >> Section.Shift;
        var reach = game.RenderDistance * game.RenderDistance + game.RenderDistance;
        bool Land(int y)
        {
            var ray = GetScreenToWorldRay(new System.Numerics.Vector2(x, y), game.Player.Camera);
            if (VoxelRay.Cast(game.World, ray.Position, ray.Direction, 1000) is not { } hit) return false;
            int dx = (hit.X >> Section.Shift) - eyeX, dz = (hit.Z >> Section.Shift) - eyeZ;
            return dx * dx + dz * dz <= reach;
        }
        var edge = -1;
        for (int y = image.Height - 1; y >= 0 && edge < 0; y--)
            if (!Land(y)) edge = y + 1;
        if (edge <= 0 || edge >= image.Height) return $"column {x} shows {(edge <= 0 ? "land to its top" : "no land")}";
        (int R, int G, int B) At(int y)
        {
            var i = (y * image.Width + x) * 4;
            return (image.Data[i], image.Data[i + 1], image.Data[i + 2]);
        }
        var step = 0;
        for (int y = Math.Max(1, edge - 16); y < Math.Min(image.Height, edge + 16); y++)
        {
            var (a, c) = (At(y - 1), At(y));
            step = Math.Max(step, Math.Max(Math.Abs(a.R - c.R), Math.Max(Math.Abs(a.G - c.G), Math.Abs(a.B - c.B))));
        }
        var (land, sky) = (At(edge), At(edge - 1));
        return $"land ends at {edge}, land {land.R} {land.G} {land.B}, above {sky.R} {sky.G} {sky.B}, largest step {step}";
    }

    [Command("voxel.depth", "The block under a pixel of the window and how far it is from the eye, along the ray and along the ground: voxel.depth <x> <y>")]
    internal static string Depth(int x, int y)
    {
        if (Game is not { } game) return "no world is loaded";
        var ray = GetScreenToWorldRay(new System.Numerics.Vector2(x, y), game.Player.Camera);
        if (VoxelRay.Cast(game.World, ray.Position, ray.Direction, 1000) is not { } hit) return "no block within 1000 blocks under that pixel";
        var point = ray.Position + System.Numerics.Vector3.Normalize(ray.Direction) * hit.Distance;
        var flat = new System.Numerics.Vector2(point.X - ray.Position.X, point.Z - ray.Position.Z).Length();
        return $"{Blocks.Get(game.World.GetBlock(hit.X, hit.Y, hit.Z)).Key} at {hit.X}, {hit.Y}, {hit.Z}, {hit.Distance:0.0} blocks along the ray and {flat:0.0} along the ground";
    }

    [Command("voxel.find", "The nearest start of a structure from the player, by the corner it is built from: voxel.find <village|igloo|pyramid|dungeon|ravine|mineshaft|ruin|shipwreck|temple|hut|outpost>")]
    internal static string Find(string name)
    {
        if (Game is not { } game) return "no world is loaded";
        if (Structures.Find(name) is not { } structure) return $"no structure is called {name}, which is {string.Join(", ", Structures.All.Select(s => s.Name))}";
        if (game.World.Generator is not Overworld land) return "only the overworld holds structures";
        var at = game.Player.Body.Position;
        int cx = (int)MathF.Floor(at.X / structure.CellSize), cz = (int)MathF.Floor(at.Z / structure.CellSize);
        // Ring by ring of cells outward, the nearest start in the first ring that holds one.
        for (int ring = 0; ring <= 40; ring++)
        {
            StructureStart? best = null;
            var bestDistance = float.MaxValue;
            for (int z = cz - ring; z <= cz + ring; z++)
                for (int x = cx - ring; x <= cx + ring; x++)
                {
                    if (Math.Max(Math.Abs(x - cx), Math.Abs(z - cz)) != ring || structure.StartIn(x, z, land) is not { } start) continue;
                    var distance = new System.Numerics.Vector2(start.X - at.X, start.Z - at.Z).Length();
                    if (distance < bestDistance) (best, bestDistance) = (start, distance);
                }
            if (best is { } found) return $"the nearest {structure.Name} is built from {found.X}, {found.Y}, {found.Z}, {bestDistance:0} blocks away";
        }
        return $"no {structure.Name} within {40 * structure.CellSize} blocks";
    }

    [Command("voxel.biome", "The nearest place of a biome from the player, its ground's top, searched outward in rings 8 blocks apart to 3000 blocks: voxel.biome <name>")]
    internal static string FindBiome(string name)
    {
        if (Game is not { } game) return "no world is loaded";
        if (Biomes.All.FirstOrDefault(b => b.Name.Equals(name.Replace('_', ' '), StringComparison.OrdinalIgnoreCase)) is not { } biome)
            return $"no biome is called {name}, which is {string.Join(", ", Biomes.All.Select(b => b.Name))}";
        if (game.World.Generator is not Overworld land) return "only the overworld has biomes";
        var at = game.Player.Body.Position;
        int px = (int)MathF.Floor(at.X), pz = (int)MathF.Floor(at.Z);
        for (int ring = 0; ring <= 375; ring++)
            for (int z = -ring; z <= ring; z++)
                for (int x = -ring; x <= ring; x++)
                {
                    if (Math.Max(Math.Abs(x), Math.Abs(z)) != ring) continue;
                    int wx = px + x * 8, wz = pz + z * 8;
                    var (height, _, _, found) = land.At(wx, wz);
                    if (found == biome) return $"the nearest {biome.Name} is at {wx}, {height}, {wz}, {ring * 8} blocks away";
                }
        return $"no {biome.Name} within 3000 blocks";
    }

    [Command("voxel.blocks", "Every block's number and name, which voxel.set, voxel.fill and voxel.select take")]
    internal static string List() => string.Join(", ", Blocks.All.Select(b => $"{(int)b.Id} {b.Key}{(b.Emits ? " (emissive)" : "")}"));
}
