using System.Numerics;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The view from inside a block, its side's texture over the whole window and darkened, as
/// Minecraft draws the inside of a block the eye has gone into, so a camera set down in a wall shows
/// the wall rather than the faces around it from behind.
/// </summary>
/// <remarks>
/// It shows while the eye's cell holds a whole opaque block, in every mode but spectator, which flies
/// through the blocks and sees past them. Leaves, glass, ice, water and the blocks smaller than their
/// cell are left out, as Minecraft leaves out the blocks that do not block its view. The texture is
/// the resource packs' for the block's side, or the side's own color where no pack has one, darkened
/// to a tenth, repeated four times across the window and four down, and slid a whole repeat for each
/// 64 degrees the player turns or pitches, as Minecraft's screen effect draws it.
/// </remarks>
public static class InWall
{
    // How dark the inside is, a tenth of its texture's color.
    private static readonly Color Shade = new(26, 26, 26);

    private static ResourcePacks _packs = ResourcePacks.None;
    private static readonly Dictionary<string, Texture2D?> _textures = [];

    /// <summary>The block whose inside the view shows from an eye, or null where the eye is in the open, in a block that does not block the view, or in spectator.</summary>
    public static BlockId? Block(VoxelWorld world, Vector3 eye, GameMode mode)
    {
        if (mode == GameMode.Spectator) return null;
        var block = world.GetBlock((int)MathF.Floor(eye.X), (int)MathF.Floor(eye.Y), (int)MathF.Floor(eye.Z));
        return Blocks.IsOpaque(block) && !Blocks.IsLeaves(block) ? block : null;
    }

    /// <summary>Draws the insides with the textures of these resource packs, loading each as it is first drawn.</summary>
    public static void Use(ResourcePacks packs)
    {
        foreach (var texture in _textures.Values)
            if (texture is { } loaded) UnloadTexture(loaded);
        _textures.Clear();
        _packs = packs;
    }

    /// <summary>Draws a block's inside over the whole window, the player facing a heading clockwise from north and pitched up, both in degrees.</summary>
    public static void Draw(BlockId block, float heading, float pitch)
    {
        var side = Blocks.Get(block).Side;
        var window = new Rectangle(0, 0, GetScreenWidth(), GetScreenHeight());
        if (PackTextures.Of(side) is { } look && Texture(look.Texture) is { } texture)
        {
            // Turning right slides it left and looking up slides it down, as the world beyond would.
            float width = texture.Width, height = texture.Height;
            DrawTexturePro(texture, new Rectangle(heading / 64 * width, -pitch / 64 * height, 4 * width, 4 * height), window, Vector2.Zero, 0, Shade);
            return;
        }
        var color = Surfaces.All[side].Color;
        DrawRectangleRec(window, new Color((byte)(color.R / 10), (byte)(color.G / 10), (byte)(color.B / 10)));
    }

    private static Texture2D? Texture(string name)
    {
        if (_textures.TryGetValue(name, out var known)) return known;
        Texture2D? texture = null;
        if (_packs.Texture(name) is { } image)
        {
            var loaded = LoadTextureFromImage(image);
            SetTextureFilter(loaded, TextureFilter.Point);
            SetTextureWrap(loaded, TextureWrap.Repeat);
            texture = loaded;
        }
        return _textures[name] = texture;
    }
}
