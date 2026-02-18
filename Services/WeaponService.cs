using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using FFA.Models;

namespace FFA.Services;

public class WeaponService
{
    private readonly string _databasePath;

    public WeaponService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "weapons.db");
    }

    // 武器を強化する
    public EnhancementResult EnhanceWeapon(string username, int weaponId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var weapons = db.GetCollection<Weapon>("weapons");
            var weapon = weapons.FindById(weaponId);
            if (weapon == null)
                return new EnhancementResult { Success = false, Message = "武器が見つかりません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new EnhancementResult { Success = false, Message = "ユーザーが見つかりません" };

            // 強化条件を確認
            if (weapon.EnhancementLevel >= weapon.MaxEnhancementLevel)
                return new EnhancementResult { Success = false, Message = "これ以上強化できません" };

            if (user.Gil < weapon.EnhancementCost)
                return new EnhancementResult { Success = false, Message = "ギルが不足しています" };

            // 成功率を計算
            double successRate = weapon.EnhancementSuccessRate;
            if (weapon.IsEnhancementSafe)
                successRate = 1.0; // 安全強化は100%成功

            Random rand = new Random();
            bool isSuccess = rand.NextDouble() < successRate;

            if (isSuccess)
            {
                // 成功時の処理
                weapon.EnhancementLevel++;
                weapon.Attack += 2; // 攻撃力を2上昇
                user.Gil -= weapon.EnhancementCost;
                weapons.Update(weapon);
                userService.UpdateUser(user);

                return new EnhancementResult
                {
                    Success = true,
                    Message = $"強化成功！Lv.{weapon.EnhancementLevel}になりました",
                    NewLevel = weapon.EnhancementLevel,
                    NewAttack = weapon.Attack
                };
            }
            else
            {
                // 失敗時の処理
                if (weapon.EnhancementLevel > 0)
                {
                    weapon.EnhancementLevel--;
                    weapon.Attack -= 2; // 攻撃力を2低下
                }
                user.Gil -= weapon.EnhancementCost;
                weapons.Update(weapon);
                userService.UpdateUser(user);

                return new EnhancementResult
                {
                    Success = false,
                    Message = $"強化失敗...Lv.{weapon.EnhancementLevel}になりました",
                    NewLevel = weapon.EnhancementLevel,
                    NewAttack = weapon.Attack
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WeaponService.EnhanceWeapon 例外: {ex.Message} - {ex.StackTrace}");
            return new EnhancementResult { Success = false, Message = "強化中にエラーが発生しました" };
        }
    }

    // 武器を修理する
    public RepairResult RepairWeapon(string username, int weaponId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var weapons = db.GetCollection<Weapon>("weapons");
            var weapon = weapons.FindById(weaponId);
            if (weapon == null)
                return new RepairResult { Success = false, Message = "武器が見つかりません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new RepairResult { Success = false, Message = "ユーザーが見つかりません" };

            if (weapon.CurrentDurability == weapon.Durability)
                return new RepairResult { Success = false, Message = "武器はすでに完全に修理されています" };

            if (user.Gil < weapon.RepairCost)
                return new RepairResult { Success = false, Message = "ギルが不足しています" };

            // 修理処理
            int repairedAmount = Math.Min(weapon.RepairAmount, weapon.Durability - weapon.CurrentDurability);
            weapon.CurrentDurability += repairedAmount;
            user.Gil -= weapon.RepairCost;
            weapons.Update(weapon);
            userService.UpdateUser(user);

            return new RepairResult
            {
                Success = true,
                Message = $"修理完了！耐久度が{repairedAmount}回復しました",
                NewDurability = weapon.CurrentDurability
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WeaponService.RepairWeapon 例外: {ex.Message} - {ex.StackTrace}");
            return new RepairResult { Success = false, Message = "修理中にエラーが発生しました" };
        }
    }

    // 武器を分解する
    public DismantleResult DismantleWeapon(string username, int weaponId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var weapons = db.GetCollection<Weapon>("weapons");
            var weapon = weapons.FindById(weaponId);
            if (weapon == null)
                return new DismantleResult { Success = false, Message = "武器が見つかりません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new DismantleResult { Success = false, Message = "ユーザーが見つかりません" };

            // 分解処理
            foreach (var material in weapon.DismantleMaterials)
            {
                var item = new InventoryItem
                {
                    Name = material.Name,
                    Type = material.Type,
                    Quantity = material.Quantity,
                    Price = material.Price
                };
                userService.AddItemToUser(username, item);
            }

            user.Exp += weapon.DismantleExperience;
            weapons.Delete(weaponId);
            userService.UpdateUser(user);

            return new DismantleResult
            {
                Success = true,
                Message = "武器の分解に成功しました",
                Materials = weapon.DismantleMaterials,
                ExperienceGained = weapon.DismantleExperience
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WeaponService.DismantleWeapon 例外: {ex.Message} - {ex.StackTrace}");
            return new DismantleResult { Success = false, Message = "分解中にエラーが発生しました" };
        }
    }

    // 武器を合成する
    public SynthesisResult SynthesizeWeapons(string username, List<int> weaponIds)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var weapons = db.GetCollection<Weapon>("weapons");
            var userWeapons = weapons.Find(w => weaponIds.Contains(w.Id)).ToList();

            if (userWeapons.Count != weaponIds.Count)
                return new SynthesisResult { Success = false, Message = "すべての武器が見つかりません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new SynthesisResult { Success = false, Message = "ユーザーが見つかりません" };

            // 合成条件を確認
            var targetWeapon = userWeapons[0].RequiredWeaponsForSynthesis.FirstOrDefault();
            if (targetWeapon == null)
                return new SynthesisResult { Success = false, Message = "合成レシピが見つかりません" };

            if (!userWeapons.All(w => w.Type == targetWeapon.Type))
                return new SynthesisResult { Success = false, Message = "武器の種類が一致しません" };

            // 合成処理
            var newWeapon = new Weapon
            {
                Name = $"合成武器 ({targetWeapon.Name})",
                Attack = userWeapons.Sum(w => w.Attack) / 2,
                Type = targetWeapon.Type,
                Rarity = targetWeapon.Rarity,
                EnhancementLevel = 0,
                Durability = 100,
                CurrentDurability = 100,
                SpecialEffects = targetWeapon.SpecialEffects,
                SpecialEffectPower = targetWeapon.SpecialEffectPower,
                DismantleMaterials = targetWeapon.DismantleMaterials,
                DismantleExperience = targetWeapon.DismantleExperience,
                RequiredWeaponsForSynthesis = targetWeapon.RequiredWeaponsForSynthesis,
                SynthesisExperience = targetWeapon.SynthesisExperience,
                MaxEnhancementLevel = targetWeapon.MaxEnhancementLevel,
                EnhancementCost = targetWeapon.EnhancementCost,
                EnhancementSuccessRate = targetWeapon.EnhancementSuccessRate,
                IsEnhancementSafe = targetWeapon.IsEnhancementSafe,
                RepairCost = targetWeapon.RepairCost,
                RepairAmount = targetWeapon.RepairAmount
            };

            foreach (var weapon in userWeapons)
            {
                weapons.Delete(weapon.Id);
            }

            newWeapon.Id = weapons.Insert(newWeapon);
            user.Exp += targetWeapon.SynthesisExperience;
            userService.UpdateUser(user);

            return new SynthesisResult
            {
                Success = true,
                Message = "武器の合成に成功しました",
                NewWeapon = newWeapon,
                ExperienceGained = targetWeapon.SynthesisExperience
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WeaponService.SynthesizeWeapons 例外: {ex.Message} - {ex.StackTrace}");
            return new SynthesisResult { Success = false, Message = "合成中にエラーが発生しました" };
        }
    }

    // 武器一覧を取得
    public IEnumerable<Weapon> GetWeaponsByUser(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var weapons = db.GetCollection<Weapon>("weapons");
            return weapons.Find(w => w.OwnerUsername == username).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WeaponService.GetWeaponsByUser 例外: {ex.Message} - {ex.StackTrace}");
            return new List<Weapon>();
        }
    }

    // 武器をインベントリに追加
    public void AddWeaponToUser(string username, Weapon weapon)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var weapons = db.GetCollection<Weapon>("weapons");
            weapon.OwnerUsername = username;
            weapons.Insert(weapon);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WeaponService.AddWeaponToUser 例外: {ex.Message} - {ex.StackTrace}");
        }
    }
}

// 強化結果クラス（武器用）
public class WeaponEnhancementResult : EnhancementResult
{
    // CS0108 handled: explicitly hiding base NewAttack to indicate weapon-specific value
    public new int NewAttack { get; set; }
}

// 修理結果クラス（武器用）
public class WeaponRepairResult : RepairResult
{
    // CS0108 handled: explicitly hiding base NewDurability to indicate weapon-specific value
    public new int NewDurability { get; set; }
}

// 分解結果クラス
public class DismantleResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public List<Material> Materials { get; set; } = new();
    public int ExperienceGained { get; set; }
}

// 合成結果クラス
public class SynthesisResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public Weapon NewWeapon { get; set; } = new();
    public int ExperienceGained { get; set; }
}