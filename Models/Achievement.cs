namespace FFA.Models;

/// <summary>
/// 実績タイプ
/// </summary>
public enum AchievementType
{
    LevelUp,          // レベル上げ
    DefeatEnemy,      // 敵撃破
    CollectItem,      // アイテム収集
    VisitLocation,    // 場所訪問
    CompleteQuest,    // クエスト完了
    WinDungeon,       // ダンジョン攻略
    Rebirth,          // 転生
    ReachMaster,      // マスター到達
    JoinGuild,        // ギルド加入
    BuildCountry,     // 国家建設
    WinPvP,           // PVP勝利
    TradeItem,        // アイテム取引
}

/// <summary>
/// 実績詳細
/// </summary>
public class Achievement
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public AchievementType Type { get; set; }
    public int TargetCount { get; set; } = 1; // 目標達成回数
    public bool IsSecret { get; set; } = false; // シークレット実績か
}

/// <summary>
/// ユーザーの実績進捗
/// </summary>
public class UserAchievement
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public int AchievementId { get; set; }
    public int CurrentCount { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// 実績データベース
/// </summary>
public static class AchievementDatabase
{
    public static List<Achievement> Achievements { get; } = new List<Achievement>
    {
        // レベル関連
        new Achievement { Id = 1, Name = "初心者", Description = "レベル5に到達", Type = AchievementType.LevelUp, TargetCount = 5 },
        new Achievement { Id = 2, Name = "中級者", Description = "レベル10に到達", Type = AchievementType.LevelUp, TargetCount = 10 },
        new Achievement { Id = 3, Name = "上級者", Description = "レベル30に到達", Type = AchievementType.LevelUp, TargetCount = 30 },
        new Achievement { Id = 4, Name = "熟練者", Description = "レベル50に到達", Type = AchievementType.LevelUp, TargetCount = 50 },
        new Achievement { Id = 5, Name = "伝説の勇者", Description = "レベル100に到達", Type = AchievementType.LevelUp, TargetCount = 100 },

        // 敵撃破関連
        new Achievement { Id = 10, Name = "雑魚杀手", Description = "10体の敵を撃破", Type = AchievementType.DefeatEnemy, TargetCount = 10 },
        new Achievement { Id = 11, Name = "モンスターハンター", Description = "50体の敵を撃破", Type = AchievementType.DefeatEnemy, TargetCount = 50 },
        new Achievement { Id = 12, Name = "悪魔斬り", Description = "100体の敵を撃破", Type = AchievementType.DefeatEnemy, TargetCount = 100 },
        new Achievement { Id = 13, Name = "災いの元凶", Description = "500体の敵を撃破", Type = AchievementType.DefeatEnemy, TargetCount = 500 },

        // アイテム関連
        new Achievement { Id = 20, Name = "収集家", Description = "アイテムを10個収集", Type = AchievementType.CollectItem, TargetCount = 10 },
        new Achievement { Id = 21, Name = "寶庫", Description = "アイテムを50個収集", Type = AchievementType.CollectItem, TargetCount = 50 },
        new Achievement { Id = 22, Name = "富人", Description = "10000ギルを所持", Type = AchievementType.CollectItem, TargetCount = 10000 },

        // 場所訪問関連
        new Achievement { Id = 30, Name = "旅人", Description = "5つの場所を訪れる", Type = AchievementType.VisitLocation, TargetCount = 5 },
        new Achievement { Id = 31, Name = "探検家", Description = "10つの場所を訪れる", Type = AchievementType.VisitLocation, TargetCount = 10 },
        new Achievement { Id = 32, Name = "世界一周", Description = "全ての場所を訪れる", Type = AchievementType.VisitLocation, TargetCount = 20, IsSecret = true },

        // クエスト関連
        new Achievement { Id = 40, Name = "新手冒険者", Description = "最初のクエストを完了", Type = AchievementType.CompleteQuest, TargetCount = 1 },
        new Achievement { Id = 41, Name = "クエストマスター", Description = "10個のクエストを完了", Type = AchievementType.CompleteQuest, TargetCount = 10 },
        new Achievement { Id = 42, Name = "伝説の冒険者", Description = "50個のクエストを完了", Type = AchievementType.CompleteQuest, TargetCount = 50 },

        // ダンジョン関連
        new Achievement { Id = 50, Name = "ダンジョンクリア", Description = "最初のダンジョンを攻略", Type = AchievementType.WinDungeon, TargetCount = 1 },
        new Achievement { Id = 51, Name = "ダンジョンマスター", Description = "10個のダンジョンを攻略", Type = AchievementType.WinDungeon, TargetCount = 10 },

        // 転生関連
        new Achievement { Id = 60, Name = "生まれ変わる", Description = "初めて転生する", Type = AchievementType.Rebirth, TargetCount = 1 },
        new Achievement { Id = 61, Name = "幾度の再生", Description = "5回転生する", Type = AchievementType.Rebirth, TargetCount = 5 },
        new Achievement { Id = 62, Name = "無限の輪廻", Description = "10回転生する", Type = AchievementType.Rebirth, TargetCount = 10 },

        // マスター関連
        new Achievement { Id = 70, Name = "マスターへの道", Description = "マスターになる", Type = AchievementType.ReachMaster, TargetCount = 1 },
        new Achievement { Id = 71, Name = "全能の者", Description = "マスターレベル5に到達", Type = AchievementType.ReachMaster, TargetCount = 5 },

        // ギルド関連
        new Achievement { Id = 80, Name = "仲間を求める", Description = "ギルドに入る", Type = AchievementType.JoinGuild, TargetCount = 1 },

        // PVP関連
        new Achievement { Id = 90, Name = "初勝利", Description = "PVPで勝利", Type = AchievementType.WinPvP, TargetCount = 1 },
        new Achievement { Id = 91, Name = "戦士", Description = "PVPで10回勝利", Type = AchievementType.WinPvP, TargetCount = 10 },
    };

    public static Achievement? GetById(int id)
    {
        return Achievements.FirstOrDefault(a => a.Id == id);
    }

    public static List<Achievement> GetByType(AchievementType type)
    {
        return Achievements.Where(a => a.Type == type).ToList();
    }
}
