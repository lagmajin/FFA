namespace FFA.Models;

/// <summary>
/// アイテムの希少度
/// </summary>
public enum Rarity
{
    White,      // 白 - 一般
    Purple,     // 紫 - 高級
    Red,        // 赤 - 稀有
    Orange,     // オレンジ - 伝説
    Gold,       // 金色 - 神話
    Rainbow     // レインボー - 最上位
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
            Rarity.White => "#FFFFFF",
            Rarity.Purple => "#9C27B0",
            Rarity.Red => "#F44336",
            Rarity.Orange => "#FF9800",
            Rarity.Gold => "#FFD700",
            Rarity.Rainbow => "linear-gradient(90deg, red, orange, yellow, green, blue, indigo, violet)",
            _ => "#FFFFFF"
        };
    }

    public static string GetBorderColor(this Rarity rarity)
    {
        return rarity switch
        {
            Rarity.White => "#BDBDBD",
            Rarity.Purple => "#7B1FA2",
            Rarity.Red => "#D32F2F",
            Rarity.Orange => "#F57C00",
            Rarity.Gold => "#FFC107",
            Rarity.Rainbow => "#FF5722",
            _ => "#BDBDBD"
        };
    }

    public static string GetName(this Rarity rarity)
    {
        return rarity switch
        {
            Rarity.White => "一般",
            Rarity.Purple => "高級",
            Rarity.Red => "稀有",
            Rarity.Orange => "伝説",
            Rarity.Gold => "神話",
            Rarity.Rainbow => "最上位",
            _ => "不明"
        };
    }

    public static int GetDropWeight(this Rarity rarity)
    {
        return rarity switch
        {
            Rarity.White => 1000,
            Rarity.Purple => 200,
            Rarity.Red => 80,
            Rarity.Orange => 30,
            Rarity.Gold => 10,
            Rarity.Rainbow => 1,
            _ => 100
        };
    }

    public static int GetMultiplier(this Rarity rarity)
    {
        return rarity switch
        {
            Rarity.White => 1,
            Rarity.Purple => 2,
            Rarity.Red => 3,
            Rarity.Orange => 5,
            Rarity.Gold => 10,
            Rarity.Rainbow => 20,
            _ => 1
        };
    }
}
