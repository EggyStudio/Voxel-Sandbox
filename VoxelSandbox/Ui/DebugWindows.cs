using System.Numerics;
using ImGuiNET;
using static Engine.E3D;

namespace VoxelSandbox;

/// <summary>
/// The engine's windows of global illumination, the frame's profile and the effects over the frame,
/// each drawn while the Debug screen of the options has it on.
/// </summary>
public sealed class DebugWindows
{
    public bool BounceWindow { get; set; }

    public bool ProfileWindow { get; set; }

    /// <summary>Whether the engine's window of the effects over the frame is drawn, the tonemap, exposure, grading, bloom and the rest.</summary>
    public bool PostProcessWindow { get; set; }

    public void Draw()
    {
        if (BounceWindow) DrawBounceWindow();
        if (ProfileWindow) DrawProfileWindow();
        if (PostProcessWindow)
        {
            // At the window's right edge the first time, clear of the overlay at its left.
            ImGui.SetNextWindowPos(new Vector2(GetScreenWidth() - 10, 10), ImGuiCond.FirstUseEver, new Vector2(1, 0));
            DrawPostProcessWindow();
        }
    }
}
