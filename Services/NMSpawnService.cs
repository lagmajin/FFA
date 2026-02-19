using LiteDB;
using FFA.Models;
using System.Text.Json;

namespace FFA.Services;

/// <summary>
/// ネームドモンスタースポーンサービス
/// FF11スタイルのNAMED MONSTERシステム
/// </summary>
public class NMSpawnService
{
    private readonly string _databasePath;
    private readonly MasterDataService _masterDataService;
    private readonly Random _random = new();
    
    // スポーン通知用のイベント
    public event EventHandler<NotoriousMonster>? OnNMSpawned;
    public event EventHandler<NotoriousMonster>? OnNMKilled;
    
    public NMSpawnService(MasterDataService masterDataService)
    {
        _masterDataService = masterDataService;
        
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "nm_spawns.db");
    }
    
    #region NM管理
    
    /// <summary>
    /// 全てのNMの状態を取得
    /// </summary>
    public List<NotoriousMonster> GetAllNMStatus()
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var nmCollection = db.GetCollection<NotoriousMonster>("notorious_monsters");
            return nmCollection.FindAll().ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NMSpawnService.GetAllNMStatus error: {ex.Message}");
            return new List<NotoriousMonster>();
        }
    }
    
    /// <summary>
    /// 特定のNMを取得
    /// </summary>
    public NotoriousMonster? GetNMStatus(int nmId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var nmCollection = db.GetCollection<NotoriousMonster>("notorious_monsters");
            return nmCollection.FindById(nmId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NMSpawnService.GetNMStatus error: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// フィールドに沸いているNMを取得
    /// </summary>
    public List<NotoriousMonster> GetActiveNMByLocation(string location)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var nmCollection = db.GetCollection<NotoriousMonster>("notorious_monsters");
            return nmCollection.Find(nm => nm.Location == location && nm.Status == NMStatus.Alive).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NMSpawnService.GetActiveNMByLocation error: {ex.Message}");
            return new List<NotoriousMonster>();
        }
    }
    
    /// <summary>
    /// 全ての沸いているNMを取得
    /// </summary>
    public List<NotoriousMonster> GetAllActiveNM()
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var nmCollection = db.GetCollection<NotoriousMonster>("notorious_monsters");
            return nmCollection.Find(nm => nm.Status == NMStatus.Alive).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NMSpawnService.GetAllActiveNM error: {ex.Message}");
            return new List<NotoriousMonster>();
        }
    }
    
    #endregion
    
    #region スポーン管理
    
    /// <summary>
    /// NMスポーンを試みる（ランダム）
    /// </summary>
    public async Task<NotoriousMonster?> TrySpawnNMAsync(string location)
    {
        // マスターデータからNM一覧を取得
        var nmMaster = await _masterDataService.LoadDataAsync<NamedMonsterMasterData>("named-monsters.json");
        if (nmMaster?.NotoriousMonsters == null) return null;
        
        // 指定ロケーションのNMのみ対象
        var availableNM = nmMaster.NotoriousMonsters
            .Where(nm => nm.Location == location)
            .ToList();
        
        if (availableNM.Count == 0) return null;
        
        // 既に沸いているNMを確認
        var activeNM = GetActiveNMByLocation(location);
        if (activeNM.Count >= 3) return null; // 同地点に3体以上沸かない
        
        // ランダムに選択
        var nmData = availableNM[_random.Next(availableNM.Count)];
        
        // 既に沸いているかチェック
        var existingNM = GetNMStatus(nmData.Id);
        if (existingNM != null && existingNM.Status == NMStatus.Alive) return null;
        
        // 沸いていない場合はスポーン
        return await SpawnNMAsync(nmData);
    }
    
    /// <summary>
    /// 特定のNMを沸かせる
    /// </summary>
    public async Task<NotoriousMonster> SpawnNMAsync(NamedMonsterData nmData)
    {
        var nm = new NotoriousMonster
        {
            Id = nmData.Id,
            Name = nmData.JapaneseName,
            Location = nmData.Location,
            Status = NMStatus.Alive,
            SpawnedAtUtc = DateTime.UtcNow,
            LastKilledAtUtc = null,
            RespawnInterval = TimeSpan.FromHours(nmData.RespawnHours),
            MaxHP = nmData.MaxHp,
            CurrentHP = nmData.MaxHp,
            Attack = nmData.Attack,
            Defense = nmData.Defense,
            RewardExp = nmData.RewardExp,
            RewardGil = nmData.RewardGil,
            DropItem = nmData.DropItems.FirstOrDefault()?.Name,
            DropRate = nmData.DropItems.FirstOrDefault()?.DropRate ?? 10,
            LastKilledBy = null
        };
        
        // データベースに保存
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var nmCollection = db.GetCollection<NotoriousMonster>("notorious_monsters");
            nmCollection.Upsert(nm);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NMSpawnService.SpawnNMAsync error: {ex.Message}");
        }
        
        // イベント通知
        OnNMSpawned?.Invoke(this, nm);
        
        Console.WriteLine($"🌟 [NM Spawn] {nm.Name} が {nm.Location} に沸きました！");
        
        return nm;
    }
    
    /// <summary>
    /// NMを倒した时的処理
    /// </summary>
    public void KillNM(int nmId, string killedBy)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var nmCollection = db.GetCollection<NotoriousMonster>("notorious_monsters");
            
            var nm = nmCollection.FindById(nmId);
            if (nm == null || nm.Status != NMStatus.Alive) return;
            
            nm.Status = NMStatus.Dead;
            nm.CurrentHP = 0;
            nm.LastKilledAtUtc = DateTime.UtcNow;
            nm.LastKilledBy = killedBy;
            
            nmCollection.Update(nm);
            
            // イベント通知
            OnNMKilled?.Invoke(this, nm);
            
            Console.WriteLine($"💀 [NM Defeated] {nm.Name} を {killedBy} が倒しました！");
            
            // リスポーンスケジュールをログ出力
            var respawnTime = nm.LastKilledAtUtc.Value.Add(nm.RespawnInterval);
            Console.WriteLine($"⏰ [NM Respawn] {nm.Name} は {respawnTime:yyyy/MM/dd HH:mm} に沸く予定");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NMSpawnService.KillNM error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// リスポーン時間を迎えたNMを再沸きさせる
    /// </summary>
    public async Task CheckAndSpawnExpiredNMAsync()
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var nmCollection = db.GetCollection<NotoriousMonster>("notorious_monsters");
            
            var now = DateTime.UtcNow;
            var deadNM = nmCollection.Find(nm => 
                nm.Status == NMStatus.Dead && 
                nm.LastKilledAtUtc.HasValue &&
                nm.LastKilledAtUtc.Value.Add(nm.RespawnInterval) <= now)
                .ToList();
            
            foreach (var nm in deadNM)
            {
                nm.Status = NMStatus.Alive;
                nm.SpawnedAtUtc = now;
                nm.CurrentHP = nm.MaxHP;
                nm.LastKilledAtUtc = null;
                nm.LastKilledBy = null;
                
                nmCollection.Update(nm);
                
                Console.WriteLine($"🌟 [NM Respawned] {nm.Name} が再沸きました！");
                
                OnNMSpawned?.Invoke(this, nm);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NMSpawnService.CheckAndSpawnExpiredNMAsync error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// NMにダメージを与える
    /// </summary>
    public void DamageNM(int nmId, int damage)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var nmCollection = db.GetCollection<NotoriousMonster>("notorious_monsters");
            
            var nm = nmCollection.FindById(nmId);
            if (nm == null || nm.Status != NMStatus.Alive) return;
            
            nm.CurrentHP = Math.Max(0, nm.CurrentHP - damage);
            
            if (nm.CurrentHP <= 0)
            {
                nm.Status = NMStatus.Dead;
                nm.LastKilledAtUtc = DateTime.UtcNow;
            }
            
            nmCollection.Update(nm);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NMSpawnService.DamageNM error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// NMの存在を確認（フィールド探索时）
    /// </summary>
    public (bool exists, NotoriousMonster? nm) CheckNMPresence(string location)
    {
        var activeNM = GetActiveNMByLocation(location);
        if (activeNM.Count > 0)
        {
            return (true, activeNM.First());
        }
        return (false, null);
    }
    
    /// <summary>
    /// NM情報をマスターデータから取得
    /// </summary>
    public async Task<NamedMonsterData?> GetNMMasterDataAsync(int nmId)
    {
        var nmMaster = await _masterDataService.LoadDataAsync<NamedMonsterMasterData>("named-monsters.json");
        return nmMaster?.NotoriousMonsters.FirstOrDefault(nm => nm.Id == nmId);
    }
    
    #endregion
    
    #region NM検索・フィルター
    
    /// <summary>
    /// 沸いているNMを検索
    /// </summary>
    public async Task<List<NotoriousMonster>> SearchActiveNMAsync(string? location = null, string? keyword = null)
    {
        var activeNM = GetAllActiveNM();
        
        if (!string.IsNullOrEmpty(location))
        {
            activeNM = activeNM.Where(nm => nm.Location.Contains(location)).ToList();
        }
        
        if (!string.IsNullOrEmpty(keyword))
        {
            activeNM = activeNM.Where(nm => 
                nm.Name.Contains(keyword) || 
                nm.Location.Contains(keyword)).ToList();
        }
        
        return activeNM;
    }
    
    /// <summary>
    /// 次回の沸き時間を取得
    /// </summary>
    public List<(NotoriousMonster nm, DateTime respawnTime)> GetUpcomingRespawns()
    {
        var result = new List<(NotoriousMonster, DateTime)>();
        
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var nmCollection = db.GetCollection<NotoriousMonster>("notorious_monsters");
            
            var deadNM = nmCollection.Find(nm => 
                nm.Status == NMStatus.Dead && 
                nm.LastKilledAtUtc.HasValue)
                .ToList();
            
            foreach (var nm in deadNM)
            {
                var respawnTime = nm.LastKilledAtUtc!.Value.Add(nm.RespawnInterval);
                if (respawnTime > DateTime.UtcNow)
                {
                    result.Add((nm, respawnTime));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"NMSpawnService.GetUpcomingRespawns error: {ex.Message}");
        }
        
        return result.OrderBy(x => x.Item2).ToList();
    }
    
    #endregion
}
