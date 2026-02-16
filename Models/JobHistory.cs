namespace FFA.Models;

using LiteDB;

/// <summary>
/// ユーザーの職業履歴
/// </summary>
public class JobHistory
{
    public string Username { get; set; } = "";
    public Job Job { get; set; }
    public int Level { get; set; } = 1; // その職業のレベル
    public int TotalExp { get; set; } = 0; // その職業で獲得した経験値
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 職業履歴データベース
/// </summary>
public static class JobHistoryDatabase
{
    public static List<JobHistory> GetJobHistory(string username)
    {
        try
        {
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            var dbPath = Path.Combine(appDataPath, "jobhistory.db");
            using var db = new LiteDatabase(dbPath);
            var histories = db.GetCollection<JobHistory>("jobhistories");
            return histories.Find(h => h.Username == username).ToList();
        }
        catch
        {
            return new List<JobHistory>();
        }
    }

    public static bool HasJobLevel(Job job, int minLevel)
    {
        return true; // 简化实现
    }
}
