using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>TNT lit and going off, as the source's does.</summary>
public sealed class TntTests
{
    [Fact]
    public void A_Tnt_Hollows_About_A_Hundred_Dirt_Twice_A_Creepers_And_Little_Stone()
    {
        // TNT's power of 4 at the ground's top, a sixteenth over its feet as the source sets it off.
        var dirt = Laid.World(c => Laid.Fill(c, 0, 10, BlockId.Dirt));
        var stone = Laid.World(c => Laid.Fill(c, 0, 10, BlockId.Stone));
        var at = new Vector3(8.5f, 11.0625f, 8.5f);
        for (int seed = 1; seed <= 3; seed++)
        {
            var inDirt = Explosion.Breaks(dirt, at, PrimedTnt.Power, new Random(seed)).Count;
            var creeper = Explosion.Breaks(dirt, at, 3, new Random(seed)).Count;
            Assert.InRange(inDirt, 80, 130);
            Assert.InRange(inDirt / (float)creeper, 1.6f, 2.4f);
            Assert.InRange(Explosion.Breaks(stone, at, PrimedTnt.Power, new Random(seed)).Count, 1, 20);
        }
    }

    [Fact]
    public void A_Lit_Tnt_Pops_Up_Lands_Flashes_And_Goes_Off_At_Four_Seconds()
    {
        var world = Laid.World(c => Laid.Fill(c, 0, 3, BlockId.Stone));
        var tnt = new PrimedTnt(new Engine.EcsWorld());
        tnt.Light(2, 4, 2);
        Assert.True(Assert.Single(tnt.All).Lit.Flashing);
        var highest = 0f;
        for (int frame = 0; frame < 4 * 60 - 1; frame++)
        {
            Assert.Empty(tnt.Update(world, 1 / 60f));
            highest = MathF.Max(highest, Assert.Single(tnt.All).Body.Position.Y);
        }
        Assert.InRange(highest, 4.3f, 5.5f);
        var (lit, body) = Assert.Single(tnt.All);
        Assert.True(body.OnGround);
        Assert.True(lit.Size > 1.25f);
        var blast = Assert.Single(tnt.Update(world, 1 / 30f));
        Assert.Empty(tnt.All);
        Assert.InRange(blast.Y, 4.06f, 4.07f);
        Assert.InRange(blast.X, 2.1f, 2.9f);

        // TNT another blast reaches burns 10 to 29 ticks.
        for (int i = 0; i < 20; i++) tnt.Chain(2, 4, 2);
        Assert.All(tnt.All, t => Assert.InRange(t.Lit.Left, 0.5f, 1.45f));
    }
}
