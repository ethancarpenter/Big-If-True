using System.Net;
using System.Net.Http.Json;
using CampaignApp.Application.DTOs;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

public class CitiesControllerTests : IClassFixture<WebApiFactory>
{
    private readonly WebApiFactory _factory;
    private readonly HttpClient _client;

    public CitiesControllerTests(WebApiFactory factory)
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
    public async Task CreateAndGetAll_ReturnsNotFound_ForCampaignOwnedByAnotherUser()
    {
        Guid foreignCampaignId;
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
            context.Campaigns.Add(foreignCampaign);
            await context.SaveChangesAsync();
            foreignCampaignId = foreignCampaign.Id;
        }

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/campaigns/{foreignCampaignId}/cities",
            new CityRequestDto { Name = "Stonehaven" });
        Assert.Equal(HttpStatusCode.NotFound, createResponse.StatusCode);

        var listResponse = await _client.GetAsync($"/api/campaigns/{foreignCampaignId}/cities");
        Assert.Equal(HttpStatusCode.NotFound, listResponse.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForCityWhoseCampaignOwnedByAnotherUser()
    {
        Guid cityId;
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
            context.Campaigns.Add(foreignCampaign);
            context.Cities.Add(city);
            await context.SaveChangesAsync();
            cityId = city.Id;
        }

        var response = await _client.GetAsync($"/api/cities/{cityId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullCrudFlow_WorksEndToEnd()
    {
        var campaignId = await CreateOwnedCampaignAsync();

        var createResponse = await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/cities", new CityRequestDto
        {
            Name = "Stonehaven",
            Population = "~8,000",
            Government = "Merchant Council",
            Region = "Northern Reach",
            Alignment = "Neutral",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CityDto>();
        Assert.NotNull(created);
        Assert.Equal(campaignId, created!.CampaignId);

        var listResponse = await _client.GetFromJsonAsync<List<CityDto>>($"/api/campaigns/{campaignId}/cities");
        Assert.Contains(listResponse!, c => c.Id == created.Id);

        var getResponse = await _client.GetAsync($"/api/cities/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await _client.PutAsJsonAsync($"/api/cities/{created.Id}", new CityRequestDto
        {
            Name = "Stonehaven (Renamed)",
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<CityDto>();
        Assert.Equal("Stonehaven (Renamed)", updated!.Name);

        var deleteResponse = await _client.DeleteAsync($"/api/cities/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/cities/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenNameMissing()
    {
        var campaignId = await CreateOwnedCampaignAsync();

        var response = await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/cities", new CityRequestDto { Name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
