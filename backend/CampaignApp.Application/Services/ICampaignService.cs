using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public interface ICampaignService
{
    Task<List<CampaignDto>> GetAllAsync();
    Task<CampaignDto?> GetByIdAsync(Guid id);
    Task<CampaignDto> CreateAsync(CampaignRequestDto request);
    Task<CampaignDto?> UpdateAsync(Guid id, CampaignRequestDto request);
    Task<bool> DeleteAsync(Guid id);
}
