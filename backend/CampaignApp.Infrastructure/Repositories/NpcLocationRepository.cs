using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CampaignApp.Infrastructure.Repositories;

public class NpcLocationRepository : INpcLocationRepository
{
    private readonly AppDbContext _context;

    public NpcLocationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<NpcLocation>> GetAllForNpcAsync(Guid npcId)
    {
        return await _context.NpcLocations
            .Include(nl => nl.Npc)
            .Include(nl => nl.Location).ThenInclude(l => l.City)
            .Where(nl => nl.NpcId == npcId)
            .OrderBy(nl => nl.Location.Name)
            .ToListAsync();
    }

    public async Task<List<NpcLocation>> GetAllForLocationAsync(Guid locationId)
    {
        return await _context.NpcLocations
            .Include(nl => nl.Npc)
            .Include(nl => nl.Location).ThenInclude(l => l.City)
            .Where(nl => nl.LocationId == locationId)
            .OrderBy(nl => nl.Npc.Name)
            .ToListAsync();
    }

    public async Task<NpcLocation?> GetByIdAsync(Guid id)
    {
        return await _context.NpcLocations
            .Include(nl => nl.Npc).ThenInclude(n => n.Campaign)
            .Include(nl => nl.Location).ThenInclude(l => l.City)
            .FirstOrDefaultAsync(nl => nl.Id == id);
    }

    public async Task<NpcLocation?> GetExistingAsync(Guid npcId, Guid locationId)
    {
        return await _context.NpcLocations
            .FirstOrDefaultAsync(nl => nl.NpcId == npcId && nl.LocationId == locationId);
    }

    public async Task<NpcLocation?> GetPrimaryForNpcAsync(Guid npcId)
    {
        return await _context.NpcLocations
            .FirstOrDefaultAsync(nl => nl.NpcId == npcId && nl.IsPrimary);
    }

    public async Task AddAsync(NpcLocation npcLocation)
    {
        await _context.NpcLocations.AddAsync(npcLocation);
    }

    public void Update(NpcLocation npcLocation)
    {
        _context.NpcLocations.Update(npcLocation);
    }

    public void Remove(NpcLocation npcLocation)
    {
        _context.NpcLocations.Remove(npcLocation);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task SavePrimarySwitchAsync(NpcLocation? demote, NpcLocation promote)
    {
        if (demote is null)
        {
            promote.IsPrimary = true;
            await _context.SaveChangesAsync(); // nothing to sequence; a single save is already atomic
            return;
        }

        // The EF Core InMemory provider (used in tests) doesn't support real
        // transactions; fall back to sequential (still correctly-ordered)
        // saves without one. InMemory isn't used to verify rollback-on-
        // failure - only the demote-before-promote ordering, which holds
        // either way.
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await _context.Database.BeginTransactionAsync();
        }
        catch (InvalidOperationException)
        {
        }

        try
        {
            demote.IsPrimary = false;
            await _context.SaveChangesAsync(); // statement 1: demotion, reaches Postgres first

            promote.IsPrimary = true;
            await _context.SaveChangesAsync(); // statement 2: promotion - at most one IsPrimary=true row ever exists

            if (transaction is not null)
            {
                await transaction.CommitAsync();
            }
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync();
            }

            throw;
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }
}
