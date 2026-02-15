namespace FFA.Models;

public class Ability
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Cost { get; set; } = 0; // mana or resource cost
    public int CooldownSeconds { get; set; } = 0;
    // Simple effect: additive gil change or experience change etc. For extension use EffectType/Value
    public string EffectType { get; set; } = "None";
    public int EffectValue { get; set; } = 0;
}
