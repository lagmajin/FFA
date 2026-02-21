using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using FFA.Models;

namespace FFA.Services
{
    public class TavernService
    {
        private readonly string _databasePath;
        //private readonly int RumorRefreshHours = 6;
        
        public TavernService()
        {
            var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            Directory.CreateDirectory(appDataPath);
            _databasePath = Path.Combine(appDataPath, "tavern.db");
        }

        public List<NPC> GetAvailableNPCs(string username)
        {
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null) return new List<NPC>();

            var npcs = GetAllNPCs();
            return npcs.Where(n => n.MinLevel <= user.Level && n.IsAvailable).ToList();
        }

        public List<NPC> GetAllNPCs()
        {
            // 固定NPCリスト
            return new List<NPC>
            {
                new NPC { Id = 1, Name = "Elder Marcus", JapaneseName = "賢者マルクス", Type = NPCType.Informant, Description = "街の歴史に詳しい老人", Icon = "👴", MinLevel = 1 },
                new NPC { Id = 2, Name = "Innkeeper Rose", JapaneseName = "宿屋のローズ", Type = NPCType.Merchant, Description = "宿屋を経営する噂の女性", Icon = "👩", MinLevel = 1 },
                new NPC { Id = 3, Name = "Hunter Jack", JapaneseName = "猟師ジャック", Type = NPCType.Trainer, Description = "獲物の追い方を教える熟練の猟師", Icon = "🎯", MinLevel = 5 },
                new NPC { Id = 4, Name = "Bard Melody", JapaneseName = "吟遊詩人メロディ", Type = NPCType.Storyteller, Description = "各地の興味深い情報を歌う", Icon = "🎵", MinLevel = 1 },
                new NPC { Id = 5, Name = "Mysterious Stranger", JapaneseName = "謎の男", Type = NPCType.QuestGiver, Description = "裏通りの謎の存在", Icon = "🕵️", MinLevel = 10 },
                new NPC { Id = 6, Name = "Gambler Lou", JapaneseName = "博打のルウ", Type = NPCType.Gambler, Description = "小さな博打を持ちかける男", Icon = "🎲", MinLevel = 15 },
                new NPC { Id = 7, Name = "Expedition Leader", JapaneseName = "冒険者リーダー", Type = NPCType.QuestGiver, Description = "危険な区域的情報を提供", Icon = "🗺️", MinLevel = 20 },
                new NPC { Id = 8, Name = "Alchemist Fiona", JapaneseName = "錬金術師フィオナ", Type = NPCType.Merchant, Description = "珍しい材料を売る錬金術師", Icon = "⚗️", MinLevel = 8 },
                new NPC { Id = 9, Name = "Master Tengu", JapaneseName = "天狗師匠", Type = NPCType.Trainer, Description = "忍者に技を教える老師匠", Icon = "👺", MinLevel = 25, RequiredJob = "Ninja" },
                new NPC { Id = 10, Name = "Guild Representative", JapaneseName = "ギルド代表者", Type = NPCType.QuestGiver, Description = "ギルドの任務的信息提供者", Icon = "⚜️", MinLevel = 1 }
            };
        }

        public NPC? GetNPCById(int id)
        {
            return GetAllNPCs().FirstOrDefault(n => n.Id == id);
        }

        public string TalkToNPC(string username, int npcId)
        {
            var npc = GetNPCById(npcId);
            if (npc == null) return "そのNPCはいません";

            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null) return "ユーザーが見つかりません";

            // {Cooldown チェック
            var interaction = GetOrCreateInteraction(username, npcId);
            var cooldownMinutes = GetNPCInteractionCooldown(npc.Type);
            
            if ((DateTime.UtcNow - interaction.LastInteractionUtc).TotalMinutes < cooldownMinutes)
            {
                int remaining = cooldownMinutes - (int)(DateTime.UtcNow - interaction.LastInteractionUtc).TotalMinutes;
                return $"まだお話しできません。{remaining}分お待ちください。";
            }

            // 対話更新
            interaction.LastInteractionUtc = DateTime.UtcNow;
            interaction.TimesInteracted++;
            SaveInteraction(interaction);

            // NPC别応答
            return GenerateNPCResponse(npc, user);
        }

        private string GenerateNPCResponse(NPC npc, User user)
        {
            var random = new Random();
            string[] responses;

            switch (npc.Type)
            {
                case NPCType.Informant:
                    responses = new[] {
                        $"冒険者さん、最近街近くの森で大規模なGoblinの行動があるそうだ",
                        $"北の山奥に賢者の隠し_libraryがあるらしい",
                        $"この周辺の海には昔、Dragonが住んでいたそうな"
                    };
                    break;
                case NPCType.Merchant:
                    responses = new[] {
                        $"いらっしゃい。何か必要なものはありますか？",
                        $"最近、品薄れて困っています",
                        $"特別サービスして差し上げましょうか"
                    };
                    break;
                case NPCType.Trainer:
                    responses = new[] {
                        $"獲物の追跡法を教えるよ",
                        $"野生の勘が重要だ",
                        $"实战で覚えていくのが一番だ"
                    };
                    break;
                case NPCType.Storyteller:
                    responses = new[] {
                        "♪〜暗い森で光る目〜♪",
                        $"最新の Rumor: {GetRandomRumor().JapaneseContent}",
                        "良い曲子 известен 各方面から"
                    };
                    break;
                case NPCType.QuestGiver:
                    responses = new[] {
                        $"冒険者さん、お願いがあるんだ",
                        $"この任務できるかね？",
                        $"報酬は RIDICULOUS だから"
                    };
                    break;
                case NPCType.Gambler:
                    if (user.Gil < 100)
                    {
                        return "金がなければ kedepできないよ";
                    }
                    return "小小的博打どうですか？当たれば2倍になりますよ";
                default:
                    responses = new[] { "有何贵干？" };
                    break;
            }

            return responses[random.Next(responses.Length)];
        }

        public Rumor GetRandomRumor()
        {
            var rumors = GetAllRumors();
            var random = new Random();
            return rumors[random.Next(rumors.Count)];
        }

        public List<Rumor> GetAllRumors()
        {
            return new List<Rumor>
            {
                new Rumor { Id = 1, Content = "北の山に巨大的Snow Caterpillarが出現", JapaneseContent = "北の雪山に巨大な雪虫が出た", Category = "monster", Reliability = 70 },
                new Rumor { Id = 2, Content = "海底の洞窟で溺れた宝", JapaneseContent = "海底の洞窟に溺れた寶がある", Category = "item", Reliability = 50 },
                new Rumor { Id = 3, Content = "森の奧に隠された祠がある", JapaneseContent = "森の奥に隠された祠がある", Category = "location", Reliability = 80 },
                new Rumor { Id = 4, Content = "砂漠の Oasisで神々の会议", JapaneseContent = "砂漠のオアシスで神々の会議が行われる", Category = "event", Reliability = 40 },
                new Rumor { Id = 5, Content = "古城の地下にDragonの卵", JapaneseContent = "古城の地下に龍の卵がある", Category = "monster", Reliability = 60 },
                new Rumor { Id = 6, Content = "町外の沼地にMagic Itemが埋在", JapaneseContent = "町外の沼地に魔法のアイテムが埋まっている", Category = "item", Reliability = 55 },
                new Rumor { Id = 7, Content = "毎晩幽霊城が出る", JapaneseContent = "毎晩幽霊城が出る", Category = "location", Reliability = 75 },
                new Rumor { Id = 8, Content = "強力なBossがRocky Mountainに覚醒", JapaneseContent = "強いBossが岩山に覚醒した", Category = "monster", Reliability = 85 }
            };
        }

        public List<Bounty> GetActiveBounties()
        {
            using var db = new LiteDatabase(_databasePath);
            var bounties = db.GetCollection<Bounty>("bounties");
            return bounties.Find(b => !b.IsCompleted).ToList();
        }

        public bool AcceptBounty(string username, int bountyId)
        {
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null) return false;

            using var db = new LiteDatabase(_databasePath);
            var bounties = db.GetCollection<Bounty>("bounties");
            var bounty = bounties.FindById(bountyId);
            
            if (bounty == null || bounty.IsCompleted) return false;
            if (user.Level < bounty.RequiredLevel) return false;
            if (user.Gil < 100) return false; // 报名费

            user.Gil -= 100;
            userService.UpdateUser(user);
            
            return true;
        }

        public (bool success, string message) CompleteBounty(string username, string targetName)
        {
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null) return (false, "ユーザーが見つかりません");

            // 報酬計算
            int baseGilReward = 500 + (user.Level * 50);
            int baseExpReward = 100 + (user.Level * 20);
            
            user.Gil += baseGilReward;
            userService.AddExpAndHandleLevel(username, baseExpReward);
            userService.UpdateUser(user);

            return (true, $"任務完了！報酬: {baseGilReward}Gil, {baseExpReward}経験値");
        }

        public int Gamble(string username, int betAmount)
        {
            var userService = new UserService();
            var user = userService.GetByUsername(username);
            if (user == null) return 0;

            if (user.Gil < betAmount) return -1;

            var random = new Random();
            bool win = random.Next(100) < 45; // 45% chance to win

            if (win)
            {
                user.Gil += betAmount;
                userService.UpdateUser(user);
                return betAmount; // WIN! gets double (bet + win)
            }
            else
            {
                user.Gil -= betAmount;
                userService.UpdateUser(user);
                return 0; // LOSE!
            }
        }

        private NPCInteraction GetOrCreateInteraction(string username, int npcId)
        {
            using var db = new LiteDatabase(_databasePath);
            var interactions = db.GetCollection<NPCInteraction>("interactions");
            var interaction = interactions.FindOne(i => i.Username == username && i.NpcId == npcId);
            
            if (interaction == null)
            {
                interaction = new NPCInteraction
                {
                    Username = username,
                    NpcId = npcId,
                    LastInteractionUtc = DateTime.MinValue,
                    TimesInteracted = 0
                };
                interactions.Insert(interaction);
            }
            
            return interaction;
        }

        private void SaveInteraction(NPCInteraction interaction)
        {
            using var db = new LiteDatabase(_databasePath);
            var interactions = db.GetCollection<NPCInteraction>("interactions");
            interactions.Update(interaction);
        }

        private int GetNPCInteractionCooldown(NPCType type) => type switch
        {
            NPCType.Informant => 30,
            NPCType.Merchant => 10,
            NPCType.Trainer => 60,
            NPCType.Storyteller => 20,
            NPCType.QuestGiver => 5,
            NPCType.Gambler => 5,
            _ => 30
        };

        public int GetFameLevel(string username)
        {
            using var db = new LiteDatabase(_databasePath);
            var interactions = db.GetCollection<NPCInteraction>("interactions");
            var total = interactions.Find(i => i.Username == username).Sum(i => i.TimesInteracted);
            return Math.Min(10, total / 10);
        }
    }
}
