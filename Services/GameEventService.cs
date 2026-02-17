using LiteDB;
using FFA.Models;

namespace FFA.Services;

/// <summary>
/// ゲームイベント Service
/// </summary>
public class GameEventService
{
    private readonly string _databasePath;
    
    public GameEventService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "game_events.db");
    }
    
    /// <summary>
    /// イベントを記録
    /// </summary>
    public void LogEvent(GameEvent gameEvent)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var events = db.GetCollection<GameEvent>("events");
            events.Insert(gameEvent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GameEventService.LogEvent Error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// ユーザーのイベントを取得
    /// </summary>
    public List<GameEvent> GetUserEvents(string username, int limit = 50)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var events = db.GetCollection<GameEvent>("events");
            return events.Find(e => e.Username == username)
                        .OrderByDescending(e => e.Timestamp)
                        .Take(limit)
                        .ToList();
        }
        catch
        {
            return new List<GameEvent>();
        }
    }
    
    /// <summary>
    /// アイテム取得イベント
    /// </summary>
    public void LogItemAcquired(string username, string itemName, int quantity, Rarity rarity)
    {
        var gameEvent = new GameEvent
        {
            Username = username,
            EventType = GameEventType.ItemAcquired,
            Description = $"Received {quantity}x {itemName}",
            ItemName = itemName,
            Quantity = quantity,
            Timestamp = DateTime.UtcNow
        };
        LogEvent(gameEvent);
        
        // 実績システムと連携
        CheckAchievements(username);
    }
    
    /// <summary>
    /// アイテム売却イベント
    /// </summary>
    public void LogItemSold(string username, string itemName, int goldAmount)
    {
        var gameEvent = new GameEvent
        {
            Username = username,
            EventType = GameEventType.ItemSold,
            Description = $"Sold {itemName} for {goldAmount} Gil",
            ItemName = itemName,
            GoldAmount = goldAmount,
            Timestamp = DateTime.UtcNow
        };
        LogEvent(gameEvent);
    }
    
    /// <summary>
    /// 採掘イベント
    /// </summary>
    public void LogMining(string username, bool success, string? materialName = null)
    {
        var gameEvent = new GameEvent
        {
            Username = username,
            EventType = success ? GameEventType.MiningSuccess : GameEventType.MiningFailed,
            Description = success ? $"Mining success: {materialName}" : "Mining failed",
            ItemName = materialName,
            Timestamp = DateTime.UtcNow
        };
        LogEvent(gameEvent);
        
        if (success)
        {
            CheckAchievements(username);
        }
    }
    
    /// <summary>
    /// 戦闘イベント
    /// </summary>
    public void LogBattle(string username, bool won, string? enemyName = null, int? expGained = null)
    {
        var gameEvent = new GameEvent
        {
            Username = username,
            EventType = won ? GameEventType.BattleWon : GameEventType.BattleLost,
            Description = won ? $"Defeated {enemyName}" : "Defeated by enemy",
            EnemyName = enemyName,
            ExperienceGained = expGained,
            Timestamp = DateTime.UtcNow
        };
        LogEvent(gameEvent);
        
        if (won)
        {
            CheckAchievements(username);
        }
    }
    
    /// <summary>
    /// レベルアップイベント
    /// </summary>
    public void LogLevelUp(string username, int newLevel)
    {
        var gameEvent = new GameEvent
        {
            Username = username,
            EventType = GameEventType.LevelUp,
            Description = $"Reached level {newLevel}!",
            Level = newLevel,
            Timestamp = DateTime.UtcNow
        };
        LogEvent(gameEvent);
        
        // 実績システムと連携
        CheckAchievements(username);
    }
    
    /// <summary>
    /// 転生イベント
    /// </summary>
    public void LogRebirth(string username, int rebirthCount, int totalLevel)
    {
        var gameEvent = new GameEvent
        {
            Username = username,
            EventType = GameEventType.Rebirth,
            Description = $"Rebirth #{rebirthCount}! Total Level: {totalLevel}",
            Level = totalLevel,
            Timestamp = DateTime.UtcNow
        };
        LogEvent(gameEvent);
        
        // 実績システムと連携
        CheckAchievements(username);
    }
    
    /// <summary>
    /// アイテム強化イベント
    /// </summary>
    public void LogEnhancement(string username, bool success, string itemName, int newLevel)
    {
        var gameEvent = new GameEvent
        {
            Username = username,
            EventType = success ? GameEventType.ItemEnhanced : GameEventType.ItemEnhancedFailed,
            Description = success ? $"Enhanced {itemName} to +{newLevel}" : $"Failed to enhance {itemName}",
            ItemName = itemName,
            Level = newLevel,
            Timestamp = DateTime.UtcNow
        };
        LogEvent(gameEvent);
        
        if (success && newLevel >= 10)
        {
            CheckAchievements(username);
        }
    }
    
    /// <summary>
    /// 実績達成をチェック
    /// </summary>
    private void CheckAchievements(string username)
    {
        try
        {
            // ここに実績システムのチェックを追加
            // AchievementServiceを使用して実績をチェック
            Console.WriteLine($"Checking achievements for {username}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CheckAchievements Error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 統計を取得
    /// </summary>
    public Dictionary<string, int> GetUserStats(string username)
    {
        var stats = new Dictionary<string, int>();
        
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var events = db.GetCollection<GameEvent>("events");
            
            var userEvents = events.Find(e => e.Username == username).ToList();
            
            stats["TotalEvents"] = userEvents.Count;
            stats["ItemsAcquired"] = userEvents.Count(e => e.EventType == GameEventType.ItemAcquired);
            stats["ItemsSold"] = userEvents.Count(e => e.EventType == GameEventType.ItemSold);
            stats["BattlesWon"] = userEvents.Count(e => e.EventType == GameEventType.BattleWon);
            stats["BattlesLost"] = userEvents.Count(e => e.EventType == GameEventType.BattleLost);
            stats["LevelUps"] = userEvents.Count(e => e.EventType == GameEventType.LevelUp);
            stats["Rebirths"] = userEvents.Count(e => e.EventType == GameEventType.Rebirth);
            stats["MiningSuccess"] = userEvents.Count(e => e.EventType == GameEventType.MiningSuccess);
            stats["EnhancementsSuccess"] = userEvents.Count(e => e.EventType == GameEventType.ItemEnhanced);
        }
        catch
        {
            // Return empty stats on error
        }
        
        return stats;
    }
}
