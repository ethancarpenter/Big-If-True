using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public enum UpdateQuestConnectionOutcome
{
    Success,
    NotFound,
    Cycle,
}

public record UpdateQuestConnectionResult(UpdateQuestConnectionOutcome Outcome, QuestConnectionDto? Connection);
