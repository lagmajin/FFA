namespace FFA.Models;

public class Weapon
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Attack { get; set; }
    public Rarity Rarity { get; set; } = Rarity.Common;
    public int EnhancementLevel { get; set; } = 0; // 強化レベル
}
