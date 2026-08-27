using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface ICityRepository
{
    Task<List<City>> GetAllForCampaignAsync(Guid campaignId);

    /// <summary>Includes the parent Campaign so callers can check ownership.</summary>
    Task<City?> GetByIdAsync(Guid id);

    /// <summary>Includes Campaign. Name matches query (case-insensitive, partial), ranked exact/prefix/contains, capped at limit.</summary>
    Task<List<City>> SearchAsync(Guid userId, string query, int limit);

    Task AddAsync(City city);
    void Update(City city);
    void Remove(City city);
    Task SaveChangesAsync();
}
