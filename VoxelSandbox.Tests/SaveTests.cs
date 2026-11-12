using System.IO.Compression;
using Xunit;

namespace VoxelSandbox.Tests;

public sealed class SaveTests : IDisposable
{
    // A folder of this test's own, removed with it, so no run reads another's save.
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "VoxelSandbox.Tests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, recursive: true);
    }

    private static WorldInfo Info(string[] blocks) => new("overworld", 1, 0, 0, 0, 0, 0, false, 10, blocks);

    [Fact]
    public void A_Kept_Column_Reads_Back_With_Its_Blocks_In_Another_Save_Of_The_Folder()
    {
        var column = new ChunkColumn(-3, 40);
        column.Set(1, 2, 3, BlockId.Glowstone);
        column.Set(15, 127, 0, BlockId.Water);
        column.Changed = true;
        var save = new WorldSave(_folder);
        save.Keep(column);
        save.Flush();
        save.WriteInfo(Info([.. Blocks.All.Select(b => b.Key)]));

        var read = new WorldSave(_folder);
        read.ReadInfo();
        read.ReadRegion(-3, 40);
        var bytes = read.Kept(-3, 40);
        Assert.NotNull(bytes);
        var back = WorldSave.Decode(-3, 40, bytes, read.Renumber);

        Assert.Equal(BlockId.Glowstone, back.Get(1, 2, 3));
        Assert.Equal(BlockId.Water, back.Get(15, 127, 0));
        Assert.Equal(BlockId.Air, back.Get(0, 0, 0));
        Assert.False(column.Changed);
        Assert.Null(read.Kept(-3, 41));
    }

    [Fact]
    public void A_Save_Whose_Blocks_Were_Numbered_Otherwise_Is_Read_By_Their_Keys()
    {
        // A save that numbered stone 1 and grass 3, the other way round from this build.
        var keys = Blocks.All.Select(b => b.Key).ToArray();
        (keys[1], keys[3]) = (keys[3], keys[1]);
        var column = new ChunkColumn(0, 0);
        column.Set(0, 0, 0, (BlockId)1);
        column.Changed = true;
        var save = new WorldSave(_folder);
        save.Keep(column);
        save.Flush();
        save.WriteInfo(Info(keys));

        var read = new WorldSave(_folder);
        read.ReadInfo();
        read.ReadRegion(0, 0);
        var back = WorldSave.Decode(0, 0, read.Kept(0, 0)!, read.Renumber);

        Assert.Equal(BlockId.Stone, back.Get(0, 0, 0));
    }

    [Fact]
    public void The_Players_Place_A_Million_Blocks_Out_Reads_Back_Whole()
    {
        var save = new WorldSave(_folder);
        save.WriteInfo(Info([.. Blocks.All.Select(b => b.Key)]) with { X = 1_139_992.37, Y = 64.125, Z = -284_700.06 });

        var info = new WorldSave(_folder).ReadInfo();

        Assert.Equal((1_139_992.37, 64.125, -284_700.06), (info?.X, info?.Y, info?.Z));
    }

    [Fact]
    public void A_Mob_A_Million_Blocks_Out_Reads_Back_To_Its_Place_In_The_Land()
    {
        var sheep = new MobRecord(MobKinds.Sheep, 3, new LandPlace(1_139_992.37, 64.125, 596.91), 1, 8, null);
        var villager = new MobRecord(MobKinds.Villager, 2, new LandPlace(-284_700.06, 70, 33.5), -2, 13, new LandPlace(-284_698.5, 70, 35.25));
        new WorldSave(_folder).WriteMobs([((71_249, 37), [sheep]), ((-17_794, 2), [villager])]);

        var (kept, _) = new WorldSave(_folder).ReadMobs();

        Assert.Equal(sheep, Assert.Single(kept[(71_249, 37)]));
        Assert.Equal(villager, Assert.Single(kept[(-17_794, 2)]));
    }

    [Fact]
    public void A_Mobs_File_Of_The_First_Version_Reads_With_Its_Places_In_Floats()
    {
        Directory.CreateDirectory(_folder);
        using (var writer = new BinaryWriter(new ZLibStream(File.Create(Path.Combine(_folder, "mobs.bin")), CompressionLevel.Fastest)))
        {
            writer.Write("E3DM"u8);
            writer.Write(1);
            writer.Write(1);
            writer.Write(MobKinds.Villager.Name);
            writer.Write(1);
            writer.Write(-2);
            writer.Write(2);
            writer.Write((ushort)1);
            writer.Write((byte)0);
            writer.Write((byte)2);
            foreach (var value in (float[])[-20.5f, 64, 40.25f, 1.5f]) writer.Write(value);
            writer.Write((byte)13);
            writer.Write(true);
            foreach (var value in (float[])[-18.5f, 64, 42.5f]) writer.Write(value);
        }

        var (kept, placed) = new WorldSave(_folder).ReadMobs();

        Assert.Equal([(-2, 2)], placed);
        Assert.Equal(new MobRecord(MobKinds.Villager, 2, new LandPlace(-20.5, 64, 40.25), 1.5f, 13, new LandPlace(-18.5, 64, 42.5)), Assert.Single(kept[(-2, 2)]));
    }

    [Fact]
    public void A_Mob_Saved_With_A_Place_That_Is_No_Number_Stands_At_Its_Home_Or_Is_Left_Out()
    {
        // As the mended fault left them, kept by the column x 0 that no number turns into, a
        // villager and a cow with their x lost and a sheep with its z lost too, beside a pig.
        var villager = new MobRecord(MobKinds.Villager, 1, new LandPlace(double.NaN, 58, -150.5), 0.5f, 20, new LandPlace(-155.5, 58, 85.5));
        var cow = new MobRecord(MobKinds.Cow, 0, new LandPlace(double.NaN, 60, -480.25), 1, 10, null);
        var sheep = new MobRecord(MobKinds.Sheep, 0, new LandPlace(double.NaN, 57, double.NaN), 0, 8, null);
        var pig = new MobRecord(MobKinds.Pig, 0, new LandPlace(4.5, 60, -479.5), 2, 10, null);
        new WorldSave(_folder).WriteMobs([((0, -10), [villager]), ((0, -31), [cow, pig, sheep])]);

        var (kept, placed) = new WorldSave(_folder).ReadMobs();

        Assert.Equal(pig, Assert.Single(kept[(0, -31)]));
        Assert.Equal(villager with { Position = new LandPlace(-155.5, 58, 85.5) }, Assert.Single(kept[(-10, 5)]));
        Assert.Equal([(-10, 5), (0, -31)], kept.Keys.Order());
        Assert.Equal([(0, -31), (0, -10)], placed.Order());
    }
}
