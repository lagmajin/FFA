namespace FFA.Models;

public class Guild
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string LeaderName { get; set; } = ""; // リーダー名
    public int MemberCount { get; set; } = 1;
    public int TotalExp { get; set; } = 0; // guild経験値
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // 拡張機能
    public List<int> LearnableSkills { get; set; } = new(); // 習得済みスキルIDリスト
}
