using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>The settings of global illumination and of the frame's post-processing, which the Render and Post Processing screens change.</summary>
public sealed class LightSettings
{
    /// <summary>
    /// The quality of global illumination, the highest by default, since the game is made to show it,
    /// the engine tracing the screen's probes at Medium and High.
    /// </summary>
    /// <remarks>
    /// The screen's probes are kept at Medium and High, though they take a share of the GPU's time,
    /// since they bring the frame's light nearer the light a path tracer finds in the same view.
    /// </remarks>
    public GlobalIllumination Quality = GlobalIllumination.High;

    // Four cascades of 0.25 cover 128 blocks across around the eye, a third of the 384 drawn at the
    // default render distance, with four cells to a block, so the middle of a tunnel one block wide
    // is two cells clear of its walls.
    public int Cascades = 4;
    public float CellSize = 0.25f;
    // Two cascades built again a frame, so the four an edit touches are built within two frames of
    // its meshes settling.
    public int Budget = 2;
    // A far cascade past the four reaching as far as the land drawn, the render distance's blocks,
    // so a ray leaving the counted cascades meets the hills and the rock it holds before the sky, its
    // cells as wide as that reach takes at 64 a side, six blocks at 12 columns. It is on, so a deep
    // cave's far walls take the rock's dark rather than the sky's light, and it reaches all the land
    // drawn, since a shorter reach costs the frame the same. Render Settings turns it off.
    public bool FarCascade = true;
    public float FarReach = 12 * Section.Size;

    // The sun's shadows reach 128 blocks, which keeps an oak's shadow dark fifty to a hundred blocks
    // off across a village, where 64 left it the contact shadows' faint one.
    public float ShadowDistance = 128;
    // The sun looks 1.5 degrees across, three times as wide as from the earth, which softens a tree's
    // long shadow in the afternoon toward its far end and keeps it sharp at the trunk, where 0.53 left
    // the shadows' edges stepping along the pixels of the blocks they cross and 3 blurred a crown's
    // shadow into a smear.
    public float ShadowSoftness = 1.5f;
    // A tenth of the light past 1 spread around it, a faint glow about a flame or a lamp, where more
    // spreads a halo far wider than the flame's own block.
    public float Bloom = 0.1f;
    // The light bloom spreads from, past white, so a lamp's face and a flame bloom, which shine many
    // times past it, and the land in the sun all but never does.
    public float BloomThreshold = 1;
    public float Occlusion;
    public bool AutoExposure;
    // The light is raised by 1.6 before the curve, which keeps a village's roofs and wheat in the
    // afternoon sun bright, the middle levels near Reinhard's curve, the darks deeper by the curve's
    // lower power and the lights brighter.
    public float Exposure = 1.6f;
    // The frame graded vividly, each channel pushed from the middle gray by 1.12 and the color by 1.4,
    // so the sun's warm light and the deep shade stay apart through the curve.
    public float Contrast = 1.12f, Saturation = 1.4f;
    // The tonemap curve, its exposure 0.9, white curve 1.3, lower curve 1.05 and upper 1.0, which
    // brings the sun's light under white with the darks a little deeper than Reinhard's.
    public float CurveExposure = 0.9f, CurveWhite = 1.3f, CurveLower = 1.05f, CurveUpper = 1f;

    public void ApplyAll()
    {
        ApplyField();
        SetGlobalIllumination(Quality);
        SetBloom(Bloom, BloomThreshold);
        SetAmbientOcclusion(Occlusion);
        ApplyExposure();
        SetShadowDistance(ShadowDistance);
        SetShadowSoftness(ShadowSoftness);
        // The sun's contact shadows are left out where the shadow map holds a point, since their
        // blur lays a soft fringe beside the map's edges, which step along the blocks' pixels, and
        // kept past the map, where the land has no other shadow of the sun.
        SetContactShadows(ContactShadows.PastShadowMap);
        // The engine's fill, a drawing of the land's depth past the shadow map's reach, is left off,
        // since the contact shadows shade that land already and the fill draws every section past the
        // map once more on the CPU and the GPU for no change to the frame.
        SetShadowFill(false);
        ApplyCurve();
        SetColorGrading(Contrast, Saturation, Color.White);
    }

    // The scene's field takes the four cascades and, where Far Cascade is on, a far one reaching the
    // render distance's blocks, its cells six blocks wide at 12 columns, rather than the camera's far
    // plane, whose cells 74 blocks wide hold the ground about the eye as a slab and take the sky's
    // blue from a wall beside a pane of glass.
    public void ApplyField()
    {
        SetSceneField(Cascades, CellSize, Budget, farCascade: FarCascade ? FarReach : 0);
        SetSceneFieldAhead(FieldAhead ? 0.5f : 0);
    }

    // The field lies ahead of the camera along its view by half the finest cascade's width, 64 cells
    // of 0.25, so that cascade holds the eight blocks before the eye rather than half of it lying
    // behind the player, for the same cost to global illumination.
    public bool FieldAhead = true;

    /// <summary>Brings the frame's light under 1 by the curve of the four numbers, which chooses that curve.</summary>
    public void ApplyCurve() => SetTonemapCurve(CurveExposure, CurveWhite, CurveLower, CurveUpper);

    public void ApplyExposure()
    {
        SetAutoExposure(AutoExposure);
        if (!AutoExposure) SetExposure(Exposure);
    }

    /// <summary>Steps to the next quality, past High back to Off.</summary>
    public void NextQuality()
    {
        Quality = (GlobalIllumination)(((int)Quality + 1) % 4);
        SetGlobalIllumination(Quality);
    }
}
