using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public interface IQuestNpcService
{
    /// <summary>Null means the quest doesn't exist or isn't owned by the current user.</summary>
    Task<List<QuestNpcDto>?> GetAllForQuestAsync(Guid questId);

    /// <summary>Null means the NPC doesn't exist or isn't owned by the current user.</summary>
    Task<List<QuestNpcDto>?> GetAllForNpcAsync(Guid npcId);

    Task<QuestNpcDto?> GetByIdAsync(Guid id);

    Task<CreateQuestNpcResult> CreateAsync(Guid questId, QuestNpcCreateRequestDto request);

    Task<QuestNpcDto?> UpdateAsync(Guid id, QuestNpcUpdateRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
