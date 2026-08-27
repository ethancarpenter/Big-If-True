using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Tests.Services;

public class QuestObjectiveServiceTests
{
    private sealed record Services(
        QuestObjectiveService ObjectiveService,
        QuestService QuestService,
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

        var objectiveRepository = new QuestObjectiveRepository(context);
        var objectiveService = new QuestObjectiveService(objectiveRepository, questRepository, currentUser);

        return new Services(objectiveService, questService, campaignService, currentUser);
    }

    private static async Task<Guid> SeedQuestAsync(Services s)
    {
        var campaign = await s.CampaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var quest = await s.QuestService.CreateAsync(campaign.Id, new QuestRequestDto
        {
            Name = "Missing Caravan",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });
        return quest!.Id;
    }

    [Fact]
    public async Task CreateAsync_AssignsSequentialSortOrder()
    {
        var s = CreateServices();
        var questId = await SeedQuestAsync(s);

        var first = await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "Talk to Eldrin Vale" });
        var second = await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "Travel to the last known location" });
        var third = await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "Search the area for clues" });

        Assert.Equal(0, first!.SortOrder);
        Assert.Equal(1, second!.SortOrder);
        Assert.Equal(2, third!.SortOrder);
        Assert.False(first.IsCompleted);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenQuestNotOwnedByCurrentUser()
    {
        var s = CreateServices();
        var questId = await SeedQuestAsync(s);

        s.User.UserId = Guid.NewGuid();
        var result = await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "Talk to Eldrin Vale" });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_CompactsRemainingSortOrders()
    {
        var s = CreateServices();
        var questId = await SeedQuestAsync(s);
        var first = await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "First" });
        var second = await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "Second" });
        var third = await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "Third" });

        await s.ObjectiveService.DeleteAsync(second!.Id); // removes the middle one (SortOrder 1)

        var remaining = await s.ObjectiveService.GetAllForQuestAsync(questId);
        Assert.Equal(2, remaining!.Count);
        Assert.Equal(0, remaining.Single(o => o.Id == first!.Id).SortOrder);
        Assert.Equal(1, remaining.Single(o => o.Id == third!.Id).SortOrder); // was 2, compacted to 1
    }

    [Fact]
    public async Task UpdateAsync_TogglesCompletionAndChangesDescription()
    {
        var s = CreateServices();
        var questId = await SeedQuestAsync(s);
        var objective = await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "Talk to Eldrin Vale" });

        var updated = await s.ObjectiveService.UpdateAsync(objective!.Id, new QuestObjectiveUpdateRequestDto
        {
            Description = "Talk to Eldrin Vale at the inn",
            IsCompleted = true,
        });

        Assert.NotNull(updated);
        Assert.True(updated!.IsCompleted);
        Assert.Equal("Talk to Eldrin Vale at the inn", updated.Description);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenParentCampaignOwnedByDifferentUser()
    {
        var s = CreateServices();
        var questId = await SeedQuestAsync(s);
        var objective = await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "Talk to Eldrin Vale" });

        s.User.UserId = Guid.NewGuid();
        var result = await s.ObjectiveService.UpdateAsync(objective!.Id, new QuestObjectiveUpdateRequestDto { Description = "x", IsCompleted = true });

        Assert.Null(result);
    }

    [Fact]
    public async Task ReorderAsync_ReassignsSortOrderToMatchRequestedOrder()
    {
        var s = CreateServices();
        var questId = await SeedQuestAsync(s);
        var first = await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "First" });
        var second = await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "Second" });
        var third = await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "Third" });

        var reordered = await s.ObjectiveService.ReorderAsync(questId, new QuestObjectiveReorderRequestDto
        {
            ObjectiveIds = [third!.Id, first!.Id, second!.Id],
        });

        Assert.NotNull(reordered);
        Assert.Equal(third.Id, reordered![0].Id);
        Assert.Equal(0, reordered[0].SortOrder);
        Assert.Equal(first.Id, reordered[1].Id);
        Assert.Equal(1, reordered[1].SortOrder);
        Assert.Equal(second.Id, reordered[2].Id);
        Assert.Equal(2, reordered[2].SortOrder);
    }

    [Fact]
    public async Task ReorderAsync_ReturnsNull_WhenIdSetDoesNotMatchQuestsObjectives()
    {
        var s = CreateServices();
        var questId = await SeedQuestAsync(s);
        var first = await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "First" });

        var result = await s.ObjectiveService.ReorderAsync(questId, new QuestObjectiveReorderRequestDto
        {
            ObjectiveIds = [first!.Id, Guid.NewGuid()], // includes an id that doesn't belong to this quest
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task ReorderAsync_ReturnsNull_WhenQuestNotOwnedByCurrentUser()
    {
        var s = CreateServices();
        var questId = await SeedQuestAsync(s);
        var first = await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "First" });

        s.User.UserId = Guid.NewGuid();
        var result = await s.ObjectiveService.ReorderAsync(questId, new QuestObjectiveReorderRequestDto { ObjectiveIds = [first!.Id] });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllForQuestAsync_ReturnsOrderedBySortOrder()
    {
        var s = CreateServices();
        var questId = await SeedQuestAsync(s);
        await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "First" });
        await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "Second" });

        var objectives = await s.ObjectiveService.GetAllForQuestAsync(questId);

        Assert.Equal(["First", "Second"], objectives!.Select(o => o.Description));
    }

    [Fact]
    public async Task DeletingQuest_CascadesItsObjectives()
    {
        var s = CreateServices();
        var questId = await SeedQuestAsync(s);
        await s.ObjectiveService.CreateAsync(questId, new QuestObjectiveCreateRequestDto { Description = "Talk to Eldrin Vale" });

        await s.QuestService.DeleteAsync(questId);

        var objectives = await s.ObjectiveService.GetAllForQuestAsync(questId);
        Assert.Null(objectives); // the quest itself is gone, so ownership resolution returns null
    }
}
