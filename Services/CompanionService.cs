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
                Attack = GetBaseAttack(rarity),
                Defense = GetBaseDefense(rarity),
                HP = GetBaseHP(rarity)
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

        private int GetBaseAttack(CompanionRarity rarity) => rarity switch
        {
            CompanionRarity.Common => 3,
            CompanionRarity.Uncommon => 5,
            CompanionRarity.Rare => 8,
            CompanionRarity.Epic => 12,
            CompanionRarity.Legendary => 18,
            _ => 3
        };

        private int GetBaseDefense(CompanionRarity rarity) => rarity switch
        {
            CompanionRarity.Common => 2,
            CompanionRarity.Uncommon => 4,
            CompanionRarity.Rare => 7,
            CompanionRarity.Epic => 10,
            CompanionRarity.Legendary => 15,
            _ => 2
        };

        private int GetBaseHP(CompanionRarity rarity) => rarity switch
        {
            CompanionRarity.Common => 30,
            CompanionRarity.Uncommon => 50,
            CompanionRarity.Rare => 80,
            CompanionRarity.Epic => 120,
            CompanionRarity.Legendary => 200,
            _ => 30
        };

        public void AddExperience(Companion companion, int exp)
        {
            // Apply rarity bonus
            double bonus = companion.Rarity switch
            {
                CompanionRarity.Common => 1.0,
                CompanionRarity.Uncommon => 1.1,
                CompanionRarity.Rare => 1.2,
                CompanionRarity.Epic => 1.35,
                CompanionRarity.Legendary => 1.5,
                _ => 1.0
            };
            
            int adjustedExp = (int)(exp * bonus);
            companion.Experience += adjustedExp;
            
            // Level up check (exp curve: 50 * level^1.5)
            int expToNext = (int)(50 * Math.Pow(companion.Level, 1.5));
            while (companion.Experience >= expToNext && companion.Level < 100)
            {
                companion.Experience -= expToNext;
                companion.Level++;
                
                // Stats growth
                double growthRate = companion.Rarity switch
                {
                    CompanionRarity.Common => 1.08,
                    CompanionRarity.Uncommon => 1.12,
                    CompanionRarity.Rare => 1.15,
                    CompanionRarity.Epic => 1.18,
                    CompanionRarity.Legendary => 1.22,
                    _ => 1.1
                };
                
                companion.Attack = (int)(companion.Attack * growthRate);
                companion.Defense = (int)(companion.Defense * growthRate);
                companion.HP = (int)(companion.HP * growthRate);
                
                expToNext = (int)(50 * Math.Pow(companion.Level, 1.5));
                
                Console.WriteLine($"[Companion] {companion.Name} leveled up to Lv{companion.Level}! ATK:{companion.Attack} DEF:{companion.Defense} HP:{companion.HP}");
            }
            
            UpdateCompanion(companion);
        }

        public List<Companion> GetByOwner(string owner)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Companion>("companions");
            return col.Find(c => c.OwnerUsername == owner).ToList();
        }

        public List<Companion> GetSummonedCompanions(string owner)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Companion>("companions");
            return col.Find(c => c.OwnerUsername == owner && c.IsSummoned).ToList();
        }

        public void SummonCompanion(int companionId)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Companion>("companions");
            var companion = col.FindById(companionId);
            if (companion != null)
            {
                companion.IsSummoned = !companion.IsSummoned; // Toggle
                col.Update(companion);
            }
        }

        public void DismissCompanion(int companionId)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Companion>("companions");
            var companion = col.FindById(companionId);
            if (companion != null)
            {
                companion.IsSummoned = false;
                col.Update(companion);
            }
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

        public int GetTotalBonus(string owner, string bonusType)
        {
            var summoned = GetSummonedCompanions(owner);
            int total = 0;
            
            foreach (var comp in summoned)
            {
                switch (bonusType)
                {
                    case "attack":
                        total += comp.Attack;
                        break;
                    case "defense":
                        total += comp.Defense;
                        break;
                    case "hp":
                        total += comp.HP;
                        break;
                    case "exp":
                        total += comp.Rarity switch
                        {
                            CompanionRarity.Common => 0,
                            CompanionRarity.Uncommon => 10,
                            CompanionRarity.Rare => 20,
                            CompanionRarity.Epic => 35,
                            CompanionRarity.Legendary => 50,
                            _ => 0
                        };
                        break;
                    case "drop":
                        total += comp.Rarity switch
                        {
                            CompanionRarity.Common => 0,
                            CompanionRarity.Uncommon => 5,
                            CompanionRarity.Rare => 10,
                            CompanionRarity.Epic => 20,
                            CompanionRarity.Legendary => 30,
                            _ => 0
                        };
                        break;
                }
            }
            return total;
        }
    }
}
