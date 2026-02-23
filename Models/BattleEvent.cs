using System;
using System.Collections.Generic;

namespace FFA.Models;

/// <summary>
/// バトルイベントタイプ
/// </summary>
public enum BattleEventType
{
    // ポジティブイベント
    TreasureChest,      // 宝箱出現
    CriticalRush,       // クリティカルラッシュ
    GilBonus,           // ギルボーナス
    ExpBonus,           // 経験値ボーナス
    HealSpring,         // 回復の泉
    PowerSurge,         // 攻撃力上昇
    DefenseAura,        // 防御力上昇
    
    // ネガティブイベント
    EnemyReinforcement, // 敵の援軍
    PoisonFog,          // 毒の霧
    ParalysisGas,       // 麻痺ガス
    SleepSpore,         // 睡眠胞子
    ConfusionMist,      // 混乱の霧
    EnemyRage,          // 敵の狂乱
    Darkness,           // 暗闇
    
    // 特殊イベント
    RareMonster,        // レアモンスター出現
    GoldenEnemy,        // ゴールデンエネミー
    DoubleDrop,         // ドロップ2倍
    SkillSeal,          // スキル封印
    MagicBoost          // 魔法強化
}

/// <summary>
/// バトルイベント
/// </summary>
public class BattleEvent
{
    public int Id { get; set; }
    public BattleEventType Type { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "🎲";
    
    // イベント効果
    public bool IsPositive { get; set; }
    public int Duration { get; set; } = 0;          // 効果持続ターン数（0=即時）
    public int TriggerChance { get; set; } = 10;    // 発生確率（%）
    
    // 効果値
    public double Value { get; set; } = 0;          // 効果の数値（ダメージ、回復量、倍率など）
    public int ValuePercent { get; set; } = 0;      // パーセンテージ効果
    
    // メッセージ
    public string TriggerMessage { get; set; } = "";
    public string EndMessage { get; set; } = "";
    
    // 条件
    public int MinTurn { get; set; } = 1;           // 発生可能な最小ターン
    public int MaxTurn { get; set; } = 999;         // 発生可能な最大ターン
    public string? RequiredLocation { get; set; }   // 特定の場所でのみ発生
}

/// <summary>
/// 発生中のバトルイベント
/// </summary>
public class ActiveBattleEvent
{
    public BattleEvent Event { get; set; } = null!;
    public int RemainingTurns { get; set; }
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public bool IsExpired => RemainingTurns <= 0;
}

/// <summary>
/// バトルイベントデータベース
/// </summary>
public static class BattleEventDatabase
{
    public static List<BattleEvent> Events { get; } = new List<BattleEvent>
    {
        // ポジティブイベント
        new BattleEvent
        {
            Id = 1,
            Type = BattleEventType.TreasureChest,
            Name = "宝箱出現",
            Description = "戦場に宝箱が出現した！",
            Icon = "📦",
            IsPositive = true,
            TriggerChance = 5,
            TriggerMessage = "📦 戦場に宝箱が出現した！",
            MinTurn = 2
        },
        new BattleEvent
        {
            Id = 2,
            Type = BattleEventType.CriticalRush,
            Name = "クリティカルラッシュ",
            Description = "次の3ターン、クリティカル率が大幅上昇！",
            Icon = "💥",
            IsPositive = true,
            Duration = 3,
            TriggerChance = 8,
            ValuePercent = 30,
            TriggerMessage = "💥 クリティカルラッシュ！次の3ターン、クリティカル率+30%！",
            EndMessage = "クリティカルラッシュの効果が切れた..."
        },
        new BattleEvent
        {
            Id = 3,
            Type = BattleEventType.GilBonus,
            Name = "ギルボーナス",
            Description = "この戦闘のギル報酬が1.5倍！",
            Icon = "💰",
            IsPositive = true,
            Duration = 99,
            TriggerChance = 6,
            Value = 1.5,
            TriggerMessage = "💰 ギルボーナス！この戦闘のギル報酬1.5倍！"
        },
        new BattleEvent
        {
            Id = 4,
            Type = BattleEventType.ExpBonus,
            Name = "経験値ブースト",
            Description = "この戦闘の経験値が1.5倍！",
            Icon = "✨",
            IsPositive = true,
            Duration = 99,
            TriggerChance = 6,
            Value = 1.5,
            TriggerMessage = "✨ 経験値ブースト！この戦闘の経験値1.5倍！"
        },
        new BattleEvent
        {
            Id = 5,
            Type = BattleEventType.HealSpring,
            Name = "回復の泉",
            Description = "HPが50%回復した！",
            Icon = "💚",
            IsPositive = true,
            TriggerChance = 7,
            ValuePercent = 50,
            TriggerMessage = "💚 回復の泉が湧き出た！HP50%回復！"
        },
        new BattleEvent
        {
            Id = 6,
            Type = BattleEventType.PowerSurge,
            Name = "攻撃力上昇",
            Description = "次の3ターン、攻撃力が上昇！",
            Icon = "⚔️",
            IsPositive = true,
            Duration = 3,
            TriggerChance = 10,
            ValuePercent = 25,
            TriggerMessage = "⚔️ 攻撃力上昇！次の3ターン、攻撃力+25%！",
            EndMessage = "攻撃力上昇の効果が切れた..."
        },
        new BattleEvent
        {
            Id = 7,
            Type = BattleEventType.DefenseAura,
            Name = "防御力上昇",
            Description = "次の3ターン、防御力が上昇！",
            Icon = "🛡️",
            IsPositive = true,
            Duration = 3,
            TriggerChance = 10,
            ValuePercent = 25,
            TriggerMessage = "🛡️ 防御力上昇！次の3ターン、防御力+25%！",
            EndMessage = "防御力上昇の効果が切れた..."
        },
        
        // ネガティブイベント
        new BattleEvent
        {
            Id = 10,
            Type = BattleEventType.EnemyReinforcement,
            Name = "敵の援軍",
            Description = "新たな敵が現れた！",
            Icon = "👥",
            IsPositive = false,
            TriggerChance = 8,
            TriggerMessage = "👥 敵の援軍が現れた！",
            MinTurn = 2
        },
        new BattleEvent
        {
            Id = 11,
            Type = BattleEventType.PoisonFog,
            Name = "毒の霧",
            Description = "毒の霧が漂い始めた...",
            Icon = "☠️",
            IsPositive = false,
            Duration = 3,
            TriggerChance = 10,
            ValuePercent = 5,
            TriggerMessage = "☠️ 毒の霧が漂い始めた！毎ターンHP5%減少！",
            EndMessage = "毒の霧が晴れた..."
        },
        new BattleEvent
        {
            Id = 12,
            Type = BattleEventType.ParalysisGas,
            Name = "麻痺ガス",
            Description = "麻痺のガスが広がった...",
            Icon = "⚡",
            IsPositive = false,
            Duration = 2,
            TriggerChance = 8,
            ValuePercent = 30,
            TriggerMessage = "⚡ 麻痺ガス！30%の確率で行動不能！",
            EndMessage = "麻痺ガスが消散した..."
        },
        new BattleEvent
        {
            Id = 13,
            Type = BattleEventType.SleepSpore,
            Name = "睡眠胞子",
            Description = "眠気を誘う胞子が舞った...",
            Icon = "💤",
            IsPositive = false,
            Duration = 2,
            TriggerChance = 7,
            ValuePercent = 25,
            TriggerMessage = "💤 睡眠胞子！25%の確率で睡眠状態！",
            EndMessage = "睡眠胞子が消えた..."
        },
        new BattleEvent
        {
            Id = 14,
            Type = BattleEventType.ConfusionMist,
            Name = "混乱の霧",
            Description = "混乱の霧が立ち込めた...",
            Icon = "🌀",
            IsPositive = false,
            Duration = 3,
            TriggerChance = 8,
            ValuePercent = 20,
            TriggerMessage = "🌀 混乱の霧！20%の確率で自分を攻撃！",
            EndMessage = "混乱の霧が晴れた..."
        },
        new BattleEvent
        {
            Id = 15,
            Type = BattleEventType.EnemyRage,
            Name = "敵の狂乱",
            Description = "敵が狂乱状態に！",
            Icon = "😤",
            IsPositive = false,
            Duration = 3,
            TriggerChance = 6,
            ValuePercent = 50,
            TriggerMessage = "😤 敵が狂乱した！攻撃力+50%、防御力-25%！",
            EndMessage = "敵の狂乱が収まった..."
        },
        new BattleEvent
        {
            Id = 16,
            Type = BattleEventType.Darkness,
            Name = "暗闇",
            Description = "視界が奪われた...",
            Icon = "🌑",
            IsPositive = false,
            Duration = 3,
            TriggerChance = 8,
            ValuePercent = 30,
            TriggerMessage = "🌑 暗闇！命中率-30%！",
            EndMessage = "視界が戻った..."
        },
        
        // 特殊イベント
        new BattleEvent
        {
            Id = 20,
            Type = BattleEventType.RareMonster,
            Name = "レアモンスター",
            Description = "レアモンスターが出現！",
            Icon = "🌟",
            IsPositive = true,
            TriggerChance = 3,
            TriggerMessage = "🌟 レアモンスターが出現！倒せば特別な報酬が！",
            MinTurn = 1,
            MaxTurn = 1
        },
        new BattleEvent
        {
            Id = 21,
            Type = BattleEventType.GoldenEnemy,
            Name = "ゴールデンエネミー",
            Description = "黄金に輝く敵が現れた！",
            Icon = "🥇",
            IsPositive = true,
            Duration = 99,
            TriggerChance = 2,
            Value = 3,
            TriggerMessage = "🥇 ゴールデンエネミー！ギルドロップ3倍！"
        },
        new BattleEvent
        {
            Id = 22,
            Type = BattleEventType.DoubleDrop,
            Name = "ドロップ率アップ",
            Description = "アイテムドロップ率が上昇！",
            Icon = "🎁",
            IsPositive = true,
            Duration = 99,
            TriggerChance = 5,
            Value = 1.5,
            TriggerMessage = "🎁 ドロップ率アップ！アイテムが出やすくなった！"
        },
        new BattleEvent
        {
            Id = 23,
            Type = BattleEventType.SkillSeal,
            Name = "スキル封印",
            Description = "スキルが封印された！",
            Icon = "🔒",
            IsPositive = false,
            Duration = 2,
            TriggerChance = 5,
            TriggerMessage = "🔒 スキル封印！スキルが使用不能！",
            EndMessage = "スキル封印が解けた..."
        },
        new BattleEvent
        {
            Id = 24,
            Type = BattleEventType.MagicBoost,
            Name = "魔法強化",
            Description = "魔力が高まった！",
            Icon = "🔮",
            IsPositive = true,
            Duration = 3,
            TriggerChance = 8,
            ValuePercent = 40,
            TriggerMessage = "🔮 魔法強化！魔法攻撃力+40%！",
            EndMessage = "魔法強化の効果が切れた..."
        }
    };
    
    public static BattleEvent? GetEvent(int id)
    {
        return Events.FirstOrDefault(e => e.Id == id);
    }
    
    public static List<BattleEvent> GetPositiveEvents()
    {
        return Events.Where(e => e.IsPositive).ToList();
    }
    
    public static List<BattleEvent> GetNegativeEvents()
    {
        return Events.Where(e => !e.IsPositive).ToList();
    }
}
