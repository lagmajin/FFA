using LiteDB;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using FFA.Models;

namespace FFA.Services;

public class UserService
{
    private readonly string _databasePath;

    public UserService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "users.db");
    }

    public bool Register(string username, string password, Models.Job job = Models.Job.Warrior, int? countryId = null)
    {
        // MAINTAINER NOTE:
        // This method performs several initialization steps (ability assignment, country
        // defaults, and creating a world grid position) and currently constructs
        // some services directly (e.g. AbilityService, CountryService, WorldGridService)
        // using `new`. Prefer injecting services via DI if you refactor this class.
        // Direct construction may bypass configuration and cause subtle bugs.
        // Keep database access via LiteDB here; changes to storage should be coordinated
        // across services to avoid data inconsistency.

        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");

            if (users.FindOne(u => u.Username == username) != null)
                return false; // 既に存在

            var hash = HashPassword(password);
            var user = new User { Username = username, PasswordHash = hash, Gil = 1000, OldCoin = 100, Job = job, CountryId = countryId };
            user.Premium = 10; // initial premium currency

            // Set initial weapon to こんぼう for all jobs
            user.EquippedWeapon = new Weapon { Name = "こんぼう", Attack = 6 };

            // Assign default equipment based on job (armor and accessory)
            switch (job)
            {
                case Models.Job.Warrior:
                    user.EquippedArmor = new Armor { Name = "Leather Armor", Defense = 5 };
                    user.EquippedAccessory1 = new Accessory { Name = "Iron Ring", Effect = "+1 Strength" };
                    break;
                case Models.Job.Monk:
                    user.EquippedArmor = new Armor { Name = "Cloth Robe", Defense = 3 };
                    user.EquippedAccessory1 = new Accessory { Name = "Monk's Bead", Effect = "+1 Dexterity" };
                    break;
                case Models.Job.WhiteMage:
                    user.EquippedArmor = new Armor { Name = "White Robe", Defense = 2 };
                    user.EquippedAccessory1 = new Accessory { Name = "White Tiara", Effect = "+5% Healing" };
                    break;
                case Models.Job.BlackMage:
                    user.EquippedArmor = new Armor { Name = "Black Robe", Defense = 2 };
                    user.EquippedAccessory1 = new Accessory { Name = "Black Orb", Effect = "+5% Magic Damage" };
                    break;
                case Models.Job.Ranger:
                    user.EquippedArmor = new Armor { Name = "Ranger Vest", Defense = 4 };
                    user.EquippedAccessory1 = new Accessory { Name = "Eagle Feather", Effect = "+2 Agility" };
                    break;
                case Models.Job.Paladin:
                    user.EquippedArmor = new Armor { Name = "Holy Plate", Defense = 7 };
                    user.EquippedAccessory1 = new Accessory { Name = "Sacred Amulet", Effect = "+3 Vitality" };
                    break;
                case Models.Job.DarkKnight:
                    user.EquippedArmor = new Armor { Name = "Dark Armor", Defense = 6 };
                    user.EquippedAccessory1 = new Accessory { Name = "Cursed Ring", Effect = "+2 Strength" };
                    break;
                case Models.Job.Bard:
                    user.EquippedArmor = new Armor { Name = "Silk Robe", Defense = 2 };
                    user.EquippedAccessory1 = new Accessory { Name = "Musical Charm", Effect = "+2 Intelligence" };
                    break;
                case Models.Job.Thief:
                    user.EquippedArmor = new Armor { Name = "Shadow Cloth", Defense = 3 };
                    user.EquippedAccessory1 = new Accessory { Name = "Thief's Mark", Effect = "+3 Luck" };
                    break;
                case Models.Job.Ninja:
                    user.EquippedArmor = new Armor { Name = "Ninja Gi", Defense = 4 };
                    user.EquippedAccessory1 = new Accessory { Name = "Shuriken Pouch", Effect = "+2 Dexterity" };
                    break;
            }
            users.Insert(user);

            // assign starter abilities
            var abilityService = new AbilityService();
            abilityService.AssignStarterAbilities(user);

            // If user did not choose a country, assign a sensible default based on job
            try
            {
                if (!countryId.HasValue)
                {
                    var countryService = new CountryService();
                    int defaultCountry = countryService.GetAllCountries()
                        .Where(c => c.Name == "Neutral Haven")
                        .Select(c => c.Id)
                        .FirstOrDefault();

                    // map some jobs to thematic countries if desired
                    switch (job)
                    {
                        case Models.Job.Warrior:
                        case Models.Job.Paladin:
                        case Models.Job.DarkKnight:
                            defaultCountry = countryService.GetAllCountries().FirstOrDefault(c => c.Name == "Inferno")?.Id ?? defaultCountry;
                            break;
                        case Models.Job.WhiteMage:
                        case Models.Job.Bard:
                            defaultCountry = countryService.GetAllCountries().FirstOrDefault(c => c.Name == "Verdania")?.Id ?? defaultCountry;
                            break;
                        case Models.Job.BlackMage:
                            defaultCountry = countryService.GetAllCountries().FirstOrDefault(c => c.Name == "Frostheim")?.Id ?? defaultCountry;
                            break;
                        case Models.Job.Ranger:
                        case Models.Job.Thief:
                        case Models.Job.Ninja:
                            defaultCountry = countryService.GetAllCountries().FirstOrDefault(c => c.Name == "Tempestia")?.Id ?? defaultCountry;
                            break;
                    }

                    user.CountryId = defaultCountry == 0 ? null : (int?)defaultCountry;

                    // Apply country bonuses immediately
                    if (user.CountryId.HasValue)
                    {
                        countryService.ApplyCountryBonus(user);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UserService.Register: country assignment failed: {ex.Message}");
            }


            // Persist country assignment and bonus changes back to DB
            users.Update(user);

            // record admin log for registration
            var logs = db.GetCollection<AdminLog>("adminlogs");
            logs.Insert(new AdminLog { Action = "Register", Detail = $"User '{username}' registered as {job}" + (user.CountryId.HasValue ? $" in Country {user.CountryId}" : " (no country)") });

            // ensure player start position is created on registration
            try
            {
                var grid = new WorldGridService();
                grid.GetOrCreatePlayerPosition(username);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UserService.Register: failed to create player position: {ex.Message}");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.Register 例外: {ex.Message} - {ex.StackTrace}");
            return false;
        }
    }

    public User? Login(string username, string password)
    {
        // MAINTAINER NOTE:
        // Important: ensure password verification happens before returning a User.
        // A previous bug returned the user regardless of password check. Do not
        // move the `return user;` outside the password-verified block.
        // If you introduce asynchronous password checks, preserve the same
        // semantics and avoid returning a user without successful verification.

        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");

            var user = users.FindOne(u => u.Username == username);
            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                // update last active on login
                user.LastActiveUtc = DateTime.UtcNow;
                users.Update(user);
                // Last IP is set by caller via UpdateIpFromContext if available
                return user;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.Login 例外: {ex.Message} - {ex.StackTrace}");
            return null;
        }
    }

    public void UpdateIpFromContext(string username, string ip)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");
            var user = users.FindOne(u => u.Username == username);
            if (user == null) return;
            user.LastIp = ip;
            users.Update(user);
        }
        catch { }
    }

    public User? GetByUsername(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");
            return users.FindOne(u => u.Username == username);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.GetByUsername 例外: {ex.Message} - {ex.StackTrace}");
            return null;
        }
    }

    public IEnumerable<User> GetAllUsers()
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");
            return users.FindAll().ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.GetAllUsers 例外: {ex.Message} - {ex.StackTrace}");
            return new List<User>();
        }
    }

    public void UpdateUser(User user)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");
            users.Update(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.UpdateUser 例外: {ex.Message} - {ex.StackTrace}");
        }
    }

    // Adjust gil (can be negative)
    public int AdjustGil(string username, int delta)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");
            var user = users.FindOne(u => u.Username == username);
            if (user == null) return 0;
            user.Gil += delta;
            users.Update(user);
            return user.Gil;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.AdjustGil 例外: {ex.Message} - {ex.StackTrace}");
            return 0;
        }
    }

    public int AdjustOldCoin(string username, int delta)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");
            var user = users.FindOne(u => u.Username == username);
            if (user == null) return 0;
            user.OldCoin += delta;
            users.Update(user);
            return user.OldCoin;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.AdjustOldCoin 例外: {ex.Message} - {ex.StackTrace}");
            return 0;
        }
    }

    public int AddExp(string username, int amount)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");
            var user = users.FindOne(u => u.Username == username);
            if (user == null) return 0;
            user.Exp += amount;
            users.Update(user);
            return user.Exp;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.AddExp 例外: {ex.Message} - {ex.StackTrace}");
            return 0;
        }
    }

    // Level-up check and grant status points
    public void AddExpAndHandleLevel(string username, int amount)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");
            var user = users.FindOne(u => u.Username == username);
            if (user == null) return;

            int oldLevel = user.Level;
            int oldStatusStr = user.Status.Str;
            int oldStatusDex = user.Status.Dex;
            int oldStatusInt = user.Status.Int;
            int oldStatusVit = user.Status.Vit;
            int oldStatusAgi = user.Status.Agi;
            int oldStatusLuk = user.Status.Luk;
            int totalLevelsGained = 0;

            user.Exp += amount;
            while (user.Exp >= user.ExpToNext && user.Level < 100)
            {
                user.Exp -= user.ExpToNext;
                user.Level++;
                totalLevelsGained++;
                
                // 基本ステータスポイント
                user.Status.PointsAvailable += 5;
                
                // ジョブ別成長適用的ステータス自動成長
                var (strBonus, dexBonus, intBonus, vitBonus, agiBonus, lukBonus) = GetJobGrowthRates(user.Job);
                user.Status.Str += strBonus;
                user.Status.Dex += dexBonus;
                user.Status.Int += intBonus;
                user.Status.Vit += vitBonus;
                user.Status.Agi += agiBonus;
                user.Status.Luk += lukBonus;
                
                // HP/MP成長
                user.MaxHP += (vitBonus * 5) + 5;
                user.HP = user.MaxHP;
                
                // レベル10,20,30...でスキルポイント bonus
                if (user.Level % 10 == 0)
                {
                    user.SkillPoints += (user.Level / 10);
                }
                
                // 経験値テーブルの更新
                user.ExpToNext = CalculateExpForLevel(user.Level + 1);
            }
            users.Update(user);

            // レベルアップイベントを記録
            if (totalLevelsGained > 0)
            {
                var eventService = new GameEventService();
                eventService.LogLevelUp(username, user.Level);
                
                // ステータスの成長を記録
                int totalStrGain = user.Status.Str - oldStatusStr;
                int totalDexGain = user.Status.Dex - oldStatusDex;
                int totalIntGain = user.Status.Int - oldStatusInt;
                int totalVitGain = user.Status.Vit - oldStatusVit;
                int totalAgiGain = user.Status.Agi - oldStatusAgi;
                int totalLukGain = user.Status.Luk - oldStatusLuk;
                
                Console.WriteLine($"[LevelUp] {username}: Lv{oldLevel}→{user.Level}, STR+{totalStrGain} DEX+{totalDexGain} INT+{totalIntGain} VIT+{totalVitGain} AGI+{totalAgiGain} LUK+{totalLukGain}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.AddExpAndHandleLevel 例外: {ex.Message} - {ex.StackTrace}");
        }
    }
    
    // Get job growth rates based on job
    private (int str, int dex, int intel, int vit, int agi, int luk) GetJobGrowthRates(Job job)
    {
        return job switch
        {
            Job.Warrior => (3, 1, 0, 2, 1, 0),
            Job.Monk => (2, 1, 0, 2, 2, 0),
            Job.WhiteMage => (1, 0, 2, 2, 1, 1),
            Job.BlackMage => (0, 1, 3, 1, 1, 1),
            Job.Ranger => (1, 2, 0, 1, 3, 0),
            Job.Paladin => (2, 0, 1, 3, 0, 1),
            Job.DarkKnight => (2, 1, 1, 2, 0, 1),
            Job.Bard => (0, 2, 1, 0, 2, 2),
            Job.Thief => (1, 3, 0, 1, 2, 0),
            Job.Ninja => (1, 2, 1, 1, 3, 0),
            Job.HolyKnight => (2, 0, 1, 3, 0, 1),
            Job.DeathKnight => (2, 1, 1, 2, 0, 1),
            Job.ArchMage => (0, 0, 3, 1, 1, 2),
            Job.BeastMaster => (1, 2, 0, 2, 2, 0),
            Job.Duelist => (2, 2, 0, 1, 2, 0),
            Job.Grandmaster => (2, 1, 1, 2, 1, 1),
            _ => (1, 1, 1, 1, 1, 1)
        };
    }
    
    // Calculate experience required for a specific level
    private int CalculateExpForLevel(int level)
    {
        if (level <= 1) return 0;
        
        // 指数関数的な経験値曲線
        // Base: 100, Growth: 1.2 per level
        double exp = 100 * Math.Pow(1.2, level - 1);
        return (int)exp;
    }

    public bool DeleteUser(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");
            return users.DeleteMany(u => u.Username == username) > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.DeleteUser 例外: {ex.Message} - {ex.StackTrace}");
            return false;
        }
    }

    // Change job (転職)
    // Returns true if successful, false otherwise (insufficient gil or user not found)
    public bool ChangeJob(string username, Job newJob, int cost = 500)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");
            var user = users.FindOne(u => u.Username == username);
            if (user == null) return false;
            if (user.Gil < cost) return false;

            user.Gil -= cost;
            user.Job = newJob;

            // assign default equipment based on new job
            switch (newJob)
            {
                case Job.Warrior:
                    user.EquippedWeapon = new Weapon { Name = "こんぼう", Attack = 6 };
                    user.EquippedArmor = new Armor { Name = "Leather Armor", Defense = 5 };
                    user.EquippedAccessory1 = new Accessory { Name = "Iron Ring", Effect = "+1 Strength" };
                    break;
                case Job.Monk:
                    user.EquippedWeapon = new Weapon { Name = "こんぼう", Attack = 6 };
                    user.EquippedArmor = new Armor { Name = "Cloth Robe", Defense = 3 };
                    user.EquippedAccessory1 = new Accessory { Name = "Monk's Bead", Effect = "+1 Dexterity" };
                    break;
                case Job.WhiteMage:
                    user.EquippedWeapon = new Weapon { Name = "こんぼう", Attack = 6 };
                    user.EquippedArmor = new Armor { Name = "White Robe", Defense = 2 };
                    user.EquippedAccessory1 = new Accessory { Name = "White Tiara", Effect = "+5% Healing" };
                    break;
                case Job.BlackMage:
                    user.EquippedWeapon = new Weapon { Name = "こんぼう", Attack = 6 };
                    user.EquippedArmor = new Armor { Name = "Black Robe", Defense = 2 };
                    user.EquippedAccessory1 = new Accessory { Name = "Black Orb", Effect = "+5% Magic Damage" };
                    break;
            }

            users.Update(user);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.ChangeJob 例外: {ex.Message} - {ex.StackTrace}");
            return false;
        }
    }

    // Inventory helper
public void AddItemToUser(string username, InventoryItem item)
{
    try
    {
        using var db = new LiteDatabase(_databasePath);
        var users = db.GetCollection<User>("users");
        var user = users.FindOne(u => u.Username == username);
        if (user == null) return;

        var existing = user.Inventory.FirstOrDefault(i => i.Name == item.Name);
        if (existing != null)
        {
            existing.Quantity += item.Quantity;
        }
        else
        {
            user.Inventory.Add(item);
        }

        users.Update(user);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"UserService.AddItemToUser 例外: {ex.Message} - {ex.StackTrace}");
    }
}

// Get user's equipped items
public (Weapon?, Armor?, Accessory?) GetEquippedItems(string username)
{
    try
    {
        using var db = new LiteDatabase(_databasePath);
        var users = db.GetCollection<User>("users");
        var user = users.FindOne(u => u.Username == username);
        if (user == null) return (null, null, null);

        return (user.EquippedWeapon, user.EquippedArmor, user.EquippedAccessory1);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"UserService.GetEquippedItems 例外: {ex.Message} - {ex.StackTrace}");
        return (null, null, null);
    }
}

// Get user's inventory items (weapons, armors, accessories)
public IEnumerable<InventoryItem> GetInventoryItems(string username)
{
    try
    {
        using var db = new LiteDatabase(_databasePath);
        var users = db.GetCollection<User>("users");
        var user = users.FindOne(u => u.Username == username);
        if (user == null) return new List<InventoryItem>();

        return user.Inventory.Where(i => i.Type == "Weapon" || i.Type == "Armor" || i.Type == "Accessory");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"UserService.GetInventoryItems 例外: {ex.Message} - {ex.StackTrace}");
        return new List<InventoryItem>();
    }
}

    // Sell item from inventory (returns sell price, which is typically half of purchase price)
    public int SellItem(string username, string itemName, int quantity = 1)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");
            var user = users.FindOne(u => u.Username == username);
            if (user == null) return 0;

            var item = user.Inventory.FirstOrDefault(i => i.Name == itemName);
            if (item == null || item.Quantity < quantity) return 0;

            // Calculate sell price (half of the original price by default)
            int sellPrice = (item.Price / 2) * quantity;
            
            // Remove item from inventory
            item.Quantity -= quantity;
            if (item.Quantity <= 0)
            {
                user.Inventory.Remove(item);
            }

            // Add gil to user
            user.Gil += sellPrice;
            users.Update(user);

            return sellPrice;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.SellItem 例外: {ex.Message} - {ex.StackTrace}");
            return 0;
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private bool VerifyPassword(string password, string hash)
    {
        var computedHash = HashPassword(password);
        return computedHash == hash;
    }

    // 転生する
    public RebirthResult Rebirth(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");
            var user = users.FindOne(u => u.Username == username);
            if (user == null)
                return new RebirthResult { Success = false, Message = "ユーザーが見つかりません" };

            if (user.Level < user.RebirthLevelRequired)
                return new RebirthResult { Success = false, Message = $"レベル{user.RebirthLevelRequired}以上でありません" };

            // 転生ボーナス計算
            int bonusStat = user.RebirthCount * 5;
            int bonusGil = user.RebirthCount * 1000;

            // 転生処理
            user.RebirthCount++;
            user.TotalLevel += user.Level;
            user.Level = 1;
            user.Exp = 0;
            user.ExpToNext = 100;
            user.Gil += bonusGil;
            user.Status.Str += bonusStat;
            user.Status.Vit += bonusStat;
            user.Status.Dex += bonusStat;
            user.Status.Int += bonusStat;
            user.Status.Agi += bonusStat;
            user.Status.Luk += bonusStat;

            // マスターシステムチェック
            if (user.TotalLevel >= 500 && !user.IsMaster)
            {
                user.IsMaster = true;
                user.MasterLevel = 1;
            }

            users.Update(user);

            return new RebirthResult
            {
                Success = true,
                Message = $"転生完了！累計レベル: {user.TotalLevel}, 支給ギル: {bonusGil}",
                NewRebirthCount = user.RebirthCount,
                BonusGil = bonusGil
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.Rebirth 例外: {ex.Message} - {ex.StackTrace}");
            return new RebirthResult { Success = false, Message = "転生中にエラーが発生しました" };
        }
    }

    // マスター経験値を獲得
    public MasterExpResult AddMasterExp(string username, int exp)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");
            var user = users.FindOne(u => u.Username == username);
            if (user == null)
                return new MasterExpResult { Success = false, Message = "ユーザーが見つかりません" };

            if (!user.IsMaster)
                return new MasterExpResult { Success = false, Message = "マスターではありません" };

            if (user.MasterLevel >= user.MaxMasterLevel)
                return new MasterExpResult { Success = false, Message = "最大マスターレベルに達しています" };

            user.MasterExp += exp;
            bool leveledUp = false;

            while (user.MasterExp >= user.MasterExpToNext && user.MasterLevel < user.MaxMasterLevel)
            {
                user.MasterExp -= user.MasterExpToNext;
                user.MasterLevel++;
                user.MasterExpToNext = (int)(user.MasterExpToNext * 1.5); // 次のレベル所需的经验值增加50%
                
                // マスターレベルが上がると全ステータスが上昇
                user.Status.Str += 2;
                user.Status.Vit += 2;
                user.Status.Dex += 2;
                user.Status.Int += 2;
                user.Status.Agi += 2;
                user.Status.Luk += 2;
                
                leveledUp = true;
            }

            users.Update(user);

            return new MasterExpResult
            {
                Success = true,
                Message = leveledUp ? $"マスターレベルアップ！Lv.{user.MasterLevel}になりました" : "マスター経験値を獲得しました",
                NewMasterLevel = user.MasterLevel,
                NewMasterExp = user.MasterExp,
                LeveledUp = leveledUp
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.AddMasterExp 例外: {ex.Message} - {ex.StackTrace}");
            return new MasterExpResult { Success = false, Message = "エラーが発生しました" };
        }
    }

    // マスターになる
    public bool UnlockMaster(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var users = db.GetCollection<User>("users");
            var user = users.FindOne(u => u.Username == username);
            if (user == null) return false;

            if (user.TotalLevel < 500)
                return false;

            user.IsMaster = true;
            user.MasterLevel = 1;
            users.Update(user);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UserService.UnlockMaster 例外: {ex.Message} - {ex.StackTrace}");
            return false;
        }
    }
}

// 転生結果クラス
public class RebirthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int NewRebirthCount { get; set; }
    public int BonusGil { get; set; }
}

// マスター経験値結果クラス
public class MasterExpResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int NewMasterLevel { get; set; }
    public int NewMasterExp { get; set; }
    public bool LeveledUp { get; set; }
}