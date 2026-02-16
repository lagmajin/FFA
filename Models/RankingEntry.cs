using LiteDB;
using System;

namespace FFA.Models;

public class RankingEntry
{
    [BsonId]
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Gil { get; set; }
    public int Level { get; set; }
    public DateTime LastUpdated { get; set; }
    // 順位（ランキング表示用）
    public int Rank { get; set; }
}