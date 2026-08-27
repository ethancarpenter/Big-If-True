using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface IQuestNpcRepository
{
    /// <summary>Includes Npc, for DTO mapping.</summary>
    Task<List<QuestNpc>> GetAllForQuestAsync(Guid questId);

    /// <summary>Includes Quest, for DTO mapping.</summary>
    Task<List<QuestNpc>> GetAllForNpcAsync(Guid npcId);

    /// <summary>Includes Quest.Campaign (ownership) and Npc (mapping).</summary>
    Task<QuestNpc?> GetByIdAsync(Guid id);

    /// <summary>For duplicate-relationship checks. No includes - existence only.</summary>
    Task<QuestNpc?> GetExistingAsync(Guid questId, Guid npcId);

    Task AddAsync(QuestNpc questNpc);
    void Update(QuestNpc questNpc);
    void Remove(QuestNpc questNpc);
    Task SaveChangesAsync();
}
