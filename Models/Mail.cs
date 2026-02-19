using LiteDB;

namespace FFA.Models;

/// <summary>
/// プレイヤー間メール
/// </summary>
public class Mail
{
    public int Id { get; set; }
    
    // 送信者ID（システムメールの場合はnull）
    public int? SenderId { get; set; }
    
    // 送信者名
    public string SenderName { get; set; } = "";
    
    // 受信者ID
    public int RecipientId { get; set; }
    
    // 受信者名
    public string RecipientName { get; set; } = "";
    
    // 件名
    public string Subject { get; set; } = "";
    
    // 本文
    public string Body { get; set; } = "";
    
    // 添付アイテム
    public MailAttachment? Attachment { get; set; }
    
    // 添付ゴールド
    public int AttachedGil { get; set; } = 0;
    
    // メールタイプ
    public MailType Type { get; set; } = MailType.Player;
    
    // 既読フラグ
    public bool IsRead { get; set; } = false;
    
    // 添付品受取フラグ
    public bool AttachmentClaimed { get; set; } = false;
    
    // 送信日時
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    
    // 有効期限（期限切れで自動削除）
    public DateTime? ExpiresAt { get; set; }
    
    // 削除フラグ
    public bool IsDeleted { get; set; } = false;
}

/// <summary>
/// メールタイプ
/// </summary>
public enum MailType
{
    /// <summary>プレイヤー間メール</summary>
    Player = 0,
    /// <summary>システムメール</summary>
    System = 1,
    /// <summary>GMからのメール</summary>
    GM = 2,
    /// <summary>イベント報酬</summary>
    Event = 3,
    /// <summary>ギルドメール</summary>
    Guild = 4
}

/// <summary>
/// メール添付アイテム
/// </summary>
public class MailAttachment
{
    public string ItemName { get; set; } = "";
    public string ItemType { get; set; } = ""; // Weapon, Armor, Accessory, Consumable, Material
    public int Quantity { get; set; } = 1;
    public int Rarity { get; set; } = 0;
    
    // アイテム固有データ（JSON形式で保存）
    public string? ItemData { get; set; }
}

/// <summary>
/// 簡易メール情報（一覧表示用）
/// </summary>
public class MailSummary
{
    public int MailId { get; set; }
    public string SenderName { get; set; } = "";
    public string Subject { get; set; } = "";
    public MailType Type { get; set; }
    public bool IsRead { get; set; }
    public bool HasAttachment { get; set; }
    public DateTime SentAt { get; set; }
}

/// <summary>
/// メール作成リクエスト
/// </summary>
public class SendMailRequest
{
    public int SenderId { get; set; }
    public string SenderName { get; set; } = "";
    public string RecipientName { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public int AttachedGil { get; set; } = 0;
    public MailAttachment? Attachment { get; set; }
}