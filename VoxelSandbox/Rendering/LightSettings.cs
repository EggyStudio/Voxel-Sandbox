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
    public float Bloom = 0.5f;
    public float Occlusion;
    public bool AutoExposure;
    // The somewhat boring curve the frame is tonemapped by takes a light of 1 to about 0.61, so the
    // light is raised by a quarter before it. Over the scripted run's views that gives the day the
    // brightness the engine's own curve gives it at an exposure of 1, the pillar under the morning
    // sun 136.6 against 136.7 in mean sRGB luminance and the noon sky 137.8 against 136.4, and a
    // room lit by a lamp at night 31.8 against 28.2, 13 percent brighter, since the curve is close
    // to a straight line in the dark, where an exposure of 1 would keep the night and darken the
    // day by 8 percent.
    public float Exposure = 1.25f;

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
        // The somewhat boring display transform keeps colors as they are and rolls the highlights
        // off without a filmic look, so the blocks' colors and the mobs' and the sky's pixel art
        // read as drawn.
        SetTonemap(Tonemap.SomewhatBoring);
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
