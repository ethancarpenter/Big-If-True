using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface ICityRepository
{
    Task<List<City>> GetAllForCampaignAsync(Guid campaignId);

    /// <summary>Includes the parent Campaign so callers can check ownership.</summary>
    Task<City?> GetByIdAsync(Guid id);

    Task AddAsync(City city);
    void Update(City city);
    void Remove(City city);
    Task SaveChangesAsync();
}
