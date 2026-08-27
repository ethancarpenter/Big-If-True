using System.Net;
using System.Net.Http.Json;
using CampaignApp.Application.DTOs;
using CampaignApp.Domain.Entities;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace CampaignApp.Tests.Controllers;

public class NpcsControllerTests : IClassFixture<WebApiFactory>
{
    private readonly WebApiFactory _factory;
    private readonly HttpClient _client;

    public NpcsControllerTests(WebApiFactory factory)
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
    public async Task GetById_ReturnsNotFound_ForNpcWhoseCampaignOwnedByAnotherUser()
    {
        Guid npcId;
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
            var npc = new Npc
            {
                Id = Guid.NewGuid(),
                CampaignId = foreignCampaign.Id,
                Name = "Eldrin Vale",
                Status = NpcStatus.Alive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            context.Campaigns.Add(foreignCampaign);
            context.Npcs.Add(npc);
            await context.SaveChangesAsync();
            npcId = npc.Id;
        }

        var response = await _client.GetAsync($"/api/npcs/{npcId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullCrudFlow_WorksEndToEnd()
    {
        var campaignId = await CreateOwnedCampaignAsync();

        var createResponse = await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/npcs", new NpcRequestDto
        {
            Name = "Eldrin Vale",
            Species = "Human",
            Gender = "Male",
            Age = 45,
            Class = NpcClass.Ranger,
            Alignment = Alignment.ChaoticGood,
            Occupation = "Innkeeper",
            Disposition = "Friendly",
            Description = "A weathered innkeeper.",
            DmNotes = "Eldrin secretly works with the Black Hand.",
            Status = NpcStatus.Alive,
            PortraitUrl = "https://example.com/eldrin.png",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<NpcDto>(TestJsonOptions.Default);
        Assert.NotNull(created);
        Assert.Equal(campaignId, created!.CampaignId);
        Assert.Equal(NpcClass.Ranger, created.Class);
        Assert.Equal(Alignment.ChaoticGood, created.Alignment);
        Assert.Equal(NpcStatus.Alive, created.Status);

        var listResponse = await _client.GetFromJsonAsync<List<NpcDto>>(
            $"/api/campaigns/{campaignId}/npcs", TestJsonOptions.Default);
        Assert.Contains(listResponse!, n => n.Id == created.Id);

        var getResponse = await _client.GetAsync($"/api/npcs/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await _client.PutAsJsonAsync($"/api/npcs/{created.Id}", new NpcRequestDto
        {
            Name = "Eldrin Vale (Renamed)",
            Status = NpcStatus.Missing,
            // Class/Alignment omitted - proves clearing them back to null works.
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<NpcDto>(TestJsonOptions.Default);
        Assert.Equal("Eldrin Vale (Renamed)", updated!.Name);
        Assert.Equal(NpcStatus.Missing, updated.Status);
        Assert.Null(updated.Class);
        Assert.Null(updated.Alignment);

        var deleteResponse = await _client.DeleteAsync($"/api/npcs/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/npcs/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task Create_SucceedsWithOnlyRequiredFields()
    {
        var campaignId = await CreateOwnedCampaignAsync();

        var response = await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/npcs", new NpcRequestDto
        {
            Name = "Unnamed Villager",
            Status = NpcStatus.Unknown,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenNameMissing()
    {
        var campaignId = await CreateOwnedCampaignAsync();

        var response = await _client.PostAsJsonAsync($"/api/campaigns/{campaignId}/npcs", new NpcRequestDto
        {
            Name = "",
            Status = NpcStatus.Alive,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenStatusMissing()
    {
        var campaignId = await CreateOwnedCampaignAsync();

        var response = await _client.PostAsync(
            $"/api/campaigns/{campaignId}/npcs",
            JsonContent.Create(new { name = "Eldrin Vale" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
