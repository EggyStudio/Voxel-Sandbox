using System.Numerics;
using System.Runtime.InteropServices;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>Draws the stacks lying in the world, each item one small mesh drawn instanced, turning slowly and bobbing.</summary>
/// <remarks>
/// A block's stack is a cube a quarter of a block a side wearing its top's, sides' and bottom's
/// pictures as the packs give the terrain them, tinted as in the plains as the block's icon is, or in
/// their colors where no pack gives one, and an item whose icon is flat, the pack's picture of it or
/// the game's own drawing, that picture a pixel deep as Minecraft's item models make it, a cube for
/// each pixel shown whose faces meet at shared corners (<see cref="Extrude"/>). A block of a shape of
/// its own that no pack draws flat stays a small box of its color,
/// but for one whose item is its whole cube, as the cactus's is, which wears its pictures as a block's.
/// They cast no shadow, which keeps them out of the scene's distance field as well, where each, moving
/// every frame, would be stamped as a box.
/// </remarks>
public sealed class DropRenderer : IDisposable
{
    private const float Distance = 48;

    private readonly ModelMaterial _material = new(Color.White) { AlphaMode = MaterialAlphaMode.Opaque, Roughness = 0.8f, CastsShadows = false };
    private readonly ModelMesh?[] _meshes = new ModelMesh?[Items.All.Count];
    // Each item's material where it wears a picture, holding a texture of its own, or null where it
    // is drawn in its colors.
    private readonly ModelMaterial?[] _materials = new ModelMaterial?[Items.All.Count];
    private readonly List<Texture2D> _textures = [];
    private readonly List<Matrix4x4>[] _draws = [.. Items.All.Select(_ => new List<Matrix4x4>())];
    private TerrainAtlas _terrain = TerrainAtlas.None;

    /// <summary>Instanced draws the last <see cref="Draw"/> made.</summary>
    public int Draws { get; private set; }

    /// <summary>Draws the stacks with the pictures of the packs' terrain and icons from the next frame, as the packs change.</summary>
    public void Use(TerrainAtlas terrain)
    {
        Forget();
        _terrain = terrain;
    }

    /// <summary>
    /// Draws the stacks within reach of the eye at a time in seconds, which turns them, and the
    /// whole blocks moving, falling sand or burning TNT, each its block's stack four times its size
    /// and the size given over that, standing on its feet. Called inside <c>BeginMode3D</c>.
    /// </summary>
    public void Draw(IEnumerable<(ItemDrop Drop, Body Body)> drops, IEnumerable<(BlockId Block, Vector3 Feet, float Size)> blocks, Vector3 eye, double time)
    {
        foreach (var list in _draws) list.Clear();
        foreach (var (block, feet, size) in blocks)
            if (Vector3.DistanceSquared(feet, eye) <= Distance * Distance)
                _draws[Items.Of(block).Id].Add(Matrix4x4.CreateScale(4 * size) * Matrix4x4.CreateTranslation(feet + new Vector3(0, 0.5f, 0)));
        foreach (var (drop, body) in drops)
        {
            if (Vector3.DistanceSquared(body.Position, eye) > Distance * Distance) continue;
            // Each stack turns and bobs out of step with the others, by its own age.
            var spin = (float)time * 1.2f + drop.Delay * 7 + body.Position.X * 3;
            var bob = 0.06f * (1 + MathF.Sin((float)time * 2.5f + body.Position.Z * 5));
            _draws[drop.Item.Id].Add(Matrix4x4.CreateRotationY(spin) * Matrix4x4.CreateTranslation(body.Position + new Vector3(0, 0.125f + bob, 0)));
        }
        var draws = 0;
        for (int i = 0; i < _draws.Length; i++)
        {
            if (_draws[i].Count == 0) continue;
            if (_meshes[i] is not { } mesh) (mesh, _materials[i]) = Build(Items.All[i]);
            _meshes[i] = mesh;
            DrawMeshInstanced(mesh, _materials[i] ?? _material, CollectionsMarshal.AsSpan(_draws[i]));
            draws++;
        }
        Draws = draws;
    }

    // A cube for a block, wearing its faces' pictures where a pack gives any, a flat square for an
    // item drawn flat, and a small box of its color for the rest.
    private (ModelMesh, ModelMaterial?) Build(Item item)
    {
        var mesh = new BoxMesh();
        if (item.Block is { } id && Blocks.Get(id) is { Shape: null or { Whole: true } } block)
        {
            if (_terrain.Tile(block.Top) != 0 || _terrain.Tile(block.Side) != 0 || _terrain.Tile(block.Bottom) != 0)
            {
                // The side's picture, the top's and the bottom's side by side, each face its third.
                (Vector2, Vector2) sides = (Vector2.Zero, new(1 / 3f, 1)), up = (new(1 / 3f, 0), new(2 / 3f, 1)), down = (new(2 / 3f, 0), Vector2.One);
                mesh.Add(new Vector3(-0.125f), new Vector3(0.125f), Color.White, [sides, sides, up, down, sides, sides]);
                return (mesh.Upload(), Pictured(Faces(block), 3 * _terrain.Pixels, _terrain.Pixels, doubleSided: false));
            }
            Color top = Surfaces.All[block.Top].Plain, side = Surfaces.All[block.Side].Plain, bottom = Surfaces.All[block.Bottom].Plain;
            mesh.Add(new Vector3(-0.125f), new Vector3(0.125f), [side, side, top, bottom, side, side]);
            return (mesh.Upload(), null);
        }
        if (Icons.IsFlat(item))
        {
            var icon = Icons.Pixels(item);
            Extrude(mesh, icon, Icons.Size, 0.15f);
            return (mesh.Upload(), Pictured(icon, Icons.Size, Icons.Size, doubleSided: false));
        }
        mesh.Add(new Vector3(-0.1f, -0.15f, -0.1f), new Vector3(0.1f, 0.15f, 0.1f), item.Color);
        return (mesh.Upload(), null);
    }

    /// <summary>The mesh and the material a stack of an item lies on the ground with, a block's cube a quarter of a block across or an item's picture a little over that, made the first time it is asked for, which the held item takes too.</summary>
    public (ModelMesh Mesh, ModelMaterial Material, bool Flat) Model(Item item)
    {
        if (_meshes[item.Id] is not { } mesh) (mesh, _materials[item.Id]) = Build(item);
        _meshes[item.Id] = mesh;
        return (mesh, _materials[item.Id] ?? _material, Icons.IsFlat(item));
    }

    /// <summary>
    /// Adds a picture of <paramref name="n"/> pixels square, <paramref name="half"/> across from its
    /// middle to each side, as Minecraft's item models make one, a pixel deep, each pixel shown a cube
    /// of its own with its front, its back and a side along each edge between it and a pixel clear or
    /// the picture's edge.
    /// </summary>
    /// <remarks>
    /// Every corner is taken from one grid of the pixels' corners, so faces that meet share their
    /// corners bit for bit and the rasterizer leaves no crack between them as the view moves, where a
    /// front and a back of the whole picture cut out where it is clear met the sides a little off
    /// their edges and the sky showed through along the outline. Each face shows its pixel's middle on
    /// the picture, so its color is the pixel's own at any size. A run of pixels is not joined into
    /// one face, since its long edge would meet the shorter edges of the sides along it at corners it
    /// does not hold, which crack as edges of different corners do.
    /// </remarks>
    public static void Extrude(BoxMesh mesh, byte[] icon, int n, float half)
    {
        float pixel = 2 * half / n, deep = pixel / 2;
        var front = new Vector3[n + 1, n + 1];
        var back = new Vector3[n + 1, n + 1];
        for (int y = 0; y <= n; y++)
            for (int x = 0; x <= n; x++)
            {
                float across = -half + x * pixel, up = half - y * pixel;
                (front[x, y], back[x, y]) = (new Vector3(across, up, deep), new Vector3(across, up, -deep));
            }
        bool Shown(int x, int y) => x >= 0 && y >= 0 && x < n && y < n && icon[(y * n + x) * 4 + 3] >= 128;
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                if (!Shown(x, y)) continue;
                var middle = new Vector2((x + 0.5f) / n, (y + 0.5f) / n);
                ReadOnlySpan<Vector2> places = [middle, middle, middle, middle];
                mesh.AddFace([front[x, y + 1], front[x + 1, y + 1], front[x + 1, y], front[x, y]], places, Color.White);
                mesh.AddFace([back[x, y + 1], back[x, y], back[x + 1, y], back[x + 1, y + 1]], places, Color.White);
                if (!Shown(x - 1, y)) mesh.AddFace([back[x, y + 1], front[x, y + 1], front[x, y], back[x, y]], places, Color.White);
                if (!Shown(x + 1, y)) mesh.AddFace([back[x + 1, y + 1], back[x + 1, y], front[x + 1, y], front[x + 1, y + 1]], places, Color.White);
                if (!Shown(x, y - 1)) mesh.AddFace([back[x, y], front[x, y], front[x + 1, y], back[x + 1, y]], places, Color.White);
                if (!Shown(x, y + 1)) mesh.AddFace([back[x, y + 1], back[x + 1, y + 1], front[x + 1, y + 1], front[x, y + 1]], places, Color.White);
            }
    }

    // A block's side, top and bottom pictures side by side, each texel as the block's icon takes it,
    // tinted as in the plains, and a face no pack gives a picture its surface's own color.
    private byte[] Faces(BlockInfo block)
    {
        var n = _terrain.Pixels;
        var pixels = new byte[3 * n * n * 4];
        int[] surfaces = [block.Side, block.Top, block.Bottom];
        for (int part = 0; part < 3; part++)
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    var texel = _terrain.Texel(surfaces[part], (x + 0.5f) / n, (y + 0.5f) / n, Biomes.Plains.GrassColor, Biomes.Plains.FoliageColor)
                        ?? Surfaces.All[surfaces[part]].Plain;
                    var at = (y * 3 * n + part * n + x) * 4;
                    (pixels[at], pixels[at + 1], pixels[at + 2], pixels[at + 3]) = (texel.R, texel.G, texel.B, texel.A);
                }
        return pixels;
    }

    // A material wearing a picture, sampled without filtering so its pixels stay square, and cut out
    // where the picture is clear.
    private ModelMaterial Pictured(byte[] pixels, int width, int height, bool doubleSided)
    {
        var texture = LoadTextureFromImage(new Image(pixels, width, height));
        SetTextureFilter(texture, TextureFilter.Point);
        _textures.Add(texture);
        return new ModelMaterial(Color.White, texture) { AlphaMode = MaterialAlphaMode.Mask, Roughness = 0.8f, CastsShadows = false, DoubleSided = doubleSided };
    }

    private void Forget()
    {
        for (int i = 0; i < _meshes.Length; i++)
        {
            if (_meshes[i] is { } mesh) UnloadMesh(mesh);
            (_meshes[i], _materials[i]) = (null, null);
        }
        foreach (var texture in _textures) UnloadTexture(texture);
        _textures.Clear();
    }

    public void Dispose() => Forget();
}
