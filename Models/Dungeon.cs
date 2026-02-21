namespace FFA.Models;

public enum DungeonType
{
    QuickBattle,   // 1回だけ戦闘して終了
    Exploration   //  floors を進んで深く探索
}

public class Dungeon
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string JapaneseName { get; set; } = "";
    public DungeonType Type { get; set; }
    public string Description { get; set; } = "";
    public int RecommendedLevel { get; set; } = 1;
    public int MaxFloors { get; set; } = 1;  // 探索ダンジョンの場合、最大フロア数
    public int CurrentFloor { get; set; } = 1;
    public int MonsterCountPerFloor { get; set; } = 3; // 1フロアあたりの戦闘回数
    public bool IsCompleted { get; set; }
    
    // 報酬
    public int FloorClearBonusGil { get; set; } = 100;
    public int FloorClearBonusExp { get; set; } = 50;
    public int DungeonClearBonusGil { get; set; } = 500;
    public int DungeonClearBonusExp { get; set; } = 300;
    
    // ドロップ補正
    public double DropBonus { get; set; } = 1.0;
    public double RareDropBonus { get; set; } = 1.0;
    public double ExpBonus { get; set; } = 1.0;
    public string SpecialDropType { get; set; } = "";
    
    // 出現モンスター
    public List<string> MonsterTypes { get; set; } = new();
}

public class ExplorationSession
{
    public string Username { get; set; } = "";
    public int DungeonId { get; set; }
    public string DungeonName { get; set; } = "";
    public int CurrentFloor { get; set; } = 1;
    public int MonstersDefeated { get; set; } = 0;
    public int TotalMonsters { get; set; } = 3;
    public int TotalGil { get; set; } = 0;
    public int TotalExp { get; set; } = 0;
    public List<string> Drops { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
