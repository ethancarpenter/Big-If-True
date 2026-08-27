using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Tests.Services;

public class QuestNpcServiceTests
{
    private sealed record Services(
        QuestNpcService QuestNpcService,
        QuestService QuestService,
        NpcService NpcService,
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

        var npcRepository = new NpcRepository(context);
        var npcService = new NpcService(npcRepository, campaignRepository, currentUser);

        var questNpcRepository = new QuestNpcRepository(context);
        var questNpcService = new QuestNpcService(questNpcRepository, questRepository, npcRepository, currentUser);

        return new Services(questNpcService, questService, npcService, campaignService, currentUser);
    }

    private static async Task<(Guid CampaignId, Guid QuestId, Guid NpcId)> SeedCampaignWithQuestAndNpcAsync(Services s)
    {
        var campaign = await s.CampaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var quest = await s.QuestService.CreateAsync(campaign.Id, new QuestRequestDto
        {
            Name = "Missing Caravan",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        });
        var npc = await s.NpcService.CreateAsync(campaign.Id, new NpcRequestDto { Name = "Eldrin Vale", Status = NpcStatus.Alive });
        return (campaign.Id, quest!.Id, npc!.Id);
    }

    [Fact]
    public async Task CreateAsync_LinksQuestAndNpc_WhenBothOwnedInSameCampaign()
    {
        var s = CreateServices();
        var (_, questId, npcId) = await SeedCampaignWithQuestAndNpcAsync(s);

        var result = await s.QuestNpcService.CreateAsync(questId, new QuestNpcCreateRequestDto
        {
            NpcId = npcId,
            Role = QuestNpcRole.QuestGiver,
            Notes = "Gives the quest at the inn.",
        });

        Assert.Equal(CreateQuestNpcOutcome.Success, result.Outcome);
        Assert.NotNull(result.Relationship);
        Assert.Equal(questId, result.Relationship!.QuestId);
        Assert.Equal("Missing Caravan", result.Relationship.QuestName);
        Assert.Equal(npcId, result.Relationship.NpcId);
        Assert.Equal("Eldrin Vale", result.Relationship.NpcName);
        Assert.Equal(QuestNpcRole.QuestGiver, result.Relationship.Role);
        Assert.Equal("Gives the quest at the inn.", result.Relationship.Notes);
    }

    [Fact]
    public async Task CreateAsync_SucceedsWithoutNotes()
    {
        var s = CreateServices();
        var (_, questId, npcId) = await SeedCampaignWithQuestAndNpcAsync(s);

        var result = await s.QuestNpcService.CreateAsync(questId, new QuestNpcCreateRequestDto
        {
            NpcId = npcId,
            Role = QuestNpcRole.Witness,
        });

        Assert.Equal(CreateQuestNpcOutcome.Success, result.Outcome);
        Assert.Null(result.Relationship!.Notes);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNotFound_WhenQuestNotOwnedByCurrentUser()
    {
        var s = CreateServices();
        var (_, questId, npcId) = await SeedCampaignWithQuestAndNpcAsync(s);

        s.User.UserId = Guid.NewGuid();
        var result = await s.QuestNpcService.CreateAsync(questId, new QuestNpcCreateRequestDto
        {
            NpcId = npcId,
            Role = QuestNpcRole.QuestGiver,
        });

        Assert.Equal(CreateQuestNpcOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNotFound_WhenNpcBelongsToDifferentCampaign()
    {
        var s = CreateServices();
        var (_, questId, _) = await SeedCampaignWithQuestAndNpcAsync(s);
        var (_, _, otherNpcId) = await SeedCampaignWithQuestAndNpcAsync(s); // a second, separate campaign

        var result = await s.QuestNpcService.CreateAsync(questId, new QuestNpcCreateRequestDto
        {
            NpcId = otherNpcId,
            Role = QuestNpcRole.QuestGiver,
        });

        Assert.Equal(CreateQuestNpcOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDuplicate_WhenRelationshipAlreadyExists()
    {
        var s = CreateServices();
        var (_, questId, npcId) = await SeedCampaignWithQuestAndNpcAsync(s);
        await s.QuestNpcService.CreateAsync(questId, new QuestNpcCreateRequestDto { NpcId = npcId, Role = QuestNpcRole.QuestGiver });

        var result = await s.QuestNpcService.CreateAsync(questId, new QuestNpcCreateRequestDto
        {
            NpcId = npcId,
            Role = QuestNpcRole.Enemy, // different role, same pair - still a duplicate
        });

        Assert.Equal(CreateQuestNpcOutcome.Duplicate, result.Outcome);

        var relationships = await s.QuestNpcService.GetAllForQuestAsync(questId);
        var single = Assert.Single(relationships!);
        Assert.Equal(QuestNpcRole.QuestGiver, single.Role); // first row untouched
    }

    [Fact]
    public async Task UpdateAsync_UpdatesRoleAndNotes()
    {
        var s = CreateServices();
        var (_, questId, npcId) = await SeedCampaignWithQuestAndNpcAsync(s);
        var created = await s.QuestNpcService.CreateAsync(questId, new QuestNpcCreateRequestDto { NpcId = npcId, Role = QuestNpcRole.Ally });

        var updated = await s.QuestNpcService.UpdateAsync(created.Relationship!.Id, new QuestNpcUpdateRequestDto
        {
            Role = QuestNpcRole.Enemy,
            Notes = "Turned after the betrayal.",
        });

        Assert.NotNull(updated);
        Assert.Equal(QuestNpcRole.Enemy, updated!.Role);
        Assert.Equal("Turned after the betrayal.", updated.Notes);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenParentCampaignOwnedByDifferentUser()
    {
        var s = CreateServices();
        var (_, questId, npcId) = await SeedCampaignWithQuestAndNpcAsync(s);
        var created = await s.QuestNpcService.CreateAsync(questId, new QuestNpcCreateRequestDto { NpcId = npcId, Role = QuestNpcRole.Ally });

        s.User.UserId = Guid.NewGuid();
        var result = await s.QuestNpcService.UpdateAsync(created.Relationship!.Id, new QuestNpcUpdateRequestDto { Role = QuestNpcRole.Enemy });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesOwnedRelationship()
    {
        var s = CreateServices();
        var (_, questId, npcId) = await SeedCampaignWithQuestAndNpcAsync(s);
        var created = await s.QuestNpcService.CreateAsync(questId, new QuestNpcCreateRequestDto { NpcId = npcId, Role = QuestNpcRole.Ally });

        var deleted = await s.QuestNpcService.DeleteAsync(created.Relationship!.Id);
        var fetched = await s.QuestNpcService.GetByIdAsync(created.Relationship.Id);

        Assert.True(deleted);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenParentCampaignOwnedByDifferentUser()
    {
        var s = CreateServices();
        var (_, questId, npcId) = await SeedCampaignWithQuestAndNpcAsync(s);
        var created = await s.QuestNpcService.CreateAsync(questId, new QuestNpcCreateRequestDto { NpcId = npcId, Role = QuestNpcRole.Ally });

        s.User.UserId = Guid.NewGuid();
        var deleted = await s.QuestNpcService.DeleteAsync(created.Relationship!.Id);

        Assert.False(deleted);
    }

    [Fact]
    public async Task DeletingQuest_CascadesItsRelationships()
    {
        var s = CreateServices();
        var (_, questId, npcId) = await SeedCampaignWithQuestAndNpcAsync(s);
        var created = await s.QuestNpcService.CreateAsync(questId, new QuestNpcCreateRequestDto { NpcId = npcId, Role = QuestNpcRole.Ally });

        await s.QuestService.DeleteAsync(questId);

        var fetched = await s.QuestNpcService.GetByIdAsync(created.Relationship!.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeletingNpc_CascadesItsRelationships()
    {
        var s = CreateServices();
        var (_, questId, npcId) = await SeedCampaignWithQuestAndNpcAsync(s);
        var created = await s.QuestNpcService.CreateAsync(questId, new QuestNpcCreateRequestDto { NpcId = npcId, Role = QuestNpcRole.Ally });

        await s.NpcService.DeleteAsync(npcId);

        var fetched = await s.QuestNpcService.GetByIdAsync(created.Relationship!.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetAllForNpcAsync_ReturnsSameRelationship_AsGetAllForQuestAsync()
    {
        var s = CreateServices();
        var (_, questId, npcId) = await SeedCampaignWithQuestAndNpcAsync(s);
        await s.QuestNpcService.CreateAsync(questId, new QuestNpcCreateRequestDto { NpcId = npcId, Role = QuestNpcRole.Contact });

        var fromQuestSide = await s.QuestNpcService.GetAllForQuestAsync(questId);
        var fromNpcSide = await s.QuestNpcService.GetAllForNpcAsync(npcId);

        var questSideRow = Assert.Single(fromQuestSide!);
        var npcSideRow = Assert.Single(fromNpcSide!);
        Assert.Equal(questSideRow.Id, npcSideRow.Id);
        Assert.Equal("Missing Caravan", npcSideRow.QuestName); // visible from the Npc side
        Assert.Equal("Eldrin Vale", questSideRow.NpcName); // visible from the Quest side
    }

    [Fact]
    public async Task GetAllForNpcAsync_ReturnsNull_WhenNpcNotOwned()
    {
        var s = CreateServices();
        var (_, _, npcId) = await SeedCampaignWithQuestAndNpcAsync(s);

        s.User.UserId = Guid.NewGuid();
        var result = await s.QuestNpcService.GetAllForNpcAsync(npcId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllForQuestAsync_ReturnsNull_WhenQuestNotOwned()
    {
        var s = CreateServices();
        var (_, questId, _) = await SeedCampaignWithQuestAndNpcAsync(s);

        s.User.UserId = Guid.NewGuid();
        var result = await s.QuestNpcService.GetAllForQuestAsync(questId);

        Assert.Null(result);
    }
}
