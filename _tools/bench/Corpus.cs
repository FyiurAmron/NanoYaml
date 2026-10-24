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

    public const int TypedSmallFiles = 1000;
    public const int TypedLargeFiles = 10;

    public const int TypedSmallBudget = 2;
    public const int TypedLargeBudget = 600;

    public static readonly List<string> Names = ["case1", "case2", "case3", "case4"];

    public static bool IsTyped(string name) =>
        name is "case3" or "case4";

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

    public static void Write(string root, int seed, int files, int budget) =>
        WriteAll(root, seed, files, rng => Serialize(BuildRoot(rng, budget)));

    public static void WriteTyped(string root, int seed, int files, int budget) =>
        WriteAll(root, seed, files, rng => {
            World world = BuildWorld(rng, budget);
            string text = Serialize(world);
            string again = Serialize(NanoYaml.Deserialize<World>(text));
            return again == text ? text : throw new InvalidOperationException($"typed round trip differs in {root}");
        });

    static void WriteAll(string root, int seed, int files, Func<Random, string> build) {
        Directory.CreateDirectory(root);
        Random rng = new(seed);
        for (int i = 0; i < files; i++) {
            File.WriteAllText(Path.Combine(root, $"case-{i + 1:D4}.yaml"), build(rng), new UTF8Encoding(false));
        }
    }

    static World BuildWorld(Random rng, int budget) =>
        new() {
            Title = Text(rng),
            Version = rng.Next(1, 100),
            Catalog = Keyed(rng, rng.Next(1, budget + 1), BuildItem),
            Units = [.. Enumerable.Range(0, rng.Next(1, budget + 1)).Select(_ => BuildUnit(rng, 0))],
        };

    static Unit BuildUnit(Random rng, int depth) =>
        new() {
            Id = $"{Word(rng)}-{rng.Next(100000)}",
            Name = Text(rng),
            Seed = rng.NextInt64(long.MinValue, long.MaxValue),
            Cost = rng.Next(4) == 0 ? null : Math.Round((decimal)rng.NextDouble() * 10000, 2),
            Stats = new() {
                Health = rng.Next(0, 10000),
                Mana = rng.Next(-100, 1000),
                Speed = Math.Round(rng.NextDouble() * 20, 3),
                Flying = rng.Next(2) == 0,
            },
            Items = [.. Enumerable.Range(0, rng.Next(0, 3)).Select(_ => BuildItem(rng))],
            Labels = Keyed(rng, rng.Next(0, 4), r => r.Next(6) == 0 ? null : Text(r)),
            Minions = depth >= 1
                ? []
                : [.. Enumerable.Range(0, rng.Next(0, 3)).Select(_ => BuildUnit(rng, depth + 1))],
        };

    static Item BuildItem(Random rng) =>
        new() {
            Name = Text(rng),
            Description = rng.Next(3) == 0 ? null : Sentence(rng),
            Rarity = (Rarity)rng.Next(4),
            Count = rng.Next(0, 1000),
            Weight = Math.Round(rng.NextDouble() * 100, 3),
            Stackable = rng.Next(2) == 0,
            Level = rng.Next(3) == 0 ? null : rng.Next(1, 100),
            Tags = [.. Enumerable.Range(0, rng.Next(0, 5)).Select(_ => Text(rng))],
            Modifiers = Keyed(rng, rng.Next(0, 4), r => r.Next(-50, 50)),
        };

    static Dictionary<string, T> Keyed<T>(Random rng, int count, Func<Random, T> build) =>
        Enumerable.Range(0, count).ToDictionary(i => Key(rng, i), _ => build(rng));

    static string Text(Random rng) =>
        Scalar(rng) as string ?? Word(rng);

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
