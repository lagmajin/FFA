using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using FFA.Models;

namespace FFA.Services
{
    // FF11風のNM（ノトーリアスモンスター）システムの基盤
    public class NotoriousMonsterService
    {
        private readonly string _dbPath;
        private readonly Random _rnd = new();

        public NotoriousMonsterService()
        {
            var appData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appData);
            _dbPath = Path.Combine(appData, "notorious.db");
        }

        public void SeedDefaults()
        {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<NotoriousMonster>("nms");
            if (col.Count() > 0) return;

            var nm = new NotoriousMonster
            {
                Name = "暴風のガルーダ",
                Location = "天空の峰",
                Status = NMStatus.Dormant,
                RespawnInterval = TimeSpan.FromHours(6),
                MaxHP = 5000,
                CurrentHP = 5000,
                Attack = 200,
                Defense = 100,
                RewardExp = 2000,
                RewardGil = 2000,
                DropItem = "ガルーダの羽",
                DropRate = 15
            };
            col.Insert(nm);
        }

        public List<NotoriousMonster> GetAll() {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<NotoriousMonster>("nms");
            return col.FindAll().ToList();
        }

        public NotoriousMonster? GetById(int id) {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<NotoriousMonster>("nms");
            return col.FindById(id);
        }

        // attempt spawn if Dormant and respawn interval passed
        public bool TrySpawn(int id) {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<NotoriousMonster>("nms");
            var nm = col.FindById(id);
            if (nm == null) return false;
            if (nm.Status != NMStatus.Dormant) return false;
            if (nm.LastKilledAtUtc.HasValue && DateTime.UtcNow - nm.LastKilledAtUtc.Value < nm.RespawnInterval) return false;

            nm.Status = NMStatus.Alive;
            nm.SpawnedAtUtc = DateTime.UtcNow;
            nm.CurrentHP = nm.MaxHP;
            col.Update(nm);
            return true;
        }

        public bool Damage(int id, string attacker, int damage)
        {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<NotoriousMonster>("nms");
            var nm = col.FindById(id);
            if (nm == null) return false;
            if (nm.Status != NMStatus.Alive) return false;

            nm.CurrentHP -= damage;
            if (nm.CurrentHP <= 0)
            {
                nm.Status = NMStatus.Dead;
                nm.LastKilledAtUtc = DateTime.UtcNow;
                nm.SpawnedAtUtc = null;
                nm.LastKilledBy = attacker;
                nm.CurrentHP = 0;
                col.Update(nm);
                return true; // slain
            }

            col.Update(nm);
            return false; // still alive
        }

        public void ResetAllExpired()
        {
            using var db = new LiteDatabase(_dbPath);
            var col = db.GetCollection<NotoriousMonster>("nms");
            var list = col.FindAll().ToList();
            foreach (var nm in list)
            {
                if (nm.Status == NMStatus.Dead && nm.LastKilledAtUtc.HasValue && DateTime.UtcNow - nm.LastKilledAtUtc.Value >= nm.RespawnInterval)
                {
                    nm.Status = NMStatus.Dormant;
                    col.Update(nm);
                }
            }
        }
    }
}
