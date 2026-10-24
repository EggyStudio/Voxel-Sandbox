namespace VoxelSandbox;

/// <summary>
/// The source's lush caves, under the wettest ground from 26 to 115 blocks under its top, as the
/// source's cave biome lies at depths of 0.2 to 0.9: moss over the caves' floors and roofs, grass,
/// tall grass and moss carpets on the floors, cave vines hanging from the roofs, a fifth of their
/// blocks bearing glow berries, and clear pools set into the floors in rims of clay.
/// </summary>
/// <remarks>
/// <para>
/// The source places its lush caves where its humidity is 0.7 or more, which is about the wettest
/// 7 percent of its land; here they lie where the climate's humidity is 0.75 or more, the same
/// share of this world's land. Its moss patches cover most of a lush cave's floors and roofs, and
/// here four in five of their blocks are moss. Of the floor's moss three in five bear a plant,
/// short grass, a moss carpet or tall grass in the source's shares of 50, 25 and 10, and one roof
/// block in seven has a vine, of a length the source's weights give, 1 to 3 most often, up to 7
/// often and up to 20 now and then, each block with berries one time in five.
/// </para>
/// <para>
/// A pool is the source's patch of clay with water in it, a disc of floor whose ground turns to
/// clay three or four blocks down, and whose top turns to water wherever the four blocks beside it
/// hold it in, so no water stands beside a cave's air. Each column tries three, of a radius of 2
/// or 3 blocks where the source's are 4 to 7, since a column decorates itself alone and a pool is
/// kept within it. The azaleas, the spore blossoms, the dripleaves and the azalea tree over the cave
/// with its roots are left out.
/// </para>
/// </remarks>
public static class LushCaves
{
    /// <summary>The climate's humidity from 0 to 1 at which the ground has lush caves under it.</summary>
    public const float Humidity = 0.75f;

    /// <summary>The fewest blocks under the ground's top a lush cave lies.</summary>
    public const int Shallowest = 26;

    /// <summary>The most blocks under the ground's top a lush cave lies.</summary>
    public const int Deepest = 115;

    private const int Salt = 0x1005;

    /// <summary>
    /// Lays the lush caves in a column's carved caves, under the ground's top at each place of the
    /// column, x then z, where a function gives the climate's humidity at a world place over it.
    /// </summary>
    public static void Decorate(ChunkColumn column, int seed, int[] grounds, Func<int, int, float> humidity)
    {
        var random = new Random((int)PlaceHash.Of(column.X, column.Z, seed ^ Salt));
        int x0 = column.X * Section.Size, z0 = column.Z * Section.Size;
        var lush = new bool[Section.Size * Section.Size];
        var any = false;
        for (int z = 0; z < Section.Size; z++)
            for (int x = 0; x < Section.Size; x++)
                any |= lush[z * Section.Size + x] = humidity(x0 + x, z0 + z) >= Humidity;
        if (!any) return;

        for (int z = 0; z < Section.Size; z++)
            for (int x = 0; x < Section.Size; x++)
            {
                if (!lush[z * Section.Size + x]) continue;
                var (low, high) = Band(grounds[z * Section.Size + x]);
                for (int y = high; y >= low; y--)
                {
                    if (column.Get(x, y, z) != BlockId.Air) continue;
                    if (Mossable(column.Get(x, y + 1, z)) && random.NextSingle() < 0.8f)
                    {
                        column.Set(x, y + 1, z, BlockId.MossBlock);
                        if (random.Next(7) == 0) Hang(column, random, x, y, z, low);
                    }
                    if (column.Get(x, y, z) == BlockId.Air && Mossable(column.Get(x, y - 1, z)) && random.NextSingle() < 0.8f)
                    {
                        column.Set(x, y - 1, z, BlockId.MossBlock);
                        if (random.NextSingle() < 0.6f) Grow(column, random, x, y, z, high);
                    }
                }
            }

        for (int pool = 0; pool < 3; pool++)
        {
            int x = 2 + random.Next(Section.Size - 4), z = 2 + random.Next(Section.Size - 4);
            var radius = 2 + random.Next(2);
            var (low, high) = Band(grounds[z * Section.Size + x]);
            var y = low + random.Next(Math.Max(1, high - low));
            if (!lush[z * Section.Size + x] || Floor(column, x, y, z, low) is not { } floor) continue;
            Pool(column, random, x, floor, z, radius);
        }
    }

    // The heights a lush cave may lie at under ground whose top is at a height, within the world.
    private static (int Low, int High) Band(int ground) =>
        (Math.Max(ground - Deepest, ChunkColumn.Bottom + 6), Math.Min(ground - Shallowest, ChunkColumn.Ceiling - 2));

    // The ground moss takes the place of, the stone, the deepslate and the dirt, as the source's moss
    // replaces, and not an ore, which stays bare in the wall.
    private static bool Mossable(BlockId block) => block is BlockId.Stone or BlockId.Deepslate or BlockId.Dirt or BlockId.Grass;

    // A plant on a floor's moss, short grass, a moss carpet or tall grass where there is room for it.
    private static void Grow(ChunkColumn column, Random random, int x, int y, int z, int high)
    {
        var pick = random.Next(85);
        if (pick < 50) column.Set(x, y, z, BlockId.ShortGrass);
        else if (pick < 75) column.Set(x, y, z, BlockId.MossCarpet);
        else if (y + 1 <= high && column.Get(x, y + 1, z) == BlockId.Air)
        {
            column.Set(x, y, z, BlockId.TallGrass);
            column.Set(x, y + 1, z, BlockId.TallGrassTop);
        }
    }

    // A vine from a roof down through the air under it, of a length by the source's weights, each
    // block with glow berries one time in five.
    private static void Hang(ChunkColumn column, Random random, int x, int y, int z, int low)
    {
        var weight = random.Next(15);
        var length = 1 + (weight < 2 ? random.Next(20) : weight < 5 ? random.Next(3) : random.Next(7));
        for (int i = 0; i < length && y - i >= low && column.Get(x, y - i, z) == BlockId.Air; i++)
            column.Set(x, y - i, z, random.Next(5) == 0 ? BlockId.CaveVinesLit : BlockId.CaveVines);
    }

    // The nearest floor at or under a height down to the lowest, air or a plant over ground a pool
    // takes, the height of what lies on the ground.
    private static int? Floor(ChunkColumn column, int x, int y, int z, int low)
    {
        for (; y > low; y--)
            if (Clear(column.Get(x, y, z)) && Pooled(column.Get(x, y - 1, z))) return y;
        return null;
    }

    // Whether a pool's floor may be laid where a block lies on the ground, air or a plant, which the pool clears.
    private static bool Clear(BlockId block) => block == BlockId.Air || BlockGroups.Plants.Contains(block);

    // The ground a pool turns to clay, what moss takes, the moss, and the clay, sand and gravel the
    // source's turns too, and not an ore or the bedrock.
    private static bool Pooled(BlockId block) => Mossable(block) || block is BlockId.MossBlock or BlockId.Clay or BlockId.Sand or BlockId.Gravel;

    // A pool in a disc of floor at one height about a place, its ground clay three or four blocks
    // down and its top water where the four blocks beside hold it in, the plants over it gone.
    private static void Pool(ChunkColumn column, Random random, int cx, int y, int cz, int radius)
    {
        List<(int X, int Z)> disc = [];
        for (int dz = -radius; dz <= radius; dz++)
            for (int dx = -radius; dx <= radius; dx++)
            {
                int x = cx + dx, z = cz + dz;
                if ((uint)x >= Section.Size || (uint)z >= Section.Size || dx * dx + dz * dz > radius * radius + 1) continue;
                // The disc's edge is ragged, as the source keeps each column at its edge seven times in ten.
                if (dx * dx + dz * dz > (radius - 1) * (radius - 1) && random.NextSingle() >= 0.7f) continue;
                var over = column.Get(x, y, z);
                if (!Clear(over) || !Pooled(column.Get(x, y - 1, z))) continue;
                if (over == BlockId.TallGrass) column.Set(x, y + 1, z, BlockId.Air);
                column.Set(x, y, z, BlockId.Air);
                var depth = random.NextSingle() < 0.8f ? 4 : 3;
                for (int d = 1; d <= depth && Pooled(column.Get(x, y - d, z)); d++) column.Set(x, y - d, z, BlockId.Clay);
                disc.Add((x, z));
            }
        List<(int X, int Z)> held = [];
        foreach (var (x, z) in disc)
        {
            var closed = true;
            foreach (var (dx, dz) in (ReadOnlySpan<(int, int)>)[(1, 0), (-1, 0), (0, 1), (0, -1)])
                closed &= (uint)(x + dx) < Section.Size && (uint)(z + dz) < Section.Size && Blocks.Collides(column.Get(x + dx, y - 1, z + dz));
            if (closed) held.Add((x, z));
        }
        foreach (var (x, z) in held) column.Set(x, y - 1, z, BlockId.Water);
    }
}
