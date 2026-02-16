namespace FFA.Models;

public class PlayerStatus
{
    // Core attributes
    public int Str { get; set; } = 5; // 力：物理攻撃力、重い武器の装備に必要
    public int Dex { get; set; } = 5; // 器用さ：命中、回避、クリティカル率
    public int Int { get; set; } = 5; // 知力：魔法攻撃力、魔法防御力
    public int Vit { get; set; } = 5; // 体力：HP、物理防御力
    public int Agi { get; set; } = 5; // 敏捷性：行動速度、回避率
    public int Luk { get; set; } = 5; // 運：クリティカル率、ドロップ率、幸運イベントの発生率

    // Equipment slots
    public string? Weapon { get; set; } = null; // 装備中の武器
    public string? Armor { get; set; } = null; // 装備中の防具
    public string? Accessory { get; set; } = null; // 装備中のアクセサリー

    // Equipment bonuses
    public int WeaponAttack { get; set; } = 0; // 装備中の武器の攻撃力
    public int ArmorDefense { get; set; } = 0; // 装備中の防具の防御力
    public string? AccessoryEffect { get; set; } = null; // 装備中のアクセサリーの効果

    // Unspent status points available for allocation
    public int PointsAvailable { get; set; } = 0;
}
