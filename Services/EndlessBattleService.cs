using FFA.Models;
using LiteDB;
using System.Collections.Concurrent;

namespace FFA.Services;

/// <summary>
/// 連戦型バトルを管理するサービス
/// </summary>
public class EndlessBattleService
{
    private readonly string _databasePath;
    private readonly Random _random = new();
    private readonly ConcurrentDictionary<string, EndlessBattleSession> _activeSessions = new();
    
    // モンスターテンプレート（連戦用）
    private static readonly List<EndlessMonsterTemplate> EndlessMonsterTemplates = new()
    {
        // Easy難易度用
        new() { Name = "強化されたスライム", Icon = "🟢", BaseHP = 80, BaseAttack = 15, BaseDefense = 5, BaseExp = 30, BaseGil = 20, Difficulty = EndlessBattleDifficulty.Easy },
        new() { Name = "凶暴なゴブリン", Icon = "👺", BaseHP = 100, BaseAttack = 20, BaseDefense = 8, BaseExp = 40, BaseGil = 30, Difficulty = EndlessBattleDifficulty.Easy },
        new() { Name = "狂ったコボルト", Icon = "🐕", BaseHP = 90, BaseAttack = 18, BaseDefense = 6, BaseExp = 35, BaseGil = 25, Difficulty = EndlessBattleDifficulty.Easy },
        
        // Normal難易度用
        new() { Name = "鋼鉄のゴーレム", Icon = "🗿", BaseHP = 200, BaseAttack = 35, BaseDefense = 25, BaseExp = 80, BaseGil = 60, Difficulty = EndlessBattleDifficulty.Normal },
        new() { Name = "暗黒騎士の亡霊", Icon = "👻", BaseHP = 180, BaseAttack = 40, BaseDefense = 20, BaseExp = 90, BaseGil = 70, Difficulty = EndlessBattleDifficulty.Normal },
        new() { Name = "魔獣オーク", Icon = "👹", BaseHP = 220, BaseAttack = 38, BaseDefense = 22, BaseExp = 85, BaseGil = 65, Difficulty = EndlessBattleDifficulty.Normal },
        
        // Hard難易度用
        new() { Name = "古代のドラゴン幼体", Icon = "🐉", BaseHP = 400, BaseAttack = 60, BaseDefense = 35, BaseExp = 150, BaseGil = 120, Difficulty = EndlessBattleDifficulty.Hard },
        new() { Name = "デーモンロード", Icon = "😈", BaseHP = 380, BaseAttack = 65, BaseDefense = 30, BaseExp = 160, BaseGil = 130, Difficulty = EndlessBattleDifficulty.Hard },
        new() { Name = "ミスリルゴーレム", Icon = "🤖", BaseHP = 450, BaseAttack = 55, BaseDefense = 45, BaseExp = 140, BaseGil = 110, Difficulty = EndlessBattleDifficulty.Hard },
        
        // Extreme難易度用
        new() { Name = "カオスドラゴン", Icon = "🔥", BaseHP = 800, BaseAttack = 100, BaseDefense = 60, BaseExp = 300, BaseGil = 250, Difficulty = EndlessBattleDifficulty.Extreme },
        new() { Name = "魔王の影", Icon = "👤", BaseHP = 750, BaseAttack = 110, BaseDefense = 55, BaseExp = 320, BaseGil = 270, Difficulty = EndlessBattleDifficulty.Extreme },
        new() { Name = "神獣キマイラ", Icon = "🦁", BaseHP = 850, BaseAttack = 95, BaseDefense = 65, BaseExp = 280, BaseGil = 230, Difficulty = EndlessBattleDifficulty.Extreme },
    };
    
    public EndlessBattleService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "users.db");
    }
    
    /// <summary>
    /// 新しい連戦セッションを開始
    /// </summary>
    public EndlessBattleSession StartSession(User user, EndlessBattleDifficulty difficulty)
    {
        var session = new EndlessBattleSession
        {
            UserId = user.Id.ToString(),
            PlayerHP = user.HP,
            PlayerMP = 0,
            PlayerMaxHP = user.MaxHP,
            PlayerMaxMP = 0,
            IsActive = true
        };
        
        // 最初の敵を生成
        GenerateEnemies(session, difficulty);
        
        _activeSessions[session.Id] = session;
        
        return session;
    }
    
    /// <summary>
    /// セッションを取得
    /// </summary>
    public EndlessBattleSession? GetSession(string sessionId)
    {
        return _activeSessions.TryGetValue(sessionId, out var session) ? session : null;
    }
    
    /// <summary>
    /// ユーザーのアクティブなセッションを取得
    /// </summary>
    public EndlessBattleSession? GetUserActiveSession(string userId)
    {
        return _activeSessions.Values.FirstOrDefault(s => s.UserId == userId && s.IsActive);
    }
    
    /// <summary>
    /// 敵を生成
    /// </summary>
    public void GenerateEnemies(EndlessBattleSession session, EndlessBattleDifficulty difficulty)
    {
        var templates = EndlessMonsterTemplates.Where(t => t.Difficulty == difficulty).ToList();
        if (!templates.Any())
        {
            templates = EndlessMonsterTemplates.Where(t => t.Difficulty == EndlessBattleDifficulty.Easy).ToList();
        }
        
        var template = templates[_random.Next(templates.Count)];
        var multiplier = session.DifficultyMultiplier;
        
        var enemy = new Enemy
        {
            Name = session.CurrentWave >= 10 ? $"【Wave{session.CurrentWave}】{template.Name}" : template.Name,
            Icon = template.Icon,
            MaxHP = (int)(template.BaseHP * multiplier),
            HP = (int)(template.BaseHP * multiplier),
            Attack = (int)(template.BaseAttack * multiplier),
            Defense = (int)(template.BaseDefense * multiplier),
            Exp = (int)(template.BaseExp * multiplier),
            Gil = (int)(template.BaseGil * multiplier),
            Level = (int)(5 * multiplier + session.CurrentWave),
            Type = GetEnemyType(session.CurrentWave),
            Speed = (int)(10 + session.CurrentWave * 2),
            MagicAttack = (int)(template.BaseAttack * 0.5 * multiplier),
            MagicDefense = (int)(template.BaseDefense * 0.5 * multiplier)
        };
        
        // ドロップアイテム設定
        if (_random.NextDouble() < 0.3 + session.CurrentWave * 0.02)
        {
            enemy.DropItems.Add(new DropItem
            {
                Name = GetRandomDropItem(session.CurrentWave),
                DropRate = 30 + session.CurrentWave * 2,
                MinQuantity = 1,
                MaxQuantity = Math.Min(3, 1 + session.CurrentWave / 5)
            });
        }
        
        session.CurrentEnemies = new List<Enemy> { enemy };
    }
    
    /// <summary>
    /// ウェーブに応じた敵タイプを取得
    /// </summary>
    private EnemyType GetEnemyType(int wave)
    {
        return wave switch
        {
            >= 20 => EnemyType.Boss,
            >= 15 => EnemyType.Rare,
            >= 10 => EnemyType.Elite,
            >= 5 => EnemyType.Elite,
            _ => EnemyType.Normal
        };
    }
    
    /// <summary>
    /// ランダムドロップアイテムを取得
    /// </summary>
    private string GetRandomDropItem(int wave)
    {
        var items = wave switch
        {
            >= 15 => new[] { "ドラゴンの鱗", "魔晶石", "オリハルコンの欠片", "古代の秘薬" },
            >= 10 => new[] { "魔獣の牙", "精霊石", "高級薬草", "ミスリルの欠片" },
            >= 5 => new[] { "獣の皮", "鉄鉱石", "薬草", "魔石の欠片" },
            _ => new[] { "スライムの粘液", "ゴブリンの牙", "小さな骨", "石ころ" }
        };
        return items[_random.Next(items.Length)];
    }
    
    /// <summary>
    /// 戦闘を実行
    /// </summary>
    public EndlessBattleResult ExecuteBattle(EndlessBattleSession session, User user)
    {
        var result = new EndlessBattleResult();
        var enemy = session.CurrentEnemies.FirstOrDefault();
        
        if (enemy == null)
        {
            result.Victory = true;
            return result;
        }
        
        var playerAttack = BattleHelper.GetTotalAttack(user);
        var playerDefense = BattleHelper.GetTotalDefense(user);
        var playerSpeed = BattleHelper.GetTotalSpeed(user);
        var playerCritChance = BattleHelper.GetCriticalChance(user);
        var playerEvasion = BattleHelper.GetEvasionChance(user);
        
        var battleLog = new List<string>();
        var totalDamageDealt = 0;
        var totalDamageTaken = 0;
        
        // 先制攻撃判定
        bool playerFirst = playerSpeed >= enemy.Speed;
        
        while (enemy.HP > 0 && session.PlayerHP > 0)
        {
            if (playerFirst)
            {
                // プレイヤーの攻撃
                var damage = CalculateDamage(playerAttack, enemy.Defense, playerCritChance, out bool isCrit);
                enemy.HP -= damage;
                totalDamageDealt += damage;
                battleLog.Add(isCrit ? $"クリティカル！ {damage}ダメージを与えた！" : $"{damage}ダメージを与えた。");
                
                if (enemy.HP <= 0) break;
            }
            
            // 敵の攻撃
            if (_random.Next(100) < playerEvasion)
            {
                battleLog.Add("攻撃を回避した！");
            }
            else
            {
                var enemyDamage = CalculateDamage(enemy.Attack, playerDefense, 5, out bool enemyCrit);
                session.PlayerHP -= enemyDamage;
                totalDamageTaken += enemyDamage;
                battleLog.Add(enemyCrit ? $"敵のクリティカル！ {enemyDamage}ダメージを受けた！" : $"{enemyDamage}ダメージを受けた。");
            }
            
            if (!playerFirst)
            {
                // プレイヤーの攻撃
                if (enemy.HP > 0)
                {
                    var damage = CalculateDamage(playerAttack, enemy.Defense, playerCritChance, out bool isCrit);
                    enemy.HP -= damage;
                    totalDamageDealt += damage;
                    battleLog.Add(isCrit ? $"クリティカル！ {damage}ダメージを与えた！" : $"{damage}ダメージを与えた。");
                }
            }
        }
        
        result.BattleLog = battleLog;
        result.DamageDealt = totalDamageDealt;
        result.DamageTaken = totalDamageTaken;
        
        if (enemy.HP <= 0)
        {
            result.Victory = true;
            result.ExpEarned = (int)(enemy.Exp * session.RewardMultiplier);
            result.GilEarned = (int)(enemy.Gil * session.RewardMultiplier);
            
            // ドロップアイテム処理
            foreach (var drop in enemy.DropItems)
            {
                if (_random.Next(100) < drop.DropRate)
                {
                    var quantity = _random.Next(drop.MinQuantity, drop.MaxQuantity + 1);
                    result.ItemsDropped.Add($"{drop.Name} x{quantity}");
                }
            }
            
            // レアドロップ
            if (enemy.RareDrop != null && _random.Next(100) < enemy.RareDropRate)
            {
                result.ItemsDropped.Add(enemy.RareDrop.Name);
            }
        }
        else
        {
            result.Victory = false;
        }
        
        // 戦闘記録を追加
        session.BattleRecords.Add(new EndlessBattleRecord
        {
            Wave = session.CurrentWave,
            EnemyName = enemy.Name,
            Victory = result.Victory,
            DamageDealt = totalDamageDealt,
            DamageTaken = totalDamageTaken,
            ExpEarned = result.ExpEarned,
            GilEarned = result.GilEarned,
            ItemsDropped = result.ItemsDropped.ToList()
        });
        
        return result;
    }
    
    /// <summary>
    /// ダメージ計算
    /// </summary>
    private int CalculateDamage(int attack, int defense, int critChance, out bool isCritical)
    {
        isCritical = _random.Next(100) < critChance;
        
        var baseDamage = Math.Max(1, attack - defense / 2);
        var variance = _random.Next(-2, 3);
        var damage = baseDamage + variance;
        
        if (isCritical)
        {
            damage = (int)(damage * 1.5);
        }
        
        return Math.Max(1, damage);
    }
    
    /// <summary>
    /// 次のウェーブへ進む
    /// </summary>
    public void NextWave(EndlessBattleSession session, EndlessBattleDifficulty difficulty)
    {
        session.CurrentWave++;
        session.MaxWaveReached = Math.Max(session.MaxWaveReached, session.CurrentWave);
        GenerateEnemies(session, difficulty);
    }
    
    /// <summary>
    /// セッションから離脱（報酬を受け取る）
    /// </summary>
    public EndlessBattleReward Retreat(EndlessBattleSession session)
    {
        var reward = new EndlessBattleReward
        {
            Exp = session.TotalExpEarned,
            Gil = session.TotalGilEarned,
            BonusExp = CalculateWaveBonus(session.CurrentWave - 1),
            BonusGil = CalculateWaveBonus(session.CurrentWave - 1) / 2
        };
        
        // アイテムは記録から収集
        foreach (var record in session.BattleRecords)
        {
            reward.Items.AddRange(record.ItemsDropped);
        }
        
        session.IsActive = false;
        session.EndTime = DateTime.UtcNow;
        _activeSessions.TryRemove(session.Id, out _);
        
        // 統計を保存
        SaveSessionStats(session);
        
        return reward;
    }
    
    /// <summary>
    /// 敗北時の処理
    /// </summary>
    public EndlessBattleReward Defeat(EndlessBattleSession session)
    {
        // 敗北時は報酬半減
        var reward = new EndlessBattleReward
        {
            Exp = session.TotalExpEarned / 2,
            Gil = session.TotalGilEarned / 2,
            BonusExp = CalculateWaveBonus(session.CurrentWave - 1) / 2,
            BonusGil = CalculateWaveBonus(session.CurrentWave - 1) / 4
        };
        
        // アイテムは記録から収集（敗北時は半分）
        var allItems = session.BattleRecords.SelectMany(r => r.ItemsDropped).ToList();
        for (int i = 0; i < allItems.Count / 2; i++)
        {
            reward.Items.Add(allItems[i]);
        }
        
        session.IsActive = false;
        session.EndTime = DateTime.UtcNow;
        _activeSessions.TryRemove(session.Id, out _);
        
        // 統計を保存
        SaveSessionStats(session);
        
        return reward;
    }
    
    /// <summary>
    /// ウェーブボーナス計算
    /// </summary>
    private int CalculateWaveBonus(int wavesCleared)
    {
        return wavesCleared * 50 + (wavesCleared * wavesCleared * 10);
    }
    
    /// <summary>
    /// セッション統計を保存
    /// </summary>
    private void SaveSessionStats(EndlessBattleSession session)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var collection = db.GetCollection<EndlessBattleStats>("endless_battle_stats");
            var stats = collection.FindById(session.UserId) ?? new EndlessBattleStats();
            
            stats.TotalSessions++;
            if (session.BattleRecords.Any(r => r.Victory))
            {
                stats.TotalVictories++;
            }
            else
            {
                stats.TotalDefeats++;
            }
            stats.MaxWaveReached = Math.Max(stats.MaxWaveReached, session.MaxWaveReached);
            stats.TotalExpEarned += session.TotalExpEarned;
            stats.TotalGilEarned += session.TotalGilEarned;
            stats.TotalMonstersDefeated += session.BattleRecords.Count(r => r.Victory);
            
            collection.Upsert(session.UserId, stats);
        }
        catch
        {
            // 統計保存エラーは無視
        }
    }
    
    /// <summary>
    /// ユーザーの統計を取得
    /// </summary>
    public EndlessBattleStats GetUserStats(string userId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var collection = db.GetCollection<EndlessBattleStats>("endless_battle_stats");
            return collection.FindById(userId) ?? new EndlessBattleStats();
        }
        catch
        {
            return new EndlessBattleStats();
        }
    }
    
    /// <summary>
    /// プレイヤーのHP/MPを更新
    /// </summary>
    public void UpdatePlayerStatus(EndlessBattleSession session, int hp, int mp)
    {
        session.PlayerHP = Math.Max(0, hp);
        session.PlayerMP = Math.Max(0, mp);
    }
}

/// <summary>
/// モンスターテンプレート
/// </summary>
internal class EndlessMonsterTemplate
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "👾";
    public int BaseHP { get; set; }
    public int BaseAttack { get; set; }
    public int BaseDefense { get; set; }
    public int BaseExp { get; set; }
    public int BaseGil { get; set; }
    public EndlessBattleDifficulty Difficulty { get; set; }
}

/// <summary>
/// 戦闘結果
/// </summary>
public class EndlessBattleResult
{
    public bool Victory { get; set; }
    public int ExpEarned { get; set; }
    public int GilEarned { get; set; }
    public List<string> ItemsDropped { get; set; } = new();
    public List<string> BattleLog { get; set; } = new();
    public int DamageDealt { get; set; }
    public int DamageTaken { get; set; }
}
