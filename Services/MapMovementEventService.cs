using System;
using System.Collections.Generic;
using FFA.Models;

namespace FFA.Services
{
    /// <summary>
    /// マップ移動時のランダムイベントサービス
    /// </summary>
    public class MapMovementEventService
    {
        private readonly Random _random = new();
        
        // イベント発生確率（10%）
        private const double EventTriggerRate = 0.10;
        
        // イベントタイプ別の発生確率
        private const double GoodEventRate = 0.35;    // 良いイベント: 35%
        private const double NeutralEventRate = 0.40; // 普通イベント: 40%
        private const double BadEventRate = 0.25;     // 悪いイベント: 25%
        
        /// <summary>
        /// 移動イベントをトリガーするかどうか判定
        /// </summary>
        public bool ShouldTriggerEvent()
        {
            return _random.NextDouble() < EventTriggerRate;
        }
        
        /// <summary>
        /// ランダムイベントを生成
        /// </summary>
        public MapMovementEvent? GenerateEvent()
        {
            var roll = _random.NextDouble();
            
            if (roll < GoodEventRate)
            {
                return GenerateGoodEvent();
            }
            else if (roll < GoodEventRate + NeutralEventRate)
            {
                return GenerateNeutralEvent();
            }
            else
            {
                return GenerateBadEvent();
            }
        }
        
        /// <summary>
        /// 良いイベントを生成
        /// </summary>
        private MapMovementEvent GenerateGoodEvent()
        {
            var events = new List<MapMovementEvent>
            {
                new MapMovementEvent
                {
                    Type = EventType.Good,
                    Title = "💚 発見！",
                    Message = "道端に落ちている寶石を発見した！",
                    GilReward = 50 + _random.Next(100),
                    ExpReward = 10 + _random.Next(20)
                },
                new MapMovementEvent
                {
                    Type = EventType.Good,
                    Title = "🌿 薬草を発見",
                    Message = "珍しい薬草を発見した！HPが回復した！",
                    HpRestore = 20 + _random.Next(30)
                },
                new MapMovementEvent
                {
                    Type = EventType.Good,
                    Title = "✨ 女神の祝福",
                    Message = "女神像の前で祈ると、素晴らしい光があなたを包んだ！",
                    ExpReward = 30 + _random.Next(50),
                    GilReward = 20 + _random.Next(30)
                },
                new MapMovementEvent
                {
                    Type = EventType.Good,
                    Title = "🍀 幸運の象徴",
                    Message = " четыре-leaf-cloverを発見した！今日はついてる！",
                    GilReward = 100 + _random.Next(150)
                },
                new MapMovementEvent
                {
                    Type = EventType.Good,
                    Title = "💰 落とし物",
                    Message = "、誰かが落とした money pouchを発見した！",
                    GilReward = 80 + _random.Next(120)
                },
                new MapMovementEvent
                {
                    Type = EventType.Good,
                    Title = "🎁 親切な商人",
                    Message = "旅商人から美味しいおにぎりをいただいた！",
                    HpRestore = 30 + _random.Next(20)
                },
                new MapMovementEvent
                {
                    Type = EventType.Good,
                    Title = "⭐ 修行の成果",
                    Message = "途中でモンスターと戦い、修行になった！経験値を獲得！",
                    ExpReward = 25 + _random.Next(35)
                }
            };
            
            return events[_random.Next(events.Count)];
        }
        
        /// <summary>
        /// 普通イベントを生成
        /// </summary>
        private MapMovementEvent GenerateNeutralEvent()
        {
            var events = new List<MapMovementEvent>
            {
                new MapMovementEvent
                {
                    Type = EventType.Neutral,
                    Title = "🌤️ 天气の変化",
                    Message = "天气が变了。不过、特に影響はない。"
                },
                new MapMovementEvent
                {
                    Type = EventType.Neutral,
                    Title = "🐦 鳥のさえずり",
                    Message = "美しい鳥のさえずりを聞きながら、旅を続ける。"
                },
                new MapMovementEvent
                {
                    Type = EventType.Neutral,
                    Title = "💭 休息",
                    Message = "疲れたので少し休む。继续。"
                },
                new MapMovementEvent
                {
                    Type = EventType.Neutral,
                    Title = "🗺️ 道に迷う",
                    Message = "少し道に迷ったが、無事に元の道に戻った。"
                },
                new MapMovementEvent
                {
                    Type = EventType.Neutral,
                    Title = "👤 他の旅人",
                    Message = "他の旅人とすれ違った。お互いに挨拶をした。"
                },
                new MapMovementEvent
                {
                    Type = EventType.Neutral,
                    Title = "🏪 休憩所",
                    Message = "小さな休憩所を見つけたが少し休んでいった。"
                }
            };
            
            return events[_random.Next(events.Count)];
        }
        
        /// <summary>
        /// 悪いイベントを生成
        /// </summary>
        private MapMovementEvent GenerateBadEvent()
        {
            var events = new List<MapMovementEvent>
            {
                new MapMovementEvent
                {
                    Type = EventType.Bad,
                    Title = "💀 落とし穴",
                    Message = "落とし穴に落ちてしまった！HPを 잃었다！",
                    HpDamage = 15 + _random.Next(25)
                },
                new MapMovementEvent
                {
                    Type = EventType.Bad,
                    Title = "🐍 毒草",
                    Message = "毒のある草に触れてしまった！",
                    HpDamage = 10 + _random.Next(15)
                },
                new MapMovementEvent
                {
                    Type = EventType.Bad,
                    Title = "👺 ゴブリン",
                    Message = "ゴブリンに襲われた！所持金を奪われた！",
                    GilLoss = 20 + _random.Next(50)
                },
                new MapMovementEvent
                {
                    Type = EventType.Bad,
                    Title = "🌧️ 雨宿り",
                    Message = "突然の雨で避けが遅くなり、濡れてしまった。"
                },
                new MapMovementEvent
                {
                    Type = EventType.Bad,
                    Title = "💨 強風",
                    Message = "強い風に煽られ、バランスが崩れた。"
                },
                new MapMovementEvent
                {
                    Type = EventType.Bad,
                    Title = "🕷️ 蜘蛛",
                    Message = "巨大的蜘蛛の巣に引き込まれ、少し苦しんだ。",
                    HpDamage = 8 + _random.Next(12)
                },
                new MapMovementEvent
                {
                    Type = EventType.Bad,
                    Title = "🥔 道端の石",
                    Message = "石に足を引っかけて転んでしまった！",
                    HpDamage = 5 + _random.Next(10)
                },
                new MapMovementEvent
                {
                    Type = EventType.Bad,
                    Title = "🐺 野良犬",
                    Message = "野良犬に吠えられ、惊慌して逃げ出した！",
                    StaminaDamage = 5 + _random.Next(10)
                }
            };
            
            return events[_random.Next(events.Count)];
        }
    }
    
    /// <summary>
    /// マップ移動イベント
    /// </summary>
    public class MapMovementEvent
    {
        public EventType Type { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        
        // 良いイベントの効果
        public int GilReward { get; set; }
        public int ExpReward { get; set; }
        public int HpRestore { get; set; }
        
        // 悪いイベントの効果
        public int HpDamage { get; set; }
        public int GilLoss { get; set; }
        public int StaminaDamage { get; set; }
    }
    
    /// <summary>
    /// イベントタイプ
    /// </summary>
    public enum EventType
    {
        Good,    // 良いイベント
        Neutral, // 普通イベント
        Bad      // 悪いイベント
    }
}
