using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public interface ICityService
{
    /// <summary>Null means the campaign doesn't exist or isn't owned by the current user.</summary>
    Task<List<CityDto>?> GetAllForCampaignAsync(Guid campaignId);

    Task<CityDto?> GetByIdAsync(Guid id);

    /// <summary>Null means the parent campaign doesn't exist or isn't owned by the current user.</summary>
    Task<CityDto?> CreateAsync(Guid campaignId, CityRequestDto request);

    Task<CityDto?> UpdateAsync(Guid id, CityRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
