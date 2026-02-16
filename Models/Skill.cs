namespace FFA.Models;

/// <summary>
/// スキルの定義
/// </summary>
public class Skill
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "✨";
    public Job RequiredJob { get; set; }
    public int Tier { get; set; } = 1; // 1-5 (5が最上位)
    public int MaxLevel { get; set; } = 1;
    public SkillType Type { get; set; } = SkillType.Passive;
    public SkillEffectType EffectType { get; set; } = SkillEffectType.None;
    public int EffectValue { get; set; } = 0;
    public int SkillPointCost { get; set; } = 1;
    public string? ParentSkillId { get; set; } // 前提スキル（ツリー構造）
    public int RequiredPlayerLevel { get; set; } = 1;
}

public enum SkillType
{
    Passive,
    Active,
    Ultimate
}

public enum SkillEffectType
{
    None,
    StrBonus,
    DexBonus,
    IntBonus,
    VitBonus,
    HpBonus,
    MpBonus,
    AtkBonus,
    DefBonus,
    ExpBonus,
    GilBonus,
    CriticalRate,
    Evasion
}
