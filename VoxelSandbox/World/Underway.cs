using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>
/// The things under way in the world that a save keeps by region beside the blocks, the blocks
/// falling, the TNT lit and each fire's age, so a save while a block falls or a fuse burns keeps it
/// where it is, and a fire burns on from its age.
/// </summary>
/// <remarks>
/// A save writes those of every loaded column, a column holding none among them, so a block that
/// lands after a save is kept by the next as a block alone. A column that unloads gives its lit TNT
/// and its fires' ages to the save, its falling blocks set down in it first, and a column that
/// joins lays them down again, the fires taking their ages as the world kindles them. A world that
/// keeps no save keeps them in memory for the run.
/// </remarks>
public sealed class Underway(EcsWorld ecs, FallingBlocks falling, PrimedTnt tnt, Fires fires)
{
    /// <summary>What a record holds, a falling block, a lit TNT or a fire's age.</summary>
    public const byte FallingKind = 0, LitKind = 1, FireKind = 2;

    private static readonly Dictionary<string, BlockId> ByKey = Blocks.All.ToDictionary(block => block.Key, block => block.Id);

    // Those of the columns that unloaded in a world that keeps no save.
    private readonly Dictionary<(int X, int Z), List<WorldSave.UnderwayRecord>> _kept = [];

    /// <summary>The save of the world, which keeps those of the columns that unload, or null for a world that keeps none.</summary>
    public WorldSave? Save { get; set; }

    /// <summary>Those of each loaded column as a save keeps them, a column with none among them, so a save clears what it held of one.</summary>
    public IEnumerable<((int X, int Z) Column, IReadOnlyList<WorldSave.UnderwayRecord> Records)> Columns(VoxelWorld world)
    {
        var held = new Dictionary<(int X, int Z), List<WorldSave.UnderwayRecord>>();
        foreach (var column in world.Columns) held[(column.X, column.Z)] = [];
        foreach (var (column, record, _) in All())
            if (held.TryGetValue(column, out var records)) records.Add(record);
        foreach (var (column, records) in held) yield return (column, records);
    }

    /// <summary>Gives a column's lit TNT and its fires' ages to the save as it unloads, its falling blocks set down in it before.</summary>
    public void Leaving(int x, int z)
    {
        var left = new List<WorldSave.UnderwayRecord>();
        var gone = new List<int>();
        foreach (var (column, record, id) in All())
        {
            if (column != (x, z)) continue;
            left.Add(record);
            gone.Add(id);
        }
        foreach (var id in gone) ecs.Despawn(id);
        if (Save is { } save) save.KeepUnderway(x, z, [.. save.TakeUnderway(x, z) ?? [], .. left]);
        else if (left.Count > 0)
        {
            if (!_kept.TryGetValue((x, z), out var kept)) _kept[(x, z)] = kept = [];
            kept.AddRange(left);
        }
    }

    /// <summary>Lays down what a column kept as it joins, the falling blocks and the lit TNT where they were and as they moved, and the fires' ages for the world's kindling.</summary>
    public void Joined(int x, int z)
    {
        var records = _kept.Remove((x, z), out var kept) ? kept : [];
        records.AddRange(Save?.TakeUnderway(x, z) ?? []);
        foreach (var record in records)
        {
            var at = LandOrigin.PlaceOf(record.At);
            switch (record.Kind)
            {
                case FallingKind when ByKey.TryGetValue(record.Block, out var block):
                    falling.Spawn(block, at, record.Velocity, record.Time);
                    break;
                case LitKind:
                    tnt.Spawn(at, record.Velocity, record.Time);
                    break;
                case FireKind:
                    fires.Aged[((int)Math.Floor(record.At.X), (int)Math.Floor(record.At.Y), (int)Math.Floor(record.At.Z))] = (int)record.Time;
                    break;
            }
        }
    }

    /// <summary>Lets go of those the columns that unloaded keep, as a world closes.</summary>
    public void Clear() => _kept.Clear();

    // Every falling block, lit TNT and fire, by the column it is in, as a save keeps it, with its
    // entity.
    private IEnumerable<((int X, int Z) Column, WorldSave.UnderwayRecord Record, int Id)> All()
    {
        var all = new List<((int X, int Z), WorldSave.UnderwayRecord, int)>();
        foreach (var (id, block, body) in ecs.Query<FallingBlock, Body>())
            all.Add((LandOrigin.ColumnOf(body.Position), new(FallingKind, Blocks.Get(block.Block).Key, LandOrigin.LandOf(body.Position), body.Velocity, block.Age), id));
        foreach (var (id, lit, body) in ecs.Query<LitTnt, Body>())
            all.Add((LandOrigin.ColumnOf(body.Position), new(LitKind, "", LandOrigin.LandOf(body.Position), body.Velocity, lit.Left), id));
        foreach (var (id, fire) in ecs.Query<Fire>())
            all.Add(((VoxelWorld.ColumnOf(fire.X), VoxelWorld.ColumnOf(fire.Z)), new(FireKind, "", new LandPlace(fire.X, fire.Y, fire.Z), Vector3.Zero, fire.Age), id));
        return all;
    }
}
