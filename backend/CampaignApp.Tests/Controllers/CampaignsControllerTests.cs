using System.Net;
using System.Net.Http.Json;
using CampaignApp.Application.DTOs;
using CampaignApp.Domain.Entities;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

public class CampaignsControllerTests : IClassFixture<CampaignsApiFactory>
{
    private readonly CampaignsApiFactory _factory;
    private readonly HttpClient _client;

    public CampaignsControllerTests(CampaignsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForCampaignOwnedByAnotherUser()
    {
        var otherUsersCampaign = new Campaign
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(), // deliberately not _factory.TestUserId
            Name = "Not Yours",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Campaigns.Add(otherUsersCampaign);
            await context.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/api/campaigns/{otherUsersCampaign.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullCrudFlow_WorksEndToEnd()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/campaigns", new CampaignRequestDto
        {
            Name = "The Northern Reach",
            Description = "A frontier campaign.",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<CampaignDto>();
        Assert.NotNull(created);

        var listResponse = await _client.GetFromJsonAsync<List<CampaignDto>>("/api/campaigns");
        Assert.Contains(listResponse!, c => c.Id == created!.Id);

        var getResponse = await _client.GetAsync($"/api/campaigns/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await _client.PutAsJsonAsync($"/api/campaigns/{created.Id}", new CampaignRequestDto
        {
            Name = "The Northern Reach (Renamed)",
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<CampaignDto>();
        Assert.Equal("The Northern Reach (Renamed)", updated!.Name);

        var deleteResponse = await _client.DeleteAsync($"/api/campaigns/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/campaigns/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenNameMissing()
    {
        var response = await _client.PostAsJsonAsync("/api/campaigns", new CampaignRequestDto { Name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
