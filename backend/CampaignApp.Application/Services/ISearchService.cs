using CampaignApp.Application.DTOs;

namespace CampaignApp.Application.Services;

public interface ISearchService
{
    Task<List<SearchResultDto>> SearchAsync(string? query);
}
