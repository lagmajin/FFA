using FFA.Models;

namespace FFA.Services;

/// <summary>
/// ランダムイベントサービス
/// フィールド探索中にランダムイベントを発生させる
/// </summary>
public class RandomEventService
{
    private readonly Random _random = new();
    
    // イベント発生確率（探索1回あたり）
    private const double BaseEventRate = 0.15; // 15%
    
    /// <summary>
    /// ランダムイベントが発生するかどうか判定
    /// </summary>
    public bool ShouldTriggerEvent(int playerLevel, string fieldType)
    {
        // 基本確率
        double rate = BaseEventRate;
        
        // ダンジョンは確率アップ
        if (fieldType == "dungeon")
            rate *= 1.5;
        
        // 安全な地域は確率ダウン
        if (fieldType == "town" || fieldType == "village")
            rate *= 0.3;
        
        // レベルが高いほどイベント確立が上がる
        rate += (playerLevel * 0.005);
        
        return _random.NextDouble() < rate;
    }
    
    /// <summary>
    /// ランダムイベントを取得
    /// </summary>
    public RandomEventResult? GetRandomEvent(int playerLevel, string fieldType)
    {
        if (!ShouldTriggerEvent(playerLevel, fieldType))
            return null;
        
        // イベントタイプを選択（重み付け）
        var eventType = SelectEventType(fieldType);
        
        return GenerateEvent(eventType, playerLevel);
    }
    
    private RandomEventType SelectEventType(string fieldType)
    {
        // フィールドタイプによって発生するイベントの傾向を変える
        var possibleTypes = new List<RandomEventType>();
        
        switch (fieldType)
        {
            case "forest":
                possibleTypes.AddRange(new[] { 
                    RandomEventType.Treasure, RandomEventType.Treasure, 
                    RandomEventType.Monster, RandomEventType.Traveler,
                    RandomEventType.Herbalist, RandomEventType.Spirit 
                });
                break;
            case "mountain":
                possibleTypes.AddRange(new[] { 
                    RandomEventType.Treasure, RandomEventType.Treasure,
                    RandomEventType.Monster, RandomEventType.Treasure,
                    RandomEventType.Miner, RandomEventType.Avalanche 
                });
                break;
            case "desert":
                possibleTypes.AddRange(new[] { 
                    RandomEventType.Treasure, RandomEventType.Treasure,
                    RandomEventType.Monster, RandomEventType.Merchant,
                    RandomEventType.Sandstorm, RandomEventType.Oasis 
                });
                break;
            case "snow":
            case "ice":
                possibleTypes.AddRange(new[] { 
                    RandomEventType.Treasure,
                    RandomEventType.Monster, RandomEventType.Traveler,
                    RandomEventType.Avalanche, RandomEventType.FrozenTreasure 
                });
                break;
            case "dungeon":
                possibleTypes.AddRange(new[] { 
                    RandomEventType.Treasure, RandomEventType.Treasure, RandomEventType.Treasure,
                    RandomEventType.Monster, RandomEventType.Monster,
                    RandomEventType.Boss, RandomEventType.Trap, RandomEventType.Chest 
                });
                break;
            case "river":
            case "lake":
            case "ocean":
                possibleTypes.AddRange(new[] { 
                    RandomEventType.Treasure, RandomEventType.Merchant,
                    RandomEventType.Monster, RandomEventType.Fisher,
                    RandomEventType.Mermaid, RandomEventType.Storm 
                });
                break;
            default: // field, plains
                possibleTypes.AddRange(new[] { 
                    RandomEventType.Treasure,
                    RandomEventType.Monster, RandomEventType.Traveler,
                    RandomEventType.Merchant, RandomEventType.Beggar,
                    RandomEventType.FoundMoney, RandomEventType.LostItem 
                });
                break;
        }
        
        return possibleTypes[_random.Next(possibleTypes.Count)];
    }
    
    private RandomEventResult GenerateEvent(RandomEventType type, int playerLevel)
    {
        return type switch
        {
            // ===== 良いイベント =====
            RandomEventType.Treasure => GenerateTreasureEvent(playerLevel),
            RandomEventType.FoundMoney => GenerateFoundMoneyEvent(playerLevel),
            RandomEventType.Merchant => GenerateMerchantEvent(playerLevel),
            RandomEventType.Traveler => GenerateTravelerEvent(playerLevel),
            RandomEventType.Herbalist => GenerateHerbalistEvent(playerLevel),
            RandomEventType.Miner => GenerateMinerEvent(playerLevel),
            RandomEventType.Fisher => GenerateFisherEvent(playerLevel),
            RandomEventType.Oasis => GenerateOasisEvent(playerLevel),
            RandomEventType.FrozenTreasure => GenerateFrozenTreasureEvent(playerLevel),
            RandomEventType.Chest => GenerateChestEvent(playerLevel),
            RandomEventType.Mermaid => GenerateMermaidEvent(playerLevel),
            RandomEventType.Spirit => GenerateSpiritEvent(playerLevel),
            RandomEventType.LuckyDay => GenerateLuckyDayEvent(playerLevel),
            RandomEventType.Festival => GenerateFestivalEvent(),
            
            // ===== 中立イベント =====
            RandomEventType.Beggar => GenerateBeggarEvent(playerLevel),
            RandomEventType.LostItem => GenerateLostItemEvent(playerLevel),
            RandomEventType.Salesman => GenerateSalesmanEvent(playerLevel),
            
            // ===== 悪いイベント =====
            RandomEventType.Monster => GenerateMonsterEvent(playerLevel),
            RandomEventType.Boss => GenerateBossEvent(playerLevel),
            RandomEventType.Thief => GenerateThiefEvent(playerLevel),
            RandomEventType.Storm => GenerateStormEvent(),
            RandomEventType.Sandstorm => GenerateSandstormEvent(),
            RandomEventType.Avalanche => GenerateAvalancheEvent(),
            RandomEventType.Trap => GenerateTrapEvent(playerLevel),
            
            _ => GenerateDefaultEvent()
        };
    }
    
    // ===== 良いイベント生成 =====
    
    private RandomEventResult GenerateTreasureEvent(int level)
    {
        var treasures = new[]
        {
            ("古の宝箱", 500 + level * 50, "宝石", 1 + level / 5),
            ("隠し財宝", 300 + level * 30, "金塊", 1),
            ("捨てられた荷物", 100 + level * 20, "消耗品", 2 + level / 3),
            ("賢者の遺産", 1000 + level * 100, "秘宝", 1),
        };
        
        var (name, gil, item, qty) = treasures[_random.Next(treasures.Length)];
        
        return new RandomEventResult
        {
            Type = RandomEventType.Treasure,
            Title = "🎉 寶藏発見！",
            Message = $"地面を掘っていたら @{name} が見つかった！",
            GilReward = gil,
            ItemReward = item,
            ItemQuantity = qty,
            ExpReward = 20 + level * 5,
            IsPositive = true
        };
    }
    
    private RandomEventResult GenerateFoundMoneyEvent(int level)
    {
        int gil = _random.Next(10, 50) + level * 5;
        
        return new RandomEventResult
        {
            Type = RandomEventType.FoundMoney,
            Title = "💰 お金を発見！",
            Message = $"道端に @gil ギルが落ちているを見つけた！",
            GilReward = gil,
            ExpReward = 5 + level,
            IsPositive = true
        };
    }
    
    private RandomEventResult GenerateMerchantEvent(int level)
    {
        int gil = _random.Next(50, 150) + level * 10;
        
        return new RandomEventResult
        {
            Type = RandomEventType.Merchant,
            Title = "🏪 商人に出会った！",
            Message = $"旅商人が珍しいアイテムを売っている。@gil ギルで買い物ことができた。",
            GilReward = -gil,
            ItemReward = "特別商品",
            ItemQuantity = 1,
            ExpReward = 10 + level,
            IsPositive = true
        };
    }
    
    private RandomEventResult GenerateTravelerEvent(int level)
    {
        var tips = new[]
        {
            "この先の森に寶藏がある",
            "あの山登るなら準備をしてくれ",
            "今日の天気は変わるぞ",
            "この地域は夜になると危険だ",
            "近くの村で祭りが開催されている"
        };
        
        return new RandomEventResult
        {
            Type = RandomEventType.Traveler,
            Title = "🚶 旅人に出会った！",
            Message = $"旅人が教えてくれた: \"@tips[_random.Next(tips.Length)]\"",
            GilReward = 0,
            ExpReward = 15 + level * 2,
            IsPositive = true
        };
    }
    
    private RandomEventResult GenerateHerbalistEvent(int level)
    {
        var herbs = new[] { "草药", "魔法の薬草", "回復の草", "聖なる草" };
        
        return new RandomEventResult
        {
            Type = RandomEventType.Herbalist,
            Title = "🌿 薬草師に出会った！",
            Message = $"薬草師が珍しい @!herbs[_random.Next(herbs.Length)] をくれた！",
            ItemReward = herbs[_random.Next(herbs.Length)],
            ItemQuantity = 2 + level / 5,
            ExpReward = 10 + level,
            IsPositive = true
        };
    }
    
    private RandomEventResult GenerateMinerEvent(int level)
    {
        var ores = new[] { "鉄鉱石", "銀鉱石", "金鉱石", "水晶", "魔法の石" };
        
        return new RandomEventResult
        {
            Type = RandomEventType.Miner,
            Title = "⛏️ 坑夫に出会った！",
            Message = $"坑夫が採掘した @!ores[_random.Next(ores.Length)] をくれた！",
            ItemReward = ores[_random.Next(ores.Length)],
            ItemQuantity = 1 + level / 3,
            ExpReward = 15 + level,
            IsPositive = true
        };
    }
    
    private RandomEventResult GenerateFisherEvent(int level)
    {
        var fish = new[] { "鮭", "鮪", "たら", "サーモン", "幻の魚" };
        
        return new RandomEventResult
        {
            Type = RandomEventType.Fisher,
            Title = "🎣 漁師に出会った！",
            Message = $"漁師が釣った @!fish[_random.Next(fish.Length)] を分けてくれた！",
            ItemReward = fish[_random.Next(fish.Length)],
            ItemQuantity = 1 + level / 4,
            ExpReward = 10 + level,
            IsPositive = true
        };
    }
    
    private RandomEventResult GenerateOasisEvent(int level)
    {
        return new RandomEventResult
        {
            Type = RandomEventType.Oasis,
            Title = "🌴 オアシス発見！",
            Message = "砂漠でオアシスを見つけた！疲れが癒された！",
            HPHeal = 50 + level * 5,
            GilReward = 100 + level * 10,
            ExpReward = 20 + level * 2,
            IsPositive = true
        };
    }
    
    private RandomEventResult GenerateFrozenTreasureEvent(int level)
    {
        return new RandomEventResult
        {
            Type = RandomEventType.FrozenTreasure,
            Title = "❄️ 凍った寶藏！",
            Message = "氷の中に寶藏が凍っていた！小心に溶かして手に入れた！",
            GilReward = 500 + level * 50,
            ItemReward = "冰晶",
            ItemQuantity = 1 + level / 5,
            ExpReward = 30 + level * 3,
            IsPositive = true
        };
    }
    
    private RandomEventResult GenerateChestEvent(int level)
    {
        return new RandomEventResult
        {
            Type = RandomEventType.Chest,
            Title = "📦 宝箱発見！",
            Message = "ダンジョンの壁に隠された宝箱を見つけた！",
            GilReward = 200 + level * 30,
            ItemReward = "rare_item",
            ItemQuantity = 1,
            ExpReward = 25 + level * 3,
            IsPositive = true
        };
    }
    
    private RandomEventResult GenerateMermaidEvent(int level)
    {
        return new RandomEventResult
        {
            Type = RandomEventType.Mermaid,
            Title = "🧜 人魚が現れた！",
            Message = "人魚が願いを叶えてくれた！",
            GilReward = 1000 + level * 100,
            ExpReward = 50 + level * 5,
            IsPositive = true
        };
    }
    
    private RandomEventResult GenerateSpiritEvent(int level)
    {
        return new RandomEventResult
        {
            Type = RandomEventType.Spirit,
            Title = "👻 精霊現る！",
            Message = "森の精霊が優しく微笑んで消えた。良いことがあるかもしれない。",
            ExpReward = 100 + level * 10,
            IsPositive = true
        };
    }
    
    private RandomEventResult GenerateLuckyDayEvent(int level)
    {
        return new RandomEventResult
        {
            Type = RandomEventType.LuckyDay,
            Title = "🍀 Luck Day!",
            Message = "今日はついてる！全てが美味しく感じる！",
            GilReward = 300 + level * 30,
            ExpReward = 50 + level * 5,
            IsPositive = true
        };
    }
    
    private RandomEventResult GenerateFestivalEvent()
    {
        return new RandomEventResult
        {
            Type = RandomEventType.Festival,
            Title = "🎉 祭りに出逢った！",
            Message = "たまたま祭りに遭遇した！楽しい時間を過ごした！",
            GilReward = 500,
            ExpReward = 100,
            IsPositive = true
        };
    }
    
    // ===== 中立イベント =====
    
    private RandomEventResult GenerateBeggarEvent(int level)
    {
        int giveGil = _random.Next(10, 30) + level;
        
        return new RandomEventResult
        {
            Type = RandomEventType.Beggar,
            Title = "🙏 乞丐がいた",
            Message = $"乞丐に@giveGil ギルを与えた。",
            GilReward = -giveGil,
            ExpReward = 10 + level,
            IsPositive = true // 良い行い
        };
    }
    
    private RandomEventResult GenerateLostItemEvent(int level)
    {
        var items = new[] { "ハンカチ", "小さな袋", "メモ", "鍵", "指輪" };
        
        return new RandomEventResult
        {
            Type = RandomEventType.LostItem,
            Title = "📝 落とし物",
            Message = $"誰かが落とした @items[_random.Next(items.Length)] を拾った。",
            GilReward = 0,
            ExpReward = 5 + level,
            IsPositive = true
        };
    }
    
    private RandomEventResult GenerateSalesmanEvent(int level)
    {
        return new RandomEventResult
        {
            Type = RandomEventType.Salesman,
            Title = "🚚 行商人",
            Message = "特別商品を持っている行商人が通りました。",
            ItemReward = "行商人の品",
            ItemQuantity = 1,
            ExpReward = 5,
            IsPositive = true
        };
    }
    
    // ===== 悪いイベント =====
    
    private RandomEventResult GenerateMonsterEvent(int level)
    {
        var monsters = new[] { "ゴブリン", "スケルトン", "ウルフ", "蝙蝠", "スライム" };
        
        return new RandomEventResult
        {
            Type = RandomEventType.Monster,
            Title = "👹  몬스터出現！",
            Message = $"突然 @!monsters[_random.Next(monsters.Length)] が襲ってきた！",
            GilReward = -_random.Next(10, 30) - level,
            HPHeal = -_random.Next(10, 20) - level,
            ExpReward = 20 + level * 2,
            IsPositive = false
        };
    }
    
    private RandomEventResult GenerateBossEvent(int level)
    {
        var bosses = new[] { "エリアボス", "リッチ", "デーモン", "龍" };
        
        return new RandomEventResult
        {
            Type = RandomEventType.Boss,
            Title = "👿 ボス出現！",
            Message = $"強力な @!bosses[_random.Next(bosses.Length)] が現れた！",
            GilReward = -_random.Next(100, 300) - level * 10,
            HPHeal = -_random.Next(30, 50) - level * 3,
            ExpReward = 100 + level * 10,
            IsPositive = false
        };
    }
    
    private RandomEventResult GenerateThiefEvent(int level)
    {
        int stolen = _random.Next(20, 100) + level * 5;
        
        return new RandomEventResult
        {
            Type = RandomEventType.Thief,
            Title = "🦹 泥棒！",
            Message = $"泥棒に @!stolen ギル盗まれた！",
            GilReward = -stolen,
            ExpReward = 5,
            IsPositive = false
        };
    }
    
    private RandomEventResult GenerateStormEvent()
    {
        return new RandomEventResult
        {
            Type = RandomEventType.Storm,
            Title = "⛈️ 嵐！",
            Message = "突然の嵐に襲われた！",
            GilReward = -20,
            HPHeal = -10,
            IsPositive = false
        };
    }
    
    private RandomEventResult GenerateSandstormEvent()
    {
        return new RandomEventResult
        {
            Type = RandomEventType.Sandstorm,
            Title = "🌪️ 砂嵐！",
            Message = "砂漠で砂嵐に遭遇した！",
            GilReward = -30,
            HPHeal = -15,
            IsPositive = false
        };
    }
    
    private RandomEventResult GenerateAvalancheEvent()
    {
        return new RandomEventResult
        {
            Type = RandomEventType.Avalanche,
            Title = "🏔️ 雪崩！",
            Message = "雪山で雪崩に遭遇した！",
            GilReward = -50,
            HPHeal = -25,
            IsPositive = false
        };
    }
    
    private RandomEventResult GenerateTrapEvent(int level)
    {
        return new RandomEventResult
        {
            Type = RandomEventType.Trap,
            Title = "💣 トラップ！",
            Message = "罠を踏んでしまった！",
            HPHeal = -_random.Next(15, 30) - level,
            IsPositive = false
        };
    }
    
    private RandomEventResult GenerateDefaultEvent()
    {
        return new RandomEventResult
        {
            Type = RandomEventType.Treasure,
            Title = "✨ 特になにも起こらなかった",
            Message = "特に何も起こらなかったが、探索続けた。",
            ExpReward = 5,
            IsPositive = true
        };
    }
}

/// <summary>
/// ランダムイベントの結果
/// </summary>
public class RandomEventResult
{
    public RandomEventType Type { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public int GilReward { get; set; }
    public string ItemReward { get; set; } = "";
    public int ItemQuantity { get; set; }
    public int ExpReward { get; set; }
    public int HPHeal { get; set; }
    public bool IsPositive { get; set; }
}
