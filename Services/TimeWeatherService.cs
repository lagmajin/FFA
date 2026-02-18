using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFA.Services
{
    public enum DayPhase { Dawn, Day, Dusk, Night }
    public enum WeatherType { Clear, Cloudy, Rain, Storm }

    public class TimeWeatherService
    {
        private readonly object _lock = new();
        private double _timeOfDay; // 0..24
        private WeatherType _weather;

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
                if (t >= 8 && t < 18) return DayPhase.Day;
                if (t >= 18 && t < 20) return DayPhase.Dusk;
                return DayPhase.Night;
            }
        }

        // Advance time by deltaHours and maybe change weather
        public void Advance(double deltaHours)
        {
            lock (_lock)
            {
                _timeOfDay += deltaHours;
                if (_timeOfDay >= 24) _timeOfDay -= 24;
                // simple weather change rule: small random chance to change
                var rnd = new Random();
                var chance = rnd.NextDouble();
                if (chance < 0.02)
                {
                    // change weather randomly
                    var vals = Enum.GetValues<WeatherType>();
                    Weather = vals[rnd.Next(vals.Length)];
                }
            }
        }
    }
}
