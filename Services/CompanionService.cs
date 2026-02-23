using FFA.Models;
using LiteDB;
using System.IO;

namespace FFA.Services;

/// <summary>
/// 仲間管理サービス（パーティシステム基盤）
/// 汎用傭兵とユニークキャラクターの2種類を管理
/// </summary>
public class CompanionService
{
    private readonly string _databasePath;
    private readonly Random _random = new();
    
    // 汎用傭兵テンプレート（職業別）
    private static readonly List<MercenaryTemplate> MercenaryTemplates = new()
    {
        // Common - 一般傭兵
        new() { Job = Job.Warrior, NamePrefix = "若き", Rarity = CompanionRarity.Common,
                BaseHP = 100, BaseAttack = 12, BaseDefense = 5, BaseMagic = 2, BaseSpeed = 8,
                HireCost = 100, DailyWage = 10, Icon = "⚔️", Color = "#8BC34A",
                DefaultSkills = new() { "斬撃", "防御" } },
        new() { Job = Job.Monk, NamePrefix = "修行中の", Rarity = CompanionRarity.Common,
                BaseHP = 90, BaseAttack = 8, BaseDefense = 6, BaseMagic = 5, BaseSpeed = 10,
                HireCost = 100, DailyWage = 10, Icon = "🧘", Color = "#CDDC39",
                DefaultSkills = new() { "打撃", "気合" } },
        new() { Job = Job.WhiteMage, NamePrefix = "見習い", Rarity = CompanionRarity.Common,
                BaseHP = 70, BaseAttack = 3, BaseDefense = 3, BaseMagic = 12, BaseSpeed = 8,
                HireCost = 150, DailyWage = 15, Icon = "✨", Color = "#E8F5E9",
                DefaultSkills = new() { "ヒール", "打撃" } },
        new() { Job = Job.BlackMage, NamePrefix = "見習い", Rarity = CompanionRarity.Common,
                BaseHP = 70, BaseAttack = 4, BaseDefense = 3, BaseMagic = 14, BaseSpeed = 9,
                HireCost = 150, DailyWage = 15, Icon = "🔮", Color = "#E1BEE7",
                DefaultSkills = new() { "ファイアボール", "打撃" } },
        new() { Job = Job.Thief, NamePrefix = "駆け出しの", Rarity = CompanionRarity.Common,
                BaseHP = 80, BaseAttack = 10, BaseDefense = 4, BaseMagic = 2, BaseSpeed = 15,
                HireCost = 120, DailyWage = 12, Icon = "🗡️", Color = "#FFF9C4",
                DefaultSkills = new() { "素早い攻撃", "盗む" } },
        
        // Uncommon - 熟練傭兵
        new() { Job = Job.Warrior, NamePrefix = "熟練の", Rarity = CompanionRarity.Uncommon,
                BaseHP = 130, BaseAttack = 18, BaseDefense = 8, BaseMagic = 3, BaseSpeed = 10,
                HireCost = 500, DailyWage = 30, Icon = "⚔️", Color = "#4CAF50",
                DefaultSkills = new() { "強斬撃", "防御", "気合" } },
        new() { Job = Job.Paladin, NamePrefix = "信仰深き", Rarity = CompanionRarity.Uncommon,
                BaseHP = 150, BaseAttack = 14, BaseDefense = 12, BaseMagic = 8, BaseSpeed = 7,
                HireCost = 600, DailyWage = 35, Icon = "🛡️", Color = "#81C784",
                DefaultSkills = new() { "聖なる一撃", "守護", "ヒール" } },
        new() { Job = Job.WhiteMage, NamePrefix = "熟練の", Rarity = CompanionRarity.Uncommon,
                BaseHP = 90, BaseAttack = 4, BaseDefense = 5, BaseMagic = 20, BaseSpeed = 10,
                HireCost = 550, DailyWage = 32, Icon = "✨", Color = "#C8E6C9",
                DefaultSkills = new() { "ヒール", "聖なる光", "浄化" } },
        new() { Job = Job.Ranger, NamePrefix = "熟練の", Rarity = CompanionRarity.Uncommon,
                BaseHP = 100, BaseAttack = 16, BaseDefense = 5, BaseMagic = 3, BaseSpeed = 18,
                HireCost = 520, DailyWage = 31, Icon = "🏹", Color = "#A5D6A7",
                DefaultSkills = new() { "狙撃", "回避", "連射" } },
        
        // Rare - ベテラン傭兵
        new() { Job = Job.DarkKnight, NamePrefix = "歴戦の", Rarity = CompanionRarity.Rare,
                BaseHP = 180, BaseAttack = 28, BaseDefense = 10, BaseMagic = 12, BaseSpeed = 9,
                HireCost = 2000, DailyWage = 80, Icon = "🌑", Color = "#388E3C",
                DefaultSkills = new() { "ダークスラッシュ", "恐怖の叫び", "呪いの鎧" } },
        new() { Job = Job.Ninja, NamePrefix = "熟練の", Rarity = CompanionRarity.Rare,
                BaseHP = 110, BaseAttack = 22, BaseDefense = 6, BaseMagic = 8, BaseSpeed = 25,
                HireCost = 2200, DailyWage = 85, Icon = "🥷", Color = "#43A047",
                DefaultSkills = new() { "影斬り", "分身", "煙幕" } },
        new() { Job = Job.Bard, NamePrefix = "吟遊の", Rarity = CompanionRarity.Rare,
                BaseHP = 100, BaseAttack = 10, BaseDefense = 6, BaseMagic = 18, BaseSpeed = 12,
                HireCost = 1800, DailyWage = 70, Icon = "🎵", Color = "#66BB6A",
                DefaultSkills = new() { "戦いの歌", "癒しの歌", "激励" } },
    };
    
    // ユニークキャラクターテンプレート
    private static readonly List<UniqueCharacterTemplate> UniqueCharacterTemplates = new()
    {
        // Epic - ユニーク
        new() { Name = "レオンハルト", Title = "剣聖", Job = Job.Warrior, Rarity = CompanionRarity.Epic,
                BaseHP = 250, BaseAttack = 45, BaseDefense = 18, BaseMagic = 5, BaseSpeed = 15,
                HireCost = 10000, DailyWage = 200, Icon = "⭐", Color = "#FF9800",
                Description = "剣の道を極めた伝説の剣士。その一撃は岩をも砕く。",
                DefaultSkills = new() { "剛剣", "居合斬り", "気合", "見切り" },
                UniqueSkillName = "秘剣・流星", UniqueSkillDescription = "敵全体に強力な斬撃", UniqueSkillPower = 80 },
        
        new() { Name = "セラフィナ", Title = "聖女", Job = Job.WhiteMage, Rarity = CompanionRarity.Epic,
                BaseHP = 180, BaseAttack = 5, BaseDefense = 12, BaseMagic = 50, BaseSpeed = 12,
                HireCost = 12000, DailyWage = 220, Icon = "👼", Color = "#FFC107",
                Description = "神の加護を受けた聖なる乙女。その癒しは奇跡と呼ばれる。",
                DefaultSkills = new() { "奇跡", "聖なる加護", "浄化の光", "復活" },
                UniqueSkillName = "女神の祈り", UniqueSkillDescription = "味方全体を全回復", UniqueSkillPower = 100 },
        
        new() { Name = "ヴォルカヌス", Title = "炎帝", Job = Job.BlackMage, Rarity = CompanionRarity.Epic,
                BaseHP = 160, BaseAttack = 8, BaseDefense = 10, BaseMagic = 55, BaseSpeed = 14,
                HireCost = 11000, DailyWage = 210, Icon = "🔥", Color = "#FF5722",
                Description = "炎の魔法を極めた大魔導士。その炎は全てを焼き尽くす。",
                DefaultSkills = new() { "メテオ", "炎の壁", "燃焼", "魔力増幅" },
                UniqueSkillName = "終末の炎", UniqueSkillDescription = "敵全体に炎属性の大ダメージ", UniqueSkillPower = 90 },
        
        new() { Name = "シャドウ", Title = "影の王", Job = Job.Ninja, Rarity = CompanionRarity.Epic,
                BaseHP = 140, BaseAttack = 40, BaseDefense = 8, BaseMagic = 15, BaseSpeed = 35,
                HireCost = 10500, DailyWage = 205, Icon = "🌑", Color = "#673AB7",
                Description = "影を操る暗殺者の頂点。その姿を見た者は生きて帰れない。",
                DefaultSkills = new() { "影渡り", "瞬殺", "暗闇", "分身" },
                UniqueSkillName = "影の国", UniqueSkillDescription = "敵全体を闇に包み大ダメージ", UniqueSkillPower = 75 },
        
        // Legendary - レジェンダリー
        new() { Name = "アルテミス", Title = "竜騎士", Job = Job.Grandmaster, Rarity = CompanionRarity.Legendary,
                BaseHP = 350, BaseAttack = 60, BaseDefense = 30, BaseMagic = 25, BaseSpeed = 20,
                HireCost = 50000, DailyWage = 500, Icon = "🐉", Color = "#F44336",
                Description = "ドラゴンと共に戦った伝説の騎士。その力は竜の如し。",
                DefaultSkills = new() { "ドラゴンブレス", "竜の爪", "飛翔", "破壊の咆哮" },
                UniqueSkillName = "竜神降臨", UniqueSkillDescription = "竜神の力で敵全体を壊滅", UniqueSkillPower = 150 },
        
        new() { Name = "エターナル", Title = "時の賢者", Job = Job.ArchMage, Rarity = CompanionRarity.Legendary,
                BaseHP = 200, BaseAttack = 10, BaseDefense = 15, BaseMagic = 80, BaseSpeed = 18,
                HireCost = 60000, DailyWage = 550, Icon = "⏳", Color = "#9C27B0",
                Description = "時を操る賢者。過去も未来も彼には等しく見える。",
                DefaultSkills = new() { "時間停止", "過去視", "未来予知", "時空断" },
                UniqueSkillName = "エタニティ", UniqueSkillDescription = "時を止めて敵全体を攻撃", UniqueSkillPower = 200 },
    };
    
    public CompanionService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "companions.db");
    }
    
    /// <summary>
    /// ユーザーの仲間リストを取得
    /// </summary>
    public List<Companion> GetUserCompanions(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var collection = db.GetCollection<Companion>("companions");
            return collection.Find(c => c.OwnerUsername == username).ToList();
        }
        catch
        {
            return new List<Companion>();
        }
    }
    
    /// <summary>
    /// 特定の仲間を取得
    /// </summary>
    public Companion? GetCompanion(int companionId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var collection = db.GetCollection<Companion>("companions");
            return collection.FindById(companionId);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// ユーザーのパーティを取得
    /// </summary>
    public Party GetUserParty(string username)
    {
        var companions = GetUserCompanions(username);
        var party = new Party { OwnerUsername = username };
        
        foreach (var companion in companions.Where(c => c.IsInParty).OrderBy(c => c.PartyPosition))
        {
            party.AddCompanion(companion.Id);
        }
        
        return party;
    }
    
    /// <summary>
    /// パーティメンバーを取得
    /// </summary>
    public List<Companion> GetPartyMembers(string username)
    {
        var companions = GetUserCompanions(username);
        return companions.Where(c => c.IsInParty).OrderBy(c => c.PartyPosition).ToList();
    }
    
    /// <summary>
    /// 仲間をパーティに追加
    /// </summary>
    public bool AddToParty(string username, int companionId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var collection = db.GetCollection<Companion>("companions");
            
            var companions = collection.Find(c => c.OwnerUsername == username && c.IsInParty).ToList();
            if (companions.Count >= 3) return false; // 最大3体
            
            var companion = collection.FindById(companionId);
            if (companion == null || companion.OwnerUsername != username) return false;
            
            companion.IsInParty = true;
            companion.PartyPosition = companions.Count;
            collection.Update(companion);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 仲間をパーティから外す
    /// </summary>
    public bool RemoveFromParty(string username, int companionId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var collection = db.GetCollection<Companion>("companions");
            
            var companion = collection.FindById(companionId);
            if (companion == null || companion.OwnerUsername != username) return false;
            
            var oldPosition = companion.PartyPosition;
            companion.IsInParty = false;
            companion.PartyPosition = -1;
            collection.Update(companion);
            
            // 残りのメンバーの位置を詰める
            var otherCompanions = collection.Find(c => c.OwnerUsername == username && c.IsInParty && c.PartyPosition > oldPosition).ToList();
            foreach (var other in otherCompanions)
            {
                other.PartyPosition--;
                collection.Update(other);
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// パーティの位置を入れ替え
    /// </summary>
    public bool SwapPartyPositions(string username, int companionId1, int companionId2)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var collection = db.GetCollection<Companion>("companions");
            
            var companion1 = collection.FindById(companionId1);
            var companion2 = collection.FindById(companionId2);
            
            if (companion1 == null || companion2 == null) return false;
            if (companion1.OwnerUsername != username || companion2.OwnerUsername != username) return false;
            if (!companion1.IsInParty || !companion2.IsInParty) return false;
            
            (companion1.PartyPosition, companion2.PartyPosition) = (companion2.PartyPosition, companion1.PartyPosition);
            
            collection.Update(companion1);
            collection.Update(companion2);
            
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 汎用傭兵を雇用
    /// </summary>
    public Companion? HireMercenary(string ownerUsername, Job job, CompanionRarity? forcedRarity = null)
    {
        var rarity = forcedRarity ?? RollMercenaryRarity();
        var templates = MercenaryTemplates.Where(t => t.Job == job && t.Rarity == rarity).ToList();
        
        if (!templates.Any())
        {
            templates = MercenaryTemplates.Where(t => t.Rarity == CompanionRarity.Common).ToList();
        }
        
        var template = templates[_random.Next(templates.Count)];
        return CreateMercenaryFromTemplate(ownerUsername, template);
    }
    
    /// <summary>
    /// ユニークキャラクターを雇用
    /// </summary>
    public Companion? HireUniqueCharacter(string ownerUsername, string uniqueName)
    {
        var template = UniqueCharacterTemplates.FirstOrDefault(t => t.Name == uniqueName);
        if (template == null) return null;
        
        return CreateUniqueFromTemplate(ownerUsername, template);
    }
    
    /// <summary>
    /// テンプレートから傭兵を作成
    /// </summary>
    public Companion CreateMercenaryFromTemplate(string ownerUsername, MercenaryTemplate template)
    {
        var jobName = template.Job.ToString();
        var name = string.IsNullOrEmpty(template.NamePrefix) 
            ? $"{jobName}の傭兵" 
            : $"{template.NamePrefix}{jobName}";
        
        var companion = new Companion
        {
            Name = name,
            OwnerUsername = ownerUsername,
            Type = CompanionType.Mercenary,
            Role = GetRoleFromJob(template.Job),
            Rarity = template.Rarity,
            Job = template.Job,
            JobName = jobName,
            IsUnique = false,
            BaseHP = template.BaseHP,
            BaseAttack = template.BaseAttack,
            BaseDefense = template.BaseDefense,
            BaseMagic = template.BaseMagic,
            BaseSpeed = template.BaseSpeed,
            HireCost = template.HireCost,
            DailyWage = template.DailyWage,
            Icon = template.Icon,
            Color = template.Color,
            CurrentHP = template.BaseHP,
            AcquiredAt = DateTime.UtcNow,
            HiredAt = DateTime.UtcNow
        };
        
        // デフォルトスキルを追加
        foreach (var skillName in template.DefaultSkills)
        {
            companion.Skills.Add(CreateDefaultSkill(skillName, companion.Role));
        }
        
        // 保存
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var collection = db.GetCollection<Companion>("companions");
            collection.Insert(companion);
        }
        catch
        {
            // エラーは無視
        }
        
        return companion;
    }
    
    /// <summary>
    /// テンプレートからユニークキャラクターを作成
    /// </summary>
    public Companion CreateUniqueFromTemplate(string ownerUsername, UniqueCharacterTemplate template)
    {
        var companion = new Companion
        {
            Name = template.Name,
            OwnerUsername = ownerUsername,
            Type = CompanionType.Unique,
            Role = GetRoleFromJob(template.Job),
            Rarity = template.Rarity,
            Job = template.Job,
            JobName = template.Job.ToString(),
            IsUnique = true,
            UniqueTitle = template.Title,
            BaseHP = template.BaseHP,
            BaseAttack = template.BaseAttack,
            BaseDefense = template.BaseDefense,
            BaseMagic = template.BaseMagic,
            BaseSpeed = template.BaseSpeed,
            HireCost = template.HireCost,
            DailyWage = template.DailyWage,
            Icon = template.Icon,
            Color = template.Color,
            Description = template.Description,
            CurrentHP = template.BaseHP,
            AcquiredAt = DateTime.UtcNow,
            HiredAt = DateTime.UtcNow,
            MaxSkills = 5 // ユニークはスキル枠が多い
        };
        
        // デフォルトスキルを追加
        foreach (var skillName in template.DefaultSkills)
        {
            companion.Skills.Add(CreateDefaultSkill(skillName, companion.Role));
        }
        
        // ユニークスキルを追加
        if (!string.IsNullOrEmpty(template.UniqueSkillName))
        {
            companion.UniqueSkill = new CompanionSkill
            {
                Name = template.UniqueSkillName,
                Description = template.UniqueSkillDescription,
                Type = CompanionSkillType.Active,
                Power = template.UniqueSkillPower,
                MPCost = template.UniqueSkillPower / 2,
                Icon = "⭐",
                IsUniqueSkill = true
            };
        }
        
        // 保存
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var collection = db.GetCollection<Companion>("companions");
            collection.Insert(companion);
        }
        catch
        {
            // エラーは無視
        }
        
        return companion;
    }
    
    /// <summary>
    /// 職業から役割を取得
    /// </summary>
    private CompanionRole GetRoleFromJob(Job job)
    {
        return job switch
        {
            Job.Warrior or Job.DarkKnight or Job.Ninja or Job.Thief => CompanionRole.Attacker,
            Job.Paladin or Job.Monk => CompanionRole.Defender,
            Job.WhiteMage => CompanionRole.Healer,
            Job.BlackMage or Job.Bard => CompanionRole.Support,
            _ => CompanionRole.Balanced
        };
    }
    
    /// <summary>
    /// 仲間を保存
    /// </summary>
    public void SaveCompanion(Companion companion)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var collection = db.GetCollection<Companion>("companions");
            collection.Update(companion);
        }
        catch
        {
            // エラーは無視
        }
    }
    
    /// <summary>
    /// 仲間に経験値を与える
    /// </summary>
    public bool GiveExperience(int companionId, int exp)
    {
        var companion = GetCompanion(companionId);
        if (companion == null) return false;
        
        var leveledUp = companion.AddExperience(exp);
        SaveCompanion(companion);
        
        return leveledUp;
    }
    
    /// <summary>
    /// 仲間の親密度を上げる
    /// </summary>
    public void IncreaseAffection(int companionId, int amount)
    {
        var companion = GetCompanion(companionId);
        if (companion == null) return;
        
        companion.IncreaseAffection(amount);
        SaveCompanion(companion);
    }
    
    /// <summary>
    /// 仲間を削除
    /// </summary>
    public bool DeleteCompanion(string username, int companionId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var collection = db.GetCollection<Companion>("companions");
            
            var companion = collection.FindById(companionId);
            if (companion == null || companion.OwnerUsername != username) return false;
            
            collection.Delete(companionId);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 傭兵のレアリティを抽選
    /// </summary>
    private CompanionRarity RollMercenaryRarity()
    {
        var roll = _random.NextDouble();
        
        // Common: 70%, Uncommon: 20%, Rare: 8%, Epic: 2%
        return roll switch
        {
            < 0.02 => CompanionRarity.Rare,
            < 0.10 => CompanionRarity.Uncommon,
            _ => CompanionRarity.Common
        };
    }
    
    /// <summary>
    /// 雇用可能な傭兵リストを取得
    /// </summary>
    public List<MercenaryTemplate> GetAvailableMercenaries()
    {
        return MercenaryTemplates.ToList();
    }
    
    /// <summary>
    /// 雇用可能なユニークキャラクターリストを取得
    /// </summary>
    public List<UniqueCharacterTemplate> GetAvailableUniqueCharacters()
    {
        return UniqueCharacterTemplates.ToList();
    }
    
    /// <summary>
    /// ユーザーの仲間所持数を取得
    /// </summary>
    public int GetCompanionCount(string username)
    {
        return GetUserCompanions(username).Count;
    }
    
    /// <summary>
    /// パーティの戦闘力を計算
    /// </summary>
    public int CalculatePartyPower(string username)
    {
        var members = GetPartyMembers(username);
        return members.Sum(m => m.Attack + m.Defense + m.Magic + m.Speed + (m.MaxHP / 10));
    }
    
    /// <summary>
    /// デフォルトスキルを作成
    /// </summary>
    private CompanionSkill CreateDefaultSkill(string name, CompanionRole role)
    {
        return name switch
        {
            // 攻撃系
            "斬撃" => new() { Name = name, Type = CompanionSkillType.Active, Power = 15, MPCost = 3, Icon = "⚔️" },
            "強斬撃" => new() { Name = name, Type = CompanionSkillType.Active, Power = 25, MPCost = 6, Icon = "⚔️" },
            "打撃" => new() { Name = name, Type = CompanionSkillType.Active, Power = 12, MPCost = 2, Icon = "👊" },
            "狙撃" => new() { Name = name, Type = CompanionSkillType.Active, Power = 20, MPCost = 5, Icon = "🏹" },
            "連射" => new() { Name = name, Type = CompanionSkillType.Active, Power = 10, MPCost = 4, Icon = "🏹", Description = "2回攻撃" },
            "ファイアボール" => new() { Name = name, Type = CompanionSkillType.Active, Power = 25, MPCost = 8, Icon = "🔥" },
            "ダークスラッシュ" => new() { Name = name, Type = CompanionSkillType.Active, Power = 35, MPCost = 12, Icon = "🌑" },
            "影斬り" => new() { Name = name, Type = CompanionSkillType.Active, Power = 30, MPCost = 10, Icon = "🌑" },
            "瞬殺" => new() { Name = name, Type = CompanionSkillType.Active, Power = 40, MPCost = 15, Icon = "🗡️", Description = "高威力・低命中" },
            "剛剣" => new() { Name = name, Type = CompanionSkillType.Active, Power = 35, MPCost = 10, Icon = "⚔️" },
            "居合斬り" => new() { Name = name, Type = CompanionSkillType.Active, Power = 45, MPCost = 12, Icon = "⚔️", Description = "先制攻撃" },
            "聖なる一撃" => new() { Name = name, Type = CompanionSkillType.Active, Power = 28, MPCost = 8, Icon = "✨" },
            "メテオ" => new() { Name = name, Type = CompanionSkillType.Active, Power = 50, MPCost = 20, Icon = "☄️", Description = "全体攻撃" },
            "時空断" => new() { Name = name, Type = CompanionSkillType.Active, Power = 60, MPCost = 25, Icon = "⏳" },
            
            // 防御系
            "防御" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 2, Icon = "🛡️", Description = "防御力アップ" },
            "守護" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 5, Icon = "🛡️", Description = "味方の防御アップ" },
            "見切り" => new() { Name = name, Type = CompanionSkillType.Passive, Power = 0, MPCost = 0, Icon = "👁️", Description = "回避率+15%" },
            
            // 回復系
            "ヒール" => new() { Name = name, Type = CompanionSkillType.Active, Power = 30, MPCost = 8, Icon = "💚", Description = "HP回復" },
            "聖なる光" => new() { Name = name, Type = CompanionSkillType.Active, Power = 50, MPCost = 15, Icon = "✨", Description = "全体HP回復" },
            "奇跡" => new() { Name = name, Type = CompanionSkillType.Active, Power = 100, MPCost = 30, Icon = "🌟", Description = "大回復" },
            "癒しの歌" => new() { Name = name, Type = CompanionSkillType.Active, Power = 25, MPCost = 8, Icon = "🎵", Description = "全体小回復" },
            
            // 支援系
            "素早い攻撃" => new() { Name = name, Type = CompanionSkillType.Active, Power = 10, MPCost = 2, Icon = "💨", Description = "素早い2回攻撃" },
            "気合" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 3, Icon = "💪", Description = "攻撃力アップ" },
            "激励" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 5, Icon = "📢", Description = "味方全体の攻撃アップ" },
            "戦いの歌" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 6, Icon = "🎵", Description = "味方全体の攻撃・防御アップ" },
            "浄化" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 5, Icon = "💧", Description = "状態異常回復" },
            "聖なる加護" => new() { Name = name, Type = CompanionSkillType.Passive, Power = 0, MPCost = 0, Icon = "👼", Description = "常時防御+10%" },
            "恐怖の叫び" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 8, Icon = "👻", Description = "敵の攻撃力ダウン" },
            "呪いの鎧" => new() { Name = name, Type = CompanionSkillType.Passive, Power = 0, MPCost = 0, Icon = "💀", Description = "被ダメージ時、敵に反撃" },
            "回避" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 3, Icon = "💨", Description = "回避率アップ" },
            "分身" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 8, Icon = "👥", Description = "回避率大幅アップ" },
            "煙幕" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 5, Icon = "💨", Description = "敵の命中率ダウン" },
            "暗闇" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 6, Icon = "🌑", Description = "敵全体の命中率ダウン" },
            "影渡り" => new() { Name = name, Type = CompanionSkillType.Passive, Power = 0, MPCost = 0, Icon = "🌑", Description = "常時回避+20%" },
            "盗む" => new() { Name = name, Type = CompanionSkillType.Active, Power = 5, MPCost = 2, Icon = "💰", Description = "攻撃+アイテム盗む" },
            "燃焼" => new() { Name = name, Type = CompanionSkillType.Active, Power = 15, MPCost = 6, Icon = "🔥", Description = "炎属性+継続ダメージ" },
            "魔力増幅" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 5, Icon = "🔮", Description = "魔法攻撃力アップ" },
            "過去視" => new() { Name = name, Type = CompanionSkillType.Passive, Power = 0, MPCost = 0, Icon = "👁️", Description = "敵の次の行動を予知" },
            "未来予知" => new() { Name = name, Type = CompanionSkillType.Passive, Power = 0, MPCost = 0, Icon = "🔮", Description = "回避率+25%" },
            "時間停止" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 30, Icon = "⏳", Description = "1ターン敵を行動不能に" },
            "復活" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 20, Icon = "💫", Description = "味方を蘇生" },
            "浄化の光" => new() { Name = name, Type = CompanionSkillType.Active, Power = 20, MPCost = 12, Icon = "✨", Description = "全体状態異常回復+小回復" },
            "ドラゴンブレス" => new() { Name = name, Type = CompanionSkillType.Active, Power = 50, MPCost = 20, Icon = "🐉", Description = "全体炎属性攻撃" },
            "竜の爪" => new() { Name = name, Type = CompanionSkillType.Active, Power = 40, MPCost = 15, Icon = "🦎", Description = "高威力単体攻撃" },
            "飛翔" => new() { Name = name, Type = CompanionSkillType.Active, Power = 0, MPCost = 5, Icon = "🦅", Description = "回避率アップ" },
            "破壊の咆哮" => new() { Name = name, Type = CompanionSkillType.Active, Power = 30, MPCost = 18, Icon = "📢", Description = "全体攻撃+防御ダウン" },
            
            _ => new() { Name = name, Type = CompanionSkillType.Active, Power = 10, MPCost = 3, Icon = "⚔️" }
        };
    }
}
