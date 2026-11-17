#!/usr/bin/env bash
#
# Downloads the Slang compiler this checkout builds its shaders with, into build/tools/slang.
#
# The game's terrain shader is Slang, compiled to SPIR-V by slangc when the game loads it, so a
# checkout needs slangc before the game's Debug build can draw its world. The engine walks up for
# build/tools/slang/bin/slangc only in a checkout of the engine, so a run of the game names this
# one in ENGINE_SLANGC, as the last line says, which build/play.sh and build/shaders.sh do for
# themselves.
#
# A shipped build does not need it. It carries every shader compiled (build/shaders.sh) in
# source/.slang-cache beside the program, which the engine reads where there is no compiler.
#
# The version is pinned so every checkout compiles the same SPIR-V. Pass --force to download it
# again over what is there.
#
# Usage:
#   build/fetch-slang.sh
#   build/fetch-slang.sh --force
#
set -euo pipefail

VERSION="2026.18.2"

BUILD_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TOOLS_DIR="$BUILD_DIR/tools"
SLANG_DIR="$TOOLS_DIR/slang"
STAMP="$SLANG_DIR/.version"

if [[ "${1:-}" != "--force" && -f "$STAMP" && "$(cat "$STAMP")" == "$VERSION" ]]; then
    echo "==> slangc $VERSION is already at $SLANG_DIR"
    exit 0
fi

case "$(uname -s)" in
    Linux)  os="linux" ;;
    Darwin) os="macos" ;;
    MINGW*|MSYS*|CYGWIN*) os="windows" ;;
    *) echo "No Slang release for $(uname -s). Install slangc and set ENGINE_SLANGC." >&2; exit 1 ;;
esac

case "$(uname -m)" in
    x86_64|amd64) arch="x86_64" ;;
    arm64|aarch64) arch="aarch64" ;;
    *) echo "No Slang release for $(uname -m). Install slangc and set ENGINE_SLANGC." >&2; exit 1 ;;
esac

# The glibc 2.28 build on Linux, which is the oldest floor Slang publishes for both architectures.
if [[ "$os" == "linux" ]]; then
    archive="slang-$VERSION-linux-$arch-glibc-2.28.tar.gz"
elif [[ "$os" == "windows" ]]; then
    archive="slang-$VERSION-windows-$arch.zip"
else
    archive="slang-$VERSION-macos-$arch.tar.gz"
fi

url="https://github.com/shader-slang/slang/releases/download/v$VERSION/$archive"
download="$TOOLS_DIR/$archive"

# Each archive's sha256 as the release lists it, so one of other bytes is refused. A new version
# fails here until its sums are written.
case "$archive" in
    slang-2026.18.2-linux-x86_64-glibc-2.28.tar.gz) sum=991f521829100d06846a2697425d588f604f3bf3a23a6bfc499136585f7e6a6f ;;
    slang-2026.18.2-linux-aarch64-glibc-2.28.tar.gz) sum=56fe614bc019c9b53cd1de01537c4ac8d95120e46f4bb6b9cce56e916883f55c ;;
    slang-2026.18.2-windows-x86_64.zip) sum=747602aec6b3623658d55fea87492d71828e15d16802d7941204fde418ceee8e ;;
    slang-2026.18.2-windows-aarch64.zip) sum=89d9c9b9a89d8f819239db0b33adf7d885b6ddba12d3a798f2cf8e6c8855361e ;;
    slang-2026.18.2-macos-x86_64.tar.gz) sum=d09a2993b4ed6d81c0c19ff8bede27944bf73437d982549bd7b2ac5a1a3202b9 ;;
    slang-2026.18.2-macos-aarch64.tar.gz) sum=410c0ed14e231089a58437bb0d8e00389c48e29ed9ffe23e3b04a5c74f1ce809 ;;
    *) echo "No sha256 is written here for $archive. Write the one its release lists." >&2; exit 1 ;;
esac
sha256() { if command -v sha256sum > /dev/null; then sha256sum "$1"; else shasum -a 256 "$1"; fi | cut -d ' ' -f 1; }

mkdir -p "$TOOLS_DIR"
echo "==> downloading $url"
curl -fsSL --retry 3 -o "$download" "$url"
if [ "$(sha256 "$download")" != "$sum" ]; then
    rm -f "$download"
    echo "$url is not the archive its release lists, its sha256 not $sum" >&2
    exit 1
fi

rm -rf "$SLANG_DIR"
mkdir -p "$SLANG_DIR"

if [[ "$archive" == *.zip ]]; then
    unzip -q "$download" -d "$SLANG_DIR"
else
    tar -xzf "$download" -C "$SLANG_DIR"
fi

rm -f "$download"

# The stamp is written once the compiler runs, so one that does not is fetched again next time.
"$SLANG_DIR/bin/slangc" -v >/dev/null
echo "$VERSION" > "$STAMP"
echo "==> slangc $VERSION is at $SLANG_DIR/bin, which a run of the game takes by"
echo "    export ENGINE_SLANGC=$SLANG_DIR/bin/slangc"
