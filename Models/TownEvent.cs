namespace FFA.Models;

/// <summary>
/// 街イベントタイプ
/// </summary>
public enum TownEventType
{
    // ポジティブイベント
    Beggar,                 // 物乞い
    LostChild,              // 迷子の子ども
    TravelingMerchant,      // 旅の商人
    Bard,                   // 吟遊詩人
    WiseOldMan,             // 賢者
    FortuneTeller,          // 占い師
    Adventurer,             // 旅の冒険者
    Noble,                  // 貴族
    Healer,                 // 癒し手
    TreasureHunter,         // 宝探しの冒険者
    
    // ニュートラルイベント
    Gambler,                // ギャンブラー
    StrangeMerchant,        // 怪しい商人
    PotionSeller,           // 薬売り
    InformationBroker,      // 情報屋
    
    // ネガティブイベント
    Thief,                  // 泥棒
    Pickpocket,             // すり
    ConArtist,              // 詐欺師
    Drunkard,               // 酔っ払い
    Rival,                  // ライバル
}

/// <summary>
/// 街イベント結果タイプ
/// </summary>
public enum TownEventResultType
{
    Gold,                   // ゴールド増減
    Experience,             // 経験値増減
    Item,                   // アイテム獲得
    Buff,                   // バフ獲得
    Debuff,                 // デバフ
    Reputation,             // 評判変化
    Karma,                  // 業変化
    HP,                     // HP増減
    MP,                     // MP増減
    Stamina,                // スタミナ増減
    Nothing,                // 何もなし
    Random,                 // ランダム
}

/// <summary>
/// 街イベント選択肢
/// </summary>
public class TownEventChoice
{
    public string Text { get; set; } = "";
    public string Description { get; set; } = "";
    public TownEventResultType ResultType { get; set; }
    public int ResultValue { get; set; }
    public int SuccessChance { get; set; } = 100; // 成功率（パーセント）
    public string? SuccessMessage { get; set; }
    public string? FailMessage { get; set; }
    public int KarmaRequirement { get; set; } = 0; // 必要な業値（下限）
    public int GoldRequirement { get; set; } = 0; // 必要なゴールド
}

/// <summary>
/// 街イベント
/// </summary>
public class TownEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public TownEventType Type { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string NpcName { get; set; } = "";
    public string NpcDialogue { get; set; } = "";
    public string Icon { get; set; } = "👤";
    public List<TownEventChoice> Choices { get; set; } = new();
    public int MinLevel { get; set; } = 1;
    public int MaxLevel { get; set; } = 999;
    public double Weight { get; set; } = 1.0; // 出現重み
    public List<string>? RequiredLocations { get; set; } // 特定の場所でのみ出現
    public bool IsOneTime { get; set; } = false; // 一回きりのイベントか
}

/// <summary>
/// 街イベント発生履歴
/// </summary>
public class TownEventHistory
{
    public string EventId { get; set; } = "";
    public string Username { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int ChoiceIndex { get; set; }
    public bool Success { get; set; }
    public int GoldChange { get; set; }
    public int ExpChange { get; set; }
}
