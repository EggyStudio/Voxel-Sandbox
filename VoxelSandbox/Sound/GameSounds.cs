using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// What the game plays, the player's and the mobs' steps on the ground they walk and the sounds of
/// the place the player stands in, through the engine's audio, the mobs' heard from where they stand.
/// </summary>
/// <remarks>
/// <para>
/// A body steps each <see cref="Places.StepLength"/> it walks on the ground or through water's top,
/// its step the ground's (<see cref="Places.StepAt"/>), a variant at random at a pitch within a
/// tenth of its own. The player's are heard in the middle at a quarter of the step's loudness, as
/// the source's steps are quiet. A mob's within sixteen blocks play where it stands through the
/// engine's spatial play, from the side it is on and fainter with its distance, silent at sixteen,
/// and fainter again by the blocks between it and the listener (<see cref="Places.Occlusion"/>).
/// </para>
/// <para>
/// Each sound of a place plays on from the start, its loudness easing over about two seconds toward
/// the level the place gives it (<see cref="Places.Levels"/>), so a sound fades in as the player
/// walks into a wood and out as it walks into a cave. The water about the player is the share of a
/// few blocks tried at random within eight each frame that are water, eased the same.
/// </para>
/// <para>
/// The room about the listener is read four times a second (<see cref="Places.Room"/>), how closed
/// it is and how far its walls stand, for a reverb the engine does not give yet, so it is said by
/// <c>voxel.sounds</c> and heard nowhere. Nor does the engine yet dull a sound behind blocks, so
/// their occlusion lowers its loudness alone.
/// </para>
/// <para>
/// The listener is the player's eye, turned as the view is, which the game sets on the engine's
/// audio each frame. The engine keeps a voice that has played out until it is stopped, and one
/// played to SDL's dummy driver, as a run with no window shown, never says it has, so each step is
/// stopped half a second after it began, past the longest step's third of a second at its lowest
/// pitch, which lets it go.
/// </para>
/// </remarks>
public sealed class GameSounds
{
    /// <summary>How far a mob's steps are heard, in blocks.</summary>
    public const float Hearing = 16;

    // The loudness of the player's own steps, of a mob's and of the places' sounds at full settings.
    private const float OwnStep = 0.25f, MobStep = 0.5f, PlaceLoudness = 0.5f;

    private readonly Sound[][] _steps;
    private readonly Sound[] _loops;
    private readonly AudioSource[] _playing;
    private readonly List<(AudioSource Voice, double Ends)> _stepsPlaying = [];
    private double _time;
    private readonly float[] _levels = new float[Enum.GetValues<AmbientSound>().Length];
    private readonly Random _random = new();
    private float _walked, _water, _roomWait;
    private Vector3? _lastFeet;

    public GameSounds()
    {
        _steps = [.. Enum.GetValues<StepSound>().Select(kind => Enumerable.Range(0, SoundBank.StepVariants).Select(v => Load(SoundBank.Step(kind, v))).ToArray())];
        _loops = [.. Enum.GetValues<AmbientSound>().Select(sound => Load(SoundBank.Loop(sound)))];
        _playing = new AudioSource[_loops.Length];
    }

    /// <summary>The loudness of every sound, of the steps and of the places' sounds, from 0 to 1, as the options set them.</summary>
    public float Master { get; set; } = 1;

    public float Steps { get; set; } = 1;

    public float Ambience { get; set; } = 1;

    /// <summary>How loud each sound of a place plays now, by <see cref="AmbientSound"/>, before the settings.</summary>
    public IReadOnlyList<float> Levels => _levels;

    /// <summary>The steps played in the last frame, where each was, what it sounded of, the player's at its feet, and the share of its loudness the blocks between let through.</summary>
    public List<(Vector3 At, StepSound Sound, bool Own, float Through)> Heard { get; } = [];

    /// <summary>The room about the listener as last read, how closed it is and how far its walls stand.</summary>
    public (float Enclosed, float Size) Room { get; private set; }

    /// <summary>The steps played since the world opened.</summary>
    public int StepsHeard { get; private set; }

    /// <summary>
    /// Goes on by so many seconds, the listener set at the eye turned by the view's yaw and pitch, the
    /// player's step played where it has walked far enough on the ground, unless its steps are not
    /// heard, as a spectator's, the mobs' that stepped in this frame within hearing, and the places'
    /// sounds eased toward their levels.
    /// </summary>
    public void Update(VoxelWorld world, Vector3 eye, float yaw, float pitch, Body player, bool stepping, bool swimming, float hour, float weather, EcsWorld ecs, bool mobsStill, float seconds)
    {
        Heard.Clear();
        if (!GetApp().World.TryGetResource<AudioServer>(out var audio)) return;
        _time += seconds;
        for (int i = _stepsPlaying.Count - 1; i >= 0; i--)
            if (_stepsPlaying[i].Ends <= _time)
            {
                _stepsPlaying[i].Voice.Stop();
                _stepsPlaying.RemoveAt(i);
            }
        audio.ListenerPosition = eye;
        audio.ListenerOrientation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0);
        if (seconds <= 0) return;

        // The player's steps, on the ground or through water's top, by how far its feet went across
        // since the last frame, none for a jump of more than two blocks, as a teleport is.
        var feet = player.Position;
        var across = _lastFeet is { } last ? new Vector2(feet.X - last.X, feet.Z - last.Z).Length() : 0;
        _lastFeet = feet;
        if (stepping && (player.OnGround || swimming) && across < 2) _walked += across;
        if (_walked >= Places.StepLength)
        {
            _walked %= Places.StepLength;
            var sound = swimming ? StepSound.Water : Places.StepAt(world, feet);
            if (Play(audio, sound, null, OwnStep * Steps)) Heard.Add((feet, sound, true, 1));
        }

        // The mobs' that stepped in this frame within hearing, at their feet.
        if (!mobsStill)
            foreach (var (_, mob, body, pose) in ecs.Query<Mob, Body, MobPose>())
            {
                if (!pose.Stepped || Vector3.DistanceSquared(body.Position, eye) > Hearing * Hearing) continue;
                var sound = Places.StepAt(world, body.Position);
                var through = Places.Occlusion(world, eye, body.Position + new Vector3(0, 0.5f, 0));
                if (Play(audio, sound, body.Position, MobStep * Steps * MathF.Min(1, 0.4f + mob.Kind.Height / 3) * through)) Heard.Add((body.Position, sound, false, through));
            }

        if ((_roomWait -= seconds) <= 0)
        {
            _roomWait = 0.25f;
            Room = Places.Room(world, eye);
        }

        // The places' sounds, each eased toward its level.
        int ex = (int)MathF.Floor(eye.X), ey = (int)MathF.Floor(eye.Y), ez = (int)MathF.Floor(eye.Z);
        var water = 0;
        for (int i = 0; i < 16; i++)
            if (Blocks.IsWater(world.GetBlock(ex + _random.Next(-8, 9), ey + _random.Next(-4, 3), ez + _random.Next(-8, 9)))) water++;
        var ease = 1 - MathF.Exp(-seconds / 2);
        _water += (MathF.Min(1, water / 6f) - _water) * ease;
        var inCave = eye.Y < Overworld.SeaLevel && world.GetLight(ex, ey, ez).Sky <= 3;
        var targets = Places.Levels(new Places.Place(world.BiomeAt(ex, ez), hour, inCave, eye.Y - Overworld.SeaLevel, _water, weather));
        for (int i = 0; i < _levels.Length; i++)
        {
            _levels[i] += (targets[i] - _levels[i]) * ease;
            var loudness = _levels[i] * PlaceLoudness * Ambience * Master;
            if (!_playing[i].IsValid || !_playing[i].IsPlaying)
            {
                if (loudness <= 0.001f) continue;
                _playing[i] = audio.Play(_loops[i], AudioVoiceParams.Default with { Looping = true, Volume = loudness });
            }
            else _playing[i].SetVolume(loudness);
        }
        SetProfileValue("sound.steps", StepsHeard);
    }

    // Plays a step of a kind, a variant at random and a pitch within a tenth, where it is heard from
    // or in the middle, and says whether it sounded, which a step set silent does not.
    private bool Play(AudioServer audio, StepSound sound, Vector3? at, float loudness)
    {
        if (loudness * Master <= 0) return false;
        StepsHeard++;
        var variants = _steps[(int)sound];
        var step = audio.Play(variants[_random.Next(variants.Length)], AudioVoiceParams.Default with
        {
            Position = at,
            Volume = loudness * Master,
            PlaybackRate = 0.9f + 0.2f * (float)_random.NextDouble(),
            FarDistance = at is null ? 0 : Hearing,
        });
        if (step.IsValid) _stepsPlaying.Add((step, _time + 0.5));
        return true;
    }

    /// <summary>Stops the places' sounds, as a world closes.</summary>
    public void Stop()
    {
        foreach (var (step, _) in _stepsPlaying) step.Stop();
        _stepsPlaying.Clear();
        for (int i = 0; i < _playing.Length; i++)
        {
            if (_playing[i].IsValid) _playing[i].Stop();
            _playing[i] = AudioSource.Invalid;
            _levels[i] = 0;
        }
        _lastFeet = null;
    }

    private static Sound Load(float[] samples) => LoadSoundFromWave(new Wave { Samples = samples, SampleRate = SoundBank.Rate, Channels = 1 });
}
