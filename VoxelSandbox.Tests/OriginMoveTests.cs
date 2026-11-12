using System.Numerics;
using Engine;
using Xunit;
using static Engine.E3D;

namespace VoxelSandbox.Tests;

/// <summary>
/// The <see cref="LandOrigin"/> moved under a game, every thing the game places kept where it stood
/// in the land, what was sent before the move carried out where it was sent from, and what a far
/// jump leaves kept by its records.
/// </summary>
[Collection(nameof(HeadlessGames))]
public sealed class OriginMoveTests : IDisposable
{
    // The flat world's feet, over its grass at 3.
    private const double Feet = 4;

    public void Dispose()
    {
        Serve(null);
        LandOrigin.Near(0, 0);
    }

    // The game the commands answer of, as the program sets it, which a test reaches through the
    // setter the program uses.
    private static void Serve(VoxelGame? game) => typeof(GameCommands).GetProperty(nameof(GameCommands.Game))!.SetValue(null, game);

    private static string Run(FormattableString line) => ConsoleCommands.Run(FormattableString.Invariant(line)) ?? "";

    // A game put 1,000 blocks east, within reach of the origin at 0, so a move to 1,024 leaves the
    // player within reach of it and the next frame keeps it. A float place between 512 and 2,048
    // less 1,024 holds whole, so every place about the player moves exactly.
    private static HeadlessGame PutNearTheLine()
    {
        var headless = new HeadlessGame(at: new LandPlace(1000.5, Feet, 0.5));
        Serve(headless.Game);
        headless.Game.Mobs.Still = true;
        Assert.Equal(0, LandOrigin.X);
        return headless;
    }

    [Fact]
    public void A_Move_Keeps_Every_Thing_The_Game_Places_Where_It_Stood_In_The_Land()
    {
        using var headless = PutNearTheLine();
        var (game, ecs) = (headless.Game, headless.Ecs);

        // A lamp, a pool raining rings, a villager kept to a home, an arrow, a blast, a block's bits
        // and a dropped stack about the player.
        Assert.StartsWith("glowstone is at", Run($"voxel.set 1002 4 2 glowstone"));
        Run($"voxel.fill 994 3 -6 998 3 6 water");
        Run($"voxel.weather turned");
        for (int frame = 0; frame < 600 && game.Particles.WaterRings.Count == 0; frame++) headless.Step(1);
        Assert.NotEmpty(game.Particles.WaterRings);
        var villager = game.Mobs.Add(new MobRecord(MobKinds.Villager, 0, new LandPlace(1003.25, Feet, 0.75), 0, MobKinds.Villager.Health, new LandPlace(1010.5, Feet, -3.5)));
        game.Arrows.Shoot(LandOrigin.PlaceOf(new LandPlace(1001.5, Feet + 3, 0.5)), new Vector3(0, 0, 10));
        game.Blasts.Add(LandOrigin.PlaceOf(new LandPlace(1004.5, Feet, 4.5)), 2);
        game.Bits.Break(BlockId.Stone, 1001, 3, 3);
        game.Drops.Spawn(Items.Find("stone")!, 1, LandOrigin.PlaceOf(new LandPlace(998.5, Feet + 2, 0.5)), Vector3.Zero);
        headless.Step(1);
        var (things, lamps, sections) = (Things(game, ecs), Lamps(game), Sections(ecs));
        Assert.Contains(lamps, lamp => (Math.Floor(lamp.At.X), Math.Floor(lamp.At.Y), Math.Floor(lamp.At.Z)) == (1002, 4, 2));

        Assert.True(game.Recenter(new LandPlace(1100.5, Feet, 0.5)));
        Assert.Equal((1024, 0, 1), (LandOrigin.X, LandOrigin.Z, game.Recentered));
        Assert.Equal(things, Things(game, ecs));
        Assert.Equal((new LandPlace(1003.25, Feet, 0.75), new LandPlace(1010.5, Feet, -3.5)), (LandOrigin.LandOf(villager.Body.Position), LandOrigin.LandOf(villager.Home!.Value)));

        // The lamps are placed again in the next frame, the land's groups made again, and each
        // stands where it stood, its cube and every section's mesh uploaded again, so the scene
        // field takes them as come, not as moved.
        headless.Step(1);
        Assert.Equal(1024, LandOrigin.X);
        Assert.Equal(lamps.Select(lamp => lamp.At), Lamps(game).Select(lamp => lamp.At));
        Assert.Empty(lamps.Select(lamp => lamp.Cube).Intersect(Lamps(game).Select(lamp => lamp.Cube)));
        var after = Sections(ecs);
        Assert.Equal(sections.Select(section => section.Place), after.Select(section => section.Place));
        Assert.Empty(sections.Select(section => section.Mesh).Intersect(after.Select(section => section.Mesh)));
    }

    // Every float place the game keeps that a move shifts, each named, in the land's coordinates.
    private static List<(string Thing, LandPlace At)> Things(VoxelGame game, EcsWorld ecs)
    {
        var things = new List<(string, LandPlace)>();
        foreach (var (id, body) in ecs.Query<Body>()) things.Add(($"body {id}", LandOrigin.LandOf(body.Position)));
        foreach (var mob in game.Mobs.All)
            if (mob.Home is { } home) things.Add(($"home of mob {mob.Id}", LandOrigin.LandOf(home)));
        foreach (var (id, arrow) in ecs.Query<Arrow>()) things.Add(($"arrow {id}", LandOrigin.LandOf(arrow.Position)));
        foreach (var blast in game.Blasts.All) things.Add(("blast", LandOrigin.LandOf(blast.At)));
        foreach (var bit in game.Bits.All) things.Add(("bit", LandOrigin.LandOf(bit.Body.Position)));
        foreach (var ring in game.Particles.WaterRings) things.Add(("ring", LandOrigin.LandOf(ring.At)));
        Assert.Contains(things, thing => thing.Item1.StartsWith("arrow"));
        Assert.Contains(things, thing => thing.Item1 == "blast");
        Assert.Contains(things, thing => thing.Item1 == "bit");
        return things;
    }

    // The lamps the land draws, by where each stands in the land and its cube's upload.
    private static List<(LandPlace At, int Cube)> Lamps(VoxelGame game) =>
        [.. game.Renderer.LampsPlaced.Select(lamp => (LandOrigin.LandOf(lamp.At.Translation), lamp.Cube.Id)).OrderBy(lamp => (lamp.Item1.X, lamp.Item1.Y, lamp.Item1.Z))];

    // The land's drawn parts, each by its section, its kind, where its entity stands in the land and
    // its mesh's upload.
    private static List<(string Place, int Mesh)> Sections(EcsWorld ecs)
    {
        var parts = new List<(string Place, int Mesh)>();
        foreach (var (id, part) in ecs.Query<SectionPart>())
            parts.Add(($"{part.Section} {part.Kind} at {LandOrigin.LandOf(ecs.GetReadOnly<Transform>(id).Position)}", ecs.GetReadOnly<Mesh>(id).Uploaded.Id));
        Assert.NotEmpty(parts);
        return [.. parts.OrderBy(part => part.Place, StringComparer.Ordinal)];
    }

    [Fact]
    public void A_Blast_Sent_Before_A_Move_Goes_Off_Where_It_Was_Sent_From()
    {
        using var headless = PutNearTheLine();
        var game = headless.Game;

        // A TNT's blast sent in a frame, as its fuse runs out, is set off in the next, after the
        // origin moved in between.
        var at = new LandPlace(1003.5, 3.0625, 0.5);
        Assert.Equal(BlockId.Grass, game.World.GetBlock(1003, 3, 0));
        GetApp().World.SendEvent(new TntBlast(at));
        Assert.True(game.Recenter(new LandPlace(1100.5, Feet, 0.5)));
        headless.Step(1);

        Assert.Equal(at, LandOrigin.LandOf(Assert.Single(game.Blasts.All).At));
        Assert.Equal(BlockId.Air, game.World.GetBlock(1003, 3, 0));
    }

    [Fact]
    public void A_Far_Jump_Keeps_The_Mobs_And_The_Stacks_It_Leaves_Where_They_Lay()
    {
        using var headless = PutNearTheLine();
        var game = headless.Game;

        // A pig and a stack at places whose parts a float holds near the origin and not a million
        // blocks out, where it steps by an eighth, the stack out of the player's reach.
        var pig = game.Mobs.Add(new MobRecord(MobKinds.Pig, 0, new LandPlace(1003.3, Feet, 0.7), 0, MobKinds.Pig.Health, null));
        var at = LandOrigin.LandOf(pig.Body.Position);
        game.Drops.Spawn(Items.Find("stone")!, 3, LandOrigin.PlaceOf(new LandPlace(994.3, Feet, 0.7)), Vector3.Zero);
        headless.Step(1);
        var lying = Stacks(game);
        Assert.Single(lying);

        Assert.StartsWith("the player stands at", Run($"voxel.tp 1139992.5 {Feet} 596.5"));
        Assert.DoesNotContain(game.Mobs.All, mob => mob == pig);
        Assert.Contains(game.Mobs.Columns().SelectMany(column => column.Mobs), mob => mob.Kind == MobKinds.Pig && mob.Position == at);
        Assert.Equal((0, 1), (game.Drops.Count, game.Drops.Kept));

        // Put back, the pig and the stack come back where they lay once their columns are in again.
        Assert.StartsWith("the player stands at", Run($"voxel.tp 1000.5 {Feet} 0.5"));
        Stand(headless);
        Assert.Contains(game.Mobs.All, mob => mob.Kind == MobKinds.Pig && LandOrigin.LandOf(mob.Body.Position) == at);
        Assert.Equal(0, game.Drops.Kept);
        AssertLying(lying, Stacks(game));
    }

    [Fact]
    public void The_Things_A_Player_Spills_Far_From_The_Spawn_Lie_There_After_The_Respawn()
    {
        // A player 3,000 blocks east, the origin at 3,072, dies with what it holds and three stacks
        // more, comes back at the spawn, the origin at 0 and the columns where it fell unloaded, and
        // is put back.
        using var headless = new HeadlessGame(at: new LandPlace(3000.5, Feet, 0.5));
        var game = headless.Game;
        Serve(game);
        game.Mode = GameMode.Survival;
        game.Inventory.Add(Items.Find("stone")!, 40);
        game.Inventory.Add(Items.Find("dirt")!, 7);
        game.Inventory.Add(Items.Find("arrow")!, 3);
        var held = Holding(game);
        Assert.True(game.Vitals.Damage(40, "fell from a high place"));
        headless.Step(60);
        Assert.Empty(Holding(game));
        var lying = Stacks(game);
        Assert.Equal(held, lying.Select(stack => (stack.Item, stack.Count)).Order());

        Assert.StartsWith("the player stands at 0.5, ", Run($"voxel.respawn"));
        Assert.Equal((0, 0, lying.Count), (LandOrigin.X, game.Drops.Count, game.Drops.Kept));

        // Six blocks east of where it fell, out of reach of them, the stacks lie where they lay, as
        // old as they were and older by the frames since.
        Assert.StartsWith("the player stands at", Run($"voxel.tp 3006.5 {Feet} 0.5"));
        Assert.Equal(3072, LandOrigin.X);
        Stand(headless);
        AssertLying(lying, Stacks(game));

        // Walked onto them, the player takes every one back.
        Assert.StartsWith("the player stands at", Run($"voxel.tp 3000.5 {Feet} 0.5"));
        headless.Step(30);
        Assert.Equal(held, Holding(game));
    }

    // Steps the game until every column in reach is in, a frame at least.
    private static void Stand(HeadlessGame headless)
    {
        var clock = System.Diagnostics.Stopwatch.StartNew();
        do
        {
            headless.Step(1);
            Thread.Sleep(2);
        }
        while (headless.Game.Streamer.Pending > 0 && clock.Elapsed.TotalSeconds < 30);
    }

    // The stacks lying in the loaded columns, each by its item, count, place in the land and age.
    private static List<(string Item, int Count, LandPlace At, float Age)> Stacks(VoxelGame game) =>
        [.. game.Drops.All.Select(stack => (stack.Drop.Item.Key, stack.Drop.Count, LandOrigin.LandOf(stack.Body.Position), stack.Drop.Age)).OrderBy(stack => (stack.Key, stack.Count))];

    // The stacks lie as they lay, each of its item and count within a hundredth of a block of where
    // it lay and no younger, a stack set down again moving on by its velocity left.
    private static void AssertLying(List<(string Item, int Count, LandPlace At, float Age)> lay, List<(string Item, int Count, LandPlace At, float Age)> lie)
    {
        Assert.Equal(lay.Select(stack => (stack.Item, stack.Count)), lie.Select(stack => (stack.Item, stack.Count)));
        foreach (var (was, now) in lay.Zip(lie))
        {
            Assert.True(Math.Abs(was.At.X - now.At.X) < 0.01 && Math.Abs(was.At.Y - now.At.Y) < 0.01 && Math.Abs(was.At.Z - now.At.Z) < 0.01, $"{was.Item} lay at {was.At} and lies at {now.At}");
            Assert.True(now.Age >= was.Age, $"{was.Item} was {was.Age} s old and is {now.Age}");
        }
    }

    // What the inventory holds, by item and count.
    private static List<(string Item, int Count)> Holding(VoxelGame game) =>
        [.. game.Inventory.Slots.OfType<ItemStack>().Select(stack => (stack.Item.Key, stack.Count)).Order()];

    [Fact]
    public void A_Place_Past_The_Edge_Is_Refused_And_Moves_The_Origin_No_Further_Than_The_Edge()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        Serve(game);

        Assert.StartsWith("the land reaches", Run($"voxel.tp 3000000000 {Feet} 0.5"));
        Assert.Equal(0, LandOrigin.X);

        // A body past the edge, as a save written by hand may put it, moves the origin to the edge
        // once and no further.
        Assert.True(game.Recenter(new LandPlace(3e9, Feet, 0.5)));
        Assert.Equal(LandOrigin.Edge, LandOrigin.X);
        Assert.False(game.Recenter(new LandPlace(3e9, Feet, 0.5)));
        Assert.Equal(1, game.Recentered);
    }
}
