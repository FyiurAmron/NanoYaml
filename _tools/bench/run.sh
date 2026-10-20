#!/usr/bin/env bash
# _tools/bench/run.sh - generate fixed corpora and time serialization and
# deserialization separately for every parser. Input files are loaded before
# timing; ser mode also parses once before timing. Results are JSON files,
# summarized by report.py. The fourth argument selects parsers, defaulting to
# nano and sharpyaml.
# Pure ASCII.
set -euo pipefail

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT="$(readlink -f "$SCRIPT_DIR/../..")"
CORPUS="${1:-$ROOT/artifacts/bench-corpus}"
RESULTS="${2:-$ROOT/artifacts/bench-results}"
RUNS="${3:-3}"
#PARSERS="${4:-nano yamldotnet sharpyaml strictyaml}"
PARSERS="${4:-nano sharpyaml}"

rm -rf "$CORPUS" "$RESULTS"
mkdir -p "$RESULTS"

echo "==> Corpora..."
dotnet run --project "$SCRIPT_DIR" -c Release -- generate "$CORPUS"

for run in $(seq 1 "$RUNS"); do
    echo "==> Run $run of $RUNS"
    for parser in $PARSERS; do
        for corpus in case1 case2; do
            for mode in ser deser; do
                result="$RESULTS/$parser-$corpus-$mode-$run.json"
                if [ "$parser" = "strictyaml" ]; then
                    python3 "$SCRIPT_DIR/strictyaml_bench.py" "$CORPUS" throughput 10 "$mode" \
                        | tee "$result"
                else
                    dotnet run --project "$SCRIPT_DIR" -c Release --no-build -- \
                        throughput "$parser" "$CORPUS" "$corpus" 10 "$mode" \
                        | tee "$result"
                fi
            done
        done
    done
done

python3 "$SCRIPT_DIR/report.py" "$RESULTS"
