using System.Numerics;

namespace VoxelSandbox;

/// <summary>
/// The TNT lit and burning down its fuse as the source's primed TNT does, each popping up from its
/// cell, falling and sliding to a stop, flashing white every five ticks and swelling over its last
/// ten, then exploding at a power of 4.
/// </summary>
/// <remarks>
/// Lit by flint and steel its fuse is four seconds, 80 ticks, and lit by another blast it is 10 to
/// 29 ticks, so a row of TNT goes off one after another. It falls faster by 0.04 of a block a tick
/// each tick and keeps 0.98 of its speed, and on the ground its speed across falls to 0.7 a tick, the
/// source's numbers.
/// </remarks>
public sealed class PrimedTnt
{
    /// <summary>The power of TNT's blast and the length of its fuse lit by flint and steel, in seconds.</summary>
    public const float Power = 4, Fuse = 4;

    private const float Tick = 0.05f, Fall = 0.04f, Keep = 0.98f, Slide = 0.7f;

    /// <summary>A TNT burning, its body and how long its fuse has left.</summary>
    public sealed class Lit(Vector3 at, float fuse)
    {
        public Body Body { get; } = new(0.49f, 0.98f) { Position = at };
        public float Left { get; set; } = fuse;

        /// <summary>Whether it shows white now, every other five ticks of its fuse, as the source's flashes.</summary>
        public bool Flashing => (int)(Left / Tick) / 5 % 2 == 0;

        /// <summary>How large it stands, swelling by up to three tenths over its last ten ticks, as the source's does.</summary>
        public float Size
        {
            get
            {
                var f = Math.Clamp(1 - Left / (10 * Tick), 0, 1);
                f *= f;
                return 1 + f * f * 0.3f;
            }
        }
    }

    private readonly List<Lit> _all = [];
    private readonly Random _random = new(1);

    /// <summary>The TNT burning.</summary>
    public IReadOnlyList<Lit> All => _all;

    /// <summary>Where TNT went off, its blasts' middles a sixteenth over its feet, for the game to set off and clear.</summary>
    public List<Vector3> Exploded { get; } = [];

    /// <summary>Lights the TNT of a cell, the cell's block taken out by the caller, with a fuse of so many seconds, popping it up as the source's does.</summary>
    public void Light(int x, int y, int z, float fuse = Fuse)
    {
        var turn = _random.NextDouble() * Math.Tau;
        var lit = new Lit(new Vector3(x + 0.5f, y, z + 0.5f), fuse);
        lit.Body.Velocity = new Vector3(-(float)Math.Sin(turn) * 0.02f, 0.2f, -(float)Math.Cos(turn) * 0.02f) / Tick;
        _all.Add(lit);
    }

    /// <summary>Lights the TNT of a cell that another blast reached, its fuse 10 to 29 ticks.</summary>
    public void Chain(int x, int y, int z) => Light(x, y, z, (10 + _random.Next(20)) * Tick);

    /// <summary>Moves the TNT on by so many seconds and burns their fuses, setting off those whose fuses have run out.</summary>
    public void Update(VoxelWorld world, float seconds)
    {
        var keep = MathF.Pow(Keep, seconds / Tick);
        var slide = MathF.Pow(Slide, seconds / Tick);
        foreach (var lit in _all)
        {
            var body = lit.Body;
            body.Velocity.Y -= Fall / (Tick * Tick) * seconds;
            body.Velocity *= keep;
            if (body.OnGround) (body.Velocity.X, body.Velocity.Z) = (body.Velocity.X * slide, body.Velocity.Z * slide);
            body.Move(world, body.Velocity * seconds, keepToEdges: false);
            if ((lit.Left -= seconds) <= 0) Exploded.Add(body.Position + new Vector3(0, 1 / 16f, 0));
        }
        _all.RemoveAll(l => l.Left <= 0);
    }

    /// <summary>Puts out every TNT, as a world closes.</summary>
    public void Clear() => _all.Clear();
}
