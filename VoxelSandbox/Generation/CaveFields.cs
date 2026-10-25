using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The caves' noises read on the GPU at the corners of the columns about to be generated, a batch of
/// columns a dispatch of <c>resources/shaders/cavefields.slang</c>, read back a frame or more later
/// without waiting, so a worker places a column's caves from its readings rather than reading the
/// noises itself.
/// </summary>
/// <remarks>
/// <para>
/// The shader reads each noise in fixed point as <see cref="SourceNoise"/> reads it on the CPU, so a
/// column's readings are the CPU's to the bit and a world is the same whichever path made each of
/// its columns, which <see cref="Verify"/> checks over columns given.
/// </para>
/// <para>
/// Up to <see cref="Flights"/> batches are in flight at once, each writing a buffer of its own, and
/// each batch's columns are a uniform of its dispatch, which the engine copies as it dispatches, so
/// sending a batch never waits for the GPU, where writing a buffer would wait for every dispatch
/// before it. A batch of eight columns is about 1.2 MB of readings, and one of sixteen stalled the
/// world's opening for over thirty seconds on the GPU it was tried on.
/// </para>
/// <para>
/// Where the device has no compute, as a run without a window has none, or the shader does not
/// load, the fields are off and every column reads its own noises.
/// </para>
/// </remarks>
public sealed class CaveFields : IDisposable
{
    /// <summary>How many columns a dispatch reads at once, as many as the shader's uniform of corners holds.</summary>
    public const int Batch = 8;

    /// <summary>How many batches may be in flight at once.</summary>
    public const int Flights = 3;

    // A column's corner, as the shader's uniform of four integers lays it out.
    private readonly record struct Origin(int X, int Z, int Unused1 = 0, int Unused2 = 0);

    private readonly Shader _shader;
    private readonly int _originsAt, _valuesAt;
    private ShaderBuffer _ranges, _levels, _permutations;
    private readonly ShaderBuffer[] _values = new ShaderBuffer[Flights];
    private Caves? _caves;
    private int _samplers;
    private readonly Queue<(int X, int Z)> _asked = new();
    private readonly HashSet<(int X, int Z)> _waiting = [];
    private readonly Dictionary<(int X, int Z), ArraySegment<int>> _ready = [];
    // The batches in flight, oldest first, each its buffer's place, its readback and its columns.
    private readonly List<(int Slot, ShaderReadback Readback, (int X, int Z)[] Columns)> _flights = [];
    private bool _failed;
    // The main thread's time a frame, eased over about a second, since the profile shows the last value set.
    private float _ms;

    /// <summary>Whether the columns about to be generated take their caves' readings from the GPU, the setting.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Whether the GPU reads them, the setting on, the shader loaded and the device able to compute.</summary>
    public bool Running => Enabled && _caves is not null && _shader.IsValid && !_failed;

    /// <summary>Columns whose readings came from the GPU since the world opened.</summary>
    public int Read { get; private set; }

    /// <summary>How many columns are asked for and not yet read back.</summary>
    public int Backlog => _waiting.Count;

    public CaveFields()
    {
        _shader = LoadComputeShader("resources/shaders/cavefields.slang");
        if (!_shader.IsValid) return;
        (_originsAt, _valuesAt) = (GetShaderLocation(_shader, "origins"), GetShaderLocation(_shader, "values"));
    }

    /// <summary>Reads the noises of a world's caves from now on, its samplers' levels and permutations given to the shader, what was asked or read for the world before let go.</summary>
    public void Use(Caves? caves)
    {
        Forget();
        _caves = caves;
        Read = 0;
        if (caves is null || !_shader.IsValid) return;
        var samplers = caves.Samplers;
        _samplers = samplers.Count;
        // The permutations each sampler's levels hold, each once, and each level's eight numbers.
        var permutations = new List<byte[]>();
        var levels = new List<uint>();
        var ranges = new List<uint>();
        foreach (var sampler in samplers)
        {
            ranges.Add((uint)(levels.Count / 8));
            ranges.Add((uint)sampler.Levels.Length);
            foreach (var (p, ox, oy, oz, kx, ky, kz, weight) in sampler.Levels)
            {
                var index = permutations.FindIndex(known => ReferenceEquals(known, p));
                if (index < 0)
                {
                    index = permutations.Count;
                    permutations.Add(p);
                }
                levels.AddRange([(uint)index, ox, oy, oz, kx, ky, kz, unchecked((uint)weight)]);
            }
        }
        var packed = new uint[permutations.Count * 64];
        for (int i = 0; i < permutations.Count; i++)
            for (int b = 0; b < 256; b++)
                packed[i * 64 + b / 4] |= (uint)permutations[i][b] << (b % 4 * 8);
        _ranges = LoadShaderBuffer<uint>([.. ranges]);
        _levels = LoadShaderBuffer<uint>([.. levels]);
        _permutations = LoadShaderBuffer<uint>(packed);
        for (int i = 0; i < Flights; i++) _values[i] = LoadShaderBuffer(Batch * Caves.Corners * _samplers * sizeof(int));
        SetShaderValueBuffer(_shader, GetShaderLocation(_shader, "ranges"), _ranges);
        SetShaderValueBuffer(_shader, GetShaderLocation(_shader, "levels"), _levels);
        SetShaderValueBuffer(_shader, GetShaderLocation(_shader, "permutations"), _permutations);
        // Every dispatch reads a whole batch, a short one's last column read again, so the counts
        // are set once.
        SetShaderValue(_shader, GetShaderLocation(_shader, "columns"), Batch);
        SetShaderValue(_shader, GetShaderLocation(_shader, "samplers"), _samplers);
        SetShaderValue(_shader, GetShaderLocation(_shader, "corners"), Caves.Corners);
    }

    /// <summary>Asks for a column's readings, once, to be read in a batch to come.</summary>
    public void Ask(int x, int z)
    {
        if (_ready.ContainsKey((x, z)) || !_waiting.Add((x, z))) return;
        _asked.Enqueue((x, z));
    }

    /// <summary>Takes a column's readings where they are back, which no one else is given.</summary>
    public bool TryTake(int x, int z, out ArraySegment<int>? readings)
    {
        readings = _ready.Remove((x, z), out var found) ? found : (ArraySegment<int>?)null;
        return readings is not null;
    }

    /// <summary>
    /// Takes the readings of the batches in flight that are back, and sends batches of the columns
    /// asked for and not yet read while there is room in the air, a frame's work on the main thread.
    /// </summary>
    /// <param name="wanted">Whether a column is still wanted, a column no longer wanted left unread and its readings let go.</param>
    public void Update(Func<int, int, bool> wanted)
    {
        if (!Running) return;
        var clock = System.Diagnostics.Stopwatch.StartNew();
        while (_flights.Count > 0 && IsShaderReadbackReady(_flights[0].Readback)) Land(wanted);
        foreach (var gone in _ready.Keys.Where(c => !wanted(c.X, c.Z)).ToList()) _ready.Remove(gone);
        while (_flights.Count < Flights && _asked.Count > 0)
        {
            var batch = new List<(int X, int Z)>(Batch);
            while (batch.Count < Batch && _asked.TryDequeue(out var column))
            {
                if (wanted(column.X, column.Z)) batch.Add(column);
                else _waiting.Remove(column);
            }
            if (batch.Count == 0) break;
            var slot = Enumerable.Range(0, Flights).First(s => _flights.All(f => f.Slot != s));
            var sent = Send(batch, slot);
            if (!sent.IsValid)
            {
                Fail();
                break;
            }
            _flights.Add((slot, sent, [.. batch]));
        }
        // The frame's time on the main thread in milliseconds and the columns asked for and not
        // yet read, which the frame's profile shows.
        _ms += ((float)clock.Elapsed.TotalMilliseconds - _ms) * 0.05f;
        SetProfileValue("gen.gpu.ms", _ms);
        SetProfileValue("gen.gpu.backlog", Backlog);
    }

    // Takes the oldest batch's readings, waiting for them where they are not yet back, each column's
    // a part of one array the workers read from.
    private void Land(Func<int, int, bool>? wanted)
    {
        var (_, readback, columns) = _flights[0];
        _flights.RemoveAt(0);
        var values = new int[Batch * Caves.Corners * _samplers];
        GetShaderReadback<int>(readback, values);
        UnloadShaderReadback(readback);
        var each = Caves.Corners * _samplers;
        for (int i = 0; i < columns.Length; i++)
        {
            _waiting.Remove(columns[i]);
            if (wanted?.Invoke(columns[i].X, columns[i].Z) ?? true) _ready[columns[i]] = new ArraySegment<int>(values, i * each, each);
        }
        Read += columns.Length;
    }

    // Dispatches a batch's columns into a buffer of its own and asks for its readings, a short batch's
    // last column read again in its place.
    private ShaderReadback Send(IReadOnlyList<(int X, int Z)> batch, int slot)
    {
        var origins = new Origin[Batch];
        for (int i = 0; i < Batch; i++)
        {
            var (x, z) = batch[Math.Min(i, batch.Count - 1)];
            origins[i] = new Origin(x * Section.Size, z * Section.Size);
        }
        SetShaderValueV<Origin>(_shader, _originsAt, origins);
        SetShaderValueBuffer(_shader, _valuesAt, _values[slot]);
        var threads = Batch * Caves.Corners * _samplers;
        ComputeShaderDispatch(_shader, (threads + 63) / 64, 1, 1);
        return ReadShaderBufferAsync(_values[slot]);
    }

    // A device that reads nothing back, as one with no compute, turns the fields off, the columns
    // asked for left to read their own noises.
    private void Fail()
    {
        _failed = true;
        _asked.Clear();
        _waiting.Clear();
    }

    /// <summary>
    /// Reads columns' readings on the GPU, waiting for them, and on the CPU through the caves'
    /// samplers, and says how many readings differ, which a world whose paths agree to the bit has none of.
    /// </summary>
    /// <returns>The readings compared and those that differ, or null where the GPU does not read them.</returns>
    public (int Compared, int Differ)? Verify(IReadOnlyList<(int X, int Z)> columns)
    {
        if (_caves is null || !_shader.IsValid || _failed) return null;
        // The batches in flight come back first, so their buffers are free.
        while (_flights.Count > 0) Land(null);
        int compared = 0, differ = 0;
        var values = new int[Batch * Caves.Corners * _samplers];
        foreach (var chunk in columns.Chunk(Batch))
        {
            var sent = Send(chunk, 0);
            if (!sent.IsValid) return null;
            GetShaderReadback<int>(sent, values);
            UnloadShaderReadback(sent);
            for (int i = 0; i < chunk.Length; i++)
                for (int corner = 0; corner < Caves.Corners; corner++)
                {
                    var (dx, y, dz) = Caves.CornerAt(corner);
                    int x = chunk[i].X * Section.Size + dx, z = chunk[i].Z * Section.Size + dz;
                    for (int s = 0; s < _samplers; s++)
                    {
                        compared++;
                        if (values[(i * Caves.Corners + corner) * _samplers + s] != _caves.Samplers[s].FixedAt(x, y, z)) differ++;
                    }
                }
        }
        return (compared, differ);
    }

    // Lets the last world's buffers and readings go.
    private void Forget()
    {
        foreach (var (_, readback, _) in _flights) UnloadShaderReadback(readback);
        _flights.Clear();
        foreach (var buffer in _values.Append(_ranges).Append(_levels).Append(_permutations))
            if (IsShaderBufferValid(buffer)) UnloadShaderBuffer(buffer);
        Array.Clear(_values);
        (_ranges, _levels, _permutations) = (default, default, default);
        _asked.Clear();
        _waiting.Clear();
        _ready.Clear();
        _failed = false;
    }

    public void Dispose()
    {
        Forget();
        if (_shader.IsValid) UnloadShader(_shader);
    }
}
