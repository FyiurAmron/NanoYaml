using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vaxquis.NanoYaml;
using Xunit;

namespace Vaxquis.NanoYaml.Tests;

/// <summary>
/// The span, reader, writer and stream entry points. Each one reads or writes
/// the same document as the string entry point does.
/// </summary>
public class NanoYamlIoTests {
    static readonly SaveLike save = new(3, "héllo \U0001F389 wörld", new() { ["gold"] = 7, ["日本"] = 2 }, ["a", "b: c"]);

    static string Text =>
        NanoYaml.Serialize(save);

    static byte[] Utf8 =>
        new UTF8Encoding(false).GetBytes(Text);

    static byte[] WithBom =>
        [.. new UTF8Encoding(true).GetPreamble(), .. Utf8];

    [Fact]
    public void EveryInputReadsTheSameRecord() {
        List<SaveLike> loaded = [
            NanoYaml.Deserialize<SaveLike>(Text.AsSpan()),
            NanoYaml.Deserialize<SaveLike>(Utf8.AsSpan()),
            NanoYaml.Deserialize<SaveLike>(WithBom.AsSpan()),
            NanoYaml.Deserialize<SaveLike>(new StringReader(Text)),
            NanoYaml.Deserialize<SaveLike>(new MemoryStream(Utf8)),
            NanoYaml.Deserialize<SaveLike>(new MemoryStream(WithBom)),
            Modes.Strict.Deserialize<SaveLike>(new MemoryStream(Utf8)),
        ];
        string expected = Text;
        Assert.All(loaded, back => Assert.Equal(expected, NanoYaml.Serialize(back)));
    }

    [Fact]
    public void EveryInputParsesTheSameTree() {
        object? tree = NanoYaml.ParseRaw(Text);
        List<object?> parsed = [
            NanoYaml.ParseRaw(Text.AsSpan()),
            NanoYaml.ParseRaw(Utf8.AsSpan()),
            NanoYaml.ParseRaw(WithBom.AsSpan()),
            NanoYaml.ParseRaw(new StringReader(Text)),
            NanoYaml.ParseRaw(new MemoryStream(WithBom)),
        ];
        Assert.All(parsed, back => Assert.True(Modes.Same(tree, back), Modes.Show(back)));
    }

    [Fact]
    public void WriterGetsTheSameText() {
        StringWriter writer = new();
        NanoYaml.Serialize(save, writer);
        Assert.Equal(Text, writer.ToString());
    }

    [Fact]
    public void StreamGetsUtf8WithoutBom() {
        MemoryStream stream = new();
        NanoYaml.Serialize(save, stream);
        Assert.Equal(Utf8, stream.GetBuffer().AsSpan(0, (int)stream.Length));
    }

    [Fact]
    public void LargeDocumentCrossesBufferAndChunkEdges() {
        Dictionary<string, string> big = [];
        for (int i = 0; i < 20000; i++) {
            big[$"k{i}"] = i % 3 == 0 ? $"ελλάδα \U0001F389 {i}" : $"v{i}";
        }

        string text = NanoYaml.Serialize(big);
        MemoryStream stream = new();
        NanoYaml.Serialize(big, stream);
        StringWriter writer = new();
        NanoYaml.Serialize(big, writer);
        Assert.Equal(new UTF8Encoding(false).GetBytes(text), stream.GetBuffer().AsSpan(0, (int)stream.Length));
        Assert.Equal(text, writer.ToString());
        stream.Position = 0;
        Assert.Equal(big, NanoYaml.Deserialize<Dictionary<string, string>>(stream));
    }

    [Fact]
    public void InvalidUtf8Throws() {
        var error = Assert.Throws<NanoYamlException>(() => NanoYaml.ParseRaw([0x61, 0x3A, 0x20, 0xC3, 0x28]));
        Assert.Equal("Invalid UTF-8", error.Message);
    }

    [Fact]
    public void ErrorsKeepTheirLine() {
        var fromText = Assert.Throws<NanoYamlException>(() => NanoYaml.ParseRaw("a: 1\nb\n"));
        var fromStream = Assert.Throws<NanoYamlException>(() => NanoYaml.ParseRaw(new MemoryStream(Encoding.UTF8.GetBytes("a: 1\nb\n"))));
        Assert.Equal(fromText.Message, fromStream.Message);
        Assert.Equal(2, fromStream.Line);
    }

    [Fact]
    public void UnpairedSurrogateStillFailsOnTheWayOut() {
        Assert.Throws<NanoYamlException>(() => NanoYaml.Serialize(new List<string> { "a\uD800b" }, new MemoryStream()));
        Assert.Throws<NanoYamlException>(() => NanoYaml.Serialize(new List<string> { "a\uD800b" }, new StringWriter()));
    }
}
