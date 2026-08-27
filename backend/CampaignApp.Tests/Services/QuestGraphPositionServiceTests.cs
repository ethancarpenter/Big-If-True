using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Tests.Services;

public class QuestGraphPositionServiceTests
{
    private sealed record Services(
        QuestGraphPositionService QuestGraphPositionService,
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

        var positionRepository = new QuestGraphPositionRepository(context);
        var positionService = new QuestGraphPositionService(positionRepository, questRepository, campaignRepository, currentUser);

        return new Services(positionService, questService, campaignService, currentUser);
    }

    private static async Task<(Guid CampaignId, Guid QuestId)> SeedCampaignWithQuestAsync(Services s)
    {
        var campaign = await s.CampaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var quest = await s.QuestService.CreateAsync(campaign.Id, new QuestRequestDto
        {
            Name = "Missing Caravan",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });
        return (campaign.Id, quest!.Id);
    }

    [Fact]
    public async Task UpsertAsync_CreatesPosition_OnFirstCall()
    {
        var s = CreateServices();
        var (_, questId) = await SeedCampaignWithQuestAsync(s);

        var result = await s.QuestGraphPositionService.UpsertAsync(questId, new QuestGraphPositionUpdateRequestDto { X = 100, Y = 200 });

        Assert.NotNull(result);
        Assert.Equal(questId, result!.QuestId);
        Assert.Equal(100, result.X);
        Assert.Equal(200, result.Y);
    }

    [Fact]
    public async Task UpsertAsync_UpdatesInPlace_OnSecondCall()
    {
        var s = CreateServices();
        var (_, questId) = await SeedCampaignWithQuestAsync(s);
        await s.QuestGraphPositionService.UpsertAsync(questId, new QuestGraphPositionUpdateRequestDto { X = 100, Y = 200 });

        var result = await s.QuestGraphPositionService.UpsertAsync(questId, new QuestGraphPositionUpdateRequestDto { X = 300, Y = 400 });

        Assert.Equal(300, result!.X);
        Assert.Equal(400, result.Y);
    }

    [Fact]
    public async Task UpsertAsync_ReturnsNull_WhenQuestNotOwnedByCurrentUser()
    {
        var s = CreateServices();
        var (_, questId) = await SeedCampaignWithQuestAsync(s);

        s.User.UserId = Guid.NewGuid();
        var result = await s.QuestGraphPositionService.UpsertAsync(questId, new QuestGraphPositionUpdateRequestDto { X = 1, Y = 1 });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllForCampaignAsync_ReturnsAllPositionsInCampaign()
    {
        var s = CreateServices();
        var (campaignId, questId) = await SeedCampaignWithQuestAsync(s);
        await s.QuestGraphPositionService.UpsertAsync(questId, new QuestGraphPositionUpdateRequestDto { X = 5, Y = 6 });

        var all = await s.QuestGraphPositionService.GetAllForCampaignAsync(campaignId);

        var single = Assert.Single(all!);
        Assert.Equal(questId, single.QuestId);
        Assert.Equal(5, single.X);
        Assert.Equal(6, single.Y);
    }

    [Fact]
    public async Task GetAllForCampaignAsync_ReturnsNull_WhenCampaignNotOwned()
    {
        var s = CreateServices();
        var (campaignId, _) = await SeedCampaignWithQuestAsync(s);

        s.User.UserId = Guid.NewGuid();
        var result = await s.QuestGraphPositionService.GetAllForCampaignAsync(campaignId);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeletingQuest_CascadesItsGraphPosition()
    {
        var s = CreateServices();
        var (campaignId, questId) = await SeedCampaignWithQuestAsync(s);
        await s.QuestGraphPositionService.UpsertAsync(questId, new QuestGraphPositionUpdateRequestDto { X = 1, Y = 2 });

        await s.QuestService.DeleteAsync(questId);

        var all = await s.QuestGraphPositionService.GetAllForCampaignAsync(campaignId);
        Assert.Empty(all!);
    }
}
