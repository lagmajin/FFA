using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;

namespace FFA.Services
{
    // シンプルな探索 / 発見サービスのスキャフォールディング
    public class ExplorationService
    {
        private readonly string _databasePath;

        public ExplorationService()
        {
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _databasePath = Path.Combine(appDataPath, "exploration.db");
        }

        // ユーザーの発見を記録
        public ExplorationResult DiscoverLocation(string username, int locationId, string locationName)
        {
            try
            {
                using var db = new LiteDatabase(_databasePath);
                var col = db.GetCollection<Discovery>("discoveries");
                var existing = col.FindOne(d => d.Username == username && d.LocationId == locationId);
                if (existing != null)
                {
                    return new ExplorationResult { Success = false, Message = "既に発見済みです" };
                }

                var discovery = new Discovery
                {
                    Username = username,
                    LocationId = locationId,
                    LocationName = locationName,
                    DiscoveredAt = DateTime.UtcNow
                };
                col.Insert(discovery);
                return new ExplorationResult { Success = true, Message = "新しい場所を発見しました！", Discovery = discovery };
            }
            catch (Exception ex)
            {
                return new ExplorationResult { Success = false, Message = ex.Message };
            }
        }

        public List<Discovery> GetDiscoveries(string username)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Discovery>("discoveries");
            return col.Find(d => d.Username == username).ToList();
        }
    }

    public class Discovery
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public DateTime DiscoveredAt { get; set; }
    }

    public class ExplorationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Discovery? Discovery { get; set; }
    }
}
