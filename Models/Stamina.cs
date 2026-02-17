namespace FFA.Models;

/// <summary>
/// スタミナタイプ
/// </summary>
public enum StaminaType
{
    Movement,    // 移動
    Battle,      // 戦闘
    Mining,      // 採掘
    Fishing,     // 釣り
    Crafting,    // 製作
    Guild,       // ギルド戦
    Exploration  // 探索
}

/// <summary>
/// プレイヤーのスタミナ
/// </summary>
public class PlayerStamina
{
    public string Username { get; set; } = "";
    public Dictionary<StaminaType, int> CurrentStamina { get; set; } = new();
    public Dictionary<StaminaType, int> MaxStamina { get; set; } = new();
    public Dictionary<StaminaType, DateTime> LastRecoveryTime { get; set; } = new();
    public Dictionary<StaminaType, DateTime> LastResetDate { get; set; } = new();
}

/// <summary>
/// スタミナ設定
/// </summary>
public class StaminaConfig
{
    public StaminaType Type { get; set; }
    public string Name { get; set; } = "";
    public int MaxValue { get; set; }
    public int RecoveryPerHour { get; set; }  // 1時間あたりの回復量
    public bool AutoResetDaily { get; set; }  // 毎日リセットするか
    public int RecoveryIntervalMinutes { get; set; }  // 回復間隔（分）
}

/// <summary>
/// スタミナ使用結果
/// </summary>
public class StaminaUseResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public StaminaType Type { get; set; }
    public int UsedAmount { get; set; }
    public int Remaining { get; set; }
    public int MaxStamina { get; set; }
    public DateTime? RecoverAt { get; set; }
}

/// <summary>
/// スタミナ情報
/// </summary>
public class StaminaInfo
{
    public string Username { get; set; } = "";
    public Dictionary<StaminaType, StaminaStatus> Status { get; set; } = new();
}

/// <summary>
/// スタミナステータス
/// </summary>
public class StaminaStatus
{
    public string TypeName { get; set; } = "";
    public int Current { get; set; }
    public int Max { get; set; }
    public int RecoveryPerHour { get; set; }
    public double RecoveryProgress { get; set; }  // 0.0 - 1.0
    public DateTime? NextRecovery { get; set; }
    public bool IsFull { get; set; }
}
