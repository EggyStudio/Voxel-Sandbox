using System.Numerics;
using Engine;
using Xunit;

namespace VoxelSandbox.Tests;

/// <summary>The looks the render settings switch, the lamps' color, the rim along the blocks' edges and the unshaded look, in a whole game.</summary>
[Collection(nameof(HeadlessGames))]
public sealed class LooksFrameTests
{
    [Fact]
    public void Each_Look_Is_A_Setting_And_A_White_Lamp_Is_As_Bright_As_Its_Color_With_Its_Face_Kept()
    {
        using var headless = new HeadlessGame();
        var game = headless.Game;
        var renderer = game.Renderer;
        var render = game.Menus.Screens(game).Render.Options;
        Option Setting(string name) => render.Single(o => o.Name == name);

        // A lantern's light is its own orange until the setting turns it white, as bright to the eye.
        var glass = Surfaces.All[Surfaces.LanternGlass];
        var orange = ChunkRenderer.LinearOf(glass.Emissive);
        Assert.True(renderer.ColoredLamps);
        Assert.Equal(orange, renderer.LightOf(glass.Id));
        Setting("Colored Lamps").Change(0);
        Assert.False(renderer.ColoredLamps);
        var white = renderer.LightOf(glass.Id);
        Assert.True(white.X == white.Y && white.Y == white.Z, $"the white light is {white}");
        Assert.Equal(ChunkRenderer.Brightness(orange), ChunkRenderer.Brightness(white), 4);

        // A pale flame's pixel shows the same under either light, its emission map dividing it by
        // the light of its material, where neither light clips it.
        var flame = new Image([230, 200, 160, 255], 1, 1);
        Vector3 Shown(Vector3 light)
        {
            var map = ChunkRenderer.EmissionMap(flame, glass, light).Data;
            return ChunkRenderer.LinearOf(new Color(map[0], map[1], map[2])) * light * glass.Glow;
        }
        var (colored, plain) = (Shown(orange), Shown(white));
        for (int c = 0; c < 3; c++)
            Assert.InRange(plain[c] / colored[c], 0.97f, 1.03f);

        Setting("Rim Light").Change(40);
        Setting("Unshaded").Change(1);
        Assert.Equal(0.4f, renderer.Rim, 3);
        Assert.True(renderer.Unshaded);
        // The game draws on with each look set and the lamps white.
        headless.Step(5);
        Setting("Colored Lamps").Change(1);
        Assert.Equal(orange, renderer.LightOf(glass.Id));
    }
}
