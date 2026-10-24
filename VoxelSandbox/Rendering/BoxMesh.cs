using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>Builds a mesh of boxes with a color on each face, as a mob's pieces and a stack on the ground are.</summary>
public sealed class BoxMesh
{
    // The corners of a box's six faces, +x, -x, +y, -y, +z and -z, as fractions of its size, each
    // face's two triangles turning counterclockwise seen from outside.
    private static readonly Vector3[][] Corners =
    [
        [new(1, 0, 0), new(1, 1, 0), new(1, 1, 1), new(1, 0, 1)],
        [new(0, 0, 0), new(0, 0, 1), new(0, 1, 1), new(0, 1, 0)],
        [new(0, 1, 0), new(0, 1, 1), new(1, 1, 1), new(1, 1, 0)],
        [new(0, 0, 0), new(1, 0, 0), new(1, 0, 1), new(0, 0, 1)],
        [new(0, 0, 1), new(1, 0, 1), new(1, 1, 1), new(0, 1, 1)],
        [new(0, 0, 0), new(0, 1, 0), new(1, 1, 0), new(1, 0, 0)],
    ];

    private static readonly Vector3[] Normals = [Vector3.UnitX, -Vector3.UnitX, Vector3.UnitY, -Vector3.UnitY, Vector3.UnitZ, -Vector3.UnitZ];

    private static readonly Vector2[] Uvs = [new(0, 1), new(0, 0), new(1, 0), new(1, 1)];

    private readonly List<ModelVertex> _vertices = [];

    /// <summary>The vertices added, each face's four in turn.</summary>
    public IReadOnlyList<ModelVertex> Vertices => _vertices;

    private readonly List<Color> _colors = [];

    /// <summary>Each vertex's color, as <see cref="Vertices"/> holds them.</summary>
    public IReadOnlyList<Color> Colors => _colors;
    private readonly List<uint> _indices = [];

    /// <summary>Adds a box between two corners in one color, its corners at one place on the texture where one is given.</summary>
    public void Add(Vector3 min, Vector3 max, Color color, Vector2? place = null) => Add(min, max, [color, color, color, color, color, color], place);

    /// <summary>Adds a box between two corners, each face its own color, in the order +x, -x, +y, -y, +z and -z.</summary>
    public void Add(Vector3 min, Vector3 max, ReadOnlySpan<Color> faces, Vector2? place = null)
    {
        var size = max - min;
        for (int face = 0; face < 6; face++)
        {
            var first = (uint)_vertices.Count;
            for (int i = 0; i < 4; i++)
            {
                _vertices.Add(new ModelVertex(min + Corners[face][i] * size, Normals[face], place ?? Uvs[i]));
                _colors.Add(faces[face]);
            }
            _indices.AddRange([first, first + 1, first + 2, first, first + 2, first + 3]);
        }
    }

    /// <summary>
    /// Adds a box between two corners in one color, each face showing its own part of the texture,
    /// from its upper left corner to its lower right, in the order +x, -x, +y, -y, +z and -z, laid as
    /// the terrain shader lays a block's texture on its faces, so a side stands upright.
    /// </summary>
    public void Add(Vector3 min, Vector3 max, Color color, ReadOnlySpan<(Vector2 From, Vector2 To)> parts)
    {
        var size = max - min;
        for (int face = 0; face < 6; face++)
        {
            var first = (uint)_vertices.Count;
            var (from, to) = parts[face];
            for (int i = 0; i < 4; i++)
            {
                var corner = Corners[face][i];
                _vertices.Add(new ModelVertex(min + corner * size, Normals[face], from + Laid(face, corner) * (to - from)));
                _colors.Add(color);
            }
            _indices.AddRange([first, first + 1, first + 2, first, first + 2, first + 3]);
        }
    }

    // A corner's place across its face's texture, right and down seen from outside, as the terrain
    // shader's across lays it.
    private static Vector2 Laid(int face, Vector3 corner) => face switch
    {
        0 => new(1 - corner.Z, 1 - corner.Y),
        1 => new(corner.Z, 1 - corner.Y),
        2 => new(corner.X, corner.Z),
        3 => new(corner.X, 1 - corner.Z),
        4 => new(corner.X, 1 - corner.Y),
        _ => new(1 - corner.X, 1 - corner.Y),
    };

    /// <summary>Adds a face of four corners counterclockwise seen from outside, each at its place on the texture, in one color.</summary>
    public void AddFace(ReadOnlySpan<Vector3> corners, ReadOnlySpan<Vector2> places, Color color)
    {
        var normal = Vector3.Normalize(Vector3.Cross(corners[1] - corners[0], corners[2] - corners[0]));
        var first = (uint)_vertices.Count;
        for (int i = 0; i < 4; i++)
        {
            _vertices.Add(new ModelVertex(corners[i], normal, places[i]));
            _colors.Add(color);
        }
        _indices.AddRange([first, first + 1, first + 2, first, first + 2, first + 3]);
    }

    /// <summary>Uploads the boxes as one mesh with a color at each vertex.</summary>
    public ModelMesh Upload() => UploadMesh([.. _vertices], [.. _indices], [.. _colors], null);
}
