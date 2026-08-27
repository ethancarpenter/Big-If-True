using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Infrastructure.Repositories;

public class NpcRepository : INpcRepository
{
    private readonly AppDbContext _context;

    public NpcRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Npc>> GetAllForCampaignAsync(Guid campaignId)
    {
        return await _context.Npcs
            .Where(n => n.CampaignId == campaignId)
            .OrderBy(n => n.Name)
            .ToListAsync();
    }

    public async Task<Npc?> GetByIdAsync(Guid id)
    {
        return await _context.Npcs
            .Include(n => n.Campaign)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task AddAsync(Npc npc)
    {
        await _context.Npcs.AddAsync(npc);
    }

    public void Update(Npc npc)
    {
        _context.Npcs.Update(npc);
    }

    public void Remove(Npc npc)
    {
        _context.Npcs.Remove(npc);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
