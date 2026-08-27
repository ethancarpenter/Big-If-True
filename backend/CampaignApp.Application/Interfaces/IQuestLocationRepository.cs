using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface IQuestLocationRepository
{
    /// <summary>Includes Location.City, for DTO mapping.</summary>
    Task<List<QuestLocation>> GetAllForQuestAsync(Guid questId);

    /// <summary>Includes Quest and Location.City, for DTO mapping.</summary>
    Task<List<QuestLocation>> GetAllForLocationAsync(Guid locationId);

    /// <summary>Includes Quest.Campaign (ownership) and Location.City (mapping).</summary>
    Task<QuestLocation?> GetByIdAsync(Guid id);

    /// <summary>For duplicate-relationship checks. No includes - existence only.</summary>
    Task<QuestLocation?> GetExistingAsync(Guid questId, Guid locationId);

    Task AddAsync(QuestLocation questLocation);
    void Update(QuestLocation questLocation);
    void Remove(QuestLocation questLocation);
    Task SaveChangesAsync();
}
