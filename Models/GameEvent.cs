namespace FFA.Models;

/// <summary>
/// ゲームイベント Types
/// </summary>
public enum GameEventType
{
    ItemAcquired,        // アイテム取得
    ItemUsed,            // アイテム使用
    ItemCrafted,         // アイテム製作
    ItemDisassembled,    // アイテム分解
    ItemSold,            // アイテム売却
    MiningSuccess,       // 採掘成功
    MiningFailed,        // 採掘失敗
    BattleWon,           // 戦闘勝利
    BattleLost,          // 戦闘敗北
    QuestStarted,        // クエスト開始
    QuestCompleted,      // クエスト完了
    QuestFailed,         // クエスト失敗
    LevelUp,             // レベルアップ
    Rebirth,             // 転生
    JobChange,           // ジョブ変更
    AchievementUnlocked,  // 実績解除
    GuildJoined,         // ギルド参加
    CountryJoined,       // 国参加
    DuelWon,             // 決闘勝利
    DuelLost,            // 決闘敗北
    ItemEnhanced,         // アイテム強化成功
    ItemEnhancedFailed,   // アイテム強化失敗
    ItemRepaired,        // アイテム修理
    EnteredDungeon,      // ダンジョン入場
    EnteredField,        // フィールド入场
    Trading,             // 取引
    BankDeposit,         // 銀行入金
    BankWithdrawal,      // 銀行引き出し
    PrizeWon,            // 賞品獲得
    PenaltyReceived      // ペナルティ受領
}

/// <summary>
/// ゲームイベント Model
/// </summary>
public class GameEvent
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public GameEventType EventType { get; set; }
    public string Description { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // 関連データ
    public string? ItemName { get; set; }
    public int? ItemId { get; set; }
    public int? Quantity { get; set; }
    public int? GoldAmount { get; set; }
    public int? ExperienceGained { get; set; }
    public int? Level { get; set; }
    public string? Location { get; set; }
    public string? EnemyName { get; set; }
}

/// <summary>
/// システム統合データ
/// </summary>
public class SystemIntegration
{
    // 装備によるステータス加成
    public static int CalculateEquipmentBonus(User user)
    {
        int bonus = 0;
        
        if (user.EquippedWeapon != null)
        {
            bonus += user.EquippedWeapon.Attack;
            bonus += user.EquippedWeapon.EnhancementLevel * 2;
        }
        
        if (user.EquippedArmor != null)
        {
            bonus += user.EquippedArmor.Defense;
            bonus += user.EquippedArmor.EnhancementLevel * 2;
        }
        
        if (user.EquippedAccessory1 != null)
        {
            bonus += user.EquippedAccessory1.EnhancementLevel;
        }
        
        if (user.EquippedAccessory2 != null)
        {
            bonus += user.EquippedAccessory2.EnhancementLevel;
        }
        
        return bonus;
    }
    
    // レアリティによるドロップ倍率
    public static double GetRarityMultiplier(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.White => 1.0,
            Rarity.Purple => 2.0,
            Rarity.Red => 3.0,
            Rarity.Orange => 5.0,
            Rarity.Gold => 10.0,
            Rarity.Rainbow => 20.0,
            _ => 1.0
        };
    }
    
    // ジョブによるスキル倍率
    public static double GetJobSkillMultiplier(Job job)
    {
        return job switch
        {
            Job.Warrior => 1.2,
            Job.WhiteMage => 1.1,
            Job.BlackMage => 1.3,
            Job.Ranger => 1.15,
            Job.Thief => 1.25,
            Job.Paladin => 1.2,
            Job.DarkKnight => 1.25,
            Job.Bard => 1.1,
            Job.Monk => 1.3,
            Job.Ninja => 1.25,
            Job.HolyKnight => 1.25,
            Job.DeathKnight => 1.3,
            Job.ArchMage => 1.4,
            Job.BeastMaster => 1.2,
            Job.Duelist => 1.3,
            Job.Grandmaster => 1.5,
            _ => 1.0
        };
    }
    
    // 強化成功率の計算
    public static double CalculateEnhancementSuccessRate(int currentLevel, bool isSafeEnhancement)
    {
        double baseRate = 0.8;
        
        // レベルが上がるごとに成功率が下がる
        double levelPenalty = currentLevel * 0.05;
        double rate = baseRate - levelPenalty;
        
        // 安全強化なら100%
        if (isSafeEnhancement)
        {
            rate = 1.0;
        }
        
        // 最低でも10%の成功率
        return Math.Max(0.1, rate);
    }
    
    // 修理コストの計算
    public static int CalculateRepairCost(int maxDurability, int currentDurability, int baseCost)
    {
        int missing = maxDurability - currentDurability;
        double durabilityPercent = (double)missing / maxDurability;
        return (int)(baseCost * durabilityPercent);
    }
    
    // アイテムの適正価格計算
    public static int CalculateItemPrice(int basePrice, Rarity rarity, int enhancementLevel)
    {
        double rarityMultiplier = GetRarityMultiplier(rarity);
        double enhancementMultiplier = 1.0 + (enhancementLevel * 0.1);
        return (int)(basePrice * rarityMultiplier * enhancementMultiplier);
    }
}
