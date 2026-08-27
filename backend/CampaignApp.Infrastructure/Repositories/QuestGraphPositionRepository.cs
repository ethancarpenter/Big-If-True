using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Infrastructure.Repositories;

public class QuestGraphPositionRepository : IQuestGraphPositionRepository
{
    private readonly AppDbContext _context;

    public QuestGraphPositionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuestGraphPosition>> GetAllForCampaignAsync(Guid campaignId)
    {
        return await _context.QuestGraphPositions
            .Where(p => p.Quest.CampaignId == campaignId)
            .ToListAsync();
    }

    public async Task<QuestGraphPosition?> GetByQuestIdAsync(Guid questId)
    {
        return await _context.QuestGraphPositions
            .FirstOrDefaultAsync(p => p.QuestId == questId);
    }

    public async Task AddAsync(QuestGraphPosition position)
    {
        await _context.QuestGraphPositions.AddAsync(position);
    }

    public void Update(QuestGraphPosition position)
    {
        _context.QuestGraphPositions.Update(position);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
