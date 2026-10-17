# Review

Direction for the session working on this repository, written by a second session that reads the
code and the history and writes none of the game. The Now list is read before a batch is started
and before each commit. This file has one writer. The session doing the work edits the Replies
section only. An item is removed from here once the commit that settles it has been read.

Reviewed up to `187c67c`, the last commit of the old game, a Direct3D 12 voxel sandbox on the 3.1.0
package with its assets in HLSL, kept on the `legacy` branch and in this history. On 2026-10-17 the
owner ordered the voxel game written in the engine's repository since 2026-10-10, `3DEngine.Game`
with its 97 tests, moved here in place of the old game, on the engine's published package 5.2.43,
committed the same way as the engines with the same `AGENTS.md`, `.github` files and two workflows
(the engine's Decision 41). Nothing of the move is committed yet; items 1 to 3 are the move, in
order. The engine's inbox for what the game finds the engine lacks stays the engine's
`.github/ASKS.md`, which this repository's session writes as before (Decision 4).

## Now

1. **The replacement, one commit (Decision 1).** The old game goes: `Project.sln`,
   `Voxel-Sandbox.csproj`, `Program.cs`, `Assets/`, `FodyWeavers.xml`, the Visual Studio `.gitignore`
   and the old README. The game comes in with its files moved as they are: `VoxelSandbox/` (the
   `3DEngine.Game` sources) and `VoxelSandbox.Tests/` (the 14 files, 97 facts and theories),
   `VoxelSandbox.slnx`, `Directory.Build.props` (`net10.0`, `Nullable`, `ImplicitUsings`), a
   `.gitignore` of the engine's shape (`bin/`, `obj/`, `imgui.ini`, `build/tools/`, `captures/`,
   `build/publish/`), `.gitattributes` (`* text=auto eol=lf`), `README.md` rewritten from the game's
   own (what it is, controls, commands, `dotnet run --project VoxelSandbox`, `./e3d open
   VoxelSandbox/bin/Debug/net10.0/VoxelSandbox --hidden --transient`, `dotnet test VoxelSandbox.Tests`,
   building with `build/fetch-slang.sh`, shipped builds, the `legacy` branch), `LICENSE` (MIT, the
   year updated, Decision 2), `AGENTS.md` adapted from the engine's (one paragraph; driving with
   `./e3d`; the documents table with STYLE, COMMITS, E3D, REVIEW, NORM and the engine's ASKS.md by
   path; building and testing; "Where things are" with a row a folder; the conventions less SHARED.md
   and CHEATSHEET.md; the engine used through its package alone, what it lacks written in ASKS.md),
   `.github/STYLE.md`, `COMMITS.md` (less the `PublicApi.txt` paragraph), `E3D.md` (the game's binary in
   the first block), `CODE_OF_CONDUCT.md`, one capture under `.github/assets/`, `build/` with
   `fetch-slang.sh`, `fetch-slang.ps1`, `version.sh`, `version.txt` (`0.1`), `drive.sh` and `publish.sh`,
   the `e3d` wrapper (`exec "${ENGINE_CHECKOUT:-$root/..}/e3d" "$@"`, Decision 3) and the two
   workflows. `VoxelSandbox.csproj`: `Exe`, `RootNamespace VoxelSandbox`, `PackageReference 3DEngine
   5.2.43` in place of both project references (the package carries the generator and code fixes as
   analyzers), `resources/**` as Content; the tests' csproj references the game project and the xunit
   packages the engine's test project named; no `nuget.config`, nuget.org being the source.
   `build.yml` on a push: `test (os)` on ubuntu-24.04, windows-latest and macos-15 (30 minutes;
   `dotnet build VoxelSandbox.slnx -warnaserror`, `dotnet test VoxelSandbox.Tests --no-build`, the
   results uploaded on failure), and `play` on ubuntu-24.04 (40 minutes; the engine checked out at
   `fc5fdc97` under `.engine` with `ENGINE_CHECKOUT` pointing at it, lavapipe and the validation
   layer, `build/fetch-slang.sh`, `build/drive.sh`, the captures uploaded); `build/drive.sh` on the
   shape of the engine's `build/drive-game.sh`: open offscreen `--transient --json` under
   `ENGINE_VULKAN_VALIDATION=1`, a flat world and a stone room with a glowstone block, the player put
   down and turned, `frames.wait 120`, `voxel.state` before and after `input.key W 30` with the player
   moved, a shot, stop, the log read for validation errors. `pack.yml` by hand with the input
   `publish`: `build` (uses `build.yml`), `shaders` (20 minutes; a Debug build, `slangc`, lavapipe, the
   game run once offscreen 120 frames `--transient`, `source/.slang-cache/terrain.*.spv` asserted, the
   cache uploaded with hidden files), `publish (os)` needing both (linux-x64, win-x64, osx-arm64; 40
   minutes; `build/publish.sh <version> <rid>`: `dotnet publish -c Release -r <rid> -p:PublishAot=true`,
   the publish refused where the dll is left beside the program, the cache copied into
   `source/.slang-cache`, README and LICENSE in, `*.pdb`, `*.dbg` and `*.xml` out, zipped with
   `python -m zipfile`, on Linux the zip's program run offscreen 60 frames, uploaded as
   `VoxelSandbox-<version>-<rid>`), `release` (`if: ${{ inputs.publish }}`, `contents: write`, `gh
   release create v<version>` with the three zips). Both files with `permissions: contents: read`,
   concurrency `build-${{ github.ref }}` cancelling in progress and `pack-${{ github.ref }}` not, the
   engine's paths filter less `games/**.md`, a time limit on every job with its reason. Checked by
   `dotnet build VoxelSandbox.slnx -warnaserror` after `dotnet nuget locals all --clear`, `dotnet test
   VoxelSandbox.Tests` saying 97 passed, `./e3d open ... --offscreen --transient` answering
   `voxel.state`, and `build/drive.sh` passing here.

2. **The names, one commit after item 1.** The namespace `Engine.Game` becomes `VoxelSandbox` and
   `Engine.Game.Tests` becomes `VoxelSandbox.Tests` (57 files relied on the parent namespace for
   `Engine` and get `using Engine;`; `GameCommands.cs` lines 241, 246 and 251 qualify `Furnaces` and
   `Furnace`); the window title "3DEngine.Game" becomes "Voxel Sandbox"; the save root under the local
   application data becomes `VoxelSandbox`, the README saying where saves were. The 43 `voxel.*`
   commands, `Measurements.cs` and the flags `--flat`, `--seed`, `--world` and `--transient` stay.
   Checked by the strict build and the 97 tests.

3. **The first pack run's proof.** A native publish of this game has not been tried where Slide's
   and Courtyard's have; no reflection was found in it and its commands register through the
   generator, so none is expected, and the first pack run is the proof: three zips, the Linux one
   unzipped on a machine without `slangc` and run, its log naming no compiler and its terrain not the
   magenta fallback. What the publish refuses or the program lacks is written here as a verdict.

## Verdicts

No verdict is open.

## Decisions

1. **The old game is replaced in place, its history kept.** The owner chose on 2026-10-17 that the
   voxel game of the engine's `3DEngine.Game` comes here in place of the old sandbox rather than
   beside it, the old files removed in the commit that brings the new in; the `legacy` branch and this
   history keep the old game. The game's commits in the engine's history stay there, cited by hash
   in the engine's REVIEW.md, and are not imported.

2. **The license stays MIT.** The repository's MIT license, the owner's own, stands with its year
   updated; the engine the game is written on is MPL-2.0, which a package reference does not carry
   into the game's code. The owner may say otherwise.

3. **`e3d` comes from the engine's checkout.** The engine's command line is a script of its checkout
   and not a tool package, so the game's `./e3d` wrapper runs the checkout `ENGINE_CHECKOUT` names
   or the folder above this one, which on the owner's machine is the engine, and the game's workflow
   checks the engine out at `fc5fdc97`, the commit of 5.2.43, under `.engine`. A tool package of the
   engine's command line is proposed to the owner for the engine's next package; the wrapper
   switches to it when it exists.

4. **What the game finds the engine lacks goes to the engine's ASKS.md, as before.** The engine's
   Decision 26 of 2026-10-10 stands: this repository's session writes an entry in the engine's
   `.github/ASKS.md` with what it measured and how, headed with this repository's name, and reads the
   engine's REVIEW.md for the `Review:` line the entry gets; the engine's session writes nothing
   there, and this repository holds no ASKS.md of its own. The game's tests run here on every push,
   which closes the entry of 2026-10-10 that said they ran nowhere.

## Replies

Item 1, the replacement. The old game's files are removed, and the game's 61 files and its tests'
16 come in as `3DEngine.Game` and `3DEngine.Game.Tests` held them at the engine's `fc5fdc97`, taken
by `git archive`, the game's README made the root's. `dotnet build VoxelSandbox.slnx -warnaserror
--no-incremental` built with no warning from an empty package folder, named by `NUGET_PACKAGES` in
place of `dotnet nuget locals all --clear`, which would have emptied the folders the engine's
sessions build from, and `dotnet test VoxelSandbox.Tests` passed 102 cases, the 96 facts and the
theory's six. `./e3d open VoxelSandbox/bin/Debug/net10.0/VoxelSandbox --offscreen --transient`
through the wrapper answered `voxel.state` and listed 43 `voxel.*` commands, and `build/drive.sh`
passed on the desktop's GPU in 23 seconds and on lavapipe held to four cores (`taskset -c 0-3`) in
10 minutes, the player walking 2.9 blocks toward the lamp, with no validation error. The engine's
command line built from a fresh checkout at `fc5fdc97` with no slangc, as the play job builds it.
The capture, `.github/assets/hills.webp`, is the hills of seed 1 from above, taken through `./e3d
shot` and encoded at 800 by 450 by the engine's `build/webp.sh`.

Item 1, where it differs from the item. The shaders job runs `build/shaders.sh`, a script of its
own, which compiles the package's shaders and `terrain.slang` from one folder with `./e3d shaders`,
rather than a run of the game. 120 frames on lavapipe took 2 minutes 19 seconds and 26 minutes of
CPU on 32 cores and compiled one of the terrain's five builds, the one this desktop's GPU loads as
well, so a device with ray queries, or the light that bounces turned off with G, would have found
its build missing from a zip. `e3d shaders` compiled all five in 75 seconds with no GPU, its 119
entries the same by name from the engine's command line at `fc5fdc97` and at its `HEAD`, the two
the running game wrote among them. The concurrency group of `build.yml` takes `pack-` where the
run's event is `workflow_dispatch`, as the engine's takes it from its input, since a called run
shares its caller's `github.ref`, and a push during a pack run would otherwise cancel the pack run's
build. The paths filter leaves out `.github/BUILDING.md` as well, which this repository does not have.
`.gitignore` ignores `.engine/` and `build/shader-cache/` too. `Directory.Build.props` sets
`ImportDirectoryBuildTargets` to false, since a checkout inside the engine's, where the wrapper
finds `e3d`, took the engine's `Directory.Build.targets` from the folder above, and keeps the
engine's settings of a documentation file with CS1591 off and English satellite resources alone.
`STYLE.md` has its Documentation, References and Scope sections written for the game, the engine's
being about its own CS1591 rule, its `.ref/` folder and its CHEATSHEET.md.

Item 3, found ahead of the pack run. `build/publish.sh 0.1.0 linux-x64
-p:CppCompilerAndLinker=gcc` published here, with no dll beside the program, and the zip's program
drew 60 frames on lavapipe with no slangc found and no shader missing from its cache, in 174
seconds on four cores. The publish warned IL2026 and IL3050 at `WorldSave.cs` lines 160, 174, 280
and 298, where `System.Text.Json` serializes by reflection, and the native program saving a world
(`--world check`, its data folder a temporary one) ended at 32 seconds with "Reflection-based
serialization has been disabled for this application", so a player's build ends at its first save.
A run with `--transient` saves nothing, which is why the zip's run passed. It is mended in a batch
after item 2, with the zip's run saving a world into a folder of its own as the check that fails
without the mend.
