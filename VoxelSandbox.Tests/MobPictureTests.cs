using System.Numerics;
using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>A pack's picture of a mob laid on the source's cuboids as Minecraft lays it.</summary>
public class MobPictureTests
{
    private static ModelPart Part(MobKind kind, Swing swing) => kind.Parts.First(p => p.Swing == swing);

    // The face of a cuboid all of whose corners lie at the extreme of an axis, the lowest or highest.
    private static (Vector3 Position, Vector2 Texel)[] Face(Cuboid cuboid, Func<Vector3, float> axis, bool highest)
    {
        var faces = cuboid.Faces();
        var all = faces.SelectMany(f => f).Select(c => axis(c.Position)).ToArray();
        var at = highest ? all.Max() : all.Min();
        return faces.Single(f => f.All(c => MathF.Abs(axis(c.Position) - at) < 1e-4f));
    }

    private static Vector2 TexelAt((Vector3 Position, Vector2 Texel)[] face, Func<Vector3, bool> corner) => face.Single(c => corner(c.Position)).Texel;

    [Fact]
    public void A_Heads_Front_Top_And_Bottom_Lie_Where_Minecraft_Lays_A_Box_Of_Eight()
    {
        var head = Part(MobKinds.Zombie, Swing.Head).Boxes[0].Source!.Value;
        // The front looks toward -z, its upper left seen from before it, toward +x, at the offset
        // moved right and down by the depth.
        var front = Face(head, p => p.Z, highest: false);
        Assert.Equal(new Vector2(8, 8), TexelAt(front, p => p.X > 0 && p.Y > 28));
        Assert.Equal(new Vector2(16, 16), TexelAt(front, p => p.X < 0 && p.Y < 28));
        // The top beside the bottom in the row above.
        var top = Face(head, p => p.Y, highest: true);
        var bottom = Face(head, p => p.Y, highest: false);
        Assert.Equal((8f, 16f, 0f, 8f), (top.Min(c => c.Texel.X), top.Max(c => c.Texel.X), top.Min(c => c.Texel.Y), top.Max(c => c.Texel.Y)));
        Assert.Equal((16f, 24f, 0f, 8f), (bottom.Min(c => c.Texel.X), bottom.Max(c => c.Texel.X), bottom.Min(c => c.Texel.Y), bottom.Max(c => c.Texel.Y)));
    }

    [Fact]
    public void A_Mirrored_Arm_Lays_Its_Front_The_Other_Way_Across()
    {
        // The right arm, at 40, 16 and four pixels by twelve by four, shows its front from 44 to 48
        // across, its upper left at 44; the left arm, mirrored, the same part turned over.
        var right = Face(Part(MobKinds.Zombie, Swing.ArmB).Boxes[0].Source!.Value, p => p.Z, highest: false);
        var left = Face(Part(MobKinds.Zombie, Swing.ArmA).Boxes[0].Source!.Value, p => p.Z, highest: false);
        var rightTop = right.Max(c => c.Position.Y);
        var leftTop = left.Max(c => c.Position.Y);
        Assert.Equal(new Vector2(44, 20), right.Where(c => c.Position.Y == rightTop).MaxBy(c => c.Position.X).Texel);
        Assert.Equal(new Vector2(48, 20), left.Where(c => c.Position.Y == leftTop).MaxBy(c => c.Position.X).Texel);
    }

    [Fact]
    public void Every_Cuboid_Lies_On_Its_Kinds_Picture_And_Its_Faces_Turn_Out()
    {
        foreach (var kind in MobKinds.All)
            foreach (var box in kind.Parts.SelectMany(p => p.Boxes))
            {
                if (box.Source is not { } cuboid) continue;
                var faces = cuboid.Faces();
                Assert.All(faces.SelectMany(f => f), c => Assert.True(c.Texel.X >= 0 && c.Texel.X <= kind.PictureSize.X && c.Texel.Y >= 0 && c.Texel.Y <= kind.PictureSize.Y,
                    $"{kind.Name}'s cuboid at {box.Min} lays a corner at {c.Texel}, off its picture of {kind.PictureSize}"));
                var corners = faces.SelectMany(f => f).Select(c => c.Position).ToArray();
                Assert.Equal(box.Min, corners.Aggregate(Vector3.Min));
                Assert.Equal(box.Max, corners.Aggregate(Vector3.Max));
                var middle = (box.Min + box.Max) / 2;
                foreach (var face in faces)
                {
                    var normal = Vector3.Cross(face[1].Position - face[0].Position, face[2].Position - face[0].Position);
                    Assert.True(Vector3.Dot(normal, (face[0].Position + face[2].Position) / 2 - middle) > 0, $"{kind.Name}'s cuboid at {box.Min} turns a face in");
                }
            }
    }

    [Fact]
    public void A_Picture_Draws_The_Cuboids_It_Covers_And_Leaves_The_Games_Own_Markings_Out()
    {
        var head = Part(MobKinds.Zombie, Swing.Head);
        var size = MobKinds.Zombie.PictureSize;
        // A picture covered everywhere draws the head and its hat's layer, and none of the eyes and
        // nose of the game's own.
        var covered = new Image(Enumerable.Repeat((byte)255, 64 * 64 * 4).ToArray(), 64, 64);
        Assert.Equal(2 * 6 * 4, MobRenderer.PicturedBoxes(head, covered, size).Vertices.Count);
        // Clear where the hat's layer lies, it draws the head alone.
        var bare = (byte[])covered.Data.Clone();
        for (int y = 0; y < 16; y++)
            for (int x = 32; x < 64; x++) bare[(y * 64 + x) * 4 + 3] = 0;
        var vertices = MobRenderer.PicturedBoxes(head, new Image(bare, 64, 64), size).Vertices;
        Assert.Equal(6 * 4, vertices.Count);
        // The front's upper left at the face's corner of the picture, over the picture's size and a
        // white row below it.
        var front = vertices.Where(v => v.Normal.Z < -0.5f).ToArray();
        var corner = front.MaxBy(v => v.Position.X + v.Position.Y);
        Assert.Equal(new Vector2(8f / 64, 8f / 65), corner.Uv);
    }

    [Fact]
    public void A_Villagers_Clothes_Lie_Over_Its_Body_Where_They_Are_Not_Clear()
    {
        // Two pixels, the clothes clear over the first and covering the second.
        var body = new Image([10, 20, 30, 255, 10, 20, 30, 255], 2, 1);
        var clothes = new Image([200, 0, 0, 0, 0, 200, 0, 255], 2, 1);
        Assert.Equal(new byte[] { 10, 20, 30, 255, 0, 200, 0, 255 }, MobRenderer.Over(body, clothes).Data);
        Assert.Equal("entity/villager/type/snow", PackTextures.OverMob("villager", 3));
        Assert.Null(PackTextures.OverMob("zombie", 0));
    }

    [Fact]
    public void A_Sheeps_Wool_Is_Its_Own_Picture_Tinted_By_Its_Color()
    {
        var sheep = MobKinds.Sheep;
        var covered = new Image(Enumerable.Repeat((byte)255, 64 * 32 * 4).ToArray(), 64, 32);
        var head = Part(sheep, Swing.Head);
        var pink = sheep.Variants[5];
        // The body's picture lays the head alone, its eyes the game's own and left out, and the wool's
        // picture the wool's head in the sheep's color.
        var skin = MobRenderer.PicturedBoxes(head, covered, sheep.PictureSize, 0, pink);
        var wool = MobRenderer.PicturedBoxes(head, covered, sheep.PictureSize, 1, pink);
        Assert.Equal(6 * 4, skin.Vertices.Count);
        Assert.Equal(6 * 4, wool.Vertices.Count);
        Assert.True(wool.Vertices.Max(v => v.Position.Y) > skin.Vertices.Max(v => v.Position.Y));
        Assert.All(wool.Colors, c => Assert.Equal(pink, c));
        Assert.All(skin.Colors, c => Assert.Equal(Color.White, c));
        Assert.Equal(2, PackTextures.OfMob("sheep").Count);
    }

    [Fact]
    public void A_Cows_Picture_Half_As_Tall_Is_Laid_As_Before_Its_Muzzle()
    {
        // Bare Bones' cow of 64 by 32 on the layout of 64 by 64, the same offsets but for the muzzle,
        // at row 33, which the picture does not reach.
        var cow = MobKinds.Cow;
        var old = new Image(Enumerable.Repeat((byte)255, 64 * 32 * 4).ToArray(), 64, 32);
        Assert.True(MobRenderer.Fits(old, cow.PictureSize));
        Assert.False(MobRenderer.Fits(new Image(new byte[64 * 48 * 4], 64, 48), cow.PictureSize));
        var head = MobRenderer.PicturedBoxes(Part(cow, Swing.Head), old, cow.PictureSize).Vertices;
        // The head and its two horns, and the front's upper left over the picture's 32 rows and the white one.
        Assert.Equal(3 * 6 * 4, head.Count);
        var front = head.Where(v => v.Normal.Z < -0.5f && v.Position.Z < -13.9f / 16).ToArray();
        Assert.Equal(new Vector2(6f / 64, 6f / 33), front.MaxBy(v => v.Position.X + v.Position.Y).Uv);
    }

    [Fact]
    public void The_Skeletons_Bow_Takes_The_White_Row_Below_The_Picture()
    {
        var size = MobKinds.Skeleton.PictureSize;
        var picture = new Image(Enumerable.Repeat((byte)128, 64 * 32 * 4).ToArray(), 64, 32);
        var whitened = MobRenderer.Whitened(picture, size);
        Assert.Equal((64, 33), (whitened.Width, whitened.Height));
        Assert.All(whitened.Data[(64 * 32 * 4)..], b => Assert.Equal(255, b));
        var arm = MobRenderer.PicturedBoxes(Part(MobKinds.Skeleton, Swing.ArmB), picture, size).Vertices;
        // The arm's cuboid and the bow's box of the game's own, its corners at the white row's middle.
        Assert.Equal(2 * 6 * 4, arm.Count);
        Assert.All(arm.Skip(24), v => Assert.Equal(new Vector2(0.5f / 64, 32.5f / 33), v.Uv));
    }
}
