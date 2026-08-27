using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface IQuestRepository
{
    /// <summary>Includes Objectives (ordered by SortOrder).</summary>
    Task<List<Quest>> GetAllForCampaignAsync(Guid campaignId);

    /// <summary>Includes Campaign (for ownership checks) and Objectives (ordered by SortOrder).</summary>
    Task<Quest?> GetByIdAsync(Guid id);

    /// <summary>Includes Campaign. Name matches query (case-insensitive, partial), ranked exact/prefix/contains, capped at limit.</summary>
    Task<List<Quest>> SearchAsync(Guid userId, string query, int limit);

    Task AddAsync(Quest quest);
    void Update(Quest quest);
    void Remove(Quest quest);
    Task SaveChangesAsync();
}
