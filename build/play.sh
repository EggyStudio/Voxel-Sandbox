#!/usr/bin/env bash
# A scripted run of the game through ./e3d, offscreen and under the validation layer, as the build
# workflow's play job runs it on lavapipe. On a flat world with nothing kept on disk it reads that
# global illumination takes the screen's probes, builds a closed room of stone with a glowstone
# block in it, puts the player in the room facing the lamp at night, walks toward it and captures
# the frame, then captures the night sky from the room's roof, the full moon and the stars over the
# flat land from two places and headings, reads a cloud's underside overhead at noon, captures the
# sun and its glow at noon and at dusk, a pillar's shadow under the morning sun, and the window of
# every item's icon, then swims the player out of a pool onto its bank and reads a white sheep's
# wool in the shade, and over seed 1's overworld stands the player over each of the eight biomes the
# climate gained and captures it, reads the chest of seed 1's nearest mineshaft, compares the caves'
# noises read on the GPU with the CPU's and reads the drawn land's edge against the sky. A second
# run that keeps its files, in a data folder of its own, then takes a screenshot with F2. It fails
# where the game does not open or answer, global illumination takes no screen probes, the player did
# not move or did not climb out onto the bank, the wool in shade is not a gray, a cloud's underside
# overhead at noon is dark, a biome is not found or not where it was found, the mineshaft's chest
# holds no rails or torches, the GPU's readings of the caves' noises are not the CPU's, the land's
# edge steps to the sky beyond it, more than 400 draws of sections are recorded there, the layer
# reported an error, or F2 left no screenshot of the window's size, and says why in an error
# annotation, so the run's page names it.
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
# Each check's cost in seconds, written as the next begins and at the end, so the run's log names
# what each costs.
checking="the open"
since=$SECONDS
check() {
  echo "took $((SECONDS - since)) s, $checking, at $SECONDS s"
  checking=$1
  since=$SECONDS
}
# Waits until the world opened stands, its loading screen gone.
stood() {
  for i in $(seq 1 600); do
    ask voxel.state | grep -q "^loading " || return 0
    cmd frames.wait 10
  done
  fail "the world did not stand within 6000 frames, $(ask voxel.state)"
}
# Waits until every column in reach is generated and meshed.
meshed() {
  for i in $(seq 1 600); do
    ask voxel.state | grep -q " 0 generating, 0 sections to mesh" && return 0
    cmd frames.wait 5
  done
  fail "the land in reach was not generated and meshed within 3000 frames, $(ask voxel.state)"
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
# The flat world's checks read what stands within a few blocks of the player, so it is drawn at four
# columns, where the default of twelve would stream and draw about 650 columns on lavapipe. A check
# that reads light waits 20 or 30 frames, since lavapipe settles the lantern's pool in 14 and the
# sheep's wool, the creeper's front and the pool's water in 12, and one that captures the land waits
# for the land in reach to be meshed and 10 frames more.
cmd voxel.distance 4
stood

check "the screen probes"
# Global illumination takes the screen's probes, which the game leaves on.
probes=$(ask gi.state | grep "screen probes")
echo "$probes"
[[ "$probes" == "screen probes every "* ]] || fail "global illumination takes no screen probes, $probes"

check "the lamp's room"
# The room is closed, so at night the lamp is its only light, and the player stands at its south
# wall facing the lamp.
cmd voxel.room 10 4 -22 22 11 -10 stone
cmd voxel.set 16 5 -16 glowstone
cmd voxel.tp 16.5 5 -11.8
cmd voxel.look 0 -20
cmd voxel.time 23
cmd frames.wait 30

check "the walk to the lamp"
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

check "the lantern's pool"
# A lantern's light on the floor of white concrete in a closed room at night falls smoothly across
# the block before its glass, the sun's shadow alone taken on the grid of eight steps a block. Read
# along a line through the middles of a column of steps, no change between neighboring points is over
# a tenth of the whole, where light taken at each step's middle falls in a few changes of up to 0.14
# of it with none between, and the light falls by at least 40 levels, so a dark frame does not pass.
# The HUD is hidden, since the outline of the block looked at lies across the line.
cmd voxel.hud off
cmd voxel.room 50 4 -24 58 9 -12 white_concrete
cmd voxel.set 54 5 -20 lantern
cmd voxel.tp 54.5 6.2 -16.5
cmd voxel.look 0 -40
cmd frames.wait 30
ask voxel.line 1 1 1 1 1 1 2 > /dev/null
cmd frames.wait 2
pool=$(ask voxel.line 54.5625 5.001 -19.25 54.5625 5.001 -18.25 33)
echo "lantern's pool ${pool%%; by place*}"
read -r first last share <<< "$(printf '%s\n' "$pool" | sed -nE 's/^from ([0-9.]+) to ([0-9.]+), largest step [0-9.]+, ([0-9.]+) of the change;.*/\1 \2 \3/p')"
[ -n "$share" ] || fail "voxel.line read no line across the lantern's pool, saying $pool"
awk -v a="$first" -v b="$last" -v s="$share" 'BEGIN { exit !(a - b >= 40 && s <= 0.1) }' || fail "the lantern's light steps across the floor or does not fall, $pool"
cmd voxel.hud on

check "the night sky"
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
# A torch in the hand lights the roof about the player at night, read a block and a half ahead with
# stone in the hand, the moon alone lighting it, and with a torch.
cmd voxel.look 0 -60
ask voxel.pixel 1 1 1 > /dev/null
cmd frames.wait 2
dark=$(ask voxel.pixel 13.5 12.001 -20.5)
cmd voxel.select torch
cmd frames.wait 10
ask voxel.pixel 1 1 1 > /dev/null
cmd frames.wait 2
lit=$(ask voxel.pixel 13.5 12.001 -20.5)
cmd voxel.select stone
echo "the roof by moonlight $dark, by a torch in the hand $lit"
read -r dr _ <<< "$(printf '%s\n' "$dark" | sed -nE 's/^red ([0-9]+), .*/\1/p')"
read -r lr _ <<< "$(printf '%s\n' "$lit" | sed -nE 's/^red ([0-9]+), .*/\1/p')"
[ -n "$dr" ] && [ -n "$lr" ] && [ "$lr" -gt $((dr + 30)) ] || fail "a torch in the hand does not light the roof about the player, $dark against $lit"

check "the stars at dawn"
# At dawn the stars brighten the sky and darken none of it. Read low in the east over a band of the
# window with the clouds hidden, no more than 20 pixels stand darker than all four of their neighbors
# three pixels away by 8 levels, where stars blended over the sky stood as 272 dark spots, and at
# least 100 stand lighter, the stars still shown.
cmd voxel.clouds off
cmd voxel.tp 0.5 10 0.5
cmd voxel.time 5.8
cmd voxel.look 90 20
cmd frames.wait 10
ask voxel.spots 1 1 1 1 1 > /dev/null
cmd frames.wait 2
spots=$(ask voxel.spots 40 20 1200 300 8)
echo "the dawn sky's spots, $spots"
read -r darker lighter <<< "$(printf '%s\n' "$spots" | sed -nE 's/^([0-9]+) darker, ([0-9]+) lighter.*/\1 \2/p')"
[ -n "$lighter" ] || fail "voxel.spots read nothing of the dawn sky, saying $spots"
[ "$darker" -le 20 ] && [ "$lighter" -ge 100 ] || fail "the stars at dawn darken the sky or do not show, $spots"
cmd voxel.clouds on

check "a cloud's underside"
# A cloud's underside at noon takes the sky's light, read from 20 blocks under it, nearer than the
# haze begins, where the clouds placed at a drift of none hold one over the eye. Its red stands at
# least 100 in sRGB levels, where it read 146, the sky beside it 45 and an underside drawn black 9.
cmd voxel.fly on
cmd voxel.tp 0.5 100 0.5
cmd voxel.time 12
cmd voxel.look 0 90
cmd voxel.clouds on 0
cmd frames.wait 10
under=$(ask voxel.color 590 310 100 100)
echo "a cloud's underside overhead, $under"
red=$(printf '%s\n' "$under" | sed -nE 's/^red ([0-9]+).*/\1/p')
[ -n "$red" ] && [ "$red" -ge 100 ] || fail "a cloud's underside overhead at noon is dark, $under"
cmd voxel.tp 0.5 10 0.5

check "the sun's glow"
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

check "the pillar's shadow"
# A pillar of stone west of the room and its shadow on the grass under the sun of 8.30, the
# shadow's edges running slantwise and stepping along the pixels of the blocks they cross, and a
# poppy's shadow beside it, seen from the air to the southeast. The mobs are taken away first, so
# none wanders into the shadow.
cmd voxel.kill all
cmd voxel.fill 1 4 -9 1 8 -9 stone
cmd voxel.set -1 4 -5 poppy
cmd voxel.fly on
cmd voxel.time 8.5
cmd voxel.tp 6 9 -1
cmd voxel.look 305 -35
cmd frames.wait 2
capture shadow.png
# The shadow stands still as the day passes within one of the light's steps, a 36th of an hour, read
# across it 3 blocks west of the pillar at 8.49 and at 8.51, two hours of the step about 8.5, each
# point within a level of the other, where the light turning with the hour moved its edges and its
# light every frame. The shadow is first found across the line, so a line laid where none falls does
# not pass.
cmd voxel.hud off
cmd voxel.time 8.49
cmd frames.wait 20
ask voxel.line 1 1 1 1 1 1 2 > /dev/null
cmd frames.wait 2
before=$(ask voxel.line -2 4.001 -11.5 -2 4.001 -8 57)
poppy=$(ask voxel.line -1.25 4.001 -6.5 -1.25 4.001 -3.5 49)
cmd voxel.time 8.51
cmd frames.wait 6
ask voxel.line 1 1 1 1 1 1 2 > /dev/null
cmd frames.wait 2
after=$(ask voxel.line -2 4.001 -11.5 -2 4.001 -8 57)
echo "the pillar's shadow at 8.49 ${before%%; by place*}, at 8.51 ${after%%; by place*}"
printf '%s\n%s\n' "${before#*by place }" "${after#*by place }" | awk '
  NR == 1 { n = split($0, a, " ") }
  NR == 2 { m = split($0, b, " ") }
  END {
    lo = a[1]; hi = a[1]
    for (i = 1; i <= n; i++) { if (a[i] < lo) lo = a[i]; if (a[i] > hi) hi = a[i]; d = a[i] - b[i]; if (d > 1 || d < -1) exit 1 }
    exit !(n == 57 && m == 57 && lo < hi / 2)
  }' || fail "the pillar's shadow is not across the line or moved within one of the light's steps, at 8.49 $before, at 8.51 $after"
# The poppy's shadow is cut by its picture, as its walls are. Read across it a quarter of a block
# west of its cell, 1 to 10 of 49 points are under 100, where the shadow of its whole walls there is
# 15 and the lit grass 154.
dark=$(printf '%s\n' "${poppy#*by place }" | tr ' ' '\n' | awk '$1 < 100 { n++ } END { print n + 0 }')
echo "the poppy's shadow, $dark of 49 points dark"
[ "$dark" -ge 1 ] && [ "$dark" -le 10 ] || fail "the poppy casts no shadow or its walls' whole shadow, $dark of 49 points dark, $poppy"
cmd voxel.hud on

check "the creative window"
# The window E opens in creative mode, every item's icon in it and the hotbar's below. While it is
# open W walks the player nowhere, as the source's screens take the keyboard.
cmd input.key E 1
cmd frames.wait 2
capture inventory.png
before=$(feet "$(ask voxel.state)")
cmd input.key W 10
after=$(feet "$(ask voxel.state)")
[ -n "$before" ] && [ "$before" = "$after" ] || fail "W walked the player with the creative window open, from $before to $after"
cmd input.key E 1
# The stone in the hand stands at the window's lower right, as the source holds a block, read there
# with the HUD shown and hidden, which hides the hand with it, the two far apart.
ask voxel.color 1 1 1 1 > /dev/null
cmd frames.wait 2
held=$(ask voxel.color 1100 600 80 60)
cmd voxel.hud off
ask voxel.color 1 1 1 1 > /dev/null
cmd frames.wait 2
bare=$(ask voxel.color 1100 600 80 60)
cmd voxel.hud on
echo "the hand's corner $held, with the HUD hidden $bare"
printf '%s\n%s\n' "$held" "$bare" | sed -nE 's/^red ([0-9.]+), green ([0-9.]+), blue ([0-9.]+) .*/\1 \2 \3/p' | paste -sd ' ' \
  | awk 'NF == 6 { d = ($1 - $4) ^ 2 + ($2 - $5) ^ 2 + ($3 - $6) ^ 2; exit !(d > 40 ^ 2) } NF != 6 { exit 1 }' \
  || fail "the hand's block does not show at the window's lower right, $held, and with the HUD hidden $bare"

check "the swim to the bank"
# A pool of water four blocks deep east of the room, with a bank on its east side level with the
# water's top block, and the player in the water at its top facing the bank. W with Space held swims
# the player up to the top and steps it out onto the bank, as Minecraft's swimmer steps out, and it
# stands there once a jump Space made on the bank has come down. The step out ends on the bank
# whether a frame takes a sixtieth of a second or the twentieth the game counts at most, as on
# lavapipe, and the bank ends at x 47, inside the columns lavapipe has loaded by then.
cmd voxel.fill 30 4 -20 47 8 -14 stone
cmd voxel.fill 31 5 -19 35 8 -15 water
cmd voxel.fly off
cmd voxel.tp 35.2 8 -17.5
cmd voxel.look 90 0
cmd frames.wait 10
cmd input.key Space 50 &
cmd input.key W 50
wait
cmd frames.wait 40
shore=$(ask voxel.state)
echo "$shore"
landed=$(feet "$shore")
printf '%s\n' "$shore" | grep -q "on the ground" && printf '%s\n' "$landed" | awk '{ exit !($1 > 36 && $2 >= 9 && $2 < 9.05) }' \
  || fail "the player did not step out of the water onto the bank, whose top is at 9, and stands at $landed"

check "the sheep's wool"
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
cmd frames.wait 20
ask voxel.color 1 1 1 1 > /dev/null
cmd frames.wait 2
wool=$(ask voxel.color 600 320 80 60)
echo "$wool"
channels=$(printf '%s\n' "$wool" | sed -nE 's/^red ([0-9.]+), green ([0-9.]+), blue ([0-9.]+) .*/\1 \2 \3/p')
printf '%s\n' "$channels" | awk 'NF == 3 { lo = $1; hi = $1; for (i = 2; i <= 3; i++) { if ($i < lo) lo = $i; if ($i > hi) hi = $i } exit !(lo >= 120 && hi <= 200 && hi - lo <= 20) } NF != 3 { exit 1 }' \
  || fail "the sheep's wool in shade reads $wool, where a gray between 120 and 200 with its channels within 20 of each other is asked"

check "the creeper's front"
# A creeper's front at noon, turned from the sun, shows no bands of its own shadow. Read down the
# sunlit part of its body's plain green from a block and a half, no two neighboring points are more
# than 2 levels apart, where the floor of its light looked up with the sun's shadow on the face itself
# laid bands that read up to 10 levels apart. The creeper is first found where the line is laid, so
# the plain ground behind an empty place does not pass.
cmd voxel.kill all
cmd voxel.tp 6.5 4 12.5
cmd voxel.look 0 0
cmd frames.wait 2
cmd voxel.spawn creeper 0
mobs=$(ask voxel.mobs 1)
[[ "$mobs" == *"creeper "*" at 6.5, 4.0, 9.5,"* ]] || fail "the creeper does not stand at 6.5, 4.0, 9.5, $mobs"
cmd voxel.tp 6.5 4 11.0
cmd voxel.look 0 -25
cmd frames.wait 20
ask voxel.line 1 1 1 1 1 1 2 > /dev/null
cmd frames.wait 2
front=$(ask voxel.line 6.5 4.71875 9.635 6.5 4.40625 9.635 61)
echo "creeper's front ${front%%; by place*}"
largest=$(printf '%s\n' "$front" | sed -nE 's/^from [0-9.]+ to [0-9.]+, largest step ([0-9.]+),.*/\1/p')
[ -n "$largest" ] || fail "voxel.line read no line down the creeper's front, saying $front"
awk -v l="$largest" 'BEGIN { exit !(l <= 2) }' || fail "the creeper's front shows bands at noon, $front"

check "the zombie's face"
# A zombie's face at noon, seen from its own height, is as bright above the eye's height as below
# it. Read down a column of the face clear of its features, no two neighboring points are more than 2
# levels apart, where the light of a face turned up, which floors the face's, taken as the eye saw it
# lit the face above the eye's height as one turned down, 15 levels darker.
cmd voxel.kill all
cmd voxel.tp 6.5 4 12.5
cmd voxel.look 0 0
cmd frames.wait 2
cmd voxel.spawn zombie 0
mobs=$(ask voxel.mobs 1)
[[ "$mobs" == *"zombie "*" at 6.5, 4.0, 9.5,"* ]] || fail "the zombie does not stand at 6.5, 4.0, 9.5, $mobs"
cmd voxel.tp 6.5 4.1 11.6
cmd voxel.look 0 -5
cmd frames.wait 20
ask voxel.line 1 1 1 1 1 1 2 > /dev/null
cmd frames.wait 2
face=$(ask voxel.line 6.71875 5.96875 9.765 6.71875 5.53125 9.765 33)
echo "zombie's face ${face%%; by place*}"
largest=$(printf '%s\n' "$face" | sed -nE 's/^from [0-9.]+ to [0-9.]+, largest step ([0-9.]+),.*/\1/p')
[ -n "$largest" ] || fail "voxel.line read no line down the zombie's face, saying $face"
awk -v l="$largest" 'BEGIN { exit !(l <= 2) }' || fail "the zombie's face steps in light at the eye's height, $face"

check "the high sapling"
# A sapling on a pillar above the eye's height is lit as one on the ground below it, read at the
# middle of each one's crown, where its crossed walls are covered. Its green is at least four fifths
# of the lower's, where a plant's wall seen from below was lit as one facing down, at half of it.
cmd voxel.kill all
cmd voxel.fill 8 4 0 10 6 1 stone
cmd voxel.set 9 7 0 oak_sapling
cmd voxel.set 9 4 3 oak_sapling
cmd voxel.tp 9.5 4 9.5
cmd voxel.look 0 0
cmd frames.wait 20
ask voxel.pixel 1 1 1 > /dev/null
cmd frames.wait 2
high=$(ask voxel.pixel 9.5 7.53 0.5)
low=$(ask voxel.pixel 9.5 4.53 3.5)
echo "the high sapling's crown $high, the low one's $low"
hg=$(printf '%s\n' "$high" | sed -nE 's/^red [0-9]+, green ([0-9]+), .*/\1/p')
lg=$(printf '%s\n' "$low" | sed -nE 's/^red [0-9]+, green ([0-9]+), .*/\1/p')
[ -n "$hg" ] && [ -n "$lg" ] || fail "voxel.pixel read no sapling, saying $high and $low"
[ $((5 * hg)) -ge $((4 * lg)) ] || fail "a plant above the eye's height is lit darker than one below it, $high against $low"

check "the grass wall"
# A wall of grass blocks at noon, seen from the player's height, is as bright above the eye's height
# as below it. The same texel of each block's side, in its dirt under the grass's fringe, is read
# from the block the eye's line crosses to three over it, and no two neighbors are more than 2 levels
# of luminance apart, where a face lit as one turned from the eye would step at the eye's line.
cmd voxel.fill -6 4 -40 6 11 -40 grass
cmd voxel.tp 0.5 4 -32.5
cmd voxel.look 0 0
cmd frames.wait 20
ask voxel.pixel 1 1 1 > /dev/null
cmd frames.wait 2
wall=""
for y in 5 6 7 8 9; do wall="$wall $(ask voxel.pixel 0.30 $y.30 -38.99 | sed -nE 's/^red ([0-9]+), green ([0-9]+), blue ([0-9]+).*/\1 \2 \3/p' | awk '{ printf "%.1f", 0.2126 * $1 + 0.7152 * $2 + 0.0722 * $3 }')"; done
echo "the grass wall's luminance from the eye's line up,$wall"
read -ra levels <<< "$wall"
[ "${#levels[@]}" -eq 5 ] || fail "voxel.pixel read no grass wall, reading$wall"
awk -v w="$wall" 'BEGIN { n = split(w, l, " "); for (i = 2; i <= n; i++) { d = l[i] - l[i - 1]; if (d > 2 || d < -2) exit 1 } }' \
  || fail "the grass wall steps in light at the eye's height,$wall"
cmd voxel.fill -6 4 -40 6 11 -40 air

check "the tall grass"
# A tall grass two blocks high on a pillar clear of the rooms built above, seen from a little below
# against the sky, shows the sky alone in its upper half's top row, which its picture leaves clear,
# read at sixteen points along one of its walls a fiftieth under its top, where a place a hair under
# 0 wrapped to the picture's bottom row of blades. The eye flies a third of a block under that row,
# so the way to it passes over the blades of the other wall and the sky behind is the deep blue
# over the horizon's haze.
cmd voxel.fill 30 4 20 30 5 20 stone
cmd voxel.set 30 6 20 tall_grass
cmd voxel.set 30 7 20 tall_grass_top
cmd voxel.fly on
cmd voxel.tp 30.5 6 21.8
cmd voxel.look 0 12
cmd frames.wait 20
ask voxel.pixel 1 1 1 > /dev/null
cmd frames.wait 2
for i in $(seq 0 15); do
  top=$(ask voxel.pixel "$(echo "30.05 + $i * 0.06" | bc -l)" 7.98 "$(echo "20.05 + $i * 0.06" | bc -l)")
  read -r tr tg tb <<< "$(printf '%s\n' "$top" | sed -nE 's/^red ([0-9]+), green ([0-9]+), blue ([0-9]+) .*/\1 \2 \3/p')"
  [ -n "$tb" ] || fail "voxel.pixel read nothing at the tall grass's top, saying $top"
  [ "$tb" -gt "$tg" ] || fail "the tall grass's clear top row shows more than the sky, $top"
done

check "the pool's reflection"
# A pool beside a wall of red concrete at noon reflects nothing of the wall. Read on the water where
# the wall's mirror image would lie, its red stays at least 8 levels under its green, where the
# glossy water reflecting the wall through the scene's distance field read red 134 to green 132. Its
# bed is white concrete, so the red of the dirt a block of water shows does not read as the wall's.
cmd voxel.fill 60 2 30 66 2 36 white_concrete
cmd voxel.fill 60 3 30 66 3 36 water
cmd voxel.fill 60 4 29 66 8 29 red_concrete
cmd voxel.tp 63.5 4 40
cmd voxel.look 0 -8
cmd frames.wait 30
ask voxel.pixel 1 1 1 > /dev/null
cmd frames.wait 2
mirror=$(ask voxel.pixel 63.5 3.876 33.5)
echo "the water where the wall would mirror, $mirror"
read -r wr wg wb <<< "$(printf '%s\n' "$mirror" | sed -nE 's/^red ([0-9]+), green ([0-9]+), blue ([0-9]+) .*/\1 \2 \3/p')"
[ -n "$wb" ] || fail "voxel.pixel read no water, saying $mirror"
[ $((wr + 8)) -le "$wg" ] || fail "the water reflects the red wall beside it, $mirror"

check "the water's depth"
# A pool lined with red concrete at noon, one lane a block deep and one five, seen from above while
# flying. The red under a block of water shows plainly, its red at least 40 levels over its green,
# where water of any depth hid a bed alike behind its own alpha, 16 over, and under five blocks the
# water's own darker color hides it, its red at least 25 levels under its green. Seen low across the
# pool, the deep lane's far end, where the eye's mirror reaches the sky over the horizon, is lighter
# than its near end by at least 32 levels of luminance, where water showing no sky read 23 lighter,
# as calm water shows the sky the lower the eye looks.
cmd voxel.fill 80 -3 40 92 3 56 red_concrete
cmd voxel.fill 80 3 40 84 3 56 water
cmd voxel.fill 88 -1 40 92 3 56 water
cmd voxel.fly on
cmd voxel.tp 86.5 6 60
cmd voxel.look 0 -35
cmd frames.wait 30
ask voxel.pixel 1 1 1 > /dev/null
cmd frames.wait 2
shallow=$(ask voxel.pixel 82.5 3.88 49.5)
deep=$(ask voxel.pixel 90.5 3.88 49.5)
echo "the red bed under a block of water $shallow, under five $deep"
read -r sr sg _ <<< "$(printf '%s\n' "$shallow" | sed -nE 's/^red ([0-9]+), green ([0-9]+), blue ([0-9]+).*/\1 \2 \3/p')"
read -r dr dg _ <<< "$(printf '%s\n' "$deep" | sed -nE 's/^red ([0-9]+), green ([0-9]+), blue ([0-9]+).*/\1 \2 \3/p')"
[ -n "$sg" ] && [ -n "$dg" ] || fail "voxel.pixel read no water in the pool, saying $shallow and $deep"
[ "$sr" -ge $((sg + 40)) ] || fail "the red bed under a block of water does not show plainly, $shallow"
[ $((dr + 25)) -le "$dg" ] || fail "the red bed under five blocks of water shows through, $deep"
cmd voxel.look 0 -8
cmd frames.wait 30
ask voxel.pixel 1 1 1 > /dev/null
cmd frames.wait 2
near=$(ask voxel.pixel 90.5 3.88 55.5)
far=$(ask voxel.pixel 90.5 3.88 41.5)
echo "the deep lane seen low, near $near, far $far"
lum() { printf '%s\n' "$1" | sed -nE 's/^red ([0-9]+), green ([0-9]+), blue ([0-9]+).*/\1 \2 \3/p' | awk '{ printf "%d", 0.2126 * $1 + 0.7152 * $2 + 0.0722 * $3 }'; }
nl=$(lum "$near"); fl=$(lum "$far")
[ -n "$nl" ] && [ -n "$fl" ] || fail "voxel.pixel read no water in the deep lane, saying $near and $far"
[ $((nl + 32)) -le "$fl" ] || fail "the water does not show the sky the lower the eye looks across it, near $near, far $far"
cmd voxel.fill 80 -3 40 92 -1 56 air
cmd voxel.fill 80 0 40 92 0 56 bedrock
cmd voxel.fill 80 1 40 92 2 56 dirt
cmd voxel.fill 80 3 40 92 3 56 grass

check "the pack's pictures"
# A zombie wears a pack's picture of it laid as the source lays it, glass is cut out where its
# picture is clear, and a lit furnace shows its top's picture above and glows at its front. The
# pack, a folder of pictures written here as PNGs with perl's own zlib, holds the zombie's, half as
# tall as its layout as an older pack's is, red where the source lays the head's front, clear where
# it lays the hat's layer, so the hat is left out, and blue elsewhere; glass's, a gray frame about a
# clear middle; the lit furnace's front in red and the furnace's top in blue; and water's, two
# frames played a tick each, white and black, so readings a whole number of slow frames apart, as on
# lavapipe at about two seconds a frame, fall on either rather than each on the same half second.
textures="$data/picture-pack/assets/minecraft/textures"
mkdir -p "$textures/entity/zombie" "$textures/block"
printf '{"pack":{"pack_format":34,"description":"Pictures in plain colors for the play script"}}' > "$data/picture-pack/pack.mcmeta"
picture() {
  perl -MCompress::Zlib -e '
    my ($w, $h, $rule) = @ARGV;
    my %rules = (
      zombie => sub { my ($x, $y) = @_; $x >= 8 && $x < 16 && $y >= 8 && $y < 16 ? "\xff\x00\x00\xff" : $x >= 32 && $y < 16 ? "\x00\x00\x00\x00" : "\x00\x00\xff\xff" },
      glass => sub { my ($x, $y) = @_; $x >= 4 && $x < 12 && $y >= 4 && $y < 12 ? "\x00\x00\x00\x00" : "\xc8\xc8\xc8\xff" },
      red => sub { "\xff\x00\x00\xff" },
      blue => sub { "\x00\x00\xff\xff" },
      halves => sub { my ($x, $y) = @_; $y < 16 ? "\xff\xff\xff\xff" : "\x00\x00\x00\xff" },
      flames => sub { my ($x, $y) = @_; my ($f, $r) = (int($y / 16), $y % 16); $r >= 1 && ($x * 5 + $r * 3 + $f * 7) % 16 < $r * 2 ? "\xff\xc0\x10\xff" : "\xff\xff\xff\x00" },
    );
    my $raw = "";
    for my $y (0 .. $h - 1) { $raw .= "\0"; $raw .= $rules{$rule}->($_, $y) for 0 .. $w - 1; }
    sub chunk { my ($type, $data) = @_; pack("N", length $data) . $type . $data . pack("N", crc32($type . $data)) }
    binmode STDOUT;
    print "\x89PNG\r\n\x1a\n", chunk("IHDR", pack("NNCCCCC", $w, $h, 8, 6, 0, 0, 0)), chunk("IDAT", compress($raw)), chunk("IEND", "");
  ' "$@"
}
picture 64 32 zombie > "$textures/entity/zombie/zombie.png"
picture 16 16 glass > "$textures/block/glass.png"
picture 16 16 red > "$textures/block/furnace_front_on.png"
picture 16 16 blue > "$textures/block/furnace_top.png"
picture 16 32 halves > "$textures/block/water_still.png"
printf '{"animation":{"frametime":1}}' > "$textures/block/water_still.png.mcmeta"
picture 16 512 flames > "$textures/block/fire_0.png"
printf '{"animation":{"frametime":1}}' > "$textures/block/fire_0.png.mcmeta"
pack="$data/picture-pack"
if command -v cygpath > /dev/null; then pack=$(cygpath -m "$pack"); fi
cmd voxel.packs "$pack"
cmd voxel.kill all
cmd voxel.tp 6.5 4 12.5
cmd voxel.look 0 0
cmd frames.wait 2
cmd voxel.spawn zombie 0
cmd voxel.tp 6.5 4 11.5
cmd voxel.look 0 -8
cmd frames.wait 20
ask voxel.pixel 1 1 1 > /dev/null
cmd frames.wait 2
front=$(ask voxel.pixel 6.5 5.75 9.76)
body=$(ask voxel.pixel 6.5 5.125 9.635)
echo "zombie's face $front, its body $body"
read -r fr fg fb <<< "$(printf '%s\n' "$front" | sed -nE 's/^red ([0-9]+), green ([0-9]+), blue ([0-9]+) .*/\1 \2 \3/p')"
read -r br bg bb <<< "$(printf '%s\n' "$body" | sed -nE 's/^red ([0-9]+), green ([0-9]+), blue ([0-9]+) .*/\1 \2 \3/p')"
[ -n "$fb" ] && [ -n "$bb" ] || fail "voxel.pixel read no zombie, saying $front and $body"
[ "$fr" -gt $((2 * fb)) ] && [ "$fr" -gt $((2 * fg)) ] || fail "the zombie's face does not wear the picture's red, $front"
[ "$bb" -gt $((2 * br)) ] || fail "the zombie's body does not wear the picture's blue, $body"
# Two panes of glass side by side before a wall of red concrete show the wall through their clear
# middles and one gray frame about both, the frame left out where they meet, so the wall shows a
# tenth of a block to either side of the join, and a lit furnace below them its blue top above its
# red front. The eye stands level with the glass, straight before the first pane, so the line
# through its middle meets the wall and no other face of it. The frame's gray is read by its red
# against its green, the wall's red lying far from its green, since the sun's warm light, which the
# sky's light whitened by day no longer cools, leaves a gray's blue below its red.
cmd voxel.fill 19 5 0 22 8 0 red_concrete
cmd voxel.set 20 6 2 glass
cmd voxel.set 21 6 2 glass
cmd voxel.set 21 4 2 lit_furnace
cmd voxel.tp 20.5 4.88 5.5
cmd voxel.look 0 -15
cmd frames.wait 20
ask voxel.pixel 1 1 1 > /dev/null
cmd frames.wait 2
middle=$(ask voxel.pixel 20.5 6.5 3.001)
frame=$(ask voxel.pixel 20.1 6.5 3.001)
before=$(ask voxel.pixel 20.9 6.5 3.001)
after=$(ask voxel.pixel 21.1 6.5 3.001)
side=$(ask voxel.pixel 21.5 4.5 3.001)
top=$(ask voxel.pixel 21.5 5.001 2.5)
echo "glass's middle $middle, its frame $frame, the join $before and $after, the lit furnace's front $side, its top $top"
channels() { printf '%s\n' "$1" | sed -nE 's/^red ([0-9]+), green ([0-9]+), blue ([0-9]+) .*/\1 \2 \3/p'; }
read -r mr mg mb <<< "$(channels "$middle")"
read -r gr gg gb <<< "$(channels "$frame")"
read -r br bg bb <<< "$(channels "$before")"
read -r ar ag ab <<< "$(channels "$after")"
read -r sr sg sb <<< "$(channels "$side")"
read -r tr tg tb <<< "$(channels "$top")"
[ -n "$mb" ] && [ -n "$gb" ] && [ -n "$bb" ] && [ -n "$ab" ] && [ -n "$sb" ] && [ -n "$tb" ] || fail "voxel.pixel read no glass or furnace, saying $middle, $frame, $before, $after, $side and $top"
[ "$mr" -gt $((2 * mg)) ] && [ "$mr" -gt $((2 * mb)) ] || fail "the glass's clear middle does not show the red wall behind it, $middle"
[ $((gr > gg ? gr - gg : gg - gr)) -le 40 ] && [ "$gr" -gt 60 ] || fail "the glass's frame does not show its gray, $frame"
[ "$br" -gt $((2 * bg)) ] && [ "$ar" -gt $((2 * ag)) ] || fail "the two panes' frames are not left out where they meet, $before and $after"
[ "$sr" -gt $((2 * sb)) ] || fail "the lit furnace's front does not wear the picture's red, $side"
[ "$tb" -gt $((2 * tr)) ] || fail "the lit furnace's top does not wear the furnace's top's blue, $top"
# A furnace dropped on the ground wears the pack's pictures as the block does, its top read from
# straight above in the picture's blue, where a stack on the ground was a cube of the block's colors.
cmd voxel.drop furnace 23.5 4.2 4.5
cmd voxel.tp 23.5 8 4.5
cmd voxel.look 0 -90
cmd frames.wait 10
ask voxel.pixel 1 1 1 > /dev/null
cmd frames.wait 2
dropped=$(ask voxel.pixel 23.5 4.3 4.5)
echo "the dropped furnace's top $dropped"
read -r dr dg db <<< "$(channels "$dropped")"
[ -n "$db" ] && [ "$db" -gt $((2 * dr)) ] && [ "$db" -gt $((2 * dg)) ] || fail "the dropped furnace's top does not wear the furnace's top's blue, $dropped"
# A pool of water a block across shows the written water's frames in turn, read eight times three
# tenths of a second apart and three and a half by turns, six ticks and seven, so readings that
# keep time with the frames still fall on each, at least once white under the water's tint and
# once black.
cmd voxel.set 26 3 2 water
cmd voxel.tp 26.5 5 4.5
cmd voxel.look 0 -50
cmd frames.wait 10
greens=""
for i in $(seq 1 8); do
  sleep "0.3$((i % 2 * 5))"
  cmd frames.wait 1
  pool=$(ask voxel.pixel 26.5 3.876 2.5)
  read -r wr wg wb <<< "$(channels "$pool")"
  [ -n "$wb" ] || fail "voxel.pixel read no water, saying $pool"
  greens="$greens $wg"
done
echo "the water's green over the frames:$greens"
printf '%s\n' $greens | sort -n | awk 'NR == 1 { low = $1 } { high = $1 } END { exit !(high - low >= 40) }' \
  || fail "the water's texture does not step through its frames, its green reading$greens"

check "the fire's top"
# A fire's walls show no line along their tops. The pack's fire is 32 frames of flames reaching the
# second row under pixels clear and white, as a pack's are, and seen from above at a slant at night
# three lines down across the top edge of its north wall, read nine times a few frames apart, are
# each no brighter than the ground beyond the wall where they start, where a texel at the wall's
# top read into the copy of the frame above it in its cell and drew its bottom rows as a line.
cmd voxel.kill all
cmd voxel.time 20
cmd voxel.set 40 3 -50 stone
cmd voxel.set 40 4 -50 fire
cmd voxel.tp 40.5 5.5 -45
cmd voxel.look 0 -12
cmd frames.wait 30
ask voxel.line 1 1 1 1 1 1 2 > /dev/null
cmd frames.wait 2
tops=""
for x in 40.3 40.5 40.7; do
  for i in 1 2 3; do
    top=$(ask voxel.line "$x" 5.44 -49.9994 "$x" 5.36 -49.9994 40 | sed -nE 's/.*by place (.*)$/\1/p')
    tops="$tops $(printf '%s\n' $top | awk 'NR == 1 { first = $1 } $1 > high { high = $1 } END { printf "%.1f", high - first }')"
    cmd frames.wait 5
  done
done
cmd voxel.set 40 4 -50 air
echo "the brightest over the ground across a fire's top edge:$tops"
[ -n "${tops// /}" ] || fail "voxel.line read nothing across a fire's top edge"
printf '%s\n' $tops | awk '$1 > 15 { bad = 1 } END { exit bad }' || fail "a fire's walls show a line along their tops, brighter than the ground by$tops"
cmd voxel.packs none

check "the far pillar"
# A far pillar high against the sky fades into the sky drawn behind it rather than the horizon's
# color, read at a height of 60 on its face and on the sky four blocks east of it, the two within a
# few levels and both the sky's deep blue, far from the horizon's paler color. It stands 59 blocks
# out, inside the four columns drawn and past the haze's end at 61.
cmd voxel.hud off
cmd voxel.time 12
cmd voxel.fill -1 4 -61 1 80 -59 stone
cmd voxel.fly on
cmd voxel.tp 0.5 5 0.5
cmd voxel.look 0 30
meshed
cmd frames.wait 10
pillar=$(ask voxel.pixel 0.5 60 -58.9)
beside=$(ask voxel.pixel 4.5 60 -58.9)
echo "the far pillar $pillar, the sky beside it $beside"
read -r pr pg pb <<< "$(channels "$pillar")"
read -r sr sg sb <<< "$(channels "$beside")"
[ -n "$pb" ] && [ -n "$sb" ] || fail "voxel.pixel read no far pillar or sky, saying $pillar and $beside"
awk -v p="$pr $pg $pb" -v s="$sr $sg $sb" 'BEGIN { split(p, a); split(s, b); for (i = 1; i <= 3; i++) if (a[i] - b[i] > 4 || b[i] - a[i] > 4) exit 1; exit !(a[3] - a[1] > 100) }' \
  || fail "the far pillar high against the sky is not the sky's color beside it, $pillar against $beside"

check "the block's bits"
# The bits a broken block throws at the end of a tunnel two high and one wide in a hill at noon, left
# to fall a moment and held, each read where it lies, the one against the tunnel's wall within 6
# levels of the rest, since each is lit at its middle, where one clipped into the wall took the
# sky's light from inside it, 29, 49 and 51 against 16, 25 and 13.
cmd voxel.time 12
cmd voxel.kill all
cmd voxel.fill 60 4 -70 70 12 -60 stone
cmd voxel.fill 65 4 -61 65 5 -60 air
cmd voxel.tp 65.5 4 -58.5
cmd voxel.fly off
cmd voxel.look 0 -2
cmd frames.wait 30
cmd voxel.break
cmd frames.wait 25
cmd voxel.still on
cmd frames.wait 10
ask voxel.pixel 1 1 1 > /dev/null
cmd frames.wait 2
levels=""
seen=""
for place in "65.02 4.05 -60.5" "65.98 4.05 -60.5" "65.02 5.05 -61.5" "65.98 5.05 -61.5" "65.5 4.05 -61.5"; do
  bits=$(ask voxel.bits $place)
  bit=$(printf '%s\n' "$bits" | sed -nE 's/.* the nearest at (-?[0-9.]+, -?[0-9.]+, -?[0-9.]+)$/\1/p')
  [ -n "$bit" ] || fail "breaking the tunnel's end threw no bits, $bits"
  [[ "$seen" == *"[$bit]"* ]] && continue
  seen="$seen[$bit]"
  read -r r g b <<< "$(channels "$(ask voxel.pixel ${bit//,/})")"
  [ -n "$b" ] && levels="$levels $(( (r + g + b) / 3 ))"
done
cmd voxel.still off
cmd voxel.fly on
echo "the tunnel's bits:$levels"
[ -n "${levels// /}" ] || fail "voxel.pixel read none of the tunnel's bits"
printf '%s\n' $levels | sort -n | awk 'NR == 1 { low = $1 } { high = $1 } END { exit !(high - low <= 6) }' \
  || fail "a broken block's bit by the tunnel's wall takes light through the wall, its levels among the rest$levels"

check "the plant's place"
# A block placed aimed at a plant takes the plant's own cell, and one placed against a block's side
# above the plant standing before it, where the aim passes over the plant, takes the plant's cell
# too, as the source's grasses give way, where the first went into the cell before the plant and the
# second was refused.
cmd voxel.time 12
cmd voxel.set 30 4 -30 short_grass
cmd voxel.set 34 4 -30 stone
cmd voxel.set 34 4 -29 short_grass
cmd voxel.select stone
# The look from feet at a place to a point, its heading clockwise from north and its pitch up.
look_at() { awk -v fx="$1" -v fy="$2" -v fz="$3" -v x="$4" -v y="$5" -v z="$6" 'BEGIN { dx = x - fx; dy = y - fy - 1.62; dz = z - fz; h = atan2(dx, -dz) * 180 / 3.14159265; if (h < 0) h += 360; printf "%.3f %.3f", h, atan2(dy, sqrt(dx * dx + dz * dz)) * 180 / 3.14159265 }'; }
cmd voxel.tp 30.5 4 -26.5
cmd voxel.look $(look_at 30.5 4 -26.5 30.5 4.3 -29.5)
cmd frames.wait 2
cmd voxel.place
aimed=$(ask voxel.light 30 4 -30)
before=$(ask voxel.light 30 4 -29)
cmd voxel.tp 34.5 4.38 -26.5
cmd voxel.look $(look_at 34.5 4.38 -26.5 34.5 4.95 -28.99)
cmd frames.wait 2
cmd voxel.place
beside=$(ask voxel.light 34 4 -29)
echo "aimed at the grass, ${aimed%%,*} in its cell and ${before%%,*} before it; against the stone's side, ${beside%%,*} in the grass's cell"
[[ "$aimed" == stone* && "$before" == air* ]] || fail "a block placed aimed at a plant did not take the plant's cell, $aimed and before it $before"
[[ "$beside" == stone* ]] || fail "a block placed against a block's side into a plant's cell did not take it, $beside"

check "the held torch"
# At midnight with a torch held, the ground's light about the player reads the same from four
# headings, every sample the frame shows of a ring two blocks out within 3 levels, since the light
# stands at the player's own place, where at the hand it swung with each turn of the view.
cmd voxel.time 0
cmd voxel.fly off
cmd voxel.select torch
cmd voxel.tp 20.5 4 -40.5
cmd frames.wait 60
ask voxel.ring 20.5 4.001 -40.5 2 8 > /dev/null
samples=""
for heading in 0 90 180 270; do
  cmd voxel.look "$heading" -60
  cmd frames.wait 40
  ring=$(ask voxel.ring 20.5 4.001 -40.5 2 8)
  samples="$samples $(printf '%s\n' "$ring" | sed -nE 's/.*by angle (.*)$/\1/p' | tr ' ' '\n' | grep -v NaN | tr '\n' ' ')"
done
cmd voxel.fly on
echo "the ground's light about a held torch from four headings:$samples"
[ -n "${samples// /}" ] || fail "voxel.ring read no ground about the player holding a torch"
printf '%s\n' $samples | sort -n | awk 'NR == 1 { low = $1 } { high = $1 } END { exit !(high - low <= 3) }' \
  || fail "a held torch's light on the ground turns with the view, reading$samples"

check "the held pickaxe"
# A wooden pickaxe held against the sky at noon, the player standing still and looking up to the
# south over none of the script's builds, lets no sky through between its faces, every pixel's faces
# taking their corners from one grid so faces that meet share them, where a front and a back of the
# whole picture cut out where it is clear met the pixels' sides a little off their edges and the sky
# showed through between them. Its corner of the window holds at most 60 pixels nearer the sky than
# both neighbors, its own slivers of a lit face a pixel wide reading 22 of them, where the cracks
# made 224, on the GPU and on lavapipe alike.
cmd voxel.time 12
cmd voxel.kill all
cmd voxel.hud on
cmd voxel.select wooden_pickaxe
cmd voxel.tp 0.5 4 0.5
cmd voxel.look 180 70
cmd frames.wait 20
ask voxel.gaps 940 330 339 389 > /dev/null
cmd frames.wait 2
gaps=$(ask voxel.gaps 940 330 339 389)
echo "a held wooden pickaxe against the sky, $gaps"
count=$(printf '%s\n' "$gaps" | sed -nE 's/^([0-9]+) gaps .*/\1/p')
[ -n "$count" ] && [ "$count" -le 60 ] || fail "a held pickaxe lets the sky through between its faces, $gaps"

check "the item's name"
# The chosen item's name over the hotbar shows as the choice changes and is gone once it has stood a
# second and a half and faded over half a second, as the source's fades, read over a strip of the
# sky above the hotbar where it lies, its red raised by the white letters while it shows, by 8 levels
# for dirt's, and the sky's own after.
cmd voxel.select dirt
cmd frames.wait 2
shown=$(ask voxel.color 520 580 240 32)
cmd frames.wait 180
gone=$(ask voxel.color 520 580 240 32)
echo "the item's name's strip $shown, after the hold $gone"
sr=$(printf '%s\n' "$shown" | sed -nE 's/^red ([0-9.]+),.*/\1/p')
gr=$(printf '%s\n' "$gone" | sed -nE 's/^red ([0-9.]+),.*/\1/p')
awk -v s="${sr:-0}" -v g="${gr:-0}" 'BEGIN { exit !(s - g > 4) }' \
  || fail "the chosen item's name does not show and then fade, its strip $shown and after the hold $gone"

check "the jungle's floor"
# The grass under a canopy of jungle leaves two deep at noon, at the sky level of 13 the leaves leave
# it, read from straight above against the grass in the shade of a wall on open ground, at sky level
# 15, the two within three of the source's levels of each other, either at least 0.72 of the other's
# light, since the terrain's floor of the sky's light lifts the canopy's floor, which the scene's
# field holds the leaves solid over, where it read half the shade's light, 20 against 42. The torch
# held before is put away, since its light would fall on both.
cmd voxel.time 12
cmd voxel.kill all
cmd voxel.select stone
cmd voxel.hud off
cmd voxel.fly on
cmd voxel.fill -60 9 20 -40 10 40 jungle_leaves
cmd voxel.fill -40 4 60 -30 14 60 stone
lit=""
for place in "-50.5 30.5" "-34.5 59.5"; do
  read -r x z <<< "$place"
  cmd voxel.tp "$x" 6.5 "$z"
  cmd voxel.look 0 -89
  meshed
  cmd frames.wait 40
  ask voxel.pixel 1 1 1 > /dev/null
  cmd frames.wait 2
  read -r r g b <<< "$(channels "$(ask voxel.pixel "$x" 4.001 "$z")")"
  [ -n "$b" ] || fail "voxel.pixel read no grass at $x, 4, $z"
  lit="$lit $(awk -v r="$r" -v g="$g" -v b="$b" 'BEGIN { printf "%.1f", 0.2126 * r + 0.7152 * g + 0.0722 * b }')"
done
read -r floor shade <<< "$lit"
sky=$(ask voxel.light -51 4 30)
echo "the jungle's floor at noon $floor against the open ground's shade $shade, $sky"
[[ "$sky" == *", sky 13, "* ]] || fail "the grass under jungle leaves two deep is not at sky level 13, $sky"
awk -v f="$floor" -v s="$shade" 'BEGIN { exit !(s > 10 && f >= 0.72 * s && s >= 0.72 * f) }' \
  || fail "the jungle's floor at noon reads $floor against the open ground's shade $shade, more than three of the source's levels apart"

check "the overworld's loading"
# Seed 1's overworld, the player standing over each of the eight biomes the climate gained, the
# nearest of each that voxel.biome finds, its name read back from voxel.state and its land captured
# from above, at a render distance of two columns, so lavapipe generates few at each, set before the
# world opens so no more are asked for meanwhile.
cmd voxel.hud on
cmd voxel.distance 2
# The overworld opens behind its loading screen, held up here to be read, the window's corner the
# screen's black while the state says it loads, and once it is let go and the world stands the
# player is set over the origin, and from above the trees there the corner is the sky's.
cmd voxel.loading on
# The run reads the caves' noises on the CPU, and one check below reads them on the GPU.
cmd voxel.gpu off
cmd voxel.world overworld 1
cmd frames.wait 2
state=$(ask voxel.state)
[[ "$state" == "loading overworld-1, "* ]] || fail "the overworld opened with no loading screen, $state"
screen=$(ask voxel.color 1220 16 40 24)
echo "$screen"
[[ "$screen" == "red 0.0, green 0.0, blue 0.0 "* ]] || fail "the loading screen's corner is not black, $screen"
cmd voxel.loading off
stood
state=$(ask voxel.state)
[[ "$state" == "feet at 0.50, "*", 0.50 in "* ]] || fail "the new overworld's player was not set over the origin, $state"
cmd voxel.fly on
cmd voxel.tp 0.5 110 0.5
cmd voxel.time 10
cmd voxel.look 0 10
cmd frames.wait 2
sky=$(ask voxel.color 1220 16 40 24)
echo "$sky"
printf '%s\n' "$sky" | sed -nE 's/^red ([0-9.]+), green ([0-9.]+), blue ([0-9.]+) .*/\3/p' | awk '{ exit !($1 > 100) }' \
  || fail "the world shown after its loading has no sky in the window's corner, $sky"
cmd voxel.fly on
cmd voxel.time 10
mkdir -p captures/biomes
for biome in swamp savanna jungle dark_forest badlands beach river cherry_grove; do
  check "the $biome"
  found=$(ask voxel.biome "$biome")
  read -r bx by bz <<< "$(printf '%s\n' "$found" | sed -nE 's/.* is at (-?[0-9]+), (-?[0-9]+), (-?[0-9]+),.*/\1 \2 \3/p')"
  [ -n "$bz" ] || fail "voxel.biome found no $biome, saying $found"
  cmd voxel.tp "$bx" "$((by + 24))" "$bz"
  cmd voxel.look 0 -45
  meshed
  cmd frames.wait 10
  state=$(ask voxel.state)
  [[ "$state" == *" in ${biome//_/ }, facing"* ]] || fail "the player over the $biome found at $bx, $bz stands in another biome, $state"
  capture "biomes/$biome.png"
done
check "the meadow"
# A meadow of the plains up close, its grass and flowers cut out of their crossed walls.
found=$(ask voxel.biome plains)
read -r bx by bz <<< "$(printf '%s\n' "$found" | sed -nE 's/.* is at (-?[0-9]+), (-?[0-9]+), (-?[0-9]+),.*/\1 \2 \3/p')"
[ -n "$bz" ] || fail "voxel.biome found no plains, saying $found"
cmd voxel.tp "$bx" "$((by + 3))" "$((bz + 6))"
cmd voxel.look 0 -20
meshed
cmd frames.wait 10
capture biomes/meadow.png
# The meadow again under Bare Bones where the machine has fetched it, the game's own looks of the
# grass's top, its blossoms, clover and pebbles, laid over the pack's grass, a machine without it, as
# the build workflow's, passing it by. A transient run's list of packs is its own and saved nowhere.
fetched="${XDG_DATA_HOME:-$HOME/.local/share}/VoxelSandbox/resourcepacks"
if [ -f "$fetched/Bare Bones 1.21.zip" ]; then
  cmd voxel.packs "$fetched/Bare Bones 1.21.zip"
  meshed
  cmd frames.wait 10
  capture biomes/meadow-pack.png
  cmd voxel.packs none
fi

check "the mineshaft's chest"
# Seed 1's nearest mineshaft to the origin, under the forest by the spawn, and the chest in a niche
# of a corridor north of its room, whose loot the first look rolls, rails or torches among it every
# time, as the third of the source's mineshaft pools gives.
cmd voxel.tp 0.5 110 0.5
found=$(ask voxel.find mineshaft)
[[ "$found" == "the nearest mineshaft is built from 71, 12, 36,"* ]] || fail "seed 1's nearest mineshaft moved, $found"
cmd voxel.tp 135.5 15 61.5
meshed
held=$(ask voxel.chest 134 15 58)
echo "$held"
[[ "$held" == *" rail"* || "$held" == *" torch"* ]] || fail "the mineshaft's chest at 134, 15, 58 holds no rails or torches, $held"

check "the GPU's caves"
# The caves' noises read on the GPU, lavapipe's in the build workflow, at the corners of the four
# columns about the player, the same to the bit as the CPU's, and the columns of a place not yet
# visited generated from what the GPU read.
cmd voxel.gpu on
verify=$(ask voxel.gpu verify 2)
echo "$verify"
[[ "$verify" == "0 of 147000 readings over 4 columns differ"* ]] || fail "the GPU's readings of the caves' noises are not the CPU's, $verify"
cmd voxel.tp 900.5 110 900.5
meshed
gpu=$(ask voxel.gpu)
echo "$gpu"
read=$(printf '%s\n' "$gpu" | sed -nE 's/.*, ([0-9]+) columns read since.*/\1/p')
[ "${read:-0}" -gt 0 ] || fail "no column of a new place was generated from the GPU's readings, $gpu"
cmd voxel.gpu off

check "the rain's splashes"
# On the meadow with the weather held turned the rain splashes about the player, which the frame's
# profile counts, burst from no more than the eleven emitters every kind together may hold, and what
# every kind keeps alive with the rings stays within the budget the profile gives. With Rain
# Splashes at none it splashes no more. The counts are eased over about a second, so each is read
# some seconds of frames after the change it shows, and the last falls under one drop.
profiled() { ask profile | sed -nE "s/^$1 (-?[0-9.]+).*/\1/p"; }
cmd voxel.tp "$bx" "$((by + 1))" "$bz"
cmd voxel.weather turned
cmd frames.wait 150
splashes=$(profiled particles.splashes)
emitters=$(profiled particles.emitters)
alive=$(ask profile | awk '/^particles\.(splashes|rings|leaves|dust|wakes) / { sum += $2 } END { print sum + 0 }')
budget=$(profiled particles.budget)
echo "the rain keeps $splashes drops of its splashes alive from $emitters emitters, $alive particles of every kind within a budget of $budget"
awk -v s="$splashes" -v e="$emitters" 'BEGIN { exit !(s > 5 && e > 0 && e <= 11) }' \
  || fail "the rain on the meadow keeps ${splashes:-no} drops of its splashes alive from ${emitters:-no} emitters, where more than 5 from 1 to 11 were written"
awk -v a="$alive" -v b="$budget" 'BEGIN { exit !(b > 0 && a <= b) }' || fail "the particles keep $alive alive over their budget of ${budget:-none}"
cmd voxel.options particles Rain_Splashes 0
cmd frames.wait 300
splashes=$(profiled particles.splashes)
awk -v s="$splashes" 'BEGIN { exit !(s != "" && s < 1) }' || fail "the rain on the meadow keeps ${splashes:-no} drops of its splashes alive with Rain Splashes at none"
cmd voxel.options particles Rain_Splashes 100
cmd voxel.weather days

check "the land's edge"
# From high over the land, the drawn land's edge meets the dome under the horizon in the haze's color
# with no step, read up the window's middle column: the land's last pixel and the one above it alike,
# and no two neighbors within 16 of the edge more than a few levels apart. It is read at four
# columns, the land within them generated and meshed first, so the edge lies past the haze's start.
cmd voxel.distance 4
cmd voxel.tp "$bx" 120 "$bz"
meshed
cmd voxel.look 0 -20
cmd frames.wait 10
ask voxel.edge 640 > /dev/null
cmd frames.wait 2
edge=$(ask voxel.edge 640)
echo "$edge"
read -r lr lg lb ar ag ab step <<< "$(printf '%s\n' "$edge" | sed -nE 's/^land ends at [0-9]+, land ([0-9]+) ([0-9]+) ([0-9]+), above ([0-9]+) ([0-9]+) ([0-9]+), largest step ([0-9]+)$/\1 \2 \3 \4 \5 \6 \7/p')"
[ -n "$step" ] || fail "voxel.edge found no edge of the land, saying $edge"
for pair in "$lr $ar" "$lg $ag" "$lb $ab"; do
  read -r land above <<< "$pair"
  [ $((land > above ? land - above : above - land)) -le 3 ] || fail "the land's edge meets the sky in another color, $edge"
done
[ "$step" -le 4 ] || fail "the land's edge steps to the sky, $edge"

check "the draws"
# Over the same land the frame records no more than 400 draws of sections, 277 when this was
# written, where drawing every section meshed at four columns, or with no flood leaving the land
# under the ground out, would record well over that.
state=$(ask voxel.state)
draws=$(printf '%s\n' "$state" | sed -nE 's/.* ([0-9]+) draws, .*/\1/p')
echo "the draws at the land's edge, $(printf '%s\n' "$state" | grep -oE '[0-9]+ sections, [0-9]+ draws, [0-9]+ triangles')"
[ -n "$draws" ] && [ "$draws" -le 400 ] || fail "the frame records more than 400 draws of sections at the land's edge, $(printf '%s\n' "$state" | grep -oE '[0-9]+ sections, [0-9]+ draws' || echo "$state")"

check "the options"
# Each setting of the options' own screen and of the Video, Render, Post Processing, Sky and
# Visuals, Particles and Music and Sounds screens is set to the middle of its reach, or to its low end where it stands there, or to
# its next choice, read back as set, through the engine's own state where the setting reads it
# there, and set back as it was, to its step's nearest where it stood between two, as the hour can.
# A setting is named in voxel.options by its words joined by underscores. The sky's colors'
# channels, which change only the game's own numbers, are walked by the tests, and the window's mode
# and vsync are left out, which the engine says of a window alone and an offscreen run has none.
same() { awk -v a="$1" -v b="$2" 'BEGIN { d = a - b; exit !(d < 0.001 && d > -0.001) }'; }
walked=0
for screen in general video render post sky particles sounds; do
  mapfile -t lines < <(ask voxel.options "$screen")
  for line in "${lines[@]}"; do
    name=${line%%: *}
    setting=${name// /_}
    [ "$screen $setting" = "video Window_Mode" ] || [ "$screen $setting" = "video VSync" ] && continue
    if [[ "$line" == *", of "* ]]; then
      shown=${line#*: }
      shown=${shown%%, of *}
      choices=${line##*, of }
      IFS=',' read -ra list <<< "${choices//, /,}"
      next=""
      for i in "${!list[@]}"; do [ "${list[$i]}" = "$shown" ] && next=${list[$(((i + 1) % ${#list[@]}))]}; done
      [ -n "$next" ] || fail "the $screen screen's $name shows $shown, none of its choices, $line"
      answer=$(ask voxel.options "$screen" "$setting" "${next// /_}")
      [[ "$answer" == "$name: $next, of "* ]] || fail "the $screen screen's $name set to $next reads $answer"
      answer=$(ask voxel.options "$screen" "$setting" "${shown// /_}")
      [[ "$answer" == "$name: $shown, of "* ]] || fail "the $screen screen's $name set back to $shown reads $answer"
    else
      read -r value _ low _ high _ step <<< "${line##*, }"
      to=$(awk -v l="$low" -v h="$high" -v s="$step" -v v="$value" 'BEGIN { m = l + int((h - l) / 2 / s + 0.5) * s; d = v - m; print (d < 0.001 && d > -0.001) ? l : m }')
      answer=$(ask voxel.options "$screen" "$setting" "$to")
      read -r now _ <<< "${answer##*, }"
      same "$now" "$to" || fail "the $screen screen's $name set to $to reads $answer"
      back=$(awk -v l="$low" -v s="$step" -v v="$value" 'BEGIN { print l + int((v - l) / s + 0.5) * s }')
      answer=$(ask voxel.options "$screen" "$setting" "$value")
      read -r now _ <<< "${answer##*, }"
      same "$now" "$back" || fail "the $screen screen's $name set back to $value reads $answer"
    fi
    walked=$((walked + 1))
  done
done
echo "the options screens' $walked settings each set and set back"
[ "$walked" -ge 48 ] || fail "the options screens hold $walked settings, where 48 were written"

./e3d stop --session "$pid" --quiet > /dev/null 2> "$said" || fail "./e3d stop ended with $?, $(head -n 1 "$said")"
[ -f "$log" ] || fail "the log e3d named, $log, is not there"
grep -q "Validation layers: ENABLED" "$log" || fail "the game drew without the validation layer"
if grep -F 'Validation Error' "$log"; then fail "the validation layer reported an error"; fi

check "the screenshot and the options file"
# F2 in a run that keeps its files writes the frame as a PNG into the screenshots folder beside the
# saves, named by the time. The list of packs is made empty first, so the run fetches none. The
# options file beside it is read as the run starts, a render distance of 5 taken and a corner shade
# it cannot read left at its default, and is written again as a setting changes.
mkdir -p "$data/VoxelSandbox"
: > "$data/VoxelSandbox/resourcepacks.txt"
printf 'render.Render_Distance:5\nrender.Corner_Shade:far\n' > "$data/VoxelSandbox/options.txt"
keep() { XDG_DATA_HOME="$data" ./e3d "$@"; }
keep open "$program" --offscreen --flat --world shot --json > captures/opened-kept.json || fail "the run that keeps its files did not open"
kept=$(perl -MJSON::PP -0777 -ne 'my $answer = eval { decode_json($_) } or exit; print $answer->{data}{pid} // ""' captures/opened-kept.json)
[ -n "$kept" ] || fail "e3d opened the run that keeps its files and did not say its process"
keep command frames.wait 30 --quiet --timeout 600 --session "$kept" 2> "$said" || fail "./e3d command frames.wait ended with $?, $(head -n 1 "$said")"
answer=$(keep command voxel.options render --timeout 600 --session "$kept" 2> "$said") || fail "./e3d command voxel.options ended with $?, $(head -n 1 "$said")"
grep -q '^Render Distance: 5 chunks, ' <<< "$answer" || fail "the options file's render distance of 5 was not read at the start, $(grep '^Render Distance' <<< "$answer")"
grep -q '^Corner Shade: 50%, ' <<< "$answer" || fail "the options file's corner shade it cannot read did not leave the default, $(grep '^Corner Shade' <<< "$answer")"
keep command voxel.options render Corner_Shade 30 --quiet --timeout 600 --session "$kept" 2> "$said" || fail "./e3d command voxel.options ended with $?, $(head -n 1 "$said")"
grep -qx 'render.Corner_Shade:30' "$data/VoxelSandbox/options.txt" && grep -qx 'render.Render_Distance:5' "$data/VoxelSandbox/options.txt" \
  || fail "the options file was not written as the corner shade changed, $(tr '\n' ' ' < "$data/VoxelSandbox/options.txt" | cut -c1-300)"
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

check ""
echo "Voxel Sandbox walked $distance blocks toward a glowstone in a closed room, saw a lantern's light fall smoothly across the floor, climbed out of a pool onto its bank, saw a sheep's wool in shade gray, saw a creeper's front at noon with no bands, saw a zombie's face as bright above the eye as below it, saw a sapling above the eye lit as one below, saw a grass wall as bright above the eye as below, saw the sky alone over a tall grass's blades, saw a pool reflect nothing of a red wall, saw deep water darker than shallow and the sky in water seen low, saw a zombie wear a pack's picture, saw a red wall through a pane of glass's clear middle and a lit furnace's top above its front, saw water's frames in turn, saw the stars at dawn darken none of the sky, saw a cloud's underside lit by day, saw a tunnel's bits lit alike by its walls, placed blocks into plants' cells, saw a held torch light the ground alike from four headings, saw no line along a fire's top, opened a mineshaft's chest of rails or torches, read the caves' noises on the GPU as on the CPU, saw the rain splash on a meadow within its particles' budget, drew no more than 400 draws of sections at the land's edge, set and set back each setting of the options screens, drew with no validation error, wrote $name on F2 and kept its options in a file"
