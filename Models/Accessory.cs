namespace FFA.Models;

public class Accessory
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string OwnerUsername { get; set; } = ""; // 所有者
    public string Type { get; set; } = ""; // 装飾品タイプ（リング、アミュレット、イアリング、ブレスレット等）
    public string Effect { get; set; } = ""; // 効果説明（例: "+5% Magic"）
    public int Attack { get; set; } // 攻撃力
    public int Defense { get; set; } // 防御力
    public int Magic { get; set; } // 魔法力
    public Rarity Rarity { get; set; } = Rarity.White;
    public int Price { get; set; } = 0; // 販売価格
    public double Weight { get; set; } = 0.1; // 重量（kg）
    public int EnhancementLevel { get; set; } = 0;

    // 耐久度関連
    public int Durability { get; set; } = 100;
    public int CurrentDurability { get; set; } = 100;

    // 特殊効果
    public List<string> SpecialEffects { get; set; } = new();
    public int SpecialEffectPower { get; set; } = 0;

    // 強化関連
    public int MaxEnhancementLevel { get; set; } = 10;
    public int EnhancementCost { get; set; } = 100;
    public double EnhancementSuccessRate { get; set; } = 0.8;

    // 修理関連
    public int RepairCost { get; set; } = 50;
    public int RepairAmount { get; set; } = 20;

    // 装備可能条件
    public int RequiredStr { get; set; } = 0;
    public int RequiredDex { get; set; } = 0;
    public int RequiredInt { get; set; } = 0;
    public int RequiredVit { get; set; } = 0;
    public int RequiredAgi { get; set; } = 0;
    public int RequiredLuk { get; set; } = 0;
}
