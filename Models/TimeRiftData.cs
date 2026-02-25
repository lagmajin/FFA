namespace FFA.Models;

public enum TimeRiftType
{
    Daily,
    Weekly,
    Special,
    Challenge
}

public class TimeRiftData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string JapaneseName { get; set; } = "";
    public string Description { get; set; } = "";
    public TimeRiftType Type { get; set; }
    public int RecommendedLevel { get; set; } = 1;
    public int TimeLimitSeconds { get; set; } = 300;
    public int MaxFloors { get; set; } = 10;
    public int BaseGilReward { get; set; } = 1000;
    public int BaseExpReward { get; set; } = 500;
    public string RewardItemId { get; set; } = "";
    public List<string> MonsterTypes { get; set; } = new();
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string ResetTime { get; set; } = "00:00";
}

public class TimeRiftPlayerSession
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public int DungeonId { get; set; }
    public string DungeonName { get; set; } = "";
    public int CurrentFloor { get; set; } = 1;
    public int MonstersDefeated { get; set; } = 0;
    public int TotalMonsters { get; set; } = 10;
    public int TotalGil { get; set; } = 0;
    public int TotalExp { get; set; } = 0;
    public List<string> Drops { get; set; } = new();
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCompleted { get; set; }
    public bool IsTimeOut { get; set; }
    public int ClearTimeSeconds { get; set; }
    public string Rank { get; set; } = "";
}

public class TimeRiftRecord
{
    public int Id { get; set; }
    public int DungeonId { get; set; }
    public string DungeonName { get; set; } = "";
    public string Username { get; set; } = "";
    public int ClearTimeSeconds { get; set; }
    public string Rank { get; set; } = "";
    public DateTime ClearDate { get; set; }
}
