namespace FFA.Models;

/// <summary>
/// 職業タイプ
/// </summary>
public enum Job
{
    Warrior,
    Monk,
    WhiteMage,
    BlackMage
}

/// <summary>
/// 職業詳細情報クラス
/// </summary>
public class JobInfo
{
    public Job Job { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "⚔️";
    public string Color { get; set; } = "#000000";
    public PlayerStatus BonusStatus { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    public string Role { get; set; } = ""; // Tank, Healer, DPS, etc.
    public string WeaponType { get; set; } = "";
}

/// <summary>
/// 職業データベース
/// </summary>
public static class JobDatabase
{
    public static List<JobInfo> Jobs { get; } = new List<JobInfo>
    {
        new JobInfo
        {
            Job = Job.Warrior,
            Name = "戦士",
            Description = "高い防御力と体力を持つ戦闘の主力。敵の攻撃を引きつけて仲間を守る。",
            Icon = "⚔️",
            Color = "#8B4513",
            Role = "坦克",
            WeaponType = "剣、斧、槍",
            BonusStatus = new PlayerStatus { Str = 5, Vit = 8 },
            Skills = new List<string> { "タンクスタンス", "挑発", "防衛姿勢" }
        },
        new JobInfo
        {
            Job = Job.Monk,
            Name = "武闘家",
            Description = "素手で戦う拳の使い手。高速な連撃と高い会心の一撃を得意とする。",
            Icon = "🥋",
            Color = "#FFD700",
            Role = "近接DPS",
            WeaponType = "拳套、指虎",
            BonusStatus = new PlayerStatus { Str = 7, Dex = 6, Vit = 4 },
            Skills = new List<string> { "連撃", "会心の一撃", "風の拳" }
        },
        new JobInfo
        {
            Job = Job.WhiteMage,
            Name = "白魔道士",
            Description = "光の魔法を使う回復職。仲間の生命を守り、状態異常を回復する。",
            Icon = "🍀",
            Color = "#FFFFFF",
            Role = "回復",
            WeaponType = "杖、聖書",
            BonusStatus = new PlayerStatus { Int = 6 },
            Skills = new List<string> { "回復魔法", "聖なる光", "状態回復" }
        },
        new JobInfo
        {
            Job = Job.BlackMage,
            Name = "黒魔道士",
            Description = "闇の魔法を使う攻撃職。強力な元素魔法で敵を一掃する。",
            Icon = "⚫",
            Color = "#000000",
            Role = "魔法DPS",
            WeaponType = "魔杖、魔道書",
            BonusStatus = new PlayerStatus { Int = 8 },
            Skills = new List<string> { "火球術", "氷結魔法", "雷撃" }
        }
    };

    /// <summary>
    /// 職業に対応する詳細情報を取得
    /// </summary>
    /// <param name="job">職業</param>
    /// <returns>職業詳細情報</returns>
    public static JobInfo GetJobInfo(Job job)
    {
        try
        {
            return Jobs.FirstOrDefault(j => j.Job == job) ?? Jobs[0];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobDatabase.GetJobInfo 例外: {ex.Message} - {ex.StackTrace}");
            return new JobInfo { Job = Job.Warrior, Name = "戦士", Icon = "⚔️", Color = "#8B4513" };
        }
    }

    /// <summary>
    /// 全職業の詳細情報を取得
    /// </summary>
    /// <returns>職業詳細情報のリスト</returns>
    public static List<JobInfo> GetAllJobs()
    {
        try
        {
            return Jobs;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobDatabase.GetAllJobs 例外: {ex.Message} - {ex.StackTrace}");
            return new List<JobInfo>();
        }
    }
}
