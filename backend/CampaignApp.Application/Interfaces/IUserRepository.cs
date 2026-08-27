using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByNormalizedEmailAsync(string normalizedEmail);
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    void Update(User user);
    Task SaveChangesAsync();
}
