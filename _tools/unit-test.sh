#!/usr/bin/env bash
# _tools/unit-test.sh - run the library's own test suite.
# Fails with the test failures. Pure ASCII.
set -euo pipefail
export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"
ROOT="$(readlink -f "$(dirname "$0")/..")"
dotnet test "$ROOT/test/NanoYamlTests.csproj" --nologo 2>&1 | tail -n 2
