using LiteDB;

namespace FFA.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    // 所持金（ギル）
    public int Gil { get; set; } = 1000;
    // 銀行に預けているギル
    public int BankedGil { get; set; } = 0;
    // 旧貨幣（Old coin）
    public int OldCoin { get; set; } = 100;
    // Job
    public Job Job { get; set; } = Job.Warrior;
    // 国ID（所属国）
    public int? CountryId { get; set; }
    // 現在のクエスト
    public Quest? CurrentQuest { get; set; }
    // ギルドID
    public int? GuildId { get; set; }
    // プレミア通貨（例: Gems）
    public int Premium { get; set; } = 10;
    // Equipped items
    public Weapon? EquippedWeapon { get; set; }
    public Armor? EquippedArmor { get; set; }
    public Accessory? EquippedAccessory1 { get; set; }
    public Accessory? EquippedAccessory2 { get; set; }
    // 後方互換性のためのプロパティ
    public Accessory? EquippedAccessory { get; set; }

    // Hit points
    public int HP { get; set; } = 100;
    public int MaxHP { get; set; } = 100;

    // 経験値
    public int Exp { get; set; } = 0;

    // Inventory
    public List<InventoryItem> Inventory { get; set; } = new();

    // Abilities (active skills)
    public List<Ability> Abilities { get; set; } = new();
    
    // Status and leveling
    public PlayerStatus Status { get; set; } = new PlayerStatus();
    public int Level { get; set; } = 1;
    public int ExpToNext { get; set; } = 100;
    // Whether this user is the current Sky Arena champion
    public bool IsChampion { get; set; } = false;

    // 転生システム
    public int RebirthCount { get; set; } = 0; // 転生回数
    public int TotalLevel { get; set; } = 1; // 累計レベル（転生時にリセットされない）
    public int RebirthLevelRequired { get; set; } = 50; // 転生所需等级

    // マスターシステム
    public bool IsMaster { get; set; } = false; // マスターかどうか
    public int MasterLevel { get; set; } = 0; // マスターレベル
    public int MasterExp { get; set; } = 0; // マスター経験値
    public int MasterExpToNext { get; set; } = 1000; // 次マスターレベル所需経験値
    public int MaxMasterLevel { get; set; } = 10; // 最大マスターレベル

    // スキルシステム
    public int SkillPoints { get; set; } = 0; // スキルポイント
    public List<UserSkill> LearnedSkills { get; set; } = new(); // 習得済みスキル
    // 最後に接続したIPアドレス
    public string? LastIp { get; set; }
    
    // 最後にアクティブだった日時（放置報酬の計算で使用）
    public DateTime LastActiveUtc { get; set; } = DateTime.UtcNow;
    
    // 所属する世界/次元（表世界、裏世界など）
    public string CurrentWorld { get; set; } = "Main";
    
    // ワールドマップ上の位置
    public int MapX { get; set; } = 10;
    public int MapY { get; set; } = 7;
    public string CurrentMapId { get; set; } = "world";
}
