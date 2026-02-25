using Tomlyn;
using Tomlyn.Syntax;
using Tomlyn.Model;
using FFA.Models;

namespace FFA.Services;

public class UniqueJobService
{
    private List<JobInfo>? _cachedUniqueJobs;
    private readonly string _tomlPath;

    public UniqueJobService()
    {
        // TOMLファイルのパスを設定
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        _tomlPath = Path.Combine(basePath, "Data", "Jobs", "unique_jobs.toml");
        
        // 開発時はプロジェクトルートからも読み込み
        if (!File.Exists(_tomlPath))
        {
            _tomlPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Jobs", "unique_jobs.toml");
        }
    }

    // Unique職リスト取得
    public List<JobInfo> GetAllUniqueJobs()
    {
        if (_cachedUniqueJobs != null)
            return _cachedUniqueJobs;

        try
        {
            if (File.Exists(_tomlPath))
            {
                var tomlContent = File.ReadAllText(_tomlPath);
                var tomlModel = Toml.Parse(tomlContent);
                var tomlTable = tomlModel.ToModel();
                _cachedUniqueJobs = ParseUniqueJobs(tomlTable);
                return _cachedUniqueJobs;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load unique_jobs.toml: {ex.Message}");
        }

        // フォールバック: 空リスト 반환
        _cachedUniqueJobs = new List<JobInfo>();
        return _cachedUniqueJobs;
    }

    private List<JobInfo> ParseUniqueJobs(TomlTable root)
    {
        var jobs = new List<JobInfo>();

        if (!root.ContainsKey("unique_jobs"))
            return jobs;

        var jobsArray = root["unique_jobs"] as TomlArray;
        if (jobsArray == null)
            return jobs;

        foreach (var item in jobsArray)
        {
            if (item is not TomlTable jobTable)
                continue;

            var jobInfo = ParseJobInfo(jobTable);
            if (jobInfo != null)
            {
                jobs.Add(jobInfo);
            }
        }

        return jobs;
    }

    private JobInfo? ParseJobInfo(TomlTable table)
    {
        try
        {
            var jobName = GetStringValue(table, "job", "");
            if (!Enum.TryParse<Job>(jobName, out var job))
            {
                Console.WriteLine($"Unknown job: {jobName}");
                return null;
            }

            var jobInfo = new JobInfo
            {
                Job = job,
                Name = GetStringValue(table, "name", ""),
                Description = GetStringValue(table, "description", ""),
                Icon = GetStringValue(table, "icon", ""),
                Color = GetStringValue(table, "color", "#808080"),
                Role = GetStringValue(table, "role", ""),
                WeaponType = GetStringValue(table, "weapon_type", ""),
                BonusStatus = new PlayerStatus
                {
                    Str = GetIntValue(table, "bonus_str", 0),
                    Agi = GetIntValue(table, "bonus_agi", 0),
                    Int = GetIntValue(table, "bonus_int", 0),
                    Vit = GetIntValue(table, "bonus_vit", 0),
                    Dex = GetIntValue(table, "bonus_dex", 0),
                    Luk = GetIntValue(table, "bonus_luk", 0)
                },
                Skills = GetStringList(table, "skills"),
                PassiveSkills = GetStringList(table, "passive_skills"),
                IsAdvanced = GetBoolValue(table, "is_advanced", false)
            };

            // 必要Jobの解析
            var requiredJobName = GetStringValue(table, "required_job", "");
            if (!string.IsNullOrEmpty(requiredJobName) && Enum.TryParse<Job>(requiredJobName, out var requiredJob))
            {
                jobInfo.RequiredJob = requiredJob;
            }
            jobInfo.RequiredJobLevel = GetIntValue(table, "required_job_level", 0);

            // NonCombatPassiveSkillsの解析
            jobInfo.NonCombatPassiveSkills = ParseNonCombatPassives(table);

            return jobInfo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing job info: {ex.Message}");
            return null;
        }
    }

    private int GetIntValue(TomlTable table, string key, int defaultValue)
    {
        if (table.TryGetValue(key, out var value))
        {
            if (value is long l) return (int)l;
            if (value is int i) return i;
        }
        return defaultValue;
    }

    private string GetStringValue(TomlTable table, string key, string defaultValue)
    {
        if (table.TryGetValue(key, out var value) && value is string s)
            return s;
        return defaultValue;
    }

    private bool GetBoolValue(TomlTable table, string key, bool defaultValue)
    {
        if (table.TryGetValue(key, out var value) && value is bool b)
            return b;
        return defaultValue;
    }

    private List<string> GetStringList(TomlTable table, string key)
    {
        var result = new List<string>();
        if (table.TryGetValue(key, out var value) && value is TomlArray arr)
        {
            foreach (var item in arr)
            {
                if (item is string s)
                    result.Add(s);
            }
        }
        return result;
    }

    private List<NonCombatPassiveSkill> ParseNonCombatPassives(TomlTable table)
    {
        var passives = new List<NonCombatPassiveSkill>();
        if (!table.ContainsKey("noncombat_passives"))
            return passives;

        if (table.TryGetValue("noncombat_passives", out var value) && value is TomlArray arr)
        {
            foreach (var item in arr)
            {
                if (item is string str)
                {
                    var parts = str.Split(':');
                    if (parts.Length == 2 && Enum.TryParse<NonCombatPassiveType>(parts[0], out var passiveType))
                    {
                        if (int.TryParse(parts[1], out var passiveValue))
                        {
                            passives.Add(NonCombatPassiveSkillHelper.CreateSkill(passiveType, passiveValue));
                        }
                    }
                }
            }
        }

        return passives;
    }

    // キャッシュクリア（テスト用）
    public void ClearCache()
    {
        _cachedUniqueJobs = null;
    }
}
