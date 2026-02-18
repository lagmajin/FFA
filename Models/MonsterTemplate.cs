namespace FFA.Models;

public class MonsterTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int BaseHP { get; set; }
    public int BaseAttack { get; set; }
    public int BaseDefense { get; set; }
    public int BaseExp { get; set; }
    public int BaseGil { get; set; }
    public string DropItem { get; set; } = "";
    public int DropRate { get; set; } = 20;
}
