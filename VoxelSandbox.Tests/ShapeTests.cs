using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

public class ShapeTests
{
    // A floor of bedrock at 0, the rest open sky.
    private static VoxelWorld Floor() => Laid.World(c => c.Set(0, 0, 0, BlockId.Bedrock));

    // Solid stone from the bottom to 20, the open sky above it.
    private static VoxelWorld Stone() => Laid.World(c => Laid.Fill(c, 0, 20, BlockId.Stone));

    [Fact]
    public void A_Torch_Is_Its_Stick_In_The_Sections_Faces_And_Its_Flame_A_Lamp()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Torch);

        var mesher = new SectionMesher();
        Assert.True(mesher.Mesh(world, new SectionKey(0, 1, 0)));
        Assert.Equal(6 * 4, mesher.Solid.Count);
        Assert.Equal(BlockId.Torch, Assert.Single(mesher.Emitters).Block);
        Assert.Equal(Surfaces.Flame, Assert.Single(Blocks.Get(BlockId.Torch).Glows).Surface);
    }

    [Fact]
    public void A_Torch_Lets_The_Sky_Through_And_Lights_The_Blocks_Around_It_From_14()
    {
        var world = Stone();
        for (int y = 10; y <= 20; y++) world.SetBlock(8, y, 8, BlockId.Air);
        world.SetBlock(8, 10, 8, BlockId.Torch);

        Assert.Equal(15, world.GetLight(8, 10, 8).Sky);
        Assert.Equal(14, world.GetLight(8, 10, 8).Block);
        Assert.Equal(13, world.GetLight(8, 11, 8).Block);
    }

    [Fact]
    public void A_Soul_Torch_Lit_Brighter_By_A_Lamp_Falls_Back_To_Its_Own_Level_When_The_Lamp_Is_Broken()
    {
        var world = Stone();
        for (int x = 6; x <= 12; x++) world.SetBlock(x, 10, 8, BlockId.Air);
        world.SetBlock(8, 10, 8, BlockId.Glowstone);
        world.SetBlock(9, 10, 8, BlockId.SoulTorch);
        Assert.Equal(14, world.GetLight(9, 10, 8).Block);

        world.SetBlock(8, 10, 8, BlockId.Air);
        Assert.Equal(10, world.GetLight(9, 10, 8).Block);
        Assert.Equal(9, world.GetLight(10, 10, 8).Block);
        Assert.Equal(9, world.GetLight(8, 10, 8).Block);
    }

    [Fact]
    public void Breaking_The_Wall_Behind_A_Torch_Takes_The_Torch_With_It()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Stone);
        world.SetBlock(8, 20, 7, BlockId.WallTorchNorth);
        world.SetBlock(8, 21, 8, BlockId.Torch);
        world.SetBlock(9, 20, 8, BlockId.WallTorchEast);

        world.SetBlock(8, 20, 8, BlockId.Air);
        Assert.Equal(BlockId.Air, world.GetBlock(8, 20, 7));
        Assert.Equal(BlockId.Air, world.GetBlock(8, 21, 8));
        Assert.Equal(BlockId.Air, world.GetBlock(9, 20, 8));
    }

    [Fact]
    public void A_Torch_Stays_When_A_Block_It_Does_Not_Hold_To_Is_Broken()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Stone);
        world.SetBlock(9, 20, 8, BlockId.Stone);
        world.SetBlock(8, 21, 8, BlockId.Torch);

        world.SetBlock(9, 20, 8, BlockId.Air);
        Assert.Equal(BlockId.Torch, world.GetBlock(8, 21, 8));
    }

    [Fact]
    public void A_Torch_On_A_Walls_Side_Leans_Away_From_It_And_A_Lantern_Under_A_Ceiling_Hangs()
    {
        var world = Floor();
        world.SetBlock(8, 20, 9, BlockId.Stone);
        world.SetBlock(5, 21, 5, BlockId.Stone);

        // The north face of the stone at z 9, met from the cell at z 8.
        Assert.Equal(BlockId.WallTorchNorth, world.Fit(BlockId.Torch, 8, 20, 8, 0, 0, -1));
        Assert.Equal(BlockId.HangingLantern, world.Fit(BlockId.Lantern, 5, 20, 5, 0, -1, 0));
        // A torch under a ceiling with nothing else to hold to is not placed.
        Assert.Equal(BlockId.Air, world.Fit(BlockId.Torch, 5, 20, 5, 0, -1, 0));
        Assert.Equal(BlockId.Stone, world.Fit(BlockId.Stone, 5, 20, 5, 0, -1, 0));
    }

    [Fact]
    public void Each_Way_Of_A_Block_That_Holds_To_Another_Touches_The_Face_Of_Its_Cell_Toward_It_And_Glows_Inside_Its_Cell()
    {
        foreach (var block in Blocks.All)
        {
            if (block.Shape is not { Falls: true } shape) continue;
            var (sx, sy, sz) = shape.Support;
            var toward = new Vector3(sx, sy, sz);
            // The box reaches the face of the cell toward its support, 0 or 1 along that axis.
            var reach = Vector3.Dot(Vector3.Abs(toward), toward.X + toward.Y + toward.Z > 0 ? shape.Max : Vector3.One - shape.Min);
            Assert.True(MathF.Abs(reach - 1) < 1e-5f, $"{block.Key} reaches {reach} of the way to what it holds to");

            foreach (var (local, _) in block.Glows)
            {
                var half = new Vector3(local.M11, local.M22, local.M33) / 2;
                Assert.True(Vector3.Min(local.Translation - half, Vector3.Zero) == Vector3.Zero, $"{block.Key} has an emissive box outside its cell");
                Assert.True(Vector3.Max(local.Translation + half, Vector3.One) == Vector3.One, $"{block.Key} has an emissive box outside its cell");
            }
        }
    }

    [Fact]
    public void A_Ray_Beside_A_Torchs_Stick_Passes_To_The_Block_Behind_It()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Torch);
        world.SetBlock(8, 20, 10, BlockId.Stone);

        var past = VoxelRay.Cast(world, new Vector3(8.1f, 20.3f, 6), Vector3.UnitZ, 10);
        Assert.Equal(10, past?.Z);

        var met = VoxelRay.Cast(world, new Vector3(8.5f, 20.3f, 6), Vector3.UnitZ, 10);
        Assert.Equal((8, 20, 8), (met?.X, met?.Y, met?.Z) is (int x, int y, int z) ? (x, y, z) : default);
        Assert.Equal(-1, met?.NormalZ);
        Assert.Equal(2 + 7 / 16f, met?.Distance ?? 0, 4);
    }

    [Fact]
    public void Stairs_Climb_The_Way_The_Player_Looks_And_A_Slab_Against_A_Ceiling_Takes_The_Upper_Half()
    {
        var world = Floor();

        Assert.Equal(BlockId.OakStairsEast, world.Fit(BlockId.OakStairs, 8, 20, 8, 0, 1, 0, Facing.East));
        Assert.Equal(BlockId.OakStairsWestTop, world.Fit(BlockId.OakStairs, 8, 20, 8, 1, 0, 0, Facing.West, 0.7f));
        Assert.Equal(BlockId.StoneSlabTop, world.Fit(BlockId.StoneSlab, 8, 20, 8, 0, -1, 0, Facing.North));
        Assert.Equal(BlockId.StoneSlab, world.Fit(BlockId.StoneSlab, 8, 20, 8, -1, 0, 0, Facing.North, 0.3f));
    }

    [Fact]
    public void Stairs_Turn_Into_An_Outer_Corner_By_The_Stairs_Behind_Them_And_An_Inner_One_By_The_Stairs_Before_Them()
    {
        var world = Floor();
        // Stairs climbing north with stairs climbing east behind them, to their north.
        world.SetBlock(8, 20, 8, BlockId.OakStairs);
        world.SetBlock(8, 20, 7, BlockId.OakStairsEast);
        Assert.Equal(StairForm.OuterRight, world.ShapeAt(8, 20, 8)!.Form);

        // Stairs climbing north with stairs climbing west before them, to their south.
        world.SetBlock(8, 20, 7, BlockId.Air);
        world.SetBlock(8, 20, 9, BlockId.OakStairsWest);
        Assert.Equal(StairForm.InnerLeft, world.ShapeAt(8, 20, 8)!.Form);

        // Stairs the same as these where those turn to keep them straight, as do stairs of the other half.
        world.SetBlock(7, 20, 8, BlockId.OakStairs);
        Assert.Equal(StairForm.Straight, world.ShapeAt(8, 20, 8)!.Form);
        world.SetBlock(7, 20, 8, BlockId.Air);
        world.SetBlock(8, 20, 9, BlockId.OakStairsWestTop);
        Assert.Equal(StairForm.Straight, world.ShapeAt(8, 20, 8)!.Form);
    }

    [Fact]
    public void A_Corner_Of_Stairs_Is_The_Slab_And_The_Quarters_Of_The_Step_Toward_Its_Side()
    {
        static (Vector3 From, Vector3 To)[] Steps(BlockShape shape) => [.. shape.Pieces.Skip(1).Select(p => (p.From, p.To))];

        // Climbing north, the outer corner to the left keeps the north-west quarter of the step.
        var outer = BlockShape.Stairs(Surfaces.Planks, Facing.North, false, StairForm.OuterLeft);
        Assert.Equal((new Vector3(0, 0, 0), new Vector3(1, 0.5f, 1)), (outer.Pieces[0].From, outer.Pieces[0].To));
        Assert.Equal([(new Vector3(0, 0.5f, 0), new Vector3(0.5f, 1, 0.5f))], Steps(outer));

        // Climbing east, the left is north, so the quarter is the north-east one.
        Assert.Equal([(new Vector3(0.5f, 0.5f, 0), new Vector3(1, 1, 0.5f))], Steps(BlockShape.Stairs(Surfaces.Planks, Facing.East, false, StairForm.OuterLeft)));

        // An inner corner to the right, climbing north, adds the south-east quarter to the step.
        var inner = Steps(BlockShape.Stairs(Surfaces.Planks, Facing.North, false, StairForm.InnerRight));
        Assert.Contains((new Vector3(0, 0.5f, 0), new Vector3(1, 1, 0.5f)), inner);
        Assert.Contains((new Vector3(0.5f, 0.5f, 0.5f), new Vector3(1, 1, 1)), inner);

        // Turned over, the corner's quarter hangs under the slab.
        Assert.Equal([(new Vector3(0, 0, 0), new Vector3(0.5f, 0.5f, 0.5f))], Steps(BlockShape.Stairs(Surfaces.Planks, Facing.North, true, StairForm.OuterLeft)));
    }

    [Fact]
    public void A_Ray_Through_A_Stairs_Open_Quarter_Reaches_The_Block_Behind_And_One_From_Above_Meets_The_Low_Step()
    {
        var world = Floor();
        // Stairs climbing north, their step on the north half, and stone east of them.
        world.SetBlock(8, 20, 8, BlockId.OakStairs);
        world.SetBlock(10, 20, 8, BlockId.Stone);

        var past = VoxelRay.Cast(world, new Vector3(6, 20.75f, 8.75f), Vector3.UnitX, 10);
        Assert.Equal((10, 20, 8), past is { } through ? (through.X, through.Y, through.Z) : default);

        var down = VoxelRay.Cast(world, new Vector3(8.5f, 22, 8.75f), -Vector3.UnitY, 10);
        Assert.Equal((8, 20, 8), down is { } met ? (met.X, met.Y, met.Z) : default);
        Assert.Equal(1, down?.NormalY);
        Assert.Equal(1.5f, down?.Distance ?? 0, 4);
    }

    [Fact]
    public void A_Stairs_Outline_Has_The_Crease_Under_Its_Step_And_No_Line_Across_Its_Flat_Back()
    {
        var edges = Blocks.Get(BlockId.OakStairs).Shape!.Edges;
        // An edge along x on a line where y is a and z is b.
        bool Along(float y, float z) => edges.Any(e => e.From.Y == y && e.To.Y == y && e.From.Z == z && e.To.Z == z && e.From.X != e.To.X);

        // Climbing north, the step's front at z 0.5 meets the slab's top at y 0.5.
        Assert.True(Along(0.5f, 0.5f));
        // The back at z 0 is one face from the bottom to the top, so no line crosses it at half height.
        Assert.False(Along(0.5f, 0));
        Assert.True(Along(1, 0) && Along(0, 0) && Along(1, 0.5f) && Along(0.5f, 1) && Along(0, 1));
        // A whole slab's outline is its box's twelve edges.
        Assert.Equal(12, Blocks.Get(BlockId.StoneSlab).Shape!.Edges.Count);
    }

    [Fact]
    public void A_Fence_Joins_The_Fence_And_The_Block_Beside_It_And_Not_The_Air()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.OakFence);
        world.SetBlock(8, 20, 7, BlockId.OakFence);
        world.SetBlock(9, 20, 8, BlockId.Stone);

        // North 1 and east 2, and the post's two rails each way.
        var shape = world.ShapeAt(8, 20, 8)!;
        Assert.Same(Blocks.Fence(1 | 2), shape);
        Assert.Equal(1 + 2 * 2, shape.Pieces.Count);
        Assert.Equal(0, shape.Min.Z, 4);
        Assert.Equal(1, shape.Max.X, 4);
    }

    [Fact]
    public void A_Door_Opens_Both_Its_Halves_And_Breaking_One_Breaks_The_Other()
    {
        var world = Floor();
        world.SetBlock(8, 19, 8, BlockId.Stone);
        world.SetBlock(8, 20, 8, BlockId.OakDoorEast);
        world.SetBlock(8, 21, 8, BlockId.OakDoorEastTop);

        Assert.True(world.Toggle(8, 21, 8));
        Assert.Equal(BlockId.OakDoorEastOpen, world.GetBlock(8, 20, 8));
        Assert.Equal(BlockId.OakDoorEastTopOpen, world.GetBlock(8, 21, 8));
        Assert.True(world.Toggle(8, 20, 8));
        Assert.Equal(BlockId.OakDoorEast, world.GetBlock(8, 20, 8));

        world.SetBlock(8, 21, 8, BlockId.Air);
        Assert.Equal(BlockId.Air, world.GetBlock(8, 20, 8));
        Assert.Equal(BlockId.Stone, world.GetBlock(8, 19, 8));
    }

    [Fact]
    public void A_Shut_Door_Lies_Across_The_Way_It_Faces_And_An_Open_One_Along_It()
    {
        // Facing east, shut across the cell's west side, and open along the north side to its left.
        var shut = Blocks.Get(BlockId.OakDoorEast).Shape!;
        var open = Blocks.Get(BlockId.OakDoorEastOpen).Shape!;
        Assert.Equal((0f, 3 / 16f, 1f), (shut.Min.X, shut.Max.X, shut.Max.Z - shut.Min.Z));
        Assert.Equal((0f, 3 / 16f, 1f), (open.Min.Z, open.Max.Z, open.Max.X - open.Min.X));
    }
}
