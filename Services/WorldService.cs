using FFA.Models;

namespace FFA.Services;

/// <summary>
/// ワールドサービス - 2Dグリッドベースの移動システム
/// </summary>
public class WorldService
{
    private readonly Dictionary<string, PlayerPosition> _playerPositions = new();
    private readonly WorldLocation[,] _grid;
    private readonly int _width = 10;
    private readonly int _height = 10;
    private readonly Random _random = new();
    
    // 1日の移動可能回数
    private const int DAILY_MOVES = 20;
    
    public WorldService()
    {
        _grid = new WorldLocation[_width, _height];
        InitializeWorld();
    }
    
    /// <summary>
    /// ワールドを初期化
    /// </summary>
    private void InitializeWorld()
    {
        // 中央に街を配置 (5, 5)
        _grid[5, 5] = new WorldLocation
        {
            X = 5, Y = 5,
            Name = "王都",
            Description = "各国の首都が集まる大きな都市",
            Type = LocationType.Town,
            LocationId = 1,
            IsDiscovered = true,
            IsAccessible = true,
            RequiredLevel = 1
        };
        
        // 街を中心として周囲にフィールドを配置
        // 北쪽 (5, 4) - 平原
        _grid[5, 4] = new WorldLocation
        {
            X = 5, Y = 4,
            Name = "緑の平原",
            Description = "穏やかな草原。初心者の冒険者でも安心して雰囲けます。",
            Type = LocationType.Field,
            LocationId = 1,
            Enemies = new[] { "スライム", "ウサギ", "蝶" },
            EnemyLevel = 1,
            EnemyCount = 1,
            Drops = new[] { "草药", "毛皮", "蝶の粉" },
            DropRate = 20,
            North = (5, 3),
            South = (5, 5),
            East = (6, 4),
            West = (4, 4)
        };
        
        // 北北 (5, 3) - 森
        _grid[5, 3] = new WorldLocation
        {
            X = 5, Y = 3,
            Name = "暗い森",
            Description = "木々が茂る暗い森。経験者がを迎えます。",
            Type = LocationType.Forest,
            LocationId = 2,
            Enemies = new[] { "ゴブリン", "狼", "毒キノコ" },
            EnemyLevel = 3,
            EnemyCount = 2,
            Drops = new[] { "木材", "毛皮", "毒キノコ", "短剣" },
            DropRate = 25,
            North = (5, 2),
            South = (5, 4),
            East = (6, 3),
            West = (4, 3)
        };
        
        // 北北北 (5, 2) - 山
        _grid[5, 2] = new WorldLocation
        {
            X = 5, Y = 2,
            Name = "凍った山",
            Description = "氷雪に覆われたの山脈。強い者がを迎えます。",
            Type = LocationType.Snow,
            LocationId = 5,
            Enemies = new[] { "雪だるま", "氷ゴーレム", "白狼" },
            EnemyLevel = 5,
            EnemyCount = 2,
            Drops = new[] { "氷の結晶", "毛皮", "雪の結晶" },
            DropRate = 30,
            RequiredLevel = 3,
            North = (5, 1),
            South = (5, 3),
            East = (6, 2),
            West = (4, 2)
        };
        
        // 北端 (5, 1) - 火山
        _grid[5, 1] = new WorldLocation
        {
            X = 5, Y = 1,
            Name = "溶岩火山",
            Description = "灼熱の山脈。最強の者がを迎えます。",
            Type = LocationType.Volcano,
            LocationId = 6,
            Enemies = new[] { "溶岩ゴーレム", "火霊", "炎Elemental" },
            EnemyLevel = 8,
            EnemyCount = 3,
            Drops = new[] { "溶岩の石", "火のエッセンス", "黒曜石" },
            DropRate = 35,
            RequiredLevel = 5,
            South = (5, 2)
        };
        
        // 東 (6, 5) - 湖
        _grid[6, 5] = new WorldLocation
        {
            X = 6, Y = 5,
            Name = "神秘的な湖",
            Description = "神秘的な湖。水の者が住んでいます。",
            Type = LocationType.Lake,
            LocationId = 3,
            Enemies = new[] { "水Elemental", "魚人", "アシガエル" },
            EnemyLevel = 4,
            EnemyCount = 2,
            Drops = new[] { "魚の肉", "真珠", "水のエッセンス" },
            DropRate = 25,
            North = (6, 4),
            South = (6, 6),
            East = (7, 5),
            West = (5, 5)
        };
        
        // 東東 (7, 5) - 砂漠
        _grid[7, 5] = new WorldLocation
        {
            X = 7, Y = 5,
            Name = "広大な砂漠",
            Description = "広大な砂漠。厳しい環境が待ち受けています。",
            Type = LocationType.Desert,
            LocationId = 4,
            Enemies = new[] { "サソリ", "トカゲ", "ミイラ" },
            EnemyLevel = 6,
            EnemyCount = 2,
            Drops = new[] { "サソリの針", "砂金", "骨", "盾牌" },
            DropRate = 30,
            RequiredLevel = 4,
            North = (7, 4),
            South = (7, 6),
            East = (8, 5),
            West = (6, 5)
        };
        
        // 東東東 (8, 5) - ダンジョン
        _grid[8, 5] = new WorldLocation
        {
            X = 8, Y = 5,
            Name = "古代のダンジョン",
            Description = "地下の迷宮。強力なモンスターで満ちています。",
            Type = LocationType.Dungeon,
            LocationId = 7,
            Enemies = new[] { "スケルトン", "リッチ", "オーク" },
            EnemyLevel = 10,
            EnemyCount = 3,
            Drops = new[] { "骨", "宝石", "盾牌", "古代の剣" },
            DropRate = 40,
            RequiredLevel = 7,
            West = (7, 5)
        };
        
        // 西 (4, 5) - 平原
        _grid[4, 5] = new WorldLocation
        {
            X = 4, Y = 5,
            Name = "風の平原",
            Description = "穏やかな風が吹き渡る草原。",
            Type = LocationType.Field,
            Enemies = new[] { "スライム", "ウサギ" },
            EnemyLevel = 1,
            EnemyCount = 1,
            Drops = new[] { "草药", "毛皮" },
            DropRate = 15,
            North = (4, 4),
            South = (4, 6),
            East = (5, 5),
            West = (3, 5)
        };
        
        // 西西 (3, 5) - 森
        _grid[3, 5] = new WorldLocation
        {
            X = 3, Y = 5,
            Name = "大な森",
            Description = "巨大な木々が茂る深い森。",
            Type = LocationType.Forest,
            Enemies = new[] { "ゴブリン", "狼" },
            EnemyLevel = 3,
            EnemyCount = 2,
            Drops = new[] { "木材", "毛皮", "短剣" },
            DropRate = 20,
            North = (3, 4),
            South = (3, 6),
            East = (4, 5),
            West = (2, 5)
        };
        
        // 南 (5, 6) - 平原
        _grid[5, 6] = new WorldLocation
        {
            X = 5, Y = 6,
            Name = "南の草原",
            Description = "南方の穏やかな草原。",
            Type = LocationType.Field,
            Enemies = new[] { "スライム", "蝶" },
            EnemyLevel = 2,
            EnemyCount = 1,
            Drops = new[] { "草药", "蝶の粉" },
            DropRate = 15,
            North = (5, 5),
            South = (5, 7),
            East = (6, 6),
            West = (4, 6)
        };
        
        // 南南 (5, 7) - 森
        _grid[5, 7] = new WorldLocation
        {
            X = 5, Y = 7,
            Name = "密林",
            Description = "密なジャングルのような森。",
            Type = LocationType.Forest,
            Enemies = new[] { "毒キノコ", "蛇", "虎" },
            EnemyLevel = 5,
            EnemyCount = 2,
            Drops = new[] { "毒キノコ", "毛皮", "牙" },
            DropRate = 25,
            RequiredLevel = 3,
            North = (5, 6),
            South = (5, 8),
            East = (6, 7),
            West = (4, 7)
        };
        
        // 南南南 (5, 8) - 海
        _grid[5, 8] = new WorldLocation
        {
            X = 5, Y = 8,
            Name = "透明度の高い海",
            Description = "美しい海。海洋生物が生息。",
            Type = LocationType.Sea,
            Enemies = new[] { "人魚", "カジキ", "ウナギ" },
            EnemyLevel = 7,
            EnemyCount = 2,
            Drops = new[] { "真珠", "魚肉", "珊瑚" },
            DropRate = 30,
            RequiredLevel = 5,
            North = (5, 7)
        };
        
        // 他の場所を発見済みに
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_grid[x, y] != null)
                {
                    _grid[x, y].IsDiscovered = true;
                }
            }
        }
    }
    
    /// <summary>
    /// プレイヤーの位置を取得（なければ初期位置）
    /// </summary>
    public PlayerPosition GetOrCreatePosition(string username)
    {
        if (!_playerPositions.ContainsKey(username))
        {
            // 初期位置は王都 (5, 5)
            _playerPositions[username] = new PlayerPosition
            {
                Username = username,
                X = 5,
                Y = 5,
                LastMoveTime = DateTime.UtcNow,
                MovesRemaining = DAILY_MOVES,
                LastResetDate = DateTime.UtcNow.Date
            };
        }
        
        // 日付が変わったら移動回数をリセット
        var pos = _playerPositions[username];
        if (pos.LastResetDate < DateTime.UtcNow.Date)
        {
            pos.MovesRemaining = DAILY_MOVES;
            pos.LastResetDate = DateTime.UtcNow.Date;
        }
        
        return pos;
    }
    
    /// <summary>
    /// プレイヤーを移動
    /// </summary>
    public MoveResult MovePlayer(string username, string direction)
    {
        var pos = GetOrCreatePosition(username);
        
        // 移動可能回数のチェック
        if (pos.MovesRemaining <= 0)
        {
            return new MoveResult
            {
                Success = false,
                Message = "今日の移動回数がありません",
                MovesRemaining = 0
            };
        }
        
        // 現在の位置から移動先を取得
        int newX = pos.X;
        int newY = pos.Y;
        
        switch (direction.ToLower())
        {
            case "north":
            case "北":
            case "n":
                newY--;
                break;
            case "south":
            case "南":
            case "s":
                newY++;
                break;
            case "east":
            case "東":
            case "e":
                newX++;
                break;
            case "west":
            case "西":
            case "w":
                newX--;
                break;
            default:
                return new MoveResult
                {
                    Success = false,
                    Message = "無効な方向です（north/south/east/west）",
                    MovesRemaining = pos.MovesRemaining
                };
        }
        
        // 範囲チェック
        if (newX < 0 || newX >= _width || newY < 0 || newY >= _height)
        {
            return new MoveResult
            {
                Success = false,
                Message = "その方向には行けません",
                MovesRemaining = pos.MovesRemaining
            };
        }
        
        // グリッドの存在チェック
        var newLocation = _grid[newX, newY];
        if (newLocation == null)
        {
            return new MoveResult
            {
                Success = false,
                Message = "その方向には行けません",
                MovesRemaining = pos.MovesRemaining
            };
        }
        
        // アクセス可能チェック
        if (!newLocation.IsAccessible)
        {
            return new MoveResult
            {
                Success = false,
                Message = "今はアクセスできません",
                MovesRemaining = pos.MovesRemaining
            };
        }
        
        // レベルチェック
        var userService = new UserService();
        var user = userService.GetByUsername(username);
        if (user != null && user.Level < newLocation.RequiredLevel)
        {
            return new MoveResult
            {
                Success = false,
                Message = $"レベル{newLocation.RequiredLevel}が必要です",
                MovesRemaining = pos.MovesRemaining
            };
        }
        
        // 移動成功
        pos.X = newX;
        pos.Y = newY;
        pos.MovesRemaining--;
        pos.LastMoveTime = DateTime.UtcNow;
        
        // 敵遭遇判定
        bool encounteredEnemies = false;
        string[]? enemyNames = null;
        
        if (newLocation.Type != LocationType.Town && newLocation.Enemies.Length > 0)
        {
            // 30%の確率で敵に遭遇
            if (_random.Next(100) < 30)
            {
                encounteredEnemies = true;
                enemyNames = new string[newLocation.EnemyCount];
                for (int i = 0; i < newLocation.EnemyCount; i++)
                {
                    enemyNames[i] = newLocation.Enemies[_random.Next(newLocation.Enemies.Length)];
                }
            }
        }
        
        return new MoveResult
        {
            Success = true,
            Message = encounteredEnemies 
                ? $"{newLocation.Name}に到着！但しかし敵が近づいてきた！"
                : $"{newLocation.Name}に到着した！",
            NewLocation = newLocation,
            MovesRemaining = pos.MovesRemaining,
            EncounteredEnemies = encounteredEnemies,
            EncounteredEnemyNames = enemyNames
        };
    }
    
    /// <summary>
    /// 現在の位置情報を取得
    /// </summary>
    public WorldMapInfo GetWorldMapInfo(string username)
    {
        var pos = GetOrCreatePosition(username);
        var location = _grid[pos.X, pos.Y];
        
        var info = new WorldMapInfo
        {
            Width = _width,
            Height = _height,
            PlayerX = pos.X,
            PlayerY = pos.Y,
            CurrentLocation = location,
            MovesRemaining = pos.MovesRemaining
        };
        
        // 発見済みの場所を追加
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_grid[x, y] != null && _grid[x, y].IsDiscovered)
                {
                    info.Locations.Add(_grid[x, y]);
                }
            }
        }
        
        return info;
    }
    
    /// <summary>
    /// 周辺の情報を取得
    /// </summary>
    public SurroundingsInfo GetSurroundings(string username)
    {
        var pos = GetOrCreatePosition(username);
        var current = _grid[pos.X, pos.Y];
        
        return new SurroundingsInfo
        {
            PlayerX = pos.X,
            PlayerY = pos.Y,
            CurrentLocationName = current?.Name ?? "不明",
            North = pos.Y > 0 ? _grid[pos.X, pos.Y - 1] : null,
            South = pos.Y < _height - 1 ? _grid[pos.X, pos.Y + 1] : null,
            East = pos.X < _width - 1 ? _grid[pos.X + 1, pos.Y] : null,
            West = pos.X > 0 ? _grid[pos.X - 1, pos.Y] : null
        };
    }
    
    /// <summary>
    /// 現在の位置の地名を取得
    /// </summary>
    public string GetCurrentLocationName(string username)
    {
        var pos = GetOrCreatePosition(username);
        var location = _grid[pos.X, pos.Y];
        return location?.Name ?? "不明";
    }
    
    /// <summary>
    /// 位置を変更（テレポート等）
    /// </summary>
    public void Teleport(string username, int x, int y)
    {
        var pos = GetOrCreatePosition(username);
        if (x >= 0 && x < _width && y >= 0 && y < _height && _grid[x, y] != null)
        {
            pos.X = x;
            pos.Y = y;
            pos.LastMoveTime = DateTime.UtcNow;
        }
    }
}
