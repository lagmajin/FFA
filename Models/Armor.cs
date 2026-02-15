namespace FFA.Models;

public class Armor
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Defense { get; set; }
    public Rarity Rarity { get; set; } = Rarity.Common;
    public int EnhancementLevel { get; set; } = 0;
}
