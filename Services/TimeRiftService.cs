using FFA.Models;

namespace FFA.Services;

public class TimeRiftService
{
    private static readonly Random _random = new();
    private readonly CombatService _combat;
    
    private static readonly Dictionary<string, TimeRiftPlayerSession> _activeSessions = new();
    private static readonly List<TimeRiftRecord> _leaderboards = new();
    
    private static readonly List<TimeRiftData> _defaultDungeons = new()
    {
        new TimeRiftData
        {
            Id = 1,
            Name = "Ancient Ruins",
            JapaneseName = "古代遺跡",
            Description = "時空の間に隠された古代の遗迹。速くクリアすればより良い報酬が得られる。",
            Type = TimeRiftType.Daily,
            RecommendedLevel = 1,
            TimeLimitSeconds = 300,
            MaxFloors = 5,
            BaseGilReward = 500,
            BaseExpReward = 300,
            MonsterTypes = new List<string> { "Skeleton", "Zombie", "Ghost" }
        },
        new TimeRiftData
        {
            Id = 2,
            Name = "Dragon's Lair",
            JapaneseName = "ドラゴンズ・レア",
            Description = "龍の巢。时间限制挑战。",
            Type = TimeRiftType.Weekly,
            RecommendedLevel = 30,
            TimeLimitSeconds = 600,
            MaxFloors = 10,
            BaseGilReward = 2000,
            BaseExpReward = 1000,
            MonsterTypes = new List<string> { "Dragon", "Wyvern", "Drake" }
        },
        new TimeRiftData
        {
            Id = 3,
            Name = "Abyss Tower",
            JapaneseName = "アビスタワー",
            Description = "深淵の塔を最速で駆け上がれ！",
            Type = TimeRiftType.Challenge,
            RecommendedLevel = 50,
            TimeLimitSeconds = 180,
            MaxFloors = 20,
            BaseGilReward = 5000,
            BaseExpReward = 3000,
            MonsterTypes = new List<string> { "Demon", "DarkKnight", "Vampire" }
        }
    };

    public TimeRiftService(CombatService combat)
    {
        _combat = combat;
    }

    public List<TimeRiftData> GetAvailableDungeons()
    {
        var today = DateTime.UtcNow.DayOfWeek;
        return _defaultDungeons.Where(d => 
            d.IsActive && 
            (d.Type == TimeRiftType.Challenge || d.Type == TimeRiftType.Special) ||
            (d.Type == TimeRiftType.Daily) ||
            (d.Type == TimeRiftType.Weekly && today == DayOfWeek.Monday))
            .ToList();
    }

    public TimeRiftData GetDailyDungeon()
    {
        var dayOfYear = DateTime.UtcNow.DayOfYear;
        var dailyIndex = dayOfYear % _defaultDungeons.Count(d => d.Type == TimeRiftType.Daily);
        return _defaultDungeons.Where(d => d.Type == TimeRiftType.Daily).ElementAt(dailyIndex);
    }

    public TimeRiftPlayerSession? StartSession(string username, int dungeonId)
    {
        var dungeon = _defaultDungeons.FirstOrDefault(d => d.Id == dungeonId);
        if (dungeon == null)
            return null;

        var session = new TimeRiftPlayerSession
        {
            Username = username,
            DungeonId = dungeon.Id,
            DungeonName = dungeon.JapaneseName,
            CurrentFloor = 1,
            MonstersDefeated = 0,
            TotalMonsters = dungeon.MaxFloors,
            StartTime = DateTime.UtcNow,
            IsActive = true
        };

        _activeSessions[username] = session;
        return session;
    }

    public TimeRiftPlayerSession? GetSession(string username)
    {
        return _activeSessions.TryGetValue(username, out var session) ? session : null;
    }

    public async Task<(bool victory, int expGained, int gilGained, List<string> drops)> 
        ExecuteBattleAsync(User user, string monsterType)
    {
        var playerDamage = user.Status.Str + user.Status.WeaponAttack + (user.EquippedWeapon?.Attack ?? 0);
        var enemyHp = _random.Next(50, 150) + (user.Level * 10);
        var enemyDamage = _random.Next(10, 30) + (user.Level * 5);
        
        var playerTurns = (int)Math.Ceiling((double)enemyHp / playerDamage);
        var enemyTurns = (int)Math.Ceiling((double)user.MaxHP / enemyDamage);
        
        var victory = playerTurns <= enemyTurns;
        
        if (victory)
        {
            var expGained = _random.Next(20, 50) + user.Level * 2;
            var gilGained = _random.Next(10, 50);
            var drops = new List<string>();
            
            if (_random.NextDouble() > 0.7)
            {
                drops.Add("Rare Herb");
            }
            if (_random.NextDouble() > 0.9)
            {
                drops.Add("Ancient Coin");
            }
            
            return (true, expGained, gilGained, drops);
        }
        
        return (false, 0, 0, new List<string>());
    }

    public void CompleteFloor(string username)
    {
        if (!_activeSessions.TryGetValue(username, out var session))
            return;

        session.MonstersDefeated++;
        if (session.MonstersDefeated >= session.TotalMonsters)
        {
            CompleteSession(username);
        }
        else
        {
            session.CurrentFloor = (session.MonstersDefeated / (session.TotalMonsters / session.TotalMonsters)) + 1;
        }
    }

    public void CompleteSession(string username)
    {
        if (!_activeSessions.TryGetValue(username, out var session))
            return;

        session.EndTime = DateTime.UtcNow;
        session.IsCompleted = true;
        session.ClearTimeSeconds = (int)(session.EndTime.Value - session.StartTime).TotalSeconds;
        
        session.Rank = CalculateRank(session);
        
        AddToLeaderboard(session);
    }

    public void TimeOutSession(string username)
    {
        if (!_activeSessions.TryGetValue(username, out var session))
            return;

        session.EndTime = DateTime.UtcNow;
        session.IsTimeOut = true;
        session.ClearTimeSeconds = (int)(session.EndTime.Value - session.StartTime).TotalSeconds;
        session.Rank = "D";
    }

    private string CalculateRank(TimeRiftPlayerSession session)
    {
        var dungeon = _defaultDungeons.FirstOrDefault(d => d.Id == session.DungeonId);
        if (dungeon == null) return "D";

        var timeRatio = (double)session.ClearTimeSeconds / dungeon.TimeLimitSeconds;
        
        return timeRatio switch
        {
            <= 0.3 => "S",
            <= 0.5 => "A",
            <= 0.7 => "B",
            <= 0.9 => "C",
            _ => "D"
        };
    }

    private void AddToLeaderboard(TimeRiftPlayerSession session)
    {
        var entry = new TimeRiftRecord
        {
            DungeonId = session.DungeonId,
            DungeonName = session.DungeonName,
            Username = session.Username,
            ClearTimeSeconds = session.ClearTimeSeconds,
            Rank = session.Rank,
            ClearDate = DateTime.UtcNow
        };
        
        _leaderboards.Add(entry);
    }

    public List<TimeRiftRecord> GetLeaderboard(int dungeonId, int top = 10)
    {
        return _leaderboards
            .Where(l => l.DungeonId == dungeonId)
            .OrderBy(l => l.ClearTimeSeconds)
            .Take(top)
            .ToList();
    }

    public List<TimeRiftRecord> GetGlobalLeaderboard(int top = 20)
    {
        return _leaderboards
            .OrderBy(l => l.ClearTimeSeconds)
            .Take(top)
            .ToList();
    }

    public (int gil, int exp, List<string> items) CalculateRewards(string username)
    {
        if (!_activeSessions.TryGetValue(username, out var session))
            return (0, 0, new List<string>());

        var dungeon = _defaultDungeons.FirstOrDefault(d => d.Id == session.DungeonId);
        if (dungeon == null) return (0, 0, new List<string>());

        var timeBonus = session.Rank switch
        {
            "S" => 2.0,
            "A" => 1.5,
            "B" => 1.2,
            "C" => 1.0,
            _ => 0.5
        };

        var gil = (int)(dungeon.BaseGilReward * timeBonus);
        var exp = (int)(dungeon.BaseExpReward * timeBonus);
        var items = session.Drops;

        return (gil, exp, items);
    }

    public void ClearSession(string username)
    {
        _activeSessions.Remove(username);
    }

    public int GetRemainingTime(string username)
    {
        if (!_activeSessions.TryGetValue(username, out var session))
            return 0;

        var dungeon = _defaultDungeons.FirstOrDefault(d => d.Id == session.DungeonId);
        if (dungeon == null) return 0;

        var elapsed = (int)(DateTime.UtcNow - session.StartTime).TotalSeconds;
        return Math.Max(0, dungeon.TimeLimitSeconds - elapsed);
    }

    public bool IsTimeOut(string username)
    {
        return GetRemainingTime(username) <= 0 && 
               _activeSessions.TryGetValue(username, out var session) && 
               session.IsActive;
    }
}
