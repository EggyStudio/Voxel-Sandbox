using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The world's small moving things of the modpack's kind, raindrops splashing on the blocks they
/// meet and spreading in rings where they meet water, leaves falling from leaf blocks and turning
/// as they fall, dust drifting in the caves with spores where moss grows, and the wakes of the
/// player and the mobs moving through water, with embers rising where lava lies near in the dark.
/// </summary>
/// <remarks>
/// <para>
/// Each kind has a density from 0, none, to 2, twice its own, which its setting sets, 1 by default.
/// At 1 the rain splashes 60 times a second at the columns within eight blocks of the eye where it
/// rains, the leaves fall four a second from the leaf blocks within sixteen with three blocks of air
/// or more under them, the dust drifts twelve a second within ten blocks of the eye while it stands
/// in a cave, under the sea's level with little of the sky's light, the spores six a second where
/// moss grows near and the embers four where lava does, and a body moving through water's top
/// throws forty drops a second.
/// </para>
/// <para>
/// They are the engine's particles but the rings. The engine steps, sorts and draws every emitter
/// each frame to its whole capacity, holding particles or not, so an emitter is made as a burst
/// needs one and let go once its last particles have died, and none is kept on a clear day in the
/// open with no trees near. An emitter bursts where it stands in the frame after it is told to, so
/// a kind bursting at two places in a frame has two emitters, the frame's first burst going to the
/// first. None of them meets the scene, which would cost the frame a pass drawing the window's
/// depth, so each lives no longer than it takes to reach the ground. A splash's drops fall back
/// under the block they leapt from, and a leaf, falling about two blocks over its life, falls only
/// where three blocks of air lie under it.
/// </para>
/// <para>
/// A particle faces the eye, so a ring would stand up off the water, and the rings are a mesh of
/// flat squares built each frame instead, each showing the frame of a sheet its age has reached,
/// spreading and fading. The frame's profile counts each kind as what it emits a second over about
/// the last second times its life, the particles it keeps alive, and the emitters held.
/// </para>
/// </remarks>
public sealed class AmbientParticles : IDisposable
{
    /// <summary>The splashes, leaves, dust, spores, embers and a body's wake's drops a second at a density of 1.</summary>
    public const float Splashes = 60, Leaves = 4, Dust = 12, Spores = 6, Embers = 4, Wake = 40;

    /// <summary>The seconds a ring takes to spread and fade, and the frames of its sheet.</summary>
    public const float RingLife = 0.6f;

    public const int RingFrames = 6;

    /// <summary>The air under a leaf block a leaf falls from, more than it falls over its life.</summary>
    public const int LeafFall = 3;

    private const int Reach = 8, LeafReach = 16, MostRings = 64, DropsASplash = 3;
    private const float SplashLife = 0.3f, LeafLife = 6, DustLife = 6, WakeLife = 0.4f;

    private readonly Texture2D _drop = Load(Dot(4, 200)), _ring = Load(Rings()), _leaf = Load(LeafFrames()), _mote = Load(Dot(4, 140));
    private readonly Pool _splashes, _leaves, _wakes;
    private readonly Stream _dust, _spores, _embers;
    private readonly List<(Vector3 At, float Age)> _rings = [];
    private readonly Random _random = new();
    private ModelMesh _ringMesh;
    private float _splashDue, _leafDue, _wakeDue;
    private double _time, _mossSeen = double.MinValue, _lavaSeen = double.MinValue;

    /// <summary>How thick the splashes and the rings fall, from 0, none, to 2, as the Particles screen sets it, and the others below likewise.</summary>
    public float SplashDensity { get; set; } = 1;

    public float LeafDensity { get; set; } = 1;

    /// <summary>How thick the dust drifts in the caves, the spores and the embers with it.</summary>
    public float DustDensity { get; set; } = 1;

    public float WakeDensity { get; set; } = 1;

    /// <summary>The most particles the emitters hold together and the rings, each emitter's cap summed over the most of it there may be.</summary>
    public int Budget => _splashes.Cap + _leaves.Cap + _wakes.Cap + _dust.Cap + _spores.Cap + _embers.Cap + MostRings;

    /// <summary>The emitters held this frame.</summary>
    public int Emitters => _splashes.Count + _leaves.Count + _wakes.Count + _dust.Count + _spores.Count + _embers.Count;

    /// <summary>The particles of each kind kept alive, what it emits a second over about the last
    /// second times their life, which the frame's profile shows.</summary>
    public (float Splashes, float Leaves, float Dust, float Wakes) Alive { get; private set; }

    // Each emitter's cap holds what it keeps alive at a density of 2 at the longest lives, a kind's
    // bursts shared among as many emitters as it may burst at in a frame of a thirtieth of a second.
    public AmbientParticles()
    {
        _splashes = new Pool(3, SplashLife * 1.3f, new ParticleEmitter
        {
            MaxParticles = 48, Emitting = false, Life = SplashLife, LifeVariation = 0.3f, Velocity = new Vector3(0, 2, 0), Spread = 60,
            SpeedVariation = 0.5f, Gravity = new Vector3(0, -14, 0), StartSize = 0.06f, EndSize = 0.04f, StartColor = new Color(200, 215, 240, 210),
            EndColor = new Color(200, 215, 240, 0), Intensity = 1, Lit = true, Blend = ParticleBlend.Alpha, Texture = _drop,
        });
        _leaves = new Pool(2, LeafLife * 1.3f, new ParticleEmitter
        {
            MaxParticles = 32, Emitting = false, Life = LeafLife, LifeVariation = 0.3f, Velocity = new Vector3(0, -0.3f, 0), Spread = 80, SpeedVariation = 0.6f,
            Gravity = new Vector3(0.15f, -0.25f, 0.1f), Drag = 0.6f, StartSize = 0.3f, EndSize = 0.28f, StartColor = Color.White, EndColor = Color.White with { A = 0 },
            Intensity = 1, Lit = true, Blend = ParticleBlend.Alpha, Texture = _leaf, TextureColumns = LeafFramesAcross, TextureRows = 1,
        });
        _wakes = new Pool(3, WakeLife * 1.3f, new ParticleEmitter
        {
            MaxParticles = 48, Emitting = false, Life = WakeLife, LifeVariation = 0.3f, Radius = 0.4f, Velocity = new Vector3(0, 1.6f, 0), Spread = 70,
            SpeedVariation = 0.5f, Gravity = new Vector3(0, -10, 0), StartSize = 0.07f, EndSize = 0.05f, StartColor = new Color(235, 240, 250, 220),
            EndColor = new Color(235, 240, 250, 0), Intensity = 1, Lit = true, Blend = ParticleBlend.Alpha, Texture = _drop,
        });
        _dust = new Stream(DustLife * 1.4f, new ParticleEmitter
        {
            MaxParticles = 208, Emitting = false, Life = DustLife, LifeVariation = 0.4f, Radius = 10, Velocity = new Vector3(0, 0.05f, 0), Spread = 180,
            SpeedVariation = 1, Drag = 0.2f, StartSize = 0.04f, EndSize = 0.04f, StartColor = new Color(170, 165, 155, 150), EndColor = new Color(170, 165, 155, 0),
            Intensity = 1, Lit = true, Blend = ParticleBlend.Alpha, Texture = _mote,
        });
        _spores = new Stream(DustLife * 1.4f, new ParticleEmitter
        {
            MaxParticles = 112, Emitting = false, Life = DustLife, LifeVariation = 0.4f, Radius = 8, Velocity = new Vector3(0, -0.1f, 0), Spread = 120,
            SpeedVariation = 1, Drag = 0.2f, StartSize = 0.04f, EndSize = 0.03f, StartColor = new Color(170, 230, 120, 200), EndColor = new Color(170, 230, 120, 0),
            Intensity = 1, Lit = true, Blend = ParticleBlend.Alpha, Texture = _mote,
        });
        // An ember emits its own light, added over what lies behind it, and rises as it cools.
        _embers = new Stream(DustLife * 1.4f, new ParticleEmitter
        {
            MaxParticles = 80, Emitting = false, Life = DustLife, LifeVariation = 0.4f, Radius = 8, Velocity = new Vector3(0, 0.3f, 0), Spread = 40,
            SpeedVariation = 0.7f, Drag = 0.3f, Gravity = new Vector3(0, 0.05f, 0), StartSize = 0.08f, EndSize = 0.03f,
            StartColor = new Color(255, 150, 50, 255), EndColor = new Color(255, 60, 10, 0), Intensity = 4, Lit = false, Blend = ParticleBlend.Additive, Texture = _mote,
        });
    }

    /// <summary>
    /// Steps the particles by so many seconds, the rain's splashes as far as it rains, the leaves
    /// about the eye, the dust and the spores where the eye stands in a cave, and the wakes of the
    /// bodies given that move through water.
    /// </summary>
    public void Update(VoxelWorld world, Vector3 eye, float rain, float seconds, IEnumerable<Body> swimmers)
    {
        if (seconds <= 0) return;
        _time += seconds;
        _splashes.Begin();
        _leaves.Begin();
        _wakes.Begin();
        for (int i = 0; i < _rings.Count; i++) _rings[i] = _rings[i] with { Age = _rings[i].Age + seconds };
        _rings.RemoveAll(ring => ring.Age >= RingLife);

        // The rain's splashes at columns where it rains, rings where its top is water.
        int splashed = 0, fallen = 0, thrown = 0;
        for (int due = Due(ref _splashDue, Splashes * SplashDensity * rain, seconds, _splashes.Most); due > 0; due--)
        {
            int x = (int)MathF.Floor(eye.X) + _random.Next(-Reach, Reach + 1), z = (int)MathF.Floor(eye.Z) + _random.Next(-Reach, Reach + 1);
            if (world.BiomeAt(x, z)?.Precipitation != Precipitation.Rain) continue;
            var top = world.Top(x, z);
            var at = new Vector3(x + (float)_random.NextDouble(), top + 1, z + (float)_random.NextDouble());
            var block = world.GetBlock(x, top, z);
            if (Blocks.IsWater(block))
            {
                // A hair over the water's top, which a still source's is an eighth under its cell's.
                if (_rings.Count < MostRings) _rings.Add((at with { Y = top + Blocks.WaterHeight(block, false) + 0.01f }, 0));
                continue;
            }
            _splashes.Burst(at, DropsASplash, _time);
            splashed++;
        }

        // A leaf falls from the lowest of a column's top leaves near the eye, which the canopy holds
        // a few deep, where enough air lies under it, up to eight columns tried for each, so a wood
        // gives its four a second and a lone tree a few. None falls while the eye is in a cave, where
        // it would not be seen.
        int ex = (int)MathF.Floor(eye.X), ey = (int)MathF.Floor(eye.Y), ez = (int)MathF.Floor(eye.Z);
        var inCave = eye.Y < Overworld.SeaLevel && world.GetLight(ex, ey, ez).Sky <= 3;
        for (int due = Due(ref _leafDue, inCave ? 0 : Leaves * LeafDensity, seconds, _leaves.Most); due > 0; due--)
            for (int tries = 0; tries < 8; tries++)
            {
                int x = (int)MathF.Floor(eye.X) + _random.Next(-LeafReach, LeafReach + 1), z = (int)MathF.Floor(eye.Z) + _random.Next(-LeafReach, LeafReach + 1);
                var y = world.Top(x, z);
                if (!Blocks.IsLeaves(world.GetBlock(x, y, z))) continue;
                for (var deep = 0; deep < 6 && Blocks.IsLeaves(world.GetBlock(x, y - 1, z)); deep++) y--;
                if (!Enumerable.Range(1, LeafFall).All(below => world.GetBlock(x, y - below, z) == BlockId.Air)) continue;
                _leaves.Burst(new Vector3(x + (float)_random.NextDouble(), y - 0.1f, z + (float)_random.NextDouble()), 1, _time);
                fallen++;
                break;
            }

        // Dust in a cave, under the sea's level where little of the sky's light reaches the eye, with
        // spores where moss grows near and embers where lava lies.
        var dustRate = inCave ? Dust * DustDensity : 0;
        // The moss and the lava are looked for among a few blocks a frame, and the spores and the
        // embers kept while either was seen in the last two seconds, so they do not flicker on and
        // off as the tries miss.
        if (inCave && DustDensity > 0)
            for (int i = 0; i < 12; i++)
                switch (world.GetBlock(ex + _random.Next(-8, 9), ey + _random.Next(-5, 6), ez + _random.Next(-8, 9)))
                {
                    case BlockId.MossBlock or BlockId.MossCarpet:
                        _mossSeen = _time;
                        break;
                    case BlockId.Lava:
                        _lavaSeen = _time;
                        break;
                }
        var sporeRate = inCave && _time - _mossSeen < 2 ? Spores * DustDensity : 0;
        var emberRate = inCave && _time - _lavaSeen < 2 ? Embers * DustDensity : 0;
        _dust.Follow(eye, dustRate, _time);
        _spores.Follow(eye, sporeRate, _time);
        _embers.Follow(eye, emberRate, _time);

        // The drops a body throws moving through water's top, from where the top meets it.
        foreach (var body in swimmers)
        {
            if (WakeDensity <= 0 || body.Velocity.X * body.Velocity.X + body.Velocity.Z * body.Velocity.Z < 1 || Surface(world, body) is not { } top) continue;
            var count = Due(ref _wakeDue, Wake * WakeDensity, seconds, 12);
            if (count <= 0) continue;
            _wakes.Burst(body.Position with { Y = top }, count, _time);
            thrown += count;
        }

        _splashes.Retire(_time);
        _leaves.Retire(_time);
        _wakes.Retire(_time);
        // What this frame emitted, as many a second, kept alive over its life, eased over about a
        // second, since the profile shows the last frame's value and a frame emits few or none.
        var ease = 1 - MathF.Exp(-seconds);
        Alive = (Ease(Alive.Splashes, splashed * DropsASplash / seconds * SplashLife), Ease(Alive.Leaves, fallen / seconds * LeafLife),
            (dustRate + sporeRate + emberRate) * DustLife, Ease(Alive.Wakes, thrown / seconds * WakeLife));
        float Ease(float was, float now) => was + (now - was) * ease;
        SetProfileValue("particles.splashes", Alive.Splashes);
        SetProfileValue("particles.rings", _rings.Count);
        SetProfileValue("particles.leaves", Alive.Leaves);
        SetProfileValue("particles.dust", Alive.Dust);
        SetProfileValue("particles.wakes", Alive.Wakes);
        SetProfileValue("particles.emitters", Emitters);
        SetProfileValue("particles.budget", Budget);
    }

    /// <summary>Draws the rings spreading on the water, a flat square each, the frame of the sheet its age has reached.</summary>
    public void Draw()
    {
        if (_ringMesh.IsValid) UnloadMesh(_ringMesh);
        _ringMesh = default;
        if (_rings.Count == 0) return;
        var vertices = new ModelVertex[_rings.Count * 4];
        var indices = new uint[_rings.Count * 6];
        for (int i = 0; i < _rings.Count; i++)
        {
            var (at, age) = _rings[i];
            var frame = Math.Min(RingFrames - 1, (int)(age / RingLife * RingFrames));
            float u = frame / (float)RingFrames, across = 1f / RingFrames, half = 0.45f;
            vertices[i * 4] = new ModelVertex(at + new Vector3(-half, 0, -half), Vector3.UnitY, new Vector2(u, 0));
            vertices[i * 4 + 1] = new ModelVertex(at + new Vector3(-half, 0, half), Vector3.UnitY, new Vector2(u, 1));
            vertices[i * 4 + 2] = new ModelVertex(at + new Vector3(half, 0, half), Vector3.UnitY, new Vector2(u + across, 1));
            vertices[i * 4 + 3] = new ModelVertex(at + new Vector3(half, 0, -half), Vector3.UnitY, new Vector2(u + across, 0));
            var first = (uint)(i * 4);
            ((ReadOnlySpan<uint>)[first, first + 1, first + 2, first, first + 2, first + 3]).CopyTo(indices.AsSpan(i * 6));
        }
        _ringMesh = UploadMesh(vertices, indices);
        DrawMesh(_ringMesh, new ModelMaterial(Color.White, _ring)
        {
            AlphaMode = MaterialAlphaMode.Blend, DoubleSided = true, CastsShadows = false, BouncesLight = false, Roughness = 1,
        }, Matrix4x4.Identity);
    }

    /// <summary>
    /// How many places are due in a frame at a rate a second, the share of one carried to the next
    /// frame, at most so many, the emitters there may be to burst at once.
    /// </summary>
    public static int Due(ref float carry, float rate, float seconds, int most)
    {
        carry += rate * seconds;
        var count = Math.Min(most, (int)carry);
        // A share piled up past what the emitters can show is let go, so a long frame bursts no more.
        carry = MathF.Min(carry - count, most);
        return count;
    }

    // Where water's top crosses a body, which is in water there and out of it above, or none.
    private static float? Surface(VoxelWorld world, Body body)
    {
        int x = (int)MathF.Floor(body.Position.X), z = (int)MathF.Floor(body.Position.Z);
        for (var y = (int)MathF.Floor(body.Position.Y); y <= (int)MathF.Floor(body.Position.Y + body.Height); y++)
        {
            var block = world.GetBlock(x, y, z);
            if (!Blocks.IsWater(block) || Blocks.IsWater(world.GetBlock(x, y + 1, z))) continue;
            var top = y + Blocks.WaterHeight(block, false);
            return top > body.Position.Y && top < body.Position.Y + body.Height ? top : null;
        }
        return null;
    }

    // The emitters of a kind that bursts at places, made as a frame's bursts need them, the frame's
    // first burst to the first, and the last let go once its particles have died.
    private sealed class Pool(int most, float linger, ParticleEmitter settings)
    {
        private readonly List<(ParticleEmitterHandle Emitter, double Last)> _emitters = [];
        private int _next;

        public int Most => most;

        public int Count => _emitters.Count;

        public int Cap => most * settings.MaxParticles;

        public void Begin() => _next = 0;

        public void Burst(Vector3 at, int count, double now)
        {
            if (_next >= most) return;
            if (_next == _emitters.Count) _emitters.Add((CreateParticleEmitter(at, settings), now));
            var emitter = _emitters[_next].Emitter;
            _emitters[_next++] = (emitter, now);
            SetParticleEmitterPosition(emitter, at);
            EmitParticles(emitter, count);
        }

        public void Retire(double now)
        {
            while (_emitters.Count > 0 && now - _emitters[^1].Last > linger)
            {
                UnloadParticleEmitter(_emitters[^1].Emitter);
                _emitters.RemoveAt(_emitters.Count - 1);
            }
        }

        public void Dispose()
        {
            foreach (var (emitter, _) in _emitters) UnloadParticleEmitter(emitter);
            _emitters.Clear();
        }
    }

    // An emitter that follows the eye at a rate, made as it begins and let go once its particles have
    // died after it stopped.
    private sealed class Stream(float linger, ParticleEmitter settings)
    {
        private ParticleEmitterHandle? _emitter;
        private double _last;
        private float _rate;

        public int Count => _emitter is null ? 0 : 1;

        public int Cap => settings.MaxParticles;

        public void Follow(Vector3 eye, float rate, double now)
        {
            if (rate > 0) _last = now;
            if (_emitter is not { } emitter)
            {
                if (rate <= 0) return;
                _emitter = emitter = CreateParticleEmitter(eye, settings);
                _rate = 0;
            }
            else if (rate <= 0 && now - _last > linger)
            {
                Dispose();
                return;
            }
            SetParticleEmitterPosition(emitter, eye);
            if (_rate == rate) return;
            _rate = rate;
            var next = settings;
            next.Rate = rate;
            next.Emitting = rate > 0;
            SetParticleEmitter(emitter, next);
        }

        public void Dispose()
        {
            if (_emitter is { } emitter) UnloadParticleEmitter(emitter);
            _emitter = null;
        }
    }

    private static Texture2D Load((byte[] Pixels, int Width, int Height) picture)
    {
        var texture = LoadTextureFromImage(new Image(picture.Pixels, picture.Width, picture.Height));
        SetTextureFilter(texture, TextureFilter.Point);
        return texture;
    }

    /// <summary>A round dot a few pixels across, white, of an alpha, for the drops and the motes.</summary>
    public static (byte[] Pixels, int Width, int Height) Dot(int side, byte alpha)
    {
        var pixels = new byte[side * side * 4];
        var middle = (side - 1) / 2f;
        for (int y = 0; y < side; y++)
            for (int x = 0; x < side; x++)
                if ((x - middle) * (x - middle) + (y - middle) * (y - middle) <= middle * middle + 0.5f)
                {
                    pixels.AsSpan((y * side + x) * 4, 3).Fill(255);
                    pixels[(y * side + x) * 4 + 3] = alpha;
                }
        return (pixels, side, side);
    }

    /// <summary>
    /// The ring a raindrop leaves on water, pale and a pixel wide, through <see cref="RingFrames"/>
    /// frames of sixteen pixels side by side, spreading from two pixels across to fifteen and fading.
    /// </summary>
    public static (byte[] Pixels, int Width, int Height) Rings()
    {
        const int side = 16;
        var pixels = new byte[side * RingFrames * side * 4];
        for (int frame = 0; frame < RingFrames; frame++)
        {
            float radius = 1 + 6.5f * frame / (RingFrames - 1), share = 1 - frame / (float)RingFrames;
            for (int y = 0; y < side; y++)
                for (int x = 0; x < side; x++)
                {
                    var r = MathF.Sqrt((x - 7.5f) * (x - 7.5f) + (y - 7.5f) * (y - 7.5f));
                    if (MathF.Abs(r - radius) > 0.6f) continue;
                    var at = (y * side * RingFrames + frame * side + x) * 4;
                    (pixels[at], pixels[at + 1], pixels[at + 2], pixels[at + 3]) = (220, 230, 250, (byte)(170 * share));
                }
        }
        return (pixels, side * RingFrames, side);
    }

    // The frames of a falling leaf's sheet, which it plays once over its life, two whole turns.
    private const int LeafFramesAcross = 16;

    /// <summary>A leaf of the oak's green six pixels long, turned an eighth of a turn a frame through sixteen frames side by side, which a falling leaf plays through as it turns.</summary>
    public static (byte[] Pixels, int Width, int Height) LeafFrames()
    {
        const int side = 8, frames = LeafFramesAcross;
        var pixels = new byte[side * frames * side * 4];
        for (int frame = 0; frame < frames; frame++)
        {
            // The leaf looks the same turned half a turn, so an eighth a frame turns it twice in sixteen.
            var angle = frame * MathF.PI / 4;
            var (c, s) = (MathF.Cos(angle), MathF.Sin(angle));
            for (int y = 0; y < side; y++)
                for (int x = 0; x < side; x++)
                {
                    float u = (x - 3.5f) * c + (y - 3.5f) * s, v = -(x - 3.5f) * s + (y - 3.5f) * c;
                    if (u * u / 9 + v * v / 2.2f > 1) continue;
                    var at = (y * side * frames + frame * side + x) * 4;
                    (pixels[at], pixels[at + 1], pixels[at + 2], pixels[at + 3]) = ((byte)(MathF.Abs(v) < 0.5f ? 70 : 90), (byte)(MathF.Abs(v) < 0.5f ? 120 : 150), 40, 255);
                }
        }
        return (pixels, side * frames, side);
    }

    public void Dispose()
    {
        if (_ringMesh.IsValid) UnloadMesh(_ringMesh);
        _splashes.Dispose();
        _leaves.Dispose();
        _wakes.Dispose();
        _dust.Dispose();
        _spores.Dispose();
        _embers.Dispose();
        foreach (var texture in new[] { _drop, _ring, _leaf, _mote }) UnloadTexture(texture);
    }
}
