using Engine;
using Xunit;
using static Engine.E3D;

namespace VoxelSandbox.Tests;

/// <summary>A pack's animated textures, their frames laid in the atlas and stepped by the game's ticks.</summary>
public sealed class AnimationTests : IDisposable
{
    private readonly string _folder = Directory.CreateTempSubdirectory("voxel-animation-").FullName;

    public void Dispose() => Directory.Delete(_folder, true);

    private static readonly Color[] Colors = [new(200, 0, 0), new(0, 200, 0), new(0, 0, 200)];

    // A strip of three frames two pixels square, one above another, red, green and blue.
    private static Image Strip() => new([.. Enumerable.Range(0, 2 * 6).SelectMany(i => new[] { Colors[i / 4].R, Colors[i / 4].G, Colors[i / 4].B, (byte)255 })], 2, 6);

    // A pack as a folder of one texture and its metadata.
    private ResourcePacks Pack(string texture, Image image, string? meta)
    {
        var path = Path.Combine(_folder, "pack", "assets", "minecraft", "textures", texture + ".png");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, ExportImageToMemory(image, ".png"));
        if (meta is not null) File.WriteAllText(path + ".mcmeta", meta);
        return new ResourcePacks([Path.Combine(_folder, "pack")]);
    }

    private static Color First(Image image) => new(image.Data[0], image.Data[1], image.Data[2]);

    [Fact]
    public void A_Strip_Plays_Its_Frames_In_The_Order_And_For_The_Times_Its_Metadata_Gives()
    {
        using var packs = Pack("block/water_still", Strip(), """{"animation":{"frametime":2,"frames":[0,2,{"index":1,"time":4},7]}}""");
        var (frames, ticks) = packs.Animation("block/water_still")!.Value;
        // The frame past the strip's three left out.
        Assert.Equal(new[] { Colors[0], Colors[2], Colors[1] }, frames.Select(First));
        Assert.Equal(new[] { 2, 2, 4 }, ticks);
        // The picture is the first frame.
        Assert.Equal(Colors[0], First(packs.Texture("block/water_still")!.Value));
    }

    [Fact]
    public void The_Atlas_Lays_An_Animations_Steps_One_After_Another_And_Steps_Them_By_The_Tick()
    {
        using var packs = Pack("block/water_still", Strip(), """{"animation":{"frametime":2,"frames":[0,2,{"index":1,"time":4}]}}""");
        var atlas = TerrainAtlas.Build(packs);
        // The red held two ticks, the blue two and the green four, in steps of two ticks, the green taking two.
        var (first, count, held) = Assert.Single(atlas.Animations);
        Assert.Equal((atlas.Tile(Surfaces.Water), 4, 2), (first, count, held));
        var (steps, ticks) = atlas.Animated(Surfaces.Water)!.Value;
        Assert.Equal(new[] { Colors[0], Colors[2], Colors[1], Colors[1] }, steps.Select(First));
        Assert.Equal(2, ticks);
        // At five ticks the third step, the green, and at nine back to the red.
        Assert.Equal(2, TerrainAtlas.Step(count, held, 5));
        Assert.Equal(0, TerrainAtlas.Step(count, held, 9));
    }

    [Fact]
    public void A_Texture_With_No_Animation_Or_One_Frame_Is_Still()
    {
        using var still = Pack("block/water_still", Strip(), null);
        Assert.Null(still.Animation("block/water_still"));
        Assert.Empty(TerrainAtlas.Build(still).Animations);
        Assert.Null(TerrainAtlas.Build(still).Animated(Surfaces.Water));
        using var one = Pack("block/water_still", Strip(), """{"animation":{"frames":[1,1]}}""");
        Assert.Null(one.Animation("block/water_still"));
    }
}
