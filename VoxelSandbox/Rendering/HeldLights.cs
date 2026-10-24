using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The light a block that gives light throws where it is carried or dropped, a torch, a lantern or
/// glowstone in the player's hand or lying on the ground, a point light of the block's own color
/// reaching as many blocks as its level of light, which follows it each frame.
/// </summary>
/// <remarks>
/// <para>
/// A held light stands at the player's own place, the body's middle at the eye's height, where
/// standing at the hand, a third of a block ahead and to the right, it swung with each turn of the
/// view and went inside a wall the player faced.
/// </para>
/// <para>
/// Each light casts shadows, so it lights no room through a wall, and global illumination takes
/// the lights where the scene's distance field lets them through, so the cascades carry a held
/// torch's light on to the walls around as they carry a placed one's. The held and the nearest
/// three dropped lights are kept, within the engine's twelve lights that cast shadows all around.
/// </para>
/// </remarks>
public sealed class HeldLights : IDisposable
{
    private const int Most = 4;

    /// <summary>
    /// How bright a light of level 15 shines, so a torch in the hand lights the ground about the
    /// player as a torch standing there lights it.
    /// </summary>
    public const float Strength = 3;

    private readonly List<(LightHandle Light, BlockId Block)> _lights = [];

    /// <summary>Puts a light at the player's place for what the hand holds, where that gives light, and at each of the nearest dropped stacks that do, as of this frame.</summary>
    public void Update(Vector3 carrier, Item? held, IReadOnlyList<ItemDrop> drops, Vector3 eye)
    {
        var wanted = new List<(Vector3 At, BlockId Block)>();
        if (held?.Block is { } carried && Blocks.Get(carried).Light > 0) wanted.Add((carrier, carried));
        foreach (var drop in drops.Where(d => d.Item.Block is { } b && Blocks.Get(b).Light > 0)
                     .OrderBy(d => Vector3.DistanceSquared(d.Body.Position, eye)).Take(Most - wanted.Count))
            wanted.Add((drop.Body.Position + new Vector3(0, 0.25f, 0), drop.Item.Block!.Value));

        for (int i = 0; i < Math.Max(wanted.Count, _lights.Count); i++)
        {
            // A light whose block changed or that is no longer wanted goes, since its reach is set
            // as it is made.
            if (i < _lights.Count && (i >= wanted.Count || _lights[i].Block != wanted[i].Block))
            {
                UnloadLight(_lights[i].Light);
                _lights[i] = (default, BlockId.Air);
            }
            if (i >= wanted.Count) continue;
            var (at, block) = wanted[i];
            var info = Blocks.Get(block);
            var (color, intensity) = (Surfaces.All[info.Side].Emissive, Strength * info.Light / 15f);
            if (i == _lights.Count) _lights.Add((CreatePointLight(at, color, intensity, info.Light, castsShadows: true), block));
            else if (_lights[i].Block == BlockId.Air) _lights[i] = (CreatePointLight(at, color, intensity, info.Light, castsShadows: true), block);
            else SetLightPosition(_lights[i].Light, at);
        }
        _lights.RemoveAll(l => l.Block == BlockId.Air);
    }

    public void Dispose()
    {
        foreach (var (light, _) in _lights) UnloadLight(light);
        _lights.Clear();
    }
}
