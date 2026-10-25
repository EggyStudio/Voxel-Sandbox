using System.Numerics;
using Engine;
using Xunit;
using static Engine.E3D;

namespace VoxelSandbox.Tests;

/// <summary>The steps the game plays as its frame moves the mobs, heard through the engine's spatial play.</summary>
[Collection(nameof(HeadlessGames))]
public sealed class SoundFrameTests
{
    [Fact]
    public void A_Mob_Running_On_The_Players_Left_Is_Heard_From_The_Left_And_None_Past_Hearing()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        game.Mobs.Clear(null);
        // The player faces north, so the west is on its left, where a pig runs from a hit from the east.
        game.Player.Yaw = 0;
        game.Player.Pitch = 0;
        var feet = game.Player.Body.Position;
        var pig = game.Mobs.Add(new MobRecord(MobKinds.Pig, 0, feet + new Vector3(-5, 0, 0), 0, MobKinds.Pig.Health, null));
        var far = game.Mobs.Add(new MobRecord(MobKinds.Pig, 0, feet + new Vector3(-28, 0, 0), 0, MobKinds.Pig.Health, null));
        pig.Hit(feet, 0);
        far.Hit(feet, 0);
        var audio = GetApp().World.Resource<AudioServer>();
        var spatial = Assert.IsAssignableFrom<ISpatialAudioProcessor>(audio.Spatial);
        var heard = new List<(Vector3 At, float Pan)>();
        for (int frame = 0; frame < 45; frame++)
        {
            headless.Step(1);
            var context = new SpatialContext
            {
                ListenerForward = Vector3.Transform(-Vector3.UnitZ, audio.ListenerOrientation),
                ListenerUp = Vector3.Transform(Vector3.UnitY, audio.ListenerOrientation),
                SourceForward = -Vector3.UnitZ,
                SourceUp = Vector3.UnitY,
                DipolePower = 1,
                NearDistance = audio.NearDistance,
                FarDistance = GameSounds.Hearing,
            };
            foreach (var (at, _, own) in game.Sounds.Heard)
                if (!own) heard.Add((at, spatial.Compute(0, at, audio.ListenerPosition, context).Pan));
        }
        // The near pig's steps are heard, each from the left while it runs west of the player, and
        // the far one's, 28 blocks off, not at all.
        Assert.NotEmpty(heard);
        Assert.All(heard, h => Assert.True(h.At.X < feet.X - 1, $"a step heard at {h.At}, past the near pig's side"));
        Assert.All(heard, h => Assert.True(h.Pan < -0.3f, $"a step at {h.At} pans {h.Pan}"));
    }
}
