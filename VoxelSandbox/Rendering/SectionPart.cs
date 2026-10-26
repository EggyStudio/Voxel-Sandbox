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

    /// <summary>The faces a pack cuts out whose shadow its texture cuts too, the plants' and every flat wall's.</summary>
    public const int Cutout = 3;

    /// <summary>The glass a pack cuts out, which casts no shadow.</summary>
    public const int CutoutGlass = 4;

    /// <summary>The see-through glass and ice, blended over what lies behind.</summary>
    public const int SeeThrough = 5;

    /// <summary>The water, blended over what lies behind.</summary>
    public const int Water = 6;

    /// <summary>The leaf blocks' faces, which let a share of the light through.</summary>
    public const int Foliage = 7;
}
