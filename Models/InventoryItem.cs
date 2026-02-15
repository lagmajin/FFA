namespace FFA.Models;

public class InventoryItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public int Price { get; set; } = 0; // gil price
}
