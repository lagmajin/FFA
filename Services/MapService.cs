using FFA.Models;
using Tomlyn;
using Tomlyn.Model;

namespace FFA.Services;

public class MapService
{
    private Dictionary<int, Map> countryMaps = new();
    private const int MapOffset = 4; // 町の座標(-4~4)をマップ座標(0~8)に変換
    
    public MapService()
    {
        LoadCountryMaps();
    }
    
    private void LoadCountryMaps()
    {
        // Data/Maps ディレクトリから国別マップを読み込み
        var mapsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Maps");
        
        // 開発時はプロジェクトルートからも読み込み
        if (!Directory.Exists(mapsPath))
        {
            mapsPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Maps");
        }
        
        if (!Directory.Exists(mapsPath))
            return;
            
        var files = Directory.GetFiles(mapsPath, "*_country_*.toml");
        
        foreach (var file in files)
        {
            try
            {
                var content = File.ReadAllText(file);
                var model = Toml.Parse(content);
                var table = model.ToModel();
                
                if (table.ContainsKey("meta"))
                {
                    var meta = table["meta"] as TomlTable;
                    if (meta != null)
                    {
                        int countryId = GetIntValue(meta, "country_id", 0);
                        string countryName = GetStringValue(meta, "description", "");
                        
                        var map = new Map
                        {
                            Id = countryId,
                            Name = countryName,
                            Width = 4,
                            Height = 4,
                            Locations = new List<MapLocation>()
                        };
                        
                        // グリッドデータを読み込み
                        if (table.ContainsKey("grid"))
                        {
                            var gridArray = table["grid"] as TomlArray;
                            if (gridArray != null)
                            {
                                foreach (var item in gridArray)
                                {
                                    var grid = item as TomlTable;
                                    if (grid == null) continue;
                                    
                                    var location = new MapLocation
                                    {
                                        X = GetIntValue(grid, "x", 0),
                                        Y = GetIntValue(grid, "y", 0),
                                        Name = GetStringValue(grid, "name", ""),
                                        Description = GetStringValue(grid, "description", ""),
                                        Type = GetStringValue(grid, "type", "field"),
                                        CanEnter = true,
                                        Events = new List<string>()
                                    };
                                    
                                    map.Locations.Add(location);
                                }
                            }
                        }
                        
                        countryMaps[countryId] = map;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load map {file}: {ex.Message}");
            }
        }
    }
    
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
