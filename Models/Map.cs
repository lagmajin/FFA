namespace FFA.Models;

public class MapLocation
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = ""; // "town", "field", "forest", "mountain", "river", "dungeon"
    public bool CanEnter { get; set; } = true;
    public List<string> Events { get; set; } = new List<string>();
}

public class Map
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Width { get; set; } = 10;
    public int Height { get; set; } = 10;
    public List<MapLocation> Locations { get; set; } = new List<MapLocation>();
}