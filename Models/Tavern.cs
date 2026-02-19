namespace FFA.Models;

public enum NPCType
{
    Informant,  // 情報提供者
    QuestGiver, // クエスト提供者
    Merchant,  // 商人
    Trainer,    // トレーナー
    Storyteller, // 語り部
    Gambler    // 博打
}

public enum InteractionResult
{
    Success,
    Failed,
    Locked,
    Cooldown
}

public class NPC
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string JapaneseName { get; set; } = "";
    public NPCType Type { get; set; }
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "👤";
    public int MinLevel { get; set; } = 1;
    public string RequiredJob { get; set; } = "";
    public bool IsAvailable { get; set; } = true;
}

public class NPCInteraction
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public int NpcId { get; set; }
    public string InteractionType { get; set; } = ""; // talk, quest, gamble, buy
    public DateTime LastInteractionUtc { get; set; }
    public int TimesInteracted { get; set; }
}

public class NPCDialogue
{
    public int Id { get; set; }
    public string JapaneseText { get; set; } = "";
    public string EnglishText { get; set; } = "";
    public string Trigger { get; set; } = ""; // quest_complete, level_up, etc.
}

public class Bounty
{
    public int Id { get; set; }
    public string TargetName { get; set; } = "";
    public string Description { get; set; } = "";
    public int RewardGil { get; set; }
    public int RewardExp { get; set; }
    public int RequiredLevel { get; set; }
    public bool IsCompleted { get; set; }
    public string CompletedBy { get; set; } = "";
    public DateTime PostedUtc { get; set; }
}

public class Rumor
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    public string JapaneseContent { get; set; } = "";
    public string Category { get; set; } = ""; // monster, location, item, event
    public int Reliability { get; set; } = 50; // 0-100
    public DateTime PostedUtc { get; set; }
    public bool IsVerified { get; set; }
}
