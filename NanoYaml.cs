using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Vaxquis.NanoYaml;

/// <summary>
/// Reports a YAML parsing, conversion, or serialization error.
/// </summary>
/// <param name="message">
/// The error description, without a line-number prefix.
/// </param>
/// <param name="line">
/// The one-based input line, or zero when unavailable.
/// </param>
/// <param name="inner">The exception that caused this error, if any.</param>
public class NanoYamlException(string message, int line = 0, Exception? inner = null)
    : InvalidOperationException(message, inner) {
    /// <summary>
    /// Gets the one-based input line associated with the error, or zero when
    /// no input line is available.
    /// </summary>
    public int Line { get; } = line;

    /// <summary>
    /// Gets the error description, prefixed with the input line when known.
    /// </summary>
    public override string Message =>
        Line > 0 ? $"line {Line}: {base.Message}" : base.Message;
}

/// <summary>
/// Provides YAML serialization and deserialization operations.
/// </summary>
public static class NanoYaml {
    /// <summary>
    /// The profile tag emitted by the serializer and accepted by the parser.
    /// </summary>
    public const string ProfileTag = "%TAG ! nanoyaml:0.1.0:";

    /// <summary>
    /// Deserializes a YAML document into <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="yaml">The YAML document to read.</param>
    /// <returns>The deserialized value.</returns>
    /// <exception cref="NanoYamlException">
    /// The document is invalid or cannot be converted to
    /// <typeparamref name="T"/>.
    /// </exception>
    public static T Deserialize<T>(string yaml) =>
        Deserializer.Default.Deserialize<T>(yaml);

    /// <summary>Parses a YAML document into an untyped object tree.</summary>
    /// <param name="yaml">The YAML document to read.</param>
    /// <returns>
    /// A mapping as <see cref="Dictionary{TKey,TValue}"/> or a sequence as
    /// <see cref="List{T}"/>; scalar values are strings or
    /// <see langword="null"/>. The root value may also be null.
    /// </returns>
    /// <exception cref="NanoYamlException">The document is invalid.</exception>
    public static object? ParseRaw(string yaml) =>
        Deserializer.Default.ParseRaw(yaml);

    /// <summary>Serializes a .NET value as a NanoYaml YAML document.</summary>
    /// <param name="obj">The value to serialize, or null.</param>
    /// <returns>A YAML document containing the NanoYaml profile tag.</returns>
    /// <exception cref="NanoYamlException">
    /// The value cannot be represented by the serializer.
    /// </exception>
    public static string Serialize(object? obj) =>
        Serializer.Default.Serialize(obj);

    /// <summary>
    /// Configures the serializer.
    /// </summary>
    /// <param name="IndentSize">
    /// The number of spaces per block nesting level.
    /// </param>
    /// <param name="AutoFlowListCount">
    /// The aggregate node-count threshold for emitting lists in flow style;
    /// zero keeps non-empty lists in block style.
    /// </param>
    /// <param name="AutoFlowMapCount">
    /// The aggregate node-count threshold for emitting mappings in flow style;
    /// zero keeps non-empty mappings in block style.
    /// </param>
    public record SerializerParams(
        int IndentSize = 2,
        uint AutoFlowListCount = 0,
        uint AutoFlowMapCount = 0
    );

    delegate void GenericMapWriter(
        Serializer serializer,
        StringBuilder sb,
        object obj,
        int indent,
        bool firstLineIndented,
        bool flow);

    /// <summary>
    /// Serializes values using configurable indentation and flow style.
    /// </summary>
    public class Serializer {
        /// <summary>
        /// A threshold that always selects flow style for lists or mappings.
        /// </summary>
        public const uint AlwaysFlow = int.MaxValue;

        const int MaxCachedBuilderCapacity = 1024 * 1024;
        [ThreadStatic]
        static StringBuilder? cachedBuilder;

        static readonly SerializerParams DefaultParams = new();

        /// <summary>
        /// Gets the default serializer used by the static facade.
        /// </summary>
        public static Serializer Default { get; } = new();

        readonly SerializerParams settings;

        /// <summary>
        /// Creates a serializer with the default settings.
        /// </summary>
        public Serializer() : this(DefaultParams) {
        }

        /// <summary>
        /// Creates a serializer with the given settings.
        /// </summary>
        /// <param name="settings">The serializer settings.</param>
        public Serializer(SerializerParams settings) {
            ArgumentNullException.ThrowIfNull(settings);
            this.settings = settings;
        }

        void Pad(StringBuilder sb, int depth) =>
            sb.Append(' ', depth * settings.IndentSize);

        bool TryLeaf(StringBuilder sb, object obj, string prefix, out TypeShape shape) {
            shape = null!;
            if (obj is null) {
                sb.Append(prefix).Append('~');
                return true;
            }

            if (obj is string text) {
                sb.Append(prefix);
                WriteText(sb, text);
                return true;
            }

            shape = ShapeOf(obj.GetType());
            if (shape.Basic) {
                sb.Append(prefix);
                SerializeBasic(sb, obj, false);
                return true;
            }

            if (!IsEmptyCollectionWithZeroFlowLimit(obj, shape, out bool emptyMap)) {
                return false;
            }

            sb.Append(prefix);
            SerializeEmptyCollection(sb, emptyMap);
            return true;
        }

        bool TryInline(StringBuilder sb, object obj, string prefix, out TypeShape shape) {
            if (TryLeaf(sb, obj, prefix, out shape)) {
                return true;
            }

            if (!CanFlow(obj, shape)) {
                return false;
            }

            sb.Append(prefix);
            SerializeFlowUnknown(sb, obj);
            return true;
        }

        void SerializeBlock(StringBuilder sb, object obj, TypeShape shape, int indent, bool firstLineIndented) {
            if (shape.Map) {
                SerializeMap(sb, obj, shape, indent, firstLineIndented);
            } else if (shape.List) {
                SerializeList(sb, obj, indent);
            } else {
                Dictionary<string, object> dict = ToDictionary(obj);
                SerializeMap(sb, dict, ShapeOf(dict.GetType()), indent, firstLineIndented);
            }
        }

        /// <summary>Serializes a .NET value as a YAML document.</summary>
        /// <param name="obj">The value to serialize, or null.</param>
        /// <returns>
        /// A YAML document containing the NanoYaml profile tag.
        /// </returns>
        /// <exception cref="NanoYamlException">
        /// The value cannot be represented by the serializer.
        /// </exception>
        public string Serialize(object? obj) {
            StringBuilder sb = cachedBuilder ?? new();
            cachedBuilder = null;
            sb.Clear();
            try {
                sb.Append(ProfileTag).Append("\n---\n");
                SerializeUnknown(sb, obj);
                return sb.ToString();
            } finally {
                if (sb.Capacity <= MaxCachedBuilderCapacity) {
                    sb.Clear();
                    cachedBuilder = sb;
                }
            }
        }

        void SerializeUnknown(StringBuilder sb, object obj) {
            if (!TryInline(sb, obj, "", out TypeShape shape)) {
                SerializeBlock(sb, obj, shape, 0, false);
            }
        }

        static bool Number(string str) {
            static bool Fraction(string str, int at) {
                int i = at + 1;
                if (i < str.Length && char.ToLowerInvariant(str[i]) is 'i' or 'n') {
                    ReadOnlySpan<char> rest = str.AsSpan(i);
                    return rest.Equals("inf", StringComparison.OrdinalIgnoreCase) ||
                           rest.Equals("nan", StringComparison.OrdinalIgnoreCase);
                }

                int digits = 0;
                while (i < str.Length && (str[i] is >= '0' and <= '9' || str[i] == '_')) {
                    i++;
                    digits++;
                }

                return digits > 0 && Exponent(str, i);
            }

            int at = str[0] is '+' or '-' ? 1 : 0;
            if (at >= str.Length) {
                return false;
            }

            char first = str[at];
            if (first == '.') {
                return Fraction(str, at);
            }

            if (first == '0' && at + 1 < str.Length) {
                char kind = char.ToLowerInvariant(str[at + 1]);
                if (kind is 'x' or 'o' or 'b') {
                    return Digits(str, at + 2, kind);
                }
            }

            if (first is < '0' or > '9') {
                return false;
            }

            int i = at;
            while (i < str.Length && (str[i] is >= '0' and <= '9' || str[i] == '_')) {
                i++;
            }

            if (i < str.Length && str[i] == '.') {
                i++;
                while (i < str.Length && (str[i] is >= '0' and <= '9' || str[i] == '_')) {
                    i++;
                }
            }

            return Exponent(str, i);
        }

        void SerializeBasic(StringBuilder sb, object obj, bool asKey) {
            if (obj is string plain) {
                WriteText(sb, plain);
                return;
            }

            Type type = obj.GetType();
            bool asText = obj is char || type.IsEnum || (asKey && !IsBasicType(type));
            if (!asText) {
                string special = "";
                if (obj is (double or float)) {
                    double value = Convert.ToDouble(obj, CultureInfo.InvariantCulture);
                    special = double.IsNaN(value)
                        ? ".nan"
                        : double.IsInfinity(value)
                            ? (value > 0 ? ".inf" : "-.inf")
                            : "";
                }

                sb.Append(special.Length > 0
                    ? special
                    : obj is bool b
                        ? (b ? "true" : "false")
                        : Convert.ToString(obj, CultureInfo.InvariantCulture));

                return;
            }

            WriteText(sb, obj.ToString()!);
        }

        static bool Special(ReadOnlySpan<char> chars) =>
            chars.ContainsAny(specialCharacters) ||
            chars.ContainsAnyInRange('\0', '\u001F') ||
            chars.ContainsAnyInRange('\u007F', '\u009F') ||
            chars.ContainsAnyInRange('\uD800', '\uDFFF') ||
            chars.ContainsAny(escapeCharacters);

        static void WriteText(StringBuilder sb, string str) {
            ReadOnlySpan<char> chars = str.AsSpan();
            int odd = chars.IndexOfAnyExcept(plainAscii);
            bool ascii = odd < 0 || (chars[odd] < '\u0080' && Ascii.IsValid(chars[odd..]));
            bool special = odd >= 0 && (chars[odd] < '\u0080' || Special(chars[odd..]));

            bool needsQuote =
                str.Length == 0 ||
                special ||
                str[0] is ' ' || str[^1] is ' ' ||
                leadingIndicators.Contains(str[0]) ||
                IsDocumentMarker(str) ||
                IsTypedWord(str) ||
                Number(str);

            if (needsQuote) {
                sb.Append('"');
                ReadOnlySpan<char> remaining = chars;
                while (!remaining.IsEmpty) {
                    int next = ascii ? remaining.IndexOfAny(asciiEscapeCharacters) : NextEscape(remaining);
                    if (next < 0) {
                        sb.Append(remaining);
                        break;
                    }

                    sb.Append(remaining[..next]);
                    char c = remaining[next];
                    if (char.IsHighSurrogate(c) && next + 1 < remaining.Length && char.IsLowSurrogate(remaining[next + 1])) {
                        int codePoint = char.ConvertToUtf32(c, remaining[next + 1]);
                        sb.Append("\\U").Append(codePoint.ToString("X8", CultureInfo.InvariantCulture));
                        remaining = remaining[(next + 2)..];
                        continue;
                    }

                    if (char.IsSurrogate(c)) {
                        throw new NanoYamlException("Unpaired surrogate cannot be serialized");
                    }

                    if (escapes.TryGetValue(c, out string escape)) {
                        sb.Append('\\').Append(escape);
                    } else if (c < ' ' || c == '\u007F' || c is >= '\u0080' and <= '\u009F') {
                        sb.Append("\\x").Append(((int)c).ToString("X2", CultureInfo.InvariantCulture));
                    } else {
                        sb.Append(c);
                    }

                    remaining = remaining[(next + 1)..];
                }

                sb.Append('"');
            } else {
                sb.Append(str);
            }
        }

        static int NextEscape(ReadOnlySpan<char> text) {
            int at = text.IndexOfAny(escapeCharacters);
            at = Earlier(at, text.IndexOfAnyInRange('\0', '\u001F'));
            at = Earlier(at, text.IndexOfAnyInRange('\u007F', '\u009F'));
            return Earlier(at, text.IndexOfAnyInRange('\uD800', '\uDFFF'));
        }

        static int Earlier(int first, int second) =>
            first < 0 ? second : second < 0 || first < second ? first : second;

        static bool Digits(string str, int at, char kind) {
            if (at >= str.Length) {
                return false;
            }

            for (int i = at; i < str.Length; i++) {
                char c = str[i];
                if (c == '_') {
                    continue;
                }

                bool ok = kind switch {
                    'x' => c is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F'),
                    'o' => c is >= '0' and <= '7',
                    _ => c is '0' or '1',
                };
                if (!ok) {
                    return false;
                }
            }

            return true;
        }

        static bool Exponent(string str, int at) {
            if (at >= str.Length) {
                return true;
            }

            if (str[at] is not ('e' or 'E')) {
                return false;
            }

            int i = at + 1;
            if (i < str.Length && (str[i] == '+' || str[i] == '-')) {
                i++;
            }

            int digits = 0;
            while (i < str.Length && str[i] is >= '0' and <= '9') {
                i++;
                digits++;
            }

            return digits > 0 && i == str.Length;
        }

        void WriteKey(StringBuilder sb, object key) {
            if (key is string text) {
                WriteText(sb, text);
            } else {
                SerializeBasic(sb, key, true);
            }
        }

        void BlockEntry(StringBuilder sb, object? key, object val, int indent, bool firstLineIndented, ref bool firstLine) {
            if (key == null) {
                return;
            }

            if (!firstLine) {
                sb.AppendLine();
            }

            if (!firstLine || !firstLineIndented) {
                Pad(sb, indent);
            }

            firstLine = false;
            WriteKey(sb, key);
            sb.Append(':');
            if (!TryInline(sb, val, " ", out TypeShape shape)) {
                sb.AppendLine();
                SerializeBlock(sb, val, shape, indent + 1, false);
            }
        }

        void FlowEntry(StringBuilder sb, object? key, object val, ref bool first) {
            if (key == null) {
                return;
            }

            if (!first) {
                sb.Append(", ");
            }

            first = false;
            WriteKey(sb, key);
            sb.Append(": ");
            SerializeFlowUnknown(sb, val);
        }

        void ListItem(StringBuilder sb, object v, int indent, ref bool first) {
            if (!first) {
                sb.AppendLine();
            }

            first = false;
            Pad(sb, indent);
            if (TryInline(sb, v, "- ", out TypeShape shape)) {
                return;
            }

            if (!shape.Map && shape.List) {
                sb.Append('-').AppendLine();
                SerializeList(sb, v, indent + 1);
            } else {
                sb.Append("- ");
                SerializeBlock(sb, v, shape, indent + 1, true);
            }
        }

        void FlowItem(StringBuilder sb, object v, ref bool first) {
            if (!first) {
                sb.Append(", ");
            }

            first = false;
            SerializeFlowUnknown(sb, v);
        }

        void SerializeMap(StringBuilder sb, object obj, TypeShape shape, int indent, bool firstLineIndented) {
            if (shape.WriteGenericMap is { } writer) {
                writer(this, sb, obj, indent, firstLineIndented, false);
                return;
            }

            bool firstLine = true;
            foreach (DictionaryEntry entry in (IDictionary)obj) {
                BlockEntry(sb, entry.Key, entry.Value, indent, firstLineIndented, ref firstLine);
            }
        }

        void SerializeMap<TKey, TValue>(StringBuilder sb, object obj, int indent, bool firstLineIndented) {
            bool firstLine = true;
            if (obj is Dictionary<TKey, TValue> dict) {
                foreach (KeyValuePair<TKey, TValue> entry in dict) {
                    BlockEntry(sb, entry.Key, entry.Value!, indent, firstLineIndented, ref firstLine);
                }

                return;
            }

            foreach (KeyValuePair<TKey, TValue> entry in (IDictionary<TKey, TValue>)obj) {
                BlockEntry(sb, entry.Key, entry.Value!, indent, firstLineIndented, ref firstLine);
            }
        }

        void SerializeList(StringBuilder sb, object obj, int indent) {
            bool first = true;
            if (obj is List<object> list) {
                foreach (object v in list) {
                    ListItem(sb, v, indent, ref first);
                }

                return;
            }

            foreach (object v in (IEnumerable)obj) {
                ListItem(sb, v, indent, ref first);
            }
        }

        void SerializeFlowUnknown(StringBuilder sb, object obj) {
            if (TryLeaf(sb, obj, "", out TypeShape shape)) {
                return;
            }

            if (shape.Map) {
                SerializeFlowMap(sb, obj, shape);
            } else if (shape.List) {
                SerializeFlowList(sb, obj);
            } else {
                Dictionary<string, object> dict = ToDictionary(obj);
                SerializeFlowMap(sb, dict, ShapeOf(dict.GetType()));
            }
        }

        void SerializeFlowList(StringBuilder sb, object obj) {
            bool first = true;
            sb.Append('[');
            if (obj is List<object> list) {
                foreach (object v in list) {
                    FlowItem(sb, v, ref first);
                }
            } else {
                foreach (object v in (IEnumerable)obj) {
                    FlowItem(sb, v, ref first);
                }
            }

            sb.Append(']');
        }

        void SerializeFlowMap(StringBuilder sb, object obj, TypeShape shape) {
            sb.Append('{');
            if (shape.WriteGenericMap is { } writer) {
                writer(this, sb, obj, 0, false, true);
            } else {
                bool first = true;
                foreach (DictionaryEntry entry in (IDictionary)obj) {
                    FlowEntry(sb, entry.Key, entry.Value!, ref first);
                }
            }

            sb.Append('}');
        }

        void SerializeFlowMap<TKey, TValue>(StringBuilder sb, object obj) {
            bool first = true;
            if (obj is Dictionary<TKey, TValue> dict) {
                foreach (KeyValuePair<TKey, TValue> entry in dict) {
                    FlowEntry(sb, entry.Key, entry.Value!, ref first);
                }

                return;
            }

            foreach (KeyValuePair<TKey, TValue> entry in (IDictionary<TKey, TValue>)obj) {
                FlowEntry(sb, entry.Key, entry.Value!, ref first);
            }
        }

        bool IsEmptyCollectionWithZeroFlowLimit(object obj, TypeShape shape, out bool isMap) {
            isMap = false;
            if ((settings.AutoFlowMapCount != 0 && settings.AutoFlowListCount != 0)
                || obj is not ICollection { Count: 0 }) {
                return false;
            }

            isMap = shape.Map;
            return (isMap || shape.List) &&
                   (isMap ? settings.AutoFlowMapCount : settings.AutoFlowListCount) == 0;
        }

        static void SerializeEmptyCollection(StringBuilder sb, bool isMap) =>
            sb.Append(isMap ? "{}" : "[]");

        static void WriteGenericMap<TKey, TValue>(
            Serializer serializer,
            StringBuilder sb,
            object obj,
            int indent,
            bool firstLineIndented,
            bool flow) {
            if (flow) {
                serializer.SerializeFlowMap<TKey, TValue>(sb, obj);
            } else {
                serializer.SerializeMap<TKey, TValue>(sb, obj, indent, firstLineIndented);
            }
        }

        static bool IsCollection(object node, out bool isMap) {
            isMap = false;
            if (node is null or string) {
                return false;
            }

            TypeShape shape = ShapeOf(node.GetType());
            isMap = shape.Map || !shape.List;
            return !shape.Basic;
        }

        bool CanFlow(object node, TypeShape shape) {
            bool isMap = shape.Map || !shape.List;
            uint limit = isMap ? settings.AutoFlowMapCount : settings.AutoFlowListCount;
            return limit >= AlwaysFlow ||
                   CountChildren(node, isMap, limit) <= limit &&
                   ChildValues(node, isMap).All(child => !IsCollection(child, out _) || CanFlow(child, ShapeOf(child.GetType())));
        }

        ulong CountChildren(object node, bool isMap, uint cap) {
            int KeyCount(IDictionary dict) {
                int count = 0;
                foreach (object key in dict.Keys) {
                    if (key != null && ++count > cap) {
                        break;
                    }
                }

                return count;
            }

            int ChildCount() =>
                !isMap
                    ? node is ICollection collection
                        ? collection.Count
                        : ((IEnumerable)node).Cast<object>().Count()
                    : node is IDictionary dict
                        ? KeyCount(dict)
                        : ToDictionary(node).Count;

            ulong total = (ulong)ChildCount();
            if (total > cap) {
                return total;
            }

            foreach (object child in ChildValues(node, isMap)) {
                if (!IsCollection(child, out bool childIsMap)) {
                    continue;
                }

                total += CountChildren(child, childIsMap, cap);
                if (total > cap) {
                    return total;
                }
            }

            return total;
        }

        IEnumerable<object> ChildValues(object node, bool isMap) {
            if (isMap) {
                if (node is IDictionary dict) {
                    List<object> values = [];
                    foreach (DictionaryEntry entry in dict) {
                        if (entry.Key != null) {
                            values.Add(entry.Value);
                        }
                    }

                    return values;
                }

                return ToDictionary(node).Values;
            }

            return ((IEnumerable)node).Cast<object>();
        }

        static readonly SearchValues<char> leadingIndicators = SearchValues.Create("-?&*!%|>@`");
        static readonly SearchValues<char> specialCharacters = SearchValues.Create(":\"\'\n[]{}#,\\\r\t\0\a\b\v\f\e\x85\xa0\u2028\u2029");
        static readonly SearchValues<char> escapeCharacters = SearchValues.Create("\\\"\u00A0\u2028\u2029\uFEFF\uFFFE\uFFFF");
        static readonly string asciiControls = $"{new string([.. Enumerable.Range(0, 32).Select(c => (char)c)])}\u007F";
        static readonly SearchValues<char> //
            plainAscii = SearchValues.Create(new string([.. Enumerable.Range(' ', '\u007F' - ' ').Select(c => (char)c).Where(c => !":\"\'[]{}#,\\".Contains(c))])),
            asciiEscapeCharacters = SearchValues.Create($"\\\"{asciiControls}");
        static bool IsTypedWord(string str) =>
            str.Length <= MaxTypedWordLength && str.Length > 0 && (str[0] | 0x20) is '~' or 'y' or 'n' or 'o' or 't' or 'f' &&
            typedWords.Contains(str);

        const int MaxTypedWordLength = 5;

        static readonly Dictionary<char, string> escapes = new() {
            ['\\'] = "\\",
            ['"'] = "\"",
            ['\0'] = "0",
            ['\a'] = "a",
            ['\b'] = "b",
            ['\t'] = "t",
            ['\n'] = "n",
            ['\v'] = "v",
            ['\f'] = "f",
            ['\r'] = "r",
            ['\u001B'] = "e",
            ['\u0085'] = "N",
            ['\u00A0'] = "_",
            ['\u2028'] = "L",
            ['\u2029'] = "P",
            ['\uFEFF'] = "uFEFF",
            ['\uFFFE'] = "uFFFE",
            ['\uFFFF'] = "uFFFF",
        };

        Dictionary<string, object> ToDictionary(object obj) {
            if (obj == null) {
                return [];
            }

            Type type = obj.GetType();
            Dictionary<string, object> dictionary = [];
            foreach (Member member in ShapeOf(type).Readable) {
                dictionary[member.Name] = member.Get!(obj);
            }

            return dictionary;
        }
    }

    /// <summary>
    /// Selects how the deserializer reads a document.
    /// </summary>
    public enum ParsingMode {
        /// <summary>
        /// Reads the NanoYaml profile only: the profile tag is required and
        /// the strict subset is the only accepted syntax.
        /// </summary>
        Strict,

        /// <summary>
        /// Reads the NanoYaml profile and the valid YAML 1.2.2 constructs the
        /// strict profile rejects; the profile tag is optional.
        /// </summary>
        Compatible,

        /// <summary>
        /// Reads documents with the profile tag in <see cref="Strict"/> mode
        /// and documents without it in <see cref="Compatible"/> mode.
        /// </summary>
        Auto,
    }

    /// <summary>
    /// Configures the deserializer.
    /// </summary>
    /// <param name="IndentSize">
    /// The number of spaces per block nesting level.
    /// </param>
    /// <param name="MaxDepth">
    /// The maximum permitted nesting depth; defaults to 128.
    /// </param>
    /// <param name="ParsingMode">
    /// How strictly documents are read; defaults to
    /// <see cref="ParsingMode.Auto"/>.
    /// </param>
    public record DeserializerParams(
        int IndentSize = 2,
        int MaxDepth = 128,
        ParsingMode ParsingMode = ParsingMode.Auto
    );

    /// <summary>
    /// Deserializes YAML documents with configurable indentation and depth.
    /// </summary>
    public class Deserializer {
        static readonly DeserializerParams DefaultParams = new();

        /// <summary>
        /// Gets the shared default deserializer used by the static facade.
        /// </summary>
        public static Deserializer Default { get; } = new();

        readonly DeserializerParams settings;
        readonly ParsingMode defaultMode;
        ParsingMode mode;

        /// <summary>
        /// Creates a deserializer with the default settings.
        /// </summary>
        public Deserializer() : this(DefaultParams) {
        }

        /// <summary>
        /// Creates a deserializer with the given settings.
        /// </summary>
        /// <param name="settings">The deserializer settings.</param>
        public Deserializer(DeserializerParams settings) {
            ArgumentNullException.ThrowIfNull(settings);
            this.settings = settings;
            defaultMode = settings.ParsingMode;
            mode = defaultMode;
        }

        int line;

        static readonly SearchValues<char> flowSeparators = SearchValues.Create(" ,]}[{");

        NanoYamlException Fail(string message, Exception? inner = null) =>
            new(message, line, inner);

        NanoYamlException Unbalanced(string text) =>
            Fail($"Unbalanced flow: '{text}'");

        NanoYamlException UnbalancedIn(string kind, string text) =>
            Fail($"Unbalanced flow {kind}: '{text}'");

        NanoYamlException EmptyFlowEntry(string text) =>
            Fail($"Empty flow entry: '{text}'");

        NanoYamlException NoConcrete(Type type) =>
            Fail($"Cannot create concrete type for interface/abstract type: {type}");

        readonly record struct Line(
            int Offset,
            int Length,
            int Start,
            int End,
            int Indent,
            int Colon,
            bool Blank,
            bool TabIndent);

        class Lines {
            public string Text = "";
            public List<Line> Items = [];

            public ReadOnlySpan<char> Raw(int index) {
                Line item = Items[index];
                return Text.AsSpan(item.Offset, item.Length);
            }

            public ReadOnlySpan<char> Content(in Line item) =>
                Text.AsSpan(item.Start, item.End - item.Start);
        }

        static readonly SearchValues<char> lineMarks = SearchValues.Create("'\"#:");

        static (int Clean, int Colon) ScanLine(ReadOnlySpan<char> line) {
            int colon = -1;
            int at = 0;
            while (true) {
                int next = line[at..].IndexOfAny(lineMarks);
                if (next < 0) {
                    return (line.Length, colon);
                }

                at += next;
                char c = line[at];
                if (c == '#') {
                    if (at == 0 || char.IsWhiteSpace(line[at - 1])) {
                        return (line[..at].TrimEnd().Length, colon);
                    }

                    at++;
                } else if (c == ':') {
                    if (colon < 0 && (at + 1 == line.Length || char.IsWhiteSpace(line[at + 1]))) {
                        colon = at;
                    }

                    at++;
                } else {
                    at = SkipQuoted(line, at + 1, c);
                }
            }
        }

        static int SkipQuoted(ReadOnlySpan<char> line, int at, char quote) {
            while (true) {
                int next = line[at..].IndexOfAny(quote, '\\');
                if (next < 0) {
                    return line.Length;
                }

                at += next;
                if (line[at] == quote) {
                    return at + 1;
                }

                at = Math.Min(at + 2, line.Length);
            }
        }

        static Line Describe(ReadOnlySpan<char> text, int offset, int length) {
            ReadOnlySpan<char> raw = text.Slice(offset, length);
            (int clean, int colon) = ScanLine(raw);
            int indent = IndentOf(raw[..clean]);
            return new(
                offset,
                length,
                offset + indent,
                offset + clean,
                indent,
                colon < 0 ? -1 : offset + colon,
                raw[..clean][indent..].IsWhiteSpace(),
                indent < length && raw[indent] == '\t');
        }

        struct QuoteWalker {
            bool inQuotes, skip;
            char quote;

            public bool Plain(char c) {
                if (skip) {
                    skip = false;
                    return false;
                }

                if (inQuotes) {
                    if (c == '\\') {
                        skip = true;
                        return false;
                    }

                    if (c == quote) {
                        inQuotes = false;
                    }

                    return false;
                }

                if (c is '\'' or '"') {
                    inQuotes = true;
                    quote = c;
                    return false;
                }

                return true;
            }
        }

        static bool Blank(Lines lines, int index) =>
            lines.Raw(index).IsWhiteSpace();

        static int IndentOf(ReadOnlySpan<char> line) {
            int at = 0;
            while (at < line.Length && line[at] == ' ') {
                at++;
            }

            return at;
        }

        static bool IsEmpty(object node) =>
            node is ICollection { Count: 0 };

        int IndexOfContent(Lines lines, int from, int to) {
            for (int i = from; i < to; i++) {
                if (!lines.Items[i].Blank) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Deserializes a YAML document into <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <param name="yaml">The YAML document to read.</param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="NanoYamlException">
        /// The document is invalid or cannot be converted to
        /// <typeparamref name="T"/>.
        /// </exception>
        public T Deserialize<T>(string yaml) {
            object? raw = ParseRaw(yaml);
            line = 0;
            object loaded = DeserializeUnknown(raw, typeof(T), 0);
            return loaded == null
                ? (T)Null(typeof(T))
                : loaded is T done
                    ? done
                    : throw Fail($"Cannot load {Shape(loaded)} into {typeof(T).Name}");
        }

        /// <summary>
        /// Parses a YAML document into an untyped object tree.
        /// </summary>
        /// <param name="yaml">The YAML document to read.</param>
        /// <returns>
        /// A mapping as <see cref="Dictionary{TKey,TValue}"/> or a sequence as
        /// <see cref="List{T}"/>; scalar values are strings or
        /// <see langword="null"/>. The root value may also be null.
        /// </returns>
        /// <exception cref="NanoYamlException">
        /// The document is invalid.
        /// </exception>
        public object? ParseRaw(string yaml) {
            static bool HasProfileTag(string yaml) {
                ReadOnlySpan<char> text = yaml.AsSpan(HasBom(yaml) ? 1 : 0);
                bool leading = true;
                while (!text.IsEmpty) {
                    int end = text.IndexOf('\n');
                    ReadOnlySpan<char> line = end < 0 ? text : text[..end];
                    text = end < 0 ? [] : text[(end + 1)..];
                    if (line.EndsWith("\r")) {
                        line = line[..^1];
                    }

                    if (leading) {
                        if (line.IsWhiteSpace()) {
                            continue;
                        }

                        leading = false;
                    }

                    if (line.IsEmpty || line[0] != '%') {
                        return false;
                    }

                    if (line.SequenceEqual(ProfileTag)) {
                        return true;
                    }
                }

                return false;
            }

            void BanTabs(string yaml) {
                if (mode == ParsingMode.Compatible) {
                    return;
                }

                int at = yaml.IndexOf('\t');
                if (at >= 0) {
                    line = yaml[..at].Count(c => c == '\n') + 1;
                    throw Fail("Tabs are banned");
                }
            }

            Lines Split(string yaml) {
                string text = (HasBom(yaml) ? yaml[1..] : yaml).Replace("\r\n", "\n").Replace('\r', '\n');
                ReadOnlySpan<char> span = text.AsSpan();
                Lines lines = new() { Text = text, Items = new(span.Count('\n') + 1) };
                int start = 0;
                while (true) {
                    int end = span[start..].IndexOf('\n');
                    if (end < 0) {
                        break;
                    }

                    lines.Items.Add(Describe(span, start, end));
                    start += end + 1;
                }

                lines.Items.Add(Describe(span, start, text.Length - start));
                if (lines.Items[^1].Length == 0 && yaml.EndsWith('\n')) {
                    lines.Items.RemoveAt(lines.Items.Count - 1);
                }

                return lines;
            }

            (int From, int To) StripHeader(Lines lines, ParsingMode mode) {
                static string DirectiveName(string text) {
                    int end = 1;
                    while (end < text.Length && !char.IsWhiteSpace(text[end])) {
                        end++;
                    }

                    return text[1..end];
                }

                void RejectYaml(string text) {
                    string version = ValidateYaml(text);
                    throw Fail($"Valid %YAML {version} directive found. Either remove the directive or use another YAML parser.");
                }

                int count = lines.Items.Count;
                int at = 0;
                while (at < count && Blank(lines, at)) {
                    at++;
                }

                if (at == count) {
                    return (0, count);
                }

                int first = at;
                bool sawTag = false;
                while (at < count && lines.Raw(at).Length > 0 && lines.Raw(at)[0] == '%') {
                    if (lines.Raw(at).SequenceEqual(ProfileTag)) {
                        if (sawTag) {
                            line = at + 1;
                            throw Fail($"Repeated profile tag: '{ProfileTag}'");
                        }

                        sawTag = true;
                    }

                    at++;
                }

                for (int directive = first; directive < at; directive++) {
                    line = directive + 1;
                    string text = lines.Raw(directive).ToString();
                    string name = DirectiveName(text);
                    if (name.Length == 0) {
                        throw Fail($"Bad directive: '{text.Trim()}'");
                    }

                    if (name == "YAML") {
                        if (mode == ParsingMode.Strict) {
                            RejectYaml(text);
                        }

                        ValidateYaml(text);
                    } else if (name == "TAG") {
                        if (text != ProfileTag && mode == ParsingMode.Strict) {
                            throw Fail($"non-profile tag directive '{text.Trim()}'");
                        }
                    } else if (mode == ParsingMode.Strict) {
                        throw Fail($"Unknown directive '%{name}'");
                    }
                }

                if (sawTag) {
                    if (at >= count || !lines.Raw(at).SequenceEqual("---")) {
                        line = Math.Min(at + 1, count);
                        throw Fail("Directives need bare '---'");
                    }

                    at++;
                } else if (mode == ParsingMode.Compatible) {
                    int scan = at;
                    while (scan < count && Blank(lines, scan)) {
                        scan++;
                    }

                    if (scan < count && lines.Raw(scan).Trim().SequenceEqual("---".AsSpan())) {
                        at = scan + 1;
                    }
                }

                line = at + 1;
                return mode == ParsingMode.Strict && !sawTag
                    ? throw Fail($"Missing profile tag: expected '{ProfileTag}'")
                    : (at, count);
            }

            bool TrySingleNodeDoc(Lines lines, int from, int to, out object value) {
                static bool IsPlainDocLine(ReadOnlySpan<char> line) {
                    ReadOnlySpan<char> text = line.Trim();

                    if (text.IsEmpty) {
                        return true;
                    }

                    char first = text[0];

                    return !(first == '-' && (text.Length == 1 || text[1] == ' ')) &&
                           first is not ('[' or '{' or '|' or '>') &&
                           !HasKeyColon(text);
                }

                value = null!;
                int first = IndexOfContent(lines, from, to);
                if (first < 0) {
                    return false;
                }

                line = first + 1;
                if (mode == ParsingMode.Compatible) {
                    BanIndentTab(lines.Raw(first));
                }

                ReadOnlySpan<char> head = CleanLine(lines.Raw(first)).Trim();
                if (head[0] == '[' || head[0] == '{') {
                    List<string> content = [];
                    for (int i = first; i < to; i++) {
                        ReadOnlySpan<char> cleaned = CleanLine(lines.Raw(i));
                        if (!cleaned.IsWhiteSpace()) {
                            content.Add(cleaned.ToString());
                        }
                    }

                    string text = content.JoinToString("\n");
                    (object node, int next) = head[0] == '['
                        ? ParseFlowList(text, 0, 1)
                        : ParseFlowMap(text, 0, 1);
                    if (next != text.Length) {
                        throw Fail($"Trailing flow content: '{head.ToString()}'");
                    }

                    value = node;
                    return true;
                }

                List<string> plain = [];
                for (int i = first; i < to; i++) {
                    ReadOnlySpan<char> cleaned = CleanLine(lines.Raw(i));
                    if (cleaned.IsWhiteSpace()) {
                        continue;
                    }

                    if (mode == ParsingMode.Compatible && IsDocumentEnd(cleaned)) {
                        continue;
                    }

                    if (!IsPlainDocLine(cleaned)) {
                        return false;
                    }

                    plain.Add(cleaned.Trim().ToString());
                }

                value = plain.Any() ? ResolveValue(plain.JoinToString(" ")) : null!;
                return true;
            }

            mode = defaultMode == ParsingMode.Auto
                ? HasProfileTag(yaml) ? ParsingMode.Strict : ParsingMode.Compatible
                : defaultMode;

            line = 0;
            BanTabs(yaml);
            Lines lines = Split(yaml);

            (int from, int to) = StripHeader(lines, mode);
            int start = from + 1;
            if (IndexOfContent(lines, from, to) < 0) {
                line = 0;
                return mode == ParsingMode.Compatible
                    ? null
                    : throw Fail("Empty document is invalid");
            }

            line = start;
            return TrySingleNodeDoc(lines, from, to, out object scalar)
                ? scalar
                : PreProcess(lines, from, to, 0, null).Item1;
        }

        object Null(Type type) =>
            type.IsValueType && UnwrapNullable(type) == type
                ? throw Fail($"Null incompatible with {type.Name}")
                : null!;

        object DeserializeUnknown(object obj, Type type, int depth) {
            object DeserializeBasic(object value, Type type) {
                object ParseBool(string marker) {
                    Dictionary<string, bool> table = mode == ParsingMode.Compatible ? compatibleBools : strictBools;
                    return table.TryGetValue(marker, out bool value)
                        ? value
                        : throw Fail($"non-'true'/'false' scalar for bool '{marker}'");
                }

                object ChangeType(object value) =>
                        Convert.ChangeType(value, type, CultureInfo.InvariantCulture);

                object ParseNumber(string text, Type type) {
                    static bool IsNumeric(Type type) =>
                        (type.IsPrimitive && type != typeof(bool) && type != typeof(char)) || type == typeof(decimal);

                    bool IsNumberText(string text) {
                        if (SpecialForm(text) is not null) {
                            return true;
                        }

                        if (text.Length > 2 && text[0] == '0' && text[1] == 'x') {
                            int hex = 0;
                            for (int i = 2; i < text.Length; i++) {
                                char c = text[i];
                                if (c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F') {
                                    hex++;
                                }
                            }

                            return hex > 0 && hex == text.Length - 2;
                        }

                        if (mode == ParsingMode.Compatible && text.Length > 2 && text[0] == '0' && text[1] == 'o') {
                            int octal = 0;
                            for (int i = 2; i < text.Length; i++) {
                                char c = text[i];
                                if (c is >= '0' and <= '7') {
                                    octal++;
                                }
                            }

                            return octal > 0 && octal == text.Length - 2;
                        }

                        int at = text[0] is '+' or '-' ? 1 : 0;
                        int digits = 0;
                        int dots = 0;
                        while (at < text.Length && (text[at] is >= '0' and <= '9' || text[at] == '.')) {
                            if (text[at] == '.') {
                                dots++;
                            } else {
                                digits++;
                            }

                            at++;
                        }

                        if (dots > 1) {
                            return false;
                        }

                        if (at < text.Length && (text[at] is 'e' or 'E')) {
                            at++;
                            if (at < text.Length && (text[at] is '+' or '-')) {
                                at++;
                            }

                            int exponent = 0;
                            while (at < text.Length && text[at] is >= '0' and <= '9') {
                                at++;
                                exponent++;
                            }

                            if (exponent == 0) {
                                return false;
                            }
                        }

                        return at == text.Length && digits > 0;
                    }

                    object ParseRadix(string text, string digits, int radix, Type type) {
                        try {
                            return ChangeType(Convert.ToInt64(digits, radix));
                        } catch (Exception e) when (e is FormatException or OverflowException or InvalidCastException) {
                            throw Fail($"cannot read '{text}' as {type.Name}", e);
                        }
                    }

                    object SpecialDouble(string text, Type type) =>
                        ChangeType(SpecialForm(text) is { } value ? value : text);

                    bool relaxed = mode == ParsingMode.Compatible;

                    if (text.StartsWith("0o") && text.Length > 2) {
                        return relaxed
                            ? ParseRadix(text, text[2..], 8, type)
                            : throw Fail($"leading '0o' '{text}'");
                    }

                    string digits = text.TrimStart('+', '-');
                    string head = type == typeof(double) || type == typeof(float)
                        ? digits.Split('.', 'e', 'E')[0]
                        : digits;
                    if (!relaxed && head.Length > 1 && head[0] == '0' && head.All(char.IsDigit)) {
                        throw Fail($"leading zero '{text}'");
                    }

                    if (IsNumeric(type) && !IsNumberText(text)) {
                        throw Fail($"cannot read '{text}' as {type.Name}");
                    }

                    try {
                        return text.StartsWith("0x") && text.Length > 2
                            ? ChangeType(Convert.ToInt64(text[2..], 16))
                            : type == typeof(double) || type == typeof(float)
                                ? SpecialDouble(text, type)
                                : ChangeType(text);
                    } catch (Exception e) when (e is FormatException or OverflowException or InvalidCastException) {
                        throw Fail($"cannot read '{text}' as {type.Name}", e);
                    }
                }

                if (value == null) {
                    return Null(type);
                }

                type = UnwrapNullable(type);

                if (value is not string marker) {
                    throw Fail($"{type.Name} needs text, not {Shape(value)}");
                }

                if (marker.Length == 0 && type != typeof(string)) {
                    throw Fail($"{type.Name} has no empty: ''");
                }

                if (type.IsEnum) {
                    try {
                        return Enum.Parse(type, value.ToString()!);
                    } catch (Exception e) when (e is ArgumentException or OverflowException) {
                        throw Fail($"cannot read '{value}' as {type.Name}", e);
                    }
                } else if (type != typeof(string)) {
                    return type == typeof(bool)
                        ? ParseBool(marker)
                        : ParseNumber(marker, type);
                }

                return marker;
            }

            IDictionary DeserializeMap(object value, Type type, Type keyType, Type valueType, int depth) {
                if (value is List<object> { Count: 0 }) {
                    return (IDictionary)Activator.CreateInstance(GetConcreteMapType(type))!;
                }

                if (value is not Dictionary<object, object> data) {
                    throw Fail($"Cannot load {Shape(value)} into {type.Name}");
                }

                IDictionary result = (IDictionary)Activator.CreateInstance(GetConcreteMapType(type))!;
                data.ForEach(kvp =>
                    result[DeserializeUnknown(kvp.Key, keyType, depth + 1)] =
                        DeserializeUnknown(kvp.Value, valueType, depth + 1));
                return result;
            }

            IList DeserializeList(object value, Type type, Type valueType, int depth) {
                if (value is Dictionary<object, object> { Count: 0 }) {
                    return (IList)Activator.CreateInstance(GetConcreteListType(type, valueType))!;
                }

                if (value is not List<object> data) {
                    throw Fail($"Cannot load {Shape(value)} into {type.Name}");
                }

                type = GetConcreteListType(type, valueType);

                if (type.IsArray) {
                    IList result = Array.CreateInstance(type.GetElementType()!, data.Count);
                    for (int i = 0; i < result.Count; i++) {
                        result[i] = DeserializeUnknown(data[i], valueType, depth + 1);
                    }

                    return result;
                } else {
                    IList result = (IList)Activator.CreateInstance(type)!;
                    data.ForEach(item => result.Add(DeserializeUnknown(item, valueType, depth + 1)));
                    return result;
                }
            }

            object DeserializeSequence(List<object> sequence, Type type, int depth) {
                List<ConstructorInfo> ctors =
                    [.. type.GetConstructors().Where(c => c.GetParameters().Length == sequence.Count)];
                if (ctors.Count != 1) {
                    throw Fail($"{type.Name} needs one {sequence.Count}-field constructor");
                }

                var constructor = ctors[0];
                List<ParameterInfo> parameters = [.. constructor.GetParameters()];
                return constructor.Invoke([
                    .. sequence.Select( ( item, i ) =>
                        DeserializeUnknown( item, parameters[i].ParameterType, depth + 1 ) ),
                ]);
            }

            object DeserializeRecord(object value, Type type, int depth) {
                Type FindConcreteTypeForData(Type baseType, Dictionary<object, object> data) {
                    var dataKeys = data.Keys.Select(k => k.ToString()).ToHashSet();
                    List<Type> matches = [
                        .. baseType.Assembly.GetTypes()
                            .Where( t => !t.IsAbstract && !t.IsInterface && baseType.IsAssignableFrom( t ) )
                            .Where( type => dataKeys.IsSubsetOf( ShapeOf( type ).Names ) ),
                    ];
                    return matches.Count != 1
                        ? throw Fail(matches.Any()
                            ? $"{baseType.Name} matches several shapes: {matches.Select(t => t.Name).JoinToString()}"
                            : $"could not find concrete type for {baseType.Name}")
                        : matches[0];
                }

                if (value is not Dictionary<object, object> data) {
                    throw Fail($"Cannot load {Shape(value)} into {type.Name}");
                }

                if (type.IsInterface || type.IsAbstract) {
                    type = FindConcreteTypeForData(type, data);
                }

                TypeShape shape = ShapeOf(type);
                List<ConstructorInfo> ctors = shape.MatchingCtors;
                return ctors.Count switch {
                    1 => Construct(type, data, shape, ctors[0], depth),
                    0 => !type.IsValueType && type.GetConstructor(Type.EmptyTypes) is null
                        ? throw Fail($"{type.Name} needs one matching constructor")
                        : Fill(type, data, shape, depth),
                    _ => throw Fail($"{type.Name} needs one matching constructor"),
                };
            }

            object Construct(
                Type type,
                Dictionary<object, object> data,
                TypeShape shape,
                ConstructorInfo constructor,
                int depth) {
                List<ParameterInfo> constructorParams = [.. constructor.GetParameters()];

                List<object> constructorArgs = [
                    .. constructorParams.Select( p =>
                        p.Name is string paramName && data.TryGetValue(paramName, out object? given)
                            ? DeserializeUnknown(given, p.ParameterType, depth + 1)
                            : p.HasDefaultValue
                                ? p.DefaultValue!
                                : GetDefaultValue( p ) ),
                ];

                HashSet<string> known = [
                    .. constructorParams.Select( p => p.Name )
                        .Concat( shape.Writable.Select( m => m.Name ) ),
                ];
                RejectUnknownKeys(type, data, known);

                object instance = constructor.Invoke([.. constructorArgs]);
                FillWritable(instance, data, shape, depth);
                return instance;
            }

            object Fill(
                Type type,
                Dictionary<object, object> data,
                TypeShape shape,
                int depth) {
                RejectUnknownKeys(type, data, [.. shape.Writable.Select(m => m.Name)]);

                object instance = type.IsValueType
                    ? Activator.CreateInstance(type)!
                    : type.GetConstructor(Type.EmptyTypes)!.Invoke([]);

                FillWritable(instance, data, shape, depth);
                return instance;
            }

            object GetDefaultValue(ParameterInfo param) {
                Type type = param.ParameterType;
                TypeShape shape = ShapeOf(type);
                return type.IsValueType || shape.Map
                    ? Activator.CreateInstance(GetConcreteMapType(type))!
                    : shape.List
                        ? Activator.CreateInstance(GetConcreteListType(type, shape.ElementType))!
                        : type == typeof(string)
                          && new NullabilityInfoContext().Create(param).WriteState == NullabilityState.NotNull
                            ? ""
                            : null!;
            }

            TypeShape shape = ShapeOf(type);
            return depth > settings.MaxDepth
                ? throw Fail($"Nesting past {settings.MaxDepth} deep")
                : obj is null
                    ? Null(type)
                    : type == typeof(object)
                        ? obj
                        : IsBasicType(type)
                            ? DeserializeBasic(obj, type)
                            : shape.Map
                                ? DeserializeMap(obj, type, shape.KeyType, shape.ValueType, depth + 1)
                                : shape.List
                                    ? DeserializeList(obj, type, shape.ElementType, depth + 1)
                                    : obj is List<object> sequence
                                        ? DeserializeSequence(sequence, type, depth + 1)
                                        : DeserializeRecord(obj, type, depth + 1);
        }

        void RejectUnknownKeys(Type type, Dictionary<object, object> data, HashSet<string> known) {
            List<string> unknown = [
                .. data.Keys
                    .Select( k => k.ToString() )
                    .Where( name => !known.Contains( name ) ),
            ];
            if (unknown.Any()) {
                throw Fail($"{type.Name} has no such key: {unknown.JoinToString()}");
            }
        }

        void FillWritable(object instance, Dictionary<object, object> data, TypeShape shape, int depth) {
            foreach (Member member in shape.Writable) {
                if (data.TryGetValue(member.Name, out object? item)) {
                    member.Set!(instance, DeserializeUnknown(item, member.Type, depth + 1));
                }
            }
        }

        static readonly Dictionary<string, double> specialForms = new() {
            [".inf"] = double.PositiveInfinity,
            ["+.inf"] = double.PositiveInfinity,
            ["-.inf"] = double.NegativeInfinity,
            [".nan"] = double.NaN,
        };

        static readonly Dictionary<string, double> compatibleSpecialForms = new() {
            [".Inf"] = double.PositiveInfinity,
            [".INF"] = double.PositiveInfinity,
            ["+.Inf"] = double.PositiveInfinity,
            ["+.INF"] = double.PositiveInfinity,
            ["-.Inf"] = double.NegativeInfinity,
            ["-.INF"] = double.NegativeInfinity,
            [".NaN"] = double.NaN,
            [".NAN"] = double.NaN,
        };

        double? SpecialForm(string text) =>
            specialForms.TryGetValue(text, out double value)
                ? value
                : mode == ParsingMode.Compatible && compatibleSpecialForms.TryGetValue(text, out value)
                    ? value
                    : null;

        static readonly Dictionary<char, char> Escapes = new() {
            ['0'] = '\0',
            ['a'] = '\a',
            ['b'] = '\b',
            ['t'] = '\t',
            ['n'] = '\n',
            ['v'] = '\v',
            ['f'] = '\f',
            ['r'] = '\r',
            ['e'] = '\e',
            [' '] = ' ',
            ['"'] = '"',
            ['/'] = '/',
            ['\\'] = '\\',
            ['N'] = '\x85',
            ['_'] = '\xa0',
            ['L'] = '\u2028',
            ['P'] = '\u2029',
        };

        static bool HasBom(string yaml) =>
            yaml.StartsWith('\uFEFF');

        void BanIndentTab(ReadOnlySpan<char> raw) {
            for (int i = 0; i < raw.Length; i++) {
                if (raw[i] == '\t') {
                    throw Fail("Tabs are banned in indentation");
                }

                if (raw[i] != ' ') {
                    return;
                }
            }
        }

        string Shape(object value) =>
            value is null ? "nothing"
            : value is Dictionary<object, object> ? "a mapping"
            : value is List<object> ? "a list"
            : $"'{value}'";

        string ResolveScalar(string str) =>
            ResolveScalar(str.AsSpan(), str);

        string ResolveScalar(ReadOnlySpan<char> str, string? owned) {
            string Unescape(ReadOnlySpan<char> text) {
                int firstEscape = text.IndexOf('\\');
                if (firstEscape < 0) {
                    return text.ToString();
                }

                StringBuilder sb = new();
                sb.Append(text[..firstEscape]);
                for (int i = firstEscape; i < text.Length; i++) {
                    if (text[i] != '\\' || i + 1 >= text.Length) {
                        sb.Append(text[i]);
                        continue;
                    }

                    char esc = text[++i];
                    if (esc is 'x' or 'u' or 'U') {
                        int len = esc == 'x'
                            ? 2
                            : esc == 'u'
                                ? 4
                                : 8;
                        try {
                            sb.Append(char.ConvertFromUtf32(Convert.ToInt32(text.Slice(i + 1, len).ToString(), 16)));
                        } catch (Exception e) when (e is FormatException or ArgumentOutOfRangeException) {
                            throw Fail($"Bad escape: '\\{esc}'", e);
                        }
                        i += len;
                        continue;
                    }

                    if (!Escapes.TryGetValue(esc, out char plain)) {
                        throw Fail($"Unknown escape: '\\{esc}'");
                    }

                    sb.Append(plain);
                }

                return sb.ToString();
            }

            string SingleQuoted(ReadOnlySpan<char> text) =>
                text.Contains("''", StringComparison.Ordinal)
                    ? text.ToString().Replace("''", "'")
                    : text.ToString();

            return str.Length >= 2 && str[0] == '"'
                ? str[^1] == '"'
                    ? Unescape(str[1..^1])
                    : throw Fail($"Unterminated quote: '{str}'")
                : str.Length >= 2 && str[0] == '\''
                    ? str[^1] == '\''
                        ? SingleQuoted(str[1..^1])
                        : throw Fail($"Unterminated quote: '{str}'")
                    : owned ?? str.ToString();
        }

        string ResolveKey(string text) =>
            ResolveScalar(CheckScalar(text));

        string ResolveKey(ReadOnlySpan<char> text) {
            CheckScalar(text);
            return ResolveScalar(text, null);
        }

        Type GetConcreteListType(Type type, Type valueType) {
            if (!type.IsInterface && !type.IsAbstract) {
                return type;
            }

            if (type.IsGenericType) {
                Type genericDefinition = type.GetGenericTypeDefinition();

                if (genericDefinition == typeof(IEnumerable<>) ||
                     genericDefinition == typeof(ICollection<>) ||
                     genericDefinition == typeof(IList<>)) {
                    return typeof(List<>).MakeGenericType(valueType);
                }

                if (genericDefinition == typeof(ISet<>)) {
                    return typeof(HashSet<>).MakeGenericType(valueType);
                }
            }

            return typeof(System.Collections.IList).IsAssignableFrom(type)
                ? typeof(System.Collections.ArrayList)
                : throw NoConcrete(type);
        }

        Type GetConcreteMapType(Type type) {
            if (!type.IsInterface && !type.IsAbstract) {
                return type;
            }

            if (type.IsGenericType) {
                Type genericDefinition = type.GetGenericTypeDefinition();
                if (genericDefinition == typeof(IDictionary<,>) ||
                     genericDefinition == typeof(IReadOnlyDictionary<,>)) {
                    List<Type> args = [.. type.GetGenericArguments()];
                    return typeof(Dictionary<,>).MakeGenericType(args[0], args[1]);
                }
            }

            return typeof(System.Collections.IDictionary).IsAssignableFrom(type)
                ? typeof(System.Collections.Hashtable)
                : throw NoConcrete(type);
        }

        (object Value, int Taken) ContinuedValue(Lines lines, int i, int to, ReadOnlySpan<char> text, int here) {
            int j = i + 1;
            if (j < to) {
                Line next = lines.Items[j];
                if (!next.Blank && next.Indent > here && next.Colon < 0 && !lines.Content(next).StartsWith("- ")) {
                    (string joined, int taken) = WithContinued(lines, i, to, text, here);
                    return (ResolveValue(joined), taken);
                }
            }

            return (ResolveValue(text, null), 0);
        }

        (string Text, int Taken) WithContinued(Lines lines, int i, int to, ReadOnlySpan<char> text, int here) {
            StringBuilder? joined = null;
            int taken = 0;
            for (int j = i + 1; j < to; j++) {
                Line next = lines.Items[j];
                if (next.Blank || next.Indent <= here || next.Colon >= 0) {
                    break;
                }

                ReadOnlySpan<char> stripped = lines.Content(next);
                if (stripped.StartsWith("- ")) {
                    break;
                }

                joined ??= new StringBuilder(text.ToString());
                joined.Append(' ').Append(stripped);
                taken++;
            }

            return (joined is null ? text.ToString() : joined.ToString(), taken);
        }

        string ValidateYaml(string text) {
            string version = text["%YAML".Length..].Trim();
            List<string> parts = [.. version.Split('.')];
            return parts.Count == 2 && parts.All(p => p.Length > 0 && p.All(c => c is >= '0' and <= '9'))
                ? version
                : throw Fail($"Bad %YAML directive: '{text.Trim()}'");
        }

        string? RejectedName(ReadOnlySpan<char> text) =>
            text.Length == 0
                ? null
                : text[0] switch {
                    _ when rejectedIndicators.Contains(text[0]) => $"leading '{text[0]}'",
                    '{' or '}' => "leading '{' or '}'",
                    _ when IsDocumentMarker(text) => "document marker",
                    _ => null,
                };

        string CheckScalar(string text) {
            CheckScalar(text.AsSpan());
            return text;
        }

        void CheckScalar(ReadOnlySpan<char> text) {
            if (text.Length >= 2 && text[0] is not ('"' or '\'') && RejectedName(text) is { } reason) {
                throw Fail($"{reason} '{text}'");
            }
        }

        object ResolveValue(string text) =>
            ResolveValue(text.AsSpan(), text);

        object ResolveValue(ReadOnlySpan<char> text, string? owned) {
            if (text.Length >= 2 && (text[0] == '"' || text[0] == '\'')) {
                return ResolveScalar(text, owned);
            }

            CheckScalar(text);
            bool compatible = mode == ParsingMode.Compatible;
            return text is "null" or "~" || (compatible && text is "Null" or "NULL")
                ? null!
                : !compatible && text.Length == 4 &&
                  (text[0] is 'n' or 'N') && text.Equals("null", StringComparison.OrdinalIgnoreCase)
                    ? throw Fail($"non-lowercase null literal '{text}'")
                    : owned ?? text.ToString();
        }

        void AddEntry(Dictionary<object, object> resultDict, object key, object val) {
            if (!resultDict.TryAdd(key, val)) {
                throw Fail($"Mapping has duplicate key: '{key}'");
            }
        }

        static bool HasKeyColon(ReadOnlySpan<char> line) =>
            KeyColon(line) >= 0;

        static int KeyColon(ReadOnlySpan<char> line) {
            if (!line.Contains(':')) {
                return -1;
            }

            QuoteWalker walker = new();
            for (int i = 0; i < line.Length; i++) {
                char c = line[i];
                if (walker.Plain(c) && c == ':' &&
                    (i + 1 == line.Length || char.IsWhiteSpace(line[i + 1]))) {
                    return i;
                }
            }

            return -1;
        }

        static int SkipFlowSpace(string text, int at) {
            while (at < text.Length && (text[at] == ' ' || text[at] == '\n')) {
                at++;
            }

            return at;
        }

        static string FoldFlowBreaks(string part) {
            string trimmed = part.Trim();
            return !trimmed.Contains('\n')
                ? trimmed
                : trimmed.Split('\n').Select(bit => bit.Trim()).JoinToString(" ");
        }

        (string Text, int Next, char Stop) ScanFlowScalar(string text, int at) {
            QuoteWalker walker = new();
            int i = at;
            while (i < text.Length) {
                char c = text[i];
                if (!walker.Plain(c)) {
                    i++;
                    continue;
                }

                if (c is ',' or ']' or '}') {
                    return (FoldFlowBreaks(text[at..i]), i, c);
                }

                if (c is '[' or '{') {
                    throw Unbalanced(text);
                }

                if (c == ':' && i + 1 < text.Length && flowSeparators.Contains(text[i + 1])) {
                    return (text[at..i], i, c);
                }

                i++;
            }

            return (FoldFlowBreaks(text[at..i]), i, '\0');
        }

        (object Node, int Next) ParseFlowNode(string text, int at, int depth) {
            (object Node, int Next) ParseFlowValue(string text, int at) {
                (string part, int next, char stop) = ScanFlowScalar(text, at);
                string trimmed = part.Trim();
                return trimmed.Length == 0
                    ? throw EmptyFlowEntry(text)
                    : stop == ':'
                        ? throw Fail($"Colon in flow scalar: '{text}'")
                        : (ResolveValue(trimmed), next);
            }

            at = SkipFlowSpace(text, at);

            return depth > settings.MaxDepth
                ? throw Fail($"Nesting past {settings.MaxDepth} deep")
                : at >= text.Length
                    ? throw Unbalanced(text)
                    : text[at] == '['
                        ? ParseFlowList(text, at, depth)
                        : text[at] == '{'
                            ? ParseFlowMap(text, at, depth)
                            : ParseFlowValue(text, at);
        }

        (object Node, int Next) ParseFlowValueNode(string text, int at, int depth) {
            at = SkipFlowSpace(text, at);

            return at >= text.Length || text[at] == ',' || text[at] == ']' || text[at] == '}'
                ? throw EmptyFlowEntry(text)
                : ParseFlowNode(text, at, depth);
        }

        object ParseFlowWhole(string text, int depth) {
            (object node, int next) = text[0] == '['
                ? ParseFlowList(text, 0, depth)
                : ParseFlowMap(text, 0, depth);
            return next != text.Length
                ? throw Fail($"Trailing flow content: '{text}'")
                : node;
        }

        (object Node, int Next) ParseFlowList(string text, int at, int depth) {
            List<object> items = [];
            at++;
            while (true) {
                at = SkipFlowSpace(text, at);

                if (at >= text.Length) {
                    throw UnbalancedIn("list", text);
                }

                if (text[at] == ']') {
                    return (items, at + 1);
                }

                if (text[at] == '[' || text[at] == '{') {
                    (object nested, int after) = ParseFlowNode(text, at, depth + 1);
                    items.Add(nested);
                    at = after;
                } else {
                    (string part, int next, char stop) = ScanFlowScalar(text, at);
                    string trimmed = part.Trim();
                    if (trimmed.Length == 0) {
                        throw EmptyFlowEntry(text);
                    }

                    if (stop == ':') {
                        (object val, int after) = ParseFlowValueNode(text, next + 1, depth + 1);
                        Dictionary<object, object> pair = [];
                        AddEntry(pair, ResolveKey(trimmed), val);
                        items.Add(pair);
                        at = after;
                    } else {
                        items.Add(ResolveValue(trimmed));
                        at = next;
                    }
                }

                at = SkipFlowSpace(text, at);

                if (at >= text.Length) {
                    throw UnbalancedIn("list", text);
                }

                if (text[at] == ',') {
                    at++;
                    continue;
                }

                return text[at] == ']'
                    ? (items, at + 1)
                    : throw UnbalancedIn("list", text);
            }
        }

        (object Node, int Next) ParseFlowMap(string text, int at, int depth) {
            Dictionary<object, object> map = [];
            at++;
            while (true) {
                at = SkipFlowSpace(text, at);

                if (at >= text.Length) {
                    throw UnbalancedIn("mapping", text);
                }

                if (text[at] == '}') {
                    return (map, at + 1);
                }

                if (text[at] == '[' || text[at] == '{') {
                    (object keyNode, int afterKey) = ParseFlowNode(text, at, depth + 1);
                    int ka = afterKey;
                    while (ka < text.Length && text[ka] == ' ') {
                        ka++;
                    }

                    bool sepColon = ka < text.Length && text[ka] == ':' &&
                                    (ka + 1 >= text.Length || flowSeparators.Contains(text[ka + 1]));
                    if (!sepColon) {
                        throw Fail($"complex flow key '{text}'");
                    }

                    (object val, int after) = ParseFlowValueNode(text, ka + 1, depth + 1);
                    AddEntry(map, keyNode, val);
                    at = after;
                } else {
                    (string part, int next, char stop) = ScanFlowScalar(text, at);
                    string trimmed = part.Trim();
                    if (trimmed.Length == 0) {
                        throw EmptyFlowEntry(text);
                    }

                    if (stop == ':') {
                        (object val, int after) = ParseFlowValueNode(text, next + 1, depth + 1);
                        AddEntry(map, ResolveKey(trimmed), val);
                        at = after;
                    } else if (stop == ']') {
                        throw UnbalancedIn("mapping", text);
                    } else {
                        string key = ResolveKey(trimmed);
                        throw Fail($"Key '{key}' has no value");
                    }
                }

                at = SkipFlowSpace(text, at);

                if (at >= text.Length) {
                    throw UnbalancedIn("mapping", text);
                }

                if (text[at] == ',') {
                    at++;
                    continue;
                }

                return text[at] == '}'
                    ? (map, at + 1)
                    : throw UnbalancedIn("mapping", text);
            }
        }

        static bool IsDocumentEnd(ReadOnlySpan<char> text) =>
            text.Trim().SequenceEqual("...".AsSpan());

        (object, int processedCount) PreProcess(Lines lines, int from, int to, int depth, Line? first, bool flatSequence = false) {
            if (depth > settings.MaxDepth) {
                throw Fail($"Nesting past {settings.MaxDepth} deep");
            }

            int need = depth * settings.IndentSize;
            bool compatible = mode == ParsingMode.Compatible;

            Dictionary<object, object>? resultDict = null;
            List<object>? resultList = null;

            for (int i = from; i < to; i++) {
                Line entry = i == from && first is { } head ? head : lines.Items[i];
                if (entry.Blank) {
                    continue;
                }

                line = i + 1;
                if (compatible && entry.TabIndent) {
                    throw Fail("Tabs are banned in indentation");
                }

                int here = entry.Indent;
                if (here < need) {
                    return Finish(i - from);
                }

                ReadOnlySpan<char> text = lines.Content(entry);
                if (compatible && IsDocumentEnd(text)) {
                    continue;
                }

                if (here == need && text[0] == '-') {
                    if (text.Length == 1 || text[1..].IsWhiteSpace()) {
                        (object res, int processedCount) =
                            PreProcess(lines, i + 1, to, depth + 1, null);
                        if (IsEmpty(res)) {
                            throw Fail("List item has no value");
                        }

                        (resultList ??= []).Add(res);
                        i += processedCount;
                        continue;
                    }

                    if (text[1] == ' ') {
                        int lead = text.Length - text[2..].TrimStart().Length;
                        ReadOnlySpan<char> item = text[lead..].TrimEnd();
                        if (item[0] == '[' || item[0] == '{') {
                            (resultList ??= []).Add(ParseFlowWhole(item.ToString(), depth + 1));
                            continue;
                        }

                        if (entry.Colon >= 0) {
                            Line nested = entry with {
                                Start = entry.Start + lead,
                                Indent = need + settings.IndentSize,
                            };
                            (object res, int processedCount) =
                                PreProcess(lines, i, to, depth + 1, nested);
                            (resultList ??= []).Add(res);
                            i += processedCount - 1;
                        } else {
                            (object value, int taken) = ContinuedValue(lines, i, to, item, here);
                            (resultList ??= []).Add(value);
                            i += taken;
                        }

                        continue;
                    }
                }

                if (flatSequence) {
                    return (resultList ??= [], i - from);
                }

                if (entry.Colon < 0) {
                    string stripped = text.ToString();
                    if (IsDocumentMarker(stripped) || stripped.StartsWith('%') || stripped == "?" ||
                         stripped.StartsWith("? ")) {
                        throw Fail($"{RejectedName(stripped)!} '{text.Trim()}'");
                    }

                    throw Fail($"Line has no colon: '{text.Trim()}'");
                }

                int at = entry.Colon - entry.Start;
                string key = ResolveScalar(text[..at].Trim(), null);
                ReadOnlySpan<char> rest = text[(at + 1)..].Trim();
                if (rest.Length == 0) {
                    (object res, int processedCount) =
                        PreProcess(lines, i + 1, to, depth + 1, null);
                    if (IsEmpty(res) && mode == ParsingMode.Compatible) {
                        (object sequence, int sequenceCount) =
                            PreProcess(lines, i + 1, to, depth, null, true);
                        if (!IsEmpty(sequence)) {
                            res = sequence;
                            processedCount = sequenceCount;
                        }
                    }

                    if (IsEmpty(res)) {
                        if (mode != ParsingMode.Compatible) {
                            throw Fail($"Key '{key}' has no value");
                        }

                        AddEntry(resultDict ??= [], key, null!);
                        continue;
                    }

                    AddEntry(resultDict ??= [], key, res);
                    i += processedCount;
                } else {
                    switch (rest[0]) {
                        case '|':
                        case '>': {
                                var header = BlockHeader(rest.ToString());
                                if (header == null) {
                                    throw Fail($"Bad block header: '{rest}'");
                                }

                                (char style, char chomp) = header.Value;
                                (List<string> content, int taken) = BlockLines(lines, i + 1, to, here);
                                string block = "";
                                if (content.Any()) {
                                    int trailing = taken - content.Count;
                                    string flat = style == '>'
                                        ? FoldLines(content)
                                        : $"{content.JoinToString("\n").TrimEnd('\n')}\n";
                                    block = chomp == '-'
                                        ? flat.TrimEnd('\n')
                                        : chomp == '+'
                                            ? flat + new string('\n', trailing)
                                            : flat;
                                }

                                AddEntry(resultDict ??= [], key, block);
                                i += taken;
                                break;
                            }
                        case '[':
                        case '{':
                            AddEntry(resultDict ??= [], key, ParseFlowWhole(rest.ToString(), depth + 1));
                            break;
                        default: {
                                (object value, int taken) = ContinuedValue(lines, i, to, rest, here);
                                AddEntry(resultDict ??= [], key, value);
                                i += taken;
                                break;
                            }
                    }
                }
            }

            return Finish(to - from);

            (char Style, char Chomp)? BlockHeader(string rest) {
                if (rest.Length == 0 || (rest[0] != '|' && rest[0] != '>')) {
                    return null;
                }

                char style = rest[0];
                char chomp = ' ';
                string flags = rest[1..];
                if (flags.Length > 0 && flags[0] >= '1' && flags[0] <= '9') {
                    flags = flags[1..];
                }

                if (flags.Length > 0 && (flags[0] == '-' || flags[0] == '+')) {
                    chomp = flags[0];
                    flags = flags[1..];
                }

                if (flags.Length > 0 && flags[0] >= '1' && flags[0] <= '9') {
                    flags = flags[1..];
                }

                return flags.Length == 0 ? (style, chomp) : null;
            }

            string FoldLines(List<string> content) {
                List<string> paragraph = [];
                List<string> paragraphs = [];
                content.ForEach(line => {
                    if (line.Length == 0) {
                        paragraphs.Add(paragraph.JoinToString(" "));
                        paragraph = [];
                    } else if (line[0] == ' ') {
                        paragraphs.Add(paragraph.JoinToString(" "));
                        paragraph = [];
                        paragraphs.Add(line);
                    } else {
                        paragraph.Add(line);
                    }
                });
                paragraphs.Add(paragraph.JoinToString(" "));
                return $"{paragraphs.Where(p => p.Length > 0).JoinToString("\n")}\n";
            }

            (List<string> Content, int Taken) BlockLines(Lines lines, int start, int to, int keyIndent) {
                List<string> content = [];
                List<string> gap = [];
                int take = -1;
                int at = start;
                for (; at < to; at++) {
                    string line = lines.Raw(at).ToString();
                    if (string.IsNullOrWhiteSpace(line)) {
                        gap.Add("");
                        continue;
                    }

                    int indent = IndentOf(line);
                    if (indent <= keyIndent) {
                        break;
                    }

                    if (take < 0) {
                        take = indent;
                    }

                    content.AddRange(gap);
                    gap.Clear();
                    content.Add(line[Math.Min(take, line.Length)..]);
                }

                return (content, at - start);
            }

            (object, int processedCount) Finish(int count) =>
                resultDict is not null && resultList is not null
                    ? throw Fail("Block has both a mapping and a list")
                    : resultDict is not null
                        ? (resultDict, count)
                        : (resultList ?? [], count);
        }

        ReadOnlySpan<char> CleanLine(ReadOnlySpan<char> line) {
            QuoteWalker walker = new();
            for (int j = 0; j < line.Length; j++) {
                if (!walker.Plain(line[j]) || line[j] != '#' || (j > 0 && !char.IsWhiteSpace(line[j - 1]))) {
                    continue;
                }

                return line[..j].TrimEnd();
            }

            return line;
        }
    }

    static readonly List<Type> basicNonPrimitive =
        [typeof(decimal), typeof(float), typeof(string), typeof(DateTime), typeof(DateTimeOffset)];

    static readonly ConditionalWeakTable<Type, TypeShape> shapes = new();
    static readonly MethodInfo genericMapWriterMethod = typeof(Serializer).GetMethod(
        "WriteGenericMap",
        BindingFlags.Static | BindingFlags.NonPublic)!;

    static TypeShape ShapeOf(Type type) =>
        shapes.GetValue(type, static shape => new TypeShape(shape));

    static GenericMapWriter CreateGenericMapWriter(Type keyType, Type valueType) =>
        (GenericMapWriter)genericMapWriterMethod.MakeGenericMethod(keyType, valueType)
            .CreateDelegate(typeof(GenericMapWriter));

    static bool IsBasicType(Type type) =>
        ShapeOf(type).Basic;

    static bool IsDocumentMarker(ReadOnlySpan<char> text) =>
        text.SequenceEqual("---") || text.SequenceEqual("...") || text.StartsWith("--- ") || text.StartsWith("... ");

    static Type UnwrapNullable(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
            ? type.GetGenericArguments()[0]
            : type;

    static bool IsMap(Type type) =>
        ShapeOf(type).Map;

    static bool IsList(Type type) =>
        ShapeOf(type).List;

    static readonly HashSet<string> typedWords = new(StringComparer.OrdinalIgnoreCase) {
        "~", "y", "n", "no", "on", "yes", "off", "null", "true", "false",
    };

    static readonly Dictionary<string, bool> strictBools = new() {
        ["true"] = true,
        ["false"] = false,
    };

    static readonly Dictionary<string, bool> compatibleBools = new() {
        ["true"] = true,
        ["True"] = true,
        ["TRUE"] = true,
        ["false"] = false,
        ["False"] = false,
        ["FALSE"] = false,
    };

    static readonly List<string> nullForms = ["null", "~"];

    static readonly List<string> compatibleNullForms = ["Null", "NULL"];

    static readonly SearchValues<char> rejectedIndicators = SearchValues.Create("&*!?%");

    static Func<object, object?> PropertyGetter(Type type, PropertyInfo property, bool compile) {
        if (!compile || !RuntimeFeature.IsDynamicCodeSupported || type.IsValueType ||
            property.GetMethod?.IsPublic != true || property.GetIndexParameters().Length > 0 ||
            property.PropertyType.IsByRef || property.PropertyType.IsByRefLike ||
            property.PropertyType.IsPointer) {
            return property.GetValue;
        }

        ParameterExpression //
            instance = Expression.Parameter(typeof(object)),
            error = Expression.Parameter(typeof(Exception));
        Expression //
            target = Expression.Convert(instance, type),
            value = Expression.Convert(Expression.Property(target, property), typeof(object)),
            wrapped = Expression.TryCatch(
                value,
                Expression.Catch(error, Expression.Throw(
                    Expression.New(typeof(TargetInvocationException).GetConstructor([typeof(Exception)])!, error),
                    typeof(object))));
        return Expression.Lambda<Func<object, object?>>(wrapped, instance).Compile();
    }

    static Func<object, object?> FieldGetter(Type type, FieldInfo field, bool compile) {
        if (!compile || !RuntimeFeature.IsDynamicCodeSupported || type.IsValueType ||
            field.FieldType.IsByRef || field.FieldType.IsByRefLike || field.FieldType.IsPointer) {
            return field.GetValue;
        }

        ParameterExpression instance = Expression.Parameter(typeof(object));
        Expression //
            target = Expression.Convert(instance, type),
            value = Expression.Convert(Expression.Field(target, field), typeof(object));
        return Expression.Lambda<Func<object, object?>>(value, instance).Compile();
    }

    class Member {
        public string Name = "";
        public Type Type = typeof(object);
        public Func<object, object?>? Get;
        public Action<object, object?>? Set;
    }

    class TypeShape {
        public bool Basic;
        public bool Map;
        public bool GenericMap;
        public Type KeyType = typeof(object);
        public Type ValueType = typeof(object);
        public GenericMapWriter? WriteGenericMap;
        public bool List;
        public Type ElementType = typeof(object);

        public TypeShape(Type type) {
            void AnalyzeMap(Type type) {
                if (type.IsGenericType) {
                    Type genericDefinition = type.GetGenericTypeDefinition();
                    if (genericDefinition == typeof(IDictionary<,>) ||
                         genericDefinition == typeof(IReadOnlyDictionary<,>)) {
                        KeyType = type.GetGenericArguments()[0];
                        ValueType = type.GetGenericArguments()[1];
                        Map = true;
                        GenericMap = genericDefinition == typeof(IDictionary<,>);
                        return;
                    }
                }

                Type dictType = type.GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

                if (dictType == null) {
                    Map = typeof(System.Collections.IDictionary).IsAssignableFrom(type);
                    return;
                }

                List<Type> args = [.. dictType.GetGenericArguments()];
                KeyType = args[0];
                ValueType = args[1];
                Map = true;
                GenericMap = true;
            }

            void AnalyzeList(Type type) {
                if (type.IsArray) {
                    ElementType = type.GetElementType()!;
                    List = true;
                    return;
                }

                if (!type.IsGenericType) {
                    List = typeof(System.Collections.IList).IsAssignableFrom(type);
                    return;
                }

                if (type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                    ElementType = type.GetGenericArguments()[0];
                    List = true;
                    return;
                }

                Type inherits = type.GetInterfaces().FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (inherits == null) {
                    return;
                }

                ElementType = inherits.GetGenericArguments()[0];
                List = true;
            }

            void AnalyzeProperties(Type type) {
                static bool Matches(ConstructorInfo constructor, HashSet<string> names) =>
                    constructor.GetParameters().Length > 0 &&
                    constructor.GetParameters().All(p => p.Name is string name && names.Contains(name));

                static List<Member> Members(Type type, bool compileGetters) {
                    List<Member> members = [];
                    foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                        members.Add(new Member {
                            Name = property.Name,
                            Type = property.PropertyType,
                            Get = property.CanRead ? PropertyGetter(type, property, compileGetters) : null,
                            Set = property.CanWrite ? (Action<object, object?>)property.SetValue : null,
                        });
                    }

                    foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                        members.Add(new Member {
                            Name = field.Name,
                            Type = field.FieldType,
                            Get = FieldGetter(type, field, compileGetters),
                            Set = field.IsInitOnly ? null : (Action<object, object?>)field.SetValue,
                        });
                    }

                    return members;
                }

                List<Member> members = [.. Members(type, !Basic && !Map && !List)];
                Names = [.. members.Select(m => m.Name)];
                MatchingCtors = [.. type.GetConstructors().Where(c => Matches(c, Names))];
                HashSet<string> ctorNames = MatchingCtors.Count == 1
                    ? [.. MatchingCtors[0].GetParameters().Select(p => p.Name ?? "")]
                    : [];
                Writable = [.. members.Where(m => m.Set is not null)];
                Readable = [.. members.Where(m => m.Get is not null
                    && (ctorNames.Contains(m.Name) || Writable.Contains(m))),
                ];
            }

            Type plain = UnwrapNullable(type);
            Basic = plain.IsPrimitive || plain.IsEnum || basicNonPrimitive.Contains(plain);
            AnalyzeMap(type);
            AnalyzeList(type);
            if (GenericMap) {
                WriteGenericMap = CreateGenericMapWriter(KeyType, ValueType);
            }

            AnalyzeProperties(type);
        }

        public List<Member> Readable = [];
        public List<Member> Writable = [];
        public HashSet<string> Names = [];
        public List<ConstructorInfo> MatchingCtors = [];

    }

}

file static class NanoYamlExtensions {
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action) {
        foreach (T item in enumerable) {
            action(item);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action) {
        int i = 0;
        foreach (T item in enumerable) {
            action(item, i);
            i++;
        }
    }

    public static string JoinToString<T>(this IEnumerable<T> items, string separator = ", ") {
        StringBuilder sb = new();
        bool first = true;
        foreach (T item in items) {
            if (!first) {
                sb.Append(separator);
            }

            sb.Append(item);
            first = false;
        }

        return sb.ToString();
    }
}
