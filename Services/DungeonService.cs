using FFA.Models;

namespace FFA.Services;

public class DungeonService
{
    private static readonly Random _random = new();
    private readonly CombatService _combat;

    public DungeonService(CombatService combat)
    {
        _combat = combat;
    }
    // Damage scaling constant: larger K => defense has weaker effect. Default 500 as recommended.
    private const double DamageDeflectionK = 500.0;

    #region ターン制バトルシステム
    
    /// <summary>
    /// バトルセッション情報
    /// </summary>
    public class BattleSession
    {
        public int Turn { get; set; } = 1;
        public bool IsPlayerTurn { get; set; } = true;
        public bool IsDefending { get; set; } = false; // 防御中かどうか
        public int ComboCount { get; set; } = 0; // 連続攻撃回数
        public int TotalDamageDealt { get; set; } = 0; // 与えた総ダメージ
        public int TotalDamageTaken { get; set; } = 0; // 受けた総ダメージ
        public List<string> BattleLog { get; set; } = new();
        public bool IsFinished { get; set; } = false;
        public bool PlayerWon { get; set; } = false;
    }
    
    /// <summary>
    /// バトルアクションの結果
    /// </summary>
    public class BattleActionResult
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "";
        public int Damage { get; set; } = 0;
        public bool IsCritical { get; set; } = false;
        public bool IsDefending { get; set; } = false;
        public bool BattleEnded { get; set; } = false;
        public bool PlayerWon { get; set; } = false;
    }
    
    /// <summary>
    /// プレイヤーの攻撃アクション
    /// </summary>
    public BattleActionResult PlayerAttack(Models.User user, Models.Enemy enemy, BattleSession session)
    {
        var result = new BattleActionResult();
        
        // ダメージ計算
        var rawDamage = CalculatePlayerDamage(user);
        var finalDamage = ApplyCriticalAndDefense(rawDamage, enemy.Defense);
        
        // クリティカル判定
        result.IsCritical = _combat?.IsCritical() ?? false;
        
        // ダメージ適用
        enemy.HP = Math.Max(0, enemy.HP - finalDamage);
        result.Damage = finalDamage;
        
        // コンボ更新
        session.ComboCount++;
        session.TotalDamageDealt += finalDamage;
        
        // メッセージ生成
        var critText = result.IsCritical ? "【クリティカル！】" : "";
        result.Message = $"⚔️ 攻撃！ {enemy.Name}に {finalDamage} ダメージを与えた！{critText}";
        
        // 敵が倒れたかチェック
        if (enemy.HP <= 0)
        {
            result.BattleEnded = true;
            result.PlayerWon = true;
            session.IsFinished = true;
            session.PlayerWon = true;
            result.Message += $"\n🎉 {enemy.Name}を倒した！";
        }
        
        session.IsDefending = false;
        return result;
    }
    
    /// <summary>
    /// プレイヤーの防御アクション
    /// </summary>
    public BattleActionResult PlayerDefend(Models.User user, Models.Enemy enemy, BattleSession session)
    {
        var result = new BattleActionResult
        {
            IsDefending = true,
            Message = "🛡️ 防御態勢をとった！次の攻撃のダメージが半減します。"
        };
        
        session.IsDefending = true;
        session.ComboCount = 0; // コンボリセット
        
        return result;
    }
    
    /// <summary>
    /// 逃走アクション
    /// </summary>
    public BattleActionResult PlayerFlee(Models.User user, Models.Enemy enemy, BattleSession session)
    {
        var result = new BattleActionResult();
        
        // 逃走成功率（敵レベルとプレイヤーレベルの差で計算）
        var userLevel = user.Exp / 100 + 1;
        var levelDiff = enemy.Level - userLevel;
        var baseFleeRate = 50;
        var fleeRate = Math.Max(10, Math.Min(90, baseFleeRate - levelDiff * 10));
        
        if (_random.Next(100) < fleeRate)
        {
            result.Success = true;
            result.Message = "🏃 逃走に成功した！";
            result.BattleEnded = true;
            session.IsFinished = true;
        }
        else
        {
            result.Success = false;
            result.Message = "🏃 逃走に失敗した...";
        }
        
        return result;
    }
    
    /// <summary>
    /// 敵の攻撃アクション
    /// </summary>
    public BattleActionResult EnemyAttack(Models.User user, Models.Enemy enemy, BattleSession session)
    {
        var result = new BattleActionResult();
        
        // 敵のダメージ計算
        var rawDamage = CalculateEnemyDamage(enemy, user);
        
        // プレイヤーが防御中ならダメージ半減
        var finalDamage = session.IsDefending ? rawDamage / 2 : rawDamage;
        
        // ダメージ適用
        user.HP = Math.Max(0, user.HP - finalDamage);
        result.Damage = finalDamage;
        
        session.TotalDamageTaken += finalDamage;
        
        // メッセージ生成
        var defendText = session.IsDefending ? "（防御で軽減！）" : "";
        result.Message = $"👾 {enemy.Name}の攻撃！ {finalDamage} ダメージを受けた！{defendText}";
        
        // プレイヤーが倒れたかチェック
        if (user.HP <= 0)
        {
            result.BattleEnded = true;
            result.PlayerWon = false;
            session.IsFinished = true;
            session.PlayerWon = false;
            result.Message += "\n💀 倒された...";
        }
        
        return result;
    }
    
    /// <summary>
    /// 先制攻撃判定
    /// </summary>
    public bool CheckPreemptiveStrike(Models.User user, Models.Enemy enemy)
    {
        var userSpeed = 10 + (user.Status?.Dex ?? 0) * 2;
        var enemySpeed = enemy.Speed;
        
        // プレイヤーの素早さが敵より高い場合、先制攻撃の確率が上がる
        var preemptiveRate = 20 + (userSpeed - enemySpeed);
        preemptiveRate = Math.Max(5, Math.Min(50, preemptiveRate));
        
        return _random.Next(100) < preemptiveRate;
    }
    
    #endregion

    #region ドロップシステム
    
    /// <summary>
    /// ドロップアイテムを生成
    /// </summary>
    public List<DropResult> GenerateDrops(Models.Enemy enemy, int comboBonus = 0)
    {
        var drops = new List<DropResult>();
        
        // 通常ドロップ
        foreach (var dropItem in enemy.DropItems)
        {
            var dropRate = dropItem.DropRate + comboBonus;
            if (_random.Next(100) < dropRate)
            {
                var quantity = _random.Next(dropItem.MinQuantity, dropItem.MaxQuantity + 1);
                drops.Add(new DropResult
                {
                    Name = dropItem.Name,
                    Quantity = quantity,
                    Rarity = dropItem.Rarity
                });
            }
        }
        
        // 単一ドロップアイテム（後方互換性）
        if (!string.IsNullOrEmpty(enemy.DropItem) && enemy.DropItems.Count == 0)
        {
            var dropRate = enemy.DropRate + comboBonus;
            if (_random.Next(100) < dropRate)
            {
                drops.Add(new DropResult
                {
                    Name = enemy.DropItem,
                    Quantity = 1,
                    Rarity = ItemRarity.Common
                });
            }
        }
        
        // レアドロップ
        if (enemy.RareDrop != null)
        {
            var rareRate = enemy.RareDropRate + (comboBonus / 2);
            if (_random.Next(100) < rareRate)
            {
                drops.Add(new DropResult
                {
                    Name = enemy.RareDrop.Name,
                    Quantity = _random.Next(enemy.RareDrop.MinQuantity, enemy.RareDrop.MaxQuantity + 1),
                    Rarity = enemy.RareDrop.Rarity,
                    IsRare = true
                });
            }
        }
        
        return drops;
    }
    
    /// <summary>
    /// ドロップ結果
    /// </summary>
    public class DropResult
    {
        public string Name { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public ItemRarity Rarity { get; set; } = ItemRarity.Common;
        public bool IsRare { get; set; } = false;
    }
    
    /// <summary>
    /// 報酬計算（コンボボーナス含む）
    /// </summary>
    public (int exp, int gil, int bonusExp, int bonusGil) CalculateRewards(Models.Enemy enemy, BattleSession session)
    {
        // 基本報酬
        var baseExp = enemy.Exp + enemy.BonusExp;
        var baseGil = enemy.Gil + enemy.BonusGil;
        
        // コンボボーナス（5コンボごとに+10%）
        var comboMultiplier = 1.0 + (session.ComboCount / 5) * 0.1;
        
        // 与ダメージボーナス
        var damageBonus = session.TotalDamageDealt / 100;
        
        // 総報酬
        var totalExp = (int)((baseExp + damageBonus) * comboMultiplier);
        var totalGil = (int)(baseGil * comboMultiplier);
        
        // ボーナス部分を分離
        var bonusExp = totalExp - enemy.Exp;
        var bonusGil = totalGil - enemy.Gil;
        
        return (enemy.Exp, enemy.Gil, bonusExp, bonusGil);
    }
    
    #endregion

    // 職業による基本ボーナス
    public int GetJobBonus(Models.Job job)
    {
        return job switch
        {
            Models.Job.Warrior => 5,  // 攻撃力+
            Models.Job.BlackMage => 8, // 魔力+
            Models.Job.WhiteMage => 3, // 回復力
            Models.Job.Monk => 4,     // バランス
            _ => 0
        };
    }

    // ランダム敵生成
    public Models.Enemy GenerateRandomEnemy(int playerLevel)
    {
        var enemies = new[]
        {
            new Models.Enemy { Name = "スライム", HP = 20, MaxHP = 20, Attack = 5, Defense = 2, Exp = 10, Gil = 5, DropItem = "草药", DropRate = 20 },
            new Models.Enemy { Name = "ゴブリン", HP = 35, MaxHP = 35, Attack = 8, Defense = 3, Exp = 20, Gil = 10, DropItem = "短剣", DropRate = 15 },
            new Models.Enemy { Name = "オーク", HP = 50, MaxHP = 50, Attack = 12, Defense = 5, Exp = 35, Gil = 20, DropItem = "盾牌", DropRate = 10 },
            new Models.Enemy { Name = "ウolf", HP = 40, MaxHP = 40, Attack = 15, Defense = 4, Exp = 30, Gil = 15, DropItem = "毛皮", DropRate = 25 },
            new Models.Enemy { Name = "スケルトン", HP = 45, MaxHP = 45, Attack = 10, Defense = 8, Exp = 25, Gil = 12, DropItem = "骨", DropRate = 30 },
        };

        // プレイヤーLvに合わせて敵を強化
        var enemy = enemies[_random.Next(enemies.Length)];
        var levelMultiplier = 1 + (playerLevel - 1) * 0.1;
        
        return new Models.Enemy
        {
            Name = enemy.Name,
            HP = (int)(enemy.HP * levelMultiplier),
            MaxHP = (int)(enemy.MaxHP * levelMultiplier),
            Attack = (int)(enemy.Attack * levelMultiplier),
            Defense = (int)(enemy.Defense * levelMultiplier),
            Exp = (int)(enemy.Exp * levelMultiplier),
            Gil = (int)(enemy.Gil * levelMultiplier),
            DropItem = enemy.DropItem,
            DropRate = enemy.DropRate
        };
    }

    // プレイヤーの攻撃計算
    public int CalculatePlayerDamage(Models.User user)
    {
        var baseAttack = 10 + user.Job switch
        {
            Models.Job.Warrior => 5,
            Models.Job.BlackMage => 8,
            Models.Job.WhiteMage => 3,
            Models.Job.Monk => 4,
            _ => 0
        };
        
        var weaponBonus = user.EquippedWeapon?.Attack ?? 0;
        // 経験値からレベル計算（100EXP = Lv1, 200EXP = Lv2...）
        var levelBonus = (user.Exp / 100) * 2;
        
        return baseAttack + weaponBonus + levelBonus + _random.Next(1, 6);
    }

    // Apply defense scaling using the formula:
    // Damage = ATK * (1 - tanh(DEF / K))
    public int ApplyDefenseScaling(int atk, int def, double k = DamageDeflectionK)
    {
        // ensure non-negative inputs
        var ATK = Math.Max(0, atk);
        var DEF = Math.Max(0, def);
        var ratio = DEF / k;
        var factor = 1.0 - Math.Tanh(ratio);
        var dmg = ATK * factor;
        var result = (int)Math.Max(1, Math.Floor(dmg));
        return result;
    }

    // Apply critical if CombatService is available externally. A simple helper to combine both steps
    public int ApplyCriticalAndDefense(int rawAtk, int def)
    {
        var isCrit = _combat?.IsCritical() ?? false;
        var afterDef = ApplyDefenseScaling(rawAtk, def);
        var final = _combat?.ApplyCritical(afterDef, isCrit) ?? afterDef;
        return final;
    }

    // 敵の攻撃計算
    public int CalculateEnemyDamage(Models.Enemy enemy, Models.User user)
    {
        var defense = user.EquippedArmor?.Defense ?? 0;
        // base enemy attack may include some randomness
        var baseAtk = enemy.Attack + _random.Next(0, 3);
        return ApplyDefenseScaling(baseAtk, defense);
    }

    // ドロップ判定
    public string? RollDrop(Models.Enemy enemy)
    {
        if (_random.Next(100) < enemy.DropRate)
        {
            return enemy.DropItem;
        }
        return null;
    }

    // 経験値計算
    public int CalculateExp(Models.Enemy enemy)
    {
        return enemy.Exp;
    }

    // ギル獲得
    public int CalculateGil(Models.Enemy enemy)
    {
        return enemy.Gil;
    }
}
