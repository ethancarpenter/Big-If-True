using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface INpcRepository
{
    Task<List<Npc>> GetAllForCampaignAsync(Guid campaignId);

    /// <summary>Includes Campaign, for ownership checks.</summary>
    Task<Npc?> GetByIdAsync(Guid id);

    Task AddAsync(Npc npc);
    void Update(Npc npc);
    void Remove(Npc npc);
    Task SaveChangesAsync();
}
