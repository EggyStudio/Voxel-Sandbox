namespace VoxelSandbox;

/// <summary>What a step sounds of, by the block walked on, as the source's step sounds are grouped.</summary>
public enum StepSound
{
    Grass,
    Stone,
    Wood,
    Sand,
    Gravel,
    Snow,
    Water,
}

/// <summary>The sounds of a place, which play on and fade in and out by where the listener stands.</summary>
public enum AmbientSound
{
    Birds,
    Crickets,
    Wind,
    Cave,
    Water,
}

/// <summary>
/// The game's sounds, made by the game itself from noise and tones rather than recorded or taken
/// from the source or its mods, a step on each kind of ground, a few of each so steps one after
/// another are not alike, and a loop for each sound of a place.
/// </summary>
/// <remarks>
/// <para>
/// All are of one channel at 22,050 samples a second, the steps under a third of a second and the
/// loops eight seconds, each made from a Random of its own seed, so a sound is the same each run.
/// A step is shaped by its noise's filtering and its envelope, grass a soft crunch, gravel a run of
/// grains, sand a hiss, snow a muffled crunch, stone a click over a low thud, wood a hollow knock and
/// water a splash with a bubble's rise. The loops are birds' calls, crickets' chirps, the wind's
/// rise and fall, a cave's low tone with water dripping, and water lapping, each joined end to start
/// by a crossfade or made of whole cycles of the loop, so it plays on with no click where it starts
/// again.
/// </para>
/// </remarks>
public static class SoundBank
{
    /// <summary>The samples a second every sound is made at.</summary>
    public const int Rate = 22050;

    /// <summary>How many different steps each kind of ground has.</summary>
    public const int StepVariants = 4;

    /// <summary>The seconds every loop lasts.</summary>
    public const float LoopSeconds = 8;

    /// <summary>A step on a kind of ground, the variant chosen by number, peaking at 0.8.</summary>
    public static float[] Step(StepSound kind, int variant)
    {
        var random = new Random((int)kind * 131 + variant * 17 + 1);
        var seconds = kind switch { StepSound.Stone => 0.12f, StepSound.Water => 0.3f, StepSound.Sand => 0.22f, _ => 0.18f };
        var samples = new float[(int)(seconds * Rate)];
        float low = 0, lower = 0;
        // A step on loose ground lands as a run of grains, gravel's coarse and loud, snow's and grass's fine.
        var grains = kind switch { StepSound.Gravel => 22, StepSound.Snow => 12, StepSound.Grass => 9, _ => 0 };
        var grainAt = new (int At, int Length, float Loud)[grains];
        for (int g = 0; g < grains; g++)
            grainAt[g] = ((int)(random.NextDouble() * random.NextDouble() * samples.Length * 0.8f), (int)(Rate * (0.002 + random.NextDouble() * 0.006)), 0.3f + (float)random.NextDouble() * 0.7f);
        var pitch = 0.9f + (float)random.NextDouble() * 0.2f;
        for (int i = 0; i < samples.Length; i++)
        {
            var t = i / (float)Rate;
            var white = (float)random.NextDouble() * 2 - 1;
            var attack = MathF.Min(1, t / 0.004f);
            float s;
            switch (kind)
            {
                case StepSound.Grass:
                    low += (white - low) * 0.35f;
                    s = low * MathF.Exp(-t / 0.045f);
                    break;
                case StepSound.Sand:
                    low += (white - low) * 0.25f;
                    s = low * MathF.Exp(-t / 0.07f) * 0.8f;
                    break;
                case StepSound.Snow:
                    low += (white - low) * 0.12f;
                    s = low * MathF.Exp(-t / 0.05f) * 1.4f;
                    break;
                case StepSound.Gravel:
                    low += (white - low) * 0.5f;
                    s = low * MathF.Exp(-t / 0.06f) * 0.3f;
                    break;
                case StepSound.Stone:
                    // A click, the noise less its slow part, over a thud.
                    low += (white - low) * 0.2f;
                    s = (white - low) * MathF.Exp(-t / 0.012f) * 0.7f + MathF.Sin(MathF.Tau * 110 * pitch * t) * MathF.Exp(-t / 0.03f);
                    break;
                case StepSound.Wood:
                    s = (MathF.Sin(MathF.Tau * 180 * pitch * t) + 0.5f * MathF.Sin(MathF.Tau * 360 * pitch * t)) * MathF.Exp(-t / 0.045f)
                        + white * MathF.Exp(-t / 0.004f) * 0.5f;
                    break;
                default:
                    // Water, a lapping noise and a bubble rising in pitch a little way in.
                    low += (white - low) * 0.15f;
                    lower += (low - lower) * 0.1f;
                    var bubble = t > 0.04f && t < 0.11f ? MathF.Sin(MathF.Tau * (400 + 7000 * (t - 0.04f)) * (t - 0.04f)) * MathF.Sin(MathF.PI * (t - 0.04f) / 0.07f) : 0;
                    s = (low - lower) * 2.5f * MathF.Exp(-t / 0.12f) + bubble * 0.5f;
                    break;
            }
            samples[i] = s * attack;
        }
        foreach (var (at, length, loud) in grainAt)
        {
            float g = 0;
            for (int i = 0; i < length && at + i < samples.Length; i++)
            {
                g += ((float)random.NextDouble() * 2 - 1 - g) * 0.6f;
                samples[at + i] += g * loud * MathF.Sin(MathF.PI * i / length) * MathF.Exp(-(at + i) / (float)Rate / 0.1f);
            }
        }
        return Peak(samples, 0.8f);
    }

    /// <summary>The loop of a place's sound, eight seconds peaking at 0.6, its end running on into its start.</summary>
    public static float[] Loop(AmbientSound sound)
    {
        var random = new Random(1000 + (int)sound);
        var length = (int)(LoopSeconds * Rate);
        var samples = sound switch
        {
            AmbientSound.Birds => Calls(random, length),
            AmbientSound.Crickets => Chirps(random, length),
            AmbientSound.Wind => Joined(Wind(random, length + Rate), length),
            AmbientSound.Cave => Cave(random, length),
            _ => Joined(Lapping(random, length + Rate), length),
        };
        return Peak(samples, 0.6f);
    }

    // Birds' calls at times of their own, each a few notes sweeping up, none running past the loop's end.
    private static float[] Calls(Random random, int length)
    {
        var samples = new float[length];
        for (int call = 0; call < 7; call++)
        {
            var at = (int)(random.NextDouble() * (length - Rate));
            var notes = 2 + random.Next(4);
            float from = 2200 + (float)random.NextDouble() * 1500, rise = 800 + (float)random.NextDouble() * 1600;
            for (int note = 0; note < notes; note++)
            {
                var noteLength = (int)(Rate * (0.05 + random.NextDouble() * 0.07));
                var phase = 0f;
                for (int i = 0; i < noteLength; i++)
                {
                    var share = i / (float)noteLength;
                    phase += MathF.Tau * (from + rise * share) / Rate;
                    samples[at + i] += MathF.Sin(phase) * MathF.Sin(MathF.PI * share) * 0.5f;
                }
                at += noteLength + (int)(Rate * 0.03);
            }
        }
        return samples;
    }

    // Two crickets chirping, each a train of three pulses of a high tone every half second or so.
    private static float[] Chirps(Random random, int length)
    {
        var samples = new float[length];
        foreach (var (tone, every) in (ReadOnlySpan<(float, float)>)[(4400, 0.55f), (4900, 0.7f)])
            for (var at = (float)random.NextDouble() * every; at < LoopSeconds - 0.2f; at += every * (0.9f + 0.2f * (float)random.NextDouble()))
                for (int pulse = 0; pulse < 3; pulse++)
                {
                    int start = (int)((at + pulse * 0.03f) * Rate), pulseLength = (int)(0.015f * Rate);
                    for (int i = 0; i < pulseLength; i++)
                        samples[start + i] += MathF.Sin(MathF.Tau * tone * (start + i) / Rate) * MathF.Sin(MathF.PI * i / pulseLength) * 0.4f;
                }
        return samples;
    }

    // The wind, deep noise rising and falling slowly.
    private static float[] Wind(Random random, int length)
    {
        var samples = new float[length];
        float low = 0, lower = 0;
        for (int i = 0; i < length; i++)
        {
            var t = i / (float)Rate;
            low += ((float)random.NextDouble() * 2 - 1 - low) * 0.05f;
            lower += (low - lower) * 0.02f;
            var swell = 0.55f + 0.3f * MathF.Sin(MathF.Tau * t / 8) + 0.15f * MathF.Sin(MathF.Tau * t * 3 / 8 + 1);
            samples[i] = (low - lower) * swell;
        }
        return samples;
    }

    // A cave's low tone, whole cycles of two hums in the loop's eight seconds, under drips of water.
    private static float[] Cave(Random random, int length)
    {
        var samples = new float[length];
        for (int i = 0; i < length; i++)
        {
            var t = i / (float)Rate;
            samples[i] = 0.25f * MathF.Sin(MathF.Tau * 55 * t) * (0.7f + 0.3f * MathF.Sin(MathF.Tau * t / 8)) + 0.12f * MathF.Sin(MathF.Tau * 82.5f * t);
        }
        for (int drip = 0; drip < 5; drip++)
        {
            var at = (int)(random.NextDouble() * (length - Rate / 2));
            var tone = 1200 + (float)random.NextDouble() * 900;
            var phase = 0f;
            for (int i = 0; i < Rate / 4; i++)
            {
                var t = i / (float)Rate;
                phase += MathF.Tau * tone * (1 - 0.4f * MathF.Min(1, t / 0.03f)) / Rate;
                samples[at + i] += MathF.Sin(phase) * MathF.Exp(-t / 0.06f) * 0.8f;
            }
        }
        return samples;
    }

    // Water lapping, soft noise swelling about twice a second.
    private static float[] Lapping(Random random, int length)
    {
        var samples = new float[length];
        float low = 0, lower = 0;
        for (int i = 0; i < length; i++)
        {
            var t = i / (float)Rate;
            low += ((float)random.NextDouble() * 2 - 1 - low) * 0.2f;
            lower += (low - lower) * 0.05f;
            samples[i] = (low - lower) * (0.5f + 0.5f * MathF.Pow(0.5f + 0.5f * MathF.Sin(MathF.Tau * t * 0.45f + MathF.Sin(t * 1.3f)), 2));
        }
        return samples;
    }

    // A loop of so many samples from a run a second longer, its last second faded into its first so
    // the end runs on into the start.
    private static float[] Joined(float[] run, int length)
    {
        var fade = run.Length - length;
        var samples = run[..length];
        for (int i = 0; i < fade; i++)
        {
            var share = i / (float)fade;
            samples[i] = samples[i] * MathF.Sqrt(share) + run[length + i] * MathF.Sqrt(1 - share);
        }
        return samples;
    }

    private static float[] Peak(float[] samples, float peak)
    {
        var most = samples.Max(MathF.Abs);
        if (most > 0) for (int i = 0; i < samples.Length; i++) samples[i] *= peak / most;
        return samples;
    }
}
