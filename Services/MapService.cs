using FFA.Models;
using Tomlyn;
using Tomlyn.Model;

namespace FFA.Services;

public class MapService
{
    private Dictionary<int, Map> countryMaps = new();
    private const int MapOffset = 4; // 町の座標(-4~4)をマップ座標(0~8)に変換

    /// <summary>読み込み診断情報</summary>
    public List<string> LoadDiagnostics { get; } = new();

    public MapService()
    {
        LoadCountryMaps();
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
                LoadDiagnostics.Add($"  {fileName}: hasMeta={hasMeta}, hasGrid={hasGrid}, keys=[{string.Join(",", table.Keys)}]");

                if (!hasMeta)
                {
                    LoadDiagnostics.Add($"  {fileName}: SKIP (no [meta])");
                    continue;
                }

                var meta = table["meta"] as TomlTable;
                if (meta == null)
                {
                    LoadDiagnostics.Add($"  {fileName}: SKIP (meta is {table["meta"]?.GetType().Name ?? "null"})");
                    continue;
                }

                int countryId = GetIntValue(meta, "country_id", 0);
                string countryName = GetStringValue(meta, "country_name",
                    GetStringValue(meta, "description", "Unknown"));

                var map = new Map
                {
                    Id = countryId,
                    Name = countryName,
                    Width = 4,
                    Height = 4,
                    Locations = new List<MapLocation>()
                };

                // グリッドデータを読み込み
                if (hasGrid)
                {
                    var gridRaw = table["grid"];
                    LoadDiagnostics.Add($"  {fileName}: grid type={gridRaw?.GetType().FullName}");

                    var gridArray = gridRaw as TomlArray;
                    if (gridArray != null)
                    {
                        LoadDiagnostics.Add($"  {fileName}: grid count={gridArray.Count}");
                        foreach (var item in gridArray)
                        {
                            var grid = item as TomlTable;
                            if (grid == null)
                            {
                                LoadDiagnostics.Add($"  {fileName}: grid item is {item?.GetType().Name ?? "null"}, not TomlTable");
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
                        }
                    }
                    else
                    {
                        LoadDiagnostics.Add($"  {fileName}: grid as TomlArray = null (actual type: {gridRaw?.GetType().FullName})");
                    }
                }

                countryMaps[countryId] = map;
                LoadDiagnostics.Add($"  {fileName}: Loaded countryId={countryId} name={countryName} locations={map.Locations.Count}");
            }
            catch (Exception ex)
            {
                LoadDiagnostics.Add($"  {fileName}: EXCEPTION: {ex.Message}");
                Console.WriteLine($"Failed to load map {file}: {ex.Message}");
            }
        }

        LoadDiagnostics.Add($"Total maps loaded: {countryMaps.Count} ({string.Join(", ", countryMaps.Keys)})");
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
