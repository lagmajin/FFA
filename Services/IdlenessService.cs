using System;
using System.Linq;
using FFA.Models;

namespace FFA.Services
{
    // 放置検出・報酬付与の簡易サービス
    public class IdlenessService
    {
        private readonly UserService _userService;
        private readonly TimeSpan _idleThreshold = TimeSpan.FromDays(1); // 放置と見なす閾値

        public IdlenessService()
        {
            _userService = new UserService();
        }

        // ユーザーの放置時間を取得
        public TimeSpan GetIdleDuration(string username)
        {
            var u = _userService.GetByUsername(username);
            if (u == null) return TimeSpan.MaxValue;
            return DateTime.UtcNow - u.LastActiveUtc;
        }

        // 放置ユーザーかどうか
        public bool IsIdle(string username)
        {
            return GetIdleDuration(username) >= _idleThreshold;
        }

        // 放置報酬を付与（安全にUserServiceと連携する）
        public bool GrantIdlenessReward(string username)
        {
            var u = _userService.GetByUsername(username);
            if (u == null) return false;
            var idle = DateTime.UtcNow - u.LastActiveUtc;
            if (idle < _idleThreshold) return false;

            // シンプル: 1日放置ごとに50ギルを付与（上限7日分）
            int days = (int)Math.Min(7, Math.Floor(idle.TotalDays));
            if (days <= 0) return false;

            int reward = 50 * days;
            _userService.AdjustGil(username, reward);

            // 更新 last active を現在にすることで重複付与を防ぐ
            u.LastActiveUtc = DateTime.UtcNow;
            _userService.UpdateUser(u);
            return true;
        }
    }
}
