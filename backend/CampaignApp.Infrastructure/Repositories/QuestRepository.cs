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
