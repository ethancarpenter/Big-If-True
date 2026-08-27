using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public enum CreateQuestConnectionOutcome
{
    Success,
    NotFound,
    Duplicate,
    Cycle,
}

public record CreateQuestConnectionResult(CreateQuestConnectionOutcome Outcome, QuestConnectionDto? Connection);
