using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFA.Models;

namespace FFA.Services;

public class RankingService
{
    private readonly string _databasePath;
    private readonly UserService _userService;

    public RankingService()
    {
        var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        Directory.CreateDirectory(appDataPath);
        _databasePath = Path.Combine(appDataPath, "ranking.db");
        _userService = new UserService();
    }

    // ランキングを更新する
    public async Task UpdateRankingAsync()
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var rankingEntries = db.GetCollection<RankingEntry>("rankingentries");

            // すべてのユーザーの情報を取得
            var users = _userService.GetAllUsers().ToList();

            // ランキングエントリーを更新
            foreach (var user in users)
            {
                var entry = rankingEntries.FindOne(e => e.Username == user.Username);
                if (entry == null)
                {
                    entry = new RankingEntry
                    {
                        Username = user.Username,
                        Gil = user.Gil,
                        Level = user.Level,
                        LastUpdated = DateTime.Now
                    };
                    rankingEntries.Insert(entry);
                }
                else
                {
                    entry.Gil = user.Gil;
                    entry.Level = user.Level;
                    entry.LastUpdated = DateTime.Now;
                    rankingEntries.Update(entry);
                }
            }

            // ランキングをソートして上位10位を保持
            var sortedEntries = rankingEntries.FindAll()
                .OrderByDescending(e => e.Gil)
                .ThenByDescending(e => e.Level)
                .Take(10)
                .ToList();

            // 上位10位以外を削除
            var top10Usernames = sortedEntries.Select(e => e.Username).ToList();
            var entriesToDelete = rankingEntries.Find(e => !top10Usernames.Contains(e.Username));
            foreach (var entry in entriesToDelete)
            {
                rankingEntries.Delete(entry.Id);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RankingService.UpdateRankingAsync 例外: {ex.Message} - {ex.StackTrace}");
        }
    }

    // 現在のランキングを取得
    public IEnumerable<RankingEntry> GetCurrentRanking()
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var rankingEntries = db.GetCollection<RankingEntry>("rankingentries");
            return rankingEntries.FindAll()
                .OrderByDescending(e => e.Gil)
                .ThenByDescending(e => e.Level)
                .Take(10)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RankingService.GetCurrentRanking 例外: {ex.Message} - {ex.StackTrace}");
            return new List<RankingEntry>();
        }
    }

    // 特定のユーザーのランキングを取得
    public RankingEntry? GetUserRanking(string username)
    {
        try
        {
            using var db = new LiteDatabase(_databasePath);
            var rankingEntries = db.GetCollection<RankingEntry>("rankingentries");
            return rankingEntries.FindOne(e => e.Username == username);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RankingService.GetUserRanking 例外: {ex.Message} - {ex.StackTrace}");
            return null;
        }
    }
}