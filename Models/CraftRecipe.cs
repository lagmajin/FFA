using LiteDB;

namespace FFA.Models;

/// <summary>
/// クラフトレシピ
/// </summary>
public class CraftRecipe
{
    public int Id { get; set; }
    
    // レシピ名
    public string Name { get; set; } = "";
    
    // 説明
    public string Description { get; set; } = "";
    
    // カテゴリ（武器、防具、装飾品、消耗品、素材）
    public CraftCategory Category { get; set; } = CraftCategory.Consumable;
    
    // 必要素材リスト
    public List<CraftMaterial> RequiredMaterials { get; set; } = new();
    
    // 必要ゴールド
    public int RequiredGil { get; set; } = 0;
    
    // 必要クラフトレベル
    public int RequiredCraftLevel { get; set; } = 1;
    
    // 作成結果アイテム
    public CraftResult ResultItem { get; set; } = new();
    
    // 作成成功率（0.0-1.0）
    public double SuccessRate { get; set; } = 1.0;
    
    // 経験値報酬
    public int ExpReward { get; set; } = 10;
    
    // 作成時間（秒）
    public int CraftTime { get; set; } = 0;
    
    // 習得条件（クエストIDなど）
    public int? UnlockQuestId { get; set; }
    
    // レシピのレアリティ
    public int Rarity { get; set; } = 1;
}

/// <summary>
/// クラフトカテゴリ
/// </summary>
public enum CraftCategory
{
    /// <summary>武器</summary>
    Weapon = 0,
    /// <summary>防具</summary>
    Armor = 1,
    /// <summary>装飾品</summary>
    Accessory = 2,
    /// <summary>消耗品</summary>
    Consumable = 3,
    /// <summary>素材</summary>
    Material = 4,
    /// <summary>料理</summary>
    Cooking = 5
}

/// <summary>
/// クラフト素材
/// </summary>
public class CraftMaterial
{
    // 素材名
    public string MaterialName { get; set; } = "";
    
    // 素材タイプ（Material, Fish, Consumable など）
    public string MaterialType { get; set; } = "Material";
    
    // 必要数量
    public int Quantity { get; set; } = 1;
}

/// <summary>
/// クラフト結果アイテム
/// </summary>
public class CraftResult
{
    // アイテム名
    public string ItemName { get; set; } = "";
    
    // アイテムタイプ（Weapon, Armor, Accessory, Consumable, Material）
    public string ItemType { get; set; } = "";
    
    // 数量
    public int Quantity { get; set; } = 1;
    
    // 武器用: 攻撃力
    public int Attack { get; set; } = 0;
    
    // 防具用: 防御力
    public int Defense { get; set; } = 0;
    
    // 装飾品用: 効果
    public string? Effect { get; set; }
    
    // 消耗品用: 回復量
    public int HealAmount { get; set; } = 0;
    
    // 消耗品用: バフ効果
    public string? BuffEffect { get; set; }
    
    // 消耗品用: バフ継続時間（秒）
    public int BuffDuration { get; set; } = 0;
    
    // 売却価格
    public int SellPrice { get; set; } = 0;
    
    // レアリティ
    public int Rarity { get; set; } = 1;
}

/// <summary>
/// クラフト結果
/// </summary>
public class CraftResultInfo
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = "";
    public CraftResult? CreatedItem { get; set; }
    public int ExpGained { get; set; }
    public int CraftExpGained { get; set; }
    public CraftStaminaInfo? StaminaInfo { get; set; } // スタミナ情報
}

/// <summary>
/// クラフトスタミナ情報
/// </summary>
public class CraftStaminaInfo
{
    public int Current { get; set; }
    public int Max { get; set; }
    public DateTime? NextRecovery { get; set; }
}

/// <summary>
/// プレイヤーのクラフト統計
/// </summary>
public class CraftStats
{
    public int CraftLevel { get; set; }
    public int CraftExp { get; set; }
    public int CraftExpToNext { get; set; }
    public int TotalCrafts { get; set; }
    public int SuccessfulCrafts { get; set; }
    public int FailedCrafts { get; set; }
    public Dictionary<int, int> RecipeCraftCounts { get; set; } = new(); // レシピID -> 作成回数
}

/// <summary>
/// クラフト素材情報
/// </summary>
public class CraftMaterialInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Rarity { get; set; } = 1;
    public int BasePrice { get; set; } = 10;
    public string Source { get; set; } = ""; // 入手方法（採掘、敵ドロップ、釣りなど）
    public List<string> DropSources { get; set; } = new(); // ドロップする敵や場所
}

/// <summary>
/// クラフトスキル
/// </summary>
public class CraftSkill
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int RequiredCraftLevel { get; set; } = 1;
    
    // 効果タイプ
    public CraftSkillType SkillType { get; set; }
    
    // 効果値（成功率UP%、経験値UP%など）
    public int EffectValue { get; set; }
}

/// <summary>
/// クラフトスキルタイプ
/// </summary>
public enum CraftSkillType
{
    /// <summary>成功率UP</summary>
    SuccessRateBonus = 0,
    /// <summary>経験値UP</summary>
    ExpBonus = 1,
    /// <summary>高速クラフト</summary>
    SpeedBonus = 2,
    /// <summary>素材節約</summary>
    MaterialSave = 3,
    /// <summary>高品質作成率UP</summary>
    QualityBonus = 4
}