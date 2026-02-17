using FFA.Models;

namespace FFA.Services;

/// <summary>
/// ギルド拡張サービス - ギルドレベル、スキル、戦等功能
/// </summary>
public class GuildEnhancementService
{
    private readonly GuildService _guildService;
    private readonly Random _random = new();
    
    // ギルド経験値テーブル（レベルごと）
    public static readonly int[] GuildExpTable = new int[]
    {
        0,          // Lv1
        1000,       // Lv2
        2500,       // Lv3
        5000,       // Lv4
        10000,      // Lv5
        20000,      // Lv6
        35000,      // Lv7
        55000,      // Lv8
        80000,      // Lv9
        120000,     // Lv10
    };
    
    // ギルドスキルの定義
    public static readonly List<GuildSkill> GuildSkills = new()
    {
        new GuildSkill { Id = 1, Name = "攻撃力強化", Description = "全員の攻撃力が5%上昇", EffectType = "AttackBonus", EffectValue = 5, RequiredLevel = 2 },
        new GuildSkill { Id = 2, Name = "防御力強化", Description = "全員の防御力が5%上昇", EffectType = "DefenseBonus", EffectValue = 5, RequiredLevel = 2 },
        new GuildSkill { Id = 3, Name = "経験値 Boost", Description = "獲得経験値が10%上昇", EffectType = "ExpBonus", EffectValue = 10, RequiredLevel = 3 },
        new GuildSkill { Id = 4, Name = "採掘効率", Description = "採掘的成功率が10%上昇", EffectType = "MiningBonus", EffectValue = 10, RequiredLevel = 3 },
        new GuildSkill { Id = 5, Name = "ドロップ率UP", Description = "アイテムドロップ率が5%上昇", EffectType = "DropBonus", EffectValue = 5, RequiredLevel = 4 },
        new GuildSkill { Id = 6, Name = " 회복력 강화", Description = "全員のHP回復速度が10%上昇", EffectType = "HPRegenBonus", EffectValue = 10, RequiredLevel = 4 },
        new GuildSkill { Id = 7, Name = "交易利益", Description = "ショップ売却価格が5%上昇", EffectType = "SellPriceBonus", EffectValue = 5, RequiredLevel = 5 },
        new GuildSkill { Id = 8, Name = "ギルド戦争", Description = "ギルド戦での攻撃力が20%上昇", EffectType = "GuildWarAttack", EffectValue = 20, RequiredLevel = 6 },
    };
    
    public GuildEnhancementService(GuildService guildService)
    {
        _guildService = guildService;
    }
    
    /// <summary>
    /// ギルドのレベルを取得
    /// </summary>
    public int GetGuildLevel(Guild guild)
    {
        if (guild == null) return 0;
        
        for (int i = GuildExpTable.Length - 1; i >= 0; i--)
        {
            if (guild.TotalExp >= GuildExpTable[i])
                return i + 1;
        }
        return 1;
    }
    
    /// <summary>
    /// 次のレベルまでの経験値を取得
    /// </summary>
    public int GetExpToNextLevel(Guild guild)
    {
        int currentLevel = GetGuildLevel(guild);
        if (currentLevel >= GuildExpTable.Length)
            return 0; // 最大レベル
        
        return GuildExpTable[currentLevel] - guild.TotalExp;
    }
    
    /// <summary>
    /// ギルドに経験値を追加
    /// </summary>
    public GuildExpResult AddGuildExp(Guild guild, int exp)
    {
        if (guild == null)
            return new GuildExpResult { Success = false, Message = "ギルドが見つかりません" };
        
        int oldLevel = GetGuildLevel(guild);
        guild.TotalExp += exp;
        int newLevel = GetGuildLevel(guild);
        
        bool leveledUp = newLevel > oldLevel;
        
        return new GuildExpResult
        {
            Success = true,
            Message = leveledUp 
                ? $"ギルド経験値 +{exp}！ギルドレベルが{oldLevel}から{newLevel}に上がりました！" 
                : $"ギルド経験値 +{exp}",
            ExpGained = exp,
            OldLevel = oldLevel,
            NewLevel = newLevel,
            LeveledUp = leveledUp,
            ExpToNextLevel = GetExpToNextLevel(guild)
        };
    }
    
    /// <summary>
    /// 習得可能なスキル一覧を取得
    /// </summary>
    public List<GuildSkill> GetAvailableSkills(Guild guild)
    {
        int level = GetGuildLevel(guild);
        return GuildSkills.Where(s => s.RequiredLevel <= level).ToList();
    }
    
    /// <summary>
    /// ギルドスキルを追加
    /// </summary>
    public GuildSkillResult AddGuildSkill(Guild guild, int skillId)
    {
        if (guild == null)
            return new GuildSkillResult { Success = false, Message = "ギルドが見つかりません" };
        
        var skill = GuildSkills.FirstOrDefault(s => s.Id == skillId);
        if (skill == null)
            return new GuildSkillResult { Success = false, Message = "スキルが見つかりません" };
        
        int level = GetGuildLevel(guild);
        if (skill.RequiredLevel > level)
            return new GuildSkillResult { Success = false, Message = $"ギルドレベル{skill.RequiredLevel}が必要です" };
        
        // スキルを追加（既に習得済みかチェック）
        if (guild.LearnableSkills == null)
            guild.LearnableSkills = new List<int>();
        
        if (guild.LearnableSkills.Contains(skillId))
            return new GuildSkillResult { Success = false, Message = "既に習得済みです" };
        
        // スキル習得（経験値が必要）
        int skillCost = skill.RequiredLevel * 1000;
        if (guild.TotalExp < skillCost)
            return new GuildSkillResult { Success = false, Message = $"スキル習得に{skillCost}経験値が必要です" };
        
        guild.TotalExp -= skillCost;
        guild.LearnableSkills.Add(skillId);
        
        return new GuildSkillResult
        {
            Success = true,
            Message = $"{skill.Name}を習得しました！",
            Skill = skill
        };
    }
    
    /// <summary>
    /// ギルドメンバーの効果を計算（スキルBonus）
    /// </summary>
    public GuildBonus CalculateMemberBonus(Guild guild, string bonusType)
    {
        if (guild?.LearnableSkills == null || guild.LearnableSkills.Count == 0)
            return new GuildBonus { BonusType = bonusType, Value = 0 };
        
        var skills = GuildSkills.Where(s => guild.LearnableSkills.Contains(s.Id) && s.EffectType == bonusType).ToList();
        
        if (skills.Count == 0)
            return new GuildBonus { BonusType = bonusType, Value = 0 };
        
        int totalValue = skills.Sum(s => s.EffectValue);
        
        return new GuildBonus
        {
            BonusType = bonusType,
            Value = totalValue,
            Description = string.Join(", ", skills.Select(s => s.Name))
        };
    }
    
    /// <summary>
    /// ギルド戦の結果を計算
    /// </summary>
    public GuildWarResult CalculateGuildWarResult(Guild attackerGuild, Guild defenderGuild, int attackerAttack, int defenderDefense)
    {
        // ギルドスキルによる加成
        int attackBonus = CalculateMemberBonus(attackerGuild, "GuildWarAttack").Value;
        int defenseBonus = CalculateMemberBonus(defenderGuild, "GuildWarAttack").Value;
        
        double attackMultiplier = 1.0 + (attackBonus / 100.0);
        double defenseMultiplier = 1.0 + (defenseBonus / 100.0);
        
        int finalAttack = (int)(attackerAttack * attackMultiplier);
        int finalDefense = (int)(defenderDefense * defenseMultiplier);
        
        bool attackerWins = finalAttack > finalDefense;
        
        // 勝利すると経験値獲得
        int expGained = 0;
        if (attackerWins)
        {
            expGained = 500 + (defenderGuild.MemberCount * 100);
            AddGuildExp(attackerGuild, expGained);
        }
        
        return new GuildWarResult
        {
            AttackerWins = attackerWins,
            AttackerFinalAttack = finalAttack,
            DefenderFinalDefense = finalDefense,
            ExpGained = expGained,
            Message = attackerWins 
                ? $"勝利！ギルド経験値 +{expGained}" 
                : "敗北..."
        };
    }
    
    /// <summary>
    /// ギルド情報を取得（拡張版）
    /// </summary>
    public GuildInfo GetGuildInfo(Guild guild)
    {
        if (guild == null)
            return new GuildInfo { ErrorMessage = "ギルドが見つかりません" };
        
        int level = GetGuildLevel(guild);
        var availableSkills = GetAvailableSkills(guild);
        var learnedSkills = GuildSkills.Where(s => guild.LearnableSkills?.Contains(s.Id) == true).ToList();
        
        return new GuildInfo
        {
            Name = guild.Name,
            LeaderName = guild.LeaderName,
            MemberCount = guild.MemberCount,
            TotalExp = guild.TotalExp,
            Level = level,
            ExpToNextLevel = GetExpToNextLevel(guild),
            AvailableSkills = availableSkills,
            LearnedSkills = learnedSkills,
            IsMaxLevel = level >= GuildExpTable.Length
        };
    }
}

/// <summary>
/// ギルド経験値結果
/// </summary>
public class GuildExpResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int ExpGained { get; set; }
    public int OldLevel { get; set; }
    public int NewLevel { get; set; }
    public bool LeveledUp { get; set; }
    public int ExpToNextLevel { get; set; }
}

/// <summary>
/// ギルドスキル
/// </summary>
public class GuildSkill
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string EffectType { get; set; } = "";
    public int EffectValue { get; set; }
    public int RequiredLevel { get; set; }
}

/// <summary>
/// ギルドスキル結果
/// </summary>
public class GuildSkillResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public GuildSkill? Skill { get; set; }
}

/// <summary>
/// ギルドBonus
/// </summary>
public class GuildBonus
{
    public string BonusType { get; set; } = "";
    public int Value { get; set; }
    public string Description { get; set; } = "";
}

/// <summary>
/// ギルド戦結果
/// </summary>
public class GuildWarResult
{
    public bool AttackerWins { get; set; }
    public int AttackerFinalAttack { get; set; }
    public int DefenderFinalDefense { get; set; }
    public int ExpGained { get; set; }
    public string Message { get; set; } = "";
}

/// <summary>
/// ギルド情報（拡張版）
/// </summary>
public class GuildInfo
{
    public string Name { get; set; } = "";
    public string LeaderName { get; set; } = "";
    public int MemberCount { get; set; }
    public int TotalExp { get; set; }
    public int Level { get; set; }
    public int ExpToNextLevel { get; set; }
    public List<GuildSkill> AvailableSkills { get; set; } = new();
    public List<GuildSkill> LearnedSkills { get; set; } = new();
    public bool IsMaxLevel { get; set; }
    public string ErrorMessage { get; set; } = "";
}
