using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public interface IQuestConnectionService
{
    Task<List<QuestConnectionDto>?> GetAllForCampaignAsync(Guid campaignId);
    Task<QuestConnectionDto?> GetByIdAsync(Guid id);
    Task<CreateQuestConnectionResult> CreateAsync(Guid campaignId, QuestConnectionCreateRequestDto request);
    Task<UpdateQuestConnectionResult> UpdateAsync(Guid id, QuestConnectionUpdateRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
