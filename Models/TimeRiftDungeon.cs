namespace FFA.Models;

public enum TimeRiftDungeonType
{
    Daily,       // 日替わりダンジョン
    Weekly,      // 週替わりダンジョン
    Special,     // 特別なダンジョン
    Challenge    // チャレンジモード
}

public class TimeRiftDungeon
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string JapaneseName { get; set; } = "";
    public string Description { get; set; } = "";
    public TimeRiftDungeonType Type { get; set; }
    public int RecommendedLevel { get; set; } = 1;
    
    // 時間制限（秒）
    public int TimeLimitSeconds { get; set; } = 300; // デフォルト5分
    
    // 階層の深さ
    public int MaxFloors { get; set; } = 10;
    
    // 報酬
    public int BaseGilReward { get; set; } = 1000;
    public int BaseExpReward { get; set; } = 500;
    public string RewardItemId { get; set; } = "";
    
    // 出現する敵タイプ
    public List<string> MonsterTypes { get; set; } = new();
    
    // 有効期間
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // アクティブか
    public bool IsActive { get; set; } = true;
    
    // リセット時間
    public string ResetTime { get; set; } = "00:00";
}

public class TimeRiftSession
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public int DungeonId { get; set; }
    public string DungeonName { get; set; } = "";
    
    // 進行状況
    public int CurrentFloor { get; set; } = 1;
    public int MonstersDefeated { get; set; } = 0;
    public int TotalMonsters { get; set; } = 10;
    
    // 獲得リソース
    public int TotalGil { get; set; } = 0;
    public int TotalExp { get; set; } = 0;
    public List<string> Drops { get; set; } = new();
    
    // 時間
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCompleted { get; set; }
    public bool IsTimeOut { get; set; }
    
    // クリア時間（秒）
    public int ClearTimeSeconds { get; set; }
    
    // 評価
    public string Rank { get; set; } = ""; // S, A, B, C, D
}

public class TimeRiftLeaderboard
{
    public int Id { get; set; }
    public int DungeonId { get; set; }
    public string DungeonName { get; set; } = "";
    public string Username { get; set; } = "";
    public int ClearTimeSeconds { get; set; }
    public string Rank { get; set; } = "";
    public DateTime ClearDate { get; set; }
}
