using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

public class MobModelTests
{
    // Each kind's width and height as Minecraft's entity sizes give them, the height of its head's
    // pivot and the length of its legs in pixels as Minecraft's model definitions give them, 24
    // less the pivot's y where Minecraft's y runs down from 24 above the feet.
    public static TheoryData<string, float, float, float, float> Minecraft => new()
    {
        { "pig", 0.9f, 0.9f, 24 - 12, 6 },
        { "cow", 0.9f, 1.4f, 24 - 4, 12 },
        { "sheep", 0.9f, 1.3f, 24 - 6, 12 },
        { "chicken", 0.4f, 0.7f, 24 - 15, 5 },
        { "villager", 0.6f, 1.95f, 24, 12 },
        { "zombie", 0.6f, 1.95f, 24, 12 },
        { "skeleton", 0.6f, 1.99f, 24, 12 },
        { "creeper", 0.6f, 1.7f, 24 - 6, 6 },
        { "spider", 1.4f, 0.9f, 24 - 15, 0 },
    };

    [Theory]
    [MemberData(nameof(Minecraft))]
    public void Each_Kind_Has_Minecrafts_Size_Head_And_Legs(string name, float width, float height, float head, float legs)
    {
        var kind = MobKinds.Find(name)!;
        Assert.Equal(width, kind.HalfWidth * 2, 4);
        Assert.Equal(height, kind.Height, 4);
        Assert.Equal(head, kind.Parts.Single(p => p.Swing == Swing.Head).Pivot.Y, 4);
        foreach (var leg in kind.Parts.Where(p => p.Swing is Swing.LegA or Swing.LegB))
        {
            // A leg reaches from its pivot down to the feet.
            Assert.Equal(legs, leg.Pivot.Y, 4);
            Assert.Equal(0, leg.Boxes.Min(b => b.Min.Y), 4);
        }
    }

    [Fact]
    public void A_Villager_Stands_As_Tall_As_Minecrafts_At_Its_Layers_Scale()
    {
        // Minecraft's layer draws the villager's 34 pixels, its legs and its head of 10, at 0.9375
        // about the feet.
        // The boxes drawn in the game's own colors, the hat's layer a pack's picture lays over the
        // head left out.
        var villager = MobKinds.Villager;
        var top = villager.Parts.SelectMany(p => p.Boxes).Where(b => b.Drawn != Drawn.Pack).Max(b => b.Max.Y);
        Assert.Equal(34, top, 4);
        Assert.Equal(0.9375f, villager.Scale);
        Assert.Equal(1.992f, top / 16 * villager.Scale, 3);
        Assert.Equal(1.406f, villager.Eyes, 3);
    }

    [Fact]
    public void A_Zombie_Reaches_Its_Arms_Out_And_A_Skeleton_Hangs_Them_Until_It_Hunts()
    {
        // An arm's hand, ten pixels below its shoulder, where the stance turns it, in blocks.
        static Vector3 Hand(MobKind kind, ModelPart arm, bool hunting) =>
            Vector3.Transform(new Vector3(arm.Pivot.X, arm.Pivot.Y - 10, arm.Pivot.Z) / 16, arm.PlacedBy(Gait.Turn(kind, arm, new Stance(0, 0, 0, 0, 0, hunting))));

        foreach (var arm in MobKinds.Zombie.Parts.Where(p => p.Swing is Swing.ArmA or Swing.ArmB))
        {
            // Out ahead, toward -z, ten degrees under level at rest and thirty over it hunting.
            var resting = Hand(MobKinds.Zombie, arm, false);
            Assert.Equal((arm.Pivot.Y - 10 * MathF.Sin(MathF.PI / 18)) / 16, resting.Y, 3);
            Assert.True(resting.Z < -0.5f, $"the zombie's hand is at {resting}");
            var hunting = Hand(MobKinds.Zombie, arm, true);
            Assert.Equal((arm.Pivot.Y + 10 * MathF.Sin(MathF.PI / 6)) / 16, hunting.Y, 3);
        }
        foreach (var arm in MobKinds.Skeleton.Parts.Where(p => p.Swing is Swing.ArmA or Swing.ArmB))
        {
            // Hanging at its side standing, and held out level ahead to aim while it hunts.
            var resting = Hand(MobKinds.Skeleton, arm, false);
            Assert.Equal((arm.Pivot.Y - 10) / 16, resting.Y, 3);
            Assert.Equal(arm.Pivot.Z / 16, resting.Z, 3);
            var aiming = Hand(MobKinds.Skeleton, arm, true);
            Assert.Equal(arm.Pivot.Y / 16, aiming.Y, 3);
            Assert.True(aiming.Z < -0.5f, $"the skeleton's hand is at {aiming}");
        }
    }

    [Fact]
    public void A_Stride_Follows_Minecrafts_Walk_And_Its_Legs_Cycle_Every_236_Blocks()
    {
        // A pig's 1.25 blocks a second at 60 frames a second, until the stride has caught up.
        float stride = 0, phase = 0;
        for (int i = 0; i < 600; i++) Gait.Step(ref stride, ref phase, 1.25f / 60, 1 / 60f);
        // Four times the blocks moved in a tick of a twentieth of a second.
        Assert.Equal(0.25f, stride, 3);

        // Ten blocks on, the legs' cos(phase × 0.6662) has cycled once every 2.36 blocks.
        var before = phase;
        for (int i = 0; i < 480; i++) Gait.Step(ref stride, ref phase, 1.25f / 60, 1 / 60f);
        Assert.Equal(2.36f, 10 * 2 * MathF.PI / (0.6662f * (phase - before)), 2);
    }

    [Fact]
    public void Legs_Turn_By_Minecrafts_Angles_A_Villagers_Half_Under_Its_Robe_And_A_Spiders_About_The_Vertical()
    {
        // At the widest of a full stride.
        var stance = new Stance(0, 1, 0, 0, 0, false);
        foreach (var kind in MobKinds.All.Where(k => k != MobKinds.Spider))
            foreach (var leg in kind.Parts.Where(p => p.Swing is Swing.LegA or Swing.LegB))
            {
                var expected = Matrix4x4.CreateRotationX((leg.Swing == Swing.LegA ? 1 : -1) * (kind == MobKinds.Villager ? 0.7f : 1.4f));
                Close(expected, Gait.Turn(kind, leg, stance));
            }

        // A spider's hind legs turn 0.4 about the vertical at the cycle's start and its front legs
        // lift 0.4, a quarter of a cycle on.
        var hind = MobKinds.Spider.Parts.Single(p => p.Swing == Swing.SpiderA && p.Pair == 0);
        Close(ModelPart.Turned(hind.Pose + new Vector3(0, 0.4f, 0)), Gait.Turn(MobKinds.Spider, hind, stance));
        var front = MobKinds.Spider.Parts.Single(p => p.Swing == Swing.SpiderB && p.Pair == 3);
        Close(ModelPart.Turned(front.Pose + new Vector3(0, 0, -0.4f)), Gait.Turn(MobKinds.Spider, front, stance));
    }

    // Two turns alike to a millionth in every entry.
    private static void Close(Matrix4x4 expected, Matrix4x4 actual)
    {
        for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
                Assert.Equal(expected[row, column], actual[row, column], 5);
    }

    [Fact]
    public void Every_Kind_Stands_On_The_Ground_And_No_Foot_Sinks_Below_It_In_A_Stride()
    {
        foreach (var kind in MobKinds.All)
        {
            // Standing, the lowest corner of the model is on the ground, or for the spider, whose
            // legs reach down at an angle, within a pixel above it.
            var standing = kind.Parts.Min(p => Lowest(p, Matrix4x4.Identity));
            Assert.InRange(standing, -1e-4f, kind == MobKinds.Spider ? 1 / 16f : 1e-4f);

            // A leg swings about its hip by up to Minecraft's 1.4 radians each way at a full
            // stride, its foot rising off the ground at the ends of the stride.
            foreach (var leg in kind.Parts.Where(p => p.Swing is Swing.LegA or Swing.LegB))
                for (float swing = -1.4f; swing <= 1.4f; swing += 0.05f)
                {
                    var lowest = Lowest(leg, Matrix4x4.CreateRotationX(swing));
                    Assert.True(lowest >= -1e-4f, $"{kind.Name}'s leg at {leg.Pivot} sinks to {lowest * 16:0.00} pixels at a swing of {swing:0.00}");
                }
        }
    }

    // The lowest corner of a piece's boxes where the renderer places it, in blocks.
    private static float Lowest(ModelPart part, Matrix4x4 swing)
    {
        var placed = part.Placed(swing);
        var lowest = float.MaxValue;
        foreach (var box in part.Boxes)
            for (int i = 0; i < 8; i++)
            {
                var corner = new Vector3((i & 1) == 0 ? box.Min.X : box.Max.X, (i & 2) == 0 ? box.Min.Y : box.Max.Y, (i & 4) == 0 ? box.Min.Z : box.Max.Z) / 16;
                lowest = MathF.Min(lowest, Vector3.Transform(corner, placed).Y);
            }
        return lowest;
    }
}
