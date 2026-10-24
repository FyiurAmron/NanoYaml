using System.Collections.Generic;

namespace Vaxquis.NanoYaml.Bench;

public enum Rarity {
    Common,
    Rare,
    Epic,
    Legendary,
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
    public int Count { get; set; }
    public double Weight { get; set; }
    public bool Stackable { get; set; }
    public int? Level { get; set; }
    public List<string> Tags { get; set; } = [];
    public Dictionary<string, int> Modifiers { get; set; } = [];
}

public class Unit {
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public long Seed { get; set; }
    public decimal? Cost { get; set; }
    public Stats Stats { get; set; } = new();
    public List<Item> Items { get; set; } = [];
    public Dictionary<string, string?> Labels { get; set; } = [];
    public List<Unit> Minions { get; set; } = [];
}

public class World {
    public string Title { get; set; } = "";
    public int Version { get; set; }
    public Dictionary<string, Item> Catalog { get; set; } = [];
    public List<Unit> Units { get; set; } = [];
}
