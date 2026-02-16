namespace FFA.Models;

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
    Grandmaster,   //  GrandMaster
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
    public string Role { get; set; } = ""; // Tank, Healer, DPS, etc.
    public string WeaponType { get; set; } = "";
    public bool IsAdvanced { get; set; } = false; // 上級職かどうか
    public Job? RequiredJob { get; set; } = null; // 必要職業
    public int RequiredJobLevel { get; set; } = 0; // 必要職業レベル
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
            Skills = new List<string> { "タンクスタンス", "挑発", "防衛姿勢" }
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
            Skills = new List<string> { "連撃", "会心の一撃", "風の拳" }
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
            Skills = new List<string> { "回復魔法", "聖なる光", "状態回復" }
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
            Skills = new List<string> { "火球術", "氷結魔法", "雷撃" }
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
            Skills = new List<string> { "精密射撃", "罠", "野生の呼び声" }
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
            Skills = new List<string> { "聖光攻撃", "庇護", "ヒール" }
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
            Skills = new List<string> { "ダークインパクト", "生命吸収", "呪い" }
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
            Skills = new List<string> { "戦いの歌", "癒しの旋律", "rally" }
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
            Skills = new List<string> { "背中攻撃", "盗み", "毒攻撃" }
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
            Skills = new List<string> { "影分身", "手里剣", "煙玉" }
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
            IsAdvanced = true,
            RequiredJob = Job.Paladin,
            RequiredJobLevel = 20
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
            IsAdvanced = true,
            RequiredJob = Job.DarkKnight,
            RequiredJobLevel = 20
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
            IsAdvanced = true,
            RequiredJob = Job.Warrior,
            RequiredJobLevel = 30
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
            return Jobs;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobDatabase.GetAllJobs 例外: {ex.Message} - {ex.StackTrace}");
            return new List<JobInfo>();
        }
    }
}
