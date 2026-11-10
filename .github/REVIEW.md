# Review

Reviewed up to `cfffcb3`, the pin of the engine's 6.2.12 with item 165, the build on a push to a
branch alone, and the workflows' engine checkout at `94097b62`, pushed at 02:54 on 2026-11-10, its
run green at 04:05 with the play in 70 minutes and the three test jobs in about two; its strict
build is clean, 364 tests pass and the scripted run passes under the validation layer in 336
seconds; it keeps the form and carries the review's files and item 166's two Replies. Before it
`4e956bc`, items 162 and 163, pushed at 08:47 on 2026-11-09, green at 10:07. The game stands at 0.2,
tagged `v0.2.0`, and is pinned to the engine's 6.2.12 since `cfffcb3`, the first package with the
flicker mend and the lamps; item 166's reads on it and on 6.1.12 show no path difference and no lag
at any speed, the clouds' step standing, and on the grove's shaded ground a seventh of the pixels
flickering in a turn by the screen probes on both, read again on the package that carries the
engine's ramp. Item 166 is read twice by 22:37 on 2026-11-09, turns clean at every speed, a walk
showing the difference between two settlings of one place in the screen probes' tiles, the engine's
item 246, and the clouds stepping as the far cascade moves, its item 245. Every push and run before
`4e956bc` since 2026-10-23 stands in REVIEW-ARCHIVE.md under Head records.

Next the pin of the package that carries the engine's ramp, with item 166's carried turn on the mask
read again on it, and 167 at the pin that carries the engine's 244. Item 29's second part and 114's
second part wait on the owner's word. A program on the GPU runs under the lock (N 7.9).

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
    as it stood is in REVIEW-ARCHIVE.md under Items closed, parked or shortened.

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

166. **The illumination read in motion.** Decision 64 on 2026-11-09. The owner wrote at 17:25 on
    2026-11-09 that the game's global illumination has looked noisy the same way for weeks, and
    every reading here since 2026-10-24 was a settled still against a reference, which a fault in
    motion passes. One read on 6.1.12 at the game's settings, High, four cascades, a cell of 0.25
    and the far cascade on, at the grove, the village at dusk with its lamps and torches and a
    torch-lit cave, each by a scripted walk and turn recorded offscreen (N 7.8), about ten seconds
    standing, walking, turning by a fraction of a probe tile a frame, and the two seconds after a
    stop; picture to picture, the share of pixels moved by 12 and by 25 levels in each of the four
    states against the same poses settled, and the frames where the share jumps with what moved in
    them, the probes' tiles, a cascade's arrival or the lamps. A sheet by N 4.9 and the frames in
    the dump by N 4.10, the sheet sent to the owner. Each fault found is placed against the engine's
    items, 237's cycles, 239's arrival after a cut and 240's bands on thin shapes, or is a new ask
    in the engine's ASKS.md with its frames, and the read is taken again at the next pin, the first
    package that carries 237. The village's lamps and torches gain reach with the engine's 241,
    whose floor follows the Field Cell setting, about 8.7 blocks at 0.125 and 17 at 0.25, which the
    engine's guide says beside the setting and the game's options leave unsaid, having no help line
    for a setting. The GPU is shared with the engine's suite, so the recording runs while it is
    quiet. After 165, before the next pin. Read at 22:05 on 2026-11-09, the sheet
    22-05-item-166-sheet in the dump and sent to the owner. Standing moves no pixel past 12 levels
    in the grove and the cave; a frame in motion against its own pose settled moves 0.55 to 1.03
    percent of the grove's pixels past 12 in the screen probes' tiles, the kind of the engine's 240,
    and under 0.11 percent in the village and the cave; the picture holds within three frames of a
    stop; no cycle of 237's and no arrival of 239's shows. One step, the near clouds losing about 15
    levels in a frame as the far cascade moves a cell, is the game's ask of 2026-11-09 in the
    engine's ASKS.md and the engine's item 245. The script walked 0.07 blocks and turned 0.25
    degrees a frame, slower than a hand, which was the review's wording of the turn. A second and
    last read by Decision 64 takes what play has and the script left out, under the lock, the grove
    with a turn of 90 degrees in two seconds and in half a second and a sprint, the day's clock
    running where the first runs held it, and the cave walked at night with a torch in hand; its
    shares by speed stand beside the first read's, and after it the owner's own recording is the
    next step. The second read at 22:37 on 2026-11-09, the sheet 22-37-item-166-sheet-second sent to
    the owner. Turns from a settled place match their settled poses within 0.06 percent past 12 at
    0.75 and 3 degrees a frame, and the cave walked at night with a torch in hand within 0.12. A
    walk's and a sprint's 0.55 to 1.0 percent is the size of the difference between two settlings of
    one place, 0.35 walked to against teleported to and 0.69 between two runs, in the screen probes'
    tiles, the game's second ask in the engine's ASKS.md and the engine's item 246. With the clock
    running the sun's light steps every 83 frames as Sky.cs sets it, 36 steps an hour so that a
    shadow's edge stands still between steps, each step moving 0.49 to 0.60 percent of the window in
    its frame with a tail under 0.03 for up to 40 frames, the game's own choice and left. Item 166
    done as read; the engine's 245 and 246 carry what it found, the script run again at the pin that
    brings them, and the owner's own recording stays asked for. The engine's read of its 246 on its
    head at 23:11 found no such difference in Sponza or in its Ore example, a settled probe's light
    being the mean of its sixteen places since its 237, so the pin of the package from the engine's
    `94097b62` repeats the walked-to against teleported-to read in the grove first, and where it
    still shows the engine takes a grove-like scene with leaves that let light through into its
    tree. Corrected at 03:11 on 2026-11-10 by the read on the pin. Neither package shows a path
    difference, one place teleported to twice and in two fresh runs within 0.006 percent on 6.1.12
    and 6.2.12 alike and the two packages within 0.005 of each other; the 0.35 and 0.69 percent were
    the script's poses, the walk's end summed in floats against a teleport rounded to a hundredth,
    half a millimeter and 3.6 mm apart, and walked to the exact end the place reads 0.000. Half a
    millimeter of the eye moves 0.34 percent of the window past 12, about half of it the sun's
    shadow edges a texel on as a cascade's middle is floored to whole texels and the rest the screen
    probes' light. So the engine's 246 is withdrawn, the second read's walk and sprint figures carry
    the same pose error, and every held pose of the rest is placed by the game's own float
    operations. The read that remains for the owner's words is the engine's measure of motion, a
    turn in place with each picture carried into the next by build/flicker.py of the engine's tree,
    in the grove with the screen probes on and off, the share past 4 and past 12 and its map, beside
    the walk, the sprint and the turns against their poses held, the clouds and the cave. The
    carried turn read at 03:29 on 2026-11-10 on 6.2.12, the grove turned 0.75 degrees a frame over
    120 frames. The carry leaves a floor of 9.8 percent past 4 in the game, the pixel textures' hard
    texel edges smeared by the resampling, where Sponza's is 0.9; with both pictures of a pair
    blurred over a tile the floor is 0.95 and the screen probes read 1.96 percent past 4 and 0.089
    past 12 where 0.954 and 0.011 with them off and 0.979 and 0.010 with the illumination off, so
    the probes double the turn's flicker past 4 and multiply it by eight past 12, small over the
    whole window. It lies in blocks of one or two tiles on the shaded ground under the canopies,
    among the grass tufts and the petals, and along the canopy's shaded edges, the kind of the
    engine's 240. The share on a mask of the shaded ground under the trees says how much it is where
    it lives, on both packages, and the read is taken again on the package that carries 240's ramp.
    On the mask at 03:43 on 2026-11-10, the grass ground with petals under the trees where its
    blurred luma lies under 80 of 255, some 74,000 pixels and 8 percent of the window, carried with
    the turn. With both pictures blurred over a tile the ground reads 20.2 percent past 4 and 1.44
    past 12 with the screen probes on, 6.0 and 0.012 with them off and 8.4 and 0.059 with the
    illumination off, and 15.1 percent of the ground past 4 and 1.43 past 12 move with the probes
    and not without them, where 0.65 and 0.001 between the two runs without them; 6.1.12 reads the
    same to a tenth, so 6.2.12 does not touch it. A seventh of the shaded ground flickering in a
    turn by the probes alone is the likeliest reading yet of the owner's words. The same mask and
    turn are read again on the package that carries the engine's ramp of its 240, and its figure
    there against 6.0 and 0.012 says whether the engine's held form is taken up. The ask for
    build/flicker.py's three options is in the engine's ASKS.md. The poses held read again at 05:15
    on 2026-11-10 on both packages, each pose placed by the game's float operations. Every frame in
    motion matches its pose held within 0.08 percent past 12 and 0.061 past 25, the walk, the
    sprint, the turns at 0.25, 0.75 and 3 degrees a frame and the cave with the torch, where the
    floor of a still view with the clock running is 0.04 to 0.05, so no light lags at any speed and
    the first reads' walk and sprint figures were the pose error. The cave at night is brighter on
    6.2.12 at every pose, its mean luma 3.26 to 3.76 and 8.51 to 10.00 at the ends, the engine's 241
    reaching further. The clouds step as before on both, the engine's 245. What remains of the item
    is the carried turn on the mask, read again on the package that carries the engine's ramp.

167. **The engine's color functions and the field's center ahead, at the pin that carries them.**
    From the engine's item 244 on 2026-11-09 (its Decision 108). The game writes the sRGB curve out
    in SkyColors.cs and TerrainAtlas.cs and a gamma of 2.2 in SectionMesher.cs and Sky.cs, and
    places the scene field half the finest cascade ahead of the eye by the engine's 64 cells times
    its own cell size in LightSettings.cs. With the package that carries the engine's 244 the game
    calls the engine's functions where it means the sRGB curve, a 2.2 that is the look's own staying
    with its reason in a comment, and takes the engine's option for the field's center, the play's
    captures unchanged. A small commit at that pin or the one after.

## Verdicts

No verdict is open. Verdicts 1 to 9 are settled, said under each in REVIEW-ARCHIVE.md.

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
    approval as its Decision 95's precedent allows. Item 166.

## Replies

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
