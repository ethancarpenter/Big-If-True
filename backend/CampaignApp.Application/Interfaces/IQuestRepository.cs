using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface IQuestRepository
{
    /// <summary>Includes Objectives (ordered by SortOrder).</summary>
    Task<List<Quest>> GetAllForCampaignAsync(Guid campaignId);

    /// <summary>Includes Campaign (for ownership checks) and Objectives (ordered by SortOrder).</summary>
    Task<Quest?> GetByIdAsync(Guid id);

    Task AddAsync(Quest quest);
    void Update(Quest quest);
    void Remove(Quest quest);
    Task SaveChangesAsync();
}
