namespace FFA.Models;

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int BonusStat { get; set; } // 国によるステータスBonus
    public string BonusType { get; set; } = ""; // "attack", "defense", "hp", "speed", "all"
    public string CapitalName { get; set; } = ""; // 首都の名前
    public string CapitalDescription { get; set; } = ""; // 首都の説明
    public string VillageName { get; set; } = ""; // 田舎の名前
    public string VillageDescription { get; set; } = ""; // 田舎の説明
    public List<Town> Towns { get; set; } = new List<Town>(); // 国の所有する街々
}
