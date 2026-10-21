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
        public Vector3[] Positions = [];
        public uint[] Indices = [];
        public uint[] Packed = [];

        public void Take(SectionMesher.Faces faces)
        {
            var positions = CollectionsMarshal.AsSpan(faces.Positions);
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

    private sealed class SectionMeshes(Matrix4x4 transform, Vector3 middle)
    {
        public readonly Matrix4x4 Transform = transform;
        public readonly Vector3 Middle = middle;
        public readonly Part Solid = new(), SeeThrough = new(), Cutout = new();
        public readonly List<(int Surface, Matrix4x4 At)> Emitters = [];
    }

    // Every section's opaque faces are drawn with the first, their colors in their vertices, and
    // its see-through faces with the second, each vertex's alpha its surface's. An alpha under 255
    // makes the draw blend, which the vertices' alpha alone does not. The see-through faces cast no
    // shadow, which keeps them out of the scene's distance field too, since the field holds a
    // see-through mesh as solid and a window of glass would then shut out the indirect light
    // through it. A section's plants are a third mesh, cut out where a pack's texture is clear, kept
    // apart so the solid faces keep the depth test that runs before their pixels are lit, and casting
    // no shadow, so the scene's distance field holds no grass. All three are drawn through the game's
    // terrain shader, the engine's light with a haze.
    private readonly ModelMaterial _terrain, _clear, _cutout, _haze;
    // The dome under the horizon in the haze's color, drawn while there is a haze to meet it.
    private readonly ModelMesh _dome;
    private float _fogEnd = float.MaxValue;
    private readonly Shader _shader;
    private readonly int _fogColor, _fogStart, _fogEndAt, _atmosphere, _atlas, _atlasLayout;
    private Texture2D _atlasTexture;
    // The textures of the emissive blocks' cubes a pack gives one, by surface, a cube drawn with its
    // own texture where a pack has one, which the scene's distance field reads its average of, and
    // the maps of the emission of those that show parts of their textures.
    private Texture2D[] _lampTextures = [], _lampEmission = [];

    private readonly Dictionary<SectionKey, SectionMeshes> _sections = [];
    private readonly SectionMesher _mesher = new();
    private readonly ModelMesh _cube = GenMeshCube(1, 1, 1);
    // Each emissive surface's cube, the one above for a surface showing its whole texture on each face.
    private readonly ModelMesh[] _lampCubes;
    private readonly List<Matrix4x4>[] _emitterDraws = [.. Surfaces.All.Select(_ => new List<Matrix4x4>())];
    private readonly List<SectionKey> _queue = [];
    private readonly List<(float Distance, SectionMeshes Entry)> _seeThrough = [];
    private ModelMaterial[] _lamps = [];
    private float _glowScale = 1;

    public ChunkRenderer()
    {
        _shader = LoadShader("resources/shaders/terrain.slang");
        // The terrain's vertex stage keeps each corner's position as the mesh holds it and unpacks
        // only its packed bytes, so each pass leaves out the sections whose boxes it cannot see,
        // where the shadow cascades and the depth pass would draw every section within reach.
        SetShaderKeepsPositions(_shader, true);
        (_fogColor, _fogStart, _fogEndAt) = (GetShaderLocation(_shader, "fogColor"), GetShaderLocation(_shader, "fogStart"), GetShaderLocation(_shader, "fogEnd"));
        _atmosphere = GetShaderLocation(_shader, "atmosphere");
        (_atlas, _atlasLayout) = (GetShaderLocation(_shader, "atlas"), GetShaderLocation(_shader, "atlasLayout"));
        _lampCubes = [.. LampParts.Select(parts => parts is { } shown ? LampCube(shown) : _cube)];
        Atlas = TerrainAtlas.None;
        _terrain = new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Opaque, Roughness = 0.9f, Shader = _shader };
        _clear = new ModelMaterial(new Color(255, 255, 255, 254)) { AlphaMode = MaterialAlphaMode.Blend, Roughness = 0.08f, CastsShadows = false, Shader = _shader };
        _cutout = new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Mask, Roughness = 0.9f, CastsShadows = false, Shader = _shader };
        _haze = new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Blend, CastsShadows = false, Shader = _shader };
        var dome = HazeDome.Build();
        _dome = UploadMesh([.. dome.Positions], [.. dome.Indices], MeshStream.From(MeshStreamFormat.UInt2, dome.Packed.ToArray().AsSpan()), null);
        Fog(Color.White, float.MaxValue, float.MaxValue);
    }

    /// <summary>Whether the haze dome is drawn, which it is while a haze fades the land's edge short of it.</summary>
    public bool Hazed => _fogEnd < HazeDome.Radius;

    /// <summary>
    /// Sets the haze the sections fade into, its color and the distances in blocks it starts at and
    /// covers by, and the atmosphere's haze nearer than that, its most and how fast it grows, none
    /// where the most is 0.
    /// </summary>
    public void Fog(Color color, float start, float end, float atmosphere = 0, float growth = 0)
    {
        SetShaderValue(_shader, _fogColor, new Vector4(color.R, color.G, color.B, 255) / 255);
        SetShaderValue(_shader, _fogStart, start);
        SetShaderValue(_shader, _fogEndAt, end);
        SetShaderValue(_shader, _atmosphere, new Vector4(atmosphere, growth, 0, 0));
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
            SetShaderValue(_shader, _atlasLayout, new Vector4(TerrainAtlas.Columns, value.Rows, value.Pixels, 0));
            foreach (var texture in _lampTextures.Concat(_lampEmission))
                if (texture.IsValid) UnloadTexture(texture);
            _lampTextures = [.. Surfaces.All.Select(s => s.Emits && value.Picture(s.Id) is { } picture ? LoadTextureFromImage(picture) : default)];
            _lampEmission = [.. Surfaces.All.Select(s => s.Emits && LampParts[s.Id] is not null && value.Picture(s.Id) is { } picture
                ? LoadTextureFromImage(EmissionMap(picture, s)) : default)];
            foreach (var texture in _lampTextures.Concat(_lampEmission))
                if (texture.IsValid) SetTextureFilter(texture, TextureFilter.Point);
            BuildLamps();
        }
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

    private readonly Visibility _visibility = new();

    /// <summary>
    /// Whether the sections the eye cannot see through open cells are left undrawn, as
    /// <see cref="Visibility"/> floods them, the lamps' cubes drawn all the same.
    /// </summary>
    /// <remarks>
    /// A section left undrawn also leaves the sun's shadows and the scene's distance field, since a
    /// draw goes to every pass, and the field builds its cascades again as sections come into sight
    /// and go out of it. The land the flood leaves out lies behind walls the drawn sections hold, the
    /// walls of a cave or a closed room, so the eye sees the same light either way, a cave lit by a
    /// glowstone within 2 of 255 in every pixel, and a cave in a hill stays dark at noon.
    /// </remarks>
    public bool Cull { get; set; } = true;

    /// <summary>Sections the last <see cref="Draw"/> left out as unseen.</summary>
    public int Culled { get; private set; }

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

    // Each surface's material for the cubes of its emissive blocks, white under a pack's texture.
    private void BuildLamps() =>
        _lamps = [.. Surfaces.All.Select(s => _lampTextures[s.Id].IsValid
            ? s.Material(_glowScale) with { Color = Color.White, Texture = _lampTextures[s.Id], EmissiveMap = _lampEmission[s.Id] }
            : s.Material(_glowScale))];

    // The parts of its texture each emissive surface's cube shows, as the source's model gives a
    // lantern's glass and a torch's flame theirs, or null for a cube showing its whole texture on each
    // face. A surface's cubes are one instanced draw of one mesh, so its lit pieces share their parts.
    private static readonly Parts?[] LampParts = [.. Surfaces.All.Select(s =>
        Blocks.All.SelectMany(b => b.Shape?.Pieces ?? []).FirstOrDefault(p => p.Glows && p.Surface == s.Id).Parts)];

    // A cube a unit across about its middle, each face showing its part of the texture.
    private static ModelMesh LampCube(Parts parts)
    {
        var (vertices, indices) = (new ModelVertex[24], new uint[36]);
        for (int face = 0; face < 6; face++)
        {
            for (int i = 0; i < 4; i++)
            {
                var corner = SectionMesher.Corner(face, i);
                vertices[face * 4 + i] = new ModelVertex(corner - new Vector3(0.5f), SectionMesher.Normal(face), SectionMesher.Place(parts, face, corner) / 16);
            }
            var first = (uint)face * 4;
            ((ReadOnlySpan<uint>)[first, first + 1, first + 2, first, first + 2, first + 3]).CopyTo(indices.AsSpan(face * 6));
        }
        return UploadMesh(vertices, indices);
    }

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
    /// emission, so each channel is divided by the material's, its color times the surface's
    /// <see cref="Surface.Glow"/>, and encoded as sRGB, as the engine decodes an emissive map.
    /// </remarks>
    public static Image EmissionMap(Image picture, Surface surface)
    {
        var (data, map) = (picture.Data, new byte[picture.Data.Length]);
        var emitted = new Vector3(TerrainAtlas.Linear(surface.Emissive.R), TerrainAtlas.Linear(surface.Emissive.G), TerrainAtlas.Linear(surface.Emissive.B)) * surface.Glow;
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
    /// nearest the eye first, until <paramref name="budgetMs"/> of the frame is spent.
    /// </summary>
    public void Update(VoxelWorld world, Vector3 eye, double budgetMs)
    {
        foreach (var key in world.Edited)
            if (!Remesh(world, key)) world.Loaded.Add(key);
        world.Reshaded.ExceptWith(world.Edited);
        world.Edited.Clear();

        if (world.Loaded.Count == 0 && world.Reshaded.Count == 0) return;
        int ex = (int)MathF.Floor(eye.X) >> Section.Shift, ey = (int)MathF.Floor(eye.Y) >> Section.Shift, ez = (int)MathF.Floor(eye.Z) >> Section.Shift;
        _queue.Clear();
        _queue.AddRange(world.Reshaded);
        _queue.AddRange(world.Loaded);
        _queue.Sort((a, b) => Distance(a).CompareTo(Distance(b)));
        int Distance(SectionKey k) => (k.X - ex) * (k.X - ex) + (k.Y - ey) * (k.Y - ey) + (k.Z - ez) * (k.Z - ez);

        var watch = Stopwatch.StartNew();
        foreach (var key in _queue)
        {
            if (watch.Elapsed.TotalMilliseconds > budgetMs) break;
            // A section whose neighbor column has not arrived is dropped rather than kept, since the
            // neighbor's arrival marks it again.
            if (!world.Reshaded.Remove(key) & !world.Loaded.Remove(key)) continue;
            Remesh(world, key);
        }
    }

    private bool Remesh(VoxelWorld world, SectionKey key)
    {
        if (!world.HasColumn(key.X, key.Z))
        {
            Forget(key);
            return true;
        }
        if (!_mesher.Mesh(world, key)) return false;

        if (_mesher.Solid.Count == 0 && _mesher.SeeThrough.Count == 0 && _mesher.Cutout.Count == 0 && _mesher.Emitters.Count == 0)
        {
            Forget(key);
            return true;
        }
        if (!_sections.TryGetValue(key, out var entry))
            _sections[key] = entry = new SectionMeshes(Matrix4x4.CreateTranslation(key.Origin), key.Origin + new Vector3(Section.Size / 2f));

        entry.Solid.Take(_mesher.Solid);
        entry.SeeThrough.Take(_mesher.SeeThrough);
        entry.Cutout.Take(_mesher.Cutout);

        entry.Emitters.Clear();
        foreach (var (index, block) in _mesher.Emitters)
        {
            var cell = Matrix4x4.CreateTranslation(key.Origin + new Vector3(index & Section.Mask, index >> 8, (index >> 4) & Section.Mask));
            foreach (var (local, surface) in Blocks.Get(block).Glows) entry.Emitters.Add((surface, local * cell));
        }
        return true;
    }

    private void Forget(SectionKey key)
    {
        if (!_sections.Remove(key, out var entry)) return;
        entry.Solid.Forget();
        entry.SeeThrough.Forget();
        entry.Cutout.Forget();
    }

    /// <summary>Lets go of the meshes of a column's sections, as it unloads.</summary>
    public void ForgetColumn(int x, int z)
    {
        for (int y = 0; y < ChunkColumn.SectionCount; y++) Forget(new SectionKey(x, y, z));
    }

    /// <summary>Lets go of every section's meshes, as a new world begins.</summary>
    public void Clear()
    {
        foreach (var key in _sections.Keys.ToList()) Forget(key);
    }

    /// <summary>
    /// Draws the sections whose column lies within <paramref name="renderDistance"/> columns of the
    /// eye's, those the eye can see where <see cref="Cull"/> is set, and the lamps in them all.
    /// </summary>
    public void Draw(VoxelWorld world, Vector3 eye, int renderDistance)
    {
        int cx = (int)MathF.Floor(eye.X) >> Section.Shift, cz = (int)MathF.Floor(eye.Z) >> Section.Shift;
        var reach = renderDistance * renderDistance + renderDistance;
        int draws = 0, triangles = 0, lamps = 0, culled = 0;
        if (Cull) _visibility.Flood(world, eye, renderDistance);
        foreach (var list in _emitterDraws) list.Clear();
        _seeThrough.Clear();

        foreach (var (key, entry) in _sections)
        {
            int dx = key.X - cx, dz = key.Z - cz;
            if (dx * dx + dz * dz > reach) continue;
            foreach (var (surface, at) in entry.Emitters) _emitterDraws[surface].Add(at);
            if (Cull && !_visibility.Sees(key))
            {
                culled++;
                continue;
            }
            if (entry.Solid.Mesh.IsValid)
            {
                DrawMesh(entry.Solid.Mesh, _terrain, entry.Transform);
                draws++;
                triangles += entry.Solid.Mesh.TriangleCount;
            }
            if (entry.Cutout.Mesh.IsValid)
            {
                DrawMesh(entry.Cutout.Mesh, _cutout, entry.Transform);
                draws++;
                triangles += entry.Cutout.Mesh.TriangleCount;
            }
            if (entry.SeeThrough.Mesh.IsValid) _seeThrough.Add((Vector3.DistanceSquared(entry.Middle, eye), entry));
        }

        // See-through sections farthest first, so each blends over what lies behind it. The faces
        // inside one section are drawn in the order they were meshed.
        _seeThrough.Sort((a, b) => b.Distance.CompareTo(a.Distance));
        foreach (var (_, entry) in _seeThrough)
        {
            DrawMesh(entry.SeeThrough.Mesh, _clear, entry.Transform);
            draws++;
            triangles += entry.SeeThrough.Mesh.TriangleCount;
        }

        for (int surface = 0; surface < _emitterDraws.Length; surface++)
        {
            var list = _emitterDraws[surface];
            if (list.Count == 0) continue;
            DrawMeshInstanced(_lampCubes[surface], _lamps[surface], CollectionsMarshal.AsSpan(list));
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
        (Draws, Triangles, Lamps, Culled) = (draws, triangles, lamps, culled);
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
        foreach (var texture in _lampTextures.Concat(_lampEmission))
            if (texture.IsValid) UnloadTexture(texture);
    }
}
