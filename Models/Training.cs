namespace FFA.Models;

public enum TrainingType
{
    Dojo,        // 道場 - 基礎トレーニング
    Arena,        // 訓練場 - 模擬戦
    Meditation,   // 瞑想所 - 精神的トレーニング
    Smithy,       // 鍛冶場 - 武器鍛錬
    Library,      // 書庫 - 知識向上
    Garden        // 庭園 - 心を养う
}

public class TrainingSession
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public TrainingType Type { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsCompleted { get; set; }
    public int ExpGained { get; set; }
    public int StatBonus { get; set; } // ステータス bonus
    public int GilSpent { get; set; }
}

public class DailyTraining
{
    public string Username { get; set; } = "";
    public int TotalSessionsToday { get; set; }
    public DateTime LastTrainingDate { get; set; }
    public int TotalExpToday { get; set; }
}

public class TrainingRecord
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public TrainingType Type { get; set; }
    public DateTime CompletedAtUtc { get; set; }
    public int ExpGained { get; set; }
    public int StatPointsGained { get; set; }
    public string StatType { get; set; } = ""; // STR, DEX, INT, VIT, AGI, LUK
}
