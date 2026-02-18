using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using FFA.Models;

namespace FFA.Services
{
    public class MonsterService
    {
        private readonly string _databasePath;
        private readonly Random _rnd = new();

        public MonsterService()
        {
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _databasePath = Path.Combine(appDataPath, "monsters.db");
        }

        public void SeedDefaultTemplates()
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<MonsterTemplate>("templates");
            if (col.Count() > 0) return;

            var list = new List<MonsterTemplate>
            {
                new MonsterTemplate { Name = "スライム", BaseHP = 20, BaseAttack = 5, BaseDefense = 2, BaseExp = 10, BaseGil = 5, DropItem = "草薬", DropRate = 20 },
                new MonsterTemplate { Name = "ゴブリン", BaseHP = 35, BaseAttack = 8, BaseDefense = 3, BaseExp = 20, BaseGil = 10, DropItem = "短剣", DropRate = 15 },
                new MonsterTemplate { Name = "オーク", BaseHP = 50, BaseAttack = 12, BaseDefense = 5, BaseExp = 35, BaseGil = 20, DropItem = "盾牌", DropRate = 10 }
            };

            foreach (var t in list) col.Insert(t);
        }

        public IEnumerable<MonsterTemplate> GetTemplates()
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<MonsterTemplate>("templates");
            return col.FindAll().ToList();
        }

        public MonsterTemplate? GetTemplate(int id)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<MonsterTemplate>("templates");
            return col.FindById(id);
        }

        // spawn enemy scaled by player level
        public Enemy SpawnEnemyFromTemplate(MonsterTemplate template, int playerLevel)
        {
            var multiplier = 1 + (playerLevel - 1) * 0.1;
            return new Enemy
            {
                Name = template.Name,
                HP = (int)(template.BaseHP * multiplier),
                MaxHP = (int)(template.BaseHP * multiplier),
                Attack = (int)(template.BaseAttack * multiplier),
                Defense = (int)(template.BaseDefense * multiplier),
                Exp = (int)(template.BaseExp * multiplier),
                Gil = (int)(template.BaseGil * multiplier),
                DropItem = template.DropItem,
                DropRate = template.DropRate
            };
        }

        // spawn random enemy based on templates
        public Enemy SpawnRandomEnemy(int playerLevel)
        {
            var templates = GetTemplates().ToList();
            if (!templates.Any()) SeedDefaultTemplates();
            templates = GetTemplates().ToList();
            var t = templates[_rnd.Next(templates.Count)];
            return SpawnEnemyFromTemplate(t, playerLevel);
        }
    }
}
