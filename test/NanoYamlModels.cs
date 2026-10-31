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

class WithPrivateField {
    [Serialize]
    string secret = "";
    public string Name = "";

    public string ReadSecret() =>
        secret;
}

class WithPrivateProperty {
    [Serialize]
    string Secret { get; set; } = "";
    public string Name { get; set; } = "";

    public string ReadSecret() =>
        Secret;
}

class WithUnmarkedPrivate {
    string secret = "";
    public string Name = "";
}

class StringBag : ICollection<string> {
    readonly List<string> items = [];

    public int Count =>
        items.Count;

    public bool IsReadOnly =>
        false;

    public void Add(string item) =>
        items.Add(item);

    public void Clear() =>
        items.Clear();

    public bool Contains(string item) =>
        items.Contains(item);

    public bool Remove(string item) =>
        items.Remove(item);

    public IEnumerator<string> GetEnumerator() =>
        items.GetEnumerator();

    void ICollection<string>.CopyTo(string[] array, int arrayIndex) =>
        items.CopyTo(array, arrayIndex);

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}

class StringMap : IDictionary<string, List<int>> {
    readonly Dictionary<string, List<int>> items = [];

    public int Count =>
        items.Count;

    public bool IsReadOnly =>
        false;

    public ICollection<string> Keys =>
        items.Keys;

    public ICollection<List<int>> Values =>
        items.Values;

    public List<int> this[string key] {
        get => items[key];
        set => items[key] = value;
    }

    public void Add(string key, List<int> value) =>
        items.Add(key, value);

    public void Add(KeyValuePair<string, List<int>> entry) =>
        items.Add(entry.Key, entry.Value);

    public void Clear() =>
        items.Clear();

    public bool Contains(KeyValuePair<string, List<int>> entry) =>
        ((ICollection<KeyValuePair<string, List<int>>>)items).Contains(entry);

    public bool ContainsKey(string key) =>
        items.ContainsKey(key);

    public IEnumerator<KeyValuePair<string, List<int>>> GetEnumerator() =>
        items.GetEnumerator();

    public bool Remove(string key) =>
        items.Remove(key);

    public bool Remove(KeyValuePair<string, List<int>> entry) =>
        ((ICollection<KeyValuePair<string, List<int>>>)items).Remove(entry);

    public bool TryGetValue(string key, out List<int> value) =>
        items.TryGetValue(key, out value);

    void ICollection<KeyValuePair<string, List<int>>>.CopyTo(KeyValuePair<string, List<int>>[] array, int arrayIndex) =>
        ((ICollection<KeyValuePair<string, List<int>>>)items).CopyTo(array, arrayIndex);

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
