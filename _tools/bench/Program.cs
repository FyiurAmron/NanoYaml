using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using SharpYaml;
using YamlDotNet.Serialization;

namespace Vaxquis.NanoYaml.Bench;

static class Program {
    const int Iterations = 5;

    static int Main(string[] args) {
        if (args.Length == 0) {
            PrintHelp();
            return 0;
        }

        string command = args[0];
        return command switch {
            "generate" => args.Length > 1 ? Generate(args[1], args) : MissingArguments(command),
            "run" or "profile" or "throughput" or "cold" =>
                args.Length > 2 ? RunCommand(command, args) : MissingArguments(command),
            _ => Unknown(command),
        };
    }

    static int RunCommand(string command, string[] args) {
        string parser = args[1];
        string root = args[2];
        return command switch {
            "run" => Run(root, parser),
            "profile" => Profile(root, parser, args),
            "throughput" => Throughput(root, parser, args),
            "cold" => Cold(root, parser, args),
            _ => Unknown(command),
        };
    }

    static void PrintHelp() =>
        Console.WriteLine($@"NanoYaml benchmark runner

Usage:
  dotnet run --project _tools/bench -c Release -- <command> [parser] <root> [options]

Commands:
  generate <root> [small-files] [small-budget] [large-files] [large-budget]
      Generate all corpora with fixed seeds. Defaults: case1 has
      {Corpus.SmallFiles} files and budget {Corpus.SmallBudget}; case2 has
      {Corpus.LargeFiles} files and budget {Corpus.LargeBudget}.
      Typed corpora have fixed sizes: case3 has {Corpus.TypedSmallFiles} files
      and budget {Corpus.TypedSmallBudget}; case4 has {Corpus.TypedLargeFiles}
      files and budget {Corpus.TypedLargeBudget}. case5 has {Corpus.FlowFiles}
      files and budget {Corpus.FlowBudget}, written in flow style where small.
      case6 has {Corpus.NumberFiles} files of {Corpus.NumberBudget} series, numbers only.

  run <parser> <root>
      Run the five-round parse/serialize benchmark for every corpus; emit JSON.

  profile <parser> <root> [corpus] [iterations] [read|write]
      Repeat a read or serialization workload. Defaults: corpus=case2,
      iterations=20, mode=read.

  throughput <parser> <root> [corpus] [seconds] [ser|deser|both]
      Repeatedly parse and serialize for a fixed duration. Defaults:
      corpus=case1, seconds=10, mode=both. ser and deser measure only that
      operation; both measures parse plus serialize. Reports bytes allocated
      per document on the measuring thread.

  cold <parser> <root> [corpus]
      Time the first parse and the first serialization of the corpus's first
      file in this process, before anything is warm; emit JSON. Run it in a
      fresh process each time. Defaults: corpus=case1.

Parser for run, profile, and throughput: nano, yamldotnet, or sharpyaml.
Corpus: case1 or case2 (untyped trees), case3 or case4 (typed objects),
case5 (untyped trees in flow style), case6 (typed, numbers only).
generate creates <root>/case1 to <root>/case6; other commands expect the
corpus directories to exist. Parser arguments immediately follow the command.

All arguments are positional and must be supplied in the order shown.");

    static int MissingArguments(string command) {
        string usage = command switch {
            "generate" => "generate <root>",
            "run" => "run <parser> <root>",
            "profile" => "profile <parser> <root>",
            "throughput" => "throughput <parser> <root>",
            "cold" => "cold <parser> <root>",
            _ => command,
        };
        Console.Error.WriteLine($"usage: bench {usage} (run with no args for help)");
        return 2;
    }

    static int Profile(string root, string parser, string[] args) {
        string name = args.Length > 3 ? args[3] : "case2";
        int iterations = args.Length > 4 ? int.Parse(args[4]) : 20;
        (Func<string, object?> Parse, Func<object?, string> Write, string Label) backend =
            Backend(parser, name);
        bool write = args.Length > 5 && args[5] == "write";
        string directory = Path.Combine(root, name);
        List<string> files = [.. Directory.GetFiles(directory, "*.yaml").Order().Select(File.ReadAllText)];
        List<object?> trees = [.. files.Select(backend.Parse)];
        int done = 0;
        for (int i = 0; i < iterations; i++) {
            foreach (object? tree in trees) {
                done += write
                    ? backend.Write(tree).Length
                    : tree is null ? 0 : 1;
            }
        }

        Console.WriteLine($"processed {done} of {files.Count * iterations} documents from {name}");
        return 0;
    }

    static int Throughput(string root, string parser, string[] args) {
        string name = args.Length > 3 ? args[3] : "case1";
        double seconds = args.Length > 4
            ? double.Parse(args[4], CultureInfo.InvariantCulture)
            : 10;
        string mode = args.Length > 5 ? args[5] : "both";
        if (mode is not ("ser" or "deser" or "both")) {
            Console.Error.WriteLine($"unknown throughput mode '{mode}' (expected ser, deser, or both)");
            return 2;
        }

        (Func<string, object?> Parse, Func<object?, string> Write, string Label) backend =
            Backend(parser, name);

        string directory = Path.Combine(root, name);
        List<string> files = [.. Directory.GetFiles(directory, "*.yaml").Order().Select(File.ReadAllText)];
        long bytes = files.Sum(text => (long)text.Length);
        List<object?> trees = mode == "ser" ? [.. files.Select(backend.Parse)] : [];

        if (mode == "ser") {
            foreach (object? tree in trees) {
                backend.Write(tree);
            }
        } else {
            foreach (string text in files) {
                object? tree = backend.Parse(text);
                if (mode == "both") {
                    backend.Write(tree);
                }
            }
        }

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        Stopwatch watch = Stopwatch.StartNew();
        long documents = 0;
        long produced = 0;
        long passes = 0;
        while (watch.Elapsed.TotalSeconds < seconds) {
            if (mode == "ser") {
                foreach (object? tree in trees) {
                    produced += backend.Write(tree).Length;
                    documents++;
                }
            } else if (mode == "deser") {
                foreach (string text in files) {
                    _ = backend.Parse(text);
                    documents++;
                }
            } else {
                foreach (string text in files) {
                    produced += backend.Write(backend.Parse(text)).Length;
                    documents++;
                }
            }

            passes++;
        }

        watch.Stop();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        double elapsed = watch.Elapsed.TotalSeconds;
        Console.WriteLine(JsonSerializer.Serialize(new {
            parser = backend.Label,
            corpus = name,
            mode,
            seconds = Math.Round(elapsed, 3),
            documents,
            input_bytes = bytes,
            output_bytes = produced,
            passes,
            documents_per_second = Math.Round(documents / elapsed, 1),
            mb_per_second = Math.Round(bytes * (double)passes / 1048576.0 / elapsed, 2),
            allocated_bytes_per_document = documents == 0 ? 0 : allocated / documents,
        }));
        return 0;
    }

    static int Cold(string root, string parser, string[] args) {
        string name = args.Length > 3 ? args[3] : "case1";
        string text = File.ReadAllText(Directory.GetFiles(Path.Combine(root, name), "*.yaml").Order().First());
        (Func<string, object?> Parse, Func<object?, string> Write, string Label) backend =
            Backend(parser, name);
        Stopwatch watch = Stopwatch.StartNew();
        object? tree = backend.Parse(text);
        double parseMs = watch.Elapsed.TotalMilliseconds;
        watch.Restart();
        string output = backend.Write(tree);
        double writeMs = watch.Elapsed.TotalMilliseconds;
        Console.WriteLine(JsonSerializer.Serialize(new {
            parser = backend.Label,
            corpus = name,
            mode = "cold",
            input_bytes = text.Length,
            output_bytes = output.Length,
            first_parse_ms = Math.Round(parseMs, 3),
            first_serialize_ms = Math.Round(writeMs, 3),
        }));
        return 0;
    }

    static int Unknown(string command) {
        Console.Error.WriteLine($"unknown command '{command}'");
        return 2;
    }

    static int Generate(string root, string[] args) {
        int smallFiles = args.Length > 2 ? int.Parse(args[2]) : Corpus.SmallFiles;
        int smallBudget = args.Length > 3 ? int.Parse(args[3]) : Corpus.SmallBudget;
        int largeFiles = args.Length > 4 ? int.Parse(args[4]) : Corpus.LargeFiles;
        int largeBudget = args.Length > 5 ? int.Parse(args[5]) : Corpus.LargeBudget;
        Corpus.Write(Path.Combine(root, "case1"), 20261022, smallFiles, smallBudget);
        Corpus.Write(Path.Combine(root, "case2"), 20261023, largeFiles, largeBudget);
        Corpus.WriteTyped(Path.Combine(root, "case3"), 20261024, Corpus.TypedSmallFiles, Corpus.TypedSmallBudget);
        Corpus.WriteTyped(Path.Combine(root, "case4"), 20261025, Corpus.TypedLargeFiles, Corpus.TypedLargeBudget);
        Corpus.WriteFlow(Path.Combine(root, "case5"), 20261026, Corpus.FlowFiles, Corpus.FlowBudget);
        Corpus.WriteNumbers(Path.Combine(root, "case6"), 20261027, Corpus.NumberFiles, Corpus.NumberBudget);
        return 0;
    }

    static int Run(string root, string parser) {
        foreach (string name in Corpus.Names) {
            (Func<string, object?> Parse, Func<object?, string> Write, string Label) backend =
                Backend(parser, name);
            string directory = Path.Combine(root, name);
            List<string> files = [.. Directory.GetFiles(directory, "*.yaml").Order().Select(File.ReadAllText)];
            if (!files.Any()) {
                Console.Error.WriteLine($"no files in {directory}");
                return 2;
            }

            foreach (string text in files) {
                backend.Write(backend.Parse(text));
            }

            double parseMs = double.MaxValue;
            double writeMs = double.MaxValue;
            long output = 0;
            for (int round = 0; round < Iterations; round++) {
                List<object?> trees = [];
                Stopwatch watch = Stopwatch.StartNew();
                foreach (string text in files) {
                    trees.Add(backend.Parse(text));
                }

                watch.Stop();
                parseMs = Math.Min(parseMs, watch.Elapsed.TotalMilliseconds);
                long bytes = 0;
                watch = Stopwatch.StartNew();
                foreach (object? tree in trees) {
                    bytes += backend.Write(tree).Length;
                }

                watch.Stop();
                writeMs = Math.Min(writeMs, watch.Elapsed.TotalMilliseconds);
                output = bytes;
            }

            Console.WriteLine(JsonSerializer.Serialize(new {
                parser = backend.Label,
                corpus = name,
                files = files.Count,
                input_bytes = files.Sum(text => (long)text.Length),
                output_bytes = output,
                parse_ms = Math.Round(parseMs, 3),
                serialize_ms = Math.Round(writeMs, 3),
                total_ms = Math.Round(parseMs + writeMs, 3),
            }));
        }

        return 0;
    }

    static object? NanoParse(string text) =>
        NanoYaml.ParseRaw(text);

    static string NanoWrite(object? tree) =>
        NanoYaml.Serialize(tree);

    static string NanoWriteFlow(object? tree) =>
        Corpus.Flow.Serialize(tree);

    static readonly IDeserializer deserializer = new DeserializerBuilder().Build();

    static readonly ISerializer serializer = new SerializerBuilder().Build();

    static object? YamlDotNetParse(string text) =>
        deserializer.Deserialize<object>(text);

    static string YamlDotNetWrite(object? tree) =>
        serializer.Serialize(tree);

    static object? SharpYamlParse(string text) =>
        YamlSerializer.Deserialize<object>(text);

    static object? NanoParseTyped(string text) =>
        NanoYaml.Deserialize<World>(text);

    static object? YamlDotNetParseTyped(string text) =>
        deserializer.Deserialize<World>(text);

    static object? SharpYamlParseTyped(string text) =>
        YamlSerializer.Deserialize<World>(text);

    static object? NanoParseNumbers(string text) =>
        NanoYaml.Deserialize<Numbers>(text);

    static object? YamlDotNetParseNumbers(string text) =>
        deserializer.Deserialize<Numbers>(text);

    static object? SharpYamlParseNumbers(string text) =>
        YamlSerializer.Deserialize<Numbers>(text);

    static string SharpYamlWrite(object? tree) =>
        YamlSerializer.Serialize(tree);

    static (Func<string, object?> Parse, Func<object?, string> Write, string Label) Backend(string parser, string corpus) =>
        Corpus.IsNumbers(corpus)
            ? parser switch {
                "yamldotnet" => (YamlDotNetParseNumbers, YamlDotNetWrite, "YamlDotNet"),
                "sharpyaml" => (SharpYamlParseNumbers, SharpYamlWrite, "SharpYaml"),
                _ => (NanoParseNumbers, NanoWrite, "NanoYaml"),
            }
            : TreeBackend(parser, corpus);

    static (Func<string, object?> Parse, Func<object?, string> Write, string Label) TreeBackend(string parser, string corpus) =>
        (parser, Corpus.IsTyped(corpus)) switch {
            ("yamldotnet", false) => (YamlDotNetParse, YamlDotNetWrite, "YamlDotNet"),
            ("yamldotnet", true) => (YamlDotNetParseTyped, YamlDotNetWrite, "YamlDotNet"),
            ("sharpyaml", false) => (SharpYamlParse, SharpYamlWrite, "SharpYaml"),
            ("sharpyaml", true) => (SharpYamlParseTyped, SharpYamlWrite, "SharpYaml"),
            (_, false) when Corpus.IsFlow(corpus) => (NanoParse, NanoWriteFlow, "NanoYaml"),
            (_, false) => (NanoParse, NanoWrite, "NanoYaml"),
            (_, true) => (NanoParseTyped, NanoWrite, "NanoYaml"),
        };
}
