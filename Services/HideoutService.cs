using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using FFA.Models;

namespace FFA.Services
{
    public class HideoutService
    {
        private readonly string _databasePath;

        public HideoutService()
        {
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _databasePath = Path.Combine(appDataPath, "hideouts.db");
        }

        public Hideout CreateHideout(string owner, string name, HideoutType type, string fieldId, int x, int y)
        {
            var hideout = new Hideout
            {
                OwnerUsername = owner,
                Name = name,
                Type = type,
                Level = 1,
                FieldId = fieldId,
                X = x,
                Y = y,
                MaxStorageSlots = GetBaseStorageSlots(type),
                CreatedUtc = DateTime.UtcNow,
                LastVisitedUtc = DateTime.UtcNow
            };

            // 初始化升级
            foreach (HideoutUpgradeType upgrade in Enum.GetValues<HideoutUpgradeType>())
            {
                hideout.Upgrades[upgrade] = 0;
            }

            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Hideout>("hideouts");
            col.Insert(hideout);
            
            return hideout;
        }

        private int GetBaseStorageSlots(HideoutType type) => type switch
        {
            HideoutType.Cabin => 20,
            HideoutType.House => 50,
            HideoutType.Mansion => 100,
            HideoutType.Castle => 200,
            HideoutType.Temple => 150,
            HideoutType.Tower => 120,
            _ => 20
        };

        public Hideout? GetByOwner(string owner)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Hideout>("hideouts");
            return col.FindOne(h => h.OwnerUsername == owner);
        }

        public Hideout? GetById(int id)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Hideout>("hideouts");
            return col.FindById(id);
        }

        public List<Hideout> GetHideoutsInField(string fieldId)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Hideout>("hideouts");
            return col.Find(h => h.FieldId == fieldId).ToList();
        }

        public void UpdateHideout(Hideout hideout)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Hideout>("hideouts");
            col.Update(hideout);
        }

        public bool UpgradeHideout(string owner, HideoutUpgradeType upgradeType)
        {
            var hideout = GetByOwner(owner);
            if (hideout == null) return false;

            int currentLevel = hideout.Upgrades.GetValueOrDefault(upgradeType, 0);
            int upgradeCost = CalculateUpgradeCost(upgradeType, currentLevel);
            
            // コストチェック（ギル）
            var userService = new UserService();
            var user = userService.GetByUsername(owner);
            if (user == null || user.Gil < upgradeCost) return false;

            // ギル支払い
            user.Gil -= upgradeCost;
            userService.UpdateUser(user);

            // アップグレード
            hideout.Upgrades[upgradeType] = currentLevel + 1;
            
            // 效果适
            ApplyUpgradeEffect(hideout, upgradeType, currentLevel + 1);
            
            UpdateHideout(hideout);
            return true;
        }

        private int CalculateUpgradeCost(HideoutUpgradeType type, int currentLevel)
        {
            int baseCost = type switch
            {
                HideoutUpgradeType.Storage => 500,
                HideoutUpgradeType.Garden => 300,
                HideoutUpgradeType.Workshop => 800,
                HideoutUpgradeType.TrainingRoom => 600,
                HideoutUpgradeType.Library => 1000,
                HideoutUpgradeType.Shrine => 1200,
                HideoutUpgradeType.Farm => 400,
                _ => 500
            };
            
            return baseCost * (int)Math.Pow(1.5, currentLevel);
        }

        private void ApplyUpgradeEffect(Hideout hideout, HideoutUpgradeType type, int newLevel)
        {
            switch (type)
            {
                case HideoutUpgradeType.Storage:
                    hideout.MaxStorageSlots += 10 * newLevel;
                    break;
                case HideoutUpgradeType.TrainingRoom:
                    // 後でバ fight bonus に応
                    break;
                case HideoutUpgradeType.Farm:
                    // 農場-slots 拡張
                    break;
            }
        }

        public void VisitHideout(int hideoutId, string visitorName)
        {
            var hideout = GetById(hideoutId);
            if (hideout == null) return;

            if (!hideout.Visitors.Contains(visitorName))
            {
                hideout.Visitors.Add(visitorName);
            }
            hideout.TotalVisits++;
            hideout.LastVisitedUtc = DateTime.UtcNow;
            UpdateHideout(hideout);
        }

        public bool PlantCrop(string owner, string cropId, string cropName, int growthTimeSeconds)
        {
            var hideout = GetByOwner(owner);
            if (hideout == null) return false;

            // 農場升级チェック
            int farmLevel = hideout.Upgrades.GetValueOrDefault(HideoutUpgradeType.Farm, 0);
            int maxCrops = 2 + farmLevel * 2;
            
            if (hideout.Crops.Count >= maxCrops) return false;

            var crop = new HideoutCrop
            {
                CropId = cropId,
                CropName = cropName,
                PlantTimeUtc = (int)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds,
                GrowthTimeSeconds = growthTimeSeconds
            };

            hideout.Crops.Add(crop);
            UpdateHideout(hideout);
            return true;
        }

        public List<HideoutCrop> HarvestCrops(string owner)
        {
            var hideout = GetByOwner(owner);
            if (hideout == null) return new List<HideoutCrop>();

            var readyCrops = hideout.Crops.Where(c => c.IsReady).ToList();
            hideout.Crops.RemoveAll(c => c.IsReady);
            UpdateHideout(hideout);

            return readyCrops;
        }

        public int GetCombatBonus(string owner)
        {
            var hideout = GetByOwner(owner);
            if (hideout == null) return 0;

            int trainingLevel = hideout.Upgrades.GetValueOrDefault(HideoutUpgradeType.TrainingRoom, 0);
            return trainingLevel * 5; // 各レベルごとに5% bonus
        }

        public int GetExpBonus(string owner)
        {
            var hideout = GetByOwner(owner);
            if (hideout == null) return 0;

            int shrineLevel = hideout.Upgrades.GetValueOrDefault(HideoutUpgradeType.Shrine, 0);
            return shrineLevel * 3; // 各レベルごとに3% bonus
        }

        public bool DeleteHideout(int id)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Hideout>("hideouts");
            var result = col.Delete(id);
            return result;
        }
    }
}
