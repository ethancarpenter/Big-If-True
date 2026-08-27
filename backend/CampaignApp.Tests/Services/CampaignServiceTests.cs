using CampaignApp.Application.DTOs;
using CampaignApp.Application.Interfaces;
using CampaignApp.Application.Services;
using CampaignApp.Infrastructure.Persistence;
using CampaignApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Tests.Services;

public class FakeCurrentUserProvider : ICurrentUserProvider
{
    public Guid UserId { get; set; } = Guid.NewGuid();

    public Guid GetCurrentUserId() => UserId;
}

public class CampaignServiceTests
{
    private static (CampaignService Service, FakeCurrentUserProvider User) CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);
        var repository = new CampaignRepository(context);
        var currentUser = new FakeCurrentUserProvider();
        var service = new CampaignService(repository, currentUser);
        return (service, currentUser);
    }

    [Fact]
    public async Task CreateAsync_SetsTimestampsAndPersists()
    {
        var (service, _) = CreateService();
        var request = new CampaignRequestDto { Name = "The Northern Reach", Description = "A frontier campaign." };

        var created = await service.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("The Northern Reach", created.Name);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);
        Assert.NotEqual(default, created.CreatedAt);

        var fetched = await service.GetByIdAsync(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal(created.Name, fetched!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_ForCampaignOwnedByDifferentUser()
    {
        var (service, user) = CreateService();
        var created = await service.CreateAsync(new CampaignRequestDto { Name = "Shadows of Black Coast" });

        user.UserId = Guid.NewGuid();
        var result = await service.GetByIdAsync(created.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_ForCampaignOwnedByDifferentUser()
    {
        var (service, user) = CreateService();
        var created = await service.CreateAsync(new CampaignRequestDto { Name = "The Lost Mines" });

        user.UserId = Guid.NewGuid();
        var result = await service.UpdateAsync(created.Id, new CampaignRequestDto { Name = "Renamed" });

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_ForCampaignOwnedByDifferentUser()
    {
        var (service, user) = CreateService();
        var created = await service.CreateAsync(new CampaignRequestDto { Name = "The Lost Mines" });

        user.UserId = Guid.NewGuid();
        var deleted = await service.DeleteAsync(created.Id);

        Assert.False(deleted);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFieldsAndTimestamp()
    {
        var (service, _) = CreateService();
        var created = await service.CreateAsync(new CampaignRequestDto { Name = "Original Name" });

        var updated = await service.UpdateAsync(created.Id, new CampaignRequestDto
        {
            Name = "Updated Name",
            Description = "New description",
        });

        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated!.Name);
        Assert.Equal("New description", updated.Description);
        Assert.True(updated.UpdatedAt >= created.UpdatedAt);
    }

    [Fact]
    public async Task GetAllAsync_OnlyReturnsCurrentUsersCampaigns()
    {
        var (service, user) = CreateService();
        await service.CreateAsync(new CampaignRequestDto { Name = "Mine" });

        user.UserId = Guid.NewGuid();
        await service.CreateAsync(new CampaignRequestDto { Name = "Someone Else's" });

        var results = await service.GetAllAsync();

        var single = Assert.Single(results);
        Assert.Equal("Someone Else's", single.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesOwnedCampaign()
    {
        var (service, _) = CreateService();
        var created = await service.CreateAsync(new CampaignRequestDto { Name = "To Be Deleted" });

        var deleted = await service.DeleteAsync(created.Id);
        var fetched = await service.GetByIdAsync(created.Id);

        Assert.True(deleted);
        Assert.Null(fetched);
    }
}
