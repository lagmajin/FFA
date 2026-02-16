namespace FFA.Models
{
    public class KarmaAdjustRequest
    {
        public string Username { get; set; } = string.Empty;
        public int Delta { get; set; }
    }
}
