using FFA.Models;
using LiteDB;
using System.IO;

namespace FFA.Services;

/// <summary>
/// 釣りシステムのサービス
/// </summary>
public class FishingService
{
    private readonly string _databasePath;
    private readonly TimeWeatherService _timeWeatherService;
    private readonly StaminaService _staminaService;
    
    private static readonly Random _random = new();
    private const int BaseFishingExpToNext = 50;
    private const int FishingStaminaCost = 1;

    public FishingService(TimeWeatherService timeWeatherService, StaminaService staminaService)
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "users.db");
        _timeWeatherService = timeWeatherService;
        _staminaService = staminaService;
        InitializeData();
    }

    private LiteDatabase GetDatabase() => new LiteDatabase(_databasePath);

    /// <summary>
    /// 初期データの投入
    /// </summary>
    private void InitializeData()
    {
        using var db = GetDatabase();
        var fish = db.GetCollection<Fish>("fish");
        var rods = db.GetCollection<FishingRod>("fishingRods");
        var baits = db.GetCollection<FishingBait>("fishingBaits");

        if (fish.Count() == 0)
        {
            fish.Insert(new List<Fish>
            {
                new Fish { Id = 1, Name = "川魚", Rarity = 1, BasePrice = 10, RequiredFishingLevel = 1,
                    Locations = new List<FishingLocation> { FishingLocation.Freshwater },
                    BaseCatchRate = 0.8, MinWeight = 100, MaxWeight = 500, ExpReward = 5 },
                new Fish { Id = 2, Name = "海魚", Rarity = 1, BasePrice = 15, RequiredFishingLevel = 1,
                    Locations = new List<FishingLocation> { FishingLocation.Saltwater },
                    BaseCatchRate = 0.7, MinWeight = 200, MaxWeight = 800, ExpReward = 8 },
                new Fish { Id = 3, Name = "森のニジマス", Rarity = 2, BasePrice = 30, RequiredFishingLevel = 3,
                    Locations = new List<FishingLocation> { FishingLocation.ForestStream },
                    BaseCatchRate = 0.5, MinWeight = 300, MaxWeight = 1200, ExpReward = 15 },
                new Fish { Id = 4, Name = "洞窟魚", Rarity = 3, BasePrice = 80, RequiredFishingLevel = 5,
                    Locations = new List<FishingLocation> { FishingLocation.Cave },
                    BaseCatchRate = 0.4, MinWeight = 150, MaxWeight = 600, ExpReward = 25 },
                new Fish { Id = 5, Name = "深海魚", Rarity = 4, BasePrice = 200, RequiredFishingLevel = 10,
                    Locations = new List<FishingLocation> { FishingLocation.DeepSea },
                    BaseCatchRate = 0.3, MinWeight = 500, MaxWeight = 3000, ExpReward = 50 },
                new Fish { Id = 6, Name = "火山魚", Rarity = 4, BasePrice = 250, RequiredFishingLevel = 12,
                    Locations = new List<FishingLocation> { FishingLocation.Volcanic },
                    BaseCatchRate = 0.25, MinWeight = 200, MaxWeight = 800, ExpReward = 60 },
                new Fish { Id = 7, Name = "雪魚", Rarity = 3, BasePrice = 100, RequiredFishingLevel = 7,
                    Locations = new List<FishingLocation> { FishingLocation.Frozen },
                    BaseCatchRate = 0.4, MinWeight = 300, MaxWeight = 1000, ExpReward = 30 }
            });
        }

        if (rods.Count() == 0)
        {
            rods.Insert(new List<FishingRod>
            {
                new FishingRod { Id = 1, Name = "竹の釣り竿", BonusCatchRate = 0, BonusExp = 0, RequiredLevel = 1, Price = 100, Rarity = 1 },
                new FishingRod { Id = 2, Name = "木の釣り竿", BonusCatchRate = 5, BonusExp = 5, RequiredLevel = 3, Price = 500, Rarity = 1 },
                new FishingRod { Id = 3, Name = "カーボンロッド", BonusCatchRate = 10, BonusExp = 10, RequiredLevel = 5, Price = 2000, Rarity = 2 },
                new FishingRod { Id = 4, Name = "深海ロッド", BonusCatchRate = 15, BonusExp = 15, RequiredLevel = 10, Price = 8000, Rarity = 3, CanFishDeepSea = true },
                new FishingRod { Id = 5, Name = "レジェンドロッド", BonusCatchRate = 25, BonusExp = 30, RequiredLevel = 20, Price = 50000, Rarity = 5, CanFishDeepSea = true, CanFishCave = true, CanFishVolcanic = true, CanFishFrozen = true }
            });
        }

        if (baits.Count() == 0)
        {
            baits.Insert(new List<FishingBait>
            {
                new FishingBait { Id = 1, Name = "ミミズ", Description = "基本的な餌", BonusCatchRate = 5, BonusRarity = 0, Price = 5, Duration = 1 },
                new FishingBait { Id = 2, Name = "小エビ", Description = "少し良い餌", BonusCatchRate = 10, BonusRarity = 2, Price = 20, Duration = 1 },
                new FishingBait { Id = 3, Name = "特製ルアー", Description = "高品質なルアー", BonusCatchRate = 15, BonusRarity = 5, Price = 100, Duration = 5 },
                new FishingBait { Id = 4, Name = "魔法の餌", Description = "レア魚を引き寄せる", BonusCatchRate = 20, BonusRarity = 20, Price = 1000, Duration = 5 }
            });
        }
    }

    /// <summary>
    /// 釣りを実行（簡易版 - スポットなしで直接釣り）
    /// </summary>
    public FishingResult Fish(int userId, int rodId, int baitId)
    {
        using var db = GetDatabase();
        var fish = db.GetCollection<Fish>("fish");
        var rods = db.GetCollection<FishingRod>("fishingRods");
        var baits = db.GetCollection<FishingBait>("fishingBaits");
        var users = db.GetCollection<User>("users");

        var user = users.FindById(userId);
        if (user == null)
            return new FishingResult { IsSuccess = false, Message = "ユーザーが見つかりません。" };

        // スタミナチェック
        var staminaResult = _staminaService.UseStamina(user.Username, StaminaType.Fishing, FishingStaminaCost);
        if (!staminaResult.Success)
        {
            return new FishingResult 
            { 
                IsSuccess = false, 
                Message = staminaResult.Message,
                StaminaInfo = new FishingStaminaInfo
                {
                    Current = staminaResult.Remaining,
                    Max = staminaResult.MaxStamina,
                    NextRecovery = staminaResult.RecoverAt
                }
            };
        }

        var rod = rods.FindById(rodId) ?? rods.FindById(1); // デフォルトは竹の釣り竿
        var bait = baits.FindById(baitId);

        // 釣れる魚のリストを生成
        var availableFish = fish.FindAll().ToList();
        if (availableFish.Count == 0)
            return new FishingResult { IsSuccess = false, Message = "魚がいません。" };

        // 捕獲率計算
        double catchRate = 0.5 + rod.BonusCatchRate / 100.0;
        if (bait != null)
            catchRate += bait.BonusCatchRate / 100.0;
        catchRate = Math.Min(catchRate, 0.95);

        // 釣り成功判定
        if (_random.NextDouble() > catchRate)
            return new FishingResult { IsSuccess = false, Message = "魚が逃げてしまった..." };

        // 魚を選択（レアリティ重み付け）
        var selectedFish = SelectFish(availableFish, bait);
        if (selectedFish == null)
            return new FishingResult { IsSuccess = false, Message = "魚が釣れなかった。" };

        int weight = _random.Next(selectedFish.MinWeight, selectedFish.MaxWeight + 1);
        int fishingExp = (int)(selectedFish.ExpReward * (1 + rod.BonusExp / 100.0));

        // インベントリに追加
        user.Inventory.Add(new InventoryItem
        {
            Name = $"{selectedFish.Name} ({weight}g)",
            Type = "Fish",
            Quantity = 1,
            Price = CalculateFishPrice(selectedFish, weight)
        });
        users.Update(user);

        return new FishingResult
        {
            IsSuccess = true,
            CaughtFish = selectedFish,
            Weight = weight,
            ExpGained = selectedFish.ExpReward,
            FishingExpGained = fishingExp,
            Message = $"{selectedFish.Name}（{weight}g）を釣った！"
        };
    }

    private Fish? SelectFish(List<Fish> availableFish, FishingBait? bait)
    {
        var weights = availableFish.Select(f =>
        {
            int w = 100 / f.Rarity;
            if (bait != null) w += bait.BonusRarity * f.Rarity;
            return w;
        }).ToList();

        int total = weights.Sum();
        int roll = _random.Next(total);
        int cumulative = 0;

        for (int i = 0; i < availableFish.Count; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative) return availableFish[i];
        }
        return availableFish.FirstOrDefault();
    }

    private int CalculateFishPrice(Fish fish, int weight)
    {
        double avg = (fish.MinWeight + fish.MaxWeight) / 2.0;
        return (int)(fish.BasePrice * (weight / avg));
    }

    public List<Fish> GetAllFish()
    {
        using var db = GetDatabase();
        return db.GetCollection<Fish>("fish").FindAll().ToList();
    }

    public List<FishingRod> GetAllRods()
    {
        using var db = GetDatabase();
        return db.GetCollection<FishingRod>("fishingRods").FindAll().ToList();
    }

    public List<FishingBait> GetAllBaits()
    {
        using var db = GetDatabase();
        return db.GetCollection<FishingBait>("fishingBaits").FindAll().ToList();
    }

    public int SellFish(int userId, string itemName)
    {
        using var db = GetDatabase();
        var users = db.GetCollection<User>("users");
        var user = users.FindById(userId);
        if (user == null) return 0;

        var item = user.Inventory.FirstOrDefault(i => i.Name == itemName && i.Type == "Fish");
        if (item == null) return 0;

        user.Inventory.Remove(item);
        user.Gil += item.Price;
        users.Update(user);
        return item.Price;
    }
}