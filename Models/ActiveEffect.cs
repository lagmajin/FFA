using System;

namespace FFA.Models;

public enum EffectType
{
    Poison,
    Sleep,
    Paralysis,
    Slow,
    Haste,
    Burn,
    Freeze,
    Blind,
    Silence
}

public class ActiveEffect
{
    public int Id { get; set; }
    // username of owner (for players) or entity id string for enemies
    public string OwnerId { get; set; } = string.Empty;
    public EffectType Type { get; set; }
    // remaining duration in seconds
    public int RemainingSeconds { get; set; }
    // effect potency / strength
    public int Strength { get; set; }
    public string? Source { get; set; }
    public DateTime AppliedAtUtc { get; set; } = DateTime.UtcNow;
}
