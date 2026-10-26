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
    // cells as wide as that reach takes at 64 a side, six blocks at 12 columns. It is off, since it
    // darkens a deep cave's far walls at Low alone and costs 1.6 to 2 ms of the frame at 1920 by 1080
    // at Low and High alike, and Render Settings turns it on.
    public bool FarCascade;
    public float FarReach = 12 * Section.Size;

    // The sun's shadows reach 64 blocks, which keeps a village's trees' shadows in a view across it
    // and takes a millisecond off the frame at 1920 by 1080 against 96, where 48 lost the trees' shadows
    // fifty blocks off.
    public float ShadowDistance = 64;
    // The sun looks 1.5 degrees across, three times as wide as from the earth, which softens a tree's
    // long shadow in the afternoon toward its far end and keeps it sharp at the trunk, where 0.53 left
    // the shadows' edges stepping along the pixels of the blocks they cross and 3 blurred a crown's
    // shadow into a smear, for about 0.6 ms of the frame at High at the cherry grove at 1920 by 1080.
    public float ShadowSoftness = 1.5f;
    // A tenth of the light past 1 spread around it, a faint glow about a flame or a lamp, where more
    // spreads a halo far wider than the flame's own block.
    public float Bloom = 0.1f;
    // The light bloom spreads from, past white, so a lamp's face and a flame bloom, which shine many
    // times past it, and the land in the sun all but never does.
    public float BloomThreshold = 1;
    public float Occlusion;
    public bool AutoExposure;
    // The light is raised by 1.6 before the modpack's curve, which gives a village's roofs and wheat in
    // the afternoon the brightness of the modpack's pictures, the middle levels within 3 of Reinhard's
    // curve at 1.7, which matched them, the darks deeper by the curve's lower power and the lights
    // brighter.
    public float Exposure = 1.6f;
    // The frame graded as vividly as the modpack's pictures, whose shader's warm light and deep shade
    // saturate them, each channel pushed from the middle gray by 1.12 and the color by 1.4.
    public float Contrast = 1.12f, Saturation = 1.4f;
    // The modpack's shader's curve with the modpack's own settings, its exposure 0.9,
    // white curve 1.3, lower curve 1.05 and upper 1.0.
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
        ApplyCurve();
        SetColorGrading(Contrast, Saturation, Color.White);
    }

    // The scene's field takes the four cascades and, where Far Cascade is on, a far one reaching the
    // render distance's blocks, its cells six blocks wide at 12 columns, rather than the camera's far
    // plane, whose cells 74 blocks wide hold the ground about the eye as a slab and took a quarter of
    // the sky's blue from a wall beside a pane of glass.
    public void ApplyField() => SetSceneField(Cascades, CellSize, Budget, farCascade: FarCascade ? FarReach : 0);

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
