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
