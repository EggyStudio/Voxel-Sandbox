using System.Numerics;
using System.Runtime.InteropServices;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The bits a block throws as it is hit and as it breaks, each a square cut from a quarter of its
/// side's picture, as Minecraft's are, falling, resting on the blocks they land on, and gone at the
/// end of their lives.
/// </summary>
/// <remarks>
/// <para>
/// A break throws 64 from a grid of four by four by four through the block, outward from its
/// middle, and each tick a block is broken over it throws one from the face hit, slower and smaller,
/// with Minecraft's numbers, a fall of 16 blocks a second each second, a drag of a fiftieth a tick, a
/// life of a fifth of a second to two and a square a tenth to a fifth of a block across. A bit on the
/// ground slides to a stop.
/// </para>
/// <para>
/// A bit's square is one of four quarters of the side's picture, the pack's as the terrain draws it,
/// tinted by the climate's colors where the block stood as its faces are tinted there, so a grass
/// block broken in the savanna throws the savanna's grass, or the side's own color in that climate
/// where no pack gives one, each turned toward the eye. They cast no shadow and global illumination
/// holds none of them. Each tint of a quarter is a texture of its own, and the climate's colors change
/// from place to place, so past 256 of them those no bit wears are let go of.
/// </para>
/// <para>
/// A square is lit by the engine's light as its face toward the eye takes it, through the game's
/// bits shader, which reads the shadow and the probes at the bit's middle, which its body keeps in
/// the open, where a square clipped into the wall beside it read the sun's and the sky's light from
/// inside the wall at its clipped part, and no less than the terrain's floor of the sky's light at
/// the sky level of the bit's cell.
/// </para>
/// </remarks>
public sealed class BlockBits : IDisposable
{
    /// <summary>How fast a bit falls faster, in blocks a second each second, as Minecraft's 0.04 a tick each tick.</summary>
    public const float Gravity = 16;

    /// <summary>How many bits a break throws.</summary>
    public const int BreakCount = 64;

    /// <summary>The longest a bit lives, in seconds, as Minecraft's 40 ticks.</summary>
    public const float MostLife = 2;

    private const float Tick = 0.05f;
    private const int Pieces = 4;

    /// <summary>A bit thrown, which picture it wears in which climate, where it is and how fast it goes, how long it lives and has lived, and how wide it is.</summary>
    public sealed class Bit(int surface, int piece, Vector3 at, Vector3 velocity, float life, float size, (Color Grass, Color Foliage) climate)
    {
        public int Surface { get; } = surface;
        public int Piece { get; } = piece;
        public Body Body { get; } = new(size / 2, size) { Position = at, Velocity = velocity };
        public float Life { get; } = life;
        public float Age { get; set; }
        public float Size { get; } = size;

        /// <summary>The grass's and the leaves' colors where its block stood, which tint its picture as the block's faces are tinted there.</summary>
        public (Color Grass, Color Foliage) Climate { get; } = climate;

        /// <summary>The sky's light level in the cell of the bit's middle, as its last move left it.</summary>
        public int Sky { get; set; }
    }

    private readonly List<Bit> _bits = [];
    private readonly Random _random = new(1);
    private const int MostPictures = 256;

    private readonly Dictionary<(int Surface, int Piece, Color Tint), Texture2D> _pictures = [];
    private readonly Dictionary<(int Surface, int Piece, int Sky, Color Tint), ModelMaterial> _materials = [];
    private readonly Dictionary<(int Surface, int Piece, int Sky, Color Tint), List<Matrix4x4>> _draws = [];
    private readonly List<Texture2D> _textures = [];
    private TerrainAtlas _terrain = TerrainAtlas.None;
    private ModelMesh? _square;
    private Shader? _shader;
    private float _hitTime;

    /// <summary>Whether the bits hold where they are and age no more, for a capture, as the mobs and the clouds hold.</summary>
    public bool Held { get; set; }

    /// <summary>The bits alive.</summary>
    public IReadOnlyList<Bit> All => _bits;

    /// <summary>Draws the bits with the pictures of the packs' terrain from the next frame, as the packs change.</summary>
    public void Use(TerrainAtlas terrain)
    {
        Forget();
        _terrain = terrain;
    }

    /// <summary>
    /// Throws the bits of a block broken at a place, outward from its middle, all 64 of a block the
    /// player breaks, or fewer at places of the grid taken at random, as the logs and leaves of a
    /// felled tree throw, many going at once, tinted by the climate's grass and leaves at the block's
    /// place, the plains' where none is given.
    /// </summary>
    public void Break(BlockId block, int x, int y, int z, int count = BreakCount, (Color Grass, Color Foliage)? climate = null)
    {
        var surface = Blocks.Get(block).Side;
        var colors = climate ?? Plains;
        if (count < BreakCount)
        {
            for (int n = 0; n < count; n++)
            {
                var within = new Vector3((_random.Next(4) + 0.5f) / 4, (_random.Next(4) + 0.5f) / 4, (_random.Next(4) + 0.5f) / 4);
                Throw(surface, new Vector3(x, y, z) + within, within - new Vector3(0.5f), power: 1, scale: 1, colors);
            }
            return;
        }
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++)
                for (int k = 0; k < 4; k++)
                {
                    var within = new Vector3((i + 0.5f) / 4, (j + 0.5f) / 4, (k + 0.5f) / 4);
                    Throw(surface, new Vector3(x, y, z) + within, within - new Vector3(0.5f), power: 1, scale: 1, colors);
                }
    }

    /// <summary>
    /// Throws a bit from the face of a block hit, for each tick of a frame of so many seconds, as a
    /// block being broken throws one a tick in Minecraft, tinted by the climate's grass and leaves at
    /// the block's place, the plains' where none is given.
    /// </summary>
    public void Hit(BlockId block, BlockHit hit, float seconds, (Color Grass, Color Foliage)? climate = null)
    {
        var surface = Blocks.Get(block).Side;
        var colors = climate ?? Plains;
        var normal = new Vector3(hit.NormalX, hit.NormalY, hit.NormalZ);
        _hitTime += seconds;
        var ticks = (int)((_hitTime + 1e-4f) / Tick);
        _hitTime -= ticks * Tick;
        for (int i = 0; i < ticks; i++)
        {
            // A place on the face a tenth of a block out from it and a tenth in from its edges.
            var across = new Vector3(0.1f + 0.8f * Next(), 0.1f + 0.8f * Next(), 0.1f + 0.8f * Next());
            var at = new Vector3(hit.X, hit.Y, hit.Z) + across * (Vector3.One - Vector3.Abs(normal)) + (normal * 0.5f + new Vector3(0.5f)) * Vector3.Abs(normal) + normal * 0.1f;
            Throw(surface, at, Vector3.Zero, power: 0.2f, scale: 0.6f, colors);
        }
    }

    // A bit thrown as Minecraft's particle starts, its way turned at random and its speed from 0.06
    // to 0.18 of a block a tick with a tenth of a block a tick upward, a hit's slowed by its power.
    private void Throw(int surface, Vector3 at, Vector3 way, float power, float scale, (Color Grass, Color Foliage) climate)
    {
        var velocity = way + new Vector3(Next() * 2 - 1, Next() * 2 - 1, Next() * 2 - 1) * 0.4f;
        var speed = (Next() + Next() + 1) * 0.15f;
        velocity = velocity.LengthSquared() > 0 ? Vector3.Normalize(velocity) * speed * 0.4f : Vector3.Zero;
        velocity = new Vector3(velocity.X * power, velocity.Y * power + 0.1f, velocity.Z * power);
        var life = MathF.Min(MostLife, 4 / (Next() * 0.9f + 0.1f) * Tick);
        var size = 0.2f * (Next() * 0.5f + 0.5f) * scale;
        _bits.Add(new Bit(surface, _random.Next(Pieces), at, velocity / Tick, life, size, climate));
    }

    private static readonly (Color Grass, Color Foliage) Plains = (Biomes.Plains.GrassColor, Biomes.Plains.FoliageColor);

    private float Next() => _random.NextSingle();

    /// <summary>Moves the bits on by so many seconds, falling and resting on what they land on, and lets go of those whose lives are over.</summary>
    public void Update(VoxelWorld world, float seconds)
    {
        if (Held) return;
        var drag = MathF.Pow(0.98f, seconds / Tick);
        var ground = MathF.Pow(0.7f, seconds / Tick);
        foreach (var bit in _bits)
        {
            bit.Age += seconds;
            var body = bit.Body;
            body.Velocity.Y -= Gravity * seconds;
            body.Velocity *= drag;
            if (body.OnGround) (body.Velocity.X, body.Velocity.Z) = (body.Velocity.X * ground, body.Velocity.Z * ground);
            body.Move(world, body.Velocity * seconds, keepToEdges: false);
            var middle = body.Position + new Vector3(0, bit.Size / 2, 0);
            bit.Sky = world.GetLight((int)MathF.Floor(middle.X), (int)MathF.Floor(middle.Y), (int)MathF.Floor(middle.Z)).Sky;
        }
        _bits.RemoveAll(b => b.Age >= b.Life);
    }

    /// <summary>Draws the bits, each square turned toward the eye. Called inside <c>BeginMode3D</c>.</summary>
    public void Draw(Vector3 eye, Vector3 forward)
    {
        if (_bits.Count == 0) return;
        foreach (var list in _draws.Values) list.Clear();
        foreach (var bit in _bits)
        {
            var key = (bit.Surface, bit.Piece, bit.Sky, _terrain.Paint(bit.Surface, bit.Climate.Grass, bit.Climate.Foliage).Tint);
            if (!_draws.TryGetValue(key, out var list)) _draws[key] = list = [];
            if (!_materials.ContainsKey(key)) _materials[key] = Pictured(key, bit.Climate);
            var middle = bit.Body.Position + new Vector3(0, bit.Size / 2, 0);
            list.Add(Matrix4x4.CreateScale(bit.Size) * Matrix4x4.CreateBillboard(middle, eye, Vector3.UnitY, forward));
        }
        var square = _square ??= Square();
        foreach (var (key, list) in _draws)
        {
            if (list.Count == 0) continue;
            DrawMeshInstanced(square, _materials[key], CollectionsMarshal.AsSpan(list));
        }
        if (_pictures.Count > MostPictures) LetGoUnworn();
    }

    // The picture of a quarter of a surface in its tint, with the sky's light level of the bits drawn
    // with it as its occlusion strength, which the bits' shader reads for its floor of the sky's light,
    // a bit having no occlusion map for the strength to darken.
    private ModelMaterial Pictured((int Surface, int Piece, int Sky, Color Tint) key, (Color Grass, Color Foliage) climate)
    {
        if (!_pictures.TryGetValue((key.Surface, key.Piece, key.Tint), out var texture))
        {
            texture = LoadTextureFromImage(Picture(_terrain, key.Surface, key.Piece, climate));
            SetTextureFilter(texture, TextureFilter.Point);
            _textures.Add(texture);
            _pictures[(key.Surface, key.Piece, key.Tint)] = texture;
        }
        return new ModelMaterial(Color.White, texture)
        {
            AlphaMode = MaterialAlphaMode.Mask, Roughness = 1, CastsShadows = false, BouncesLight = false, DoubleSided = true,
            OcclusionStrength = key.Sky / (float)Lighting.Max,
            Shader = _shader ??= LoadShader("resources/shaders/bits.slang"),
        };
    }

    /// <summary>
    /// The picture a bit wears, a quarter of a surface's picture at one of four places a quarter of the
    /// picture in from its corners, each texel tinted by a climate's colors as the terrain draws it, or
    /// the surface's own color in that climate where no pack gives it a picture.
    /// </summary>
    public static Image Picture(TerrainAtlas terrain, int surface, int piece, (Color Grass, Color Foliage) climate)
    {
        var n = Math.Max(1, terrain.Pixels / 4);
        var (left, top) = (terrain.Pixels / 8 + piece % 2 * terrain.Pixels / 2, terrain.Pixels / 8 + piece / 2 * terrain.Pixels / 2);
        var own = terrain.Paint(surface, climate.Grass, climate.Foliage).Tint;
        var pixels = new byte[n * n * 4];
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                var texel = terrain.Texel(surface, (left + x + 0.5f) / terrain.Pixels, (top + y + 0.5f) / terrain.Pixels, climate.Grass, climate.Foliage) ?? own;
                var at = (y * n + x) * 4;
                (pixels[at], pixels[at + 1], pixels[at + 2], pixels[at + 3]) = (texel.R, texel.G, texel.B, texel.A);
            }
        return new Image(pixels, n, n);
    }

    // Lets go of the pictures no bit wears this frame, and of the materials wearing them.
    private void LetGoUnworn()
    {
        var worn = _draws.Where(d => d.Value.Count > 0).Select(d => (d.Key.Surface, d.Key.Piece, d.Key.Tint)).ToHashSet();
        foreach (var key in _draws.Where(d => d.Value.Count == 0).Select(d => d.Key).ToList()) _draws.Remove(key);
        foreach (var key in _materials.Keys.Where(k => !worn.Contains((k.Surface, k.Piece, k.Tint))).ToList()) _materials.Remove(key);
        foreach (var (key, texture) in _pictures.Where(p => !worn.Contains(p.Key)).ToList())
        {
            UnloadTexture(texture);
            _textures.Remove(texture);
            _pictures.Remove(key);
        }
    }

    // A square a block across about its middle, facing +z, which a billboard turns toward the eye.
    private static ModelMesh Square()
    {
        var mesh = new BoxMesh();
        mesh.AddFace([new(-0.5f, -0.5f, 0), new(0.5f, -0.5f, 0), new(0.5f, 0.5f, 0), new(-0.5f, 0.5f, 0)],
            [new(0, 1), new(1, 1), new(1, 0), new(0, 0)], Color.White);
        return mesh.Upload();
    }

    private void Forget()
    {
        foreach (var texture in _textures) UnloadTexture(texture);
        _textures.Clear();
        _pictures.Clear();
        _materials.Clear();
    }

    public void Dispose()
    {
        Forget();
        if (_square is { } square) UnloadMesh(square);
        _square = null;
        if (_shader is { } shader) UnloadShader(shader);
        _shader = null;
    }
}
