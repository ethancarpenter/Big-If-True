using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Tests.Services;

public class QuestLocationServiceTests
{
    private sealed record Services(
        QuestLocationService QuestLocationService,
        QuestService QuestService,
        CityService CityService,
        LocationService LocationService,
        CampaignService CampaignService,
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

        var questRepository = new QuestRepository(context);
        var questService = new QuestService(questRepository, campaignRepository, currentUser);

        var cityRepository = new CityRepository(context);
        var cityService = new CityService(cityRepository, campaignRepository, currentUser);

        var locationRepository = new LocationRepository(context);
        var locationService = new LocationService(locationRepository, campaignRepository, cityRepository, currentUser);

        var questLocationRepository = new QuestLocationRepository(context);
        var questLocationService = new QuestLocationService(questLocationRepository, questRepository, locationRepository, currentUser);

        return new Services(questLocationService, questService, cityService, locationService, campaignService, currentUser);
    }

    private static async Task<(Guid CampaignId, Guid QuestId, Guid LocationId)> SeedCampaignWithQuestAndLocationAsync(Services s)
    {
        var campaign = await s.CampaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var quest = await s.QuestService.CreateAsync(campaign.Id, new QuestRequestDto
        {
            Name = "Missing Caravan",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });
        var city = await s.CityService.CreateAsync(campaign.Id, new CityRequestDto { Name = "Stonehaven" });
        var location = await s.LocationService.CreateAsync(campaign.Id, new LocationRequestDto
        {
            CityId = city!.Id,
            Name = "Ironforge Inn",
            Type = LocationType.Tavern,
        });
        return (campaign.Id, quest!.Id, location!.Id);
    }

    [Fact]
    public async Task CreateAsync_LinksQuestAndLocation_WhenBothOwnedInSameCampaign()
    {
        var s = CreateServices();
        var (_, questId, locationId) = await SeedCampaignWithQuestAndLocationAsync(s);

        var result = await s.QuestLocationService.CreateAsync(questId, new QuestLocationCreateRequestDto
        {
            LocationId = locationId,
            Role = QuestLocationRole.StartingLocation,
            Notes = "The party first hears rumors here.",
        });

        Assert.Equal(CreateQuestLocationOutcome.Success, result.Outcome);
        Assert.NotNull(result.Relationship);
        Assert.Equal(questId, result.Relationship!.QuestId);
        Assert.Equal("Missing Caravan", result.Relationship.QuestName);
        Assert.Equal(locationId, result.Relationship.LocationId);
        Assert.Equal("Ironforge Inn", result.Relationship.LocationName);
        Assert.Equal("Stonehaven", result.Relationship.CityName);
        Assert.Equal(QuestLocationRole.StartingLocation, result.Relationship.Role);
        Assert.Equal("The party first hears rumors here.", result.Relationship.Notes);
    }

    [Fact]
    public async Task CreateAsync_SucceedsWithoutNotes()
    {
        var s = CreateServices();
        var (_, questId, locationId) = await SeedCampaignWithQuestAndLocationAsync(s);

        var result = await s.QuestLocationService.CreateAsync(questId, new QuestLocationCreateRequestDto
        {
            LocationId = locationId,
            Role = QuestLocationRole.Destination,
        });

        Assert.Equal(CreateQuestLocationOutcome.Success, result.Outcome);
        Assert.Null(result.Relationship!.Notes);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNotFound_WhenQuestNotOwnedByCurrentUser()
    {
        var s = CreateServices();
        var (_, questId, locationId) = await SeedCampaignWithQuestAndLocationAsync(s);

        s.User.UserId = Guid.NewGuid();
        var result = await s.QuestLocationService.CreateAsync(questId, new QuestLocationCreateRequestDto
        {
            LocationId = locationId,
            Role = QuestLocationRole.StartingLocation,
        });

        Assert.Equal(CreateQuestLocationOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNotFound_WhenLocationBelongsToDifferentCampaign()
    {
        var s = CreateServices();
        var (_, questId, _) = await SeedCampaignWithQuestAndLocationAsync(s);
        var (_, _, otherLocationId) = await SeedCampaignWithQuestAndLocationAsync(s); // a second, separate campaign

        var result = await s.QuestLocationService.CreateAsync(questId, new QuestLocationCreateRequestDto
        {
            LocationId = otherLocationId,
            Role = QuestLocationRole.StartingLocation,
        });

        Assert.Equal(CreateQuestLocationOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDuplicate_WhenRelationshipAlreadyExists()
    {
        var s = CreateServices();
        var (_, questId, locationId) = await SeedCampaignWithQuestAndLocationAsync(s);
        await s.QuestLocationService.CreateAsync(questId, new QuestLocationCreateRequestDto { LocationId = locationId, Role = QuestLocationRole.StartingLocation });

        var result = await s.QuestLocationService.CreateAsync(questId, new QuestLocationCreateRequestDto
        {
            LocationId = locationId,
            Role = QuestLocationRole.EncounterLocation, // different role, same pair - still a duplicate
        });

        Assert.Equal(CreateQuestLocationOutcome.Duplicate, result.Outcome);

        var relationships = await s.QuestLocationService.GetAllForQuestAsync(questId);
        var single = Assert.Single(relationships!);
        Assert.Equal(QuestLocationRole.StartingLocation, single.Role); // first row untouched
    }

    [Fact]
    public async Task UpdateAsync_UpdatesRoleAndNotes()
    {
        var s = CreateServices();
        var (_, questId, locationId) = await SeedCampaignWithQuestAndLocationAsync(s);
        var created = await s.QuestLocationService.CreateAsync(questId, new QuestLocationCreateRequestDto { LocationId = locationId, Role = QuestLocationRole.RelatedLocation });

        var updated = await s.QuestLocationService.UpdateAsync(created.Relationship!.Id, new QuestLocationUpdateRequestDto
        {
            Role = QuestLocationRole.EncounterLocation,
            Notes = "An ambush occurs here.",
        });

        Assert.NotNull(updated);
        Assert.Equal(QuestLocationRole.EncounterLocation, updated!.Role);
        Assert.Equal("An ambush occurs here.", updated.Notes);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenParentCampaignOwnedByDifferentUser()
    {
        var s = CreateServices();
        var (_, questId, locationId) = await SeedCampaignWithQuestAndLocationAsync(s);
        var created = await s.QuestLocationService.CreateAsync(questId, new QuestLocationCreateRequestDto { LocationId = locationId, Role = QuestLocationRole.RelatedLocation });

        s.User.UserId = Guid.NewGuid();
        var result = await s.QuestLocationService.UpdateAsync(created.Relationship!.Id, new QuestLocationUpdateRequestDto { Role = QuestLocationRole.EncounterLocation });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesOwnedRelationship()
    {
        var s = CreateServices();
        var (_, questId, locationId) = await SeedCampaignWithQuestAndLocationAsync(s);
        var created = await s.QuestLocationService.CreateAsync(questId, new QuestLocationCreateRequestDto { LocationId = locationId, Role = QuestLocationRole.RelatedLocation });

        var deleted = await s.QuestLocationService.DeleteAsync(created.Relationship!.Id);
        var fetched = await s.QuestLocationService.GetByIdAsync(created.Relationship.Id);

        Assert.True(deleted);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenParentCampaignOwnedByDifferentUser()
    {
        var s = CreateServices();
        var (_, questId, locationId) = await SeedCampaignWithQuestAndLocationAsync(s);
        var created = await s.QuestLocationService.CreateAsync(questId, new QuestLocationCreateRequestDto { LocationId = locationId, Role = QuestLocationRole.RelatedLocation });

        s.User.UserId = Guid.NewGuid();
        var deleted = await s.QuestLocationService.DeleteAsync(created.Relationship!.Id);

        Assert.False(deleted);
    }

    [Fact]
    public async Task DeletingQuest_CascadesItsRelationships()
    {
        var s = CreateServices();
        var (_, questId, locationId) = await SeedCampaignWithQuestAndLocationAsync(s);
        var created = await s.QuestLocationService.CreateAsync(questId, new QuestLocationCreateRequestDto { LocationId = locationId, Role = QuestLocationRole.RelatedLocation });

        await s.QuestService.DeleteAsync(questId);

        var fetched = await s.QuestLocationService.GetByIdAsync(created.Relationship!.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeletingLocation_CascadesItsRelationships()
    {
        var s = CreateServices();
        var (_, questId, locationId) = await SeedCampaignWithQuestAndLocationAsync(s);
        var created = await s.QuestLocationService.CreateAsync(questId, new QuestLocationCreateRequestDto { LocationId = locationId, Role = QuestLocationRole.RelatedLocation });

        await s.LocationService.DeleteAsync(locationId);

        var fetched = await s.QuestLocationService.GetByIdAsync(created.Relationship!.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetAllForLocationAsync_ReturnsSameRelationship_AsGetAllForQuestAsync()
    {
        var s = CreateServices();
        var (_, questId, locationId) = await SeedCampaignWithQuestAndLocationAsync(s);
        await s.QuestLocationService.CreateAsync(questId, new QuestLocationCreateRequestDto { LocationId = locationId, Role = QuestLocationRole.ObjectiveLocation });

        var fromQuestSide = await s.QuestLocationService.GetAllForQuestAsync(questId);
        var fromLocationSide = await s.QuestLocationService.GetAllForLocationAsync(locationId);

        var questSideRow = Assert.Single(fromQuestSide!);
        var locationSideRow = Assert.Single(fromLocationSide!);
        Assert.Equal(questSideRow.Id, locationSideRow.Id);
        Assert.Equal("Missing Caravan", locationSideRow.QuestName); // visible from the Location side
        Assert.Equal("Ironforge Inn", questSideRow.LocationName); // visible from the Quest side
        Assert.Equal("Stonehaven", questSideRow.CityName);
    }

    [Fact]
    public async Task GetAllForLocationAsync_ReturnsNull_WhenLocationNotOwned()
    {
        var s = CreateServices();
        var (_, _, locationId) = await SeedCampaignWithQuestAndLocationAsync(s);

        s.User.UserId = Guid.NewGuid();
        var result = await s.QuestLocationService.GetAllForLocationAsync(locationId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllForQuestAsync_ReturnsNull_WhenQuestNotOwned()
    {
        var s = CreateServices();
        var (_, questId, _) = await SeedCampaignWithQuestAndLocationAsync(s);

        s.User.UserId = Guid.NewGuid();
        var result = await s.QuestLocationService.GetAllForQuestAsync(questId);

        Assert.Null(result);
    }
}
