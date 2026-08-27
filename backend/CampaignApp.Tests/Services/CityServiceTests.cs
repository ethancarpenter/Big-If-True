using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Tests.Services;

public class CityServiceTests
{
    private static (CityService CityService, CampaignService CampaignService, FakeCurrentUserProvider User) CreateServices()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);
        var currentUser = new FakeCurrentUserProvider();
        var campaignRepository = new CampaignRepository(context);
        var campaignService = new CampaignService(campaignRepository, currentUser);
        var cityRepository = new CityRepository(context);
        var cityService = new CityService(cityRepository, campaignRepository, currentUser);
        return (cityService, campaignService, currentUser);
    }

    [Fact]
    public async Task CreateAsync_SetsFieldsAndTimestamps_WhenCampaignOwned()
    {
        var (cityService, campaignService, _) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });

        var created = await cityService.CreateAsync(campaign.Id, new CityRequestDto
        {
            Name = "Stonehaven",
            Population = "~8,000",
            Government = "Merchant Council",
            Region = "Northern Reach",
            Alignment = "Neutral",
        });

        Assert.NotNull(created);
        Assert.Equal(campaign.Id, created!.CampaignId);
        Assert.Equal("Stonehaven", created.Name);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenCampaignNotOwnedByCurrentUser()
    {
        var (cityService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });

        user.UserId = Guid.NewGuid();
        var result = await cityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Stonehaven" });

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenCampaignDoesNotExist()
    {
        var (cityService, _, _) = CreateServices();

        var result = await cityService.CreateAsync(Guid.NewGuid(), new CityRequestDto { Name = "Stonehaven" });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenParentCampaignOwnedByDifferentUser()
    {
        var (cityService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var city = await cityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Stonehaven" });

        user.UserId = Guid.NewGuid();
        var result = await cityService.GetByIdAsync(city!.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenParentCampaignOwnedByDifferentUser()
    {
        var (cityService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var city = await cityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Stonehaven" });

        user.UserId = Guid.NewGuid();
        var result = await cityService.UpdateAsync(city!.Id, new CityRequestDto { Name = "Renamed" });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenParentCampaignOwnedByDifferentUser()
    {
        var (cityService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var city = await cityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Stonehaven" });

        user.UserId = Guid.NewGuid();
        var deleted = await cityService.DeleteAsync(city!.Id);

        Assert.False(deleted);
    }

    [Fact]
    public async Task GetAllForCampaignAsync_ReturnsNull_WhenCampaignNotOwned()
    {
        var (cityService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });

        user.UserId = Guid.NewGuid();
        var result = await cityService.GetAllForCampaignAsync(campaign.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllForCampaignAsync_ReturnsOnlyCitiesForThatCampaign()
    {
        var (cityService, campaignService, _) = CreateServices();
        var campaignA = await campaignService.CreateAsync(new CampaignRequestDto { Name = "Campaign A" });
        var campaignB = await campaignService.CreateAsync(new CampaignRequestDto { Name = "Campaign B" });
        await cityService.CreateAsync(campaignA.Id, new CityRequestDto { Name = "Stonehaven" });
        await cityService.CreateAsync(campaignB.Id, new CityRequestDto { Name = "Ironhold" });

        var result = await cityService.GetAllForCampaignAsync(campaignA.Id);

        var single = Assert.Single(result!);
        Assert.Equal("Stonehaven", single.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFieldsAndTimestamp_WhenOwned()
    {
        var (cityService, campaignService, _) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var city = await cityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Stonehaven" });

        var updated = await cityService.UpdateAsync(city!.Id, new CityRequestDto
        {
            Name = "Stonehaven Renamed",
            Region = "The Reach",
        });

        Assert.NotNull(updated);
        Assert.Equal("Stonehaven Renamed", updated!.Name);
        Assert.Equal("The Reach", updated.Region);
        Assert.True(updated.UpdatedAt >= city.UpdatedAt);
    }

    [Fact]
    public async Task DeleteAsync_RemovesOwnedCity()
    {
        var (cityService, campaignService, _) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var city = await cityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Stonehaven" });

        var deleted = await cityService.DeleteAsync(city!.Id);
        var fetched = await cityService.GetByIdAsync(city.Id);

        Assert.True(deleted);
        Assert.Null(fetched);
    }
}
