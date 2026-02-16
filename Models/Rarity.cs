namespace FFA.Models;

/// <summary>
/// アイテムの希少度
/// </summary>
public enum Rarity
{
    Common,      // 灰白色 - 一般
    Uncommon,    // 緑色 - 通常
    Rare,        // 青色 - 高級
    Epic,        // 紫 - 稀有
    Legendary,    // オレンジ - 伝説
    Mythic,      // 赤色 - 神話
    Unique       // 金色 - 唯一
}

/// <summary>
/// 希少度に関する拡張メソッド
/// </summary>
public static class RarityExtensions
{
    public static string GetColor(this Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => "#9e9e9e",
            Rarity.Uncommon => "#4caf50",
            Rarity.Rare => "#2196f3",
            Rarity.Epic => "#9c27b0",
            Rarity.Legendary => "#ff9800",
            Rarity.Mythic => "#f44336",
            Rarity.Unique => "#ffd700",
            _ => "#9e9e9e"
        };
    }

    public static string GetBackgroundColor(this Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => "#f5f5f5",
            Rarity.Uncommon => "#e8f5e9",
            Rarity.Rare => "#e3f2fd",
            Rarity.Epic => "#f3e5f5",
            Rarity.Legendary => "#fff3e0",
            Rarity.Mythic => "#ffebee",
            Rarity.Unique => "#fffde7",
            _ => "#f5f5f5"
        };
    }

    public static string GetBorderColor(this Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => "#bdbdbd",
            Rarity.Uncommon => "#81c784",
            Rarity.Rare => "#64b5f6",
            Rarity.Epic => "#ba68c8",
            Rarity.Legendary => "#ffb74d",
            Rarity.Mythic => "#ef5350",
            Rarity.Unique => "#ffd54f",
            _ => "#bdbdbd"
        };
    }

    public static string GetName(this Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => "一般",
            Rarity.Uncommon => "通常",
            Rarity.Rare => "高級",
            Rarity.Epic => "稀有",
            Rarity.Legendary => "伝説",
            Rarity.Mythic => "神話",
            Rarity.Unique => "唯一",
            _ => "不明"
        };
    }

    public static int GetDropWeight(this Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => 1000,
            Rarity.Uncommon => 400,
            Rarity.Rare => 150,
            Rarity.Epic => 50,
            Rarity.Legendary => 15,
            Rarity.Mythic => 5,
            Rarity.Unique => 1,
            _ => 100
        };
    }
}
