using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface ICampaignRepository
{
    Task<List<Campaign>> GetAllForUserAsync(Guid userId);
    Task<Campaign?> GetByIdForUserAsync(Guid id, Guid userId);
    Task AddAsync(Campaign campaign);
    void Update(Campaign campaign);
    void Remove(Campaign campaign);
    Task SaveChangesAsync();
}
