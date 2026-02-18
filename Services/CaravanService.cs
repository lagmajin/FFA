using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;

namespace FFA.Services
{
    // 商隊 / 交易ルートのスキャフォールディング
    public class CaravanService
    {
        private readonly string _databasePath;

        public CaravanService()
        {
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _databasePath = Path.Combine(appDataPath, "caravans.db");
        }

        public CaravanResult StartCaravan(string owner, string routeName, int durationHours, int expectedProfit)
        {
            try
            {
                using var db = new LiteDatabase(_databasePath);
                var col = db.GetCollection<Caravan>("caravans");
                var c = new Caravan
                {
                    Owner = owner,
                    RouteName = routeName,
                    Start = DateTime.UtcNow,
                    End = DateTime.UtcNow.AddHours(durationHours),
                    ExpectedProfit = expectedProfit,
                    Status = CaravanStatus.OnRoute
                };
                col.Insert(c);
                return new CaravanResult { Success = true, Message = "商隊を出発させました", CaravanId = c.Id };
            }
            catch (Exception ex)
            {
                return new CaravanResult { Success = false, Message = ex.Message };
            }
        }

        public List<Caravan> GetActiveCaravans()
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Caravan>("caravans");
            return col.Find(c => c.Status == CaravanStatus.OnRoute).ToList();
        }

        public CaravanResult CompleteCaravan(int caravanId)
        {
            try
            {
                using var db = new LiteDatabase(_databasePath);
                var col = db.GetCollection<Caravan>("caravans");
                var c = col.FindById(caravanId);
                if (c == null) return new CaravanResult { Success = false, Message = "商隊が見つかりません" };
                c.Status = CaravanStatus.Completed;
                col.Update(c);
                return new CaravanResult { Success = true, Message = "商隊が帰還しました", CaravanId = c.Id };
            }
            catch (Exception ex)
            {
                return new CaravanResult { Success = false, Message = ex.Message };
            }
        }
    }

    public class Caravan
    {
        public int Id { get; set; }
        public string Owner { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int ExpectedProfit { get; set; }
        public CaravanStatus Status { get; set; }
    }

    public enum CaravanStatus { OnRoute, Completed, Lost }

    public class CaravanResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int CaravanId { get; set; }
    }
}
