# Review

Direction for the session working on this repository, written by a second session that reads the
code and the history and writes none of the game. The Now list is read before a batch is started
and before each commit. This file has one writer. The session doing the work edits the Replies
section only. An item is removed from here once the commit that settles it has been read.

Reviewed up to `4fc4809`, item 7's first commit and nothing else, 4 files. The terrain shader's
`pixelMiddle` moves the lit point to the middle of the block pixel it lies in on the two axes across
the face's normal, and the lighting takes that copy of the vertex output, so a shadow's edge steps
along the blocks' pixels, the lamps' and the bounced light taken at the same points, the header
comment saying so; the README gains a Shadows section saying what steps and that the engine's
contact shadow leaves a fringe about half a block wide since the package has no setting to turn it
off; `build/play.sh` raises a pillar of stone west of the room under the sun of 8.30 and captures it
from the air to the southeast, the capture holding one edge stepping along the pixels and the other
with the soft fringe, no test since the snapping lives in the shader. Checked were the strict build
with no warning, 107 tests, the style checks over the staged diff and the play on the GPU with no
validation error. The engine's ASKS.md carries the contact shadows' entry with the reviewer's line,
item 33 of the engine's REVIEW.md after its item 32, and item 7's second commit turns them off when
a package brings the setting. Next item 8, the owner's push of the two commits and their build run,
and items 6 and 7's second commits after the package.

Before it, `bfba72b` came, item 6's first commit and nothing else, 10 files. `Rendering/SkyArt.cs`
holds the pixel art as bytes, a sun of 12 pixels, a square with its corners cut, a moon of 12 with
craters whose eight phases sit in a sheet of four by two with a clear gutter of a pixel between
cells, since the point filter read the neighboring phase at a cell's edge without it, each phase lit
as a ball with its square pixels drawn onto the round face, and a star of three pixels; `Sky.cs`
draws the three with camera-facing billboards under the point filter at 3,000 and 3,200 units inside
the far plane and before the backdrop, land and clouds hiding them, turns the sky about the
north-south axis with the moon opposite the sun and 500 fixed stars turned by one matrix a frame,
fading in as the day fades and near the horizon, the painted disc, moon and stars gone from the map
and the halo kept for item 8; the day counts up at midnight while the cycle runs and a save keeps
it, the night's phase chosen by the day with the hours before noon belonging to the night before,
day 0's night full, `voxel.day` setting the day and naming the phase, `voxel.state` saying the day
and the settings window holding a field. `SkyTests` gains five tests, the day at midnight, the
phases by day and hour, the moon opposite the sun with the stars keeping their places, each phase's
lit pixels by side, and each cell's clear edge, 107 passing; `build/play.sh` captures the night sky
from the room's roof at hour 20, the full moon among the stars, passing on the GPU with no
validation error; the README says the sky. The other game is named where its count, its order of
phases and its first night's full moon are taken (Decision 5). Measured on the development machine's
GPU at 1280 by 720, drawing took 2.08 and 1.82 ms by night with the stars against 1.76 and 1.77 at
noon without. Right. The engine's ASKS.md carries the stars' entry with the reviewer's line, item 32
of the engine's REVIEW.md, and the second commit of item 6 moves the stars to the engine's particles
when a package brings it. Next item 7, the owner's push of this commit and its build run, and item
6's second commit after the package.

Before it, `4358f3b` came, item 5 done and nothing else, 8 files. README.md's opening and
AGENTS.md's paragraph say what the game is, a voxel game set in an endless generated world whose
blocks are placed and broken, with its light, its water, its mobs and the items they drop, Minecraft
named afterwards only where its sizes, speeds and light levels are taken, "hostile mobs in its dark"
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
   brings the parented emitter, the item done then. First commit done at `bfba72b` on 2026-10-18,
   the sun and the moon pixel art on camera-facing billboards turning with the sky, the moon's eight
   phases by a day count that `voxel.day` sets and a save keeps, the stars drawn by the game as
   billboards of three pixels, the blobs gone; the engine's ASKS.md entry became its item 32, and
   the second commit, the stars as the engine's particles, waits on the package that brings it.

7. **The sun's shadows are pixelated (Decision 6).** The owner asked on 2026-10-18 at 22:25 that the
   shadows be pixelated too. The shadow map's texels are sized to the block grid, a texel a fraction
   of a block, and sampled without filtering, so a shadow's edge steps along the blocks' pixels
   rather than blurring; `voxel.shadows` keeps setting the distance. Where the engine's shadow
   settings offer no filter or texel size, the ask goes to the engine's ASKS.md (Decision 4) and the
   item waits on the package that brings it. Checked by a capture under a low sun in the drive and
   `build/play.sh`. One commit. Read against the package on 2026-10-18 at 23:08, the engine's shadow
   settings are the map's size and the distance alone; its lookup offsets the point 1.5 texels along
   the normal and averages nine nearest texels, and the cascades split the distance at 12 and 45 of
   150 on tiles of 2,048 texels, which at the game's 96 blocks puts texels of about 0.01, 0.03 and
   0.09 blocks against a block pixel's 0.0625, so the two near cascades draw an edge already crisp
   but running diagonally across the pixels, and the far one steps by texels of 1.27 block pixels;
   the sun's light is also multiplied by a contact shadow traced through the scene field at half the
   window's size and blurred with the occlusion, which nothing in the package turns off short of the
   field itself, and it lays a soft fringe of up to half a block past a hard edge. The game can step
   the edge itself: the terrain shader moves the lit point to the middle of the block pixel it lies
   in, along the face, before the lighting, so the shadow's edge steps along the block pixels, and
   the lamps' and the bounced light are taken once a block pixel too, which a capture of the
   lamp-lit room reads the same. The reviewer's call is one commit at once with that snapping,
   `build/play.sh` and a capture under a low sun, and an entry in the engine's ASKS.md for a setting
   that turns the sun's contact shadows off, noting as a lesser thing the far cascade's texel of
   1.27 block pixels; a second commit turns the contact shadows off once a package brings the
   setting, the item done then. First commit done at `4fc4809` on 2026-10-18, the lit point snapped
   to the block pixel's middle in the terrain shader, the README's Shadows section and the pillar's
   capture in `build/play.sh`; the engine's ASKS.md entry became its item 33, and the second commit,
   the contact shadows off, waits on the package that brings the setting.

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



