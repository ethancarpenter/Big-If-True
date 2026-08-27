using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Infrastructure.Repositories;

public class QuestRepository : IQuestRepository
{
    private readonly AppDbContext _context;

    public QuestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Quest>> GetAllForCampaignAsync(Guid campaignId)
    {
        return await _context.Quests
            .Include(q => q.Objectives)
            .Where(q => q.CampaignId == campaignId)
            .OrderBy(q => q.Name)
            .ToListAsync();
    }

    public async Task<Quest?> GetByIdAsync(Guid id)
    {
        return await _context.Quests
            .Include(q => q.Campaign)
            .Include(q => q.Objectives)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<List<Quest>> SearchAsync(Guid userId, string query, int limit)
    {
        // See CampaignRepository.SearchAsync for why .ToLower()/.Contains() is used
        // instead of Npgsql's EF.Functions.ILike (InMemory-provider compatibility).
        var normalizedQuery = query.ToLower();
        return await _context.Quests
            .Include(q => q.Campaign)
            .Where(q => q.Campaign.UserId == userId && q.Name.ToLower().Contains(normalizedQuery))
            .OrderBy(q => q.Name.ToLower() == normalizedQuery ? 0 : q.Name.ToLower().StartsWith(normalizedQuery) ? 1 : 2)
            .ThenBy(q => q.Name)
            .Take(limit)
            .ToListAsync();
    }

    public async Task AddAsync(Quest quest)
    {
        await _context.Quests.AddAsync(quest);
    }

    public void Update(Quest quest)
    {
        _context.Quests.Update(quest);
    }

    public void Remove(Quest quest)
    {
        _context.Quests.Remove(quest);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
