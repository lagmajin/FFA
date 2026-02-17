namespace FFA.Models;

public enum ChatChannel
{
    World,      // 世界チャット
    Guild,      // ギルドチャット
    Whisper,    // 個人チャット（耳打ち）
    System      // システムメッセージ
}

public class ChatMessage
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ChatChannel Channel { get; set; } = ChatChannel.World;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? TargetUsername { get; set; } // 個人チャットの場合の宛先
    public string? GuildName { get; set; } // ギルド名（ギルドチャット用）
}
