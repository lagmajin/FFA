using FFA.Models;

namespace FFA.Services;

/// <summary>
/// 戦闘以外のパッシブスキルサービス
/// </summary>
public class NonCombatPassiveService
{
    /// <summary>
    /// プレイヤーの戦闘外パッシブスキルを取得
    /// </summary>
    public List<NonCombatPassiveSkill> GetPlayerPassiveSkills(User user)
    {
        if (user == null) return new List<NonCombatPassiveSkill>();
        var jobInfo = JobDatabase.GetJobInfo(user.Job);
        return jobInfo.NonCombatPassiveSkills ?? new List<NonCombatPassiveSkill>();
    }
    
    /// <summary>
    /// 特定タイプのパッシブスキル効果値を取得
    /// </summary>
    public double GetPassiveBonus(User user, NonCombatPassiveType type)
    {
        var skills = GetPlayerPassiveSkills(user);
        var skill = skills.FirstOrDefault(s => s.Type == type);
        return skill?.Value ?? 0;
    }
    
    /// <summary>
    /// 全てのパッシブスキル効果を辞書で取得
    /// </summary>
    public Dictionary<NonCombatPassiveType, double> GetAllPassiveBonuses(User user)
    {
        var skills = GetPlayerPassiveSkills(user);
        return skills.ToDictionary(s => s.Type, s => s.Value);
    }
    
    #region 採掘系
    
    /// <summary>
    /// 採掘速度ボーナスを取得（パーセント）
    /// </summary>
    public double GetMiningSpeedBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.MiningSpeedBonus);
    }
    
    /// <summary>
    /// レア鉱石発見率ボーナスを取得（パーセント）
    /// </summary>
    public double GetMiningRareFindBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.MiningRareFindBonus);
    }
    
    /// <summary>
    /// 採掘スタミナ消費減少を取得（パーセント）
    /// </summary>
    public double GetMiningStaminaReduce(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.MiningStaminaReduce);
    }
    
    #endregion
    
    #region 釣り系
    
    /// <summary>
    /// 釣り速度ボーナスを取得（パーセント）
    /// </summary>
    public double GetFishingSpeedBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.FishingSpeedBonus);
    }
    
    /// <summary>
    /// レア魚発見率ボーナスを取得（パーセント）
    /// </summary>
    public double GetFishingRareFindBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.FishingRareFindBonus);
    }
    
    /// <summary>
    /// 魚の逃走率減少を取得（パーセント）
    /// </summary>
    public double GetFishingEscapeReduce(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.FishingEscapeReduce);
    }
    
    #endregion
    
    #region 探索系
    
    /// <summary>
    /// 探索速度ボーナスを取得（パーセント）
    /// </summary>
    public double GetExplorationSpeedBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.ExplorationSpeedBonus);
    }
    
    /// <summary>
    /// 発見率ボーナスを取得（パーセント）
    /// </summary>
    public double GetExplorationFindBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.ExplorationFindBonus);
    }
    
    /// <summary>
    /// エンカウント率調整を取得（パーセント、負の値で減少）
    /// </summary>
    public double GetEncounterRateAdjust(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.EncounterRateAdjust);
    }
    
    #endregion
    
    #region クラフト系
    
    /// <summary>
    /// クラフト成功率ボーナスを取得（パーセント）
    /// </summary>
    public double GetCraftingSuccessBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.CraftingSuccessBonus);
    }
    
    /// <summary>
    /// 品質ボーナスを取得（パーセント）
    /// </summary>
    public double GetCraftingQualityBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.CraftingQualityBonus);
    }
    
    /// <summary>
    /// 材料節約を取得（パーセント）
    /// </summary>
    public double GetCraftingMaterialSave(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.CraftingMaterialSave);
    }
    
    #endregion
    
    #region 経済系
    
    /// <summary>
    /// 売却価格ボーナスを取得（パーセント）
    /// </summary>
    public double GetShopSellPriceBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.ShopSellPriceBonus);
    }
    
    /// <summary>
    /// 購入価格割引を取得（パーセント）
    /// </summary>
    public double GetShopBuyPriceDiscount(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.ShopBuyPriceDiscount);
    }
    
    /// <summary>
    /// 銀行金利ボーナスを取得（パーセント）
    /// </summary>
    public double GetBankInterestBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.BankInterestBonus);
    }
    
    #endregion
    
    #region 回復系
    
    /// <summary>
    /// HP自然回復ボーナスを取得（パーセント）
    /// </summary>
    public double GetHPRegenBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.HPRegenBonus);
    }
    
    /// <summary>
    /// MP自然回復ボーナスを取得（パーセント）
    /// </summary>
    public double GetMPRegenBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.MPRegenBonus);
    }
    
    /// <summary>
    /// 状態異常回復ボーナスを取得（パーセント）
    /// </summary>
    public double GetStatusRecoveryBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.StatusRecoveryBonus);
    }
    
    #endregion
    
    #region その他
    
    /// <summary>
    /// ドロップ率ボーナスを取得（パーセント）
    /// </summary>
    public double GetDropRateBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.DropRateBonus);
    }
    
    /// <summary>
    /// 経験値ボーナスを取得（パーセント）
    /// </summary>
    public double GetExperienceBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.ExperienceBonus);
    }
    
    /// <summary>
    /// ゴールドボーナスを取得（パーセント）
    /// </summary>
    public double GetGoldBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.GoldBonus);
    }
    
    /// <summary>
    /// 移動速度ボーナスを取得（パーセント）
    /// </summary>
    public double GetTravelSpeedBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.TravelSpeedBonus);
    }
    
    /// <summary>
    /// スタミナ回復ボーナスを取得（パーセント）
    /// </summary>
    public double GetStaminaRegenBonus(User user)
    {
        return GetPassiveBonus(user, NonCombatPassiveType.StaminaRegenBonus);
    }
    
    #endregion
    
    #region 計算適用メソッド
    
    /// <summary>
    /// ボーナス適用後の値を計算
    /// </summary>
    public double ApplyBonus(double baseValue, double bonusPercent)
    {
        return baseValue * (1 + bonusPercent / 100.0);
    }
    
    /// <summary>
    /// 割引適用後の値を計算
    /// </summary>
    public double ApplyDiscount(double baseValue, double discountPercent)
    {
        return baseValue * (1 - discountPercent / 100.0);
    }
    
    /// <summary>
    /// 売却価格を計算（ボーナス適用）
    /// </summary>
    public long CalculateSellPrice(User user, long basePrice)
    {
        var bonus = GetShopSellPriceBonus(user);
        return (long)ApplyBonus(basePrice, bonus);
    }
    
    /// <summary>
    /// 購入価格を計算（割引適用）
    /// </summary>
    public long CalculateBuyPrice(User user, long basePrice)
    {
        var discount = GetShopBuyPriceDiscount(user);
        return (long)ApplyDiscount(basePrice, discount);
    }
    
    /// <summary>
    /// 経験値を計算（ボーナス適用）
    /// </summary>
    public long CalculateExperience(User user, long baseExp)
    {
        var bonus = GetExperienceBonus(user);
        return (long)ApplyBonus(baseExp, bonus);
    }
    
    /// <summary>
    /// ゴールドを計算（ボーナス適用）
    /// </summary>
    public long CalculateGold(User user, long baseGold)
    {
        var bonus = GetGoldBonus(user);
        return (long)ApplyBonus(baseGold, bonus);
    }
    
    /// <summary>
    /// ドロップ判定（ボーナス適用）
    /// </summary>
    public bool RollDrop(User user, double baseDropRate, Random? random = null)
    {
        random ??= new Random();
        var bonus = GetDropRateBonus(user);
        var adjustedRate = baseDropRate * (1 + bonus / 100.0);
        return random.NextDouble() * 100 < adjustedRate;
    }
    
    /// <summary>
    /// HP自然回復量を計算
    /// </summary>
    public int CalculateHPRegen(User user, int baseRegen)
    {
        var bonus = GetHPRegenBonus(user);
        return (int)ApplyBonus(baseRegen, bonus);
    }
    
    /// <summary>
    /// MP自然回復量を計算
    /// </summary>
    public int CalculateMPRegen(User user, int baseRegen)
    {
        var bonus = GetMPRegenBonus(user);
        return (int)ApplyBonus(baseRegen, bonus);
    }
    
    /// <summary>
    /// 移動時間を計算（ボーナス適用）
    /// </summary>
    public TimeSpan CalculateTravelTime(User user, TimeSpan baseTime)
    {
        var bonus = GetTravelSpeedBonus(user);
        return TimeSpan.FromMilliseconds(baseTime.TotalMilliseconds / (1 + bonus / 100.0));
    }
    
    #endregion
}
