using LiteDB;

namespace FFA.Models;

/// <summary>
/// 魚情報
/// </summary>
public class Fish
{
    public int Id { get; set; }
    
    // 魚の名前
    public string Name { get; set; } = "";
    
    // レアリティ（1-5）
    public int Rarity { get; set; } = 1;
    
    // 基本価格
    public int BasePrice { get; set; } = 10;
    
    // 必要な釣りレベル
    public int RequiredFishingLevel { get; set; } = 1;
    
    // 釣れる場所タイプ
    public List<FishingLocation> Locations { get; set; } = new();
    
    // 釣れる時間帯
    public List<FishingTimeOfDay> ActiveTimes { get; set; } = new();
    
    // 釣れる天候
    public List<WeatherCondition> WeatherConditions { get; set; } = new();
    
    // 基本捕獲確率（0.0-1.0）
    public double BaseCatchRate { get; set; } = 0.5;
    
    // 重量範囲（グラム）
    public int MinWeight { get; set; } = 100;
    public int MaxWeight { get; set; } = 1000;
    
    // 経験値
    public int ExpReward { get; set; } = 10;
    
    // 料理可能かどうか
    public bool CanCook { get; set; } = true;
    
    // 料理後のアイテム名
    public string? CookedItemName { get; set; }
    
    // 料理後の回復量
    public int CookedHealAmount { get; set; } = 0;
    
    // 料理後のバフ効果
    public string? CookedBuffEffect { get; set; }
}

/// <summary>
/// 釣り場所
/// </summary>
public enum FishingLocation
{
    /// <summary>淡水（川・湖）</summary>
    Freshwater = 0,
    /// <summary>海水（海）</summary>
    Saltwater = 1,
    /// <summary>深海</summary>
    DeepSea = 2,
    /// <summary>洞窟の水域</summary>
    Cave = 3,
    /// <summary>火山の水域</summary>
    Volcanic = 4,
    /// <summary>雪原地帯</summary>
    Frozen = 5,
    /// <summary>森の小川</summary>
    ForestStream = 6,
    /// <summary>砂漠のオアシス</summary>
    Oasis = 7
}

/// <summary>
/// 釣り時間帯
/// </summary>
public enum FishingTimeOfDay
{
    /// <summary>いつでも</summary>
    Any = 0,
    /// <summary>朝</summary>
    Morning = 1,
    /// <summary>昼</summary>
    Day = 2,
    /// <summary>夕方</summary>
    Evening = 3,
    /// <summary>夜</summary>
    Night = 4
}

/// <summary>
/// 天候条件
/// </summary>
public enum WeatherCondition
{
    /// <summary>いつでも</summary>
    Any = 0,
    /// <summary>晴れ</summary>
    Sunny = 1,
    /// <summary>曇り</summary>
    Cloudy = 2,
    /// <summary>雨</summary>
    Rainy = 3,
    /// <summary>嵐</summary>
    Stormy = 4,
    /// <summary>雪</summary>
    Snowy = 5
}

/// <summary>
/// 釣り結果
/// </summary>
public class FishingResult
{
    public bool IsSuccess { get; set; }
    public Fish? CaughtFish { get; set; }
    public int Weight { get; set; } // グラム
    public int ExpGained { get; set; }
    public int FishingExpGained { get; set; }
    public string Message { get; set; } = "";
    public bool IsNewRecord { get; set; } // 自己最高記録更新
    public FishingStaminaInfo? StaminaInfo { get; set; } // スタミナ情報
}

/// <summary>
/// 釣りスタミナ情報
/// </summary>
public class FishingStaminaInfo
{
    public int Current { get; set; }
    public int Max { get; set; }
    public DateTime? NextRecovery { get; set; }
}

/// <summary>
/// 釣り統計
/// </summary>
public class FishingStats
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public int TotalCatches { get; set; }
    public int TotalWeight { get; set; } // 総重量
    public int BiggestWeight { get; set; } // 最大魚重量
    public string? BiggestFishName { get; set; }
    public int FishingLevel { get; set; }
    public int FishingExp { get; set; }
    public int FishingExpToNext { get; set; }
    public int UniqueSpeciesCaught { get; set; } // 種類数
    public Dictionary<int, FishingRecord> Records { get; set; } = new(); // 魚ID -> 記録
}

/// <summary>
/// 個別の魚の釣り記録
/// </summary>
public class FishingRecord
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public int FishId { get; set; }
    public string FishName { get; set; } = "";
    public int Count { get; set; } // 釣った回数
    public int BiggestWeight { get; set; } // 最大重量
    public DateTime LastCaught { get; set; } // 最後に釣った日時
}

/// <summary>
/// 釣りスポット情報
/// </summary>
public class FishingSpot
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public FishingLocation LocationType { get; set; }
    public int RequiredLevel { get; set; } = 1;
    public int WorldX { get; set; } // ワールドマップ上の位置
    public int WorldY { get; set; }
    public List<int> AvailableFishIds { get; set; } = new();
    public bool IsAccessible { get; set; } = true;
}

/// <summary>
/// 釣り竿
/// </summary>
public class FishingRod
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int BonusCatchRate { get; set; } // 捕獲率ボーナス（%）
    public int BonusExp { get; set; } // 経験値ボーナス（%）
    public int RequiredLevel { get; set; } = 1;
    public int Price { get; set; }
    public int Rarity { get; set; }
    public bool CanFishDeepSea { get; set; } = false;
    public bool CanFishCave { get; set; } = false;
    public bool CanFishVolcanic { get; set; } = false;
    public bool CanFishFrozen { get; set; } = false;
}

/// <summary>
/// 釣り餌
/// </summary>
public class FishingBait
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int BonusCatchRate { get; set; } // 捕獲率ボーナス（%）
    public int BonusRarity { get; set; } // レア魚出現率ボーナス（%）
    public int Price { get; set; }
    public int Duration { get; set; } // 効果回数
}