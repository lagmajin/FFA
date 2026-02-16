using System;
using System.Linq;
using FFA.Models;

namespace FFA.Services;

public class RandomEventService
{
    private const int CooldownMinutes = 30; // イベント発生クールダウン

    // ランダムイベントを引き起こす
    public EventTriggerResult TriggerEvent(string username)
    {
        try
        {
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new EventTriggerResult { Success = false, Message = "ユーザーが見つかりません" };

            // ランダムイベント取得
            var eventData = RandomEventDatabase.GetRandomEvent(user.Level);
            if (eventData == null)
                return new EventTriggerResult
                {
                    Success = true,
                    Message = "今日は特に何も起こらなかった...",
                    EventOccurred = false
                };

            // 奖励/惩罚获取
            var reward = RandomEventDatabase.GetReward(eventData.Type);

            // 奖励/惩罚適用
            if (reward.Gil != 0)
            {
                user.Gil += reward.Gil;
            }

            if (reward.Exp != 0)
            {
                user.Exp += reward.Exp;
            }

            if (!string.IsNullOrEmpty(reward.ItemName))
            {
                var item = new InventoryItem
                {
                    Name = reward.ItemName,
                    Type = "イベント",
                    Quantity = reward.Quantity,
                    Price = 100
                };
                userService.AddItemToUser(username, item);
            }

            userService.UpdateUser(user);

            // 結果を作成
            string resultMessage = reward.Gil > 0 
                ? $"✅ {reward.Message} ギル+{reward.Gil}"
                : reward.Gil < 0 
                    ? $"⚠️ {reward.Message} ギル{reward.Gil}"
                    : reward.Message;

            if (reward.Exp != 0)
            {
                resultMessage += $" Exp+{reward.Exp}";
            }

            return new EventTriggerResult
            {
                Success = true,
                Message = resultMessage,
                EventOccurred = true,
                EventName = eventData.Name,
                EventDescription = eventData.Description,
                EventType = eventData.Type
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("RandomEventService.TriggerEvent error: " + ex.Message);
            return new EventTriggerResult { Success = false, Message = "エラーが発生しました" };
        }
    }

    // 商店街の特別な商品を取得（イベント用）
    public SpecialSaleItem? GetSpecialSaleItem()
    {
        var random = new Random();
        if (random.NextDouble() > 0.3) // 30%の確率
            return null;

        var items = new[]
        {
            new SpecialSaleItem { Name = "强化石", Price = 500, Description = "装備を強化できる石" },
            new SpecialSaleItem { Name = "金 ключ", Price = 1000, Description = "特別な箱を開けられる" },
            new SpecialSaleItem { Name = "経験値の本", Price = 800, Description = "経験値を大量に得られる" },
            new SpecialSaleItem { Name = "転生チケット", Price = 5000, Description = "転生せずにレベルをリセットできる" },
            new SpecialSaleItem { Name = "raska", Price = 300, Description = "MPを回復する" },
        };

        return items[random.Next(items.Length)];
    }
}

/// <summary>
/// イベント結果
/// </summary>
public class EventTriggerResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public bool EventOccurred { get; set; }
    public string EventName { get; set; } = "";
    public string EventDescription { get; set; } = "";
    public RandomEventType EventType { get; set; }
}

/// <summary>
/// 特別セール商品
/// </summary>
public class SpecialSaleItem
{
    public string Name { get; set; } = "";
    public int Price { get; set; }
    public string Description { get; set; } = "";
}
