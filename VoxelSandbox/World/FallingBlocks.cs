using System.Numerics;

namespace VoxelSandbox;

/// <summary>
/// The blocks falling as the source's falling blocks do, each a block's body falling until it lands,
/// where it stands as its block again, or over a block that does not give way, as a torch or a slab
/// in its cell, breaks into its item.
/// </summary>
/// <remarks>
/// A falling block falls faster by 0.04 of a block a tick each tick and keeps 0.98 of its speed a
/// tick, Minecraft's numbers, so it falls about 40 blocks a second at most. One that falls out of the
/// world or for a minute is let go.
/// </remarks>
public sealed class FallingBlocks
{
    private const float Tick = 0.05f, Fall = 0.04f, Keep = 0.98f, MostSeconds = 60;

    /// <summary>A block falling, which block it is, its body and how long it has fallen.</summary>
    public sealed class Falling(BlockId block, Vector3 at)
    {
        public BlockId Block { get; } = block;
        public Body Body { get; } = new(0.49f, 0.98f) { Position = at };
        public float Age { get; set; }
    }

    private readonly List<Falling> _all = [];

    /// <summary>The blocks falling.</summary>
    public IReadOnlyList<Falling> All => _all;

    /// <summary>The blocks that landed where they could not stand, where and which, for the game to drop as items and clear.</summary>
    public List<(int X, int Y, int Z, BlockId Block)> Broken { get; } = [];

    /// <summary>Lets fall the blocks the world loosened, each from its cell.</summary>
    public void Take(VoxelWorld world)
    {
        foreach (var (x, y, z, block) in world.Loosened) _all.Add(new Falling(block, new Vector3(x + 0.5f, y, z + 0.5f)));
        world.Loosened.Clear();
    }

    /// <summary>Moves the falling blocks on by so many seconds, and sets down or breaks those that land.</summary>
    public void Update(VoxelWorld world, float seconds)
    {
        var keep = MathF.Pow(Keep, seconds / Tick);
        // The lowest moves first, so one falling over another finds it standing once it has landed,
        // as a column of sand stacks again where it lands.
        _all.Sort((a, b) => a.Body.Position.Y.CompareTo(b.Body.Position.Y));
        var landed = new List<Falling>();
        foreach (var falling in _all)
        {
            var body = falling.Body;
            falling.Age += seconds;
            body.Velocity.Y -= Fall / (Tick * Tick) * seconds;
            body.Velocity *= keep;
            body.Move(world, body.Velocity * seconds, keepToEdges: false);
            if (body.OnGround)
            {
                int x = (int)MathF.Floor(body.Position.X), y = (int)MathF.Floor(body.Position.Y + 0.01f), z = (int)MathF.Floor(body.Position.Z);
                if (!BlockGroups.GivesWay(world.GetBlock(x, y, z)) || !world.SetBlock(x, y, z, falling.Block)) Broken.Add((x, y, z, falling.Block));
                landed.Add(falling);
            }
            else if (falling.Age > MostSeconds || body.Position.Y < ChunkColumn.Bottom - 16) landed.Add(falling);
        }
        _all.RemoveAll(landed.Contains);
    }

    /// <summary>Lets a block fall from its cell, as a felled tree's log does.</summary>
    public void Drop(BlockId block, int x, int y, int z) => _all.Add(new Falling(block, new System.Numerics.Vector3(x + 0.5f, y, z + 0.5f)));

    /// <summary>Lets go of every falling block, as a world closes.</summary>
    public void Clear() => _all.Clear();
}
