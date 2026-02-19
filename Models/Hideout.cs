namespace FFA.Models;

public enum HideoutType
{
    Cabin,      // 小屋
    House,      // 家
    Mansion,    // 豪宅
    Castle,     // 城
    Temple,     // 神殿
    Tower       // 塔
}

public enum HideoutUpgradeType
{
    Storage,    // 倉庫
    Garden,     // 庭園
    Workshop,   // 作業場
    TrainingRoom, // トレーニング部屋
    Library,    // 图书馆
    Shrine,     // 祠
    Farm        // 農場
}

public class Hideout
{
    public int Id { get; set; }
    public string OwnerUsername { get; set; } = "";
    public string Name { get; set; } = "";
    public HideoutType Type { get; set; } = HideoutType.Cabin;
    public int Level { get; set; } = 1;
    
    // 位置情報（フィーとルドIDと座標）
    public string? FieldId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    
    // ステータ
    public int MaxStorageSlots { get; set; } = 20;
    public int CurrentStorageItems { get; set; }
    
    // アップグレード
    public Dictionary<HideoutUpgradeType, int> Upgrades { get; set; } = new();
    
    // 最后的访问时间
    public DateTime LastVisitedUtc { get; set; } = DateTime.UtcNow;
    
    // 建設日
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    // 訪問者リスト
    public List<string> Visitors { get; set; } = new();
    public int TotalVisits { get; set; }
    
    // 装饰品
    public List<string> Decorations { get; set; } = new();
    
    // 农业生产用
    public List<HideoutCrop> Crops { get; set; } = new();
}

public class HideoutCrop
{
    public string CropId { get; set; } = "";
    public string CropName { get; set; } = "";
    public int PlantTimeUtc { get; set; }
    public int GrowthTimeSeconds { get; set; } = 3600; // 1时间默认
    public bool IsReady => (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds >= PlantTimeUtc + GrowthTimeSeconds;
    public int Yield { get; set; } = 1;
}
