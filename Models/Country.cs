namespace FFA.Models;

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int BonusStat { get; set; } // 国によるステータスBonus
    public string BonusType { get; set; } = ""; // "attack", "defense", "hp"
}
