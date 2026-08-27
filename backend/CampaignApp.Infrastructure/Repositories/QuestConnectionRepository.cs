using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Infrastructure.Repositories;

public class QuestConnectionRepository : IQuestConnectionRepository
{
    private readonly AppDbContext _context;

    public QuestConnectionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuestConnection>> GetAllForCampaignAsync(Guid campaignId)
    {
        return await _context.QuestConnections
            .Include(qc => qc.SourceQuest)
            .Include(qc => qc.TargetQuest)
            .Where(qc => qc.SourceQuest.CampaignId == campaignId)
            .OrderBy(qc => qc.SourceQuest.Name)
            .ToListAsync();
    }

    public async Task<QuestConnection?> GetByIdAsync(Guid id)
    {
        return await _context.QuestConnections
            .Include(qc => qc.SourceQuest).ThenInclude(q => q.Campaign)
            .Include(qc => qc.TargetQuest)
            .FirstOrDefaultAsync(qc => qc.Id == id);
    }

    public async Task<QuestConnection?> GetExistingAsync(Guid sourceQuestId, Guid targetQuestId)
    {
        return await _context.QuestConnections
            .FirstOrDefaultAsync(qc => qc.SourceQuestId == sourceQuestId && qc.TargetQuestId == targetQuestId);
    }

    public async Task AddAsync(QuestConnection connection)
    {
        await _context.QuestConnections.AddAsync(connection);
    }

    public void Update(QuestConnection connection)
    {
        _context.QuestConnections.Update(connection);
    }

    public void Remove(QuestConnection connection)
    {
        _context.QuestConnections.Remove(connection);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
