namespace FFA.Models;

public class Field
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string[] Enemies { get; set; } = Array.Empty<string>();
    public string[] Drops { get; set; } = Array.Empty<string>();
    public int Difficulty { get; set; } = 1; // 1-5
}
