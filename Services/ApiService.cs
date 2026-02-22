using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FFA.Models;

namespace FFA.Services
{
    /// <summary>
    /// APIサービス layer - 将来DBアクセス用のabstract layer
    /// 現時点ではserviceをwrap，将来はWeb APIやDB clientに切り替え可能
    /// </summary>
    public class ApiService
    {
        private readonly Dictionary<string, object> _services = new();
        
        public ApiService()
        {
            // Service registration will be done via DI in the future
        }
        
        /// <summary>
        /// サービスを取得（遅延解決）
        /// </summary>
        public T GetService<T>() where T : class
        {
            var key = typeof(T).FullName ?? typeof(T).Name;
            if (_services.TryGetValue(key, out var service))
            {
                return (T)service;
            }
            
            // For now, services are registered manually
            throw new InvalidOperationException($"Service {key} not registered");
        }
        
        /// <summary>
        /// サービスを登録
        /// </summary>
        public void RegisterService<T>(T service) where T : class
        {
            var key = typeof(T).FullName ?? typeof(T).Name;
            _services[key] = service;
        }
        
        /// <summary>
        /// TimeWeather情報の非同期取得
        /// </summary>
        public async Task<ApiResponse<TimeWeatherInfo>> GetTimeWeatherInfoAsync()
        {
            try
            {
                // 遅延解決のための寺寺寺
                var twService = GetTimeWeatherService();
                
                var info = new TimeWeatherInfo
                {
                    TimeOfDay = twService.TimeOfDay,
                    Phase = twService.Phase.ToString(),
                    Weather = twService.Weather.ToString(),
                    IsNight = twService.IsNight,
                    PhaseDescription = twService.GetPhaseDescription(),
                    Modifiers = twService.GetModifiers()
                };
                
                return await Task.FromResult(ApiResponse<TimeWeatherInfo>.Ok(info));
            }
            catch (Exception ex)
            {
                return ApiResponse<TimeWeatherInfo>.Fail(ex.Message, ApiErrorCodes.InternalError);
            }
        }
        
        /// <summary>
        /// 敵の夜間buff込みのステータスを計算
        /// </summary>
        public async Task<ApiResponse<EnemyStats>> CalculateEnemyStatsAsync(EnemyStats baseStats)
        {
            try
            {
                var twService = GetTimeWeatherService();
                var modifiers = twService.GetModifiers();
                
                var adjustedStats = new EnemyStats
                {
                    Name = baseStats.Name,
                    HP = (int)(baseStats.HP * modifiers.MonsterHPMultiplier),
                    MaxHP = (int)(baseStats.MaxHP * modifiers.MonsterHPMultiplier),
                    Attack = (int)(baseStats.Attack * modifiers.MonsterAttackMultiplier),
                    Defense = (int)(baseStats.Defense * modifiers.MonsterDefenseMultiplier),
                    Exp = (int)(baseStats.Exp * modifiers.MonsterExpMultiplier),
                    DropRateMultiplier = modifiers.MonsterDropRateMultiplier
                };
                
                return await Task.FromResult(ApiResponse<EnemyStats>.Ok(adjustedStats));
            }
            catch (Exception ex)
            {
                return ApiResponse<EnemyStats>.Fail(ex.Message, ApiErrorCodes.InternalError);
            }
        }
        
        private TimeWeatherService GetTimeWeatherService()
        {
            var key = typeof(TimeWeatherService).FullName ?? nameof(TimeWeatherService);
            if (_services.TryGetValue(key, out var service))
            {
                return (TimeWeatherService)service;
            }
            
            // Return a default instance if not registered
            return new TimeWeatherService();
        }
    }
    
    /// <summary>
    /// 時間・天候情報DTO
    /// </summary>
    public class TimeWeatherInfo
    {
        public double TimeOfDay { get; set; }
        public string Phase { get; set; } = "";
        public string Weather { get; set; } = "";
        public bool IsNight { get; set; }
        public string PhaseDescription { get; set; } = "";
        public TimeOfDayModifiers? Modifiers { get; set; }
    }
    
    /// <summary>
    /// 敵ステータスDTO
    /// </summary>
    public class EnemyStats
    {
        public string Name { get; set; } = "";
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Exp { get; set; }
        public double DropRateMultiplier { get; set; } = 1.0;
    }
}
