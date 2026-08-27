using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public interface INpcService
{
    /// <summary>Null means the campaign doesn't exist or isn't owned by the current user.</summary>
    Task<List<NpcDto>?> GetAllForCampaignAsync(Guid campaignId);

    Task<NpcDto?> GetByIdAsync(Guid id);

    /// <summary>Null means the campaign doesn't exist or isn't owned by the current user.</summary>
    Task<NpcDto?> CreateAsync(Guid campaignId, NpcRequestDto request);

    Task<NpcDto?> UpdateAsync(Guid id, NpcRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
