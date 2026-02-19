using FFA.Models;
using LiteDB;
using System.IO;

namespace FFA.Services;

/// <summary>
/// フレンドシステムのサービス
/// </summary>
public class FriendService
{
    private readonly string _databasePath;

    public FriendService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "users.db");
    }

    private LiteDatabase GetDatabase() => new LiteDatabase(_databasePath);

    /// <summary>
    /// フレンド申請を送信
    /// </summary>
    public FriendResult SendRequest(int requesterId, int addresseeId)
    {
        if (requesterId == addresseeId)
            return FriendResult.Failed("自分自身をフレンドに追加することはできません。");

        using var db = GetDatabase();
        var friends = db.GetCollection<Friend>("friends");
        friends.EnsureIndex(x => x.RequesterId);
        friends.EnsureIndex(x => x.AddresseeId);

        var existing = friends.FindOne(f =>
            (f.RequesterId == requesterId && f.AddresseeId == addresseeId) ||
            (f.RequesterId == addresseeId && f.AddresseeId == requesterId));

        if (existing != null)
        {
            return existing.Status switch
            {
                FriendStatus.Pending => FriendResult.Failed("既にフレンド申請中です。"),
                FriendStatus.Accepted => FriendResult.Failed("既にフレンドです。"),
                FriendStatus.Blocked => FriendResult.Failed("このユーザーはブロックされています。"),
                _ => FriendResult.Failed("既にフレンド申請を送っています。")
            };
        }

        var friend = new Friend
        {
            RequesterId = requesterId,
            AddresseeId = addresseeId,
            Status = FriendStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        friends.Insert(friend);
        return FriendResult.Successful("フレンド申請を送信しました。", friend);
    }

    /// <summary>
    /// フレンド申請を承認
    /// </summary>
    public FriendResult AcceptRequest(int userId, int requestId)
    {
        using var db = GetDatabase();
        var friends = db.GetCollection<Friend>("friends");
        
        var friend = friends.FindById(requestId);
        if (friend == null)
            return FriendResult.Failed("申請が見つかりません。");

        if (friend.AddresseeId != userId)
            return FriendResult.Failed("この申請を承認する権限がありません。");

        if (friend.Status != FriendStatus.Pending)
            return FriendResult.Failed("この申請は既に処理されています。");

        friend.Status = FriendStatus.Accepted;
        friend.AcceptedAt = DateTime.UtcNow;
        friends.Update(friend);

        return FriendResult.Successful("フレンド申請を承認しました。", friend);
    }

    /// <summary>
    /// フレンド申請を拒否
    /// </summary>
    public FriendResult RejectRequest(int userId, int requestId)
    {
        using var db = GetDatabase();
        var friends = db.GetCollection<Friend>("friends");
        
        var friend = friends.FindById(requestId);
        if (friend == null)
            return FriendResult.Failed("申請が見つかりません。");

        if (friend.AddresseeId != userId)
            return FriendResult.Failed("この申請を拒否する権限がありません。");

        friend.Status = FriendStatus.Rejected;
        friends.Update(friend);

        return FriendResult.Successful("フレンド申請を拒否しました。");
    }

    /// <summary>
    /// フレンドを削除
    /// </summary>
    public FriendResult RemoveFriend(int userId, int friendUserId)
    {
        using var db = GetDatabase();
        var friends = db.GetCollection<Friend>("friends");
        
        var friend = friends.FindOne(f =>
            ((f.RequesterId == userId && f.AddresseeId == friendUserId) ||
             (f.RequesterId == friendUserId && f.AddresseeId == userId)) &&
            f.Status == FriendStatus.Accepted);

        if (friend == null)
            return FriendResult.Failed("フレンド関係が見つかりません。");

        friends.Delete(friend.Id);
        return FriendResult.Successful("フレンドを削除しました。");
    }

    /// <summary>
    /// ユーザーをブロック
    /// </summary>
    public FriendResult BlockUser(int userId, int targetUserId)
    {
        if (userId == targetUserId)
            return FriendResult.Failed("自分自身をブロックすることはできません。");

        using var db = GetDatabase();
        var friends = db.GetCollection<Friend>("friends");

        var existing = friends.FindOne(f =>
            f.RequesterId == userId && f.AddresseeId == targetUserId);

        if (existing != null)
        {
            existing.Status = FriendStatus.Blocked;
            friends.Update(existing);
            return FriendResult.Successful("ユーザーをブロックしました。");
        }

        var block = new Friend
        {
            RequesterId = userId,
            AddresseeId = targetUserId,
            Status = FriendStatus.Blocked,
            CreatedAt = DateTime.UtcNow
        };

        friends.Insert(block);
        return FriendResult.Successful("ユーザーをブロックしました。");
    }

    /// <summary>
    /// ブロックを解除
    /// </summary>
    public FriendResult UnblockUser(int userId, int targetUserId)
    {
        using var db = GetDatabase();
        var friends = db.GetCollection<Friend>("friends");

        var block = friends.FindOne(f =>
            f.RequesterId == userId &&
            f.AddresseeId == targetUserId &&
            f.Status == FriendStatus.Blocked);

        if (block == null)
            return FriendResult.Failed("ブロックが見つかりません。");

        friends.Delete(block.Id);
        return FriendResult.Successful("ブロックを解除しました。");
    }

    /// <summary>
    /// フレンド一覧を取得
    /// </summary>
    public List<FriendInfo> GetFriends(int userId)
    {
        using var db = GetDatabase();
        var friends = db.GetCollection<Friend>("friends");
        var users = db.GetCollection<User>("users");

        var friendRelations = friends.Find(f =>
            (f.RequesterId == userId || f.AddresseeId == userId) &&
            f.Status == FriendStatus.Accepted).ToList();

        var result = new List<FriendInfo>();
        foreach (var rel in friendRelations)
        {
            var friendId = rel.RequesterId == userId ? rel.AddresseeId : rel.RequesterId;
            var friendUser = users.FindById(friendId);
            if (friendUser == null) continue;

            result.Add(new FriendInfo
            {
                FriendId = friendId,
                Username = friendUser.Username,
                Level = friendUser.Level,
                Job = friendUser.Job,
                IsOnline = (DateTime.UtcNow - friendUser.LastActiveUtc).TotalMinutes < 5,
                LastActiveUtc = friendUser.LastActiveUtc,
                GuildId = friendUser.GuildId
            });
        }

        return result;
    }

    /// <summary>
    /// 受信したフレンド申請一覧を取得
    /// </summary>
    public List<FriendRequestInfo> GetPendingRequests(int userId)
    {
        using var db = GetDatabase();
        var friends = db.GetCollection<Friend>("friends");
        var users = db.GetCollection<User>("users");

        var requests = friends.Find(f =>
            f.AddresseeId == userId &&
            f.Status == FriendStatus.Pending).ToList();

        var result = new List<FriendRequestInfo>();
        foreach (var req in requests)
        {
            var requester = users.FindById(req.RequesterId);
            if (requester == null) continue;

            result.Add(new FriendRequestInfo
            {
                RequestId = req.Id,
                RequesterId = req.RequesterId,
                RequesterName = requester.Username,
                RequesterLevel = requester.Level,
                RequesterJob = requester.Job,
                CreatedAt = req.CreatedAt
            });
        }

        return result;
    }

    /// <summary>
    /// 送信したフレンド申請一覧を取得
    /// </summary>
    public List<FriendRequestInfo> GetSentRequests(int userId)
    {
        using var db = GetDatabase();
        var friends = db.GetCollection<Friend>("friends");
        var users = db.GetCollection<User>("users");

        var requests = friends.Find(f =>
            f.RequesterId == userId &&
            f.Status == FriendStatus.Pending).ToList();

        var result = new List<FriendRequestInfo>();
        foreach (var req in requests)
        {
            var addressee = users.FindById(req.AddresseeId);
            if (addressee == null) continue;

            result.Add(new FriendRequestInfo
            {
                RequestId = req.Id,
                RequesterId = userId,
                RequesterName = addressee.Username,
                RequesterLevel = addressee.Level,
                RequesterJob = addressee.Job,
                CreatedAt = req.CreatedAt
            });
        }

        return result;
    }

    /// <summary>
    /// フレンド数を取得
    /// </summary>
    public int GetFriendCount(int userId)
    {
        using var db = GetDatabase();
        var friends = db.GetCollection<Friend>("friends");
        return friends.Count(f =>
            (f.RequesterId == userId || f.AddresseeId == userId) &&
            f.Status == FriendStatus.Accepted);
    }

    /// <summary>
    /// フレンドかどうかを確認
    /// </summary>
    public bool AreFriends(int userId1, int userId2)
    {
        using var db = GetDatabase();
        var friends = db.GetCollection<Friend>("friends");
        return friends.Exists(f =>
            ((f.RequesterId == userId1 && f.AddresseeId == userId2) ||
             (f.RequesterId == userId2 && f.AddresseeId == userId1)) &&
            f.Status == FriendStatus.Accepted);
    }
}

/// <summary>
/// フレンド操作の結果
/// </summary>
public class FriendResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = "";
    public Friend? FriendData { get; set; }

    public static FriendResult Successful(string message, Friend? friend = null) =>
        new() { IsSuccess = true, Message = message, FriendData = friend };

    public static FriendResult Failed(string message) =>
        new() { IsSuccess = false, Message = message };
}

/// <summary>
/// フレンド申請情報
/// </summary>
public class FriendRequestInfo
{
    public int RequestId { get; set; }
    public int RequesterId { get; set; }
    public string RequesterName { get; set; } = "";
    public int RequesterLevel { get; set; }
    public Job RequesterJob { get; set; }
    public DateTime CreatedAt { get; set; }
}