namespace FFA.Models;

/// <summary>
/// ランダムイベントタイプ
/// </summary>
public enum RandomEventType
{
    Merchant,       // 商人が現れる
    Treasure,       // 寶藏発見
    Monster,        //  몬스터 出現
    Traveler,       // 旅人が情報をくれる
    Beggar,         // 物乞いがいる
    Thief,          // 泥棒
    Salesman,       // 行商人が特別商品を扱う
    Festival,       // 祭り
    Storm,          // 嵐
    LuckyDay,       // 幸运の日
}

/// <summary>
/// ランダムイベント
/// </summary>
public class RandomEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public RandomEventType Type { get; set; }
    public int MinPlayerLevel { get; set; } = 1;
    public double OccurrenceRate { get; set; } = 0.1; // 発生確率
}

/// <summary>
/// イベント奖励/惩罚
/// </summary>
public class EventReward
{
    public string ItemName { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public int Gil { get; set; } = 0;
    public int Exp { get; set; } = 0;
    public string Message { get; set; } = "";
}

/// <summary>
/// ランダムイベントデータベース
/// </summary>
public static class RandomEventDatabase
{
    public static List<RandomEvent> Events { get; } = new List<RandomEvent>
    {
        // 良いイベント
        new RandomEvent { Id = 1, Name = "商人出現", Description = "珍しい商品を扱っている商人が...", Type = RandomEventType.Merchant, MinPlayerLevel = 1, OccurrenceRate = 0.15 },
        new RandomEvent { Id = 2, Name = "寶藏発見", Description = "地面に光るものが...", Type = RandomEventType.Treasure, MinPlayerLevel = 5, OccurrenceRate = 0.05 },
        new RandomEvent { Id = 3, Name = "旅人", Description = "旅人が情報をくれた...", Type = RandomEventType.Traveler, MinPlayerLevel = 1, OccurrenceRate = 0.1 },
        new RandomEvent { Id = 4, Name = "行商人", Description = "特別商品を持っている行商人が...", Type = RandomEventType.Salesman, MinPlayerLevel = 10, OccurrenceRate = 0.08 },
        new RandomEvent { Id = 5, Name = "祭り", Description = "今日は特別な祭りの日！", Type = RandomEventType.Festival, MinPlayerLevel = 1, OccurrenceRate = 0.03 },
        new RandomEvent { Id = 6, Name = "幸运の日", Description = "今日はついてる！", Type = RandomEventType.LuckyDay, MinPlayerLevel = 1, OccurrenceRate = 0.05 },

        // 中立イベント
        new RandomEvent { Id = 7, Name = "物乞丐", Description = "物が欲しそうな人が...", Type = RandomEventType.Beggar, MinPlayerLevel = 1, OccurrenceRate = 0.1 },

        // 悪いイベント
        new RandomEvent { Id = 8, Name = "モンスタ袭来", Description = "敌が襲いかかってきた！", Type = RandomEventType.Monster, MinPlayerLevel = 3, OccurrenceRate = 0.12 },
        new RandomEvent { Id = 9, Name = "泥棒", Description = "谁かが物を盗み始めた...", Type = RandomEventType.Thief, MinPlayerLevel = 5, OccurrenceRate = 0.08 },
        new RandomEvent { Id = 10, Name = "嵐", Description = "突然の嵐が...", Type = RandomEventType.Storm, MinPlayerLevel = 1, OccurrenceRate = 0.07 },
    };

    public static RandomEvent? GetRandomEvent(int playerLevel)
    {
        var available = Events.Where(e => e.MinPlayerLevel <= playerLevel).ToList();
        var random = new Random();
        var selected = available[random.Next(available.Count)];

        // 確率チェック
        if (random.NextDouble() > selected.OccurrenceRate)
            return null;

        return selected;
    }

    public static EventReward GetReward(RandomEventType type)
    {
        var random = new Random();
        return type switch
        {
            RandomEventType.Merchant => new EventReward
            {
                Gil = random.Next(50, 200),
                Message = "商人から買い物ができた！"
            },
            RandomEventType.Treasure => new EventReward
            {
                ItemName = "寶石",
                Quantity = random.Next(1, 5),
                Gil = random.Next(100, 500),
                Message = "寶藏を発見した！"
            },
            RandomEventType.Traveler => new EventReward
            {
                Exp = random.Next(20, 50),
                Message = "旅の情報を得た！"
            },
            RandomEventType.Salesman => new EventReward
            {
                ItemName = "特別药水",
                Quantity = random.Next(1, 3),
                Message = "行商人から珍しいものを購入した"
            },
            RandomEventType.Festival => new EventReward
            {
                Gil = random.Next(200, 500),
                Exp = random.Next(50, 100),
                Message = "祭りで楽しんだ！"
            },
            RandomEventType.LuckyDay => new EventReward
            {
                Gil = random.Next(100, 300),
                Exp = random.Next(30, 80),
                Message = "幸运が訪れた！"
            },
            RandomEventType.Beggar => new EventReward
            {
                Gil = -random.Next(10, 50),
                Message = "乞丐に施した..."
            },
            RandomEventType.Monster => new EventReward
            {
                Gil = -random.Next(20, 100),
                Exp = random.Next(10, 30),
                Message = "モンスタと戦った！"
            },
            RandomEventType.Thief => new EventReward
            {
                Gil = -random.Next(50, 200),
                Message = "泥棒に金を盗まれた！"
            },
            RandomEventType.Storm => new EventReward
            {
                Gil = -random.Next(10, 30),
                Message = "嵐で衣類が濡れた..."
            },
            _ => new EventReward { Message = "特に何も起こらなかった" }
        };
    }
}
