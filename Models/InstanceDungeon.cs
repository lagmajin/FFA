namespace FFA.Models;

public enum InstanceStatus { Pending, Active, Completed, Abandoned }

public class InstanceDungeon
{
    public int Id { get; set; }
    public string OwnerUsername { get; set; } = string.Empty; // who created / hosted
    public string Name { get; set; } = "Instance";
    public InstanceStatus Status { get; set; } = InstanceStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(1);

    // simple parameters
    public int FloorCount { get; set; } = 3;
    public int MaxPlayers { get; set; } = 4;

    // participants
    public List<string> Participants { get; set; } = new();

    // reward summary
    public int RewardGil { get; set; } = 500;
    public int RewardExp { get; set; } = 200;
}
