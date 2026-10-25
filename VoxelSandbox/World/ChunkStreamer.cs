using System.Collections.Concurrent;
using System.Numerics;

namespace VoxelSandbox;

/// <summary>
/// Loads the columns around the player and unloads those left behind, generating each on a worker
/// and taking it into the world on the main thread.
/// </summary>
/// <remarks>
/// Columns are loaded two further than the render distance, since a section on the edge of what is
/// drawn is meshed only once the eight columns around it are there, its corners shaded by their
/// blocks and light, and unloaded three further, so walking back and forth over a boundary does
/// not load and unload the same columns each time. A column the player changed is read from the
/// save rather than generated, and kept by the save as it unloads. The mobs standing in a column
/// come and go with it. A column's deep sections out of the eye's reach are packed by the worker
/// that makes it, and the loaded columns' again, a few a frame, each time the eye enters another
/// section, as <see cref="Reach"/> has it.
/// </remarks>
public sealed class ChunkStreamer(VoxelWorld world, ChunkRenderer renderer, Mobs mobs, Chests chests, WorldSave? save)
{
    private readonly ConcurrentQueue<(int X, int Z, ChunkColumn? Column)> _done = new();
    private readonly HashSet<(int X, int Z)> _pending = [];
    private readonly List<(int X, int Z)> _wanted = [];
    private readonly List<(int X, int Z)> _leaving = [];
    private readonly int _workers = Math.Clamp(Environment.ProcessorCount - 2, 1, 8);
    private (int X, int Z, int Distance)? _planned;
    private int _running;
    // The columns still to pack for the eye's section and render distance last packed for.
    private readonly Queue<(int X, int Z)> _packing = new();
    private (SectionKey Eye, int Distance)? _packedFor;
    private const int PackedAFrame = 16;

    /// <summary>Columns asked for and not yet in the world.</summary>
    public int Pending => _pending.Count;

    /// <summary>Generates the columns around a point on this thread, so the player has ground under them before the first frame.</summary>
    public void LoadNow(Vector3 at, int radius)
    {
        int cx = VoxelWorld.ColumnOf((int)MathF.Floor(at.X)), cz = VoxelWorld.ColumnOf((int)MathF.Floor(at.Z));
        for (int z = cz - radius; z <= cz + radius; z++)
            for (int x = cx - radius; x <= cx + radius; x++)
                if (!world.HasColumn(x, z)) Join(Make(world.Generator, Kept(x, z), save?.Renumber, x, z, Reach.Of(at), radius + Reach.Kept));
    }

    /// <summary>Takes in the columns the workers finished, asks for those now in reach, and unloads those out of it.</summary>
    public void Update(Vector3 player, int renderDistance)
    {
        while (_done.TryDequeue(out var result))
        {
            Interlocked.Decrement(ref _running);
            _pending.Remove((result.X, result.Z));
            if (result.Column is not null) Join(result.Column);
        }

        int cx = VoxelWorld.ColumnOf((int)MathF.Floor(player.X)), cz = VoxelWorld.ColumnOf((int)MathF.Floor(player.Z));
        var load = renderDistance + 2;
        var (eye, kept) = (Reach.Of(player), renderDistance + Reach.Kept);
        if (_packedFor != (eye, kept))
        {
            _packedFor = (eye, kept);
            _packing.Clear();
            foreach (var column in world.Columns) _packing.Enqueue((column.X, column.Z));
        }
        for (int i = 0; i < PackedAFrame && _packing.TryDequeue(out var place); i++)
            if (world.TryGetColumn(place.X, place.Z, out var column)) column.Pack(eye, kept);
        if (_planned != (cx, cz, renderDistance))
        {
            _planned = (cx, cz, renderDistance);
            Plan(cx, cz, load);
            Unload(cx, cz, load + 1);
        }

        foreach (var (x, z) in _wanted)
        {
            if (_running >= _workers) break;
            if (world.HasColumn(x, z) || !_pending.Add((x, z))) continue;
            Interlocked.Increment(ref _running);
            var (generator, saved, renumber) = (world.Generator, Kept(x, z), save?.Renumber);
            Task.Run(() =>
            {
                ChunkColumn? column = null;
                try
                {
                    column = Make(generator, saved, renumber, x, z, eye, kept);
                }
                catch (Exception ex)
                {
                    // The worker's place is given back and the column is asked for again once the
                    // player moves, rather than the exception ending a task nobody waits on.
                    Console.Error.WriteLine($"Column {x}, {z} could not be generated. {ex}");
                }
                _done.Enqueue((x, z, column));
            });
        }
        _wanted.RemoveAll(c => world.HasColumn(c.X, c.Z) || _pending.Contains(c));
    }

    private void Join(ChunkColumn column)
    {
        world.Add(column);
        mobs.Joined(column);
        chests.Joined(column);
    }

    // The compressed blocks the save keeps for a column, read on the main thread, or null.
    private byte[]? Kept(int x, int z)
    {
        save?.ReadRegion(x, z);
        return save?.Kept(x, z);
    }

    // A column from its kept blocks or its generator, with its own light worked out, which leaves the
    // main thread only the light across its edges, its open air sharing its arrays and its deep
    // sections out of the eye's reach packed.
    private static ChunkColumn Make(IWorldGenerator generator, byte[]? kept, ushort[]? renumber, int x, int z, SectionKey eye, int distance)
    {
        ChunkColumn column;
        if (kept is null) column = generator.Generate(x, z);
        else
        {
            column = WorldSave.Decode(x, z, kept, renumber, WorldSave.Shallow(kept) ? generator.Generate(x, z) : null);
            generator.Paint(column);
        }
        Lighting.Compute(column);
        column.Settle();
        column.Pack(eye, distance);
        return column;
    }

    // The columns within the load distance, nearest first.
    private void Plan(int cx, int cz, int load)
    {
        _wanted.Clear();
        var reach = load * load + load;
        for (int z = cz - load; z <= cz + load; z++)
            for (int x = cx - load; x <= cx + load; x++)
            {
                int dx = x - cx, dz = z - cz;
                if (dx * dx + dz * dz <= reach && !world.HasColumn(x, z)) _wanted.Add((x, z));
            }
        _wanted.Sort((a, b) => ((a.X - cx) * (a.X - cx) + (a.Z - cz) * (a.Z - cz)).CompareTo((b.X - cx) * (b.X - cx) + (b.Z - cz) * (b.Z - cz)));
    }

    private void Unload(int cx, int cz, int keep)
    {
        var reach = keep * keep + keep;
        _leaving.Clear();
        foreach (var column in world.Columns)
        {
            int dx = column.X - cx, dz = column.Z - cz;
            if (dx * dx + dz * dz > reach) _leaving.Add((column.X, column.Z));
        }
        foreach (var (x, z) in _leaving)
        {
            if (save is not null && world.TryGetColumn(x, z, out var column) && column.Changed) save.Keep(column);
            mobs.Leaving(x, z);
            world.Remove(x, z);
            renderer.ForgetColumn(x, z);
        }
    }
}
