using FFA.Models;
using System.Collections.Generic;

namespace FFA.Services;

/// <summary>
/// 職業管理サービス
/// </summary>
public class JobService
{
    private readonly UserService _userService;

    public JobService()
    {
        _userService = new UserService();
    }

    /// <summary>
    /// 全職業の詳細情報を取得
    /// </summary>
    /// <returns>職業詳細情報のリスト</returns>
    public List<JobInfo> GetAllJobs()
    {
        try
        {
            return JobDatabase.GetAllJobs();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobService.GetAllJobs 例外: {ex.Message} - {ex.StackTrace}");
            return new List<JobInfo>();
        }
    }

    /// <summary>
    /// 職業に対応する詳細情報を取得
    /// </summary>
    /// <param name="job">職業</param>
    /// <returns>職業詳細情報</returns>
    public JobInfo GetJobInfo(Job job)
    {
        try
        {
            return JobDatabase.GetJobInfo(job);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobService.GetJobInfo 例外: {ex.Message} - {ex.StackTrace}");
            return JobDatabase.GetJobInfo(Job.Warrior);
        }
    }

    /// <summary>
    /// ユーザーの現在の職業詳細情報を取得
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <returns>職業詳細情報</returns>
    public JobInfo GetUserJobInfo(string username)
    {
        try
        {
            var user = _userService.GetByUsername(username);
            if (user == null)
                return JobDatabase.GetJobInfo(Job.Warrior);

            return JobDatabase.GetJobInfo(user.Job);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobService.GetUserJobInfo 例外: {ex.Message} - {ex.StackTrace}");
            return JobDatabase.GetJobInfo(Job.Warrior);
        }
    }

    /// <summary>
    /// 職業転換可能か判定
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <param name="cost">転職費用</param>
    /// <returns>転職可能ならtrue</returns>
    public bool CanChangeJob(string username, int cost)
    {
        try
        {
            var user = _userService.GetByUsername(username);
            if (user == null)
                return false;

            return user.Gil >= cost;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobService.CanChangeJob 例外: {ex.Message} - {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// 職業転換の実行
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <param name="newJob">新しい職業</param>
    /// <param name="cost">転職費用</param>
    /// <returns>転職成功ならtrue</returns>
    public bool ChangeJob(string username, Job newJob, int cost)
    {
        try
        {
            if (!CanChangeJob(username, cost))
                return false;

            return _userService.ChangeJob(username, newJob, cost);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobService.ChangeJob 例外: {ex.Message} - {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// 職業固有のスキルをユーザーに付与
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <param name="job">職業</param>
    public void AssignJobSkills(string username, Job job)
    {
        try
        {
            var user = _userService.GetByUsername(username);
            if (user == null)
                return;

            var jobInfo = JobDatabase.GetJobInfo(job);
            var abilityService = new AbilityService();

            // 職業固有のアビリティを付与
            abilityService.AssignStarterAbilities(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobService.AssignJobSkills 例外: {ex.Message} - {ex.StackTrace}");
        }
    }

    /// <summary>
    /// 職業に基づいたステータスボーナスを計算
    /// </summary>
    /// <param name="baseStatus">基本ステータス</param>
    /// <param name="job">職業</param>
    /// <returns>ステータスボーナスを含む新しいステータス</returns>
    public PlayerStatus CalculateJobBonusStatus(PlayerStatus baseStatus, Job job)
    {
        try
        {
            var jobInfo = JobDatabase.GetJobInfo(job);
            var bonusStatus = new PlayerStatus
            {
                Str = baseStatus.Str + jobInfo.BonusStatus.Str,
                Dex = baseStatus.Dex + jobInfo.BonusStatus.Dex,
                Int = baseStatus.Int + jobInfo.BonusStatus.Int,
                Vit = baseStatus.Vit + jobInfo.BonusStatus.Vit,
                PointsAvailable = baseStatus.PointsAvailable
            };

            return bonusStatus;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobService.CalculateJobBonusStatus 例外: {ex.Message} - {ex.StackTrace}");
            return baseStatus;
        }
    }

    /// <summary>
    /// 職業のロールに基づいた推奨装備タイプを取得
    /// </summary>
    /// <param name="job">職業</param>
    /// <returns>推奨装備タイプ</returns>
    public string GetRecommendedEquipmentType(Job job)
    {
        try
        {
            var jobInfo = JobDatabase.GetJobInfo(job);
            return jobInfo.WeaponType;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobService.GetRecommendedEquipmentType 例外: {ex.Message} - {ex.StackTrace}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 職業のロールを取得
    /// </summary>
    /// <param name="job">職業</param>
    /// <returns>ロール（Tank, Healer, DPSなど）</returns>
    public string GetJobRole(Job job)
    {
        try
        {
            var jobInfo = JobDatabase.GetJobInfo(job);
            return jobInfo.Role;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobService.GetJobRole 例外: {ex.Message} - {ex.StackTrace}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 職業の配色を取得
    /// </summary>
    /// <param name="job">職業</param>
    /// <returns>配色のHEXコード</returns>
    public string GetJobColor(Job job)
    {
        try
        {
            var jobInfo = JobDatabase.GetJobInfo(job);
            return jobInfo.Color;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobService.GetJobColor 例外: {ex.Message} - {ex.StackTrace}");
            return "#000000";
        }
    }

    /// <summary>
    /// 職業のアイコンを取得
    /// </summary>
    /// <param name="job">職業</param>
    /// <returns>アイコンのUnicode</returns>
    public string GetJobIcon(Job job)
    {
        try
        {
            var jobInfo = JobDatabase.GetJobInfo(job);
            return jobInfo.Icon;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobService.GetJobIcon 例外: {ex.Message} - {ex.StackTrace}");
            return "⚔️";
        }
    }

    /// <summary>
    /// 職業の説明文を取得
    /// </summary>
    /// <param name="job">職業</param>
    /// <returns>説明文</returns>
    public string GetJobDescription(Job job)
    {
        try
        {
            var jobInfo = JobDatabase.GetJobInfo(job);
            return jobInfo.Description;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobService.GetJobDescription 例外: {ex.Message} - {ex.StackTrace}");
            return string.Empty;
        }
    }
}