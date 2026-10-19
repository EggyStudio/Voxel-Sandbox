#!/usr/bin/env bash
# Takes the README's pictures through ./e3d, each from the seed, the place, the hour and the heading
# written below, so a change to how the world is drawn is shown again by running it. The game opens
# offscreen on the overworld of seed 1, saving nothing, at its own render distance and light, with
# every mob held where the land placed it and the clouds held where each picture places them, and
# each picture is captured at 1280 by 720 with the HUD hidden, once every column in reach is meshed,
# and encoded at 800 by 450 into .github/assets by the engine checkout's build/webp.sh.
#
#   build/pictures.sh
#
# The game is the Debug build that `dotnet build VoxelSandbox.slnx` makes. ./e3d and build/webp.sh
# run out of the engine checkout ENGINE_CHECKOUT names, or the folder above this one.
set -euo pipefail
cd "$(dirname "$0")/.."
engine="${ENGINE_CHECKOUT:-..}"
program=VoxelSandbox/bin/Debug/net10.0/VoxelSandbox
mkdir -p captures
opened=captures/pictures.json

[ -f "$program" ] || [ -f "$program.exe" ] || { echo "$program is not built, which dotnet build VoxelSandbox.slnx does" >&2; exit 1; }
./e3d open "$program" --offscreen --transient --json > "$opened"
pid=$(perl -MJSON::PP -0777 -ne 'my $answer = eval { decode_json($_) } or exit; print $answer->{data}{pid} // ""' "$opened")
[ -n "$pid" ] || { echo "e3d did not open the game, it said $(cat "$opened")" >&2; exit 1; }
trap './e3d stop --session "$pid" --quiet > /dev/null 2>&1 || true' EXIT
cmd() { ./e3d command "$@" --quiet --timeout 600 --session "$pid"; }

cmd voxel.world overworld 1
cmd voxel.still on
cmd voxel.hud off
cmd voxel.fly on

# Puts the player at a place and waits until every column in reach of it is generated and meshed.
go() {
  cmd voxel.tp "$1" "$2" "$3"
  for i in $(seq 1 600); do
    ./e3d command voxel.state --timeout 600 --session "$pid" | grep -q " 0 generating, 0 sections to mesh" && break
    cmd frames.wait 10
  done
}

# A picture named for its file, taken from a place at an hour, facing a heading in degrees clockwise
# from north and pitched in degrees up, with the clouds drifted so many blocks east.
take() {
  local name="$1" x="$2" y="$3" z="$4" hour="$5" heading="$6" pitch="$7" drift="$8"
  go "$x" "$y" "$z"
  cmd voxel.time "$hour"
  cmd voxel.look "$heading" "$pitch"
  cmd voxel.clouds on "$drift"
  # Global illumination settles over the new place.
  cmd frames.wait 240
  ./e3d shot "$PWD/captures/$name.png" --timeout 600 --session "$pid" > /dev/null
  "$engine/build/webp.sh" "captures/$name.png" ".github/assets/$name.webp" lossy 800x450
  echo "took .github/assets/$name.webp"
}

# A mob of a kind and variant set down on the ground below a place, facing a heading, by the
# crosshair from three blocks above it, where it stands still.
place() {
  local kind="$1" variant="$2" x="$3" z="$4" heading="$5"
  cmd voxel.tp "$x" 64 "$z"
  cmd voxel.look "$heading" -90
  cmd frames.wait 2
  cmd voxel.spawn "$kind" "$variant"
}

# The houses of the village south of the spawn from its meadow in the afternoon, with two sheep, a
# cow and a pig set down in the meadow and a villager on its path, as the land's herds graze about
# its villages, so each run shows them where the last did.
go 200.5 67 60.5
for mob in "sheep 0 201.5 50.5 200" "sheep 0 210.5 54.5 150" "cow 0 207.5 49.5 250" "pig 0 204.5 53.5 120" \
  "villager 0 209.5 49.5 30"; do
  place $mob
done
take village 200.5 67 60.5 16 30 -12 0
# The wooded hills by the sea from above in the morning, the sun and its rings of glow over the
# clouds, which have drifted past it.
take hills -20.5 96 10.5 7.6 88 -6 80
