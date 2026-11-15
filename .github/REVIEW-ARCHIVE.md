# Review archive

What REVIEW.md no longer carries, moved here at 19:30 on 2026-10-23 and after: the items of its Now
list that are done or dropped and the verdicts that are settled, each under its number as it stood,
for the voxel game. REVIEW.md holds the current work, this file the record; nothing here is read
before a batch.

## Items done

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
   and no figure of speech or casual paraphrase, with the pairs named and their replacements: the
   light that bounces becomes global illumination, radiance cascades where the technique is meant
   and indirect light where the effect is; gives off light and glowing become emissive; driving the
   game becomes controlling or scripting it through the command line, and a drive a scripted run.
   The colon joins it: STYLE.md's rule, which allows a colon before a list, tightens so that a
   running sentence holds none, the sentence recast or split where one appended an elaboration or a
   list, as in "makes the program the README opens with: a window with a turning cube"; a colon
   stands before a bulleted list, a code block or a command, and as a label in a table, and
   STYLE.md's check of colons joining clauses reads prose lines of Markdown as well as comments. The
   sweep covers README.md, AGENTS.md, every Markdown under `.github`, the comments of the code and
   the scripts, and the sentences the game prints, its commands' answers among them; 19 files hold
   the first pair 52 times, and a rough count finds a colon inside a sentence on some 30 prose lines
   of the documents. `build/drive.sh` becomes `build/play.sh`, new and cheap to rename, with
   `build.yml`, AGENTS.md and the README following. Checked by the strict build, the tests,
   `build/play.sh` and `git grep -i` for the pairs finding this file's history alone. One commit.

5. **The register widens to likenesses and figures of speech (Decision 5), README.md:3 and
   AGENTS.md:3 and 5, done at `4358f3b`.** The README opens with "A voxel game in the manner of
   Minecraft, and the ground floor of a larger one", and AGENTS.md's paragraph with the same
   likeness and with what the game grows toward; the opening was read on 2026-10-17 at 20:34 and the
   ask was that no phrasing of the kind stand. The game is said by what it is, an endless generated
   voxel world whose blocks are placed and broken, with its light, its water, its mobs and the items
   they drop, the other game named only where a number or a rule is taken from it, as the README's
   later lines do with its sizes, speeds and light levels, and no line says what the game is the
   start of; AGENTS.md changes by this decision (N 7.4); STYLE.md's Register section takes the rule
   with this pair; and `git grep -i` for the idioms Decision 5 names finds nothing, by hand in its
   technical sense apart. Checked by review. One commit.

6. **The sun, the moon and the stars fixed on the sky's sphere with textures, the moon with phases,
   the glow around the sun and the moon as particles parented to them (Decisions 6 and 7).**
   Reported on 2026-10-18 at 22:25, the sky's pixelated blobs standing for the stars, the moon and
   the sun look ugly. The sun and the moon become billboards facing the camera at their fixed places
   on the sky's turning, each with a texture drawn as pixel art, the moon's texture holding its
   eight phases, the count the other game has, shown by the day of the game's calendar; the stars
   become a particle effect parented to the sun's and the moon's fixed positions, so they turn with
   the sky, each star a camera-facing billboard of a few pixels fading in as the day fades, as
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
   to be asked of the engine until Decision 7 made it moot: nothing of the sky faces the camera. The
   second commit draws the sun, the moon and the stars as quads fixed on the sky's sphere, their
   right side and up the sphere's at their place, turned with the hour as one, the stars one mesh of
   500 quads with their alpha in the vertex color, sampled without filtering as before, and the halo
   around the sun as a quad fixed with the sun on the sun's own grid, its rings item 8's, the moon
   given a dimmer halo the same way, since a particle stands upright in the world and faces the
   camera, so as a particle the halo's rings would part from the sun's pixels by up to 45 degrees
   mid-morning and turn with the view, the fault Decision 7 names, which the game's coder measured
   at 21:09; the sun a bit smaller, as asked at 21:09, its width on the sky the other game's, whose
   quad is 60 across at a distance of 100 with the visible sun the middle 8 of its texture's 32
   pixels, so 15 across at 100, about 8.6 degrees, and the moon 10 at 100, about 5.7, where the
   game's were 9.0 and 6.6, the numbers said in the code's remark as taken from it, the reviewer's
   first figures having taken the quad's half-width for the whole; the README's sky picture taken
   anew; checked by a drive that turns and moves the camera while the bodies stand still on the sky,
   the captures retaken, and the stars below the horizon faded as before. Done at `6d71cd1` on
   2026-10-19, on `5.2.92`, held by a test of the quads' sides and the night sky's capture.

7. **The sun's shadows are pixelated (Decision 6).** Asked on 2026-10-18 at 22:25 that the shadows
   be pixelated too. The shadow map's texels are sized to the block grid, a texel a fraction of a
   block, and sampled without filtering, so a shadow's edge steps along the blocks' pixels rather
   than blurring; `voxel.shadows` keeps setting the distance. Where the engine's shadow settings
   offer no filter or texel size, the ask goes to the engine's ASKS.md (Decision 4) and the item
   waits on the package that brings it. Checked by a capture under a low sun in the drive and
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
   done at `344ef5b`.** Asked on 2026-10-18 at 22:25 that the sky around the sun be pixelated as the
   other game has it, the sky's gradient elsewhere smooth. The glow around the sun is drawn in the
   sky's pixel grid, the sun's billboard's pixel size, as a ring of pixel-art brightness that
   follows the sun, and the sky's gradient from horizon to zenith and its dawn and dusk colors stay
   smooth as `Sky.cs` paints them. Checked by captures at noon and at dusk in the drive and
   `build/play.sh`. One commit. Done at `344ef5b` on 2026-10-18, six rings of brightness on the
   sun's own pixel grid behind the sun, the painted sky the smooth gradient alone, a test and the
   noon and dusk captures holding it.

9. **The icons of the block and item types are made anew (Decision 6).** Asked on 2026-10-18 at
   22:25 that the icons for the different types of blocks and items be updated. The hotbar and the
   inventory draw a block as a colored square and an item by strokes (`Ui/Hud.cs:57-61`,
   `Ui/InventoryWindow.cs:256`); each block type gets an icon drawn from its own textures as a small
   cube seen from above and two sides, and each item type a pixel-art icon of its own, drawn once
   into an atlas the HUD and the inventory window read. Checked by a capture of the inventory open
   in the drive and `build/play.sh`. One commit. Read against the engine on 2026-10-18 at 23:21, the
   HUD can draw an atlas texture, and the inventory window cannot, since the engine's ImGui pass
   binds the font atlas's set once (`ImGuiRenderNode.cs:172`) and draws every command with it,
   ignoring the command's texture id, so an image of one of the game's textures draws the font
   atlas. The reviewer's call is one commit at once as items 6 and 7 went: `Ui/Icons.cs` draws every
   item's icon once into an atlas of 16-pixel cells with clear gutters, each block a small cube seen
   from above and two sides cast per pixel against the block's own boxes so slabs, stairs, torches,
   fences and doors keep their shapes, each face its surface's color under the plains tint with a
   fixed speckle for the texture, shaded top, left and right as the other game's inventory shades
   them, emissive faces unshaded, and each item that places no block pixel art of its own, the tools
   one shape a kind with the head in the tier's color; the atlas is a point-filtered texture the
   hotbar draws, and the inventory window draws the atlas's pixels as rectangles with each row's
   runs of one color merged, its cost measured with the creative window open; the strokes and the
   hotbar's emission ring go; an entry in the engine's ASKS.md asks for a texture id of a
   `Texture2D` that the ImGui pass binds per command, and a second commit moves the inventory onto
   the atlas texture when a package brings it. Checked by tests of the atlas, every item with an
   icon, a cube's three shades, a slab's clear rows and the clear gutters, a capture of the
   inventory open in `build/play.sh`, and the strict build. First commit done at `80ab301` on
   2026-10-18, the atlas of 97 icons drawn once, the hotbar drawing it as a texture and the
   inventory window as merged rectangles at half a millisecond a frame, the four tests and the
   capture; the engine's ASKS.md entry became its item 34, and the second commit, the window on the
   atlas texture, waits on the package that brings it. The second commit done at `1477e82` on
   2026-10-19, the window and the hotbar drawing the atlas as a texture through the engine's item
   34, the rectangles gone, the inventory capture pixel for pixel the same.

10. **Some of the animals' feet do not show (Decision 6).** Reported at 21:05 on 2026-10-19, some of
    the animals' feet do not show in the game. Read before it is mended, with a capture of each
    animal kind standing and walking on flat ground and on a slope: which animals, and whether the
    feet stand below the ground's surface by the model's origin or the mob's height above its block,
    are culled as faces turned away, are hidden by the pixelated shadow or the contact shadow of
    item 7, or are not in the model at all; then one commit for the cause found, with a test that
    holds each animal's lowest face at or above the ground it stands on, and the captures that show
    it. Read at 22:04 on `1477e82` as the head says, three candidates and no one cause: the toes
    dipping below the ground at a stride's ends, the mobs in shade almost black against lit grass,
    and one legless frame not reproduced. The shade finding was withdrawn at 22:13: the cameras that
    showed black animals beside lit grass sat inside the hill, the grass the sky through its missing
    faces, and measured on the flat world under a canopy of leaves at sky light level 11 a white
    sheep's wool reads 90 against 44 for the grass beside it and 155 against 169 in the open, so a
    mob in shade is lit as the grass or brighter through the engine's indirect light, and the
    terrain's light-level factor, 0.995 at level 11, would only darken the mobs in deep shade and
    caves, so the mobs' lighting stays. One mend goes ahead without the answer: each leg is lifted
    by however far its lowest corner would sink, so no foot goes below the ground in any stride,
    with item 13's models; the legless frame is hunted by the herd drive captured frame by frame,
    and if it comes again the frame goes to the engine's coder with the instance count and the view,
    since the engine draws instances in blocks of 64 and encloses each block for culling. The answer
    names which of the three was seen. The stride's mend and the test are in at `a31d456`; what
    stays open is the answer and the legless frame, hunted. The legless frame found and mended at
    `35696e2` on 2026-10-20, a stride worked out from nothing over nothing in the frame before the
    first drawing, not a number from then on, so the legs of the mobs placed before the first frame
    were drawn in no pass; the stride is left as it was in a frame that took no time, held by a
    test. Done; what was seen was this, the mobs near the spawn, and the stride's dip beside it.

11. **The README's picture shows the world's life, structures, a village and animals, and not
    terrain alone (Decision 6).** Reported at 21:23 on 2026-10-19, the README's picture shows
    terrain and a hill alone where the world is lively when entered, with structures, villages and
    animals. The picture at the README's head is taken where a village stands with its buildings in
    view and animals near, at the hour and heading that show them, with the render distance of item
    12, taken by the capture script from a seed and a place written in it so it is taken the same
    again, the sky's picture of item 6 beside it. One commit, with item 6's or after it. Done at
    `569b5fb` on 2026-10-19, the village and the hills side by side from a script with the seed, the
    place, the hour and the cloud drift written in.

12. **The render distance is read and set by what the frame affords (Decision 6).** Reported at
    21:23 on 2026-10-19, the render distance is very short. Read first: what the game's distance is
    in chunks and blocks, where it is set, whether the captures and ordinary play run at the same
    distance, and what bounds it, the frame time on the development machine's GPU and on lavapipe at
    each distance, the scene field's and the cascades' cost among it, and the memory; then the
    distance set to what the frame affords at the game's pace on the development machine, with the
    figures per distance in the commit's message and the setting's remark (N 3.5), and said in the
    README's running section. Read at 22:42: the distance is 8 columns of 16, 128 blocks, set at the
    game with a slider and a command clamping it to 2 to 16 and not kept by a save, so ordinary play
    and every capture run at 8; the shadows reach 96 blocks, the field's four cascades cover 128
    across, the haze runs from two thirds of the distance, and mobs draw within 64. On the
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
    own.** Decided at 22:03 on 2026-10-19, the game may use the other game's original sizes and
    models of the mobs, since they are numbers. For each mob kind the game has, the model's cuboids,
    their sizes, places and pivots, and the mob's height and width for its body are taken from the
    other game's model definitions as numbers, said in the remark as taken from it, the game's own
    pixel art mapped onto the cuboids' faces at the same pixel density; the walk, the head's look
    and a wing's flap keep their pivots; held by a test of each kind's sizes against the numbers.
    Read with item 10, since feet that stand below the ground may be a leg's length or a model's
    origin, and mended in one commit a kind or one for all where the change is the same shape, the
    captures of item 10 retaken on it. The game's body sizes are the other game's already, and most
    cuboids within a pixel of its, the pig's hind legs and head apart, so the commits are the
    differences kind by kind, with item 10's test folded in, no face below the ground standing and
    the dip within its bound mid-stride. After item 10's reading and before item 11. Done at
    `a31d456` on 2026-10-19, one commit for every kind, item 10's test in it.

14. **The game's tonemapping is the engine's somewhat boring curve (Decision 9).** Decided at 22:06
    on 2026-10-19, the game uses a somewhat boring tonemapping, which is the engine's
    `Tonemap.SomewhatBoring`, where the game draws with the engine's default curve. The game sets it
    at its light settings with the reason in the remark, every capture of the play script is taken
    again under it, and the README's pictures of items 6 and 11 are taken under it, so item 11's
    picture comes after this. Read at 22:17 on the development machine's GPU, the curve takes a
    luminance of 1.0 to 0.61 where the engine's own curve is unchanged to 0.9, so at exposure 1 the
    frame reads about 8 percent darker, the pillar under the morning sun 133.4 where 146.0 and the
    noon sky 126.5 where 136.4 in mean luminance, the sun's core 207 where 251, and at exposure 1.25
    the pillar 145.2 and the noon sky about 137. Answered at 22:36, 1.25 is taken if it gives the
    light values back, so the exposure is set to 1.25 beside the curve with the reason in the
    remark, the check being those two means within two levels of the engine curve's at exposure 1
    and the sun's core brighter than at 1, the night room's within one; the captures and the
    README's pictures are taken under both. Measured at 22:38 on the play script's captures,
    exposure 1.25 holds the day, the pillar 136.6 where 136.7 and the noon sky 137.8 where 136.4,
    the sun's core 219 where 207 under the curve at 1 and 251 under the engine's, and misses the
    night, the lamp-lit room 31.8 where 28.2, 13 percent brighter, since the curve is near a
    straight line in the dark and rolls the day's light off, so no one exposure meets both: 1.25
    gives the day back with nights a fifth brighter, 1 keeps the nights with the day 8 percent
    darker, and about 1.12 leaves both off by a few levels. 1.25 was chosen at 22:53. One commit,
    before item 11. Done at `bde1fc1` on 2026-10-19 at exposure 1.25 as chosen.

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
    README's costs section as before, the figure settled by decision where the reading leaves a
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
    default's figure, 12 or 10, is the owner's to choose, put up with the reviewer's word for 12 and
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
    open view, flying at 96 looking east over the forest to the sea, and the figure waits on that
    table. The flight and leak readings stand, from other places, and lavapipe's were from the
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
    flies as 12 does; 12 keeps 6.9 ms spare and 700 MB less. The figure, 12 or 14, is again a
    choice, put up with the reviewer's word for 12, the margin and the memory on machines slower
    than the development machine's; the tree holds 12 meanwhile. Lavapipe's GPU work from the open
    view with the marks is 0.49 s a frame at 4 columns, 0.82 at 8, 1.20 at 12 and 1.42 at 14, most
    of it the camera's pass. 12 was chosen at 10:36 on 2026-10-20, so the default is 12 columns, its
    remark giving the table and the margin, and the commit goes in. Done at `435b2de` on 2026-10-20,
    as the head says.

17. **Out of the water onto the shore, swimming while any of the body is in it.** The first point of
    2026-10-20 at 23:30, inside water one cannot jump out. The controller counts the player as
    swimming only while the body's middle is in water and Space then lifts, so once the middle
    clears the surface the body falls under full gravity, bobbing with its feet under, and a shore
    one block up needs more lift than that gives. The mend is the source's rule: swimming while any
    part of the body is in water, and a body in water pushing against a block's side with the space
    above free lifted at a climbing speed until it stands, so the shore is reached by walking into
    it; a test in the scripted run swims to a shore one block up and reads the player standing on
    it. One commit, first. Done at `5cd1521` on 2026-10-21, as the head says.

18. **The mobs' proportions toward the source's, the villager first, with some character kept.** The
    second point, the villagers are not the right proportions. Item 13 took the other game's cuboids
    as numbers, and the villager still reads wrong, so each mob's cuboids are read again against the
    source's model boxes at the same scale, the villager's head, nose, robe, arms folded and legs
    among them, each box's size and offset compared and put right where off, the game's own
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
    Two commits, the cutout and the faces, then the animation. Added shortly before 06:46 on
    2026-10-21, the lantern could take Bare Bones' texture to look better, a lantern's glass being a
    lamp's cube drawn white today under a cap and a chain in their surfaces' colors; so the first
    part also gives the lantern's, the hanging lantern's and the soul lantern's pieces a pack's
    lantern texture by the coordinates the source's lantern model gives each piece's faces, read as
    numbers as the mobs' boxes were, and the torches' flames and sticks the torch texture the same
    way, each piece keeping its color where no pack has one. After item 46. First commit done at
    `8f2a2c8` on 2026-10-21, the lanterns' and torches' pieces, as the head says; the cutout and the
    faces, then the animation follow after items 60 and 61. Second commit done at `b1cba53` on
    2026-10-23, the cutout and the furnace's faces, as the head says; the animation follows. Third
    commit done at `710fe0d` on 2026-10-23, the animation; item 39 done.

40. **F2 takes a screenshot, as the source does (Decision 19).** Decided shortly before 03:42 on
    2026-10-21, as in Minecraft, a screenshot is to be taken in the game with F2, the same for
    BevyCSharp, the captures put in the root or a subfolder. F2 writes the frame as a PNG into a
    `screenshots` folder beside the saves and the resourcepacks folder, made on first use, the file
    named by the time as the source names its own, `2026-10-21_03.45.12.png` and a count where two
    fall in one second, through the engine's capture, without the HUD's own message in the picture;
    a line on the HUD says the file was saved, as the source's chat does, and the README's controls
    gain the row; a transient run writes nowhere. When the engine's package brings its own F2 of
    item 73, the game takes that key and keeps its folder and its message. Tests: the name from a
    time and the count for a second, and the press through the command line leaving a file of the
    window's size in a scratch data folder. One commit, small, before item 25. The engine's
    performance window moves from F2 to F3 with its item 73, which is the game's own F3; when that
    package comes, the game turns the engine's window key off or gives it a free key through the
    engine's call, its own F3 window keeping the developer's parts. Done at `5064479` on 2026-10-21,
    as the head says, the engine's performance window hidden off screen until item 73 gives it a
    key.

41. **A pack's blockstate variants give a surface several tiles picked per block by its position,
    and Simple Grass Flowers is a fourth default pack (Decision 20).** Reported shortly before 03:53
    on 2026-10-21, Simple Grass Flowers looks great, its gallery linked, and beside flowers and
    grass, which the game lacks yet, it adds variations of the ground. The reading: the pack by
    2DWisp puts flowers, rocks and clovers on the tops of grass, podzol and mycelium through the
    source's block states, a block's state listing several models with weights, each naming its own
    top texture, and the source picking one per block by a hash of its position; its page says all
    rights reserved and allows adding it to modpacks with credit. The game reads textures by a table
    of its own and no block states, so it gains, for the surfaces the table names, a reader of a
    pack's block state variants and their models' textures, the surface then holding several tiles
    with weights in the atlas's free bits, the mesher picking one per block by a hash of its
    position as the source does, the same block the same tile every time, and the table's single
    tile where a pack lists no variants. Simple Grass Flowers joins the manifest as a fourth default
    pack, fetched from its maker's own Modrinth page as the three are, pinned by its version and
    hash, after Bare Bones in the list so Bare Bones' blocks stay and the flowers go on top, and
    credited in the README with its terms; nothing of it ships. Item 25's own flowers and its short
    and tall grass stay as blocks. Tests: a block state file of two weighted variants parsed to its
    tiles, the pick by position stable and within the weights over a field, and a capture of a
    meadow with the pack where the machine has it, skipped where not. After item 25 and before item
    39. One commit. Changed shortly before 05:00 on 2026-10-21 by decision on seeing the pack over
    Bare Bones, that it looks good but changes the way Bare Bones' grass top looks and the style
    does not fit, so it is to be done natively in the game's own style: Simple Grass Flowers leaves
    the manifest and the defaults are the three again; the game's own ground looks give the grass
    top its variants wherever no pack's block state lists any, flat little blossoms in a few colors,
    patches of clover darkened from the grass under them and tinted by the climate, and pebbles,
    each turned four ways and weighed near the pack's, about four in five plain, a twelfth clover
    and a few in a hundred flowers or stones, drawn as the game's own pixels in its flat style and
    lying over Bare Bones' grass top and the game's own alike; the block state reader stays, so a
    pack that lists looks still takes their place; the tests the own looks' weights over a field,
    the clover tinted and the decor untinted, and the plain look leaving the grass as it was. One
    commit as before. Under Decision 22 the block state reader goes too, the grass top taking the
    game's own looks alone over a pack's grass tile, the README saying a pack gives the pictures
    alone. Done at `236763d` on 2026-10-21, as the head says, the grass's tile never turning by
    decision.

44. **The land's fading edge meets a sky of the haze's color below the horizon.** Asked shortly
    before 04:02 on 2026-10-21, seeing the cherry grove's capture from the scripted run's walk, what
    could be done about the chunks' fading border contrasting with the dark lower half of the
    skybox, and whether distance fog would do it. The reading: the terrain shader's haze fades the
    land toward the sky's horizon color from 0.65 to 0.95 of the render distance, and past the last
    section the skybox's lower half shows, which the sky's equirectangular image paints dark since
    it lights the scene through the environment map too, so the fogged edge meets a darker sky than
    the color it fades into, most at small render distances; a longer fog alone would not mend it.
    The mend, as the source does it: the game draws a dome around the eye below the horizon in the
    haze's color after the skybox, blending into the sky over a few degrees above the horizon, so
    the edge fades into the color it meets, the image's darker lower half kept for the lighting
    alone; the dome's color following the haze's by the hour when item 43 gives the haze its tint.
    Tests: a capture's pixels read across the land's edge at the horizon with no step over a pixel,
    and the dome's color equal to the haze's. One commit, small, after item 25. Done at `cc46f25` on
    2026-10-21, as the head says.

45. **The look without a pack takes Bare Bones' colors (Decision 23).** Decided shortly before 05:10
    on 2026-10-21, the look without a resource pack can use Bare Bones' colors to match it at least,
    the game having different colors for them. The game's own flat colors, its surfaces' colors used
    where no pack is loaded, become Bare Bones' averages read as numbers from the player's copy as
    Terralith's colors were, nothing of its pixels shipping: `voxel.palette` prints each surface the
    table names with its texture's alpha-weighted sRGB average from the packs loaded, so the numbers
    can be read again; the surfaces take those numbers for the full blocks, the grass and the leaves
    as their gray before the climate's tint, the water as its gray under the table's tint, glass and
    ice keeping their alpha and the emissive blocks their glow; the plants keep their own masks and
    colors, since a flower's average mixes stem and petals. Tests: the field's colors unchanged in
    kind, and a capture with no pack beside one with Bare Bones showing the match. One commit,
    small, before item 39. Done at `8261308` on 2026-10-21, as the head says, the ores keeping their
    own colors.

47. **The haze dome no longer cuts the sun, the moon and their glows (Decision 25).** Asked shortly
    before 05:40 on 2026-10-21 whether the far plane or the skybox cut the sun's and the moon's
    flare off at the bottom. Neither: the far plane is at 4,000 and the bodies at 3,000 and 3,200,
    and item 44's dome at 2,400 is drawn in the model pass writing depth over its whole span to its
    top ring six degrees up, so the bodies drawn after it fail behind it even where its alpha is
    zero, measured with a low sun over the sea cut with the dome and whole with the fog off; the
    engine's material has no depth write switch in 5.2.190. The mend: the dome moved beyond the
    bodies, between the stars' 3,200 and the far plane, at 3,600, and each body's quad a grid whose
    vertices take the dome's clear share at their height, so the haze still covers a sinking sun as
    blending the dome over it would and nothing is cut; the test of item 44 kept and one of a low
    sun's glow whole across the horizon's six degrees. One commit, small, before item 46's final
    captures. Done at `f7df279` on 2026-10-21, as the head says.

48. **No tunnel mouth and no ravine under a village (Decision 25).** Reported shortly before 05:40
    on 2026-10-21, a lot of rifts in villages, to be made rarer since they make the structure buggy
    and villagers could fall. The reading: the rifts are tunnel mouths, where a patch of high ground
    opens to the tunnels, and ravines, both carved before the village builds its roads and footings
    at the ground's height, so a footing or a road hangs over the pit, seen at three plains villages
    and a desert one. The mend: no mouth and no ravine within a village's square of 48 and a margin
    of a few blocks, the generator and the ravines asking for the villages' places, which depend on
    the seed and the ground alone; tunnels deeper than three blocks under the ground stay, so a
    village's ground is whole. A test generating the columns of seed 1's first villages and finding
    no open cell under a road or a footing. One commit, before item 46's final captures. Done at
    `0c93d3e` on 2026-10-21, as the head says.

49. **The sun and the moon in the source's default look, square with a dithered glow, and a pack's
    own where it has one (Decision 25).** Reported shortly before 05:40 on 2026-10-21, the sun and
    the flares fall short, the source's default dithered pixelated square one looking better. So the
    sun and the moon drop the game's six rings and take the source's default look, a square body
    with a glow dithered in its own pixels around it, drawn by the game in that style as its own
    pixels, since the source's own picture cannot ship; where a pack has an environment sun and moon
    phases picture, the game draws those, as pictures alone under Decision 22, the moon's phase by
    the day; the dusk's color fan of the sky's gradient stays. A capture of the sun at noon and low
    and the moon at night in the record. One commit, small, with item 47 before item 46's final
    captures. Done at `f7df279` on 2026-10-21, as the head says.

50. **The block's texture over the whole window while the eye is inside a block, as the source draws
    it (Decision 25).** Reported shortly before 05:42 on 2026-10-21, the camera lands inside blocks
    in some captures, and when it does the game is to show the block's texture on the whole screen,
    and the script is to check before a capture whether the eye is inside a block and get out first
    or find a better spot; in the scouting, eye-level spots chosen from the ground landed inside
    dark oak crowns and logs and the frame read black or showed the faces' insides. While the eye's
    cell holds a block that blocks the view, a whole opaque cube, every mode but spectator draws
    that block's side texture over the whole window under the HUD, the pack's or the surface's own
    color, darkened to a tenth, repeated four times across and down and slid by the yaw and the
    pitch over 64 as the source's screen effect draws it; leaves left out as the source leaves them,
    and glass, water and the shaped blocks. A test of the overlay's block for an eye inside stone,
    none inside air, leaves or glass, and none in spectator. Item 46's script checks the eye before
    each capture, `voxel.state` saying the eye is inside a block when its cell holds anything but
    air, the script moving the eye up a block at a time to eight until its cell is air or failing
    with the view's name, and the views chosen where the eye is in the open. One commit, small,
    after item 48 and before item 46's final captures. Done at `a96cea5` on 2026-10-21, as the head
    says.

51. **The sun's and the moon's glow smooth as Bare Bones' is, in the game's own art too (Decision
    25).** Decided shortly before 05:55 on 2026-10-21, on seeing item 49, Bare Bones' one is
    preferred and is to be implemented in the native look without resources too, being a smoothing
    of the dithering, the flares of the sun and the moon. The reading of the player's copy as
    numbers, nothing copied: a 128-pixel picture, the same three-ring square body at four texels to
    each of the source's pixels, opaque and added to the sky, with a smooth glow from about a tenth
    of its color beside the body falling nearly linearly to nothing fifteen of the body's pixels
    from the middle. So the game paints the sun and the moon at four texels to a pixel, 128 across,
    the body's pixels as four by four blocks so it stays pixel art, the glow smooth with no
    dithering or levels from about a tenth beside the body falling linearly to nothing at fifteen
    pixels, the moon's as bright as its phase is lit; the tests swap the dither's check for a smooth
    fall, never rising outward and never stepping by more than a few levels between neighbors; the
    sky's grid stays one quad to each of the 32 pixels. One commit, small, after item 48. Done at
    `e4483af` on 2026-10-21, as the head says.

52. **The stars as plain squares, the game's layout and the source's compared for the choice
    (Decision 25).** Asked shortly before 06:02 on 2026-10-21, squares in place of the star shape
    and a try of the source's star layout with its values, comparing both, the values as they stand
    and the source's. The cross goes and every star is a plain square. Two layouts are taken from
    the same night view at the same hour, moon and heading and shown side by side: the game's as
    now, 500 stars of 0.16 degrees at a brightness of 0.45 to 1 with their sides along the sky's
    turn; and the source's, read from its client as numbers with nothing copied, 1,500 stars each
    placed by a point in the unit cube kept where its squared length lies between 0.01 and 1 and
    pushed out to the sphere, a square of 0.17 to 0.29 degrees across turned by a random angle about
    its own axis, white at the sky's star brightness. One is chosen, and the chosen layout is
    committed with a test of the count, the sizes and the even spread over the sky. One commit,
    small, after item 50 and before item 46's captures, since a night picture shows the stars. On
    the pair shown shortly before 06:10 on 2026-10-21, the night sky in the modpack was found to
    look different; the modpack's shader draws its stars per pixel, the sky's direction projected
    onto a plane from the moon's place, scaled and snapped to a grid so a star is a square of about
    0.28 degrees at the zenith, a cell a star where three hash noises multiplied pass 0.7, about
    0.58 percent of cells, its light the excess squared times a cool white, faded over the lowest 20
    degrees and hidden near the sun and the moon; the shader's license forbids redistributing or
    changing its code, so it was read as numbers and the game's own written, a third layout of 3,000
    stars spread evenly over the turning sky, squares of 0.28 degrees, each drawn until its product
    passes 0.7, the light taken to sRGB, with the fade and the hiding. Of the three shown side by
    side, the modpack's shader's was chosen, so the game keeps it alone, the cross, the star texture
    and the comparison switch gone, the test covering the count, the size, an even spread on each
    side of the three axes, the middle star faint and the brightest bright and cool white; the
    shader's bloom on its brightest stars and its stars sliding with the moon rather than turning
    with the sky left out. Done at `f7bcf26` on 2026-10-21, as the head says.

53. **The bloom at a tenth (Decision 25).** Reported shortly before 06:26 on 2026-10-21, there is
    too much bloom, and of the lamps' room shown at night at 0.5, the game's default, 0.25, 0.1 and
    0, 0.1 was chosen. The default goes from 0.5 to 0.1 with the reason in its comment, the
    settings' slider still reaching 2. One line, committed alone after its checks and before item
    46's captures, which then show it. Done at `6845654` on 2026-10-21, as the head says.

56. **More plants, the source's counts read and the modpack's look matched (Decision 27).** Reported
    shortly before 07:12 on 2026-10-21, there is way too little vegetation. The grasses stand at 140
    a thousand places in the plains, 180 in the savanna, 80 in the jungle, 60 in the birch forest,
    50 in the forest, 40 in the swamp and the cherry grove, 20 in the dark forest and 10 in the
    mountains, the ferns 40 in the taiga and the jungle, the flowers 14 in the plains and 8 in the
    woods. Read first the source's own counts for 26.3, its placed features' grass, tall grass and
    flower tries a chunk, from the client's data as the mobs' boxes were read; measure the game's
    and the source's covered share of a chunk's grass top; and raise the game's toward the modpack's
    look, the biome overhaul's numbers read as numbers, each biome's grasses, tall grass, ferns and
    flowers. A test counting short grass, ferns and flowers in a 12 by 12 columns plains of seed 1
    against the new share, and the meadow's capture taken again beside the modpack's picture. One
    commit, after item 55. Done at `d32ee21` on 2026-10-21, as the head says; the blades reading
    flat at a distance beside the modpack's is left for a decision.

57. **The plants stand offset in their block as the source places them (Decision 27).** Reported
    shortly before 07:36 on 2026-10-21, in Minecraft the grass and the flowers are not placed
    uniformly on the block but randomly positioned and rotated or so. The source offsets them and
    does not turn them, read from the client's code as numbers: from a seed of the place, up to a
    quarter block either way in 16 steps along x and z, and the short grass and the ferns sinking up
    to a fifth of a block too, the flowers, the saplings and the tall grass only sideways. The
    mesher offsets each plant's walls by the place's hash the same way, the grass and the ferns
    sinking, the petals, the wheat, the mushrooms and the dead bushes keeping their place as in the
    source; a wall carries its own texture coordinates, the mesher numbering its corners from the
    lower left with the vertex stage handing the fragment zero to one across and down, since the
    shader laid a wall's texture by its world position, which would slide under an offset, the light
    staying snapped to the block's pixels; the README's plants paragraph. Tests: the same place
    giving the same offset and two places differing, every offset within a quarter, the grass
    sinking no more than a fifth and the flowers not at all, and the walls a block wide corner to
    corner. One commit, small, before item 39. Done at `815e4ca` on 2026-10-21, as the head says.

58. **The light and the grade after the modpack's shader settings it is played with, read as numbers
    (Decision 28).** Decided shortly before 07:38 on 2026-10-21, the blades should look like the
    modpack's, and comparing the colors the game's and the modpack's pictures differ, so the modpack
    is to be pulled and the shader settings it is played with fetched, the colors by time of day,
    the saturation and so on. The modpack's shader runs under a license forbidding redistributing or
    changing its code, so it is read as numbers and the game's own written: the reference modpack's
    settings against the shader's defaults, the exposure 0.90 against 1.40, the white curve 1.3
    against 2.0, the lower curve 1.05 against 1.20 and the upper 1.00 against 1.50, the saturation
    1.05 against 1.55, the ambient 50 against 150, the fog 0.95 against 0.50, the night tint 1.10,
    0.90 and 1.30, the bloom 0.027, the shadows stepped at 8 a block and reaching 64, no color
    grading; the shader's tonemap multiplying by the exposure, dividing by the white curve's term, a
    power from the lower curve in the darks to the upper in the lights mixed by the root, gamma and
    the saturation about the channels' mean; its sunlight at noon a warm yellow, orange at sunset by
    a power past noon, dim and cool at night, the ambient the sky's color to the 0.65 times 0.85
    scaled by the ambient setting, where the game's sun is near white, the warmth and the deep shade
    being most of the pictures' difference. Five parts, each shown beside the modpack's pictures
    before its commit: the sun's color by hour after the shader's numbers and the sky's light at the
    share the saved ambient gives, measured as the shade-to-sun ratio on a white wall before and
    after, which takes item 43's sun from it; the night sky and the moonlight tinted; the grade, the
    saturation through the engine's color grading and the curve asked of the engine in its ASKS.md,
    a parametric curve set by the four numbers or a tonemap through a program's shader, the engine's
    nearest curve and exposure chosen meanwhile by the curves' values at a few levels and stated;
    the haze's start and strength read from the fog setting and matched, which item 43's haze
    follows; and the shadows stepped at 8 a block and reaching 64, offered rather than taken. The
    bloom stays at the chosen tenth. Tests on the sun's color at four hours, the ratio, the tint and
    the haze. Three or four commits, after item 57 and before item 39. The ambient's share, read at
    08:00, cannot be set apart from the drawn sky with the engine's environment map, so it is asked
    of the engine as its item 81 and the game takes the share when that package comes; the grade
    chosen at 08:00 is Reinhard at an exposure of 1.7 with the saturation and contrast graded,
    checked again on the green grass of item 59. Part a done at `82da862` and part c at `396047d` on
    2026-10-21, the saturation 1.4 by the second decision, the two entries in the engine's ASKS.md
    for its items 80 and 81; part b done at `293a5d1` and parts d and e at `b56fa0b` on 2026-10-21,
    the shadows' grid eight a block by decision, Decision 29; the ambient's share waits on the
    engine's item 81 as said. Done at `b56fa0b`. The curve at `2b265f3` on 2026-10-24,
    SetTonemapCurve(0.9, 1.3, 1.05, 1) with the pre-exposure at 1.6 for the middle levels, contrast
    1.12 and saturation 1.4 kept until a choice is made from the pictures, 1.2 near the look
    approved.

59. **Each biome's grass and foliage color from the source's fixed values, not a blend of the game's
    own climate (Decision 28).** Reported shortly before 08:01 on 2026-10-21, the game's grass is
    not the same as the modpack's, which is green where the game's is tealish. Measured, the
    modpack's sunlit grass top reads 127, 139 and 31 and the game's 140, 157 and 97 graded; the
    cause is the game's tint, which blends the colormap's corners by its own temperature and
    humidity noise, the plains at 0.38 and 0.41 giving 137, 184 and 116 with blue in it, where the
    source gives each biome one color from its fixed temperature and downfall, the plains' 0.8 and
    0.4 giving 145, 189 and 89; lowering and whitening the sky's light alone took the blue only from
    97 to the seventies. So each biome's grass and foliage color is the colormap's value at that
    biome's own temperature and downfall as the source's biome files give them, read as numbers, the
    plains, forest, birch forest, taiga, snowy, savanna, jungle, windswept hills and river, blended
    across biome edges by weight as the special biomes already blend, the dark forest, swamp,
    badlands and cherry grove keeping their own; the continuous climate colormap goes. A test reads
    each biome's middle at its color within a level or two. Then the grade is checked again by eye
    on the green grass. One commit, before item 58's first commit. Done at `fa7fba0` on 2026-10-21,
    as the head says.

60. **The lamps' light falls off smoothly, the eight-a-block grid taking the sun's light and shadow
    alone.** Asked at about 09:20 on 2026-10-21, why the torches' falloff is smooth and the
    lanterns' pixelated. The cause, confirmed in a live session: the terrain shader takes all of a
    face's light at the middle of its step on the grid of Decision 29, the sun's light and shadow,
    the lamps' light and the global illumination alike; a lantern's lit glass stands on the floor,
    so its light on the floor falls steeply over the first half block and the eighth-block steps
    show as squares, where a torch's flame of 2 by 7 to 10 pixels lights the floor slowly and the
    steps do not show; at 64 steps a block the pools come out round. The mend, Decision 29 read as
    the sun's shadows alone as its words say: the grid stays for the sun's light and shadow and the
    rest of the light is taken at the point itself, as the mob shader already takes the sun apart
    from the rest, one more shadow lookup a pixel, its cost measured on the machine at the usual
    view and in the commit's description; a test reading a lantern's pool on the floor in a night
    capture for a smooth fall across a block, and the README's shadows paragraph saying the grid is
    the sun's. One commit, small, after item 39's first commit and before its second part. Done at
    `9a13c54` on 2026-10-21, as the head says, the cost unreadable within the sessions' swing.

61. **A mob's side faces take no bands from their own shadow at noon.** Reported at about 09:20 on
    2026-10-21, the capture with the mobs showed pixelated layer lines on their bodies, which should
    not happen. The cause, confirmed in a live session: the mob shader floors each face's light at
    the flat shade of the light a face turned up takes at the same point, which looks up the sun's
    shadow at a point on a side face with an upward normal, so the shadow map's offset runs along
    the face instead of out of it and the face shadows itself in horizontal bands wherever the floor
    outweighs its own light, every side face at noon and none at 16:00 with the sun on them; with
    the turned-up light taken a sixteenth of a block out from the face along its own normal the
    bands are gone. The mend: that line in mob.slang, a test reading a mob's side face in a noon
    capture for alternating rows and finding none, and mobs/daylight.webp taken again. One commit,
    small, after item 60. Done at `92112c5` on 2026-10-21, as the head says.

62. **Every mob wears the pack's entity picture (Decision 30).** Asked shortly after 09:40 on
    2026-10-21, that the villager's face come from Bare Bones, the zombie's too and the other mobs',
    read as every mob taking the pack's entity texture. Bare Bones holds pictures for all nine
    kinds, the villager with its biome type and profession overlays, the zombie, the skeleton, the
    creeper, the spider, the pig, the cow, the sheep with its fur and the chicken, fetched with the
    pack as Decision 18 has it, nothing shipped. The mobs' boxes are already the source's cuboids
    taken as numbers, colored a face with no texture, the faces extra small boxes. The mend: each
    cuboid takes its texture offset from the source's model definitions, read as numbers as the
    boxes were, and the source's box unwrap, the top and bottom at the offset moved right by the
    depth, then the right, front, left and back faces in the row below, a mirrored box flipped; one
    material a kind with the pack's picture, the mob shader sampling it in place of the vertex
    color; the face boxes left out where a pack gives the picture and kept with the colors where
    none does; the villager's biome type drawn over its base picture and a profession where the game
    has one; the sheep's fur as a second box set tinted by the wool's color; each mob's own colors
    kept with no pack. Tests: a cuboid's face coordinates against the source's numbers, a face's
    pixels read in a capture, and mobs/daylight.webp and the pair sheets taken again. Two commits,
    the unwrap with the shader and the material, then the villager's layers and the sheep's fur.
    After items 60 and 61 and before item 39's remaining parts. First commit done at `e4b850f` on
    2026-10-21, eight kinds wearing the pictures, as the head says; the villager's layers and the
    sheep's fur follow, with the daylight row in Bare Bones beside the own one. Second commit done
    at `9ce44d7` on 2026-10-21, the villager's biome layers as one picture a biome, no profession
    layer since the game has none, and the sheep's tinted wool; item 62 done. A fault found at 10:31
    while item 65's layouts were checked: with Bare Bones the pig and the cow stood in their own
    colors, since Bare Bones 1.21's pictures of them are 64 by 32 where the 26.3 client's layers lay
    their cuboids on 64 by 64, the source having doubled the two pictures at 1.21.5 and given the
    cow a muzzle cuboid at row 33, so the fit refused both and the captures' pink and brown were
    read as the pack's; the older layout matches the newer in every offset but the muzzle. The mend,
    a small commit before item 65: a picture fits when it is as tall as its layout or half as tall
    at a whole scale, the texture coordinates taken over the picture's own rows and one white row, a
    cuboid all of whose faces lie below the picture left out as the clear-face rule already leaves
    it, so a cow on an older picture loses its muzzle alone, as the source's cow had none before
    1.21.5; tests of a 64 by 32 cow fitting the layout, its head laying the head and horns but not
    the muzzle, its coordinates over 33 rows, and mobs/daylight-pack.webp taken again. Mended at
    `d2bc3f0` on 2026-10-21, as the head says.

63. **A mob's face is lit evenly from top to bottom, the turned-up light seen from straight above.**
    Reported shortly before 09:50 on 2026-10-21 of item 61's noon picture, in the images of the
    three mobs an entity gets darker above a certain height, about the height above the zombie's
    lips. The cause, confirmed live and older than `92112c5`: down the zombie's face at noon the
    floor reads 134.7 above a height and 150.3 below it, an even 134.7 with the floor removed, the
    boundary at the eye's height of 5.72 wherever the eye stands and whatever the sample's distance
    out, the whole face 150 with the eye raised 0.3 blocks, since the engine's surfaceNormal turns a
    normal pointing away from the eye over as for the back of a double-sided face, so the turned-up
    light taken above the eye's height is lit as a face turned down and the floor falls under the
    face's own light there. The mend: one line in mob.slang, the turned-up light's view direction
    straight down from above, the face reading an even 150.3 to 150.7 from the same eye height; a
    play-script check reading down a zombie's face at noon from its own height asking no step over 2
    levels outside the mouth's rows, which fails today with a step of 15.6. One commit, small,
    before item 64. Done at `e6a2c55` on 2026-10-21, as the head says.

64. **The zombie's own face is the source's archetype and the villager's eyes sit lower (Decision
    31).** Asked shortly before 09:50 on 2026-10-21, that the zombie's default own face take the
    same archetype as the source's default, with horizontal lines and a nose but no mouth, and that
    the villager's eyes sit a little lower. The game's own art, worn where no pack gives a picture:
    the zombie's eyes as horizontal dark lines, a nose and no mouth, as the source's default zombie
    face is read, and the villager's eyes a little lower; the pair sheets of the two taken again.
    One commit, small, after item 63 and before item 62, so item 62 keeps these faces for a game
    with no pack. Done at `69ecb49` on 2026-10-21, as the head says.

65. **The Bare Bones x Fresh Animations pack as a fourth default pack above Bare Bones, so the mobs
    wear its pictures (Decision 32).** Reported shortly before 10:15 on 2026-10-21, a mod in the
    modpack replaces the creeper skin for Bare Bones, to be checked. Read in the modpack: its pack
    order from lowest to highest is Bare Bones 1.21, Fresh Animations 1.9.2 and Bare Bones x Fresh
    Animations 1.9, so the creeper seen in play is the last pack's, Bare Bones' creeper with a
    darker face plus Fresh Animations' pieces in parts of the picture the source's cuboids never
    read, 64 by 32 in the vanilla layout; Entity Model Features draws Fresh Animations' animated
    models, which the game does not draw, and Creeper Overhaul gives each biome its own creeper in
    its own style and namespace, which no pack restyles and which stays out. On Modrinth the pack is
    bare-bones-x-fresh-animations by TeamHibiscus under the Unlicense. The mend, after item 62: the
    pack as a fourth default fetched as the three are under Decision 18 with its version and SHA-512
    in resources/packs.json, listed above Bare Bones so its mob pictures win as in the modpack's
    order, the game reading a picture from the first pack that has one; first each of the nine
    kinds' pictures in it checked for the vanilla layout as the creeper's is, a kind whose picture
    departs from it kept on Bare Bones' and named in the commit, and a capture of the mobs with and
    without the pack beside the reading. One commit, small, after item 62's second commit and before
    item 39's remaining parts. Read at 10:31: Fresh Animations 1.9.2 is the release whose creeper
    and cow are byte for byte the modpack's 1.9 beta, its pig and cow 64 by 32 as Bare Bones 1.21's,
    its sheep, chicken, villager, creeper and spider covering every face the cuboids read as Bare
    Bones' do, and it has no zombie or skeleton, which stay on Bare Bones'; after item 62's mend of
    the half-height fit. Dropped at 10:45 on the check: the release 1.9.2, whose creeper and cow are
    byte for byte the modpack's beta, compared pixel by pixel with Bare Bones 1.21 over every face
    the cuboids read gives the chicken and the spider alike on every face, and differs on the pig's,
    cow's, sheep's and villager's head fronts and the villager's nose alone, the differences being
    the eyes, which Fresh Animations takes off the face onto pieces of its animated model elsewhere
    on the picture, and a clear hole where the cow's and sheep's muzzles meet the face; captures of
    the six with and without the pack show the villagers and the pig without eyes and the rest
    unchanged, the creeper keeping its own colors by item 66 and the zombie and skeleton absent from
    the release. So by the item's rule four kinds stay on Bare Bones' and two take pictures
    identical to Bare Bones' on every face read, the pack changing nothing the game shows at the
    cost of a fourth fetch, and nothing is committed, the working tree's edits reverted with the
    fetched copy kept outside git.

66. **The creeper keeps the game's own vanilla-style art with any pack (Decision 33).** Decided
    shortly before 10:38 on 2026-10-21, the creeper's texture in Bare Bones is not taken and the
    vanilla one is kept. The repository cannot hold the source's own creeper picture, and the game's
    own creeper is drawn after it with its patches and its face, so the creeper is taken out of the
    pack table with the reason in its comment and wears the game's own art whatever packs are
    loaded; the play script's picture check moves to the zombie, whose head at 0, 0 and body at 16,
    16 take the same offsets on one 64 by 32 picture that the half-height rule lays on its layout;
    the README's sentence on which kinds wear pictures, and mobs/daylight-pack.webp taken again. One
    commit, small, before item 65, whose layout check then leaves the creeper out. Done at `f45b75e`
    on 2026-10-21, as the head says.

19. **Four game modes and the switcher, F3 with F4 opening the row of icons in boxes, F4 stepping,
    spectator among them.** Reported, F3 with F4 does not switch modes, spectator is not there, and
    the small panel of boxes with icons is missing. The game has creative and survival; it gains
    adventure, no placing or breaking but interaction, and spectator, flying through blocks with no
    body, no collision and no inventory, which takes the engine's character flight of 5.2.190 where
    it fits; F3 held with F4 shows the switcher, four boxes with icons and the mode's name, F4
    stepping through them while held and the choice taken on release, as the source does; the mode
    in the status and saved. One commit. Done at `d9c2f8d` on 2026-10-21, as the head says,
    spectator flying by the game's own controller.

20. **The inventory's quick moves as the source has them.** Reported, the quick inventory control
    with shift and alt does not work in survival. The inventory window takes left and right clicks
    alone; it gains shift-click moving a stack between the hotbar and the inventory or into the open
    container, number keys over a slot swapping it with the hotbar's, double-click gathering a kind,
    drag over slots spreading a stack, and drop by Q with control for the stack, the keys read where
    the window is open in survival and creative alike. A sort of the inventory or the open container
    by a key and by a middle click on its panel, as the sorting mods of the modpack do (Decision
    14), kinds together and stacks filled. One commit. Done at `9bd58e3` on 2026-10-21, as the head
    says, the furnace and the grids being the open containers the game has.

21. **Stairs join at corners, place by the hit, and the aim picks the shape and not its cell.**
    Reported, stairs do not connect on a corner, placing is odd, and the hover outline is the block
    and not the mesh, so the aim picks the one in front. The stairs' shape reads its neighbors for
    the inner and outer corner forms as the source does, placing reads the hit's half and the
    player's facing for upside-down and direction, the ray tests each piece of a shape and not the
    box of all, so a ray through a stair's empty quarter reaches the block behind, and the outline
    draws the shape's pieces; the same for slabs, fences and every shape of pieces. Two commits, the
    corners and the placing, then the ray and the outline. Done at `baf26b9` and `5b2f9d3` on
    2026-10-21, as the head says, the placing found right already.

22. **The baked ambient occlusion has a strength.** Reported, the AO baked into the mesh needs a
    strength parameter. The mesher bakes four fixed levels; it takes a strength from zero to one
    that scales the levels toward one, set in the settings and by a command, the meshes built again
    when it changes, the default the levels of today. One commit. Done at `bf13ff6` on 2026-10-21,
    as the head says.

23. **The interface drawn as the game's own, in the source's style, ImGui kept for the debug
    windows.** Reported, the UI is very ImGui-styled. The hotbar and HUD are drawn shapes already;
    the inventory, the creative items, the settings and the pause screen become the game's own 2D
    drawing, panels of nine-slice textures, slots with item icons drawn from the block's faces,
    tooltips and a cursor stack, in the source's look with the game's own touches, the keys of item
    20 kept, ImGui left for the debug and settings' developer parts. The look is Bare Bones'
    (Decision 13), flat shapes, few lines and plain colors, drawn as the game's own art, and where a
    pack of item 24 is loaded its gui textures, the hotbar, the hearts, the slots and the panels,
    are drawn in place of the game's. The HUD's hearts, hunger and air fade when full and unchanged
    and return on a change, as the modpack's HUD does, a setting on by default (Decision 14). Two or
    three commits, the panels and slots first, then each window. Done at `8722725`, `8b1d584` and
    `4c0c50e` on 2026-10-21, the art and the HUD, the windows, then the menu and the options, as the
    heads say.

24. **Texture packs read from the player's own resource packs, shipping no one's textures.** The
    biggest point, texture pack support, with a mod called Shine in the reference folder as what
    adds a lot. Shine is a Fabric mod of code and shaders under all rights reserved, so none of it
    may enter the game, only what it shows done, which is item 28; the packs the player's own
    instance holds are the source's resource packs, zips or folders of block, item and entity
    textures by name under the source's paths. The game gains a pack loader reading such a pack from
    a folder beside the game or a chosen path, mapping each block's faces to the pack's texture
    names by a table the game keeps, falling back to its own texture where a pack lacks one, the
    atlas built from the pack at load with mipmaps and the pack's resolution, a setting naming the
    pack and a command to reload, so a player's pack shows and the repository ships only the game's
    own art. The loader reads the pack's item textures for the hotbar's and the inventory's icons
    and its gui textures for item 23's interface as well as its blocks, so Bare Bones shows whole
    (Decision 13); a pack is a zip or a folder, and a list of packs in order as the source layers
    them, the later over the earlier, so Bare Bones with its add-ons reads as it does in the
    player's instance. The list is the source's plain order, the first pack over the rest, which
    gives the chosen order as the packs are, Bare Bones first and Recolourful Containers second
    (Decision 17); the windows and the HUD are drawn from a pack's gui textures by the source's
    paths and layouts, the container pictures of 176 by 166, the HUD's sprites of 9 and the widgets'
    nine-slices by their metadata, where a pack has them. Entity textures and models follow in item
    30. Three commits, the loader and the atlas, then the windows and the HUD from a pack's gui
    textures, then the setting, the list and the reload with a resourcepacks folder beside the saves
    as the source keeps one; the game's own art stays as it is, the packs' own look having been
    asked for instead once it was seen moved toward the two looks, shortly before 03:23 on
    2026-10-21 (Decision 17). First commit done at `193da5e` on 2026-10-21, the loader and the
    atlas, the second at `2839dba`, the windows, the HUD and the font from a pack, and the third at
    `056bb7b`, the setting, the list and the reload, as the heads say. A fourth commit by Decision
    18: with no list written and the run not transient, the game fetches the three packs named from
    their creators' own Modrinth pages into its resourcepacks folder on a background task, each file
    pinned by its Modrinth version, its file URL and its sha512 in a manifest the repository holds
    as a modpack's manifest does, checks each hash, writes the list in the chosen order and reloads
    when all three are in, the first frames drawn with the game's own art meanwhile; offline or on a
    failed check it plays with its own art and tries again at the next start; the README says the
    game fetches them at first start and names the three and their terms, the Resource Packs screen
    shows the fetch's state with a switch to stop it, and the request carries a user agent naming
    the game as Modrinth's API asks. Tests: the manifest's hashes read and a file of the wrong hash
    refused, with the fetch itself skipped in the suite and in CI. Done at `e14134d` on 2026-10-21
    with the fourth commit, after `c5df182` took the windows' titles off, as the head says.

25. **More biomes, more blocks and more trees.** Asked three times. The climate has eight biomes,
    the features three trees and the cactus, and the blocks about 140 ids: biomes gain the swamp,
    the savanna, the jungle, the dark forest, the badlands, the beach and the river, each with its
    surface, its colors of grass and leaves and its features; blocks gain each biome's wood, leaves,
    sapling, flowers, tall grass, mushrooms, sand and gravel kinds, clay, ores by depth and the
    glass and wool colors; trees gain the jungle, the acacia, the dark oak, the big oak and the
    giant mushroom, from the source's shapes as numbers with the game's own touches. The climate
    ranges, the surface rules and the feature counts of the seven biomes are read for their numbers
    from the biome overhaul's JSON in the modpack as from the source's, a model and not a copy,
    since its license forbids redistribution (Decision 14). Three commits, biomes, blocks, trees,
    each with the scripted run's walk through one of each and a capture. The badlands with its
    terracotta bands around turquoise pools and a cherry grove join the biomes, and a lantern and
    the flowers the look shows join the blocks (Decision 21). First commit done at `a6ec820` on
    2026-10-21, the eight biomes with their land, colors and new blocks, the second at `723e28c`,
    the seventy blocks, the plants as planes and the ores by depth, and the third at `c54f362`, the
    nine trees with the cherry's lanterns, as the heads say; item 25 done.

71. **The stars are points of light with no dark backing, faded by the sky's light.** Reported on
    2026-10-23 (Decision 34), the stars are visible at dusk and dawn with half of them black, and at
    night some black or dark. The cause to read: a star drawn as a quad whose dark corners or
    backing write over the sky, or blended with the wrong operator. The mend: stars drawn additive
    or with an alpha that is zero off the point, their brightness scaled by the sky's darkness so
    none shows by day and all show at night; a test reading the sky around a star for no pixel
    darker than the sky at dusk. One commit, small. Done at `fd276d3` on 2026-10-23, additive stars,
    as the head says.

76. **The stepped shadows move in whole steps as the day passes.** Reported on 2026-10-23 (Decision
    34), when timing the day the pixelated shadows flicker, unable to choose between staying and
    moving to their next place. The cause: the sun's direction moves every frame, so a shadow edge
    near a step's boundary falls on either side from frame to frame, the engine's texel snapping
    holding the map against the camera's motion and not against the light's. The mend: the shadow's
    sun direction steps on a coarse clock, every few game minutes, while the sky's sun moves
    smoothly, so an edge jumps a step at once and stands still between, the step chosen so no jump
    is longer than a step at the shadow's reach; a test reading a shadow edge still across the
    frames between two steps. One commit, small. Done at `7a0d61d` on 2026-10-24, 36 steps an hour
    rounded to the nearest, the departures in the head.

77. **The distance fog takes the sky's color at the ray's height, so a mountain's top is not the
    horizon's color.** Reported on 2026-10-23 (Decision 34), with the chunk-distance fog on
    mountains the color is the horizon's applied to blocks near the sky, with the ask for a
    height-based color or the modpack's way. The mend: the fog's color for a pixel is the sky drawn
    behind it, read along the view ray's elevation from the sky's gradient, so a block high against
    the sky fades into that sky's blue and one at the horizon into the horizon's haze, as the
    modpack's shader does; the haze of item 58 keeping its share. A test reading a far block high in
    view against the sky's pixel beside it. One commit, small. Done at `532e45c` on 2026-10-23, in
    the shader from the sky map, as the head says.

82. **No plant stands in the air where a cave was carved.** Reported on 2026-10-23 (Decision 34),
    grass is placed in the air where a cave was carved at a later stage. The mend: the carving
    before the decoration in the chunk's stages, as the source orders them, or every plant placed
    re-checked for the block under it after carving; a test of a carved chunk holding no plant over
    air. One commit, small. Done at `f9ab3bb` on 2026-10-23, as the head says.

84. **A loading screen while the world generates.** Decided on 2026-10-23 (Decision 34), the
    beginning should show a loading screen for the world generation, the window being blank while
    the world loads in for a second. The mend: a screen with the world's name, a progress bar over
    the sections generated and meshed around the player and the pack's fetch, shown until the
    player's column and its neighbors stand, then the world shown; a test reading the screen's
    pixels in the first frames and the world's after. One commit, small. Done at `a9b0fcf` on
    2026-10-23, the departures in the head.

85. **No line on a grass plant's top.** Reported on 2026-10-23 (Decision 34), a thin line of the
    grass plane's pixels shows on the top of it, a faint cross at the top of some. The cause to
    read: the crossed quads reaching a texel beyond their picture in the atlas or the atlas's
    padding, so the row above bleeds in. The mend: the quads' coordinates inset by half a texel or
    the atlas padded with each tile's edge; a test reading the pixels above a grass plant's picture
    for the sky alone. One commit, small. Done at `2f9ed14` on 2026-10-23, in shader form, as the
    head says.

86. **Glass and water reflect nothing for the present (Decision 36).** Decided on 2026-10-23
    (Decision 34), the reflections on glass and water show the scene field, clashing with the game's
    voxel style, and should stay non-reflective for the time being. The mend: the glass's and
    water's materials set with no reflection, the water keeping its tint and transparency, the
    field's reflection left off until asked for; a test reading a water pixel for the sky's color
    alone. One commit, small. Done at `10cf3f3` on 2026-10-23, by roughness since the package has no
    switch, as the head says.

87. **The game's own plant masks show only where no pack gives the plant's picture.** Found by the
    coder on 2026-10-23 under item 85: TerrainAtlas.Build lays the game's own plant masks over every
    pack's plant pictures, the branch after the grass looks checking nothing of whether a pack gave
    the tile though its comment says a plant no pack has, so with Bare Bones every grass, flower and
    sapling shows the game's own masks. The mend: the one condition, the masks laid where no pack
    gave the tile alone, and a test of a plant with a pack tile keeping the pack's pixels and one
    without taking the mask; the plains village picture taken again if it changes. One commit,
    small, before item 71. Done at `6454b80` on 2026-10-23, as the head says.

88. **The play job comes back under 40 minutes.** On the build run 39996906286 of `22616f4` on
    2026-10-23 the play job took 53 minutes 16 seconds against its limit of 60, where the walk at a
    render distance of 2 alone took 24 minutes 39 seconds on the run before; the checks of items 60
    to 87, each opening the world, waiting frames and reading pixels, grew it by half an hour on
    lavapipe, and the next few items add theirs. The mend: the play script's log timed check by
    check from the run's log, the costs named in the commit, and the job brought under 40 minutes by
    sharing one open world among the checks that can share it, waiting the frames a check needs and
    no more, and reading at the render distance each check needs; the limit stays at 60 as Decision
    7 set it. One commit, before item 77. The run of `532e45c` took 57 minutes 38 seconds, within
    three minutes of the limit, so this item goes out before any check is added. Done at `b730088`
    on 2026-10-24, pushed at 00:54, the departures and the two open points in the head; settles on
    its play job's time. The run of `b730088` ended green, its play job 28.8 minutes, under 40.

104. **The sun's map and the field see every section the eye could, whichever way it looks.** In
    play in a cave at about 10:5x on 2026-10-24, on looking down the ceiling was seen to go, blue
    coming from the top and the sun's light on the floor. Every draw the game makes goes to the
    engine's shadow pass and its field as well as the camera's pass, and ChunkRenderer's flood walks
    open cells rather than the view's frustum, so the ceiling's sections should be in the draw set;
    read first that they are, looking down in the flat world's stone room with an opening, the draw
    set listed. If they are, the fault is the engine's, its cascades culling the casters to the
    sphere about the view's slice, which is item 94's reading there, and its field's top counting as
    sky, which item 72's far cascade answers with the package, and this item closes with the reading
    written under the engine's item 94. If they are not, the flood walks every open way with no
    bound to the way the eye looks and the reached sections are drawn whether the camera sees them
    or not, the cost read under item 93 and a test that looking down keeps the ceiling's sections.
    One commit at most, small, first. The cave reading of 10:59 shows nothing leaking near the eye
    and the far leaks the map's and the field's, so the draw set is not the cause; the reading of
    the draw set looking down closes this item. Confirmed at 11:0x, the deep cave's draw set reading
    2,277 sections, 83 draws and 2,031 culled the same at pitches of 60, 0 and minus 89, the flood
    reading neither the look nor the frustum, its one bound beside the joins and the reach the
    source's never-back rule, the same whichever way the eye looks; closed, no change.

103. **The flames' top row is the picture's own, with no stair-stepped line.** Seen in play at about
    10:55 on 2026-10-24, at the top of a fire a line one pixel wide with stair-stepped edges, a
    texture coordinate that is not pixel-exact the likely cause. The flames' pictures are the pack's
    animated fire, frames stacked in one image, and the source's fire models stretch their faces
    over a frame; a coordinate reaching the frame's edge reads the next frame's bottom row or the
    atlas's neighbor through the filter, so the row shows as a line that steps with the face's
    slant. The mend by the reading, the frame's coordinates inset by half a texel, or the frame
    sampled nearest and clamped within its own rows, as the terrain's tiles are kept within theirs;
    a test reading the flame's top row against the frame's own and a capture of a fire at a slant.
    One commit, small, after item 102. Done at `3236b56` on 2026-10-24, the carried place clamped
    half a texel of the sampled level inside its tile, the check in play.sh reading the north wall's
    top edge against the ground beyond.

93. **The frame's cost on the development machine with the taller world, read and brought down.**
    Found in play on 2026-10-24, the performance is bad. Read first at the saved settings, the
    render distance of 12, High and the three packs, at the cherry grove and the plains village with
    the engine's profile: the GPU's passes, the scene pass, the window depth, the shadows, the
    global illumination and the rest, and the main thread's parts, with the deep sections' share,
    the aquifers' and the caves' meshing, the screen probes off; say the numbers and what each would
    give back. Then the mends by the reading, in order of what they give for what they cost, each a
    commit with its before and after: the sun's cascades' count and the shadow distance, the deep
    sections drawn only where a cave opens to the eye's reach, the meshing of sections the eye
    cannot see held back, and whatever else the reading names; the frame held under 16.7 ms at 12 on
    the 4070 as item 90's draw test holds the draws. After item 92, before item 27's part C. The
    reading at 11:18 on 2026-10-24, Release, offscreen, the three packs, 12, High, the probes off,
    300 frames, the cherry grove and the plains village at hour 16. At 1280 by 720 the frame is 14.9
    and 15.3 ms, the GPU's scene pass 8.3 to 9.6, the global illumination 2.1 to 2.6, the shadows
    1.8 to 2.3 and the window depth 1.5, the graph's recording 8.9 to 9.9 ms of CPU with the game's
    drawing 1.6, 1,683 draws and 1.18 million triangles; at 1920 by 1080 the frame is 19.0 and 19.2,
    the scene pass 12.2 to 13.3 and the rest unchanged, so the GPU and its scene pass bound the
    frame. With global illumination off the frame is 11.9 against 18.6 at 1080p, 2.9 of it the pass
    and 5.6 the cascades read per pixel inside the scene pass, which is the engine's. The build
    played is Debug through dotnet run, standing still 13.4 to 13.7 ms, in a sprinting flight over
    unloaded land 17.7 to 26.1 with the game's update at 4.2 to 7.8 ms of joining and meshing, where
    Release reads 11.6 to 17.8 with the update at 1.0 to 1.3. The mends by gain for cost, each with
    its before and after when the GPU is quiet, are the terrain shader's stepped light reading the
    sun's shadow once per pixel rather than twice, then the shadow distance from 96 toward 64 or 48,
    about 1 to 1.4 ms of GPU at 48 with the shadow line nearer, then the deep sections drawn only
    where a cave opens within reach, about 0.6 ms of CPU for a hundred draws fewer and the GPU's
    share read then, then the meshing of unseen sections held back, the flight's update; and the
    engine's per-pixel read of the cascades in the scene pass, 5.6 ms at 1080p, goes to the engine
    as an entry in its ASKS.md. Play runs in Release, dotnet run -c Release or a published build,
    which the record says. The second mend at `89d8caa`, the shadow distance 64, about a millisecond
    at 1920 by 1080, the village's trees' shadows kept where 48 loses them; the third at `08d0943` a
    counter alone, the flood already drawing a deep section only where the eye's open cells join it,
    3.5 to 10 percent of the triangles, a cut by distance hiding cave interiors seen through their
    mouths; the fourth at `4a2cd7b`, the unseen sections' meshing held back, the flight's update 0.7
    to 0.9 ms where 1.3 stood. Done at `4a2cd7b` on 2026-10-24, the cherry grove's frame at 1920 by
    1080 about 16.8 ms where 17.8 stood before the mends and 14 to 15 at 1280 by 720, the first
    mend's one read with the package carrying the engine's item 99 as item 105.

99. **No tree stands cut by a cave or a cliff, and no leaves hang with no trunk.** Seen in play at
    about 10:51 on 2026-10-24, trees cut off where caves and cliffs carved the terrain after the
    tree was placed, some leaves staying in the air, as the grass over carved caves did before item
    82. The source carves before it places its features, so the caves of item 27, the entrances
    among them, and the cliffs' cuts run before the trees and the plants in the column's pipeline,
    the neighbors a tree spills into carved first as well; and a tree whose base would stand on air
    or whose trunk a cut would cross is not placed there. Tests, over a seeded region, that no log
    has air under the trunk's base, that no leaf stands with no log within the crown's reach, and
    that the trees' count stays within the source's range for the biome, failing without the mend; a
    capture of a cliff's edge and a cave's mouth under trees. One commit, after item 98 and before
    item 94. Done at `dc2ad9f` on 2026-10-24, the departure in the head.

100. **Felling takes every log of the tree, and the crown decays within a second or two.** Trees
    were felled at about {now} on 2026-10-24 and it worked but for one tree whose single log stayed
    disconnected, its leaves hanging until that log was broken by hand, and the leaves decayed too
    slowly. So the felling's flood follows a log to every one of its 26 neighbors, the corners
    across two axes among them, as the branch logs of the bigger trees stand, and counts the trunk's
    logs under the cut as the tree's where the cut was made above the base; and the crown's leaves
    decay each at its own time within one to two seconds of the last log, where item 69 spread them
    over half a second to four. Tests, over the game's tree kinds, that felling leaves no log and no
    leaf within the crown's reach, and that the last leaf goes within two seconds; the play check as
    it stands. One commit, small, after item 99. Done at `54fc848` on 2026-10-24, the cause two
    trees joined through their branches, the departure in the head.

101. **Grass and the other plants give way to a block placed where they stand.** Blocks were placed
    at about {now} on 2026-10-24 and aiming at a plant, the plant highlighted, placed the block in
    front of it rather than in its place, and a block placed beside another was refused where a
    plant stood in the cell. The source's plants are replaceable, so a placement whose cell holds
    one of BlockGroups.GivesWay, short and tall grass, ferns, dead bushes and their kin, replaces
    the plant, and aiming at such a plant places the block in the plant's own cell rather than on
    its face; the plant drops nothing. Tests of a block placed on a plant's face landing in the
    plant's cell and of a block placed beside another into a cell a plant holds; the play check as
    it stands. One commit, small, after item 100. Done at `ad3499f` on 2026-10-24, the play check in
    place of a unit test since none builds a game.

102. **A held light shines from the player, not from the hand.** Found in play at about 10:53 on
    2026-10-24, the held light stands at the hand, a third of a block ahead of the eye and to the
    right, where it clips into a wall the player faces and turns with the camera, so the light about
    the player swings as the eye turns. Item 74's light moves to the player's own place, the body's
    middle at the eye's height, so the camera's turn has no part in it and no wall takes it inside;
    its color, strength and range stay as item 74 set them. A test that the ground's light about the
    player reads the same at four headings with a torch held, and the play check of the lamp room's
    roof as it stands. One commit, small, after item 101. Done at `1f07038` on 2026-10-24, the light
    at the eye's place in the body's middle.

98. **The particles keep their light at every angle of looking.** Seen in play at about 10:51 on
    2026-10-24, the particles showing only the ambient color at some angles of looking. Read first
    which particles, the game's own bits cut from a block's side, the engine's particle sheets of
    the blasts' clouds, or the flames, and at which angles between the eye, the sun and the
    billboard, with the readings of one particle from eight headings around it under the sun; then
    the mend where the cause lies. A billboard's face follows the eye, so a lighting that takes its
    normal against the sun goes dark where the sun stands behind the eye's side of it, and the right
    light for a particle is the sun's reaching its center through the shadow map with no normal
    term, or the normal's dot taken on both sides, with the probes' light gathered about its center;
    where the cause is the game's bits shader the game mends it, and where it is the engine's
    particle sheet the gap goes into the engine's ASKS.md as an entry and the game waits on the
    package. A test reading the same particle from eight headings within a few levels of each other.
    One commit, after item 93. Done at `56cb608` on 2026-10-24, the bits lit as a face turned up,
    the clouds and the flames not it. Reopened at 13:21 on 2026-10-24 by decision, the bits lit as a
    face turned up not being meant and that reverted; the fault is a broken block's bits dropping
    with a part clipping into the wall beside them, as in a tunnel two high and one wide dug into a
    hill, and taking the sun's or the sky's light there. The mend by the reading, a bit's light
    sampled at the open cell it came from or the air it falls through, a bit whose center enters a
    block keeping the last open cell's light, so no sun or sky reaches a bit through a wall; the
    turned-up face taken out; a test breaking a block at the end of such a tunnel under the sun and
    reading the bits beside the wall within a few levels of the tunnel's floor. Added at 13:21, the
    turned-up face makes the bits dropping from a block above dark where the face toward the eye
    says they take the light from the eye's side, so the bits' lighting before `56cb608` is read
    first and put back, and the mend for the clipping is built on it. One commit, first, before item
    94. Done again at `ccf6fcf` on 2026-10-24, the engine's lighting back with the light read at the
    bit's middle, the tunnel's bits within six levels of one another.

106. **The corner shade stands at a half by default.** Decided at 13:23 on 2026-10-24, the corner
    shade should be 0.5 by default; `SectionMesher.CornerStrength` starts at 1, the whole of the
    occlusion's darkening at a block's corners. The default becomes 0.5, the settings window's
    slider, the menu's and `voxel.corners` unchanged, a saved setting still read over it; the
    README's line on how far corners darken says the half, and the mesher's test of the default
    reads it. One commit, small, after item 98 and before item 94. Done at `e82d965` on 2026-10-24,
    the default a half, no saved setting since the game saves none.

107. **The rain forest's floor is shaded, not dark, the canopy taking its levels at its edges.**
    Reported at 13:26 on 2026-10-24, the rain forest is too dark in the middle with the ask for a
    better balance, with the example that the light level should not drop by one with each block of
    leaves but at the way in and the way out, a canopy five deep dropping two and the blocks between
    ignored. `Lighting.cs` takes one level for each block of leaves, the way down too, so the rain
    forest's layered canopy darkens its floor as a cave is darkened. The mend by the example or a
    better balance the coder finds and says in the commit, a leaf lit from a leaf taking nothing and
    the canopy's outer layers taking the levels, so a canopy five deep takes two, one deep still
    one, sideways spread under it as before; a capture of a rain forest's floor at noon before and
    after, the forests and the other biomes read for no change beyond the canopies, and a test of a
    column under five leaves reading two levels under the open sky's and under one leaf reading one.
    One commit, after item 106 and before item 94. Done at `aaeaa2a` on 2026-10-24, the levels'
    balance in; the floor's darkness found to be the engine's light under a canopy the field holds
    solid, taken on as item 117 and an engine entry.

117. **A face under a canopy is lit at least by the sky's light at its level.** From item 107's
    reading at 14:02 on 2026-10-24, the rain forest's floor at noon near black in the capture while
    its sky level reads 13, since the canopy shadows the sun and the field holds the leaves solid,
    so the engine's sky light does not reach the floor where the source lights it at 13 of 15. The
    terrain shader lifts a face's light to a floor of the sky's light at the hour, the sky's color
    and strength the game already feeds the sky, scaled by the face's sky level on the source's
    brightness curve, as a maximum against the engine's light, so open ground and the sunlit patches
    are unchanged and the floor under a canopy reads as the source's level 13 does, about 0.85 of
    the open ground's sky light, and at night the floor term is the night sky's and so near nothing;
    the plants and the held bits the same; the rain forest's floor at noon and at midnight captured
    before and after with the plains for no change, and the play check reading the jungle's floor at
    noon within a few levels of the open ground's shade. The proper mend is the engine's, the field
    letting a share of light through leaves, which the game's ASKS.md entry asks and the engine's
    item 100 takes; this floor stands meanwhile and stays as a floor after it. One commit, after
    item 107 and before item 108. Done at `f480dbf` on 2026-10-24, the rain forest's floor at noon
    42.4 where 20.2 stood, open ground and the night unchanged, the departures in the head.

108. **The clouds reach the camera's far plane, fading into the sky there.** Reported at 13:29 on
    2026-10-24, the clouds still have a limit, with the question whether the scene field's last
    cascade reaches the far plane as the lowest level of detail, which the engine's item 72 gave it
    on its main and the package after 5.2.215 carries. The clouds are the game's own, `Clouds.cs`
    building the cells within twice the render distance of the eye and fading them into the sky from
    six tenths of that reach, 384 blocks at 12, so their edge shows under a high sky where the
    land's does not. The clouds reach the camera's far plane, the near cells as boxes as they are
    and the cells beyond the present reach as the flat layer alone where their sides are not seen,
    the fade into the sky at the far plane; the drift, the night's darkening and the capture's hold
    unchanged; the clouds' cost read before and after at 1920 by 1080 with the cells' count, and a
    capture of the horizon under clouds at noon. One commit, after item 107 and before item 94.
    Closed at 14:49 on 2026-10-24 by the decision given to the game's session at about 15:00,
    Decision 44, the clouds not stretched so far, the before images right where they covered the
    area about the player and did not reach the horizon; what was built, the boxes within twice the
    render distance and the flat layer out to the haze dome at 3,576, 18,131 faces against 2,723 and
    about 1.05 ms of the scene pass against 0.4 at 1920 by 1080, reverted uncommitted, its play
    check on a cloud's underside red besides.

109. **The cactus is the source's model, its sides a pixel in and their clear edges cut out so the
    thorns stick out.** Reported at 13:31 on 2026-10-24, the cactus needs to be one pixel thinner,
    white showing in its corners, and at 13:46, the thorns are on the white border, so the faces go
    a pixel in and the white is cut out as alpha so the thorns at the edges stick out. The game
    draws the cactus as a whole cube whose texture's clear edges show the color the pixel holds, the
    TODO line on the source's model open. The cactus becomes the source's model, each side face
    standing a pixel in from its side of the cell and spanning the cell's full width, the clear
    pixels cut out as the plants' are so the thorns on the picture's edge columns stick out past the
    face beside them, the top and the bottom on the inset square, the top at the height the source's
    model gives it, the bottom on the ground, with the picture's rows and columns as the source maps
    them; its collision box the source's; a capture of a cactus up close against the before, and the
    mesher's test of the cactus's faces. One commit, small, after item 108 and before item 94. Done
    at `02abb9d` on 2026-10-24, the source's model with its two departures in the head.

110. **The bits of a tinted block carry the biome's tint.** Reported at 13:31 on 2026-10-24, the
    color of the particles for grass and dirt is not adjusted to the temperature of the biome,
    breaking grass in the savanna dropping pure green bits and not the savanna's color. The bits cut
    from a block's picture take the tint the block is drawn with at its place, the grass block's top
    and its side's overlay, the short grass, the ferns and the tall plants by the biome's grass
    color, the leaves by its foliage color, the untinted blocks as they are, dirt among them; a test
    breaking a grass block in the savanna and in the plains reading the bits' colors as the blocks'
    tops read there, within a few levels, and a capture of each. One commit, small, after item 109
    and before item 94. Done at `2a399af` on 2026-10-24, each bit carrying its climate, the test on
    the savanna against the plains.

111. **A felled tree's crown drops as its logs fall, not after they land.** Decided at 13:33 on
    2026-10-24, breaking a tree with the axe drops the logs first and the leaves only once the logs
    have fallen, and both should happen at the same time. Felling starts the crown's decay and the
    leaves' drop in the same tick as the logs begin to fall, the crown's pace of item 100 kept, so
    the leaves and the logs are in the air together; the felling test reads the first leaf gone
    within the tick of the first log's fall, and a capture of a felled tree in the air. One commit,
    small, after item 110 and before item 94. Done at `f00f649` on 2026-10-24, the crown read at
    once and its leaves going from the first log's tick.

112. **Felling wears the axe by the logs it drops as items, the falling logs not counted.** Decided
    at 13:36 on 2026-10-24, the axe's use should drop by the number of logs felling breaks, a tree
    of eight logs taking eight, and where three of the eight drop as items and the rest fall as
    blocks, three, the falling logs not counted. Felling takes one use of the axe for each log it
    turns into a dropped item, none for a log that falls as a block, and the axe that runs out
    mid-tree breaks the logs it had uses for and leaves the rest standing or falling as the source
    does; the felling test reads the axe's uses after a tree of known logs with a known share
    falling, and the README's felling paragraph says the wear. One commit, small, after item 111 and
    before item 94. Done at `fd17371` on 2026-10-24, one use a dropped log and none a falling one,
    the felling stopping with the axe, the departure in the head.

113. **Pyramids are as rare as the source's.** Reported at 13:36 on 2026-10-24, there are far too
    many pyramids. The desert pyramid is placed at the source's spacing, one at most in each region
    of 32 columns a side with a separation of 8, the region's one place drawn from the seed, and
    only where the desert holds it, so a desert of the size seen shows one or none where it shows
    several; a test counting the pyramids over a seeded span of desert against the source's bound,
    and the README's line on the structures saying the rarity. One commit, small, after item 112 and
    before item 94. Done at `031d169` on 2026-10-24, 6 pyramids on seed 1's 8,192-block square where
    128 stood.

115. **A wall torch's wood leans up to its head's foot, and the lit head alone stands straight.**
    Reported at 13:41 on 2026-10-24, the torch on a wall goes straight a pixel too early, the wood
    bending to straight where only the lit part should stand straight up. The wall torch is a
    leaning stick piece, its top's corners moved out by the lean, and a lit head piece standing
    straight on it; the stick's lean reaches the head's foot, the head the picture's own two rows of
    coal and nothing of the wood, one pixel higher than it stands, read against the source's torch
    picture and its wall torch model; a capture of a wall torch up close before and after, and the
    mesher's test of the wall torch's pieces reading the stick's top at the head's foot. One commit,
    small, after item 114 and before item 94. Done at `65eec11` on 2026-10-24, the split at row 8,
    the head's glow scaled so the light stays.

116. **A held axe faces the right way, its blade forward.** Reported at 13:44 on 2026-10-24, the axe
    is backwards when held. The held tools are the source's item pictures stood up in the hand, and
    the axe's blade points back toward the player where the source holds it forward; the tool's
    picture in the hand is turned as the source's first-person item display turns it, the blade and
    the pick's head forward and the handle toward the eye, read for the pickaxe, the shovel, the hoe
    and the sword as well so each matches the source; a capture of each tool held before and after,
    and the test of the held item's placement reading the axe's blade ahead of its handle. One
    commit, small, after item 115 and before item 94. Done at `cce1536` on 2026-10-24, a tool's
    picture turned by a half about its handle's diagonal before the quarter turn, the departure on
    the source's sign in the head.

118. **A held tool's pixels meet with no gap along its borders.** Reported at 16:39 on 2026-10-24,
    the held tool items show a pixel gap everywhere around the borders, stair-stepped aliasing. The
    held picture is a block of pixel cubes cut from the item's picture, and along its outline a gap
    shows between the border pixels and the body, or the background through the border's edge. Read
    first where the gap is, in a capture of a held tool at four times the size against the picture's
    pixels, whether the cutout's alpha test drops the border texels where the filter blends them
    with the clear ones beside, the cubes' faces standing a half texel off their pixels, the atlas's
    padding, or the depth of the side faces against the front; then the mend at the cause, the
    picture sampled nearest with the texel's own color at every cube and the cubes meeting edge to
    edge; a test reading a held tool's capture for no background pixel inside its outline, and the
    capture before and after. Added at 16:40, the tool jiggles as the camera moves, which closes and
    opens the gaps at different places, so the gaps move with the subpixel position, as cracks do
    between faces whose shared edges are not the same floating-point vertices, each cube's corners
    worked out on their own, or as a front face and its side faces fight in depth along their edge;
    the reading takes this first, and the mend then has every cube take its corners from one grid of
    vertices indexed by the pixel's corner so neighbors share them bit for bit, the front faces
    merged along runs of pixels, and the item drawn without sway for the capture to tell the sway
    from the cracks. One commit, first, before item 94's pieces. Done at `9a3d746` on 2026-10-24,
    each pixel its own faces from one grid of corners, the cracks read as the front's cut edge
    against the sides' geometry, the departure in the head.

119. **The terrain's faces meet with no crack, so no light shines through a dark cave's walls in
    pixel-wide lines.** Reported at 16:41 on 2026-10-24, blocks show the same pixel-wide gap as the
    held tools, and it is the complaint from the black cave, the one-pixel stair-stepped lines of
    light where light shines through, read at 11:0x as item 83's missing mend in the engine. Added
    at 16:42, the gap is not everywhere but on a lot of edges, which fits cracks where quads of
    different sizes abut and not a seam at every block. The game's mesher is not greedy, each block
    face its own quad with corners on the block grid, so the lines are not T-junction cracks of
    merged quads; the cause is read, the faces' shared corners not the same values, a face's edge
    against a neighbor's at a different depth, or the shadow map's. Read first on 5.2.281 under item
    27, since the engine's item 83 mend was for these lines, in a dark cave at a fixed view with the
    sun's shadow off and with it on, and with the camera moved by a fraction of a pixel, whether the
    lines are cracks of the mesh, which stay with the shadow off and move with the camera, or the
    shadow map's, which go with the shadow off, saying which with a capture; then the mend for the
    cracks, at the cause the reading finds, said in the commit with the draw counts before and after
    where the mesh changes; a mesher test over a sample section reading no vertex on the inside of
    another face's edge, and the cave capture before and after. The shadow map's share, where the
    reading finds one, goes to the engine's ASKS.md. One commit, after item 27's package pin and
    before item 120. Done at `ffa7a7b` on 2026-10-24 as a reading, the lines the sun's map's and not
    the mesh's, the mesher's test added, the map's share the engine's item 101 through ASKS.md.

120. **The selected item's name over the hotbar fades away.** Decided at 16:44 on 2026-10-24,
    selecting an item from the hotbar shows its name, which should fade away and not stick forever.
    The name shows when the selection changes and fades out as the source's does, held about a
    second and a half and faded over about half a second, shown again on the next change, and the
    HUD's own fade of full rows unchanged; a test reading the name's alpha over the frames after a
    selection, full, then gone, and the play check's HUD capture read for no name after the hold.
    One commit, small, after item 119 and before item 94's pieces. Done at `7ece2c1` on 2026-10-24,
    held 1.5 s and faded over 0.5 as the source's.

121. **A shadow darkens the ground and keeps its warmth, the sky's blue out of it.** Reported at
    18:09 on 2026-10-24, with a capture of the plains village from above, the shadow of the near
    tree is very blue while the far shadows, faded by the cascades, read as a darkening that keeps
    the warmer colors, which is easier on the eyes in the warm setting, with the question whether
    that look can be had everywhere, on the assumption that the blue is the ambient color the
    cascades take from the sky. Read first what lights a shadow at noon, the environment's
    irradiance the game feeds the engine for the hour and the probes' share of sky, each read alone
    on the village's tree, with the numbers; then the mend, the light the game feeds for the shadows
    drawn toward neutral at the hour's brightness, the dome's blue untouched, so a shadow is the
    ground darker with its own color, as the source's are, and the probes' sky share following it
    where the engine takes it from the same color, or an ASKS.md entry where the engine's sky share
    has no such control; the night's moonlight kept as it is; captures of the village's tree at
    noon, the cherry grove and the rain forest's floor before and after, and the play checks'
    readings held within their bounds. One commit, first, before item 94's pieces. Done at `4142a32`
    on 2026-10-24, the environment's light drawn toward white by day through SetEnvironmentLight,
    the after images found good, the departure on the sky's reflections in the head.

122. **The overworld's loading screen is the red of the source's loading overlay.** Asked of the
    game's session at about 18:4x on 2026-10-24, the loading overworld screen in the red from the
    source's logo, a nice color. The README says a world opens behind a loading screen in that red
    already, so the screen meant is the one shown while the overworld loads that is not, read from
    the game and said in the commit. Its ground becomes the source's loading overlay red, 239, 50,
    61, the bar and the words on it readable against it as the source's are, the other screens
    unchanged; a test reading the overlay's color and a capture. One commit, small, before item 94's
    pieces. Done at `e04a1a5` on 2026-10-24, the one loading screen, dark gray before, in the
    source's red.

125. **The loading screen is pure black.** Decided to the game's session at about 18:5x on
    2026-10-24, the red loading screen is too neon and is to be pure black.
    VoxelGame.LoadingBackground becomes 0, 0, 0, the bar's empty part with it, the title and the bar
    reading clearly, and the play check's corner reads black; the README's line follows. One commit,
    small, after item 27's pin of 5.2.283 and before item 124. Done at `e643539` on 2026-10-24.

124. **The settings live in the game's own screens, by area, and F3 keeps only what is read.** Asked
    of the game's session at about 18:5x on 2026-10-24, to convert the settings from F3 to the
    in-game UI, where further things can be set, as a render settings screen holding the global
    illumination's settings, a post processing screen, and sky and visuals, the sun's color at noon
    and the like where it is not hardcoded, since F3 becomes the information screen and not a
    settings screen later. The game's options, reached from the pause menu and the title as the
    source's are, gain the screens, Render with the quality, the probes, the shadow distance, the
    render distance and the corner shade, Post processing with what the engine offers, and Sky and
    visuals with the day's colors where the game sets them; each setting that F3's window holds
    moves there and F3's window keeps its readings and the toggles of debugging alone for the
    present, the whole of F3 the information screen on a later word; the settings in the source's
    style of buttons and sliders, read for their reach and their defaults and said in the README; a
    test walking each screen's settings and reading them applied, and captures of each screen. Two
    commits, the screens and the moves, after item 125 and before item 94's arrows. The screens at
    `a82e342` on 2026-10-24, the departure on the test in the head; the moves out of F3 next. Done
    at `5f7bad8` on 2026-10-24 with the moves, F3 holding its readings, the debugging toggles and
    the world's choice; the video settings and the saved options follow as items 126 and 127.

126. **A Video Settings screen holds the resolution, the window mode, the frame limit and VSync.**
    Reported to the game's session at about 19:2x on 2026-10-24, video settings are needed too, for
    the resolution and the window mode. The screen, beside the others under Options, holds the
    resolution from the monitor's modes through GetMonitorModes, the window mode as windowed,
    fullscreen or borderless through the engine's SetWindowFullscreenMode and
    ToggleBorderlessWindowed, the frame limit through SetTargetFPS, and VSync where the engine
    exposes it or an ASKS.md entry where it does not; each applied at once and read back,
    voxel.options covering them, the README's table gaining the rows; play.sh walks them where the
    runner allows, the window mode left out offscreen. One commit, after item 124's second commit
    and before item 127. Done at `80d149e` on 2026-10-24, the four settings read back live, VSync
    through the package.

127. **The settings are kept across runs.** From item 124's first commit on 2026-10-24, no setting
    is saved, the game having had no options file. The settings of every screen are written to an
    options file in the save root as the source's options.txt is, one line a setting by screen and
    name as voxel.options names them, written on a change and read at start before the world opens,
    a missing or malformed line taking the default; a test writing, reading back and ignoring a bad
    line, and the play check reading a setting after a restart. One commit, small, after item 126
    and before item 94's arrows. Done at `d6532d1` on 2026-10-24, options.txt beside the saves, the
    world's own settings left to its save.

96. **The player and the game's states live in the ECS (Decision 43).** The player an entity with
    its body, its vitals and its hand as components, the inventory a resource, the first person
    controller a behavior reading the input from its context; the game mode, the menus and the
    loading screen as ECS states, with InState, OnEnter and OnExit on the behaviors they gate and a
    run condition where VoxelGame.Update holds an if, so what runs when is read from the attributes
    rather than the one long Update. One or two commits, after item 95; the commands of voxel.* and
    the interface's immediate drawing stay as they are. The plan, read at 21:22 on 2026-10-24, two
    commits. The frame today runs the game's update before BeginDrawing, which pumps input and runs
    First through Update, the game's Draw sets the camera and EndDrawing runs PostUpdate through
    Last, so a controller in PreUpdate moves the player before the camera is set with no lag while
    Aim and Interact left in the update would read the frame before's eye; so the game's update
    moves into the stages in its present order, the program loop calling Draw alone and VoxelGame a
    resource the systems reach. First, the player an entity with its FirstPersonController, Body,
    Vitals and HandRenderer, the VoxelGame properties pointing at the same objects so voxel.* and
    the interface stand, the inventory a resource the drops' PickUp reads; GameFrame.Begin on First
    for the world switch, the loading stand, the frame's seconds, the save timer, the toggles, the
    cursor and the hotbar keys, setting GameStep's seconds and the player's look and move flags;
    PlayerControl.Steer on PreUpdate, the controller reading the engine's input in place of the flat
    calls and the hand's swing from the distance walked; GameFrame.Play on Update with the rest of
    the update in order; input pumped before the stages, the game reading this frame's keys where it
    read the frame before's. Second, the states, GameMode, Menu and a WorldState of Loading and
    Playing as engine states through AddState, read and written through GetState and SetState, each
    if an attribute, InState Playing on Steer and Play and RunIf Lives on a Live system split out of
    Play, Lives a computed state of survival and adventure, OnEnter Pause freeing the cursor and
    OnExit None stopping the breaking, the loading screen's draw keyed off the state; the order
    within Update between Play and the entities' systems as loose as it is, their events drained the
    frame after. Risks, the commands' timing against Begin within First and play's input checks if
    input lands a frame earlier, read against today's readings. Taken as planned. The first commit
    at `e77030b`, the player an entity and the update in the stages, the readings held; the walk's
    6.82 blocks in 60 frames against 6.15 read at 21:49 as the frame time's spread, both builds
    ending at the same place under a fixed frame time of a sixtieth, 4.30 blocks each, W seen for 60
    frames in both and one frame earlier now, nothing to mend. Done at `2926dda` on 2026-10-24 with
    the states, four engine states and the ifs as attributes, the ordered ones inside Play until the
    engine's item 110 gives the order.

70. **The land's light is the same above the eye's height as below it.** Reported on 2026-10-23
    (Decision 34), a visual fault with the height, the land lighter below a line and darker above,
    related to the horizon, the top of it taking the dirt texture's color with the grass about
    orange or more teal, nearly black at points and flickering in motion, taken for the same fault
    the mobs had from the lips up. The cause is read as item 63's: the engine's model pass turns a
    normal that points away from the eye over, so a sample the terrain shader takes with a turned-up
    normal above the eye is lit as a face turned down, and the overlay's choice of the grass top's
    color over the dirt side may ride on the same normal. The mend: every sample the terrain and
    plant shaders take with a normal of their own sees it from straight above or along the face as
    item 63's does, and the overlay's color is chosen by the face's own side; a play-script check
    reading a grass wall's side from the player's height across the eye's line for no step over 2
    levels, and the plains village picture taken again. One commit, small, first of the list; the
    engine's item 82 removes the trap. Done at `22616f4` on 2026-10-23, the plants' walls, as the
    head says; the land's own faces were never turned. Done at `8bcbfde` on 2026-10-24 as a reading,
    the land's faces showing no step across the eye's height, the grass wall check added and the
    village capture taken again; the dirt's crushed blue read under item 43.

43. **The sun's look, warm and low with long soft shadows, and a blue haze over distant land
    (Decision 21).** The look aimed at has a warm low sun with long soft shadows and a blue haze
    over the far land. The game gives the sun a color and a strength by the hour as the source's
    shader packs do, white at noon, warm at a low sun and red at the edge of night, the sky's
    ambient turning with it; the shadows' softness read against the engine's, a penumbra widening
    with the distance from the caster where the engine has it and asked of it in its ASKS.md where
    not, with the hour's long shadows kept sharp enough to read; and a haze over distant land as fog
    by distance and by height tinted by the sky, thin at noon and thick at dawn, through the
    engine's fog where it takes a tint and a height, the far land of item 26 fading into it. A
    setting for each, on by default. Captures at dawn, noon and dusk of one view in the record
    beside the modpack's. Tests: the sun's color at four hours and the haze's density by distance
    and height. After item 26. One or two commits. Item 58 takes the sun's color by hour and the
    haze's strength from the modpack's settings; this item keeps the shadows' softness, the haze by
    height and the captures at three hours. The reading at 22:29 on 2026-10-24, a dirt wall at noon
    with the packs, the same texel with one step of the frame taken out after another, the pack's
    dirt averaging 155, 101, 49 and drawn 190, 105, 0, saturation 1 giving 174, 111, 46, contrast 1
    then 169, 113, 55, the engine's curve 205, 127, 59, a white noon sun 205, 138, 73, exposure 1
    166, 111, 58 and no global illumination 174, 108, 59, so the crush is the grading's saturation
    of 1.4 pushing the dirt's low blue under 0, the rest moving it little; the village at noon and
    17 at 1.4 and 1.2 sent, the lower half's blue averaging 2.7 and 1.4 at 1.4 against 41.7 and 14.8
    at 1.2, the default changing by decision alone. The package has no soft-shadow call, so the
    penumbra widening with the distance from the caster goes to the engine as its item 111 through
    ASKS.md; the haze grows by distance alone in the terrain shader and gains the haze by height and
    its thickening toward dawn, each a setting under Sky and Visuals on by default; captures at
    dawn, noon and dusk of one view, tests of the sun's color at four hours and the haze's density
    by distance and height through a twin of the shader's formula. Done at `4b398bd` on 2026-10-24
    but for the penumbra, the engine's item 111, and the saturation default by decision, the haze by
    height and toward dawn with their settings and the sun's color read at four hours.

35. **A zoom key (Decision 15).** C held narrows the field of view to a quarter of the setting over
    a tenth of a second and widens it back on release, the mouse's sensitivity scaled with it so the
    aim stays steady, the wheel while held narrowing further to a tenth, as the modpack's zoom does;
    the key settable in the settings. Tests: the field of view over the frames of a press and a
    release, and the sensitivity's scale. One commit, small. Done at `ce693af` on 2026-10-25, the
    share eased, the wheel's steps and the key a general option.

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
    of dark and spores in lush and deep caves drift as the look shows (Decision 21). The weather at
    `24a4916` on 2026-10-25, the clock by seed and day, the precipitation by biome, the overcast sky
    and the walls of rain and snow; the snow layer at `1c881d0`, laid on the tops while it snows and
    melting after; the particles as the second commit next, the splashes and rings, the falling
    leaves, the cave dust and the wakes, each a setting with a density. Done at `7443bdc` on
    2026-10-25 with the particles, the weather at `24a4916` and the snow layer at `1c881d0` before
    it, the three engine gaps as its items 114 to 116.

94. **The things that move become entities with behaviors (Decision 43).** The mobs, the drops, the
    arrows, the primed TNT, the falling blocks, the fires and the blasts, which the game keeps in
    lists of its own and steps from one Update, become entities of the engine's ECS: components for
    what each is, its body of position, velocity and box, its kind, its health, its path and target,
    its fuse or age, its stack, its light, and a behavior for each concern, the movement against the
    voxel world and its collisions, the hostile and the grazing minds, the aging and the vanishing,
    the loot on a death, the renderer reading them by query; the save keeping them per column from
    the components as it keeps them today. One kind of thing a commit, in this order, the drops, the
    arrows, the primed TNT and the falling blocks, the fires and the blasts, the mobs last, each
    with its tests and the play checks' readings unchanged, the game playable at every commit. Where
    the ECS lacks a thing the game needs, queries over more than three components, a save of
    entities, a spatial lookup or a system run on a worker, the gap goes into the engine's ASKS.md
    as an entry and the engine takes it ahead of its other work, the game waiting on the package
    where it must and never keeping a structure of its own around the gap. After items 92 and 93.
    The drops at `5113176` on 2026-10-24, each stack an entity with its behavior and Body and three
    systems by stage, the three gaps found as the engine's items 104 to 106 through ASKS.md; the
    arrows next, after items 27's pin, 125 and 124. The arrows at `c0dd6e4`, one behavior and one
    system, the strikes dealt a frame after through GameStep; the primed TNT and the falling blocks
    next. The lit TNT and the falling blocks at `e0d9788`, their results handed back through
    resources, which the engine's own event channel took over at `299aa33`, the shared lists gone;
    the fires and the blasts at `9a70f28`, a lookup by a component's key asked of the engine as its
    item 107, the engine handles of the blasts' clouds and lights kept in the game's pool outside
    the systems; the mobs last, their plan read here before the code, the six-component queries of
    the engine's item 105 landing with the next package. The mobs' plan, read at 20:48, three
    commits each playable with HostileTests, MobTests and play's readings unchanged. First, each mob
    an entity with Body and the Mob class as a component, its mind inside unchanged, the systems by
    stage, Mind on update for think, hunt and move, Crowd after it pushing mobs apart and off the
    player, and Vanish aging the dying, despawning the gone and the far and sending a MobDied event
    with the loot's kind and place, MobStrike, MobShot and MobBlast as engine events, the Mobs
    facade keeping spawn, Cast, Overlaps, Columns and the joined and leaving over queries since a
    mob leaving its column becomes a record for the save. Second, the health, hurt and dying into a
    MobLife struct with its own behavior, Hit and Harm writing it. Third, the pose into a MobPose
    struct written by the movement and read by MobRenderer through a query, the hostile state into a
    MobHostile struct with the hostile mind as its own system, the path and the Random staying in a
    MobMind class component by E3D006; the third after the package carrying the engine's item 105,
    so its queries over Body, the identity, the life, the pose and the hostile state take the
    four-to-six-component forms rather than GetRef beside a narrower query, and the column
    bookkeeping taking item 107's lookup when it lands, the walk meanwhile. The mobs' first commit
    at `75be580`, the Mob class a component with three systems by stage and its events. The second
    at `5bfafef`, MobLife as a struct with its behavior; the third with the package. The third at
    `3275e76` on 2026-10-25, MobPose and MobHostile with the wide queries, its departures in the
    head. Done at `3275e76`, the drops, the arrows, the TNT and the falling blocks, the fires and
    the blasts and the mobs as entities with their events, the column walks taking the lookup by key
    in the commit after.

123. **The sections the flood culls still cast the sun's shadow.** From the engine's reading of item
    101 at 18:48 on 2026-10-24 on the game at `758e9df`, the deep cave's tops read sunlit because
    the flood draws only the sections the eye sees through open cells, so the ground over the cave
    is never drawn and never reaches the shadow pass, and with voxel.cull off the tops read 0 at
    every shadow distance. With the package carrying the engine's item 101, a draw into the shadow
    maps alone, the game draws the culled sections that lie between its view and the sun within the
    shadow distance through that draw, the sections above and toward the sun of the drawn set's
    bounds, kept meshed as the drawn ones are and counted in the profile as draw.sections.shadow;
    the deep cave's tops and edge lines read again at the entry's place, and the cost at the grove
    and in the cave before and after. One commit, with the package carrying the engine's item 101.
    Done at `c7c1b41` on 2026-10-25, the casters marched from the drawn sections' corners toward the
    light and drawn into the shadow maps alone, the cave's tops dark at 1.5 ms, the departure in the
    head.

105. **The stepped sun's shadow is read once a pixel through the engine's hook.** From item 93's
    first mend, which waits on the engine's item 99, `litLight` taking the shadow light's shadow
    from the caller; the game's `litStepped` then hands the step's read in and reads the shadow
    once, the scene pass about 1.6 ms cheaper at 1920 by 1080, the README's costs paragraph read
    again. One commit, small, with the package carrying the engine's item 99. Done at `fb04f6b` on
    2026-10-25, one read at the step's middle, 0.3 to 0.4 ms at the grove on the mended scene pass,
    the step itself free.

128. **The entities' tests step the real game through the engine's headless app, the twins gone.**
    From the coder's trial at 02:40 on 2026-10-25, after the engine's item 104 made App.Frame and
    DefaultPlugins public. An app built as the engine's behaviors guide says steps the game's
    registered systems, a pig falling through the real MobMind.Think, and a whole VoxelGame builds
    in a test under E3D_HEADLESS with InitWindow, a transient flat world stepping 30 frames of
    game.Draw with its mobs in 2 s. The plan, one kind of thing a commit with play's readings
    unchanged, is a HeadlessGame fixture in a non-parallel collection, the variable, InitWindow, a
    transient flat VoxelGame and CloseWindow on dispose, GameStep.Random seedable so the seeded
    tests keep their sequences; then each kind's tests laying their scene in the game's flat world,
    the placed animals cleared where they would interfere, stepping game.Draw frames, the mobs, the
    fires, the TNT and the falling blocks, the drops and the arrows in that order, each twin, the
    EcsWorld overloads and the facades' Update methods calling them, going in the same commit with
    its logic moved into the context system; the tests of pure logic staying on their small laid
    worlds, the pathfinder, a mob's own Update and Hit, the mesher and the rest. Five or six
    commits, after the lookups. The first commit at `4105293` on 2026-10-25, the fixture and the
    mobs, their twins gone; the fires next with GameStep.Random settable and Fires.Take taking the
    step's Random so a seeded test seeds every draw. The fires at `01eb71f`; the TNT and the falling
    blocks next. The TNT, the falling blocks and the flashes at `480d97b`; the drops next, then the
    arrows. The drops at `8b01c69`; the arrows next, which close the list. The arrows at `6a7e5b1`.
    Done at `6a7e5b1` on 2026-10-25 in five commits, the fixture and the mobs, the fires, the TNT
    and the falling blocks, the drops and the arrows, every twin gone.

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
    commits, the footsteps and the ambience, then the occlusion and the reverb. The first commit at
    `5b62bef` on 2026-10-25, the steps and the loops made as the game starts, the listener and the
    mobs' steps spatial, two engine faults as its items 118 and 119; the occlusion through the
    blocks and the room's reverb next, the low-pass asked of the engine. The second at `e433e5c`,
    the occlusion by the blocks crossed and the room read by rays, the reverb and the low-pass the
    engine's item 120, a mob's non-number mended on the way. Done at `e433e5c` on {day} in two
    commits.

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
    item 30. Two commits, the camera and the model, then the animations. The first commit at
    `a00fdcc` on 2026-10-25, the camera and the model, the reaching arms of every unarmed kind found
    on the way; the animations next, sneaking, swimming, climbing and the air, the swing, the held
    item, the body turning after the head and the first-person hand from the model's arm. The second
    at `6a56e0f`, the poses by the source's numbers with two departures in the head. Done at
    `6a56e0f` on 2026-10-25 in two commits, the pack's player model with item 30.

34. **Connected textures from the packs' properties (Decision 15).** A pack of item 24 that carries
    connected texture properties, OptiFine's `.properties` under its `optifine/ctm` folder with the
    methods `ctm`, `horizontal`, `vertical`, `top`, `repeat` and `random` and their tiles, has them
    read, and the mesher picks a block's face texture by its neighbors of the same kind as the
    methods say, the 47 tiles of the full method by the eight neighbors, so glass, bookshelves and
    sandstone join as they do in the player's instance; the atlas takes the tiles as it takes the
    blocks. Without a pack, the game's own blocks stay as they are. After item 24. Tests: the
    properties parsed to their methods and tiles, the full method's tile chosen for each of a set of
    neighbor cases, a random method stable by position, and a capture of a glass wall from a pack
    where the machine has one, skipped where not. One commit. Under Decision 22 the properties are
    read as a picture's metadata, the tiles and the method's name, and the methods are the game's
    own. Done at `85c3ff6` on 2026-10-25, the six methods read from a pack's properties with the
    full method's table, the departures in the head.

28. **The looks Shine shows, done as the game's own: bloom on emissive blocks, colored light from
    emissive blocks, a rim around blocks, and an unshaded look.** From item 24's reading of the
    reference folder's mod, whose code may not be taken: the engine's bloom on the emissive blocks'
    faces, the colored light the engine's lamps and global illumination give from emissive blocks
    read and set so a lantern's light is its color, a rim light as a setting in the terrain shader,
    and an unshaded look as a setting, each a switch in the settings with a capture. After item 24.
    One or two commits. Done at `79ad110` on 2026-10-25, the four as settings with captures, the
    lamp colors kept the surfaces' own and the rim an edge outline, the departures in the head.

31. **Structures as the source's, more kinds and the modpack's taste (Decision 15).** The world has
    igloos, desert pyramids, dungeons and villages; it gains mineshafts with their corridors, rails,
    supports and chests, underwater ruins and shipwrecks in oceans, jungle temples, witch huts in
    swamps and pillager outposts, each placed by the biome and a spacing and salt as the source
    places them, built from blocks with loot in chests from item 25's blocks; the villages gain the
    biome's kind, desert, savanna, taiga and snowy, with their own blocks and the modpack's
    structure mods read for their shapes as a model; a structure that crosses a column's edge is
    finished in the neighbor as the villages are. After items 25 and 26. Tests: each kind placed in
    a laid world at a known seed with its blocks counted, a structure crossing an edge whole on both
    sides, and a capture of each in the record. Two or three commits by kind. The first commit at
    `af9d829` on 2026-10-25, the mineshafts with the chest, the rail and the cobweb, the departures
    in the head; the second next, the ocean ruins, the shipwrecks, the jungle temples, the witch
    huts and the outposts. The second at `211e57a` on 2026-10-25, the ocean ruins, the shipwrecks,
    the jungle temples, the witch huts and the outposts, the departures in the head; the third next,
    the villages' biome kinds with the savanna added, each with its own blocks. The third at
    `057011a` on 2026-10-25, the villages' biome kinds with the savanna added, the house shapes one
    set for every kind, the departure in the head. Done in the three commits.

129. **The rain's particle texture is drawn upside down.** From the word of 2026-10-25 at about
    05:20, given to the game's session, that the rain particles' texture is upside down; item 32's
    rain draws its streaks from the texture the wrong way up, so the mend turns the texture or its
    coordinates and reads the snow and the other ambient particles for the same. A capture of the
    rain before and after, N 4.8. One commit, after item 31's third commit. Done at `bbe814c` on
    2026-10-25, the column's v taken downward so the rows run down the wall, the snow with it, the
    before-and-after picture in the devlog.

30. **Entity models, textures and animations read from the player's packs in OptiFine's form, the
    game's own the fallback (Decision 13).** Reported at about 00:55 on 2026-10-21, the packs of the
    modpack, Bare Bones and Fresh Animations among them, have the look aimed at, and a Bedrock
    add-on reworks all of this in the same spirit. Fresh Animations is 100 custom entity models
    under `optifine/cem`, `.jem` files of parts and boxes with `.jpm` parts and animation
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
    value, and a capture of a villager and a cow from the chosen packs beside the game's own in the
    record, read from the packs where the machine has them and skipped where it has not. After item
    24. Two commits, the models and the textures, then the animations. Under Decision 22 the reader
    takes a model's geometry and its texture as assets and none of its animation expressions, the
    mobs animated by the game's own gait of item 18 on the pack's boxes; two commits become one.
    Done at `87d089e` on 2026-10-25, the packs' models read as boxes with their pivots and turns and
    the mobs walked by the game's own gait, the departures in the head.

130. **The fog does not light a dark cave's far walls.** From the engine's reading for its item 102
    on 2026-10-25, which answers the question of the cave's distant light asked with item 108's
    clouds. The far cave's tops at 90 blocks read 9, 13, 18 at High under every cascade alone and
    with the merge off, and 0, 0, 0 with the game's fog off, so the light seen far down a dark cave
    is the terrain shader's fog laid over a fragment by its distance alone, and no cascade's light
    reaches those walls. The fog's share at a fragment is scaled by the sky light reaching it, the
    flood's sky level the terrain stream carries for the block the face lies in, so a cave's far
    walls fade to black and not to the fog's color, a cave mouth taking the fog as its sky level
    rises, and the open land's fog stays as it is. The tests read the far cave's tops at 90 blocks
    at 0, 0, 0 at High with the fog on and the plains at noon unchanged by RMSE against before, with
    a before-and-after capture of the cave under N 4.8. One commit, in the gap before the package
    carrying item 108. Done at `0babebb` on 2026-10-25, the haze following the higher of the point's
    and the eye's sky level, so a cave's walls fade to black from inside and the land keeps its
    haze, the departure in the head.

131. **The screen probes are on, with no setting for a player.** From the decision given to the
    engine's session before 09:19 on 2026-10-25, Decision 48, that the screen probes stay on and
    that off is for debugging alone. LightSettings.ScreenProbes is true by default and the Screen
    Probes toggle leaves the options, a saved options file carrying the old value read as on; the
    two calls to SetGlobalIlluminationScreenProbes stay while the engine keeps the call and go in
    the commit that takes the package without it, if it is decided to go. A capture of the plains
    village at noon with the probes on, N 4.8, and the play run reading the probes on. One commit,
    in the gap before the package carrying item 108. Settled further at 09:39 under the engine's
    Decision 72 as amended, the engine keying the screen probes to its quality, left out at Low with
    the glow lights through the glow-only pass and present at Medium and High, so once the package
    carrying its item 103 comes the game's probes follow its global illumination quality row and the
    two calls go, no other row added. Done at `f97a4c9` on 2026-10-25, the probes on by default and
    the toggle gone, the two calls waiting on the package with the engine's item 103.

132. **The tree's documents and comments keep N 4.7 and STYLE.md.** From the sweep of 10:41 on
    2026-10-25 in the gap before item 95's package, over the tracked files less the review's.
    Thirteen places name a person, the README at lines 234, 1063, 1148, 1263, 1323, 1397 and 1400
    with that person's modpack, copy and settings, and the comments of Surface.cs, Climate.cs,
    Trees.cs, GroundArt.cs, VitalsFade.cs and VoxelGame.cs; the modpack is never named where a
    reader can open it, Sky.cs naming its shader alone, so they become the reference modpack, the
    reference copy and the reference modpack's settings, the reference folder being the name the
    records give the copy under .ref, unless the pack's name is given first. Twenty-two colon joints
    stand in comments, InventoryWindow, GuiArt, VoxelGame, ModeSwitcher, SlotMoves and BlockShape
    two or three each and one each in Gui, Sky, SectionMesher, PlantArt, GroundArt, Climate, play.sh
    and mob-pairs.py, and three in the README at lines 433, 574 and 1443; AmbientParticles.cs
    paraphrases emission five times. Clean of dashes, banners, the forbidden verbs, clefts and
    history phrasing, the spaced hyphens arithmetic and the semicolon joints left, STYLE.md not
    forbidding them. One commit. Done at `1767348` on 2026-10-25, forty-four replacements and the
    greps clean, the pack called the reference modpack.

95. **The world's sections become mesh entities the engine draws (Decision 43).** Each meshed
    section becomes an entity with the engine's Mesh, Material and Transform and a component of the
    game's naming its column and height, made and replaced by the main thread when a worker's mesh
    arrives as today, despawned when the section leaves the reach, so the engine's model pass draws,
    culls and keeps them with its change detection and the game's loop of DrawMesh over every
    section, with item 90's kept flood, goes; the lava's and the plants' parts as entities of their
    own materials. The frame read before and after at item 93's views and the draw count test kept
    on the engine's count. Generation and meshing stay on the workers. One or two commits, after
    item 94. The coder's reading at 21:19 on 2026-10-24 found the engine's mesh entity path short of
    a section in six ways, no shader on the ECS Material, a Mesh of positions, normals and uvs
    alone, looks forgotten past 4,096, a mesh not drawn in a frame freed, no visibility or
    shadow-only component and the entity draws not in the profile, which the engine takes as its
    item 108 through ASKS.md; the flood stays as the game's visibility, a Visible component the
    engine honors once it has one, and the shadow-only draw of item 123 the ShadowOnly component.
    Waits on the package carrying the engine's item 108; item 96 taken before it. Read on 5.2.313's
    source at 17:06 on 2026-10-25, a gap and a split, taken. A mesh entity cannot say it casts no
    shadow or bounces no light, the ECS Material having neither flag and MeshEntityDraws building
    every entity's draw with both true, while four of a section's seven parts need one through
    ModelMaterial today, the see-through glass and ice, the water and a pack's cut-out glass casting
    no shadow, which also keeps them out of the field that would hold a window as solid, and the
    plants bouncing no light so the field holds no grass. The three parts needing neither move first
    in one commit, the solid, the solid cut-out and the molten, each an entity with its uploaded
    mesh, a white Material with the terrain shader and its alpha mode, a Transform, Visible from the
    flood and ShadowOnly for the casters written where they change, and a SectionPart component
    naming the section and the part; the cut-out plants, the see-through, the water, the cut-out
    glass, the lamps' instanced cubes and the dome stay on DrawMesh; an ASKS.md entry in the
    engine's tree asks for CastsShadows and BouncesLight on the ECS Material or a component for
    each, the engine's item 124 under Decision 68, after which the rest move; the draw count test
    reads entities.drawn plus the draws left, and the frame is read three ways, 5.2.291, 5.2.313
    with DrawMesh and 5.2.313 with the entities, so the package's own change shows apart. nuget.org
    served 5.2.313 late, its flat container at 404 at 17:04. The first commit at `b7608c7` on
    2026-10-25, the three flagless parts as entities on 5.2.313, the grove's frame from about 43 ms
    to 23.6 at 1080p with the scene pass halved, the readings in the devlog's note; the four flagged
    parts move on the package carrying the engine's item 124, which closes this item. The README's
    costs at `cc49c81` on 2026-10-25, the distance table with the probes on keeping 60 a second at
    12 columns where it lost it before, the flights left for a quiet GPU. Read again on powersave at
    `c802dea`, 12 columns at the edge of 60 a second, the README saying so with both frames and the
    note naming the profile's doubt. Done at `a4d383c` on 2026-10-26 with the last four parts, every
    section part a mesh entity with its flags on 5.2.316, the picture the same and the frame as it
    was, the DrawMesh loop gone.

133. **The leaves let the engine's light through, and item 117's floor is read again beside it.**
    From the engine's item 100 and the word of 2026-10-26 at 00:36. The engine's LightThrough on
    both materials sets the share of light passing a unit of a surface's depth, the cascades' and
    the screen probes' rays crossing up to four such surfaces, and its second part at `a16227a7`,
    not yet in a package, sets a crossing's reach where the ray stops and lets the merge see past
    leaves, so a canopy's floor takes about a third of the sky light it held back where it took
    none. Two parts. On 5.2.316 the leaves' and the plants' materials take a LightThrough from the
    pack's leaf opacity, about a half for a leaf block a unit thick, the rain forest's floor and the
    oak grove's read before and after with item 117's sky-level floor as it stands, and the cost
    read at the grove at 1080p at High; on the package carrying `a16227a7`, the same readings again,
    and item 117's floor lowered or taken out where the engine's light carries the canopy on its
    own, the floor kept only where the readings say it still must, with a before-and-after picture
    of the rain forest's floor at noon. Two commits. A snag read at 00:40 and the way through,
    taken. With the three default packs every leaf tile is fully opaque, the seven leaf kinds
    keeping all 256 pixels at the 0.5 cutoff, so the leaves sit in the Solid part with the stone and
    a share from the pack's opacity would be 0, while a share on the Solid material would let light
    through stone. The leaf blocks' faces go to an eighth part, Foliage, drawn and cast as the Solid
    part is through a material of its own with LightThrough set, its share 0.5 a unit as the
    source's leaves let light through, and where a pack cuts its leaves one less the tiles' mean
    coverage squared for the two faces a ray crosses in a block, no less than 0.5; the plants'
    material takes one less their tiles' mean coverage, which matters to the screen probes' rays
    alone since the plants bounce no light. A part more means an entity more a section where it has
    leaves, about 600 at the grove, and the leaves leave the Solid mesh the field keeps. The first
    part at `48e5bea` on 2026-10-26, the Foliage part with its share, the floors unmoved on 5.2.316
    as expected and the global illumination's cost at the grove a lead of 3.6 to 7.0 ms on one pair;
    the second part on the package carrying `a16227a7`, the floors and the cost read on a quiet GPU
    and item 117's floor lowered where the engine carries the canopy, the cost's lead going to the
    engine as an ask if it holds. Done at `1944378` on 2026-10-26 with the second part on 5.2.319,
    the floor lowered to 0.65 of item 117's and not taken out, the engine alone carrying the canopy
    to 0.74 of the open shade and the two together to 0.80, the rain forest's floor pictured, the
    crossings' 2.7 ms at the grove to the engine as its item 127.

89. **The clouds are not drawn black by the fire's uniform.** Found at item 46's retake on
    2026-10-24: in every daytime view the clouds' undersides drew a dark navy near black, where the
    pictures of `7039d5a` from 2026-10-21 showed them light gray. The cause, read by the coder:
    `ee557b3` gave the terrain shader a `flames` uniform naming the fire's tile, whose faces draw at
    twice their color, and the clouds' own copy of the shader never sets it, so it reads 0, the
    clouds' tile, and every cloud face drew at nought times its color, black, hazed to navy, tops
    and undersides alike. The mend: the uniform holds the tile plus one, 0 naming none, so a copy
    that sets nothing matches no face, and the clouds keep their own shading; a test reading a
    cloud's underside at a known daytime view against a bound, failing without the mend. One commit,
    small, before item 46's final run so the README's pictures show lit clouds. Done at `cfdc298` on
    2026-10-24.

55. **The plants cast shadows, cut by their texture (Decision 27).** Decided shortly before 07:12 on
    2026-10-21, on the README's pictures, the grass blades and the plants are to cast shadows too,
    fitting as they are pixelated. The cutout mesh casts no shadow today, so the grass, the flowers
    and the wheat throw none. With it casting, the sun's shadow map must cut each blade by its
    texture's alpha, or a blade casts a whole quad's shadow; read first whether the engine's shadow
    pass draws a mask material through the program's own shader, which samples the atlas, or through
    its own depth shader without it, and where it cannot cut them, ask the engine in its ASKS.md
    with the reading and keep the plants shadowless until the package. The shadow's pixels step with
    the sun's map, which is read as fitting the look; the plants stay out of the scene's field, so
    the global illumination is untouched. Tests: a blade's shadow on the ground read from a
    capture's pixels as narrower than its quad, and a capture of a meadow at a low sun in the
    record. One commit, small, or the ask. Read at `bc02459` on 2026-10-21: the shadow pass does not
    cut a masked surface drawn through a program's shader, so the entry is in the engine's ASKS.md
    as its item 79 and the game takes the shadows when the package brings the cut; the capture
    committed. The engine's item 79 is in 5.2.215, pushed to the feed at 21:04 on 2026-10-23; this
    item follows item 88 once the feed lists it. Done at `d7c4892` on 2026-10-24 on 5.2.215, the
    departures and the cost in the head.

67. **The inventory screens take no player key, draw a dragged stack every frame, and dropped items
    wear the pack's pictures.** Reported on 2026-10-23 (Decision 34), in the crafting HUD Ctrl
    crouches though a screen is open, a stack held by the left button or dragged with Ctrl is not
    shown moving until the button is released, and dropped items are not textured with the resource
    pack. The mend in three parts: while a screen is open no key of the player's acts, crouch, jump,
    sprint and the hotbar among them, as the source's screens take the keyboard; a stack picked up
    or dragged is drawn under the pointer every frame the button is held, the slots it passes marked
    as the source marks them; and a dropped item in the world wears the pack's picture of its block
    or item as the blocks do, the game's own art without a pack. Tests: a key pressed with a screen
    open moving nothing, the dragged stack's place read from the frame mid-drag, and a dropped
    item's pixel with the test pack. One or two commits. Done at `073d84b` on 2026-10-24, the
    departures in the head; the held stack's lag stays for a look by eye on a real mouse.

68. **Breaking a block shows the cracks, throws particles of its side texture, and keeps its
    progress across jumps.** Decided on 2026-10-23 (Decision 34), breaking shows no texture for
    breaking, which is asked for with Bare Bones loaded and the game's own behavior without,
    breaking should throw the side texture as particles dropping as the source does, and a block out
    of reach hit between jumps should keep its breaking state so it breaks over several jumps as the
    source lets it. The mend: the ten destroy stages drawn over the block being broken, the pack's
    destroy_stage pictures with a pack and the game's own set without; particles cut from the
    block's side texture thrown at a hit and at the break, falling and fading as the source's do;
    and the progress on a block kept while the button stays held and the same block is aimed again
    within the source's delay, so a block above reach breaks over a few jumps. Tests: the stage
    picture on the block at a known progress, the particles' count and fall, and the progress kept
    across a lost and regained aim. Two commits, the cracks and the particles, then the progress.
    Done at `8bc0796` and `7da1d96` on 2026-10-24, the departures in the head.

69. **Felling a tree takes its logs and leaves, some logs falling, and gravity blocks fall (Decision
    35).** Decided on 2026-10-23 (Decision 34), trees should break all wood and break leaves with a
    particle effect dropping the leaves, with some wooden blocks dropping down still, and in that
    regard sand and the like dropping. The mend: breaking a log the trunk stands on fells the tree,
    its logs breaking from the cut upward with their drops, its leaves decaying over a few seconds
    with leaf particles and the source's drops, and a share of the logs falling as blocks that land
    and settle; sand, red sand, gravel and concrete powder fall as blocks when unsupported, as the
    source's gravity blocks do, landing as blocks or breaking on a non-solid as the source has it,
    the cobble named read as that list unless decided otherwise. Tests: a tree's logs and leaves
    gone after the cut with the drops counted, a falling log landing, and a sand column falling onto
    a floor. Two commits, the gravity blocks, then the felling. Done at `cc4f29a` and `45b4b93` on
    2026-10-24, the departures in the head.

72. **Fire, a block that burns, animated and emissive (Decision 34).** Reported on 2026-10-23
    (Decision 34), fire is missing and could show the radiance cascades. The mend: a fire block kind
    with the source's animated picture through item 39's frames, emissive into the field so the
    cascades carry its light, lit on a flammable block's top by a flint and steel item and by TNT's
    blast, burning out in the source's time where nothing feeds it and spreading to a flammable
    neighbor at the source's chance within a small radius, so a house burns and a stone floor does
    not; a test of a fire placed on planks spreading and one on stone dying, and a night capture of
    a fire lighting a room. One commit, after item 73. Done at `ee557b3` on 2026-10-24, the
    departures in the head; TNT's blast lights nothing as the source's does not, at `edbc3b2`.

73. **TNT and the creeper's blast.** Asked on 2026-10-23 (Decision 34), TNT and the creeper's
    explosion. The mend: a TNT block primed by a hit with flint and steel or by a neighboring blast,
    flashing white over its fuse of four seconds as the source's does, then exploding with the
    source's ray-cast destruction at power 4, blocks in the blast dropping at the source's chance,
    the player knocked back and hurt by distance; the creeper swelling and hissing for a second and
    a half within reach of the player and exploding at power 3, as the source's does, with smoke and
    flash particles; the blast's light a flash the cascades carry for a frame or two. Tests: a TNT's
    crater's counts, the creeper's swell and blast on the flat world, and a capture. Two commits,
    TNT, then the creeper. TNT at `1eeeeff` on 2026-10-24, the departures in the head; the creeper's
    commit next, its hiss waiting on the game having sound. The creeper at `cba35ca`, the cloud an
    engine particle sheet of the pack's frames and the flash a point light; item 73 done.

74. **A held light lights the ground around the player.** Decided on 2026-10-23 (Decision 34),
    holding a light should dynamically illuminate the area around. The mend: a torch, lantern,
    glowstone or other emissive block held in hand gives a point light at the player with the
    block's color and reach, following the player each frame, and the field taking it so the
    cascades carry it; the same for a dropped one lying in the world; a test reading the floor's
    light at night with a torch in hand against none. One commit, small, after item 75. Done at
    `2aea2bf` on 2026-10-24, the light casting shadows, the departures in the head.

75. **The player's hand and held item in view.** Decided on 2026-10-23 (Decision 34), the player's
    hand and the items should be displayed in view. The mend: a first-person arm and the held item
    drawn in the camera's frame as the source draws them, a block as a cube, an item as its picture
    extruded, swinging on a hit or a use and bobbing with the walk, drawn after the world with its
    own depth so it never cuts into walls, hidden with the HUD under F1; a test reading the hand's
    pixels at a known view and a capture. One commit. Done at `1dce7b7` on 2026-10-24, the
    departures in the head. The empty arm goes by decision, Decision 38, at `adefc9d`.

78. **Grass as the source places it, with the two-block grass.** Reported on 2026-10-23 (Decision
    34), there is far too much grass, with the ask for the source's balance, and the source had
    two-block-high grass. The mend: the plains', forests' and other biomes' grass and flowers placed
    by the source's patch features at the source's counts and chances, read as numbers from its
    worldgen data, rather than by the game's density, and tall grass as a two-block plant with its
    two pictures; a test of a plains chunk's grass count within the source's range and the tall
    grass standing on two blocks; the plains village picture taken again. One commit. Done at
    `b032e76` on 2026-10-24, the departures in the head.

79. **Swimming as the source has it, no launch out of the water.** Decided on 2026-10-23 (Decision
    34), the player should not jump out of the water automatically or with such force, with the ask
    for the source's way, where jumping in water lifts the player to a high point, lets them sink
    and rise again, and lets them climb out at the high point. The mend: the source's water
    movement, a swim-up impulse on jump held, gravity and drag in water as the source's numbers give
    them, the bob at the surface, and the step out onto a block at the surface's high point, no
    launch; a test of the player's height over time in deep water with jump held, bobbing within the
    source's band, and the climb out at a bank. One commit. Done at `526f20b` on 2026-10-24, the
    departures in the head; a one-block bank climbable is the owner's to choose.

80. **Villages spaced as the source spaces them.** Reported on 2026-10-23 (Decision 34), there are
    too many villages too near. The mend: villages placed by the source's structure set rules, a
    spacing of 34 chunks with a separation of 8 and the salt's random grid, so two villages never
    stand within eight chunks and most are a few hundred blocks apart; a test counting villages over
    a 64-chunk square within the source's bound. One commit, small. Done at `9b25aa3` on 2026-10-24,
    the game's own hash in place of the source's salted random.

81. **Biomes placed by the source's climate, so a badlands does not border a forest.** Reported on
    2026-10-23 (Decision 34), a mesa biome stands right next to green land, with the question
    whether that fits the source's logic and whether temperature, weirdness and the other values are
    accounted for as the source does. The reading first: the game's biome choice against the
    source's multi-noise, temperature, humidity, continentalness, erosion, depth and weirdness with
    the source's parameter table, said in the commit; then the mend, the source's table taken as
    numbers so badlands sit in hot dry land beside savanna and desert with the source's transitions,
    or, where the game keeps its own climate, a border rule keeping badlands off forests. A test of
    a map's biome pairs holding no pair the source never makes. One commit. Done at `8da806a` on
    2026-10-24, the game's climate kept and the choice mended.

83. **Glass without the diagonal streaks, its borders connecting across neighbors.** Asked on
    2026-10-23 (Decision 34), that the glass not have the diagonal streaks and that the borders
    connect so the middle of two glass blocks carries no border, as connected glass has it. The
    mend: the game's own glass picture redrawn without the streaks, and glass faces drawn with
    connected borders, a face's edge border left out where the neighbor along that edge is glass of
    the same kind, as the modpack's connected textures do, the pack's border pieces read where the
    pack gives them and the game's own otherwise; a test of two glass blocks side by side showing
    one border around both. One commit. Done at `dd64fd0` on 2026-10-24, the departures in the head.

90. **The frame at a render distance of 12 comes back under 16.7 ms with the taller world, the main
    thread's draws read first.** Item 26's readings on 2026-10-24: at 12 the frame stands at 17.5 ms
    with far land off and up to 23.4 with it, where the GPU takes 14.3 to 15.0, so the main thread
    binds the frame, drawing the taller world's sections, 5,064 at the cherry grove view, one draw
    each. Read first where the main thread's time goes at that view through the engine's profile,
    the draws' recording, the sections' culling, the far tiles and the rest, and say the numbers;
    then the mend by the reading, the sections of a column drawn as one through the engine's mesh
    over buffers with an indirect draw, or the draws recorded once and kept while nothing changes,
    whichever the reading names, holding the frame under 16.7 ms at 12 with far land at 32 on the
    development machine's GPU; a test of the draw count at a known view against a bound. One or two
    commits, before the items in order. Done at `ba60ae4` on 2026-10-24, the flood kept until its
    key changes, the numbers in the head; the GPU at 14.5 ms is the engine's bound, no item.

91. **The screen probes stay off by default (Decision 42).** Seen in play on 2026-10-24, black
    spreading across the screen in places, gone with the screen probes off, and the light spotty,
    noisy and unstable on movement; the black is the engine's item 90, a glow light's non-number the
    probes' history keeps, mended in the engine's tree and in the package after 5.2.215, and the
    spots, the noise and the shift are the engine's sixth term, the probes' own, read under its item
    71 and left as a limit. So the game calls SetGlobalIlluminationScreenProbes(false) at its start
    and keeps its setting for turning them on, with the capture of a view with the probes off; the
    play check that reads the light holds as it stands or moves to the probes off. One commit,
    small, first. Done at `23aa4ff` on 2026-10-24, a checkbox and no e3d verb. Superseded on
    2026-10-25 by item 131, the probes on by default (Decision 48).

92. **Far land goes (Decision 41).** Found in play on 2026-10-24, the far land looks bad, and it is
    to be removed. FarLand, its rings, tiles and skirts, FarCache and the far folder of the save,
    the generator's Surface where the far land alone read it, voxel.far, the settings slider, the
    play check of the far land and the README's words and costs table go, the taller world and the
    sections by reach of item 26's first two parts staying; the haze ends at the near land as before
    item 26's third part; the README's world pictures taken again through pictures.sh without far
    land. One commit, after item 91. Done at `2263618` on 2026-10-24, the pictures taken again, old
    saves' far folder left unread.

38. **The screen probes off by default, and the cascades' faults given to the engine with repros
    (Decision 16).** The decision of about 01:00 on 2026-10-21: the game is not to enable the screen
    probes, whose look is pixelated, the cascades without them being the better option for the
    game's art, and the cascades still show faults. Two parts. First, now: an entry in the engine's
    ASKS.md as Decision 26 has it, with a repro of each fault named, the world's seed, the place,
    the hour, the commands that put the camera there and a capture, the light block's light leaking
    through the thin gap at the corner of two columns that hold it off, a penumbra on the ground
    with zig-zags, the screen probes shifting on movement, z-fighting between the global
    illumination and the opaque textures where it shows, and the noise, read in the game at
    `28eb41c` on 5.2.190 with `voxel.gi high`, and beside each what `gi.screenprobe off` changes;
    the engine's item 71 takes them. Second, when the package brings the engine's call that leaves
    the screen probes out: the game calls it at start, off by default, a setting and `voxel.gi`
    taking a word for them, the README's paragraph on the light saying why, and the measurement rows
    and the captures made again without them. The entry also reads the model pass, `gpu.hdr_scene`
    in the profile, with `gi.toggle screen off` against on, in a scene of many light blocks in view,
    a village at night or a room of forty glowstones, since without the screen's probes the engine
    takes the glow lights at each pixel and the cost grows with the lights in view, 0.3 to 1.0 ms on
    four emitters in the engine's reading, which the engine's item 71 bounds. The first part before
    item 37, the second when its package is listed. First part done at `404ab74` on 2026-10-21 with
    the entry in the engine's ASKS.md, as the head says; the second part waits on the package. The
    second part is moot since 2026-10-25, item 131 turning the probes on by default (Decision 48)
    and the engine's item 103 taking the call away, so the item is done with its first part, closed
    on 2026-10-26.

54. **The game takes the engine's F1 and F3, its HUD in the hidden scope and its developer's parts
    as overlay sections (Decision 26).** Asked shortly before 07:05 on 2026-10-21 for F1 to hide the
    interface as Minecraft does, as an engine feature, F2 the screenshot, and F3 a debug overlay
    like the newest Minecraft's, sorted in parts logically with modules a game adds, world
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
    package comes. From the engine on 2026-10-24: the package after 5.2.215 has no
    SetProfileWindowKey, F3 showing the debug overlay in its place through SetDebugOverlayKey(Key),
    Key.Null turning it off, and SetUiHiddenKey(Key) beside it for F1, so the game's
    SetProfileWindowKey(Key.Null) of item 55 becomes SetDebugOverlayKey(Key.Null) when that package
    comes, and SetUiHiddenKey(Key.Null) where it keeps F1. On 2026-10-26 the F3 window's parts found
    their homes, Decision 50, the World part's kind, seed and opening going to item 134 with
    voxel.world keeping them meanwhile. Done at `7f59530` on 2026-10-26, the F3 window gone into the
    engine's overlay sections and a Debug Settings screen, the HUD and the hand under the engine's
    F1, Decision 50, the World part to item 134.

135. **A leaf block's faces take the sky floor at level 15 as at 14.** From the report on item 133's
    rain forest picture on 2026-10-26, the forest floor showing black sides and blocks under the
    trees. The coder's reading at seed 1's rain forest floor at noon found every near-black pixel a
    jungle or oak leaf face, south or west, 5 to 10 blocks off, facing air at sky level 15 inside a
    pocket of the canopy open to the sky straight up; the screen probes change nothing, the cascades
    give such a face about nothing since the rays leaving a vertical face in a narrow pocket meet
    leaves within a block or two and the sky at grazing angles alone, and the game's floor gives it
    nothing since it fades out from 14 to 15 so a wall on open ground keeps the engine's shade, the
    one face at sky 14 lifted to 0,19,0 by the floor and 0,0,0 without; the near-black share of the
    view 0.138 at `a4d383c` and 0.126 as committed, so it stood before item 133, and the canopy's
    floor is not black at any depth, the grass under 2, 3, 4 and 6 leaves 33.1, 27.8, 26.1 and 27.8
    against the shade's 43.5. So leaf blocks' faces carry their sky level at most 14, the floor
    applying to them at 15 as at 14, only leaves changing so a wall on open ground keeps its shade,
    and the haze's share at a leaf moving from skyBrightness(15) to (14), which the eye's own level
    overrides in the open; the near-black share of that view read before and after, the picture with
    the near-black pixels marked beside the mended view, and a test of a leaf face at sky 15 reading
    the floor's lift. One commit, small, before item 134. The engine's part, a vertical face in a
    narrow pocket of a canopy taking about no light from the cascades, is read as the light such a
    pocket gets and not a fault, no ask sent unless the mended view is read otherwise. Done at
    `0ba4cb4` on 2026-10-26, the leaf faces' sky level 14 at most, the near-black share of the rain
    forest view from 0.126 to about nothing, the marked view pictured beside the mended one.

134. **A world screen in the pause menu, the saves listed to open and a new world begun.** From item
    54's move of the F3 window on 2026-10-26, Decision 50. The pause menu gains World, listing the
    saves under the save root by name with the open one marked, each opened by its button, and New
    World beginning an Overworld or a Flat world with a seed, typed where the engine's interface has
    a text field and otherwise stepped by buttons with a random one offered, the game saving and
    leaving the open world before it opens another as voxel.world does; a test opening a second
    world from the screen and reading its seed, and a captioned picture. One commit, after item 54.
    Done at `5145d2f` on 2026-10-26, the World screen listing the saves and the New World screen
    with a typed seed, the engine's missing text field to ASKS.md.

97. **The game's entries in the engine's ASKS.md hold no colon inside a running sentence.** Reported
    at about 10:3x on 2026-10-24, some documents join sentences with a colon and STYLE.md forbids
    it; the entries this session wrote in the engine's ASKS.md hold 24 such lines, found by
    STYLE.md's awk over the Markdown files outside their code. Each is recast as STYLE.md says, a
    sentence of its own or the same sentence recast, a label at a line's end left as it is, the
    Review lines being the reviewing session's; the recast goes into the engine's tree as the
    entries do and the engine's next batch carries it. Small, after items 92 and 93 and before item
    94. Done in the engine's tree at 10:4x on 2026-10-24, 21 lines recast, the 15 headings with a
    comma after the game's name and six sentences split or recast, the engine's next batch carrying
    it; the file's header lines are the engine's sweep's, item 93 there. The engine's batches of
    2026-10-24 and 2026-10-25 carried the recast, and the one join left, at the update's rule, was
    recast in the engine's tree on 2026-10-26; done that day.

26. **The world's layout: sections streamed, meshed and saved in three dimensions with a taller
    world, and far land drawn at a coarser level by distance, the blocks' size kept.** Reported, a
    column is 16 by 16 by 128, to be made cubic at 16 or 32 or given a level of detail, where an
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
    four commits. The reading before any commit, at 07:10 on 2026-10-24 on seed 1 at three places: a
    column generates in 9 to 11 ms and lights in 1.2 to 1.8, 100 KB in memory, a section meshes in
    0.6 to 1.2 ms, the surface alone costs a tenth of a column; the taller world of 24 sections from
    minus 64 to 320 at an estimated 15 to 20 ms a column and three times the memory, 147 MB at a
    distance of 12, old saves read as 0 to 127 with the ground below generated and the format given
    a version; a sphere of sections at 8, 12 and 16 keeping 47, 65 and 78 percent of the cylinder, a
    column generated whole once and its far sections saved and let go, an unloaded section opaque to
    its neighbors; the far rings by doubling, 12 to 24, 24 to 48 and 48 to 64 columns, cells of 2 to
    the k blocks in tiles of 16 by 16, from saved tops first and the generator's surface otherwise,
    boxes at their own place in the top block's average color, flat lit, hazed, no shadows, field,
    plants or mobs, the surface sampled to 64 columns in under a second on the workers. Four commits
    as said. Deepslate comes with part 1 as the source lays it below 0, its ores and caves with item
    27; the far reach in columns and the rings by doubling as read. Part 1 at `d2f8b0b` on
    2026-10-24, the costs in the head. Part 2 at `1b53cee`, the numbers in the head. Part 3 at
    `3d7b1ed`, off by default until part 4. Part 4 at `a528c79`, far land on at 32 by default, the
    readings in the head; item 26 done, held for the push. The world pictures are taken again with
    far land in a small commit after item 90's reading. The pictures at `3a99729`. The play job's
    climb read at 09:4x from the runs' logs: the 31 checks took 2,090 s with far land against 1,452
    without, every check 30 to 45 percent slower, since far land's tiles are drawn every frame by
    lavapipe's software rasterizer with the near land at two to four columns; so with item 27's part
    B play.sh keeps far land off from the start and a short check turns it on at 6 over the last
    biome and reads tiles drawn within 300 frames, the other checks' readings unchanged, 145 to 115
    s locally. Done at `a528c79` and `3a99729` on 2026-10-24 with the play job's climb answered
    under item 27's part B, archived on 2026-10-26.

27. **Caves as the source's newer ones, cheese, spaghetti and noodle, with aquifers.** Reported,
    caves are a big one, the source's newer ones are beautiful. The world has tunnels where two
    noises are both near zero, caverns where a third is high below a depth, and ravines; it gains
    the source's three kinds as numbers, cheese caves from a large three-dimensional noise's low
    values, spaghetti tunnels from two noises thinned by a third, noodle caves thinner still, and
    aquifers setting water and lava levels in caves by depth, with the deep sections item 26 gives
    them, and a cave's lighting from the sky above through its openings. After item 26. Two commits,
    the caves, then the aquifers. The lush caves of the source, moss, glow berries hanging from the
    roof and vines over clear pools, join the kinds (Decision 21). Found by item 26's second part on
    2026-10-24: a cave at minus 36 under ground at 64 takes the sky's light through global
    illumination, its floor reading 13 to 26 at noon against 0 to 11 at midnight, since the engine's
    field reaches 64 up from the eye and a ray leaving it counts as sky; the engine's item 72 gives
    the field a far cascade a ray reads before the sky, in the package after 5.2.215, so this item
    takes it when that package comes and reads the cave again. Part A at `d8a05a1` on 2026-10-24,
    the caves and the ores, the departures in the head. Part B at `9cb81ad`, the aquifers with lava,
    the departures in the head; the lush caves part C. Part C at `c8dbbee`, the departures in the
    head; item 27 done, the deep caves' sky light read again when the engine's package comes. Seen
    at about 10:5x on 2026-10-24, the sun's light leaking onto the tops of blocks in caves; the
    engine's item 94 reads whether it is the sun's map, whose cascades may leave the ground above a
    cave's view out of their depth, or the field's top, which item 72's far cascade answers; the
    game reads it first at a cave of the kind reported, the tops' light with the shadows off and on
    and with global illumination off and on, and when the package comes it sets SetSceneField's far
    cascade so a cave's rays meet the ground above before the sky. Added at 10:51, the sun shows in
    a cave at a distance, an opening far off the likely way in; past the engine's shadow distance of
    150 nothing casts a shadow until item 72's fill cascade, in the package after 5.2.215, so the
    far parts of a cave are sunlit on 5.2.215 whatever the map's range near the eye, and the game's
    reading tells the near from the far. The cave reading of 10:59 is written under the engine's
    item 94, the near sun the map's and the far sun and the blue item 72's. Added, the global
    illumination's qualities move where a black cave's ambient fog color begins, near at Low and far
    at High, and it shows at a distance even at High; that is the counted cascades' reach and the
    sky past them, and the game sets the engine's far cascade through SetSceneField when the package
    comes, so the far rays of a cave meet the ground before the sky at every quality, read again
    then. The package part at `758e9df` on 2026-10-24, 5.2.281 pinned with the keys renamed, the far
    cascade left off and the cave's readings unchanged, said in the head; the far cascade set once
    the engine's item 102 mends its slab, and the cave read again with the engine's item 101. The
    pin of 5.2.283 at `3d70b20`, the grove's frame 19 to 22 ms where 5.2.281 read 33 to 35 and
    5.2.215 15 to 18, the cave unchanged. The pin of 5.2.291 at `9dad43e` on 2026-10-25, the grove's
    frame as 5.2.283's. The last part taken on 2026-10-26 on 5.2.319, the far cascade set through
    SetSceneField since the engine's item 102 mended its slab in 5.2.313, the deep cave read again
    at every quality for its far walls meeting the ground before the sky, and the cost at the grove,
    with a captioned picture. Read at 06:3x on 2026-10-26 with SetSceneField(4, 0.25, 2, 32768,
    192), the deep cave's far walls at Low darkening by about a quarter, their rays meeting rock
    where they met the sky, Medium and High unchanged with the tops at 90 blocks black already, and
    the grove at 1920 by 1080 paying 1.1 ms of global illumination and 0.5 to 1 ms of the scene
    pass, 1.6 to 2 ms of the frame at both qualities; so the far cascade ships off by default behind
    a Far Cascade toggle on Render Settings, its reach following the render distance, the README's
    options table saying it is for Low underground, the readings in a note and the cave at Low
    pictured off and on, and the item closes with it, the choice open to the player in the game.
    Done at `f1aa450` on 2026-10-26, the far cascade off by default behind Render Settings' Far
    Cascade with its reach the render distance, the cave at Low pictured off and on; the item closed
    with every part.

42. **Water that mirrors the sky and the shore, with the bed seen through it (Decision 21).** The
    look aimed at shows calm water mirroring the sky and the shore and clear pools in caves. The
    game's water gains a surface of its own: the sky and the land across the water reflected in it,
    through the engine's reflections where they reach a flat surface and a planar reflection of the
    game's own where they do not, read first and said; the bed seen through the water, refracted a
    little and darkened with depth by the water's color, so a pool reads clear and a lake reads
    deep; a slow wave in the normal so the reflection moves; the water's edge drawn where it meets a
    block without a hard line; and the view from under the water tinted and fogged as the source's
    is. Captures of a lake at noon and at a low sun and of a cave pool in the record, beside the
    modpack's own pictures. Tests: the reflection's picture read in a capture's pixels where the sky
    should mirror, and the bed's color darkening with depth in a laid pool. After item 27. Two
    commits, the reflection, then the depth and the edge. The reading at 22:47 on 2026-10-24 on
    5.2.283, the engine's traced reflection for a glossy surface under roughness 0.5 going through
    the frame before's picture, the field's cascades and the GPU's rays, the box reflection probes
    for a room, and no planar mirror and no reflection call for a surface the transparent pass
    draws; the water at roughness 0.05 read live looks as it does rough, the river in the haze with
    no shore or sky in it and the pool by the red wall 143, 128, 183 where the wall's image would
    lie, a blur and not a mirror. The call is the coder's, the mirror asked of the engine as its
    item 112, a planar reflection at a height a program's material reads as a texture, and the cheap
    part built first as this item's second commit, the sky's color in a fresnel by the view's angle
    from the environment map, the bed seen through the water darkened with depth, the edge and the
    view from under, with the shore's mirror when the engine's plane lands. The second commit at
    `9d4b295` on 2026-10-24, the depth, the edge and the sky's fresnel, the water a mesh of its own;
    the shore's mirror with the package carrying the engine's item 112. Done at `7c36ec8` on
    2026-10-26 on 5.2.322, the shore's mirror through the engine's plane at the water's top behind
    Water Reflections, the river mirroring the far bank pictured, 2.6 ms of the frame at 1920 by
    1080.

136. **The sun's shadows soften with the distance from their caster.** From the game's item 43's ask
    of 2026-10-24, answered by the engine's item 111 at `45edfbf6` in 5.2.322,
    SetShadowSoftness(size) as the sun's width across in degrees, 0 by default keeping the sharp
    edge. The game sets the width from a Render Settings slider, Soft Shadows from 0 to 5 degrees
    kept in the options file, its default chosen from readings at noon and at hour 16 at seed 1's
    village at 0.53 degrees, the sun seen from the earth, at 1.5 and at 3, pictured against 0 with
    the frame's cost at the grove at 1920 by 1080 at Low and High, the default the one nearest the
    look of a warm low sun with long soft shadows at under a millisecond of the frame, the README's
    options table saying so and the readings in a note. One commit, after item 42. Done at `6001eed`
    on 2026-10-26, Soft Shadows on Render Settings with 1.5 degrees the default, the village's oaks
    at hour 16 pictured at 0, 0.53, 1.5 and 3, the frame at High 0.6 ms more at the grove.

137. **The seed field edits through the engine's EditText.** From item 134's own field and the
    engine's item 128 at `0754b975`, on the package carrying it. The New World screen's seed field
    becomes a TextEdit the engine's EditText updates, gaining paste, a selection, word moves and
    Tab, IsTextInputActive replacing the game's own key gate on the screen, the box, the text and
    the caret drawn by the game in the pack's look; the pack's bitmap font, ascii.png on a 16 by 16
    grid with its own glyph widths, is not an engine Font, so the caret a click places is measured
    either in a Font built from the sheet through LoadFontFromImage or, where the widths cannot be
    given that way, through a measure the engine's EditText takes, asked in ASKS.md first; a test
    typing and pasting a seed through the screen, and the README's pause menu paragraph. One commit,
    small, after item 136. Done at `b25e668` on 2026-10-26 on 5.2.326, the seed field a TextEdit
    measured in a Font built from the pack's sheet, play.sh typing and pasting a seed.

138. **The README's costs table is read again at the defaults on 5.2.326.** Proposed by the coder on
    2026-10-26. The table read at `cc49c81` no longer matches the default frame, since the defaults
    gained the sun's width at 1.5 degrees, about 0.6 ms more at High, and the water's mirror, 2.6 ms
    where water shows, while 5.2.326's cheaper crossings took about 0.8 ms off global illumination
    at High; the table's views are read again on 5.2.326 at the defaults, alternated against nothing
    on the GPU with the power profile stated, in one docs commit with a note. With it the comment
    above ApplyField in LightSettings.cs, which still says the engine's far cascade is left off at
    the camera's far plane with 74-block cells at about 5 ms, says what stands since item 27, the
    Far Cascade setting reaching the render distance with six-block cells at 1.6 to 2 ms, off by
    default. A pool above a river taking the sky alone, the engine mirroring one plane, is left as a
    lead until it is remarked on. One commit, small. Done at `9c07673` on 2026-10-26, the view
    mended to a flight over the spawn, 12 columns at 20.4 ms and no longer 60 from the open view,
    the shadows' and the mirror's shares read, the comment mended; the frame's return to 60 at 12 is
    item 139.

139. **The frame at 12 columns comes back under 16.7 ms at the defaults.** From item 138's reading
    of 2026-10-26, the open view over the spawn at 1280 by 720 reading 20.4 ms at 12 columns, the
    GPU 16.6 and the CPU's passes 16.7, where item 90 read under 16.7 at `ba60ae4`; since then the
    defaults gained the sun's width, the water's mirror and the leaves' share of light, and the
    passes' CPU time grew with the sections as mesh entities. The time is read for where it went
    since item 90, the CPU's recording of the depth pass, the cascades, the mirror pass and the
    scene pass by draws at 8, 12 and 16 columns with the draws counted, and the GPU's shadows,
    global illumination, mirror and scene; what is the engine's, the per-draw recording of a frame
    drawn four times over for 2,000 and more section draws, goes to its ASKS.md as an entry with the
    table, since the engine's own note under its item 112 named the recording as the mirror's CPU
    share, and what is the game's, a mirror plane set with no water in view or a shadow setting out
    of step with the distance, is mended here; the default stays 12 by item 90's decision until the
    engine answers, and the plants paragraph's claim of 60 at 12 over the plains is read again and
    rewritten. One commit, the reading and the entry, the game's mends with it. Done at `fc0937a` on
    2026-10-26, the shadow fill off as the game's mend with the frame at 12 columns back to 16.7 to
    17.1 ms, the recording's cost to the engine as its item 132, the default kept at 12.

140. **The window opens at its saved size and mode.** Reported on 2026-10-30, the window's settings,
    its size and mode, are applied after the window has loaded, which looks odd, and the start
    should read them somehow if that does not overcomplicate things. Program.cs opens the window at
    1280 by 720 and the options are applied once the game is up; the start reads the options file
    first, the saved size, the window mode, fullscreen or borderless, and the monitor if kept, and
    opens the window with them, falling back to 1280 by 720 windowed where the file is missing or
    unreadable, the rest of the options applied as before; a headless test writes an options file
    and reads the window's size and mode from the start, and the README's options paragraph says the
    window opens as saved. One commit, small. Done at `5c80c99` on 2026-10-30, the departures in the
    head.

142. **The window shows what the devlog's pictures show.** Reported on 2026-10-30, the game run by
    hand does not look like the devlog's pictures, the engine's cascade lighting among the
    differences, where the pictures come from offscreen and headless runs. A reading first, in the
    reported way of running with the same build, window size and mode. The saved options file is
    read and each render setting printed against the default, since a kept setting from an older
    build, the quality, the probes, the far cascade or the sun's width, stands over a new default; a
    window screenshot at the devlog pictures' views and hours is read against offscreen captures of
    the same views, pixel by pixel, the window's swapchain format and tone mapping against the
    capture's; and the first seconds after the window opens and the view under movement are read
    against a still that settled, since the cascades and the screen probes converge over frames.
    Then the mend for what the reading shows, a changed default taking precedence over an older kept
    value where it was never chosen, a window path matching the capture's, or the README saying what
    settles and how long; what is the engine's goes to ASKS.md. First, before item 141 if the
    reading is quick, with a note and the pair of pictures. Added, the game is run with dotnet run,
    the bin folder deleted at times, so the build is current and the changes show, and the sessions
    may open a window on this machine and capture it; the reading runs windowed the same way, a
    Debug build by dotnet run, and compares the window's screenshot with the pictures. The engine's
    item 97 picture was sent as the game's most visible glitch, black specks over every surface,
    which the engine read on lavapipe alone and mended at `1703c54e`, in no package yet; the ninth
    pack run started for it, the game to pin the package, and the reading asks first which Vulkan
    device that run uses, the log's device line read, since specks the RTX never showed point at a
    software or integrated device chosen under the user profile. The device read on 2026-10-30, a
    plain windowed dotnet run with the saved options picking the RTX 4070 Laptop GPU on SDL's x11
    path with a B8G8R8A8Unorm swapchain presented immediately, no device variable set, so the specks
    seen have another cause than the device, the package's mend pending; a side finding for the
    engine, the window 2560 by 1440 from the start under item 140 while the swapchain stays 1280 by
    720 from the loading screen's first frame at 2.35 s until the first present after the load
    reports it out of date at 10.86 s, the engine to recreate the swapchain on a resize before the
    first frame, an ASKS.md entry. Done at `7508d5e` on 2026-10-30, the window matching offscreen,
    the old kept Soft_Shadows 0 read as the default through the options file's version, the frame at
    2560 by 1440 GPU-bound at about 46 ms; the specks wait on the package.

141. **The far cascade is on by default and the shadow distance is 128 blocks.** The decision of
    2026-10-30, Decision 51, over item 27's call of 2026-10-26. LightSettings.FarCascade defaults to
    true and ShadowDistance to 128 where it is 64, Render Settings' toggle and the options file
    following, the README's options table and the costs table read again at the defaults on a quiet
    GPU with the profile stated, the frame at 8, 12 and 16 columns and the shadows' share, and the
    deep cave and the village pictured at the new defaults; a saved options file keeps a player's
    own choice. One commit, small, with a note. Done at `2c458b0` on 2026-10-30, the frame at 12
    columns 17.0 ms at the new defaults.

143. **Sections are drawn in groups of several, so a frame makes a quarter of its draws.** Decision
    52's first proposal, from the reference's merging of 2 by 2 by 2 chunks into one mesh a part. At
    12 columns the game makes about 3,100 draws, some 10 ms of the main thread at 3 to 5
    microseconds a draw a pass, and at 16 columns the CPU's passes reach 22 ms; the sections of a
    column two tall, or 2 by 2 by 2, are meshed as one entity a part, an edit remeshing the group
    and the flood culling going coarser by the group. Read by the draws and the CPU's passes at 8,
    12 and 16 columns before and after, 16 columns expected to fall toward its GPU's 21 ms, and the
    frame's picture unchanged. It overlaps the engine's item 132, which waits behind the cascades,
    so this goes first. One or two commits. Done at `5f31bdf` on 2026-10-30, groups 2 by 2 across
    and one high, the draws at 12 columns 3,150 to 1,380 and the frame at 16 from 27 to 31 to 23 to
    26 ms, the memory's 500 MB more a lead with the engine's ring its larger part.

146. **The columns past 8 draw at double cells.** Decision 52's sixth proposal, large. The reference
    draws coarser octree levels far out with a dithered cross-fade; the game's columns past 8 are
    meshed at double cells and cross-faded. Read by the draws, the triangles and the frame at 16
    columns before and after, and the far land's picture. After item 145, in phases the coder
    proposes. Withdrawn on 2026-10-30 by decision, no far land in the game, Decision 53.

144. **Opaque faces of one kind are merged into larger quads where it halves the triangles.**
    Decision 52's third proposal. The reference merges faces of equal block, occlusion, light and
    tint with border fans against cracks, where the game draws a quad a face, 1.24 million triangles
    at 12 columns; smooth light and the corner shade make many faces unequal, so a reading first,
    the mesher counting the faces that would merge at the spawn and the village, and the merge goes
    ahead only where the triangles halve, lowering the depth, shadow and scene passes. One commit
    for the reading, one more if it goes ahead, after item 143. Counted on 2026-10-30 on a live
    game, the spawn's 356,437 solid quads merging to 318,082 and the village's 329,184 to 267,356,
    only 18 and 31% of whole faces flat at all four corners since smooth light and the corner shade
    vary by corner, far from halving; closed at the count by its own rule, the reading in the
    Replies.

147. **The held light casts no shadow.** The decision of 2026-10-30, Decision 54, over item 74's
    light casting shadows. A light held in the hand, the torch and the like, is a plain point light
    with no shadow map, the ground and the walls around the player lit as before; the play check of
    item 74 reads the light with a torch in hand against none as it does, and the cost of a held
    light at the grove read before and after. One commit, small, before item 144. Done at `b5626c6`
    on 2026-10-30 with the 5.2.333 pin, the held and dropped lights plain, the shadows 0.7 ms less
    at the grove.

148. **The cactus without a pack is a plain inset box.** Reported on 2026-10-30, with the game's own
    pictures the cactus's faces extending a pixel out look weird, since the picture has no
    transparent thorns to carry, with the ask for the same dimensions, a pixel inset on the sides,
    with no face extending past the box, cubic. With the game's own pictures the cactus's sides
    stand a pixel in from the block's faces as before and the thorn's extension of a pixel is
    dropped, the top and bottom as they are, the faces' texture coordinates following the inset; a
    pack's own model keeps its shape. A mesher test reads the cactus's side faces at the inset and
    none past the box, and the desert pictured before and after. One commit, small, after item 147.
    Done at `5d96667` on 2026-10-30, plain pieces where no picture cuts, the pack's source shape
    kept where its side is clear at the edges. Reported on 2026-10-31, the top faces on the right
    were still too big; at `adf5141` the top and bottom are inset a pixel too, each face the middle
    fourteen pixels of its picture through BlockShape.cs's shared middle part, no lid past the sides
    and no lip where two stack, a pack's cut side keeping its model, the pair pictured. Done again
    at `adf5141`.

145. **Streaming spends a time budget a frame.** Decision 52's fourth proposal. The reference spends
    3 ms a frame starting generation and 4 ms meshing, ordered by distance then facing; the game's
    flights read 18 to 19 ms at 8 columns and 21 to 29 at 12. The stream's work on the main thread
    is held to a budget a frame in that order. Read by the flight's frame at its 99th part and the
    stream's main-thread ms a frame before and after. One commit, after item 144. Closed at
    `a272e45` on 2026-10-30 with the reading and voxel.frames, the budget left out, the crossings'
    cost to the engine's item 146 and the game's item 149. The engine's answer, its item 146,
    arrived in 5.2.342 and is pinned at `7bd8f1c` on 2026-10-31, the six steps' 99th part at 12
    columns 49.3 and 85.1 ms to 37.8 and 43.8 and the slowest 62.7 and 95.4 to 49.8 and 58.4, the
    pair and the note under .github/devlog named by this item.

149. **The flood and the casters' search at a crossing leave the frame.** From item 145's reading on
    2026-10-30, a step of one section over loaded land costing the game's flood culling and the sun
    casters' search 9 to 15 ms of the frame when a section is first crossed. The two run off the
    main thread or spread over frames at a crossing, the frame keeping the last result meanwhile;
    read by the same step test, the frame's slowest and 99th part at 8 and 12 columns before and
    after with the engine's field cost set apart. One or two commits. Done at `17704ca` on
    2026-10-30, the flood spread at 2 ms a frame at a step and the casters after it, the step's
    placing peak about halved, the engine's field left to its item 146.

150. **The play job keeps clear of its time limit.** From the coder's note of 2026-10-30, the play
    job's time grown from 68 to 73, 77, 80 and 84 minutes over the last five runs against its limit
    of 90, the defaults of item 141, the new checks and lavapipe's pace among the likely causes. The
    limit goes to 150 minutes at once, since the Actions bill is no concern, and the checks' times
    are read from the last run's log and listed in a note, the growth named check by check; a check
    that reads nothing a cheaper one does not is shortened or dropped with its reason, and the
    README's play paragraph says the job's time. One commit, small, first. Done at `8a1e308` on
    2026-10-31, the limit 150, two waits shortened to what they read, none dropped.

151. **The 250 ms frame after a jump of three sections is not the field's.** Read at `7bd8f1c` on
    2026-10-31 with 5.2.342 pinned, the step after the first three-section jump reads a frame of
    about 250 ms in every round of both packages while cpu.scene_field stays under 20 ms, so the
    cost is the game's own, the sections' meshing, the streamer's load, the flood or the casters'
    search at a jump, read by the profile's rows in that frame. A reading first with the frame's
    rows named, then the mend where it is the game's and small, else an ASKS.md entry where it is
    the engine's. One commit, after the pin's run. Done at `efbb357` on 2026-10-31, a reading. The
    frame is held at the profile's cap; its length is 23.7 to 24.0 s, cpu.mirror 22.5 to 23.0 s, the
    field 2.6 to 6.6 ms, the first frame SetReflectionPlane is set in, the first water 64 blocks
    east of seed 1's spawn; later frames with the plane read cpu.mirror 0.9 to 4.6 ms, and with the
    driver's shader cache at a fresh folder a first run 23.7 s and a second 1.1 s, so the driver
    compiles the mirrored pass's pipelines, which the engine's cache holds in memory alone. The mend
    is the engine's, item 147, the pipeline cache kept between runs and the pipelines made ahead; no
    game stopgap, the engine's mend being near. The note 2026-10-31-item-151-mirror.md.

152. **The package 6.0.11 pinned and its mends read.** Done at `8efd1d0` on 2026-10-31. The engine's
    147 took the first water's frames from 23.7 s to 1.3 with both caches cold and 1.0 warm, its 132
    the four passes' main thread at 16 columns from 7.6 and 7.8 ms to 3.0 and 3.4, 1.5 to 1.8
    microseconds a draw where 3.9 to 4.0, the frame the GPU's at 23.0 ms, and its 150 the first
    frame after a jump 3,480 pixels off the settled frame where 8,421, the deep cave at Low alike;
    the note 2026-10-31-item-152-package-6011.md and the collage
    2026-10-31-item-152-jump-first.webp; the Review lines in the engine's ASKS.md.

153. **The package 6.0.14 pinned and its mends read.** Done at `23ba89e` on 2026-11-01. The engine's
    151, static mesh buffers in device memory, took the GPU's passes at item 139's view from 12.8 to
    9.3 ms at 8 columns, 17.4 to 11.8 at 12 and 22.8 to 13.6 at 16, the shadows from 2.2 to 2.6 ms
    to under half, the depth pass at 16 from 3.1 to 0.19, the mirror from 3.0 to 1.1 and the scene
    from 8.8 to 6.6, global illumination at 5.2 either way, so the frame holds 60 at 8 and 12
    columns and reads 16.9 ms at 16 where 22.8; the four passes' CPU unchanged at 1.7 microseconds a
    draw; the spawn view identical within 3 of 255. The note 2026-11-01-item-153-package-6014.md,
    the README's costs rewritten, the Review line in the engine's ASKS.md under the cascades' entry.

154. **The grove's global illumination at the game's settings against the engine's budgets.** From
    the engine's item 140 on 2026-11-03, read in a scratch build of this game's source against the
    engine's 5d0c44e4 at seed 1, the spawn, 16:00 and distance 12, on the quiet RTX 4070:
    gpu.global_illumination at 1080p, 1440p and 4K reads Low 4.49, 4.82 and 5.61 ms, Medium 6.79,
    7.01 and 8.01, High 8.05, 8.25 and 9.33, the distance to the reference 0.10 at every size; the
    engine's budgets at its own settings are under 2, 3 and 4 ms at 1080p, and the game's source of
    2026-10-25 read 2.3 at High there, so the rest is the game's choices since, the screen probes
    turned on at about 3.3 ms of High's 8.05 and the far cascade reaching the land drawn. When the
    next package is pinned, the game weighs those two against its frame at each quality, the screen
    probes' quality or their reach, the far cascade's reach, read by the same measure at the spawn
    and by the 139 view, and records what it keeps; nothing is owed by the engine here. After the
    next package. The package arrived at 15:29 on 2026-11-04 as 3DEngine 6.0.91 from the engine's
    fourteenth pack run, the first since 6.0.14, carrying the engine's items 139 to 145 and 166 to
    201 of the week, among them the far cascade's cost (140, 141), the glow lights past the four
    brightest (192), the thin-wall hold at a wall's foot (193), the lamp march's clearance (195),
    the sections' two-byte indices and packed positions (144), a relight no longer uploading the
    section whole (145), every linear texture sampler at 16 (201) and Vulkan 1.4 asked outright
    (182). The game takes it, runs its suite, and reads this item, then 155. Done at `6eb6540`,
    pushed at 17:07 on 2026-11-04, the game pinned to 6.0.91. At seed 1's cherry grove at 1920 by
    1080 in Debug, GI at the game's settings reads 3.48 to 3.83 ms at Low, 5.67 to 5.78 at Medium
    and 6.56 to 6.83 at High with the distance to a 256-path reference 0.173, 0.166 and 0.166,
    against 3.96 to 4.12, 6.49 and 7.32 to 7.44 on 6.0.14; the screen probes cost 2.3 to 2.7 ms of
    Medium and High for 0.006 of distance; the far cascade costs 0.4 to 0.7 ms of GI and 0.6 to 1.1
    of the frame and without it the deep cave's far walls take the sky's light, so the game keeps
    both at reach 192 with High the default, Medium reading the same distance for about a
    millisecond less, left to be weighed. The frame at item 139's view at 720p holds 60 at 8, 12 and
    16 columns on both packages, the GPU at 16 columns 12.5 to 12.7 ms where 14.0, the memory at 12
    columns 1,468 to 1,562 MB where 2,098 to 2,413. The README takes the far cascade row, the cost
    table and the corner's eighteen bytes with 144's packing. GI over the open land at 720p reads
    6.1 to 7.6 ms on 6.0.91 where 5.3 to 5.6 on 6.0.14, the engine's item 202. The note and the cave
    picture under .github/devlog; strict build clean, 364 tests, the play 277 s. The README's eight
    "colour" are a pack's proper name and its addresses, which keep their maker's spelling.

155. **The scene field's center ahead of the camera.** From the engine's item 196 on 2026-11-04. The
    game sets no center, so the engine centers its field and probes on the camera and the finest
    cascade spends half its width behind the player; in the engine's reading of a bay and an aisle,
    an aisle centered on the eye read 17 to 29% over its reference against within 5% centered ahead,
    as Sponza sets it with the camera plus half the view. Set the field's center ahead of the camera
    along its view by half the finest cascade's width through SetSceneFieldCenter, read by the
    grove's and the cave's GI against the engine's references and by the frame at the twelve-column
    settings. After the next package, with 154. From the engine's 197 on 2026-11-04: centered on the
    eye, cascade 1's layers in the engine's bay and aisle read 1.17 of their reference at one bounce
    against 1.02 centered ahead, the placement's own first-bounce excess, and the engine's 199 (the
    probes' faces at their surfaces) raises the pier's join by 4 to 10 points around the eye while
    lowering it ahead; the center ahead is the mend for both. Done at `fe3638e` (`c6ac101` amended
    before its push to carry the day's devlog), read at 17:14, to push after `6eb6540`'s run. Each
    frame before BeginDrawing the game sets the field's center to the camera plus its forward by
    half the finest cascade's width, eight blocks at the default cell, following the Field Cell
    setting. The grove's distance 0.171, 0.164 and 0.164 ahead against 0.173, 0.166 and 0.166 around
    the eye at the three qualities, GI's cost the same within 0.2 ms, the shots differing under the
    near crown; the open land over the spawn at twelve columns at High 0.776 against 0.780 with the
    frame and the GPU within their spread; the cave black both ways; a full turn over 240 frames
    24.1 and 27.9 ms ahead against 24.3 and 23.9, the field's CPU time 1.15 to 1.3 against 1.0 to
    1.1. So the gain is about 1% in the grove where the engine's aisle went from 17 to 29% over to
    within 5, a cheaper placement's benefit being the bay-and-aisle kind of space the game has few
    of. The README's eight "colour" are a pack's proper name and its addresses and stay. The note
    and the grove picture under .github/devlog; strict build clean, 364 tests, the play 247 s.

157. **The version and its tags in the documents.** Decision 60 on 2026-11-08. COMMITS.md gains a
    section on the version, `build/version.txt`'s major and minor raised by this session or the
    reviewer in a commit of its own when a step is done and the changes behind it are meaningful,
    the major at a step the size of a release, and the tags of the letter v and the version that the
    reviewer sets on that commit and on a release build's commit, no other. When the session next
    wakes for the engine's package. Done at `8d8d1b2`, pushed at 07:00 on 2026-11-09; COMMITS.md's
    section on the version, the tags on the commit that raises it and on a release build's commit,
    7e052e8 the example.

158. **The license is the MPL 2.0.** Decision 61 on 2026-11-08. `LICENSE` holds the Mozilla Public
    License Version 2.0 in its standard text, as both engines' do, in place of the MIT text whose
    copyright line named a person; the MPL carries no such line. Every mention of MIT in the tree
    follows, and nothing in the tree names a person, read with a case-insensitive search for the two
    parts of the name over every file. One commit, told to the reviewer, who pushes it; the game's
    package rests as it is. Before 157, when the session wakes. Done at `06fd267`, pushed at 15:30
    on 2026-11-08, the text mozilla.org's, nothing else in the tree naming MIT or a person.

159. **The middle dot out.** Decision 62 on 2026-11-08. The README's first line, changed in the
    tree, goes in the next commit, and STYLE.md's Punctuation gains the sentence that the middle dot
    is not used, a row of links separated by a space, a list by commas and a product said in words;
    nothing else in the tree carries one. With 157, when the session wakes. With it, by Decision 63,
    STYLE.md's Register gains the sentence that a comment stands on the code's own terms, and the
    game's comments that explain by another product, an outside asset's file name, a person, a
    reference picture or a session's doing are rewritten in the game's terms, Minecraft's names kept
    where the game takes a rule or a number from the game it follows, as STYLE.md allows. Done at
    `8d8d1b2`, pushed at 07:00 on 2026-11-09; STYLE.md's paragraph on the middle dot and its
    Register sentence in the game's words, Minecraft named where a rule or a number comes from it
    and a pack's format where the game reads it, with a check; some 45 comments in 27 files
    rewritten to what a number or a look makes the frame do, the modpack, its shaders, the galleries
    and the reference copies gone from them.

160. **The game on 6.1.12.** The engine's package of 2026-11-09 from its 8dba0d1c, tagged v6.1.12,
    carrying the mend of the exit that ended the play at the ambient occlusion toggle (engine item
    223, Verdict 8 here), the night sky from the Sky module, the flicker's mend, the lamps through
    their glass and the metals form. Done at `8d8d1b2`, pushed at 07:00 on 2026-11-09; the project
    references 6.1.12, the strict build clean, 364 tests, the scripted play through every option on
    this machine in 388 s, the grove and the snowy village within a quarter of a level of 6.0.91's
    frames in the mean, the sheet in the dump and sent; the README's readings taken on 6.0.91 stand,
    since the frame draws the same.

161. **The workflows' engine ref and the readings in comments.** From the session's report of
    2026-11-09. The build and pack workflows check the engine out at d3473746 and say it is the
    commit the package names, 5.2.215, which has been false since the pins after it; the ref follows
    the pin from here, 8dba0d1c for 6.1.12, and the comment says that the ref is the package's
    commit and moves with each pin, so the engine's tools the workflows run match the package. With
    it, by the widened N 4.11 in the game's NORM.md, the readings in LightSettings' comments, the
    grove's milliseconds and the distances to the path-traced reference, go to the review's records
    and the comments keep the reason alone. With the next batch. Done at `f43aa9e`, pushed at 08:36
    on 2026-11-09; both workflows check the engine out at 8dba0d1c with a comment naming no version
    or hash, every reading is out of the lighting settings' comments, and STYLE.md's Register cites
    N 4.11.

162. **The last readings in comments.** From the session's report of 2026-11-09, under N 4.11. Seven
    comment lines outside the lighting settings still carry readings, in ChunkRenderer.cs at 123,
    548 and 1119, Visibility.cs at 170 and VoxelGame.cs at 316, 317 and 321, the frame's
    milliseconds at 8, 12 and 16 columns among them; each keeps its reason and loses the figure, the
    readings standing in the review and RENDERING.md where they are recorded; VoxelGame.cs's 4 ms at
    line 16 is a setting and stays. One small commit when the session next wakes, before the
    engine's next pin. Done at `9a3ec77`, pushed at 08:47 on 2026-11-09 with 4e956bc; the seven
    readings are out, ChunkRenderer.cs 122's microseconds with 123's, and a search for microseconds
    and frames a second finds no other reading in a comment.

163. **The engine checkouts take the submodules.** From Verdict 9. The engine has carried the Sky
    module as the git submodule modules/Sky since its `bd081fa6`, and its project references the
    module's project, so a checkout of 8dba0d1c without the submodules has no Sky project and the
    play job's build of e3d fails on the missing namespace. Both engine checkouts, build.yml's in
    the play job and pack.yml's, take submodules on, as the engine's own workflows do, with their
    comment saying that the engine carries modules as submodules. After item 162's `9a3ec77`, on its
    own, with no other change; the commit carries the review's files, and the two are pushed
    together with one run on the last. Done at `4e956bc`, pushed at 08:47 on 2026-11-09; both
    checkouts take submodules on with the comment, checked in a fresh clone of the engine at
    8dba0d1c, where the build without the submodule gave the run's errors and with it none.

165. **The build workflow runs on branch pushes alone.** From 3DEngine's item 236 on 2026-11-09. The
    workflow runs on any push, so the push of the tag v0.2.0 started a build run of the tag's ref
    beside the run of the same commit's push to main, two runs of one commit with the play in each;
    a tag marks a commit whose push already runs, so the push trigger names the branches and leaves
    the tags out, the pack workflow unchanged. A workflow change alone, in the commit of the next
    pin of the engine's package. Done at `cfffcb3` on 2026-11-10 with the pin of 6.2.12.

166. **The illumination read in motion.** Decision 64 on 2026-11-09. Reported at 17:25 on
    2026-11-09, the game's global illumination has looked noisy the same way for weeks, and every
    reading here since 2026-10-24 was a settled still against a reference, which a fault in motion
    passes. One read on 6.1.12 at the game's settings, High, four cascades, a cell of 0.25 and the
    far cascade on, at the grove, the village at dusk with its lamps and torches and a torch-lit
    cave, each by a scripted walk and turn recorded offscreen (N 7.8), about ten seconds standing,
    walking, turning by a fraction of a probe tile a frame, and the two seconds after a stop;
    picture to picture, the share of pixels moved by 12 and by 25 levels in each of the four states
    against the same poses settled, and the frames where the share jumps with what moved in them,
    the probes' tiles, a cascade's arrival or the lamps. A sheet by N 4.9 and the frames in the dump
    by N 4.10, the sheet sent. Each fault found is placed against the engine's items, 237's cycles,
    239's arrival after a cut and 240's bands on thin shapes, or is a new ask in the engine's
    ASKS.md with its frames, and the read is taken again at the next pin, the first package that
    carries 237. The village's lamps and torches gain reach with the engine's 241, whose floor
    follows the Field Cell setting, about 8.7 blocks at 0.125 and 17 at 0.25, which the engine's
    guide says beside the setting and the game's options leave unsaid, having no help line for a
    setting. The GPU is shared with the engine's suite, so the recording runs while it is quiet.
    After 165, before the next pin. Read at 22:05 on 2026-11-09, the sheet 22-05-item-166-sheet in
    the dump and sent. Standing moves no pixel past 12 levels in the grove and the cave; a frame in
    motion against its own pose settled moves 0.55 to 1.03 percent of the grove's pixels past 12 in
    the screen probes' tiles, the kind of the engine's 240, and under 0.11 percent in the village
    and the cave; the picture holds within three frames of a stop; no cycle of 237's and no arrival
    of 239's shows. One step, the near clouds losing about 15 levels in a frame as the far cascade
    moves a cell, is the game's ask of 2026-11-09 in the engine's ASKS.md and the engine's item 245.
    The script walked 0.07 blocks and turned 0.25 degrees a frame, slower than a hand, which was the
    review's wording of the turn. A second and last read by Decision 64 takes what play has and the
    script left out, under the lock, the grove with a turn of 90 degrees in two seconds and in half
    a second and a sprint, the day's clock running where the first runs held it, and the cave walked
    at night with a torch in hand; its shares by speed stand beside the first read's, and after it a
    recording of real play is the next step. The second read at 22:37 on 2026-11-09, the sheet
    22-37-item-166-sheet-second sent. Turns from a settled place match their settled poses within
    0.06 percent past 12 at 0.75 and 3 degrees a frame, and the cave walked at night with a torch in
    hand within 0.12. A walk's and a sprint's 0.55 to 1.0 percent is the size of the difference
    between two settlings of one place, 0.35 walked to against teleported to and 0.69 between two
    runs, in the screen probes' tiles, the game's second ask in the engine's ASKS.md and the
    engine's item 246. With the clock running the sun's light steps every 83 frames as Sky.cs sets
    it, 36 steps an hour so that a shadow's edge stands still between steps, each step moving 0.49
    to 0.60 percent of the window in its frame with a tail under 0.03 for up to 40 frames, the
    game's own choice and left. Item 166 done as read; the engine's 245 and 246 carry what it found,
    the script run again at the pin that brings them, and a recording of real play stays asked for.
    The engine's read of its 246 on its head at 23:11 found no such difference in Sponza or in its
    Ore example, a settled probe's light being the mean of its sixteen places since its 237, so the
    pin of the package from the engine's `94097b62` repeats the walked-to against teleported-to read
    in the grove first, and where it still shows the engine takes a grove-like scene with leaves
    that let light through into its tree. Corrected at 03:11 on 2026-11-10 by the read on the pin.
    Neither package shows a path difference, one place teleported to twice and in two fresh runs
    within 0.006 percent on 6.1.12 and 6.2.12 alike and the two packages within 0.005 of each other;
    the 0.35 and 0.69 percent were the script's poses, the walk's end summed in floats against a
    teleport rounded to a hundredth, half a millimeter and 3.6 mm apart, and walked to the exact end
    the place reads 0.000. Half a millimeter of the eye moves 0.34 percent of the window past 12,
    about half of it the sun's shadow edges a texel on as a cascade's middle is floored to whole
    texels and the rest the screen probes' light. So the engine's 246 is withdrawn, the second
    read's walk and sprint figures carry the same pose error, and every held pose of the rest is
    placed by the game's own float operations. The read that remains for the report is the engine's
    measure of motion, a turn in place with each picture carried into the next by build/flicker.py
    of the engine's tree, in the grove with the screen probes on and off, the share past 4 and past
    12 and its map, beside the walk, the sprint and the turns against their poses held, the clouds
    and the cave. The carried turn read at 03:29 on 2026-11-10 on 6.2.12, the grove turned 0.75
    degrees a frame over 120 frames. The carry leaves a floor of 9.8 percent past 4 in the game, the
    pixel textures' hard texel edges smeared by the resampling, where Sponza's is 0.9; with both
    pictures of a pair blurred over a tile the floor is 0.95 and the screen probes read 1.96 percent
    past 4 and 0.089 past 12 where 0.954 and 0.011 with them off and 0.979 and 0.010 with the
    illumination off, so the probes double the turn's flicker past 4 and multiply it by eight past
    12, small over the whole window. It lies in blocks of one or two tiles on the shaded ground
    under the canopies, among the grass tufts and the petals, and along the canopy's shaded edges,
    the kind of the engine's 240. The share on a mask of the shaded ground under the trees says how
    much it is where it lives, on both packages, and the read is taken again on the package that
    carries 240's ramp. On the mask at 03:43 on 2026-11-10, the grass ground with petals under the
    trees where its blurred luma lies under 80 of 255, some 74,000 pixels and 8 percent of the
    window, carried with the turn. With both pictures blurred over a tile the ground reads 20.2
    percent past 4 and 1.44 past 12 with the screen probes on, 6.0 and 0.012 with them off and 8.4
    and 0.059 with the illumination off, and 15.1 percent of the ground past 4 and 1.43 past 12 move
    with the probes and not without them, where 0.65 and 0.001 between the two runs without them;
    6.1.12 reads the same to a tenth, so 6.2.12 does not touch it. A seventh of the shaded ground
    flickering in a turn by the probes alone is the likeliest reading yet of the report. The same
    mask and turn are read again on the package that carries the engine's ramp of its 240, and its
    figure there against 6.0 and 0.012 says whether the engine's held form is taken up. The ask for
    build/flicker.py's three options is in the engine's ASKS.md. The poses held read again at 05:15
    on 2026-11-10 on both packages, each pose placed by the game's float operations. Every frame in
    motion matches its pose held within 0.08 percent past 12 and 0.061 past 25, the walk, the
    sprint, the turns at 0.25, 0.75 and 3 degrees a frame and the cave with the torch, where the
    floor of a still view with the clock running is 0.04 to 0.05, so no light lags at any speed and
    the first reads' walk and sprint figures were the pose error. The cave at night is brighter on
    6.2.12 at every pose, its mean luma 3.26 to 3.76 and 8.51 to 10.00 at the ends, the engine's 241
    reaching further. The clouds step as before on both, the engine's 245. What remains of the item
    is the carried turn on the mask, read again on the package that carries the engine's ramp. On
    6.2.14 at 09:43 on 2026-11-10, the same mask and turn. With the screen probes on the shaded
    ground reads 12.2 percent past 4 and 0.24 past 12 where 20.2 and 1.44 on 6.2.12, the ground
    without them 5.97 and 0.012 on both; 7.1 percent past 4 and 0.23 past 12 move with the probes
    and not without, where 15.1 and 1.43. So the engine's ramp halves the probes' share past 4 and
    takes five sixths of it past 12, and what remains stays in tiles on the grass under the trees,
    twice the ground's floor past 4. That is over the floor by enough for the engine's held form,
    its item 248, read again here on the package that carries it. Its cost read at 10:02, the two
    packages in turn at the game's defaults at 1280 by 720. At the spawn on the ground the frame
    takes 13.49 ms where 13.29, the illumination's pass 8.51 where 8.42 and the lit scene's 3.61
    where 3.51; in the README's view the frame 11.92 where 12.04, the illumination's 6.83 where 7.09
    and the lit scene's 3.80 where 3.63, two runs of one package differing by up to 0.13 in the
    illumination's pass. So the ramp costs the game 0.10 to 0.17 ms in the lit scene, a tenth of
    what it costs Sponza's main pass, and the cheaper form the engine kept as a patch is not needed
    here. On 6.2.17 at 13:27 on 2026-11-10 the clouds' step is gone. The walk's 123rd frame moves
    9.32 percent past 12 as the frames beside it do, where 11.63 on 6.2.14, and the near cloud
    settled at the two poses reads 159, 179, 205 then 163, 179, 203, brightening a little as it does
    with the far cascade off, where it dropped 15 levels; the engine's 245 is confirmed here. A cut
    into the torch-lit cave settles alike on both packages, the mean within 2 levels of the settled
    picture from the cut's first frame, with 2.3 to 2.9 percent of the pixels past 12 for seven
    frames on the near wall and its ledge by a shadow's shape and then none, so it is not the
    coarser cascades' light and does not test the engine's 239, the cave's light standing inside the
    finest cascade. Two reads remain, a cut in the grove by day from 150 blocks off, where the sky's
    light through the coarser cascades lights the shaded ground, on both packages, and the cave's
    cut with the illumination off and without the torch in hand, to place the seven frames. The
    runner's play draws every step 1.31 times as long on 6.2.14 while its open on the CPU takes the
    same, so the frame costs lavapipe more, to be measured at the spawn under lavapipe on 6.2.12 and
    6.2.14. The cuts read at 14:00. In the grove by day, cut from 150 blocks off to the walk's
    start, the mean lies within 2 levels from the 31st to the 61st frame on 6.2.14 and from the 34th
    to the 58th on 6.2.17, three cuts each, about 22 percent of the window past 12 until then, the
    shaded ground and the trees' undersides lit as if open, dropping in one frame; with the
    illumination off at most 2.5 percent, gone by the 28th frame as the shadows and the meshes come
    in, and with the game's culling off the 105th. So the lag is the illumination's, grows with what
    is drawn, and the engine's 239 does not shorten it here, the field holding no trees until its
    build of the new place lands, which comes after the meshes do. The cave's seven late frames stay
    with stone in the hand and with the history, the screen probes or the filter off and go with the
    illumination off, so they are the same build on a smaller place, a level on one wall for an
    eighth of a second, and no ask is written for them. What remains is whether it shows in play, a
    death and a respawn at a wooded spawn read once, with the field's budget raised by the game for
    the second after the respawn as the lever of the game's own, and the ask to the engine written
    with both figures, the raised budget held while meshes still arrive after a cut. The respawn and
    the lever read at 16:21. A death and a respawn at the wooded spawn lies under 0.1 percent past
    12 three frames after the view comes in, the respawn meshing its sections before it moves, so
    the case in play is sound and only a teleport by command shows the lag. The lever of the game's
    own cannot be brief, a changed budget making the field again, the grove relit twice and settled
    at the 132nd frame where the 77th plain, so no game commit; held at four times the base the mean
    lies within 2 levels from the seventh frame at 0.4 ms more of the field's pass in the frames
    after a cut. The ask is in the engine's ASKS.md and is its item 251. Whether the game holds four
    times the base for the whole of play is item 168, after a walk's timing with it.

167. **The engine's color functions and the field's center ahead, at the pin that carries them.**
    From the engine's item 244 on 2026-11-09 (its Decision 108). The game writes the sRGB curve out
    in SkyColors.cs and TerrainAtlas.cs and a gamma of 2.2 in SectionMesher.cs and Sky.cs, and
    places the scene field half the finest cascade ahead of the eye by the engine's 64 cells times
    its own cell size in LightSettings.cs. With the package that carries the engine's 244 the game
    calls the engine's functions where it means the sRGB curve, a 2.2 that is the look's own staying
    with its reason in a comment, and takes the engine's option for the field's center, the play's
    captures unchanged. A small commit at that pin or the one after. Done at `e3dea22` on 2026-11-11
    with the pin of 6.2.32. SkyColors and TerrainAtlas call SrgbToLinear, LinearToSrgb and
    ColorFromLinear, which give the same bytes, the field lies ahead by SetSceneFieldAhead(0.5f),
    and the 1/2.2 in SectionMesher twice and in Sky's stars stay, the look's own, level 0 a fifth
    brighter and the faintest stars about three times brighter by them than by the curve. Two runs
    with it differ from each other as from 6.2.17, dusk 2.5 percent past 24 levels in both and every
    other picture under 0.2, so the play's pictures stand as they were.

168. **The scene field's triangle budget at four times the default, where a walk's cost allows.**
    From item 166's lever read on 2026-11-10. Held at 131,072 triangles a frame the field takes a
    teleport's new place within seven frames where 35 to 77, at 1.41 ms of its pass against 1.01 in
    the frames after the cut, and where nothing changes a larger budget costs nothing. One timing of
    a walk and of a sprinted flight over unloaded land with it and without, the frame and the
    field's pass under flock -x, and the frames a section's light lags its mesh at the streaming
    edge; where the walk's frame holds within the runs' spread, the game sets it in LightSettings,
    one line with its reading in the commit's description, and where it costs the flight, it stays
    at the default and the engine's 251 carries the cut alone. Read on 6.2.17 at 19:42 on 2026-11-10
    and held for the next package. At the streaming edge after a teleport over the river a section's
    light lags its mesh by 21 frames at the 75th part and 50 at the 90th at the default, 12 and 21
    at 131,072, 154 and 135 at most. A sprinted flight costs on the median with it, 14.10 and 13.48
    ms a frame against 12.42 and 12.41, the field's pass 1.73 and 1.38 against 1.24 and 0.58, so the
    larger budget is not free in a flight. The engine's 251 changes what a held budget does, the
    lift after a cut holding while meshes arrive and a changed budget no longer making the field
    again, so the budget is decided on the package that carries it, with the grove's teleport, the
    edge's lag and the walk and the flight read again. None of the timings here or in 166 took
    field.wait as the field's build time, an engine fault having let that wait run out with nothing
    left to build, mended in the same package. Decided at 04:00 on 2026-11-11 on 6.2.32, the default
    stays and the game changes nothing. With the engine's 251 the larger budget gains nothing, the
    grove's mean within 2 levels from the frames 7, 6 and 9 at the default against 8 and 10 at four
    times it, past 12 under 0.1 percent from the frames 21, 36 and 42 against 52 and 66, and the
    edge's lag 10 and 16 frames at both. The walk's and the flight's figures stand in the Reply with
    the load beside them. Item 168 done as read.

169. **A build keeps the resources' folders whatever path reaches it, and the game stops at its open
    where a file of its own is missing.** From the pin of 6.2.32 on 2026-11-11. A build made in a
    worktree reached through /home, a link to /var/home on this machine, copied the game's resources
    flat beside the program, terrain.slang, bits.slang, mob.slang and packs.json at the top and no
    resources/shaders, since the project's Content glob leaves the place to MSBuild, which takes a
    file reached through a link as lying outside the project; and the game then ran without its own
    terrain shader and said nothing, LoadShader giving the default as the followed engine's does, so
    the only sign was a lantern's pool at half its light, read for an hour as a fault of the
    engine's and written as its item 252, withdrawn. Two mends in one commit. The Content items name
    their place with their folders, so the layout is the same from every path, with a test or a step
    of the scripted run that the built folder holds resources/shaders/terrain.slang. And the game
    checks each shader of its own and packs.json as it opens and stops with the file's path where
    one is missing or did not compile, the play failing at its first step and not at a picture.
    After the pin's commit. Done at `08bf109` on 2026-11-11, pushed at 03:33. The Content items
    carry their folders by Link, so a build through /home holds resources/shaders and
    resources/packs.json; OwnFiles lists the game's four shaders and packs.json, and the game checks
    them after the window opens and stops with 1 and the path where one is missing or a shader does
    not compile; the play quotes the game's first error line where the open fails. Read with
    bits.slang removed, the game stopping in 1.3 s with the path, and with mob.slang broken,
    slangc's reason with it; 366 tests pass and the play passes from an empty cache in 463 seconds.

170. **The game's local checks run through a client that no build of the engine's tree reaches, and
    stop what they started by its pid.** From 2026-11-11. The checks drive the game through ./e3d of
    the engine's live tree, whose client builds again whenever a source of the engine's changed, so
    the engine's session building its tree failed a play at 02:03 with CS0006, lost a SetTargetFPS
    in item 168's first pass, and took the stop from the play's exit, which goes through the same
    client; the game it left ran offscreen on the GPU outside the lock from 02:10 to 03:41, beside
    another session's exclusive timing and beside 168's own walk and flight, which read 25 to 43 ms
    a frame for it and are taken again. Two mends in one commit. The checks drive through an e3d
    client of the pinned engine's commit, built once into a folder of the game's own outside the
    engine's live tree, as the runner builds it from its checkout; where e3d has no way to run
    without building, the ask goes into the engine's ASKS.md. And each check script stops by pid any
    game left from its own build folder as it ends, whatever the client answers. After 168's reads.
    Written by 04:00 on 2026-11-11 and held uncommitted. build/engine.sh keeps a checkout of the
    engine at the workflows' commit in .engine with its e3d client, built only when the checkout
    moves, 66 s and 264 MB the first time, and play.sh and pictures.sh stop their games by process
    where the client's stop leaves them. Through the pinned client two cold plays in a row fail the
    water's mirror, terrain.slang's pipeline made at the pool in 1.57 and 1.66 s, where the three
    cold passes before went through the live tree's client, so two cold plays through the live
    client are taken first; where they fail too, the engine's 253 is flaky from a cold cache
    whatever the client and 170 is committed with that in its Reply, and where they pass, the
    clients' driving at the pool's steps is compared before the commit. The two cold plays through
    the live client, reported by 04:06, do not settle it, one failing at the lantern's pool after
    the fifth frame made 22 pipelines in 47 s and one passing whole in 391 s, so among cold plays
    that reached the mirror it passed 4 of 4 through the live client and 0 of 2 through the pinned,
    the clients' source the same. No more plays are taken. Whatever leans it is timing against a
    race on the engine's side, its item 253 either way, the 47 s its one lock running the compiles
    one after another, mended at its `21122dcd`; a play here is reliable on 6.2.32 from neither a
    cold nor a warm cache, and the runner's play is the gate. 170 is committed as it stands with
    these figures in its Reply, and the mirror is read on the next package, cold and warm, three
    plays each with the session's log copied at the pool. Done at `8ded865` on 2026-11-11, pushed at
    05:11.

172. **A shader of the game's calls the engine's lighting once.** From the engine's item 255 on
    2026-11-11. The driver's own first compile of the terrain pipeline takes 12 s from an empty
    cache folder with no layer, a player's first run, and half of terrain.slang's 1.3 MB fragment is
    a second copy of the engine's lighting that can never run, from its first line calling lit on
    the branch drawnUnlit takes, where lit itself returns unlit and its litLight is dead to all but
    the compiler. With that line returning unlit(albedo + emitted(input)) the fragment is 702,448
    bytes where 1,303,784 and the cold compile 5.9 and 5.2 s where 12.4 and 13.6, by the engine's
    trial on a copy. The line is changed in terrain.slang, and mob.slang, whose pipeline took 19.1
    s, bits.slang and cavefields.slang are read for the same call of lit or litLight on a branch
    that cannot be lit, or twice in one shader. One commit, the play's pictures unchanged, with one
    cold play with the layer off before and after, the fifth frame's seconds and the pool's, in its
    Reply. Done at `1f1e955` on 2026-11-11, held at 18:53 until the pin's run ends. terrain.slang,
    mob.slang and bits.slang return unlit on the branch drawn unlit, each fragment about 590 KB
    smaller, terrain's 690,816 bytes where 1,280,560; cavefields.slang is a compute shader with no
    lighting; mob's second litLight is live, the light of a face turned up, and stays. From an empty
    cache folder with the layer off the fifth frame's 22 pipelines take 20.3 s where 43.9, terrain's
    depth pipelines 4.9 s where 12.0 and 12.2, mob's 8.0 where 17.1 and the pool's four 5.4 to 5.7 s
    each where 13.1 to 13.2. Three plays pass whole and 16 of 17 pictures differ in at most 0.16
    percent, dusk in 2.5 against 2.3 between two plays of one build.

175. **The game far from the origin.** After 174, a read and no mend. The game's worlds have no
    edge, and its own shaders and its own arithmetic work in world numbers too. The same built scene
    at 16 and at 256 times the saved world's distance, 71,000 and 1,140,000 blocks out, and a walk
    there, to say what goes wrong first and where, the terrain's seams, the stepped light's squares,
    the water, the mobs, the shadows, picking a block, the physics, each with a picture and the
    distance at which it is first seen. What is the engine's goes to it as asks, and what is the
    game's becomes items. The read is done at 23:46, on 6.2.41 with the game of `6b0043b`, the scene
    built block for block at 16, 4,432, 17,808, 70,992, 284,688 and 1,139,984 blocks out, the sheet
    23-45-item-175-far.png in the dump. From 4,432 the engine's light on a wall a glow light lights
    is mottled, the west wall's mean step at Low 0.34 at the origin, then 0.48, 0.89, 0.75 and 1.11
    out to 284,688, speckles in the frame from 17,808, the engine's 257 taking the 4,432 case back
    to the origin's. From 284,688 the land's faces part in a lattice of cracks with the sky through
    them, the game handing each section a float matrix and the engine placing every vertex in world
    floats, 0.03 a step there; and the walk along the far axis is 13 percent slower, the body's
    position a float Vector3 and a frame's move of 0.07 rounding to 0.0625. At 1,139,984, where a
    float steps by 0.125, picking gets 5 of 14 right, most misses one block east, the ray cast from
    the float eye, the walk along x jumps 2 to 4 blocks a read and stops with the body 0.3 inside
    the wall, a teleport to a half lands on a whole, a pig walks along z alone, and the scene's
    faces tear into strips with the shadows' edges ragged. The fall, standing and the walk along the
    near axis hold at every distance. A teleport of over 741,000 blocks left the old columns loaded,
    an int squared, mended at `88d6303`. What is the engine's is an ask under its item 258, the
    vertices placed in world floats; what is the game's is item 177.

173. **A built room's striped wall, read for the engine.** From the report of 2026-11-11 at about
    18:35 and the engine's item 257. A player's saved world, overworld of seed 1, opens at 4448.4,
    24, 599.8 facing 48 degrees in a dark closed room built there, the illumination at Low, and the
    first view shows the noise reported for weeks, stripes of small squares in a glow light's light
    on the back wall; the review saw the same from the game's built program on a copy of the
    player's data folder, ~/.cache/claude-reviewer/owner-view/data, with its captures beside it
    under shots. The game's coder reads the room from that copy and never from the player's own
    folder, by XDG_DATA_HOME naming the copy. The blocks that glow within reach of the wall, their
    kinds and places, the wall's and the ceiling's planes, the eye's place; the view captured as it
    opens and with each glowing block taken away in turn in a second copy, to say which light draws
    the stripes; and the engine's gi.show and field.march at a striped place and a smooth one, as
    its coder asks. Written as an ask in the engine's ASKS.md with a sheet by N 4.9. A check for the
    play after the mend, the room's wall read for its squares at each quality. After 172's commit,
    and ahead of all else. The engine's coder reads the stripes at a glance as the glow light's
    march calling the wall itself an occluder and asks three things first, the glowing blocks'
    places and sizes against the wall's plane, the wall's plane with the field's cell, and
    field.march from a striped texel and from a smooth one beside it toward the nearest glowing
    block; they go to it through the review as they come. The read is done at 19:06, nothing
    committed. The removals on a second copy say the lantern alone draws the stripes, the wall
    reading 0 with it made white concrete and as before with the 14 magma blocks made stone. The
    sheet is 19-00-item-173-room.png in the dump, and the ask is the last entry of the engine's
    ASKS.md, which the engine's next commit carries. What remains is the check after the mend. When
    the engine's coder has the mend in a package of its own feed, unpublished, the game's coder
    captures the first view reported from its copy on 6.2.41 and on that package, built in its work
    folder, the wall's rows read at Low, Medium and High on both, before and after on one sheet. The
    work folder i173 is kept until then. The check's first step is done at 23:32 on the engine's
    package 6.2.41-local.257. The stripes are gone, no row of the east wall with a step over 3
    levels at any quality at 1280 by 720 or at 2560 by 1440 where the published package has up to 29
    and up to 16, the figures under the engine's item 257. One thing reads worse, a line across the
    ceiling at rows 173 to 175 at 2560 by 1440 alone, which went to the engine's coder. The sheet
    23-50-item-173-mend.png went out. The second step, 174's scene on the package at both places,
    follows.

174. **The same built scene near the origin and 4,432 blocks out.** From the engine's item 257 on
    2026-11-11. The reported noise was float precision 4,450 units from the origin in points and
    normals the engine takes from the depth, which no scene at the origin shows, and the engine's
    item 258 reads its other passes for the same. The game gives it the controlled case. In a copy
    of the data, one small scene built of blocks twice, block for block, once within 64 blocks of
    the origin and once beside the built room near 4448, 24, 600, each on a platform in the air
    clear of the terrain, a floor, two walls, a roof slab that shades half the floor, a pillar, a
    sea lantern set in a wall and one standing free, at the same hour. Each is read from the same
    eye against its scene at Low, Medium and High with each of the engine's stages on and off in
    turn, ambient occlusion, the screen's probes, the glow lights, the filter, the frame before's
    light and the shadows, by rows of voxel.line on the shaded floor, the lit floor, a wall and the
    shadow's edge, the steps over 3 levels and the mean step, on 6.2.41. A sheet by N 4.9, the two
    places side by side with their difference, and a table of what differs between the places by
    more than one place differs between two runs. Each stage that differs goes to the engine as an
    ask under its item 258, and the scene stays as the check of each mend. Under the lock, shared.
    The read is done at 22:59, four sessions, two at each place. The screen's probes, ambient
    occlusion at 1 and the sun's shadow differ between the places by more than a place differs
    between two sessions, the figures under the engine's item 258 and in its three asks, the sheet
    23-30-item-174-places.png in the dump. The scene in i174 stays as the check of each mend, read
    again on the engine's package of its 257 first. Read again at 23:46 on the engine's package
    6.2.41-local.257, four sessions. The screen's probes read alike at the two places, the west
    wall's mean step 0.34, 0.35 and 0.37 near against 0.36, 0.37 and 0.39 to 0.40 beside the room
    where the gap was 0.12 to 0.17; ambient occlusion and the sun's shadow read as before, not being
    in that mend; nothing reads worse past the noise. The sheet is 23-55-item-174-mend.png.

178. **Mobs saved with a place that is no number.** From the read of a copy of the review's backup
    of a player's saves on 2026-11-12. In one world 166 of 1,992 saved mobs hold a place whose x is
    no number, one of them its z too, all in columns whose x is 0, from 0, -43 to 0, -18; in another
    113 more in columns 0, -18 to 0, 0, 54 of them villagers; two other worlds hold none. Both
    versions of mobs.bin carry them as they are. None loads until a player comes near the spawn.
    First the read, on a copy at the spawn. What such a mob does when its column loads, its column
    reckoned as the least int so Leaving never takes it, its matrix reaching the renderer and the
    engine's field; whether the game of the tree still makes one, and where, a place becoming no
    number in the mobs' moving, their paths or their homes about the column x 0; and what a frame
    near the spawn reads with them loaded against the same frame with them taken out. Then the mend
    on both sides, the cause, and the reader, which sets a record whose place is no number down
    again where its column's generation set it or leaves it out and says how many, with a test of
    each. Ahead of the reach, which waits for the engine's package. Done at `35a0829`, green at
    05:11 on 2026-11-12 with the play in 66 minutes. The first such mobs came of a fall mended at
    `e433e5c` and the rest of the crowding's push, a box with no place overlapping every box of its
    height; a move that is no number is not taken, such a box overlaps none, and the save's reader
    sets a mob with no place down at its home where it has one and leaves it out otherwise.

179. **Comments that say what the code is as it stands.** N 4.11 as widened on 2026-11-12. A comment
    says what the code is and why in its own terms. It does not liken one thing to another's state,
    tell what the code was before, what a mend changed or which fault led to it, nor name a program
    built on this one as its example. A sweep of every comment in the sources, the shaders, the
    tests and the scripts for those three things, the words that give them away among them, as it
    was, as before, no longer, still, used to, once, already. Each rewritten to say what stands, the
    story going to nothing or to the Reply. A commit of comments alone, with the norm's search added
    to STYLE.md's so it is found again.

177. **The body's and the ray's places far from the origin.** From 175's read on 2026-11-11. The
    player's body, the mobs' and the picking ray keep their places in a float Vector3, so from
    284,688 blocks a walk along the far axis is 13 percent slower and at 1,139,984 picking misses by
    a block, the walk jumps and stops inside a wall and a teleport lands half a block off. No player
    walks there in a session, a teleport does. The places kept in doubles, or as a column and an
    offset within it, for the body, the mobs and the ray, the float made only where the engine takes
    one, with tests at 284,688 and 1,139,984 of the walk's speed along each axis, the stop before a
    wall, picking's 14 and a teleport's landing. After the checks of the engine's mends, which go
    ahead of it whenever a package comes; a read of what it touches first, as a Reply, since the
    body's place runs through the physics, the saves and the network of calls about them. Decided at
    23:50 on the coder's read. Of the three ways, doubles, a column and an offset, or one origin for
    the whole game, the origin is taken. A whole column the floats are kept against, set at load
    near the player and moved when the player has gone far from it, every place and the camera made
    again against it, the conversion standing at the floors that turn a place into a block and at
    the saves. It keeps the floats and their arithmetic where they are, and it hands the engine
    every place near zero, so what the engine loses far from the origin, its item 258's whole list,
    the vertices, the shadows and the frame before's lookups, no longer reaches the game at any
    distance, the saved world's 4,450 among them. So it is no filler and goes next after each check.
    The origin a multiple of 1,024 blocks, so every grid of the engine's falls against the land as
    before; moved only past 2,048 from it, so a walk does not move it back and forth. Saves and
    commands keep and print the land's own places, mobs.bin in a new version with the old one read.
    The shaders' world places that are fractions of a block stand, and one taken whole, as the
    clouds' drift, has the origin added. In steps, each a commit with the suite and a play. First
    the origin with the floors, the saves and the commands, fixed at load. Then the move, with a
    test that a walk across it keeps the picture once the field has settled and nothing jumps, and
    its cost read, the field and the probes made again, an ask to the engine for a way to be told
    the world moved by a whole offset where that cost shows. Then the tests at 284,688 and
    1,139,984, the walk's speed along each axis, the stop before a wall, picking's 14 and a
    teleport's landing, and 174's scene read at 4,432 on the published package with the origin,
    which should read as near the origin does. Step 2's read, recorded at 00:49 on 2026-11-12, the
    reach 1,024 by the coder's ladder, the same scene at 16, 272, 528, 1,040 and 2,000 blocks from a
    zero origin reading the west wall's mean step at High 0.37, 0.38, 0.41, 0.40 and 0.51, within
    0.04 of the origin's through 1,040 and the mottle of 4,432 at 2,000, so the review's 2,048 was
    too wide, set without a figure. A move shifts every float place by the whole step, at a frame's
    start and first in a teleport and a respawn. Moving the sections' transforms made the engine's
    field take every section for a mover, stamped as a box for 1,024 frames, the land 8 levels
    darker for ten seconds and more; every mesh uploaded again at the move, the field takes them as
    meshes that came and builds them at once. The cost is the frame of the move and the next, 342
    and 579 ms at Low and about 600 each at High where a frame reads 163 to 183 through e3d, a hitch
    of 0.2 to 0.4 s once in 1,024 blocks. A player standing still sees one frame 3 to 5 percent
    brighter, High settling over 16 frames; in a flight the crossing frame changes 1.58 levels at
    Low against 0.82 and 1.77 at High against 0.30, the 50 frames after 0.93 and 1.22. The same view
    after a trip to 500 and back reads 0.00 percent of pixels over 2 levels, and after a trip to
    1,100 and back, the origin then at 1,024 and the view 990 from the engine's zero, 2.21 percent,
    the wall 0.41 against 0.37. Accepted as it stands, with the ask to the engine for a way to be
    told the world moved by a whole offset, which takes the hitch and the bright frame away. The
    reach is read again on the engine's next published package, whose screen passes no longer lose
    what the ladder measured, and widens if 4,096 then reads as the origin does. The ladder again on
    the engine's package 6.2.41-local.257.1, recorded at 01:37, the scene at 16, 1,040, 2,000 and
    4,112 from a zero origin through games without the origin. On the package the west wall's mean
    step reads 0.34, 0.35 and 0.37 at 16 and 0.36, 0.36 and 0.37 at 4,112 at Low, Medium and High,
    alike at every distance, where the published 6.2.41 reads 0.62, 0.64 and 0.73 at 4,112. What
    stays is the sun's shadow, 25, 24 and 24 steps over 3 on the shaded floor at 4,112 against 26,
    25 and 26 at 16, and ambient occlusion unread, at 0 in the saved options. So on the published
    package of that mend a reach of 4,096 with the step at 1,024 reads as the origin does, a move a
    quarter as often; the reach widens then, with ambient occlusion at 1 read at the reach's edge
    first. Done with the pin at `4d96da7`, green at 14:57 on 2026-11-12. The origin follows the
    player, its reach 4,096 blocks on the engine's 6.2.56, every row of the built scene reading
    alike to 4,112 blocks from the engine's zero with ambient occlusion at 0; the hitch of a move
    comes once in at least 3,584 blocks walked, and the engine is asked for a way to be told the
    world moved.

171. **A reading of time is taken with the validation layer off, and a pixel read after a first
    sight waits for its pipelines.** From the pin of 6.2.41 on 2026-11-11. The play runs with the
    validation layer on, by ENGINE_VULKAN_VALIDATION, the engine's package being a Release build
    that leaves it off otherwise, and the engine's trial on terrain.slang puts about 1.2 s of every
    such pipeline on the layer's own work, 1,274 ms compiled and 1,229 ms from the cache with it
    against 94 ms and under 100 ms without, so the seconds the game read at its fifth frame and at
    the pool, and the mirror's failures on 6.2.32, were the layer's and none a player has. The play
    stays under the layer, which is its check of correctness. A reading of time or of a stall is
    taken with the layer off, by ENGINE_VULKAN_VALIDATION=0, and says so; one cold play and one warm
    with it off give the fifth frame's pipelines and seconds and the pool's three beside
    2026-11-11's with it on. The mirror's check, a pixel read 32 frames after a first sight, waits
    for the pipelines once the engine has a command that returns when none is being made. After the
    pin's run. Read at 15:48 on 2026-11-11 with the layer off, both plays passing the mirror. Warm,
    no frame after the first makes a pipeline over 100 ms and the first frame takes 314 ms, where
    4.4 to 4.6 s at the fifth frame and 1.17 to 1.37 s at the pool with it on, so the layer is the
    warm seconds. Cold, the fifth frame makes its 22 pipelines in 49.0 s, one after another, where
    49.7 to 62.3 s with it on, and the pool's four terrain.slang pipelines compile in 15.5 to 15.7 s
    each, so the layer is not the cold ones; an empty cache folder empties the driver's own cache
    under it, and these read as the driver's first compiles, a player's first run. That is the
    engine's item 255. What remains here is the mirror's check waiting for its pipelines at the pin
    that carries the engine's 254. Done at `4d96da7`, the pin of the engine's 6.2.56 that carries
    its 254, pushed at 13:48 on 2026-11-12 and green at 14:57, the play asking pipelines.wait before
    the mirror's read and before the plain water's, six plays passing whole.

176. **Bright colored blobs in the distance under bloom.** Reported to the game's coder on
    2026-11-11, colored blobs of bloom in the background at a distance. Read on 6.2.41 at `1f1e955`,
    the sheet 21-30-red-blob.png in the dump. One stands in the origin scene's view, a spruce
    trunk's foot on snow at -98, 105, 39, 134 blocks off. With bloom at 0 it is one pixel reading
    255, 209, 199 beside a trunk of 54, 46, 42, which bloom spreads into a red blob over the tree.
    The pixel is the same through every part gi.toggle leaves out, with each cascade alone and at
    Low, Medium and High, and only the illumination off clears it, the snow then 221, 215, 205; no
    block light, fire or mob is near. From 15 blocks a trunk's lower half above the snow reads 172,
    90, 0, blue at exactly 0, where its upper half reads 82, 50, 23. So the illumination's light at
    a trunk's foot beside sunlit snow is far too bright, whether in the engine's light or in
    terrain.slang's taking of it not yet said. The read goes ahead of 174 since it was asked for.
    Which term carries it at that pixel and from 15 blocks, the light's parts as voxel.pixel and the
    engine's views give them; what clears it, the snow made grass or stone, the trunk made stone,
    the sun's shadows off, the hour at noon and at dusk, the far cascade off; and the smallest built
    scene that shows it, a log standing on snow in the sun, added to 174's scene at both places so
    it is read near the origin and beside the built room alike. Then an ask to the engine with the
    numbers and a sheet, or an item of the game's where terrain.slang takes the light wrongly.
    Recorded at 21:53, the coder's read so far, on 6.2.41 from a copy of 174's data, bloom at 0. The
    sheet's fourth panel is not the fault. The orange block is the dirt under the trunk, its sides
    showing where the snowy ground steps down, taking the same light as the log over it, both 229,
    209, 183 with every surface drawn white, and its orange with blue at 0 is dirt's own color under
    the frame's grading at a saturation of 1.4, 138, 60, 0 with the illumination off; so nothing is
    kept by the block. The blob is one pixel on a silhouette's edge, 272, 352 from 174's origin eye,
    the trunk in the pixel to its left at 55, 46, 42 and snow two blocks behind it, 138 blocks off.
    It reads 255, 209, 199 in five reads 7 frames apart and clears with the eye moved a quarter
    block or turned 0.05 degrees. Every part gi.toggle leaves out, the specular too, each cascade
    alone, the three qualities, the sun's shadows off, the sky's floor at 0, the light levels off,
    noon, and the block under the pixel made stone, snow or grass leave it as it is, and only the
    illumination off clears it, 221, 215, 205. Its color is the log's albedo under about ten times
    its light. So an edge pixel takes the trunk's color with light from a term no toggle reaches,
    and the game cannot split it further, gi.show drawing the probes' data and no pixel's resolved
    light. To come, the eye stepped sideways from 133 blocks at both built places counting edge
    pixels and blobs, then the ask. The read is done at 22:45. The blob comes of shading at 4
    samples a pixel, gone at 1 sample, dimmed with the temporal antialiasing on, one pixel still at
    twice the size; the sweep's counts and the sheet 22-40-item-176-samples.png are in the ask and
    under the engine's item 259, which mends it. What remains here is the check after that mend, the
    sweep's counts at both places read again. The engine's mend read at 10:05 on 2026-11-12 on its
    local package 6.2.41-local.259, the vertex color and uv2 read at the centroid where a pixel's
    middle outside its triangle had given a half float's most. By the origin the steps where bloom
    adds over 20 levels fall from 7 to none and bloom's most from 142 levels to 13, beside the room
    from 1 to none, nothing reading worse. The pinned check comes at the publish that carries it.
    Done with the pin of the engine's 6.2.60 at `f201940`, pushed at 19:54 on 2026-11-12. The sweep
    reads no step with a blob at either built place, bloom's most 13 levels by the origin and 7
    beside the room where 142 and 24, the lone bright pixels left ordinary ones bloom does not
    spread.

180. **The deep audit's findings.** From the review's read-only audit of 2026-11-14 at `f201940`,
    ~/.cache/claude-reviewer/audits-2026-11-14/Voxel-Sandbox-deep-audit.md, five areas each read by
    a finder and every finding put to three verifiers, one reading the code, one its reach and one
    the records. 23 held, 13 of medium severity and 10 of low, findings 3 and 4 the same fault, each
    with its file and line, its scenario and a direction for the mend. First finding 6, which the
    review checked against the code. A test game given a save folder is not transient, so it reads
    the machine's own pack list and options under the player's data folder and, where the pack list
    is missing as on a runner, fetches the default packs into that folder; three tests build one,
    and until the mend the suite runs here with XDG_DATA_HOME naming a folder of its own, since no
    session touches the player's data. Then finding 8, play.sh's trap stopping whichever game
    session serves on the machine when the play fails before naming its own. Then a Reply saying of
    each other finding whether it holds at the head, and the mends a few to a commit, the saves'
    first, 1, 2, 14 and 15, then the input's, 3 to 5 and 18, then the costs, 10 to 13, 20 and 21,
    each read before and after, then the rest, each with a test that fails first; a finding that
    does not hold says why. Ahead of the pin, which is taken when the engine's package comes.

181. **The second read's findings.** The coder's own read of 2026-11-15 of what the audit's finders
    left, the save's formats under a fuzz of every cut and altered field, the shaders, and the
    scripts with the workflow, 34 Reads in the Replies, each with its file and line and a scenario.
    The mends in this order, a few to a commit, each with a test that fails first where one can be
    written. First the worlds that cannot be opened or that hang while an altered file stands, Reads
    2 to 10, a reader refusing or bounding the value and the file moved aside as a region that
    cannot be read is, so a player keeps the rest of the save. Then Read 1, an entry's length
    bounded before it is allocated. Then the play's false passes and fails, Reads 17 to 22 and 34.
    Then the shaders' Reads 12 and 13 with a picture pair each. The low ones last, each mended or
    left with its reason in a Reply.

182. **A number typed at the console that is not a number.** Every command taking a float or a
    double read with NaN and with both infinities, the time, the field of view, the scale, the ring
    and the options' values among them, the teleport, the look and the flicker refusing them
    already. Each either refuses with a line saying so or bounds the value, one way across all of
    them, and a test gives every such command the three values and reads that the state stays
    finite.

183. **A save that cannot be written.** A save throwing an input or output error, a full disk or a
    folder without write access, ends a world switch before the next world opens and ends the quit's
    save, and the timed save spends a frame's update every 30 seconds on it. The switch and the quit
    go on, the player told once on screen that the world is not being saved and why, the timed save
    backing off while the error stands. A test with a save folder that cannot be written reads the
    switch, the quit and the message.

184. **A file error outside the save.** The coder's read of 2026-11-15 in the Replies, three kinds.
    In the drawing, where a throw between the frame's begin and its end leaves the game never
    drawing again, the options write, the worlds list, the packs screen's listing and its Done. At
    the start, where the program ends before its first frame, the options read, the pack list, the
    packs, the first world's folder and the save's readers that let an access error through. In a
    frame's update, the screenshot's folder and its claim of a file never written, the options
    written by a key or a command, the packs reopened, the defaults taken, a switch to a world whose
    folder cannot be made, and the flicker run's file. One rule across them as item 183 set for the
    save. The drawing and the update never end at a file error, the player is told once on the HUD
    with the file and the reason, a setting or a list not written is kept for the next change, a
    world that cannot be opened is left unopened with the player kept in the current one and told,
    and the start goes on with defaults where a file cannot be read, the log saying which. The
    drawing's kind first, then the start, then the update, a commit a kind, each path with a test
    that fails first where the test game can reach it.

185. **The console's commands the tests do not send.** The coder's third read of 2026-11-15 in the
    Replies, seven findings, the first, the fifth and the sixth read against the code by the review
    at 17:00 and standing. In one commit, a fill or a room whose corner lies at the end of the
    integers counting its sides in long and refusing a corner past the land's edge as the teleport
    does, with a test bounded in time, and the biome search stepping in rings 8 apart to 512 blocks
    and 32 apart beyond, its time measured again. In a second commit, a save that answers whether it
    wrote, tested on a folder that cannot be written; "unsaved" in place of "transient" for a world
    played without a folder; the quality of the illumination and the world's mode refused when they
    name no defined value; the flicker's frame count bounded at 36,000; and the button's summary put
    back over the button. Open.

186. **Comments that liken a thing to another game's.** About 76 comment lines explain a thing by
    likeness to another game's, as the button's summary does (N 4.11). Asked on 2026-11-15, one
    commit of comments alone. The likeness clause goes, and where it stood in for the rule the
    comment states the rule in the game's own terms. The lines that name the source of a number or a
    rule stay, and so do the command descriptions a person reads. Each hit is read, no code changes,
    the count before and after given. Open.

187. **Resource packs as hostile input.** The coder's pick of 2026-11-15, agreed. The packs come
    from the internet and from whatever lies in the packs folder, and no read has fed the readers
    malformed content. Read and fuzzed without a device, bounded by a timeout, an archive with a bad
    directory or a huge entry, a picture of absurd size or cut short, a model or a block state file
    malformed, deeply nested or vast, an animation's frame times at zero, negative or enormous and
    its frames past the strip, a connected texture's tile range, and a font's glyph sizes. For each
    what the reader does, with its place and a scenario, in a Reply before any mend. The Reply came
    at 19:38 on 2026-11-15, 27 cases run, eight findings, the worst a throw out of the reload ending
    the program at every start while the pack stays in the list. The mends are asked in two commits,
    each with tests that fail first and make their own pack. First, the reload building everything
    and swapping only when all of it built, a pack file read within a bound and from a regular file
    alone with a failed read taken as a file the pack lacks and the pack named once on the screen,
    the connected range held below the integers end and stopped at the atlas bound as it is read,
    the weights summed in long, and a section that cannot be meshed said in the log. Second, a
    picture refused by its header past a bound that an animation strip of a large pack still passes,
    one frame kept for each index, and a model held to 4,096 boxes of finite numbers. Open.

188. **The inventory windows and the world menus, the fourth read's six findings.** The read's Reply
    names six low findings in what the tests do not reach, and each is mended as the Reply proposes,
    in one commit or two. The light quality's key and the view's key held behind an open inventory
    or a menu as the inventory's key is, and the inventory's key held while the player is dead. A
    crafting table's window closed when the block at its place is no table. A pack on the list whose
    file is gone named once. The worlds screen scrolled by the wheel's sign as the packs screen is.
    The menu's save in a run that is not saved answering on the HUD what the console's save answers.
    With them, a picture refused by its size passed over for the next pack's copy in the same call,
    as an unreadable file is. Each with a test through the game's state, a seam no larger than the
    reload's where one is needed.

189. **The mobs, the drops and the block light, the fifth read.** Given at 22:20 on 2026-11-15, the
    coder's own read as the fourth was. The lane is `Entities` whole, the item drops, and
    `World/Lighting.cs`, read by what their tests do not reach. Look for a mob or a drop that
    outlives its column's unloading or is kept twice across a save and a load, a count that grows
    without a bound, a place or a speed that is not finite after a blast or a fall, a mob spawned or
    moved into a block, a hit or a pickup that acts behind a screen or on a dead player, and block
    light left behind or missing after a block is set, a column loads beside a lit one, or the
    origin moves. Each finding is told with the lines that make it and a test that fails on the
    head, in a Reply, the order proposed with it. The mends follow in one or two commits once the
    list is agreed, each test failing on a head copy. A finding that is the engine's goes to ASKS.md
    and is not worked around.

190. **The player and the land's generation, the sixth read.** Given at 22:45 on 2026-11-15, the
    coder's own read as the fifth was. The lane is `Player` whole and `Generation` whole, read by
    what their tests do not reach. Look for a player moved into a block or through one at a column's
    edge, a low frame rate or a long frame that tunnels, a fall's damage counted twice or lost
    across a teleport, a respawn or a mode change, a vital that leaves its range or is not finite, a
    view or a reach that acts through a wall, and in the generation a column that differs between
    two runs of one seed, a tree, an ore or a cave cut at a column's edge by the order the columns
    generate in, a height or an index past its array for a far place or a negative one, and a seed
    whose extremes overflow. Each finding is told with the lines that make it and a test that fails
    on the head, in a Reply, the order proposed with it. The mends follow once the list is agreed. A
    finding that is the engine's goes to ASKS.md and is not worked around.

## Verdicts settled

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

2. **The play job's held torch check fails on lavapipe after item 102.** Read at 12:09 on 2026-10-24
    from the run of `1f07038`, the three suites green and the play job red at its check "a torch in
    the hand does not light the roof about the player", the roof reading 27, 25, 39 at 640, 243 with
    the torch held against 0, 0, 0 without it, so the held light adds nothing there on lavapipe
    where the GPU's play.sh passed the same check in 136 s. The light moved from the hand to the
    eye's place in the body's middle at `1f07038`, so the reading is of that, a point light at the
    eye's own point or inside the player's box on lavapipe. The mend by the cause, read in the run's
    log and locally on lavapipe with the validation layer; the light's place kept at the player, the
    camera's turn having no part, as asked. One commit, before the next push. Mended at `fa6963d`,
    read at 12:21, the cause a point light at the camera's own point, which lavapipe shades with
    black specks across the frame, the probe's pixel one of them, where the GPU shades it whole; the
    shadow cube, the contact shadows and the global illumination each off left the specks, so the
    fault is the engine's shading of a light at the eye, sent to the engine as its item 97 through
    ASKS.md; the game's light stands a third of a block under the eye, within the player's box,
    still not turning with the view. Settled at 13:11 on 2026-10-24 on the run of `fa6963d`, green
    on the three suites and the play job in 36.0 minutes.

3. **The headless fixture's world does not load within its frames on the runners.** Read at 03:01 on
    2026-10-25 from the run of `01eb71f`, the three test jobs red on FireFrameTests' two tests with
    "the flat world did not load within 600 frames" from HeadlessGame's constructor after 140 and
    199 ms, 312 passed, the play job green in 52.4 minutes. The fixture steps 600 frames and asks
    the game whether it still loads, and since `01eb71f` set the frame target to 0 the 600 frames
    pass in a fifth of a second on the runners while the world's columns load on workers by wall
    time, so the frames run out before the world is in; on the development machine the workers are
    faster than the frames. The mend is the fixture waiting by wall time, stepping frames until the
    game has loaded or 30 s have passed with a short yield between frames while the world loads, the
    fixture's own test of a load, and every headless test on it; one commit, before the next push,
    read on the three runners. Mended at `76053f6`, read at 03:12, the fixture waiting by a
    stopwatch with a yield and the fire squares laid within the columns' reach. Settled at 03:44 on
    2026-10-25, the three test jobs green on the run of `76053f6`.

4. **The run of `1d31dda` is red, the play job's GPU caves check reading no column generated from
    the GPU.** Read at 07:56 on 2026-10-25 from the job's page and log. The three test jobs are
    green. In the play job on lavapipe, verify 2 read 0 of 147,000 readings over 4 columns
    differing, so the compute path runs there and gives the CPU's values; then voxel.tp 900.5 110
    900.5 and the script's meshed wait, and voxel.gpu read 'the GPU's reading of the caves' noises
    is on, 0 columns read since the world opened', where the NVIDIA GPU read 69 in 2 s. So on
    lavapipe the columns of the new place came from the workers' own reading and none from a batch's
    readback within the wait, the readbacks slower than the workers there, or the streamer not
    asking the GPU ahead of them for a teleport, or the count not moved where a worker took the
    readings; the coder reads which. The mend keeps the check as a check of the GPU path and not of
    lavapipe's speed, a wait on the batches in flight or a smaller place, with the reason said in
    the commit. The log is at the run's play job, run 40168842433. Settled at `9955bf3` on
    2026-10-25, the streamer's Pending counting the columns waiting for the GPU's readings beside
    the workers', so the play script's meshed wait holds until they arrive; the check unchanged.

5. **The run of `f97a4c9` is lost, the play job cancelled at its limit of 60 minutes.** Read at
    11:06 on 2026-10-25 from the run's jobs, run 40178969652. The three test jobs are green; the
    play job ran from 10:01 to 11:02 and was cancelled at the workflow's limit, which its comment
    says is reached only where the game or e3d hangs. Nothing hung. The job took 39 minutes on the
    run of `2549786` and 40 on `9955bf3`, 52 to 54 on the runs of `79ad110`, `bbe814c` and `0babebb`
    where the runner ran about 1.37 times slower check by check, and this run added item 131's
    screen probes, about 10% a frame on lavapipe by the coder's reading, and its new check, so a
    slow runner with the probes on passed 60. The mend raises the limit so a slow runner with the
    probes on fits, 90 minutes with the comment saying why, the rule weighing no cost of the Actions
    bill, and reads the job's log check by check for any check that grew out of proportion, saying
    so in the commit; the walk's checks are not trimmed for it. The mend goes ahead of item 95.
    Settled at `ddede23` on 2026-10-25, the limit 90 minutes with the reasons in its comment, the
    lost run's checks grown evenly and none out of proportion; the walk untouched.

6. **The run of `b7608c7` is red, the play job's water texture check reading no frame step on
    lavapipe.** Read at 17:58 on 2026-10-25 from the run's jobs and the play job's log, run
    40212359716. The three test jobs are green; the play job ran 19 minutes to the check after the
    water's depth and failed it, the water's texture not stepping through its frames, its green
    reading 114 eight times where item 39's animated textures step by their metadata's frame time.
    The game's log ends with Vulkan's validation saying an Output variable is a pointer to a float32
    and this mismatch between vector sizes is not allowed unless maintenance4 is enabled, a shader
    interface mismatch the entity path's pipeline or the terrain shader's new use may raise. On the
    GPU the play run passed in 389 s, so the fault shows on lavapipe or on the first run after the
    commit; the water stays on DrawMesh, so the coder reads what the entities' commit changed in the
    frame's time for the terrain shader, the animation table's step set with the Place pass where it
    was set each frame, or the shader values the entities' Material holds, and the validation
    message beside it. The mend goes ahead of item 29's meshing on the workers. Read at 18:09, the
    cause is the check and not the water. On local lavapipe with validation the two-frame water pack
    reads 74 and 115 in turn on both `ddede23` and `b7608c7`, so the animation runs; the check reads
    the last frame drawn, a frame takes its tick from the time it is drawn, and frames at a steady
    pace a whole number of tenths of a second long land on the same of the two frames whenever the
    script reads them, which the entities' faster frames on the runner likely made, the checks there
    running about a fifth faster. The mend gives ChunkRenderer HeldTicks and a voxel.ticks command
    holding the animated textures and the lamps at a tick, the check reading the pool at ticks 0 to
    3 for white and black alternating by 40 or more in green and then setting off, without a sleep.
    The validation lines are performance notes new with item 95, two in the game's log, a pipeline
    the entity path builds pairing the terrain's vertex stage with a fragment stage that reads none
    of its output at location 11, likely the water depth, the write discarded; the pairing is the
    engine's choice a pass, so it goes to the engine's item 125 and not to the shader blind. Settled
    at `117392b` on 2026-10-25, the animated textures held at a tick for the check, which reads the
    two frames in turn on the RTX and on lavapipe alike.

7. **The run of `c802dea` is red on the runners' dotnet install, not on the game.** Read at 21:00 on
    2026-10-25 from the run's jobs, run 40228490833. All four jobs failed within a minute at
    setup-dotnet, asked for 10.0.x and resolving 10.0.403, which the install script could not find
    as an SDK while the release feed's index named 10.0.402 the latest, the same failure that took
    3DEngine's runs of `23b4793b` and BevyCSharp's of `ddf51f5a` since 20:17, their Verdicts 92 and
    45; the commit touches the README and the devlog alone. Nothing to mend in the tree; the jobs
    run again when the engines' do, and the verdict settles when they go green. Settled at 22:35 on
    2026-10-25 by the second attempt, green on the four jobs once Microsoft's feed served the
    release.

8. **The play job of `06fd267` ended with the game gone.** The run of 06fd267, the license alone
    with the review's file, went red at 16:30 on 2026-11-08 in its play job at 58 minutes, after
    every check to the land's edge had passed, when `voxel.options post Ambient_Occlusion 0` found
    the game's process gone, its log ending in a stack through `E3D.EndDrawing` from
    `VoxelGame.Draw`; the three test jobs were green in two minutes. Nothing of the commit's touches
    the game or the engine, which rests at 6.0.91, and the same script passed on f37154b, so the
    exit is read as the runner's or lavapipe's unless it repeats. The play job is rerun at 16:34; if
    it ends red the same way, the item is the coder's, to read the game's log of the exit and hold
    the option's toggle in a test. The rerun ended the same way at 17:20, at 45 minutes, so the exit
    is the engine's and not the runner's. The game's own log from the play artifact has it, at 2,648
    s a NullReferenceException inside a memory copy in `SceneFieldRenderer.Pool`, called from
    `Record` and `Draw` in the render graph, then an AccessViolationException on the same copy and
    the process gone, seconds after the scene's distance field was made three times in two seconds
    with 5, 4 and 5 cascades as the ambient occlusion option was posted. The fault is 3DEngine's at
    6.0.91, the pool's buffers copied after the field's cascade count changed under them, and it is
    the engine's item 223; the game's run stays red until the engine's fix is packed and the game
    pins that package, and the game's session has nothing to mend. Recorded at 17:24. Settled, the
    play through the toggle and every check green on 8d8d1b2's run at 08:26 on 2026-11-09, the game
    on the engine's 6.1.12 with its item 223.

9. **The play job's engine build fails on the missing Sky module.** On the build run of `f43aa9e`,
    42580457818, on 2026-11-09, the three test jobs passed in under two minutes and the play job
    failed after 54 seconds in build/play.sh's build of e3d, the engine's compiler reporting eight
    errors of the type or namespace Sky not found in Api/SkyDay.cs,
    Graphics/GraphicsDevice.Atmosphere.cs and the Rendering/Lighting sky files, after the warning
    that the referenced project ../modules/Sky/Sky/Sky.csproj does not exist. The fault is in item
    161's ref. The engine took the Sky module as the git submodule modules/Sky at its `bd081fa6`,
    its own workflows check it out with submodules on, and the game's two engine checkouts, in
    build.yml's play job and in pack.yml, moved from d3473746, which had no submodule, to 8dba0d1c,
    which has one, without that setting. The mend is item 163, the setting on both checkouts.
    Settles on the build run of 163's commit with its play job green. Settled on that run on
    2026-11-09, the play job green in 80 minutes 11 seconds through the engine's build and the walk.

10. **The macOS test job of `cb071a9` failed its restore, the pinned package not yet at that
    runner's feed.** The run 42812809757 of the pin of 6.2.17, pushed at 13:05 on 2026-11-10, has
    its macOS test job red after 18 seconds, the play green in 91 minutes and the Windows and Ubuntu
    test jobs green. The restore reads NU1102, 3DEngine.Diagnostics at 6.2.17 not found and 6.2.14
    the nearest of three versions, at 13:05, where every one of the fourteen ids listed 6.2.17 from
    this machine at 12:57 and the other two systems restored it in the same minute, so nuget.org's
    index had not reached that runner eight minutes after it reached here. Nothing in the game is at
    fault. The failed job is started again at 14:39; the verdict settles when it is green. The
    review tells a pin's version a quarter of an hour after every id lists, not at once. Written at
    14:39 on 2026-11-10. Settled at 14:41 on 2026-11-10, the job green on the run's second attempt.

11. **The run of `068fd5f` is red in its play job before the game is built, the runner not reaching
    its package mirror.** In run 43038975424 the play job failed after five minutes at the step that
    installs lavapipe and the validation layer, apt-get ending with exit code 100, a temporary
    failure resolving azure.archive.ubuntu.com, the Mesa driver's package not fetched; the three
    test jobs are green. Nothing of the game's ran, so it is the runner's network and no fault of
    the commit's. The failed job is run again, started at 15:36 on 2026-11-11 as a second attempt,
    and the verdict settles where its play passes. Written at 15:36 on 2026-11-11. The second
    attempt failed the same way at 15:41, the mirror's name still not resolving on the runner from
    15:34 to 15:41 while it answers from this machine and GitHub's status page reports nothing, and
    the two Linux native jobs of the bridge's pack run failed by it in the same minutes. A third
    attempt is started a quarter of an hour on. The third attempt failed the same way at 16:03, and
    by then the fault had taken the Linux jobs of the bridge's two runs twice and both Linux legs of
    the engine's run of its `4d9227d0`, 30 lines of the same name not resolving, GitHub's status
    page reporting all systems working. From 16:07 the play job is run again every quarter hour as
    the probe, and the others' failed jobs are started again when its package step passes. The tenth
    attempt, started at 17:50 on 2026-11-11, passed its package step at 17:54 and its play at 19:26
    in 96 minutes, the run green, which settles this.

12. **The run of `6b0043b` never started, left queued by an outage of GitHub's Actions.** Run
    43096571236, made by the push at 21:31 on 2026-11-11, has stood queued with no job since,
    GitHub's status page reporting a major outage of Actions from about 21:35 to 22:11. At 23:01 it
    could be neither canceled, GitHub answering that a completed run cannot be canceled and that one
    not yet queued cannot be canceled by force, nor run again, the workflow already running by its
    word. The commit after it, `5ad3f58`, changes the review's file alone and starts no run by the
    workflow's paths. Nothing of the commit's failed, its own checks having passed here, the strict
    build, 367 tests and two plays. A watcher starts the run again when GitHub lets it go, and
    otherwise the run of the game's next commit that changes code settles this. Written at 23:03 on
    2026-11-11. The run of `88d6303`, the game's next commit that changes code, was green at 00:33
    on 2026-11-12 with the play in 66 minutes, which settles this; the run of `6b0043b` still stands
    queued on GitHub's side and is left.

13. **The run of `3bfa75d` is red on macOS in a test of the far game's save, the villager not where
    the test set it.** In run 43132544029 the macOS test job fails 2 of 386, both cases of
    FarGameTests.A_Game_Far_Out_Keeps_The_Players_Place_A_Broken_Block_And_A_Villager_Across_A_Save
    at its line 104, from `85160b6`. At -284,680.5, -33.5 the villager stands at x -284,684.049, and
    at 1,139,992.5, 596.5 it stands at z 595.801, half a block and 0.7 from where a whole or a half
    would put it; Windows and Linux pass the same tests and the play is still going. That reads as
    the villager having walked before the test read it, the count of frames before the read resting
    on the runner's speed, and not as a place lost by the origin; the coder reads which. The mend,
    the villager held where it is set or its place compared across the save against what was saved,
    goes up at once with `0aaf8e2`. Written at 03:09 on 2026-11-12. Read by the coder and mended at
    `636e0bb`, pushed at 03:15 with `0aaf8e2`. The headless game opened again ran the mobs from its
    first frame, the villager walking for as many frames as the world took to stand, and the test
    compared it with where it was set within half a block; with the mobs free and 120 frames stepped
    the case fails here as on macOS, the saved record and the home exact throughout. The game opened
    again holds its mobs still from the first frame and the test asserts the record's place and home
    against the villager's, exactly, after 120 frames. Settles on its run. The run of `636e0bb` was
    green at 04:03 on 2026-11-12, its macOS test job among it and the play in 48 minutes, which
    settles this.

## Decisions

1. **The old game is replaced in place, its history kept.** Decided on 2026-10-17, the voxel game of
   the engine's `3DEngine.Game` comes here in place of the old sandbox rather than beside it, the
   old files removed in the commit that brings the new in; the `legacy` branch and this history keep
   the old game. The game's commits in the engine's history stay there, cited by hash in the
   engine's REVIEW.md, and are not imported.

2. **The license stays MIT.** The repository's MIT license, the owner's own, stands with its year
   updated; the engine the game is written on is MPL-2.0, which a package reference does not carry
   into the game's code. This may be decided otherwise.

3. **`e3d` comes from the engine's checkout.** The engine's command line is a script of its checkout
   and not a tool package, so the game's `./e3d` wrapper runs the checkout `ENGINE_CHECKOUT` names
   or the folder above this one, which on the development machine is the engine, and the game's
   workflow checks the engine out at `fc5fdc97`, the commit of 5.2.43, under `.engine`. A tool
   package of the engine's command line is proposed for the engine's next package; the wrapper
   switches to it when it exists.

4. **What the game finds the engine lacks goes to the engine's ASKS.md, as before.** The engine's
   Decision 26 of 2026-10-10 stands: this repository's session writes an entry in the engine's
   `.github/ASKS.md` with what it measured and how, headed with this repository's name, and reads
   the engine's REVIEW.md for the `Review:` line the entry gets; the engine's session writes nothing
   there, and this repository holds no ASKS.md of its own. The game's tests run here on every push,
   which closes the entry of 2026-10-10 that said they ran nowhere.

5. **Prose is technical and generic.** Reported on 2026-10-17, phrasings such as "the light that
   bounces" and "how to drive the game" read as casual and do not fit. A feature is named by its
   technical term, the engine's own where it has one, and never by a figure of speech or a
   paraphrase: global illumination, and radiance cascades for the technique, where the light
   bounced; emissive where a block gave off light; a running program controlled or scripted through
   the command line where it was driven, and a scripted run where a drive. Added at 14:37, the colon
   is overused, an elaboration or a list appended to a sentence after one where a sentence would do,
   in documents, comments and logs alike, which STYLE.md should have covered; its rule tightens so
   that a running sentence holds no colon. STYLE.md carries both rules with these pairs, as the
   engines' do (3DEngine's Decision 43), every document, comment and message a reader sees is swept
   (item 4), and a new name follows the rule, `build/play.sh` where `build/drive.sh` was. On
   2026-10-17 at 20:34 the README's opening was read, "A voxel game in the manner of Minecraft, and
   the ground floor of a larger one", and the ask was that no phrasing of the kind stand either. A
   likeness is not a description and a figure of speech is not a term, so a thing is said by what it
   is and does, another product is named only where a rule or a number is taken from it, as the
   sizes, speeds and light levels are, and a plan is not written into a description; item 5, and the
   engines' items 29 and 25 under their Decisions 43 and 37.

6. **Development resumes; the sky, the shadows and the icons are the game's week.** On 2026-10-18 at
   22:25, with every run green on the engines and here, development resumed on both engines and the
   game. For the game the items named were the sky's sun, moon and stars as camera-facing billboards
   with textures and the moon's phases (item 6), the sun's shadows pixelated (item 7), the sun's
   surround pixelated with the rest of the sky smooth (item 8), and the block and item icons made
   anew (item 9). The radiance cascades the engine works on this week reach the game through the
   engine's packages after its commits, so the game's lighting changes wait for them and the items
   above do not.

7. **Nothing in the sky faces the camera; the sun, the moon and the stars are fixed on the sky and
   turn with the hour.** Decided at 21:05 on 2026-10-19, the sky's bodies are not to be billboards
   in the voxel game, since a thing facing the camera turns with the view and looks wrong in motion,
   and they behave as the other game's, placed on the sky and depending on position alone. So the
   sun, the moon and the stars are quads fixed on the sky's sphere, turned with the hour as one
   body, their orientation the sphere's at their place and never the camera's, and the words of
   Decision 6 on camera-facing billboards are read so; the halos around the sun and the moon are
   quads fixed with them on their grids, since a particle stands upright in the world and faces the
   camera, which would turn the halo's rings against the sun's pixels and with the view, so the
   particle effect Decision 6 asked for is read as the glow fixed with its body, and the engine's
   emitters stay for the game's other effects. Item 6 changes to it, and the engine ask the game was
   about to write for particles square on the screen is not written.

8. **The mobs' sizes and models may be the other game's, since they are numbers.** Decided at 22:03
   on 2026-10-19, the voxel game can use the original sizes and models of the mobs from the other
   game, since they are numbers. So a mob's cuboids, their sizes, places and pivots, and its height
   and width are taken from that game's model definitions and named as taken from it in the remark,
   as Decision 5 allows a number taken from another product; its textures and sounds are not numbers
   and stay the game's own, drawn in its pixel art. Added at 22:06, the game keeps a character of
   its own: its pixel art, its palette, its world, its names, its sounds and what it does its own
   way stay the game's, and the numbers taken are the mobs' sizes and boxes, so a reader knows the
   game by itself and not as a copy. Item 13 does it, read with item 10.

9. **The tonemapping is the engine's somewhat boring curve.** Decided at 22:06 on 2026-10-19, the
   game uses a somewhat boring tonemapping, the display transform of that name among the engine's
   curves, which keeps colors as they are and rolls the highlights off without a filmic look, so the
   game's pixel art reads as drawn. The game sets `Tonemap.SomewhatBoring`; item 14. At 22:36 the
   answer was to take exposure 1.25 beside the curve if it gives the light values back, since the
   curve alone reads the frame about 8 percent darker; item 14 measures that it does. 1.25 was
   chosen at 22:53, the day exact and the nights about a fifth brighter.

10. **The review's files are committed by the batch that comes next, and by the reviewer at most
    once a day.** Decided on 2026-10-20, the review's commits had become nine of every ten in the
    history and were to stop, and then the reviewer is to edit the review's files in the tree and
    commit nothing, as it did before 2026-10-19, with one commit a day at most where that makes
    sense. So REVIEW.md, SHARED.md and NORM.md stay edited in the tree and the coder's next batch
    carries them, as AGENTS.md has it; the reviewer commits them in a commit of their own only at a
    day's end where no batch carried them, never for a reading, a run or a package. The coder adds
    one sentence saying so to COMMITS.md in the next batch, in the paragraph on what a commit
    carries, naming the review's files, the batch that comes next and the one commit a day at most,
    and no one who decided it (N 4.7).

11. **The list of 2026-10-20 at 23:30, in the reviewer's order.** The game's next work was named
    whole, with the ask to the reviewer for the best approach, its order and its items: out of the
    water first; the mobs' proportions toward the source's with some character kept; more types,
    biomes, blocks and trees; the interface in its own style rather than ImGui's; the inventory's
    quick moves; the four modes with the switcher of icons and spectator; the ambient occlusion's
    strength; texture pack support, with the mod Shine in the reference folder as what adds a lot;
    stairs at corners, their placing and the aim's outline; caves as the source's newer ones; and
    the world's layout, cubic at 16 or 32 or a level of detail, an earlier one having scaled far
    blocks up so they looked as big as near ones, which looked wrong. The reviewer's order is items
    17 to 28: the small mechanics first in the order given where there was one, the water, the mobs,
    the modes, the quick moves, the stairs and the occlusion's strength, then the interface, then
    the texture packs, then the content, biomes, blocks and trees, then the world's layout as a
    reading first, the caves after it since they live in the deep sections it gives, and the looks
    Shine shows last since they build on the packs; Shine's code and shaders stay out, all rights
    reserved, and only what they show is done. The world's layout is the reviewer's answer to the
    question: generation by column with the world taller, everything else by section in three
    dimensions, and far land at coarser levels that keep the world's size and let far blocks look
    smaller with distance, the opposite of the earlier scaling. The game's coder reads item 17
    first.

12. **Compute shaders for generation, the direction of 2026-10-21.** Reported at about 00:30 on
    2026-10-21, compute shaders for generation and the rest are a big one too, with the ask to the
    reviewer for the approach as for the rest of Decision 11. The reviewer's answer is item 29: the
    blocks stay on the CPU, where collisions, edits, light and saves read them, and the GPU computes
    the noise fields and the meshes, the fields read back without a wait and the meshes drawn where
    they are written, with one noise in fixed point in both the shader and C# so the two paths give
    one world; the engine first reads a buffer back without a wait and draws a mesh over a shader's
    buffers, items 68 and 69 of its review, asked through its ASKS.md, and the game's parts follow
    each package. Items 18 to 25 go on meanwhile, and item 26's reading takes the GPU path into its
    costs.

13. **The chosen packs set the look, read from the player's copy and never shipped.** Reported at
    about 00:55 on 2026-10-21, the resource packs of the reference modpack, older but with visuals
    that are the aim, the HUD among them, are in the reference folder, Bare Bones and its textures
    are very good, and Fresh Animations and the better mods like it, a Bedrock add-on reworking all
    of this among them, give the general gist with no reference at hand. The reading: 30 packs, Bare
    Bones 1.21 with 1,059 block, 594 item, 607 entity and 320 gui textures and its add-ons, Fresh
    Animations 1.9.2 with 100 custom entity models in OptiFine's form and their animations, the
    packs fitting the two together, and smaller ones; Bare Bones carries no license file and Fresh
    Animations' terms allow personal use and building on it with credit and forbid sharing, so none
    of them ships and the game reads the player's own copies. So item 24 reads a list of packs
    layered as the source layers them, with their item and gui textures beside the blocks; item 23
    draws the interface in Bare Bones' look as the game's own art and in a loaded pack's gui
    textures where there is one; and item 30 reads the entity models, textures and animations in
    OptiFine's form, the game's own the fallback. Item 18 goes on with the scale, the cow and the
    pairs, since the game's own mobs stay what a player without packs sees.

14. **A modpack is the reference of the game's feel, read for shapes, numbers and ideas, and nothing
    of it ships.** Reported at about 01:00 on 2026-10-21, the mods of the reference modpack are in
    the reference folder too, for reference and for what was in the modpack, not to be added whole
    or at all, with the ask to the reviewer for the best approach. The reading: 223 Fabric jars for
    1.21.1 under `.ref/mods`, the packs moved to `.ref/resourcepacks`, each under its own license
    and left out of the repository. The approach: the modpack is the look and feel aimed at, written
    down, so each item reads the mods that bear on it for their shapes, numbers and what they show,
    as it reads the source itself, a model and never a copy; a mod that is data, the biome
    overhaul's JSON worldgen, is read for its numbers and the game writes its own; a mod that is
    code is read only for what it shows. Where that changed an item: item 20 gains the inventory's
    sort, item 23 the HUD's fading when full and unchanged, item 25 reads the biome overhaul's
    climate and surface numbers for its seven biomes, item 26 feeds the far levels from the saved
    world first as the far-chunk cache does, and items 18 and 30 are confirmed by the entity model
    and texture mods that load Fresh Animations' models. Not taken into items, named for a decision
    since the list is the owner's to set: structures in the world, villages, dungeons, mineshafts
    and ruins; ambient particles and weather, falling leaves, rain on surfaces, cave dust and wakes
    on water; ambient sound, footsteps by block and the sound's reverb and occlusion by the room,
    which the engine's default spatial processor of its item 58 begins; connected textures from the
    packs' properties; a zoom key; and the player's own animations in third person. The speed mods,
    the culling and the chunk threading, are what items 26 and 29 are about in the game's own terms,
    and the play mods, the combat and the trees, stay out until one is named.

15. **The six candidates of Decision 14 are taken.** Answered shortly before 00:55 on 2026-10-21, to
    the reviewer's list of what the modpack holds that was not yet an item, structures, ambient
    particles and weather, ambient sound, connected textures, a zoom key and the player's
    third-person animations, all sound good. So items 31 to 36, each after the items it builds on
    and otherwise in the order the list had, the play mods still out until one is named.

16. **The screen probes stay off in the game, and the cascades' faults go to the engine.** Decided
    at about 01:00 on 2026-10-21, the game is not to enable the screen probes, since they give a
    pixelated look even where none is meant, the radiance cascades without them are the better
    option for the game's art, and the cascades still show faults, a light block's light through a
    thin corner gap, zig-zag penumbrae, the screen probes' shift on movement, z-fighting with the
    opaque textures and noise. So item 38, the repros to the engine's ASKS.md first, which the
    engine's item 71 takes, and the screen probes off by default once the engine gives the call.

17. **The windows after Recolourful Containers and the HUD after Bare Bones, read from the player's
    packs, and the game's own art moved toward them.** Decided twice during item 24, shortly before
    02:44 on 2026-10-21, the HUD is to be done like the Recolourful Containers GUI pack, its gallery
    linked, and then those are taken directly and the hearts and the rest from Bare Bones directly,
    with some room for character of the game's own, those looking the best. The reading: Recolourful
    Containers 1.1.2 replaces the source's container pictures, grey slots with white edges, the
    crafting area on planks and the furnace's top in stone, and 73 HUD sprites too, 68 of them
    differing from Bare Bones'; Bare Bones carries its own container pictures beside its flat
    hearts, food, air, crosshair, dark olive hotbar with a green selection and its nine-slice
    widgets; neither carries a license file, so neither ships. The coder's first reading had Bare
    Bones carrying its own container pictures, which would have needed an entry limited to folders;
    corrected at 03:09, Bare Bones' container folder holds the anvil and the beacon alone, so the
    source's plain order, Bare Bones first and Recolourful second, gives the chosen order as the
    packs are, Bare Bones' hearts, food, air, hotbar, selection, crosshair, buttons and sliders over
    Recolourful's inventory, crafting table, furnace and furnace sprites, with Recolourful's tooltip
    and creative scroller coming along since Bare Bones lacks them and the creative window's items
    keeping the game's own art since neither pack has them; the hotbar from Bare Bones by decision
    shortly before 02:47; no entry names folders, the list being the plain order; and the game's own
    art, what ships with no pack, moves toward the two looks as the room for character allows,
    Recolourful's grey slots and white edges with the planks and the stone in the windows and Bare
    Bones' flat hearts, food, hotbar and green selection in the HUD, all drawn as the game's own
    pixels and nothing traced from either. Asked shortly before 02:50 what license the font pack
    has: qrafty's capitalized font holds one bitmap font picture and no license file, its page says
    all rights reserved with no terms, and Recolourful Containers' page says the same with terms
    allowing personal use and forbidding distribution of its assets in any form; so neither ships,
    and item 24's second commit reads the font picture from the player's pack where one has it, each
    glyph's width from its rightmost lit column as the source's bitmap font measures it with a
    pixel's gap and the shadow a pixel down and right, drawn at the interface's scale through the
    game's text calls, which fall back to the engine's font where no pack has one, the game's own
    art keeping the engine's font. Shortly before 03:23 on 2026-10-21 the game's own art was seen
    moved toward the two looks, uncommitted, flat hearts and food, a green lit button, white-edged
    slots, planks behind the crafting grids and stone over the furnace, and found ugly, the game's
    own windows and HUD named, with the ask to use what the earlier captures showed, the interface
    with Bare Bones, Recolourful and the font pack loaded; so the restyle was discarded before any
    commit, the game's own art stays as at `2839dba`, and the look asked for is the packs' own,
    which the fourth commit's setting gives at every start on a machine that has them, the packs
    read from a resourcepacks folder beside the saves and the setting naming which are on and in
    what order. Added shortly before 03:27, the windows need not say inventory or crafting and such;
    read as the windows' titles, Crafting, Furnace, the Inventory label over the player's rows and
    the creative window's Items, all off with a pack and without, the slots, tooltips, counts and
    menus keeping their text; a small commit of its own after item 24's last.

18. **The three packs are not shipped and are fetched at first start instead.** Asked shortly before
    03:29 on 2026-10-21, to include Bare Bones, Recolourful Containers and the font pack as defaults
    shipped in the repository. The coder said it cannot be done, all three being all rights
    reserved, Bare Bones' page forbidding any reupload, Recolourful's terms forbidding distribution
    of its assets on any platform in any form without consent, the font giving no terms, and a
    public repository or a release zip being that distribution; and offered three ways, of which the
    fetch on start was chosen: the game downloads the three pinned files from the creators' own
    Modrinth pages into its resourcepacks folder, checks each file's sha512 and writes the list in
    the chosen order, the repository holding only each pack's name, its Modrinth version and file
    URL and its hash, as a Modrinth modpack's manifest does, which Recolourful's terms allow for
    free modpacks. The reviewer adds that the fetch runs on a background task so the start is not
    held, the README says it, the Resource Packs screen shows it with a switch to stop it, and the
    request names the game in its user agent as Modrinth's API asks. Item 24's fourth commit.

19. **A screenshot on F2.** Decided shortly before 03:42 on 2026-10-21, as in Minecraft, a
    screenshot is to be taken in the game with F2, the same for BevyCSharp, the captures put in the
    root or a subfolder. So item 40, a screenshots folder beside the saves with the source's names
    and the HUD's line, and the engines' items 73 and 47 giving every program the key.

20. **Simple Grass Flowers as a fourth default pack, through block state variants.** Reported
    shortly before 03:53 on 2026-10-21, during item 25, Simple Grass Flowers looks great and adds
    variations of the ground beside the grass and flowers the game lacks. Its page allows modpacks
    with credit, so it joins the manifest as a fourth default pack fetched from its maker's page and
    credited, and the game reads a pack's block state variants for the surfaces its table names,
    picked per block by position as the source does; item 41, after item 25. Shortly before 05:00
    the pack was seen over Bare Bones and found to look good but to change the way Bare Bones' grass
    top looks and not to fit the style, with the recommendation that it be done natively in the
    style of the game, which looks very good; so the pack leaves the manifest, the game draws its
    own ground looks in its flat style, and the block state reader stays for packs that list looks.

21. **The look the game aims at is the modpack's gallery.** Reported shortly before 03:57 on
    2026-10-21, during item 25's biome commit, the visual aim is the modpack, Better Trailer
    Optimized+, with its gallery of 39 pictures, which the coder read and the reviewer looked at:
    Bare Bones' textures under a warm low sun with long soft shadows, calm water mirroring the sky
    and the shore, a blue haze over distant land, ores, lanterns, lava, sculk and a portal emissive
    with bloom in dark caves and halls, lush caves hung with glow berries and vines over clear
    pools, villages with flowers and lanterns, cherry groves, badlands of yellow and orange
    terracotta around turquoise pools, snowy spruce peaks, and embers and spores drifting in the
    dark. Against the game: the global illumination, the bloom, the fog, the sun's shadows and Bare
    Bones' textures are in; the rest goes by item, the glow of emissive blocks in item 28, the
    flowers and the biomes with the badlands and the cherry grove in item 25, lush caves with glow
    berries and vines in item 27, the ambient particles with the embers and spores in item 32, the
    villages' lanterns in item 31, and two items added for what no item held, the water's reflection
    and depth as item 42 and the sun's color by the hour with the soft shadows and the haze as item
    43; the nether stays out until it is named. Each item that carries a part of the look puts its
    capture beside the modpack's picture in the record, so the distance is judged by eye.

22. **Packs give pictures alone, and every rule is the game's own.** Decided shortly before 05:04 on
    2026-10-21, during item 41, the game has only visual resource packs, Simple Grass Flowers tried
    to implement logic, which is not to be done, the Shine mods too, and logic is implemented while
    textures and assets with licenses are pulled and used. So a pack gives its pictures and what
    says how a picture is cut, sliced or stepped, its textures, sprites and font and their metadata,
    each under its license; the game reads nothing of a pack's block states, models or rules that
    decide how a block is drawn or chosen, and every behavior is written natively; mods are
    references for what to build and are never implemented from their code or their data rules. Item
    41 drops its block state reader and keeps the game's own ground looks; item 30 reads a pack's
    entity textures and its models' geometry, the boxes, as assets and animates the mobs natively
    with the game's own gait, taking none of a pack's animation expressions; item 39's animated
    textures stepped by their metadata's frame time are a picture's own timing and stay; item 34's
    connected textures take the tiles and the method a pack's properties name, as metadata of a
    picture, with the methods written natively, open to a decision otherwise if the rule is read to
    cover it; item 28 was already what Shine shows done natively.

23. **The look without a pack takes Bare Bones' colors.** Decided shortly before 05:10 on
    2026-10-21, after item 41, for the look without a resource pack the game can use Bare Bones'
    colors to match it at least, since the game has different colors for them. So item 45, the
    surfaces' flat colors set to the pack's averages read as numbers, nothing of its pixels
    shipping, the plants keeping their own.

24. **The README's pictures, an animation at its head and pictures in its parts.** Asked shortly
    before 05:15 on 2026-10-21, to update the README's images, to put a GIF of different captures
    250 ms apart at the top, and to showcase things as images in the parts below, the interface and
    crafting among them. So item 46, every picture taken by the pictures script with no pack in the
    game's own art, none of a pack's textures in the repository. Added shortly before 05:25, the
    captures use the resource packs as well, so the pictures are taken with the three default packs
    on, read as their terms allow for pictures of the game, each credited with a link beside the
    pictures, the repository holding no file of any pack and one pair showing the look with no pack
    beside the packs'.

25. **Three words during item 46's scouting, the sky's bodies cut, the rifts in villages and the
    sun's look.** Reported shortly before 05:40 on 2026-10-21, on the pictures scouted, the sun's
    and the moon's flare are cut off at the bottom, with the question whether the far plane or the
    skybox does it, which is item 44's dome and item 47; there are a lot of rifts in villages, to be
    made rarer since they make the structure buggy and villagers could fall, which is item 48; and
    the sun and the flares fall short, the source's default dithered pixelated square one looking
    better, which is item 49. The three come before item 46's final captures, since the pictures
    show them. Also reported of the earlier village and forest shots, they are not composed well,
    other images being better suited and better looking villages existing, so both are replaced in
    item 46. Added shortly before 05:42, the camera lands inside blocks in some captures, with the
    ask to show the block's texture on the whole screen then and to check the eye before a capture
    and move out or find a better spot, which is item 50 and a check in item 46's script. Shortly
    before 05:55, on item 49's sun, Bare Bones' was preferred and is to be implemented in the native
    look without resources too, a smoothing of the dithering, the flares of the sun and the moon;
    item 51, the glow smooth as the pack's, read as numbers. Asked shortly before 06:02, what the
    stars are and what the source's are, and shortly before 06:02 the ask was for squares in place
    of the star shape and a try of the source's layout with its values, comparing both; item 52, the
    choice between the two made side by side. On the pair shown shortly before 06:10, the night sky
    in the modpack was found to look different, and a third layout from the modpack's shader read as
    numbers was shown and chosen. Shortly before 06:26 the ask came twice to capture with the
    resource packs for the README, and the report that there is too much bloom, 0.1 chosen of four
    shown, item 53. Added shortly before 06:46, the lantern could take Bare Bones' texture to look
    better, which item 39's first part takes with the torches.

26. **The engine's F1 and F3 come to the game.** Asked shortly before 07:05 on 2026-10-21 for F1 to
    hide the interface as Minecraft does, as an engine feature, F2 the screenshot, and F3 a debug
    overlay like the newest Minecraft's, sorted in parts logically with modules a game adds, world
    generation among them, showing the system, the camera and the renderer, memory, fps and
    frametime with a graph like the RivaTuner overlay's, both for BevyCSharp and 3DEngine, F1 hiding
    the interface and ImGui but not the F3 overlay so everything hides but the overlay over the
    renderer, and Shift+F3 showing the frame debugger too; and answered the reviewer's questions
    shortly before 07:06: the interface hidden by a scope, F3 the overlay's on its release alone
    with any chord left to the program, the look Minecraft's with no header, window styling or
    border, text on a transparent backing, and the order as the reviewer sees fit. The engines take
    them as their items 78 and 49, and the game takes them as item 54 when the package comes, its
    developer's parts as overlay sections and its F3 chords kept.

27. **More vegetation, and the plants cast shadows.** Decided shortly before 07:12 on 2026-10-21, on
    the README's pictures, there is way too little vegetation and the plants are to cast shadows
    too, the grass blades and such, which fit as they are pixelated. So item 55, the plants' shadows
    cut by their texture or an ask of the engine, first since it is small, and item 56, more plants
    from the source's counts and the modpack's look; item 39's lantern part waits behind them.
    Shortly before 07:19 the README's animation was found to switch too fast and two seconds a
    picture were chosen, and the vegetation meant is the grass and the flowers and such. Added
    shortly before 07:36, in Minecraft the grass and the flowers are randomly positioned and rotated
    or so, which the source does by an offset and no turn; item 57.

28. **The blades like the modpack's, and the light and the grade from the shader settings it is
    played with.** Answered shortly before 07:38 on 2026-10-21, on how the blades should look, as in
    the modpack; and comparing the colors the game's and the modpack's pictures differ, so the
    modpack is to be pulled and the shader settings it is played with fetched, the colors by time of
    day, the saturation and so on. The shader's license forbids redistributing or changing its code,
    so its settings and its formulas are read as numbers and the game's own written, item 58, the
    tonemap's curve asked of the engine; the blades' look comes with the light, the warmth and the
    deep shade being most of the difference. Of three grades shown beside the modpack's picture
    shortly before 08:01, Reinhard graded was chosen; and the game's grass was found tealish where
    the modpack's is green, which is the game's climate tint against the source's fixed colors a
    biome, item 59. Asked again on the green grass shortly before 08:16, the answer was that it
    looks good but the modpack's saturation is to be checked, and of 1.05, 1.2 and 1.4 shown 1.4 was
    chosen; and screenshots are taken zoomed with alt to about 40 degrees, which the pictures'
    retake takes.

29. **The sun's shadows step on a grid of eight a block.** On a house and a tree's shadow shown at
    sixteen and at eight steps a block and at a reach of 64, before the commit of 08:46 on
    2026-10-21, the choice was to pixelate at 8, a block having 8 shadow pixels, so the terrain
    shader takes each point's light on a grid of eight steps a block, half a texture's sixteen
    pixels, as the modpack's shader steps its shadows with the settings it is played with; the reach
    stays 96, since 64 dropped part of a near tree's shadow in the capture and was not taken. Read
    on the question of about 09:20 as the sun's light and shadow alone, the lamps' light and the
    global illumination smooth, item 60.

30. **The mobs take Bare Bones' entity pictures.** Asked shortly after 09:40 on 2026-10-21, the
    villager's face from Bare Bones, and the zombie's too, and the other mobs', so every mob wears
    the pack's entity picture over the source's cuboids, item 62, the pictures fetched with the pack
    as Decision 18 has it and the game's own colors kept where no pack is given, as Decision 22
    keeps every rule the game's own.

31. **The zombie's own face is the source's archetype, the villager's eyes lower.** Asked shortly
    before 09:50 on 2026-10-21, to change the default native face of the zombie to the source's
    archetype default, horizontal lines and a nose but no mouth, and to set the villager's eyes a
    bit lower, item 64, the game's own art that shows where no pack is given.

32. **The Bare Bones x Fresh Animations pack is the fourth default pack, above Bare Bones.**
    Reported shortly before 10:15 on 2026-10-21, a mod in the modpack replaces the creeper's skin
    for Bare Bones, to be checked; it is the Bare Bones x Fresh Animations pack of TeamHibiscus,
    highest of the three mob packs in the modpack's order, under the Unlicense on Modrinth, so it is
    fetched as the others are under Decision 18 and listed above Bare Bones, item 65, its pictures
    laid on the source's cuboids as Bare Bones' are and its animated models, which need Entity Model
    Features, not drawn. Checked at 10:45 and withdrawn: the pack's pictures differ from Bare Bones'
    only where Fresh Animations moves the eyes onto its animated model's pieces, which the game does
    not draw, so the pack is not a default pack and the mobs stay on Bare Bones', item 65 dropped.

33. **The creeper keeps the vanilla look, the game's own art, under every pack.** Decided shortly
    before 10:38 on 2026-10-21, the creeper's picture in Bare Bones is not taken and the vanilla is
    kept; the game cannot carry the source's picture, so its own creeper, drawn in the source's
    style, stays whatever packs are loaded and no pack's creeper picture is read, item 66, item 65's
    pack included.

34. **The observations of 2026-10-23 and their order.** After play, a list of what was seen came on
    2026-10-23, written as items 67 to 86: the inventory screens' keys, drag and dropped items, the
    breaking's cracks, particles and kept progress, trees felled and gravity blocks falling, the
    land's light above the eye's height, the stars' dark backing, fire, TNT and the creeper's blast,
    a held light, the hand in view, the shadows' flicker with the day, the fog's color by height,
    the grass's balance and the tall grass, swimming, villages' spacing, the biomes' climate, plants
    over carved caves, connected glass, a loading screen, the grass plant's top line, and
    reflections off. The order the reviewer set: item 39's parts in hand first, then the visual
    faults, 70, 85, 71, 86, 82, 84, 77, 76 and 83, then the play, 67, 68, 79, 78, 80, 81, 69, 75,
    74, 72 and 73, then the pictures taken again, then the items that waited.

35. **Trees fell as one and gravity blocks fall.** Decided on 2026-10-23, trees should break all
    wood and break their leaves with a particle effect, some wooden blocks dropping down still, and
    sand and the like dropping, so the game fells a tree from its cut log with its leaves decaying
    and a share of its logs falling as blocks, beyond the source's rule, and sand, red sand, gravel
    and concrete powder fall unsupported as the source's gravity blocks do, item 69.

36. **Glass and water reflect nothing for the present.** Decided on 2026-10-23, the reflections on
    glass and water show the scene field, which clashes with the voxel art style, and should remain
    non-reflective for the present, item 86; reflections return by a later decision.

37. **Branches are named for their work, never claude/ or a random string, and work stays on main
    where one session writes.** Decided on 2026-10-23, no branches are created named claude or the
    like nor with random characters, and work is on main, or, where several sessions on one
    repository make a branch necessary, on a branch with a descriptive name; N 7.5 holds it, and
    COMMITS.md says it.

38. **The empty hand shows no arm.** Reported to the game's session at about 04:30 on 2026-10-24, on
    item 75's hand in view, nothing is drawn when the hand holds nothing, so the arm mesh and the
    skin go in a commit of its own on top of `1dce7b7`; the held item stays as item 75 has it.

39. **A devlog under `.github/devlog`, and AGENTS.md's rows for it and for the pushes.** Asked at
    about 04:35 on 2026-10-24 that the pictures the sessions made be fetched and saved with a text
    in a folder under .github; `.github/devlog/README.md` holds what landed by day, newest first,
    with the repository's captures, the items closed and the pictures sent on the way as WebP files,
    written by the reviewing session and carried by the coder's commits like REVIEW.md. At about
    04:50 the game's session was given the word for AGENTS.md's Documents row and for its
    Conventions line to match COMMITS.md's pushes by the reviewing session. At about 07:56 the ask
    was that the devlog be `DEVLOG.md` at the repository's root, the pictures made on the way
    staying under `.github/devlog`; `.github/devlog/README.md` goes and AGENTS.md's row names
    `DEVLOG.md`.

40. **Far land at 32 columns by default, the render distance kept at 12.** On item 26's readings of
    2026-10-24: far land costs under a millisecond of GPU at any distance, so it stands on at 32;
    the render distance stays at 12 since it was set so by choice, and the frame there, over 16.7 ms
    from the taller world's draws on the main thread, is item 90's to bring back rather than the
    distance's to give up; near 8 with far 32 holds 60 at 1 GB where a machine needs it.

41. **Far land goes.** Found in play at about 10:11 on 2026-10-24, the far lands look bad and are to
    be removed; so item 26's third and fourth parts go as item 92, the taller world and the sections
    by reach staying, and Decision 40's far land at 32 stands no more.

42. **The screen probes are off by default.** Seen in play at about 10:11 on 2026-10-24, black
    spreading across the screen that goes with the screen probes off, and the light spotty, noisy
    and unstable on movement; the black is the engine's item 90 and the rest the engine's sixth
    term, the probes' own, so the game leaves the screen probes off by default as item 91, the
    setting kept, and they are judged again by eye when the engine's package with item 90 comes.

43. **The game uses the engine's ECS where it fits, beside the loop.** Decided at about 10:23 on
    2026-10-24, the game should move to the ECS where it fits, the engine's ECS being its own
    feature over raylib, with the immediate loop kept where it is best, loops and data such as the
    chunks in the ECS and the whole easier to read; and answered the reviewer's questions: the
    things that move, the world's sections as mesh entities, and the player and the states move,
    generation and meshing stay on the workers; piece by piece on main after items 92 and 93, the
    game playable at every commit, no branch; where the ECS falls short the engine grows first, each
    gap an entry in its ASKS.md taken ahead of other engine work; and the engine's DESIGN.md says
    both where each fits with this game as the model, the engine's own small games staying as they
    are. Items 94 to 96.

44. **The clouds cover the area about the player and do not reach the horizon.** Asked of the game's
    session at about 15:00 on 2026-10-24, on item 108's try, that the clouds not be stretched so
    far, since the before images looked right where the clouds reached not to the horizon but
    covered the area around. So the clouds stay within twice the render distance of the eye, fading
    into the sky from six tenths of that reach, as Clouds.cs has them; item 108 is closed and its
    build reverted.

45. **A session saves the pictures of its work in the repository, N 4.8.** Decided at about 18:5x on
    2026-10-24, after BevyCSharp's session kept none of a day's captures, the rule should be written
    where the sessions read it and be the same in the three repositories, as this game does. N 4.8
    says it, the game's pictures under .github/devlog/ as WebP files named by the date and the item
    in the commit of their work with the commit's sentence as the caption, DEVLOG.md generated from
    them, which the game's session has done since Decision 39 through the reviewer's collection and
    does itself from here on.

46. **The README's pictures taken again, an animation at its head and pictures in its parts
    (Decision 24).** Asked shortly before 05:15 on 2026-10-21, to update the README's images, to put
    at the top a GIF of different captures with 250 ms between them, and to showcase things as
    images in the parts below, the interface, crafting and such. At the head, one animated image of
    about ten captures 250 ms apart, the village, the hills, the mesas, the cherry grove at dusk
    with its lanterns, the jungle, a swamp, a cave lit by glowstone, the night sky, a river valley
    and the snowy peaks, each from a fixed seed, place, hour and heading in `build/pictures.sh` as
    the head pictures are, encoded together at 800 by 450 with the tools on the machine, a GIF as
    asked, measured beside an animated WebP of the same frames, which the site plays too, and the
    GIF taken unless it is too large, said with the sizes; the engine's recorder of its item 70
    takes the frames once its package comes. In the parts below, pictures at 800 by 450 by the same
    script, the biomes as a sheet of four, the plants and the grass's looks, the trees, water, glass
    and ice, torches and lanterns lighting a room at night, the interface with the HUD, the
    inventory, a crafting table, a furnace and the menu, the resource packs' screen, and caves and
    ravines; the script gaining the interface's shots through input keys and commands and a list of
    what it takes. The decision shortly before 05:25 on 2026-10-21: the captures use the resource
    packs as well. So the pictures are taken with the three default packs on in the manifest's
    order, read as the terms allow, a picture of the game drawn with a pack holding none of its
    files where Bare Bones forbids reuploading any instance of the pack, Recolourful's terms
    allowing its use in any form of media with a link to its page, and the font's page offering it
    for content creation; the repository still holds no file of any pack, and the README credits
    each pack with a link beside the pictures. The pictures script fetches the packs by the
    manifest's URLs into a captures folder, checks each hash and opens the game with a pack flag for
    each; the resource packs section keeps one pair, the same view in the game's own art and with
    the packs, so the look with no pack stays shown; the Resource Packs screen is left out since a
    transient run lists no packs and prints the machine's folder; a `voxel.scale` command sets the
    interface's scale for the scripted shots, the windows taken at 2 and cut out at 800 by 450 with
    no resampling. One commit, after item 45 and before item 39. Done at `7039d5a` on 2026-10-21, as
    the head says, the GIF at 1.45 MB. The animation's pictures at two seconds each at `fc38219` by
    decision. The pictures are taken again after item 39, one run covering the plants, the lantern
    and the cutout. Decided shortly before 08:16, screenshots are taken zoomed with alt to about 40
    degrees of field of view and the cinematic captures should too, so the retake takes the world's
    views at 40 degrees through a `voxel.fov` command, each view's place moved back to frame the
    same subject. From item 80 at 03:5x on 2026-10-24: seed 1's villages stand elsewhere, so the
    plains village views at 15.4, 700.6 and the snowy and night views at 454.4, 77.4 point at empty
    land, the desert view at 720.4, minus 697.6 still at its village's edge; the retake moves the
    plains views to a village of seed 1, 576, minus 432 the nearest, and the snowy one to another
    seed or a village found with voxel.find, none standing within 1,600 blocks on seed 1; the
    plains-village.webp of `b032e76` shows a village that no longer stands there until then. The
    retake at `5556519` on 2026-10-24, the views as the head says; play.sh passed; item 46 done.

47. **The devlog written as the work goes.** Asked on 2026-10-25 at 05:13 that the sessions write
    the devlog more often than at a day's end, though not at every commit. N 4.8 gains the cadence,
    an item that changes what is seen ending with at least one captioned picture, a pair before and
    after where it mends and one a phase where it is built in phases, and DEVLOG.md regenerated by
    the review as each item is archived, the coder's next commit carrying it as it carries the
    review's files; the three devlogs were regenerated at 05:17 with the day's pictures and items.
    The same in 3DEngine (Decision 71) and BevyCSharp (Decision 64).

48. **The screen probes are on, off a debugging switch alone.** Decided in the engine's session
    before 09:19 on 2026-10-25, with the engine's item 103 held on its cost, that the screen probes
    stay on as the engine recommended and that off is for debugging alone and not for a player.
    Decision 42's default of off, taken when the engine's item 90 spread black across the screen, is
    over, item 90 mended since; item 131 turns them on and takes the setting out of the options.

49. **The devlog carries notes beside the pictures.** Asked on 2026-10-25 at about 12:01, shown the
    table of the screen probes' cost at four resolutions from 3DEngine's item 122, that it be
    written in a devlog or something along those lines. N 4.8 gains the note, a reading worth the
    devlog's reader with no picture to carry it, a table or a few sentences, saved beside the
    pictures as a Markdown file named the same way, and DEVLOG.md carries it on its day under the
    pictures; the generator reads them, and the first note is 3DEngine's
    2026-10-25-item-122-resolution.md. The same in 3DEngine (Decision 73) and BevyCSharp (Decision
    65).

50. **The F3 window's parts find their homes.** Asked by the game's session on 2026-10-26 under item
    54. The readouts become the engine's overlay sections, Player with the feet, the heading,
    flying, the target block or mob with its face's light, health and food; Generation with the
    world's kind and seed, the biome, the temperature and humidity at the eye and the GPU caves'
    state and backlog; Sky with the hour and the day; Light with the sky and block light at the
    eye's cell; Sections with the columns, the generating, the ones to mesh, the drawn, the draws,
    the triangles, the entities, the lamps, the clouds' faces and the mesh and stream updates; Mobs
    with the loaded, the drawn, the draws and the kept. The developer's toggles go to a Debug
    options screen reached from Options, the engine's three windows, global illumination, the
    frame's profile and the effects over the frame, and the game's Light Levels and Cull Unseen
    Sections, since a player sets none of them; Corners shaded goes, the Corner Shade slider at 0
    being the same; Mobs stand still stays the command voxel.still. Of the World part, the mode is
    F3 and F4's and voxel.mode's, the button saving at once goes with the game saving every 30 s and
    on leaving, and the kind, the seed and opening or beginning a world go to item 134, a world
    screen, voxel.world keeping them meanwhile; open to a decision.

51. **The far cascade is on by default and the shadow distance is 128 blocks.** Decided on
    2026-10-30, the far cascades should be default on and the shadow distance 128 blocks, over the
    reviewer's call of 2026-10-26 under item 27 that kept the far cascade off for its 1.6 to 2 ms;
    item 141 does it and reads the frame again.

52. **A voxel engine in Rust is the reference for the game's performance.** Pointed to on
    2026-10-30, github.com/rornic/ochre for the voxel sandbox's performance; the game's session
    reads it for what it does about meshing, drawing and streaming that the game does not, and
    proposes items from it with readings, the engine taking what is the engine's through ASKS.md.
    Read on 2026-10-30, six proposals in the order the readings say binds the frame: the game's
    sections drawn in groups (item 143), shadow casters culled to each cascade's slice swept toward
    the sun (the engine's, by ASKS.md), faces merged where the triangles halve (144), streaming by a
    time budget (145), packed vertices (the engine's, by ASKS.md) and the far columns at double
    cells (146); the reference has no occlusion culling, where the game's flood culling stands
    ahead.

53. **No far land in the game.** Made clear on 2026-10-30, there is no far land in Voxel-Sandbox
    either, holding to Decision 41 of 2026-10-24 that took the far land out; item 146, the columns
    past 8 drawn at double cells from the Rust engine's octree levels, is withdrawn, and no level of
    detail past the render distance is proposed again without the owner's word.

54. **Held lights are plain lights without shadows.** Decided on 2026-10-30, the handheld lights in
    the voxel sandbox are simple lights and are not to be given shadows, over item 74 of 2026-10-24
    where the held light cast them; item 147 takes the shadow off.

55. **A reading's picture explains itself, and a plain capture or pair stays where it shows the
    thing.** Reported on 2026-10-31, 3DEngine's sheets for item 136, the canopy and the grove with
    their labeled panels, difference panels and lettered boxes, were found good, plain captures and
    side-by-side views are good too, and each is used where it fits. N 4.9 in the three repositories
    says what a sheet carries. On BevyCSharp's item 82 sheet shown at 09:1x the same day, the rule
    was said to be no list of parts to implement, the plain captures and pairs made before being
    already right, and four questions were answered, a principle and not a checklist, labels where
    panels could be confused and a box only where a number is read there, a white ground as before,
    and difference panels and plots where they tell the reading. N 4.9 is rewritten so.

56. **Every picture a session makes goes to the repository's dump folder.** Asked on 2026-10-31 at
    09:3x that all the pictures a session makes be kept apart from the devlog's chosen ones, in a
    dump folder, for every project, and chose the folder inside each repository and ignored by git,
    the pictures as captured in PNG named by time, item and a word, and a captions line a picture
    written as the shot is taken; N 4.10 in the three repositories. Added at 09:5x, the sessions had
    captured and compared pictures without showing them, with the ask that every picture a session
    reads or compares be shown in the session as it is made, none kept behind the scenes; N 4.10
    says so.

57. **The version's major and minor move by the reviewer's judgment.** Reported on 2026-10-31 at
    12:4x, the reviewer moves build/version.txt by its judgment of the project, commits it and
    pushes it, no other hand needed. The judgment is a minor at a step the project has taken, a
    capability or a changed surface that a package or release carries, and the major by decision.
    The game moves to 0.2 at `7e052e8`, 0.1 having stood since 2026-10-17 over 235 commits.

58. **No one is named in a picture.** Decided in the 3DEngine coder's session on 2026-11-03 before
    08:05, on a reading sheet whose label named a person as a reference picture's source, pictures
    and comparisons must not name a person, as the rule for documents has it. So N 4.10 says a
    picture's labels, its file name and its captions line name no person and no session, a picture
    sent in being a screenshot and a reference picture the reference; the engine session relabelled
    the sheet, renamed the dump's files and reworded its captions back to 2026-10-31, and read the
    59 comparison sheets' labels clean.

## Replies archived

Item 54, under Decision 50. The settings that moved are Light Levels and Cull Unseen Sections, which
went to a new Debug Settings screen beside the engine's windows of global illumination, the frame's
profile and the effects over the frame, none of them kept in the options file. The settings the
options and the menu already had are the shaded corners, which Render Settings' Corner Shade at 0
turns off, the mode, which F3 with F4 switches, and saving, which the game menu's Save the World
does. AGENTS.md's row for `VoxelSandbox/Ui` names the settings window, which item 54 takes away, and
the row is the owner's to change.

Item 134. The engine's interface has no text field of its own, but it gives the characters typed
(`GetCharPressed`), a key's repeat (`IsKeyPressedRepeat`) and the input method's place
(`SetTextInputArea`), so the seed is typed into a field the game draws in its own interface's look,
and the toggles' keys are held while it is open.

Item 27. The far cascade ships off by default with a Far Cascade toggle on Render Settings, its reach
the render distance's, the reviewer's call on the readings, since it darkens the deep cave's far
walls at Low alone and costs 1.6 to 2 ms of the frame at Low and High.

Item 166. Read on the engine's 6.1.12 between 21:34 and 22:03 on 2026-11-09, offscreen at 1280 by
720 at the game's settings on tuned's power-saving profile, the cave and the checks after it under
the GPU's lock, every frame recorded through 120 standing, a walk of 180 frames at 0.0717 blocks a
frame, the stop, a turn of 180 at 0.25 degrees a frame, about a quarter of a probe tile at the
window's middle, and the stop, the mobs, the clouds and the animated textures held. Standing, no
pixel of the grove or the cave moves past 12 levels from one frame to the next, and the village
moves 0.001 to 0.004 percent, its torches' flames and the particles over its roofs. In the walk and
the turn each frame moves from the one before as the same two poses settled do, within 0.3 percent
of the window past 12 levels, and differs from the settled picture of its own pose on 0.55 to 1.03
percent past 12 and 0.12 to 0.32 past 25 in the grove, in the screen probes' 8-pixel tiles, which is
240's kind, 0.02 to 0.11 and under 0.03 in the village, and 0.01 to 0.07 and under 0.04 in the cave.
After each stop the picture holds within one to three frames. The motions' starts aside, the share
jumps at one frame, the grove walk's 123rd, where the near clouds take 15 levels less at once, the
far cascade's move by a cell changing the light the model pass gives them, as settled shots with the
far cascade off and with the illumination off show. It stands in the engine's ASKS.md as an entry of
2026-11-09, 239 being the nearest item. No cycle of 237's kind shows, nor an arrival after a cut,
since a walk has none, and the cave reads a mean of 0.3 to 3.6 of 255, so its shares say little
before 241's reach. So the noise seen in play is not in these motions at these speeds on 6.1.12, a
turn of a few degrees a frame as a hand makes it is not read, and by Decision 64 a recording from
play with its settings file would show where the noise is. The sheet is 22-05-item-166-sheet.png in
the dump, sent, with the three recordings and the frames beside it.

Item 166, the second read. Recorded under the GPU's lock between 22:11 and 22:35 on 2026-11-09 as
the first, the frame's time held at a sixtieth of a second by `time.frame` so the day's clock runs
the same in every run, each sample shot settled at its own pose and hour. Each frame against the
settled picture of its pose, past 12 and past 25: the grove's turn of 90 degrees in 2 s, 0.75
degrees a frame, 0.023 to 0.056 and 0.009 to 0.013 percent, in 0.5 s, 3 degrees a frame, 0.024 to
0.048 and 0.009 to 0.014, its sprint at 0.094 blocks a frame 0.73 to 0.99 and 0.23 to 0.32, and the
cave at night walked with a torch in hand 0.016 to 0.119 and 0.007 to 0.091, turned 0.029 to 0.038
and 0.007 to 0.014. The same place settled twice differs by 0.35 and 0.07 percent walked to and
teleported to in one run, and by 0.69 and 0.25 teleported to in two runs, in the screen probes'
8-pixel tiles, a settled view's frames being the same to the bit. So the grove's walk, its sprint
and the first read's turn after the walk differ from their poses settled by what the path leaves,
and a turn made where the eye settled matches within 0.06 at every speed, so no lag of the light
behind the motion shows at any speed read. What moves in play is which held estimate a place shows,
which stands in the engine's ASKS.md as a second entry of 2026-11-09, 240's kind the nearest, and it
replaces the first read's placing of the grove's 0.55 to 1.03 percent under 240 alone. With the
clock running, the sun's light steps every 83 frames by the game's 36 steps an hour, moving 0.49 to
0.60 percent past 12 in one frame and a tail of 0.01 to 0.03 for up to 40 frames as the illumination
follows. The sheet is 22-37-item-166-sheet-second.png in the dump, sent, with the two recordings,
the settled shots and the path's pictures beside it.

Item 166, read again on 6.2.12 at the pin `cfffcb3` and on 6.1.12 from a build kept beside it,
between 02:54 and 05:10 on 2026-11-10, under the GPU's lock with the frames on disk and each script
in a scope with a memory bound. It corrects the second read. The walk ends where `voxel.flicker`
puts it by adding its step in floats each frame, at -739.33984, -972.4695, and the second read
teleported to that place rounded to a hundredth, half a millimeter off, and placed the first read's
settled poses in doubles, up to 3.6 millimeters off. Half a millimeter moves 0.34 percent of the
window past 12, 0.17 of it with the illumination off as the sun's shadow edges move a texel of its
map, so the 0.35 and 0.69 percent of the second read were the poses and not the path. Teleported to
the walk's own end, the place walked to and the place teleported to differ on no pixel past 12, two
settlings of one place on 0.003 to 0.006 percent and two runs on 0.002 to 0.003, on both packages,
and the engine's 246 is withdrawn.

With each pose held placed as the game's floats place it, by a script that repeats FlickerRun's
steps and the heading's conversion, every frame in motion matches its pose held past 12 within 0.018
percent for the grove's walk and slow turn, 0.068 for its turns of 0.75 and 3 degrees a frame and
0.074 for its sprint with the clock running, whose own floor standing is 0.039 to 0.054, and 0.079
for the cave walked and turned at night with a torch, the same on both packages. So no frame in
motion lags its pose at any speed read. The cave is brighter on 6.2.12 by a mean of 0.5 to 1.5 of
255 as its torches reach further by the engine's 241, and the grove differs between the packages on
0.05 percent past 12. The clouds still step at the walk's 123rd frame, 11.65 percent past 12 against
9.4 on either side, the near cloud reading 158, 178, 205 then 144, 161, 184 at the walk's poses 90
and 122 with the far cascade on and holding without it, the same to the level on both packages, the
engine's 245.

The turn carried picture to picture by the engine's `build/flicker.py`, at 0.75 degrees a frame in
the grove with the screen probes on, off and the illumination off, reads 10.9, 9.9 and 9.8 percent
of the window past 4, a floor the carry leaves on the game's hard-edged textures. Read as the pixels
a pair moves past 4 with the probes and not without at the same heading, after a blur of radius 3,
the probes move 1.23 percent of the window and 15 percent of the shaded ground under the trees,
against 0.25 and 0.65 between the probes off and the illumination off, and 1.4 percent of that
ground past 12, in blocks of the probes' 8-pixel tiles among the grass and the petals, the same on
6.1.12 and 6.2.12. That is the likeliest form of the noise seen in play, and the engine's ASKS.md
carries an entry asking `build/flicker.py` for the two forms and a map that follows the turn. The
sheets are 03-10-item-166-pose, 03-28-item-166-carried-turn, 03-52-item-166-carried-masked and
05-20-item-166-poses-held in the dump, each sent.

Item 167, with the pin of 6.2.32. The bare pin was never read on a sound build. Its failing run at
the lantern's pool came from a build made in a git worktree reached through `/home`, a link to
`/var/home`, where MSBuild copied the game's resources flat beside the program, so the game drew
without its terrain shader, and the engine's 252 is withdrawn. Built through `/var/home`, the pin
alone, the pin with only the center swapped for `SetSceneFieldAhead(0.5f)` and item 167's tree with
the center each frame put back read the pool from 249.2 to 90.6 to 90.9 in 3 runs of 3 each, as
6.2.17 does. The field lies ahead through `SetSceneFieldAhead(0.5f)` in `ApplyField` in place of the
center set each frame. The game's two copies of the sRGB curve, in SkyColors.cs and TerrainAtlas.cs,
call the engine's `SrgbToLinear`, `LinearToSrgb` and `ColorFromLinear`, which give the same bytes.
The three powers of 1/2.2, two in SectionMesher.cs and the stars' in Sky.cs, stay with their reason
in a comment, since against the sRGB curve the shader decodes by they show light level 0 a fifth
brighter and a faint star three times, so the curve in their place would darken the caves and the
night sky. Item 167's captures match 6.2.17's, two runs with the item differing from each other as
much as from 6.2.17's, dusk 2.5 percent past 24 levels and every other picture under 0.2. With the
shaders cached the water's mirror failed in four of six runs, terrain.slang's pipelines made in play
each run though the pipeline file was read, the engine's 253, and from an empty cache the whole run
passes in 375 s. 3DEngine.GlobalIllumination comes in through the game's 3DEngine reference. The
sheet 01-27-item-167-room in the dump shows the flat build's room beside the sound one, sent.

Item 168, read on 6.2.32 with the engine's 251, at e3dea22's tree. The grove's teleport, three cuts
at the default and two at 131,072, has the mean within 2 levels from frames 7, 6 and 9 against 8
and 10, and the share past 12 under 0.1 percent from frames 21, 36 and 42 against 52 and 66, where
35 to 77 on 6.2.17. At the streaming edge over the river a section's light lags its mesh by 10
frames at the 75th part and 16 at the 90th at both budgets, 137 and 156 at most, where 21 and 50 at
the default on 6.2.17. So the larger budget gains nothing with 251's lift, and the budget stays at
the default with no change to the game. The walk west over the plains from -736, -1358, its route
probed first since the grove's east walk stood against a cherry tree, and the sprinted flight, in
the order a b b a under the lock alone, read the walk's frame 15.27 and 14.94 ms at the default
against 15.13 and 15.71, and the flight's median 11.06 and 12.39 against 14.83 and 11.96, with the
machine's load near 7 from other builds and tests throughout and the client's build of item 170
beside the middle two runs, so they decide nothing. A taking from 03:30 that read 25 to 43 ms a
frame ran beside a game a failed play had left running outside the lock and is set aside.

Item 169. The Content items name their place by their folders, `Link="resources\%(RecursiveDir)..."`,
and a build through `/home` holds resources/shaders and resources/packs.json as one through
`/var/home` does. OwnFiles in Rendering lists the game's four shaders and packs.json beside the
program, and two tests read the tests' folder, which takes the game's resources as the program's
does, for each file in its place and for the list naming every shader shipped. At its open the game
checks each file is there and each shader compiles, and where one is not it writes the file's path
to the log as an error and exits with 1. A build with bits.slang taken out stops in 1.3 s with
"The game cannot open, since <its path> is missing", and one with mob.slang broken with slangc's
reason and "did not compile". The play quotes that error when the game does not open, so its first
step fails with the path, in 3 s.

Item 170. `build/engine.sh` checks the engine out at the commit `build.yml` names into `.engine`, as
the runner does, at depth 1 with its submodules, and builds its command line there whenever the
checkout moves, 66 s and 264 MB the first time and 0.02 s after. The engine's `./e3d` builds its
client only where the program is missing, so with `ENGINE_CHECKOUT` naming `.engine` no build of a
live engine tree reaches a run, and no ask of the engine's is needed. The session's log moves to
`.engine/build/sessions` with it, out of the way of another program of the same name. `build/play.sh`
and `build/pictures.sh` stop their games by process ten seconds after a stop through e3d that left
them running, and kill them five after that. E3D.md and the README say how a local check is run.
The strict build is clean and 366 tests pass. The play here fails by the engine's 253 on 6.2.32,
the mirror passing 0 of 2 cold plays through the pinned client and 4 of 4 through the live tree's
among the cold plays that reached it, the client's source the same in both and the cause unread,
and one cold play through the live tree's failing at the lantern after frame 5 made 22 pipelines in
47 s. The logs are in `~/.cache/3dengine-tools/work/i167/logs`. The runner's play is the gate, and it
passed the mirror at 03:32 on e3dea22 through the pinned checkout's client.

Item 172. terrain.slang's litStepped and the fragments of mob.slang and bits.slang called lit on the
branch drawnUnlit takes, and each returns `unlit(albedo + emitted(input))` there, as the engine's lit
does, with terrain.slang saying why. cavefields.slang is a compute shader and calls no lighting.
Each fragment fell by about 590 KB, terrain.slang's from 1,280,560 to 690,816 bytes, mob.slang's
from 1,590,576 to 1,000,524 and bits.slang's from 1,204,408 to 614,356. mob.slang keeps a second
litLight, which runs, for the light a face turned up takes a sixteenth of a block out, the floor of
Minecraft's flat shade, and it is 368 KB of the fragment, 632,184 bytes on a copy without it. It
stays, since taking it out changes a mob's light. Read with the layer off, each play from an empty
cache folder of its own under the lock exclusive, on powersave-bazzite. Frame 5 made its 22
pipelines in 43.9 s before and 20.3 s after, terrain.slang's depth pipelines in 12.0 and 12.2 s
before and 4.9 and 4.9 s after, and mob.slang's in 17.1 s before and 8.0 s after. At the pool,
terrain.slang's four pipelines with its fragmentMain compiled in 13.1 to 13.2 s each before and 5.4
to 5.7 s each after. Both plays passed whole with the mirror at red 171, green 148, blue 167, and a
play under the layer from its own empty folder passed whole, its frame 5 in 24.2 s where 49.7 to
62.3 s on the pin's plays. The 17 pictures of the plays before and after differ by more than 2
levels in at most 0.16 percent of their pixels, but dusk in 2.5 percent, where two plays of the
same shaders differ in 2.3 percent there, the mobs having wandered. The strict build is clean and
366 tests pass. The logs are in `~/.cache/3dengine-tools/work/voxel-172/logs`.

Item 175, read on 6.2.41 with the game of `6b0043b`, the scene of 174 with 176's logs and a pool
built block for block 16, 4,432, 17,808, 70,992, 284,688 and 1,139,984 blocks out, one session at
each. From 4,432 the light a glow light lays on a wall mottles, the engine's, the west wall's mean
step at Low 0.34 at the origin, 0.48, 0.89, 0.75 and 1.11 out to 284,688. From 284,688 the land's
faces part in a lattice of cracks with the sky through them, the engine placing each vertex in
world floats from the float matrix the game gives each section, and the walk along the far axis is
13 percent slower, 1.95 blocks a read where 2.24 along z, the game's float body. At 1,139,984,
where a float steps by an eighth of a block, the game picks the right block for 5 of 14 points,
most misses a block east, its walk along x jumps 2 and 4 blocks a read and stops 0.3 into the wall,
and a teleport to a half block lands on the whole one, while the engine tears the scene's faces
into strips and warps the pig. Picking, the fall, the stand and the walk along z hold at every
other place. The sheet is 23-45-item-175-far.png in the dump, and the reads are in
`~/.cache/3dengine-tools/work/i175`.

Item 173, read on 6.2.41 with the game of `1f1e955` from two copies of the review's copy of the
data, under the lock. The stripes lie on the east wall's face, the plane x = 4455, at y 26.9 to 27.9
and z 596.0 to 598.3, over the red pillar at 4454, 24 to 25, 596. The one glow light in reach is the
sea lantern at 4452, 24, 595, in the north wall at the floor, its open face on z = 596 in a pocket
of red concrete open upward. With it made white concrete the wall reads 0, and with the 14 magma
blocks outside made stone the stripes read as before, so the lantern alone draws them. `field.march`
from a tenth of a cell off the wall meets the wall at once at striped and smooth texels alike, and
from a cell and a half off the striped ones pass over the pocket's rim and the smooth one meets it.
The ask is the engine's ASKS.md entry of 2026-11-11 on a sea lantern's light, the sheet in the dump
under `item-173-room`, and the readings in `~/.cache/3dengine-tools/work/i173`.

voxel.spawning, asked for on 2026-11-11 after 172's sheet showed two plays' dusk apart by
their mobs. `voxel.spawning off` takes every mob away before the next frame, the animals kept by the
columns they stand in. A column first generated places none, one loading again brings none back,
and the night spawns none, though `voxel.spawn` still places one. `voxel.spawning on` brings back
those the loaded columns keep. build/play.sh turns it off before its first check. Two plays from
one cache folder under the layer passed whole in 317 and 218 s, the mirror at red 171, green 148,
blue 167, and of their 17 captures dusk differs in 0.00 percent of its pixels by more than 2 levels,
where 2.3 percent before. sky2 differs in 0.18 percent, all within the night's clouds, which drift.
The strict build is clean and 367 tests pass, MobTests reading a sheep kept out and a column's cow
placed none, both back where they stood.

Item 174, read on 6.2.41 at `1f1e955` in i174's copy of the data under the lock, two sessions at
each place, the scene's corner at 16, 120, 16 and 4432, 120, 592, 836 blocks the same in the save.
The two sessions at one place read alike to a hundredth of a level in most rows, so nearly every row
differs between the places by more than that. Those that differ by a real size are three stages.
The screen's probes: the west wall beside the lantern set in it reads a mean step of 0.34 to 0.37
near the origin and 0.45 to 0.53 at 4432, mottled in squares at Medium and High, and 0.34 to 0.36
against 0.36 to 0.37 with them off. Ambient occlusion at 1: the north wall 7 to 7.5 levels darker at
4432 and every row's mean step 0.08 to 0.3 higher. The sun's shadow: the floor under the roof's edge
has 23 steps over 3 levels at 4432 where 26 to 27 near the origin, both 8 with the shadows off. The
filter and the frame before's light left out keep the difference as it was. Three asks under the
engine's 258 stand in its ASKS.md, the sheet is 23-30-item-174-places.png in the dump, and the
rows, captures and table are in `~/.cache/3dengine-tools/work/i174`, kept as the check of each mend.

From item 175's read, the game's ChunkStreamer squared a column's distance from the player's in an
int, so after a teleport of more than 46,340 columns, some 741,000 blocks, the square wrapped
negative and the columns left behind stayed loaded, 1,330 held where 665 after voxel.tp from 284,688
to 1,139,984. It squares in longs. A test in HeadlessGameTests teleports the player 1,000,000 blocks
and finds no column left behind, where before the fix the column it left was still in, of 138. The
strict build is clean, 368 tests pass and the play passes whole under the layer in 310 s, the mirror
at red 171, green 148, blue 167.

Item 173's check on the engine's mend 6.2.41-local.257, the game of `6b0043b` built on each package
in the work folder from a fresh copy of the review's copy of the data, under the lock. Of eight rows
across the east wall at y 26.625 to 27.875, 6.2.41 has 0 to 29 steps over 3 levels a row at 1280 by
720 and 2 to 16 at 2560 by 1440, and the mend 0 in every row at Low, Medium and High at both sizes,
the mean step 0.18 to 0.36 where 0.29 to 1.31. What reads worse is a line across the ceiling at
2560 by 1440 alone, rows 173 to 175, 3.4 levels over its neighbors at every quality, which 6.2.41
does not draw. Outside the striped corner the frame is smoother on the mend everywhere. On the mend,
174's scene reads the screen's probes alike at the two places, the west wall's mean step 0.34 to
0.37 near the origin and 0.36 to 0.40 at 4432, where 0.45 to 0.53 on 6.2.41, the ambient occlusion
7.6 levels darker at 4432 as before and the shadow's steps as before within one. The sheets are
23-50-item-173-mend.png and 23-55-item-174-mend.png in the dump.

Verdict 13, read and mended. It is the frames, not the origin. The game opened again ran its mobs
from the first frame, so the villager walked for as many frames as the streamer took to stand the
world, and the test read its place against where it was set with half a block to spare. Run here
with the mobs left to move and 120 frames stepped before the read, the case at 1,139,992.5, 596.5
fails as macOS did, the villager at 1,139,990.31, 597.31 where it was saved at 1,139,989.5, 596.5,
its home and the saved record equal to the bit. HeadlessGame takes a switch holding the mobs still
from the first frame, and the reopened game uses it, steps 120 frames, and compares the villager's
place and home with the saved record exactly, which both cases pass.

Item 178, read on fresh copies of the review's backup with the game of `0aaf8e2`, under the lock. At
8.5, 90, -400.5 in overworld-1, the mobs held still, 173 mobs load and 148 hold an x that is no
number, kept by the columns 0, -29 to 0, -22. They are drawn, 148 in 42 draws, and show nowhere, the
field reads 536 still and 0 stamped, and the frame with them reads as with them taken out to a level
at Low, as it does at High with 40 more added. They do not stay loaded for good. With the origin at
0, no number turns into the column x 0 they are kept by, and a jump of 3,000 blocks unloaded every
one. Where they come from is a push. Crowd pushes each pair of overlapping mobs apart, and a box
whose place is no number overlaps every box of its height, each comparison with it being false, so
each takes a push along x alone by an overlap that is no number. With 25 mobs and 40 such loaded and
let move, 10 frames gave 13 of the 25 an x that is no number and kept their z. That is overworld-1's
165 with x alone. The sheep with both, and overworld-5's 52 with both, 13 villagers among them
fallen to -95,054, are the first ones, from the fault `e433e5c` mended on 2026-10-25, a mob straight
under its next cell, as a villager climbing out of a village's well. Since .NET 9 a float that is no
number turns into the int 0, so the save filed each by the column x 0 with its z kept. The tree
makes none that the reading finds. Every normalization feeding a velocity is guarded, and HEAD's
game made none in 12,000 frames from 17.5 to 0.5 at the nearest village of a freshly generated
overworld-5, 10 villagers and 102 animals, nor in 8,000 among 96 animals where the save's village
stood. The mend is on both sides. Body.Move takes no step that is no number and lets the velocity
go, and Crowd pushes nothing where either place is no number, so one such mob hands it to none. The
reader sets a mob whose place is no number down at its home where it has one that is a number, in
its home's column, and leaves the rest out, the log saying how many of each. Set down where its
column's generation set it would be kinder, but the generation does not give an animal's place here,
its x being lost and the column it was kept by being x 0 and not its own. A villager's home does
give a place, so a villager stands there, and Mobs.Joined brings back a column's kept mobs when it
is placed for the first time too, since a home's column may be one never loaded. voxel.restock fills
the loaded columns left holding none. On the backup's copies, overworld-1 leaves out 166 and keeps
1,826, and overworld-5 sets 54 villagers down at their homes and leaves out 59 animals. The tests
are a move that is no number left untaken, a mob whose place is no number among cows handing none of
them its x in 10 frames, the reader's three cases beside a pig, and a column placed first bringing
back its kept villager beside its own sheep. The strict build is clean, 394 tests pass, and the play
passes whole in 208 s with the mirror at red 171. Of its 17 captures 1 moved by over 0.01 percent
against `0aaf8e2`'s, the meadow by 0.03 percent. The meadow takes one of two forms from play to
play, four of the last six plays alike and the other two within 0.01 of each other and 0.02 to 0.03
from the four, which the change of this commit, to mobs the play keeps out, does not reach.

The first view read like for like. Item 173's room read of the origin, run again unchanged on the
game of `35a0829` from a fresh copy of the review's copy of the data, whose world.json and options
are the backup's, reads as the origin's did, every picture's mean the same and 0.01 percent of the
first one's pixels and none of the six others' apart by more than 2 levels. The 3 levels were the
two pictures' framing. The origin read's first picture holds the HUD at 1280 by 720, where its line
of help and its hotbar take four times the share of the frame they take at 2560 by 1440, and the
path read's first view at 2560 by 1440 holds the HUD and the held block, which hides the floor's
lower right. Against the room read's Low at 2560 by 1440 with the HUD hidden, the pixels apart by
more than 8 levels are the held block, the hotbar and the line of help alone, the room's own
reading as it does. The sheet is 04-10-first-view-like-for-like.png in the dump. The comments of
Body.Move and of the mobs' reader give their reason without this session's count, as N 4.11 asks.

The saves read whole, on fresh copies of the review's backup of four worlds, by a program on disk
that calls the game's own readers of `35a0829`, ReadInfo, ReadRegion, Kept and Decode for every
column, ReadChests, ReadFurnaces and ReadMobs, and the generator for the blocks under chests in
columns no region keeps. Every world.json's fields lie in range, its 253 block keys all known and
every inventory slot read. Every region file opens, 2, 15, 1 and 3 of them for overworld--1, 1, 2
and 5, holding 6, 187, 2 and 49 columns, every column decoded, none ending inside a section, and
59 in overworld-1 and 8 in overworld-5 of the eight sections kept while the world stood from 0 to
128, which the game lays over generated deep stone as it loads them. Of 19.6 million blocks none
holds a number the keys do not hold or a key this build does not know. Every chest entry, 32, 50,
35 and 22, stands at a chest, all with their loot not yet rolled and none holding items, and the one
furnace, in overworld-1, at a furnace. The mobs read 386, 1,826, 145 and 111, none above or below
the world and none kept by a column it does not stand in, once overworld-1's 166 and overworld-5's
59 mobs whose place was no number are left out and overworld-5's 54 villagers set down at their
homes. Those homes lie where the generator of the time built a village, and the generator of the
tree builds none there, its nearest at -432, 16, so the villagers stand in the open there. No file
is left half written. overworld-1 keeps a folder `far` of 14 files and 786 KB, the cache of the far
land that `3d7b1ed` added and `2263618` took out on 2026-10-24, which nothing reads, safe to delete.

Item 179. The comments of the game's sources, shaders, tests and scripts were swept for N 4.11, by
the words that give it away and by the verbs that tell a story, each hit read, 37 files changed and
comments alone. This night's accounts say what the code does and why, in Body.Move, the game's
Dispose, the mobs' reader, the renderer's Shift, the streamer's square, and the tests of the far
game, the crowd and the save. The older ones likewise, the gait's stride, the drop renderer's
corners, the cave fields' batch, the noises' fixed point said as the GPU and the CPU reckoning the
same bits, the options file's defaults by the versions of the file that hold them, the generation
tests' counts of what the land held before, and the notes of the play that told the fault each
check found. Likenings that explain a thing by another's state are gone, a plant's walls, a fire's
box, a cave's dark, a mob's legs, as the sections do, as a furnace is, as the block is. What stands
is the source's rules, a kind named as the instance of a property, and words about the running
game, a mob held still, a light no longer wanted, a block already set. STYLE.md's checks hold the
search, whose 234 hits are such uses. The strict build is clean and 394 tests pass.

The pin of 6.2.56, the engine's 8561816b, its one package 3DEngine named alone and the workflows'
engine checkout at that commit, which build/engine.sh follows. Restored into a package folder of its
own, 3dengine 6.2.56 comes alone with none of the area packages beside it, its nuspec naming
8561816b. What reads worse comes from ambient occlusion at 1 alone, which the options hold at 0. At
the reach's edge, 174's scene read from a zero origin through a build whose origin does not move,
the dither off, the north wall steps 0.57 to 0.59 a sample at 4,112 where 0.48 to 0.49 at 16 and
the west wall 0.41 where 0.35 to 0.38, within 0.03 at 2,000, so a player with it on sees a little
more mottle at the far edge before the origin moves. And it lights the same scene by its place,
the west wall 117 to 118 at 4,432 against 99 to 101 by the origin and the north wall 7 levels
lighter, where 6.2.41 read the far place about 7 darker, with the rows' steps alike at both. At 0
every row reads alike from 16 to 4,112, the west wall 0.34 to 0.37 at 16 and 0.36 to 0.37 at 4,112
where 6.2.41 read 0.62 to 0.73 there, the floor 1.05 to 1.15 and the north wall 0.46 to 0.49 at
every distance. So LandOrigin.Reach is four steps, 4,096, the player walking at least 3,584 blocks
between moves, and the tests that moved the origin are written on its value, the walk crossing the
reach and back with the exhaustion of 32 blocks, the move 2,000 past it, and the places of
OriginMoveTests a step and 1,000 blocks short of it. The mirror's reads wait on pipelines.wait.
Three plays from an empty cache and three from the cache each left, under the layer and through the
pinned client, passed whole in 288 to 294 s cold and 203 to 214 s warm, the water at red 171, green
149, blue 168 each time against 171, 148 and 167, each wait answering after 0 frames, the water's
pipelines made in the 30 frames before it. Against 6.2.41's last play every one of the 17 pictures
moves by the dither's grain, 21 to 62 percent of its pixels by 1 or 2 levels and none by more, and
7 move by more than 2 in over 0.01 percent of their pixels. The river by 1.90, the beach by 1.66,
the dark forest by 0.85 and the badlands by 0.08 are the four the old reach moved the origin for,
which against the play before any move read 0.00 to 0.07, the beach 0.18. The others are sky2 by
0.15 in the night's clouds and the meadow by 0.06 and its pack's by 0.02 in specks. The six plays
of the pin read alike but for sky2's clouds, at most 0.19 percent. The built room on the pin, the
dither off, reads no step over 3 levels in any of the east wall's eight rows at Low, Medium or High
at 1280 by 720 or 2560 by 1440, the mean step 0.14 to 0.30, and at 2560 by 1440 no row of the
ceiling from 150 to 200 stands more than 0.04 levels from its neighbors, where 257 drew a line at
173 to 175 3.4 over them. 174's scene reads alike at its two places with every stage on, the west
wall's mean step 0.34 to 0.37 at both and the shaded floor's steps over 3 26 to 27 at both, the
screen's probes and the shadows alike. The strict build is clean and 394 tests pass. The asks the
game wrote in the engine's ASKS.md name no one, the change left in the engine's tree for its
next commit.

Item 177's read, what the places far from the origin touch, before any code. The body keeps its
place and its velocity in float Vector3s (Entities/Body.cs), and the player, every mob, the dropped
items, the falling blocks, the primed TNT, the arrows and the bits of broken blocks each carry one.
Its collisions, the step up and the support under it reckon from that place against the blocks'
whole coordinates, and 61 floors in 29 files turn a float place into a block's. VoxelRay casts from
the float eye. The world file keeps the player's place as JSON numbers, which read and write
doubles as they are, and the mobs file keeps each mob's place and home as binary floats, which
doubles or offsets would need a new version of with the old kept readable. Five commands take a
place in floats, voxel.tp among them, and the answers print two places after the point. The engine
takes floats at 15 DrawMesh calls, the camera, the held lights, the scene field's center and the
sections' matrices. So doubles or a column and an offset for the body, the mobs and the ray run
through every moving thing's place and the mobs file. A third way is one origin for the whole game,
a whole column the floats are kept against and moved to as the player goes far from it, each place
and the camera made again against the new one. It keeps the floats and their arithmetic where they
are, puts the conversion at the floors that look up blocks and at the saves, and hands the engine
every place near zero, which would mend the land's cracks and torn faces of 175 on the game's side
too. Which way is the review's to choose before the code.

Item 177's first step, the origin fixed at load. LandOrigin holds a whole place in the land, a
multiple of 1,024 blocks set as the world opens near the saved place, 0 for a new world and in the
tests, and every float place of the game is kept against it, the bodies, the mobs and their homes,
the drops, the falling blocks, the TNT, the arrows, the bits, the particles, the camera, the
sections' and the lamps' matrices, the rain's walls and the outline and the cracks. Blocks, columns
and sections keep the land's own whole coordinates, and the floors that turn a place into a block,
the ray's and the sounds' cells and the columns of the streamer, the mobs and the drops take the
origin through BlockOf, BlockX, BlockZ and ColumnOf. A mob's record keeps the land's place and home
in doubles, mobs.bin writes them in its version 2 and reads version 1's floats, and world.json keeps
the player's place in doubles, which read an old file's numbers as they are. voxel.tp, voxel.drop,
voxel.bits, voxel.ring, voxel.pixel and voxel.line take the land's places in doubles, and
voxel.state, voxel.mobs, voxel.respawn, voxel.sounds and the debug overlay print them. The shaders'
world places stand, each a fraction of a block or a height, terrain.slang's texture places through
frac, the glass's joins as differences from the block's cell, the rim, the stepped shadow's
eighths, the haze's height and the mirror's plane, and mob.slang and bits.slang take the places the
engine hands them. The one place taken whole is the clouds' cell, which Clouds.cs lays over the land
with the origin added, so a cloud stands over the same land wherever the origin is. Six tests in
LandOriginTests stand a million blocks out with the origin near, the walk going 4.32 blocks along
each axis, the stop 0.3 short of a wall to four places, picking five points a 50th from an edge and
a mob's record whole, and with the origin left at 0 five of them fail, the walk going 7.5 blocks,
the stop at 0.25 and picking the block east. Two in SaveTests read a far mob back whole and a mobs
file of version 1. The strict build is clean and 376 tests pass. HEAD's game and this one played
once each under the layer, both whole, the mirror at red 171, and of the 17 captures none moved, 16
reading 0.00 percent of pixels over 2 levels apart and sky2 0.17 percent in the night's clouds,
which drift, 0.18 percent between two plays of one game. The room from the review's copy of the
data, its origin at 4096, 0, 1024, reads on 6.2.41 no step over 3 levels in any row at Low, Medium
and High at 1280 by 720 and at 2560 by 1440 but one at Medium at 2560, where 6.2.41 reads up to 29,
the mean step 0.14 to 0.31 at 1280 where the engine's mend reads 0.18 to 0.32, and 0.14 to 0.47 at
2560 where the mend reads 0.19 to 0.36, a faint mottle left at High there and no line across the
ceiling. A teleport keeps the origin of the load until the second step moves it. The sheet is
01-30-item-177-room.png in the dump of 2026-11-12.

Item 177's second step, the origin's move, with a fault of the saves mended first. The game was
disposed twice as it closed, by the program's own `using` and again by the app's shutdown as a
resource it holds, and when the engine let the ECS's mobs go before the second, that save wrote the
loaded columns as holding none, so the animals about the player were lost on quitting. A log of a
failing run read 16 mobs in 22 columns at voxel.save and at the close, and 0 mobs in 15 columns at
the shutdown's dispose. The game is disposed once, and HeadlessGameTests closes it, clears the mobs
as the shutdown does and closes it again, and finds the pig the first close saved, which fails
without the mend. The review's copy of the saves holds the mark in overworld-1, none of 665 placed
columns within 14 of the player holding a mob where 1,402 of 15,497 beyond hold 1,992, and none in
the other three worlds. No cheap way tells a world closing from mobs that died, since the dead are
taken from the ECS by MobMind's behavior, so the flag stands alone.

The origin moves by a multiple of 1,024 to the one nearest the player once the player is 1,024 from
it along x or z, every float place moving by the same whole step, the bodies, the arrows, the
blasts, the mobs' homes, the bits, the rings on the water and the land drawn, at a frame's start and
first in voxel.tp and voxel.respawn, so a teleport lands exactly. The reach was 2,048 until the read
showed the engine's mottle back 2,000 blocks from its zero, the scene built at 16, 272, 528, 1,040
and 2,000 reading a west wall's mean step of 0.37, 0.38, 0.41, 0.40 and 0.51 at High. A move
uploads every section's mesh again, since the engine's scene field knows a mesh by its vertices and
took one at a new place for a figure that walks, stamped as a box for 1,024 frames, the land at Low
reading 145.8 where 153.9 for 1,120 frames. With the uploads it reads 153.9 throughout, the frame of
the move and the next taking 342 and 579 ms at Low and 603 and 601 at High where a frame reads 163
to 183 through e3d, and the player still before the scene sees one frame brighter, 158.6 at Low and
159.5 at High against 153.9 and 152.2, back in 2 frames at Low and in 16 at High. A flight across a
move changes by 1.58 and 1.77 levels at its frame where 0.82 and 0.30 crossing none, and by a mean of
0.93 and 1.22 over the 50 after where 0.82 and 0.30. The same view after a move and back reads 2.21
percent of pixels over 2 levels apart, 0.21 percent over 8 in the scene, standing 990 blocks from the
engine's zero, where a trip that moves nothing reads 0.00. The ask is the engine's ASKS.md entry of
2026-11-12 on a world moved by a whole offset, and the README says the move and its hitch.

The second reader's list. HeadlessGame takes a place of the land to stand at and a folder to save
in, and FarGameTests plays the game at 1,139,992.5, 596.5 and at -284,680.5, -33.5 through the
commands, voxel.tp and voxel.state's place, the block ahead broken, a villager set down, saved and
opened again with the player, the block and the villager's home where they were, a death far out
coming back at the spawn exactly, and a move uploading every section's mesh again. SaveTests reads
a world.json with a far place back whole, FireFrameTests and TntFrameTests take the player's block
through the origin, the shadow's march says why it reckons in the land's coordinates, and a stack's
turn and bob take their phase from its place in the land. voxel.spawning holds for the session, a
world opened taking the switch, tested both ways, since the play's overworld had its animals back.
The strict build is clean and 385 tests pass. HeadlessGame's own steps after a teleport stood the
world before the streamer saw the new place, and it steps once before it looks. HEAD's game and this
one played once each under the layer, both whole, the mirror at red 171, and 6 of the 17 captures
moved by over 0.01 percent of their pixels, savanna 0.21 percent, its animals kept out, and the
badlands 0.07, the dark forest 0.76, the beach 1.66 and the river 1.68, where the play's search
stands the dark forest at 1,016, -1,120 and the origin moves to 1,024, -1,024 for it and the three
after it, in the far hazy foliage. The reach is to be read again on the engine's next published
package.

voxel.restock, asked for on 2026-11-12 to give back the animals the closing save lost. For each
loaded column that holds no mob and keeps none, its generation is made again on as many workers as
the processor has, and each animal or villager it placed is placed again where the pathfinder says
it can stand in the world as it stands, the rest left out and counted, villagers apart. A test in
HeadlessGameTests empties the loaded columns as the fault did, leaves a pig in one and stone at one
spawn's feet, and finds every other spawn back at its place, the pig's column left alone, the
stone's spawn left out, and a second restock placing none. On a copy of the review's copy of the
data, overworld-1 opened at the room with 0 mobs loaded and 1,992 kept beyond, it placed 112 animals,
25 pigs, 23 cows, 31 sheep and 33 chickens, in 54 of the 665 loaded columns in 1.1 s, leaving none
out, the land there having no village. The strict build is clean and 386 tests pass, and the play
passes whole, the mirror at red 171, its captures as 85160b6's but sky2 by 0.06 percent in the
night's clouds.

The second reader's list on 85160b6. The events a frame carries out in the next, a TNT's blast, a
mob's blast, shot and strike, an arrow's strike and a mob's death, carry the land's place, made as
sent and turned back as carried out, so a move between them leaves none 1,024 off. voxel.tp and a
respawn unload what the jump leaves out of reach before the origin moves, the changed columns kept
by the save and their mobs by their records in doubles. The stacks on the ground lying in a column
that unloads, by a jump or a walk, are kept by that column with their item, count, wear, age, delay
and velocity and their place in the land's doubles, and set down where they lay as it loads again,
not saved, as before. So the things a player spills in a death far from the spawn lie there after
the respawn, which the first form of this commit let go at the press of Respawn. Leave lets go of
four kinds in the columns gone, none a player's. The arrows are the skeletons', the game has no bow
and no one picks one up, and each goes within a minute. A falling block in land not loaded falls
through it and goes within a minute, as one does today whose column unloads under it. A lit TNT's
block left the land as it was lit, and its blast in land not loaded breaks nothing. A flash shows
half a second. The game has no boat, minecart, item frame or tamed animal, the animals and villagers
that follow are mobs with records, and fires, chests and furnaces are kept by their blocks and not
touched. voxel.tp refuses a place past 2^30 along x or z, and the origin goes no further than that
edge, where past 2^31 it overflowed and moved by nothing each frame. A move uploads the lamps' cubes
again. In a flat world's room at midnight with 400 glowstones in view, HEAD's game read 103 meshes
still and 256 stamped from the move for 256 to 512 frames at Low and 512 to 768 at High, so 144
lamps were in neither. Its frame read within 0.3 levels of before over the whole, the far middle and
the near floor from the second frame to the 1,152nd, and the move's own frame 1.5 and 2.0 darker
over the whole. The lamps light that room as glow lights, the whole reading red 87.6 with every part
and 39.1 with `gi.toggle glow off`, the same as with the illumination off, so the field's boxes
carried no lamp's light to lose. With the cubes uploaded the field reads 503 still and 0 stamped
from the frame after the move, the frames as HEAD's. OriginMoveTests moves the origin under a lamp,
a pool raining rings, a villager kept to a home, an arrow, a blast, a block's bits, a stack and the
flat world's animals, and finds each where it stood in the land, the lamps and every section's part
among them with their meshes uploaded again, which takes the place of FarGameTests' check of the
mesh ids alone. A TNT's blast sent before a move goes off where it was sent from. A teleport of
1,139,000 blocks keeps a pig where it stood to the last bit and a stack where it lay, and a teleport
back brings both back there. A player 3,000 blocks out dies holding 11 stacks, which lie where it
fell, respawns at the spawn with the origin back at 0, and put back finds every stack where it lay
within a hundredth of a block and no younger, and walked onto them holds all 11 again. A place past
the edge is refused and moves the origin once to the edge. The walk across the line goes back west
over it with no second move, and the 32 blocks tire the player by 0.32 with the saturation
unchanged. The far jump's test found the streamer keeping its last plan across the jump, so a jump
back to that column loaded nothing, and leaving clears it. The blast's flash and cloud and the mobs'
steps playing move with the origin, the Respawn button respawns as the next frame begins, Recenter
uploads the land's meshes before any place moves and moves the origin last, and the Shift methods
and `_disposed` stand with their kind. The strict build is clean, 390 tests pass, and the play
passes whole in 209 s with the mirror at red 171. Of its 17 captures 1 moved by over 0.01 percent
against 3bfa75d's, sky2 by 0.07 percent in the night's clouds. The first form's play moved the
meadow by 0.02 percent in a few specks of its sky, which this one reads alike. The room's sheet is
02-10-item-177-lamps.png in the dump.

The pin of 6.2.41, with the engine's item 253. The package, the workflows' engine checkout at
`894542da` and `build/engine.sh` following it. The strict build is clean and 366 tests pass. The
water's mirror was read with each play's caches in a folder of its own (N 7.11), three plays from an
empty folder, each followed by one from the cache it left, under the validation layer and through
the pinned client. All six passed whole in 393, 417 and 434 s cold and 237, 242 and 221 s warm, the
water reading red 171, green 148, blue 167 each time. Cold, the pipelines' file grew at each write
in play, from 8.8 MB after frame 5 to 21.8 MB, where 6.2.32 wrote 0 bytes, and the pool's frame made
three of terrain.slang's pipelines with its fragmentMain, stages 7e3b6259, compiled in 14.3 to 19.0
s each and all three within the 15.0, 17.3 and 19.1 s since the log's line before. Warm, the file
read at 21265 KB, every pipeline made in play said "from the cache" with the hash the cold play
gave, and terrain.slang's three at the pool read "from the cache" in 1.17 to 1.37 s each. No log
says "the driver saying not where from". The cold start's fifth frame made 22 pipelines in 49.7,
61.3 and 62.3 s of the driver's time, 54.4, 66.2 and 67.6 s passing from the probes made to it, so
one after another, beside 6.2.32's 47. Warm it took 4.4, 4.4 and 4.6 s. The logs are in
`~/.cache/3dengine-tools/work/voxel-6241/logs`, and the same reading stands under the engine's 253
in its ASKS.md. The runner's play is the gate.

Item 171, read on 6.2.41 with the layer off by ENGINE_VULKAN_VALIDATION=0, the log saying
`Validation layers: DISABLED`, one play from an empty cache folder and one from the cache it left,
each under the lock for its own run. Both passed whole with the mirror at red 171. Warm, the layer is
the time. No frame after the first made pipelines for 100 ms, the engine's least for its line, where
with it on frame 5 took 4.4 to 4.6 s and terrain.slang's three at the pool 1.17 to 1.37 s each.
Cold, it is not. Frame 5 made its 22 pipelines in 49.0 s of the driver's time, 48.9 s passing from
the probes made to it, beside 49.7 to 62.3 s and 54.4 to 67.6 s with the layer, terrain.slang's
depth pipeline taking 14.9 and 12.1 s and mob.slang's 19.1 s. The pool's frame made four of
terrain.slang's pipelines with its fragmentMain compiled in 15.5 to 15.7 s each, beside three in
14.3 to 19.0 s with it. Each cold folder ends with the driver's own cache in `nvidia` beside the
engine's, 23 MB, and an empty folder empties that too, so a cold play here is the driver's first
compile of each pipeline. A trial whose driver cache is warm may read "compiled" in under 100 ms. The
logs are in `~/.cache/3dengine-tools/work/voxel-6241/logs/off-cold` and `off-warm`.

Item 176, read on 6.2.41 at `1f1e955` from copies of i174's data, under the lock. The blob over
the spruce 134 blocks off is one pixel at the trunk's lower right corner against snow two blocks
behind it, 255, 209, 199 beside the trunk's 55, 46, 42, the trunk's color under about ten times its
light. It holds over frames, through every part gi.toggle names, each cascade alone, every quality,
the sun's shadows off and the game's own terms changed, and goes with the illumination off or the eye
moved a quarter block. With one sample a pixel it is gone, and at the engine's default 4 it stays
one pixel at twice the size and dims to 247, 221, 211 under temporal antialiasing, so it is how a
pixel a surface covers in part is shaded. From 133 blocks off, 20 eye steps a twentieth of a block
apart held 27 such pixels and 7 blobs over 20 levels near the origin and 8 and 2 beside the room,
the same in a second session, all in the land behind, and none at four built logs on snow at either
place. The orange trunk foot of the first sheet is the dirt under it, its own color under the
grading. Two asks stand in the engine's ASKS.md, this pixel and a pixel's light read by its terms.
The sheets are 21-30-red-blob.png and 22-40-item-176-samples.png in the dump. The game's options
offer FXAA and not the engine's temporal antialiasing, which takes the blobs away at 4 samples, so
a player can do no more today than set Bloom to 0 in Post Processing, which leaves each one pixel.

Item 176 read again on the engine's package 6.2.41-local.259, unpublished, against 6.2.41, the game
of `8fda020` built on each from a fresh copy of i174's data, the dither the package brings turned
off by SetDither, under the lock. The sweep of 20 eye steps a twentieth of a block apart from 133
blocks reads on 6.2.41 27 lone bright pixels and 7 steps where bloom adds over 20 levels by the
origin, as before, and 6 and 1 beside the room, the origin there at 4096, 1024, where 8 and 2
without it. On 259 bloom adds over 20 levels at no step at either place, at most 13 levels by the
origin and 7 beside the room, where 142 and 24 on 6.2.41. The 19 lone bright pixels left by the
origin and none beside the room are the same pixels at the same levels on both packages, orange
and near white, which bloom does not spread. Nothing reads worse. Without bloom 0.07 to 0.13
percent of a frame's pixels differ by more than 2 levels and 0.002 to 0.005 percent by more than 8,
the largest lone pixels in the sky on 6.2.41 that 259 does not draw and the edges of foliage on
snow moved a few levels. From item 174's eye by the origin neither package drew a blob at the
moment read, so the sheet shows the sweep's worst two steps, 10-30-item-176-blob-mended.png, with
the changed pixels in 10-35-item-176-edges.png. The pinned check comes with the publish.

The pin of 6.2.60, the engine's baa21ff4, 3DEngine named alone and the workflows' engine checkout at
that commit. Restored into a package folder of its own, 3dengine 6.2.60 comes alone, its nuspec
naming baa21ff4. Nothing reads worse. Item 176's pinned check, the sweep of 20 eye steps a
twentieth of a block apart from 133 blocks at both built places, the game of main built on each
package, the dither off for the count and each world opened with its origin near the place read:
on 6.2.56 bloom adds over 20 levels at 7 steps by the origin and 1 beside the room, at most 142 and
24 levels, with 27 and 6 lone bright pixels, and on 6.2.60 at no step at either place, at most 13
and 7 levels, with 21 and none. Of the 21, 19 are pixels 6.2.56 draws as bright at the same place
and 2 are new, and bloom spreads none of them. Three plays from an empty cache and three from the
cache each left, under the layer and through the pinned client, passed whole in 296 to 298 s cold
and 208 to 215 s warm, the water at red 171, green 149, blue 168 each time, as on 6.2.56. Against
6.2.56's play, 13 of the 17 pictures move by more than 2 levels in over 0.01 percent of their
pixels, the meadow under its pack the most at 0.74 percent and 4,914 pixels over 10 levels, the
meadow 0.47, the river 0.34, the shadow 0.30 and the swamp 0.24, and noon, the room, the sky and
the badlands read alike. Read enlarged where each moved most, the edges of blocks lose the dark
fringe 6.2.56 drew under the pillar of the shadow's picture and about the jungle's leaves, and the
meadow's and the river's move by a few pixels at their edges with the same detail, so none reads
worse to the eye. The strict build is clean and 394 tests pass. The sheets are
20-10-item-176-pinned.png, the sweep's two worst steps before and after, and
20-15-pin-6260-edges.png in the dump.

Item 180, the deep audit, read at `f201940`. Finding 6 holds. `HeadlessGame.cs:47` builds the game
with `transient: save is null`, and in a game that is not transient `VoxelGame.cs:80` reads the pack
list, `:83` fetches the default packs where the list is missing and `:88` reads the options, each
under the folder above `WorldSave.Root` (`WorldSave.cs:79-80`), so the three tests that pass a save
folder read the machine's own. I ran the suite without XDG_DATA_HOME since 2026-11-12, whole or with
FarGameTests or Closing_The_Game_Again in its filter, between 01:00 and 03:46, at 12:35, at 13:04
and at 19:19 on 2026-11-12 in the game's checkout, and at 13:16, 14:32 and 15:14 on 2026-11-14 in
the work copies of 264.10, of the faces trial and of the pin. Those runs read the player's
resourcepacks.txt and options.txt and loaded whatever packs the list names. None of them wrote
either file. Each found the list there and so fetched nothing, and the one writer of the list,
`ChoosePacks` (`VoxelGame.cs:140`), is reached from a finished fetch (`:133`), from `StopDefaults`
(`:122`), from `voxel.packs` (`GameCommands.cs:294`) and from the packs screen (`Menus.cs:349`),
none of which a test calls. resourcepacks.txt's time, read by `stat` with the file left unopened, is
18:21:59 on 2026-11-12, and options.txt's is 17:10 on 2026-11-10. My session made no call between
13:42 and 19:03 that day, and the runs it had started in the background had ended by 13:39, so
nothing of mine wrote it.

The mend comes with this Reply. `VoxelGame` takes `settings`, true by default, and a run without it
leaves the player's settings alone as a transient one does, the pack list and the options neither
read nor written and the default packs not fetched. `HeadlessGame` passes it false. The test
`A_Game_That_Saves_Leaves_The_Players_Settings_Alone` builds a game that saves and asserts it has no
pack on and no fetch begun. On a copy of the head with the test alone added, and XDG_DATA_HOME
naming a folder of mine whose list names one pack, it fails with the collection `["a-pack.zip"]`,
and with the mend it passes. The suite runs here with XDG_DATA_HOME naming a folder of my own all
the same.

Finding 8 holds. `play.sh:65` sets the trap before the build check at `:104` and the open at `:110`,
`pid` is filled only at `:123`, and with it empty the trap stops by the name `E3D_NAME` gives, which
the engine's `Sessions.Pick` (`3DEngine.Cli/Sessions.cs:17-20`) matches against the name of every
serving session of the user's. The open passed no `--timeout`, so it gave up after 60 s
(`Launch.cs:87`), and an open that gives up names no process (`Launch.cs:107-119`). I read it with a
decoy, a game serving in a sessions folder of mine that `E3D_SESSIONS` named to play.sh, run in an
unbuilt copy of the head. The head's script failed at the build check and its trap stopped the
decoy. The mended one failed at the same place and left the decoy serving. The mend exports
`E3D_SESSIONS` as a folder inside the run's own data folder, so the open's game, the kept run's and
every stop are in it, and a stop by name before the open has answered reaches no game but the run's
own. The open waits 600 s, as each command does. The kept run's stop in the trap goes without
XDG_DATA_HOME, its session being in that folder too. `pictures.sh:69` sets its trap only once the
open's pid is checked at `:58`, so it stops no other game.

The other findings, each read at its file and line at the head, all hold.

- Finding 1. ReadRegion keeps a region's bytes as read (`WorldSave.cs:107`), the renumbering is set
  from world.json's keys alone (`:137`) and applied only as a column is decoded (`:444`), Keep
  encodes in this build's numbers (`:144`), Flush writes a changed region's other columns back as
  read (`:154-167`), and SaveWorld writes this build's keys every time (`VoxelGame.cs:618-619`).
- Finding 2. ReadRegion marks the region read before reading it (`WorldSave.cs:95`) and its catch
  logs and keeps what it parsed (`:110-114`), Flush moves a region made from `_kept` alone over the
  file (`:157-170`), and ReadMobs, ReadFurnaces, ReadChests and ReadInfo fall back to none
  (`:292-297`, `:359-362`, `:392-395`, `:211-215`), each file written again whole at the next save.
- Findings 3 and 4 are one fault. The loop ends on `WindowShouldClose` (`Program.cs:54`), Dispose
  saves at once (`VoxelGame.cs:1472-1474`), and `CloseInventory` (`:999-1003`), the one caller of
  `InventoryWindow.Close`, is on neither that path nor SwitchWorld's (`:397-401`), whose Open clears
  the inventory (`:442`).
- Finding 5. The button sets `Open = Menu.None` on the press (`Menus.cs:124`), TakeCursor captures
  on entering None (`GameFrame.cs:68-71`), which the engine's transitions run after PreUpdate
  (`App.Execution.cs:111-115` at 6.2.60), only the click that captures sets `_swallowClick`
  (`VoxelGame.cs:669-674`), and Interact runs on `_captured && !_swallowClick` (`:702`) while the
  button is down.
- Finding 7. `Part.Take` returns `Change.None` where the positions, the indices and the packed bytes
  match (`ChunkRenderer.cs:77-81`), before it copies the colors (`:82`), and the packed form holds
  no albedo (`SectionMesher.cs:141-142`). I have not reloaded a pack to see it.
- Finding 9. `VoxelWorld.Add` marks the sections of all nine columns (`VoxelWorld.cs:71-74`),
  `InReach` is false for a column not loaded (`ChunkRenderer.cs:823-824`), so Update parks the key
  (`:802`, `:808-809`), and only the reach pass lets one out (`:773-775`). A parked key whose column
  loads after is meshed and stays parked, so the next reach pass meshes it again.
- Finding 10. A failed `WalkTo` leaves `_path` null (`Mob.Hostile.cs:187`), and `:184` searches
  again whenever `_path` is null, whatever `Repath` holds, up to 2,000 cells (`Pathfinder.cs:29`,
  `:42`).
- Finding 11. `Explosion.Exposure`, 27 rays (`Explosion.cs:39-46`), is an argument of `Damage` for
  every mob (`VoxelGame.cs:911`) and for the player (`:905`), so it runs before `Damage`'s test of
  the distance (`Explosion.cs:56`).
- Finding 12. `Gather` takes `GetRef` of both stacks before comparing the item or the distance
  (`ItemDrops.cs:94-99`), for every pair, under no guard on the step, so it runs while the game is
  paused.
- Finding 13. `Add` and `Remove` both count `Openings` (`VoxelWorld.cs:70`, `:84`), which is part of
  the flood's key (`Visibility.cs:183-184`), and the flood reaches only columns within rd² + rd
  (`:196`, `:215`), inside the load ring of rd + 2 (`ChunkStreamer.cs:65`).
- Finding 14. `Columns()` yields a column's loaded mobs or its kept ones and not both
  (`Mobs.cs:330-331`), and `voxel.spawn` adds a mob while spawning is off (`GameCommands.cs:505`).
- Finding 15. Every save file is written beside itself as `.new` and moved over with no flush to
  disk (`WorldSave.cs:157-170`, `:221-222`, `:307-338`, `:370-371`, `:403-404`), and the class's
  remarks say a crash leaves the region as it was (`:43-45`), which holds for the process's crash
  and not for the machine's.
- Finding 16. With no target `Hunts` clears `Hunting` and leaves `Fuse` (`Mob.Hostile.cs:93-97`),
  and a lit fuse burns on within 7 blocks once the hunt resumes (`:128-132`).
- Finding 17. `voxel.tp` tests `x` and `z` against the edge by `Math.Abs` (`GameCommands.cs:96`),
  which NaN passes, and `y` not at all, and the engine's console parses a double with
  `NumberStyles.Float` (`ConsoleBuiltins.cs:311` at 6.2.60), which takes NaN and Infinity.
- Finding 18. Death frees the cursor once (`VoxelGame.cs:832`), the click that captures checks no
  death (`:669`), and TakeCursor captures on leaving a menu (`GameFrame.cs:71`).
- Finding 19. The dome's ring at 1.5 degrees has an alpha of 191 (`HazeDome.cs:30`, `:66`), under
  the 240 that keeps the sky level in the packed form (`SectionMesher.cs:140`), so its corners carry
  sky 0 (`terrain.slang:162`), and where the alpha stays at 0.95 or over the haze is scaled by the
  sky level (`terrain.slang:429`). I have not looked at it in a picture.
- Finding 20. `Respawn` unloads every column and makes the 25 about the spawn on the main thread
  (`VoxelGame.cs:836-846`, `ChunkStreamer.cs:46-51`). I have not timed it.
- Finding 21. `Crowd` takes every mob's body and tests every pair (`Mobs.cs:57-69`).
- Finding 22. An answer with no `by place` leaves `top` empty (`play.sh:759`), awk then prints `0.0`
  (`:760`), so the guard on an empty string (`:766`) cannot fail and the test of 15 levels passes
  (`:767`).
- Finding 23. The far pig stands 28 blocks west (`SoundFrameTests.cs:23`), and both assertions
  (`:48-49`) hold for its steps as for the near one's, hearing being cut only at
  `GameSounds.cs:137`.

The mends follow in the order given, the saves (1, 2, 14, 15), the input (3 to 5, 18), the costs (10
to 13, 20, 21) and the rest (7, 9, 16, 17, 19, 22, 23), a few to a commit, each with a test that
fails first.

The saves of item 180, findings 1, 2, 14 and 15, mended. Each test was run first on a copy of
`0c08b19` with the tests alone added, their calls to `WorldSave.Decode` given the head's renumbering
argument, and failed there.

Finding 1. A region holds the key of each block by its number before its columns in version 2,
written where the world's info numbers the blocks otherwise than this build, the info then keeping
the keys it was read with (`WorldSave.BlockKeys`, which `SaveWorld` writes), and in version 1
otherwise, which a build before this one reads. A region numbered otherwise has its columns given
this build's numbers as it is read, so what is kept and written is in this build's numbers alone,
and `Decode` and the streamer take no renumbering. The test
`A_World_Whose_Blocks_Were_Numbered_Otherwise_Keeps_Them_Through_A_Save` writes a world as a build
that numbered stone and grass the other way round would, with a stone in a region of version 1
holding the player's column and one in the region beside, plays it with a block set in the player's
column, and opens it again. On the head the last open reads grass where the stone was, and mended it
reads both stones and the block set. A build before this one reads a region of version 2 as one it
cannot read, so a world numbered otherwise is not to be opened in one once this build has saved it.
On a copy of the review's copy of the data, whose info numbers the blocks as this build does, the
mended game opened overworld-1 at its saved eye alike with the head within 3 levels, a glowstone set
at a height of -63 under the player wrote r.8.1.bin again in version 1, and the next open found it
there and drew the eye alike within 3 levels.

Finding 2. A file of the save that cannot be read is moved aside under its name and `.unread`, a
number added where one lies there, before anything can write it, and one that cannot be moved is
written by no save of the run (`SetAside`, `Replace`). A region keeps the columns read before the
fault and is written again at the next save, and a column whose bytes end early is a fault, where
the head kept it short. The tests
`A_Region_That_Cannot_Be_Read_Is_Kept_Beside_The_One_Written_Again` and
`A_File_Of_The_Save_That_Cannot_Be_Read_Is_Kept_Beside_The_One_Written_Again`, the second for
world.json, mobs.bin, furnaces.json and chests.json, fail on the head, which leaves no `.unread`
file and writes over the one it could not read.

Finding 14. `Mobs.Columns()` yields a column's loaded mobs together with those it keeps out of the
world. The test `A_Column_Saves_The_Mobs_It_Keeps_Out_Of_The_World_With_One_Placed_Among_Them` fails
on the head, whose list for the column holds only the pig placed after spawning was turned off.

Finding 15. Every file of the save goes through `Replace`, which writes it beside its name, flushes
it to the disk (`FileStream.Flush(flushToDisk: true)`) and then moves it over. No test of the suite
can see a flush to the disk, so this one is read by a shim loaded into the save tests' process with
LD_PRELOAD, which logs each fsync and rename of a path under a test's folder in order. On the head
the save tests move a `.new` file over its name 11 times, regions, world.json, mobs.bin,
furnaces.json and chests.json among them, with no fsync of any, and mended each of the 11 moves
follows the fsync of its own file. The folder is not synced, so a crash of the machine may lose a
move, which leaves the old file whole.

The input of item 180, findings 3 to 5 and 18, mended.

Findings 3 and 4. `SaveWorld` saves the inventory with the stacks the window E opens holds, in its
grids and on the mouse (`InventoryWindow.Holding`), put into it as shutting the window puts them, so
a save while the window is open, the autosave or the one as the game closes, keeps them, and the
window holds them on while it stays open. A stack the inventory has no room for is left out of the
save, as shutting the window throws it and a thrown stack is not saved. `SwitchWorld` shuts the
window first, so its stacks go back into the inventory of the world left, none comes into the next,
and no chest or furnace of the last is drawn there. The tests
`The_Stacks_Of_The_Inventory_Window_Are_Saved_While_It_Is_Open_And_As_The_Game_Closes` and
`A_World_Switched_To_Takes_No_Stack_Of_The_Inventory_Window_From_The_One_Left` fail on the head,
with no stack of three sticks in the saved inventory.

Findings 5 and 18. `Capture` takes the cursor only while no screen is open and the player lives,
which holds for the click beside the death screen's window, the menu shut over it and the death
screen's Respawn pressed with the menu open, and a capture made while the left button is down leaves
the press to the button that made it, so the world takes no click until it is let go
(`_swallowClick`). The check "the menu's button and the death screen" in play.sh holds the press on
Back to Game for 12 frames over a stone the player looks at in creative mode, and reads a dead
player's cursor by `IsCursorHidden` after a click beside the death screen's window and after the
menu opened and shut. Run against the head's build, the play fails at the check, the stone broken
and the player looking at none after the press. Its half for the death screen, which the play
reaches only past that, read with the same commands in a session of the head's build, found the
cursor hidden after the click and after the menu, and in one of the mended build free after both,
the stone standing.

The costs of item 180, findings 10 to 13, 20 and 21, mended. Each was read three times on the head
and three on the mended build, run in turn, under the GPU's lock taken exclusive on tuned's
powersave-bazzite, in one session each of the flat world, transient, at 1280 by 720 with the frame
unlimited, the validation layer off and the render distance of 12, each function timed by eval over
calls in the live game. The tests of 10, 13 and 20 were run first on a copy of `fcd5574` with the
tests alone added, and failed there.

Finding 10. A hostile mob searches for a way to the player only once its wait runs out, half a
second after a search that found one and two seconds after one that found none, and runs straight at
the player meanwhile. The test `A_Zombie_With_No_Way_To_The_Player_Looks_For_One_Each_Half_Second`
reads the wait over 15 frames of a zombie below a player on a pillar three high, which on the head
stands at 0.5 in every frame. With 8 zombies about such a player at night, the hunt over 120 calls
each stepping a sixtieth of a second reads 108.1, 105.5 and 105.9 ms a call on the head and 1.30,
1.15 and 1.17 mended, a search of its 2,000 cells costing about 13 ms.

Finding 11. The rays from a blast to a body are cast only within its reach, twice its power
(`Explosion.Reach`), for the mobs and for the player in survival. An explosion of power 3 in the air
30 blocks over the player with 180 pigs loaded reads 63.9, 65.2 and 64.0 ms on the head and 2.63,
2.59 and 2.56 mended. The damage within the reach is unchanged, which the blast tests of
HostileTests hold.

Finding 12. The stacks are gathered by cells of the joining's reach, half a block, each with its
item, and a stack is weighed against those of its item in the 27 cells about its own, in the order
the stacks were gathered, so the joins are those of weighing every pair. The gathering is skipped
while the game takes no time, as under the menu. With 1,024 sticks lying a block apart it reads
27.2, 26.9 and 27.7 ms on the head and 3.0, 3.0 and 3.7 mended. The test
`Two_Stacks_Of_One_Item_Lying_Either_Side_Of_The_Edge_Of_A_Joining_Cell_Join` holds a join across a
cell's edge, and passes on the head too.

Finding 13. The flood's key holds the columns loaded within its reach by their count and a sum of a
mix of each one's place and its `ChunkColumn.Openings`, a count of its blocks set that may open or
shut a way, which takes the place of the world's own count, so a column coming or going beyond the
reach, or a block set in one, starts no flood. The test
`A_Column_Coming_Or_Changing_Past_The_Floods_Reach_Starts_No_Flood` fails on the head, where the
column joining five out floods again. The profile gains `draw.sections.floods`, the share of the
last second's frames a flood finished in, put on the head's copy too for the read. Over a flight
east of 600 frames in the overworld at 140 high, read each second, it reads 0.008 to 0.044 in the
seconds of new land on the head and 0.009 to 0.010 mended, a whole flood taking 4.4 to 7.8 ms on
both, and the flight's median 6.98, 6.25 and 6.35 ms on the head against 6.05, 6.16 and 6.06, its
99th part 37.3, 33.5 and 33.9 against 28.1, 29.8 and 32.4. A flood so finishes 2 to 4 times a second
over new land on the head, by the frames of those seconds, and about once mended, where the finding
gives one a frame.

Finding 20. A respawn sets the player over the spawn and has the workers make its land, as a new
world's opening does, the player standing on the spawn's highest block once its columns are in. A
save while the player waits, as on closing the game, makes the spawn's column and sets the player on
it first, so it keeps where the player stands. The test
`A_Respawn_Far_From_The_Spawn_Has_Its_Land_Made_By_The_Workers` fails on the head, which has the
spawn's column made in the call, and
`A_Game_Closed_While_The_Respawned_Player_Waits_Saves_It_On_The_Spawn` holds the save. From 3,000
blocks east the call reads 206.6, 197.4 and 200.7 ms on the head and 3.8, 4.2 and 3.9 mended, the
slowest frame across it 250 ms on the head, the engine's cap on a frame's time, and 68.4, 62.3 and
55.2 mended, among the frames the land loads in.

Finding 21. The mobs' bodies are crowded by cells of two blocks along x and z, wider than any two
mobs' half widths together, and a body is weighed against those of the 9 cells about its own. With
180 pigs loaded it reads 2.18, 2.44 and 2.27 ms on the head and 0.35, 0.37 and 0.37 mended. The test
`Two_Mobs_Overlapping_Either_Side_Of_The_Edge_Of_A_Crowding_Cell_Are_Pushed_Apart` holds a push
across a cell's edge, and passes on the head too.

Of the rest of item 180, findings 16, 17, 22 and 23, mended, each test run first on a copy of
`042862b` with the tests alone added.

Finding 16. A hostile mob with no player to hunt lets its creeper's fuse go out with its hunt
(`MobHostile.Fuse` set to -1 where `Hunts` has no target). The test
`A_Creepers_Fuse_Goes_Out_As_Its_Hunt_Ends_With_No_Player_To_Hunt` lights a creeper's fuse beside a
player and thinks it once with no target, and fails on the head, whose fuse burns on.

Finding 17. `voxel.tp` and `voxel.drop` refuse a place one of whose numbers is none, NaN or Infinity
as the console parses them, and a height past the land's edge, which a float would hold as no
number. The test `A_Place_That_Is_No_Number_Neither_Moves_The_Player_Nor_Lays_A_Stack` sends NaN,
Infinity and 1e39 in each place and fails on the head, which answers that the player stands at NaN.

Finding 22. The check of a fire's top in play.sh fails a read of `voxel.line` that holds no 40
samples by place, naming the read, and the guard on the joined string, which could not fail, is
gone. Fed the answer "a point of the line is off the picture", the head's form passes with its
readings " 0.0" and the mended one fails with 0 samples.

Finding 23. The test of the mobs' steps asserts every step heard lies within `GameSounds.Hearing` of
the player's eye. With the cut-off at `GameSounds.cs:137` taken out of a copy of the head, the
head's test passes and the mended one fails on a step of the far pig at -27.98, 4, -0.33.

Findings 7 and 9 of item 180, mended. Each test was run first on a copy of `a28ab91` with the tests
and two read-only accessors of `ChunkRenderer` added, `SolidColors` and `Parked`, which change
nothing the game does.

Finding 7. A part takes a meshing's faces as unchanged only where their colors match too, so a pack
read again whose tiles keep their numbers gives the scene's light field its new colors
(`Part.Take`). The test `A_Pack_Of_The_Same_Tiles_In_Other_Colors_Gives_The_Light_Field_Its_Colors`
draws the flat world's ground with a pack painted red, paints the pack green and reads it again as
F3 with T does, and fails on the head, whose ground under the player keeps the red's colors, 34, 5,
5. Taken from no pack to the red one the test passes on the head too, since the packed bytes change
there.

Finding 9. A section of a column not loaded is dropped rather than parked, since the column's
arrival marks it again (`ChunkRenderer.Update`). The test
`No_Section_Of_A_Column_That_Never_Loaded_Is_Kept_Parked` fails on the head, where 960 of the 2,040
sections parked once the test game stands are of columns never loaded.

Finding 19 of item 180, mended, the last of the audit's. The haze dome is drawn with a material of
an alpha of 252, as water's of 253, and the terrain shader gives every corner of a draw so marked
the open sky's level, the fading rings' among them, which the packed form keeps none of under an
alpha of 240 (`HazeMark` in terrain.slang, `ChunkRenderer`'s `_haze`). No test of the suite runs a
shader, so it is read in pictures. In the flat world at noon under a roof of stone nine blocks
square three over the feet, the eye at sky level 10, looking level east, the head draws a line along
the horizon, its rows 357 to 359 at 218.8, 212.9 and 204.3 of luma across the middle where the rows
about them read 197.9 to 198.2, and the mended build reads 198.0 to 198.1 there, as both read in the
open, where the frame of either is the other's within 1 level. The pair is
`04-03-haze-dome-line-under-a-roof.png` in the dump. The material's alpha takes 1.2% from the dome's
cover, so as much of the sky behind it shows through at the horizon. The play's captures read as
before, within 5 levels of the 6.2.60 pin's but sky2's, which moves as much between that pin's own
runs.

Item 181, the second read, the shaders, the scripts under `build` with the workflows, and the save's
formats, which the audit's five readers left, written before any mend. The formats were read by a
scratch test kept out of the repository, on a copy of `0be5c8e`, cutting each file the save writes
at every length and setting each byte of a region's head and entries' heads, of a sample of its
columns' bytes and of the mobs file's to 0, 255, 127 and 128 in turn, and each field of
`world.json`, `furnaces.json` and `chests.json` to null, -1, 0, 1e300, -1e300, 2147483647, an empty
string, a word, an empty list, an object and true, each read back by the game's own readers, noting
what escaped a reader, what took past 64 MB and what came back as a block or a record the game
cannot hold. A second scratch test opened a whole test game on a saved world with each field of its
`world.json` altered and with the player's column cut. The tests ran on threads 16 to 31 outside the
GPU's lock, their build under it after the quiet check.

Read 1. `WorldSave.cs:129`. `ReadBytes(length)` makes an array of the length a column's entry names
before reading any of it. A region with one byte of an entry's length or place altered took 212 to
2,032 MB in 14 of its altered bytes, on the main thread, where the streamer reads a region
(`ChunkStreamer.cs:131`), and a length past .NET's largest array throws `OutOfMemoryException`,
which the filter at :134 lets through, out of the frame. A test can fail first, a region whose entry
names 2,000,000,000 bytes read within a bound on what the thread takes, and one naming
`int.MaxValue` read with no exception escaping.

Read 2. `WorldSave.cs:346`. The mobs file's count of kinds makes its array before any kind is read.
A count below 0 throws `OverflowException`, which the filter at :381 lets through and nothing in
`VoxelGame.Open` (`VoxelGame.cs:424`) catches, so the world cannot be opened while the file stands,
and a count past two billion took 16,256 MB, which this machine gave under overcommit. Since a
record names its kind by a byte (:356), more than 256 kinds cannot be told apart. A test can fail
first, a mobs file whose count is -1.

Read 3. `WorldSave.cs:443` and :474. A furnace or a chest whose `Slots` is null throws
`NullReferenceException` at `r.Slots.Length`, which the filters at :447 and :478 let through and
`VoxelGame.Open` (:444-445) does not catch, so the world cannot be opened while the file stands. A
test can fail first, a `furnaces.json` with `"Slots": null`.

Read 4. `Inventory.cs:65`. `Decode` splits its slot with no test for null, so a null among
`world.json`'s `Inventory` or a furnace's or a chest's `Slots` throws out of the reader or out of
`Inventory.Load` (`VoxelGame.cs:453`), as the test game's open showed for `world.json`, and the
world cannot be opened while the file stands. A test can fail first, a `world.json` whose
`Inventory` holds a null.

Read 5. `IWorldGenerator.cs:32`. `Create` lowers its kind with no test for null, so a `world.json`
whose `Kind` is null throws `NullReferenceException` out of `VoxelGame.Open` (`VoxelGame.cs:421`),
as the test game's open showed, and the world cannot be opened while the file stands. A test can
fail first, a `world.json` with `"Kind": null`.

Read 6. `VoxelGame.cs:420` and :475. The player's place in `world.json` is taken as it reads, past
the land's edge of 2^30 or past a float, where `voxel.tp` refuses both. Opened with an `X` of 1e300,
the player stands at a float's infinity and the first move hangs the frame for good, since
`Body.cs:127-129` loop from `Floor` of the box's sides, which a cast of infinity holds at
`int.MaxValue`, while `x <= int.MaxValue`, which every `int` is. The test game's open had not
returned after 890 s. A `Y` or a `Z` of 1e300 reaches the same lines. A test can fail first, a
`world.json` with an `X` of 1e300 opened within a bound of time.

Read 7. `Vitals.cs:65`. `Math.Clamp(saturation, 0, food)` throws `ArgumentException` for a food
below 0, so a `world.json` whose `Food` is -1 throws out of `VoxelGame.Open` (`VoxelGame.cs:457`),
as the test game's open showed, and the world cannot be opened while the file stands. A test can
fail first, a `world.json` with a `Food` of -1.

Read 8. `VoxelGame.cs:641-643`, with `WorldSave.cs:312`. The save writes the player's heading, pitch
and the sky's hour as floats, which the JSON writer refuses where one is infinite or NaN, so a
`world.json` whose `Heading` is 1e300, read as a float's infinity, opens and then throws
`ArgumentException` at its next save, as the test game's showed. `WriteInfo` comes last, after the
regions, the mobs, the furnaces and the chests, so `world.json` keeps its last form, and
`_sinceSave` is not set back, so the timed save at :684 runs and throws in every frame past 30 s,
and a switch of world (:410) and the quit (:1509) throw before they leave. The same world stood with
5 columns still asked for 10 s on, which I did not follow. A test can fail first, a `world.json`
with a `Heading` of 1e300, opened and saved.

Read 9. `ChunkRenderer.cs:1350`. `MathF.Sign` throws `ArithmeticException` for NaN, and a sky's hour
of a float's infinity, a `world.json` whose `Hour` is 1e300, gives the sun's way NaN, so the march
of the shadows' casters throws out of the first frame drawn, as the test game's open showed. A test
can fail first, `March` given a way of NaN, or that `world.json` opened.

Read 10. `ChunkStreamer.cs:146`. A kept column whose compressed bytes do not inflate throws on the
worker, which the catch at :110 logs and gives back, and the column is asked for again as the player
moves, from the same bytes, so it fails each time and the world keeps a hole there, and the next
save of its region writes the bytes again. Where the save makes the spawn's column on the main
thread after a death (`ChunkStreamer.cs:51`, from `VoxelGame.cs:626`), the throw leaves `SaveWorld`
and the world is not saved. In the region of version 1, 184 of the 186 faults found were such
columns. A world with such a column within two of the player's never leaves its loading screen,
which ends once the 25 columns about the player are in (`VoxelGame.cs:522`). Opened by the test game
with the first column of its region, one beside the player's, given a zlib head of 0, the world did
not stand within 10 s, 3,298 frames. A test can fail first, that open, which never stands on the
head.

Read 11. `WorldSave.cs:442`. A furnace's `Burning`, `BurnTotal` and `Cooked` are taken as they read,
below 0 or infinite where 1e300 is cast to a float, so a furnace of an infinite burn burns for good,
and the window's flame and arrow take an infinite fraction to an `int` (`InventoryWindow.cs:140` and
:142). Low, since only a file changed by hand holds such a number. A test can fail first, a
`furnaces.json` with a `Burning` of 1e300.

Every file cut at every length reads clean, each reader giving up within its filter and setting the
file aside, and so do a region's version and count, the keys of a region of version 2, which
`ReadKeys` bounds (:172), and every record the mobs file gives, its place, its health and its
variant held at :365-366. No region gave a block past this build's. A `world.json` with no `Blocks`
reads as a world from before the keys were kept, its regions in this build's numbers, as meant.

The shaders under `resources/shaders`, read against the engine's 6.2.60 model pass.

Read 12. `terrain.slang:390-392`. A face in flames or lava returns `color.rgb * flames.y`, the
tile's color made linear at :215 and never encoded, where every other path returns `toDisplay`. The
model pass takes a returned color as encoded and the window's output is not linear, so a burning
block's dark texels are crushed, its bright ones pass white, and the frame's exposure is not applied
to them (`ChunkRenderer.cs:341-342` sets the pair, `FlamesShown` and `LavaShown` at :721 and :724).
No test of the suite runs a shader, so a picture pair alone shows it, a fire and a lava pool at noon
and at night.

Read 13. `terrain.slang:412`. Water mirrors as much of the sky as its corner's sky level says,
`uv2.y / 15`, and the light otherwise. With a pack, `TerrainAtlas.Paint` (:388 and :403) gives water
an alpha of 255, so the corner carries a sky level and it reads 0 underground, and a pool there by a
torch falls to the light and mirrors the sky by it. A picture pair alone shows it, a pool in a
closed cave with a torch beside it, with a pack and without.

Read 14. `terrain.slang:395`. The rim is laid on water's side faces, whose `waterDepth` is -1, and
on a shaped piece with no parts, whose `surface.w` is 0, as a fence's post (`BlockShape.cs:375`) and
a carpet (:464), where the comment above it says it is for whole blocks' faces alone. It shows only
with Rim Light above 0, its default 0. A picture alone shows it.

Read 15. `terrain.slang:287-295`, and `mob.slang:54` alike. The Unshaded mode takes away
`shadow * NdotL` of the sun as the engine adds it, where the engine adds the light arrived times the
diffuse color times one less the fresnel and the sun's contact (`modelpass.slang:757-774` of
6.2.60), so a face toward the sun reads about 4% darker than one turned from it, where the mode
holds them alike. A picture alone shows it.

Read 16. `terrain.slang:89` and :99, with `ChunkRenderer.cs:1194-1195`. The ticks are given the
shader as a float of `GetTime() * 20`, which past about 4.9 days of the game running steps by a
whole tick, so the textures' frames stutter and part from the lamps' light, which
`TerrainAtlas.Step` (:328) steps in doubles. A test can fail first on a mirror of the shader's step
in C#, the frame of an animation at a time past 4.9 days against `TerrainAtlas.Step`'s.

The rest of the shaders reads clean, the packed vertex's bits and their tables, every index against
its table's bounds, the uniforms against the C# that sets them, the places far out, each division
and square root against 0 and NaN, the haze dome's mark, `bits.slang`, `mob.slang` but for Read 15,
and `cavefields.slang`, which agrees with the game's C# reading of the fields bit for bit on a
sample of 4,096 places.

The scripts under `build` and the workflow.

Read 17. `play.sh:1235-1237`, and `fail` at :53. The layer's checks and the log's last lines read
the log the game writes under the checkout's `build/sessions`, not the run's own folder, so another
VoxelSandbox opened from the checkout meanwhile writes it, and the play fails the layer's line or
passes a run whose own log held an error. A run of the play beside a second open of the game fails
first, on the head.

Read 18. `play.sh:1248`. The kept run's open takes `./e3d`'s 60 s, not the 600 s of every other
open, and on a timeout `kept` is empty, `fail` exits, the trap stops no game and the run's data
folder is deleted under the game still running. A test can fail first only by an open made slow, a
shim of `./e3d` answering late.

Read 19. `play.sh:122`, :142, :874 and :1230. Under `set -e` and `pipefail`, an assignment of a
pipeline whose grep finds nothing ends the script with no `::error` line, so the run reads red with
no reason given, at :122 inside the very branch that would say why the game did not open. A run with
a fed answer that holds no match fails first, as finding 22's check was read.

Read 20. `play.sh:983`. `awk '{ exit !($1 > 100) }'` on no input exits 0, so a `voxel.color` answer
the sed does not match passes the sky's check. Fed an error answer, the head passes.

Read 21. `play.sh:554` and :561. The answers of `pipelines.wait` are printed and never checked, so a
wait that runs out reads the water's pixel from pipelines not yet made. Fed a wait's timeout, the
head goes on.

Read 22. `play.sh` sets no `LC_ALL`, where `pictures.sh:34` sets it, and the game sets no culture,
so its answers write numbers as the runner's locale does, `voxel.bits` among them with `F2`
(`GameCommands.cs:541`). On a desktop of a German locale the bits' answer reads `65,02`, which the
sed at `play.sh:819` does not match, and the play fails there. A run under a German locale fails
first, on the head.

Read 23. `play.sh:817-831`. The check that a bit by the tunnel's wall takes no light through it
passes on one reading, since a single level's spread is 0. Fed four answers that throw no new bit,
the head passes.

Read 24. `play.sh:88-94`. `stood` takes any answer without `loading ` as a world that stands, an
error or "no world is loaded" among them. Fed such an answer, the head passes.

Read 25. `play.sh:1234`. `pid` is left set after the game is stopped, so the trap stops that number
again at the run's end, which another process may hold by then. Low, and no test fails first short
of a shim.

Read 26. `pictures.sh:57`. The open takes `./e3d`'s 60 s and comes before the trap that stops the
game, so a slow open leaves a game serving. A shim answering late shows it.

Read 27. `pictures.sh:85-91`. `go` returns as a success after its 600 polls run out, so the next
capture is of a world that never stood. Fed answers that never stand, the head goes on.

Read 28. `pictures.sh:300-304`. F3 is held by an `input.key` in the background and F4 is sent ten
frames on, with no read of `input.state` between, so F4 can reach the game before F3 is held. A
picture of the chord's result shows it on a slow frame.

Read 29. `pictures.sh:47-55`. A `packs.json` that does not parse fails inside the process
substitution, unseen, and the loop reads no packs and takes the pictures without them. Fed a cut
`packs.json`, the head goes on.

Read 30. `publish.sh:39`. The publish clears `bin` and keeps `obj`, so a second publish in a
checkout may build nothing and skip the warnings' check. I read it as so and did not run it.

Read 31. `fetch-slang.sh:61`. The archive is unpacked with no check of its sum against one written
in the script. A test can fail first on a fed archive of other bytes.

Read 32. `fetch-slang.sh:73`. The stamp is written before :75 checks that `slangc` runs, so a
compiler that does not run is kept as fetched. A fed archive whose binary exits 1 fails first.

Read 33. `engine.sh:28`. The engine's build writes to `/dev/null`, so a failed build reads only as
its exit, and :18-21 update the submodules only when HEAD moves, so a checkout whose submodule was
moved by hand builds against it. Low.

Read 34. `pack.yml:131` and :156. `echo "version=$(build/version.sh)"` takes `echo`'s status, so a
`version.sh` that fails gives an empty version and a release tagged `v`. A step run with a
`version.sh` that exits 1 passes on the head, where it is to fail.

`shaders.sh`, `version.sh` and `./e3d` read clean, and so do the workflows, with the engine's
checkout at `baa21ff4`, which is `v6.2.60` as the project pins it, no secret given a step that does
not need it, every cache keyed by what it holds, and every job bounded by a timeout.

Reads 2, 3 and 4 of item 181, mended. Each test was run first on a copy of `0be5c8e` with the tests
added.

Read 2. The mobs file's list of kinds is refused where it holds fewer than none or more than the 256
a record's byte tells apart, and the file is moved aside and the columns' mobs placed again, as for
a file that cannot be read (`WorldSave.ReadMobs`). The test
`A_Mobs_File_Listing_More_Kinds_Than_A_Byte_Tells_Apart_Is_Kept_Beside_And_Its_Mobs_Placed_Again`
writes a list of -1 kinds and one of `int.MaxValue` and fails on the head, by `OverflowException`
and `OutOfMemoryException`.

Read 3. A furnace or a chest listed with no slots stands empty at its place, and the rest of the
file is read (`ReadFurnaces`, `ReadChests`). The test
`A_Furnace_Or_A_Chest_Listed_With_No_Slots_Stands_Empty` fails on the head by
`NullReferenceException`.

Read 4. A slot that is none reads as an empty slot, in the inventory, a furnace and a chest alike
(`Inventory.Decode`). The test `A_Slot_That_Is_None_Reads_Empty_Beside_The_Stacks_Of_The_Rest` reads
a chest whose first slot is none and whose second holds two sticks, and fails on the head by
`NullReferenceException`.

The slots are bounded and their file kept, not moved aside, since an empty slot is the nearest value
the game holds for either, and a file moved aside would empty every furnace or every chest of the
world for one of them. A list of kinds has no nearest value, so its file is moved aside. The strict
build is clean, 418 tests pass outside the GPU's lock, and the play passes in 180 s, its captures
within 8 levels of the last play's but sky2's, which moves as much between plays.

Reads 5 to 9 of item 181, mended. Each test was run first on a copy of `a3c9897` with the tests
added.

`world.json` is read with each value the game cannot hold taken as a new world's, the rest as saved,
with a warning in the log (`WorldSave.Bounded`), as the slots were bounded at `a3c9897`, since the
file holds the inventory, the mode and the keys a region of version 1 is read by. The test
`A_World_Whose_Info_Holds_A_Value_The_Game_Cannot_Hold_Opens_With_The_Rest_And_Saves` saves a world
with its third slot chosen, gives one field of its info a value by hand, opens it, steps five frames
and closes it, and holds that it opens as the flat world with the slot chosen, the player within 16
blocks of the middle, and its info written again. Each of its five rows fails on the head, each run
in a process of its own, since one row's throw out of a frame leaves the next to fail on the frame
left open.

Read 5. A kind that is none is one this build makes none of, so the world opens as the kind asked
for (`Bounded`, and the saves' list). The row `"Kind": null` fails on the head by
`NullReferenceException`.

Read 6. A place that is no number or lies past the land's edge lands the player over the spawn as in
a new world, the hour and the day kept (`WorldInfo.Landing`, `LandPlace.InLand`). A box's sides are
floored within the land's edge, so no loop over its cells runs to `int.MaxValue`, and a move longer
than 1,024 blocks, far past the 20 a frame's fall takes, is not taken and its velocity let go, so
the steps it is cut into stay few (`Body`). The row `"X": 1e300` hangs on the head and was ended at
180 s. `A_Body_Past_The_Lands_Edge_Moves_In_Time` moves a body at x 3e9 and at infinity, and
`A_Move_Longer_Than_Any_A_Frame_Takes_Is_Not_Taken_And_Its_Velocity_Let_Go` one by 1e30, each on a
worker given ten seconds, and fail on the head, which spins on.

Read 7. Saturation is held within the food as held, not as given, so a food below 0 is 0 with none
(`Vitals.Set`). The row `"Food": -1` fails on the head by `ArgumentException`.

Read 8. A float that is no number or infinite is written by its name, which the writer refuses
otherwise (the save's JSON context), so no value of the game's ends a save, and the reader bounds
it. The timed save is set back as a save begins, so one that throws waits its 30 s. The row
`"Heading": 1e300` fails on the head at the test game's stand, 5 columns still asked for 30 s on,
which I did not follow, past which its save throws `ArgumentException`, as the open fuzz showed.
`An_Info_With_A_Heading_And_A_Place_That_Are_No_Number_Is_Written_And_Read_Back_As_A_New_Worlds`
writes an x of NaN, a heading of infinity and an hour of NaN and fails on the head at the write.

Read 9. The march of the shadows' casters meets no section along a way that is no number, and an
hour that is no number reads as 10, the hour the sky begins at (`ChunkRenderer.March`,
`Sky.FirstHour`). The row `"Hour": 1e300` fails on the head by `ArithmeticException` out of the
first frame, and `A_Ray_Along_A_Way_That_Is_No_Number_Marks_No_Section` by the same out of `March`.

The strict build is clean, 428 tests pass outside the GPU's lock, and the play passes in 177 s, its
captures within 8 levels of the last play's but sky2's, which moves as much between plays.

Read 10 of item 181, mended, with what the heading's row showed and the moves' bound, as asked. Each
test was run first on a copy of `f761d7d` with the tests added.

Read 10. A kept column whose blocks do not inflate is made by its generator, as one never kept is,
and the save leaves its bytes out, moves its region's file aside and writes the region's other
columns again at the next save (`ChunkStreamer.Make` and `Take`, `WorldSave.Unreadable`). The path
that makes the spawn's column on the main thread after a death goes the same way (`LoadNow`). The
test `A_Kept_Column_Whose_Blocks_Do_Not_Inflate_Is_Generated_Again_And_Its_Regions_Others_Kept`
saves a glowstone 70 high in the player's column and in the one east of it, the region's two, gives
the first the region lists a zlib head of 0, opens the world and holds that the cut column stands as
generated, the other keeps its glowstone, the region's file lies aside as it was cut, and a third
open keeps the other's glowstone. It fails on the head, whose world does not stand within 30 s,
10,567 frames.

The heading's row stood with 5 columns asked for on the head since the game's frame threw before it
took them in. With a heading of infinity the yaw is minus infinity and the eye's look no number, and
`VoxelRay.Cast` (`VoxelRay.cs:28`) gives `Math.Sign` the look's x, which throws, from `Aim`
(`VoxelGame.cs:975`) in `Play` (:727), so every frame's `Play` ended there, short of the streamer's
update at :799, and the columns its workers had made, each to the last step of `Make`, waited in its
queue. A scratch probe on a copy of `0be5c8e` read it in the engine's log, the streamer's update
reached in five frames once the world stood and in none after, `Play` throwing `ArithmeticException`
in each frame from there, past 1,000 times within 5 s, and the timed save throwing in `Begin` too
once 30 s of the game had passed. With the ray's way that is no number taken as meeting no block,
the same world stands with its 69 columns and the frame logs no throw. The other ways in are
`voxel.look`, whose heading or pitch the console reads as NaN or, past a float, infinity, and which
set it as given, a pitch that is no number passing `Math.Clamp`, and `voxel.flicker`, which turns
the player by its turn over its frames and steps it by its step (`Measurements.cs:45-46`). The ray
meets no block along a way that is no number (`VoxelRay.Cast`), and `voxel.look` refuses a heading
or a pitch that is none and `voxel.flicker` a turn or a step, as `voxel.tp` refuses such a place.
`A_Ray_Along_A_Way_That_Is_No_Number_Meets_No_Block` fails on the head by `ArithmeticException`, and
`A_Look_Or_A_Turn_That_Is_No_Number_Leaves_The_Player_Facing_As_Before` sends `voxel.look` `NaN 0`,
`0 NaN` and `1e39 0` and `voxel.flicker` a turn and a step of NaN, and fails on the head at its
first look, which the head answers with the player facing NaN. The game's other turn of the yaw is
the mouse's, by a finite step (`FirstPersonController.cs:129`).

No path of the game's moves a body more than 1,024 blocks in one `Move`. The six that call it move
by a velocity times the game's step, which `VoxelGame.cs:680` holds to a twentieth of a second,
given the behaviors at :712 and the bits at :753, the player (`FirstPersonController.cs:218`), the
mobs (`Mob.cs:460`), the dropped stacks (`ItemDrops.cs:158`), the bits (`BlockBits.cs:181`), the
primed TNT (`PrimedTnt.cs:49`) and the falling blocks (`FallingBlocks.cs:47`). Their velocities are
held, a fall at 78 a second for the player and the mobs (`FirstPersonController.cs:14` and :211,
`Mob.cs:134` and :455), a stack's at 40 (`ItemDrops.cs:152`), the TNT's and the falling blocks' by a
drag each tick (`PrimedTnt.cs:46-47`, `FallingBlocks.cs:45-46`), a walk or a flight at 21.6 at most
(`FirstPersonController.cs:17`), a mob's hit pushing the player by 6 and 5 (`VoxelGame.cs:907`), a
blast's by 12 at most (:937, `Explosion.Impact` at most the exposure, a share), and the crowd's push
about one a second for each mob pressing on another (`Mobs.cs:67-80`). So a move takes some 4 blocks
at most, and one past 1,024 needs a velocity past 20,480 a second, some 1,700 blasts reaching the
player in one step. The places the game puts a body at are set and not moved, `voxel.tp`
(`GameCommands.cs:102`), the open (`VoxelGame.cs:471` and :476), the landing on the spawn (:542 and
:630) and the respawn (:873), each by `FirstPersonController.Teleport` (:288-294), which sets the
place and lets the velocity go, and the origin's step, which shifts every body's place by the step,
the player's among them as an entity of the game's ECS (`VoxelGame.cs:564`, :75). The comment on
`LongestMove` said a fall took 20 blocks over the longest frame's quarter of a second, where the
game's step is a twentieth, and says some 4 blocks over a twentieth.

The strict build is clean, 431 tests pass outside the GPU's lock, and the play passes in 196 s, its
captures within 8 levels of the last play's but sky2's, which moves as much between plays.

Read 1 of item 181, mended. The test was run first on a copy of `91af9b2` with the test added.

Read 1. A region's column whose length is past what the file has left is taken as the early end it
is before an array of that length is made, and the region is moved aside and its columns made again,
as for a region that cannot be read (`WorldSave.ReadRegion`). The test
`A_Region_Whose_Column_Names_More_Bytes_Than_It_Holds_Is_Kept_Beside_With_No_Array_Made_For_Them`
writes a region of one column naming 2,000,000,000 bytes and one naming `int.MaxValue`, each with 16
bytes after, and holds that the read takes under 1 MB, keeps no column and moves the file aside. It
fails on the head, the first taking past the 1 MB and the second throwing `OutOfMemoryException`.
The strict build is clean, 433 tests pass outside the GPU's lock, and the play passes in 183 s, its
captures within 9 levels of the last play's but sky2's, which moves as much between plays.

Reads 17 to 22 and 34 of item 181, mended, with the line that refuses a negative length, as asked.
`ReadRegion` refuses a column whose length is below 0 at `WorldSave.cs:138`, before the bound of
`10f81cb` at :141, as the region's fault, the file moved aside and its columns made again. Each line
of the scripts below was run first as the head writes it, the play's against a decoy and the rest
fed the answer each misreads, under the play's `set -euo pipefail` with `ask` answering the fed line
and `fail` saying it failed.

Read 17. The play opens the game by a link of the run's own name beside it, `VoxelSandbox-play-` and
the script's process, which e3d names the game's log after (`3DEngine.Cli/Launch.cs:54` and :23 of
`baa21ff4`), so the log in the engine checkout's `build/sessions` is the run's alone, and the trap
takes the link away (`play.sh:55-57`, :84). On Windows the apphost is copied there, which finds the
game beside it as the link does. Run with a decoy, a VoxelSandbox opened 33 s into the play through
the same checkout's `./e3d` by a script of my own, with folders of its own for its sessions and
data, no validation layer and the shared lock after the quiet check, the head's play fails at its
layer check, "the game drew without the validation layer", the decoy having written over the log the
two shared, and the mended play passes with the decoy opened 28 s into it, its log
`VoxelSandbox-play-3887385.log` beside the decoy's `VoxelSandbox.log` and no link left after. Each
run leaves its log under its own name there, which the workflow takes up as its artifact.

Read 18. The kept run's open waits 600 s as every other open does, and the trap ends a game of the
run that no stop reached by the link it was opened by, a pattern no other program's command line
holds, `pkill -f` matching the link's path to its end, so it ends neither another session's game nor
the owner's (`play.sh:1266`, :83). Tried on three stand-in processes whose first words were the link
of a run of the process 999, the link of one of 9990 and the game's own name, the pattern for the
run of 999 ended the first alone.

Read 19. The four assignments that take a `grep` go on where it finds nothing, so the check after
each says why (`play.sh:136`, :156, :892 and :1248). Fed an answer holding no match, each line of
the head ends the run with no error line, and each mended one fails with its own.

Read 20. The sky's check fails where `voxel.color` gives no blue to read, as an error answers
(`play.sh:1001`). Fed "no world is loaded", the head's line passes and the mended one fails. The
bank's check at :416, which reads a line too, is given a line by its `printf` whatever `feet` finds,
and an empty field fails it, so it stands as it was.

Read 21. The two reads of the pool's water fail where `pipelines.wait` answers that pipelines are
still being made, not "no pipeline is being made" (`play.sh:568-570` and :577-579). Fed the wait's
answer that it is still waiting, the head goes on and the mended line fails.

Read 22. The play exports `LC_ALL=C`, as `pictures.sh` does (`play.sh:34`), so the numbers it writes
and the game's answers it reads take a point, the game starting in the invariant culture. A program
of .NET 10 writing 65.02 with `F2` writes `65,02` under `de_DE.UTF-8` and `65.02` under `C`.

Read 34. Both steps that work out the version take it on a line of its own, so a `version.sh` that
fails ends the step (`pack.yml:133-134` and :160-161). Run as the head writes it with a `version.sh`
that exits 1, the step passes with `version=`, and the mended step fails.

The strict build is clean, 433 tests pass outside the GPU's lock, and the play passes in 182 s, its
captures within 8 levels of the last play's but sky2's, which moves as much between plays.

Reads 12 and 13 of item 181, mended. No test of the suite runs a shader, so each is read in a
picture pair of the head's build and the mended one, the flat world with Bare Bones on, from a copy
of the review's data, since without a pack the atlas has no tile of flames or lava and water carries
no sky level.

Read 12. A face in flames or lava is shown at its light for the frame as every other face is, by the
view's exposure and encoding (`terrain.slang:390-393`), and its color is taken once rather than
twice (`FlamesShown` and `LavaShown`), since the exposure of 1.6 the game draws at
(`LightSettings.cs:54`) carries it to the pack's own. Seen from five blocks off at noon, the head
draws a pool of lava three blocks across at a mean of 255, 95, 0, a luma of 122, its dark texels
crushed toward red, and the mended build at 246, 132, 0, a luma of 147, where the pack's lava
averages 255, 128, 29, a luma of 148. At midnight they read 255, 114, 0 and 247, 126, 0. Kept at
twice its color through the encoding, the lava read 255, 180, 0, a luma of 183, past the pack's own.
A fire on stone beside it reads alike, red and yellow on the head and the pack's orange and yellow
mended. The pair is `05-01-fire-and-lava-shown-at-their-light.png` in the dump.

Read 13. Water reads its corner's sky level where the corner carries one, as its color's alpha of 1
says, the mark the haze reads at :438, and its light otherwise (`terrain.slang:415`), so with a pack
on, whose water carries a sky level of 0 under a roof, a pool there mirrors no sky. In a closed room
of stone with a pool two deep and a torch beside it, the eye at sky level 0 and the water's planar
mirror off so the sky's term shows, the head mirrors the sky across the pool's far half at a mean of
20.8, 34.1, 54.2 and the mended build reads 2.1, 0, 0 there. Without a pack, water's own color holds
an alpha below 240, which carries no sky level, and takes its light as before. The pair is
`05-02-closed-pool-mirrors-no-sky.png`.

The strict build is clean, 433 tests pass outside the GPU's lock, and the play passes in 176 s, its
captures within 9 levels of the last play's but sky2's, which moves as much between plays, the play
drawing with no pack.

Reads 11, 14, 15 and 16 of item 181, two mended and two left with their reasons, and the lava's
flecks and halo of `c180b45`'s sheet, as asked.

Read 11, mended. A furnace's burn or smelt that is no number or below none reads as none, and a burn
past its fuel's whole takes itself as the whole, so the window's flame and arrow stay within their
pictures (`WorldSave.ReadFurnaces`). The test
`A_Furnace_Burning_Or_Smelting_By_No_Number_Or_Below_None_Reads_As_None` reads a furnace burning
1e300 of -1 with a smelt of NaN, and one burning 8 of 5 with a smelt of -2, and fails on the head,
which reads the first burning at infinity.

Read 16, mended. The game works out the step each animated texture shows in doubles each frame, as
`TerrainAtlas.Step` does for the lamps' light, and gives it the shader in the animation's unused
fourth number, so the shader takes no float of the ticks (`ChunkRenderer.AnimationSteps`,
`terrain.slang:84-100`). Worked out over two seconds of frames at 60 a second for an animation of 32
steps, a float of the ticks puts 40 of 120 frames a step off the lamps' from 4.9 days of the game
running, and shows 16 of the 32 steps at 10 days and 8 at 20, where the doubles show all 32. No test
of the suite runs a shader, and short of a copy of the head's float sum in a test none fails first,
so it is read in pictures. With the animated textures held at tick 0 and at tick 40, the lava of the
sheet below is drawn by the mended build as by `c180b45`'s, at most 18 levels apart on under 0.01%
of the pixels, the step the table gives being the one the float gave at those ticks.

Read 14, left. The rim is laid on water's side faces and on a shaped piece with no parts, a fence's
post or a carpet, and shows only with Rim Light above 0, which is its default
(`ChunkRenderer.cs:253`). Water's side faces could be told by the water's material in the vertex
stage, but a piece with no parts carries no bit that tells it from a whole block's face, every bit
of the packed vertex's first four bytes being taken, so it would take a change to the packing for a
look that is off unless chosen.

Read 15, left. Unshaded takes away the sun's light by `shadow * NdotL` where the engine adds it by
the light arrived times the diffuse color, one less the fresnel and the sun's contact, so a face
toward the sun reads about 4% darker than one turned from it. Matching it means working out the
engine's sun term again in the game's shader, which a later pin of the engine may change under it,
for a look that is off unless chosen (`ChunkRenderer.cs:254`). An entry in the engine's ASKS.md for
the sun term a face took could close it, if you want one written.

The lava's flecks. With the animated textures held at one tick, noon and midnight draw the lava
alike to the level in both builds, so the flatter lava of the sheet's midnight is another step of
its animation, not the night. Over the pool's middle, the mended build at tick 0 reads a luma of 150
and a spread of 16.2 against the pack's first frame's 154 and 17.7, and at tick 40 144 and 9.5
against its frame's 147 and 10.2, where the head reads 141 and 35.2, and 125 and 21.3. The pack's
brightest lava texel is 255, 179, 56, so none of its flecks reaches a green of 200, where 21% of the
head's pixels did at tick 0, its bright texels shown past white. The flecks read as the pack draws
them. The halo follows from the lower value shown, the lava's red, 255 in nearly every texel, going
to the frame at 2 on the head and at 1.6 mended, before the pass after the scene bends it under 1.

The strict build is clean, 434 tests pass outside the GPU's lock, and the play passes in 190 s, its
captures within 9 levels of the last play's but sky2's, which moves as much between plays.

Reads 23 to 33 of item 181, mended, and Read 15's entry in the engine's ASKS.md, as asked. Each line
was run first as the head writes it, fed the answer it misreads under the script's
`set -euo pipefail`, where a line can be run apart from the game, and the rest read. The entry, the
sun's share of a face's light that a look replacing it cannot read, stands at the end of the
engine's `.github/ASKS.md`, written in the engine checkout and left for its next commit.

Read 23. The tunnel's bits are held alike only over two readings at least, so a run that reads one
fails, naming its levels (`play.sh:852`). Fed one level, the head's line passes on a spread of 0 and
the mended one fails.

Read 24. `stood` takes a world as standing where `voxel.state` begins with the player's feet, waits
while it begins with `loading` and fails on any other answer (`play.sh:103`). Fed "no world is
loaded", the head's form returns as stood and the mended one fails.

Read 25. Once the game is stopped, the play lets its process's number go, so the trap stops no
process given it since (`play.sh:1259`).

Read 26. The pictures' open waits 600 s, and the trap is set before it, ending a game whose open ran
out by the run's link, `VoxelSandbox-pictures-` and the script's process, as `play.sh` does, and
taking the link away (`pictures.sh:75-82`).

Read 27. `go` fails, naming the place, once the world has not stood and meshed within its 600 reads,
rather than go on to the next picture (`pictures.sh:106`). Fed an answer that never stands, the
head's form goes on and the mended one fails.

Read 28. F4 goes once the engine holds F3, read from `input.state`, as `play.sh` sends Control and V
(`pictures.sh:319`). It is read and not run, since the pictures' run fetches the default packs and
takes every picture.

Read 29. The list of packs is read whole before the loop, so a `packs.json` that does not parse ends
the run (`pictures.sh:49`). Fed a cut `packs.json`, the head's loop reads no pack and goes on and
the mended line fails.

Read 30. The publish clears the game's intermediate files for its system before it compiles, so a
second publish in a checkout compiles and warns as the first did (`publish.sh:34`). I read it as
before and did not run a publish.

Read 31. `fetch-slang.sh` holds each archive's sha256 as the release lists it and refuses a download
of other bytes, and a version without its sums fails until they are written (`fetch-slang.sh:61` and
:75). Run in a copy with a folder of its own, the archive of the version pinned passes, and with its
sum changed the download is refused and taken away, with no stamp left.

Read 32. The stamp is written once `slangc` runs, so a compiler that does not is fetched again
(`fetch-slang.sh:94`).

Read 33. The engine's checkout sets its submodules to the commits it names on every run, and a
client that does not build prints its build's last lines (`engine.sh:24` and :31). The play below
ran it.

The strict build is clean, 434 tests pass outside the GPU's lock, and the play passes in 178 s, the
tunnel's bits read five times, its captures within 8 levels of the last play's but sky2's, which
moves as much between plays.

The pin's rehearsal on a local package of the engine's `42483382` as 6.2.83, read on 2026-11-15. The
package was built in a work folder of my own from the commit and its Sky module, as the engine's
`build/pack.sh` builds it but for the release notes, which need the history, and the templates. It
holds the library and its areas built in Release at the version, the code fixes, the shaders' cache
made through the commit's own `./e3d` with slangc 2026.18.2 as the engine pins it, 49 shaders, and
the pack, 361 files, its core assembly carrying 6.2.83. It was built under the shared lock after the
quiet check, with a NuGet folder and caches of its own, and nothing was written into the engine's
tree or `VoxelSandbox/bin/Debug`. A compiler server the commit's `./e3d` started for its client held
the shared lock until it idled out, and builds under the lock start none since.

The game on it is `fc97f0f` with the pin's three diffs of 264.10 joined, which apply to it cleanly,
the bits' place form in `bits.slang`, the origin's step through the engine's `MoveWorld`, the one
line 6.2.83's public listing adds over 6.2.60, and a lamp closed in on every side left out, with the
package named at 6.2.83 and a NuGet folder of its own, so the local package is never taken for the
one published.

The built room with its sea lantern at 4448.36, 24, 599.75 facing 48 at High, on a fresh copy of the
review's data for each build, its frame's mean against one traced reference of 4096 samples:

| build | the game's floor of the sky's light, 0.65 | the floor at 0 |
|---|---|---|
| 6.2.60, the head | 2.206 | 2.186 |
| the local 6.2.83 | 1.198 | 1.102 |
| the game-side faces experiment on 264.10, the bar | 1.197 | 1.079 |

The reference reads a mean of 0.001442 in all three, so the scene is the same. With the game's floor
the pin's room is within the bar of 1.2. With the floor at 0 it reads 1.102, 0.023 past the bar of
1.08. By the regions of the reference's map, the room's white concrete by its faces, the engine's
buried faces leave more light than the game-side faces did on the floor, 19% over the reference
against 0%, on the walls facing -z, 26% against 4%, and -x, 14% against 7%, and less on the walls
facing +z, 2% against 9%, and on the ceiling, 7% against 10%. The sheet is
`05-04-built-room-on-the-local-6283.png` in the dump, its last column red where the frame is
brighter than the reference.

The pin will be one commit of the package's version in `VoxelSandbox.csproj`, the engine's commit in
`build.yml:84` and `pack.yml:54`, and the three diffs, `bits.slang` in 7 lines, the origin's step
through `MoveWorld` in `VoxelGame.cs`, `ChunkRenderer.cs`, `AmbientParticles.cs` and
`BlastEffects.cs` with `OriginMoveTests` in 45, and the closed-in lamps in `SectionMesher.cs` and
`ChunkRenderer.cs` with `MesherTests` in 45. The engine gives a lamp closed in on every side no
light through its buried faces, and the game leaving it out keeps it from holding a light of the
engine's that lights nothing.

On the local 6.2.83 the pin's copy builds clean in the strict build, 435 tests pass outside the
GPU's lock, and the play passes in 303 s from a cold cache under the validation layer with no
validation error, the tunnel's bits 28, 28, 28, 27, 27. Against the head's captures on 6.2.60, no
picture moves past 8 levels on more than 0.11% of its pixels, sky2's clouds the most at 116 levels,
then noon at 44 and the shadow at 35 on under 0.01%. The Reply waits in REVIEW.md for the pin's
commit, which waits for a package published.

The frame's cost on the local 6.2.83 against 6.2.60, read on 2026-11-15 from 12:26 to 13:00 under
the GPU's lock taken exclusive, a run of each build first under the shared lock to fill its caches
and not read, then three of each in turn. The head's game on 6.2.60 and the pin's copy on the local
6.2.83, each as it ships with its origin on, from a fresh copy of the review's data, offscreen at
1280 by 720, the frame unlimited, the validation layer off, on the RTX 4070 Laptop GPU with the
power profile powersave-bazzite and no desktop shell on the GPU, as the 264.9 read took it, in the
built room standing still and a flight east at 110 over seed 1's loaded land, at Low and at High,
each over 600 frames of `voxel.frames` and three samples of the engine's profile. The figures are
the three runs' means in milliseconds, with their range where it is wider than the shown places.

| scene | the frame's median, 6.2.60 | 6.2.83 | the GPU's passes, 6.2.60 | 6.2.83 | `cpu.shadows` | `cpu.window_depth` | `render.graph` |
|---|---|---|---|---|---|---|---|
| the room at Low | 5.62 (5.58 to 5.65) | 5.38 (5.36 to 5.39) | 5.58 | 5.33 | 0.55 to 0.38 | 0.40 to 0.29 | 2.29 to 1.97 |
| the room at High | 8.29 (8.23 to 8.33) | 7.86 (7.85 to 7.87) | 8.30 | 7.87 | 0.62 to 0.42 | 0.45 to 0.33 | 3.01 to 2.63 |
| the flight at Low | 8.82 (8.65 to 8.98) | 9.12 (8.98 to 9.21) | 8.54 | 8.83 | 0.62 to 0.45 | 0.49 to 0.38 | 3.37 to 3.10 |
| the flight at High | 12.34 (11.92 to 12.94) | 12.17 (12.00 to 12.26) | 11.94 | 12.25 | 0.96 to 0.48 | 0.72 to 0.41 | 6.79 to 4.62 |

The register mend shows in the CPU's passes, the shadows' CPU side a third less in every scene and
half in the high flight, the window's depth a quarter less, and the render graph's work 0.3 to 2.2
ms less. The frame is held by the GPU in every scene, so it follows the GPU's passes, and the CPU's
time saved shows as a longer wait at the frame's fence in the flights, 3.08 to 3.59 ms at Low and
3.25 to 5.01 at High. In the room the frame is 0.24 and 0.43 ms less, global illumination's GPU pass
3.19 to 2.94 ms at Low and 6.15 to 5.72 at High. In the flights the GPU's passes are about 0.3 ms
more, the scene's 4.78 to 4.95 ms at Low and 4.52 to 4.70 at High and the scene's field 1.29 to 1.41
and 1.42 to 1.51, so the low flight's median is 0.30 ms more, its ranges meeting at 8.98, and the
high flight's within its spread. Its 99th part is 26.16 ms against 24.61 at Low and 35.70 against
44.12 at High, where one run of 6.2.60's read 60.50.

Item 183, mended. The test was run first on a copy of `fc97f0f` with the test added, but for the
lines that read the HUD's line and the wait, which the head has no way to give.

A save that cannot be written, an `IOException` or an `UnauthorizedAccessException` out of any of
its files, leaves the game going (`VoxelGame.SaveWorld`, :661). A world switched to opens and the
quit goes on, what the save did not write is kept to be written by a later one, the log says each
failure, and the HUD says the first, "The world could not be saved", with the system's reason, and
"The world is saved again" once a save is written. While saves fail, the timed save waits twice as
long each time, from 30 s to at most 480 (:23, :708), and a world opened begins at 30 again (:470).
The test `A_Save_That_Cannot_Be_Written_Lets_The_Game_Close_And_Switch_And_Waits_Longer` saves a
world once, sets a folder in the place of each file's copy written before it takes the name, so no
file of the save can be written on any system, and holds that the game closes with a block changed,
that two saves tell the player once and leave the wait at 120 s, that the first save's block still
reads back, and that a switch opens the next world with the wait at 30. It fails on the head as the
game closes, by `UnauthorizedAccessException` out of the region's write. The checks of both items
are read once, under item 182's Reply.

Item 182, mended. Every console command that takes a float or a double refuses a number that is NaN
or infinite, as the console reads "NaN", "Infinity", "-Infinity" or a number past a float, with one
line, "a number given is NaN or infinite, which no command takes" (`GameCommands.NotNumbers`, :349).
The place commands' check, `voxel.look`'s and `voxel.flicker`'s go through it, and `voxel.options`
refuses such a value for a setting (:647). The test
`Every_Command_Taking_A_Number_Refuses_One_That_Is_NaN_Or_Infinite` gives each number of the 16
commands that take one NaN and both infinities, 102 lines, and holds that each is refused. On the
head 30 were refused, the place's, the look's and the flicker's, and 72 taken, `voxel.time NaN`
answering "the hour is NaN", `voxel.options render Rim_Light NaN` "NaN%", `voxel.fov Infinity`
taking 110, and the corners, the shadows, the field, the clouds, the bits, the ring, the spots, the
pixel and the line taking them as given. A scratch copy of the test on the head wrote each answer,
where the test itself stops at its first line, the place's, which the head refused in other words.

The strict build is clean over both items, 436 tests pass outside the GPU's lock, and the play
passes in 184 s, its captures within 8 levels of the last play's but sky2's, which moves as much
between plays.

The read of the other places a file error can end the game outside `SaveWorld`, as asked, written
before any mend, at `7785818`. A file error here is an `IOException` or an
`UnauthorizedAccessException`, as a full disk, a folder the account cannot write or read, or a
directory where a file is to be. They fall in three kinds by where they throw.

File error 1, in the frame's drawing, the worst. `VoxelGame.Draw` calls `Menus.Draw`
(`VoxelGame.cs:1448`) and then `KeepOptions` as a menu screen is left (:1452) before its
`EndDrawing` (:1456), so a throw from either leaves the drawing begun, and every frame after throws
"BeginDrawing was called twice", as the open fuzz's cascade showed, and the game draws no frame
again until it is closed. Four paths reach it. (a) Leaving a menu screen writes the options file
(`OptionsFile.Write`, `OptionsFile.cs:62-65`) with no catch, so a data folder that cannot be written
or a full disk ends the drawing. (b) The menu's World button lists the saves (`Menus.cs:128`,
`WorldSave.Saved`), where `Directory.GetDirectories` (`WorldSave.cs:303`) has no catch and each
world's info is caught as an `IOException` or a `JsonException` alone (:312), so a saves folder or a
`world.json` the account cannot read ends it. (c) The packs screen lists the packs' folder
(`Menus.cs:303` and :308, `PackList.Available`, `PackList.cs:28-31`), made and listed with no catch.
(d) The packs screen's Done (`Menus.cs:349`) writes the packs' list (`VoxelGame.cs:150`,
`PackList.Write`, `PackList.cs:44-45`) and opens the packs (:161, `ResourcePacks`,
`ResourcePacks.cs:40` and :46), where a pack zip that is no zip throws `InvalidDataException` and
one the account cannot read throws too. A test can fail first for (b), (c) and (d) on the test game
with a folder in a file's place or a cut zip, the menu opened and frames drawn after. For (a) the
test game keeps no options (`settings: false`), so a test needs the options file's folder given, a
seam of a line.

File error 2, at the start, where the program ends before its first frame. The options file is read
by `video.Restore` (`Program.cs:40`, `OptionsFile.Find`, `OptionsFile.cs:105`) and by the game
(`VoxelGame.cs:99`, `OptionsFile.Read`, :75), the packs' list by :90 (`PackList.Read`,
`PackList.cs:38`), the packs by :92, and the first world's folder is made by :424 (`new WorldSave`,
`WorldSave.cs:99`), each with no catch, so an options file or a pack the account cannot read, a pack
zip that is no zip, or a saves folder that cannot be made ends the program. The save's readers catch
an `IOException` and a `JsonException` or an `InvalidDataException` (`WorldSave.cs:146`, :335, :431,
:504 and :536), so a `world.json`, a region, `mobs.bin`, `furnaces.json` or `chests.json` the
account cannot read throws `UnauthorizedAccessException` past them and ends the open, at the start
or at a switch. A test can fail first for each on the test game, as item 183's did, with a folder in
a file's place or a file the test cannot read.

File error 3, in a frame's update, where the frame's `Begin` ends at the throw and the player is
told nothing. (a) F2 (`VoxelGame.cs:1034`) makes the screenshots' folder (`Screenshots.Take`,
`Screenshots.cs:43`) with no catch, so one that cannot be made loses the frame's update and says
nothing, and where the folder is there but cannot be written, the engine writes the picture after
the frame and only logs that it could not (`E3D.Window.cs:115` of `baa21ff4`), so the HUD says
"Saved screenshot as" a file that was never written. (b) G (:1038) and `voxel.options`
(`GameCommands.cs:652`) write the options file as (1a) does, the frame or the command ending at the
throw. (c) F3 with T (:1031) opens the packs as (1d) does. (d) The default packs, once fetched, are
chosen in `TakeDefaults` (:681), which clears its wait before `ChoosePacks` writes the list and
opens the packs, so a throw there leaves them not taken and not tried again in the run. (e) A world
switched to whose folder cannot be made throws in `Open` (:424) after the world left is saved, so
the player stays in the world left with the next world's ask cleared and is told nothing. (f)
`voxel.flicker`'s recording writes its files when its frames end (`Measurements.cs:54` and :61-69)
and stays set where the write throws, so every frame's `Begin` ends there after, and the game's
update stops, a command of the measuring's alone. A test can fail first for each but the
screenshot's write, which the engine does after the frame and the test game, drawing to no device,
takes none of.

Clean are the default packs' fetch, on a worker that catches its IO and access errors and says the
problem on the packs screen (`DefaultPacks.cs:94-109`), the setting aside of a save's file, which
catches both (`WorldSave.cs:221`), and the engine's own writing of a screenshot, which logs. The
mend I would make is one way across them, as item 183's is for the save, where the drawing and the
update never end at a file error, the player told once on the HUD with the file and the reason, a
setting or a list not written kept to be written at the next change, a world that cannot be opened
left unopened with the world before kept, and the start going on with defaults where a file cannot
be read.

Item 184's first kind, the drawing's, mended. Each test was run first on a copy of `7785818` with
the tests added and the two seams the tests reach the paths by, the options file given and the
worlds listed under a folder given.

A file the game keeps is read or written through one way, `VoxelGame.Guarded` (:121), which gives a
fallback where the file fails as a full disk or a folder the account cannot read or write does,
tells the log each time, and the player once on the HUD by what failed and the reason until that
file is read or written again. The drawing reaches four, and none ends it. (a) The settings, written
as a menu screen is left (`KeepOptions`, :104), the same write G and `voxel.options` make. (b) The
worlds listed by the menu's World button (`Menus.cs:128`), where a world whose info the account
cannot read is left out too, as one whose info does not parse is (`WorldSave.cs:313`). (c) The
packs' folder listed by the packs screen (`Menus.cs:303` and :308). (d) The packs chosen on it
(`VoxelGame.cs:187`), whose list is kept through the same way, and a pack that cannot be read, a zip
that is no zip or a folder the account cannot read, is left out of those drawn and said by its name
(`ResourcePacks`, :35 and :49, `VoxelGame.cs:199`), where it ended the drawing and the opening of
every pack beside it.

The test `Leaving_A_Menu_Whose_Settings_Cannot_Be_Kept_Draws_On_And_Tells_The_Player_Once` gives the
test game an options file with a folder in the place of its copy, leaves a menu screen twice, frames
drawn after each, and holds that the HUD says "The settings could not be kept" once. It fails on the
head by `UnauthorizedAccessException` out of the frame.
`A_World_Whose_Info_The_Account_Cannot_Read_Is_Left_Out_Of_The_Worlds_Listed` lists two saved
worlds, one whose `world.json` the account cannot read, a mode set on Linux and macOS alone, and
fails on the head by `UnauthorizedAccessException`.
`A_Pack_That_Cannot_Be_Read_Is_Left_Out_And_Said` chooses a pack that is no zip and fails on the
head by `InvalidDataException`. The packs' folder listed is read and not tested, since the test game
reaches it only through the player's own data folder, which a test leaves alone.

The strict build is clean, 439 tests pass outside the GPU's lock, and the play passes in 204 s, its
captures within 8 levels of the last play's but sky2's, which moves as much between plays.

Item 184's second kind, the start's, mended. Each test was run first on a copy of `c51b643` with the
tests added and the list of what the game said of its files, a line, which the start's test reads
since the HUD's line ages out while the test game stands.

The start goes on where a file cannot be read. The settings the game reads (`VoxelGame.cs:99`) and
the packs' list (:90) go through `Guarded`, the player told once and the defaults kept, and the
window's own read of the options file finds no value in one it cannot read (`OptionsFile.Find`,
`OptionsFile.cs:126`), so the window takes its defaults. An options file that cannot be read is held
as written with the defaults (:85), so it is written over only once the player changes a setting
from them, as asked. A world whose folder cannot be made is played unsaved, the player told
(`VoxelGame.cs:466`), where the program ended. The save's readers take an
`UnauthorizedAccessException` as the fault they take an `IOException` for, the file moved aside and
the world opened from what reads (`WorldSave.cs:146`, :336, :432, :505 and :537), so a save's file
the account cannot read is neither the end of the open nor of the start.

The test `A_File_Of_The_Save_The_Account_Cannot_Read_Is_Kept_Beside_And_The_World_Opens` gives each
of the save's five files a mode the account cannot read, on Linux and macOS, reads the save and
holds that the file lies aside, and fails on the head by `UnauthorizedAccessException` for each.
`An_Unreadable_Options_File_Keeps_The_Defaults_And_Is_Written_Once_A_Setting_Changes` finds no value
in such a file, reads it with the render distance kept, writes nothing with the defaults standing
and writes once the render distance changes, and fails on the head at the find.
`A_World_Whose_Folder_Cannot_Be_Made_Is_Played_Unsaved_And_Said` opens the test game at a world
under a file and fails on the head by `DirectoryNotFoundException` out of the start. The game's own
reads at the start are read and not tested, since the test game keeps no settings and reads no
packs' list.

The strict build is clean, 446 tests pass outside the GPU's lock, and the play passes in 195 s, its
captures within 9 levels of the last play's but sky2's, which moves as much between plays.

Item 184's third kind, the update's, mended, and the screenshot's write that fails after the frame
in the engine's ASKS.md. Each test was run first on a copy of `2e6c699` with the tests added and the
two seams they reach the screenshot by, the screenshot taken as F2 takes it and the folder it goes
into given.

F2 says "Saved screenshot as" only where the screenshots' folder could be made, and where it could
not, "Screenshots could not be kept" with the reason, once (`VoxelGame.Screenshot`,
`VoxelGame.cs:419`). The engine writes the picture after the frame and says a write that fails to
its log alone (`E3D.Window.cs:115` of `baa21ff4`), so the HUD cannot know of it, and the engine's
ASKS.md holds that as an entry of 2026-11-15, uncommitted in the engine's tree. A switch to a world
whose folder cannot be made keeps the player in the world played and says so, where the game begins
plays it unsaved (:477). A recording of `voxel.flicker` whose file cannot be written ends and says
so (`Measurements.cs:55`), where it stayed set and ended every frame's `Begin` after. The settings
written by G and `voxel.options`, F3 with T and the default packs taken were mended with the
drawing's kind at `c51b643`, the same writes and the same packs' opening.

The test `A_Screenshot_Whose_Folder_Cannot_Be_Made_Says_So_And_Not_That_It_Was_Saved` fails on the
head by `DirectoryNotFoundException`,
`A_Switch_To_A_World_Whose_Folder_Cannot_Be_Made_Keeps_The_World_Played_And_Says_So` fails on the
head, which opened the world asked for unsaved, and
`A_Recording_Whose_File_Cannot_Be_Written_Ends_And_Says_So` fails on the head, whose recording stays
set after its frames.

The strict build is clean, 449 tests pass outside the GPU's lock, and the play passes in 181 s, its
captures within 8 levels of the last play's but sky2's, which moves as much between plays.

Item 184's paths left read, given tests, as asked. The game takes the folder its settings file and
its packs' list are kept in, the data folder beside the saves where none is given, and reaches its
packs' list through itself (`VoxelGame`, its `settingsFolder` and `PackList`), where
`PackList.Default` and `OptionsFile.Default` fixed it to the player's data folder, so the nine uses
of the list in the game, the menus and `voxel.packs` go through the game. `Program` gives no folder,
so the player's game reads and writes its files where it did. The test game given a folder keeps its
settings there, with an empty packs' list where the test gives none so it fetches no pack, and one
given none keeps no settings and is transient where it is given no save, as before, so no test reads
or writes the real data folder. Read over the suite, no test opens the packs screen or sends
`voxel.packs` but with a folder of its own, and the tests that choose packs give their whole paths
with the settings off.

Each test was run first on a copy of `7785818`, before item 184's mends, with the same change made
to it by one script.
`A_Start_Whose_Options_File_Cannot_Be_Read_Keeps_The_Defaults_And_Leaves_The_File` fails there by
`UnauthorizedAccessException` out of the start, and passes with the file's write time unchanged
through the start, the stand and the close.
`A_Start_Whose_Packs_List_Cannot_Be_Read_Draws_No_Pack_And_Says_So` fails there likewise and passes
with no pack drawn. Both set a file's mode the account cannot read, on Linux and macOS alone.
`The_Packs_Screen_Over_A_Packs_Folder_That_Cannot_Be_Listed_Draws_On_And_Says_So` opens the packs
screen over a file in the place of the packs' folder, on every system, and fails there by
`IOException` out of the frame. The options file written over only once a setting changes stays
tested on the file itself, since the test game sets its render distance after its start. The strict
build is clean, 452 tests pass outside the GPU's lock, and the play passes in 186 s, its captures
within 11 levels of the last play's, on no pixel past 8 but sky2's, which moves as much between
plays.

`PackList.Default` had no caller left, the game's nine uses of the list going through the game's
own, and is taken out. `OptionsFile.Default` keeps its one, `Program.cs:40`, which restores the
window's resolution and mode from the options file before the window opens and before the game is
made, the same file the game given no folder reads, so it stays. The strict build is clean, 452
tests pass outside the GPU's lock, and the play passes in 193 s, its captures within 8 levels of the
last play's but sky2's, which moves as much between plays.

The third read of the lane, the play's script and the workflow apart, of what a player can do in
the menus and the console that the tests do not reach, before any mend, as asked. The tests send 26
of the game's 71 console commands, and the read went through the bodies of the other 45, the menus'
screens and the inventory's window. Each finding was read in the code and not run, but for the
search's time in the second.

1. `voxel.fill` and `voxel.room` hang the game where a corner stands at int's top.
   `GameCommands.cs:146` counts each side of the box in int before it widens the count, so a side
   from 0 to 2,147,483,647 counts minus 2,147,483,648 blocks and the box passes the fill's bound at
   :147, and the loops at :150 to :152 run while a place stays at or under its corner, which a
   corner at 2,147,483,647 never ends, the place wrapping past it. So
   `voxel.fill 0 0 0 2147483647 0 0 stone` holds the frame without end, and
   `voxel.fill 2147483647 0 0 2147483647 0 0 stone`, a box of one block counted right, as well.
   The mend I would make is the sides counted in long and a corner past `LandOrigin.Edge` on any
   axis refused with a line, as `voxel.tp` refuses a place past it (:97), with a test giving both
   boxes and failing first by a time bound.
2. `voxel.biome` holds a frame for seconds where the biome named lies past its reach.
   `GameCommands.cs:843` to :851 sample the overworld 8 blocks apart over 375 rings, 564,001 places,
   each by `Overworld.At`, the land's height with its climate, on the frame's thread. A scratch test
   of the same loop on cores 16 to 31 took 6.74 s for the whole search, so a biome none of the land
   within 3,000 blocks holds freezes the game that long, and e3d's command waits as long. The mend I
   would make keeps the rings 8 apart to 512 blocks and takes them 32 apart past it to 2,976, as
   Minecraft's search of a biome steps 32 blocks, 50,521 places, some 0.6 s by the same cost a
   place, with a test timing a search that finds nothing and failing first.
3. `voxel.save` answers "saved to" a folder where the save failed. The save catches its error
   (`VoxelGame.cs:724` to :733), so `SaveNow` (`GameCommands.cs:248` to :249) answers alike either
   way, and a player asking again while the disk stays full is told the world was saved, where the
   HUD said once that it could not be. The mend I would make is the save saying whether it wrote and
   the command answering that the world could not be saved, with the reason, a test with a folder
   that cannot be written failing first.
4. A world played unsaved, its folder not made at the start, is called transient by `voxel.state`
   (`GameCommands.cs:48`) and `voxel.save` (:247), where the game is not transient
   (`VoxelGame.Transient`) and the HUD said it is played unsaved. The mend I would make says
   unsaved where the game is not transient, held in a test of a folder not made.
5. `voxel.gi` takes a number as a quality (`GameCommands.cs:614`), `Enum.TryParse` reading `7` or
   `-1` as a value the enum names none of, where `voxel.view` (:451) and `voxel.mode` (:424) hold
   theirs to `Enum.IsDefined`. The game keeps it as its quality, the HUD's help reads "G light: 7",
   and the engine's table gives such a quality no cascades (`Illumination.cs:37` in the engine),
   what it draws then not read. G and the options' button step it back to off. The mend I would
   make is the same `Enum.IsDefined`, a test giving `7` and `-1` failing first. Beside it, the
   world's info reads its mode the same way (`VoxelGame.cs:522`), so a world.json whose mode is `7`
   plays a mode that touches but neither builds nor lives, titled as spectator, and is saved back as
   7, a reader item 181 left, mended alike.
6. `voxel.flicker` takes any count of frames (`GameCommands.cs:701`), and the recording reads the
   screen back in each of them (`Measurements.cs:34`) and keeps 16 bytes a frame, so a count of
   2,147,483,647 records for 414 days at 60 frames a second, through a switch of worlds, until
   another recording takes its place. Nothing hangs. The mend I would make is a bound of 36,000
   frames, ten minutes at 60 a second, or the count is left as given, a measuring command's choice.
7. `Gui.cs:139` and :140 hold the button's summary and returns over the text field's, so
   `TextField` carries two summaries and `Button` (:181) none, since `b25e668` added the field. I
   would mend it in passing with the first commit.

The rest reads clean. The commands that take a number hold it where a loop, an index or a size takes
it, `voxel.distance` to 2 to 16, `voxel.scale` to 0 to 4, `voxel.field` and `voxel.gpu` to 1 to 8,
`voxel.day` from 0 with its moon by a modulo, `voxel.give` from 1, `voxel.furnace` and
`voxel.vitals` to their slots' and their bars' ranges, `voxel.spawn` its variant by a modulo,
`voxel.mobs` and `voxel.fires` a count taken, none below 0, `voxel.find` to 40 rings of cells,
`voxel.ticks` from 0, `voxel.hurt` none below 0, and `voxel.color`, `voxel.gaps` and `voxel.edge` to
the window. `voxel.craft` ends where the materials do, and `voxel.view` and `voxel.mode` take only a
value named. The packs' screen and the worlds' screen index within their lists and scroll within
them, the new world's screen takes a seed of 32 characters, any text giving one (`Menus.SeedOf`),
the options' sliders and choices hold their values to their ends (`Option.Change`, `Option.Shown`),
the sky's colors index a list of a fixed size, and the inventory's window takes, splits, spreads,
gathers, sorts and drops within its slots and the stacks' sizes, the made slot and the furnace's
output only taken from (`InventoryWindow.Pointer`), a shifted craft ending where the materials or
the room do.

The order I would take is 1 and 2, the frame held, then 3 and 4, the console's words, then 5 with 7,
and 6 last, a commit for the first two and one for the rest.

Item 185's first commit, the third read's findings 1 and 2, mended. A fill or a room refuses a
corner past the land's edge on any axis, 1,073,741,824 blocks from its zero, as `voxel.tp` refuses a
place past it, and counts each side of its box in a long, so its loops end short of the int's end
and a box from edge to edge counts its 2,147,483,649 blocks and is refused by the fill's bound
(`GameCommands.Box`). `A_Fill_Or_A_Room_Reaching_The_Ints_End_Answers_At_Once_And_Changes_No_Block`
gives four boxes on a thread each, the two of the read, one from the int's low end and one from
edge to edge, and fails on a copy of `40bcc10` with the test added, its first box giving no answer
within 10 s.

The biome's search steps its rings 8 blocks apart to 512 blocks and 32 apart past them to 2,976,
50,521 places where 564,001. Timed on cores 16 to 31 for a biome it never meets, the whole search
takes 0.59 s where 6.85 s, read twice each. In the game, over the 16 biomes from the spawns of the
overworlds of seeds 1, 2, 3 and 42, the longest search takes 0.24 s where 1.39 s on the head, the
mountains 1,312 blocks out of seed 42's spawn, and of the 64 answers the 9 past 512 blocks name a
place of the biome 8 to 24 blocks farther than the head's, the rest the same place. Every biome
stands within 1,312 blocks of those four spawns, so the whole search's time is measured in the
place of a test, as the item asks.

The strict build is clean, 453 tests pass outside the GPU's lock, and the play passes in 227 s, its
captures within 11 levels of the last play's but sky2's, which moves as much between plays.

Item 185's second commit, the third read's findings 3 to 7, mended, with what the engine drew at a
quality of 7. Each test was run first on a copy of `9f96eae` with the tests added.

Finding 3. The game keeps why its last save could not be written (`VoxelGame.SaveFault`), which
holds the HUD's line to the first failure, and `voxel.save` answers that the world could not be
saved, with the reason, where it was not.
`The_Save_Command_Says_So_Where_The_Worlds_Files_Cannot_Be_Written` puts a folder in the place of
each file a save writes, as item 183's test does, and fails on the head, which answers that it
saved to the world's folder.

Finding 4. `voxel.state` and `voxel.save` call a world played unsaved, its folder not made,
unsaved, and transient only where the game is.
`A_World_Played_Unsaved_Is_Said_To_Be_Unsaved_And_Not_Transient` fails on the head, whose state
says transient.

Finding 5. `voxel.gi` refuses a quality the enum names none of, as `voxel.view` and `voxel.mode`
refuse theirs, and the world's info giving a mode the enum names none of is read as a new world's,
creative. `A_Quality_Of_The_Light_Naming_No_Value_Is_Refused` fails on the head, which answers that
global illumination is at 7, and
`A_Mode_The_Worlds_Info_Gives_By_A_Number_Naming_None_Is_Read_As_A_New_Worlds` fails on the head,
which plays the mode 7. At a quality of 7 the engine draws no frame. Read on `9f96eae`, the flat
world in a room of glowstone offscreen, `RenderPlugin.Render` threw `IndexOutOfRangeException` in
every frame from `Illumination.TexelsFor`, which indexes the empty list of cascades `TexelsAt`
gives such a quality, 100 times within 1.8 s. The window held the last frame drawn at High, which
`voxel.color` read, and `./e3d shot` gave no answer within 600 frames, until the quality went off.
That stands in the engine's ASKS.md as an entry, uncommitted there, since the flat call takes the
value.

Finding 6. `voxel.flicker` refuses a recording past 36,000 frames, ten minutes at 60 a second, or a
motion past 35,988 of them, and says the most.
`A_Recording_Past_36000_Frames_Is_Refused_With_Its_Most` gives 36,001 frames, 2,147,483,647 frames
and a motion over 2,147,483,647, and fails on the head, which records 36,001.

Finding 7. The button's summary and returns stand over `Gui.Button`, and the text field's alone
over `Gui.TextField`.

The comment over the biome's rings says why 32 blocks serve past 512, a far biome found within a
step of its nearest place, where a patch narrower than the step can lie unseen between two rings.

The strict build is clean, 458 tests pass outside the GPU's lock, and the play passes in 249 s, its
captures within 8 levels of the last play's but sky2's, which moves as much between plays.

Item 186, the comments that liken a thing to another game's, reworded. Before, 302 comment lines
named Minecraft, 83 of them by likeness, 76 on one line and 7 across a line's break, which the
read's count of 76 missed, with one more in the summary of a block's facing, its torch. After, 218
name it and none by likeness. In each the likeness clause is gone, and where it stood for the rule
the comment says the rule, the button lit with its label yellow so the one a click takes stands
out, the crosshair's middle pixel on the window's middle, the wool's odds in a thousand, a sprite
whose metadata gives no scaling drawn stretched, the furnace's sprites drawn as far as the fuel and
the smelting have gone, a block on a wall facing away from it, and the packs read again as F3 with
T asks. The player's box keeps its 0.6 by 1.8 without the likeness, as the item's line takes the
form. The 111 lines left that say "as Minecraft's model definitions give it", "as Minecraft's 32
ticks" and the like name where a rule or a number comes from and stay, and so do the command
descriptions. 44 files change, in comments alone, 167 lines in and 173 out, each block a clause
left short wrapped again to 100 columns, and the mode switcher's summary has a sentence's end where
it had a semicolon.

The strict build is clean, 458 tests pass outside the GPU's lock, and the play passes in 208 s, its
captures within 8 levels of the last play's but sky2's, which moves as much between plays.

Item 187, the resource packs read as hostile input, read and fuzzed, before any mend. A script made
27 packs, the 24 cases the item and the review's message name and three of connected textures on
the flat world's grass, which the cases on stone did not reach since the flat world shows none. The
test game of a copy of `c250bcd` opened each at its start, drew 30 frames, read the packs again as
F3 with T reads them and drew 10 more, each pack in a process of its own outside the GPU's lock on
cores 16 to 31, bounded by 180 s and a memory group of 8 GB, each step written to a report as it
ended. A pack of one valid picture opens in 2.6 s at 183 MB, the control.

The rule the findings fall under. A throw out of `VoxelGame.ReloadPacks` (`VoxelGame.cs:203`) ends
the program, since nothing between it and the loop in `Program.cs` catches. It runs at the start
(`VoxelGame.cs:91`), where the packs' list keeps the pack, so every start ends until the player
takes it out of `resourcepacks.txt` by hand, on F3 with T (:1099) and at the packs screen's Done
(`Menus.cs:349`), in the frame, and by `voxel.packs` and `voxel.reload` (`GameCommands.cs:306` and
:326), where the console catches it. A throw past the atlas, as finding 6's, leaves the reload half
done, the terrain, the icons, the interface and the sky on the new packs, the mobs on some of them,
`Packs` on the old ones and the new zips left open.

1. A file of a pack whose read throws ends the start. The constructor's catch
   (`ResourcePacks.cs:42`) covers a zip's opening, and nothing covers its entries' reads.
   - A zip entry whose data does not inflate throws `InvalidDataException` from the entry's copy in
     `ResourcePacks.Read` (`ResourcePacks.cs:110`), through `Picture` (:127) out of
     `TerrainAtlas.Build` (`TerrainAtlas.cs:121`), the start ended in 1.6 s.
   - An entry that inflates past 2 GB, a 2.6 MB zip holding 2.5 GB of zeros as `stone.png`,
     throws `IOException`, "Stream was too long", from the same copy at 4.2 GB in 5.7 s.
   - A folder pack whose `stone.png` is a link to `/dev/zero` throws `OutOfMemoryException` from
     `File.ReadAllBytes` (:107) at 4.3 GB in 5.1 s.
   - An entry whose headers say 1,000 bytes and which inflates to 1.5 GB is read to its 1,000 by
     .NET and taken as a picture that does not decode, and a zip cut in half is left out and said,
     as they should.
2. A folder pack whose `stone.png` is a named pipe holds the start without end, `File.ReadAllBytes`
   (`ResourcePacks.cs:107`) waiting for a writer, the case ended by its bound at 180 s.
3. A connected texture's tiles and weights.
   - `ConnectedTextures.cs:187`, `for (int n = from; n <= to; n++)`, where a range ending at
     2,147,483,647 wraps past its end and adds names until memory runs out, 6.2 GB in 32 s, out of
     the start. The range's check holds its length, `to - from < 256`, and not its end. Of the
     game's 193 loops with an inclusive upper bound, two take the bound from a file or a command,
     this one and `GameCommands.Box`, mended at `9f96eae`. The rest end at a constant, at a body's
     cells, which `Body.Floor` holds within the land's edge, or at a clamped value, the render
     distance's clouds' cells, and the loops that count down end at constants or at the column's
     bottom.
   - A properties file of 400,000 ranges of 256, 2.4 MB, names 102 million tiles in
     `ConnectedTextures.Tiles` before the atlas's bound of 512 tiles is read
     (`TerrainAtlas.cs:182`), out of memory at 5.7 GB in 29 s.
   - Random tiles whose weights sum past the int's top throw `OverflowException` from
     `Enumerable.Sum` in `RandomTile` (`ConnectedTextures.cs:126`), on the mesher's worker, where
     `ChunkRenderer.cs:904` writes it to the console's error stream alone and keeps the section
     unmeshed. Every section holding the rule's block goes undrawn, 0 sections drawn after 30
     frames with the rule on grass, where the same rule without weights draws 37, nothing in the
     engine's log.
   - A repeated sheet of 2,147,483,647 by 2,147,483,647 overflows its tile's index, which the
     mesher's check (`SectionMesher.cs:463`) takes as the surface's own tile, and draws.
4. A picture of a vast size within the decoder's. A 16,384 by 16,384 picture of one color, 8 KB
   zipped, as `stone.png` or as the font's sheet, takes the start to 2.2 GB and 7.5 s and the
   reload to 3.3 GB, the font's to 4.3 GB. Each is decoded whole before the atlas resizes it to its
   tile of at most 128 pixels (`TerrainAtlas.Resized`), and the font's sheet is made a texture
   whole (`PackGui.cs:190`). A picture of 25,000 by 25,000 and a header of 65,535 by 65,535 are
   refused by the decoder as too large, and a picture cut short is taken as one that does not
   decode, as they should.
5. An animation's frames list copies a frame for each of its entries (`ResourcePacks.cs:171` to
   :177). A list of 2,000,000 entries over a strip of two frames of 16 pixels, a 12 KB zip, takes
   the start to 2.4 GB in 8 s, and frames of 64 pixels would take 32 GB. Times of 0, below 0 or
   2,147,483,647, indexes past the strip and a width or a height past the picture are refused or
   bounded (`ResourcePacks.Number`, :180, and :169), and that pack draws.
6. A model of OptiFine's form has no bound on its boxes. A `.jem` of 1,000,000 boxes, a 180 KB zip,
   takes the start to 3.9 GB in 19 s and the reload out of memory in `BoxMesh.AddFace`
   (`BoxMesh.cs:97`, from `MobRenderer.PicturedBoxes`, `MobRenderer.cs:235`), past the atlas. A
   number past a float, 1e39 in a box's corner or a part's translation, reads as infinity and makes
   the pig's box NaN at both corners, which the mob's mesh takes, what it draws not read. A texture
   size of 0 leaves the boxes finite and their pictures' offsets infinite. A nesting of 300 parts
   is past the JSON reader's depth of 64 and the model is left out, as is a file that is no JSON.
7. No entry's name reaches the disk. The game reads a zip's entries by the names its own listing
   gives, into memory (`ResourcePacks.Open`, :63 to :70), a folder's files are those its listing
   finds under `assets/minecraft`, and nothing of a pack is written. Entries named
   `assets/minecraft/../../../../../../tmp/voxel-187-climb.txt`, one climbing from
   `textures/block`, an absolute `/assets/minecraft/...`, one parted by backslashes and one
   beginning `../` opened, drew and were read again with nothing written, a search of the disk
   after finding none of their names. A folder pack holding a link to its own parent is listed and
   drawn. The default packs are taken only by their SHA-512 (`DefaultPacks.Accepts`).
8. The game reads no block model and no block state. Its blocks' shapes are its own
   (`BlockShape`), and the JSON it reads of a pack is a texture's `.mcmeta` (`ResourcePacks.Meta`,
   which takes a file that is no JSON as none) and the models of OptiFine's form
   (`PackModels.Read`, which catches the reader's exceptions and four others). The font is the
   `font/ascii` sheet alone (`PackGui.cs:99`), its glyphs' widths measured from its pixels, with no
   file of glyph sizes, and a sheet that is not square or not of 16 cells is left out.

The mends I would make, in this order. First the rule and 1 to 3, the start ended, a hang and the
world undrawn. The reload builds the new packs' pieces and takes them all only once each is built,
so a throw leaves the game drawing with the packs before. A file of a pack is read within a bound,
a regular file or an entry of its declared length up to 64 MB, a read that fails or passes the
bound taken as a file the pack lacks, and the pack named once on the HUD. The connected tiles' range
is held below the int's top and its list stopped at the atlas's bound as it is read, the weights
summed in a long, and a section that cannot be meshed said in the engine's log. Then 4 to 6, the
memory. A picture's size is read from its header before it decodes and refused past 4,096 a side,
an animation keeps one frame for each index it plays, and a model is held to 4,096 boxes with each
number finite or the model left out. Each with a test that fails first on a pack the test makes,
the zip of 2.5 GB of zeros among them, made in the test in about 10 s.

Item 187's first commit, the rule and findings 1 to 3 mended within the bounds as set, and finding
7's result held by a test. Each test was run first on a copy of `069a7d9` with the tests added, the
reload's test with its seam's lines added there too.

The reload. The pieces drawn with the packs, the terrain's atlas, the icons, the stacks on the
ground, the bits, the cracks, the interface, the view from inside a block, the sky, the mobs and the
blasts, take the packs in turn, each loading its textures as it does. A fault partway is met by each
piece taking the packs before again, the game's own art at the start, and the new packs closed
(`VoxelGame.ReloadPacks`). The pieces are not split into a build and a swap, since each makes its
textures in its own `Use` and splitting them would touch ten classes for the same end, every piece
on one set. Where the packs before fault too, each piece takes none, which reads nothing of a pack.
The packs are named once on the HUD and in the log. The start, F3 with T and a choice of packs, the
packs screen's Done, `voxel.packs` and the default packs' arrival among them, call the reload
through a catch of their own, so a fault the reload let through is said and the program goes on
(`VoxelGame.ReloadHeld`). The seam is `VoxelGame.PackStep`, called before each piece by its name, an
instance member and public since the tests see none of the game's internals.
`A_Reload_Failing_At_A_Piece_Draws_On_With_The_Packs_Before_And_Holds_None_Of_The_New` throws at
the sky once and then twice, and holds that the game draws on with the packs before, named once, and
then with its own art, each zip opened after to be written alone, which a handle kept would refuse.
It fails on the head by the fault out of `ChoosePacks`.

The reads. A file of a pack is read within 64 MB, and one that cannot be read is taken as a file
that pack lacks, the next pack's or the game's own drawn, its pack named once on the HUD with the
file and the reason (`ResourcePacks.Read` and `Faults`). A folder's file is read only where the
system gives it a size within the bound, its link's target's where it is a link, so a pipe or a
device, which the system gives no size, is left out before it is opened, since .NET tells a file's
kind no further than a folder or a link. A zip's entry is refused by the size its headers give and
read to that size.
- `A_Pack_Entry_That_Does_Not_Inflate_Is_Left_Out_And_The_Game_Opens_Saying_So` fails on the head
  by `InvalidDataException` out of the start.
- `A_Pack_Entry_Inflating_Past_64_MB_Is_Left_Out_Before_It_Is_Read`, 2 GB of zeros zipped in the
  test, fails on the head by `IOException`, "Stream was too long", out of the start.
- `A_Folder_Packs_Picture_That_Is_A_Link_To_A_Device_Is_Left_Out_Before_It_Is_Opened` fails on the
  head by `OutOfMemoryException` out of the start.
- `A_Folder_Packs_Picture_That_Is_A_Pipe_Is_Left_Out_Before_It_Is_Opened` fails on the head, its
  read giving no answer within 10 s. It and the device's test run on Linux and macOS alone.

The connected textures. A range is counted in a long, so one ending at the int's top ends there,
the list stops past the atlas's 512 tiles, which leave its rule out, the random tiles' weights are
summed in a long, and a section that cannot be meshed is said in the engine's log, where it went to
the console's error stream alone.
- `A_Connected_Rule_Naming_Tiles_To_The_Ints_Top_Or_Past_The_Atlas_Is_Read_At_Once` fails on the
  head, its first rule giving no answer within 10 s.
- `Random_Tiles_Whose_Weights_Pass_The_Ints_Top_Together_Choose_One_Of_Them` fails on the head by
  `OverflowException`.

Finding 7. `A_Packs_Entries_Named_Out_Of_The_Packs_Folder_Write_Nothing_Outside_It` opens a zip of
entries climbing out of the packs folder, an absolute one and one parted by backslashes among them,
draws it, reads it again and holds that the test's folder holds the zip alone after. It passes on
the head, as it is a test that stays.

The test game takes the paths of packs to open (`HeadlessGame`). The eight tests take 7 s together.
The strict build is clean, 466 tests pass outside the GPU's lock, and the play passes in 190 s, its
captures within 8 levels of the last play's but sky2's, which moves as much between plays.

The pin of the engine's 6.2.86, `c54a38c6`, published at 20:45 and listed by nuget by 21:02, with
the parts that waited on it as rehearsed on 6.2.83, the engine having added only the scene packs'
types between. 3DEngine is at 6.2.86 and the workflows' engine checkout at
`c54a38c66f3ff6f1240396c0265cb3e6972928cb`. The bits' shader takes the bit's middle as a place
(`bits.slang`). The origin's step is told to the engine by `MoveWorld`, the land's meshes, its
groups and its lamps' cubes kept as they are. A lamp filling its cell with no face showing is left
out of the lights (`SectionMesher.Emitters` and `ChunkRenderer`). The diffs applied as rehearsed,
one context line of the mesher's moved by item 186's wrapping, and one comment over the lamps
reworded. `A_Lamp_Shows_The_Faces_Its_Neighbors_Leave_Open` reads the faces the emitters of the head
lack.

The strict build is clean on 6.2.86 restored from nuget, 467 tests pass outside the GPU's lock, and
the play passes cold in 260 s and warm in 173 s in a cache of its own. Against the captures of
6.2.60 the pictures move by 44 levels at most and on 116 pixels at most past 8, the shadow's along
its soft edge both ways and noon's at lone pixels, sky2 as it moves between plays, a sheet of the
two in the dump at `21-28-item-264-pin.png`.

Item 187's second commit, findings 4 to 6 mended within the bounds as set. Each test was run first
on a copy of `aa19470` with the tests added.

A picture's size is read from its PNG header before it decodes, and one past 4,096 wide or past
4,096 by 16,384 pixels is taken as a file its pack lacks and its pack named once, a strip of 32
frames of 512 pixels taken whole (`ResourcePacks.Decoded`). A file that is no PNG is taken as a
picture that does not decode, as the decoder took it. The default packs' largest picture is 1,024
wide, a title panorama's, their tallest strip 1,024 by 64, the conduit's wind, and a block's 512 by
32, the flowing lava's, their largest file 440 KB. The engine's decoder reads a PNG whole with no
smaller decode, so a strip wider than the atlas's tile is decoded whole and resized after, and the
bounds stand alone. An animation cuts each frame from its strip once however often its list plays
it, and its list stops past the atlas's 512 tiles, past which the atlas draws it still. A model of
OptiFine's form is read with 4,096 parts and 4,096 boxes at most, every number finite and its
picture's size above 0, or it is left out and the mob drawn with its own model
(`PackModels.Parse`). A folder's file the system gives no size is refused with that reason alone.
- `A_Picture_Past_4096_Wide_Or_4096_By_16384_Pixels_Is_Left_Out_Before_It_Is_Decoded` gives a
  picture 4,097 by 1, one 4,096 by 16,385 and a strip 512 by 16,384, made in the test, and fails on
  the head, which decodes the first.
- `An_Animations_Frames_Are_Cut_Once_Each_However_Long_Its_List_Plays_Them` gives a list of 200,000
  entries over a strip of two frames and fails on the head, which cuts 200,000 frames.
- `A_Model_Past_4096_Boxes_Or_With_A_Number_Past_A_Float_Or_No_Picture_Size_Is_Left_Out` gives a
  model of 4,097 boxes, one translated by 1e39 and one with a picture's size of 0, each refused, and
  one of two boxes read, and fails on the head, which reads the first.

The strict build is clean, 470 tests pass outside the GPU's lock, and the play passes in 172 s, its
captures within 10 levels of the pin's play but sky2's, which moves as much between plays.

The fourth read, of the inventory's windows and the world's menus by what their tests do not reach,
before any mend. The tests reach the slots' moves (`SlotMovesTests`), the crafting grids, the
chests' loot, the inventory's save, the new world's screen and the packs screen's listing, and
none of the window's clicks or the menus' buttons, since those take the pointer and the keys through
the engine, which a test game is given none of. The read went by the code through the window's
click, right click, drag, double click, shift click, number keys, Q and sort in the inventory, the
crafting table, the furnace, the chest and the creative window, its open and its close, and through
the game menu, the worlds' screen, the packs screen, the options' screens and the death screen. A
seed typed with a G in it acts no toggle, since a field taking text holds the keys back
(`VoxelGame.cs:842`). Six findings, all low.

1. G and F5 act behind an open screen. `VoxelGame.Toggles` (:1136) runs wherever no field takes
   text, so with the inventory or a menu open G steps the light's quality and writes the options
   file (:1166) and F5 steps the view (:1163), where the frame's comment says no key of the
   player's acts behind a screen (:854 to :857). The mend I would make is the two held to the game
   with no screen open, as E is (:1150).
2. A pack on the list whose file is gone is left out unsaid. `ResourcePacks.Open` passes over a
   path that is neither a folder nor a zip (`ResourcePacks.cs:71` to :91), so a pack the player
   deleted from the folder stays on the list, drawn by nothing, where one that cannot be read is
   named on the HUD. The mend I would make names it once as a pack that is not there.
3. The worlds' screen scrolls by the wheel's move cut to an int (`Menus.cs:373`), so a touchpad's
   steps of less than one never scroll it, where the packs screen (:317) and the creative window
   (`InventoryWindow.cs:168`) take the move's sign. The mend I would make takes its sign.
4. Save the World in a transient run or a world played unsaved does nothing and says nothing
   (`Menus.cs:131`, `VoxelGame.SaveWorld` returning where there is no save). The mend I would make
   says on the HUD what `voxel.save` answers.
5. E opens the inventory under the death screen (`VoxelGame.cs:1150`, the death spilling and
   clearing the inventory at :1001 to :1009), an empty window under the Respawn button, closed by E
   or Escape. The mend I would make holds E while the player is dead.
6. A crafting table's window stays open as a table's after a blast takes the table. Furnaces and
   chests close theirs as their blocks go (`VoxelGame.cs:897` and :902), and nothing closes
   `AtTable`, so its grid of three crafts on until the window shuts. The player cannot break a block
   with the window open, so only TNT or a creeper takes it. The mend I would make closes the window
   where the block at the table's place is no table.

Read clean. The window's clicks take, put, join, swap and split within each stack's room, and a
right click puts one only onto its item with room (`InventoryWindow.Click` and `RightClick`). A
drag's spread and its shares, a double click's gathering and a sort keep every item
(`SlotMoves`, tested). A shift click into a furnace goes by what the item smelts or burns, into a
chest by its rows, and out of either into the inventory. A shift craft stops where the grid's
materials or the inventory's room end. The made slot and the furnace's output are only taken from.
The number keys swap a slot with the hotbar's, but its own, and Q drops one or with Control the
stack, thrown by the game in the frame. Closing the window puts the grids' stacks and the held one
back and throws what does not fit (`InventoryWindow.Close`). A chest or a furnace broken while open
closes its window as its contents drop, so nothing is taken out twice, and a death closes the
window before the inventory spills. The creative window's scroll is clamped to its rows and a click
puts one of the item in the chosen slot. The game menu's buttons, the worlds' screen, its list
clamped to its saves, and the packs screen, its rows, its order and its Done through the reload's
catch, read clean, as do the options' sliders and choices and the sky's colors' screens.

The gap the tests leave is the window's and the menus' input, which the engine gives a test game
none of. The mends above can each be tested through the game's state, by a seam no larger than the
reload's where one is needed, and the clicks themselves stay read by the code and by the play's
picture of the inventory. The order I would take is 1 and 5 together, the keys behind a screen,
then 6, then 2 to 4, in one commit or two.

Item 188, the fourth read's six findings mended as proposed, with the picture refused by its size
giving way to the next pack's. Each test was run first on a copy of `39e5442` with the tests added,
and with the seams' lines added there too.

The keys. G and F5 act only with no screen open, the inventory's or a menu's, as E acts only with no
menu open, and E does nothing while the player is dead, whose inventory has spilled
(`VoxelGame.Toggles`). The seam is `VoxelGame.Pressed`, the keys a test presses for the toggles in
place of the engine's, an instance member as `PackStep` is.
`The_Light_And_View_Keys_Act_On_No_Screen_And_The_Inventorys_Key_On_No_Death` presses G and F5 with
the inventory open, with the game menu open and with neither, and E after a death, and fails on the
head, whose G steps the light's quality behind the inventory.

The crafting table's window shuts where the block at the table's place is no table, as a furnace's
and a chest's shut as theirs go. `A_Crafting_Tables_Window_Shuts_As_The_Table_Goes` opens a table
by `voxel.place`, sets air at its place as a blast would and fails on the head, whose window stays
open.

The game menu's Save the World in a run that keeps no save says on the HUD what `voxel.save`
answers, the same words from one place (`VoxelGame.NoSave` and `SaveAsked`).
`The_Menus_Save_In_A_Run_That_Keeps_None_Says_What_The_Consoles_Save_Answers` fails on the head,
whose save says nothing.

The worlds' screen and the packs screen scroll a row for each move of the wheel by its sign
(`Menus.Scrolled`), so a touchpad's moves of less than one row scroll them.
`A_Lists_Wheel_Moves_It_A_Row_For_A_Move_However_Small` fails on the head's worlds' screen, whose
cut to an int takes a move of a quarter as none.

A pack on the list that is not there, or that is neither a zip nor a folder, is named once
(`ResourcePacks.Open`). `A_Pack_On_The_List_That_Is_Not_There_Is_Named` fails on the head, which
names nothing. A picture refused by its header's size is passed over for the next pack's copy in
the same call, the size read from the header before the metadata and before the decode, so a
texture that does not animate is decoded once as before (`ResourcePacks.FirstPicture`).
`A_Picture_Refused_By_Its_Size_Gives_Way_To_The_Next_Packs_In_The_Same_Call` fails on the head,
whose first call gives no picture, the game's own drawn until a reload.

The strict build is clean, 476 tests pass outside the GPU's lock, and the play passes in 182 s, its
captures within 10 levels of the pin's play but sky2's, which moves as much between plays.

The fifth read, of `Entities` whole, the item drops and `World/Lighting.cs` by what their tests do
not reach, before any mend. The tests reach a mob's fall, hit, way, home and keeping by its column,
the hostile mobs' spawning, hunting, burning and blasts, the crowding of two mobs across a cell's
edge, the origin's step, and the light's sky, shafts, lamps, water, canopies, glass and a lamp at a
column's edge. Each candidate below was held by a test on a copy of `ff96b6e`, outside the GPU's
lock, and two failed. Those are the findings, each test the one that fails on the head.

1. Mobs that stand still beside land not loaded gain speed while they overlap. `MobMind.Crowd`
   (`Mobs.cs:52` to :82) pushes every pair of mobs whose boxes overlap, adding to their velocities
   each frame, while `MobMind.Think` (:27 to :37) moves only a mob whose column has the columns
   around it loaded, so a frozen mob's velocity is never spent or slowed (`Mob.Move`, `Mob.cs:442`
   to :460). Two pigs a third of a block apart in a test game's last loaded column east of the
   player gain 57.6 blocks a second over 90 frames of a thirtieth of a second, and each moves 1.9
   blocks in the frame its neighbors load, the speed growing for as long as the player stays. A herd
   placed close at the edge of what is loaded is flung across the land as the player comes near,
   into land not loaded where it stands frozen again, and a mob standing in a column never loaded is
   left out of a save made while it stands there, since `Mobs.Columns` (:335 to :354) gives the mobs
   of the columns placed alone.
   `Two_Mobs_Overlapping_Where_Their_Neighbors_Are_Not_Loaded_Gain_No_Speed` holds their speed under
   10 blocks a second and fails on the head at 57.6. The mend I would make pushes a mob only where
   its own column has the columns around it loaded, as it thinks only there.
2. A spider spawns into the wall beside its cell. `Mobs.Spawnable` (`Mobs.cs:419` to :429) holds
   free only the cells over the spawn's middle, while a spider's box is 1.4 blocks wide, reaching
   0.2 of a block into each cell beside it, so in a cave a spider spawns with its sides in the
   stone. `A_Spider_Does_Not_Spawn_Into_The_Wall_Beside_Its_Cell` sets a stone beside a cell of a
   stone floor, where a zombie may spawn, and fails on the head, which lets the spider spawn there.
   The mend I would make holds free every cell the kind's box covers at its place.

Read clean, by the code and the light by two tests of mine that pass on the head. A stack whose
column unloads is kept by the column at its place in the land and set down as the column loads, and
a stack ages and moves only in a loaded column (`ItemDrop.Fall`, `ItemDrops.Leaving` and `Joined`).
The player picks up only while alive and while the game's time passes, which a menu holds, and picks
up with the inventory's window open, as the source does. An arrow is gone a minute after it is
loosed, stuck or flying, and moves with the origin. A mob's hit on a dead player harms nothing,
`Vitals.Damage` returning on a death, and the hostile mobs hunt only a living one. A mob's hit
knocks it away from a hit at its own place along its facing. A dying mob ages wherever it stands,
`MobLife.Age` running over every mob, so it vanishes and drops its loot in a column not loaded too.
The origin's step moves every body, arrow, blast and mob's home (`VoxelGame.Recenter`), and the kept
mobs and stacks hold places of the land. A lamp on a column's edge takes its light back from the
column beside it as it is broken, and a column loading again beside a lamp takes its light and gives
it back behind a wall set across the edge (two tests, passing on the head). The light keeps the
land's block places, which the origin's step does not touch. Two ways normalized in the blast's push
on the player and in a skeleton's shot are zero only for a blast half a block over the player's
middle or a skeleton's eye on the player's, which play does not reach, and are left.

The order I would take is 1 and then 2, in one commit, each test failing on a head copy.

Item 189, the fifth read's two findings mended as agreed, in one commit. Each test was run first on
a copy of `ff96b6e` with the tests added.

1. The crowding pushes each mob of a pair only where it moves, its column's neighbors loaded, the
   same check by which the passive mobs think and the hostile ones hunt, and pushes a mob away
   from the player likewise, so a mob standing still beside land not loaded takes no push and the
   one of a pair that moves takes its own (`MobMind.Crowd`).
   `A_Mob_Standing_Still_Beside_Land_Not_Loaded_Takes_No_Push_And_One_That_Moves_Takes_Its_Own`
   crowds two pigs in the last loaded column east of the player and two either side of its west
   edge for 90 frames, and holds the three standing still at no speed and the one that moves pushed
   west. It fails on the head, which gives the first a speed.
2. A hostile mob spawns where every cell its box takes about the cell's middle, at each height it
   takes, holds no box and no water, a spider's reaching into the cells beside its own
   (`Mobs.Spawnable`).
   `A_Spider_Does_Not_Spawn_Into_The_Wall_Or_The_Water_Beside_Its_Cell_Where_A_Zombie_Does` sets a
   stone and a water beside two cells of a stone floor, and holds that a zombie spawns in each and
   a spider in neither, and one in an open cell. It fails on the head, which lets the spider spawn
   beside the stone.

The light's two tests of the read stay in `LightingTests`, a lamp on a column's edge taking its
light back from the column beside it as it is broken, and a column loading again beside a lamp
taking its light and giving it back over a wall set across the edge. Both pass on the head and take
under a second together.

The strict build is clean, 480 tests pass outside the GPU's lock, and the play passes in 191 s, its
captures within 10 levels of the pin's play but sky2's, which moves as much between plays.

The sixth read, of `Player` whole and `Generation` whole by what their tests do not reach, before
any mend. Each candidate was held by a test on a copy of `e84d2db`, outside the GPU's lock, and one
fault failed two of them.

1. The vitals' reckoning carries over a time out of survival. The vitals are taken only while the
   player lives, in survival or adventure (`GameFrame.Live`, `[InState(Living.Yes)]`), and each
   frame they take the move since the place they last read (`Vitals.Update`, `Vitals.cs:82` to
   :99). Nothing starts that reckoning again as the mode comes back, as `Vitals.Arrived` starts it
   after a teleport (`VoxelGame.cs:1003` to :1007), so the first frame back in survival takes the
   whole way since the mode left it as one move, and the fall it had counted.
   - `A_Flight_In_Creative_Tires_The_Player_Nothing_Back_In_Survival` flies 200 blocks east in
     creative and fails on the head, which gives the player 2.0 of exhaustion on its return, half a
     point of food. An hour's flight at creative's 21.6 blocks a second would take 194 points at
     once, every point of food the player holds and its saturation.
   - `A_Fall_Begun_In_Survival_And_Ended_In_Creative_Harms_Nothing_Back_In_Survival` falls 12.4
     blocks in survival, lands in creative and comes back, and fails on the head, which takes 10 of
     the player's 20 health on its return, the fall's damage dealt minutes after the fall.
   The mend I would make starts the reckoning again as the player comes to live, by
   `Vitals.Arrived` on entering `Living.Yes`, as a teleport starts it.

Read clean, by tests that pass on the head where named. A seed's columns are the same made alone, in
order and on eight workers at once, 49 columns of seed 1 alike block for block
(`A_Seeds_Columns_Are_The_Same_Made_Alone_In_Order_Or_On_Many_Workers_At_Once`). Seeds at the ints'
ends, 0 and -1, and columns at the land's edge, 67,108,863 columns out either way, make ground
within the world, the overworld's and the flat world's, with no index past its array. A tree, a
patch or a structure is placed by every column its reach covers from the cells around, each column
asking the same noise of the ground under a trunk in a neighbor (`Caves.Places.DensityAt` reads the
noise's corners at any place), so a crown is not cut at a column's edge. A body moves in steps of at
most 0.45 of a block, shorter than the player's or any mob's box is wide or tall, so no frame the
game takes carries one through a block, and one standing in a block walks out. The ray to the block
aimed at meets a smaller block's pieces, a mob is aimed at only where it is nearer than the block,
and the camera behind or before the player stands short of the nearest block any of five rays meets.
A teleport and a respawn start the vitals again, a vital set by a save or a command is held to its
range, and breath, food and saturation stay within theirs.

One aside of the last lane's, read on the way and low. A stack, a quarter of a block tall, falling
at its 40 blocks a second in a frame of a twentieth moves 0.4 a step, so in a sixteenth of the
places over a snow layer, an eighth of a block thick, it passes the layer and lies inside it on the
block under it. At 60 frames a second it steps 0.33 and does not. I would leave it.

The order I would take is the one mend, its two tests in one commit.

Item 190, the sixth read's finding mended in one commit, in the game's mode setter rather than on
entering `Living.Yes`. Each test was run first on a copy of `e84d2db` with the tests added.

The vitals' reckoning starts again as the mode comes to one in which the player lives from one in
which it does not (`VoxelGame.Mode`, which calls `Vitals.Arrived` as a teleport does). Every way
into survival or adventure sets the mode through it, from creative or spectator by the switcher or
`voxel.mode`, and into a world opened in survival or adventure, whose mode is set as it opens,
where `Vitals.Set` starts the reckoning too. A switch between survival and adventure keeps the
reckoning, both taking the vitals, and a respawn starts it by `Vitals.Set`.

The mend was first an `[OnEnter(Living.Yes)]` system, and both tests failed on it. A probe logging
the frame showed the `InState(Living.Yes)` system that takes the vitals running in the frame of the
switch, the fall dealt there, and the enter system in the next frame, after it. `Living` is computed
from the mode, and the engine activates a computed state that comes back from none for its enter at
the next transition point, while its `InState` systems take it as current at once. That stands in
the engine's ASKS.md as an entry, uncommitted there. The setter is the game's own place for the
reckoning, and no system of the game's needs the enter.
- `A_Flight_In_Creative_Or_Spectator_Tires_The_Player_Nothing_Back_In_Survival` flies 200 blocks
  east in creative and 200 in spectator, each followed by survival, and holds the exhaustion, the
  saturation and the food as they were. It fails on the head, whose 4.0 of exhaustion take a point
  of the player's saturation.
- `A_Fall_Begun_In_Survival_And_Ended_In_Creative_Harms_Nothing_Back_In_Survival` falls twelve
  blocks in survival and lands in creative, and fails on the head, which takes 10 of the player's
  20 health on its return.

The strict build is clean, 482 tests pass outside the GPU's lock, and the play passes in 186 s, its
captures within 10 levels of the pin's play but sky2's, which moves as much between plays.

The seventh read, of `Blocks`, `Items` and `World` apart from the save and the lighting by what
their tests do not reach, before any mend. Each candidate was held by a test on a copy of `e84d2db`,
outside the GPU's lock, and two findings failed theirs.

1. Water stopped at the edge of a column not loaded stays stopped once that column loads. A cell of
   water flows only at its tick, which a block changed beside it asks for (`Fluids.Around`,
   `VoxelWorld.cs:152`), and its flow into a column not loaded is refused, `SetBlock` setting
   nothing there (`VoxelWorld.cs:131` to :132). As the column arrives nothing asks the water beside
   its edge to look again (`VoxelWorld.Add`, :60 to :72), so a stream or a fall the player made ends
   in a wall of air along the edge for good.
   `Water_Stopped_At_A_Column_Not_Loaded_Flows_On_As_It_Loads` sets a source at x 14 beside a column
   not loaded, runs 100 ticks, loads the column and runs 100 more, and fails on the head, which
   leaves air at x 16. The mend I would make has a column's arrival ask each cell of water on its
   loaded neighbors' edges that faces air across the edge to look again.
2. A block falling and a lit TNT as the world closes or switches are lost. Both are entities of the
   engine's ECS while they fall or burn, their cells taken out of the world as they began, and the
   save keeps neither (`VoxelGame.SaveWorld`), the switch letting them go with the world left
   (`VoxelGame.cs:573` and :576). A sand, a gravel, a felled tree's log or a TNT is gone, neither a
   block nor an item.
   `A_Block_Falling_And_A_Lit_Tnt_As_The_World_Closes_Are_In_It_When_It_Opens_Again` lets a sand
   fall from 12 and lights a TNT, closes the world a frame after and opens it again, and fails on
   the head, which holds neither, the column's other change kept. The mend I would make sets each
   falling block down where it would land and each lit TNT back as a block at its cell as the world
   closes or switches, before the last save, a timed save leaving them falling and lit.

Read clean, by the code and the tests named. A furnace's save keeps its slots, its burning and its
smelting, and a chest's its slots and its loot to roll (`WorldSave.FurnaceRecord`, `ChestRecord`), a
furnace or a chest whose block goes spills what it held, and one in a column not loaded is left
alone, both updates asking the column first (`Furnaces.Update`, `Chests.Update`). A crafting grid's
and the held stack go back on the window's close. A furnace smelts only where its output has room, a
tool breaks at its durability (`VoxelGame.Wear`), the inventory adds within a stack's size, and the
chests' loot rolls at most 24 of an item and one of a tool. A blast's rays end as their strength
does and set nothing in a column not loaded, a fire in a column not loaded is let go at its tick and
named again as its column arrives (`VoxelWorld.Kindled`), a water's tick at a cell that is no water
is dropped, and a falling block landing where it cannot stand drops its item. Of every shaped block,
`Every_Shaped_Blocks_Collision_Against_What_It_Draws` finds the boxes that stop the player the boxes
it draws, but the cactus's a sixteenth inside them and the fence's half a block over its post, as
each is meant, and the plants, the torches, the rails, the carpets, the fire and the cobweb walked
through.

One low finding is left. A fire held only by a block in a neighbor column not loaded goes out at its
tick, `Fires.CanStand` reading that block as air, at least 32 blocks from the player where no fire
is seen.

The order I would take is 1 and then 2, in one commit, each test failing on a head copy.

## Items closed, parked or shortened

Items closed without a mend, recorded or parked, and the standing items' texts as they stood before
they were shortened on 2026-11-09, each under its number; the devlog reads the items under Items
done alone, so nothing here counts as done.

29. **Generation and meshing on the GPU, the blocks kept on the CPU (Decision 12).** The decision of
    about 00:30 on 2026-10-21: compute shaders for generation and the rest are a big one. The
    reading at `5cd1521`: each column is generated on a worker task off the main thread, and item
    16's flight readings put the frame's cost on the main thread in the join and the meshing, 21 to
    29 ms a frame at 12 columns, so generation on the GPU makes columns arrive faster and meshing on
    the GPU is what cuts the frame; the engine's compute, `LoadComputeShader`, the shader buffers
    and `ComputeShaderDispatch`, reads a buffer back only through `ReadShaderBuffer`, which holds
    the program until the GPU has caught up, and draws nothing a shader wrote as a mesh. The
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
    costs. Three or four commits, one a part, each after its package. Asked at 06:12 on 2026-10-25
    whether the fixed-point noise, which moves every seed's land, keeps a generator version in
    WorldInfo so worlds saved before keep the float noise or replaces it outright with seams in old
    worlds, the review chose outright, under the rule of 2026-10-23 that nothing is kept for
    upgrading before a release, the tests and play.sh pinned to the new places in the same commit
    and seed 1's places read again for the records, saved worlds generated anew, unless it is
    decided that saved worlds keep their land. Corrected at 06:13, the change is far smaller than
    asked, the noises being Perlin's improved noise over a permutation table shuffled from the seed,
    which a fixed-point port keeps with the same gradients, fades and lerps, so the values differ by
    the rounding alone, about 1e-5, and the blocks that change are those where a height or a cave's
    density sits at a rounding edge, a scattering and not new land, seed 1's pinned places expected
    to hold, an old world showing a rare one-block step at its saved columns' edges; the changed
    blocks over a region counted first, before the GPU half. The shader's fixed point is 32-bit, the
    engine enabling no 64-bit integers, a Q16 product split into two 32-bit multiplies giving C#'s
    64-bit result exactly; on the CPU a column takes 15.45 ms, the caves' density grid 5.31 and the
    256 ground lookups 0.82, the rest the block loop, the ores, the plants and the structures, so
    part one's first gain is the caves' grid, the ground's 2D noises going with it and the CPU
    combining the fields as today. Part one's first half at `2549786` on 2026-10-25, every noise in
    fixed point on the CPU with the changed blocks counted, a scattering of 0.032% and no pinned
    place moved; the GPU half next, the caves' corner readings in a compute shader read back without
    a wait, voxel.gpu on or off as a setting and voxel.gpu verify comparing both paths bit for bit
    in play.sh. The GPU half at `1d31dda` on 2026-10-25, the caves' readings in a compute shader
    read back without a wait in batches of 8 with the CPU path kept and verified bit for bit, a
    worker at 10.98 ms a column against 16.47, which ends part one; part two, the meshing on the
    engine's item 69, next. Part two waits on the engine from 07:48 on 2026-10-25, its ASKS.md entry
    in the engine's tree the engine's item 121, since a mesh over shader buffers on 5.2.291 holds
    one stream and the field reads colors from a UNorm8x4 stream alone, so a section meshed in
    compute would bounce white, and holds whole buffers, so each section would be sized for its
    fullest case; the reading before it, three ten-second sprint flights east over unloaded land on
    seed 1 at render distance 12 offscreen at 1280 by 720 on the RTX 4070 Laptop GPU, the frame
    22.0, 37.5 and 37.0 ms, update.mesh 1.0, 7.8 and 6.9, update.stream 0.9, 1.9 and 2.4,
    cpu.scene_field 6.8, 8.6 and 9.3 and the GPU's passes about 12, so the main thread binds the
    frame and the meshing is the largest part the game owns, the two profile values going with the
    next commit. Part three next where it needs no engine change, item 95 otherwise once the package
    publishes. Part three is dropped at 07:51, item 92 having removed far land under Decision 41, so
    nothing of it remains to move to the GPU; the light levels stay a fourth part if their cost
    shows, the flights' numbers showing the meshing and the field's main-thread cost and not the
    flood. Part two's number read again at 17:40 on item 95's tree, the same three sprint flights at
    distance 12 offscreen at 1280 by 720 on a quiet GPU: Debug frames of 21.7, 16.7 and 19.0 ms with
    update.mesh 0.93, 2.59 and 1.09, Release with the entities 16.3, 13.7 and 14.6 with 0.35, 1.07
    and 0.15, Release on DrawMesh 17.0, 14.6 and 18.0, against the morning's Debug frames of 22.0,
    37.5 and 37.0 with update.mesh up to 7.8 on 5.2.291, the difference the package, item 95 and the
    quieter GPU; so the meshing is at most 2.6 ms of a Debug frame and about 1 of a Release one, and
    the frame stands under 16.7 ms in Release in two flights of three. On 5.2.313 part two would
    upload each frame's sections in one write of blocks, light and borders of about 17 KB each
    before one dispatch, since UpdateShaderBuffer waits for every dispatch before it, needs a count
    pass read back a frame later before a range of fixed capacity is allocated from a pool and the
    mesh pass runs, so edits stay on the CPU mesher, and ports the whole mesher to Slang, the seven
    face classes, the shapes and plant walls, the connected glass, the water depth, the per-corner
    light with the open-cell gather, the corner shade, the biome tints, the atlas's spots and the
    field's colors, several commits over days for those 1 to 2.6 ms in flights alone. Chosen, the
    meshing moves to the workers first, SectionMesher filling its arrays off the main thread and the
    main thread taking the finished arrays in and uploading them, a commit or two; part two as
    written is held on these numbers for the decision, Decision 12 being the owner's. The meshing on
    the workers at `bb0b9bb` on 2026-10-25, update.mesh under half a millisecond in the flights and
    the settle no slower, the readings in the devlog's note; part two as written stays held for the
    decision.

156. **The ground's atlas takes the anisotropic filter.** Decision 59 on 2026-11-04. The engine's
    item 201 filters every linear texture sampler at 16, but the ground does not move, since
    ChunkRenderer sets its atlas to TextureFilter.Point and terrain.slang reads it with SampleLevel
    at a level of its own choosing, which takes no anisotropy. The ground takes the 16 once the
    atlas is sampled linearly with gradients (Sample or SampleGrad) at Trilinear, each tile's mips
    clamped so neighbors do not bleed across tile edges, read by the ground at a grazing angle
    against the picture before and by the frame; the owner says whether the blocks keep their pixel
    look, which Point gives and the 16 would soften. After the next package, with 154 and 155.
    Closed at 11:40 on 2026-11-04 on the answer that the game filters by pixel and needs no change,
    the blocks keeping their pixel look, the atlas at Point, nothing changing.

164. **The engine's split packages at the restore.** From 3DEngine's item 225 on 2026-11-09, read by
    review the same day. The engine is a project an area since its `1a11d7bf`, each packed under the
    package 3DEngine and the area's name at the one version, and the flat API's package 3DEngine
    depends on the twelve. The game has no nuget.config, maps no package source and names no source
    in its workflows or scripts, so its restore takes the package 3DEngine and the twelve it depends
    on from nuget.org alone, and nothing changes at the next pin. Done as read; the pin itself is
    the next package's item.

114. **Cliffs are rarer, the land mostly rolling.** Reported at 13:38 on 2026-10-24, there are too
    many cliffs. The cliffs' cuts come where the terrain's slope is steep and the generator carves
    them; their count falls to what the source's land shows, a cliff here and there at a hill's or a
    mountain's side and none across the plains and the forests, by a higher slope for a cut, a wider
    spacing, or a noise that gates them by region, the coder reading the land of seed 1 at the spawn
    and the plains village for the count before and after and saying it in the commit; a test
    counting the cliffs over a seeded span against a bound, and a capture of the same land before
    and after. One commit, after item 113 and before item 94. The coder's reading at 15:38 on
    2026-10-24 found no cliff feature; what reads as cliffs is two things. The drops of two blocks
    or more between neighboring columns all lie along the rivers, the channel's cut from the valley
    floor to three under sea level over about six blocks giving a river on high ground a wall
    several blocks high, 1,458 such steps on the 512-block square at the spawn and 6,026 on the
    2,048-block square about the origin against 315 and 1,319 with the channel's cut off; and the
    brown lines across the plains are single-block steps of the rolling hills, plus or minus 18 at a
    scale of 160 over five octaves, a step's dirt side showing, none of them two or more away from a
    river. The call, the river channel's band widened with the drop it cuts so its banks fall at
    about a block per two blocks, in one commit with the counts before and after; the hills'
    amplitude on the plains and the forests halved only by decision, since it moves the land
    everywhere, asked. The first part at `3869d0c` on 2026-10-24, the banks sloping a block per two,
    the spawn's square at 460 steps of two or more where 1,458 stood and the village at 1 where 116;
    the second part waiting on that decision. The second part was answered on 2026-11-11 at about
    16:40, pictures first. The same views with the hills as they are and with their amplitude on the
    plains and the forests halved, at the spawn on seed 1 and at the plains village, from a walk's
    eye and from above, as pairs on one sheet with the count of steps beside each, sent for the
    choice; the halved land a build in the work folder and no commit, the choice made from the
    pictures. The sheet went out at 16:26, 16-26-item-114-hills in the dump of 2026-11-11. With the
    hills halved on the temperate land the spawn's square has 71,370 steps of one block where
    79,620, 228 of two or more where 460 and none of three or more where 7, the village's 40,561
    where 44,038, and the plains' brown lines stand in both pictures, the dirt sides of one-block
    steps that come of other terms of the height. The answer at 16:39 was that no difference is
    seen, that either is acceptable, and that the review decides what is best. The review's
    decision, the land stays as it is, the second part closed with no commit, the river's banks of
    `3869d0c` being what the item changed; the trial's worktree is deleted.

## Head records

The head paragraph of REVIEW.md as it stood when it was shortened, with the order line where it had
one, every push and run from 2026-10-23 to the shortening in it; a later shortening appends its head
after.

Reviewed up to `4e956bc`, items 162 and 163, the last readings out of the comments and the engine
checkouts taking the submodules, pushed at 08:47 on 2026-11-09 with `9a3ec77` and one run on the
last, green at 10:07 with the play in 80 minutes through the engine's build with the Sky module and
the three test jobs in about two, which settles Verdict 9; they keep the form and carry the review's
files. Nothing is pushed after it. Before it `f43aa9e`, item 161, the workflows' engine checkout at
the package's commit and the lighting comments without their readings, pushed at 08:36 on
2026-11-09, its run red at 08:39 in the play job after 54 seconds, the engine's build without the
Sky module, Verdict 9; it keeps the form and carries the review's files. Before it `8d8d1b2`, item
160, the pin of 6.1.12 with items 157 and 159, pushed at 07:00 on 2026-11-09, its run green at 08:26
with the play in 85 minutes and the three test jobs in about two, the play through the ambient
occlusion toggle where 06fd267's twice ended in the engine's fault, which settles Verdict 8; it
keeps the form and carries the review's files. Before it `23ba89e`, item 153, the pin of 6.0.14,
pushed at 04:46 on 2026-11-01 after the run of `8efd1d0` went green at 00:30, its run green at 06:07
with play in 81 minutes. Two rounds of each package in the same hour at item 139's view, the GPU's
passes at 8 columns 12.8 ms to 9.3, at 12 17.4 to 11.8 and at 16 22.8 to 13.6, gpu.shadows 2.2 to
2.6 to 0.41 to 0.47, the depth pass at 16 3.1 to 0.19, the mirror 3.0 to 1.1, the scene 8.8 to 6.6,
global illumination 5.2 either way, the frame held at 60 at 8 and 12 and 16.9 ms at 16 where 22.8,
the four passes' CPU at 16 unchanged at 1.7 microseconds a draw; the spawn view settled identical
within 3 of 255, the pair shown from the dump; the README's frame paragraph, the shadows' and the
mirror's costs and the table rewritten to 6.0.14's readings; 364 tests, the GPU play in 331 s. After
it `56e1372`, the review's files alone, the norm's rule 4.10 and item 154's record, pushed at 14:51
on 2026-11-03, with no run since the build passes over the review's files; it keeps the form. After
it `6eb6540`, item 154, the game on 6.0.91 with its far cascade and screen probes kept, pushed at
17:07 on 2026-11-04, its run red at 18:15 on macOS alone, the job never acquired by a hosted runner
for the macOS capacity notice and cancelled at its 15 minutes, the play in 65 minutes and the
Windows and Linux tests green, nothing of the game's in it; it keeps the form and carries the
review's files. After it `fe3638e`, item 155, the field's center ahead of the eye, pushed at 18:17,
its run green at 19:33 with the play in 76 minutes and the three tests in 2 to 3 minutes, macOS
acquired this time, the newest green commit; it keeps the form and carries the review's files and
the devlog. After it `7a548ff`, the review's files alone, the heads' runs, items 154 to 156 as they
closed and the spellings made American, pushed at 10:45 on 2026-11-05, with no run since the build
passes over the review's files; it keeps the form. After it `f37154b`, the review's record with its
done hash in backticks and the devlog, pushed at 18:52 on 2026-11-06, its run green at 20:24 with
play in 92 minutes and the tests in one to two, since the devlog is among the build's paths; it
keeps the form. Before it `8efd1d0`, item 152, the pin of 6.0.11, pushed at 23:07 on 2026-10-31, its
run green at 00:30 with play in 82 minutes, recorded under the item in REVIEW-ARCHIVE.md. Before it
`efbb357`, item 151, pushed with `7bd8f1c` at 13:40, its run green at 14:43 with play in 61 minutes,
the first mirror frame's 24 s read as the driver's compile, recorded under the item in
REVIEW-ARCHIVE.md. Before it `7e052e8`, the version to 0.2, pushed at 12:43, its run green at 13:40.
Before it `adf5141`, item 148's second commit, pushed at 08:17, its run green at 09:35 with play in
76 minutes. Before it `8a1e308`, item 150, pushed at 00:31, the play job's limit at 150 minutes, the
checks summing 4,568 to 4,983 s over two runs after item 141 gave lavapipe more to draw, the rain's
splashes the largest at 795 s and read every 20 frames until they fall under one where 300 frames
waited, the seed's field holding Control about a second of frames, none dropped, the README's play
paragraph gained. Before it `17704ca`, item 149, pushed with `8a1e308` at 00:31, the run of
`8a1e308` green at 01:28 with play in 57 minutes under the new limit, the flood taking a budget of 2
ms a frame at a step into an adjacent section with the last whole flood kept and a flood under way
run to its end, a jump of more than a section flooding at once so teleports and the captures stay
exact, the casters' search in the frame after. Before it `a272e45`, item 145, pushed at 23:27, its
run green at 00:29 with play in 61 minutes, the time budget built and left out since flights read no
gain, 12 columns at a 99th part of 86 to 105 ms without and 92 to 110 with, 8 at 49 to 72 and 47 to
55, within the noise. Before it `5d96667`, item 148, pushed with `a272e45` at 23:27. Before it
`b5626c6`, item 147, pushed at 21:59. Before it `5f31bdf`, item 143, pushed with `b5626c6` at 21:59,
the run of `b5626c6` green at 23:23 with play in 84 minutes, the sections grouped 2 by 2 across and
one high since groups two high drew the caves under the land and cost the GPU 3 to 4 ms at 12
columns. Before it `2c458b0`, item 141, pushed at 20:36, its run green at 21:56 with play in 80
minutes. Before it `7508d5e`, item 142, pushed at 19:18 on 2026-10-30, its run green at 20:31 with
play in 73 minutes. Before it `5c80c99`, item 140, pushed at 18:21 on 2026-10-30, its run green at
19:01 with play in 40 minutes. The start restores the options file's resolution and mode right after
InitWindow, before the game is built, the window appearing as saved about 8 s before the world loads
where it changed after; no monitor setting exists to read, and a hidden window cannot be shown later
since the engine takes a program- set hidden window as a --hidden run; the test reads the values and
a missing file's 1280 by 720. AGENTS.md's REVIEW.md line reads the head, the batch's items and the
Replies by decision in the coder's session. It carries NORM.md with N 7.7 and the README's first
line. Before it `fc0937a`, item 139, pushed by hand on 2026-10-26 while the sessions were paused
until 2026-10-30, its run green. Before it on 2026-10-26, each green: `9c07673` (138); `b25e668`
(137); `4aef50c` with `6001eed` (the README's quick links, 136); `7c36ec8` (42); `f1aa450` (27);
`5145d2f` (134); `0ba4cb4` (135); `7f59530` (54); `1944378` and `48e5bea` (133); `a4d383c` (95).
Each commit's detail stands in its item in REVIEW-ARCHIVE.md, its devlog note and its own words. On
2026-10-30 at 18:0x came items 140 and 141 (Decision 51), the README's links as the devlog and
3DEngine, a Rust voxel engine as the performance reference (Decision 52), and the ask that the
sessions spend fewer tokens at the same quality, N 7.7. Item 144 closed at its count on 2026-10-30,
the quads merging to 81 to 89% and not half; 143's other 245 MB is the driver's too, the ring's
mapped memory counted as the device mapping and its shared backing, the game's own arrays 68 MB
more. After it `ca7ee6d`, the review's files alone, item 153's record and the regenerated devlog,
pushed at 13:44 on 2026-11-01, its run green at 15:09 on the three systems. After it `06fd267`, item
158, the license as the Mozilla Public License 2.0 in its standard text, pushed at 15:30 on
2026-11-08, its run red at 16:30 in the play job alone, the game's process gone at the ambient
occlusion toggle after 58 minutes of passed checks, nothing of the commit's (Verdict 8), the job
rerun at 16:34 and red the same way, the exit the engine's (Verdict 8); it keeps the form and
carries the review's file.

Items done and verdicts settled before 2026-10-23 stand in REVIEW-ARCHIVE.md beside this file, their
numbers kept, moved there at 19:30 on 2026-10-23 so this file holds the current work alone and is
read whole before a batch; the archive is read when an item or a verdict named here is wanted in
full.

Reviewed up to `5ad3f58`, the review's file alone with the Replies of 174 and 176, pushed at 23:00
on 2026-11-11, no run by the workflow's paths. Before it `6b0043b`, voxel.spawning, asked on
2026-11-11, every mob kept out of the world from the next frame on and brought back where it stood,
the play switching it off before its first check, pushed at 21:31 on 2026-11-11, its run left queued
by GitHub's outage and not started, Verdict 12; its strict build is clean and 367 tests pass, two
plays under the layer pass whole in 317 and 218 s, and of their 17 captures dusk differs in 0.00
percent of pixels by more than 2 levels where 2.3 before, the night sky in 0.18, within its clouds;
it keeps the form and carries the review's files. Before it `1f1e955`, item 172, terrain.slang,
mob.slang and bits.slang returning unlit on the branch drawn unlit, each fragment about 590 KB
smaller and the fifth frame's 22 pipelines made in 20.3 s where 43.9 from an empty cache with the
layer off, pushed at 19:29 on 2026-11-11 once the pin's run had ended, its run green at 20:36 with
the play in 67 minutes where the pin's took 96; it keeps the form and carries the review's files.
Before it `068fd5f`, the pin of the engine's 6.2.41 with the workflows' engine checkout at
`894542da`, pushed at 15:29 on 2026-11-11, its run red at 15:34 in the play job before the game was
built, the runner not reaching its package mirror for two hours and a quarter, Verdict 11, green at
19:26 on its tenth attempt, the play in 96 minutes, which settles the verdict; its strict build is
clean and 366 tests pass. The water's mirror passes six plays of six under the layer, three from an
empty cache in 393, 417 and 434 s and three from the cache each left in 237, 242 and 221 s, each
play's caches in a folder of its own; from a cold cache the pipelines' file grows to 21.8 MB and the
pool's three terrain.slang pipelines compile in 14 to 19 s each at once, and from a warm one every
pipeline made in play says from the cache, the pool's three in 1.17 to 1.37 s each; frame 5's 22
pipelines take 54 to 68 s cold, one after another, and 4.4 to 4.6 s warm. The engine's trial puts
about 1.2 s of each such pipeline on the validation layer, 94 ms compiled without it, so these are
the layer's seconds, which the play turns on, item 171. It keeps the form and carries the review's
files. Before it `8ded865`, item 170, the local checks run through an e3d client of the pinned
engine built once in .engine and the play and the pictures stopping by its process a game left
running, pushed at 05:11 on 2026-11-11, its run green at 06:37 with the play in 86 minutes on the
runner, the mirror passing there; its strict build is clean and 366 tests pass, the play here
failing the mirror on 6.2.32 by the engine's item 253, the runner's play the gate; it keeps the form
and carries the review's files. Before it `08bf109`, item 169, the resources kept in their folders
from any path and the game stopping at its open with the path of a file of its own that is missing
or does not compile, pushed at 03:33 on 2026-11-11, its run green at 05:08 with the play in 95
minutes on the runner; 366 tests pass and the play passes from an empty cache in 463 seconds; it
keeps the form and carries the review's file. Before it `e3dea22`, the pin of the engine's 6.2.32
with item 167, the scene field laid ahead of the eye by the engine's option in place of a center set
each frame, the two copies of the sRGB curve calling the engine's and the three powers of 1/2.2
staying with their reason, the workflows' engine checkout at `02c3a743`, pushed at 02:05 on
2026-11-11, its run green at 03:32 with the play in 88 minutes on the runner; its strict build is
clean, 364 tests pass and the scripted run passes from an empty cache in 375 seconds with the
layer's check, the mirror failing from warm caches, the engine's item 253; it keeps the form and
carries the review's files. A lantern read at half its light on the bare pin was a build made
through the /home link with the resources copied flat, the engine's item 252 withdrawn and item 169
written. Before it `cb071a9`, the pin of the engine's 6.2.17, the package with the far cascade's
band weighed around the eye and every cascade built at once after a cut, the workflows' engine
checkout at `f68075f1`, pushed at 13:05 on 2026-11-10, its run green at 14:41 on its second attempt,
the macOS test job's restore having failed on the first, the package not yet at that runner's feed,
Verdict 10, the play in 91 minutes on the runner; its strict build is clean, 364 tests pass and the
scripted run passes under the validation layer, 359 seconds here where 329 on 6.2.14 and 336 on
6.2.12, one run each; it keeps the form and carries the review's file. Before it `d8ef6a7`, the pin
of 6.2.14 with the ramp, pushed at 09:29 on 2026-11-10, its run green at 11:00 with the play in 91
minutes on the runner where 70 on 6.2.12, placed at 21:23 on 2026-11-10 as the runner's spread and
not the ramp. Under lavapipe here in a small form, render distance 4 at 640 by 360 with 10 frames a
reading and each run alone under the lock, 6.2.12 and 6.2.14 twice in turn, the lit scene's pass
reads 315 and 343 ms against 347 and 344, the illumination's 446 and 428 against 448 and 430 and the
frame's work 1006 and 951 against 944 and 880, so the ramp costs lavapipe nothing that shows, the
sheet 21-23-item-168-lavapipe in the dump. In the play's logs over the same 48 steps against
6.2.12's run, two runs on 6.1.12 drew 1.24 and 1.16 times as long, 6.2.14's and 6.2.17's 1.33 each
and 5.2.333's 0.99, lavapipe drawing by the processor's vector units, which the open and the build
do not use, so an open of equal length does not rule a slower processor out. The play has 59 minutes
under its limit and nothing follows. Before it `cfffcb3`, the pin of 6.2.12 with item 165, pushed at
02:54 on 2026-11-10, its run green at 04:05 with the play in 70 minutes. Before it `4e956bc`, items
162 and 163, pushed at 08:47 on 2026-11-09, green at 10:07. The game stands at 0.2, tagged `v0.2.0`,
and is pinned to the engine's 6.2.17 since `cb071a9`; item 166's reads on it and on 6.1.12 show no
path difference and no lag at any speed, the clouds' step standing, and on the grove's shaded ground
a seventh of the pixels flickering in a turn by the screen probes on both, read again on the package
that carries the engine's ramp. Item 166 is read twice by 22:37 on 2026-11-09, turns clean at every
speed, a walk showing the difference between two settlings of one place in the screen probes' tiles,
the engine's item 246, and the clouds stepping as the far cascade moves, its item 245. Every push
and run before `4e956bc` since 2026-10-23 stands in REVIEW-ARCHIVE.md under Head records.
