namespace FFA.Models;

/// <summary>
/// 連戦型バトルのセッション情報
/// </summary>
public class EndlessBattleSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = "";
    public int CurrentWave { get; set; } = 1;
    public int MaxWaveReached { get; set; } = 0;
    public int TotalExpEarned { get; set; } = 0;
    public int TotalGilEarned { get; set; } = 0;
    public List<EndlessBattleRecord> BattleRecords { get; set; } = new();
    public List<Enemy> CurrentEnemies { get; set; } = new();
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public bool IsActive { get; set; } = true;
    public int PlayerHP { get; set; } = 0;
    public int PlayerMP { get; set; } = 0;
    public int PlayerMaxHP { get; set; } = 0;
    public int PlayerMaxMP { get; set; } = 0;
    
    /// <summary>
    /// 現在のウェーブの難易度倍率
    /// </summary>
    public double DifficultyMultiplier => 1.0 + (CurrentWave - 1) * 0.15;
    
    /// <summary>
    /// 次のウェーブの報酬倍率
    /// </summary>
    public double RewardMultiplier => 1.0 + (CurrentWave - 1) * 0.1;
}

/// <summary>
/// 連戦バトルの戦闘記録
/// </summary>
public class EndlessBattleRecord
{
    public int Wave { get; set; }
    public string EnemyName { get; set; } = "";
    public bool Victory { get; set; }
    public int DamageDealt { get; set; }
    public int DamageTaken { get; set; }
    public int ExpEarned { get; set; }
    public int GilEarned { get; set; }
    public List<string> ItemsDropped { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 連戦型バトルの難易度設定
/// </summary>
public enum EndlessBattleDifficulty
{
    Easy,       // 簡単（通常モンスターより少し強い）
    Normal,     // 普通（エリート級）
    Hard,       // 難しい（レア級）
    Extreme     // 激難（ボス級）
}

/// <summary>
/// 連戦型バトルの報酬
/// </summary>
public class EndlessBattleReward
{
    public int Exp { get; set; }
    public int Gil { get; set; }
    public List<string> Items { get; set; } = new();
    public int BonusExp { get; set; } // 連戦ボーナス
    public int BonusGil { get; set; } // 連戦ボーナス
    public int TotalExp => Exp + BonusExp;
    public int TotalGil => Gil + BonusGil;
}

/// <summary>
/// 連戦型バトルの統計
/// </summary>
public class EndlessBattleStats
{
    public int TotalSessions { get; set; }
    public int TotalVictories { get; set; }
    public int TotalDefeats { get; set; }
    public int MaxWaveReached { get; set; }
    public int TotalExpEarned { get; set; }
    public int TotalGilEarned { get; set; }
    public int TotalMonstersDefeated { get; set; }
    public double WinRate => TotalSessions > 0 ? (double)TotalVictories / TotalSessions * 100 : 0;
}
