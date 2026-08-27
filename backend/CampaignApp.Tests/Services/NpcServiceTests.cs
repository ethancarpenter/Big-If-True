using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Tests.Services;

public class NpcServiceTests
{
    private static (NpcService NpcService, CampaignService CampaignService, FakeCurrentUserProvider User) CreateServices()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);
        var currentUser = new FakeCurrentUserProvider();
        var campaignRepository = new CampaignRepository(context);
        var campaignService = new CampaignService(campaignRepository, currentUser);
        var npcRepository = new NpcRepository(context);
        var npcService = new NpcService(npcRepository, campaignRepository, currentUser);
        return (npcService, campaignService, currentUser);
    }

    [Fact]
    public async Task CreateAsync_SetsFieldsAndTimestamps_WhenCampaignOwned()
    {
        var (npcService, campaignService, _) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });

        var created = await npcService.CreateAsync(campaign.Id, new NpcRequestDto
        {
            Name = "Eldrin Vale",
            Species = "Human",
            Gender = "Male",
            Age = 45,
            Class = NpcClass.Ranger,
            Alignment = Alignment.ChaoticGood,
            Occupation = "Innkeeper",
            Disposition = "Friendly",
            Description = "A weathered innkeeper with a hidden past.",
            DmNotes = "Eldrin secretly works with the Black Hand.",
            Status = NpcStatus.Alive,
            PortraitUrl = "https://example.com/eldrin.png",
        });

        Assert.NotNull(created);
        Assert.Equal(campaign.Id, created!.CampaignId);
        Assert.Equal("Eldrin Vale", created.Name);
        Assert.Equal(NpcClass.Ranger, created.Class);
        Assert.Equal(Alignment.ChaoticGood, created.Alignment);
        Assert.Equal(NpcStatus.Alive, created.Status);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);
    }

    [Fact]
    public async Task CreateAsync_SucceedsWithOnlyRequiredFields()
    {
        var (npcService, campaignService, _) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });

        var created = await npcService.CreateAsync(campaign.Id, new NpcRequestDto
        {
            Name = "Unnamed Villager",
            Status = NpcStatus.Unknown,
        });

        Assert.NotNull(created);
        Assert.Null(created!.Species);
        Assert.Null(created.Class);
        Assert.Null(created.Alignment);
        Assert.Null(created.Age);
        Assert.Equal(NpcStatus.Unknown, created.Status);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenCampaignNotOwnedByCurrentUser()
    {
        var (npcService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });

        user.UserId = Guid.NewGuid();
        var result = await npcService.CreateAsync(campaign.Id, new NpcRequestDto { Name = "Eldrin Vale", Status = NpcStatus.Alive });

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenCampaignDoesNotExist()
    {
        var (npcService, _, _) = CreateServices();

        var result = await npcService.CreateAsync(Guid.NewGuid(), new NpcRequestDto { Name = "Eldrin Vale", Status = NpcStatus.Alive });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenParentCampaignOwnedByDifferentUser()
    {
        var (npcService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var npc = await npcService.CreateAsync(campaign.Id, new NpcRequestDto { Name = "Eldrin Vale", Status = NpcStatus.Alive });

        user.UserId = Guid.NewGuid();
        var result = await npcService.GetByIdAsync(npc!.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllForCampaignAsync_ReturnsNull_WhenCampaignNotOwned()
    {
        var (npcService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });

        user.UserId = Guid.NewGuid();
        var result = await npcService.GetAllForCampaignAsync(campaign.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllForCampaignAsync_ReturnsOnlyNpcsForThatCampaign()
    {
        var (npcService, campaignService, _) = CreateServices();
        var campaignA = await campaignService.CreateAsync(new CampaignRequestDto { Name = "Campaign A" });
        var campaignB = await campaignService.CreateAsync(new CampaignRequestDto { Name = "Campaign B" });
        await npcService.CreateAsync(campaignA.Id, new NpcRequestDto { Name = "Eldrin Vale", Status = NpcStatus.Alive });
        await npcService.CreateAsync(campaignB.Id, new NpcRequestDto { Name = "Captain Mara", Status = NpcStatus.Alive });

        var result = await npcService.GetAllForCampaignAsync(campaignA.Id);

        var single = Assert.Single(result!);
        Assert.Equal("Eldrin Vale", single.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFieldsAndTimestamp()
    {
        var (npcService, campaignService, _) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var npc = await npcService.CreateAsync(campaign.Id, new NpcRequestDto
        {
            Name = "Eldrin Vale",
            Class = NpcClass.Ranger,
            Status = NpcStatus.Alive,
        });

        var updated = await npcService.UpdateAsync(npc!.Id, new NpcRequestDto
        {
            Name = "Eldrin Vale (Renamed)",
            Status = NpcStatus.Missing,
        });

        Assert.NotNull(updated);
        Assert.Equal("Eldrin Vale (Renamed)", updated!.Name);
        Assert.Equal(NpcStatus.Missing, updated.Status);
        Assert.Null(updated.Class); // clearing Class by omitting it should persist as null
        Assert.True(updated.UpdatedAt >= npc.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenParentCampaignOwnedByDifferentUser()
    {
        var (npcService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var npc = await npcService.CreateAsync(campaign.Id, new NpcRequestDto { Name = "Eldrin Vale", Status = NpcStatus.Alive });

        user.UserId = Guid.NewGuid();
        var result = await npcService.UpdateAsync(npc!.Id, new NpcRequestDto { Name = "Renamed", Status = NpcStatus.Alive });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesOwnedNpc()
    {
        var (npcService, campaignService, _) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var npc = await npcService.CreateAsync(campaign.Id, new NpcRequestDto { Name = "Eldrin Vale", Status = NpcStatus.Alive });

        var deleted = await npcService.DeleteAsync(npc!.Id);
        var fetched = await npcService.GetByIdAsync(npc.Id);

        Assert.True(deleted);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenParentCampaignOwnedByDifferentUser()
    {
        var (npcService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var npc = await npcService.CreateAsync(campaign.Id, new NpcRequestDto { Name = "Eldrin Vale", Status = NpcStatus.Alive });

        user.UserId = Guid.NewGuid();
        var deleted = await npcService.DeleteAsync(npc!.Id);

        Assert.False(deleted);
    }
}
