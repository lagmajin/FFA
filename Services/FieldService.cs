namespace FFA.Services;

public class FieldService
{
    // 全エリア取得
    public List<Models.Field> GetAllFields()
    {
        return new List<Models.Field>
        {
            new Models.Field 
            { 
                Id = 1, 
                Name = "平地", 
                Description = "穏やかな草原。初心者の冒険者でも安心して雰囲けます。",
                Enemies = new[] { "スライム", "ウサギ", "蝶" },
                Drops = new[] { "草药", "毛皮", "蝶の粉" },
                Difficulty = 1
            },
            new Models.Field 
            { 
                Id = 2, 
                Name = "森", 
                Description = "木々が茂る暗い森。、経験者が雰囲けます。",
                Enemies = new[] { "ゴブリン", "狼", "毒キノコ" },
                Drops = new[] { "木材", "毛皮", "毒キノコ", "短剣" },
                Difficulty = 2
            },
            new Models.Field 
            { 
                Id = 3, 
                Name = "湖", 
                Description = "神秘的な湖。水の者が住んでいます。",
                Enemies = new[] { "水.Elemental", "魚人", "アシガエル" },
                Drops = new[] { "魚の肉", "珍珠", "水のエッセンス" },
                Difficulty = 2
            },
            new Models.Field 
            { 
                Id = 4, 
                Name = "砂漠", 
                Description = "広大な砂漠。厳しい環境が待ち受けています。",
                Enemies = new[] { "サソリ", "トカゲ", "ミイラ" },
                Drops = new[] { "サソリの針", "砂金", "骨", "盾牌" },
                Difficulty = 3
            },
            new Models.Field 
            { 
                Id = 5, 
                Name = "雪山", 
                Description = "冰、雪覆盖的山脉。強い者が雰囲けます。",
                Enemies = new[] { "雪だるま", "冰ゴーレム", "白狼" },
                Drops = new[] { "冰の結晶", "毛皮", "雪の結晶" },
                Difficulty = 4
            },
            new Models.Field 
            { 
                Id = 6, 
                Name = "火山", 
                Description = "灼热的山脉。只有最强者才能来。",
                Enemies = new[] { "溶岩ゴーレム", "火織", "炎 Elemental" },
                Drops = new[] { "溶岩の石", "火のエッセンス", "黒曜石" },
                Difficulty = 5
            },
            new Models.Field 
            { 
                Id = 7, 
                Name = "ダンジョン", 
                Description = "地下的迷宫。充满了强大的怪物。",
                Enemies = new[] { "スケルトン", "リッチ", "オーク" },
                Drops = new[] { "骨", "宝石", "盾牌", "古代の剣" },
                Difficulty = 5
            }
        };
    }

    // エリアIDから取得
    public Models.Field? GetFieldById(int fieldId)
    {
        return GetAllFields().FirstOrDefault(f => f.Id == fieldId);
    }
}
