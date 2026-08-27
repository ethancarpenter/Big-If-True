using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface IQuestObjectiveRepository
{
    /// <summary>Ordered by SortOrder. No includes - the caller already holds the parent Quest.</summary>
    Task<List<QuestObjective>> GetAllForQuestAsync(Guid questId);

    /// <summary>Includes Quest.Campaign, for ownership checks.</summary>
    Task<QuestObjective?> GetByIdAsync(Guid id);

    Task AddAsync(QuestObjective objective);
    void Update(QuestObjective objective);
    void Remove(QuestObjective objective);
    Task SaveChangesAsync();
}
