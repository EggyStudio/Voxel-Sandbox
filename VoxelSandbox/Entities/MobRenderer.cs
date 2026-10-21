using System.Numerics;
using System.Runtime.InteropServices;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>Draws the mobs, each piece of each kind's model one mesh, and every copy of a mesh one instanced draw.</summary>
/// <remarks>
/// <para>
/// A piece's boxes are one mesh with a color at each vertex, built facing north, so a mob is drawn
/// by turning each piece about its pivot by the head's look, the legs' stride or a wing's flap, and
/// the whole by its yaw. A piece that takes its mob's own color, a sheep's wool or a villager's
/// robe, is meshed once for each variant. A mob hit, or dying, is drawn through a red material for
/// the half second it stays red, and one dying falls onto its side, as Minecraft's do.
/// </para>
/// <para>
/// Mobs are drawn within 64 blocks of the eye, as Minecraft draws its entities, through the game's
/// mob shader, which has none of the haze the sections fade into and takes the color out of the
/// light from all around them, as the README's section on mobs says. They cast shadows and hold
/// no light for others, their materials' <see cref="ModelMaterial.BouncesLight"/>
/// off, which leaves them out of the scene's distance field, where each piece that moves would be
/// stamped as a box every frame, milliseconds a frame for a herd walking in sight. A pig's pink so
/// does not reach the grass.
/// </para>
/// </remarks>
public sealed class MobRenderer : IDisposable
{
    private const float Distance = 64;

    // A piece of a kind in a variant, its layers each a mesh and the materials it is drawn with
    // plain, hurt and flashing, a layer a picture, as a sheep's body and its wool are.
    private sealed class Slot(IReadOnlyList<(ModelMesh Mesh, Materials Materials)> layers)
    {
        public readonly IReadOnlyList<(ModelMesh Mesh, Materials Materials)> Layers = layers;
        public readonly List<Matrix4x4> Plain = [], Hurt = [], Flash = [];
    }

    private readonly record struct Materials(ModelMaterial Plain, ModelMaterial Hurt, ModelMaterial Flash);

    // The game's mob shader, which takes the color out of the light from all around a mob and
    // floors each face's light at Minecraft's flat shade of its top's (resources/shaders/mob.slang).
    private readonly Shader _shader;
    private readonly Materials _own;
    private readonly List<Slot> _slots = [];
    // The kinds' pictures from the packs loaded, which their materials draw with.
    private readonly List<Texture2D> _pictures = [];
    // Each kind's pieces' slots, by kind, piece and variant, a piece of one color holding one.
    private readonly Slot[][][] _byKind = new Slot[MobKinds.All.Length][][];

    public MobRenderer()
    {
        _shader = LoadShader("resources/shaders/mob.slang");
        // Its vertex stage keeps the positions, so each pass leaves out the mobs it cannot see.
        SetShaderKeepsPositions(_shader, true);
        var skin = new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Opaque, Roughness = 0.85f, BouncesLight = false, Shader = _shader };
        // A mob hit is drawn red, and a creeper's flash as its fuse burns white and a little emissive.
        _own = new Materials(skin, skin with { Color = new Color(255, 96, 96) }, skin with { Emissive = Color.White, EmissiveIntensity = 0.6f });
        Use(ResourcePacks.None);
    }

    /// <summary>
    /// Draws each kind with its pictures from the packs loaded, laid on the source's cuboids as the
    /// source lays them, and in the game's own colors a kind one of whose pictures no pack gives or
    /// is not of the size its cuboids are laid on.
    /// </summary>
    /// <remarks>
    /// A kind's picture is drawn cut out where it is clear, and a cuboid whose faces it leaves clear
    /// throughout, as most packs leave a zombie's hat, is left out of the mesh. A box of the game's
    /// own drawn with the picture, the skeleton's bow, takes a row of white laid below the picture,
    /// tinted by its color. A villager's clothes of its biome are laid over its body's picture where
    /// they are not clear, as Minecraft draws the layer of its type over the villager cut out, into
    /// one picture a biome, rather than drawn as a second layer on the same cuboids, whose faces
    /// would meet the first's at the same depth. A sheep's wool is a picture of its own on cuboids
    /// of its own, grown about the body's, tinted by the sheep's color.
    /// </remarks>
    public void Use(ResourcePacks packs)
    {
        Forget();
        var textures = new Dictionary<byte[], Materials>(ReferenceEqualityComparer.Instance);
        foreach (var kind in MobKinds.All)
        {
            _byKind[kind.Id] = new Slot[kind.Parts.Length][];
            var names = PackTextures.OfMob(kind.Name);
            Image?[] found = [.. names.Select(list => list.Select(packs.Texture).FirstOrDefault(p => p is { } image && Fits(image, kind.PictureSize)))];
            if (names.Count > 0 && found.All(p => p is not null))
            {
                // Each variant's pictures, its body's with any clothes over it.
                var pictures = Enumerable.Range(0, kind.Variants.Length).Select(v => found.Select((p, i) =>
                    i == 0 && PackTextures.OverMob(kind.Name, v) is { } over && packs.Texture(over) is { } clothes && Fits(clothes, kind.PictureSize) ? Over(p!.Value, clothes) : p!.Value).ToArray()).ToArray();
                var varies = pictures.Any(v => !ReferenceEquals(v[0].Data, pictures[0][0].Data));
                for (int p = 0; p < kind.Parts.Length; p++)
                {
                    var part = kind.Parts[p];
                    var count = varies || part.Boxes.Any(b => b.Source is { Tinted: true }) ? kind.Variants.Length : 1;
                    _byKind[kind.Id][p] = [.. Enumerable.Range(0, count).Select(v => Add(new Slot([.. Enumerable.Range(0, found.Length)
                        .Select(i => (Boxes: PicturedBoxes(part, pictures[v][i], kind.PictureSize, i, kind.Variants[v]), Picture: pictures[v][i]))
                        .Where(layer => layer.Boxes.Vertices.Count > 0)
                        .Select(layer => (layer.Boxes.Upload(), MaterialsOf(layer.Picture, kind.PictureSize)))])))];
                }
                continue;
            }
            for (int p = 0; p < kind.Parts.Length; p++)
            {
                var part = kind.Parts[p];
                _byKind[kind.Id][p] = [.. Enumerable.Range(0, part.Varies ? kind.Variants.Length : 1).Select(v => Add(new Slot([(Build(part, kind.Variants[v]), _own)])))];
            }
        }

        Slot Add(Slot slot)
        {
            _slots.Add(slot);
            return slot;
        }

        // A picture's materials, its texture made once however many pieces it is laid on.
        Materials MaterialsOf(Image picture, Vector2 size)
        {
            if (textures.TryGetValue(picture.Data, out var materials)) return materials;
            var texture = LoadTextureFromImage(Whitened(picture, size));
            SetTextureFilter(texture, TextureFilter.Point);
            _pictures.Add(texture);
            return textures[picture.Data] = new Materials(_own.Plain with { Texture = texture, AlphaMode = MaterialAlphaMode.Mask },
                _own.Hurt with { Texture = texture, AlphaMode = MaterialAlphaMode.Mask }, _own.Flash with { Texture = texture, AlphaMode = MaterialAlphaMode.Mask });
        }
    }

    /// <summary>
    /// A picture with another laid over it, the upper's pixels kept where they are not clear, a
    /// tenth of full alpha or more, as Minecraft cuts out a mob's layers, and the lower's elsewhere,
    /// the upper scaled to the lower's pixels where a pack draws them at another scale.
    /// </summary>
    public static Image Over(Image lower, Image upper)
    {
        var data = (byte[])lower.Data.Clone();
        for (int y = 0; y < lower.Height; y++)
            for (int x = 0; x < lower.Width; x++)
            {
                var from = (y * upper.Height / lower.Height * upper.Width + x * upper.Width / lower.Width) * 4;
                if (upper.Data[from + 3] >= 26) Array.Copy(upper.Data, from, data, (y * lower.Width + x) * 4, 4);
            }
        return new Image(data, lower.Width, lower.Height);
    }

    // Lets go of the slots' meshes and the pictures.
    private void Forget()
    {
        foreach (var slot in _slots)
            foreach (var (mesh, _) in slot.Layers) UnloadMesh(mesh);
        foreach (var texture in _pictures) UnloadTexture(texture);
        _slots.Clear();
        _pictures.Clear();
    }

    /// <summary>
    /// Whether a picture fits the layout a kind's cuboids are laid on, at a whole scale, as a pack
    /// drawn at twice the pixels is, and as tall as the layout or half as tall, as the pig's and the
    /// cow's were before Minecraft 1.21.5 doubled them on the same offsets.
    /// </summary>
    public static bool Fits(Image picture, Vector2 size)
    {
        if (picture.Width < size.X || picture.Width % (int)size.X != 0 || picture.Height % (picture.Width / (int)size.X) != 0) return false;
        var rows = Rows(picture, size);
        return rows == size.Y || rows * 2 == size.Y;
    }

    // How many of the layout's rows of pixels a picture holds.
    private static int Rows(Image picture, Vector2 size) => picture.Height / (picture.Width / (int)size.X);

    /// <summary>Instanced draws the last <see cref="Draw"/> made.</summary>
    public int Draws { get; private set; }

    /// <summary>Mobs the last <see cref="Draw"/> drew.</summary>
    public int Drawn { get; private set; }

    // A piece's boxes drawn in the game's own colors as one mesh in blocks, its own boxes in a variant's color.
    private static ModelMesh Build(ModelPart part, Color variant)
    {
        var mesh = new BoxMesh();
        foreach (var box in part.Boxes)
            if (box.Drawn != VoxelSandbox.Drawn.Pack) mesh.Add(box.Min / 16, box.Max / 16, box.Color ?? variant);
        return mesh.Upload();
    }

    /// <summary>
    /// A kind's picture with a row of white below it, at the picture's own scale, which the boxes of
    /// the game's own drawn with the picture take, so a texture coordinate of the picture's in pixels
    /// is its place over the layout's width and the picture's rows and one more.
    /// </summary>
    public static Image Whitened(Image picture, Vector2 size)
    {
        var scale = picture.Width / (int)size.X;
        var data = new byte[picture.Width * (picture.Height + scale) * 4];
        Array.Copy(picture.Data, data, picture.Data.Length);
        Array.Fill(data, (byte)255, picture.Data.Length, data.Length - picture.Data.Length);
        return new Image(data, picture.Width, picture.Height + scale);
    }

    /// <summary>
    /// A piece's boxes drawn with one of its kind's pictures in blocks, each of the source's cuboids
    /// laid with its part of that picture, one whose faces the picture leaves clear or
    /// does not reach left out, a tinted one in the variant's color, and with the body's picture each
    /// box of the game's own drawn with the picture, laid on the white row below it.
    /// </summary>
    public static BoxMesh PicturedBoxes(ModelPart part, Image picture, Vector2 size, int index = 0, Color variant = default)
    {
        var mesh = new BoxMesh();
        var scale = picture.Width / size.X;
        var rows = Rows(picture, size);
        var through = new Vector2(1 / size.X, 1f / (rows + 1));
        Span<Vector3> corners = stackalloc Vector3[4];
        Span<Vector2> places = stackalloc Vector2[4];
        foreach (var box in part.Boxes)
        {
            if (box.Drawn == VoxelSandbox.Drawn.Own) continue;
            if (box.Source is not { } cuboid)
            {
                if (index == 0) mesh.Add(box.Min / 16, box.Max / 16, box.Color ?? Color.White, new Vector2(0.5f, rows + 0.5f) * through);
                continue;
            }
            if (cuboid.Picture != index) continue;
            var faces = cuboid.Faces();
            if (faces.All(face => Clear(picture, face, scale))) continue;
            foreach (var face in faces)
            {
                for (int i = 0; i < 4; i++) (corners[i], places[i]) = (face[i].Position / 16, face[i].Texel * through);
                mesh.AddFace(corners, places, cuboid.Tinted ? variant : Color.White);
            }
        }
        return mesh;
    }

    // Whether a face's part of the picture is clear throughout.
    private static bool Clear(Image picture, (Vector3 Position, Vector2 Texel)[] face, float scale)
    {
        int x0 = (int)(face.Min(c => c.Texel.X) * scale), x1 = (int)(face.Max(c => c.Texel.X) * scale);
        int y0 = (int)(face.Min(c => c.Texel.Y) * scale), y1 = (int)(face.Max(c => c.Texel.Y) * scale);
        for (int y = Math.Max(0, y0); y < Math.Min(picture.Height, y1); y++)
            for (int x = Math.Max(0, x0); x < Math.Min(picture.Width, x1); x++)
                if (picture.Data[(y * picture.Width + x) * 4 + 3] != 0) return false;
        return true;
    }

    /// <summary>Draws the mobs within reach of the eye. Called inside <c>BeginMode3D</c>.</summary>
    public void Draw(IReadOnlyList<Mob> mobs, Vector3 eye)
    {
        foreach (var slot in _slots)
        {
            slot.Plain.Clear();
            slot.Hurt.Clear();
            slot.Flash.Clear();
        }
        var drawn = 0;
        foreach (var mob in mobs)
        {
            if (Vector3.DistanceSquared(mob.Body.Position, eye) > Distance * Distance) continue;
            drawn++;
            var body = Matrix4x4.CreateScale(mob.Kind.Scale) * Matrix4x4.CreateRotationY(mob.Yaw) * Matrix4x4.CreateTranslation(mob.Body.Position);
            // A mob dying falls onto its side over most of a second, and a creeper swells as its
            // fuse burns, flashing white four times a second.
            if (mob.Dying >= 0) body = Matrix4x4.CreateRotationZ(MathF.Sqrt(MathF.Min(1, mob.Dying / 0.6f)) * MathF.PI / 2) * body;
            if (mob.Fuse >= 0) body = Matrix4x4.CreateScale(1 + 0.15f * mob.Fuse) * body;
            var red = mob.Hurt > 0 || mob.Dying >= 0;
            var white = mob.Fuse >= 0 && (int)(mob.Fuse * 8) % 2 == 1;
            var stance = new Stance(mob.StridePhase, mob.Stride, mob.HeadYaw - mob.Yaw, mob.HeadPitch, mob.WingAngle, mob.Hunting);
            var parts = mob.Kind.Parts;
            for (int p = 0; p < parts.Length; p++)
            {
                var part = parts[p];
                var local = part.PlacedBy(Gait.Turn(mob.Kind, part, stance));
                var variants = _byKind[mob.Kind.Id][p];
                var slot = variants[variants.Length == 1 ? 0 : mob.Variant % variants.Length];
                (red ? slot.Hurt : white ? slot.Flash : slot.Plain).Add(local * body);
            }
        }

        var draws = 0;
        foreach (var slot in _slots)
        {
            foreach (var (mesh, materials) in slot.Layers)
            {
                if (slot.Plain.Count > 0)
                {
                    DrawMeshInstanced(mesh, materials.Plain, CollectionsMarshal.AsSpan(slot.Plain));
                    draws++;
                }
                if (slot.Hurt.Count > 0)
                {
                    DrawMeshInstanced(mesh, materials.Hurt, CollectionsMarshal.AsSpan(slot.Hurt));
                    draws++;
                }
                if (slot.Flash.Count > 0)
                {
                    DrawMeshInstanced(mesh, materials.Flash, CollectionsMarshal.AsSpan(slot.Flash));
                    draws++;
                }
            }
        }
        (Draws, Drawn) = (draws, drawn);
    }

    public void Dispose()
    {
        Forget();
        UnloadShader(_shader);
    }
}
