using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public interface IQuestGraphPositionService
{
    Task<List<QuestGraphPositionDto>?> GetAllForCampaignAsync(Guid campaignId);
    Task<QuestGraphPositionDto?> UpsertAsync(Guid questId, QuestGraphPositionUpdateRequestDto request);
}
