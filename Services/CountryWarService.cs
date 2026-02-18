using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;

namespace FFA.Services
{
    public class CountryWarService
    {
        private readonly string _databasePath;

        public CountryWarService()
        {
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _databasePath = Path.Combine(appDataPath, "countrywars.db");
        }

        public WarResult CreateWar(string name, DateTime start, DateTime end, List<string> participatingCountries)
        {
            try
            {
                using var db = new LiteDatabase(_databasePath);
                var col = db.GetCollection<CountryWar>("wars");
                var war = new CountryWar
                {
                    Name = name,
                    Start = start,
                    End = end,
                    Status = WarStatus.Scheduled,
                    CountryScores = participatingCountries.ToDictionary(c => c, c => 0)
                };
                col.Insert(war);
                return new WarResult { Success = true, Message = "War scheduled", WarId = war.Id };
            }
            catch (Exception ex)
            {
                return new WarResult { Success = false, Message = ex.Message };
            }
        }

        public List<CountryWar> GetActiveWars()
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<CountryWar>("wars");
            var now = DateTime.UtcNow;
            return col.Find(w => w.Status == WarStatus.Ongoing || (w.Status == WarStatus.Scheduled && w.Start <= now && w.End >= now)).ToList();
        }

        public WarResult AddScore(int warId, string countryName, int delta)
        {
            try
            {
                using var db = new LiteDatabase(_databasePath);
                var col = db.GetCollection<CountryWar>("wars");
                var war = col.FindById(warId);
                if (war == null) return new WarResult { Success = false, Message = "War not found" };
                if (!war.CountryScores.ContainsKey(countryName)) return new WarResult { Success = false, Message = "Country not participating" };
                war.CountryScores[countryName] += delta;
                col.Update(war);
                return new WarResult { Success = true, Message = "Score added" };
            }
            catch (Exception ex)
            {
                return new WarResult { Success = false, Message = ex.Message };
            }
        }

        public void CloseExpiredWars()
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<CountryWar>("wars");
            var now = DateTime.UtcNow;
            var wars = col.FindAll().ToList();
            foreach (var w in wars)
            {
                if (w.Status == WarStatus.Scheduled && w.End <= now)
                {
                    w.Status = WarStatus.Completed;
                    col.Update(w);
                }
            }
        }
    }

    public class CountryWar
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public WarStatus Status { get; set; }
        public Dictionary<string, int> CountryScores { get; set; } = new();
    }

    public enum WarStatus { Scheduled, Ongoing, Completed, Cancelled }

    public class WarResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int WarId { get; set; }
    }
}
