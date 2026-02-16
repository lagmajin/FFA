namespace FFA.Models;

/// <summary>
/// 鉱石タイプ
/// </summary>
public enum OreType
{
    Copper,      // 銅
    Iron,        // 鉄
    Silver,      // 銀
    Gold,        // 金
    Mithril,     // ミスリル
    Adamantite,  // アダマンタイト
    Orichalcum,  // オリハルコン
    Crystal,     // 結晶
    Diamond,     // ダイヤ
    Phoenix,     // 不死鳥の羽
}

/// <summary>
/// 鉱石
/// </summary>
public class Ore
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string JapaneseName { get; set; } = "";
    public OreType Type { get; set; }
    public int Value { get; set; } // 販売価格
    public int RequiredSkillLevel { get; set; } // 必要スキルレベル
    public double DropRate { get; set; } = 0.5; // ドロップ率
    public int MinQuantity { get; set; } = 1;
    public int MaxQuantity { get; set; } = 3;
}

/// <summary>
/// ユーザーの採掘スキル
/// </summary>
public class MiningSkill
{
    public string Username { get; set; } = "";
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int ExperienceToNext { get; set; } = 100;
    public int TotalOresMined { get; set; } = 0;
}

/// <summary>
/// 採掘スキルデータベース
/// </summary>
public static class MiningDatabase
{
    public static List<Ore> Ores { get; } = new List<Ore>
    {
        new Ore { Id = 1, Name = "Copper Ore", JapaneseName = "銅鉱石", Type = OreType.Copper, Value = 10, RequiredSkillLevel = 1, DropRate = 0.9, MinQuantity = 2, MaxQuantity = 5 },
        new Ore { Id = 2, Name = "Iron Ore", JapaneseName = "鉄鉱石", Type = OreType.Iron, Value = 25, RequiredSkillLevel = 5, DropRate = 0.8, MinQuantity = 1, MaxQuantity = 3 },
        new Ore { Id = 3, Name = "Silver Ore", JapaneseName = "銀鉱石", Type = OreType.Silver, Value = 50, RequiredSkillLevel = 10, DropRate = 0.7, MinQuantity = 1, MaxQuantity = 2 },
        new Ore { Id = 4, Name = "Gold Ore", JapaneseName = "金鉱石", Type = OreType.Gold, Value = 100, RequiredSkillLevel = 20, DropRate = 0.6, MinQuantity = 1, MaxQuantity = 2 },
        new Ore { Id = 5, Name = "Mithril Ore", JapaneseName = "ミスリル鉱石", Type = OreType.Mithril, Value = 250, RequiredSkillLevel = 30, DropRate = 0.5, MinQuantity = 1, MaxQuantity = 1 },
        new Ore { Id = 6, Name = "Adamantite Ore", JapaneseName = "アダマンタナイト鉱石", Type = OreType.Adamantite, Value = 500, RequiredSkillLevel = 40, DropRate = 0.4, MinQuantity = 1, MaxQuantity = 1 },
        new Ore { Id = 7, Name = "Orichalcum Ore", JapaneseName = "オリハルコン鉱石", Type = OreType.Orichalcum, Value = 1000, RequiredSkillLevel = 50, DropRate = 0.3, MinQuantity = 1, MaxQuantity = 1 },
        new Ore { Id = 8, Name = "Magic Crystal", JapaneseName = "魔法結晶", Type = OreType.Crystal, Value = 2000, RequiredSkillLevel = 60, DropRate = 0.25, MinQuantity = 1, MaxQuantity = 1 },
        new Ore { Id = 9, Name = "Diamond", JapaneseName = "ダイヤモンド", Type = OreType.Diamond, Value = 5000, RequiredSkillLevel = 75, DropRate = 0.15, MinQuantity = 1, MaxQuantity = 1 },
        new Ore { Id = 10, Name = "Phoenix Feather", JapaneseName = "不死鳥の羽", Type = OreType.Phoenix, Value = 10000, RequiredSkillLevel = 90, DropRate = 0.05, MinQuantity = 1, MaxQuantity = 1 },
    };

    /// <summary>
    /// スキルレベル对应的採掘可能鉱石を取得
    /// </summary>
    public static List<Ore> GetAvailableOres(int skillLevel)
    {
        return Ores.Where(o => o.RequiredSkillLevel <= skillLevel).ToList();
    }

    /// <summary>
    /// ランダムな鉱石を取得（スキルレベル制限）
    /// </summary>
    public static Ore? GetRandomOre(int skillLevel)
    {
        var available = GetAvailableOres(skillLevel);
        if (available.Count == 0) return null;

        // 重み付けランダム選択
        var random = new Random();
        var totalWeight = available.Sum(o => o.DropRate);
        var randomValue = random.NextDouble() * totalWeight;

        double cumulative = 0;
        foreach (var ore in available)
        {
            cumulative += ore.DropRate;
            if (randomValue <= cumulative)
                return ore;
        }

        return available.LastOrDefault();
    }
}
