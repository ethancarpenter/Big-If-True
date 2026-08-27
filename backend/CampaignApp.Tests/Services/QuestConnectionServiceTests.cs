using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Tests.Services;

public class QuestConnectionServiceTests
{
    private sealed record Services(
        QuestConnectionService QuestConnectionService,
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

        var questConnectionRepository = new QuestConnectionRepository(context);
        var questConnectionService = new QuestConnectionService(
            questConnectionRepository, questRepository, campaignRepository, currentUser);

        return new Services(questConnectionService, questService, campaignService, currentUser);
    }

    private static async Task<Guid> CreateQuestAsync(Services s, Guid campaignId, string name) =>
        (await s.QuestService.CreateAsync(campaignId, new QuestRequestDto
        {
            Name = name,
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
        }))!.Id;

    private static async Task<(Guid CampaignId, Guid A, Guid B, Guid C)> SeedCampaignWithThreeQuestsAsync(Services s)
    {
        var campaign = await s.CampaignService.CreateAsync(new CampaignRequestDto { Name = "The Northern Reach" });
        var a = await CreateQuestAsync(s, campaign.Id, "Quest A");
        var b = await CreateQuestAsync(s, campaign.Id, "Quest B");
        var c = await CreateQuestAsync(s, campaign.Id, "Quest C");
        return (campaign.Id, a, b, c);
    }

    private static QuestConnectionCreateRequestDto Request(Guid source, Guid target, QuestConnectionType type) =>
        new() { SourceQuestId = source, TargetQuestId = target, ConnectionType = type };

    [Fact]
    public async Task CreateAsync_LinksTwoQuests_WhenBothOwnedInSameCampaign()
    {
        var s = CreateServices();
        var (campaignId, a, b, _) = await SeedCampaignWithThreeQuestsAsync(s);

        var result = await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Unlocks));

        Assert.Equal(CreateQuestConnectionOutcome.Success, result.Outcome);
        Assert.NotNull(result.Connection);
        Assert.Equal(a, result.Connection!.SourceQuestId);
        Assert.Equal("Quest A", result.Connection.SourceQuestName);
        Assert.Equal(b, result.Connection.TargetQuestId);
        Assert.Equal("Quest B", result.Connection.TargetQuestName);
        Assert.Equal(QuestConnectionType.Unlocks, result.Connection.ConnectionType);
    }

    [Theory]
    [InlineData(QuestConnectionType.Unlocks)]
    [InlineData(QuestConnectionType.Requires)]
    [InlineData(QuestConnectionType.Optional)]
    [InlineData(QuestConnectionType.AlternativePath)]
    [InlineData(QuestConnectionType.FailureLeadsTo)]
    [InlineData(QuestConnectionType.Related)]
    public async Task CreateAsync_SucceedsForEveryConnectionType(QuestConnectionType type)
    {
        var s = CreateServices();
        var (campaignId, a, b, _) = await SeedCampaignWithThreeQuestsAsync(s);

        var result = await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, type));

        Assert.Equal(CreateQuestConnectionOutcome.Success, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNotFound_WhenCampaignNotOwnedByCurrentUser()
    {
        var s = CreateServices();
        var (campaignId, a, b, _) = await SeedCampaignWithThreeQuestsAsync(s);

        s.User.UserId = Guid.NewGuid();
        var result = await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Unlocks));

        Assert.Equal(CreateQuestConnectionOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNotFound_WhenTargetQuestBelongsToDifferentCampaign()
    {
        var s = CreateServices();
        var (campaignId, a, _, _) = await SeedCampaignWithThreeQuestsAsync(s);
        var (_, otherA, _, _) = await SeedCampaignWithThreeQuestsAsync(s); // a second, separate campaign

        var result = await s.QuestConnectionService.CreateAsync(campaignId, Request(a, otherA, QuestConnectionType.Unlocks));

        Assert.Equal(CreateQuestConnectionOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_ReturnsDuplicate_ForSameOrderedPair()
    {
        var s = CreateServices();
        var (campaignId, a, b, _) = await SeedCampaignWithThreeQuestsAsync(s);
        await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Unlocks));

        var result = await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Related));

        Assert.Equal(CreateQuestConnectionOutcome.Duplicate, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_AllowsReversePair_WithDifferentType_ForNonRelatedTypes()
    {
        var s = CreateServices();
        var (campaignId, a, b, _) = await SeedCampaignWithThreeQuestsAsync(s);
        var first = await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Unlocks));

        var second = await s.QuestConnectionService.CreateAsync(campaignId, Request(b, a, QuestConnectionType.Requires));

        Assert.Equal(CreateQuestConnectionOutcome.Success, first.Outcome);
        Assert.Equal(CreateQuestConnectionOutcome.Success, second.Outcome);
    }

    [Fact]
    public async Task CreateAsync_RejectsReverseRelated_WhenRelatedAlreadyExists()
    {
        var s = CreateServices();
        var (campaignId, a, b, _) = await SeedCampaignWithThreeQuestsAsync(s);
        await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Related));

        var result = await s.QuestConnectionService.CreateAsync(campaignId, Request(b, a, QuestConnectionType.Related));

        Assert.Equal(CreateQuestConnectionOutcome.Duplicate, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_AllowsReverseNonRelatedType_AfterRelatedExists()
    {
        var s = CreateServices();
        var (campaignId, a, b, _) = await SeedCampaignWithThreeQuestsAsync(s);
        await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Related));

        var result = await s.QuestConnectionService.CreateAsync(campaignId, Request(b, a, QuestConnectionType.Unlocks));

        Assert.Equal(CreateQuestConnectionOutcome.Success, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_RejectsCycle_ThroughPlainUnlocksChain()
    {
        var s = CreateServices();
        var (campaignId, a, b, c) = await SeedCampaignWithThreeQuestsAsync(s);
        await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Unlocks));
        await s.QuestConnectionService.CreateAsync(campaignId, Request(b, c, QuestConnectionType.Unlocks));

        var result = await s.QuestConnectionService.CreateAsync(campaignId, Request(c, a, QuestConnectionType.Unlocks));

        Assert.Equal(CreateQuestConnectionOutcome.Cycle, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_RejectsCycle_ThroughRequiresDirectionNormalization()
    {
        var s = CreateServices();
        var (campaignId, a, b, c) = await SeedCampaignWithThreeQuestsAsync(s);
        // Requires normalizes Target -> Source, so these three raw rows form the
        // same closed progression loop as a plain Unlocks 3-cycle would.
        await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Requires));
        await s.QuestConnectionService.CreateAsync(campaignId, Request(b, c, QuestConnectionType.Requires));

        var result = await s.QuestConnectionService.CreateAsync(campaignId, Request(c, a, QuestConnectionType.Requires));

        Assert.Equal(CreateQuestConnectionOutcome.Cycle, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_RejectsCycle_ThroughMixedUnlocksAndRequires()
    {
        var s = CreateServices();
        var (campaignId, a, b, c) = await SeedCampaignWithThreeQuestsAsync(s);
        // A Unlocks B: progression A->B.
        await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Unlocks));
        // C Requires B: progression B->C (Target -> Source).
        await s.QuestConnectionService.CreateAsync(campaignId, Request(c, b, QuestConnectionType.Requires));

        // C Unlocks A would close A->B->C->A.
        var result = await s.QuestConnectionService.CreateAsync(campaignId, Request(c, a, QuestConnectionType.Unlocks));

        Assert.Equal(CreateQuestConnectionOutcome.Cycle, result.Outcome);
    }

    [Fact]
    public async Task CreateAsync_AllowsCycle_ForNonProgressionTypes()
    {
        var s = CreateServices();
        var (campaignId, a, b, c) = await SeedCampaignWithThreeQuestsAsync(s);
        var first = await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Optional));
        var second = await s.QuestConnectionService.CreateAsync(campaignId, Request(b, c, QuestConnectionType.Optional));
        var third = await s.QuestConnectionService.CreateAsync(campaignId, Request(c, a, QuestConnectionType.Optional));

        Assert.Equal(CreateQuestConnectionOutcome.Success, first.Outcome);
        Assert.Equal(CreateQuestConnectionOutcome.Success, second.Outcome);
        Assert.Equal(CreateQuestConnectionOutcome.Success, third.Outcome);
    }

    [Fact]
    public async Task UpdateAsync_ChangesConnectionType()
    {
        var s = CreateServices();
        var (campaignId, a, b, _) = await SeedCampaignWithThreeQuestsAsync(s);
        var created = await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Related));

        var updated = await s.QuestConnectionService.UpdateAsync(
            created.Connection!.Id, new QuestConnectionUpdateRequestDto { ConnectionType = QuestConnectionType.Unlocks });

        Assert.Equal(UpdateQuestConnectionOutcome.Success, updated.Outcome);
        Assert.Equal(QuestConnectionType.Unlocks, updated.Connection!.ConnectionType);
    }

    [Fact]
    public async Task UpdateAsync_ToSameType_NeverSpuriouslyRejectsAsCycle()
    {
        var s = CreateServices();
        var (campaignId, a, b, _) = await SeedCampaignWithThreeQuestsAsync(s);
        var created = await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Unlocks));

        var updated = await s.QuestConnectionService.UpdateAsync(
            created.Connection!.Id, new QuestConnectionUpdateRequestDto { ConnectionType = QuestConnectionType.Unlocks });

        Assert.Equal(UpdateQuestConnectionOutcome.Success, updated.Outcome);
    }

    [Fact]
    public async Task UpdateAsync_RejectsCycle_WhenRetypingWouldCloseOne()
    {
        var s = CreateServices();
        var (campaignId, a, b, c) = await SeedCampaignWithThreeQuestsAsync(s);
        await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Unlocks));
        await s.QuestConnectionService.CreateAsync(campaignId, Request(b, c, QuestConnectionType.Unlocks));
        // Related, C -> A: not a progression edge yet, so no cycle so far.
        var created = await s.QuestConnectionService.CreateAsync(campaignId, Request(c, a, QuestConnectionType.Related));
        Assert.Equal(CreateQuestConnectionOutcome.Success, created.Outcome);

        // Retyping C->A to Unlocks would close A->B->C->A.
        var updated = await s.QuestConnectionService.UpdateAsync(
            created.Connection!.Id, new QuestConnectionUpdateRequestDto { ConnectionType = QuestConnectionType.Unlocks });

        Assert.Equal(UpdateQuestConnectionOutcome.Cycle, updated.Outcome);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFound_WhenCampaignOwnedByDifferentUser()
    {
        var s = CreateServices();
        var (campaignId, a, b, _) = await SeedCampaignWithThreeQuestsAsync(s);
        var created = await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Related));

        s.User.UserId = Guid.NewGuid();
        var result = await s.QuestConnectionService.UpdateAsync(
            created.Connection!.Id, new QuestConnectionUpdateRequestDto { ConnectionType = QuestConnectionType.Unlocks });

        Assert.Equal(UpdateQuestConnectionOutcome.NotFound, result.Outcome);
    }

    [Fact]
    public async Task DeleteAsync_RemovesOwnedConnection()
    {
        var s = CreateServices();
        var (campaignId, a, b, _) = await SeedCampaignWithThreeQuestsAsync(s);
        var created = await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Related));

        var deleted = await s.QuestConnectionService.DeleteAsync(created.Connection!.Id);
        var fetched = await s.QuestConnectionService.GetByIdAsync(created.Connection.Id);

        Assert.True(deleted);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenCampaignOwnedByDifferentUser()
    {
        var s = CreateServices();
        var (campaignId, a, b, _) = await SeedCampaignWithThreeQuestsAsync(s);
        var created = await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Related));

        s.User.UserId = Guid.NewGuid();
        var deleted = await s.QuestConnectionService.DeleteAsync(created.Connection!.Id);

        Assert.False(deleted);
    }

    [Fact]
    public async Task DeletingSourceQuest_CascadesItsOutgoingConnections()
    {
        var s = CreateServices();
        var (campaignId, a, b, _) = await SeedCampaignWithThreeQuestsAsync(s);
        var created = await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Unlocks));

        await s.QuestService.DeleteAsync(a);

        var fetched = await s.QuestConnectionService.GetByIdAsync(created.Connection!.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task DeletingTargetQuest_CascadesItsIncomingConnections()
    {
        var s = CreateServices();
        var (campaignId, a, b, _) = await SeedCampaignWithThreeQuestsAsync(s);
        var created = await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Unlocks));

        await s.QuestService.DeleteAsync(b);

        var fetched = await s.QuestConnectionService.GetByIdAsync(created.Connection!.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetAllForCampaignAsync_ReturnsAllConnections()
    {
        var s = CreateServices();
        var (campaignId, a, b, c) = await SeedCampaignWithThreeQuestsAsync(s);
        await s.QuestConnectionService.CreateAsync(campaignId, Request(a, b, QuestConnectionType.Unlocks));
        await s.QuestConnectionService.CreateAsync(campaignId, Request(b, c, QuestConnectionType.Related));

        var all = await s.QuestConnectionService.GetAllForCampaignAsync(campaignId);

        Assert.Equal(2, all!.Count);
    }

    [Fact]
    public async Task GetAllForCampaignAsync_ReturnsNull_WhenCampaignNotOwned()
    {
        var s = CreateServices();
        var (campaignId, _, _, _) = await SeedCampaignWithThreeQuestsAsync(s);

        s.User.UserId = Guid.NewGuid();
        var result = await s.QuestConnectionService.GetAllForCampaignAsync(campaignId);

        Assert.Null(result);
    }
}
