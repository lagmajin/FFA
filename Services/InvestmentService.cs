using FFA.Models;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FFA.Services;

/// <summary>
/// 投資管理サービス
/// </summary>
public class InvestmentService
{
    private readonly UserService _userService;
    private readonly Random _random = new();

    public InvestmentService()
    {
        _userService = new UserService();
    }

    /// <summary>
    /// ユーザーの投資一覧を取得
    /// </summary>
    public List<UserInvestment> GetUserInvestments(string username)
    {
        try
        {
            using var db = new LiteDatabase("FFA.db");
            var col = db.GetCollection<UserInvestment>("investments");
            var investments = col.Find(i => i.Username == username).ToList();
            
            // 商品情報を結合
            foreach (var inv in investments)
            {
                inv.Product = InvestmentDatabase.GetProduct(inv.ProductId);
            }
            
            return investments;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InvestmentService.GetUserInvestments 例外: {ex.Message}");
            return new List<UserInvestment>();
        }
    }

    /// <summary>
    /// 投資可能な商品一覧を取得
    /// </summary>
    public List<InvestmentProduct> GetAvailableProducts(string username)
    {
        var user = _userService.GetByUsername(username);
        if (user == null) return new List<InvestmentProduct>();
        
        return InvestmentDatabase.GetAvailableProducts(user.Level);
    }

    /// <summary>
    /// 投資を実行
    /// </summary>
    public InvestmentResult Invest(string username, int productId, int amount, int durationDays)
    {
        var result = new InvestmentResult();
        
        try
        {
            var user = _userService.GetByUsername(username);
            if (user == null)
            {
                result.Success = false;
                result.Message = "ユーザーが見つかりません";
                return result;
            }

            var product = InvestmentDatabase.GetProduct(productId);
            if (product == null)
            {
                result.Success = false;
                result.Message = "投資商品が見つかりません";
                return result;
            }

            // 条件チェック
            if (user.Level < product.RequiredLevel)
            {
                result.Success = false;
                result.Message = $"レベル{product.RequiredLevel}以上が必要です";
                return result;
            }

            if (amount < product.MinInvestment)
            {
                result.Success = false;
                result.Message = $"最小投資額は{product.MinInvestment}ギルです";
                return result;
            }

            if (amount > product.MaxInvestment)
            {
                result.Success = false;
                result.Message = $"最大投資額は{product.MaxInvestment}ギルです";
                return result;
            }

            if (user.Gil < amount)
            {
                result.Success = false;
                result.Message = "ギルが不足しています";
                return result;
            }

            if (durationDays < product.MinDurationDays || durationDays > product.MaxDurationDays)
            {
                result.Success = false;
                result.Message = $"投資期間は{product.MinDurationDays}〜{product.MaxDurationDays}日です";
                return result;
            }

            // 投資実行
            user.Gil -= amount;
            _userService.UpdateUser(user);

            var investment = new UserInvestment
            {
                Username = username,
                ProductId = productId,
                InvestedAmount = amount,
                CurrentValue = amount,
                InvestedAt = DateTime.UtcNow,
                MaturesAt = DateTime.UtcNow.AddDays(durationDays),
                Status = InvestmentStatus.Active
            };

            using var db = new LiteDatabase("FFA.db");
            var col = db.GetCollection<UserInvestment>("investments");
            col.Insert(investment);

            result.Success = true;
            result.Message = $"{product.Name}に{amount}ギル投資しました（期間: {durationDays}日）";
            result.Investment = investment;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InvestmentService.Invest 例外: {ex.Message}");
            result.Success = false;
            result.Message = "投資エラーが発生しました";
        }

        return result;
    }

    /// <summary>
    /// 投資の価値を更新（日次処理）
    /// </summary>
    public void UpdateInvestmentValues()
    {
        try
        {
            using var db = new LiteDatabase("FFA.db");
            var col = db.GetCollection<UserInvestment>("investments");
            var activeInvestments = col.Find(i => i.Status == InvestmentStatus.Active).ToList();

            foreach (var inv in activeInvestments)
            {
                var product = InvestmentDatabase.GetProduct(inv.ProductId);
                if (product == null) continue;

                // 前日の記録を取得
                var lastRecord = inv.DailyRecords.OrderByDescending(r => r.Date).FirstOrDefault();
                var lastValue = lastRecord?.Value ?? inv.InvestedAmount;

                // 価値変動を計算
                var changePercent = CalculateDailyChange(product);
                var newValue = Math.Max(0, (int)(lastValue * (1 + changePercent / 100.0)));

                // 日次記録を追加
                var record = new InvestmentDailyRecord
                {
                    Date = DateTime.UtcNow.Date,
                    Value = newValue,
                    ChangePercent = changePercent,
                    Event = GenerateMarketEvent(product, changePercent)
                };
                inv.DailyRecords.Add(record);

                // 現在価値を更新
                inv.CurrentValue = newValue;

                // 満期チェック
                if (DateTime.UtcNow >= inv.MaturesAt)
                {
                    inv.Status = InvestmentStatus.Matured;
                }

                col.Update(inv);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InvestmentService.UpdateInvestmentValues 例外: {ex.Message}");
        }
    }

    /// <summary>
    /// 日次変動率を計算
    /// </summary>
    private int CalculateDailyChange(InvestmentProduct product)
    {
        // 基本リターン率を日割り
        var dailyBaseReturn = product.BaseReturnRate / 10.0; // 期間中の期待リターンの約1/10
        
        // ボラティリティに基づくランダム変動
        var volatility = product.Volatility;
        var randomChange = (_random.NextDouble() - 0.5) * 2 * volatility;
        
        // リスクレベルによる調整
        var riskMultiplier = product.RiskLevel * 0.2;
        
        // 最終変動率
        var totalChange = dailyBaseReturn + randomChange * riskMultiplier;
        
        // 極端な値を制限
        return (int)Math.Clamp(totalChange, -volatility * 2, volatility * 2);
    }

    /// <summary>
    /// 市場イベントを生成
    /// </summary>
    private string? GenerateMarketEvent(InvestmentProduct product, int changePercent)
    {
        if (Math.Abs(changePercent) < 10) return null;

        var events = changePercent > 0 ? new[]
        {
            $"{product.Name}が好調！需要急増",
            "市場全体が上昇基調",
            "有利なニュースが発表されました",
            "大手投資家が参入"
        } : new[]
        {
            $"{product.Name}が急落...",
            "市場不安が広がる",
            "悪いニュースが報じられました",
            "投資家が売り浴びせ"
        };

        return events[_random.Next(events.Length)];
    }

    /// <summary>
    /// 満期投資を受け取る
    /// </summary>
    public InvestmentResult ClaimInvestment(string username, int investmentId)
    {
        var result = new InvestmentResult();

        try
        {
            var user = _userService.GetByUsername(username);
            if (user == null)
            {
                result.Success = false;
                result.Message = "ユーザーが見つかりません";
                return result;
            }

            using var db = new LiteDatabase("FFA.db");
            var col = db.GetCollection<UserInvestment>("investments");
            var investment = col.FindOne(i => i.Id == investmentId && i.Username == username);

            if (investment == null)
            {
                result.Success = false;
                result.Message = "投資が見つかりません";
                return result;
            }

            if (investment.Status != InvestmentStatus.Matured)
            {
                result.Success = false;
                result.Message = "まだ満期に達していません";
                return result;
            }

            // リターン計算
            var returnRate = (int)((double)investment.CurrentValue / investment.InvestedAmount * 100) - 100;
            investment.ReturnValue = investment.CurrentValue;
            investment.ReturnRate = returnRate;
            investment.Status = InvestmentStatus.Completed;
            investment.CompletedAt = DateTime.UtcNow;

            // ユーザーに支払い
            user.Gil += investment.ReturnValue;
            _userService.UpdateUser(user);

            col.Update(investment);

            result.Success = true;
            result.Message = $"投資完了！{investment.ReturnValue}ギルを受け取りました（リターン: {returnRate:+#;-#;0}%）";
            result.Investment = investment;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InvestmentService.ClaimInvestment 例外: {ex.Message}");
            result.Success = false;
            result.Message = "受取エラーが発生しました";
        }

        return result;
    }

    /// <summary>
    /// 投資を早期清算
    /// </summary>
    public InvestmentResult LiquidateInvestment(string username, int investmentId)
    {
        var result = new InvestmentResult();

        try
        {
            var user = _userService.GetByUsername(username);
            if (user == null)
            {
                result.Success = false;
                result.Message = "ユーザーが見つかりません";
                return result;
            }

            using var db = new LiteDatabase("FFA.db");
            var col = db.GetCollection<UserInvestment>("investments");
            var investment = col.FindOne(i => i.Id == investmentId && i.Username == username);

            if (investment == null)
            {
                result.Success = false;
                result.Message = "投資が見つかりません";
                return result;
            }

            if (investment.Status != InvestmentStatus.Active)
            {
                result.Success = false;
                result.Message = "この投資は清算できません";
                return result;
            }

            // 早期清算ペナルティ（-10%）
            var penaltyRate = 0.9;
            var liquidationValue = (int)(investment.CurrentValue * penaltyRate);

            investment.ReturnValue = liquidationValue;
            investment.ReturnRate = (int)((double)liquidationValue / investment.InvestedAmount * 100) - 100;
            investment.Status = InvestmentStatus.Liquidated;
            investment.CompletedAt = DateTime.UtcNow;

            // ユーザーに支払い
            user.Gil += liquidationValue;
            _userService.UpdateUser(user);

            col.Update(investment);

            result.Success = true;
            result.Message = $"早期清算完了。{liquidationValue}ギルを受け取りました（ペナルティ適用）";
            result.Investment = investment;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InvestmentService.LiquidateInvestment 例外: {ex.Message}");
            result.Success = false;
            result.Message = "清算エラーが発生しました";
        }

        return result;
    }

    /// <summary>
    /// ユーザーの投資統計を取得
    /// </summary>
    public InvestmentStats GetUserStats(string username)
    {
        var stats = new InvestmentStats();
        
        try
        {
            var investments = GetUserInvestments(username);
            
            stats.TotalInvestments = investments.Count;
            stats.ActiveInvestments = investments.Count(i => i.Status == InvestmentStatus.Active);
            stats.MaturedInvestments = investments.Count(i => i.Status == InvestmentStatus.Matured);
            stats.CompletedInvestments = investments.Count(i => i.Status == InvestmentStatus.Completed);
            
            stats.TotalInvested = investments.Where(i => i.Status == InvestmentStatus.Active)
                .Sum(i => i.InvestedAmount);
            stats.CurrentValue = investments.Where(i => i.Status == InvestmentStatus.Active)
                .Sum(i => i.CurrentValue);
            
            var completed = investments.Where(i => i.Status == InvestmentStatus.Completed || 
                                                   i.Status == InvestmentStatus.Liquidated);
            stats.TotalReturns = completed.Sum(i => i.ReturnValue - i.InvestedAmount);
            stats.BestReturn = completed.Max(i => i.ReturnRate);
            stats.WorstReturn = completed.Min(i => i.ReturnRate);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InvestmentService.GetUserStats 例外: {ex.Message}");
        }

        return stats;
    }
}

/// <summary>
/// 投資結果
/// </summary>
public class InvestmentResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public UserInvestment? Investment { get; set; }
}

/// <summary>
/// 投資統計
/// </summary>
public class InvestmentStats
{
    public int TotalInvestments { get; set; }
    public int ActiveInvestments { get; set; }
    public int MaturedInvestments { get; set; }
    public int CompletedInvestments { get; set; }
    
    public int TotalInvested { get; set; }
    public int CurrentValue { get; set; }
    public int TotalReturns { get; set; }
    public int BestReturn { get; set; }
    public int WorstReturn { get; set; }
    
    public int PendingReturns => CurrentValue - TotalInvested;
}
