using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// What a blast shows besides the blocks it breaks, the source's cloud of puffs about it and a flash
/// of light for a tenth of a second, which global illumination carries onto the land around.
/// </summary>
/// <remarks>
/// <para>
/// The cloud is the source's huge explosion, six puffs a tick for eight ticks, each within four
/// blocks of the blast, three blocks across and living six to nine ticks, unlit, played through the
/// pack's sixteen pictures of <c>particle/explosion</c> over its life, or the engine's soft dot
/// fading out where no pack has them. Each blast has an emitter of its own, since the engine emits
/// a burst where its emitter stands in the next frame, and two blasts of a chain in one frame would
/// otherwise share a place.
/// </para>
/// <para>
/// The flash is a point light of a warm white a block over the blast, in the cloud, so the ground under
/// it is not lit past white and bloomed to a disc, reaching twice the blast's power, that casts
/// shadows, so the cascades take it where the scene's distance field lets it through.
/// </para>
/// </remarks>
public sealed class BlastEffects : IDisposable
{
    private const int Ticks = 8, PuffsATick = 6, Frames = 16;
    private const float Reach = 4, Width = 3, Life = 0.375f, FlashSeconds = 0.1f, FlashStrength = 10;
    // How long a cloud's emitter is kept from its blast, its last puffs' lives included, before
    // another blast may take it.
    private const float Kept = Ticks / 20f + Life * 1.25f;

    private ResourcePacks? _packs;
    private Texture2D? _sheet;
    private bool _sheetRead;
    private readonly List<(ParticleEmitterHandle Emitter, float Age, int Emitted)> _clouds = [];
    private readonly List<(LightHandle Light, float Left)> _flashes = [];

    /// <summary>Draws the clouds from now on with the pictures of these packs.</summary>
    public void Use(ResourcePacks packs)
    {
        Forget();
        _packs = packs;
    }

    /// <summary>Shows a blast of a power at a place.</summary>
    public void Add(Vector3 at, float power)
    {
        var free = _clouds.FindIndex(c => c.Age >= Kept);
        var emitter = free >= 0 ? _clouds[free].Emitter : CreateParticleEmitter(at, Cloud());
        SetParticleEmitterPosition(emitter, at);
        EmitParticles(emitter, PuffsATick);
        if (free >= 0) _clouds[free] = (emitter, 0, PuffsATick);
        else _clouds.Add((emitter, 0, PuffsATick));
        _flashes.Add((CreatePointLight(at + Vector3.UnitY, new Color(255, 236, 200), FlashStrength * power, 2 * power, castsShadows: true), FlashSeconds));
    }

    /// <summary>Goes on by so many seconds, each cloud emitting the puffs of the ticks it reached and the flashes whose time is over taken out.</summary>
    public void Update(float seconds)
    {
        for (int i = 0; i < _clouds.Count; i++)
        {
            var (emitter, age, emitted) = _clouds[i];
            if (age >= Kept) continue;
            age += seconds;
            var due = Math.Min(Ticks, (int)(age * 20) + 1) * PuffsATick;
            if (due > emitted) EmitParticles(emitter, due - emitted);
            _clouds[i] = (emitter, age, due);
        }
        for (int i = _flashes.Count - 1; i >= 0; i--)
        {
            var (light, left) = _flashes[i];
            if ((left -= seconds) > 0)
            {
                _flashes[i] = (light, left);
                continue;
            }
            UnloadLight(light);
            _flashes.RemoveAt(i);
        }
    }

    // The settings of a cloud's emitter, its puffs still where they are born.
    private ParticleEmitter Cloud()
    {
        var sheet = Sheet();
        var gray = new Color(179, 179, 179, 255);
        return new ParticleEmitter
        {
            MaxParticles = Ticks * PuffsATick, Emitting = false, Life = Life, LifeVariation = 0.2f, Radius = Reach,
            StartSize = Width, EndSize = Width, StartColor = gray, EndColor = sheet is null ? gray with { A = 0 } : gray,
            Intensity = 1, Lit = false, Blend = ParticleBlend.Alpha,
            Texture = sheet ?? default, TextureColumns = sheet is null ? 0 : Frames, TextureRows = 1,
        };
    }

    // The pack's pictures of an explosion side by side, read once for the packs in use, or null
    // where a pack lacks one or they are not all of one size.
    private Texture2D? Sheet()
    {
        if (_sheetRead) return _sheet;
        _sheetRead = true;
        if (_packs is null) return null;
        var frames = new Image[Frames];
        for (int i = 0; i < Frames; i++)
            if (_packs.Texture($"particle/explosion_{i}") is not { } frame || (i > 0 && (frame.Width != frames[0].Width || frame.Height != frames[0].Height))) return null;
            else frames[i] = frame;
        int width = frames[0].Width, height = frames[0].Height;
        var data = new byte[Frames * width * height * 4];
        for (int i = 0; i < Frames; i++)
            for (int y = 0; y < height; y++)
                Array.Copy(frames[i].Data, y * width * 4, data, (y * Frames * width + i * width) * 4, width * 4);
        var sheet = LoadTextureFromImage(new Image(data, Frames * width, height));
        SetTextureFilter(sheet, TextureFilter.Point);
        return _sheet = sheet;
    }

    // Lets the clouds' emitters and their pictures go, so the next blast reads the packs again.
    private void Forget()
    {
        foreach (var (emitter, _, _) in _clouds) UnloadParticleEmitter(emitter);
        _clouds.Clear();
        if (_sheet is { } sheet) UnloadTexture(sheet);
        (_sheet, _sheetRead) = (null, false);
    }

    public void Dispose()
    {
        Forget();
        foreach (var (light, _) in _flashes) UnloadLight(light);
        _flashes.Clear();
    }
}
