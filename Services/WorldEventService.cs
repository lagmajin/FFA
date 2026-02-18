using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;

namespace FFA.Services
{
    // 世界イベントスケジューラのスキャフォールディング
    public class WorldEventService
    {
        private readonly string _databasePath;

        public WorldEventService()
        {
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _databasePath = Path.Combine(appDataPath, "worldevents.db");
        }

        public EventResult ScheduleEvent(string name, DateTime start, DateTime end, string description)
        {
            try
            {
                using var db = new LiteDatabase(_databasePath);
                var col = db.GetCollection<WorldEvent>("events");
                var ev = new WorldEvent
                {
                    Name = name,
                    Start = start,
                    End = end,
                    Description = description,
                    Status = EventStatus.Scheduled
                };
                col.Insert(ev);
                return new EventResult { Success = true, Message = "イベントをスケジュールしました", EventId = ev.Id };
            }
            catch (Exception ex)
            {
                return new EventResult { Success = false, Message = ex.Message };
            }
        }

        public List<WorldEvent> GetActiveEvents()
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<WorldEvent>("events");
            var now = DateTime.UtcNow;
            return col.Find(e => e.Status == EventStatus.Ongoing || (e.Status == EventStatus.Scheduled && e.Start <= now && e.End >= now)).ToList();
        }

        public EventResult UpdateEventStatus(int eventId, EventStatus status)
        {
            try
            {
                using var db = new LiteDatabase(_databasePath);
                var col = db.GetCollection<WorldEvent>("events");
                var ev = col.FindById(eventId);
                if (ev == null) return new EventResult { Success = false, Message = "イベントが見つかりません" };
                ev.Status = status;
                col.Update(ev);
                return new EventResult { Success = true, Message = "更新しました" };
            }
            catch (Exception ex)
            {
                return new EventResult { Success = false, Message = ex.Message };
            }
        }
    }

    public class WorldEvent
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Description { get; set; } = string.Empty;
        public EventStatus Status { get; set; }
    }

    public enum EventStatus { Scheduled, Ongoing, Completed, Cancelled }

    public class EventResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int EventId { get; set; }
    }
}

