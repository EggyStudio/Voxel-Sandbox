// A voxel game built to test global illumination, with an endless generated world of colored
// blocks to walk, fly and build in, emissive blocks, and a sky with a sun that crosses it. `--flat`
// and `--seed <n>` pick the kind of world and its seed, `--world <name>` the save it is kept in,
// its kind and seed by default, `--transient` keeps nothing on disk, as a test run does, and each
// `--pack <path>` a Minecraft resource pack, a zip or a folder, whose textures the blocks and items
// are drawn with, the first given over the rest.
using System.Runtime.InteropServices;
using Engine;
using VoxelSandbox;
using static Engine.E3D;

var seed = 1;
var flat = false;
var transient = false;
string? name = null;
var packs = new List<string>();
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--flat") flat = true;
    else if (args[i] == "--transient") transient = true;
    else if (args[i] == "--seed" && i + 1 < args.Length && int.TryParse(args[i + 1], out var number)) seed = number;
    else if (args[i] == "--world" && i + 1 < args.Length) name = args[i + 1];
    else if (args[i] == "--pack" && i + 1 < args.Length) packs.Add(args[i + 1]);
}

SetConfigFlags(ConfigFlags.WindowResizable);
InitWindow(1280, 720, "Voxel Sandbox");
// The game's own shaders and packs.json are read from beside the program, and the engine draws on
// with its own shader where one is missing or does not compile, so the game stops here with the
// file's path rather than drawing the land wrong.
if (OwnFiles.Fault() is { } fault)
{
    TraceLog(LogLevel.Error, fault);
    CloseWindow();
    return 1;
}
// The window takes the resolution and the mode the options file keeps before its first frame and
// before the world opens, so it shows as it was left, and 1280 by 720 in a window where the file
// keeps neither. A transient run reads no options.
var video = new VideoSettings();
if (!transient) video.Restore(OptionsFile.Default);
// Escape frees the cursor, as it opens Minecraft's menu, rather than closing the window.
SetExitKey(Key.Null);
// F1 hides the HUD and F3 shows the debug overlay, the engine's own, and F2 is the game's own
// screenshot, which says on the HUD where it went and keeps none in a transient run, so the
// engine's screenshot on it is turned off.
InitAudioDevice();
SetScreenshotKey(Key.Null);

// A request to end the process, as a session's close, a kill by name or Control and C sends, ends
// the frames as the window's close does, so the world is saved as the game closes.
using var ending = new CancellationTokenSource();
void End(PosixSignalContext context)
{
    context.Cancel = true;
    ending.Cancel();
}
using var terminated = PosixSignalRegistration.Create(PosixSignal.SIGTERM, End);
using var hungUp = PosixSignalRegistration.Create(PosixSignal.SIGHUP, End);
using var interrupted = PosixSignalRegistration.Create(PosixSignal.SIGINT, End);

using (var game = new VoxelGame(flat ? "flat" : "overworld", seed, name, transient, packs, video))
{
    GameCommands.Game = game;
    OverlaySections.Add(game);
    // The game's frame runs in the engine's stages, which Draw's BeginDrawing and EndDrawing run.
    while (!WindowShouldClose() && !game.Menus.QuitAsked && !ending.IsCancellationRequested) game.Draw();
    GameCommands.Game = null;
}

CloseAudioDevice();
CloseWindow();
return 0;
