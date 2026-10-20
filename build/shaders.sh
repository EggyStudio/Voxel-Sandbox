#!/usr/bin/env bash
# Compiles every build of every shader the game draws with into build/shader-cache, which
# build/publish.sh puts in each native build, since a player's machine has no slangc. The shaders
# are the engine's, as the package stages them beside the Debug build, and the game's own, and
# `./e3d shaders` compiles each one's plain build and its build for each capability or frame the
# engine loads another for, such as a device with ray queries or a frame without global
# illumination.
#
#   build/shaders.sh
#
# It needs the Debug build that `dotnet build VoxelSandbox.slnx` makes, slangc
# (build/fetch-slang.sh) and the engine checkout ./e3d runs out of. The game's terrain.slang and
# mob.slang import the engine's model pass, so they are compiled from one folder with the engine's
# shaders. An entry is keyed by a shader's source, its defines and the files it imports, and not by
# the folder, so the program finds it when it loads resources/shaders/terrain.slang. A run of the
# game fills only the builds its device and its frames load, as 120 frames on lavapipe compiled one
# of the terrain's five.
set -euo pipefail
cd "$(dirname "$0")/.."

staged=VoxelSandbox/bin/Debug/net10.0/source/shaders
if [ ! -d "$staged" ]; then
  echo "$staged is not there, which dotnet build VoxelSandbox.slnx stages from the package" >&2
  exit 1
fi

folder=$(mktemp -d)
trap 'rm -rf "$folder"' EXIT
cp -R "$staged/." "$folder/"
cp VoxelSandbox/resources/shaders/*.slang "$folder/"
rm -rf build/shader-cache
./e3d shaders "$folder" build/shader-cache

for shader in terrain mob; do
  if ! ls build/shader-cache/$shader.*.spv > /dev/null 2>&1; then
    echo "build/shader-cache holds no build of $shader.slang" >&2
    exit 1
  fi
done
echo "build/shader-cache holds $(ls build/shader-cache/*.spv | wc -l | tr -d ' ') compiled stages, $(ls build/shader-cache/terrain.*.spv | wc -l | tr -d ' ') of them the terrain's and $(ls build/shader-cache/mob.*.spv | wc -l | tr -d ' ') the mobs'"
