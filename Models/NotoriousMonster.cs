namespace FFA.Models;

public enum NMStatus { Dormant, Alive, Dead }

public class NotoriousMonster
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty; // location identifier or name
    public NMStatus Status { get; set; } = NMStatus.Dormant;
    public DateTime? SpawnedAtUtc { get; set; }
    public DateTime? LastKilledAtUtc { get; set; }
    public TimeSpan RespawnInterval { get; set; } = TimeSpan.FromHours(6);

    // Combat stats
    public int MaxHP { get; set; }
    public int CurrentHP { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }

    // Rewards
    public int RewardExp { get; set; }
    public int RewardGil { get; set; }
    public string? DropItem { get; set; }
    public int DropRate { get; set; } = 10; // percent

    // Optional: who killed it last
    public string? LastKilledBy { get; set; }
}
