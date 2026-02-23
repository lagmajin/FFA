namespace FFA.Models;

/// <summary>
/// 未鑑定アイテムの種類
/// </summary>
public enum UnidentifiedItemType
{
    Weapon = 0,    // 武器
    Armor = 1,     // 防具
    Accessory = 2, // アクセサリー
    Material = 3   // 素材
}

/// <summary>
/// 未鑑定アイテム
/// </summary>
public class UnidentifiedItem
{
    public int Id { get; set; }
    
    /// <summary>
    /// 所有者のユーザー名
    /// </summary>
    public string Username { get; set; } = "";
    
    /// <summary>
    /// 表示名（「古びた剣」「謎の防具」など）
    /// </summary>
    public string DisplayName { get; set; } = "";
    
    /// <summary>
    /// 説明文
    /// </summary>
    public string Description { get; set; } = "";
    
    /// <summary>
    /// アイテムタイプ
    /// </summary>
    public UnidentifiedItemType ItemType { get; set; }
    
    /// <summary>
    /// 見た目のレアリティ（1-5、実際のレアリティとは異なる場合がある）
    /// </summary>
    public int ApparentRarity { get; set; } = 1;
    
    /// <summary>
    /// 鑑定コスト
    /// </summary>
    public int AppraisalCost { get; set; } = 100;
    
    /// <summary>
    /// 実際のアイテムID（鑑定後に判明）
    /// </summary>
    public int? ActualItemId { get; set; }
    
    /// <summary>
    /// 実際のアイテムタイプ（鑑定後に判明）
    /// </summary>
    public string? ActualItemType { get; set; }
    
    /// <summary>
    /// 実際のアイテム名（鑑定後に判明）
    /// </summary>
    public string? ActualItemName { get; set; }
    
    /// <summary>
    /// 鑑定済みかどうか
    /// </summary>
    public bool IsIdentified { get; set; } = false;
    
    /// <summary>
    /// ドロップ元（モンスター名、宝箱など）
    /// </summary>
    public string? Source { get; set; }
    
    /// <summary>
    /// 取得日時
    /// </summary>
    public DateTime ObtainedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 鑑定日時
    /// </summary>
    public DateTime? IdentifiedAt { get; set; }
}

/// <summary>
/// 未鑑定アイテムテンプレート（ドロップ用）
/// </summary>
public class UnidentifiedItemTemplate
{
    public int Id { get; set; }
    
    /// <summary>
    /// 表示名
    /// </summary>
    public string DisplayName { get; set; } = "";
    
    /// <summary>
    /// 説明文
    /// </summary>
    public string Description { get; set; } = "";
    
    /// <summary>
    /// アイテムタイプ
    /// </summary>
    public UnidentifiedItemType ItemType { get; set; }
    
    /// <summary>
    /// 見た目のレアリティ
    /// </summary>
    public int ApparentRarity { get; set; } = 1;
    
    /// <summary>
    /// 基本鑑定コスト
    /// </summary>
    public int BaseAppraisalCost { get; set; } = 100;
    
    /// <summary>
    /// 可能な実際のアイテムIDリスト（重み付け）
    /// </summary>
    public List<(int ItemId, string ItemType, int Weight)> PossibleItems { get; set; } = new();
    
    /// <summary>
    /// ドロップ率（0.0-1.0）
    /// </summary>
    public double DropRate { get; set; } = 0.1;
    
    /// <summary>
    /// 必要なドロップ元レベル
    /// </summary>
    public int RequiredSourceLevel { get; set; } = 1;
}

/// <summary>
/// 鑑定結果
/// </summary>
public class AppraisalResult
{
    public bool Success { get; set; }
    public UnidentifiedItem? Item { get; set; }
    public string Message { get; set; } = "";
    public int Cost { get; set; }
    
    /// <summary>
    /// 鑑定で判明したアイテムが実際に良いものだったか
    /// </summary>
    public bool IsGoodResult { get; set; }
    
    /// <summary>
    /// 実際のレアリティ
    /// </summary>
    public int ActualRarity { get; set; }
}

/// <summary>
/// 鑑定士のスキルレベル
/// </summary>
public enum AppraiserLevel
{
    Novice = 1,      // 新人鑑定士 - 基本鑑定のみ
    Apprentice = 2,  // 見習い鑑定士 - レアアイテム鑑定可能
    Journeyman = 3,  // 熟練鑑定士 - 割引あり
    Expert = 4,      // 専門鑑定士 - 大幅割引
    Master = 5       // マスター鑑定士 - 無料鑑定あり
}
