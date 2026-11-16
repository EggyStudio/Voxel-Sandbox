using System.IO.Compression;
using Xunit;
using static Engine.E3D;

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
    [InlineData("regions/m.0.0.bin")]
    [InlineData("furnaces.json")]
    [InlineData("regions/c.0.0.bin")]
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
            case "regions/m.0.0.bin":
                Assert.Null(save.TakeMobs(0, 0));
                save.WriteMobs([((0, 0), [])]);
                break;
            case "furnaces.json":
                Assert.Empty(save.ReadFurnaces().All);
                save.WriteFurnaces(new Furnaces());
                break;
            default:
                Assert.Null(save.TakeChests(0, 0));
                save.WriteChests([((0, 0), [])]);
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
        // A region's file, and the one file a world saved before kept every column's mobs in.
        foreach (var (name, version) in new[] { ("regions/m.0.0.bin", 3), ("mobs.bin", 2) })
        {
            new WorldSave(_folder);
            var path = Path.Combine(_folder, name);
            using (var writer = new BinaryWriter(new ZLibStream(File.Create(path), CompressionLevel.Fastest)))
            {
                writer.Write("E3DM"u8);
                writer.Write(version);
                writer.Write(listed);
            }

            Assert.Null(new WorldSave(_folder).TakeMobs(0, 0));
            Assert.True(File.Exists(path + ".unread"));
        }
    }

    [Theory]
    [InlineData("world.json")]
    [InlineData("regions/m.0.0.bin")]
    [InlineData("furnaces.json")]
    [InlineData("regions/c.0.0.bin")]
    [InlineData("regions/r.0.0.bin")]
    public void A_File_Of_The_Save_The_Account_Cannot_Read_Is_Kept_Beside_And_The_World_Opens(string name)
    {
        // A file's mode the account cannot read is set on Linux and macOS alone.
        if (OperatingSystem.IsWindows()) return;
        var save = new WorldSave(_folder);
        save.WriteInfo(Info([.. Blocks.All.Select(b => b.Key)]));
        save.WriteMobs([((0, 0), [])]);
        save.WriteFurnaces(new Furnaces());
        save.WriteChests([((0, 0), [])]);
        var column = new ChunkColumn(0, 0);
        column.Set(1, 2, 3, BlockId.Glowstone);
        column.Changed = true;
        save.Keep(column);
        save.Flush();
        var path = Path.Combine(_folder, name);
        File.SetUnixFileMode(path, UnixFileMode.None);

        var read = new WorldSave(_folder);
        read.ReadInfo();
        read.TakeMobs(0, 0);
        read.ReadFurnaces();
        read.TakeChests(0, 0);
        read.ReadRegion(0, 0);

        Assert.True(File.Exists(path + ".unread"));
        File.SetUnixFileMode(path + ".unread", UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }

    [Fact]
    public void A_World_Whose_Info_The_Account_Cannot_Read_Is_Left_Out_Of_The_Worlds_Listed()
    {
        // A file's mode the account cannot read is set on Linux and macOS alone.
        if (OperatingSystem.IsWindows()) return;
        var (unread, read) = (Path.Combine(_folder, "unread"), Path.Combine(_folder, "read"));
        new WorldSave(read).WriteInfo(Info([.. Blocks.All.Select(b => b.Key)]));
        new WorldSave(unread).WriteInfo(Info([.. Blocks.All.Select(b => b.Key)]));
        File.SetUnixFileMode(Path.Combine(unread, "world.json"), UnixFileMode.None);
        try
        {
            Assert.Equal(["read"], WorldSave.Saved(_folder).Select(w => w.Name));
        }
        finally
        {
            File.SetUnixFileMode(Path.Combine(unread, "world.json"), UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    [Fact]
    public void A_Furnace_Or_A_Chest_Listed_With_No_Slots_Stands_Empty()
    {
        new WorldSave(_folder);
        File.WriteAllText(Path.Combine(_folder, "furnaces.json"), """[{"X":1,"Y":2,"Z":3,"Slots":null,"Burning":0,"BurnTotal":0,"Cooked":0}]""");
        File.WriteAllText(Path.Combine(_folder, "chests.json"), """[{"X":4,"Y":5,"Z":6,"Slots":null,"Loot":null,"Seed":0}]""");

        var save = new WorldSave(_folder);
        var chests = new Chests(save);
        chests.Joined(new ChunkColumn(0, 0));

        Assert.All(save.ReadFurnaces().All[(1, 2, 3)].Slots, slot => Assert.Null(slot));
        Assert.All(chests.All[(4, 5, 6)].Slots, slot => Assert.Null(slot));
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

        var chests = new Chests(new WorldSave(_folder));
        chests.Joined(new ChunkColumn(0, 0));
        var chest = chests.All[(7, 8, 9)];

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

        var read = new WorldSave(_folder);

        Assert.Equal(sheep, Assert.Single(read.TakeMobs(71_249, 37)!));
        Assert.Equal(villager, Assert.Single(read.TakeMobs(-17_794, 2)!));
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

        var villager = new MobRecord(MobKinds.Villager, 2, new LandPlace(-20.5, 64, 40.25), 1.5f, 13, new LandPlace(-18.5, 64, 42.5));
        var read = new WorldSave(_folder);
        var taken = read.TakeMobs(-2, 2)!;
        Assert.Equal(villager, Assert.Single(taken));

        // Saved as the game saves its loaded columns, the one file's mobs stand in the regions' files
        // and the file is kept beside as read.
        read.WriteMobs([((-2, 2), taken)]);
        Assert.True(File.Exists(Path.Combine(_folder, "mobs.bin.read")));
        Assert.False(File.Exists(Path.Combine(_folder, "mobs.bin")));
        Assert.Equal(villager, Assert.Single(new WorldSave(_folder).TakeMobs(-2, 2)!));
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
        WriteOneFile(((0, -10), [villager]), ((0, -31), [cow, pig, sheep]));

        var read = new WorldSave(_folder);

        Assert.Equal(pig, Assert.Single(read.TakeMobs(0, -31)!));
        Assert.Equal(villager with { Position = new LandPlace(-155.5, 58, 85.5) }, Assert.Single(read.TakeMobs(-10, 5)!));
        Assert.Empty(read.TakeMobs(0, -10)!);
    }

    [Fact]
    public void A_Mob_Of_A_Regions_File_Whose_Place_Is_No_Number_Is_Left_Out()
    {
        var cow = new MobRecord(MobKinds.Cow, 0, new LandPlace(double.NaN, 60, -480.25), 1, 10, null);
        var pig = new MobRecord(MobKinds.Pig, 0, new LandPlace(4.5, 60, -479.5), 2, 10, null);
        new WorldSave(_folder).WriteMobs([((0, -31), [cow, pig])]);

        Assert.Equal(pig, Assert.Single(new WorldSave(_folder).TakeMobs(0, -31)!));
    }

    // Writes the one file a world saved before kept every column's mobs in, of its version 2.
    private void WriteOneFile(params ((int X, int Z) Column, MobRecord[] Mobs)[] columns)
    {
        Directory.CreateDirectory(_folder);
        using var writer = new BinaryWriter(new ZLibStream(File.Create(Path.Combine(_folder, "mobs.bin")), CompressionLevel.Fastest));
        writer.Write("E3DM"u8);
        writer.Write(2);
        writer.Write(MobKinds.All.Length);
        foreach (var kind in MobKinds.All) writer.Write(kind.Name);
        writer.Write(columns.Length);
        foreach (var ((x, z), mobs) in columns)
        {
            writer.Write(x);
            writer.Write(z);
            writer.Write((ushort)mobs.Length);
            foreach (var mob in mobs)
            {
                writer.Write(mob.Kind.Id);
                writer.Write(mob.Variant);
                foreach (var value in (double[])[mob.Position.X, mob.Position.Y, mob.Position.Z]) writer.Write(value);
                writer.Write(mob.Yaw);
                writer.Write((byte)mob.Health);
                writer.Write(mob.Home is not null);
                if (mob.Home is { } home)
                    foreach (var value in (double[])[home.X, home.Y, home.Z]) writer.Write(value);
            }
        }
    }

    // A column's bytes as the save keeps them, its sections' blocks by their numbers through zlib.
    private static byte[] Zipped(Action<Stream> write)
    {
        using var output = new MemoryStream();
        using (var zip = new ZLibStream(output, CompressionLevel.Fastest, leaveOpen: true)) write(zip);
        return output.ToArray();
    }

    // A region of version 1 of columns by the places in it the file gives, which may lie outside it.
    private void WriteRegion(int count, params (short X, short Z, byte[] Bytes)[] columns)
    {
        Directory.CreateDirectory(Path.Combine(_folder, "regions"));
        using var writer = new BinaryWriter(File.Create(Path.Combine(_folder, "regions", "r.0.0.bin")));
        writer.Write("E3DR"u8);
        writer.Write(1);
        writer.Write(count);
        foreach (var (x, z, bytes) in columns)
        {
            writer.Write(x);
            writer.Write(z);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }
    }

    [Fact]
    public void A_Kept_Block_Of_A_Number_Past_The_Games_Blocks_Reads_As_Air()
    {
        var blocks = new ushort[ChunkColumn.SectionCount * Section.Volume];
        blocks[5 * Section.Volume + 1] = 60_000;
        blocks[5 * Section.Volume + 2] = (ushort)BlockId.Stone;

        var column = WorldSave.Decode(0, 0, Zipped(zip => zip.Write(System.Runtime.InteropServices.MemoryMarshal.AsBytes(blocks.AsSpan()))), out var unknown);

        Assert.Equal((BlockId.Air, BlockId.Stone, 1), (column.Get(1, 16, 0), column.Get(2, 16, 0), unknown));
        Lighting.Compute(column);
    }

    [Fact]
    public void Blocks_Of_Numbers_Past_The_Games_Are_Said_Once_A_Region_With_Their_Count()
    {
        var path = Path.Combine(_folder, "regions", "r.0.0.bin");
        var said = new List<string>();
        SetTraceLogCallback((_, line) =>
        {
            if (line.Contains(path)) lock (said) said.Add(line);
        });
        try
        {
            var save = new WorldSave(_folder);
            save.Unknown(0, 0, 3);
            save.Unknown(5, 7, 2);
            save.Flush();
            save.Flush();

            Assert.EndsWith($"The region {path} holds 5 blocks of numbers this game does not know, which are read as air.", Assert.Single(said));
        }
        finally
        {
            SetTraceLogCallback(null);
        }
    }

    [Fact]
    public void A_Region_Of_Every_Column_The_Game_Keeps_Reads_Whole()
    {
        var save = new WorldSave(_folder);
        for (int z = 0; z < WorldSave.RegionSize; z++)
            for (int x = 0; x < WorldSave.RegionSize; x++)
            {
                var column = new ChunkColumn(x, z);
                column.Set(1, 2, 3, BlockId.Glowstone);
                column.Changed = true;
                save.Keep(column);
            }
        save.Flush();

        var read = new WorldSave(_folder);
        read.ReadRegion(0, 0);

        Assert.All(Enumerable.Range(0, WorldSave.RegionSize * WorldSave.RegionSize), i => Assert.NotNull(read.Kept(i % WorldSave.RegionSize, i / WorldSave.RegionSize)));
        Assert.False(File.Exists(Path.Combine(_folder, "regions", "r.0.0.bin.unread")));
    }

    [Fact]
    public void A_Column_Whose_Blocks_Inflate_Past_Its_Sections_Is_Read_Within_A_Bound()
    {
        // 256 MB of zeros, which zlib keeps in a quarter of a megabyte, where a column holds 192 KB.
        var zeros = new byte[1 << 20];
        var bytes = Zipped(zip => { for (int i = 0; i < 256; i++) zip.Write(zeros); });

        GC.Collect();
        var before = GC.GetAllocatedBytesForCurrentThread();
        WorldSave.Shallow(bytes);
        WorldSave.Decode(0, 0, bytes);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.InRange(allocated, 0, 64L << 20);
    }

    [Fact]
    public void A_Region_Placing_A_Column_Outside_It_Is_Kept_Beside_And_What_Came_Before_It_Stays()
    {
        var bytes = Zipped(zip => zip.Write(new byte[ChunkColumn.SectionCount * Section.Volume * sizeof(ushort)]));
        WriteRegion(2, (0, 0, bytes), (40, 0, bytes));
        var written = File.ReadAllBytes(Path.Combine(_folder, "regions", "r.0.0.bin"));

        var save = new WorldSave(_folder);
        save.ReadRegion(0, 0);

        Assert.NotNull(save.Kept(0, 0));
        Assert.Null(save.Kept(40, 0));
        // The region is kept beside under another name, whole.
        Assert.Equal(written, File.ReadAllBytes(Path.Combine(_folder, "regions", "r.0.0.bin.unread")));
    }

    [Fact]
    public void A_Region_Counting_Far_More_Columns_Than_It_Holds_Is_Kept_Beside()
    {
        WriteRegion(int.MaxValue);
        var written = File.ReadAllBytes(Path.Combine(_folder, "regions", "r.0.0.bin"));

        var save = new WorldSave(_folder);
        save.ReadRegion(0, 0);

        Assert.Null(save.Kept(0, 0));
        Assert.Equal(written, File.ReadAllBytes(Path.Combine(_folder, "regions", "r.0.0.bin.unread")));
    }

    // A region's mobs file of one column at a place in the region, holding a pig's record so many
    // times over.
    private static byte[] Pigs(int count, byte x = 0) => Zipped(zip =>
    {
        using var writer = new BinaryWriter(zip, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write("E3DM"u8);
        writer.Write(3);
        writer.Write(1);
        writer.Write(MobKinds.Pig.Name);
        writer.Write(1);
        writer.Write(x);
        writer.Write((byte)0);
        writer.Write(count);
        for (int i = 0; i < count; i++)
        {
            writer.Write((byte)0);
            writer.Write((byte)0);
            foreach (var value in (double[])[2.5, 4, 2.5]) writer.Write(value);
            writer.Write(0f);
            writer.Write((byte)10);
            writer.Write(false);
        }
    });

    [Fact]
    public void A_Regions_Mobs_That_Inflate_Past_A_Bound_Are_Kept_Beside_Within_It()
    {
        // Five million pigs in a column, in a file of some 1.5 MB.
        var path = Path.Combine(_folder, "regions", "m.0.0.bin");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, Pigs(5_000_000));
        var written = File.ReadAllBytes(path);

        var save = new WorldSave(_folder);
        GC.Collect();
        var before = GC.GetAllocatedBytesForCurrentThread();
        var taken = save.TakeMobs(0, 0);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Null(taken);
        Assert.InRange(allocated, 0, 64L << 20);
        Assert.Equal(written, File.ReadAllBytes(path + ".unread"));
    }

    [Fact]
    public void A_Mobs_File_Of_One_That_Inflates_Past_A_Bound_Is_Kept_Beside_Within_It()
    {
        var pig = new MobRecord(MobKinds.Pig, 0, new LandPlace(2.5, 4, 2.5), 0, 10, null);
        WriteOneFile([.. Enumerable.Range(0, 5000).Select(c => ((c, 0), Enumerable.Repeat(pig, 1000).ToArray()))]);
        var path = Path.Combine(_folder, "mobs.bin");
        var written = File.ReadAllBytes(path);

        var save = new WorldSave(_folder);
        GC.Collect();
        var before = GC.GetAllocatedBytesForCurrentThread();
        var taken = save.TakeMobs(0, 0);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Null(taken);
        Assert.InRange(allocated, 0, 512L << 20);
        Assert.Equal(written, File.ReadAllBytes(path + ".unread"));
    }

    [Fact]
    public void A_Regions_Mobs_Placed_Outside_It_Are_Kept_Beside()
    {
        var path = Path.Combine(_folder, "regions", "m.0.0.bin");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, Pigs(1, x: 40));
        var written = File.ReadAllBytes(path);

        new WorldSave(_folder).TakeMobs(0, 0);

        Assert.Equal(written, File.ReadAllBytes(path + ".unread"));
    }

    [Fact]
    public void A_Mobs_Turn_That_Is_No_Number_Is_Read_As_None()
    {
        var pig = new MobRecord(MobKinds.Pig, 0, new LandPlace(2.5, 4, 2.5), float.NaN, 10, null);
        new WorldSave(_folder).WriteMobs([((0, 0), [pig])]);

        Assert.Equal(pig with { Yaw = 0 }, Assert.Single(new WorldSave(_folder).TakeMobs(0, 0)!));
    }

    [Fact]
    public void A_Region_Of_Every_Column_The_Game_Keeps_Holds_Its_Mobs_Whole()
    {
        // Each column of a region with sixteen mobs, a herd of four and a dozen villagers.
        var cow = new MobRecord(MobKinds.Cow, 0, new LandPlace(2.5, 4, 2.5), 0, 10, null);
        var columns = Enumerable.Range(0, WorldSave.RegionSize * WorldSave.RegionSize)
            .Select(i => (Column: (X: i % WorldSave.RegionSize, Z: i / WorldSave.RegionSize), Mobs: (IReadOnlyList<MobRecord>)Enumerable.Repeat(cow, 16).ToList())).ToList();
        new WorldSave(_folder).WriteMobs(columns);

        var read = new WorldSave(_folder);

        Assert.All(columns, column => Assert.Equal(16, read.TakeMobs(column.Column.X, column.Column.Z)!.Count));
        Assert.False(File.Exists(Path.Combine(_folder, "regions", "m.0.0.bin.unread")));
    }

    [Fact]
    public void A_Worlds_Saturation_That_Is_No_Number_Is_A_New_Worlds()
    {
        new WorldSave(_folder).WriteInfo(Info([.. Blocks.All.Select(b => b.Key)]) with { Saturation = float.NaN });

        Assert.Equal(5, new WorldSave(_folder).ReadInfo()!.Saturation);
    }

    [Fact]
    public void A_Worlds_List_Of_Blocks_Far_Past_The_Numbers_Is_Read_In_Time_And_Held_To_Them()
    {
        // Three hundred thousand keys, a file of some 6 MB.
        new WorldSave(_folder).WriteInfo(Info([.. Enumerable.Range(0, 300_000).Select(i => $"block_{i}")]));
        var save = new WorldSave(_folder);

        var clock = System.Diagnostics.Stopwatch.StartNew();
        save.ReadInfo();

        Assert.InRange(clock.ElapsedMilliseconds, 0, 1000);
        Assert.Equal(ushort.MaxValue + 1, save.BlockKeys.Length);
    }

    [Fact]
    public void A_Worlds_List_Of_Blocks_Holding_No_Key_Reads_It_As_Air()
    {
        new WorldSave(_folder).WriteInfo(Info(["stone", null!, "air"]));
        var save = new WorldSave(_folder);

        save.ReadInfo();

        Assert.Equal(["stone", null!, "air"], save.BlockKeys);
    }

    [Theory]
    [InlineData("world.json", 20)]
    [InlineData("chests.json", 80)]
    [InlineData("furnaces.json", 80)]
    public void A_Saves_File_Far_Past_What_The_Game_Writes_Is_Kept_Beside(string name, int megabytes)
    {
        var save = new WorldSave(_folder);
        save.WriteInfo(Info([.. Blocks.All.Select(b => b.Key)]));
        var path = Path.Combine(_folder, name);
        // Spaces before a file the reader takes, as many megabytes as given.
        File.WriteAllText(path, new string(' ', megabytes << 20) + (name == "world.json" ? File.ReadAllText(path) : "[]"));
        var written = File.ReadAllBytes(path);

        var read = new WorldSave(_folder);
        GC.Collect();
        var before = GC.GetAllocatedBytesForCurrentThread();
        switch (name)
        {
            case "world.json":
                Assert.Null(read.ReadInfo());
                break;
            case "chests.json":
                Assert.Null(read.TakeChests(0, 0));
                break;
            default:
                Assert.Empty(read.ReadFurnaces().All);
                break;
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.InRange(allocated, 0, (megabytes - 4L) << 20);
        Assert.Equal(written, File.ReadAllBytes(path + ".unread"));
    }

    // A region's chests file of one column at a place in the region, holding so many chests each of
    // a slot whose text is so long, the file counting so many columns.
    private static byte[] ChestsFile(int held, byte x, int text, int count) => Zipped(zip =>
    {
        using var writer = new BinaryWriter(zip, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Write("E3DC"u8);
        writer.Write(1);
        writer.Write(count);
        writer.Write(x);
        writer.Write((byte)0);
        writer.Write(held);
        for (int i = 0; i < held; i++)
        {
            writer.Write((byte)(i % 16));
            writer.Write((short)(i / 16 % 300));
            writer.Write((byte)0);
            writer.Write((byte)1);
            writer.Write(new string('x', text));
            writer.Write(false);
            writer.Write(0u);
        }
    });

    [Theory]
    [InlineData(5000, 0, 8, 1)]
    [InlineData(1, 40, 8, 1)]
    [InlineData(1, 0, 300, 1)]
    [InlineData(1, 0, 8, 2000)]
    public void A_Regions_Chests_Past_A_Bound_Are_Kept_Beside(int held, byte x, int text, int count)
    {
        // More chests in a column than the bound, a column outside the region, a slot's text past
        // its bound, and more columns than a region holds.
        var path = Path.Combine(_folder, "regions", "c.0.0.bin");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, ChestsFile(held, x, text, count));
        var written = File.ReadAllBytes(path);

        Assert.Null(new WorldSave(_folder).TakeChests(0, 0));
        Assert.Equal(written, File.ReadAllBytes(path + ".unread"));
    }

    [Fact]
    public void A_Region_Of_Every_Column_The_Game_Keeps_Holds_Its_Chests_Whole()
    {
        // Each column of a region with a floor's row of sixteen chests, every slot full, and one
        // column holding chests to the bound, sixteen floors of them.
        var full = Enumerable.Repeat("stone 64", Chest.Size).ToArray();
        var columns = Enumerable.Range(0, WorldSave.RegionSize * WorldSave.RegionSize).Select(i =>
        {
            var (cx, cz) = (i % WorldSave.RegionSize, i / WorldSave.RegionSize);
            var many = i == 0 ? 4096 : 16;
            return (Column: (X: cx, Z: cz), Chests: (IReadOnlyList<WorldSave.ChestRecord>)[.. Enumerable.Range(0, many)
                .Select(k => new WorldSave.ChestRecord(cx * 16 + k % 16, 4 + k / 256, cz * 16 + k / 16 % 16, full, null, 0))]);
        }).ToList();
        new WorldSave(_folder).WriteChests(columns);

        var read = new WorldSave(_folder);

        Assert.All(columns, column => Assert.Equal(column.Chests.Count, read.TakeChests(column.Column.X, column.Column.Z)!.Count));
        Assert.False(File.Exists(Path.Combine(_folder, "regions", "c.0.0.bin.unread")));
    }
}
