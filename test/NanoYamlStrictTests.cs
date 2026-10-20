using System.Collections.Generic;
using Vaxquis.NanoYaml;
using Xunit;

namespace Vaxquis.NanoYaml.Tests;

/// <summary>
/// The documents the strict profile rejects. Every test here reads the
/// document in the strict mode and holds it to the strict answer; the
/// compatible mode reads the same documents, so the two groups answer each
/// other. Documents that are not about the header carry the profile tag, so
/// the rejection is the construct's and not the missing tag's.
/// </summary>
public class NanoYamlStrictTests {
    [Fact]
    public void MissingProfileThrows() =>
        Assert.Throws<NanoYamlException>(() => Modes.Strict.Deserialize<WithCount>("N: 1\n"));

    [Fact]
    public void EmptyDocThrows() =>
        Assert.All(["", "   \n", "# nothing here\n"], yaml =>
            Assert.Throws<NanoYamlException>(() =>
                Modes.Strict.Deserialize<Dictionary<string, string>>(Modes.Profile + yaml)));

    [Fact]
    public void EmptyValueThrows() {
        Assert.Throws<NanoYamlException>(() => Modes.Strict.Deserialize<WithIList>($"{Modes.Profile}Items:\n"));
        Assert.Throws<NanoYamlException>(() => Modes.Strict.ParseRaw($"{Modes.Profile}a:\n  "));
    }

    [Fact]
    public void EmptyNestedValueThrows() =>
        Assert.Throws<NanoYamlException>(() => Modes.Strict.Deserialize<WithNames>($"{Modes.Profile}Names:\n"));

    [Fact]
    public void DocMarkersThrow() =>
        Assert.All(["---\n", "...\n"], yaml =>
            Assert.Throws<NanoYamlException>(() =>
                Modes.Strict.Deserialize<Dictionary<string, string>>(Modes.Profile + yaml)));

    [Fact]
    public void BareDashStillMarked() =>
        Assert.Throws<NanoYamlException>(() => Modes.Strict.Deserialize<WithCount>($"{Modes.Profile}---\nN: 1\n"));

    [Fact]
    public void WildNullFormsThrow() =>
        Assert.All(["a: nUll\n", "a: nuLL\n", "a: NulL\n", "a: nULL\n"], yaml =>
            Assert.Throws<NanoYamlException>(() =>
                Modes.Strict.Deserialize<Dictionary<string, string>>(Modes.Profile + yaml)));

    [Fact]
    public void CapitalNullFormsThrow() =>
        Assert.All(["a: Null\n", "a: NULL\n"], yaml =>
            Assert.Throws<NanoYamlException>(() =>
                Modes.Strict.Deserialize<Dictionary<string, string>>(Modes.Profile + yaml)));

    [Fact]
    public void OctalLoadThrows() =>
        Assert.Throws<NanoYamlException>(() => Modes.Strict.Deserialize<WithCount>($"{Modes.Profile}N: 0o17\n"));

    [Fact]
    public void LeadingZeroThrows() =>
        Assert.All(["01234", "017"], padded =>
            Assert.Throws<NanoYamlException>(() =>
                Modes.Strict.Deserialize<WithCount>($"{Modes.Profile}N: {padded}\n")));

    [Fact]
    public void PaddedFloatThrows() =>
        Assert.All(["01234", "01234.5"], padded =>
            Assert.Throws<NanoYamlException>(() =>
                Modes.Strict.Deserialize<WithFloat>($"{Modes.Profile}X: {padded}\n")));

    [Fact]
    public void SpecialDoubleCaseFormsThrow() =>
        Assert.All([".Inf", ".INF", "+.INF", "-.Inf", ".NaN", ".NAN"], word =>
            Assert.Throws<NanoYamlException>(() =>
                Modes.Strict.Deserialize<WithFloat>($"{Modes.Profile}X: {word}\n")));

    [Fact]
    public void CapitalBoolThrows() =>
        Assert.All(["True", "TRUE", "False", "FALSE"], word =>
            Assert.Throws<NanoYamlException>(() =>
                Modes.Strict.Deserialize<FlagRec>($"{Modes.Profile}On: {word}\n")));

    [Fact]
    public void TabInContentThrows() =>
        Assert.All(["k:\ta\n", "k: a\tb\n", "k: \"a\tb\"\n", "k: |\n  a\tb\n"], yaml =>
            Assert.Throws<NanoYamlException>(() =>
                Modes.Strict.Deserialize<Dictionary<string, string>>(Modes.Profile + yaml)));

    [Fact]
    public void ForeignTagDirectiveThrows() =>
        Assert.Throws<NanoYamlException>(() => Modes.Strict.Deserialize<WithCount>("%TAG ! other\n---\nN: 1\n"));

    [Fact]
    public void ValidYamlDirectiveThrows() {
        List<(string Yaml, int Line, string Version)> cases = [
            ("%YAML 1.1\n---\nN: 1\n", 1, "1.1"),
            ("%YAML 1.2\n---\nN: 1\n", 1, "1.2"),
            ("%YAML 2.0\n---\nN: 1\n", 1, "2.0"),
            ("%TAG ! nanoyaml:0.1.0:\n%YAML 1.2\n---\nN: 1\n", 2, "1.2"),
        ];
        Assert.All(cases, c => {
            NanoYamlException exception =
                Assert.Throws<NanoYamlException>(() => Modes.Strict.Deserialize<WithCount>(c.Yaml));
            Assert.Equal(c.Line, exception.Line);
            Assert.Equal(
                $"line {c.Line}: Valid %YAML {c.Version} directive found. Either remove the directive or use another YAML parser.",
                exception.Message);
        });
    }

    [Fact]
    public void UnknownDirectiveThrowsNamed() {
        Dictionary<string, string> cases = new() {
            ["%FOO bar baz\n---\na: 1\n"] = "line 1: Unknown directive '%FOO'",
            ["%TAG ! nanoyaml:0.1.0:\n%FOO bar\n---\na: 1\n"] = "line 2: Unknown directive '%FOO'",
            ["%YAMLfoo bar\n---\na: 1\n"] = "line 1: Unknown directive '%YAMLfoo'",
            ["%TAGfoo bar\n---\na: 1\n"] = "line 1: Unknown directive '%TAGfoo'",
            ["%YAML1.2\n---\na: 1\n"] = "line 1: Unknown directive '%YAML1.2'",
            ["%TAG ! nanoyaml:0.1.0:\n%YAMLfoo bar\n---\na: 1\n"] = "line 2: Unknown directive '%YAMLfoo'",
            ["%TAG ! nanoyaml:0.1.0:\n%TAGfoo bar\n---\na: 1\n"] = "line 2: Unknown directive '%TAGfoo'",
        };
        Assert.All(cases, c => Assert.Contains(
            c.Value,
            Assert.Throws<NanoYamlException>(() => Modes.Strict.Deserialize<WithCount>(c.Key)).Message));
    }
}
