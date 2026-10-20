# Review

Direction for the session working on this repository, written by a second session that reads the
code and the history and writes none of the game. The Now list is read before a batch is started
and before each commit. This file has one writer. The session doing the work edits the Replies
section only. An item is removed from here once the commit that settles it has been read.

Reviewed up to `35696e2`, item 10's legless frame found and mended, one commit with its test, with
the review's commit between. The game's loop updates before its first drawing begins, and the engine
times a frame when it begins, so the frame's time reads nothing there; a mob's stride is the
distance moved over the frame's seconds, so a mob standing still in that frame got nothing over
nothing, which is not a number, the smaller of one and that is not a number, and not a number times
anything stays so, so the mob's stride and every leg's swing matrix were not numbers from then on
and its legs were drawn in no pass, their shadows with them; the mobs moved in that first frame are
those placed with the columns around the spawn before it, so the cows and pigs near the spawn
floated legless while the herds loaded later stood on their legs, which fits the owner's words and
the frame of 2026-10-19. The stride is left as it was in a frame that took no time, the one division
by the frame's seconds in the game; a mob test updates a cow for no seconds and then for a second
and holds its stride and phase finite, failing without the mend; read live on the flat world 120
frames after opening, 9 of 124 mobs had a stride that was not a number before, four pigs and four
cows among them near the spawn, and none after, the herd south-east of the spawn legless and
floating before and standing after; the engine's culling of instances in blocks of 64 was read on
the way and is not at fault. The strict build is clean and 126 tests pass. Right. The engine's item
36 is done at `089761fb`, so item 16 marks the terrain's shaders and raises the distance when the
package comes, which the reviewer's next pack run brings. Next item 16 on that package, the leak
read on it with both halves, and the pictures script's row in AGENTS.md on the owner's word.

Before it, `cc334ab` came, item 15 and nothing else, with the review's commits between. An opaque
block keeps its face toward a lamp block in its section's mesh, hidden behind the lamp's cube, so
the mesh is closed around the lamp, a whole emissive block drawn apart as a cube being named a lamp,
the see-through mesh left as it was since it casts no shadow and the field reads it not, the lamp
drawn as before and the README's paragraph on how the world is drawn saying why; a mesher test lays
a floor and a wall of stone with a glowstone in their corner and holds that no edge of the mesh
belongs to one triangle alone and that the floor's top and the wall's side toward the lamp are kept,
failing without the change; the strict build is clean and 125 tests pass. Read on `5.2.92`, which
lacks the engine's walling-in, at the leak scene with a 512-path reference: the corner probe, moved
into the stone before and holding 855 percent over the reference, is moved into the room's air and
holds 13 under, since the stone beside the lamp reads as inside; the grass at the wall's foot by the
lamp with the screen probes off from 14.1 times the reference to 1.67, its red over green from 1.40
to 0.46 against 0.36, the same row away from the lamp at 1.49 and 1.46; with the probes on the view
at 1.40 and 1.41 either way; the room inside unmoved. The package with the engine's `a7c2da8b` and
this commit together is read when it comes. Right. Next the legless frame's hunt, item 36's package
before the distance rises, and the pictures script's row in AGENTS.md on the owner's word.

Before it, `9158df8` came, item 12, with `569b5fb`, item 11, and `bde1fc1`, item 14, before it, and
the review's commits between. Item 14: the engine's somewhat boring curve with an exposure of 1.25
beside it at the light settings, the remark giving the figures over the play script's views, the
pillar 136.6 against 136.7, the noon sky 137.8 against 136.4 and the night room 31.8 against 28.2,
13 percent brighter, why the curve does that in the dark and what an exposure of 1 would do, naming
no one; the captures taken under it, the README with a paragraph on the curve and the exposure and
its picture retaken. Item 11: a pictures script takes both README pictures from the seed, the place,
the hour, the heading and the cloud drift it writes in, at the game's render distance, encoded at
800 by 450 through the engine checkout's script, the overworld of seed 1 opened transient with the
mobs held where the land put them; the village picture shows the village south of the spawn from its
meadow at hour 16, five houses with torches, a villager by the path facing the camera, the village's
chickens and two sheep, a cow and a pig, the four placed by the script from above since the land's
herds and the villagers at their homes stood out of view, said in its comment, so each run shows
them where the last did; the hills picture is item 6's view with the clouds drifted 80 blocks so the
sun stands clear of one; the still command holds the clouds as it holds the mobs and the clouds
command takes a drift; the README opens with the two pictures side by side in a table as the
engine's does, says which script takes them, and its commands paragraph names the new parts;
AGENTS.md's row for the build folder names the workflows' scripts alone and waits on the owner's
word to name the pictures script (N 7.4). Item 12: the distance stays at 8, the setting's remark
giving the GPU's 12.1 ms at 8 and 16.6, 21.8, 29.7 and 46.4 at 9, 10, 12 and 16 with the shadows at
4.3 against 23.5 and the reason, the engine culling none of the terrain's draws through the game's
shader in the depth pass and the three cascades, the README's section on what it costs carrying the
table at six distances with lavapipe's 0.58 and 0.92 seconds and losing its sentence on the engine
culling each pass. 124 tests pass and the strict build is clean at each commit. Open: the legless
frame on the hunt, the engine's item 36 before the distance can rise. On 2026-10-20 at 00:48 the
engine's coder, mending the leak through a wall, found the mesher leaving out the stone's faces
against a lamp block, so the scene field reads that stone as open; item 15. Right. Next item 15, the
pictures script's row in AGENTS.md on the owner's word, and the engine's packages as they come.

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

6. **The sun, the moon and the stars fixed on the sky's sphere with textures, the moon with phases,
   the glow around the sun and the moon as particles parented to them (Decisions 6 and 7).** The
   owner said on 2026-10-18 at 22:25 that the sky's pixelated blobs standing for the stars, the moon
   and the sun look ugly. The sun and the moon become billboards facing the camera at their fixed
   places on the sky's turning, each with a texture drawn as pixel art, the moon's texture holding
   its eight phases, the count the other game has, shown by the day of the game's calendar; the
   stars become a particle effect parented to the sun's and the moon's fixed positions, so they turn
   with the sky, each star a camera-facing billboard of a few pixels fading in as the day fades, as
   `Sky.cs:119` has them fade; and the blobs go. `voxel.time` sets the hour as it does, and the
   phase follows the day count, a command setting the day. Checked by the tests of the sky's hour
   and phase, a capture at night from the drive, and `build/play.sh`. What the engine lacks for a
   parented billboard particle goes to the engine's ASKS.md (Decision 4). One commit. Read against
   the package on 2026-10-18 at 22:50, the engine's billboards face the camera through the draw
   list, depth tested, so the sun and the moon are drawn with the point filter and the far plane as
   asked, the moon's eight phases from a sheet of four by two chosen by the day count and a
   `voxel.day` command setting the day; but the engine's particles are simulated on the GPU in world
   space, born at a random point within the emitter's radius, and the emitter's rotation and parent
   never move a particle already born, so a star field turning with the sky needs two things the
   engine lacks, an emitter whose particles live in its entity's space and turn with it and its
   parent, and particles placed where the program says and living until removed, which go to the
   engine's ASKS.md (Decision 4). The reviewer's call is one commit now, the sun and the moon as
   billboards, the blobs gone and the stars drawn by the game as camera-facing billboards of a few
   pixels on the sky's turning, fading in as the day fades, so they look and move as asked, and a
   second commit moving the stars to the engine's particles once a package brings the parented
   emitter, the item done then. First commit done at `bfba72b` on 2026-10-18, the sun and the moon
   pixel art on camera-facing billboards turning with the sky, the moon's eight phases by a day
   count that `voxel.day` sets and a save keeps, the stars drawn by the game as billboards of three
   pixels, the blobs gone; the engine's ASKS.md entry became its item 32, and the second commit,
   tried on `5.2.92` on 2026-10-19 with the stars as placed particles, showed a particle's square
   tilting on the screen with its place in the view and one alpha for an emitter's points, which was
   to be asked of the engine until the owner's word of Decision 7 made it moot: nothing of the sky
   faces the camera. The second commit draws the sun, the moon and the stars as quads fixed on the
   sky's sphere, their right side and up the sphere's at their place, turned with the hour as one,
   the stars one mesh of 500 quads with their alpha in the vertex color, sampled without filtering
   as before, and the halo around the sun as a quad fixed with the sun on the sun's own grid, its
   rings item 8's, the moon given a dimmer halo the same way, since a particle stands upright in the
   world and faces the camera, so as a particle the halo's rings would part from the sun's pixels by
   up to 45 degrees mid-morning and turn with the view, the fault Decision 7 names, which the game's
   coder measured at 21:09; the sun a bit smaller, as the owner asked at 21:09, its width on the sky
   the other game's, whose quad is 60 across at a distance of 100 with the visible sun the middle 8
   of its texture's 32 pixels, so 15 across at 100, about 8.6 degrees, and the moon 10 at 100, about
   5.7, where the game's were 9.0 and 6.6, the numbers said in the code's remark as taken from it,
   the reviewer's first figures having taken the quad's half-width for the whole; the README's sky
   picture taken anew; checked by a drive that turns and moves the camera while the bodies stand
   still on the sky, the captures retaken, and the stars below the horizon faded as before. Done at
   `6d71cd1` on 2026-10-19, on `5.2.92`, held by a test of the quads' sides and the night sky's
   capture.

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
   the contact shadows off, waits on the package that brings the setting. The second commit sets the
   engine's contact shadows to past the shadow map alone, so the stepped edge stands with no fringe
   within the 96 blocks and the land past the map keeps the soft shadow where it would have none,
   recaptures the pillar with both edges stepped and drops the fringe from the README's Shadows
   section.

8. **The sun's surround in the sky is pixelated and the rest of the sky stays smooth (Decision 6),
   done at `344ef5b`.** The owner asked on 2026-10-18 at 22:25 that the sky around the sun be
   pixelated as the other game has it, the sky's gradient elsewhere smooth. The glow around the sun
   is drawn in the sky's pixel grid, the sun's billboard's pixel size, as a ring of pixel-art
   brightness that follows the sun, and the sky's gradient from horizon to zenith and its dawn and
   dusk colors stay smooth as `Sky.cs` paints them. Checked by captures at noon and at dusk in the
   drive and `build/play.sh`. One commit. Done at `344ef5b` on 2026-10-18, six rings of brightness
   on the sun's own pixel grid behind the sun, the painted sky the smooth gradient alone, a test and
   the noon and dusk captures holding it.

9. **The icons of the block and item types are made anew (Decision 6).** The owner asked on
   2026-10-18 at 22:25 that the icons for the different types of blocks and items be updated. The
   hotbar and the inventory draw a block as a colored square and an item by strokes
   (`Ui/Hud.cs:57-61`, `Ui/InventoryWindow.cs:256`); each block type gets an icon drawn from its own
   textures as a small cube seen from above and two sides, and each item type a pixel-art icon of
   its own, drawn once into an atlas the HUD and the inventory window read. Checked by a capture of
   the inventory open in the drive and `build/play.sh`. One commit. Read against the engine on
   2026-10-18 at 23:21, the HUD can draw an atlas texture, and the inventory window cannot, since
   the engine's ImGui pass binds the font atlas's set once (`ImGuiRenderNode.cs:172`) and draws
   every command with it, ignoring the command's texture id, so an image of one of the game's
   textures draws the font atlas. The reviewer's call is one commit at once as items 6 and 7 went:
   `Ui/Icons.cs` draws every item's icon once into an atlas of 16-pixel cells with clear gutters,
   each block a small cube seen from above and two sides cast per pixel against the block's own
   boxes so slabs, stairs, torches, fences and doors keep their shapes, each face its surface's
   color under the plains tint with a fixed speckle for the texture, shaded top, left and right as
   the other game's inventory shades them, emissive faces unshaded, and each item that places no
   block pixel art of its own, the tools one shape a kind with the head in the tier's color; the
   atlas is a point-filtered texture the hotbar draws, and the inventory window draws the atlas's
   pixels as rectangles with each row's runs of one color merged, its cost measured with the
   creative window open; the strokes and the hotbar's emission ring go; an entry in the engine's
   ASKS.md asks for a texture id of a `Texture2D` that the ImGui pass binds per command, and a
   second commit moves the inventory onto the atlas texture when a package brings it. Checked by
   tests of the atlas, every item with an icon, a cube's three shades, a slab's clear rows and the
   clear gutters, a capture of the inventory open in `build/play.sh`, and the strict build. First
   commit done at `80ab301` on 2026-10-18, the atlas of 97 icons drawn once, the hotbar drawing it
   as a texture and the inventory window as merged rectangles at half a millisecond a frame, the
   four tests and the capture; the engine's ASKS.md entry became its item 34, and the second commit,
   the window on the atlas texture, waits on the package that brings it. The second commit done at
   `1477e82` on 2026-10-19, the window and the hotbar drawing the atlas as a texture through the
   engine's item 34, the rectangles gone, the inventory capture pixel for pixel the same.

10. **Some of the animals' feet do not show (Decision 6).** The owner wrote at 21:05 on 2026-10-19
    that some of the animals' feet do not show in the game. Read before it is mended, with a capture
    of each animal kind standing and walking on flat ground and on a slope: which animals, and
    whether the feet stand below the ground's surface by the model's origin or the mob's height
    above its block, are culled as faces turned away, are hidden by the pixelated shadow or the
    contact shadow of item 7, or are not in the model at all; then one commit for the cause found,
    with a test that holds each animal's lowest face at or above the ground it stands on, and the
    captures that show it. Read at 22:04 on `1477e82` as the head says, three candidates and no one
    cause: the toes dipping below the ground at a stride's ends, the mobs in shade almost black
    against lit grass, and one legless frame not reproduced. The shade finding was withdrawn at
    22:13: the cameras that showed black animals beside lit grass sat inside the hill, the grass the
    sky through its missing faces, and measured on the flat world under a canopy of leaves at sky
    light level 11 a white sheep's wool reads 90 against 44 for the grass beside it and 155 against
    169 in the open, so a mob in shade is lit as the grass or brighter through the engine's indirect
    light, and the terrain's light-level factor, 0.995 at level 11, would only darken the mobs in
    deep shade and caves, so the mobs' lighting stays. One mend goes ahead without the owner's
    answer: each leg is lifted by however far its lowest corner would sink, so no foot goes below
    the ground in any stride, with item 13's models; the legless frame is hunted by the herd drive
    captured frame by frame, and if it comes again the frame goes to the engine's coder with the
    instance count and the view, since the engine draws instances in blocks of 64 and encloses each
    block for culling. The owner's answer names which of the three the owner saw. The stride's mend
    and the test are in at `a31d456`; what stays open is the owner's answer and the legless frame,
    hunted. The legless frame found and mended at `35696e2` on 2026-10-20, a stride worked out from
    nothing over nothing in the frame before the first drawing, not a number from then on, so the
    legs of the mobs placed before the first frame were drawn in no pass; the stride is left as it
    was in a frame that took no time, held by a test. Done; what the owner saw was this, the mobs
    near the spawn, and the stride's dip beside it.

11. **The README's picture shows the world's life, structures, a village and animals, and not
    terrain alone (Decision 6).** The owner wrote at 21:23 on 2026-10-19 that the README's picture
    shows terrain and a hill alone where the world is lively when entered, with structures, villages
    and animals. The picture at the README's head is taken where a village stands with its buildings
    in view and animals near, at the hour and heading that show them, with the render distance of
    item 12, taken by the capture script from a seed and a place written in it so it is taken the
    same again, the sky's picture of item 6 beside it. One commit, with item 6's or after it. Done
    at `569b5fb` on 2026-10-19, the village and the hills side by side from a script with the seed,
    the place, the hour and the cloud drift written in.

12. **The render distance is read and set by what the frame affords (Decision 6).** The owner wrote
    at 21:23 on 2026-10-19 that the render distance is very short. Read first: what the game's
    distance is in chunks and blocks, where it is set, whether the captures and the owner's play run
    at the same distance, and what bounds it, the frame time on the development machine's GPU and on
    lavapipe at each distance, the scene field's and the cascades' cost among it, and the memory;
    then the distance set to what the frame affords at the game's pace on the development machine,
    with the figures per distance in the commit's message and the setting's remark (N 3.5), and said
    in the README's running section. Read at 22:42: the distance is 8 columns of 16, 128 blocks, set
    at the game with a slider and a command clamping it to 2 to 16 and not kept by a save, so the
    owner's play and every capture run at 8; the shadows reach 96 blocks, the field's four cascades
    cover 128 across, the haze runs from two thirds of the distance, and mobs draw within 64. On the
    development machine's GPU at 1280 by 720 at noon over seed 1's spawn, the frame holds 60 a
    second to 8 columns, 11 to 12 ms of GPU with the shadows 3 to 4, the main pass 3.7 and the
    illumination 2.1, and misses it from 9, 17.2 ms, 21.7 at 10, 29.7 at 12 and 46.7 at 16, where
    the shadows take 23.5 and the main pass 12.9 while the illumination stays near 2 and the field
    under 0.01; on lavapipe the GPU's work is 584 ms at 4 columns and 923 at 8. The cause is the
    engine's: the terrain draws through the game's own shader, whose vertex stage keeps the mesh's
    positions, and the engine's model pass gives a batch with a program's own shader no box to cull
    by (`ModelRenderer.Instances.cs:108`), so every pass draws every section within the distance,
    the nearest shadow cascade, which holds the first 7.7 of the 96 blocks, drawing as many sections
    as the camera, 563 against 627 at 8 columns and 2,021 against 2,251 at 16. So the distance stays
    at 8 and the game asks the engine for culling of a draw through a program's own shader by its
    mesh's box, which a program whose vertex stage keeps the positions asks for, the engine's item
    36, with the tables; the game raises the distance once a package brings it. The commit puts the
    figures in the setting's remark and the README's running section. One commit, after item 11's.
    Done at `9158df8` on 2026-10-19, the distance at 8 with the figures in the remark and the
    README's costs section.

13. **The mobs' models take the other game's sizes and boxes (Decision 8), the textures the game's
    own.** The owner wrote at 22:03 on 2026-10-19 that the game may use the other game's original
    sizes and models of the mobs, since they are numbers. For each mob kind the game has, the
    model's cuboids, their sizes, places and pivots, and the mob's height and width for its body are
    taken from the other game's model definitions as numbers, said in the remark as taken from it,
    the game's own pixel art mapped onto the cuboids' faces at the same pixel density; the walk, the
    head's look and a wing's flap keep their pivots; held by a test of each kind's sizes against the
    numbers. Read with item 10, since feet that stand below the ground may be a leg's length or a
    model's origin, and mended in one commit a kind or one for all where the change is the same
    shape, the captures of item 10 retaken on it. The game's body sizes are the other game's
    already, and most cuboids within a pixel of its, the pig's hind legs and head apart, so the
    commits are the differences kind by kind, with item 10's test folded in, no face below the
    ground standing and the dip within its bound mid-stride. After item 10's reading and before item
    11. Done at `a31d456` on 2026-10-19, one commit for every kind, item 10's test in it.

14. **The game's tonemapping is the engine's somewhat boring curve (Decision 9).** The owner wrote
    at 22:06 on 2026-10-19 that the game uses a somewhat boring tonemapping, which is the engine's
    `Tonemap.SomewhatBoring`, where the game draws with the engine's default curve. The game sets it
    at its light settings with the reason in the remark, every capture of the play script is taken
    again under it, and the README's pictures of items 6 and 11 are taken under it, so item 11's
    picture comes after this. Read at 22:17 on the development machine's GPU, the curve takes a
    luminance of 1.0 to 0.61 where the engine's own curve is unchanged to 0.9, so at exposure 1 the
    frame reads about 8 percent darker, the pillar under the morning sun 133.4 where 146.0 and the
    noon sky 126.5 where 136.4 in mean luminance, the sun's core 207 where 251, and at exposure 1.25
    the pillar 145.2 and the noon sky about 137. The owner said at 22:36 to take 1.25 if it gives
    the light values back, so the exposure is set to 1.25 beside the curve with the reason in the
    remark, the check being those two means within two levels of the engine curve's at exposure 1
    and the sun's core brighter than at 1, the night room's within one; the captures and the
    README's pictures are taken under both. Measured at 22:38 on the play script's captures,
    exposure 1.25 holds the day, the pillar 136.6 where 136.7 and the noon sky 137.8 where 136.4,
    the sun's core 219 where 207 under the curve at 1 and 251 under the engine's, and misses the
    night, the lamp-lit room 31.8 where 28.2, 13 percent brighter, since the curve is near a
    straight line in the dark and rolls the day's light off, so no one exposure meets both: 1.25
    gives the day back with nights a fifth brighter, 1 keeps the nights with the day 8 percent
    darker, and about 1.12 leaves both off by a few levels. The owner chose 1.25 at 22:53. One
    commit, before item 11. Done at `bde1fc1` on 2026-10-19 at exposure 1.25 by the owner's choice.

15. **The section's mesh keeps the stone's faces against a lamp block, so the scene field reads the
    stone as solid.** Found by the engine's coder on 2026-10-20 reading the leak of light through a
    wall (the engine's item 30, part (h)): the mesher leaves the stone's faces against a lamp block
    out of the section's mesh (`SectionMesher.cs:214, 242`) and the lamp is drawn apart as a cube,
    so the engine's scene field, which takes a cell whose nearest point lies on an unshared edge as
    outside, reads the stone cells beside the lamp, in the wall's slab and the floor under it, as
    the open in front of the cube's faces, as far outside as they lie inside, and a world probe at
    that corner was moved into the stone. The engine walls such a probe in since `a7c2da8b`, and the
    field's reading of the stone stays wrong until the mesh says it: the faces against a lamp block
    stay in the section's mesh, or the lamp is meshed with the section as a block whose faces close
    the stone, whichever keeps the lamp drawn as it is; read with the engine's field instruments at
    that corner before and after, the cells beside the lamp reading inside, the light at the wall's
    foot against the reference the engine's coder gave, 1.42 of it with the probe walled in; held by
    a mesher test. Done at `cc334ab` on 2026-10-20, the corner probe moved into the air and the
    grass at the wall's foot from 14.1 times the reference to 1.67 on `5.2.92` alone, held by a
    mesher test.

16. **The terrain's shaders are marked as keeping their positions, and the render distance rises to
    what the frame then affords (items 12 and 36 of the engine).** The engine's `089761fb` lets a
    program say of a shader that its vertex stage keeps the positions
    (`E3D.SetShaderKeepsPositions`), so every pass culls its batches by their meshes' boxes;
    measured by the engine's coder in this game with the two terrain shaders marked, the frame at 16
    columns fell from 42 ms to 26 to 30 on the development machine's GPU and the shadows from 18.8
    to 2.8, the pictures unchanged. When a package carries it, the game marks both copies of
    `terrain.slang`, the sections' and the clouds', after loading them, with the reason in the
    remark, reads the frame at 8 to 16 columns again as item 12 did, and sets the default distance
    to what holds 60 a second at the game's pace, the figures in the setting's remark and the
    README's costs section as before, the owner settling the figure where the reading leaves a
    choice. One commit, with the package's bump. Read on `5.2.155` by 09:40 on 2026-10-20, the
    package the engine published at 08:38: standing at seed 1's spawn at noon offscreen at 1280 by
    720 on the development machine's GPU with the two terrain shaders marked, the frame and the
    GPU's work read 16.7 and 6.3 ms at 4 columns, 16.7 and 6.5 at 8, 16.9 and 7.7 at 10, 16.9 and
    9.1 at 12 with 4.2 ms of each frame waiting, 19.0 and 10.7 at 14 and 22.1 and 12.2 at 16, the
    shadows 1.2 to 1.3 ms from 8 to 16 where they took 4.3 and 23.5 unmarked and the camera's pass
    from 12.0 to 5.5 at 16, against 17.2, 26.0 and 42.5 ms at 8, 12 and 16 without the marks, so the
    gain is the marks'; from 13 columns the main thread bounds the frame; memory 865 MB at 8, 1,256
    at 12 and 2,084 at 16; flying east over new land for ten seconds, 8 columns 18.3 to 18.9 ms, 10
    18.4 to 20.9, 11 19.4 to 23.8 and 12 21.4 to 28.8, no distance holding 60 there since the update
    meshes and joins the new columns on the main thread, 5 ms a frame at 8 and 10 to 14 at 12;
    lavapipe's GPU work 0.57 s at 4, 0.88 at 8, 1.12 at 10 and 1.27 at 12. So standing holds 60 to
    12 columns, and in a flight over new land 10 costs about what 8 does and 12 a third more; the
    default's figure, 12 or 10, is the owner's, put to the owner with the reviewer's word for 12 and
    the flight's meshing as a reading of its own after. The leak on `5.2.155` with both halves reads
    as `cc334ab` did alone on `5.2.92`, the corner probe 13 under its reference, the grass at the
    wall's foot 1.73 of the reference with the screen probes off and 1.34 on where the same row away
    from the lamp reads 1.55 and 1.56, the room facing the lamp 1 under, and the walled-in rule
    changing nothing since no probe lies in the stone after the mesh's mend. In the tree: the bump,
    the workflows' engine checkout at `9f9180b7`, both terrain shaders marked with the reason and
    Decision 10's sentence, the strict build clean and 126 tests passing; one commit carrying
    REVIEW.md once the figure is in. By 09:51 the standing figures above were found read with an oak
    leaf block one step before the eye filling the frame, item 12's teleport standing the player at
    a leaf, so the camera's pass shaded almost nothing and its time and the frame's read low, and
    item 12's table was very likely read the same way; the coder reads every distance again from an
    open view, flying at 96 looking east over the forest to the sea, and the owner's figure waits on
    that table. The flight and leak readings stand, from other places, and lavapipe's were from the
    blocked view too and are read again after the GPU's table; the build workflow's play run on
    lavapipe held to four cores passes at both distances, 448 seconds at 8 columns and 452 at 12,
    since lavapipe does not finish meshing the wider land within the run. The open view read by
    10:05: flying at 96 looking east 8 down over the forest to the sea, seed 1 at noon, 1280 by 720
    offscreen on the development machine's GPU with the marks, each distance three times in a fresh
    session over 15 seconds, the frame holds 16.7 ms to 12 columns with the GPU at 6.6, 8.1 and 9.5
    ms at 8, 10 and 12 and 11.3, 9.9 and 6.9 ms of each frame spare, 16.7 to 16.9 at 14 with the GPU
    at 11.3 and 3.2 spare, and 16.9 to 18.8 at 16 with the GPU at 12.8 and 0.4 spare, the shadows
    1.1 to 1.2 ms at every distance and the global illumination 1.3 to 1.5, the 3.1 to 3.4 ms read
    at 4 columns having been the leaf, 1.8 from the open view as item 12 had it; memory 812 MB at 8
    columns, 1,277 at 12, 1,971 at 14 and 2,169 at 16; the main thread bounds the frame as the
    distance grows while the GPU's work stays under 13 ms, the fifteen sessions agreeing within 0.2
    ms below 16 columns where the blocked view's had moved between sessions; the gain is the marks',
    the GPU without them 11.3, 26.0 and 42.5 ms at 8, 12 and 16 against 6.5, 9.1 and 12.2 in the
    same view. Flying over new land the frame reads 18.3 to 18.9 ms at 8, 18.4 to 20.9 at 10, 21.4
    to 28.8 at 12 and 23.8 to 24.7 at 14, the update meshing and joining new columns on the main
    thread, 12 ms a frame at 14. So 14 is the most that holds 60 standing, with 3.2 ms spare, and
    flies as 12 does; 12 keeps 6.9 ms spare and 700 MB less. The figure, 12 or 14, is the owner's,
    put to the owner with the reviewer's word for 12, the margin and the memory on machines slower
    than the development machine's; the tree holds 12 meanwhile. Lavapipe's GPU work from the open
    view with the marks is 0.49 s a frame at 4 columns, 0.82 at 8, 1.20 at 12 and 1.42 at 14, most
    of it the camera's pass. The owner chose 12 at 10:36 on 2026-10-20, so the default is 12
    columns, its remark giving the table and the margin, and the commit goes in.

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

7. **Nothing in the sky faces the camera; the sun, the moon and the stars are fixed on the sky and
   turn with the hour.** The owner wrote at 21:05 on 2026-10-19 that the sky's bodies are not to be
   billboards in the voxel game, since a thing facing the camera turns with the view and looks wrong
   in motion, and that they behave as the other game's, placed on the sky and depending on position
   alone. So the sun, the moon and the stars are quads fixed on the sky's sphere, turned with the
   hour as one body, their orientation the sphere's at their place and never the camera's, and the
   words of Decision 6 on camera-facing billboards are read so; the halos around the sun and the
   moon are quads fixed with them on their grids, since a particle stands upright in the world and
   faces the camera, which would turn the halo's rings against the sun's pixels and with the view,
   so the particle effect Decision 6 asked for is read as the glow fixed with its body, and the
   engine's emitters stay for the game's other effects. Item 6 changes to it, and the engine ask the
   game was about to write for particles square on the screen is not written.

8. **The mobs' sizes and models may be the other game's, since they are numbers.** The owner wrote
   at 22:03 on 2026-10-19 that the voxel game can use the original sizes and models of the mobs from
   the other game, since they are numbers. So a mob's cuboids, their sizes, places and pivots, and
   its height and width are taken from that game's model definitions and named as taken from it in
   the remark, as Decision 5 allows a number taken from another product; its textures and sounds are
   not numbers and stay the game's own, drawn in its pixel art. At 22:06 the owner added that the
   game keeps a character of its own: its pixel art, its palette, its world, its names, its sounds
   and what it does its own way stay the game's, and the numbers taken are the mobs' sizes and
   boxes, so a reader knows the game by itself and not as a copy. Item 13 does it, read with item
   10.

9. **The tonemapping is the engine's somewhat boring curve.** The owner wrote at 22:06 on 2026-10-19
   that the game uses a somewhat boring tonemapping, the display transform of that name among the
   engine's curves, which keeps colors as they are and rolls the highlights off without a filmic
   look, so the game's pixel art reads as drawn. The game sets `Tonemap.SomewhatBoring`; item 14. At
   22:36 the owner said to take exposure 1.25 beside the curve if it gives the light values back,
   since the curve alone reads the frame about 8 percent darker; item 14 measures that it does. The
   owner chose 1.25 at 22:53, the day exact and the nights about a fifth brighter.

10. **The review's files are committed by the batch that comes next, and by the reviewer at most
    once a day.** The owner wrote on 2026-10-20 that the review's commits had become nine of every
    ten in the history and were to stop, and then that the reviewer is to edit the review's files in
    the tree and commit nothing, as it did before 2026-10-19, with one commit a day at most where
    that makes sense. So REVIEW.md, SHARED.md and NORM.md stay edited in the tree and the coder's
    next batch carries them, as AGENTS.md has it; the reviewer commits them in a commit of their own
    only at a day's end where no batch carried them, never for a reading, a run or a package. The
    coder adds one sentence saying so to COMMITS.md in the next batch, in the paragraph on what a
    commit carries, naming the review's files, the batch that comes next and the one commit a day at
    most, and no one who decided it (N 4.7).

## Replies



