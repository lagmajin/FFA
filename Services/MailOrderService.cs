using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using FFA.Models;

namespace FFA.Services;

public class MailOrderService
{
    private readonly string _databasePath;

    public MailOrderService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "mailorders.db");
    }

    // 注文する
    public OrderResult PlaceOrder(string username, int productId, int quantity = 1)
    {
        try
        {
            var product = MailOrderCatalog.GetById(productId);
            if (product == null || !product.IsAvailable)
                return new OrderResult { Success = false, Message = "商品が見つかりません" };

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null)
                return new OrderResult { Success = false, Message = "ユーザーが見つかりません" };

            int totalPrice = product.Price * quantity;
            if (user.Gil < totalPrice)
                return new OrderResult { Success = false, Message = "ギルが不足しています" };

            // 注文を作成
            user.Gil -= totalPrice;
            userService.UpdateUser(user);

            var order = new MailOrder
            {
                Username = username,
                ItemName = product.Name,
                ItemType = product.Type,
                Quantity = quantity,
                TotalPrice = totalPrice,
                Status = OrderStatus.Pending,
                OrderedAt = DateTime.UtcNow,
                EstimatedDeliveryAt = DateTime.UtcNow.AddDays(product.DeliveryDays),
                DeliveryDays = product.DeliveryDays
            };

            using var db = new LiteDatabase(_databasePath);
            var orders = db.GetCollection<MailOrder>("mailorders");
            orders.Insert(order);

            return new OrderResult
            {
                Success = true,
                Message = $"{product.Name}を注文しました！{product.DeliveryDays}日後に配達予定",
                OrderId = order.Id,
                EstimatedDeliveryDays = product.DeliveryDays,
                TotalPrice = totalPrice
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("MailOrderService.PlaceOrder error: " + ex.Message);
            return new OrderResult { Success = false, Message = "注文中にエラーが発生しました" };
        }
    }

    // ユーザーの注文一覧を取得
    public List<MailOrder> GetOrders(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var orders = db.GetCollection<MailOrder>("mailorders");
            return orders.Find(o => o.Username == username).OrderByDescending(o => o.OrderedAt).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine("MailOrderService.GetOrders error: " + ex.Message);
            return new List<MailOrder>();
        }
    }

    // 配達可能な注文を確認して配達
    public List<OrderDeliveryResult> CheckAndDeliverOrders(string username)
    {
        try
        {
            var results = new List<OrderDeliveryResult>();
            var now = DateTime.UtcNow;

            using var db = new LiteDatabase(_databasePath);
            var orders = db.GetCollection<MailOrder>("mailorders");
            var userOrders = orders.Find(o => o.Username == username && o.Status == OrderStatus.Pending).ToList();

            var userService = new UserService();

            foreach (var order in userOrders)
            {
                if (order.EstimatedDeliveryAt.HasValue && now >= order.EstimatedDeliveryAt.Value)
                {
                    // 配達
                    order.Status = OrderStatus.Delivered;
                    order.DeliveredAt = now;
                    orders.Update(order);

                    // アイテムをインベントリに追加
                    var item = new InventoryItem
                    {
                        Name = order.ItemName,
                        Type = order.ItemType,
                        Quantity = order.Quantity,
                        Price = order.TotalPrice / order.Quantity
                    };
                    userService.AddItemToUser(username, item);

                    results.Add(new OrderDeliveryResult
                    {
                        ItemName = order.ItemName,
                        Quantity = order.Quantity,
                        OrderId = order.Id
                    });
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine("MailOrderService.CheckAndDeliverOrders error: " + ex.Message);
            return new List<OrderDeliveryResult>();
        }
    }

    // 注文をキャンセル
    public bool CancelOrder(string username, int orderId)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var orders = db.GetCollection<MailOrder>("mailorders");
            var order = orders.FindOne(o => o.Id == orderId && o.Username == username);

            if (order == null || order.Status != OrderStatus.Pending)
                return false;

            // キャンセル
            order.Status = OrderStatus.Cancelled;
            orders.Update(order);

            // 返金
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user != null)
            {
                user.Gil += order.TotalPrice;
                userService.UpdateUser(user);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("MailOrderService.CancelOrder error: " + ex.Message);
            return false;
        }
    }

    // 配達待ちの注文数を取得
    public int GetPendingCount(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var orders = db.GetCollection<MailOrder>("mailorders");
            return orders.Count(o => o.Username == username && o.Status == OrderStatus.Pending);
        }
        catch
        {
            return 0;
        }
    }
}

/// <summary>
/// 注文結果
/// </summary>
public class OrderResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int OrderId { get; set; }
    public int EstimatedDeliveryDays { get; set; }
    public int TotalPrice { get; set; }
}

/// <summary>
/// 配達結果
/// </summary>
public class OrderDeliveryResult
{
    public string ItemName { get; set; } = "";
    public int Quantity { get; set; }
    public int OrderId { get; set; }
}
