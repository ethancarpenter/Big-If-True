using CampaignApp.Application.DTOs;
using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _locationRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICityRepository _cityRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public LocationService(
        ILocationRepository locationRepository,
        ICampaignRepository campaignRepository,
        ICityRepository cityRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _locationRepository = locationRepository;
        _campaignRepository = campaignRepository;
        _cityRepository = cityRepository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<LocationDto>?> GetAllForCampaignAsync(Guid campaignId, Guid? cityId)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var campaign = await _campaignRepository.GetByIdForUserAsync(campaignId, userId);
        if (campaign is null)
        {
            return null;
        }

        var locations = await _locationRepository.GetAllForCampaignAsync(campaignId, cityId);
        return locations.Select(ToDto).ToList();
    }

    public async Task<LocationDto?> GetByIdAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var location = await _locationRepository.GetByIdAsync(id);
        return IsOwnedByUser(location, userId) ? ToDto(location!) : null;
    }

    public async Task<LocationDto?> CreateAsync(Guid campaignId, LocationRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var campaign = await _campaignRepository.GetByIdForUserAsync(campaignId, userId);
        if (campaign is null)
        {
            return null;
        }

        var city = await _cityRepository.GetByIdAsync(request.CityId);
        if (city is null || city.CampaignId != campaignId)
        {
            // The city doesn't exist, or belongs to a different campaign than
            // the one this request is scoped to - treated the same as "not
            // found" rather than confirming some other campaign's city exists.
            return null;
        }

        var now = DateTime.UtcNow;
        var location = new Location
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            CityId = request.CityId,
            City = city,
            Name = request.Name,
            Type = request.Type!.Value,
            Description = request.Description,
            DmNotes = request.DmNotes,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _locationRepository.AddAsync(location);
        await _locationRepository.SaveChangesAsync();

        return ToDto(location);
    }

    public async Task<LocationDto?> UpdateAsync(Guid id, LocationRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var location = await _locationRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(location, userId))
        {
            return null;
        }

        if (request.CityId != location!.CityId)
        {
            var city = await _cityRepository.GetByIdAsync(request.CityId);
            if (city is null || city.CampaignId != location.CampaignId)
            {
                return null;
            }

            location.CityId = request.CityId;
            location.City = city;
        }

        location.Name = request.Name;
        location.Type = request.Type!.Value;
        location.Description = request.Description;
        location.DmNotes = request.DmNotes;
        location.UpdatedAt = DateTime.UtcNow;

        _locationRepository.Update(location);
        await _locationRepository.SaveChangesAsync();

        return ToDto(location);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var location = await _locationRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(location, userId))
        {
            return false;
        }

        _locationRepository.Remove(location!);
        await _locationRepository.SaveChangesAsync();
        return true;
    }

    private static bool IsOwnedByUser(Location? location, Guid userId) =>
        location is not null && location.Campaign.UserId == userId;

    private static LocationDto ToDto(Location location) => new()
    {
        Id = location.Id,
        CampaignId = location.CampaignId,
        CityId = location.CityId,
        CityName = location.City.Name,
        Name = location.Name,
        Type = location.Type,
        Description = location.Description,
        DmNotes = location.DmNotes,
        CreatedAt = location.CreatedAt,
        UpdatedAt = location.UpdatedAt,
    };
}
