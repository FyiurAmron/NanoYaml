using System.Collections.Generic;
using System.Linq;
using Vaxquis.NanoYaml;
using Xunit;

namespace Vaxquis.NanoYaml.Tests;

/// <summary>
/// The documents the strict profile rejects and the compatible mode reads:
/// the valid YAML 1.2.2 constructs the strict subset leaves out. Every test
/// here mirrors a strict test with the same document.
/// </summary>
public class NanoYamlCompatibleTests {
    [Fact]
    public void MissingProfileLoads() =>
        Assert.Equal(1, Modes.Compatible.Deserialize<WithCount>("N: 1\n").N);

    [Fact]
    public void EmptyDocIsNull() =>
        Assert.All(["", "   \n", "# nothing here\n"], yaml =>
            Assert.Null(Modes.Compatible.Deserialize<Dictionary<string, string>?>(yaml)));

    [Fact]
    public void EmptyValueIsNull() {
        Assert.Null(Modes.Compatible.Deserialize<WithIList>("Items:\n").Items);
        Assert.Null(((Dictionary<object, object>)Modes.Compatible.ParseRaw("a:\n  "))["a"]);
    }

    [Fact]
    public void EmptyNestedValueIsNull() =>
        Assert.Null(Modes.Compatible.Deserialize<WithNames>("Names:\n").Names);

    [Fact]
    public void DocMarkersAreIgnored() {
        Assert.Equal("v", Modes.Compatible.Deserialize<Dictionary<string, string>>("---\nk: v\n")["k"]);
        Assert.Equal("v", Modes.Compatible.Deserialize<Dictionary<string, string>>("k: v\n...\n")["k"]);
    }

    [Fact]
    public void BareDashIsIgnored() =>
        Assert.Equal(1, Modes.Compatible.Deserialize<WithCount>("---\nN: 1\n").N);

    [Fact]
    public void WildNullFormsStayStrings() {
        List<(string Yaml, string Word)> cases = [
            ("a: nUll\n", "nUll"),
            ("a: nuLL\n", "nuLL"),
            ("a: NulL\n", "NulL"),
            ("a: nULL\n", "nULL"),
        ];
        Assert.All(cases, c =>
            Assert.Equal(c.Word, Modes.Compatible.Deserialize<Dictionary<string, string>>(c.Yaml)["a"]));
    }

    [Fact]
    public void CapitalNullFormsAreNull() =>
        Assert.All(["a: Null\n", "a: NULL\n"], yaml =>
            Assert.Null(Modes.Compatible.Deserialize<Dictionary<string, string>>(yaml)["a"]));

    [Fact]
    public void OctalLoads() =>
        Assert.Equal(15, Modes.Compatible.Deserialize<WithCount>("N: 0o17\n").N);

    [Fact]
    public void LeadingZeroLoads() =>
        Assert.Equal(1234, Modes.Compatible.Deserialize<WithCount>("N: 01234\n").N);

    [Fact]
    public void PaddedFloatLoads() {
        Assert.Equal(1234d, Modes.Compatible.Deserialize<WithFloat>("X: 01234\n").X);
        Assert.Equal(1234.5d, Modes.Compatible.Deserialize<WithFloat>("X: 01234.5\n").X);
    }

    [Fact]
    public void SpecialDoubleCaseFormsLoad() {
        Assert.Equal(double.PositiveInfinity, Modes.Compatible.Deserialize<WithFloat>("X: .Inf\n").X);
        Assert.Equal(double.PositiveInfinity, Modes.Compatible.Deserialize<WithFloat>("X: .INF\n").X);
        Assert.Equal(double.PositiveInfinity, Modes.Compatible.Deserialize<WithFloat>("X: +.INF\n").X);
        Assert.Equal(double.NegativeInfinity, Modes.Compatible.Deserialize<WithFloat>("X: -.Inf\n").X);
        Assert.True(double.IsNaN(Modes.Compatible.Deserialize<WithFloat>("X: .NaN\n").X));
        Assert.True(double.IsNaN(Modes.Compatible.Deserialize<WithFloat>("X: .NAN\n").X));
    }

    [Fact]
    public void CapitalBoolLoads() {
        Assert.True(Modes.Compatible.Deserialize<FlagRec>("On: True\n").On);
        Assert.True(Modes.Compatible.Deserialize<FlagRec>("On: TRUE\n").On);
        Assert.False(Modes.Compatible.Deserialize<FlagRec>("On: False\n").On);
        Assert.False(Modes.Compatible.Deserialize<FlagRec>("On: FALSE\n").On);
    }

    [Fact]
    public void TabInContentLoads() {
        Assert.Equal("a", Modes.Compatible.Deserialize<Dictionary<string, string>>("k:\ta\n")["k"]);
        Assert.Equal("a\tb", Modes.Compatible.Deserialize<Dictionary<string, string>>("k: a\tb\n")["k"]);
        Assert.Equal("a\tb", Modes.Compatible.Deserialize<Dictionary<string, string>>("k: \"a\tb\"\n")["k"]);
        Assert.Equal("a\tb\n", Modes.Compatible.Deserialize<Dictionary<string, string>>("k: |\n  a\tb\n")["k"]);
    }

    [Fact]
    public void TabAfterIndentStaysContent() =>
        Assert.Equal("\ta\n", Modes.Compatible.Deserialize<Dictionary<string, string>>("k: |\n  \ta\n")["k"]);

    [Fact]
    public void ForeignTagDirectiveIsIgnored() =>
        Assert.Equal(1, Modes.Compatible.Deserialize<WithCount>("%TAG ! other\n---\nN: 1\n").N);

    [Fact]
    public void ValidYamlDirectiveIsIgnored() =>
        Assert.Equal(1, Modes.Compatible.Deserialize<WithCount>("%YAML 1.2\n---\nN: 1\n").N);

    [Fact]
    public void UnknownDirectiveIsIgnored() =>
        Assert.Equal(1, Modes.Compatible.Deserialize<WithCount>("%FOO bar baz\n---\nN: 1\n").N);

    [Fact]
    public void FlatSequenceLoads() {
        var block = Modes.Compatible.Deserialize<Dictionary<string, List<string>>>("key:\n- a\n- b\n");
        Assert.Equal(["a", "b"], block["key"]);
    }

    [Fact]
    public void FlatSequenceOfMapsLoads() {
        var block = Modes.Compatible.Deserialize<Dictionary<string, List<Dictionary<string, string>>>>(
            "key:\n- a: 1\n  b: 2\n- c: 3\n");
        Assert.Equal("1", block["key"][0]["a"]);
        Assert.Equal("3", block["key"][1]["c"]);
    }

    [Fact]
    public void FlatSequenceKeepsSiblings() {
        var block = Modes.Compatible.Deserialize<Dictionary<string, object>>("key:\n- a\nother: 1\n");
        Assert.Equal(["a"], ((List<object>)block["key"]).Cast<string>());
        Assert.Equal("1", block["other"]);
    }
}
