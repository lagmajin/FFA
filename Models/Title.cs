namespace FFA.Models;

/// <summary>
/// 称号カテゴリ
/// </summary>
public enum TitleCategory
{
    Achievement,   // 実績達成
    Level,         // レベル達成
    Combat,       // 戦闘
    Collection,   // 収集
    Exploration,  // 探索
    Social,       // 社交
    Special,      // 特殊
    Seasonal,     // 季節イベント
}

/// <summary>
/// 称号
/// </summary>
public class Title
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public TitleCategory Category { get; set; }
    public int Requirement { get; set; } = 1; // 必要条件（レベル、撃破数など）
    public string RequirementType { get; set; } = ""; // "level", "enemies", "quests", "dungeons" etc.
    public bool IsSecret { get; set; } = false;
    public bool IsUnique { get; set; } = false; // 重複獲得不可
}

/// <summary>
/// ユーザーの称号所有
/// </summary>
public class UserTitle
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public int TitleId { get; set; }
    public DateTime ObtainedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 称号データベース
/// </summary>
public static class TitleDatabase
{
    public static List<Title> Titles { get; } = new List<Title>
    {
        // レベル称号
        new Title { Id = 1, Name = "見習い冒険者", Description = "レベル5に到達", Category = TitleCategory.Level, Requirement = 5, RequirementType = "level" },
        new Title { Id = 2, Name = "冒険者", Description = "レベル10に到達", Category = TitleCategory.Level, Requirement = 10, RequirementType = "level" },
        new Title { Id = 3, Name = "中級冒険者", Description = "レベル20に到達", Category = TitleCategory.Level, Requirement = 20, RequirementType = "level" },
        new Title { Id = 4, Name = "上級冒険者", Description = "レベル30に到達", Category = TitleCategory.Level, Requirement = 30, RequirementType = "level" },
        new Title { Id = 5, Name = "熟練冒険者", Description = "レベル40に到達", Category = TitleCategory.Level, Requirement = 40, RequirementType = "level" },
        new Title { Id = 6, Name = "精英冒険者", Description = "レベル50に到達", Category = TitleCategory.Level, Requirement = 50, RequirementType = "level" },
        new Title { Id = 7, Name = "英雄", Description = "レベル75に到達", Category = TitleCategory.Level, Requirement = 75, RequirementType = "level" },
        new Title { Id = 8, Name = "伝説の勇者", Description = "レベル100に到達", Category = TitleCategory.Level, Requirement = 100, RequirementType = "level" },

        // 戦闘称号
        new Title { Id = 10, Name = "モンスター杀手", Description = "10体の敵を撃破", Category = TitleCategory.Combat, Requirement = 10, RequirementType = "enemies" },
        new Title { Id = 11, Name = "モンスターハンター", Description = "50体の敵を撃破", Category = TitleCategory.Combat, Requirement = 50, RequirementType = "enemies" },
        new Title { Id = 12, Name = "悪魔斬り", Description = "100体の敵を撃破", Category = TitleCategory.Combat, Requirement = 100, RequirementType = "enemies" },
        new Title { Id = 13, Name = "災いの元凶", Description = "500体の敵を撃破", Category = TitleCategory.Combat, Requirement = 500, RequirementType = "enemies" },
        new Title { Id = 14, Name = "死神", Description = "1000体の敵を撃破", Category = TitleCategory.Combat, Requirement = 1000, RequirementType = "enemies" },

        // クエスト称号
        new Title { Id = 20, Name = "新手冒険者", Description = "1個のクエストを完了", Category = TitleCategory.Combat, Requirement = 1, RequirementType = "quests" },
        new Title { Id = 21, Name = "クエスト 헌터", Description = "10個のクエストを完了", Category = TitleCategory.Combat, Requirement = 10, RequirementType = "quests" },
        new Title { Id = 22, Name = "クエストマスター", Description = "50個のクエストを完了", Category = TitleCategory.Combat, Requirement = 50, RequirementType = "quests" },
        new Title { Id = 23, Name = "伝説の冒険者", Description = "100個のクエストを完了", Category = TitleCategory.Combat, Requirement = 100, RequirementType = "quests" },

        // ダンジョン称号
        new Title { Id = 30, Name = "ダンジョンクリア", Description = "1個のダンジョンを攻略", Category = TitleCategory.Combat, Requirement = 1, RequirementType = "dungeons" },
        new Title { Id = 31, Name = "ダンジョンマスター", Description = "10個のダンジョンを攻略", Category = TitleCategory.Combat, Requirement = 10, RequirementType = "dungeons" },
        new Title { Id = 32, Name = "ダンジョンナイト", Description = "50個のダンジョンを攻略", Category = TitleCategory.Combat, Requirement = 50, RequirementType = "dungeons" },

        // 探索称号
        new Title { Id = 40, Name = "旅人", Description = "5つの場所を訪れる", Category = TitleCategory.Exploration, Requirement = 5, RequirementType = "locations" },
        new Title { Id = 41, Name = "探検家", Description = "10つの場所を訪れる", Category = TitleCategory.Exploration, Requirement = 10, RequirementType = "locations" },
        new Title { Id = 42, Name = "世界一周", Description = "20つの場所を訪れる", Category = TitleCategory.Exploration, Requirement = 20, RequirementType = "locations", IsSecret = true },

        // 収集称号
        new Title { Id = 50, Name = "収集家", Description = "アイテムを10個収集", Category = TitleCategory.Collection, Requirement = 10, RequirementType = "items" },
        new Title { Id = 51, Name = "寶庫", Description = "アイテムを50個収集", Category = TitleCategory.Collection, Requirement = 50, RequirementType = "items" },
        new Title { Id = 52, Name = "大富豪", Description = "10000ギルを所持", Category = TitleCategory.Collection, Requirement = 10000, RequirementType = "gold", IsUnique = true },

        // 転生称号
        new Title { Id = 60, Name = "生まれ変わる", Description = "初めて転生する", Category = TitleCategory.Special, Requirement = 1, RequirementType = "rebirths" },
        new Title { Id = 61, Name = "幾度の再生", Description = "5回転生する", Category = TitleCategory.Special, Requirement = 5, RequirementType = "rebirths" },
        new Title { Id = 62, Name = "無限の輪廻", Description = "10回転生する", Category = TitleCategory.Special, Requirement = 10, RequirementType = "rebirths" },

        // マスター称号
        new Title { Id = 70, Name = "マスターへの道", Description = "マスターになる", Category = TitleCategory.Special, Requirement = 1, RequirementType = "master" },
        new Title { Id = 71, Name = "全能の者", Description = "マスターレベル5に到達", Category = TitleCategory.Special, Requirement = 5, RequirementType = "master_level" },

        // PVP称号
        new Title { Id = 80, Name = "初勝利", Description = "PVPで勝利", Category = TitleCategory.Combat, Requirement = 1, RequirementType = "pvp_wins" },
        new Title { Id = 81, Name = "戦士", Description = "PVPで10回勝利", Category = TitleCategory.Combat, Requirement = 10, RequirementType = "pvp_wins" },
        new Title { Id = 82, Name = "闘士", Description = "PVPで50回勝利", Category = TitleCategory.Combat, Requirement = 50, RequirementType = "pvp_wins" },
        new Title { Id = 83, Name = "チャンピオン", Description = "PVPで100回勝利", Category = TitleCategory.Combat, Requirement = 100, RequirementType = "pvp_wins" },

        // ギルド称号
        new Title { Id = 90, Name = "仲間を求める", Description = "ギルドに入る", Category = TitleCategory.Social, Requirement = 1, RequirementType = "guild" },
        new Title { Id = 91, Name = "Guild Master", Description = "ギルドを创立", Category = TitleCategory.Social, Requirement = 1, RequirementType = "guild_leader", IsUnique = true },

        // 釣り称号
        new Title { Id = 100, Name = "钓鱼新手", Description = "魚を1匹釣る", Category = TitleCategory.Collection, Requirement = 1, RequirementType = "fish" },
        new Title { Id = 101, Name = "钓鱼高手", Description = "魚を10匹釣る", Category = TitleCategory.Collection, Requirement = 10, RequirementType = "fish" },
        new Title { Id = 102, Name = "钓鱼大师", Description = "魚を50匹釣る", Category = TitleCategory.Collection, Requirement = 50, RequirementType = "fish" },

        // 採掘称号
        new Title { Id = 110, Name = "鉱夫", Description = "鉱石を10個採掘", Category = TitleCategory.Collection, Requirement = 10, RequirementType = "ores" },
        new Title { Id = 111, Name = "鉱山師", Description = "鉱石を50個採掘", Category = TitleCategory.Collection, Requirement = 50, RequirementType = "ores" },
        new Title { Id = 112, Name = "鉱帝", Description = "鉱石を100個採掘", Category = TitleCategory.Collection, Requirement = 100, RequirementType = "ores" },

        //  специальные称号
        new Title { Id = 200, Name = "テストプレイヤー", Description = "テストプレイヤーとして参加", Category = TitleCategory.Special, Requirement = 1, RequirementType = "test_player", IsUnique = true },
        new Title { Id = 201, Name = " Original", Description = "最初期の冒険者", Category = TitleCategory.Special, Requirement = 1, RequirementType = "founder", IsUnique = true },
    };

    public static Title? GetById(int id)
    {
        return Titles.FirstOrDefault(t => t.Id == id);
    }

    public static List<Title> GetByCategory(TitleCategory category)
    {
        return Titles.Where(t => t.Category == category).ToList();
    }

    public static List<Title> GetUnlocked(int requirement, string requirementType)
    {
        return Titles.Where(t => t.RequirementType == requirementType && t.Requirement <= requirement).ToList();
    }
}
