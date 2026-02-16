namespace FFA.Models;

public class BiomeType
{
    public const string Town = "town";
    public const string Field = "field";
    public const string Forest = "forest";
    public const string Mountain = "mountain";
    public const string River = "river";
    public const string Desert = "desert";
    public const string Plains = "plains";
    public const string Swamp = "swamp";
    public const string SnowMountain = "snow_mountain";
    public const string Volcano = "volcano";
    public const string Beach = "beach";
    public const string Lake = "lake";
    public const string Cave = "cave";
    public const string Ruins = "ruins";
    public const string Dungeon = "dungeon";
    public const string SnowPlains = "snow_plains";
    public const string Jungle = "jungle";
}

public class MapLocation
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = ""; // "town", "field", "forest", "mountain", "river", "dungeon", "desert", "plains", "swamp", etc.
    public string Biome { get; set; } = ""; // バイオームタイプ（砂漠、平原、湿地など）
    public bool CanEnter { get; set; } = true;
    public List<string> Events { get; set; } = new List<string>();
    public int DangerLevel { get; set; } = 1; // 危険度 1-10
    public string[] Enemies { get; set; } = Array.Empty<string>(); // 出現する敵
    public string[] Resources { get; set; } = Array.Empty<string>(); // 採取可能的资源
    public bool IsDungeonEntrance { get; set; } = false; // ダンジョン入口か
    public int? ConnectedDungeonId { get; set; } = null; // 接続されたダンジョンのID
    public int RequiredLevel { get; set; } = 1; // 必要レベル
}

public class Map
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Width { get; set; } = 10;
    public int Height { get; set; } = 10;
    public List<MapLocation> Locations { get; set; } = new List<MapLocation>();
}