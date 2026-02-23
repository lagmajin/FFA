namespace FFA.Models;

public class InventoryItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public int Price { get; set; } = 0; // gil price
    public string Type { get; set; } = ""; // Weapon, Armor, Accessory, Consumable, Fish, Bait
    public int ItemId { get; set; } = 0; // アイテムの元ID（餌など）
    
    // Weapon properties
    public int Attack { get; set; } = 0;
    
    // Armor properties
    public int Defense { get; set; } = 0;
    
    // Accessory properties
    public string Effect { get; set; } = "";
    
    // Equipment requirements
    public int RequiredStr { get; set; } = 0;
    public int RequiredDex { get; set; } = 0;
    public int RequiredInt { get; set; } = 0;
    public int RequiredVit { get; set; } = 0;
    public int RequiredAgi { get; set; } = 0;
    public int RequiredLuk { get; set; } = 0;
}
