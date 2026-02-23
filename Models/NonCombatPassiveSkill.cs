namespace FFA.Models;

/// <summary>
/// 戦闘以外のパッシブスキルタイプ
/// </summary>
public enum NonCombatPassiveType
{
    // 採掘系
    MiningSpeedBonus,           // 採掘速度ボーナス
    MiningRareFindBonus,        // レア鉱石発見率ボーナス
    MiningStaminaReduce,        // 採掘スタミナ消費減少
    
    // 釣り系
    FishingSpeedBonus,          // 釣り速度ボーナス
    FishingRareFindBonus,       // レア魚発見率ボーナス
    FishingEscapeReduce,        // 魚の逃走率減少
    
    // 探索系
    ExplorationSpeedBonus,      // 探索速度ボーナス
    ExplorationFindBonus,       // 発見率ボーナス
    EncounterRateAdjust,        // エンカウント率調整
    
    // クラフト系
    CraftingSuccessBonus,       // クラフト成功率ボーナス
    CraftingQualityBonus,       // 品質ボーナス
    CraftingMaterialSave,       // 材料節約
    
    // 経済系
    ShopSellPriceBonus,         // 売却価格ボーナス
    ShopBuyPriceDiscount,       // 購入価格割引
    BankInterestBonus,          // 銀行金利ボーナス
    
    // 回復系
    HPRegenBonus,               // HP自然回復ボーナス
    MPRegenBonus,               // MP自然回復ボーナス
    StatusRecoveryBonus,        // 状態異常回復ボーナス
    
    // その他
    DropRateBonus,              // ドロップ率ボーナス
    ExperienceBonus,            // 経験値ボーナス
    GoldBonus,                  // ゴールドボーナス
    TravelSpeedBonus,           // 移動速度ボーナス
    StaminaRegenBonus,          // スタミナ回復ボーナス
}

/// <summary>
/// 戦闘以外のパッシブスキル
/// </summary>
public class NonCombatPassiveSkill
{
    /// <summary>
    /// スキルタイプ
    /// </summary>
    public NonCombatPassiveType Type { get; set; }
    
    /// <summary>
    /// スキル名
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// 説明
    /// </summary>
    public string Description { get; set; } = "";
    
    /// <summary>
    /// 効果値（パーセンテージ）
    /// </summary>
    public double Value { get; set; }
    
    /// <summary>
    /// アイコン
    /// </summary>
    public string Icon { get; set; } = "✨";
    
    public NonCombatPassiveSkill()
    {
    }
    
    public NonCombatPassiveSkill(NonCombatPassiveType type, string name, string description, double value, string icon = "✨")
    {
        Type = type;
        Name = name;
        Description = description;
        Value = value;
        Icon = icon;
    }
}

/// <summary>
/// 戦闘以外のパッシブスキルヘルパー
/// </summary>
public static class NonCombatPassiveSkillHelper
{
    /// <summary>
    /// スキルタイプのデフォルト情報を取得
    /// </summary>
    public static (string Name, string Description, string Icon) GetSkillInfo(NonCombatPassiveType type)
    {
        return type switch
        {
            // 採掘系
            NonCombatPassiveType.MiningSpeedBonus => ("採掘速度UP", "採掘の速度が向上する", "⛏️"),
            NonCombatPassiveType.MiningRareFindBonus => ("鉱脈発見", "レアな鉱石を見つけやすくなる", "💎"),
            NonCombatPassiveType.MiningStaminaReduce => ("採掘持久", "採掘のスタミナ消費が減少する", "💪"),
            
            // 釣り系
            NonCombatPassiveType.FishingSpeedBonus => ("釣り速度UP", "魚がかかるまでの時間が短縮される", "🎣"),
            NonCombatPassiveType.FishingRareFindBonus => ("大物発見", "レアな魚を見つけやすくなる", "🐠"),
            NonCombatPassiveType.FishingEscapeReduce => ("逃走防止", "魚の逃走率が低下する", "🪝"),
            
            // 探索系
            NonCombatPassiveType.ExplorationSpeedBonus => ("探索速度UP", "探索の進行速度が向上する", "🚶"),
            NonCombatPassiveType.ExplorationFindBonus => ("発見の目", "隠されたアイテムを見つけやすくなる", "👁️"),
            NonCombatPassiveType.EncounterRateAdjust => ("気配消し", "エンカウント率が低下する", "🥷"),
            
            // クラフト系
            NonCombatPassiveType.CraftingSuccessBonus => ("匠の技", "クラフトの成功率が向上する", "🔨"),
            NonCombatPassiveType.CraftingQualityBonus => ("品質向上", "クラフト品の品質が向上する", "⭐"),
            NonCombatPassiveType.CraftingMaterialSave => ("材料節約", "クラフトの材料消費が減少する", "📦"),
            
            // 経済系
            NonCombatPassiveType.ShopSellPriceBonus => ("商才", "アイテムの売却価格が上昇する", "💰"),
            NonCombatPassiveType.ShopBuyPriceDiscount => ("交渉術", "ショップの購入価格が割引される", "🛒"),
            NonCombatPassiveType.BankInterestBonus => ("投資の才", "銀行の金利が上昇する", "🏦"),
            
            // 回復系
            NonCombatPassiveType.HPRegenBonus => ("自然治癒", "HPの自然回復量が上昇する", "❤️"),
            NonCombatPassiveType.MPRegenBonus => ("魔力回復", "MPの自然回復量が上昇する", "💙"),
            NonCombatPassiveType.StatusRecoveryBonus => ("抵抗力", "状態異常の自然回復が早くなる", "💚"),
            
            // その他
            NonCombatPassiveType.DropRateBonus => ("幸運", "アイテムドロップ率が上昇する", "🍀"),
            NonCombatPassiveType.ExperienceBonus => ("学習能力", "獲得経験値が上昇する", "📚"),
            NonCombatPassiveType.GoldBonus => ("金運", "獲得ゴールドが上昇する", "🪙"),
            NonCombatPassiveType.TravelSpeedBonus => ("快速移動", "移動速度が上昇する", "👟"),
            NonCombatPassiveType.StaminaRegenBonus => ("体力回復", "スタミナ回復速度が上昇する", "⚡"),
            
            _ => ("不明", "不明なスキル", "❓")
        };
    }
    
    /// <summary>
    /// スキルを作成
    /// </summary>
    public static NonCombatPassiveSkill CreateSkill(NonCombatPassiveType type, double value)
    {
        var (name, description, icon) = GetSkillInfo(type);
        return new NonCombatPassiveSkill(type, name, description, value, icon);
    }
}
