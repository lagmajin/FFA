using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using FFA.Models;

namespace FFA.Services;

public class AccessoryService
{
    private readonly string _databasePath;

    public AccessoryService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "accessories.db");
    }

    /// <summary>
    /// 装飾品をDBに保存
    /// </summary>
    public void Save(Accessory accessory)
    {
        using var db = new LiteDatabase(_databasePath);
        var accessories = db.GetCollection<Accessory>("accessories");
        accessories.Upsert(accessory);
    }

    /// <summary>
    /// IDで装飾品を取得
    /// </summary>
    public Accessory? GetById(int id)
    {
        using var db = new LiteDatabase(_databasePath);
        var accessories = db.GetCollection<Accessory>("accessories");
        return accessories.FindById(id);
    }

    /// <summary>
    /// プレイヤーの所有する装飾品を取得
    /// </summary>
    public List<Accessory> GetByOwner(string username)
    {
        using var db = new LiteDatabase(_databasePath);
        var accessories = db.GetCollection<Accessory>("accessories");
        return accessories.Find(a => a.OwnerUsername == username).ToList();
    }

    ///  装飾品を<summary>
    ///削除
    /// </summary>
    public bool Delete(int id)
    {
        using var db = new LiteDatabase(_databasePath);
        var accessories = db.GetCollection<Accessory>("accessories");
        return accessories.Delete(id);
    }

    /// <summary>
    /// 装飾品を装備する
    /// </summary>
    public string EquipAccessory(string username, int accessoryId, int slot = 1)
    {
        var userService = new UserService();
        var user = userService.GetByUsername(username);
        if (user == null) return "ユーザーが見つかりません";

        var accessory = GetById(accessoryId);
        if (accessory == null) return "装飾品が見つかりません";

        if (accessory.OwnerUsername != username) return "この装飾品を装備する権限がありません";

        // 装備条件をチェック
        if (user.Status.Str < accessory.RequiredStr)
            return $"力不足です（必要: {accessory.RequiredStr}, 現在の力: {user.Status.Str}）";
        if (user.Status.Dex < accessory.RequiredDex)
            return $"器用さ不足です（必要: {accessory.RequiredDex}, 現在の器用さ: {user.Status.Dex}）";
        if (user.Status.Int < accessory.RequiredInt)
            return $"知力不足です（必要: {accessory.RequiredInt}, 現在の知力: {user.Status.Int}）";
        if (user.Status.Vit < accessory.RequiredVit)
            return $"体力不足です（必要: {accessory.RequiredVit}, 現在の体力: {user.Status.Vit}）";
        if (user.Status.Agi < accessory.RequiredAgi)
            return $"敏捷性不足です（必要: {accessory.RequiredAgi}, 現在の敏捷性: {user.Status.Agi}）";
        if (user.Status.Luk < accessory.RequiredLuk)
            return $"運不足です（必要: {accessory.RequiredLuk}, 現在の運: {user.Status.Luk}）";

        // 装备配件
        if (slot == 1)
        {
            user.EquippedAccessory1 = accessory;
        }
        else if (slot == 2)
        {
            user.EquippedAccessory2 = accessory;
        }
        else
        {
            return "無効なスロットです";
        }

        userService.UpdateUser(user);
        
        return "装備しました";
    }

    /// <summary>
    /// 装飾品を装備解除する
    /// </summary>
    public string UnequipAccessory(string username, int slot = 1)
    {
        var userService = new UserService();
        var user = userService.GetByUsername(username);
        if (user == null) return "ユーザーが見つかりません";

        if (slot == 1)
        {
            user.EquippedAccessory1 = null;
        }
        else if (slot == 2)
        {
            user.EquippedAccessory2 = null;
        }
        else
        {
            return "無効なスロットです";
        }

        userService.UpdateUser(user);
        return "装備を解除しました";
    }

    /// <summary>
    /// 装飾品による総防御力を取得
    /// </summary>
    public int GetTotalDefense(string username)
    {
        try
        {
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null) return 0;
            
            int defense = 0;
            
            if (user.EquippedAccessory1 != null)
            {
                defense += user.EquippedAccessory1.Defense;
                defense += user.EquippedAccessory1.EnhancementLevel * 2;
                defense = (int)(defense * SystemIntegration.GetRarityMultiplier(user.EquippedAccessory1.Rarity));
            }
            
            if (user.EquippedAccessory2 != null)
            {
                defense += user.EquippedAccessory2.Defense;
                defense += user.EquippedAccessory2.EnhancementLevel * 2;
                defense = (int)(defense * SystemIntegration.GetRarityMultiplier(user.EquippedAccessory2.Rarity));
            }
            
            return defense;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 装飾品による総攻撃力を取得
    /// </summary>
    public int GetTotalAttack(string username)
    {
        try
        {
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null) return 0;
            
            int attack = 0;
            
            if (user.EquippedAccessory1 != null)
            {
                attack += user.EquippedAccessory1.Attack;
                attack += user.EquippedAccessory1.EnhancementLevel * 2;
                attack = (int)(attack * SystemIntegration.GetRarityMultiplier(user.EquippedAccessory1.Rarity));
            }
            
            if (user.EquippedAccessory2 != null)
            {
                attack += user.EquippedAccessory2.Attack;
                attack += user.EquippedAccessory2.EnhancementLevel * 2;
                attack = (int)(attack * SystemIntegration.GetRarityMultiplier(user.EquippedAccessory2.Rarity));
            }
            
            return attack;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 装飾品による総魔法力を取得
    /// </summary>
    public int GetTotalMagic(string username)
    {
        try
        {
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null) return 0;
            
            int magic = 0;
            
            if (user.EquippedAccessory1 != null)
            {
                magic += user.EquippedAccessory1.Magic;
                magic += user.EquippedAccessory1.EnhancementLevel * 2;
                magic = (int)(magic * SystemIntegration.GetRarityMultiplier(user.EquippedAccessory1.Rarity));
            }
            
            if (user.EquippedAccessory2 != null)
            {
                magic += user.EquippedAccessory2.Magic;
                magic += user.EquippedAccessory2.EnhancementLevel * 2;
                magic = (int)(magic * SystemIntegration.GetRarityMultiplier(user.EquippedAccessory2.Rarity));
            }
            
            return magic;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 装飾品を強化する
    /// </summary>
    public EnhancementResult EnhanceAccessory(string username, int accessoryId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var accessories = db.GetCollection<Accessory>("accessories");
            var accessory = accessories.FindById(accessoryId);
            if (accessory == null)
                return new AccessoryEnhancementResult { Success = false, Message = "装飾品が見つかりません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new AccessoryEnhancementResult { Success = false, Message = "ユーザーが見つかりません" };

            // 強化条件を確認
            if (accessory.EnhancementLevel >= accessory.MaxEnhancementLevel)
                return new AccessoryEnhancementResult { Success = false, Message = "これ以上強化できません" };

            if (user.Gil < accessory.EnhancementCost)
                return new AccessoryEnhancementResult { Success = false, Message = "ギルが不足しています" };

            // 成功率を計算
            double successRate = accessory.EnhancementSuccessRate;
            // 強化レベルが上がるほど成功率が下がる
            successRate -= accessory.EnhancementLevel * 0.05;
            if (successRate < 0.1) successRate = 0.1;

            Random rand = new Random();
            bool isSuccess = rand.NextDouble() < successRate;

            if (isSuccess)
            {
                accessory.EnhancementLevel++;
                accessories.Update(accessory);
                
                // ユーザーからギルを控除
                user.Gil -= accessory.EnhancementCost;
                userService.UpdateUser(user);
                
                return new AccessoryEnhancementResult 
                { 
                    Success = true, 
                    Message = $"強化成功！Lv.{accessory.EnhancementLevel}",
                    NewAttack = accessory.Attack + accessory.EnhancementLevel * 2,
                    NewDefense = accessory.Defense + accessory.EnhancementLevel * 2,
                    NewMagic = accessory.Magic + accessory.EnhancementLevel * 2
                };
            }
            else
            {
                // 強化失敗 - ギルは消費される
                user.Gil -= accessory.EnhancementCost;
                userService.UpdateUser(user);
                
                return new AccessoryEnhancementResult 
                { 
                    Success = false, 
                    Message = "強化失敗...",
                    NewAttack = accessory.Attack,
                    NewDefense = accessory.Defense,
                    NewMagic = accessory.Magic
                };
            }
        }
        catch (Exception ex)
        {
            return new AccessoryEnhancementResult { Success = false, Message = $"エラー: {ex.Message}" };
        }
    }

    /// <summary>
    /// 装飾品を修理する
    /// </summary>
    public RepairResult RepairAccessory(string username, int accessoryId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var accessories = db.GetCollection<Accessory>("accessories");
            var accessory = accessories.FindById(accessoryId);
            if (accessory == null)
                return new AccessoryRepairResult { Success = false, Message = "装飾品が見つかりません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new AccessoryRepairResult { Success = false, Message = "ユーザーが見つかりません" };

            // 耐久度が減っていない場合は修理不要
            if (accessory.CurrentDurability >= accessory.Durability)
                return new AccessoryRepairResult { Success = false, Message = "耐久度は満点です" };

            if (user.Gil < accessory.RepairCost)
                return new AccessoryRepairResult { Success = false, Message = "ギルが不足しています" };

            // 修理を実行
            user.Gil -= accessory.RepairCost;
            accessory.CurrentDurability = Math.Min(accessory.CurrentDurability + accessory.RepairAmount, accessory.Durability);
            accessories.Update(accessory);
            userService.UpdateUser(user);

            return new AccessoryRepairResult 
            { 
                Success = true, 
                Message = $"修理完了！耐久度: {accessory.CurrentDurability}/{accessory.Durability}",
                NewDurability = accessory.CurrentDurability
            };
        }
        catch (Exception ex)
        {
            return new AccessoryRepairResult { Success = false, Message = $"エラー: {ex.Message}" };
        }
    }

    /// <summary>
    /// 装飾品を売却する
    /// </summary>
    public AccessorySellResult SellAccessory(string username, int accessoryId)
    {
        try
        {
            var accessory = GetById(accessoryId);
            if (accessory == null)
                return new AccessorySellResult { Success = false, Message = "装飾品が見つかりません" };

            if (accessory.OwnerUsername != username)
                return new AccessorySellResult { Success = false, Message = "この装飾品を売却する権限がありません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new AccessorySellResult { Success = false, Message = "ユーザーが見つかりません" };

            // 装備中の装飾品かどうかはチェック
            if ((user.EquippedAccessory1?.Id == accessoryId) || (user.EquippedAccessory2?.Id == accessoryId))
                return new AccessorySellResult { Success = false, Message = "装備中の装飾品は売却できません" };

            // 売却価格を計算
            int sellPrice = CalculateSellPrice(accessory);
            
            // ユーザーにギルを加算
            user.Gil += sellPrice;
            userService.UpdateUser(user);

            // データベースから削除
            Delete(accessoryId);

            return new AccessorySellResult 
            { 
                Success = true, 
                Message = $"{accessory.Name}を{sellPrice}ギルで売却しました",
                Price = sellPrice
            };
        }
        catch (Exception ex)
        {
            return new AccessorySellResult { Success = false, Message = $"エラー: {ex.Message}" };
        }
    }

    /// <summary>
    /// 売却価格を計算する
    /// </summary>
    public int CalculateSellPrice(Accessory accessory)
    {
        if (accessory == null) return 0;

        // 基本価格 = 攻撃力 + 防御力 + 魔法力 + レアリティ倍率
        int basePrice = accessory.Attack + accessory.Defense + accessory.Magic;
        
        // レアリティによる加成
        int rarityMultiplier = (int)SystemIntegration.GetRarityMultiplier(accessory.Rarity);
        
        // 強化レベルによる加成
        int enhancementBonus = accessory.EnhancementLevel * 10;
        
        // 最終価格
        int finalPrice = (basePrice * rarityMultiplier) + enhancementBonus;
        
        // 最低価格
        return Math.Max(finalPrice, 1);
    }

    /// <summary>
    /// 装飾品を作成する（デバッグ用/ゲーム内生成用）
    /// </summary>
    public Accessory CreateAccessory(string name, string type, int attack, int defense, int magic, Rarity rarity, int price, string ownerUsername)
    {
        var accessory = new Accessory
        {
            Name = name,
            Type = type,
            Attack = attack,
            Defense = defense,
            Magic = magic,
            Rarity = rarity,
            Price = price,
            OwnerUsername = ownerUsername,
            EnhancementLevel = 0,
            Durability = 100,
            CurrentDurability = 100,
            MaxEnhancementLevel = 10,
            EnhancementCost = 100,
            EnhancementSuccessRate = 0.8,
            RepairCost = 50,
            RepairAmount = 20
        };

        Save(accessory);
        return accessory;
    }
}

// 強化結果クラス
public class AccessoryEnhancementResult : EnhancementResult
{
    public int NewAttack { get; set; }
    public int NewDefense { get; set; }
    public int NewMagic { get; set; }
}

// 修理結果クラス
public class AccessoryRepairResult : RepairResult
{
    public int NewDurability { get; set; }
}

// 売却結果クラス
public class AccessorySellResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int Price { get; set; }
}
