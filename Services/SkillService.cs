using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using FFA.Models;

namespace FFA.Services;

public class SkillService
{
    private readonly string _databasePath;

    public SkillService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "skills.db");
    }

    /// <summary>
    /// デフォルトのスキルリストを取得（DBにあればそこから、なければデフォルトを返す）
    /// </summary>
    public List<Skill> GetAllSkills()
    {
        // デフォルトスキルリストを返す（実装簡略化のため）
        return GetDefaultSkills();
    }

    /// <summary>
    /// ジョブ別のスキルを取得
    /// </summary>
    public List<Skill> GetSkillsByJob(Job job)
    {
        return GetAllSkills().Where(s => s.RequiredJob == job).ToList();
    }

    /// <summary>
    /// スキルポイントを獲得（レベルアップ時など）
    /// </summary>
    public void AddSkillPoints(string username, int points)
    {
        var userService = new UserService();
        var user = userService.GetByUsername(username);
        if (user == null) return;

        user.SkillPoints += points;
        userService.UpdateUser(user);
    }

    /// <summary>
    /// スキルを習得する
    /// </summary>
    public string LearnSkill(string username, string skillId)
    {
        var userService = new UserService();
        var user = userService.GetByUsername(username);
        if (user == null) return "ユーザーが見つかりません";

        var skill = GetAllSkills().FirstOrDefault(s => s.Id == skillId);
        if (skill == null) return "スキルが見つかりません";

        // 既に習得済みかチェック
        var existingSkill = user.LearnedSkills.FirstOrDefault(s => s.SkillId == skillId);
        if (existingSkill != null && existingSkill.CurrentLevel >= skill.MaxLevel)
            return "このスキルは最大レベルです";

        // スキルポイントの確認
        if (user.SkillPoints < skill.SkillPointCost)
            return "スキルポイントが不足しています";

        // プレイヤーレベルの確認
        if (user.Level < skill.RequiredPlayerLevel)
            return $"プレイヤーレベルが不足しています（必要: Lv.{skill.RequiredPlayerLevel}）";

        // 前提スキルの確認
        if (!string.IsNullOrEmpty(skill.ParentSkillId))
        {
            var parentSkill = user.LearnedSkills.FirstOrDefault(s => s.SkillId == skill.ParentSkillId);
            if (parentSkill == null || parentSkill.CurrentLevel < 1)
                return "前提スキルを先に習得してください";
        }

        // スキルポイントを消費
        user.SkillPoints -= skill.SkillPointCost;

        // スキルを習得
        if (existingSkill != null)
        {
            existingSkill.CurrentLevel++;
        }
        else
        {
            user.LearnedSkills.Add(new UserSkill
            {
                SkillId = skillId,
                CurrentLevel = 1,
                LearnedAt = DateTime.UtcNow
            });
        }

        userService.UpdateUser(user);
        return "スキルを習得しました";
    }

    /// <summary>
    /// ユーザーのスキルによるステータス加成を計算
    /// </summary>
    public int CalculateSkillBonus(User user, SkillEffectType effectType)
    {
        if (user?.LearnedSkills == null) return 0;

        int totalBonus = 0;
        var allSkills = GetAllSkills();

        foreach (var userSkill in user.LearnedSkills)
        {
            var skill = allSkills.FirstOrDefault(s => s.Id == userSkill.SkillId);
            if (skill != null && skill.EffectType == effectType)
            {
                totalBonus += skill.EffectValue * userSkill.CurrentLevel;
            }
        }

        return totalBonus;
    }

    /// <summary>
    /// 経験値加成スキルを適用
    /// </summary>
    public double GetExpBonusMultiplier(User user)
    {
        int expBonus = CalculateSkillBonus(user, SkillEffectType.ExpBonus);
        return 1.0 + (expBonus / 100.0);
    }

    /// <summary>
    /// ギル加成スキルを適用
    /// </summary>
    public double GetGilBonusMultiplier(User user)
    {
        int gilBonus = CalculateSkillBonus(user, SkillEffectType.GilBonus);
        return 1.0 + (gilBonus / 100.0);
    }

    /// <summary>
    /// デフォルトスキルリスト
    /// </summary>
    private List<Skill> GetDefaultSkills()
    {
        return new List<Skill>
        {
            // Warrior スキル
            new() { Id = "warrior_1", Name = "基礎攻撃", Description = "基礎的な攻撃力を+5", Icon = "⚔️", RequiredJob = Job.Warrior, Tier = 1, MaxLevel = 5, SkillPointCost = 1, EffectType = SkillEffectType.AtkBonus, EffectValue = 5, RequiredPlayerLevel = 1 },
            new() { Id = "warrior_2", Name = "防御強化", Description = "防御力を+10", Icon = "🛡️", RequiredJob = Job.Warrior, Tier = 2, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.DefBonus, EffectValue = 10, ParentSkillId = "warrior_1", RequiredPlayerLevel = 5 },
            new() { Id = "warrior_3", Name = "HP強化", Description = "最大HP+50", Icon = "❤️", RequiredJob = Job.Warrior, Tier = 3, MaxLevel = 3, SkillPointCost = 3, EffectType = SkillEffectType.HpBonus, EffectValue = 50, ParentSkillId = "warrior_2", RequiredPlayerLevel = 10 },
            new() { Id = "warrior_4", Name = "筋体力強化", Description = "STR+5", Icon = "💪", RequiredJob = Job.Warrior, Tier = 3, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.StrBonus, EffectValue = 5, ParentSkillId = "warrior_1", RequiredPlayerLevel = 8 },
            new() { Id = "warrior_ult", Name = "戦士の怒り", Description = "30秒間攻撃力が2倍", Icon = "⚡", RequiredJob = Job.Warrior, Tier = 5, MaxLevel = 1, SkillPointCost = 5, Type = SkillType.Ultimate, ParentSkillId = "warrior_3", RequiredPlayerLevel = 20 },

            // Monk スキル
            new() { Id = "monk_1", Name = "基礎体術", Description = "素早い攻撃でクリティカル率+5%", Icon = "👊", RequiredJob = Job.Monk, Tier = 1, MaxLevel = 5, SkillPointCost = 1, EffectType = SkillEffectType.CriticalRate, EffectValue = 5, RequiredPlayerLevel = 1 },
            new() { Id = "monk_2", Name = "回避熟練", Description = "回避率+10%", Icon = "💨", RequiredJob = Job.Monk, Tier = 2, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.Evasion, EffectValue = 10, ParentSkillId = "monk_1", RequiredPlayerLevel = 5 },
            new() { Id = "monk_3", Name = "HP強化", Description = "最大HP+50", Icon = "❤️", RequiredJob = Job.Monk, Tier = 3, MaxLevel = 3, SkillPointCost = 3, EffectType = SkillEffectType.HpBonus, EffectValue = 50, ParentSkillId = "monk_2", RequiredPlayerLevel = 10 },
            new() { Id = "monk_4", Name = "器用さ強化", Description = "DEX+5", Icon = "🎯", RequiredJob = Job.Monk, Tier = 3, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.DexBonus, EffectValue = 5, ParentSkillId = "monk_1", RequiredPlayerLevel = 8 },

            // White Mage スキル
            new() { Id = "whitemage_1", Name = "基礎治癒", Description = "回復効果+10%", Icon = "✨", RequiredJob = Job.WhiteMage, Tier = 1, MaxLevel = 5, SkillPointCost = 1, EffectType = SkillEffectType.MpBonus, EffectValue = 10, RequiredPlayerLevel = 1 },
            new() { Id = "whitemage_2", Name = "魔力強化", Description = "INT+5", Icon = "📘", RequiredJob = Job.WhiteMage, Tier = 2, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.IntBonus, EffectValue = 5, ParentSkillId = "whitemage_1", RequiredPlayerLevel = 5 },
            new() { Id = "whitemage_3", Name = "EXPバフ", Description = "獲得経験値+20%", Icon = "📈", RequiredJob = Job.WhiteMage, Tier = 3, MaxLevel = 3, SkillPointCost = 3, EffectType = SkillEffectType.ExpBonus, EffectValue = 20, ParentSkillId = "whitemage_2", RequiredPlayerLevel = 10 },
            new() { Id = "whitemage_4", Name = "体力強化", Description = "VIT+5", Icon = "🛡️", RequiredJob = Job.WhiteMage, Tier = 3, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.VitBonus, EffectValue = 5, ParentSkillId = "whitemage_1", RequiredPlayerLevel = 8 },

            // Black Mage スキル
            new() { Id = "blackmage_1", Name = "基礎魔法", Description = "魔法攻撃力+10", Icon = "🔥", RequiredJob = Job.BlackMage, Tier = 1, MaxLevel = 5, SkillPointCost = 1, EffectType = SkillEffectType.IntBonus, EffectValue = 10, RequiredPlayerLevel = 1 },
            new() { Id = "blackmage_2", Name = "魔力集中", Description = "MP効率+15%", Icon = "💎", RequiredJob = Job.BlackMage, Tier = 2, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.MpBonus, EffectValue = 15, ParentSkillId = "blackmage_1", RequiredPlayerLevel = 5 },
            new() { Id = "blackmage_3", Name = "ギルバフ", Description = "入手ギル+25%", Icon = "💰", RequiredJob = Job.BlackMage, Tier = 3, MaxLevel = 3, SkillPointCost = 3, EffectType = SkillEffectType.GilBonus, EffectValue = 25, ParentSkillId = "blackmage_2", RequiredPlayerLevel = 10 },
            new() { Id = "blackmage_4", Name = "知力強化", Description = "INT+5", Icon = "📚", RequiredJob = Job.BlackMage, Tier = 3, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.IntBonus, EffectValue = 5, ParentSkillId = "blackmage_1", RequiredPlayerLevel = 8 },

            // Ranger スキル
            new() { Id = "ranger_1", Name = "弓術基礎", Description = "攻撃力+5", Icon = "🏹", RequiredJob = Job.Ranger, Tier = 1, MaxLevel = 5, SkillPointCost = 1, EffectType = SkillEffectType.AtkBonus, EffectValue = 5, RequiredPlayerLevel = 1 },
            new() { Id = "ranger_2", Name = "命中精度", Description = "DEX+5", Icon = "🎯", RequiredJob = Job.Ranger, Tier = 2, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.DexBonus, EffectValue = 5, ParentSkillId = "ranger_1", RequiredPlayerLevel = 5 },
            new() { Id = "ranger_3", Name = "経験値バフ", Description = "経験値+15%", Icon = "📈", RequiredJob = Job.Ranger, Tier = 3, MaxLevel = 3, SkillPointCost = 3, EffectType = SkillEffectType.ExpBonus, EffectValue = 15, ParentSkillId = "ranger_2", RequiredPlayerLevel = 10 },

            // Thief スキル
            new() { Id = "thief_1", Name = "盗賊の技術", Description = "ギル入手量+10%", Icon = "👛", RequiredJob = Job.Thief, Tier = 1, MaxLevel = 5, SkillPointCost = 1, EffectType = SkillEffectType.GilBonus, EffectValue = 10, RequiredPlayerLevel = 1 },
            new() { Id = "thief_2", Name = "回避技術", Description = "回避率+5%", Icon = "💨", RequiredJob = Job.Thief, Tier = 2, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.Evasion, EffectValue = 5, ParentSkillId = "thief_1", RequiredPlayerLevel = 5 },
            new() { Id = "thief_3", Name = "クリティカル率", Description = "クリティカル率+10%", Icon = "💥", RequiredJob = Job.Thief, Tier = 3, MaxLevel = 3, SkillPointCost = 3, EffectType = SkillEffectType.CriticalRate, EffectValue = 10, ParentSkillId = "thief_2", RequiredPlayerLevel = 10 },

            // Paladin スキル
            new() { Id = "paladin_1", Name = "聖印", Description = "DEF+5", Icon = "✝️", RequiredJob = Job.Paladin, Tier = 1, MaxLevel = 5, SkillPointCost = 1, EffectType = SkillEffectType.DefBonus, EffectValue = 5, RequiredPlayerLevel = 1 },
            new() { Id = "paladin_2", Name = "聖なる守り", Description = "HP+30", Icon = "🛡️", RequiredJob = Job.Paladin, Tier = 2, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.HpBonus, EffectValue = 30, ParentSkillId = "paladin_1", RequiredPlayerLevel = 5 },
            new() { Id = "paladin_3", Name = "信仰心", Description = "VIT+5", Icon = "🙏", RequiredJob = Job.Paladin, Tier = 3, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.VitBonus, EffectValue = 5, ParentSkillId = "paladin_2", RequiredPlayerLevel = 10 },

            // DarkKnight スキル
            new() { Id = "darkknight_1", Name = "暗黒剣", Description = "ATK+5", Icon = "🗡️", RequiredJob = Job.DarkKnight, Tier = 1, MaxLevel = 5, SkillPointCost = 1, EffectType = SkillEffectType.AtkBonus, EffectValue = 5, RequiredPlayerLevel = 1 },
            new() { Id = "darkknight_2", Name = "生命吸収", Description = "HP+30", Icon = "🩸", RequiredJob = Job.DarkKnight, Tier = 2, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.HpBonus, EffectValue = 30, ParentSkillId = "darkknight_1", RequiredPlayerLevel = 5 },
            new() { Id = "darkknight_3", Name = "STR強化", Description = "STR+5", Icon = "💪", RequiredJob = Job.DarkKnight, Tier = 3, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.StrBonus, EffectValue = 5, ParentSkillId = "darkknight_2", RequiredPlayerLevel = 10 },

            // Bard スキル
            new() { Id = "bard_1", Name = "演奏の基礎", Description = "INT+3", Icon = "🎵", RequiredJob = Job.Bard, Tier = 1, MaxLevel = 5, SkillPointCost = 1, EffectType = SkillEffectType.IntBonus, EffectValue = 3, RequiredPlayerLevel = 1 },
            new() { Id = "bard_2", Name = "/MP回復", Description = "MP+20", Icon = "🎶", RequiredJob = Job.Bard, Tier = 2, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.MpBonus, EffectValue = 20, ParentSkillId = "bard_1", RequiredPlayerLevel = 5 },
            new() { Id = "bard_3", Name = "経験値歌", Description = "経験値+15%", Icon = "🎤", RequiredJob = Job.Bard, Tier = 3, MaxLevel = 3, SkillPointCost = 3, EffectType = SkillEffectType.ExpBonus, EffectValue = 15, ParentSkillId = "bard_2", RequiredPlayerLevel = 10 },

            // Ninja スキル
            new() { Id = "ninja_1", Name = "忍者の心得", Description = "DEX+5", Icon = "⭐", RequiredJob = Job.Ninja, Tier = 1, MaxLevel = 5, SkillPointCost = 1, EffectType = SkillEffectType.DexBonus, EffectValue = 5, RequiredPlayerLevel = 1 },
            new() { Id = "ninja_2", Name = "隠密", Description = "回避率+5%", Icon = "🌑", RequiredJob = Job.Ninja, Tier = 2, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.Evasion, EffectValue = 5, ParentSkillId = "ninja_1", RequiredPlayerLevel = 5 },
            new() { Id = "ninja_3", Name = "攻撃速度", Description = "ATK+5", Icon = "⚡", RequiredJob = Job.Ninja, Tier = 3, MaxLevel = 5, SkillPointCost = 2, EffectType = SkillEffectType.AtkBonus, EffectValue = 5, ParentSkillId = "ninja_2", RequiredPlayerLevel = 10 },
        };
    }
}
