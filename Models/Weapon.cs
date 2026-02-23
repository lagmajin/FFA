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
    public string OwnerUsername { get; set; } = ""; // 所有者
    public int Attack { get; set; }
    public Rarity Rarity { get; set; } = Rarity.White;
    public int Price { get; set; } = 0; // 販売価格
    public double Weight { get; set; } = 1.0; // 重量（kg）
public int EnhancementLevel { get; set; } = 0; // 強化レベル
public WeaponType Type { get; set; } = WeaponType.Sword; // 武器タイプ

// 耐久度関連
public int Durability { get; set; } = 100; // 耐久度 (最大100)
public int CurrentDurability { get; set; } = 100; // 現在の耐久度

// 特殊効果
public List<string> SpecialEffects { get; set; } = new(); // 特殊効果リスト
public int SpecialEffectPower { get; set; } = 0; // 特殊効果の強さ

// 分解関連
public List<Material> DismantleMaterials { get; set; } = new(); // 分解で得られる素材
public int DismantleExperience { get; set; } = 0; // 分解で得られる経験値

// 合成関連
public List<Weapon> RequiredWeaponsForSynthesis { get; set; } = new(); // 合成に必要な武器
public int SynthesisExperience { get; set; } = 0; // 合成で得られる経験値

// 装備可能条件
public int RequiredStr { get; set; } = 0; // 必要力

// 強化関連
public int MaxEnhancementLevel { get; set; } = 10; // 最大強化レベル
public int EnhancementCost { get; set; } = 100; // 強化費用
public double EnhancementSuccessRate { get; set; } = 0.8; // 強化成功率
public bool IsEnhancementSafe { get; set; } = false; // 安全強化かどうか

// 修理関連
public int RepairCost { get; set; } = 50; // 修理費用
public int RepairAmount { get; set; } = 20; // 修理で回復する耐久度
    public int RequiredDex { get; set; } = 0; // 必要器用さ
    public int RequiredInt { get; set; } = 0; // 必要知力
    public int RequiredVit { get; set; } = 0; // 必要体力
    public int RequiredAgi { get; set; } = 0; // 必要敏捷性
    public int RequiredLuk { get; set; } = 0; // 必要運
}
