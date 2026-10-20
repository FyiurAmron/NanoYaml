#!/usr/bin/env bash
# _tools/gate.sh - the full local verification gate: style, then tests.
# Fails with the first failing step. Pure ASCII.
set -euo pipefail
ROOT="$(readlink -f "$(dirname "$0")/..")"
bash "$ROOT/_tools/check-style.sh"
bash "$ROOT/_tools/unit-test.sh"
