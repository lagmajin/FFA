using LiteDB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FFA.Models;

namespace FFA.Services;

public class MarketService
{
    private readonly string _databasePath;
    // In-memory locks to prevent concurrent operations on same market item within this process
    private static readonly ConcurrentDictionary<int, object> _locks = new();

    public MarketService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "market.db");
    }

    // アイテムを出品する
    public bool ListItem(string sellerUsername, InventoryItem item, int price)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var marketItems = db.GetCollection<MarketItem>("marketitems");

            var marketItem = new MarketItem
            {
                SellerUsername = sellerUsername,
                Item = item,
                Price = price,
                ListedAt = DateTime.Now,
                IsSold = false
            };

            marketItems.Insert(marketItem);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MarketService.ListItem 例外: {ex.Message} - {ex.StackTrace}");
            return false;
        }
    }

    // 出品中のアイテム一覧を取得
    public IEnumerable<MarketItem> GetListedItems()
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var marketItems = db.GetCollection<MarketItem>("marketitems");
            return marketItems.Find(m => !m.IsSold).OrderByDescending(m => m.ListedAt).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MarketService.GetListedItems 例外: {ex.Message} - {ex.StackTrace}");
            return new List<MarketItem>();
        }
    }

    // 特定のユーザーの出品アイテムを取得
    public IEnumerable<MarketItem> GetItemsBySeller(string sellerUsername)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var marketItems = db.GetCollection<MarketItem>("marketitems");
            return marketItems.Find(m => m.SellerUsername == sellerUsername && !m.IsSold).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MarketService.GetItemsBySeller 例外: {ex.Message} - {ex.StackTrace}");
            return new List<MarketItem>();
        }
    }

    // アイテムを購入する
    public bool BuyItem(string buyerUsername, int marketItemId)
    {
        try
        {
            var lockObj = _locks.GetOrAdd(marketItemId, _ => new object());
            lock (lockObj)
            {
                using var db = new LiteDatabase(_databasePath);
                var marketItems = db.GetCollection<MarketItem>("marketitems");
                var marketItem = marketItems.FindById(marketItemId);
                if (marketItem == null || marketItem.IsSold)
                    return false;

                // 購入者の情報を取得
                var userService = new UserService();
                var buyer = userService.GetByUsername(buyerUsername);
                if (buyer == null)
                    return false;

                // 購入価格と手数料を計算
                int price = marketItem.Price;
                int fee = (int)(price * 0.001); // 0.1%の手数料
                int totalCost = price + fee;

                // 購入者が十分なギルを持っているか確認
                if (buyer.Gil < totalCost)
                    return false;

                // 購入者からギルを引き、手数料を徴収
                buyer.Gil -= totalCost;
                userService.UpdateUser(buyer);

                // 売却者にギルを加算
                var seller = userService.GetByUsername(marketItem.SellerUsername);
                if (seller != null)
                {
                    seller.Gil += price;
                    userService.UpdateUser(seller);
                }

                // マーケットアイテムを更新
                marketItem.IsSold = true;
                marketItem.BuyerUsername = buyerUsername;
                marketItem.SoldAt = DateTime.Now;
                marketItems.Update(marketItem);

                // 購入者のインベントリにアイテムを追加
                var itemToGive = new InventoryItem
                {
                    Name = marketItem.Item.Name,
                    Type = marketItem.Item.Type,
                    Quantity = 1,
                    Price = marketItem.Item.Price
                };
                userService.AddItemToUser(buyerUsername, itemToGive);

                // remove lock
                _locks.TryRemove(marketItemId, out _);

                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MarketService.BuyItem 例外: {ex.Message} - {ex.StackTrace}");
            return false;
        }
    }

    // 出品を取り消す
    public bool CancelListing(int marketItemId)
    {
        try
        {
            var lockObj = _locks.GetOrAdd(marketItemId, _ => new object());
            lock (lockObj)
            {
                using var db = new LiteDatabase(_databasePath);
                var marketItems = db.GetCollection<MarketItem>("marketitems");
                var marketItem = marketItems.FindById(marketItemId);
                if (marketItem == null || marketItem.IsSold)
                    return false;

                marketItems.Delete(marketItemId);

                _locks.TryRemove(marketItemId, out _);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MarketService.CancelListing 例外: {ex.Message} - {ex.StackTrace}");
            return false;
        }
    }

    // 取引履歴を取得
    public IEnumerable<MarketItem> GetTransactionHistory(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var marketItems = db.GetCollection<MarketItem>("marketitems");
            return marketItems.Find(m => (m.SellerUsername == username || m.BuyerUsername == username) && m.IsSold)
                .OrderByDescending(m => m.SoldAt)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MarketService.GetTransactionHistory 例外: {ex.Message} - {ex.StackTrace}");
            return new List<MarketItem>();
        }
    }
}