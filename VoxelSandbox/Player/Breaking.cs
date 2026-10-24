namespace VoxelSandbox;

/// <summary>
/// How far the block aimed at is broken in survival, kept while the button stays held and the aim
/// is lost for a moment, so a block above reach breaks over a few jumps, and begun again on another
/// block.
/// </summary>
public sealed class Breaking
{
    /// <summary>
    /// How long the aim may be lost with the button held before the progress goes, in seconds, longer
    /// than the 0.53 a jump keeps the player in the air.
    /// </summary>
    public const float KeepSeconds = 1;

    private float _lost;

    /// <summary>The block being broken, or null.</summary>
    public (int X, int Y, int Z)? At { get; private set; }

    /// <summary>How far it is broken, from 0 to 1.</summary>
    public float Progress { get; private set; }

    /// <summary>Lets go of the block being broken, as the button let go does.</summary>
    public void Stop() => (At, Progress, _lost) = (null, 0, 0);

    /// <summary>
    /// Breaks on by a frame of so many seconds at the block aimed at, or none where the aim is lost,
    /// a block taking so many seconds to break, and says whether it broke.
    /// </summary>
    public bool Step((int X, int Y, int Z)? aimed, float seconds, float breakSeconds)
    {
        if (aimed is null)
        {
            if (At is not null && (_lost += seconds) > KeepSeconds) Stop();
            return false;
        }
        _lost = 0;
        if (At != aimed) (At, Progress) = (aimed, 0);
        Progress += breakSeconds <= 0 ? 1 : seconds / breakSeconds;
        if (Progress < 1) return false;
        Stop();
        return true;
    }
}
