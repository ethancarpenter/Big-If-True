using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface ILocationRepository
{
    /// <summary>Includes City (for CityName on the DTO); results are scoped to one campaign, optionally to one city.</summary>
    Task<List<Location>> GetAllForCampaignAsync(Guid campaignId, Guid? cityId);

    /// <summary>Includes Campaign (for ownership checks) and City (for CityName on the DTO).</summary>
    Task<Location?> GetByIdAsync(Guid id);

    Task AddAsync(Location location);
    void Update(Location location);
    void Remove(Location location);
    Task SaveChangesAsync();
}
