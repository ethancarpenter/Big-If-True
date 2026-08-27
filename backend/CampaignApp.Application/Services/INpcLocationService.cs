using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public interface INpcLocationService
{
    /// <summary>Null means the NPC doesn't exist or isn't owned by the current user.</summary>
    Task<List<NpcLocationDto>?> GetAllForNpcAsync(Guid npcId);

    /// <summary>Null means the Location doesn't exist or isn't owned by the current user.</summary>
    Task<List<NpcLocationDto>?> GetAllForLocationAsync(Guid locationId);

    Task<NpcLocationDto?> GetByIdAsync(Guid id);

    Task<CreateNpcLocationResult> CreateAsync(Guid npcId, NpcLocationCreateRequestDto request);

    Task<NpcLocationDto?> UpdateAsync(Guid id, NpcLocationUpdateRequestDto request);

    Task<bool> DeleteAsync(Guid id);
}
