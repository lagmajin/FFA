using FFA.Models;
using System.Collections.Generic;
using Tomlyn;
using Tomlyn.Model;

namespace FFA.Services;

public class MapService
{
    private Dictionary<int, Map> countryMaps = new();
    private Dictionary<string, Map> allMaps = new(); // 全てのマップ
    private const int MapOffset = 4; // 町の座標(-4~4)をマップ座標(0~8)に変換

    /// <summary>読み込み診断情報</summary>
    public List<string> LoadDiagnostics { get; } = new();

    public MapService()
    {
        LoadCountryMaps();
        LoadAllMaps(); // 中間地域など全マップを読み込む
    }

    private void LoadCountryMaps()
    {
        var path1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Maps");
        var path2 = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Maps");

        LoadDiagnostics.Add($"Path1: {path1} exists={Directory.Exists(path1)}");
        LoadDiagnostics.Add($"Path2: {path2} exists={Directory.Exists(path2)}");

        // Data/Maps ディレクトリから国別マップを読み込み
        var mapsPath = path1;

        // 開発時はプロジェクトルートからも読み込み
        if (!Directory.Exists(mapsPath))
        {
            mapsPath = path2;
        }

        if (!Directory.Exists(mapsPath))
        {
            LoadDiagnostics.Add("ERROR: No Data/Maps directory found at either path.");
            return;
        }

        LoadDiagnostics.Add($"Using: {mapsPath}");

        var files = Directory.GetFiles(mapsPath, "*_country_*.toml");
        LoadDiagnostics.Add($"Found {files.Length} *_country_*.toml files");

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            try
            {
                var content = File.ReadAllText(file);
                var doc = Toml.Parse(content);

                if (doc.HasErrors)
                {
                    LoadDiagnostics.Add($"  {fileName}: TOML parse errors: {string.Join("; ", doc.Diagnostics)}");
                    continue;
                }

                var table = doc.ToModel();

                bool hasMeta = table.ContainsKey("meta");
                bool hasGrid = table.ContainsKey("grid");
                bool hasLocations = table.ContainsKey("locations");
                LoadDiagnostics.Add($"  {fileName}: hasMeta={hasMeta}, hasGrid={hasGrid}, hasLocations={hasLocations}, keys=[{string.Join(",", table.Keys)}]");

                // Use [meta] when present; otherwise fall back to root table for legacy formats
                TomlTable? meta = null;
                if (hasMeta && table["meta"] is TomlTable m) meta = m;
                else meta = table as TomlTable;

                if (meta == null)
                {
                    LoadDiagnostics.Add($"  {fileName}: SKIP (meta is null)");
                    continue;
                }

                int countryId = GetIntValue(meta, "country_id", 0);
                string countryName = GetStringValue(meta, "country_name",
                    GetStringValue(meta, "country",
                        GetStringValue(meta, "description", Path.GetFileNameWithoutExtension(file))));

                // assign stable fallback id when none provided (negative to avoid colliding with real ids)
                int assignedId = countryId != 0 ? countryId : -Math.Abs(Path.GetFileNameWithoutExtension(file).GetHashCode());

                var map = new Map
                {
                    Id = assignedId,
                    Name = countryName,
                    Width = GetIntValue(meta, "width", 4),
                    Height = GetIntValue(meta, "height", 4),
                    Locations = new List<MapLocation>()
                };

                // Determine which key contains location entries
                string? entriesKey = hasGrid ? "grid" : (hasLocations ? "locations" : null);
                if (entriesKey != null && table.ContainsKey(entriesKey))
                {
                    var gridRaw = table[entriesKey];
                    LoadDiagnostics.Add($"  {fileName}: entriesKey={entriesKey}, rawType={gridRaw?.GetType().FullName}");

                    if (gridRaw is IEnumerable<object> entries)
                    {
                        int count = 0;
                        foreach (var item in entries)
                        {
                            var grid = item as TomlTable;
                            if (grid == null)
                            {
                                LoadDiagnostics.Add($"  {fileName}: entries item is {item?.GetType().Name ?? "null"}, not TomlTable");
                                continue;
                            }

                            var location = new MapLocation
                            {
                                X = GetIntValue(grid, "x", 0),
                                Y = GetIntValue(grid, "y", 0),
                                Name = GetStringValue(grid, "name", ""),
                                Description = GetStringValue(grid, "description", ""),
                                Type = GetStringValue(grid, "type", "field"),
                                DangerLevel = GetIntValue(grid, "difficulty", 0),
                                CanEnter = true,
                                Events = new List<string>()
                            };

                            map.Locations.Add(location);
                            count++;
                        }
                        LoadDiagnostics.Add($"  {fileName}: entries count={count}");
                    }
                    else
                    {
                        LoadDiagnostics.Add($"  {fileName}: entries is not enumerable (actual type: {gridRaw?.GetType().FullName})");
                    }
                }

                countryMaps[assignedId] = map;
                LoadDiagnostics.Add($"  {fileName}: Loaded assignedId={assignedId} name={countryName} locations={map.Locations.Count}");
            }
            catch (Exception ex)
            {
                LoadDiagnostics.Add($"  {fileName}: EXCEPTION: {ex.Message}");
                Console.WriteLine($"Failed to load map {file}: {ex.Message}");
            }
        }

        LoadDiagnostics.Add($"Total maps loaded: {countryMaps.Count} ({string.Join(", ", countryMaps.Keys)})");
    }
    
    /// <summary>
    /// 全てのマップを読み込む（国マップ + 中立地域など）
    /// </summary>
    private void LoadAllMaps()
    {
        var path1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Maps");
        var path2 = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Maps");
        
        var mapsPath = path1;
        if (!Directory.Exists(mapsPath))
        {
            mapsPath = path2;
        }
        
        if (!Directory.Exists(mapsPath)) return;
        
        // 全てのTOMLマップファイルを取得
        var files = Directory.GetFiles(mapsPath, "*.toml");
        
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            try
            {
                var content = File.ReadAllText(file);
                var doc = Toml.Parse(content);
                
                if (doc.HasErrors) continue;
                
                var table = doc.ToModel();
                
                // start_position または meta から情報を取得
                Map? map = null;
                
                if (table.ContainsKey("start_position") && table["start_position"] is TomlTable startPos)
                {
                    map = new Map
                    {
                        Id = allMaps.Count,
                        Name = GetStringValue(startPos, "country", GetStringValue(startPos, "capital", fileName)),
                        Width = GetIntValue(table, "width", 5),
                        Height = GetIntValue(table, "height", 5),
                        Locations = new List<MapLocation>()
                    };
                }
                else if (table.ContainsKey("meta") && table["meta"] is TomlTable meta)
                {
                    map = new Map
                    {
                        Id = GetIntValue(meta, "country_id", allMaps.Count),
                        Name = GetStringValue(meta, "country_name", GetStringValue(meta, "country", fileName)),
                        Width = GetIntValue(meta, "width", GetIntValue(table, "width", 4)),
                        Height = GetIntValue(meta, "height", GetIntValue(table, "height", 4)),
                        Locations = new List<MapLocation>()
                    };
                }
                
                if (map == null) continue;
                
                // locations を読み込み
                if (table.ContainsKey("locations") && table["locations"] is IEnumerable<object> locations)
                {
                    foreach (var loc in locations)
                    {
                        if (loc is TomlTable t)
                        {
                            var location = new MapLocation
                            {
                                X = GetIntValue(t, "x", 0),
                                Y = GetIntValue(t, "y", 0),
                                Name = GetStringValue(t, "name", ""),
                                Description = GetStringValue(t, "description", ""),
                                Type = GetStringValue(t, "type", "field"),
                                DangerLevel = GetIntValue(t, "difficulty", GetIntValue(t, "enemy_level", 0)),
                                CanEnter = true,
                                Connection = GetStringValue(t, "connection", ""),
                                Events = new List<string>()
                            };
                            map.Locations.Add(location);
                        }
                    }
                }
                
                // マップを保存（キーはマップ名）
                allMaps[map.Name] = map;
                
                // 国マップとしても保存
                if (table.ContainsKey("meta") || fileName.Contains("country"))
                {
                    if (map.Id > 0) countryMaps[map.Id] = map;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load map {file}: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// 場所から他の地域への接続を取得
    /// </summary>
    public MapConnection? GetConnection(int countryId, int x, int y)
    {
        var map = GetMapByCountryId(countryId);
        var location = map.Locations.FirstOrDefault(l => l.X == x && l.Y == y);
        
        if (location == null || string.IsNullOrEmpty(location.Connection)) return null;
        
        // 接続先マップを探す
        var targetMap = allMaps.Values.FirstOrDefault(m => 
            m.Name.Equals(location.Connection, StringComparison.OrdinalIgnoreCase) ||
            m.Name.Replace("_", " ").Contains(location.Connection, StringComparison.OrdinalIgnoreCase));
        
        if (targetMap == null) return null;
        
        return new MapConnection
        {
            FromMapId = countryId,
            FromX = x,
            FromY = y,
            ToMapId = targetMap.Id,
            ToMapName = targetMap.Name,
            ToX = targetMap.Width / 2, // 中央から開始
            ToY = targetMap.Height / 2
        };
    }
    
    /// <summary>
    /// マップの端で接続があるかチェック
    /// </summary>
    public MapConnection? CheckEdgeConnection(int countryId, Direction direction, int currentX, int currentY)
    {
        var map = GetMapByCountryId(countryId);
        
        int checkX = currentX;
        int checkY = currentY;
        
        switch (direction)
        {
            case Direction.North: checkY = -1; break;
            case Direction.South: checkY = map.Height; break;
            case Direction.East: checkX = map.Width; break;
            case Direction.West: checkX = -1; break;
        }
        
        // マップ端にあるGateタイプの人を探
        var gate = map.Locations.FirstOrDefault(l => 
            l.Type == "Gate" && 
            ((direction == Direction.North && l.Y == 0) ||
             (direction == Direction.South && l.Y == map.Height - 1) ||
             (direction == Direction.East && l.X == map.Width - 1) ||
             (direction == Direction.West && l.X == 0)));
        
        if (gate != null && !string.IsNullOrEmpty(gate.Connection))
        {
            return GetConnection(countryId, gate.X, gate.Y);
        }
        
        return null;
    }
    
    /// <summary>
    /// 全て利用可能なマップを取得
    /// </summary>
    public IEnumerable<Map> GetAllMaps() => allMaps.Values;
    
    private int GetIntValue(TomlTable table, string key, int defaultValue)
    {
        if (table.TryGetValue(key, out var value))
        {
            if (value is long l) return (int)l;
            if (value is int i) return i;
        }
        return defaultValue;
    }
    
    private string GetStringValue(TomlTable table, string key, string defaultValue)
    {
        if (table.TryGetValue(key, out var value))
        {
            return value?.ToString() ?? defaultValue;
        }
        return defaultValue;
    }
    
    /// <summary>
    /// 国のマップを取得（デフォルトは中立之城）
    /// </summary>
    public Map GetMapByCountryId(int countryId)
    {
        if (countryMaps.TryGetValue(countryId, out var map))
            return map;
            
        // デフォルトは中立之城 (id=5)
        if (countryMaps.TryGetValue(5, out var neutralMap))
            return neutralMap;
            
        // フォールバック
        return new Map
        {
            Id = 0,
            Name = "Unknown",
            Width = 4,
            Height = 4,
            Locations = new List<MapLocation>()
        };
    }
    
    /// <summary>
    /// 国のマップを座標で取得
    /// </summary>
    public MapLocation GetLocationByCoords(int countryId, int townX, int townY)
    {
        var map = GetMapByCountryId(countryId);
        
        // 町の座標 directly使用
        var location = map.Locations.FirstOrDefault(l => l.X == townX && l.Y == townY);
        
        if (location != null)
            return location;
            
        // フォールバック:  Generic location
        return new MapLocation
        {
            X = townX,
            Y = townY,
            Name = "不明な場所",
            Description = "ここはどこだろう...",
            Type = "field"
        };
    }
    
    /// <summary>
    /// 指定座標に移動（4方向）
    /// </summary>
    public MapLocation Move(int countryId, Direction direction, int currentX, int currentY)
    {
        var map = GetMapByCountryId(countryId);
        int newX = currentX;
        int newY = currentY;
        
        switch (direction)
        {
            case Direction.North:
                newY = currentY - 1;
                break;
            case Direction.South:
                newY = currentY + 1;
                break;
            case Direction.East:
                newX = currentX + 1;
                break;
            case Direction.West:
                newX = currentX - 1;
                break;
        }
        
        // マップの範囲内かチェック
        if (newX < -2 || newX > 2 || newY < -2 || newY > 2)
        {
            // 範囲外の場合は現在位置を返す
            return GetLocationByCoords(countryId, currentX, currentY);
        }
        
        return GetLocationByCoords(countryId, newX, newY);
    }
    
    /// <summary>
    /// 現在のマップを取得（旧バージョン互換性のため）
    /// </summary>
    public Map GetCurrentMap()
    {
        return GetMapByCountryId(5); // 中立之城
    }
    
    /// <summary>
    /// 指定座標を取得（旧バージョン互換性のため）
    /// </summary>
    public MapLocation GetLocation(int x, int y)
    {
        // 旧バージョンとの互換性: オフセット変換
        int mapX = x - MapOffset;
        int mapY = y - MapOffset;
        return GetLocationByCoords(5, mapX, mapY);
    }
    
    /// <summary>
    /// 移動（旧バージョン互換性のため）
    /// </summary>
    public MapLocation Move(Direction direction, int currentX, int currentY)
    {
        return Move(5, direction, currentX, currentY);
    }
}

public enum Direction
{
    North,
    South,
    East,
    West
}
