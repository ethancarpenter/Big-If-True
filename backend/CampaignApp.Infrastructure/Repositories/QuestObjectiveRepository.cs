using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Infrastructure.Repositories;

public class QuestObjectiveRepository : IQuestObjectiveRepository
{
    private readonly AppDbContext _context;

    public QuestObjectiveRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuestObjective>> GetAllForQuestAsync(Guid questId)
    {
        return await _context.QuestObjectives
            .Where(o => o.QuestId == questId)
            .OrderBy(o => o.SortOrder)
            .ToListAsync();
    }

    public async Task<QuestObjective?> GetByIdAsync(Guid id)
    {
        return await _context.QuestObjectives
            .Include(o => o.Quest).ThenInclude(q => q.Campaign)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task AddAsync(QuestObjective objective)
    {
        await _context.QuestObjectives.AddAsync(objective);
    }

    public void Update(QuestObjective objective)
    {
        _context.QuestObjectives.Update(objective);
    }

    public void Remove(QuestObjective objective)
    {
        _context.QuestObjectives.Remove(objective);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
