namespace FFA.Models;

/// <summary>
/// モンスターの攻撃スキル
/// </summary>
public class MonsterSkill
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string JapaneseName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "💢";
    
    // スキルタイプ
    public MonsterSkillType Type { get; set; } = MonsterSkillType.Physical;
    
    // 基礎威力（攻撃力の何倍か）
    public double BasePower { get; set; } = 1.0;
    
    // 固定ダメージ
    public int FixedDamage { get; set; } = 0;
    
    // MPコスト（使用条件）
    public int MpCost { get; set; } = 0;
    
    // 成功率（%）
    public int SuccessRate { get; set; } = 100;
    
    // 追加効果
    public MonsterSkillEffect Effect { get; set; } = new();
    
    // ターゲット（単体/複数/自身）
    public SkillTarget Target { get; set; } = SkillTarget.SingleEnemy;
    
    // CT（ターン数後に使用可能）
    public int CooldownTurns { get; set; } = 0;
    
    // 使用確率（%）
    public int UsageRate { get; set; } = 20;
}

/// <summary>
/// モンスターのスキルタイプ
/// </summary>
public enum MonsterSkillType
{
    Physical,     // 物理攻撃
    Magical,       // 魔法攻撃
    Heal,         // 回復
    Buff,         // 強化
    Debuff,       // 弱体化
    Special       // 特殊
}

/// <summary>
/// スキルターゲット
/// </summary>
public enum SkillTarget
{
    SingleEnemy,  // 敵単体
    AllEnemies,  // 敵全体
    Self,         // 自身
    SelfAlly,     // 味方の自身
    RandomEnemy   // ランダム敵
}

/// <summary>
/// スキル追加効果
/// </summary>
public class MonsterSkillEffect
{
    // 状態異常
    public StatusEffectType? StatusEffect { get; set; }
    public int StatusEffectRate { get; set; } = 0; // 状態異常付与率%
    
    // 回復量
    public int HealAmount { get; set; } = 0;
    
    // 防御力DOWN
    public int DefenseDown { get; set; } = 0;
    public int DefenseDownTurns { get; set; } = 0;
    
    // 攻撃力DOWN
    public int AttackDown { get; set; } = 0;
    public int AttackDownTurns { get; set; } = 0;
    
    // 毒
    public int PoisonDamage { get; set; } = 0;
    
    // 睡眠
    public int SleepTurns { get; set; } = 0;
    
    // 沈黙
    public int SilenceTurns { get; set; } = 0;
    
    // 石化
    public int PetrifyTurns { get; set; } = 0;
    
    // 麻痺
    public int ParalysisTurns { get; set; } = 0;
    
    // 怯え
    public int FearTurns { get; set; } = 0;
    
    // 気絶
    public int StunTurns { get; set; } = 0;
    
    // 猛毒
    public int ToxicDamage { get; set; } = 0;
    
    // 暗闇
    public int BlindTurns { get; set; } = 0;
    
    // 沉默
    public int MuteTurns { get; set; } = 0;
    
    // 呪い
    public int CurseTurns { get; set; } = 0;
    
    // ドロップ率UP
    public int DropRateBonus { get; set; } = 0;
}

/// <summary>
/// 状態異常タイプ
/// </summary>
public enum StatusEffectType
{
    None,
    Poison,       // 毒
    Sleep,        // 睡眠
    Silence,      // 沈黙
    Petrify,      // 石化
    Paralysis,    // 麻痺
    Fear,         // 怯え
    Stun,         // 気絶
    Blind,        // 暗闘
    Curse,        // 呪い
    Toxic,        // 猛毒
    Burn,         // 炎上
    Freeze,       // 凍結
    Shock,        // 感電
    Slow,         // スロウ
    Stop,         // 停止
    Regen,        // リegen（回復状態）
    Haste         // ヘイスト
}

/// <summary>
/// モンスターのスキルセット（1体のモンスターが持つスキルのリスト）
/// </summary>
public class MonsterSkillSet
{
    public string MonsterId { get; set; } = "";
    public List<MonsterSkill> Skills { get; set; } = new();
}
