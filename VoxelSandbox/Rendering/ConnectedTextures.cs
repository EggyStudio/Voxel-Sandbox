namespace VoxelSandbox;

/// <summary>How a connected texture's tile is chosen for a face, by OptiFine's methods of the same names.</summary>
public enum ConnectMethod
{
    /// <summary>OptiFine's <c>ctm</c>, 47 tiles chosen by the face's eight neighbors in its plane.</summary>
    Full,

    /// <summary>Four tiles by the neighbors on the left and the right.</summary>
    Horizontal,

    /// <summary>Four tiles by the neighbors below and above.</summary>
    Vertical,

    /// <summary>One tile on a side whose block has the same block over it.</summary>
    Top,

    /// <summary>A sheet of tiles laid across the faces by their place, so many across and down.</summary>
    Repeat,

    /// <summary>A tile at random by the face's place, weighed where weights are given.</summary>
    Random,
}

/// <summary>
/// A pack's connected texture, read from one of OptiFine's <c>.properties</c> files under its
/// <c>optifine/ctm</c> folder as a picture's metadata, its method, its tiles' paths in its pack, which
/// blocks or textures it takes, and how it joins.
/// </summary>
/// <param name="Method">How a face's tile is chosen.</param>
/// <param name="Tiles">The tiles' paths under <c>assets/minecraft</c>, each a picture, or null for the face's own texture, as <c>&lt;default&gt;</c> names it.</param>
/// <param name="Blocks">The blocks it takes by their keys, as <c>matchBlocks</c> names them.</param>
/// <param name="Textures">The textures it takes by their paths under <c>textures</c>, as <c>matchTiles</c> names them.</param>
/// <param name="Width">A repeated sheet's tiles across, 1 for another method.</param>
/// <param name="Height">A repeated sheet's tiles down.</param>
/// <param name="Weights">Each random tile's weight, or none for tiles alike.</param>
/// <param name="ByTile">Whether a face joins the faces of the same texture beside it, as <c>connect=tile</c> says, rather than those of the same block.</param>
/// <param name="InnerSeams">Whether a face joins a neighbor only where the cell in front of the neighbor is not the same, as <c>innerSeams</c> says.</param>
/// <param name="Faces">The faces it takes, a bit for each in the mesher's order, east, west, up, down, south and north.</param>
public sealed record ConnectedRule(ConnectMethod Method, string?[] Tiles, IReadOnlySet<string> Blocks, IReadOnlySet<string> Textures, int Width, int Height,
    int[] Weights, bool ByTile, bool InnerSeams, int Faces);

/// <summary>
/// The connected textures of OptiFine's form a pack gives, read and chosen as the game's own, the
/// properties parsed to their method and tiles, and each method's choice of a face's tile by its
/// neighbors or its place.
/// </summary>
/// <remarks>
/// <para>
/// A face's left, below, right and above are those of its texture as the source lays one on it, the
/// north up on a top and the south on a bottom, each side's up the sky's, and its left as one facing
/// the face from outside sees it. A neighbor joins where it is the same block, or for a
/// rule of textures where it shows the same texture on that side, and a corner joins only where both
/// its sides do. The full method's tile is the template's for those eight, the table below by the
/// neighbors' bits, the left 1, the lower left corner 2, the lower 4, the lower right 8, the right 16,
/// the upper right 32, the upper 64 and the upper left 128, as OptiFine lays its 47 tiles, which
/// MCPatcher and Continuity read alike.
/// </para>
/// <para>
/// The methods <c>ctm_compact</c>, <c>overlay</c>, <c>fixed</c> and those joined with a plus are not
/// read, and a rule naming one is left out, so its blocks keep their own textures.
/// </para>
/// </remarks>
public static class ConnectedTextures
{
    // The full method's tile by the eight neighbors' bits, as the template lays its 47 tiles.
    private static readonly byte[] Full =
    [
        0, 3, 0, 3, 12, 5, 12, 15, 0, 3, 0, 3, 12, 5, 12, 15,
        1, 2, 1, 2, 4, 7, 4, 29, 1, 2, 1, 2, 13, 31, 13, 14,
        0, 3, 0, 3, 12, 5, 12, 15, 0, 3, 0, 3, 12, 5, 12, 15,
        1, 2, 1, 2, 4, 7, 4, 29, 1, 2, 1, 2, 13, 31, 13, 14,
        36, 17, 36, 17, 24, 19, 24, 43, 36, 17, 36, 17, 24, 19, 24, 43,
        16, 18, 16, 18, 6, 46, 6, 21, 16, 18, 16, 18, 28, 9, 28, 22,
        36, 17, 36, 17, 24, 19, 24, 43, 36, 17, 36, 17, 24, 19, 24, 43,
        37, 40, 37, 40, 30, 8, 30, 34, 37, 40, 37, 40, 25, 23, 25, 45,
        0, 3, 0, 3, 12, 5, 12, 15, 0, 3, 0, 3, 12, 5, 12, 15,
        1, 2, 1, 2, 4, 7, 4, 29, 1, 2, 1, 2, 13, 31, 13, 14,
        0, 3, 0, 3, 12, 5, 12, 15, 0, 3, 0, 3, 12, 5, 12, 15,
        1, 2, 1, 2, 4, 7, 4, 29, 1, 2, 1, 2, 13, 31, 13, 14,
        36, 39, 36, 39, 24, 41, 24, 27, 36, 39, 36, 39, 24, 41, 24, 27,
        16, 42, 16, 42, 6, 20, 6, 10, 16, 42, 16, 42, 28, 35, 28, 44,
        36, 39, 36, 39, 24, 41, 24, 27, 36, 39, 36, 39, 24, 41, 24, 27,
        37, 38, 37, 38, 30, 11, 30, 32, 37, 38, 37, 38, 25, 33, 25, 26,
    ];

    // The tile of the horizontal and the vertical methods by their two neighbors, the first 1 and the second 2.
    private static readonly byte[] Pair = [3, 2, 0, 1];

    /// <summary>The full method's tile by which of the face's sides and corners join, as their bits say, a corner joining only where both its sides do.</summary>
    public static int FullTile(bool left, bool down, bool right, bool up, bool downLeft, bool downRight, bool upRight, bool upLeft)
    {
        var bits = (left ? 1 : 0) | (down ? 4 : 0) | (right ? 16 : 0) | (up ? 64 : 0)
            | (left && down && downLeft ? 2 : 0) | (down && right && downRight ? 8 : 0) | (right && up && upRight ? 32 : 0) | (up && left && upLeft ? 128 : 0);
        return Full[bits];
    }

    /// <summary>The horizontal method's tile by whether the left and the right join.</summary>
    public static int HorizontalTile(bool left, bool right) => Pair[(left ? 1 : 0) | (right ? 2 : 0)];

    /// <summary>The vertical method's tile by whether the lower and the upper join.</summary>
    public static int VerticalTile(bool down, bool up) => Pair[(down ? 1 : 0) | (up ? 2 : 0)];

    /// <summary>
    /// A repeated sheet's tile for a face of a block at a place, the sheet laid across each direction's
    /// faces as OptiFine lays it, so many tiles across and down.
    /// </summary>
    public static int RepeatTile(int face, int x, int y, int z, int width, int height)
    {
        var (across, down) = face switch
        {
            0 => (-z - 1, -y),
            1 => (z, -y),
            2 => (x, z),
            3 => (x, -z - 1),
            4 => (x, -y),
            _ => (-x - 1, -y),
        };
        return width * Wrap(down, height) + Wrap(across, width);
    }

    /// <summary>A random tile for a face of a block at a place, the same every time it is meshed, each as likely as its weight or all alike.</summary>
    public static int RandomTile(int face, int x, int y, int z, int count, IReadOnlyList<int> weights)
    {
        var roll = PlaceHash.Of(x, z, y * 6 + face);
        if (weights.Count < count || weights.Take(count).Sum() <= 0) return (int)(roll % (uint)count);
        var left = (int)(roll % (uint)weights.Take(count).Sum());
        for (int i = 0; i < count; i++)
            if ((left -= weights[i]) < 0) return i;
        return count - 1;
    }

    private static int Wrap(int value, int size) => (value % size + size) % size;

    /// <summary>
    /// Reads a rule from a properties file's text at its path under <c>assets/minecraft</c>, or
    /// null where its method is not one of those read or it names no tiles. A rule that names
    /// neither blocks nor textures takes the texture its file is named for, as OptiFine does.
    /// </summary>
    public static ConnectedRule? Parse(string path, string text)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line[0] is '#' or '!') continue;
            var equals = line.IndexOf('=');
            if (equals <= 0) continue;
            values[line[..equals].Trim()] = line[(equals + 1)..].Trim();
        }
        ConnectMethod? method = values.GetValueOrDefault("method", "ctm") switch
        {
            "ctm" or "glass" => ConnectMethod.Full,
            "horizontal" or "bookshelf" => ConnectMethod.Horizontal,
            "vertical" => ConnectMethod.Vertical,
            "top" => ConnectMethod.Top,
            "repeat" => ConnectMethod.Repeat,
            "random" => ConnectMethod.Random,
            _ => null,
        };
        if (method is not { } chosen) return null;
        var folder = path.Contains('/') ? path[..path.LastIndexOf('/')] : "";
        var tiles = Tiles(values.GetValueOrDefault("tiles", ""), folder);
        if (tiles.Length == 0) return null;

        var blocks = Names(values.GetValueOrDefault("matchBlocks", ""), texture: false);
        var textures = Names(values.GetValueOrDefault("matchTiles", ""), texture: true);
        if (blocks.Count == 0 && textures.Count == 0)
            textures = [$"block/{Path.GetFileNameWithoutExtension(path)}"];
        var byTile = values.TryGetValue("connect", out var connect) ? connect == "tile" : blocks.Count == 0;
        int Number(string key, int fallback) => int.TryParse(values.GetValueOrDefault(key), out var n) && n > 0 ? n : fallback;
        int[] weights = [.. values.GetValueOrDefault("weights", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(w => int.TryParse(w, out var n) ? Math.Max(0, n) : 0)];
        return new ConnectedRule(chosen, tiles, blocks, textures, Number("width", 1), Number("height", 1), weights, byTile,
            values.GetValueOrDefault("innerSeams") == "true", Faces(values.GetValueOrDefault("faces", "all")));
    }

    // The tiles a rule names, ranges of numbered pictures in its folder and pictures by name there,
    // a name with a slash under assets/minecraft, and <default> the face's own texture.
    private static string?[] Tiles(string list, string folder)
    {
        var tiles = new List<string?>();
        foreach (var token in list.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var dash = token.IndexOf('-');
            if (dash > 0 && int.TryParse(token[..dash], out var from) && int.TryParse(token[(dash + 1)..], out var to) && to >= from && to - from < 256)
            {
                for (int n = from; n <= to; n++) tiles.Add($"{folder}/{n}.png");
                continue;
            }
            if (token is "<default>" or "<skip>")
            {
                tiles.Add(null);
                continue;
            }
            var name = token.EndsWith(".png", StringComparison.Ordinal) ? token : token + ".png";
            tiles.Add(name.StartsWith("./", StringComparison.Ordinal) ? $"{folder}/{name[2..]}" : name.Contains('/') ? name.Replace("minecraft:", "") : $"{folder}/{name}");
        }
        return [.. tiles];
    }

    // Block keys or texture paths as a rule names them, a namespace and a block's states left out and
    // a texture's folder taken as the blocks' where none is given.
    private static HashSet<string> Names(string list, bool texture)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var token in list.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var name = token.Replace("minecraft:", "");
            if (name.IndexOf(':') is var colon and > 0) name = name[..colon];
            if (texture && !name.Contains('/')) name = $"block/{name}";
            names.Add(name);
        }
        return names;
    }

    // The faces a rule names, by the source's names of them, all six where it names none.
    private static int Faces(string list)
    {
        var faces = 0;
        foreach (var token in list.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            faces |= token switch
            {
                "east" => 1, "west" => 2, "top" or "up" => 4, "bottom" or "down" => 8, "south" => 16, "north" => 32,
                "sides" => 1 | 2 | 16 | 32, "all" => 63, _ => 0,
            };
        return faces == 0 ? 63 : faces;
    }
}
