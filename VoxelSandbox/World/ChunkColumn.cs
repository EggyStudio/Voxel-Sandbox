using Engine;

namespace VoxelSandbox;

/// <summary>A column of sections from the bottom of the world to its top, the unit the world is generated and loaded in.</summary>
/// <remarks>
/// <para>
/// The world stands from a height of -64 to one of 320, as the source's overworld does, so its
/// ground and sea keep their heights over the deep stone and the mountains rise well past them. A
/// section is known by the height of its lowest block over 16, from -4 to 19, the way a block's
/// height over 16 rounds down, so a place's section is found the same above the floor of 0 and
/// below it.
/// </para>
/// <para>
/// Every section is made with the column, air or not, since the light in the air above the ground
/// and in a cave is kept as the blocks are.
/// </para>
/// </remarks>
public sealed class ChunkColumn
{
    /// <summary>The height of the world's lowest blocks.</summary>
    public const int Bottom = -64;

    /// <summary>The height over the world's highest blocks, the first of the sky over it.</summary>
    public const int Ceiling = 320;

    public const int Height = Ceiling - Bottom;
    public const int SectionCount = Height / Section.Size;

    /// <summary>The lowest section's number, the height of its lowest block over 16.</summary>
    public const int BottomSection = Bottom >> Section.Shift;

    /// <summary>The number over the highest section's.</summary>
    public const int CeilingSection = Ceiling >> Section.Shift;

    /// <summary>Whether a height lies within the world, from <see cref="Bottom"/> to under <see cref="Ceiling"/>.</summary>
    public static bool Holds(int y) => (uint)(y - Bottom) < Height;

    public ChunkColumn(int x, int z)
    {
        X = x;
        Z = z;
        for (int i = 0; i < SectionCount; i++) Sections[i] = new Section();
    }

    public int X { get; }
    public int Z { get; }

    /// <summary>Whether a block of it changed since it was generated or last kept by the save.</summary>
    public bool Changed { get; set; }

    /// <summary>Its sections from the bottom.</summary>
    public readonly Section[] Sections = new Section[SectionCount];

    /// <summary>The color its grass is tinted at each place, x fastest, from the climate, which its generator paints.</summary>
    public readonly Color[] GrassTint = new Color[Section.Size * Section.Size];

    /// <summary>The color its oak leaves are tinted at each place, as <see cref="GrassTint"/> is.</summary>
    public readonly Color[] FoliageTint = new Color[Section.Size * Section.Size];

    /// <summary>The biome of each place, by its id in <see cref="Biomes.All"/>, as <see cref="GrassTint"/> is laid out.</summary>
    public readonly byte[] Biome = new byte[Section.Size * Section.Size];

    /// <summary>The mobs its generator placed in it, which the world's <see cref="Mobs"/> takes the first time it loads.</summary>
    public readonly List<MobRecord> Spawns = [];

    /// <summary>The section a height inside the world lies in.</summary>
    public Section At(int y) => Sections[(y - Bottom) >> Section.Shift];

    /// <summary>The section of a number from <see cref="BottomSection"/> to under <see cref="CeilingSection"/>, or null past them.</summary>
    public Section? Numbered(int section) => (uint)(section - BottomSection) < SectionCount ? Sections[section - BottomSection] : null;

    /// <summary>The block at a place inside the column, x and z from 0 to 15 and y from <see cref="Bottom"/> to under <see cref="Ceiling"/>.</summary>
    public BlockId Get(int x, int y, int z) => At(y).Get(x, y & Section.Mask, z);

    public void Set(int x, int y, int z, BlockId block) => At(y).Set(x, y & Section.Mask, z, block);

    /// <summary>The height of the highest block that is not air, or one under <see cref="Bottom"/> in a column of air.</summary>
    public int Top(int x, int z)
    {
        for (int y = Ceiling - 1; y >= Bottom; y--)
            if (Get(x, y, z) != BlockId.Air) return y;
        return Bottom - 1;
    }
}
