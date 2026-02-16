namespace FFA.Services;

public class CountryService
{
    // 国リスト取得
    public List<Models.Country> GetAllCountries()
    {
        return new List<Models.Country>
        {
            new Models.Country 
            { 
                Id = 1, 
                Name = "Inferno", 
                Description = "火山地帯に位置する国。火属性に強く、攻撃力が上昇する。",
                BonusStat = 5,
                BonusType = "attack",
                CapitalName = "Vulcanis",
                CapitalDescription = "火山の近くに建てられた熱気のある首都。鍛冶技術が発達している。",
                VillageName = "Ashfield",
                VillageDescription = "火山の麓にある小さな村。火山の灰で肥沃な土地が広がっている。",
                Towns = new List<Models.Town>
                {
                    new Models.Town
                    {
                        Id = 1,
                        Name = "Vulcanis",
                        Description = "火山の近くに建てられた熱気のある首都。鍛冶技術が発達している。",
                        Type = "capital",
                        CountryId = 1,
                        CountryName = "Inferno",
                        HasSpecialShop = true,
                        SpecialShopType = "武器",
                        Population = 10000,
                        Prosperity = 85,
                        Facilities = new List<string> { "鍛冶屋", "魔法学校", "戦闘訓練場", "温泉" },
                        Events = new List<string> { "炎の祭り", "火山観光", "鍛冶大会" }
                    },
                    new Models.Town
                    {
                        Id = 2,
                        Name = "Ashfield",
                        Description = "火山の麓にある小さな村。火山の灰で肥沃な土地が広がっている。",
                        Type = "village",
                        CountryId = 1,
                        CountryName = "Inferno",
                        HasSpecialShop = false,
                        Population = 500,
                        Prosperity = 45,
                        Facilities = new List<string> { "農場", "小規模鍛冶屋", "教会" },
                        Events = new List<string> { "収穫祭", "火山の恵み祭" }
                    },
                    new Models.Town
                    {
                        Id = 3,
                        Name = "Pyrrhus",
                        Description = "炎の国を代表する商業都市。様々な武器商人が集まっている。",
                        Type = "town",
                        CountryId = 1,
                        CountryName = "Inferno",
                        HasSpecialShop = true,
                        SpecialShopType = "武器",
                        Population = 5000,
                        Prosperity = 70,
                        Facilities = new List<string> { "武器商人", "旅館", "酒場" },
                        Events = new List<string> { "武器市", "相撲大会" }
                    }
                }
            },
            new Models.Country 
            { 
                Id = 2, 
                Name = "Frostheim", 
                Description = "雪山之国。防御力が上昇し、寒さに対処できる。",
                BonusStat = 5,
                BonusType = "defense",
                CapitalName = "Glaciermount",
                CapitalDescription = "雪山の中に建てられた美しい首都。氷の彫刻が特徴的。",
                VillageName = "Snowpeak",
                VillageDescription = "雪山の麓にある平和な村。氷漁が盛ん。",
                Towns = new List<Models.Town>
                {
                    new Models.Town
                    {
                        Id = 4,
                        Name = "Glaciermount",
                        Description = "雪山の中に建てられた美しい首都。氷の彫刻が特徴的。",
                        Type = "capital",
                        CountryId = 2,
                        CountryName = "Frostheim",
                        HasSpecialShop = true,
                        SpecialShopType = "防具",
                        Population = 8000,
                        Prosperity = 80,
                        Facilities = new List<string> { "氷の彫刻館", "魔法学院", "氷の城", "スキー場" },
                        Events = new List<string> { "氷の祭り", "スキー大会", "氷の彫刻展" }
                    },
                    new Models.Town
                    {
                        Id = 5,
                        Name = "Snowpeak",
                        Description = "雪山の麓にある平和な村。氷漁が盛ん。",
                        Type = "village",
                        CountryId = 2,
                        CountryName = "Frostheim",
                        HasSpecialShop = false,
                        Population = 400,
                        Prosperity = 40,
                        Facilities = new List<string> { "漁港", "小規模防具店", "神社" },
                        Events = new List<string> { "氷漁祭", "雪合戦大会" }
                    },
                    new Models.Town
                    {
                        Id = 6,
                        Name = "Frostport",
                        Description = "寒冷地での交易で栄えた港町。防具の品揃えが豊富。",
                        Type = "town",
                        CountryId = 2,
                        CountryName = "Frostheim",
                        HasSpecialShop = true,
                        SpecialShopType = "防具",
                        Population = 6000,
                        Prosperity = 65,
                        Facilities = new List<string> { "防具商人", "港", "温泉旅館" },
                        Events = new List<string> { "海産物祭", "防具展示会" }
                    }
                }
            },
            new Models.Country 
            { 
                Id = 3, 
                Name = "Verdania", 
                Description = "森林之国。自然治癒能力が上がり、HP上限が上昇する。",
                BonusStat = 20,
                BonusType = "hp",
                CapitalName = "Sylvantis",
                CapitalDescription = "巨大な樹木の中に建てられた自然に溶け込む首都。薬草学が発達している。",
                VillageName = "Timberwood",
                VillageDescription = "森林の奥深くにある村。木々と共生している。",
                Towns = new List<Models.Town>
                {
                    new Models.Town
                    {
                        Id = 7,
                        Name = "Sylvantis",
                        Description = "巨大な樹木の中に建てられた自然に溶け込む首都。薬草学が発達している。",
                        Type = "capital",
                        CountryId = 3,
                        CountryName = "Verdania",
                        HasSpecialShop = true,
                        SpecialShopType = "消耗品",
                        Population = 7000,
                        Prosperity = 75,
                        Facilities = new List<string> { "薬草園", "魔法大学", "大聖樹の寺", "自然公園" },
                        Events = new List<string> { "花祭り", "薬草収穫祭", "大聖樹の儀式" }
                    },
                    new Models.Town
                    {
                        Id = 8,
                        Name = "Timberwood",
                        Description = "森林の奥深くにある村。木々と共生している。",
                        Type = "village",
                        CountryId = 3,
                        CountryName = "Verdania",
                        HasSpecialShop = false,
                        Population = 300,
                        Prosperity = 35,
                        Facilities = new List<string> { "木工作坊", "薬草店", "森の神社" },
                        Events = new List<string> { "森の祭り", "木彫り大会" }
                    },
                    new Models.Town
                    {
                        Id = 9,
                        Name = "Herbhaven",
                        Description = "薬草の集散地として有名な町。珍しい薬草が手に入る。",
                        Type = "town",
                        CountryId = 3,
                        CountryName = "Verdania",
                        HasSpecialShop = true,
                        SpecialShopType = "消耗品",
                        Population = 4000,
                        Prosperity = 60,
                        Facilities = new List<string> { "薬草商人", "研究所", "民宿" },
                        Events = new List<string> { "薬草即売会", "採取大会" }
                    }
                }
            },
            new Models.Country 
            { 
                Id = 4, 
                Name = "Tempestia", 
                Description = "雷鳴が鳴り響く国。素早さが上がり、命中率が増加する。",
                BonusStat = 3,
                BonusType = "speed",
                CapitalName = "Thunderhold",
                CapitalDescription = "雷雲が常に覆う国の首都。雷のエネルギーを利用した技術が先進的。",
                VillageName = "Stormwatch",
                VillageDescription = "雷鳴がよく響く高地の村。雷よけの工芸が有名。",
                Towns = new List<Models.Town>
                {
                    new Models.Town
                    {
                        Id = 10,
                        Name = "Thunderhold",
                        Description = "雷雲が常に覆う国の首都。雷のエネルギーを利用した技術が先進的。",
                        Type = "capital",
                        CountryId = 4,
                        CountryName = "Tempestia",
                        HasSpecialShop = true,
                        SpecialShopType = "装飾品",
                        Population = 9000,
                        Prosperity = 90,
                        Facilities = new List<string> { "雷の研究所", "魔法工房", "雷の塔", "スピード訓練場" },
                        Events = new List<string> { "雷の祭り", "高速走行大会", "雷の石展示会" }
                    },
                    new Models.Town
                    {
                        Id = 11,
                        Name = "Stormwatch",
                        Description = "雷鳴がよく響く高地の村。雷よけの工芸が有名。",
                        Type = "village",
                        CountryId = 4,
                        CountryName = "Tempestia",
                        HasSpecialShop = false,
                        Population = 450,
                        Prosperity = 50,
                        Facilities = new List<string> { "雷の石鉱山", "小規模装飾品店", "雷の神社" },
                        Events = new List<string> { "雷の石祭り", "山登り大会" }
                    },
                    new Models.Town
                    {
                        Id = 12,
                        Name = "Voltcastle",
                        Description = "電気系統の発明で知られた城塞都市。技術の中心地。",
                        Type = "town",
                        CountryId = 4,
                        CountryName = "Tempestia",
                        HasSpecialShop = true,
                        SpecialShopType = "装飾品",
                        Population = 5500,
                        Prosperity = 72,
                        Facilities = new List<string> { "発明工房", "学園", "城砦" },
                        Events = new List<string> { "発明展", "競速大会" }
                    }
                }
            },
            new Models.Country 
            { 
                Id = 5, 
                Name = "Neutral Haven", 
                Description = "どの国にも属さない中立の都市。バランス型。",
                BonusStat = 2,
                BonusType = "all",
                CapitalName = "Concordia",
                CapitalDescription = "各国からの商人が集まる大きな都市。多文化が融合している。",
                VillageName = "Harmonyfield",
                VillageDescription = "都市の郊外にある平和な村。農業が盛ん。",
                Towns = new List<Models.Town>
                {
                    new Models.Town
                    {
                        Id = 13,
                        Name = "Concordia",
                        Description = "各国からの商人が集まる大きな都市。多文化が融合している。",
                        Type = "capital",
                        CountryId = 5,
                        CountryName = "Neutral Haven",
                        HasSpecialShop = true,
                        SpecialShopType = "特殊",
                        Population = 15000,
                        Prosperity = 95,
                        Facilities = new List<string> { "大市場", "各国大使館", "図書館", "競技場" },
                        Events = new List<string> { "国際祭り", "商業博覧会", "格闘大会" }
                    },
                    new Models.Town
                    {
                        Id = 14,
                        Name = "Harmonyfield",
                        Description = "都市の郊外にある平和な村。農業が盛ん。",
                        Type = "village",
                        CountryId = 5,
                        CountryName = "Neutral Haven",
                        HasSpecialShop = false,
                        Population = 600,
                        Prosperity = 55,
                        Facilities = new List<string> { "大農場", "市場", "教会" },
                        Events = new List<string> { "農業祭", "秋の収穫祭" }
                    },
                    new Models.Town
                    {
                        Id = 15,
                        Name = "Tradesquare",
                        Description = "世界中の商人が集まる交易都市。あらゆる商品が手に入る。",
                        Type = "town",
                        CountryId = 5,
                        CountryName = "Neutral Haven",
                        HasSpecialShop = true,
                        SpecialShopType = "特殊",
                        Population = 8000,
                        Prosperity = 85,
                        Facilities = new List<string> { "大商館", "銀行", "ホテル" },
                        Events = new List<string> { "商品市", "商談会" }
                    }
                }
            }
        };
    }

    // 国IDから国情報取得
    public Models.Country? GetCountryById(int countryId)
    {
        return GetAllCountries().FirstOrDefault(c => c.Id == countryId);
    }

    // 国のBonus適用
    public void ApplyCountryBonus(Models.User user)
    {
        if (user.CountryId == null) return;
        
        var country = GetCountryById(user.CountryId.Value);
        if (country == null) return;

        switch (country.BonusType)
        {
            case "attack":
                // 攻撃力は計算時に適用
                break;
            case "defense":
                // 防御力は計算時に適用
                break;
            case "hp":
                user.MaxHP += country.BonusStat;
                user.HP = user.MaxHP;
                break;
            case "all":
                user.MaxHP += 10;
                user.HP = user.MaxHP;
                break;
        }
    }
}
