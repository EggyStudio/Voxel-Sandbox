using Xunit;

namespace VoxelSandbox.Tests;

[Collection(nameof(Icons))]
public class IconTests
{
    private static int Alpha(byte[] icon, int x, int y) => icon[(y * Icons.Size + x) * 4 + 3];

    private static int Red(byte[] icon, int x, int y) => icon[(y * Icons.Size + x) * 4];

    [Fact]
    public void Every_Item_Has_An_Icon_Of_Its_Own()
    {
        var seen = new HashSet<string>();
        foreach (var item in Items.All)
        {
            var icon = Icons.Pixels(item);
            var shown = Enumerable.Range(0, Icons.Size * Icons.Size).Count(i => icon[i * 4 + 3] > 0);
            Assert.True(shown >= 6, $"{item.Key} shows {shown} pixels");
            Assert.True(seen.Add(Convert.ToBase64String(icon)), $"{item.Key} has the icon of another item");
        }
    }

    [Fact]
    public void A_Cube_Shows_Its_Top_Brightest_And_Its_Right_Side_Darkest_And_An_Emissive_One_Unshaded()
    {
        var stone = Icons.Pixels(Items.Of(BlockId.Stone));
        // The middles of the top, of the south side on the left and of the east side on the right.
        int top = Red(stone, 8, 3), left = Red(stone, 4, 9), right = Red(stone, 11, 9);
        Assert.True(top > left && left > right, $"top {top}, left {left}, right {right}");

        var glowstone = Icons.Pixels(Items.Of(BlockId.Glowstone));
        int lit = Red(glowstone, 8, 3);
        Assert.InRange(Red(glowstone, 4, 9), lit * 0.88f, lit * 1.12f);
        Assert.InRange(Red(glowstone, 11, 9), lit * 0.88f, lit * 1.12f);
    }

    [Fact]
    public void A_Slab_Keeps_A_Cubes_Width_With_Clear_Rows_Above_It_And_A_Torch_Stands_Larger_Than_Its_Share()
    {
        static (int Left, int Right, int Top, int Bottom) Extent(byte[] icon)
        {
            (int left, int right, int top, int bottom) = (Icons.Size, -1, Icons.Size, -1);
            for (int y = 0; y < Icons.Size; y++)
                for (int x = 0; x < Icons.Size; x++)
                    if (Alpha(icon, x, y) > 0) (left, right, top, bottom) = (Math.Min(left, x), Math.Max(right, x), Math.Min(top, y), Math.Max(bottom, y));
            return (left, right, top, bottom);
        }

        var cube = Extent(Icons.Pixels(Items.Of(BlockId.Stone)));
        var slab = Extent(Icons.Pixels(Items.Of(BlockId.StoneSlab)));
        Assert.Equal((cube.Left, cube.Right), (slab.Left, slab.Right));
        Assert.True(slab.Top >= 2, $"the slab's top row is {slab.Top}");

        // A torch is a stick two sixteenths across, which drawn at a cube's size is a pixel wide.
        var torch = Extent(Icons.Pixels(Items.Of(BlockId.Torch)));
        Assert.True(torch.Bottom - torch.Top >= 12 && torch.Right - torch.Left >= 2, $"the torch spans {torch}");
    }

    [Fact]
    public void The_Atlas_Keeps_A_Clear_Pixel_Around_Every_Icon()
    {
        var atlas = Icons.Atlas;
        Assert.Equal(Icons.Width * Icons.Height * 4, atlas.Length);
        Assert.True(Icons.Columns * Icons.Rows >= Items.All.Count);
        for (int y = 0; y < Icons.Height; y++)
            for (int x = 0; x < Icons.Width; x++)
                if (x % Icons.Cell is 0 or Icons.Cell - 1 || y % Icons.Cell is 0 or Icons.Cell - 1)
                    Assert.Equal(0, atlas[(y * Icons.Width + x) * 4 + 3]);
    }
}
