using System.Collections.Generic;

namespace Vaxquis.NanoYaml.Bench;

public enum Rarity {
    Common,
    Rare,
    Epic,
    Legendary,
}

public enum Element {
    None,
    Fire,
    Water,
    Earth,
    Air,
}

public enum Faction {
    Neutral,
    North,
    South,
    East,
    West,
}

public record Position(int X, int Y, int Z) {
    public Position() : this(0, 0, 0) {
    }
}

public record Effect(string Name, Element Element, Rarity Rarity, int Power, double Duration, int? Charges) {
    public Effect() : this("", Element.None, Rarity.Common, 0, 0, null) {
    }
}

public class Stats {
    public int Health { get; set; }
    public int Mana { get; set; }
    public double Speed { get; set; }
    public bool Flying { get; set; }
}

public class Item {
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public Rarity Rarity { get; set; }
    public Element Element { get; set; }
    public int Count { get; set; }
    public double Weight { get; set; }
    public bool Stackable { get; set; }
    public int? Level { get; set; }
    public List<string> Tags { get; set; } = [];
    public Dictionary<string, int> Modifiers { get; set; } = [];
    public Effect? Enchantment { get; set; }
}

public class Unit {
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public long Seed { get; set; }
    public decimal? Cost { get; set; }
    public Faction Faction { get; set; }
    public Element Affinity { get; set; }
    public Position At { get; set; } = new();
    public Stats Stats { get; set; } = new();
    public List<Effect> Effects { get; set; } = [];
    public List<Item> Items { get; set; } = [];
    public Dictionary<string, string?> Labels { get; set; } = [];
    public List<Unit> Minions { get; set; } = [];
}

public class Sample {
    public long Tick { get; set; }
    public int Channel { get; set; }
    public double Value { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public float Gain { get; set; }
    public decimal Price { get; set; }
    public double? Error { get; set; }
}

public class Series {
    public int Id { get; set; }
    public double Rate { get; set; }
    public List<double> Values { get; set; } = [];
    public List<int> Counts { get; set; } = [];
    public List<Sample> Samples { get; set; } = [];
}

public class Numbers {
    public long Version { get; set; }
    public List<Series> Series { get; set; } = [];
}

public class World {
    public string Title { get; set; } = "";
    public int Version { get; set; }
    public Dictionary<string, Item> Catalog { get; set; } = [];
    public List<Unit> Units { get; set; } = [];
}
