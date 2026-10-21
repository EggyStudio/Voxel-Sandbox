# Review

Direction for the session working on this repository, written by a second session that reads the
code and the history and writes none of the game. The Now list is read before a batch is started
and before each commit. This file has one writer. The session doing the work edits the Replies
section only. An item is removed from here once the commit that settles it has been read.

Reviewed up to `8f2a2c8`, item 39's first part, the lanterns' and torches' pieces, and nothing else,
carrying the review's file with items 60 and 61: the lantern's, the hanging lantern's and the soul
lantern's pieces and the torches' sticks and flames take the source models' face coordinates,
written as numbers in BlockShape, the handle and chain the source's two crossed walls turned 45
degrees and cut out, a face with no overlay carrying its corner's texture place in the overlay's
nine bits as kind 3, each emissive surface's cube a mesh of its own with the parts, an emission map
making the glass's and flame's bright pixels emit four times their color with the iron dark while
the field keeps the uniform emission so the light cast is unchanged, the own art's lantern iron a
mask of links, the terrain shader's LightSteps comment reworded under N 4.7; the strict build clean,
215 tests, the play script clean twice under the validation layer, the second after a renaming pass
for STYLE.md's word table. Read at 09:16 on 2026-10-21. The build run 39557862107 of `b56fa0b` ended
green, the three test jobs within 70 seconds and the play job in 36 minutes 3 seconds with the walk
at a render distance of 2, down from 48 minutes 27 seconds at 4 the run before, so the head was
pushed at 09:28. Shortly after 09:40 the owner asked for the mobs' faces from Bare Bones, item 62
and Decision 30, written at 09:27. Next items 60 and 61, then 62, then item 39's cutout leaves,
doors and glass, then its animated textures, then the pictures taken again at 40 degrees, then 26
and the items in order; items 55, 38's second part and 54 when their packages come; the AGENTS.md
rows for the Ui and build folders wait on the owner's word.

Before it, `b56fa0b` came, item 58's parts d and e, the haze and the shadows' grid, after `293a5d1`,
its part b, the night tint, and nothing else, the first carrying the review's file. Read at 08:49,
pushed at 08:49 with the four before them after the build run of `815e4ca` ended green, its play job
48 minutes 27 seconds at a render distance of 4. Its build run ended green with the play job at 36
minutes 3 seconds.

Before it, `396047d` came, item 58's part c, the grade, after `82da862`, its part a, and `fa7fba0`,
item 59, and nothing else, the first of them carrying the review's file. Read at 08:16, pushed at
08:49 with `be968a2`, `293a5d1` and `b56fa0b`.

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
    columns, its remark giving the table and the margin, and the commit goes in. Done at `435b2de`
    on 2026-10-20, as the head says.

17. **Out of the water onto the shore, swimming while any of the body is in it.** The owner's first
    word of 2026-10-20 at 23:30: inside water one cannot jump out. The controller counts the player
    as swimming only while the body's middle is in water and Space then lifts, so once the middle
    clears the surface the body falls under full gravity, bobbing with its feet under, and a shore
    one block up needs more lift than that gives. The mend is the source's rule: swimming while any
    part of the body is in water, and a body in water pushing against a block's side with the space
    above free lifted at a climbing speed until it stands, so the shore is reached by walking into
    it; a test in the scripted run swims to a shore one block up and reads the player standing on
    it. One commit, first. Done at `5cd1521` on 2026-10-21, as the head says.

18. **The mobs' proportions toward the source's, the villager first, with some character kept.** The
    owner's second word: the villagers are not the right proportions. Item 13 took the other game's
    cuboids as numbers, and the villager still reads wrong, so each mob's cuboids are read again
    against the source's model boxes at the same scale, the villager's head, nose, robe, arms folded
    and legs among them, each box's size and offset compared and put right where off, the game's own
    character kept in the faces, colors and the few touches that were its own, with a capture of
    each mob beside its source's silhouette in the record. Two commits. The first at `7fe7639` on
    2026-10-21, the scale, the cow and the nine pairs, which read the boxes right and the walk
    wrong, as the head says; the second takes the source's walk and rest poses, the swing's angles
    and the stride's 2.36 blocks a cycle for each kind, the villager's legs under its robe, the
    zombie's arms 10 degrees under level at rest, the skeleton's arms hanging until it aims, the
    spider's legs turning 7 and lifting 7, with the mid-stride pairs made again. Done at `28eb41c`
    on 2026-10-21, as the head says.

37. **A mob's faces away from the sun read blue-grey, lit by the sky alone.** Found by item 18's
    pairs on 2026-10-21: the sheep's wool, white at 233, 236 and 236, reads 117, 160 and 173 on its
    body below the head in the front capture and 119, 161 and 178 on its side at noon, about half
    its red and three quarters of its blue, where its sunlit face reads 194, 181 and 162; the source
    shades a face by its direction alone, 0.5 to 0.8 of the color with no hue, and a mob on grass at
    noon in the game should read its white darkened and a little green from the ground, not blue.
    Read first whether the mobs are drawn with the engine's indirect light, the field the terrain is
    lit by, or by the sun and the sky's ambient alone, and say which; then the mend, the mobs taking
    the indirect light where they lack it, or where they have it the sky's share of a mob's ambient
    brought toward the ground's and the source's flat shade used as the floor under which no face
    falls, so the wool in shade reads near 150 on all three and the pairs' colors say so. Tests: the
    sheep's shaded body read from a capture within a distance of its white darkened, and a daylight
    capture of the nine mobs in the record. One commit, after item 18 and before item 19. Done at
    `e3d9a74` on 2026-10-21, as the head says, the mobs found to take the indirect light already and
    the mend a shader of the game's own.

38. **The screen probes off by default, and the cascades' faults given to the engine with repros
    (Decision 16).** The owner's word of about 01:00 on 2026-10-21: the game is not to enable the
    screen probes, whose look is pixelated, the cascades without them being the better option for
    the game's art, and the cascades still show faults. Two parts. First, now: an entry in the
    engine's ASKS.md as Decision 26 has it, with a repro of each fault the owner names, the world's
    seed, the place, the hour, the commands that put the camera there and a capture, the light
    block's light leaking through the thin gap at the corner of two columns that hold it off, a
    penumbra on the ground with zig-zags, the screen probes shifting on movement, z-fighting between
    the global illumination and the opaque textures where it shows, and the noise, read in the game
    at `28eb41c` on 5.2.190 with `voxel.gi high`, and beside each what `gi.screenprobe off` changes;
    the engine's item 71 takes them. Second, when the package brings the engine's call that leaves
    the screen probes out: the game calls it at start, off by default, a setting and `voxel.gi`
    taking a word for them, the README's paragraph on the light saying why, and the measurement rows
    and the captures made again without them. The entry also reads the model pass, `gpu.hdr_scene`
    in the profile, with `gi.toggle screen off` against on, in a scene of many light blocks in view,
    a village at night or a room of forty glowstones, since without the screen's probes the engine
    takes the glow lights at each pixel and the cost grows with the lights in view, 0.3 to 1.0 ms on
    four emitters in the engine's reading, which the engine's item 71 bounds. The first part before
    item 37, the second when its package is listed. First part done at `404ab74` on 2026-10-21 with
    the entry in the engine's ASKS.md, as the head says; the second part waits on the package.

39. **Cutout textures, a texture a face for the lamp's kind, and animated textures.** Found by item
    24's first commit on 2026-10-21: a pack's clear pixels in leaves, a door's window and a cactus's
    edges show the pixel's stored color, since the solid faces cut nothing out; a lamp cube takes
    one texture on all six faces, which puts the lit furnace's front on its top; and an animated
    texture shows its first frame alone. The mend in three parts: a block kind marked cutout has its
    faces drawn with the shader discarding clear pixels and its faces kept where a neighbor of the
    same kind would cull them, as the source draws leaves, so a pack's leaves and doors read as they
    do in the source; the furnace, the lamp and every kind with a texture a face take their faces
    from the table by side, top and bottom, and the lit furnace its front; and an animated texture's
    frames are laid in the atlas as the strip holds them and stepped by its metadata's frame time,
    the mesher writing the first and the shader stepping by the frame, so water, lava and fire move.
    Tests: a cutout face kept beside its kind and its clear pixels discarded in a capture's pixels,
    the furnace's front on its front, and an animated tile's frame at a known time. After item 24.
    Two commits, the cutout and the faces, then the animation. The owner added shortly before 06:46
    on 2026-10-21 that the lantern could take Bare Bones' texture to look better, a lantern's glass
    being a lamp's cube drawn white today under a cap and a chain in their surfaces' colors; so the
    first part also gives the lantern's, the hanging lantern's and the soul lantern's pieces a
    pack's lantern texture by the coordinates the source's lantern model gives each piece's faces,
    read as numbers as the mobs' boxes were, and the torches' flames and sticks the torch texture
    the same way, each piece keeping its color where no pack has one. After item 46. First commit
    done at `8f2a2c8` on 2026-10-21, the lanterns' and torches' pieces, as the head says; the cutout
    and the faces, then the animation follow after items 60 and 61.

40. **F2 takes a screenshot, as the source does (Decision 19).** The owner wrote shortly before
    03:42 on 2026-10-21 that, as in Minecraft, a screenshot is to be taken in the game with F2, the
    same for BevyCSharp, the captures put in the root or a subfolder. F2 writes the frame as a PNG
    into a `screenshots` folder beside the saves and the resourcepacks folder, made on first use,
    the file named by the time as the source names its own, `2026-10-21_03.45.12.png` and a count
    where two fall in one second, through the engine's capture, without the HUD's own message in the
    picture; a line on the HUD says the file was saved, as the source's chat does, and the README's
    controls gain the row; a transient run writes nowhere. When the engine's package brings its own
    F2 of item 73, the game takes that key and keeps its folder and its message. Tests: the name
    from a time and the count for a second, and the press through the command line leaving a file of
    the window's size in a scratch data folder. One commit, small, before item 25. The engine's
    performance window moves from F2 to F3 with its item 73, which is the game's own F3; when that
    package comes, the game turns the engine's window key off or gives it a free key through the
    engine's call, its own F3 window keeping the developer's parts. Done at `5064479` on 2026-10-21,
    as the head says, the engine's performance window hidden off screen until item 73 gives it a
    key.

41. **A pack's blockstate variants give a surface several tiles picked per block by its position,
    and Simple Grass Flowers is a fourth default pack (Decision 20).** The owner wrote shortly
    before 03:53 on 2026-10-21 that Simple Grass Flowers looks great, linking its gallery, and that
    beside flowers and grass, which the game lacks yet, it adds variations of the ground. The
    reading: the pack by 2DWisp puts flowers, rocks and clovers on the tops of grass, podzol and
    mycelium through the source's block states, a block's state listing several models with weights,
    each naming its own top texture, and the source picking one per block by a hash of its position;
    its page says all rights reserved and allows adding it to modpacks with credit. The game reads
    textures by a table of its own and no block states, so it gains, for the surfaces the table
    names, a reader of a pack's block state variants and their models' textures, the surface then
    holding several tiles with weights in the atlas's free bits, the mesher picking one per block by
    a hash of its position as the source does, the same block the same tile every time, and the
    table's single tile where a pack lists no variants. Simple Grass Flowers joins the manifest as a
    fourth default pack, fetched from its maker's own Modrinth page as the three are, pinned by its
    version and hash, after Bare Bones in the list so Bare Bones' blocks stay and the flowers go on
    top, and credited in the README with its terms; nothing of it ships. Item 25's own flowers and
    its short and tall grass stay as blocks. Tests: a block state file of two weighted variants
    parsed to its tiles, the pick by position stable and within the weights over a field, and a
    capture of a meadow with the pack where the machine has it, skipped where not. After item 25 and
    before item 39. One commit. Changed shortly before 05:00 on 2026-10-21 by the owner's word on
    seeing the pack over Bare Bones, that it looks good but changes the way Bare Bones' grass top
    looks and the style does not fit, so it is to be done natively in the game's own style: Simple
    Grass Flowers leaves the manifest and the defaults are the three again; the game's own ground
    looks give the grass top its variants wherever no pack's block state lists any, flat little
    blossoms in a few colors, patches of clover darkened from the grass under them and tinted by the
    climate, and pebbles, each turned four ways and weighed near the pack's, about four in five
    plain, a twelfth clover and a few in a hundred flowers or stones, drawn as the game's own pixels
    in its flat style and lying over Bare Bones' grass top and the game's own alike; the block state
    reader stays, so a pack that lists looks still takes their place; the tests the own looks'
    weights over a field, the clover tinted and the decor untinted, and the plain look leaving the
    grass as it was. One commit as before. Under Decision 22 the block state reader goes too, the
    grass top taking the game's own looks alone over a pack's grass tile, the README saying a pack
    gives the pictures alone. Done at `236763d` on 2026-10-21, as the head says, the grass's tile
    never turning on the owner's word.

42. **Water that mirrors the sky and the shore, with the bed seen through it (Decision 21).** The
    look the owner aims at shows calm water mirroring the sky and the shore and clear pools in
    caves. The game's water gains a surface of its own: the sky and the land across the water
    reflected in it, through the engine's reflections where they reach a flat surface and a planar
    reflection of the game's own where they do not, read first and said; the bed seen through the
    water, refracted a little and darkened with depth by the water's color, so a pool reads clear
    and a lake reads deep; a slow wave in the normal so the reflection moves; the water's edge drawn
    where it meets a block without a hard line; and the view from under the water tinted and fogged
    as the source's is. Captures of a lake at noon and at a low sun and of a cave pool in the
    record, beside the modpack's own pictures. Tests: the reflection's picture read in a capture's
    pixels where the sky should mirror, and the bed's color darkening with depth in a laid pool.
    After item 27. Two commits, the reflection, then the depth and the edge.

43. **The sun's look, warm and low with long soft shadows, and a blue haze over distant land
    (Decision 21).** The look the owner aims at has a warm low sun with long soft shadows and a blue
    haze over the far land. The game gives the sun a color and a strength by the hour as the
    source's shader packs do, white at noon, warm at a low sun and red at the edge of night, the
    sky's ambient turning with it; the shadows' softness read against the engine's, a penumbra
    widening with the distance from the caster where the engine has it and asked of it in its
    ASKS.md where not, with the hour's long shadows kept sharp enough to read; and a haze over
    distant land as fog by distance and by height tinted by the sky, thin at noon and thick at dawn,
    through the engine's fog where it takes a tint and a height, the far land of item 26 fading into
    it. A setting for each, on by default. Captures at dawn, noon and dusk of one view in the record
    beside the modpack's. Tests: the sun's color at four hours and the haze's density by distance
    and height. After item 26. One or two commits. Item 58 takes the sun's color by hour and the
    haze's strength from the modpack's settings; this item keeps the shadows' softness, the haze by
    height and the captures at three hours.

44. **The land's fading edge meets a sky of the haze's color below the horizon.** The owner asked
    shortly before 04:02 on 2026-10-21, seeing the cherry grove's capture from the scripted run's
    walk, what could be done about the chunks' fading border contrasting with the dark lower half of
    the skybox, and whether distance fog would do it. The reading: the terrain shader's haze fades
    the land toward the sky's horizon color from 0.65 to 0.95 of the render distance, and past the
    last section the skybox's lower half shows, which the sky's equirectangular image paints dark
    since it lights the scene through the environment map too, so the fogged edge meets a darker sky
    than the color it fades into, most at small render distances; a longer fog alone would not mend
    it. The mend, as the source does it: the game draws a dome around the eye below the horizon in
    the haze's color after the skybox, blending into the sky over a few degrees above the horizon,
    so the edge fades into the color it meets, the image's darker lower half kept for the lighting
    alone; the dome's color following the haze's by the hour when item 43 gives the haze its tint.
    Tests: a capture's pixels read across the land's edge at the horizon with no step over a pixel,
    and the dome's color equal to the haze's. One commit, small, after item 25. Done at `cc46f25` on
    2026-10-21, as the head says.

45. **The look without a pack takes Bare Bones' colors (Decision 23).** The owner wrote shortly
    before 05:10 on 2026-10-21 that the look without a resource pack can use Bare Bones' colors to
    match it at least, the game having different colors for them. The game's own flat colors, its
    surfaces' colors used where no pack is loaded, become Bare Bones' averages read as numbers from
    the owner's copy as Terralith's colors were, nothing of its pixels shipping: `voxel.palette`
    prints each surface the table names with its texture's alpha-weighted sRGB average from the
    packs loaded, so the numbers can be read again; the surfaces take those numbers for the full
    blocks, the grass and the leaves as their gray before the climate's tint, the water as its gray
    under the table's tint, glass and ice keeping their alpha and the emissive blocks their glow;
    the plants keep their own masks and colors, since a flower's average mixes stem and petals.
    Tests: the field's colors unchanged in kind, and a capture with no pack beside one with Bare
    Bones showing the match. One commit, small, before item 39. Done at `8261308` on 2026-10-21, as
    the head says, the ores keeping their own colors.

46. **The README's pictures taken again, an animation at its head and pictures in its parts
    (Decision 24).** The owner wrote shortly before 05:15 on 2026-10-21 to update the README's
    images, to put at the top a GIF of different captures with 250 ms between them, and to showcase
    things as images in the parts below, the interface, crafting and such. At the head, one animated
    image of about ten captures 250 ms apart, the village, the hills, the mesas, the cherry grove at
    dusk with its lanterns, the jungle, a swamp, a cave lit by glowstone, the night sky, a river
    valley and the snowy peaks, each from a fixed seed, place, hour and heading in
    `build/pictures.sh` as the head pictures are, encoded together at 800 by 450 with the tools on
    the machine, a GIF as the owner asked, measured beside an animated WebP of the same frames,
    which the site plays too, and the GIF taken unless it is too large, said with the sizes; the
    engine's recorder of its item 70 takes the frames once its package comes. In the parts below,
    pictures at 800 by 450 by the same script, the biomes as a sheet of four, the plants and the
    grass's looks, the trees, water, glass and ice, torches and lanterns lighting a room at night,
    the interface with the HUD, the inventory, a crafting table, a furnace and the menu, the
    resource packs' screen, and caves and ravines; the script gaining the interface's shots through
    input keys and commands and a list of what it takes. The owner's word shortly before 05:25 on
    2026-10-21: the captures use the resource packs as well. So the pictures are taken with the
    three default packs on in the manifest's order, read as the terms allow, a picture of the game
    drawn with a pack holding none of its files where Bare Bones forbids reuploading any instance of
    the pack, Recolourful's terms allowing its use in any form of media with a link to its page, and
    the font's page offering it for content creation; the repository still holds no file of any
    pack, and the README credits each pack with a link beside the pictures. The pictures script
    fetches the packs by the manifest's URLs into a captures folder, checks each hash and opens the
    game with a pack flag for each; the resource packs section keeps one pair, the same view in the
    game's own art and with the packs, so the look with no pack stays shown; the Resource Packs
    screen is left out since a transient run lists no packs and prints the machine's folder; a
    `voxel.scale` command sets the interface's scale for the scripted shots, the windows taken at 2
    and cut out at 800 by 450 with no resampling. One commit, after item 45 and before item 39. Done
    at `7039d5a` on 2026-10-21, as the head says, the GIF at 1.45 MB. The animation's pictures at
    two seconds each at `fc38219` on the owner's word. The pictures are taken again after item 39,
    one run covering the plants, the lantern and the cutout. The owner said shortly before 08:16
    that for screenshots they zoom with alt to about 40 degrees of field of view and the cinematic
    captures should too, so the retake takes the world's views at 40 degrees through a `voxel.fov`
    command, each view's place moved back to frame the same subject.

47. **The haze dome no longer cuts the sun, the moon and their glows (Decision 25).** The owner
    asked shortly before 05:40 on 2026-10-21 whether the far plane or the skybox cut the sun's and
    the moon's flare off at the bottom. Neither: the far plane is at 4,000 and the bodies at 3,000
    and 3,200, and item 44's dome at 2,400 is drawn in the model pass writing depth over its whole
    span to its top ring six degrees up, so the bodies drawn after it fail behind it even where its
    alpha is zero, measured with a low sun over the sea cut with the dome and whole with the fog
    off; the engine's material has no depth write switch in 5.2.190. The mend: the dome moved beyond
    the bodies, between the stars' 3,200 and the far plane, at 3,600, and each body's quad a grid
    whose vertices take the dome's clear share at their height, so the haze still covers a sinking
    sun as blending the dome over it would and nothing is cut; the test of item 44 kept and one of a
    low sun's glow whole across the horizon's six degrees. One commit, small, before item 46's final
    captures. Done at `f7df279` on 2026-10-21, as the head says.

48. **No tunnel mouth and no ravine under a village (Decision 25).** The owner wrote shortly before
    05:40 on 2026-10-21 of seeing a lot of rifts in villages, to be made rarer since they make the
    structure buggy and villagers could fall. The reading: the rifts are tunnel mouths, where a
    patch of high ground opens to the tunnels, and ravines, both carved before the village builds
    its roads and footings at the ground's height, so a footing or a road hangs over the pit, seen
    at three plains villages and a desert one. The mend: no mouth and no ravine within a village's
    square of 48 and a margin of a few blocks, the generator and the ravines asking for the
    villages' places, which depend on the seed and the ground alone; tunnels deeper than three
    blocks under the ground stay, so a village's ground is whole. A test generating the columns of
    seed 1's first villages and finding no open cell under a road or a footing. One commit, before
    item 46's final captures. Done at `0c93d3e` on 2026-10-21, as the head says.

49. **The sun and the moon in the source's default look, square with a dithered glow, and a pack's
    own where it has one (Decision 25).** The owner wrote shortly before 05:40 on 2026-10-21 that
    they do not quite like the sun and the flares, the source's default dithered pixelated square
    one looking better. So the sun and the moon drop the game's six rings and take the source's
    default look, a square body with a glow dithered in its own pixels around it, drawn by the game
    in that style as its own pixels, since the source's own picture cannot ship; where a pack has an
    environment sun and moon phases picture, the game draws those, as pictures alone under Decision
    22, the moon's phase by the day; the dusk's color fan of the sky's gradient stays. A capture of
    the sun at noon and low and the moon at night in the record. One commit, small, with item 47
    before item 46's final captures. Done at `f7df279` on 2026-10-21, as the head says.

50. **The block's texture over the whole window while the eye is inside a block, as the source draws
    it (Decision 25).** The owner wrote shortly before 05:42 on 2026-10-21 that the camera lands
    inside blocks in some captures, and that when it does the game is to show the block's texture on
    the whole screen, and the script is to check before a capture whether the eye is inside a block
    and get out first or find a better spot; in the scouting, eye-level spots chosen from the ground
    landed inside dark oak crowns and logs and the frame read black or showed the faces' insides.
    While the eye's cell holds a block that blocks the view, a whole opaque cube, every mode but
    spectator draws that block's side texture over the whole window under the HUD, the pack's or the
    surface's own color, darkened to a tenth, repeated four times across and down and slid by the
    yaw and the pitch over 64 as the source's screen effect draws it; leaves left out as the source
    leaves them, and glass, water and the shaped blocks. A test of the overlay's block for an eye
    inside stone, none inside air, leaves or glass, and none in spectator. Item 46's script checks
    the eye before each capture, `voxel.state` saying the eye is inside a block when its cell holds
    anything but air, the script moving the eye up a block at a time to eight until its cell is air
    or failing with the view's name, and the views chosen where the eye is in the open. One commit,
    small, after item 48 and before item 46's final captures. Done at `a96cea5` on 2026-10-21, as
    the head says.

51. **The sun's and the moon's glow smooth as Bare Bones' is, in the game's own art too (Decision
    25).** The owner wrote shortly before 05:55 on 2026-10-21, seeing item 49, that they like Bare
    Bones' one more and that it should be implemented in the native look without resources too,
    being a smoothing of the dithering, the flares of the sun and the moon. The reading of the
    owner's copy as numbers, nothing copied: a 128-pixel picture, the same three-ring square body at
    four texels to each of the source's pixels, opaque and added to the sky, with a smooth glow from
    about a tenth of its color beside the body falling nearly linearly to nothing fifteen of the
    body's pixels from the middle. So the game paints the sun and the moon at four texels to a
    pixel, 128 across, the body's pixels as four by four blocks so it stays pixel art, the glow
    smooth with no dithering or levels from about a tenth beside the body falling linearly to
    nothing at fifteen pixels, the moon's as bright as its phase is lit; the tests swap the dither's
    check for a smooth fall, never rising outward and never stepping by more than a few levels
    between neighbors; the sky's grid stays one quad to each of the 32 pixels. One commit, small,
    after item 48. Done at `e4483af` on 2026-10-21, as the head says.

52. **The stars as plain squares, the game's layout and the source's compared for the owner's choice
    (Decision 25).** The owner wrote shortly before 06:02 on 2026-10-21 to do squares instead of the
    star shape and to try the source's star layout with its values, comparing both, the values as
    they stand and the source's. The cross goes and every star is a plain square. Two layouts are
    taken from the same night view at the same hour, moon and heading and shown to the owner side by
    side: the game's as now, 500 stars of 0.16 degrees at a brightness of 0.45 to 1 with their sides
    along the sky's turn; and the source's, read from its client as numbers with nothing copied,
    1,500 stars each placed by a point in the unit cube kept where its squared length lies between
    0.01 and 1 and pushed out to the sphere, a square of 0.17 to 0.29 degrees across turned by a
    random angle about its own axis, white at the sky's star brightness. The owner chooses, and the
    chosen layout is committed with a test of the count, the sizes and the even spread over the sky.
    One commit, small, after item 50 and before item 46's captures, since a night picture shows the
    stars. The owner, shown the pair shortly before 06:10 on 2026-10-21, said the night sky in their
    modpack looks different; the modpack's shader draws its stars per pixel, the sky's direction
    projected onto a plane from the moon's place, scaled and snapped to a grid so a star is a square
    of about 0.28 degrees at the zenith, a cell a star where three hash noises multiplied pass 0.7,
    about 0.58 percent of cells, its light the excess squared times a cool white, faded over the
    lowest 20 degrees and hidden near the sun and the moon; the shader's license forbids
    redistributing or changing its code, so it was read as numbers and the game's own written, a
    third layout of 3,000 stars spread evenly over the turning sky, squares of 0.28 degrees, each
    drawn until its product passes 0.7, the light taken to sRGB, with the fade and the hiding. Shown
    the three side by side, the owner chose the modpack's shader's, so the game keeps it alone, the
    cross, the star texture and the comparison switch gone, the test covering the count, the size,
    an even spread on each side of the three axes, the middle star faint and the brightest bright
    and cool white; the shader's bloom on its brightest stars and its stars sliding with the moon
    rather than turning with the sky left out. Done at `f7bcf26` on 2026-10-21, as the head says.

53. **The bloom at a tenth (Decision 25).** The owner wrote shortly before 06:26 on 2026-10-21 that
    there is too much bloom, and shown the lamps' room at night at 0.5, the game's default, 0.25,
    0.1 and 0, chose 0.1. The default goes from 0.5 to 0.1 with the reason in its comment, the
    settings' slider still reaching 2. One line, committed alone after its checks and before item
    46's captures, which then show it. Done at `6845654` on 2026-10-21, as the head says.

54. **The game takes the engine's F1 and F3, its HUD in the hidden scope and its developer's parts
    as overlay sections (Decision 26).** The owner asked shortly before 07:05 on 2026-10-21 for F1
    to hide the interface as Minecraft does, as an engine feature, F2 the screenshot, and F3 a debug
    overlay like the newest Minecraft's, sorted in parts logically with modules a game adds, world
    generation among them, showing the system, the camera and the renderer, memory, fps and
    frametime with a graph like the RivaTuner overlay's, both for BevyCSharp and 3DEngine, F1 hiding
    the interface and ImGui but not the F3 overlay so everything hides but the overlay over the
    renderer, and Shift+F3 showing the frame debugger too; and answered the reviewer's questions
    shortly before 07:06: the interface hidden by a scope, F3 the overlay's on its release alone
    with any chord left to the program, the look Minecraft's with no header, window styling or
    border, text on a transparent backing, and the order as the reviewer sees fit. When the package
    brings the engine's item 78, the game's HUD goes inside the engine's interface scope so F1 hides
    it, and its screens, the inventory, the tables, the menu and the options, stay outside the scope
    and never hide, as the source hides its HUD and not its screens, the F3 settings window of the
    game's own gives way, its developer's parts becoming sections of the engine's overlay, the
    world's generation, the biome and the climate at the eye, the light levels, the loaded and drawn
    columns and sections, the mobs and the streaming, with the game's own settings moved to the
    options screen of item 23, and the game's F3 chords stay, F3 with F4 the switcher and F3 with T
    the reload, which the engine's rule leaves to the program; the engine's performance window
    hidden off screen since item 40 is no longer needed. Tests: F1 leaving the frame without the HUD
    read from its pixels, and the game's sections in the overlay's text. One commit, when the
    package comes.

55. **The plants cast shadows, cut by their texture (Decision 27).** The owner wrote shortly before
    07:12 on 2026-10-21, seeing the README's pictures, that the grass blades and the plants are to
    cast shadows too, fitting as they are pixelated. The cutout mesh casts no shadow today, so the
    grass, the flowers and the wheat throw none. With it casting, the sun's shadow map must cut each
    blade by its texture's alpha, or a blade casts a whole quad's shadow; read first whether the
    engine's shadow pass draws a mask material through the program's own shader, which samples the
    atlas, or through its own depth shader without it, and where it cannot cut them, ask the engine
    in its ASKS.md with the reading and keep the plants shadowless until the package. The shadow's
    pixels step with the sun's map, which the owner reads as fitting the look; the plants stay out
    of the scene's field, so the global illumination is untouched. Tests: a blade's shadow on the
    ground read from a capture's pixels as narrower than its quad, and a capture of a meadow at a
    low sun in the record. One commit, small, or the ask. Read at `bc02459` on 2026-10-21: the
    shadow pass does not cut a masked surface drawn through a program's shader, so the entry is in
    the engine's ASKS.md as its item 79 and the game takes the shadows when the package brings the
    cut; the capture committed.

56. **More plants, the source's counts read and the modpack's look matched (Decision 27).** The
    owner wrote shortly before 07:12 on 2026-10-21 that there is way too little vegetation. The
    grasses stand at 140 a thousand places in the plains, 180 in the savanna, 80 in the jungle, 60
    in the birch forest, 50 in the forest, 40 in the swamp and the cherry grove, 20 in the dark
    forest and 10 in the mountains, the ferns 40 in the taiga and the jungle, the flowers 14 in the
    plains and 8 in the woods. Read first the source's own counts for 26.3, its placed features'
    grass, tall grass and flower tries a chunk, from the client's data as the mobs' boxes were read;
    measure the game's and the source's covered share of a chunk's grass top; and raise the game's
    toward the modpack's look, the biome overhaul's numbers read as numbers, each biome's grasses,
    tall grass, ferns and flowers. A test counting short grass, ferns and flowers in a 12 by 12
    columns plains of seed 1 against the new share, and the meadow's capture taken again beside the
    modpack's picture. One commit, after item 55. Done at `d32ee21` on 2026-10-21, as the head says;
    the blades reading flat at a distance beside the modpack's is for the owner's word.

57. **The plants stand offset in their block as the source places them (Decision 27).** The owner
    wrote shortly before 07:36 on 2026-10-21 that in Minecraft the grass and the flowers are not
    placed uniformly on the block but randomly positioned and rotated or so. The source offsets them
    and does not turn them, read from the client's code as numbers: from a seed of the place, up to
    a quarter block either way in 16 steps along x and z, and the short grass and the ferns sinking
    up to a fifth of a block too, the flowers, the saplings and the tall grass only sideways. The
    mesher offsets each plant's walls by the place's hash the same way, the grass and the ferns
    sinking, the petals, the wheat, the mushrooms and the dead bushes keeping their place as in the
    source; a wall carries its own texture coordinates, the mesher numbering its corners from the
    lower left with the vertex stage handing the fragment zero to one across and down, since the
    shader laid a wall's texture by its world position, which would slide under an offset, the light
    staying snapped to the block's pixels; the README's plants paragraph. Tests: the same place
    giving the same offset and two places differing, every offset within a quarter, the grass
    sinking no more than a fifth and the flowers not at all, and the walls a block wide corner to
    corner. One commit, small, before item 39. Done at `815e4ca` on 2026-10-21, as the head says.

58. **The light and the grade after the modpack's shader settings the owner set, read as numbers
    (Decision 28).** The owner wrote shortly before 07:38 on 2026-10-21 that the blades should look
    like the modpack's, and that comparing the colors the game's and the modpack's pictures differ,
    so the modpack is to be pulled and the shader settings they set fetched, the colors by time of
    day, the saturation and so on. The modpack's shader runs under a license forbidding
    redistributing or changing its code, so it is read as numbers and the game's own written: the
    owner's settings against the shader's defaults, the exposure 0.90 against 1.40, the white curve
    1.3 against 2.0, the lower curve 1.05 against 1.20 and the upper 1.00 against 1.50, the
    saturation 1.05 against 1.55, the ambient 50 against 150, the fog 0.95 against 0.50, the night
    tint 1.10, 0.90 and 1.30, the bloom 0.027, the shadows stepped at 8 a block and reaching 64, no
    color grading; the shader's tonemap multiplying by the exposure, dividing by the white curve's
    term, a power from the lower curve in the darks to the upper in the lights mixed by the root,
    gamma and the saturation about the channels' mean; its sunlight at noon a warm yellow, orange at
    sunset by a power past noon, dim and cool at night, the ambient the sky's color to the 0.65
    times 0.85 scaled by the ambient setting, where the game's sun is near white, the warmth and the
    deep shade being most of the pictures' difference. Five parts, each shown beside the modpack's
    pictures before its commit: the sun's color by hour after the shader's numbers and the sky's
    light at the share the owner's ambient gives, measured as the shade-to-sun ratio on a white wall
    before and after, which takes item 43's sun from it; the night sky and the moonlight tinted; the
    grade, the saturation through the engine's color grading and the curve asked of the engine in
    its ASKS.md, a parametric curve set by the four numbers or a tonemap through a program's shader,
    the engine's nearest curve and exposure chosen meanwhile by the curves' values at a few levels
    and stated; the haze's start and strength read from the fog setting and matched, which item 43's
    haze follows; and the shadows stepped at 8 a block and reaching 64, offered to the owner rather
    than taken. The bloom stays at the owner's tenth. Tests on the sun's color at four hours, the
    ratio, the tint and the haze. Three or four commits, after item 57 and before item 39. The
    ambient's share, read at 08:00, cannot be set apart from the drawn sky with the engine's
    environment map, so it is asked of the engine as its item 81 and the game takes the share when
    that package comes; the grade chosen by the owner at 08:00 is Reinhard at an exposure of 1.7
    with the saturation and contrast graded, checked again on the green grass of item 59. Part a
    done at `82da862` and part c at `396047d` on 2026-10-21, the saturation 1.4 on the owner's
    second word, the two entries in the engine's ASKS.md for its items 80 and 81; part b done at
    `293a5d1` and parts d and e at `b56fa0b` on 2026-10-21, the shadows' grid eight a block on the
    owner's word, Decision 29; the ambient's share waits on the engine's item 81 as said. Done at
    `b56fa0b`.

59. **Each biome's grass and foliage color from the source's fixed values, not a blend of the game's
    own climate (Decision 28).** The owner wrote shortly before 08:01 on 2026-10-21 that the game's
    grass is not the same as the modpack's, which is green where the game's is tealish. Measured,
    the modpack's sunlit grass top reads 127, 139 and 31 and the game's 140, 157 and 97 graded; the
    cause is the game's tint, which blends the colormap's corners by its own temperature and
    humidity noise, the plains at 0.38 and 0.41 giving 137, 184 and 116 with blue in it, where the
    source gives each biome one color from its fixed temperature and downfall, the plains' 0.8 and
    0.4 giving 145, 189 and 89; lowering and whitening the sky's light alone took the blue only from
    97 to the seventies. So each biome's grass and foliage color is the colormap's value at that
    biome's own temperature and downfall as the source's biome files give them, read as numbers, the
    plains, forest, birch forest, taiga, snowy, savanna, jungle, windswept hills and river, blended
    across biome edges by weight as the special biomes already blend, the dark forest, swamp,
    badlands and cherry grove keeping their own; the continuous climate colormap goes. A test reads
    each biome's middle at its color within a level or two. Then the grade is checked again with the
    owner on the green grass. One commit, before item 58's first commit. Done at `fa7fba0` on
    2026-10-21, as the head says.

60. **The lamps' light falls off smoothly, the eight-a-block grid taking the sun's light and shadow
    alone.** The owner asked at about 09:20 on 2026-10-21, relayed verbatim, "Why is the torches
    fallof smooth and the lanterns pixelated?" The cause, confirmed in a live session: the terrain
    shader takes all of a face's light at the middle of its step on the grid of Decision 29, the
    sun's light and shadow, the lamps' light and the global illumination alike; a lantern's lit
    glass stands on the floor, so its light on the floor falls steeply over the first half block and
    the eighth-block steps show as squares, where a torch's flame of 2 by 7 to 10 pixels lights the
    floor slowly and the steps do not show; at 64 steps a block the pools come out round. The mend,
    Decision 29 read as the sun's shadows alone as its words say: the grid stays for the sun's light
    and shadow and the rest of the light is taken at the point itself, as the mob shader already
    takes the sun apart from the rest, one more shadow lookup a pixel, its cost measured on the
    machine at the usual view and in the commit's description; a test reading a lantern's pool on
    the floor in a night capture for a smooth fall across a block, and the README's shadows
    paragraph saying the grid is the sun's. One commit, small, after item 39's first commit and
    before its second part.

61. **A mob's side faces take no bands from their own shadow at noon.** The owner said at about
    09:20 on 2026-10-21, relayed verbatim, that "the capture with the mobs showed pixelated layer
    lines on there bodies which should not happen i think". The cause, confirmed in a live session:
    the mob shader floors each face's light at the flat shade of the light a face turned up takes at
    the same point, which looks up the sun's shadow at a point on a side face with an upward normal,
    so the shadow map's offset runs along the face instead of out of it and the face shadows itself
    in horizontal bands wherever the floor outweighs its own light, every side face at noon and none
    at 16:00 with the sun on them; with the turned-up light taken a sixteenth of a block out from
    the face along its own normal the bands are gone. The mend: that line in mob.slang, a test
    reading a mob's side face in a noon capture for alternating rows and finding none, and
    mobs/daylight.webp taken again. One commit, small, after item 60.

62. **Every mob wears the pack's entity picture (Decision 30).** The owner wrote shortly after 09:40
    on 2026-10-21, relayed verbatim, "Make the face of the villager from the bare bones and the
    zombie too and the other mobs", read as every mob taking the pack's entity texture. Bare Bones
    holds pictures for all nine kinds, the villager with its biome type and profession overlays, the
    zombie, the skeleton, the creeper, the spider, the pig, the cow, the sheep with its fur and the
    chicken, fetched with the pack as Decision 18 has it, nothing shipped. The mobs' boxes are
    already the source's cuboids taken as numbers, colored a face with no texture, the faces extra
    small boxes. The mend: each cuboid takes its texture offset from the source's model definitions,
    read as numbers as the boxes were, and the source's box unwrap, the top and bottom at the offset
    moved right by the depth, then the right, front, left and back faces in the row below, a
    mirrored box flipped; one material a kind with the pack's picture, the mob shader sampling it in
    place of the vertex color; the face boxes left out where a pack gives the picture and kept with
    the colors where none does; the villager's biome type drawn over its base picture and a
    profession where the game has one; the sheep's fur as a second box set tinted by the wool's
    color; each mob's own colors kept with no pack. Tests: a cuboid's face coordinates against the
    source's numbers, a face's pixels read in a capture, and mobs/daylight.webp and the pair sheets
    taken again. Two commits, the unwrap with the shader and the material, then the villager's
    layers and the sheep's fur. After items 60 and 61 and before item 39's remaining parts.

19. **Four game modes and the switcher, F3 with F4 opening the row of icons in boxes, F4 stepping,
    spectator among them.** The owner's word: F3 with F4 does not switch modes, spectator is not
    there, and the small panel of boxes with icons is missing. The game has creative and survival;
    it gains adventure, no placing or breaking but interaction, and spectator, flying through blocks
    with no body, no collision and no inventory, which takes the engine's character flight of
    5.2.190 where it fits; F3 held with F4 shows the switcher, four boxes with icons and the mode's
    name, F4 stepping through them while held and the choice taken on release, as the source does;
    the mode in the status and saved. One commit. Done at `d9c2f8d` on 2026-10-21, as the head says,
    spectator flying by the game's own controller.

20. **The inventory's quick moves as the source has them.** The owner's word: the quick inventory
    control with shift and alt does not work in survival. The inventory window takes left and right
    clicks alone; it gains shift-click moving a stack between the hotbar and the inventory or into
    the open container, number keys over a slot swapping it with the hotbar's, double-click
    gathering a kind, drag over slots spreading a stack, and drop by Q with control for the stack,
    the keys read where the window is open in survival and creative alike. A sort of the inventory
    or the open container by a key and by a middle click on its panel, as the sorting mods of the
    owner's modpack do (Decision 14), kinds together and stacks filled. One commit. Done at
    `9bd58e3` on 2026-10-21, as the head says, the furnace and the grids being the open containers
    the game has.

21. **Stairs join at corners, place by the hit, and the aim picks the shape and not its cell.** The
    owner's word: stairs do not connect on a corner, placing is odd, and the hover outline is the
    block and not the mesh, so the aim picks the one in front. The stairs' shape reads its neighbors
    for the inner and outer corner forms as the source does, placing reads the hit's half and the
    player's facing for upside-down and direction, the ray tests each piece of a shape and not the
    box of all, so a ray through a stair's empty quarter reaches the block behind, and the outline
    draws the shape's pieces; the same for slabs, fences and every shape of pieces. Two commits, the
    corners and the placing, then the ray and the outline. Done at `baf26b9` and `5b2f9d3` on
    2026-10-21, as the head says, the placing found right already.

22. **The baked ambient occlusion has a strength.** The owner's word: the AO baked into the mesh
    needs a strength parameter. The mesher bakes four fixed levels; it takes a strength from zero to
    one that scales the levels toward one, set in the settings and by a command, the meshes built
    again when it changes, the default the levels of today. One commit. Done at `bf13ff6` on
    2026-10-21, as the head says.

23. **The interface drawn as the game's own, in the source's style, ImGui kept for the debug
    windows.** The owner's word: the UI is very ImGui-styled. The hotbar and HUD are drawn shapes
    already; the inventory, the creative items, the settings and the pause screen become the game's
    own 2D drawing, panels of nine-slice textures, slots with item icons drawn from the block's
    faces, tooltips and a cursor stack, in the source's look with the game's own touches, the keys
    of item 20 kept, ImGui left for the debug and settings' developer parts. The look is Bare Bones'
    (Decision 13), flat shapes, few lines and plain colors, drawn as the game's own art, and where a
    pack of item 24 is loaded its gui textures, the hotbar, the hearts, the slots and the panels,
    are drawn in place of the game's. The HUD's hearts, hunger and air fade when full and unchanged
    and return on a change, as the owner's modpack's HUD does, a setting on by default (Decision
    14). Two or three commits, the panels and slots first, then each window. Done at `8722725`,
    `8b1d584` and `4c0c50e` on 2026-10-21, the art and the HUD, the windows, then the menu and the
    options, as the heads say.

24. **Texture packs read from the player's own resource packs, shipping no one's textures.** The
    owner's biggest word: texture pack support, with a mod called Shine in the reference folder as
    what adds a lot. Shine is a Fabric mod of code and shaders under all rights reserved, so none of
    it may enter the game, only what it shows done, which is item 28; the packs the owner's own
    instance holds are the source's resource packs, zips or folders of block, item and entity
    textures by name under the source's paths. The game gains a pack loader reading such a pack from
    a folder beside the game or a chosen path, mapping each block's faces to the pack's texture
    names by a table the game keeps, falling back to its own texture where a pack lacks one, the
    atlas built from the pack at load with mipmaps and the pack's resolution, a setting naming the
    pack and a command to reload, so a player's pack shows and the repository ships only the game's
    own art. The loader reads the pack's item textures for the hotbar's and the inventory's icons
    and its gui textures for item 23's interface as well as its blocks, so Bare Bones shows whole
    (Decision 13); a pack is a zip or a folder, and a list of packs in order as the source layers
    them, the later over the earlier, so Bare Bones with its add-ons reads as it does in the owner's
    instance. The list is the source's plain order, the first pack over the rest, which gives the
    owner's pick as the packs are, Bare Bones first and Recolourful Containers second (Decision 17);
    the windows and the HUD are drawn from a pack's gui textures by the source's paths and layouts,
    the container pictures of 176 by 166, the HUD's sprites of 9 and the widgets' nine-slices by
    their metadata, where a pack has them. Entity textures and models follow in item 30. Three
    commits, the loader and the atlas, then the windows and the HUD from a pack's gui textures, then
    the setting, the list and the reload with a resourcepacks folder beside the saves as the source
    keeps one; the game's own art stays as it is, the owner having seen it moved toward the two
    looks and asked for the packs' own look instead, shortly before 03:23 on 2026-10-21 (Decision
    17). First commit done at `193da5e` on 2026-10-21, the loader and the atlas, the second at
    `2839dba`, the windows, the HUD and the font from a pack, and the third at `056bb7b`, the
    setting, the list and the reload, as the heads say. A fourth commit by Decision 18: with no list
    written and the run not transient, the game fetches the three packs the owner named from their
    creators' own Modrinth pages into its resourcepacks folder on a background task, each file
    pinned by its Modrinth version, its file URL and its sha512 in a manifest the repository holds
    as a modpack's manifest does, checks each hash, writes the list in the owner's order and reloads
    when all three are in, the first frames drawn with the game's own art meanwhile; offline or on a
    failed check it plays with its own art and tries again at the next start; the README says the
    game fetches them at first start and names the three and their terms, the Resource Packs screen
    shows the fetch's state with a switch to stop it, and the request carries a user agent naming
    the game as Modrinth's API asks. Tests: the manifest's hashes read and a file of the wrong hash
    refused, with the fetch itself skipped in the suite and in CI. Done at `e14134d` on 2026-10-21
    with the fourth commit, after `c5df182` took the windows' titles off, as the head says.

25. **More biomes, more blocks and more trees.** The owner's word, three times. The climate has
    eight biomes, the features three trees and the cactus, and the blocks about 140 ids: biomes gain
    the swamp, the savanna, the jungle, the dark forest, the badlands, the beach and the river, each
    with its surface, its colors of grass and leaves and its features; blocks gain each biome's
    wood, leaves, sapling, flowers, tall grass, mushrooms, sand and gravel kinds, clay, ores by
    depth and the glass and wool colors; trees gain the jungle, the acacia, the dark oak, the big
    oak and the giant mushroom, from the source's shapes as numbers with the game's own touches. The
    climate ranges, the surface rules and the feature counts of the seven biomes are read for their
    numbers from the biome overhaul's JSON in the owner's modpack as from the source's, a model and
    not a copy, since its license forbids redistribution (Decision 14). Three commits, biomes,
    blocks, trees, each with the scripted run's walk through one of each and a capture. The badlands
    with its terracotta bands around turquoise pools and a cherry grove join the biomes, and a
    lantern and the flowers the look shows join the blocks (Decision 21). First commit done at
    `a6ec820` on 2026-10-21, the eight biomes with their land, colors and new blocks, the second at
    `723e28c`, the seventy blocks, the plants as planes and the ores by depth, and the third at
    `c54f362`, the nine trees with the cherry's lanterns, as the heads say; item 25 done.

26. **The world's layout: sections streamed, meshed and saved in three dimensions with a taller
    world, and far land drawn at a coarser level by distance, the blocks' size kept.** The owner's
    word: a column is 16 by 16 by 128, make it cubic at 16 or 32 or do a level of detail, where an
    earlier level of detail scaled far blocks by powers of two so they looked as big as near ones,
    which looked wrong. The reviewer's reading: meshing, drawing and culling go by 16-cubed sections
    already, and generation, streaming and saving by whole columns; the terrain's generation needs
    the column's context, the heightmap, the biome and the surface, so it stays by column, and the
    world grows to 24 sections, 384 blocks from minus 64 to 320 as the source's, with the deep
    sections for caves; streaming, meshing and saving go by section with a distance in three
    dimensions, so a far column's deep sections are neither meshed nor kept, and a column is
    generated whole once and its sections saved apart. Far land is drawn at a coarser level by
    distance, each doubling of the distance halving the detail, a far section meshed from blocks
    merged two by two by two and then four by four by four into cubes of the merged size at their
    own place, so the world keeps its size and far blocks look smaller with distance as near ones
    do, lit flat with no ambient occlusion and no entities, the coarse levels made from the
    generator's surface alone, the heightmap and the top blocks, without the column's full blocks,
    so a far column costs a surface and not a volume; the earlier level of detail scaled the far
    blocks up to look as big as near ones, which is the thing to avoid. The render distance of near
    sections stays at the setting, and the far levels reach as far as the setting for them, with the
    memory and the frame read at 8, 12 and 16 near and 32 and 64 far. Read first, with the costs of
    the three parts and the form of the coarse mesh said before any commit: the taller world and the
    sections' streaming and saving, then the far levels. The far levels read first from the saved
    world, the sections a player has already visited and saved, before the generator's surface, as
    the modpack's far-chunk cache keeps far land drawn from what was seen (Decision 14). Three or
    four commits.

27. **Caves as the source's newer ones, cheese, spaghetti and noodle, with aquifers.** The owner's
    word: caves are a big one, the source's newer ones are beautiful. The world has tunnels where
    two noises are both near zero, caverns where a third is high below a depth, and ravines; it
    gains the source's three kinds as numbers, cheese caves from a large three-dimensional noise's
    low values, spaghetti tunnels from two noises thinned by a third, noodle caves thinner still,
    and aquifers setting water and lava levels in caves by depth, with the deep sections item 26
    gives them, and a cave's lighting from the sky above through its openings. After item 26. Two
    commits, the caves, then the aquifers. The lush caves of the source, moss, glow berries hanging
    from the roof and vines over clear pools, join the kinds (Decision 21).

28. **The looks Shine shows, done as the game's own: bloom on emissive blocks, colored light from
    emissive blocks, a rim around blocks, and an unshaded look.** From item 24's reading of the
    reference folder's mod, whose code may not be taken: the engine's bloom on the emissive blocks'
    faces, the colored light the engine's lamps and global illumination give from emissive blocks
    read and set so a lantern's light is its color, a rim light as a setting in the terrain shader,
    and an unshaded look as a setting, each a switch in the settings with a capture. After item 24.
    One or two commits.

29. **Generation and meshing on the GPU, the blocks kept on the CPU (Decision 12).** The owner's
    word of about 00:30 on 2026-10-21: compute shaders for generation and the rest are a big one.
    The reading at `5cd1521`: each column is generated on a worker task off the main thread, and
    item 16's flight readings put the frame's cost on the main thread in the join and the meshing,
    21 to 29 ms a frame at 12 columns, so generation on the GPU makes columns arrive faster and
    meshing on the GPU is what cuts the frame; the engine's compute, `LoadComputeShader`, the shader
    buffers and `ComputeShaderDispatch`, reads a buffer back only through `ReadShaderBuffer`, which
    holds the program until the GPU has caught up, and draws nothing a shader wrote as a mesh. The
    approach: the blocks stay in memory on the CPU, since collisions, edits, light and saves read
    them there and a column's blocks are about 100 KB, cheap to read back; the GPU computes what is
    heavy, the noise fields and the meshes. Three parts, each after the engine's package brings what
    it needs, asked in the engine's ASKS.md by the game's coder with the readings above. Part one,
    the fields, after the engine reads a buffer back without a wait (its item 68): a compute shader
    a batch of columns writes the terrain's density, the biome's climate values and item 27's cave
    noises as fields of 16 by 16 by the world's height, 16 bits a cell, read back a frame or more
    later while the game goes on, and the generator on the CPU places blocks from the fields as it
    does from its noises today; the noise written as integer hashes in fixed point in both the
    shader and C#, so the two give one field, and the CPU's functions kept as the path for a device
    without compute, for the tests and for `voxel.gpu off`, a setting; a test that both paths give
    the same field for a seed on a column, bit for bit, and the play run on lavapipe on the CPU path
    and once on the GPU path. Part two, the meshing, after the engine draws a mesh over buffers a
    shader wrote with a count the shader writes (its item 69): a compute mesher a section, the
    section's blocks uploaded as 4 KB and its six neighbors' faces, writing the positions and item
    7's packed stream into the mesh's buffers with the ambient occlusion of item 22 as a uniform,
    the engine drawing them with no upload and the global illumination reading them as any mesh; an
    edit uploads the section's blocks and dispatches again; the frame's meshing cost then read
    against item 16's 21 to 29 ms, the CPU mesher kept for the path without compute and as the
    reference the GPU mesh is tested against by triangle count and bounds. Part three, the far
    levels of item 26 on the GPU, the heightmap and the top blocks of a far column computed and
    meshed there without the column's blocks, so far land costs the CPU nothing but a dispatch.
    Light levels, the flood of sky and block light, stay on the CPU until the three parts are read,
    and are a fourth part if their cost shows. Item 26's reading says where the GPU path changes its
    costs. Three or four commits, one a part, each after its package.

30. **Entity models, textures and animations read from the player's packs in OptiFine's form, the
    game's own the fallback (Decision 13).** The owner's word of about 00:55 on 2026-10-21: the
    packs of their modpack, Bare Bones and Fresh Animations among them, have the look they like, and
    a Bedrock add-on reworks all of this in the same spirit. Fresh Animations is 100 custom entity
    models under `optifine/cem`, `.jem` files of parts and boxes with `.jpm` parts and animation
    expressions over the entity's variables, with 170 entity textures, and its terms allow personal
    use and building on it with credit, never redistribution, so none of it ships; the game reads
    the player's own copy as item 24 reads the blocks. The game gains a reader of the form: a `.jem`
    model's parts, boxes, texture offsets, pivots and rotations into the game's cuboid model of item
    13 at the source's scale, its texture from the pack by the model's name, and its animations as
    expressions parsed once and evaluated each frame over the variables the form names,
    `limb_swing`, `limb_speed`, `head_yaw`, `head_pitch`, `age`, `hurt_time`, `is_child` and the
    rest the game can give, the functions of the form's expression language in a small evaluator of
    the game's own; a mob whose kind the loaded packs model is drawn with the pack's model and
    animated by its expressions, the game's own model and walk of item 18 otherwise, read per kind
    at load and on `voxel.packs reload`; the parts the game cannot give, random variables and the
    attack times, hold their defaults. Tests: a `.jem` of a known box and a one-line animation
    parsed to the counts and evaluated at two values, the expression language's functions each on a
    value, and a capture of a villager and a cow from the owner's packs beside the game's own in the
    record, read from the packs where the machine has them and skipped where it has not. After item
    24. Two commits, the models and the textures, then the animations. Under Decision 22 the reader
    takes a model's geometry and its texture as assets and none of its animation expressions, the
    mobs animated by the game's own gait of item 18 on the pack's boxes; two commits become one.

31. **Structures as the source's, more kinds and the modpack's taste (Decision 15).** The world has
    igloos, desert pyramids, dungeons and villages; it gains mineshafts with their corridors, rails,
    supports and chests, underwater ruins and shipwrecks in oceans, jungle temples, witch huts in
    swamps and pillager outposts, each placed by the biome and a spacing and salt as the source
    places them, built from blocks with loot in chests from item 25's blocks; the villages gain the
    biome's kind, desert, savanna, taiga and snowy, with their own blocks and the modpack's
    structure mods read for their shapes as a model; a structure that crosses a column's edge is
    finished in the neighbor as the villages are. After items 25 and 26. Tests: each kind placed in
    a laid world at a known seed with its blocks counted, a structure crossing an edge whole on both
    sides, and a capture of each in the record. Two or three commits by kind.

32. **Weather and the ambient particles, rain and snow by biome, falling leaves, cave dust and wakes
    (Decision 15).** The game gains weather, rain in the wet biomes and snow in the cold at the same
    hours, a chance a day and a length, with the sky and the light dimmed, the rain drawn as the
    source draws it in a ring about the camera from the sky down to the highest block and the snow
    laid a layer thick on the top blocks while it snows; and the ambient particles of the modpack's
    kind through the engine's emitters, raindrops splashing on the blocks they meet and rings on
    water, leaves falling from leaf blocks in the forests at a slow rate and turning as they fall,
    dust drifting in caves under a depth, wakes and splashes on water the player and the mobs move
    through, each a setting on by default with a density. The weather is saved. Tests: the weather's
    clock over days at a seed, the snow laid and melted, the particle counts within their budget at
    the default density in the frame's profile, and a capture of rain in a forest and snow on a
    village in the record. Two commits, the weather, then the particles. Embers in the nether's kind
    of dark and spores in lush and deep caves drift as the look shows (Decision 21).

33. **Ambient sound, footsteps by the block and the sound's occlusion and reverb by the room
    (Decision 15).** The game gains footsteps that sound by the block walked on, grass, stone, wood,
    sand, gravel, snow and water, for the player and the mobs within hearing, through the engine's
    spatial play of its item 58 so a mob's steps come from its side; ambient sounds by the place,
    birds in the forests by day, crickets by night, wind on the heights, drips and a low tone in
    caves, water near water, faded in and out by where the listener stands; and the sound's
    occlusion, a ray through the blocks from the listener to the source lowering the volume by what
    it crosses, with the reverb of a cave or a room by the space around the listener, read by rays.
    The low-pass of an occluded sound and the reverb itself are the engine's to give, a filter and a
    reverb on a voice, which the game asks of it in the engine's ASKS.md with its readings when it
    reaches this item, and the game does with the volume alone until the package brings them. The
    sounds are the game's own recordings or synthesized, none from the source or the mods. Tests:
    the step's sound chosen by the block, the occlusion's volume through one and three blocks, the
    ambient sound chosen by the place, and the mix read as the engine's spatial tests read it. Two
    commits, the footsteps and the ambience, then the occlusion and the reverb.

34. **Connected textures from the packs' properties (Decision 15).** A pack of item 24 that carries
    connected texture properties, OptiFine's `.properties` under its `optifine/ctm` folder with the
    methods `ctm`, `horizontal`, `vertical`, `top`, `repeat` and `random` and their tiles, has them
    read, and the mesher picks a block's face texture by its neighbors of the same kind as the
    methods say, the 47 tiles of the full method by the eight neighbors, so glass, bookshelves and
    sandstone join as they do in the owner's instance; the atlas takes the tiles as it takes the
    blocks. Without a pack, the game's own blocks stay as they are. After item 24. Tests: the
    properties parsed to their methods and tiles, the full method's tile chosen for each of a set of
    neighbor cases, a random method stable by position, and a capture of a glass wall from a pack
    where the machine has one, skipped where not. One commit. Under Decision 22 the properties are
    read as a picture's metadata, the tiles and the method's name, and the methods are the game's
    own.

35. **A zoom key (Decision 15).** C held narrows the field of view to a quarter of the setting over
    a tenth of a second and widens it back on release, the mouse's sensitivity scaled with it so the
    aim stays steady, the wheel while held narrowing further to a tenth, as the modpack's zoom does;
    the key settable in the settings. Tests: the field of view over the frames of a press and a
    release, and the sensitivity's scale. One commit, small.

36. **The player seen in third person, with their own animations (Decision 15).** F5 cycles the
    first-person view, a camera behind the player and one in front, as the source's does, the camera
    pulled in where a block would hide it; the player has a model of the source's layout, head,
    body, arms and legs at the source's scale with the game's own skin, animated by walking,
    running, sneaking, swimming, climbing, jumping, falling, mining and placing, the swings and the
    stride from the source's numbers as item 18's mobs take them, the held item in the hand, and the
    head following the aim; where item 30's packs model the player, the pack's model and animations
    are taken, and the first-person arm and held item are drawn from the same model. Tests: the
    camera's distance against blocks, each state's pose at a known phase, and a capture of the
    player mid-stride and mid-swim from behind beside the source's silhouette in the record. After
    item 30. Two commits, the camera and the model, then the animations.

## Verdicts

No verdict is open. Verdict 1 is settled on the build run of `8261308` on 2026-10-21, said under it.

1. **The build's play job is lost to its 40 minute limit on the walk over the eight biomes.** On the
    build run of `c54f362`, 39534411760, on 2026-10-21, the three test jobs passed in a minute and
    the play job was cancelled at 40 minutes 19 seconds, the job having exceeded its maximum
    execution time, its scripted run on lavapipe walking the eight biomes with the plants and the
    trees of item 25's second and third commits in the world, where the run of `a6ec820` before them
    had taken 37 minutes 17 seconds of the same limit. The fault is a limit set for a walk of one
    stop, kept as the walk grew to nine. The mend, made before the loss in item 44's commit, is the
    limit raised to 60 minutes with its reason in the comment, and the walk at render distance 2
    with the horizon's reading at 4 alone once this run reports the walk's time; a program that
    never reaches a stop still fails within the limit. Mended at `cc46f25` on 2026-10-21. Settles on
    the build run of `8261308` with its play job green and its time read. Settled on that run on
    2026-10-21, the play job green in 24 minutes 30 seconds. The walk went to render distance 2 at
    `be968a2` on 2026-10-21 after the run of `7039d5a` took 46 minutes.

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

11. **The owner's list of 2026-10-20 at 23:30, in the reviewer's order.** The owner named the game's
    next work whole and asked the reviewer for the best approach, its order and its items: out of
    the water first; the mobs' proportions toward the source's with some character kept; more types,
    biomes, blocks and trees; the interface in its own style rather than ImGui's; the inventory's
    quick moves; the four modes with the switcher of icons and spectator; the ambient occlusion's
    strength; texture pack support, with the mod Shine in the reference folder as what adds a lot;
    stairs at corners, their placing and the aim's outline; caves as the source's newer ones; and
    the world's layout, cubic at 16 or 32 or a level of detail, an earlier one of the owner's having
    scaled far blocks up so they looked as big as near ones, which looked wrong. The reviewer's
    order is items 17 to 28: the small mechanics first in the owner's own order where it was given,
    the water, the mobs, the modes, the quick moves, the stairs and the occlusion's strength, then
    the interface, then the texture packs, then the content, biomes, blocks and trees, then the
    world's layout as a reading first, the caves after it since they live in the deep sections it
    gives, and the looks Shine shows last since they build on the packs; Shine's code and shaders
    stay out, all rights reserved, and only what they show is done. The world's layout is the
    reviewer's answer to the owner's question: generation by column with the world taller,
    everything else by section in three dimensions, and far land at coarser levels that keep the
    world's size and let far blocks look smaller with distance, the opposite of the earlier scaling.
    The game's coder reads item 17 first.

12. **Compute shaders for generation, the owner's word of 2026-10-21.** The owner wrote at about
    00:30 on 2026-10-21 that compute shaders for generation and the rest are a big one too, and
    asked the reviewer for the approach as for the rest of Decision 11. The reviewer's answer is
    item 29: the blocks stay on the CPU, where collisions, edits, light and saves read them, and the
    GPU computes the noise fields and the meshes, the fields read back without a wait and the meshes
    drawn where they are written, with one noise in fixed point in both the shader and C# so the two
    paths give one world; the engine first reads a buffer back without a wait and draws a mesh over
    a shader's buffers, items 68 and 69 of its review, asked through its ASKS.md, and the game's
    parts follow each package. Items 18 to 25 go on meanwhile, and item 26's reading takes the GPU
    path into its costs.

13. **The owner's packs set the look, read from the player's copy and never shipped.** The owner
    wrote at about 00:55 on 2026-10-21 that the resource packs of their modpack, older but with
    visuals they like a lot, the HUD among them, are in the reference folder, that Bare Bones and
    its textures are awesome, that Fresh Animations and the better mods like it, a Bedrock add-on
    reworking all of this among them, give the general gist with no reference at hand. The reading:
    30 packs, Bare Bones 1.21 with 1,059 block, 594 item, 607 entity and 320 gui textures and its
    add-ons, Fresh Animations 1.9.2 with 100 custom entity models in OptiFine's form and their
    animations, the packs fitting the two together, and smaller ones; Bare Bones carries no license
    file and Fresh Animations' terms allow personal use and building on it with credit and forbid
    sharing, so none of them ships and the game reads the player's own copies. So item 24 reads a
    list of packs layered as the source layers them, with their item and gui textures beside the
    blocks; item 23 draws the interface in Bare Bones' look as the game's own art and in a loaded
    pack's gui textures where there is one; and item 30 reads the entity models, textures and
    animations in OptiFine's form, the game's own the fallback. Item 18 goes on with the scale, the
    cow and the pairs, since the game's own mobs stay what a player without packs sees.

14. **The owner's modpack is the reference of the game's feel, read for shapes, numbers and ideas,
    and nothing of it ships.** The owner wrote at about 01:00 on 2026-10-21 that the mods of their
    modpack are in the reference folder too, for reference and for what was in the modpack, not to
    be added whole or at all, and asked the reviewer for the best approach. The reading: 223 Fabric
    jars for 1.21.1 under `.ref/mods`, the packs moved to `.ref/resourcepacks`, each under its own
    license and left out of the repository. The approach: the modpack is the owner's taste written
    down, so each item reads the mods that bear on it for their shapes, numbers and what they show,
    as it reads the source itself, a model and never a copy; a mod that is data, the biome
    overhaul's JSON worldgen, is read for its numbers and the game writes its own; a mod that is
    code is read only for what it shows. Where that changed an item: item 20 gains the inventory's
    sort, item 23 the HUD's fading when full and unchanged, item 25 reads the biome overhaul's
    climate and surface numbers for its seven biomes, item 26 feeds the far levels from the saved
    world first as the far-chunk cache does, and items 18 and 30 are confirmed by the entity model
    and texture mods that load Fresh Animations' models. Not taken into items, named for the owner's
    word since the list is to be the owner's: structures in the world, villages, dungeons,
    mineshafts and ruins; ambient particles and weather, falling leaves, rain on surfaces, cave dust
    and wakes on water; ambient sound, footsteps by block and the sound's reverb and occlusion by
    the room, which the engine's default spatial processor of its item 58 begins; connected textures
    from the packs' properties; a zoom key; and the player's own animations in third person. The
    speed mods, the culling and the chunk threading, are what items 26 and 29 are about in the
    game's own terms, and the play mods, the combat and the trees, stay out until the owner names
    one.

15. **The six candidates of Decision 14 are taken.** The owner wrote shortly before 00:55 on
    2026-10-21, to the reviewer's list of what the modpack holds that was not yet an item,
    structures, ambient particles and weather, ambient sound, connected textures, a zoom key and the
    player's third-person animations, that all sound good. So items 31 to 36, each after the items
    it builds on and otherwise in the order the list had, the play mods still out until the owner
    names one.

16. **The screen probes stay off in the game, and the cascades' faults go to the engine.** The owner
    wrote at about 01:00 on 2026-10-21 that the game is not to enable the screen probes, since they
    give a pixelated look even where none is meant, that the radiance cascades without them are the
    better option for the game's art, and that the cascades still show faults, a light block's light
    through a thin corner gap, zig-zag penumbrae, the screen probes' shift on movement, z-fighting
    with the opaque textures and noise. So item 38, the repros to the engine's ASKS.md first, which
    the engine's item 71 takes, and the screen probes off by default once the engine gives the call.

17. **The windows after Recolourful Containers and the HUD after Bare Bones, read from the player's
    packs, and the game's own art moved toward them.** The owner wrote twice during item 24, shortly
    before 02:44 on 2026-10-21, that the HUD is to be done like the Recolourful Containers GUI pack,
    linking its gallery, and then that those are taken directly and the hearts and the rest from
    Bare Bones directly, with some room for character of the game's own, those looking the best. The
    reading: Recolourful Containers 1.1.2 replaces the source's container pictures, grey slots with
    white edges, the crafting area on planks and the furnace's top in stone, and 73 HUD sprites too,
    68 of them differing from Bare Bones'; Bare Bones carries its own container pictures beside its
    flat hearts, food, air, crosshair, dark olive hotbar with a green selection and its nine-slice
    widgets; neither carries a license file, so neither ships. The coder's first reading had Bare
    Bones carrying its own container pictures, which would have needed an entry limited to folders;
    corrected at 03:09, Bare Bones' container folder holds the anvil and the beacon alone, so the
    source's plain order, Bare Bones first and Recolourful second, gives the owner's pick as the
    packs are, Bare Bones' hearts, food, air, hotbar, selection, crosshair, buttons and sliders over
    Recolourful's inventory, crafting table, furnace and furnace sprites, with Recolourful's tooltip
    and creative scroller coming along since Bare Bones lacks them and the creative window's items
    keeping the game's own art since neither pack has them; the hotbar from Bare Bones by the
    owner's word shortly before 02:47; no entry names folders, the list being the plain order; and
    the game's own art, what ships with no pack, moves toward the two looks as the room for
    character allows, Recolourful's grey slots and white edges with the planks and the stone in the
    windows and Bare Bones' flat hearts, food, hotbar and green selection in the HUD, all drawn as
    the game's own pixels and nothing traced from either. The owner asked shortly before 02:50 what
    license their font pack has: qrafty's capitalized font holds one bitmap font picture and no
    license file, its page says all rights reserved with no terms, and Recolourful Containers' page
    says the same with terms allowing personal use and forbidding distribution of its assets in any
    form; so neither ships, and item 24's second commit reads the font picture from the player's
    pack where one has it, each glyph's width from its rightmost lit column as the source's bitmap
    font measures it with a pixel's gap and the shadow a pixel down and right, drawn at the
    interface's scale through the game's text calls, which fall back to the engine's font where no
    pack has one, the game's own art keeping the engine's font. Shortly before 03:23 on 2026-10-21
    the owner saw the game's own art moved toward the two looks, uncommitted, flat hearts and food,
    a green lit button, white-edged slots, planks behind the crafting grids and stone over the
    furnace, and said it looks ugly, naming the game's own windows and HUD, and to use what the
    earlier captures showed, the interface with Bare Bones, Recolourful and the font pack loaded; so
    the restyle was discarded before any commit, the game's own art stays as at `2839dba`, and the
    look the owner asks for is the packs' own, which the fourth commit's setting gives at every
    start on a machine that has them, the packs read from a resourcepacks folder beside the saves
    and the setting naming which are on and in what order. Shortly before 03:27 the owner added that
    the windows need not say inventory or crafting and such; read as the windows' titles, Crafting,
    Furnace, the Inventory label over the player's rows and the creative window's Items, all off
    with a pack and without, the slots, tooltips, counts and menus keeping their text; a small
    commit of its own after item 24's last.

18. **The three packs are not shipped and are fetched at first start instead.** The owner wrote
    shortly before 03:29 on 2026-10-21 to include Bare Bones, Recolourful Containers and the font
    pack as defaults shipped in the repository. The coder said it cannot be done, all three being
    all rights reserved, Bare Bones' page forbidding any reupload, Recolourful's terms forbidding
    distribution of its assets on any platform in any form without consent, the font giving no
    terms, and a public repository or a release zip being that distribution; and offered three ways,
    of which the owner chose the fetch on start: the game downloads the three pinned files from the
    creators' own Modrinth pages into its resourcepacks folder, checks each file's sha512 and writes
    the list in the owner's order, the repository holding only each pack's name, its Modrinth
    version and file URL and its hash, as a Modrinth modpack's manifest does, which Recolourful's
    terms allow for free modpacks. The reviewer adds that the fetch runs on a background task so the
    start is not held, the README says it, the Resource Packs screen shows it with a switch to stop
    it, and the request names the game in its user agent as Modrinth's API asks. Item 24's fourth
    commit.

19. **A screenshot on F2.** The owner wrote shortly before 03:42 on 2026-10-21 that, as in
    Minecraft, a screenshot is to be taken in the game with F2, the same for BevyCSharp, the
    captures put in the root or a subfolder. So item 40, a screenshots folder beside the saves with
    the source's names and the HUD's line, and the engines' items 73 and 47 giving every program the
    key.

20. **Simple Grass Flowers as a fourth default pack, through block state variants.** The owner wrote
    shortly before 03:53 on 2026-10-21, during item 25, that Simple Grass Flowers looks great and
    adds variations of the ground beside the grass and flowers the game lacks. Its page allows
    modpacks with credit, so it joins the manifest as a fourth default pack fetched from its maker's
    page and credited, and the game reads a pack's block state variants for the surfaces its table
    names, picked per block by position as the source does; item 41, after item 25. Shortly before
    05:00 the owner saw the pack over Bare Bones and said it looks good but changes the way Bare
    Bones' grass top looks and the style does not fit, recommending it be done natively in the style
    of the game, which looks very good; so the pack leaves the manifest, the game draws its own
    ground looks in its flat style, and the block state reader stays for packs that list looks.

21. **The look the game aims at is the owner's modpack's gallery.** The owner wrote shortly before
    03:57 on 2026-10-21, during item 25's biome commit, that visually they aim at their modpack,
    Better Trailer Optimized+, linking its gallery of 39 pictures, which the coder read and the
    reviewer looked at: Bare Bones' textures under a warm low sun with long soft shadows, calm water
    mirroring the sky and the shore, a blue haze over distant land, ores, lanterns, lava, sculk and
    a portal emissive with bloom in dark caves and halls, lush caves hung with glow berries and
    vines over clear pools, villages with flowers and lanterns, cherry groves, badlands of yellow
    and orange terracotta around turquoise pools, snowy spruce peaks, and embers and spores drifting
    in the dark. Against the game: the global illumination, the bloom, the fog, the sun's shadows
    and Bare Bones' textures are in; the rest goes by item, the glow of emissive blocks in item 28,
    the flowers and the biomes with the badlands and the cherry grove in item 25, lush caves with
    glow berries and vines in item 27, the ambient particles with the embers and spores in item 32,
    the villages' lanterns in item 31, and two items added for what no item held, the water's
    reflection and depth as item 42 and the sun's color by the hour with the soft shadows and the
    haze as item 43; the nether stays out until the owner names it. Each item that carries a part of
    the look puts its capture beside the modpack's picture in the record, so the owner judges the
    distance.

22. **Packs give pictures alone, and every rule is the game's own.** The owner wrote shortly before
    05:04 on 2026-10-21, during item 41, that the game has only visual resource packs, that Simple
    Grass Flowers tried to implement logic, which is not to be done, the Shine mods too, and that
    logic is implemented while textures and assets with licenses are pulled and used. So a pack
    gives its pictures and what says how a picture is cut, sliced or stepped, its textures, sprites
    and font and their metadata, each under its license; the game reads nothing of a pack's block
    states, models or rules that decide how a block is drawn or chosen, and every behavior is
    written natively; mods are references for what to build and are never implemented from their
    code or their data rules. Item 41 drops its block state reader and keeps the game's own ground
    looks; item 30 reads a pack's entity textures and its models' geometry, the boxes, as assets and
    animates the mobs natively with the game's own gait, taking none of a pack's animation
    expressions; item 39's animated textures stepped by their metadata's frame time are a picture's
    own timing and stay; item 34's connected textures take the tiles and the method a pack's
    properties name, as metadata of a picture, with the methods written natively, which the owner
    says otherwise if they read it as a rule; item 28 was already what Shine shows done natively.

23. **The look without a pack takes Bare Bones' colors.** The owner wrote shortly before 05:10 on
    2026-10-21, after item 41, that for the look without a resource pack the game can use Bare
    Bones' colors to match it at least, since the game has different colors for them. So item 45,
    the surfaces' flat colors set to the pack's averages read as numbers, nothing of its pixels
    shipping, the plants keeping their own.

24. **The README's pictures, an animation at its head and pictures in its parts.** The owner wrote
    shortly before 05:15 on 2026-10-21 to update the README's images, to put a GIF of different
    captures 250 ms apart at the top, and to showcase things as images in the parts below, the
    interface and crafting among them. So item 46, every picture taken by the pictures script with
    no pack in the game's own art, none of a pack's textures in the repository. Shortly before 05:25
    the owner added that the captures use the resource packs as well, so the pictures are taken with
    the three default packs on, read as their terms allow for pictures of the game, each credited
    with a link beside the pictures, the repository holding no file of any pack and one pair showing
    the look with no pack beside the packs'.

25. **Three words during item 46's scouting, the sky's bodies cut, the rifts in villages and the
    sun's look.** The owner wrote shortly before 05:40 on 2026-10-21, seeing the pictures scouted,
    that the sun's and the moon's flare are cut off at the bottom, asking whether the far plane or
    the skybox does it, which is item 44's dome and item 47; that there are a lot of rifts in
    villages, to be made rarer since they make the structure buggy and villagers could fall, which
    is item 48; and that they do not quite like the sun and the flares, the source's default
    dithered pixelated square one looking better, which is item 49. The three come before item 46's
    final captures, since the pictures show them. The owner also said of the earlier village and
    forest shots that they are not composed well, other images being better suited and better
    looking villages existing, so both are replaced in item 46. Shortly before 05:42 the owner added
    that the camera lands inside blocks in some captures, to show the block's texture on the whole
    screen then and to check the eye before a capture and move out or find a better spot, which is
    item 50 and a check in item 46's script. Shortly before 05:55 the owner, seeing item 49's sun,
    said they like Bare Bones' more and that it is to be implemented in the native look without
    resources too, a smoothing of the dithering, the flares of the sun and the moon; item 51, the
    glow smooth as the pack's, read as numbers. Shortly before 06:02 the owner asked what the stars
    are and what the source's are, and shortly before 06:02 the owner said to do squares instead of
    the star shape and to try the source's layout with its values, comparing both; item 52, the
    owner choosing between the two side by side. Shown the pair shortly before 06:10, the owner said
    the night sky in their modpack looks different, and shown a third layout from the modpack's
    shader read as numbers, chose it. Shortly before 06:26 the owner said twice to capture with the
    resource packs for the README, and that there is too much bloom, choosing 0.1 of four shown,
    item 53. Shortly before 06:46 the owner added that the lantern could take Bare Bones' texture to
    look better, which item 39's first part takes with the torches.

26. **The engine's F1 and F3 come to the game.** The owner asked shortly before 07:05 on 2026-10-21
    for F1 to hide the interface as Minecraft does, as an engine feature, F2 the screenshot, and F3
    a debug overlay like the newest Minecraft's, sorted in parts logically with modules a game adds,
    world generation among them, showing the system, the camera and the renderer, memory, fps and
    frametime with a graph like the RivaTuner overlay's, both for BevyCSharp and 3DEngine, F1 hiding
    the interface and ImGui but not the F3 overlay so everything hides but the overlay over the
    renderer, and Shift+F3 showing the frame debugger too; and answered the reviewer's questions
    shortly before 07:06: the interface hidden by a scope, F3 the overlay's on its release alone
    with any chord left to the program, the look Minecraft's with no header, window styling or
    border, text on a transparent backing, and the order as the reviewer sees fit. The engines take
    them as their items 78 and 49, and the game takes them as item 54 when the package comes, its
    developer's parts as overlay sections and its F3 chords kept.

27. **More vegetation, and the plants cast shadows.** The owner wrote shortly before 07:12 on
    2026-10-21, seeing the README's pictures, that there is way too little vegetation and that the
    plants are to cast shadows too, the grass blades and such, which fit as they are pixelated. So
    item 55, the plants' shadows cut by their texture or an ask of the engine, first since it is
    small, and item 56, more plants from the source's counts and the modpack's look; item 39's
    lantern part waits behind them. Shortly before 07:19 the owner said the README's animation
    switches too fast and chose two seconds a picture, and that the vegetation meant is the grass
    and the flowers and such. Shortly before 07:36 the owner added that in Minecraft the grass and
    the flowers are randomly positioned and rotated or so, which the source does by an offset and no
    turn; item 57.

28. **The blades like the modpack's, and the light and the grade from the shader settings the owner
    set.** The owner wrote shortly before 07:38 on 2026-10-21, asked how the blades should look,
    like in the modpack; and that comparing the colors the game's and the modpack's pictures differ,
    so the modpack is to be pulled and the shader settings they set fetched, the colors by time of
    day, the saturation and so on. The shader's license forbids redistributing or changing its code,
    so its settings and its formulas are read as numbers and the game's own written, item 58, the
    tonemap's curve asked of the engine; the blades' look comes with the light, the warmth and the
    deep shade being most of the difference. Shown three grades beside the modpack's picture shortly
    before 08:01, the owner chose Reinhard graded; and said the game's grass is tealish where the
    modpack's is green, which is the game's climate tint against the source's fixed colors a biome,
    item 59. Asked again on the green grass shortly before 08:16, the owner said it looks good but
    to check the modpack's saturation, and shown 1.05, 1.2 and 1.4 chose 1.4 as chosen; and said
    that for screenshots they zoom with alt to about 40 degrees, which the pictures' retake takes.

29. **The sun's shadows step on a grid of eight a block.** Shown a house and a tree's shadow at
    sixteen and at eight steps a block and at a reach of 64, before the commit of 08:46 on
    2026-10-21, the owner said, verbatim, "Pixelate 8 so a block has 8 shadow pixels", so the
    terrain shader takes each point's light on a grid of eight steps a block, half a texture's
    sixteen pixels, as the modpack's shader steps its shadows with the settings it is played with;
    the reach stays 96, since 64 dropped part of a near tree's shadow in the capture and the owner
    did not take it. Read on the owner's question of about 09:20 as the sun's light and shadow
    alone, the lamps' light and the global illumination smooth, item 60.

30. **The mobs take Bare Bones' entity pictures.** The owner wrote shortly after 09:40 on 2026-10-21
    to make the villager's face from Bare Bones, and the zombie's too, and the other mobs', so every
    mob wears the pack's entity picture over the source's cuboids, item 62, the pictures fetched
    with the pack as Decision 18 has it and the game's own colors kept where no pack is given, as
    Decision 22 keeps every rule the game's own.

## Replies



