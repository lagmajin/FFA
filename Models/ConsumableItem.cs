namespace FFA.Models;

/// <summary>
/// 消耗品タイプ
/// </summary>
public enum ConsumableType
{
    HPHeal,      // HP回復
    MPHeal,      // MP回復（将来用）
    StaminaHeal, // スタミナ回復
    Buff,        // バフ
    Debuff,      // デバフ解除
    Teleport,    // テレポート
    Key,         // 鍵/キーアイテム
    Quest,       // クエストアイテム
    Other        // その他
}

/// <summary>
/// 消耗品アイテム
/// </summary>
public class ConsumableItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public ConsumableType Type { get; set; }
    public int Price { get; set; }  // 販売価格
    public int UseLevel { get; set; } = 1;  // 使用可能レベル
    
    // 回復量等
    public int HealAmount { get; set; }  // 回復量（HP/MP等）
    public int HealPercent { get; set; }  // 回復率（最大HPの○%）
    
    // スタミナ回復
    public int StaminaHealAmount { get; set; }
    public StaminaType? StaminaType { get; set; }  // 哪种スタミナ
    
    // バフ効果
    public string? BuffType { get; set; }  // "Attack", "Defense", "Speed" etc.
    public int BuffAmount { get; set; }
    public int BuffDurationMinutes { get; set; }
    
    // テレポート
    public string? TeleportLocation { get; set; }  // "Town", "Home", etc.
    
    // 使用条件
    public bool CanUseInBattle { get; set; } = true;
    public bool CanUseInDungeon { get; set; } = true;
    public bool CanUseInField { get; set; } = true;
    public bool CanUseInTown { get; set; } = true;
    
    // スタック
    public int MaxStack { get; set; } = 99;
    
    // レアリティ
    public Rarity Rarity { get; set; } = Rarity.White;
}

/// <summary>
/// アイテム使用結果
/// </summary>
public class ItemUseResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int HPHealed { get; set; }
    public int MPHealed { get; set; }
    public int StaminaHealed { get; set; }
    public string? BuffApplied { get; set; }
    public int BuffDurationMinutes { get; set; }
    public bool ItemConsumed { get; set; }
    public string? TeleportLocation { get; set; }
}

/// <summary>
/// インベントリ内の消耗品
/// </summary>
public class ConsumableInventoryItem
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; } = "";
    public int Quantity { get; set; }
    public ConsumableType Type { get; set; }
}
