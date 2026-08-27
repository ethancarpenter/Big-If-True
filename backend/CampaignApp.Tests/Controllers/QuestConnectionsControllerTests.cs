using System.Net;
using System.Net.Http.Json;
using CampaignApp.Application.DTOs;
using CampaignApp.Domain.Entities;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

public class QuestConnectionsControllerTests : IClassFixture<WebApiFactory>
{
    private readonly WebApiFactory _factory;
    private readonly HttpClient _client;

    public QuestConnectionsControllerTests(WebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid CampaignId, Guid A, Guid B, Guid C)> CreateOwnedCampaignWithThreeQuestsAsync()
    {
        var campaignResponse = await _client.PostAsJsonAsync("/api/campaigns", new CampaignRequestDto { Name = "The Northern Reach" });
        var campaign = await campaignResponse.Content.ReadFromJsonAsync<CampaignDto>();

        async Task<Guid> CreateQuestAsync(string name)
        {
            var response = await _client.PostAsJsonAsync($"/api/campaigns/{campaign!.Id}/quests", new QuestRequestDto
            {
                Name = name,
                Status = QuestStatus.Planned,
                QuestType = QuestType.MainQuest,
            });
            var quest = await response.Content.ReadFromJsonAsync<QuestDto>(TestJsonOptions.Default);
            return quest!.Id;
        }

        var a = await CreateQuestAsync("Quest A");
        var b = await CreateQuestAsync("Quest B");
        var c = await CreateQuestAsync("Quest C");

        return (campaign!.Id, a, b, c);
    }

    private static QuestConnectionCreateRequestDto Request(Guid source, Guid target, QuestConnectionType type) =>
        new() { SourceQuestId = source, TargetQuestId = target, ConnectionType = type };

    [Fact]
    public async Task Create_ReturnsBadRequest_ForSelfLink()
    {
        var (campaignId, a, _, _) = await CreateOwnedCampaignWithThreeQuestsAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/campaigns/{campaignId}/quest-connections", Request(a, a, QuestConnectionType.Related));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflictWithDuplicateReason_ForDuplicatePair()
    {
        var (campaignId, a, b, _) = await CreateOwnedCampaignWithThreeQuestsAsync();
        await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/quest-connections", Request(a, b, QuestConnectionType.Unlocks));

        var response = await _client.PostAsJsonAsync(
            $"/api/campaigns/{campaignId}/quest-connections", Request(a, b, QuestConnectionType.Related));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"reason\":\"Duplicate\"", body);
    }

    [Fact]
    public async Task Create_ReturnsConflictWithCycleReason_ForClosedProgressionLoop()
    {
        var (campaignId, a, b, c) = await CreateOwnedCampaignWithThreeQuestsAsync();
        await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/quest-connections", Request(a, b, QuestConnectionType.Unlocks));
        await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/quest-connections", Request(b, c, QuestConnectionType.Unlocks));

        var response = await _client.PostAsJsonAsync(
            $"/api/campaigns/{campaignId}/quest-connections", Request(c, a, QuestConnectionType.Unlocks));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"reason\":\"Cycle\"", body);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenTargetQuestBelongsToAnotherCampaign()
    {
        var (campaignId, a, _, _) = await CreateOwnedCampaignWithThreeQuestsAsync();
        var (_, otherA, _, _) = await CreateOwnedCampaignWithThreeQuestsAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/campaigns/{campaignId}/quest-connections", Request(a, otherA, QuestConnectionType.Unlocks));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForConnectionWhoseCampaignOwnedByAnotherUser()
    {
        Guid connectionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var foreignCampaign = new Campaign
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(), // not _factory.TestUserId
                Name = "Not Yours",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            var questA = new Quest
            {
                Id = Guid.NewGuid(),
                CampaignId = foreignCampaign.Id,
                Name = "Quest A",
                Status = QuestStatus.Planned,
                QuestType = QuestType.MainQuest,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            var questB = new Quest
            {
                Id = Guid.NewGuid(),
                CampaignId = foreignCampaign.Id,
                Name = "Quest B",
                Status = QuestStatus.Planned,
                QuestType = QuestType.MainQuest,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            var connection = new QuestConnection
            {
                Id = Guid.NewGuid(),
                SourceQuestId = questA.Id,
                TargetQuestId = questB.Id,
                ConnectionType = QuestConnectionType.Unlocks,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            context.Campaigns.Add(foreignCampaign);
            context.Quests.AddRange(questA, questB);
            context.QuestConnections.Add(connection);
            await context.SaveChangesAsync();
            connectionId = connection.Id;
        }

        var response = await _client.GetAsync($"/api/quest-connections/{connectionId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullCrudFlow_WorksEndToEnd()
    {
        var (campaignId, a, b, _) = await CreateOwnedCampaignWithThreeQuestsAsync();

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/campaigns/{campaignId}/quest-connections", Request(a, b, QuestConnectionType.Unlocks));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<QuestConnectionDto>(TestJsonOptions.Default);
        Assert.NotNull(created);
        Assert.Equal(QuestConnectionType.Unlocks, created!.ConnectionType);

        var listResponse = await _client.GetFromJsonAsync<List<QuestConnectionDto>>(
            $"/api/campaigns/{campaignId}/quest-connections", TestJsonOptions.Default);
        Assert.Contains(listResponse!, c => c.Id == created.Id);

        var getResponse = await _client.GetAsync($"/api/quest-connections/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/quest-connections/{created.Id}",
            new QuestConnectionUpdateRequestDto { ConnectionType = QuestConnectionType.Requires });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<QuestConnectionDto>(TestJsonOptions.Default);
        Assert.Equal(QuestConnectionType.Requires, updated!.ConnectionType);

        var deleteResponse = await _client.DeleteAsync($"/api/quest-connections/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/quest-connections/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task Create_SerializesConnectionType_AsStringName_InRawJson()
    {
        var (campaignId, a, b, _) = await CreateOwnedCampaignWithThreeQuestsAsync();

        var response = await _client.PostAsJsonAsync(
            $"/api/campaigns/{campaignId}/quest-connections", Request(a, b, QuestConnectionType.AlternativePath));

        var rawJson = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"connectionType\":\"AlternativePath\"", rawJson);
    }
}
