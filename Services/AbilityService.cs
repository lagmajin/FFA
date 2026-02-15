using System.Collections.Concurrent;
using FFA.Models;

namespace FFA.Services;

public class AbilityService
{
    // store definitions of abilities
    private readonly List<Ability> _abilities = new()
    {
        new Ability { Name = "Heal", Description = "Restore 30 HP", Cost = 0, CooldownSeconds = 10, EffectType = "Heal", EffectValue = 30 },
        new Ability { Name = "Power Strike", Description = "Deal extra damage next attack", Cost = 0, CooldownSeconds = 15, EffectType = "BuffDamage", EffectValue = 10 },
        new Ability { Name = "Karma Boost", Description = "Increase karma by 1", Cost = 0, CooldownSeconds = 60, EffectType = "Karma", EffectValue = 1 }
    };

    // per-user cooldown tracking
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, DateTime>> _cooldowns = new();

    public IReadOnlyList<Ability> GetAllAbilities() => _abilities;

    public Ability? GetById(string id) => _abilities.FirstOrDefault(a => a.Id == id);

    public void AssignStarterAbilities(User user)
    {
        if (user == null) return;
        if (user.Abilities == null) user.Abilities = new List<Ability>();
        if (!user.Abilities.Any(a => a.Name == "Heal"))
            user.Abilities.Add(_abilities.First(a => a.Name == "Heal"));
        if (!user.Abilities.Any(a => a.Name == "Power Strike"))
            user.Abilities.Add(_abilities.First(a => a.Name == "Power Strike"));
        // ensure status exists
        if (user.Status == null) user.Status = new PlayerStatus();
    }

    public bool CanUse(string username, string abilityId)
    {
        var ab = GetById(abilityId);
        if (ab == null) return false;
        var userDict = _cooldowns.GetOrAdd(username, new ConcurrentDictionary<string, DateTime>());
        if (userDict.TryGetValue(abilityId, out var until))
        {
            return DateTime.UtcNow >= until;
        }
        return true;
    }

    public void SetCooldown(string username, string abilityId, int seconds)
    {
        var userDict = _cooldowns.GetOrAdd(username, new ConcurrentDictionary<string, DateTime>());
        userDict[abilityId] = DateTime.UtcNow.AddSeconds(seconds);
    }

    // Apply the ability effect (simple built-in effects)
    public string UseAbility(User user, string abilityId)
    {
        if (user == null) return "No user";
        var ab = GetById(abilityId);
        if (ab == null) return "Ability not found";
        if (!CanUse(user.Username, ab.Id)) return "On cooldown";

        // Apply simple effects
        switch (ab.EffectType)
        {
            case "Heal":
                user.HP = Math.Min(user.MaxHP, user.HP + ab.EffectValue);
                break;
            case "BuffDamage":
                // For demo, add Exp as proxy for damage buff
                user.Exp += ab.EffectValue;
                break;
            case "Karma":
                // no direct karma change here; leave to KarmaService usually
                break;
        }

        SetCooldown(user.Username, ab.Id, ab.CooldownSeconds);
        return "OK";
    }
}
