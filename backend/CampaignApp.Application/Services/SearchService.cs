using CampaignApp.Application.DTOs;
using CampaignApp.Application.Interfaces;

namespace CampaignApp.Application.Services;

public class SearchService : ISearchService
{
    private const int ResultsPerType = 5;

    private readonly ICampaignRepository _campaignRepository;
    private readonly ICityRepository _cityRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly INpcRepository _npcRepository;
    private readonly IQuestRepository _questRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public SearchService(
        ICampaignRepository campaignRepository,
        ICityRepository cityRepository,
        ILocationRepository locationRepository,
        INpcRepository npcRepository,
        IQuestRepository questRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _campaignRepository = campaignRepository;
        _cityRepository = cityRepository;
        _locationRepository = locationRepository;
        _npcRepository = npcRepository;
        _questRepository = questRepository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<SearchResultDto>> SearchAsync(string? query)
    {
        var trimmed = query?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return [];
        }

        var userId = _currentUserProvider.GetCurrentUserId();

        // Sequential, not Task.WhenAll: all five repositories share one
        // scoped AppDbContext, which cannot run concurrent operations.
        var campaigns = await _campaignRepository.SearchAsync(userId, trimmed, ResultsPerType);
        var cities = await _cityRepository.SearchAsync(userId, trimmed, ResultsPerType);
        var locations = await _locationRepository.SearchAsync(userId, trimmed, ResultsPerType);
        var npcs = await _npcRepository.SearchAsync(userId, trimmed, ResultsPerType);
        var quests = await _questRepository.SearchAsync(userId, trimmed, ResultsPerType);

        var results = new List<SearchResultDto>();

        results.AddRange(campaigns.Select(c => new SearchResultDto
        {
            Type = SearchResultType.Campaign,
            Id = c.Id,
            Name = c.Name,
            CampaignId = c.Id,
            CampaignName = c.Name,
            Url = $"/campaigns/{c.Id}",
        }));

        results.AddRange(cities.Select(c => new SearchResultDto
        {
            Type = SearchResultType.City,
            Id = c.Id,
            Name = c.Name,
            CampaignId = c.CampaignId,
            CampaignName = c.Campaign.Name,
            Url = $"/campaigns/{c.CampaignId}/cities/{c.Id}",
        }));

        results.AddRange(locations.Select(l => new SearchResultDto
        {
            Type = SearchResultType.Location,
            Id = l.Id,
            Name = l.Name,
            CampaignId = l.CampaignId,
            CampaignName = l.Campaign.Name,
            ParentContext = l.City.Name,
            Url = $"/campaigns/{l.CampaignId}/locations/{l.Id}",
        }));

        results.AddRange(npcs.Select(n => new SearchResultDto
        {
            Type = SearchResultType.Npc,
            Id = n.Id,
            Name = n.Name,
            CampaignId = n.CampaignId,
            CampaignName = n.Campaign.Name,
            Url = $"/campaigns/{n.CampaignId}/npcs/{n.Id}",
        }));

        results.AddRange(quests.Select(q => new SearchResultDto
        {
            Type = SearchResultType.Quest,
            Id = q.Id,
            Name = q.Name,
            CampaignId = q.CampaignId,
            CampaignName = q.Campaign.Name,
            Url = $"/campaigns/{q.CampaignId}/quests/{q.Id}",
        }));

        return results;
    }
}
