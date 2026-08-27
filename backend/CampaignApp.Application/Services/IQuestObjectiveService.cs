using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public interface IQuestObjectiveService
{
    /// <summary>Null means the quest doesn't exist or isn't owned by the current user.</summary>
    Task<List<QuestObjectiveDto>?> GetAllForQuestAsync(Guid questId);

    /// <summary>Null means the quest doesn't exist or isn't owned by the current user.</summary>
    Task<QuestObjectiveDto?> CreateAsync(Guid questId, QuestObjectiveCreateRequestDto request);

    Task<QuestObjectiveDto?> UpdateAsync(Guid id, QuestObjectiveUpdateRequestDto request);
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Null means the quest doesn't exist/isn't owned, or the given id list
    /// doesn't exactly match the quest's current set of objective ids.
    /// </summary>
    Task<List<QuestObjectiveDto>?> ReorderAsync(Guid questId, QuestObjectiveReorderRequestDto request);
}
