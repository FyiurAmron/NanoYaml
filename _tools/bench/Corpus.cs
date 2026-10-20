using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Vaxquis.NanoYaml;

namespace Vaxquis.NanoYaml.Bench;

static class Corpus {
    public const int SmallFiles = 1000;
    public const int LargeFiles = 10;

    public const int SmallBudget = 200;
    public const int LargeBudget = 2000000;

    static readonly List<string> words = [
        "alpha", "beta", "gamma", "delta", "epsilon", "zeta", "eta", "theta",
        "iota", "kappa", "lambda", "mu", "nu", "xi", "omicron", "pi",
        "config", "value", "name", "kind", "count", "path", "host", "port",
        "user", "password", "token", "retry", "timeout", "buffer", "chunk",
        "north", "south", "east", "west", "up", "down", "left", "right",
    ];

    static readonly List<string> specials = [
        "a: b", "key # hash", "- dash", "quote's", "double\"quote", "[bracket]",
        "{brace}", "trailing ", " leading", "tab\there", "back\\slash", "amp&and",
        "semi;colon", "comma,separated", "star*glob", "at@sign", "per%cent",
    ];

    static readonly List<string> unicode = [
        "héllo wörld", "日本語テキスト", "emoji \U0001F389", "grüße", "ελλάδα",
        "русский текст", "עברית", "한국어",
    ];

    public static void Write(string root, int seed, int files, int budget) {
        Directory.CreateDirectory(root);
        Random rng = new(seed);
        for (int i = 0; i < files; i++) {
            object? tree = BuildRoot(rng, budget);
            string text = Serialize(tree);
            File.WriteAllText(Path.Combine(root, $"case-{i + 1:D4}.yaml"), text, new UTF8Encoding(false));
        }
    }

    public static string Serialize(object? tree) {
        string text = NanoYaml.Serialize(tree);
        List<string> lines = [.. text.Split('\n')];
        return lines.Skip(2).JoinToString("\n");
    }

    static object? BuildRoot(Random rng, int budget) {
        Dictionary<string, object?> map = [];
        int entries = rng.Next(3, 9);
        int share = Math.Max(1, budget / entries);
        for (int i = 0; i < entries; i++) {
            map[Key(rng, i)] = BuildCollection(rng, share, 1);
        }

        return map;
    }

    static object? BuildCollection(Random rng, int budget, int depth) {
        int count = rng.Next(1, Math.Min(budget, 12) + 1);
        int share = Math.Max(1, budget / (count + 1));
        if (rng.Next(100) < 40) {
            List<object?> items = [];
            for (int i = 0; i < count; i++) {
                items.Add(Build(rng, share, depth + 1));
            }

            return items;
        }

        Dictionary<string, object?> map = [];
        for (int i = 0; i < count; i++) {
            map[Key(rng, i)] = Build(rng, share, depth + 1);
        }

        return map;
    }

    static object? Build(Random rng, int budget, int depth) {
        if (budget <= 1 || depth >= 8) {
            return Scalar(rng);
        }

        int roll = rng.Next(100);
        if (roll < 52) {
            return Scalar(rng);
        }

        int count = rng.Next(1, Math.Min(budget, 12) + 1);
        int share = Math.Max(1, budget / (count + 1));
        if (roll < 78) {
            List<object?> items = [];
            for (int i = 0; i < count; i++) {
                items.Add(Build(rng, share, depth + 1));
            }

            return items;
        }

        Dictionary<string, object?> map = [];
        for (int i = 0; i < count; i++) {
            map[Key(rng, i)] = Build(rng, share, depth + 1);
        }

        return map;
    }

    static string Key(Random rng, int index) {
        string key = $"{Word(rng)}_{index}";
        int roll = rng.Next(100);
        if (roll < 12) {
            key = $"{key} {Word(rng)}";
        } else if (roll < 20) {
            key = $"{key}:{Word(rng)}";
        } else if (roll < 24) {
            key = $"{unicode[rng.Next(unicode.Count)]}_{index}";
        }

        return key;
    }

    static object? Scalar(Random rng) {
        int roll = rng.Next(100);
        return roll switch {
            < 6 => null,
            < 20 => rng.Next(-100000, 100000).ToString(CultureInfo.InvariantCulture),
            < 26 => (rng.NextDouble() * 1000).ToString("F3", CultureInfo.InvariantCulture),
            < 32 => rng.Next(0, 2) == 0 ? "true" : "false",
            < 36 => rng.Next(0, 2) == 0 ? "~" : "null",
            < 44 => specials[rng.Next(specials.Count)],
            < 52 => unicode[rng.Next(unicode.Count)],
            < 58 => "",
            < 68 => Sentence(rng),
            _ => Word(rng),
        };
    }

    static string Word(Random rng) =>
        words[rng.Next(words.Count)];

    static string Sentence(Random rng) {
        int count = rng.Next(6, 20);
        List<string> parts = [];
        for (int i = 0; i < count; i++) {
            parts.Add(Word(rng));
        }

        return parts.JoinToString(" ");
    }
}
