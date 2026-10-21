#!/usr/bin/env bash
# A scripted run of the game through ./e3d, offscreen and under the validation layer, as the build
# workflow's play job runs it on lavapipe. On a flat world with nothing kept on disk it builds a
# closed room of stone with a glowstone block in it, puts the player in the room facing the lamp at
# night, walks toward it and captures the frame, then captures the night sky from the room's roof,
# the full moon and the stars over the flat land from two places and headings, the sun and its glow
# at noon and at dusk, a pillar's shadow under the morning sun, and the window of every item's
# icon, then swims the player out of a pool onto its bank and reads a white sheep's wool in the
# shade, and over seed 1's overworld stands the player over each of the eight biomes the climate
# gained and captures it. A second run that keeps its files, in a data folder of its own, then takes a screenshot
# with F2. It fails where the game does not open or answer, the player did not move or did not climb
# out onto the bank, the wool in shade is not a gray, a biome is not found or not where it was
# found, the layer reported an error, or F2 left no
# screenshot of the window's size, and says why in an error annotation, so the run's page names it.
#
#   build/play.sh
#
# The game is the Debug build that `dotnet build VoxelSandbox.slnx` makes. ./e3d runs out of the
# engine checkout ENGINE_CHECKOUT names, or the folder above this one, and keeps the session's log
# in that checkout's build/sessions. The captures and what e3d answered go to captures.
set -euo pipefail
cd "$(dirname "$0")/.."
mkdir -p captures

program=VoxelSandbox/bin/Debug/net10.0/VoxelSandbox
opened=captures/opened.json
said=captures/said.txt
log=""
# Every command and the stop go to this game by the process the open names, so a program an earlier
# run left serving, or another session's game of the same name, neither makes them ambiguous nor
# takes the stop meant for this one. Before the open has answered, the stop goes by the game's name.
export E3D_NAME=VoxelSandbox
pid=""
kept=""
# The kept run's data folder, under which the game keeps its saves, its packs' list and its
# screenshots and e3d its sessions, so it leaves the machine's own alone.
data=$(mktemp -d)
# The script's own output, which an error is written to, so one said inside $(ask ...) reaches the
# page and not the variable.
exec 3>&1

fail() {
  local ending=""
  [ -n "$log" ] && [ -f "$log" ] && ending=$(tail -n 3 "$log" | tr -s '\r\n' '  ' | sed 's/ *$//; s/\.$//' | cut -c1-600)
  echo "::error title=Voxel Sandbox through e3d::$1.${ending:+ Its log ends with $ending.}" >&3
  exit 1
}
trap 'if [ -n "$pid" ]; then ./e3d stop --session "$pid" --quiet; else ./e3d stop --quiet; fi > /dev/null 2>&1 || true
  [ -n "$kept" ] && XDG_DATA_HOME="$data" ./e3d stop --session "$kept" --quiet > /dev/null 2>&1 || true
  rm -rf "$data"' EXIT
# A command e3d refuses fails with its code and sentence, which it writes to standard error.
cmd() { ./e3d command "$@" --quiet --timeout 600 --session "$pid" 2> "$said" || fail "./e3d command $* ended with $?, $(head -n 1 "$said")"; }
ask() {
  local answer
  answer=$(./e3d command "$@" --timeout 600 --session "$pid" 2> "$said") || fail "./e3d command $* ended with $?, $(head -n 1 "$said")"
  printf '%s\n' "$answer"
}
# The player's feet from voxel.state, which begins "feet at x, y, z".
feet() { printf '%s\n' "$1" | sed -nE 's/^feet at (-?[0-9.]+), (-?[0-9.]+), (-?[0-9.]+) .*/\1 \2 \3/p'; }

[ -f "$program" ] || [ -f "$program.exe" ] || fail "$program is not built, which dotnet build VoxelSandbox.slnx does"

# The answer goes to a file and is read from there, as the engine's scripts of its games read
# theirs, since on Windows the game is started holding every handle e3d holds, and $(...) would wait
# on that pipe until the game ends. The log it names is read with perl, which every system the
# script may run on has.
if ! ENGINE_VULKAN_VALIDATION=1 ./e3d open "$program" --offscreen --transient --flat --json > "$opened"; then
  answer=$(perl -MJSON::PP -0777 -ne '
    my $answer = eval { decode_json($_) } or do { s/\s+/ /g; print length ? "an answer that is not JSON, $_" : "nothing"; exit };
    my $error = $answer->{errors}[0] || {};
    (my $sentence = $error->{message} // "") =~ s/\.$//;
    print "$error->{code}, $sentence";' "$opened")
  log=$(perl -MJSON::PP -0777 -ne 'my $answer = eval { decode_json($_) } or exit; print $answer->{data}{log} // ""' "$opened")
  fail "did not open, e3d said $answer"
fi
log=$(perl -MJSON::PP -0777 -ne 'my $answer = eval { decode_json($_) } or exit; print $answer->{data}{log} // ""' "$opened")
pid=$(perl -MJSON::PP -0777 -ne 'my $answer = eval { decode_json($_) } or exit; print $answer->{data}{pid} // ""' "$opened")
[ -n "$pid" ] || fail "e3d opened the game and did not say its process"

# The room is closed, so at night the lamp is its only light, and the player stands at its south
# wall facing the lamp.
cmd voxel.room 10 4 -22 22 11 -10 stone
cmd voxel.set 16 5 -16 glowstone
cmd voxel.tp 16.5 5 -11.8
cmd voxel.look 0 -20
cmd voxel.time 23
cmd frames.wait 120

# The room as the lamp lights it, before the walk takes the player up to the lamp. The capture's
# path is whole, since the game writes it from its own folder, and on Windows the form the game
# reads.
capture() {
  local shot="$PWD/captures/$1"
  if command -v cygpath > /dev/null; then shot=$(cygpath -m "$shot"); fi
  ./e3d shot "$shot" --timeout 600 --session "$pid" > /dev/null 2> "$said" || fail "./e3d shot ended with $?, $(head -n 1 "$said")"
}
capture room.png

before=$(ask voxel.state)
cmd input.key W 30
after=$(ask voxel.state)
echo "$before"
echo "$after"
from=$(feet "$before")
to=$(feet "$after")
[ -n "$from" ] && [ -n "$to" ] || fail "voxel.state did not say where the player stands, $before"
distance=$(printf '%s %s\n' "$from" "$to" | awk '{ printf "%.2f", sqrt(($4 - $1) ^ 2 + ($5 - $2) ^ 2 + ($6 - $3) ^ 2) }')
awk -v d="$distance" 'BEGIN { exit !(d > 0.5) }' || fail "the player did not walk toward the lamp, from $from to $to"

# The night sky from the roof, the full moon of the first night ahead in the east-northeast and
# the stars around it, then from three blocks to the northwest looking further north and higher.
# The moon, its glow and the stars are quads fixed on the sky, so the second capture shows each at
# the same place on the sky with its sides running the same way there, turned on the window with
# the view.
cmd voxel.tp 16.5 12 -16
cmd voxel.time 20
cmd voxel.look 71 25
cmd frames.wait 2
capture sky.png
cmd voxel.tp 13.5 12 -19
cmd voxel.look 40 35
cmd frames.wait 2
capture sky2.png

# The sun high in the south at noon and low in the west at dusk, its glow's rings stepping along
# the sun's pixels, the sky's gradient around them smooth. The clouds are left out at noon, where
# the one drifting over the sun at that hour would hide it.
cmd voxel.time 12
cmd voxel.look 180 60
cmd voxel.clouds off
cmd frames.wait 2
capture noon.png
cmd voxel.clouds on
cmd voxel.time 17.6
cmd voxel.look 270 5
cmd frames.wait 2
capture dusk.png

# A pillar of stone west of the room and its shadow on the grass under the sun of 8.30, the
# shadow's edges running slantwise and stepping along the pixels of the blocks they cross, seen from
# the air to the southeast. The mobs are taken away first, so none wanders into the shadow.
cmd voxel.kill all
cmd voxel.fill 1 4 -9 1 8 -9 stone
cmd voxel.fly on
cmd voxel.time 8.5
cmd voxel.tp 6 9 -1
cmd voxel.look 305 -35
cmd frames.wait 2
capture shadow.png

# The window E opens in creative mode, every item's icon in it and the hotbar's below.
cmd input.key E 1
cmd frames.wait 2
capture inventory.png
cmd input.key E 1

# A pool of water four blocks deep east of the room, with a bank on its east side a block above the
# water's top cell, and the player in the water facing the bank. W alone takes the player to the
# bank, up its side and onto it, as walking into a shore does. The walk ends on the bank whether a
# frame takes a sixtieth of a second or the twentieth the game counts at most, as on lavapipe, and
# the bank ends at x 47, inside the columns lavapipe has loaded by then.
cmd voxel.fill 30 4 -20 47 8 -14 stone
cmd voxel.fill 31 5 -19 35 8 -15 water
cmd voxel.fill 36 9 -20 47 9 -14 stone
cmd voxel.fly off
cmd voxel.tp 35.2 6 -17.5
cmd voxel.look 90 0
cmd frames.wait 10
cmd input.key W 60
shore=$(ask voxel.state)
echo "$shore"
landed=$(feet "$shore")
printf '%s\n' "$shore" | grep -q "on the ground" && printf '%s\n' "$landed" | awk '{ exit !($1 > 36 && $2 == 10) }' \
  || fail "the player did not climb out of the water onto the bank, whose top is at 10, and stands at $landed"

# A white sheep south of the spawn at noon, held, its side in the shade seen from two blocks east at
# its middle's height. The game's mob shader takes the color out of the light from all around it,
# so its wool reads a gray near 150 on all three channels where the sky's blue reached it before.
cmd voxel.clouds off
cmd voxel.time 12
cmd voxel.kill all
cmd voxel.fly on
cmd voxel.tp 0.5 4 12.5
cmd voxel.look 0 0
cmd frames.wait 2
cmd voxel.spawn sheep 0
cmd voxel.still on
cmd voxel.hud off
cmd voxel.tp 2.5 3.1 9.5
cmd voxel.look 270 0
cmd frames.wait 60
ask voxel.color 1 1 1 1 > /dev/null
cmd frames.wait 2
wool=$(ask voxel.color 600 320 80 60)
echo "$wool"
channels=$(printf '%s\n' "$wool" | sed -nE 's/^red ([0-9.]+), green ([0-9.]+), blue ([0-9.]+) .*/\1 \2 \3/p')
printf '%s\n' "$channels" | awk 'NF == 3 { lo = $1; hi = $1; for (i = 2; i <= 3; i++) { if ($i < lo) lo = $i; if ($i > hi) hi = $i } exit !(lo >= 120 && hi <= 200 && hi - lo <= 20) } NF != 3 { exit 1 }' \
  || fail "the sheep's wool in shade reads $wool, where a gray between 120 and 200 with its channels within 20 of each other is asked"

# Seed 1's overworld, the player standing over each of the eight biomes the climate gained, the
# nearest of each that voxel.biome finds, its name read back from voxel.state and its land captured
# from above, at a render distance of four columns, so lavapipe generates few at each.
cmd voxel.hud on
cmd voxel.world overworld 1
cmd voxel.distance 4
cmd voxel.fly on
cmd voxel.time 10
mkdir -p captures/biomes
for biome in swamp savanna jungle dark_forest badlands beach river cherry_grove; do
  found=$(ask voxel.biome "$biome")
  read -r bx by bz <<< "$(printf '%s\n' "$found" | sed -nE 's/.* is at (-?[0-9]+), (-?[0-9]+), (-?[0-9]+),.*/\1 \2 \3/p')"
  [ -n "$bz" ] || fail "voxel.biome found no $biome, saying $found"
  cmd voxel.tp "$bx" "$((by + 24))" "$bz"
  cmd voxel.look 0 -45
  cmd frames.wait 90
  state=$(ask voxel.state)
  [[ "$state" == *" in ${biome//_/ }, facing"* ]] || fail "the player over the $biome found at $bx, $bz stands in another biome, $state"
  capture "biomes/$biome.png"
done

./e3d stop --session "$pid" --quiet > /dev/null 2> "$said" || fail "./e3d stop ended with $?, $(head -n 1 "$said")"
[ -f "$log" ] || fail "the log e3d named, $log, is not there"
grep -q "Validation layers: ENABLED" "$log" || fail "the game drew without the validation layer"
if grep -F 'Validation Error' "$log"; then fail "the validation layer reported an error"; fi

# F2 in a run that keeps its files writes the frame as a PNG into the screenshots folder beside the
# saves, named by the time. The list of packs is made empty first, so the run fetches none.
mkdir -p "$data/VoxelSandbox"
: > "$data/VoxelSandbox/resourcepacks.txt"
keep() { XDG_DATA_HOME="$data" ./e3d "$@"; }
keep open "$program" --offscreen --flat --world shot --json > captures/opened-kept.json || fail "the run that keeps its files did not open"
kept=$(perl -MJSON::PP -0777 -ne 'my $answer = eval { decode_json($_) } or exit; print $answer->{data}{pid} // ""' captures/opened-kept.json)
[ -n "$kept" ] || fail "e3d opened the run that keeps its files and did not say its process"
keep command frames.wait 30 --quiet --timeout 600 --session "$kept" 2> "$said" || fail "./e3d command frames.wait ended with $?, $(head -n 1 "$said")"
keep command input.key F2 1 --quiet --timeout 600 --session "$kept" 2> "$said" || fail "./e3d command input.key F2 ended with $?, $(head -n 1 "$said")"
keep command frames.wait 5 --quiet --timeout 600 --session "$kept" 2> "$said" || fail "./e3d command frames.wait ended with $?, $(head -n 1 "$said")"
keep stop --session "$kept" --quiet > /dev/null 2> "$said" || fail "./e3d stop of the run that keeps its files ended with $?, $(head -n 1 "$said")"
kept=""
shots=("$data"/VoxelSandbox/screenshots/*.png)
[ -f "${shots[0]}" ] || fail "F2 wrote no screenshot into the screenshots folder"
name=$(basename "${shots[0]}")
[[ "$name" =~ ^[0-9]{4}-[0-9]{2}-[0-9]{2}_[0-9]{2}\.[0-9]{2}\.[0-9]{2}\.png$ ]] || fail "the screenshot is named $name, where a name by the time as Minecraft's is asked"
size=$(perl -e 'open my $png, "<", shift or exit; binmode $png; read $png, my $head, 24; print join " ", unpack "NN", substr $head, 16, 8' "${shots[0]}")
[ "$size" = "1280 720" ] || fail "the screenshot is $size pixels, where the window's 1280 by 720 is asked"

echo "Voxel Sandbox walked $distance blocks toward a glowstone in a closed room, climbed out of a pool onto its bank, saw a sheep's wool in shade gray, drew with no validation error and wrote $name on F2"
