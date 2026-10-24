namespace VoxelSandbox;

/// <summary>What falls from the sky on a biome while the weather turns.</summary>
public enum Precipitation
{
    None,
    Rain,
    Snow,
}

/// <summary>
/// The world's weather, one sky over every biome as the source's is, turning at the same hours
/// everywhere and falling as each biome's precipitation, rain on the wet, snow on the cold and
/// nothing on the dry, the sky and the light dimmed while it lasts.
/// </summary>
/// <remarks>
/// <para>
/// Each day turns at a chance of two in five, at an hour of its own and for three to twelve hours,
/// reckoned from the world's seed and the day alone, so the weather is the same each time a world's
/// days come round and a save that keeps the hour keeps the weather too. A turn begun late runs on
/// past midnight, and it comes and goes over half an hour.
/// </para>
/// <para>
/// A command or the setting may hold the weather clear or turned, which the save keeps.
/// </para>
/// </remarks>
public sealed class Weather
{
    /// <summary>The chance a day turns, and the least and most hours a turn lasts.</summary>
    public const float Chance = 0.4f, LeastHours = 3, MostHours = 12;

    /// <summary>The hours a turn takes to come and to go.</summary>
    public const float FadeHours = 0.5f;

    /// <summary>The world's seed, which its days' turns are reckoned from.</summary>
    public int Seed { get; set; }

    /// <summary>Whether the weather is held, turned where true and clear where false, or follows its days where none.</summary>
    public bool? Held { get; set; }

    /// <summary>Whether the weather turns at all, as the setting has it.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>The turn a day holds, the hour it begins and how many hours it lasts, or none for a clear day.</summary>
    public (float Start, float Hours)? TurnOn(int day)
    {
        var random = new Random(unchecked(Seed * 73856093 ^ day * 19349663 ^ 0x5EA7));
        if (random.NextDouble() >= Chance) return null;
        return ((float)random.NextDouble() * 24, LeastHours + (float)random.NextDouble() * (MostHours - LeastHours));
    }

    /// <summary>How far the weather has turned at an hour of a day, from 0 clear to 1, coming and going over half an hour.</summary>
    public float Share(int day, float hour)
    {
        if (!Enabled) return 0;
        if (Held is { } held) return held ? 1 : 0;
        var share = 0f;
        // A turn of the day before may run past midnight.
        for (int d = day - 1; d <= day; d++)
        {
            if (TurnOn(d) is not var (start, hours)) continue;
            var since = (day - d) * 24 + hour - start;
            if (since < 0 || since > hours) continue;
            share = MathF.Max(share, MathF.Min(1, MathF.Min(since, hours - since) / FadeHours));
        }
        return share;
    }
}
