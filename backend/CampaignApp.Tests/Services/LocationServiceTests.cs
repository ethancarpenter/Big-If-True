using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Tests.Services;

public class LocationServiceTests
{
    private static (LocationService LocationService, CampaignService CampaignService, CityService CityService, FakeCurrentUserProvider User) CreateServices()
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
        var locationRepository = new LocationRepository(context);
        var locationService = new LocationService(locationRepository, campaignRepository, cityRepository, currentUser);
        return (locationService, campaignService, cityService, currentUser);
    }

    private static async Task<(CampaignDto Campaign, CityDto City)> SeedCampaignWithCityAsync(
        CampaignService campaignService, CityService cityService)
    {
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var city = await cityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Stonehaven" });
        return (campaign, city!);
    }

    [Fact]
    public async Task CreateAsync_SetsFieldsAndTimestamps_WhenCampaignAndCityOwned()
    {
        var (locationService, campaignService, cityService, _) = CreateServices();
        var (campaign, city) = await SeedCampaignWithCityAsync(campaignService, cityService);

        var created = await locationService.CreateAsync(campaign.Id, new LocationRequestDto
        {
            CityId = city.Id,
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
            Description = "A bustling inn.",
            DmNotes = "Eldrin secretly works with the Black Hand.",
        });

        Assert.NotNull(created);
        Assert.Equal(campaign.Id, created!.CampaignId);
        Assert.Equal(city.Id, created.CityId);
        Assert.Equal("Stonehaven", created.CityName);
        Assert.Equal(LocationType.Tavern, created.Type);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenCampaignNotOwnedByCurrentUser()
    {
        var (locationService, campaignService, cityService, user) = CreateServices();
        var (campaign, city) = await SeedCampaignWithCityAsync(campaignService, cityService);

        user.UserId = Guid.NewGuid();
        var result = await locationService.CreateAsync(campaign.Id, new LocationRequestDto
        {
            CityId = city.Id,
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenCityDoesNotExist()
    {
        var (locationService, campaignService, cityService, _) = CreateServices();
        var (campaign, _) = await SeedCampaignWithCityAsync(campaignService, cityService);

        var result = await locationService.CreateAsync(campaign.Id, new LocationRequestDto
        {
            CityId = Guid.NewGuid(),
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenCityBelongsToDifferentCampaign()
    {
        var (locationService, campaignService, cityService, _) = CreateServices();
        var (campaignA, _) = await SeedCampaignWithCityAsync(campaignService, cityService);
        var (_, cityB) = await SeedCampaignWithCityAsync(campaignService, cityService);

        var result = await locationService.CreateAsync(campaignA.Id, new LocationRequestDto
        {
            CityId = cityB.Id, // belongs to a different campaign than campaignA
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenParentCampaignOwnedByDifferentUser()
    {
        var (locationService, campaignService, cityService, user) = CreateServices();
        var (campaign, city) = await SeedCampaignWithCityAsync(campaignService, cityService);
        var location = await locationService.CreateAsync(campaign.Id, new LocationRequestDto
        {
            CityId = city.Id,
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
        });

        user.UserId = Guid.NewGuid();
        var result = await locationService.GetByIdAsync(location!.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllForCampaignAsync_FiltersByCityId()
    {
        var (locationService, campaignService, cityService, _) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var cityA = await cityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Stonehaven" });
        var cityB = await cityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Ironhold" });
        await locationService.CreateAsync(campaign.Id, new LocationRequestDto { CityId = cityA!.Id, Name = "Ironforge Inn", Type = LocationType.Tavern });
        await locationService.CreateAsync(campaign.Id, new LocationRequestDto { CityId = cityB!.Id, Name = "The Rusty Anvil", Type = LocationType.Shop });

        var all = await locationService.GetAllForCampaignAsync(campaign.Id, cityId: null);
        var filtered = await locationService.GetAllForCampaignAsync(campaign.Id, cityId: cityA.Id);

        Assert.Equal(2, all!.Count);
        var single = Assert.Single(filtered!);
        Assert.Equal("Ironforge Inn", single.Name);
    }

    [Fact]
    public async Task GetAllForCampaignAsync_ReturnsNull_WhenCampaignNotOwned()
    {
        var (locationService, campaignService, cityService, user) = CreateServices();
        var (campaign, _) = await SeedCampaignWithCityAsync(campaignService, cityService);

        user.UserId = Guid.NewGuid();
        var result = await locationService.GetAllForCampaignAsync(campaign.Id, null);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFieldsAndReassignsCity()
    {
        var (locationService, campaignService, cityService, _) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var cityA = await cityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Stonehaven" });
        var cityB = await cityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Ironhold" });
        var location = await locationService.CreateAsync(campaign.Id, new LocationRequestDto
        {
            CityId = cityA!.Id,
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
        });

        var updated = await locationService.UpdateAsync(location!.Id, new LocationRequestDto
        {
            CityId = cityB!.Id,
            Name = "Ironforge Inn (Relocated)",
            Type = LocationType.Shop,
        });

        Assert.NotNull(updated);
        Assert.Equal(cityB.Id, updated!.CityId);
        Assert.Equal("Ironhold", updated.CityName);
        Assert.Equal(LocationType.Shop, updated.Type);
        Assert.True(updated.UpdatedAt >= location.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenReassignedCityBelongsToDifferentCampaign()
    {
        var (locationService, campaignService, cityService, _) = CreateServices();
        var (campaignA, cityA) = await SeedCampaignWithCityAsync(campaignService, cityService);
        var (_, cityB) = await SeedCampaignWithCityAsync(campaignService, cityService);
        var location = await locationService.CreateAsync(campaignA.Id, new LocationRequestDto
        {
            CityId = cityA.Id,
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
        });

        var result = await locationService.UpdateAsync(location!.Id, new LocationRequestDto
        {
            CityId = cityB.Id, // belongs to a different campaign than the location's own
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesOwnedLocation()
    {
        var (locationService, campaignService, cityService, _) = CreateServices();
        var (campaign, city) = await SeedCampaignWithCityAsync(campaignService, cityService);
        var location = await locationService.CreateAsync(campaign.Id, new LocationRequestDto
        {
            CityId = city.Id,
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
        });

        var deleted = await locationService.DeleteAsync(location!.Id);
        var fetched = await locationService.GetByIdAsync(location.Id);

        Assert.True(deleted);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenParentCampaignOwnedByDifferentUser()
    {
        var (locationService, campaignService, cityService, user) = CreateServices();
        var (campaign, city) = await SeedCampaignWithCityAsync(campaignService, cityService);
        var location = await locationService.CreateAsync(campaign.Id, new LocationRequestDto
        {
            CityId = city.Id,
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
        });

        user.UserId = Guid.NewGuid();
        var deleted = await locationService.DeleteAsync(location!.Id);

        Assert.False(deleted);
    }
}
