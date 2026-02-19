using LiteDB;

namespace FFA.Models;

/// <summary>
/// フレンド関係を表すモデル
/// </summary>
public class Friend
{
    public int Id { get; set; }
    
    // フレンド申請を送ったユーザーID
    public int RequesterId { get; set; }
    
    // フレンド申請を受け取ったユーザーID
    public int AddresseeId { get; set; }
    
    // フレンド状態（承認済みかどうか）
    public FriendStatus Status { get; set; } = FriendStatus.Pending;
    
    // 作成日時
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // 承認日時
    public DateTime? AcceptedAt { get; set; }
}

/// <summary>
/// フレンド状態
/// </summary>
public enum FriendStatus
{
    /// <summary>承認待ち</summary>
    Pending = 0,
    /// <summary>フレンド承認済み</summary>
    Accepted = 1,
    /// <summary>拒否された</summary>
    Rejected = 2,
    /// <summary>ブロック中</summary>
    Blocked = 3
}

/// <summary>
/// フレンド情報の詳細表示用
/// </summary>
public class FriendInfo
{
    public int FriendId { get; set; }
    public string Username { get; set; } = "";
    public int Level { get; set; }
    public Job Job { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastActiveUtc { get; set; }
    public int? GuildId { get; set; }
    public string? GuildName { get; set; }
}