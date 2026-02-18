using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using FFA.Models;

namespace FFA.Services;

public class ArmorService
{
    private readonly string _databasePath;

    public ArmorService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "armors.db");
    }

    /// <summary>
    /// 装備による総防御力を取得
    /// </summary>
    public int GetTotalDefense(string username)
    {
        try
        {
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user?.EquippedArmor == null) return 0;
            
            int defense = user.EquippedArmor.Defense;
            defense += user.EquippedArmor.EnhancementLevel * 2; // 強化レベルによる加成
            
            // レアリティによる加成
            defense = (int)(defense * SystemIntegration.GetRarityMultiplier(user.EquippedArmor.Rarity));
            
            return defense;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 装備による総攻撃力を取得
    /// </summary>
    public int GetTotalAttack(string username)
    {
        try
        {
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user?.EquippedWeapon == null) return 0;
            
            int attack = user.EquippedWeapon.Attack;
            attack += user.EquippedWeapon.EnhancementLevel * 2;
            
            // レアリティによる加成
            attack = (int)(attack * SystemIntegration.GetRarityMultiplier(user.EquippedWeapon.Rarity));
            
            return attack;
        }
        catch
        {
            return 0;
        }
    }

    // 防具を強化する
    public EnhancementResult EnhanceArmor(string username, int armorId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var armors = db.GetCollection<Armor>("armors");
            var armor = armors.FindById(armorId);
            if (armor == null)
                return new ArmorEnhancementResult { Success = false, Message = "防具が見つかりません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new ArmorEnhancementResult { Success = false, Message = "ユーザーが見つかりません" };

            // 強化条件を確認
            if (armor.EnhancementLevel >= armor.MaxEnhancementLevel)
                return new ArmorEnhancementResult { Success = false, Message = "これ以上強化できません" };

            if (user.Gil < armor.EnhancementCost)
                return new ArmorEnhancementResult { Success = false, Message = "ギルが不足しています" };

            // 成功率を計算
            double successRate = armor.EnhancementSuccessRate;
            if (armor.IsEnhancementSafe)
                successRate = 1.0; // 安全強化は100%成功

            Random rand = new Random();
            bool isSuccess = rand.NextDouble() < successRate;

            if (isSuccess)
            {
                // 成功時の処理
                armor.EnhancementLevel++;
                armor.Defense += 2; // 防御力を2上昇
                user.Gil -= armor.EnhancementCost;
                armors.Update(armor);
                userService.UpdateUser(user);

                return new ArmorEnhancementResult
                {
                    Success = true,
                    Message = $"強化成功！Lv.{armor.EnhancementLevel}になりました",
                    NewLevel = armor.EnhancementLevel,
                    NewDefense = armor.Defense
                };
            }
            else
            {
                // 失敗時の処理
                if (armor.EnhancementLevel > 0)
                {
                    armor.EnhancementLevel--;
                    armor.Defense -= 2; // 防御力を2低下
                }
                user.Gil -= armor.EnhancementCost;
                armors.Update(armor);
                userService.UpdateUser(user);

                return new ArmorEnhancementResult
                {
                    Success = false,
                    Message = $"強化失敗...Lv.{armor.EnhancementLevel}になりました",
                    NewLevel = armor.EnhancementLevel,
                    NewDefense = armor.Defense
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ArmorService.EnhanceArmor 例外: {ex.Message} - {ex.StackTrace}");
            return new ArmorEnhancementResult { Success = false, Message = "強化中にエラーが発生しました" };
        }
    }

    // 防具を修理する
    public ArmorRepairResult RepairArmor(string username, int armorId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var armors = db.GetCollection<Armor>("armors");
            var armor = armors.FindById(armorId);
            if (armor == null)
                return new ArmorRepairResult { Success = false, Message = "防具が見つかりません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new ArmorRepairResult { Success = false, Message = "ユーザーが見つかりません" };

            if (armor.CurrentDurability == armor.Durability)
                return new ArmorRepairResult { Success = false, Message = "防具はすでに完全に修理されています" };

            if (user.Gil < armor.RepairCost)
                return new ArmorRepairResult { Success = false, Message = "ギルが不足しています" };

            // 修理処理
            int repairedAmount = Math.Min(armor.RepairAmount, armor.Durability - armor.CurrentDurability);
            armor.CurrentDurability += repairedAmount;
            user.Gil -= armor.RepairCost;
            armors.Update(armor);
            userService.UpdateUser(user);

            return new ArmorRepairResult
            {
                Success = true,
                Message = $"修理完了！耐久度が{repairedAmount}回復しました",
                NewDurability = armor.CurrentDurability
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ArmorService.RepairArmor 例外: {ex.Message} - {ex.StackTrace}");
            return new ArmorRepairResult { Success = false, Message = "修理中にエラーが発生しました" };
        }
    }

    // 防具を分解する
    public ArmorDismantleResult DismantleArmor(string username, int armorId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var armors = db.GetCollection<Armor>("armors");
            var armor = armors.FindById(armorId);
            if (armor == null)
                return new ArmorDismantleResult { Success = false, Message = "防具が見つかりません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new ArmorDismantleResult { Success = false, Message = "ユーザーが見つかりません" };

            // 分解処理
            foreach (var material in armor.DismantleMaterials)
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

            user.Exp += armor.DismantleExperience;
            armors.Delete(armorId);
            userService.UpdateUser(user);

            return new ArmorDismantleResult
            {
                Success = true,
                Message = "防具の分解に成功しました",
                Materials = armor.DismantleMaterials,
                ExperienceGained = armor.DismantleExperience
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ArmorService.DismantleArmor 例外: {ex.Message} - {ex.StackTrace}");
            return new ArmorDismantleResult { Success = false, Message = "分解中にエラーが発生しました" };
        }
    }

    // 防具を合成する
    public ArmorSynthesisResult SynthesizeArmors(string username, List<int> armorIds)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var armors = db.GetCollection<Armor>("armors");
            var userArmors = armors.Find(a => armorIds.Contains(a.Id)).ToList();

            if (userArmors.Count != armorIds.Count)
                return new ArmorSynthesisResult { Success = false, Message = "すべての防具が見つかりません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new ArmorSynthesisResult { Success = false, Message = "ユーザーが見つかりません" };

            // 合成条件を確認
            var targetArmor = userArmors[0].RequiredArmorsForSynthesis.FirstOrDefault();
            if (targetArmor == null)
                return new ArmorSynthesisResult { Success = false, Message = "合成レシピが見つかりません" };

            if (!userArmors.All(a => a.Type == targetArmor.Type))
                return new ArmorSynthesisResult { Success = false, Message = "防具の種類が一致しません" };

            // 合成処理
            var newArmor = new Armor
            {
                Name = $"合成防具 ({targetArmor.Name})",
                Defense = userArmors.Sum(a => a.Defense) / 2,
                Type = targetArmor.Type,
                Rarity = targetArmor.Rarity,
                EnhancementLevel = 0,
                Durability = 100,
                CurrentDurability = 100,
                SpecialEffects = targetArmor.SpecialEffects,
                SpecialEffectPower = targetArmor.SpecialEffectPower,
                DismantleMaterials = targetArmor.DismantleMaterials,
                DismantleExperience = targetArmor.DismantleExperience,
                RequiredArmorsForSynthesis = targetArmor.RequiredArmorsForSynthesis,
                SynthesisExperience = targetArmor.SynthesisExperience,
                MaxEnhancementLevel = targetArmor.MaxEnhancementLevel,
                EnhancementCost = targetArmor.EnhancementCost,
                EnhancementSuccessRate = targetArmor.EnhancementSuccessRate,
                IsEnhancementSafe = targetArmor.IsEnhancementSafe,
                RepairCost = targetArmor.RepairCost,
                RepairAmount = targetArmor.RepairAmount
            };

            foreach (var armor in userArmors)
            {
                armors.Delete(armor.Id);
            }

            newArmor.Id = armors.Insert(newArmor);
            user.Exp += targetArmor.SynthesisExperience;
            userService.UpdateUser(user);

            return new ArmorSynthesisResult
            {
                Success = true,
                Message = "防具の合成に成功しました",
                NewArmor = newArmor,
                ExperienceGained = targetArmor.SynthesisExperience
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ArmorService.SynthesizeArmors 例外: {ex.Message} - {ex.StackTrace}");
            return new ArmorSynthesisResult { Success = false, Message = "合成中にエラーが発生しました" };
        }
    }

    // 防具一覧を取得
    public IEnumerable<Armor> GetArmorsByUser(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var armors = db.GetCollection<Armor>("armors");
            return armors.Find(a => a.OwnerUsername == username).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ArmorService.GetArmorsByUser 例外: {ex.Message} - {ex.StackTrace}");
            return new List<Armor>();
        }
    }

    // 防具をインベントリに追加
    public void AddArmorToUser(string username, Armor armor)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var armors = db.GetCollection<Armor>("armors");
            armor.OwnerUsername = username;
            armors.Insert(armor);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ArmorService.AddArmorToUser 例外: {ex.Message} - {ex.StackTrace}");
        }
    }

    // 防具を装備する
    public EquipResult EquipArmor(string username, int armorId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var armors = db.GetCollection<Armor>("armors");
            var armor = armors.FindById(armorId);
            if (armor == null)
                return new EquipResult { Success = false, Message = "防具が見つかりません" };

            if (armor.OwnerUsername != username)
                return new EquipResult { Success = false, Message = "この防具を装備することはできません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new EquipResult { Success = false, Message = "ユーザーが見つかりません" };

            // 装備条件を確認
            if (user.Status.Str < armor.RequiredStr)
                return new EquipResult { Success = false, Message = "必要STRを満たしていません" };
            if (user.Status.Dex < armor.RequiredDex)
                return new EquipResult { Success = false, Message = "必要DEXを満たしていません" };
            if (user.Status.Int < armor.RequiredInt)
                return new EquipResult { Success = false, Message = "必要INTを満たしていません" };
            if (user.Status.Vit < armor.RequiredVit)
                return new EquipResult { Success = false, Message = "必要VITを満たしていません" };
            if (user.Status.Agi < armor.RequiredAgi)
                return new EquipResult { Success = false, Message = "必要AGIを満たしていません" };
            if (user.Status.Luk < armor.RequiredLuk)
                return new EquipResult { Success = false, Message = "必要LUKを満たしていません" };

            // 現在の装備をインベントリに戻す
            if (user.EquippedArmor != null)
            {
                user.EquippedArmor.OwnerUsername = username;
                armors.Insert(user.EquippedArmor);
            }

            // 新しい防具を装备
            user.EquippedArmor = armor;
            armors.Delete(armorId);
            userService.UpdateUser(user);

            return new EquipResult
            {
                Success = true,
                Message = $"{armor.Name}を装備しました",
                EquippedArmor = armor
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ArmorService.EquipArmor 例外: {ex.Message} - {ex.StackTrace}");
            return new EquipResult { Success = false, Message = "装備中にエラーが発生しました" };
        }
    }

    // 防具を装備解除する
    public EquipResult UnequipArmor(string username)
    {
        try
        {
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new EquipResult { Success = false, Message = "ユーザーが見つかりません" };

            if (user.EquippedArmor == null)
                return new EquipResult { Success = false, Message = "装備している防具がありません" };

            using var db = new LiteDatabase(_databasePath);
            var armors = db.GetCollection<Armor>("armors");

            var unequippedArmor = user.EquippedArmor;
            unequippedArmor.OwnerUsername = username;
            armors.Insert(unequippedArmor);

            user.EquippedArmor = null;
            userService.UpdateUser(user);

            return new EquipResult
            {
                Success = true,
                Message = $"{unequippedArmor.Name}の装備を解除しました",
                EquippedArmor = null
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ArmorService.UnequipArmor 例外: {ex.Message} - {ex.StackTrace}");
            return new EquipResult { Success = false, Message = "装備解除中にエラーが発生しました" };
        }
    }

    // 防具を売却する
    public SellResult SellArmor(string username, int armorId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var armors = db.GetCollection<Armor>("armors");
            var armor = armors.FindById(armorId);
            if (armor == null)
                return new SellResult { Success = false, Message = "防具が見つかりません" };

            if (armor.OwnerUsername != username)
                return new SellResult { Success = false, Message = "この防具を売却することはできません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new SellResult { Success = false, Message = "ユーザーが見つかりません" };

            // 売却価格計算
            int sellPrice = CalculateSellPrice(armor);

            // 装备中の防具の場合は装备解除
            if (user.EquippedArmor != null && user.EquippedArmor.Id == armorId)
            {
                user.EquippedArmor = null;
            }

            user.Gil += sellPrice;
            armors.Delete(armorId);
            userService.UpdateUser(user);

            // ゲームイベントを記録
            var eventService = new GameEventService();
            eventService.LogItemSold(username, armor.Name, sellPrice);

            return new SellResult
            {
                Success = true,
                Message = $"{armor.Name}を{sellPrice}ギルで売却しました",
                SellPrice = sellPrice
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ArmorService.SellArmor 例外: {ex.Message} - {ex.StackTrace}");
            return new SellResult { Success = false, Message = "売却中にエラーが発生しました" };
        }
    }

    // 売却価格を計算する
    public int CalculateSellPrice(Armor armor)
    {
        // ベース価格 = レアリティ 基本価格 * レアリティ倍率
        int basePrice = armor.Price > 0 ? armor.Price : 100;
        
        // レアリティによる倍率
        double rarityMultiplier = armor.Rarity switch
        {
            Rarity.White => 1.0,
            Rarity.Purple => 2.0,
            Rarity.Red => 3.0,
            Rarity.Orange => 5.0,
            Rarity.Gold => 10.0,
            Rarity.Rainbow => 20.0,
            _ => 1.0
        };

        // 強化レベルによる加成（強化レベル × 50ギル）
        int enhancementBonus = armor.EnhancementLevel * 50;

        // 耐久度による加成（現在の耐久度%/100）
        double durabilityMultiplier = armor.Durability > 0 ? (double)armor.CurrentDurability / armor.Durability : 0.5;

        // 最終価格計算
        int finalPrice = (int)((basePrice * rarityMultiplier + enhancementBonus) * durabilityMultiplier);
        
        return Math.Max(finalPrice, 1); // 最低1ギル
    }

    // 売却価格を設定する（管理機能）
    public bool SetArmorPrice(string username, int armorId, int newPrice)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var armors = db.GetCollection<Armor>("armors");
            var armor = armors.FindById(armorId);
            if (armor == null)
                return false;

            if (armor.OwnerUsername != username)
                return false;

            armor.Price = newPrice;
            armors.Update(armor);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ArmorService.SetArmorPrice 例外: {ex.Message} - {ex.StackTrace}");
            return false;
        }
    }
}

// 強化結果クラス（基本）
public class EnhancementResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int NewLevel { get; set; }
    public int NewAttack { get; set; }
    public int NewDefense { get; set; }
}

// 強化結果クラス（防具用）
public class ArmorEnhancementResult : EnhancementResult
{
    // Intentionally hide base NewDefense to provide armor-specific value.
    // CS0108 handled: using 'new' to explicitly indicate hiding as per project guidance.
    public new int NewDefense { get; set; }
}

// 修理結果クラス（基本）
public class RepairResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int NewDurability { get; set; }
}

// 修理結果クラス（防具用）
public class ArmorRepairResult : RepairResult
{
}

// 分解結果クラス
public class ArmorDismantleResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public List<Material> Materials { get; set; } = new();
    public int ExperienceGained { get; set; }
}

// 合成結果クラス
public class ArmorSynthesisResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public Armor NewArmor { get; set; } = new();
    public int ExperienceGained { get; set; }
}

// 装備結果クラス
public class EquipResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public Armor? EquippedArmor { get; set; }
}

// 売却結果クラス
public class SellResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int SellPrice { get; set; }
}