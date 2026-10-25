using System.Numerics;
using System.Text.Json;

namespace VoxelSandbox;

/// <summary>
/// The models a resource pack gives the mobs in OptiFine's form, a <c>.jem</c> file of parts and
/// boxes under <c>optifine/cem</c> by the mob's name, read as assets into the game's pieces, so a
/// mob a pack models is drawn with the pack's boxes and walked by the game's own gait.
/// </summary>
/// <remarks>
/// <para>
/// A model's parts nest, each with its pivot and its turn, and the top ones are named for the
/// source's parts, its head, its body, its legs, its arms and its wings, by which each takes its
/// role in the game's gait. A part's boxes and every box nested in it are placed by the turns and
/// moves of the parts they lie in, so a piece of the game's model is a top part with all it holds,
/// and a part nested in another but named for a role, as Fresh Animations lays its head inside its
/// body as <c>head2</c>, is lifted out into a piece of its own, so the head still turns with the look.
/// A model's animations are expressions the game takes none of, so every part stands as the file
/// places it and moves only as the game's gait moves it.
/// </para>
/// <para>
/// The form's coordinates are the game's model space in pixels, y up from the feet and the front
/// toward -z, as Blockbench reads them. A top part's pivot is its translation turned about, its
/// boxes stand where their coordinates say, a part nested in a top part has its pivot where its
/// translation says, and one nested deeper and every nested part's boxes stand from the pivot of
/// the part holding them. Each turn is in degrees about x, then y, then z, about the part's pivot.
/// A box's faces take the parts of the picture each names, or the source's layout from its offset,
/// in the model's size of picture, scaled to the size the game lays its kind's picture at, and a
/// leg's or an arm's pivot standing low in it, as Fresh Animations stands its cow's legs on their
/// pivots at the hoof, is set at the top of its boxes, where the gait swings it from.
/// </para>
/// </remarks>
public static class PackModels
{
    // The roles of the top parts the source names.
    private static readonly Dictionary<string, Swing> Roles = new()
    {
        ["head"] = Swing.Head,
        ["headwear"] = Swing.Head,
        ["headwear2"] = Swing.Head,
        ["nose"] = Swing.Head,
        ["bill"] = Swing.Head,
        ["chin"] = Swing.Head,
        ["right_arm"] = Swing.ArmB,
        ["left_arm"] = Swing.ArmA,
        ["right_leg"] = Swing.LegB,
        ["left_leg"] = Swing.LegA,
        ["right_wing"] = Swing.RightWing,
        ["left_wing"] = Swing.LeftWing,
    };

    // The roles a nested part is lifted out for, by its name less any digits at its end.
    private static readonly Dictionary<string, Swing> Lifted = new()
    {
        ["head"] = Swing.Head,
        ["right_wing"] = Swing.RightWing,
        ["left_wing"] = Swing.LeftWing,
    };

    /// <summary>
    /// The pieces of a kind's model the packs give, its body's model and, for a sheep, its wool's
    /// laid with its second picture and tinted, or null where no pack models the kind or its file is
    /// not of the form.
    /// </summary>
    public static ModelPart[]? Read(ResourcePacks packs, MobKind kind)
    {
        if (packs.Read($"optifine/cem/{kind.Name}.jem") is not { } body) return null;
        try
        {
            var parts = Parse(body, kind, picture: 0, tinted: false);
            if (kind.Name == "sheep" && packs.Read("optifine/cem/sheep_wool.jem") is { } wool)
                parts.AddRange(Parse(wool, kind, picture: 1, tinted: true));
            return parts.Count > 0 ? [.. parts] : null;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException or KeyNotFoundException or IndexOutOfRangeException)
        {
            return null;
        }
    }

    // A piece being gathered, its role, its pivot in the model, its boxes, whether it is a leg the
    // form numbers rather than names by its side, and a spider's leg's pair and splay.
    private sealed class Piece(Swing swing, Vector3 pivot, bool numbered = false)
    {
        public Swing Swing = swing;
        public Vector3 Pivot = pivot;
        public readonly bool Numbered = numbered;
        public readonly List<ModelBox> Boxes = [];
        public int Pair;
        public Vector3 Pose;
    }

    /// <summary>The pieces of one model file of the form, laid with one of its kind's pictures.</summary>
    public static List<ModelPart> Parse(byte[] json, MobKind kind, int picture, bool tinted)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var size = root.TryGetProperty("textureSize", out var given) ? new Vector2(given[0].GetSingle(), given[1].GetSingle()) : kind.PictureSize;
        // The model's pixels of picture in the kind's, as Fresh Animations gives its pictures' parts
        // on a picture four times the source's.
        var scale = kind.PictureSize.X / size.X;
        var pieces = new List<Piece>();
        foreach (var top in root.GetProperty("models").EnumerateArray())
        {
            var name = top.TryGetProperty("part", out var part) ? part.GetString() ?? "" : "";
            var numbered = name.StartsWith("leg", StringComparison.Ordinal) && name.Length > 3 && name[3..].All(char.IsAsciiDigit);
            var piece = new Piece(Roles.GetValueOrDefault(name), -Vector3OrZero(top, "translate"), numbered);
            pieces.Add(piece);
            var turn = Turn(top, piece.Pivot);
            Gather(top, piece, turn, Vector3.Zero, depth: 0, absolute: true, pieces);
        }
        pieces.RemoveAll(p => p.Boxes.Count == 0);
        Settle(pieces, kind);
        return [.. pieces.Select(p => new ModelPart(p.Swing, p.Pivot, [.. p.Boxes]) { Pair = p.Pair, Pose = p.Pose })];

        // A part's boxes into a piece, placed by the turns of every part they lie in, and its nested
        // parts after them, one named for a role lifted out into a piece of its own.
        void Gather(JsonElement element, Piece piece, Matrix4x4 placed, Vector3 origin, int depth, bool absolute, List<Piece> into)
        {
            var mirror = element.TryGetProperty("mirrorTexture", out var mirrored) && (mirrored.GetString() ?? "").Contains('u');
            if (element.TryGetProperty("boxes", out var boxes))
                foreach (var box in boxes.EnumerateArray())
                    piece.Boxes.Add(Box(box, absolute ? placed : Matrix4x4.CreateTranslation(origin) * placed, mirror));
            if (!element.TryGetProperty("submodels", out var subs)) return;
            foreach (var sub in subs.EnumerateArray())
            {
                // A part nested in a top part stands where its translation says, and one deeper
                // from the pivot of the part holding it.
                var at = Vector3OrZero(sub, "translate") + (depth == 0 ? Vector3.Zero : origin);
                var around = Turn(sub, at) * placed;
                var id = sub.TryGetProperty("id", out var named) ? named.GetString() ?? "" : "";
                if (piece.Swing == Swing.None && Lifted.TryGetValue(id.TrimEnd("0123456789".ToCharArray()), out var role))
                {
                    var lifted = new Piece(role, Vector3.Transform(at, placed));
                    into.Add(lifted);
                    Gather(sub, lifted, around, at, depth + 1, absolute: false, into);
                }
                else Gather(sub, piece, around, at, depth + 1, absolute: false, into);
            }
        }

        // A box of the form as one of the source's cuboids at its corner, its own faces' parts or
        // its offset, grown by its added size, placed into the model.
        ModelBox Box(JsonElement box, Matrix4x4 placed, bool mirror)
        {
            var c = box.GetProperty("coordinates");
            float x = c[0].GetSingle(), y = c[1].GetSingle(), z = c[2].GetSingle(), w = c[3].GetSingle(), h = c[4].GetSingle(), d = c[5].GetSingle();
            var grow = box.TryGetProperty("sizeAdd", out var added) ? added.GetSingle() : 0;
            Vector4?[]? faces = null;
            var offset = Vector2.Zero;
            if (box.TryGetProperty("textureOffset", out var at)) offset = new Vector2(at[0].GetSingle(), at[1].GetSingle()) * scale;
            else faces = [.. new[] { "uvDown", "uvUp", "uvWest", "uvNorth", "uvEast", "uvSouth" }.Select(face =>
                box.TryGetProperty(face, out var uv) ? (Vector4?)(new Vector4(uv[0].GetSingle(), uv[1].GetSingle(), uv[2].GetSingle(), uv[3].GetSingle()) * scale) : null)];
            // The corner in the source's space, x mirrored and y down from 24 pixels, which the
            // cuboid places back into the game's.
            var cuboid = new Cuboid(Vector3.Zero, new Vector3(-(x + w), 24 - (y + h), z), new Vector3(w, h, d), grow, false, offset, mirror, picture, tinted)
            {
                Placed = placed,
                Parts = faces,
            };
            var corners = Corners(cuboid);
            return new ModelBox(corners.Min, corners.Max, null, cuboid, Drawn.Pack);
        }
    }

    // A part's turn about its pivot, from its angles in degrees about x, then y, then z.
    private static Matrix4x4 Turn(JsonElement element, Vector3 pivot)
    {
        var angles = Vector3OrZero(element, "rotate") * (MathF.PI / 180);
        if (angles == Vector3.Zero) return Matrix4x4.Identity;
        return Matrix4x4.CreateTranslation(-pivot) * ModelPart.Turned(angles) * Matrix4x4.CreateTranslation(pivot);
    }

    private static Vector3 Vector3OrZero(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) ? new Vector3(value[0].GetSingle(), value[1].GetSingle(), value[2].GetSingle()) : Vector3.Zero;

    // The box around a cuboid's eight placed corners, whichever of its faces are drawn.
    private static (Vector3 Min, Vector3 Max) Corners(Cuboid cuboid)
    {
        var (low, high) = (cuboid.From - new Vector3(cuboid.Grow), cuboid.From + cuboid.Size + new Vector3(cuboid.Grow));
        var points = Enumerable.Range(0, 8).Select(i => cuboid.Place(new Vector3((i & 1) == 0 ? low.X : high.X, (i & 2) == 0 ? low.Y : high.Y, (i & 4) == 0 ? low.Z : high.Z))).ToList();
        return (points.Aggregate(Vector3.Min), points.Aggregate(Vector3.Max));
    }

    // The roles of the numbered legs by where they stand, a four-legged kind's diagonal pairs
    // swinging together as the game's own legs do, the right hind leg, at +x and +z, with the left
    // front one, and a spider's by its side and its pair from hind to front, with the game's own
    // spider's splay of that leg, turned at its end by the body. A leg's or an arm's pivot standing
    // in the lower three quarters of its boxes is moved up to their top.
    private static void Settle(List<Piece> pieces, MobKind kind)
    {
        var spider = kind.Parts.Any(p => p.Swing is Swing.SpiderA or Swing.SpiderB);
        foreach (var piece in pieces.Where(p => p.Numbered))
        {
            var middle = Middle(piece);
            piece.Swing = spider ? middle.X > 0 ? Swing.SpiderA : Swing.SpiderB
                : middle.Z > 0 == middle.X > 0 ? Swing.LegA : Swing.LegB;
        }
        if (spider)
            foreach (var side in new[] { Swing.SpiderA, Swing.SpiderB })
            {
                var row = pieces.Where(p => p.Numbered && p.Swing == side).OrderByDescending(p => Middle(p).Z).ToList();
                var own = kind.Parts.Where(p => p.Swing == side).OrderBy(p => p.Pair).ToList();
                for (int i = 0; i < row.Count && i < own.Count; i++)
                {
                    // A spider's leg turns at its end by the body, where a model may pivot it at its tip.
                    var middle = Middle(row[i]);
                    var inner = side == Swing.SpiderA ? row[i].Boxes.Min(b => b.Min.X) : row[i].Boxes.Max(b => b.Max.X);
                    (row[i].Pair, row[i].Pose, row[i].Pivot) = (own[i].Pair, own[i].Pose, new Vector3(inner, middle.Y, middle.Z));
                }
            }
        foreach (var piece in pieces.Where(p => p.Swing is Swing.LegA or Swing.LegB or Swing.ArmA or Swing.ArmB))
        {
            float bottom = piece.Boxes.Min(b => b.Min.Y), top = piece.Boxes.Max(b => b.Max.Y);
            if (piece.Pivot.Y >= bottom + 0.75f * (top - bottom)) continue;
            var middle = Middle(piece);
            piece.Pivot = new Vector3(middle.X, top, middle.Z);
        }
    }

    private static Vector3 Middle(Piece piece) =>
        (piece.Boxes.Select(b => b.Min).Aggregate(Vector3.Min) + piece.Boxes.Select(b => b.Max).Aggregate(Vector3.Max)) / 2;
}
