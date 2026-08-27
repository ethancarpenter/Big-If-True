using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface INpcRepository
{
    Task<List<Npc>> GetAllForCampaignAsync(Guid campaignId);

    /// <summary>Includes Campaign, for ownership checks.</summary>
    Task<Npc?> GetByIdAsync(Guid id);

    /// <summary>Includes Campaign. Name matches query (case-insensitive, partial), ranked exact/prefix/contains, capped at limit.</summary>
    Task<List<Npc>> SearchAsync(Guid userId, string query, int limit);

    Task AddAsync(Npc npc);
    void Update(Npc npc);
    void Remove(Npc npc);
    Task SaveChangesAsync();
}
