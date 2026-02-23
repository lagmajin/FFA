using FFA.Models;
using System.Collections.Generic;

namespace FFA.Services;

/// <summary>
/// 職業管理サービス
/// </summary>
public class JobService
{
    private readonly UserService _userService;
    private readonly WorldService _worldService;

    public JobService()
    {
        _userService = new UserService();
        _worldService = new WorldService();
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
    /// <param name="targetJob">転職先の職業（オプション）</param>
    /// <returns>転職可能ならtrue</returns>
    public bool CanChangeJob(string username, int cost, Job? targetJob = null)
    {
        try
        {
            var user = _userService.GetByUsername(username);
            if (user == null)
                return false;

            // ギルチェック
            if (user.Gil < cost)
                return false;

            // 上級職の条件チェック
            if (targetJob.HasValue)
            {
                var jobInfo = JobDatabase.GetJobInfo(targetJob.Value);
                if (jobInfo.IsAdvanced)
                {
                    // 前提職業チェック
                    if (jobInfo.RequiredJob.HasValue && user.Job != jobInfo.RequiredJob.Value)
                        return false;

                    // 必要レベルチェック
                    if (user.Level < jobInfo.RequiredJobLevel)
                        return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobService.CanChangeJob 例外: {ex.Message} - {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// 転職条件の詳細を取得
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <param name="targetJob">転職先の職業</param>
    /// <returns>転職条件の結果</returns>
    public JobChangeRequirementResult CheckJobChangeRequirements(string username, Job targetJob)
    {
        var result = new JobChangeRequirementResult { Job = targetJob };
        
        try
        {
            var user = _userService.GetByUsername(username);
            if (user == null)
            {
                result.CanChange = false;
                result.FailureReasons.Add("ユーザーが見つかりません");
                return result;
            }

            var jobInfo = JobDatabase.GetJobInfo(targetJob);
            result.JobInfo = jobInfo;

            // ギルチェック
            result.RequiredGil = 500; // 基本転職費用
            result.HasEnoughGil = user.Gil >= result.RequiredGil;
            if (!result.HasEnoughGil)
                result.FailureReasons.Add($"ギルが不足しています（必要: {result.RequiredGil}G、所持: {user.Gil}G）");

            // 上級職の条件チェック
            if (jobInfo.IsAdvanced)
            {
                result.IsAdvancedJob = true;

                // 前提職業チェック
                if (jobInfo.RequiredJob.HasValue)
                {
                    result.RequiredJob = jobInfo.RequiredJob.Value;
                    result.RequiredJobName = JobDatabase.GetJobInfo(jobInfo.RequiredJob.Value).Name;
                    result.HasRequiredJob = user.Job == jobInfo.RequiredJob.Value;
                    if (!result.HasRequiredJob)
                        result.FailureReasons.Add($"前提職業「{result.RequiredJobName}」が必要です（現在: {JobDatabase.GetJobInfo(user.Job).Name}）");
                }

                // 必要レベルチェック
                result.RequiredLevel = jobInfo.RequiredJobLevel;
                result.HasRequiredLevel = user.Level >= jobInfo.RequiredJobLevel;
                if (!result.HasRequiredLevel)
                    result.FailureReasons.Add($"レベル{result.RequiredLevel}以上が必要です（現在: Lv.{user.Level}）");
            }

            // 場所制限チェック
            if (jobInfo.IsLocationRestricted && jobInfo.RequiredLocations.Any())
            {
                result.IsLocationRestricted = true;
                result.RequiredLocations = jobInfo.RequiredLocations;
                result.LocationRestrictionMessage = jobInfo.LocationRestrictionMessage;
                
                // 現在の場所を取得
                result.CurrentLocation = _worldService.GetCurrentLocationName(username);
                result.HasRequiredLocation = jobInfo.RequiredLocations.Contains(result.CurrentLocation);
                
                if (!result.HasRequiredLocation)
                {
                    var locationsStr = string.Join("、", jobInfo.RequiredLocations);
                    result.FailureReasons.Add($"特定の場所でのみ転職できます（必要場所: {locationsStr}、現在: {result.CurrentLocation}）");
                }
            }

            result.CanChange = result.FailureReasons.Count == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JobService.CheckJobChangeRequirements 例外: {ex.Message} - {ex.StackTrace}");
            result.CanChange = false;
            result.FailureReasons.Add("エラーが発生しました");
        }

        return result;
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

/// <summary>
/// 転職条件チェック結果
/// </summary>
public class JobChangeRequirementResult
{
    public Job Job { get; set; }
    public JobInfo? JobInfo { get; set; }
    public bool CanChange { get; set; }
    public List<string> FailureReasons { get; set; } = new();
    
    // ギル条件
    public int RequiredGil { get; set; }
    public bool HasEnoughGil { get; set; }
    
    // 上級職条件
    public bool IsAdvancedJob { get; set; }
    public Job? RequiredJob { get; set; }
    public string? RequiredJobName { get; set; }
    public bool HasRequiredJob { get; set; }
    public int RequiredLevel { get; set; }
    public bool HasRequiredLevel { get; set; }
    
    // 場所制限
    public bool IsLocationRestricted { get; set; }
    public List<string> RequiredLocations { get; set; } = new();
    public string? CurrentLocation { get; set; }
    public bool HasRequiredLocation { get; set; }
    public string? LocationRestrictionMessage { get; set; }
}