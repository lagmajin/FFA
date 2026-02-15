using System;
using LiteDB;
using System.IO;

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

        public bool HasClaimed(string username)
        {
            if (string.IsNullOrEmpty(username)) return false;
            lock (_lock)
            {
                using var db = new LiteDatabase(_dbPath);
                var col = db.GetCollection<UserClaim>("claims");
                var ent = col.FindOne(x => x.Username == username);
                if (ent == null) return false;
                return ent.LastClaimDate.Date == DateTime.UtcNow.Date;
            }
        }

        public bool Claim(string username, out int gilReward)
        {
            gilReward = 0;
            if (string.IsNullOrEmpty(username)) return false;
            lock (_lock)
            {
                using var db = new LiteDatabase(_dbPath);
                var col = db.GetCollection<UserClaim>("claims");
                var ent = col.FindOne(x => x.Username == username);
                if (ent != null && ent.LastClaimDate.Date == DateTime.UtcNow.Date) return false;
                var reward = new Random().Next(50, 201); // 50-200 gil
                gilReward = reward;
                if (ent == null)
                {
                    col.Insert(new UserClaim { Username = username, LastClaimDate = DateTime.UtcNow });
                }
                else
                {
                    ent.LastClaimDate = DateTime.UtcNow;
                    col.Update(ent);
                }
                return true;
            }
        }

        private class UserClaim
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public DateTime LastClaimDate { get; set; }
        }
    }
}
