using System.Numerics;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The tests that set the <see cref="LandOrigin"/>, which every place of the game is kept against, and so run alone.</summary>
[CollectionDefinition(nameof(LandOrigins), DisableParallelization = true)]
public sealed class LandOrigins;

/// <summary>The game's places kept against an origin far from the land's own, as a million blocks out.</summary>
[Collection(nameof(LandOrigins))]
public sealed class LandOriginTests : IDisposable
{
    // A block a million blocks out, where a float steps by an eighth of a block, the scene of the
    // far reads of 2026-11-11 placed there.
    private const int FarX = 1_139_984, FarZ = 592;

    public LandOriginTests() => LandOrigin.Near(FarX, FarZ);

    public void Dispose() => LandOrigin.Near(0, 0);

    // A floor of stone with its top at 11 about the far block.
    private static VoxelWorld Floor() => Laid.World(c => Laid.Fill(c, 0, 10, BlockId.Stone), 2, FarX >> Section.Shift, FarZ >> Section.Shift);

    [Fact]
    public void The_Origin_Is_The_Multiple_Of_1024_Nearest_The_Place()
    {
        Assert.Equal((1_139_712, 1024), (LandOrigin.X, LandOrigin.Z));
        LandOrigin.Near(-1_600, 511);
        Assert.Equal((-2048, 0), (LandOrigin.X, LandOrigin.Z));
    }

    [Fact]
    public void A_Place_Of_The_Land_Far_Out_Turns_Into_A_Float_Near_Zero_And_Back_Whole()
    {
        var land = new LandPlace(FarX + 8.5, 121, FarZ + 4.125);
        var place = LandOrigin.PlaceOf(land);

        Assert.Equal(new Vector3(280.5f, 121, -427.875f), place);
        Assert.Equal(land, LandOrigin.LandOf(place));
        Assert.Equal((FarX + 8, 121, FarZ + 4), LandOrigin.BlockOf(place));
        Assert.Equal(new Vector3(280, 121, -428), LandOrigin.PlaceOf(FarX + 8, 121, FarZ + 4));
        Assert.Equal(LandOrigin.PlaceOf(FarX & ~Section.Mask, 112, FarZ & ~Section.Mask), LandOrigin.PlaceOf(new SectionKey(FarX >> Section.Shift, 7, FarZ >> Section.Shift)));
    }

    [Fact]
    public void A_Body_Far_Out_Walking_Into_A_Wall_Stops_With_Its_Side_On_The_Walls_Face()
    {
        var world = Floor();
        world.SetBlock(FarX + 7, 11, FarZ + 4, BlockId.Stone);
        world.SetBlock(FarX + 7, 12, FarZ + 4, BlockId.Stone);
        var body = new Body { Position = LandOrigin.PlaceOf(new LandPlace(FarX + 4.5, 11, FarZ + 4.5)) };

        body.Move(world, new Vector3(5, 0, 0), keepToEdges: false);

        Assert.Equal(FarX + 7 - (double)body.HalfWidth, LandOrigin.LandOf(body.Position).X, 4);
        Assert.True(body.OnGround);
    }

    [Fact]
    public void A_Body_Far_Out_Walks_As_Far_Along_Each_Axis_As_Near_The_Origin()
    {
        var world = Floor();
        foreach (var way in (Vector3[])[Vector3.UnitX, Vector3.UnitZ, -Vector3.UnitX, -Vector3.UnitZ])
        {
            var start = LandOrigin.PlaceOf(new LandPlace(FarX + 8.5, 11, FarZ + 8.5));
            var body = new Body { Position = start };
            // Sixty frames of a walk of 4.3 blocks a second, as the player's, which at 284,688 blocks
            // in the land's own floats went 13 percent short along x and a million out in eighths.
            for (int frame = 0; frame < 60; frame++) body.Move(world, way * 4.317f / 60, keepToEdges: false);
            Assert.Equal(4.317f, Vector3.Distance(start, body.Position), 2);
        }
    }

    [Fact]
    public void The_Crosshair_Far_Out_Picks_The_Block_A_Fiftieth_From_An_Edge()
    {
        var world = Floor();
        for (int x = 0; x < 8; x++)
            for (int y = 11; y < 15; y++)
                world.SetBlock(FarX + x, y, FarZ, BlockId.Stone);
        var eye = LandOrigin.PlaceOf(new LandPlace(FarX + 4.5, 12.62, FarZ + 3.5));
        foreach (var (x, y) in ((double X, double Y)[])[(1.02, 12.5), (1.98, 13.02), (4.5, 13.98), (5.02, 14.02), (6.98, 11.98)])
        {
            var point = LandOrigin.PlaceOf(new LandPlace(FarX + x, y, FarZ + 1));
            var hit = VoxelRay.Cast(world, eye, point - eye, 5);
            Assert.Equal((FarX + (int)Math.Floor(x), (int)Math.Floor(y), FarZ), (hit?.X, hit?.Y, hit?.Z));
        }
    }

    [Fact]
    public void A_Mob_Far_Out_Keeps_Its_Place_And_Home_In_Its_Record()
    {
        var record = new MobRecord(MobKinds.Villager, 2, new LandPlace(FarX + 3.25, 64, FarZ - 7.75), 0, 13, new LandPlace(FarX + 5.5, 64, FarZ - 2.5));
        var mob = new Mob(1, record, 1);

        Assert.Equal(LandOrigin.PlaceOf(record.Position), mob.Body.Position);
        Assert.Equal(record, mob.Record);
        Assert.Equal(LandOrigin.ColumnOf(mob.Body.Position), ((FarX + 3) >> Section.Shift, (FarZ - 8) >> Section.Shift));
    }
}
