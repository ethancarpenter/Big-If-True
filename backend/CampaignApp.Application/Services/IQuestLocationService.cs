using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public interface IQuestLocationService
{
    /// <summary>Null means the quest doesn't exist or isn't owned by the current user.</summary>
    Task<List<QuestLocationDto>?> GetAllForQuestAsync(Guid questId);

    /// <summary>Null means the Location doesn't exist or isn't owned by the current user.</summary>
    Task<List<QuestLocationDto>?> GetAllForLocationAsync(Guid locationId);

    Task<QuestLocationDto?> GetByIdAsync(Guid id);

    Task<CreateQuestLocationResult> CreateAsync(Guid questId, QuestLocationCreateRequestDto request);

    Task<QuestLocationDto?> UpdateAsync(Guid id, QuestLocationUpdateRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
