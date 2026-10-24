namespace VoxelSandbox;

/// <summary>
/// Which of the loaded sections are within the eye's reach, to be meshed, drawn and kept unpacked.
/// A column's sections from the one under its floor up are within reach wherever the column is, so
/// the land's surface is drawn as far as the columns go, and its deep sections only within so many
/// sections of the eye's in all three dimensions, so a far column's deep stone is neither meshed
/// nor kept whole.
/// </summary>
public static class Reach
{
    /// <summary>Whether a section of a column whose lowest section that is not deep is <paramref name="floorSection"/> lies within reach of the eye's section.</summary>
    public static bool Within(SectionKey key, int floorSection, SectionKey eye, int distance)
    {
        if (key.Y >= floorSection) return true;
        int dx = key.X - eye.X, dy = key.Y - eye.Y, dz = key.Z - eye.Z;
        return dx * dx + dy * dy + dz * dz <= distance * distance + distance;
    }

    /// <summary>The section an eye stands in.</summary>
    public static SectionKey Of(System.Numerics.Vector3 eye) =>
        new((int)MathF.Floor(eye.X) >> Section.Shift, (int)MathF.Floor(eye.Y) >> Section.Shift, (int)MathF.Floor(eye.Z) >> Section.Shift);

    /// <summary>How many sections past the render distance a deep section is kept unpacked, so the sections the mesher reads beside those it meshes are whole.</summary>
    public const int Kept = 2;
}
