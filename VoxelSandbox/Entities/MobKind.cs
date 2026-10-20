using System.Numerics;
using Engine;

namespace VoxelSandbox;

/// <summary>A box of a mob's model between two corners in sixteenths of a block, its color, or null for the color its mob varies by.</summary>
public readonly record struct ModelBox(Vector3 Min, Vector3 Max, Color? Color);

/// <summary>
/// How a piece of a model moves, whether not at all, with the head's look, as one of the two pairs
/// of legs, as a wing, as one of two arms swinging against the legs, or as one of a spider's two
/// sets of legs, which swing about the vertical.
/// </summary>
public enum Swing
{
    None,
    Head,
    LegA,
    LegB,
    LeftWing,
    RightWing,
    ArmA,
    ArmB,
    SpiderA,
    SpiderB,
}

/// <summary>A piece of a mob's model that moves as one, turning about its pivot, in sixteenths of a block.</summary>
public sealed record ModelPart(Swing Swing, Vector3 Pivot, ModelBox[] Boxes)
{
    private Vector3[]? _corners;

    /// <summary>
    /// How the piece is turned about its pivot before it swings, in radians about x, then y, then
    /// z, the order Minecraft turns its models' parts in, as a zombie holds its arms out.
    /// </summary>
    public Vector3 Pose { get; init; }

    /// <summary>Whether a box of it takes its mob's own color, so it is meshed once for each.</summary>
    public bool Varies => Boxes.Any(b => b.Color is null);

    /// <summary>
    /// Where the piece's boxes go in its model, in blocks, turned by its pose and then by a swing
    /// about its pivot, and a leg lifted by however far its lowest corner would sink below the
    /// feet, so a foot swung ahead or behind stays on the ground.
    /// </summary>
    /// <remarks>
    /// A leg turns about its top, so the corner of its foot ahead of the turn goes down as the leg
    /// swings, a chicken's toes, reaching three pixels ahead of the hip, by most of a pixel.
    /// </remarks>
    public Matrix4x4 Placed(Matrix4x4 swing)
    {
        var turn = Pose == Vector3.Zero ? swing
            : Matrix4x4.CreateRotationX(Pose.X) * Matrix4x4.CreateRotationY(Pose.Y) * Matrix4x4.CreateRotationZ(Pose.Z) * swing;
        var pivot = Pivot / 16;
        var placed = turn.IsIdentity ? Matrix4x4.Identity : Matrix4x4.CreateTranslation(-pivot) * turn * Matrix4x4.CreateTranslation(pivot);
        if (Swing is not (Swing.LegA or Swing.LegB) || placed.IsIdentity) return placed;
        var lowest = float.MaxValue;
        foreach (var corner in _corners ??= [.. Boxes.SelectMany(Corners)])
            lowest = MathF.Min(lowest, Vector3.Transform(corner, placed).Y);
        return lowest < 0 ? placed * Matrix4x4.CreateTranslation(0, -lowest, 0) : placed;
    }

    // A box's eight corners in blocks.
    private static IEnumerable<Vector3> Corners(ModelBox box)
    {
        for (int i = 0; i < 8; i++)
            yield return new Vector3((i & 1) == 0 ? box.Min.X : box.Max.X, (i & 2) == 0 ? box.Min.Y : box.Max.Y, (i & 4) == 0 ? box.Min.Z : box.Max.Z) / 16;
    }
}

/// <summary>
/// A kind of mob, with its box, how fast it walks, its health, the colors it varies by, and its
/// model, facing north (toward -z) with its feet's middle at the origin.
/// </summary>
/// <param name="Name">The name <c>voxel.spawn</c> takes and a save keeps.</param>
/// <param name="HalfWidth">Half its box's width, as Minecraft's sizes give it.</param>
/// <param name="Height">Its box's height.</param>
/// <param name="Speed">How fast it wanders, in blocks a second.</param>
/// <param name="Health">The hits it takes before it dies.</param>
/// <param name="Variants">The colors its model's own boxes take, one a variant, chosen as <see cref="Odds"/> say.</param>
/// <param name="Parts">Its model.</param>
public sealed record MobKind(string Name, float HalfWidth, float Height, float Speed, int Health, Color[] Variants, ModelPart[] Parts)
{
    /// <summary>Its place in <see cref="MobKinds.All"/>.</summary>
    public byte Id { get; init; }

    /// <summary>How often each variant is chosen, in thousandths, or null where each is as likely.</summary>
    public int[]? Odds { get; init; }

    /// <summary>Whether it opens a shut door in its way and shuts it behind it, as a villager does.</summary>
    public bool OpensDoors { get; init; }

    /// <summary>Whether it falls slowly, flapping, as a chicken does.</summary>
    public bool FallsSlowly { get; init; }

    /// <summary>Whether it hunts the player in survival, spawns in the dark and is not kept by a column.</summary>
    public bool Hostile { get; init; }

    /// <summary>The health a hit of its takes from the player, for a mob that strikes.</summary>
    public int AttackDamage { get; init; }

    /// <summary>How fast it runs at the player, in blocks a second.</summary>
    public float ChaseSpeed { get; init; }

    /// <summary>How far off it notices the player it can see.</summary>
    public float FollowRange { get; init; } = 16;

    /// <summary>Whether it shoots arrows rather than striking, as a skeleton does.</summary>
    public bool Shoots { get; init; }

    /// <summary>Whether it hisses and explodes beside the player rather than striking, as a creeper does.</summary>
    public bool Explodes { get; init; }

    /// <summary>Whether it burns under the open sky by day, as zombies and skeletons do.</summary>
    public bool Burns { get; init; }

    /// <summary>
    /// How large its model is drawn against the numbers its parts are given in, about its feet, as
    /// Minecraft's layer of the villager is drawn at 0.9375.
    /// </summary>
    public float Scale { get; init; } = 1;

    /// <summary>The height of its eyes, which turn toward the player, at its head's pivot.</summary>
    public float Eyes => Parts.FirstOrDefault(p => p.Swing == Swing.Head)?.Pivot.Y / 16 * Scale ?? Height * 0.85f;

    /// <summary>A variant from a roll of 0 to 999.</summary>
    public byte Variant(int roll)
    {
        if (Odds is null) return (byte)(roll % Variants.Length);
        for (int i = 0; i < Odds.Length; i++)
        {
            if (roll < Odds[i]) return (byte)i;
            roll -= Odds[i];
        }
        return 0;
    }
}

/// <summary>
/// Every kind of mob, by its id, the cuboids of their models, their pivots and their bodies' sizes
/// taken from Minecraft Java Edition's model definitions and entity sizes, and their colors, faces
/// and markings the game's own.
/// </summary>
/// <remarks>
/// Minecraft gives a cuboid as a corner and a size in pixels from its part's pivot, with y down,
/// x mirrored and the feet 24 pixels below the model's origin, and a four-legged body as a box
/// lying a quarter turn from upright. <see cref="Mc"/> places such a cuboid in the game's model
/// space, y up from the feet, and <see cref="At"/> a pivot, so the numbers below are written as
/// Minecraft's definitions write them. The markings, eyes and patches drawn on the cuboids' faces
/// are boxes of the game's in its own model space, standing a twentieth of a pixel out from the
/// face they lie on.
/// </remarks>
public static class MobKinds
{
    private static readonly Color Black = new(28, 24, 24), White = new(236, 236, 236);

    // An arm held out ahead, as a zombie's and a skeleton's are, Minecraft's turn of -π/2 about x
    // with its y down. It and the colors above are set before the kinds below, which read them as
    // they are made, the class's fields being set in the order they are written.
    private static readonly Vector3 Ahead = new(MathF.PI / 2, 0, 0);

    // A box between two corners in the game's model space, in sixteenths of a block.
    private static ModelBox B(float x0, float y0, float z0, float x1, float y1, float z1, Color? color) =>
        new(new Vector3(x0, y0, z0), new Vector3(x1, y1, z1), color);

    // A pivot as Minecraft's model definitions place it, in the game's model space.
    private static Vector3 At(Vector3 pivot) => new(-pivot.X, 24 - pivot.Y, pivot.Z);

    // A cuboid as Minecraft's model definitions give it, its corner and size from its part's pivot,
    // grown on every side as a cube deformation grows it, and turned a quarter about x where it
    // lies as a four-legged body does, placed in the game's model space.
    private static ModelBox Mc(Vector3 pivot, float x, float y, float z, float w, float h, float d, Color? color, float grow = 0, bool lying = false)
    {
        var min = new Vector3(x - grow, y - grow, z - grow);
        var max = new Vector3(x + w + grow, y + h + grow, z + d + grow);
        // The quarter turn about x takes a point's y to -z and its z to y.
        if (lying) (min, max) = (new Vector3(min.X, -max.Z, min.Y), new Vector3(max.X, -min.Z, max.Y));
        return new ModelBox(
            new Vector3(-(pivot.X + max.X), 24 - (pivot.Y + max.Y), pivot.Z + min.Z),
            new Vector3(-(pivot.X + min.X), 24 - (pivot.Y + min.Y), pivot.Z + max.Z), color);
    }

    // A pair of eyes on a face at z, each a dark pixel inward of a light one where a light color is given.
    private static IEnumerable<ModelBox> Eyes(float inner, float y, float z, Color pupil, Color? white = null)
    {
        yield return B(-inner - 1, y, z - 0.05f, -inner, y + 1, z, pupil);
        yield return B(inner, y, z - 0.05f, inner + 1, y + 1, z, pupil);
        if (white is not { } light) yield break;
        yield return B(-inner - 2, y, z - 0.05f, -inner - 1, y + 1, z, light);
        yield return B(inner + 1, y, z - 0.05f, inner + 2, y + 1, z, light);
    }

    // Four legs at Minecraft's pivots, x to either side of the middle and the hind and front pairs
    // at their z, the right hind and the left front swinging together and the other two against
    // them, as Minecraft swings them.
    private static ModelPart[] Legs(float x, float y, float hind, float front, Func<Vector3, IEnumerable<ModelBox>> boxes) =>
    [
        .. new (float X, float Z, Swing Swing)[] { (-x, hind, Swing.LegA), (x, hind, Swing.LegB), (-x, front, Swing.LegB), (x, front, Swing.LegA) }
            .Select(leg => new ModelPart(leg.Swing, At(new Vector3(leg.X, y, leg.Z)), [.. boxes(new Vector3(leg.X, y, leg.Z))])),
    ];

    public static readonly MobKind Pig = MakePig();
    public static readonly MobKind Cow = MakeCow();
    public static readonly MobKind Sheep = MakeSheep();
    public static readonly MobKind Chicken = MakeChicken();
    public static readonly MobKind Villager = MakeVillager();
    public static readonly MobKind Zombie = MakeZombie();
    public static readonly MobKind Skeleton = MakeSkeleton();
    public static readonly MobKind Creeper = MakeCreeper();
    public static readonly MobKind Spider = MakeSpider();

    public static readonly MobKind[] All = [Pig, Cow, Sheep, Chicken, Villager, Zombie, Skeleton, Creeper, Spider];

    /// <summary>The hostile kinds, and how often each of a hundred spawns is of it, as Minecraft's overworld weighs them.</summary>
    public static readonly (MobKind Kind, int Weight)[] Night = [(Zombie, 40), (Skeleton, 30), (Spider, 15), (Creeper, 15)];

    public static MobKind? Find(string name) => All.FirstOrDefault(k => k.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    private static MobKind MakePig()
    {
        Color pink = new(236, 158, 160), snout = new(218, 118, 130), hoof = new(196, 120, 124);
        Vector3 body = new(0, 11, 2), head = new(0, 12, -6);
        ModelPart[] parts =
        [
            new(Swing.None, Vector3.Zero, [Mc(body, -5, -10, -7, 10, 16, 8, pink, lying: true)]),
            new(Swing.Head, At(head), [Mc(head, -4, -4, -8, 8, 8, 8, pink), Mc(head, -2, 0, -9, 4, 3, 1, snout), .. Eyes(2, 12, -14, Black, White)]),
            .. Legs(3, 18, 7, -5, leg => [Mc(leg, -2, 0, -2, 4, 6, 4, pink), Mc(leg, -2.05f, 5, -2.05f, 4.1f, 1, 4.1f, hoof)]),
        ];
        return new MobKind("pig", 0.45f, 0.9f, 1.25f, 10, [pink], parts) { Id = 0 };
    }

    private static MobKind MakeCow()
    {
        Color brown = new(78, 56, 40), patch = new(226, 224, 216), horn = new(214, 204, 178), muzzle = new(196, 160, 146), udder = new(226, 160, 166);
        Vector3 body = new(0, 5, 2), head = new(0, 4, -8);
        ModelPart[] parts =
        [
            new(Swing.None, Vector3.Zero,
            [
                Mc(body, -6, -10, -7, 12, 18, 10, brown, lying: true), Mc(body, -2, 2, -8, 4, 6, 1, udder, lying: true),
                B(6, 14, -3, 6.05f, 20, 4, patch), B(-6.05f, 13, 1, -6, 18, 7, patch), B(-3, 22, -5, 2, 22.05f, 1, patch),
            ]),
            // The nose standing a pixel out from the face and the horns as Minecraft 26.3's cow has
            // them, the blaze above the nose the game's own.
            new(Swing.Head, At(head),
            [
                Mc(head, -4, -4, -6, 8, 8, 6, brown), Mc(head, -5, -5, -5, 1, 3, 1, horn), Mc(head, 4, -5, -5, 1, 3, 1, horn),
                Mc(head, -3, 1, -7, 6, 3, 1, muzzle), B(-2, 19, -14.05f, 2, 22, -14, patch), .. Eyes(3, 20, -14, Black),
            ]),
            .. Legs(4, 12, 7, -6, leg => [Mc(leg, -2, 0, -2, 4, 12, 4, brown), Mc(leg, -2.05f, 8, -2.05f, 4.1f, 4, 4.1f, patch)]),
        ];
        return new MobKind("cow", 0.45f, 1.4f, 1.1f, 10, [brown], parts) { Id = 1 };
    }

    private static MobKind MakeSheep()
    {
        Color skin = new(222, 192, 170);
        Vector3 body = new(0, 5, 2), head = new(0, 6, -8);
        // The wool is Minecraft's fur layer over the sheep, the head's and the legs' grown about
        // their cuboids and the body's in place of it, since the game's sheep are never shorn and
        // the body beneath the wool would not show.
        ModelPart[] parts =
        [
            new(Swing.None, Vector3.Zero, [Mc(body, -4, -10, -7, 8, 16, 6, null, grow: 1.75f, lying: true)]),
            new(Swing.Head, At(head), [Mc(head, -3, -4, -6, 6, 6, 8, skin), Mc(head, -3, -4, -4, 6, 6, 6, null, grow: 0.6f), .. Eyes(1, 19, -14, Black)]),
            .. Legs(3, 12, 7, -5, leg => [Mc(leg, -2, 0, -2, 4, 12, 4, skin), Mc(leg, -2, 0, -2, 4, 6, 4, null, grow: 0.5f)]),
        ];
        // White, light gray, gray, black, brown and pink wool, as often as Minecraft's sheep are born each.
        Color[] wool = [new(233, 236, 236), new(142, 142, 135), new(63, 68, 72), new(24, 24, 30), new(114, 71, 40), new(237, 141, 172)];
        return new MobKind("sheep", 0.45f, 1.3f, 1.15f, 8, wool, parts) { Id = 2, Odds = [818, 50, 50, 50, 30, 2] };
    }

    private static MobKind MakeChicken()
    {
        Color feathers = new(240, 240, 238), beak = new(234, 164, 44), wattle = new(198, 34, 36), leg = new(226, 170, 52);
        Vector3 body = new(0, 16, 0), head = new(0, 15, -4), leftWing = new(4, 13, 0), rightWing = new(-4, 13, 0), leftLeg = new(1, 19, 1), rightLeg = new(-2, 19, 1);
        // A leg's cuboid holds a stick a pixel square at its middle and the toes, a plate half a
        // pixel thick across its foot, as Minecraft's leg is drawn on its cuboid.
        IEnumerable<ModelBox> Leg(Vector3 at) => [Mc(at, 0, 0, -2, 1, 4.5f, 1, leg), Mc(at, -1, 4.5f, -3, 3, 0.5f, 3, leg)];
        ModelPart[] parts =
        [
            new(Swing.None, Vector3.Zero, [Mc(body, -3, -4, -3, 6, 8, 6, feathers, lying: true)]),
            new(Swing.Head, At(head),
                [Mc(head, -2, -6, -2, 4, 6, 3, feathers), Mc(head, -2, -4, -4, 4, 2, 2, beak), Mc(head, -1, -2, -3, 2, 2, 2, wattle), .. Eyes(1, 13, -6, Black)]),
            new(Swing.LeftWing, At(leftWing), [Mc(leftWing, -1, 0, -3, 1, 4, 6, feathers)]),
            new(Swing.RightWing, At(rightWing), [Mc(rightWing, 0, 0, -3, 1, 4, 6, feathers)]),
            new(Swing.LegA, At(leftLeg), [.. Leg(leftLeg)]),
            new(Swing.LegB, At(rightLeg), [.. Leg(rightLeg)]),
        ];
        return new MobKind("chicken", 0.2f, 0.7f, 1.0f, 4, [feathers], parts) { Id = 3, FallsSlowly = true };
    }

    private static MobKind MakeVillager()
    {
        Color skin = new(186, 134, 104), nose = new(168, 116, 88), brow = new(66, 48, 38), eye = new(58, 128, 60), pants = new(72, 56, 44);
        Vector3 leftLeg = new(2, 12, 0), rightLeg = new(-2, 12, 0), arms = new(0, 3, -1);
        // The robe is Minecraft's jacket, grown about a cuboid 20 pixels long from the shoulders,
        // the body under it left out, and the arms cross in front of it, turned up by Minecraft's
        // 0.75 about x, the hands on the bar across.
        ModelPart[] parts =
        [
            new(Swing.LegA, At(leftLeg), [Mc(leftLeg, -2, 0, -2, 4, 12, 4, pants)]),
            new(Swing.LegB, At(rightLeg), [Mc(rightLeg, -2, 0, -2, 4, 12, 4, pants)]),
            new(Swing.None, Vector3.Zero, [Mc(Vector3.Zero, -4, 0, -3, 8, 20, 6, null, grow: 0.5f)]),
            new(Swing.None, At(arms),
                [Mc(arms, -8, -2, -2, 4, 8, 4, null), Mc(arms, 4, -2, -2, 4, 8, 4, null), Mc(arms, -4, 2, -2, 8, 4, 4, null), B(-3, 15.5f, -3.05f, 3, 18.5f, -3, skin)])
                { Pose = new Vector3(0.75f, 0, 0) },
            new(Swing.Head, At(Vector3.Zero),
            [
                Mc(Vector3.Zero, -4, -10, -4, 8, 10, 8, skin), Mc(new Vector3(0, -2, 0), -1, -1, -6, 2, 4, 2, nose), B(-4, 30, -4.05f, 4, 31, -4, brow),
                .. Eyes(1, 29, -4, eye, White),
            ]),
        ];
        // A robe of the plains, the desert, the taiga and the snow, as Minecraft's villagers wear by biome.
        Color[] robes = [new(118, 86, 58), new(204, 168, 112), new(86, 62, 46), new(214, 218, 228)];
        // Drawn at the scale Minecraft's layer gives it, about its feet, so its 34 pixels stand 1.99
        // blocks as Minecraft's villager does, where at 1 they would stand 2.13.
        return new MobKind("villager", 0.3f, 1.95f, 1.5f, 20, robes, parts) { Id = 4, OpensDoors = true, Scale = 0.9375f };
    }

    private static MobKind MakeZombie()
    {
        Color skin = new(86, 138, 70), shirt = new(52, 150, 158), pants = new(60, 58, 138), eye = new(24, 44, 22);
        Vector3 leftLeg = new(1.9f, 12, 0), rightLeg = new(-1.9f, 12, 0), leftArm = new(5, 2, 0), rightArm = new(-5, 2, 0);
        ModelPart[] parts =
        [
            new(Swing.LegA, At(leftLeg), [Mc(leftLeg, -2, 0, -2, 4, 12, 4, pants)]),
            new(Swing.LegB, At(rightLeg), [Mc(rightLeg, -2, 0, -2, 4, 12, 4, pants)]),
            new(Swing.None, Vector3.Zero, [Mc(Vector3.Zero, -4, 0, -2, 8, 12, 4, shirt)]),
            new(Swing.ArmA, At(leftArm), [Mc(leftArm, -1, -2, -2, 4, 12, 4, skin), Mc(leftArm, -1, -2, -2, 4, 6, 4, shirt, grow: 0.05f)]) { Pose = Ahead },
            new(Swing.ArmB, At(rightArm), [Mc(rightArm, -3, -2, -2, 4, 12, 4, skin), Mc(rightArm, -3, -2, -2, 4, 6, 4, shirt, grow: 0.05f)]) { Pose = Ahead },
            new(Swing.Head, At(Vector3.Zero), [Mc(Vector3.Zero, -4, -8, -4, 8, 8, 8, skin), .. Eyes(1, 28, -4, eye), B(-2, 25, -4.05f, 2, 26, -4, eye)]),
        ];
        return new MobKind("zombie", 0.3f, 1.95f, 1.0f, 20, [skin], parts)
        {
            Id = 5, Hostile = true, AttackDamage = 3, ChaseSpeed = 2.3f, FollowRange = 24, Burns = true,
        };
    }

    private static MobKind MakeSkeleton()
    {
        Color bone = new(204, 204, 198), dark = new(44, 44, 44), bow = new(112, 82, 44);
        Vector3 leftLeg = new(2, 12, 0), rightLeg = new(-2, 12, 0), leftArm = new(5, 2, 0), rightArm = new(-5, 2, 0);
        ModelPart[] parts =
        [
            new(Swing.LegA, At(leftLeg), [Mc(leftLeg, -1, 0, -1, 2, 12, 2, bone)]),
            new(Swing.LegB, At(rightLeg), [Mc(rightLeg, -1, 0, -1, 2, 12, 2, bone)]),
            // A spine with ribs about it and shoulders across its top, within the body's cuboid of
            // 8 by 12 by 4, which Minecraft's texture leaves open but for the bones.
            new(Swing.None, Vector3.Zero, [B(-1, 12, -1, 1, 22, 1, bone), B(-3, 15, -1.5f, 3, 21, 1.5f, bone), B(-4, 22, -1, 4, 24, 1, bone)]),
            new(Swing.ArmA, At(leftArm), [Mc(leftArm, -1, -2, -1, 2, 12, 2, bone)]) { Pose = Ahead },
            // The right hand holds a bow upright once the arm is held out.
            new(Swing.ArmB, At(rightArm), [Mc(rightArm, -1, -2, -1, 2, 12, 2, bone), B(4.5f, 10, -4, 5.5f, 12, 4, bow)]) { Pose = Ahead },
            new(Swing.Head, At(Vector3.Zero),
                [Mc(Vector3.Zero, -4, -8, -4, 8, 8, 8, bone), .. Eyes(1, 27, -4, dark), B(-3, 28, -4.05f, 3, 29, -4, dark), B(-0.5f, 26, -4.05f, 0.5f, 27, -4, dark)]),
        ];
        return new MobKind("skeleton", 0.3f, 1.99f, 1.2f, 20, [bone], parts)
        {
            Id = 6, Hostile = true, ChaseSpeed = 2.4f, FollowRange = 16, Shoots = true, Burns = true,
        };
    }

    private static MobKind MakeCreeper()
    {
        Color green = new(84, 170, 64), patch = new(58, 128, 44), face = new(20, 24, 20);
        Vector3 top = new(0, 6, 0);
        ModelPart[] parts =
        [
            new(Swing.None, Vector3.Zero, [Mc(top, -4, 0, -2, 8, 12, 4, green), B(-4.05f, 9, -1, -4, 13, 1.5f, patch), B(4, 12, -1.5f, 4.05f, 16, 1, patch)]),
            new(Swing.Head, At(top),
            [
                Mc(top, -4, -8, -4, 8, 8, 8, green), B(-3, 22, -4.05f, -1, 24, -4, face), B(1, 22, -4.05f, 3, 24, -4, face),
                B(-1, 19, -4.05f, 1, 22, -4, face), B(-2, 18, -4.05f, -1, 21, -4, face), B(1, 18, -4.05f, 2, 21, -4, face),
            ]),
            .. Legs(2, 18, 4, -4, leg => [Mc(leg, -2, 0, -2, 4, 6, 4, patch)]),
        ];
        return new MobKind("creeper", 0.3f, 1.7f, 1.0f, 20, [green], parts)
        {
            Id = 7, Hostile = true, ChaseSpeed = 2.2f, FollowRange = 16, Explodes = true,
        };
    }

    private static MobKind MakeSpider()
    {
        Color body = new(52, 44, 40), back = new(66, 56, 50), red = new(196, 30, 30);
        Vector3 head = new(0, 15, -3), neck = new(0, 15, 0), abdomen = new(0, 15, 9);
        var parts = new List<ModelPart>
        {
            new(Swing.None, Vector3.Zero, [Mc(neck, -3, -3, -3, 6, 6, 6, body), Mc(abdomen, -5, -4, -6, 10, 8, 12, back)]),
            new(Swing.Head, At(head),
                [Mc(head, -4, -4, -8, 8, 8, 8, body), .. Eyes(1, 10, -11, red), B(-3, 11, -11.05f, -2, 12, -11, red), B(2, 11, -11.05f, 3, 12, -11, red)]),
        };
        // Four legs a side from hind to front, each reaching out along x from the neck, turned
        // about y to spread from back to forward and about z to reach down to the ground by
        // Minecraft's angles, which the mirror of its x and y turns about y the other way here, the
        // sides' sets swinging against each other.
        (float Z, float AboutY, float AboutZ)[] reach = [(2, MathF.PI / 4, -MathF.PI / 4), (1, MathF.PI / 8, -0.58119464f), (0, -MathF.PI / 8, -0.58119464f), (-1, -MathF.PI / 4, -MathF.PI / 4)];
        for (int i = 0; i < reach.Length; i++)
        {
            var (z, aboutY, aboutZ) = reach[i];
            Vector3 right = new(-4, 15, z), left = new(4, 15, z);
            parts.Add(new(i % 2 == 0 ? Swing.SpiderA : Swing.SpiderB, At(right), [Mc(right, -15, -1, -1, 16, 2, 2, body)]) { Pose = new Vector3(0, -aboutY, aboutZ) });
            parts.Add(new(i % 2 == 0 ? Swing.SpiderB : Swing.SpiderA, At(left), [Mc(left, -1, -1, -1, 16, 2, 2, body)]) { Pose = new Vector3(0, aboutY, -aboutZ) });
        }
        return new MobKind("spider", 0.7f, 0.9f, 1.5f, 16, [body], [.. parts])
        {
            Id = 8, Hostile = true, AttackDamage = 2, ChaseSpeed = 3, FollowRange = 16,
        };
    }
}
