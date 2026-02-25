using System;
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
    
    // 装飾品スロット（タイプ別）
    // リング: 2個まで
    public Accessory? EquippedRing1 { get; set; }
    public Accessory? EquippedRing2 { get; set; }
    // アミュレット: 1個
    public Accessory? EquippedAmulet { get; set; }
    // イアリング: 2個まで
    public Accessory? EquippedEarring1 { get; set; }
    public Accessory? EquippedEarring2 { get; set; }
    // ブレスレット: 1個
    public Accessory? EquippedBracelet { get; set; }
    // ネックレス: 1個
    public Accessory? EquippedNecklace { get; set; }
    //  Belt: 1個
    public Accessory? EquippedBelt { get; set; }
    
    // 後方互換性のためのプロパティ（非推奨）
    [Obsolete("代わりにEquippedRing1を使用してください")]
    public Accessory? EquippedAccessory1 { get; set; }
    [Obsolete("代わりにEquippedRing2を使用してください")]
    public Accessory? EquippedAccessory2 { get; set; }
    [Obsolete("代わりに各タイプ別スロットを使用してください")]
    public Accessory? EquippedAccessory { get; set; }

    // 装备饰品帮助方法
    public List<Accessory> GetEquippedAccessories()
    {
        var accessories = new List<Accessory>();
        if (EquippedRing1 != null) accessories.Add(EquippedRing1);
        if (EquippedRing2 != null) accessories.Add(EquippedRing2);
        if (EquippedAmulet != null) accessories.Add(EquippedAmulet);
        if (EquippedEarring1 != null) accessories.Add(EquippedEarring1);
        if (EquippedEarring2 != null) accessories.Add(EquippedEarring2);
        if (EquippedBracelet != null) accessories.Add(EquippedBracelet);
        if (EquippedNecklace != null) accessories.Add(EquippedNecklace);
        if (EquippedBelt != null) accessories.Add(EquippedBelt);
        return accessories;
    }

    // 获取装备饰品的总属性加成
    public int GetTotalAccessoryStat(string statName)
    {
        var accessories = GetEquippedAccessories();
        int total = 0;
        foreach (var acc in accessories)
        {
            total += statName switch
            {
                "Attack" => acc.Attack,
                "Defense" => acc.Defense,
                "Magic" => acc.Magic,
                _ => 0
            };
        }
        return total;
    }

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
    
    // 称号システム
    public int? EquippedTitleId { get; set; } // 装備中の称号ID
    public List<int> OwnedTitleIds { get; set; } = new(); // 所有称号リスト
    
    // 釣りシステム
    public List<int> OwnedRodIds { get; set; } = new() { 1 }; // 所有釣り竿リスト（初期は竹の釣り竿）
    
    // 仲間システム（パーティシステム基盤）
    public List<int> CompanionIds { get; set; } = new(); // 所有仲間IDリスト
    public int MaxCompanions { get; set; } = 10; // 最大仲間所持数
    public int MaxPartySize { get; set; } = 3; // パーティ最大人数
}
