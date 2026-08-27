using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface ICampaignRepository
{
    Task<List<Campaign>> GetAllForUserAsync(Guid userId);
    Task<Campaign?> GetByIdForUserAsync(Guid id, Guid userId);

    /// <summary>Name matches query (case-insensitive, partial), ranked exact/prefix/contains, capped at limit.</summary>
    Task<List<Campaign>> SearchAsync(Guid userId, string query, int limit);
    Task AddAsync(Campaign campaign);
    void Update(Campaign campaign);
    void Remove(Campaign campaign);
    Task SaveChangesAsync();
}
