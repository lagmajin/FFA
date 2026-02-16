using LiteDB;
using System;

namespace FFA.Models;

public class MarketItem
{
    [BsonId]
    public int Id { get; set; }
    public string SellerUsername { get; set; } = string.Empty;
    public InventoryItem Item { get; set; } = new InventoryItem();
    public int Price { get; set; }
    public DateTime ListedAt { get; set; }
    public bool IsSold { get; set; }
    public string? BuyerUsername { get; set; }
    public DateTime? SoldAt { get; set; }
}