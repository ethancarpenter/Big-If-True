using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Infrastructure.Repositories;

public class QuestLocationRepository : IQuestLocationRepository
{
    private readonly AppDbContext _context;

    public QuestLocationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuestLocation>> GetAllForQuestAsync(Guid questId)
    {
        return await _context.QuestLocations
            .Include(ql => ql.Quest)
            .Include(ql => ql.Location).ThenInclude(l => l.City)
            .Where(ql => ql.QuestId == questId)
            .OrderBy(ql => ql.Location.Name)
            .ToListAsync();
    }

    public async Task<List<QuestLocation>> GetAllForLocationAsync(Guid locationId)
    {
        return await _context.QuestLocations
            .Include(ql => ql.Quest)
            .Include(ql => ql.Location).ThenInclude(l => l.City)
            .Where(ql => ql.LocationId == locationId)
            .OrderBy(ql => ql.Quest.Name)
            .ToListAsync();
    }

    public async Task<QuestLocation?> GetByIdAsync(Guid id)
    {
        return await _context.QuestLocations
            .Include(ql => ql.Quest).ThenInclude(q => q.Campaign)
            .Include(ql => ql.Location).ThenInclude(l => l.City)
            .FirstOrDefaultAsync(ql => ql.Id == id);
    }

    public async Task<QuestLocation?> GetExistingAsync(Guid questId, Guid locationId)
    {
        return await _context.QuestLocations
            .FirstOrDefaultAsync(ql => ql.QuestId == questId && ql.LocationId == locationId);
    }

    public async Task AddAsync(QuestLocation questLocation)
    {
        await _context.QuestLocations.AddAsync(questLocation);
    }

    public void Update(QuestLocation questLocation)
    {
        _context.QuestLocations.Update(questLocation);
    }

    public void Remove(QuestLocation questLocation)
    {
        _context.QuestLocations.Remove(questLocation);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
