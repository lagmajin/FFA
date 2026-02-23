using FFA.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FFA.Services;

/// <summary>
/// バトルイベント管理サービス
/// </summary>
public class BattleEventService
{
    private readonly Random _random = new();

    /// <summary>
    /// ターン開始時にイベントをチェック
    /// </summary>
    public BattleEventResult? CheckForEvent(int currentTurn, string? location = null)
    {
        // 発生可能なイベントをフィルタリング
        var possibleEvents = BattleEventDatabase.Events
            .Where(e => currentTurn >= e.MinTurn && currentTurn <= e.MaxTurn)
            .Where(e => string.IsNullOrEmpty(e.RequiredLocation) || e.RequiredLocation == location)
            .ToList();

        if (!possibleEvents.Any())
            return null;

        // 各イベントの発生をチェック
        foreach (var evt in possibleEvents)
        {
            if (_random.Next(100) < evt.TriggerChance)
            {
                return new BattleEventResult
                {
                    Event = evt,
                    Message = evt.TriggerMessage
                };
            }
        }

        return null;
    }

    /// <summary>
    /// イベント効果を適用
    /// </summary>
    public EventEffectResult ApplyEventEffect(BattleEvent evt, User user, Enemy? enemy)
    {
        var result = new EventEffectResult
        {
            EventType = evt.Type
        };

        switch (evt.Type)
        {
            case BattleEventType.TreasureChest:
                // 宝箱の中身を生成
                var treasure = GenerateTreasure(user.Level);
                result.Items = treasure.Items;
                result.Gil = treasure.Gil;
                result.Message = $"宝箱を開けた！{treasure.Gil}ギルとアイテムを入手！";
                break;

            case BattleEventType.HealSpring:
                var healAmount = (int)(user.MaxHP * evt.ValuePercent / 100.0);
                result.HPChange = healAmount;
                result.Message = $"回復の泉でHPが{healAmount}回復！";
                break;

            case BattleEventType.EnemyReinforcement:
                // 援軍敵を生成
                result.SpawnEnemy = true;
                result.Message = "新たな敵が現れた！";
                break;

            case BattleEventType.PoisonFog:
            case BattleEventType.ParalysisGas:
            case BattleEventType.SleepSpore:
            case BattleEventType.ConfusionMist:
            case BattleEventType.Darkness:
                result.StatusEffect = evt.Type.ToString();
                result.Duration = evt.Duration;
                result.Message = evt.TriggerMessage;
                break;

            case BattleEventType.EnemyRage:
                result.EnemyAttackMod = evt.ValuePercent;
                result.EnemyDefenseMod = -25;
                result.Duration = evt.Duration;
                result.Message = evt.TriggerMessage;
                break;

            default:
                // 持続効果のあるイベント
                if (evt.Duration > 0)
                {
                    result.Duration = evt.Duration;
                    result.Message = evt.TriggerMessage;
                }
                break;
        }

        return result;
    }

    /// <summary>
    /// 持続イベントのターン経過処理
    /// </summary>
    public List<ActiveBattleEvent> ProcessActiveEvents(List<ActiveBattleEvent> activeEvents)
    {
        var expiredEvents = new List<ActiveBattleEvent>();

        foreach (var active in activeEvents)
        {
            active.RemainingTurns--;
            if (active.RemainingTurns <= 0)
            {
                expiredEvents.Add(active);
            }
        }

        // 期限切れイベントを削除
        activeEvents.RemoveAll(e => e.RemainingTurns <= 0);

        return expiredEvents;
    }

    /// <summary>
    /// 持続イベントの効果を計算
    /// </summary>
    public EventModifiers CalculateEventModifiers(List<ActiveBattleEvent> activeEvents)
    {
        var modifiers = new EventModifiers();

        foreach (var active in activeEvents)
        {
            var evt = active.Event;
            switch (evt.Type)
            {
                case BattleEventType.CriticalRush:
                    modifiers.CriticalChanceBonus += evt.ValuePercent;
                    break;
                case BattleEventType.GilBonus:
                    modifiers.GilMultiplier *= evt.Value;
                    break;
                case BattleEventType.ExpBonus:
                    modifiers.ExpMultiplier *= evt.Value;
                    break;
                case BattleEventType.PowerSurge:
                    modifiers.AttackBonus += evt.ValuePercent;
                    break;
                case BattleEventType.DefenseAura:
                    modifiers.DefenseBonus += evt.ValuePercent;
                    break;
                case BattleEventType.PoisonFog:
                    modifiers.PoisonDamagePercent = evt.ValuePercent;
                    break;
                case BattleEventType.ParalysisGas:
                    modifiers.ParalysisChance = evt.ValuePercent;
                    break;
                case BattleEventType.SleepSpore:
                    modifiers.SleepChance = evt.ValuePercent;
                    break;
                case BattleEventType.ConfusionMist:
                    modifiers.ConfusionChance = evt.ValuePercent;
                    break;
                case BattleEventType.Darkness:
                    modifiers.AccuracyPenalty = evt.ValuePercent;
                    break;
                case BattleEventType.EnemyRage:
                    modifiers.EnemyAttackBonus += evt.ValuePercent;
                    modifiers.EnemyDefensePenalty += 25;
                    break;
                case BattleEventType.GoldenEnemy:
                    modifiers.GilMultiplier *= evt.Value;
                    break;
                case BattleEventType.DoubleDrop:
                    modifiers.DropRateMultiplier *= evt.Value;
                    break;
                case BattleEventType.SkillSeal:
                    modifiers.SkillSealed = true;
                    break;
                case BattleEventType.MagicBoost:
                    modifiers.MagicBonus += evt.ValuePercent;
                    break;
            }
        }

        return modifiers;
    }

    /// <summary>
    /// 宝箱の中身を生成
    /// </summary>
    private TreasureResult GenerateTreasure(int userLevel)
    {
        var result = new TreasureResult();
        
        // ギル
        result.Gil = _random.Next(50, 200) * userLevel;

        // アイテム（簡易版）
        if (_random.Next(100) < 30)
        {
            result.Items.Add(new InventoryItem
            {
                ItemId = _random.Next(1, 100),
                Name = "回復薬",
                Quantity = _random.Next(1, 3),
                Type = "Consumable"
            });
        }

        return result;
    }

    /// <summary>
    /// 援軍敵を生成（簡易版）
    /// </summary>
    public Enemy? GenerateReinforcement(int baseDifficulty)
    {
        // 簡易的な敵生成（Battle.razor側で生成するため、ここではnullを返す）
        return null;
    }
}

/// <summary>
/// バトルイベント結果
/// </summary>
public class BattleEventResult
{
    public BattleEvent Event { get; set; } = null!;
    public string Message { get; set; } = "";
}

/// <summary>
/// イベント効果結果
/// </summary>
public class EventEffectResult
{
    public BattleEventType EventType { get; set; }
    public string Message { get; set; } = "";
    public int HPChange { get; set; }
    public int Gil { get; set; }
    public List<InventoryItem> Items { get; set; } = new();
    public bool SpawnEnemy { get; set; }
    public string? StatusEffect { get; set; }
    public int Duration { get; set; }
    public int EnemyAttackMod { get; set; }
    public int EnemyDefenseMod { get; set; }
}

/// <summary>
/// イベント修飾子
/// </summary>
public class EventModifiers
{
    public int CriticalChanceBonus { get; set; }
    public double GilMultiplier { get; set; } = 1.0;
    public double ExpMultiplier { get; set; } = 1.0;
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public int MagicBonus { get; set; }
    public int PoisonDamagePercent { get; set; }
    public int ParalysisChance { get; set; }
    public int SleepChance { get; set; }
    public int ConfusionChance { get; set; }
    public int AccuracyPenalty { get; set; }
    public int EnemyAttackBonus { get; set; }
    public int EnemyDefensePenalty { get; set; }
    public double DropRateMultiplier { get; set; } = 1.0;
    public bool SkillSealed { get; set; }
}

/// <summary>
/// 宝箱結果
/// </summary>
public class TreasureResult
{
    public int Gil { get; set; }
    public List<InventoryItem> Items { get; set; } = new();
}
