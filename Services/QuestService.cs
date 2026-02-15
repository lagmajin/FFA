namespace FFA.Services;

public class QuestService
{
    private static readonly Random _random = new();

    // ランダムクエスト生成
    public Models.Quest GenerateRandomQuest()
    {
        var quests = new[]
        {
            // 討伐クエスト
            new Models.Quest 
            { 
                Id = 1,
                Name = "スライム退治",
                Description = "村周辺のスライムを退治してほしい",
                Type = Models.QuestType.Slay,
                Target = "スライム",
                TargetCount = 3,
                RewardGil = 100,
                RewardExp = 50
            },
            new Models.Quest 
            { 
                Id = 2,
                Name = "ゴブリン掃討",
                Description = "ゴブリンの群れを退治してくれる勇士を求める",
                Type = Models.QuestType.Slay,
                Target = "ゴブリン",
                TargetCount = 5,
                RewardGil = 200,
                RewardExp = 100
            },
            new Models.Quest 
            { 
                Id = 3,
                Name = "オーク討伐",
                Description = "オークの拠点を壊滅させてほしい",
                Type = Models.QuestType.Slay,
                Target = "オーク",
                TargetCount = 3,
                RewardGil = 300,
                RewardExp = 150
            },
            new Models.Quest 
            { 
                Id = 4,
                Name = "スケルトン撃退",
                Description = "墓地のスケルトンを退治してほしい",
                Type = Models.QuestType.Slay,
                Target = "スケルトン",
                TargetCount = 5,
                RewardGil = 250,
                RewardExp = 120
            },
            // 収集クエスト
            new Models.Quest 
            { 
                Id = 5,
                Name = "草药採集",
                Description = "薬草を5個採集ってきてほしい",
                Type = Models.QuestType.Collect,
                Target = "草药",
                TargetCount = 5,
                RewardGil = 80,
                RewardExp = 30
            },
            new Models.Quest 
            { 
                Id = 6,
                Name = "骨収集",
                Description = "スケルトンの骨を採集してほしい",
                Type = Models.QuestType.Collect,
                Target = "骨",
                TargetCount = 3,
                RewardGil = 120,
                RewardExp = 60
            },
            new Models.Quest 
            { 
                Id = 7,
                Name = "毛皮調達",
                Description = "ウolfの毛皮を調達してほしい",
                Type = Models.QuestType.Collect,
                Target = "毛皮",
                TargetCount = 3,
                RewardGil = 150,
                RewardExp = 80
            },
            new Models.Quest 
            { 
                Id = 8,
                Name = "武器素材",
                Description = "オークの武器素材を調達してほしい",
                Type = Models.QuestType.Collect,
                Target = "盾牌",
                TargetCount = 2,
                RewardGil = 200,
                RewardExp = 100
            }
        };

        return quests[_random.Next(quests.Length)];
    }

    // クエスト進捗更新（敵倒した時）
    public void UpdateProgress(Models.User user, string? enemyName = null, string? itemName = null)
    {
        if (user.CurrentQuest == null || user.CurrentQuest.IsCompleted) return;

        var quest = user.CurrentQuest;

        if (quest.Type == Models.QuestType.Slay && enemyName != null)
        {
            if (quest.Target == enemyName)
            {
                quest.CurrentCount++;
                if (quest.CurrentCount >= quest.TargetCount)
                {
                    quest.IsCompleted = true;
                }
            }
        }
        else if (quest.Type == Models.QuestType.Collect && itemName != null)
        {
            // アイテム持っていたらか確認（インベントリ）
            var hasItem = user.Inventory.Any(i => i.Name == itemName);
            if (hasItem)
            {
                quest.CurrentCount = Math.Min(quest.TargetCount, 
                    user.Inventory.Where(i => i.Name == itemName).Sum(i => i.Quantity));
                if (quest.CurrentCount >= quest.TargetCount)
                {
                    quest.IsCompleted = true;
                }
            }
        }
    }

    // クエスト報酬受取
    public void ClaimReward(Models.User user)
    {
        if (user.CurrentQuest == null || !user.CurrentQuest.IsCompleted) return;

        user.Gil += user.CurrentQuest.RewardGil;
        user.Exp += user.CurrentQuest.RewardExp;
        
        // 収集クエストの場合、アイテム消費
        if (user.CurrentQuest.Type == Models.QuestType.Collect)
        {
            var targetItem = user.CurrentQuest.Target;
            var itemsToRemove = user.Inventory.Where(i => i.Name == targetItem).ToList();
            foreach (var item in itemsToRemove)
            {
                user.Inventory.Remove(item);
            }
        }

        // クエストクリア
        user.CurrentQuest = null;
    }
}
