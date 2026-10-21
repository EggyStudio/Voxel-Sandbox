namespace VoxelSandbox;

/// <summary>
/// How much of the hearts, the food and the bubbles of breath shows, each fading out once it is
/// full and has not changed for a few seconds and showing again at once when it changes, as the
/// HUD of the owner's modpack hides what has nothing to say.
/// </summary>
public sealed class VitalsFade
{
    /// <summary>Seconds a full row stays before it fades.</summary>
    public const float Wait = 3;

    /// <summary>Seconds a row takes to fade out.</summary>
    public const float Out = 0.5f;

    private (int Health, int Food, float Air)? _last;
    private float _healthStill, _foodStill, _airStill;
    // Whether breath has run short since it last finished fading, so a full breath that never ran
    // short shows no bubbles.
    private bool _airWasShort;

    /// <summary>Whether full rows fade, on unless the settings turn it off.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>How much of the hearts shows, from 0 to 1.</summary>
    public float Hearts { get; private set; } = 1;

    /// <summary>How much of the food shows, from 0 to 1.</summary>
    public float Food { get; private set; } = 1;

    /// <summary>How much of the bubbles of breath shows, from 0 to 1, none while breath is full and the rows do not fade.</summary>
    public float Air { get; private set; }

    /// <summary>Follows the vitals over a frame of so many seconds.</summary>
    public void Update(Vitals vitals, float seconds)
    {
        var now = (vitals.Health, vitals.Food, vitals.Air);
        var last = _last ?? now;
        _healthStill = now.Health != last.Health ? 0 : _healthStill + seconds;
        _foodStill = now.Food != last.Food ? 0 : _foodStill + seconds;
        _airStill = now.Air != last.Air ? 0 : _airStill + seconds;
        _last = now;
        Hearts = Shown(vitals.Health >= Vitals.Most, _healthStill);
        Food = Shown(vitals.Food >= Vitals.Most, _foodStill);
        // Breath shows while it is short, and full only as it fades after filling again.
        if (vitals.Air < Vitals.Breath) _airWasShort = true;
        Air = vitals.Air < Vitals.Breath ? 1 : Enabled && _airWasShort ? Shown(true, _airStill) : 0;
        if (Air == 0) _airWasShort = false;
    }

    /// <summary>Shows every row again, as when the vitals begin to show in a mode that has them.</summary>
    public void Reset()
    {
        (_last, _healthStill, _foodStill, _airStill, _airWasShort) = (null, 0, 0, 0, false);
        (Hearts, Food, Air) = (1, 1, 0);
    }

    private float Shown(bool full, float still) => !Enabled || !full ? 1 : Math.Clamp(1 - (still - Wait) / Out, 0, 1);
}
