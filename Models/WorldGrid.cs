namespace FFA.Models;

/// <summary>
/// 場所タイプ
/// </summary>
public enum LocationType
{
    Town,      // 街
    Field,     // 平原
    Forest,    // 森
    Mountain,  // 山
    Dungeon,   // ダンジョン
    Lake,      // 湖
    Desert,    // 砂漠
    Volcano,   // 火山
    Snow,      // 雪山
    Sea        // 海
}

/// <summary>
/// 世界/次元のタイプ
/// </summary>
public enum WorldType
{
    Main,      // メイン世界（表世界）
    Underworld // 裏世界
}

/// <summary>
/// 世界の場所（一つのグリッドセル）
/// </summary>
public class WorldLocation
{
    public WorldType World { get; set; } = WorldType.Main; // 所属する世界/次元
    public int X { get; set; }
    public int Y { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public LocationType Type { get; set; }
    // Optional country association (matches Country.Name)
    public string? CountryName { get; set; }
    public int? LocationId { get; set; } // 街IDやフィールドID
    public bool IsDiscovered { get; set; } = false; // 発見済みか
    public bool IsAccessible { get; set; } = true; // アクセス可能か
    public int RequiredLevel { get; set; } = 1; // 所需等级
    
    // 敵出現
    public string[] Enemies { get; set; } = Array.Empty<string>();
    public int EnemyLevel { get; set; } = 1;
    public int EnemyCount { get; set; } = 1; // 1回の遭遇で出る敵の数
    
    // ドロップ
    public string[] Drops { get; set; } = Array.Empty<string>();
    public int DropRate { get; set; } = 10; // ドロップ率%
    
    // 8方向への接続（東西南北 + 対角線）
    public (int X, int Y)? North { get; set; }
    public (int X, int Y)? South { get; set; }
    public (int X, int Y)? East { get; set; }
    public (int X, int Y)? West { get; set; }
    public (int X, int Y)? NorthEast { get; set; }
    public (int X, int Y)? NorthWest { get; set; }
    public (int X, int Y)? SouthEast { get; set; }
    public (int X, int Y)? SouthWest { get; set; }
}

/// <summary>
/// プレイヤーの位置情報
/// </summary>
public class PlayerPosition
{
    public string Username { get; set; } = "";
    public WorldType World { get; set; } = WorldType.Main; // 現在の所属世界
    public int X { get; set; }
    public int Y { get; set; }
    public DateTime LastMoveTime { get; set; } = DateTime.UtcNow;
    public int MovesRemaining { get; set; } = 10; // 1日の移動可能回数
    public DateTime LastResetDate { get; set; } = DateTime.UtcNow.Date;
}

/// <summary>
/// 移動結果
/// </summary>
public class MoveResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public WorldLocation? NewLocation { get; set; }
    public int MovesRemaining { get; set; }
    public bool EncounteredEnemies { get; set; }
    public string[]? EncounteredEnemyNames { get; set; }
}

/// <summary>
/// ワールドマップ情報
/// </summary>
public class WorldMapInfo
{
    public int Width { get; set; }
    public int Height { get; set; }
    public List<WorldLocation> Locations { get; set; } = new();
    public int PlayerX { get; set; }
    public int PlayerY { get; set; }
    public WorldLocation? CurrentLocation { get; set; }
    public int MovesRemaining { get; set; }
}

/// <summary>
/// 周辺の情報（プレイヤーの周りのセル）- 8方向
/// </summary>
public class SurroundingsInfo
{
    public WorldLocation? North { get; set; }
    public WorldLocation? South { get; set; }
    public WorldLocation? East { get; set; }
    public WorldLocation? West { get; set; }
    public WorldLocation? NorthEast { get; set; }
    public WorldLocation? NorthWest { get; set; }
    public WorldLocation? SouthEast { get; set; }
    public WorldLocation? SouthWest { get; set; }
    public int PlayerX { get; set; }
    public int PlayerY { get; set; }
    public string CurrentLocationName { get; set; } = "";
}
