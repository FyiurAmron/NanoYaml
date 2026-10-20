using System.Collections;
using System.Collections.Generic;

namespace Vaxquis.NanoYaml.Tests;

record SaveLike(int Version, string PlayerName, Dictionary<string, int> Inv, List<string> Dead);

record Help(string Type, string? Text);

record WithId(string Id);

enum Tint {
    None = 0,
    Red = 1,
}

record WithTint(Tint T);

record FlagRec(bool On);

record WithLegacy(ArrayList Items, Hashtable Map, IDictionary Old);

record WithCount(int N);

class Fields {
    public string Name = "";
    public int Count;
}

struct Point {
    public int Left { get; set; }
    public int Top { get; set; }
}

struct Cell {
    public int Left;
    public int Top;

    public Cell() {
        Left = 0;
        Top = 0;
    }
}

class Mixed {
    public string Name { get; set; } = "";
    public int Count;
}

class Poco {
    public string Name { get; set; } = "";
    public int Count;
    public List<string> Tags { get; set; } = [];
    public Poco? Nested { get; set; }
}

record WithOpt(int? N);

record WithFloat(double X);

record Rec(string A, string B);

record MaybeRec(string? A, string? B);

record WithNames(List<string> Names);

record WithIList(IList<string> Items);

record WithList(List<int> Nums);

enum NoZero {
    A = 1,
}

[System.Flags]
enum Flags {
    A = 1,
    B = 2,
}

class BadKey {
    public override string ToString() =>
        "a\nb";
}

record WithNoZero(NoZero N);

record Trimmed(string Name) {
    public string Name { get; init; } = Name.Trim();
}

record Chain(string Name, Chain? Next) {
    public static int Built;
    public string Name { get; init; } = Count(Name);

    static string Count(string name) {
        Built++;
        return name;
    }
}

record WithExtra(string A) {
    public string B { get; set; } = "";
}

record WithComputed(string A) {
    public string Shout => A.ToUpperInvariant();
}

interface IPair;

record PLow(string a, string b) : IPair;

record PLower(string a, string b, string c) : IPair;

record WithMapAndFlag(string A, IDictionary<string, string> M);

interface IShape;

record UpShape(string A) : IShape;

record LowShape(string a) : IShape;

class TwoWays {
    public string A { get; }
    public string B { get; }
    public string C { get; set; } = "";

    public TwoWays(string a, string b) {
        A = a;
        B = b;
    }

    public TwoWays(string a, string b, string c) {
        A = a;
        B = b;
        C = c;
    }
}
