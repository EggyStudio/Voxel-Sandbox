using System.Numerics;

namespace VoxelSandbox;

/// <summary>
/// A building's own frame, x along its front, z from its front wall back and y from its floor,
/// turned a number of quarters clockwise so its front, north in its frame, faces a way, which a
/// structure lays a building out in and turns with it, a torch over its door, a stair and a chest
/// included.
/// </summary>
public readonly record struct StructureFrame(ColumnClip Clip, int X, int Y, int Z, int Turn)
{
    private static readonly Facing[] Round = [Facing.North, Facing.East, Facing.South, Facing.West];

    public (int X, int Z) At(int x, int z) => Turn switch
    {
        0 => (X + x, Z + z),
        1 => (X - z, Z + x),
        2 => (X - x, Z - z),
        _ => (X + z, Z - x),
    };

    public BlockId Get(int x, int y, int z)
    {
        var (wx, wz) = At(x, z);
        return Clip.Get(wx, Y + y, wz);
    }

    public void Set(int x, int y, int z, BlockId block)
    {
        var (wx, wz) = At(x, z);
        Clip.Set(wx, Y + y, wz, Turned(block));
    }

    public void Fill(int x0, int y0, int z0, int x1, int y1, int z1, BlockId block)
    {
        for (int y = y0; y <= y1; y++)
            for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++)
                    Set(x, y, z, block);
    }

    /// <summary>A way given in the frame turned with it.</summary>
    public Facing Turned(Facing facing) => Array.IndexOf(Round, facing) is >= 0 and var at ? Round[(at + Turn) % 4] : facing;

    // A block facing a side is turned with the frame, so a torch or stairs given facing north face its front.
    private BlockId Turned(BlockId block)
    {
        var info = Blocks.Get(block);
        if (info.Shape is not { Facing: not (Facing.Up or Facing.Down) } shape) return block;
        return Blocks.Variant(info.Item, Turned(shape.Facing), shape.Upper);
    }

    /// <summary>A chest at a place of the frame, its latch facing a way of the frame, holding a table's loot.</summary>
    public void Chest(int x, int y, int z, Facing facing, string loot, int seed)
    {
        var (wx, wz) = At(x, z);
        Clip.Chest(wx, Y + y, wz, Turned(facing), loot, seed);
    }

    /// <summary>A villager of a robe standing at a place of the frame, facing its front, at home at another.</summary>
    public void Villager(int x, int z, byte robe, int homeX, int homeZ)
    {
        var (wx, wz) = At(x, z);
        var (hx, hz) = At(homeX, homeZ);
        Clip.Spawn(new MobRecord(MobKinds.Villager, robe, new Vector3(wx + 0.5f, Y + 1, wz + 0.5f), -Turn * MathF.PI / 2,
            MobKinds.Villager.Health, new Vector3(hx + 0.5f, Y + 1, hz + 0.5f)));
    }

    /// <summary>
    /// A footing of a block under each place of a footprint down to the ground, so a building on a
    /// slope stands on it rather than over a gap, and the space over it cleared of the trees.
    /// </summary>
    public void Ground(int x0, int x1, int depth, BlockId footing, int height)
    {
        for (int z = 0; z < depth; z++)
            for (int x = x0; x <= x1; x++)
                for (int d = 1; d <= 6 && Get(x, -d, z) is BlockId.Air or BlockId.Water or BlockId.OakLeaves or BlockId.SpruceLeaves or BlockId.BirchLeaves; d++)
                    Set(x, -d, z, footing);
        Fill(x0 - 1, 1, -1, x1 + 1, height, depth, BlockId.Air);
    }
}
