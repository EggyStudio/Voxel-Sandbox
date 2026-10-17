#!/usr/bin/env bash
# Publishes the game as native code for the system it runs on and zips it as a player gets it, into
# build/publish as VoxelSandbox-<version>-<rid>.zip: the program, its resources, the engine's shaders
# with the cache of every shader compiled, the README and the license.
#
#   build/publish.sh <version> <rid> [options for dotnet publish]
#
# Native code is compiled for the system it is made on, so the pack workflow publishes each rid on a
# runner of its own. The options are passed on, as -p:CppCompilerAndLinker=gcc on a machine with no
# clang. A player's machine has no slangc, so the program reads every shader from
# source/.slang-cache, which takes build/shader-cache, filled by build/shaders.sh, as the pack
# workflow's shaders job fills it and each publish job downloads it. On Linux the zip is unpacked
# into a folder of its own and its program run offscreen where no slangc is found, saving a world
# and opening it again, so a shader the cache lacks or a save native code cannot write fails here
# before a player finds it.
set -euo pipefail

version="$1"
rid="$2"
shift 2
cd "$(dirname "$0")/.."

name="VoxelSandbox-$version-$rid"
out="build/publish/$name"
fail() { echo "::error title=$name::$1"; echo "publish.sh: $1" >&2; exit 1; }

if ! ls build/shader-cache/terrain.*.spv > /dev/null 2>&1; then
  fail "build/shader-cache holds no build of terrain.slang, which build/shaders.sh compiles"
fi

rm -rf "$out" "$out.zip"
published="build/publish/$name-publish.log"
mkdir -p build/publish
dotnet publish VoxelSandbox -c Release -r "$rid" -p:PublishAot=true -p:Version="$version" -o "$out" "$@" 2>&1 | tee "$published"

# A trim or AOT warning at a line of the game's own code names a call native code may not keep,
# such as serializing by reflection, so it fails the publish. The engine's dependencies warn of
# their own assemblies, which the game cannot mend, and are left to the engine.
if grep -E 'VoxelSandbox[/\\][^ ]*\.cs\([0-9,]+\): (Trim analysis |AOT analysis )?warning IL[0-9]+' "$published"; then
  fail "the publish warned of code native code may not keep, at the lines above"
fi

# The program the publish wrote, native code with no assembly beside it that would run in its place.
if [ -f "$out/VoxelSandbox.dll" ]; then
  fail "the publish left VoxelSandbox.dll beside the program, so it is not native code"
fi

# Beside the engine's own entries, which the package carries, every build of the game's shaders.
mkdir -p "$out/source/.slang-cache"
cp -R build/shader-cache/. "$out/source/.slang-cache/"

# What a player has no use for: the symbols the native compiler writes beside the program, a folder
# of them on macOS, and the documentation of the game and the engine.
rm -rf "$out"/*.pdb "$out"/*.dbg "$out"/*.xml "$out"/*.dSYM
cp README.md LICENSE "$out/"

# Zipped by Python's own module, which each system the script runs on has, where zip is missing on
# Windows. There it is python, since python3 may be the store's stand-in for one.
case "$(uname -s)" in
  MINGW* | MSYS* | CYGWIN*) python=python ;;
  *) python=python3 ;;
esac
(cd build/publish && "$python" -m zipfile -c "$name.zip" "$name")
echo "build/publish/$name.zip"

# On Linux, the zip as a player unpacks it, with unzip, which keeps the program's mode, in a folder
# outside the checkout, where the engine finds no build/tools/slang walking up from it. Its program
# runs twice with its data folder, where saves go, in that folder as well. The first run makes a
# flat world and saves it on quitting, and the second asks for the world by its name alone, which
# opens flat only where the first run's save was read back, and saves it again. Native code turns
# serializing by reflection off, so a save serialized that way ends the first run.
if [ "$(uname -s)" = Linux ]; then
  played=$(mktemp -d)
  trap 'rm -rf "$played"' EXIT
  unzip -q "build/publish/$name.zip" -d "$played"
  run() {
    local log="build/publish/$name-run-$1.log"
    shift
    if ! (cd "$played/$name" && env -u ENGINE_SLANGC XDG_DATA_HOME="$played/data" ./VoxelSandbox --offscreen "$@") > "$log" 2>&1; then
      tail -n 40 "$log"
      fail "the zip's program run with $* did not run to its end, its log ends with $(tail -n 3 "$log" | tr -s '\r\n' '  ' | cut -c1-600)"
    fi
    grep -q "slangc was not found" "$log" || fail "the zip's program found a slangc, so its run did not prove the cache"
    if grep -E "is not in the shader cache|did not compile" "$log"; then fail "a shader the zip's program loaded was not in its cache"; fi
  }
  saved="$played/data/VoxelSandbox/saves/check"
  run 1 --frames 60 --flat --world check
  [ -f "$saved/world.json" ] && [ -f "$saved/furnaces.json" ] || fail "the zip's program saved no world.json and furnaces.json in its world's folder"
  run 2 --frames 10 --world check
  grep -q '"Kind": "flat"' "$saved/world.json" || fail "the zip's program did not open the world it saved as the flat world it was"
  echo "the zip's program drew offscreen with no slangc, saved a world and opened it again"
fi
