namespace FFA.Models;

public enum QuestType
{
    Slay,      // 討伐（敵を倒す）
    Collect    // 収集（アイテムを届ける）
}

public class Quest
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public QuestType Type { get; set; }
    public string Target { get; set; } = ""; // 敵名またはアイテム名
    public int TargetCount { get; set; } = 1; // 必要数
    public int CurrentCount { get; set; } = 0; // 現在の進行
    public int RewardGil { get; set; }
    public int RewardExp { get; set; }
    public bool IsCompleted { get; set; } = false;
}
