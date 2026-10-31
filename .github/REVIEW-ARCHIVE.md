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
    and the faces, then the animation follow after items 60 and 61. Second commit done at `b1cba53`
    on 2026-10-23, the cutout and the furnace's faces, as the head says; the animation follows.
    Third commit done at `710fe0d` on 2026-10-23, the animation; item 39 done.

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
    `b56fa0b`. The curve at `2b265f3` on 2026-10-24, SetTonemapCurve(0.9, 1.3, 1.05, 1) with the
    pre-exposure at 1.6 for the middle levels, contrast 1.12 and saturation 1.4 kept until the owner
    chooses from the pictures, 1.2 near the look approved.

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
    before its second part. Done at `9a13c54` on 2026-10-21, as the head says, the cost unreadable
    within the sessions' swing.

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
    mobs/daylight.webp taken again. One commit, small, after item 60. Done at `92112c5` on
    2026-10-21, as the head says.

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
    layers and the sheep's fur. After items 60 and 61 and before item 39's remaining parts. First
    commit done at `e4b850f` on 2026-10-21, eight kinds wearing the pictures, as the head says; the
    villager's layers and the sheep's fur follow, with the daylight row in Bare Bones beside the own
    one. Second commit done at `9ce44d7` on 2026-10-21, the villager's biome layers as one picture a
    biome, no profession layer since the game has none, and the sheep's tinted wool; item 62 done. A
    fault found at 10:31 while item 65's layouts were checked: with Bare Bones the pig and the cow
    stood in their own colors, since Bare Bones 1.21's pictures of them are 64 by 32 where the 26.3
    client's layers lay their cuboids on 64 by 64, the source having doubled the two pictures at
    1.21.5 and given the cow a muzzle cuboid at row 33, so the fit refused both and the captures'
    pink and brown were read as the pack's; the older layout matches the newer in every offset but
    the muzzle. The mend, a small commit before item 65: a picture fits when it is as tall as its
    layout or half as tall at a whole scale, the texture coordinates taken over the picture's own
    rows and one white row, a cuboid all of whose faces lie below the picture left out as the
    clear-face rule already leaves it, so a cow on an older picture loses its muzzle alone, as the
    source's cow had none before 1.21.5; tests of a 64 by 32 cow fitting the layout, its head laying
    the head and horns but not the muzzle, its coordinates over 33 rows, and mobs/daylight-pack.webp
    taken again. Mended at `d2bc3f0` on 2026-10-21, as the head says.

63. **A mob's face is lit evenly from top to bottom, the turned-up light seen from straight above.**
    The owner wrote shortly before 09:50 on 2026-10-21 of item 61's noon picture, relayed verbatim,
    "The provided images of the 3 mobs it shows at some height the entity gets darker about the
    height of he height above hevlips ofbthe uombie". The cause, confirmed live and older than
    `92112c5`: down the zombie's face at noon the floor reads 134.7 above a height and 150.3 below
    it, an even 134.7 with the floor removed, the boundary at the eye's height of 5.72 wherever the
    eye stands and whatever the sample's distance out, the whole face 150 with the eye raised 0.3
    blocks, since the engine's surfaceNormal turns a normal pointing away from the eye over as for
    the back of a double-sided face, so the turned-up light taken above the eye's height is lit as a
    face turned down and the floor falls under the face's own light there. The mend: one line in
    mob.slang, the turned-up light's view direction straight down from above, the face reading an
    even 150.3 to 150.7 from the same eye height; a play-script check reading down a zombie's face
    at noon from its own height asking no step over 2 levels outside the mouth's rows, which fails
    today with a step of 15.6. One commit, small, before item 64. Done at `e6a2c55` on 2026-10-21,
    as the head says.

64. **The zombie's own face is the source's archetype and the villager's eyes sit lower (Decision
    31).** The owner wrote shortly before 09:50 on 2026-10-21, relayed verbatim, "And change the
    default native face of the zombie to be the same archtype default like in minecraft with
    horizotnal lines and a nose but no mouth and the villagers eyes lower a bit." The game's own
    art, worn where no pack gives a picture: the zombie's eyes as horizontal dark lines, a nose and
    no mouth, as the source's default zombie face is read, and the villager's eyes a little lower;
    the pair sheets of the two taken again. One commit, small, after item 63 and before item 62, so
    item 62 keeps these faces for a game with no pack. Done at `69ecb49` on 2026-10-21, as the head
    says.

65. **The Bare Bones x Fresh Animations pack as a fourth default pack above Bare Bones, so the mobs
    wear its pictures (Decision 32).** The owner wrote shortly before 10:15 on 2026-10-21, relayed
    verbatim, "I have a mod that replaces the creeper skin for bare bones. Check". Read in the
    modpack: its pack order from lowest to highest is Bare Bones 1.21, Fresh Animations 1.9.2 and
    Bare Bones x Fresh Animations 1.9, so the creeper the owner sees is the last pack's, Bare Bones'
    creeper with a darker face plus Fresh Animations' pieces in parts of the picture the source's
    cuboids never read, 64 by 32 in the vanilla layout; Entity Model Features draws Fresh
    Animations' animated models, which the game does not draw, and Creeper Overhaul gives each biome
    its own creeper in its own style and namespace, which no pack restyles and which stays out. On
    Modrinth the pack is bare-bones-x-fresh-animations by TeamHibiscus under the Unlicense. The
    mend, after item 62: the pack as a fourth default fetched as the three are under Decision 18
    with its version and SHA-512 in resources/packs.json, listed above Bare Bones so its mob
    pictures win as in the owner's order, the game reading a picture from the first pack that has
    one; first each of the nine kinds' pictures in it checked for the vanilla layout as the
    creeper's is, a kind whose picture departs from it kept on Bare Bones' and named in the commit,
    and a capture of the mobs with and without the pack beside the reading. One commit, small, after
    item 62's second commit and before item 39's remaining parts. Read at 10:31: Fresh Animations
    1.9.2 is the release whose creeper and cow are byte for byte the owner's 1.9 beta, its pig and
    cow 64 by 32 as Bare Bones 1.21's, its sheep, chicken, villager, creeper and spider covering
    every face the cuboids read as Bare Bones' do, and it has no zombie or skeleton, which stay on
    Bare Bones'; after item 62's mend of the half-height fit. Dropped at 10:45 on the check: the
    release 1.9.2, whose creeper and cow are byte for byte the owner's beta, compared pixel by pixel
    with Bare Bones 1.21 over every face the cuboids read gives the chicken and the spider alike on
    every face, and differs on the pig's, cow's, sheep's and villager's head fronts and the
    villager's nose alone, the differences being the eyes, which Fresh Animations takes off the face
    onto pieces of its animated model elsewhere on the picture, and a clear hole where the cow's and
    sheep's muzzles meet the face; captures of the six with and without the pack show the villagers
    and the pig without eyes and the rest unchanged, the creeper keeping its own colors by item 66
    and the zombie and skeleton absent from the release. So by the item's rule four kinds stay on
    Bare Bones' and two take pictures identical to Bare Bones' on every face read, the pack changing
    nothing the game shows at the cost of a fourth fetch, and nothing is committed, the working
    tree's edits reverted with the fetched copy kept outside git.

66. **The creeper keeps the game's own vanilla-style art with any pack (Decision 33).** The owner
    wrote shortly before 10:38 on 2026-10-21, relayed verbatim, "I dont like the creeper texture in
    barebones. Keep the vanilla". The repository cannot hold the source's own creeper picture, and
    the game's own creeper is drawn after it with its patches and its face, so the creeper is taken
    out of the pack table with the reason in its comment and wears the game's own art whatever packs
    are loaded; the play script's picture check moves to the zombie, whose head at 0, 0 and body at
    16, 16 take the same offsets on one 64 by 32 picture that the half-height rule lays on its
    layout; the README's sentence on which kinds wear pictures, and mobs/daylight-pack.webp taken
    again. One commit, small, before item 65, whose layout check then leaves the creeper out. Done
    at `f45b75e` on 2026-10-21, as the head says.

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

71. **The stars are points of light with no dark backing, faded by the sky's light.** The owner
    wrote on 2026-10-23 (Decision 34) that the stars are visible at dusk and dawn with half of them
    black, and at night some black or dark. The cause to read: a star drawn as a quad whose dark
    corners or backing write over the sky, or blended with the wrong operator. The mend: stars drawn
    additive or with an alpha that is zero off the point, their brightness scaled by the sky's
    darkness so none shows by day and all show at night; a test reading the sky around a star for no
    pixel darker than the sky at dusk. One commit, small. Done at `fd276d3` on 2026-10-23, additive
    stars, as the head says.

76. **The stepped shadows move in whole steps as the day passes.** The owner wrote on 2026-10-23
    (Decision 34) that when timing the day the pixelated shadows flicker, unable to choose between
    staying and moving to their next place. The cause: the sun's direction moves every frame, so a
    shadow edge near a step's boundary falls on either side from frame to frame, the engine's texel
    snapping holding the map against the camera's motion and not against the light's. The mend: the
    shadow's sun direction steps on a coarse clock, every few game minutes, while the sky's sun
    moves smoothly, so an edge jumps a step at once and stands still between, the step chosen so no
    jump is longer than a step at the shadow's reach; a test reading a shadow edge still across the
    frames between two steps. One commit, small. Done at `7a0d61d` on 2026-10-24, 36 steps an hour
    rounded to the nearest, the departures in the head.

77. **The distance fog takes the sky's color at the ray's height, so a mountain's top is not the
    horizon's color.** The owner wrote on 2026-10-23 (Decision 34) that with the chunk-distance fog
    on mountains the color is the horizon's applied to blocks near the sky, asking for a
    height-based color or the modpack's way. The mend: the fog's color for a pixel is the sky drawn
    behind it, read along the view ray's elevation from the sky's gradient, so a block high against
    the sky fades into that sky's blue and one at the horizon into the horizon's haze, as the
    modpack's shader does; the haze of item 58 keeping its share. A test reading a far block high in
    view against the sky's pixel beside it. One commit, small. Done at `532e45c` on 2026-10-23, in
    the shader from the sky map, as the head says.

82. **No plant stands in the air where a cave was carved.** The owner wrote on 2026-10-23 (Decision
    34) that grass is placed in the air where a cave was carved at a later stage. The mend: the
    carving before the decoration in the chunk's stages, as the source orders them, or every plant
    placed re-checked for the block under it after carving; a test of a carved chunk holding no
    plant over air. One commit, small. Done at `f9ab3bb` on 2026-10-23, as the head says.

84. **A loading screen while the world generates.** The owner wrote on 2026-10-23 (Decision 34) that
    the beginning should show a loading screen for the world generation, the window being blank
    while the world loads in for a second. The mend: a screen with the world's name, a progress bar
    over the sections generated and meshed around the player and the pack's fetch, shown until the
    player's column and its neighbors stand, then the world shown; a test reading the screen's
    pixels in the first frames and the world's after. One commit, small. Done at `a9b0fcf` on
    2026-10-23, the departures in the head.

85. **No line on a grass plant's top.** The owner wrote on 2026-10-23 (Decision 34) that a thin line
    of the grass plane's pixels shows on the top of it, a faint cross at the top of some. The cause
    to read: the crossed quads reaching a texel beyond their picture in the atlas or the atlas's
    padding, so the row above bleeds in. The mend: the quads' coordinates inset by half a texel or
    the atlas padded with each tile's edge; a test reading the pixels above a grass plant's picture
    for the sky alone. One commit, small. Done at `2f9ed14` on 2026-10-23, in shader form, as the
    head says.

86. **Glass and water reflect nothing for the present (Decision 36).** The owner wrote on 2026-10-23
    (Decision 34) that the reflections on glass and water show the scene field, clashing with the
    game's voxel style, and should stay non-reflective for the time being. The mend: the glass's and
    water's materials set with no reflection, the water keeping its tint and transparency, the
    field's reflection left off until the owner asks; a test reading a water pixel for the sky's
    color alone. One commit, small. Done at `10cf3f3` on 2026-10-23, by roughness since the package
    has no switch, as the head says.

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

104. **The sun's map and the field see every section the eye could, whichever way it looks.** The
    owner, playing in a cave at about 10:5x on 2026-10-24, looked down and saw the ceiling go, blue
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

103. **The flames' top row is the picture's own, with no stair-stepped line.** The owner played at
    about 10:55 on 2026-10-24 and saw at the top of a fire a line one pixel wide with stair-stepped
    edges, a texture coordinate that is not pixel-exact the likely cause. The flames' pictures are
    the pack's animated fire, frames stacked in one image, and the source's fire models stretch
    their faces over a frame; a coordinate reaching the frame's edge reads the next frame's bottom
    row or the atlas's neighbor through the filter, so the row shows as a line that steps with the
    face's slant. The mend by the reading, the frame's coordinates inset by half a texel, or the
    frame sampled nearest and clamped within its own rows, as the terrain's tiles are kept within
    theirs; a test reading the flame's top row against the frame's own and a capture of a fire at a
    slant. One commit, small, after item 102. Done at `3236b56` on 2026-10-24, the carried place
    clamped half a texel of the sampled level inside its tile, the check in play.sh reading the
    north wall's top edge against the ground beyond.

93. **The frame's cost on the development machine with the taller world, read and brought down.**
    The owner played on 2026-10-24 and found the performance bad. Read first at the owner's
    settings, the render distance of 12, High and the three packs, at the cherry grove and the
    plains village with the engine's profile: the GPU's passes, the scene pass, the window depth,
    the shadows, the global illumination and the rest, and the main thread's parts, with the deep
    sections' share, the aquifers' and the caves' meshing, the screen probes off; say the numbers
    and what each would give back. Then the mends by the reading, in order of what they give for
    what they cost, each a commit with its before and after: the sun's cascades' count and the
    shadow distance, the deep sections drawn only where a cave opens to the eye's reach, the meshing
    of sections the eye cannot see held back, and whatever else the reading names; the frame held
    under 16.7 ms at 12 on the 4070 as item 90's draw test holds the draws. After item 92, before
    item 27's part C. The reading at 11:18 on 2026-10-24, Release, offscreen, the three packs, 12,
    High, the probes off, 300 frames, the cherry grove and the plains village at hour 16. At 1280 by
    720 the frame is 14.9 and 15.3 ms, the GPU's scene pass 8.3 to 9.6, the global illumination 2.1
    to 2.6, the shadows 1.8 to 2.3 and the window depth 1.5, the graph's recording 8.9 to 9.9 ms of
    CPU with the game's drawing 1.6, 1,683 draws and 1.18 million triangles; at 1920 by 1080 the
    frame is 19.0 and 19.2, the scene pass 12.2 to 13.3 and the rest unchanged, so the GPU and its
    scene pass bound the frame. With global illumination off the frame is 11.9 against 18.6 at
    1080p, 2.9 of it the pass and 5.6 the cascades read per pixel inside the scene pass, which is
    the engine's. The owner's build is Debug through dotnet run, standing still 13.4 to 13.7 ms, in
    a sprinting flight over unloaded land 17.7 to 26.1 with the game's update at 4.2 to 7.8 ms of
    joining and meshing, where Release reads 11.6 to 17.8 with the update at 1.0 to 1.3. The mends
    by gain for cost, each with its before and after when the GPU is quiet, are the terrain shader's
    stepped light reading the sun's shadow once per pixel rather than twice, then the shadow
    distance from 96 toward 64 or 48, about 1 to 1.4 ms of GPU at 48 with the shadow line nearer,
    then the deep sections drawn only where a cave opens within reach, about 0.6 ms of CPU for a
    hundred draws fewer and the GPU's share read then, then the meshing of unseen sections held
    back, the flight's update; and the engine's per-pixel read of the cascades in the scene pass,
    5.6 ms at 1080p, goes to the engine as an entry in its ASKS.md. The owner runs Release, dotnet
    run -c Release or a published build, which the record says to them. The second mend at
    `89d8caa`, the shadow distance 64, about a millisecond at 1920 by 1080, the village's trees'
    shadows kept where 48 loses them; the third at `08d0943` a counter alone, the flood already
    drawing a deep section only where the eye's open cells join it, 3.5 to 10 percent of the
    triangles, a cut by distance hiding cave interiors seen through their mouths; the fourth at
    `4a2cd7b`, the unseen sections' meshing held back, the flight's update 0.7 to 0.9 ms where 1.3
    stood. Done at `4a2cd7b` on 2026-10-24, the cherry grove's frame at 1920 by 1080 about 16.8 ms
    where 17.8 stood before the mends and 14 to 15 at 1280 by 720, the first mend's one read with
    the package carrying the engine's item 99 as item 105.

99. **No tree stands cut by a cave or a cliff, and no leaves hang with no trunk.** The owner played
    at about 10:51 on 2026-10-24 and saw trees cut off where caves and cliffs carved the terrain
    after the tree was placed, some leaves staying in the air, as the grass over carved caves did
    before item 82. The source carves before it places its features, so the caves of item 27, the
    entrances among them, and the cliffs' cuts run before the trees and the plants in the column's
    pipeline, the neighbors a tree spills into carved first as well; and a tree whose base would
    stand on air or whose trunk a cut would cross is not placed there. Tests, over a seeded region,
    that no log has air under the trunk's base, that no leaf stands with no log within the crown's
    reach, and that the trees' count stays within the source's range for the biome, failing without
    the mend; a capture of a cliff's edge and a cave's mouth under trees. One commit, after item 98
    and before item 94. Done at `dc2ad9f` on 2026-10-24, the departure in the head.

100. **Felling takes every log of the tree, and the crown decays within a second or two.** The owner
    felled trees at about {now} on 2026-10-24 and found it working but for one tree whose single log
    stayed disconnected, its leaves hanging until that log was broken by hand, and found the leaves
    decaying too slowly. So the felling's flood follows a log to every one of its 26 neighbors, the
    corners across two axes among them, as the branch logs of the bigger trees stand, and counts the
    trunk's logs under the cut as the tree's where the cut was made above the base; and the crown's
    leaves decay each at its own time within one to two seconds of the last log, where item 69
    spread them over half a second to four. Tests, over the game's tree kinds, that felling leaves
    no log and no leaf within the crown's reach, and that the last leaf goes within two seconds; the
    play check as it stands. One commit, small, after item 99. Done at `54fc848` on 2026-10-24, the
    cause two trees joined through their branches, the departure in the head.

101. **Grass and the other plants give way to a block placed where they stand.** The owner placed
    blocks at about {now} on 2026-10-24 and found that aiming at a plant, the plant highlighted,
    placed the block in front of it rather than in its place, and that a block placed beside another
    was refused where a plant stood in the cell. The source's plants are replaceable, so a placement
    whose cell holds one of BlockGroups.GivesWay, short and tall grass, ferns, dead bushes and their
    kin, replaces the plant, and aiming at such a plant places the block in the plant's own cell
    rather than on its face; the plant drops nothing. Tests of a block placed on a plant's face
    landing in the plant's cell and of a block placed beside another into a cell a plant holds; the
    play check as it stands. One commit, small, after item 100. Done at `ad3499f` on 2026-10-24, the
    play check in place of a unit test since none builds a game.

102. **A held light shines from the player, not from the hand.** The owner played at about 10:53 on
    2026-10-24 and found the held light standing at the hand, a third of a block ahead of the eye
    and to the right, where it clips into a wall the player faces and turns with the camera, so the
    light about the player swings as the eye turns. Item 74's light moves to the player's own place,
    the body's middle at the eye's height, so the camera's turn has no part in it and no wall takes
    it inside; its color, strength and range stay as item 74 set them. A test that the ground's
    light about the player reads the same at four headings with a torch held, and the play check of
    the lamp room's roof as it stands. One commit, small, after item 101. Done at `1f07038` on
    2026-10-24, the light at the eye's place in the body's middle.

98. **The particles keep their light at every angle of looking.** The owner played at about 10:51 on
    2026-10-24 and saw the particles showing only the ambient color at some angles of looking. Read
    first which particles, the game's own bits cut from a block's side, the engine's particle sheets
    of the blasts' clouds, or the flames, and at which angles between the eye, the sun and the
    billboard, with the readings of one particle from eight headings around it under the sun; then
    the mend where the cause lies. A billboard's face follows the eye, so a lighting that takes its
    normal against the sun goes dark where the sun stands behind the eye's side of it, and the right
    light for a particle is the sun's reaching its center through the shadow map with no normal
    term, or the normal's dot taken on both sides, with the probes' light gathered about its center;
    where the cause is the game's bits shader the game mends it, and where it is the engine's
    particle sheet the gap goes into the engine's ASKS.md as an entry and the game waits on the
    package. A test reading the same particle from eight headings within a few levels of each other.
    One commit, after item 93. Done at `56cb608` on 2026-10-24, the bits lit as a face turned up,
    the clouds and the flames not it. Reopened at 13:21 on 2026-10-24 on the owner's word, who did
    not mean the bits lit as a face turned up and has that reverted; the fault is a broken block's
    bits dropping with a part clipping into the wall beside them, as in a tunnel two high and one
    wide dug into a hill, and taking the sun's or the sky's light there. The mend by the reading, a
    bit's light sampled at the open cell it came from or the air it falls through, a bit whose
    center enters a block keeping the last open cell's light, so no sun or sky reaches a bit through
    a wall; the turned-up face taken out; a test breaking a block at the end of such a tunnel under
    the sun and reading the bits beside the wall within a few levels of the tunnel's floor. The
    owner added at 13:21 that the turned-up face makes the bits dropping from a block above dark
    where the face toward the eye says they take the light from the eye's side, so the bits'
    lighting before `56cb608` is read first and put back, and the mend for the clipping is built on
    it. One commit, first, before item 94. Done again at `ccf6fcf` on 2026-10-24, the engine's
    lighting back with the light read at the bit's middle, the tunnel's bits within six levels of
    one another.

106. **The corner shade stands at a half by default.** The owner wrote at 13:23 on 2026-10-24 that
    the corner shade should be 0.5 by default; `SectionMesher.CornerStrength` starts at 1, the whole
    of the occlusion's darkening at a block's corners. The default becomes 0.5, the settings
    window's slider, the menu's and `voxel.corners` unchanged, a saved setting still read over it;
    the README's line on how far corners darken says the half, and the mesher's test of the default
    reads it. One commit, small, after item 98 and before item 94. Done at `e82d965` on 2026-10-24,
    the default a half, no saved setting since the game saves none.

107. **The rain forest's floor is shaded, not dark, the canopy taking its levels at its edges.** The
    owner wrote at 13:26 on 2026-10-24 that the rain forest is too dark in the middle and asked for
    a better balance, with the example that the light level should not drop by one with each block
    of leaves but at the way in and the way out, a canopy five deep dropping two and the blocks
    between ignored. `Lighting.cs` takes one level for each block of leaves, the way down too, so
    the rain forest's layered canopy darkens its floor as a cave is darkened. The mend by the
    owner's example or a better balance the coder finds and says in the commit, a leaf lit from a
    leaf taking nothing and the canopy's outer layers taking the levels, so a canopy five deep takes
    two, one deep still one, sideways spread under it as before; a capture of a rain forest's floor
    at noon before and after, the forests and the other biomes read for no change beyond the
    canopies, and a test of a column under five leaves reading two levels under the open sky's and
    under one leaf reading one. One commit, after item 106 and before item 94. Done at `aaeaa2a` on
    2026-10-24, the levels' balance in; the floor's darkness found to be the engine's light under a
    canopy the field holds solid, taken on as item 117 and an engine entry.

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

108. **The clouds reach the camera's far plane, fading into the sky there.** The owner wrote at
    13:29 on 2026-10-24 that the clouds still have a limit, asking whether the scene field's last
    cascade reaches the far plane as the lowest level of detail, which the engine's item 72 gave it
    on its main and the package after 5.2.215 carries. The clouds are the game's own, `Clouds.cs`
    building the cells within twice the render distance of the eye and fading them into the sky from
    six tenths of that reach, 384 blocks at 12, so their edge shows under a high sky where the
    land's does not. The clouds reach the camera's far plane, the near cells as boxes as they are
    and the cells beyond the present reach as the flat layer alone where their sides are not seen,
    the fade into the sky at the far plane; the drift, the night's darkening and the capture's hold
    unchanged; the clouds' cost read before and after at 1920 by 1080 with the cells' count, and a
    capture of the horizon under clouds at noon. One commit, after item 107 and before item 94.
    Closed at 14:49 on 2026-10-24 on the owner's word to the game's session at about 15:00, Decision
    44, the clouds not stretched so far, the before images right where they covered the area about
    the player and did not reach the horizon; what was built, the boxes within twice the render
    distance and the flat layer out to the haze dome at 3,576, 18,131 faces against 2,723 and about
    1.05 ms of the scene pass against 0.4 at 1920 by 1080, reverted uncommitted, its play check on a
    cloud's underside red besides.

109. **The cactus is the source's model, its sides a pixel in and their clear edges cut out so the
    thorns stick out.** The owner wrote at 13:31 on 2026-10-24 that the cactus needs to be one pixel
    thinner, white showing in its corners, and at 13:46 that the thorns are on the white border, so
    the faces go a pixel in and the white is cut out as alpha so the thorns at the edges stick out.
    The game draws the cactus as a whole cube whose texture's clear edges show the color the pixel
    holds, the TODO line on the source's model open. The cactus becomes the source's model, each
    side face standing a pixel in from its side of the cell and spanning the cell's full width, the
    clear pixels cut out as the plants' are so the thorns on the picture's edge columns stick out
    past the face beside them, the top and the bottom on the inset square, the top at the height the
    source's model gives it, the bottom on the ground, with the picture's rows and columns as the
    source maps them; its collision box the source's; a capture of a cactus up close against the
    before, and the mesher's test of the cactus's faces. One commit, small, after item 108 and
    before item 94. Done at `02abb9d` on 2026-10-24, the source's model with its two departures in
    the head.

110. **The bits of a tinted block carry the biome's tint.** The owner wrote at 13:31 on 2026-10-24
    that the color of the particles for grass and dirt is not adjusted to the temperature of the
    biome, breaking grass in the savanna dropping pure green bits and not the savanna's color. The
    bits cut from a block's picture take the tint the block is drawn with at its place, the grass
    block's top and its side's overlay, the short grass, the ferns and the tall plants by the
    biome's grass color, the leaves by its foliage color, the untinted blocks as they are, dirt
    among them; a test breaking a grass block in the savanna and in the plains reading the bits'
    colors as the blocks' tops read there, within a few levels, and a capture of each. One commit,
    small, after item 109 and before item 94. Done at `2a399af` on 2026-10-24, each bit carrying its
    climate, the test on the savanna against the plains.

111. **A felled tree's crown drops as its logs fall, not after they land.** The owner wrote at 13:33
    on 2026-10-24 that breaking a tree with the axe drops the logs first and the leaves only once
    the logs have fallen, and that both should happen at the same time. Felling starts the crown's
    decay and the leaves' drop in the same tick as the logs begin to fall, the crown's pace of item
    100 kept, so the leaves and the logs are in the air together; the felling test reads the first
    leaf gone within the tick of the first log's fall, and a capture of a felled tree in the air.
    One commit, small, after item 110 and before item 94. Done at `f00f649` on 2026-10-24, the crown
    read at once and its leaves going from the first log's tick.

112. **Felling wears the axe by the logs it drops as items, the falling logs not counted.** The
    owner wrote at 13:36 on 2026-10-24 that the axe's use should drop by the number of logs felling
    breaks, a tree of eight logs taking eight, and where three of the eight drop as items and the
    rest fall as blocks, three, the falling logs not counted. Felling takes one use of the axe for
    each log it turns into a dropped item, none for a log that falls as a block, and the axe that
    runs out mid-tree breaks the logs it had uses for and leaves the rest standing or falling as the
    source does; the felling test reads the axe's uses after a tree of known logs with a known share
    falling, and the README's felling paragraph says the wear. One commit, small, after item 111 and
    before item 94. Done at `fd17371` on 2026-10-24, one use a dropped log and none a falling one,
    the felling stopping with the axe, the departure in the head.

113. **Pyramids are as rare as the source's.** The owner wrote at 13:36 on 2026-10-24 that there are
    far too many pyramids. The desert pyramid is placed at the source's spacing, one at most in each
    region of 32 columns a side with a separation of 8, the region's one place drawn from the seed,
    and only where the desert holds it, so a desert of the owner's size shows one or none where it
    shows several; a test counting the pyramids over a seeded span of desert against the source's
    bound, and the README's line on the structures saying the rarity. One commit, small, after item
    112 and before item 94. Done at `031d169` on 2026-10-24, 6 pyramids on seed 1's 8,192-block
    square where 128 stood.

115. **A wall torch's wood leans up to its head's foot, and the lit head alone stands straight.**
    The owner wrote at 13:41 on 2026-10-24 that the torch on a wall goes straight a pixel too early,
    the wood bending to straight where only the lit part should stand straight up. The wall torch is
    a leaning stick piece, its top's corners moved out by the lean, and a lit head piece standing
    straight on it; the stick's lean reaches the head's foot, the head the picture's own two rows of
    coal and nothing of the wood, one pixel higher than it stands, read against the source's torch
    picture and its wall torch model; a capture of a wall torch up close before and after, and the
    mesher's test of the wall torch's pieces reading the stick's top at the head's foot. One commit,
    small, after item 114 and before item 94. Done at `65eec11` on 2026-10-24, the split at row 8,
    the head's glow scaled so the light stays.

116. **A held axe faces the right way, its blade forward.** The owner wrote at 13:44 on 2026-10-24
    that the axe is backwards when held. The held tools are the source's item pictures stood up in
    the hand, and the axe's blade points back toward the player where the source holds it forward;
    the tool's picture in the hand is turned as the source's first-person item display turns it, the
    blade and the pick's head forward and the handle toward the eye, read for the pickaxe, the
    shovel, the hoe and the sword as well so each matches the source; a capture of each tool held
    before and after, and the test of the held item's placement reading the axe's blade ahead of its
    handle. One commit, small, after item 115 and before item 94. Done at `cce1536` on 2026-10-24, a
    tool's picture turned by a half about its handle's diagonal before the quarter turn, the
    departure on the source's sign in the head.

118. **A held tool's pixels meet with no gap along its borders.** The owner wrote at 16:39 on
    2026-10-24 that the held tool items show a pixel gap everywhere around the borders,
    stair-stepped aliasing. The held picture is a block of pixel cubes cut from the item's picture,
    and along its outline a gap shows between the border pixels and the body, or the background
    through the border's edge. Read first where the gap is, in a capture of a held tool at four
    times the size against the picture's pixels, whether the cutout's alpha test drops the border
    texels where the filter blends them with the clear ones beside, the cubes' faces standing a half
    texel off their pixels, the atlas's padding, or the depth of the side faces against the front;
    then the mend at the cause, the picture sampled nearest with the texel's own color at every cube
    and the cubes meeting edge to edge; a test reading a held tool's capture for no background pixel
    inside its outline, and the capture before and after. The owner added at 16:40 that the tool
    jiggles as the camera moves, which closes and opens the gaps at different places, so the gaps
    move with the subpixel position, as cracks do between faces whose shared edges are not the same
    floating-point vertices, each cube's corners worked out on their own, or as a front face and its
    side faces fight in depth along their edge; the reading takes this first, and the mend then has
    every cube take its corners from one grid of vertices indexed by the pixel's corner so neighbors
    share them bit for bit, the front faces merged along runs of pixels, and the item drawn without
    sway for the capture to tell the sway from the cracks. One commit, first, before item 94's
    pieces. Done at `9a3d746` on 2026-10-24, each pixel its own faces from one grid of corners, the
    cracks read as the front's cut edge against the sides' geometry, the departure in the head.

119. **The terrain's faces meet with no crack, so no light shines through a dark cave's walls in
    pixel-wide lines.** The owner wrote at 16:41 on 2026-10-24 that blocks show the same pixel-wide
    gap as the held tools, and that it is the complaint from the black cave, the one-pixel
    stair-stepped lines of light where light shines through, read at 11:0x as item 83's missing mend
    in the engine. The owner added at 16:42 that the gap is not everywhere but on a lot of edges,
    which fits cracks where quads of different sizes abut and not a seam at every block. The game's
    mesher is not greedy, each block face its own quad with corners on the block grid, so the lines
    are not T-junction cracks of merged quads; the cause is read, the faces' shared corners not the
    same values, a face's edge against a neighbor's at a different depth, or the shadow map's. Read
    first on 5.2.281 under item 27, since the engine's item 83 mend was for these lines, in a dark
    cave at a fixed view with the sun's shadow off and with it on, and with the camera moved by a
    fraction of a pixel, whether the lines are cracks of the mesh, which stay with the shadow off
    and move with the camera, or the shadow map's, which go with the shadow off, saying which with a
    capture; then the mend for the cracks, at the cause the reading finds, said in the commit with
    the draw counts before and after where the mesh changes; a mesher test over a sample section
    reading no vertex on the inside of another face's edge, and the cave capture before and after.
    The shadow map's share, where the reading finds one, goes to the engine's ASKS.md. One commit,
    after item 27's package pin and before item 120. Done at `ffa7a7b` on 2026-10-24 as a reading,
    the lines the sun's map's and not the mesh's, the mesher's test added, the map's share the
    engine's item 101 through ASKS.md.

120. **The selected item's name over the hotbar fades away.** The owner wrote at 16:44 on 2026-10-24
    that selecting an item from the hotbar shows its name, which should fade away and not stick
    forever. The name shows when the selection changes and fades out as the source's does, held
    about a second and a half and faded over about half a second, shown again on the next change,
    and the HUD's own fade of full rows unchanged; a test reading the name's alpha over the frames
    after a selection, full, then gone, and the play check's HUD capture read for no name after the
    hold. One commit, small, after item 119 and before item 94's pieces. Done at `7ece2c1` on
    2026-10-24, held 1.5 s and faded over 0.5 as the source's.

121. **A shadow darkens the ground and keeps its warmth, the sky's blue out of it.** The owner wrote
    at 18:09 on 2026-10-24, with a capture of the plains village from above, that the shadow of the
    near tree is very blue while the far shadows, faded by the cascades, read as a darkening that
    keeps the warmer colors, which is easier on the eyes in the warm setting, and asked whether that
    look can be had everywhere, assuming the blue is the ambient color the cascades take from the
    sky. Read first what lights a shadow at noon, the environment's irradiance the game feeds the
    engine for the hour and the probes' share of sky, each read alone on the village's tree, with
    the numbers; then the mend, the light the game feeds for the shadows drawn toward neutral at the
    hour's brightness, the dome's blue untouched, so a shadow is the ground darker with its own
    color, as the source's are, and the probes' sky share following it where the engine takes it
    from the same color, or an ASKS.md entry where the engine's sky share has no such control; the
    night's moonlight kept as it is; captures of the village's tree at noon, the cherry grove and
    the rain forest's floor before and after, and the play checks' readings held within their
    bounds. One commit, first, before item 94's pieces. Done at `4142a32` on 2026-10-24, the
    environment's light drawn toward white by day through SetEnvironmentLight, the owner finding the
    after images nice, the departure on the sky's reflections in the head.

122. **The overworld's loading screen is the red of the source's loading overlay.** The owner wrote
    to the game's session at about 18:4x on 2026-10-24 to make the loading overworld screen the red
    from the source's logo, a nice color. The README says a world opens behind a loading screen in
    that red already, so the screen the owner means is the one shown while the overworld loads that
    is not, read from the game and said in the commit. Its ground becomes the source's loading
    overlay red, 239, 50, 61, the bar and the words on it readable against it as the source's are,
    the other screens unchanged; a test reading the overlay's color and a capture. One commit,
    small, before item 94's pieces. Done at `e04a1a5` on 2026-10-24, the one loading screen, dark
    gray before, in the source's red.

125. **The loading screen is pure black.** The owner wrote to the game's session at about 18:5x on
    2026-10-24 that the red loading screen is too neon and to make it pure black.
    VoxelGame.LoadingBackground becomes 0, 0, 0, the bar's empty part with it, the title and the bar
    reading clearly, and the play check's corner reads black; the README's line follows. One commit,
    small, after item 27's pin of 5.2.283 and before item 124. Done at `e643539` on 2026-10-24.

124. **The settings live in the game's own screens, by area, and F3 keeps only what is read.** The
    owner wrote to the game's session at about 18:5x on 2026-10-24 to convert the settings from F3
    to the in-game UI, where further things can be set, as a render settings screen holding the
    global illumination's settings, a post processing screen, and sky and visuals, the sun's color
    at noon and the like where it is not hardcoded, since F3 becomes the information screen and not
    a settings screen later. The game's options, reached from the pause menu and the title as the
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
    The owner wrote to the game's session at about 19:2x on 2026-10-24 that video settings are
    needed too, for the resolution and the window mode. The screen, beside the others under Options,
    holds the resolution from the monitor's modes through GetMonitorModes, the window mode as
    windowed, fullscreen or borderless through the engine's SetWindowFullscreenMode and
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

70. **The land's light is the same above the eye's height as below it.** The owner wrote on
    2026-10-23 (Decision 34) of a visual fault with the height, the land lighter below a line and
    darker above, related to the horizon, the top of it taking the dirt texture's color with the
    grass about orange or more teal, nearly black at points and flickering as the owner moves, which
    the owner read as the same fault the mobs had from the lips up. The cause is read as item 63's:
    the engine's model pass turns a normal that points away from the eye over, so a sample the
    terrain shader takes with a turned-up normal above the eye is lit as a face turned down, and the
    overlay's choice of the grass top's color over the dirt side may ride on the same normal. The
    mend: every sample the terrain and plant shaders take with a normal of their own sees it from
    straight above or along the face as item 63's does, and the overlay's color is chosen by the
    face's own side; a play-script check reading a grass wall's side from the player's height across
    the eye's line for no step over 2 levels, and the plains village picture taken again. One
    commit, small, first of the owner's list; the engine's item 82 removes the trap. Done at
    `22616f4` on 2026-10-23, the plants' walls, as the head says; the land's own faces were never
    turned. Done at `8bcbfde` on 2026-10-24 as a reading, the land's faces showing no step across
    the eye's height, the grass wall check added and the village capture taken again; the dirt's
    crushed blue read under item 43.

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
    height and the captures at three hours. The reading at 22:29 on 2026-10-24, a dirt wall at noon
    with the packs, the same texel with one step of the frame taken out after another, the pack's
    dirt averaging 155, 101, 49 and drawn 190, 105, 0, saturation 1 giving 174, 111, 46, contrast 1
    then 169, 113, 55, the engine's curve 205, 127, 59, a white noon sun 205, 138, 73, exposure 1
    166, 111, 58 and no global illumination 174, 108, 59, so the crush is the grading's saturation
    of 1.4 pushing the dirt's low blue under 0, the rest moving it little; the village at noon and
    17 at 1.4 and 1.2 sent to the owner, the lower half's blue averaging 2.7 and 1.4 at 1.4 against
    41.7 and 14.8 at 1.2, the default changing on the owner's word alone. The package has no
    soft-shadow call, so the penumbra widening with the distance from the caster goes to the engine
    as its item 111 through ASKS.md; the haze grows by distance alone in the terrain shader and
    gains the haze by height and its thickening toward dawn, each a setting under Sky and Visuals on
    by default; captures at dawn, noon and dusk of one view, tests of the sun's color at four hours
    and the haze's density by distance and height through a twin of the shader's formula. Done at
    `4b398bd` on 2026-10-24 but for the penumbra, the engine's item 111, and the saturation default
    on the owner's word, the haze by height and toward dawn with their settings and the sun's color
    read at four hours.

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
    sandstone join as they do in the owner's instance; the atlas takes the tiles as it takes the
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

131. **The screen probes are on, with no setting for a player.** From the owner's word to the
    engine's session before 09:19 on 2026-10-25, Decision 48, that the screen probes stay on and
    that off is for debugging alone. LightSettings.ScreenProbes is true by default and the Screen
    Probes toggle leaves the options, a saved options file carrying the old value read as on; the
    two calls to SetGlobalIlluminationScreenProbes stay while the engine keeps the call and go in
    the commit that takes the package without it, if the owner has it go. A capture of the plains
    village at noon with the probes on, N 4.8, and the play run reading the probes on. One commit,
    in the gap before the package carrying item 108. Settled further at 09:39 under the engine's
    Decision 72 as amended, the engine keying the screen probes to its quality, left out at Low with
    the glow lights through the glow-only pass and present at Medium and High, so once the package
    carrying its item 103 comes the game's probes follow its global illumination quality row and the
    two calls go, no other row added. Done at `f97a4c9` on 2026-10-25, the probes on by default and
    the toggle gone, the two calls waiting on the package with the engine's item 103.

132. **The tree's documents and comments keep N 4.7 and STYLE.md.** From the sweep of 10:41 on
    2026-10-25 in the gap before item 95's package, over the tracked files less the review's.
    Thirteen places name the owner, the README at lines 234, 1063, 1148, 1263, 1323, 1397 and 1400
    with the owner's modpack, copy and settings, and the comments of Surface.cs, Climate.cs,
    Trees.cs, GroundArt.cs, VitalsFade.cs and VoxelGame.cs; the modpack is never named where a
    reader can open it, Sky.cs naming its shader alone, so they become the reference modpack, the
    reference copy and the reference modpack's settings, the reference folder being the name the
    records give the copy under .ref, unless the owner gives the pack's name first. Twenty-two colon
    joints stand in comments, InventoryWindow, GuiArt, VoxelGame, ModeSwitcher, SlotMoves and
    BlockShape two or three each and one each in Gui, Sky, SectionMesher, PlantArt, GroundArt,
    Climate, play.sh and mob-pairs.py, and three in the README at lines 433, 574 and 1443;
    AmbientParticles.cs paraphrases emission five times. Clean of dashes, banners, the forbidden
    verbs, clefts and history phrasing, the spaced hyphens arithmetic and the semicolon joints left,
    STYLE.md not forbidding them. One commit. Done at `1767348` on 2026-10-25, forty-four
    replacements and the greps clean, the pack called the reference modpack.

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
    cut; the capture committed. The engine's item 79 is in 5.2.215, pushed to the feed at 21:04 on
    2026-10-23; this item follows item 88 once the feed lists it. Done at `d7c4892` on 2026-10-24 on
    5.2.215, the departures and the cost in the head.

67. **The inventory screens take no player key, draw a dragged stack every frame, and dropped items
    wear the pack's pictures.** The owner wrote on 2026-10-23 (Decision 34) that in the crafting HUD
    Ctrl crouches though a screen is open, that a stack held by the left button or dragged with Ctrl
    is not shown moving until the button is released, and that dropped items are not textured with
    the resource pack. The mend in three parts: while a screen is open no key of the player's acts,
    crouch, jump, sprint and the hotbar among them, as the source's screens take the keyboard; a
    stack picked up or dragged is drawn under the pointer every frame the button is held, the slots
    it passes marked as the source marks them; and a dropped item in the world wears the pack's
    picture of its block or item as the blocks do, the game's own art without a pack. Tests: a key
    pressed with a screen open moving nothing, the dragged stack's place read from the frame
    mid-drag, and a dropped item's pixel with the test pack. One or two commits. Done at `073d84b`
    on 2026-10-24, the departures in the head; the held stack's lag stays for the owner's eye on a
    real mouse.

68. **Breaking a block shows the cracks, throws particles of its side texture, and keeps its
    progress across jumps.** The owner wrote on 2026-10-23 (Decision 34) that breaking shows no
    texture for breaking, asking for it with Bare Bones loaded and the game's own behavior without,
    that breaking should throw the side texture as particles dropping as the source does, and that a
    block out of reach hit between jumps should keep its breaking state so it breaks over several
    jumps as the source lets it. The mend: the ten destroy stages drawn over the block being broken,
    the pack's destroy_stage pictures with a pack and the game's own set without; particles cut from
    the block's side texture thrown at a hit and at the break, falling and fading as the source's
    do; and the progress on a block kept while the button stays held and the same block is aimed
    again within the source's delay, so a block above reach breaks over a few jumps. Tests: the
    stage picture on the block at a known progress, the particles' count and fall, and the progress
    kept across a lost and regained aim. Two commits, the cracks and the particles, then the
    progress. Done at `8bc0796` and `7da1d96` on 2026-10-24, the departures in the head.

69. **Felling a tree takes its logs and leaves, some logs falling, and gravity blocks fall (Decision
    35).** The owner wrote on 2026-10-23 (Decision 34) that trees should break all wood and break
    leaves with a particle effect dropping the leaves, with some wooden blocks dropping down still,
    and in that regard sand and the like dropping. The mend: breaking a log the trunk stands on
    fells the tree, its logs breaking from the cut upward with their drops, its leaves decaying over
    a few seconds with leaf particles and the source's drops, and a share of the logs falling as
    blocks that land and settle; sand, red sand, gravel and concrete powder fall as blocks when
    unsupported, as the source's gravity blocks do, landing as blocks or breaking on a non-solid as
    the source has it, the owner's cobble read as that list unless they say otherwise. Tests: a
    tree's logs and leaves gone after the cut with the drops counted, a falling log landing, and a
    sand column falling onto a floor. Two commits, the gravity blocks, then the felling. Done at
    `cc4f29a` and `45b4b93` on 2026-10-24, the departures in the head.

72. **Fire, a block that burns, animated and emissive (Decision 34).** The owner wrote on 2026-10-23
    (Decision 34) that fire is missing and could show the radiance cascades. The mend: a fire block
    kind with the source's animated picture through item 39's frames, emissive into the field so the
    cascades carry its light, lit on a flammable block's top by a flint and steel item and by TNT's
    blast, burning out in the source's time where nothing feeds it and spreading to a flammable
    neighbor at the source's chance within a small radius, so a house burns and a stone floor does
    not; a test of a fire placed on planks spreading and one on stone dying, and a night capture of
    a fire lighting a room. One commit, after item 73. Done at `ee557b3` on 2026-10-24, the
    departures in the head; TNT's blast lights nothing as the source's does not, at `edbc3b2`.

73. **TNT and the creeper's blast.** The owner wrote on 2026-10-23 (Decision 34) asking for TNT and
    the creeper's explosion. The mend: a TNT block primed by a hit with flint and steel or by a
    neighboring blast, flashing white over its fuse of four seconds as the source's does, then
    exploding with the source's ray-cast destruction at power 4, blocks in the blast dropping at the
    source's chance, the player knocked back and hurt by distance; the creeper swelling and hissing
    for a second and a half within reach of the player and exploding at power 3, as the source's
    does, with smoke and flash particles; the blast's light a flash the cascades carry for a frame
    or two. Tests: a TNT's crater's counts, the creeper's swell and blast on the flat world, and a
    capture. Two commits, TNT, then the creeper. TNT at `1eeeeff` on 2026-10-24, the departures in
    the head; the creeper's commit next, its hiss waiting on the game having sound. The creeper at
    `cba35ca`, the cloud an engine particle sheet of the pack's frames and the flash a point light;
    item 73 done.

74. **A held light lights the ground around the player.** The owner wrote on 2026-10-23 (Decision
    34) that holding a light should dynamically illuminate the area around. The mend: a torch,
    lantern, glowstone or other emissive block held in hand gives a point light at the player with
    the block's color and reach, following the player each frame, and the field taking it so the
    cascades carry it; the same for a dropped one lying in the world; a test reading the floor's
    light at night with a torch in hand against none. One commit, small, after item 75. Done at
    `2aea2bf` on 2026-10-24, the light casting shadows, the departures in the head.

75. **The player's hand and held item in view.** The owner wrote on 2026-10-23 (Decision 34) that
    the player's hand and the items should be displayed in view. The mend: a first-person arm and
    the held item drawn in the camera's frame as the source draws them, a block as a cube, an item
    as its picture extruded, swinging on a hit or a use and bobbing with the walk, drawn after the
    world with its own depth so it never cuts into walls, hidden with the HUD under F1; a test
    reading the hand's pixels at a known view and a capture. One commit. Done at `1dce7b7` on
    2026-10-24, the departures in the head. The empty arm goes by the owner's word, Decision 38, at
    `adefc9d`.

78. **Grass as the source places it, with the two-block grass.** The owner wrote on 2026-10-23
    (Decision 34) that there is far too much grass, asking for the source's balance, and that the
    source had two-block-high grass. The mend: the plains', forests' and other biomes' grass and
    flowers placed by the source's patch features at the source's counts and chances, read as
    numbers from its worldgen data, rather than by the game's density, and tall grass as a two-block
    plant with its two pictures; a test of a plains chunk's grass count within the source's range
    and the tall grass standing on two blocks; the plains village picture taken again. One commit.
    Done at `b032e76` on 2026-10-24, the departures in the head.

79. **Swimming as the source has it, no launch out of the water.** The owner wrote on 2026-10-23
    (Decision 34) that the player should not jump out of the water automatically or with such force,
    asking for the source's way, where jumping in water lifts the player to a high point, lets them
    sink and rise again, and lets them climb out at the high point. The mend: the source's water
    movement, a swim-up impulse on jump held, gravity and drag in water as the source's numbers give
    them, the bob at the surface, and the step out onto a block at the surface's high point, no
    launch; a test of the player's height over time in deep water with jump held, bobbing within the
    source's band, and the climb out at a bank. One commit. Done at `526f20b` on 2026-10-24, the
    departures in the head; a one-block bank climbable is the owner's to choose.

80. **Villages spaced as the source spaces them.** The owner wrote on 2026-10-23 (Decision 34) that
    there are too many villages too near. The mend: villages placed by the source's structure set
    rules, a spacing of 34 chunks with a separation of 8 and the salt's random grid, so two villages
    never stand within eight chunks and most are a few hundred blocks apart; a test counting
    villages over a 64-chunk square within the source's bound. One commit, small. Done at `9b25aa3`
    on 2026-10-24, the game's own hash in place of the source's salted random.

81. **Biomes placed by the source's climate, so a badlands does not border a forest.** The owner
    wrote on 2026-10-23 (Decision 34) that a mesa biome stands right next to green land, asking
    whether that fits the source's logic and whether temperature, weirdness and the other values are
    accounted for as the source does. The reading first: the game's biome choice against the
    source's multi-noise, temperature, humidity, continentalness, erosion, depth and weirdness with
    the source's parameter table, said in the commit; then the mend, the source's table taken as
    numbers so badlands sit in hot dry land beside savanna and desert with the source's transitions,
    or, where the game keeps its own climate, a border rule keeping badlands off forests. A test of
    a map's biome pairs holding no pair the source never makes. One commit. Done at `8da806a` on
    2026-10-24, the game's climate kept and the choice mended.

83. **Glass without the diagonal streaks, its borders connecting across neighbors.** The owner wrote
    on 2026-10-23 (Decision 34) to make the glass not have the diagonal streaks and to connect the
    borders so the middle of two glass blocks carries no border, as connected glass has it. The
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

91. **The screen probes stay off by default (Decision 42).** The owner played on 2026-10-24 and saw
    black spreading across the screen in places, gone with the screen probes off, and the light
    spotty, noisy and unstable on movement; the black is the engine's item 90, a glow light's
    non-number the probes' history keeps, mended in the engine's tree and in the package after
    5.2.215, and the spots, the noise and the shift are the engine's sixth term, the probes' own,
    read under its item 71 and left as a limit. So the game calls
    SetGlobalIlluminationScreenProbes(false) at its start and keeps its setting for turning them on,
    with the capture of a view with the probes off; the play check that reads the light holds as it
    stands or moves to the probes off. One commit, small, first. Done at `23aa4ff` on 2026-10-24, a
    checkbox and no e3d verb. Superseded on 2026-10-25 by item 131, the probes on by default
    (Decision 48).

92. **Far land goes (Decision 41).** The owner played on 2026-10-24 and found the far land looking
    bad, and said to remove it. FarLand, its rings, tiles and skirts, FarCache and the far folder of
    the save, the generator's Surface where the far land alone read it, voxel.far, the settings
    slider, the play check of the far land and the README's words and costs table go, the taller
    world and the sections by reach of item 26's first two parts staying; the haze ends at the near
    land as before item 26's third part; the README's world pictures taken again through pictures.sh
    without far land. One commit, after item 91. Done at `2263618` on 2026-10-24, the pictures taken
    again, old saves' far folder left unread.

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
    the entry in the engine's ASKS.md, as the head says; the second part waits on the package. The
    second part is moot since 2026-10-25, item 131 turning the probes on by default (Decision 48)
    and the engine's item 103 taking the call away, so the item is done with its first part, closed
    on 2026-10-26.

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
    package comes. From the engine on 2026-10-24: the package after 5.2.215 has no
    SetProfileWindowKey, F3 showing the debug overlay in its place through SetDebugOverlayKey(Key),
    Key.Null turning it off, and SetUiHiddenKey(Key) beside it for F1, so the game's
    SetProfileWindowKey(Key.Null) of item 55 becomes SetDebugOverlayKey(Key.Null) when that package
    comes, and SetUiHiddenKey(Key.Null) where it keeps F1. On 2026-10-26 the F3 window's parts found
    their homes, Decision 50, the World part's kind, seed and opening going to item 134 with
    voxel.world keeping them meanwhile. Done at `7f59530` on 2026-10-26, the F3 window gone into the
    engine's overlay sections and a Debug Settings screen, the HUD and the hand under the engine's
    F1, Decision 50, the World part to item 134.

135. **A leaf block's faces take the sky floor at level 15 as at 14.** From the owner's word on item
    133's rain forest picture on 2026-10-26, the forest floor showing black sides and blocks under
    the trees. The coder's reading at seed 1's rain forest floor at noon found every near-black
    pixel a jungle or oak leaf face, south or west, 5 to 10 blocks off, facing air at sky level 15
    inside a pocket of the canopy open to the sky straight up; the screen probes change nothing, the
    cascades give such a face about nothing since the rays leaving a vertical face in a narrow
    pocket meet leaves within a block or two and the sky at grazing angles alone, and the game's
    floor gives it nothing since it fades out from 14 to 15 so a wall on open ground keeps the
    engine's shade, the one face at sky 14 lifted to 0,19,0 by the floor and 0,0,0 without; the
    near-black share of the view 0.138 at `a4d383c` and 0.126 as committed, so it stood before item
    133, and the canopy's floor is not black at any depth, the grass under 2, 3, 4 and 6 leaves
    33.1, 27.8, 26.1 and 27.8 against the shade's 43.5. So leaf blocks' faces carry their sky level
    at most 14, the floor applying to them at 15 as at 14, only leaves changing so a wall on open
    ground keeps its shade, and the haze's share at a leaf moving from skyBrightness(15) to (14),
    which the eye's own level overrides in the open; the near-black share of that view read before
    and after, the picture with the near-black pixels marked beside the mended view, and a test of a
    leaf face at sky 15 reading the floor's lift. One commit, small, before item 134. The engine's
    part, a vertical face in a narrow pocket of a canopy taking about no light from the cascades, is
    read as the light such a pocket gets and not a fault, no ask sent unless the owner reads the
    mended view otherwise. Done at `0ba4cb4` on 2026-10-26, the leaf faces' sky level 14 at most,
    the near-black share of the rain forest view from 0.126 to about nothing, the marked view
    pictured beside the mended one.

134. **A world screen in the pause menu, the saves listed to open and a new world begun.** From item
    54's move of the F3 window on 2026-10-26, Decision 50. The pause menu gains World, listing the
    saves under the save root by name with the open one marked, each opened by its button, and New
    World beginning an Overworld or a Flat world with a seed, typed where the engine's interface has
    a text field and otherwise stepped by buttons with a random one offered, the game saving and
    leaving the open world before it opens another as voxel.world does; a test opening a second
    world from the screen and reading its seed, and a captioned picture. One commit, after item 54.
    Done at `5145d2f` on 2026-10-26, the World screen listing the saves and the New World screen
    with a typed seed, the engine's missing text field to ASKS.md.

97. **The game's entries in the engine's ASKS.md hold no colon inside a running sentence.** The
    owner wrote at about 10:3x on 2026-10-24 that some documents join sentences with a colon and
    that STYLE.md forbids it; the entries this session wrote in the engine's ASKS.md hold 24 such
    lines, found by STYLE.md's awk over the Markdown files outside their code. Each is recast as
    STYLE.md says, a sentence of its own or the same sentence recast, a label at a line's end left
    as it is, the Review lines being the reviewing session's; the recast goes into the engine's tree
    as the entries do and the engine's next batch carries it. Small, after items 92 and 93 and
    before item 94. Done in the engine's tree at 10:4x on 2026-10-24, 21 lines recast, the 15
    headings with a comma after the game's name and six sentences split or recast, the engine's next
    batch carrying it; the file's header lines are the engine's sweep's, item 93 there. The engine's
    batches of 2026-10-24 and 2026-10-25 carried the recast, and the one join left, at the update's
    rule, was recast in the engine's tree on 2026-10-26; done that day.

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

27. **Caves as the source's newer ones, cheese, spaghetti and noodle, with aquifers.** The owner's
    word: caves are a big one, the source's newer ones are beautiful. The world has tunnels where
    two noises are both near zero, caverns where a third is high below a depth, and ravines; it
    gains the source's three kinds as numbers, cheese caves from a large three-dimensional noise's
    low values, spaghetti tunnels from two noises thinned by a third, noodle caves thinner still,
    and aquifers setting water and lava levels in caves by depth, with the deep sections item 26
    gives them, and a cave's lighting from the sky above through its openings. After item 26. Two
    commits, the caves, then the aquifers. The lush caves of the source, moss, glow berries hanging
    from the roof and vines over clear pools, join the kinds (Decision 21). Found by item 26's
    second part on 2026-10-24: a cave at minus 36 under ground at 64 takes the sky's light through
    global illumination, its floor reading 13 to 26 at noon against 0 to 11 at midnight, since the
    engine's field reaches 64 up from the eye and a ray leaving it counts as sky; the engine's item
    72 gives the field a far cascade a ray reads before the sky, in the package after 5.2.215, so
    this item takes it when that package comes and reads the cave again. Part A at `d8a05a1` on
    2026-10-24, the caves and the ores, the departures in the head. Part B at `9cb81ad`, the
    aquifers with lava, the departures in the head; the lush caves part C. Part C at `c8dbbee`, the
    departures in the head; item 27 done, the deep caves' sky light read again when the engine's
    package comes. The owner saw at about 10:5x on 2026-10-24 the sun's light leaking onto the tops
    of blocks in caves; the engine's item 94 reads whether it is the sun's map, whose cascades may
    leave the ground above a cave's view out of their depth, or the field's top, which item 72's far
    cascade answers; the game reads it first at a cave of the owner's kind, the tops' light with the
    shadows off and on and with global illumination off and on, and when the package comes it sets
    SetSceneField's far cascade so a cave's rays meet the ground above before the sky. The owner
    added at 10:51 that the sun shows in a cave at a distance, an opening far off the likely way in;
    past the engine's shadow distance of 150 nothing casts a shadow until item 72's fill cascade, in
    the package after 5.2.215, so the far parts of a cave are sunlit on 5.2.215 whatever the map's
    range near the eye, and the game's reading tells the near from the far. The cave reading of
    10:59 is written under the engine's item 94, the near sun the map's and the far sun and the blue
    item 72's. The owner added that the global illumination's qualities move where a black cave's
    ambient fog color begins, near at Low and far at High, and that it shows at a distance even at
    High; that is the counted cascades' reach and the sky past them, and the game sets the engine's
    far cascade through SetSceneField when the package comes, so the far rays of a cave meet the
    ground before the sky at every quality, read again then. The package part at `758e9df` on
    2026-10-24, 5.2.281 pinned with the keys renamed, the far cascade left off and the cave's
    readings unchanged, said in the head; the far cascade set once the engine's item 102 mends its
    slab, and the cave read again with the engine's item 101. The pin of 5.2.283 at `3d70b20`, the
    grove's frame 19 to 22 ms where 5.2.281 read 33 to 35 and 5.2.215 15 to 18, the cave unchanged.
    The pin of 5.2.291 at `9dad43e` on 2026-10-25, the grove's frame as 5.2.283's. The last part
    taken on 2026-10-26 on 5.2.319, the far cascade set through SetSceneField since the engine's
    item 102 mended its slab in 5.2.313, the deep cave read again at every quality for its far walls
    meeting the ground before the sky, and the cost at the grove, with a captioned picture. Read at
    06:3x on 2026-10-26 with SetSceneField(4, 0.25, 2, 32768, 192), the deep cave's far walls at Low
    darkening by about a quarter, their rays meeting rock where they met the sky, Medium and High
    unchanged with the tops at 90 blocks black already, and the grove at 1920 by 1080 paying 1.1 ms
    of global illumination and 0.5 to 1 ms of the scene pass, 1.6 to 2 ms of the frame at both
    qualities; so the far cascade ships off by default behind a Far Cascade toggle on Render
    Settings, its reach following the render distance, the README's options table saying it is for
    Low underground, the readings in a note and the cave at Low pictured off and on, and the item
    closes with it, the choice open to the owner in the game. Done at `f1aa450` on 2026-10-26, the
    far cascade off by default behind Render Settings' Far Cascade with its reach the render
    distance, the cave at Low pictured off and on; the item closed with every part.

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
    After item 27. Two commits, the reflection, then the depth and the edge. The reading at 22:47 on
    2026-10-24 on 5.2.283, the engine's traced reflection for a glossy surface under roughness 0.5
    going through the frame before's picture, the field's cascades and the GPU's rays, the box
    reflection probes for a room, and no planar mirror and no reflection call for a surface the
    transparent pass draws; the water at roughness 0.05 read live looks as it does rough, the river
    in the haze with no shore or sky in it and the pool by the red wall 143, 128, 183 where the
    wall's image would lie, a blur and not a mirror. The call is the coder's, the mirror asked of
    the engine as its item 112, a planar reflection at a height a program's material reads as a
    texture, and the cheap part built first as this item's second commit, the sky's color in a
    fresnel by the view's angle from the environment map, the bed seen through the water darkened
    with depth, the edge and the view from under, with the shore's mirror when the engine's plane
    lands. The second commit at `9d4b295` on 2026-10-24, the depth, the edge and the sky's fresnel,
    the water a mesh of its own; the shore's mirror with the package carrying the engine's item 112.
    Done at `7c36ec8` on 2026-10-26 on 5.2.322, the shore's mirror through the engine's plane at the
    water's top behind Water Reflections, the river mirroring the far bank pictured, 2.6 ms of the
    frame at 1920 by 1080.

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
    lead until the owner remarks on it. One commit, small. Done at `9c07673` on 2026-10-26, the view
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

140. **The window opens at its saved size and mode.** The owner wrote on 2026-10-30 that the
    window's settings, its size and mode, are applied after the window has loaded, which looks odd,
    and that the start should read them somehow if that does not overcomplicate things. Program.cs
    opens the window at 1280 by 720 and the options are applied once the game is up; the start reads
    the options file first, the saved size, the window mode, fullscreen or borderless, and the
    monitor if kept, and opens the window with them, falling back to 1280 by 720 windowed where the
    file is missing or unreadable, the rest of the options applied as before; a headless test writes
    an options file and reads the window's size and mode from the start, and the README's options
    paragraph says the window opens as saved. One commit, small. Done at `5c80c99` on 2026-10-30,
    the departures in the head.

142. **The window shows what the devlog's pictures show.** The owner wrote on 2026-10-30 that the
    game run by hand does not look like the devlog's pictures, the engine's cascade lighting among
    the differences, where the pictures come from offscreen and headless runs. A reading first, in
    the owner's own way of running with the same build, window size and mode. The owner's options
    file is read and each render setting printed against the default, since a kept setting from an
    older build, the quality, the probes, the far cascade or the sun's width, stands over a new
    default; a window screenshot at the devlog pictures' views and hours is read against offscreen
    captures of the same views, pixel by pixel, the window's swapchain format and tone mapping
    against the capture's; and the first seconds after the window opens and the view under movement
    are read against a still that settled, since the cascades and the screen probes converge over
    frames. Then the mend for what the reading shows, a changed default taking precedence over an
    older kept value where the owner never chose it, a window path matching the capture's, or the
    README saying what settles and how long; what is the engine's goes to ASKS.md. First, before
    item 141 if the reading is quick, with a note and the pair of pictures. The owner added that the
    game is run with dotnet run, the bin folder deleted at times, so the build is current and the
    changes show, and that the sessions may open a window on this machine and capture it; the
    reading runs windowed the owner's way, a Debug build by dotnet run, and compares the window's
    screenshot with the pictures. The owner sent the engine's item 97 picture as the game's most
    visible glitch, black specks over every surface, which the engine read on lavapipe alone and
    mended at `1703c54e`, in no package yet; the ninth pack run started for it, the game to pin the
    package, and the reading asks first which Vulkan device the owner's run uses, the log's device
    line read, since specks the RTX never showed point at a software or integrated device chosen
    under the owner's profile. The device read on 2026-10-30, a plain windowed dotnet run with the
    owner's options picking the RTX 4070 Laptop GPU on SDL's x11 path with a B8G8R8A8Unorm swapchain
    presented immediately, no device variable set, so the specks the owner sees have another cause
    than the device, the package's mend pending; a side finding for the engine, the window 2560 by
    1440 from the start under item 140 while the swapchain stays 1280 by 720 from the loading
    screen's first frame at 2.35 s until the first present after the load reports it out of date at
    10.86 s, the engine to recreate the swapchain on a resize before the first frame, an ASKS.md
    entry. Done at `7508d5e` on 2026-10-30, the window matching offscreen, the old kept Soft_Shadows
    0 read as the default through the options file's version, the frame at 2560 by 1440 GPU-bound at
    about 46 ms; the specks wait on the package.

141. **The far cascade is on by default and the shadow distance is 128 blocks.** The owner's word of
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
    proposes. Withdrawn on 2026-10-30 by the owner's word, no far land in the game, Decision 53.

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

147. **The held light casts no shadow.** The owner's word of 2026-10-30, Decision 54, over item 74's
    light casting shadows. A light held in the hand, the torch and the like, is a plain point light
    with no shadow map, the ground and the walls around the player lit as before; the play check of
    item 74 reads the light with a torch in hand against none as it does, and the cost of a held
    light at the grove read before and after. One commit, small, before item 144. Done at `b5626c6`
    on 2026-10-30 with the 5.2.333 pin, the held and dropped lights plain, the shadows 0.7 ms less
    at the grove.

148. **The cactus without a pack is a plain inset box.** The owner wrote on 2026-10-30 that with the
    game's own pictures the cactus's faces extending a pixel out look weird, since the picture has
    no transparent thorns to carry, and asked for the same dimensions, a pixel inset on the sides,
    with no face extending past the box, cubic. With the game's own pictures the cactus's sides
    stand a pixel in from the block's faces as before and the thorn's extension of a pixel is
    dropped, the top and bottom as they are, the faces' texture coordinates following the inset; a
    pack's own model keeps its shape. A mesher test reads the cactus's side faces at the inset and
    none past the box, and the desert pictured before and after. One commit, small, after item 147.
    Done at `5d96667` on 2026-10-30, plain pieces where no picture cuts, the pack's source shape
    kept where its side is clear at the edges.

145. **Streaming spends a time budget a frame.** Decision 52's fourth proposal. The reference spends
    3 ms a frame starting generation and 4 ms meshing, ordered by distance then facing; the game's
    flights read 18 to 19 ms at 8 columns and 21 to 29 at 12. The stream's work on the main thread
    is held to a budget a frame in that order. Read by the flight's frame at its 99th part and the
    stream's main-thread ms a frame before and after. One commit, after item 144. Closed at
    `a272e45` on 2026-10-30 with the reading and voxel.frames, the budget left out, the crossings'
    cost to the engine's item 146 and the game's item 149.

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
    camera's turn having no part, as the owner asked. One commit, before the next push. Mended at
    `fa6963d`, read at 12:21, the cause a point light at the camera's own point, which lavapipe
    shades with black specks across the frame, the probe's pixel one of them, where the GPU shades
    it whole; the shadow cube, the contact shadows and the global illumination each off left the
    specks, so the fault is the engine's shading of a light at the eye, sent to the engine as its
    item 97 through ASKS.md; the game's light stands a third of a block under the eye, within the
    player's box, still not turning with the view. Settled at 13:11 on 2026-10-24 on the run of
    `fa6963d`, green on the three suites and the play job in 36.0 minutes.

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
    probes on fits, 90 minutes with the comment saying why, the owner's rule weighing no cost of the
    Actions bill, and reads the job's log check by check for any check that grew out of proportion,
    saying so in the commit; the walk's checks are not trimmed for it. The mend goes ahead of item
    95. Settled at `ddede23` on 2026-10-25, the limit 90 minutes with the reasons in its comment,
    the lost run's checks grown evenly and none out of proportion; the walk untouched.

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

31. **The zombie's own face is the source's archetype, the villager's eyes lower.** The owner wrote
    shortly before 09:50 on 2026-10-21 to change the default native face of the zombie to the
    source's archetype default, horizontal lines and a nose but no mouth, and to set the villager's
    eyes a bit lower, item 64, the game's own art that shows where no pack is given.

32. **The Bare Bones x Fresh Animations pack is the fourth default pack, above Bare Bones.** The
    owner wrote shortly before 10:15 on 2026-10-21 that they have a mod replacing the creeper's skin
    for Bare Bones and to check; it is the Bare Bones x Fresh Animations pack of TeamHibiscus,
    highest of the three mob packs in the modpack's order, under the Unlicense on Modrinth, so it is
    fetched as the others are under Decision 18 and listed above Bare Bones, item 65, its pictures
    laid on the source's cuboids as Bare Bones' are and its animated models, which need Entity Model
    Features, not drawn. Checked at 10:45 and withdrawn: the pack's pictures differ from Bare Bones'
    only where Fresh Animations moves the eyes onto its animated model's pieces, which the game does
    not draw, so the pack is not a default pack and the mobs stay on Bare Bones', item 65 dropped.

33. **The creeper keeps the vanilla look, the game's own art, under every pack.** The owner wrote
    shortly before 10:38 on 2026-10-21 that they do not like the creeper's picture in Bare Bones and
    to keep the vanilla; the game cannot carry the source's picture, so its own creeper, drawn in
    the source's style, stays whatever packs are loaded and no pack's creeper picture is read, item
    66, item 65's pack included.

34. **The owner's observations of 2026-10-23 and their order.** Having played the game, the owner
    wrote on 2026-10-23 a list of what they saw, written as items 67 to 86: the inventory screens'
    keys, drag and dropped items, the breaking's cracks, particles and kept progress, trees felled
    and gravity blocks falling, the land's light above the eye's height, the stars' dark backing,
    fire, TNT and the creeper's blast, a held light, the hand in view, the shadows' flicker with the
    day, the fog's color by height, the grass's balance and the tall grass, swimming, villages'
    spacing, the biomes' climate, plants over carved caves, connected glass, a loading screen, the
    grass plant's top line, and reflections off. The order the reviewer set: item 39's parts in hand
    first, then the visual faults, 70, 85, 71, 86, 82, 84, 77, 76 and 83, then the play, 67, 68, 79,
    78, 80, 81, 69, 75, 74, 72 and 73, then the pictures taken again, then the items that waited.

35. **Trees fell as one and gravity blocks fall.** The owner wrote on 2026-10-23 that trees should
    break all wood and break their leaves with a particle effect, some wooden blocks dropping down
    still, and sand and the like dropping, so the game fells a tree from its cut log with its leaves
    decaying and a share of its logs falling as blocks, beyond the source's rule, and sand, red
    sand, gravel and concrete powder fall unsupported as the source's gravity blocks do, item 69.

36. **Glass and water reflect nothing for the present.** The owner wrote on 2026-10-23 that the
    reflections on glass and water show the scene field, which clashes with the voxel art style, and
    should remain non-reflective for the present, item 86; reflections return on their word.

37. **Branches are named for their work, never claude/ or a random string, and work stays on main
    where one session writes.** The owner wrote on 2026-10-23 not to create branches named claude or
    the like nor with random characters, but to work on main, or, where several sessions on one
    repository make a branch necessary, on a branch with a descriptive name; N 7.5 holds it, and
    COMMITS.md says it.

38. **The empty hand shows no arm.** The owner wrote to the game's session at about 04:30 on
    2026-10-24, on item 75's hand in view, that nothing is drawn when the hand holds nothing, so the
    arm mesh and the skin go in a commit of its own on top of `1dce7b7`; the held item stays as item
    75 has it.

39. **A devlog under `.github/devlog`, and AGENTS.md's rows for it and for the pushes.** The owner
    asked at about 04:35 on 2026-10-24 that the pictures the sessions made be fetched and saved with
    a text in a folder under .github; `.github/devlog/README.md` holds what landed by day, newest
    first, with the repository's captures, the items closed and the pictures sent on the way as WebP
    files, written by the reviewing session and carried by the coder's commits like REVIEW.md. At
    about 04:50 the owner gave the game's session the word for AGENTS.md's Documents row and for its
    Conventions line to match COMMITS.md's pushes by the reviewing session. At about 07:56 the owner
    asked that the devlog be `DEVLOG.md` at the repository's root, the pictures made on the way
    staying under `.github/devlog`; `.github/devlog/README.md` goes and AGENTS.md's row names
    `DEVLOG.md`.

40. **Far land at 32 columns by default, the render distance kept at 12.** On item 26's readings of
    2026-10-24: far land costs under a millisecond of GPU at any distance, so it stands on at 32;
    the render distance stays at 12 since the owner set it, and the frame there, over 16.7 ms from
    the taller world's draws on the main thread, is item 90's to bring back rather than the
    distance's to give up; near 8 with far 32 holds 60 at 1 GB where a machine needs it.

41. **Far land goes.** The owner played the game at about 10:11 on 2026-10-24 and wrote that the far
    lands look bad, remove them; so item 26's third and fourth parts go as item 92, the taller world
    and the sections by reach staying, and Decision 40's far land at 32 stands no more.

42. **The screen probes are off by default.** The owner, playing at about 10:11 on 2026-10-24, saw
    black spreading across the screen that goes with the screen probes off, and the light spotty,
    noisy and unstable on movement; the black is the engine's item 90 and the rest the engine's
    sixth term, the probes' own, so the game leaves the screen probes off by default as item 91, the
    setting kept, and the owner's eye judges them again when the engine's package with item 90
    comes.

43. **The game uses the engine's ECS where it fits, beside the loop.** The owner wrote at about
    10:23 on 2026-10-24 that the game should move to the ECS where it fits, the engine's ECS being
    its own feature over raylib, with the immediate loop kept where it is best, loops and data such
    as the chunks in the ECS and the whole easier to read; and answered the reviewer's questions:
    the things that move, the world's sections as mesh entities, and the player and the states move,
    generation and meshing stay on the workers; piece by piece on main after items 92 and 93, the
    game playable at every commit, no branch; where the ECS falls short the engine grows first, each
    gap an entry in its ASKS.md taken ahead of other engine work; and the engine's DESIGN.md says
    both where each fits with this game as the model, the engine's own small games staying as they
    are. Items 94 to 96.

44. **The clouds cover the area about the player and do not reach the horizon.** The owner wrote to
    the game's session at about 15:00 on 2026-10-24, on item 108's try, not to stretch the clouds so
    far, since the before images looked right where the clouds reached not to the horizon but
    covered the area around. So the clouds stay within twice the render distance of the eye, fading
    into the sky from six tenths of that reach, as Clouds.cs has them; item 108 is closed and its
    build reverted.

45. **A session saves the pictures of its work in the repository, N 4.8.** The owner wrote at about
    18:5x on 2026-10-24, after BevyCSharp's session kept none of a day's captures, that the rule
    should be written where the sessions read it and be the same in the three repositories, as this
    game does. N 4.8 says it, the game's pictures under .github/devlog/ as WebP files named by the
    date and the item in the commit of their work with the commit's sentence as the caption,
    DEVLOG.md generated from them, which the game's session has done since Decision 39 through the
    reviewer's collection and does itself from here on.

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
    command, each view's place moved back to frame the same subject. From item 80 at 03:5x on
    2026-10-24: seed 1's villages stand elsewhere, so the plains village views at 15.4, 700.6 and
    the snowy and night views at 454.4, 77.4 point at empty land, the desert view at 720.4, minus
    697.6 still at its village's edge; the retake moves the plains views to a village of seed 1,
    576, minus 432 the nearest, and the snowy one to another seed or a village found with
    voxel.find, none standing within 1,600 blocks on seed 1; the plains-village.webp of `b032e76`
    shows a village that no longer stands there until then. The retake at `5556519` on 2026-10-24,
    the views as the head says; play.sh passed; item 46 done.

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
