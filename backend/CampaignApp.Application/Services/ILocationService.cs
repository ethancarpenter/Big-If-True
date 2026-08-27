using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public interface ILocationService
{
    /// <summary>Null means the campaign doesn't exist or isn't owned by the current user.</summary>
    Task<List<LocationDto>?> GetAllForCampaignAsync(Guid campaignId, Guid? cityId);

    Task<LocationDto?> GetByIdAsync(Guid id);

    /// <summary>
    /// Null means the parent campaign doesn't exist/isn't owned by the current
    /// user, or the given CityId doesn't exist within that campaign.
    /// </summary>
    Task<LocationDto?> CreateAsync(Guid campaignId, LocationRequestDto request);

    /// <summary>Null also covers reassigning to a CityId outside the location's own campaign.</summary>
    Task<LocationDto?> UpdateAsync(Guid id, LocationRequestDto request);

    Task<bool> DeleteAsync(Guid id);
}
