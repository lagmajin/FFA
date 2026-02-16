using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using FFA.Models;

namespace FFA.Services;

public class BlackMarketService
{
    private readonly string _databasePath;
    private const int ReputationCooldownDays = 1; // 評判変更のクールダウン

    public BlackMarketService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "blackmarket.db");
    }

    // 商品を購入
    public BlackMarketResult PurchaseItem(string username, int itemId)
    {
        try
        {
            var item = BlackMarketCatalog.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                return new BlackMarketResult { Success = false, Message = "商品が見つかりません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new BlackMarketResult { Success = false, Message = "ユーザーが見つかりません" };

            // 評判チェック
            int reputation = GetReputation(username);
            if (reputation < item.ReputationRequired)
                return new BlackMarketResult { Success = false, Message = $"{item.ReputationRequired}以上の評判が必要です" };

            // 金銭チェック
            if (user.Gil < item.Price)
                return new BlackMarketResult { Success = false, Message = "ギルが不足しています" };

            // 違法アイテムのリスクチェック
            if (item.IsIllegal)
            {
                var random = new Random();
                if (random.Next(100) < item.RiskLevel * 2) // リスク発動
                {
                    // 捕まる！アイテムを没收
                    user.Gil -= item.Price / 2; // 半額罰金
                    userService.UpdateUser(user);

                    return new BlackMarketResult
                    {
                        Success = false,
                        Message = $"�_police_ が近づいてきた！慌てて逃げたらアイテムを落としてしまった... 罰金{item.Price / 2}ギル",
                        Arrested = true,
                        Fine = item.Price / 2
                    };
                }
            }

            // 購入成功
            user.Gil -= item.Price;
            userService.UpdateUser(user);

            // アイテムをインベントリに追加
            var inventoryItem = new InventoryItem
            {
                Name = item.Name,
                Type = item.Type,
                Quantity = 1,
                Price = item.Price
            };
            userService.AddItemToUser(username, inventoryItem);

            // 評判を獲得（アイテム购买）
            AddReputation(username, 5);

            return new BlackMarketResult
            {
                Success = true,
                Message = $"{item.Name}を購入しました！",
                ItemName = item.Name,
                Price = item.Price,
                IsIllegal = item.IsIllegal
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("BlackMarketService.PurchaseItem error: " + ex.Message);
            return new BlackMarketResult { Success = false, Message = "エラーが発生しました" };
        }
    }

    // ユーザーの評判を取得
    public int GetReputation(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var reputations = db.GetCollection<BlackMarketReputation>("reputations");
            var rep = reputations.FindOne(r => r.Username == username);
            return rep?.Reputation ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    // 評判を追加
    public void AddReputation(string username, int amount)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var reputations = db.GetCollection<BlackMarketReputation>("reputations");
            var rep = reputations.FindOne(r => r.Username == username);

            if (rep == null)
            {
                rep = new BlackMarketReputation
                {
                    Username = username,
                    Reputation = 0,
                    LastUpdated = DateTime.UtcNow
                };
                reputations.Insert(rep);
            }

            rep.Reputation += amount;
            rep.LastUpdated = DateTime.UtcNow;
            reputations.Upsert(rep);
        }
        catch (Exception ex)
        {
            Console.WriteLine("BlackMarketService.AddReputation error: " + ex.Message);
        }
    }

    // 評判を下げる（捕まった時など）
    public void ReduceReputation(string username, int amount)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var reputations = db.GetCollection<BlackMarketReputation>("reputations");
            var rep = reputations.FindOne(r => r.Username == username);

            if (rep != null)
            {
                rep.Reputation = Math.Max(0, rep.Reputation - amount);
                rep.LastUpdated = DateTime.UtcNow;
                reputations.Upsert(rep);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("BlackMarketService.ReduceReputation error: " + ex.Message);
        }
    }

    // 利用可能な商品を获取
    public List<BlackMarketItem> GetAvailableItems(string username)
    {
        int reputation = GetReputation(username);
        return BlackMarketCatalog.GetAvailableItems(reputation);
    }
}

/// <summary>
/// _blackmarket 評判
/// </summary>
public class BlackMarketReputation
{
    public string Username { get; set; } = "";
    public int Reputation { get; set; } = 0;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// _blackmarket 結果
/// </summary>
public class BlackMarketResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string ItemName { get; set; } = "";
    public int Price { get; set; }
    public bool IsIllegal { get; set; }
    public bool Arrested { get; set; }
    public int Fine { get; set; }
}
