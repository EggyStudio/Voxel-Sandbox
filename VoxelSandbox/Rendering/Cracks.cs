using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The cracks drawn over a block as it is broken, ten stages from the first hit to the break, the
/// pack's destroy stages where a pack has them and the game's own where none does.
/// </summary>
/// <remarks>
/// Each stage is laid on a cube a hair larger than each box of the block, blended over it and lit
/// as the block is, so a pack's black cracks show black and a slab's cracks cover the slab alone.
/// The game's own cracks are branches walking out from about the block's middle across its sixteen
/// pixels, each stage showing a step more of every branch, so they grow as the block breaks.
/// </remarks>
public sealed class Cracks : IDisposable
{
    /// <summary>How many stages the cracks go through, as Minecraft's ten destroy stages.</summary>
    public const int Stages = 10;

    private const int Size = 16;
    private const int Branches = 8;

    // The eight ways around, which a branch of the game's own cracks starts along and turns from by
    // one at most a step.
    private static readonly (int X, int Y)[] Ways = [(1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1), (0, -1), (1, -1)];

    private readonly ModelMaterial?[] _materials = new ModelMaterial?[Stages];
    private readonly List<Texture2D> _textures = [];
    private ResourcePacks _packs = ResourcePacks.None;
    private ModelMesh? _cube;

    /// <summary>Draws the cracks with the packs' destroy stages from the next frame, as the packs change.</summary>
    public void Use(ResourcePacks packs)
    {
        Forget();
        _packs = packs;
    }

    /// <summary>The stage the cracks stand at for a block broken so far, from 0 to 1.</summary>
    public static int Stage(float progress) => Math.Clamp((int)(progress * Stages), 0, Stages - 1);

    /// <summary>A stage's picture, the pack's <c>destroy_stage</c> of it, or the game's own where no pack has one.</summary>
    public static Image Picture(ResourcePacks packs, int stage) => packs.Texture($"block/destroy_stage_{stage}") ?? Own(stage);

    /// <summary>
    /// The pixels of the game's own cracks at a stage, across and down the block's face, each
    /// branch's first steps, one more at each stage, so a stage holds every pixel of the one before.
    /// </summary>
    public static IEnumerable<(int X, int Y)> OwnPixels(int stage)
    {
        var seen = new HashSet<(int, int)>();
        for (int branch = 0; branch < Branches; branch++)
        {
            int x = 7 + (int)(PlaceHash.Of(branch, 0, 41) % 2), y = 7 + (int)(PlaceHash.Of(branch, 1, 41) % 2), way = branch;
            for (int step = 0; step <= stage; step++)
            {
                if (seen.Add((x, y))) yield return (x, y);
                way = (way + (int)(PlaceHash.Of(branch, step, 43) % 3) - 1 + Branches) % Branches;
                (x, y) = (Math.Clamp(x + Ways[way].X, 0, Size - 1), Math.Clamp(y + Ways[way].Y, 0, Size - 1));
            }
        }
    }

    // A stage of the game's own cracks, a dark brown nearly opaque, clear elsewhere.
    private static Image Own(int stage)
    {
        var pixels = new byte[Size * Size * 4];
        foreach (var (x, y) in OwnPixels(stage))
            (pixels[(y * Size + x) * 4], pixels[(y * Size + x) * 4 + 1], pixels[(y * Size + x) * 4 + 2], pixels[(y * Size + x) * 4 + 3]) = (30, 24, 20, 230);
        return new Image(pixels, Size, Size);
    }

    /// <summary>Draws the cracks of a block broken so far, from 0 to 1, over each of its boxes. Called inside <c>BeginMode3D</c>.</summary>
    public void Draw(BlockHit hit, BlockShape? shape, float progress)
    {
        var stage = Stage(progress);
        var material = _materials[stage] ??= Load(stage);
        var cube = _cube ??= UnitCube();
        foreach (var box in shape?.Boxes ?? [Box.Cell])
        {
            var size = box.Max - box.Min + new Vector3(0.006f);
            DrawMesh(cube, material, Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(new Vector3(hit.X, hit.Y, hit.Z) + box.Min - new Vector3(0.003f)));
        }
    }

    private ModelMaterial Load(int stage)
    {
        var texture = LoadTextureFromImage(Picture(_packs, stage));
        SetTextureFilter(texture, TextureFilter.Point);
        _textures.Add(texture);
        return new ModelMaterial(Color.White, texture) { AlphaMode = MaterialAlphaMode.Blend, Roughness = 1, CastsShadows = false, BouncesLight = false };
    }

    // A cube from 0 to 1, its every face showing the whole picture as the terrain lays one on a block.
    private static ModelMesh UnitCube()
    {
        var mesh = new BoxMesh();
        (Vector2, Vector2) whole = (Vector2.Zero, Vector2.One);
        mesh.Add(Vector3.Zero, Vector3.One, Color.White, [whole, whole, whole, whole, whole, whole]);
        return mesh.Upload();
    }

    private void Forget()
    {
        foreach (var texture in _textures) UnloadTexture(texture);
        _textures.Clear();
        Array.Clear(_materials);
    }

    public void Dispose()
    {
        Forget();
        if (_cube is { } cube) UnloadMesh(cube);
        _cube = null;
    }
}
