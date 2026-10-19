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
    public void A_Zombie_And_A_Skeleton_Hold_Their_Arms_Out_Ahead()
    {
        foreach (var kind in new[] { MobKinds.Zombie, MobKinds.Skeleton })
            foreach (var arm in kind.Parts.Where(p => p.Swing is Swing.ArmA or Swing.ArmB))
            {
                // The arm's hand, below its shoulder, turned out ahead of the body, toward -z.
                var hand = Vector3.Transform(new Vector3(arm.Pivot.X, arm.Pivot.Y - 10, arm.Pivot.Z) / 16, arm.Placed(Matrix4x4.Identity));
                Assert.Equal(arm.Pivot.Y / 16, hand.Y, 3);
                Assert.True(hand.Z < -0.5f, $"{kind.Name}'s hand is at {hand}");
            }
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

            // A leg swings about its hip by up to 0.8 radians each way, as the renderer swings it,
            // its foot rising off the ground at the ends of the stride.
            foreach (var leg in kind.Parts.Where(p => p.Swing is Swing.LegA or Swing.LegB))
                for (float swing = -0.8f; swing <= 0.8f; swing += 0.05f)
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
