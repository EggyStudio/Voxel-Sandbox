# Voxel-Sandbox devlog

What landed in Voxel-Sandbox, by day and newest first, with the pictures taken on the way, the
captures the repository keeps under `.github/assets` and the pictures made while an item was built,
kept under `.github/devlog` as WebP files named by their day, each on the day its name says, and the
notes written beside them as Markdown files named the same way. A line under Done names the item of
.github/REVIEW.md or .github/REVIEW-ARCHIVE.md and the commit that closed it.

## 2026-10-25

![](.github/devlog/2026-10-25-item-32-snow-layer.webp)
Seed 6's snowy village before and after three minutes of snow, the spruces' and the bushes' tops and
the ground laid with a layer an eighth deep, the stairs of the roofs taking none.

![](.github/devlog/2026-10-25-item-32-embers.webp)
A dark room of stone laid under seed 1's forest round a pool of lava, embers rising from it as they
cool, with the cave's dust lit only where the lava's light reaches.

![](.github/devlog/2026-10-25-item-32-falling-leaves.webp)
Leaves falling and turning under a canopy of oak leaves laid over seed 1's forest, with Falling
Leaves at 200%.

![](.github/devlog/2026-10-25-item-32-rain-splashes.webp)
A river of seed 1 from over its bank under the rain, before on the left and after on the right,
rings spreading on the water and drops leaping from the bank where the rain meets them.

![](.github/devlog/2026-10-25-item-27-grove-291.webp)
Seed 1's cherry grove at 1920 by 1080 at High on the 3DEngine 5.2.283 package and on 5.2.291, alike
to the pixel but for 129 of them, the frame 19 to 21 ms on the first and 18 to 21 ms on the second.

![](.github/devlog/2026-10-25-item-123-cave-tops.webp)
The top 200 rows of seed 1's deep cave at -59.5, -4, -91.5 looking 0 -30 at noon with the shadows
reaching 96 blocks, brightened two and a half times, before above with the sun lighting the tops and
their edges through 70 blocks of rock left undrawn, and after below with that rock drawn into the
shadow maps alone and the tops dark.

![](.github/devlog/2026-10-25-item-105-grove.webp)
Seed 1's cherry grove at 1920 by 1080 at High with the sun's stepped shadow read three times a pixel
on the left and once on the right, alike to the pixel but for 96 of them.

![](.github/devlog/2026-10-25-item-36-views.webp)
Seed 1's plains at nine in the morning seen from behind the player as it walks, on the left, and
from in front of it looking back, on the right, the camera four blocks from the eye and the player
in the game's own colors.

![](.github/devlog/2026-10-25-item-36-poses.webp)
The player's poses on seed 1, mid-stride from behind as it walks sideways on a floor of stone in the
sky and mid-swim from behind in a pool, above, and from in front holding a block on the plains and
the empty right hand from the eye, below.

![](.github/devlog/2026-10-25-item-28-bloom.webp)
The lantern and the soul lantern of that room with no bloom on the left and a bloom of 0.6 on the
right, a halo about each flame's glass.

![](.github/devlog/2026-10-25-item-28-lamps.webp)
A room of stone in the sky over seed 1 at midnight, a lantern and a soul lantern on its white floor
and glowstone in its ceiling, each block's light its own color on the left and white as bright on
the right.

![](.github/devlog/2026-10-25-item-28-rim.webp)
The oaks of seed 1's coast at noon without a rim on the left and with a rim of 100% on the right, a
lit line along each block's edges.

![](.github/devlog/2026-10-25-item-28-unshaded.webp)
The same coast as the game shades it on the left and unshaded on the right, every open face taking
the sun's light as one facing it does.

![](.github/devlog/2026-10-25-item-31-mineshaft.webp)
Seed 1's nearest mineshaft under the forest, a corridor with its rails, its posts of oak under beams
of planks and a lantern hanging from one, on the left, and a chest in a niche of another, lit by the
torch held and one set on its floor, on the right.

![](.github/devlog/2026-10-25-item-31-kinds.webp)
Seed 1's nearest of the new kinds at noon, a mossy temple in the jungle, a witch's hut on posts in
the swamp and a pillagers' tower on open land above, and under the sea a ruin of broken walls and a
wrecked ship with its mast below.

![](.github/devlog/2026-10-25-item-31-villages.webp)
Seed 1's nearest village of each kind at ten in the morning, the desert's of sandstone with corners
of yellow terracotta and the savanna's of acacia on floors of orange terracotta above, the taiga's
of spruce and the snowy plains' under roofs of snow below.

![](.github/devlog/2026-10-25-item-129-rain.webp)
Rain over seed 1's plains village at noon seen against the sky through its nearest columns, twice as
large, its streaks standing upside down with their faint ends below on the left and the right way up
on the right, faint at their tops and brighter at their feet.

![](.github/devlog/2026-10-25-item-30-models.webp)
A villager, a chicken, a sheep, a pig and a cow at noon with a zombie, a skeleton, a creeper and a
spider under a roof behind them, in the game's own models above and in Fresh Animations' models
under Bare Bones' pictures below, walked by the game's own gait.

![](.github/devlog/2026-10-25-item-130-cave-haze.webp)
Seed 1's lava lake at noon seen from in its cave, the far wall at the top left in the sky's haze on
the left and black on the right, where the haze follows the sky's light that reaches the wall or the
eye.

![](.github/devlog/2026-10-25-item-131-probes.webp)
Seed 1's plains village at noon with the screen's probes off on the left and on, as the game starts,
on the right, the light much the same in the open at noon.

# Item 95, the sections as entities

The frame at item 93's views, seed 1's cherry grove and plains village at hour 16, the three default
packs, render distance 12, High with the screen's probes, unlimited frames, offscreen on the RTX 4070
Laptop GPU, each a profile over 300 frames in a Release build, read twice for each build in the order
5.2.291, 5.2.313, entities, entities, 5.2.313, 5.2.291. Another program held 10 to 50 percent of the
GPU through the runs, so the ranges are wide and the frames longer than a quiet GPU's.

The builds are the game on 5.2.291 drawing every section through `DrawMesh`, the game on 5.2.313
drawing the same way, and the game on 5.2.313 with each section's solid faces, its faces a pack cuts
out that cast a whole shadow and its lava as entities. `draw.sections` is the game's main thread
drawing the sections each frame. Times are in milliseconds.

| Build | View | Frame | Scene pass | Shadows | Global illumination | `draw.sections` |
|---|---|---|---|---|---|---|
| 5.2.291 | grove at 1920 by 1080 | 41.5 to 46.6 | 19.8 to 23.6 | 5.8 to 8.9 | 4.5 to 6.4 | 1.81 to 5.96 |
| 5.2.291 | grove at 1280 by 720 | 29.7 to 31.0 | 14.4 to 15.7 | 7.3 to 7.7 | 4.3 to 5.0 | 1.59 to 1.85 |
| 5.2.291 | village at 1920 by 1080 | 33.5 to 34.8 | 19.8 to 19.9 | 6.9 to 7.1 | 4.1 to 4.4 | 1.48 to 1.68 |
| 5.2.291 | village at 1280 by 720 | 27.5 to 29.5 | 14.7 to 15.2 | 7.1 to 7.2 | 3.8 to 4.5 | 1.27 to 1.59 |
| 5.2.313 | grove at 1920 by 1080 | 35.1 to 45.7 | 19.8 to 21.2 | 6.2 to 6.4 | 4.5 to 4.8 | 3.64 to 5.66 |
| 5.2.313 | grove at 1280 by 720 | 29.1 to 32.8 | 13.0 to 14.1 | 5.2 to 6.3 | 3.6 to 4.5 | 1.45 to 3.46 |
| 5.2.313 | village at 1920 by 1080 | 27.9 to 33.4 | 17.3 to 19.4 | 4.7 to 7.2 | 3.2 to 4.4 | 1.42 to 1.50 |
| 5.2.313 | village at 1280 by 720 | 20.3 to 29.2 | 11.5 to 13.0 | 4.7 to 6.5 | 2.7 to 3.9 | 1.22 to 3.43 |
| Entities | grove at 1920 by 1080 | 23.2 to 24.0 | 9.8 to 10.3 | 5.5 to 5.6 | 4.7 to 4.8 | 0.68 to 1.50 |
| Entities | grove at 1280 by 720 | 19.6 to 20.0 | 6.5 to 6.6 | 5.2 to 5.7 | 3.9 to 4.0 | 0.78 to 0.93 |
| Entities | village at 1920 by 1080 | 23.5 to 27.1 | 11.6 to 12.0 | 5.0 to 5.8 | 3.6 to 3.9 | 0.65 to 1.77 |
| Entities | village at 1280 by 720 | 19.2 to 20.0 | 7.1 to 7.5 | 4.8 to 5.5 | 3.1 to 3.6 | 0.71 to 1.54 |

The entities draw the same frame. At the grove, the village, the lava lake and the ravine the
sections, draws and triangles `voxel.state` counts are the same in both 5.2.313 builds, 1,673 draws
of 1.15 million triangles at the grove, and the captures differ by an RMSE of 0.0002 to 0.0011 with
at most five pixels more than 3 percent apart. The engine draws 1,059 entities at the grove and 976
at the village, and its `MeshEntityDraws` takes 0.25 to 0.33 ms a frame. With them the scene pass
takes about half its time on the GPU and the passes' recording on the CPU less than half, the
shadows' 4.5 to 6.1 ms against 8.6 to 11.9 at the grove. 5.2.313 alone takes 1 to 2 ms off the
shadows and up to 1.5 ms off global illumination against 5.2.291 within the noise of the load.

Done that day

- Item 27 at `9dad43e`. Caves as the source's newer ones, cheese, spaghetti and noodle, with
  aquifers.
- Item 28 at `79ad110`. The looks Shine shows, done as the game's own, bloom on emissive blocks,
  colored light from emissive blocks, a rim around blocks, and an unshaded look.
- Item 30 at `87d089e`. Entity models, textures and animations read from the player's packs in
  OptiFine's form, the game's own the fallback (Decision 13).
- Item 31 at `057011a`. Structures as the source's, more kinds and the modpack's taste (Decision
  15).
- Item 32 at `7443bdc`. Weather and the ambient particles, rain and snow by biome, falling leaves,
  cave dust and wakes (Decision 15).
- Item 33 at `e433e5c`. Ambient sound, footsteps by the block and the sound's occlusion and reverb
  by the room (Decision 15).
- Item 34 at `85c3ff6`. Connected textures from the packs' properties (Decision 15).
- Item 35 at `ce693af`. A zoom key (Decision 15).
- Item 36 at `6a56e0f`. The player seen in third person, with their own animations (Decision 15).
- Item 94 at `3275e76`. The things that move become entities with behaviors (Decision 43).
- Item 105 at `fb04f6b`. The stepped sun's shadow is read once a pixel through the engine's hook.
- Item 123 at `c7c1b41`. The sections the flood culls still cast the sun's shadow.
- Item 128 at `6a7e5b1`. The entities' tests step the real game through the engine's headless app,
  the twins gone.
- Item 129 at `bbe814c`. The rain's particle texture is drawn upside down.
- Item 130 at `0babebb`. The fog does not light a dark cave's far walls.
- Item 131 at `f97a4c9`. The screen probes are on, with no setting for a player.
- Item 132 at `1767348`. The tree's documents and comments keep N 4.7 and STYLE.md.

## 2026-10-24

![](.github/devlog/2026-10-24-breaking-pair.webp)
Breaking stone in survival, Bare Bones' destroy stage on the left and the game's own cracks with no
pack on the right, with the bits cut from the stone's side.

![](.github/devlog/2026-10-24-curve-choice.webp)
The plains village under the modpack's own tonemap curve at three saturations, beside today's
Reinhard look and the modpack.

![](.github/devlog/2026-10-24-hand-three.webp)
The hand in view under Bare Bones, holding stone, holding a stick a pixel deep, and the empty arm
wearing Steve's sleeve.

![](.github/devlog/2026-10-24-fire1.webp)
A fire lighting a closed stone room at night, its flames the source's models over the pack's
animated fire.

![](.github/devlog/2026-10-24-use2.webp)
The fires a TNT's blast leaves in its crater.

![](.github/devlog/2026-10-24-drag-pair.webp)
A dragged stack over the inventory, its shares lit on the slots it passes (item 67).

![](.github/devlog/2026-10-24-fell-pair.webp)
An oak before and after felling, its logs taken and a log landed (item 69).

![](.github/devlog/2026-10-24-fire-all.webp)
Fire spreading over planks by the source's odds, from the first flame to the burn-out (item 72).

![](.github/devlog/2026-10-24-hand-four.webp)
The hand in view in four states, with stone, with a stick, empty and with the HUD hidden (item 75).

![](.github/devlog/2026-10-24-swell-all.webp)
The creeper's swell and flash before it goes off, and its blast's cloud (item 73).

![](.github/devlog/2026-10-24-tnt-all.webp)
TNT from its placing to its crater, the fuse, the flash, the swell and the blast (item 73).

![](.github/devlog/2026-10-24-views-40.webp)
The world views retaken at a field of view of 40, the village views moved (item 46).

![](.github/devlog/2026-10-24-views-70.webp)
The world views at a field of view of 70, before the retake (item 46).

![](.github/devlog/2026-10-24-trees-cmp.webp)
Trees before and after the caves and ravines carve ahead of them, the coast by seed 1's first ravine
in the middle row where leaf clusters floated before, and the ravine's magma under the canopy below
(item 99).

![](.github/devlog/2026-10-24-item-103-fire-line.webp)
A fire under a resource pack at night, read while a carried texture's place came to be kept half a
texel inside its tile, so a fire's walls draw no line along their tops.

![](.github/devlog/2026-10-24-item-107-canopy.webp)
A jungle's floor under its canopy before and after the sky's light lost a level on the way in and
one past the first layer, and with no scene field.

![](.github/devlog/2026-10-24-item-108-clouds.webp)
The clouds before and after they reached twice the render distance, a change taken back, since they
read better covering the land about the player than reaching the horizon.

![](.github/devlog/2026-10-24-item-109-cactus.webp)
The cactus as the source's model, its sides a pixel in and the thorns standing out at its edges,
from its corner and from above.

![](.github/devlog/2026-10-24-item-110-bits.webp)
Grass broken in the plains and in the savanna, its bits taking each climate's grass as its faces do.

![](.github/devlog/2026-10-24-item-111-fell.webp)
A felled tree 8 and 16 ticks after the cut, before and after its crown was read as it fell and its
leaves went one after another over the second.

![](.github/devlog/2026-10-24-item-114-cliff.webp)
A river's valley before and after it widened with the depth it sinks and its floor fell to the water
as a bank of about a block down for two across.

![](.github/devlog/2026-10-24-item-114-wide.webp)
The rivers about the spawn before and after, the water in blue and the banks still too steep in red.

![](.github/devlog/2026-10-24-item-115-torch.webp)
A wall torch from the front and the side before and after its wood came to lean up to its upright
lit head.

![](.github/devlog/2026-10-24-item-116-tools.webp)
The iron tools held before and after each was turned half about its handle, which brings an axe's
blade ahead of the handle.

![](.github/devlog/2026-10-24-item-117-floor.webp)
The jungle's and the plains' floors before and after a face's light was kept no less than the sky's
light at its sky level on the source's curve.

![](.github/devlog/2026-10-24-item-118-held.webp)
A held item's picture close up before and after each pixel became a cube whose faces share their
corners with the faces they meet, the cracks between them gone.

![](.github/devlog/2026-10-24-item-119-lines.webp)
The deep cave's lines of light along its blocks' edges, the sun's map and not the mesh, since a test
holds that no corner of a section's faces lies inside another face's edge.

![](.github/devlog/2026-10-24-item-120-name.webp)
The chosen item's name over the hotbar, which shows as the choice changes, stays a second and a half
and fades over half a second.

![](.github/devlog/2026-10-24-item-121-shade.webp)
A tree's shadow on a village's grass at noon before and after the sky's light on the land was drawn
toward a white of its own brightness, the shadow now the ground darker and not blue.

![](.github/devlog/2026-10-24-item-122-loading.webp)
The loading screen a world opens behind in the red of the source's own loading screen.

![](.github/devlog/2026-10-24-item-27-far-cascade-cave.webp)
The deep cave at noon on the 3DEngine 5.2.215 and 5.2.281 packages, its block tops 45 blocks off lit
by the sun's map on both.

![](.github/devlog/2026-10-24-item-27-grove.webp)
Seed 1's cherry grove at 1920 by 1080 at High on the 3DEngine 5.2.215 package and on 5.2.281.

![](.github/devlog/2026-10-24-item-93-shadows-cherry.webp)
The cherry grove with the sun's shadows reaching 48, 64 and 96 blocks from left to right.

![](.github/devlog/2026-10-24-item-93-shadows-hills.webp)
The hills with the sun's shadows reaching 48, 64 and 96 blocks from left to right.

![](.github/devlog/2026-10-24-item-93-shadows-village.webp)
The plains village with the sun's shadows reaching 48, 64 and 96 blocks from left to right, read to
choose 64, which keeps the trees' shadows in view.

![](.github/devlog/2026-10-24-item-98-tunnel-short.webp)
A broken block's bits in a short tunnel, lit as the engine lit them beside lit at each bit's middle,
read while the bits came to take the shadow and the probes at their middles.

![](.github/devlog/2026-10-24-item-27-grove-283.webp)
Seed 1's cherry grove at 1920 by 1080 at High on the 3DEngine 5.2.281 package and on 5.2.283, alike
to the pixel but for 324 of them, the frame 33 to 35 ms on the first and 19 to 22 ms on the second.

![](.github/devlog/2026-10-24-item-125-loading.webp)
The loading screen in the source's red and in black, the white words and the green bar reading
plainly on the black.

![](.github/devlog/2026-10-24-item-124-screens.webp)
The options with a button to each area's screen, Render Settings, Post Processing, Sky and Visuals,
the sky's colors and the noon sunlight's own screen, each in the source's buttons and sliders.

![](.github/devlog/2026-10-24-item-124-f3.webp)
The F3 window before and after the settings moved to the options' screens, keeping what the frame
holds and costs, the toggles of debugging and the world's kind, seed and mode.

![](.github/devlog/2026-10-24-item-126-video.webp)
The options with Video Settings among the areas' screens, and the screen of the window's mode, its
resolution, the frame limit and vsync.

![](.github/devlog/2026-10-24-item-94-arrows.webp)
A skeleton at night in the flat world, whose arrows, now entities of the engine's ECS, took eight of
the player's twenty points of health in five seconds.

![](.github/devlog/2026-10-24-item-94-tnt.webp)
A TNT lit in the flat world a half second after it popped up from its cell, burning as an entity of
the engine's ECS, which blew its crater five seconds on.

![](.github/devlog/2026-10-24-item-94-fires.webp)
A block of planks forty seconds into a burn from three fires, each fire an entity of the engine's
ECS, its top's 41 fires out of the view but for one.

![](.github/devlog/2026-10-24-item-70-grass-wall.webp)
A wall of grass blocks at noon seen from the player's height, as bright above the eye's line as
below it, the same texel of each block reading 186 to 192 in red from the line up.

![](.github/devlog/2026-10-24-item-70-plains-village.webp)
The plains village as the README showed it beside the same view taken again, its shadows the
ground's darker color and its river's banks widened since.

![](.github/devlog/2026-10-24-item-43-haze.webp)
The plains village at a quarter past six, at noon and at half past five in the evening, above before
the haze thinned with height and thickened toward dawn and below after, the dawn's mist hiding more
of the island.

![](.github/devlog/2026-10-24-item-43-saturation.webp)
The plains village at noon and at 17 with the grading's saturation of 1.4 on the left, which crushes
the dirt's blue to 0, and 1.2 on the right, the choice between them open.

![](.github/devlog/2026-10-24-item-42-water.webp)
The sea by an island at noon and at 17, a lush cave's pools and a laid pool of three depths seen
low, before on the left and after on the right, the sea deep and pale toward the horizon, the
seabed's steps gone under it, the cave's pools clear and the shallow lane's bed showing.

![](.github/devlog/2026-10-24-item-32-weather.webp)
Seed 1's forest from over its trees and seed 6's snowy village, clear on the left and under the
weather's full turn on the right, rain over the forest and snow on the village under a grayed sky.

![](.github/assets/world/plant-shadows.webp)
The plants cast the shadows of their blades and flowers on the 3DEngine 5.2.215 package, the terrain
shader cutting each wall's shadow by its picture while glass a pack cuts out still casts none, the
engine's own F2 and F3 are turned off since the game takes those keys, and the play script reads
across a poppy's shadow. (`d7c4892`)

![](.github/assets/world/lava-lake.webp)
Caves fill from the source's aquifers with water and lava to levels of their own and with lava under
-54, through a lava block meshed with its own emission, and play.sh keeps far land off but for one
check of it. (`9cb81ad`)

![](.github/assets/world/lush-cave.webp)
Lush caves lie under the wettest ground as the source's do, with moss and grass, cave vines whose
glow berries light them, and clear pools in rims of clay. (`c8dbbee`)

Done that day

- Item 26 at `3a99729`. The world's layout, sections streamed, meshed and saved in three dimensions
  with a taller world, and far land drawn at a coarser level by distance, the blocks' size kept.
- Item 43 at `4b398bd`. The sun's look, warm and low with long soft shadows, and a blue haze over
  distant land (Decision 21).
- Item 55 at `d7c4892`. The plants cast shadows, cut by their texture (Decision 27).
- Item 67 at `073d84b`. The inventory screens take no player key, draw a dragged stack every frame,
  and dropped items wear the pack's pictures.
- Item 68 at `8bc0796`. Breaking a block shows the cracks, throws particles of its side texture, and
  keeps its progress across jumps.
- Item 69 at `cc4f29a`. Felling a tree takes its logs and leaves, some logs falling, and gravity
  blocks fall (Decision 35).
- Item 70 at `8bcbfde`. The land's light is the same above the eye's height as below it.
- Item 72 at `ee557b3`. Fire, a block that burns, animated and emissive (Decision 34).
- Item 73 at `cba35ca`. TNT and the creeper's blast.
- Item 74 at `2aea2bf`. A held light lights the ground around the player.
- Item 75 at `1dce7b7`. The player's hand and held item in view.
- Item 76 at `7a0d61d`. The stepped shadows move in whole steps as the day passes.
- Item 78 at `b032e76`. Grass as the source places it, with the two-block grass.
- Item 79 at `526f20b`. Swimming as the source has it, no launch out of the water.
- Item 80 at `9b25aa3`. Villages spaced as the source spaces them.
- Item 81 at `8da806a`. Biomes placed by the source's climate, so a badlands does not border a
  forest.
- Item 83 at `dd64fd0`. Glass without the diagonal streaks, its borders connecting across neighbors.
- Item 88 at `b730088`. The play job comes back under 40 minutes.
- Item 89 at `cfdc298`. The clouds are not drawn black by the fire's uniform.
- Item 90 at `ba60ae4`. The frame at a render distance of 12 comes back under 16.7 ms with the
  taller world, the main thread's draws read first.
- Item 91 at `23aa4ff`. The screen probes stay off by default (Decision 42).
- Item 92 at `2263618`. Far land goes (Decision 41).
- Item 93 at `4a2cd7b`. The frame's cost on the development machine with the taller world, read and
  brought down.
- Item 96 at `2926dda`. The player and the game's states live in the ECS (Decision 43).
- Item 98 at `ccf6fcf`. The particles keep their light at every angle of looking.
- Item 99 at `dc2ad9f`. No tree stands cut by a cave or a cliff, and no leaves hang with no trunk.
- Item 100 at `54fc848`. Felling takes every log of the tree, and the crown decays within a second
  or two.
- Item 101 at `ad3499f`. Grass and the other plants give way to a block placed where they stand.
- Item 102 at `1f07038`. A held light shines from the player, not from the hand.
- Item 103 at `3236b56`. The flames' top row is the picture's own, with no stair-stepped line.
- Item 106 at `e82d965`. The corner shade stands at a half by default.
- Item 107 at `aaeaa2a`. The rain forest's floor is shaded, not dark, the canopy taking its levels
  at its edges.
- Item 109 at `02abb9d`. The cactus is the source's model, its sides a pixel in and their clear
  edges cut out so the thorns stick out.
- Item 110 at `2a399af`. The bits of a tinted block carry the biome's tint.
- Item 111 at `f00f649`. A felled tree's crown drops as its logs fall, not after they land.
- Item 112 at `fd17371`. Felling wears the axe by the logs it drops as items, the falling logs not
  counted.
- Item 113 at `031d169`. Pyramids are as rare as the source's.
- Item 115 at `65eec11`. A wall torch's wood leans up to its head's foot, and the lit head alone
  stands straight.
- Item 116 at `cce1536`. A held axe faces the right way, its blade forward.
- Item 117 at `f480dbf`. A face under a canopy is lit at least by the sky's light at its level.
- Item 118 at `9a3d746`. A held tool's pixels meet with no gap along its borders.
- Item 119 at `ffa7a7b`. The terrain's faces meet with no crack, so no light shines through a dark
  cave's walls in pixel-wide lines.
- Item 120 at `7ece2c1`. The selected item's name over the hotbar fades away.
- Item 121 at `4142a32`. A shadow darkens the ground and keeps its warmth, the sky's blue out of it.
- Item 122 at `e04a1a5`. The overworld's loading screen is the red of the source's loading overlay.
- Item 124 at `5f7bad8`. The settings live in the game's own screens, by area, and F3 keeps only
  what is read.
- Item 125 at `e643539`. The loading screen is pure black.
- Item 126 at `80d149e`. A Video Settings screen holds the resolution, the window mode, the frame
  limit and VSync.
- Item 127 at `d6532d1`. The settings are kept across runs.

## 2026-10-23

![](.github/devlog/2026-10-23-grass-fixed-crop.webp)

![](.github/devlog/2026-10-23-grass-now-crop.webp)
The grass before and after its tint followed the biome (item 70).

![](.github/devlog/2026-10-23-lamps-crop.webp)
A lantern beside a glowstone block, the small emitters' light (item 39).

![](.github/devlog/2026-10-23-pool-before-crop.webp)
A pool before and after its surface stopped reflecting the scene field, the water rough for the
present (item 86).

![](.github/devlog/2026-10-23-pool-rough-crop.webp)

![](.github/devlog/2026-10-23-village-ab.webp)
A village before and after the grass and the paths were mended (item 70).

Done that day

- Item 39 at `710fe0d`. Cutout textures, a texture a face for the lamp's kind, and animated
  textures.
- Item 71 at `fd276d3`. The stars are points of light with no dark backing, faded by the sky's
  light.
- Item 77 at `532e45c`. The distance fog takes the sky's color at the ray's height, so a mountain's
  top is not the horizon's color.
- Item 82 at `f9ab3bb`. No plant stands in the air where a cave was carved.
- Item 84 at `a9b0fcf`. A loading screen while the world generates.
- Item 85 at `2f9ed14`. No line on a grass plant's top.
- Item 86 at `10cf3f3`. Glass and water reflect nothing for the present (Decision 36).
- Item 87 at `6454b80`. The game's own plant masks show only where no pack gives the plant's
  picture.

## 2026-10-21

![](.github/devlog/2026-10-21-edges.webp)
At a render distance of 4, from high, from low and at night, the land fades into a dome of the
haze's color below the horizon, with no dark band.

![](.github/devlog/2026-10-21-hud-sheet.webp)

![](.github/devlog/2026-10-21-menus-sheet.webp)

![](.github/devlog/2026-10-21-own-art.webp)
The game's own art with no pack. The planks in this sheet are the first try; they became boards
after.

![](.github/devlog/2026-10-21-packs-interface.webp)
With the packs Bare Bones, Recolorful Containers and qrafty's font, Bare Bones' HUD and buttons,
Recolorful's inventory and furnace, and the pack's font.

![](.github/devlog/2026-10-21-rows2.webp)
Plants as flat crossed planes, lit as brightly as the ground, the game's own art on the left and
Bare Bones on the right.

![](.github/devlog/2026-10-21-trees-sheet.webp)
Acacias in the savanna, the cherry grove's pink crowns, the dark forest with giant mushrooms, and
the jungle floor under its canopy, all with Bare Bones.

![](.github/devlog/2026-10-21-windows-sheet2.webp)
The new interface, the inventory, furnace and creative windows, the Escape menu and options, and the
HUD with its hearts fading when full and returning after a hit.

![](.github/devlog/2026-10-21-bb-above.webp)
Bare Bones from above, the cliffs and the sea (item 24).

![](.github/devlog/2026-10-21-blocks-crop.webp)
A door, a trapdoor and a furnace among the blocks (item 39).

![](.github/devlog/2026-10-21-craft-crop.webp)
The inventory's crafting grid making planks (item 20).

![](.github/devlog/2026-10-21-creeper-ab.webp)
The creeper before and after its face was redrawn (item 61).

![](.github/devlog/2026-10-21-faces-crop.webp)
The zombie and the villager with their faces at a sixteenth (item 62).

![](.github/devlog/2026-10-21-green-compare.webp)
The grade compared on the plains village, the greens under the candidate settings (item 58).

![](.github/devlog/2026-10-21-hud-0-crop.webp)
The HUD with the hotbar, the hearts and the held block's name (item 23).

![](.github/devlog/2026-10-21-modes-sheet.webp)
Survival and creative told apart by the HUD, the hearts and the hunger (item 19).

![](.github/devlog/2026-10-21-outline-sheet.webp)
The block outline on a stair, a slab and a fence (item 21).

![](.github/devlog/2026-10-21-own-table-crop.webp)
The game's own crafting table art with no pack (item 24).

![](.github/devlog/2026-10-21-previews.webp)
Views of the world as the pictures were first chosen.

![](.github/devlog/2026-10-21-room-before.webp)
A lantern's room before the lamps' light was read again (item 39).

![](.github/devlog/2026-10-21-room-pair.webp)
A room's lamps under two packs (item 24).

![](.github/devlog/2026-10-21-rows.webp)
The villagers in their professions' robes (item 65).

![](.github/devlog/2026-10-21-scenes-sheet.webp)
Lamps in the open and in a room, the readings of the global illumination's faults (item 38).

![](.github/devlog/2026-10-21-sheet1.webp)
The README's world pictures as a sheet, the first set at a field of view of 70.

![](.github/devlog/2026-10-21-sheets.webp)
The zombie's and the villager's skin layers as the pack lays them (item 62).

![](.github/devlog/2026-10-21-stars-three.webp)
The stars at night in three readings against the source's (item 52).

![](.github/assets/mobs/chicken.webp)
![](.github/assets/mobs/cow.webp)
![](.github/assets/mobs/creeper.webp)
![](.github/assets/mobs/pig.webp)
![](.github/assets/mobs/sheep.webp)
![](.github/assets/mobs/skeleton.webp)
![](.github/assets/mobs/spider.webp)
![](.github/assets/mobs/villager.webp)
![](.github/assets/mobs/zombie.webp)
The villager is drawn at the 0.9375 that Minecraft's layer gives it and the cow has the nose and
horns of Minecraft 26.3, with each mob beside Minecraft's outline through the same camera, standing
and mid-stride, in the assets. (`7fe7639`)

![](.github/assets/gi/forty.webp)
![](.github/assets/gi/leak.webp)
![](.github/assets/gi/penumbra.webp)
The captures of the faults global illumination shows at 28eb41c, a light block's light leaking
through the edge where two blocks meet, the zig-zag edges of its light and of a lamp's shadow, and
forty glowstones with the screen's probes and without, for the engine's entry that links them.
(`404ab74`)

![](.github/assets/mobs/daylight.webp)
The mobs are drawn through a shader of the game's own that takes the color out of the light from all
around them and floors each face at Minecraft's flat shade of its top's light, so a white sheep's
side in the shade reads gray, with the scripted run reading it, the nine in daylight and the pairs
drawn again. (`e3d9a74`)

![](.github/assets/gi/blotches.webp)
![](.github/assets/interface/crafting.webp)
![](.github/assets/interface/creative.webp)
![](.github/assets/interface/furnace.webp)
![](.github/assets/interface/hud.webp)
![](.github/assets/interface/inventory.webp)
![](.github/assets/interface/options.webp)
![](.github/assets/interface/switcher.webp)
![](.github/assets/world/badlands.webp)
![](.github/assets/world/cherry-grove.webp)
![](.github/assets/world/desert-village.webp)
![](.github/assets/world/jungle.webp)
![](.github/assets/world/meadow.webp)
![](.github/assets/world/night.webp)
![](.github/assets/world/plains-village-own.webp)
![](.github/assets/world/plains-village.webp)
![](.github/assets/world/ravine.webp)
![](.github/assets/world/room.webp)
![](.github/assets/world/savanna.webp)
![](.github/assets/world/snowy-taiga.webp)
![](.github/assets/world/snowy-village.webp)
![](.github/assets/world/sunrise.webp)
![](.github/assets/world/swamp.webp)
![](.github/assets/world/village-night.webp)
The README opens on an animated GIF of ten views of seed 1 and shows the biomes, plants, villages,
lamps, sky and interface in its sections, all taken with the three default packs by
build/pictures.sh, which fetches them, sets the windows at a scale of 2 through voxel.scale and
raises the eye out of any block voxel.state finds it in. (`7039d5a`)

![](.github/assets/gi/plant-shadows.webp)
A capture of the plants' shadows drawn as whole quads, which the engine's ask on masked surfaces
drawn through a program's shader links. (`bc02459`)

![](.github/assets/mobs/daylight-pack.webp)
With a pack loaded, the villager wears its body's picture with the clothes of its biome laid over it
where they are not clear and the sheep its body's picture under its wool's tinted by its color, the
pig's, cow's and chicken's pictures are found by the names Minecraft 26 gives them, and the daylight
row is taken again wearing the default packs. (`9ce44d7`)

Done that day

- Item 17 at `5cd1521`. Out of the water onto the shore, swimming while any of the body is in it.
- Item 18 at `28eb41c`. The mobs' proportions toward the source's, the villager first, with some
  character kept.
- Item 19 at `d9c2f8d`. Four game modes and the switcher, F3 with F4 opening the row of icons in
  boxes, F4 stepping, spectator among them.
- Item 20 at `9bd58e3`. The inventory's quick moves as the source has them.
- Item 21 at `baf26b9`. Stairs join at corners, place by the hit, and the aim picks the shape and
  not its cell.
- Item 22 at `bf13ff6`. The baked ambient occlusion has a strength.
- Item 23 at `8722725`. The interface drawn as the game's own, in the source's style, ImGui kept for
  the debug windows.
- Item 24 at `e14134d`. Texture packs read from the player's own resource packs, shipping no one's
  textures.
- Item 25 at `a6ec820`. More biomes, more blocks and more trees.
- Item 37 at `e3d9a74`. A mob's faces away from the sun read blue-grey, lit by the sky alone.
- Item 38 at `404ab74`. The screen probes off by default, and the cascades' faults given to the
  engine with repros (Decision 16).
- Item 40 at `5064479`. F2 takes a screenshot, as the source does (Decision 19).
- Item 41 at `236763d`. A pack's blockstate variants give a surface several tiles picked per block
  by its position, and Simple Grass Flowers is a fourth default pack (Decision 20).
- Item 44 at `cc46f25`. The land's fading edge meets a sky of the haze's color below the horizon.
- Item 45 at `8261308`. The look without a pack takes Bare Bones' colors (Decision 23).
- Item 47 at `f7df279`. The haze dome no longer cuts the sun, the moon and their glows (Decision
  25).
- Item 48 at `0c93d3e`. No tunnel mouth and no ravine under a village (Decision 25).
- Item 49 at `f7df279`. The sun and the moon in the source's default look, square with a dithered
  glow, and a pack's own where it has one (Decision 25).
- Item 50 at `a96cea5`. The block's texture over the whole window while the eye is inside a block,
  as the source draws it (Decision 25).
- Item 51 at `e4483af`. The sun's and the moon's glow smooth as Bare Bones' is, in the game's own
  art too (Decision 25).
- Item 52 at `f7bcf26`. The stars as plain squares, the game's layout and the source's compared for
  the choice (Decision 25).
- Item 53 at `6845654`. The bloom at a tenth (Decision 25).
- Item 56 at `d32ee21`. More plants, the source's counts read and the modpack's look matched
  (Decision 27).
- Item 57 at `815e4ca`. The plants stand offset in their block as the source places them (Decision
  27).
- Item 58 at `b56fa0b`. The light and the grade after the modpack's shader settings as chosen, read
  as numbers (Decision 28).
- Item 59 at `fa7fba0`. Each biome's grass and foliage color from the source's fixed values, not a
  blend of the game's own climate (Decision 28).
- Item 60 at `9a13c54`. The lamps' light falls off smoothly, the eight-a-block grid taking the sun's
  light and shadow alone.
- Item 61 at `92112c5`. A mob's side faces take no bands from their own shadow at noon.
- Item 62 at `9ce44d7`. Every mob wears the pack's entity picture (Decision 30).
- Item 63 at `e6a2c55`. A mob's face is lit evenly from top to bottom, the turned-up light seen from
  straight above.
- Item 64 at `69ecb49`. The zombie's own face is the source's archetype and the villager's eyes sit
  lower (Decision 31).
- Item 66 at `f45b75e`. The creeper keeps the game's own vanilla-style art with any pack (Decision
  33).

## 2026-10-20

![](.github/devlog/2026-10-20-all-after.webp)
Each mob in the game after the walk fix, with Minecraft 26.3's outline in red, standing and mid-
stride.

![](.github/devlog/2026-10-20-all-mobs.webp)
Each mob in the game with Minecraft 26.3's outline in red, standing and mid-stride, through the same
camera. The villager at full size second.

![](.github/devlog/2026-10-20-villager.webp)

![](.github/devlog/2026-10-20-frame-pair.webp)
A lamp's light on the walls as the engine's glow light draws it, two frames apart (item 15).

Done that day

- Item 10 at `35696e2`. Some of the animals' feet do not show (Decision 6).
- Item 15 at `cc334ab`. The section's mesh keeps the stone's faces against a lamp block, so the
  scene field reads the stone as solid.
- Item 16 at `435b2de`. The terrain's shaders are marked as keeping their positions, and the render
  distance rises to what the frame then affords (items 12 and 36 of the engine).

## 2026-10-19

![](.github/devlog/2026-10-19-models-side.webp)
The mobs from the side, the pig, the cow, the sheep, the chicken, the villager, the zombie, the
skeleton, the creeper and the spider.

![](.github/devlog/2026-10-19-q-all.webp)
The sun over the plains at the global illumination's qualities, the halo read against the sky.

Done that day

- Item 6 at `6d71cd1`. The sun, the moon and the stars fixed on the sky's sphere with textures, the
  moon with phases, the glow around the sun and the moon as particles parented to them (Decisions 6
  and 7).
- Item 9 at `1477e82`. The icons of the block and item types are made anew (Decision 6).
- Item 11 at `569b5fb`. The README's picture shows the world's life, structures, a village and
  animals, and not terrain alone (Decision 6).
- Item 12 at `9158df8`. The render distance is read and set by what the frame affords (Decision 6).
- Item 13 at `a31d456`. The mobs' models take the other game's sizes and boxes (Decision 8), the
  textures the game's own.
- Item 14 at `bde1fc1`. The game's tonemapping is the engine's somewhat boring curve (Decision 9).

## 2026-10-18

Done that day

- Item 7 at `4fc4809`. The sun's shadows are pixelated (Decision 6).
- Item 8 at `344ef5b`. The sun's surround in the sky is pixelated and the rest of the sky stays
  smooth (Decision 6).

## 2026-10-17

Done that day

- Item 1 at `c08d259`. The replacement, one commit (Decision 1).
- Item 2 at `e0f4e66`. The names, one commit after item 1.
- Item 3 at `105bdd1`. The first pack run's proof, its known failure mended at `105bdd1`; done on
  the pack run of 2026-10-17.
- Item 4 at `b87e386`. The register, STYLE.md's rule and the sweep (Decision 5).
- Item 5 at `4358f3b`. The register widens to likenesses and figures of speech (Decision 5),
  README.md:3 and AGENTS.md:3 and 5.
