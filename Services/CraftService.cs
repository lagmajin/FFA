using FFA.Models;
using LiteDB;
using System.IO;

namespace FFA.Services;

/// <summary>
/// クラフトシステムのサービス
/// </summary>
public class CraftService
{
    private readonly string _databasePath;
    private readonly StaminaService _staminaService;
    private static readonly Random _random = new();
    private const int CraftStaminaCost = 1;

    public CraftService(StaminaService staminaService)
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "users.db");
        _staminaService = staminaService;
        InitializeData();
    }

    private LiteDatabase GetDatabase() => new LiteDatabase(_databasePath);

    private void InitializeData()
    {
        using var db = GetDatabase();
        var recipes = db.GetCollection<CraftRecipe>("craftRecipes");
        var materials = db.GetCollection<CraftMaterialInfo>("craftMaterials");

        if (recipes.Count() == 0)
        {
            recipes.Insert(new List<CraftRecipe>
            {
                new CraftRecipe
                {
                    Id = 1, Name = "回復薬", Description = "HPを50回復する", Category = CraftCategory.Consumable,
                    RequiredMaterials = new List<CraftMaterial> { new CraftMaterial { MaterialName = "薬草", MaterialType = "Material", Quantity = 2 } },
                    RequiredGil = 10, RequiredCraftLevel = 1,
                    ResultItem = new CraftResult { ItemName = "回復薬", ItemType = "Consumable", Quantity = 1, HealAmount = 50, SellPrice = 25 },
                    SuccessRate = 1.0, ExpReward = 5
                },
                new CraftRecipe
                {
                    Id = 2, Name = "高級回復薬", Description = "HPを200回復する", Category = CraftCategory.Consumable,
                    RequiredMaterials = new List<CraftMaterial>
                    {
                        new CraftMaterial { MaterialName = "薬草", MaterialType = "Material", Quantity = 5 },
                        new CraftMaterial { MaterialName = "魔法の粉", MaterialType = "Material", Quantity = 1 }
                    },
                    RequiredGil = 50, RequiredCraftLevel = 5,
                    ResultItem = new CraftResult { ItemName = "高級回復薬", ItemType = "Consumable", Quantity = 1, HealAmount = 200, SellPrice = 100 },
                    SuccessRate = 0.9, ExpReward = 15
                },
                new CraftRecipe
                {
                    Id = 3, Name = "鉄の剣", Description = "基本的な鉄の剣", Category = CraftCategory.Weapon,
                    RequiredMaterials = new List<CraftMaterial>
                    {
                        new CraftMaterial { MaterialName = "鉄鉱石", MaterialType = "Material", Quantity = 3 },
                        new CraftMaterial { MaterialName = "木材", MaterialType = "Material", Quantity = 1 }
                    },
                    RequiredGil = 100, RequiredCraftLevel = 2,
                    ResultItem = new CraftResult { ItemName = "鉄の剣", ItemType = "Weapon", Quantity = 1, Attack = 15, SellPrice = 200, Rarity = 1 },
                    SuccessRate = 0.85, ExpReward = 20
                },
                new CraftRecipe
                {
                    Id = 4, Name = "革の鎧", Description = "基本的な革の鎧", Category = CraftCategory.Armor,
                    RequiredMaterials = new List<CraftMaterial> { new CraftMaterial { MaterialName = "革", MaterialType = "Material", Quantity = 5 } },
                    RequiredGil = 80, RequiredCraftLevel = 2,
                    ResultItem = new CraftResult { ItemName = "革の鎧", ItemType = "Armor", Quantity = 1, Defense = 10, SellPrice = 150, Rarity = 1 },
                    SuccessRate = 0.9, ExpReward = 15
                },
                new CraftRecipe
                {
                    Id = 5, Name = "焼き魚", Description = "美味しい焼き魚", Category = CraftCategory.Cooking,
                    RequiredMaterials = new List<CraftMaterial> { new CraftMaterial { MaterialName = "川魚", MaterialType = "Fish", Quantity = 1 } },
                    RequiredGil = 5, RequiredCraftLevel = 1,
                    ResultItem = new CraftResult { ItemName = "焼き魚", ItemType = "Consumable", Quantity = 1, HealAmount = 30, SellPrice = 20, Rarity = 1 },
                    SuccessRate = 1.0, ExpReward = 3
                }
            });
        }

        if (materials.Count() == 0)
        {
            materials.Insert(new List<CraftMaterialInfo>
            {
                new CraftMaterialInfo { Id = 1, Name = "薬草", Description = "基本的な薬草", Rarity = 1, BasePrice = 5, Source = "採取" },
                new CraftMaterialInfo { Id = 2, Name = "魔法の粉", Description = "魔力を含んだ粉", Rarity = 2, BasePrice = 30, Source = "敵ドロップ" },
                new CraftMaterialInfo { Id = 3, Name = "鉄鉱石", Description = "鉄の鉱石", Rarity = 1, BasePrice = 20, Source = "採掘" },
                new CraftMaterialInfo { Id = 4, Name = "革", Description = "動物の皮", Rarity = 1, BasePrice = 15, Source = "敵ドロップ" },
                new CraftMaterialInfo { Id = 5, Name = "木材", Description = "一般的な木材", Rarity = 1, BasePrice = 10, Source = "採取" }
            });
        }
    }

    /// <summary>
    /// クラフトを実行
    /// </summary>
    public CraftResultInfo Craft(int userId, int recipeId, int quantity = 1)
    {
        using var db = GetDatabase();
        var recipes = db.GetCollection<CraftRecipe>("craftRecipes");
        var users = db.GetCollection<User>("users");

        var user = users.FindById(userId);
        if (user == null)
            return new CraftResultInfo { IsSuccess = false, Message = "ユーザーが見つかりません。" };

        var recipe = recipes.FindById(recipeId);
        if (recipe == null)
            return new CraftResultInfo { IsSuccess = false, Message = "レシピが見つかりません。" };

        // スタミナチェック
        int staminaCost = CraftStaminaCost * quantity;
        var staminaResult = _staminaService.UseStamina(user.Username, StaminaType.Crafting, staminaCost);
        if (!staminaResult.Success)
        {
            return new CraftResultInfo
            {
                IsSuccess = false,
                Message = staminaResult.Message,
                StaminaInfo = new CraftStaminaInfo
                {
                    Current = staminaResult.Remaining,
                    Max = staminaResult.MaxStamina,
                    NextRecovery = staminaResult.RecoverAt
                }
            };
        }

        // 素材チェック
        foreach (var material in recipe.RequiredMaterials)
        {
            int required = material.Quantity * quantity;
            int have = user.Inventory
                .Where(i => i.Name == material.MaterialName && i.Type == material.MaterialType)
                .Sum(i => i.Quantity);
            
            if (have < required)
                return new CraftResultInfo { IsSuccess = false, Message = $"{material.MaterialName}が不足しています。" };
        }

        // ゴールドチェック
        int requiredGil = recipe.RequiredGil * quantity;
        if (user.Gil < requiredGil)
            return new CraftResultInfo { IsSuccess = false, Message = "ゴールドが不足しています。" };

        // 成功判定
        if (_random.NextDouble() > recipe.SuccessRate)
        {
            // 失敗
            ConsumeMaterials(user, recipe, quantity);
            user.Gil -= requiredGil;
            users.Update(user);
            return new CraftResultInfo { IsSuccess = false, Message = "クラフトに失敗した...", ExpGained = recipe.ExpReward / 2 };
        }

        // 成功
        ConsumeMaterials(user, recipe, quantity);
        user.Gil -= requiredGil;

        var item = new InventoryItem
        {
            Name = recipe.ResultItem.ItemName,
            Type = recipe.ResultItem.ItemType,
            Quantity = recipe.ResultItem.Quantity * quantity,
            Price = recipe.ResultItem.SellPrice,
            Attack = recipe.ResultItem.Attack,
            Defense = recipe.ResultItem.Defense,
            Effect = recipe.ResultItem.Effect ?? ""
        };

        var existing = user.Inventory.FirstOrDefault(i => i.Name == item.Name && i.Type == item.Type);
        if (existing != null)
            existing.Quantity += item.Quantity;
        else
            user.Inventory.Add(item);

        users.Update(user);

        return new CraftResultInfo
        {
            IsSuccess = true,
            Message = $"{recipe.ResultItem.ItemName} x{quantity}を作成した！",
            CreatedItem = recipe.ResultItem,
            ExpGained = recipe.ExpReward * quantity
        };
    }

    private void ConsumeMaterials(User user, CraftRecipe recipe, int quantity)
    {
        foreach (var material in recipe.RequiredMaterials)
        {
            int remaining = material.Quantity * quantity;
            var items = user.Inventory
                .Where(i => i.Name == material.MaterialName && i.Type == material.MaterialType)
                .ToList();

            foreach (var item in items)
            {
                if (item.Quantity <= remaining)
                {
                    remaining -= item.Quantity;
                    user.Inventory.Remove(item);
                }
                else
                {
                    item.Quantity -= remaining;
                    remaining = 0;
                }
                if (remaining <= 0) break;
            }
        }
    }

    public List<CraftRecipe> GetAllRecipes()
    {
        using var db = GetDatabase();
        return db.GetCollection<CraftRecipe>("craftRecipes").FindAll().ToList();
    }

    public List<CraftRecipe> GetRecipesByCategory(CraftCategory category)
    {
        using var db = GetDatabase();
        return db.GetCollection<CraftRecipe>("craftRecipes").Find(r => r.Category == category).ToList();
    }

    public List<CraftMaterialInfo> GetAllMaterials()
    {
        using var db = GetDatabase();
        return db.GetCollection<CraftMaterialInfo>("craftMaterials").FindAll().ToList();
    }
}