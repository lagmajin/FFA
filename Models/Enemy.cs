namespace FFA.Models;

public class Enemy
{
    public string Name { get; set; } = "";
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Exp { get; set; }
    public int Gil { get; set; }
    public string DropItem { get; set; } = "";
    public int DropRate { get; set; } = 30; // %
    
    // 拡張プロパティ
    public string Icon { get; set; } = "👾"; // モンスターのアイコン
    public int Level { get; set; } = 1; // モンスターのレベル
    public EnemyType Type { get; set; } = EnemyType.Normal; // モンスターの種類
    public int Speed { get; set; } = 10; // 素早さ（先制攻撃判定に使用）
    public int MagicAttack { get; set; } = 0; // 魔法攻撃力
    public int MagicDefense { get; set; } = 0; // 魔法防御力
    
    // 複数ドロップ対応
    public List<DropItem> DropItems { get; set; } = new();
    
    // 特殊ドロップ（レアアイテム）
    public DropItem? RareDrop { get; set; }
    public int RareDropRate { get; set; } = 5; // レアドロップ率(%)
    
    // ボーナス報酬
    public int BonusExp { get; set; } = 0; // ボーナス経験値
    public int BonusGil { get; set; } = 0; // ボーナスギル
}

/// <summary>
/// モンスターの種類
/// </summary>
public enum EnemyType
{
    Normal,     // 通常モンスター
    Elite,      // エリート（強め）
    Rare,       // レアモンスター
    Boss,       // ボス
    Event       // イベントモンスター
}

/// <summary>
/// ドロップアイテム情報
/// </summary>
public class DropItem
{
    public string Name { get; set; } = "";
    public int DropRate { get; set; } = 30; // ドロップ率(%)
    public int MinQuantity { get; set; } = 1; // 最小個数
    public int MaxQuantity { get; set; } = 1; // 最大個数
    public ItemRarity Rarity { get; set; } = ItemRarity.Common; // レアリティ
}

/// <summary>
/// アイテムのレアリティ
/// </summary>
public enum ItemRarity
{
    Common,     // コモン（白）
    Uncommon,   // アンコモン（緑）
    Rare,       // レア（青）
    Epic,       // エピック（紫）
    Legendary   // レジェンダリー（橙）
}
