# Review

Direction for the session working on this repository, written by a second session that reads the
code and the history and writes none of the game. The Now list is read before a batch is started
and before each commit. This file has one writer. The session doing the work edits the Replies
section only. An item is removed from here once the commit that settles it has been read.

Reviewed up to `6d71cd1`, item 6's second commit and nothing else, on `5.2.92`. The sun, the moon, a
halo around each and the 500 stars are quads fixed on the sky, each quad's right side the cross of
its way and the sky's axis and its up the cross of that and the way, given in the sky's space as it
stands at six under the eye's translation and the hour's turn, the stars one run of quads with their
alpha in the vertex color and the fade near the horizon as before, the moon's halo the sun's rings
on the moon's grid in the moonlight's color at 0.2 by night, and nothing reading the camera but its
eye. The sizes are the other game's measured from its sky code and textures, its quad 60 across at
100 with the visible sun the middle 8 of 32 pixels, so the sun 15 across at 100, about 8.6 degrees,
and the moon 10 at 100, about 5.7, where the game's were 9.0 and 6.6, the remark giving the numbers
as taken from it, and the reviewer's figures of 17 and 11 degrees, which took the half-width for the
whole, corrected under item 6; the halo's texture grew from 96 to 104 so its rings clear the quad's
edge at the smaller pixel. A test holds a quad's sides at an hour as its sides at six turned, square
across the way, the right side along the turn and the front toward the middle, and the moon upright
with east on its right at midnight; 113 tests pass with the strict build clean; the play script adds
a night sky from three blocks northwest where the moon and its halo stand at one place on the sky
with their sides turned on the window, under the validation layer, and the room, shadow and
inventory captures match item 7's exactly. The README's picture is taken again at 800 by 450, the
wooded hills and the sea with the sun, its rings and the clouds, until item 11 moves it to a
village; the commit's message gives the reason and not the owner's words or time, as COMMITS.md and
N 4.7 have it, and item 10's will too. Right. Next the three reproductions for the engine's coder,
item 9's second commit, item 10's reading, items 11 and 12, and the owner's push of `58fa8a9` and
`6d71cd1` with them. The owner pushed `58fa8a9` at 21:08, its build run green, and `6d71cd1` at
21:23, its run running at 21:28.

Before it, `58fa8a9` came, item 7's second commit and nothing else, 7 files. The game stands on the
engine's `5.2.92`, the contact shadows set past the shadow map through the engine's item 33, so the
land within the 96 blocks keeps its stepped shadow and the land past the map its soft one, the
pillar's capture retaken with both edges stepped, the README's Shadows section without its fringe,
and the workflows' engine checkout at `33fd0804`, the commit of the package. The strict build and
the tests pass, and the captures with the bump alone matched `5.2.43`'s, so the engine's light moved
none of them. Right. At 21:09 the owner wrote that the sun is to be a bit smaller, as the other
game's, and that the README's picture is taken anew, both in item 6's second commit. Next item 6's
second commit, the sun, the moon and the stars fixed on the sky with the halos as quads on their
grids, the sun at the other game's width and the README's sky picture anew; item 9's second commit;
item 10's reading; and the owner's push of `58fa8a9` with them. At 21:23 the owner wrote that the
README's picture shows terrain and a hill alone where the world is lively, with structures, villages
and animals, and that the render distance is very short, items 11 and 12.

Before it, `80ab301` came, item 9's first commit and nothing else, 10 files. `Ui/Icons.cs` paints
the atlas once on first use, 97 icons of 16 pixels in cells of 18 with clear gutters, ten across:
each block's pixel casts an orthographic ray from the south-east and 30 degrees up against the
block's own boxes, each face its surface's plains color with a fixed speckle of five percent, since
the blocks have colors and not textures, shaded 1, 0.8 and 0.62 for the top, the south and the east,
emissive faces unshaded, a block smaller than its cell both ways drawn up to two and a half times
larger and centered so torches, lanterns and fences read where the first capture showed a torch as
one pixel; an item that places no block is rasterized from strokes and dots outlined in a dark shade
of its color with its upper-left edge lit, the tools one shape a kind with the head in the
material's color, the meats one shape an animal differing by color. The hotbar draws the atlas as a
point-filtered texture at twice the size, the inventory window draws each icon's runs of one color
as rectangles at twice the size, 11,087 for the 97 icons of the creative window, measured on the
development machine at 1280 by 720 as about half a millisecond a frame while the window is open; the
strokes, the swatches and the hotbar's emission ring are gone, and the color's docs say what it is
for. `IconTests` holds four tests, every item an icon whose stretches cover its pixels, a cube's
three shades with an emissive one unshaded, the slab's width and clear rows with the torch standing
tall, and the clear gutters, 112 passing; the strict build has no warning; `build/play.sh` captures
the creative window with all 97 icons over the hotbar, passing on the GPU with no validation error;
the README gains a paragraph on the icons. The other game is named where its shading rule is taken
(Decision 5). Right. The engine's ASKS.md carries the third entry with the reviewer's line, item 34
of the engine's REVIEW.md after its items 32 and 33, and items 6, 7 and 9 each wait on a second
commit when a package brings those three. The owner pushed the four commits at 04:07 on 2026-10-19,
and the build run ended green at 04:12, the tests on the three systems and the play on lavapipe with
its captures. Next the engine's package, and items 6, 7 and 9's second commits from it. On
2026-10-19 at 21:03 the game stood on the engine's `5.2.92`, which carries items 32 to 34 of the
engine's review, the captures with the bump alone matching `5.2.43`'s within two levels outside the
HUD's text and where the mobs, the clouds and the arm moved between runs, so the engine's light of
the day moved none of them. Item 6's second commit was tried on it and loses two things the game
cannot mend: a particle's square is upright in the world, so on the screen it tilts with its place
in the view, 19 degrees at the horizon's edge and 57 looking 45 degrees up at the game's 70 degrees,
and the five-pixel crosses come out slanted with broken arms where the billboards stay square; and a
placed point takes its emitter's color, one alpha for every star of an emitter, so the fade near the
horizon is lost and 41 stars show below it from 90 blocks up where the billboards show none. The
brightness the game mended, the alpha decoded from sRGB since the engine adds a particle's light in
linear light, the matched stars' peak 186 against the billboards' 169. The reviewer's choice is that
the stars stay the game's billboards, as Decision 6 asks of the sky, and item 6's second commit
waits on the engine package that brings particles square on the screen and an alpha per placed
point, asked in the engine's ASKS.md as its item 36; the bump to `5.2.92` goes with item 7's commit,
then item 9's. At 21:05 the owner wrote that the sky's bodies are not to be billboards, since a
thing facing the camera turns with the view and looks wrong in motion, and that they behave as the
other game's, placed on the sky and turning with the hour alone (Decision 7), so item 6 changes: the
sun, the moon and the stars stand as quads fixed on the sky's sphere, drawn by the game, the glow
around the sun and the moon as the engine's particles parented to them, and no engine ask is needed,
the second commit going on `5.2.92`. At 21:05 the owner wrote that some of the animals' feet do not
show, item 10, read before it is mended.

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
   the window on the atlas texture, waits on the package that brings it.

10. **Some of the animals' feet do not show (Decision 6).** The owner wrote at 21:05 on 2026-10-19
    that some of the animals' feet do not show in the game. Read before it is mended, with a capture
    of each animal kind standing and walking on flat ground and on a slope: which animals, and
    whether the feet stand below the ground's surface by the model's origin or the mob's height
    above its block, are culled as faces turned away, are hidden by the pixelated shadow or the
    contact shadow of item 7, or are not in the model at all; then one commit for the cause found,
    with a test that holds each animal's lowest face at or above the ground it stands on, and the
    captures that show it. After item 9's commit.

11. **The README's picture shows the world's life, structures, a village and animals, and not
    terrain alone (Decision 6).** The owner wrote at 21:23 on 2026-10-19 that the README's picture
    shows terrain and a hill alone where the world is lively when entered, with structures, villages
    and animals. The picture at the README's head is taken where a village stands with its buildings
    in view and animals near, at the hour and heading that show them, with the render distance of
    item 12, taken by the capture script from a seed and a place written in it so it is taken the
    same again, the sky's picture of item 6 beside it. One commit, with item 6's or after it.

12. **The render distance is read and set by what the frame affords (Decision 6).** The owner wrote
    at 21:23 on 2026-10-19 that the render distance is very short. Read first: what the game's
    distance is in chunks and blocks, where it is set, whether the captures and the owner's play run
    at the same distance, and what bounds it, the frame time on the development machine's GPU and on
    lavapipe at each distance, the scene field's and the cascades' cost among it, and the memory;
    then the distance set to what the frame affords at the game's pace on the development machine,
    with the figures per distance in the commit's message and the setting's remark (N 3.5), and said
    in the README's running section. One commit, after item 11's, the owner's word settling the
    figure where the reading leaves a choice.

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

## Replies



