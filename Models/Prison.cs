namespace FFA.Models;

/// <summary>
/// 収監理由
/// </summary>
public enum PrisonReason
{
    BlackMarket,    // 違法取引
    Theft,          // 盗賊
    Assault,        // 暴行
    Murder,         // 殺人
    Smuggling,      // 密輸
}

/// <summary>
/// 収監情報
/// </summary>
public class PrisonRecord
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public PrisonReason Reason { get; set; }
    public DateTime ImprisonedAt { get; set; }
    public DateTime ReleaseAt { get; set; } // 解放予定時刻
    public int SentenceMinutes { get; set; } // 刑期（分）
    public bool IsReleased { get; set; } = false;
    public string CrimeDescription { get; set; } = ""; // 罪行説明
}

/// <summary>
/// 刑務所タスク
/// </summary>
public class PrisonTask
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int ExpReward { get; set; } // 経験値奖励
    public int GilReward { get; set; } // ギル奖励（少ない）
}

/// <summary>
/// 刑務所データベース
/// </summary>
public static class PrisonDatabase
{
    public static List<PrisonTask> Tasks { get; } = new List<PrisonTask>
    {
        new PrisonTask { Id = 1, Name = "土を掘る", Description = "穴を掘る...", ExpReward = 5, GilReward = 1 },
        new PrisonTask { Id = 2, Name = "石を運ぶ", Description = "重い石を運ぶ...", ExpReward = 8, GilReward = 2 },
        new PrisonTask { Id = 3, Name = "水を汲む", Description = "水を汲んでくる...", ExpReward = 6, GilReward = 1 },
        new PrisonTask { Id = 4, Name = "草を刈る", Description = "牢獄の草を刈る...", ExpReward = 4, GilReward = 1 },
        new PrisonTask { Id = 5, Name = "祈り", Description = "罪を懺悔する...", ExpReward = 10, GilReward = 0 },
    };

    public static PrisonTask? GetRandomTask()
    {
        var random = new Random();
        return Tasks[random.Next(Tasks.Count)];
    }
}
