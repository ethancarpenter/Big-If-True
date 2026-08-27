using System.Net;
using System.Net.Http.Json;
using CampaignApp.Application.DTOs;
using CampaignApp.Domain.Entities;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

public class QuestLocationRelationshipsControllerTests : IClassFixture<WebApiFactory>
{
    private readonly WebApiFactory _factory;
    private readonly HttpClient _client;

    public QuestLocationRelationshipsControllerTests(WebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid CampaignId, Guid QuestId, Guid LocationId)> CreateOwnedCampaignWithQuestAndLocationAsync()
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

        var cityResponse = await _client.PostAsJsonAsync($"/api/campaigns/{campaign.Id}/cities", new CityRequestDto { Name = "Stonehaven" });
        var city = await cityResponse.Content.ReadFromJsonAsync<CityDto>();

        var locationResponse = await _client.PostAsJsonAsync($"/api/campaigns/{campaign.Id}/locations", new LocationRequestDto
        {
            CityId = city!.Id,
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
        });
        var location = await locationResponse.Content.ReadFromJsonAsync<LocationDto>(TestJsonOptions.Default);

        return (campaign.Id, quest!.Id, location!.Id);
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
            var city = new City
            {
                Id = Guid.NewGuid(),
                CampaignId = foreignCampaign.Id,
                Name = "Stonehaven",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            var location = new Location
            {
                Id = Guid.NewGuid(),
                CampaignId = foreignCampaign.Id,
                CityId = city.Id,
                Name = "Ironforge Inn",
                Type = LocationType.Tavern,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            var relationship = new QuestLocation
            {
                Id = Guid.NewGuid(),
                QuestId = quest.Id,
                LocationId = location.Id,
                Role = QuestLocationRole.StartingLocation,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            context.Campaigns.Add(foreignCampaign);
            context.Quests.Add(quest);
            context.Cities.Add(city);
            context.Locations.Add(location);
            context.QuestLocations.Add(relationship);
            await context.SaveChangesAsync();
            relationshipId = relationship.Id;
        }

        var response = await _client.GetAsync($"/api/quest-locations/{relationshipId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_ForDuplicateRelationship()
    {
        var (_, questId, locationId) = await CreateOwnedCampaignWithQuestAndLocationAsync();
        await _client.PostAsJsonAsync($"/api/quests/{questId}/locations", new QuestLocationCreateRequestDto
        {
            LocationId = locationId,
            Role = QuestLocationRole.StartingLocation,
        });

        var response = await _client.PostAsJsonAsync($"/api/quests/{questId}/locations", new QuestLocationCreateRequestDto
        {
            LocationId = locationId,
            Role = QuestLocationRole.EncounterLocation,
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenLocationBelongsToAnotherCampaign()
    {
        var (_, questId, _) = await CreateOwnedCampaignWithQuestAndLocationAsync();
        var (_, _, otherLocationId) = await CreateOwnedCampaignWithQuestAndLocationAsync();

        var response = await _client.PostAsJsonAsync($"/api/quests/{questId}/locations", new QuestLocationCreateRequestDto
        {
            LocationId = otherLocationId,
            Role = QuestLocationRole.StartingLocation,
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullCrudFlow_WorksEndToEnd_AndIsVisibleFromBothSides()
    {
        var (campaignId, questId, locationId) = await CreateOwnedCampaignWithQuestAndLocationAsync();

        var createResponse = await _client.PostAsJsonAsync($"/api/quests/{questId}/locations", new QuestLocationCreateRequestDto
        {
            LocationId = locationId,
            Role = QuestLocationRole.StartingLocation,
            Notes = "The party first hears rumors here.",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<QuestLocationDto>(TestJsonOptions.Default);
        Assert.NotNull(created);
        Assert.Equal(QuestLocationRole.StartingLocation, created!.Role);

        var fromQuestSide = await _client.GetFromJsonAsync<List<QuestLocationDto>>(
            $"/api/quests/{questId}/locations", TestJsonOptions.Default);
        Assert.Contains(fromQuestSide!, r => r.Id == created.Id);

        var fromLocationSide = await _client.GetFromJsonAsync<List<QuestLocationDto>>(
            $"/api/locations/{locationId}/quests", TestJsonOptions.Default);
        Assert.Contains(fromLocationSide!, r => r.Id == created.Id && r.QuestName == "Missing Caravan");

        var getResponse = await _client.GetAsync($"/api/quest-locations/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await _client.PutAsJsonAsync($"/api/quest-locations/{created.Id}", new QuestLocationUpdateRequestDto
        {
            Role = QuestLocationRole.EncounterLocation,
            Notes = "An ambush occurs here.",
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<QuestLocationDto>(TestJsonOptions.Default);
        Assert.Equal(QuestLocationRole.EncounterLocation, updated!.Role);
        Assert.Equal("An ambush occurs here.", updated.Notes);

        var deleteResponse = await _client.DeleteAsync($"/api/quest-locations/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/quest-locations/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenRoleMissing()
    {
        var (_, questId, locationId) = await CreateOwnedCampaignWithQuestAndLocationAsync();

        var response = await _client.PostAsync(
            $"/api/quests/{questId}/locations",
            JsonContent.Create(new { locationId }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_SerializesRole_AsStringName_InRawJson()
    {
        var (_, questId, locationId) = await CreateOwnedCampaignWithQuestAndLocationAsync();

        var response = await _client.PostAsJsonAsync($"/api/quests/{questId}/locations", new QuestLocationCreateRequestDto
        {
            LocationId = locationId,
            Role = QuestLocationRole.Destination,
        });

        var rawJson = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"role\":\"Destination\"", rawJson);
    }
}
