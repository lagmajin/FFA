namespace FFA.Models;

/// <summary>
/// ユーザーが習得したスキル
/// </summary>
public class UserSkill
{
    public string SkillId { get; set; } = "";
    public int CurrentLevel { get; set; } = 0;
    public DateTime LearnedAt { get; set; } = DateTime.UtcNow;
}
