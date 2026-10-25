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

    [Fact]
    public void A_Sound_Behind_Blocks_Comes_Through_Halved_By_Each_And_A_Room_Reads_As_Closed_Or_Open()
    {
        // Air over a floor of stone, and walls of one block and of three across the line to a sound.
        var world = Laid.World(c => Laid.Fill(c, 0, 10, BlockId.Stone));
        var listener = new Vector3(4.5f, 12.5f, 4.5f);
        Assert.Equal(1, Places.Occlusion(world, listener, new Vector3(12.5f, 12.5f, 4.5f)));
        world.SetBlock(8, 12, 4, BlockId.Stone);
        Assert.Equal(0.5f, Places.Occlusion(world, listener, new Vector3(12.5f, 12.5f, 4.5f)));
        world.SetBlock(9, 12, 4, BlockId.Stone);
        world.SetBlock(10, 12, 4, BlockId.OakPlanks);
        Assert.Equal(0.125f, Places.Occlusion(world, listener, new Vector3(12.5f, 12.5f, 4.5f)));
        // Glass lets it through, and the cells of the two ends do not count, the stone of the first here.
        world.SetBlock(8, 12, 5, BlockId.Glass);
        Assert.Equal(1, Places.Occlusion(world, new Vector3(8.5f, 12.5f, 4.5f), new Vector3(8.5f, 12.5f, 6.5f)));

        // Over the open floor the rays down meet it and those up and across reach nothing, and in a
        // closed box of stone five blocks across every ray meets a wall within three.
        var open = Places.Room(world, new Vector3(20.5f, 12.5f, 20.5f));
        Assert.InRange(open.Enclosed, 0.3f, 0.4f);
        for (int z = 18; z <= 24; z++)
            for (int y = 11; y <= 17; y++)
                for (int x = 18; x <= 24; x++)
                    world.SetBlock(x, y, z, x is 18 or 24 || y is 11 or 17 || z is 18 or 24 ? BlockId.Stone : BlockId.Air);
        var closed = Places.Room(world, new Vector3(21.5f, 14.5f, 21.5f));
        Assert.Equal(1, closed.Enclosed);
        Assert.InRange(closed.Size, 2, 4);
    }
}
