using System.Numerics;
using Engine;
using Xunit;
using static Engine.E3D;

namespace VoxelSandbox.Tests;

/// <summary>The cracks over a block being broken and the bits it throws.</summary>
public sealed class BreakingTests : IDisposable
{
    private readonly string _folder = Directory.CreateTempSubdirectory("voxel-breaking-").FullName;

    public void Dispose() => Directory.Delete(_folder, true);

    // The ground's top at 4, the rest open sky.
    private static VoxelWorld Ground() => Laid.World(c => Laid.Fill(c, 0, 3, BlockId.Stone));

    [Fact]
    public void The_Cracks_Go_Through_Ten_Stages_And_The_Games_Own_Grow_As_The_Block_Breaks()
    {
        Assert.Equal(0, Cracks.Stage(0));
        Assert.Equal(5, Cracks.Stage(0.55f));
        Assert.Equal(9, Cracks.Stage(0.99f));
        Assert.Equal(9, Cracks.Stage(1));
        for (int stage = 1; stage < Cracks.Stages; stage++)
            Assert.Superset(Cracks.OwnPixels(stage - 1).ToHashSet(), Cracks.OwnPixels(stage).ToHashSet());
        Assert.True(Cracks.OwnPixels(9).Count() > 4 * Cracks.OwnPixels(0).Count());
    }

    [Fact]
    public void A_Pack_Gives_The_Stages_It_Has_And_The_Game_Draws_The_Rest()
    {
        var red = new Image([.. Enumerable.Range(0, 256).SelectMany(_ => new byte[] { 255, 0, 0, 255 })], 16, 16);
        var path = Path.Combine(_folder, "assets", "minecraft", "textures", "block", "destroy_stage_5.png");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, ExportImageToMemory(red, ".png"));
        using var packs = new ResourcePacks([_folder]);
        Assert.Equal(red.Data, Cracks.Picture(packs, 5).Data);
        var own = Cracks.Picture(packs, 4);
        Assert.Equal(Cracks.OwnPixels(4).Count(), Enumerable.Range(0, 256).Count(i => own.Data[i * 4 + 3] > 0));
    }

    [Fact]
    public void A_Break_Throws_Its_Bits_From_Inside_The_Block_And_They_Fall_Rest_And_Are_Gone()
    {
        var world = Ground();
        var bits = new BlockBits();
        bits.Break(BlockId.Stone, 2, 4, 2);
        Assert.Equal(BlockBits.BreakCount, bits.All.Count);
        Assert.All(bits.All, b => Assert.InRange(b.Body.Position.Y, 4, 5));
        var starts = bits.All.ToDictionary(b => b, b => b.Body.Position.Y);

        // A frame at a time, as the game steps them, for three quarters of a second, by which every
        // bit has fallen below where it started, the fastest thrown up at 5.6 blocks a second.
        for (int i = 0; i < 45; i++) bits.Update(world, 1 / 60f);
        Assert.All(bits.All, b => Assert.True(b.Body.Position.Y < starts[b]));
        // Those still alive at a second have come down onto the ground, none through it.
        for (int i = 0; i < 15; i++) bits.Update(world, 1 / 60f);
        Assert.NotEmpty(bits.All);
        Assert.All(bits.All, b => Assert.True(b.Body.Position.Y >= 4 - 1e-3f && b.Body.OnGround));
        for (int i = 0; i < 61; i++) bits.Update(world, 1 / 60f);
        Assert.Empty(bits.All);
    }

    [Fact]
    public void A_Block_Being_Broken_Throws_A_Smaller_Bit_A_Tick_From_The_Face_Hit()
    {
        var bits = new BlockBits();
        var hit = new BlockHit(2, 4, 2, 0, 1, 0, 1);
        // A frame of a fifth of a second holds four ticks.
        bits.Hit(BlockId.Stone, hit, 0.2f);
        Assert.Equal(4, bits.All.Count);
        Assert.All(bits.All, b =>
        {
            Assert.InRange(b.Body.Position.Y, 5.09f, 5.11f);
            Assert.InRange(b.Body.Position.X, 2.1f, 2.9f);
            Assert.True(b.Size <= 0.2f * 0.6f);
        });
    }
}
