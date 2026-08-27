using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Infrastructure.Repositories;

public class CampaignRepository : ICampaignRepository
{
    private readonly AppDbContext _context;

    public CampaignRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Campaign>> GetAllForUserAsync(Guid userId)
    {
        return await _context.Campaigns
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Campaign?> GetByIdForUserAsync(Guid id, Guid userId)
    {
        return await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    public async Task<List<Campaign>> SearchAsync(Guid userId, string query, int limit)
    {
        // .ToLower()/.Contains()/.StartsWith() rather than Npgsql's EF.Functions.ILike:
        // these translate to plain SQL on Postgres (LOWER()+LIKE) *and* evaluate
        // correctly against the InMemory provider every test in this suite uses -
        // ILike throws "not supported" once InMemory has to evaluate it against
        // actual rows, since it's an Npgsql-only function InMemory has no
        // translation for.
        var normalizedQuery = query.ToLower();
        return await _context.Campaigns
            .Where(c => c.UserId == userId && c.Name.ToLower().Contains(normalizedQuery))
            .OrderBy(c => c.Name.ToLower() == normalizedQuery ? 0 : c.Name.ToLower().StartsWith(normalizedQuery) ? 1 : 2)
            .ThenBy(c => c.Name)
            .Take(limit)
            .ToListAsync();
    }

    public async Task AddAsync(Campaign campaign)
    {
        await _context.Campaigns.AddAsync(campaign);
    }

    public void Update(Campaign campaign)
    {
        _context.Campaigns.Update(campaign);
    }

    public void Remove(Campaign campaign)
    {
        _context.Campaigns.Remove(campaign);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
