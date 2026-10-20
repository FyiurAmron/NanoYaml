import glob
import json
import os
import sys


def collect(results):
    runs = {}
    for path in sorted(glob.glob(os.path.join(results, "*.json"))):
        with open(path, encoding="utf-8") as handle:
            for line in handle:
                line = line.strip()
                if not line:
                    continue
                row = json.loads(line)
                key = (row["parser"], row["corpus"], row.get("mode", "both"))
                runs.setdefault(key, []).append(row)
    return runs


def allocated(row):
    value = row.get("allocated_bytes_per_document")
    return "-" if value is None else f"{value / 1024:.1f}"


def main(results):
    runs = collect(results)
    timed = {key: rows for key, rows in runs.items() if key[2] != "cold"}
    cold = {key: rows for key, rows in runs.items() if key[2] == "cold"}
    print("| corpus | parser | mode | seconds | documents/s | MB/s | KB allocated/doc |")
    print("|---|---|---|---:|---:|---:|---:|")
    for (parser, corpus, mode), rows in sorted(
        timed.items(), key=lambda item: (item[0][1], item[0][0], item[0][2])
    ):
        row = max(rows, key=lambda item: item["documents_per_second"])
        print(f"| {corpus} | {parser} | {mode} | {row['seconds']:.1f} | "
              f"{row['documents_per_second']:.1f} | {row['mb_per_second']:.2f} | "
              f"{allocated(row)} |")
    if not cold:
        return
    print()
    print("| corpus | parser | first parse ms | first serialize ms |")
    print("|---|---|---:|---:|")
    for (parser, corpus, _), rows in sorted(cold.items(), key=lambda item: (item[0][1], item[0][0])):
        parse = sorted(row["first_parse_ms"] for row in rows)[len(rows) // 2]
        write = sorted(row["first_serialize_ms"] for row in rows)[len(rows) // 2]
        print(f"| {corpus} | {parser} | {parse:.1f} | {write:.1f} |")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("usage: report.py <results>", file=sys.stderr)
        sys.exit(2)
    main(sys.argv[1])
