namespace FFA.Services;

public class TownService
{
    private readonly CountryService _countryService;

    public TownService()
    {
        _countryService = new CountryService();
    }

    // 全ての街を取得
    public List<Models.Town> GetAllTowns()
    {
        var towns = new List<Models.Town>();
        var countries = _countryService.GetAllCountries();

        foreach (var country in countries)
        {
            towns.AddRange(country.Towns);
        }

        return towns;
    }

    // 国IDから街を取得
    public List<Models.Town> GetTownsByCountryId(int countryId)
    {
        var country = _countryService.GetCountryById(countryId);
        return country?.Towns ?? new List<Models.Town>();
    }

    // 街IDから街を取得
    public Models.Town? GetTownById(int townId)
    {
        var towns = GetAllTowns();
        return towns.FirstOrDefault(t => t.Id == townId);
    }

    // 国IDから首都を取得
    public Models.Town? GetCapitalByCountryId(int countryId)
    {
        var towns = GetTownsByCountryId(countryId);
        return towns.FirstOrDefault(t => t.Type == "capital");
    }

    // 国IDから田舎を取得
    public Models.Town? GetVillageByCountryId(int countryId)
    {
        var towns = GetTownsByCountryId(countryId);
        return towns.FirstOrDefault(t => t.Type == "village");
    }

    // 街の種類ごとに街を取得
    public List<Models.Town> GetTownsByType(string type)
    {
        var towns = GetAllTowns();
        return towns.Where(t => t.Type == type).ToList();
    }

    // 特殊商店のある街を取得
    public List<Models.Town> GetTownsWithSpecialShops()
    {
        var towns = GetAllTowns();
        return towns.Where(t => t.HasSpecialShop).ToList();
    }

    // 街の施設を取得
    public List<string> GetTownFacilities(int townId)
    {
        var town = GetTownById(townId);
        return town?.Facilities ?? new List<string>();
    }

    // 街のイベントを取得
    public List<string> GetTownEvents(int townId)
    {
        var town = GetTownById(townId);
        return town?.Events ?? new List<string>();
    }
}