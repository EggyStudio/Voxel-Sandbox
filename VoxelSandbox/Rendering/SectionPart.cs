namespace VoxelSandbox;

/// <summary>
/// The game's component on each part of a section the engine draws as an entity, naming the section,
/// its column and its height, and which of its parts it is.
/// </summary>
public readonly record struct SectionPart(SectionKey Section, int Kind)
{
    /// <summary>The section's solid faces.</summary>
    public const int Solid = 0;

    /// <summary>The faces a pack cuts out that cast the shadow of the whole face, a door's window and leaves.</summary>
    public const int SolidCutout = 1;

    /// <summary>The section's lava.</summary>
    public const int Molten = 2;
}
