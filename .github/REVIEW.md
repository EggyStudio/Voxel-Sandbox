# Review

Direction for the session working on this repository, written by a second session that reads the
code and the history and writes none of the game. The Now list is read before a batch is started
and before each commit. This file has one writer. The session doing the work edits the Replies
section only. An item is removed from here once the commit that settles it has been read.

Reviewed up to `01eb71f`, item 128's fires, read at 02:01 on 2026-10-25 after the clocks went back
an hour, carrying REVIEW.md. Fire.Spread's overload is folded into its context system and
Fires.Update is gone, Fires holding no Random of its own, Light and Take taking one and the game
passing the step's, which is settable; FireFrameTests lays a 16 by 16 square of planks or stone
ringed by stone in the game's flat world, seeds the step's Random with 1, 2 and 3, lights a fire and
steps 2,400 frames of 0.05 s a seed with the bounds as before, FireTests keeping the two small-world
tests; the fixture gains a frame length and a target of 0, the headless app having paced itself at
60 a second so the fire tests took four minutes and take 13 s. 314 tests in 25 s, play.sh 335 s on
the GPU. The five pushed at 02:07 on 2026-10-25 after the run of `3275e76` went green; the run
watched. After `4105293`, item 128's first commit, the fixture and the mobs, read at 02:48 on
2026-10-25, carrying REVIEW.md. HeadlessGame sets the headless and the frame time variables, calls
InitWindow, builds a transient flat VoxelGame at render distance 2, steps game.Draw until it stops
loading within 600 frames and on dispose disposes the game and closes the window, its tests in a
collection run one at a time; the twins go, MobLife.Age's overload becoming the instance Pass the
Age system calls a mob, a lone mob's tests calling it through GetRef, and Think, Crowd, Vanish and
Hunt folded into their context systems, Vanish sending each MobDied as it despawns. MobFrameTests
reads a pig and a zombie in the air each taken by one of the two queries and falling within three
frames with the pose the movement wrote, a zombie beside a survival player taking it to at most 17
within 60 frames through MobStrike, and a pig killed by Hit standing after 15 frames and gone after
45 with its porkchop among the drops through MobDied. 314 tests in 15 s where 311 took 12 to 13,
each headless game about 2.5 s; play.sh 308 s on the GPU with the readings as before. After
`d647b0e`, the lookups, read at 02:40 on 2026-10-25, carrying the review's files. Fire is keyed by
its cell and Fires.Take asks the engine's index in place of building a set of every fire each frame
the world names a cell, each named cell still drawing its delay so the seeds' sequences hold; a test
reads a lit cell named again keeping its one fire, a new cell getting one, the index finding it by
its key and an empty cell not found; Mobs.Leaving keeps its walk with a comment pointing at the
engine's item 117, the README's Fire section gaining a line. 312 tests, play.sh 325 s on the GPU.
The entities' tests through the engine's stepped app are item 128, planned in the item. After
`fb04f6b`, item 105, the stepped shadow read once, read at 02:33 on 2026-10-25, carrying the
review's files and the devlog's grove. litStepped reads lightReaching once at the step's middle with
its held share and calls litLight with the shadow and the held, its own correction and the sun's
specular rebuild gone, passing minus 1 where there is no shadow light; the grove alike to the pixel
but for 96 of 2.07 million, the pillar's shadow still stepping and the lantern's pool and the grass
wall as before. The scene pass at the grove reads 9.87 and 8.96 ms with one read against 10.19 and
9.38 with three, 0.3 to 0.4 ms where the item expected 1.6 measured on the older pass of 12.5, and
the step costs nothing measurable against no step, the README's costs paragraph rewritten. 311
tests, play.sh 324 s on the GPU. The lookups next, the fires keyed by cell through the index and the
mobs still walked since a key names one entity, an index whose key names many asked of the engine as
its item 117. After `c7c1b41`, item 123, the culled sections casting the sun's shadow, read at 02:15
on 2026-10-25, carrying the review's files and the devlog's cave tops. ChunkRenderer.FindCasters
casts a ray toward the light from each of the eight corners of every drawn, meshed section within
the shadow distance plus 16 blocks, shared corners once, each ray walking the section grid after
Amanatides and Woo up to the world's top or as far as the loaded land reaches and marking every
section the flood left out, the marked sections drawing their meshes through the same materials with
ShadowOnly true, taken off the unseen set and meshed like the drawn; the marking redone when a flood
runs, the light's step turns or the shadow distance changes; draw.sections.shadow counts them and
Casts switches the casting off for a reading. Departure, the casters found by marching from the
drawn sections' corners toward the light rather than sweeping the drawn set's bounds, which hold
thousands of sections under open land casting onto nothing. The cave at the entry's place reads the
tops 45 blocks ahead 0, 0, 0 where 35, 29, 20 stood, the same at a shadow distance of 150 and the
same as with the culling off, the 90-block tops 9, 13, 18 where 19, 20, 22, the band's pixels lit
above 24 falling from 4,427 to 4 at 96 and from 2,325 to 4 at 150, the frame 4.6 ms where 3.1 stood
with 357 shadow-only draws and 6.7 at 150, against 28 to 31 with the culling off; the grove draws 20
shadow-only and reads the frame the same with the casting on and off within one session.
VisibilityTests walks the march, the README's culling paragraph gains the casters and the numbers.
311 tests, play.sh 327 s on the GPU.

Before it, `3275e76` came, the mobs' third commit and item 27's pin of 5.2.291 with `9dad43e`,
pushed at 02:09 on 2026-10-25 before the clocks went back, its run green, the play job 54.2 minutes.

Before it, `7443bdc` came, item 32's particles, pushed at 01:31 on 2026-10-25, its run green, the
play job 33.3 minutes.

Items done and verdicts settled before 2026-10-23 stand in REVIEW-ARCHIVE.md beside this file, their
numbers kept, moved there at 19:30 on 2026-10-23 so this file holds the current work alone and is
read whole before a batch; the archive is read when an item or a verdict named here is wanted in
full.

## Now

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
    comes, and SetUiHiddenKey(Key.Null) where it keeps F1.

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
    s locally.

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
    checkbox and no e3d verb.

92. **Far land goes (Decision 41).** The owner played on 2026-10-24 and found the far land looking
    bad, and said to remove it. FarLand, its rings, tiles and skirts, FarCache and the far folder of
    the save, the generator's Surface where the far land alone read it, voxel.far, the settings
    slider, the play check of the far land and the README's words and costs table go, the taller
    world and the sections by reach of item 26's first two parts staying; the haze ends at the near
    land as before item 26's third part; the README's world pictures taken again through pictures.sh
    without far land. One commit, after item 91. Done at `2263618` on 2026-10-24, the pictures taken
    again, old saves' far folder left unread.

97. **The game's entries in the engine's ASKS.md hold no colon inside a running sentence.** The
    owner wrote at about 10:3x on 2026-10-24 that some documents join sentences with a colon and
    that STYLE.md forbids it; the entries this session wrote in the engine's ASKS.md hold 24 such
    lines, found by STYLE.md's awk over the Markdown files outside their code. Each is recast as
    STYLE.md says, a sentence of its own or the same sentence recast, a label at a line's end left
    as it is, the Review lines being the reviewing session's; the recast goes into the engine's tree
    as the entries do and the engine's next batch carries it. Small, after items 92 and 93 and
    before item 94. Done in the engine's tree at 10:4x on 2026-10-24, 21 lines recast, the 15
    headings with a comma after the game's name and six sentences split or recast, the engine's next
    batch carrying it; the file's header lines are the engine's sweep's, item 93 there.

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
    Waits on the package carrying the engine's item 108; item 96 taken before it.

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
    The pin of 5.2.291 at `9dad43e` on 2026-10-25, the grove's frame as 5.2.283's.

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
    blocks next.

## Verdicts

No verdict is open. Verdicts 1 and 2 are settled, said under each in REVIEW-ARCHIVE.md.

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

## Replies
