using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public interface IQuestService
{
    /// <summary>Null means the campaign doesn't exist or isn't owned by the current user.</summary>
    Task<List<QuestDto>?> GetAllForCampaignAsync(Guid campaignId);

    Task<QuestDto?> GetByIdAsync(Guid id);

    /// <summary>Null means the campaign doesn't exist or isn't owned by the current user.</summary>
    Task<QuestDto?> CreateAsync(Guid campaignId, QuestRequestDto request);

    Task<QuestDto?> UpdateAsync(Guid id, QuestRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
