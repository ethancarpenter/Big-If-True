using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _context;

    public LocationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Location>> GetAllForCampaignAsync(Guid campaignId, Guid? cityId)
    {
        var query = _context.Locations
            .Include(l => l.City)
            .Where(l => l.CampaignId == campaignId);

        if (cityId.HasValue)
        {
            query = query.Where(l => l.CityId == cityId.Value);
        }

        return await query.OrderBy(l => l.Name).ToListAsync();
    }

    public async Task<Location?> GetByIdAsync(Guid id)
    {
        return await _context.Locations
            .Include(l => l.Campaign)
            .Include(l => l.City)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<List<Location>> SearchAsync(Guid userId, string query, int limit)
    {
        // See CampaignRepository.SearchAsync for why .ToLower()/.Contains() is used
        // instead of Npgsql's EF.Functions.ILike (InMemory-provider compatibility).
        var normalizedQuery = query.ToLower();
        return await _context.Locations
            .Include(l => l.Campaign)
            .Include(l => l.City)
            .Where(l => l.Campaign.UserId == userId && l.Name.ToLower().Contains(normalizedQuery))
            .OrderBy(l => l.Name.ToLower() == normalizedQuery ? 0 : l.Name.ToLower().StartsWith(normalizedQuery) ? 1 : 2)
            .ThenBy(l => l.Name)
            .Take(limit)
            .ToListAsync();
    }

    public async Task AddAsync(Location location)
    {
        await _context.Locations.AddAsync(location);
    }

    public void Update(Location location)
    {
        _context.Locations.Update(location);
    }

    public void Remove(Location location)
    {
        _context.Locations.Remove(location);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
