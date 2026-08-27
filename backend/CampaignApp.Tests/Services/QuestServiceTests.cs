using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Tests.Services;

public class QuestServiceTests
{
    private static (QuestService QuestService, CampaignService CampaignService, FakeCurrentUserProvider User) CreateServices()
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
        return (questService, campaignService, currentUser);
    }

    [Fact]
    public async Task CreateAsync_SetsFieldsAndTimestamps_WhenCampaignOwned()
    {
        var (questService, campaignService, _) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });

        var created = await questService.CreateAsync(campaign.Id, new QuestRequestDto
        {
            Name = "Missing Caravan",
            Description = "A merchant caravan has gone missing on the north road.",
            Status = QuestStatus.Active,
            QuestType = QuestType.MainQuest,
            RecommendedLevelMin = 3,
            RecommendedLevelMax = 5,
            DmNotes = "The caravan was actually raided by the Black Hand.",
        });

        Assert.NotNull(created);
        Assert.Equal(campaign.Id, created!.CampaignId);
        Assert.Equal("Missing Caravan", created.Name);
        Assert.Equal(QuestStatus.Active, created.Status);
        Assert.Equal(QuestType.MainQuest, created.QuestType);
        Assert.Equal(3, created.RecommendedLevelMin);
        Assert.Equal(5, created.RecommendedLevelMax);
        Assert.Empty(created.Objectives);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);
    }

    [Fact]
    public async Task CreateAsync_SucceedsWithOnlyRequiredFields()
    {
        var (questService, campaignService, _) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });

        var created = await questService.CreateAsync(campaign.Id, new QuestRequestDto
        {
            Name = "Unnamed Quest",
            Status = QuestStatus.Planned,
            QuestType = QuestType.Other,
        });

        Assert.NotNull(created);
        Assert.Null(created!.Description);
        Assert.Null(created.RecommendedLevelMin);
        Assert.Null(created.RecommendedLevelMax);
        Assert.Null(created.DmNotes);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenCampaignNotOwnedByCurrentUser()
    {
        var (questService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });

        user.UserId = Guid.NewGuid();
        var result = await questService.CreateAsync(campaign.Id, new QuestRequestDto
        {
            Name = "Missing Caravan",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenCampaignDoesNotExist()
    {
        var (questService, _, _) = CreateServices();

        var result = await questService.CreateAsync(Guid.NewGuid(), new QuestRequestDto
        {
            Name = "Missing Caravan",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenParentCampaignOwnedByDifferentUser()
    {
        var (questService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var quest = await questService.CreateAsync(campaign.Id, new QuestRequestDto
        {
            Name = "Missing Caravan",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });

        user.UserId = Guid.NewGuid();
        var result = await questService.GetByIdAsync(quest!.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllForCampaignAsync_ReturnsNull_WhenCampaignNotOwned()
    {
        var (questService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });

        user.UserId = Guid.NewGuid();
        var result = await questService.GetAllForCampaignAsync(campaign.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllForCampaignAsync_ReturnsOnlyQuestsForThatCampaign()
    {
        var (questService, campaignService, _) = CreateServices();
        var campaignA = await campaignService.CreateAsync(new CampaignRequestDto { Name = "Campaign A" });
        var campaignB = await campaignService.CreateAsync(new CampaignRequestDto { Name = "Campaign B" });
        await questService.CreateAsync(campaignA.Id, new QuestRequestDto { Name = "Missing Caravan", Status = QuestStatus.Planned, QuestType = QuestType.MainQuest });
        await questService.CreateAsync(campaignB.Id, new QuestRequestDto { Name = "The Black Coin", Status = QuestStatus.Planned, QuestType = QuestType.SideQuest });

        var result = await questService.GetAllForCampaignAsync(campaignA.Id);

        var single = Assert.Single(result!);
        Assert.Equal("Missing Caravan", single.Name);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFieldsAndTimestamp()
    {
        var (questService, campaignService, _) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var quest = await questService.CreateAsync(campaign.Id, new QuestRequestDto
        {
            Name = "Missing Caravan",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });

        var updated = await questService.UpdateAsync(quest!.Id, new QuestRequestDto
        {
            Name = "Missing Caravan (Renamed)",
            Status = QuestStatus.Completed,
            QuestType = QuestType.MainQuest,
        });

        Assert.NotNull(updated);
        Assert.Equal("Missing Caravan (Renamed)", updated!.Name);
        Assert.Equal(QuestStatus.Completed, updated.Status);
        Assert.True(updated.UpdatedAt >= quest.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenParentCampaignOwnedByDifferentUser()
    {
        var (questService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var quest = await questService.CreateAsync(campaign.Id, new QuestRequestDto
        {
            Name = "Missing Caravan",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });

        user.UserId = Guid.NewGuid();
        var result = await questService.UpdateAsync(quest!.Id, new QuestRequestDto
        {
            Name = "Renamed",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesOwnedQuest()
    {
        var (questService, campaignService, _) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var quest = await questService.CreateAsync(campaign.Id, new QuestRequestDto
        {
            Name = "Missing Caravan",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });

        var deleted = await questService.DeleteAsync(quest!.Id);
        var fetched = await questService.GetByIdAsync(quest.Id);

        Assert.True(deleted);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenParentCampaignOwnedByDifferentUser()
    {
        var (questService, campaignService, user) = CreateServices();
        var campaign = await campaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var quest = await questService.CreateAsync(campaign.Id, new QuestRequestDto
        {
            Name = "Missing Caravan",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });

        user.UserId = Guid.NewGuid();
        var deleted = await questService.DeleteAsync(quest!.Id);

        Assert.False(deleted);
    }
}
