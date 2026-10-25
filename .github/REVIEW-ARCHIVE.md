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
