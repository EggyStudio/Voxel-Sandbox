# Review

Reviewed up to `9d25749`, 177's first step, one origin for the whole game, a multiple of 1,024
blocks set near the saved place at open that every float place is kept against, blocks, columns and
sections keeping the land's own whole coordinates, the saves and the commands the land's places in
doubles, mobs.bin in a second version with the first read, 46 files, pushed at 00:38 on 2026-11-12
after two reads, the review's and a second reader's of every floor, distance and conversion, which
found no place where one kind of place is taken for the other; the strict build is clean, 376 tests
pass, the play passes whole and none of its 17 pictures moves; on the published 6.2.41 the owner's
room then reads no row with a step over 3 levels at Low, Medium or High at 1280 by 720 and one at
Medium at 2560 by 1440; it keeps the form and carries the review's files. Before it `760522a`, the
review's file alone with the Replies of 173's check, 175 and 177's read, pushed at 23:49 on
2026-11-11, no run by the workflow's paths. Before it `88d6303`, a fault 175's read found,
ChunkStreamer.Unload squaring a column's distance from the player in an int, which wrapped negative
past 46,340 columns so a teleport of some 741,000 blocks left the old columns loaded, squared in
longs with a test that teleports a million blocks, pushed at 23:27 on 2026-11-11, its run started
where the run of the commit before it never did and green at 00:33 with the play in 66 minutes,
which settles Verdict 12; the strict build is clean, 368 tests pass and the play passes whole in 310
s; it keeps the form and carries the review's files. Before it `5ad3f58`, the review's file alone
with the Replies of 174 and 176, pushed at 23:00 on 2026-11-11, no run by the workflow's paths.
Before it `6b0043b`, voxel.spawning, asked by the owner on 2026-11-11, every mob kept out of the
world from the next frame on and brought back where it stood, the play switching it off before its
first check, pushed at 21:31 on 2026-11-11, its run left queued by GitHub's outage and not started,
Verdict 12; its strict build is clean and 367 tests pass, two plays under the layer pass whole in
317 and 218 s, and of their 17 captures dusk differs in 0.00 percent of pixels by more than 2 levels
where 2.3 before, the night sky in 0.18, within its clouds; it keeps the form and carries the
review's files. Before it `1f1e955`, item 172, terrain.slang, mob.slang and bits.slang returning
unlit on the branch drawn unlit, each fragment about 590 KB smaller and the fifth frame's 22
pipelines made in 20.3 s where 43.9 from an empty cache with the layer off, pushed at 19:29 on
2026-11-11 once the pin's run had ended, its run green at 20:36 with the play in 67 minutes where
the pin's took 96; it keeps the form and carries the review's files. Before it `068fd5f`, the pin of
the engine's 6.2.41 with the workflows' engine checkout at `894542da`, pushed at 15:29 on
2026-11-11, its run red at 15:34 in the play job before the game was built, the runner not reaching
its package mirror for two hours and a quarter, Verdict 11, green at 19:26 on its tenth attempt, the
play in 96 minutes, which settles the verdict; its strict build is clean and 366 tests pass. The
water's mirror passes six plays of six under the layer, three from an empty cache in 393, 417 and
434 s and three from the cache each left in 237, 242 and 221 s, each play's caches in a folder of
its own; from a cold cache the pipelines' file grows to 21.8 MB and the pool's three terrain.slang
pipelines compile in 14 to 19 s each at once, and from a warm one every pipeline made in play says
from the cache, the pool's three in 1.17 to 1.37 s each; frame 5's 22 pipelines take 54 to 68 s
cold, one after another, and 4.4 to 4.6 s warm. The engine's trial puts about 1.2 s of each such
pipeline on the validation layer, 94 ms compiled without it, so these are the layer's seconds, which
the play turns on, item 171. It keeps the form and carries the review's files. The pushes before
these stand in REVIEW-ARCHIVE.md's head records.

Next the checks of the engine's mends as its packages come, the ceiling's line at 2560 by 1440
first, then the dither and 176's blobs, each on the owner's room and on 174's scene; between them
177, one origin for the whole game, a whole column near the player that every float place is kept
against, so the engine is handed places near zero at any distance, in three steps; 173's check,
174's, 175's and 176's reads are done; then the mirror's check waiting for its pipelines at the pin
that carries the engine's 254. 172, the engine's lighting called once in each of the game's shaders,
is done at `1f1e955`, green at 20:36, and 173's read is done. The owner on 2026-11-11, of taller and
more varied terrain, "there are more important stuff currently and the terrain generation is nice
even if not fully fledged out", so nothing of the terrain is an item. Item 29's second part stays
parked by the owner's word. A program on the GPU runs under the lock (N 7.9).

REVIEW-ARCHIVE.md beside this file holds, their numbers kept, the items done, the items closed or
parked, the verdicts settled, Decisions 1 to 58, the Replies before those here and the head records,
moved there on 2026-10-23 and again on 2026-11-09 so that this file holds the current work alone and
stays under 100 KB, read at each watch tick; a batch reads this file's head, its items and the
Replies (N 7.7), and the archive when an item, a verdict or a decision named here is wanted in full.

## Now

Closed without a change or done as read, each in full in REVIEW-ARCHIVE.md under Items closed,
parked or shortened: 156, the ground's atlas and the anisotropic filter, nothing changing by the
owner's word; 164, the engine's split packages at the restore, read with nothing to change.

29. **Generation and meshing on the GPU, the blocks kept on the CPU (Decision 12).** Part one, the
    noise fields, landed at `2549786` and `1d31dda` on 2026-10-25, every noise in fixed point and
    the caves' readings in a compute shader read back without a wait, verified bit for bit against
    the CPU path. The meshing moved to the workers at `bb0b9bb` the same day, update.mesh under half
    a millisecond in the flights. Part three went with the far land (Decision 41). Part two, a
    compute mesher a section, is held on its numbers, 1 to 2.6 ms of a frame in flights alone
    against several commits over days, for the owner's word, Decision 12 being the owner's. The item
    as it stood is in REVIEW-ARCHIVE.md under Items closed, parked or shortened. The owner answered
    on 2026-11-11 at about 16:40 that part two stays parked.

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
    that carries the engine's 254.

173. **A built room's striped wall, read for the engine.** From the owner on 2026-11-11 at about
    18:35 and the engine's item 257. The owner's saved world, overworld of seed 1, opens at 4448.4,
    24, 599.8 facing 48 degrees in a dark closed room they built, the illumination at Low, and the
    first view shows the noise they have named for weeks, stripes of small squares in a glow light's
    light on the back wall; the review saw the same from the game's built program on a copy of the
    owner's data folder, ~/.cache/claude-reviewer/owner-view/data, with its captures beside it under
    shots. The game's coder reads the room from that copy and never from the owner's own folder, by
    XDG_DATA_HOME naming the copy. The blocks that glow within reach of the wall, their kinds and
    places, the wall's and the ceiling's planes, the eye's place; the view captured as it opens and
    with each glowing block taken away in turn in a second copy, to say which light draws the
    stripes; and the engine's gi.show and field.march at a striped place and a smooth one, as its
    coder asks. Written as an ask in the engine's ASKS.md with a sheet by N 4.9. A check for the
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
    captures the owner's first view from its copy on 6.2.41 and on that package, built in its work
    folder, the wall's rows read at Low, Medium and High on both, before and after on one sheet for
    the owner. The work folder i173 is kept until then. The check's first step is done at 23:32 on
    the engine's package 6.2.41-local.257. The stripes are gone, no row of the east wall with a step
    over 3 levels at any quality at 1280 by 720 or at 2560 by 1440 where the published package has
    up to 29 and up to 16, the figures under the engine's item 257. One thing reads worse, a line
    across the ceiling at rows 173 to 175 at 2560 by 1440 alone, which went to the engine's coder.
    The sheet 23-50-item-173-mend.png went to the owner. The second step, 174's scene on the package
    at both places, follows.

174. **The same built scene near the origin and 4,432 blocks out.** From the engine's item 257 on
    2026-11-11. The owner's noise was float precision 4,450 units from the origin in points and
    normals the engine takes from the depth, which no scene at the origin shows, and the engine's
    item 258 reads its other passes for the same. The game gives it the controlled case. In a copy
    of the data, one small scene built of blocks twice, block for block, once within 64 blocks of
    the origin and once beside the owner's room near 4448, 24, 600, each on a platform in the air
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

176. **Bright colored blobs in the distance under bloom.** From the owner on 2026-11-11, to the
    game's coder, "At distance there are these colored bloomed blobs in the background". Read on
    6.2.41 at `1f1e955`, the sheet 21-30-red-blob.png in the dump. One stands in the origin scene's
    view, a spruce trunk's foot on snow at -98, 105, 39, 134 blocks off. With bloom at 0 it is one
    pixel reading 255, 209, 199 beside a trunk of 54, 46, 42, which bloom spreads into a red blob
    over the tree. The pixel is the same through every part gi.toggle leaves out, with each cascade
    alone and at Low, Medium and High, and only the illumination off clears it, the snow then 221,
    215, 205; no block light, fire or mob is near. From 15 blocks a trunk's lower half above the
    snow reads 172, 90, 0, blue at exactly 0, where its upper half reads 82, 50, 23. So the
    illumination's light at a trunk's foot beside sunlit snow is far too bright, whether in the
    engine's light or in terrain.slang's taking of it not yet said. The read goes ahead of 174 since
    the owner asked it. Which term carries it at that pixel and from 15 blocks, the light's parts as
    voxel.pixel and the engine's views give them; what clears it, the snow made grass or stone, the
    trunk made stone, the sun's shadows off, the hour at noon and at dusk, the far cascade off; and
    the smallest built scene that shows it, a log standing on snow in the sun, added to 174's scene
    at both places so it is read near the origin and at the owner's place alike. Then an ask to the
    engine with the numbers and a sheet, or an item of the game's where terrain.slang takes the
    light wrongly. Recorded at 21:53, the coder's read so far, on 6.2.41 from a copy of 174's data,
    bloom at 0. The sheet's fourth panel is not the fault. The orange block is the dirt under the
    trunk, its sides showing where the snowy ground steps down, taking the same light as the log
    over it, both 229, 209, 183 with every surface drawn white, and its orange with blue at 0 is
    dirt's own color under the frame's grading at a saturation of 1.4, 138, 60, 0 with the
    illumination off; so nothing is kept by the block. The blob is one pixel on a silhouette's edge,
    272, 352 from 174's origin eye, the trunk in the pixel to its left at 55, 46, 42 and snow two
    blocks behind it, 138 blocks off. It reads 255, 209, 199 in five reads 7 frames apart and clears
    with the eye moved a quarter block or turned 0.05 degrees. Every part gi.toggle leaves out, the
    specular too, each cascade alone, the three qualities, the sun's shadows off, the sky's floor at
    0, the light levels off, noon, and the block under the pixel made stone, snow or grass leave it
    as it is, and only the illumination off clears it, 221, 215, 205. Its color is the log's albedo
    under about ten times its light. So an edge pixel takes the trunk's color with light from a term
    no toggle reaches, and the game cannot split it further, gi.show drawing the probes' data and no
    pixel's resolved light. To come, the eye stepped sideways from 133 blocks at both built places
    counting edge pixels and blobs, then the ask. The read is done at 22:45. The blob comes of
    shading at 4 samples a pixel, gone at 1 sample, dimmed with the temporal antialiasing on, one
    pixel still at twice the size; the sweep's counts and the sheet 22-40-item-176-samples.png are
    in the ask and under the engine's item 259, which mends it. What remains here is the check after
    that mend, the sweep's counts at both places read again.

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
    distance, the owner's 4,450 among them. So it is no filler and goes next after each check. The
    origin a multiple of 1,024 blocks, so every grid of the engine's falls against the land as
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
    what the ladder measured, and widens if 4,096 then reads as the origin does.

## Verdicts

No verdict is open. Verdicts 1 to 12 are settled, said under each in REVIEW-ARCHIVE.md.

## Decisions

Decisions 1 The old game is replaced in place, its history kept; 2 The license stays MIT; 3 `e3d`
comes from the engine's checkout; 4 What the game finds the engine lacks goes to the engine's
ASKS.md, as before; 5 Prose is technical and generic; 6 Development resumes; the sky, the shadows
and the icons are the game's week; 7 Nothing in the sky faces the camera; the sun, the moon and the
stars are fixed on the sky and turn with the hour; 8 The mobs' sizes and models may be the other
game's, since they are numbers; 9 The tonemapping is the engine's somewhat boring curve; 10 The
review's files are committed by the batch that comes next, and by the reviewer at most once a day;
11 The owner's list of 2026-10-20 at 23:30, in the reviewer's order; 12 Compute shaders for
generation, the owner's word of 2026-10-21; 13 The owner's packs set the look, read from the
player's copy and never shipped; 14 The owner's modpack is the reference of the game's feel, read
for shapes, numbers and ideas, and nothing of it ships; 15 The six candidates of Decision 14 are
taken; 16 The screen probes stay off in the game, and the cascades' faults go to the engine; 17 The
windows after Recolourful Containers and the HUD after Bare Bones, read from the player's packs, and
the game's own art moved toward them; 18 The three packs are not shipped and are fetched at first
start instead; 19 A screenshot on F2; 20 Simple Grass Flowers as a fourth default pack, through
block state variants; 21 The look the game aims at is the owner's modpack's gallery; 22 Packs give
pictures alone, and every rule is the game's own; 23 The look without a pack takes Bare Bones'
colors; 24 The README's pictures, an animation at its head and pictures in its parts; 25 Three words
during item 46's scouting, the sky's bodies cut, the rifts in villages and the sun's look; 26 The
engine's F1 and F3 come to the game; 27 More vegetation, and the plants cast shadows; 28 The blades
like the modpack's, and the light and the grade from the shader settings the owner set; 29 The sun's
shadows step on a grid of eight a block; 30 The mobs take Bare Bones' entity pictures; 31 The
zombie's own face is the source's archetype, the villager's eyes lower; 32 The Bare Bones x Fresh
Animations pack is the fourth default pack, above Bare Bones; 33 The creeper keeps the vanilla look,
the game's own art, under every pack; 34 The owner's observations of 2026-10-23 and their order; 35
Trees fell as one and gravity blocks fall; 36 Glass and water reflect nothing for the present; 37
Branches are named for their work, never claude/ or a random string, and work stays on main where
one session writes; 38 The empty hand shows no arm; 39 A devlog under `.github/devlog`, and
AGENTS.md's rows for it and for the pushes; 40 Far land at 32 columns by default, the render
distance kept at 12; 41 Far land goes; 42 The screen probes are off by default; 43 The game uses the
engine's ECS where it fits, beside the loop; 44 The clouds cover the area about the player and do
not reach the horizon; 45 A session saves the pictures of its work in the repository, N 4.8; 46 The
README's pictures taken again, an animation at its head and pictures in its parts (Decision 24); 47
The devlog written as the work goes; 48 The screen probes are on, off a debugging switch alone; 49
The devlog carries notes beside the pictures; 50 The F3 window's parts find their homes; 51 The far
cascade is on by default and the shadow distance is 128 blocks; 52 A voxel engine in Rust is the
reference for the game's performance; 53 No far land in the game; 54 Held lights are plain lights
without shadows; 55 A reading's picture explains itself, and a plain capture or pair stays where it
shows the thing; 56 Every picture a session makes goes to the repository's dump folder; 57 The
version's major and minor move by the reviewer's judgment; 58 No one is named in a picture, each in
full under Decisions in REVIEW-ARCHIVE.md.

59. **Anisotropic filtering at 16 everywhere.** The owner wrote on 2026-11-04 at 11:07, "Anisotropic
   filtering is at x16 everywhere because it is cheap and on steep angles it is visually better." So
   every sampler that reads a texture, color, normal, surface or atlas, filters anisotropically at
   16, or at the device's most where it offers less, at every quality and with no knob; the shadow,
   probe and table samplers that are not textures keep their own filters. The game's textures read
   through the engine's samplers, so it comes with the engine's item 201 and the next package; the
   game sets nothing. At 11:40 the owner added that the game is pixel filtering and needs no change,
   so its atlas stays at Point and item 156 is closed.

60. **Version tags.** The owner wrote at about 14:00 on 2026-11-08, having tagged every commit of
    the Unity Essentials repositories as `v2.0.30` and the like, that the three projects take tags
    too, and answered the review's questions at 14:1x. The form is `v6.0.91`, the letter v before
    `build/version.sh`'s number. Tags go on each major or minor change and on the commits packages
    were built from, since "doing every commit clutters it and is unnecessary as we want to focus
    the version tags to releases time apart with meaningful changes and not commits apart". The
    major and the minor are the sessions' to raise, "each session working on the project can decide
    when the time is right to rise that number in minor or major as each commit counts as patch",
    with "full liberty" when "a step is done and meaningful changes were done". N 6.3 holds the rule
    and the reviewer sets the tags as it pushes. `v0.2.0` marks `7e052e8`, which set 0.2 (Decision
    57), set at 14:12; a release build of `build/publish.sh` is tagged when the owner names one.
    Decision 57 stands with the game's session able to raise the version as well as the reviewer.

61. **No personal name in any repository.** On 2026-11-08 at about 15:25 the owner showed the
    account's code search finding the personal name in six files of four repositories, this game's
    `LICENSE` among them, after asking the same morning that the name stand nowhere, and at about 15:27 asked that the game's license be the Mozilla Public License Version 2.0, as the engines'
    are; the MPL names no holder. The history's authors are not rewritten here, since the review's
    files, the devlog and the package's release notes cite the commits by their hashes, and the
    commits' noreply address is the account's already.

62. **No middle dot.** The owner wrote at 17:57 and 18:00 on 2026-11-08 that the middle dot is not
    used, the README's top links separated by a space alone and the dot nowhere else; the reviewer
    changed the README's first line in the tree on their word, for the next commit to carry (item
    159). The rule goes into STYLE.md's Punctuation as the shared text has it everywhere.

63. **Comments on the code's own terms.** The owner read a comment of scenes_sponza at about 19:00
    on 2026-11-08, "The frame graded as that scene's HDRP profile grades it, by the look of its AgX
    - Powerful.exr, a lookup build/unity-lut.py writes from that strip, a gamma of w 0.225, and a
    bloom of intensity 0.25, threshold 0.333 and scatter 0.7, worked out as HDRP works them out",
    and wrote that it "sounds just bad", reads as an em dash, and "is taking too much liberty of
    outside knowledge and in-process session knowledge", in scenes_sponza explicitly and probably
    everywhere else. The rule from it, in STYLE.md's Register and as N 4.11, is that a comment
    stands on the code's own terms. It says what the code does and why in the engine's words; a
    number taken from elsewhere stands with the reason it is right here, what it makes the frame or
    the program do, and the source is named once in the documents' references and not in the
    comment; the file names of outside assets, another product's profile or process as the
    explanation, "as X works it out", and the facts of the sessions' work are not written in a
    comment. Item 159 carries STYLE.md's sentence and the game's own sweep.

64. **A fault the owner sees in motion is read in motion.** The owner wrote at 17:25 on 2026-11-09
    that the game's global illumination has looked noisy the same way for weeks, and the review's
    answer read only the shader cache. The readings here since 2026-10-24 were settled stills, which
    a fault in motion passes, so a fault named while moving is read from a recording, the owner's
    where there is one and a scripted walk and turn offscreen where not, picture to picture, with
    the standing, the walking, the turning and the two seconds after a stop each given their share
    of pixels moved, and the review asks the owner for a recording and the settings file when a
    fault is named without one. The engine's Decision 107 says the same; told to the owner for
    approval as its Decision 95's precedent allows. Item 166. Approved by the owner on 2026-11-11,
    asked by the review with N 7.9, N 7.10 and N 7.11.

## Replies

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
