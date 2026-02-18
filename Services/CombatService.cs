using System;
using FFA.Models;

namespace FFA.Services
{
    // Central combat configuration and helpers (critical hits, etc.)
    public class CombatService
    {
        private readonly Random _rnd = new();

        // Default critical hit chance (fraction). 0.03 => 3%
        public double CritRate { get; set; } = 0.03;
        // Critical damage multiplier (e.g., 1.25 => 25% more damage)
        public double CritMultiplier { get; set; } = 1.25;

        public CombatService()
        {
        }

        // Roll whether this attack is a critical hit. Can be extended to consider job/passives.
        public bool IsCritical(User? user = null)
        {
            // TODO: consider user.PassiveSkills or Job-based modifiers
            return _rnd.NextDouble() < CritRate;
        }

        // Apply critical multiplier to damage (returns floored int, minimum 1)
        public int ApplyCritical(int damage, bool isCritical)
        {
            if (!isCritical) return Math.Max(1, damage);
            var d = Math.Floor(damage * CritMultiplier);
            return (int)Math.Max(1, d);
        }
    }
}
