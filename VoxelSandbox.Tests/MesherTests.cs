using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

public class MesherTests
{
    // A floor of bedrock at 0, so the section above it is meshed with the world around it loaded.
    private static VoxelWorld Floor() => Laid.World(c => c.Set(0, 0, 0, BlockId.Bedrock));

    private static SectionMesher Meshed(VoxelWorld world)
    {
        var mesher = new SectionMesher();
        Assert.True(mesher.Mesh(world, new SectionKey(0, 1, 0)));
        return mesher;
    }

    [Fact]
    public void A_Block_In_The_Air_Shows_Its_Six_Faces()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Stone);

        var mesher = Meshed(world);
        Assert.Equal(24, mesher.Solid.Count);
        Assert.Equal(36, mesher.Solid.Indices.Count);
        Assert.Equal(2 * 24, mesher.Solid.Packed.Count);
        Assert.Equal(0, mesher.SeeThrough.Count);
    }

    [Fact]
    public void Lava_Shows_Its_Faces_In_A_Mesh_Of_Its_Own_And_None_Between_Two_Blocks_Of_It()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Lava);
        world.SetBlock(9, 20, 8, BlockId.Lava);

        var mesher = Meshed(world);
        Assert.Equal(10 * 4, mesher.Molten.Count);
        Assert.Equal(0, mesher.Solid.Count);
        Assert.Empty(mesher.Emitters);
        // A source's top stands an eighth under its cell's, as water's does.
        Assert.Equal(4.875f, mesher.Molten.Positions.Max(p => p.Y));
    }

    [Fact]
    public void Two_Blocks_Side_By_Side_Hide_The_Faces_Between_Them()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Stone);
        world.SetBlock(9, 20, 8, BlockId.Dirt);

        Assert.Equal(10 * 4, Meshed(world).Solid.Count);
    }

    [Fact]
    public void Glass_Beside_Glass_Shows_No_Face_Between_And_Glass_Beside_Stone_Shows_The_Stone()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Glass);
        world.SetBlock(9, 20, 8, BlockId.Glass);
        world.SetBlock(7, 20, 8, BlockId.Stone);

        var mesher = Meshed(world);
        // The panes' ten faces less the one toward the stone, and the stone's six with its face toward the glass.
        Assert.Equal(9 * 4, mesher.SeeThrough.Count);
        Assert.Equal(6 * 4, mesher.Solid.Count);
    }

    [Fact]
    public void Water_Open_To_The_Air_Stands_An_Eighth_Low()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Water);

        var tops = Meshed(world).SeeThrough.Positions.Select(p => p.Y).Distinct().Order().ToArray();
        Assert.Equal(new[] { 4f, 4.875f }, tops);
    }

    [Fact]
    public void A_Block_That_Gives_Off_Light_Is_Left_Out_Of_The_Faces_For_Its_Own_Cube()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Glowstone);

        var mesher = Meshed(world);
        Assert.Equal(0, mesher.Solid.Count);
        Assert.Equal(BlockId.Glowstone, Assert.Single(mesher.Emitters).Block);
    }

    [Fact]
    public void Stone_Against_A_Lamp_Keeps_Its_Faces_Toward_It_So_Its_Mesh_Has_No_Open_Edge()
    {
        // A floor and a wall of stone with a glowstone in their corner, as a room's lamp stands. The
        // scene's distance field reads a cell whose nearest point lies on an edge only one triangle
        // of its mesh has as open, so stone with a hole where the lamp is would read as air.
        var world = Floor();
        for (int z = 6; z <= 8; z++)
        {
            for (int x = 6; x <= 8; x++) world.SetBlock(x, 20, z, BlockId.Stone);
            for (int y = 21; y <= 22; y++) world.SetBlock(6, y, z, BlockId.Stone);
        }
        world.SetBlock(7, 21, 7, BlockId.Glowstone);

        var mesher = Meshed(world);
        var faces = mesher.Solid;
        var edges = new Dictionary<(System.Numerics.Vector3, System.Numerics.Vector3), int>();
        for (int i = 0; i < faces.Indices.Count; i += 3)
            for (int k = 0; k < 3; k++)
            {
                var (a, b) = (faces.Positions[(int)faces.Indices[i + k]], faces.Positions[(int)faces.Indices[i + (k + 1) % 3]]);
                var key = a.X < b.X || a.X == b.X && (a.Y < b.Y || a.Y == b.Y && a.Z < b.Z) ? (a, b) : (b, a);
                edges[key] = edges.GetValueOrDefault(key) + 1;
            }
        Assert.DoesNotContain(edges, e => e.Value == 1);
        // The floor's top under the lamp and the wall's side behind it, each a face of four corners
        // whose lowest is the lamp's cell's.
        var lowest = Enumerable.Range(0, faces.Count / 4)
            .Select(q => (faces.Unpack(4 * q).Face, Enumerable.Range(4 * q, 4).Select(i => faces.Positions[i]).Aggregate(System.Numerics.Vector3.Min)))
            .ToArray();
        Assert.Contains((2, new System.Numerics.Vector3(7, 5, 7)), lowest);
        Assert.Contains((0, new System.Numerics.Vector3(7, 5, 7)), lowest);
        Assert.Equal(BlockId.Glowstone, Assert.Single(mesher.Emitters).Block);
    }

    [Fact]
    public void A_Corner_Where_Two_Blocks_Meet_Is_Darker_Than_An_Open_One()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Stone);
        // Walls on two sides of the top face's corner at +x, +z.
        world.SetBlock(9, 21, 8, BlockId.Stone);
        world.SetBlock(8, 21, 9, BlockId.Stone);

        var mesher = Meshed(world);
        var faces = mesher.Solid;
        var top = Enumerable.Range(0, faces.Count).Where(i => faces.Unpack(i).Face == 2 && faces.Positions[i].Y == 5).ToArray();
        var closed = top.Single(i => faces.Positions[i].X == 9 && faces.Positions[i].Z == 9);
        var open = top.Single(i => faces.Positions[i].X == 8 && faces.Positions[i].Z == 8);
        Assert.True(faces.Unpack(closed).Shade < faces.Unpack(open).Shade);
        Assert.True(faces.Colors[closed].R < faces.Colors[open].R);
    }

    [Fact]
    public void The_Corner_Strength_Takes_A_Closed_Corners_Shade_That_Far_From_One()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Stone);
        world.SetBlock(9, 21, 8, BlockId.Stone);
        world.SetBlock(8, 21, 9, BlockId.Stone);

        // With the light levels left out, the corner between the two walls is Minecraft's darkest,
        // 0.45, at a strength of 1, halfway to 1 at 0.5, the strength a mesher starts at, and 1 at 0.
        Assert.Equal(0.5f, new SectionMesher().CornerStrength);
        foreach (var (strength, shade) in new[] { (1f, 0.45f), (0.5f, 0.725f), (0f, 1f) })
        {
            var mesher = new SectionMesher { LightLevels = false, CornerStrength = strength };
            Assert.True(mesher.Mesh(world, new SectionKey(0, 1, 0)));
            var faces = mesher.Solid;
            var closed = Enumerable.Range(0, faces.Count).Single(i => faces.Unpack(i).Face == 2 && faces.Positions[i] == new System.Numerics.Vector3(9, 5, 9));
            // Within half a step of the 128 levels a corner's shade is packed in.
            Assert.InRange(faces.Unpack(closed).Shade, shade - 0.5f / 127, shade + 0.5f / 127);
        }
    }

    [Fact]
    public void A_Corners_Packed_Bytes_Hold_Its_Face_Corner_Shade_And_Color_As_The_Shader_Reads_Them()
    {
        var (first, second) = SectionMesher.Pack(4, 3, 0.5f, new Color(200, 100, 50, 168));
        var (face, corner, shade, tint) = SectionMesher.Unpack(first, second);

        Assert.Equal((4, 3), (face, corner));
        Assert.Equal(64 / 127f, shade, 4);
        Assert.Equal(new Color(200, 100, 50, 168), tint);
    }

    [Fact]
    public void A_Corners_Alpha_From_240_Up_Reads_As_One_With_Its_Sky_Level_And_One_Below_As_It_Is_With_None()
    {
        var (_, opaque) = SectionMesher.Pack(2, 0, 1, new Color(10, 20, 30, 255), sky: 13);
        Assert.Equal((13, (byte)255), (SectionMesher.SkyOf(opaque), SectionMesher.Unpack(0, opaque).Tint.A));
        var (_, water) = SectionMesher.Pack(2, 0, 1, new Color(44, 84, 162, 191), sky: 13);
        Assert.Equal((0, (byte)191), (SectionMesher.SkyOf(water), SectionMesher.Unpack(0, water).Tint.A));
    }

    [Fact]
    public void A_Face_And_A_Plant_Under_Two_Leaves_Carry_Sky_Level_13_And_A_Face_In_The_Open_15()
    {
        // The terrain shader's floor of the sky's light reads the level each corner carries, which
        // under a canopy two leaves deep is the source's 13.
        var world = Floor();
        for (int x = 4; x <= 12; x++)
            for (int z = 4; z <= 12; z++)
                for (int y = 26; y <= 27; y++)
                    world.SetBlock(x, y, z, BlockId.OakLeaves);
        world.SetBlock(8, 20, 8, BlockId.Stone);
        world.SetBlock(10, 20, 10, BlockId.Stone);
        world.SetBlock(10, 21, 10, BlockId.ShortGrass);
        world.SetBlock(1, 20, 1, BlockId.Stone);

        var mesher = Meshed(world);
        var faces = mesher.Solid;
        int[] Top(int x, int z) => [.. Enumerable.Range(0, faces.Count).Where(i => faces.Unpack(i).Face == 2 && faces.Positions[i].Y == 5
            && faces.Positions[i].X is var px && px >= x && px <= x + 1 && faces.Positions[i].Z is var pz && pz >= z && pz <= z + 1)];
        Assert.Equal(4, Top(8, 8).Length);
        Assert.All(Top(8, 8), i => Assert.Equal(13, faces.SkyOf(i)));
        Assert.All(Top(1, 1), i => Assert.Equal(15, faces.SkyOf(i)));
        Assert.NotEmpty(Top(1, 1));
        Assert.NotEqual(0, mesher.Cutout.Count);
        Assert.All(Enumerable.Range(0, mesher.Cutout.Count), i => Assert.Equal(13, mesher.Cutout.SkyOf(i)));
    }

    [Fact]
    public void A_Cactus_Is_The_Sources_Model_Its_Sides_A_Pixel_In_Across_The_Whole_Cell_And_Its_Box_A_Pixel_In()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Stone);
        world.SetBlock(8, 21, 8, BlockId.Cactus);

        // The cactus's cell is 5 to 6 in the section. Its bottom on the stone is left out, and the
        // stone's top under it, which the cactus does not hide, lies at 5.
        var faces = Meshed(world).Solid;
        var quads = Enumerable.Range(0, faces.Count / 4)
            .Select(q => (faces.Unpack(4 * q).Face,
                Min: Enumerable.Range(4 * q, 4).Select(i => faces.Positions[i]).Aggregate(System.Numerics.Vector3.Min),
                Max: Enumerable.Range(4 * q, 4).Select(i => faces.Positions[i]).Aggregate(System.Numerics.Vector3.Max)))
            .Where(f => f.Min.Y >= 5 && f.Max.Y <= 6 && !(f.Face == 2 && f.Max.Y == 5))
            .OrderBy(f => f.Face).ToArray();
        const float In = 1 / 16f, Out = 15 / 16f;
        (int, System.Numerics.Vector3, System.Numerics.Vector3)[] model =
        [
            (0, new(8 + Out, 5, 8), new(8 + Out, 6, 9)),
            (1, new(8 + In, 5, 8), new(8 + In, 6, 9)),
            (2, new(8, 6, 8), new(9, 6, 9)),
            (4, new(8, 5, 8 + Out), new(9, 6, 8 + Out)),
            (5, new(8, 5, 8 + In), new(9, 6, 8 + In)),
        ];
        Assert.Equal(model.Length, quads.Length);
        for (int i = 0; i < model.Length; i++)
        {
            Assert.Equal(model[i].Item1, quads[i].Face);
            Assert.True(System.Numerics.Vector3.Distance(model[i].Item2, quads[i].Min) < 1e-5f && System.Numerics.Vector3.Distance(model[i].Item3, quads[i].Max) < 1e-5f,
                $"face {quads[i].Face} from {quads[i].Min} to {quads[i].Max}");
        }

        var box = Assert.Single(Blocks.Get(BlockId.Cactus).Boxes);
        Assert.Equal(new Box(new System.Numerics.Vector3(In, 0, In), new System.Numerics.Vector3(Out, Out, Out)), box);
    }

    [Fact]
    public void No_Corner_Of_A_Generated_Sections_Faces_Lies_Inside_Another_Faces_Edge()
    {
        // Seed 1's ground at the origin, generated and meshed with its neighbors about it. Each block's
        // face is a quad of its own with its corners on the block grid, so faces that meet share their
        // corners, and none lies inside another's edge, where the rasterizer would leave a crack
        // between the two faces as the view moves.
        var land = new Overworld(1);
        var world = new VoxelWorld(land);
        for (int z = -1; z <= 1; z++)
            for (int x = -1; x <= 1; x++)
            {
                var column = land.Generate(x, z);
                Lighting.Compute(column);
                world.Add(column);
            }
        var mesher = new SectionMesher();
        var ground = land.At(8, 8).Height;
        Assert.True(mesher.Mesh(world, new SectionKey(0, ground >> Section.Shift, 0)));
        var faces = mesher.Solid;
        Assert.True(faces.Count > 400, $"{faces.Count / 4} faces");

        var corners = Enumerable.Range(0, faces.Count).Select(i => faces.Positions[i]).Distinct().ToList();
        var inside = 0;
        for (int face = 0; face < faces.Count; face += 4)
            for (int i = 0; i < 4; i++)
            {
                System.Numerics.Vector3 a = faces.Positions[face + i], b = faces.Positions[face + (i + 1) % 4];
                var along = b - a;
                var length = along.LengthSquared();
                foreach (var corner in corners)
                {
                    var t = System.Numerics.Vector3.Dot(corner - a, along) / length;
                    if (t <= 1e-4f || t >= 1 - 1e-4f) continue;
                    if (System.Numerics.Vector3.DistanceSquared(a + along * t, corner) < 1e-8f) inside++;
                }
            }
        Assert.Equal(0, inside);
    }

    [Fact]
    public void Each_Corner_Of_A_Face_Lies_Toward_The_Faces_Normal_From_The_Blocks_Middle()
    {
        // The face a corner's packed bytes name is the one its position lies on, so the shader's
        // normal from the face points out of the block the corner belongs to.
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Stone);

        var faces = Meshed(world).Solid;
        var middle = new System.Numerics.Vector3(8.5f, 4.5f, 8.5f);
        for (int i = 0; i < faces.Count; i++)
        {
            var normal = SectionMesher.Normal(faces.Unpack(i).Face);
            Assert.Equal(0.5f, System.Numerics.Vector3.Dot(faces.Positions[i] - middle, normal), 4);
        }
    }

    [Fact]
    public void A_Change_Of_Light_Keeps_The_Positions_And_Changes_The_Packed_Bytes()
    {
        // A closed cavity in stone, dark until a lamp is set in it.
        var world = Laid.World(c => Laid.Fill(c, 0, 40, BlockId.Stone));
        for (int x = 6; x <= 11; x++) world.SetBlock(x, 20, 8, BlockId.Air);
        var mesher = Meshed(world);
        var positions = mesher.Solid.Positions.ToArray();
        var packed = mesher.Solid.Packed.ToArray();

        // The lamp's own cube is drawn apart, so the faces stay where they were, the one it covers
        // left out after the others, and their light changes.
        world.SetBlock(11, 20, 8, BlockId.Glowstone);
        mesher = Meshed(world);
        var lit = Enumerable.Range(0, mesher.Solid.Count).Where(i => positions.Contains(mesher.Solid.Positions[i])).ToArray();

        Assert.NotEmpty(lit);
        Assert.Contains(lit, i => mesher.Solid.Unpack(i).Shade > SectionMesher.Unpack(packed[2 * Array.IndexOf(positions, mesher.Solid.Positions[i])], packed[2 * Array.IndexOf(positions, mesher.Solid.Positions[i]) + 1]).Shade);
    }
}
