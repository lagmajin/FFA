using FFA.Models;
using LiteDB;

namespace FFA.Services;

public class TitleService
{
    private readonly string _databasePath;
    private readonly ILiteCollection<User> _users;
    private readonly ILiteCollection<UserTitle> _userTitles;

    public TitleService()
    {
        _databasePath = Path.Combine(AppContext.BaseDirectory, "ffa.db");
        var db = new LiteDatabase(_databasePath);
        _users = db.GetCollection<User>("users");
        _userTitles = db.GetCollection<UserTitle>("user_titles");
    }

    /// <summary>
    /// ユーザーの所有称号を取得
    /// </summary>
    public List<Title> GetOwnedTitles(string username)
    {
        var user = _users.FindOne(u => u.Username == username);
        if (user == null) return new List<Title>();

        var titles = new List<Title>();
        foreach (var titleId in user.OwnedTitleIds)
        {
            var title = TitleDatabase.GetById(titleId);
            if (title != null) titles.Add(title);
        }
        return titles;
    }

    /// <summary>
    /// 装备中の称号を取得
    /// </summary>
    public Title? GetEquippedTitle(string username)
    {
        var user = _users.FindOne(u => u.Username == username);
        if (user?.EquippedTitleId == null) return null;
        return TitleDatabase.GetById(user.EquippedTitleId.Value);
    }

    /// <summary>
    /// 称号を装備
    /// </summary>
    public bool EquipTitle(string username, int titleId)
    {
        var user = _users.FindOne(u => u.Username == username);
        if (user == null) return false;

        // ユーザーがその称号を持っているか確認
        if (!user.OwnedTitleIds.Contains(titleId)) return false;

        user.EquippedTitleId = titleId;
        _users.Update(user);
        return true;
    }

    /// <summary>
    /// 称号の装备を解除
    /// </summary>
    public bool UnequipTitle(string username)
    {
        var user = _users.FindOne(u => u.Username == username);
        if (user == null) return false;

        user.EquippedTitleId = null;
        _users.Update(user);
        return true;
    }

    /// <summary>
    /// 称号を獲得
    /// </summary>
    public bool AcquireTitle(string username, int titleId)
    {
        var user = _users.FindOne(u => u.Username == username);
        if (user == null) return false;

        var title = TitleDatabase.GetById(titleId);
        if (title == null) return false;

        // 既に所持しているか確認
        if (user.OwnedTitleIds.Contains(titleId)) return false;

        // 唯一称号か確認
        if (title.IsUnique)
        {
            // 他のユーザーが所持しているか確認（简单检查）
            var existingOwner = _users.FindOne(u => u.OwnedTitleIds.Contains(titleId));
            if (existingOwner != null) return false;
        }

        user.OwnedTitleIds.Add(titleId);
        _users.Update(user);

        // ユーザー称号记录を作成
        var userTitle = new UserTitle
        {
            Username = username,
            TitleId = titleId,
            ObtainedAt = DateTime.UtcNow
        };
        _userTitles.Insert(userTitle);

        return true;
    }

    /// <summary>
    /// 条件に基づいて称号を獲得可能性があるかチェックし、自动獲得
    /// </summary>
    public void CheckAndAcquireTitles(string username)
    {
        var user = _users.FindOne(u => u.Username == username);
        if (user == null) return;

        var newTitles = new List<Title>();

        // レベル称号チェック
        foreach (var title in TitleDatabase.GetUnlocked(user.Level, "level"))
        {
            if (!user.OwnedTitleIds.Contains(title.Id))
            {
                AcquireTitle(username, title.Id);
                newTitles.Add(title);
            }
        }

        // 敵撃破数称号チェック（ユーザーに撃破数が必要）
        // ここでは简单にチェック（実際のプロパティが追加され次第擴張）

        // 転生称号チェック
        if (user.RebirthCount > 0)
        {
            foreach (var title in TitleDatabase.GetUnlocked(user.RebirthCount, "rebirths"))
            {
                if (!user.OwnedTitleIds.Contains(title.Id))
                {
                    AcquireTitle(username, title.Id);
                    newTitles.Add(title);
                }
            }
        }

        // マスター称号チェック
        if (user.IsMaster)
        {
            foreach (var title in TitleDatabase.GetUnlocked(1, "master"))
            {
                if (!user.OwnedTitleIds.Contains(title.Id))
                {
                    AcquireTitle(username, title.Id);
                    newTitles.Add(title);
                }
            }
        }

        // マスターレベル称号チェック
        if (user.MasterLevel > 0)
        {
            foreach (var title in TitleDatabase.GetUnlocked(user.MasterLevel, "master_level"))
            {
                if (!user.OwnedTitleIds.Contains(title.Id))
                {
                    AcquireTitle(username, title.Id);
                    newTitles.Add(title);
                }
            }
        }

        // ゴールド称号チェック
        if (user.Gil >= 10000)
        {
            foreach (var title in TitleDatabase.GetUnlocked(user.Gil, "gold"))
            {
                if (!user.OwnedTitleIds.Contains(title.Id))
                {
                    AcquireTitle(username, title.Id);
                    newTitles.Add(title);
                }
            }
        }

        // ギルド加入称号チェック
        if (user.GuildId.HasValue)
        {
            foreach (var title in TitleDatabase.GetUnlocked(1, "guild"))
            {
                if (!user.OwnedTitleIds.Contains(title.Id))
                {
                    AcquireTitle(username, title.Id);
                    newTitles.Add(title);
                }
            }
        }
    }

    /// <summary>
    /// 獲得可能な称号リストを取得
    /// </summary>
    public List<Title> GetAvailableTitles(string username)
    {
        var user = _users.FindOne(u => u.Username == username);
        if (user == null) return new List<Title>();

        var available = new List<Title>();
        
        // レベル称号
        foreach (var title in TitleDatabase.Titles.Where(t => t.RequirementType == "level"))
        {
            if (!user.OwnedTitleIds.Contains(title.Id) && title.Requirement <= user.Level)
                available.Add(title);
        }

        // 転生称号
        foreach (var title in TitleDatabase.Titles.Where(t => t.RequirementType == "rebirths"))
        {
            if (!user.OwnedTitleIds.Contains(title.Id) && title.Requirement <= user.RebirthCount)
                available.Add(title);
        }

        // マスター称号
        foreach (var title in TitleDatabase.Titles.Where(t => t.RequirementType == "master"))
        {
            if (!user.OwnedTitleIds.Contains(title.Id) && user.IsMaster && title.Requirement == 1)
                available.Add(title);
        }

        // マスターレベル称号
        foreach (var title in TitleDatabase.Titles.Where(t => t.RequirementType == "master_level"))
        {
            if (!user.OwnedTitleIds.Contains(title.Id) && title.Requirement <= user.MasterLevel)
                available.Add(title);
        }

        // ゴールド称号
        foreach (var title in TitleDatabase.Titles.Where(t => t.RequirementType == "gold"))
        {
            if (!user.OwnedTitleIds.Contains(title.Id) && title.Requirement <= user.Gil)
                available.Add(title);
        }

        // ギルド称号
        foreach (var title in TitleDatabase.Titles.Where(t => t.RequirementType == "guild"))
        {
            if (!user.OwnedTitleIds.Contains(title.Id) && user.GuildId.HasValue && title.Requirement == 1)
                available.Add(title);
        }

        return available;
    }

    /// <summary>
    /// 未獲得のシークレット称号リストを取得
    /// </summary>
    public List<Title> GetSecretTitles(string username)
    {
        var user = _users.FindOne(u => u.Username == username);
        if (user == null) return new List<Title>();

        return TitleDatabase.Titles
            .Where(t => t.IsSecret && !user.OwnedTitleIds.Contains(t.Id))
            .ToList();
    }

    /// <summary>
    /// 称号の説明を取得（含まないように伏字）
    /// </summary>
    public string GetTitleDisplayName(Title title, bool isOwned)
    {
        if (title.IsSecret && !isOwned)
        return "???";
        return title.Name;
    }

    public string GetTitleDisplayDescription(Title title, bool isOwned)
    {
        if (title.IsSecret && !isOwned)
            return "未確認の称号";
        return title.Description;
    }
}
