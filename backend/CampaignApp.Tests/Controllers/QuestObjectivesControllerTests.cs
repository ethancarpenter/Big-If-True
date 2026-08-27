using System.Net;
using System.Net.Http.Json;
using CampaignApp.Application.DTOs;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Tests.Controllers;

public class QuestObjectivesControllerTests : IClassFixture<WebApiFactory>
{
    private readonly HttpClient _client;

    public QuestObjectivesControllerTests(WebApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateOwnedQuestAsync()
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
        return quest!.Id;
    }

    [Fact]
    public async Task FullLifecycle_AddToggleEditDeleteReorder_WorksEndToEnd()
    {
        var questId = await CreateOwnedQuestAsync();

        var firstResponse = await _client.PostAsJsonAsync($"/api/quests/{questId}/objectives", new QuestObjectiveCreateRequestDto { Description = "Talk to Eldrin Vale" });
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        var first = await firstResponse.Content.ReadFromJsonAsync<QuestObjectiveDto>();
        Assert.Equal(0, first!.SortOrder);
        Assert.False(first.IsCompleted);

        var secondResponse = await _client.PostAsJsonAsync($"/api/quests/{questId}/objectives", new QuestObjectiveCreateRequestDto { Description = "Travel to the last known location" });
        var second = await secondResponse.Content.ReadFromJsonAsync<QuestObjectiveDto>();
        Assert.Equal(1, second!.SortOrder);

        var listResponse = await _client.GetFromJsonAsync<List<QuestObjectiveDto>>($"/api/quests/{questId}/objectives");
        Assert.Equal(2, listResponse!.Count);

        var toggleResponse = await _client.PutAsJsonAsync($"/api/quests/{questId}/objectives/{first.Id}", new QuestObjectiveUpdateRequestDto
        {
            Description = first.Description,
            IsCompleted = true,
        });
        Assert.Equal(HttpStatusCode.OK, toggleResponse.StatusCode);
        var toggled = await toggleResponse.Content.ReadFromJsonAsync<QuestObjectiveDto>();
        Assert.True(toggled!.IsCompleted);

        var reorderResponse = await _client.PutAsJsonAsync($"/api/quests/{questId}/objectives/reorder", new QuestObjectiveReorderRequestDto
        {
            ObjectiveIds = [second.Id, first.Id],
        });
        Assert.Equal(HttpStatusCode.OK, reorderResponse.StatusCode);
        var reordered = await reorderResponse.Content.ReadFromJsonAsync<List<QuestObjectiveDto>>();
        Assert.Equal(second.Id, reordered![0].Id);
        Assert.Equal(first.Id, reordered[1].Id);

        var deleteResponse = await _client.DeleteAsync($"/api/quests/{questId}/objectives/{first.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var finalList = await _client.GetFromJsonAsync<List<QuestObjectiveDto>>($"/api/quests/{questId}/objectives");
        var remaining = Assert.Single(finalList!);
        Assert.Equal(second.Id, remaining.Id);
        Assert.Equal(0, remaining.SortOrder); // compacted after delete
    }

    [Fact]
    public async Task GetAll_ReturnsNotFound_WhenQuestDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/quests/{Guid.NewGuid()}/objectives");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenDescriptionMissing()
    {
        var questId = await CreateOwnedQuestAsync();

        var response = await _client.PostAsync(
            $"/api/quests/{questId}/objectives",
            JsonContent.Create(new { }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Reorder_ReturnsNotFound_WhenIdSetDoesNotMatch()
    {
        var questId = await CreateOwnedQuestAsync();
        await _client.PostAsJsonAsync($"/api/quests/{questId}/objectives", new QuestObjectiveCreateRequestDto { Description = "Talk to Eldrin Vale" });

        var response = await _client.PutAsJsonAsync($"/api/quests/{questId}/objectives/reorder", new QuestObjectiveReorderRequestDto
        {
            ObjectiveIds = [Guid.NewGuid()],
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
