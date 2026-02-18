using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;

namespace FFA.Services
{
    public class AuctionService
    {
        private readonly string _databasePath;

        public AuctionService()
        {
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _databasePath = Path.Combine(appDataPath, "auctions.db");
        }

        public AuctionResult CreateAuction(string seller, string itemName, int quantity, int startingPrice, int? buyoutPrice, TimeSpan duration)
        {
            try
            {
                using var db = new LiteDatabase(_databasePath);
                var col = db.GetCollection<Auction>("auctions");
                var auction = new Auction
                {
                    Seller = seller,
                    ItemName = itemName,
                    Quantity = quantity,
                    StartingPrice = startingPrice,
                    BuyoutPrice = buyoutPrice,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.Add(duration),
                    Status = AuctionStatus.Active,
                    Bids = new List<Bid>()
                };

                col.Insert(auction);
                return new AuctionResult { Success = true, Message = "出品が作成されました。", AuctionId = auction.Id };
            }
            catch (Exception ex)
            {
                return new AuctionResult { Success = false, Message = ex.Message };
            }
        }

        public AuctionResult PlaceBid(int auctionId, string bidder, int amount)
        {
            try
            {
                using var db = new LiteDatabase(_databasePath);
                var col = db.GetCollection<Auction>("auctions");
                var auction = col.FindById(auctionId);
                if (auction == null) return new AuctionResult { Success = false, Message = "オークションが見つかりません。" };
                if (auction.Status != AuctionStatus.Active) return new AuctionResult { Success = false, Message = "オークションはアクティブではありません。" };
                var highest = auction.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
                int minAccept = highest?.Amount + 1 ?? auction.StartingPrice;
                if (amount < minAccept) return new AuctionResult { Success = false, Message = $"入札額は最低 {minAccept} 以上である必要があります。" };

                // buyout
                if (auction.BuyoutPrice.HasValue && amount >= auction.BuyoutPrice.Value)
                {
                    // immediate win
                    auction.Bids.Add(new Bid { Bidder = bidder, Amount = auction.BuyoutPrice.Value, Time = DateTime.UtcNow });
                    auction.Status = AuctionStatus.Closed;
                    auction.Winner = bidder;
                    auction.FinalPrice = auction.BuyoutPrice.Value;
                    col.Update(auction);
                    return new AuctionResult { Success = true, Message = "即決購入で落札しました。", AuctionId = auction.Id };
                }

                auction.Bids.Add(new Bid { Bidder = bidder, Amount = amount, Time = DateTime.UtcNow });
                col.Update(auction);
                // update bidder last active
                var userService = new UserService();
                var b = userService.GetByUsername(bidder);
                if (b != null) { b.LastActiveUtc = DateTime.UtcNow; userService.UpdateUser(b); }
                return new AuctionResult { Success = true, Message = "入札が受け付けられました。", AuctionId = auction.Id };
            }
            catch (Exception ex)
            {
                return new AuctionResult { Success = false, Message = ex.Message };
            }
        }

        public List<Auction> GetActiveAuctions()
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Auction>("auctions");
            return col.Find(a => a.Status == AuctionStatus.Active).ToList();
        }

        public Auction? GetAuctionById(int id)
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Auction>("auctions");
            return col.FindById(id);
        }

        public void CloseExpiredAuctions()
        {
            using var db = new LiteDatabase(_databasePath);
            var col = db.GetCollection<Auction>("auctions");
            var now = DateTime.UtcNow;
            var expired = col.Find(a => a.Status == AuctionStatus.Active && a.EndTime <= now).ToList();
            foreach (var a in expired)
            {
                a.Status = AuctionStatus.Closed;
                var highest = a.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
                if (highest != null)
                {
                    a.Winner = highest.Bidder;
                    a.FinalPrice = highest.Amount;
                }
                col.Update(a);
            }
        }
    }

    public class Auction
    {
        public int Id { get; set; }
        public string Seller { get; set; } = "";
        public string ItemName { get; set; } = "";
        public int Quantity { get; set; }
        public int StartingPrice { get; set; }
        public int? BuyoutPrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AuctionStatus Status { get; set; }
        public List<Bid> Bids { get; set; } = new();
        public string? Winner { get; set; }
        public int? FinalPrice { get; set; }
    }

    public class Bid
    {
        public string Bidder { get; set; } = "";
        public int Amount { get; set; }
        public DateTime Time { get; set; }
    }

    public enum AuctionStatus { Active, Closed, Cancelled }

    public class AuctionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int AuctionId { get; set; }
    }
}
