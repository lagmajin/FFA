using FFA.Models;

namespace FFA.Services;

public class MapService
{
    private List<Map> maps = new List<Map>();
    
    public MapService()
    {
        InitializeMaps();
    }
    
    private void InitializeMaps()
    {
        // 初期マップの作成
        var initialMap = new Map
        {
            Id = 1,
            Name = "初期マップ",
            Width = 10,
            Height = 10,
            Locations = new List<MapLocation>()
        };
        
        // マップの位置を初期化
        for (int y = 0; y < initialMap.Height; y++)
        {
            for (int x = 0; x < initialMap.Width; x++)
            {
                initialMap.Locations.Add(new MapLocation
                {
                    X = x,
                    Y = y,
                    Name = GetLocationName(x, y),
                    Description = GetLocationDescription(x, y),
                    Type = GetLocationType(x, y),
                    CanEnter = GetCanEnter(x, y),
                    Events = GetLocationEvents(x, y)
                });
            }
        }
        
        maps.Add(initialMap);
    }
    
    private string GetLocationName(int x, int y)
    {
        if (x == 5 && y == 5) return "町の広場";
        if (x == 3 && y == 3) return "森の入口";
        if (x == 7 && y == 7) return "山の道";
        if (x == 5 && y == 8) return "川のほとり";
        if (x == 1 && y == 1) return "洞窟の入口";
        
        return "不明な場所";
    }
    
    private string GetLocationDescription(int x, int y)
    {
        if (x == 5 && y == 5) return "町の中心部で、多くの人々が集まる場所です。";
        if (x == 3 && y == 3) return "深い森の入口。中には様々な動物が住んでいます。";
        if (x == 7 && y == 7) return "険しい山道。登ると山頂が見えます。";
        if (x == 5 && y == 8) return "清らかな川のほとり。魚が泳いでいます。";
        if (x == 1 && y == 1) return "暗い洞窟の入口。中には宝物が眠っているといわれています。";
        
        return "平凡な野原です。";
    }
    
    private string GetLocationType(int x, int y)
    {
        if (x == 5 && y == 5) return "town";
        if (x == 3 && y == 3) return "forest";
        if (x == 7 && y == 7) return "mountain";
        if (x == 5 && y == 8) return "river";
        if (x == 1 && y == 1) return "dungeon";
        
        return "field";
    }
    
    private bool GetCanEnter(int x, int y)
    {
        // すべての場所に入れるとする
        return true;
    }
    
    private List<string> GetLocationEvents(int x, int y)
    {
        var events = new List<string>();
        
        if (x == 3 && y == 3)
        {
            events.Add("森の中から何かが動いている音がする...");
            events.Add("木の下に小さな宝箱がある。");
        }
        if (x == 5 && y == 8)
        {
            events.Add("川に魚が泳いでいる。");
        }
        if (x == 1 && y == 1)
        {
            events.Add("洞窟の中から怪しい光が漏れている。");
        }
        
        return events;
    }
    
    public Map GetCurrentMap()
    {
        return maps.FirstOrDefault() ?? new Map();
    }
    
    public MapLocation GetLocation(int x, int y)
    {
        var map = GetCurrentMap();
        return map.Locations.FirstOrDefault(l => l.X == x && l.Y == y) ?? new MapLocation { X = x, Y = y, Name = "不明な場所", Description = "ここはどこだろう...", Type = "field" };
    }
    
    public MapLocation Move(Direction direction, int currentX, int currentY)
    {
        var map = GetCurrentMap();
        int newX = currentX;
        int newY = currentY;
        
        switch (direction)
        {
            case Direction.North:
                newY = Math.Max(0, currentY - 1);
                break;
            case Direction.South:
                newY = Math.Min(map.Height - 1, currentY + 1);
                break;
            case Direction.East:
                newX = Math.Min(map.Width - 1, currentX + 1);
                break;
            case Direction.West:
                newX = Math.Max(0, currentX - 1);
                break;
        }
        
        return GetLocation(newX, newY);
    }
}

public enum Direction
{
    North,
    South,
    East,
    West
}