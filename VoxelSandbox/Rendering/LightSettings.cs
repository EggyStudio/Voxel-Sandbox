using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>The settings of global illumination and of the frame's post-processing, which the settings window changes.</summary>
public sealed class LightSettings
{
    /// <summary>The quality of global illumination, the highest by default, since the game is made to show it.</summary>
    public GlobalIllumination Quality = GlobalIllumination.High;

    // The screen's probes are left out, so every face reads the cascades' light alone, smoother and
    // coarser, since theirs moved and spotted the light as the eye moved, and a glow light's
    // non-number their history kept spread black across the frame, both of the engine's.
    public bool ScreenProbes;

    // Four cascades of 0.25 cover 128 blocks across around the eye, a third of the 384 drawn at the
    // default render distance, with four cells to a block, so the middle of a tunnel one block wide
    // is two cells clear of its walls.
    public int Cascades = 4;
    public float CellSize = 0.25f;
    // Two cascades built again a frame, so the four an edit touches are built within two frames of
    // its meshes settling.
    public int Budget = 2;

    // The sun's shadows reach 64 blocks, which keeps a village's trees' shadows in a view across it
    // and takes a millisecond off the frame at 1920 by 1080 against 96, where 48 lost the trees' shadows
    // fifty blocks off.
    public float ShadowDistance = 64;
    // A tenth of the light past 1 spread around it, a faint glow about a flame or a lamp, where more
    // spreads a halo far wider than the flame's own block.
    public float Bloom = 0.1f;
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

    public void ApplyAll()
    {
        ApplyField();
        SetGlobalIllumination(Quality);
        SetGlobalIlluminationScreenProbes(ScreenProbes);
        SetBloom(Bloom);
        SetAmbientOcclusion(Occlusion);
        ApplyExposure();
        SetShadowDistance(ShadowDistance);
        // The sun's contact shadows are left out where the shadow map holds a point, since their
        // blur lays a soft fringe beside the map's edges, which step along the blocks' pixels, and
        // kept past the map, where the land has no other shadow of the sun.
        SetContactShadows(ContactShadows.PastShadowMap);
        // The modpack's shader's curve with the settings of the owner's modpack, its exposure 0.9,
        // white curve 1.3, lower curve 1.05 and upper 1.0.
        SetTonemapCurve(0.9f, 1.3f, 1.05f, 1f);
        SetColorGrading(Contrast, Saturation, Color.White);
    }

    // The engine's far cascade is left off. Reaching the camera's far plane its cells are 74 blocks
    // wide, which hold the ground about the eye as a slab and took a quarter of the sky's blue from a
    // wall beside a pane of glass, cost the scene pass about 5 ms at the cherry grove at 1920 by 1080,
    // and left the light deep in a cave as it was.
    public void ApplyField() => SetSceneField(Cascades, CellSize, Budget);

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
