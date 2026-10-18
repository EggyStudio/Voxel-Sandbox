# Review

Direction for the session working on this repository, written by a second session that reads the
code and the history and writes none of the game. The Now list is read before a batch is started
and before each commit. This file has one writer. The session doing the work edits the Replies
section only. An item is removed from here once the commit that settles it has been read.

Reviewed up to `4358f3b`, item 5 done and nothing else, 8 files. README.md's opening and AGENTS.md's
paragraph say what the game is, a voxel game set in an endless generated world whose blocks are
placed and broken, with its light, its water, its mobs and the items they drop, Minecraft named
afterwards only where its sizes, speeds and light levels are taken, "hostile mobs in its dark"
reading "where it is dark", and AGENTS.md's line on what the game grows toward gone; `Program.cs:1`
and the project file's comment lose the same likeness, and `build/publish.sh` names the alias that
opens the Microsoft Store where it had a stand-in. STYLE.md's Register section carries the rule, a
likeness is not a description and a figure of speech is not a term, another product named only where
a rule or a number is taken from it, a description holding no plan, with the README's pair as a row,
the ten idioms named and by hand kept in its literal sense, and its checks and COMMITS.md's
staged-diff pattern search for them. Each rewrite, read, says what the thing is and loses no fact.
Checked on the working machine were the strict build with no warning, the 102 tests, and the search
for the idioms finding STYLE.md's rule and checks and COMMITS.md's pattern alone; `build/play.sh`
was not run, comments and documents alone having changed. Right. Nothing is pending; the build and
the pack run are green at `b87e386`, and the owner pushed this commit at 21:31, its build run ending
green at 21:36, the tests on the three systems and the play on lavapipe. At 22:25 on 2026-10-18 the
owner resumed development on the game (Decision 6): the sky's sun, moon and stars as camera-facing
billboards with textures in place of the pixelated blobs (item 6), the sun's shadows pixelated (item
7), the sun's surround in the sky pixelated with the rest of the sky smooth (item 8), and the block
and item icons made anew (item 9); the radiance cascades' changes come through the engine's packages
after its commits. Next items 6 to 9 in order, one commit each.

Before it, `b87e386` came, item 4 done and nothing else, 58 files. STYLE.md has a Register section
with the rule and the owner's pairs, the engine's API names and Minecraft's glowstone keeping their
spelling and a game identifier its name unless the thing is new and the rename cheap, and its colon
rule reads that a running sentence holds no colon, one standing at the end of a sentence before a
bulleted list, a code block or a command and after a label in a table, its own text recast to it and
its table given the README-opener and exit-code cases; its colon check reads comments in C#, Slang,
shell and YAML and, with awk outside the code fences, the prose of the five Markdown files, and a
`git grep` for the pairs stands beside it. The sweep took the README, AGENTS.md, E3D.md and
COMMITS.md, every C# and Slang comment, the scripts and the workflows, and the printed strings, the
`voxel.gi` and `voxel.light` descriptions and answers, the settings window's labels and the save
warnings among them; `build/drive.sh` is `build/play.sh` with `build.yml`, `.gitignore`, AGENTS.md
and the README following, and the identifiers `Glows`, `Glow`, `GlowScale` and `BounceWindow` stay
as the rule allows. Read: the rewrites say the same thing in the rule's register, none loses a fact.
Checked on the working machine: the strict build, the 102 tests, `build/play.sh` on the GPU in 17
seconds with the player walked 2.8 blocks and no validation error, and the pairs found by `git grep`
only where the rule and its checks name them. Right. Two things of the engine's found here go to its
item 29, the title of its global-illumination window and a colon in its API logger's warnings. The
owner pushed it at 15:17 and its build run was whole green; the repository stands whole, every item
done and proven. At 20:34 on 2026-10-17 the owner read the README's opening and asked that no
likeness or figure of speech stand, item 5 under Decision 5, the one thing pending; the build and
the pack run are green.

Before it, `105bdd1` came, item 3's mend over `e0f4e66`, item 2, and nothing else. At `e0f4e66` the
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

4. **The register: STYLE.md's rule and the sweep (Decision 5), done at `b87e386`.** STYLE.md gains a
   section, Register: prose uses the technical and generic term, the engine's own where it has one,
   and no figure of speech or casual paraphrase, with the pairs the owner named and their
   replacements: the light that bounces becomes global illumination, radiance cascades where the
   technique is meant and indirect light where the effect is; gives off light and glowing become
   emissive; driving the game becomes controlling or scripting it through the command line, and a
   drive a scripted run. The colon joins it: STYLE.md's rule, which allows a colon before a list,
   tightens so that a running sentence holds none, the sentence recast or split where one appended
   an elaboration or a list, as in "makes the program the README opens with: a window with a turning
   cube"; a colon stands before a bulleted list, a code block or a command, and as a label in a
   table, and STYLE.md's check of colons joining clauses reads prose lines of Markdown as well as
   comments. The sweep covers README.md, AGENTS.md, every Markdown under `.github`, the comments of
   the code and the scripts, and the sentences the game prints, its commands' answers among them; 19
   files hold the first pair 52 times, and a rough count finds a colon inside a sentence on some 30
   prose lines of the documents. `build/drive.sh` becomes `build/play.sh`, new and cheap to rename,
   with `build.yml`, AGENTS.md and the README following. Checked by the strict build, the tests,
   `build/play.sh` and `git grep -i` for the pairs finding this file's history alone. One commit.

5. **The register widens to likenesses and figures of speech (Decision 5), README.md:3 and
   AGENTS.md:3 and 5, done at `4358f3b`.** The README opens with "A voxel game in the manner of
   Minecraft, and the ground floor of a larger one", and AGENTS.md's paragraph with the same
   likeness and with what the game grows toward; the owner read the opening on 2026-10-17 at 20:34
   and asked that no phrasing of the kind stand. The game is said by what it is, an endless
   generated voxel world whose blocks are placed and broken, with its light, its water, its mobs and
   the items they drop, the other game named only where a number or a rule is taken from it, as the
   README's later lines do with its sizes, speeds and light levels, and no line says what the game
   is the start of; AGENTS.md changes on this word of the owner's (N 7.4); STYLE.md's Register
   section takes the rule with this pair; and `git grep -i` for the idioms Decision 5 names finds
   nothing, by hand in its technical sense apart. Checked by review. One commit.

6. **The sun, the moon and the stars as camera-facing billboards with textures, the moon with its
   phases (Decision 6).** The owner said on 2026-10-18 at 22:25 that the sky's pixelated blobs
   standing for the stars, the moon and the sun look ugly. The sun and the moon become billboards
   facing the camera at their fixed places on the sky's turning, each with a texture drawn as pixel
   art, the moon's texture holding its eight phases, the count the other game has, shown by the day
   of the game's calendar; the stars become a particle effect parented to the sun's and the moon's
   fixed positions, so they turn with the sky, each star a camera-facing billboard of a few pixels
   fading in as the day fades, as `Sky.cs:119` has them fade; and the blobs go. `voxel.time` sets
   the hour as it does, and the phase follows the day count, a command setting the day. Checked by
   the tests of the sky's hour and phase, a capture at night from the drive, and `build/play.sh`.
   What the engine lacks for a parented billboard particle goes to the engine's ASKS.md (Decision
   4). One commit. Read against the package on 2026-10-18 at 22:50, the engine's billboards face the
   camera through the draw list, depth tested, so the sun and the moon are drawn with the point
   filter and the far plane as asked, the moon's eight phases from a sheet of four by two chosen by
   the day count and a `voxel.day` command setting the day; but the engine's particles are simulated
   on the GPU in world space, born at a random point within the emitter's radius, and the emitter's
   rotation and parent never move a particle already born, so a star field turning with the sky
   needs two things the engine lacks, an emitter whose particles live in its entity's space and turn
   with it and its parent, and particles placed where the program says and living until removed,
   which go to the engine's ASKS.md (Decision 4). The reviewer's call is one commit now, the sun and
   the moon as billboards, the blobs gone and the stars drawn by the game as camera-facing
   billboards of a few pixels on the sky's turning, fading in as the day fades, so they look and
   move as asked, and a second commit moving the stars to the engine's particles once a package
   brings the parented emitter, the item done then.

7. **The sun's shadows are pixelated (Decision 6).** The owner asked on 2026-10-18 at 22:25 that the
   shadows be pixelated too. The shadow map's texels are sized to the block grid, a texel a fraction
   of a block, and sampled without filtering, so a shadow's edge steps along the blocks' pixels
   rather than blurring; `voxel.shadows` keeps setting the distance. Where the engine's shadow
   settings offer no filter or texel size, the ask goes to the engine's ASKS.md (Decision 4) and the
   item waits on the package that brings it. Checked by a capture under a low sun in the drive and
   `build/play.sh`. One commit.

8. **The sun's surround in the sky is pixelated and the rest of the sky stays smooth (Decision 6).**
   The owner asked on 2026-10-18 at 22:25 that the sky around the sun be pixelated as the other game
   has it, the sky's gradient elsewhere smooth. The glow around the sun is drawn in the sky's pixel
   grid, the sun's billboard's pixel size, as a ring of pixel-art brightness that follows the sun,
   and the sky's gradient from horizon to zenith and its dawn and dusk colors stay smooth as
   `Sky.cs` paints them. Checked by captures at noon and at dusk in the drive and `build/play.sh`.
   One commit.

9. **The icons of the block and item types are made anew (Decision 6).** The owner asked on
   2026-10-18 at 22:25 that the icons for the different types of blocks and items be updated. The
   hotbar and the inventory draw a block as a colored square and an item by strokes
   (`Ui/Hud.cs:57-61`, `Ui/InventoryWindow.cs:256`); each block type gets an icon drawn from its own
   textures as a small cube seen from above and two sides, and each item type a pixel-art icon of
   its own, drawn once into an atlas the HUD and the inventory window read. Checked by a capture of
   the inventory open in the drive and `build/play.sh`. One commit.

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
   was. On 2026-10-17 at 20:34 the owner read the README's opening, "A voxel game in the manner of
   Minecraft, and the ground floor of a larger one", and asked that no phrasing of the kind stand
   either. A likeness is not a description and a figure of speech is not a term, so a thing is said
   by what it is and does, another product is named only where a rule or a number is taken from it,
   as the sizes, speeds and light levels are, and a plan is not written into a description; item 5,
   and the engines' items 29 and 25 under their Decisions 43 and 37.

6. **Development resumes; the sky, the shadows and the icons are the game's week.** On 2026-10-18 at
   22:25, with every run green on the engines and here, the owner resumed development on both
   engines and the game. For the game the owner named the sky's sun, moon and stars as camera-facing
   billboards with textures and the moon's phases (item 6), the sun's shadows pixelated (item 7),
   the sun's surround pixelated with the rest of the sky smooth (item 8), and the block and item
   icons made anew (item 9). The radiance cascades the engine works on this week reach the game
   through the engine's packages after its commits, so the game's lighting changes wait for them and
   the items above do not.

## Replies



