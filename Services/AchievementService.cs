using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using FFA.Models;

namespace FFA.Services;

public class AchievementService
{
    private readonly string _databasePath;

    public AchievementService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "achievements.db");
    }

    // ユーザーの実績進捗を取得
    public List<UserAchievement> GetUserAchievements(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var userAchievements = db.GetCollection<UserAchievement>("userachievements");
            return userAchievements.Find(a => a.Username == username).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine("AchievementService.GetUserAchievements error: " + ex.Message);
            return new List<UserAchievement>();
        }
    }

    // 実績を達成する
    public AchievementProgressResult ProgressAchievement(string username, AchievementType type, int count = 1)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var userAchievements = db.GetCollection<UserAchievement>("userachievements");

            // このタイプの実績を取得
            var achievements = AchievementDatabase.GetByType(type);
            var results = new List<AchievementProgressInfo>();

            foreach (var achievement in achievements)
            {
                var userAchievement = userAchievements.FindOne(a => a.Username == username && a.AchievementId == achievement.Id);

                if (userAchievement == null)
                {
                    // 新規作成
                    userAchievement = new UserAchievement
                    {
                        Username = username,
                        AchievementId = achievement.Id,
                        CurrentCount = 0,
                        IsCompleted = false
                    };
                    userAchievements.Insert(userAchievement);
                }

                if (userAchievement.IsCompleted)
                    continue;

                // 進捗を更新
                userAchievement.CurrentCount += count;

                // 達成チェック
                if (userAchievement.CurrentCount >= achievement.TargetCount)
                {
                    userAchievement.IsCompleted = true;
                    userAchievement.CompletedAt = DateTime.UtcNow;
                    results.Add(new AchievementProgressInfo
                    {
                        AchievementId = achievement.Id,
                        AchievementName = achievement.Name,
                        JustCompleted = true
                    });
                }

                userAchievements.Update(userAchievement);
            }

            return new AchievementProgressResult
            {
                Success = true,
                ProgressedAchievements = results
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("AchievementService.ProgressAchievement error: " + ex.Message);
            return new AchievementProgressResult { Success = false };
        }
    }

    // 特定の実績を達成する
    public bool CompleteAchievement(string username, int achievementId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var userAchievements = db.GetCollection<UserAchievement>("userachievements");

            var userAchievement = userAchievements.FindOne(a => a.Username == username && a.AchievementId == achievementId);

            if (userAchievement == null)
            {
                userAchievement = new UserAchievement
                {
                    Username = username,
                    AchievementId = achievementId,
                    CurrentCount = 1,
                    IsCompleted = true,
                    CompletedAt = DateTime.UtcNow
                };
                userAchievements.Insert(userAchievement);
            }
            else if (!userAchievement.IsCompleted)
            {
                userAchievement.IsCompleted = true;
                userAchievement.CompletedAt = DateTime.UtcNow;
                userAchievements.Update(userAchievement);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("AchievementService.CompleteAchievement error: " + ex.Message);
            return false;
        }
    }

    // 実績を達成した总数を取得
    public int GetCompletedCount(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var userAchievements = db.GetCollection<UserAchievement>("userachievements");
            return userAchievements.Count(a => a.Username == username && a.IsCompleted);
        }
        catch (Exception ex)
        {
            Console.WriteLine("AchievementService.GetCompletedCount error: " + ex.Message);
            return 0;
        }
    }

    // 達成率を取得（percentage）
    public double GetCompletionRate(string username)
    {
        try
        {
            var completed = GetCompletedCount(username);
            var total = AchievementDatabase.Achievements.Count;
            return total > 0 ? (double)completed / total * 100 : 0;
        }
        catch
        {
            return 0;
        }
    }
}

// 実績進捗結果
public class AchievementProgressResult
{
    public bool Success { get; set; }
    public List<AchievementProgressInfo> ProgressedAchievements { get; set; } = new();
}

public class AchievementProgressInfo
{
    public int AchievementId { get; set; }
    public string AchievementName { get; set; } = "";
    public bool JustCompleted { get; set; }
}
