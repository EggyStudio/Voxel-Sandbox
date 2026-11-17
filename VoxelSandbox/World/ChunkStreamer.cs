using System.Collections.Concurrent;
using System.Numerics;
using Engine;
using static Engine.E3D;

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
/// and the stacks lying in it come and go with it. A column's deep sections out of the eye's reach are packed by the worker
/// that makes it, and the loaded columns' again, a few a frame, each time the eye enters another
/// section, as <see cref="Reach"/> has it. The columns the workers make are taken in
/// <see cref="JoinedAFrame"/> a frame, the nearest the player first, since the columns of a batch
/// of the caves' readings go to the workers together and finish together, and the work their
/// joining brings, in the game and in the engine, is spread over frames this way.
/// </remarks>
public sealed class ChunkStreamer(VoxelWorld world, ChunkRenderer renderer, Mobs mobs, ItemDrops drops, Chests chests, WorldSave? save, CaveFields? fields = null, FallingBlocks? falling = null)
{
    private readonly ConcurrentQueue<(int X, int Z, ChunkColumn? Column, bool Unread, int Unknown)> _done = new();
    private readonly HashSet<(int X, int Z)> _pending = [];
    private readonly List<(int X, int Z)> _wanted = [];
    private readonly List<(int X, int Z)> _leaving = [];
    // The columns the workers made and the update has not taken in.
    private readonly List<(int X, int Z, ChunkColumn Column, bool Unread, int Unknown)> _made = [];

    /// <summary>How many columns the workers made an update takes into the world at most.</summary>
    public const int JoinedAFrame = 2;

    /// <summary>
    /// How many of those an update takes in at most from past the player's own load circle, which
    /// only the next column's circle asks for.
    /// </summary>
    /// <remarks>
    /// The next column's circle is asked for as the player steps into a column, and its nearest
    /// columns join a few frames after the step, each marking sections within reach to mesh again,
    /// so they join one a frame, and the meshes and uploads each brings fall a frame apart.
    /// </remarks>
    public const int AheadAFrame = 1;

    /// <summary>How many columns are generated at once.</summary>
    internal int Workers { get; init; } = Math.Clamp(Environment.ProcessorCount - 2, 1, 8);

    /// <summary>How many times a column whose making fails is asked for before it is left out.</summary>
    public const int Tries = 3;

    // How many times each column's making failed in this world.
    private readonly Dictionary<(int X, int Z), int> _failed = [];
    private (int X, int Z, (int X, int Z)? Ahead, int Distance)? _planned;
    private Vector3 _lastPlayer;
    private int _running;
    // The columns still to pack for the eye's section and render distance last packed for.
    private readonly Queue<(int X, int Z)> _packing = new();
    private (SectionKey Eye, int Distance)? _packedFor;
    private const int PackedAFrame = 16;

    /// <summary>
    /// Columns asked for and not yet in the world, those a worker is generating, those waiting for
    /// a worker or for their caves' readings from the GPU and those made and not yet taken in.
    /// </summary>
    /// <remarks>
    /// Where the GPU reads the caves' noises, a column waits for its readings with no worker on it, so
    /// a count of the workers' columns alone reads none while a new place's columns wait on a slow
    /// device, as on lavapipe.
    /// </remarks>
    public int Pending => _pending.Count + _wanted.Count;

    /// <summary>Whether a column's tries ended with no column made, so it is left out of the world.</summary>
    public bool LeftOut(int x, int z) => _failed.GetValueOrDefault((x, z)) >= Tries;

    /// <summary>
    /// Columns the workers made that wait to be taken in, <see cref="JoinedAFrame"/> a frame.
    /// </summary>
    public int Made => _done.Count + _made.Count;

    /// <summary>Generates the columns around a point on this thread, so the player has ground under them before the first frame.</summary>
    public void LoadNow(Vector3 at, int radius)
    {
        var (cx, cz) = LandOrigin.ColumnOf(at);
        for (int z = cz - radius; z <= cz + radius; z++)
            for (int x = cx - radius; x <= cx + radius; x++)
                if (!world.HasColumn(x, z)) Take(x, z, Make(world.Generator, Kept(x, z), x, z, Reach.Of(at), radius + Reach.Kept));
    }

    /// <summary>Takes in the columns the workers finished, asks for those now in reach, and unloads those out of it.</summary>
    public void Update(Vector3 player, int renderDistance)
    {
        // A worker is free once its column is made, and the column stays pending until it is taken
        // in, so it is not asked for again.
        while (_done.TryDequeue(out var result))
        {
            Interlocked.Decrement(ref _running);
            if (result.Column is null) Failed(result.X, result.Z);
            else _made.Add((result.X, result.Z, result.Column, result.Unread, result.Unknown));
        }

        // While a timed save is written on a worker nothing else touches the save, so no column is
        // taken in, unloaded or asked for, which joining, unloading and reading a kept one would.
        if (save is { Busy: true }) return;
        var (cx, cz) = LandOrigin.ColumnOf(player);
        var load = renderDistance + 2;
        TakeMade(cx, cz, load);
        var (eye, kept) = (Reach.Of(player), renderDistance + Reach.Kept);
        if (_packedFor != (eye, kept))
        {
            _packedFor = (eye, kept);
            _packing.Clear();
            foreach (var column in world.Columns) _packing.Enqueue((column.X, column.Z));
        }
        for (int i = 0; i < PackedAFrame && _packing.TryDequeue(out var place); i++)
            if (world.TryGetColumn(place.X, place.Z, out var column)) column.Pack(eye, kept);
        var ahead = Ahead(player, cx, cz);
        _lastPlayer = player;
        if (_planned != (cx, cz, ahead, renderDistance))
        {
            _planned = (cx, cz, ahead, renderDistance);
            Plan(cx, cz, load, ahead);
            Unload(cx, cz, load + 1);
        }

        // A column to generate waits for its caves' readings where the GPU reads them, a few batches
        // asked ahead of the workers.
        fields?.Update((x, z) => _wanted.Contains((x, z)));
        // The GPU is asked ahead of the workers, the nearest columns first, so a worker that comes
        // free finds a column's readings back.
        var asking = fields is { Running: true } && world.Generator is Overworld;
        foreach (var (x, z) in _wanted)
        {
            if (_running >= Workers && !asking) break;
            if (world.HasColumn(x, z) || _pending.Contains((x, z))) continue;
            var (generator, saved) = (world.Generator, Kept(x, z));
            var gpu = asking && saved is null;
            ArraySegment<int>? readings = null;
            if (_running >= Workers || gpu && !fields!.TryTake(x, z, out readings))
            {
                if (gpu && fields!.Backlog < CaveFields.Batch * CaveFields.Flights * 2) fields.Ask(x, z);
                else if (_running >= Workers) break;
                continue;
            }
            _pending.Add((x, z));
            Interlocked.Increment(ref _running);
            Task.Run(() =>
            {
                (ChunkColumn? Column, bool Unread, int Unknown) made = (null, false, 0);
                try
                {
                    made = Make(generator, saved, x, z, eye, kept, readings);
                }
                catch (Exception ex)
                {
                    // The worker's place is given back and the column is asked for again, rather
                    // than the exception ending a task nobody waits on.
                    TraceLog(LogLevel.Warning, $"Column {x}, {z} could not be generated. {ex}");
                }
                _done.Enqueue((x, z, made.Column, made.Unread, made.Unknown));
            });
        }
        _wanted.RemoveAll(c => world.HasColumn(c.X, c.Z) || _pending.Contains(c));
    }

    // Takes in the made columns nearest the player's column first, JoinedAFrame at most, and of
    // those past the player's load circle AheadAFrame at most. A column made at once while its
    // worker made it, as the one a save lands the player in, is the one the world keeps, with what
    // changed in it since and its fires named once.
    private void TakeMade(int cx, int cz, int load)
    {
        var (reach, ahead) = ((long)load * load + load, 0);
        for (int joined = 0; joined < JoinedAFrame && _made.Count > 0;)
        {
            var nearest = 0;
            for (int i = 1; i < _made.Count; i++)
                if (Distance(_made[i]) < Distance(_made[nearest])) nearest = i;
            var past = Distance(_made[nearest]) > reach;
            if (past && ahead == AheadAFrame) break;
            var (x, z, column, unread, unknown) = _made[nearest];
            _made.RemoveAt(nearest);
            _pending.Remove((x, z));
            if (world.HasColumn(x, z)) continue;
            Take(x, z, (column, unread, unknown));
            joined++;
            if (past) ahead++;
        }
        // Squared in longs, as Unload squares them.
        long Distance((int X, int Z, ChunkColumn, bool, int) made) =>
            (long)(made.X - cx) * (made.X - cx) + (long)(made.Z - cz) * (made.Z - cz);
    }

    // Joins a column made, the save leaving out the kept blocks of one that could not read them.
    private void Take(int x, int z, (ChunkColumn Column, bool Unread, int Unknown) made)
    {
        if (made.Unread) save?.Unreadable(x, z);
        if (made.Unknown > 0) save?.Unknown(x, z, made.Unknown);
        Join(made.Column);
    }

    private void Join(ChunkColumn column)
    {
        world.Add(column);
        mobs.Joined(column);
        drops.Joined(column.X, column.Z);
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
    // sections out of the eye's reach packed. A kept column whose blocks do not inflate is made by
    // its generator, as one never kept is, and said to be unread.
    private static (ChunkColumn Column, bool Unread, int Unknown) Make(IWorldGenerator generator, byte[]? kept, int x, int z, SectionKey eye, int distance, ArraySegment<int>? readings = null)
    {
        ChunkColumn? column = null;
        var unknown = 0;
        if (kept is not null)
            try
            {
                column = WorldSave.Decode(x, z, kept, out unknown, WorldSave.Shallow(kept) ? generator.Generate(x, z) : null);
                generator.Paint(column);
            }
            catch (InvalidDataException)
            {
                // Made below by the generator.
            }
        var unread = kept is not null && column is null;
        column ??= readings is not null && generator is Overworld land ? land.Generate(x, z, readings) : generator.Generate(x, z);
        Lighting.Compute(column);
        column.Settle();
        column.Pack(eye, distance);
        return (column, unread, unknown);
    }

    // The column the player moves into next along the motion since the last update, or none. Its
    // circle is asked for as the player steps into a column, so its columns join in the frames
    // after the step, the corners of the drawn reach the next step brings among them, and out of
    // the frames halfway across the column in which the scene's field does its own work.
    private (int X, int Z)? Ahead(Vector3 player, int cx, int cz)
    {
        var motion = player - _lastPlayer;
        if (motion.LengthSquared() < 1e-8f || motion.LengthSquared() > Section.Size * Section.Size) return null;
        (int X, int Z)? next = null;
        var soonest = float.MaxValue;
        foreach (var (moved, at, step) in new[] { (motion.X, player.X, (1, 0)), (motion.Z, player.Z, (0, 1)) })
        {
            if (moved == 0) continue;
            // The place within the column, the land's origin lying on whole columns.
            var inside = at - MathF.Floor(at / Section.Size) * Section.Size;
            var face = moved > 0 ? Section.Size - inside : inside;
            if (face / MathF.Abs(moved) >= soonest) continue;
            soonest = face / MathF.Abs(moved);
            var sign = MathF.Sign(moved);
            next = (cx + step.Item1 * (int)sign, cz + step.Item2 * (int)sign);
        }
        return next;
    }

    // A column whose making failed is asked for again first, while the player stands still too, for
    // its tries at most, and once they end it is named and left out of the plans.
    private void Failed(int x, int z)
    {
        _pending.Remove((x, z));
        var tries = _failed[(x, z)] = _failed.GetValueOrDefault((x, z)) + 1;
        if (tries < Tries) _wanted.Insert(0, (x, z));
        else TraceLog(LogLevel.Error, $"Column {x}, {z} could not be generated in {Tries} tries, so it is left out of the world.");
    }

    // The columns within the load distance of the player's column and, where the player nears the
    // next, of that column too, nearest the player's first, those whose tries ended left out.
    private void Plan(int cx, int cz, int load, (int X, int Z)? ahead)
    {
        _wanted.Clear();
        var reach = load * load + load;
        var (ax, az) = ahead ?? (cx, cz);
        for (int z = Math.Min(cz, az) - load; z <= Math.Max(cz, az) + load; z++)
            for (int x = Math.Min(cx, ax) - load; x <= Math.Max(cx, ax) + load; x++)
            {
                int dx = x - cx, dz = z - cz, nx = x - ax, nz = z - az;
                if ((dx * dx + dz * dz <= reach || nx * nx + nz * nz <= reach) && !world.HasColumn(x, z) && _failed.GetValueOrDefault((x, z)) < Tries) _wanted.Add((x, z));
            }
        _wanted.Sort((a, b) => ((a.X - cx) * (a.X - cx) + (a.Z - cz) * (a.Z - cz)).CompareTo((b.X - cx) * (b.X - cx) + (b.Z - cz) * (b.Z - cz)));
    }

    /// <summary>
    /// Unloads the columns out of reach of a place of the land's own the player is put at, before the
    /// origin moves near it, so the columns the player changed are kept and the mobs' and the stacks'
    /// places taken while they are near the origin. The next update plans the columns about the player
    /// again, so a jump back to the column the last plan was made for loads its columns again.
    /// </summary>
    public void Leave(LandPlace to, int renderDistance)
    {
        Unload(VoxelWorld.ColumnOf((int)Math.Floor(to.X)), VoxelWorld.ColumnOf((int)Math.Floor(to.Z)), renderDistance + 3);
        _planned = null;
    }

    private void Unload(int cx, int cz, int keep)
    {
        var reach = keep * keep + keep;
        _leaving.Clear();
        foreach (var column in world.Columns)
        {
            // Squared in longs, since a column 46,341 or more from the player's, as after a teleport
            // of some 741,000 blocks, squares past an int.
            long dx = column.X - cx, dz = column.Z - cz;
            if (dx * dx + dz * dz > reach) _leaving.Add((column.X, column.Z));
        }
        foreach (var (x, z) in _leaving)
        {
            // A block falling over the column is set down in it first, so the column keeps it.
            falling?.SetDown(world, x, z);
            if (save is not null && world.TryGetColumn(x, z, out var column) && column.Changed) save.Keep(column);
            mobs.Leaving(x, z);
            drops.Leaving(x, z);
            chests.Leaving(x, z);
            world.Remove(x, z);
            renderer.ForgetColumn(x, z);
        }
    }
}
