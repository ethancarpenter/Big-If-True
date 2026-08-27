namespace CampaignApp.Application.DTOs;

/// <summary>
/// Not a persisted/Domain enum - purely a response-shaping label for
/// SearchResultDto, so it lives with the DTOs rather than Domain.Enums.
/// </summary>
public enum SearchResultType
{
    Campaign,
    City,
    Location,
    Npc,
    Quest,
}
