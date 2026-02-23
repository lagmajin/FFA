namespace FFA.Models;

/// <summary>
/// 仲間のレアリティ
/// </summary>
public enum CompanionRarity 
{ 
    Common,     // 一般傭兵
    Uncommon,   // 熟練傭兵
    Rare,       // ベテラン傭兵
    Epic,       // ユニーク
    Legendary   // レジェンダリー
}

/// <summary>
/// 仲間のタイプ
/// </summary>
public enum CompanionType
{
    Mercenary,      // 傭兵（汎用）
    Unique,         // ユニークキャラクター
    Special         // 特殊（イベント等）
}

/// <summary>
/// 仲間の役割
/// </summary>
public enum CompanionRole
{
    Attacker,   // 攻撃特化
    Defender,   // 防御特化
    Healer,     // 回復支援
    Support,    // バフ/デバフ
    Balanced    // バランス型
}

/// <summary>
/// 仲間モデル（パーティシステム基盤）
/// </summary>
public class Companion
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerUsername { get; set; } = string.Empty;
    public CompanionRarity Rarity { get; set; } = CompanionRarity.Common;
    public CompanionType Type { get; set; } = CompanionType.Mercenary;
    public CompanionRole Role { get; set; } = CompanionRole.Balanced;
    
    // 職業（傭兵用）
    public Job? Job { get; set; }
    public string? JobName { get; set; }
    
    // ユニーク用
    public bool IsUnique { get; set; } = false;
    public string? UniqueTitle { get; set; } // 「剣聖」「魔導王」など
    
    // レベル・経験値
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int ExpToNextLevel { get; set; } = 100;
    public int MaxLevel { get; set; } = 50;
    
    // 基本ステータス
    public int BaseHP { get; set; } = 100;
    public int BaseAttack { get; set; } = 10;
    public int BaseDefense { get; set; } = 5;
    public int BaseMagic { get; set; } = 5;
    public int BaseSpeed { get; set; } = 10;
    
    // 現在のステータス（レベル補正込み）
    public int CurrentHP { get; set; } = 100;
    public int MaxHP => CalculateMaxHP();
    public int Attack => CalculateAttack();
    public int Defense => CalculateDefense();
    public int Magic => CalculateMagic();
    public int Speed => CalculateSpeed();
    
    // 親密度・信頼度
    public int Affection { get; set; } = 50; // 0-100
    public int Trust { get; set; } = 0; // 0-100
    
    // 雇用コスト
    public int HireCost { get; set; } = 100; // 雇用費用
    public int DailyWage { get; set; } = 10; // 日当
    public int ContractDays { get; set; } = 30; // 契約期間（日）
    public DateTime HiredAt { get; set; } = DateTime.UtcNow;
    
    // 装備スロット
    public Weapon? EquippedWeapon { get; set; }
    public Armor? EquippedArmor { get; set; }
    public Accessory? EquippedAccessory { get; set; }
    
    // スキル
    public List<CompanionSkill> Skills { get; set; } = new();
    public int MaxSkills { get; set; } = 4;
    
    // ユニークスキル（ユニークキャラクターのみ）
    public CompanionSkill? UniqueSkill { get; set; }
    
    // パーティ関連
    public bool IsInParty { get; set; } = false;
    public int PartyPosition { get; set; } = -1; // -1 = パーティ外
    
    // 召喚状態
    public bool IsSummoned { get; set; } = false;
    
    // 見た目
    public string Icon { get; set; } = "👤";
    public string Color { get; set; } = "#4CAF50";
    
    // 説明
    public string? Description { get; set; }
    
    // 取得日時
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
    
    // ステータス計算メソッド
    private int CalculateMaxHP()
    {
        var baseVal = BaseHP + (Level - 1) * 10;
        var rarityBonus = GetRarityBonus();
        var jobBonus = GetJobHPBonus();
        return (int)(baseVal * rarityBonus + jobBonus);
    }
    
    private int CalculateAttack()
    {
        var baseVal = BaseAttack + (Level - 1) * 2;
        var rarityBonus = GetRarityBonus();
        var weaponBonus = EquippedWeapon?.Attack ?? 0;
        var jobBonus = GetJobAttackBonus();
        return (int)((baseVal + weaponBonus + jobBonus) * rarityBonus);
    }
    
    private int CalculateDefense()
    {
        var baseVal = BaseDefense + (Level - 1) * 1;
        var rarityBonus = GetRarityBonus();
        var armorBonus = EquippedArmor?.Defense ?? 0;
        var jobBonus = GetJobDefenseBonus();
        return (int)((baseVal + armorBonus + jobBonus) * rarityBonus);
    }
    
    private int CalculateMagic()
    {
        var baseVal = BaseMagic + (Level - 1) * 1;
        var rarityBonus = GetRarityBonus();
        var jobBonus = GetJobMagicBonus();
        return (int)((baseVal + jobBonus) * rarityBonus);
    }
    
    private int CalculateSpeed()
    {
        var baseVal = BaseSpeed + (Level - 1);
        var rarityBonus = GetRarityBonus();
        var jobBonus = GetJobSpeedBonus();
        return (int)((baseVal + jobBonus) * rarityBonus);
    }
    
    private double GetRarityBonus()
    {
        return Rarity switch
        {
            CompanionRarity.Common => 1.0,
            CompanionRarity.Uncommon => 1.1,
            CompanionRarity.Rare => 1.25,
            CompanionRarity.Epic => 1.5,
            CompanionRarity.Legendary => 2.0,
            _ => 1.0
        };
    }
    
    // 職業ボーナス
    private int GetJobHPBonus() => Job switch
    {
        FFA.Models.Job.Warrior => 20,
        FFA.Models.Job.Paladin => 30,
        FFA.Models.Job.Monk => 15,
        _ => 0
    };
    
    private int GetJobAttackBonus() => Job switch
    {
        FFA.Models.Job.Warrior => 5,
        FFA.Models.Job.DarkKnight => 8,
        FFA.Models.Job.Thief => 3,
        FFA.Models.Job.Ninja => 4,
        _ => 0
    };
    
    private int GetJobDefenseBonus() => Job switch
    {
        FFA.Models.Job.Paladin => 8,
        FFA.Models.Job.Warrior => 3,
        _ => 0
    };
    
    private int GetJobMagicBonus() => Job switch
    {
        FFA.Models.Job.BlackMage => 10,
        FFA.Models.Job.WhiteMage => 8,
        _ => 0
    };
    
    private int GetJobSpeedBonus() => Job switch
    {
        FFA.Models.Job.Thief => 8,
        FFA.Models.Job.Ninja => 10,
        FFA.Models.Job.Ranger => 5,
        _ => 0
    };
    
    /// <summary>
    /// 経験値を追加してレベルアップチェック
    /// </summary>
    public bool AddExperience(int exp)
    {
        Experience += exp;
        bool leveledUp = false;
        
        while (Experience >= ExpToNextLevel && Level < MaxLevel)
        {
            Experience -= ExpToNextLevel;
            Level++;
            ExpToNextLevel = (int)(ExpToNextLevel * 1.5);
            leveledUp = true;
            
            // レベルアップ時にHP全回復
            CurrentHP = MaxHP;
        }
        
        return leveledUp;
    }
    
    /// <summary>
    /// 親密度を上げる
    /// </summary>
    public void IncreaseAffection(int amount)
    {
        Affection = Math.Min(100, Affection + amount);
        
        // 親密度が高いと信頼度も上昇
        if (Affection >= 80 && Trust < 50)
        {
            Trust = Math.Min(100, Trust + 1);
        }
    }
}

/// <summary>
/// 仲間のスキル
/// </summary>
public class CompanionSkill
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public CompanionSkillType Type { get; set; } = CompanionSkillType.Active;
    public int Power { get; set; } = 10;
    public int MPCost { get; set; } = 5;
    public int Cooldown { get; set; } = 0;
    public int CurrentCooldown { get; set; } = 0;
    public int RequiredLevel { get; set; } = 1;
    public string Icon { get; set; } = "⚔️";
    public bool IsUniqueSkill { get; set; } = false;
}

/// <summary>
/// 仲間スキルタイプ
/// </summary>
public enum CompanionSkillType
{
    Active,     // アクティブスキル
    Passive,    // パッシブスキル
    Reaction    // 反応スキル
}

/// <summary>
/// パーティ情報
/// </summary>
public class Party
{
    public string OwnerUsername { get; set; } = "";
    public List<int> CompanionIds { get; set; } = new();
    public int MaxPartySize { get; set; } = 3;
    
    /// <summary>
    /// パーティに仲間を追加
    /// </summary>
    public bool AddCompanion(int companionId)
    {
        if (CompanionIds.Count >= MaxPartySize) return false;
        if (CompanionIds.Contains(companionId)) return false;
        
        CompanionIds.Add(companionId);
        return true;
    }
    
    /// <summary>
    /// パーティから仲間を削除
    /// </summary>
    public bool RemoveCompanion(int companionId)
    {
        return CompanionIds.Remove(companionId);
    }
    
    /// <summary>
    /// パーティメンバーの位置を入れ替え
    /// </summary>
    public void SwapPositions(int index1, int index2)
    {
        if (index1 < 0 || index1 >= CompanionIds.Count) return;
        if (index2 < 0 || index2 >= CompanionIds.Count) return;
        
        (CompanionIds[index1], CompanionIds[index2]) = (CompanionIds[index2], CompanionIds[index1]);
    }
}

/// <summary>
/// 傭兵テンプレート（汎用傭兵生成用）
/// </summary>
public class MercenaryTemplate
{
    public Job Job { get; set; } = Job.Warrior;
    public string NamePrefix { get; set; } = ""; // 「若き」「熟練の」など
    public CompanionRarity Rarity { get; set; } = CompanionRarity.Common;
    public int BaseHP { get; set; } = 100;
    public int BaseAttack { get; set; } = 10;
    public int BaseDefense { get; set; } = 5;
    public int BaseMagic { get; set; } = 5;
    public int BaseSpeed { get; set; } = 10;
    public int HireCost { get; set; } = 100;
    public int DailyWage { get; set; } = 10;
    public string Icon { get; set; } = "👤";
    public string Color { get; set; } = "#4CAF50";
    public List<string> DefaultSkills { get; set; } = new();
}

/// <summary>
/// ユニークキャラクターテンプレート
/// </summary>
public class UniqueCharacterTemplate
{
    public string Name { get; set; } = "";
    public string Title { get; set; } = ""; // 「剣聖」「魔導王」など
    public Job Job { get; set; } = Job.Warrior;
    public CompanionRarity Rarity { get; set; } = CompanionRarity.Epic;
    public int BaseHP { get; set; } = 150;
    public int BaseAttack { get; set; } = 20;
    public int BaseDefense { get; set; } = 10;
    public int BaseMagic { get; set; } = 10;
    public int BaseSpeed { get; set; } = 15;
    public int HireCost { get; set; } = 10000;
    public int DailyWage { get; set; } = 100;
    public string Icon { get; set; } = "⭐";
    public string Color { get; set; } = "#FFD700";
    public string Description { get; set; } = "";
    public List<string> DefaultSkills { get; set; } = new();
    public string UniqueSkillName { get; set; } = "";
    public string UniqueSkillDescription { get; set; } = "";
    public int UniqueSkillPower { get; set; } = 50;
}
