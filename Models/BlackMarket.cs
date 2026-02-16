namespace FFA.Models;

/// <summary>
/// ブラックマーケット商品
/// </summary>
public class BlackMarketItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = ""; // 武器、防具、消耗品、材料
    public int Price { get; set; } // 通常市場の2~5倍
    public int ReputationRequired { get; set; } = 0; // 必要評判
    public bool IsIllegal { get; set; } = false; // 違法アイテムか
    public int RiskLevel { get; set; } = 0; // リスクレベル（捕まる確率）
    public string RareType { get; set; } = ""; // レアリティ
}

/// <summary>
/// ブラックマーケット商品データベース
/// </summary>
public static class BlackMarketCatalog
{
    public static List<BlackMarketItem> Items { get; } = new List<BlackMarketItem>
    {
        // 違法武器
        new BlackMarketItem { Id = 1, Name = "暗殺者の匕首", Description = "静かに敵を倒せる...", Type = "武器", Price = 5000, IsIllegal = true, RiskLevel = 5, RareType = "Epic" },
        new BlackMarketItem { Id = 2, Name = "毒刀", Description = "毒塗りの刃", Type = "武器", Price = 3000, IsIllegal = true, RiskLevel = 3, RareType = "Rare" },
        new BlackMarketItem { Id = 3, Name = " 금지の弓", Description = "矢から毒が飞出", Type = "武器", Price = 4000, IsIllegal = true, RiskLevel = 4, RareType = "Rare" },

        // 違法防具
        new BlackMarketItem { Id = 10, Name = "忍びの衣", Description = "姿を消すことができる...", Type = "防具", Price = 8000, IsIllegal = true, RiskLevel = 5, RareType = "Legendary" },
        new BlackMarketItem { Id = 11, Name = "盗賊の外套", Description = "盗賊団の象徴", Type = "防具", Price = 2500, IsIllegal = true, RiskLevel = 2, RareType = "Uncommon" },

        // 禁忌の消耗品
        new BlackMarketItem { Id = 20, Name = "狂人の药水", Description = "力を得るが体を坏す...", Type = "消耗品", Price = 1500, IsIllegal = true, RiskLevel = 4 },
        new BlackMarketItem { Id = 21, Name = "闇の契約書", Description = "悪魔との契約", Type = "消耗品", Price = 10000, IsIllegal = true, RiskLevel = 10, RareType = "Mythic" },

        // レア材料（合法）
        new BlackMarketItem { Id = 30, Name = "龍の鱗", Description = "伝説の龍の鱗", Type = "材料", Price = 5000, ReputationRequired = 50 },
        new BlackMarketItem { Id = 31, Name = "妖精の羽", Description = "妖精の力を持つ羽", Type = "材料", Price = 3000, ReputationRequired = 30 },
        new BlackMarketItem { Id = 32, Name = "賢者の石", Description = "全ての元素を統合した石", Type = "材料", Price = 10000, ReputationRequired = 70, RareType = "Legendary" },

        // 特別武器（合法）
        new BlackMarketItem { Id = 40, Name = "伝説の剣", Description = "英雄が使用していた剣", Type = "武器", Price = 15000, ReputationRequired = 100, RareType = "Legendary" },
        new BlackMarketItem { Id = 41, Name = "天使の弓", Description = "天界的力を持有的弓", Type = "武器", Price = 12000, ReputationRequired = 80, RareType = "Epic" },

        // 悪魔的知识
        new BlackMarketItem { Id = 50, Name = "禁断の書", Description = "含まれていている知识は...", Type = "消耗品", Price = 8000, IsIllegal = true, RiskLevel = 8, RareType = "Mythic" },
        new BlackMarketItem { Id = 51, Name = "魂の器", Description = "魂を 保存できる器", Type = "消耗品", Price = 6000, IsIllegal = true, RiskLevel = 6, RareType = "Epic" },
    };

    public static List<BlackMarketItem> GetAvailableItems(int reputation)
    {
        return Items.Where(i => i.ReputationRequired <= reputation).ToList();
    }

    public static List<BlackMarketItem> GetIllegalItems()
    {
        return Items.Where(i => i.IsIllegal).ToList();
    }
}
