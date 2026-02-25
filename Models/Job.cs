namespace FFA.Models;

using FFA.Services;

/// <summary>
/// 職業タイプ
/// </summary>
public enum Job
{
    // 基本職
    Warrior,
    Monk,
    WhiteMage,
    BlackMage,
    Ranger,
    Paladin,
    DarkKnight,
    Bard,
    Thief,
    Ninja,
    // 上級職
    HolyKnight,     // 聖騎士
    DeathKnight,   // 死騎
    ArchMage,      // 大魔道師
    BeastMaster,   // ビーストマスター
    Duelist,       // 剣豪
    Grandmaster,   // GrandMaster
    GraveRobber,   // 墓荒らし
    // _unique職
    Samurai,       // 侍
    Dragoon,       // 竜騎士
    Sage,          // 賢者
    Necromancer,   // ネクロマンサー
    Viking,        // バイキング
    Mystic,        // 神秘家
    Gambler,       // ギャンブラー
    Alchemist,     // 錬金術師
    Chronomancer,  // 時空魔術師
    Runemaster,    // ルーンマスター
}

/// <summary>
/// 職業詳細情報クラス
/// </summary>
public class JobInfo
{
    public Job Job { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "⚔️";
    public string Color { get; set; } = "#000000";
    public PlayerStatus BonusStatus { get; set; } = new();
    public List<string> Skills { get; set; } = new();
    // パッシブスキル（常時効果）
    public List<string> PassiveSkills { get; set; } = new();
    // 戦闘以外のパッシブスキル
    public List<NonCombatPassiveSkill> NonCombatPassiveSkills { get; set; } = new();
    public string Role { get; set; } = ""; // Tank, Healer, DPS, etc.
    public string WeaponType { get; set; } = "";
    public bool IsAdvanced { get; set; } = false; // 上級職かどうか
    public Job? RequiredJob { get; set; } = null; // 必要職業
    public int RequiredJobLevel { get; set; } = 0; // 必要職業レベル
    
    // 場所制限
    public bool IsLocationRestricted { get; set; } = false; // 場所制限があるか
    public List<string> RequiredLocations { get; set; } = new(); // 必要な場所（町名やエリア名）
    public string? LocationRestrictionMessage { get; set; } // 場所制限のメッセージ
}

/// <summary>
/// 職業データベース
/// </summary>
public static class JobDatabase
{
    public static List<JobInfo> Jobs { get; } = new List<JobInfo>
    {
        new JobInfo
        {
            Job = Job.Warrior,
            Name = "戦士",
            Description = "高い防御力と体力を持つ戦闘の主力。敵の攻撃を引きつけて仲間を守る。",
            Icon = "⚔️",
            Color = "#8B4513",
            Role = "坦克",
            WeaponType = "剣、斧、槍",
            BonusStatus = new PlayerStatus { Str = 5, Vit = 8 },
            Skills = new List<string> { "タンクスタンス", "挑発", "防衛姿勢" },
            PassiveSkills = new List<string> { "被ダメージ-5%（常時）", "挑発成功率+5%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.HPRegenBonus, 5),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.TravelSpeedBonus, 3)
            }
        },
        new JobInfo
        {
            Job = Job.Monk,
            Name = "武闘家",
            Description = "素手で戦う拳の使い手。高速な連撃と高い会心の一撃を得意とする。",
            Icon = "🥋",
            Color = "#FFD700",
            Role = "近接DPS",
            WeaponType = "拳套、指虎",
            BonusStatus = new PlayerStatus { Str = 7, Dex = 6, Vit = 4 },
            Skills = new List<string> { "連撃", "会心の一撃", "風の拳" },
            PassiveSkills = new List<string> { "クリティカル率+3%", "連撃時ダメージ+5%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.HPRegenBonus, 10),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.StaminaRegenBonus, 8)
            }
        },
        new JobInfo
        {
            Job = Job.WhiteMage,
            Name = "白魔道士",
            Description = "光の魔法を使う回復職。仲間の生命を守り、状態異常を回復する。",
            Icon = "🍀",
            Color = "#FFFFFF",
            Role = "回復",
            WeaponType = "杖、聖書",
            BonusStatus = new PlayerStatus { Int = 6 },
            Skills = new List<string> { "回復魔法", "聖なる光", "状態回復" },
            PassiveSkills = new List<string> { "回復効果+10%", "状態異常耐性+5%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.MPRegenBonus, 8),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.StatusRecoveryBonus, 10)
            }
        },
        new JobInfo
        {
            Job = Job.BlackMage,
            Name = "黒魔道士",
            Description = "闇の魔法を使う攻撃職。強力な元素魔法で敵を一掃する。",
            Icon = "⚫",
            Color = "#000000",
            Role = "魔法DPS",
            WeaponType = "魔杖、魔道書",
            BonusStatus = new PlayerStatus { Int = 8 },
            Skills = new List<string> { "火球術", "氷結魔法", "雷撃" },
            PassiveSkills = new List<string> { "魔法攻撃力+8%", "MP 回復速度+5%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.MPRegenBonus, 5),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.CraftingSuccessBonus, 5)
            }
        },
        new JobInfo
        {
            Job = Job.Ranger,
            Name = "レンジャー",
            Description = "弓と罠を使う遠距離攻撃職。自然界の力を使って戦う。",
            Icon = "🏹",
            Color = "#228B22",
            Role = "遠距離DPS",
            WeaponType = "弓、クロスボー",
            BonusStatus = new PlayerStatus { Dex = 8, Agi = 6 },
            Skills = new List<string> { "精密射撃", "罠", "野生の呼び声" },
            PassiveSkills = new List<string> { "遠距離会心率+5%", "移動速度+5%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.TravelSpeedBonus, 8),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.ExplorationFindBonus, 5),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.FishingSpeedBonus, 5)
            }
        },
        new JobInfo
        {
            Job = Job.Paladin,
            Name = "パラディン",
            Description = "聖なる力を持つ騎士。味方を守りながら戦う万能戦士。",
            Icon = "🛡️",
            Color = "#FFD700",
            Role = "ハイブリッド",
            WeaponType = "剣、斧",
            BonusStatus = new PlayerStatus { Str = 4, Vit = 6, Int = 3 },
            Skills = new List<string> { "聖光攻撃", "庇護", "ヒール" },
            PassiveSkills = new List<string> { "被ダメージ-3%", "味方回復効果+5%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.HPRegenBonus, 5),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.StatusRecoveryBonus, 5)
            },
            IsLocationRestricted = true,
            RequiredLocations = new List<string> { "聖堂", "光の神殿", "聖なる祭壇", "大聖堂" },
            LocationRestrictionMessage = "聖なる場所でのみ転職できます"
        },
        new JobInfo
        {
            Job = Job.DarkKnight,
            Name = "ダークナイト",
            Description = "闇の力を使う暗黒騎士。敵の生命力を奪う攻撃が可能。",
            Icon = "🗡️",
            Color = "#4B0082",
            Role = "アタッカー",
            WeaponType = "大剣、刀",
            BonusStatus = new PlayerStatus { Str = 7, Vit = 4 },
            Skills = new List<string> { "ダークインパクト", "生命吸収", "呪い" },
            PassiveSkills = new List<string> { "与ダメージ+5%", "HP 吸収効果+3%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.HPRegenBonus, 3),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.GoldBonus, 5)
            },
            IsLocationRestricted = true,
            RequiredLocations = new List<string> { "闇の神殿", "暗黒の祭壇", "魔界の門" },
            LocationRestrictionMessage = "闇の力が満ちる場所でのみ転職できます"
        },
        new JobInfo
        {
            Job = Job.Bard,
            Name = "吟遊詩人",
            Description = "音楽で味方を支援する職。バフと回復を提供する。",
            Icon = "🎵",
            Color = "#9370DB",
            Role = "サポート",
            WeaponType = "杖、楽器",
            BonusStatus = new PlayerStatus { Int = 5, Dex = 4 },
            Skills = new List<string> { "戦いの歌", "癒しの旋律", "rally" },
            PassiveSkills = new List<string> { "味方の攻撃力+3%（範囲）", "MP 回復効果+3%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.ShopSellPriceBonus, 10),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.ExperienceBonus, 5)
            }
        },
        new JobInfo
        {
            Job = Job.Thief,
            Name = "盗賊",
            Description = "高速で敵の隙を突く職。アイテム調達も得意。",
            Icon = "🗝️",
            Color = "#8B4513",
            Role = "速攻DPS",
            WeaponType = "短剣、ダガー",
            BonusStatus = new PlayerStatus { Agi = 8, Luk = 5 },
            Skills = new List<string> { "背中攻撃", "盗み", "毒攻撃" },
            PassiveSkills = new List<string> { "回避率+5%", "ドロップ率+3%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.DropRateBonus, 8),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.ShopSellPriceBonus, 5),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.TravelSpeedBonus, 5)
            }
        },
        new JobInfo
        {
            Job = Job.Ninja,
            Name = "忍者",
            Description = "隠密と忍術を使う職。多种多様なスキルを持つ。",
            Icon = "🥷",
            Color = "#000080",
            Role = "万能",
            WeaponType = "手里剣、刀",
            BonusStatus = new PlayerStatus { Str = 4, Dex = 6, Agi = 6 },
            Skills = new List<string> { "影分身", "手里剣", "煙玉" },
            PassiveSkills = new List<string> { "初手回避率+5%", "手裏剣ダメージ+4%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.TravelSpeedBonus, 10),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.EncounterRateAdjust, -15),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.DropRateBonus, 3)
            }
        },
        // 上級職
        new JobInfo
        {
            Job = Job.HolyKnight,
            Name = "聖騎士",
            Description = "光と闇の両方を操る伝説の騎士。パラディンの進化系。",
            Icon = "⚜️",
            Color = "#FFD700",
            Role = "究極Tank",
            WeaponType = "聖剣、盾",
            BonusStatus = new PlayerStatus { Str = 10, Vit = 10, Int = 5 },
            Skills = new List<string> { "神圣之光", "剛光", "天地葬送" },
            PassiveSkills = new List<string> { "被ダメージ-8%", "周囲味方の防御+3%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.HPRegenBonus, 8),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.StatusRecoveryBonus, 10),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.BankInterestBonus, 3)
            },
            IsAdvanced = true,
            RequiredJob = Job.Paladin,
            RequiredJobLevel = 20,
            IsLocationRestricted = true,
            RequiredLocations = new List<string> { "聖堂", "光の神殿", "聖なる祭壇", "大聖堂", "天界の門" },
            LocationRestrictionMessage = "光に満ちた聖なる場所でのみ転職できます"
        },
        new JobInfo
        {
            Job = Job.DeathKnight,
            Name = "死騎",
            Description = "死者の力を得た最強の暗黒騎士。ダークナイトの進化系。",
            Icon = "💀",
            Color = "#2F4F4F",
            Role = "暗黒戦士",
            WeaponType = "死亡剣",
            BonusStatus = new PlayerStatus { Str = 12, Vit = 8 },
            Skills = new List<string> { "屍剣", "死の舞踏", "冥府門" },
            PassiveSkills = new List<string> { "与ダメージ+7%", "復活時HP回復+10%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.HPRegenBonus, 5),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.GoldBonus, 10),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.DropRateBonus, 5)
            },
            IsAdvanced = true,
            RequiredJob = Job.DarkKnight,
            RequiredJobLevel = 20,
            IsLocationRestricted = true,
            RequiredLocations = new List<string> { "冥界の門", "死者の神殿", "闇の深淵" },
            LocationRestrictionMessage = "冥界に近い場所でのみ転職できます"
        },
        new JobInfo
        {
            Job = Job.ArchMage,
            Name = "大魔道師",
            Description = "全ての魔法を操る者。黒魔道士、白魔道士の両方を極めた者のみになれる。",
            Icon = "🔮",
            Color = "#8A2BE2",
            Role = "完全魔法",
            WeaponType = "魔杖王",
            BonusStatus = new PlayerStatus { Int = 15, Dex = 5 },
            Skills = new List<string> { "元素合一", "禁呪解放", "魔法之源" },
            PassiveSkills = new List<string> { "魔法威力+10%", "詠唱速度+5%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.MPRegenBonus, 15),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.CraftingSuccessBonus, 10),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.CraftingQualityBonus, 5)
            },
            IsAdvanced = true,
            RequiredJob = Job.BlackMage,
            RequiredJobLevel = 20
        },
        new JobInfo
        {
            Job = Job.BeastMaster,
            Name = "ビーストマスター",
            Description = "獣と契約し、共闘する者。レンジャーの進化系。",
            Icon = "🦁",
            Color = "#8B4513",
            Role = "獣戦士",
            WeaponType = "鞭、斧",
            BonusStatus = new PlayerStatus { Str = 8, Agi = 10 },
            Skills = new List<string> { "獣召喚", "野性化", "共有感知" },
            PassiveSkills = new List<string> { "ペット攻撃力+8%", "ペットHP+10%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.TravelSpeedBonus, 10),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.ExplorationFindBonus, 10),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.FishingSpeedBonus, 8),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.MiningSpeedBonus, 5)
            },
            IsAdvanced = true,
            RequiredJob = Job.Ranger,
            RequiredJobLevel = 20
        },
        new JobInfo
        {
            Job = Job.Duelist,
            Name = "剣豪",
            Description = "剣の達人中の達人。忍者の進化系。",
            Icon = "🌸",
            Color = "#FF69B4",
            Role = "剣術師",
            WeaponType = "日本刀",
            BonusStatus = new PlayerStatus { Str = 10, Agi = 12 },
            Skills = new List<string> { "一刀両断", "居合斬", "剣意" },
            PassiveSkills = new List<string> { "会心率+6%", "回避時反撃+3%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.TravelSpeedBonus, 12),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.CraftingQualityBonus, 8)
            },
            IsAdvanced = true,
            RequiredJob = Job.Ninja,
            RequiredJobLevel = 20
        },
        new JobInfo
        {
            Job = Job.Grandmaster,
            Name = "マスター",
            Description = "全ての武芸を極めた伝説の存在。どの職業からでもなっている。",
            Icon = "🌟",
            Color = "#FFD700",
            Role = "全能",
            WeaponType = "全武器",
            BonusStatus = new PlayerStatus { Str = 8, Vit = 8, Dex = 8, Int = 8 },
            Skills = new List<string> { " Ultimate", "全能の光", "Infinity" },
            PassiveSkills = new List<string> { "全能力+5%", "全属性耐性+3%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.ExperienceBonus, 10),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.GoldBonus, 10),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.DropRateBonus, 5),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.HPRegenBonus, 5),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.MPRegenBonus, 5)
            },
            IsAdvanced = true,
            RequiredJob = Job.Warrior,
            RequiredJobLevel = 30
        },
        new JobInfo
        {
            Job = Job.GraveRobber,
            Name = "墓荒らし",
            Description = "墓地を彷徨う暗黒の盗賊。死者の遺品を漁り、闇の力を操る。シーフのダーク進化系。",
            Icon = "⚰️",
            Color = "#4A4A4A",
            Role = "暗黒盗賊",
            WeaponType = "短剣、鎌",
            BonusStatus = new PlayerStatus { Dex = 10, Int = 6 },
            Skills = new List<string> { "死体漁り", "闇歩き", "魂の略奪" },
            PassiveSkills = new List<string> { "ドロップ率+5%", "闇属性耐性+10%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.DropRateBonus, 15),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.ExplorationFindBonus, 10),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.GoldBonus, 8)
            },
            IsAdvanced = true,
            RequiredJob = Job.Thief,
            RequiredJobLevel = 15,
            IsLocationRestricted = true,
            RequiredLocations = new List<string> { "墓地", "死者の神殿", "闇の深淵", "廃墟の墓所" },
            LocationRestrictionMessage = "死者が眠る場所でのみ転職できます"
        },
        // ユニーク職
        new JobInfo
        {
            Job = Job.Samurai,
            Name = "侍",
            Description = "古流の剣術を極めた elite戦士。刀からの凄まじいダメージと素早い攻撃が強み。",
            Icon = "🌸",
            Color = "#DC143C",
            Role = "剣士",
            WeaponType = "刀",
            BonusStatus = new PlayerStatus { Str = 12, Agi = 8 },
            Skills = new List<string> { "居合い", "二刀流", "抜刀術" },
            PassiveSkills = new List<string> { "会心率+5%", "攻撃速度+10%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.DropRateBonus, 10),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.TravelSpeedBonus, 10)
            },
            IsAdvanced = true,
            RequiredJob = Job.Duelist,
            RequiredJobLevel = 20
        },
        new JobInfo
        {
            Job = Job.Dragoon,
            Name = "竜騎士",
            Description = "空の王者。龍を Partnerにし、高所からの攻撃で敵を殲滅する。",
            Icon = "🐉",
            Color = "#4169E1",
            Role = "飛行戦士",
            WeaponType = "槍、弓",
            BonusStatus = new PlayerStatus { Str = 10, Agi = 10 },
            Skills = new List<string> { "ジャンプ攻撃", "龍召喚", "上空移動" },
            PassiveSkills = new List<string> { "対空攻撃+20%", "回避率+8%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.TravelSpeedBonus, 15),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.ExplorationFindBonus, 8)
            },
            IsAdvanced = true,
            RequiredJob = Job.Ranger,
            RequiredJobLevel = 20
        },
        new JobInfo
        {
            Job = Job.Sage,
            Name = "賢者",
            Description = "全ての魔法を Masterした知の Master。回復と攻撃の両方を扱える全能 MagicUser。",
            Icon = "📚",
            Color = "#9370DB",
            Role = "万能魔導士",
            WeaponType = "杖、本",
            BonusStatus = new PlayerStatus { Int = 15, Vit = 5 },
            Skills = new List<string> { "全魔法+", "賢者の知恵", "魔法反射" },
            PassiveSkills = new List<string> { "MP消費-15%", "魔法防御+15%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.MPRegenBonus, 10),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.StaminaRegenBonus, 15)
            },
            IsAdvanced = true,
            RequiredJob = Job.ArchMage,
            RequiredJobLevel = 25
        },
        new JobInfo
        {
            Job = Job.Necromancer,
            Name = "ネクロマンサー",
            Description = "死者の力 を操る禁術使い。死体を Familyに、魂を武器とする。",
            Icon = "💀",
            Color = "#2F4F4F",
            Role = "召喚術師",
            WeaponType = "杖、鎌",
            BonusStatus = new PlayerStatus { Int = 12, Vit = 8 },
            Skills = new List<string> { "死者召喚", "魂吸収", "蘇生術" },
            PassiveSkills = new List<string> { "召喚生物HP+20%", "闇魔法+10%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.DropRateBonus, 20),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.ExplorationFindBonus, 10)
            },
            IsAdvanced = true,
            RequiredJob = Job.DeathKnight,
            RequiredJobLevel = 20
        },
        new JobInfo
        {
            Job = Job.Viking,
            Name = "バイキング",
            Description = "北欧の勇士。斧と盾武装し、 Wildな攻撃で敵を粉砕する。",
            Icon = "🪓",
            Color = "#8B4513",
            Role = "戦士",
            WeaponType = "斧、盾",
            BonusStatus = new PlayerStatus { Str = 15, Vit = 5 },
            Skills = new List<string> { "猛斧", "盾防御", "怒りの嵐" },
            PassiveSkills = new List<string> { "斧攻撃力+15%", "防御力+10%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.DropRateBonus, 15),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.StaminaRegenBonus, 10)
            },
            IsAdvanced = true,
            RequiredJob = Job.Warrior,
            RequiredJobLevel = 15
        },
        new JobInfo
        {
            Job = Job.Mystic,
            Name = "神秘家",
            Description = "オカルトの力 を極めた者。 Fortuneと不幸を操る謎めいた存在。",
            Icon = "🔮",
            Color = "#9932CC",
            Role = "支援術師",
            WeaponType = "杖、オーブ",
            BonusStatus = new PlayerStatus { Int = 10, Luk = 10 },
            Skills = new List<string> { "運命の輪", "幸運", "不幸の呪い" },
            PassiveSkills = new List<string> { "Luck+20%", "全異常耐性+10%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.DropRateBonus, 20),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.StatusRecoveryBonus, 10)
            },
            IsAdvanced = true,
            RequiredJob = Job.Bard,
            RequiredJobLevel = 15
        },
        new JobInfo
        {
            Job = Job.Gambler,
            Name = "ギャンブラー",
            Description = "全てを運に委ねる HighRisk・HighReturnの職業。",
            Icon = "🎰",
            Color = "#FFD700",
            Role = "特殊",
            WeaponType = "カード、サイコロ",
            BonusStatus = new PlayerStatus { Luk = 15, Agi = 5 },
            Skills = new List<string> { "必勝", "運命のカード", "ダブル-or Nothing" },
            PassiveSkills = new List<string> { "ドロップ率+20%", "会心率+10%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.DropRateBonus, 20),
            },
            IsAdvanced = true,
            RequiredJob = Job.Thief,
            RequiredJobLevel = 10
        },
        new JobInfo
        {
            Job = Job.Alchemist,
            Name = "錬金術師",
            Description = "材料を Goldに変える Master。 craftingと経済のプロ。",
            Icon = "⚗️",
            Color = "#20B2AA",
            Role = "生産",
            WeaponType = "杖",
            BonusStatus = new PlayerStatus { Int = 8, Dex = 8, Vit = 4 },
            Skills = new List<string> { "錬金術", "賢者の石", "item変換" },
            PassiveSkills = new List<string> { "制作成功率+20%", "採集量+15%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.CraftingSuccessBonus, 20),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.MiningRareFindBonus, 15),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.GoldBonus, 10)
            },
            IsAdvanced = true,
            RequiredJob = Job.Monk,
            RequiredJobLevel = 10
        },
        new JobInfo
        {
            Job = Job.Chronomancer,
            Name = "時空魔術師",
            Description = "時間を操る Master。敵を Slowさせ、自分を Speedupさせる。",
            Icon = "⏳",
            Color = "#00CED1",
            Role = "時間操作",
            WeaponType = "杖、オーブ",
            BonusStatus = new PlayerStatus { Int = 12, Agi = 8 },
            Skills = new List<string> { "時間停止", "スロー", "時間逆流" },
            PassiveSkills = new List<string> { "行動速度+15%", "CT短縮+20%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.TravelSpeedBonus, 15),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.StaminaRegenBonus, 20)
            },
            IsAdvanced = true,
            RequiredJob = Job.ArchMage,
            RequiredJobLevel = 25
        },
        new JobInfo
        {
            Job = Job.Runemaster,
            Name = "ルーンマスター",
            Description = "古代の符文を操る者。符文武器で敵を弱点属性で攻撃する。",
            Icon = "🧜",
            Color = "#8A2BE2",
            Role = "符文戦士",
            WeaponType = "剣、斧",
            BonusStatus = new PlayerStatus { Str = 8, Int = 10, Vit = 2 },
            Skills = new List<string> { "符文付与", "属性解放", "ルーン魔法" },
            PassiveSkills = new List<string> { "属性攻撃+20%", "武器攻撃+10%" },
            NonCombatPassiveSkills = new List<NonCombatPassiveSkill>
            {
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.DropRateBonus, 20),
                NonCombatPassiveSkillHelper.CreateSkill(NonCombatPassiveType.GoldBonus, 10)
            },
            IsAdvanced = true,
            RequiredJob = Job.DarkKnight,
            RequiredJobLevel = 20
        }
    };

    /// <summary>
    /// 職業に対応する詳細情報を取得
    /// </summary>
    /// <param name="job">職業</param>
    /// <returns>職業詳細情報</returns>
    public static JobInfo GetJobInfo(Job job)
    {
        try
        {
            return Jobs.FirstOrDefault(j => j.Job == job) ?? Jobs[0];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobDatabase.GetJobInfo 例外: {ex.Message} - {ex.StackTrace}");
            return new JobInfo { Job = Job.Warrior, Name = "戦士", Icon = "⚔️", Color = "#8B4513" };
        }
    }

    /// <summary>
    /// 全職業の詳細情報を取得
    /// </summary>
    /// <returns>職業詳細情報のリスト</returns>
    public static List<JobInfo> GetAllJobs()
    {
        try
        {
            // 基本職 + Unique職を 병합
            var allJobs = new List<JobInfo>(Jobs);
            var uniqueJobService = new UniqueJobService();
            var uniqueJobs = uniqueJobService.GetAllUniqueJobs();
            allJobs.AddRange(uniqueJobs);
            return allJobs;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobDatabase.GetAllJobs 例外: {ex.Message} - {ex.StackTrace}");
            return new List<JobInfo>();
        }
    }
}
