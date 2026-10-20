#!/usr/bin/env bash
# _tools/bench/run.sh - generate fixed corpora and time serialization and
# deserialization separately for every parser; case3 and case4 deserialize
# into typed objects in the .NET runners, case5 is written in flow style,
# case6 is a typed numbers-only corpus.
# Input files are loaded before timing; ser mode also parses once before
# timing. The .NET runners also report allocations per document and, in a
# fresh process per sample, the cold first parse and serialization. Results
# are JSON files, summarized by report.py.
#
# Arguments: [corpus dir] [results dir] [runs] [parsers] [seconds]. Parsers
# default to nano, sharpyaml and libyaml; yamldotnet and strictyaml are
# available too. libyaml is the C reference: built from a pinned, checksum
# verified release by build-libyaml.sh, and it loads every corpus into its
# own node tree, since C has no typed model.
# Pure ASCII.
set -euo pipefail

SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
ROOT="$(readlink -f "$SCRIPT_DIR/../..")"
CORPUS="${1:-$ROOT/artifacts/bench-corpus}"
RESULTS="${2:-$ROOT/artifacts/bench-results}"
RUNS="${3:-3}"
PARSERS="${4:-nano sharpyaml libyaml}"
SECONDS_PER_RUN="${5:-10}"

rm -rf "$CORPUS" "$RESULTS"
mkdir -p "$RESULTS"

echo "==> Corpora..."
dotnet build "$SCRIPT_DIR" -c Release --nologo -v quiet
BENCH="$SCRIPT_DIR/bin/Release/net10.0/bench.dll"
dotnet "$BENCH" generate "$CORPUS"

LIBYAML=""
if [[ " $PARSERS " == *" libyaml "* ]]; then
    echo "==> libyaml..."
    LIBYAML="$(bash "$SCRIPT_DIR/build-libyaml.sh" | tail -n 1)"
fi

for run in $(seq 1 "$RUNS"); do
    echo "==> Run $run of $RUNS"
    for parser in $PARSERS; do
        for corpus in case1 case2 case3 case4 case5 case6; do
            for mode in ser deser; do
                result="$RESULTS/$parser-$corpus-$mode-$run.json"
                case "$parser" in
                    strictyaml)
                        # No typed model; this runner covers case1 and case2 itself.
                        [ "$corpus" = "case1" ] || continue
                        python3 "$SCRIPT_DIR/strictyaml_bench.py" "$CORPUS" throughput \
                            "$SECONDS_PER_RUN" "$mode" | tee "$result"
                        ;;
                    libyaml)
                        "$LIBYAML" throughput "$CORPUS" "$corpus" "$SECONDS_PER_RUN" "$mode" \
                            | tee "$result"
                        ;;
                    *)
                        dotnet "$BENCH" throughput "$parser" "$CORPUS" "$corpus" "$SECONDS_PER_RUN" "$mode" \
                            | tee "$result"
                        ;;
                esac
            done
        done
    done
done

# Cold start: a fresh process per sample, so nothing is warm. Only the .NET
# runners have a JIT and a type cache to warm up.
for parser in $PARSERS; do
    case "$parser" in
        strictyaml | libyaml) continue ;;
    esac
    for corpus in case1 case3; do
        for sample in $(seq 1 5); do
            dotnet "$BENCH" cold "$parser" "$CORPUS" "$corpus" \
                | tee "$RESULTS/$parser-$corpus-cold-$sample.json"
        done
    done
done

python3 "$SCRIPT_DIR/report.py" "$RESULTS"
