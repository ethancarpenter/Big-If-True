using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Tests.Services;

public class SearchServiceTests
{
    private sealed record Services(
        SearchService SearchService,
        CampaignService CampaignService,
        CityService CityService,
        LocationService LocationService,
        NpcService NpcService,
        QuestService QuestService,
        FakeCurrentUserProvider User);

    private static Services CreateServices()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);
        var currentUser = new FakeCurrentUserProvider();

        var campaignRepository = new CampaignRepository(context);
        var campaignService = new CampaignService(campaignRepository, currentUser);

        var cityRepository = new CityRepository(context);
        var cityService = new CityService(cityRepository, campaignRepository, currentUser);

        var locationRepository = new LocationRepository(context);
        var locationService = new LocationService(locationRepository, campaignRepository, cityRepository, currentUser);

        var npcRepository = new NpcRepository(context);
        var npcService = new NpcService(npcRepository, campaignRepository, currentUser);

        var questRepository = new QuestRepository(context);
        var questService = new QuestService(questRepository, campaignRepository, currentUser);

        var searchService = new SearchService(
            campaignRepository, cityRepository, locationRepository, npcRepository, questRepository, currentUser);

        return new Services(searchService, campaignService, cityService, locationService, npcService, questService, currentUser);
    }

    private static async Task<(Guid CampaignId, Guid CityId, Guid LocationId, Guid NpcId, Guid QuestId)> SeedFullCampaignAsync(
        Services s, string campaignName = "The Northern Reach", string cityName = "Stonehaven",
        string locationName = "Ironforge Inn", string npcName = "Eldrin Vale", string questName = "Missing Caravan")
    {
        var campaign = await s.CampaignService.CreateAsync(new CampaignRequestDto { Name = campaignName });
        var city = await s.CityService.CreateAsync(campaign.Id, new CityRequestDto { Name = cityName });
        var location = await s.LocationService.CreateAsync(campaign.Id, new LocationRequestDto
        {
            CityId = city!.Id,
            Name = locationName,
            Type = LocationType.Tavern,
        });
        var npc = await s.NpcService.CreateAsync(campaign.Id, new NpcRequestDto { Name = npcName, Status = NpcStatus.Alive });
        var quest = await s.QuestService.CreateAsync(campaign.Id, new QuestRequestDto
        {
            Name = questName,
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });
        return (campaign.Id, city.Id, location!.Id, npc!.Id, quest!.Id);
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmptyList_ForEmptyQuery()
    {
        var s = CreateServices();
        await SeedFullCampaignAsync(s);

        var result = await s.SearchService.SearchAsync("");

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmptyList_ForNullQuery()
    {
        var s = CreateServices();
        await SeedFullCampaignAsync(s);

        var result = await s.SearchService.SearchAsync(null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmptyList_ForWhitespaceQuery()
    {
        var s = CreateServices();
        await SeedFullCampaignAsync(s);

        var result = await s.SearchService.SearchAsync("   ");

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmptyList_ForNoMatchingQuery()
    {
        var s = CreateServices();
        await SeedFullCampaignAsync(s);

        var result = await s.SearchService.SearchAsync("Xyzzyxyz");

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAsync_MatchesCaseInsensitively()
    {
        var s = CreateServices();
        await SeedFullCampaignAsync(s);

        var result = await s.SearchService.SearchAsync("eldrin");

        Assert.Contains(result, r => r.Type == SearchResultType.Npc && r.Name == "Eldrin Vale");
    }

    [Fact]
    public async Task SearchAsync_MatchesPartialSubstring()
    {
        var s = CreateServices();
        await SeedFullCampaignAsync(s);

        var result = await s.SearchService.SearchAsync("Vale");

        Assert.Contains(result, r => r.Type == SearchResultType.Npc && r.Name == "Eldrin Vale");
    }

    [Fact]
    public async Task SearchAsync_ReturnsCorrectlyShapedResults_AcrossAllFiveTypes()
    {
        var s = CreateServices();
        var (campaignId, _, locationId, npcId, questId) = await SeedFullCampaignAsync(
            s, campaignName: "Zephyr Campaign", cityName: "Zephyr City",
            locationName: "Zephyr Inn", npcName: "Zephyr the Wise", questName: "Zephyr's Trial");

        var result = await s.SearchService.SearchAsync("Zephyr");

        Assert.Equal(5, result.Count); // Campaign, City, Location, Npc, Quest all match "Zephyr"

        var campaignResult = Assert.Single(result, r => r.Type == SearchResultType.Campaign);
        Assert.Equal(campaignId, campaignResult.Id);
        Assert.Equal(campaignId, campaignResult.CampaignId);
        Assert.Equal("Zephyr Campaign", campaignResult.CampaignName);
        Assert.Null(campaignResult.ParentContext);
        Assert.Equal($"/campaigns/{campaignId}", campaignResult.Url);

        var locationResult = Assert.Single(result, r => r.Type == SearchResultType.Location);
        Assert.Equal(locationId, locationResult.Id);
        Assert.Equal(campaignId, locationResult.CampaignId);
        Assert.Equal("Zephyr Campaign", locationResult.CampaignName);
        Assert.Equal("Zephyr City", locationResult.ParentContext);
        Assert.Equal($"/campaigns/{campaignId}/locations/{locationId}", locationResult.Url);

        var npcResult = Assert.Single(result, r => r.Type == SearchResultType.Npc);
        Assert.Equal(npcId, npcResult.Id);
        Assert.Null(npcResult.ParentContext);
        Assert.Equal($"/campaigns/{campaignId}/npcs/{npcId}", npcResult.Url);

        var questResult = Assert.Single(result, r => r.Type == SearchResultType.Quest);
        Assert.Equal(questId, questResult.Id);
        Assert.Equal($"/campaigns/{campaignId}/quests/{questId}", questResult.Url);
    }

    [Fact]
    public async Task SearchAsync_CapsResultsPerType()
    {
        var s = CreateServices();
        var campaign = await s.CampaignService.CreateAsync(new CampaignRequestDto { Name = "Cap Test Campaign" });
        for (var i = 0; i < 8; i++)
        {
            await s.NpcService.CreateAsync(campaign.Id, new NpcRequestDto { Name = $"Guard {i}", Status = NpcStatus.Alive });
        }

        var result = await s.SearchService.SearchAsync("Guard");

        Assert.Equal(5, result.Count(r => r.Type == SearchResultType.Npc));
    }

    [Fact]
    public async Task SearchAsync_OrdersExactMatchBeforePrefixBeforeContains()
    {
        var s = CreateServices();
        var campaign = await s.CampaignService.CreateAsync(new CampaignRequestDto { Name = "Ordering Test Campaign" });
        await s.NpcService.CreateAsync(campaign.Id, new NpcRequestDto { Name = "The Great Mara", Status = NpcStatus.Alive }); // contains
        await s.NpcService.CreateAsync(campaign.Id, new NpcRequestDto { Name = "Mara the Bold", Status = NpcStatus.Alive }); // prefix
        await s.NpcService.CreateAsync(campaign.Id, new NpcRequestDto { Name = "Mara", Status = NpcStatus.Alive }); // exact

        var result = await s.SearchService.SearchAsync("Mara");
        var npcNames = result.Where(r => r.Type == SearchResultType.Npc).Select(r => r.Name).ToList();

        Assert.Equal(["Mara", "Mara the Bold", "The Great Mara"], npcNames);
    }

    [Fact]
    public async Task SearchAsync_NeverMatchesDmNotesOrDescription()
    {
        var s = CreateServices();
        var campaign = await s.CampaignService.CreateAsync(new CampaignRequestDto { Name = "Notes Test Campaign" });
        await s.NpcService.CreateAsync(campaign.Id, new NpcRequestDto
        {
            Name = "Ordinary Villager",
            Status = NpcStatus.Alive,
            Description = "Secretly a Shadowmancer in disguise.",
            DmNotes = "Shadowmancer plot twist - reveal in session 12.",
        });

        var result = await s.SearchService.SearchAsync("Shadowmancer");

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAsync_IsolatesResults_BetweenTwoRealUsers()
    {
        var s = CreateServices();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        s.User.UserId = userA;
        var (campaignAId, _, _, npcAId, _) = await SeedFullCampaignAsync(s, npcName: "Duplicate Name");

        s.User.UserId = userB;
        var (campaignBId, _, _, npcBId, _) = await SeedFullCampaignAsync(s, npcName: "Duplicate Name");

        s.User.UserId = userA;
        var resultsAsA = await s.SearchService.SearchAsync("Duplicate Name");
        var npcResultsAsA = resultsAsA.Where(r => r.Type == SearchResultType.Npc).ToList();
        Assert.Single(npcResultsAsA);
        Assert.Equal(npcAId, npcResultsAsA[0].Id);
        Assert.Equal(campaignAId, npcResultsAsA[0].CampaignId);
        Assert.DoesNotContain(npcResultsAsA, r => r.Id == npcBId);

        s.User.UserId = userB;
        var resultsAsB = await s.SearchService.SearchAsync("Duplicate Name");
        var npcResultsAsB = resultsAsB.Where(r => r.Type == SearchResultType.Npc).ToList();
        Assert.Single(npcResultsAsB);
        Assert.Equal(npcBId, npcResultsAsB[0].Id);
        Assert.Equal(campaignBId, npcResultsAsB[0].CampaignId);
        Assert.DoesNotContain(npcResultsAsB, r => r.Id == npcAId);
    }
}
