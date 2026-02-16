namespace FFA.Models;

/// <summary>
/// デイリーのログ・報酬データ
/// </summary>
public class UserDailyLog
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public DateTime LastLoginDate { get; set; } = DateTime.UtcNow;
    public DateTime LastClaimDate { get; set; } = DateTime.MinValue;
    public int ConsecutiveDays { get; set; } = 0; // 連続ログイン日数
    public int TotalLoginDays { get; set; } = 0; // 累計ログイン日数
    public int TotalClaims { get; set; } = 0; // 累計報酬受取回数
    public int WeeklyBonus { get; set; } = 0; // 今週のボーナスpt
    public int MonthlyBonus { get; set; } = 0; // 今月のボーナスpt
}

/// <summary>
/// 報酬タイプ
/// </summary>
public enum RewardType
{
    Gil,
    Exp,
    Premium,
    OldCoin,
    Item,
    SkillPoint
}

/// <summary>
/// 日次報酬定義
/// </summary>
public class DailyRewardDefinition
{
    public int DayNumber { get; set; } // 1-30 or more
    public RewardType Type { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "🎁";
    public bool IsSpecial { get; set; } = false; // 特別な報酬（週次/月次ボンジョ）
}

/// <summary>
/// ユーザーの本日の活動ログ
/// </summary>
public class DailyActivityLog
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public string ActivityType { get; set; } = ""; // login, quest_complete, dungeon_clear, etc.
    public string Description { get; set; } = "";
    public int PointsEarned { get; set; } = 0;
}

/// <summary>
/// ログインテストリー報酬
/// </summary>
public class LoginStreakReward
{
    public int DaysRequired { get; set; }
    public RewardType Type { get; set; }
    public int Amount { get; set; }
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "⭐";
}
