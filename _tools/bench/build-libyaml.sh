#!/usr/bin/env bash
# _tools/bench/build-libyaml.sh - build the libyaml (C) benchmark runner.
# Downloads the pinned libyaml release once, verifies its SHA-256, and
# compiles it together with libyaml_bench.c at -O2 into one static binary,
# so the benchmark never depends on whatever libyaml the system carries.
# Prints the binary path. Needs curl, tar and a C compiler (cc). Pure ASCII.
set -euo pipefail

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT="$(readlink -f "$SCRIPT_DIR/../..")"
BUILD="${1:-$ROOT/artifacts/libyaml}"

VERSION=0.2.5
SHA256=c642ae9b75fee120b2d96c712538bd2cf283228d2337df2cf2988e3c02678ef4
URL="https://github.com/yaml/libyaml/releases/download/$VERSION/yaml-$VERSION.tar.gz"
ARCHIVE="$BUILD/yaml-$VERSION.tar.gz"
SOURCE="$BUILD/yaml-$VERSION"
BINARY="$BUILD/libyaml_bench"

mkdir -p "$BUILD"
if [ ! -f "$ARCHIVE" ]; then
    curl -fsSL -o "$ARCHIVE.part" "$URL"
    mv "$ARCHIVE.part" "$ARCHIVE"
fi
echo "$SHA256  $ARCHIVE" | sha256sum -c --quiet -
if [ ! -d "$SOURCE" ]; then
    tar -xzf "$ARCHIVE" -C "$BUILD"
fi

if [ ! -x "$BINARY" ] || [ "$SCRIPT_DIR/libyaml_bench.c" -nt "$BINARY" ]; then
    cc -O2 -std=gnu99 -w \
        -DYAML_VERSION_MAJOR=0 -DYAML_VERSION_MINOR=2 -DYAML_VERSION_PATCH=5 \
        -DYAML_VERSION_STRING="\"$VERSION\"" -DYAML_DECLARE_STATIC \
        -I"$SOURCE/include" \
        "$SCRIPT_DIR/libyaml_bench.c" "$SOURCE"/src/*.c \
        -o "$BINARY"
fi

echo "$BINARY"
