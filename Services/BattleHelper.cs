using FFA.Models;

#pragma warning disable CS0618 // Obsolete警告を無効化

namespace FFA.Services;

/// <summary>
/// 戦闘時のプレイヤー情報を取得する便利クラス
/// 装備補正込みのステータス計算などを提供
/// </summary>
public static class BattleHelper
{
    /// <summary>
    /// プレイヤーの総合攻撃力を取得（装備込み）
    /// </summary>
    public static int GetTotalAttack(User user)
    {
        if (user == null) return 0;
        
        var baseAttack = 10; // 基礎攻撃力
        var statusBonus = user.Status?.Str ?? 0; // 力ステータス
        
        // 職業ボーナス
        var jobBonus = GetJobAttackBonus(user.Job);
        
        // 武器攻撃力
        var weaponAttack = user.EquippedWeapon?.Attack ?? 0;
        var weaponEnhanceBonus = (user.EquippedWeapon?.EnhancementLevel ?? 0) * 2;
        
        // アクセサリー攻撃力
        var accessoryAttack = (user.EquippedAccessory1?.Attack ?? 0) + (user.EquippedAccessory2?.Attack ?? 0);
        
        // レベル補正
        var levelBonus = user.Level / 2;
        
        return baseAttack + statusBonus + jobBonus + weaponAttack + weaponEnhanceBonus + accessoryAttack + levelBonus;
    }
    
    /// <summary>
    /// プレイヤーの総合防御力を取得（装備込み）
    /// </summary>
    public static int GetTotalDefense(User user)
    {
        if (user == null) return 0;
        
        var baseDefense = 5; // 基礎防御力
        var statusBonus = user.Status?.Vit ?? 0; // 体力ステータス
        
        // 職業ボーナス
        var jobBonus = GetJobDefenseBonus(user.Job);
        
        // 防具防御力
        var armorDefense = user.EquippedArmor?.Defense ?? 0;
        var armorEnhanceBonus = (user.EquippedArmor?.EnhancementLevel ?? 0) * 1;
        
        // アクセサリー防御力
        var accessoryDefense = (user.EquippedAccessory1?.Defense ?? 0) + (user.EquippedAccessory2?.Defense ?? 0);
        
        // レベル補正
        var levelBonus = user.Level / 3;
        
        return baseDefense + statusBonus + jobBonus + armorDefense + armorEnhanceBonus + accessoryDefense + levelBonus;
    }
    
    /// <summary>
    /// プレイヤーの総合魔法力を取得（装備込み）
    /// </summary>
    public static int GetTotalMagic(User user)
    {
        if (user == null) return 0;
        
        var baseMagic = 5; // 基礎魔法力
        var statusBonus = user.Status?.Int ?? 0; // 知力ステータス
        
        // 職業ボーナス
        var jobBonus = GetJobMagicBonus(user.Job);
        
        // アクセサリー魔法力
        var accessoryMagic = (user.EquippedAccessory1?.Magic ?? 0) + (user.EquippedAccessory2?.Magic ?? 0);
        
        // レベル補正
        var levelBonus = user.Level / 4;
        
        return baseMagic + statusBonus + jobBonus + accessoryMagic + levelBonus;
    }
    
    /// <summary>
    /// プレイヤーの総合素早さを取得（装備込み）
    /// </summary>
    public static int GetTotalSpeed(User user)
    {
        if (user == null) return 10;
        
        var baseSpeed = 10;
        var statusBonus = user.Status?.Agi ?? 0;
        
        // 重量ペナルティ
        var weightPenalty = (int)(GetTotalWeight(user) / 10);
        
        // 職業ボーナス
        var jobBonus = GetJobSpeedBonus(user.Job);
        
        return Math.Max(1, baseSpeed + statusBonus + jobBonus - weightPenalty);
    }
    
    /// <summary>
    /// プレイヤーの総合重量を取得（装備込み）
    /// </summary>
    public static double GetTotalWeight(User user)
    {
        if (user == null) return 0;
        
        var weight = 0.0;
        
        // 武器重量
        weight += user.EquippedWeapon?.Weight ?? 0;
        
        // 防具重量
        weight += user.EquippedArmor?.Weight ?? 0;
        
        // アクセサリー重量（ほぼ無視できるが一応計算）
        weight += user.EquippedAccessory1?.Weight ?? 0;
        weight += user.EquippedAccessory2?.Weight ?? 0;
        
        return weight;
    }
    
    /// <summary>
    /// プレイヤーのクリティカル率を取得（%）
    /// </summary>
    public static int GetCriticalChance(User user)
    {
        if (user == null) return 3;
        
        var baseCrit = 3; // 基礎クリティカル率3%
        
        // 運ステータスボーナス
        var luckBonus = (user.Status?.Luk ?? 0) / 10;
        
        // 職業ボーナス
        var jobBonus = GetJobCriticalBonus(user.Job);
        
        // アクセサリーボーナス（特殊効果から計算）
        var accessoryBonus = GetAccessoryCriticalBonus(user);
        
        return baseCrit + luckBonus + jobBonus + accessoryBonus;
    }
    
    /// <summary>
    /// プレイヤーの回避率を取得（%）
    /// </summary>
    public static int GetEvasionChance(User user)
    {
        if (user == null) return 5;
        
        var baseEvasion = 5; // 基礎回避率5%
        
        // 素早さボーナス
        var speedBonus = GetTotalSpeed(user) / 20;
        
        // 職業ボーナス
        var jobBonus = GetJobEvasionBonus(user.Job);
        
        // 軽装ボーナス（重量が軽いほど回避しやすい）
        var weightBonus = Math.Max(0, (int)((20 - GetTotalWeight(user)) / 2));
        
        return baseEvasion + speedBonus + jobBonus + weightBonus;
    }
    
    /// <summary>
    /// プレイヤーの命中率を取得（%）
    /// </summary>
    public static int GetAccuracyChance(User user)
    {
        if (user == null) return 90;
        
        var baseAccuracy = 90; // 基礎命中率90%
        
        // 器用さボーナス
        var dexBonus = (user.Status?.Dex ?? 0) / 5;
        
        // 職業ボーナス
        var jobBonus = GetJobAccuracyBonus(user.Job);
        
        return Math.Min(100, baseAccuracy + dexBonus + jobBonus);
    }
    
    /// <summary>
    /// プレイヤーのHP再生量を取得（ターン毎）
    /// </summary>
    public static int GetHPRegeneration(User user)
    {
        if (user == null) return 0;
        
        var regen = 0;
        
        // 体力ステータスボーナス
        regen += (user.Status?.Vit ?? 0) / 10;
        
        // 職業ボーナス
        regen += GetJobHPRegenBonus(user.Job);
        
        return regen;
    }
    
    /// <summary>
    /// 戦闘時の総合ステータス情報を取得
    /// </summary>
    public static BattleStats GetBattleStats(User user)
    {
        return new BattleStats
        {
            Attack = GetTotalAttack(user),
            Defense = GetTotalDefense(user),
            Magic = GetTotalMagic(user),
            Speed = GetTotalSpeed(user),
            CriticalChance = GetCriticalChance(user),
            EvasionChance = GetEvasionChance(user),
            AccuracyChance = GetAccuracyChance(user),
            HPRegeneration = GetHPRegeneration(user),
            TotalWeight = GetTotalWeight(user),
            CurrentHP = user?.HP ?? 0,
            MaxHP = user?.MaxHP ?? 100,
            Level = user?.Level ?? 1
        };
    }
    
    #region 職業ボーナス取得メソッド
    
    private static int GetJobAttackBonus(Job job) => job switch
    {
        Job.Warrior => 5,
        Job.Paladin => 3,
        Job.DarkKnight => 8,
        Job.HolyKnight => 4,
        Job.DeathKnight => 10,
        Job.Duelist => 6,
        Job.Grandmaster => 8,
        Job.Thief => 2,
        Job.Ninja => 4,
        Job.Ranger => 3,
        Job.Monk => 6,
        Job.BlackMage => 1,
        Job.WhiteMage => 0,
        Job.ArchMage => 2,
        Job.BeastMaster => 3,
        Job.Bard => 2,
        Job.GraveRobber => 3,
        _ => 0
    };
    
    private static int GetJobDefenseBonus(Job job) => job switch
    {
        Job.Warrior => 3,
        Job.Paladin => 8,
        Job.DarkKnight => 4,
        Job.HolyKnight => 6,
        Job.DeathKnight => 5,
        Job.Duelist => 3,
        Job.Grandmaster => 4,
        Job.Thief => 1,
        Job.Ninja => 2,
        Job.Ranger => 2,
        Job.Monk => 4,
        Job.BlackMage => 0,
        Job.WhiteMage => 1,
        Job.ArchMage => 1,
        Job.BeastMaster => 2,
        Job.Bard => 1,
        Job.GraveRobber => 1,
        _ => 0
    };
    
    private static int GetJobMagicBonus(Job job) => job switch
    {
        Job.Warrior => 0,
        Job.Paladin => 2,
        Job.DarkKnight => 3,
        Job.HolyKnight => 3,
        Job.DeathKnight => 2,
        Job.Duelist => 1,
        Job.Grandmaster => 2,
        Job.Thief => 0,
        Job.Ninja => 1,
        Job.Ranger => 0,
        Job.Monk => 2,
        Job.BlackMage => 10,
        Job.WhiteMage => 8,
        Job.ArchMage => 12,
        Job.BeastMaster => 3,
        Job.Bard => 4,
        Job.GraveRobber => 2,
        _ => 0
    };
    
    private static int GetJobSpeedBonus(Job job) => job switch
    {
        Job.Warrior => 0,
        Job.Paladin => -1,
        Job.DarkKnight => 0,
        Job.HolyKnight => -2,
        Job.DeathKnight => 0,
        Job.Duelist => 3,
        Job.Grandmaster => 5,
        Job.Thief => 8,
        Job.Ninja => 10,
        Job.Ranger => 5,
        Job.Monk => 4,
        Job.BlackMage => 0,
        Job.WhiteMage => 0,
        Job.ArchMage => 1,
        Job.BeastMaster => 2,
        Job.Bard => 3,
        Job.GraveRobber => 3,
        _ => 0
    };
    
    private static int GetJobCriticalBonus(Job job) => job switch
    {
        Job.Warrior => 0,
        Job.Paladin => 0,
        Job.DarkKnight => 2,
        Job.HolyKnight => 0,
        Job.DeathKnight => 3,
        Job.Duelist => 4,
        Job.Grandmaster => 5,
        Job.Thief => 4,
        Job.Ninja => 5,
        Job.Ranger => 3,
        Job.Monk => 2,
        Job.BlackMage => 1,
        Job.WhiteMage => 0,
        Job.ArchMage => 1,
        Job.BeastMaster => 2,
        Job.Bard => 1,
        Job.GraveRobber => 2,
        _ => 0
    };
    
    private static int GetJobEvasionBonus(Job job) => job switch
    {
        Job.Warrior => 0,
        Job.Paladin => -1,
        Job.DarkKnight => 0,
        Job.HolyKnight => -2,
        Job.DeathKnight => -1,
        Job.Duelist => 2,
        Job.Grandmaster => 3,
        Job.Thief => 5,
        Job.Ninja => 7,
        Job.Ranger => 3,
        Job.Monk => 4,
        Job.BlackMage => 0,
        Job.WhiteMage => 1,
        Job.ArchMage => 0,
        Job.BeastMaster => 1,
        Job.Bard => 2,
        Job.GraveRobber => 2,
        _ => 0
    };
    
    private static int GetJobAccuracyBonus(Job job) => job switch
    {
        Job.Warrior => 0,
        Job.Paladin => 2,
        Job.DarkKnight => 1,
        Job.HolyKnight => 2,
        Job.DeathKnight => 1,
        Job.Duelist => 4,
        Job.Grandmaster => 5,
        Job.Thief => 3,
        Job.Ninja => 4,
        Job.Ranger => 5,
        Job.Monk => 2,
        Job.BlackMage => 1,
        Job.WhiteMage => 0,
        Job.ArchMage => 1,
        Job.BeastMaster => 2,
        Job.Bard => 2,
        Job.GraveRobber => 1,
        _ => 0
    };
    
    private static int GetJobHPRegenBonus(Job job) => job switch
    {
        Job.Warrior => 1,
        Job.Paladin => 2,
        Job.DarkKnight => 0,
        Job.HolyKnight => 2,
        Job.DeathKnight => 0,
        Job.Duelist => 1,
        Job.Grandmaster => 1,
        Job.Thief => 0,
        Job.Ninja => 0,
        Job.Ranger => 0,
        Job.Monk => 3,
        Job.BlackMage => 0,
        Job.WhiteMage => 1,
        Job.ArchMage => 0,
        Job.BeastMaster => 0,
        Job.Bard => 0,
        Job.GraveRobber => 0,
        _ => 0
    };
    
    #endregion
    
    #region アクセサリーボーナス
    
    private static int GetAccessoryCriticalBonus(User user)
    {
        var bonus = 0;
        
        // 特殊効果に「クリティカル」が含まれる場合のボーナス
        if (user.EquippedAccessory1?.SpecialEffects?.Any(e => e.Contains("クリティカル") || e.Contains("Critical")) == true)
            bonus += 3;
        if (user.EquippedAccessory2?.SpecialEffects?.Any(e => e.Contains("クリティカル") || e.Contains("Critical")) == true)
            bonus += 3;
        
        return bonus;
    }
    
    #endregion
}

/// <summary>
/// 戦闘時の総合ステータス情報
/// </summary>
public class BattleStats
{
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Magic { get; set; }
    public int Speed { get; set; }
    public int CriticalChance { get; set; }
    public int EvasionChance { get; set; }
    public int AccuracyChance { get; set; }
    public int HPRegeneration { get; set; }
    public double TotalWeight { get; set; }
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public int Level { get; set; }
    
    /// <summary>
    /// ステータス概要文字列
    /// </summary>
    public string Summary => $"ATK:{Attack} DEF:{Defense} MAG:{Magic} SPD:{Speed} CRT:{CriticalChance}% EVD:{EvasionChance}%";
}
