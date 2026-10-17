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
# into a folder of its own and its program run offscreen for 60 frames, where no slangc is found, so
# a shader the cache lacks fails here before a player finds it.
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
dotnet publish VoxelSandbox -c Release -r "$rid" -p:PublishAot=true -p:Version="$version" -o "$out" "$@"

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
# outside the checkout, where the engine finds no build/tools/slang walking up from it.
if [ "$(uname -s)" = Linux ]; then
  played=$(mktemp -d)
  trap 'rm -rf "$played"' EXIT
  unzip -q "build/publish/$name.zip" -d "$played"
  log="build/publish/$name-run.log"
  if ! (cd "$played/$name" && env -u ENGINE_SLANGC ./VoxelSandbox --offscreen --frames 60 --transient) > "$log" 2>&1; then
    tail -n 40 "$log"
    fail "the zip's program did not run to frame 60, its log ends with $(tail -n 3 "$log" | tr -s '\r\n' '  ' | cut -c1-600)"
  fi
  grep -q "slangc was not found" "$log" || fail "the zip's program found a slangc, so its run did not prove the cache"
  if grep -E "is not in the shader cache|did not compile" "$log"; then fail "a shader the zip's program loaded was not in its cache"; fi
  echo "the zip's program drew 60 frames offscreen with no slangc"
fi
