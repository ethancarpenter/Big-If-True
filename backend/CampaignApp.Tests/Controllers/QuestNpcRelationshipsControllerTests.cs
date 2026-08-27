using System.Net;
using System.Net.Http.Json;
using CampaignApp.Application.DTOs;
using CampaignApp.Domain.Entities;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

public class QuestNpcRelationshipsControllerTests : IClassFixture<WebApiFactory>
{
    private readonly WebApiFactory _factory;
    private readonly HttpClient _client;

    public QuestNpcRelationshipsControllerTests(WebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid CampaignId, Guid QuestId, Guid NpcId)> CreateOwnedCampaignWithQuestAndNpcAsync()
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

        var npcResponse = await _client.PostAsJsonAsync($"/api/campaigns/{campaign.Id}/npcs", new NpcRequestDto
        {
            Name = "Eldrin Vale",
            Status = NpcStatus.Alive,
        });
        var npc = await npcResponse.Content.ReadFromJsonAsync<NpcDto>(TestJsonOptions.Default);

        return (campaign.Id, quest!.Id, npc!.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForRelationshipWhoseCampaignOwnedByAnotherUser()
    {
        Guid relationshipId;
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
            var npc = new Npc
            {
                Id = Guid.NewGuid(),
                CampaignId = foreignCampaign.Id,
                Name = "Eldrin Vale",
                Status = NpcStatus.Alive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            var relationship = new QuestNpc
            {
                Id = Guid.NewGuid(),
                QuestId = quest.Id,
                NpcId = npc.Id,
                Role = QuestNpcRole.QuestGiver,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            context.Campaigns.Add(foreignCampaign);
            context.Quests.Add(quest);
            context.Npcs.Add(npc);
            context.QuestNpcs.Add(relationship);
            await context.SaveChangesAsync();
            relationshipId = relationship.Id;
        }

        var response = await _client.GetAsync($"/api/quest-npcs/{relationshipId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_ForDuplicateRelationship()
    {
        var (_, questId, npcId) = await CreateOwnedCampaignWithQuestAndNpcAsync();
        await _client.PostAsJsonAsync($"/api/quests/{questId}/npcs", new QuestNpcCreateRequestDto
        {
            NpcId = npcId,
            Role = QuestNpcRole.QuestGiver,
        });

        var response = await _client.PostAsJsonAsync($"/api/quests/{questId}/npcs", new QuestNpcCreateRequestDto
        {
            NpcId = npcId,
            Role = QuestNpcRole.Enemy,
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenNpcBelongsToAnotherCampaign()
    {
        var (_, questId, _) = await CreateOwnedCampaignWithQuestAndNpcAsync();
        var (_, _, otherNpcId) = await CreateOwnedCampaignWithQuestAndNpcAsync();

        var response = await _client.PostAsJsonAsync($"/api/quests/{questId}/npcs", new QuestNpcCreateRequestDto
        {
            NpcId = otherNpcId,
            Role = QuestNpcRole.QuestGiver,
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullCrudFlow_WorksEndToEnd_AndIsVisibleFromBothSides()
    {
        var (campaignId, questId, npcId) = await CreateOwnedCampaignWithQuestAndNpcAsync();

        var createResponse = await _client.PostAsJsonAsync($"/api/quests/{questId}/npcs", new QuestNpcCreateRequestDto
        {
            NpcId = npcId,
            Role = QuestNpcRole.QuestGiver,
            Notes = "Gives the quest at the inn.",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<QuestNpcDto>(TestJsonOptions.Default);
        Assert.NotNull(created);
        Assert.Equal(QuestNpcRole.QuestGiver, created!.Role);

        var fromQuestSide = await _client.GetFromJsonAsync<List<QuestNpcDto>>(
            $"/api/quests/{questId}/npcs", TestJsonOptions.Default);
        Assert.Contains(fromQuestSide!, r => r.Id == created.Id);

        var fromNpcSide = await _client.GetFromJsonAsync<List<QuestNpcDto>>(
            $"/api/npcs/{npcId}/quests", TestJsonOptions.Default);
        Assert.Contains(fromNpcSide!, r => r.Id == created.Id && r.QuestName == "Missing Caravan");

        var getResponse = await _client.GetAsync($"/api/quest-npcs/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await _client.PutAsJsonAsync($"/api/quest-npcs/{created.Id}", new QuestNpcUpdateRequestDto
        {
            Role = QuestNpcRole.Enemy,
            Notes = "Turned after the betrayal.",
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<QuestNpcDto>(TestJsonOptions.Default);
        Assert.Equal(QuestNpcRole.Enemy, updated!.Role);
        Assert.Equal("Turned after the betrayal.", updated.Notes);

        var deleteResponse = await _client.DeleteAsync($"/api/quest-npcs/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/quest-npcs/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenRoleMissing()
    {
        var (_, questId, npcId) = await CreateOwnedCampaignWithQuestAndNpcAsync();

        var response = await _client.PostAsync(
            $"/api/quests/{questId}/npcs",
            JsonContent.Create(new { npcId }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_SerializesRole_AsStringName_InRawJson()
    {
        var (_, questId, npcId) = await CreateOwnedCampaignWithQuestAndNpcAsync();

        var response = await _client.PostAsJsonAsync($"/api/quests/{questId}/npcs", new QuestNpcCreateRequestDto
        {
            NpcId = npcId,
            Role = QuestNpcRole.Witness,
        });

        var rawJson = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"role\":\"Witness\"", rawJson);
    }
}
