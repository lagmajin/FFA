namespace FFA.Models;

public class AdminLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Action { get; set; } = "";
    public string Detail { get; set; } = "";
}
