using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The steps chosen by the ground, the sounds of a place chosen by where the listener stands, and the sounds the game makes.</summary>
public sealed class SoundTests
{
    [Fact]
    public void A_Step_Sounds_Of_The_Ground_It_Falls_On()
    {
        Assert.Equal(StepSound.Grass, Places.StepOf(BlockId.Grass));
        Assert.Equal(StepSound.Grass, Places.StepOf(BlockId.Dirt));
        Assert.Equal(StepSound.Grass, Places.StepOf(BlockId.OakLeaves));
        Assert.Equal(StepSound.Sand, Places.StepOf(BlockId.Sand));
        Assert.Equal(StepSound.Sand, Places.StepOf(BlockId.RedSand));
        Assert.Equal(StepSound.Gravel, Places.StepOf(BlockId.Gravel));
        Assert.Equal(StepSound.Snow, Places.StepOf(BlockId.Snow));
        Assert.Equal(StepSound.Snow, Places.StepOf(BlockId.WhiteWool));
        Assert.Equal(StepSound.Wood, Places.StepOf(BlockId.OakPlanks));
        Assert.Equal(StepSound.Wood, Places.StepOf(BlockId.SpruceLog));
        Assert.Equal(StepSound.Stone, Places.StepOf(BlockId.Stone));
        Assert.Equal(StepSound.Stone, Places.StepOf(BlockId.Cobblestone));
        Assert.Equal(StepSound.Water, Places.StepOf(BlockId.Water));

        // Standing on planks, on a layer of snow over them, and in water over them.
        var world = Laid.World(c => Laid.Fill(c, 0, 10, BlockId.OakPlanks));
        Assert.Equal(StepSound.Wood, Places.StepAt(world, new Vector3(4.5f, 11, 4.5f)));
        world.SetBlock(4, 11, 4, BlockId.SnowLayer);
        Assert.Equal(StepSound.Snow, Places.StepAt(world, new Vector3(4.5f, 11.125f, 4.5f)));
        world.SetBlock(6, 11, 6, BlockId.Water);
        Assert.Equal(StepSound.Water, Places.StepAt(world, new Vector3(6.5f, 11, 6.5f)));
    }

    [Fact]
    public void A_Place_Sounds_Of_Its_Woods_Its_Nights_Its_Heights_Its_Caves_And_Its_Water()
    {
        float Level(Places.Place place, AmbientSound sound) => Places.Levels(place)[(int)sound];
        var forestByDay = new Places.Place(Biomes.Forest, 12, InCave: false, Height: 10, Water: 0, Weather: 0);
        Assert.Equal(1, Level(forestByDay, AmbientSound.Birds));
        Assert.Equal(0, Level(forestByDay, AmbientSound.Crickets));
        Assert.Equal(0, Level(forestByDay, AmbientSound.Wind));
        // By night the birds are still and the crickets chirp, but not where it is dry or cold.
        Assert.Equal(0, Level(forestByDay with { Hour = 23 }, AmbientSound.Birds));
        Assert.Equal(0.8f, Level(forestByDay with { Hour = 23 }, AmbientSound.Crickets));
        Assert.Equal(0, Level(forestByDay with { Hour = 23, Biome = Biomes.Desert }, AmbientSound.Crickets));
        Assert.Equal(0, Level(forestByDay with { Hour = 23, Biome = Biomes.SnowyPlains }, AmbientSound.Crickets));
        // The weather hushes the birds to a fifth.
        Assert.Equal(0.2f, Level(forestByDay with { Weather = 1 }, AmbientSound.Birds), 3);
        // The wind from 32 over the sea, loudest at 80.
        Assert.Equal(0.5f, Level(forestByDay with { Height = 56 }, AmbientSound.Wind), 3);
        Assert.Equal(1, Level(forestByDay with { Height = 120 }, AmbientSound.Wind));
        // In a cave the cave alone and the water about it.
        var cave = Places.Levels(forestByDay with { InCave = true, Water = 0.4f });
        Assert.Equal([0, 0, 0, 1, 0.4f], cave);
    }

    [Fact]
    public void Each_Sound_Is_Heard_And_Each_Loop_Runs_On_Into_Its_Start()
    {
        foreach (var kind in Enum.GetValues<StepSound>())
            for (int variant = 0; variant < SoundBank.StepVariants; variant++)
            {
                var step = SoundBank.Step(kind, variant);
                Assert.InRange(step.Length, SoundBank.Rate / 10, SoundBank.Rate / 3);
                Assert.Equal(0.8f, step.Max(MathF.Abs), 3);
                Assert.All(step, s => Assert.True(float.IsFinite(s)));
            }
        // Two variants of a step differ.
        Assert.NotEqual(SoundBank.Step(StepSound.Gravel, 0), SoundBank.Step(StepSound.Gravel, 1));
        foreach (var sound in Enum.GetValues<AmbientSound>())
        {
            var loop = SoundBank.Loop(sound);
            Assert.Equal((int)(SoundBank.LoopSeconds * SoundBank.Rate), loop.Length);
            Assert.Equal(0.6f, loop.Max(MathF.Abs), 3);
            Assert.True(MathF.Abs(loop[^1] - loop[0]) < 0.1f, $"{sound} steps from {loop[^1]} to {loop[0]} where it starts again");
        }
    }
}
