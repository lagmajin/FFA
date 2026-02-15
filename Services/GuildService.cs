namespace FFA.Services;

public class GuildService
{
    private static int _nextId = 1;
    private static readonly List<Models.Guild> _guilds = new();

    // 全ギルド取得
    public List<Models.Guild> GetAllGuilds()
    {
        return _guilds.ToList();
    }

    // ギルド作成
    public Models.Guild CreateGuild(string name, string description, string leaderName)
    {
        var guild = new Models.Guild
        {
            Id = _nextId++,
            Name = name,
            Description = description,
            LeaderName = leaderName,
            MemberCount = 1,
            TotalExp = 0,
            CreatedAt = DateTime.Now
        };
        _guilds.Add(guild);
        return guild;
    }

    // ギルドに参加
    public bool JoinGuild(Models.User user, int guildId)
    {
        var guild = _guilds.FirstOrDefault(g => g.Id == guildId);
        if (guild == null) return false;
        
        user.GuildId = guildId;
        guild.MemberCount++;
        return true;
    }

    // ギルド脱退
    public bool LeaveGuild(Models.User user)
    {
        if (user.GuildId == null) return false;
        
        var guild = _guilds.FirstOrDefault(g => g.Id == user.GuildId);
        if (guild != null)
        {
            guild.MemberCount--;
            if (guild.MemberCount <= 0)
            {
                _guilds.Remove(guild);
            }
        }
        
        user.GuildId = null;
        return true;
    }

    // ユーザーのギルド取得
    public Models.Guild? GetUserGuild(Models.User user)
    {
        if (user.GuildId == null) return null;
        return _guilds.FirstOrDefault(g => g.Id == user.GuildId);
    }

    // ギルド経験値追加（ダンジョンクリア時など）
    public void AddGuildExp(Models.User user, int exp)
    {
        if (user.GuildId == null) return;
        
        var guild = _guilds.FirstOrDefault(g => g.Id == user.GuildId);
        if (guild != null)
        {
            guild.TotalExp += exp;
        }
    }

    // ギルド解散（リーダーのみ）
    public bool DisbandGuild(Models.User user)
    {
        var guild = GetUserGuild(user);
        if (guild == null || guild.LeaderName != user.Username) return false;
        
        // 全員のギルドIDをクリア
        // 注意: ここでは簡易的に、全ユーザーの GuildId をクリアする必要があります
        // 本来はユーザーサービスと連携
        
        _guilds.Remove(guild);
        return true;
    }
}
