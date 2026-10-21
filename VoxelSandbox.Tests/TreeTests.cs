using Xunit;

namespace VoxelSandbox.Tests;

public class TreeTests
{
    public static TheoryData<string> Trees => ["jungle", "mega jungle", "jungle bush", "acacia", "dark oak", "big oak", "brown mushroom", "red mushroom", "cherry"];

    private static IFeature Make(string name) => name switch
    {
        "jungle" => new JungleTree(),
        "mega jungle" => new MegaJungleTree(),
        "jungle bush" => new JungleBush(),
        "acacia" => new AcaciaTree(),
        "dark oak" => new DarkOakTree(),
        "big oak" => new BigOakTree(),
        "brown mushroom" => new GiantMushroom(false),
        "red mushroom" => new GiantMushroom(true),
        _ => new CherryTree(),
    };

    // The tree at the middle of the nine columns around the origin's, placed by each of them through
    // its own clip, as the generator places a tree crossing columns.
    private static ChunkColumn[] Grown(IFeature tree, uint hash)
    {
        var columns = new List<ChunkColumn>();
        for (int cz = -1; cz <= 1; cz++)
            for (int cx = -1; cx <= 1; cx++)
            {
                var column = new ChunkColumn(cx, cz);
                for (int z = 0; z < 16; z++)
                    for (int x = 0; x < 16; x++)
                        for (int y = 0; y <= 40; y++) column.Set(x, y, z, y == 40 ? BlockId.Grass : BlockId.Dirt);
                tree.Place(new ColumnClip(column), 8, 40, 8, hash);
                columns.Add(column);
            }
        return [.. columns];
    }

    [Theory]
    [MemberData(nameof(Trees))]
    public void A_Tree_Stands_Within_Its_Reach_With_Its_Wood_And_Its_Crown(string name)
    {
        var tree = Make(name);
        foreach (var hash in new uint[] { 1, 77, 4096, 9_999_991 })
        {
            int logs = 0, crown = 0, farthest = 0;
            foreach (var column in Grown(tree, hash))
                for (int y = 41; y < ChunkColumn.Height; y++)
                    for (int z = 0; z < 16; z++)
                        for (int x = 0; x < 16; x++)
                        {
                            var block = column.Get(x, y, z);
                            if (block == BlockId.Air) continue;
                            if (BlockGroups.Logs.Contains(block) || block == BlockId.MushroomStem) logs++;
                            else crown++;
                            farthest = Math.Max(farthest, Math.Max(Math.Abs(column.X * 16 + x - 8), Math.Abs(column.Z * 16 + z - 8)));
                        }
            Assert.True(logs > 0 && crown > 0, $"the {name} of hash {hash} has {logs} wood and {crown} crown");
            Assert.True(farthest <= tree.Reach, $"the {name} of hash {hash} reaches {farthest}, past its reach of {tree.Reach}");
        }
    }

    [Fact]
    public void A_Cherry_Grove_Hangs_A_Lantern_Under_Some_Of_Its_Crowns()
    {
        var lanterns = Enumerable.Range(0, 30).Count(hash => Grown(new CherryTree(), (uint)hash * 7919)
            .Any(c => c.Sections.Any(s => s.Blocks.Contains((ushort)BlockId.HangingLantern))));
        Assert.InRange(lanterns, 3, 25);
    }

    [Fact]
    public void Leaves_Let_The_Sky_Through_At_A_Level_Each()
    {
        // Three layers of leaves over air in a column of its own, open to the sky above.
        var column = new ChunkColumn(0, 0);
        for (int z = 0; z < 16; z++)
            for (int x = 0; x < 16; x++)
            {
                column.Set(x, 10, z, BlockId.Stone);
                for (int y = 20; y < 23; y++) column.Set(x, y, z, BlockId.JungleLeaves);
            }
        Lighting.Compute(column);
        var under = column.Sections[15 >> Section.Shift].SkyLight(Section.Index(8, 15 & Section.Mask, 8));
        Assert.InRange(under, 6, 12);
    }
}
