using System.Numerics;
using Engine;
using ImGuiNET;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>The game's state and its frame, which hold the world, its mobs, the player, the sky, and what is drawn over them.</summary>
public sealed class VoxelGame : IDisposable
{
    private const float Reach = 5;
    // Holding a mouse button breaks or places a block four times a second, as creative mode does.
    private const float Repeat = 0.25f;
    private const double MeshBudgetMs = 4;

    // The changed columns and the player are saved this often, beside a save on leaving a world.
    private const float SaveSeconds = 30;

    private readonly bool _transient;
    private readonly Random _random = new();
    // Seconds food takes to eat, as Minecraft's 32 ticks.
    private const float EatSeconds = 1.6f;

    private float _breakWait, _placeWait, _sinceSave;
    private int _teleports;
    private bool _spilled;
    private bool _captured, _swallowClick;
    private GameMode _mode;
    // Whether F4 or T was pressed while F3 has been held, so letting F3 go opens no settings.
    private bool _switched;

    /// <summary>
    /// Opens the world saved under <paramref name="name"/>, or under its kind and seed when no name is
    /// given, or begins it there from <paramref name="kind"/> and <paramref name="seed"/> where none is
    /// saved yet. A <paramref name="transient"/> world is neither read from disk nor written to it, as
    /// a test run's is. The game is drawn with the resource packs at <paramref name="packs"/>, the first
    /// over the rest, where they are given, and otherwise with those the setting names, which a
    /// transient run leaves alone, so it is drawn the same on every machine. Where the setting names
    /// none yet, as at the first start, the default packs are fetched and turned on once they are in.
    /// </summary>
    public VoxelGame(string kind, int seed, string? name, bool transient, IEnumerable<string>? packs = null)
    {
        _transient = transient;
        Hotbar = new Hotbar(Inventory);
        Renderer = new ChunkRenderer();
        var given = packs?.Select(Path.GetFullPath).ToList() ?? [];
        PackEntries = given.Count > 0 || transient ? given : PackList.Default.Read();
        Sky = new Sky(10);
        ReloadPacks();
        if (given.Count == 0 && !transient && !File.Exists(PackList.Default.File)) FetchDefaults();
        Light.ApplyAll();
        Open(kind, seed, name);
        Capture(true);
    }

    public VoxelWorld World { get; private set; } = null!;

    /// <summary>The player's resource packs the game is drawn with, or none.</summary>
    public ResourcePacks Packs { get; private set; } = ResourcePacks.None;

    /// <summary>The packs on, the first over the rest, each a line of the setting or a path given to the game.</summary>
    public IReadOnlyList<string> PackEntries { get; private set; } = [];

    /// <summary>The packs the game is drawn with by default and their fetch.</summary>
    public DefaultPacks Defaults { get; } = DefaultPacks.Load();

    // Whether the default packs are turned on over the rest once the fetch under way brings them in.
    private bool _awaitingDefaults;

    /// <summary>Fetches the default packs that are missing from the game's folder, and turns them on over the rest once all are in.</summary>
    public void FetchDefaults()
    {
        _awaitingDefaults = true;
        Defaults.Start(PackList.Default);
    }

    /// <summary>Stops fetching the default packs, saving the packs on as they are, so the next start does not fetch them again.</summary>
    public void StopDefaults()
    {
        Defaults.Stop();
        _awaitingDefaults = false;
        ChoosePacks(PackEntries);
    }

    // The default packs, once all are in and checked, turned on over the rest, and the game drawn
    // with them; a fetch that failed leaves the setting unwritten, so the next start tries again.
    private void TakeDefaults()
    {
        if (!_awaitingDefaults || Defaults.State is FetchState.Fetching or FetchState.Idle) return;
        _awaitingDefaults = false;
        if (Defaults.State != FetchState.Done) return;
        var names = Defaults.Packs.Select(p => p.File).ToList();
        ChoosePacks([.. names, .. PackEntries.Where(e => !names.Contains(e))]);
    }

    /// <summary>Draws the game with these packs from now on and saves them as the setting, but in a transient run.</summary>
    public void ChoosePacks(IReadOnlyList<string> entries)
    {
        PackEntries = entries;
        if (!_transient) PackList.Default.Write(entries);
        ReloadPacks();
    }

    /// <summary>
    /// Reads the packs on again from their files, as Minecraft does with F3 and T, and draws the
    /// terrain, the icons and the interface with them, the sections shaded again with their tiles.
    /// </summary>
    public void ReloadPacks()
    {
        var list = PackList.Default;
        var packs = new ResourcePacks(PackEntries.Select(list.PathOf));
        var terrain = TerrainAtlas.Build(packs);
        Renderer.Atlas = terrain;
        Icons.Use(terrain, packs);
        PackGui.Use(packs);
        Sky.Use(packs);
        Packs.Dispose();
        Packs = packs;
        // The world is not there yet as the game begins, and its sections are meshed as it opens.
        if (World is { } world) world.ReshadeAll();
    }

    /// <summary>The animals and villagers of the loaded columns, and those the unloaded ones keep.</summary>
    public Mobs Mobs { get; private set; } = null!;

    public MobRenderer MobRenderer { get; } = new();

    /// <summary>The stacks of items lying in the world.</summary>
    public ItemDrops Drops { get; } = new();

    /// <summary>The arrows skeletons have loosed.</summary>
    public Arrows Arrows { get; } = new();

    /// <summary>The world's furnaces by the place of their block.</summary>
    public Furnaces Furnaces { get; private set; } = new();

    // The explosions of the last half second, where and how long ago, drawn as a fading flash.
    private readonly List<(Vector3 At, float Power, float Age)> _blasts = [];

    public DropRenderer DropRenderer { get; } = new();

    public Inventory Inventory { get; } = new();

    /// <summary>The player's health, food and breath, which change in survival alone.</summary>
    public Vitals Vitals { get; } = new();

    /// <summary>How far through eating the held food the player is, from 0 to 1, while the right button is held.</summary>
    public float Eating { get; private set; }

    public ChunkStreamer Streamer { get; private set; } = null!;

    /// <summary>Where the world is saved, or null for a transient one.</summary>
    public WorldSave? Save { get; private set; }

    public ChunkRenderer Renderer { get; }

    public FirstPersonController Player { get; } = new();

    public Sky Sky { get; }

    public Clouds Clouds { get; } = new();

    public LightSettings Light { get; } = new();

    public Hotbar Hotbar { get; }

    public InventoryWindow InventoryWindow { get; } = new();

    /// <summary>
    /// How the player plays, which in survival and adventure stops flight and makes blocks take time
    /// to break and drop items, and in spectator flies the player through the blocks.
    /// </summary>
    public GameMode Mode
    {
        get => _mode;
        set
        {
            if (value != _mode) PreviousMode = _mode;
            _mode = value;
            Player.MayFly = value == GameMode.Creative;
            Player.Spectating = value == GameMode.Spectator;
            if (value == GameMode.Spectator) Player.Flying = true;
            else if (value != GameMode.Creative) Player.Flying = false;
            if (!value.Touches() && InventoryOpen) CloseInventory();
            (BreakingAt, BreakProgress) = (null, 0);
        }
    }

    /// <summary>The mode played before the one now, which the mode switcher opens on, or null before the first change.</summary>
    public GameMode? PreviousMode { get; private set; }

    /// <summary>The switcher F3 held with F4 opens.</summary>
    public ModeSwitcher Switcher { get; } = new();

    /// <summary>How much of the hearts, the food and the breath the HUD shows, each fading once full and unchanged.</summary>
    public VitalsFade Fade { get; } = new();

    /// <summary>The menu Escape opens and its options, which hold the world still while open.</summary>
    public Menus Menus { get; } = new();

    /// <summary>The block being broken in survival, and how far from 0 to 1, as of this frame.</summary>
    public (int X, int Y, int Z)? BreakingAt { get; private set; }

    public float BreakProgress { get; private set; }

    public SettingsWindow Settings { get; } = new();

    /// <summary>How many columns around the player are drawn, and two more loaded.</summary>
    /// <remarks>
    /// Twelve by default, 192 blocks. Over the first seed's spawn at noon, seen from above the
    /// forest at 1280 by 720 on the development machine's RTX 4070 Laptop GPU, the frame holds 60 a
    /// second there with 6.9 ms of each frame to spare, its GPU's work 9.5 ms, where 8 columns take
    /// 6.6 with 11.3 to spare, 14 take 11.3 with 3.2 to spare and 16 take 12.8 and lose the 60 a
    /// second, the main thread bounding the frame, the game's update and drawing with the engine's
    /// render work. So 12 keeps twice the margin of 14, the most that holds. Every section the eye
    /// can see within it is drawn each frame, since one left out leaves the scene field, and each of
    /// the engine's passes leaves out the sections whose boxes it cannot see, the terrain's shader
    /// keeping its positions, so the shadows, though they reach 96 blocks, take 1.2 ms at 12 columns
    /// and at 16, and global illumination 1.4. Flying over land not yet loaded, the main thread
    /// meshes and joins the columns that come into reach, and the frame takes 18 to 19 ms at 8
    /// columns and 21 to 29 at 12.
    /// </remarks>
    public int RenderDistance { get; set; } = 12;

    /// <summary>The block the crosshair rests on, as of this frame, or null where a mob stands before it.</summary>
    public BlockHit? Target { get; private set; }

    /// <summary>The mob the crosshair rests on within <see cref="Mobs.Reach"/>, as of this frame, nearer than any block.</summary>
    public Mob? TargetMob { get; private set; }

    public bool HudHidden { get; set; }

    /// <summary>Where F2 writes its screenshots.</summary>
    public Screenshots Shots { get; } = Screenshots.Default;

    // The line on the HUD saying where the last screenshot went and how long it has shown, and
    // whether it was said this frame, which the picture is of, so the line shows from the next.
    private (string Text, float Age)? _said;
    private bool _saidFresh;

    // F2 writes the frame as a screenshot, as Minecraft's does, but in a transient run, which
    // writes nowhere, and says so on the HUD.
    private void Screenshot()
    {
        _said = (_transient ? "Screenshots are not kept in a transient run" : $"Saved screenshot as {Shots.Take()}", 0);
        _saidFresh = true;
    }

    /// <summary>Whether the world fades into the sky's color toward the render distance, as Minecraft's fog hides where the world ends.</summary>
    public bool Fog { get; set; } = true;

    /// <summary>Whether the window E opens is open, the inventory in survival and every item in creative, or a crafting table's.</summary>
    public bool InventoryOpen { get; private set; }

    /// <summary>Whether the window open is a crafting table's, which a right click on one opens.</summary>
    public bool AtTable { get; private set; }

    /// <summary>The place of the furnace whose window is open, which a right click on one opens, or null.</summary>
    public (int X, int Y, int Z)? AtFurnace { get; private set; }

    /// <summary>A recording of the picture's change around a motion, which <c>voxel.flicker</c> starts, read a frame at a time.</summary>
    public FlickerRun? Flicker { get; set; }

    /// <summary>A world asked for by a command or the settings window by its kind and seed, opened at the start of the next frame.</summary>
    public (string Kind, int Seed)? NextWorld { get; set; }

    /// <summary>Saves the world loaded and opens the one saved under a kind and seed, or begins it.</summary>
    public void SwitchWorld(string kind, int seed)
    {
        SaveWorld();
        Open(kind, seed, null);
    }

    private void Open(string kind, int seed, string? name)
    {
        Save = _transient ? null : new WorldSave(Path.Combine(WorldSave.Root, name ?? $"{kind}-{seed}"));
        var info = Save?.ReadInfo();
        var generator = (info is null ? null : Generators.Create(info.Kind, info.Seed)) ?? Generators.Create(kind, seed) ?? new Overworld(seed);
        Renderer.Clear();
        World = new VoxelWorld(generator);
        var (kept, placed) = Save?.ReadMobs() ?? ([], []);
        Mobs = new Mobs(kept, placed);
        Drops.Clear();
        Arrows.Clear();
        Furnaces = Save?.ReadFurnaces() ?? new Furnaces();
        _blasts.Clear();
        // The mode and the inventory are the world's own, a new world's in creative with its hotbar of blocks.
        Mode = Enum.TryParse<GameMode>(info?.Mode, ignoreCase: true, out var mode) ? mode : GameMode.Creative;
        PreviousMode = null;
        Inventory.Clear();
        if (info?.Inventory is { } slots) Inventory.Load(slots);
        else if (Mode == GameMode.Creative) Hotbar.FillCreative();
        Hotbar.Selected = Math.Clamp(info?.Selected ?? 0, 0, Hotbar.Size - 1);
        if (info is null) Vitals.Set();
        else Vitals.Set(info.Health, info.Food, info.Saturation);
        // The old streamer's workers finish into its own queue, which nothing reads again.
        Streamer = new ChunkStreamer(World, Renderer, Mobs, Save);
        Clouds.Follow(generator.Seed);
        _sinceSave = 0;

        if (info is null)
        {
            // A new world's player stands on the highest block at the origin.
            Streamer.LoadNow(Vector3.Zero, 2);
            Player.Flying = false;
            Player.Teleport(new Vector3(0.5f, World.Top(0, 0) + 1, 0.5f));
            return;
        }
        var at = new Vector3(info.X, info.Y, info.Z);
        Streamer.LoadNow(at, 2);
        Player.Teleport(at);
        (Player.Heading, Player.Pitch, Player.Flying, Sky.Hour, Sky.Day) = (info.Heading, info.Pitch * MathF.PI / 180, info.Flying, info.Hour, info.Day);
    }

    /// <summary>Keeps every loaded column the player changed and the player's place, and writes them, unless the world is transient.</summary>
    public void SaveWorld()
    {
        if (Save is null) return;
        foreach (var column in World.Columns)
            if (column.Changed) Save.Keep(column);
        Save.Flush();
        Save.WriteMobs(Mobs.Columns());
        Save.WriteFurnaces(Furnaces);
        var at = Player.Body.Position;
        Save.WriteInfo(new WorldInfo(World.Generator.Name, World.Generator.Seed, at.X, at.Y, at.Z,
            Player.Heading, Player.Pitch * 180 / MathF.PI, Player.Flying, Sky.Hour, [.. Blocks.All.Select(b => b.Key)],
            Mode.ToString().ToLowerInvariant(), Inventory.Save(), Hotbar.Selected, Vitals.Health, Vitals.Food, Vitals.Saturation, Sky.Day));
        _sinceSave = 0;
    }

    public void Update()
    {
        TakeDefaults();
        if (NextWorld is { } next)
        {
            NextWorld = null;
            SwitchWorld(next.Kind, next.Seed);
        }
        if (Flicker is { } run && !run.Step(this)) Flicker = null;
        // A long frame, as when the window is dragged, is taken as a short one, so the player does
        // not fall through the time it took.
        var seconds = MathF.Min(GetFrameTime(), 0.05f);
        // The menu holds the world still, as Minecraft's does when played alone, the frame taking
        // no time, as the game's first does.
        if (Menus.Open != Menu.None) seconds = 0;
        _sinceSave += seconds;
        if (_sinceSave > SaveSeconds) SaveWorld();
        var io = ImGui.GetIO();
        var keys = !io.WantTextInput;

        if (keys) Toggles();
        // A click takes the cursor back unless it falls on a window of the game's own or of ImGui's.
        if (!_captured && IsMouseButtonPressed(MouseButton.Left) && !io.WantCaptureMouse && !InventoryOpen && Menus.Open == Menu.None)
        {
            Capture(true);
            _swallowClick = true;
        }
        if (_swallowClick && !IsMouseButtonDown(MouseButton.Left)) _swallowClick = false;

        // The number keys choose a hotbar slot unless a window of slots takes them to swap one, the
        // inventory, a crafting table or a furnace, where the creative window of items leaves them.
        var slotsShown = InventoryOpen && (Mode.Lives() || AtTable || AtFurnace is not null);
        Hotbar.Update(keys && !slotsShown, _captured);
        var alive = !Mode.Lives() || !Vitals.Dead;
        Player.MaySprint = !Mode.Lives() || Vitals.MaySprint;
        Player.Update(World, seconds, look: _captured && alive, move: keys && alive);
        if (Mode.Lives())
        {
            Live(seconds);
            Fade.Update(Vitals, seconds);
        }
        else Fade.Reset();
        var hunted = Mode.Lives() && !Vitals.Dead;
        Mobs.Update(World, Player.Body, Player.Eye, Sky.Hour, seconds, hunted);
        if (Mode.Lives()) Mobs.Spawn(World, Player.Body.Position, Sky.Hour, seconds);
        Answer(seconds);
        Aim();
        if (_captured && !_swallowClick && alive && Mode.Touches()) Interact(seconds);
        else (BreakingAt, BreakProgress, Eating) = (null, 0, 0);
        if (_captured && keys && Mode.Touches() && IsKeyPressed(Key.Q)) Throw(IsKeyDown(Key.LeftControl));
        // What Q dropped from a slot of the window E opens is thrown ahead, as from the hotbar.
        foreach (var stack in InventoryWindow.Dropped)
        {
            var look = Vector3.Normalize(Player.Look);
            Drops.Spawn(stack.Item, stack.Count, Player.Eye - new Vector3(0, 0.3f, 0) + look * 0.3f, look * 6 + new Vector3(0, 1, 0), ItemDrops.ThrownDelay, stack.Wear);
        }
        InventoryWindow.Dropped.Clear();
        LeaveDrops();
        foreach (var ((x, y, z), stack) in Furnaces.Update(World, seconds))
        {
            if (AtFurnace == (x, y, z)) CloseInventory();
            Drops.Spawn(stack.Item, stack.Count, new Vector3(x + 0.5f, y + 0.5f, z + 0.5f), new Vector3(0, 2, 0), wear: stack.Wear);
        }
        Drops.Update(World, Player.Body, Inventory, seconds, pickUp: alive && Mode.Touches());

        Sky.Update(seconds);
        World.Fluids.Update(seconds);
        Clouds.Update(seconds);
        Streamer.Update(Player.Body.Position, RenderDistance);
        Renderer.Update(World, Player.Eye, MeshBudgetMs);
    }

    // The vitals' frame in survival, reckoned afresh after a teleport, and on death the inventory
    // spilled where the player fell and the cursor freed for the screen that offers a new life.
    private void Live(float seconds)
    {
        if (Player.Teleports != _teleports)
        {
            _teleports = Player.Teleports;
            Vitals.Arrived();
        }
        Vitals.Update(World, Player.Body, Player.Eye, Player.Swimming, Player.Flying, Player.Sprinting, seconds);
        // A death is met once, however it came, in this frame or by a command before it.
        if (!Vitals.Dead || _spilled)
        {
            _spilled &= Vitals.Dead;
            return;
        }
        _spilled = true;
        if (InventoryOpen) CloseInventory();
        for (int i = 0; i < Inventory.Size; i++)
            if (Inventory.Slots[i] is { } stack)
            {
                var angle = (float)_random.NextDouble() * MathF.Tau;
                Drops.Spawn(stack.Item, stack.Count, Player.Body.Position + new Vector3(0, 1, 0), new Vector3(MathF.Cos(angle) * 2, 3, MathF.Sin(angle) * 2), wear: stack.Wear);
            }
        Inventory.Clear();
        Capture(false);
    }

    /// <summary>Gives the player a new life at the world's spawn, the highest block at the origin, with full health and food.</summary>
    public void Respawn()
    {
        Vitals.Set();
        Streamer.LoadNow(Vector3.Zero, 2);
        Player.Flying = false;
        Player.Teleport(new Vector3(0.5f, World.Top(0, 0) + 1, 0.5f));
        Capture(true);
    }

    // Carries out what the hostile mobs did, their strikes, shots and blasts, and flies the arrows,
    // each striking the player for its damage and a knock away from where it came.
    private void Answer(float seconds)
    {
        foreach (var e in Mobs.Events)
            switch (e)
            {
                case MobStrike strike:
                    Harm(strike.Damage, strike.From, $"were slain by a {strike.Mob.Kind.Name}");
                    break;
                case MobShot shot:
                    Arrows.Shoot(shot.From, shot.Velocity);
                    break;
                case MobBlast blast:
                    Explode(blast.At, blast.Power, $"were blown up by a {blast.Mob.Kind.Name}");
                    break;
            }
        Mobs.Events.Clear();
        foreach (var (damage, from) in Arrows.Update(World, Player.Body, seconds)) Harm(damage, from, "were shot by a skeleton");
        for (int i = _blasts.Count - 1; i >= 0; i--)
        {
            _blasts[i] = _blasts[i] with { Age = _blasts[i].Age + seconds };
            if (_blasts[i].Age > 0.5f) _blasts.RemoveAt(i);
        }
    }

    // Harms the player in survival and knocks it away from a place, as a mob's hit does.
    private void Harm(int damage, Vector3 from, string cause)
    {
        if (!Mode.Lives() || !Vitals.Damage(damage, cause)) return;
        var away = Player.Body.Position - from;
        away.Y = 0;
        if (away.LengthSquared() > 1e-6f) away = Vector3.Normalize(away);
        Player.Body.Velocity += new Vector3(away.X * 6, 5, away.Z * 6);
    }

    /// <summary>
    /// Sets off an explosion of a power at a place, as a creeper's of 3, which breaks the blocks
    /// its rays reach, drops a third of them in survival, and harms and throws the player and the
    /// mobs near it by its impact, as Minecraft's explosions do.
    /// </summary>
    public void Explode(Vector3 at, float power, string cause)
    {
        var survival = Mode.Lives();
        var pickaxe = Items.Find("stone_pickaxe");
        foreach (var (x, y, z) in Explosion.Breaks(World, at, power, _random))
        {
            var block = World.GetBlock(x, y, z);
            if (!World.SetBlock(x, y, z, BlockId.Air) || !survival || _random.NextDouble() >= 1 / power) continue;
            foreach (var (item, count) in Loot.Of(block, _random, pickaxe)) Drops.Pop(item, count, x, y, z);
        }
        var middle = Player.Body.Position + new Vector3(0, Player.Body.Height / 2, 0);
        var distance = Vector3.Distance(middle, at);
        var exposure = Explosion.Exposure(World, at, Player.Body.Min, Player.Body.Max);
        if (survival && Explosion.Damage(distance, power, exposure) is > 0 and var damage && Vitals.Damage(damage, cause))
            Player.Body.Velocity += Vector3.Normalize(middle - at + new Vector3(0, 0.5f, 0)) * Explosion.Impact(distance, power, exposure) * 12;
        foreach (var mob in Mobs.All)
        {
            var near = Vector3.Distance(mob.Body.Position + new Vector3(0, mob.Kind.Height / 2, 0), at);
            var harm = Explosion.Damage(near, power, Explosion.Exposure(World, at, mob.Body.Min, mob.Body.Max));
            if (harm > 0) mob.Hit(at, harm);
        }
        _blasts.Add((at, power, 0));
    }

    // What the mobs that died and, in survival, the blocks that fell with the block they held to leave
    // on the ground.
    private void LeaveDrops()
    {
        foreach (var mob in Mobs.Died)
            foreach (var (item, count) in Loot.Of(mob, _random))
                Drops.Spawn(item, count, mob.Body.Position + new Vector3(0, 0.25f, 0), new Vector3(0, 3, 0));
        Mobs.Died.Clear();
        if (Mode.Lives())
            foreach (var (x, y, z, block) in World.Fallen)
                foreach (var (item, count) in Loot.Of(block, _random))
                    Drops.Pop(item, count, x, y, z);
        World.Fallen.Clear();
    }

    /// <summary>Throws one of the chosen slot's items ahead of the player, or its whole stack, as Q and Control with Q do.</summary>
    public bool Throw(bool all)
    {
        if (Hotbar.Current is not { } stack) return false;
        var count = Inventory.Take(Hotbar.Selected, all ? stack.Count : 1);
        var look = Vector3.Normalize(Player.Look);
        Drops.Spawn(stack.Item, count, Player.Eye - new Vector3(0, 0.3f, 0) + look * 0.3f, look * 6 + new Vector3(0, 1, 0), ItemDrops.ThrownDelay, stack.Wear);
        return true;
    }

    // The block and the mob the crosshair rests on, a mob before a block hiding it.
    private void Aim()
    {
        Target = VoxelRay.Cast(World, Player.Eye, Player.Look, Reach);
        TargetMob = null;
        if (Mobs.Cast(Player.Eye, Player.Look, Mobs.Reach) is { } met && (Target is not { } hit || met.Distance < hit.Distance))
        {
            TargetMob = met.Mob;
            Target = null;
        }
    }

    private void Toggles()
    {
        // Escape shuts the window open, goes back a menu, or opens the menu over the game.
        if (IsKeyPressed(Key.Escape))
        {
            if (InventoryOpen)
            {
                CloseInventory();
                Capture(!Settings.Open);
            }
            else if (Menus.Open != Menu.None)
            {
                Menus.Back();
                Capture(Menus.Open == Menu.None && !Settings.Open);
            }
            else
            {
                Menus.Open = Menu.Pause;
                Capture(false);
            }
        }
        if (IsKeyPressed(Key.E) && Mode.Touches() && Menus.Open == Menu.None)
        {
            if (InventoryOpen) CloseInventory();
            else InventoryOpen = true;
            Capture(!InventoryOpen && !Settings.Open);
        }
        // F3 held with F4 opens the mode switcher and steps it, and letting F3 go plays the mode
        // chosen; F3 let go with no F4 pressed while it was held opens or shuts the settings, as
        // Minecraft's debug screen opens when F3 is let go with no other key.
        if (IsKeyDown(Key.F3) && IsKeyPressed(Key.F4))
        {
            Switcher.Step(Mode, PreviousMode);
            _switched = true;
        }
        // F3 held with T reads the resource packs again, as Minecraft's does.
        if (IsKeyDown(Key.F3) && IsKeyPressed(Key.T))
        {
            ReloadPacks();
            _switched = true;
        }
        if (IsKeyReleased(Key.F3))
        {
            if (Switcher.Release() is { } chosen) Mode = chosen;
            else if (!_switched)
            {
                Settings.Open = !Settings.Open;
                Capture(!Settings.Open && !InventoryOpen);
            }
            _switched = false;
        }
        if (IsKeyPressed(Key.F1)) HudHidden = !HudHidden;
        if (IsKeyPressed(Key.F2)) Screenshot();
        if (_said is var (said, age)) _said = age + GetFrameTime() < Hud.SaidSeconds ? (said, age + GetFrameTime()) : null;
        if (IsKeyPressed(Key.G)) Light.NextQuality();
    }

    private void Capture(bool captured)
    {
        _captured = captured;
        if (captured) DisableCursor();
        else EnableCursor();
    }

    // Closes the window E opens, the stack held in it going back into the inventory and what does
    // not fit thrown.
    private void CloseInventory()
    {
        (InventoryOpen, AtTable, AtFurnace) = (false, false, null);
        foreach (var left in InventoryWindow.Close(Inventory))
            Drops.Spawn(left.Item, left.Count, Player.Eye - new Vector3(0, 0.3f, 0), Vector3.Normalize(Player.Look) * 3, ItemDrops.ThrownDelay, left.Wear);
    }

    private void Interact(float seconds)
    {
        _breakWait -= seconds;
        _placeWait -= seconds;
        if (IsMouseButtonPressed(MouseButton.Left)) _breakWait = 0;
        if (IsMouseButtonPressed(MouseButton.Right)) _placeWait = 0;

        if (Mode == GameMode.Creative && IsMouseButtonDown(MouseButton.Left) && _breakWait <= 0)
        {
            _breakWait = Repeat;
            if (!Attack()) Break();
        }
        if (Mode == GameMode.Survival) Dig(seconds);
        // In adventure a click hits a mob and breaks nothing.
        if (Mode == GameMode.Adventure && IsMouseButtonPressed(MouseButton.Left)) Attack();
        if (Mode.Lives() && Eat(seconds)) return;
        if (IsMouseButtonDown(MouseButton.Right) && _placeWait <= 0)
        {
            _placeWait = Repeat;
            Use();
        }
        if (IsMouseButtonPressed(MouseButton.Middle) && Target is { } hit)
            Hotbar.Pick(Items.Of(World.GetBlock(hit.X, hit.Y, hit.Z)), Mode == GameMode.Creative);
    }

    // Holding the right button with food while hungry eats it over a second and a half, unless the
    // crosshair rests on a door, a crafting table or a furnace, which the button opens instead.
    // Whether the player is eating.
    private bool Eat(float seconds)
    {
        var opens = Target is { } hit && World.GetBlock(hit.X, hit.Y, hit.Z) is var block && (block == BlockId.CraftingTable || Furnaces.IsFurnace(block) || Blocks.Get(block).Toggled is not null);
        if (!IsMouseButtonDown(MouseButton.Right) || opens || Hotbar.Current is not { Item.Food: > 0 } food || Vitals.Food >= Vitals.Most)
        {
            Eating = 0;
            return false;
        }
        Eating += seconds / EatSeconds;
        if (Eating < 1) return true;
        Eating = 0;
        if (Vitals.Eat(food.Item)) Inventory.Take(Hotbar.Selected);
        return true;
    }

    // In survival a click hits a mob, and holding the button on a block breaks it over the time its
    // hardness takes, from the start again where the crosshair leaves it, and waits a quarter
    // second before the next, as Minecraft's does.
    private void Dig(float seconds)
    {
        if (!IsMouseButtonDown(MouseButton.Left))
        {
            (BreakingAt, BreakProgress) = (null, 0);
            return;
        }
        if (IsMouseButtonPressed(MouseButton.Left) && Attack() || TargetMob is not null || _breakWait > 0) return;
        if (Target is not { } hit || !Blocks.Get(World.GetBlock(hit.X, hit.Y, hit.Z)).Breakable)
        {
            (BreakingAt, BreakProgress) = (null, 0);
            return;
        }
        if (BreakingAt != (hit.X, hit.Y, hit.Z)) (BreakingAt, BreakProgress) = ((hit.X, hit.Y, hit.Z), 0);
        var time = Loot.BreakSeconds(World.GetBlock(hit.X, hit.Y, hit.Z), Hotbar.Current?.Item);
        BreakProgress += time <= 0 ? 1 : seconds / time;
        if (BreakProgress < 1) return;
        Break();
        (BreakingAt, BreakProgress, _breakWait) = (null, 0, Repeat);
    }

    /// <summary>
    /// Opens the crafting table or the furnace the crosshair rests on, opens or shuts the door it rests on, or places
    /// the chosen slot's block against the face it rests on, using one up in survival, as a right
    /// click does, placing nothing in adventure and doing nothing in spectator.
    /// </summary>
    /// <returns>The table or furnace opened, the door's half turned or the block placed, or air where nothing happened.</returns>
    public BlockId Use()
    {
        if (!Mode.Touches()) return BlockId.Air;
        if (Target is { } table && World.GetBlock(table.X, table.Y, table.Z) == BlockId.CraftingTable)
        {
            (InventoryOpen, AtTable) = (true, true);
            Capture(false);
            return BlockId.CraftingTable;
        }
        if (Target is { } oven && Furnaces.IsFurnace(World.GetBlock(oven.X, oven.Y, oven.Z)))
        {
            Furnaces.At(oven.X, oven.Y, oven.Z);
            (InventoryOpen, AtFurnace) = (true, (oven.X, oven.Y, oven.Z));
            Capture(false);
            return World.GetBlock(oven.X, oven.Y, oven.Z);
        }
        var toggles = Target is { } door && Blocks.Get(World.GetBlock(door.X, door.Y, door.Z)).Toggled is not null;
        // In adventure a door opens or shuts and nothing is placed.
        if (!Mode.Builds())
            return Target is { } shut && World.Toggle(shut.X, shut.Y, shut.Z) ? World.GetBlock(shut.X, shut.Y, shut.Z) : BlockId.Air;
        var placed = Place(Hotbar.CurrentBlock);
        if (Mode == GameMode.Survival && !toggles && placed != BlockId.Air) Inventory.Take(Hotbar.Selected);
        return placed;
    }

    /// <summary>
    /// Hits the mob the crosshair rests on with the item held, a hand's one point for an item that is
    /// not a weapon or a tool, unless it is still red from the last hit, wearing a sword by one use
    /// and another tool by two, as Minecraft's do.
    /// </summary>
    public bool Attack()
    {
        if (TargetMob is not { } mob || !mob.Hit(Player.Body.Position, Hotbar.Current?.Item.Damage ?? 1)) return false;
        Vitals.Exhaust(0.1f);
        if (Mode.Lives() && Hotbar.Current?.Item.Tool is { } tool and not ToolKind.None) Wear(tool == ToolKind.Sword ? 1 : 2);
        return true;
    }

    // Wears the chosen slot's tool by some uses, taking it away once they are all worn.
    private void Wear(int uses)
    {
        if (Hotbar.Current is not { Item.Durability: > 0 } stack) return;
        Inventory.Slots[Hotbar.Selected] = stack.Wear + uses >= stack.Item.Durability ? null : stack with { Wear = stack.Wear + uses };
    }

    /// <summary>
    /// Breaks the block the crosshair rests on at once, unless it cannot be broken or the mode breaks
    /// none, as adventure and spectator do, and in survival drops what it leaves to the item held and
    /// wears a tool held by a use, a sword by two, as Minecraft's do for a block that does not break
    /// at once.
    /// </summary>
    public bool Break()
    {
        if (!Mode.Builds() || Target is not { } hit) return false;
        var block = World.GetBlock(hit.X, hit.Y, hit.Z);
        if (!Blocks.Get(block).Breakable || !World.SetBlock(hit.X, hit.Y, hit.Z, BlockId.Air)) return false;
        if (Mode != GameMode.Survival) return true;
        Vitals.Exhaust(0.005f);
        var held = Hotbar.Current?.Item;
        foreach (var (item, count) in Loot.Of(block, _random, held)) Drops.Pop(item, count, hit.X, hit.Y, hit.Z);
        if (held is { Tool: not ToolKind.None } tool && Loot.Hardness(block) > 0) Wear(tool.Tool == ToolKind.Sword ? 2 : 1);
        return true;
    }

    /// <summary>
    /// Places a block against the face the crosshair rests on, unless it would take up some of the
    /// player or of a mob. A torch or a lantern is placed the way of it that has a block to hold
    /// to, stairs climbing the way the player looks, a door two blocks high, and a slab on the open
    /// half of a slab of its kind joined to it as the whole block. A door met is opened or shut
    /// instead.
    /// </summary>
    /// <returns>The block placed, which for a torch on a wall is the wall torch, or air where none was.</returns>
    public BlockId Place(BlockId block)
    {
        if (Target is not { Inside: false } hit) return BlockId.Air;
        var met = Player.Eye + Vector3.Normalize(Player.Look) * hit.Distance;
        var height = met.Y - MathF.Floor(met.Y);
        var look = MathF.Abs(Player.Look.X) > MathF.Abs(Player.Look.Z)
            ? Player.Look.X > 0 ? Facing.East : Facing.West
            : Player.Look.Z > 0 ? Facing.South : Facing.North;

        // A door opens or shuts, whatever is held.
        var under = World.GetBlock(hit.X, hit.Y, hit.Z);
        if (World.Toggle(hit.X, hit.Y, hit.Z)) return World.GetBlock(hit.X, hit.Y, hit.Z);

        // A slab met on its open half, its top for the lower one and its underside for the upper.
        if (Blocks.Get(under) is { Item: var item, Doubled: { } whole, Shape: { } slab } && item == block && hit.NormalY == (slab.Upper ? -1 : 1))
            return World.SetBlock(hit.X, hit.Y, hit.Z, whole) ? whole : BlockId.Air;

        var (x, y, z) = hit.Beside;
        var there = World.GetBlock(x, y, z);
        // A door stands two blocks high, shut and facing the way the player looks, on a whole block.
        if (Blocks.Get(block).Toggled is not null)
        {
            BlockId lower = Blocks.Variant(block, look), upper = Blocks.Variant(block, look, true);
            if (there != BlockId.Air || World.GetBlock(x, y + 1, z) != BlockId.Air || !Blocks.IsSturdy(World.GetBlock(x, y - 1, z))
                || Player.Body.Overlaps(x, y, z, Blocks.Get(lower).Boxes) || Player.Body.Overlaps(x, y + 1, z, Blocks.Get(upper).Boxes)
                || Mobs.Overlaps(x, y, z, Blocks.Get(lower).Boxes) || Mobs.Overlaps(x, y + 1, z, Blocks.Get(upper).Boxes)) return BlockId.Air;
            World.SetBlock(x, y + 1, z, upper);
            return World.SetBlock(x, y, z, lower) ? lower : BlockId.Air;
        }
        var placed = World.Fit(block, x, y, z, hit.NormalX, hit.NormalY, hit.NormalZ, look, height);
        if (placed == BlockId.Air) return BlockId.Air;
        // A slab placed into the other half of a slab of its kind beside the face met fills the block.
        if (Blocks.Get(there) is { Item: var beside, Doubled: { } joined, Shape: { } half } && beside == block && Blocks.Get(placed).Shape?.Upper != half.Upper)
            placed = joined;
        // A whole block goes into air or water, the water giving way to it, and a smaller one into air.
        else if (there != BlockId.Air && !Blocks.IsWater(there) || Blocks.Get(placed).Shape is not null && there != BlockId.Air) return BlockId.Air;
        if (Player.Body.Overlaps(x, y, z, Blocks.Get(placed).Boxes) || Mobs.Overlaps(x, y, z, Blocks.Get(placed).Boxes)) return BlockId.Air;
        return World.SetBlock(x, y, z, placed) ? placed : BlockId.Air;
    }

    public void Draw()
    {
        // The haze begins two thirds of the way to the render distance and covers the world at it,
        // and under water it is the water's blue and covers it within 16 blocks.
        var reach = RenderDistance * Section.Size;
        if (Player.EyeInWater) Renderer.Fog(new Color(24, 52, 120), 2, 16);
        else if (Fog) Renderer.Fog(Sky.Horizon, reach * 0.65f, reach * 0.95f);
        else Renderer.Fog(Sky.Horizon, float.MaxValue, float.MaxValue);

        BeginDrawing();
        ClearBackground(Sky.Horizon);
        BeginMode3D(Player.Camera);
        DrawSkybox();
        Sky.Draw(Player.Eye, Renderer.Hazed);
        Renderer.Draw(World, Player.Eye, RenderDistance);
        MobRenderer.Draw(Mobs.All, Player.Eye);
        DropRenderer.Draw(Drops.All, Player.Eye, GetTime());
        DrawArrows();
        // An explosion's flash, a ball swelling to the power's size and fading over half a second.
        foreach (var (at, power, age) in _blasts)
            DrawSphere(at, power * (0.4f + age), new Color(255, 236, 200, (byte)(220 * (1 - age / 0.5f))));
        Clouds.Draw(Player.Eye, RenderDistance, Sky.Horizon);
        if (Target is { } hit && !HudHidden && Mode.Touches())
        {
            Hud.DrawOutline(hit, World.ShapeAt(hit.X, hit.Y, hit.Z));
            if (BreakingAt == (hit.X, hit.Y, hit.Z)) Hud.DrawBreaking(hit, World.ShapeAt(hit.X, hit.Y, hit.Z), BreakProgress);
        }
        EndMode3D();

        // Under water the view is tinted blue, as Minecraft's is.
        if (Player.EyeInWater) DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), new Color(20, 50, 140, 110));
        // A hit reddens the view for the half second no other harms.
        if (Mode.Lives() && Vitals.Hurt > 0) DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), new Color(200, 0, 0, (byte)(90 * Vitals.Hurt / 0.5f)));
        if (!HudHidden)
        {
            if (Mode.Touches()) Hud.DrawCrosshair();
            if (Eating > 0) Hud.DrawEating(Eating);
            if (Mode.Touches()) Hud.DrawHotbar(Hotbar, Mode.Lives());
            if (Mode.Lives()) Hud.DrawVitals(Vitals, Fade);
            if (!Settings.Open) Hud.DrawHelp(Light.Quality, Mode);
            if (_said is var (said, age) && !_saidFresh) Hud.DrawSaid(said, age, Mode.Lives());
        }
        _saidFresh = false;
        Switcher.Draw();
        // The engine's performance window opens with F2, which the game takes for its screenshots,
        // so the window is kept far out of sight, what it shows drawn there, until the engine's
        // package gives it a key of its own.
        ImGui.SetNextWindowPos(new Vector2(-10000, -10000), ImGuiCond.Always);
        ImGui.Begin("Performance", ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav);
        ImGui.End();
        Settings.Draw(this);
        if (Mode.Lives() && Vitals.Dead) DrawDeath();
        if (InventoryOpen && !(AtFurnace is var (fx, fy, fz) ? InventoryWindow.DrawFurnace(Inventory, Furnaces.At(fx, fy, fz))
            : AtTable ? InventoryWindow.DrawInventory(Inventory, InventoryWindow.Table)
            : Mode.Lives() ? InventoryWindow.DrawInventory(Inventory, InventoryWindow.Small)
            : InventoryWindow.DrawCreative(Hotbar)))
        {
            CloseInventory();
            Capture(!Settings.Open);
        }
        var menu = Menus.Open;
        Menus.Draw(this);
        // A button that shut the menu gives the cursor back to the game.
        if (menu != Menu.None && Menus.Open == Menu.None) Capture(!Settings.Open);
        EndDrawing();
    }

    // Each arrow a thin shaft along its flight, or along its last flight once stuck, with grey fletching at its tail.
    private void DrawArrows()
    {
        foreach (var arrow in Arrows.All)
        {
            if (Vector3.DistanceSquared(arrow.Position, Player.Eye) > 64 * 64) continue;
            var way = arrow.Velocity.LengthSquared() > 1e-6f ? Vector3.Normalize(arrow.Velocity) : Vector3.UnitY;
            var tail = arrow.Position - way * 0.5f;
            DrawCylinderEx(tail, arrow.Position, 0.025f, 0.02f, 4, new Color(112, 84, 52));
            DrawCylinderEx(tail, tail + way * 0.12f, 0.06f, 0.03f, 4, new Color(200, 200, 200));
        }
    }

    // The screen of a death, the view dimmed red under what took the player's life and a button for a new one.
    private void DrawDeath()
    {
        DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), new Color(110, 0, 0, 120));
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        if (ImGui.Begin("You died", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove))
        {
            ImGui.TextUnformatted($"You {Vitals.Cause ?? "died"}.");
            if (ImGui.Button("Respawn", new Vector2(200, 0))) Respawn();
        }
        ImGui.End();
    }

    public void Dispose()
    {
        SaveWorld();
        Renderer.Dispose();
        MobRenderer.Dispose();
        DropRenderer.Dispose();
        Clouds.Dispose();
        Sky.Dispose();
        Packs.Dispose();
    }
}
