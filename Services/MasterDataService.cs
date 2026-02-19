using System.Text.Json;
using System.Text.Json.Serialization;

namespace FFA.Services;

/// <summary>
/// JSONマスターデータ読み込みサービス
/// </summary>
public class MasterDataService
{
    private readonly IWebHostEnvironment _environment;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    
    // キャッシュ
    private static readonly Dictionary<string, object> _cache = new();
    private static readonly object _cacheLock = new();
    
    public MasterDataService(IWebHostEnvironment environment, HttpClient httpClient)
    {
        _environment = environment;
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
    }
    
    /// <summary>
    /// JSONファイルからデータを読み込む（キャッシュ付き）
    /// </summary>
    public async Task<T?> LoadDataAsync<T>(string relativePath) where T : class
    {
        var cacheKey = typeof(T).Name + "_" + relativePath;
        
        // キャッシュチェック
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                return cached as T;
            }
        }
        
        try
        {
            // wwwroot/data からの相対パス
            var filePath = Path.Combine(_environment.WebRootPath, "data", relativePath);
            
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                var data = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                
                // キャッシュに保存
                if (data != null)
                {
                    lock (_cacheLock)
                    {
                        _cache[cacheKey] = data;
                    }
                }
                
                return data;
            }
            
            // ファイルが見つからない場合はHTTPで取得（Blazor WebAssembly用）
            var url = $"/data/{relativePath}";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                
                if (data != null)
                {
                    lock (_cacheLock)
                    {
                        _cache[cacheKey] = data;
                    }
                }
                
                return data;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MasterDataService.LoadDataAsync error: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// キャッシュをクリア
    /// </summary>
    public void ClearCache()
    {
        lock (_cacheLock)
        {
            _cache.Clear();
        }
    }
    
    /// <summary>
    /// 特定のデータのキャッシュをクリア
    /// </summary>
    public void ClearCache<T>(string relativePath)
    {
        var cacheKey = typeof(T).Name + "_" + relativePath;
        lock (_cacheLock)
        {
            _cache.Remove(cacheKey);
        }
    }
}

#region マスターデータモデル

/// <summary>
/// 鉱石マスターデータ
/// </summary>
public class OreMasterData
{
    public List<OreData> Ores { get; set; } = new();
}

public class OreData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string JapaneseName { get; set; } = "";
    public string Type { get; set; } = "";
    public int Value { get; set; }
    public int RequiredSkillLevel { get; set; }
    public double DropRate { get; set; }
    public int MinQuantity { get; set; }
    public int MaxQuantity { get; set; }
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "🪨";
}

/// <summary>
/// 魚マスターデータ
/// </summary>
public class FishMasterData
{
    public List<FishData> Fish { get; set; } = new();
}

public class FishData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Rarity { get; set; }
    public int BasePrice { get; set; }
    public int RequiredFishingLevel { get; set; }
    public List<string> Locations { get; set; } = new();
    public List<string> ActiveTimes { get; set; } = new();
    public List<string> WeatherConditions { get; set; } = new();
    public double BaseCatchRate { get; set; }
    public int MinWeight { get; set; }
    public int MaxWeight { get; set; }
    public int ExpReward { get; set; }
    public bool CanCook { get; set; }
    public string? CookedItemName { get; set; }
    public int CookedHealAmount { get; set; }
    public string? CookedBuffEffect { get; set; }
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "🐟";
}

/// <summary>
/// モンスターマスターデータ
/// </summary>
public class MonsterMasterData
{
    public List<MonsterData> Monsters { get; set; } = new();
}

public class MonsterData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "👾";
    public int Level { get; set; }
    public string Type { get; set; } = "Normal";
    public int Hp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
    public int Exp { get; set; }
    public int Gil { get; set; }
    public List<MonsterDropItem> DropItems { get; set; } = new();
    public MonsterDropItem? RareDrop { get; set; }
    public string Description { get; set; } = "";
    public List<string> Habitat { get; set; } = new();
}

public class MonsterDropItem
{
    public string Name { get; set; } = "";
    public int DropRate { get; set; }
    public int MinQuantity { get; set; }
    public int MaxQuantity { get; set; }
    public string Rarity { get; set; } = "Common";
}

/// <summary>
/// フィールドマスターデータ
/// </summary>
public class FieldMasterData
{
    public List<FieldData> Fields { get; set; } = new();
}

public class FieldData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Difficulty { get; set; }
    public string Icon { get; set; } = "🗺️";
    public List<string> Enemies { get; set; } = new();
    public List<string> Drops { get; set; } = new();
    public int RecommendedLevel { get; set; }
}

/// <summary>
/// ネームドモンスターマスターデータ
/// </summary>
public class NamedMonsterMasterData
{
    public List<NamedMonsterData> NotoriousMonsters { get; set; } = new();
}

public class NamedMonsterData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string JapaneseName { get; set; } = "";
    public string EnglishName { get; set; } = "";
    public string Icon { get; set; } = "👾";
    public string Location { get; set; } = "";
    public int RespawnHours { get; set; } = 6;
    public int MaxHp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int RewardExp { get; set; }
    public int RewardGil { get; set; }
    public List<MonsterDropItem> DropItems { get; set; } = new();
    public string Description { get; set; } = "";
    public string? Weakness { get; set; }
    public string? Strategy { get; set; }
}

#endregion
