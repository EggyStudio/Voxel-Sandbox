#!/usr/bin/env bash
# A scripted run of the game through ./e3d, offscreen and under the validation layer, as the build
# workflow's play job runs it on lavapipe. On a flat world with nothing kept on disk it builds a
# closed room of stone with a glowstone block in it, puts the player in the room facing the lamp at
# night, walks toward it and captures the frame, then captures the night sky from the room's roof,
# the full moon and the stars over the flat land from two places and headings, the sun and its glow
# at noon and at dusk, a pillar's shadow under the morning sun, and the window of every item's icon,
# then swims the player out of a pool onto its bank and reads a white sheep's wool in the shade, and
# over seed 1's overworld stands the player over each of the eight biomes the climate gained and
# captures it and reads the drawn land's edge against the sky. A second run that keeps its files, in
# a data folder of its own, then takes a screenshot with F2. It fails where the game does not open
# or answer, the player did not move or did not climb out onto the bank, the wool in shade is not a
# gray, a biome is not found or not where it was found, the land's edge steps to the sky beyond it,
# the layer reported an error, or F2 left no screenshot of the window's size, and says why in an
# error annotation, so the run's page names it.
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
cmd frames.wait 120
ask voxel.line 1 1 1 1 1 1 2 > /dev/null
cmd frames.wait 2
pool=$(ask voxel.line 54.5625 5.001 -19.25 54.5625 5.001 -18.25 33)
echo "lantern's pool ${pool%%; by place*}"
read -r first last share <<< "$(printf '%s\n' "$pool" | sed -nE 's/^from ([0-9.]+) to ([0-9.]+), largest step [0-9.]+, ([0-9.]+) of the change;.*/\1 \2 \3/p')"
[ -n "$share" ] || fail "voxel.line read no line across the lantern's pool, saying $pool"
awk -v a="$first" -v b="$last" -v s="$share" 'BEGIN { exit !(a - b >= 40 && s <= 0.1) }' || fail "the lantern's light steps across the floor or does not fall, $pool"
cmd voxel.hud on

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
cmd frames.wait 60
ask voxel.line 1 1 1 1 1 1 2 > /dev/null
cmd frames.wait 2
front=$(ask voxel.line 6.5 4.71875 9.635 6.5 4.40625 9.635 61)
echo "creeper's front ${front%%; by place*}"
largest=$(printf '%s\n' "$front" | sed -nE 's/^from [0-9.]+ to [0-9.]+, largest step ([0-9.]+),.*/\1/p')
[ -n "$largest" ] || fail "voxel.line read no line down the creeper's front, saying $front"
awk -v l="$largest" 'BEGIN { exit !(l <= 2) }' || fail "the creeper's front shows bands at noon, $front"

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
cmd frames.wait 60
ask voxel.line 1 1 1 1 1 1 2 > /dev/null
cmd frames.wait 2
face=$(ask voxel.line 6.71875 5.96875 9.765 6.71875 5.53125 9.765 33)
echo "zombie's face ${face%%; by place*}"
largest=$(printf '%s\n' "$face" | sed -nE 's/^from [0-9.]+ to [0-9.]+, largest step ([0-9.]+),.*/\1/p')
[ -n "$largest" ] || fail "voxel.line read no line down the zombie's face, saying $face"
awk -v l="$largest" 'BEGIN { exit !(l <= 2) }' || fail "the zombie's face steps in light at the eye's height, $face"

# A sapling on a pillar above the eye's height is lit as one on the ground below it, read at the
# middle of each one's crown, where its crossed walls are covered. Its green is at least four fifths
# of the lower's, where a plant's wall seen from below was lit as one facing down, at half of it.
cmd voxel.kill all
cmd voxel.fill 8 4 0 10 6 1 stone
cmd voxel.set 9 7 0 oak_sapling
cmd voxel.set 9 4 3 oak_sapling
cmd voxel.tp 9.5 4 9.5
cmd voxel.look 0 0
cmd frames.wait 30
ask voxel.pixel 1 1 1 > /dev/null
cmd frames.wait 2
high=$(ask voxel.pixel 9.5 7.53 0.5)
low=$(ask voxel.pixel 9.5 4.53 3.5)
echo "the high sapling's crown $high, the low one's $low"
hg=$(printf '%s\n' "$high" | sed -nE 's/^red [0-9]+, green ([0-9]+), .*/\1/p')
lg=$(printf '%s\n' "$low" | sed -nE 's/^red [0-9]+, green ([0-9]+), .*/\1/p')
[ -n "$hg" ] && [ -n "$lg" ] || fail "voxel.pixel read no sapling, saying $high and $low"
[ $((5 * hg)) -ge $((4 * lg)) ] || fail "a plant above the eye's height is lit darker than one below it, $high against $low"

# A zombie wears a pack's picture of it laid as the source lays it, glass is cut out where its
# picture is clear, and a lit furnace shows its top's picture above and glows at its front. The
# pack, a folder of pictures written here as PNGs with perl's own zlib, holds the zombie's, half as
# tall as its layout as an older pack's is, red where the source lays the head's front, clear where
# it lays the hat's layer, so the hat is left out, and blue elsewhere; glass's, a gray frame about a
# clear middle; the lit furnace's front in red and the furnace's top in blue; and water's, two
# frames played half a second each, white and black.
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
printf '{"animation":{"frametime":10}}' > "$textures/block/water_still.png.mcmeta"
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
cmd frames.wait 30
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
# A pane of glass before a wall of red concrete shows the wall through its clear middle and its gray
# frame around it, and a lit furnace beside it its blue top above its red front.
# The eye stands level with the glass, straight before it, so the line through its middle meets the
# wall and no other face of it.
cmd voxel.fill 19 5 0 22 8 0 red_concrete
cmd voxel.set 20 6 2 glass
cmd voxel.set 21 4 2 lit_furnace
cmd voxel.tp 20.5 4.88 5.5
cmd voxel.look 0 -15
cmd frames.wait 30
ask voxel.pixel 1 1 1 > /dev/null
cmd frames.wait 2
middle=$(ask voxel.pixel 20.5 6.5 3.001)
frame=$(ask voxel.pixel 20.9 6.5 3.001)
side=$(ask voxel.pixel 21.5 4.5 3.001)
top=$(ask voxel.pixel 21.5 5.001 2.5)
echo "glass's middle $middle, its frame $frame, the lit furnace's front $side, its top $top"
channels() { printf '%s\n' "$1" | sed -nE 's/^red ([0-9]+), green ([0-9]+), blue ([0-9]+) .*/\1 \2 \3/p'; }
read -r mr mg mb <<< "$(channels "$middle")"
read -r gr gg gb <<< "$(channels "$frame")"
read -r sr sg sb <<< "$(channels "$side")"
read -r tr tg tb <<< "$(channels "$top")"
[ -n "$mb" ] && [ -n "$gb" ] && [ -n "$sb" ] && [ -n "$tb" ] || fail "voxel.pixel read no glass or furnace, saying $middle, $frame, $side and $top"
[ "$mr" -gt $((2 * mg)) ] && [ "$mr" -gt $((2 * mb)) ] || fail "the glass's clear middle does not show the red wall behind it, $middle"
[ $((gr > gb ? gr - gb : gb - gr)) -le 40 ] && [ "$gr" -gt 60 ] || fail "the glass's frame does not show its gray, $frame"
[ "$sr" -gt $((2 * sb)) ] || fail "the lit furnace's front does not wear the picture's red, $side"
[ "$tb" -gt $((2 * tr)) ] || fail "the lit furnace's top does not wear the furnace's top's blue, $top"
# A pool of water a block across shows the written water's frames in turn, read eight times three
# tenths of a second apart, at least once white under the water's tint and once black.
cmd voxel.set 26 3 2 water
cmd voxel.tp 26.5 5 4.5
cmd voxel.look 0 -50
cmd frames.wait 10
greens=""
for i in $(seq 1 8); do
  sleep 0.3
  cmd frames.wait 1
  pool=$(ask voxel.pixel 26.5 3.876 2.5)
  read -r wr wg wb <<< "$(channels "$pool")"
  [ -n "$wb" ] || fail "voxel.pixel read no water, saying $pool"
  greens="$greens $wg"
done
echo "the water's green over the frames:$greens"
printf '%s\n' $greens | sort -n | awk 'NR == 1 { low = $1 } { high = $1 } END { exit !(high - low >= 40) }' \
  || fail "the water's texture does not step through its frames, its green reading$greens"
cmd voxel.packs none

# Seed 1's overworld, the player standing over each of the eight biomes the climate gained, the
# nearest of each that voxel.biome finds, its name read back from voxel.state and its land captured
# from above, at a render distance of two columns, so lavapipe generates few at each.
cmd voxel.hud on
cmd voxel.world overworld 1
cmd voxel.distance 2
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
# A meadow of the plains up close, its grass and flowers cut out of their crossed walls.
found=$(ask voxel.biome plains)
read -r bx by bz <<< "$(printf '%s\n' "$found" | sed -nE 's/.* is at (-?[0-9]+), (-?[0-9]+), (-?[0-9]+),.*/\1 \2 \3/p')"
[ -n "$bz" ] || fail "voxel.biome found no plains, saying $found"
cmd voxel.tp "$bx" "$((by + 3))" "$((bz + 6))"
cmd voxel.look 0 -20
cmd frames.wait 90
capture biomes/meadow.png
# The meadow again under Bare Bones where the machine has fetched it, the game's own looks of the
# grass's top, its blossoms, clover and pebbles, laid over the pack's grass, a machine without it, as
# the build workflow's, passing it by. A transient run's list of packs is its own and saved nowhere.
fetched="${XDG_DATA_HOME:-$HOME/.local/share}/VoxelSandbox/resourcepacks"
if [ -f "$fetched/Bare Bones 1.21.zip" ]; then
  cmd voxel.packs "$fetched/Bare Bones 1.21.zip"
  cmd frames.wait 90
  capture biomes/meadow-pack.png
  cmd voxel.packs none
fi

# From high over the land, the drawn land's edge meets the dome under the horizon in the haze's color
# with no step, read up the window's middle column: the land's last pixel and the one above it alike,
# and no two neighbors within 16 of the edge more than a few levels apart. It is read at four
# columns, the land within them generated and meshed first, so the edge lies past the haze's start.
cmd voxel.distance 4
cmd voxel.tp "$bx" 120 "$bz"
for i in $(seq 1 600); do
  ask voxel.state | grep -q " 0 generating, 0 sections to mesh" && break
  cmd frames.wait 10
done
cmd voxel.look 0 -20
cmd frames.wait 60
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

echo "Voxel Sandbox walked $distance blocks toward a glowstone in a closed room, saw a lantern's light fall smoothly across the floor, climbed out of a pool onto its bank, saw a sheep's wool in shade gray, saw a creeper's front at noon with no bands, saw a zombie's face as bright above the eye as below it, saw a sapling above the eye lit as one below, saw a zombie wear a pack's picture, saw a red wall through a pane of glass's clear middle and a lit furnace's top above its front, saw water's frames in turn, drew with no validation error and wrote $name on F2"
