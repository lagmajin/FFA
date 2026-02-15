using LiteDB;
using System.IO;

using Microsoft.AspNetCore.SignalR;
using FFA.Hubs;

namespace FFA.Services
{
    public class KarmaService
    {
        private readonly string _databasePath;
        private readonly object _lock = new();
        private readonly IHubContext<WorldHub>? _hub;

        public KarmaService()
        {
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _databasePath = Path.Combine(appDataPath, "karma.db");
        }

        // Optional constructor injection for notifications
        public KarmaService(IHubContext<WorldHub> hub) : this()
        {
            _hub = hub;
        }

        private class KarmaEntry
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public int Value { get; set; }
        }

        public int GetKarma(string username)
        {
            if (string.IsNullOrEmpty(username)) return 0;
            lock (_lock)
            {
                using var db = new LiteDatabase(_databasePath);
                var col = db.GetCollection<KarmaEntry>("karma");
                var ent = col.FindOne(x => x.Username == username);
                return ent?.Value ?? 0;
            }
        }

        public void SetKarma(string username, int value)
        {
            if (string.IsNullOrEmpty(username)) return;
            lock (_lock)
            {
                using var db = new LiteDatabase(_databasePath);
                var col = db.GetCollection<KarmaEntry>("karma");
                var ent = col.FindOne(x => x.Username == username);
                if (ent == null)
                {
                    ent = new KarmaEntry { Username = username, Value = value };
                    col.Insert(ent);
                }
                else
                {
                    ent.Value = value;
                    col.Update(ent);
                }
            }
        }

        public int AdjustKarma(string username, int delta)
        {
            if (string.IsNullOrEmpty(username)) return 0;
            lock (_lock)
            {
                using var db = new LiteDatabase(_databasePath);
                var col = db.GetCollection<KarmaEntry>("karma");
                var ent = col.FindOne(x => x.Username == username);
                if (ent == null)
                {
                    ent = new KarmaEntry { Username = username, Value = delta };
                    col.Insert(ent);
                    NotifyKarma(username, ent.Value);
                    return ent.Value;
                }
                ent.Value += delta;
                col.Update(ent);
                NotifyKarma(username, ent.Value);
                return ent.Value;
            }
        }

        private void NotifyKarma(string username, int value)
        {
            if (_hub == null) return;
            _ = _hub.Clients.All.SendAsync("KarmaUpdated", new { username = username, value = value });
        }
    }
}
