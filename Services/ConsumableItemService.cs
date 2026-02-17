using FFA.Models;

namespace FFA.Services;

/// <summary>
/// 消耗品サービス - 消耗品アイテムの管理と使用
/// </summary>
public class ConsumableItemService
{
    private readonly StaminaService _staminaService;
    
    // 消耗品データベース
    public static readonly List<ConsumableItem> ConsumableItems = new()
    {
        // HP回復アイテム
        new ConsumableItem
        {
            Id = 1,
            Name = "薬草",
            Description = "基本的な回復薬。HPを30回復する",
            Type = ConsumableType.HPHeal,
            Price = 10,
            HealAmount = 30,
            MaxStack = 99
        },
        new ConsumableItem
        {
            Id = 2,
            Name = "回復薬",
            Description = "上級回復薬。HPを100回復する",
            Type = ConsumableType.HPHeal,
            Price = 50,
            HealAmount = 100,
            UseLevel = 5,
            MaxStack = 99
        },
        new ConsumableItem
        {
            Id = 3,
            Name = "高級回復薬",
            Description = "高級回復薬。HPを50%回復する",
            Type = ConsumableType.HPHeal,
            Price = 200,
            HealPercent = 50,
            UseLevel = 10,
            MaxStack = 50
        },
        new ConsumableItem
        {
            Id = 4,
            Name = "完全回復薬",
            Description = "HPを完全に回復する",
            Type = ConsumableType.HPHeal,
            Price = 500,
            HealPercent = 100,
            UseLevel = 20,
            MaxStack = 10
        },
        
        // スタミナ回復アイテム
        new ConsumableItem
        {
            Id = 10,
            Name = "疲劳回復薬",
            Description = "移動スタミナを30回復する",
            Type = ConsumableType.StaminaHeal,
            Price = 20,
            StaminaHealAmount = 30,
            StaminaType = StaminaType.Movement,
            MaxStack = 99
        },
        new ConsumableItem
        {
            Id = 11,
            Name = "戦闘疲労解除",
            Description = "戦闘スタミナを20回復する",
            Type = ConsumableType.StaminaHeal,
            Price = 30,
            StaminaHealAmount = 20,
            StaminaType = StaminaType.Battle,
            MaxStack = 99
        },
        new ConsumableItem
        {
            Id = 12,
            Name = "採取精力剤",
            Description = "採掘スタミナを15回復する",
            Type = ConsumableType.StaminaHeal,
            Price = 25,
            StaminaHealAmount = 15,
            StaminaType = StaminaType.Mining,
            MaxStack = 99
        },
        new ConsumableItem
        {
            Id = 13,
            Name = "万能回復薬",
            Description = "すべてのスタミナを50回復する",
            Type = ConsumableType.StaminaHeal,
            Price = 100,
            StaminaHealAmount = 50,
            MaxStack = 30
        },
        
        // バフアイテム
        new ConsumableItem
        {
            Id = 20,
            Name = "攻撃力向上薬",
            Description = "攻撃力が10%上昇する（30分）",
            Type = ConsumableType.Buff,
            Price = 100,
            BuffType = "Attack",
            BuffAmount = 10,
            BuffDurationMinutes = 30,
            MaxStack = 20
        },
        new ConsumableItem
        {
            Id = 21,
            Name = "防御力向上薬",
            Description = "防御力が10%上昇する（30分）",
            Type = ConsumableType.Buff,
            Price = 100,
            BuffType = "Defense",
            BuffAmount = 10,
            BuffDurationMinutes = 30,
            MaxStack = 20
        },
        new ConsumableItem
        {
            Id = 22,
            Name = "経験値boost",
            Description = "獲得経験値が20%上昇する（60分）",
            Type = ConsumableType.Buff,
            Price = 300,
            BuffType = "Exp",
            BuffAmount = 20,
            BuffDurationMinutes = 60,
            UseLevel = 15,
            MaxStack = 10
        },
        new ConsumableItem
        {
            Id = 23,
            Name = "ドロップ率UP",
            Description = "ドロップ率が15%上昇する（60分）",
            Type = ConsumableType.Buff,
            Price = 250,
            BuffType = "DropRate",
            BuffAmount = 15,
            BuffDurationMinutes = 60,
            UseLevel = 10,
            MaxStack = 10
        },
        
        // テレポートアイテム
        new ConsumableItem
        {
            Id = 30,
            Name = "帰還の巻物",
            Description = "街へテレポートする",
            Type = ConsumableType.Teleport,
            Price = 50,
            TeleportLocation = "Town",
            MaxStack = 10
        },
        new ConsumableItem
        {
            Id = 31,
            Name = "ホームテレポート",
            Description = "開始地点へテレポートする",
            Type = ConsumableType.Teleport,
            Price = 100,
            TeleportLocation = "Home",
            MaxStack = 5
        },
        
        // 鍵アイテム
        new ConsumableItem
        {
            Id = 40,
            Name = "ダンジョンの鍵",
            Description = "ダンジョンの鍵",
            Type = ConsumableType.Key,
            Price = 0,
            MaxStack = 1
        },
        new ConsumableItem
        {
            Id = 41,
            Name = "宝の鍵",
            Description = "寶箱を開ける鍵",
            Type = ConsumableType.Key,
            Price = 0,
            MaxStack = 10
        },
    };
    
    // プレイヤーのバフ状態
    private readonly Dictionary<string, List<ActiveBuff>> _activeBuffs = new();
    
    public ConsumableItemService()
    {
        _staminaService = new StaminaService();
    }
    
    /// <summary>
    /// アイテムを取得
    /// </summary>
    public ConsumableItem? GetItem(int itemId)
    {
        return ConsumableItems.FirstOrDefault(i => i.Id == itemId);
    }
    
    /// <summary>
    /// アイテムを取得（名前で）
    /// </summary>
    public ConsumableItem? GetItemByName(string name)
    {
        return ConsumableItems.FirstOrDefault(i => i.Name == name);
    }
    
    /// <summary>
    /// アイテムをアイテムIDで使用
    /// </summary>
    public ItemUseResult UseItem(User user, int itemId, string location = "Town")
    {
        var item = GetItem(itemId);
        if (item == null)
            return new ItemUseResult { Success = false, Message = "アイテムが見つかりません" };
        
        return UseItem(user, item, location);
    }
    
    /// <summary>
    /// アイテムを名前で使用
    /// </summary>
    public ItemUseResult UseItemByName(User user, string itemName, string location = "Town")
    {
        var item = GetItemByName(itemName);
        if (item == null)
            return new ItemUseResult { Success = false, Message = "アイテムが見つかりません" };
        
        return UseItem(user, item, location);
    }
    
    /// <summary>
    /// アイテムを使用（共通処理）
    /// </summary>
    public ItemUseResult UseItem(User user, ConsumableItem item, string location)
    {
        // レベルのチェック
        if (user.Level < item.UseLevel)
            return new ItemUseResult { Success = false, Message = $"レベル{item.UseLevel}が必要です" };
        
        // 場所による使用制限
        bool canUse = location switch
        {
            "Town" => item.CanUseInTown,
            "Battle" => item.CanUseInBattle,
            "Dungeon" => item.CanUseInDungeon,
            "Field" => item.CanUseInField,
            _ => true
        };
        
        if (!canUse)
            return new ItemUseResult { Success = false, Message = "ここでは使用できません" };
        
        // インベントリにあるかチェック
        var invItem = user.Inventory.FirstOrDefault(i => i.Name == item.Name && i.Quantity > 0);
        if (invItem == null)
            return new ItemUseResult { Success = false, Message = "アイテムがありません" };
        
        var result = new ItemUseResult { ItemConsumed = true };
        
        switch (item.Type)
        {
            case ConsumableType.HPHeal:
                result = HandleHPHeal(user, item, result);
                break;
                
            case ConsumableType.StaminaHeal:
                result = HandleStaminaHeal(user, item, result);
                break;
                
            case ConsumableType.Buff:
                result = HandleBuff(user, item, result);
                break;
                
            case ConsumableType.Teleport:
                result = HandleTeleport(user, item, result);
                break;
                
            default:
                return new ItemUseResult { Success = false, Message = "まだ使用できません" };
        }
        
        // アイテム消費
        if (result.Success && result.ItemConsumed)
        {
            invItem.Quantity--;
            if (invItem.Quantity <= 0)
                user.Inventory.Remove(invItem);
        }
        
        return result;
    }
    
    /// <summary>
    /// HP回復処理
    /// </summary>
    private ItemUseResult HandleHPHeal(User user, ConsumableItem item, ItemUseResult result)
    {
        int healAmount = 0;
        
        // 固定回復量
        if (item.HealAmount > 0)
            healAmount = item.HealAmount;
        
        // 百分率回復
        if (item.HealPercent > 0)
            healAmount = (int)(user.MaxHP * item.HealPercent / 100.0);
        
        // 最大HPを超えない
        int oldHP = user.HP;
        user.HP = Math.Min(user.MaxHP, user.HP + healAmount);
        int actualHeal = user.HP - oldHP;
        
        result.Success = true;
        result.Message = $"HPを{actualHeal}回復した！";
        result.HPHealed = actualHeal;
        
        return result;
    }
    
    /// <summary>
    /// スタミナ回復処理
    /// </summary>
    private ItemUseResult HandleStaminaHeal(User user, ConsumableItem item, ItemUseResult result)
    {
        // スタミナタイプが指定されている場合
        if (item.StaminaType.HasValue)
        {
            var type = item.StaminaType.Value;
            // 回復量はアイテム使用で處理（簡易実装）
            result.Success = true;
            result.Message = $"{type}のスタミナを{item.StaminaHealAmount}回復した！";
            result.StaminaHealed = item.StaminaHealAmount;
        }
        else
        {
            // 全スタミナ回復
            result.Success = true;
            result.Message = $"すべてのスタミナを{item.StaminaHealAmount}回復した！";
            result.StaminaHealed = item.StaminaHealAmount;
        }
        
        return result;
    }
    
    /// <summary>
    /// バフ適用処理
    /// </summary>
    private ItemUseResult HandleBuff(User user, ConsumableItem item, ItemUseResult result)
    {
        if (string.IsNullOrEmpty(item.BuffType))
            return new ItemUseResult { Success = false, Message = "バフ効果がありません" };
        
        // バフを追加
        if (!_activeBuffs.ContainsKey(user.Username))
            _activeBuffs[user.Username] = new List<ActiveBuff>();
        
        var buff = new ActiveBuff
        {
            Type = item.BuffType,
            Amount = item.BuffAmount,
            AppliedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(item.BuffDurationMinutes)
        };
        
        _activeBuffs[user.Username].Add(buff);
        
        result.Success = true;
        result.Message = $"{item.BuffType}が{item.BuffAmount}%上昇した！（{item.BuffDurationMinutes}分）";
        result.BuffApplied = item.BuffType;
        result.BuffDurationMinutes = item.BuffDurationMinutes;
        
        return result;
    }
    
    /// <summary>
    /// テレポート処理
    /// </summary>
    private ItemUseResult HandleTeleport(User user, ConsumableItem item, ItemUseResult result)
    {
        if (string.IsNullOrEmpty(item.TeleportLocation))
            return new ItemUseResult { Success = false, Message = "テレポート先がありません" };
        
        result.Success = true;
        result.Message = $"{item.TeleportLocation}へテレポートした！";
        result.TeleportLocation = item.TeleportLocation;
        
        return result;
    }
    
    /// <summary>
    /// プレイヤーのアクティブなバフを取得
    /// </summary>
    public List<ActiveBuff> GetActiveBuffs(string username)
    {
        if (!_activeBuffs.ContainsKey(username))
            return new List<ActiveBuff>();
        
        // 期限切れバフを削除
        var now = DateTime.UtcNow;
        _activeBuffs[username] = _activeBuffs[username]
            .Where(b => b.ExpiresAt > now)
            .ToList();
        
        return _activeBuffs[username];
    }
    
    /// <summary>
    /// バフによる加成を計算
    /// </summary>
    public int CalculateBuffBonus(string username, string statType, int baseValue)
    {
        var buffs = GetActiveBuffs(username);
        var buff = buffs.FirstOrDefault(b => b.Type == statType);
        
        if (buff == null)
            return baseValue;
        
        // 百分率加成
        int bonus = (int)(baseValue * buff.Amount / 100.0);
        return baseValue + bonus;
    }
    
    /// <summary>
    /// 経験値バフ加成
    /// </summary>
    public int CalculateExpBonus(string username, int baseExp)
    {
        var buffs = GetActiveBuffs(username);
        var expBuff = buffs.FirstOrDefault(b => b.Type == "Exp");
        
        if (expBuff == null)
            return baseExp;
        
        int bonus = (int)(baseExp * expBuff.Amount / 100.0);
        return baseExp + bonus;
    }
    
    /// <summary>
    /// ドロップ率バフ加成
    /// </summary>
    public int CalculateDropRateBonus(string username, int baseDropRate)
    {
        var buffs = GetActiveBuffs(username);
        var dropBuff = buffs.FirstOrDefault(b => b.Type == "DropRate");
        
        if (dropBuff == null)
            return baseDropRate;
        
        return Math.Min(100, baseDropRate + dropBuff.Amount);
    }
}

/// <summary>
/// アクティブなバフ
/// </summary>
public class ActiveBuff
{
    public string Type { get; set; } = "";
    public int Amount { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
