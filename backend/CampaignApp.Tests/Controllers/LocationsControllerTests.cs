using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CampaignApp.Application.DTOs;
using CampaignApp.Domain.Entities;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

public class LocationsControllerTests : IClassFixture<WebApiFactory>
{
    // The server (Program.cs) serializes enums as strings via a
    // JsonStringEnumConverter registered on MVC's JSON options; the test's
    // HttpClient uses System.Text.Json defaults for its own
    // ReadFromJsonAsync/GetFromJsonAsync calls, which doesn't know about
    // that converter, so LocationDto.Type needs it passed explicitly here.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly WebApiFactory _factory;
    private readonly HttpClient _client;

    public LocationsControllerTests(WebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid CampaignId, Guid CityId)> CreateOwnedCampaignWithCityAsync()
    {
        var campaignResponse = await _client.PostAsJsonAsync("/api/campaigns", new CampaignRequestDto { Name = "The Northern Reach" });
        var campaign = await campaignResponse.Content.ReadFromJsonAsync<CampaignDto>();

        var cityResponse = await _client.PostAsJsonAsync($"/api/campaigns/{campaign!.Id}/cities", new CityRequestDto { Name = "Stonehaven" });
        var city = await cityResponse.Content.ReadFromJsonAsync<CityDto>();

        return (campaign.Id, city!.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForLocationWhoseCampaignOwnedByAnotherUser()
    {
        Guid locationId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var foreignCampaign = new Campaign
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
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
            context.Campaigns.Add(foreignCampaign);
            context.Cities.Add(city);
            context.Locations.Add(location);
            await context.SaveChangesAsync();
            locationId = location.Id;
        }

        var response = await _client.GetAsync($"/api/locations/{locationId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenCityBelongsToAnotherCampaign()
    {
        var (campaignId, _) = await CreateOwnedCampaignWithCityAsync();
        var (_, otherCityId) = await CreateOwnedCampaignWithCityAsync();

        var response = await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/locations", new LocationRequestDto
        {
            CityId = otherCityId,
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullCrudFlow_WorksEndToEnd()
    {
        var (campaignId, cityId) = await CreateOwnedCampaignWithCityAsync();

        var createResponse = await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/locations", new LocationRequestDto
        {
            CityId = cityId,
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
            Description = "A bustling inn known for its hearty meals.",
            DmNotes = "Eldrin secretly works with the Black Hand.",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<LocationDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(campaignId, created!.CampaignId);
        Assert.Equal(cityId, created.CityId);
        Assert.Equal("Stonehaven", created.CityName);
        Assert.Equal(LocationType.Tavern, created.Type);

        var listResponse = await _client.GetFromJsonAsync<List<LocationDto>>(
            $"/api/campaigns/{campaignId}/locations", JsonOptions);
        Assert.Contains(listResponse!, l => l.Id == created.Id);

        var filteredResponse = await _client.GetFromJsonAsync<List<LocationDto>>(
            $"/api/campaigns/{campaignId}/locations?cityId={cityId}", JsonOptions);
        Assert.Contains(filteredResponse!, l => l.Id == created.Id);

        var getResponse = await _client.GetAsync($"/api/locations/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await _client.PutAsJsonAsync($"/api/locations/{created.Id}", new LocationRequestDto
        {
            CityId = cityId,
            Name = "Ironforge Inn (Renamed)",
            Type = LocationType.Landmark,
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<LocationDto>(JsonOptions);
        Assert.Equal("Ironforge Inn (Renamed)", updated!.Name);
        Assert.Equal(LocationType.Landmark, updated.Type);

        var deleteResponse = await _client.DeleteAsync($"/api/locations/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/locations/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenNameMissing()
    {
        var (campaignId, cityId) = await CreateOwnedCampaignWithCityAsync();

        var response = await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/locations", new LocationRequestDto
        {
            CityId = cityId,
            Name = "",
            Type = LocationType.Tavern,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenTypeMissing()
    {
        var (campaignId, cityId) = await CreateOwnedCampaignWithCityAsync();

        var response = await _client.PostAsync(
            $"/api/campaigns/{campaignId}/locations",
            JsonContent.Create(new { cityId, name = "Ironforge Inn" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
