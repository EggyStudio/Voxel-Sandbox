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
/// terrain shader unpacks into its normal, its tiles in the atlas of the resource packs and its color
/// (SectionMesher.Pack), twenty bytes a vertex on the GPU. The colors also go with the upload for the scene's distance
/// field, which reads them as the model pass reads the packed bytes, so indirect light takes each
/// block's color from the mesh it shares with its neighbors. A section meshed again with the same
/// faces keeps its mesh and takes new packed bytes and new colors in place, which keep its
/// positions, the field knowing a mesh by those, so a change of light leaves the section settled in
/// the field, which paints the section's cells in their new colors in the frame after, as the
/// ray-traced scene does its corners.
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
        // Where a blended part's entity stands in its section, the middle of its faces to the
        // nearest block, its positions held from there, since the engine draws blended entities
        // farthest first by where each stands, so a section's water and its glass each sort by
        // their own faces, a pane over the water drawn after it from above and before it from under
        // the water. A part drawn solid stands at the section's corner.
        public Vector3 Center;

        public void Take(SectionMesher.Faces faces, bool centered = false)
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
                // illumination reads, the positions kept.
                if (packed.SequenceEqual(Packed)) return;
                UpdateMeshBuffer<uint>(Mesh, MeshStreamBuffer, packed, 0);
                UpdateMeshBuffer<Color>(Mesh, 3, CollectionsMarshal.AsSpan(faces.Colors), 0);
                Packed = packed.ToArray();
                return;
            }
            Forget();
            // The store keeps the arrays it is given until the frame uploads them, so each mesh has
            // arrays of its own that nothing changes afterward. The colors go with the upload for the
            // scene's light field, which reads its colors from them.
            (Positions, Indices, Packed) = (positions.ToArray(), indices.ToArray(), packed.ToArray());
            Mesh = Positions.Length > 0 ? UploadMesh(Positions, Indices, MeshStream.From(MeshStreamFormat.UInt2, Packed.AsSpan()), faces.Colors.ToArray()) : default;
        }

        public void Forget()
        {
            if (Mesh.IsValid) UnloadMesh(Mesh);
            Mesh = default;
        }
    }

    private sealed class SectionMeshes
    {
        public readonly Part Solid = new(), SeeThrough = new(), Water = new(), Cutout = new(), CutoutGlass = new(), SolidCutout = new(), Molten = new(), Foliage = new();
        public readonly List<(int Surface, Matrix4x4 At)> Emitters = [];

        public Part[] Parts => [Solid, SeeThrough, Water, Cutout, CutoutGlass, SolidCutout, Molten, Foliage];
    }

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
    // window lets the sun through as glass the pack leaves whole does. The faces a pack cuts out that
    // cast a shadow of the whole face, a door's window and a pack's leaves clear between them, are a
    // fifth. A section's lava is a sixth, drawn with lava's emission, which the field takes from the
    // mesh, so a lake lights its cave with one draw where a lamp's cube for each block would be
    // thousands; the field reads a mesh made again without its light for a few frames, which a lake
    // seldom is. All six are drawn through the game's terrain shader, the engine's light with a haze.
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
    private readonly int _fogColor, _fogStart, _fogEndAt, _atmosphere, _hazeHeight, _eyeCovered, _atlas, _atlasLayout, _animations, _ticks, _flames, _looks;
    private Texture2D _atlasTexture;
    // The textures of the emissive blocks' cubes a pack gives one, by surface, a cube drawn with its
    // own texture where a pack has one, which the scene's distance field reads its average of, and
    // the maps of the emission of those that show parts of their textures.
    private Texture2D[] _lampTextures = [], _lampEmission = [];

    private readonly Dictionary<SectionKey, SectionMeshes> _sections = [];
    private readonly SectionMesher _mesher = new();
    // The meshers lent to the workers, a section each, its copy taken here and its faces made on a
    // worker, so the main thread takes in finished faces while the world streams in. As many are
    // made at once as half the processor's cores, so the main thread keeps a core, and a section
    // copied waits for one, so a slow frame, as on a device that draws on the processor, meshes as
    // many as the workers make meanwhile rather than one round a frame. The sections the workers made
    // come back with the meshing asked of each, and the meshing last asked of a section is kept, so a
    // section edited or asked again while a worker meshes it takes only its newest faces. An edit is
    // meshed here at once, so a block set shows in its own frame.
    private readonly Stack<SectionMesher> _lent = new(Enumerable.Range(0, 64).Select(_ => new SectionMesher()));
    private readonly SemaphoreSlim _building = new(Math.Clamp(Environment.ProcessorCount / 2, 1, 16));
    private readonly System.Collections.Concurrent.ConcurrentQueue<(SectionKey Key, int Asked, SectionMesher Mesher, bool Made)> _built = new();
    private readonly Dictionary<SectionKey, int> _asked = [];
    // Each meshing asked is numbered from one count, so faces a worker made before a section was let
    // go never pass for those asked of it after.
    private int _asks, _asking;
    private readonly ModelMesh _cube = GenMeshCube(1, 1, 1);
    // Each emissive surface's cube, the one above for a surface showing its whole texture on each face.
    private readonly ModelMesh[] _lampCubes;
    private readonly List<Matrix4x4>[] _emitterDraws = [.. Surfaces.All.Select(_ => new List<Matrix4x4>())];
    private readonly List<SectionKey> _queue = [];
    // The loaded sections out of the eye's reach, meshed once it comes near, and the eye's section
    // and render distance they were last sorted for.
    private readonly HashSet<SectionKey> _parked = [];
    // The loaded sections within reach the last flood left out, meshed once a flood reaches them,
    // since no pass draws a section the flood leaves out, so the stone under the land within reach
    // is meshed only where a cave opens to it, about 4,400 sections held in a flight over seed 1's
    // cherry grove against some 400 meshed a second.
    private readonly HashSet<SectionKey> _unseen = [];
    // The sections the flood leaves out that stand between a drawn one and the light, drawn into the
    // shadow maps alone, and the light's direction and shadow distance they were found for.
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
        (_atlas, _atlasLayout) = (GetShaderLocation(_shader, "atlas"), GetShaderLocation(_shader, "atlasLayout"));
        (_animations, _ticks) = (GetShaderLocation(_shader, "animations"), GetShaderLocation(_shader, "ticks"));
        _lampCubes = [.. Surfaces.All.Select(s => LampParts[s.Id] is { } shown ? LampCube(shown, new Vector2(16))
            : LampFaces[s.Id] is not null ? LampCube(new Parts(new Vector4(16, 0, 32, 16), new Vector4(0, 0, 16, 16), new Vector4(32, 0, 48, 16)), new Vector2(48, 16)) : _cube)];
        Atlas = TerrainAtlas.None;
        _terrain = new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Opaque, Roughness = 0.9f, Shader = _shader };
        // Glass, ice and water are rough, so they reflect neither the land the scene's distance field
        // traces nor the sky, which in the field's coarse cells clashes with the blocks' pixels.
        _clear = new ModelMaterial(new Color(255, 255, 255, 254)) { AlphaMode = MaterialAlphaMode.Blend, Roughness = 1, CastsShadows = false, Shader = _shader };
        // Water is see-through as glass is, its material's alpha of 253 telling the terrain shader
        // its faces carry the water's depth, which it darkens and clears the water by.
        _water = _clear with { Color = new Color(255, 255, 255, 253) };
        _cutoutGlass = new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Mask, Roughness = 0.9f, CastsShadows = false, Shader = _shader };
        _solidCutout = new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Mask, Roughness = 0.9f, Shader = _shader };
        (_terrainLook, _clearLook, _waterLook) = (Look(_terrain), Look(_clear), Look(_water));
        (_cutoutGlassLook, _solidCutoutLook) = (Look(_cutoutGlass), Look(_solidCutout));
        _haze = new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Blend, CastsShadows = false, Shader = _shader };
        var dome = HazeDome.Build();
        _dome = UploadMesh([.. dome.Positions], [.. dome.Indices], MeshStream.From(MeshStreamFormat.UInt2, dome.Packed.ToArray().AsSpan()), null);
        Fog(Color.White, float.MaxValue, float.MaxValue);
    }

    /// <summary>The tick the animated textures and lamps are held at, a twentieth of a second each, or null for the time's own.</summary>
    /// <remarks>
    /// A frame takes its tick from the time it is drawn at, so frames drawn at a steady pace whose
    /// length is a whole number of tenths of a second all show the same of two frames a tick each, as
    /// lavapipe's can, and a held tick shows each frame of an animation however the frames fall.
    /// </remarks>
    public double? HeldTicks { get; set; }

    /// <summary>Whether the haze dome is drawn, which it is while a haze fades the land's edge short of it.</summary>
    public bool Hazed => _fogEnd < HazeDome.Radius;

    /// <summary>
    /// Sets the haze the sections fade into, its color and the distances in blocks it starts at and
    /// covers by, and the atmosphere's haze nearer than that, its most and how fast it grows, none
    /// where the most is 0. Where the sky is behind, each point fades into the sky drawn behind it
    /// under the haze dome's band, and the color is the dome's, otherwise each fades into the color.
    /// A point takes the haze by the sky's light reaching it or the eye, whichever is higher, so a
    /// cave seen from in it takes none, the sky's light level at the eye given, or the open sky's.
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
            var table = new Vector4[TerrainAtlas.MostAnimations];
            for (int i = 0; i < value.Animations.Count; i++) table[i] = new Vector4(value.Animations[i].First, value.Animations[i].Count, value.Animations[i].Ticks, 0);
            SetShaderValueV<Vector4>(_shader, _animations, table);
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
        foreach (var entry in _sections.Values)
        {
            if (entry.Foliage.Entity >= 0) _ecs.Update(entry.Foliage.Entity, _foliageLook);
            if (entry.Cutout.Entity >= 0) _ecs.Update(entry.Cutout.Entity, _cutoutLook);
        }
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

    /// <summary>The share of global illumination's light passing a unit of a leaf block's depth, and of a plant's.</summary>
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
    /// The color is the surface's own rather than one read from a pack's texture. The mean of a
    /// pack's lamp's pixels by how much each emits is far paler, Bare Bones's glowstone 255, 228,
    /// 133 against 255, 200, 120 and its soul lantern 186, 252, 255 against 70, 210, 255, since its
    /// brightest pixels are nearly white, and graded at a saturation of 1.4 the glowstone's turned a
    /// white room olive.
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

    /// <summary>How much brighter than its face the rim along each whole block's edges is drawn, 0 for none.</summary>
    public float Rim
    {
        get => _rim;
        set
        {
            _rim = Math.Max(0, value);
            SetLooks();
        }
    }

    /// <summary>Whether every open face takes the sun's light as one facing the sun does, for a flat look with its shadows kept.</summary>
    public bool Unshaded
    {
        get => _unshaded;
        set
        {
            _unshaded = value;
            SetLooks();
        }
    }

    private void SetLooks() => SetShaderValue(_shader, _looks, new Vector4(_rim, _unshaded ? 1 : 0, 0, 0));

    /// <summary>
    /// The light an emissive surface gives at a glow of 1 in linear light, its surface's emission, or
    /// white as bright where <see cref="ColoredLamps"/> is off.
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

    /// <summary>A light in linear light as the color of its hue at its brightest and how many times that it is, so no part of it clips in a color's bytes.</summary>
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

    private readonly Visibility _visibility = new();

    /// <summary>
    /// Whether the sections the eye cannot see through open cells are left undrawn, as
    /// <see cref="Visibility"/> floods them, the lamps' cubes drawn all the same.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A section left undrawn also leaves the scene's distance field, which builds its cascades again
    /// as sections come into sight and go out of it. The land the flood leaves out lies behind walls
    /// the drawn sections hold, the walls of a cave or a closed room, so the eye sees the same light
    /// either way, a cave lit by a glowstone within 2 of 255 in every pixel, and a cave in a hill
    /// stays dark at noon.
    /// </para>
    /// <para>
    /// The sun's shadow is another matter, since the rock over a cave stands between the light and
    /// what the eye sees there. So the sections the flood leaves out that lie toward the light from
    /// a drawn section within the shadow distance are drawn into the shadow maps alone, through the
    /// engine's shadow-only draw, their opaque faces casting and seen nowhere. They are found by rays
    /// toward the light from each corner of every drawn section within the shadow distance, through
    /// the sections to the world's top, again when a flood changes what is drawn, the light turns or
    /// the shadow distance changes, and are meshed as the drawn ones are.
    /// </para>
    /// </remarks>
    public bool Cull { get; set; } = true;

    /// <summary>Sections the last <see cref="Draw"/> left out as unseen.</summary>
    public int Culled { get; private set; }

    /// <summary>Whether the sections the flood leaves out cast the light's shadow where they stand toward it, as <see cref="Cull"/> says, true unless set.</summary>
    public bool Casts { get; set; } = true;

    /// <summary>Sections the last <see cref="Draw"/> drew into the shadow maps alone, of those it left out, and the draws it made of them.</summary>
    public (int Sections, int Draws) Casting { get; private set; }

    /// <summary>Deep sections the last <see cref="Draw"/> drew, those under their column's floor, and their triangles.</summary>
    public (int Sections, int Triangles) Deep { get; private set; }

    /// <summary>Draw calls the last <see cref="Draw"/> made, each instanced draw of lamps counted once.</summary>
    public int Draws { get; private set; }

    public int Triangles { get; private set; }

    public int Lamps { get; private set; }

    /// <summary>Sections holding faces or lamps.</summary>
    public int Sections => _sections.Count;

    /// <summary>Whether faces are darkened by light levels, which takes effect as sections are shaded again.</summary>
    public bool LightLevels
    {
        get => _mesher.LightLevels;
        set => _mesher.LightLevels = value;
    }

    /// <summary>Whether corners are darkened where blocks meet, which takes effect as sections are shaded again.</summary>
    public bool CornerShade
    {
        get => _mesher.CornerShade;
        set => _mesher.CornerShade = value;
    }

    /// <summary>How far corners darken where blocks meet, from 0 to Minecraft's levels at 1, which takes effect as sections are shaded again.</summary>
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
        foreach (var entry in _sections.Values)
            if (entry.Molten.Entity >= 0) _ecs.Update(entry.Molten.Entity, _moltenLook);
        _lamps = [.. Surfaces.All.Select(s => _lampTextures[s.Id].IsValid
            ? Lamp(s) with { Color = Color.White, Texture = _lampTextures[s.Id], EmissiveMap = _lampEmission[s.Id] }
            : s.Unseen ? Lamp(s) with { AlphaMode = MaterialAlphaMode.Mask, Color = s.Color with { A = 0 } }
            : Lamp(s))];
        _lampSteps = [.. Surfaces.All.Select(s => _lampStepEmission[s.Id].Length == 0 ? null : _lampStepEmission[s.Id].Select(map => _lamps[s.Id] with { EmissiveMap = map }).ToArray())];
    }

    // The parts of its texture each emissive surface's cube shows, as the source's model gives a
    // lantern's glass and a torch's flame theirs, or null for a cube showing its whole texture on each
    // face. A surface's cubes are one instanced draw of one mesh, so its lit pieces share their parts.
    private static readonly Parts?[] LampParts = [.. Surfaces.All.Select(s =>
        Blocks.All.SelectMany(b => b.Shape?.Pieces ?? []).FirstOrDefault(p => p.Glows && p.Surface == s.Id).Parts)];

    // Each emissive surface's whole lamp whose faces show more than one surface, its top's, its
    // sides' and its bottom's, as a lit furnace's do, or null for one showing the one on every face.
    private static readonly (int Top, int Side, int Bottom)?[] LampFaces = [.. Surfaces.All.Select(s =>
        Blocks.All.FirstOrDefault(b => b.Shape is null && b.Emits && b.Lit == s.Id && !(b.Top == b.Side && b.Side == b.Bottom)) is { } lamp
            ? ((int, int, int)?)(lamp.Top, lamp.Side, lamp.Bottom) : null)];

    /// <summary>
    /// A lamp's faces' pictures side by side, its top's, its sides' and its bottom's, each a pack's
    /// texture or a tile of the surface's own color, or with <paramref name="glow"/> the map of their
    /// emission, an emissive surface's picture's bright pixels as <see cref="EmissionMap"/> lights them
    /// against <paramref name="light"/> or its own color's whole, and nothing of the rest, so a lit
    /// furnace's top shows the furnace's and its front alone glows.
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

    // A cube a unit across about its middle, each face showing its part of a texture of a size in pixels.
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

    /// <summary>How many times their own color a fire's flames show, whatever light reaches them.</summary>
    public const float FlamesShown = 2;

    /// <summary>How many times its own color a pack's lava shows, whatever light reaches it, where without a pack it shows its surface's emission over its light.</summary>
    public const float LavaShown = 2;

    /// <summary>How many times its own color a lit piece's bright pixels show, a lantern's glass or a torch's flame.</summary>
    public const float LampShown = 4;

    /// <summary>
    /// The map of the emission of a lit piece that shows parts of its texture, so its bright pixels
    /// emit <see cref="LampShown"/> times their own color and its dark ones, a lantern's iron and a
    /// torch's stick, show as they are lit, where the material's emission over every pixel would draw
    /// the iron as bright as the flame. The scene's distance field takes the material's emission
    /// whole, so the light the lamp casts is the same.
    /// </summary>
    /// <remarks>
    /// A pixel emits by the eighth power of its brightest channel, so iron at 0.4 keeps under a
    /// thousandth of the emission and a flame's yellow most of it. The map multiplies the material's
    /// emission, so each channel is divided by the material's, its light, the surface's emission
    /// unless <paramref name="light"/> is given, times the surface's <see cref="Surface.Glow"/>, and
    /// encoded as sRGB, as the engine decodes an emissive map.
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
    /// Meshes every section an edit changed, then those whose shading changed and those that loaded,
    /// nearest the eye first, until <paramref name="budgetMs"/> of the frame is spent. A section out
    /// of the eye's reach, as <see cref="Reach"/> has it at <paramref name="renderDistance"/>, is set
    /// aside and meshed once the eye comes near, and a deep one's meshes are let go a section past it.
    /// </summary>
    public void Update(VoxelWorld world, Vector3 eye, int renderDistance, double budgetMs)
    {
        while (_built.TryDequeue(out var done))
        {
            _asking--;
            _lent.Push(done.Mesher);
            if (done.Made && _asked.TryGetValue(done.Key, out var asked) && asked == done.Asked) Take(world, done.Key, done.Mesher);
        }
        foreach (var key in world.Edited)
            if (!Remesh(world, key)) world.Loaded.Add(key);
        world.Reshaded.ExceptWith(world.Edited);
        world.Edited.Clear();

        var at = Reach.Of(eye);
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
                Forget(key);
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
        var meshed = 0;
        foreach (var key in _queue)
        {
            if (watch.Elapsed.TotalMilliseconds > budgetMs) break;
            // A section out of reach is parked, one the flood leaves out held back and one of air let
            // go, each at once, while one to mesh waits marked for a mesher to come back once every
            // one is lent.
            var far = !InReach(world, key, at, renderDistance);
            var unseen = !far && Cull && !_visibility.Sees(key) && !_casters.Contains(key);
            var empty = !far && !unseen && (!world.TryGetColumn(key.X, key.Z, out var column) || column.Numbered(key.Y) is not { Filled: > 0 });
            if (!far && !unseen && !empty && _lent.Count == 0) continue;
            // A section whose neighbor column has not arrived is dropped rather than kept, since the
            // neighbor's arrival marks it again.
            if (!world.Reshaded.Remove(key) & !world.Loaded.Remove(key)) continue;
            if (far) _parked.Add(key);
            else if (unseen) _unseen.Add(key);
            else if (empty) Forget(key);
            else
            {
                Send(world, key);
                meshed++;
            }
        }
        SetProfileValue("update.meshed", meshed);
        SetProfileValue("update.unseen", _unseen.Count);
    }

    // Whether a loaded section is within the eye's reach, one of a column not loaded being let go.
    private static bool InReach(VoxelWorld world, SectionKey key, SectionKey eye, int distance) =>
        world.TryGetColumn(key.X, key.Z, out var column) && Reach.Within(key, column.FloorSection, eye, distance);

    /// <summary>How many sections the workers are meshing or have meshed and the next update takes in.</summary>
    public int Meshing => _asking;

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
    // section whose neighbor column has not arrived is dropped, since the neighbor's arrival marks it
    // again.
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
        // A section of air has no faces to make, so it is let go here rather than wait a frame for a
        // worker, as most sections over the land are.
        var gathered = mesher.Gather(world, key);
        if (!gathered || mesher.Empty)
        {
            if (gathered) Take(world, key, mesher);
            _lent.Push(mesher);
            return;
        }
        _asking++;
        Task.Run(async () =>
        {
            await _building.WaitAsync();
            var made = true;
            try
            {
                mesher.Build();
            }
            catch (Exception ex)
            {
                // The section keeps the faces it had, and is meshed again once it is marked.
                Console.Error.WriteLine($"Section {key} could not be meshed. {ex}");
                made = false;
            }
            finally
            {
                _building.Release();
            }
            _built.Enqueue((key, asked, mesher, made));
        });
    }

    // Takes a section's faces from a mesher into its meshes and entities, or lets the section go
    // where it has none.
    private void Take(VoxelWorld world, SectionKey key, SectionMesher mesher)
    {
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

        entry.Solid.Take(mesher.Solid);
        entry.SeeThrough.Take(mesher.SeeThrough, centered: true);
        entry.Water.Take(mesher.Water, centered: true);
        entry.Cutout.Take(mesher.Cutout);
        entry.CutoutGlass.Take(mesher.CutoutGlass);
        entry.SolidCutout.Take(mesher.SolidCutout);
        entry.Molten.Take(mesher.Molten);
        entry.Foliage.Take(mesher.Foliage);
        Keep(key, entry.Solid, SectionPart.Solid, _terrainLook);
        Keep(key, entry.Foliage, SectionPart.Foliage, _foliageLook);
        Keep(key, entry.SeeThrough, SectionPart.SeeThrough, _clearLook);
        Keep(key, entry.Water, SectionPart.Water, _waterLook);
        Keep(key, entry.Cutout, SectionPart.Cutout, _cutoutLook);
        Keep(key, entry.CutoutGlass, SectionPart.CutoutGlass, _cutoutGlassLook);
        Keep(key, entry.SolidCutout, SectionPart.SolidCutout, _solidCutoutLook);
        Keep(key, entry.Molten, SectionPart.Molten, _moltenLook);
        _changed = true;

        entry.Emitters.Clear();
        foreach (var (index, block) in mesher.Emitters)
        {
            var cell = Matrix4x4.CreateTranslation(key.Origin + new Vector3(index & Section.Mask, index >> 8, (index >> 4) & Section.Mask));
            foreach (var (local, surface) in Blocks.Get(block).Glows) entry.Emitters.Add((surface, local * cell));
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
            _ecs.Add(part.Entity, new Transform(key.Origin + part.Center));
            _ecs.Add(part.Entity, new Visible(false));
            _ecs.Add(part.Entity, new ShadowOnly(false));
            _ecs.Add(part.Entity, new SectionPart(key, kind));
        }
        else
        {
            if (_ecs.GetReadOnly<Mesh>(part.Entity).Uploaded.Id != part.Mesh.Id) _ecs.Update(part.Entity, new Mesh(part.Mesh));
            if (_ecs.GetReadOnly<Transform>(part.Entity).Position != key.Origin + part.Center) _ecs.Update(part.Entity, new Transform(key.Origin + part.Center));
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
    private void Show(Part part, bool shown, bool shadowOnly)
    {
        if (part.Entity < 0) return;
        if (_ecs.GetReadOnly<Visible>(part.Entity).Value != shown) _ecs.Update(part.Entity, new Visible(shown));
        if (_ecs.GetReadOnly<ShadowOnly>(part.Entity).Value != shadowOnly) _ecs.Update(part.Entity, new ShadowOnly(shadowOnly));
    }

    private void Forget(SectionKey key)
    {
        _asked.Remove(key);
        if (!_sections.Remove(key, out var entry)) return;
        _changed = true;
        foreach (var part in entry.Parts) Drop(part);
        foreach (var part in entry.Parts) part.Forget();
    }

    /// <summary>Lets go of the meshes of a column's sections, as it unloads.</summary>
    public void ForgetColumn(int x, int z)
    {
        for (int y = ChunkColumn.BottomSection; y < ChunkColumn.CeilingSection; y++)
        {
            Forget(new SectionKey(x, y, z));
            _parked.Remove(new SectionKey(x, y, z));
            _unseen.Remove(new SectionKey(x, y, z));
        }
    }

    /// <summary>Lets go of every section's meshes, as a new world begins.</summary>
    public void Clear()
    {
        foreach (var key in _sections.Keys.ToList()) Forget(key);
        _asked.Clear();
        _parked.Clear();
        _unseen.Clear();
        _casters.Clear();
        _castFor = null;
        _reachedFor = null;
    }

    /// <summary>
    /// Draws the sections whose column lies within <paramref name="renderDistance"/> columns of the
    /// eye's, those the eye can see where <see cref="Cull"/> is set, the lamps in them all, and the
    /// opaque faces of those left out that stand toward the light, given by the way toward it, from
    /// those drawn within the shadow distance into the shadow maps alone.
    /// </summary>
    public void Draw(VoxelWorld world, Vector3 eye, int renderDistance, Vector3 towardLight, float shadowDistance)
    {
        // The game ticks the animated textures step by, a twentieth of a second each, as Minecraft steps them.
        var ticks = HeldTicks ?? GetTime() * 20;
        SetShaderValue(_shader, _ticks, (float)ticks);
        int cx = (int)MathF.Floor(eye.X) >> Section.Shift, cz = (int)MathF.Floor(eye.Z) >> Section.Shift;
        var laps = System.Diagnostics.Stopwatch.StartNew();
        // The sections held back as unseen that a new flood reaches or that now cast toward the light,
        // or all of them once culling is off, are meshed from the next update.
        var flooded = Cull && _visibility.Flood(world, eye, renderDistance, Atlas);
        var cast = Cull && (flooded || _castFor != (Casts ? towardLight : Vector3.Zero, shadowDistance));
        if (cast) FindCasters(eye, towardLight, shadowDistance, (renderDistance + 1) * Section.Size);
        if (flooded || cast || !Cull && _unseen.Count > 0)
        {
            foreach (var key in _unseen)
                if (_visibility.Sees(key) || _casters.Contains(key) || !Cull) world.Loaded.Add(key);
            _unseen.ExceptWith(world.Loaded);
        }
        SetProfileValue("draw.sections.flood", laps.Elapsed.TotalMilliseconds);
        laps.Restart();
        if (flooded || cast || _changed || _placedFor != (cx, cz, renderDistance, Cull)) Place(world, cx, cz, renderDistance);
        int draws = _placedDraws, triangles = _placedTriangles, lamps = 0;
        SetProfileValue("draw.sections.opaque", laps.Elapsed.TotalMilliseconds);
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

    // Shows the sections within reach the eye can see, draws the opaque faces of those left out that
    // stand toward the light into the shadow maps alone, hides the rest, and gathers the lamps of
    // every section within reach, once the flood, the casters, the reach or a section's parts change.
    private void Place(VoxelWorld world, int cx, int cz, int renderDistance)
    {
        _placedFor = (cx, cz, renderDistance, Cull);
        _changed = false;
        var reach = renderDistance * renderDistance + renderDistance;
        int draws = 0, triangles = 0, culled = 0, deep = 0, deepTriangles = 0, casting = 0, castDraws = 0;
        foreach (var list in _emitterDraws) list.Clear();
        foreach (var (key, entry) in _sections)
        {
            int dx = key.X - cx, dz = key.Z - cz;
            var within = dx * dx + dz * dz <= reach;
            var seen = within && (!Cull || _visibility.Sees(key));
            var castsAlone = within && !seen && _casters.Contains(key);
            // The parts that cast a shadow are drawn into the shadow maps alone where the section
            // casts alone, and the glass, the water and the lava, which the flood's casters leave
            // out, only where it is seen.
            foreach (var part in (ReadOnlySpan<Part>)[entry.Solid, entry.Foliage, entry.Cutout, entry.SolidCutout])
                Show(part, seen || castsAlone, castsAlone);
            foreach (var part in (ReadOnlySpan<Part>)[entry.SeeThrough, entry.Water, entry.CutoutGlass, entry.Molten])
                Show(part, seen, false);
            if (!within) continue;
            foreach (var (surface, at) in entry.Emitters) _emitterDraws[surface].Add(at);
            if (!seen)
            {
                culled++;
                if (!castsAlone) continue;
                casting++;
                foreach (var part in (ReadOnlySpan<Part>)[entry.Solid, entry.Foliage, entry.Cutout, entry.SolidCutout])
                    if (part.Mesh.IsValid) castDraws++;
                continue;
            }
            if (world.TryGetColumn(key.X, key.Z, out var column) && key.Y < column.FloorSection)
            {
                deep++;
                deepTriangles += entry.Solid.Mesh.TriangleCount + entry.Cutout.Mesh.TriangleCount + entry.CutoutGlass.Mesh.TriangleCount
                    + entry.SolidCutout.Mesh.TriangleCount + entry.Molten.Mesh.TriangleCount + entry.SeeThrough.Mesh.TriangleCount + entry.Water.Mesh.TriangleCount;
            }
            foreach (var part in entry.Parts)
            {
                if (!part.Mesh.IsValid) continue;
                draws++;
                triangles += part.Mesh.TriangleCount;
            }
        }
        (_placedDraws, _placedTriangles, Culled, Deep, Casting) = (draws, triangles, culled, (deep, deepTriangles), (casting, castDraws));
        SetProfileValue("draw.sections.shadow", castDraws);
    }

    // Marks the sections the flood left out that a ray toward the light meets from a corner of a
    // drawn section within the shadow distance, as far across as the loaded land reaches, none while
    // the light lies under the horizon or the flood leaves nothing out.
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
            if (!_sections.ContainsKey(key) || Vector3.DistanceSquared(key.Origin + new Vector3(Section.Size / 2f), eye) > reach * reach) continue;
            for (int corner = 0; corner < 8; corner++)
                _corners.Add(((key.X + (corner & 1)) * Section.Size, (key.Y + (corner >> 1 & 1)) * Section.Size, (key.Z + (corner >> 2)) * Section.Size));
        }
        var light = Vector3.Normalize(towardLight);
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
        UnloadMesh(_cube);
        foreach (var cube in _lampCubes)
            if (cube.Id != _cube.Id) UnloadMesh(cube);
        UnloadMesh(_dome);
        UnloadShader(_shader);
        if (_atlasTexture.IsValid) UnloadTexture(_atlasTexture);
        foreach (var texture in _lampTextures.Concat(_lampEmission).Concat(_lampStepEmission.SelectMany(steps => steps)))
            if (texture.IsValid) UnloadTexture(texture);
    }
}
