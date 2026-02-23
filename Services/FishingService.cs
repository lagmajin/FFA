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
    private readonly UserService _userService;
    
    private static readonly Random _random = new();
    private const int BaseFishingExpToNext = 50;
    private const int FishingStaminaCost = 1;

    public FishingService(TimeWeatherService timeWeatherService, StaminaService staminaService, UserService userService)
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "users.db");
        _timeWeatherService = timeWeatherService;
        _staminaService = staminaService;
        _userService = userService;
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
        var spots = db.GetCollection<FishingSpot>("fishingSpots");

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
                    BaseCatchRate = 0.4, MinWeight = 300, MaxWeight = 1000, ExpReward = 30 },
                new Fish { Id = 8, Name = "オアシスコイ", Rarity = 2, BasePrice = 40, RequiredFishingLevel = 4,
                    Locations = new List<FishingLocation> { FishingLocation.Oasis },
                    BaseCatchRate = 0.5, MinWeight = 200, MaxWeight = 800, ExpReward = 12 },
                new Fish { Id = 9, Name = "黄金の鯉", Rarity = 5, BasePrice = 500, RequiredFishingLevel = 15,
                    Locations = new List<FishingLocation> { FishingLocation.Freshwater, FishingLocation.Freshwater },
                    BaseCatchRate = 0.15, MinWeight = 800, MaxWeight = 2000, ExpReward = 100 },
                new Fish { Id = 10, Name = "幻の深海魚", Rarity = 5, BasePrice = 1000, RequiredFishingLevel = 20,
                    Locations = new List<FishingLocation> { FishingLocation.DeepSea },
                    BaseCatchRate = 0.1, MinWeight = 1000, MaxWeight = 5000, ExpReward = 200 }
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
                new FishingRod { Id = 5, Name = "洞窟ロッド", BonusCatchRate = 12, BonusExp = 12, RequiredLevel = 8, Price = 5000, Rarity = 3, CanFishCave = true },
                new FishingRod { Id = 6, Name = "火山ロッド", BonusCatchRate = 12, BonusExp = 15, RequiredLevel = 12, Price = 7000, Rarity = 3, CanFishVolcanic = true },
                new FishingRod { Id = 7, Name = "氷雪ロッド", BonusCatchRate = 12, BonusExp = 12, RequiredLevel = 10, Price = 6000, Rarity = 3, CanFishFrozen = true },
                new FishingRod { Id = 8, Name = "レジェンドロッド", BonusCatchRate = 25, BonusExp = 30, RequiredLevel = 20, Price = 50000, Rarity = 5, CanFishDeepSea = true, CanFishCave = true, CanFishVolcanic = true, CanFishFrozen = true }
            });
        }

        if (baits.Count() == 0)
        {
            baits.Insert(new List<FishingBait>
            {
                new FishingBait { Id = 1, Name = "ミミズ", Description = "基本的な餌", BonusCatchRate = 5, BonusRarity = 0, Price = 5, Duration = 1 },
                new FishingBait { Id = 2, Name = "小エビ", Description = "少し良い餌", BonusCatchRate = 10, BonusRarity = 2, Price = 20, Duration = 1 },
                new FishingBait { Id = 3, Name = "特製ルアー", Description = "高品質なルアー", BonusCatchRate = 15, BonusRarity = 5, Price = 100, Duration = 5 },
                new FishingBait { Id = 4, Name = "魔法の餌", Description = "レア魚を引き寄せる", BonusCatchRate = 20, BonusRarity = 20, Price = 1000, Duration = 5 },
                new FishingBait { Id = 5, Name = "黄金のルアー", Description = "最高級ルアー", BonusCatchRate = 25, BonusRarity = 30, Price = 5000, Duration = 10 }
            });
        }

        if (spots.Count() == 0)
        {
            spots.Insert(new List<FishingSpot>
            {
                new FishingSpot { Id = 1, Name = "村の小川", Description = "初心者向けの穏やかな小川", LocationType = FishingLocation.Freshwater, RequiredLevel = 1, WorldX = 0, WorldY = 0, IsAccessible = true },
                new FishingSpot { Id = 2, Name = "海岸", Description = "波打ち際で海釣り", LocationType = FishingLocation.Saltwater, RequiredLevel = 1, WorldX = 5, WorldY = 0, IsAccessible = true },
                new FishingSpot { Id = 3, Name = "森の沢", Description = "森の中を流れる清らかな小川", LocationType = FishingLocation.ForestStream, RequiredLevel = 3, WorldX = 2, WorldY = 3, IsAccessible = true },
                new FishingSpot { Id = 4, Name = "洞窟の湖", Description = "暗く冷たい地下湖", LocationType = FishingLocation.Cave, RequiredLevel = 5, WorldX = -3, WorldY = 2, IsAccessible = true },
                new FishingSpot { Id = 5, Name = "深海", Description = "深海の神秘的な世界", LocationType = FishingLocation.DeepSea, RequiredLevel = 10, WorldX = 8, WorldY = 0, IsAccessible = true },
                new FishingSpot { Id = 6, Name = "火山温泉", Description = "熱気漂う温泉地帯", LocationType = FishingLocation.Volcanic, RequiredLevel = 12, WorldX = -5, WorldY = -3, IsAccessible = true },
                new FishingSpot { Id = 7, Name = "氷結湖", Description = "凍りついた湖", LocationType = FishingLocation.Frozen, RequiredLevel = 7, WorldX = 0, WorldY = 8, IsAccessible = true },
                new FishingSpot { Id = 8, Name = "砂漠のオアシス", Description = "砂漠の中の小さな楽園", LocationType = FishingLocation.Oasis, RequiredLevel = 4, WorldX = 6, WorldY = -5, IsAccessible = true }
            });
        }
    }

    /// <summary>
    /// 釣りスポット一覧を取得
    /// </summary>
    public List<FishingSpot> GetAllSpots()
    {
        using var db = GetDatabase();
        return db.GetCollection<FishingSpot>("fishingSpots").FindAll().ToList();
    }

    /// <summary>
    /// 釣りスポットを取得
    /// </summary>
    public FishingSpot? GetSpot(int spotId)
    {
        using var db = GetDatabase();
        return db.GetCollection<FishingSpot>("fishingSpots").FindById(spotId);
    }

    /// <summary>
    /// ユーザーの釣り統計を取得
    /// </summary>
    public FishingStats GetUserStats(string username)
    {
        using var db = GetDatabase();
        var statsCollection = db.GetCollection<FishingStats>("fishingStats");
        var stats = statsCollection.FindOne(s => s.Username == username);
        
        if (stats == null)
        {
            stats = new FishingStats { Username = username, FishingLevel = 1, FishingExp = 0 };
            statsCollection.Insert(stats);
        }
        
        return stats;
    }

    /// <summary>
    /// 釣り経験値を追加
    /// </summary>
    public void AddFishingExp(string username, int exp)
    {
        using var db = GetDatabase();
        var statsCollection = db.GetCollection<FishingStats>("fishingStats");
        var stats = statsCollection.FindOne(s => s.Username == username);
        
        if (stats == null)
        {
            stats = new FishingStats { Username = username, FishingLevel = 1, FishingExp = 0 };
        }
        
        stats.FishingExp += exp;
        
        // レベルアップ判定
        int expToNext = GetExpToNextLevel(stats.FishingLevel);
        while (stats.FishingExp >= expToNext)
        {
            stats.FishingExp -= expToNext;
            stats.FishingLevel++;
            expToNext = GetExpToNextLevel(stats.FishingLevel);
        }
        
        statsCollection.Upsert(stats);
    }

    /// <summary>
    /// 次のレベルに必要な経験値を取得
    /// </summary>
    public int GetExpToNextLevel(int level)
    {
        return (int)(BaseFishingExpToNext * Math.Pow(1.5, level - 1));
    }

    /// <summary>
    /// 釣り記録を更新
    /// </summary>
    public void UpdateRecord(string username, Fish fish, int weight)
    {
        using var db = GetDatabase();
        var recordsCollection = db.GetCollection<FishingRecord>("fishingRecords");
        var record = recordsCollection.FindOne(r => r.Username == username && r.FishId == fish.Id);
        
        if (record == null)
        {
            record = new FishingRecord
            {
                Username = username,
                FishId = fish.Id,
                FishName = fish.Name,
                Count = 1,
                BiggestWeight = weight,
                LastCaught = DateTime.UtcNow
            };
            recordsCollection.Insert(record);
        }
        else
        {
            record.Count++;
            if (weight > record.BiggestWeight)
                record.BiggestWeight = weight;
            record.LastCaught = DateTime.UtcNow;
            recordsCollection.Update(record);
        }
    }

    /// <summary>
    /// ユーザーの釣り記録一覧を取得
    /// </summary>
    public List<FishingRecord> GetUserRecords(string username)
    {
        using var db = GetDatabase();
        return db.GetCollection<FishingRecord>("fishingRecords")
            .Find(r => r.Username == username)
            .ToList();
    }

    /// <summary>
    /// 釣りを実行（スポット指定版）
    /// </summary>
    public FishingResult Fish(int userId, int spotId, int rodId, int baitId)
    {
        using var db = GetDatabase();
        var fish = db.GetCollection<Fish>("fish");
        var rods = db.GetCollection<FishingRod>("fishingRods");
        var baits = db.GetCollection<FishingBait>("fishingBaits");
        var users = db.GetCollection<User>("users");
        var spots = db.GetCollection<FishingSpot>("fishingSpots");

        var user = users.FindById(userId);
        if (user == null)
            return new FishingResult { IsSuccess = false, Message = "ユーザーが見つかりません。" };

        var spot = spots.FindById(spotId);
        if (spot == null)
            return new FishingResult { IsSuccess = false, Message = "釣りスポットが見つかりません。" };

        // ユーザーの釣りレベル確認
        var stats = GetUserStats(user.Username);
        if (stats.FishingLevel < spot.RequiredLevel)
            return new FishingResult { IsSuccess = false, Message = $"このスポットには釣りレベル{spot.RequiredLevel}以上が必要です。" };

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

        var rod = rods.FindById(rodId) ?? rods.FindById(1);
        var bait = baitId > 0 ? baits.FindById(baitId) : null;

        // 釣り竿の特殊場所対応チェック
        if (spot.LocationType == FishingLocation.DeepSea && !rod.CanFishDeepSea && rod.Id > 1)
            return new FishingResult { IsSuccess = false, Message = "この釣り竿では深海釣りができません。深海ロッド以上が必要です。" };
        if (spot.LocationType == FishingLocation.Cave && !rod.CanFishCave && rod.Id > 1)
            return new FishingResult { IsSuccess = false, Message = "この釣り竿では洞窟釣りができません。" };
        if (spot.LocationType == FishingLocation.Volcanic && !rod.CanFishVolcanic && rod.Id > 1)
            return new FishingResult { IsSuccess = false, Message = "この釣り竿では火山釣りができません。" };
        if (spot.LocationType == FishingLocation.Frozen && !rod.CanFishFrozen && rod.Id > 1)
            return new FishingResult { IsSuccess = false, Message = "この釣り竿では氷結地釣りができません。" };

        // スポットに対応する魚のリストを生成
        var availableFish = fish.Find(f => f.Locations.Contains(spot.LocationType) && f.RequiredFishingLevel <= stats.FishingLevel).ToList();
        if (availableFish.Count == 0)
            return new FishingResult { IsSuccess = false, Message = "この場所には釣れる魚がいません。" };

        // 天候・時間帯の影響を取得
        var weather = _timeWeatherService.Weather;
        var phase = _timeWeatherService.Phase;
        double weatherBonus = GetWeatherBonus(weather, spot.LocationType);
        double timeBonus = GetTimeBonus(phase);

        // 捕獲率計算
        double catchRate = 0.5 + rod.BonusCatchRate / 100.0 + weatherBonus + timeBonus;
        if (bait != null)
            catchRate += bait.BonusCatchRate / 100.0;
        catchRate = Math.Min(catchRate, 0.95);

        // 釣り成功判定
        if (_random.NextDouble() > catchRate)
            return new FishingResult { IsSuccess = false, Message = "魚が逃げてしまった...", StaminaInfo = new FishingStaminaInfo
            {
                Current = staminaResult.Remaining,
                Max = staminaResult.MaxStamina,
                NextRecovery = staminaResult.RecoverAt
            }};

        // 魚を選択（レアリティ重み付け）
        var selectedFish = SelectFish(availableFish, bait, stats.FishingLevel);
        if (selectedFish == null)
            return new FishingResult { IsSuccess = false, Message = "魚が釣れなかった。" };

        int weight = _random.Next(selectedFish.MinWeight, selectedFish.MaxWeight + 1);
        int fishingExp = (int)(selectedFish.ExpReward * (1 + rod.BonusExp / 100.0));

        // 経験値追加
        AddFishingExp(user.Username, fishingExp);

        // 記録更新
        UpdateRecord(user.Username, selectedFish, weight);

        // 自己最高記録チェック
        bool isNewRecord = CheckNewRecord(user.Username, selectedFish.Id, weight);

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
            Message = $"{selectedFish.Name}（{weight}g）を釣った！",
            IsNewRecord = isNewRecord,
            StaminaInfo = new FishingStaminaInfo
            {
                Current = staminaResult.Remaining - 1,
                Max = staminaResult.MaxStamina,
                NextRecovery = staminaResult.RecoverAt
            }
        };
    }

    /// <summary>
    /// 釣りを実行（簡易版 - スポットなしで直接釣り）
    /// </summary>
    public FishingResult Fish(int userId, int rodId, int baitId)
    {
        // デフォルトで村の小川を使用
        return Fish(userId, 1, rodId, baitId);
    }

    /// <summary>
    /// 天候によるボーナスを取得
    /// </summary>
    private double GetWeatherBonus(WeatherType weather, FishingLocation location)
    {
        return (weather, location) switch
        {
            (WeatherType.Rain, FishingLocation.Freshwater) => 0.1,
            (WeatherType.Rain, FishingLocation.ForestStream) => 0.15,
            (WeatherType.Storm, _) => -0.1,
            (WeatherType.Clear, FishingLocation.Oasis) => 0.1,
            (WeatherType.Snow, FishingLocation.Frozen) => 0.1,
            _ => 0
        };
    }

    /// <summary>
    /// 時間帯によるボーナスを取得
    /// </summary>
    private double GetTimeBonus(DayPhase phase)
    {
        return phase switch
        {
            DayPhase.Dawn => 0.05,
            DayPhase.Dusk => 0.1,
            DayPhase.Night => 0.05,
            _ => 0
        };
    }

    private Fish? SelectFish(List<Fish> availableFish, FishingBait? bait, int fishingLevel)
    {
        var weights = availableFish.Select(f =>
        {
            int w = 100 / f.Rarity;
            if (bait != null) w += bait.BonusRarity * f.Rarity;
            // 釣りレベルが高いほどレア魚が出やすくなる
            w += (fishingLevel - f.RequiredFishingLevel) * 2;
            return Math.Max(w, 1);
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

    private bool CheckNewRecord(string username, int fishId, int weight)
    {
        using var db = GetDatabase();
        var recordsCollection = db.GetCollection<FishingRecord>("fishingRecords");
        var record = recordsCollection.FindOne(r => r.Username == username && r.FishId == fishId);
        
        if (record == null) return true;
        return weight > record.BiggestWeight;
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

    /// <summary>
    /// 釣り竿を購入
    /// </summary>
    public (bool Success, string Message) BuyRod(string username, int rodId)
    {
        using var db = GetDatabase();
        var users = db.GetCollection<User>("users");
        var user = users.FindOne(u => u.Username == username);
        if (user == null) return (false, "ユーザーが見つかりません。");

        var rod = db.GetCollection<FishingRod>("fishingRods").FindById(rodId);
        if (rod == null) return (false, "釣り竿が見つかりません。");

        var stats = GetUserStats(username);
        if (stats.FishingLevel < rod.RequiredLevel)
            return (false, $"釣りレベル{rod.RequiredLevel}以上が必要です。");

        if (user.Gil < rod.Price)
            return (false, "ギルが足りません。");

        // 既に所持しているかチェック
        if (user.OwnedRodIds.Contains(rodId))
            return (false, "既に所持しています。");

        user.Gil -= rod.Price;
        user.OwnedRodIds.Add(rodId);
        users.Update(user);

        return (true, $"{rod.Name}を購入しました！");
    }

    /// <summary>
    /// 餌を購入
    /// </summary>
    public (bool Success, string Message) BuyBait(string username, int baitId, int quantity)
    {
        using var db = GetDatabase();
        var users = db.GetCollection<User>("users");
        var user = users.FindOne(u => u.Username == username);
        if (user == null) return (false, "ユーザーが見つかりません。");

        var bait = db.GetCollection<FishingBait>("fishingBaits").FindById(baitId);
        if (bait == null) return (false, "餌が見つかりません。");

        int totalCost = bait.Price * quantity;
        if (user.Gil < totalCost)
            return (false, "ギルが足りません。");

        user.Gil -= totalCost;
        
        // インベントリに追加
        var existingBait = user.Inventory.FirstOrDefault(i => i.Type == "Bait" && i.ItemId == baitId);
        if (existingBait != null)
        {
            existingBait.Quantity += quantity;
        }
        else
        {
            user.Inventory.Add(new InventoryItem
            {
                Name = bait.Name,
                Type = "Bait",
                ItemId = baitId,
                Quantity = quantity,
                Price = bait.Price
            });
        }
        users.Update(user);

        return (true, $"{bait.Name}を{quantity}個購入しました！");
    }

    /// <summary>
    /// ユーザーが所持している釣り竿一覧を取得
    /// </summary>
    public List<FishingRod> GetOwnedRods(string username)
    {
        using var db = GetDatabase();
        var user = db.GetCollection<User>("users").FindOne(u => u.Username == username);
        if (user == null) return new List<FishingRod>();

        var allRods = db.GetCollection<FishingRod>("fishingRods").FindAll().ToList();
        return allRods.Where(r => user.OwnedRodIds.Contains(r.Id)).ToList();
    }

    /// <summary>
    /// ユーザーが所持している餌一覧を取得
    /// </summary>
    public List<(FishingBait Bait, int Quantity)> GetOwnedBaits(string username)
    {
        using var db = GetDatabase();
        var user = db.GetCollection<User>("users").FindOne(u => u.Username == username);
        if (user == null) return new List<(FishingBait, int)>();

        var allBaits = db.GetCollection<FishingBait>("fishingBaits").FindAll().ToList();
        var result = new List<(FishingBait, int)>();

        foreach (var item in user.Inventory.Where(i => i.Type == "Bait"))
        {
            var bait = allBaits.FirstOrDefault(b => b.Id == item.ItemId);
            if (bait != null)
            {
                result.Add((bait, item.Quantity));
            }
        }

        return result;
    }

    /// <summary>
    /// 餌を消費
    /// </summary>
    public bool ConsumeBait(string username, int baitId)
    {
        using var db = GetDatabase();
        var users = db.GetCollection<User>("users");
        var user = users.FindOne(u => u.Username == username);
        if (user == null) return false;

        var bait = user.Inventory.FirstOrDefault(i => i.Type == "Bait" && i.ItemId == baitId);
        if (bait == null || bait.Quantity <= 0) return false;

        bait.Quantity--;
        if (bait.Quantity <= 0)
            user.Inventory.Remove(bait);
        
        users.Update(user);
        return true;
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

    /// <summary>
    /// 全ての魚を売却
    /// </summary>
    public int SellAllFish(int userId)
    {
        using var db = GetDatabase();
        var users = db.GetCollection<User>("users");
        var user = users.FindById(userId);
        if (user == null) return 0;

        var fishItems = user.Inventory.Where(i => i.Type == "Fish").ToList();
        int total = fishItems.Sum(f => f.Price);
        
        foreach (var item in fishItems)
            user.Inventory.Remove(item);
        
        user.Gil += total;
        users.Update(user);
        return total;
    }
}
