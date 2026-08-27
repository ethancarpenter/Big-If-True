using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Tests.Services;

public class NpcLocationServiceTests
{
    private sealed record Services(
        NpcLocationService NpcLocationService,
        CampaignService CampaignService,
        CityService CityService,
        LocationService LocationService,
        NpcService NpcService,
        FakeCurrentUserProvider User);

    private static Services CreateServices()
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

        var npcRepository = new NpcRepository(context);
        var npcService = new NpcService(npcRepository, campaignRepository, currentUser);

        var npcLocationRepository = new NpcLocationRepository(context);
        var npcLocationService = new NpcLocationService(npcLocationRepository, npcRepository, locationRepository, currentUser);

        return new Services(npcLocationService, campaignService, cityService, locationService, npcService, currentUser);
    }

    private static async Task<(Guid CampaignId, Guid CityId, Guid LocationId, Guid NpcId)> SeedCampaignWithLocationAndNpcAsync(Services s)
    {
        var campaign = await s.CampaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var city = await s.CityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Stonehaven" });
        var location = await s.LocationService.CreateAsync(campaign.Id, new LocationRequestDto
        {
            CityId = city!.Id,
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
        });
        var npc = await s.NpcService.CreateAsync(campaign.Id, new NpcRequestDto { Name = "Eldrin Vale", Status = NpcStatus.Alive });
        return (campaign.Id, city.Id, location!.Id, npc!.Id);
    }

    [Fact]
    public async Task CreateAsync_LinksNpcAndLocation_WhenBothOwnedInSameCampaign()
    {
        var s = CreateServices();
        var (_, _, locationId, npcId) = await SeedCampaignWithLocationAndNpcAsync(s);

        var result = await s.NpcLocationService.CreateAsync(npcId, new NpcLocationCreateRequestDto
        {
            LocationId = locationId,
            RelationshipType = NpcLocationRelationshipType.WorksAt,
            IsPrimary = false,
        });

        Assert.Equal(CreateNpcLocationOutcome.Success, result.Outcome);
        Assert.NotNull(result.Relationship);
        Assert.Equal(npcId, result.Relationship!.NpcId);
        Assert.Equal(locationId, result.Relationship.LocationId);
        Assert.Equal("Eldrin Vale", result.Relationship.NpcName);
        Assert.Equal("Ironforge Inn", result.Relationship.LocationName);
        Assert.Equal("Stonehaven", result.Relationship.CityName);
        Assert.Equal(NpcLocationRelationshipType.WorksAt, result.Relationship.RelationshipType);
        Assert.False(result.Relationship.IsPrimary);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNotFound_WhenNpcNotOwnedByCurrentUser()
    {
        var s = CreateServices();
        var (_, _, locationId, npcId) = await SeedCampaignWithLocationAndNpcAsync(s);

        s.User.UserId = Guid.NewGuid();
        var result = await s.NpcLocationService.CreateAsync(npcId, new NpcLocationCreateRequestDto
        {
            LocationId = locationId,
            RelationshipType = NpcLocationRelationshipType.WorksAt,
        });

        Assert.Equal(CreateNpcLocationOutcome.NotFound, result.Outcome);
        Assert.Null(result.Relationship);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNotFound_WhenLocationBelongsToDifferentCampaign()
    {
        var s = CreateServices();
        var (_, _, _, npcId) = await SeedCampaignWithLocationAndNpcAsync(s);
        var (_, _, otherLocationId, _) = await SeedCampaignWithLocationAndNpcAsync(s); // a second, separate campaign

        var result = await s.NpcLocationService.CreateAsync(npcId, new NpcLocationCreateRequestDto
        {
            LocationId = otherLocationId, // belongs to the other campaign
            RelationshipType = NpcLocationRelationshipType.WorksAt,
        });

        Assert.Equal(CreateNpcLocationOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNotFound_WhenLocationDoesNotExist()
    {
        var s = CreateServices();
        var (_, _, _, npcId) = await SeedCampaignWithLocationAndNpcAsync(s);

        var result = await s.NpcLocationService.CreateAsync(npcId, new NpcLocationCreateRequestDto
        {
            LocationId = Guid.NewGuid(),
            RelationshipType = NpcLocationRelationshipType.WorksAt,
        });

        Assert.Equal(CreateNpcLocationOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDuplicate_WhenRelationshipAlreadyExists()
    {
        var s = CreateServices();
        var (_, _, locationId, npcId) = await SeedCampaignWithLocationAndNpcAsync(s);
        await s.NpcLocationService.CreateAsync(npcId, new NpcLocationCreateRequestDto
        {
            LocationId = locationId,
            RelationshipType = NpcLocationRelationshipType.WorksAt,
        });

        var result = await s.NpcLocationService.CreateAsync(npcId, new NpcLocationCreateRequestDto
        {
            LocationId = locationId,
            RelationshipType = NpcLocationRelationshipType.Owns, // different type, same pair - still a duplicate
        });

        Assert.Equal(CreateNpcLocationOutcome.Duplicate, result.Outcome);
        Assert.Null(result.Relationship);

        var relationships = await s.NpcLocationService.GetAllForNpcAsync(npcId);
        var single = Assert.Single(relationships!);
        Assert.Equal(NpcLocationRelationshipType.WorksAt, single.RelationshipType); // first row untouched
    }

    [Fact]
    public async Task CreateAsync_SetsPrimary_WhenNoExistingPrimary()
    {
        var s = CreateServices();
        var (_, _, locationId, npcId) = await SeedCampaignWithLocationAndNpcAsync(s);

        var result = await s.NpcLocationService.CreateAsync(npcId, new NpcLocationCreateRequestDto
        {
            LocationId = locationId,
            RelationshipType = NpcLocationRelationshipType.LivesAt,
            IsPrimary = true,
        });

        Assert.True(result.Relationship!.IsPrimary);
    }

    [Fact]
    public async Task CreateAsync_DemotesExistingPrimary_WhenNewRelationshipIsPrimary()
    {
        var s = CreateServices();
        var campaign = await s.CampaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var city = await s.CityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Stonehaven" });
        var locationA = await s.LocationService.CreateAsync(campaign.Id, new LocationRequestDto { CityId = city!.Id, Name = "Ironforge Inn", Type = LocationType.Tavern });
        var locationB = await s.LocationService.CreateAsync(campaign.Id, new LocationRequestDto { CityId = city.Id, Name = "Temple of Moradin", Type = LocationType.Temple });
        var npc = await s.NpcService.CreateAsync(campaign.Id, new NpcRequestDto { Name = "Eldrin Vale", Status = NpcStatus.Alive });

        var first = await s.NpcLocationService.CreateAsync(npc!.Id, new NpcLocationCreateRequestDto
        {
            LocationId = locationA!.Id,
            RelationshipType = NpcLocationRelationshipType.LivesAt,
            IsPrimary = true,
        });
        var second = await s.NpcLocationService.CreateAsync(npc.Id, new NpcLocationCreateRequestDto
        {
            LocationId = locationB!.Id,
            RelationshipType = NpcLocationRelationshipType.WorksAt,
            IsPrimary = true,
        });

        Assert.True(second.Relationship!.IsPrimary);

        var relationships = await s.NpcLocationService.GetAllForNpcAsync(npc.Id);
        Assert.Single(relationships!, r => r.IsPrimary);
        var demoted = relationships!.Single(r => r.Id == first.Relationship!.Id);
        Assert.False(demoted.IsPrimary);
    }

    [Fact]
    public async Task UpdateAsync_DemotesExistingPrimary_WhenSwitchedToPrimary()
    {
        var s = CreateServices();
        var campaign = await s.CampaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var city = await s.CityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Stonehaven" });
        var locationA = await s.LocationService.CreateAsync(campaign.Id, new LocationRequestDto { CityId = city!.Id, Name = "Ironforge Inn", Type = LocationType.Tavern });
        var locationB = await s.LocationService.CreateAsync(campaign.Id, new LocationRequestDto { CityId = city.Id, Name = "Temple of Moradin", Type = LocationType.Temple });
        var npc = await s.NpcService.CreateAsync(campaign.Id, new NpcRequestDto { Name = "Eldrin Vale", Status = NpcStatus.Alive });

        var first = await s.NpcLocationService.CreateAsync(npc!.Id, new NpcLocationCreateRequestDto
        {
            LocationId = locationA!.Id,
            RelationshipType = NpcLocationRelationshipType.LivesAt,
            IsPrimary = true,
        });
        var second = await s.NpcLocationService.CreateAsync(npc.Id, new NpcLocationCreateRequestDto
        {
            LocationId = locationB!.Id,
            RelationshipType = NpcLocationRelationshipType.WorksAt,
            IsPrimary = false,
        });

        var updated = await s.NpcLocationService.UpdateAsync(second.Relationship!.Id, new NpcLocationUpdateRequestDto
        {
            RelationshipType = NpcLocationRelationshipType.WorksAt,
            IsPrimary = true,
        });

        Assert.True(updated!.IsPrimary);
        var relationships = await s.NpcLocationService.GetAllForNpcAsync(npc.Id);
        Assert.Single(relationships!, r => r.IsPrimary);
        Assert.False(relationships!.Single(r => r.Id == first.Relationship!.Id).IsPrimary);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenParentCampaignOwnedByDifferentUser()
    {
        var s = CreateServices();
        var (_, _, locationId, npcId) = await SeedCampaignWithLocationAndNpcAsync(s);
        var created = await s.NpcLocationService.CreateAsync(npcId, new NpcLocationCreateRequestDto
        {
            LocationId = locationId,
            RelationshipType = NpcLocationRelationshipType.WorksAt,
        });

        s.User.UserId = Guid.NewGuid();
        var result = await s.NpcLocationService.UpdateAsync(created.Relationship!.Id, new NpcLocationUpdateRequestDto
        {
            RelationshipType = NpcLocationRelationshipType.Owns,
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesOwnedRelationship()
    {
        var s = CreateServices();
        var (_, _, locationId, npcId) = await SeedCampaignWithLocationAndNpcAsync(s);
        var created = await s.NpcLocationService.CreateAsync(npcId, new NpcLocationCreateRequestDto
        {
            LocationId = locationId,
            RelationshipType = NpcLocationRelationshipType.WorksAt,
        });

        var deleted = await s.NpcLocationService.DeleteAsync(created.Relationship!.Id);
        var fetched = await s.NpcLocationService.GetByIdAsync(created.Relationship.Id);

        Assert.True(deleted);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenParentCampaignOwnedByDifferentUser()
    {
        var s = CreateServices();
        var (_, _, locationId, npcId) = await SeedCampaignWithLocationAndNpcAsync(s);
        var created = await s.NpcLocationService.CreateAsync(npcId, new NpcLocationCreateRequestDto
        {
            LocationId = locationId,
            RelationshipType = NpcLocationRelationshipType.WorksAt,
        });

        s.User.UserId = Guid.NewGuid();
        var deleted = await s.NpcLocationService.DeleteAsync(created.Relationship!.Id);

        Assert.False(deleted);
    }

    [Fact]
    public async Task DeletingNpc_CascadesItsRelationships()
    {
        var s = CreateServices();
        var (campaignId, _, locationId, npcId) = await SeedCampaignWithLocationAndNpcAsync(s);
        var created = await s.NpcLocationService.CreateAsync(npcId, new NpcLocationCreateRequestDto
        {
            LocationId = locationId,
            RelationshipType = NpcLocationRelationshipType.WorksAt,
        });

        await s.NpcService.DeleteAsync(npcId);

        var fetched = await s.NpcLocationService.GetByIdAsync(created.Relationship!.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeletingLocation_CascadesItsRelationships()
    {
        var s = CreateServices();
        var (campaignId, _, locationId, npcId) = await SeedCampaignWithLocationAndNpcAsync(s);
        var created = await s.NpcLocationService.CreateAsync(npcId, new NpcLocationCreateRequestDto
        {
            LocationId = locationId,
            RelationshipType = NpcLocationRelationshipType.WorksAt,
        });

        await s.LocationService.DeleteAsync(locationId);

        var fetched = await s.NpcLocationService.GetByIdAsync(created.Relationship!.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetAllForLocationAsync_ReturnsSameRelationship_AsGetAllForNpcAsync()
    {
        var s = CreateServices();
        var (_, _, locationId, npcId) = await SeedCampaignWithLocationAndNpcAsync(s);
        await s.NpcLocationService.CreateAsync(npcId, new NpcLocationCreateRequestDto
        {
            LocationId = locationId,
            RelationshipType = NpcLocationRelationshipType.Guards,
        });

        var fromNpcSide = await s.NpcLocationService.GetAllForNpcAsync(npcId);
        var fromLocationSide = await s.NpcLocationService.GetAllForLocationAsync(locationId);

        var npcSideRow = Assert.Single(fromNpcSide!);
        var locationSideRow = Assert.Single(fromLocationSide!);
        Assert.Equal(npcSideRow.Id, locationSideRow.Id);
        Assert.Equal("Eldrin Vale", locationSideRow.NpcName); // visible from the Location side
        Assert.Equal("Ironforge Inn", npcSideRow.LocationName); // visible from the Npc side
        Assert.Equal("Stonehaven", npcSideRow.CityName);
    }

    [Fact]
    public async Task GetAllForLocationAsync_ReturnsNull_WhenLocationNotOwned()
    {
        var s = CreateServices();
        var (_, _, locationId, _) = await SeedCampaignWithLocationAndNpcAsync(s);

        s.User.UserId = Guid.NewGuid();
        var result = await s.NpcLocationService.GetAllForLocationAsync(locationId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllForNpcAsync_ReturnsNull_WhenNpcNotOwned()
    {
        var s = CreateServices();
        var (_, _, _, npcId) = await SeedCampaignWithLocationAndNpcAsync(s);

        s.User.UserId = Guid.NewGuid();
        var result = await s.NpcLocationService.GetAllForNpcAsync(npcId);

        Assert.Null(result);
    }
}
