using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using FFA.Models;

namespace FFA.Services
{
    public class TrainingService
    {
        private readonly string _databasePath;
        private readonly int MaxDailySessions = 10;
        
        public TrainingService()
        {
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _databasePath = Path.Combine(appDataPath, "training.db");
        }

        public TrainingSession? StartTraining(string username, TrainingType type, int durationMinutes)
        {
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null) return null;

            // 費用計算
            int cost = CalculateCost(type, durationMinutes);
            if (user.Gil < cost) return null;

            // 日次限制チェック
            var daily = GetDailyTraining(username);
            if (daily == null || daily.TotalSessionsToday >= MaxDailySessions) return null;

            // 支払い
            user.Gil -= cost;
            userService.UpdateUser(user);

            // セッション作成
            var session = new TrainingSession
            {
                Username = username,
                Type = type,
                StartTimeUtc = DateTime.UtcNow,
                DurationMinutes = durationMinutes,
                IsCompleted = false,
                GilSpent = cost
            };

            using var db = new LiteDatabase(_databasePath);
            var sessions = db.GetCollection<TrainingSession>("sessions");
            sessions.Insert(session);

            // 日次更新
            UpdateDailyTraining(username);
            
            return session;
        }

        private int CalculateCost(TrainingType type, int durationMinutes)
        {
            int baseCost = type switch
            {
                TrainingType.Dojo => 50,
                TrainingType.Arena => 100,
                TrainingType.Meditation => 30,
                TrainingType.Smithy => 80,
                TrainingType.Library => 40,
                TrainingType.Garden => 25,
                _ => 50
            };
            
            return baseCost * durationMinutes;
        }

        public bool CompleteTraining(int sessionId)
        {
            using var db = new LiteDatabase(_databasePath);
            var sessions = db.GetCollection<TrainingSession>("sessions");
            var session = sessions.FindById(sessionId);
            
            if (session == null || session.IsCompleted) return false;
            if (DateTime.UtcNow < session.StartTimeUtc.AddMinutes(session.DurationMinutes)) return false;

            session.IsCompleted = true;
            
            // 経験値とステータス bonus 計算
            var (exp, statPoints, statType) = CalculateRewards(session.Type, session.DurationMinutes);
            session.ExpGained = exp;
            session.StatBonus = statPoints;
            
            sessions.Update(session);

            // ユーザーに奖励
            var userService = new UserService();
            userService.AddExpAndHandleLevel(session.Username, exp);

            // ステータスポints付与
            var user = userService.GetByUsername(session.Username);
            if (user != null)
            {
                switch (statType)
                {
                    case "Str":
                        user.Status.Str += statPoints;
                        break;
                    case "Dex":
                        user.Status.Dex += statPoints;
                        break;
                    case "Int":
                        user.Status.Int += statPoints;
                        break;
                    case "Vit":
                        user.Status.Vit += statPoints;
                        break;
                    case "Agi":
                        user.Status.Agi += statPoints;
                        break;
                    case "Luk":
                        user.Status.Luk += statPoints;
                        break;
                }
                userService.UpdateUser(user);
            }

            // 記録保存
            var record = new TrainingRecord
            {
                Username = session.Username,
                Type = session.Type,
                CompletedAtUtc = DateTime.UtcNow,
                ExpGained = exp,
                StatPointsGained = statPoints,
                StatType = statType
            };
            
            var records = db.GetCollection<TrainingRecord>("records");
            records.Insert(record);

            return true;
        }

        private (int exp, int statPoints, string statType) CalculateRewards(TrainingType type, int durationMinutes)
        {
            // 基本奖励
            int baseExp = type switch
            {
                TrainingType.Dojo => 20,
                TrainingType.Arena => 35,
                TrainingType.Meditation => 15,
                TrainingType.Smithy => 25,
                TrainingType.Library => 18,
                TrainingType.Garden => 12,
                _ => 20
            };

            int exp = baseExp * durationMinutes / 10; // 10分あたり
            
            // ステータス bonus (1-3ポイント)
            string statType = type switch
            {
                TrainingType.Dojo => "Str",
                TrainingType.Arena => "Vit",
                TrainingType.Meditation => "Int",
                TrainingType.Smithy => "Dex",
                TrainingType.Library => "Int",
                TrainingType.Garden => "Agi",
                _ => "Str"
            };
            
            int statPoints = Math.Max(1, durationMinutes / 30);
            if (durationMinutes >= 60) statPoints += 1;
            if (durationMinutes >= 120) statPoints += 2;

            return (exp, statPoints, statType);
        }

        public List<TrainingSession> GetActiveSessions(string username)
        {
            using var db = new LiteDatabase(_databasePath);
            var sessions = db.GetCollection<TrainingSession>("sessions");
            return sessions.Find(s => s.Username == username && !s.IsCompleted).ToList();
        }

        public List<TrainingSession> GetCompletedSessions(string username, int limit = 10)
        {
            using var db = new LiteDatabase(_databasePath);
            var sessions = db.GetCollection<TrainingSession>("sessions");
            return sessions.Find(s => s.Username == username && s.IsCompleted)
                .OrderByDescending(s => s.StartTimeUtc)
                .Take(limit)
                .ToList();
        }

        public DailyTraining? GetDailyTraining(string username)
        {
            using var db = new LiteDatabase(_databasePath);
            var daily = db.GetCollection<DailyTraining>("daily");
            var result = daily.FindOne(d => d.Username == username);
            
            if (result == null)
            {
                result = new DailyTraining { Username = username };
                daily.Insert(result);
            }
            
            // 日付变更チェック
            if (result.LastTrainingDate.Date != DateTime.UtcNow.Date)
            {
                result.TotalSessionsToday = 0;
                result.TotalExpToday = 0;
                result.LastTrainingDate = DateTime.UtcNow;
                daily.Update(result);
            }
            
            return result;
        }

        private void UpdateDailyTraining(string username)
        {
            using var db = new LiteDatabase(_databasePath);
            var daily = db.GetCollection<DailyTraining>("daily");
            var result = daily.FindOne(d => d.Username == username);
            
            if (result == null)
            {
                result = new DailyTraining
                {
                    Username = username,
                    TotalSessionsToday = 1,
                    LastTrainingDate = DateTime.UtcNow
                };
                daily.Insert(result);
            }
            else
            {
                if (result.LastTrainingDate.Date != DateTime.UtcNow.Date)
                {
                    result.TotalSessionsToday = 0;
                    result.TotalExpToday = 0;
                }
                result.TotalSessionsToday++;
                result.LastTrainingDate = DateTime.UtcNow;
                daily.Update(result);
            }
        }

        public int GetRemainingSessions(string username)
        {
            var daily = GetDailyTraining(username);
            return Math.Max(0, MaxDailySessions - (daily?.TotalSessionsToday ?? 0));
        }

        public List<TrainingRecord> GetTrainingHistory(string username, int limit = 30)
        {
            using var db = new LiteDatabase(_databasePath);
            var records = db.GetCollection<TrainingRecord>("records");
            return records.Find(r => r.Username == username)
                .OrderByDescending(r => r.CompletedAtUtc)
                .Take(limit)
                .ToList();
        }
    }
}
