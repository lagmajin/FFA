namespace FFA.Models;

public class Town
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = ""; // "capital" (首都), "village" (田舎), "town" (一般街)
    public int CountryId { get; set; } // 所属する国のID
    public string CountryName { get; set; } = ""; // 所属する国の名前
    public bool HasSpecialShop { get; set; } = false; // 特殊商店の有無
    public string SpecialShopType { get; set; } = ""; // 特殊商店の種類（武器、防具、装飾品、消耗品、特殊）
    public int Population { get; set; } = 0; // 人口
    public int Prosperity { get; set; } = 50; // 繁栄度（0-100）
    public List<string> Facilities { get; set; } = new List<string>(); // 施設一覧
    public List<string> Events { get; set; } = new List<string>(); // イベント一覧
}