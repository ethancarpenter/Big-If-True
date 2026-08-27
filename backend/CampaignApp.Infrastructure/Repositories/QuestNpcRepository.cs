using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Infrastructure.Repositories;

public class QuestNpcRepository : IQuestNpcRepository
{
    private readonly AppDbContext _context;

    public QuestNpcRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuestNpc>> GetAllForQuestAsync(Guid questId)
    {
        return await _context.QuestNpcs
            .Include(qn => qn.Quest)
            .Include(qn => qn.Npc)
            .Where(qn => qn.QuestId == questId)
            .OrderBy(qn => qn.Npc.Name)
            .ToListAsync();
    }

    public async Task<List<QuestNpc>> GetAllForNpcAsync(Guid npcId)
    {
        return await _context.QuestNpcs
            .Include(qn => qn.Quest)
            .Include(qn => qn.Npc)
            .Where(qn => qn.NpcId == npcId)
            .OrderBy(qn => qn.Quest.Name)
            .ToListAsync();
    }

    public async Task<QuestNpc?> GetByIdAsync(Guid id)
    {
        return await _context.QuestNpcs
            .Include(qn => qn.Quest).ThenInclude(q => q.Campaign)
            .Include(qn => qn.Npc)
            .FirstOrDefaultAsync(qn => qn.Id == id);
    }

    public async Task<QuestNpc?> GetExistingAsync(Guid questId, Guid npcId)
    {
        return await _context.QuestNpcs
            .FirstOrDefaultAsync(qn => qn.QuestId == questId && qn.NpcId == npcId);
    }

    public async Task AddAsync(QuestNpc questNpc)
    {
        await _context.QuestNpcs.AddAsync(questNpc);
    }

    public void Update(QuestNpc questNpc)
    {
        _context.QuestNpcs.Update(questNpc);
    }

    public void Remove(QuestNpc questNpc)
    {
        _context.QuestNpcs.Remove(questNpc);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
