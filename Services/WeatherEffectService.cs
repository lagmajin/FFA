using System;
using System.Collections.Generic;

namespace FFA.Services
{
    public enum ExtendedWeatherType
    {
        Clear,      // 晴れ
        Cloudy,     // 曇り
        Rain,       // 雨
        Storm,      // 嵐
        Snow,       // 雪
        Fog,        // 霧
        Blizzard,   // 吹雪
        Thunder,    // 雷雨
        HeatWave,   // 猛暑
        ColdWave    // 寒波
    }

    public enum WeatherBonusType
    {
        DropRate,       // ドロップ率
        RareDropRate,   // レアドロップ率
        ExpRate,        // 経験値
        BossSpawnRate,  // ボス出現率
        NMSpawnRate,    // NM出現率
        StealRate,      // 盗む成功率
        CatchRate,      // 捕獲成功率
        FishingRate,    // 釣り成功率
        MiningRate,     // 採掘成功率
        StaminaRegen    // スタミナ回復
    }

    public class WeatherEffectService
    {
        private readonly TimeWeatherService _timeWeatherService;
        private readonly Random _random = new();

        // Weather effect multipliers
        private static readonly Dictionary<ExtendedWeatherType, Dictionary<WeatherBonusType, double>> WeatherBonuses = new()
        {
            // Clear (晴れ) - baseline
            {
                ExtendedWeatherType.Clear, new Dictionary<WeatherBonusType, double>
                {
                    { WeatherBonusType.DropRate, 1.0 },
                    { WeatherBonusType.RareDropRate, 1.0 },
                    { WeatherBonusType.ExpRate, 1.0 },
                    { WeatherBonusType.BossSpawnRate, 1.0 },
                    { WeatherBonusType.NMSpawnRate, 1.0 },
                    { WeatherBonusType.StealRate, 1.2 },
                    { WeatherBonusType.CatchRate, 1.2 },
                    { WeatherBonusType.FishingRate, 1.3 },
                    { WeatherBonusType.MiningRate, 1.2 },
                    { WeatherBonusType.StaminaRegen, 1.2 }
                }
            },
            // Cloudy (曇り)
            {
                ExtendedWeatherType.Cloudy, new Dictionary<WeatherBonusType, double>
                {
                    { WeatherBonusType.DropRate, 1.1 },
                    { WeatherBonusType.RareDropRate, 1.1 },
                    { WeatherBonusType.ExpRate, 1.0 },
                    { WeatherBonusType.BossSpawnRate, 1.0 },
                    { WeatherBonusType.NMSpawnRate, 1.0 },
                    { WeatherBonusType.StealRate, 1.1 },
                    { WeatherBonusType.CatchRate, 1.1 },
                    { WeatherBonusType.FishingRate, 1.1 },
                    { WeatherBonusType.MiningRate, 1.1 },
                    { WeatherBonusType.StaminaRegen, 1.1 }
                }
            },
            // Rain (雨) - good for drops and fishing
            {
                ExtendedWeatherType.Rain, new Dictionary<WeatherBonusType, double>
                {
                    { WeatherBonusType.DropRate, 1.3 },
                    { WeatherBonusType.RareDropRate, 1.5 },
                    { WeatherBonusType.ExpRate, 1.1 },
                    { WeatherBonusType.BossSpawnRate, 1.2 },
                    { WeatherBonusType.NMSpawnRate, 1.3 },
                    { WeatherBonusType.StealRate, 1.0 },
                    { WeatherBonusType.CatchRate, 0.8 },
                    { WeatherBonusType.FishingRate, 1.5 }, // 雨の日には魚が釣りやすい
                    { WeatherBonusType.MiningRate, 0.9 },
                    { WeatherBonusType.StaminaRegen, 0.9 }
                }
            },
            // Storm (嵐) - rare spawning
            {
                ExtendedWeatherType.Storm, new Dictionary<WeatherBonusType, double>
                {
                    { WeatherBonusType.DropRate, 1.5 },
                    { WeatherBonusType.RareDropRate, 2.0 },
                    { WeatherBonusType.ExpRate, 1.3 },
                    { WeatherBonusType.BossSpawnRate, 1.5 },
                    { WeatherBonusType.NMSpawnRate, 2.0 }, // NM出現率大幅アップ
                    { WeatherBonusType.StealRate, 0.8 },
                    { WeatherBonusType.CatchRate, 0.5 },
                    { WeatherBonusType.FishingRate, 2.0 }, // 嵐の日は大型魚
                    { WeatherBonusType.MiningRate, 0.7 },
                    { WeatherBonusType.StaminaRegen, 0.7 }
                }
            },
            // Snow (雪)
            {
                ExtendedWeatherType.Snow, new Dictionary<WeatherBonusType, double>
                {
                    { WeatherBonusType.DropRate, 1.2 },
                    { WeatherBonusType.RareDropRate, 1.3 },
                    { WeatherBonusType.ExpRate, 1.2 },
                    { WeatherBonusType.BossSpawnRate, 1.1 },
                    { WeatherBonusType.NMSpawnRate, 1.2 },
                    { WeatherBonusType.StealRate, 1.0 },
                    { WeatherBonusType.CatchRate, 1.3 }, // 雪の動物
                    { WeatherBonusType.FishingRate, 0.8 },
                    { WeatherBonusType.MiningRate, 1.2 }, // 凍土
                    { WeatherBonusType.StaminaRegen, 0.8 }
                }
            },
            // Fog (霧) - stealth activities
            {
                ExtendedWeatherType.Fog, new Dictionary<WeatherBonusType, double>
                {
                    { WeatherBonusType.DropRate, 1.1 },
                    { WeatherBonusType.RareDropRate, 1.2 },
                    { WeatherBonusType.ExpRate, 1.0 },
                    { WeatherBonusType.BossSpawnRate, 0.8 },
                    { WeatherBonusType.NMSpawnRate, 0.9 },
                    { WeatherBonusType.StealRate, 1.5 }, // 霧の中で盗み
                    { WeatherBonusType.CatchRate, 1.0 },
                    { WeatherBonusType.FishingRate, 0.9 },
                    { WeatherBonusType.MiningRate, 1.0 },
                    { WeatherBonusType.StaminaRegen, 1.0 }
                }
            },
            // Blizzard (吹雪) - extreme
            {
                ExtendedWeatherType.Blizzard, new Dictionary<WeatherBonusType, double>
                {
                    { WeatherBonusType.DropRate, 1.8 },
                    { WeatherBonusType.RareDropRate, 2.5 },
                    { WeatherBonusType.ExpRate, 1.5 },
                    { WeatherBonusType.BossSpawnRate, 2.0 },
                    { WeatherBonusType.NMSpawnRate, 2.5 },
                    { WeatherBonusType.StealRate, 0.5 },
                    { WeatherBonusType.CatchRate, 1.5 },
                    { WeatherBonusType.FishingRate, 0.3 },
                    { WeatherBonusType.MiningRate, 1.5 },
                    { WeatherBonusType.StaminaRegen, 0.5 }
                }
            },
            // Thunder (雷雨) - electric monsters
            {
                ExtendedWeatherType.Thunder, new Dictionary<WeatherBonusType, double>
                {
                    { WeatherBonusType.DropRate, 1.4 },
                    { WeatherBonusType.RareDropRate, 1.8 },
                    { WeatherBonusType.ExpRate, 1.2 },
                    { WeatherBonusType.BossSpawnRate, 1.8 },
                    { WeatherBonusType.NMSpawnRate, 2.0 },
                    { WeatherBonusType.StealRate, 0.7 },
                    { WeatherBonusType.CatchRate, 0.6 },
                    { WeatherBonusType.FishingRate, 1.2 },
                    { WeatherBonusType.MiningRate, 0.8 },
                    { WeatherBonusType.StaminaRegen, 0.8 }
                }
            },
            // HeatWave (猛暑) - desert monsters
            {
                ExtendedWeatherType.HeatWave, new Dictionary<WeatherBonusType, double>
                {
                    { WeatherBonusType.DropRate, 1.2 },
                    { WeatherBonusType.RareDropRate, 1.4 },
                    { WeatherBonusType.ExpRate, 1.3 },
                    { WeatherBonusType.BossSpawnRate, 1.3 },
                    { WeatherBonusType.NMSpawnRate, 1.4 },
                    { WeatherBonusType.StealRate, 1.0 },
                    { WeatherBonusType.CatchRate, 0.7 },
                    { WeatherBonusType.FishingRate, 0.5 },
                    { WeatherBonusType.MiningRate, 1.4 },
                    { WeatherBonusType.StaminaRegen, 0.6 }
                }
            },
            // ColdWave (寒波) - ice monsters
            {
                ExtendedWeatherType.ColdWave, new Dictionary<WeatherBonusType, double>
                {
                    { WeatherBonusType.DropRate, 1.3 },
                    { WeatherBonusType.RareDropRate, 1.6 },
                    { WeatherBonusType.ExpRate, 1.2 },
                    { WeatherBonusType.BossSpawnRate, 1.4 },
                    { WeatherBonusType.NMSpawnRate, 1.5 },
                    { WeatherBonusType.StealRate, 1.0 },
                    { WeatherBonusType.CatchRate, 1.4 },
                    { WeatherBonusType.FishingRate, 1.3 },
                    { WeatherBonusType.MiningRate, 1.3 },
                    { WeatherBonusType.StaminaRegen, 0.7 }
                }
            }
        };

        // Monster type weather preferences (spawn rate modifiers)
        private static readonly Dictionary<string, List<ExtendedWeatherType>> MonsterWeatherPreferences = new()
        {
            { "Fire", new List<ExtendedWeatherType> { ExtendedWeatherType.HeatWave, ExtendedWeatherType.Clear } },
            { "Ice", new List<ExtendedWeatherType> { ExtendedWeatherType.ColdWave, ExtendedWeatherType.Snow, ExtendedWeatherType.Blizzard } },
            { "Water", new List<ExtendedWeatherType> { ExtendedWeatherType.Rain, ExtendedWeatherType.Storm } },
            { "Electric", new List<ExtendedWeatherType> { ExtendedWeatherType.Thunder, ExtendedWeatherType.Storm } },
            { "Normal", new List<ExtendedWeatherType> { ExtendedWeatherType.Clear, ExtendedWeatherType.Cloudy } },
            { "Poison", new List<ExtendedWeatherType> { ExtendedWeatherType.Fog, ExtendedWeatherType.Cloudy } },
            { "Ghost", new List<ExtendedWeatherType> { ExtendedWeatherType.Fog, ExtendedWeatherType.Storm } },
            { "Dragon", new List<ExtendedWeatherType> { ExtendedWeatherType.Storm, ExtendedWeatherType.Thunder, ExtendedWeatherType.Blizzard } }
        };

        public WeatherEffectService(TimeWeatherService timeWeatherService)
        {
            _timeWeatherService = timeWeatherService;
        }

        /// <summary>
        /// Get current weather as extended type
        /// </summary>
        public ExtendedWeatherType GetCurrentExtendedWeather()
        {
            // Map existing weather to extended
            var currentWeather = _timeWeatherService.Weather;
            if (currentWeather == WeatherType.Clear) return ExtendedWeatherType.Clear;
            if (currentWeather == WeatherType.Cloudy) return ExtendedWeatherType.Cloudy;
            if (currentWeather == WeatherType.Rain) return ExtendedWeatherType.Rain;
            if (currentWeather == WeatherType.Storm) return ExtendedWeatherType.Storm;
            return ExtendedWeatherType.Clear;
        }

        /// <summary>
        /// Get weather bonus multiplier for given bonus type
        /// </summary>
        public double GetWeatherBonus(WeatherBonusType bonusType)
        {
            var weather = GetCurrentExtendedWeather();
            if (WeatherBonuses.TryGetValue(weather, out var bonuses))
            {
                if (bonuses.TryGetValue(bonusType, out var bonus))
                {
                    return bonus;
                }
            }
            return 1.0;
        }

        /// <summary>
        /// Get all weather bonuses as a dictionary
        /// </summary>
        public Dictionary<WeatherBonusType, double> GetAllWeatherBonuses()
        {
            var weather = GetCurrentExtendedWeather();
            if (WeatherBonuses.TryGetValue(weather, out var bonuses))
            {
                return new Dictionary<WeatherBonusType, double>(bonuses);
            }
            return new Dictionary<WeatherBonusType, double>();
        }

        /// <summary>
        /// Check if NM/Boss spawns are boosted
        /// </summary>
        public bool IsBossSpawnBoosted()
        {
            var bossRate = GetWeatherBonus(WeatherBonusType.BossSpawnRate);
            return bossRate > 1.5;
        }

        /// <summary>
        /// Check if rare drops are boosted
        /// </summary>
        public bool IsRareDropBoosted()
        {
            var rareRate = GetWeatherBonus(WeatherBonusType.RareDropRate);
            return rareRate > 1.5;
        }

        /// <summary>
        /// Calculate drop count with weather bonus
        /// </summary>
        public int CalculateDropCount(int baseCount)
        {
            var dropRate = GetWeatherBonus(WeatherBonusType.DropRate);
            return (int)Math.Ceiling(baseCount * dropRate);
        }

        /// <summary>
        /// Calculate rare drop chance with weather bonus
        /// </summary>
        public bool RollForRareDrop(double baseChance)
        {
            var rareRate = GetWeatherBonus(WeatherBonusType.RareDropRate);
            var adjustedChance = baseChance * rareRate;
            return _random.NextDouble() < Math.Min(adjustedChance, 1.0);
        }

        /// <summary>
        /// Check if monster type is more common in current weather
        /// </summary>
        public double GetMonsterTypeSpawnModifier(string monsterElement)
        {
            var currentWeather = GetCurrentExtendedWeather();
            
            if (MonsterWeatherPreferences.TryGetValue(monsterElement, out var preferredWeathers))
            {
                if (preferredWeathers.Contains(currentWeather))
                {
                    return 2.0; // Double spawn rate
                }
            }
            return 1.0;
        }

        /// <summary>
        /// Get weather display name
        /// </summary>
        public string GetWeatherDisplayName()
        {
            var weather = GetCurrentExtendedWeather();
            return weather switch
            {
                ExtendedWeatherType.Clear => "☀️ 晴れ",
                ExtendedWeatherType.Cloudy => "☁️ 曇り",
                ExtendedWeatherType.Rain => "🌧️ 雨",
                ExtendedWeatherType.Storm => "⛈️ 嵐",
                ExtendedWeatherType.Snow => "❄️ 雪",
                ExtendedWeatherType.Fog => "🌫️ 霧",
                ExtendedWeatherType.Blizzard => "🌨️ 吹雪",
                ExtendedWeatherType.Thunder => "⚡ 雷雨",
                ExtendedWeatherType.HeatWave => "🔥 猛暑",
                ExtendedWeatherType.ColdWave => "🥶 寒波",
                _ => "不明"
            };
        }

        /// <summary>
        /// Get weather description
        /// </summary>
        public string GetWeatherDescription()
        {
            var bonuses = GetAllWeatherBonuses();
            
            var descriptions = new List<string>();
            
            if (bonuses.TryGetValue(WeatherBonusType.DropRate, out var drop) && drop > 1.2)
                descriptions.Add("ドロップ率UP");
            if (bonuses.TryGetValue(WeatherBonusType.RareDropRate, out var rare) && rare > 1.3)
                descriptions.Add("レアドロップ率UP");
            if (bonuses.TryGetValue(WeatherBonusType.ExpRate, out var exp) && exp > 1.1)
                descriptions.Add("経験値UP");
            if (bonuses.TryGetValue(WeatherBonusType.NMSpawnRate, out var nm) && nm > 1.5)
                descriptions.Add("NM出現率UP");
            if (bonuses.TryGetValue(WeatherBonusType.BossSpawnRate, out var boss) && boss > 1.3)
                descriptions.Add("ボス出現率UP");
            if (bonuses.TryGetValue(WeatherBonusType.FishingRate, out var fish) && fish > 1.2)
                descriptions.Add("釣り効率UP");
            if (bonuses.TryGetValue(WeatherBonusType.StaminaRegen, out var stamina) && stamina < 1.0)
                descriptions.Add("スタミナ消費UP");
                
            return descriptions.Count > 0 
                ? string.Join(" / ", descriptions) 
                : "特に効果なし";
        }

        /// <summary>
        /// Check if current weather is dangerous (reduces stamina regen)
        /// </summary>
        public bool IsDangerousWeather()
        {
            var weather = GetCurrentExtendedWeather();
            return weather is ExtendedWeatherType.Storm 
                or ExtendedWeatherType.Blizzard 
                or ExtendedWeatherType.Thunder;
        }

        /// <summary>
        /// Get current weather time remaining (approximation based on cycle)
        /// </summary>
        public double GetWeatherDurationHours()
        {
            // Weather changes approximately every 6 game hours
            return 6.0;
        }
    }
}
