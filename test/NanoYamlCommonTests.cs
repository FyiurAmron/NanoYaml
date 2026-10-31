using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaxquis.NanoYaml;
using Xunit;

namespace Vaxquis.NanoYaml.Tests;

/// <summary>
/// The documents both modes read the same way. Every test here runs the
/// document through the strict, the compatible and the auto mode and holds
/// them to the same result.
/// </summary>
public class NanoYamlCommonTests {
    [Fact]
    public void HashGluesToTheScalar() {
        var help = Modes.Common<Help>("Type: jukebox\nText: Play a#b loop # loud\n");
        Assert.Equal("Play a#b loop", help.Text);
    }

    [Fact]
    public void BlankLineSurvivesLiteral() {
        var block = Modes.Common<Dictionary<string, string>>("k: |\n  a\n\n  b\n");
        Assert.Equal("a\n\nb\n", block["k"]);
    }

    [Fact]
    public void FoldedBlockFolds() {
        var block = Modes.Common<Dictionary<string, string>>("k: >\n  a\n  b\n");
        Assert.Equal("a b\n", block["k"]);
    }

    [Fact]
    public void IndentSurvivesLiteral() {
        var block = Modes.Common<Dictionary<string, string>>("k: |\n  a\n    b\n");
        Assert.Equal("a\n  b\n", block["k"]);
    }

    [Fact]
    public void TildeStringIsNull() {
        var help = Modes.Common<Help>("Type: x\nText: ~\n");
        Assert.Null(help.Text);
    }

    [Fact]
    public void EmptyStringStaysEmpty() {
        var help = Modes.Common<Help>("Type: x\nText: \"\"\n");
        Assert.Equal("", help.Text);
    }

    [Fact]
    public void SharedDeserializerKeepsModesApart() {
        string body = string.Concat(Enumerable.Range(0, 50).Select(i => $"k{i}: true\n"));
        string strict = $"{Modes.Profile}{body}On: True\n";
        string compatible = $"{body}On: True\n";
        var expected = Assert.Throws<NanoYamlException>(() => NanoYaml.Deserialize<Dictionary<string, bool>>(strict)).Message;
        List<string> outcomes = [
            .. Enumerable.Range(0, 4000).AsParallel().Select(i => {
                if (i % 2 == 0) {
                    return NanoYaml.Deserialize<Dictionary<string, bool>>(compatible)["On"] ? "" : "compatible read wrong";
                }

                try {
                    NanoYaml.Deserialize<Dictionary<string, bool>>(strict);
                    return "strict accepted";
                } catch (NanoYamlException e) {
                    return e.Message == expected ? "" : e.Message;
                }
            }).Where(outcome => outcome.Length > 0).Distinct(),
        ];
        Assert.Empty(outcomes);
    }

    [Fact]
    public void ConstructorKeepsItsNormalization() =>
        Assert.Equal("padded", Modes.Common<Trimmed>("Name: \"  padded  \"\n").Name);

    [Fact]
    public void NestedRecordsConstructOnce() {
        Chain? chain = null;
        for (int i = 0; i < 20; i++) {
            chain = new($"n{i}", chain);
        }

        string yaml = NanoYaml.Serialize(chain);
        Chain.Built = 0;
        var back = NanoYaml.Deserialize<Chain>(yaml);
        Assert.Equal(20, Chain.Built);
        Assert.Equal(chain, back);
    }

    [Fact]
    public void ConstructorLeavesOtherMembersToSetters() {
        var extra = Modes.Common<WithExtra>("A: x\nB: y\n");
        Assert.Equal(("x", "y"), (extra.A, extra.B));
    }

    [Fact]
    public void EmptyStringRoundTrips() {
        string yaml = NanoYaml.Serialize(new Rec("", "y"));
        Assert.Contains("A: \"\"", yaml);
        Assert.Equal(new Rec("", "y"), Modes.Common<Rec>(yaml));
        Assert.Equal("", Modes.Common<string>(NanoYaml.Serialize("")));
    }

    [Fact]
    public void NullLikeStringsRoundTrip() =>
        Assert.All(["~", "null", "Null", "NULL"], word =>
            Assert.Equal(new Rec(word, "y"),
                Modes.Common<Rec>(NanoYaml.Serialize(new Rec(word, "y")))));

    [Fact]
    public void TypedStringsStayQuoted() =>
        Assert.All([
            "42", "-7", "+7", "3.14", ".5", "1e5", "1.", "0x1F", "0o17", "0b101",
            "01234", "1_000", ".inf", "-.inf", ".nan", "true", "True", "FALSE",
            "yes", "no", "on", "off", "y", "n",
        ], word => Assert.Contains($"\"{word}\"", NanoYaml.Serialize(new Rec(word, "y"))));

    [Fact]
    public void TypedFormsStayQuoted() =>
        Assert.All([
            "~", "null", "Null", "NULL", "true", "True", "TRUE", "false", "FALSE",
            "yes", "no", "on", "off", "y", "n", "Y", "N", "42", "-7", "+7", "3.14",
            ".5", "1e5", "1.", "0x1F", "0o17", "0b101", "01234", "1_000", ".inf",
            "-.inf", ".nan", ".INF", ".NaN", "0", "-0", "+0", "0.0", ".0", "1E5",
            "1e-5", "1e+5", "0X1f", "0O17", "0B1", "1__0", "0x_", "1_", "1._",
            "1.2e3", ".5e3", "1_0.0_1", "0.0e0",
        ], word => Assert.Contains($"\"{word}\"", NanoYaml.Serialize(new Rec(word, "y"))));

    [Fact]
    public void PlainWordsStayPlain() =>
        Assert.All([
            "word", "42a", "1.2.3", "truthy", "offset", "yes_please", "inf", "nan",
            "0x", "0xZZ", "1e", "1e+", "+", ".", "..", "0b2", "0o8", "_1",
            "e5", "E5", "nullx", "truex", "offx", "yesx", "nox", "onx", "nul",
            "tru", "fals", "12e", "12e+", "++1", "1-",
        ], word => Assert.Contains($"A: {word}", NanoYaml.Serialize(new Rec(word, "y"))));

    [Fact]
    public void IndicatorStringsStayQuoted() =>
        Assert.All(["-", "--1", "?x", "*x", "!x", "|x", ">x", "@x", "`x"],
            word => Assert.Contains($"\"{word}\"", NanoYaml.Serialize(new Rec(word, "y"))));

    [Fact]
    public void TypedStringsRoundTrip() =>
        Assert.All(["42", "true", "01234", ".inf"], word =>
            Assert.Equal(new Rec(word, "y"),
                Modes.Common<Rec>(NanoYaml.Serialize(new Rec(word, "y")))));

    [Fact]
    public void QuotedNumberStillReadsAsNumber() =>
        Assert.Equal(42, Modes.Common<WithCount>("N: \"42\"\n").N);

    [Fact]
    public void EmptyStringIntoIntThrows() =>
        Modes.CommonFails<WithCount>("N: \"\"\n");

    [Fact]
    public void TildeBoolThrows() =>
        Modes.CommonFails<FlagRec>("On: ~\n");

    [Fact]
    public void DoubledQuoteUnescapes() {
        var block = Modes.Common<Dictionary<string, string>>("k: 'it''s'\n");
        Assert.Equal("it's", block["k"]);
    }

    [Fact]
    public void QuotedCommaSurvivesFlow() {
        var block = Modes.Common<Dictionary<string, List<string>>>("k: [a, \"b,c\"]\n");
        Assert.Equal(["a", "b,c"], block["k"]);
    }

    [Fact]
    public void QuotedColonKeySurvives() {
        var block = Modes.Common<Dictionary<string, string>>("\"a: b\": 1\n");
        Assert.Equal("1", block["a: b"]);
    }

    [Fact]
    public void NullFieldRoundTrips() {
        var back = Modes.Common<MaybeRec>(NanoYaml.Serialize(new MaybeRec(null, "y")));
        Assert.Equal(new MaybeRec(null, "y"), back);
    }

    [Fact]
    public void NewlineFieldRoundTrips() {
        var back = Modes.Common<Rec>(NanoYaml.Serialize(new Rec("a\nb", "x")));
        Assert.Equal(new Rec("a\nb", "x"), back);
    }

    [Fact]
    public void ZeroEnumRoundTrips() {
        var back = Modes.Common<WithNoZero>(NanoYaml.Serialize(new WithNoZero((NoZero)0)));
        Assert.Equal((NoZero)0, back.N);
    }

    [Fact]
    public void CharControlEscapes() {
        string yaml = NanoYaml.Serialize(new Dictionary<string, char> { ["c"] = '\u0001' });
        Assert.Contains("c: \"\\x01\"", yaml);
        Assert.Equal('\u0001', Modes.Common<Dictionary<string, char>>(yaml)["c"]);
    }

    [Fact]
    public void FlaggedEnumQuotesInFlow() {
        var serializer = new NanoYaml.Serializer(new() { AutoFlowMapCount = NanoYaml.Serializer.AlwaysFlow });
        string yaml = serializer.Serialize(new Dictionary<string, Flags> { ["f"] = Flags.A | Flags.B });
        Assert.Contains("\"A, B\"", yaml);
        Assert.Equal(Flags.A | Flags.B, Modes.Common<Dictionary<string, Flags>>(yaml)["f"]);
    }

    [Fact]
    public void UnpairedCharThrows() =>
        Assert.Throws<NanoYamlException>(() =>
            NanoYaml.Serialize(new Dictionary<string, char> { ["c"] = (char)0xD800 }));

    [Fact]
    public void ObjectKeyEscapes() {
        string yaml = NanoYaml.Serialize(new Hashtable { [new BadKey()] = "x" });
        Assert.Contains("\"a\\nb\": x", yaml);
    }

    [Fact]
    public void IntListSerializes() {
        string yaml = NanoYaml.Serialize(new WithList([1, 2]));
        Assert.Equal([1, 2], Modes.Common<WithList>(yaml).Nums);
    }

    [Fact]
    public void FlowListLimitCollapsesShortLists() {
        NanoYaml.Serializer serializer = new(new() { AutoFlowListCount = 1 });
        string yaml = serializer.Serialize(new List<string> { "a" });
        Assert.EndsWith("[a]", yaml);
        Assert.Equal(["a"], Modes.Common<List<string>>(yaml));
    }

    [Fact]
    public void FlowLimitCountsWholeSubtree() {
        NanoYaml.Serializer serializer = new(new() { AutoFlowListCount = 1 });
        string yaml = serializer.Serialize(new List<List<string>> { new() { "a" } });
        Assert.Contains("- [a]", yaml);
        Assert.Equal(["a"], Modes.Common<List<List<string>>>(yaml)[0]);
    }

    [Fact]
    public void FlowMapLimitCollapsesShortMaps() {
        NanoYaml.Serializer serializer = new(new() { AutoFlowMapCount = 1 });
        string yaml = serializer.Serialize(new Dictionary<string, string> { ["a"] = "b" });
        Assert.EndsWith("{a: b}", yaml);
        Assert.Equal("b", Modes.Common<Dictionary<string, string>>(yaml)["a"]);
    }

    [Fact]
    public void FlowLimitsArePerType() {
        NanoYaml.Serializer serializer = new(new() { AutoFlowListCount = 10 });
        string yaml = serializer.Serialize(new List<Dictionary<string, string>> { new() { ["a"] = "b" } });
        Assert.DoesNotContain("[", yaml);
        Assert.DoesNotContain("{", yaml);
        Assert.Equal("b", Modes.Common<List<Dictionary<string, string>>>(yaml)[0]["a"]);
    }

    [Fact]
    public void AlwaysFlowSentinelFlowsEverything() {
        NanoYaml.Serializer serializer = new(new() {
            AutoFlowListCount = NanoYaml.Serializer.AlwaysFlow,
            AutoFlowMapCount = NanoYaml.Serializer.AlwaysFlow,
        });
        string yaml = serializer.Serialize(new List<Dictionary<string, List<string>>> { new() { ["a"] = ["b"] } });
        Assert.EndsWith("[{a: [b]}]", yaml);
        Assert.Equal("b", Modes.Common<List<Dictionary<string, List<string>>>>(yaml)[0]["a"][0]);
    }

    [Fact]
    public void EmptyCollectionsWithZeroFlowLimitsEmitExplicitFlowNodes() {
        NanoYaml.Serializer serializer = new();
        Assert.EndsWith("[]", serializer.Serialize(new List<object>()));
        Assert.EndsWith("{}", serializer.Serialize(new Dictionary<string, object>()));
        Assert.EndsWith("nested: []", serializer.Serialize(
            new Dictionary<string, object> { ["nested"] = new List<object>() }));
        Assert.EndsWith("- {}", serializer.Serialize(
            new List<object> { new Dictionary<string, object>() }));
    }

    [Fact]
    public void FlowQuotesSeparators() {
        NanoYaml.Serializer serializer = new(new() { AutoFlowListCount = 1 });
        string yaml = serializer.Serialize(new List<string> { "a,b" });
        Assert.EndsWith("[\"a,b\"]", yaml);
        Assert.Equal(["a,b"], Modes.Common<List<string>>(yaml));
    }

    [Fact]
    public void ScalarTargetRejectsMapping() =>
        Modes.CommonFails<int>("k: 1\n");

    [Fact]
    public void PaddedRoundTrips() {
        var back = Modes.Common<Rec>(NanoYaml.Serialize(new Rec("  x  ", "y")));
        Assert.Contains("\"  x  \"", NanoYaml.Serialize(new Rec("  x  ", "y")));
        Assert.Equal(new Rec("  x  ", "y"), back);
    }

    [Fact]
    public void DashDocThrows() =>
        Modes.CommonFails<string>("- foo");

    [Fact]
    public void LoadErrorsCarryTheirLine() {
        Dictionary<string, int> cases = new() {
            ["k: |\n\ta\n"] = 2,
            ["a: 1\nbad line\n"] = 2,
            ["%YAML 1.2.3\n---\nN: 1\n"] = 1,
            ["a: 1\na: 2\n"] = 2,
            ["k: [a\n"] = 1,
        };
        Assert.All(cases, c => {
            Assert.Contains(
                $"line {c.Value}",
                Assert.Throws<NanoYamlException>(
                    () => Modes.Auto.Deserialize<Dictionary<string, string>>(c.Key)).Message);
            Assert.Contains(
                $"line {c.Value + 2}",
                Assert.Throws<NanoYamlException>(
                    () => Modes.Strict.Deserialize<Dictionary<string, string>>(Modes.Profile + c.Key)).Message);
        });
    }

    [Fact]
    public void MissingMapDefaultsEmpty() {
        var rec = Modes.Common<WithMapAndFlag>("A: x\n");
        Assert.Equal([], rec.M);
    }

    [Fact]
    public void AmbiguousShapeThrows() =>
        Modes.CommonFails<IPair>("a: 1\nb: 2\n");

    [Fact]
    public void BadBlockHeaderThrows() =>
        Assert.All(["k: | foo\n", "k: |0\n"], Modes.CommonFails<Dictionary<string, string>>);

    [Fact]
    public void NullListRoundTrips() {
        string yaml = NanoYaml.Serialize(new WithNames(["a", null!]));
        Assert.Contains("- ~", yaml);
        Assert.Equal(["a", null], Modes.Common<WithNames>(yaml).Names);
    }

    [Fact]
    public void NullMapValueRoundTrips() {
        string yaml = NanoYaml.Serialize(new Dictionary<string, string?> { ["a"] = null, ["b"] = "x" });
        Assert.Contains("a: ~", yaml);
        var back = Modes.Common<Dictionary<string, string?>>(yaml);
        Assert.Null(back["a"]);
        Assert.Equal("x", back["b"]);
    }

    [Fact]
    public void RootNullRoundTrips() {
        string yaml = NanoYaml.Serialize(null);
        Assert.Null(Modes.CommonRaw(yaml));
        Assert.Null(Modes.Common<string?>(yaml));
        Assert.Null(Modes.Auto.ParseRaw("~"));
        Assert.EndsWith("---\n~", NanoYaml.Serializer.Default.Serialize(null));
    }

    [Fact]
    public void SpecNullForms() {
        Assert.Null(Modes.Common<Dictionary<string, string>>("a: ~\n")["a"]);
        Assert.Null(Modes.Common<Dictionary<string, string>>("a: null\n")["a"]);
    }

    [Fact]
    public void DeepNestingThrows() {
        var yaml = string.Concat(Enumerable.Range(0, 200).Select(i => $"{new string(' ', i * 2)}k{i}:\n"));
        Modes.CommonFails<Dictionary<string, object>>(yaml);
    }

    [Fact]
    public void BareDashThrowsMarked() =>
        Assert.All(["-\n", "- \n"], Modes.CommonFails<Dictionary<string, object>>);

    [Fact]
    public void NestedFlowListsLoad() {
        var list = Modes.Common<Dictionary<string, List<object>>>("k: [[a], b]\n")["k"];
        Assert.Equal("a", ((List<object>)list[0])[0]);
        Assert.Equal("b", list[1]);
    }

    [Fact]
    public void FlowMapsLoad() {
        var map = Modes.Common<Dictionary<string, Dictionary<string, int>>>("k: {a: 1, b: 2}\n")["k"];
        Assert.Equal(1, map["a"]);
        Assert.Equal(2, map["b"]);
    }

    [Fact]
    public void FlowMixedNestingLoads() {
        var list = Modes.Common<Dictionary<string, List<object>>>("k: [{el1: el11}, el2]\n")["k"];
        Assert.Equal("el11", ((Dictionary<object, object>)list[0])["el1"]);
        Assert.Equal("el2", list[1]);
    }

    [Fact]
    public void FlowSingletonMapsLoad() {
        var list = Modes.Common<Dictionary<string, List<object>>>("k: [a: b, c]\n")["k"];
        Assert.Equal("b", ((Dictionary<object, object>)list[0])["a"]);
        Assert.Equal("c", list[1]);
    }

    [Fact]
    public void FlowMapsNest() {
        var root = (Dictionary<object, object>)Modes.Common<Dictionary<string, object>>("k: {a: {b: 1}, c: [x, {d: y}]}\n")["k"];
        Assert.Equal("1", ((Dictionary<object, object>)root["a"])["b"]);
        var seq = (List<object>)root["c"];
        Assert.Equal("x", seq[0]);
        Assert.Equal("y", ((Dictionary<object, object>)seq[1])["d"]);
        var solo = (Dictionary<object, object>)Modes.Common<Dictionary<string, object>>("k: {x: 1}\n")["k"];
        Assert.Equal("1", solo["x"]);
    }

    [Fact]
    public void FlowEmptyValuesThrowMarked() =>
        Assert.All([
            "k: {a: }\n",
            "k: {a, b}\n",
            "k: {a:b}\n",
            "k: {a: , b: \"\"}\n",
            "k: [a: , b]\n",
        ], Modes.CommonFails<Dictionary<string, object>>);

    [Fact]
    public void FlowTrailingCommaAndEmptyLoad() {
        Assert.Equal(["a", "b"], Modes.Common<Dictionary<string, List<string>>>("k: [a, b, ]\n")["k"]);
        Assert.Empty(Modes.Common<Dictionary<string, List<string>>>("k: []\n")["k"]);
        Assert.Empty(Modes.Common<Dictionary<string, Dictionary<string, string>>>("k: {}\n")["k"]);
    }

    [Fact]
    public void FlowDupKeyThrows() =>
        Modes.CommonFails<Dictionary<string, object>>("k: {a: 1, a: 2}\n");

    [Fact]
    public void FlowSpecialsThrowMarked() =>
        Assert.All(["k: [&a]\n", "k: {a: *b}\n"], Modes.CommonFails<Dictionary<string, object>>);

    [Fact]
    public void FlowQuotedSpecialsSurvive() {
        var list = Modes.Common<Dictionary<string, List<string>>>("k: [\"&a\", '*b']\n")["k"];
        Assert.Equal(["&a", "*b"], list);
    }

    [Fact]
    public void FlowStrayColonThrows() =>
        Modes.CommonFails<Dictionary<string, object>>("k: {a: b: c}\n");

    [Fact]
    public void FlowBlockItemsLoad() {
        var list = Modes.Common<List<object>>("- [a, b]\n- c\n");
        Assert.Equal(["a", "b"], ((List<object>)list[0]).Cast<string>());
        Assert.Equal("c", list[1]);
    }

    [Fact]
    public void FlowDepthThrows() {
        var deserializer = new NanoYaml.Deserializer(new() { MaxDepth = 4 });
        Assert.Throws<NanoYamlException>(() => deserializer.Deserialize<Dictionary<string, object>>("k: [[[[[a]]]]]\n"));
    }

    [Fact]
    public void ComplexFlowKeyThrows() =>
        Modes.CommonFails<Dictionary<string, object>>("k: {{a: 1}, {b: 2}}\n");

    [Fact]
    public void SpecialScalarsThrowMarked() =>
        Assert.All(["k: &a v\n", "k: *a\n", "k: !!str v\n"], Modes.CommonFails<Dictionary<string, string>>);

    [Fact]
    public void QuotedSpecialsSurvive() {
        Assert.Equal("&a", Modes.Common<Dictionary<string, string>>("k: \"&a\"\n")["k"]);
        Assert.Equal("*a", Modes.Common<Dictionary<string, string>>("k: \"*a\"\n")["k"]);
    }

    [Fact]
    public void ColonScalarsStayScalars() {
        var list = Modes.Common<List<string>>("- \"a: b\"\n- http://x\n");
        Assert.Equal(["a: b", "http://x"], list);
    }

    [Fact]
    public void StrayLineThrows() =>
        Modes.CommonFails<Dictionary<string, string>>("a: 1\noops\n");

    [Fact]
    public void IndentedItemThrows() =>
        Modes.CommonFails<Dictionary<string, string>>("  - x\n");

    [Fact]
    public void MixedBlockThrows() =>
        Modes.CommonFails<Dictionary<string, string>>("a: 1\n- x\n");

    [Fact]
    public void DuplicateKeyThrows() =>
        Modes.CommonFails<Dictionary<string, string>>("a: 1\na: 2\n");

    [Fact]
    public void UnterminatedInputThrows() =>
        Assert.All([
            "k: \"abc\n",
            "k: [a\n",
            "k: [a], b]\n",
        ], Modes.CommonFails<Dictionary<string, string>>);

    [Fact]
    public void EscapedQuoteHidesComment() {
        var block = Modes.Common<Dictionary<string, string>>("k: \"a\\\"b\" # c\n");
        Assert.Equal("a\"b", block["k"]);
    }

    [Fact]
    public void EnumTakesNamesAndNumbers() {
        Assert.Equal(Tint.Red, Modes.Common<WithTint>("T: Red\n").T);
        Assert.Equal((Tint)2, Modes.Common<WithTint>("T: 2\n").T);
    }

    [Fact]
    public void UnknownEnumNameThrowsMarked() =>
        Modes.CommonFails<WithTint>("T: Bogus\n");

    [Fact]
    public void ShapeMismatchIsLoadError() =>
        Modes.CommonFails<Dictionary<string, string>>("- a\n- b\n");

    [Fact]
    public void CasePicksTheExactShape() {
        var shape = Modes.Common<IShape>("a: 1\n");
        Assert.IsType<LowShape>(shape);
    }

    [Fact]
    public void AmbiguousConstructorThrows() =>
        Modes.CommonFails<TwoWays>("A: x\nB: y\n");

    [Fact]
    public void ComputedGetterSkipped() {
        string yaml = NanoYaml.Serialize(new WithComputed("hi"));
        Assert.DoesNotContain("Shout", yaml);
        Assert.Equal("hi", Modes.Common<WithComputed>(yaml).A);
    }

    [Fact]
    public void ColonKeySurvives() {
        var block = Modes.Common<Dictionary<string, string>>("http://x: 1\n");
        Assert.Equal("1", block["http://x"]);
    }

    [Fact]
    public void PipeValueRoundTrips() {
        string yaml = NanoYaml.Serialize(new Rec("|", "y"));
        Assert.Contains("\"|\"", yaml);
        Assert.Equal(new Rec("|", "y"), Modes.Common<Rec>(yaml));
    }

    [Fact]
    public void FoldIndentBreaks() {
        var block = Modes.Common<Dictionary<string, string>>("k: >\n  a\n    b\n  c\n");
        Assert.Equal("a\n  b\nc\n", block["k"]);
    }

    [Fact]
    public void BomKeyStrips() {
        var block = Modes.Common<Dictionary<string, string>>("\uFEFFk: 1\n");
        Assert.Equal("1", block["k"]);
    }

    [Fact]
    public void ChompStripDropsNewline() {
        var block = Modes.Common<Dictionary<string, string>>("k: |-\n  a\n");
        Assert.Equal("a", block["k"]);
    }

    [Fact]
    public void ChompKeepKeepsNewlines() {
        var block = Modes.Common<Dictionary<string, string>>("k: |+\n  a\n\n");
        Assert.Equal("a\n\n", block["k"]);
    }

    [Fact]
    public void ChompIndentIndicatorLoads() {
        var block = Modes.Common<Dictionary<string, string>>("k: |2\n  a\n");
        Assert.Equal("a\n", block["k"]);
    }

    [Fact]
    public void QuotedNullStays() {
        var help = Modes.Common<Help>("Type: x\nText: \"~\"\n");
        Assert.Equal("~", help.Text);
        var word = Modes.Common<Help>("Type: x\nText: \"null\"\n");
        Assert.Equal("null", word.Text);
    }

    [Fact]
    public void BlockNullStays() {
        var block = Modes.Common<Dictionary<string, string>>("k: |\n  ~\n");
        Assert.Equal("~\n", block["k"]);
    }

    [Fact]
    public void UnknownEscapeThrows() =>
        Modes.CommonFails<Dictionary<string, string>>("k: \"a\\qb\"\n");

    [Fact]
    public void BrokenEscapesThrowMarked() =>
        Assert.All(["k: \"\\xZZ\"\n", "k: \"\\u12\"\n", "k: \"\\U00112233\"\n"], Modes.CommonFails<Dictionary<string, string>>);

    [Fact]
    public void BareDashNests() {
        var list = Modes.Common<List<Dictionary<string, string>>>("- \n  a: 1\n");
        Assert.Equal("1", list[0]["a"]);
    }

    [Fact]
    public void ContinuedPlainJoins() {
        var block = Modes.Common<Dictionary<string, string>>("k: some\n  continued\nnext: 1\n");
        Assert.Equal("some continued", block["k"]);
        Assert.Equal("1", block["next"]);
    }

    [Fact]
    public void ListAffixTrimmed() =>
        Assert.Equal(["x"], Modes.Common<List<string>>("-  x\n"));

    [Fact]
    public void HexLoad() =>
        Assert.Equal(31, Modes.Common<WithCount>("N: 0x1F\n").N);

    [Fact]
    public void PaddedStringKept() =>
        Assert.Equal("01234", Modes.Common<Dictionary<string, string>>("k: 01234\n")["k"]);

    [Fact]
    public void PocoLoadsByProperty() {
        var poco = Modes.Common<Poco>("Name: alpha\nCount: 3\n");
        Assert.Equal("alpha", poco.Name);
        Assert.Equal(3, poco.Count);
    }

    [Fact]
    public void PocoSavesByProperty() {
        string yaml = NanoYaml.Serialize(new Poco { Name = "alpha", Count = 3 });
        Assert.Contains("Name: alpha", yaml);
        Assert.Contains("Count: 3", yaml);
    }

    [Fact]
    public void PocoRoundTrips() {
        Poco poco = new() { Name = "alpha", Count = 3, Tags = ["a", "b"] };
        Poco back = Modes.Common<Poco>(NanoYaml.Serialize(poco));
        Assert.Equal(poco.Name, back.Name);
        Assert.Equal(poco.Count, back.Count);
        Assert.Equal(poco.Tags, back.Tags);
    }

    [Fact]
    public void PocoUnknownKeyThrows() =>
        Modes.CommonFails<Poco>("Name: alpha\nBogus: 1\n");

    [Fact]
    public void PocoNestedLoads() {
        var poco = Modes.Common<Poco>("Name: alpha\nNested:\n  Name: beta\n");
        Assert.Equal("beta", poco.Nested?.Name);
    }

    [Fact]
    public void FieldOnlyClassLoadsAndSaves() {
        var holder = Modes.Common<Fields>("Name: alpha\nCount: 3\n");
        Assert.Equal("alpha", holder.Name);
        Assert.Equal(3, holder.Count);
        string yaml = NanoYaml.Serialize(holder);
        Assert.Contains("Name: alpha", yaml);
        Assert.Contains("Count: 3", yaml);
    }

    [Fact]
    public void FieldOnlyClassRoundTrips() {
        Fields holder = new() { Name = "alpha", Count = 3 };
        Fields back = Modes.Common<Fields>(NanoYaml.Serialize(holder));
        Assert.Equal(holder.Name, back.Name);
        Assert.Equal(holder.Count, back.Count);
    }

    [Fact]
    public void PropertyStructLoadsAndSaves() {
        var point = Modes.Common<Point>("Left: 2\nTop: 3\n");
        Assert.Equal(2, point.Left);
        Assert.Equal(3, point.Top);
        string yaml = NanoYaml.Serialize(point);
        Assert.Contains("Left: 2", yaml);
        Assert.Contains("Top: 3", yaml);
    }

    [Fact]
    public void FieldStructLoadsAndSaves() {
        var cell = Modes.Common<Cell>("Left: 2\nTop: 3\n");
        Assert.Equal(2, cell.Left);
        Assert.Equal(3, cell.Top);
        string yaml = NanoYaml.Serialize(cell);
        Assert.Contains("Left: 2", yaml);
        Assert.Contains("Top: 3", yaml);
    }

    [Fact]
    public void StructRoundTrips() {
        Point point = new() { Left = 2, Top = 3 };
        Point back = Modes.Common<Point>(NanoYaml.Serialize(point));
        Assert.Equal(point, back);
    }

    [Fact]
    public void MixedMembersRoundTrip() {
        Mixed mixed = new() { Name = "alpha", Count = 3 };
        Mixed back = Modes.Common<Mixed>(NanoYaml.Serialize(mixed));
        Assert.Equal(mixed.Name, back.Name);
        Assert.Equal(mixed.Count, back.Count);
    }

    [Fact]
    public void DigitGroupSeparatorThrowsMarked() =>
        Assert.All(["1,000", "1,0", "12,345"], grouped =>
            Modes.CommonFails<WithCount>($"N: {grouped}\n"));

    [Fact]
    public void DigitGroupSeparatorKept() =>
        Assert.Equal("1,000", Modes.Common<Dictionary<string, string>>("k: 1,000\n")["k"]);

    [Fact]
    public void SpecialDoublesUseYamlForms() {
        Assert.Contains("X: .inf", NanoYaml.Serialize(new WithFloat(double.PositiveInfinity)));
        Assert.Contains("X: -.inf", NanoYaml.Serialize(new WithFloat(double.NegativeInfinity)));
        Assert.Contains("X: .nan", NanoYaml.Serialize(new WithFloat(double.NaN)));
    }

    [Fact]
    public void SpecialDoubleFormsRead() {
        Assert.Equal(double.PositiveInfinity, Modes.Common<WithFloat>("X: .inf\n").X);
        Assert.Equal(double.PositiveInfinity, Modes.Common<WithFloat>("X: +.inf\n").X);
        Assert.Equal(double.NegativeInfinity, Modes.Common<WithFloat>("X: -.inf\n").X);
        Assert.True(double.IsNaN(Modes.Common<WithFloat>("X: .nan\n").X));
    }

    [Fact]
    public void NonSpecialDoubleFormsThrow() =>
        Assert.All(["nan", "NaN", "inf", "infinity", "Infinity"], word =>
            Modes.CommonFails<WithFloat>($"X: {word}\n"));

    [Fact]
    public void SpecialDoublesRoundTrip() {
        Assert.Equal(double.PositiveInfinity, Modes.Common<WithFloat>("X: .inf\n").X);
        Assert.Equal(double.NegativeInfinity, Modes.Common<WithFloat>("X: -.inf\n").X);
        Assert.True(double.IsNaN(Modes.Common<WithFloat>("X: .nan\n").X));
    }

    [Fact]
    public void ProfileHeaderLoads() {
        Assert.Equal(1, Modes.Common<WithCount>("%TAG ! nanoyaml:0.1.0:\n---\nN: 1\n").N);
        Assert.Equal(["a"], Modes.Common<List<string>>("%TAG ! nanoyaml:0.1.0:\n---\n- a\n"));
    }

    [Fact]
    public void ProfileHeaderPassesStrict() =>
        Assert.Equal(1, Modes.Strict.Deserialize<WithCount>("%TAG ! nanoyaml:0.1.0:\n---\nN: 1\n").N);

    [Fact]
    public void BadYamlDirectiveThrows() =>
        Assert.All(["%YAML\n", "%YAML x\n", "%YAML 1\n", "%YAML 1.2.3\n"], yaml =>
            Modes.CommonFails<WithCount>($"{yaml}N: 1\n"));

    [Fact]
    public void EmptyDirectiveNameThrowsBadDirective() {
        Dictionary<string, string> cases = new() {
            ["%\n---\na: 1\n"] = "line 1: Bad directive: '%'",
            ["% FOO\n---\na: 1\n"] = "line 1: Bad directive: '% FOO'",
        };
        Assert.All(cases, c => Assert.Contains(
            c.Value,
            Assert.Throws<NanoYamlException>(
                () => Modes.Strict.Deserialize<WithCount>(c.Key)).Message));
    }

    [Fact]
    public void ProfileTagNeedsDash() =>
        Modes.CommonFails<WithCount>("%TAG ! nanoyaml:0.1.0:\nN: 1\n");

    [Fact]
    public void DashWithContentThrows() =>
        Modes.CommonFails<WithCount>("%TAG ! nanoyaml:0.1.0:\n--- x\nN: 1\n");

    [Fact]
    public void RepeatedProfileTagThrows() =>
        Modes.CommonFails<WithCount>("%TAG ! nanoyaml:0.1.0:\n%TAG ! nanoyaml:0.1.0:\n---\nN: 1\n");

    [Fact]
    public void SerializeEmitsProfile() =>
        Assert.StartsWith("%TAG ! nanoyaml:0.1.0:\n---\n", NanoYaml.Serialize(new WithCount(1)));

    [Fact]
    public void MaxDepthIsTunable() {
        Assert.Equal(128, new NanoYaml.DeserializerParams().MaxDepth);
        var deserializer = new NanoYaml.Deserializer(new() { MaxDepth = 4 });
        Assert.Throws<NanoYamlException>(() => deserializer.Deserialize<Dictionary<string, object>>(
            "a:\n  b:\n    c:\n      d:\n        e:\n          f:\n            g: 1\n"));
    }

    [Fact]
    public void NullIntoIntThrows() =>
        Modes.CommonFails<WithCount>("N: ~\n");

    [Fact]
    public void NullIntoNullableIntLoads() {
        Assert.Null(Modes.Common<WithOpt>("N: ~\n").N);
        Assert.Equal(1, Modes.Common<WithOpt>("N: 1\n").N);
    }

    [Fact]
    public void ScalarDocLoads() {
        Assert.Equal("abc", Modes.Common<string>("abc\n"));
        Assert.Equal("a: b", Modes.Common<string>("\"a: b\"\n"));
        Assert.Equal(5, Modes.Common<int>("5\n"));
        Assert.True(Modes.Common<bool>("true\n"));
        Assert.Null(Modes.Common<int?>("~\n"));
    }

    [Fact]
    public void ScalarRoundTrip() =>
        Assert.Equal("x", Modes.Common<string>(NanoYaml.Serialize("x")));

    [Fact]
    public void MultilineFlowRootLoads() {
        Assert.Equal(["a", "b"], Modes.Common<List<string>>("[\n  a,\n  b,\n]\n"));
        Assert.Equal(1, Modes.Common<Dictionary<string, int>>("{\n  a: 1,\n}\n")["a"]);
    }

    [Fact]
    public void MultilineScalarDocLoads() {
        Assert.Equal("abc def", Modes.Common<string>("abc\ndef\n"));
        Modes.CommonFails<Dictionary<string, string>>("abc\ndef\n");
    }

    [Fact]
    public void FlowRootLoads() {
        Assert.Equal(["a", "b"], Modes.Common<List<string>>("[a, b]\n"));
        Assert.Equal("v", Modes.Common<Dictionary<string, string>>("{k: v}\n")["k"]);
        Modes.CommonFails<List<string>>("[a] x\n");
    }

    [Fact]
    public void ExplicitMinusOneLoadsAsMinusOne() =>
        Assert.Equal(-1, Modes.Common<WithCount>("N: -1\n").N);

    [Fact]
    public void UnderscoreLoadThrows() =>
        Modes.CommonFails<WithCount>("N: 1_000\n");

    [Fact]
    public void BrokenNumbersThrowMarked() =>
        Assert.All(["N: abc\n", "N: 0xZZ\n", "N: 99999999999999999999\n"], Modes.CommonFails<WithCount>);

    [Fact]
    public void TabInIndentThrows() =>
        Assert.All(["k: |\n\ta\n", "a:\n\tb: 1\n"], Modes.CommonFails<Dictionary<string, string>>);

    [Fact]
    public void LegacyCollectionsLoad() {
        var rec = Modes.Common<WithLegacy>("Items:\n  - a\nMap:\n  k: v\nOld: {}\n");
        Assert.Equal(["a"], rec.Items.Cast<string>());
        Assert.Equal("v", rec.Map["k"]);
        Assert.IsType<Hashtable>(rec.Old);
        Assert.Empty(rec.Old);
    }

    [Fact]
    public void FloatSpecialLoads() {
        Assert.Equal(double.PositiveInfinity, Modes.Common<WithFloat>("X: .inf\n").X);
        Assert.True(double.IsNaN(Modes.Common<WithFloat>("X: .nan\n").X));
    }

    [Fact]
    public void FloatPlainLoads() =>
        Assert.Equal(0.5, Modes.Common<WithFloat>("X: 0.5\n").X);

    [Fact]
    public void NonPrintRoundTrips() {
        string yaml = NanoYaml.Serialize(new Rec("a\u0085b", "y"));
        Assert.Contains("\\N", yaml);
        Assert.Equal(new Rec("a\u0085b", "y"), Modes.Common<Rec>(yaml));
    }

    [Fact]
    public void NonPrintableCharactersEscape() {
        var cases = new Dictionary<string, string> {
            ["\u0001"] = "\\x01",
            ["\u000E"] = "\\x0E",
            ["\u001F"] = "\\x1F",
            ["\u007F"] = "\\x7F",
            ["\u0080"] = "\\x80",
            ["\u009F"] = "\\x9F",
            ["\uFEFF"] = "\\uFEFF",
            ["\uFFFE"] = "\\uFFFE",
            ["\uFFFF"] = "\\uFFFF",
            ["\U0001F600"] = "\\U0001F600",
        };

        Assert.All(cases, c => {
            string yaml = NanoYaml.Serialize(new Rec(c.Key, "y"));
            Assert.Contains(c.Value, yaml);
            Assert.Equal(new Rec(c.Key, "y"), Modes.Common<Rec>(yaml));
        });
    }

    [Fact]
    public void NonPrintableKeyEscapes() {
        string yaml = NanoYaml.Serialize(new Dictionary<string, string> { ["\u0001"] = "x" });
        Assert.Contains("\\x01", yaml);
        Assert.Equal("x", Modes.Common<Dictionary<string, string>>(yaml)["\u0001"]);
    }

    [Fact]
    public void UnpairedSurrogateThrows() =>
        Assert.All(["\uD800", "\uDC00", "a\uD800b", "a\uDC00b"], value =>
            Assert.Throws<NanoYamlException>(() => NanoYaml.Serialize(new Rec(value, "y"))));

    [Fact]
    public void SerializedCharactersStayPrintable() {
        StringBuilder value = new();
        for (int c = 0; c <= 0xFFFF; c++) {
            if (c is >= 0xD800 and <= 0xDFFF) {
                continue;
            }

            value.Append((char)c);
        }

        value.Append("\U0001F600");
        var rec = new Rec(value.ToString(), "y");
        string yaml = NanoYaml.Serialize(rec);
        Assert.All(yaml, c => Assert.True(IsPrintable(c), $"U+{(int)c:X4} is outside YAML's c-printable set"));
        Assert.Equal(rec, Modes.Common<Rec>(yaml));
    }

    static bool IsPrintable(char c) =>
        c is '\t' or '\n' or '\r' ||
        c is >= ' ' and <= '~' ||
        c == '\u0085' ||
        c is >= '\u00A0' and <= '\uD7FF' ||
        c is >= '\uE000' and <= '\uFFFD';

    [Fact]
    public void HashLineSurvivesLiteral() {
        var block = Modes.Common<Dictionary<string, string>>("k: |\n  a # kept\n  # kept\n");
        Assert.Equal("a # kept\n# kept\n", block["k"]);
    }

    [Fact]
    public void UnknownKeyFailsTheLoad() =>
        Modes.CommonFails<List<WithId>>("- Id: donut\n  Bogus: 1\n");

    [Fact]
    public void SavesRoundTripThroughYaml() {
        SaveLike before = new(1, "Wax", new() { ["donut"] = 2 }, ["slim"]);
        string yaml = NanoYaml.Serialize(before);
        Assert.Contains("PlayerName: Wax", yaml);
        var after = Modes.Common<SaveLike>(yaml);
        Assert.Equal(before.PlayerName, after.PlayerName);
        Assert.Equal(2, after.Inv["donut"]);
        Assert.Equal(["slim"], after.Dead);
    }

    [Fact]
    public void PrivateFieldOptInRoundTrips() {
        var back = Modes.Common<WithPrivateField>("secret: hi\nName: yo\n");
        Assert.Equal("hi", back.ReadSecret());
        Assert.Equal("yo", back.Name);
        string yaml = NanoYaml.Serialize(back);
        Assert.Contains("secret: hi", yaml);
        Assert.Contains("Name: yo", yaml);
    }

    [Fact]
    public void PrivatePropertyOptInRoundTrips() {
        var back = Modes.Common<WithPrivateProperty>("Secret: hi\nName: yo\n");
        Assert.Equal("hi", back.ReadSecret());
        Assert.Equal("yo", back.Name);
        string yaml = NanoYaml.Serialize(back);
        Assert.Contains("Secret: hi", yaml);
        Assert.Contains("Name: yo", yaml);
    }

    [Fact]
    public void UnmarkedPrivateStaysOut() {
        string yaml = NanoYaml.Serialize(new WithUnmarkedPrivate { Name = "yo" });
        Assert.Contains("Name: yo", yaml);
        Assert.DoesNotContain("secret", yaml);
        Modes.CommonFails<WithUnmarkedPrivate>("secret: hi\nName: yo\n");
    }

    [Fact]
    public void GenericSetsRoundTrip() {
        var hashed = Modes.Common<HashSet<string>>("- a\n- b\n");
        Assert.True(hashed.SetEquals(["a", "b"]));
        Assert.Contains("- a", NanoYaml.Serialize(hashed));
        var sorted = Modes.Common<SortedSet<string>>("- b\n- a\n");
        Assert.True(sorted.SetEquals(["a", "b"]));
        Assert.Contains("- a", NanoYaml.Serialize(sorted));
    }

    [Fact]
    public void ReadOnlyCollectionTargetsFill() {
        var setted = Modes.Common<ISet<int>>("- 1\n- 2\n");
        Assert.IsType<HashSet<int>>(setted);
        Assert.True(setted.SetEquals([1, 2]));
        var listed = Modes.Common<IReadOnlyList<int>>("- 1\n- 2\n");
        Assert.IsType<List<int>>(listed);
        Assert.Equal([1, 2], listed);
    }

    [Fact]
    public void CustomCollectionRoundTrips() {
        var back = Modes.Common<StringBag>("- a\n- b\n");
        Assert.True(back.SequenceEqual(["a", "b"]));
        string yaml = NanoYaml.Serialize(back);
        Assert.Contains("- a", yaml);
        Assert.Contains("- b", yaml);
    }

    [Fact]
    public void CustomMapRoundTrips() {
        var back = Modes.Common<StringMap>("a:\n  - 1\n  - 2\n");
        Assert.Equal([1, 2], back["a"]);
        string yaml = NanoYaml.Serialize(back);
        Assert.Contains("- 1", yaml);
        Assert.Contains("- 2", yaml);
    }

    [Fact]
    public void CustomMapNestedFlowCounts() {
        NanoYaml.Serializer serializer = new(new() { AutoFlowMapCount = 10 });
        string yaml = serializer.Serialize(Modes.Common<StringMap>("a:\n  - 1\n"));
        Assert.Contains("- 1", yaml);
    }

    [Fact]
    public void AddLessCollectionFails() =>
        Modes.CommonFails<Queue<int>>("- 1\n");

    public static TheoryData<string, string> RoundTripCases => new() {
        { "map", "a: 1\nb: x\n" },
        { "nested map", "a:\n  b: 1\n  c: x\n" },
        { "list", "- a\n- b\n" },
        { "list of maps", "- a: 1\n  b: 2\n- c: 3\n" },
        { "map of lists", "a:\n  - 1\n  - 2\nb: x\n" },
        { "list of lists", "-\n  - a\n" },
        { "list of lists, two", "-\n  - a\n  - b\n" },
        { "map of list of lists", "a:\n  -\n    - x\n" },
        { "list of map of list", "- a:\n    - x\n" },
        { "flow root list", "[a, b]\n" },
        { "flow root map", "{a: 1, b: 2}\n" },
        { "flow nested", "a: [1, {b: 2}]\n" },
        { "flow list in list", "- [a, b]\n- c\n" },
        { "flow list of lists", "[[a], [b]]\n" },
        { "empty root list", "[]\n" },
        { "empty root map", "{}\n" },
        { "empty list value", "a: []\n" },
        { "empty map value", "a: {}\n" },
        { "empty list item list", "- []\n" },
        { "empty map item list", "- {}\n" },
        { "empty list nested map", "a:\n  b: []\n" },
        { "empty map nested map", "a:\n  b: {}\n" },
        { "empty list in map in list", "- a: []\n" },
        { "empty string", "a: \"\"\n" },
        { "null tilde", "a: ~\n" },
        { "null word", "a: null\n" },
        { "root null", "~\n" },
        { "root empty string", "\"\"\n" },
        { "null list item", "- ~\n" },
        { "empty key", "\"\": 1\n" },
        { "literal", "a: |\n  line1\n  line2\n" },
        { "folded", "a: >\n  line1\n  line2\n" },
        { "chomp strip", "a: |-\n  x\n" },
        { "chomp keep", "a: |+\n  x\n\n" },
        { "literal indent", "a: |\n  a\n    b\n" },
        { "multiline plain", "a: some\n  continued\nb: 1\n" },
        { "value amp", "a: \"&x\"\n" },
        { "value star", "a: \"*x\"\n" },
        { "value bang", "a: \"!x\"\n" },
        { "value question", "a: \"?x\"\n" },
        { "value percent", "a: \"%x\"\n" },
        { "value pipe", "a: \"|x\"\n" },
        { "value fold", "a: \">x\"\n" },
        { "value doc end", "a: \"...\"\n" },
        { "value doc start", "a: \"---\"\n" },
        { "value dash space", "a: \"- x\"\n" },
        { "value dash", "a: \"-\"\n" },
        { "value hash", "a: \"#x\"\n" },
        { "value colon", "a: \"a: b\"\n" },
        { "value comma", "a: \"a,b\"\n" },
        { "value brackets", "a: \"[x]\"\n" },
        { "value braces", "a: \"{x}\"\n" },
        { "value quote", "a: \"a\\\"b\"\n" },
        { "value backslash", "a: \"a\\\\b\"\n" },
        { "value newline", "a: \"a\\nb\"\n" },
        { "value tab", "a: \"a\\tb\"\n" },
        { "value nel", "a: \"a\\u0085b\"\n" },
        { "value nbsp", "a: \"a\\u00A0b\"\n" },
        { "value ls", "a: \"a\\u2028b\"\n" },
        { "value ps", "a: \"a\\u2029b\"\n" },
        { "value nul", "a: \"a\\0b\"\n" },
        { "value padded", "a: \"  x  \"\n" },
        { "value bom", "a: \"\uFEFF\"\n" },
        { "root amp", "\"&x\"\n" },
        { "root star", "\"*x\"\n" },
        { "root bang", "\"!x\"\n" },
        { "root question", "\"?x\"\n" },
        { "root percent", "\"%x\"\n" },
        { "root pipe", "\"|x\"\n" },
        { "root fold", "\">x\"\n" },
        { "root doc end", "\"...\"\n" },
        { "root doc start", "\"---\"\n" },
        { "root dash space", "\"- x\"\n" },
        { "root dash", "\"-\"\n" },
        { "root hash", "\"#x\"\n" },
        { "root colon", "\"a: b\"\n" },
        { "root simple", "\"x\"\n" },
        { "root padded", "\"  x  \"\n" },
        { "root bom", "\"\uFEFF\"\n" },
        { "key dash space", "\"- k\": 1\n" },
        { "key dash", "\"-\": 1\n" },
        { "key amp", "\"&k\": 1\n" },
        { "key star", "\"*k\": 1\n" },
        { "key bang", "\"!k\": 1\n" },
        { "key question", "\"?k\": 1\n" },
        { "key percent", "\"%k\": 1\n" },
        { "key yaml directive", "\"%YAML\": 1\n" },
        { "key tag directive", "\"%TAG\": 1\n" },
        { "key pipe", "\"|k\": 1\n" },
        { "key fold", "\">k\": 1\n" },
        { "key doc start", "\"---\": 1\n" },
        { "key doc end", "\"...\": 1\n" },
        { "key hash", "\"#k\": 1\n" },
        { "key colon", "\"a: b\": 1\n" },
        { "key empty", "\"\": 1\n" },
        { "key padded", "\"  k  \": 1\n" },
        { "key bom", "\"\uFEFF\": 1\n" },
        { "unicode literal", "a: caf\u00E9\n" },
        { "emoji", "a: \"\U0001F600\"\n" },
        { "comments", "# top\na: 1 # x\n" },
        { "header", "%TAG ! nanoyaml:0.1.0:\n---\na: 1\n" },
    };

    [Theory]
    [MemberData(nameof(RoundTripCases))]
    public void RoundTripsSemantically(string name, string yaml) {
        var before = Modes.CommonRaw(yaml);
        var output = NanoYaml.Serialize(before);
        var after = Modes.CommonRaw(output);
        Assert.True(Modes.Same(before, after), $"{name}: {Modes.Show(before)} -> {Modes.Show(output)} -> {Modes.Show(after)}");
    }
}
