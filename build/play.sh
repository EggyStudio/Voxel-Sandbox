#!/usr/bin/env bash
# A scripted run of the game through ./e3d, offscreen and under the validation layer, as the build
# workflow's play job runs it on lavapipe. On a flat world with nothing kept on disk it builds a
# closed room of stone with a glowstone block in it, puts the player in the room facing the lamp at
# night, walks toward it and captures the frame. It fails where the game does not open or answer,
# the player did not move, or the layer reported an error, and says why in an error annotation, so
# the run's page names it.
#
#   build/play.sh
#
# The game is the Debug build that `dotnet build VoxelSandbox.slnx` makes. ./e3d runs out of the
# engine checkout ENGINE_CHECKOUT names, or the folder above this one, and keeps the session's log
# in that checkout's build/sessions. The capture and what e3d answered go to captures.
set -euo pipefail
cd "$(dirname "$0")/.."
mkdir -p captures

program=VoxelSandbox/bin/Debug/net10.0/VoxelSandbox
opened=captures/opened.json
said=captures/said.txt
log=""
# Every command and the stop go to this game by its name, so a program an earlier run left serving
# neither makes them ambiguous nor takes the stop meant for this one.
export E3D_NAME=VoxelSandbox
# The script's own output, which an error is written to, so one said inside $(ask ...) reaches the
# page and not the variable.
exec 3>&1

fail() {
  local ending=""
  [ -n "$log" ] && [ -f "$log" ] && ending=$(tail -n 3 "$log" | tr -s '\r\n' '  ' | sed 's/ *$//; s/\.$//' | cut -c1-600)
  echo "::error title=Voxel Sandbox through e3d::$1.${ending:+ Its log ends with $ending.}" >&3
  exit 1
}
trap './e3d stop --quiet > /dev/null 2>&1 || true' EXIT
# A command e3d refuses fails with its code and sentence, which it writes to standard error.
cmd() { ./e3d command "$@" --quiet --timeout 600 2> "$said" || fail "./e3d command $* ended with $?, $(head -n 1 "$said")"; }
ask() {
  local answer
  answer=$(./e3d command "$1" --timeout 600 2> "$said") || fail "./e3d command $1 ended with $?, $(head -n 1 "$said")"
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
shot="$PWD/captures/room.png"
if command -v cygpath > /dev/null; then shot=$(cygpath -m "$shot"); fi
./e3d shot "$shot" --timeout 600 > /dev/null 2> "$said" || fail "./e3d shot ended with $?, $(head -n 1 "$said")"

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

./e3d stop --quiet > /dev/null 2> "$said" || fail "./e3d stop ended with $?, $(head -n 1 "$said")"
[ -f "$log" ] || fail "the log e3d named, $log, is not there"
grep -q "Validation layers: ENABLED" "$log" || fail "the game drew without the validation layer"
if grep -F 'Validation Error' "$log"; then fail "the validation layer reported an error"; fi
echo "Voxel Sandbox walked $distance blocks toward a glowstone in a closed room and drew with no validation error"
