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
}
