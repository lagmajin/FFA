namespace FFA.Models;

public enum CompanionRarity { Common, Uncommon, Rare, Epic, Legendary }

public class Companion
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerUsername { get; set; } = string.Empty;
    public CompanionRarity Rarity { get; set; } = CompanionRarity.Common;
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;

    // basic stats
    public int Attack { get; set; } = 1;
    public int Defense { get; set; } = 1;
    public int HP { get; set; } = 100;

    // equipped flag
    public bool IsSummoned { get; set; } = false;

    // simple skill list (names)
    public List<string> Skills { get; set; } = new();
}
