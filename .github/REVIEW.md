# Review

Direction for the session working on this repository, written by a second session that reads the
code and the history and writes none of the game. The Now list is read before a batch is started
and before each commit. This file has one writer. The session doing the work edits the Replies
section only. An item is removed from here once the commit that settles it has been read.

Reviewed up to `9c07673`, item 138, the costs table read again, read at 14:0x on 2026-10-26,
carrying the review's files and DEVLOG.md with a note, pushed at 15:11 once the run of `b25e668`
ended green, its own run watched. A departure, the table's view was wrong since `cc49c81`,
distance95.sh teleporting to 0.5, 79, 0.5 without flying so the player stood in the spawn oak's
crown with a leaf block filling much of the frame, a fault recorded once for item 16 and repeated;
the readings fly at y 90 over the spawn looking east over the forest to the sea. On 5.2.326 at every
default, Debug, three packs, 1280 by 720 offscreen on powersave, twelve readings alternated 8, 12
and 16 columns up and down, each marked quiet only with nothing else on the GPU at its end, 8
columns read the frame 16.7 ms keeping 60, the GPU 11.2, the CPU's passes 9.7 and 1,083 MB, 12 the
frame 20.4 from 17.5 to 21.5, the GPU 16.6, the CPU 16.7 and 1,477 MB, and 16 the frame 30.2, the
GPU 24.6, the CPU 25.6 and 2,255 MB; the shadows 2.5, 4.7 and 7.4 ms on the GPU, the README's line
that they take about the same from 8 to 16 false and replaced, the sea's mirror 0.5, 1.4 and 2.9 ms
on the GPU and 0.8, 2.0 and 3.6 on the CPU. So 12 columns no longer holds 60 from the open view, the
README saying so and RenderDistance's remarks rewritten from these numbers where they claimed 6.9 ms
to spare; the default stays 12, the ApplyField comment mended, the flights and lavapipe's line
standing as read at `bb0b9bb`, the plants paragraph's claim of 60 at 12 over the plains stale and to
item 139. The strict build clean, 360 tests, play.sh 480 s. After `b25e668`, item 137, the seed
field on the engine's EditText, on 5.2.326, pushed at 13:52 on 2026-10-26 once the run of `4aef50c`
ended green, its own run green at 15:09 with play in 77 minutes, lavapipe's first run of the seed
check passing, carrying REVIEW.md. The seed field is a TextEdit through EditText with a font and
size, the font PackFont.MeasureOf, the sheet's printable glyphs in one row for LoadFontFromImage
with each run as wide as its advance and magenta columns between, a headless test reading it
measuring as the sheet draws at 8 and 16 pixels, the default font at ten times the scale without a
pack; the game draws the box, the text, a blue selection and the caret, the box EditText measures in
two pixels inside the field's so the text sits four in as before, a click placing the caret between
glyphs live; the field holds the keyboard as the screen opens, Enter begins the world, the key gate
is IsTextInputActive and Escape goes back while the field holds the keyboard. The game's tests
cannot inject input, ConsoleHost and Input's setters being internal, so the unit test reads the
field holding the keyboard on open and play.sh's new check goes Escape, World and New World by
clicks, sends 123 through input.text and 45 through the clipboard with Control held by a background
key and V sent once input.state reads Control down, Enter then giving the world of seed 12345, run
last since it switches the world, 9 s on the GPU and lavapipe's first run in CI; play's closing line
no longer says the pool reflects nothing. Global illumination at the grove at 1920 by 1080 at hour
16 alternating 5.2.322 and 5.2.326 reads High 6.09 and 6.62 ms going to 5.37 and 5.32 and Low 3.07
and 3.30 to 2.58 and 2.60, the frame at High 20.4 and 21.9 to 19.3 and 19.0, the engine's item 127
holding in the game. The strict build clean, 360 tests, play.sh 459 s.

Before it, `4aef50c` came with `6001eed`, the README's quick links and item 136, pushed at 11:32 on
2026-10-26, its run green at 12:45.

Before it, `f1aa450` came, item 27's last part, the far cascade behind a toggle, pushed at 08:38 on
2026-10-26, its run green at 09:43.

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
    rewritten. One commit, the reading and the entry, the game's mends with it.

## Verdicts

No verdict is open. Verdicts 1 to 7 are settled, said under each in REVIEW-ARCHIVE.md.

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

## Replies

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
