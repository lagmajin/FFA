using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using FFA.Models;

namespace FFA.Services;

public class MiningService
{
    private readonly string _databasePath;
    private const int MiningCost = 10; // 採掘费用
    private const int MaxSkillLevel = 100;

    public MiningService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "mining.db");
    }

    // ユーザーの採掘スキルを取得
    public MiningSkill? GetSkill(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var skills = db.GetCollection<MiningSkill>("mining_skills");
            return skills.FindOne(s => s.Username == username);
        }
        catch (Exception ex)
        {
            Console.WriteLine("MiningService.GetSkill error: " + ex.Message);
            return null;
        }
    }

    // 採掘スキルを初期化（ユーザーが初めて採掘する場合）
    public MiningSkill InitializeSkill(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var skills = db.GetCollection<MiningSkill>("mining_skills");

            var existing = skills.FindOne(s => s.Username == username);
            if (existing != null) return existing;

            var newSkill = new MiningSkill
            {
                Username = username,
                Level = 1,
                Experience = 0,
                ExperienceToNext = 100,
                TotalOresMined = 0
            };
            skills.Insert(newSkill);
            return newSkill;
        }
        catch (Exception ex)
        {
            Console.WriteLine("MiningService.InitializeSkill error: " + ex.Message);
            return new MiningSkill { Username = username };
        }
    }

    // 採掘を行う
    public MiningResult Mine(string username)
    {
        try
        {
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new MiningResult { Success = false, Message = "ユーザーが見つかりません" };

            // 費用をチェック
            if (user.Gil < MiningCost)
                return new MiningResult { Success = false, Message = "ギルが不足しています" };

            // スキルを取得/初期化
            var skill = GetSkill(username);
            if (skill == null)
                skill = InitializeSkill(username);

            // 鉱石を取得
            var ore = MiningDatabase.GetRandomOre(skill.Level);
            if (ore == null)
                return new MiningResult { Success = false, Message = "採掘可能な鉱石がありません" };

            // 鉱石を入手
            var random = new Random();
            var quantity = random.Next(ore.MinQuantity, ore.MaxQuantity + 1);

            // 経験値を獲得
            int expGained = ore.Value * quantity / 2; // 価値の半分を経験値に
            skill.Experience += expGained;
            skill.TotalOresMined += quantity;

            // レベルアップチェック
            bool leveledUp = false;
            while (skill.Experience >= skill.ExperienceToNext && skill.Level < MaxSkillLevel)
            {
                skill.Experience -= skill.ExperienceToNext;
                skill.Level++;
                skill.ExperienceToNext = (int)(skill.ExperienceToNext * 1.2); // 20%増加
                leveledUp = true;
            }

            // アイテムをインベントリに追加
            var item = new InventoryItem
            {
                Name = ore.JapaneseName,
                Type = "鉱石",
                Quantity = quantity,
                Price = ore.Value
            };
            userService.AddItemToUser(username, item);

            // 費用を差し引く
            user.Gil -= MiningCost;
            userService.UpdateUser(user);

            // スキルを保存
            using var db = new LiteDatabase(_databasePath);
            var skills = db.GetCollection<MiningSkill>("mining_skills");
            skills.Upsert(skill);

            // ゲームイベントを記録
            var eventService = new GameEventService();
            eventService.LogMining(username, true, ore.JapaneseName);

            return new MiningResult
            {
                Success = true,
                Message = leveledUp 
                    ? $"{ore.JapaneseName}を{quantity}個採掘しました！スキルレベルが{skill.Level}に上がりました！"
                    : $"{ore.JapaneseName}を{quantity}個採掘しました",
                OreName = ore.JapaneseName,
                Quantity = quantity,
                SkillLevel = skill.Level,
                SkillExp = skill.Experience,
                ExpToNext = skill.ExperienceToNext,
                ExpGained = expGained,
                LeveledUp = leveledUp,
                Cost = MiningCost
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("MiningService.Mine error: " + ex.Message);
            return new MiningResult { Success = false, Message = "採掘中にエラーが発生しました" };
        }
    }

    // スキルレベルを取得
    public int GetSkillLevel(string username)
    {
        var skill = GetSkill(username);
        return skill?.Level ?? 1;
    }
}

/// <summary>
/// 採掘結果
/// </summary>
public class MiningResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string OreName { get; set; } = "";
    public int Quantity { get; set; }
    public int SkillLevel { get; set; }
    public int SkillExp { get; set; }
    public int ExpToNext { get; set; }
    public int ExpGained { get; set; }
    public bool LeveledUp { get; set; }
    public int Cost { get; set; }
}
