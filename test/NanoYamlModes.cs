using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vaxquis.NanoYaml;
using Xunit;

namespace Vaxquis.NanoYaml.Tests;

/// <summary>
/// The three parsing modes, and the helpers that read the same document
/// through all of them.
/// </summary>
static class Modes {
    public const string Profile = "%TAG ! nanoyaml:0.1.0:\n---\n";

    /// <summary>
    /// A deserializer in the given mode. Every call gets its own instance:
    /// a deserializer keeps the line of the failure it reports, so it is
    /// not shared between the test classes running in parallel.
    /// </summary>
    public static NanoYaml.Deserializer Deserializer(NanoYaml.ParsingMode mode) =>
        new(new() { ParsingMode = mode });

    public static NanoYaml.Deserializer Strict => Deserializer(NanoYaml.ParsingMode.Strict);

    public static NanoYaml.Deserializer Compatible => Deserializer(NanoYaml.ParsingMode.Compatible);

    public static NanoYaml.Deserializer Auto => Deserializer(NanoYaml.ParsingMode.Auto);

    /// <summary>
    /// Reads the document in every mode and asserts they agree. A document
    /// without the profile tag is tagged for the strict and compatible reads,
    /// since the strict mode wants the tag.
    /// </summary>
    public static T Common<T>(string yaml) {
        string tagged = Tag(yaml);
        T inStrict = Strict.Deserialize<T>(tagged);
        T inCompatible = Compatible.Deserialize<T>(tagged);
        T inAuto = Auto.Deserialize<T>(yaml);
        Assert.Equal(NanoYaml.Serialize(inStrict), NanoYaml.Serialize(inCompatible));
        Assert.Equal(NanoYaml.Serialize(inStrict), NanoYaml.Serialize(inAuto));
        return inStrict;
    }

    /// <summary>
    /// Reads the document raw in every mode and asserts they agree.
    /// </summary>
    public static object? CommonRaw(string yaml) {
        string tagged = Tag(yaml);
        object? inStrict = Strict.ParseRaw(tagged);
        object? inCompatible = Compatible.ParseRaw(tagged);
        object? inAuto = Auto.ParseRaw(yaml);
        Assert.True(Same(inStrict, inCompatible), $"{Show(inStrict)} != {Show(inCompatible)}");
        Assert.True(Same(inStrict, inAuto), $"{Show(inStrict)} != {Show(inAuto)}");
        return inStrict;
    }

    /// <summary>
    /// Asserts every mode rejects the document.
    /// </summary>
    public static void CommonFails<T>(string yaml) {
        string tagged = Tag(yaml);
        Assert.Throws<NanoYamlException>(() => Strict.Deserialize<T>(tagged));
        Assert.Throws<NanoYamlException>(() => Compatible.Deserialize<T>(tagged));
        Assert.Throws<NanoYamlException>(() => Auto.Deserialize<T>(yaml));
    }

    /// <summary>
    /// Puts the profile tag on a document that has none. A byte order mark
    /// stays in front of it, since only the very start of the document is one.
    /// </summary>
    static string Tag(string yaml) =>
        yaml.StartsWith("%TAG", StringComparison.Ordinal)
            ? yaml
            : yaml.Length > 0 && yaml[0] == '\uFEFF'
                ? $"\uFEFF{Profile}{yaml[1..]}"
                : Profile + yaml;

    public static bool Same(object? a, object? b) {
        if (a is null || b is null) {
            return a is null && b is null;
        }

        if ((a is IDictionary) != (b is IDictionary)) {
            return false;
        }

        if (a is IDictionary left && b is IDictionary right) {
            if (left.Count != right.Count) {
                return false;
            }

            foreach (DictionaryEntry entry in left) {
                if (!right.Contains(entry.Key) || !Same(entry.Value, right[entry.Key])) {
                    return false;
                }
            }

            return true;
        }

        if (a is IEnumerable first && a is not string &&
            b is IEnumerable second && b is not string) {
            var firstItems = first.Cast<object?>().ToList();
            var secondItems = second.Cast<object?>().ToList();
            return firstItems.Count == secondItems.Count &&
                   firstItems.Zip(secondItems).All(pair => Same(pair.First, pair.Second));
        }

        return a.Equals(b);
    }

    public static string Show(object? o) =>
        o switch {
            null => "null",
            string s => $"\"{Escape(s)}\"",
            IDictionary d => $"{{{JoinToString(d.Keys.Cast<object>().Select(k => $"{Show(k)}: {Show(d[k])}"))}}}",
            IEnumerable e => $"[{JoinToString(e.Cast<object>().Select(Show))}]",
            _ => o.ToString()!,
        };

    static string JoinToString(IEnumerable<string> items) =>
        items.Aggregate("", (left, right) => left.Length == 0 ? right : $"{left}, {right}");

    static string Escape(string s) =>
        s
            .Replace("\\", "\\\\")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")
            .Replace("\uFEFF", "<BOM>")
            .Replace("\u0085", "<NEL>")
            .Replace("\u00A0", "<NBSP>")
            .Replace("\u2028", "<LS>")
            .Replace("\u2029", "<PS>");
}
