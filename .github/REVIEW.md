# Review

Reviewed up to `7508d5e`, item 142, pushed at 19:18 on 2026-10-30, its run green at 20:31 with play
in 73 minutes. After it `2c458b0`, item 141, pushed at 20:36, its run green at 21:56 with play in 80
minutes: FarCascade true and ShadowDistance 128, the options file at version 3 so an older file's
values take the new defaults; the quiet table at 1280 by 720 reads 8 columns at 16.7 ms, 12 at 17.0
and 16 at 26.6, the 128-block shadows adding 1.8 ms of the GPU and 1 of the CPU at 12 with the
cascade draws 389 to 1,300 and the far cascade 1.9 of the GPU, the far oak's shadow and the cave
pictured. After it `5f31bdf`, item 143, pushed with `b5626c6` at 21:59, the run of `b5626c6` green
at 23:23 with play in 84 minutes, the sections grouped 2 by 2 across and one high since groups two
high drew the caves under the land and cost the GPU 3 to 4 ms at 12 columns; at 12 the draws 3,150
to 1,380, the CPU's passes 13.4 to 7.4 and 9.5 ms, the frame 17.1 to 17.3 GPU-bound, at 16 the frame
27 to 31 going to 23 to 26; the pictures the same within 0.0016; memory about 500 MB higher at 8 and
12 columns, 255 MB of it the engine's ring of mapped buffers kept for good for a mesh written in
place, an ASKS.md entry, the engine's item 145. After it `b5626c6`, item 147, pushed at 21:59:
5.2.333 pinned, held and dropped lights plain point lights, the grove's frame at 1920 by 1080 with a
torch 22.9 to 23.2 ms going to 21.8 to 22.6 and the GPU's shadows 4.0 to 3.3; the specks read on a
midnight meadow, 17 black pixels among lit neighbors going to 4, the 13 gone a seam of the held
light's shadow and the 4 left stars and edges against the sky, none on a lit surface before or
after. After it `5d96667`, item 148, pushed with `a272e45` at 23:27: BlockShape gains plain pieces
the mesher draws where none of the shape's pictures cut anything out, so the game's own pictures
give the cactus an inset box with the sides a pixel in and the top and bottom as before, while a
pack whose cactus side is clear at its edges keeps the source's sides; tests in MesherTests and
CutoutTests, two cacti on the desert pictured before and after. After it `a272e45`, item 145, pushed
at 23:27 and watched, the time budget built and left out since flights read no gain, 12 columns at a
99th part of 86 to 105 ms without and 92 to 110 with, 8 at 49 to 72 and 47 to 55, within the noise;
the slowest frames, 64 to 79 ms, are the engine's scene field at 8 to 30 ms of CPU and the game's
own drawing up to 20, a step of one section over loaded land giving 32 to 82 ms frames with the
field 12 to 42 ms at every step and the flood and casters 9 to 15 at a section first crossed;
voxel.frames reads the median, 99th part and slowest of the last 600 frames; the field's crossings
to the engine as its item 146, the flood and casters item 149. The window path matches offscreen, an
RMSE of 0.004 to 0.007 with the owner's options at 2560 by 1440 and 0.001 to 0.002 fresh at 1280 by
720, cloud edges mostly; a turn settles in about 22 frames; Debug and Release both run about 46 ms
at 2560 by 1440, GPU-bound at 22 frames a second against the pictures' 17 ms at 720p; the owner's
file kept Soft_Shadows 0, never chosen. The options file's first line is its version, 2, and an
older file's line equal to a default since changed reads as that default; the note is
2026-10-30-item-142-run-by-hand.md. The swapchain finding stands in the engine's ASKS.md, its item
134. Before it `5c80c99`, item 140, pushed at 18:21 on 2026-10-30, its run green at 19:01 with play
in 40 minutes. The start restores the options file's resolution and mode right after InitWindow,
before the game is built, the window appearing as saved about 8 s before the world loads where it
changed after; no monitor setting exists to read, and a hidden window cannot be shown later since
the engine takes a program-set hidden window as a --hidden run; the test reads the values and a
missing file's 1280 by 720. AGENTS.md's REVIEW.md line reads the head, the batch's items and the
Replies by the owner's word in the coder's session. It carries NORM.md with N 7.7 and the README's
first line. Before it `fc0937a`, item 139, pushed by the owner's hand on 2026-10-26 while the
sessions were paused until 2026-10-30, its run green. Before it on 2026-10-26, each green: `9c07673`
(138); `b25e668` (137); `4aef50c` with `6001eed` (the README's quick links, 136); `7c36ec8` (42);
`f1aa450` (27); `5145d2f` (134); `0ba4cb4` (135); `7f59530` (54); `1944378` and `48e5bea` (133);
`a4d383c` (95). Each commit's detail stands in its item in REVIEW-ARCHIVE.md, its devlog note and
its own words. On 2026-10-30 at 18:0x the owner wrote items 140 and 141 (Decision 51), the README's
links as the devlog and 3DEngine, a Rust voxel engine as the performance reference (Decision 52),
and asked that the sessions spend fewer tokens at the same quality, N 7.7. Item 144 closed at its
count on 2026-10-30, the quads merging to 81 to 89% and not half; 143's other 245 MB is the driver's
too, the ring's mapped memory counted as the device mapping and its shared backing, the game's own
arrays 68 MB more.

Items done and verdicts settled before 2026-10-23 stand in REVIEW-ARCHIVE.md beside this file, their
numbers kept, moved there at 19:30 on 2026-10-23 so this file holds the current work alone and is
read whole before a batch; the archive is read when an item or a verdict named here is wanted in
full.

## Now

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
    costs. Three or four commits, one a part, each after its package. Asked at 06:12 on 2026-10-25
    whether the fixed-point noise, which moves every seed's land, keeps a generator version in
    WorldInfo so worlds saved before keep the float noise or replaces it outright with seams in old
    worlds, the review chose outright, under the rule of 2026-10-23 that nothing is kept for
    upgrading before a release, the tests and play.sh pinned to the new places in the same commit
    and seed 1's places read again for the records, the owner's saved worlds generated anew; the
    owner says otherwise if the saved worlds are to keep their land. Corrected at 06:13, the change
    is far smaller than asked, the noises being Perlin's improved noise over a permutation table
    shuffled from the seed, which a fixed-point port keeps with the same gradients, fades and lerps,
    so the values differ by the rounding alone, about 1e-5, and the blocks that change are those
    where a height or a cave's density sits at a rounding edge, a scattering and not new land, seed
    1's pinned places expected to hold, an old world showing a rare one-block step at its saved
    columns' edges; the changed blocks over a region counted first, before the GPU half. The
    shader's fixed point is 32-bit, the engine enabling no 64-bit integers, a Q16 product split into
    two 32-bit multiplies giving C#'s 64-bit result exactly; on the CPU a column takes 15.45 ms, the
    caves' density grid 5.31 and the 256 ground lookups 0.82, the rest the block loop, the ores, the
    plants and the structures, so part one's first gain is the caves' grid, the ground's 2D noises
    going with it and the CPU combining the fields as today. Part one's first half at `2549786` on
    2026-10-25, every noise in fixed point on the CPU with the changed blocks counted, a scattering
    of 0.032% and no pinned place moved; the GPU half next, the caves' corner readings in a compute
    shader read back without a wait, voxel.gpu on or off as a setting and voxel.gpu verify comparing
    both paths bit for bit in play.sh. The GPU half at `1d31dda` on 2026-10-25, the caves' readings
    in a compute shader read back without a wait in batches of 8 with the CPU path kept and verified
    bit for bit, a worker at 10.98 ms a column against 16.47, which ends part one; part two, the
    meshing on the engine's item 69, next. Part two waits on the engine from 07:48 on 2026-10-25,
    its ASKS.md entry in the engine's tree the engine's item 121, since a mesh over shader buffers
    on 5.2.291 holds one stream and the field reads colors from a UNorm8x4 stream alone, so a
    section meshed in compute would bounce white, and holds whole buffers, so each section would be
    sized for its fullest case; the reading before it, three ten-second sprint flights east over
    unloaded land on seed 1 at render distance 12 offscreen at 1280 by 720 on the RTX 4070 Laptop
    GPU, the frame 22.0, 37.5 and 37.0 ms, update.mesh 1.0, 7.8 and 6.9, update.stream 0.9, 1.9 and
    2.4, cpu.scene_field 6.8, 8.6 and 9.3 and the GPU's passes about 12, so the main thread binds
    the frame and the meshing is the largest part the game owns, the two profile values going with
    the next commit. Part three next where it needs no engine change, item 95 otherwise once the
    package publishes. Part three is dropped at 07:51, item 92 having removed far land under
    Decision 41, so nothing of it remains to move to the GPU; the light levels stay a fourth part if
    their cost shows, the flights' numbers showing the meshing and the field's main-thread cost and
    not the flood. Part two's number read again at 17:40 on item 95's tree, the same three sprint
    flights at distance 12 offscreen at 1280 by 720 on a quiet GPU: Debug frames of 21.7, 16.7 and
    19.0 ms with update.mesh 0.93, 2.59 and 1.09, Release with the entities 16.3, 13.7 and 14.6 with
    0.35, 1.07 and 0.15, Release on DrawMesh 17.0, 14.6 and 18.0, against the morning's Debug frames
    of 22.0, 37.5 and 37.0 with update.mesh up to 7.8 on 5.2.291, the difference the package, item
    95 and the quieter GPU; so the meshing is at most 2.6 ms of a Debug frame and about 1 of a
    Release one, and the frame stands under 16.7 ms in Release in two flights of three. On 5.2.313
    part two would upload each frame's sections in one write of blocks, light and borders of about
    17 KB each before one dispatch, since UpdateShaderBuffer waits for every dispatch before it,
    needs a count pass read back a frame later before a range of fixed capacity is allocated from a
    pool and the mesh pass runs, so edits stay on the CPU mesher, and ports the whole mesher to
    Slang, the seven face classes, the shapes and plant walls, the connected glass, the water depth,
    the per-corner light with the open-cell gather, the corner shade, the biome tints, the atlas's
    spots and the field's colors, several commits over days for those 1 to 2.6 ms in flights alone.
    Chosen, the meshing moves to the workers first, SectionMesher filling its arrays off the main
    thread and the main thread taking the finished arrays in and uploading them, a commit or two;
    part two as written is held on these numbers for the owner's word, Decision 12 being the
    owner's. The meshing on the workers at `bb0b9bb` on 2026-10-25, update.mesh under half a
    millisecond in the flights and the settle no slower, the readings in the devlog's note; part two
    as written stays held for the owner's word.

114. **Cliffs are rarer, the land mostly rolling.** The owner wrote at 13:38 on 2026-10-24 that
    there are too many cliffs. The cliffs' cuts come where the terrain's slope is steep and the
    generator carves them; their count falls to what the source's land shows, a cliff here and there
    at a hill's or a mountain's side and none across the plains and the forests, by a higher slope
    for a cut, a wider spacing, or a noise that gates them by region, the coder reading the land of
    seed 1 at the owner's spawn and the plains village for the count before and after and saying it
    in the commit; a test counting the cliffs over a seeded span against a bound, and a capture of
    the same land before and after. One commit, after item 113 and before item 94. The coder's
    reading at 15:38 on 2026-10-24 found no cliff feature; what reads as cliffs is two things. The
    drops of two blocks or more between neighboring columns all lie along the rivers, the channel's
    cut from the valley floor to three under sea level over about six blocks giving a river on high
    ground a wall several blocks high, 1,458 such steps on the 512-block square at the spawn and
    6,026 on the 2,048-block square about the origin against 315 and 1,319 with the channel's cut
    off; and the brown lines across the plains are single-block steps of the rolling hills, plus or
    minus 18 at a scale of 160 over five octaves, a step's dirt side showing, none of them two or
    more away from a river. The call, the river channel's band widened with the drop it cuts so its
    banks fall at about a block per two blocks, in one commit with the counts before and after; the
    hills' amplitude on the plains and the forests halved only on the owner's word, since it moves
    the land everywhere, asked. The first part at `3869d0c` on 2026-10-24, the banks sloping a block
    per two, the spawn's square at 460 steps of two or more where 1,458 stood and the village at 1
    where 116; the second part on the owner's word.

149. **The flood and the casters' search at a crossing leave the frame.** From item 145's reading on
    2026-10-30, a step of one section over loaded land costing the game's flood culling and the sun
    casters' search 9 to 15 ms of the frame when a section is first crossed. The two run off the
    main thread or spread over frames at a crossing, the frame keeping the last result meanwhile;
    read by the same step test, the frame's slowest and 99th part at 8 and 12 columns before and
    after with the engine's field cost set apart. One or two commits.

## Verdicts

No verdict is open. Verdicts 1 to 7 are settled, said under each in REVIEW-ARCHIVE.md.

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
README's pictures taken again, an animation at its head and pictures in its parts (Decision 24),
each in full under Decisions in REVIEW-ARCHIVE.md.

47. **The devlog written as the work goes.** The owner asked on 2026-10-25 at 05:13 that the
    sessions write the devlog more often than at a day's end, though not at every commit. N 4.8
    gains the cadence, an item that changes what is seen ending with at least one captioned picture,
    a pair before and after where it mends and one a phase where it is built in phases, and
    DEVLOG.md regenerated by the review as each item is archived, the coder's next commit carrying
    it as it carries the review's files; the three devlogs were regenerated at 05:17 with the day's
    pictures and items. The same in 3DEngine (Decision 71) and BevyCSharp (Decision 64).

48. **The screen probes are on, off a debugging switch alone.** The owner wrote to the engine's
    session before 09:19 on 2026-10-25, with the engine's item 103 held on its cost, that the screen
    probes stay on as the engine recommended and that off is for debugging alone and not for a
    player. Decision 42's default of off, taken when the engine's item 90 spread black across the
    screen, is over, item 90 mended since; item 131 turns them on and takes the setting out of the
    options.

49. **The devlog carries notes beside the pictures.** The owner asked on 2026-10-25 at about 12:01,
    shown the table of the screen probes' cost at four resolutions from 3DEngine's item 122, that it
    be written in a devlog or something along those lines. N 4.8 gains the note, a reading worth the
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
    screen, voxel.world keeping them meanwhile; open to the owner's word.

51. **The far cascade is on by default and the shadow distance is 128 blocks.** The owner wrote on
    2026-10-30 that the far cascades should be default on and the shadow distance 128 blocks, over
    the reviewer's call of 2026-10-26 under item 27 that kept the far cascade off for its 1.6 to 2
    ms; item 141 does it and reads the frame again.

52. **A voxel engine in Rust is the reference for the game's performance.** The owner pointed on
    2026-10-30 at github.com/rornic/ochre for the voxel sandbox's performance; the game's session
    reads it for what it does about meshing, drawing and streaming that the game does not, and
    proposes items from it with readings, the engine taking what is the engine's through ASKS.md.
    Read on 2026-10-30, six proposals in the order the readings say binds the frame: the game's
    sections drawn in groups (item 143), shadow casters culled to each cascade's slice swept toward
    the sun (the engine's, by ASKS.md), faces merged where the triangles halve (144), streaming by a
    time budget (145), packed vertices (the engine's, by ASKS.md) and the far columns at double
    cells (146); the reference has no occlusion culling, where the game's flood culling stands
    ahead.

53. **No far land in the game.** The owner wrote on 2026-10-30, "also no farlands in voxel sandbox
    to be clear", holding to Decision 41 of 2026-10-24 that took the far land out; item 146, the
    columns past 8 drawn at double cells from the Rust engine's octree levels, is withdrawn, and no
    level of detail past the render distance is proposed again without the owner's word.

54. **Held lights are plain lights without shadows.** The owner wrote on 2026-10-30 that the
    handheld lights in the voxel sandbox are simple lights and are not to be given shadows, over
    item 74 of 2026-10-24 where the held light cast them; item 147 takes the shadow off.

## Replies
