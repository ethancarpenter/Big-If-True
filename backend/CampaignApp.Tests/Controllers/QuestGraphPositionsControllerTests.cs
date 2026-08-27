using System.Net;
using System.Net.Http.Json;
using CampaignApp.Application.DTOs;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Tests.Controllers;

public class QuestGraphPositionsControllerTests : IClassFixture<WebApiFactory>
{
    private readonly HttpClient _client;

    public QuestGraphPositionsControllerTests(WebApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(Guid CampaignId, Guid QuestId)> CreateOwnedCampaignWithQuestAsync()
    {
        var campaignResponse = await _client.PostAsJsonAsync("/api/campaigns", new CampaignRequestDto { Name = "The Northern Reach" });
        var campaign = await campaignResponse.Content.ReadFromJsonAsync<CampaignDto>();

        var questResponse = await _client.PostAsJsonAsync($"/api/campaigns/{campaign!.Id}/quests", new QuestRequestDto
        {
            Name = "Missing Caravan",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });
        var quest = await questResponse.Content.ReadFromJsonAsync<QuestDto>(TestJsonOptions.Default);

        return (campaign.Id, quest!.Id);
    }

    [Fact]
    public async Task Upsert_CreatesThenUpdates_AndIsVisibleInCampaignList()
    {
        var (campaignId, questId) = await CreateOwnedCampaignWithQuestAsync();

        var createResponse = await _client.PutAsJsonAsync(
            $"/api/quests/{questId}/graph-position", new QuestGraphPositionUpdateRequestDto { X = 10, Y = 20 });
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<QuestGraphPositionDto>(TestJsonOptions.Default);
        Assert.Equal(10, created!.X);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/quests/{questId}/graph-position", new QuestGraphPositionUpdateRequestDto { X = 30, Y = 40 });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<QuestGraphPositionDto>(TestJsonOptions.Default);
        Assert.Equal(30, updated!.X);
        Assert.Equal(40, updated.Y);

        var listResponse = await _client.GetFromJsonAsync<List<QuestGraphPositionDto>>(
            $"/api/campaigns/{campaignId}/quest-graph-positions", TestJsonOptions.Default);
        var single = Assert.Single(listResponse!, p => p.QuestId == questId);
        Assert.Equal(30, single.X);
    }

    [Fact]
    public async Task Upsert_ReturnsNotFound_ForUnknownQuest()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/quests/{Guid.NewGuid()}/graph-position", new QuestGraphPositionUpdateRequestDto { X = 1, Y = 1 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
