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


def main(results):
    runs = collect(results)
    print("| corpus | parser | mode | seconds | documents/s | MB/s |")
    print("|---|---|---|---:|---:|---:|")
    for (parser, corpus, mode), rows in sorted(
        runs.items(), key=lambda item: (item[0][1], item[0][0], item[0][2])
    ):
        row = max(rows, key=lambda item: item["documents_per_second"])
        print(f"| {corpus} | {parser} | {mode} | {row['seconds']:.1f} | "
              f"{row['documents_per_second']:.1f} | {row['mb_per_second']:.2f} |")


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("usage: report.py <results>", file=sys.stderr)
        sys.exit(2)
    main(sys.argv[1])
