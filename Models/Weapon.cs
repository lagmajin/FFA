namespace FFA.Models;

public enum WeaponType
{
    Sword, // 剣
    Axe, // 斧
    Spear, // 槍
    Katana, // 刀
    Dagger, // 短剣
    Club, // 棍棒
    Bow, // 弓
    Staff // 杖
}

public class Weapon
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Attack { get; set; }
    public Rarity Rarity { get; set; } = Rarity.Common;
    public int EnhancementLevel { get; set; } = 0; // 強化レベル
    public WeaponType Type { get; set; } = WeaponType.Sword; // 武器タイプ

    // 装備可能条件
    public int RequiredStr { get; set; } = 0; // 必要力
    public int RequiredDex { get; set; } = 0; // 必要器用さ
    public int RequiredInt { get; set; } = 0; // 必要知力
    public int RequiredVit { get; set; } = 0; // 必要体力
    public int RequiredAgi { get; set; } = 0; // 必要敏捷性
    public int RequiredLuk { get; set; } = 0; // 必要運
}
