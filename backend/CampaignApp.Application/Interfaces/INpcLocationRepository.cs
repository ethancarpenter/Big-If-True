using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface INpcLocationRepository
{
    /// <summary>Includes Npc and Location.City, for DTO mapping.</summary>
    Task<List<NpcLocation>> GetAllForNpcAsync(Guid npcId);

    /// <summary>Includes Npc and Location.City, for DTO mapping.</summary>
    Task<List<NpcLocation>> GetAllForLocationAsync(Guid locationId);

    /// <summary>Includes Npc.Campaign (ownership) and Location.City (mapping).</summary>
    Task<NpcLocation?> GetByIdAsync(Guid id);

    /// <summary>For duplicate-relationship checks. No includes - existence only.</summary>
    Task<NpcLocation?> GetExistingAsync(Guid npcId, Guid locationId);

    /// <summary>The NPC's current primary relationship, if any. No includes - flag only.</summary>
    Task<NpcLocation?> GetPrimaryForNpcAsync(Guid npcId);

    Task AddAsync(NpcLocation npcLocation);
    void Update(NpcLocation npcLocation);
    void Remove(NpcLocation npcLocation);
    Task SaveChangesAsync();

    /// <summary>
    /// Persists a primary-relationship switch for an NPC: demotes `demote`
    /// (if given) via its own SaveChangesAsync BEFORE promoting `promote` via
    /// a second SaveChangesAsync, wrapped in one transaction. See
    /// NpcLocationService for why ordering and atomicity both matter here.
    /// </summary>
    Task SavePrimarySwitchAsync(NpcLocation? demote, NpcLocation promote);
}
