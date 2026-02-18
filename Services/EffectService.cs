using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using FFA.Models;

namespace FFA.Services
{
    // 状態異常 / バフの管理サービス
    public class EffectService
    {
        private readonly string _dbPath;

        public EffectService()
        {
            var appData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appData);
            _dbPath = Path.Combine(appData, "effects.db");
        }

        public ActiveEffect ApplyEffect(string ownerId, EffectType type, int durationSeconds, int strength = 1, string? source = null)
        {
            var effect = new ActiveEffect
            {
                OwnerId = ownerId,
                Type = type,
                RemainingSeconds = durationSeconds,
                Strength = strength,
                Source = source,
                AppliedAtUtc = DateTime.UtcNow
            };

            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<ActiveEffect>("effects");
            col.Insert(effect);
            return effect;
        }

        public List<ActiveEffect> GetEffects(string ownerId)
        {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<ActiveEffect>("effects");
            return col.Find(e => e.OwnerId == ownerId).ToList();
        }

        public void RemoveEffect(int effectId)
        {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<ActiveEffect>("effects");
            col.Delete(effectId);
        }

        public void TickAll(int seconds = 1)
        {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<ActiveEffect>("effects");
            var all = col.FindAll().ToList();
            foreach (var e in all)
            {
                e.RemainingSeconds -= seconds;
                if (e.RemainingSeconds <= 0)
                {
                    col.Delete(e.Id);
                }
                else
                {
                    col.Update(e);
                }
            }
        }
    }
}
