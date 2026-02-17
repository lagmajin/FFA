using FFA.Models;

namespace FFA.Services;

/// <summary>
/// スタミナサービス - プレイヤーのスタミナ管理
/// </summary>
public class StaminaService
{
    private readonly Dictionary<string, PlayerStamina> _playerStaminas = new();
    
    // スタミナのデフォルト設定
    public static readonly Dictionary<StaminaType, StaminaConfig> DefaultConfigs = new()
    {
        { 
            StaminaType.Movement, 
            new StaminaConfig { 
                Type = StaminaType.Movement, 
                Name = "移動", 
                MaxValue = 100, 
                RecoveryPerHour = 20,
                AutoResetDaily = true,
                RecoveryIntervalMinutes = 10 
            } 
        },
        { 
            StaminaType.Battle, 
            new StaminaConfig { 
                Type = StaminaType.Battle, 
                Name = "戦闘", 
                MaxValue = 50, 
                RecoveryPerHour = 10,
                AutoResetDaily = true,
                RecoveryIntervalMinutes = 15 
            } 
        },
        { 
            StaminaType.Mining, 
            new StaminaConfig { 
                Type = StaminaType.Mining, 
                Name = "採掘", 
                MaxValue = 30, 
                RecoveryPerHour = 5,
                AutoResetDaily = true,
                RecoveryIntervalMinutes = 30 
            } 
        },
        { 
            StaminaType.Fishing, 
            new StaminaConfig { 
                Type = StaminaType.Fishing, 
                Name = "釣り", 
                MaxValue = 20, 
                RecoveryPerHour = 5,
                AutoResetDaily = true,
                RecoveryIntervalMinutes = 30 
            } 
        },
        { 
            StaminaType.Crafting, 
            new StaminaConfig { 
                Type = StaminaType.Crafting, 
                Name = "製作", 
                MaxValue = 50, 
                RecoveryPerHour = 15,
                AutoResetDaily = true,
                RecoveryIntervalMinutes = 20 
            } 
        },
        { 
            StaminaType.Guild, 
            new StaminaConfig { 
                Type = StaminaType.Guild, 
                Name = "ギルド戦", 
                MaxValue = 10, 
                RecoveryPerHour = 2,
                AutoResetDaily = true,
                RecoveryIntervalMinutes = 60 
            } 
        },
        { 
            StaminaType.Exploration, 
            new StaminaConfig { 
                Type = StaminaType.Exploration, 
                Name = "探索", 
                MaxValue = 25, 
                RecoveryPerHour = 5,
                AutoResetDaily = true,
                RecoveryIntervalMinutes = 30 
            } 
        },
    };
    
    /// <summary>
    /// プレイヤーのスタミナを取得または作成
    /// </summary>
    public PlayerStamina GetOrCreateStamina(string username)
    {
        if (!_playerStaminas.ContainsKey(username))
        {
            _playerStaminas[username] = new PlayerStamina
            {
                Username = username,
                CurrentStamina = new Dictionary<StaminaType, int>(),
                MaxStamina = new Dictionary<StaminaType, int>(),
                LastRecoveryTime = new Dictionary<StaminaType, DateTime>(),
                LastResetDate = new Dictionary<StaminaType, DateTime>()
            };
            
            // デフォルト値で初期化
            foreach (var config in DefaultConfigs)
            {
                _playerStaminas[username].CurrentStamina[config.Key] = config.Value.MaxValue;
                _playerStaminas[username].MaxStamina[config.Key] = config.Value.MaxValue;
                _playerStaminas[username].LastRecoveryTime[config.Key] = DateTime.UtcNow;
                _playerStaminas[username].LastResetDate[config.Key] = DateTime.UtcNow.Date;
            }
        }
        
        var stamina = _playerStaminas[username];
        
        // 回復とリセットのチェック
        foreach (var config in DefaultConfigs)
        {
            var type = config.Key;
            
            // 日次リセット
            if (config.Value.AutoResetDaily && stamina.LastResetDate[type] < DateTime.UtcNow.Date)
            {
                stamina.CurrentStamina[type] = config.Value.MaxValue;
                stamina.LastResetDate[type] = DateTime.UtcNow.Date;
            }
            
            // 自然回復
            RecoverStamina(username, type);
        }
        
        return stamina;
    }
    
    /// <summary>
    /// スタミナを回復
    /// </summary>
    private void RecoverStamina(string username, StaminaType type)
    {
        if (!DefaultConfigs.TryGetValue(type, out var config))
            return;
            
        var stamina = _playerStaminas[username];
        
        // 最大なら回復不要
        if (stamina.CurrentStamina[type] >= config.MaxValue)
            return;
            
        var lastRecovery = stamina.LastRecoveryTime[type];
        var now = DateTime.UtcNow;
        var minutesPassed = (now - lastRecovery).TotalMinutes;
        
        // 回復間隔チェック
        if (minutesPassed >= config.RecoveryIntervalMinutes)
        {
            // 回復量を計算
            int intervalsPassed = (int)(minutesPassed / config.RecoveryIntervalMinutes);
            int recoverAmount = intervalsPassed * config.RecoveryPerHour / 60 * config.RecoveryIntervalMinutes;
            recoverAmount = Math.Max(1, recoverAmount);
            
            stamina.CurrentStamina[type] = Math.Min(config.MaxValue, stamina.CurrentStamina[type] + recoverAmount);
            stamina.LastRecoveryTime[type] = now;
        }
    }
    
    /// <summary>
    /// スタミナを使用
    /// </summary>
    public StaminaUseResult UseStamina(string username, StaminaType type, int amount = 1)
    {
        var stamina = GetOrCreateStamina(username);
        
        if (!DefaultConfigs.TryGetValue(type, out var config))
        {
            return new StaminaUseResult
            {
                Success = false,
                Message = "不明なスタミナタイプです",
                Type = type
            };
        }
        
        // 回復チェック
        RecoverStamina(username, type);
        
        int current = stamina.CurrentStamina[type];
        
        if (current < amount)
        {
            // 次回回復時間を計算
            var nextRecovery = stamina.LastRecoveryTime[type].AddMinutes(config.RecoveryIntervalMinutes);
            
            return new StaminaUseResult
            {
                Success = false,
                Message = $"スタミナが不足しています（必要: {amount}, 現在: {current}）",
                Type = type,
                UsedAmount = 0,
                Remaining = current,
                MaxStamina = config.MaxValue,
                RecoverAt = nextRecovery
            };
        }
        
        // スタミナ使用
        stamina.CurrentStamina[type] -= amount;
        
        return new StaminaUseResult
        {
            Success = true,
            Message = $"{config.Name}を{amount}回行った",
            Type = type,
            UsedAmount = amount,
            Remaining = stamina.CurrentStamina[type],
            MaxStamina = config.MaxValue
        };
    }
    
    /// <summary>
    /// スタミナを回復（アイテム等使用）
    /// </summary>
    public StaminaUseResult RecoverStaminaItem(string username, StaminaType type, int amount)
    {
        var stamina = GetOrCreateStamina(username);
        
        if (!DefaultConfigs.TryGetValue(type, out var config))
        {
            return new StaminaUseResult
            {
                Success = false,
                Message = "不明なスタミナタイプです",
                Type = type
            };
        }
        
        int oldValue = stamina.CurrentStamina[type];
        stamina.CurrentStamina[type] = Math.Min(config.MaxValue, stamina.CurrentStamina[type] + amount);
        
        return new StaminaUseResult
        {
            Success = true,
            Message = $"{config.Name}のスタミナを{amount}回復した",
            Type = type,
            UsedAmount = 0,
            Remaining = stamina.CurrentStamina[type],
            MaxStamina = config.MaxValue
        };
    }
    
    /// <summary>
    /// 全てのスタミナ情報を取得
    /// </summary>
    public StaminaInfo GetStaminaInfo(string username)
    {
        var stamina = GetOrCreateStamina(username);
        
        var info = new StaminaInfo
        {
            Username = username,
            Status = new Dictionary<StaminaType, StaminaStatus>()
        };
        
        foreach (var config in DefaultConfigs)
        {
            RecoverStamina(username, config.Key);
            
            int current = stamina.CurrentStamina[config.Key];
            int max = config.Value.MaxValue;
            
            info.Status[config.Key] = new StaminaStatus
            {
                TypeName = config.Value.Name,
                Current = current,
                Max = max,
                RecoveryPerHour = config.Value.RecoveryPerHour,
                IsFull = current >= max,
                NextRecovery = stamina.LastRecoveryTime[config.Key].AddMinutes(config.Value.RecoveryIntervalMinutes),
                RecoveryProgress = max > 0 ? (double)current / max : 0
            };
        }
        
        return info;
    }
    
    /// <summary>
    /// 特定のスタミナタイプのみ取得
    /// </summary>
    public StaminaStatus GetStaminaStatus(string username, StaminaType type)
    {
        var stamina = GetOrCreateStamina(username);
        RecoverStamina(username, type);
        
        if (!DefaultConfigs.TryGetValue(type, out var config))
        {
            return new StaminaStatus { TypeName = "不明" };
        }
        
        int current = stamina.CurrentStamina[type];
        
        return new StaminaStatus
        {
            TypeName = config.Name,
            Current = current,
            Max = config.MaxValue,
            RecoveryPerHour = config.RecoveryPerHour,
            IsFull = current >= config.MaxValue,
            NextRecovery = stamina.LastRecoveryTime[type].AddMinutes(config.RecoveryIntervalMinutes),
            RecoveryProgress = config.MaxValue > 0 ? (double)current / config.MaxValue : 0
        };
    }
    
    /// <summary>
    /// スタミナを全回復
    /// </summary>
    public StaminaUseResult RecoverAll(string username)
    {
        var stamina = GetOrCreateStamina(username);
        
        foreach (var config in DefaultConfigs)
        {
            stamina.CurrentStamina[config.Key] = config.Value.MaxValue;
            stamina.LastRecoveryTime[config.Key] = DateTime.UtcNow;
            stamina.LastResetDate[config.Key] = DateTime.UtcNow.Date;
        }
        
        return new StaminaUseResult
        {
            Success = true,
            Message = "全てのスタミナを回復しました",
            Remaining = 0,
            MaxStamina = 0
        };
    }
    
    /// <summary>
    /// スタミナが十分にあるかチェック
    /// </summary>
    public bool HasStamina(string username, StaminaType type, int amount = 1)
    {
        var stamina = GetOrCreateStamina(username);
        RecoverStamina(username, type);
        return stamina.CurrentStamina[type] >= amount;
    }
}
