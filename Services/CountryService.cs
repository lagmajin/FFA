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
                BonusType = "attack"
            },
            new Models.Country 
            { 
                Id = 2, 
                Name = "氷之国", 
                Description = "雪山之国。防御力が上昇し、寒さに対処できる。",
                BonusStat = 5,
                BonusType = "defense"
            },
            new Models.Country 
            { 
                Id = 3, 
                Name = "緑之国", 
                Description = "森林之国。自然治癒能力が上がり、HP上限が上昇する。",
                BonusStat = 20,
                BonusType = "hp"
            },
            new Models.Country 
            { 
                Id = 4, 
                Name = "雷之国", 
                Description = "雷鳴が鳴り響く国。素早さが上がり、命中率が増加する。",
                BonusStat = 3,
                BonusType = "speed"
            },
            new Models.Country 
            { 
                Id = 5, 
                Name = "中立之城", 
                Description = "どの国にも属さない中立の都市。バランス型。",
                BonusStat = 2,
                BonusType = "all"
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
