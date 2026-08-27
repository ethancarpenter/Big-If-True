using System.Net;
using System.Net.Http.Json;
using CampaignApp.Application.DTOs;
using CampaignApp.Domain.Entities;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

public class NpcLocationRelationshipsControllerTests : IClassFixture<WebApiFactory>
{
    private readonly WebApiFactory _factory;
    private readonly HttpClient _client;

    public NpcLocationRelationshipsControllerTests(WebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid CampaignId, Guid LocationId, Guid NpcId)> CreateOwnedCampaignWithLocationAndNpcAsync()
    {
        var campaignResponse = await _client.PostAsJsonAsync("/api/campaigns", new CampaignRequestDto { Name = "The Northern Reach" });
        var campaign = await campaignResponse.Content.ReadFromJsonAsync<CampaignDto>();

        var cityResponse = await _client.PostAsJsonAsync($"/api/campaigns/{campaign!.Id}/cities", new CityRequestDto { Name = "Stonehaven" });
        var city = await cityResponse.Content.ReadFromJsonAsync<CityDto>();

        var locationResponse = await _client.PostAsJsonAsync($"/api/campaigns/{campaign.Id}/locations", new LocationRequestDto
        {
            CityId = city!.Id,
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
        });
        var location = await locationResponse.Content.ReadFromJsonAsync<LocationDto>(TestJsonOptions.Default);

        var npcResponse = await _client.PostAsJsonAsync($"/api/campaigns/{campaign.Id}/npcs", new NpcRequestDto
        {
            Name = "Eldrin Vale",
            Status = NpcStatus.Alive,
        });
        var npc = await npcResponse.Content.ReadFromJsonAsync<NpcDto>(TestJsonOptions.Default);

        return (campaign.Id, location!.Id, npc!.Id);
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
            var npc = new Npc
            {
                Id = Guid.NewGuid(),
                CampaignId = foreignCampaign.Id,
                Name = "Eldrin Vale",
                Status = NpcStatus.Alive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            var relationship = new NpcLocation
            {
                Id = Guid.NewGuid(),
                NpcId = npc.Id,
                LocationId = location.Id,
                RelationshipType = NpcLocationRelationshipType.WorksAt,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            context.Campaigns.Add(foreignCampaign);
            context.Cities.Add(city);
            context.Locations.Add(location);
            context.Npcs.Add(npc);
            context.NpcLocations.Add(relationship);
            await context.SaveChangesAsync();
            relationshipId = relationship.Id;
        }

        var response = await _client.GetAsync($"/api/npc-locations/{relationshipId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsConflict_ForDuplicateRelationship()
    {
        var (_, locationId, npcId) = await CreateOwnedCampaignWithLocationAndNpcAsync();
        await _client.PostAsJsonAsync($"/api/npcs/{npcId}/locations", new NpcLocationCreateRequestDto
        {
            LocationId = locationId,
            RelationshipType = NpcLocationRelationshipType.WorksAt,
        });

        var response = await _client.PostAsJsonAsync($"/api/npcs/{npcId}/locations", new NpcLocationCreateRequestDto
        {
            LocationId = locationId,
            RelationshipType = NpcLocationRelationshipType.Owns,
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenLocationBelongsToAnotherCampaign()
    {
        var (_, _, npcId) = await CreateOwnedCampaignWithLocationAndNpcAsync();
        var (_, otherLocationId, _) = await CreateOwnedCampaignWithLocationAndNpcAsync();

        var response = await _client.PostAsJsonAsync($"/api/npcs/{npcId}/locations", new NpcLocationCreateRequestDto
        {
            LocationId = otherLocationId,
            RelationshipType = NpcLocationRelationshipType.WorksAt,
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullCrudFlow_WorksEndToEnd_AndIsVisibleFromBothSides()
    {
        var (campaignId, locationId, npcId) = await CreateOwnedCampaignWithLocationAndNpcAsync();

        var createResponse = await _client.PostAsJsonAsync($"/api/npcs/{npcId}/locations", new NpcLocationCreateRequestDto
        {
            LocationId = locationId,
            RelationshipType = NpcLocationRelationshipType.WorksAt,
            IsPrimary = true,
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<NpcLocationDto>(TestJsonOptions.Default);
        Assert.NotNull(created);
        Assert.True(created!.IsPrimary);

        var fromNpcSide = await _client.GetFromJsonAsync<List<NpcLocationDto>>(
            $"/api/npcs/{npcId}/locations", TestJsonOptions.Default);
        Assert.Contains(fromNpcSide!, r => r.Id == created.Id);

        var fromLocationSide = await _client.GetFromJsonAsync<List<NpcLocationDto>>(
            $"/api/locations/{locationId}/npcs", TestJsonOptions.Default);
        Assert.Contains(fromLocationSide!, r => r.Id == created.Id && r.NpcName == "Eldrin Vale");

        var getResponse = await _client.GetAsync($"/api/npc-locations/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await _client.PutAsJsonAsync($"/api/npc-locations/{created.Id}", new NpcLocationUpdateRequestDto
        {
            RelationshipType = NpcLocationRelationshipType.Owns,
            IsPrimary = true,
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<NpcLocationDto>(TestJsonOptions.Default);
        Assert.Equal(NpcLocationRelationshipType.Owns, updated!.RelationshipType);

        var deleteResponse = await _client.DeleteAsync($"/api/npc-locations/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/npc-locations/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenRelationshipTypeMissing()
    {
        var (_, locationId, npcId) = await CreateOwnedCampaignWithLocationAndNpcAsync();

        var response = await _client.PostAsync(
            $"/api/npcs/{npcId}/locations",
            JsonContent.Create(new { locationId }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
