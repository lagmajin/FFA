using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using FFA.Models;

namespace FFA.Services;

public class PrisonService
{
    private readonly string _databasePath;

    public PrisonService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "prison.db");
    }

    // 収監する
    public PrisonResult Imprison(string username, PrisonReason reason, int sentenceMinutes, string description = "")
    {
        try
        {
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new PrisonResult { Success = false, Message = "ユーザーが見つかりません" };

            // 収監記録を作成
            var record = new PrisonRecord
            {
                Username = username,
                Reason = reason,
                ImprisonedAt = DateTime.UtcNow,
                ReleaseAt = DateTime.UtcNow.AddMinutes(sentenceMinutes),
                SentenceMinutes = sentenceMinutes,
                CrimeDescription = description
            };

            using var db = new LiteDatabase(_databasePath);
            var records = db.GetCollection<PrisonRecord>("prison_records");
            records.Insert(record);

            return new PrisonResult
            {
                Success = true,
                Message = $"収監されました！刑期：{sentenceMinutes}分",
                SentenceMinutes = sentenceMinutes,
                ReleaseAt = record.ReleaseAt
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("PrisonService.Imprison error: " + ex.Message);
            return new PrisonResult { Success = false, Message = "エラーが発生しました" };
        }
    }

    // 収監されているかチェック
    public bool IsImprisoned(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var records = db.GetCollection<PrisonRecord>("prison_records");
            var record = records.FindOne(r => r.Username == username && !r.IsReleased);
            if (record == null) return false;

            // 刑期結束チェック
            if (DateTime.UtcNow >= record.ReleaseAt)
            {
                record.IsReleased = true;
                records.Update(record);
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    // 収監情報を取得
    public PrisonRecord? GetPrisonInfo(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var records = db.GetCollection<PrisonRecord>("prison_records");
            return records.FindOne(r => r.Username == username && !r.IsReleased);
        }
        catch
        {
            return null;
        }
    }

    // 収監残留時間を取得（分）
    public int GetRemainingMinutes(string username)
    {
        var record = GetPrisonInfo(username);
        if (record == null) return 0;

        var remaining = (record.ReleaseAt - DateTime.UtcNow).TotalMinutes;
        return remaining > 0 ? (int)remaining : 0;
    }

    // 刑務所タスクを行う
    public PrisonTaskResult DoTask(string username)
    {
        try
        {
            // 収監中かチェック
            if (!IsImprisoned(username))
                return new PrisonTaskResult { Success = false, Message = "収監されていません" };

            var task = PrisonDatabase.GetRandomTask();
            if (task == null)
                return new PrisonTaskResult { Success = false, Message = "タスクが見つかりません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new PrisonTaskResult { Success = false, Message = "ユーザーが見つかりません" };

            // 奖励を適用
            user.Exp += task.ExpReward;
            user.Gil += task.GilReward;
            userService.UpdateUser(user);

            return new PrisonTaskResult
            {
                Success = true,
                Message = $"{task.Name}を完了した！ Exp+{task.ExpReward}",
                TaskName = task.Name,
                ExpReward = task.ExpReward,
                GilReward = task.GilReward
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("PrisonService.DoTask error: " + ex.Message);
            return new PrisonTaskResult { Success = false, Message = "エラーが発生しました" };
        }
    }

    // 脱獄を試みる
    public bool AttemptEscape(string username)
    {
        var random = new Random();
        int chance = random.Next(100);

        // 30%の成功率
        if (chance < 30)
        {
            // 脱獄成功
            try
            {
                using var db = new LiteDatabase(_databasePath);
                var records = db.GetCollection<PrisonRecord>("prison_records");
                var record = records.FindOne(r => r.Username == username && !r.IsReleased);
                if (record != null)
                {
                    record.IsReleased = true;
                    records.Update(record);
                }
            }
            catch { }
            return true;
        }

        // 脱獄失敗 - 刑期が延⻑
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var records = db.GetCollection<PrisonRecord>("prison_records");
            var record = records.FindOne(r => r.Username == username && !r.IsReleased);
            if (record != null)
            {
                record.SentenceMinutes += 10;
                record.ReleaseAt = record.ReleaseAt.AddMinutes(10);
                records.Update(record);
            }
        }
        catch { }

        return false;
    }
}

/// <summary>
/// 収監結果
/// </summary>
public class PrisonResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int SentenceMinutes { get; set; }
    public DateTime ReleaseAt { get; set; }
}

/// <summary>
/// 刑務所タスク結果
/// </summary>
public class PrisonTaskResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string TaskName { get; set; } = "";
    public int ExpReward { get; set; }
    public int GilReward { get; set; }
}
