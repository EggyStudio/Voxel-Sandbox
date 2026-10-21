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
