using System.Numerics;

namespace VoxelSandbox;

/// <summary>A place in the land's own coordinates, in blocks, as a save and a command keep it.</summary>
/// <remarks>
/// Doubles, since a float holds a block's parts only near the origin, a sixteenth of a block at some
/// 300,000 blocks out and an eighth at a million.
/// </remarks>
public readonly record struct LandPlace(double X, double Y, double Z);

/// <summary>
/// The whole place in the land that every float place of the game is kept against, so the floats it
/// reckons with and hands the engine stay near zero however far out the player goes.
/// </summary>
/// <remarks>
/// <para>
/// A float steps by 0.0005 at 4,450 blocks, 0.03 at 284,688 and 0.125 at 1,139,984, so a place kept
/// in the land's own coordinates walks slower along the far axis and picks the wrong block out
/// there, and the engine, which places every vertex and reads its shadows and its screen in floats,
/// mottles the light and cracks the land. Kept against an origin near the player, every place is
/// within a few thousand blocks of zero.
/// </para>
/// <para>
/// Blocks, columns and sections keep the land's own whole coordinates. A float place turns into a
/// block by <see cref="BlockOf"/> and its axes' floors, and a block into a place by
/// <see cref="PlaceOf(int, int, int)"/>, and a place of the land's own, as a save or a command holds
/// one, by <see cref="PlaceOf(LandPlace)"/> and <see cref="LandOf"/>. The origin lies on the ground's
/// plane, so heights need none.
/// </para>
/// <para>
/// The origin is a multiple of <see cref="Step"/> blocks, so every grid the engine lays over the
/// land, its field's cells and its probes, falls against the blocks as it does at the origin. It is
/// one for the game, since the game opens one world at a time, and 0 until a world is opened.
/// </para>
/// </remarks>
public static class LandOrigin
{
    /// <summary>The blocks the origin moves by, which every grid of the engine's divides.</summary>
    public const int Step = 1024;

    /// <summary>The origin's x in blocks.</summary>
    public static int X { get; private set; }

    /// <summary>The origin's z in blocks.</summary>
    public static int Z { get; private set; }

    /// <summary>The origin as a float offset, exact, since a multiple of 1,024 holds whole in a float.</summary>
    public static Vector3 Offset => new(X, 0, Z);

    /// <summary>Sets the origin to the multiple of <see cref="Step"/> nearest a place of the land's own.</summary>
    public static void Near(double x, double z) =>
        (X, Z) = ((int)Math.Round(x / Step) * Step, (int)Math.Round(z / Step) * Step);

    /// <summary>The block a float place lies in, in the land's own coordinates.</summary>
    public static (int X, int Y, int Z) BlockOf(Vector3 place) => (BlockX(place.X), (int)MathF.Floor(place.Y), BlockZ(place.Z));

    /// <summary>The block's x a float place's x lies in, in the land's own coordinates.</summary>
    public static int BlockX(float x) => (int)MathF.Floor(x) + X;

    /// <summary>The block's z a float place's z lies in, in the land's own coordinates.</summary>
    public static int BlockZ(float z) => (int)MathF.Floor(z) + Z;

    /// <summary>The float place of a block's lowest corner.</summary>
    public static Vector3 PlaceOf(int x, int y, int z) => new(x - X, y, z - Z);

    /// <summary>The float place of a place of the land's own.</summary>
    public static Vector3 PlaceOf(LandPlace land) => new((float)(land.X - X), (float)land.Y, (float)(land.Z - Z));

    /// <summary>The float place of a section's lowest corner.</summary>
    public static Vector3 PlaceOf(SectionKey key) => PlaceOf(key.X * Section.Size, key.Y * Section.Size, key.Z * Section.Size);

    /// <summary>A float place in the land's own coordinates.</summary>
    public static LandPlace LandOf(Vector3 place) => new(place.X + (double)X, place.Y, place.Z + (double)Z);

    /// <summary>The column a float place lies in.</summary>
    public static (int X, int Z) ColumnOf(Vector3 place) => (VoxelWorld.ColumnOf(BlockX(place.X)), VoxelWorld.ColumnOf(BlockZ(place.Z)));
}
