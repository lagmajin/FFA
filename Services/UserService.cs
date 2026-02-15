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

    public bool Register(string username, string password, Models.Job job = Models.Job.Warrior)
    {
        using var db = new LiteDatabase(_databasePath);
        var users = db.GetCollection<User>("users");

        if (users.FindOne(u => u.Username == username) != null)
            return false; // 既に存在

        var hash = HashPassword(password);
        var user = new User { Username = username, PasswordHash = hash, Gil = 1000, OldCoin = 100, Job = job };
        user.Premium = 10; // initial premium currency

        // Set initial weapon to こんぼう for all jobs
        user.EquippedWeapon = new Weapon { Name = "こんぼう", Attack = 6 };

        // Assign default equipment based on job (armor and accessory)
        switch (job)
        {
            case Models.Job.Warrior:
                user.EquippedArmor = new Armor { Name = "Leather Armor", Defense = 5 };
                user.EquippedAccessory = new Accessory { Name = "Iron Ring", Effect = "+1 Strength" };
                break;
            case Models.Job.Monk:
                user.EquippedArmor = new Armor { Name = "Cloth Robe", Defense = 3 };
                user.EquippedAccessory = new Accessory { Name = "Monk's Bead", Effect = "+1 Dexterity" };
                break;
            case Models.Job.WhiteMage:
                user.EquippedArmor = new Armor { Name = "White Robe", Defense = 2 };
                user.EquippedAccessory = new Accessory { Name = "White Tiara", Effect = "+5% Healing" };
                break;
            case Models.Job.BlackMage:
                user.EquippedArmor = new Armor { Name = "Black Robe", Defense = 2 };
                user.EquippedAccessory = new Accessory { Name = "Black Orb", Effect = "+5% Magic Damage" };
                break;
        }
        users.Insert(user);

        // assign starter abilities
        var abilityService = new AbilityService();
        abilityService.AssignStarterAbilities(user);


        // record admin log for registration
        var logs = db.GetCollection<AdminLog>("adminlogs");
        logs.Insert(new AdminLog { Action = "Register", Detail = $"User '{username}' registered as {job}" });

        return true;
    }

    public User? Login(string username, string password)
    {
        using var db = new LiteDatabase(_databasePath);
        var users = db.GetCollection<User>("users");

        var user = users.FindOne(u => u.Username == username);
        if (user != null && VerifyPassword(password, user.PasswordHash))
            return user;

        return null;
    }

    public User? GetByUsername(string username)
    {
        using var db = new LiteDatabase(_databasePath);
        var users = db.GetCollection<User>("users");
        return users.FindOne(u => u.Username == username);
    }

    public IEnumerable<User> GetAllUsers()
    {
        using var db = new LiteDatabase(_databasePath);
        var users = db.GetCollection<User>("users");
        return users.FindAll().ToList();
    }

    public void UpdateUser(User user)
    {
        using var db = new LiteDatabase(_databasePath);
        var users = db.GetCollection<User>("users");
        users.Update(user);
    }

    // Adjust gil (can be negative)
    public int AdjustGil(string username, int delta)
    {
        using var db = new LiteDatabase(_databasePath);
        var users = db.GetCollection<User>("users");
        var user = users.FindOne(u => u.Username == username);
        if (user == null) return 0;
        user.Gil += delta;
        users.Update(user);
        return user.Gil;
    }

    public int AdjustOldCoin(string username, int delta)
    {
        using var db = new LiteDatabase(_databasePath);
        var users = db.GetCollection<User>("users");
        var user = users.FindOne(u => u.Username == username);
        if (user == null) return 0;
        user.OldCoin += delta;
        users.Update(user);
        return user.OldCoin;
    }

    public int AddExp(string username, int amount)
    {
        using var db = new LiteDatabase(_databasePath);
        var users = db.GetCollection<User>("users");
        var user = users.FindOne(u => u.Username == username);
        if (user == null) return 0;
        user.Exp += amount;
        users.Update(user);
        return user.Exp;
    }

    // Level-up check and grant status points
    public void AddExpAndHandleLevel(string username, int amount)
    {
        using var db = new LiteDatabase(_databasePath);
        var users = db.GetCollection<User>("users");
        var user = users.FindOne(u => u.Username == username);
        if (user == null) return;

        user.Exp += amount;
        while (user.Exp >= user.ExpToNext)
        {
            user.Exp -= user.ExpToNext;
            user.Level++;
            user.Status.PointsAvailable += 5; // grant 5 points per level
            user.ExpToNext = (int)(user.ExpToNext * 1.2);
        }
        users.Update(user);
    }

    public bool DeleteUser(string username)
    {
        using var db = new LiteDatabase(_databasePath);
        var users = db.GetCollection<User>("users");
        return users.DeleteMany(u => u.Username == username) > 0;
    }

    // Change job (転職)
    // Returns true if successful, false otherwise (insufficient gil or user not found)
    public bool ChangeJob(string username, Job newJob, int cost = 500)
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
                user.EquippedAccessory = new Accessory { Name = "Iron Ring", Effect = "+1 Strength" };
                break;
            case Job.Monk:
                user.EquippedWeapon = new Weapon { Name = "こんぼう", Attack = 6 };
                user.EquippedArmor = new Armor { Name = "Cloth Robe", Defense = 3 };
                user.EquippedAccessory = new Accessory { Name = "Monk's Bead", Effect = "+1 Dexterity" };
                break;
            case Job.WhiteMage:
                user.EquippedWeapon = new Weapon { Name = "こんぼう", Attack = 6 };
                user.EquippedArmor = new Armor { Name = "White Robe", Defense = 2 };
                user.EquippedAccessory = new Accessory { Name = "White Tiara", Effect = "+5% Healing" };
                break;
            case Job.BlackMage:
                user.EquippedWeapon = new Weapon { Name = "こんぼう", Attack = 6 };
                user.EquippedArmor = new Armor { Name = "Black Robe", Defense = 2 };
                user.EquippedAccessory = new Accessory { Name = "Black Orb", Effect = "+5% Magic Damage" };
                break;
        }

        users.Update(user);
        return true;
    }

    // Inventory helper
    public void AddItemToUser(string username, InventoryItem item)
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

    // Sell item from inventory (returns sell price, which is typically half of purchase price)
    public int SellItem(string username, string itemName, int quantity = 1)
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
}