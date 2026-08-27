using CampaignApp.Application.DTOs;
using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Services;

public class CampaignService : ICampaignService
{
    private readonly ICampaignRepository _repository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public CampaignService(ICampaignRepository repository, ICurrentUserProvider currentUserProvider)
    {
        _repository = repository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<CampaignDto>> GetAllAsync()
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var campaigns = await _repository.GetAllForUserAsync(userId);
        return campaigns.Select(ToDto).ToList();
    }

    public async Task<CampaignDto?> GetByIdAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var campaign = await _repository.GetByIdForUserAsync(id, userId);
        return campaign is null ? null : ToDto(campaign);
    }

    public async Task<CampaignDto> CreateAsync(CampaignRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var now = DateTime.UtcNow;

        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            Description = request.Description,
            CoverImageUrl = request.CoverImageUrl,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(campaign);
        await _repository.SaveChangesAsync();

        return ToDto(campaign);
    }

    public async Task<CampaignDto?> UpdateAsync(Guid id, CampaignRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var campaign = await _repository.GetByIdForUserAsync(id, userId);
        if (campaign is null)
        {
            return null;
        }

        campaign.Name = request.Name;
        campaign.Description = request.Description;
        campaign.CoverImageUrl = request.CoverImageUrl;
        campaign.UpdatedAt = DateTime.UtcNow;

        _repository.Update(campaign);
        await _repository.SaveChangesAsync();

        return ToDto(campaign);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var campaign = await _repository.GetByIdForUserAsync(id, userId);
        if (campaign is null)
        {
            return false;
        }

        _repository.Remove(campaign);
        await _repository.SaveChangesAsync();
        return true;
    }

    private static CampaignDto ToDto(Campaign campaign) => new()
    {
        Id = campaign.Id,
        Name = campaign.Name,
        Description = campaign.Description,
        CoverImageUrl = campaign.CoverImageUrl,
        CreatedAt = campaign.CreatedAt,
        UpdatedAt = campaign.UpdatedAt,
    };
}
