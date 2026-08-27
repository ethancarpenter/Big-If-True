using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public enum CreateQuestNpcOutcome
{
    Success,

    /// <summary>The quest doesn't exist/isn't owned by the current user, or the NPC doesn't exist/isn't in the same campaign as the quest.</summary>
    NotFound,

    /// <summary>A relationship between this Quest and NPC already exists.</summary>
    Duplicate,
}

public record CreateQuestNpcResult(CreateQuestNpcOutcome Outcome, QuestNpcDto? Relationship);
