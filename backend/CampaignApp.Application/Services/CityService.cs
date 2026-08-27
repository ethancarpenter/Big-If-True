using CampaignApp.Application.DTOs;
using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Services;

public class CityService : ICityService
{
    private readonly ICityRepository _cityRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public CityService(
        ICityRepository cityRepository,
        ICampaignRepository campaignRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _cityRepository = cityRepository;
        _campaignRepository = campaignRepository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<CityDto>?> GetAllForCampaignAsync(Guid campaignId)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var campaign = await _campaignRepository.GetByIdForUserAsync(campaignId, userId);
        if (campaign is null)
        {
            return null;
        }

        var cities = await _cityRepository.GetAllForCampaignAsync(campaignId);
        return cities.Select(ToDto).ToList();
    }

    public async Task<CityDto?> GetByIdAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var city = await _cityRepository.GetByIdAsync(id);
        return IsOwnedByUser(city, userId) ? ToDto(city!) : null;
    }

    public async Task<CityDto?> CreateAsync(Guid campaignId, CityRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var campaign = await _campaignRepository.GetByIdForUserAsync(campaignId, userId);
        if (campaign is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var city = new City
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = request.Name,
            Description = request.Description,
            Population = request.Population,
            Government = request.Government,
            Region = request.Region,
            Alignment = request.Alignment,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _cityRepository.AddAsync(city);
        await _cityRepository.SaveChangesAsync();

        return ToDto(city);
    }

    public async Task<CityDto?> UpdateAsync(Guid id, CityRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var city = await _cityRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(city, userId))
        {
            return null;
        }

        city!.Name = request.Name;
        city.Description = request.Description;
        city.Population = request.Population;
        city.Government = request.Government;
        city.Region = request.Region;
        city.Alignment = request.Alignment;
        city.UpdatedAt = DateTime.UtcNow;

        _cityRepository.Update(city);
        await _cityRepository.SaveChangesAsync();

        return ToDto(city);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var city = await _cityRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(city, userId))
        {
            return false;
        }

        _cityRepository.Remove(city!);
        await _cityRepository.SaveChangesAsync();
        return true;
    }

    private static bool IsOwnedByUser(City? city, Guid userId) =>
        city is not null && city.Campaign.UserId == userId;

    private static CityDto ToDto(City city) => new()
    {
        Id = city.Id,
        CampaignId = city.CampaignId,
        Name = city.Name,
        Description = city.Description,
        Population = city.Population,
        Government = city.Government,
        Region = city.Region,
        Alignment = city.Alignment,
        CreatedAt = city.CreatedAt,
        UpdatedAt = city.UpdatedAt,
    };
}
