using System;
using FFA.Models;

namespace FFA.Services;

public class RockSmashService
{
    private readonly UserService _userService;
    private readonly Random _rand = new();

    public RockSmashService(UserService userService)
    {
        _userService = userService;
    }

    public SmashResult AttemptSmash(string username)
    {
        var user = _userService.GetByUsername(username);
        if (user == null)
            return new SmashResult { Success = false, Message = "ユーザーが見つかりません" };

        int weaponAttack = user.EquippedWeapon?.Attack ?? 1;
        int str = user.Status?.Str ?? 5;

        // 基礎ダメージ: 武器攻撃 + 力の倍率
        double baseDamage = weaponAttack + str * 2;
        // ばらつき（80%〜120%）
        double variance = _rand.Next(80, 121) / 100.0;
        int totalDamage = (int)Math.Round(baseDamage * variance);

        // 固定閾値（将来的に岩のランクで可変にする）
        int threshold = 100;

        bool success = totalDamage >= threshold;

        string rewardName = "";
        int rewardQty = 0;

        if (success)
        {
            // 報酬: 10%で希少な宝石、それ以外は岩の欠片 1〜3個
            if (_rand.Next(100) < 10)
            {
                rewardName = "希少な宝石";
                rewardQty = 1;
                var item = new InventoryItem
                {
                    Name = rewardName,
                    Type = "Accessory",
                    Quantity = 1,
                    Price = 500,
                    Effect = "貴重"
                };
                _userService.AddItemToUser(username, item);
                _userService.AdjustGil(username, 200);
            }
            else
            {
                rewardName = "岩の欠片";
                rewardQty = _rand.Next(1, 4);
                var item = new InventoryItem
                {
                    Name = rewardName,
                    Type = "Material",
                    Quantity = rewardQty,
                    Price = 10
                };
                _userService.AddItemToUser(username, item);
            }

            // 更新
            var updatedUser = _userService.GetByUsername(username);
            if (updatedUser != null)
                _userService.UpdateUser(updatedUser);
        }

        return new SmashResult
        {
            Success = success,
            Damage = totalDamage,
            Threshold = threshold,
            RewardName = rewardName,
            RewardQty = rewardQty,
            Message = success ? $"成功！{rewardName} x{rewardQty} を獲得しました。" : "破壊できませんでした。"
        };
    }
}

public class SmashResult
{
    public bool Success { get; set; }
    public int Damage { get; set; }
    public int Threshold { get; set; }
    public string RewardName { get; set; } = "";
    public int RewardQty { get; set; }
    public string Message { get; set; } = "";
}