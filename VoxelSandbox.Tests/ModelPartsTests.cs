using System.Numerics;
using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The parts of their textures a lantern's and a torch's pieces show, as the source's models give them.</summary>
public class ModelPartsTests
{
    // A floor of bedrock at 0, the rest open sky.
    private static VoxelWorld Floor() => Laid.World(c => c.Set(0, 0, 0, BlockId.Bedrock));

    private static float Linear(byte value)
    {
        var c = value / 255f;
        return c <= 0.04045f ? c / 12.92f : MathF.Pow((c + 0.055f) / 1.055f, 2.4f);
    }

    // The corners of faces whose place on the texture each carries.
    private static IEnumerable<(Vector3 Position, int Face, Vector2 Place)> Placed(SectionMesher.Faces faces) =>
        Enumerable.Range(0, faces.Count).Select(i => (faces.Positions[i], faces.Unpack(i).Face, SectionMesher.PlaceOf(faces.Packed[2 * i])!.Value));

    [Fact]
    public void A_Lanterns_Cap_And_Handle_Show_The_Parts_Of_The_Lantern_Texture_The_Source_Gives_Them()
    {
        var world = Floor();
        world.SetBlock(8, 20, 8, BlockId.Lantern);
        var mesher = new SectionMesher();
        Assert.True(mesher.Mesh(world, new SectionKey(0, 1, 0)));

        // The cap from 6 to 10 across and 7 to 9 up, its top showing columns 1 to 5 of rows 10 to 14
        // and its sides rows 0 and 1, the glass a lamp's cube apart.
        var cap = Placed(mesher.Solid).ToList();
        Assert.Equal(6 * 4, cap.Count);
        Assert.Equal(new Vector2(1, 10), cap.Single(c => c.Face == 2 && c.Position == new Vector3(8 + 6 / 16f, 4 + 9 / 16f, 8 + 6 / 16f)).Place);
        Assert.Equal(new Vector2(5, 14), cap.Single(c => c.Face == 2 && c.Position == new Vector3(8 + 10 / 16f, 4 + 9 / 16f, 8 + 10 / 16f)).Place);
        Assert.Equal(new Vector2(5, 0), cap.Single(c => c.Face == 0 && c.Position == new Vector3(8 + 10 / 16f, 4 + 9 / 16f, 8 + 6 / 16f)).Place);
        Assert.Equal(new Vector2(1, 2), cap.Single(c => c.Face == 0 && c.Position == new Vector3(8 + 10 / 16f, 4 + 7 / 16f, 8 + 10 / 16f)).Place);

        // The handle's two crossed walls, both sides of each cut out, showing columns 11 to 14 of
        // rows 1 to 3 and of rows 10 to 12.
        var handle = Placed(mesher.Cutout).ToList();
        Assert.Equal(2 * 2 * 4, handle.Count);
        Assert.All(handle, c => Assert.Contains(c.Place.X, new[] { 11f, 14f }));
        Assert.Equal(new[] { 1f, 3f, 10f, 12f }, handle.Select(c => c.Place.Y).Distinct().Order());
        Assert.Equal(BlockId.Lantern, Assert.Single(mesher.Emitters).Block);
    }

    [Fact]
    public void A_Wall_Torchs_Leaning_Stick_Shows_The_Torchs_Stick_Whichever_Way_It_Faces()
    {
        foreach (var torch in new[] { BlockId.WallTorchNorth, BlockId.WallTorchSouth, BlockId.WallTorchWest, BlockId.WallTorchEast })
        {
            var world = Floor();
            world.SetBlock(8, 20, 8, torch);
            var mesher = new SectionMesher();
            Assert.True(mesher.Mesh(world, new SectionKey(0, 1, 0)));
            var stick = Placed(mesher.Solid).ToList();
            Assert.Equal(6 * 4, stick.Count);
            // Columns 7 and 8, its sides rows 9 to 15, its top rows 6 and 7 and its bottom rows 13 and 14.
            Assert.All(stick, c => Assert.Contains(c.Place.X, new[] { 7f, 9f }));
            Assert.All(stick.Where(c => c.Face is not 2 and not 3), c => Assert.Contains(c.Place.Y, new[] { 9f, 16f }));
            Assert.All(stick.Where(c => c.Face == 2), c => Assert.Contains(c.Place.Y, new[] { 6f, 8f }));
            Assert.All(stick.Where(c => c.Face == 3), c => Assert.Contains(c.Place.Y, new[] { 13f, 15f }));
        }
    }

    [Fact]
    public void A_Faces_Place_Runs_Across_It_From_Its_Left_And_Down_From_Its_Top_Seen_From_Outside()
    {
        var parts = new Parts(new Vector4(0, 2, 6, 9), new Vector4(0, 9, 6, 15), default);
        // Seen from the north, outside the face toward -z, its left is toward +x.
        Assert.Equal(new Vector2(0, 2), SectionMesher.Place(parts, 5, new Vector3(1, 1, 0)));
        Assert.Equal(new Vector2(6, 9), SectionMesher.Place(parts, 5, new Vector3(0, 0, 0)));
        // Seen from the east its left is toward +z, and from above the top's rows run toward +z.
        Assert.Equal(new Vector2(0, 2), SectionMesher.Place(parts, 0, new Vector3(1, 1, 1)));
        Assert.Equal(new Vector2(6, 15), SectionMesher.Place(parts, 2, new Vector3(1, 1, 1)));
        // A face laid by where it lies in its block carries no place.
        Assert.Null(SectionMesher.PlaceOf(SectionMesher.Pack(2, 0, 1, Color.White, 5).First));
    }

    [Fact]
    public void Every_Lit_Piece_Of_A_Surface_Shows_The_Same_Parts_So_One_Cube_Draws_Them_All()
    {
        var lit = Blocks.All.SelectMany(b => b.Shape?.Pieces ?? []).Where(p => p.Glows).GroupBy(p => p.Surface);
        foreach (var surface in lit) Assert.Single(surface.Select(p => p.Parts).Distinct());
        Assert.Equal(new Vector4(0, 2, 6, 9), Blocks.Get(BlockId.SoulHangingLantern).Shape!.Pieces.Single(p => p.Glows).Parts!.Value.Sides);
        Assert.Equal(new Vector4(7, 6, 9, 9), Blocks.Get(BlockId.WallTorchEast).Shape!.Pieces.Single(p => p.Glows).Parts!.Value.Sides);
    }

    [Fact]
    public void A_Lanterns_Emission_Shows_Its_Flames_Pixels_At_Four_Times_Their_Color_And_Leaves_Its_Iron_Dark()
    {
        // A pixel of the lantern's dark iron and one of its flame's yellow.
        Color iron = new(69, 85, 101), flame = new(255, 200, 98);
        var picture = new Image([iron.R, iron.G, iron.B, 255, flame.R, flame.G, flame.B, 255], 2, 1);
        var glass = Surfaces.All[Surfaces.LanternGlass];
        var map = ChunkRenderer.EmissionMap(picture, glass).Data;
        // The emission the face shows, the map's times the material's.
        float Shown(int at, byte emissive) => Linear(map[at]) * Linear(emissive) * glass.Glow;

        Assert.All(new[] { Shown(0, glass.Emissive.R), Shown(1, glass.Emissive.G), Shown(2, glass.Emissive.B) }, e => Assert.True(e < 0.01f));
        var expected = new[] { Linear(flame.R), Linear(flame.G), Linear(flame.B) }.Select(c => c * ChunkRenderer.LampShown).ToArray();
        var shown = new[] { Shown(4, glass.Emissive.R), Shown(5, glass.Emissive.G), Shown(6, glass.Emissive.B) };
        for (int c = 0; c < 3; c++) Assert.InRange(shown[c] / expected[c], 0.97f, 1.03f);
    }

    [Fact]
    public void A_Lanterns_Iron_With_No_Pack_Shows_Its_Handles_Arch_And_A_Whole_Cap()
    {
        var atlas = TerrainAtlas.None;
        Color Texel(int x, int y) => atlas.Texel(Surfaces.LanternIron, (x + 0.5f) / 16, (y + 0.5f) / 16, Color.White, Color.White)!.Value;
        Assert.Equal(0, Texel(12, 2).A);
        Assert.Equal(255, Texel(11, 2).A);
        Assert.Equal(Surfaces.All[Surfaces.LanternIron].Color, Texel(2, 0));
        Assert.Equal(atlas.Tile(Surfaces.LanternIron), atlas.Tile(Surfaces.SoulLanternIron));
    }
}
