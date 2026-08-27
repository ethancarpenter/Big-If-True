using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public enum CreateNpcLocationOutcome
{
    Success,

    /// <summary>The NPC doesn't exist/isn't owned by the current user, or the Location doesn't exist/isn't in the same campaign as the NPC.</summary>
    NotFound,

    /// <summary>A relationship between this NPC and Location already exists.</summary>
    Duplicate,
}

public record CreateNpcLocationResult(CreateNpcLocationOutcome Outcome, NpcLocationDto? Relationship);
