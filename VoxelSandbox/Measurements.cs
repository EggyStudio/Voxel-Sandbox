using System.Globalization;
using System.Numerics;
using System.Text;
using Engine;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// A recording of how much the picture changes from each frame to the next, before, during and
/// after the player turns or steps over some frames, to measure how long global illumination takes
/// to settle after the camera moves and how much it flickers meanwhile.
/// </summary>
/// <remarks>
/// Each frame's change is the mean over the window of each pixel's difference in red, green and
/// blue from the frame before, in sRGB levels from 0 to 255, and the share of pixels whose
/// difference in any of them passes 8 levels, read through <c>LoadImageFromScreen</c>, which waits
/// for the GPU each frame, so the frames come slower while it runs. Nothing else moves, so before
/// the motion the change is global illumination's own noise, during it the motion, and after it
/// what is left is global illumination settling.
/// </remarks>
public sealed class FlickerRun(int frames, int motionAt, int over, float turn, float step, string path)
{
    private readonly List<(double Mean, double Share)> _changes = [];
    private Image? _last, _stopped;
    private int _frame;

    /// <summary>Reads a frame and moves the player on the motion's frame.</summary>
    /// <returns>False once every frame is read and the recording is written to its file.</returns>
    public bool Step(VoxelGame game)
    {
        if (!Measurements.Keeping())
            return true;
        var image = LoadImageFromScreen();
        if (_last is { } last) _changes.Add((Measurements.MeanChange(last, image), Measurements.ShareChanged(last, image, 8)));
        _last = image;
        // The screen is read a frame late, so the first frame drawn after the motion ends is read two
        // frames after its last part.
        if (_frame == motionAt + over + 2) _stopped = image;
        if (_frame >= motionAt && _frame < motionAt + over)
        {
            // The motion in equal parts over its frames, the step along the way the player faced at its start.
            var player = game.Player;
            var ahead = new Vector3(-MathF.Sin(player.Yaw), 0, -MathF.Cos(player.Yaw));
            player.Heading += turn / over;
            player.Teleport(player.Body.Position + ahead * (step / over));
        }
        if (++_frame <= frames) return true;

        var text = new StringBuilder();
        text.AppendLine(CultureInfo.InvariantCulture, $"# the change of each frame from the one before, the mean in sRGB levels and the share of pixels past 8 levels, the player turning {turn} degrees and stepping {step} blocks over frames {motionAt} to {motionAt + over}");
        text.AppendLine("frame\tchange\tshare");
        for (int i = 0; i < _changes.Count; i++) text.AppendLine(CultureInfo.InvariantCulture, $"{i + 1}\t{_changes[i].Mean:0.0000}\t{_changes[i].Share:0.00000}");
        File.WriteAllText(path, text.ToString());

        // The first frame after the motion, the last, and their difference sixteen times over, which
        // shows where the picture went on changing.
        if (_stopped is { } stopped)
        {
            var stem = Path.ChangeExtension(path, null);
            ExportImage(stopped, stem + "-stopped.png");
            ExportImage(image, stem + "-settled.png");
            var difference = new byte[image.Data.Length];
            for (int i = 0; i < difference.Length; i += 4)
            {
                for (int c = 0; c < 3; c++) difference[i + c] = (byte)Math.Min(255, 16 * Math.Abs(stopped.Data[i + c] - image.Data[i + c]));
                difference[i + 3] = 255;
            }
            ExportImage(new Image(difference, image.Width, image.Height), stem + "-difference.png");
        }
        return false;
    }
}

/// <summary>Readings of the window's picture the game's measuring commands share.</summary>
public static class Measurements
{
    private static bool _keeping;

    /// <summary>
    /// Whether the engine keeps a copy of each frame, which it does from the first time the screen
    /// is read, so that first read is the clear color and is thrown away here.
    /// </summary>
    /// <returns>False on the first call, whose frame is not kept yet, and true after it.</returns>
    public static bool Keeping()
    {
        if (_keeping) return true;
        LoadImageFromScreen();
        _keeping = true;
        return false;
    }

    /// <summary>The mean over every pixel of the difference in red, green and blue between two pictures of one size, in sRGB levels.</summary>
    public static double MeanChange(Image a, Image b)
    {
        if (a.Width != b.Width || a.Height != b.Height) return double.NaN;
        long sum = 0;
        var (x, y) = (a.Data, b.Data);
        for (int i = 0; i < x.Length; i += 4)
            sum += Math.Abs(x[i] - y[i]) + Math.Abs(x[i + 1] - y[i + 1]) + Math.Abs(x[i + 2] - y[i + 2]);
        return sum / (3.0 * a.Width * a.Height);
    }

    /// <summary>The share of pixels whose red, green or blue differs between two pictures of one size by more than <paramref name="levels"/>.</summary>
    public static double ShareChanged(Image a, Image b, int levels)
    {
        if (a.Width != b.Width || a.Height != b.Height) return double.NaN;
        long changed = 0;
        var (x, y) = (a.Data, b.Data);
        for (int i = 0; i < x.Length; i += 4)
            if (Math.Abs(x[i] - y[i]) > levels || Math.Abs(x[i + 1] - y[i + 1]) > levels || Math.Abs(x[i + 2] - y[i + 2]) > levels) changed++;
        return (double)changed / (a.Width * a.Height);
    }

    /// <summary>The mean luminance in sRGB levels of the pixels within <paramref name="radius"/> of a pixel, or NaN off the picture.</summary>
    public static double Luminance(Image image, int x, int y, int radius)
    {
        double sum = 0;
        var count = 0;
        for (int dy = -radius; dy <= radius; dy++)
            for (int dx = -radius; dx <= radius; dx++)
            {
                int px = x + dx, py = y + dy;
                if ((uint)px >= image.Width || (uint)py >= image.Height) continue;
                var at = (py * image.Width + px) * 4;
                sum += 0.2126 * image.Data[at] + 0.7152 * image.Data[at + 1] + 0.0722 * image.Data[at + 2];
                count++;
            }
        return count == 0 ? double.NaN : sum / count;
    }

    /// <summary>The mean red, green and blue in sRGB levels of the pixels of a rectangle of the picture, or null where none of it is on the picture.</summary>
    public static (double R, double G, double B)? MeanColor(Image image, int x, int y, int width, int height)
    {
        double r = 0, g = 0, b = 0;
        var count = 0;
        for (int py = Math.Max(0, y); py < Math.Min(image.Height, y + height); py++)
            for (int px = Math.Max(0, x); px < Math.Min(image.Width, x + width); px++)
            {
                var at = (py * image.Width + px) * 4;
                (r, g, b) = (r + image.Data[at], g + image.Data[at + 1], b + image.Data[at + 2]);
                count++;
            }
        return count == 0 ? null : (r / count, g / count, b / count);
    }

    /// <summary>
    /// The picture's luminance at points around a circle on a horizontal plane, by angle from +x
    /// toward +z, and how far it swings, given as its mean, its lowest and highest, the swing over
    /// the mean, and how many peaks stand above their neighbors.
    /// </summary>
    public static string Ring(Image image, Camera3D camera, Vector3 middle, float radius, int samples)
    {
        var values = new double[samples];
        for (int i = 0; i < samples; i++)
        {
            var angle = 2 * MathF.PI * i / samples;
            var point = middle + new Vector3(MathF.Cos(angle), 0, MathF.Sin(angle)) * radius;
            // A point behind the eye projects onto the picture mirrored, so it is read as off it.
            if (!IsPointInFrontOfCamera(point, camera))
            {
                values[i] = double.NaN;
                continue;
            }
            var at = GetWorldToScreen(point, camera);
            values[i] = Luminance(image, (int)MathF.Round(at.X), (int)MathF.Round(at.Y), 2);
        }
        var seen = values.Where(v => !double.IsNaN(v)).ToArray();
        if (seen.Length == 0) return "no point of the circle is in the picture";
        double mean = seen.Average(), low = seen.Min(), high = seen.Max();
        // A peak is higher than the two samples on either side of it and than the mean.
        var peaks = 0;
        for (int i = 0; i < samples; i++)
        {
            var v = values[i];
            if (double.IsNaN(v) || v <= mean) continue;
            var higher = true;
            for (int d = 1; d <= 2 && higher; d++)
                higher = v > values[(i + d) % samples] && v > values[(i - d + samples) % samples];
            if (higher) peaks++;
        }
        var list = string.Join(" ", values.Select(v => v.ToString("0.0", CultureInfo.InvariantCulture)));
        return string.Create(CultureInfo.InvariantCulture,
            $"radius {radius}: mean {mean:0.00}, low {low:0.00}, high {high:0.00}, swing {(high - low) / mean:0.000} of the mean, {peaks} peaks; by angle {list}");
    }

    /// <summary>
    /// How many pixels of a rectangle of the picture stand darker than all four of their neighbors
    /// three pixels away, by more than so many levels of luminance, and how many stand lighter, as a
    /// star's few pixels do against the sky, out of the pixels read.
    /// </summary>
    public static (int Darker, int Lighter, int Read) Spots(Image image, int x, int y, int width, int height, double levels)
    {
        int darker = 0, lighter = 0, read = 0;
        for (int py = Math.Max(3, y); py < Math.Min(image.Height - 3, y + height); py++)
            for (int px = Math.Max(3, x); px < Math.Min(image.Width - 3, x + width); px++)
            {
                var at = Luminance(image, px, py, 0);
                double left = Luminance(image, px - 3, py, 0), right = Luminance(image, px + 3, py, 0), up = Luminance(image, px, py - 3, 0), down = Luminance(image, px, py + 3, 0);
                if (at < Math.Min(Math.Min(left, right), Math.Min(up, down)) - levels) darker++;
                if (at > Math.Max(Math.Max(left, right), Math.Max(up, down)) + levels) lighter++;
                read++;
            }
        return (darker, lighter, read);
    }

    /// <summary>The picture's color in sRGB levels at the pixel a point in the world lies on, or why none is read.</summary>
    public static string Pixel(Image image, Camera3D camera, Vector3 point)
    {
        if (!IsPointInFrontOfCamera(point, camera)) return "the point is behind the eye";
        var at = GetWorldToScreen(point, camera);
        int x = (int)MathF.Round(at.X), y = (int)MathF.Round(at.Y);
        if ((uint)x >= image.Width || (uint)y >= image.Height) return "the point is off the picture";
        var i = (y * image.Width + x) * 4;
        return $"red {image.Data[i]}, green {image.Data[i + 1]}, blue {image.Data[i + 2]} at {x}, {y}";
    }

    /// <summary>
    /// The picture's luminance at points evenly along a line in the world, from its first end to its
    /// second, and how smoothly it changes, given as its first and last values and the largest change
    /// between neighboring points as a share of the whole, which a light stepped on a grid across the
    /// line holds in a few large changes with none between them.
    /// </summary>
    public static string Line(Image image, Camera3D camera, Vector3 from, Vector3 to, int samples)
    {
        var values = new double[samples];
        for (int i = 0; i < samples; i++)
        {
            var point = Vector3.Lerp(from, to, i / (samples - 1f));
            if (!IsPointInFrontOfCamera(point, camera)) return "a point of the line is behind the eye";
            var at = GetWorldToScreen(point, camera);
            values[i] = Luminance(image, (int)MathF.Round(at.X), (int)MathF.Round(at.Y), 1);
            if (double.IsNaN(values[i])) return "a point of the line is off the picture";
        }
        var largest = 0.0;
        for (int i = 1; i < samples; i++) largest = Math.Max(largest, Math.Abs(values[i] - values[i - 1]));
        var whole = Math.Abs(values[^1] - values[0]);
        var list = string.Join(" ", values.Select(v => v.ToString("0.0", CultureInfo.InvariantCulture)));
        return string.Create(CultureInfo.InvariantCulture,
            $"from {values[0]:0.0} to {values[^1]:0.0}, largest step {largest:0.0}, {(whole > 0 ? largest / whole : 0):0.000} of the change; by place {list}");
    }
}
