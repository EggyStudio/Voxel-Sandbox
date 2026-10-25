using System.Numerics;
using System.Text;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>A pack's models of the mobs in OptiFine's form read into the game's pieces, their roles in the gait, their boxes and their faces' parts of the picture.</summary>
public sealed class PackModelTests
{
    // A cow of the form, a body whose head lies inside it as Fresh Animations lays it, turned a
    // quarter about x as the source lays a cow's body, and four legs pivoted at their hooves, on a
    // picture four times the source's.
    private const string Cow = """
        {
          "textureSize": [256, 128],
          "models": [
            {
              "part": "body", "id": "body", "invertAxis": "xy", "translate": [0, -19, -2],
              "submodels": [
                {
                  "id": "rotation", "invertAxis": "xy", "translate": [0, 13, 2], "rotate": [-90, 0, 0],
                  "boxes": [ { "coordinates": [-6, -8, -1, 12, 18, 10], "uvNorth": [112, 56, 160, 128], "uvUp": [160, 56, 112, 16] } ]
                },
                {
                  "id": "head2", "invertAxis": "xy", "translate": [0, 20, -8],
                  "boxes": [ { "coordinates": [-4, -4, -6, 8, 8, 6], "uvNorth": [24, 24, 56, 56] } ]
                }
              ]
            },
            { "part": "head", "id": "head", "invertAxis": "xy" },
            { "part": "leg1", "id": "leg1", "invertAxis": "xy", "translate": [4, 0, -7], "boxes": [ { "coordinates": [-6, 0, 5, 4, 12, 4], "uvNorth": [16, 80, 32, 128] } ] },
            { "part": "leg2", "id": "leg2", "invertAxis": "xy", "translate": [-4, 0, -7], "boxes": [ { "coordinates": [2, 0, 5, 4, 12, 4], "uvNorth": [16, 80, 32, 128] } ] },
            { "part": "leg3", "id": "leg3", "invertAxis": "xy", "translate": [4, 0, 5], "boxes": [ { "coordinates": [-6, 0, -7, 4, 12, 4], "uvNorth": [16, 80, 32, 128] } ] },
            { "part": "leg4", "id": "leg4", "invertAxis": "xy", "translate": [-4, 0, 5], "boxes": [ { "coordinates": [2, 0, -7, 4, 12, 4], "uvNorth": [16, 80, 32, 128] } ] }
          ]
        }
        """;

    private static List<ModelPart> Parse(string json, MobKind kind) => PackModels.Parse(Encoding.UTF8.GetBytes(json), kind, picture: 0, tinted: false);

    private static (Vector3 Min, Vector3 Max) Bounds(ModelPart part) =>
        (part.Boxes.Select(b => b.Min).Aggregate(Vector3.Min), part.Boxes.Select(b => b.Max).Aggregate(Vector3.Max));

    [Fact]
    public void A_Models_Parts_Take_Their_Roles_The_Head_Lifted_From_The_Body_And_The_Legs_Paired_Across()
    {
        var parts = Parse(Cow, MobKinds.Cow);
        // The empty head is left out, the body's head lifted out of it into a piece of its own.
        Assert.Equal([Swing.None, Swing.Head, Swing.LegB, Swing.LegA, Swing.LegA, Swing.LegB], parts.Select(p => p.Swing));
        var (body, head) = (parts[0], parts[1]);
        Assert.Equal(new Vector3(0, 19, 2), body.Pivot);
        Assert.Equal(new Vector3(0, 20, -8), head.Pivot);
        // The body turned a quarter about x lies over the legs from its head's side to its tail's.
        var (low, high) = Bounds(body);
        Assert.Equal(12, low.Y, 3);
        Assert.Equal(22, high.Y, 3);
        Assert.Equal(-8, low.Z, 3);
        Assert.Equal(10, high.Z, 3);
        Assert.Equal((new Vector3(-4, 16, -14), new Vector3(4, 24, -8)), Bounds(head));
        // A leg pivoted at its hoof swings from the top of its box instead, the right hind one, at +x
        // and +z, with the left front one, as the game's own legs pair.
        Assert.Equal(new Vector3(-4, 12, 7), parts[2].Pivot);
        Assert.Equal((new Vector3(-6, 0, 5), new Vector3(-2, 12, 9)), Bounds(parts[2]));
    }

    [Fact]
    public void A_Boxs_Faces_Take_Their_Own_Parts_Of_The_Picture_Scaled_To_The_Kinds_And_Leave_Out_Those_Not_Given()
    {
        var head = Parse(Cow, MobKinds.Cow)[1];
        var cuboid = head.Boxes[0].Source!.Value;
        var faces = cuboid.Faces();
        // One face given, its part of the picture of 256 by 128 laid on the cow's of 64 at a quarter.
        var face = Assert.Single(faces);
        Assert.Equal(new Vector2(6, 6), face.Select(c => c.Texel).Aggregate(Vector2.Min));
        Assert.Equal(new Vector2(14, 14), face.Select(c => c.Texel).Aggregate(Vector2.Max));
        // It is the front, all its corners at the box's front.
        Assert.All(face, c => Assert.Equal(-14, c.Position.Z, 3));
    }

    [Fact]
    public void A_Kind_No_Pack_Models_Keeps_Its_Own_And_A_File_Not_Of_The_Form_Gives_None()
    {
        Assert.Null(PackModels.Read(ResourcePacks.None, MobKinds.Cow));
        var folder = Directory.CreateTempSubdirectory("voxel-cem-").FullName;
        try
        {
            var cem = Path.Combine(folder, "pack", "assets", "minecraft", "optifine", "cem");
            Directory.CreateDirectory(cem);
            File.WriteAllText(Path.Combine(cem, "cow.jem"), Cow);
            File.WriteAllText(Path.Combine(cem, "pig.jem"), "{ \"models\": [ { \"part\": \"body\", \"boxes\": [ { \"coordinates\": [1, 2] } ] } ] }");
            using var packs = new ResourcePacks([Path.Combine(folder, "pack")]);
            Assert.Equal(6, PackModels.Read(packs, MobKinds.Cow)!.Length);
            Assert.Null(PackModels.Read(packs, MobKinds.Pig));
            Assert.Null(PackModels.Read(packs, MobKinds.Sheep));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void Fresh_Animations_Where_The_Machine_Has_It_Models_Every_Kind_The_Game_Has_With_A_Head_That_Turns()
    {
        // The copy in the reference folder over the checkout, which no other machine has.
        var folder = new DirectoryInfo(AppContext.BaseDirectory);
        while (folder is not null && !File.Exists(Path.Combine(folder.FullName, ".ref", "resourcepacks", "FreshAnimations_v1.9.2.zip"))) folder = folder.Parent;
        if (folder is null) return;
        using var packs = new ResourcePacks([Path.Combine(folder.FullName, ".ref", "resourcepacks", "FreshAnimations_v1.9.2.zip")]);
        foreach (var kind in MobKinds.All)
        {
            var parts = PackModels.Read(packs, kind);
            Assert.True(parts is not null, $"no model of the {kind.Name}");
            Assert.Contains(parts!, p => p.Swing == Swing.Head);
            // As many legs of the body's picture as the game's own, a sheep's wool's legs apart.
            Assert.Equal(kind.Parts.Count(p => p.Swing is Swing.LegA or Swing.LegB or Swing.SpiderA or Swing.SpiderB),
                parts!.Count(p => p.Swing is Swing.LegA or Swing.LegB or Swing.SpiderA or Swing.SpiderB && p.Boxes.All(b => b.Source?.Picture == 0)));
        }
    }
}
