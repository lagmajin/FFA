namespace FFA.Models;

/// <summary>
/// 素材クラス
/// </summary>
public class Material
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public int Quantity { get; set; }
    public int Price { get; set; }
}
