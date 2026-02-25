using FFA.Models;
using LiteDB;
using System.Security.Cryptography;

namespace FFA.Services;

/// <summary>
/// 街のNPCイベントサービス
/// </summary>
public class TownEventService
{
    private readonly string _databasePath;
    private readonly List<TownEvent> _events;
    private readonly Random _random = new();
    
    public TownEventService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "users.db");
        _events = InitializeEvents();
    }
    
    /// <summary>
    /// イベントデータを初期化
    /// </summary>
    private List<TownEvent> InitializeEvents()
    {
        return new List<TownEvent>
        {
            // === ポジティブイベント ===
            new TownEvent
            {
                Type = TownEventType.Beggar,
                Title = "物乞い",
                Description = "ボロボロの服を着た老人が道端に座っています。",
                NpcName = "物乞いの老人",
                NpcDialogue = "すみません...少しの恵みをいただけませんか...",
                Icon = "🧓",
                Weight = 2.0,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "100G恵んであげる",
                        Description = "老人に100ゴールドを恵む",
                        ResultType = TownEventResultType.Karma,
                        ResultValue = 5,
                        GoldRequirement = 100,
                        SuccessMessage = "老人は感謝の涙を流しました。「ありがとうございます...あなたにはきっと良いことがありますように」"
                    },
                    new TownEventChoice
                    {
                        Text = "500G恵んであげる",
                        Description = "老人に500ゴールドを恵む",
                        ResultType = TownEventResultType.Karma,
                        ResultValue = 15,
                        GoldRequirement = 500,
                        SuccessMessage = "老人は深く頭を下げました。「なんと慈悲深いお方...神のご加護がありますように」"
                    },
                    new TownEventChoice
                    {
                        Text = "無視して立ち去る",
                        Description = "老人を無視する",
                        ResultType = TownEventResultType.Karma,
                        ResultValue = -2,
                        SuccessMessage = "老人は悲しげな目であなたを見送りました..."
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.LostChild,
                Title = "迷子の子ども",
                Description = "泣いている子どもが道端に座り込んでいます。",
                NpcName = "迷子の子ども",
                NpcDialogue = "うぅ...ママ...どこ...?",
                Icon = "👦",
                Weight = 1.5,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "親を探してあげる",
                        Description = "子どもの親を一緒に探す",
                        ResultType = TownEventResultType.Experience,
                        ResultValue = 50,
                        SuccessChance = 80,
                        SuccessMessage = "子どもの親を見つけました！「ありがとうございます！この恩は忘れません！」親からお礼を受け取りました。",
                        FailMessage = "親を探しましたが見つかりませんでした...しかし、子どもは笑顔で「ありがとう」と言ってくれました。"
                    },
                    new TownEventChoice
                    {
                        Text = "お菓子をあげる",
                        Description = "持っているお菓子をあげる",
                        ResultType = TownEventResultType.Karma,
                        ResultValue = 3,
                        SuccessMessage = "子どもはお菓子を見て笑顔になりました！「ありがとう！」"
                    },
                    new TownEventChoice
                    {
                        Text = "衛兵に知らせる",
                        Description = "衛兵に子どものことを知らせる",
                        ResultType = TownEventResultType.Reputation,
                        ResultValue = 2,
                        SuccessMessage = "衛兵が子どもを保護してくれました。「市民のために感謝する」"
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.TravelingMerchant,
                Title = "旅の商人",
                Description = "珍しい品物を売っている商人に出会いました。",
                NpcName = "旅の商人",
                NpcDialogue = "いらっしゃい！珍しい品物がございますよ。いかがですか？",
                Icon = "🧳",
                Weight = 1.0,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "神秘の薬を買う（200G）",
                        Description = "効果不明の薬を購入",
                        ResultType = TownEventResultType.Random,
                        ResultValue = 0,
                        GoldRequirement = 200,
                        SuccessChance = 100,
                        SuccessMessage = "薬を購入しました！飲んでみると..."
                    },
                    new TownEventChoice
                    {
                        Text = "幸運のお守りを買う（500G）",
                        Description = "幸運を呼ぶというお守りを購入",
                        ResultType = TownEventResultType.Buff,
                        ResultValue = 10, // ドロップ率+10%
                        GoldRequirement = 500,
                        SuccessMessage = "お守りを手に入れました！何かいいことが起こりそうです。"
                    },
                    new TownEventChoice
                    {
                        Text = "立ち去る",
                        Description = "何も買わずに立ち去る",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessMessage = "商人に別れを告げました。「またのお越しをお待ちしております！」"
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.Bard,
                Title = "吟遊詩人",
                Description = "街角で吟遊詩人が美しい歌を歌っています。",
                NpcName = "吟遊詩人",
                NpcDialogue = "♪〜 旅人よ、私の歌を聞いていきませんか？",
                Icon = "🎸",
                Weight = 1.5,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "歌を聞く（50G）",
                        Description = "歌を聞いて心を癒す",
                        ResultType = TownEventResultType.MP,
                        ResultValue = 20,
                        GoldRequirement = 50,
                        SuccessMessage = "美しい歌に心が癒されました。MPが回復しました！"
                    },
                    new TownEventChoice
                    {
                        Text = "励ましの歌をリクエスト（100G）",
                        Description = "戦いの勇気をくれる歌をリクエスト",
                        ResultType = TownEventResultType.Buff,
                        ResultValue = 5, // 攻撃力+5%
                        GoldRequirement = 100,
                        SuccessMessage = "勇気ある歌に士気が高まりました！"
                    },
                    new TownEventChoice
                    {
                        Text = "チップを渡す（200G）",
                        Description = "素晴らしい歌にチップを渡す",
                        ResultType = TownEventResultType.Karma,
                        ResultValue = 5,
                        GoldRequirement = 200,
                        SuccessMessage = "吟遊詩人は喜んで「ありがとうございます！あなたの旅に幸あれ！」"
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.WiseOldMan,
                Title = "賢者",
                Description = "長い髭を生やした賢者が道端で瞑想しています。",
                NpcName = "賢者",
                NpcDialogue = "若き旅人よ...知恵を授けましょうか？",
                Icon = "🧙",
                Weight = 0.5,
                MinLevel = 10,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "戦いの知恵を授かる",
                        Description = "戦闘に関する知恵を授かる",
                        ResultType = TownEventResultType.Experience,
                        ResultValue = 100,
                        SuccessMessage = "賢者から戦いの極意を学びました。経験値を獲得！"
                    },
                    new TownEventChoice
                    {
                        Text = "人生の知恵を授かる",
                        Description = "人生に関する知恵を授かる",
                        ResultType = TownEventResultType.Karma,
                        ResultValue = 10,
                        SuccessMessage = "賢者の言葉に心が洗われました。業が増えました。"
                    },
                    new TownEventChoice
                    {
                        Text = "立ち去る",
                        Description = "そっとしておく",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessMessage = "賢者は静かに微笑んでいます。"
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.FortuneTeller,
                Title = "占い師",
                Description = "水晶玉を前にした占い師があなたを呼び止めます。",
                NpcName = "占い師",
                NpcDialogue = "あなたの運命が見えます...占ってみませんか？",
                Icon = "🔮",
                Weight = 1.0,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "運勢を占う（100G）",
                        Description = "今日の運勢を占う",
                        ResultType = TownEventResultType.Random,
                        ResultValue = 0,
                        GoldRequirement = 100,
                        SuccessChance = 100,
                        SuccessMessage = "占い師が水晶を覗き込みました。「あなたの運命は...」"
                    },
                    new TownEventChoice
                    {
                        Text = "恋愛運を占う（50G）",
                        Description = "恋愛運を占う",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        GoldRequirement = 50,
                        SuccessMessage = "「素晴らしい恋が待っています...きっと」占い師は謎めいた笑みを浮かべました。"
                    },
                    new TownEventChoice
                    {
                        Text = "立ち去る",
                        Description = "占いを断る",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessMessage = "占い師は残念そうに「またのご縁を」と言いました。"
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.Healer,
                Title = "癒し手",
                Description = "怪我をした人を治療している癒し手がいます。",
                NpcName = "癒し手",
                NpcDialogue = "怪我をしていますね？治療しましょうか？",
                Icon = "💚",
                Weight = 1.5,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "治療してもらう（無料）",
                        Description = "HPを回復する",
                        ResultType = TownEventResultType.HP,
                        ResultValue = 50,
                        SuccessMessage = "癒し手の魔法でHPが回復しました！「お大事に」"
                    },
                    new TownEventChoice
                    {
                        Text = "寄付をする（100G）",
                        Description = "癒し手の活動に寄付する",
                        ResultType = TownEventResultType.Karma,
                        ResultValue = 5,
                        GoldRequirement = 100,
                        SuccessMessage = "癒し手は感謝しました。「あなたの優しさが、誰かを救います」"
                    },
                    new TownEventChoice
                    {
                        Text = "立ち去る",
                        Description = "何もせず立ち去る",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessMessage = "癒し手は微笑んで「お気をつけて」と言いました。"
                    }
                }
            },
            
            // === ニュートラルイベント ===
            new TownEvent
            {
                Type = TownEventType.Gambler,
                Title = "ギャンブラー",
                Description = "サイコロを振っている男があなたを呼び止めます。",
                NpcName = "ギャンブラー",
                NpcDialogue = "おい、ちょっと賭けないか？運試しだ！",
                Icon = "🎲",
                Weight = 1.0,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "100G賭ける",
                        Description = "100ゴールドを賭ける",
                        ResultType = TownEventResultType.Gold,
                        ResultValue = 200, // 勝った場合
                        GoldRequirement = 100,
                        SuccessChance = 45,
                        SuccessMessage = "勝ちました！200ゴールドを獲得！",
                        FailMessage = "負けてしまいました...100ゴールドを失いました。"
                    },
                    new TownEventChoice
                    {
                        Text = "500G賭ける",
                        Description = "500ゴールドを賭ける",
                        ResultType = TownEventResultType.Gold,
                        ResultValue = 1000,
                        GoldRequirement = 500,
                        SuccessChance = 40,
                        SuccessMessage = "大勝利！1000ゴールドを獲得！",
                        FailMessage = "負けてしまいました...500ゴールドを失いました。"
                    },
                    new TownEventChoice
                    {
                        Text = "断る",
                        Description = "賭けを断る",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessMessage = "「ちっ、つまらない奴だ」とギャンブラーは去っていきました。"
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.StrangeMerchant,
                Title = "怪しい商人",
                Description = "怪しいローブを着た商人が路地裏で商売をしています。",
                NpcName = "怪しい商人",
                NpcDialogue = "へへへ...特別な品物がございますよ...",
                Icon = "🎭",
                Weight = 0.8,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "謎の箱を買う（300G）",
                        Description = "中身がわからない箱を購入",
                        ResultType = TownEventResultType.Random,
                        ResultValue = 0,
                        GoldRequirement = 300,
                        SuccessChance = 100,
                        SuccessMessage = "箱を開けると..."
                    },
                    new TownEventChoice
                    {
                        Text = "闇の護符を買う（1000G）",
                        Description = "闇の力が宿るという護符",
                        ResultType = TownEventResultType.Buff,
                        ResultValue = 15,
                        GoldRequirement = 1000,
                        SuccessMessage = "護符を手に入れました。闇の力を感じます..."
                    },
                    new TownEventChoice
                    {
                        Text = "立ち去る",
                        Description = "怪しいので立ち去る",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessMessage = "商人は「機会があればまた...」とつぶやきました。"
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.PotionSeller,
                Title = "薬売り",
                Description = "色とりどりの薬を売っている行商人がいます。",
                NpcName = "薬売り",
                NpcDialogue = "薬はいかがですか？効能は保証しますよ！",
                Icon = "🧪",
                Weight = 1.5,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "回復薬を買う（50G）",
                        Description = "HP回復薬を購入",
                        ResultType = TownEventResultType.HP,
                        ResultValue = 30,
                        GoldRequirement = 50,
                        SuccessMessage = "回復薬を飲みました。HPが回復！"
                    },
                    new TownEventChoice
                    {
                        Text = "マナ薬を買う（50G）",
                        Description = "MP回復薬を購入",
                        ResultType = TownEventResultType.MP,
                        ResultValue = 30,
                        GoldRequirement = 50,
                        SuccessMessage = "マナ薬を飲みました。MPが回復！"
                    },
                    new TownEventChoice
                    {
                        Text = "元気薬を買う（100G）",
                        Description = "スタミナ回復薬を購入",
                        ResultType = TownEventResultType.Stamina,
                        ResultValue = 50,
                        GoldRequirement = 100,
                        SuccessMessage = "元気薬を飲みました。スタミナが回復！"
                    },
                    new TownEventChoice
                    {
                        Text = "立ち去る",
                        Description = "何も買わずに立ち去る",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessMessage = "薬売りは「またどうぞ！」と笑顔で見送りました。"
                    }
                }
            },
            
            // === ネガティブイベント ===
            new TownEvent
            {
                Type = TownEventType.Thief,
                Title = "泥棒",
                Description = "突然、誰かがあなたにぶつかってきました！",
                NpcName = "泥棒",
                NpcDialogue = "へへっ、ありがとうよ！",
                Icon = "🦹",
                Weight = 1.0,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "追いかける",
                        Description = "泥棒を追いかける",
                        ResultType = TownEventResultType.Gold,
                        ResultValue = 100,
                        SuccessChance = 50,
                        SuccessMessage = "泥棒を捕まえました！盗まれた金を取り戻し、さらに追加で100G手に入れました！",
                        FailMessage = "泥棒に逃げられてしまいました...しかし、盗まれた金は戻りませんでした。"
                    },
                    new TownEventChoice
                    {
                        Text = "衛兵に知らせる",
                        Description = "衛兵に泥棒のことを知らせる",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessChance = 70,
                        SuccessMessage = "衛兵が泥棒を捕まえてくれました！盗まれた金を取り戻しました。",
                        FailMessage = "衛兵が探しましたが、泥棒は見つかりませんでした..."
                    },
                    new TownEventChoice
                    {
                        Text = "諦める",
                        Description = "諦めて立ち去る",
                        ResultType = TownEventResultType.Gold,
                        ResultValue = -50,
                        SuccessMessage = "泥棒に50ゴールド盗まれてしまいました..."
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.Pickpocket,
                Title = "すり",
                Description = "人混みの中で誰かがあなたのポケットを探っています。",
                NpcName = "すり",
                NpcDialogue = "...",
                Icon = "🖐️",
                Weight = 1.2,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "取り押さえる",
                        Description = "すりを取り押さえる",
                        ResultType = TownEventResultType.Gold,
                        ResultValue = 50,
                        SuccessChance = 60,
                        SuccessMessage = "すりを取り押さえました！お詫びとして50G受け取りました。",
                        FailMessage = "すりに逃げられてしまいました..."
                    },
                    new TownEventChoice
                    {
                        Text = "見逃す",
                        Description = "見逃してあげる",
                        ResultType = TownEventResultType.Karma,
                        ResultValue = 3,
                        SuccessMessage = "すりは驚いて逃げていきました。あなたの慈悲深さに感謝しているようです。"
                    },
                    new TownEventChoice
                    {
                        Text = "大声を上げる",
                        Description = "周囲に知らせる",
                        ResultType = TownEventResultType.Reputation,
                        ResultValue = 1,
                        SuccessMessage = "すりは逃げていきましたが、あなたの警戒心は評価されました。"
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.ConArtist,
                Title = "詐欺師",
                Description = "怪しげな男があなたに近づいてきます。",
                NpcName = "怪しい男",
                NpcDialogue = "実は今、絶好の投資チャンスがあるんですよ！",
                Icon = "🤥",
                Weight = 0.8,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "投資する（500G）",
                        Description = "500ゴールドを投資する",
                        ResultType = TownEventResultType.Gold,
                        ResultValue = 1500,
                        GoldRequirement = 500,
                        SuccessChance = 20,
                        SuccessMessage = "なんと！投資が大当たりしました！1500Gの配当を受け取りました！",
                        FailMessage = "男は逃げてしまいました...500Gを失いました。"
                    },
                    new TownEventChoice
                    {
                        Text = "断る",
                        Description = "怪しいので断る",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessMessage = "男は「ちっ、慎重な奴だ」と言って去っていきました。"
                    },
                    new TownEventChoice
                    {
                        Text = "衛兵に通報する",
                        Description = "詐欺師を衛兵に知らせる",
                        ResultType = TownEventResultType.Reputation,
                        ResultValue = 3,
                        SuccessChance = 70,
                        SuccessMessage = "衛兵が男を捕まえました！「詐欺師を捕まえる手助けに感謝する」",
                        FailMessage = "男は逃げてしまいましたが、あなたの警戒心は評価されました。"
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.Drunkard,
                Title = "酔っ払い",
                Description = "酔っ払った男がふらついています。",
                NpcName = "酔っ払い",
                NpcDialogue = "おいらは〜最強の冒険者だったんだぞ〜",
                Icon = "🍺",
                Weight = 1.5,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "話を聞く",
                        Description = "酔っ払いの話を聞く",
                        ResultType = TownEventResultType.Experience,
                        ResultValue = 30,
                        SuccessMessage = "酔っ払いの武勇伝を聞きました。意外と参考になる話もあり、経験値を獲得！"
                    },
                    new TownEventChoice
                    {
                        Text = "酒を奢る（50G）",
                        Description = "酒を奢って機嫌を取る",
                        ResultType = TownEventResultType.Experience,
                        ResultValue = 80,
                        GoldRequirement = 50,
                        SuccessMessage = "酔っ払いは喜んで、冒険の秘訣を教えてくれました！経験値獲得！"
                    },
                    new TownEventChoice
                    {
                        Text = "無視して立ち去る",
                        Description = "無視する",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessMessage = "酔っ払いは「けちんぼうめ！」と叫びましたが、気にせず立ち去りました。"
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.Rival,
                Title = "ライバル",
                Description = "あなたのライバルが現れました！",
                NpcName = "ライバル",
                NpcDialogue = "おや、また会いましたね。まだ冒険を続けていたんですか？",
                Icon = "😤",
                Weight = 0.5,
                MinLevel = 20,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "挑発に乗る",
                        Description = "ライバルと競う",
                        ResultType = TownEventResultType.Experience,
                        ResultValue = 100,
                        SuccessChance = 50,
                        SuccessMessage = "ライバルとの勝負に勝ちました！経験値獲得！",
                        FailMessage = "ライバルに負けてしまいました...しかし、良い経験になりました。"
                    },
                    new TownEventChoice
                    {
                        Text = "無視する",
                        Description = "ライバルを無視する",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessMessage = "ライバルは「臆病者め」と笑いましたが、あなたは気にしませんでした。"
                    },
                    new TownEventChoice
                    {
                        Text = "握手を求める",
                        Description = "和解を求める",
                        ResultType = TownEventResultType.Karma,
                        ResultValue = 5,
                        SuccessMessage = "ライバルは驚きましたが、握手に応じてくれました。「...悪くない奴ですね」"
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.Adventurer,
                Title = "旅の冒険者",
                Description = "傷だらけの冒険者が道端で休んでいます。",
                NpcName = "冒険者",
                NpcDialogue = "すみません...回復薬を持っていませんか？",
                Icon = "⚔️",
                Weight = 1.0,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "回復薬を渡す（無料）",
                        Description = "回復薬を分け与える",
                        ResultType = TownEventResultType.Karma,
                        ResultValue = 8,
                        SuccessMessage = "冒険者は感謝しました。「ありがとう！この恩は忘れません！」"
                    },
                    new TownEventChoice
                    {
                        Text = "情報を交換する",
                        Description = "冒険情報を交換する",
                        ResultType = TownEventResultType.Experience,
                        ResultValue = 50,
                        SuccessMessage = "冒険者と情報を交換しました。有益な情報を得て経験値獲得！"
                    },
                    new TownEventChoice
                    {
                        Text = "立ち去る",
                        Description = "何もせず立ち去る",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessMessage = "冒険者は残念そうに見送りました。"
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.Noble,
                Title = "貴族",
                Description = "豪華な服を着た貴族が通りかかります。",
                NpcName = "貴族",
                NpcDialogue = "おや、冒険者か。少し手伝ってくれないか？",
                Icon = "👑",
                Weight = 0.7,
                MinLevel = 15,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "手伝う",
                        Description = "貴族の手伝いをする",
                        ResultType = TownEventResultType.Gold,
                        ResultValue = 200,
                        SuccessMessage = "貴族の手伝いをしました。「ありがとう、これは謝礼だ」200Gを獲得！"
                    },
                    new TownEventChoice
                    {
                        Text = "断る",
                        Description = "断る",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessMessage = "貴族は「つまらない奴だ」と言って去っていきました。"
                    },
                    new TownEventChoice
                    {
                        Text = "高い報酬を要求する",
                        Description = "より多くの報酬を要求",
                        ResultType = TownEventResultType.Gold,
                        ResultValue = 500,
                        SuccessChance = 30,
                        SuccessMessage = "貴族は「交渉上手だ」と笑い、500Gを支払いました！",
                        FailMessage = "貴族は「強欲な奴だ」と怒って去っていきました。"
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.TreasureHunter,
                Title = "宝探しの冒険者",
                Description = "地図を広げている冒険者がいます。",
                NpcName = "宝探しの冒険者",
                NpcDialogue = "この地図の宝の場所がわからないんだ...",
                Icon = "🗺️",
                Weight = 0.6,
                MinLevel = 25,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "一緒に探す",
                        Description = "宝を一緒に探す",
                        ResultType = TownEventResultType.Gold,
                        ResultValue = 300,
                        SuccessChance = 60,
                        SuccessMessage = "宝を見つけました！山分けして300Gを獲得！",
                        FailMessage = "宝は見つかりませんでしたが、良い冒険でした。"
                    },
                    new TownEventChoice
                    {
                        Text = "地図を買う（200G）",
                        Description = "地図を購入する",
                        ResultType = TownEventResultType.Random,
                        ResultValue = 0,
                        GoldRequirement = 200,
                        SuccessChance = 100,
                        SuccessMessage = "地図を手に入れました！"
                    },
                    new TownEventChoice
                    {
                        Text = "立ち去る",
                        Description = "何もせず立ち去る",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessMessage = "冒険者は「チャンスを逃すなんて」と言いました。"
                    }
                }
            },
            new TownEvent
            {
                Type = TownEventType.InformationBroker,
                Title = "情報屋",
                Description = "影の中に潜む情報屋があなたを呼び止めます。",
                NpcName = "情報屋",
                NpcDialogue = "興味深い情報がありますよ...どうです？",
                Icon = "🕵️",
                Weight = 0.8,
                MinLevel = 10,
                Choices = new List<TownEventChoice>
                {
                    new TownEventChoice
                    {
                        Text = "情報を買う（100G）",
                        Description = "有益な情報を購入",
                        ResultType = TownEventResultType.Experience,
                        ResultValue = 80,
                        GoldRequirement = 100,
                        SuccessMessage = "貴重な情報を得ました！経験値獲得！"
                    },
                    new TownEventChoice
                    {
                        Text = "レアモンスター情報を買う（300G）",
                        Description = "レアモンスターの居場所を聞く",
                        ResultType = TownEventResultType.Buff,
                        ResultValue = 20, // ドロップ率+20%
                        GoldRequirement = 300,
                        SuccessMessage = "レアモンスターの居場所を聞きました！幸運が上がった気がします！"
                    },
                    new TownEventChoice
                    {
                        Text = "立ち去る",
                        Description = "何も買わず立ち去る",
                        ResultType = TownEventResultType.Nothing,
                        ResultValue = 0,
                        SuccessMessage = "情報屋は「損をするのはあなたですよ」とつぶやきました。"
                    }
                }
            }
        };
    }
    
    /// <summary>
    /// ランダムイベントを取得
    /// </summary>
    public TownEvent? GetRandomEvent(User user, string? location = null)
    {
        if (user == null) return null;
        
        // レベルと場所でフィルタリング
        var availableEvents = _events.Where(e => 
            user.Level >= e.MinLevel && 
            user.Level <= e.MaxLevel &&
            (e.RequiredLocations == null || e.RequiredLocations.Count == 0 || 
             (location != null && e.RequiredLocations.Contains(location)))
        ).ToList();
        
        if (availableEvents.Count == 0) return null;
        
        // 重み付きランダム選択
        var totalWeight = availableEvents.Sum(e => e.Weight);
        var randomValue = _random.NextDouble() * totalWeight;
        var currentWeight = 0.0;
        
        foreach (var evt in availableEvents)
        {
            currentWeight += evt.Weight;
            if (randomValue <= currentWeight)
            {
                return evt;
            }
        }
        
        return availableEvents[_random.Next(availableEvents.Count)];
    }
    
    /// <summary>
    /// イベント選択肢を実行
    /// </summary>
    public (bool Success, string Message, Dictionary<string, int> Effects) ExecuteChoice(
        TownEvent evt, 
        int choiceIndex, 
        User user,
        NonCombatPassiveService? passiveService = null)
    {
        if (choiceIndex < 0 || choiceIndex >= evt.Choices.Count)
        {
            return (false, "無効な選択肢です。", new Dictionary<string, int>());
        }
        
        var choice = evt.Choices[choiceIndex];
        var effects = new Dictionary<string, int>();
        
        // ゴールド要件チェック
        if (choice.GoldRequirement > 0 && user.Gil < choice.GoldRequirement)
        {
            return (false, $"ゴールドが足りません。（必要: {choice.GoldRequirement}G）", effects);
        }
        
        // 業要件チェック（Karmaプロパティがない場合はスキップ）
        // if (choice.KarmaRequirement > 0 && user.Karma < choice.KarmaRequirement)
        // {
        //     return (false, $"業が足りません。（必要: {choice.KarmaRequirement}）", effects);
        // }
        
        // 成功判定
        var roll = _random.Next(100);
        var success = roll < choice.SuccessChance;
        
        // ゴールド消費
        if (choice.GoldRequirement > 0)
        {
            effects["Gold"] = -choice.GoldRequirement;
        }
        
        // 成功時の効果
        if (success)
        {
            switch (choice.ResultType)
            {
                case TownEventResultType.Gold:
                    effects["Gold"] = (effects.ContainsKey("Gold") ? effects["Gold"] : 0) + choice.ResultValue;
                    break;
                case TownEventResultType.Experience:
                    var exp = choice.ResultValue;
                    if (passiveService != null)
                    {
                        exp = (int)passiveService.ApplyBonus(exp, passiveService.GetExperienceBonus(user));
                    }
                    effects["Experience"] = exp;
                    break;
                case TownEventResultType.Karma:
                    effects["Karma"] = choice.ResultValue;
                    break;
                case TownEventResultType.HP:
                    effects["HP"] = choice.ResultValue;
                    break;
                case TownEventResultType.MP:
                    effects["MP"] = choice.ResultValue;
                    break;
                case TownEventResultType.Stamina:
                    effects["Stamina"] = choice.ResultValue;
                    break;
                case TownEventResultType.Reputation:
                    effects["Reputation"] = choice.ResultValue;
                    break;
                case TownEventResultType.Random:
                    // ランダム効果
                    var randomEffects = new[] { "Gold", "Experience", "HP", "MP", "Karma" };
                    var randomEffect = randomEffects[_random.Next(randomEffects.Length)];
                    var randomValue = _random.Next(-50, 151);
                    if (randomEffect == "HP" || randomEffect == "MP")
                    {
                        randomValue = Math.Abs(randomValue);
                    }
                    effects[randomEffect] = randomValue;
                    break;
            }
            
            return (true, choice.SuccessMessage ?? "成功しました！", effects);
        }
        else
        {
            // 失敗時の効果
            if (choice.ResultType == TownEventResultType.Gold && choice.FailMessage?.Contains("失い") == true)
            {
                // 既にゴールド消費済み
            }
            
            return (false, choice.FailMessage ?? "失敗しました...", effects);
        }
    }
    
    /// <summary>
    /// イベント履歴を保存
    /// </summary>
    public void SaveHistory(TownEventHistory history)
    {
        using var db = new LiteDatabase(_databasePath);
        var col = db.GetCollection<TownEventHistory>("town_event_histories");
        col.Insert(history);
    }
    
    /// <summary>
    /// ユーザーのイベント履歴を取得
    /// </summary>
    public List<TownEventHistory> GetUserHistory(string username, int limit = 10)
    {
        using var db = new LiteDatabase(_databasePath);
        var col = db.GetCollection<TownEventHistory>("town_event_histories");
        return col.Query()
            .Where(h => h.Username == username)
            .OrderByDescending(h => h.Timestamp)
            .Limit(limit)
            .ToList();
    }
    
    /// <summary>
    /// イベント発生判定（確率ベース）
    /// </summary>
    public bool ShouldTriggerEvent(int baseChance = 15)
    {
        return _random.Next(100) < baseChance;
    }
}
