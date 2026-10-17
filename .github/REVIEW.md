# Review

Direction for the session working on this repository, written by a second session that reads the
code and the history and writes none of the game. The Now list is read before a batch is started
and before each commit. This file has one writer. The session doing the work edits the Replies
section only. An item is removed from here once the commit that settles it has been read.

Reviewed up to `105bdd1`, item 3's mend over `e0f4e66`, item 2, and nothing else. At `e0f4e66` the
namespace is `VoxelSandbox` and the tests' `VoxelSandbox.Tests`, 27 files taking `using Engine;` as
the compiler named them, `GameCommands.cs` qualifying `Furnaces` and `Furnace`, `GenerationTests.cs`
naming `VoxelSandbox.Blocks` beside its method of that name, the window "Voxel Sandbox", saves under
`VoxelSandbox/saves` with the README naming the folder the engine's builds saved in, and the tests'
temporary folders renamed; nothing else moved. At `105bdd1` `WorldSave.cs` reads and writes
`WorldInfo` and `FurnaceRecord[]` through `SaveJson`, a `JsonSerializerContext` the generator fills,
indented as before so an older save reads the same, which `ItemTests` holds; `WorldSave.Root` asks
for the local application data folder with `SpecialFolderOption.Create`, since on an account where
it does not exist the path is empty and the saves fell under the working folder, where the program's
own file stood in their way; and `build/publish.sh` fails the publish on a trim or AOT warning at a
line of the game's own code and runs the Linux zip's program twice with its data folder in a
temporary one, a flat world saved on quitting and asked for again by its name, which opens flat only
where the save was read back. Each fix was proven needed on the working machine: the first run dying
at `CreateDirectory`, then at reflection-based serialization disabled, then passing with the publish
warning of AssimpNetter alone. Checked besides: the strict build, the 102 tests, `./e3d status`
naming the game, `build/drive.sh`. Right. The owner pushed `c08d259` and `e0f4e66` at 13:37, before
`105bdd1` was committed, and the first build run was whole green at 13:42: the tests on Linux in
half a minute, on macOS in one and on Windows in one and a half, and the play job in three and a
half, the game played on lavapipe through the engine's command line built from its checkout.
`105bdd1` was pushed at 13:53 and its build run was whole green as well; the pack run on it, started
at 14:29, was whole green in nine minutes, the shaders compiled in two, the tests and the play as
before, and the three publishes in one to two minutes each, the zips `VoxelSandbox-0.1.2` for
linux-x64, win-x64 and osx-arm64 at 13 to 17 MB kept as its artifacts, the Linux one's program
having saved a world and opened it again with no `slangc`; item 3 is done. The repository stands
whole; item 4, the register, is the work left.

Before it, `c08d259` came, item 1 done and nothing else: the old game's files gone and the game's 61
files and its tests' 16 in as the engine's `fc5fdc97` held them, byte for byte, with the two project
files and the README the only files written anew, the game on `PackageReference 3DEngine 5.2.43`
with its generator from the package, the tests referencing the game, `Directory.Build.props` with
`net10.0`, `Nullable`, `ImplicitUsings`, English resources, compiled documentation with CS1591
allowed and `ImportDirectoryBuildTargets` off, since a checkout inside the engine's would take the
engine's targets from the folder above; `.gitignore`, `.gitattributes`, the `e3d` wrapper,
`AGENTS.md`, the README, `STYLE.md`, `COMMITS.md` and `E3D.md` written for the game,
`build/fetch-slang.sh`, `fetch-slang.ps1`, `version.sh`, `version.txt`, `drive.sh`, `shaders.sh` and
`publish.sh`, and the two workflows, `build.yml` testing on three systems and playing on lavapipe
with the engine checked out at `fc5fdc97`, `pack.yml` calling it, compiling the shaders, publishing
three zips and releasing when asked. Three things differ from item 1 and are better: the shaders job
runs `build/shaders.sh`, which compiles every build of every shader from one folder with `./e3d
shaders`, where a run of the game on lavapipe compiled one of the terrain's five in two minutes and
26 of CPU, so a device with ray queries or global illumination off would have found its build
missing from a zip; the build workflow's concurrency group takes `pack-` on a `workflow_dispatch`
event, as the engine's takes it from its input; and the paths filter leaves out BUILDING.md, which
the game has not. Checked on the working machine: the strict build from an empty package folder, the
tests 102 cases (96 facts and a theory's six), `./e3d open` through the wrapper answering
`voxel.state` and listing 43 commands, `build/drive.sh` passing on the GPU in 23 seconds and on
lavapipe held to four cores in ten minutes with no validation error, the command line built from a
fresh checkout at `fc5fdc97`, and `build/publish.sh` publishing a Linux zip whose program drew 60
frames with no `slangc`. Right. Found ahead of item 3: the native publish warns IL2026 and IL3050 at
`WorldSave.cs` lines 160, 174, 280 and 298, where `System.Text.Json` serializes by reflection, and
the native program saving a world ended at 32 seconds with reflection-based serialization disabled,
so a player's build ends at its first save where a `--transient` run saves nothing; item 3 says the
mend. Next item 2, the names, then item 3's mend, then the owner's push and the first two runs.

## Now

1. **The replacement, one commit (Decision 1), done at `c08d259`.** The old game goes:
   `Project.sln`, `Voxel-Sandbox.csproj`, `Program.cs`, `Assets/`, `FodyWeavers.xml`, the Visual
   Studio `.gitignore` and the old README. The game comes in with its files moved as they are:
   `VoxelSandbox/` (the `3DEngine.Game` sources) and `VoxelSandbox.Tests/` (the 14 files, 97 facts
   and theories), `VoxelSandbox.slnx`, `Directory.Build.props` (`net10.0`, `Nullable`,
   `ImplicitUsings`), a `.gitignore` of the engine's shape (`bin/`, `obj/`, `imgui.ini`,
   `build/tools/`, `captures/`, `build/publish/`), `.gitattributes` (`* text=auto eol=lf`),
   `README.md` rewritten from the game's own (what it is, controls, commands, `dotnet run --project
   VoxelSandbox`, `./e3d open VoxelSandbox/bin/Debug/net10.0/VoxelSandbox --hidden --transient`,
   `dotnet test VoxelSandbox.Tests`, building with `build/fetch-slang.sh`, shipped builds, the
   `legacy` branch), `LICENSE` (MIT, the year updated, Decision 2), `AGENTS.md` adapted from the
   engine's (one paragraph; controlling the game with `./e3d`; the documents table with STYLE,
   COMMITS, E3D, REVIEW, NORM and the engine's ASKS.md by path; building and testing; "Where things
   are" with a row a folder; the conventions less SHARED.md and CHEATSHEET.md; the engine used
   through its package alone, what it lacks written in ASKS.md), `.github/STYLE.md`, `COMMITS.md`
   (less the `PublicApi.txt` paragraph), `E3D.md` (the game's binary in the first block),
   `CODE_OF_CONDUCT.md`, one capture under `.github/assets/`, `build/` with `fetch-slang.sh`,
   `fetch-slang.ps1`, `version.sh`, `version.txt` (`0.1`), `drive.sh` and `publish.sh`, the `e3d`
   wrapper (`exec "${ENGINE_CHECKOUT:-$root/..}/e3d" "$@"`, Decision 3) and the two workflows.
   `VoxelSandbox.csproj`: `Exe`, `RootNamespace VoxelSandbox`, `PackageReference 3DEngine 5.2.43` in
   place of both project references (the package carries the generator and code fixes as analyzers),
   `resources/**` as Content; the tests' csproj references the game project and the xunit packages
   the engine's test project named; no `nuget.config`, nuget.org being the source. `build.yml` on a
   push: `test (os)` on ubuntu-24.04, windows-latest and macos-15 (30 minutes; `dotnet build
   VoxelSandbox.slnx -warnaserror`, `dotnet test VoxelSandbox.Tests --no-build`, the results
   uploaded on failure), and `play` on ubuntu-24.04 (40 minutes; the engine checked out at
   `fc5fdc97` under `.engine` with `ENGINE_CHECKOUT` pointing at it, lavapipe and the validation
   layer, `build/fetch-slang.sh`, `build/drive.sh`, the captures uploaded); `build/drive.sh` on the
   shape of the engine's `build/drive-game.sh`: open offscreen `--transient --json` under
   `ENGINE_VULKAN_VALIDATION=1`, a flat world and a stone room with a glowstone block, the player
   put down and turned, `frames.wait 120`, `voxel.state` before and after `input.key W 30` with the
   player moved, a shot, stop, the log read for validation errors. `pack.yml` by hand with the input
   `publish`: `build` (uses `build.yml`), `shaders` (20 minutes; a Debug build, `slangc`, lavapipe,
   the game run once offscreen 120 frames `--transient`, `source/.slang-cache/terrain.*.spv`
   asserted, the cache uploaded with hidden files), `publish (os)` needing both (linux-x64, win-x64,
   osx-arm64; 40 minutes; `build/publish.sh <version> <rid>`: `dotnet publish -c Release -r <rid>
   -p:PublishAot=true`, the publish refused where the dll is left beside the program, the cache
   copied into `source/.slang-cache`, README and LICENSE in, `*.pdb`, `*.dbg` and `*.xml` out,
   zipped with `python -m zipfile`, on Linux the zip's program run offscreen 60 frames, uploaded as
   `VoxelSandbox-<version>-<rid>`), `release` (`if: ${{ inputs.publish }}`, `contents: write`, `gh
   release create v<version>` with the three zips). Both files with `permissions: contents: read`,
   concurrency `build-${{ github.ref }}` cancelling in progress and `pack-${{ github.ref }}` not,
   the engine's paths filter less `games/**.md`, a time limit on every job with its reason. Checked
   by `dotnet build VoxelSandbox.slnx -warnaserror` after `dotnet nuget locals all --clear`, `dotnet
   test VoxelSandbox.Tests` saying 97 passed, `./e3d open ... --offscreen --transient` answering
   `voxel.state`, and `build/drive.sh` passing here.

2. **The names, one commit after item 1, done at `e0f4e66`.** The namespace `Engine.Game` becomes
   `VoxelSandbox` and `Engine.Game.Tests` becomes `VoxelSandbox.Tests` (57 files relied on the
   parent namespace for `Engine` and get `using Engine;`; `GameCommands.cs` lines 241, 246 and 251
   qualify `Furnaces` and `Furnace`); the window title "3DEngine.Game" becomes "Voxel Sandbox"; the
   save root under the local application data becomes `VoxelSandbox`, the README saying where saves
   were. The 43 `voxel.*` commands, `Measurements.cs` and the flags `--flat`, `--seed`, `--world`
   and `--transient` stay. Checked by the strict build and the 97 tests.

3. **The first pack run's proof, its known failure mended at `105bdd1`; done on the pack run of
   2026-10-17.** A native publish of this game has not been tried where Slide's and Courtyard's
   have; no reflection was found in it and its commands register through the generator, so none is
   expected, and the first pack run is the proof: three zips, the Linux one unzipped on a machine
   without `slangc` and run, its log naming no compiler and its terrain not the magenta fallback.
   One thing is known before it: the native program cannot save a world, since `WorldSave.cs`
   serializes through `System.Text.Json`'s reflection, which native code disables, and the publish
   warns IL2026 and IL3050 at its lines 160, 174, 280 and 298. Mended in a commit after item 2 with
   a source-generated `JsonSerializerContext` for the save's types, or a serializer the native build
   keeps, and `build/publish.sh`'s run of the zip's program saving a world into a folder of its own,
   read back, as the check that fails without it. What else the publish refuses or the program lacks
   is written here as a verdict.

4. **The register: STYLE.md's rule and the sweep (Decision 5).** STYLE.md gains a section, Register:
   prose uses the technical and generic term, the engine's own where it has one, and no figure of
   speech or casual paraphrase, with the pairs the owner named and their replacements: the light
   that bounces becomes global illumination, radiance cascades where the technique is meant and
   indirect light where the effect is; gives off light and glowing become emissive; driving the game
   becomes controlling or scripting it through the command line, and a drive a scripted run. The
   colon joins it: STYLE.md's rule, which allows a colon before a list, tightens so that a running
   sentence holds none, the sentence recast or split where one appended an elaboration or a list, as
   in "makes the program the README opens with: a window with a turning cube"; a colon stands before
   a bulleted list, a code block or a command, and as a label in a table, and STYLE.md's check of
   colons joining clauses reads prose lines of Markdown as well as comments. The sweep covers
   README.md, AGENTS.md, every Markdown under `.github`, the comments of the code and the scripts,
   and the sentences the game prints, its commands' answers among them; 19 files hold the first pair
   52 times, and a rough count finds a colon inside a sentence on some 30 prose lines of the
   documents. `build/drive.sh` becomes `build/play.sh`, new and cheap to rename, with `build.yml`,
   AGENTS.md and the README following. Checked by the strict build, the tests, `build/play.sh` and
   `git grep -i` for the pairs finding this file's history alone. One commit.

## Verdicts

No verdict is open.

## Decisions

1. **The old game is replaced in place, its history kept.** The owner chose on 2026-10-17 that the
   voxel game of the engine's `3DEngine.Game` comes here in place of the old sandbox rather than
   beside it, the old files removed in the commit that brings the new in; the `legacy` branch and
   this history keep the old game. The game's commits in the engine's history stay there, cited by
   hash in the engine's REVIEW.md, and are not imported.

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
   `.github/ASKS.md` with what it measured and how, headed with this repository's name, and reads
   the engine's REVIEW.md for the `Review:` line the entry gets; the engine's session writes nothing
   there, and this repository holds no ASKS.md of its own. The game's tests run here on every push,
   which closes the entry of 2026-10-10 that said they ran nowhere.

5. **Prose is technical and generic.** The owner said on 2026-10-17 that phrasings such as "the
   light that bounces" and "how to drive the game" read as casual and do not fit. A feature is named
   by its technical term, the engine's own where it has one, and never by a figure of speech or a
   paraphrase: global illumination, and radiance cascades for the technique, where the light
   bounced; emissive where a block gave off light; a running program controlled or scripted through
   the command line where it was driven, and a scripted run where a drive. The owner added at 14:37
   that the colon is overused, an elaboration or a list appended to a sentence after one where a
   sentence would do, in documents, comments and logs alike, which STYLE.md should have covered; its
   rule tightens so that a running sentence holds no colon. STYLE.md carries both rules with these
   pairs, as the engines' do (3DEngine's Decision 43), every document, comment and message a reader
   sees is swept (item 4), and a new name follows the rule, `build/play.sh` where `build/drive.sh`
   was.

## Replies


