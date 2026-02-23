using System;

namespace FFA.Models;

/// <summary>
/// 投資商品タイプ
/// </summary>
public enum InvestmentType
{
    Stock,          // 株式 - 高リスク高リターン
    Bond,           // 債券 - 低リスク低リターン
    Commodity,      // 商品 - 中リスク中リターン
    Crypto,         // 暗号資産 - 超高リスク超高リターン
    RealEstate,     // 不動産 - 安定低リターン
    Venture         // ベンチャー - 高リスク高リターン
}

/// <summary>
/// 投資商品
/// </summary>
public class InvestmentProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public InvestmentType Type { get; set; }
    public string Icon { get; set; } = "📈";
    
    // 投資設定
    public int MinInvestment { get; set; } = 100;           // 最小投資額
    public int MaxInvestment { get; set; } = 100000;        // 最大投資額
    
    // リスク・リターン（パーセンテージ）
    public int BaseReturnRate { get; set; } = 5;            // 基本リターン率（%）
    public int RiskLevel { get; set; } = 1;                 // リスクレベル（1-5）
    public int Volatility { get; set; } = 10;               // ボラティリティ（変動幅%）
    
    // 期間設定
    public int MinDurationDays { get; set; } = 1;           // 最短投資期間（日）
    public int MaxDurationDays { get; set; } = 30;          // 最長投資期間（日）
    
    // 条件
    public int RequiredLevel { get; set; } = 1;             // 必要レベル
    public int RequiredGil { get; set; } = 0;               // 必要所持ギル
    
    public bool IsAvailable { get; set; } = true;
}

/// <summary>
/// ユーザーの投資履歴
/// </summary>
public class UserInvestment
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public int ProductId { get; set; }
    public InvestmentProduct? Product { get; set; }
    
    public int InvestedAmount { get; set; }                 // 投資額
    public int CurrentValue { get; set; }                   // 現在価値
    public int ReturnValue { get; set; }                    // 返却額（確定時）
    
    public DateTime InvestedAt { get; set; }                // 投資開始日時
    public DateTime MaturesAt { get; set; }                 // 満期日時
    public DateTime? CompletedAt { get; set; }              // 完了日時
    
    public InvestmentStatus Status { get; set; } = InvestmentStatus.Active;
    public int ReturnRate { get; set; }                     // 実際のリターン率（%）
    
    // 日次履歴
    public List<InvestmentDailyRecord> DailyRecords { get; set; } = new();
}

/// <summary>
/// 投資ステータス
/// </summary>
public enum InvestmentStatus
{
    Active,         // 投資中
    Matured,        // 満期済み（受取可能）
    Completed,      // 完了（受取済み）
    Liquidated,     // 清算（早期解約）
    Failed          // 失敗（元本割れ）
}

/// <summary>
/// 日次投資記録
/// </summary>
public class InvestmentDailyRecord
{
    public DateTime Date { get; set; }
    public int Value { get; set; }                          // その日の評価額
    public int ChangePercent { get; set; }                  // 前日比（%）
    public string? Event { get; set; }                      // イベント（市場ニュース等）
}

/// <summary>
/// 投資市場イベント
/// </summary>
public class MarketEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public InvestmentType? AffectedType { get; set; }       // null = 全体に影響
    public int ImpactPercent { get; set; }                  // 影響度（%）
    public bool IsPositive { get; set; }                    // プラス/マイナス影響
    public DateTime OccurredAt { get; set; }
    public int DurationDays { get; set; } = 1;              // 影響期間
}

/// <summary>
/// 投資データベース
/// </summary>
public static class InvestmentDatabase
{
    public static List<InvestmentProduct> Products { get; } = new List<InvestmentProduct>
    {
        new InvestmentProduct
        {
            Id = 1,
            Name = "王国債券",
            Description = "王国が発行する安全な債券。安定した低リターン。",
            Icon = "📜",
            Type = InvestmentType.Bond,
            MinInvestment = 100,
            MaxInvestment = 50000,
            BaseReturnRate = 3,
            RiskLevel = 1,
            Volatility = 5,
            MinDurationDays = 3,
            MaxDurationDays = 7,
            RequiredLevel = 1
        },
        new InvestmentProduct
        {
            Id = 2,
            Name = "商工会議所株",
            Description = "商工会議所の株式。安定した中程度のリターン。",
            Icon = "🏛️",
            Type = InvestmentType.Stock,
            MinInvestment = 500,
            MaxInvestment = 100000,
            BaseReturnRate = 6,
            RiskLevel = 2,
            Volatility = 15,
            MinDurationDays = 5,
            MaxDurationDays = 14,
            RequiredLevel = 5
        },
        new InvestmentProduct
        {
            Id = 3,
            Name = "冒険者ギルド出資",
            Description = "冒険者ギルドへの出資。成果により変動。",
            Icon = "⚔️",
            Type = InvestmentType.Venture,
            MinInvestment = 1000,
            MaxInvestment = 200000,
            BaseReturnRate = 10,
            RiskLevel = 3,
            Volatility = 25,
            MinDurationDays = 7,
            MaxDurationDays = 21,
            RequiredLevel = 10
        },
        new InvestmentProduct
        {
            Id = 4,
            Name = "魔法鉱石先物",
            Description = "魔法鉱石の先物取引。価格変動が激しい。",
            Icon = "💎",
            Type = InvestmentType.Commodity,
            MinInvestment = 2000,
            MaxInvestment = 150000,
            BaseReturnRate = 8,
            RiskLevel = 4,
            Volatility = 35,
            MinDurationDays = 3,
            MaxDurationDays = 10,
            RequiredLevel = 15
        },
        new InvestmentProduct
        {
            Id = 5,
            Name = "古代遺跡ファンド",
            Description = "古代遺跡発掘への投資。大当たりor大損。",
            Icon = "🏺",
            Type = InvestmentType.Venture,
            MinInvestment = 5000,
            MaxInvestment = 300000,
            BaseReturnRate = 15,
            RiskLevel = 5,
            Volatility = 50,
            MinDurationDays = 10,
            MaxDurationDays = 30,
            RequiredLevel = 20
        },
        new InvestmentProduct
        {
            Id = 6,
            Name = "不動産投資信託",
            Description = "街の不動産への投資。安定した家収入。",
            Icon = "🏠",
            Type = InvestmentType.RealEstate,
            MinInvestment = 3000,
            MaxInvestment = 250000,
            BaseReturnRate = 5,
            RiskLevel = 2,
            Volatility = 10,
            MinDurationDays = 7,
            MaxDurationDays = 30,
            RequiredLevel = 8
        },
        new InvestmentProduct
        {
            Id = 7,
            Name = "エーテルコイン",
            Description = "新興の魔法通貨。超高リスク超高リターン。",
            Icon = "🪙",
            Type = InvestmentType.Crypto,
            MinInvestment = 1000,
            MaxInvestment = 100000,
            BaseReturnRate = 20,
            RiskLevel = 5,
            Volatility = 70,
            MinDurationDays = 1,
            MaxDurationDays = 7,
            RequiredLevel = 25
        }
    };
    
    public static InvestmentProduct? GetProduct(int id)
    {
        return Products.FirstOrDefault(p => p.Id == id);
    }
    
    public static List<InvestmentProduct> GetAvailableProducts(int userLevel)
    {
        return Products.Where(p => p.IsAvailable && p.RequiredLevel <= userLevel).ToList();
    }
}
