import json
import os
import sys
import time

import strictyaml

ITERATIONS = 5


def load(path):
    with open(path, encoding="utf-8") as handle:
        return handle.read()


def run(root):
    for case in ("case1", "case2"):
        directory = os.path.join(root, case)
        names = sorted(name for name in os.listdir(directory) if name.endswith(".yaml"))
        texts = [load(os.path.join(directory, name)) for name in names]
        for text in texts:
            strictyaml.load(text).as_yaml()
        parse_ms = float("inf")
        serialize_ms = float("inf")
        output = 0
        for _ in range(ITERATIONS):
            start = time.perf_counter()
            documents = [strictyaml.load(text) for text in texts]
            parse_ms = min(parse_ms, (time.perf_counter() - start) * 1000)
            start = time.perf_counter()
            output = sum(len(document.as_yaml()) for document in documents)
            serialize_ms = min(serialize_ms, (time.perf_counter() - start) * 1000)
            del documents
        print(json.dumps({
            "parser": "StrictYAML",
            "corpus": case,
            "files": len(texts),
            "input_bytes": sum(len(text) for text in texts),
            "output_bytes": output,
            "parse_ms": round(parse_ms, 3),
            "serialize_ms": round(serialize_ms, 3),
            "total_ms": round(parse_ms + serialize_ms, 3),
        }))


def throughput(root, seconds, mode):
    for case in ("case1", "case2"):
        directory = os.path.join(root, case)
        names = sorted(name for name in os.listdir(directory) if name.endswith(".yaml"))
        texts = [load(os.path.join(directory, name)) for name in names]
        trees = [strictyaml.load(text) for text in texts] if mode == "ser" else []
        if mode == "ser":
            for tree in trees:
                tree.as_yaml()
        elif texts:
            tree = strictyaml.load(texts[0])
            if mode == "both":
                tree.as_yaml()

        documents = 0
        produced = 0
        processed_bytes = 0
        passes = 0
        start = time.perf_counter()
        while time.perf_counter() - start < seconds:
            if mode == "ser":
                for text, tree in zip(texts, trees):
                    if time.perf_counter() - start >= seconds:
                        break
                    produced += len(tree.as_yaml())
                    documents += 1
                    processed_bytes += len(text)
            elif mode == "deser":
                for text in texts:
                    if time.perf_counter() - start >= seconds:
                        break
                    strictyaml.load(text)
                    documents += 1
                    processed_bytes += len(text)
            else:
                for text in texts:
                    if time.perf_counter() - start >= seconds:
                        break
                    document = strictyaml.load(text)
                    produced += len(document.as_yaml())
                    documents += 1
                    processed_bytes += len(text)

            passes += 1
        elapsed = time.perf_counter() - start
        size = sum(len(text) for text in texts)
        print(json.dumps({
            "parser": "StrictYAML",
            "corpus": case,
            "mode": mode,
            "seconds": round(elapsed, 3),
            "documents": documents,
            "input_bytes": size,
            "output_bytes": produced,
            "passes": passes,
            "documents_per_second": round(documents / elapsed, 1),
            "mb_per_second": round(processed_bytes / 1048576.0 / elapsed, 2),
        }))


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("usage: strictyaml_bench.py <root> [throughput [seconds] [ser|deser|both]]", file=sys.stderr)
        sys.exit(2)
    if len(sys.argv) > 2 and sys.argv[2] == "throughput":
        seconds = float(sys.argv[3]) if len(sys.argv) > 3 else 10
        mode = sys.argv[4] if len(sys.argv) > 4 else "both"
        if mode not in ("ser", "deser", "both"):
            print("throughput mode must be ser, deser, or both", file=sys.stderr)
            sys.exit(2)
        throughput(sys.argv[1], seconds, mode)
    else:
        run(sys.argv[1])
