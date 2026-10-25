using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>What TNT's blast breaks, as the source's does.</summary>
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
}
