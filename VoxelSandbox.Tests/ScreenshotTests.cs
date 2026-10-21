using Xunit;

namespace VoxelSandbox.Tests;

public sealed class ScreenshotTests : IDisposable
{
    private readonly string _folder = Directory.CreateTempSubdirectory("voxel-shots-").FullName;

    public void Dispose() => Directory.Delete(_folder, true);

    [Fact]
    public void A_Screenshot_Is_Named_By_Its_Time_As_Minecraft_Names_Its_Own()
    {
        var time = new DateTime(2026, 10, 21, 3, 45, 12);
        Assert.Equal("2026-10-21_03.45.12.png", Screenshots.Name(time, 1));
        Assert.Equal("2026-10-21_03.45.12_2.png", Screenshots.Name(time, 2));
    }

    [Fact]
    public void Two_In_One_Second_Take_The_Next_Count_Past_A_File_On_Disk_And_One_Not_Written_Yet()
    {
        var shots = new Screenshots(_folder);
        var time = new DateTime(2026, 10, 21, 3, 45, 12);
        File.WriteAllBytes(Path.Combine(_folder, "2026-10-21_03.45.12.png"), []);

        Assert.Equal(Path.Combine(_folder, "2026-10-21_03.45.12_2.png"), shots.Reserve(time));
        // The engine writes the frame once presented, so the second in the same second is not on disk yet.
        Assert.Equal(Path.Combine(_folder, "2026-10-21_03.45.12_3.png"), shots.Reserve(time));
        Assert.Equal(Path.Combine(_folder, "2026-10-21_03.45.13.png"), shots.Reserve(time.AddSeconds(1)));
    }
}
