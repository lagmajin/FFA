namespace FFA.Models;

/// <summary>
/// 注文状態
/// </summary>
public enum OrderStatus
{
    Pending,     // 注文待
    Shipping,    // 発送中
    Delivered,   // 配達済み
    Cancelled,   // キャンセル
}

/// <summary>
/// 通信販売注文
/// </summary>
public class MailOrder
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string ItemName { get; set; } = "";
    public string ItemType { get; set; } = ""; // 武器、防具、装飾品、消耗品
    public int Quantity { get; set; } = 1;
    public int TotalPrice { get; set; } = 0;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime OrderedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EstimatedDeliveryAt { get; set; } // 予定配達日時
    public DateTime? DeliveredAt { get; set; } // 實際配達日時
    public int DeliveryDays { get; set; } = 3; // 配達所需天数
}

/// <summary>
/// 通信販売商品
/// </summary>
public class MailOrderProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public int Price { get; set; }
    public int DeliveryDays { get; set; } = 3; // 配達所需天数
    public bool IsAvailable { get; set; } = true;
}

/// <summary>
/// 通信販売カタログ
/// </summary>
public static class MailOrderCatalog
{
    public static List<MailOrderProduct> Products { get; } = new List<MailOrderProduct>
    {
        // 武器
        new MailOrderProduct { Id = 1, Name = "鉄の剣", Type = "武器", Price = 500, DeliveryDays = 2 },
        new MailOrderProduct { Id = 2, Name = "鋼の斧", Type = "武器", Price = 800, DeliveryDays = 2 },
        new MailOrderProduct { Id = 3, Name = "長弓", Type = "武器", Price = 600, DeliveryDays = 2 },
        new MailOrderProduct { Id = 4, Name = "魔法杖", Type = "武器", Price = 1000, DeliveryDays = 3 },
        new MailOrderProduct { Id = 5, Name = "dagger", Type = "武器", Price = 400, DeliveryDays = 1 },

        // 防具
        new MailOrderProduct { Id = 10, Name = "革の鎧", Type = "防具", Price = 400, DeliveryDays = 2 },
        new MailOrderProduct { Id = 11, Name = "鎖帷子", Type = "防具", Price = 700, DeliveryDays = 3 },
        new MailOrderProduct { Id = 12, Name = "プレートメイル", Type = "防具", Price = 1500, DeliveryDays = 5 },
        new MailOrderProduct { Id = 13, Name = "魔法ローブ", Type = "防具", Price = 600, DeliveryDays = 2 },

        // 装飾品
        new MailOrderProduct { Id = 20, Name = "力のリング", Type = "装飾品", Price = 300, DeliveryDays = 1 },
        new MailOrderProduct { Id = 21, Name = "知恵のピアス", Type = "装飾品", Price = 300, DeliveryDays = 1 },
        new MailOrderProduct { Id = 22, Name = "敏捷のアミュレット", Type = "装飾品", Price = 350, DeliveryDays = 1 },
        new MailOrderProduct { Id = 23, Name = "幸運の-CHARM", Type = "装飾品", Price = 500, DeliveryDays = 2 },

        // 消耗品
        new MailOrderProduct { Id = 30, Name = "体力药水", Type = "消耗品", Price = 50, DeliveryDays = 1 },
        new MailOrderProduct { Id = 31, Name = "MP药水", Type = "消耗品", Price = 80, DeliveryDays = 1 },
        new MailOrderProduct { Id = 32, Name = "解毒剤", Type = "消耗品", Price = 100, DeliveryDays = 1 },
        new MailOrderProduct { Id = 33, Name = "エーテル", Type = "消耗品", Price = 200, DeliveryDays = 2 },

        // プレミアム商品（長い配達時間）
        new MailOrderProduct { Id = 40, Name = "伝説の剣", Type = "武器", Price = 10000, DeliveryDays = 7 },
        new MailOrderProduct { Id = 41, Name = "天使の鎧", Type = "防具", Price = 15000, DeliveryDays = 10 },
        new MailOrderProduct { Id = 42, Name = "龍の卵", Type = "特殊", Price = 50000, DeliveryDays = 14 },
    };

    public static List<MailOrderProduct> GetByType(string type)
    {
        return Products.Where(p => p.Type == type && p.IsAvailable).ToList();
    }

    public static MailOrderProduct? GetById(int id)
    {
        return Products.FirstOrDefault(p => p.Id == id);
    }
}
