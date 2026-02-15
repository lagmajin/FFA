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
                Name = "炎之国", 
                Description = "火山地帯に位置する国。火属性に強く、攻撃力が上昇する。",
                BonusStat = 5,
                BonusType = "attack",
                CapitalName = "炎の都",
                CapitalDescription = "火山の近くに建てられた熱気のある首都。鍛冶技術が発達している。",
                VillageName = "火の里",
                VillageDescription = "火山の麓にある小さな村。火山の灰で肥沃な土地が広がっている。",
                Towns = new List<Models.Town>
                {
                    new Models.Town
                    {
                        Id = 1,
                        Name = "炎の都",
                        Description = "火山の近くに建てられた熱気のある首都。鍛冶技術が発達している。",
                        Type = "capital",
                        CountryId = 1,
                        CountryName = "炎之国",
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
                        Name = "火の里",
                        Description = "火山の麓にある小さな村。火山の灰で肥沃な土地が広がっている。",
                        Type = "village",
                        CountryId = 1,
                        CountryName = "炎之国",
                        HasSpecialShop = false,
                        Population = 500,
                        Prosperity = 45,
                        Facilities = new List<string> { "農場", "小規模鍛冶屋", "教会" },
                        Events = new List<string> { "収穫祭", "火山の恵み祭" }
                    }
                }
            },
            new Models.Country 
            { 
                Id = 2, 
                Name = "氷之国", 
                Description = "雪山之国。防御力が上昇し、寒さに対処できる。",
                BonusStat = 5,
                BonusType = "defense",
                CapitalName = "氷の都",
                CapitalDescription = "雪山の中に建てられた美しい首都。氷の彫刻が特徴的。",
                VillageName = "雪の里",
                VillageDescription = "雪山の麓にある平和な村。氷漁が盛ん。",
                Towns = new List<Models.Town>
                {
                    new Models.Town
                    {
                        Id = 3,
                        Name = "氷の都",
                        Description = "雪山の中に建てられた美しい首都。氷の彫刻が特徴的。",
                        Type = "capital",
                        CountryId = 2,
                        CountryName = "氷之国",
                        HasSpecialShop = true,
                        SpecialShopType = "防具",
                        Population = 8000,
                        Prosperity = 80,
                        Facilities = new List<string> { "氷の彫刻館", "魔法学院", "氷の城", "スキー場" },
                        Events = new List<string> { "氷の祭り", "スキー大会", "氷の彫刻展" }
                    },
                    new Models.Town
                    {
                        Id = 4,
                        Name = "雪の里",
                        Description = "雪山の麓にある平和な村。氷漁が盛ん。",
                        Type = "village",
                        CountryId = 2,
                        CountryName = "氷之国",
                        HasSpecialShop = false,
                        Population = 400,
                        Prosperity = 40,
                        Facilities = new List<string> { "漁港", "小規模防具店", "神社" },
                        Events = new List<string> { "氷漁祭", "雪合戦大会" }
                    }
                }
            },
            new Models.Country 
            { 
                Id = 3, 
                Name = "緑之国", 
                Description = "森林之国。自然治癒能力が上がり、HP上限が上昇する。",
                BonusStat = 20,
                BonusType = "hp",
                CapitalName = "緑の都",
                CapitalDescription = "巨大な樹木の中に建てられた自然に溶け込む首都。薬草学が発達している。",
                VillageName = "森の里",
                VillageDescription = "森林の奥深くにある村。木々と共生している。",
                Towns = new List<Models.Town>
                {
                    new Models.Town
                    {
                        Id = 5,
                        Name = "緑の都",
                        Description = "巨大な樹木の中に建てられた自然に溶け込む首都。薬草学が発達している。",
                        Type = "capital",
                        CountryId = 3,
                        CountryName = "緑之国",
                        HasSpecialShop = true,
                        SpecialShopType = "消耗品",
                        Population = 7000,
                        Prosperity = 75,
                        Facilities = new List<string> { "薬草園", "魔法大学", "大聖樹の寺", "自然公園" },
                        Events = new List<string> { "花祭り", "薬草収穫祭", "大聖樹の儀式" }
                    },
                    new Models.Town
                    {
                        Id = 6,
                        Name = "森の里",
                        Description = "森林の奥深くにある村。木々と共生している。",
                        Type = "village",
                        CountryId = 3,
                        CountryName = "緑之国",
                        HasSpecialShop = false,
                        Population = 300,
                        Prosperity = 35,
                        Facilities = new List<string> { "木工作坊", "薬草店", "森の神社" },
                        Events = new List<string> { "森の祭り", "木彫り大会" }
                    }
                }
            },
            new Models.Country 
            { 
                Id = 4, 
                Name = "雷之国", 
                Description = "雷鳴が鳴り響く国。素早さが上がり、命中率が増加する。",
                BonusStat = 3,
                BonusType = "speed",
                CapitalName = "雷の都",
                CapitalDescription = "雷雲が常に覆う国の首都。雷のエネルギーを利用した技術が先進的。",
                VillageName = "雷の里",
                VillageDescription = "雷の山脈の中にある村。雷の石が豊富にある。",
                Towns = new List<Models.Town>
                {
                    new Models.Town
                    {
                        Id = 7,
                        Name = "雷の都",
                        Description = "雷雲が常に覆う国の首都。雷のエネルギーを利用した技術が先進的。",
                        Type = "capital",
                        CountryId = 4,
                        CountryName = "雷之国",
                        HasSpecialShop = true,
                        SpecialShopType = "装飾品",
                        Population = 9000,
                        Prosperity = 90,
                        Facilities = new List<string> { "雷の研究所", "魔法工房", "雷の塔", "スピード訓練場" },
                        Events = new List<string> { "雷の祭り", "高速走行大会", "雷の石展示会" }
                    },
                    new Models.Town
                    {
                        Id = 8,
                        Name = "雷の里",
                        Description = "雷の山脈の中にある村。雷の石が豊富にある。",
                        Type = "village",
                        CountryId = 4,
                        CountryName = "雷之国",
                        HasSpecialShop = false,
                        Population = 450,
                        Prosperity = 50,
                        Facilities = new List<string> { "雷の石鉱山", "小規模装飾品店", "雷の神社" },
                        Events = new List<string> { "雷の石祭り", "山登り大会" }
                    }
                }
            },
            new Models.Country 
            { 
                Id = 5, 
                Name = "中立之城", 
                Description = "どの国にも属さない中立の都市。バランス型。",
                BonusStat = 2,
                BonusType = "all",
                CapitalName = "中立の都",
                CapitalDescription = "各国からの商人が集まる大きな都市。多文化が融合している。",
                VillageName = "中立の里",
                VillageDescription = "都市の郊外にある平和な村。農業が盛ん。",
                Towns = new List<Models.Town>
                {
                    new Models.Town
                    {
                        Id = 9,
                        Name = "中立の都",
                        Description = "各国からの商人が集まる大きな都市。多文化が融合している。",
                        Type = "capital",
                        CountryId = 5,
                        CountryName = "中立之城",
                        HasSpecialShop = true,
                        SpecialShopType = "特殊",
                        Population = 15000,
                        Prosperity = 95,
                        Facilities = new List<string> { "大市場", "各国大使館", "図書館", "競技場" },
                        Events = new List<string> { "国際祭り", "商業博覧会", "格闘大会" }
                    },
                    new Models.Town
                    {
                        Id = 10,
                        Name = "中立の里",
                        Description = "都市の郊外にある平和な村。農業が盛ん。",
                        Type = "village",
                        CountryId = 5,
                        CountryName = "中立之城",
                        HasSpecialShop = false,
                        Population = 600,
                        Prosperity = 55,
                        Facilities = new List<string> { "大農場", "市場", "教会" },
                        Events = new List<string> { "農業祭", "秋の収穫祭" }
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
