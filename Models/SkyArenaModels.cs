namespace FFA.Models;

public class Opponent
{
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "⚔️";
    public int Level { get; set; }
    public int HP { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int RewardGil { get; set; }
    public int RewardExp { get; set; }
}

public class BattleResult
{
    public bool Win { get; set; }
    public string Message { get; set; } = "";
    public int RewardGil { get; set; }
    public int RewardExp { get; set; }
}