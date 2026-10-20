#!/usr/bin/env bash
# _tools/check-style.sh - verify style with dedicated tools.
# CS goes through dotnet format (.editorconfig rules) and through
# _tools/style-lint (the Roslyn rules: comments, statements, collections,
# access, groups, counts, error periods, test vars, lists, and more); text
# hygiene through _tools/check-text-hygiene.sh, the same code the pre-commit
# hook runs. Exits 1 and lists offenders when any check fails. Pure ASCII.
set -euo pipefail

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT="$(readlink -f "$SCRIPT_DIR/..")"
fail=0

# shellcheck source=_tools/check-text-hygiene.sh
. "$SCRIPT_DIR/check-text-hygiene.sh"

command -v dotnet >/dev/null || { echo "need dotnet on PATH"; exit 2; }

# Every project, found instead of named: no project name in the tooling.
echo "==> CS: dotnet format --verify-no-changes..."
while IFS= read -r proj; do
    if ! dotnet format "$proj" --verify-no-changes; then
        fail=1
    fi
done < <(find "$ROOT" -name '*.csproj' -not -path '*/.godot/*' | sort)

# The style lint is a tiny project next to this script, kept out of the
# library assembly by NanoYaml.csproj; it parses every source in the
# repository with Roslyn and fails on a style violation.
echo "==> CS lint: comments, statements, collections, access, lists..."
if ! (cd "$ROOT" && dotnet run --project "$SCRIPT_DIR/style-lint" -c Release -- .); then
    fail=1
fi

echo "==> Text: newline at EOF, no trailing whitespace, docs within 80/120..."
if ! (cd "$ROOT" && check_text_files $(git ls-files)); then
    fail=1
fi

exit $fail
