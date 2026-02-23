using FFA.Models;
using LiteDB;
using System.IO;
using Tomlyn;
using Tomlyn.Model;

namespace FFA.Services;

/// <summary>
/// 未鑑定アイテムの鑑定サービス
/// </summary>
public class AppraisalService
{
    private readonly string _databasePath;
    private readonly UserService _userService;
    private readonly string _tomlPath;
    private List<UnidentifiedItemTemplate>? _cachedTemplates;
    
    private static readonly Random _random = new();

    public AppraisalService(UserService userService)
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "users.db");
        _userService = userService;
        
        // TOMLファイルのパスを設定
        _tomlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Items", "unidentified_items.toml");
        if (!File.Exists(_tomlPath))
        {
            _tomlPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Items", "unidentified_items.toml");
        }
        
        LoadTemplatesFromToml();
    }

    private LiteDatabase GetDatabase() => new LiteDatabase(_databasePath);

    /// <summary>
    /// TOMLファイルからテンプレートを読み込む
    /// </summary>
    private void LoadTemplatesFromToml()
    {
        _cachedTemplates = new List<UnidentifiedItemTemplate>();
        
        if (!File.Exists(_tomlPath))
        {
            Console.WriteLine($"Warning: unidentified_items.toml not found at {_tomlPath}, using defaults");
            InitializeDefaultTemplates();
            return;
        }

        try
        {
            var tomlContent = File.ReadAllText(_tomlPath);
            var doc = Toml.Parse(tomlContent);
            var model = doc.ToModel();
            
            if (model.TryGetValue("templates", out var templatesObj) && templatesObj is TomlArray templatesArray)
            {
                foreach (var templateObj in templatesArray)
                {
                    if (templateObj is TomlTable templateTable)
                    {
                        var template = ParseTemplate(templateTable);
                        if (template != null)
                        {
                            _cachedTemplates.Add(template);
                        }
                    }
                }
            }
            
            Console.WriteLine($"Loaded {_cachedTemplates.Count} unidentified item templates from TOML");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading unidentified_items.toml: {ex.Message}");
            InitializeDefaultTemplates();
        }
    }

    /// <summary>
    /// TOMLテーブルからテンプレートを解析
    /// </summary>
    private UnidentifiedItemTemplate? ParseTemplate(TomlTable table)
    {
        try
        {
            var template = new UnidentifiedItemTemplate
            {
                Id = GetIntValue(table, "id", 0),
                DisplayName = GetStringValue(table, "display_name", "???"),
                Description = GetStringValue(table, "description", ""),
                ItemType = ParseItemType(GetStringValue(table, "item_type", "Weapon")),
                ApparentRarity = GetIntValue(table, "apparent_rarity", 1),
                BaseAppraisalCost = GetIntValue(table, "base_appraisal_cost", 100),
                DropRate = GetDoubleValue(table, "drop_rate", 0.1),
                RequiredSourceLevel = GetIntValue(table, "required_source_level", 1),
                PossibleItems = new List<(int, string, int)>()
            };

            // possible_itemsの解析
            if (table.TryGetValue("possible_items", out var possibleItemsObj) && possibleItemsObj is TomlArray possibleItemsArray)
            {
                foreach (var itemObj in possibleItemsArray)
                {
                    if (itemObj is TomlTable itemTable)
                    {
                        int itemId = GetIntValue(itemTable, "item_id", 0);
                        string itemType = GetStringValue(itemTable, "item_type", "Weapon");
                        int weight = GetIntValue(itemTable, "weight", 1);
                        template.PossibleItems.Add((itemId, itemType, weight));
                    }
                }
            }

            return template;
        }
        catch
        {
            return null;
        }
    }

    private UnidentifiedItemType ParseItemType(string type)
    {
        return type?.ToLower() switch
        {
            "weapon" => UnidentifiedItemType.Weapon,
            "armor" => UnidentifiedItemType.Armor,
            "accessory" => UnidentifiedItemType.Accessory,
            "material" => UnidentifiedItemType.Material,
            _ => UnidentifiedItemType.Weapon
        };
    }

    private int GetIntValue(TomlTable table, string key, int defaultValue)
    {
        if (table.TryGetValue(key, out var value) && value is long l)
            return (int)l;
        if (table.TryGetValue(key, out var value2) && value2 is int i)
            return i;
        return defaultValue;
    }

    private double GetDoubleValue(TomlTable table, string key, double defaultValue)
    {
        if (table.TryGetValue(key, out var value) && value is double d)
            return d;
        if (table.TryGetValue(key, out var value2) && value2 is long l)
            return l;
        return defaultValue;
    }

    private string GetStringValue(TomlTable table, string key, string defaultValue)
    {
        if (table.TryGetValue(key, out var value) && value is string s)
            return s;
        return defaultValue;
    }

    /// <summary>
    /// デフォルトテンプレートの初期化（TOMLが見つからない場合）
    /// </summary>
    private void InitializeDefaultTemplates()
    {
        _cachedTemplates = new List<UnidentifiedItemTemplate>
        {
            new UnidentifiedItemTemplate
            {
                Id = 1,
                DisplayName = "古びた剣",
                Description = "錆びついた古い剣。本来の姿は不明。",
                ItemType = UnidentifiedItemType.Weapon,
                ApparentRarity = 1,
                BaseAppraisalCost = 100,
                DropRate = 0.15,
                RequiredSourceLevel = 1,
                PossibleItems = new List<(int, string, int)>
                {
                    (1, "Weapon", 50),
                    (2, "Weapon", 30),
                    (3, "Weapon", 15),
                    (4, "Weapon", 5),
                }
            },
            new UnidentifiedItemTemplate
            {
                Id = 6,
                DisplayName = "古びた鎧",
                Description = "ボロボロの鎧。まだ使えるかもしれない。",
                ItemType = UnidentifiedItemType.Armor,
                ApparentRarity = 1,
                BaseAppraisalCost = 100,
                DropRate = 0.15,
                RequiredSourceLevel = 1,
                PossibleItems = new List<(int, string, int)>
                {
                    (1, "Armor", 50),
                    (2, "Armor", 30),
                    (3, "Armor", 15),
                    (4, "Armor", 5),
                }
            },
            new UnidentifiedItemTemplate
            {
                Id = 9,
                DisplayName = "古びた指輪",
                Description = "装飾が薄れた指輪。",
                ItemType = UnidentifiedItemType.Accessory,
                ApparentRarity = 1,
                BaseAppraisalCost = 150,
                DropRate = 0.12,
                RequiredSourceLevel = 1,
                PossibleItems = new List<(int, string, int)>
                {
                    (1, "Accessory", 50),
                    (2, "Accessory", 30),
                    (3, "Accessory", 15),
                    (4, "Accessory", 5),
                }
            },
            new UnidentifiedItemTemplate
            {
                Id = 12,
                DisplayName = "謎の鉱石",
                Description = "未知の鉱石。何かの素材になりそう。",
                ItemType = UnidentifiedItemType.Material,
                ApparentRarity = 2,
                BaseAppraisalCost = 100,
                DropRate = 0.20,
                RequiredSourceLevel = 1,
                PossibleItems = new List<(int, string, int)>
                {
                    (1, "Material", 60),
                    (2, "Material", 30),
                    (3, "Material", 10),
                }
            }
        };
    }

    /// <summary>
    /// 未鑑定アイテムテンプレート一覧を取得
    /// </summary>
    public List<UnidentifiedItemTemplate> GetAllTemplates()
    {
        return _cachedTemplates ?? new List<UnidentifiedItemTemplate>();
    }

    /// <summary>
    /// ユーザーの未鑑定アイテム一覧を取得
    /// </summary>
    public List<UnidentifiedItem> GetUserItems(string username)
    {
        using var db = GetDatabase();
        return db.GetCollection<UnidentifiedItem>("unidentifiedItems")
            .Find(i => i.Username == username)
            .ToList();
    }

    /// <summary>
    /// 未鑑定アイテムを生成（ドロップ用）
    /// </summary>
    public UnidentifiedItem? GenerateItem(int sourceLevel, string source)
    {
        if (_cachedTemplates == null || !_cachedTemplates.Any())
            return null;
        
        // 条件に合うテンプレートを取得
        var validTemplates = _cachedTemplates.Where(t => t.RequiredSourceLevel <= sourceLevel).ToList();
        if (!validTemplates.Any()) return null;

        // ドロップ判定
        var candidates = new List<UnidentifiedItemTemplate>();
        foreach (var template in validTemplates)
        {
            if (_random.NextDouble() < template.DropRate)
            {
                candidates.Add(template);
            }
        }

        if (!candidates.Any()) return null;

        // ランダムに1つ選択
        var selectedTemplate = candidates[_random.Next(candidates.Count)];
        
        // 実際のアイテムを決定
        var (itemId, itemType, _) = SelectActualItem(selectedTemplate.PossibleItems);

        return new UnidentifiedItem
        {
            DisplayName = selectedTemplate.DisplayName,
            Description = selectedTemplate.Description,
            ItemType = selectedTemplate.ItemType,
            ApparentRarity = selectedTemplate.ApparentRarity,
            AppraisalCost = selectedTemplate.BaseAppraisalCost,
            ActualItemId = itemId,
            ActualItemType = itemType,
            Source = source,
            ObtainedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// ユーザーに未鑑定アイテムを追加
    /// </summary>
    public void AddItemToUser(string username, UnidentifiedItem item)
    {
        using var db = GetDatabase();
        item.Username = username;
        db.GetCollection<UnidentifiedItem>("unidentifiedItems").Insert(item);
    }

    /// <summary>
    /// アイテムを鑑定
    /// </summary>
    public AppraisalResult AppraiseItem(string username, int itemId, AppraiserLevel appraiserLevel = AppraiserLevel.Novice)
    {
        using var db = GetDatabase();
        var items = db.GetCollection<UnidentifiedItem>("unidentifiedItems");
        var users = db.GetCollection<User>("users");
        
        var item = items.FindById(itemId);
        if (item == null)
            return new AppraisalResult { Success = false, Message = "アイテムが見つかりません。" };
        
        if (item.Username != username)
            return new AppraisalResult { Success = false, Message = "このアイテムはあなたのものではありません。" };
        
        if (item.IsIdentified)
            return new AppraisalResult { Success = false, Message = "このアイテムは既に鑑定済みです。" };

        var user = users.FindOne(u => u.Username == username);
        if (user == null)
            return new AppraisalResult { Success = false, Message = "ユーザーが見つかりません。" };

        // 鑑定コスト計算（鑑定士レベルによる割引）
        int cost = CalculateAppraisalCost(item.AppraisalCost, appraiserLevel);
        
        if (user.Gil < cost)
            return new AppraisalResult { Success = false, Message = $"ギルが足りません。必要: {cost} G", Cost = cost };

        // ギルを消費
        user.Gil -= cost;
        users.Update(user);

        // アイテムを鑑定済みに
        item.IsIdentified = true;
        item.IdentifiedAt = DateTime.UtcNow;
        
        // 実際のアイテム名を取得
        item.ActualItemName = GetActualItemName(item.ActualItemId, item.ActualItemType);
        items.Update(item);

        // 実際のレアリティを取得
        int actualRarity = GetActualRarity(item.ActualItemId, item.ActualItemType);
        bool isGoodResult = actualRarity > item.ApparentRarity;

        return new AppraisalResult
        {
            Success = true,
            Item = item,
            Message = $"鑑定完了！「{item.ActualItemName}」でした！",
            Cost = cost,
            IsGoodResult = isGoodResult,
            ActualRarity = actualRarity
        };
    }

    /// <summary>
    /// 鑑定済みアイテムを受け取る（インベントリに追加）
    /// </summary>
    public (bool Success, string Message) ClaimItem(string username, int itemId)
    {
        using var db = GetDatabase();
        var items = db.GetCollection<UnidentifiedItem>("unidentifiedItems");
        var users = db.GetCollection<User>("users");
        
        var item = items.FindById(itemId);
        if (item == null)
            return (false, "アイテムが見つかりません。");
        
        if (item.Username != username)
            return (false, "このアイテムはあなたのものではありません。");
        
        if (!item.IsIdentified)
            return (false, "まだ鑑定されていません。");

        var user = users.FindOne(u => u.Username == username);
        if (user == null)
            return (false, "ユーザーが見つかりません。");

        // インベントリに追加
        var inventoryItem = CreateInventoryItem(item);
        user.Inventory.Add(inventoryItem);
        users.Update(user);

        // 未鑑定アイテムを削除
        items.Delete(itemId);

        return (true, $"「{item.ActualItemName}」を入手しました！");
    }

    /// <summary>
    /// 未鑑定アイテムを捨てる
    /// </summary>
    public bool DiscardItem(string username, int itemId)
    {
        using var db = GetDatabase();
        var items = db.GetCollection<UnidentifiedItem>("unidentifiedItems");
        
        var item = items.FindById(itemId);
        if (item == null || item.Username != username)
            return false;

        items.Delete(itemId);
        return true;
    }

    /// <summary>
    /// 実際のアイテムを選択
    /// </summary>
    private (int ItemId, string ItemType, int Weight) SelectActualItem(List<(int ItemId, string ItemType, int Weight)> possibleItems)
    {
        int totalWeight = possibleItems.Sum(i => i.Weight);
        int roll = _random.Next(totalWeight);
        int cumulative = 0;

        foreach (var item in possibleItems)
        {
            cumulative += item.Weight;
            if (roll < cumulative)
                return item;
        }

        return possibleItems.First();
    }

    /// <summary>
    /// 鑑定コストを計算
    /// </summary>
    private int CalculateAppraisalCost(int baseCost, AppraiserLevel level)
    {
        return level switch
        {
            AppraiserLevel.Novice => baseCost,
            AppraiserLevel.Apprentice => (int)(baseCost * 0.9),
            AppraiserLevel.Journeyman => (int)(baseCost * 0.75),
            AppraiserLevel.Expert => (int)(baseCost * 0.5),
            AppraiserLevel.Master => Math.Max(0, baseCost - 500),
            _ => baseCost
        };
    }

    /// <summary>
    /// 実際のアイテム名を取得
    /// </summary>
    private string GetActualItemName(int? itemId, string? itemType)
    {
        if (itemId == null || string.IsNullOrEmpty(itemType))
            return "不明なアイテム";

        // アイテムタイプとIDに基づいて名前を生成
        return itemType switch
        {
            "Weapon" => GetWeaponName(itemId.Value),
            "Armor" => GetArmorName(itemId.Value),
            "Accessory" => GetAccessoryName(itemId.Value),
            "Material" => GetMaterialName(itemId.Value),
            _ => $"アイテム#{itemId}"
        };
    }

    /// <summary>
    /// 武器名を取得
    /// </summary>
    private string GetWeaponName(int itemId)
    {
        // IDに基づく武器名のマッピング
        return itemId switch
        {
            1 => "錆びた短剣",
            2 => "鉄の剣",
            3 => "鋼の剣",
            4 => "ミスリルソード",
            5 => "炎の剣",
            6 => "氷の剣",
            7 => "光の剣",
            8 => "伝説の剣",
            _ => $"武器#{itemId}"
        };
    }

    /// <summary>
    /// 防具名を取得
    /// </summary>
    private string GetArmorName(int itemId)
    {
        return itemId switch
        {
            1 => "ボロ布の服",
            2 => "革の鎧",
            3 => "鎖帷子",
            4 => "プレートアーマー",
            5 => "ミスリルアーマー",
            6 => "ドラゴンアーマー",
            _ => $"防具#{itemId}"
        };
    }

    /// <summary>
    /// アクセサリー名を取得
    /// </summary>
    private string GetAccessoryName(int itemId)
    {
        return itemId switch
        {
            1 => "石の指輪",
            2 => "銅の指輪",
            3 => "銀の指輪",
            4 => "金の指輪",
            5 => "ルビーの指輪",
            6 => "ダイヤの指輪",
            7 => "伝説の指輪",
            _ => $"アクセサリー#{itemId}"
        };
    }

    /// <summary>
    /// 素材名を取得
    /// </summary>
    private string GetMaterialName(int itemId)
    {
        return itemId switch
        {
            1 => "鉄鉱石",
            2 => "銅鉱石",
            3 => "ミスリル鉱石",
            4 => "オリハルコン",
            5 => "ドラゴンの鱗",
            _ => $"素材#{itemId}"
        };
    }

    /// <summary>
    /// 実際のレアリティを取得
    /// </summary>
    private int GetActualRarity(int? itemId, string? itemType)
    {
        if (itemId == null) return 1;

        // IDに基づいてレアリティを計算（高いIDほど高いレアリティ）
        return Math.Min(5, (itemId.Value / 2) + 1);
    }

    /// <summary>
    /// インベントリアイテムを作成
    /// </summary>
    private InventoryItem CreateInventoryItem(UnidentifiedItem item)
    {
        var inventoryItem = new InventoryItem
        {
            Name = item.ActualItemName ?? "不明なアイテム",
            Type = item.ActualItemType ?? "Unknown",
            ItemId = item.ActualItemId ?? 0,
            Quantity = 1,
            Price = EstimateItemValue(item.ActualItemId, item.ActualItemType)
        };

        // 武器・防具の詳細情報を設定（IDに基づく推定値）
        if (item.ActualItemType == "Weapon" && item.ActualItemId.HasValue)
        {
            inventoryItem.Attack = 5 + (item.ActualItemId.Value * 3);
        }
        else if (item.ActualItemType == "Armor" && item.ActualItemId.HasValue)
        {
            inventoryItem.Defense = 3 + (item.ActualItemId.Value * 2);
        }

        return inventoryItem;
    }

    /// <summary>
    /// アイテムの推定価値を計算
    /// </summary>
    private int EstimateItemValue(int? itemId, string? itemType)
    {
        if (itemId == null) return 0;

        // IDに基づいて価値を計算
        return itemType switch
        {
            "Weapon" => 100 + (itemId.Value * 50),
            "Armor" => 80 + (itemId.Value * 40),
            "Accessory" => 150 + (itemId.Value * 75),
            "Material" => 50 + (itemId.Value * 30),
            _ => 100
        };
    }
}
