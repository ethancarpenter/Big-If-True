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
