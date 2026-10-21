#!/usr/bin/env bash
# Takes the README's pictures through ./e3d, each from the seed, the place, the hour and the heading
# written below, so a change to how the world is drawn is shown again by running it. The game opens
# offscreen on the overworld of seed 1, saving nothing, at its own render distance and light, with
# every mob held where the land placed it and the clouds held where each picture places them, and
# drawn with the three default resource packs resources/packs.json names, in its order, as the game
# is drawn once it has fetched them at its first start. Each picture is captured at 1280 by 720 once
# every column in reach is meshed. A view of the world is taken with the HUD hidden and encoded at
# 800 by 450 by the engine checkout's build/webp.sh. A view of the interface is drawn at 2 of the
# screen's pixels to each of its own and cut out at 800 by 450 around what it shows, so its pixels
# keep their edges. The animation at the README's head is ten of the views, two seconds each, as a
# GIF with a palette of its own for each frame, which every viewer of a README plays.
#
#   build/pictures.sh
#
# It writes, under .github/assets,
#
#   head.gif                   ten of the views below, two seconds each
#   world/<view>.webp          the views of the world, of its biomes, plants, trees, water, lamps,
#                              caves and sky, and one view with no pack beside the same with them
#   interface/<view>.webp      the HUD, the options, the inventory, a crafting table, a furnace, the
#                              creative window and the mode switcher
#
# The packs are fetched from the URLs resources/packs.json names into captures/packs, which git
# ignores, and each is refused unless its SHA-512 is the one named there, so the repository holds
# none of a pack's files.
#
# The game is the Debug build that `dotnet build VoxelSandbox.slnx` makes. ./e3d and build/webp.sh
# run out of the engine checkout ENGINE_CHECKOUT names, or the folder above this one. ImageMagick
# cuts the interface's views and ffmpeg encodes the GIF.
set -euo pipefail
# Numbers are read and written with a point, whatever the machine's own locale writes them with.
export LC_ALL=C
cd "$(dirname "$0")/.."
engine="${ENGINE_CHECKOUT:-..}"
program=VoxelSandbox/bin/Debug/net10.0/VoxelSandbox
mkdir -p captures/packs captures/head .github/assets/world .github/assets/interface
opened=captures/pictures.json

[ -f "$program" ] || [ -f "$program.exe" ] || { echo "$program is not built, which dotnet build VoxelSandbox.slnx does" >&2; exit 1; }

# The default packs, each kept from an earlier run where its bytes are still the ones named, and
# fetched with the user agent the game sends, as Modrinth asks a program to name itself.
sha512() { sha512sum "$1" | cut -d ' ' -f 1; }
packs=()
while IFS=$'\t' read -r file url sum; do
  path="captures/packs/$file"
  if [ ! -f "$path" ] || [ "$(sha512 "$path")" != "$sum" ]; then
    curl -fsSL -A "VoxelSandbox build/pictures.sh (+https://github.com/EggyStudio/Voxel-Sandbox)" -o "$path.part" "$url"
    [ "$(sha512 "$path.part")" = "$sum" ] || { echo "$file from $url is not the file resources/packs.json names" >&2; exit 1; }
    mv "$path.part" "$path"
  fi
  packs+=(--pack "$PWD/$path")
done < <(perl -MJSON::PP -0777 -ne 'print join("\t", @$_{qw(file url sha512)}), "\n" for @{decode_json($_)->{packs}}' VoxelSandbox/resources/packs.json)

./e3d open "$program" --offscreen --transient "${packs[@]}" --json > "$opened"
pid=$(perl -MJSON::PP -0777 -ne 'my $answer = eval { decode_json($_) } or exit; print $answer->{data}{pid} // ""' "$opened")
[ -n "$pid" ] || { echo "e3d did not open the game, it said $(cat "$opened")" >&2; exit 1; }
trap './e3d stop --session "$pid" --quiet > /dev/null 2>&1 || true' EXIT
cmd() { ./e3d command "$@" --quiet --timeout 600 --session "$pid"; }
shot() { ./e3d shot "$PWD/captures/$1.png" --timeout 600 --session "$pid" > /dev/null; }

cmd voxel.world overworld 1
cmd voxel.still on
cmd voxel.hud off
cmd voxel.fly on
cmd voxel.scale 2

# Puts the player at a place and waits until every column in reach of it is generated and meshed.
go() {
  cmd voxel.tp "$1" "$2" "$3"
  for i in $(seq 1 600); do
    ./e3d command voxel.state --timeout 600 --session "$pid" | grep -q " 0 generating, 0 sections to mesh" && break
    cmd frames.wait 10
  done
}

# Raises the eye a block at a time, up to eight, until its cell holds air, so no view is taken from
# inside a block, and fails naming the view where it does not get out.
clear_eye() {
  for i in $(seq 0 8); do
    local state
    state=$(./e3d command voxel.state --timeout 600 --session "$pid")
    [[ "$state" != *", the eye inside "* ]] && return
    local feet
    feet=$(printf '%s\n' "$state" | sed -n 's/^feet at \([-0-9.]*\), \([-0-9.]*\), \([-0-9.]*\).*/\1 \2 \3/p')
    set -- $feet
    cmd voxel.tp "$1" "$(echo "$2 + 1" | bc)" "$3"
  done
  echo "the view $name is taken from inside a block, $(./e3d command voxel.state --timeout 600 --session "$pid" | head -c 200)" >&2
  exit 1
}

# A view of the world named for its file, taken from a place at an hour, facing a heading in degrees
# clockwise from north and pitched in degrees up, with the clouds drifted so many blocks east, or with
# none where the drift is off.
view() {
  local name="$1" x="$2" y="$3" z="$4" hour="$5" heading="$6" pitch="$7" drift="$8"
  go "$x" "$y" "$z"
  clear_eye
  cmd voxel.time "$hour"
  cmd voxel.look "$heading" "$pitch"
  if [ "$drift" = off ]; then cmd voxel.clouds off; else cmd voxel.clouds on "$drift"; fi
  # Global illumination settles over the new place.
  cmd frames.wait 240
  shot "$name"
  "$engine/build/webp.sh" "captures/$name.png" ".github/assets/world/$name.webp" lossy 800x450
  echo "took .github/assets/world/$name.webp"
}

# A view of the interface as the frame shows it now, 800 by 450 of its pixels cut out from the side a
# gravity names, encoded lossy, since the world behind it is a lit scene.
window() {
  local name="$1" gravity="$2"
  cmd frames.wait 3
  shot "$name"
  magick "captures/$name.png" -gravity "$gravity" -crop 800x450+0+0 +repage "captures/$name-cut.png"
  "$engine/build/webp.sh" "captures/$name-cut.png" ".github/assets/interface/$name.webp" lossy
  echo "took .github/assets/interface/$name.webp"
}

# A mob of a kind and variant set down on the ground at a place, the height of the ground's top block
# given, facing a heading, by the crosshair from three blocks above it, within its reach, where it
# stands still.
place() {
  local kind="$1" variant="$2" x="$3" y="$4" z="$5" heading="$6"
  cmd voxel.tp "$x" $(( y + 3 )) "$z"
  cmd voxel.look "$heading" -90
  cmd frames.wait 2
  cmd voxel.spawn "$kind" "$variant"
}

# The center of an inventory window's slot at 2 of the screen's pixels to each of the interface's, by
# its place in the window of 176 by 166 that Minecraft gives it, and of a slot of the inventory by
# its number, the hotbar's 1 to 9 and the rest's 10 to 36.
slot() { echo $(( (1280 - 352) / 2 + ($1 + 8) * 2 )) $(( (720 - 332) / 2 + ($2 + 8) * 2 )); }
held() { if [ "$1" -le 9 ]; then slot $(( 8 + ($1 - 1) * 18 )) 142; else slot $(( 8 + ($1 - 10) % 9 * 18 )) $(( 84 + ($1 - 10) / 9 * 18 )); fi; }
# A right click where the pointer is, a press and a release of the right button with no move.
right() { cmd input.move "$@"; cmd input.drag Right 0 0 1 1; }

# The biomes, each from inside it in the afternoon, the badlands' mesas over a river's canyon, a
# swamp's pools among its oaks, the savanna's acacias over its grass and the snowy taiga's spruces.
view badlands 1440 69 608 15 0 -12 0
view swamp -32 63 -1440 15 0 -12 0
view savanna 352 72 -448 15 90 -12 0
view snowy-taiga 256 70 1312 15 90 -12 0
# A cherry grove from under the edge of its crowns, a lantern hanging from one and pink petals on the
# grass, and the jungle's giant trees from over its canopy.
view cherry-grove -736 79 -960 16 345 -12 0
view jungle -928 98 -1376 15 0 -12 0
# The plants at a meadow's patch of flowers, seen from a step away, and a ravine on the plains at
# noon, the sun down its walls and its magma on its floor.
view meadow -253.5 69 72.5 15 0 -28 0
view ravine -195.3 74 216.2 12 253 -30 0

# The villages: the desert's beside its pyramid in the evening, the snowy plains' among its spruces,
# and the plains' by its river with two sheep, a cow, a pig and a villager set down in the meadow
# before it, as the land's herds graze about its villages, so each run shows them where the last did.
view desert-village 720.4 66 -697.6 17 135 -18 0
view snowy-village 454.4 69 77.4 15 135 -18 0
go 15.4 66 700.6
for mob in "sheep 0 22.5 55 693.5 200" "sheep 0 25.5 55 696.5 150" "cow 0 21.5 54 689.5 250" "pig 0 26.5 54 692.5 120" "villager 0 25.5 54 690.5 30"; do
  place $mob
done
view plains-village 15.4 66 700.6 16 45 -18 0
# The same village by night, its lanterns and torches lit.
view village-night 454.4 69 77.4 21 135 -18 0

# The sea at sunrise from over the water, the sun's square over an island, and the sky at midnight
# with no moon up, its stars over the island with the clouds out of the way.
view sunrise -64 72 -416 6.3 90 3 0
cmd voxel.day 3
view night -64 72 -416 0 200 18 off
cmd voxel.day 0

# The game's own art beside the packs', the plains village again with every pack off.
cmd voxel.packs none
view plains-village-own 15.4 66 700.6 16 45 -18 0
cmd voxel.packs "$(printf '%s; ' "${packs[@]}" | sed 's/--pack; //g; s/; $//')"

# A closed room of oak planks in the air at night, lit by a lantern hanging from its ceiling and two
# torches on its far walls, with blocks of wool, a crafting table and a furnace on its floor to take
# the light's color, seen from a corner.
go 304 103 304
cmd voxel.room 300 100 300 308 105 308 oak_planks
cmd voxel.set 304 104 304 hanging_lantern
cmd voxel.set 307 102 304 wall_torch_west
cmd voxel.set 304 102 307 wall_torch_north
for block in "306 101 306 red_wool" "305 101 306 crafting_table" "306 101 305 furnace" "302 101 306 blue_wool" "306 101 302 yellow_wool"; do
  cmd voxel.set $block
done
view room 301.5 101.4 301.5 21 135 -4 0

# The interface in survival on the plains village's meadow in the afternoon, the player on the
# ground facing the village and its mobs, the hotbar's nine slots set to tools, blocks and food one by
# one, since the game begins with blocks in them, more in the inventory, health and food down a few
# points, and the HUD shown.
go 14.5 70 702.5
cmd voxel.fly off
cmd frames.wait 120
cmd voxel.mode survival
cmd voxel.time 16
cmd voxel.look 45 -6
cmd voxel.clouds on 0
cmd voxel.hud on
keys=(One Two Three Four Five Six Seven Eight Nine)
hotbar=(iron_pickaxe iron_sword stone_axe stone_shovel torch cobblestone oak_planks bread lantern)
for i in "${!hotbar[@]}"; do
  cmd input.key "${keys[$i]}" 1
  cmd voxel.select "${hotbar[$i]}"
done
cmd input.key One 1
for item in "iron_ingot 7" "stick 12" "coal 15" "oak_log 20" "wheat 18" "iron_ore 9" "white_wool 16" "apple 3" "raw_beef 4"; do
  cmd voxel.give $item
done
cmd voxel.vitals 15 14
cmd frames.wait 240
window hud south

# The inventory, the pointer over the iron pickaxe naming it with its uses left.
cmd input.key E 1
cmd input.move $(held 1)
window inventory center
cmd input.key E 1

# A crafting table at the eye's height two blocks to the north, the player turned to it, an iron
# pickaxe laid out in its grid, three ingots over two sticks, the pointer over what the grid makes.
feet=$(./e3d command voxel.state --timeout 600 --session "$pid" | sed -n 's/^feet at \([-0-9.]*\), \([-0-9.]*\), \([-0-9.]*\).*/\1 \2 \3/p')
read -r bx by bz <<< "$(echo "$feet" | awk 'function floor(v) { return v < int(v) ? int(v) - 1 : int(v) } { print floor($1), floor($2 + 1.62), floor($3) - 2 }')"
cmd voxel.set "$bx" "$by" "$bz" crafting_table
cmd voxel.look 0 0
cmd frames.wait 2
cmd voxel.place
cmd input.click $(held 10)
for column in 0 1 2; do right $(slot $(( 30 + column * 18 )) 17); done
cmd input.click $(held 10)
cmd input.click $(held 11)
for row in 1 2; do right $(slot 48 $(( 17 + row * 18 ))); done
cmd input.click $(held 11)
cmd input.move $(slot 124 35)
window crafting center
cmd input.key E 1

# A furnace in the table's place smelting iron ore over coal, an ingot made and the next on its way.
cmd voxel.set "$bx" "$by" "$bz" furnace
cmd voxel.furnace "$bx" "$by" "$bz" iron_ore 8 coal 3
cmd frames.wait 840
cmd voxel.place
cmd input.move $(slot 116 35)
window furnace center
cmd input.key E 1
cmd voxel.set "$bx" "$by" "$bz" air
cmd voxel.look 45 -6

# The menu's options over the darkened world.
cmd input.key Escape 1
cmd input.click 640 280
window options center
cmd input.key Escape 1
cmd input.key Escape 1

# Creative mode's window of every item, and the mode switcher F3 held with F4 opens.
cmd voxel.mode creative
cmd input.key E 1
window creative center
cmd input.key E 1
./e3d command input.key F3 90 --quiet --timeout 600 --session "$pid" &
cmd frames.wait 10
cmd input.key F4 1
window switcher center
wait

# The animation at the README's head, ten of the views two seconds each, time to look at each,
# every frame with a palette of its own, dithered by error diffusion, which keeps the haze's
# gradients smooth.
frames=(desert-village badlands cherry-grove plains-village swamp jungle snowy-village village-night room sunrise)
for i in "${!frames[@]}"; do magick "captures/${frames[$i]}.png" -resize 800x450 "captures/head/$(printf '%02d' "$i").png"; done
ffmpeg -loglevel error -y -framerate 0.5 -i captures/head/%02d.png \
  -vf "split[a][b];[a]palettegen=stats_mode=single[p];[b][p]paletteuse=new=1:dither=sierra2_4a" -loop 0 .github/assets/head.gif
echo "took .github/assets/head.gif"
