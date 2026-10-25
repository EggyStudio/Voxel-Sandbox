// A voxel game built to test global illumination, with an endless generated world of colored
// blocks to walk, fly and build in, emissive blocks, and a sky with a sun that crosses it. `--flat`
// and `--seed <n>` pick the kind of world and its seed, `--world <name>` the save it is kept in,
// its kind and seed by default, `--transient` keeps nothing on disk, as a test run does, and each
// `--pack <path>` a Minecraft resource pack, a zip or a folder, whose textures the blocks and items
// are drawn with, the first given over the rest.
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
// Escape frees the cursor, as it opens Minecraft's menu, rather than closing the window.
SetExitKey(Key.Null);
// F1, F2 and F3 are the game's own, its HUD's hiding, its screenshot and its debug screen as
// Minecraft's, so the engine's hiding of the interface, its screenshot and its debug overlay on them
// are turned off.
SetUiHiddenKey(Key.Null);
InitAudioDevice();
SetScreenshotKey(Key.Null);
SetDebugOverlayKey(Key.Null);

using (var game = new VoxelGame(flat ? "flat" : "overworld", seed, name, transient, packs))
{
    GameCommands.Game = game;
    // The game's frame runs in the engine's stages, which Draw's BeginDrawing and EndDrawing run.
    while (!WindowShouldClose() && !game.Menus.QuitAsked) game.Draw();
    GameCommands.Game = null;
}

CloseAudioDevice();
CloseWindow();
