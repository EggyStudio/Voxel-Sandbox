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

    /// <summary>
    /// Writes a region of version 1, the columns' blocks by the numbers they hold and no list of
    /// keys, so it is read by the keys the world's info lists.
    /// </summary>
    internal static void WriteVersion1Region(string folder, int regionX, int regionZ, params ChunkColumn[] columns)
    {
        using var writer = new BinaryWriter(File.Create(Path.Combine(folder, "regions", $"r.{regionX}.{regionZ}.bin")));
        writer.Write("E3DR"u8);
        writer.Write(1);
        writer.Write(columns.Length);
        foreach (var column in columns)
        {
            var output = new MemoryStream();
            using (var zip = new ZLibStream(output, CompressionLevel.Fastest))
                foreach (var section in column.Sections)
                    zip.Write(System.Runtime.InteropServices.MemoryMarshal.AsBytes(section.Blocks.AsSpan()));
            var bytes = output.ToArray();
            writer.Write((short)(column.X - regionX * WorldSave.RegionSize));
            writer.Write((short)(column.Z - regionZ * WorldSave.RegionSize));
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }
    }

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
        var back = WorldSave.Decode(-3, 40, bytes);

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
        new WorldSave(_folder).WriteInfo(Info(keys));
        WriteVersion1Region(_folder, 0, 0, column);

        var read = new WorldSave(_folder);
        read.ReadInfo();
        read.ReadRegion(0, 0);
        var back = WorldSave.Decode(0, 0, read.Kept(0, 0)!);

        Assert.Equal(BlockId.Stone, back.Get(0, 0, 0));
    }

    [Fact]
    public void A_Region_That_Cannot_Be_Read_Is_Kept_Beside_The_One_Written_Again()
    {
        var save = new WorldSave(_folder);
        var path = Path.Combine(_folder, "regions", "r.0.0.bin");
        byte[] unreadable = [.. "E3DR"u8, 2, 0, 0, 0, 7];
        File.WriteAllBytes(path, unreadable);
        var column = new ChunkColumn(0, 0);
        column.Set(1, 2, 3, BlockId.Glowstone);
        column.Changed = true;
        save.Keep(column);
        save.Flush();

        Assert.Equal(unreadable, File.ReadAllBytes(path + ".unread"));
        var read = new WorldSave(_folder);
        read.ReadRegion(0, 0);
        Assert.Equal(BlockId.Glowstone, WorldSave.Decode(0, 0, read.Kept(0, 0)!).Get(1, 2, 3));
    }

    [Theory]
    [InlineData("world.json")]
    [InlineData("mobs.bin")]
    [InlineData("furnaces.json")]
    [InlineData("chests.json")]
    public void A_File_Of_The_Save_That_Cannot_Be_Read_Is_Kept_Beside_The_One_Written_Again(string name)
    {
        var save = new WorldSave(_folder);
        var path = Path.Combine(_folder, name);
        File.WriteAllText(path, "no file of a save");
        switch (name)
        {
            case "world.json":
                Assert.Null(save.ReadInfo());
                save.WriteInfo(Info([.. Blocks.All.Select(b => b.Key)]));
                break;
            case "mobs.bin":
                Assert.Empty(save.ReadMobs().Kept);
                save.WriteMobs([]);
                break;
            case "furnaces.json":
                Assert.Empty(save.ReadFurnaces().All);
                save.WriteFurnaces(new Furnaces());
                break;
            default:
                Assert.Empty(save.ReadChests().All);
                save.WriteChests(new Chests());
                break;
        }

        Assert.Equal("no file of a save", File.ReadAllText(path + ".unread"));
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void An_Info_With_A_Heading_And_A_Place_That_Are_No_Number_Is_Written_And_Read_Back_As_A_New_Worlds()
    {
        var save = new WorldSave(_folder);

        save.WriteInfo(Info([.. Blocks.All.Select(b => b.Key)]) with { X = double.NaN, Heading = float.PositiveInfinity, Hour = float.NaN });
        var read = new WorldSave(_folder).ReadInfo()!;

        Assert.Equal((0, 0, 0), (read.X, read.Y, read.Z));
        Assert.Equal(0, read.Heading);
        Assert.Equal(Sky.FirstHour, read.Hour);
    }

    [Theory]
    [InlineData(2_000_000_000)]
    [InlineData(int.MaxValue)]
    public void A_Region_Whose_Column_Names_More_Bytes_Than_It_Holds_Is_Kept_Beside_With_No_Array_Made_For_Them(int length)
    {
        new WorldSave(_folder);
        var path = Path.Combine(_folder, "regions", "r.0.0.bin");
        using (var writer = new BinaryWriter(File.Create(path)))
        {
            writer.Write("E3DR"u8);
            writer.Write(1);
            writer.Write(1);
            writer.Write((short)0);
            writer.Write((short)0);
            writer.Write(length);
            writer.Write(new byte[16]);
        }

        var save = new WorldSave(_folder);
        var before = GC.GetAllocatedBytesForCurrentThread();
        save.ReadRegion(0, 0);

        Assert.InRange(GC.GetAllocatedBytesForCurrentThread() - before, 0, 1 << 20);
        Assert.Null(save.Kept(0, 0));
        Assert.True(File.Exists(path + ".unread"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void A_Mobs_File_Listing_More_Kinds_Than_A_Byte_Tells_Apart_Is_Kept_Beside_And_Its_Mobs_Placed_Again(int listed)
    {
        new WorldSave(_folder);
        var path = Path.Combine(_folder, "mobs.bin");
        using (var writer = new BinaryWriter(new ZLibStream(File.Create(path), CompressionLevel.Fastest)))
        {
            writer.Write("E3DM"u8);
            writer.Write(2);
            writer.Write(listed);
        }

        var (kept, placed) = new WorldSave(_folder).ReadMobs();

        Assert.Empty(kept);
        Assert.Empty(placed);
        Assert.True(File.Exists(path + ".unread"));
    }

    [Fact]
    public void A_Furnace_Or_A_Chest_Listed_With_No_Slots_Stands_Empty()
    {
        new WorldSave(_folder);
        File.WriteAllText(Path.Combine(_folder, "furnaces.json"), """[{"X":1,"Y":2,"Z":3,"Slots":null,"Burning":0,"BurnTotal":0,"Cooked":0}]""");
        File.WriteAllText(Path.Combine(_folder, "chests.json"), """[{"X":4,"Y":5,"Z":6,"Slots":null,"Loot":null,"Seed":0}]""");

        var save = new WorldSave(_folder);

        Assert.All(save.ReadFurnaces().All[(1, 2, 3)].Slots, slot => Assert.Null(slot));
        Assert.All(save.ReadChests().All[(4, 5, 6)].Slots, slot => Assert.Null(slot));
    }

    [Fact]
    public void A_Furnace_Burning_Or_Smelting_By_No_Number_Or_Below_None_Reads_As_None()
    {
        new WorldSave(_folder);
        File.WriteAllText(Path.Combine(_folder, "furnaces.json"), """[{"X":1,"Y":2,"Z":3,"Slots":[],"Burning":1e300,"BurnTotal":-1,"Cooked":"NaN"},{"X":4,"Y":5,"Z":6,"Slots":[],"Burning":8,"BurnTotal":5,"Cooked":-2}]""");

        var furnaces = new WorldSave(_folder).ReadFurnaces().All;

        Assert.Equal((0f, 0f, 0f), (furnaces[(1, 2, 3)].Burning, furnaces[(1, 2, 3)].BurnTotal, furnaces[(1, 2, 3)].Cooked));
        Assert.Equal((8f, 8f, 0f), (furnaces[(4, 5, 6)].Burning, furnaces[(4, 5, 6)].BurnTotal, furnaces[(4, 5, 6)].Cooked));
    }

    [Fact]
    public void A_Slot_That_Is_None_Reads_Empty_Beside_The_Stacks_Of_The_Rest()
    {
        new WorldSave(_folder);
        File.WriteAllText(Path.Combine(_folder, "chests.json"), """[{"X":7,"Y":8,"Z":9,"Slots":[null,"stick 2"],"Loot":null,"Seed":0}]""");

        var chest = new WorldSave(_folder).ReadChests().All[(7, 8, 9)];

        Assert.Null(chest.Slots[0]);
        Assert.Equal(new ItemStack(Items.Stick, 2), chest.Slots[1]);
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
        // Kept by the column x 0 that no number turns into, a villager and a cow whose x is no number
        // and a sheep whose z is no number too, beside a pig.
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
