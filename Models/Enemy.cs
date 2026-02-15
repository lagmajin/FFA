namespace FFA.Models;

public class Enemy
{
    public string Name { get; set; } = "";
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Exp { get; set; }
    public int Gil { get; set; }
    public string DropItem { get; set; } = "";
    public int DropRate { get; set; } = 30; // %
}
