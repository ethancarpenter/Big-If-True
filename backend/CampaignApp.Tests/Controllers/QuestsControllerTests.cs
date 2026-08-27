using System.Net;
using System.Net.Http.Json;
using CampaignApp.Application.DTOs;
using CampaignApp.Domain.Entities;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

public class QuestsControllerTests : IClassFixture<WebApiFactory>
{
    private readonly WebApiFactory _factory;
    private readonly HttpClient _client;

    public QuestsControllerTests(WebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<Guid> CreateOwnedCampaignAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/campaigns", new CampaignRequestDto { Name = "The Northern Reach" });
        var campaign = await response.Content.ReadFromJsonAsync<CampaignDto>();
        return campaign!.Id;
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForQuestWhoseCampaignOwnedByAnotherUser()
    {
        Guid questId;
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
            var quest = new Quest
            {
                Id = Guid.NewGuid(),
                CampaignId = foreignCampaign.Id,
                Name = "Missing Caravan",
                Status = QuestStatus.Planned,
                QuestType = QuestType.MainQuest,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            context.Campaigns.Add(foreignCampaign);
            context.Quests.Add(quest);
            await context.SaveChangesAsync();
            questId = quest.Id;
        }

        var response = await _client.GetAsync($"/api/quests/{questId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullCrudFlow_WorksEndToEnd_IncludingEmbeddedObjectives()
    {
        var campaignId = await CreateOwnedCampaignAsync();

        var createResponse = await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/quests", new QuestRequestDto
        {
            Name = "Missing Caravan",
            Description = "A merchant caravan has gone missing.",
            Status = QuestStatus.Active,
            QuestType = QuestType.MainQuest,
            RecommendedLevelMin = 3,
            RecommendedLevelMax = 5,
            DmNotes = "Raided by the Black Hand.",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<QuestDto>(TestJsonOptions.Default);
        Assert.NotNull(created);
        Assert.Equal(campaignId, created!.CampaignId);
        Assert.Equal(QuestStatus.Active, created.Status);
        Assert.Equal(QuestType.MainQuest, created.QuestType);
        Assert.Empty(created.Objectives);

        var listResponse = await _client.GetFromJsonAsync<List<QuestDto>>(
            $"/api/campaigns/{campaignId}/quests", TestJsonOptions.Default);
        Assert.Contains(listResponse!, q => q.Id == created.Id);

        var getResponse = await _client.GetAsync($"/api/quests/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await _client.PutAsJsonAsync($"/api/quests/{created.Id}", new QuestRequestDto
        {
            Name = "Missing Caravan (Renamed)",
            Status = QuestStatus.Completed,
            QuestType = QuestType.MainQuest,
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<QuestDto>(TestJsonOptions.Default);
        Assert.Equal("Missing Caravan (Renamed)", updated!.Name);
        Assert.Equal(QuestStatus.Completed, updated.Status);

        var deleteResponse = await _client.DeleteAsync($"/api/quests/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/quests/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task Create_SucceedsWithOnlyRequiredFields()
    {
        var campaignId = await CreateOwnedCampaignAsync();

        var response = await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/quests", new QuestRequestDto
        {
            Name = "Unnamed Quest",
            Status = QuestStatus.Planned,
            QuestType = QuestType.Other,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenNameMissing()
    {
        var campaignId = await CreateOwnedCampaignAsync();

        var response = await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/quests", new QuestRequestDto
        {
            Name = "",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenStatusOrQuestTypeMissing()
    {
        var campaignId = await CreateOwnedCampaignAsync();

        var response = await _client.PostAsync(
            $"/api/campaigns/{campaignId}/quests",
            JsonContent.Create(new { name = "Missing Caravan" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenRecommendedLevelMinExceedsMax()
    {
        var campaignId = await CreateOwnedCampaignAsync();

        var response = await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/quests", new QuestRequestDto
        {
            Name = "Missing Caravan",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
            RecommendedLevelMin = 10,
            RecommendedLevelMax = 5,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_Succeeds_WhenOnlyOneRecommendedLevelBoundGiven()
    {
        var campaignId = await CreateOwnedCampaignAsync();

        var minOnlyResponse = await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/quests", new QuestRequestDto
        {
            Name = "Min Only",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
            RecommendedLevelMin = 3,
        });
        Assert.Equal(HttpStatusCode.Created, minOnlyResponse.StatusCode);

        var maxOnlyResponse = await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/quests", new QuestRequestDto
        {
            Name = "Max Only",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
            RecommendedLevelMax = 5,
        });
        Assert.Equal(HttpStatusCode.Created, maxOnlyResponse.StatusCode);
    }
}
