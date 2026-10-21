using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>The settings of global illumination and of the frame's post-processing, which the settings window changes.</summary>
public sealed class LightSettings
{
    /// <summary>The quality of global illumination, the highest by default, since the game is made to show it.</summary>
    public GlobalIllumination Quality = GlobalIllumination.High;

    // Four cascades of 0.25 cover 128 blocks across around the eye, a third of the 384 drawn at the
    // default render distance, with four cells to a block, so the middle of a tunnel one block wide
    // is two cells clear of its walls.
    public int Cascades = 4;
    public float CellSize = 0.25f;
    // Two cascades built again a frame, so the four an edit touches are built within two frames of
    // its meshes settling.
    public int Budget = 2;

    public float ShadowDistance = 96;
    // A tenth of the light past 1 spread around it, a faint glow about a flame or a lamp, where more
    // spreads a halo far wider than the flame's own block.
    public float Bloom = 0.1f;
    public float Occlusion;
    public bool AutoExposure;
    // Reinhard's curve takes a light of 1 to a half, so the light is raised by 1.7 before it, which
    // gives a village's roofs and wheat in the afternoon the brightness of the modpack's pictures.
    public float Exposure = 1.7f;
    // The frame graded as vividly as the modpack's pictures, whose shader's warm light and deep shade
    // saturate them, each channel pushed from the middle gray by 1.12 and the color by 1.4.
    public float Contrast = 1.12f, Saturation = 1.4f;

    public void ApplyAll()
    {
        ApplyField();
        SetGlobalIllumination(Quality);
        SetBloom(Bloom);
        SetAmbientOcclusion(Occlusion);
        ApplyExposure();
        SetShadowDistance(ShadowDistance);
        // The sun's contact shadows are left out where the shadow map holds a point, since their
        // blur lays a soft fringe beside the map's edges, which step along the blocks' pixels, and
        // kept past the map, where the land has no other shadow of the sun.
        SetContactShadows(ContactShadows.PastShadowMap);
        // Reinhard's curve is the nearest of the engine's to the modpack's shader's with the settings
        // of the owner's modpack, its exposure 0.9, white curve 1.3, lower curve 1.05 and upper 1.0,
        // within 0.023 of it in sRGB levels at lights from 0.05 to 4 at an exposure of 1.05, where
        // ACES is 0.055 off and a cut at 1 0.062.
        SetTonemap(Tonemap.Reinhard);
        SetColorGrading(Contrast, Saturation, Color.White);
    }

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
