using FFA.Models;
using LiteDB;
using System.IO;

namespace FFA.Services;

/// <summary>
/// メールシステムのサービス
/// </summary>
public class MailService
{
    private readonly string _databasePath;
    private const int MaxMailsPerUser = 100;
    private const int DefaultExpiryDays = 30;

    public MailService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "users.db");
    }

    private LiteDatabase GetDatabase() => new LiteDatabase(_databasePath);

    /// <summary>
    /// プレイヤー間メールを送信
    /// </summary>
    public MailResult SendMail(SendMailRequest request)
    {
        using var db = GetDatabase();
        var mails = db.GetCollection<Mail>("mails");
        var users = db.GetCollection<User>("users");

        var sender = users.FindById(request.SenderId);
        if (sender == null)
            return MailResult.Failed("送信者が見つかりません。");

        var recipient = users.FindOne(u => u.Username == request.RecipientName);
        if (recipient == null)
            return MailResult.Failed("受信者が見つかりません。");

        var currentCount = mails.Count(m => m.RecipientId == recipient.Id && !m.IsDeleted);
        if (currentCount >= MaxMailsPerUser)
            return MailResult.Failed("受信者のメールボックスが満杯です。");

        if (request.AttachedGil > 0 && sender.Gil < request.AttachedGil)
            return MailResult.Failed("所持金が不足しています。");

        if (request.AttachedGil > 0)
        {
            sender.Gil -= request.AttachedGil;
            users.Update(sender);
        }

        var mail = new Mail
        {
            SenderId = request.SenderId,
            SenderName = request.SenderName,
            RecipientId = recipient.Id,
            RecipientName = recipient.Username,
            Subject = request.Subject,
            Body = request.Body,
            AttachedGil = request.AttachedGil,
            Attachment = request.Attachment,
            Type = MailType.Player,
            SentAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(DefaultExpiryDays)
        };

        mails.Insert(mail);
        return MailResult.Success("メールを送信しました。", mail);
    }

    /// <summary>
    /// システムメールを送信
    /// </summary>
    public MailResult SendSystemMail(int recipientId, string subject, string body, int attachedGil = 0, MailAttachment? attachment = null)
    {
        using var db = GetDatabase();
        var mails = db.GetCollection<Mail>("mails");
        var users = db.GetCollection<User>("users");

        var recipient = users.FindById(recipientId);
        if (recipient == null)
            return MailResult.Failed("受信者が見つかりません。");

        var mail = new Mail
        {
            SenderId = null,
            SenderName = "システム",
            RecipientId = recipientId,
            RecipientName = recipient.Username,
            Subject = subject,
            Body = body,
            AttachedGil = attachedGil,
            Attachment = attachment,
            Type = MailType.System,
            SentAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(DefaultExpiryDays)
        };

        mails.Insert(mail);
        return MailResult.Success("システムメールを送信しました。", mail);
    }

    /// <summary>
    /// イベント報酬メールを送信
    /// </summary>
    public MailResult SendEventRewardMail(int recipientId, string eventName, string body, int attachedGil = 0, MailAttachment? attachment = null)
    {
        using var db = GetDatabase();
        var mails = db.GetCollection<Mail>("mails");
        var users = db.GetCollection<User>("users");

        var recipient = users.FindById(recipientId);
        if (recipient == null)
            return MailResult.Failed("受信者が見つかりません。");

        var mail = new Mail
        {
            SenderId = null,
            SenderName = "イベント",
            RecipientId = recipientId,
            RecipientName = recipient.Username,
            Subject = $"【イベント報酬】{eventName}",
            Body = body,
            AttachedGil = attachedGil,
            Attachment = attachment,
            Type = MailType.Event,
            SentAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(DefaultExpiryDays * 2)
        };

        mails.Insert(mail);
        return MailResult.Success("イベント報酬メールを送信しました。", mail);
    }

    /// <summary>
    /// メール一覧を取得
    /// </summary>
    public List<MailSummary> GetMailList(int userId, int page = 1, int pageSize = 20)
    {
        using var db = GetDatabase();
        var mails = db.GetCollection<Mail>("mails");

        return mails.Find(m => m.RecipientId == userId && !m.IsDeleted)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MailSummary
            {
                MailId = m.Id,
                SenderName = m.SenderName,
                Subject = m.Subject,
                Type = m.Type,
                IsRead = m.IsRead,
                HasAttachment = m.Attachment != null || m.AttachedGil > 0,
                SentAt = m.SentAt
            }).ToList();
    }

    /// <summary>
    /// メール詳細を取得
    /// </summary>
    public Mail? GetMail(int userId, int mailId)
    {
        using var db = GetDatabase();
        var mails = db.GetCollection<Mail>("mails");

        var mail = mails.FindById(mailId);
        if (mail == null || mail.RecipientId != userId || mail.IsDeleted)
            return null;

        if (!mail.IsRead)
        {
            mail.IsRead = true;
            mails.Update(mail);
        }

        return mail;
    }

    /// <summary>
    /// 添付品を受け取る
    /// </summary>
    public MailResult ClaimAttachment(int userId, int mailId)
    {
        using var db = GetDatabase();
        var mails = db.GetCollection<Mail>("mails");
        var users = db.GetCollection<User>("users");

        var mail = mails.FindById(mailId);
        if (mail == null || mail.RecipientId != userId)
            return MailResult.Failed("メールが見つかりません。");

        if (mail.AttachmentClaimed)
            return MailResult.Failed("既に添付品を受け取っています。");

        if (mail.Attachment == null && mail.AttachedGil <= 0)
            return MailResult.Failed("添付品がありません。");

        var user = users.FindById(userId);
        if (user == null)
            return MailResult.Failed("ユーザーが見つかりません。");

        if (mail.AttachedGil > 0)
            user.Gil += mail.AttachedGil;

        if (mail.Attachment != null)
        {
            user.Inventory.Add(new InventoryItem
            {
                Name = mail.Attachment.ItemName,
                Type = mail.Attachment.ItemType,
                Quantity = mail.Attachment.Quantity
            });
        }

        users.Update(user);

        mail.AttachmentClaimed = true;
        mails.Update(mail);

        return MailResult.Success("添付品を受け取りました。", mail);
    }

    /// <summary>
    /// メールを削除
    /// </summary>
    public MailResult DeleteMail(int userId, int mailId)
    {
        using var db = GetDatabase();
        var mails = db.GetCollection<Mail>("mails");

        var mail = mails.FindById(mailId);
        if (mail == null || mail.RecipientId != userId)
            return MailResult.Failed("メールが見つかりません。");

        if (!mail.AttachmentClaimed && (mail.Attachment != null || mail.AttachedGil > 0))
            return MailResult.Failed("添付品未受取のメールは削除できません。");

        mail.IsDeleted = true;
        mails.Update(mail);

        return MailResult.Success("メールを削除しました。");
    }

    /// <summary>
    /// 未読メール数を取得
    /// </summary>
    public int GetUnreadCount(int userId)
    {
        using var db = GetDatabase();
        var mails = db.GetCollection<Mail>("mails");
        return mails.Count(m => m.RecipientId == userId && !m.IsRead && !m.IsDeleted);
    }
}

/// <summary>
/// メール操作の結果
/// </summary>
public class MailResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = "";
    public Mail? MailData { get; set; }

    public static MailResult Success(string message, Mail? mail = null) =>
        new() { IsSuccess = true, Message = message, MailData = mail };

    public static MailResult Failed(string message) =>
        new() { IsSuccess = false, Message = message };
}