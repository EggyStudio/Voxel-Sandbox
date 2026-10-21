using System.Numerics;

namespace VoxelSandbox;

/// <summary>
/// The leaves and logs the larger trees are grown of, balls, disks and lines of blocks placed
/// through a column's clip, so a tree crossing a column's edge is placed whole by each column.
/// </summary>
internal static class Grower
{
    /// <summary>A ball of leaves about a place, its edge ragged by a hash, growing only into air.</summary>
    public static void Ball(ColumnClip clip, int x, int y, int z, float radius, BlockId leaves, Random random)
    {
        var reach = (int)MathF.Ceiling(radius);
        for (int dy = -reach; dy <= reach; dy++)
            for (int dz = -reach; dz <= reach; dz++)
                for (int dx = -reach; dx <= reach; dx++)
                {
                    var distance = MathF.Sqrt(dx * dx + dy * dy * 1.4f + dz * dz);
                    if (distance <= radius - 0.5f || distance <= radius && random.Next(3) > 0) clip.Grow(x + dx, y + dy, z + dz, leaves);
                }
    }

    /// <summary>A flat disk of leaves about a place, its corners left out, and some of the cells at its edge where <paramref name="ragged"/>.</summary>
    public static void Disk(ColumnClip clip, int x, int y, int z, int radius, BlockId leaves, Random random, bool ragged = false)
    {
        for (int dz = -radius; dz <= radius; dz++)
            for (int dx = -radius; dx <= radius; dx++)
            {
                var edge = Math.Max(Math.Abs(dx), Math.Abs(dz)) == radius;
                if (edge && Math.Abs(dx) == Math.Abs(dz) && radius > 0) continue;
                if (edge && ragged && random.Next(4) == 0) continue;
                clip.Grow(x + dx, y, z + dz, leaves);
            }
    }

    /// <summary>Logs along a line between two places, each step to the nearest cell, as a branch runs.</summary>
    public static void Line(ColumnClip clip, Vector3 from, Vector3 to, BlockId log)
    {
        var steps = (int)MathF.Ceiling(Math.Max(Math.Max(MathF.Abs(to.X - from.X), MathF.Abs(to.Y - from.Y)), MathF.Abs(to.Z - from.Z)));
        for (int i = 0; i <= steps; i++)
        {
            var at = steps == 0 ? from : Vector3.Lerp(from, to, (float)i / steps);
            clip.Set((int)MathF.Round(at.X), (int)MathF.Round(at.Y), (int)MathF.Round(at.Z), log);
        }
    }

    public static Random Seeded(uint hash) => new(unchecked((int)hash));

    /// <summary>Whether a tree so tall fits under the world's ceiling with room for its crown.</summary>
    public static bool Fits(int y, int height) => y + height + 4 < ChunkColumn.Height;
}

/// <summary>
/// A jungle tree, four to eleven logs high under a ball of jungle leaves, as Minecraft's of its
/// straight trunk and blob of leaves two out.
/// </summary>
public sealed class JungleTree : IFeature
{
    public int Reach => 3;

    public bool GrowsOn(BlockId ground) => ground == BlockId.Grass;

    public void Place(ColumnClip clip, int x, int y, int z, uint hash)
    {
        var random = Grower.Seeded(hash);
        var height = 4 + random.Next(8);
        if (!Grower.Fits(y, height)) return;
        clip.Set(x, y, z, BlockId.Dirt);
        for (int i = 1; i <= height; i++) clip.Set(x, y + i, z, BlockId.JungleLog);
        Crown(clip, x, y + height, z, 2, random);
    }

    /// <summary>A crown of three layers, as wide as given at the trunk's top and below it and one narrower over it, as the source's blob of leaves.</summary>
    internal static void Crown(ColumnClip clip, int x, int y, int z, int radius, Random random)
    {
        Grower.Disk(clip, x, y - 1, z, radius, BlockId.JungleLeaves, random, ragged: true);
        Grower.Disk(clip, x, y, z, radius, BlockId.JungleLeaves, random, ragged: true);
        Grower.Disk(clip, x, y + 1, z, radius - 1, BlockId.JungleLeaves, random);
    }
}

/// <summary>
/// A giant jungle tree of two logs by two, ten to thirty high, as Minecraft's mega jungle tree, with
/// branches out from its upper half each under a small clump of leaves, and a wide crown on top, each
/// a few layers thick as the source's, so the light reaches the floor.
/// </summary>
public sealed class MegaJungleTree : IFeature
{
    public int Reach => 7;

    public bool GrowsOn(BlockId ground) => ground == BlockId.Grass;

    public void Place(ColumnClip clip, int x, int y, int z, uint hash)
    {
        var random = Grower.Seeded(hash);
        var height = 10 + random.Next(3) + random.Next(18);
        if (!Grower.Fits(y, height + 3)) return;
        for (int dz = 0; dz < 2; dz++)
            for (int dx = 0; dx < 2; dx++)
            {
                clip.Set(x + dx, y, z + dz, BlockId.Dirt);
                for (int i = 1; i <= height; i++) clip.Set(x + dx, y + i, z + dz, BlockId.JungleLog);
            }
        // Branches every few logs up the upper half, each out a few blocks and up one, under a clump.
        for (int up = height / 2; up < height - 2; up += 2 + random.Next(3))
        {
            var angle = random.NextSingle() * MathF.Tau;
            var reach = 2 + random.Next(2);
            var from = new Vector3(x + 0.5f, y + up, z + 0.5f);
            var to = from + new Vector3(MathF.Cos(angle) * reach, 1, MathF.Sin(angle) * reach);
            Grower.Line(clip, from, to, BlockId.JungleLog);
            Grower.Disk(clip, (int)MathF.Round(to.X), (int)to.Y + 1, (int)MathF.Round(to.Z), 2, BlockId.JungleLeaves, random, ragged: true);
            Grower.Disk(clip, (int)MathF.Round(to.X), (int)to.Y + 2, (int)MathF.Round(to.Z), 1, BlockId.JungleLeaves, random);
        }
        // A wide crown, thin as the source's, so the light reaches the floor through it.
        JungleTree.Crown(clip, x, y + height, z, 4, random);
        Grower.Disk(clip, x + 1, y + height + 1, z + 1, 3, BlockId.JungleLeaves, random, ragged: true);
    }
}

/// <summary>A jungle's bush, one log on the ground under a mound of oak leaves, as Minecraft's jungle bush.</summary>
public sealed class JungleBush : IFeature
{
    public int Reach => 2;

    public bool GrowsOn(BlockId ground) => ground == BlockId.Grass;

    public void Place(ColumnClip clip, int x, int y, int z, uint hash)
    {
        var random = Grower.Seeded(hash);
        clip.Set(x, y + 1, z, BlockId.JungleLog);
        Grower.Disk(clip, x, y + 1, z, 2, BlockId.OakLeaves, random, ragged: true);
        Grower.Disk(clip, x, y + 2, z, 1, BlockId.OakLeaves, random);
        clip.Grow(x, y + 3, z, BlockId.OakLeaves);
    }
}

/// <summary>
/// An acacia, its trunk five to nine logs high bending out to one side for its upper part, under a
/// flat crown three out with a narrower layer on it, and now and then a second branch with a crown
/// of its own, as Minecraft's forking acacia.
/// </summary>
public sealed class AcaciaTree : IFeature
{
    public int Reach => 7;

    public bool GrowsOn(BlockId ground) => ground == BlockId.Grass;

    public void Place(ColumnClip clip, int x, int y, int z, uint hash)
    {
        var random = Grower.Seeded(hash);
        var height = 5 + random.Next(3) + random.Next(3);
        if (!Grower.Fits(y, height)) return;
        clip.Set(x, y, z, BlockId.Dirt);
        int[] ways = [0, 1, 2, 3];
        var way = ways[random.Next(4)];
        (int Dx, int Dz) step = way switch { 0 => (1, 0), 1 => (-1, 0), 2 => (0, 1), _ => (0, -1) };
        var bendAt = height - random.Next(4) - 1;
        int cx = x, cz = z;
        for (int i = 1; i <= height; i++)
        {
            if (i >= bendAt && i < height) (cx, cz) = (cx + step.Dx, cz + step.Dz);
            clip.Set(cx, y + i, cz, BlockId.AcaciaLog);
        }
        Crown(clip, cx, y + height, cz, random);
        // A second branch the other way from below the bend, now and then.
        if (random.Next(2) == 0)
        {
            int bx = x, bz = z;
            var top = bendAt - 1 + random.Next(2) + 2;
            for (int i = bendAt - 1; i <= top; i++)
            {
                (bx, bz) = (bx - step.Dx, bz - step.Dz);
                clip.Set(bx, y + i, bz, BlockId.AcaciaLog);
            }
            Crown(clip, bx, y + top, bz, random);
        }
    }

    // A flat crown three out and a narrower layer two out on it, as the source's acacia foliage.
    private static void Crown(ColumnClip clip, int x, int y, int z, Random random)
    {
        Grower.Disk(clip, x, y, z, 3, BlockId.AcaciaLeaves, random, ragged: true);
        Grower.Disk(clip, x, y + 1, z, 2, BlockId.AcaciaLeaves, random);
    }
}

/// <summary>
/// A dark oak of two logs by two, six to nine high, its trunk bending a block or so near the top,
/// under a wide low crown of dark oak leaves, as Minecraft's dark oak.
/// </summary>
public sealed class DarkOakTree : IFeature
{
    public int Reach => 6;

    public bool GrowsOn(BlockId ground) => ground == BlockId.Grass;

    public void Place(ColumnClip clip, int x, int y, int z, uint hash)
    {
        var random = Grower.Seeded(hash);
        var height = 6 + random.Next(3) + random.Next(2);
        if (!Grower.Fits(y, height)) return;
        var (sx, sz) = (random.Next(3) - 1, random.Next(3) - 1);
        var bendAt = height - 2 - random.Next(2);
        for (int dz = 0; dz < 2; dz++)
            for (int dx = 0; dx < 2; dx++)
            {
                clip.Set(x + dx, y, z + dz, BlockId.Dirt);
                for (int i = 1; i <= height; i++)
                {
                    var bent = i >= bendAt ? 1 : 0;
                    clip.Set(x + dx + sx * bent, y + i, z + dz + sz * bent, BlockId.DarkOakLog);
                }
            }
        int tx = x + sx, tz = z + sz, top = y + height;
        // A wide layer at the trunk's top and one under it, a narrower one over it, and a cap.
        for (int layer = -1; layer <= 1; layer++)
        {
            var radius = layer == 1 ? 2 : 3 + (layer == 0 ? 1 : 0);
            for (int dz = -radius; dz <= radius + 1; dz++)
                for (int dx = -radius; dx <= radius + 1; dx++)
                {
                    var corner = (dx == -radius || dx == radius + 1) && (dz == -radius || dz == radius + 1);
                    if (corner || (dx == -radius || dx == radius + 1 || dz == -radius || dz == radius + 1) && random.Next(3) == 0) continue;
                    clip.Grow(tx + dx, top + layer, tz + dz, BlockId.DarkOakLeaves);
                }
        }
        Grower.Disk(clip, tx, top + 2, tz, 1, BlockId.DarkOakLeaves, random);
    }
}

/// <summary>
/// A big oak, its trunk six to fourteen logs high with branches leaning out and up from its upper
/// part, each ending in a ball of leaves, and a ball on top, as Minecraft's fancy oak.
/// </summary>
public sealed class BigOakTree : IFeature
{
    public int Reach => 6;

    public bool GrowsOn(BlockId ground) => ground is BlockId.Grass or BlockId.Snow;

    public void Place(ColumnClip clip, int x, int y, int z, uint hash)
    {
        var random = Grower.Seeded(hash);
        var height = 6 + random.Next(9);
        if (!Grower.Fits(y, height)) return;
        clip.Set(x, y, z, BlockId.Dirt);
        for (int i = 1; i <= height; i++) clip.Set(x, y + i, z, BlockId.OakLog);
        var branches = 2 + height / 4;
        for (int b = 0; b < branches; b++)
        {
            var up = height / 2 + random.Next(Math.Max(1, height / 2));
            var angle = random.NextSingle() * MathF.Tau;
            var reach = 2 + random.Next(3);
            var from = new Vector3(x, y + up, z);
            var to = from + new Vector3(MathF.Cos(angle) * reach, 1 + random.Next(2), MathF.Sin(angle) * reach);
            Grower.Line(clip, from, to, BlockId.OakLog);
            Grower.Ball(clip, (int)MathF.Round(to.X), (int)to.Y + 1, (int)MathF.Round(to.Z), 2.4f, BlockId.OakLeaves, random);
        }
        Grower.Ball(clip, x, y + height, z, 2.8f, BlockId.OakLeaves, random);
    }
}

/// <summary>
/// A giant mushroom, a pale stem four to seven high under a cap, the brown one's flat and wide, the
/// red one's a dome hanging down its sides, as Minecraft's huge mushrooms.
/// </summary>
public sealed class GiantMushroom(bool red) : IFeature
{
    public int Reach => 3;

    public bool GrowsOn(BlockId ground) => ground == BlockId.Grass;

    public void Place(ColumnClip clip, int x, int y, int z, uint hash)
    {
        var random = Grower.Seeded(hash);
        var height = 4 + random.Next(4);
        if (!Grower.Fits(y, height)) return;
        clip.Set(x, y, z, BlockId.Dirt);
        for (int i = 1; i <= height; i++) clip.Set(x, y + i, z, BlockId.MushroomStem);
        if (!red)
        {
            Grower.Disk(clip, x, y + height + 1, z, 3, BlockId.BrownMushroomBlock, random);
            return;
        }
        // A cap of two out on top and its sides hanging three blocks down, open under it.
        Grower.Disk(clip, x, y + height + 1, z, 1, BlockId.RedMushroomBlock, random);
        for (int dy = -2; dy <= 0; dy++)
            for (int dz = -2; dz <= 2; dz++)
                for (int dx = -2; dx <= 2; dx++)
                {
                    var side = Math.Max(Math.Abs(dx), Math.Abs(dz)) == 2 && !(Math.Abs(dx) == 2 && Math.Abs(dz) == 2);
                    if (side) clip.Grow(x + dx, y + height + dy, z + dz, BlockId.RedMushroomBlock);
                }
    }
}

/// <summary>
/// A cherry tree, its trunk four to six logs high forking into two branches that lean out to
/// either side, each under a wide crown of pink leaves with leaves hanging from its edge, and now
/// and then a lantern hung under a crown, as the owner's modpack's cherry groves show them.
/// </summary>
public sealed class CherryTree : IFeature
{
    public int Reach => 8;

    public bool GrowsOn(BlockId ground) => ground == BlockId.Grass;

    public void Place(ColumnClip clip, int x, int y, int z, uint hash)
    {
        var random = Grower.Seeded(hash);
        var height = 4 + random.Next(3);
        if (!Grower.Fits(y, height + 4)) return;
        clip.Set(x, y, z, BlockId.Dirt);
        for (int i = 1; i <= height; i++) clip.Set(x, y + i, z, BlockId.CherryLog);
        var angle = random.NextSingle() * MathF.Tau;
        for (int branch = 0; branch < 2; branch++)
        {
            var way = angle + branch * MathF.PI + (random.NextSingle() - 0.5f) * 0.8f;
            var reach = 2 + random.Next(3);
            var from = new Vector3(x, y + height, z);
            var to = from + new Vector3(MathF.Cos(way) * reach, 2 + random.Next(2), MathF.Sin(way) * reach);
            Grower.Line(clip, from, to, BlockId.CherryLog);
            int cx = (int)MathF.Round(to.X), cy = (int)to.Y, cz = (int)MathF.Round(to.Z);
            Crown(clip, cx, cy, cz, random);
            // A lantern under one crown in three, hung from the leaves at its underside.
            if (random.Next(3) == 0)
            {
                var (lx, lz) = (cx + random.Next(5) - 2, cz + random.Next(5) - 2);
                if (clip.Get(lx, cy, lz) == BlockId.CherryLeaves && clip.Get(lx, cy - 1, lz) == BlockId.Air) clip.Set(lx, cy - 1, lz, BlockId.HangingLantern);
            }
        }
    }

    // A wide, low crown, a layer four out at the branch's end, a narrower one over it and a cap,
    // with leaves hanging a block or two from the wide layer's edge.
    private static void Crown(ColumnClip clip, int x, int y, int z, Random random)
    {
        Grower.Disk(clip, x, y, z, 4, BlockId.CherryLeaves, random, ragged: true);
        Grower.Disk(clip, x, y + 1, z, 3, BlockId.CherryLeaves, random, ragged: true);
        Grower.Disk(clip, x, y + 2, z, 2, BlockId.CherryLeaves, random);
        // The numbers are drawn for every cell of the edge whatever the column holds, so each column
        // the crown crosses draws the same ones and the crown is the same across its edge.
        for (int dz = -4; dz <= 4; dz++)
            for (int dx = -4; dx <= 4; dx++)
            {
                if (Math.Max(Math.Abs(dx), Math.Abs(dz)) < 3) continue;
                bool hangs = random.Next(6) == 0, longer = random.Next(3) == 0;
                if (!hangs || clip.Get(x + dx, y, z + dz) != BlockId.CherryLeaves) continue;
                clip.Grow(x + dx, y - 1, z + dz, BlockId.CherryLeaves);
                if (longer) clip.Grow(x + dx, y - 2, z + dz, BlockId.CherryLeaves);
            }
    }
}
