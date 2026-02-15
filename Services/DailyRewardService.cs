using System;
using LiteDB;
using System.IO;
using FFA.Models;

namespace FFA.Services
{
    public class DailyRewardService
    {
        private readonly string _dbPath;
        private readonly object _lock = new();

        public DailyRewardService()
        {
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _dbPath = Path.Combine(appDataPath, "daily.db");
        }

        public UserDailyLog GetUserDailyLog(string username)
        {
            if (string.IsNullOrEmpty(username)) return new UserDailyLog { Username = username };
            
            lock (_lock)
            {
                try
                {
                    using var db = new LiteDatabase(_dbPath);
                    var col = db.GetCollection<UserDailyLog>("userDailyLogs");
                    var log = col.FindOne(x => x.Username == username);
                    
                    if (log == null)
                    {
                        log = new UserDailyLog
                        {
                            Username = username,
                            LastLoginDate = DateTime.UtcNow,
                            LastClaimDate = DateTime.MinValue,
                            ConsecutiveDays = 1,
                            TotalLoginDays = 1,
                            TotalClaims = 0,
                            WeeklyBonus = 0,
                            MonthlyBonus = 0
                        };
                        col.Insert(log);
                    }
                    else
                    {
                        CheckLoginUpdate(log);
                        col.Update(log);
                    }
                    
                    return log;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DailyRewardService.GetUserDailyLog 例外: {ex.Message} - {ex.StackTrace}");
                    return new UserDailyLog { Username = username };
                }
            }
        }

        public bool HasClaimed(string username)
        {
            var log = GetUserDailyLog(username);
            return log.LastClaimDate.Date == DateTime.UtcNow.Date;
        }

        public bool Claim(string username, out int gilReward, out RewardType rewardType)
        {
            gilReward = 0;
            rewardType = RewardType.Gil;
            
            if (string.IsNullOrEmpty(username)) return false;
            
            lock (_lock)
            {
                try
                {
                    using var db = new LiteDatabase(_dbPath);
                    var col = db.GetCollection<UserDailyLog>("userDailyLogs");
                    var log = col.FindOne(x => x.Username == username);
                    
                    if (log == null) log = GetUserDailyLog(username);
                    
                    if (log.LastClaimDate.Date == DateTime.UtcNow.Date) return false;
                    
                    var reward = CalculateDailyReward(log);
                    gilReward = reward;
                    
                    log.LastClaimDate = DateTime.UtcNow;
                    log.TotalClaims++;
                    log.WeeklyBonus++;
                    log.MonthlyBonus++;
                    
                    col.Update(log);
                    
                    AddActivityLog(username, "DailyClaim", "デイリー報酬受取", 100);
                    
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DailyRewardService.Claim 例外: {ex.Message} - {ex.StackTrace}");
                    return false;
                }
            }
        }

        public void RecordLogin(string username)
        {
            if (string.IsNullOrEmpty(username)) return;
            
            lock (_lock)
            {
                try
                {
                    using var db = new LiteDatabase(_dbPath);
                    var col = db.GetCollection<UserDailyLog>("userDailyLogs");
                    var log = col.FindOne(x => x.Username == username);
                    
                    if (log == null)
                    {
                        log = new UserDailyLog
                        {
                            Username = username,
                            LastLoginDate = DateTime.UtcNow,
                            LastClaimDate = DateTime.MinValue,
                            ConsecutiveDays = 1,
                            TotalLoginDays = 1,
                            TotalClaims = 0,
                            WeeklyBonus = 0,
                            MonthlyBonus = 0
                        };
                        col.Insert(log);
                    }
                    else
                    {
                        CheckLoginUpdate(log);
                        col.Update(log);
                    }
                    
                    AddActivityLog(username, "Login", "ログイン", 50);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DailyRewardService.RecordLogin 例外: {ex.Message} - {ex.StackTrace}");
                }
            }
        }

        public List<DailyRewardDefinition> GetDailyRewards()
        {
            var rewards = new List<DailyRewardDefinition>
            {
                new() { DayNumber = 1, Type = RewardType.Gil, Amount = 100, Description = "100ギル", Icon = "💰" },
                new() { DayNumber = 2, Type = RewardType.Gil, Amount = 150, Description = "150ギル", Icon = "💰" },
                new() { DayNumber = 3, Type = RewardType.OldCoin, Amount = 2, Description = "旧貨幣2個", Icon = "🏺" },
                new() { DayNumber = 4, Type = RewardType.Gil, Amount = 200, Description = "200ギル", Icon = "💰" },
                new() { DayNumber = 5, Type = RewardType.Exp, Amount = 1000, Description = "経験値1000", Icon = "📈" },
                new() { DayNumber = 6, Type = RewardType.Gil, Amount = 250, Description = "250ギル", Icon = "💰" },
                new() { DayNumber = 7, Type = RewardType.Premium, Amount = 5, Description = "プレミアム5", Icon = "💎", IsSpecial = true },
                new() { DayNumber = 8, Type = RewardType.Gil, Amount = 300, Description = "300ギル", Icon = "💰" },
                new() { DayNumber = 9, Type = RewardType.OldCoin, Amount = 3, Description = "旧貨幣3個", Icon = "🏺" },
                new() { DayNumber = 10, Type = RewardType.Exp, Amount = 1500, Description = "経験値1500", Icon = "📈" },
                new() { DayNumber = 11, Type = RewardType.Gil, Amount = 350, Description = "350ギル", Icon = "💰" },
                new() { DayNumber = 12, Type = RewardType.Premium, Amount = 3, Description = "プレミアム3", Icon = "💎" },
                new() { DayNumber = 13, Type = RewardType.Gil, Amount = 400, Description = "400ギル", Icon = "💰" },
                new() { DayNumber = 14, Type = RewardType.Premium, Amount = 10, Description = "プレミアム10", Icon = "💎", IsSpecial = true }
            };
            return rewards;
        }

        public List<LoginStreakReward> GetStreakRewards()
        {
            return new List<LoginStreakReward>
            {
                new() { DaysRequired = 7, Type = RewardType.Premium, Amount = 15, Description = "プレミアム15 + 伝説のチケット", Icon = "⭐" },
                new() { DaysRequired = 14, Type = RewardType.Premium, Amount = 30, Description = "プレミアム30 + 神話のチケット", Icon = "⭐" },
                new() { DaysRequired = 21, Type = RewardType.Premium, Amount = 50, Description = "プレミアム50 + レジェンドのチケット", Icon = "⭐" },
                new() { DaysRequired = 30, Type = RewardType.Premium, Amount = 100, Description = "プレミアム100 + ユニークのチケット", Icon = "⭐" }
            };
        }

        public List<DailyActivityLog> GetActivityLogs(string username, int days = 7)
        {
            if (string.IsNullOrEmpty(username)) return new List<DailyActivityLog>();
            
            lock (_lock)
            {
                try
                {
                    using var db = new LiteDatabase(_dbPath);
                    var col = db.GetCollection<DailyActivityLog>("activityLogs");
                    var startDate = DateTime.UtcNow.Date.AddDays(-days);
                    return col.Find(x => x.Username == username && x.Date >= startDate).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DailyRewardService.GetActivityLogs 例外: {ex.Message} - {ex.StackTrace}");
                    return new List<DailyActivityLog>();
                }
            }
        }

        private void CheckLoginUpdate(UserDailyLog log)
        {
            var today = DateTime.UtcNow.Date;
            var lastLogin = log.LastLoginDate.Date;
            
            if (lastLogin != today)
            {
                if (lastLogin == today.AddDays(-1))
                {
                    log.ConsecutiveDays++;
                }
                else if (lastLogin < today.AddDays(-1))
                {
                    log.ConsecutiveDays = 1;
                }
                
                log.LastLoginDate = DateTime.UtcNow;
                log.TotalLoginDays++;
            }
        }

        private int CalculateDailyReward(UserDailyLog log)
        {
            var baseReward = new Random().Next(50, 201);
            var streakBonus = log.ConsecutiveDays * 2;
            var weeklyBonus = log.WeeklyBonus >= 7 ? 50 : 0;
            var monthlyBonus = log.MonthlyBonus >= 30 ? 100 : 0;
            
            return baseReward + streakBonus + weeklyBonus + monthlyBonus;
        }

        private void AddActivityLog(string username, string activityType, string description, int points)
        {
            try
            {
                using var db = new LiteDatabase(_dbPath);
                var col = db.GetCollection<DailyActivityLog>("activityLogs");
                col.Insert(new DailyActivityLog
                {
                    Username = username,
                    Date = DateTime.UtcNow.Date,
                    ActivityType = activityType,
                    Description = description,
                    PointsEarned = points
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DailyRewardService.AddActivityLog 例外: {ex.Message} - {ex.StackTrace}");
            }
        }
    }
}
