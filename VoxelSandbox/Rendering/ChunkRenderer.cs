using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// Keeps a mesh for each section, meshes sections again as the world changes, and draws those
/// within the render distance with the emissive blocks.
/// </summary>
/// <remarks>
/// <para>
/// A section is one mesh in one white material, each vertex its position and eight packed bytes the
/// terrain shader unpacks into its normal, its tiles in the atlas of the resource packs and its
/// color (SectionMesher.Pack), twenty bytes a vertex on the GPU. The colors also go with the upload
/// for the scene's distance field, which reads them as the model pass reads the packed bytes, so
/// indirect light takes each block's color from the mesh it shares with its neighbors. A section
/// meshed again with the same faces keeps its mesh and takes new packed bytes and new colors in
/// place, which keep its positions, the field knowing a mesh by those, so a change of light leaves
/// the section settled in the field, which paints the section's cells in their new colors in the
/// frame after, as the ray-traced scene does its corners.
/// </para>
/// <para>
/// A material's emission is one for its whole draw, so an emissive block is a cube drawn on its
/// own, and the flame of a torch or the glass of a lantern a cube scaled to it, copies of one mesh
/// in one instanced draw for each surface. Sections are drawn by distance and by what the eye can
/// see through open cells, and not culled to the view's frustum, since a mesh left out of a frame
/// leaves the field and the light it gave or blocked goes with it until it settles again. Every
/// section within the render distance that the eye can see is drawn each frame, through the field's
/// gather and each pass's culling, so the render distance sets much of the frame's cost, which
/// <see cref="Draws"/> counts.
/// </para>
/// </remarks>
public sealed class ChunkRenderer : IDisposable
{
    // One of a section's meshes and the arrays it was made from, kept to tell a new meshing's faces
    // from the same faces in new light.
    private sealed class Part
    {
        public ModelMesh Mesh;
        // The entity the engine draws the part as, or -1.
        public int Entity = -1;
        public Vector3[] Positions = [];
        public uint[] Indices = [];
        public uint[] Packed = [];
        public Color[] Colors = [];
        // Where a blended part's entity stands in its section, the middle of its faces to the
        // nearest block, its positions held from there, since the engine draws blended entities
        // farthest first by where each stands, so a section's water and its glass each sort by
        // their own faces, a pane over the water drawn after it from above and before it from under
        // the water. A part drawn solid stands at the section's corner.
        public Vector3 Center;

        public bool HasFaces => Positions.Length > 0;

        public int Triangles => Indices.Length / 3;

        // Takes a meshing's faces, uploading them as the part's own mesh unless its group draws
        // them, and says whether they changed, the light alone or the faces too.
        public Change Take(SectionMesher.Faces faces, bool centered = false, bool upload = true)
        {
            var positions = CollectionsMarshal.AsSpan(faces.Positions);
            if (centered && positions.Length > 0)
            {
                Vector3 low = positions[0], high = positions[0];
                foreach (var at in positions) (low, high) = (Vector3.Min(low, at), Vector3.Max(high, at));
                var center = Vector3.Round((low + high) / 2);
                var held = new Vector3[positions.Length];
                for (int i = 0; i < held.Length; i++) held[i] = positions[i] - center;
                positions = held;
                Center = center;
            }
            var indices = CollectionsMarshal.AsSpan(faces.Indices);
            var packed = CollectionsMarshal.AsSpan(faces.Packed);
            if (positions.SequenceEqual(Positions) && indices.SequenceEqual(Indices))
            {
                // A relight writes the packed bytes, eight a vertex, and the colors global
                // illumination reads, the positions kept, the colors alone changing where a pack's
                // textures change and their tiles keep their numbers.
                if (packed.SequenceEqual(Packed) && CollectionsMarshal.AsSpan(faces.Colors).SequenceEqual(Colors)) return Change.None;
                (Packed, Colors) = (packed.ToArray(), faces.Colors.ToArray());
                if (upload)
                {
                    UpdateMeshBuffer<uint>(Mesh, MeshStreamBuffer, Packed, 0);
                    UpdateMeshBuffer<Color>(Mesh, 3, Colors, 0);
                }
                return Change.Light;
            }
            Forget();
            // The store keeps the arrays it is given until the frame uploads them, so each mesh has
            // arrays of its own that nothing changes afterward. The colors go with the upload for
            // the scene's light field, which reads its colors from them.
            (Positions, Indices, Packed, Colors) = (positions.ToArray(), indices.ToArray(), packed.ToArray(), faces.Colors.ToArray());
            Mesh = upload && Positions.Length > 0 ? UploadMesh(Positions, Indices, MeshStream.From(MeshStreamFormat.UInt2, Packed.AsSpan()), Colors.ToArray()) : default;
            return Change.Faces;
        }

        public void Forget()
        {
            if (Mesh.IsValid) UnloadMesh(Mesh);
            Mesh = default;
        }
    }

    private enum Change { None, Light, Faces }

    private sealed class SectionMeshes
    {
        public readonly Part Solid = new(), SeeThrough = new(), Water = new(), Cutout = new(), CutoutGlass = new(), SolidCutout = new(), Molten = new(), Foliage = new();
        public readonly List<(int Surface, Matrix4x4 At)> Emitters = [];

        public Part[] Parts => [Solid, SeeThrough, Water, Cutout, CutoutGlass, SolidCutout, Molten, Foliage];

        // The parts a group draws, in the order of a group's parts.
        public Part[] Grouped => [Solid, Foliage, Cutout, SolidCutout, Molten];
    }

    // The sections two by two across and one high whose opaque and cut-out parts are drawn as one
    // entity each, a quarter of the draws a part of each section made where the four hold it, since
    // the engine records each draw in each of its passes, the depth's, the camera's, the mirror's
    // and the cascades', on the main thread. A group two sections high would draw the caves'
    // sections under the land beside the land seen, which the flood leaves out alone, for more of
    // the GPU's time. The blended parts stay a section's own, since the engine draws
    // them farthest first by where each entity stands. A group's mesh is made again from its
    // sections' arrays at most once a frame, and its streams written in place where only the light
    // of its sections changed, so the scene's field keeps it settled. The engine keeps a mesh whose
    // stream is written in place in a ring of mapped buffers from then on, a group's four times a
    // section's, so the driver maps about 255 MB more at 8 columns.
    private sealed class Group
    {
        public readonly GroupPart[] Parts =
            [new(SectionPart.Solid), new(SectionPart.Foliage), new(SectionPart.Cutout), new(SectionPart.SolidCutout), new(SectionPart.Molten)];
        public Change Change;
        // Whether the group is marked only by its sections leaving, as they pass out of reach or
        // their column unloads, and whether since the last Rebuild.
        public bool Leaving, Fresh;
    }

    private sealed class GroupPart(int kind)
    {
        public readonly int Kind = kind;
        public ModelMesh Mesh;
        public int Entity = -1;
        public bool Seen, Casts;

        public void Forget()
        {
            if (Mesh.IsValid) UnloadMesh(Mesh);
            Mesh = default;
        }
    }

    // How many sections a group spans along each axis, as powers of two, two across and one high.
    private const int GroupX = 1, GroupY = 0, GroupZ = 1;

    private static (int X, int Y, int Z) GroupOf(SectionKey key) => (key.X >> GroupX, key.Y >> GroupY, key.Z >> GroupZ);

    private readonly Dictionary<(int X, int Y, int Z), Group> _groups = [];

    /// <summary>
    /// How many groups marked only by their sections leaving an update makes again at most, the
    /// nearest the eye first, each once the first update after its marking has passed.
    /// </summary>
    /// <remarks>
    /// The step into a column unloads the columns past the load circle behind the player, 25 to 29
    /// of them, and a group holding a section of one and of a column kept is made again without it.
    /// None of those is drawn, standing past the render distance, so they wait out the step's
    /// frame, where the flood shows the sections it brings, and are made two a frame after it.
    /// </remarks>
    internal const int LeftAFrame = 2;

    // The groups marked only by their sections leaving, and those of them the update makes again.
    private readonly List<(int X, int Y, int Z)> _left = [];
    private readonly HashSet<(int X, int Y, int Z)> _leftMade = [];

    /// <summary>
    /// Called with each group's corner section as it is made again, for a test to count them a
    /// frame.
    /// </summary>
    internal Action<SectionKey>? Remade { get; set; }

    // Every section's opaque faces are drawn with the first, their colors in their vertices, and
    // its see-through faces with the second, each vertex's alpha its surface's. An alpha under 255
    // makes the draw blend, which the vertices' alpha alone does not. The see-through faces cast no
    // shadow, which keeps them out of the scene's distance field too, since the field holds a
    // see-through mesh as solid and a window of glass would then shut out the indirect light
    // through it. A section's plants are a third mesh, cut out where the pack's texture is clear,
    // kept apart so the solid faces keep the depth test that runs before their pixels are lit, and
    // casting the shadow of what the texture keeps, which the shader's shadowMain cuts as its
    // fragment stage cuts the face, while global illumination holds none of them, so the scene's
    // distance field holds no grass. Glass a pack cuts out is a fourth, casting no shadow, so a
    // window lets the sun through as glass the pack leaves whole does. The faces a pack cuts out
    // that cast a shadow of the whole face, a door's window and a pack's leaves clear between them,
    // are a fifth. A section's lava is a sixth, drawn with lava's emission, which the field takes
    // from the mesh, so a lake lights its cave with one draw where a lamp's cube for each block
    // would be thousands; the field reads a mesh made again without its light for a few frames,
    // which a lake seldom is. All six are drawn through the game's terrain shader, the engine's
    // light with a haze.
    private readonly ModelMaterial _terrain, _clear, _water, _cutoutGlass, _solidCutout, _haze;
    // The plants' and the leaves' materials, each letting a share of global illumination's light
    // through, made again as the atlas changes, and lava's.
    private ModelMaterial _cutout, _foliage, _molten;

    // Each of a section's seven parts is an entity the engine draws, a Mesh of the part's upload, a
    // Material through the terrain shader with its material's shadow and its share in the scene's
    // field, a Transform, a Visible and a ShadowOnly, and the game's SectionPart naming it, so the
    // model pass gathers them with its change detection and a section standing still costs no call.
    // Visible and ShadowOnly are written where the flood's answer for the section changes.
    private readonly EcsWorld _ecs;
    private Material _terrainLook, _clearLook, _waterLook, _cutoutLook, _cutoutGlassLook, _solidCutoutLook, _moltenLook, _foliageLook;
    // The eye's column, render distance and culling the last placement was made for, and whether a
    // section's parts or lamps changed since.
    private (int X, int Z, int Distance, bool Cull)? _placedFor;
    private bool _changed;
    private int _placedDraws, _placedTriangles;

    // The dome under the horizon in the haze's color, drawn while there is a haze to meet it.
    private readonly ModelMesh _dome;
    private float _fogEnd = float.MaxValue;
    private readonly Shader _shader;
    private readonly int _fogColor, _fogStart, _fogEndAt, _atmosphere, _hazeHeight, _eyeCovered, _atlas, _atlasLayout, _animations, _flames, _looks;
    // The atlas's animations as the terrain shader takes them, the step each shows set each frame.
    private readonly Vector4[] _animationTable = new Vector4[TerrainAtlas.MostAnimations];
    private Texture2D _atlasTexture;
    // The textures of the emissive blocks' cubes a pack gives one, by surface, a cube drawn with
    // its own texture where a pack has one, which the scene's distance field reads its average of,
    // and the maps of the emission of those that show parts of their textures.
    private Texture2D[] _lampTextures = [], _lampEmission = [];

    private readonly Dictionary<SectionKey, SectionMeshes> _sections = [];
    private readonly SectionMesher _mesher = new();
    // The meshers lent to the workers, a section each, its copy taken here and its faces made on a
    // worker, so the main thread takes in finished faces while the world streams in. As many are
    // made at once as half the processor's cores, so the main thread keeps a core, and a section
    // copied waits for one, so a slow frame, as on a device that draws on the processor, meshes as
    // many as the workers make meanwhile rather than one round a frame. The sections the workers
    // made come back with the meshing asked of each, and the meshing last asked of a section is
    // kept, so a section edited or asked again while a worker meshes it takes only its newest
    // faces. An edit is meshed here at once, so a block set shows in its own frame.
    private readonly Stack<SectionMesher> _lent = new(Enumerable.Range(0, 64).Select(_ => new SectionMesher()));
    private readonly SemaphoreSlim _building = new(Math.Clamp(Environment.ProcessorCount / 2, 1, 16));
    private readonly System.Collections.Concurrent.ConcurrentQueue<(SectionKey Key, int Asked, SectionMesher Mesher, bool Made)> _built = new();
    private readonly Dictionary<SectionKey, int> _asked = [];
    // Each meshing asked is numbered from one count, so faces a worker made before a section was
    // let go never pass for those asked of it after.
    private int _asks, _asking;
    private ModelMesh _cube;
    // Each emissive surface's cube, the one above for a surface showing its whole texture on each
    // face, both made again as the origin moves (Shift).
    private ModelMesh[] _lampCubes = [];
    private readonly List<Matrix4x4>[] _emitterDraws = [.. Surfaces.All.Select(_ => new List<Matrix4x4>())];
    // The place of a lamp's cube filling its cell (Blocks.Glows).
    private static readonly Matrix4x4 WholeCell = Matrix4x4.CreateTranslation(new Vector3(0.5f));
    private readonly List<SectionKey> _queue = [];
    // The loaded sections out of the eye's reach, meshed once it comes near, and the eye's section
    // and render distance they were last sorted for.
    private readonly HashSet<SectionKey> _parked = [];
    // The loaded sections within reach the last flood left out, meshed once a flood reaches them,
    // since no pass draws a section the flood leaves out, so the stone under the land within reach
    // is meshed only where a cave opens to it, about 4,400 sections held in a flight over seed 1's
    // cherry grove against some 400 meshed a second.
    private readonly HashSet<SectionKey> _unseen = [];
    // The sections the flood leaves out that stand between a drawn one and the light, drawn into
    // the shadow maps alone, and the light's direction and shadow distance they were found for.
    private readonly HashSet<SectionKey> _casters = [];
    private readonly HashSet<(int X, int Y, int Z)> _corners = [];
    private (Vector3 TowardLight, float Distance)? _castFor;
    private readonly List<SectionKey> _gone = [];
    private (SectionKey Eye, int Distance)? _reachedFor;
    private ModelMaterial[] _lamps = [];
    // Each animated emissive surface's steps, its texture's steps and the ticks each is held, the
    // maps of their emission and its material with each.
    private (Image[] Steps, int Ticks)?[] _lampAnimations = [];
    private Texture2D[][] _lampStepEmission = [];
    private ModelMaterial[]?[] _lampSteps = [];
    private float _glowScale = 1;
    // Whether each emissive surface's light keeps its color.
    private bool _colored = true;
    private float _rim;
    private bool _unshaded;

    public ChunkRenderer(EcsWorld ecs)
    {
        _ecs = ecs;
        _shader = LoadShader("resources/shaders/terrain.slang");
        // The terrain's vertex stage keeps each corner's position as the mesh holds it and unpacks
        // only its packed bytes, so each pass leaves out the sections whose boxes it cannot see,
        // where the shadow cascades and the depth pass would draw every section within reach.
        SetShaderKeepsPositions(_shader, true);
        (_fogColor, _fogStart, _fogEndAt) = (GetShaderLocation(_shader, "fogColor"), GetShaderLocation(_shader, "fogStart"), GetShaderLocation(_shader, "fogEnd"));
        _atmosphere = GetShaderLocation(_shader, "atmosphere");
        _hazeHeight = GetShaderLocation(_shader, "hazeHeight");
        _eyeCovered = GetShaderLocation(_shader, "eyeCovered");
        _flames = GetShaderLocation(_shader, "flames");
        _looks = GetShaderLocation(_shader, "looks");
        SetLooks();
        (_atlas, _atlasLayout) = (GetShaderLocation(_shader, "atlas"), GetShaderLocation(_shader, "atlasLayout"));
        _animations = GetShaderLocation(_shader, "animations");
        MakeLampCubes();
        Atlas = TerrainAtlas.None;
        _terrain = new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Opaque, Roughness = 0.9f, Shader = _shader };
        // Glass, ice and water are rough, so they reflect neither the land the scene's distance
        // field traces nor the sky, which in the field's coarse cells clashes with the blocks'
        // pixels.
        _clear = new ModelMaterial(new Color(255, 255, 255, 254)) { AlphaMode = MaterialAlphaMode.Blend, Roughness = 1, CastsShadows = false, Shader = _shader };
        // Water is see-through as glass is, its material's alpha of 253 telling the terrain shader
        // its faces carry the water's depth, which it darkens and clears the water by.
        _water = _clear with { Color = new Color(255, 255, 255, 253) };
        _cutoutGlass = new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Mask, Roughness = 0.9f, CastsShadows = false, Shader = _shader };
        _solidCutout = new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Mask, Roughness = 0.9f, Shader = _shader };
        (_terrainLook, _clearLook, _waterLook) = (Look(_terrain), Look(_clear), Look(_water));
        (_cutoutGlassLook, _solidCutoutLook) = (Look(_cutoutGlass), Look(_solidCutout));
        // The haze dome's material, its alpha of 252 telling the terrain shader the dome's corners
        // stand under the open sky.
        _haze = new ModelMaterial(new Color(255, 255, 255, 252)) { AlphaMode = MaterialAlphaMode.Blend, CastsShadows = false, Shader = _shader };
        var dome = HazeDome.Build();
        _dome = UploadMesh([.. dome.Positions], [.. dome.Indices], MeshStream.From(MeshStreamFormat.UInt2, dome.Packed.ToArray().AsSpan()), null);
        Fog(Color.White, float.MaxValue, float.MaxValue);
    }

    /// <summary>
    /// The tick the animated textures and lamps are held at, a twentieth of a second each, or null
    /// for the time's own.
    /// </summary>
    /// <remarks>
    /// A frame takes its tick from the time it is drawn at, so frames drawn at a steady pace whose
    /// length is a whole number of tenths of a second all show the same of two frames a tick each,
    /// as lavapipe's can, and a held tick shows each frame of an animation however the frames fall.
    /// </remarks>
    public double? HeldTicks { get; set; }

    // The atlas's animations as the terrain shader takes them, each with the step it shows at a
    // time in game ticks as TerrainAtlas.Step gives it.
    private static Vector4[] AnimationSteps(TerrainAtlas atlas, Vector4[] table, double ticks)
    {
        for (int i = 0; i < atlas.Animations.Count; i++)
        {
            var (_, count, held) = atlas.Animations[i];
            table[i].W = TerrainAtlas.Step(count, held, ticks);
        }
        return table;
    }

    /// <summary>
    /// Whether the haze dome is drawn, which it is while a haze fades the land's edge short of it.
    /// </summary>
    public bool Hazed => _fogEnd < HazeDome.Radius;

    /// <summary>
    /// Sets the haze the sections fade into, its color and the distances in blocks it starts at and
    /// covers by, and the atmosphere's haze nearer than that, its most and how fast it grows, none
    /// where the most is 0. Where the sky is behind, each point fades into the sky drawn behind it
    /// under the haze dome's band, and the color is the dome's, otherwise each fades into the
    /// color. A point takes the haze by the sky's light reaching it or the eye, whichever is
    /// higher, so a cave seen from in it takes none, the sky's light level at the eye given, or the
    /// open sky's.
    /// </summary>
    public void Fog(Color color, float start, float end, float atmosphere = 0, float growth = 0, bool sky = false, bool byHeight = false, float eyeSky = Lighting.Max)
    {
        SetShaderValue(_shader, _eyeCovered, Lighting.Max - eyeSky);
        SetShaderValue(_shader, _hazeHeight, new Vector4(Haze.Base, Haze.Scale, byHeight ? 1 : 0, 0));
        SetShaderValue(_shader, _fogColor, new Vector4(color.R, color.G, color.B, 255) / 255);
        SetShaderValue(_shader, _fogStart, start);
        SetShaderValue(_shader, _fogEndAt, end);
        SetShaderValue(_shader, _atmosphere, new Vector4(atmosphere, growth, sky ? 1 : 0, HazeDome.Radius / 2));
        _fogEnd = end;
    }

    /// <summary>
    /// The textures of the packs loaded, whose tiles the faces are drawn with, the image built with
    /// its mip levels as it is set, which takes effect on the faces as sections are shaded again.
    /// </summary>
    public TerrainAtlas Atlas
    {
        get => _mesher.Atlas;
        set
        {
            _mesher.Atlas = value;
            if (_atlasTexture.IsValid) UnloadTexture(_atlasTexture);
            _atlasTexture = LoadTextureFromImage(new Image(value.Image(), value.Width, value.Height));
            GenTextureMipmaps(ref _atlasTexture);
            SetTextureFilter(_atlasTexture, TextureFilter.Point);
            SetShaderValueTexture(_shader, _atlas, _atlasTexture);
            SetShaderValue(_shader, _atlasLayout, new Vector4(TerrainAtlas.Columns, value.Rows, value.Pixels, value.Animations.Count));
            Array.Clear(_animationTable);
            for (int i = 0; i < value.Animations.Count; i++) _animationTable[i] = new Vector4(value.Animations[i].First, value.Animations[i].Count, value.Animations[i].Ticks, 0);
            SetShaderValueV<Vector4>(_shader, _animations, _animationTable);
            SetShaderValue(_shader, _flames, new Vector4(value.Tile(Surfaces.Fire) is > 0 and var flames ? flames + 1 : 0, FlamesShown,
                value.Tile(Surfaces.Lava) is > 0 and var lava ? lava + 1 : 0, LavaShown));
            LoadLamps();
            Shares();
        }
    }

    // The surfaces of the leaf blocks and of the plants' walls, whose tiles the shares of light
    // through them are read from.
    private static readonly int[] LeafSurfaces = [.. Blocks.All.Where(b => Blocks.IsLeaves(b.Id)).SelectMany(b => new[] { b.Top, b.Side, b.Bottom }).Distinct()];
    private static readonly int[] PlantSurfaces = [.. Blocks.All.Where(b => b.Shape is not null)
        .SelectMany(b => b.Shape!.Pieces.Where(p => b.Shape.Cutout || p.Plane).Select(p => p.Surface)).Distinct()];

    // The leaves' and the plants' materials for the atlas, the scene's light passing a leaf block
    // by half, as the source's leaves let the light through, or by what a pack's cut-out leaves
    // leave clear across a block's two faces where that is more, and passing a plant by the share
    // of its tiles left clear, which the screen's probes take, the scene's field holding no plant.
    private void Shares()
    {
        var (leaves, plants, cut) = SharesOf(_mesher.Atlas);
        _foliage = new ModelMaterial(Color.White)
        {
            AlphaMode = cut ? MaterialAlphaMode.Mask : MaterialAlphaMode.Opaque, Roughness = 0.9f, LightThrough = leaves, Shader = _shader,
        };
        _cutout = new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Mask, Roughness = 0.9f, BouncesLight = false, LightThrough = plants, Shader = _shader };
        (_foliageLook, _cutoutLook) = (Look(_foliage), Look(_cutout));
        foreach (var group in _groups.Values)
            foreach (var part in group.Parts)
                if (part.Entity >= 0 && part.Kind is SectionPart.Foliage or SectionPart.Cutout) _ecs.Update(part.Entity, LookOf(part.Kind));
    }

    /// <summary>
    /// The shares of global illumination's light passing a unit of a leaf block's depth and of a
    /// plant's for an atlas, and whether it cuts any leaves out.
    /// </summary>
    public static (float Leaves, float Plants, bool LeavesCut) SharesOf(TerrainAtlas atlas)
    {
        float? Mean(IEnumerable<int> surfaces) => surfaces.Select(atlas.Coverage).OfType<float>().DefaultIfEmpty(-1).Average() is var mean and >= 0 ? mean : null;
        var cut = LeafSurfaces.Where(atlas.Cuts).ToList();
        var leaves = Mean(cut) is { } covered ? Math.Max(0.5f, (1 - covered) * (1 - covered)) : 0.5f;
        return (leaves, 1 - (Mean(PlantSurfaces.Where(atlas.Cuts)) ?? 1), cut.Count > 0);
    }

    /// <summary>
    /// The share of global illumination's light passing a unit of a leaf block's depth, and of a
    /// plant's.
    /// </summary>
    public (float Leaves, float Plants) LightThrough => (_foliage.LightThrough, _cutout.LightThrough);

    // The emissive blocks' textures and the maps of their emission from the atlas, and their
    // materials, again as the atlas or the lamps' color changes, since each map is divided by the
    // light of its material.
    private void LoadLamps()
    {
        var value = _mesher.Atlas;
        foreach (var texture in _lampTextures.Concat(_lampEmission).Concat(_lampStepEmission.SelectMany(steps => steps)))
            if (texture.IsValid) UnloadTexture(texture);
        _lampTextures = [.. Surfaces.All.Select(s => !s.Emits ? default
            : LampFaces[s.Id] is { } faces ? LoadTextureFromImage(Strip(value, faces, s, glow: false))
            : value.Picture(s.Id) is { } picture ? LoadTextureFromImage(picture) : default)];
        _lampEmission = [.. Surfaces.All.Select(s => !s.Emits ? default
            : LampFaces[s.Id] is { } faces ? LoadTextureFromImage(Strip(value, faces, s, glow: true, LightOf(s.Id)))
            : LampParts[s.Id] is not null && value.Picture(s.Id) is { } picture ? LoadTextureFromImage(EmissionMap(picture, s, LightOf(s.Id))) : default)];
        // An animated lamp keeps its first frame's texture, which the scene's distance field reads
        // the average of as its color, since a draw whose color changes is no longer the same and
        // is stamped again without its light for frames, and steps its emission map instead, which
        // the field does not read and which shows most of what a lamp's face shows.
        _lampAnimations = [.. Surfaces.All.Select(s => s.Emits && LampFaces[s.Id] is null ? value.Animated(s.Id) : null)];
        _lampStepEmission = [.. Surfaces.All.Select(s => _lampAnimations[s.Id] is { } animation
            ? animation.Steps.Select(step => LoadTextureFromImage(EmissionMap(step, s, LightOf(s.Id)))).ToArray() : [])];
        foreach (var texture in _lampTextures.Concat(_lampEmission).Concat(_lampStepEmission.SelectMany(steps => steps)))
            if (texture.IsValid) SetTextureFilter(texture, TextureFilter.Point);
        BuildLamps();
    }

    /// <summary>What the light of every emissive surface is multiplied by.</summary>
    public float GlowScale
    {
        get => _glowScale;
        set
        {
            if (value == _glowScale) return;
            _glowScale = value;
            BuildLamps();
        }
    }

    /// <summary>
    /// Whether each emissive block's light is its own color, its surface's emission, a lantern's
    /// orange and a soul lantern's cyan, or white as bright to the eye, as the source's light is.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The engine draws a lamp's face by its material's emission times its emission map, and the
    /// scene's distance field and so the global illumination take that emission whole, so white
    /// light keeps each face's colors through its map up to the light's own brightness.
    /// </para>
    /// <para>
    /// The color is the surface's own rather than one read from a pack's texture. A lamp's
    /// brightest pixels are nearly white, so the mean of its pixels by how much each emits is far
    /// paler than the color the lamp stands for, and the scene's saturation grade turns such a pale
    /// yellow toward olive on a white wall.
    /// </para>
    /// </remarks>
    public bool ColoredLamps
    {
        get => _colored;
        set
        {
            if (value == _colored) return;
            _colored = value;
            LoadLamps();
        }
    }

    /// <summary>
    /// How much brighter than its face the rim along each whole block's edges is drawn, 0 for none.
    /// </summary>
    public float Rim
    {
        get => _rim;
        set
        {
            _rim = Math.Max(0, value);
            SetLooks();
        }
    }

    /// <summary>
    /// Whether every open face takes the sun's light as one facing the sun does, for a flat look
    /// with its shadows kept.
    /// </summary>
    public bool Unshaded
    {
        get => _unshaded;
        set
        {
            _unshaded = value;
            SetLooks();
        }
    }

    /// <summary>
    /// How much of the floor of the sky's light by a face's sky level the terrain keeps, from 0 to
    /// 1, 0.65 by default.
    /// </summary>
    /// <remarks>
    /// The engine's light passes the leaves, so grass under a canopy two leaves deep at noon takes
    /// 0.74 of the light of grass in a wall's shade on open ground from the engine alone, the floor
    /// whole lifted it to 1.05, past the open ground's own, and 0.65 of it lifts it to about 0.80,
    /// as the source lights the canopy's level 13 against the open sky's 15.
    /// </remarks>
    public float SkyFloor
    {
        get => _skyFloor;
        set
        {
            _skyFloor = Math.Clamp(value, 0, 1);
            SetLooks();
        }
    }

    private float _skyFloor = 0.65f;

    private void SetLooks() => SetShaderValue(_shader, _looks, new Vector4(_rim, _unshaded ? 1 : 0, 1 - _skyFloor, 0));

    /// <summary>
    /// The light an emissive surface gives at a glow of 1 in linear light, its surface's emission,
    /// or white as bright where <see cref="ColoredLamps"/> is off.
    /// </summary>
    public Vector3 LightOf(int surface)
    {
        var own = LinearOf(Surfaces.All[surface].Emissive);
        return _colored ? own : new Vector3(Brightness(own));
    }

    /// <summary>A color in linear light.</summary>
    public static Vector3 LinearOf(Color color) => new(TerrainAtlas.Linear(color.R), TerrainAtlas.Linear(color.G), TerrainAtlas.Linear(color.B));

    /// <summary>How bright a light in linear light is to the eye, by Rec. 709's weights.</summary>
    public static float Brightness(Vector3 light) => 0.2126f * light.X + 0.7152f * light.Y + 0.0722f * light.Z;

    /// <summary>
    /// A light in linear light as the color of its hue at its brightest and how many times that it
    /// is, so no part of it clips in a color's bytes.
    /// </summary>
    public static (Color Hue, float Most) Split(Vector3 light)
    {
        var most = MathF.Max(light.X, MathF.Max(light.Y, light.Z));
        var hue = light / MathF.Max(most, 1e-6f);
        return (TerrainAtlas.Encode(hue.X, hue.Y, hue.Z), most);
    }

    // An emissive surface's material with its light as LightOf has it.
    private ModelMaterial Lamp(Surface s)
    {
        var material = s.Material(_glowScale);
        if (!s.Emits) return material;
        var (hue, most) = Split(LightOf(s.Id));
        return material with { Emissive = hue, EmissiveIntensity = s.Glow * _glowScale * most };
    }

    // The flood drawn by, and the flood made ahead of the eye from the section it moves into, which
    // the step into that section takes in place of its own where it holds what that would make.
    private Visibility _visibility = new(), _ahead = new();
    private Vector3 _lastEye;
    // Whether the flood made ahead has finished for the section the eye moves into, whose sections
    // it sees are meshed before the step, so the step draws them in its own frame.
    private bool _aheadReady;
    // How near the face of the next section along the eye's motion the flood ahead begins, in
    // blocks, half a section, which a flight crosses in some forty frames and the flood takes three
    // of.
    private const float AheadBlocks = Section.Size / 2;
    // The sections held back as unseen that the finished flood ahead sees, released to be meshed
    // ReleasedAFrame a frame, so the fifty or so a flight's step brings are meshed and taken in
    // over some seven frames, well before the step.
    private readonly List<SectionKey> _releasing = [];
    internal const int ReleasedAFrame = 8;
    // Those released that still wait to be sent to a mesher, a frame's meshing having spent its
    // time before them, which count against the next frame's release, so no frame sends more of
    // them than ReleasedAFrame however long the frames' meshing takes.
    private readonly HashSet<SectionKey> _released = [];
    // The sections toward the light the flood leaves out sent a frame at most, the nearest the eye
    // first and the rest kept marked for the next, so the casters a flight's step's flood finds
    // are meshed over a few frames, apart from those released, rather than in one.
    internal const int CastersAFrame = 4;

    /// <summary>
    /// The time a frame's meshing takes in place of what the game gives it where set, so a test can
    /// make a frame's meshing run short or as long as it asks.
    /// </summary>
    internal double? MeshMs { get; set; }

    /// <summary>
    /// The time a frame gives a flood begun as the eye steps into the next section, or made ahead
    /// of it, which goes on in the frames after, the last whole flood drawn meanwhile, 2 ms unless
    /// set.
    /// </summary>
    internal double FloodBudgetMs { get; set; } = 2;

    /// <summary>
    /// Called with the eye at each step that takes the flood made ahead, for a test to set it
    /// against a whole one.
    /// </summary>
    internal Action<Vector3, IReadOnlySet<SectionKey>>? TookAhead { get; set; }

    /// <summary>
    /// A step each meshing worker takes with its section before it makes the faces, which a test
    /// sets to hold it.
    /// </summary>
    internal Action<SectionKey>? Building { get; set; }

    /// <summary>
    /// Called with each section as its faces are taken and the atlas they were made with, for a
    /// test to read.
    /// </summary>
    internal Action<SectionKey, TerrainAtlas>? Taking { get; set; }

    private bool _castsDue;

    /// <summary>
    /// The most a frame's flood, casters' search and placement of the sections took, in ms, since
    /// it was last set to 0.
    /// </summary>
    public double PeakMs { get; set; }

    /// <summary>
    /// Whether the sections the eye cannot see through open cells are left undrawn, as
    /// <see cref="Visibility"/> floods them, the lamps' cubes drawn all the same.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A section left undrawn also leaves the scene's distance field, which builds its cascades
    /// again as sections come into sight and go out of it. The land the flood leaves out lies
    /// behind walls the drawn sections hold, the walls of a cave or a closed room, so the eye sees
    /// the same light either way, a glowstone's in a cave and the dark of a cave in a hill at noon.
    /// </para>
    /// <para>
    /// The sun's shadow is another matter, since the rock over a cave stands between the light and
    /// what the eye sees there. So the sections the flood leaves out that lie toward the light from
    /// a drawn section within the shadow distance are drawn into the shadow maps alone, through the
    /// engine's shadow-only draw, their opaque faces casting and seen nowhere. They are found by
    /// rays toward the light from each corner of every drawn section within the shadow distance,
    /// through the sections to the world's top, again when a flood changes what is drawn, the light
    /// turns or the shadow distance changes, and are meshed as the drawn ones are.
    /// </para>
    /// </remarks>
    public bool Cull { get; set; } = true;

    /// <summary>
    /// How many sections were sent whose gathering failed for a column around them not loaded.
    /// </summary>
    internal int Ungathered { get; private set; }

    /// <summary>Whether the last flood reached a section.</summary>
    internal bool Sees(SectionKey key) => _visibility.Sees(key);

    /// <summary>
    /// Whether a section the flood left out stands toward the light from a drawn one, meshed apart
    /// from the release, <see cref="CastersAFrame"/> a frame.
    /// </summary>
    internal bool CastsTowardLight(SectionKey key) => _casters.Contains(key);

    /// <summary>
    /// For each part a section holds faces of, whether its group's part stands in the scene and
    /// whether it stands in the shadow maps and the field alone.
    /// </summary>
    internal IEnumerable<(bool Shown, bool ShadowOnly)> Standing(SectionKey key)
    {
        if (!_sections.TryGetValue(key, out var entry) || !_groups.TryGetValue(GroupOf(key), out var group)) yield break;
        for (int k = 0; k < entry.Grouped.Length; k++)
            if (entry.Grouped[k].HasFaces && group.Parts[k].Entity >= 0)
                yield return (_ecs.GetReadOnly<Visible>(group.Parts[k].Entity).Value, _ecs.GetReadOnly<ShadowOnly>(group.Parts[k].Entity).Value);
    }

    /// <summary>Sections the last <see cref="Draw"/> left out as unseen.</summary>
    public int Culled { get; private set; }

    /// <summary>
    /// Whether the sections the flood leaves out stand in the shadow maps and the scene's field,
    /// those meshed and those toward the light from a drawn one, as <see cref="Cull"/> says, true
    /// unless set.
    /// </summary>
    public bool Casts { get; set; } = true;

    /// <summary>
    /// Sections the last <see cref="Draw"/> drew into the shadow maps alone, of those it left out,
    /// and the draws it made of them.
    /// </summary>
    public (int Sections, int Draws) Casting { get; private set; }

    /// <summary>
    /// Deep sections the last <see cref="Draw"/> drew, those under their column's floor, and their
    /// triangles.
    /// </summary>
    public (int Sections, int Triangles) Deep { get; private set; }

    /// <summary>
    /// Draw calls the last <see cref="Draw"/> made, each instanced draw of lamps counted once.
    /// </summary>
    public int Draws { get; private set; }

    public int Triangles { get; private set; }

    public int Lamps { get; private set; }

    /// <summary>
    /// The lamps the last placement draws, each by its surface's cube and where the cube stands.
    /// </summary>
    public IEnumerable<(ModelMesh Cube, Matrix4x4 At)> LampsPlaced
    {
        get
        {
            for (int surface = 0; surface < _emitterDraws.Length; surface++)
                foreach (var at in _emitterDraws[surface]) yield return (_lampCubes[surface], at);
        }
    }

    /// <summary>Sections holding faces or lamps.</summary>
    public int Sections => _sections.Count;

    /// <summary>
    /// The colors of a section's solid faces as the scene's light field reads them, or none for a
    /// section with no faces.
    /// </summary>
    public IReadOnlyList<Color> SolidColors(SectionKey key) => _sections.TryGetValue(key, out var entry) ? entry.Solid.Colors : [];

    /// <summary>
    /// The sections out of the eye's reach, each of a column loaded, meshed once the eye comes
    /// near.
    /// </summary>
    public IReadOnlyCollection<SectionKey> Parked => _parked;

    /// <summary>
    /// Whether faces are darkened by light levels, which takes effect as sections are shaded again.
    /// </summary>
    public bool LightLevels
    {
        get => _mesher.LightLevels;
        set => _mesher.LightLevels = value;
    }

    /// <summary>
    /// Whether corners are darkened where blocks meet, which takes effect as sections are shaded
    /// again.
    /// </summary>
    public bool CornerShade
    {
        get => _mesher.CornerShade;
        set => _mesher.CornerShade = value;
    }

    /// <summary>
    /// How far corners darken where blocks meet, from 0 to Minecraft's levels at 1, which takes
    /// effect as sections are shaded again.
    /// </summary>
    public float CornerStrength
    {
        get => _mesher.CornerStrength;
        set => _mesher.CornerStrength = Math.Clamp(value, 0, 1);
    }

    // Each surface's material for the cubes of its emissive blocks, white under a pack's texture,
    // and lava's, its color in its vertices.
    private void BuildLamps()
    {
        var lava = Lamp(Surfaces.All[Surfaces.Lava]);
        _molten = new ModelMaterial(Color.White)
        {
            AlphaMode = MaterialAlphaMode.Opaque, Roughness = lava.Roughness, Shader = _shader, Emissive = lava.Emissive, EmissiveIntensity = lava.EmissiveIntensity,
        };
        _moltenLook = Look(_molten);
        foreach (var group in _groups.Values)
            if (group.Parts[4].Entity >= 0) _ecs.Update(group.Parts[4].Entity, _moltenLook);
        _lamps = [.. Surfaces.All.Select(s => _lampTextures[s.Id].IsValid
            ? Lamp(s) with { Color = Color.White, Texture = _lampTextures[s.Id], EmissiveMap = _lampEmission[s.Id] }
            : s.Unseen ? Lamp(s) with { AlphaMode = MaterialAlphaMode.Mask, Color = s.Color with { A = 0 } }
            : Lamp(s))];
        _lampSteps = [.. Surfaces.All.Select(s => _lampStepEmission[s.Id].Length == 0 ? null : _lampStepEmission[s.Id].Select(map => _lamps[s.Id] with { EmissiveMap = map }).ToArray())];
    }

    // The parts of its texture each emissive surface's cube shows, as the source's model gives a
    // lantern's glass and a torch's flame theirs, or null for a cube showing its whole texture on
    // each face. A surface's cubes are one instanced draw of one mesh, so its lit pieces share
    // their parts.
    private static readonly Parts?[] LampParts = [.. Surfaces.All.Select(s =>
        Blocks.All.SelectMany(b => b.Shape?.Pieces ?? []).FirstOrDefault(p => p.Glows && p.Surface == s.Id).Parts)];

    // Each emissive surface's whole lamp whose faces show more than one surface, its top's, its
    // sides' and its bottom's, as a lit furnace's do, or null for one showing the one on every
    // face.
    private static readonly (int Top, int Side, int Bottom)?[] LampFaces = [.. Surfaces.All.Select(s =>
        Blocks.All.FirstOrDefault(b => b.Shape is null && b.Emits && b.Lit == s.Id && !(b.Top == b.Side && b.Side == b.Bottom)) is { } lamp
            ? ((int, int, int)?)(lamp.Top, lamp.Side, lamp.Bottom) : null)];

    /// <summary>
    /// A lamp's faces' pictures side by side, its top's, its sides' and its bottom's, each a pack's
    /// texture or a tile of the surface's own color, or with <paramref name="glow"/> the map of
    /// their emission, an emissive surface's picture's bright pixels as <see cref="EmissionMap"/>
    /// lights them against <paramref name="light"/> or its own color's whole, and nothing of the
    /// rest, so a lit furnace's top shows the furnace's and its front alone glows.
    /// </summary>
    public static Image Strip(TerrainAtlas atlas, (int Top, int Side, int Bottom) faces, Surface lit, bool glow, Vector3? light = null)
    {
        var size = atlas.Pixels;
        var data = new byte[3 * size * size * 4];
        int[] surfaces = [faces.Top, faces.Side, faces.Bottom];
        for (int tile = 0; tile < 3; tile++)
        {
            var surface = Surfaces.All[surfaces[tile]];
            var picture = atlas.Picture(surface.Id);
            byte[] pixels = glow
                ? !surface.Emits ? new byte[size * size * 4] : picture is { } shown ? EmissionMap(shown, lit, light).Data : Filled(Color.White)
                : picture is { } own ? own.Data : Filled(surface.Color);
            for (int y = 0; y < size; y++)
                Array.Copy(pixels, y * size * 4, data, (y * 3 * size + tile * size) * 4, size * 4);
        }
        return new Image(data, 3 * size, size);

        byte[] Filled(Color color)
        {
            var pixels = new byte[size * size * 4];
            for (int i = 0; i < pixels.Length; i += 4) (pixels[i], pixels[i + 1], pixels[i + 2], pixels[i + 3]) = (color.R, color.G, color.B, 255);
            return pixels;
        }
    }

    // Uploads each emissive surface's cube, those uploaded before let go.
    private void MakeLampCubes()
    {
        UnloadLampCubes();
        _cube = GenMeshCube(1, 1, 1);
        _lampCubes = [.. Surfaces.All.Select(s => LampParts[s.Id] is { } shown ? LampCube(shown, new Vector2(16))
            : LampFaces[s.Id] is not null ? LampCube(new Parts(new Vector4(16, 0, 32, 16), new Vector4(0, 0, 16, 16), new Vector4(32, 0, 48, 16)), new Vector2(48, 16)) : _cube)];
    }

    private void UnloadLampCubes()
    {
        foreach (var cube in _lampCubes)
            if (cube.Id != _cube.Id) UnloadMesh(cube);
        if (_cube.IsValid) UnloadMesh(_cube);
    }

    // A cube a unit across about its middle, each face showing its part of a texture of a size in
    // pixels.
    private static ModelMesh LampCube(Parts parts, Vector2 size)
    {
        var (vertices, indices) = (new ModelVertex[24], new uint[36]);
        for (int face = 0; face < 6; face++)
        {
            for (int i = 0; i < 4; i++)
            {
                var corner = SectionMesher.Corner(face, i);
                vertices[face * 4 + i] = new ModelVertex(corner - new Vector3(0.5f), SectionMesher.Normal(face), SectionMesher.Place(parts, face, corner) / size);
            }
            var first = (uint)face * 4;
            ((ReadOnlySpan<uint>)[first, first + 1, first + 2, first, first + 2, first + 3]).CopyTo(indices.AsSpan(face * 6));
        }
        return UploadMesh(vertices, indices);
    }

    /// <summary>
    /// How many times their own color a fire's flames show, whatever light reaches them.
    /// </summary>
    public const float FlamesShown = 1;

    /// <summary>
    /// How many times its own color a pack's lava shows, whatever light reaches it, where without a
    /// pack it shows its surface's emission over its light.
    /// </summary>
    public const float LavaShown = 1;

    /// <summary>
    /// How many times its own color a lit piece's bright pixels show, a lantern's glass or a
    /// torch's flame.
    /// </summary>
    public const float LampShown = 4;

    /// <summary>
    /// The map of the emission of a lit piece that shows parts of its texture, so its bright pixels
    /// emit <see cref="LampShown"/> times their own color and its dark ones, a lantern's iron and a
    /// torch's stick, show as they are lit, where the material's emission over every pixel would
    /// draw the iron as bright as the flame. The scene's distance field takes the material's
    /// emission whole, so the light the lamp casts is the same.
    /// </summary>
    /// <remarks>
    /// A pixel emits by the eighth power of its brightest channel, so iron at 0.4 keeps under a
    /// thousandth of the emission and a flame's yellow most of it. The map multiplies the
    /// material's emission, so each channel is divided by the material's, its light, the surface's
    /// emission unless <paramref name="light"/> is given, times the surface's
    /// <see cref="Surface.Glow"/>, and encoded as sRGB, as the engine decodes an emissive map.
    /// </remarks>
    public static Image EmissionMap(Image picture, Surface surface, Vector3? light = null)
    {
        var (data, map) = (picture.Data, new byte[picture.Data.Length]);
        var emitted = (light ?? LinearOf(surface.Emissive)) * surface.Glow;
        for (int at = 0; at < data.Length; at += 4)
        {
            var bright = MathF.Pow(Math.Max(data[at], Math.Max(data[at + 1], data[at + 2])) / 255f, 8) * LampShown;
            var own = new Vector3(TerrainAtlas.Linear(data[at]), TerrainAtlas.Linear(data[at + 1]), TerrainAtlas.Linear(data[at + 2]));
            var share = Vector3.Min(own * bright / Vector3.Max(emitted, new Vector3(1e-4f)), Vector3.One);
            var encoded = TerrainAtlas.Encode(share.X, share.Y, share.Z);
            (map[at], map[at + 1], map[at + 2], map[at + 3]) = (encoded.R, encoded.G, encoded.B, 255);
        }
        return new Image(map, picture.Width, picture.Height);
    }

    /// <summary>
    /// Meshes every section an edit changed, then those whose shading changed and those that
    /// loaded, nearest the eye first, until <paramref name="budgetMs"/> of the frame is spent. A
    /// section out of the eye's reach, as <see cref="Reach"/> has it at
    /// <paramref name="renderDistance"/>, is set aside and meshed once the eye comes near, and a
    /// deep one's meshes are let go a section past it.
    /// </summary>
    public void Update(VoxelWorld world, Vector3 eye, int renderDistance, double budgetMs)
    {
        budgetMs = MeshMs ?? budgetMs;
        while (_built.TryDequeue(out var done))
        {
            _asking--;
            if (--_sent[done.Key] == 0) _sent.Remove(done.Key);
            _lent.Push(done.Mesher);
            if (!done.Made || !_asked.TryGetValue(done.Key, out var asked) || asked != done.Asked) continue;
            // Faces made with an atlas before the one set would show the tiles of packs let go, so
            // the section is meshed again with the atlas set.
            if (done.Mesher.Atlas == _mesher.Atlas) Take(world, done.Key, done.Mesher);
            else world.Reshaded.Add(done.Key);
        }
        foreach (var key in world.Edited)
            if (!Remesh(world, key)) world.Loaded.Add(key);
        world.Reshaded.ExceptWith(world.Edited);
        world.Edited.Clear();
        var at = Reach.Of(eye);
        Rebuild(at);

        if (_reachedFor != (at, renderDistance))
        {
            _reachedFor = (at, renderDistance);
            foreach (var key in _parked)
                if (InReach(world, key, at, renderDistance)) world.Loaded.Add(key);
            _parked.ExceptWith(world.Loaded);
            _gone.Clear();
            foreach (var key in _sections.Keys)
                if (!InReach(world, key, at, renderDistance + 1)) _gone.Add(key);
            foreach (var key in _gone)
            {
                Forget(key, leaving: true);
                _parked.Add(key);
            }
        }

        if (world.Loaded.Count == 0 && world.Reshaded.Count == 0) return;
        var (ex, ey, ez) = (at.X, at.Y, at.Z);
        _queue.Clear();
        _queue.AddRange(world.Reshaded);
        _queue.AddRange(world.Loaded);
        _queue.Sort((a, b) => Distance(a).CompareTo(Distance(b)));
        int Distance(SectionKey k) => (k.X - ex) * (k.X - ex) + (k.Y - ey) * (k.Y - ey) + (k.Z - ez) * (k.Z - ez);

        var watch = Stopwatch.StartNew();
        var (meshed, casters) = (0, 0);
        foreach (var key in _queue)
        {
            if (watch.Elapsed.TotalMilliseconds > budgetMs) break;
            // A section out of reach is parked, one the flood leaves out held back and one of air
            // let go, each at once, while one to mesh waits marked for a mesher to come back once
            // every one is lent.
            var far = !InReach(world, key, at, renderDistance);
            // One the flood ahead alone sees waits among those it releases, however it was marked,
            // as a column joining late marks its neighbors' sections, unless it is one released.
            var hidden = !far && Cull && !_visibility.Sees(key) && !_casters.Contains(key);
            var held = hidden && _aheadReady && _ahead.Sees(key) && !_released.Contains(key);
            var unseen = hidden && !(_aheadReady && _ahead.Sees(key)) || held;
            var empty = !far && !unseen && (!world.TryGetColumn(key.X, key.Z, out var column) || column.Numbered(key.Y) is not { Filled: > 0 });
            if (!far && !unseen && !empty && _lent.Count == 0) continue;
            var caster = !far && Cull && !_visibility.Sees(key) && _casters.Contains(key);
            if (caster && !empty && casters >= CastersAFrame) continue;
            // A section whose neighbor column has not arrived is dropped rather than kept, since
            // the neighbor's arrival marks it again, and one of a column not loaded is dropped
            // rather than parked, since the column's arrival marks it.
            if (!world.Reshaded.Remove(key) & !world.Loaded.Remove(key)) continue;
            if (far)
            {
                if (world.HasColumn(key.X, key.Z)) _parked.Add(key);
            }
            else if (unseen)
            {
                _unseen.Add(key);
                if (held) _releasing.Add(key);
            }
            else if (empty) Forget(key);
            // One whose gathering needs a column around it not yet loaded is dropped too, its
            // column's join marking it again, rather than sent to fail.
            else if (!AroundLoaded(world, key)) continue;
            else
            {
                Send(world, key);
                meshed++;
                if (caster) casters++;
            }
        }
        SetProfileValue("update.meshed", meshed);
        SetProfileValue("update.unseen", _unseen.Count);
    }

    // Whether the eight columns about a section's own are loaded, which its gathering reads.
    private static bool AroundLoaded(VoxelWorld world, SectionKey key)
    {
        for (int dz = -1; dz <= 1; dz++)
            for (int dx = -1; dx <= 1; dx++)
                if (!world.HasColumn(key.X + dx, key.Z + dz)) return false;
        return true;
    }

    // Whether a loaded section is within the eye's reach, one of a column not loaded being let go.
    private static bool InReach(VoxelWorld world, SectionKey key, SectionKey eye, int distance) =>
        world.TryGetColumn(key.X, key.Z, out var column) && Reach.Within(key, column.FloorSection, eye, distance);

    /// <summary>
    /// How many sections the workers are meshing or have meshed and the next update takes in.
    /// </summary>
    public int Meshing => _asking;

    // The sections sent to the meshers and not taken in yet, by how many of their meshings are out.
    private readonly Dictionary<SectionKey, int> _sent = [];

    /// <summary>
    /// Whether a section is with the meshers, sent and its mesh not taken in yet.
    /// </summary>
    public bool Sent(SectionKey key) => _sent.ContainsKey(key);

    // How many of those the workers have meshed.
    internal int Meshed => _built.Count;

    // Meshes a section here at once.
    private bool Remesh(VoxelWorld world, SectionKey key)
    {
        if (!world.HasColumn(key.X, key.Z))
        {
            Forget(key);
            return true;
        }
        _asked[key] = ++_asks;
        if (!_mesher.Mesh(world, key)) return false;
        Take(world, key, _mesher);
        return true;
    }

    // Takes a section's copy here and has a worker make its faces, which a later update takes in. A
    // section whose neighbor column has not arrived is dropped, since the neighbor's arrival marks
    // it again.
    private void Send(VoxelWorld world, SectionKey key)
    {
        if (!world.HasColumn(key.X, key.Z))
        {
            Forget(key);
            return;
        }
        var mesher = _lent.Pop();
        (mesher.Atlas, mesher.LightLevels, mesher.CornerShade, mesher.CornerStrength) = (_mesher.Atlas, _mesher.LightLevels, _mesher.CornerShade, _mesher.CornerStrength);
        var asked = _asked[key] = ++_asks;
        // A section of air has no faces to make, so it is let go here rather than wait a frame for
        // a worker, as most sections over the land are.
        var gathered = mesher.Gather(world, key);
        if (!gathered) Ungathered++;
        if (!gathered || mesher.Empty)
        {
            if (gathered) Take(world, key, mesher);
            _lent.Push(mesher);
            return;
        }
        _asking++;
        _sent[key] = _sent.GetValueOrDefault(key) + 1;
        Task.Run(async () =>
        {
            await _building.WaitAsync();
            var made = false;
            try
            {
                Building?.Invoke(key);
                mesher.Build();
                made = true;
            }
            catch (Exception ex)
            {
                // The section keeps the faces it had, and is meshed again once it is marked.
                TraceLog(LogLevel.Warning, $"Section {key} could not be meshed. {ex}");
            }
            finally
            {
                // The mesher and the count of those meshing come back whatever the build or its
                // warning did.
                _building.Release();
                _built.Enqueue((key, asked, mesher, made));
            }
        });
    }

    // Takes a section's faces from a mesher into its meshes and entities, or lets the section go
    // where it has none.
    private void Take(VoxelWorld world, SectionKey key, SectionMesher mesher)
    {
        Taking?.Invoke(key, mesher.Atlas);
        if (!world.HasColumn(key.X, key.Z))
        {
            Forget(key);
            return;
        }
        if (mesher.Solid.Count == 0 && mesher.SeeThrough.Count == 0 && mesher.Water.Count == 0 && mesher.Cutout.Count == 0 && mesher.CutoutGlass.Count == 0 && mesher.SolidCutout.Count == 0
            && mesher.Molten.Count == 0 && mesher.Foliage.Count == 0 && mesher.Emitters.Count == 0)
        {
            Forget(key);
            return;
        }
        if (!_sections.TryGetValue(key, out var entry))
            _sections[key] = entry = new SectionMeshes();

        entry.SeeThrough.Take(mesher.SeeThrough, centered: true);
        entry.Water.Take(mesher.Water, centered: true);
        entry.CutoutGlass.Take(mesher.CutoutGlass);
        Keep(key, entry.SeeThrough, SectionPart.SeeThrough, _clearLook);
        Keep(key, entry.Water, SectionPart.Water, _waterLook);
        Keep(key, entry.CutoutGlass, SectionPart.CutoutGlass, _cutoutGlassLook);
        // The opaque and cut-out parts keep their arrays for their group, which draws them.
        var change = Change.None;
        foreach (var (part, faces) in (ReadOnlySpan<(Part, SectionMesher.Faces)>)[(entry.Solid, mesher.Solid), (entry.Foliage, mesher.Foliage),
            (entry.Cutout, mesher.Cutout), (entry.SolidCutout, mesher.SolidCutout), (entry.Molten, mesher.Molten)])
        {
            var made = part.Take(faces, upload: false);
            if (made > change) change = made;
        }
        Regroup(key, change);
        _changed = true;

        entry.Emitters.Clear();
        foreach (var (index, block, faces) in mesher.Emitters)
        {
            var cell = Matrix4x4.CreateTranslation(LandOrigin.PlaceOf(key) + new Vector3(index & Section.Mask, index >> 8, (index >> 4) & Section.Mask));
            // A lamp filling its cell with no face showing, closed in on every side, is left out,
            // so no light is placed where its light reaches nothing.
            foreach (var (local, surface) in Blocks.Get(block).Glows)
                if (faces != 0 || local != WholeCell) entry.Emitters.Add((surface, local * cell));
        }
    }

    // The entity's look of a part drawn through a material, as DrawMesh would draw it.
    private Material Look(ModelMaterial material) => new(material.Color)
    {
        AlphaMode = material.AlphaMode, AlphaCutoff = material.AlphaCutoff, RoughnessFactor = material.Roughness, DoubleSided = material.DoubleSided,
        EmissiveFactor = LinearOf(material.Emissive) * material.EmissiveIntensity, CastsShadows = material.CastsShadows, BouncesLight = material.BouncesLight,
        LightThrough = material.LightThrough,
        Shader = _shader,
    };

    /// <summary>
    /// Places every section's and group's entity and every lamp again as the
    /// <see cref="LandOrigin"/> moves by a whole step, so the land drawn stands where it stood.
    /// </summary>
    /// <remarks>
    /// The meshes, the groups' faces and the lamps' cubes stay as they are, each drawn at its place
    /// moved by the step, which the engine hears of in the same frame
    /// (<see cref="E3D.MoveWorld"/>), so its scene field takes each for the same still mesh. Each
    /// place is the one under the origin before the move, which moves after this.
    /// </remarks>
    public void Shift(Vector3 by)
    {
        foreach (var (key, entry) in _sections)
        {
            foreach (var part in entry.Parts)
                if (part.Entity >= 0 && part.HasFaces) _ecs.Update(part.Entity, new Transform(LandOrigin.PlaceOf(key) + part.Center + by));
            for (int i = 0; i < entry.Emitters.Count; i++)
            {
                var (surface, at) = entry.Emitters[i];
                at.Translation += by;
                entry.Emitters[i] = (surface, at);
            }
        }
        foreach (var (at, group) in _groups)
            foreach (var part in group.Parts)
                if (part.Entity >= 0) _ecs.Update(part.Entity, new Transform(LandOrigin.PlaceOf(new SectionKey(at.X << GroupX, at.Y << GroupY, at.Z << GroupZ)) + by));
        // The lamps are gathered again at the next placement.
        _changed = true;
    }

    // Spawns a part's entity for its first mesh, gives it a mesh made again, and lets it go once
    // the part has no faces, hidden until the next placement shows it.
    private void Keep(SectionKey key, Part part, int kind, Material look)
    {
        if (!part.Mesh.IsValid)
        {
            Drop(part);
            return;
        }
        if (part.Entity < 0)
        {
            part.Entity = _ecs.Spawn();
            _ecs.Add(part.Entity, new Mesh(part.Mesh));
            _ecs.Add(part.Entity, look);
            _ecs.Add(part.Entity, new Transform(LandOrigin.PlaceOf(key) + part.Center));
            _ecs.Add(part.Entity, new Visible(false));
            _ecs.Add(part.Entity, new ShadowOnly(false));
            _ecs.Add(part.Entity, new SectionPart(key, kind));
        }
        else
        {
            if (_ecs.GetReadOnly<Mesh>(part.Entity).Uploaded.Id != part.Mesh.Id) _ecs.Update(part.Entity, new Mesh(part.Mesh));
            if (_ecs.GetReadOnly<Transform>(part.Entity).Position != LandOrigin.PlaceOf(key) + part.Center) _ecs.Update(part.Entity, new Transform(LandOrigin.PlaceOf(key) + part.Center));
        }
    }

    private void Drop(Part part)
    {
        if (part.Entity < 0) return;
        _ecs.Despawn(part.Entity);
        part.Entity = -1;
    }

    // Shows a part's entity, or draws it into the shadow maps alone, or hides it, writing only what
    // changed, so the model pass gathers again only the chunks of the sections the flood changed.
    private void Show(int entity, bool shown, bool shadowOnly)
    {
        if (entity < 0) return;
        if (_ecs.GetReadOnly<Visible>(entity).Value != shown) _ecs.Update(entity, new Visible(shown));
        if (_ecs.GetReadOnly<ShadowOnly>(entity).Value != shadowOnly) _ecs.Update(entity, new ShadowOnly(shadowOnly));
    }

    // Lets go of a section's meshes, leaving where it passes out of reach or its column unloads.
    private void Forget(SectionKey key, bool leaving = false)
    {
        _asked.Remove(key);
        if (!_sections.Remove(key, out var entry)) return;
        _changed = true;
        foreach (var part in entry.Parts) Drop(part);
        foreach (var part in entry.Parts) part.Forget();
        if (entry.Grouped.Any(part => part.HasFaces)) Regroup(key, Change.Faces, leaving);
    }

    // Marks a section's group to be made again in the next update, or its light written, the
    // greater change of its sections standing. A group marked by a section leaving stays leaving
    // until a change of another kind marks it.
    private void Regroup(SectionKey key, Change change, bool leaving = false)
    {
        if (change == Change.None) return;
        var at = GroupOf(key);
        if (!_groups.TryGetValue(at, out var group)) _groups[at] = group = new Group();
        (group.Leaving, group.Fresh) = (leaving && (group.Leaving || group.Change == Change.None), leaving);
        if (change > group.Change) group.Change = change;
    }

    // Makes each group whose sections changed again from their arrays, a mesh of each part over the
    // sections that hold it, standing at the group's corner, or writes their light where only it
    // changed, and lets a group whose sections hold nothing go. A group marked only by its sections
    // leaving and drawn nowhere waits out the first update after its marking, and then LeftAFrame
    // of those are made again an update, the nearest the eye first.
    private void Rebuild(SectionKey eye)
    {
        _left.Clear();
        _leftMade.Clear();
        foreach (var (at, group) in _groups)
        {
            if (group.Change == Change.None || !Waits(group)) continue;
            if (group.Fresh) group.Fresh = false;
            else _left.Add(at);
        }
        _left.Sort((a, b) => Far(a).CompareTo(Far(b)));
        for (int i = 0; i < _left.Count && i < LeftAFrame; i++) _leftMade.Add(_left[i]);
        List<(int X, int Y, int Z)>? empty = null;
        foreach (var (at, group) in _groups)
        {
            if (group.Change == Change.None || Waits(group) && !_leftMade.Contains(at)) continue;
            var change = group.Change;
            (group.Change, group.Leaving, group.Fresh) = (Change.None, false, false);
            _changed = true;
            var corner = new SectionKey(at.X << GroupX, at.Y << GroupY, at.Z << GroupZ);
            // The group stands at its corner kept against the origin, its sections' faces about it.
            var origin = LandOrigin.PlaceOf(corner);
            var (held, made) = (false, false);
            for (int k = 0; k < group.Parts.Length; k++)
            {
                var part = group.Parts[k];
                _members.Clear();
                for (int i = 0; i < 1 << (GroupX + GroupY + GroupZ); i++)
                {
                    var key = new SectionKey(corner.X + (i & ((1 << GroupX) - 1)), corner.Y + ((i >> GroupX) & ((1 << GroupY) - 1)), corner.Z + (i >> (GroupX + GroupY)));
                    if (_sections.TryGetValue(key, out var entry) && entry.Grouped[k].HasFaces) _members.Add((key, entry.Grouped[k]));
                }
                if (_members.Count == 0)
                {
                    DropGroupPart(part);
                    continue;
                }
                held = true;
                int vertices = 0, indices = 0;
                foreach (var (_, member) in _members) (vertices, indices) = (vertices + member.Positions.Length, indices + member.Indices.Length);
                var packed = new uint[vertices * 2];
                var colors = new Color[vertices];
                var placed = 0;
                foreach (var (_, member) in _members)
                {
                    member.Packed.CopyTo(packed, placed * 2);
                    member.Colors.CopyTo(colors, placed);
                    placed += member.Positions.Length;
                }
                // Where the light alone changed, the faces are the same and the streams are written
                // whole in place, which keeps the mesh the scene's field knows settled, the engine
                // writing a stream into the next of a ring of buffers.
                if (change == Change.Light && part.Mesh.IsValid && part.Mesh.VertexCount == vertices)
                {
                    UpdateMeshBuffer<uint>(part.Mesh, MeshStreamBuffer, packed, 0);
                    UpdateMeshBuffer<Color>(part.Mesh, 3, colors, 0);
                    continue;
                }
                var positions = new Vector3[vertices];
                var triangles = new uint[indices];
                (placed, indices) = (0, 0);
                foreach (var (key, member) in _members)
                {
                    var offset = LandOrigin.PlaceOf(key) - origin;
                    for (int v = 0; v < member.Positions.Length; v++) positions[placed + v] = member.Positions[v] + offset;
                    for (int t = 0; t < member.Indices.Length; t++) triangles[indices + t] = member.Indices[t] + (uint)placed;
                    placed += member.Positions.Length;
                    indices += member.Indices.Length;
                }
                part.Forget();
                part.Mesh = UploadMesh(positions, triangles, MeshStream.From(MeshStreamFormat.UInt2, packed.AsSpan()), colors);
                if (part.Entity < 0)
                {
                    part.Entity = _ecs.Spawn();
                    _ecs.Add(part.Entity, new Mesh(part.Mesh));
                    _ecs.Add(part.Entity, LookOf(part.Kind));
                    _ecs.Add(part.Entity, new Transform(origin));
                    _ecs.Add(part.Entity, new Visible(false));
                    _ecs.Add(part.Entity, new ShadowOnly(false));
                    _ecs.Add(part.Entity, new SectionPart(corner, part.Kind));
                }
                else _ecs.Update(part.Entity, new Mesh(part.Mesh));
                made = true;
            }
            if (made) Remade?.Invoke(corner);
            if (!held) (empty ??= []).Add(at);
        }
        if (empty is null) return;
        foreach (var at in empty) _groups.Remove(at);

        // Whether a group marked waits, marked only by its sections leaving and none of its parts
        // drawn or casting a shadow at the last placement.
        static bool Waits(Group group) => group.Leaving && !group.Parts.Any(part => part.Seen || part.Casts);
        // A group's squared distance from the eye's section across, in half sections.
        long Far((int X, int Y, int Z) at)
        {
            long dx = 2L * (at.X << GroupX) + (1 << GroupX) - 2L * eye.X - 1, dz = 2L * (at.Z << GroupZ) + (1 << GroupZ) - 2L * eye.Z - 1;
            return dx * dx + dz * dz;
        }
    }

    private readonly List<(SectionKey Key, Part Part)> _members = [];

    private Material LookOf(int kind) => kind switch
    {
        SectionPart.Solid => _terrainLook,
        SectionPart.Foliage => _foliageLook,
        SectionPart.Cutout => _cutoutLook,
        SectionPart.SolidCutout => _solidCutoutLook,
        _ => _moltenLook,
    };

    private void DropGroupPart(GroupPart part)
    {
        if (part.Entity >= 0) _ecs.Despawn(part.Entity);
        part.Entity = -1;
        part.Forget();
    }

    /// <summary>Lets go of the meshes of a column's sections, as it unloads.</summary>
    public void ForgetColumn(int x, int z)
    {
        for (int y = ChunkColumn.BottomSection; y < ChunkColumn.CeilingSection; y++)
        {
            Forget(new SectionKey(x, y, z), leaving: true);
            _parked.Remove(new SectionKey(x, y, z));
            _unseen.Remove(new SectionKey(x, y, z));
        }
    }

    /// <summary>Lets go of every section's meshes, as a new world begins.</summary>
    public void Clear()
    {
        foreach (var key in _sections.Keys.ToList()) Forget(key);
        foreach (var group in _groups.Values)
            foreach (var part in group.Parts) DropGroupPart(part);
        _groups.Clear();
        _asked.Clear();
        _parked.Clear();
        _unseen.Clear();
        _casters.Clear();
        _castFor = null;
        _reachedFor = null;
        ForgetFloods();
    }

    /// <summary>
    /// Lets go of the floods and what they release, as a world opens or the player is set at its
    /// spawn, so no section of the place a flood of the place before left out is held back unseen
    /// while it loads, a new flood seeing every section until it has flooded once.
    /// </summary>
    public void ForgetFloods()
    {
        (_visibility, _ahead, _aheadReady, _castsDue, _placedFor) = (new(), new(), false, false, null);
        _releasing.Clear();
        _released.Clear();
    }

    /// <summary>
    /// Draws the sections whose column lies within <paramref name="renderDistance"/> columns of the
    /// eye's, those the eye can see where <see cref="Cull"/> is set, the lamps in them all, and the
    /// opaque faces of those left out that stand toward the light, given by the way toward it, from
    /// those drawn within the shadow distance into the shadow maps alone.
    /// </summary>
    public void Draw(VoxelWorld world, Vector3 eye, int renderDistance, Vector3 towardLight, float shadowDistance)
    {
        // The game ticks the animated textures step by, a twentieth of a second each, as Minecraft
        // steps them.
        var ticks = HeldTicks ?? GetTime() * 20;
        SetShaderValueV<Vector4>(_shader, _animations, AnimationSteps(Atlas, _animationTable, ticks));
        int cx = LandOrigin.BlockX(eye.X) >> Section.Shift, cz = LandOrigin.BlockZ(eye.Z) >> Section.Shift;
        var laps = System.Diagnostics.Stopwatch.StartNew();
        // The sections held back as unseen that a new flood reaches or that now cast toward the
        // light, or all of them once culling is off, are meshed from the next update.
        var flooded = false;
        if (Cull && !_visibility.Flooding && !_visibility.Holds(world, eye, renderDistance, Atlas) && _ahead.Holds(world, eye, renderDistance, Atlas))
        {
            (_visibility, _ahead) = (_ahead, _visibility);
            _aheadReady = false;
            flooded = true;
            TookAhead?.Invoke(eye, _visibility.Seen);
        }
        flooded |= Cull && _visibility.Flood(world, eye, renderDistance, Atlas, FloodBudgetMs);
        if (Cull && !_visibility.Flooding) FloodAhead(world, eye, renderDistance);
        _lastEye = eye;
        // The casters of a finished flood are found in the frame after it, so no frame takes both
        // the flood and the search for its casters.
        var cast = Cull && (!flooded && _castsDue || _castFor != (Casts ? towardLight : Vector3.Zero, shadowDistance));
        _castsDue |= flooded;
        if (cast)
        {
            FindCasters(eye, towardLight, shadowDistance, (renderDistance + 1) * Section.Size);
            _castsDue = false;
        }
        if (flooded || cast || !Cull && _unseen.Count > 0)
        {
            foreach (var key in _unseen)
                if (_visibility.Sees(key) || _casters.Contains(key) || !Cull) world.Loaded.Add(key);
            _unseen.ExceptWith(world.Loaded);
        }
        var flood = laps.Elapsed.TotalMilliseconds;
        SetProfileValue("draw.sections.flood", flood);
        // The share of the frames of the last second a flood finished in.
        SetProfileValue("draw.sections.floods", flooded ? 1 : 0, 1);
        laps.Restart();
        if (flooded || cast || _changed || _placedFor != (cx, cz, renderDistance, Cull)) Place(world, cx, cz, renderDistance);
        int draws = _placedDraws, triangles = _placedTriangles, lamps = 0;
        SetProfileValue("draw.sections.opaque", laps.Elapsed.TotalMilliseconds);
        PeakMs = Math.Max(PeakMs, flood + laps.Elapsed.TotalMilliseconds);
        for (int surface = 0; surface < _emitterDraws.Length; surface++)
        {
            var list = _emitterDraws[surface];
            if (list.Count == 0) continue;
            var material = _lampSteps[surface] is { } steps ? steps[TerrainAtlas.Step(steps.Length, _lampAnimations[surface]!.Value.Ticks, ticks)] : _lamps[surface];
            DrawMeshInstanced(_lampCubes[surface], material, CollectionsMarshal.AsSpan(list));
            draws++;
            triangles += _lampCubes[surface].TriangleCount * list.Count;
            lamps += list.Count;
        }
        // The dome under the horizon, about the eye, while a haze fades the land's edge into it.
        if (Hazed)
        {
            DrawMesh(_dome, _haze, Matrix4x4.CreateTranslation(eye));
            draws++;
        }
        (Draws, Triangles, Lamps) = (draws, triangles, lamps);
        SetProfileValue("draw.sections.deep", Deep.Sections);
    }

    // Shows the sections within reach the eye can see, draws the opaque faces of those left out
    // that stand toward the light into the shadow maps alone, hides the rest, and gathers the lamps
    // of every section within reach, once the flood, the casters, the reach or a section's parts
    // change.
    private void Place(VoxelWorld world, int cx, int cz, int renderDistance)
    {
        _placedFor = (cx, cz, renderDistance, Cull);
        _changed = false;
        var reach = renderDistance * renderDistance + renderDistance;
        int draws = 0, triangles = 0, culled = 0, deep = 0, deepTriangles = 0, casting = 0, castDraws = 0;
        foreach (var list in _emitterDraws) list.Clear();
        foreach (var group in _groups.Values)
            foreach (var part in group.Parts) part.Seen = part.Casts = false;
        foreach (var (key, entry) in _sections)
        {
            int dx = key.X - cx, dz = key.Z - cz;
            var within = dx * dx + dz * dz <= reach;
            var seen = within && (!Cull || _visibility.Sees(key));
            var castsAlone = within && !seen && Casts;
            // The glass and the water, which the flood's casters leave out, are drawn only where
            // the section is seen. A group draws its parts where one of its sections holding them
            // is seen, and its parts that cast a shadow, all but the lava, into the shadow maps and
            // the scene's field alone where none is seen, so a section the flood leaves out shadows
            // and fills the field as a drawn one does, and the light of what is drawn does not hang
            // on what the eye cannot see.
            foreach (var part in (ReadOnlySpan<Part>)[entry.SeeThrough, entry.Water, entry.CutoutGlass]) Show(part.Entity, seen, false);
            if (_groups.TryGetValue(GroupOf(key), out var group))
            {
                var grouped = entry.Grouped;
                for (int k = 0; k < grouped.Length; k++)
                {
                    if (!grouped[k].HasFaces) continue;
                    group.Parts[k].Seen |= seen;
                    group.Parts[k].Casts |= castsAlone && group.Parts[k].Kind != SectionPart.Molten;
                }
            }
            if (!within) continue;
            foreach (var (surface, at) in entry.Emitters) _emitterDraws[surface].Add(at);
            if (!seen)
            {
                culled++;
                if (castsAlone) casting++;
                continue;
            }
            if (world.TryGetColumn(key.X, key.Z, out var column) && key.Y < column.FloorSection)
            {
                deep++;
                deepTriangles += entry.Solid.Triangles + entry.Cutout.Triangles + entry.CutoutGlass.Triangles
                    + entry.SolidCutout.Triangles + entry.Molten.Triangles + entry.SeeThrough.Triangles + entry.Water.Triangles;
            }
            foreach (var part in (ReadOnlySpan<Part>)[entry.SeeThrough, entry.Water, entry.CutoutGlass])
            {
                if (!part.Mesh.IsValid) continue;
                draws++;
                triangles += part.Mesh.TriangleCount;
            }
        }
        foreach (var group in _groups.Values)
            foreach (var part in group.Parts)
            {
                Show(part.Entity, part.Seen || part.Casts, !part.Seen && part.Casts);
                if (!part.Mesh.IsValid) continue;
                if (part.Seen)
                {
                    draws++;
                    triangles += part.Mesh.TriangleCount;
                }
                else if (part.Casts) castDraws++;
            }
        (_placedDraws, _placedTriangles, Culled, Deep, Casting) = (draws, triangles, culled, (deep, deepTriangles), (casting, castDraws));
        SetProfileValue("draw.sections.shadow", castDraws);
    }

    // Floods from the section the eye moves into once the eye lies within AheadBlocks of its face
    // along the motion since the last frame, at the frame's budget, so the step into it finds the
    // flood finished and draws the land it brings into reach in that frame. A turn leaves it
    // unused, and a step it does not hold what a flood from the eye would make of floods anew.
    private void FloodAhead(VoxelWorld world, Vector3 eye, int renderDistance)
    {
        var motion = eye - _lastEye;
        if (motion.LengthSquared() < 1e-8f || motion.LengthSquared() > Section.Size * Section.Size) return;
        var (axis, toFace) = (-1, 0f);
        var soonest = float.MaxValue;
        for (int a = 0; a < 3; a++)
        {
            var moved = a == 0 ? motion.X : a == 1 ? motion.Y : motion.Z;
            if (moved == 0) continue;
            var at = a == 0 ? eye.X : a == 1 ? eye.Y : eye.Z;
            // The place within the section, the land's origin lying on whole sections.
            var inside = at - MathF.Floor(at / Section.Size) * Section.Size;
            var face = moved > 0 ? Section.Size - inside : inside;
            if (face > AheadBlocks || face / MathF.Abs(moved) >= soonest) continue;
            (axis, toFace, soonest) = (a, face, face / MathF.Abs(moved));
        }
        if (axis < 0)
        {
            _aheadReady = false;
            return;
        }
        var past = (toFace + 0.01f) * MathF.Sign(axis == 0 ? motion.X : axis == 1 ? motion.Y : motion.Z);
        var ahead = eye + (axis == 0 ? new Vector3(past, 0, 0) : axis == 1 ? new Vector3(0, past, 0) : new Vector3(0, 0, past));
        // The sections the finished flood sees are meshed from the next update, those held back as
        // unseen among them ReleasedAFrame a frame, and stand in the shadow maps and the field
        // until the step draws them.
        if (_ahead.Flood(world, ahead, renderDistance, Atlas, FloodBudgetMs, spread: true))
        {
            _aheadReady = true;
            _releasing.Clear();
            foreach (var key in _unseen)
                if (_ahead.Sees(key)) _releasing.Add(key);
        }
        if (!_aheadReady) return;
        _released.RemoveWhere(key => !world.Loaded.Contains(key));
        for (var released = _released.Count; released < ReleasedAFrame && _releasing.Count > 0; _releasing.RemoveAt(_releasing.Count - 1))
            if (_unseen.Remove(_releasing[^1]))
            {
                world.Loaded.Add(_releasing[^1]);
                _released.Add(_releasing[^1]);
                released++;
            }
    }

    // Marks the sections the flood left out that a ray toward the light meets from a corner of a
    // drawn section within the shadow distance, as far across as the loaded land reaches, none
    // while the light lies under the horizon or the flood leaves nothing out.
    private void FindCasters(Vector3 eye, Vector3 towardLight, float shadowDistance, float across)
    {
        if (!Casts) towardLight = Vector3.Zero;
        _castFor = (towardLight, shadowDistance);
        _casters.Clear();
        if (!_visibility.Culled || towardLight.Y <= 0.01f) return;
        _corners.Clear();
        var reach = shadowDistance + Section.Size;
        foreach (var key in _visibility.Seen)
        {
            if (!_sections.ContainsKey(key) || Vector3.DistanceSquared(LandOrigin.PlaceOf(key) + new Vector3(Section.Size / 2f), eye) > reach * reach) continue;
            for (int corner = 0; corner < 8; corner++)
                _corners.Add(((key.X + (corner & 1)) * Section.Size, (key.Y + (corner >> 1 & 1)) * Section.Size, (key.Z + (corner >> 2)) * Section.Size));
        }
        var light = Vector3.Normalize(towardLight);
        // The rays start from the sections' corners in the land's own coordinates, multiples of 16,
        // which a float holds whole to 268 million blocks, and say which sections they cross, not a
        // point within one, so they need no origin.
        // A low light's rays run far across before they reach the top, and stop past the land.
        var longest = across / MathF.Max(1e-4f, MathF.Sqrt(light.X * light.X + light.Z * light.Z));
        Func<SectionKey, bool> sees = _visibility.Sees;
        foreach (var (x, y, z) in _corners) March(new Vector3(x, y, z), light, longest, sees, _casters);
    }

    /// <summary>
    /// Walks a ray through the sections from a point along a way that rises, to the world's top or
    /// so far along it, one section at a time as a ray through a grid is walked, adding each it
    /// meets that is not seen to <paramref name="casters"/>.
    /// </summary>
    public static void March(Vector3 from, Vector3 way, float longest, Func<SectionKey, bool> seen, HashSet<SectionKey> casters)
    {
        // A way that is no number, as a sky's hour that is none gives the sun, meets no section.
        if (float.IsNaN(way.X) || float.IsNaN(way.Y) || float.IsNaN(way.Z)) return;
        const float size = Section.Size;
        int x = (int)MathF.Floor(from.X / size), y = (int)MathF.Floor(from.Y / size), z = (int)MathF.Floor(from.Z / size);
        int stepX = MathF.Sign(way.X), stepZ = MathF.Sign(way.Z);
        float Next(float at, int cell, int step, float part) => step == 0 ? float.MaxValue : ((cell + (step > 0 ? 1 : 0)) * size - at) / part;
        float nextX = Next(from.X, x, stepX, way.X), nextY = Next(from.Y, y, 1, way.Y), nextZ = Next(from.Z, z, stepZ, way.Z);
        float acrossX = stepX == 0 ? float.MaxValue : size / MathF.Abs(way.X), acrossY = size / way.Y, acrossZ = stepZ == 0 ? float.MaxValue : size / MathF.Abs(way.Z);
        // How far along the ray the section walked into begins.
        var along = 0f;
        while (y < ChunkColumn.CeilingSection && along <= longest)
        {
            var key = new SectionKey(x, y, z);
            if (!seen(key)) casters.Add(key);
            if (nextX <= nextY && nextX <= nextZ) (x, along, nextX) = (x + stepX, nextX, nextX + acrossX);
            else if (nextY <= nextZ) (y, along, nextY) = (y + 1, nextY, nextY + acrossY);
            else (z, along, nextZ) = (z + stepZ, nextZ, nextZ + acrossZ);
        }
    }

    public void Dispose()
    {
        Clear();
        UnloadLampCubes();
        UnloadMesh(_dome);
        UnloadShader(_shader);
        if (_atlasTexture.IsValid) UnloadTexture(_atlasTexture);
        foreach (var texture in _lampTextures.Concat(_lampEmission).Concat(_lampStepEmission.SelectMany(steps => steps)))
            if (texture.IsValid) UnloadTexture(texture);
    }
}
