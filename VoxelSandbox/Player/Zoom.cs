using Engine;

namespace VoxelSandbox;

/// <summary>
/// The zoom its key gives while held, as the modpack's zoom does, the view narrowed to a quarter of
/// its field over a tenth of a second and widened back as the key is let go, the wheel narrowing it
/// further while held to a tenth.
/// </summary>
/// <remarks>
/// The share of the field shown also scales the mouse's turn, so a pixel turns the view as far
/// across the narrowed picture as across the whole, and the aim holds steady. A notch of the wheel
/// away from the player narrows the view by a fifth and one toward it widens it back to a quarter,
/// and the next press starts again at a quarter.
/// </remarks>
public sealed class Zoom
{
    /// <summary>The share of the field the zoom narrows to as its key is pressed, and the least the wheel narrows it to.</summary>
    public const float First = 0.25f, Least = 0.1f;

    /// <summary>Seconds the view takes to narrow and to widen back.</summary>
    public const float Seconds = 0.1f;

    private float _target = First, _in;

    /// <summary>The key held to zoom, C unless the options set another.</summary>
    public Key Key { get; set; } = Key.C;

    /// <summary>The share of the field of view shown now, 1 unzoomed.</summary>
    public float Share { get; private set; } = 1;

    /// <summary>Whether the key was held at the last update.</summary>
    public bool Held { get; private set; }

    /// <summary>Goes on by so many seconds, the key held or not and the wheel turned by so many notches, away from the player above 0.</summary>
    public void Update(float seconds, bool held, float wheel)
    {
        Held = held;
        if (held && wheel != 0) _target = Math.Clamp(_target * MathF.Pow(0.8f, wheel), Least, First);
        _in = Math.Clamp(_in + (held ? seconds : -seconds) / Seconds, 0, 1);
        if (!held && _in == 0) _target = First;
        Share = 1 + (_target - 1) * _in;
    }
}
