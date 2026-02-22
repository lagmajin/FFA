using System;
using System.Collections.Generic;
using System.Linq;

namespace FFA.Services
{
    public enum DayPhase { Dawn, Day, Dusk, Night }
    public enum WeatherType { Clear, Cloudy, Rain, Storm, Snow, Fog }

    /// <summary>
    /// 昼夜システムを実装するサービス
    /// 夜はモンスターが強くなり、特定のボスが出現する
    /// </summary>
    public class TimeWeatherService
    {
        private readonly object _lock = new();
        private double _timeOfDay; // 0..24
        private WeatherType _weather;
        private readonly Random _random = new();
        
        // ゲーム内時間の流れる速さ（1現実秒 = 1ゲーム時間分）
        private const double HoursPerSecond = 0.5;

        public TimeWeatherService()
        {
            _timeOfDay = 8.0; // start morning
            _weather = WeatherType.Clear;
        }

        public double TimeOfDay
        {
            get { lock (_lock) { return _timeOfDay; } }
            private set { lock (_lock) { _timeOfDay = value; } }
        }

        public WeatherType Weather
        {
            get { lock (_lock) { return _weather; } }
            private set { lock (_lock) { _weather = value; } }
        }

        public DayPhase Phase
        {
            get
            {
                var t = TimeOfDay;
                if (t >= 5 && t < 8) return DayPhase.Dawn;
                if (t >= 8 && t < 17) return DayPhase.Day;
                if (t >= 17 && t < 20) return DayPhase.Dusk;
                return DayPhase.Night;
            }
        }
        
        /// <summary>
        /// 夜間かどうかを判定
        /// </summary>
        public bool IsNight => Phase == DayPhase.Night || Phase == DayPhase.Dusk;
        
        /// <summary>
        /// 朝的かどうかを判定
        /// </summary>
        public bool IsDawn => Phase == DayPhase.Dawn;

        /// <summary>
        /// 時間帯に応じたマルチプライヤー情報を取得
        /// </summary>
        public TimeOfDayModifiers GetModifiers()
        {
            var phase = Phase;
            var weather = Weather;
            
            return new TimeOfDayModifiers
            {
                // 몬스터 능력치 – 밤에 상승
                MonsterAttackMultiplier = phase switch
                {
                    DayPhase.Night => 1.5,
                    DayPhase.Dusk => 1.25,
                    DayPhase.Dawn => 1.1,
                    _ => 1.0
                },
                MonsterDefenseMultiplier = phase switch
                {
                    DayPhase.Night => 1.3,
                    DayPhase.Dusk => 1.15,
                    _ => 1.0
                },
                MonsterHPMultiplier = phase switch
                {
                    DayPhase.Night => 1.4,
                    DayPhase.Dusk => 1.2,
                    DayPhase.Dawn => 1.1,
                    _ => 1.0
                },
                MonsterExpMultiplier = phase switch
                {
                    DayPhase.Night => 1.5,
                    DayPhase.Dusk => 1.25,
                    DayPhase.Dawn => 1.15,
                    _ => 1.0
                },
                MonsterDropRateMultiplier = phase switch
                {
                    DayPhase.Night => 1.8,
                    DayPhase.Dusk => 1.4,
                    _ => 1.0
                },
                
                // 天候による 영향
                WeatherAttackMultiplier = weather switch
                {
                    WeatherType.Storm => 0.9,
                    WeatherType.Rain => 0.95,
                    WeatherType.Snow => 0.85,
                    WeatherType.Fog => 0.9,
                    _ => 1.0
                },
                
                // プレイヤーへの 영향
                PlayerStealthBonus = phase == DayPhase.Night ? 1.3 : 1.0,
                PlayerEvasionBonus = phase == DayPhase.Night ? 1.2 : 1.0,
                PlayerAccuracyPenalty = phase == DayPhase.Night ? 0.8 : 1.0,
                
                // 可視性（夜は暗い）
                VisibilityRadius = phase switch
                {
                    DayPhase.Night => 0.5,
                    DayPhase.Dusk => 0.7,
                    DayPhase.Dawn => 0.85,
                    _ => 1.0
                },
                
                // 採集/伐採成功率
                GatheringSuccessRate = weather switch
                {
                    WeatherType.Storm => 0.7,
                    WeatherType.Rain => 0.8,
                    WeatherType.Snow => 0.75,
                    _ => 1.0
                },
                
                // 漁獲量
                FishingSuccessRate = weather switch
                {
                    WeatherType.Storm => 0.5,
                    WeatherType.Rain => 0.7,
                    WeatherType.Clear => 1.2,
                    _ => 1.0
                }
            };
        }
        
        /// <summary>
        /// 夜出现的-exclusive monsters
        /// </summary>
        public List<string> GetNightOnlyMonsters()
        {
            return Phase switch
            {
                DayPhase.Night => new List<string>
                {
                    "暗黒狼", "幽霊", "ナイトメア", "バンパイア",
                    "ウサギ（悪魔）", "骨の騎士", "シャドウ", "デーモン"
                },
                DayPhase.Dusk => new List<string>
                {
                    "黄昏の狼", "ゴースト", "ライブラ"
                },
                DayPhase.Dawn => new List<string>
                {
                    "朝霧の妖精", "夜明けの天使"
                },
                _ => new List<string>()
            };
        }
        
        /// <summary>
        /// 現在の時間帯の説明を取得
        /// </summary>
        public string GetPhaseDescription()
        {
            return Phase switch
            {
                DayPhase.Dawn => "夜が明けたばかり。モは弱っている。",
                DayPhase.Day => "日中はモンスターも活動に活発。",
                DayPhase.Dusk => "夕暮れ時。モンスターが少し強くなる。",
                DayPhase.Night => "夜になった。モンスター能力が上昇し、特別な敵が出現！",
                _ => ""
            };
        }
        
        /// <summary>
        /// 時間帯に応じた敵の強さを計算
        /// </summary>
        public int CalculateMonsterStrength(int baseStrength)
        {
            var modifiers = GetModifiers();
            return (int)(baseStrength * modifiers.MonsterAttackMultiplier);
        }
        
        /// <summary>
        /// 時間帯に応じたドロップ率を計算
        /// </summary>
        public double CalculateDropRate(double baseRate)
        {
            var modifiers = GetModifiers();
            return baseRate * modifiers.MonsterDropRateMultiplier;
        }
        
        /// <summary>
        /// 遭遇する特別な敵を確認（夜のみ）
        /// </summary>
        public string? GetSpecialEncounter()
        {
            // 夜のみ特別な敵に出逢う可能性がある
            if (Phase != DayPhase.Night) return null;
            
            var chance = _random.NextDouble();
            
            // 5%の確率で特別な敵に出逢う
            if (chance < 0.05)
            {
                var nightMonsters = GetNightOnlyMonsters();
                return nightMonsters[_random.Next(nightMonsters.Count)];
            }
            
            return null;
        }

        // Advance time by deltaHours and maybe change weather
        public void Advance(double deltaHours)
        {
            lock (_lock)
            {
                _timeOfDay += deltaHours;
                if (_timeOfDay >= 24) _timeOfDay -= 24;
                
                // 天候変化の確率
                UpdateWeather();
            }
        }
        
        private void UpdateWeather()
        {
            // 天候は確率で変化する
            var chance = _random.NextDouble();
            
            // 2%の確率で天候が変わる
            if (chance < 0.02)
            {
                var possibleWeather = GetPossibleWeather();
                Weather = possibleWeather[_random.Next(possibleWeather.Count)];
            }
        }
        
        private List<WeatherType> GetPossibleWeather()
        {
            // 時間帯によって出現し得る天候が変わる
            return Phase switch
            {
                DayPhase.Dawn => new List<WeatherType> { WeatherType.Clear, WeatherType.Cloudy, WeatherType.Fog },
                DayPhase.Day => new List<WeatherType> { WeatherType.Clear, WeatherType.Cloudy, WeatherType.Rain },
                DayPhase.Dusk => new List<WeatherType> { WeatherType.Clear, WeatherType.Cloudy, WeatherType.Fog },
                DayPhase.Night => new List<WeatherType> { WeatherType.Clear, WeatherType.Cloudy, WeatherType.Fog, WeatherType.Storm },
                _ => new List<WeatherType> { WeatherType.Clear }
            };
        }
        
        /// <summary>
        /// 時刻を進める（Tickごとに调用）
        /// </summary>
        public void Tick()
        {
            Advance(HoursPerSecond);
        }
    }
    
    /// <summary>
    /// 時間帯によるマルチプライヤー
    /// </summary>
    public class TimeOfDayModifiers
    {
        // モンスター
        public double MonsterAttackMultiplier { get; set; } = 1.0;
        public double MonsterDefenseMultiplier { get; set; } = 1.0;
        public double MonsterHPMultiplier { get; set; } = 1.0;
        public double MonsterExpMultiplier { get; set; } = 1.0;
        public double MonsterDropRateMultiplier { get; set; } = 1.0;
        
        // 天候の影響
        public double WeatherAttackMultiplier { get; set; } = 1.0;
        
        // プレイヤー
        public double PlayerStealthBonus { get; set; } = 1.0;
        public double PlayerEvasionBonus { get; set; } = 1.0;
        public double PlayerAccuracyPenalty { get; set; } = 1.0;
        
        // 環境
        public double VisibilityRadius { get; set; } = 1.0;
        public double GatheringSuccessRate { get; set; } = 1.0;
        public double FishingSuccessRate { get; set; } = 1.0;
    }
}
