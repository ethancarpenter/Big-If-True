using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public enum CreateQuestLocationOutcome
{
    Success,

    /// <summary>The quest doesn't exist/isn't owned by the current user, or the Location doesn't exist/isn't in the same campaign as the quest.</summary>
    NotFound,

    /// <summary>A relationship between this Quest and Location already exists.</summary>
    Duplicate,
}

public record CreateQuestLocationResult(CreateQuestLocationOutcome Outcome, QuestLocationDto? Relationship);
