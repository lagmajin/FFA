namespace FFA.Models;

public class Accessory
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Effect { get; set; } = ""; // e.g. "+5% Magic"
    public Rarity Rarity { get; set; } = Rarity.Common;
    public int EnhancementLevel { get; set; } = 0;
}
