using Xunit;

namespace VoxelSandbox.Tests;

public class VitalsFadeTests
{
    // Follows the vitals for so many seconds in frames of a sixtieth.
    private static void Run(VitalsFade fade, Vitals vitals, float seconds)
    {
        for (var t = 0f; t < seconds; t += 1 / 60f) fade.Update(vitals, 1 / 60f);
    }

    [Fact]
    public void Full_Rows_Fade_After_Their_Wait_And_A_Hit_Brings_The_Hearts_Back_At_Once()
    {
        var (vitals, fade) = (new Vitals(), new VitalsFade());
        Run(fade, vitals, VitalsFade.Wait - 0.1f);
        Assert.Equal(1, fade.Hearts);
        Assert.Equal(1, fade.Food);
        Run(fade, vitals, 0.1f + VitalsFade.Out + 0.1f);
        Assert.Equal(0, fade.Hearts);
        Assert.Equal(0, fade.Food);
        // Breath that never ran short shows no bubbles.
        Assert.Equal(0, fade.Air);

        vitals.Damage(3, "a test");
        fade.Update(vitals, 1 / 60f);
        Assert.Equal(1, fade.Hearts);
        Assert.Equal(0, fade.Food);
    }

    [Fact]
    public void With_The_Fading_Off_The_Rows_Always_Show()
    {
        var (vitals, fade) = (new Vitals(), new VitalsFade { Enabled = false });
        Run(fade, vitals, VitalsFade.Wait + VitalsFade.Out + 1);
        Assert.Equal(1, fade.Hearts);
        Assert.Equal(1, fade.Food);
        Assert.Equal(0, fade.Air);
    }
}
