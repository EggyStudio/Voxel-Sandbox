using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>
/// A save ended at each of its writes and moves, as a crash of the game or of the machine ends it,
/// and the world opened from what it left.
/// </summary>
[Collection(nameof(HeadlessGames))]
public sealed class SaveCutTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("voxel-cut-").FullName;

    public void Dispose() => Directory.Delete(_root, true);

    // Chests A and B, a chest C broken, a furnace, a glowstone placed and a sea lantern broken, in
    // the regions (0, 0) and (1, 0) about the player.
    private static readonly (int X, int Z) A = (508, 4), B = (514, 4), C = (509, 6), F = (515, 6), G = (530, 3), D = (505, 3);

    [Fact]
    public void A_Save_Ended_At_Any_Of_Its_Writes_Opens_As_The_World_Before_It_Or_After_It()
    {
        // A world saved, then changed by things moved between its files each way, coal taken from a
        // chest and iron put into another, planks put into a furnace, a block placed and one
        // broken, a chest broken with what it held taken up, and a pig gone into the next region.
        var folder = Path.Combine(_root, "world");
        var cuts = new List<string>();
        string before, after, whole;
        using (var headless = new HeadlessGame(save: folder, at: new LandPlace(512.5, 120, 0.5), still: true))
        {
            var game = headless.Game;
            var y = game.World.Top(G.X, G.Z + 1) + 1;
            game.Mobs.Clear(null);
            game.Inventory.Clear();
            foreach (var (x, z) in new[] { A, B, C }) game.World.SetBlock(x, y, z, BlockId.Chest);
            game.World.SetBlock(F.X, y, F.Z, BlockId.Furnace);
            game.World.SetBlock(D.X, y, D.Z, BlockId.SeaLantern);
            game.Chests.At(A.X, y, A.Z).Slots[0] = new ItemStack(Items.Coal, 8);
            game.Chests.At(B.X, y, B.Z);
            game.Chests.At(C.X, y, C.Z).Slots[0] = new ItemStack(Items.Redstone, 6);
            game.Furnaces.At(F.X, y, F.Z);
            game.Inventory.Add(Items.IronIngot, 5);
            game.Inventory.Add(Items.Of(BlockId.OakPlanks), 3);
            game.Inventory.Add(Items.Of(BlockId.Glowstone), 1);
            game.Mobs.Add(new MobRecord(MobKinds.Pig, 0, new LandPlace(510.5, y, 8.5), 0, 7, null));
            game.SaveWorld();
            before = Counts(game);

            game.Chests.At(A.X, y, A.Z).Slots[0] = null;
            game.Inventory.Add(Items.Coal, 8);
            game.Inventory.Take(game.Inventory.Find(Items.IronIngot), 5);
            game.Chests.At(B.X, y, B.Z).Slots[0] = new ItemStack(Items.IronIngot, 5);
            game.Inventory.Take(game.Inventory.Find(Items.Of(BlockId.OakPlanks)), 3);
            game.Furnaces.At(F.X, y, F.Z).Slots[Furnace.Fuel] = new ItemStack(Items.Of(BlockId.OakPlanks), 3);
            game.Inventory.Take(game.Inventory.Find(Items.Of(BlockId.Glowstone)), 1);
            game.World.SetBlock(G.X, y, G.Z, BlockId.Glowstone);
            game.World.SetBlock(D.X, y, D.Z, BlockId.Air);
            game.Inventory.Add(Items.Of(BlockId.SeaLantern), 1);
            game.World.SetBlock(C.X, y, C.Z, BlockId.Air);
            foreach (var (_, stack) in game.Chests.Update(game.World)) game.Inventory.Add(stack.Item, stack.Count);
            game.Mobs.All.Single(m => m.Kind == MobKinds.Pig).Body.Position = LandOrigin.PlaceOf(new LandPlace(513.5, y, 8.5));
            after = Counts(game);

            // Ended before each file it writes beside its name, before its list takes its name, and
            // before each file moves into its own.
            game.Save!.Writing = _ => cuts.Add(Copy(folder));
            game.SaveWorld();
            game.Save.Writing = null;
            Assert.Null(game.SaveFault);
            whole = Copy(folder);
        }
        var (was, became) = (Files(cuts[0]), Files(whole));
        Assert.False(Same(was, became));
        var first = cuts.FindIndex(cut => File.Exists(Path.Combine(cut, "save.list")));
        Assert.InRange(first, 1, cuts.Count - 2);
        var listed = Copy(cuts[first]);

        // Each ended before the list stands opens as the world before the save, each after it as
        // the world after, nothing written beside left.
        Assert.All(cuts.Select((cut, i) => (cut, i)), ended =>
        {
            _ = new WorldSave(ended.cut);
            Assert.True(Same(Files(ended.cut), ended.i < first ? was : became), $"the save ended at {Path.GetFileName(ended.cut)} opens as other than the world {(ended.i < first ? "before" : "after")} it");
        });

        // The game opens the last ended before the list as the world before, and a cut among the
        // moves of the world's opening as the world after.
        Assert.Equal(before, Opened(cuts[first - 1]));
        var finishing = new List<string>();
        _ = new WorldSave(listed, _ => finishing.Add(Copy(listed)));
        Assert.NotEmpty(finishing);
        Assert.All(finishing, cut =>
        {
            _ = new WorldSave(cut);
            Assert.True(Same(Files(cut), became), $"the opening ended at {Path.GetFileName(cut)} opens as other than the world after the save");
        });
        Assert.Equal(after, Opened(finishing[^1]));
    }

    [Fact]
    public void A_Save_That_Fails_Before_Its_List_Changes_No_File_And_The_Next_Writes_What_It_Held()
    {
        var folder = Path.Combine(_root, "world");
        var pig = new MobRecord(MobKinds.Pig, 0, new LandPlace(40_000.5, 70, 0.5), 0, 7, null);
        using (var headless = new HeadlessGame(save: folder, still: true))
        {
            var game = headless.Game;
            game.World.SetBlock(2, 70, 2, BlockId.Glowstone);
            game.SaveWorld();
            var saved = Files(folder);

            // The mobs of a column far from the player the save holds till it writes them, a block
            // changed, and a folder in the place of the world's file before it takes its name, as a
            // full disk fails a save.
            game.Save!.KeepMobs(2500, 0, [pig]);
            game.World.SetBlock(4, 70, 2, BlockId.Glowstone);
            Directory.CreateDirectory(Path.Combine(folder, "world.json.new"));
            game.SaveWorld();

            Assert.NotNull(game.SaveFault);
            Assert.True(Same(Files(folder), saved), "a save that failed before its list changed the world's files");
            Directory.Delete(Path.Combine(folder, "world.json.new"));
            game.SaveWorld();
            Assert.Null(game.SaveFault);
        }

        Assert.Equal(pig, Assert.Single(new WorldSave(folder).TakeMobs(2500, 0)!));
        using var reopened = new HeadlessGame(save: folder, still: true);
        Assert.Equal(BlockId.Glowstone, reopened.Game.World.GetBlock(4, 70, 2));
    }

    // What the world holds of each thing the test moves, where it lies, as the game sees it.
    private static string Counts(VoxelGame game)
    {
        var y = game.World.Top(G.X, G.Z + 1) + 1;
        int InChest((int X, int Z) at, Item item) => game.Chests.All.TryGetValue((at.X, y, at.Z), out var chest) ? chest.Slots.Sum(s => s is { } stack && stack.Item == item ? stack.Count : 0) : 0;
        int InChests(Item item) => game.Chests.All.Values.Sum(chest => chest.Slots.Sum(s => s is { } stack && stack.Item == item ? stack.Count : 0));
        var planks = Items.Of(BlockId.OakPlanks);
        var burning = game.Furnaces.All.TryGetValue((F.X, y, F.Z), out var furnace) ? furnace.Slots.Sum(s => s is { } stack && stack.Item == planks ? stack.Count : 0) : 0;
        var pigs = game.Mobs.All.Where(m => m.Kind == MobKinds.Pig).Select(m => LandOrigin.LandOf(m.Body.Position).X.ToString("0.0"));
        var inventory = game.Inventory;
        return $"coal {InChest(A, Items.Coal)}+{inventory.Count(Items.Coal)}, iron {inventory.Count(Items.IronIngot)}+{InChest(B, Items.IronIngot)}, " +
            $"planks {inventory.Count(planks)}+{burning}, glowstone {inventory.Count(Items.Of(BlockId.Glowstone))}+{(game.World.GetBlock(G.X, y, G.Z) == BlockId.Glowstone ? 1 : 0)}, " +
            $"lantern {(game.World.GetBlock(D.X, y, D.Z) == BlockId.SeaLantern ? 1 : 0)}+{inventory.Count(Items.Of(BlockId.SeaLantern))}, " +
            $"redstone {InChests(Items.Redstone)}+{inventory.Count(Items.Redstone)}, pigs at {string.Join(" ", pigs)}";
    }

    // What the game finds in a world's folder as it opens it.
    private static string Opened(string folder)
    {
        using var headless = new HeadlessGame(save: folder, still: true);
        return Counts(headless.Game);
    }

    // A copy of a world's folder as it stands, in a folder of its own.
    private string Copy(string from)
    {
        var to = Path.Combine(_root, $"cut-{Directory.GetDirectories(_root).Length:000}");
        foreach (var file in Directory.GetFiles(from, "*", SearchOption.AllDirectories))
        {
            var copy = Path.Combine(to, Path.GetRelativePath(from, file));
            Directory.CreateDirectory(Path.GetDirectoryName(copy)!);
            File.Copy(file, copy);
        }
        return to;
    }

    private static Dictionary<string, byte[]> Files(string folder) =>
        Directory.GetFiles(folder, "*", SearchOption.AllDirectories).ToDictionary(file => Path.GetRelativePath(folder, file), File.ReadAllBytes);

    private static bool Same(Dictionary<string, byte[]> one, Dictionary<string, byte[]> other) =>
        one.Count == other.Count && one.All(file => other.TryGetValue(file.Key, out var bytes) && bytes.AsSpan().SequenceEqual(file.Value));
}
