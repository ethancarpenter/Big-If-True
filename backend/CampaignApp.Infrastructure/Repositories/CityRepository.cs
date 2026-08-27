using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Infrastructure.Repositories;

public class CityRepository : ICityRepository
{
    private readonly AppDbContext _context;

    public CityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<City>> GetAllForCampaignAsync(Guid campaignId)
    {
        return await _context.Cities
            .Where(c => c.CampaignId == campaignId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<City?> GetByIdAsync(Guid id)
    {
        return await _context.Cities
            .Include(c => c.Campaign)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<City>> SearchAsync(Guid userId, string query, int limit)
    {
        // See CampaignRepository.SearchAsync for why .ToLower()/.Contains() is used
        // instead of Npgsql's EF.Functions.ILike (InMemory-provider compatibility).
        var normalizedQuery = query.ToLower();
        return await _context.Cities
            .Include(c => c.Campaign)
            .Where(c => c.Campaign.UserId == userId && c.Name.ToLower().Contains(normalizedQuery))
            .OrderBy(c => c.Name.ToLower() == normalizedQuery ? 0 : c.Name.ToLower().StartsWith(normalizedQuery) ? 1 : 2)
            .ThenBy(c => c.Name)
            .Take(limit)
            .ToListAsync();
    }

    public async Task AddAsync(City city)
    {
        await _context.Cities.AddAsync(city);
    }

    public void Update(City city)
    {
        _context.Cities.Update(city);
    }

    public void Remove(City city)
    {
        _context.Cities.Remove(city);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
