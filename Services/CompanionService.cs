using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using FFA.Models;

namespace FFA.Services
{
    public class CompanionService
    {
        private readonly string _databasePath;

        public CompanionService()
        {
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _databasePath = Path.Combine(appDataPath, "companions.db");
        }

        public Companion CreateCompanion(string owner, string name, CompanionRarity rarity)
        {
            var comp = new Companion
            {
                OwnerUsername = owner,
                Name = name,
                Rarity = rarity,
                Level = 1,
                Experience = 0,
                Attack = 5,
                Defense = 3,
                HP = 200
            };

            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Companion>("companions");
            col.Insert(comp);
            // touch owner last active
            var us = new UserService();
            var u = us.GetByUsername(owner);
            if (u != null) { u.LastActiveUtc = DateTime.UtcNow; us.UpdateUser(u); }
            return comp;
        }

        public List<Companion> GetByOwner(string owner)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Companion>("companions");
            return col.Find(c => c.OwnerUsername == owner).ToList();
        }

        public Companion? GetById(int id)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Companion>("companions");
            return col.FindById(id);
        }

        public void UpdateCompanion(Companion comp)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Companion>("companions");
            col.Update(comp);
        }

        public void DeleteCompanion(int id)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Companion>("companions");
            col.Delete(id);
        }
    }
}
