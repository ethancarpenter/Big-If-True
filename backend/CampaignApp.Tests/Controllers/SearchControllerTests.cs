using System.Net;
using System.Net.Http.Json;
using CampaignApp.Application.DTOs;
using CampaignApp.Domain.Entities;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

public class SearchControllerTests : IClassFixture<WebApiFactory>
{
    private readonly WebApiFactory _factory;
    private readonly HttpClient _client;

    public SearchControllerTests(WebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Search_ReturnsEmptyList_WhenQueryParameterIsMissing()
    {
        var response = await _client.GetAsync("/api/search");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<List<SearchResultDto>>(TestJsonOptions.Default);
        Assert.Empty(results!);
    }

    [Fact]
    public async Task Search_ReturnsMatchingResults_ForOwnedData()
    {
        var campaignResponse = await _client.PostAsJsonAsync("/api/campaigns", new CampaignRequestDto { Name = "Search Campaign" });
        var campaign = await campaignResponse.Content.ReadFromJsonAsync<CampaignDto>();

        await _client.PostAsJsonAsync($"/api/campaigns/{campaign!.Id}/npcs", new NpcRequestDto
        {
            Name = "Findable Npc",
            Status = NpcStatus.Alive,
        });

        var response = await _client.GetAsync("/api/search?q=Findable");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<List<SearchResultDto>>(TestJsonOptions.Default);
        var npcResult = Assert.Single(results!, r => r.Type == SearchResultType.Npc);
        Assert.Equal("Findable Npc", npcResult.Name);
        Assert.Equal(campaign.Id, npcResult.CampaignId);
        Assert.Equal("Search Campaign", npcResult.CampaignName);
    }

    [Fact]
    public async Task Search_SerializesTypeAsStringName_InRawJson()
    {
        var campaignResponse = await _client.PostAsJsonAsync("/api/campaigns", new CampaignRequestDto { Name = "Enum Check Campaign" });
        var campaign = await campaignResponse.Content.ReadFromJsonAsync<CampaignDto>();
        await _client.PostAsJsonAsync($"/api/campaigns/{campaign!.Id}/quests", new QuestRequestDto
        {
            Name = "Enum Check Quest",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });

        var response = await _client.GetAsync($"/api/search?q={Uri.EscapeDataString("Enum Check Quest")}");
        var rawJson = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"type\":\"Quest\"", rawJson);
    }

    [Fact]
    public async Task Search_NeverReturnsAnotherUsersEntities()
    {
        var otherUsersCampaign = new Campaign
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(), // deliberately not _factory.TestUserId
            Name = "Not Yours Campaign",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        var otherUsersNpc = new Npc
        {
            Id = Guid.NewGuid(),
            CampaignId = otherUsersCampaign.Id,
            Name = "Not Yours Npc",
            Status = NpcStatus.Alive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Campaigns.Add(otherUsersCampaign);
            context.Npcs.Add(otherUsersNpc);
            await context.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/search?q={Uri.EscapeDataString("Not Yours")}");

        var results = await response.Content.ReadFromJsonAsync<List<SearchResultDto>>(TestJsonOptions.Default);
        Assert.Empty(results!);
    }
}
