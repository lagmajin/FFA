using Microsoft.AspNetCore.Mvc;
using FFA.Services;
using FFA.Models;

namespace FFA.Controllers
{
    /// <summary>
    /// ゲーム状態APIコントローラー
    /// 将来的に外部APIとして公開するためのendpoint
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly TimeWeatherService _timeWeatherService;
        
        public GameController(TimeWeatherService timeWeatherService)
        {
            _timeWeatherService = timeWeatherService;
        }
        
        /// <summary>
        /// 現在の時間・天候情報を取得
        /// GET /api/game/time
        /// </summary>
        [HttpGet("time")]
        public IActionResult GetTimeWeather()
        {
            var info = new TimeWeatherInfo
            {
                TimeOfDay = _timeWeatherService.TimeOfDay,
                Phase = _timeWeatherService.Phase.ToString(),
                Weather = _timeWeatherService.Weather.ToString(),
                IsNight = _timeWeatherService.IsNight,
                PhaseDescription = _timeWeatherService.GetPhaseDescription(),
                Modifiers = _timeWeatherService.GetModifiers()
            };
            
            return Ok(ApiResponse<TimeWeatherInfo>.Ok(info));
        }
        
        /// <summary>
        /// 敵のステータスを夜間buff込みで計算
        /// POST /api/game/enemy/stats
        /// </summary>
        [HttpPost("enemy/stats")]
        public IActionResult CalculateEnemyStats([FromBody] EnemyStats baseStats)
        {
            var modifiers = _timeWeatherService.GetModifiers();
            
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
            
            return Ok(ApiResponse<EnemyStats>.Ok(adjustedStats));
        }
        
        /// <summary>
        /// 夜間の特別敵リストを取得
        /// GET /api/game/enemies/night
        /// </summary>
        [HttpGet("enemies/night")]
        public IActionResult GetNightEnemies()
        {
            var enemies = _timeWeatherService.GetNightOnlyMonsters();
            return Ok(ApiResponse<List<string>>.Ok(enemies));
        }
        
        /// <summary>
        /// 時間帯の説明を取得
        /// GET /api/game/phase/description
        /// </summary>
        [HttpGet("phase/description")]
        public IActionResult GetPhaseDescription()
        {
            var description = _timeWeatherService.GetPhaseDescription();
            return Ok(ApiResponse<string>.Ok(description));
        }
    }
    
    /// <summary>
    /// マップ/エリアAPIコントローラー
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MapController : ControllerBase
    {
        private readonly MapService _mapService;
        
        public MapController(MapService mapService)
        {
            _mapService = mapService;
        }
        
        /// <summary>
        /// 全マップリストを取得
        /// GET /api/map/all
        /// </summary>
        [HttpGet("all")]
        public IActionResult GetAllMaps()
        {
            var maps = _mapService.GetAllMaps().ToList();
            return Ok(ApiResponse<List<Map>>.Ok(maps));
        }
        
        /// <summary>
        /// 特定の国のマップを取得
        /// GET /api/map/country/{countryId}
        /// </summary>
        [HttpGet("country/{countryId}")]
        public IActionResult GetMapByCountry(int countryId)
        {
            var map = _mapService.GetMapByCountryId(countryId);
            if (map == null)
            {
                return NotFound(ApiResponse<object>.Fail("Map not found", ApiErrorCodes.NotFound));
            }
            return Ok(ApiResponse<Map>.Ok(map));
        }
        
        /// <summary>
        /// 中立边境のマップを取得
        /// GET /api/map/neutral
        /// </summary>
        [HttpGet("neutral")]
        public IActionResult GetNeutralMap()
        {
            // 国ID 5 = 中立边境
            var map = _mapService.GetMapByCountryId(5);
            if (map == null)
            {
                return NotFound(ApiResponse<object>.Fail("Neutral map not found", ApiErrorCodes.NotFound));
            }
            return Ok(ApiResponse<Map>.Ok(map));
        }
        
        /// <summary>
        /// ゲート位置から接続を取得
        /// GET /api/map/connection/{countryId}/{x}/{y}
        /// </summary>
        [HttpGet("connection/{countryId}/{x}/{y}")]
        public IActionResult GetConnection(int countryId, int x, int y)
        {
            var connection = _mapService.GetConnection(countryId, x, y);
            return Ok(ApiResponse<MapConnection?>.Ok(connection));
        }
    }
}
