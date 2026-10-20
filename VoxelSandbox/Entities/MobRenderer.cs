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

    private sealed class Slot(ModelMesh mesh)
    {
        public readonly ModelMesh Mesh = mesh;
        public readonly List<Matrix4x4> Plain = [], Hurt = [], Flash = [];
    }

    // The game's mob shader, which takes the color out of the light from all around a mob and
    // floors each face's light at Minecraft's flat shade of its top's (resources/shaders/mob.slang).
    private readonly Shader _shader;
    private readonly ModelMaterial _skin, _hurt, _flash;
    private readonly List<Slot> _slots = [];
    // Each kind's pieces' slots, by kind, piece and variant, a piece of one color holding one.
    private readonly Slot[][][] _byKind;

    public MobRenderer()
    {
        _shader = LoadShader("resources/shaders/mob.slang");
        // Its vertex stage keeps the positions, so each pass leaves out the mobs it cannot see.
        SetShaderKeepsPositions(_shader, true);
        _skin = new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Opaque, Roughness = 0.85f, BouncesLight = false, Shader = _shader };
        _hurt = new ModelMaterial(new Color(255, 96, 96)) { AlphaMode = MaterialAlphaMode.Opaque, Roughness = 0.85f, BouncesLight = false, Shader = _shader };
        // A creeper's flash as its fuse burns, white and a little emissive.
        _flash = new ModelMaterial(Color.White) { AlphaMode = MaterialAlphaMode.Opaque, Roughness = 0.85f, BouncesLight = false, Shader = _shader, Emissive = Color.White, EmissiveIntensity = 0.6f };
        _byKind = new Slot[MobKinds.All.Length][][];
        foreach (var kind in MobKinds.All)
        {
            _byKind[kind.Id] = new Slot[kind.Parts.Length][];
            for (int p = 0; p < kind.Parts.Length; p++)
            {
                var part = kind.Parts[p];
                var variants = part.Varies ? kind.Variants.Length : 1;
                _byKind[kind.Id][p] = new Slot[variants];
                for (int v = 0; v < variants; v++)
                {
                    var slot = new Slot(Build(part, kind.Variants[v]));
                    _slots.Add(slot);
                    _byKind[kind.Id][p][v] = slot;
                }
            }
        }
    }

    /// <summary>Instanced draws the last <see cref="Draw"/> made.</summary>
    public int Draws { get; private set; }

    /// <summary>Mobs the last <see cref="Draw"/> drew.</summary>
    public int Drawn { get; private set; }

    // A piece's boxes as one mesh in blocks, its own boxes in a variant's color.
    private static ModelMesh Build(ModelPart part, Color variant)
    {
        var mesh = new BoxMesh();
        foreach (var box in part.Boxes) mesh.Add(box.Min / 16, box.Max / 16, box.Color ?? variant);
        return mesh.Upload();
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
            if (slot.Plain.Count > 0)
            {
                DrawMeshInstanced(slot.Mesh, _skin, CollectionsMarshal.AsSpan(slot.Plain));
                draws++;
            }
            if (slot.Hurt.Count > 0)
            {
                DrawMeshInstanced(slot.Mesh, _hurt, CollectionsMarshal.AsSpan(slot.Hurt));
                draws++;
            }
            if (slot.Flash.Count > 0)
            {
                DrawMeshInstanced(slot.Mesh, _flash, CollectionsMarshal.AsSpan(slot.Flash));
                draws++;
            }
        }
        (Draws, Drawn) = (draws, drawn);
    }

    public void Dispose()
    {
        foreach (var slot in _slots) UnloadMesh(slot.Mesh);
        UnloadShader(_shader);
    }
}
