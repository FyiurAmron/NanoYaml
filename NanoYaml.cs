using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Vaxquis.NanoYaml;

/// <summary>
/// Opts a non-public field or property into YAML serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SerializeAttribute : Attribute {
}

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

    /// <inheritdoc cref="Deserializer.Deserialize{T}(ReadOnlySpan{char})"/>
    public static T Deserialize<T>(ReadOnlySpan<char> yaml) =>
        Deserializer.Default.Deserialize<T>(yaml);

    /// <inheritdoc cref="Deserializer.Deserialize{T}(ReadOnlySpan{byte})"/>
    public static T Deserialize<T>(ReadOnlySpan<byte> utf8) =>
        Deserializer.Default.Deserialize<T>(utf8);

    /// <inheritdoc cref="Deserializer.Deserialize{T}(TextReader)"/>
    public static T Deserialize<T>(TextReader reader) =>
        Deserializer.Default.Deserialize<T>(reader);

    /// <inheritdoc cref="Deserializer.Deserialize{T}(Stream)"/>
    public static T Deserialize<T>(Stream stream) =>
        Deserializer.Default.Deserialize<T>(stream);

    /// <inheritdoc cref="Deserializer.ParseRaw(ReadOnlySpan{char})"/>
    public static object? ParseRaw(ReadOnlySpan<char> yaml) =>
        Deserializer.Default.ParseRaw(yaml);

    /// <inheritdoc cref="Deserializer.ParseRaw(ReadOnlySpan{byte})"/>
    public static object? ParseRaw(ReadOnlySpan<byte> utf8) =>
        Deserializer.Default.ParseRaw(utf8);

    /// <inheritdoc cref="Deserializer.ParseRaw(TextReader)"/>
    public static object? ParseRaw(TextReader reader) =>
        Deserializer.Default.ParseRaw(reader);

    /// <inheritdoc cref="Deserializer.ParseRaw(Stream)"/>
    public static object? ParseRaw(Stream stream) =>
        Deserializer.Default.ParseRaw(stream);

    /// <summary>Serializes a .NET value as a NanoYaml YAML document.</summary>
    /// <param name="obj">The value to serialize, or null.</param>
    /// <returns>A YAML document containing the NanoYaml profile tag.</returns>
    /// <exception cref="NanoYamlException">
    /// The value cannot be represented by the serializer.
    /// </exception>
    public static string Serialize(object? obj) =>
        Serializer.Default.Serialize(obj);

    /// <inheritdoc cref="Serializer.Serialize(object, TextWriter)"/>
    public static void Serialize(object? obj, TextWriter writer) =>
        Serializer.Default.Serialize(obj, writer);

    /// <inheritdoc cref="Serializer.Serialize(object, Stream)"/>
    public static void Serialize(object? obj, Stream stream) =>
        Serializer.Default.Serialize(obj, stream);

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

    delegate void GenericListFiller(
        object target,
        List<object> data,
        Func<object, object?> convert);

    delegate void GenericMapFiller(
        object target,
        Dictionary<object, object> data,
        Func<object, object?> convertKey,
        Func<object, object?> convertValue);

    delegate int GenericMapCounter(object target, uint cap);

    delegate List<object> GenericMapValues(object target);

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
                bool firstLine = true;
                foreach (Member member in shape.Emitted) {
                    StartEntry(sb, indent, firstLineIndented, ref firstLine);
                    sb.Append(member.Key ??= Rendered(member.Name)).Append(':');
                    if (member.WriteScalar is { } scalar) {
                        sb.Append(' ');
                        scalar(sb, obj);
                    } else {
                        EntryValue(sb, member.Get!(obj), indent);
                    }
                }
            }
        }

        static string Rendered(string text) {
            StringBuilder sb = new();
            WriteText(sb, text);
            return sb.ToString();
        }

        void StartEntry(StringBuilder sb, int indent, bool firstLineIndented, ref bool firstLine) {
            if (!firstLine) {
                sb.AppendLine();
            }

            if (!firstLine || !firstLineIndented) {
                Pad(sb, indent);
            }

            firstLine = false;
        }

        void EntryValue(StringBuilder sb, object val, int indent) {
            if (!TryInline(sb, val, " ", out TypeShape shape)) {
                sb.AppendLine();
                SerializeBlock(sb, val, shape, indent + 1, false);
            }
        }

        static Action<StringBuilder, object> TypedScalar<TOwner, TValue>(MethodInfo getter) {
            Func<TOwner, TValue> typed = getter.CreateDelegate<Func<TOwner, TValue>>();
            return (sb, instance) => {
                TValue value;
                try {
                    value = typed((TOwner)instance);
                } catch (Exception e) {
                    throw new TargetInvocationException(e);
                }

                WriteScalar(sb, value);
            };
        }

        static void WriteScalar<T>(StringBuilder sb, T value) =>
            _ = value switch {
                bool flag => Literal(sb, flag ? "true" : "false"),
                int number => Invariant(sb, number),
                long number => Invariant(sb, number),
                double number => Floating(sb, number),
                float number => Floating(sb, number),
                decimal number => Invariant(sb, number),
                short number => Invariant(sb, number),
                byte number => Invariant(sb, number),
                uint number => Invariant(sb, number),
                ulong number => Invariant(sb, number),
                ushort number => Invariant(sb, number),
                sbyte number => Invariant(sb, number),
                _ => throw new InvalidOperationException($"not a scalar: {typeof(T)}"),
            };

        /// <summary>Serializes a .NET value as a YAML document.</summary>
        /// <param name="obj">The value to serialize, or null.</param>
        /// <returns>
        /// A YAML document containing the NanoYaml profile tag.
        /// </returns>
        /// <exception cref="NanoYamlException">
        /// The value cannot be represented by the serializer.
        /// </exception>
        public string Serialize(object? obj) =>
            Render(obj, static sb => sb.ToString());

        /// <summary>Writes a .NET value as a YAML document.</summary>
        /// <param name="obj">The value to serialize, or null.</param>
        /// <param name="writer">
        /// The writer that receives the document; it is not flushed or closed.
        /// </param>
        /// <exception cref="NanoYamlException">
        /// The value cannot be represented by the serializer.
        /// </exception>
        public void Serialize(object? obj, TextWriter writer) {
            ArgumentNullException.ThrowIfNull(writer);
            Render(obj, sb => {
                foreach (ReadOnlyMemory<char> chunk in sb.GetChunks()) {
                    writer.Write(chunk.Span);
                }

                return true;
            });
        }

        /// <summary>Writes a .NET value as a UTF-8 YAML document.</summary>
        /// <param name="obj">The value to serialize, or null.</param>
        /// <param name="stream">
        /// The stream that receives the document, without a byte order mark;
        /// it is not flushed or closed.
        /// </param>
        /// <exception cref="NanoYamlException">
        /// The value cannot be represented by the serializer.
        /// </exception>
        public void Serialize(object? obj, Stream stream) {
            ArgumentNullException.ThrowIfNull(stream);
            Render(obj, sb => {
                Encoder encoder = utf8.GetEncoder();
                byte[] bytes = ArrayPool<byte>.Shared.Rent(StreamBufferSize);
                try {
                    foreach (ReadOnlyMemory<char> chunk in sb.GetChunks()) {
                        ReadOnlySpan<char> chars = chunk.Span;
                        bool completed = false;
                        while (!completed) {
                            encoder.Convert(chars, bytes, false, out int used, out int written, out completed);
                            stream.Write(bytes, 0, written);
                            chars = chars[used..];
                        }
                    }

                    encoder.Convert([], bytes, true, out _, out int tail, out _);
                    stream.Write(bytes, 0, tail);
                } finally {
                    ArrayPool<byte>.Shared.Return(bytes);
                }

                return true;
            });
        }

        const int StreamBufferSize = 16 * 1024;

        static readonly UTF8Encoding utf8 = new(false, true);

        TResult Render<TResult>(object? obj, Func<StringBuilder, TResult> finish) {
            StringBuilder sb = cachedBuilder ?? new();
            cachedBuilder = null;
            sb.Clear();
            try {
                sb.Append(ProfileTag).Append("\n---\n");
                SerializeUnknown(sb, obj);
                return finish(sb);
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

            if (TryWriteNumber(sb, obj)) {
                return;
            }

            Type type = obj.GetType();
            bool asText = obj is char || type.IsEnum || (asKey && !IsBasicType(type));
            if (!asText) {
                sb.Append(obj is bool b
                    ? (b ? "true" : "false")
                    : Convert.ToString(obj, CultureInfo.InvariantCulture));
                return;
            }

            WriteText(sb, obj.ToString()!);
        }

        static bool TryWriteNumber(StringBuilder sb, object obj) =>
            obj switch {
                int number => Invariant(sb, number),
                long number => Invariant(sb, number),
                double number => Floating(sb, number),
                float number => Floating(sb, number),
                decimal number => Invariant(sb, number),
                short number => Invariant(sb, number),
                byte number => Invariant(sb, number),
                uint number => Invariant(sb, number),
                ulong number => Invariant(sb, number),
                ushort number => Invariant(sb, number),
                sbyte number => Invariant(sb, number),
                _ => false,
            };

        static bool Invariant<T>(StringBuilder sb, T number) where T : ISpanFormattable {
            sb.Append(CultureInfo.InvariantCulture, $"{number}");
            return true;
        }

        const double MaxIntegral = 1e15;

        static bool Floating<T>(StringBuilder sb, T number) where T : INumberBase<T>, ISpanFormattable =>
            T.IsNaN(number)
                ? Literal(sb, ".nan")
                : T.IsInfinity(number)
                    ? Literal(sb, T.IsPositive(number) ? ".inf" : "-.inf")
                    : number is double value && Integral(value)
                        ? Invariant(sb, (long)value)
                        : Invariant(sb, number);

        static bool Integral(double value) =>
            Math.Abs(value) < MaxIntegral && value == Math.Floor(value) && !(value == 0 && double.IsNegative(value));

        static bool Literal(StringBuilder sb, string text) {
            sb.Append(text);
            return true;
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

            StartEntry(sb, indent, firstLineIndented, ref firstLine);
            WriteKey(sb, key);
            sb.Append(':');
            EntryValue(sb, val, indent);
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
            if (obj is Dictionary<object, object> raw) {
                SerializeMap<object, object>(sb, raw, indent, firstLineIndented);
                return;
            }

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
            if (obj is Dictionary<TKey, TValue> scalars && scalarTypes.Contains(typeof(TValue))) {
                foreach (KeyValuePair<TKey, TValue> entry in scalars) {
                    StartEntry(sb, indent, firstLineIndented, ref firstLine);
                    WriteKey(sb, entry.Key!);
                    sb.Append(": ");
                    WriteScalar(sb, entry.Value);
                }

                return;
            }

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
                bool first = true;
                sb.Append('{');
                foreach (Member member in shape.Emitted) {
                    FlowEntry(sb, member.Name, member.Get!(obj), ref first);
                }

                sb.Append('}');
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
            if (obj is Dictionary<object, object> raw) {
                SerializeFlowMap<object, object>(sb, raw);
            } else if (shape.WriteGenericMap is { } writer) {
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
                   limit == 0 && CountChildren(node, isMap, 0) == 0 ||
                   limit > 0 &&
                   CountChildren(node, isMap, limit) <= limit &&
                   ChildrenFlow(node, isMap);
        }

        bool ChildFlows(object child) =>
            !IsCollection(child, out _) || CanFlow(child, ShapeOf(child.GetType()));

        bool ChildrenFlow(object node, bool isMap) {
            if (node is Dictionary<object, object> raw) {
                foreach (object child in raw.Values) {
                    if (!ChildFlows(child)) {
                        return false;
                    }
                }

                return true;
            }

            if (node is List<object> list) {
                foreach (object child in list) {
                    if (!ChildFlows(child)) {
                        return false;
                    }
                }

                return true;
            }

            return ChildValues(node, isMap).All(ChildFlows);
        }

        ulong CountChildren(object node, bool isMap, uint cap) {
            if (node is Dictionary<object, object> or List<object>) {
                return CountRaw((ICollection)node, cap);
            }

            int KeyCount(IDictionary dict) {
                int count = 0;
                foreach (object key in dict.Keys) {
                    if (key != null && ++count > cap) {
                        break;
                    }
                }

                return count;
            }

            int ChildMapCount(TypeShape shape) =>
                shape.CountGenericMap is { } counter
                    ? counter(node, cap)
                    : shape.Emitted.Count;

            int ChildCount() =>
                !isMap
                    ? node is ICollection collection
                        ? collection.Count
                        : ((IEnumerable)node).Cast<object>().Count()
                    : node is IDictionary dict
                        ? KeyCount(dict)
                        : ChildMapCount(ShapeOf(node.GetType()));

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

        ulong CountRaw(ICollection node, uint cap) {
            ulong total = (ulong)node.Count;
            if (total > cap) {
                return total;
            }

            IEnumerable<object> children = node is Dictionary<object, object> raw ? raw.Values : (List<object>)node;
            foreach (object child in children) {
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

                TypeShape shape = ShapeOf(node.GetType());
                return shape.ValuesGenericMap is { } gather
                    ? gather(node)
                    : shape.Emitted.Select(member => member.Get!(node));
            }

            return ((IEnumerable)node).Cast<object>();
        }

        static readonly SearchValues<char> leadingIndicators = SearchValues.Create("-?&*!%|>@`");
        static readonly SearchValues<char> specialCharacters = SearchValues.Create(":\"\'\n[]{}#,\\\r\t\0\a\b\v\f\e\x85\xa0\u2028\u2029");
        static readonly SearchValues<char> escapeCharacters = SearchValues.Create("\\\"\u00A0\u2028\u2029\uFEFF\uFFFE\uFFFF");
        const string //
            AsciiControls = "\0\x01\x02\x03\x04\x05\x06\a\b\t\n\v\f\r\x0E\x0F\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1A\e\x1C\x1D\x1E\x1F\x7F",
            PlainAscii = " !$%&()*+-./0123456789;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ^_`abcdefghijklmnopqrstuvwxyz|~";
        static readonly SearchValues<char> //
            plainAscii = SearchValues.Create(PlainAscii),
            asciiEscapeCharacters = SearchValues.Create($"\\\"{AsciiControls}");
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
    /// An instance is safe to share between threads.
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

        static readonly SearchValues<char> scanMarks = SearchValues.Create("'\"#:\n");

        static void ScanLines(ReadOnlySpan<char> text, List<Line> items) {
            int start = 0;
            int clean = -1;
            int colon = -1;
            int at = 0;
            while (true) {
                int next = text[at..].IndexOfAny(scanMarks);
                if (next < 0) {
                    items.Add(Finish(text, start, text.Length, clean, colon));
                    return;
                }

                at += next;
                char c = text[at];
                if (c == '\n') {
                    items.Add(Finish(text, start, at, clean, colon));
                    at++;
                    start = at;
                    clean = -1;
                    colon = -1;
                } else if (c == '#') {
                    if (clean < 0 && (at == start || char.IsWhiteSpace(text[at - 1]))) {
                        clean = at;
                        int end = text[at..].IndexOf('\n');
                        at = end < 0 ? text.Length : at + end;
                    } else {
                        at++;
                    }
                } else if (c == ':') {
                    if (colon < 0 && (at + 1 == text.Length || char.IsWhiteSpace(text[at + 1]))) {
                        colon = at;
                    }

                    at++;
                } else {
                    at = SkipQuoted(text, at + 1, c);
                }
            }
        }

        static int SkipQuoted(ReadOnlySpan<char> text, int at, char quote) {
            while (true) {
                int next = text[at..].IndexOfAny(quote, '\\', '\n');
                if (next < 0) {
                    return text.Length;
                }

                at += next;
                char c = text[at];
                if (c == quote) {
                    return at + 1;
                }

                if (c == '\n') {
                    return at;
                }

                at = at + 1 < text.Length && text[at + 1] != '\n' ? at + 2 : at + 1;
            }
        }

        static Line Finish(ReadOnlySpan<char> text, int start, int end, int comment, int colon) {
            ReadOnlySpan<char> raw = text[start..end];
            int clean = comment < 0 ? raw.Length : raw[..(comment - start)].TrimEnd().Length;
            int indent = IndentOf(raw[..clean]);
            return new(
                start,
                raw.Length,
                start + indent,
                start + clean,
                indent,
                colon >= 0 && colon - start < clean ? colon : -1,
                raw[..clean][indent..].IsWhiteSpace(),
                indent < raw.Length && raw[indent] == '\t');
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
        public T Deserialize<T>(string yaml) =>
            Session().DeserializeOnce<T>(yaml);

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
        public object? ParseRaw(string yaml) =>
            Session().ParseOnce(yaml);

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
        public T Deserialize<T>(ReadOnlySpan<char> yaml) =>
            Deserialize<T>(yaml.ToString());

        /// <summary>
        /// Deserializes a UTF-8 YAML document into <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <param name="utf8">
        /// The UTF-8 bytes of the document, with or without a byte order mark.
        /// </param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="NanoYamlException">
        /// The document is not valid UTF-8, is invalid, or cannot be converted
        /// to <typeparamref name="T"/>.
        /// </exception>
        public T Deserialize<T>(ReadOnlySpan<byte> utf8) =>
            Deserialize<T>(Decode(utf8));

        /// <summary>
        /// Reads a YAML document to its end and deserializes it into
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <param name="reader">
        /// The reader to read the document from; it is not closed.
        /// </param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="NanoYamlException">
        /// The document is invalid or cannot be converted to
        /// <typeparamref name="T"/>.
        /// </exception>
        public T Deserialize<T>(TextReader reader) =>
            Deserialize<T>(ReadAll(reader));

        /// <summary>
        /// Reads a UTF-8 YAML document to the end of the stream and
        /// deserializes it into <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <param name="stream">
        /// The stream to read the document from, with or without a byte order
        /// mark; it is not closed.
        /// </param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="NanoYamlException">
        /// The document is not valid UTF-8, is invalid, or cannot be converted
        /// to <typeparamref name="T"/>.
        /// </exception>
        public T Deserialize<T>(Stream stream) =>
            Deserialize<T>(ReadAll(stream));

        /// <summary>
        /// Parses a YAML document into an untyped object tree.
        /// </summary>
        /// <param name="yaml">The YAML document to read.</param>
        /// <returns>
        /// The same tree as <see cref="ParseRaw(string)"/>.
        /// </returns>
        /// <exception cref="NanoYamlException">
        /// The document is invalid.
        /// </exception>
        public object? ParseRaw(ReadOnlySpan<char> yaml) =>
            ParseRaw(yaml.ToString());

        /// <summary>
        /// Parses a UTF-8 YAML document into an untyped object tree.
        /// </summary>
        /// <param name="utf8">
        /// The UTF-8 bytes of the document, with or without a byte order mark.
        /// </param>
        /// <returns>
        /// The same tree as <see cref="ParseRaw(string)"/>.
        /// </returns>
        /// <exception cref="NanoYamlException">
        /// The document is not valid UTF-8 or is invalid.
        /// </exception>
        public object? ParseRaw(ReadOnlySpan<byte> utf8) =>
            ParseRaw(Decode(utf8));

        /// <summary>
        /// Reads a YAML document to its end and parses it into an untyped
        /// object tree.
        /// </summary>
        /// <param name="reader">
        /// The reader to read the document from; it is not closed.
        /// </param>
        /// <returns>
        /// The same tree as <see cref="ParseRaw(string)"/>.
        /// </returns>
        /// <exception cref="NanoYamlException">
        /// The document is invalid.
        /// </exception>
        public object? ParseRaw(TextReader reader) =>
            ParseRaw(ReadAll(reader));

        /// <summary>
        /// Reads a UTF-8 YAML document to the end of the stream and parses it
        /// into an untyped object tree.
        /// </summary>
        /// <param name="stream">
        /// The stream to read the document from, with or without a byte order
        /// mark; it is not closed.
        /// </param>
        /// <returns>
        /// The same tree as <see cref="ParseRaw(string)"/>.
        /// </returns>
        /// <exception cref="NanoYamlException">
        /// The document is not valid UTF-8 or is invalid.
        /// </exception>
        public object? ParseRaw(Stream stream) =>
            ParseRaw(ReadAll(stream));

        static readonly UTF8Encoding strictUtf8 = new(false, true);

        static string Decode(ReadOnlySpan<byte> utf8) {
            try {
                return strictUtf8.GetString(utf8.StartsWith(strictUtf8.Preamble) ? utf8[strictUtf8.Preamble.Length..] : utf8);
            } catch (DecoderFallbackException e) {
                throw new NanoYamlException("Invalid UTF-8", 0, e);
            }
        }

        static string ReadAll(TextReader reader) {
            ArgumentNullException.ThrowIfNull(reader);
            return reader.ReadToEnd();
        }

        static string ReadAll(Stream stream) {
            ArgumentNullException.ThrowIfNull(stream);
            using MemoryStream buffer = new();
            stream.CopyTo(buffer);
            return Decode(buffer.GetBuffer().AsSpan(0, (int)buffer.Length));
        }

        Deserializer Session() =>
            new(settings);

        T DeserializeOnce<T>(string yaml) {
            object? raw = ParseOnce(yaml);
            line = 0;
            object loaded = DeserializeUnknown(raw, typeof(T), 0);
            return loaded == null
                ? (T)Null(ShapeOf(typeof(T)))
                : loaded is T done
                    ? done
                    : throw Fail($"Cannot load {Shape(loaded)} into {typeof(T).Name}");
        }

        object? ParseOnce(string yaml) {
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
                ScanLines(span, lines.Items);
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

        object Null(TypeShape shape) =>
            shape.NullRejected
                ? throw Fail($"Null incompatible with {shape.Type.Name}")
                : null!;

        Type ConcreteMap(TypeShape shape) =>
            shape.ConcreteMap ??= GetConcreteMapType(shape.Type);

        Type ConcreteList(TypeShape shape) =>
            shape.ConcreteList ??= GetConcreteListType(shape.Type, shape.ElementType);

        object DeserializeUnknown(object obj, Type type, int depth) =>
            DeserializeUnknown(obj, ShapeOf(type), depth);

        object DeserializeUnknown(object obj, TypeShape shape, int depth) {
            object DeserializeBasic(object value, TypeShape shape) {
                Type type = shape.Plain;

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
                    return Null(shape);
                }

                if (value is not string marker) {
                    throw Fail($"{type.Name} needs text, not {Shape(value)}");
                }

                if (type == typeof(string)) {
                    return marker;
                }

                if (marker.Length == 0) {
                    throw Fail($"{type.Name} has no empty: ''");
                }

                if (type.IsEnum) {
                    try {
                        return Enum.Parse(type, value.ToString()!);
                    } catch (Exception e) when (e is ArgumentException or OverflowException) {
                        throw Fail($"cannot read '{value}' as {type.Name}", e);
                    }
                }

                return type == typeof(bool)
                    ? ParseBool(marker)
                    : TryFastNumber(marker, shape.Code, mode == ParsingMode.Compatible, out object number)
                        ? number
                        : ParseNumber(marker, type);
            }

            object DeserializeMap(object value, TypeShape shape, int depth) {
                if (value is List<object> { Count: 0 }) {
                    return Activator.CreateInstance(ConcreteMap(shape))!;
                }

                if (value is not Dictionary<object, object> data) {
                    throw Fail($"Cannot load {Shape(value)} into {shape.Type.Name}");
                }

                object result = Activator.CreateInstance(ConcreteMap(shape))!;
                TypeShape //
                    keys = shape.KeyShape,
                    values = shape.ValueShape;
                if (result is IDictionary dict) {
                    foreach (KeyValuePair<object, object> entry in data) {
                        dict[DeserializeUnknown(entry.Key, keys, depth + 1)] =
                            DeserializeUnknown(entry.Value, values, depth + 1);
                    }

                    return result;
                }

                if (shape.FillGenericMap is { } fill) {
                    fill(
                        result,
                        data,
                        key => DeserializeUnknown(key, keys, depth + 1),
                        val => DeserializeUnknown(val, values, depth + 1));
                    return result;
                }

                throw Fail($"{shape.Type.Name} needs an indexer");
            }

            object DeserializeList(object value, TypeShape shape, int depth) {
                if (value is Dictionary<object, object> { Count: 0 }) {
                    return Activator.CreateInstance(ConcreteList(shape))!;
                }

                if (value is not List<object> data) {
                    throw Fail($"Cannot load {Shape(value)} into {shape.Type.Name}");
                }

                Type type = ConcreteList(shape);
                TypeShape element = shape.ElementShape;
                if (type.IsArray) {
                    IList array = Array.CreateInstance(type.GetElementType()!, data.Count);
                    for (int i = 0; i < array.Count; i++) {
                        array[i] = DeserializeUnknown(data[i], element, depth + 1);
                    }

                    return array;
                }

                object result = Activator.CreateInstance(type)!;
                if (result is IList list) {
                    foreach (object item in data) {
                        list.Add(DeserializeUnknown(item, element, depth + 1));
                    }

                    return result;
                }

                if (shape.FillGenericList is { } fill) {
                    TypeShape filled = shape.FillElementShape;
                    fill(result, data, item => DeserializeUnknown(item, filled, depth + 1));
                    return result;
                }

                throw Fail($"{shape.Type.Name} needs Add");
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
                        .. ShapeOf(baseType).Implementations
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
                    0 => !type.IsValueType && shape.DefaultCtor is null
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
                List<ParameterInfo> constructorParams = shape.CtorParams;

                List<object> constructorArgs = [
                    .. constructorParams.Select((p, i) =>
                        p.Name is string paramName && data.TryGetValue(paramName, out object? given)
                            ? DeserializeUnknown(given, shape.CtorShapes[i], depth + 1)
                            : p.HasDefaultValue
                                ? p.DefaultValue!
                                : GetDefaultValue(p, shape.CtorNotNull[i])),
                ];

                RejectUnknownKeys(type, data, shape.CtorKnown);

                object instance = shape.Construct(constructorArgs);
                FillMembers(instance, data, shape.SetAfterCtor, depth);
                return instance;
            }

            object Fill(
                Type type,
                Dictionary<object, object> data,
                TypeShape shape,
                int depth) {
                bool ordered = shape.UniqueWritable && InOrder(data, shape.Writable);
                if (!ordered) {
                    RejectUnknownKeys(type, data, shape.WritableNames);
                }

                object instance = type.IsValueType
                    ? Activator.CreateInstance(type)!
                    : shape.Create();

                if (ordered) {
                    FillOrdered(instance, data, shape.Writable, depth);
                } else {
                    FillWritable(instance, data, shape, depth);
                }

                return instance;
            }

            object GetDefaultValue(ParameterInfo param, bool notNull) {
                Type type = param.ParameterType;
                TypeShape shape = ShapeOf(type);
                return type.IsValueType || shape.Map
                    ? Activator.CreateInstance(ConcreteMap(shape))!
                    : shape.List
                        ? Activator.CreateInstance(ConcreteList(shape))!
                        : type == typeof(string) && notNull
                            ? ""
                            : null!;
            }

            Type type = shape.Type;
            return depth > settings.MaxDepth
                ? throw Fail($"Nesting past {settings.MaxDepth} deep")
                : obj is null
                    ? Null(shape)
                    : type == typeof(object)
                        ? obj
                        : shape.Basic
                            ? DeserializeBasic(obj, shape)
                            : shape.Map
                                ? DeserializeMap(obj, shape, depth + 1)
                                : shape.List
                                    ? DeserializeList(obj, shape, depth + 1)
                                    : obj is List<object> sequence
                                        ? DeserializeSequence(sequence, type, depth + 1)
                                        : DeserializeRecord(obj, type, depth + 1);
        }

        static bool TryFastNumber(string text, TypeCode code, bool relaxed, out object value) {
            value = null!;
            ReadOnlySpan<char> chars = text;
            if (chars.IsEmpty || chars.ContainsAnyExcept(numberCharacters)) {
                return false;
            }

            int at = chars[0] is '+' or '-' ? 1 : 0;
            int head = at;
            int digits = 0;
            int dots = 0;
            while (at < chars.Length && chars[at] is (>= '0' and <= '9') or '.') {
                if (chars[at] == '.') {
                    dots++;
                } else {
                    digits++;
                }

                at++;
            }

            if (at < chars.Length && chars[at] is 'e' or 'E') {
                at++;
                if (at < chars.Length && chars[at] is '+' or '-') {
                    at++;
                }

                int exponent = at;
                while (at < chars.Length && chars[at] is >= '0' and <= '9') {
                    at++;
                }

                if (at == exponent) {
                    return false;
                }
            }

            if (dots > 1 || digits == 0 || at != chars.Length) {
                return false;
            }

            ReadOnlySpan<char> lead = chars[head..];
            if (code is TypeCode.Double or TypeCode.Single && lead.IndexOfAny('.', 'e', 'E') is var cut and >= 0) {
                lead = lead[..cut];
            }

            if (!relaxed && lead.Length > 1 && lead[0] == '0' && !lead.ContainsAnyExceptInRange('0', '9')) {
                return false;
            }

            CultureInfo invariant = CultureInfo.InvariantCulture;
            const NumberStyles //
                integer = NumberStyles.Integer,
                floating = NumberStyles.Float | NumberStyles.AllowThousands;
            bool parsed;
            (parsed, value) = code switch {
                TypeCode.SByte => (sbyte.TryParse(text, integer, invariant, out sbyte number), number),
                TypeCode.Byte => (byte.TryParse(text, integer, invariant, out byte number), number),
                TypeCode.Int16 => (short.TryParse(text, integer, invariant, out short number), number),
                TypeCode.UInt16 => (ushort.TryParse(text, integer, invariant, out ushort number), number),
                TypeCode.Int32 => (int.TryParse(text, integer, invariant, out int number), number),
                TypeCode.UInt32 => (uint.TryParse(text, integer, invariant, out uint number), number),
                TypeCode.Int64 => (long.TryParse(text, integer, invariant, out long number), number),
                TypeCode.UInt64 => (ulong.TryParse(text, integer, invariant, out ulong number), number),
                TypeCode.Single => (float.TryParse(text, floating, invariant, out float number), number),
                TypeCode.Double => (double.TryParse(text, floating, invariant, out double number), number),
                TypeCode.Decimal => (decimal.TryParse(text, NumberStyles.Number, invariant, out decimal number), number),
                _ => (false, (object)null!),
            };
            return parsed;
        }

        static readonly SearchValues<char> numberCharacters = SearchValues.Create("+-.0123456789eE");

        void RejectUnknownKeys(Type type, Dictionary<object, object> data, HashSet<string> known) {
            foreach (object key in data.Keys) {
                if (!known.Contains(key as string ?? key.ToString()!)) {
                    RejectUnknownKeysSlow(type, data, known);
                }
            }
        }

        void RejectUnknownKeysSlow(Type type, Dictionary<object, object> data, HashSet<string> known) {
            List<string> unknown = [
                .. data.Keys
                    .Select( k => k.ToString() )
                    .Where( name => !known.Contains( name ) ),
            ];
            if (unknown.Any()) {
                throw Fail($"{type.Name} has no such key: {unknown.JoinToString()}");
            }
        }

        static bool InOrder(Dictionary<object, object> data, List<Member> members) {
            int at = 0;
            foreach (object key in data.Keys) {
                if (key is not string name) {
                    return false;
                }

                while (at < members.Count && !string.Equals(members[at].Name, name)) {
                    at++;
                }

                if (at == members.Count) {
                    return false;
                }

                at++;
            }

            return true;
        }

        void FillOrdered(object instance, Dictionary<object, object> data, List<Member> members, int depth) {
            int at = 0;
            foreach (KeyValuePair<object, object> entry in data) {
                while (!string.Equals(members[at].Name, (string)entry.Key)) {
                    at++;
                }

                Member member = members[at];
                member.Set!(instance, DeserializeUnknown(entry.Value, member.Shape, depth + 1));
                at++;
            }
        }

        void FillWritable(object instance, Dictionary<object, object> data, TypeShape shape, int depth) =>
            FillMembers(instance, data, shape.Writable, depth);

        void FillMembers(object instance, Dictionary<object, object> data, List<Member> members, int depth) {
            foreach (Member member in members) {
                if (data.TryGetValue(member.Name, out object? item)) {
                    member.Set!(instance, DeserializeUnknown(item, member.Shape, depth + 1));
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
                    genericDefinition == typeof(IList<>) ||
                    genericDefinition == typeof(IReadOnlyCollection<>) ||
                    genericDefinition == typeof(IReadOnlyList<>)) {
                    return typeof(List<>).MakeGenericType(valueType);
                }

                if (genericDefinition == typeof(ISet<>) ||
                    genericDefinition == typeof(IReadOnlySet<>)) {
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
    static readonly MethodInfo //
        genericMapWriterMethod = typeof(Serializer).GetMethod(
            "WriteGenericMap",
            BindingFlags.Static | BindingFlags.NonPublic)!,
        genericListFillerMethod = typeof(NanoYaml).GetMethod(
            nameof(FillGenericList),
            BindingFlags.Static | BindingFlags.NonPublic)!,
        genericMapFillerMethod = typeof(NanoYaml).GetMethod(
            nameof(FillGenericMap),
            BindingFlags.Static | BindingFlags.NonPublic)!,
        genericMapCounterMethod = typeof(NanoYaml).GetMethod(
            nameof(CountGenericMap),
            BindingFlags.Static | BindingFlags.NonPublic)!,
        genericMapValuesMethod = typeof(NanoYaml).GetMethod(
            nameof(ValuesGenericMap),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    static TypeShape ShapeOf(Type type) =>
        shapes.GetValue(type, static shape => new TypeShape(shape));

    static GenericMapWriter CreateGenericMapWriter(Type keyType, Type valueType) =>
        (GenericMapWriter)genericMapWriterMethod.MakeGenericMethod(keyType, valueType)
            .CreateDelegate(typeof(GenericMapWriter));

    static GenericListFiller CreateGenericListFiller(Type elementType) =>
        (GenericListFiller)genericListFillerMethod.MakeGenericMethod(elementType)
            .CreateDelegate(typeof(GenericListFiller));

    static GenericMapFiller CreateGenericMapFiller(Type keyType, Type valueType) =>
        (GenericMapFiller)genericMapFillerMethod.MakeGenericMethod(keyType, valueType)
            .CreateDelegate(typeof(GenericMapFiller));

    static Type? CollectionFillType(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>)
            ? type.GetGenericArguments()[0]
            : type.GetInterfaces().Where(i => i.IsGenericType)
                .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(ICollection<>))
                ?.GetGenericArguments()[0];

    static void FillGenericList<T>(object target, List<object> data, Func<object, object?> convert) {
        ICollection<T> list = (ICollection<T>)target;
        foreach (object item in data) {
            list.Add((T)convert(item)!);
        }
    }

    static void FillGenericMap<TKey, TValue>(
        object target,
        Dictionary<object, object> data,
        Func<object, object?> convertKey,
        Func<object, object?> convertValue) {
        IDictionary<TKey, TValue> map = (IDictionary<TKey, TValue>)target;
        foreach (KeyValuePair<object, object> entry in data) {
            map[(TKey)convertKey(entry.Key)!] = (TValue)convertValue(entry.Value)!;
        }
    }

    static GenericMapCounter CreateGenericMapCounter(Type keyType, Type valueType) =>
        (GenericMapCounter)genericMapCounterMethod.MakeGenericMethod(keyType, valueType)
            .CreateDelegate(typeof(GenericMapCounter));

    static GenericMapValues CreateGenericMapValues(Type keyType, Type valueType) =>
        (GenericMapValues)genericMapValuesMethod.MakeGenericMethod(keyType, valueType)
            .CreateDelegate(typeof(GenericMapValues));

    static int CountGenericMap<TKey, TValue>(object target, uint cap) {
        int count = 0;
        foreach (KeyValuePair<TKey, TValue> entry in (IDictionary<TKey, TValue>)target) {
            if (entry.Key is not null && ++count > cap) {
                break;
            }
        }

        return count;
    }

    static List<object> ValuesGenericMap<TKey, TValue>(object target) =>
        [.. ((IDictionary<TKey, TValue>)target).Where(entry => entry.Key is not null).Select(entry => entry.Value!)];

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

    static bool Accessible(Type type, PropertyInfo property, bool compile) =>
        compile && RuntimeFeature.IsDynamicCodeSupported && !type.IsValueType &&
        property.GetMethod?.IsPublic == true && property.GetIndexParameters().Length == 0 &&
        !property.PropertyType.IsByRef && !property.PropertyType.IsByRefLike && !property.PropertyType.IsPointer;

    static Func<object, object?> PropertyGetter(Type type, PropertyInfo property, bool compile, bool hot) =>
        !Accessible(type, property, compile)
            ? property.GetValue
            : hot
                ? CompiledGetter(type, property)
                : Typed<Func<MethodInfo, Func<object, object?>>>(typedGetter, type, property.PropertyType)(property.GetMethod!);

    static TFactory Typed<TFactory>(MethodInfo factory, Type owner, Type value) where TFactory : Delegate =>
        factory.MakeGenericMethod(owner, value).CreateDelegate<TFactory>();

    static readonly MethodInfo //
        typedGetter = typeof(NanoYaml).GetMethod(nameof(TypedGetter), BindingFlags.Static | BindingFlags.NonPublic)!,
        typedSetter = typeof(NanoYaml).GetMethod(nameof(TypedSetter), BindingFlags.Static | BindingFlags.NonPublic)!,
        typedFactory = typeof(NanoYaml).GetMethod(nameof(TypedFactory), BindingFlags.Static | BindingFlags.NonPublic)!;

    static Func<object, object?> TypedGetter<TOwner, TValue>(MethodInfo getter) {
        Func<TOwner, TValue> typed = getter.CreateDelegate<Func<TOwner, TValue>>();
        return instance => {
            try {
                return typed((TOwner)instance);
            } catch (Exception e) {
                throw new TargetInvocationException(e);
            }
        };
    }

    static Action<object, object?> TypedSetter<TOwner, TValue>(MethodInfo setter) {
        Action<TOwner, TValue> typed = setter.CreateDelegate<Action<TOwner, TValue>>();
        return (instance, value) => {
            try {
                typed((TOwner)instance, (TValue)value!);
            } catch (Exception e) {
                throw new TargetInvocationException(e);
            }
        };
    }

    static Func<object> TypedFactory<T>() where T : new() =>
        static () => {
            try {
                return new T();
            } catch (TargetInvocationException e) when (e.InnerException is not null) {
                throw new TargetInvocationException(e.InnerException);
            } catch (Exception e) {
                throw new TargetInvocationException(e);
            }
        };

    static Func<object, object?> CompiledGetter(Type type, PropertyInfo property) {
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

    static Func<object, object?> FieldGetter(Type type, FieldInfo field, bool compile, bool hot) {
        if (!hot || !compile || !RuntimeFeature.IsDynamicCodeSupported || type.IsValueType ||
            field.FieldType.IsByRef || field.FieldType.IsByRefLike || field.FieldType.IsPointer) {
            return field.GetValue;
        }

        ParameterExpression instance = Expression.Parameter(typeof(object));
        Expression //
            target = Expression.Convert(instance, type),
            value = Expression.Convert(Expression.Field(target, field), typeof(object));
        return Expression.Lambda<Func<object, object?>>(value, instance).Compile();
    }

    static Action<object, object?> MemberSetter(
        Type type,
        MemberInfo member,
        Type valueType,
        Action<object, object?> slow,
        bool compile,
        bool hot) {
        if (!compile || !RuntimeFeature.IsDynamicCodeSupported || type.IsValueType ||
            valueType.IsByRef || valueType.IsByRefLike || valueType.IsPointer ||
            member is PropertyInfo { SetMethod.IsPublic: not true } ||
            member is PropertyInfo property && property.GetIndexParameters().Length > 0 ||
            !hot && member is not PropertyInfo) {
            return slow;
        }

        Action<object, object?> fast = hot
            ? CompiledSetter(type, member, valueType)
            : Typed<Func<MethodInfo, Action<object, object?>>>(typedSetter, type, valueType)(((PropertyInfo)member).SetMethod!);
        Type check = Nullable.GetUnderlyingType(valueType) ?? valueType;
        bool nullable = !valueType.IsValueType || check != valueType;
        return (obj, item) => {
            if (item is null ? nullable : check.IsInstanceOfType(item)) {
                fast(obj, item);
            } else {
                slow(obj, item);
            }
        };
    }

    static Action<object, object?> CompiledSetter(Type type, MemberInfo member, Type valueType) {
        ParameterExpression //
            instance = Expression.Parameter(typeof(object)),
            value = Expression.Parameter(typeof(object)),
            error = Expression.Parameter(typeof(Exception));
        Expression //
            target = Expression.Convert(instance, type),
            access = member is PropertyInfo info
                ? Expression.Property(target, info)
                : Expression.Field(target, (FieldInfo)member),
            assign = Expression.Assign(access, Expression.Convert(value, valueType)),
            body = member is PropertyInfo
                ? Expression.TryCatch(
                    Expression.Block(typeof(void), assign),
                    Expression.Catch(error, Expression.Throw(
                        Expression.New(typeof(TargetInvocationException).GetConstructor([typeof(Exception)])!, error))))
                : Expression.Block(typeof(void), assign);
        return Expression.Lambda<Action<object, object?>>(body, instance, value).Compile();
    }

    static Func<List<object>, object> Constructor(ConstructorInfo constructor, List<ParameterInfo> parameters) {
        Func<List<object>, object> slow = values => constructor.Invoke([.. values]);
        if (!RuntimeFeature.IsDynamicCodeSupported ||
            parameters.Any(p => p.ParameterType.IsByRef || p.ParameterType.IsPointer || p.ParameterType.IsByRefLike)) {
            return slow;
        }

        ParameterExpression //
            args = Expression.Parameter(typeof(List<object>)),
            error = Expression.Parameter(typeof(Exception));
        Expression body = Expression.TryCatch(
            Expression.Convert(
                Expression.New(constructor, parameters.Select((p, i) =>
                    Expression.Convert(Expression.Property(args, "Item", Expression.Constant(i)), p.ParameterType))),
                typeof(object)),
            Expression.Catch(error, Expression.Throw(
                Expression.New(typeof(TargetInvocationException).GetConstructor([typeof(Exception)])!, error),
                typeof(object))));
        Func<List<object>, object> fast = Expression.Lambda<Func<List<object>, object>>(body, args).Compile();
        List<(Type Check, bool Nullable)> checks = [
            .. parameters.Select(p => (
                Nullable.GetUnderlyingType(p.ParameterType) ?? p.ParameterType,
                !p.ParameterType.IsValueType || Nullable.GetUnderlyingType(p.ParameterType) is not null)),
        ];

        bool Fits(List<object> values) {
            if (values.Count != checks.Count) {
                return false;
            }

            for (int i = 0; i < values.Count; i++) {
                (Type check, bool nullable) = checks[i];
                if (values[i] is null ? !nullable : !check.IsInstanceOfType(values[i])) {
                    return false;
                }
            }

            return true;
        }

        return values => Fits(values) ? fast(values) : slow(values);
    }

    static readonly HashSet<Type> scalarTypes = [
        typeof(bool), typeof(int), typeof(long), typeof(double), typeof(float), typeof(decimal),
        typeof(short), typeof(byte), typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte),
    ];

    static readonly MethodInfo //
        writeScalarMethod = typeof(Serializer).GetMethod("WriteScalar", BindingFlags.Static | BindingFlags.NonPublic)!,
        typedScalar = typeof(Serializer).GetMethod("TypedScalar", BindingFlags.Static | BindingFlags.NonPublic)!;

    static Action<StringBuilder, object>? ScalarWriter(Type type, MemberInfo member, Type valueType, bool compile, bool hot) {
        if (!compile || !RuntimeFeature.IsDynamicCodeSupported || type.IsValueType || !scalarTypes.Contains(valueType) ||
            member is PropertyInfo { GetMethod.IsPublic: not true } ||
            member is PropertyInfo indexed && indexed.GetIndexParameters().Length > 0 ||
            !hot && member is not PropertyInfo) {
            return null;
        }

        if (!hot) {
            return Typed<Func<MethodInfo, Action<StringBuilder, object>>>(typedScalar, type, valueType)(((PropertyInfo)member).GetMethod!);
        }

        ParameterExpression //
            sb = Expression.Parameter(typeof(StringBuilder)),
            instance = Expression.Parameter(typeof(object)),
            error = Expression.Parameter(typeof(Exception));
        Expression //
            target = Expression.Convert(instance, type),
            value = member is PropertyInfo property
                ? Expression.TryCatch(
                    Expression.Property(target, property),
                    Expression.Catch(error, Expression.Throw(
                        Expression.New(typeof(TargetInvocationException).GetConstructor([typeof(Exception)])!, error),
                        valueType)))
                : Expression.Field(target, (FieldInfo)member),
            call = Expression.Call(writeScalarMethod.MakeGenericMethod(valueType), sb, value);
        return Expression.Lambda<Action<StringBuilder, object>>(call, sb, instance).Compile();
    }

    static Func<object>? Factory(Type type, ConstructorInfo? constructor, bool hot) {
        if (constructor is null || type.IsValueType || !RuntimeFeature.IsDynamicCodeSupported) {
            return null;
        }

        if (!hot) {
            return typedFactory.MakeGenericMethod(type).CreateDelegate<Func<Func<object>>>()();
        }

        ParameterExpression error = Expression.Parameter(typeof(Exception));
        Expression body = Expression.TryCatch(
            Expression.Convert(Expression.New(constructor), typeof(object)),
            Expression.Catch(error, Expression.Throw(
                Expression.New(typeof(TargetInvocationException).GetConstructor([typeof(Exception)])!, error),
                typeof(object))));
        return Expression.Lambda<Func<object>>(body).Compile();
    }

    const int CompileAfter = 64;

    class Warmup(Action promote) {
        int uses;

        public void Tick() {
            if (Interlocked.Increment(ref uses) == CompileAfter) {
                promote();
            }
        }
    }

    class Member {
        public string Name = "";
        public Type Type = typeof(object);
        public Func<object, object?>? Get;
        public Action<object, object?>? Set;
        public Action<StringBuilder, object>? WriteScalar;

        public void Install(
            Func<bool, Func<object, object?>?> get,
            Func<bool, Action<StringBuilder, object>?> writeScalar,
            Func<bool, Action<object, object?>?> set) {
            Func<object, object?>? coldGet = get(false);
            Action<StringBuilder, object>? coldScalar = writeScalar(false);
            Action<object, object?>? coldSet = set(false);
            Warmup warmup = new(() => {
                Func<object, object?>? hotGet = get(true);
                Action<StringBuilder, object>? hotScalar = writeScalar(true);
                Action<object, object?>? hotSet = set(true);
                Get = hotGet;
                WriteScalar = hotScalar;
                Set = hotSet;
            });
            Get = coldGet is null
                ? null
                : obj => {
                    warmup.Tick();
                    return coldGet(obj);
                };
            WriteScalar = coldScalar is null
                ? null
                : (sb, obj) => {
                    warmup.Tick();
                    coldScalar(sb, obj);
                };
            Set = coldSet is null
                ? null
                : (obj, value) => {
                    warmup.Tick();
                    coldSet(obj, value);
                };
        }

        public string? Key;
        TypeShape? shape;

        public TypeShape Shape =>
            shape ??= ShapeOf(Type);
    }

    class TypeShape {
        public Type Type;
        public Type Plain;
        public TypeCode Code;
        public bool NullRejected;
        public bool Basic;
        public bool Map;
        public bool GenericMap;
        public Type KeyType = typeof(object);
        public Type ValueType = typeof(object);
        public GenericMapWriter? WriteGenericMap;
        public GenericMapFiller? FillGenericMap;
        public GenericMapCounter? CountGenericMap;
        public GenericMapValues? ValuesGenericMap;
        public bool List;
        public Type ElementType = typeof(object);
        public Type FillElementType = typeof(object);
        public GenericListFiller? FillGenericList;

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
                    if (List || type == typeof(string)) {
                        return;
                    }
                } else if (type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                    ElementType = type.GetGenericArguments()[0];
                    List = true;
                    return;
                }

                Type? inherits = type.GetInterfaces().FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                if (inherits is null) {
                    return;
                }

                ElementType = inherits.GetGenericArguments()[0];
                List = true;
            }

            void AnalyzeProperties() {
                Type type = Type;
                static bool Matches(ConstructorInfo constructor, HashSet<string> names) =>
                    constructor.GetParameters().Length > 0 &&
                    constructor.GetParameters().All(p => p.Name is string name && names.Contains(name));

                static bool WantedProperty(PropertyInfo property) =>
                    property.GetIndexParameters().Length == 0
                    && (property.GetMethod?.IsPublic == true
                        || property.SetMethod?.IsPublic == true
                        || property.IsDefined(typeof(SerializeAttribute), true));

                static bool WantedField(FieldInfo field) =>
                    !field.IsDefined(typeof(CompilerGeneratedAttribute), false)
                    && (field.IsPublic || field.IsDefined(typeof(SerializeAttribute), true));

                static List<PropertyInfo> InstanceProperties(Type type) {
                    List<Type> hierarchy = [];
                    for (Type? current = type; current is not null && current != typeof(object); current = current.BaseType) {
                        hierarchy.Add(current);
                    }

                    hierarchy.Reverse();
                    List<PropertyInfo> properties = [];
                    foreach (Type current in hierarchy) {
                        foreach (PropertyInfo property in current.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
                            properties.Add(property);
                        }
                    }

                    return properties;
                }

                static List<FieldInfo> InstanceFields(Type type) {
                    List<Type> hierarchy = [];
                    for (Type? current = type; current is not null && current != typeof(object); current = current.BaseType) {
                        hierarchy.Add(current);
                    }

                    hierarchy.Reverse();
                    List<FieldInfo> fields = [];
                    foreach (Type current in hierarchy) {
                        foreach (FieldInfo field in current.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
                            fields.Add(field);
                        }
                    }

                    return fields;
                }

                static List<Member> Members(Type type, bool compileGetters) {
                    List<Member> members = [];
                    foreach (PropertyInfo property in InstanceProperties(type)) {
                        if (!WantedProperty(property)) {
                            continue;
                        }

                        Member member = new() { Name = property.Name, Type = property.PropertyType };
                        member.Install(
                            hot => property.CanRead ? PropertyGetter(type, property, compileGetters, hot) : null,
                            hot => property.CanRead
                                ? ScalarWriter(type, property, property.PropertyType, compileGetters, hot)
                                : null,
                            hot => property.CanWrite
                                ? MemberSetter(type, property, property.PropertyType, property.SetValue, compileGetters, hot)
                                : null);
                        members.Add(member);
                    }

                    foreach (FieldInfo field in InstanceFields(type)) {
                        if (!WantedField(field)) {
                            continue;
                        }

                        Member member = new() { Name = field.Name, Type = field.FieldType };
                        member.Install(
                            hot => FieldGetter(type, field, compileGetters, hot),
                            hot => ScalarWriter(type, field, field.FieldType, compileGetters, hot),
                            hot => field.IsInitOnly
                                ? null
                                : MemberSetter(type, field, field.FieldType, field.SetValue, compileGetters, hot));
                        members.Add(member);
                    }

                    return members;
                }

                List<Member> members = [.. Members(type, !Basic && !Map && !List)];
                names = [.. members.Select(m => m.Name)];
                matchingCtors = [.. type.GetConstructors().Where(c => Matches(c, names))];
                HashSet<string> ctorNames = matchingCtors.Count == 1
                    ? [.. matchingCtors[0].GetParameters().Select(p => p.Name ?? "")]
                    : [];
                writable = [.. members.Where(m => m.Set is not null)];
                readable = [.. members.Where(m => m.Get is not null
                    && (ctorNames.Contains(m.Name) || writable.Contains(m))),
                ];
                writableNames = [.. writable.Select(m => m.Name)];
                uniqueWritable = writableNames.Count == writable.Count;
                Dictionary<string, int> slots = [];
                foreach (Member member in readable) {
                    if (slots.TryGetValue(member.Name, out int slot)) {
                        emitted[slot] = member;
                    } else {
                        slots[member.Name] = emitted.Count;
                        emitted.Add(member);
                    }
                }

                ctorParams = matchingCtors.Count == 1 ? [.. matchingCtors[0].GetParameters()] : [];
                ctorKnown = [.. ctorParams.Select(p => p.Name!).Concat(writableNames)];
                HashSet<string> bound = [.. ctorParams.Select(p => p.Name!)];
                setAfterCtor = [.. writable.Where(m => !bound.Contains(m.Name))];
                NullabilityInfoContext nullability = new();
                ctorNotNull = [
                    .. ctorParams.Select(p => p.ParameterType == typeof(string)
                        && nullability.Create(p).WriteState == NullabilityState.NotNull),
                ];
                defaultCtor = type.GetConstructor(Type.EmptyTypes);
            }

            Type = type;
            Type plain = UnwrapNullable(type);
            Plain = plain;
            Code = Type.GetTypeCode(plain);
            NullRejected = type.IsValueType && plain == type;
            Basic = plain.IsPrimitive || plain.IsEnum || basicNonPrimitive.Contains(plain);
            AnalyzeMap(type);
            AnalyzeList(type);
            if (List && CollectionFillType(type) is { } fill) {
                FillElementType = fill;
                FillGenericList = CreateGenericListFiller(fill);
            }

            if (GenericMap) {
                WriteGenericMap = CreateGenericMapWriter(KeyType, ValueType);
                FillGenericMap = CreateGenericMapFiller(KeyType, ValueType);
                CountGenericMap = CreateGenericMapCounter(KeyType, ValueType);
                ValuesGenericMap = CreateGenericMapValues(KeyType, ValueType);
            }

            analyze = AnalyzeProperties;
        }

        Action? analyze;
        readonly object analyzing = new();

        TypeShape Analyzed() {
            if (analyze is not null) {
                lock (analyzing) {
                    analyze?.Invoke();
                    analyze = null;
                }
            }

            return this;
        }

        List<Member> readable = [];
        List<Member> writable = [];
        HashSet<string> names = [];
        List<ConstructorInfo> matchingCtors = [];
        HashSet<string> writableNames = [];
        bool uniqueWritable;
        List<Member> emitted = [];
        HashSet<string> ctorKnown = [];
        List<ParameterInfo> ctorParams = [];
        List<Member> setAfterCtor = [];
        List<bool> ctorNotNull = [];
        ConstructorInfo? defaultCtor;

        public List<Member> Readable => Analyzed().readable;
        public List<Member> Writable => Analyzed().writable;
        public HashSet<string> Names => Analyzed().names;
        public List<ConstructorInfo> MatchingCtors => Analyzed().matchingCtors;
        public HashSet<string> WritableNames => Analyzed().writableNames;
        public bool UniqueWritable => Analyzed().uniqueWritable;
        public List<Member> Emitted => Analyzed().emitted;
        public HashSet<string> CtorKnown => Analyzed().ctorKnown;
        public List<ParameterInfo> CtorParams => Analyzed().ctorParams;
        public List<Member> SetAfterCtor => Analyzed().setAfterCtor;
        public List<bool> CtorNotNull => Analyzed().ctorNotNull;
        public ConstructorInfo? DefaultCtor => Analyzed().defaultCtor;
        List<Type>? implementations;

        public List<Type> Implementations =>
            implementations ??= [
                .. Type.Assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface && Type.IsAssignableFrom(t)),
            ];
        List<TypeShape>? ctorShapes;

        public List<TypeShape> CtorShapes =>
            ctorShapes ??= [.. CtorParams.Select(p => ShapeOf(p.ParameterType))];
        public Type? ConcreteMap;
        public Type? ConcreteList;
        TypeShape? keyShape;
        TypeShape? valueShape;
        TypeShape? elementShape;
        TypeShape? fillElementShape;
        Func<object>? factory;
        Func<List<object>, object>? construct;

        public TypeShape KeyShape =>
            keyShape ??= ShapeOf(KeyType);

        public TypeShape ValueShape =>
            valueShape ??= ShapeOf(ValueType);

        public TypeShape ElementShape =>
            elementShape ??= ShapeOf(ElementType);

        public TypeShape FillElementShape =>
            fillElementShape ??= ShapeOf(FillElementType);

        public object Create() =>
            (factory ??= WarmFactory())();

        public object Construct(List<object> args) =>
            (construct ??= WarmConstruct())(args);

        Func<object> WarmFactory() {
            Func<object> cold = Factory(Type, DefaultCtor, false) ?? (() => DefaultCtor!.Invoke([]));
            Warmup warmup = new(() => factory = Factory(Type, DefaultCtor, true) ?? cold);
            return () => {
                warmup.Tick();
                return cold();
            };
        }

        Func<List<object>, object> WarmConstruct() {
            Func<List<object>, object> cold = values => MatchingCtors[0].Invoke([.. values]);
            Warmup warmup = new(() => construct = Constructor(MatchingCtors[0], CtorParams));
            return values => {
                warmup.Tick();
                return cold(values);
            };
        }

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
