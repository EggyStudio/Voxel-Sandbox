using System.IO.Compression;

namespace VoxelSandbox;

/// <summary>A cube of 16 blocks a side, the unit the world is meshed and drawn in, with each block's light levels.</summary>
/// <remarks>
/// <para>
/// A section of air open to the sky, as most of the world over the ground is, shares one pair of
/// arrays with every other until a block or a level in it changes, so it costs nothing of its own,
/// and a section out of the eye's reach may be packed, its blocks and light compressed, until
/// something reads it again. Every change goes through <see cref="Set"/> and
/// <see cref="SetLevel"/>, which give a section arrays of its own first, so the shared ones are
/// never written.
/// </para>
/// <para>
/// A section is read and written on one thread at a time, its column's worker before the column
/// joins the world and the main thread after.
/// </para>
/// </remarks>
public sealed class Section
{
    public const int Size = 16;
    public const int Shift = 4;
    public const int Mask = Size - 1;
    public const int Volume = Size * Size * Size;

    private static readonly ushort[] NoBlocks = new ushort[Volume];
    private static readonly byte[] OpenSky = Enumerable.Repeat((byte)(Lighting.Max << 4), Volume).ToArray();

    private ushort[]? _blocks = new ushort[Volume];
    private byte[]? _light = new byte[Volume];
    // The blocks and then the light, compressed, while the section is packed.
    private byte[]? _packed;

    public ushort[] Blocks => _blocks ?? Unpack()._blocks!;

    /// <summary>Each block's light, the sky's level from 0 to 15 in the high four bits and the emissive blocks' in the low four, as Minecraft keeps them.</summary>
    public byte[] Light => _light ?? Unpack()._light!;

    /// <summary>Whether it is air open to the sky throughout, sharing its arrays.</summary>
    public bool Open => _blocks == NoBlocks && _light == OpenSky;

    /// <summary>Whether its blocks and light are packed.</summary>
    public bool Packed => _packed is not null;

    /// <summary>Whether, packed, every level in it is 0, so no light crosses into it or out of it.</summary>
    public bool Dark { get; private set; }

    /// <summary>The bytes it holds of its own, its arrays or its packed bytes, and none while it shares.</summary>
    public int Bytes => _packed?.Length ?? (Open ? 0 : Volume * sizeof(ushort) + Volume);

    /// <summary>How many of its blocks are not air, so an empty section is skipped by the mesher.</summary>
    public int Filled { get; private set; }

    /// <summary>A count of its block changes, which the faces its open cells join are worked out again by.</summary>
    public int Version { get; private set; }

    /// <summary>
    /// Which pairs of its faces its open cells join, the <see cref="Version"/> they were worked out at
    /// and the <see cref="TerrainAtlas.Serial"/> of the atlas they were worked out under, whose packs
    /// cut some blocks out, which <see cref="Visibility"/> keeps.
    /// </summary>
    internal (ulong Joins, int Version, int Marks) Visibility = (0, -1, -1);

    public static int Index(int x, int y, int z) => (y << 8) | (z << 4) | x;

    public BlockId Get(int x, int y, int z) => (BlockId)Blocks[Index(x, y, z)];

    public void Set(int x, int y, int z, BlockId block)
    {
        var blocks = Blocks;
        ref var slot = ref blocks[Index(x, y, z)];
        if (slot == (ushort)block) return;
        if (blocks == NoBlocks)
        {
            _blocks = new ushort[Volume];
            slot = ref _blocks[Index(x, y, z)];
        }
        Filled += (slot == 0 ? 1 : 0) - (block == BlockId.Air ? 1 : 0);
        slot = (ushort)block;
        Version++;
    }

    public int SkyLight(int index) => Light[index] >> 4;

    public int BlockLight(int index) => Light[index] & 15;

    /// <summary>One channel's level at an index, the sky's or the blocks'.</summary>
    public int Level(int index, bool sky) => sky ? Light[index] >> 4 : Light[index] & 15;

    public void SetLevel(int index, bool sky, int level)
    {
        var light = Light;
        var now = (byte)(sky ? (light[index] & 0x0F) | (level << 4) : (light[index] & 0xF0) | level);
        if (light[index] == now) return;
        if (light == OpenSky) _light = light = (byte[])OpenSky.Clone();
        light[index] = now;
    }

    /// <summary>Shares the arrays of air open to the sky, where it holds no block and every level is the open sky's, and says whether it does.</summary>
    public bool Share()
    {
        if (Open) return true;
        if (Packed || Filled > 0 || !Light.AsSpan().SequenceEqual(OpenSky)) return false;
        (_blocks, _light) = (NoBlocks, OpenSky);
        return true;
    }

    /// <summary>Compresses its blocks and light, letting its arrays go until something reads it again, unless it shares them or is packed.</summary>
    public void Pack()
    {
        if (Packed || Open) return;
        var raw = new byte[Volume * sizeof(ushort) + Volume];
        System.Runtime.InteropServices.MemoryMarshal.AsBytes(_blocks.AsSpan()).CopyTo(raw);
        _light.AsSpan().CopyTo(raw.AsSpan(Volume * sizeof(ushort)));
        var packed = new byte[BrotliEncoder.GetMaxCompressedLength(raw.Length)];
        if (!BrotliEncoder.TryCompress(raw, packed, out var written, 1, 16)) return;
        Dark = !_light.AsSpan().ContainsAnyExcept((byte)0);
        (_packed, _blocks, _light) = (packed[..written], null, null);
    }

    // Takes the packed blocks and light back into arrays of its own.
    private Section Unpack()
    {
        var raw = new byte[Volume * sizeof(ushort) + Volume];
        if (!BrotliDecoder.TryDecompress(_packed, raw, out var read) || read != raw.Length)
            throw new InvalidOperationException("A packed section did not unpack whole.");
        var blocks = new ushort[Volume];
        raw.AsSpan(0, Volume * sizeof(ushort)).CopyTo(System.Runtime.InteropServices.MemoryMarshal.AsBytes(blocks.AsSpan()));
        (_blocks, _light, _packed) = (blocks, raw[(Volume * sizeof(ushort))..], null);
        return this;
    }
}

/// <summary>A section's place in the world, in sections along each axis.</summary>
public readonly record struct SectionKey(int X, int Y, int Z)
{
    /// <summary>The world position of its lowest corner.</summary>
    public System.Numerics.Vector3 Origin => new(X * Section.Size, Y * Section.Size, Z * Section.Size);
}
