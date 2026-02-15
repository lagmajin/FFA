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
}
