#!/usr/bin/env bash
# Checks the engine out at the commit the build workflow names into .engine beside the game, as the
# runner does, and builds its e3d command line there, so that with ENGINE_CHECKOUT naming .engine the
# game's ./e3d and build/play.sh drive through a client of the pinned engine that no build of a live
# engine tree touches. The engine's ./e3d builds its client only where the program is missing, so a
# client built here is never built again under a run. Run it again after each pin, which moves the
# checkout to the pin's commit and builds the client again.
#   build/engine.sh
set -euo pipefail
cd "$(dirname "$0")/.."

# The commit the workflow's step taking EggyStudio/3DEngine checks out.
ref=$(sed -n '/repository: EggyStudio\/3DEngine/,/ref:/s/^ *ref: *//p' .github/workflows/build.yml)
[ -n "$ref" ] || { echo "build.yml names no commit of the engine's" >&2; exit 1; }

[ -d .engine/.git ] || git init --quiet .engine
git -C .engine remote get-url origin > /dev/null 2>&1 || git -C .engine remote add origin https://github.com/EggyStudio/3DEngine.git
if [ "$(git -C .engine rev-parse --verify --quiet HEAD || true)" != "$ref" ]; then
  git -C .engine fetch --quiet --depth 1 origin "$ref"
  git -C .engine checkout --quiet --force --detach FETCH_HEAD
  git -C .engine submodule update --quiet --init --recursive --depth 1
fi

# Built whenever the checkout moved or the program is missing, so a client of the last pin is not kept.
client=.engine/3DEngine.Cli/bin/Debug/net10.0/e3d
built=.engine/3DEngine.Cli/bin/Debug/net10.0/e3d.commit
if [ "$(cat "$built" 2> /dev/null || true)" != "$ref" ] || { [ ! -x "$client" ] && [ ! -f "$client.exe" ]; }; then
  dotnet build .engine/3DEngine.Cli/3DEngine.Cli.csproj -v q --nologo -p:UseSharedCompilation=false > /dev/null
  printf '%s\n' "$ref" > "$built"
fi
echo "e3d of the engine's $ref, with ENGINE_CHECKOUT=$PWD/.engine"
