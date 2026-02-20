using Tomlyn;
using Tomlyn.Model;

namespace FFA.Services;

public class CountryService
{
    private List<Models.Country>? _cachedCountries;
    private readonly string _tomlPath;

    public CountryService()
    {
        // TOMLファイルのパスを設定
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        _tomlPath = Path.Combine(basePath, "Data", "Towns", "towns.toml");
        
        // 開発時はプロジェクトルートからも読み込み
        if (!File.Exists(_tomlPath))
        {
            _tomlPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Towns", "towns.toml");
        }
    }

    // 国リスト取得
    public List<Models.Country> GetAllCountries()
    {
        if (_cachedCountries != null)
            return _cachedCountries;

        try
        {
            if (File.Exists(_tomlPath))
            {
                var tomlContent = File.ReadAllText(_tomlPath);
                var tomlModel = Toml.Parse(tomlContent);
                var tomlTable = tomlModel.ToModel();
                _cachedCountries = ParseCountries(tomlTable);
                return _cachedCountries;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load towns.toml: {ex.Message}");
        }

        // フォールバック: 空リスト 반환
        _cachedCountries = new List<Models.Country>();
        return _cachedCountries;
    }

    private List<Models.Country> ParseCountries(TomlTable root)
    {
        var countries = new List<Models.Country>();

        if (!root.ContainsKey("countries"))
            return countries;

        var countriesArray = root["countries"] as TomlArray;
        if (countriesArray == null)
            return countries;

        foreach (var countryItem in countriesArray)
        {
            var countryToml = countryItem as TomlTable;
            if (countryToml == null)
                continue;

            var country = new Models.Country
            {
                Id = GetIntValue(countryToml, "id", 0),
                Name = GetStringValue(countryToml, "name", ""),
                Description = GetStringValue(countryToml, "description", ""),
                BonusStat = GetIntValue(countryToml, "bonus_stat", 0),
                BonusType = GetStringValue(countryToml, "bonus_type", ""),
                CapitalName = GetStringValue(countryToml, "capital_name", ""),
                CapitalDescription = GetStringValue(countryToml, "capital_description", ""),
                VillageName = GetStringValue(countryToml, "village_name", ""),
                VillageDescription = GetStringValue(countryToml, "village_description", ""),
                Towns = new List<Models.Town>()
            };

            // 街の解析
            if (countryToml.ContainsKey("towns"))
            {
                var townsArray = countryToml["towns"] as TomlArray;
                if (townsArray != null)
                {
                    foreach (var townItem in townsArray)
                    {
                        var townToml = townItem as TomlTable;
                        if (townToml == null)
                            continue;

                        var town = new Models.Town
                        {
                            Id = GetIntValue(townToml, "id", 0),
                            Name = GetStringValue(townToml, "name", ""),
                            Description = GetStringValue(townToml, "description", ""),
                            Type = GetStringValue(townToml, "type", ""),
                            CountryId = GetIntValue(townToml, "country_id", 0),
                            CountryName = GetStringValue(townToml, "country_name", ""),
                            HasSpecialShop = GetBoolValue(townToml, "has_special_shop", false),
                            SpecialShopType = GetStringValue(townToml, "special_shop_type", ""),
                            Population = GetIntValue(townToml, "population", 0),
                            Prosperity = GetIntValue(townToml, "prosperity", 0),
                            Facilities = GetStringList(townToml, "facilities"),
                            Events = GetStringList(townToml, "events")
                        };
                        country.Towns.Add(town);
                    }
                }
            }

            countries.Add(country);
        }

        return countries;
    }

    private int GetIntValue(TomlTable table, string key, int defaultValue)
    {
        if (table.TryGetValue(key, out var value))
        {
            if (value is long l) return (int)l;
            if (value is int i) return i;
        }
        return defaultValue;
    }

    private string GetStringValue(TomlTable table, string key, string defaultValue)
    {
        if (table.TryGetValue(key, out var value) && value is string s)
            return s;
        return defaultValue;
    }

    private bool GetBoolValue(TomlTable table, string key, bool defaultValue)
    {
        if (table.TryGetValue(key, out var value) && value is bool b)
            return b;
        return defaultValue;
    }

    private List<string> GetStringList(TomlTable table, string key)
    {
        var result = new List<string>();
        if (table.TryGetValue(key, out var value) && value is TomlArray arr)
        {
            foreach (var item in arr)
            {
                if (item is string s)
                    result.Add(s);
            }
        }
        return result;
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
