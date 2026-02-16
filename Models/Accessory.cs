namespace FFA.Models;

public class Accessory
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Effect { get; set; } = ""; // e.g. "+5% Magic"
    public Rarity Rarity { get; set; } = Rarity.White;
    public int EnhancementLevel { get; set; } = 0;

    // 装備可能条件
    public int RequiredStr { get; set; } = 0; // 必要力
    public int RequiredDex { get; set; } = 0; // 必要器用さ
    public int RequiredInt { get; set; } = 0; // 必要知力
    public int RequiredVit { get; set; } = 0; // 必要体力
    public int RequiredAgi { get; set; } = 0; // 必要敏捷性
    public int RequiredLuk { get; set; } = 0; // 必要運
}
